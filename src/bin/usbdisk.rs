#![no_std]
#![no_main]

use embassy_executor::Spawner;
use embassy_usb::UsbDevice;
use embassy_usb::driver::EndpointError;
// use ft8132_probe::usb_mas::{MasClass, State}; // Replaced by local implementation
use esp_hal::timer::timg::TimerGroup;
use static_cell::StaticCell;
use log::{info, error};
use esp_hal::otg_fs::{
    Usb,
    asynch::{Driver, Config},
};
use esp_backtrace as _;

use esp_bootloader_esp_idf::esp_app_desc;
use embedded_storage::{ReadStorage, Storage};
use esp_storage::FlashStorage;

// Imports for MasClass
use core::mem::MaybeUninit;
use embassy_usb::control::{self, InResponse, OutResponse, Recipient, Request, RequestType};
use embassy_usb::driver::{Driver as UsbDriverTrait, Endpoint, EndpointIn, EndpointOut};
use embassy_usb::types::InterfaceNumber;
use embassy_usb::{Builder, Handler};

esp_app_desc!();

const BLOCK_SIZE: usize = 512;
const DISK_SIZE: usize = 1024 * 1024; // 1MB
const BLOCK_COUNT: u32 = (DISK_SIZE / BLOCK_SIZE) as u32;

// CBW Signature: 'USBC'
const CBW_SIGNATURE: u32 = 0x43425355;
// CSW Signature: 'USBS'
const CSW_SIGNATURE: u32 = 0x53425355;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum Direction {
    HostToDevice = 0x00,
    DeviceToHost = 0x80,
}

#[derive(Debug)]
struct Cbw {
    tag: u32,
    data_transfer_length: u32,
    flags: u8,
    lun: u8,
    cb_length: u8,
    cb: [u8; 16],
}

impl Cbw {
    fn parse(buf: &[u8]) -> Option<Self> {
        if buf.len() != 31 {
            return None;
        }

        let signature = u32::from_le_bytes(buf[0..4].try_into().unwrap());
        if signature != CBW_SIGNATURE {
            return None;
        }

        Some(Self {
            tag: u32::from_le_bytes(buf[4..8].try_into().unwrap()),
            data_transfer_length: u32::from_le_bytes(buf[8..12].try_into().unwrap()),
            flags: buf[12],
            lun: buf[13] & 0x0F,
            cb_length: buf[14] & 0x1F,
            cb: buf[15..31].try_into().unwrap(),
        })
    }
}

struct Csw {
    tag: u32,
    data_residue: u32,
    status: u8,
}

impl Csw {
    fn to_bytes(&self, buf: &mut [u8]) {
        buf[0..4].copy_from_slice(&CSW_SIGNATURE.to_le_bytes());
        buf[4..8].copy_from_slice(&self.tag.to_le_bytes());
        buf[8..12].copy_from_slice(&self.data_residue.to_le_bytes());
        buf[12] = self.status;
    }
}

// SCSI Commands
const SCSI_CMD_TEST_UNIT_READY: u8 = 0x00;
const SCSI_CMD_REQUEST_SENSE: u8 = 0x03;
const SCSI_CMD_INQUIRY: u8 = 0x12;
const SCSI_CMD_MODE_SENSE_6: u8 = 0x1A;
const SCSI_CMD_PREVENT_ALLOW_MEDIUM_REMOVAL: u8 = 0x1E;
const SCSI_CMD_READ_CAPACITY_10: u8 = 0x25;
const SCSI_CMD_READ_10: u8 = 0x28;
const SCSI_CMD_WRITE_10: u8 = 0x2A;

const SENSE_KEY_NO_SENSE: u8 = 0x00;
const SENSE_KEY_RECOVERED_ERROR: u8 = 0x01;
const SENSE_KEY_NOT_READY: u8 = 0x02;
const SENSE_KEY_MEDIUM_ERROR: u8 = 0x03;
const SENSE_KEY_HARDWARE_ERROR: u8 = 0x04;
const SENSE_KEY_ILLEGAL_REQUEST: u8 = 0x05;
const SENSE_KEY_UNIT_ATTENTION: u8 = 0x06;
const SENSE_KEY_DATA_PROTECT: u8 = 0x07;
const SENSE_KEY_BLANK_CHECK: u8 = 0x08;
const SENSE_KEY_ABORTED_COMMAND: u8 = 0x0B;

