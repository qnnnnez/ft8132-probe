#![no_std]
#![no_main]

use core::cell::UnsafeCell;

use embassy_executor::Spawner;
use embassy_stm32::gpio::{Flex, Pull, Speed};
use embassy_stm32::interrupt::{self, InterruptExt};
use embassy_stm32::time::Hertz;
use embassy_stm32::Config;
use embassy_sync::blocking_mutex::raw::CriticalSectionRawMutex;
use embassy_sync::signal::Signal;
use embassy_time::{Duration, Instant, Timer};
use ft8132_probe::asic::{self, AsicBus};
use ft8132_probe::usb_hw::{ProbeUsbBus, UsbHw};
use panic_halt as _;
use static_cell::StaticCell;
use stm32_usbd::UsbBus;
use usb_device::bus::UsbBusAllocator;
use usb_device::prelude::{UsbDevice, UsbDeviceBuilder, UsbVidPid};
use usbd_storage::subclass::scsi::{Scsi, ScsiCommand};
use usbd_storage::subclass::Command;

#[cfg(feature = "mock-asic")]
use ft8132_probe::mock_asic::MockAsic;
#[cfg(not(feature = "mock-asic"))]
use ft8132_probe::ft8132::FT8132;

// Size-sensitive: keep format strings out of flash.
macro_rules! info {
    ($($t:tt)*) => {{}};
}
macro_rules! warn {
    ($($t:tt)*) => {{}};
}

static USB_SIGNAL: Signal<CriticalSectionRawMutex, ()> = Signal::new();

type UsbBusType = ProbeUsbBus;
type ScsiDev = Scsi<usbd_storage::transport::bbb::BulkOnly<'static, UsbBusType, IoBuf>>;

#[cfg(feature = "mock-asic")]
type AsicDev = MockAsic;
#[cfg(not(feature = "mock-asic"))]
type AsicDev = FT8132;

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

/// 1024-byte 0/1 MTP image for `FE 10` / `FE 20`. Programming reads this
/// buffer only, so a dialect has no USB gap inside it.
struct LabImage(UnsafeCell<[u8; asic::MTP_BITS]>);
unsafe impl Sync for LabImage {}
static LAB_IMAGE: LabImage = LabImage(UnsafeCell::new([0; asic::MTP_BITS]));

fn lab_image() -> &'static mut [u8] {
    unsafe { &mut *LAB_IMAGE.0.get() }
}

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

#[allow(non_snake_case)]
#[unsafe(no_mangle)]
pub unsafe extern "C" fn USB_LP_CAN1_RX0() {
    USB_SIGNAL.signal(());
    interrupt::USB_LP_CAN1_RX0.disable();
}

fn usb_irq_enable() {
    interrupt::USB_LP_CAN1_RX0.unpend();
    unsafe { interrupt::USB_LP_CAN1_RX0.enable() };
}

fn cdb_reg_len(raw: &[u8; 16]) -> (u16, usize, u8, u8) {
    let acc = raw[6];
    let opt = raw[9];
    let addr = u16::from_be_bytes([raw[4], raw[5]]);
    let mut len = u16::from_be_bytes([raw[7], raw[8]]) as usize;
    if len == 0 {
        len = 1;
    }
    (addr, len.min(512), acc, opt)
}

fn is_find_bflash(acc: u8, opt: u8, addr: u16, len: usize) -> bool {
    acc == 5 && opt == 5 && addr == 0 && len == asic::BFLASH_LEN
}

fn is_mtp_read(acc: u8, opt: u8) -> bool {
    acc == 5 && matches!(opt, 5 | 6)
}

fn is_mtp_write(acc: u8, opt: u8) -> bool {
    acc == 5 && matches!(opt, 5 | 6 | 133)
}

/// Stashed bk/c1 CDBs; programmed with c2 as one I2C session (no USB hole).
struct MtpStash {
    bk: [u8; asic::BMTP_BITS],
    c1: [u8; asic::CMTP_HALF],
    c2: [u8; asic::CMTP_HALF],
}

impl MtpStash {
    const fn new() -> Self {
        Self {
            bk: [0; asic::BMTP_BITS],
            c1: [0; asic::CMTP_HALF],
            c2: [0; asic::CMTP_HALF],
        }
    }
}

