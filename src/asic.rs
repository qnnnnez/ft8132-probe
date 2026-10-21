//! HAL-free FT8132 I2C protocol helpers.

use core::fmt::Debug;

/// 7-bit I2C address used after knock (default / Find path).
pub const I2C_ADDR: u8 = 0x00;

/// `reg 0x01` value the official Find path requires.
pub const CHIP_ID: u8 = 0x02;

/// EEPROM / bflash window used by Find (`F6 00 00` then 128-byte read).
pub const BFLASH_LEN: usize = 128;

/// GUI eFuse bit array (`MAX_EFUSE_NUM`): 128 packed bytes = 1024 bits.
pub const MTP_BITS: usize = BFLASH_LEN * 8;

/// abflash / bkflash bit counts (opt 5). cflash is 748 bits from offset 128 (opt 6).
pub const BMTP_BITS: usize = 110;
pub const CMTP_BITS: usize = 748;
pub const CMTP_OFF: u16 = 128;
pub const BKMTP_OFF: u16 = 896;

/// Official verify clocks at most 128 one-bit-per-byte payloads per `F6`.
const MTP_RD_CHUNK: usize = 128;
/// Program `F6` payload cap (same 128 as verify).
const MTP_WR_CHUNK: usize = 128;

/// After `F6 00 00 0C` / `F9` (erase.sr / write.sr). USB task waits this
/// without blocking `poll_command`.
pub const ERASE_PHASE1_US: u32 = 80_000;
/// After `F6 00 00 0E` / `F9`.
pub const ERASE_PHASE2_US: u32 = 250_000;

/// Bus used by protocol helpers: real I2C+RMT or the capture-seeded mock.
pub trait AsicBus {
    type Error: Debug;

    /// Knock on SCL with SDA held low, then leave the bus in I2C mode.
    fn knock(&mut self) -> Result<(), Self::Error>;

    /// I2C write to [`I2C_ADDR`] (`START addrW payload STOP`).
    fn i2c_write(&mut self, bytes: &[u8]) -> Result<(), Self::Error>;

    /// Program-path write (`F6`, `FF C1`, select). Real bus matches
    /// `write.sr` (~13 µs/bit). Default is slow I2C (mock). Do not IRQ-mask
    /// a whole window.
    fn i2c_write_program(&mut self, bytes: &[u8]) -> Result<(), Self::Error> {
        self.i2c_write(bytes)
    }

    /// I2C read from [`I2C_ADDR`] (`START addrR data STOP`).
    fn i2c_read(&mut self, buf: &mut [u8]) -> Result<(), Self::Error>;

    /// Program-path read (select status, `F9`). Same SCL as
    /// [`Self::i2c_write_program`]. Default is slow I2C (mock).
    fn i2c_read_program(&mut self, buf: &mut [u8]) -> Result<(), Self::Error> {
        self.i2c_read(buf)
    }

    /// Optional leave-I2C / pin release. Default is a no-op.
    fn exit_i2c(&mut self) -> Result<(), Self::Error> {
        Ok(())
    }

    /// Hold both lines low so a VIN cycle is not back-powered through the
    /// pull-ups, or release them. Default is a no-op (mock).
    fn clamp_bus(&mut self, _low: bool) -> Result<(), Self::Error> {
        Ok(())
    }

    /// Busy-wait. Default is a no-op (mock). Real bus uses DWT.
    fn delay_us(&mut self, _us: u32) {}
}

/// Write `reg, val` as one I2C payload.
pub fn reg_write<B: AsicBus>(bus: &mut B, reg: u8, val: u8) -> Result<(), B::Error> {
    bus.i2c_write(&[reg, val])
}

/// Write register address, STOP, then read `buf.len()` bytes.
pub fn reg_read_n<B: AsicBus>(bus: &mut B, reg: u8, buf: &mut [u8]) -> Result<(), B::Error> {
    bus.i2c_write(&[reg])?;
    bus.i2c_read(buf)
}

