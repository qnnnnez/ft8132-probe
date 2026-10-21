# Testing firmware against FT8132Debugger

The Fortior app **FT8132Debugger** is a Windows WPF GUI plus native `FTechUsbWrapperDllU.dll`. It uses the same USB MSC / SCSI vendor CDBs as `host/ft8132/`. The dongle is a Generalplus MCU; the RAM image it runs is [probe-fw.md](probe-fw.md).

Official build used on this bench: **V1.0.6** ([download](https://download.fortiortech.com/download?file=files/20260427/FT8132Debugger_V1.0.6.rar&attname=FT8132Debugger_V1.0.6.rar)). The PE FileVersion inside the EXE is still `1.0.5.1`; the window title is `FT8132Debugger V1.0.6`. Extract with `unrar` (RAR 5). Keep the archive under gitignored `data/official/` — do not commit it.

Firmware identity the GUI looks for:

| Item | Value |
|------|--------|
| USB VID:PID | `1b3f:8000` |
| SCSI INQUIRY vendor / product / rev | `GENPLUS ` / `USB-MSDC DISK A ` / `1.00` |
| INQUIRY bytes 36–43 | `GP-PROD.` |
| Removable disk | yes, **no medium** (`TEST UNIT READY` → sense 0x3A) |

## Linux stand-in (this server)

Same SCSI language as GUI Find / Read ID / GetNandID. Use this for day-to-day checks.

DUT VIN must be on (see [workbench](workbench.md)). Then:

```bash
uv run --project scripts bench psu on          # existing setpoint
uv run --project scripts bench chmod
uv run --project scripts bench list            # /dev/sgN for 1b3f:8000
uv run --project scripts python scripts/probe_fw_test.py   # MainWindow.cs SCSI matrix
# or piecemeal:
uv run --project scripts bench find            # GUI Read ID / Find
uv run --project scripts bench state           # 44-byte window at 0x40
uv run --project scripts bench reg get 0x01
```

`find` / the matrix should report `chip_id` 2, `comm_status` 0, `bflash_111` 0, and a non-zero `state` window. Do not run the CLI and the GUI on the same gadget at once (exclusive SCSI).

`--features mock-asic` makes Find succeed from fixtures with no FT8132 wired. Use it only to check USB identity; a real GUI-vs-chip test needs the non-mock image.

## Wine on this Debian bench (measured 2026-10-15)

WPF **does** start under Wine 10 + native .NET 3.5 SP1. The USB wrapper **does not** see the Blue Pill, so Find stays disabled (`Device: Disconnected`). Use the Linux CLI above for SCSI, or the libvirt Windows 10 run below for a real GUI-vs-chip pass.

Prefix (32-bit; lives in `$HOME`, not the repo):

```bash
sudo dpkg --add-architecture i386   # once
sudo apt-get install -y wine32 wine64 winetricks cabextract unrar xvfb xdotool udisks2
export WINEPREFIX="$HOME/.wine-ft8132" WINEARCH=win32
timeout 5m xvfb-run -a env WINEPREFIX="$WINEPREFIX" WINEARCH="$WINEARCH" wineboot -i
# corefonts: if GitHub 500s, fetch the EXEs from SourceForge into ~/.cache/winetricks/corefonts/ first
timeout 30m xvfb-run -a env WINEPREFIX="$WINEPREFIX" WINEARCH="$WINEARCH" winetricks -q dotnet35sp1 corefonts
```

Workbench, then GUI (exclusive SCSI — stop the CLI first):

```bash
uv run --project scripts bench psu on
uv run --project scripts bench chmod
sudo chmod a+rw /dev/sdd /dev/bus/usb/003/$(cat /sys/bus/usb/devices/3-1.1/devnum)
# map the 0-byte removable disk so mountmgr has a unix node
ln -sfn /dev/sdd "$WINEPREFIX/dosdevices/d:"
ln -sfn /dev/sdd "$WINEPREFIX/dosdevices/d::"
GUI=data/official/FT8132Debugger_V1.0.6
# skip the sensor-mode dialog; do not pass -q (that Finds then default -act2 Verify)
timeout 2m xvfb-run -a -s "-screen 0 1600x900x24" \
  env WINEPREFIX="$WINEPREFIX" WINEARCH="$WINEARCH" wine "$GUI/FT8132Debugger.exe" -senmode 2 -curtp 0
```

What this run showed:

| Layer | Result |
|-------|--------|
| CLI `bench find` | `chip_id` 2, `comm_status` 0, `bflash_111` 0 |
| WPF | Main window `FT8132Debugger V1.0.6` paints (WineD3D) |
| `wineusb` | Opens `1b3f:8000` (chmod the usbfs node) |
| `mountmgr` + udisks2 | `\Device\Harddisk1` for `/dev/sdd` (`GENPLUS USB-MSDC DISK A`, size 0) |
| `FTechUsbWrapperDllU` | `SetupDiGetClassDevs(GUID_DEVINTERFACE_DISK / CDROM)` returns **no interfaces** → status **Disconnected**, Find never enables |

The wrapper enumerates SCSI disks via SetupDi storage device interfaces, not raw wineusb handles. Wine 10 never registers `GUID_DEVINTERFACE_DISK` for this no-medium gadget, so the official EXE cannot Find here. Do not unbind `usb-storage` to feed wineusb; that would drop `/dev/sgN` and would not help SetupDi.

## libvirt Windows 10 (measured 2026-10-15)

Domain `win10` (guest `win10vm.lan` / `192.168.24.53`). Official V1.0.6 lives at `C:\ft8132\FT8132Debugger_V1.0.6\` (.NET 3.5 installed). Automate over **OpenSSH** as `ft8132`; there is no system Python — use guest `uv`.

Host CLI first (exclusive SCSI), then pass the Blue Pill into the VM:

```bash
uv run --project scripts bench chmod
uv run --project scripts bench find          # chip_id 2, comm_status 0, bflash_111 0
virsh -c qemu:///system attach-device win10 /tmp/ft8132-usb.xml --live   # 1b3f:8000
```

Windows shows `GENPLUS USB-MSDC DISK A`, size 0, **No Media** — do not format it. Guest SPTI INQUIRY is `GENPLUS ` / `USB-MSDC DISK A ` / `1.00` / `GP-PROD.`.

Host `usbmon` on bus 3 (`3:115`) shows why. Windows USBSTOR only polls `TEST UNIT READY` → CSW fail → `REQUEST_SENSE` **0x3A** (no medium), plus `INQUIRY` **36** bytes (no `GP-PROD.`). The wrapper’s fingerprint is `INQUIRY` **50** bytes (so bytes 36–43 are `GP-PROD.`) then vendor `F0 F0` auth. A Limited-integrity GUI never sends those two CDBs (`Device count: 0`) even though Disk Management already lists the GENPLUS drive — SetupDi/CreateFile never reaches SPTI. Elevated, those CDBs appear and count becomes 1.

The GUI process must run **elevated** in the interactive RDP/SPICE session (SSH session 0 cannot click WPF). `scripts/win_gui_probe.py` default is Find then Download-tab GenCode / Erase / Write / Verify; pass `--debug` for Find / Exit I2C / Find / Debug / Stop. `scripts/win_gui_find.py` is Find-only.

```bat
uv run --python 3.12 --with pywinauto --with pillow python C:\Users\ft8132\win_gui_probe.py
```

Launch that via a Scheduled Task (`/IT /RL HIGHEST`) so it shares the desktop. Start `FT8132Debugger.exe -senmode 2 -curtp 0`, then hotplug `1b3f:8000` if it was already attached (WndProc `WM_DEVICECHANGE`). Do not pass `-q` / `-auto` (that Finds then default `-act2` Verify). Exclusive SCSI: do not run `bench` and the GUI at the same time. Erase / Write / Verify are in-scope (`FD 20` is chip erase).

If Device Manager never lists `VID_1B3F&PID_8000`, D+ is pulled up but firmware is not answering SETUP — reflash and NRST via J-Link.

## SCSI matrix vs MainWindow.cs (measured 2026-10-15)

Host gadget on `/dev/sg2`, PSU on, USB **not** in `win10`. Runner: `scripts/probe_fw_test.py` (JSON: `data/probe_fw_test.json`). Parallel `usbmon` on bus 3 device 115.

| Step | C# / firmware | CDB | CLI result |
|------|---------------|-----|------------|
| Identity | USBSTOR vs wrapper INQUIRY; TUR no medium | INQUIRY 50 + 36; TUR | `GENPLUS ` / `USB-MSDC DISK A ` / `1.00` / `GP-PROD.`; TUR ASC **0x3A** |
| Auth | `usbdisk2.rs` ~122 | `F0 F0` | resp `437e` |
| Find | `btn_ReadID_Click` ~7824 | `FD 00` knock, `FF FF` scan, `FD 28` `0x00`/`0x01`/`0x88` + 128 B bflash | `chip_id` **2**, `comm_status` **0**, `bflash[111]` **0** |
| State | `GetNandID(..., 64, 44)` ~7287 | **one** `FD 28` at `0x40` len 44 | 17 nonzero fields (`motor_status` 4, live `theta`/`udc`/currents). **Not** `F0 F8` |
| StopMotor | `StopMotor` ~8006 | `FD 2A` `0x02`/`0x03` = 0 | readback 0 |
| Exit I2C | `btn_WriteID_Click` ~7766 | `FF FF` OUT | ok |
| Find again | same as Find | same | `chip_id` 2, `comm` 0, `bflash[111]` 0 |
| Poke `0x04` | CLI `direction` RMW | `FD 28`/`FD 2A` at `0x04` | flipped then restored |

Matrix **ok**. Fail rules (chip id ≠ 2, scan comm ≠ 0, bflash[111] ≠ 0, all-zero state after Find) did not trip. usbmon CBWs are annotated with the same step names in the JSON.

## Elevated GUI probe (measured 2026-10-15)

`scripts/win_gui_probe.py` via schtask `/IT /RL HIGHEST` as `ft8132`. Screenshots + JSON: `data/gui-probe/`. Host `usbmon` during the click sequence: `data/gui-probe/usbmon.json`.

| Click | XAML | UI | usbmon |
|-------|------|----|--------|
| Find | `btn_ReadID` | ChipID **`0x02`**, I2CAddr `0x00`, **Device: Connected**, IDLE | `FD 00`, `FF FF` scan, `FD 28` id/bflash |
| Exit I2C | `btn_WriteID` | still Connected / IDLE; Debug/Stop/Test disabled | `FF FF` OUT |
| Find | `btn_ReadID` | ChipID **`0x02`**, Connected again | same as first Find |
| Debug | `btnDebug` / `ExecuteDebugOpt` ~4249 | **Device: Debugging...**, red **FAULT**, progress 100% | `FD 2A` ×115 (debug regs `0x86`–`0xB9` plus `0x02`/`0x03`/`0x04`/`0x06`/`0xF2`, twice because Stop also calls `ExecuteDebugOperation`); then GetNandID loop |
| Stop | `btnStartBw` / `ExecuteStopOperation` ~4541 | **Device: Pause...**, blue **PAUSE** | writes speed 0, cancels the read loop |

Find / Exit / Find **pass**. Debug **does write** ~57 ASIC registers (in scope). Gauges on the Sensorless tab stay at config defaults; live motor fields live on the Display tab (not captured). Status-bar FAULT after Debug is the default GUI parameter set hitting protection, not an identity failure.

### GetNandID: `F0 F8` stub vs `FD 28`

C# `ComputeFibonacci` calls `DLL_GPLB32DownloaderGetNandID(sn, start=64, len=44)`. That is **not** firmware `F0 F8`/`F0 F9` (those stubs never appeared on the wire).

| Path | CDB | This bench |
|------|-----|------------|
| Linux `bench state` / matrix | one `FD 28` at `0x40`, xfer **44** | live ASIC window (17 nonzero fields) |
| GUI Debug loop (wrapper) | **44×** `FD 28` xfer **1** at `0x40`…`0x6B` (~30 loops in 2.5 s) | same registers, bytewise; **0×** `F0 F8`, **0×** `F0 F9`, **0×** 44-byte burst |
| Firmware `F0 F8` / `F0 F9` | stub zeros | unused by this GUI |

## Results matrix (C# → CDB → firmware → CLI → GUI)

| Handler | CDB | Firmware | CLI (2026-10-15) | GUI (2026-10-15) |
|---------|-----|----------|------------------|------------------|
| INQUIRY / TUR | `12` len 50 vs 36; TUR | identity + sense `0x3A` | pass | wrapper INQUIRY 50 then `F0 F0`; USBSTOR also INQUIRY EVPD |
| Auth | `F0 F0` | implemented | pass | pass (usbmon) |
| `btn_ReadID_Click` ~7824 Find | `FD 00`, `FF FF`, `FD 28` | knock + I2C | chip 2 / comm 0 / bflash[111] 0 | ChipID `0x02`, Connected |
| `GetNandID` ~7287 | `FD 28` @ `0x40` | live `reg_read_n` | 44-byte burst, live fields | bytewise `FD 28` 0x40–0x6B (not `F0 F8`) |
| `btn_WriteID_Click` ~7766 Exit I2C | `FF FF` OUT | implemented | pass | Debug/Stop greyed; Connected |
| Find after exit | same as Find | implemented | pass | ChipID `0x02` |
| `ExecuteDebugOpt` ~4249 | `FD 2A` debug regs | implemented | — (CLI matrix skips) | Debugging… / FAULT after ~57 writes |
| `ExecuteStopOperation` / `StopMotor` ~8006 | `FD 2A` `0x02`/`0x03`=0 | implemented | readback 0 | Pause / PAUSE |
| direction poke `0x04` | `FD 28`/`2A` | implemented | restore ok | not in GUI pass |
| Erase / Write / Verify | `FD 20` + `FD 28`/`2A` acc=5 | chip erase + bit R/W | `bench mtp dump\|erase\|write\|verify` | see [GUI Download](#gui-download-erase--write--verify-measured-2026-10-15) |
| `F0 F8` / `F0 F9` | telemetry stub | **zeros** | unused | unused |

## eFuse / MTP (measured 2026-10-15)

Linux GenCode verify has not reached **`0x77eb`**. c2 alone at 512/640/768 is **`0xb61f`**. The frame at bit 384 erases that c2. This bench part now holds SDA low, so Find reads zeros. Ruled-out dialects and the firmware guard: [cflash.md](cflash.md).

GUI **Erase / Write / Verify** are the 1024-entry eFuse bit array (`m_byAdminREfuseBitArr`), not STM32 flash and not MSC READ/WRITE10. Wrapper CDBs (16-bit BE addr/len, `GP` at `[14:16]`):

| GUI | opt / acc | CDB | Addr / len | Data |
|-----|-----------|-----|------------|------|
| Erase (`ChipErase1`) | 40 / 5 | **`FD 20`**, no payload | ignored | none |
| Read abflash / bkflash | 5 / 5 | `FD 28` | 0 / 110 and 896 / 110 | IN, 1 byte per bit |
| Read cflash | 6 / 5 | `FD 28` | 128 / 374 and 502 / 374 | IN |
| Write bkflash / cflash | 133 / 5 | `FD 2A` | 896 / 110; 128 / 374; 502 / 374 | OUT |
| Find EEPROM dump | 5 / 5 | `FD 28` | **0 / 128** | IN **128 packed bytes** (`F6 00 00`) |
| Status | 1 / 2 | `FD 28`/`2A` | **244 (`0xF4`)** | 1 byte |

I2C (PulseView `write.sr` / `erase.sr` / `verify.sr`: **1 MS/s** analyzer, SCL **~77 kHz**, ~13 samples/clock — enough to decode. `find.sr` is 16 MS/s on the same ~78 kHz bus. Do not treat `samplerate=1 MHz` as 1 MHz SCL.)

Official **write** (`write.sr`, SDA sampled on SCL rise):

- Prime: select, `F6 00 00`, **read 110 bytes**.
- Erase: unlock `FE D2 F2`, `F6 00 00 0C`, wait ~78 ms, `F9` (read `0x10`), `F6 00 00 0E`, wait ~252 ms, `F9`, lock `FE D2`.
- **bkflash:** select, unlock, one `F6` @ 896 + 110× `02`/`03`, lock `FE D2` (**no `F9`**).
- **c1:** select only (**no unlock**), `F6` @ 128 / 256 / 384 (128+128+118). No echo. ~5 ms idle after each STOP.
- **c2:** after c1, **lone `FF C1` (~2.2 ms)**, then `FF C1 56` / `FF` / `03`, `F6` @ 502 / 630 / 758 (**no unlock, no `F9`**).
- Then verify reads (`F6`+addr, bulk read). The lone `FF C1` also appears after prime, erase, and bk.

The 502 CDB programs only the page-aligned c2 slice. The firmware refuses the frame at bit 384, a second slice, and a frame that crosses a 128-bit page. Per-CDB short select wipes this DUT (`0xf9be`). See [cflash.md](cflash.md).

Linux CLI (DUT VIN on, exclusive SCSI):

```bash
uv run --project scripts bench mtp dump -o data/mtp-before.bin
uv run --project scripts bench mtp erase
uv run --project scripts bench mtp dump -o data/mtp-erased.bin
uv run --project scripts bench mtp write --bin data/mtp-before.bin
uv run --project scripts bench mtp verify --bin data/mtp-before.bin
```

SG_IO timeout on MTP CDBs is 30 s. `dump`/`verify` report GUI `crc16` over 748 cflash bits from offset 128.

Live on this DUT: Find packed `F6` 128 B still works. `mtp_read_bits` retries an all-zero F6 chunk once (GUI Verify empty-window CRC `0xF9BE`). `FD 20` HV waits are spanned across USB polls (~80 ms + ~250 ms); a warm erase dumps **cflash ones 0 / crc `0xf9be`** (abflash 48 typically stays). Crossing 502 in one 128-byte frame wipes the array. GUI Download Write (2026-10-16) still read Chip CRC **`0xF9BE`** (empty). Golden `data/mtp-before.bin` still verifies because its 58 cflash 1s are all in c2.

Measured 2026-10-16 night through 2026-10-18 (Linux `mtp write --bin data/mtp-gencode.bin` after PSU cycle + Find):

| Dialect | crc | Notes |
|---------|-----|--------|
| Per-CDB long `FF C1 56` select, no lone `FF C1` | **`0x384a`** | c1 148 ones; c2 empty (62 holes from 514) |
| Lone `FF C1` after erase/bk/c1, or tight `FF C1`+short select on the 502 CDB | **`0xf9be`** | wipes c1; c2 never appears |
| Stash bk+c1; on 502 replay `write.sr` as one burst (`FF C1` 1.7/1.8 ms, no USB hole) | **`0x384a`** | c1 kept (no wipe); c2 128-byte `F6` @ 502 still empty |
| Same burst, program `F6`/`FF C1`/select at 13 µs/bit (6/7 µs, inter-byte low, mid-high ACK); fail 502 unless c2 status `0x03` | **`0x384a`** | write GOOD (c2 status was `0x03`); 62 holes from 514; Find `bflash` live (`ffff00ff…`, `bflash_111` 0) |
| Same burst plus a leading post-erase `FF C1`/`F4` (capture has that toggle before bk) | **`0xf9be`** | 502 CDB CHECK CONDITION; Find `bflash` all `00` until PSU cycle |
| c2 128-byte windows as the first cflash row | **`0x384a`** | 502+ frames never latch; later c1 windows do |
| c2-only 1-bit `F6`+`03`+echo+dummy+`F9` after bk | **`0xb61f`** | isolated c2 works; 148 c1 holes |
| c1 mixed then that c2 1-bit dialect | **`0x384a`** | c1 kept; c2 still empty |
| that c2 1-bit dialect then c1 mixed | **`0xf9be`** | wipes both |
| Per-CDB short select like `sub_20000C98` (`FF C1 56` / windows / `FF C1`; unlock only 896) | **`0xf9be`** | write GOOD; dump ones 0 (bk+cflash); Find `bflash` all `00` until PSU cycle |
| External 4.7 kΩ SCL/SDA to 5 V, both pins open-drain, internal pull-up off (2026-10-21) | **`0x384a`** | Find live (`ffff00ff…`, `bflash_111` 0); same 62 holes from 514. Pin high level ruled out |

Current firmware programs only page-aligned c2, and only after a healthy Find and an erase in the same knock session. Isolated c2 is **`0xb61f`**. GUI Verify cannot PASS until both halves store (**`0x77eb`**). Do not IRQ-mask a whole 16 ms window — that drops Windows USBSTOR. The part on this bench now holds SDA low; see [cflash.md](cflash.md).

## GUI Download Erase / Write / Verify (measured 2026-10-16)

Elevated `win_gui_probe.py` (no `--debug`) on V1.0.6. Screenshots + JSON: `data/gui-probe/`. GenCode src CRC **`0x77EB`** (210 cflash 1s: 148 in bits 128–501, 62 in 502–875). Linux image: `data/mtp-gencode.bin` (first 1024 bytes of the 1280-byte GenCode file).

The official wrapper always sets BOT `dCBWDataTransferLength` **512** for MTP `FD 28`/`2A`, while CDB `[7:8]` is the bit count (110 or 374). Firmware drains the full 512-byte OUT, then programs the CDB payload. `FD 20` / per-bit `FD 2A` waits are spanned so USB SOF stays alive.

| Click | XAML | UI | Linux / notes |
|-------|------|----|----------------|
| Find | `btn_ReadID` | ChipID **`0x02`**, Connected | `FD 00`, `FF FF`, `FD 28` id/bflash |
| Download tab | `tabItem_Download` | tab selected | — |
| Enable Writing Code Mode | `checkBox_EnableWritingMode` | checked | — |
| GenCode | `btn_Generate` | src CRC **`0x77EB`** | no SCSI |
| Erase | `btn_Erase` / `ChipErase1` | circle **PASS** | Linux `mtp erase` then dump: cflash ones **0**, crc **`0xf9be`** (~0.45 s) |
| Write | `btn_WriteEfuse` | circle **FAIL**, Chip CRC **`0xF9BE`** | GUI readback empty; Linux after PSU cycle + Find + two erases + same image: **`0x384a`** (c1 only) |
| Verify | `btn_Verify` | circle **FAIL**, Chip CRC **`0xF9BE`** | empty-window / blank cflash |

The 2026-10-21 5 V open-drain retest of the old full-image path was crc **`0x384a`** (c1 only, Find live). The firmware no longer sends that path. GUI Download Verify cannot PASS until a read is **`0x77eb`**. This bench part now holds SDA low.
