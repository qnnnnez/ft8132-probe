"""Named / typed register map (GUI packing + GetNandID status window)."""

from __future__ import annotations

from typing import Any

STATUS_FIELDS: list[tuple[str, int, int]] = [
    # (name, offset in 0x40 window, width in bytes, big-endian)
    ("motor_status", 0, 1),
    ("fault", 1, 1),
    ("theta", 2, 2),
    ("speed", 4, 2),
    ("udc", 6, 2),
    ("ipd_position", 8, 2),
    ("wout", 10, 2),
    ("uq", 12, 2),
    ("ud", 14, 2),
    ("ia_max", 16, 2),
    ("ib_max", 18, 2),
    ("ic_max", 20, 2),
    ("ia_aref", 22, 2),
    ("ib_aref", 24, 2),
    ("ic_aref", 26, 2),
    ("emf", 28, 2),
    ("pwm_real", 30, 2),
    ("iq_fdb", 32, 2),
    ("tim_arr", 34, 2),
    ("trip", 36, 2),
    ("power", 38, 2),
    ("hall_omega", 40, 2),
    ("rsv", 42, 2),
]

REG_NAMES: dict[int, str] = {
    0x00: "ident",
    0x01: "chip_id",
    0x02: "speed_cmd_hi",
    0x03: "speed_cmd_lo",
    0x04: "ctrl",
    0x06: "fds",
    0x88: "slave_raw",
    0xF2: "dig_mon",
}

for name, off, _width in STATUS_FIELDS:
    REG_NAMES.setdefault(0x40 + off, f"status_{name}")


def be_u(data: bytes, off: int, width: int) -> int:
    return int.from_bytes(data[off : off + width], "big")


def encode_speed_duty(duty: float) -> tuple[int, int]:
    """GUI: u16 = (uint16)(40.96 * bySpdCtrl) << 4 → 0x02 / 0x03."""
    if duty < 0 or duty > 100:
        raise ValueError("speed duty must be 0..100")
    # C# `(ushort)(40.96 * duty) << 4` wraps at 100; clamp so 100 stays full-scale.
    raw = min(int(40.96 * duty) << 4, 0xFFFF)
    return (raw >> 8) & 0xFF, raw & 0xFF


def decode_speed_duty(hi: int, lo: int) -> float:
    raw = ((hi & 0xFF) << 8) | (lo & 0xFF)
    return (raw >> 4) / 40.96


def decode_ctrl(val: int) -> dict[str, int]:
    v = val & 0xFF
    return {
        "override": v & 1,
        "motor_dir": (v >> 1) & 1,
        "rsvd2": (v >> 2) & 1,
        "test_mode": (v >> 3) & 1,
        "pwm_en": (v >> 4) & 1,
        "dbg_md": (v >> 6) & 3,
        "raw": v,
    }


def encode_ctrl(fields: dict[str, Any], base: int = 0) -> int:
    v = base & 0xFF
    if "raw" in fields and len(fields) == 1:
        return int(fields["raw"]) & 0xFF

    def set_bits(offset: int, width: int, name: str) -> None:
        nonlocal v
        if name not in fields:
            return
        mask = ((1 << width) - 1) << offset
        v = (v & ~mask) | ((int(fields[name]) & ((1 << width) - 1)) << offset)

    set_bits(0, 1, "override")
    set_bits(1, 1, "motor_dir")
    set_bits(2, 1, "rsvd2")
    set_bits(3, 1, "test_mode")
    set_bits(4, 1, "pwm_en")
    set_bits(6, 2, "dbg_md")
    if "raw" in fields:
        v = int(fields["raw"]) & 0xFF
    return v


def decode_slave_reg(val: int) -> dict[str, int]:
    """0x88: packed resta/isd/la/slave_id/uvlo (arr[2] in CalculateParam)."""
    v = val & 0xFF
    return {
        "uvlo_dis": v & 1,
        "slave_id": (v >> 1) & 7,
        "la_md": (v >> 5) & 1,
        "isd_fw_md": (v >> 6) & 1,
        "resta_en": (v >> 7) & 1,
        "raw": v,
        "display": (v >> 1) & 0x7F,
    }


def encode_slave_id(slave_id: int, base: int) -> int:
    if slave_id < 0 or slave_id > 7:
        raise ValueError("slave-id must be 0..7 (GUI 3-bit field)")
    v = base & 0xFF
    v = (v & ~0x0E) | ((slave_id & 7) << 1)
    return v


def decode_status(window: bytes) -> dict[str, int]:
    if len(window) < 44:
        raise ValueError("status window must be 44 bytes")
    out: dict[str, int] = {}
    for name, off, width in STATUS_FIELDS:
        out[name] = be_u(window, off, width)
    out["raw_hex"] = window.hex()
    return out


def decode_reg(addr: int, value: int, *, extra: dict[str, int] | None = None) -> dict[str, Any]:
    extra = extra or {}
    rec: dict[str, Any] = {
        "addr": addr,
        "name": REG_NAMES.get(addr),
        "raw": value & 0xFF,
    }
    if addr == 0x04:
        rec["fields"] = decode_ctrl(value)
    elif addr == 0x88:
        rec["fields"] = decode_slave_reg(value)
    elif addr in (0x02, 0x03) and extra:
        hi = extra.get(0x02, value if addr == 0x02 else 0)
        lo = extra.get(0x03, value if addr == 0x03 else 0)
        rec["fields"] = {
            "duty": round(decode_speed_duty(hi, lo), 4),
            "raw16": (hi << 8) | lo,
        }
    elif addr in (0x00, 0x01):
        rec["fields"] = {"value": value & 0xFF}
    return rec


def dump_json(blob: bytes, start: int) -> dict[str, Any]:
    regs: dict[str, Any] = {}
    raw_hex: dict[str, str] = {}
    by_addr = { (start + i) & 0xFF: b for i, b in enumerate(blob) }
    for i, b in enumerate(blob):
        addr = (start + i) & 0xFF
        extra = {}
        if addr in (0x02, 0x03):
            extra = {0x02: by_addr.get(0x02, 0), 0x03: by_addr.get(0x03, 0)}
        rec = decode_reg(addr, b, extra=extra)
        regs[f"0x{addr:02x}"] = rec
        raw_hex[f"0x{addr:02x}"] = f"{b:02x}"
    out: dict[str, Any] = {"start": start, "length": len(blob), "regs": regs, "raw_hex": raw_hex}
    # Overlay status window if present.
    if all(((0x40 + i) & 0xFF) in by_addr for i in range(44)):
        window = bytes(by_addr[0x40 + i] for i in range(44))
        out["state"] = decode_status(window)
    return out


def json_to_bytes(doc: dict[str, Any], start: int, length: int) -> bytes:
    buf = bytearray(length)
    raw_hex = doc.get("raw_hex") or {}
    regs = doc.get("regs") or {}
    for i in range(length):
        addr = (start + i) & 0xFF
        key = f"0x{addr:02x}"
        if key in raw_hex:
            buf[i] = int(str(raw_hex[key]), 16)
        elif key in regs:
            rec = regs[key]
            if isinstance(rec, dict) and "raw" in rec:
                buf[i] = int(rec["raw"]) & 0xFF
            else:
                buf[i] = int(rec) & 0xFF
        else:
            raise KeyError(f"missing {key} in JSON dump")
    return bytes(buf)
