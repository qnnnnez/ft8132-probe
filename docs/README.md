# Docs

- [Workbench](workbench.md) — bench on host `n3150` (Blue Pill, FT8132, J-Link, WeAct PDPowerMiniV1).
- [Official GUI](gui.md) — how to exercise `usbdisk2` against Fortior **FT8132Debugger**.
- [Probe firmware](probe-fw.md) — `ft8132Debugger.bin` on the Generalplus dongle (boot script, SCSI dispatch, IDA ELF).
- [cflash](cflash.md) — GenCode `0x77eb` not reached. c2 alone at 512 is `0xb61f`. The frame at bit 384 erases c2. This bench part now holds SDA low.
- [../scripts/README.md](../scripts/README.md) — `bench` CLI (PSU / flash / Find).

## Files that are not in git

These paths are gitignored. Nothing here is a second copy of the Fortior installer.

| Path | How to get it |
|------|----------------|
| `data/official/FT8132Debugger_V1.0.6/` | Download [FT8132Debugger V1.0.6](https://download.fortiortech.com/download?file=files/20260427/FT8132Debugger_V1.0.6.rar&attname=FT8132Debugger_V1.0.6.rar) and extract the RAR here (`unrar`, RAR5). The EXE, `FTechUsbWrapperDllU.dll`, `conf/FT8132Debugger.conf`, and `code/ft8132Debugger.bin` are inside that archive. |
| `data/FTechUsbWrapperDllU.dll` | Copy `FTechUsbWrapperDllU.dll` out of the extracted archive. |
| `data/ft-debugger-src/` | Local .NET decompile of `FT8132Debugger.exe`. Open that EXE in a decompiler and export it. Do not commit the export. |
| `data/official/.../code/ft8132Debugger.elf` and `ft8132-probe-fw.elf.i64` | The `.bin` is in the archive. Build the ELF with the commands in [probe-fw.md](probe-fw.md), then open it in IDA and save the `.i64` next to it. |
| `data/FTechUsbWrapperDllU.dll.i64` | IDA database for the DLL above. Create it by loading that DLL in IDA. |
| `data/mtp-gencode.bin` | First 1024 bytes of `UserConfigFile/FT8132_20250923_114955.bin` in the V1.0.6 archive (the file is 1280 bytes). `dd if=FT8132_20250923_114955.bin of=data/mtp-gencode.bin bs=1024 count=1` |
| `data/mtp-*.bin`, `scripts/data/` | Live chip images. `uv run --project scripts bench mtp dump -o data/mtp-before.bin` |
| `data/gui-probe/` | Screenshots and JSON from `scripts/win_gui_probe.py` on a Windows session that has the official GUI installed. |
| `data/probe_fw_test.json` | Output of `scripts/probe_fw_test.py`. |

`data/pulseview/` and `data/mock/` are in git. The mock bins are cut from those captures by `scripts/extract_mock.py`.
