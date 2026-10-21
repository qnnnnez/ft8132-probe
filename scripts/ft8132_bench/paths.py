"""Repo-root paths and bench identity."""

from __future__ import annotations

import os
from pathlib import Path

REPO_ROOT = Path(__file__).resolve().parents[2]
CHIP = os.environ.get("CHIP", "STM32F103C6")
ELF = Path(
    os.environ.get(
        "ELF",
        str(REPO_ROOT / "target" / "thumbv7m-none-eabi" / "release" / "ft8132-probe"),
    )
)

PSU_VID = 0x1A86
PSU_PID = 0xFE0C
PSU_SERIAL = "acdeb4545ce4"
PSU_BAUD = 115200
