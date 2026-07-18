use esp_hal::{
    gpio::{AnyPin, Level, OutputPin},
    rmt::{PulseCode, TxChannelConfig, TxChannelCreator, Rmt, Channel, Tx},
    Async,
};
use embassy_time::{Duration, Timer};

pub struct Ws2812<'d> {
    tx: Channel<'d, Async, Tx>,
    pulses: [PulseCode; 25],
}

impl<'d> Ws2812<'d> {
    pub fn new<C>(channel: C, pin: impl OutputPin + 'd) -> Self 
    where C: TxChannelCreator<'d, Async>
    {
        // 80MHz / 4 = 20MHz. 1 tick = 50ns.
        let tx_config = TxChannelConfig::default().with_clk_divider(4);
        let tx = channel.configure_tx(pin, tx_config).unwrap();
        
        Self {
            tx,
            pulses: [PulseCode::new(Level::Low, 0, Level::Low, 0); 25],
        }
    }

    pub async fn write_rgb(&mut self, r: u8, g: u8, b: u8) {
        self.fill_grb(r, g, b);
        self.tx.transmit(&self.pulses).await.unwrap();
    }

    fn fill_grb(&mut self, r: u8, g: u8, b: u8) {
        // GRB order
        let colors = [g, r, b];
        let mut idx = 0;

        // Timings for 20MHz clock (50ns tick):
        // T0H: 0.4us = 400ns = 8 ticks
        // T0L: 0.85us = 850ns = 17 ticks
        // T1H: 0.8us = 800ns = 16 ticks
        // T1L: 0.45us = 450ns = 9 ticks

        let t0h = 8;
        let t0l = 17;
        let t1h = 16;
        let t1l = 9;

        for byte in colors {
            for i in 0..8 {
                let bit = (byte >> (7 - i)) & 1;

                if bit != 0 {
                    self.pulses[idx] = PulseCode::new(Level::High, t1h, Level::Low, t1l);
                } else {
                    self.pulses[idx] = PulseCode::new(Level::High, t0h, Level::Low, t0l);
                }
                idx += 1;
            }
        }

        self.pulses[idx] = PulseCode::end_marker();
    }
}

#[embassy_executor::task]
pub async fn blink(rmt: Rmt<'static, Async>, pin: AnyPin<'static>) {
    let mut driver = Ws2812::new(rmt.channel0, pin);

    loop {
        driver.write_rgb(1, 1, 1).await;
        Timer::after(Duration::from_secs(1)).await;

        driver.write_rgb(0, 0, 0).await;
        Timer::after(Duration::from_secs(1)).await;
    }
}
