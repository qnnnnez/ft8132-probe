# Docs

- [Workbench](workbench.md) — bench on host `n3150` (Blue Pill, FT8132, J-Link, WeAct PDPowerMiniV1).
- [Official GUI](gui.md) — how to exercise `usbdisk2` against Fortior **FT8132Debugger**.
- [Probe firmware](probe-fw.md) — `ft8132Debugger.bin` on the Generalplus dongle (boot script, SCSI dispatch, IDA ELF).
- [cflash](cflash.md) — GenCode `0x77eb` not reached. c2 alone at 512 is `0xb61f`. The frame at bit 384 erases c2. This bench part now holds SDA low.
- [../scripts/README.md](../scripts/README.md) — `bench` CLI (PSU / flash / Find).