/// Single-byte [`reg_read_n`].
pub fn reg_read<B: AsicBus>(bus: &mut B, reg: u8) -> Result<u8, B::Error> {
    let mut b = [0u8; 1];
    reg_read_n(bus, reg, &mut b)?;
    Ok(b[0])
}

/// Knock and confirm chip ID (`0x01 == 0x02`).
pub fn enter_i2c<B: AsicBus>(bus: &mut B) -> Result<u8, B::Error> {
    bus.knock()?;
    let id = reg_read(bus, 0x01)?;
    Ok(id)
}

pub fn mtp_select<B: AsicBus>(bus: &mut B) -> Result<(), B::Error> {
    bus.i2c_write(&[0xFF, 0xC1, 0x56])?;
    bus.delay_us(4_000);
    let mut st = [0u8];
    for _ in 0..20 {
        bus.i2c_write(&[0xFF])?;
        bus.i2c_read(&mut st)?;
        if st[0] == 0x03 {
            break;
        }
        bus.delay_us(1_000);
    }
    bus.delay_us(4_000);
    Ok(())
}

/// End an MTP program CDB (`sub_2000094E(0)`: lone `FF C1`).
pub fn mtp_deselect<B: AsicBus>(bus: &mut B) -> Result<(), B::Error> {
    bus.i2c_write_program(&[0xFF, 0xC1])
}

/// Program select (`sub_2000094E(1)`): `FF C1 56` / `FF` / one status byte.
/// Same SCL as the `F6` windows. Find/verify keep [`mtp_select`].
/// Does not fail on a status other than `0x03`.
pub fn mtp_select_prog<B: AsicBus>(bus: &mut B) -> Result<u8, B::Error> {
    bus.i2c_write_program(&[0xFF, 0xC1, 0x56])?;
    bus.i2c_write_program(&[0xFF])?;
    let mut st = [0u8];
    bus.i2c_read_program(&mut st)?;
    Ok(st[0])
}

fn mtp_unlock<B: AsicBus>(bus: &mut B) -> Result<(), B::Error> {
    bus.i2c_write_program(&[0xFE, 0xD2, 0xF2])
}

pub fn mtp_lock<B: AsicBus>(bus: &mut B) -> Result<(), B::Error> {
    bus.i2c_write_program(&[0xFE, 0xD2])
}

/// `sub_2000286E`: write `F9`, read one status byte, retry while bit 2 is set.
fn f9_commit<B: AsicBus>(bus: &mut B) -> Result<(), B::Error> {
    let mut st = [0u8];
    for _ in 0..3 {
        bus.i2c_write_program(&[0xF9])?;
        let _ = bus.i2c_read_program(&mut st);
        if st[0] & 4 == 0 {
            break;
        }
    }
    Ok(())
}

/// Find-path EEPROM dump: `FF C1 56` / `FF` status, then `F6 00 00` and a bulk read.
pub fn bflash_read<B: AsicBus>(bus: &mut B, buf: &mut [u8]) -> Result<(), B::Error> {
    mtp_select(bus)?;
    bus.i2c_write(&[0xF6, 0x00, 0x00])?;
    bus.delay_us(4_000);
    bus.i2c_read(buf)?;
    hv_observe_bflash(buf);
    Ok(())
}

/// Live Find header. All-zero or all-ones means the bus is wedged.
const BFLASH_HDR: [u8; 4] = [0xff, 0xff, 0x00, 0xff];

struct HvState {
    /// Chip id or `bflash` failed. Program clocks stay off.
    wedged: bool,
    /// Health check passed since the last knock.
    hv_ok: bool,
    /// Erase finished in this knock session, and no `FF C1` has closed it.
    erase_armed: bool,
    /// Program slices already closed with `FF C1` since that erase.
    slices: u8,
    /// This command sent `FF C1`. The next command must not keep programming.
    closed: bool,
}

struct Hv(core::cell::UnsafeCell<HvState>);
unsafe impl Sync for Hv {}

static HV: Hv = Hv(core::cell::UnsafeCell::new(HvState {
    wedged: false,
    hv_ok: false,
    erase_armed: false,
    slices: 0,
    closed: false,
}));

