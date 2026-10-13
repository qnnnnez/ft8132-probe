#!/usr/bin/env python3
"""Decode PulseView .sr captures into mock-ASIC fixtures.

Host-only. Firmware includes the generated blobs via include_bytes!.
"""
from __future__ import annotations

import zipfile
from pathlib import Path

import numpy as np

ROOT = Path(__file__).resolve().parents[1]
PV = ROOT / "data" / "pulseview"
OUT = ROOT / "data" / "mock"
SDA_BIT, SCL_BIT = 1, 3


def load_sr(path: Path):
    z = zipfile.ZipFile(path)
    meta = z.read("metadata").decode()

    def key(n: str):
        parts = n.split("-")
        return tuple(int(p) if p.isdigit() else p for p in parts)

    names = sorted((n for n in z.namelist() if n.startswith("logic-")), key=key)
    data = b"".join(z.read(n) for n in names)
    rate = None
    for line in meta.splitlines():
        if line.startswith("samplerate="):
            s = line.split("=", 1)[1].strip()
            if s.endswith("MHz"):
                rate = float(s.replace("MHz", "")) * 1e6
            elif s.endswith("kHz"):
                rate = float(s.replace("kHz", "")) * 1e3
            elif s.endswith("Hz"):
                rate = float(s.replace("Hz", ""))
    return rate, np.frombuffer(data, dtype=np.uint8)


def i2c_txs(arr: np.ndarray):
    sda = (arr >> SDA_BIT) & 1
    scl = (arr >> SCL_BIT) & 1
    dsda = np.diff(sda.astype(np.int8))
    dscl = np.diff(scl.astype(np.int8))
    idx = np.arange(1, len(arr))
    starts = idx[(dsda == -1) & (scl[:-1] == 1)]
    stops = idx[(dsda == 1) & (scl[:-1] == 1)]
    scl_rise = idx[dscl == 1]
    events = sorted([(s, "S") for s in starts] + [(p, "P") for p in stops])
    i_rise = 0
    txs = []
    cur = None
    for t, kind in events:
        if kind == "S":
            if cur is None:
                cur = {"start": t}
            else:
                cur.setdefault("restarts", []).append(t)
        else:
            if cur is None:
                continue
            st = cur["start"]
            while i_rise < len(scl_rise) and scl_rise[i_rise] < st:
                i_rise += 1
            j = i_rise
            bits = []
            while j < len(scl_rise) and scl_rise[j] < t:
                bits.append(int(sda[scl_rise[j]]))
                j += 1
            data = []
            for off in range(0, len(bits) // 9 * 9, 9):
                val = 0
                for b in bits[off : off + 8]:
                    val = (val << 1) | b
                data.append(val)
            cur["bytes"] = data
            txs.append(cur)
            cur = None
            i_rise = j
    return txs


def replay_regs(txs):
    """Replay captured I2C onto a 256-byte file + extras.

    The F6 dump is EEPROM/bflash, not the live register file, so it is kept
    separate and does not overwrite chip ID / control regs.
    """
    regs = bytearray(256)
    ptr = 0
    bflash = bytearray(128)
    have_bflash = False
    pending_f6 = False
    last_ff_read = 0x03
    for tx in txs:
        b = tx.get("bytes") or []
        if len(b) < 1:
            continue
        rw = b[0] & 1
        payload = b[1:]
        if rw == 0:  # write
            if not payload:
                continue
            ptr = payload[0]
            data = payload[1:]
            if payload[:1] == [0xF6] and data[:2] == [0x00, 0x00]:
                pending_f6 = True
                continue
            # Skip command prefixes that are not normal data regs.
            if payload[0] in (0xFE, 0xFF, 0xF6, 0xF4, 0xF9):
                ptr = payload[0]
                continue
            for i, v in enumerate(data):
                addr = (ptr + i) & 0xFF
                regs[addr] = v
            if data:
                ptr = (ptr + len(data)) & 0xFF
        else:  # read
            if pending_f6 and len(payload) >= 128:
                bflash[:] = payload[:128]
                have_bflash = True
                pending_f6 = False
                continue
            for i, v in enumerate(payload):
                addr = (ptr + i) & 0xFF
                regs[addr] = v
                if addr == 0xFF:
                    last_ff_read = v
            if payload:
                ptr = (ptr + len(payload)) & 0xFF
    return regs, bytes(bflash) if have_bflash else None, last_ff_read


def main():
    OUT.mkdir(parents=True, exist_ok=True)

    _, find_arr = load_sr(PV / "find.sr")
    find_txs = i2c_txs(find_arr)
    regs, bflash, ff = replay_regs(find_txs)
    if bflash is None:
        raise SystemExit("find.sr: no 128-byte F6 dump")

    # ID / handshake regs from early Find traffic (also present in replay).
    print(f"find 0x00={regs[0]:02X} 0x01={regs[1]:02X} 0x04={regs[4]:02X} 0x88={regs[0x88]:02X}")
    print(f"find FF readback={ff:02X} bflash[0:8]={bflash[:8].hex()} bflash[111]={bflash[111]:02X}")

    _, debug_arr = load_sr(PV / "debug.sr")
    debug_txs = i2c_txs(debug_arr)
    debug_regs, _, _ = replay_regs(debug_txs)
    status = bytes(debug_regs[0x40 : 0x40 + 44])
    print(f"debug status@0x40 ({len(status)} B)={status[:16].hex()}...")

    (OUT / "find_regs.bin").write_bytes(bytes(regs))
    (OUT / "find_bflash.bin").write_bytes(bflash)
    (OUT / "debug_status.bin").write_bytes(status)
    (OUT / "ff_status.bin").write_bytes(bytes([ff]))
    print(f"wrote {OUT}")


if __name__ == "__main__":
    main()
