# ft8132-probe

Bare-metal Rust firmware for **STM32F103C6T6** (Blue Pill) that probes an **FT8132** (GPIO knock → I2C) and presents USB mass storage / SCSI to the host.

This is a small probe / experiment repo — the binary is a demo, not a finished product.

## Requirements

- STM32F103C6T6 Blue Pill (or a C8; see [Memory](#memory))
- Stable Rust **1.99** with `thumbv7m-none-eabi` (`rust-toolchain.toml`)
- [`probe-rs`](https://probe.rs/) on `PATH` and a SWD probe (ST-Link, DAPLink, …)
- 8 MHz HSE crystal (stock Blue Pill)

## Hardware

| Function | Pins |
|----------|------|
| USB D− / D+ | PA11 / PA12 (on-board 1.5 kΩ D+ pull-up) |
| FT8132 SDA / SCL | PB7 / PB6. Knock is bit-banged on SCL with SDA held low |

Clocks: 8 MHz HSE → PLL ×9 → 72 MHz SYSCLK, USB 48 MHz.

Both I2C lines are open-drain. External 4.7 kΩ resistors to 5 V provide the high. A high is a released pin. PB6 and PB7 are 5 V tolerant only while released.

## Memory

Default [`memory.x`](memory.x) is **C6**: 32 KiB flash, 10 KiB SRAM. USB packet memory (512 B PMA) is **not** taken from SRAM, so a 512-byte SCSI buffer still fits.

Release size of `ft8132-probe` without `mock-asic` (`.vector_table` + `.text` + `.rodata` + `.data` in flash; `.data` + `.bss` in RAM):

| | Used | Limit |
|--|------|-------|
| Flash | ~26 KiB | 32 KiB |
| Static RAM | ~4.7 KiB | 10 KiB |

If a later change does not link on C6, switch `memory.x` to **C8** (64 KiB flash / 20 KiB SRAM) and run with `--chip STM32F103C8`. Same firmware, bigger Blue Pill.

## Build & flash

Default target is `thumbv7m-none-eabi`. `cargo run` uses `probe-rs run --chip STM32F103C6`.

```bash
# Size-optimized image (recommended)
cargo build --release
cargo run --release

# Capture-seeded mock ASIC (no FT8132 on PB7/PB6)
cargo build --release --features mock-asic
cargo run --release --features mock-asic
```

VID/PID is `0x1B3F`/`0x8000`, SCSI INQUIRY magic `GP-PROD.` — the same identity the official debugger looks for.

## Host CLI

Python CLI in `host/ft8132/` speaks the same SCSI vendor CDBs as FT8132Debugger (`F0 F0` auth, `FD`/`FF` register and Find). Linux only (`SG_IO` on `/dev/sg*`).

```bash
cd host/ft8132
uv sync
uv run ft8132 --help
uv run ft8132 list
uv run ft8132 find
uv run ft8132 state
uv run ft8132 stop            # writes 0x02/0x03 = 0 (not a full debug download)
uv run ft8132 speed 30
uv run ft8132 direction 0
uv run ft8132 regs dump --json dump.json --bin dump.bin
uv run ft8132 reg get 0x04 --typed
```

The sg node needs read/write access. Example udev rule:

```
SUBSYSTEM=="scsi_generic", ATTRS{idVendor}=="1b3f", ATTRS{idProduct}=="8000", MODE="0660", GROUP="plugdev"
```

`start` / `stop` / `speed` only change the live I2C speed command. They assume Debug parameters are already on the chip.

MTP lab commands (`lab load`, `lab run --erase`, `lab clamp`) use a 1024-byte image in Blue Pill SRAM. See [docs/cflash.md](docs/cflash.md). The official debugger archive, GenCode image, and live dumps are not in git: [docs/README.md](docs/README.md#files-that-are-not-in-git).

## cflash findings

Programming both GenCode halves (CRC `0x77eb`) did not succeed. Details and the ruled-out list are in [docs/cflash.md](docs/cflash.md).

- c2 alone, at page starts 512/640/768, stores 62 ones (CRC `0xb61f`) and survives a VIN cycle.
- Completing c1 in that same erase cycle erases those ones. The frame at bit 384 is the commit. Power-cycling does not bring them back.
- The official dongle sends the same `F6` bytes and does store c2. Its gap after `FF C1` is 1.8 ms. A longer USB gap on this full-speed port wipes cflash. Matching the short gap inside one command keeps c1 and still does not store the official 502/630/758 frames, which cross a 128-bit page.
- A knock between erase and the program `F6` acknowledges the bytes and does not burn the cells.
- The firmware refuses a program clock unless Find is healthy (`chip id 0x02`, `bflash` `ffff00ff`), the erase is still the current knock session, and the frame is the first slice and stays inside a 128-bit page. The frame at bit 384 is refused.
- The part on this bench now holds SDA low across a VIN cycle. Find reads zeros. Do not send it another program clock.

## Layout

```
src/
  lib.rs                 # Shared crate
  asic.rs                # HAL-free FT8132 protocol
  ft8132.rs              # Knock (PB6) + blocking I2C1
  usb_hw.rs              # stm32-usbd UsbPeripheral (F103 FS)
  hexstring.rs           # Compile-time hex_bytes! macro
  mock_asic.rs           # --features mock-asic
  bin/usbdisk2.rs        # Firmware entry (SCSI / BBB)
vendor/usbd-storage/     # Nested git clone (qnnnnez/usbd-storage); crate in usbd-storage/
host/ft8132/             # Python CLI (uv)
memory.x                 # C6 32K/10K (change to C8 64K/20K if needed)
```

## License / vendor

`vendor/usbd-storage` is a nested git clone of [`qnnnnez/usbd-storage`](https://github.com/qnnnnez/usbd-storage) (fork of [`apohrebniak/usbd-storage`](https://github.com/apohrebniak/usbd-storage)). The firmware path-depends on the crate at `vendor/usbd-storage/usbd-storage`; see that tree for its docs and license terms.
