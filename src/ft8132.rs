use esp_hal::{
    gpio::{AnyPin, Level, Pin},
    rmt::{PulseCode, TxChannelConfig, Rmt, TxChannelCreator, Channel, Tx},
    Async,
    peripherals::Peripherals,
    i2c::master::{I2c, Config},
    time::Rate,
};
use core::marker::PhantomData;
use core::result::Result;

/// Helper trait to locate a specific I2C peripheral from the Peripherals struct
pub trait I2cInstance: 'static + esp_hal::i2c::master::Instance {
    fn steal_from(p: &Peripherals) -> Self;
}

// Implement for the available I2C peripherals on ESP32-S3
impl I2cInstance for esp_hal::peripherals::I2C0<'static> {
    fn steal_from(p: &Peripherals) -> Self { unsafe { p.I2C0.clone_unchecked() } }
}
impl I2cInstance for esp_hal::peripherals::I2C1<'static> {
    fn steal_from(p: &Peripherals) -> Self { unsafe { p.I2C1.clone_unchecked() } }
}

pub enum Mode<'d> {
    Rmt(Channel<'d, Async, Tx>),
    I2c(I2c<'d, Async>),
    None, // Transient state during switching
}

pub struct FT8132<'d, I> {
    sda_num: u8,
    scl_num: u8,
    state: Mode<'d>,
    _i2c: PhantomData<I>,
}

impl<'d, I: I2cInstance> FT8132<'d, I> {
    /// Consumes the Pin and I2C/RMT peripherals to prove ownership.
    /// Starts in RMT mode (ready to knock).
    pub fn new<P1, P2, R>(
        sda: P1,
        scl: P2,
        _i2c: I,
        _rmt: R
    ) -> Self 
    where 
        P1: Pin,
        P2: Pin,
    {
        // Initial setup: Create RMT driver from stolen peripherals
        // We know we own them because they were passed in (and consumed/dropped by not storing them)
        let rmt_channel = {
            let p = unsafe { Peripherals::steal() };
            // Re-create RMT driver
            let rmt = Rmt::new(p.RMT, Rate::from_mhz(80)).unwrap().into_async();
            let pin = unsafe { AnyPin::steal(sda.number()) };
            
            // 80MHz / 8 = 10MHz. 1 tick = 100ns.
            let tx_config = TxChannelConfig::default().with_clk_divider(8);
            rmt.channel0.configure_tx(pin, tx_config).unwrap()
        };

        Self {
            sda_num: sda.number(),
            scl_num: scl.number(),
            state: Mode::Rmt(rmt_channel),
            _i2c: PhantomData,
        }
    }

    pub async fn knock(&mut self) {
        self.switch_to_rmt();

        if let Mode::Rmt(ref mut tx) = self.state {
            tx.transmit(&RMT_PULSES).await.unwrap();
        } else {
            // If in I2C mode, we need to switch back to RMT?
            // Or panic? For now, let's assume correct usage flow.
            panic!("Not in RMT mode!");
        }
    }

    pub fn switch_to_i2c(&mut self) {
        // Take ownership of the current state replacing with None
        let prev_state = core::mem::replace(&mut self.state, Mode::None);
        
        // Ensure we drop the RMT driver to release hardware resources
        drop(prev_state);

        // Create I2C driver
        let i2c = {
            let p = unsafe { Peripherals::steal() };
            let i2c_peri = I::steal_from(&p);
            let sda = unsafe { AnyPin::steal(self.sda_num) };
            let scl = unsafe { AnyPin::steal(self.scl_num) };

            I2c::new(i2c_peri, Config::default())
                .unwrap()
                .with_sda(sda)
                .with_scl(scl)
                .into_async()
        };

        self.state = Mode::I2c(i2c);
    }

    pub fn switch_to_rmt(&mut self) {
        // Take ownership of the current state replacing with None
        let prev_state = core::mem::replace(&mut self.state, Mode::None);
        
        // Ensure we drop the I2C driver to release hardware resources
        drop(prev_state);

        // Re-create RMT driver
        let rmt_channel = {
            let p = unsafe { Peripherals::steal() };
            // Re-create RMT driver
            let rmt = Rmt::new(unsafe { p.RMT.clone_unchecked() }, Rate::from_mhz(80)).unwrap().into_async();
            let pin = unsafe { AnyPin::steal(self.sda_num) };
            
            // 80MHz / 8 = 10MHz. 1 tick = 100ns.
            let tx_config = TxChannelConfig::default().with_clk_divider(8);
            rmt.channel0.configure_tx(pin, tx_config).unwrap()
        };

        self.state = Mode::Rmt(rmt_channel);
    }
    
    pub fn get_i2c(&mut self) -> &mut I2c<'d, Async> {
        self.switch_to_i2c();

        if let Mode::I2c(ref mut i2c) = self.state {
            i2c
        } else {
            panic!("Not in I2C mode!");
        }
    }

    pub async fn i2c_read_byte(&mut self, address: u8) -> Result<u8, esp_hal::i2c::master::Error> {
        self.switch_to_i2c();
        let i2c = self.get_i2c();

        let mut buffer = [0u8; 1];
        i2c.read_async(address, &mut buffer).await?;
        
        Ok(buffer[0])
    }

    pub async fn i2c_write(&mut self, address: u8, data: &[u8]) -> Result<(), esp_hal::i2c::master::Error> {
        self.switch_to_i2c();
        let i2c = self.get_i2c();

        i2c.write_async(address, data).await
    }
}

const RMT_PULSES: [PulseCode; 17] = [
    PulseCode::new(Level::Low, 80, Level::Low, 100),    // 0 * 18
    PulseCode::new(Level::High, 30, Level::Low, 10),    // 1110
    PulseCode::new(Level::High, 10, Level::Low, 30),    // 1000
    PulseCode::new(Level::High, 30, Level::Low, 10),    // 1110
    PulseCode::new(Level::High, 10, Level::Low, 30),    // 1000
    PulseCode::new(Level::High, 10, Level::Low, 30),    // 1000
    PulseCode::new(Level::High, 30, Level::Low, 10),    // 1110
    PulseCode::new(Level::High, 30, Level::Low, 10),    // 1110
    PulseCode::new(Level::High, 40, Level::Low, 180),   // 1111 + 0 * 18
    PulseCode::new(Level::High, 30, Level::Low, 10),    // 1110
    PulseCode::new(Level::High, 10, Level::Low, 30),    // 1000
    PulseCode::new(Level::High, 30, Level::Low, 10),    // 1110
    PulseCode::new(Level::High, 10, Level::Low, 30),    // 1000
    PulseCode::new(Level::High, 10, Level::Low, 30),    // 1000
    PulseCode::new(Level::High, 30, Level::Low, 10),    // 1110
    PulseCode::new(Level::High, 30, Level::Low, 10),    // 1110
    PulseCode::new(Level::High, 40, Level::Low, 0),     // 1111
];
