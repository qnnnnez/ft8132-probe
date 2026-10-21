# Bench CLI

Host-only Typer app for the `n3150` workbench. Firmware stays `no_std`. SCSI
subcommands (`list`, `find`, `state`, `reg`, …) are the same functions as
`host/ft8132/` (`uv run --project host/ft8132 ft8132 …`).

```bash
uv run --project scripts bench --help
```

| Subcommand | What |
|------------|------|
| `psu status\|on\|off\|cycle` | PDPowerMiniV1 CDC (does not change V/I setpoint) |
| `flash [--mock]` | `cargo build --release` + `probe-rs download` + `reset` |
| `chmod` | Wait for `1b3f:8000` `/dev/sgN` and `chmod a+rw` if needed |
| `list` `find` `state` `reg` `regs` … | Host SCSI CLI (shared implementation) |
| `mtp dump\|erase\|write\|verify` | eFuse/MTP (GUI Erase / Write / Verify) |
| `lab load\|run\|clamp` | SRAM image and one program slice (`FE 10` / `FE 20` / `FE 30`) |

`extract_mock.py` is a PulseView → mock-ASIC fixture helper, not part of `bench`.

```bash
uv run --project scripts bench psu status
uv run --project scripts bench psu on          # existing setpoint (here 15 V)
uv run --project scripts bench flash
uv run --project scripts bench chmod
uv run --project scripts bench list
uv run --project scripts bench find
uv run --project scripts bench state
uv run --project scripts bench reg get 0x01
uv run --project scripts bench mtp dump -o data/mtp-before.bin
uv run --project scripts bench mtp erase
uv run --project scripts bench mtp dump -o data/mtp-erased.bin
uv run --project scripts bench mtp write --bin data/mtp-before.bin
uv run --project scripts bench mtp verify --bin data/mtp-before.bin
uv run --project scripts bench lab load data/mtp-gencode.bin
uv run --project scripts bench lab run --erase c2-512
uv run --project scripts bench psu cycle       # DUT VIN off/on if I2C stuck
```

`PSU_TTY` / `PSU_SERIAL` override the PSU port. `CHIP` / `ELF` override the
probe-rs target and image path (default `STM32F103C6` and the release ELF).
