"""Typer CLI: same SCSI language as FT8132Debugger."""

from __future__ import annotations

import json
from pathlib import Path
from typing import Annotated, Optional

import typer

from .protocol import Protocol, ProtocolError
from .regs import (
    decode_ctrl,
    decode_reg,
    decode_slave_reg,
    decode_status,
    dump_json,
    encode_ctrl,
    encode_slave_id,
    encode_speed_duty,
    json_to_bytes,
)
from .scsi import ScsiError, ScsiProbe, discover_sg_devices

app = typer.Typer(no_args_is_help=True, add_completion=False)
reg_app = typer.Typer(no_args_is_help=True, help="Single-register get/set.")
regs_app = typer.Typer(no_args_is_help=True, help="Multi-register dump/load.")
app.add_typer(reg_app, name="reg")
app.add_typer(regs_app, name="regs")


def parse_int(value: str) -> int:
    s = value.strip().lower()
    if s.endswith("h") and s[:-1]:
        return int(s[:-1], 16)
    return int(s, 0)


def emit(obj: object) -> None:
    typer.echo(json.dumps(obj, indent=2, default=_json_default))


def _json_default(o: object) -> object:
    if isinstance(o, bytes):
        return o.hex()
    raise TypeError(type(o))


def open_protocol(
    device: str | None,
    skip_enter: bool,
    *,
    do_enter: bool = True,
    do_auth: bool = True,
) -> tuple[ScsiProbe, Protocol]:
    path = device
    if not path:
        found = discover_sg_devices()
        if not found:
            raise typer.BadParameter(
                "no probe found (VID 1b3f:8000). Pass --device /dev/sgN "
                "and check udev/sg permissions."
            )
        if len(found) > 1:
            raise typer.BadParameter(f"multiple probes: {found}; pass --device")
        path = found[0]
    scsi = ScsiProbe.open(path)
    try:
        scsi.check_inquiry()
        if do_auth:
            scsi.auth()
        proto = Protocol(scsi)
        if do_enter and not skip_enter:
            proto.enter()
        return scsi, proto
    except Exception:
        scsi.close()
        raise


DeviceOpt = Annotated[
    Optional[str],
    typer.Option("--device", "-d", help="SG device, e.g. /dev/sg2"),
]
SkipEnterOpt = Annotated[
    bool,
    typer.Option("--skip-enter", help="Do not send FD 00 knock/enter"),
]


@app.callback()
def _root() -> None:
    """FT8132 probe CLI (official SCSI CDBs over Linux SG_IO)."""


@app.command("list")
def list_devices() -> None:
    """List /dev/sg* nodes for VID 0x1B3F / PID 0x8000."""
    found = discover_sg_devices()
    emit({"devices": found})


@app.command()
def find(
    device: DeviceOpt = None,
) -> None:
    """GUI Find: knock, scan, chip id, slave id, 128-byte bflash."""
    scsi, proto = open_protocol(device, skip_enter=True, do_enter=False)
    try:
        info = proto.find()
        bflash = info.pop("bflash")
        info["bflash_len"] = len(bflash)
        info["bflash_111"] = bflash[111] if len(bflash) > 111 else None
        info["bflash_hex"] = bflash.hex()
        emit(info)
    finally:
        scsi.close()


@app.command()
def state(
    device: DeviceOpt = None,
    skip_enter: SkipEnterOpt = False,
) -> None:
    """Live telemetry (GetNandID): 44 bytes at 0x40."""
    scsi, proto = open_protocol(device, skip_enter)
    try:
        window = proto.read_status()
        emit(decode_status(window))
    finally:
        scsi.close()


@app.command()
def stop(
    device: DeviceOpt = None,
    skip_enter: SkipEnterOpt = False,
) -> None:
    """StopMotor: write 0x02/0x03 = 0 (does not re-download debug image)."""
    _set_speed(0, device, skip_enter)


@app.command()
def start(
    speed: Annotated[float, typer.Option("--speed", help="Duty 0..100")] = 30,
    device: DeviceOpt = None,
    skip_enter: SkipEnterOpt = False,
) -> None:
    """Set I2C speed command (same as `speed`). Debug image must already be loaded."""
    _set_speed(speed, device, skip_enter)


