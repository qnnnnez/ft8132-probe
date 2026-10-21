# Official probe firmware (`ft8132Debugger.bin`)

Measured 2026-10-18 against FT8132Debugger **V1.0.6** on the bench probe (`1b3f:8000`, high-speed, `bcdDevice` 1.00). The archive is not in git. Download it and extract to `data/official/` as described in [README.md](README.md). GUI SCSI behavior: [gui.md](gui.md).

`code/ft8132Debugger.bin` (14504 bytes) is ARM code for the **Generalplus GPL32xxx** inside the USB dongle. It is not FT8132 eFuse data and not the Blue Pill image. The Blue Pill firmware is a reimplementation of what this file does after it is running.

## ROM, then this image

At plug-in the on-chip USB ROM enumerates as `GENPLUS USB-MSDC DISK A` / `GP-PROD.` and accepts only the download commands below. `FD 00` is illegal until this image is running. `conf/FT8132Debugger.conf` is the boot script the DLL parses into those SCSI commands. `EnableBootWrite = 0`, so the file is not stored in flash. A power cycle returns the dongle to the ROM, and the debugger loads the file again.

The script’s live steps:

| Step | Action | Target | What it does |
|------|--------|--------|----------------|
| 1 | Write | register `0xD0200040` | value `0x00004512` |
| 2 | Write | RAM `0x20000000` | `code/ft8132Debugger.bin`, then read it back |
| 3 | Jump | RAM `0x20000000` | start the image |

Step 4 is a raw `FD 00` CDB with `Disabled = 1`, so the tool does not send it. After the jump, this image handles `FD` / `FF` itself. The Windows DLL does not bit-bang I2C.

## Load and run

The ROM enters the image at `0x20000000`. The first four bytes are Thumb (`ADR` / `BX`) and switch to ARM at `0x20000004`:

- `MSR CPSR, #0xD3` (SVC, IRQs masked)
- clear `0xFF000000`
- `0xD0100038 = 1`
- `0xD0300000`, `0xD0300040`, `0xD0300080`, `0xD03000C0` = `0x200`
- `0xD05001FC = 0`
- copy `0x3620` bytes from `0x20000150` to `0xF8000000`
- copy `0x138` bytes from `0x20003770` to `0xF8003A00`
- zero `0x280` bytes at `0xF8003B38`
- set SP, store it at `0xF8002FF0`, then `BX` to Thumb `sub_20000140`

Code keeps running at the load address. `0xF8000000` is the relocated RW RAM (and the saved SP), not the instruction PC. `enter start()` (`sub_20001782`) is the main loop. It does not return.

## SCSI dispatch

`sub_20001322` reads the 31-byte CBW. CDB byte 0 is CBW byte 15. It accepts the command only when CBW bytes 29–30 are `GP` (CDB bytes 14–15). The switch is on `CDB[0]<<8 | CDB[1]`. Anything else returns status 2. A missing `GP` returns status 3.

| CDB | Load address | Role |
|-----|--------------|------|
| `F0 F0`, `FF F0` | `sub_20001C0C` | auth (`F0 F0` matches the GUI) |
| `F0 00` | `sub_20001B1A` | |
| `F0 01` | `sub_20001C38` | |
| `F0 10` | `sub_20001C4A` | |
| `FD 00` | `sub_20000924` | enter. CDB byte 7 is 6 or 7 |
| `FD 20` | `sub_20001314` | calls `sub_20002AF0` (`beg chip erase`) |
| `FD 28`, `FF 28` | `sub_200012C0` | read |
| `FD 2A`, `FF 2A` | `sub_20000F50` | write |
| `FD FF` | `sub_200023DC` | also called once before the poll loop |
| `FF FF` | `sub_200008B8` | scan / exit |

`FD 2A` then switches on CDB byte 9 (`opt`):

| `opt` | Load address |
|-------|----------------|
| 5, 6, 133 | `sub_20000C98` (133 is the GUI MTP write) |
| 0, 1, 2, 4, 19, 48, 49, 50 | `sub_20000D92` (register write) |

IDA addresses above are in the load image at `0x20000000`.

## MTP write (`sub_20000C98`, opt 133)

Each `FD 2A` is handled immediately. There is no stash of 896/128 until 502.

CDB address (`sub_2000180C`, bytes 4–5) chooses the wrapper; length (`sub_20001B0E`, bytes 7–8) is the bit count. Acc is CDB byte 6 (`sub_20001922(21)`).

| Address | Wrapper |
|---------|---------|
| 0 or 896 | `sub_2000094E(1)` select, `sub_20000C6A(1)` unlock, `sub_200009B0`, lock, `sub_2000094E(0)` deselect |
| anything else (128 and 502) | select, `sub_200009B0`, deselect. No unlock |

`sub_2000094E(1)` writes `FF C1 56`, then `FF` and reads one status byte. `sub_2000094E(0)` writes `FF C1` only. No 4 ms retry loop.

`sub_200009B0` splits length into 128-bit windows from the CDB address (374 → 128+128+118 at 502 / 630 / 758). Each window is one I2C write (`sub_200028BC`): `F6`, addr bits `[9:8]`, addr bits `[7:0]`, then `03` if the SCSI byte is nonzero else `02`. No echo, no `F9`. Inter-window idle in `write.sr` (~4.4 ms) is the UART `btp_ret` / `etp_ret` prints in `sub_20002CAA`, not a programmed wait.

I2C bit time is `sub_2000259E(5)` on each SCL edge (busy loop `79*n`, subtract 10). ACK (`sub_20002696`) samples SDA up to three times in a shorter high. Chip erase (`sub_20002AF0`) is select, unlock, `F6 00 00 0C` / `F9` / `F6 00 00 0E` / `F9`, lock, deselect.

The loops in `sub_20000C98` that look like 128/640 pages are **sector erase** (`sub_20002B34`, string `beg sec erase`) and run only when `opt != 133`.

Mismatch vs Blue Pill firmware: [cflash.md](cflash.md).

## ELF for IDA

The `.bin` ships in the V1.0.6 archive at `code/ft8132Debugger.bin`. The ELF and the IDA database `ft8132-probe-fw.elf.i64` (204 functions, 48 strings, Hex-Rays) do not. Build the ELF, open it in IDA, and save the database beside it:

```bash
CODE=data/official/FT8132Debugger_V1.0.6/code
cat > /tmp/ft8132.ld << 'EOF'
OUTPUT_FORMAT("elf32-littlearm")
OUTPUT_ARCH(arm)
ENTRY(_entry)
SECTIONS
{
  . = 0x20000000;
  .text : {
    _entry = . + 4;
    *(.data)
  }
}
EOF
arm-none-eabi-ld -T /tmp/ft8132.ld -b binary -o "$CODE/ft8132Debugger.elf" "$CODE/ft8132Debugger.bin"
arm-none-eabi-strip -s "$CODE/ft8132Debugger.elf"
```

Entry is `0x20000004`. `.text` is `AX` at `0x20000000`, program header `R E`. Do not `objcopy --rename-section` this file: that rewrite left the load segment as zeros.