struct VendorOut {
    active: bool,
    addr: u16,
    /// Bytes to drain from the BOT OUT phase (`dCBWDataTransferLength`).
    need: usize,
    /// CDB length: bits/bytes actually programmed after the drain.
    payload: usize,
    acc: u8,
    opt: u8,
    got: usize,
    buf: [u8; 512],
    stash: MtpStash,
}

impl VendorOut {
    const fn new() -> Self {
        Self {
            active: false,
            addr: 0,
            need: 0,
            payload: 0,
            acc: 0,
            opt: 0,
            got: 0,
            buf: [0; 512],
            stash: MtpStash::new(),
        }
    }

    fn reset(&mut self) {
        self.active = false;
        self.got = 0;
        self.payload = 0;
    }
}

/// `FD 20` HV waits spanned across `poll_command` so USB SOF keeps moving.
#[derive(Clone, Copy)]
enum MtpErase {
    Idle,
    Wait1 { deadline: Instant },
    Wait2 { deadline: Instant },
}

impl MtpErase {
    const fn new() -> Self {
        Self::Idle
    }
}

fn hv_deadline_us(us: u32) -> Instant {
    Instant::now() + Duration::from_micros(us as u64)
}

fn hv_expired(deadline: Instant) -> bool {
    Instant::now() >= deadline
}

fn fail_illegal(cmd: Command<'_, ScsiCommand, ScsiDev>, current_sense: &mut (u8, u8, u8)) {
    *current_sense = (
        SENSE_KEY_ILLEGAL_REQUEST,
        ASC_INVALID_COMMAND_OP_CODE,
        ASCQ_INVALID_COMMAND_OP_CODE,
    );
    cmd.fail(0);
}

fn stash_copy(dst: &mut [u8], src: &[u8]) {
    let n = src.len().min(dst.len());
    for i in 0..n {
        dst[i] = src[i];
    }
    dst[n..].fill(0);
}

fn handle_mtp_write<B: AsicBus>(
    asic: &mut B,
    cmd: Command<'_, ScsiCommand, ScsiDev>,
    current_sense: &mut (u8, u8, u8),
    out: &mut VendorOut,
    addr: u16,
    n: usize,
    processed: u32,
) {
    match addr {
        asic::BKMTP_OFF => {
            stash_copy(&mut out.stash.bk, &out.buf[..n.min(asic::BMTP_BITS)]);
            out.reset();
            cmd.pass(processed);
        }
        asic::CMTP_OFF => {
            stash_copy(&mut out.stash.c1, &out.buf[..n.min(asic::CMTP_HALF)]);
            out.reset();
            cmd.pass(processed);
        }
        502 => {
            stash_copy(&mut out.stash.c2, &out.buf[..n.min(asic::CMTP_HALF)]);
            let res = asic::mtp_program_image(asic, &out.stash.bk, &out.stash.c1, &out.stash.c2);
            out.reset();
            match res {
                Ok(()) => cmd.pass(processed),
                Err(_e) => {
                    warn!("mtp program image failed: {_e:?}");
                    fail_illegal(cmd, current_sense);
                }
            }
        }
        _ => {
            out.reset();
            fail_illegal(cmd, current_sense);
        }
    }
}

fn handle_lab_load(
    mut cmd: Command<'_, ScsiCommand, ScsiDev>,
    current_sense: &mut (u8, u8, u8),
    out: &mut VendorOut,
) {
    let raw = cmd.cdb;
    let offset = u16::from_be_bytes([raw[4], raw[5]]) as usize;
    let len = u16::from_be_bytes([raw[7], raw[8]]) as usize;
    if len == 0 || len > out.buf.len() || offset.saturating_add(len) > asic::MTP_BITS {
        out.reset();
        fail_illegal(cmd, current_sense);
        return;
    }
    if !out.active || out.addr as usize != offset || out.need != len {
        out.active = true;
        out.addr = offset as u16;
        out.need = len;
        out.payload = len;
        out.acc = 0;
        out.opt = 0;
        out.got = 0;
    }
    if cmd.is_data_out() && cmd.data_transfer_len() > 0 && out.got < out.need {
        let n = cmd.read_data(&mut out.buf[out.got..out.need]).unwrap_or(0);
        out.got = out.got.saturating_add(n);
    }
    if out.got < out.need && cmd.is_data_out() && cmd.data_transfer_len() > 0 {
        return;
    }
    if out.got < len {
        out.reset();
        fail_illegal(cmd, current_sense);
        return;
    }
    lab_image()[offset..offset + len].copy_from_slice(&out.buf[..len]);
    let processed = out.got as u32;
    out.reset();
    cmd.pass(processed);
}

