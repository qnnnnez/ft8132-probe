"""Workbench CLI: PSU, flash, chmod, and SCSI probe subcommands."""

from __future__ import annotations

import json
import time

import typer

from ft8132.cli import app as scsi_app
from ft8132.protocol import ProtocolError
from ft8132.scsi import ScsiError

from . import flash as flash_mod
from . import paths, usb
from .errors import BenchError
from .psu import PdPowerMini, find_tty

app = typer.Typer(no_args_is_help=True, add_completion=False)
psu_app = typer.Typer(no_args_is_help=True, help="WeAct PDPowerMiniV1 (does not change V/I setpoint).")
app.add_typer(psu_app, name="psu")


def _emit(obj: object) -> None:
    typer.echo(json.dumps(obj, indent=2))


def _open_psu() -> PdPowerMini:
    return PdPowerMini(find_tty())


@app.callback()
def _root() -> None:
    """n3150 workbench: PSU, flash, and FT8132 SCSI (same CDBs as FT8132Debugger)."""


@psu_app.command("status")
def psu_status() -> None:
    """Read enable, setpoint, and measured V/I."""
    with _open_psu() as psu:
        _emit(psu.status().as_dict())


@psu_app.command("on")
def psu_on() -> None:
    """Enable main output (existing setpoint)."""
    with _open_psu() as psu:
        psu.set_enable(True)
        _emit(psu.status().as_dict())


@psu_app.command("off")
def psu_off() -> None:
    """Disable main output."""
    with _open_psu() as psu:
        psu.set_enable(False)
        _emit(psu.status().as_dict())


@psu_app.command("cycle")
def psu_cycle(
    off_s: float = typer.Option(0.5, "--off-s", help="Seconds off before on."),
) -> None:
    """Power-cycle DUT VIN."""
    with _open_psu() as psu:
        psu.set_enable(False)
        time.sleep(off_s)
        psu.set_enable(True)
        time.sleep(0.4)
        _emit(psu.status().as_dict())


@app.command()
def flash(
    mock: bool = typer.Option(False, "--mock", help="Build --features mock-asic"),
) -> None:
    """cargo build --release, probe-rs download, reset (does not stay attached)."""
    flash_mod.flash(mock=mock)
    _emit({"elf": str(paths.ELF), "chip": paths.CHIP, "mock": mock})


@app.command("chmod")
def chmod_sg() -> None:
    """Wait for 1b3f:8000 sg node and chmod a+rw if needed."""
    sg = usb.wait_sg()
    path = usb.ensure_writable(sg)
    _emit({"device": str(path)})


def _share_scsi_cli() -> None:
    """Reuse host `ft8132` command registrations (list/find/state/reg/…) at top level."""
    app.registered_commands.extend(scsi_app.registered_commands)
    app.registered_groups.extend(scsi_app.registered_groups)


_share_scsi_cli()


def main() -> None:
    try:
        app()
    except (ScsiError, ProtocolError, BenchError, OSError, ValueError) as e:
        typer.echo(f"error: {e}", err=True)
        raise SystemExit(1) from e


if __name__ == "__main__":
    main()
