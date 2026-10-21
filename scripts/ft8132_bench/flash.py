"""Build and probe-rs download + reset."""

from __future__ import annotations

from . import paths
from .proc import run


def flash(*, mock: bool = False) -> None:
    cargo = ["cargo", "build", "--release"]
    if mock:
        cargo.extend(["--features", "mock-asic"])
    run(cargo, cwd=paths.REPO_ROOT, timeout=300)
    run(
        [
            "probe-rs",
            "download",
            "--chip",
            paths.CHIP,
            "--protocol",
            "swd",
            "--verify",
            "--non-interactive",
            str(paths.ELF),
        ],
        cwd=paths.REPO_ROOT,
        timeout=120,
    )
    run(
        [
            "probe-rs",
            "reset",
            "--chip",
            paths.CHIP,
            "--protocol",
            "swd",
            "--non-interactive",
        ],
        cwd=paths.REPO_ROOT,
        timeout=20,
    )
