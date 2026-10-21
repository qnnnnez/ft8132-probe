"""Parse Linux usbmon 'Nu' text and list MSC CBW CDBs."""

from __future__ import annotations

import argparse
import pathlib
import sys

CBW_SIG = bytes.fromhex("55534243")
CSW_SIG = bytes.fromhex("55534253")

OP = {
    0x00: "TUR",
    0x03: "REQUEST_SENSE",
    0x12: "INQUIRY",
    0x1A: "MODE_SENSE6",
    0x1E: "PREVENT_ALLOW",
    0x23: "READ_FORMAT_CAP",
    0x25: "READ_CAPACITY",
    0x28: "READ10",
    0x2A: "WRITE10",
    0x5A: "MODE_SENSE10",
    0x85: "ATA_PASS",
    0xF0: "VENDOR_F0",
    0xFD: "VENDOR_FD",
    0xFF: "VENDOR_FF",
}


def hex_to_bytes(s: str) -> bytes:
    return bytes.fromhex(s.replace(" ", "").replace("=", ""))


def parse(path: pathlib.Path, addr_substr: str) -> list[dict]:
    events: list[dict] = []
    for line in path.read_text(errors="replace").splitlines():
        if addr_substr not in line:
            continue
        parts = line.split()
        if len(parts) < 4:
            continue
        rec: dict = {
            "ts": parts[1],
            "dir": parts[2],
            "type": parts[3],
            "raw": line,
        }
        if "=" in line:
            data = hex_to_bytes(line.split("=", 1)[1].strip())
            rec["data"] = data.hex()
            rec["n"] = len(data)
            if data.startswith(CBW_SIG) and len(data) >= 31:
                cdb_len = data[14]
                cdb = data[15 : 15 + max(cdb_len, 16)]
                rec["kind"] = "CBW"
                rec["tag"] = data[4:8].hex()
                rec["xfer"] = int.from_bytes(data[8:12], "little")
                rec["cdb"] = cdb[:cdb_len].hex(" ")
                rec["op"] = OP.get(cdb[0], f"0x{cdb[0]:02X}")
                rec["cdb0"] = cdb[0]
                rec["cdb1"] = cdb[1] if len(cdb) > 1 else 0
            elif data.startswith(CSW_SIG) and len(data) >= 13:
                rec["kind"] = "CSW"
                rec["status"] = data[12]
            elif rec["dir"].startswith("C") and "Bi" in rec["type"]:
                rec["kind"] = "IN"
        events.append(rec)
    return events


def _cdb_bytes(e: dict) -> bytes:
    return bytes.fromhex(e["cdb"].replace(" ", ""))


def vendor_cbws(events: list[dict]) -> list[dict]:
    out = []
    for e in events:
        if e.get("kind") != "CBW":
            continue
        if e.get("cdb0") in (0xF0, 0xFD, 0xFF) or (
            e.get("cdb0") == 0x12 and e.get("xfer", 0) >= 44
        ):
            rec = {
                "op": e["op"],
                "cdb": e["cdb"],
                "xfer": e["xfer"],
                "ts": e["ts"],
                "cdb0": e["cdb0"],
                "cdb1": e.get("cdb1"),
            }
            b = _cdb_bytes(e)
            rec["addr"] = b[5] if len(b) > 5 else None
            out.append(rec)
    return out