fn hv() -> &'static mut HvState {
    unsafe { &mut *HV.0.get() }
}

fn hv_note_bad() {
    let g = hv();
    g.wedged = true;
    g.hv_ok = false;
    g.erase_armed = false;
}

/// A knock starts a new session. The previous erase can no longer program.
pub fn hv_on_knock() {
    let g = hv();
    g.hv_ok = false;
    g.erase_armed = false;
    g.slices = 0;
    g.closed = false;
}

/// Call at the start of each SCSI command. A `FF C1` from the previous
/// command already ended the program session.
pub fn hv_on_command() {
    let g = hv();
    if g.closed {
        g.hv_ok = false;
        g.erase_armed = false;
        g.slices = 0;
        g.closed = false;
    }
}

pub fn hv_on_erase_done() {
    let g = hv();
    if g.hv_ok && !g.wedged {
        g.erase_armed = true;
        g.slices = 0;
        g.closed = false;
    }
}

pub fn hv_observe_id(id: u8) {
    if id != 0x02 {
        hv_note_bad();
    }
}

pub fn hv_observe_bflash(buf: &[u8]) {
    if buf.len() >= 4 && buf[..4] != BFLASH_HDR {
        hv_note_bad();
    }
}

/// Chip id `0x02` and `bflash` `ffff00ff` before any program clock.
pub fn hv_allow_erase<B: AsicBus>(bus: &mut B) -> Result<bool, B::Error> {
    let id = reg_read(bus, 0x01)?;
    hv_observe_id(id);
    let mut hdr = [0u8; 4];
    bflash_read(bus, &mut hdr)?;
    if id == 0x02 && hdr == BFLASH_HDR {
        let g = hv();
        g.wedged = false;
        g.hv_ok = true;
        Ok(true)
    } else {
        hv_note_bad();
        Ok(false)
    }
}

fn hits_c1_commit(addr: u16, n: usize) -> bool {
    let end = addr as usize + n;
    (addr as usize) < end && (addr as usize..end).contains(&384)
}

fn crosses_page(addr: u16, n: usize) -> bool {
    let end = addr as usize + n;
    let page_end = (addr as usize / 128 + 1) * 128;
    end > page_end
}

/// `false` means do not put this buffer on the wire.
pub fn hv_admit(bytes: &[u8]) -> bool {
    let Some(&op) = bytes.first() else {
        return true;
    };
    // Address-only `F6`, select (`FF C1 56`), and a one-byte status read
    // are ordinary I2C. Find still works when programming is locked.
    if op == 0xF6 && bytes.len() <= 3 || bytes == [0xFF, 0xC1, 0x56] || bytes == [0xFF] {
        return true;
    }
    if !matches!(op, 0xF6 | 0xF9 | 0xFE | 0xFF | 0xF4) {
        return true;
    }
    let g = hv();
    if g.wedged || !g.hv_ok {
        return false;
    }
    if bytes == [0xFF, 0xC1] {
        g.slices = g.slices.saturating_add(1);
        g.closed = true;
        return true;
    }
    if bytes.len() >= 4 && bytes[0] == 0xF6 {
        let n = bytes.len() - 3;
        let erase_tag = n == 1 && matches!(bytes[3], 0x0C | 0x0E);
        let bits = !erase_tag && bytes[3..].iter().all(|b| matches!(*b, 0x02 | 0x03));
        if bits {
            let addr = u16::from_be_bytes([bytes[1], bytes[2]]);
            if !g.erase_armed || g.slices > 0 || hits_c1_commit(addr, n) || crosses_page(addr, n)
            {
                return false;
            }
        }
    }
    true
}

/// Unpack LSB-first bits from a packed 128-byte F6 window into 0/1 SCSI bytes.
pub fn unpack_mtp_bits(packed: &[u8], bit_addr: u16, bits: &mut [u8]) {
    for (i, slot) in bits.iter_mut().enumerate() {
        let bit = bit_addr as usize + i;
        let byte = packed.get(bit / 8).copied().unwrap_or(0);
        *slot = (byte >> (bit % 8)) & 1;
    }
}

