"""USB MSC sg node for the Blue Pill probe."""

from __future__ import annotations

import os
import stat
import subprocess
import time
from pathlib import Path

from ft8132.scsi import discover_sg_devices

from .errors import BenchError


def wait_sg(*, timeout_s: float = 8.0) -> str:
    deadline = time.monotonic() + timeout_s
    while True:
        found = discover_sg_devices()
        if found:
            return found[0]
        if time.monotonic() >= deadline:
            raise BenchError("no 1b3f:8000 sg node (is firmware enumerated?)")
        time.sleep(0.2)


def ensure_writable(path: str) -> Path:
    p = Path(path)
    if os.access(p, os.W_OK):
        return p
    proc = subprocess.run(["sudo", "chmod", "a+rw", str(p)], check=False)
    if proc.returncode != 0 or not os.access(p, os.W_OK):
        mode = stat.filemode(p.stat().st_mode)
        raise BenchError(f"cannot chmod {p} ({mode}); need sudo or udev")
    return p
