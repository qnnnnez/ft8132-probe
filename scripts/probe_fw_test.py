"""Linux SCSI matrix matching FT8132Debugger MainWindow.cs handlers.

Run on the host with the gadget NOT passed through to a VM:

    uv run --project scripts python scripts/probe_fw_test.py
    uv run --project scripts python scripts/probe_fw_test.py --usbmon /tmp/usb-cap/cli.mon
"""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
import time
from pathlib import Path

from ft8132 import INQ_MAGIC
from ft8132.protocol import CHIP_ID, Protocol, ProtocolError
from ft8132.regs import decode_ctrl, decode_status, encode_ctrl
from ft8132.scsi import ScsiError, ScsiProbe, cdb_inquiry, discover_sg_devices

REPO = Path(__file__).resolve().parents[1]


def _sense_asc(sense: bytes) -> int | None:
    if len(sense) < 13:
        return None
    if sense[0] & 0x7F in (0x70, 0x71):
        return sense[12]
    return None


def _open(device: str | None) -> ScsiProbe:
    path = device
    if not path:
        found = discover_sg_devices()
        if not found:
            raise SystemExit("no 1b3f:8000 sg node")
        if len(found) > 1:
            raise SystemExit(f"multiple probes: {found}")
        path = found[0]
    return ScsiProbe.open(path)


def step_identity(scsi: ScsiProbe) -> dict:
    """INQUIRY + TUR (no medium). Wrapper uses 50-byte INQUIRY; USBSTOR uses 36."""
    inq50 = scsi.sg_io(cdb_inquiry(50), data_in=50)
    inq36 = scsi.sg_io(cdb_inquiry(36), data_in=36)
    tur = {"ok": False, "asc": None, "error": None}
    try:
        scsi.sg_io(bytes([0, 0, 0, 0, 0, 0]))
        tur["error"] = "TUR passed (want CHECK CONDITION 0x3A)"
    except ScsiError as e:
        tur["asc"] = _sense_asc(e.sense)
        tur["ok"] = tur["asc"] == 0x3A
        tur["error"] = None if tur["ok"] else str(e)
    rec = {
        "src": "USBSTOR vs FTechUsbWrapperDllU INQUIRY; TUR sense 0x3A",
        "vendor": inq50[8:16].decode("ascii", "replace"),
        "product": inq50[16:32].decode("ascii", "replace"),
        "rev": inq50[32:36].decode("ascii", "replace"),
        "magic": inq50[36:44].decode("ascii", "replace") if len(inq50) >= 44 else "",
        "inq36_len": len(inq36),
        "tur": tur,
        "ok": (
            inq50[8:16] == b"GENPLUS "
            and inq50[16:32] == b"USB-MSDC DISK A "
            and inq50[36:44] == INQ_MAGIC
            and tur["ok"]
        ),
    }
    return rec


def step_auth(scsi: ScsiProbe) -> dict:
    """usbdisk2.rs handle_vendor F0 F0."""
    resp = scsi.auth()
    return {"src": "usbdisk2.rs auth F0 F0", "resp": resp.hex(), "ok": True}


def step_find(proto: Protocol, *, label: str) -> dict:
    """btn_ReadID_Click ~7824: FD 00, FF FF, 0x01==2, 0x88, bflash[111]==0."""
    info = proto.find()
    bflash = info.pop("bflash")
    rec = {
        "src": "MainWindow.btn_ReadID_Click ~7824",
        "label": label,
        "ident": info["ident"],
        "chip_id": info["chip_id"],
        "slave_raw": info["slave_raw"],
        "scan_slave": info["scan_slave"],
        "comm_status": info["comm_status"],
        "bflash_len": len(bflash),
        "bflash_111": bflash[111] if len(bflash) > 111 else None,
        "ok": (
            info["chip_id"] == CHIP_ID
            and info["comm_status"] == 0
            and len(bflash) == 128
            and bflash[111] == 0
        ),
    }
    return rec


def step_state(proto: Protocol) -> dict:
    """GetNandID(..., 64, 44) via FD 28 at 0x40 — not the F0 F8 stub."""
    window = proto.read_status()
    decoded = decode_status(window)
    nonzero = sum(1 for k, v in decoded.items() if k != "raw_hex" and v != 0)
    rec = {
        "src": "MainWindow.ComputeFibonacci GetNandID start=64 len=44 ~7287 (via FD 28)",
        "decoded": {k: v for k, v in decoded.items() if k != "raw_hex"},
        "raw_hex": decoded["raw_hex"],
        "nonzero_fields": nonzero,
        "ok": nonzero > 0,
    }
    return rec


