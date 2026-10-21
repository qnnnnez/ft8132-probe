//! FT8132 knock (bit-bang on SCL) then software I2C1 on PB6/PB7.
//!
//! Blue Pill: **PB7 = SDA**, **PB6 = SCL**. Knock holds SDA low and replays the
//! captured 1 µs bit pattern on SCL (`18×0` + `E8 E8 8E EF`, twice).
//!
//! Hardware I2C1 is not used. After a GPIO knock the F1 I2C block latches BUSY
//! if SDA is still low, and AF open-drain disconnects any GPIO pull. Both lines
//! are open-drain with the internal pull-up off; external 4.7 kΩ resistors to
//! 5 V provide the high level. PB6/PB7 are 5 V tolerant only while released.

use crate::asic::{AsicBus, I2C_ADDR};
use embassy_stm32::gpio::{Flex, Speed};
use embassy_stm32::Peripherals;

/// SYSCLK after 8 MHz HSE × 9 PLL.
const CYCLES_PER_US: u32 = 72;

/// Half-bit time for 100 kHz software I2C.
const TICK_US: u32 = 5;

/// Program-path SCL high (`write.sr` ~6 µs). Find / erase / verify stay on
/// [`TICK_US`].
const PROG_HIGH_US: u32 = 6;
/// Program-path SCL low and SDA setup (`write.sr` ~7 µs).
const PROG_LOW_US: u32 = 7;
/// SCL-low after ACK before the next byte. Capture: intra-byte low ~7 µs,
/// low after ACK ~15 µs, so this extra is ~8 µs.
const PROG_INTERBYTE_US: u32 = 8;
/// ACK SCL high (`write.sr` ~9 µs, longer than a data bit).
const PROG_ACK_HIGH_US: u32 = 12;
/// Sample ACK after the external pull-up has raised a released SDA.
const PROG_ACK_SAMPLE_US: u32 = 8;

/// Max wait for an open-drain line to be pulled high by the external resistor.
const RISE_TIMEOUT_US: u32 = 80;

/// Find-path knock payload after 18 zero bits (MSB first).
const KNOCK_BYTES: [u8; 4] = [0xE8, 0xE8, 0x8E, 0xEF];

struct Pins {
    scl: Flex<'static>,
    sda: Flex<'static>,
}

enum Mode {
    Pins(Pins),
    None,
}

pub struct FT8132 {
    state: Mode,
}

#[derive(Debug)]
pub enum Error {
    Nack,
    Bus,
}

impl FT8132 {
    pub fn new() -> Self {
        Self { state: Mode::None }
    }

    fn enable_dwt() {
        unsafe {
            let mut core = cortex_m::Peripherals::steal();
            core.DCB.enable_trace();
            core.DWT.enable_cycle_counter();
        }
    }

    /// Busy-wait `us` using CYCCNT. `asm::delay(72)` is several cycles/loop, so a
    /// "1 µs" knock bit was ~3–4 µs and the ASIC never entered I2C.
    fn delay_us(us: u32) {
        Self::delay_cycles(us.saturating_mul(CYCLES_PER_US));
    }

    fn delay_cycles(cycles: u32) {
        let start = cortex_m::peripheral::DWT::cycle_count();
        while cortex_m::peripheral::DWT::cycle_count().wrapping_sub(start) < cycles {}
    }

    /// Release SCL and hold high for `high_us`, counting rise in that window.
    fn scl_high_hold(pin: &mut Flex<'_>, high_us: u32) -> Result<(), Error> {
        let start = cortex_m::peripheral::DWT::cycle_count();
        Self::float(pin);
        Self::wait_high(pin)?;
        let target = high_us.saturating_mul(CYCLES_PER_US);
        while cortex_m::peripheral::DWT::cycle_count().wrapping_sub(start) < target {}
        Ok(())
    }

    fn steal() -> Peripherals {
        unsafe { Peripherals::steal() }
    }

    fn wait_high(pin: &Flex<'_>) -> Result<(), Error> {
        for _ in 0..RISE_TIMEOUT_US {
            if pin.is_high() {
                return Ok(());
            }
            Self::delay_us(1);
        }
        if pin.is_high() {
            Ok(())
        } else {
            Err(Error::Bus)
        }
    }

    /// Release the pin. Open-drain high leaves it Hi-Z; no internal pull-up.
    fn float(pin: &mut Flex<'_>) {
        pin.set_high();
        pin.set_as_input_output(Speed::VeryHigh);
    }

