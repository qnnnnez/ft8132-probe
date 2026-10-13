#![no_std]
#![no_main]

use embassy_executor::Spawner;
use embassy_sync::blocking_mutex::raw::CriticalSectionRawMutex;
use embassy_sync::signal::Signal;
use embassy_time::{Duration, Timer};
use esp_backtrace as _;
use esp_hal::{
    interrupt::{self, Priority},
    otg_fs::{Usb, UsbBus},
    peripherals::Interrupt,
    timer::timg::TimerGroup,
};
use ft8132_probe::asic::{self, AsicBus};
use log::{info, warn};
use static_cell::StaticCell;
use usb_device::bus::UsbBusAllocator;
use usb_device::prelude::{UsbDevice, UsbDeviceBuilder, UsbVidPid};
use usbd_storage::subclass::scsi::{Scsi, ScsiCommand};
use usbd_storage::subclass::Command;

#[cfg(feature = "mock-asic")]
use ft8132_probe::mock_asic::MockAsic;
#[cfg(not(feature = "mock-asic"))]
use ft8132_probe::ft8132::FT8132;

esp_bootloader_esp_idf::esp_app_desc!();

static USB_SIGNAL: Signal<CriticalSectionRawMutex, ()> = Signal::new();

type UsbBusType = UsbBus<Usb<'static>>;
type ScsiDev = Scsi<usbd_storage::transport::bbb::BulkOnly<'static, UsbBusType, IoBuf>>;

#[cfg(feature = "mock-asic")]
type AsicDev = MockAsic;
#[cfg(not(feature = "mock-asic"))]
type AsicDev = FT8132<'static, esp_hal::peripherals::I2C0<'static>>;

// Sense keys
const SENSE_KEY_NO_SENSE: u8 = 0x00;
const SENSE_KEY_NOT_READY: u8 = 0x02;
const SENSE_KEY_ILLEGAL_REQUEST: u8 = 0x05;

// ASC/ASCQ
const ASC_MEDIUM_NOT_PRESENT: u8 = 0x3A;
const ASCQ_MEDIUM_NOT_PRESENT: u8 = 0x00;
const ASC_INVALID_COMMAND_OP_CODE: u8 = 0x20;
const ASCQ_INVALID_COMMAND_OP_CODE: u8 = 0x00;

// SCSI INQUIRY identification strings (fixed width, ASCII)
const INQ_VENDOR: [u8; 8] = *b"GENPLUS ";
const INQ_PRODUCT: [u8; 16] = *b"USB-MSDC DISK A ";
const INQ_REV: [u8; 4] = *b"1.00";
const INQ_MAGIC: [u8; 8] = *b"GP-PROD.";

// SCSI VPD unit serial number (ASCII)
const UNIT_SERIAL: &[u8] = b"12345678";