fn handle_vendor<B: AsicBus>(
    asic: &mut B,
    mut cmd: Command<'_, ScsiCommand, ScsiDev>,
    current_sense: &mut (u8, u8, u8),
    out: &mut VendorOut,
    erase: &mut MtpErase,
) {
    asic::hv_on_command();
    let raw = cmd.cdb;
    let op = raw[0];
    let sub = raw[1];
    let data_in = cmd.is_data_in();

    if !matches!((op, sub), (0xFD, 0x2A) | (0xFE, 0x10)) {
        out.reset();
    }
    if (op, sub) != (0xFD, 0x20) {
        *erase = MtpErase::Idle;
    }

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
            let n = cmd.write_data(&resp).unwrap_or(0);
            cmd.pass(n as u32);
        }
        (0xF0, 0xF8) => {
            warn!("stub vendor CDB F0 F8: {raw:02x?}");
            let n = cmd.write_data(&[0; 4]).unwrap_or(0);
            cmd.pass(n as u32);
        }
        (0xF0, 0xF9) => {
            warn!("stub vendor CDB F0 F9: {raw:02x?}");
            let n = cmd.write_data(&[0; 20]).unwrap_or(0);
            cmd.pass(n as u32);
        }
        (0xFD, 0x00) => {
            info!("pre-op / enter I2C (FD 00)");
            asic::hv_on_knock();
            match asic.knock() {
                Ok(()) => cmd.pass(0),
                Err(_e) => {
                    warn!("knock failed: {_e:?}");
                    fail_illegal(cmd, current_sense);
                }
            }
        }
        (0xFD, 0x20) => {
            info!("chip erase (FD 20)");
            match *erase {
                MtpErase::Idle => match asic::hv_allow_erase(asic) {
                    Ok(true) => match asic::mtp_erase_start(asic) {
                        Ok(()) => {
                            *erase = MtpErase::Wait1 {
                                deadline: hv_deadline_us(asic::ERASE_PHASE1_US),
                            };
                        }
                        Err(_e) => {
                            warn!("mtp erase start failed: {_e:?}");
                            fail_illegal(cmd, current_sense);
                        }
                    },
                    Ok(false) => fail_illegal(cmd, current_sense),
                    Err(_e) => {
                        warn!("mtp erase health failed: {_e:?}");
                        fail_illegal(cmd, current_sense);
                    }
                },
                MtpErase::Wait1 { deadline } => {
                    if !hv_expired(deadline) {
                        return;
                    }
                    match asic::mtp_erase_phase2(asic) {
                        Ok(()) => {
                            *erase = MtpErase::Wait2 {
                                deadline: hv_deadline_us(asic::ERASE_PHASE2_US),
                            };
                        }
                        Err(_e) => {
                            *erase = MtpErase::Idle;
                            warn!("mtp erase phase2 failed: {_e:?}");
                            fail_illegal(cmd, current_sense);
                        }
                    }
                },
                MtpErase::Wait2 { deadline } => {
                    if !hv_expired(deadline) {
                        return;
                    }
                    *erase = MtpErase::Idle;
                    match asic::mtp_erase_finish(asic) {
                        Ok(()) => {
                            asic::hv_on_erase_done();
                            cmd.pass(0);
                        }
                        Err(_e) => {
                            warn!("mtp erase finish failed: {_e:?}");
                            fail_illegal(cmd, current_sense);
                        }
                    }
                }
            }
        }
        (0xFD, 0x28) => {
            let (addr, len, acc, opt) = cdb_reg_len(&raw);
            info!("reg read FD 28 addr={addr:#06x} len={len} acc={acc} opt={opt}");
            let mut buf = [0u8; 512];
            let n = len;
            let res = if is_find_bflash(acc, opt, addr, n) {
                asic::bflash_read(asic, &mut buf[..n])
            } else if is_mtp_read(acc, opt) {
                asic::mtp_read_bits(asic, addr, &mut buf[..n])
            } else {
                asic::reg_read_n(asic, addr as u8, &mut buf[..n])
            };
            match res {
                Ok(()) => {
                    if addr == 0x01 && n >= 1 {
                        asic::hv_observe_id(buf[0]);
                    }
                    let written = cmd.write_data(&buf[..n]).unwrap_or(0);
                    cmd.pass(written as u32);
                }
                Err(_e) => {
                    warn!("reg read failed: {_e:?}");
                    fail_illegal(cmd, current_sense);
                }
            }
        }
        (0xFD, 0x2A) => {
            let (addr, payload, acc, opt) = cdb_reg_len(&raw);
            // Official wrapper pads MTP OUT to 512 even when CDB len is 110/374.
            // Drain the full CBW length before programming so USBSTOR does not
            // retry a short residue (seen as "Write bkflash Fail..." after 30s).
            let xfer = (cmd.data_transfer_len() as usize).min(out.buf.len());
            let drain = xfer.max(payload).min(out.buf.len());
            info!("reg write FD 2A addr={addr:#06x} len={payload} acc={acc} opt={opt}");
            if !out.active
                || out.addr != addr
                || out.need != drain
                || out.payload != payload
                || out.acc != acc
                || out.opt != opt
            {
                out.active = true;
                out.addr = addr;
                out.need = drain;
                out.payload = payload;
                out.acc = acc;
                out.opt = opt;
                out.got = 0;
            }
            if cmd.is_data_out() && cmd.data_transfer_len() > 0 && out.got < out.need {
                let n = cmd
                    .read_data(&mut out.buf[out.got..out.need])
                    .unwrap_or(0);
                out.got = out.got.saturating_add(n);
            }
            if out.got < out.need && cmd.is_data_out() && cmd.data_transfer_len() > 0 {
                // CBW processed before the OUT data packet; keep bytes and wait.
                return;
            }
            let n = out.payload.min(out.got);
            let processed = out.got as u32;
            if is_mtp_write(acc, opt) {
                handle_mtp_write(asic, cmd, current_sense, out, addr, n, processed);
                return;
            }
            let res = if asic::needs_handshake(addr as u8) {
                out.buf[..n].iter().enumerate().try_fold((), |_, (i, &v)| {
                    asic::write_reg_protected(asic, (addr as u8).wrapping_add(i as u8), v)
                })
            } else {
                let mut pkt = [0u8; 513];
                pkt[0] = addr as u8;
                pkt[1..1 + n].copy_from_slice(&out.buf[..n]);
                asic.i2c_write(&pkt[..1 + n])
            };
            out.reset();
            match res {
                Ok(()) => cmd.pass(processed),
                Err(_e) => {
                    warn!("reg write failed: {_e:?}");
                    fail_illegal(cmd, current_sense);
                }
            }
        }
        (0xFF, 0xFF) if data_in => {
            info!("I2C scan (FF FF IN)");
            match asic::scan_i2c(asic) {
                Ok((id, st)) => {
                    info!("scan slave={id:#04x} status={st}");
                    let n = cmd.write_data(&[id, st]).unwrap_or(0);
                    cmd.pass(n as u32);
                }
                Err(_e) => {
                    warn!("scan failed: {_e:?}");
                    let n = cmd.write_data(&[0, 1]).unwrap_or(0);
                    cmd.pass(n as u32);
                }
            }
        }
        (0xFE, 0x10) => handle_lab_load(cmd, current_sense, out),
        (0xFE, 0x11) => {
            let off = u16::from_be_bytes([raw[4], raw[5]]) as usize;
            let len = u16::from_be_bytes([raw[7], raw[8]]) as usize;
            let img = lab_image();
            if len == 0 || len > 512 || off.saturating_add(len) > img.len() {
                fail_illegal(cmd, current_sense);
            } else {
                let n = cmd.write_data(&img[off..off + len]).unwrap_or(0);
                cmd.pass(n as u32);
            }
        }
        (0xFE, 0x30) => match asic.clamp_bus(raw[2] != 0) {
            Ok(()) => cmd.pass(0),
            Err(_e) => fail_illegal(cmd, current_sense),
        },
        (0xFE, 0x20) => {
            let dialect = raw[2];
            let gap = u32::from(u16::from_be_bytes([raw[4], raw[5]]));
            match asic::lab_program(asic, lab_image(), dialect, gap) {
                Ok(true) => cmd.pass(0),
                Ok(false) => fail_illegal(cmd, current_sense),
                Err(_e) => {
                    warn!("lab program failed: {_e:?}");
                    fail_illegal(cmd, current_sense);
                }
            }
        }
        (0xFF, 0xFF) => {
            info!("exit I2C (FF FF OUT)");
            match asic.exit_i2c() {
                Ok(()) => cmd.pass(0),
                Err(_e) => {
                    warn!("exit I2C failed: {_e:?}");
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

async fn run_usb(
    mut usb_dev: UsbDevice<'static, UsbBusType>,
    mut scsi: ScsiDev,
    mut asic: AsicDev,
) {
    info!("USB task started!");

    // Current sense: (Key, ASC, ASCQ)
    let mut current_sense = (SENSE_KEY_NO_SENSE, 0x00, 0x00);
    let mut vendor_out = VendorOut::new();
    let mut mtp_erase = MtpErase::new();

    loop {
        // Drain events
        loop {
            let processed = usb_dev.poll(&mut [&mut scsi]);

            // Handle the current SCSI command. UsbClass::poll (via usb_dev.poll) drives BBB.
            let _ = scsi.poll_command(|mut cmd| {
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
                                    cmd.pass(max_len as u32);
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
                                    cmd.pass(max_len as u32);
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
                                    cmd.pass(max_len as u32);
                                }
                                _ => {
                                    current_sense = (
                                        SENSE_KEY_ILLEGAL_REQUEST,
                                        ASC_INVALID_COMMAND_OP_CODE,
                                        ASCQ_INVALID_COMMAND_OP_CODE,
                                    );
                                    cmd.fail(0);
                                }
                            }
                            return Ok(());
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
                        cmd.pass(max_len as u32);
                    }
                    ScsiCommand::TestUnitReady => {
                        // Report "no media"
                        current_sense = (
                            SENSE_KEY_NOT_READY,
                            ASC_MEDIUM_NOT_PRESENT,
                            ASCQ_MEDIUM_NOT_PRESENT,
                        );
                        cmd.fail(0);
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
                        cmd.pass(max_len as u32);
                    }
                    ScsiCommand::ModeSense6 { alloc_len, .. } => {
                        // No media: return CHECK CONDITION (NOT READY / MEDIUM NOT PRESENT).
                        let _ = alloc_len;
                        current_sense = (
                            SENSE_KEY_NOT_READY,
                            ASC_MEDIUM_NOT_PRESENT,
                            ASCQ_MEDIUM_NOT_PRESENT,
                        );
                        cmd.fail(0);
                    }
                    ScsiCommand::ModeSense10 { alloc_len, .. } => {
                        let _ = alloc_len;
                        current_sense = (
                            SENSE_KEY_NOT_READY,
                            ASC_MEDIUM_NOT_PRESENT,
                            ASCQ_MEDIUM_NOT_PRESENT,
                        );
                        cmd.fail(0);
                    }
                    ScsiCommand::ReadCapacity10
                    | ScsiCommand::ReadCapacity16 { .. }
                    | ScsiCommand::Read { .. }
                    | ScsiCommand::Write { .. }
                    | ScsiCommand::ReadFormatCapacities { .. } => {
                        current_sense = (
                            SENSE_KEY_NOT_READY,
                            ASC_MEDIUM_NOT_PRESENT,
                            ASCQ_MEDIUM_NOT_PRESENT,
                        );
                        cmd.fail(0);
                    }
                    ScsiCommand::Unknown { .. } => {
                        handle_vendor(
                            &mut asic,
                            cmd,
                            &mut current_sense,
                            &mut vendor_out,
                            &mut mtp_erase,
                        );
                    }
                    _ => {
                        let _raw = cmd.cdb_bytes();
                        info!("Unknown command: {:?}", &_raw[.._raw.len() as usize]);
                        current_sense = (
                            SENSE_KEY_ILLEGAL_REQUEST,
                            ASC_INVALID_COMMAND_OP_CODE,
                            ASCQ_INVALID_COMMAND_OP_CODE,
                        );
                        cmd.fail(0);
                    }
                }
                Ok(())
            });

            if !processed {
                break;
            }
        }

        usb_irq_enable();
        Timer::after_ticks(1).await;
    }
}

