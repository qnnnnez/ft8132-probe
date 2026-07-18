use core::convert::TryInto;
use log::{error, info, warn};
use usb_device::class_prelude::*;
use usb_device::{Result, control};

pub const USB_CLASS_MSC: u8 = 0x08;
pub const MSC_SUBCLASS_SCSI: u8 = 0x06;
pub const MSC_PROTOCOL_BOT: u8 = 0x50;

const REQ_GET_MAX_LUN: u8 = 0xFE;
const REQ_RESET: u8 = 0xFF;

// CBW Signature: 'USBC'
const CBW_SIGNATURE: u32 = 0x43425355;
// CSW Signature: 'USBS'
const CSW_SIGNATURE: u32 = 0x53425355;

// SCSI Commands
const SCSI_CMD_TEST_UNIT_READY: u8 = 0x00;
const SCSI_CMD_REQUEST_SENSE: u8 = 0x03;
const SCSI_CMD_INQUIRY: u8 = 0x12;
const SCSI_CMD_MODE_SENSE_6: u8 = 0x1A;
const SCSI_CMD_PREVENT_ALLOW_MEDIUM_REMOVAL: u8 = 0x1E;
const SCSI_CMD_READ_CAPACITY_10: u8 = 0x25;
const SCSI_CMD_READ_10: u8 = 0x28;
const SCSI_CMD_WRITE_10: u8 = 0x2A;
const SCSI_CMD_READ_FORMAT_CAPACITIES: u8 = 0x23;

const SENSE_KEY_NO_SENSE: u8 = 0x00;
// const SENSE_KEY_RECOVERED_ERROR: u8 = 0x01;
const SENSE_KEY_NOT_READY: u8 = 0x02;
// const SENSE_KEY_MEDIUM_ERROR: u8 = 0x03;
// const SENSE_KEY_HARDWARE_ERROR: u8 = 0x04;
const SENSE_KEY_ILLEGAL_REQUEST: u8 = 0x05;
// const SENSE_KEY_UNIT_ATTENTION: u8 = 0x06;
// const SENSE_KEY_DATA_PROTECT: u8 = 0x07;
// const SENSE_KEY_BLANK_CHECK: u8 = 0x08;
// const SENSE_KEY_ABORTED_COMMAND: u8 = 0x0B;

const REQ_CLEAR_FEATURE: u8 = 0x01;

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
enum State {
    Idle,
    /// Waiting for data from host to drain it (WRITE_10)
    #[allow(dead_code)]
    ReceivingData { residue: u32, csw: Csw },
    /// Waiting for IN endpoint to be free to send CSW
    SendingCsw(Csw),
    /// Waiting for IN endpoint to be unstalled by host before sending CSW
    WaitingForUnstall(Csw),
}

#[derive(Debug, Clone, Copy)]
struct Cbw {
    tag: u32,
    data_transfer_length: u32,
    flags: u8,
    #[allow(dead_code)]
    lun: u8,
    #[allow(dead_code)]
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

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
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

pub struct UsbMassStorage<'a, B: UsbBus> {
    interface: InterfaceNumber,
    read_ep: EndpointOut<'a, B>,
    write_ep: EndpointIn<'a, B>,
    state: State,
    max_lun: u8,
    current_sense: (u8, u8, u8), // Key, ASC, ASCQ
    buf: [u8; 64],
}

impl<'a, B: UsbBus> UsbMassStorage<'a, B> {
    pub fn new(alloc: &'a UsbBusAllocator<B>, max_lun: u8) -> Self {
        Self {
            interface: alloc.interface(),
            read_ep: alloc.bulk(64),
            write_ep: alloc.bulk(64),
            state: State::Idle,
            max_lun,
            current_sense: (SENSE_KEY_NO_SENSE, 0x00, 0x00),
            buf: [0; 64],
        }
    }

    fn write_csw_or_transition(&mut self, csw: Csw) -> Result<()> {
        let mut buf = [0u8; 13];
        csw.to_bytes(&mut buf);
        info!("Sending CSW: Tag=0x{:X}, Residue={}, Status={}", csw.tag, csw.data_residue, csw.status);
        
        match self.write_ep.write(&buf) {
            Ok(_) => {
                self.state = State::Idle;
                Ok(())
            }
            Err(UsbError::WouldBlock) => {
                info!("CSW WouldBlock -> SendingCsw");
                self.state = State::SendingCsw(csw);
                Ok(())
            }
            Err(e) => {
                error!("CSW write error: {:?}", e);
                Err(e)
            }
        }
    }
}

impl<'a, B: UsbBus> UsbClass<B> for UsbMassStorage<'a, B> {
    fn get_configuration_descriptors(&self, writer: &mut DescriptorWriter) -> Result<()> {
        writer.interface(
            self.interface,
            USB_CLASS_MSC,
            MSC_SUBCLASS_SCSI,
            MSC_PROTOCOL_BOT,
        )?;
        writer.endpoint(&self.read_ep)?;
        writer.endpoint(&self.write_ep)?;
        Ok(())
    }

