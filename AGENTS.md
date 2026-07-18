# AGENTS.md

## Project

Bare-metal Rust firmware for **ESP32-S3** that:

1. Probes an **FT8132** over RMT “knock” then I2C (`src/ft8132.rs`, `src/bin/main.rs`).
2. Experiments with **USB device** modes: CDC serial and mass storage / SCSI (`src/bin/usb*.rs`, `src/usbd_mass_storage.rs`).

This is a probe / experiment repo: binaries are small demos, not a polished product.

## Stack

- `#![no_std]` + `#![no_main]` firmware; heap via `esp-alloc`
- Target: `xtensa-esp32s3-none-elf` (see `.cargo/config.toml`)
- Toolchain: `esp` channel (`rust-toolchain.toml`) — use `espup` / Espressif Rust, not stock stable
- Async: Embassy + `esp-rtos` (`#[esp_rtos::main]`)
- HAL: `esp-hal` ~1.0 (`esp32s3`, `unstable` where needed)
- USB: `esp-hal::otg_fs` + `usb-device`; MSC via vendored `vendor/usbd-storage` and/or in-tree `usbd_mass_storage`
- Logging: `log` + `esp-println` (`ESP_LOG=info` in cargo config)

## Layout

| Path | Role |
|------|------|
| `src/lib.rs` | Shared crate: `led`, `ft8132`, `hexstring`, `usbd_mass_storage` |
| `src/bin/main.rs` | Default bin (`ft8132-probe`): FT8132 knock + I2C switch |
| `src/bin/usbserial*.rs`, `emusbserial.rs` | USB CDC serial experiments |
| `src/bin/usbdisk*.rs` | USB MSC / SCSI experiments (`usbdisk2` uses `usbd-storage`; `usbdisk3` uses in-tree MSC) |
| `vendor/usbd-storage/` | Path dependency; prefer minimal local patches |

Bins under `src/bin/` are auto-discovered. Prefer extending an existing bin over adding a new one unless the experiment needs isolation.

## Hardware (as used in code)

- FT8132: **SDA = GPIO48**, **SCL = GPIO4**, I2C0 + RMT
- USB OTG FS: **D+ = GPIO20**, **D− = GPIO19**, `USB0`
- Optional WS2812 on GPIO48 in `led` (conflicts with FT8132 SDA — don’t enable both casually)

## Build / flash / monitor

```bash
# Default binary
cargo build
cargo run                    # espflash flash --monitor --chip esp32s3

# Named binary
cargo build --bin usbdisk2
cargo run --bin usbdisk2

# Release (size-optimized; both profiles use opt-level = "s")
cargo build --release --bin usbdisk2
```

Requires `espflash` on `PATH`. Do not change the default target or drop `build-std = ["alloc", "core"]` without a strong reason.

Long commands: wrap with `timeout` (e.g. `timeout 5m cargo build --bin usbdisk2`).

## Conventions

- Keep firmware `no_std`. Host-only helpers stay out of the firmware crate.
- Prefer async Embassy tasks; use `StaticCell` for `'static` USB bus / buffers.
- USB interrupt pattern (see `usbdisk2` / `usbserial2`): handler signals `embassy_sync::Signal`, disables USB IRQ; task polls then re-enables IRQ.
- Deny `clippy::mem_forget` on esp-hal buffer owners; treat large stacks carefully (`stack-size-threshold = 1024` in `.clippy.toml`).
- Match existing style: early `esp_bootloader_esp_idf::esp_app_desc!()`, `esp_backtrace as _`, logger init from env.
- Use `ft8132_probe::hex_bytes!` for compile-time hex constants when useful.
- Logging: `log::{info,warn,error}` — not `println!` / defmt unless already in that path.

## Boundaries

- Do not rewrite `vendor/usbd-storage` wholesale; small fixes or feature use only.
- Do not add `std`, Tokio, or host networking crates to the firmware package.
- Do not commit secrets, flash dumps, or large binary artifacts under `target/`.
- Do not force-push or amend git history unless explicitly asked.
- Avoid drive-by refactors across unrelated bins; change the bin or module under discussion.

## When unsure

Prefer the patterns in the nearest similar bin (`usbdisk2` for SCSI/`usbd-storage`, `usbdisk3` for in-tree MSC, `usbserial2` for CDC + IRQ signaling, `main` for FT8132).