    fn scl_high(pin: &mut Flex<'_>) -> Result<(), Error> {
        Self::float(pin);
        Self::wait_high(pin)
    }

    fn drive_low(pin: &mut Flex<'_>) {
        pin.set_low();
        pin.set_as_input_output(Speed::VeryHigh);
    }

    /// Knock on SCL (SDA held low), then idle both lines open-drain.
    ///
    /// If addr `0x00` NACKs, retry with PB6/PB7 swapped (crossed SCL/SDA wiring).
    pub fn knock(&mut self) -> Result<(), Error> {
        Self::enable_dwt();
        self.knock_mapped(false);
        for _ in 0..5 {
            if self.probe_ack().is_ok() {
                return Ok(());
            }
            Self::delay_us(2_000);
        }
        self.knock_mapped(true);
        if self.probe_ack().is_ok() {
            return Ok(());
        }
        // Leave the unswapped mapping. A failed swap must not stick.
        self.knock_mapped(false);
        Err(Error::Nack)
    }

    fn knock_mapped(&mut self, swapped: bool) {
        let prev = core::mem::replace(&mut self.state, Mode::None);
        drop(prev);

        let p = Self::steal();
        let (mut scl, mut sda) = if swapped {
            (Flex::new(p.PB7), Flex::new(p.PB6))
        } else {
            (Flex::new(p.PB6), Flex::new(p.PB7))
        };

        // Open-drain. External 4.7 kΩ to 5 V makes a released SCL high.
        Self::drive_low(&mut sda);
        Self::float(&mut scl);

        // Mask USB/TIM2 so 1 µs knock bits are not stretched by IRQs.
        cortex_m::interrupt::free(|_| {
            Self::delay_us(1_000);
            Self::bitbang_knock(&mut scl);
            scl.set_high();
            Self::delay_us(TICK_US);
        });

        // STOP: SCL already high, SDA was low — release SDA so the line can rise.
        Self::float(&mut sda);
        let _ = Self::scl_high(&mut scl);
        Self::delay_us(2_000);

        self.state = Mode::Pins(Pins { scl, sda });
    }

    fn probe_ack(&mut self) -> Result<(), Error> {
        self.start()?;
        let r = self.write_byte(I2C_ADDR << 1);
        let _ = self.stop();
        r
    }

    fn bitbang_knock(scl: &mut Flex<'_>) {
        fn zeros(scl: &mut Flex<'_>, n: u32) {
            scl.set_low();
            FT8132::delay_us(n);
        }
        fn bytes(scl: &mut Flex<'_>, payload: &[u8]) {
            for &b in payload {
                for i in (0..8).rev() {
                    if (b >> i) & 1 != 0 {
                        scl.set_high();
                    } else {
                        scl.set_low();
                    }
                    FT8132::delay_us(1);
                }
            }
        }

        zeros(scl, 18);
        bytes(scl, &KNOCK_BYTES);
        zeros(scl, 18);
        bytes(scl, &KNOCK_BYTES);
    }

    fn pins(&mut self) -> Result<&mut Pins, Error> {
        if !matches!(self.state, Mode::Pins(_)) {
            return Err(Error::Bus);
        }
        match &mut self.state {
            Mode::Pins(p) => Ok(p),
            Mode::None => unreachable!(),
        }
    }

    fn start(&mut self) -> Result<(), Error> {
        let p = self.pins()?;
        Self::float(&mut p.sda);
        Self::scl_high(&mut p.scl)?;
        Self::delay_us(TICK_US);
        Self::drive_low(&mut p.sda);
        Self::delay_us(TICK_US);
        Self::drive_low(&mut p.scl);
        Self::delay_us(TICK_US);
        Ok(())
    }

    fn stop(&mut self) -> Result<(), Error> {
        let p = self.pins()?;
        Self::drive_low(&mut p.sda);
        Self::delay_us(TICK_US);
        Self::scl_high(&mut p.scl)?;
        Self::delay_us(TICK_US);
        Self::float(&mut p.sda);
        Self::delay_us(TICK_US);
        Ok(())
    }

    fn write_bit(&mut self, one: bool) -> Result<(), Error> {
        let p = self.pins()?;
        if one {
            Self::float(&mut p.sda);
        } else {
            Self::drive_low(&mut p.sda);
        }
        Self::delay_us(TICK_US);
        Self::scl_high(&mut p.scl)?;
        Self::delay_us(TICK_US);
        Self::drive_low(&mut p.scl);
        Self::delay_us(TICK_US);
        Ok(())
    }