/// Pack one SCSI 0/1 bit into the 128-byte LSB-first image.
pub fn pack_mtp_bit(packed: &mut [u8; BFLASH_LEN], bit_addr: u16, one: bool) {
    let bit = bit_addr as usize;
    if bit >= MTP_BITS {
        return;
    }
    let mask = 1u8 << (bit % 8);
    if one {
        packed[bit / 8] |= mask;
    } else {
        packed[bit / 8] &= !mask;
    }
}

/// LSB-first bit from the packed 1024-bit image.
pub fn packed_bit(packed: &[u8; BFLASH_LEN], bit_addr: u16) -> bool {
    let bit = bit_addr as usize;
    if bit >= MTP_BITS {
        return false;
    }
    packed[bit / 8] & (1u8 << (bit % 8)) != 0
}

/// Next run of consecutive 1s at or after `start` (max [`MTP_RD_CHUNK`]).
pub fn next_one_run(packed: &[u8; BFLASH_LEN], start: u16) -> Option<(u16, usize)> {
    let mut b = start as usize;
    while b < MTP_BITS {
        if packed_bit(packed, b as u16) {
            let mut n = 1;
            while b + n < MTP_BITS && packed_bit(packed, (b + n) as u16) && n < MTP_RD_CHUNK {
                n += 1;
            }
            return Some((b as u16, n));
        }
        b += 1;
    }
    None
}

/// Idle after a program `F6` STOP before the next `F6` in the same slice
/// (`write.sr` ~4.3–4.4 ms). Official fills this with UART debug prints.
pub const PROGRAM_SETTLE_US: u32 = 4_400;
/// After `FF`/`03` (or unlock) before the first `F6` of a slice (~1.7 ms).
pub const PROG_AFTER_SELECT_US: u32 = 1_700;
/// After the last `F6` of a slice before `FF C1` (~1.7 ms).
pub const PROG_AFTER_WINDOW_US: u32 = 1_700;
/// After a slice `FF C1` before the next short select (`write.sr` ~1.8 ms).
pub const PROG_BETWEEN_SLICES_US: u32 = 1_825;
pub const CMTP_HALF: usize = CMTP_BITS / 2;

/// bkflash: one 110-byte `F6` at 896 (`write.sr`).
pub const MTP_BK_WINDOWS: [(u16, usize); 1] = [(BKMTP_OFF, BMTP_BITS)];
/// c1: 128+128+118 at 128 / 256 / 384.
pub const MTP_C1_WINDOWS: [(u16, usize); 3] = [
    (CMTP_OFF, MTP_WR_CHUNK),
    (256, MTP_WR_CHUNK),
    (384, 118),
];
/// c2: 128+128+118 at 502 / 630 / 758. Official CDB windows (`F6 01 F6`).
pub const MTP_C2_WINDOWS: [(u16, usize); 3] = [
    (502, MTP_WR_CHUNK),
    (630, MTP_WR_CHUNK),
    (758, 118),
];
/// This bit-bang latches c2 at page starts. SCSI c2 still starts at 502;
/// bits 502–511 are unused in GenCode, so `c2[10..]` maps onto these.
pub const MTP_C2_512_WINDOWS: [(u16, usize); 3] = [
    (512, MTP_WR_CHUNK),
    (640, MTP_WR_CHUNK),
    (768, 108),
];

/// Official program windows for a GUI/Linux MTP write CDB, or `None`.
pub fn mtp_prog_windows(addr: u16) -> Option<&'static [(u16, usize)]> {
    match addr {
        BKMTP_OFF => Some(&MTP_BK_WINDOWS),
        CMTP_OFF => Some(&MTP_C1_WINDOWS),
        502 => Some(&MTP_C2_WINDOWS),
        _ => None,
    }
}

/// bkflash is the only program slice that uses `FE D2 F2` / `FE D2`.
pub fn mtp_prog_unlocks(addr: u16) -> bool {
    addr == BKMTP_OFF
}