    fn reset(&mut self) {
        info!("MSC Reset");
        self.state = State::Idle;
        self.current_sense = (SENSE_KEY_NO_SENSE, 0x00, 0x00);
    }

    fn poll(&mut self) {
        if let State::SendingCsw(csw) = self.state {
             info!("Polling CSW retry");
            self.write_csw_or_transition(csw).ok();
        }
    }

    fn control_in(&mut self, xfer: ControlIn<B>) {
        let req = xfer.request();
        info!("CTRL IN: Type={:?}, Req=0x{:02X}, Val=0x{:04X}, Idx=0x{:04X}, Len={}", 
            req.request_type, req.request, req.value, req.index, req.length);

        if req.request_type == control::RequestType::Class
            && req.recipient == control::Recipient::Interface
            && req.index == u8::from(self.interface) as u16
        {
            if req.request == REQ_GET_MAX_LUN && req.length == 1 {
                info!("GET_MAX_LUN");
                xfer.accept(|buf| {
                    buf[0] = self.max_lun;
                    Ok(1)
                }).ok();
            } else {
                warn!("Unknown Class IN request: 0x{:02X}", req.request);
                xfer.reject().ok();
            }
        }
    }

    fn control_out(&mut self, xfer: ControlOut<B>) {
        let req = xfer.request();
        info!("CTRL OUT: Type={:?}, Req=0x{:02X}, Val=0x{:04X}, Idx=0x{:04X}, Len={}", 
            req.request_type, req.request, req.value, req.index, req.length);

        // Check for Standard Clear Feature on Endpoint (Unstall)
        if req.request_type == control::RequestType::Standard
            && req.recipient == control::Recipient::Endpoint
            && req.request == REQ_CLEAR_FEATURE
            && req.value == 0x00 // ENDPOINT_HALT
        {
            let addr = EndpointAddress::from(req.index as u8);
            if addr == self.write_ep.address() {
                info!("Clear Halt on IN EP");
                if let State::WaitingForUnstall(csw) = self.state {
                    // Endpoint is being unstalled by host. Transition to SendingCsw.
                    // The actual unstall logic happens in UsbDevice after this callback.
                    // So we wait for next poll() to send CSW.
                    self.state = State::SendingCsw(csw);
                }
            } else if addr == self.read_ep.address() {
                info!("Clear Halt on OUT EP");
            }
        }

        if req.request_type == control::RequestType::Class
            && req.recipient == control::Recipient::Interface
            && req.index == u8::from(self.interface) as u16
        {
            if req.request == REQ_RESET {
                info!("Bulk-Only Mass Storage Reset");
                self.state = State::Idle;
                self.current_sense = (SENSE_KEY_NO_SENSE, 0x00, 0x00);
                xfer.accept().ok();
            } else {
                warn!("Unknown Class OUT request: 0x{:02X}", req.request);
                xfer.reject().ok();
            }
        }
    }

    fn endpoint_in_complete(&mut self, addr: EndpointAddress) {
        if addr == self.write_ep.address() {
            if let State::SendingCsw(csw) = self.state {
                info!("EP IN Complete -> Retry CSW");
                if let Err(e) = self.write_csw_or_transition(csw) {
                     error!("Failed to write CSW in complete: {:?}", e);
                     self.state = State::Idle; // Reset on error to recover
                }
            }
        }
    }