    fn read_bit(&mut self) -> Result<bool, Error> {
        let p = self.pins()?;
        Self::float(&mut p.sda);
        Self::delay_us(TICK_US);
        Self::scl_high(&mut p.scl)?;
        let one = p.sda.is_high();
        Self::delay_us(TICK_US);
        Self::drive_low(&mut p.scl);
        Self::delay_us(TICK_US);
        Ok(one)
    }

    fn write_byte(&mut self, byte: u8) -> Result<(), Error> {
        for i in (0..8).rev() {
            self.write_bit((byte >> i) & 1 != 0)?;
        }
        if self.read_bit()? {
            Err(Error::Nack)
        } else {
            Ok(())
        }
    }

    fn read_byte(&mut self, ack: bool) -> Result<u8, Error> {
        let mut v = 0u8;
        for _ in 0..8 {
            v = (v << 1) | u8::from(self.read_bit()?);
        }
        self.write_bit(!ack)?;
        Ok(v)
    }

    fn xfer_write(&mut self, bytes: &[u8]) -> Result<(), Error> {
        self.start()?;
        if let Err(e) = self.write_byte(I2C_ADDR << 1) {
            let _ = self.stop();
            return Err(e);
        }
        for &b in bytes {
            if let Err(e) = self.write_byte(b) {
                let _ = self.stop();
                return Err(e);
            }
        }
        self.stop()
    }

    fn start_prog(&mut self) -> Result<(), Error> {
        let p = self.pins()?;
        // Released high, same electrical level as official `sub_200024C0`.
        Self::float(&mut p.sda);
        Self::float(&mut p.scl);
        Self::delay_us(PROG_HIGH_US);
        Self::drive_low(&mut p.sda);
        Self::delay_us(PROG_HIGH_US);
        Self::drive_low(&mut p.scl);
        Ok(())
    }

    fn stop_prog(&mut self) -> Result<(), Error> {
        let p = self.pins()?;
        Self::drive_low(&mut p.sda);
        Self::delay_us(PROG_LOW_US);
        Self::float(&mut p.scl);
        Self::delay_us(PROG_HIGH_US);
        Self::float(&mut p.sda);
        Self::delay_us(PROG_HIGH_US);
        Ok(())
    }

    fn write_bit_prog(&mut self, one: bool) -> Result<(), Error> {
        let p = self.pins()?;
        // Both lines open-drain. A push-pull high would hold the 5 V bus at 3.3 V.
        if one {
            Self::float(&mut p.sda);
        } else {
            Self::drive_low(&mut p.sda);
        }
        Self::delay_us(PROG_LOW_US);
        Self::float(&mut p.scl);
        Self::delay_us(PROG_HIGH_US);
        Self::drive_low(&mut p.scl);
        Ok(())
    }

    /// ACK sampled mid-high. Do not fall back to 15 µs [`Self::read_bit`].
    fn ack_bit_prog(&mut self) -> Result<bool, Error> {
        let p = self.pins()?;
        Self::float(&mut p.sda);
        Self::delay_us(PROG_LOW_US);
        Self::float(&mut p.scl);
        Self::delay_us(PROG_ACK_SAMPLE_US);
        let nack = p.sda.is_high();
        Self::delay_us(PROG_ACK_HIGH_US.saturating_sub(PROG_ACK_SAMPLE_US));
        Self::drive_low(&mut p.scl);
        // Official `sub_20002696`: SDA low after ACK (next byte or STOP).
        Self::drive_low(&mut p.sda);
        Ok(nack)
    }

    fn write_byte_prog(&mut self, byte: u8, last: bool) -> Result<(), Error> {
        for i in (0..8).rev() {
            self.write_bit_prog((byte >> i) & 1 != 0)?;
        }
        let nack = self.ack_bit_prog()?;
        if !last {
            Self::delay_us(PROG_INTERBYTE_US);
        }
        if nack {
            Err(Error::Nack)
        } else {
            Ok(())
        }
    }

    fn read_bit_prog(&mut self) -> Result<bool, Error> {
        let p = self.pins()?;
        Self::float(&mut p.sda);
        Self::delay_us(PROG_LOW_US);
        Self::scl_high_hold(&mut p.scl, PROG_HIGH_US)?;
        let one = p.sda.is_high();
        Self::drive_low(&mut p.scl);
        Ok(one)
    }