/// GUI Verify / BMTP+CMTP read: `F6` bit address, then one `00`/`FF` I2C byte per bit.
///
/// Official verify clocks at most 128 bytes per `F6` (128+128+118 for a 374-bit
/// slice). A single 374-byte read after one `F6` does not match the capture.
pub fn mtp_read_bits<B: AsicBus>(
    bus: &mut B,
    mut bit_addr: u16,
    bits: &mut [u8],
) -> Result<(), B::Error> {
    let _ = mtp_select_prog(bus)?;
    let mut off = 0;
    while off < bits.len() {
        let n = bits.len() - off;
        let n = if n > MTP_RD_CHUNK { MTP_RD_CHUNK } else { n };
        let hdr = [0xF6, (bit_addr >> 8) as u8, bit_addr as u8];
        bus.i2c_write_program(&hdr)?;
        bus.delay_us(4_000);
        bus.i2c_read_program(&mut bits[off..off + n])?;
        // First F6 after enter/select is often an empty window (GUI Verify CRC
        // 0xF9BE). Retry once only when this chunk is all zero so a slice that
        // already has 1s is not replaced by the following empty window.
        if bits[off..off + n].iter().all(|b| b & 1 == 0) {
            bus.i2c_write_program(&hdr)?;
            bus.delay_us(4_000);
            bus.i2c_read_program(&mut bits[off..off + n])?;
        }
        off += n;
        bit_addr = bit_addr.wrapping_add(n as u16);
    }
    for b in bits.iter_mut() {
        *b &= 1;
    }
    Ok(())
}

/// Start chip erase (`sub_20002AF0`): short select, unlock, `F6 00 00 0C`.
/// Caller waits [`ERASE_PHASE1_US`].
pub fn mtp_erase_start<B: AsicBus>(bus: &mut B) -> Result<(), B::Error> {
    let _ = mtp_select_prog(bus)?;
    mtp_unlock(bus)?;
    bus.i2c_write_program(&[0xF6, 0x00, 0x00, 0x0C])
}

/// After phase-1 wait: `F9`, then `F6 00 00 0E`. Caller waits [`ERASE_PHASE2_US`].
pub fn mtp_erase_phase2<B: AsicBus>(bus: &mut B) -> Result<(), B::Error> {
    f9_commit(bus)?;
    bus.i2c_write_program(&[0xF6, 0x00, 0x00, 0x0E])
}

/// After phase-2 wait: `F9`, lock. Do not `FF C1` here: that close plus a USB
/// hole wiped this DUT (`0xf9be`). Official still closes; host F4 is enough.
pub fn mtp_erase_finish<B: AsicBus>(bus: &mut B) -> Result<(), B::Error> {
    f9_commit(bus)?;
    mtp_lock(bus)
}

/// GUI Erase (`FD 20` acc=5 opt=40): blocking helper (mock / tests).
pub fn mtp_erase<B: AsicBus>(bus: &mut B) -> Result<(), B::Error> {
    mtp_erase_start(bus)?;
    bus.delay_us(ERASE_PHASE1_US);
    mtp_erase_phase2(bus)?;
    bus.delay_us(ERASE_PHASE2_US);
    mtp_erase_finish(bus)
}

/// One SCSI slice's bytes for a program `F6` window.
pub fn mtp_window_bits(bits: &[u8], slice_base: u16, win_addr: u16, win_n: usize) -> &[u8] {
    let off = win_addr.wrapping_sub(slice_base) as usize;
    if off >= bits.len() {
        return &[];
    }
    let n = win_n.min(bits.len() - off);
    &bits[off..off + n]
}

/// Write the windows of `bits` at `windows`, waiting [`PROGRAM_SETTLE_US`]
/// between them (not after the last).
fn mtp_write_windows<B: AsicBus>(
    bus: &mut B,
    slice_base: u16,
    bits: &[u8],
    windows: &[(u16, usize)],
) -> Result<(), B::Error> {
    for (i, &(win_addr, win_n)) in windows.iter().enumerate() {
        mtp_write_window(bus, win_addr, mtp_window_bits(bits, slice_base, win_addr, win_n))?;
        if i + 1 < windows.len() {
            bus.delay_us(PROGRAM_SETTLE_US);
        }
    }
    Ok(())
}