#[repr(C, align(4))]
struct AlignedBuffer {
    buffer: [u8; BLOCK_SIZE],
}

// =========================================================================
// Mass Storage Class Implementation (Inlined)
// =========================================================================

/// This should be used as `device_class` when building the `UsbDevice`.
pub const USB_CLASS_MSC: u8 = 0x08;
pub const MSC_SUBCLASS_SCSI: u8 = 0x06;
pub const MSC_PROTOCOL_BOT: u8 = 0x50;

const REQ_GET_MAX_LUN: u8 = 0xFE;
const REQ_RESET: u8 = 0xFF;

/// Internal state for MSC
pub struct State<'a> {
    control: MaybeUninit<Control<'a>>,
}

impl<'a> Default for State<'a> {
    fn default() -> Self {
        Self::new()
    }
}

impl<'a> State<'a> {
    /// Create a new `State`.
    pub const fn new() -> Self {
        Self {
            control: MaybeUninit::uninit(),
        }
    }
}

/// Packet level implementation of a MSC device.
///
/// This class can be used directly to implement the Bulk-Only Transport protocol.
pub struct MasClass<'d, D: UsbDriverTrait<'d>> {
    read_ep: D::EndpointOut,
    write_ep: D::EndpointIn,
}

struct Control<'a> {
    comm_if: InterfaceNumber,
    max_lun: u8,
    _marker: core::marker::PhantomData<&'a ()>,
}

impl<'d> Handler for Control<'d> {
    fn control_out(&mut self, req: control::Request, _data: &[u8]) -> Option<OutResponse> {
        if (req.request_type, req.recipient, req.index)
            != (RequestType::Class, Recipient::Interface, self.comm_if.0 as u16)
        {
            return None;
        }

        match req.request {
            REQ_RESET => {
                // Bulk-Only Mass Storage Reset
                // The device shall ready itself for the next CBW.
                // The host shall also clear the Bulk-In and Bulk-Out feature (halt) after this.
                Some(OutResponse::Accepted)
            }
            _ => Some(OutResponse::Rejected),
        }
    }

    fn control_in<'a>(&'a mut self, req: Request, buf: &'a mut [u8]) -> Option<InResponse<'a>> {
        if (req.request_type, req.recipient, req.index)
            != (RequestType::Class, Recipient::Interface, self.comm_if.0 as u16)
        {
            return None;
        }

        match req.request {
            REQ_GET_MAX_LUN => {
                if req.length != 1 {
                    return Some(InResponse::Rejected);
                }
                buf[0] = self.max_lun;
                Some(InResponse::Accepted(&buf[0..1]))
            }
            _ => Some(InResponse::Rejected),
        }
    }
}

impl<'d, D: UsbDriverTrait<'d>> MasClass<'d, D> {
    /// Creates a new MasClass with the provided UsbBus and `max_packet_size` in bytes.
    /// `max_lun` is the maximum Logical Unit Number (0-15).
    pub fn new(
        builder: &mut Builder<'d, D>,
        state: &'d mut State<'d>,
        max_packet_size: u16,
        max_lun: u8,
    ) -> Self {
        let mut func = builder.function(USB_CLASS_MSC, MSC_SUBCLASS_SCSI, MSC_PROTOCOL_BOT);

        // Interface
        let mut iface = func.interface();
        let comm_if = iface.interface_number();
        let mut alt = iface.alt_setting(USB_CLASS_MSC, MSC_SUBCLASS_SCSI, MSC_PROTOCOL_BOT, None);

        let read_ep = alt.endpoint_bulk_out(None, max_packet_size);
        let write_ep = alt.endpoint_bulk_in(None, max_packet_size);

        drop(func);

        let control = state.control.write(Control {
            comm_if,
            max_lun,
            _marker: core::marker::PhantomData,
        });
        builder.handler(control);

        MasClass { read_ep, write_ep }
    }

    /// Gets the maximum packet size in bytes.
    pub fn max_packet_size(&self) -> u16 {
        // The size is the same for both endpoints.
        self.read_ep.info().max_packet_size
    }

    /// Writes a single packet into the IN endpoint.
    pub async fn write_packet(&mut self, data: &[u8]) -> Result<(), EndpointError> {
        self.write_ep.write(data).await
    }

    /// Reads a single packet from the OUT endpoint.
    pub async fn read_packet(&mut self, data: &mut [u8]) -> Result<usize, EndpointError> {
        self.read_ep.read(data).await
    }

    /// Waits for the USB host to enable this interface
    pub async fn wait_connection(&mut self) {
        self.read_ep.wait_enabled().await;
    }
}

