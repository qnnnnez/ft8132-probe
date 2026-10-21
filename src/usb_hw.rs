//! `stm32-usbd` glue for STM32F103 USB FS (PA11 D−, PA12 D+, 512 B PMA).

use stm32_usbd::{MemoryAccess, UsbBus, UsbPeripheral};

/// USB FS registers + packet-memory window on STM32F103.
pub struct UsbHw;

unsafe impl Sync for UsbHw {}
unsafe impl Send for UsbHw {}

unsafe impl UsbPeripheral for UsbHw {
    const REGISTERS: *const () = 0x4000_5C00 as *const ();
    const DP_PULL_UP_FEATURE: bool = false;
    const EP_MEMORY: *const () = 0x4000_6000 as *const ();
    const EP_MEMORY_SIZE: usize = 512;
    // F103: CPU sees each 16-bit PMA word as a 32-bit location (high half unused).
    const EP_MEMORY_ACCESS: MemoryAccess = MemoryAccess::Word16x1;

    fn enable() {
        let rcc = embassy_stm32::pac::RCC;
        rcc.apb1enr().modify(|w| w.set_usben(true));
        // Dummy read: APB1 clock enable is not instantaneous; USB register
        // access before it lands wedges the F103 AHB (and SWD).
        let _ = rcc.apb1enr().read();
        rcc.apb1rstr().modify(|w| w.set_usbrst(true));
        rcc.apb1rstr().modify(|w| w.set_usbrst(false));
        let _ = rcc.apb1enr().read();
        cortex_m::asm::delay(72 * 10);
    }

    fn startup_delay() {
        // Analog PHY power-up after PDWN: datasheet tSTARTUP ~1 µs; give 5 µs at 72 MHz.
        cortex_m::asm::delay(72 * 5);
    }
}

pub type ProbeUsbBus = UsbBus<UsbHw>;