    fn read_byte_prog(&mut self, ack: bool, last: bool) -> Result<u8, Error> {
        let mut v = 0u8;
        for _ in 0..8 {
            v = (v << 1) | u8::from(self.read_bit_prog()?);
        }
        self.write_bit_prog(!ack)?;
        if !last {
            Self::delay_us(PROG_INTERBYTE_US);
        }
        Ok(v)
    }

    /// Program write, fully IRQ-masked. A USB SOF between bytes stretches the
    /// 15 µs post-ACK low (`write.sr`) and the following c1 window then
    /// replaces c2. Hardware NAK covers this ~15 ms; the host is waiting on CSW.
    fn xfer_write_prog(&mut self, bytes: &[u8]) -> Result<(), Error> {
        Self::enable_dwt();
        cortex_m::interrupt::free(|_| {
            self.start_prog()?;
            if let Err(e) = self.write_byte_prog(I2C_ADDR << 1, bytes.is_empty()) {
                let _ = self.stop_prog();
                return Err(e);
            }
            for (i, &b) in bytes.iter().enumerate() {
                if let Err(e) = self.write_byte_prog(b, i + 1 == bytes.len()) {
                    let _ = self.stop_prog();
                    return Err(e);
                }
            }
            self.stop_prog()
        })
    }

    /// Same SCL as [`Self::xfer_write_prog`] (`sub_200027FC` status / `F9`).
    fn xfer_read_prog(&mut self, buf: &mut [u8]) -> Result<(), Error> {
        if buf.is_empty() {
            return Ok(());
        }
        Self::enable_dwt();
        self.start_prog()?;
        if let Err(e) = self.write_byte_prog((I2C_ADDR << 1) | 1, false) {
            let _ = self.stop_prog();
            return Err(e);
        }
        let last = buf.len() - 1;
        for (i, slot) in buf.iter_mut().enumerate() {
            match self.read_byte_prog(i != last, i == last) {
                Ok(b) => *slot = b,
                Err(e) => {
                    let _ = self.stop_prog();
                    return Err(e);
                }
            }
        }
        self.stop_prog()
    }

    fn xfer_read(&mut self, buf: &mut [u8]) -> Result<(), Error> {
        if buf.is_empty() {
            return Ok(());
        }
        self.start()?;
        if let Err(e) = self.write_byte((I2C_ADDR << 1) | 1) {
            let _ = self.stop();
            return Err(e);
        }
        let last = buf.len() - 1;
        for (i, slot) in buf.iter_mut().enumerate() {
            match self.read_byte(i != last) {
                Ok(b) => *slot = b,
                Err(e) => {
                    let _ = self.stop();
                    return Err(e);
                }
            }
        }
        self.stop()
    }
}

impl Default for FT8132 {
    fn default() -> Self {
        Self::new()
    }
}

impl AsicBus for FT8132 {
    type Error = Error;

    fn knock(&mut self) -> Result<(), Self::Error> {
        FT8132::knock(self)
    }

    fn i2c_write(&mut self, bytes: &[u8]) -> Result<(), Self::Error> {
        if !crate::asic::hv_admit(bytes) {
            return Err(Error::Bus);
        }
        self.xfer_write(bytes)
    }

    fn i2c_write_program(&mut self, bytes: &[u8]) -> Result<(), Self::Error> {
        if !crate::asic::hv_admit(bytes) {
            return Err(Error::Bus);
        }
        self.xfer_write_prog(bytes)
    }

    fn i2c_read_program(&mut self, buf: &mut [u8]) -> Result<(), Self::Error> {
        self.xfer_read_prog(buf)
    }

    fn i2c_read(&mut self, buf: &mut [u8]) -> Result<(), Self::Error> {
        self.xfer_read(buf)
    }

    fn delay_us(&mut self, us: u32) {
        FT8132::delay_us(us);
    }

    fn clamp_bus(&mut self, low: bool) -> Result<(), Self::Error> {
        if !low {
            return self.exit_i2c();
        }
        let prev = core::mem::replace(&mut self.state, Mode::None);
        drop(prev);
        let p = Self::steal();
        let mut scl = Flex::new(p.PB6);
        let mut sda = Flex::new(p.PB7);
        Self::drive_low(&mut scl);
        Self::drive_low(&mut sda);
        self.state = Mode::Pins(Pins { scl, sda });
        Ok(())
    }

    fn exit_i2c(&mut self) -> Result<(), Self::Error> {
        if matches!(self.state, Mode::Pins(_)) {
            let _ = self.stop();
        }
        let prev = core::mem::replace(&mut self.state, Mode::None);
        drop(prev);
        Ok(())
    }
}