// =========================================================================

#[esp_rtos::main]
async fn main(spawner: Spawner) {
    esp_println::logger::init_logger_from_env();
    let peripherals = esp_hal::init(esp_hal::Config::default());

    esp_alloc::heap_allocator!(#[esp_hal::ram(reclaimed)] size: 73744);

    let timg0 = TimerGroup::new(peripherals.TIMG0);
    esp_rtos::start(timg0.timer0);

    info!("Embassy initialized!");

    let usb = Usb::new(peripherals.USB0, peripherals.GPIO20, peripherals.GPIO19);
    let driver_config = Config::default();
    static EP_MEMORY_CELL: StaticCell<[u8; 8192]> = StaticCell::new();
    let driver = Driver::new(usb, EP_MEMORY_CELL.init([0; 8192]), driver_config);

    // Create embassy-usb Config
    let config = {
        let mut config = embassy_usb::Config::new(0x1B3F, 0x8000);
        config.manufacturer = Some("General");
        config.product = Some("USB Mass Storage");
        config.serial_number = None;
        config.device_release = 0x0100;
        config.self_powered = true;
        config.max_power = 0;
        config.max_packet_size_0 = 64;
        config
    };
 
    // Create embassy-usb DeviceBuilder using the driver and config.
    let mut builder = {
        static CONFIG_DESCRIPTOR: StaticCell<[u8; 256]> = StaticCell::new();
        static BOS_DESCRIPTOR: StaticCell<[u8; 256]> = StaticCell::new();
        static CONTROL_BUF: StaticCell<[u8; 64]> = StaticCell::new();

        let builder = embassy_usb::Builder::new(
            driver,
            config,
            CONFIG_DESCRIPTOR.init([0; 256]),
            BOS_DESCRIPTOR.init([0; 256]),
            &mut [], // no msos descriptors
            CONTROL_BUF.init([0; 64]),
        );
        builder
    };

    // Create classes on the builder.
    let mut class = {
        static STATE: StaticCell<State> = StaticCell::new();
        let state = STATE.init(State::new());
        MasClass::new(&mut builder, state, 64, 0)
    };

    // Build the builder.
    let usb = builder.build();

    // Run the USB device.
    spawner.spawn(usb_task(usb)).unwrap();

    // Flash storage handling - Removed as per request (No Media)
    // let mut flash = FlashStorage::new(peripherals.FLASH);
    // ... logic removed ...
    info!("Storage media emulation disabled (No Media)");

    // Do stuff with the class!
    let mut buf = [0; 64];
    let mut current_sense = (SENSE_KEY_NO_SENSE, 0x00, 0x00); // Key, ASC, ASCQ

    loop {
        class.wait_connection().await;
        info!("Connected");
        
        // BOT Loop
        loop {
            // Read CBW
            match class.read_packet(&mut buf).await {
                Ok(n) => {
                    if n == 0 { continue; }
                    let cbw = match Cbw::parse(&buf[..n]) {
                        Some(cbw) => cbw,
                        None => {
                             error!("Invalid CBW");
                            continue;
                        }
                    };
                    
                    // Process SCSI Command
                    let mut residue = cbw.data_transfer_length;
                    let mut status = 0x00; // Passed

                    match cbw.cb[0] {
                        SCSI_CMD_TEST_UNIT_READY => {
                            status = 0x01; // Check Condition
                            current_sense = (SENSE_KEY_NOT_READY, 0x3A, 0x00); // Medium Not Present
                        }
                        SCSI_CMD_REQUEST_SENSE => {
                            let sense_data = [
                                0x70, // Fixed
                                0x00,
                                current_sense.0, // Sense Key
                                0x00,
                                0x00,
                                0x00,
                                0x00,
                                0x0A, // Additional Sense Length (10)
                                0x00,
                                0x00,
                                0x00,
                                0x00,
                                current_sense.1, // ASC
                                current_sense.2, // ASCQ
                                0x00, 0x00, 0x00, 0x00,
                            ];
                            // Clear sense after reading
                            current_sense = (SENSE_KEY_NO_SENSE, 0x00, 0x00);

                            let len = core::cmp::min(residue as usize, sense_data.len());
                            if let Err(_) = class.write_packet(&sense_data[..len]).await {
                                status = 0x01;
                            } else {
                                residue -= len as u32;
                            }
                        }
                        SCSI_CMD_PREVENT_ALLOW_MEDIUM_REMOVAL => {
                            // Just ACK
                        }
                        SCSI_CMD_INQUIRY => {
                            let inquiry_data = [
                                0x00, // Direct Access Block Device
                                0x80, // Removable
                                0x00, // ANSI version - 0x00 No Compliance to any Standard (0x00)
                                0x00, // Response data format - 0x00 Unknown
                                36 - 5, // Additional Length
                                0x00, 0x00, 0x00,
                                // Vendor "GENPLUS "
                                0x47, 0x45, 0x4e, 0x50, 0x4c, 0x55, 0x53, 0x20,
                                // Product "USB-MSDC DISK A "
                                0x55, 0x53, 0x42, 0x2d, 0x4d, 0x53, 0x44, 0x43,
                                0x20, 0x44, 0x49, 0x53, 0x4b, 0x20, 0x41, 0x20,
                                // Revision "1.00"
                                0x31, 0x2E, 0x30, 0x30,
                            ];
                            let len = core::cmp::min(residue as usize, inquiry_data.len());
                            if len > 0 {
                                if let Err(_) = class.write_packet(&inquiry_data[..len]).await {
                                    status = 0x01; // Failed
                                } else {
                                    residue -= len as u32;
                                }
                            }
                        }
                        SCSI_CMD_READ_CAPACITY_10 => {
                            status = 0x01;
                            current_sense = (SENSE_KEY_NOT_READY, 0x3A, 0x00);
                        }
                        SCSI_CMD_READ_10 => {
                            status = 0x01;
                            current_sense = (SENSE_KEY_NOT_READY, 0x3A, 0x00);
                        }
                        SCSI_CMD_WRITE_10 => {
                            status = 0x01;
                            current_sense = (SENSE_KEY_NOT_READY, 0x3A, 0x00);
                            // Need to drain the data from host
                            let mut drain_buf = [0u8; 64];
                            while residue > 0 {
                                let to_read = core::cmp::min(residue as usize, 64);
                                if let Ok(n) = class.read_packet(&mut drain_buf[..to_read]).await {
                                    residue -= n as u32;
                                } else {
                                    break;
                                }
                            }
                        }
                        SCSI_CMD_MODE_SENSE_6 => {
                            // Mode Parameter Header (4 bytes)
                            let header = [0x03, 0x00, 0x00, 0x00];
                             let len = core::cmp::min(residue as usize, header.len());
                            if let Err(_) = class.write_packet(&header[..len]).await {
                                status = 0x01; 
                            } else {
                                residue -= len as u32;
                            }
                        }
                        0x23 => { // READ_FORMAT_CAPACITIES
                            status = 0x01;
                            current_sense = (SENSE_KEY_NOT_READY, 0x3A, 0x00);
                        }
                        _ => {
                            // Unsupported
                            status = 0x01;
                            current_sense = (SENSE_KEY_ILLEGAL_REQUEST, 0x20, 0x00); // Invalid Command Operation Code
                            // Check direction
                            if cbw.flags & 0x80 != 0 {
                                // IN (Device to Host)
                                if residue > 0 {
                                    panic!("Invalid CBW: IN direction but data to send");
                                }
                            } else {
                                // OUT (Host to Device)
                                // Drain OUT data if any
                                if residue > 0 {
                                    let mut drain_buf = [0u8; 64];
                                    while residue > 0 {
                                         let to_read = core::cmp::min(residue as usize, 64);
                                         if let Ok(n) = class.read_packet(&mut drain_buf[..to_read]).await {
                                            residue -= n as u32;
                                        } else {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Send CSW
                    let csw = Csw {
                        tag: cbw.tag,
                        data_residue: residue,
                        status,
                    };
                    let mut csw_buf = [0u8; 13];
                    csw.to_bytes(&mut csw_buf);
                    if let Err(e) = class.write_packet(&csw_buf).await {
                        error!("Failed to write CSW: {:?}", e);
                        break;
                    }
                }
                Err(EndpointError::Disabled) => break,
                Err(_) => continue,
            }
        }
        info!("Disconnected");
    }
}

type MyDriver = Driver<'static>;
type MyUsbDevice = UsbDevice<'static, MyDriver>;

#[embassy_executor::task]
async fn usb_task(mut usb: MyUsbDevice) -> ! {
    usb.run().await
}
