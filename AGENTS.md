# AGENTS.md

## Project

Bare-metal Rust firmware for **STM32F103C6T6** (Blue Pill) that:

1. Probes an **FT8132** over a GPIO “knock” on SCL, then I2C1 (`src/ft8132.rs`).
2. Speaks USB MSC / SCSI to the host (`src/bin/usbdisk2.rs`) so `host/ft8132/` and FT8132Debugger work.

This is a probe / experiment repo: the binary is a demo, not a polished product.

## Stack

- `#![no_std]` + `#![no_main]` firmware; no heap
- Target: `thumbv7m-none-eabi` (see `.cargo/config.toml`)
- Toolchain: **1.99.0** (`rust-toolchain.toml`) — stock stable, not the Espressif `esp` channel
- Async: Embassy (`#[embassy_executor::main]`)
- HAL: `embassy-stm32` 0.6 (`stm32f103c6`)
- USB: `stm32-usbd` + `usb-device`; MSC via nested git clone `vendor/usbd-storage` (crate: `vendor/usbd-storage/usbd-storage`)
- Panic: `panic-halt` (no `log` / defmt in the firmware image)

## Layout

| Path | Role |
|------|------|
| `src/lib.rs` | Shared crate: `asic`, `ft8132`, `hexstring`, `usb_hw` |
| `src/bin/usbdisk2.rs` | Sole binary (`ft8132-probe`): SCSI CDB dispatch + USB IRQ |
| `src/asic.rs` | HAL-free FT8132 I2C protocol |
| `src/ft8132.rs` | Bit-bang knock on PB6 + blocking I2C1 |
| `src/usb_hw.rs` | `stm32-usbd` `UsbPeripheral` for F103 USB FS |
| `vendor/usbd-storage/` | Nested git clone of [qnnnnez/usbd-storage](https://github.com/qnnnnez/usbd-storage) (fork of apohrebniak/usbd-storage); crate is `vendor/usbd-storage/usbd-storage` |
| `host/ft8132/` | Python CLI (`bench` shares it; MTP `dump`/`erase`/`write`/`verify`) |

There is a single `[[bin]]`. Prefer extending `usbdisk2` over adding a new binary unless the experiment needs isolation.

## Hardware (as used in code)

- USB: **PA11 D−**, **PA12 D+** (on-board 1.5 kΩ D+ pull-up)
- FT8132 I2C1: **PB7 SDA**, **PB6 SCL** (knock is bit-banged on SCL with SDA held low)
- Clocks: 8 MHz HSE → 72 MHz SYSCLK, USB 48 MHz (`usbpre` ÷1.5)
- Optional idle LED on **PC13** is not driven

## Memory

`memory.x` is **C6**: 32 KiB flash, 10 KiB SRAM. USB PMA (512 B) is separate from SRAM.

Release `--bin ft8132-probe` (no `mock-asic`) last measured about **20.4 KiB** flash (`.vector_table`+`.text`+`.rodata`) / **2.9 KiB** static RAM (`.data`+`.bss`), so C6 still fits. If a later change overflows flash or RAM, set `memory.x` to **C8** (64 KiB / 20 KiB) and flash with `--chip STM32F103C8`.

## Build / flash / monitor

```bash
# Default binary (ft8132-probe)
cargo build --release
cargo run --release              # probe-rs run --chip STM32F103C6

# Mock ASIC (no PB7/PB6 wiring)
cargo build --release --features mock-asic
```

Requires `probe-rs` on `PATH` and a SWD probe. Do not change the default target to xtensa or reintroduce `build-std`.

Long commands: wrap with `timeout` (e.g. `timeout 5m cargo build --release`).

## Conventions

- Keep firmware `no_std`. Host-only helpers stay out of the firmware crate.
- Prefer async Embassy tasks; use `StaticCell` for `'static` USB bus / buffers.
- USB interrupt pattern: `USB_LP_CAN1_RX0` signals `embassy_sync::Signal`, disables the IRQ; the USB task polls then re-enables.
- Use `ft8132_probe::hex_bytes!` for compile-time hex constants when useful.
- Do not pull `log` / `defmt` into release firmware (flash budget).

## Boundaries

- Do not rewrite `vendor/usbd-storage` wholesale; small fixes or feature use only.
- Do not add `std`, Tokio, or host networking crates to the firmware package.
- Do not commit secrets, flash dumps, or large binary artifacts under `target/`.
- Do not force-push or amend git history unless explicitly asked.
- Avoid a dual ESP/STM crate: this tree is STM32-only (stable vs `esp` toolchains cannot share one default).

## When unsure

Prefer the patterns in `usbdisk2` (SCSI / `usbd-storage` / USB IRQ) and `ft8132` (knock + blocking I2C).
