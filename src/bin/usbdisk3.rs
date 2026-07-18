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
use usb_device::{device::StringDescriptors, LangID};
use log::info;

use ft8132_probe::usbd_mass_storage::UsbMassStorage;

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
    mut msc: UsbMassStorage<'static, UsbBusType>,
) {
    info!("USB task started!");

    loop {
        USB_SIGNAL.wait().await;
        
        // Drain events
        loop {
            let processed = usb_dev.poll(&mut [&mut msc]);
            
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

    let msc = UsbMassStorage::new(usb_bus, 0);
    
    // VID/PID for testing. device_class(0) means look at interface descriptors.
    let usb_dev = UsbDeviceBuilder::new(usb_bus, UsbVidPid(0x303A, 0x3001))
        .strings(&[StringDescriptors::new(LangID::EN)
            .manufacturer("Espressif")
            .product("USB Mass Storage")
            .serial_number("12345678")])
        .expect("Failed to set strings")
        .device_class(0x00)
        // 0x0210 = USB 2.10. Required for BOS descriptors if you want to support them properly.
        // However, if we don't implement BOS handler, host might get confused if we claim > 2.0.
        // Try USB 2.0 (0x0200) to avoid BOS requirement, or 0x0210 if we want to handle it.
        // Default is 0x0200 in usb-device.
        .usb_rev(usb_device::device::UsbRev::Usb200)
        .build();

    spawner.spawn(usb_task(usb_dev, msc)).unwrap();

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