def step_stop(proto: Protocol) -> dict:
    """ExecuteStopOperation / StopMotor write 0x02/0x03 = 0 ~8006."""
    proto.write_reg(0x02, 0)
    proto.write_reg(0x03, 0)
    hi, lo = proto.read_reg(0x02), proto.read_reg(0x03)
    return {
        "src": "MainWindow.StopMotor ~8006",
        "0x02": hi,
        "0x03": lo,
        "ok": hi == 0 and lo == 0,
    }


def step_exit(proto: Protocol) -> dict:
    """btn_WriteID_Click ~7766: FF FF OUT."""
    proto.exit_i2c()
    return {"src": "MainWindow.btn_WriteID_Click ~7766", "ok": True}


def step_poke04(proto: Protocol) -> dict:
    """Safe RMW of 0x04 motor_dir bit (direction command)."""
    old = proto.read_reg(0x04)
    flipped = encode_ctrl({"motor_dir": 1 - decode_ctrl(old)["motor_dir"]}, old)
    proto.write_reg(0x04, flipped)
    mid = proto.read_reg(0x04)
    proto.write_reg(0x04, old)
    rest = proto.read_reg(0x04)
    return {
        "src": "direction RMW 0x04 bit 1 (cli direction)",
        "old": old,
        "flipped": flipped,
        "readback_flip": mid,
        "restored": rest,
        "ok": rest == old and mid == flipped,
    }


def run_matrix(device: str | None) -> dict:
    report: dict = {"ok": True, "steps": {}}
    scsi = _open(device)
    try:
        report["device"] = scsi.path
        report["steps"]["identity"] = step_identity(scsi)
        report["steps"]["auth"] = step_auth(scsi)
        proto = Protocol(scsi)
        report["steps"]["find"] = step_find(proto, label="first")
        report["steps"]["state"] = step_state(proto)
        report["steps"]["stop"] = step_stop(proto)
        report["steps"]["exit_i2c"] = step_exit(proto)
        report["steps"]["find_again"] = step_find(proto, label="after exit")
        report["steps"]["poke_0x04"] = step_poke04(proto)
    except (ScsiError, ProtocolError, OSError, ValueError) as e:
        report["error"] = str(e)
        report["ok"] = False
    finally:
        scsi.close()

    for name, rec in report["steps"].items():
        if not rec.get("ok", False):
            report["ok"] = False
            report.setdefault("failed", []).append(name)
    return report


def maybe_usbmon(path: Path | None):
    if path is None:
        return None
    path.parent.mkdir(parents=True, exist_ok=True)
    mon = Path("/sys/kernel/debug/usb/usbmon/3u")
    try:
        if not mon.is_file():
            return {"error": f"missing {mon} (modprobe usbmon)"}
    except OSError as e:
        return {"error": f"usbmon not readable: {e}"}
    proc = subprocess.Popen(
        ["sudo", "-n", "timeout", "90s", "cat", str(mon)],
        stdout=path.open("w"),
        stderr=subprocess.DEVNULL,
    )
    time.sleep(0.3)
    if proc.poll() is not None:
        return {"error": "usbmon cat exited (need passwordless sudo or root)"}
    return proc


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--device")
    ap.add_argument("--usbmon", type=Path, help="usbmon 3u capture path")
    ap.add_argument("--json", type=Path, default=REPO / "data" / "probe_fw_test.json")
    args = ap.parse_args()

    cap = maybe_usbmon(args.usbmon)
    try:
        report = run_matrix(args.device)
    finally:
        if hasattr(cap, "terminate"):
            cap.terminate()
            try:
                cap.wait(timeout=3)
            except Exception:
                cap.kill()

    if args.usbmon and args.usbmon.is_file() and args.usbmon.stat().st_size > 0:
        sys.path.insert(0, str(Path(__file__).parent))
        from usbmon_parse import annotate_cli_cbws, classify_getnandid, parse, vendor_cbws

        events = parse(args.usbmon, ":115:")
        cbws = vendor_cbws(events)

        report["usbmon"] = {
            "path": str(args.usbmon),
            "vendor_cbws": annotate_cli_cbws(cbws),
            "getnandid": classify_getnandid(cbws),
        }
    elif isinstance(cap, dict):
        report["usbmon"] = cap

    args.json.parent.mkdir(parents=True, exist_ok=True)
    args.json.write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps(report, indent=2))
    return 0 if report.get("ok") else 1


if __name__ == "__main__":
    sys.exit(main())
