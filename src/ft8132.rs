use crate::asic::{AsicBus, I2C_ADDR};
use core::marker::PhantomData;
use esp_hal::{
    delay::Delay,
    gpio::{AnyPin, Level, Output, OutputConfig, Pin},
    i2c::master::{Config, I2c},
    peripherals::Peripherals,
    rmt::{Channel, PulseCode, Rmt, Tx, TxChannelConfig, TxChannelCreator},
    time::Rate,
    Blocking,
};

/// Helper trait to locate a specific I2C peripheral from the Peripherals struct
pub trait I2cInstance: 'static + esp_hal::i2c::master::Instance {
    fn steal_from(p: &Peripherals) -> Self;
}

impl I2cInstance for esp_hal::peripherals::I2C0<'static> {
    fn steal_from(p: &Peripherals) -> Self {
        unsafe { p.I2C0.clone_unchecked() }
    }
}
impl I2cInstance for esp_hal::peripherals::I2C1<'static> {
    fn steal_from(p: &Peripherals) -> Self {
        unsafe { p.I2C1.clone_unchecked() }
    }
}

enum Mode<'d> {
    I2c(I2c<'d, Blocking>),
    None,
}

pub struct FT8132<'d, I> {
    sda_num: u8,
    scl_num: u8,
    state: Mode<'d>,
    _i2c: PhantomData<I>,
}

#[derive(Debug)]
pub enum Error {
    I2c(esp_hal::i2c::master::Error),
    Rmt,
}

impl From<esp_hal::i2c::master::Error> for Error {
    fn from(e: esp_hal::i2c::master::Error) -> Self {
        Self::I2c(e)
    }
}

impl<'d, I: I2cInstance> FT8132<'d, I> {
    /// Consumes the Pin and I2C/RMT peripherals to prove ownership.
    pub fn new<P1, P2, R>(sda: P1, scl: P2, _i2c: I, _rmt: R) -> Self
    where
        P1: Pin,
        P2: Pin,
    {
        Self {
            sda_num: sda.number(),
            scl_num: scl.number(),
            state: Mode::None,
            _i2c: PhantomData,
        }
    }

    fn delay_ms(ms: u32) {
        Delay::new().delay_millis(ms);
    }

    /// Knock on SCL (SDA held low), then switch both pins to I2C master.
    pub fn knock(&mut self) -> Result<(), Error> {
        let prev = core::mem::replace(&mut self.state, Mode::None);
        drop(prev);

        {
            let p = unsafe { Peripherals::steal() };
            let sda = unsafe { AnyPin::steal(self.sda_num) };
            let scl = unsafe { AnyPin::steal(self.scl_num) };
            let _sda_low = Output::new(sda, Level::Low, OutputConfig::default());

            let rmt = Rmt::new(unsafe { p.RMT.clone_unchecked() }, Rate::from_mhz(80)).unwrap();
            let tx_config = TxChannelConfig::default()
                .with_clk_divider(8)
                .with_idle_output_level(Level::High)
                .with_idle_output(true);
            let tx: Channel<'_, Blocking, Tx> = rmt.channel0.configure_tx(scl, tx_config).unwrap();

            Self::delay_ms(3);
            let tx = tx.transmit(&RMT_PULSES).map_err(|_| Error::Rmt)?;
            match tx.wait() {
                Ok(_) => {}
                Err(_) => return Err(Error::Rmt),
            }
            // `_sda_low` drops here, releasing SDA.
        }

        Self::delay_ms(2);
        self.switch_to_i2c();
        Ok(())
    }

    pub fn switch_to_i2c(&mut self) {
        let prev = core::mem::replace(&mut self.state, Mode::None);
        drop(prev);

        let i2c = {
            let p = unsafe { Peripherals::steal() };
            let i2c_peri = I::steal_from(&p);
            let sda = unsafe { AnyPin::steal(self.sda_num) };
            let scl = unsafe { AnyPin::steal(self.scl_num) };
            let cfg = Config::default().with_frequency(Rate::from_khz(100));
            I2c::new(i2c_peri, cfg)
                .unwrap()
                .with_sda(sda)
                .with_scl(scl)
        };
        self.state = Mode::I2c(i2c);
    }

    fn i2c(&mut self) -> &mut I2c<'d, Blocking> {
        if !matches!(self.state, Mode::I2c(_)) {
            self.switch_to_i2c();
        }
        match &mut self.state {
            Mode::I2c(i2c) => i2c,
            _ => unreachable!(),
        }
    }
}

impl<I: I2cInstance> AsicBus for FT8132<'_, I> {
    type Error = Error;

    fn knock(&mut self) -> Result<(), Self::Error> {
        FT8132::knock(self)
    }

    fn i2c_write(&mut self, bytes: &[u8]) -> Result<(), Self::Error> {
        self.i2c().write(I2C_ADDR, bytes).map_err(Into::into)
    }

    fn i2c_read(&mut self, buf: &mut [u8]) -> Result<(), Self::Error> {
        self.i2c().read(I2C_ADDR, buf).map_err(Into::into)
    }

    fn exit_i2c(&mut self) -> Result<(), Self::Error> {
        let prev = core::mem::replace(&mut self.state, Mode::None);
        drop(prev);
        Ok(())
    }
}

const RMT_PULSES: [PulseCode; 17] = [
    PulseCode::new(Level::Low, 80, Level::Low, 100), // 0 * 18
    PulseCode::new(Level::High, 30, Level::Low, 10), // 1110
    PulseCode::new(Level::High, 10, Level::Low, 30), // 1000
    PulseCode::new(Level::High, 30, Level::Low, 10), // 1110
    PulseCode::new(Level::High, 10, Level::Low, 30), // 1000
    PulseCode::new(Level::High, 10, Level::Low, 30), // 1000
    PulseCode::new(Level::High, 30, Level::Low, 10), // 1110
    PulseCode::new(Level::High, 30, Level::Low, 10), // 1110
    PulseCode::new(Level::High, 40, Level::Low, 180), // 1111 + 0 * 18
    PulseCode::new(Level::High, 30, Level::Low, 10), // 1110
    PulseCode::new(Level::High, 10, Level::Low, 30), // 1000
    PulseCode::new(Level::High, 30, Level::Low, 10), // 1110
    PulseCode::new(Level::High, 10, Level::Low, 30), // 1000
    PulseCode::new(Level::High, 10, Level::Low, 30), // 1000
    PulseCode::new(Level::High, 30, Level::Low, 10), // 1110
    PulseCode::new(Level::High, 30, Level::Low, 10), // 1110
    PulseCode::new(Level::High, 40, Level::Low, 0),  // 1111 (end marker)
];
