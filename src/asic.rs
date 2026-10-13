//! HAL-free FT8132 I2C protocol helpers.

use core::fmt::Debug;

/// 7-bit I2C address used after knock (default / Find path).
pub const I2C_ADDR: u8 = 0x00;

/// `reg 0x01` value the official Find path requires.
pub const CHIP_ID: u8 = 0x02;

/// EEPROM / bflash window used by Find (`F6 00 00` then 128-byte read).
pub const BFLASH_LEN: usize = 128;

/// Bus used by protocol helpers: real I2C+RMT or the capture-seeded mock.
pub trait AsicBus {
    type Error: Debug;

    /// Knock on SCL with SDA held low, then leave the bus in I2C mode.
    fn knock(&mut self) -> Result<(), Self::Error>;

    /// I2C write to [`I2C_ADDR`] (`START addrW payload STOP`).
    fn i2c_write(&mut self, bytes: &[u8]) -> Result<(), Self::Error>;

    /// I2C read from [`I2C_ADDR`] (`START addrR data STOP`).
    fn i2c_read(&mut self, buf: &mut [u8]) -> Result<(), Self::Error>;

    /// Optional leave-I2C / pin release. Default is a no-op.
    fn exit_i2c(&mut self) -> Result<(), Self::Error> {
        Ok(())
    }
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

/// Find-path EEPROM dump: `FF C1 56` / `FF` status, then `F6 00 00` and a bulk read.
pub fn bflash_read<B: AsicBus>(bus: &mut B, buf: &mut [u8]) -> Result<(), B::Error> {
    bus.i2c_write(&[0xFF, 0xC1, 0x56])?;
    let mut st = [0u8];
    bus.i2c_write(&[0xFF])?;
    bus.i2c_read(&mut st)?;
    bus.i2c_write(&[0xF6, 0x00, 0x00])?;
    bus.i2c_read(buf)
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
