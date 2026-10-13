//! Capture-seeded FT8132 stand-in (no `esp-hal`).

use crate::asic::{AsicBus, BFLASH_LEN};

const FIND_REGS: &[u8] = include_bytes!("../data/mock/find_regs.bin");
const FIND_BFLASH: &[u8] = include_bytes!("../data/mock/find_bflash.bin");
const DEBUG_STATUS: &[u8] = include_bytes!("../data/mock/debug_status.bin");
const FF_STATUS: &[u8] = include_bytes!("../data/mock/ff_status.bin");

/// In-firmware I2C slave seeded from `data/pulseview` captures.
pub struct MockAsic {
    regs: [u8; 256],
    bflash: [u8; BFLASH_LEN],
    ptr: u8,
    entered: bool,
    #[allow(dead_code)]
    unlocked: bool,
    pending_f6: bool,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum MockError {
    NotEntered,
    EmptyWrite,
}

impl Default for MockAsic {
    fn default() -> Self {
        Self::from_fixtures()
    }
}

impl MockAsic {
    pub fn from_fixtures() -> Self {
        let mut regs = [0u8; 256];
        let n = FIND_REGS.len().min(256);
        regs[..n].copy_from_slice(&FIND_REGS[..n]);
        let st = DEBUG_STATUS.len().min(44);
        regs[0x40..0x40 + st].copy_from_slice(&DEBUG_STATUS[..st]);

        let mut bflash = [0u8; BFLASH_LEN];
        let bn = FIND_BFLASH.len().min(BFLASH_LEN);
        bflash[..bn].copy_from_slice(&FIND_BFLASH[..bn]);

        Self {
            regs,
            bflash,
            ptr: 0,
            entered: false,
            unlocked: false,
            pending_f6: false,
        }
    }

    fn require_entered(&self) -> Result<(), MockError> {
        if self.entered {
            Ok(())
        } else {
            Err(MockError::NotEntered)
        }
    }
}

impl AsicBus for MockAsic {
    type Error = MockError;

    fn knock(&mut self) -> Result<(), Self::Error> {
        self.entered = true;
        self.pending_f6 = false;
        self.unlocked = false;
        Ok(())
    }

    fn i2c_write(&mut self, bytes: &[u8]) -> Result<(), Self::Error> {
        self.require_entered()?;
        if bytes.is_empty() {
            return Err(MockError::EmptyWrite);
        }
        self.ptr = bytes[0];
        let data = &bytes[1..];

        if bytes[0] == 0xF6 && data == [0x00, 0x00].as_slice() {
            self.pending_f6 = true;
            return Ok(());
        }
        if bytes[0] == 0xFE && data == [0xD2, 0xF2].as_slice() {
            self.unlocked = true;
            return Ok(());
        }
        if bytes[0] == 0xFE && data == [0xD2].as_slice() {
            self.unlocked = false;
            return Ok(());
        }
        if bytes[0] == 0xFF {
            // `FF C1 56` / write `FF` — next read of 0xFF returns capture status.
            return Ok(());
        }

        for (i, &v) in data.iter().enumerate() {
            let addr = self.ptr.wrapping_add(i as u8);
            self.regs[addr as usize] = v;
        }
        if !data.is_empty() {
            self.ptr = self.ptr.wrapping_add(data.len() as u8);
        }
        Ok(())
    }

    fn i2c_read(&mut self, buf: &mut [u8]) -> Result<(), Self::Error> {
        self.require_entered()?;
        if self.pending_f6 {
            let n = buf.len().min(self.bflash.len());
            buf[..n].copy_from_slice(&self.bflash[..n]);
            if n < buf.len() {
                buf[n..].fill(0);
            }
            self.pending_f6 = false;
            return Ok(());
        }
        for (i, slot) in buf.iter_mut().enumerate() {
            let addr = self.ptr.wrapping_add(i as u8);
            *slot = if addr == 0xFF {
                FF_STATUS.first().copied().unwrap_or(0x03)
            } else {
                self.regs[addr as usize]
            };
        }
        if !buf.is_empty() {
            self.ptr = self.ptr.wrapping_add(buf.len() as u8);
        }
        Ok(())
    }

    fn exit_i2c(&mut self) -> Result<(), Self::Error> {
        self.entered = false;
        Ok(())
    }
}