fn rcc_72mhz_usb() -> Config {
    let mut config = Config::default();
    {
        use embassy_stm32::rcc::*;
        config.rcc.hse = Some(Hse {
            freq: Hertz(8_000_000),
            mode: HseMode::Oscillator,
        });
        config.rcc.pll = Some(Pll {
            src: PllSource::HSE,
            prediv: PllPreDiv::DIV1,
            mul: PllMul::MUL9,
        });
        config.rcc.sys = Sysclk::PLL1_P;
        config.rcc.ahb_pre = AHBPrescaler::DIV1;
        config.rcc.apb1_pre = APBPrescaler::DIV2;
        config.rcc.apb2_pre = APBPrescaler::DIV1;
    }
    config
}

#[embassy_executor::main]
async fn main(_spawner: Spawner) {
    #[allow(unused_mut)]
    let mut p = embassy_stm32::init(rcc_72mhz_usb());

    // Pulse D+ low *before* USBEN so the host drops a previous failed enum.
    // Then both USB pads must stay floating inputs: analog disconnects the
    // F1 PHY, and Drop of Flex would analog them — so forget the pins.
    let mut dp = Flex::new(p.PA12);
    dp.set_low();
    dp.set_as_input_output(Speed::Low);
    Timer::after_millis(100).await;
    dp.set_as_input(Pull::None);
    let mut dm = Flex::new(p.PA11);
    dm.set_as_input(Pull::None);
    core::mem::forget(dp);
    core::mem::forget(dm);

    #[cfg(feature = "mock-asic")]
    let asic = {
        info!("usbdisk2: mock ASIC (no FT8132 wiring)");
        let mut asic = MockAsic::from_fixtures();
        if asic::enter_i2c(&mut asic).is_ok() {
            let _ = asic::scan_i2c(&mut asic);
            let mut bflash = [0u8; asic::BFLASH_LEN];
            let _ = asic::bflash_read(&mut asic, &mut bflash);
            let mut status = [0u8; 44];
            let _ = asic::reg_read_n(&mut asic, 0x40, &mut status);
            let _ = asic::write_reg_protected(&mut asic, 0x86, 0x5A);
            let _ = asic::reg_read(&mut asic, 0x86);
            let _ = asic.exit_i2c();
        }
        asic
    };
    #[cfg(not(feature = "mock-asic"))]
    let asic = {
        info!("usbdisk2: real FT8132 on PB7/PB6");
        let _ = p.I2C1;
        let _ = p.PB6;
        let _ = p.PB7;
        FT8132::new()
    };

    // Let PLL USB 48 MHz settle before the first USB register access.
    Timer::after_millis(5).await;

    static USB_BUS: StaticCell<UsbBusAllocator<UsbBusType>> = StaticCell::new();
    let usb_bus = USB_BUS.init(UsbBus::new(UsbHw));

    static IO_BUF: StaticCell<[u8; 512]> = StaticCell::new();
    let io_buf_arr = IO_BUF.init([0; 512]);
    let io_buf = IoBuf(&mut io_buf_arr[..]);

    let scsi = Scsi::new(usb_bus, 64, 0, io_buf).unwrap();

    let usb_dev = UsbDeviceBuilder::new(usb_bus, UsbVidPid(0x1B3F, 0x8000))
        .device_class(0x00)
        .max_packet_size_0(8)
        .unwrap()
        .max_power(100)
        .unwrap()
        .usb_rev(usb_device::device::UsbRev::Usb200)
        .build();

    usb_irq_enable();
    USB_SIGNAL.signal(());
    run_usb(usb_dev, scsi, asic).await;
}
