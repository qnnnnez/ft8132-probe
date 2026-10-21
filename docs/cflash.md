# cflash

GenCode wants both halves: CRC **`0x77eb`** (210 ones). That result was not reached.

Page-aligned c2 alone does store. After erase, program-clock `F6` at 512/640/768 in the same session reads back CRC **`0xb61f`** (62 ones). Those ones are still there after a VIN cycle. The bytes at bit 512 match `data/mtp-gencode.bin`. That file is not in git. It is the first 1024 bytes of `UserConfigFile/FT8132_20250923_114955.bin` from the official V1.0.6 archive ([how to obtain the archive](README.md#files-that-are-not-in-git)).

Completing c1 in that same erase cycle erases them. The 118-byte frame at bit 384, that frame cut off at bit 497, a 100 kHz c1, or one pulse per c1 one all end at CRC **`0x384a`** (148 c1 ones, c2 empty). The 48 abflash ones that a normal erase leaves behind are cleared too. A later VIN cycle does not restore them. c1 at 128 and 256 only, with no frame at 384, leaves c2 in place and does not store c1.

The bench part no longer answers Find. SDA stays low with the Blue Pill pin released. SCL still rises. Chip id reads `0x00` and `bflash` stays all `00` after VIN is removed. Do not send another program clock to this part.

DUT VIN is 15 V. SCSI is `/dev/sg2`.

CRC16 is the GUI CCITT over the 748 cflash bits from offset 128:

| crc | Meaning |
|-----|---------|
| `0xf9be` | empty (0 ones) |
| `0x384a` | c1 only (148 ones; 62 holes in 502–875) |
| `0xb61f` | c2 only (62 ones; 148 holes in 128–501) |
| `0x77eb` | both (210 ones) — GenCode / GUI PASS |

## Why the official byte list is not enough

[`write.sr`](probe-fw.md) is the official dongle. After `F6` at 502/630/758 it reads those addresses back with data. The gap from the last c1 `F6` to `FF C1` is 1.7 ms. The gap from that `FF C1` to the next select is 1.8 ms, across a SCSI command on a high-speed dongle. `sub_20000C98` adds nothing else for opt 133: no sector erase, no `F9`, no second unlock.

```
896: FF C1 56 / FF / status, FE D2 F2, F6 @ 896 (110× 02/03), FE D2, FF C1
128: FF C1 56 / FF / status, F6 @ 128/256/384, FF C1
502: FF C1 56 / FF / status, F6 @ 502/630/758, FF C1
```

On this Blue Pill a full-speed round trip is longer than 1.8 ms. `FF C1` and then that longer idle really wipes cflash (`0xf9be` after a VIN cycle; abflash’s 48 ones remain). Collapsing the gap inside one command stops the wipe. c1 stays. The official c2 frames still do not stick.

Those three frames are the ones that cross a 128-bit boundary (502→512, 630→640, 758→768). Frames that stay inside a page program. 512/640/768 do. The 5 V pull-ups (2026-10-21) showed the chip sees valid highs, so the remaining difference is the edge timing inside a crossing frame.

`F6` during that transfer is the program clock. Each SCL pulse tells the chip to apply its internal programming voltage to one MTP bit. The wire levels stay ordinary I2C. A new knock in the middle of that session leaves later `F6` bytes acknowledged and the cells unwritten.

## Firmware guard

[`hv_admit`](../src/asic.rs) drops a program clock unless all of these hold:

- Chip id is `0x02` and `bflash` starts with `ffff00ff`. Any other header locks programming off.
- Erase finished after the current knock. A new knock cancels that permission.
- This is the first slice since that erase. `FF C1` closes the slice. The next SCSI command cannot program until a new erase.
- The frame does not start at bit 384 and does not cross a 128-bit page.

The stock 502 CDB programs only 512/640/768 from the stashed c2 image (`c2[10..]`). Find and register reads still run when programming is locked.

Bus highs are the external 4.7 kΩ resistors to 5 V. `float` / `drive_low` in [`ft8132.rs`](../src/ft8132.rs) are open-drain. A released pin is Hi-Z. PB6 and PB7 are 5 V tolerant only while released.

## SRAM lab

`lab load` copies the 1024-byte image into Blue Pill SRAM (`FE 10` / `FE 11`). `lab run --erase <dialect>` erases and programs from that buffer in one session (`FE 20`). `lab clamp` holds both lines low across a VIN cycle (`FE 30`).

Dialects: `c2-512`, `c2-502`, `c2-502x10`, `c2-bridge`, `c1-partial-then-c2`, `c2-then-c1`, `c1-then-c2`. The guard refuses `c2-502`, the bridge’s 502 frame, and any dialect that sends the frame at 384 or a second slice.

The one clean `c2-512` run was abflash 48, c2 62, CRC `0xb61f`. The 502-versus-10-bit runs did not finish; SDA was already stuck.

## Ruled out

| Try | Result |
|-----|--------|
| Per-CDB long select, no lone `FF C1` | `0x384a` |
| Per-CDB short select (`sub_20000C98`) | `0xf9be`; Find wedged. Cold: Find live, abflash 48, cflash 0 |
| Stash + official windows + 77 kHz + capture delays | `0x384a` |
| Program-clock verify | still `0x384a` (c2 empty) |
| IRQ-mask `F6`+address so `F6 01 F6` is atomic | `0x384a` |
| 1-byte `F6`+`03` at each c2 one | `0x384a` |
| `F6 00 00` prime before c2 (no unlock) | `0x384a` |
| Unlock + 1-bit echo + `F9` for c1 and c2 | `0x384a` |
| Two 1-bit `F9` sessions, either order | `0x384a` |
| Isolated c2 1-bit echo + `F9` | **`0xb61f`** |
| Isolated 100 kHz from 512 after unlock + `F6 00 00` | **`0xb61f`** |
| c1 then 512 windows, or 512 windows then the 384 frame | `0x384a` |
| 512 windows then 1-bit c1, or 1-bit c1 then 512 | `0x384a` |
| Interleaved 1-bit c1/c2 | `0x384a` |
| c1 then 100 kHz official 502/630/758 | `0x384a` |
| Open-drain program SDA, or push-pull SCL/SDA | `0x384a` |
| Extra SCL-high idle between bytes | program I2C NACK |
| 4.7 kΩ SCL/SDA to 5 V, both pins open-drain (2026-10-21) | `0x384a`; Find live |
| c1 closed, new select, program-clock 512/640/768 | `0x384a` |
| c2-512, then full c1, separate selects | `0x384a` after a VIN cycle. c2 cells are gone |
| c2-512 only, program clock, no unlock | **`0xb61f`** after a VIN cycle |
| c2 stored, VIN cycle, then c1 with no erase | hot read `0xf9be`; after another cycle, c2 still **`0xb61f`**, c1 absent |
| c1 at 128/256 only, no frame at 384 | c2 stays (62); c1 does not latch; abflash 48 stays |
| 384 frame stopped at bit 497, 100 kHz c1, c2 bytes appended past 501, re-knock, or 1-bit ones | `0x384a`. abflash 48 erased where a VIN cycle was done |
| Knock between erase and the program `F6` | `F6` ACKed, cells stay erased |

Do not add another unlock or a program `F9` on a mixed c1/c2 path. Do not put a lone `FF C1` after a USB-sized hole. Do not IRQ-mask a whole 16 ms window (Windows USBSTOR drops that).

## Check

Stop if Find is not live. This bench part currently fails that check.

```bash
uv run --project scripts bench psu cycle
uv run --project scripts bench find          # want bflash ffff00ff…, bflash_111 0, chip id 0x02
uv run --project scripts bench lab load data/mtp-gencode.bin
uv run --project scripts bench lab run --erase c2-512
uv run --project scripts bench mtp dump -o /tmp/mtp.bin
```

A full GenCode pass is still `mtp verify` CRC **`0x77eb`**. The guarded write path does not attempt that. It programs the c2 slice only.
