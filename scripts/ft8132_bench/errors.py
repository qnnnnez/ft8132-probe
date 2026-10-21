"""Workbench failures (PSU, USB, flash)."""


class BenchError(RuntimeError):
    """A recoverable bench-side error; the CLI prints `error: …` and exits 1."""
