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
use log::info;
use esp_hal::rmt::Rmt;
use esp_hal::time::Rate;
use esp_hal::gpio::Pin;
use esp_hal::gpio::{Level, OutputConfig, Output};

extern crate alloc;
use ft8132_probe::led;
use ft8132_probe::ft8132;
use ft8132_probe::usb_mas;

esp_bootloader_esp_idf::esp_app_desc!();

#[allow(
    clippy::large_stack_frames,
    reason = "it's not unusual to allocate larger buffers etc. in main"
)]
#[esp_rtos::main]
async fn main(spawner: Spawner) -> ! {
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
    
    ft.knock().await;
    ft.switch_to_i2c();
    
    {
        let _i2c = ft.get_i2c();
        // Do something with i2c...
    }

    ft.switch_to_rmt();
    ft.knock().await;

    // let rmt = Rmt::new(peripherals.RMT, Rate::from_mhz(80)).unwrap().into_async();
    // let pin = peripherals.GPIO48.degrade();

    // Use GPIO48 for WS2812
    // spawner.spawn(led::blink(rmt, pin)).ok();

    loop {
        info!("Hello world!");
        Timer::after(Duration::from_secs(1)).await;
    }
}