def annotate_cli_cbws(cbws: list[dict]) -> list[dict]:
    """Label CLI-matrix vendor CBWs with the matching MainWindow.cs step."""
    find_pass = 0
    labeled: list[dict] = []
    for e in cbws:
        b = _cdb_bytes(e)
        op = b[0]
        sub = b[1] if len(b) > 1 else 0
        addr = b[5] if len(b) > 5 else 0
        xfer = e.get("xfer", 0)
        step = "other"
        src = ""
        if op == 0x12:
            step, src = "identity", "INQUIRY (wrapper 50 / USBSTOR 36)"
        elif op == 0xF0 and sub == 0xF0:
            step, src = "auth", "usbdisk2.rs ~122 F0 F0"
        elif op == 0xF0 and sub == 0xF8:
            step, src = "getnand_stub", "F0 F8 telemetry stub (zeros)"
        elif op == 0xF0 and sub == 0xF9:
            step, src = "getnand_stub", "F0 F9 telemetry stub (zeros)"
        elif op == 0xFD and sub == 0x00:
            find_pass += 1
            step = "find" if find_pass == 1 else "find_again"
            src = "MainWindow.btn_ReadID_Click ~7824 FD 00 knock"
        elif op == 0xFF:
            if xfer == 0:
                step, src = "exit_i2c", "MainWindow.btn_WriteID_Click ~7766 FF FF OUT"
            else:
                step = "find" if find_pass <= 1 else "find_again"
                src = "btn_ReadID_Click FF FF scan comm==0"
        elif op == 0xFD and sub == 0x28:
            if addr == 0x40 and xfer >= 44:
                step, src = "state", "GetNandID start=64 len=44 ~7287 via FD 28"
            elif addr in (0x02, 0x03):
                step, src = "stop", "StopMotor ~8006 readback"
            elif addr == 0x04:
                step, src = "poke_0x04", "direction RMW 0x04"
            else:
                step = "find" if find_pass <= 1 else "find_again"
                src = f"btn_ReadID_Click FD 28 addr=0x{addr:02x} xfer={xfer}"
        elif op == 0xFD and sub == 0x2A:
            if addr in (0x02, 0x03):
                step, src = "stop", "StopMotor write 0x02/0x03=0 ~8006"
            elif addr == 0x04:
                step, src = "poke_0x04", "direction write 0x04"
            else:
                step, src = "debug_regs", "ExecuteDebugOpt FD 2A"
        rec = dict(e)
        rec["step"] = step
        rec["src"] = src
        labeled.append(rec)
    return labeled


def classify_getnandid(cbws: list[dict]) -> dict:
    """Whether GetNandID used the F0 F8 stub, a 44-byte FD 28 burst, or bytewise FD 28."""
    f0f8 = [e for e in cbws if e.get("cdb0") == 0xF0 and e.get("cdb1") == 0xF8]
    f0f9 = [e for e in cbws if e.get("cdb0") == 0xF0 and e.get("cdb1") == 0xF9]
    fd28_40_burst = [
        e
        for e in cbws
        if e.get("cdb0") == 0xFD
        and e.get("cdb1") == 0x28
        and e.get("addr") == 0x40
        and e.get("xfer", 0) >= 44
    ]
    fd28_40_byte = [
        e
        for e in cbws
        if e.get("cdb0") == 0xFD
        and e.get("cdb1") == 0x28
        and isinstance(e.get("addr"), int)
        and 0x40 <= e["addr"] <= 0x6B
        and e.get("xfer", 0) == 1
    ]
    fd2a = [e for e in cbws if e.get("cdb0") == 0xFD and e.get("cdb1") == 0x2A]
    if f0f8:
        path = "F0 F8 stub"
    elif fd28_40_burst:
        path = "FD 28 @ 0x40 burst"
    elif fd28_40_byte:
        path = "FD 28 bytewise 0x40-0x6B"
    else:
        path = "none"
    return {
        "path": path,
        "f0_f8": len(f0f8),
        "f0_f9": len(f0f9),
        "fd28_at_0x40_burst": len(fd28_40_burst),
        "fd28_at_0x40_bytewise": len(fd28_40_byte),
        "fd2a_writes": len(fd2a),
    }


def summarize(events: list[dict]) -> None:
    cbws = [e for e in events if e.get("kind") == "CBW"]
    print(f"lines={len(events)} cbw={len(cbws)}")
    for e in cbws:
        if e.get("cdb0") in (0x00, 0x03, 0x1A, 0x1E, 0x23, 0x25, 0x85):
            continue
        print(f"  t={e['ts']} {e['op']:16} cdb={e['cdb']} xfer={e['xfer']}")


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("path")
    ap.add_argument("--addr", default=":115:")
    ap.add_argument("--annotate-cli", action="store_true")
    args = ap.parse_args()
    events = parse(pathlib.Path(args.path), args.addr)
    summarize(events)
    cbws = vendor_cbws(events)
    g = classify_getnandid(cbws)
    print(f"getnandid path={g['path']} F0F8={g['f0_f8']} F0F9={g['f0_f9']} FD28@40={g['fd28_at_0x40']} FD2A={g['fd2a_writes']}")
    if args.annotate_cli:
        for e in annotate_cli_cbws(cbws):
            print(f"  [{e['step']:12}] {e['op']:16} cdb={e['cdb']} xfer={e['xfer']}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
