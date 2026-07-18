# ft8132-probe

Bare-metal Rust firmware for **ESP32-S3** that probes an **FT8132** (RMT knock → I2C) and experiments with USB device modes (CDC serial and mass storage / SCSI).

This is a small probe / experiment repo — the binaries are demos, not a finished product.

## Requirements

- ESP32-S3 board with USB OTG pins available
- [Espressif Rust toolchain](https://docs.esp-rs.org/book/) (`espup` → `esp` channel; see `rust-toolchain.toml`)
- [`espflash`](https://github.com/esp-rs/espflash) on `PATH`
- Rust edition 2024 / rustc ≥ 1.88 (as declared in `Cargo.toml`)

## Hardware

| Function | Pins |
|----------|------|
| FT8132 SDA / SCL | GPIO48 / GPIO4 (I2C0 + RMT) |
| USB OTG D+ / D− | GPIO20 / GPIO19 (`USB0`) |

GPIO48 is also used by the optional WS2812 helper in `src/led.rs` — don’t enable that alongside FT8132 SDA without remapping.

## Build & flash

Default target is `xtensa-esp32s3-none-elf`. `cargo run` flashes and opens a serial monitor via `espflash`.

```bash
# Default binary (ft8132-probe / main.rs)
cargo build
cargo run

# Named binary
cargo build --bin usbdisk2
cargo run --bin usbdisk2

# Size-optimized release
cargo build --release --bin usbdisk2
```

Log level defaults to `info` (`ESP_LOG` in `.cargo/config.toml`).

## Binaries

| Binary | Source | Purpose |
|--------|--------|---------|
| `ft8132-probe` | `src/bin/main.rs` | FT8132 knock + switch to I2C |
| `usbserial` | `src/bin/usbserial.rs` | USB CDC serial (`usb-device` + `usbd-serial`) |
| `usbserial2` | `src/bin/usbserial2.rs` | CDC serial with IRQ → Embassy task signaling |
| `emusbserial` | `src/bin/emusbserial.rs` | Embassy-USB serial experiment |
| `usbdisk` | `src/bin/usbdisk.rs` | Early USB MSC experiment |
| `usbdisk2` | `src/bin/usbdisk2.rs` | SCSI / BBB via vendored `usbd-storage` |
| `usbdisk3` | `src/bin/usbdisk3.rs` | MSC via in-tree `usbd_mass_storage` |

## Layout

```
src/
  lib.rs                 # Shared crate
  ft8132.rs              # FT8132 RMT/I2C driver
  usbd_mass_storage.rs   # In-tree USB MSC class
  hexstring.rs           # Compile-time hex_bytes! macro
  led.rs                 # WS2812 over RMT
  bin/                   # Firmware entry points (see table above)
vendor/usbd-storage/     # Path dependency (BBB + SCSI)
```

## License / vendor

`vendor/usbd-storage` is a vendored copy of the experimental [`usbd-storage`](https://crates.io/crates/usbd-storage) crate; see that directory for its own docs and license terms.