/// Official program window: `F6` + bit address + up to 128 `02`/`03` bytes,
/// then STOP. Slow I2C (`write.sr` ~77 kHz). No echo.
pub fn mtp_write_window<B: AsicBus>(
    bus: &mut B,
    bit_addr: u16,
    bits: &[u8],
) -> Result<(), B::Error> {
    let n = if bits.len() > MTP_WR_CHUNK {
        MTP_WR_CHUNK
    } else {
        bits.len()
    };
    if n == 0 {
        return Ok(());
    }
    let mut pkt = [0u8; 3 + MTP_WR_CHUNK];
    pkt[0] = 0xF6;
    pkt[1] = (bit_addr >> 8) as u8;
    pkt[2] = bit_addr as u8;
    for (i, &v) in bits[..n].iter().enumerate() {
        pkt[3 + i] = 0x02 | (v & 1);
    }
    bus.i2c_write_program(&pkt[..3 + n])
}

/// One `FD 2A` program CDB (`sub_20000C98`): short select, this slice’s
/// windows, deselect. Unlock only at 896. No extra `FF C1` before 502.
pub fn mtp_write_bits<B: AsicBus>(
    bus: &mut B,
    bit_addr: u16,
    bits: &[u8],
) -> Result<(), B::Error> {
    let _ = mtp_select_prog(bus)?;
    if mtp_prog_unlocks(bit_addr) {
        mtp_unlock(bus)?;
    }
    bus.delay_us(PROG_AFTER_SELECT_US);
    let windows = mtp_prog_windows(bit_addr).unwrap_or(&[]);
    if windows.is_empty() {
        mtp_write_window(bus, bit_addr, bits)?;
    } else {
        mtp_write_windows(bus, bit_addr, bits, windows)?;
    }
    if mtp_prog_unlocks(bit_addr) {
        mtp_lock(bus)?;
    }
    bus.delay_us(PROG_AFTER_WINDOW_US);
    mtp_deselect(bus)
}

/// One slice after erase: page-aligned c2 at 512/640/768.
/// The frame at bit 384, a second slice, and a page-crossing frame are
/// refused by [`hv_admit`].
pub fn mtp_program_image<B: AsicBus>(
    bus: &mut B,
    _bk: &[u8],
    _c1: &[u8],
    c2: &[u8],
) -> Result<(), B::Error> {
    let _ = mtp_select_prog(bus)?;
    bus.delay_us(PROG_AFTER_SELECT_US);
    let from512 = if c2.len() > 10 { &c2[10..] } else { &[] };
    mtp_write_windows(bus, 512, from512, &MTP_C2_512_WINDOWS)?;
    bus.delay_us(PROG_AFTER_WINDOW_US);
    mtp_deselect(bus)
}

/// Lab dialect ids for `FE 20`. The 1024-byte 0/1 image is already in SRAM.
/// `0` c2 at 512/640/768. `1` official 502/630/758. `2` ten bits at 502.
/// `3` full c1, gap, c2-512. `4` c2-512, gap, full c1.
/// `5` c1 at 128/256 only, gap, c2-512. `6` ten bits at 502 then c2-512, one select.
pub const LAB_C2_512: u8 = 0;
pub const LAB_C2_502: u8 = 1;
pub const LAB_C2_502_10: u8 = 2;
pub const LAB_C1_THEN_C2: u8 = 3;
pub const LAB_C2_THEN_C1: u8 = 4;
pub const LAB_C1_PARTIAL_THEN_C2: u8 = 5;
pub const LAB_C2_BRIDGE: u8 = 6;

