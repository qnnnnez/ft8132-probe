#![no_std]
#![no_main]
#![deny(
    clippy::mem_forget,
    reason = "mem::forget is generally not safe to do with esp_hal types, especially those \
    holding buffers for the duration of a data transfer."
)]
#![deny(clippy::large_stack_frames)]

use embassy_executor::Spawner;
use embassy_time::{Duration, Timer};
use esp_backtrace as _;
use esp_hal::clock::CpuClock;
use esp_hal::timer::timg::TimerGroup;
use log::{info, warn};

extern crate alloc;
use ft8132_probe::asic;
use ft8132_probe::ft8132;

esp_bootloader_esp_idf::esp_app_desc!();

#[allow(
    clippy::large_stack_frames,
    reason = "it's not unusual to allocate larger buffers etc. in main"
)]
#[esp_rtos::main]
async fn main(_spawner: Spawner) -> ! {
    esp_println::logger::init_logger_from_env();

    let config = esp_hal::Config::default().with_cpu_clock(CpuClock::max());
    let peripherals = esp_hal::init(config);

    esp_alloc::heap_allocator!(#[esp_hal::ram(reclaimed)] size: 73744);

    let timg0 = TimerGroup::new(peripherals.TIMG0);
    esp_rtos::start(timg0.timer0);

    info!("Embassy initialized!");

    let sda = peripherals.GPIO48;
    let scl = peripherals.GPIO4;
    let mut ft = ft8132::FT8132::new(sda, scl, peripherals.I2C0, peripherals.RMT);

    match asic::enter_i2c(&mut ft) {
        Ok(id) => info!("FT8132 chip id {id:#04x}"),
        Err(e) => warn!("FT8132 enter I2C failed: {e:?}"),
    }

    loop {
        info!("Hello world!");
        Timer::after(Duration::from_secs(1)).await;
    }
}
