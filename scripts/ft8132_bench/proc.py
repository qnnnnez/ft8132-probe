"""Subprocess helpers (no shell)."""

from __future__ import annotations

import subprocess
from collections.abc import Sequence
from pathlib import Path

import typer


def run(cmd: Sequence[str], *, cwd: Path, timeout: float) -> None:
    try:
        proc = subprocess.run(
            list(cmd),
            cwd=cwd,
            timeout=timeout,
            check=False,
        )
    except FileNotFoundError as e:
        typer.echo(f"error: missing executable: {e.filename}", err=True)
        raise typer.Exit(1) from e
    except subprocess.TimeoutExpired as e:
        typer.echo(f"error: timed out after {timeout:.0f}s: {' '.join(cmd)}", err=True)
        raise typer.Exit(124) from e
    if proc.returncode != 0:
        raise typer.Exit(proc.returncode)