fn lab_slice<B: AsicBus>(
    bus: &mut B,
    image: &[u8],
    windows: &[(u16, usize)],
) -> Result<(), B::Error> {
    let _ = mtp_select_prog(bus)?;
    bus.delay_us(PROG_AFTER_SELECT_US);
    mtp_write_windows(bus, 0, image, windows)?;
    bus.delay_us(PROG_AFTER_WINDOW_US);
    mtp_deselect(bus)
}

/// Program `image` from SRAM. `gap_us` is the idle between slices (`0` → [`PROG_BETWEEN_SLICES_US`]).
/// Returns `Ok(false)` for an unknown dialect.
pub fn lab_program<B: AsicBus>(
    bus: &mut B,
    image: &[u8],
    op: u8,
    gap_us: u32,
) -> Result<bool, B::Error> {
    if image.len() < MTP_BITS {
        return Ok(false);
    }
    let gap = if gap_us == 0 {
        PROG_BETWEEN_SLICES_US
    } else {
        gap_us
    };
    let c1_all = &MTP_C1_WINDOWS[..];
    let c1_partial = &MTP_C1_WINDOWS[..2];
    match op {
        LAB_C2_512 => {
            let _ = mtp_select_prog(bus)?;
            bus.delay_us(PROG_AFTER_SELECT_US);
            mtp_write_windows(bus, 512, &image[512..876], &MTP_C2_512_WINDOWS)?;
            bus.delay_us(PROG_AFTER_WINDOW_US);
            mtp_deselect(bus)?;
        }
        LAB_C2_502 => lab_slice(bus, image, &MTP_C2_WINDOWS)?,
        LAB_C2_502_10 => lab_slice(bus, image, &[(502, 10)])?,
        LAB_C1_THEN_C2 => {
            lab_slice(bus, image, c1_all)?;
            bus.delay_us(gap);
            lab_slice(bus, image, &MTP_C2_512_WINDOWS)?;
        }
        LAB_C2_THEN_C1 => {
            lab_slice(bus, image, &MTP_C2_512_WINDOWS)?;
            bus.delay_us(gap);
            lab_slice(bus, image, c1_all)?;
        }
        LAB_C1_PARTIAL_THEN_C2 => {
            lab_slice(bus, image, c1_partial)?;
            bus.delay_us(gap);
            lab_slice(bus, image, &MTP_C2_512_WINDOWS)?;
        }
        LAB_C2_BRIDGE => lab_slice(
            bus,
            image,
            &[(502, 10), (512, MTP_WR_CHUNK), (640, MTP_WR_CHUNK), (768, 108)],
        )?,
        _ => return Ok(false),
    }
    Ok(true)
}

/// Config / efuse registers that use the `0x04=0x41` (+ optional `FE`) handshake.
pub fn needs_handshake(reg: u8) -> bool {
    matches!(reg, 0x02 | 0x03 | 0x06 | 0x86..=0xB9 | 0xF2)
}

fn needs_fe_wrap(reg: u8) -> bool {
    matches!(reg, 0x86 | 0x87 | 0xF2)
}

/// Config-register write matching the captured `0x04=0x41` / `FE D2 F2` handshake.
pub fn write_reg_protected<B: AsicBus>(bus: &mut B, reg: u8, val: u8) -> Result<(), B::Error> {
    if needs_handshake(reg) {
        let _ = reg_read(bus, 0x04);
        reg_write(bus, 0x04, 0x41)?;
        if needs_fe_wrap(reg) {
            bus.i2c_write(&[0xFE, 0xD2, 0xF2])?;
        }
    }
    reg_write(bus, reg, val)?;
    if needs_fe_wrap(reg) {
        bus.i2c_write(&[0xFE, 0xD2])?;
    }
    Ok(())
}

/// Slave ID from `0x88` plus comm status (0 = OK).
pub fn scan_i2c<B: AsicBus>(bus: &mut B) -> Result<(u8, u8), B::Error> {
    match (reg_read(bus, 0x00), reg_read(bus, 0x88)) {
        (Ok(_), Ok(id)) => Ok((id, 0)),
        _ => Ok((0, 1)),
    }
}