@app.command()
def speed(
    duty: Annotated[float, typer.Argument(help="Duty percent 0..100")],
    device: DeviceOpt = None,
    skip_enter: SkipEnterOpt = False,
) -> None:
    """Write speed command registers 0x02/0x03."""
    _set_speed(duty, device, skip_enter)


def _set_speed(duty: float, device: str | None, skip_enter: bool) -> None:
    hi, lo = encode_speed_duty(duty)
    scsi, proto = open_protocol(device, skip_enter)
    try:
        proto.write_reg(0x02, hi)
        proto.write_reg(0x03, lo)
        emit({"duty": duty, "0x02": hi, "0x03": lo})
    finally:
        scsi.close()


@app.command()
def direction(
    dir_: Annotated[int, typer.Argument(help="0 or 1 (byMotorDir, 0x04 bit 1)")],
    device: DeviceOpt = None,
    skip_enter: SkipEnterOpt = False,
) -> None:
    """RMW motor direction in 0x04 bit 1."""
    if dir_ not in (0, 1):
        raise typer.BadParameter("direction must be 0 or 1")
    scsi, proto = open_protocol(device, skip_enter)
    try:
        cur = proto.read_reg(0x04)
        new = encode_ctrl({"motor_dir": dir_}, cur)
        proto.write_reg(0x04, new)
        emit({"addr": 0x04, "old": cur, "new": new, "fields": decode_ctrl(new)})
    finally:
        scsi.close()


@app.command("slave-id")
def slave_id(
    sid: Annotated[int, typer.Argument(help="Logical ID 0..7 (GUI textBox_SlvID)")],
    device: DeviceOpt = None,
    skip_enter: SkipEnterOpt = False,
) -> None:
    """RMW 3-bit slave id in 0x88 (not the 7-bit I2C address 0x00)."""
    scsi, proto = open_protocol(device, skip_enter)
    try:
        cur = proto.read_reg(0x88)
        new = encode_slave_id(sid, cur)
        proto.write_reg(0x88, new)
        emit({"addr": 0x88, "old": cur, "new": new, "fields": decode_slave_reg(new)})
    finally:
        scsi.close()


@app.command()
def exit_i2c(
    device: DeviceOpt = None,
) -> None:
    """FF FF OUT: leave I2C / release pins."""
    scsi, proto = open_protocol(device, skip_enter=True, do_enter=False)
    try:
        proto.exit_i2c()
        emit({"ok": True})
    finally:
        scsi.close()


@regs_app.command("dump")
def regs_dump(
    json_path: Annotated[Optional[Path], typer.Option("--json")] = None,
    bin_path: Annotated[Optional[Path], typer.Option("--bin")] = None,
    start: Annotated[str, typer.Option("--from")] = "0x00",
    end: Annotated[str, typer.Option("--to")] = "0xff",
    device: DeviceOpt = None,
    skip_enter: SkipEnterOpt = False,
) -> None:
    """Dump a register range to JSON and/or a raw bin file."""
    lo = parse_int(start) & 0xFF
    hi = parse_int(end) & 0xFF
    if hi < lo:
        raise typer.BadParameter("--to must be >= --from")
    length = hi - lo + 1
    scsi, proto = open_protocol(device, skip_enter)
    try:
        blob = proto.read_regs(lo, length)
    finally:
        scsi.close()

    if bin_path:
        bin_path.write_bytes(blob)
    if json_path:
        json_path.write_text(json.dumps(dump_json(blob, lo), indent=2) + "\n")
    if not json_path and not bin_path:
        emit(dump_json(blob, lo))
    elif json_path or bin_path:
        emit({"wrote_json": str(json_path) if json_path else None,
              "wrote_bin": str(bin_path) if bin_path else None,
              "start": lo, "length": length})