    fn endpoint_out(&mut self, addr: EndpointAddress) {
        if addr != self.read_ep.address() {
            return;
        }

        match self.state {
            State::Idle => {
                let count = match self.read_ep.read(&mut self.buf) {
                    Ok(c) => c,
                    Err(UsbError::WouldBlock) => return,
                    Err(_) => return,
                };

                info!("EP OUT: {} bytes: {:02X?}", count, &self.buf[..count]);

                let cbw = match Cbw::parse(&self.buf[..count]) {
                    Some(c) => c,
                    None => {
                        error!("Invalid CBW received. Length: {}", count);
                        return; // Ignore invalid CBW
                    }
                };

                info!("CBW: Tag=0x{:X}, Len={}, Flags=0x{:02X}, Cmd=0x{:02X}", 
                    cbw.tag, cbw.data_transfer_length, cbw.flags, cbw.cb[0]);

                let mut residue = cbw.data_transfer_length;
                let mut status = 0x00; // Passed

                match cbw.cb[0] {
                    SCSI_CMD_TEST_UNIT_READY => {
                        info!("SCSI: TEST_UNIT_READY");
                        status = 0x01; // Check Condition
                        self.current_sense = (SENSE_KEY_NOT_READY, 0x3A, 0x00); // Medium Not Present
                    }
                    SCSI_CMD_REQUEST_SENSE => {
                        info!("SCSI: REQUEST_SENSE");
                        let sense_data = [
                            0x70, // Fixed
                            0x00,
                            self.current_sense.0, // Sense Key
                            0x00,
                            0x00,
                            0x00,
                            0x00,
                            0x0A, // Additional Sense Length (10)
                            0x00,
                            0x00,
                            0x00,
                            0x00,
                            self.current_sense.1, // ASC
                            self.current_sense.2, // ASCQ
                            0x00, 0x00, 0x00, 0x00,
                        ];
                        // Clear sense after reading
                        self.current_sense = (SENSE_KEY_NO_SENSE, 0x00, 0x00);

                        let len = core::cmp::min(residue as usize, sense_data.len());
                        if let Err(e) = self.write_ep.write(&sense_data[..len]) {
                             error!("Write SENSE failed: {:?}", e);
                             status = 0x01;
                        } else {
                            residue -= len as u32;
                        }
                    }
                    SCSI_CMD_PREVENT_ALLOW_MEDIUM_REMOVAL => {
                        info!("SCSI: PREVENT_ALLOW_MEDIUM_REMOVAL");
                        // Just ACK
                    }
                    SCSI_CMD_INQUIRY => {
                        info!("SCSI: INQUIRY");
                        let inquiry_data = [
                            0x00, // Direct Access Block Device
                            0x80, // Removable
                            0x00, // ANSI version - 0x00 No Compliance to any Standard
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
                             if let Err(e) = self.write_ep.write(&inquiry_data[..len]) {
                                error!("Write INQUIRY failed: {:?}", e);
                                status = 0x01;
                            } else {
                                residue -= len as u32;
                            }
                        }
                    }
                    SCSI_CMD_READ_CAPACITY_10 => {
                        info!("SCSI: READ_CAPACITY_10");
                        status = 0x01;
                        self.current_sense = (SENSE_KEY_NOT_READY, 0x3A, 0x00);
                    }
                    SCSI_CMD_READ_10 => {
                        info!("SCSI: READ_10");
                        status = 0x01;
                        self.current_sense = (SENSE_KEY_NOT_READY, 0x3A, 0x00);
                    }
                    SCSI_CMD_WRITE_10 => {
                         info!("SCSI: WRITE_10");
                         status = 0x01;
                         self.current_sense = (SENSE_KEY_NOT_READY, 0x3A, 0x00);
                         // Data OUT error handled in default match below (residue > 0)
                    }
                    SCSI_CMD_MODE_SENSE_6 => {
                         info!("SCSI: MODE_SENSE_6");
                         let header = [0x03, 0x00, 0x00, 0x00];
                         let len = core::cmp::min(residue as usize, header.len());
                         if let Err(e) = self.write_ep.write(&header[..len]) {
                            error!("Write MODE_SENSE_6 failed: {:?}", e);
                            status = 0x01;
                         } else {
                            residue -= len as u32;
                         }
                    }
                     SCSI_CMD_READ_FORMAT_CAPACITIES => {
                        info!("SCSI: READ_FORMAT_CAPACITIES");
                        status = 0x01;
                        self.current_sense = (SENSE_KEY_NOT_READY, 0x3A, 0x00);
                    }
                    _ => {
                        warn!("SCSI: Unsupported Command 0x{:02X}", cbw.cb[0]);
                        status = 0x01;
                        self.current_sense = (SENSE_KEY_ILLEGAL_REQUEST, 0x20, 0x00);
                    }
                }

                if status != 0 && residue > 0 {
                    let csw = Csw {
                        tag: cbw.tag,
                        data_residue: residue,
                        status,
                    };

                    if (cbw.flags & 0x80) != 0 {
                        warn!("Stalling IN endpoint due to failed Data-IN command");
                        // Data IN direction - Stall write_ep
                        self.write_ep.stall();
                        self.state = State::WaitingForUnstall(csw);
                        return;
                    } else {
                        warn!("Stalling OUT endpoint due to failed Data-OUT command");
                        // Data OUT direction - Stall read_ep
                        // For Data OUT, we can try to send CSW immediately on write_ep
                        // because write_ep is not stalled.
                        // Host will clear read_ep stall then read CSW from write_ep.
                        self.read_ep.stall();
                        self.write_csw_or_transition(csw).ok();
                        return;
                    }
                }

                // If we are here, we are ready to send CSW (or transition to SendingCsw)
                
                let csw = Csw {
                    tag: cbw.tag,
                    data_residue: residue,
                    status,
                };
                
                self.write_csw_or_transition(csw).ok();
            }
            State::ReceivingData { mut residue, mut csw } => {
                info!("Receiving Data, residue: {}", residue);
                // Should not reach here if we stall on error. 
                // Kept for cases where we decide not to stall but drain (if any).
                match self.read_ep.read(&mut self.buf) {
                    Ok(n) => {
                         info!("EP OUT (Data): {} bytes", n);
                         if residue >= n as u32 {
                            residue -= n as u32;
                         } else {
                            residue = 0;
                         }
                         
                         if residue == 0 {
                             csw.data_residue = residue;
                             self.write_csw_or_transition(csw).ok();
                         } else {
                             self.state = State::ReceivingData { residue, csw };
                         }
                    }
                    Err(_) => {}
                }
            }
            State::SendingCsw(_) | State::WaitingForUnstall(_) => {
                // Ignore
            }
        }
    }
}
