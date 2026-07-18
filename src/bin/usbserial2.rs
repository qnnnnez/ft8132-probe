#![no_std]
#![no_main]

use embassy_executor::Spawner;
use embassy_sync::blocking_mutex::raw::CriticalSectionRawMutex;
use embassy_sync::signal::Signal;
use embassy_time::{Duration, Timer};
use esp_backtrace as _;
use esp_hal::{
    interrupt::{self, Priority},
    otg_fs::{Usb, UsbBus},
    peripherals::Interrupt,
    timer::timg::TimerGroup,
};
use static_cell::StaticCell;
use usb_device::bus::UsbBusAllocator;
use usb_device::prelude::{UsbDevice, UsbDeviceBuilder, UsbVidPid};
use usbd_serial::{SerialPort, USB_CLASS_CDC};
use log::info;

esp_bootloader_esp_idf::esp_app_desc!();

static USB_SIGNAL: Signal<CriticalSectionRawMutex, ()> = Signal::new();

type UsbBusType = UsbBus<Usb<'static>>;

#[esp_hal::handler(priority = Priority::Priority2)]
fn usb_interrupt_handler() {
    USB_SIGNAL.signal(());
    let _ = interrupt::disable(esp_hal::system::Cpu::ProCpu, Interrupt::USB);
    let _ = interrupt::disable(esp_hal::system::Cpu::AppCpu, Interrupt::USB);
}

#[embassy_executor::task]
async fn usb_task(
    mut usb_dev: UsbDevice<'static, UsbBusType>,
    mut serial: SerialPort<'static, UsbBusType>,
) {
    info!("USB task started!");

    loop {
        USB_SIGNAL.wait().await;
        
        // Drain events
        loop {
            let processed = usb_dev.poll(&mut [&mut serial]);
            
            let mut buf = [0u8; 64];
            match serial.read(&mut buf) {
                Ok(count) if count > 0 => {
                    // Echo back in upper case
                    for c in buf[0..count].iter_mut() {
                        if 0x61 <= *c && *c <= 0x7a {
                            *c &= !0x20;
                        }
                    }

                    let mut write_offset = 0;
                    while write_offset < count {
                        match serial.write(&buf[write_offset..count]) {
                            Ok(len) if len > 0 => {
                                write_offset += len;
                            }
                            _ => {}
                        }
                    }
                }
                _ => {}
            }
            
            if !processed {
                break;
            }
        }

        interrupt::enable(Interrupt::USB, usb_interrupt_handler.priority()).unwrap();
    }
}

#[esp_rtos::main]
async fn main(spawner: Spawner) -> ! {
    esp_println::logger::init_logger_from_env();
    let peripherals = esp_hal::init(esp_hal::Config::default());

    esp_alloc::heap_allocator!(#[esp_hal::ram(reclaimed)] size: 73744);

    let timg0 = TimerGroup::new(peripherals.TIMG0);
    esp_rtos::start(timg0.timer0);

    info!("Embassy initialized!");

    let usb = Usb::new(peripherals.USB0, peripherals.GPIO20, peripherals.GPIO19);
    static EP_MEMORY_CELL: StaticCell<[u32; 1024]> = StaticCell::new();
    static USB_BUS: StaticCell<UsbBusAllocator<UsbBusType>> = StaticCell::new();
    
    let usb_bus = USB_BUS.init(UsbBus::new(usb, EP_MEMORY_CELL.init([0; 1024])));

    let serial = SerialPort::new(usb_bus);
    
    let usb_dev = UsbDeviceBuilder::new(usb_bus, UsbVidPid(0x303A, 0x3001))
        .device_class(USB_CLASS_CDC)
        .build();

    spawner.spawn(usb_task(usb_dev, serial)).unwrap();

    unsafe {
        interrupt::bind_interrupt(Interrupt::USB, usb_interrupt_handler.handler());
    }
    interrupt::enable(Interrupt::USB, usb_interrupt_handler.priority()).unwrap();

    info!("USB initialized!");

    loop {
        // Main task can do other things
        Timer::after(Duration::from_secs(1)).await;
    }
}