@regs_app.command("load")
def regs_load(
    json_path: Annotated[Optional[Path], typer.Option("--json")] = None,
    bin_path: Annotated[Optional[Path], typer.Option("--bin")] = None,
    start: Annotated[str, typer.Option("--from")] = "0x00",
    device: DeviceOpt = None,
    skip_enter: SkipEnterOpt = False,
) -> None:
    """Write a JSON or bin dump back via FD 2A."""
    if bool(json_path) == bool(bin_path):
        raise typer.BadParameter("pass exactly one of --json / --bin")
    lo = parse_int(start) & 0xFF
    if bin_path:
        blob = bin_path.read_bytes()
    else:
        assert json_path is not None
        doc = json.loads(json_path.read_text())
        length = int(doc.get("length") or len(doc.get("raw_hex") or {}))
        start_doc = int(doc.get("start", lo))
        blob = json_to_bytes(doc, start_doc, length)
        lo = start_doc
    scsi, proto = open_protocol(device, skip_enter)
    try:
        proto.write_regs(lo, blob)
        emit({"wrote": len(blob), "start": lo})
    finally:
        scsi.close()


@reg_app.command("get")
def reg_get(
    addr: Annotated[str, typer.Argument(help="Register address (e.g. 0x04)")],
    raw: Annotated[bool, typer.Option("--raw")] = False,
    typed: Annotated[bool, typer.Option("--typed")] = False,
    device: DeviceOpt = None,
    skip_enter: SkipEnterOpt = False,
) -> None:
    """Read one register. Default: typed if mapped, else raw."""
    a = parse_int(addr) & 0xFF
    scsi, proto = open_protocol(device, skip_enter)
    try:
        extra: dict[int, int] = {}
        if a in (0x02, 0x03):
            extra[0x02] = proto.read_reg(0x02)
            extra[0x03] = proto.read_reg(0x03)
            value = extra[a]
        else:
            value = proto.read_reg(a)
        rec = decode_reg(a, value, extra=extra)
        want_raw = raw or (not typed and rec.get("fields") is None)
        if want_raw and not typed:
            emit({"addr": a, "raw": value})
        else:
            emit(rec)
    finally:
        scsi.close()


@reg_app.command("set")
def reg_set(
    addr: Annotated[str, typer.Argument()],
    value: Annotated[str, typer.Argument(help="Integer (--raw) or JSON object (--typed)")],
    raw: Annotated[bool, typer.Option("--raw")] = False,
    typed: Annotated[bool, typer.Option("--typed")] = False,
    device: DeviceOpt = None,
    skip_enter: SkipEnterOpt = False,
) -> None:
    """Write one register."""
    a = parse_int(addr) & 0xFF
    scsi, proto = open_protocol(device, skip_enter)
    try:
        if typed or (not raw and value.strip().startswith("{")):
            fields = json.loads(value)
            if a == 0x04:
                cur = proto.read_reg(0x04)
                proto.write_reg(a, encode_ctrl(fields, cur))
            elif a == 0x88:
                cur = proto.read_reg(0x88)
                if "slave_id" in fields:
                    proto.write_reg(a, encode_slave_id(int(fields["slave_id"]), cur))
                elif "raw" in fields:
                    proto.write_reg(a, int(fields["raw"]) & 0xFF)
                else:
                    raise typer.BadParameter("typed 0x88 needs slave_id or raw")
            elif a in (0x02, 0x03) and "duty" in fields:
                hi, lo = encode_speed_duty(float(fields["duty"]))
                proto.write_reg(0x02, hi)
                proto.write_reg(0x03, lo)
            elif "raw" in fields:
                proto.write_reg(a, int(fields["raw"]) & 0xFF)
            else:
                raise typer.BadParameter(f"no typed encoder for 0x{a:02x}")
        else:
            proto.write_reg(a, parse_int(value) & 0xFF)
        emit({"addr": a, "raw": proto.read_reg(a)})
    finally:
        scsi.close()


def main() -> None:
    try:
        app()
    except (ScsiError, ProtocolError, OSError, ValueError) as e:
        typer.echo(f"error: {e}", err=True)
        raise SystemExit(1) from e


if __name__ == "__main__":
    main()