struct IoBuf(&'static mut [u8]);

impl core::borrow::Borrow<[u8]> for IoBuf {
    fn borrow(&self) -> &[u8] {
        self.0
    }
}

impl core::borrow::BorrowMut<[u8]> for IoBuf {
    fn borrow_mut(&mut self) -> &mut [u8] {
        self.0
    }
}

#[esp_hal::handler(priority = Priority::Priority2)]
fn usb_interrupt_handler() {
    USB_SIGNAL.signal(());
    let _ = interrupt::disable(esp_hal::system::Cpu::ProCpu, Interrupt::USB);
    let _ = interrupt::disable(esp_hal::system::Cpu::AppCpu, Interrupt::USB);
}

fn cdb_reg_len(raw: &[u8; 16]) -> (u8, usize, u8, u8) {
    let acc = raw[6];
    let opt = raw[9];
    let addr = raw[5];
    let mut len = u16::from_be_bytes([raw[7], raw[8]]) as usize;
    if len == 0 {
        len = 1;
    }
    (addr, len.min(512), acc, opt)
}

fn fail_illegal(cmd: Command<'_, ScsiCommand, ScsiDev>, current_sense: &mut (u8, u8, u8)) {
    *current_sense = (
        SENSE_KEY_ILLEGAL_REQUEST,
        ASC_INVALID_COMMAND_OP_CODE,
        ASCQ_INVALID_COMMAND_OP_CODE,
    );
    cmd.fail();
}

fn handle_vendor<B: AsicBus>(
    asic: &mut B,
    mut cmd: Command<'_, ScsiCommand, ScsiDev>,
    current_sense: &mut (u8, u8, u8),
) {
    let raw = cmd.cdb;
    let op = raw[0];
    let sub = raw[1];
    let data_in = cmd.is_data_in();

    match (op, sub) {
        (0xF0, 0xF0) => {
            info!("auth command 0xF0/0xF0");
            let c = raw[2..=8]
                .iter()
                .map(|&b| b as u16)
                .sum::<u16>()
                ^ 0x4750;
            info!("auth code: {c:#06x}");
            let resp = c.to_be_bytes();
            let _ = cmd.write_data(&resp);
            cmd.pass();
        }
        (0xF0, 0xF8) => {
            warn!("stub vendor CDB F0 F8: {raw:02x?}");
            let _ = cmd.write_data(&[0; 4]);
            cmd.pass();
        }
        (0xF0, 0xF9) => {
            warn!("stub vendor CDB F0 F9: {raw:02x?}");
            let _ = cmd.write_data(&[0; 20]);
            cmd.pass();
        }
        (0xFD, 0x00) => {
            info!("pre-op / enter I2C (FD 00)");
            match asic.knock() {
                Ok(()) => cmd.pass(),
                Err(e) => {
                    warn!("knock failed: {e:?}");
                    fail_illegal(cmd, current_sense);
                }
            }
        }
        (0xFD, 0x20) => {
            info!("flash start (FD 20)");
            cmd.pass();
        }
        (0xFD, 0x28) => {
            let (addr, len, acc, opt) = cdb_reg_len(&raw);
            info!("reg read FD 28 addr={addr:#04x} len={len} acc={acc} opt={opt}");
            let mut buf = [0u8; 512];
            let n = len;
            let res = if addr == 0 && n == asic::BFLASH_LEN {
                asic::bflash_read(asic, &mut buf[..n])
            } else {
                asic::reg_read_n(asic, addr, &mut buf[..n])
            };
            match res {
                Ok(()) => {
                    let _ = cmd.write_data(&buf[..n]);
                    cmd.pass();
                }
                Err(e) => {
                    warn!("reg read failed: {e:?}");
                    fail_illegal(cmd, current_sense);
                }
            }
        }
        (0xFD, 0x2A) => {
            let (addr, len, acc, opt) = cdb_reg_len(&raw);
            info!("reg write FD 2A addr={addr:#04x} len={len} acc={acc} opt={opt}");
            let mut buf = [0u8; 512];
            let n = len;
            let got = if cmd.is_data_out() && cmd.data_transfer_len() > 0 {
                cmd.read_data(&mut buf[..n]).unwrap_or(0)
            } else {
                0
            };
            if got == 0 && cmd.is_data_out() && cmd.data_transfer_len() > 0 {
                // CBW processed before the OUT data packet; try again next poll.
                return;
            }
            let payload = &buf[..got.min(n)];
            let res = if asic::needs_handshake(addr) {
                payload.iter().enumerate().try_fold((), |_, (i, &v)| {
                    asic::write_reg_protected(asic, addr.wrapping_add(i as u8), v)
                })
            } else {
                let mut pkt = [0u8; 513];
                pkt[0] = addr;
                pkt[1..1 + payload.len()].copy_from_slice(payload);
                asic.i2c_write(&pkt[..1 + payload.len()])
            };
            match res {
                Ok(()) => cmd.pass(),
                Err(e) => {
                    warn!("reg write failed: {e:?}");
                    fail_illegal(cmd, current_sense);
                }
            }
        }
        (0xFF, 0xFF) if data_in => {
            info!("I2C scan (FF FF IN)");
            match asic::scan_i2c(asic) {
                Ok((id, st)) => {
                    info!("scan slave={id:#04x} status={st}");
                    let _ = cmd.write_data(&[id, st]);
                    cmd.pass();
                }
                Err(e) => {
                    warn!("scan failed: {e:?}");
                    let _ = cmd.write_data(&[0, 1]);
                    cmd.pass();
                }
            }
        }
        (0xFF, 0xFF) => {
            info!("exit I2C (FF FF OUT)");
            match asic.exit_i2c() {
                Ok(()) => cmd.pass(),
                Err(e) => {
                    warn!("exit I2C failed: {e:?}");
                    fail_illegal(cmd, current_sense);
                }
            }
        }
        _ => {
            info!("Unknown command: {:02x?}", &raw[..raw.len()]);
            fail_illegal(cmd, current_sense);
        }
    }
}

#[embassy_executor::task]
async fn usb_task(
    mut usb_dev: UsbDevice<'static, UsbBusType>,
    mut scsi: ScsiDev,
    mut asic: AsicDev,
) {
    info!("USB task started!");

    // Current sense: (Key, ASC, ASCQ)
    let mut current_sense = (SENSE_KEY_NO_SENSE, 0x00, 0x00);

    loop {
        USB_SIGNAL.wait().await;
        
        // Drain events
        loop {
            let processed = usb_dev.poll(&mut [&mut scsi]);

            // Drive SCSI/BOT state machine. This is required in addition to usb_dev.poll().
            let _ = scsi.poll(|mut cmd| {
                match cmd.kind {
                    ScsiCommand::Inquiry {
                        evpd,
                        page_code,
                        alloc_len,
                        ..
                    } => {
                        if evpd {
                            match page_code {
                                0x00 => {
                                    // Supported VPD pages
                                    // [Peripheral, Page=0x00, Rsvd, Len, ...pages]
                                    let vpd = [0x00, 0x00, 0x00, 0x03, 0x00, 0x80, 0x83];
                                    let max_len = core::cmp::min(alloc_len as usize, vpd.len());
                                    let _ = cmd.try_write_data_all(&vpd[..max_len]);
                                    cmd.pass();
                                }
                                0x80 => {
                                    // Unit Serial Number VPD
                                    // [Peripheral, Page=0x80, Rsvd, PageLen, ...Serial]
                                    let serial_len = UNIT_SERIAL.len() as u8;
                                    let mut vpd = [0u8; 4 + 32];
                                    vpd[0] = 0x00;
                                    vpd[1] = 0x80;
                                    vpd[2] = 0x00;
                                    vpd[3] = serial_len;
                                    vpd[4..4 + UNIT_SERIAL.len()].copy_from_slice(UNIT_SERIAL);

                                    let total_len = 4 + UNIT_SERIAL.len();
                                    let max_len = core::cmp::min(alloc_len as usize, total_len);
                                    let _ = cmd.try_write_data_all(&vpd[..max_len]);
                                    cmd.pass();
                                }
                                0x83 => {
                                    // Device Identification VPD (minimal, one ASCII vendor-specific designator)
                                    // Page header: [Peripheral, 0x83, Rsvd, PageLen]
                                    // Designator: [CodeSet=ASCII(0x01), IdType=VendorSpecific(0x00), Rsvd, IdLen, ...Id]
                                    let id_len = UNIT_SERIAL.len() as u8;
                                    let page_len = 4 + id_len; // designator header (4) + identifier

                                    let mut vpd = [0u8; 4 + 4 + 8];
                                    vpd[0] = 0x00;
                                    vpd[1] = 0x83;
                                    vpd[2] = 0x00;
                                    vpd[3] = page_len;
                                    vpd[4] = 0x01; // Code set: ASCII
                                    vpd[5] = 0x00; // Identifier type: vendor specific; association: LUN
                                    vpd[6] = 0x00;
                                    vpd[7] = id_len;
                                    vpd[8..8 + UNIT_SERIAL.len()].copy_from_slice(UNIT_SERIAL);

                                    let total_len = 8 + UNIT_SERIAL.len();
                                    let max_len = core::cmp::min(alloc_len as usize, total_len);
                                    let _ = cmd.try_write_data_all(&vpd[..max_len]);
                                    cmd.pass();
                                }
                                _ => {
                                    current_sense = (
                                        SENSE_KEY_ILLEGAL_REQUEST,
                                        ASC_INVALID_COMMAND_OP_CODE,
                                        ASCQ_INVALID_COMMAND_OP_CODE,
                                    );
                                    cmd.fail();
                                }
                            }
                            return;
                        }

                        // Standard inquiry data (36 bytes + 8 magic bytes)
                        let mut inquiry_data = [
                            0x00, // Peripheral Device Type: Direct-access block device
                            0x80, // Removable
                            0x00, // Version: no standard conformance
                            0x00, // Response data format: unknown
                            36 - 5, // Additional length
                            0x00, 0x00, 0x00,
                            // Vendor (8)
                            0, 0, 0, 0, 0, 0, 0, 0,
                            // Product (16)
                            0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                            // Revision (4)
                            0, 0, 0, 0,
                            // magic (8)
                            0, 0, 0, 0, 0, 0, 0, 0,
                        ];
                        inquiry_data[8..16].copy_from_slice(&INQ_VENDOR);
                        inquiry_data[16..32].copy_from_slice(&INQ_PRODUCT);
                        inquiry_data[32..36].copy_from_slice(&INQ_REV);
                        inquiry_data[36..44].copy_from_slice(&INQ_MAGIC);

                        let max_len = core::cmp::min(alloc_len as usize, inquiry_data.len());
                        let _ = cmd.try_write_data_all(&inquiry_data[..max_len]);
                        cmd.pass();
                    }
                    ScsiCommand::TestUnitReady => {
                        // Report "no media"
                        current_sense = (
                            SENSE_KEY_NOT_READY,
                            ASC_MEDIUM_NOT_PRESENT,
                            ASCQ_MEDIUM_NOT_PRESENT,
                        );
                        cmd.fail();
                    }
                    ScsiCommand::RequestSense { alloc_len, .. } => {
                        // Fixed format sense data (18 bytes)
                        let sense_data = [
                            0xf0, // Response Code (Current errors)
                            0x00,
                            current_sense.0, // Sense Key
                            0x00, 0x00, 0x00, 0x00, // Information
                            0x0A, // Additional Sense Length
                            0x00, 0x00, 0x00, 0x00, // Cmd specific info
                            current_sense.1, // ASC
                            current_sense.2, // ASCQ
                            0x00, // FRU
                            0x00, 0x00, 0x00, // Sense key specific
                        ];

                        // Clear sense after reporting it
                        current_sense = (SENSE_KEY_NO_SENSE, 0x00, 0x00);

                        let max_len = core::cmp::min(alloc_len as usize, sense_data.len());
                        let _ = cmd.try_write_data_all(&sense_data[..max_len]);
                        cmd.pass();
                    }
                    ScsiCommand::ModeSense6 { alloc_len, .. } => {
                        // No media: return CHECK CONDITION (NOT READY / MEDIUM NOT PRESENT).
                        //
                        // Many OSes probe MODE SENSE early; when there is no backing medium we
                        // want consistent behavior with TEST UNIT READY / READ CAPACITY, so the
                        // host learns the 0x02/0x3A00 condition via REQUEST SENSE.
                        let _ = alloc_len; // keep pattern consistent; no data phase on failure
                        current_sense = (
                            SENSE_KEY_NOT_READY,
                            ASC_MEDIUM_NOT_PRESENT,
                            ASCQ_MEDIUM_NOT_PRESENT,
                        );
                        cmd.fail();
                    }
                    ScsiCommand::ModeSense10 { alloc_len, .. } => {
                        // No media: return CHECK CONDITION (NOT READY / MEDIUM NOT PRESENT).
                        let _ = alloc_len; // keep pattern consistent; no data phase on failure
                        current_sense = (
                            SENSE_KEY_NOT_READY,
                            ASC_MEDIUM_NOT_PRESENT,
                            ASCQ_MEDIUM_NOT_PRESENT,
                        );
                        cmd.fail();
                    }
                    ScsiCommand::ReadCapacity10
                    | ScsiCommand::ReadCapacity16 { .. }
                    | ScsiCommand::Read { .. }
                    | ScsiCommand::Write { .. }
                    | ScsiCommand::ReadFormatCapacities { .. } => {
                        // No media: fail all capacity/read/write style commands
                        current_sense = (
                            SENSE_KEY_NOT_READY,
                            ASC_MEDIUM_NOT_PRESENT,
                            ASCQ_MEDIUM_NOT_PRESENT,
                        );
                        cmd.fail();
                    }
                    ScsiCommand::Unknown { .. } => {
                        handle_vendor(&mut asic, cmd, &mut current_sense);
                    }
                    _ => {
                        let raw = cmd.cdb_bytes();
                        info!("Unknown command: {:?}", &raw[..raw.len() as usize]);
                        // `ScsiCommand` is non_exhaustive; default to ILLEGAL REQUEST.
                        current_sense = (
                            SENSE_KEY_ILLEGAL_REQUEST,
                            ASC_INVALID_COMMAND_OP_CODE,
                            ASCQ_INVALID_COMMAND_OP_CODE,
                        );
                        cmd.fail();
                    }
                }
            });

            if !processed {
                break;
            }
        }

        interrupt::enable(Interrupt::USB, usb_interrupt_handler.priority()).unwrap();
    }
}

#[esp_rtos::main]
async fn main(spawner: Spawner) -> ! {
    esp_println::logger::init_logger_from_env();
    let peripherals = esp_hal::init(esp_hal::Config::default());

    esp_alloc::heap_allocator!(#[esp_hal::ram(reclaimed)] size: 73744);

    let timg0 = TimerGroup::new(peripherals.TIMG0);
    esp_rtos::start(timg0.timer0);

    info!("Embassy initialized!");

    #[cfg(feature = "mock-asic")]
    let asic = {
        info!("usbdisk2: mock ASIC (no FT8132 wiring)");
        let mut asic = MockAsic::from_fixtures();
        match asic::enter_i2c(&mut asic) {
            Ok(id) => {
                let scan = asic::scan_i2c(&mut asic).ok();
                let mut bflash = [0u8; asic::BFLASH_LEN];
                let b111 = asic::bflash_read(&mut asic, &mut bflash)
                    .ok()
                    .map(|_| bflash[111]);
                let mut status = [0u8; 44];
                let st0 = asic::reg_read_n(&mut asic, 0x40, &mut status)
                    .ok()
                    .map(|_| status[0]);
                let wr = asic::write_reg_protected(&mut asic, 0x86, 0x5A)
                    .ok()
                    .and_then(|_| asic::reg_read(&mut asic, 0x86).ok());
                info!(
                    "mock Find self-check: chip_id={id:#04x} (want {:#04x}) scan={scan:?} bflash[111]={b111:?} status[0]={st0:?} write 0x86->{wr:?}",
                    asic::CHIP_ID
                );
                let _ = asic.exit_i2c();
            }
            Err(e) => warn!("mock enter_i2c failed: {e:?}"),
        }
        asic
    };
    #[cfg(not(feature = "mock-asic"))]
    let asic = {
        info!("usbdisk2: real FT8132 on GPIO48/GPIO4");
        FT8132::new(
            peripherals.GPIO48,
            peripherals.GPIO4,
            peripherals.I2C0,
            peripherals.RMT,
        )
    };

    let usb = Usb::new(peripherals.USB0, peripherals.GPIO20, peripherals.GPIO19);
    static EP_MEMORY_CELL: StaticCell<[u32; 512]> = StaticCell::new();
    static USB_BUS: StaticCell<UsbBusAllocator<UsbBusType>> = StaticCell::new();
    
    let usb_bus = USB_BUS.init(UsbBus::new(usb, EP_MEMORY_CELL.init([0; 512])));

    static IO_BUF: StaticCell<[u8; 512]> = StaticCell::new();
    let io_buf_arr = IO_BUF.init([0; 512]);
    let io_buf = IoBuf(&mut io_buf_arr[..]);

    let scsi = Scsi::new(usb_bus, 64, 0, io_buf).unwrap();

    // device_class(0) means use interface descriptors (MSC is defined there).
    let usb_dev = UsbDeviceBuilder::new(usb_bus, UsbVidPid(0x1B3F, 0x8000))
        // .strings(&[
        //     StringDescriptors::new(LangID::EN)
        //         .manufacturer("GENPLUS")
        //         .product("USB-MSDC DISK A")
        //         .serial_number("12345678"),
        // ]).unwrap()
        .device_class(0x00)
        .max_packet_size_0(64).unwrap()
        .max_power(0).unwrap()
        .usb_rev(usb_device::device::UsbRev::Usb200)
        .build();

    spawner.spawn(usb_task(usb_dev, scsi, asic)).unwrap();

    unsafe {
        interrupt::bind_interrupt(Interrupt::USB, usb_interrupt_handler.handler());
    }
    interrupt::enable(Interrupt::USB, usb_interrupt_handler.priority()).unwrap();

    info!("USB initialized!");

    loop {
        // Main task can do other things
        Timer::after(Duration::from_secs(1)).await;
    }
}
