"""WeAct PDPowerMiniV1 USB CDC (protocol v1.2)."""

from __future__ import annotations

import os
import time
from dataclasses import dataclass
from types import TracebackType
from typing import Self

import serial
from serial.tools import list_ports

from . import paths
from .errors import BenchError

END = 0x0A
CMD_READ = 0x80
CMD_WHO = 0x01
CMD_EN = 0x02
CMD_DATA = 0x04
CMD_DISP = 0x05
CMD_IN = 0x0A
CMD_VER = 0x42
CMD_SN = 0x43


class PsuError(BenchError):
    pass


def _u16(lo: int, hi: int) -> int:
    return lo | (hi << 8)


def _text_payload(raw: bytes) -> str:
    body = raw[1:] if raw else b""
    if body.endswith(b"\n"):
        body = body[:-1]
    return body.decode("utf-8", "replace")


def find_tty(serial_no: str | None = None) -> str:
    env = os.environ.get("PSU_TTY")
    if env:
        return env
    want = serial_no or os.environ.get("PSU_SERIAL", paths.PSU_SERIAL)
    for port in list_ports.comports():
        if port.serial_number == want:
            return port.device
        if port.vid == paths.PSU_VID and port.pid == paths.PSU_PID:
            return port.device
    raise PsuError("PDPowerMiniV1 TTY not found (set PSU_TTY)")


@dataclass(frozen=True)
class PsuStatus:
    who: str
    version: str
    serial: str
    enable: int | None
    set_mV: int | None
    set_mA: int | None
    meas_mV: int | None
    meas_mA: int | None
    input_state: int | None
    input_mV: int | None
    pd_V: float | None

    def as_dict(self) -> dict[str, object]:
        return {
            "who": self.who,
            "version": self.version,
            "serial": self.serial,
            "enable": self.enable,
            "set_mV": self.set_mV,
            "set_mA": self.set_mA,
            "meas_mV": self.meas_mV,
            "meas_mA": self.meas_mA,
            "input_state": self.input_state,
            "input_mV": self.input_mV,
            "pd_V": self.pd_V,
        }


class PdPowerMini:
    def __init__(self, port: str, baud: int = paths.PSU_BAUD, timeout: float = 0.3) -> None:
        self._ser = serial.Serial(port, baud, timeout=timeout)
        time.sleep(0.05)

    def close(self) -> None:
        if self._ser.is_open:
            self._ser.close()

    def __enter__(self) -> Self:
        return self

    def __exit__(
        self,
        exc_type: type[BaseException] | None,
        exc: BaseException | None,
        tb: TracebackType | None,
    ) -> None:
        self.close()

    def _query(self, pkt: bytes, n: int = 64, wait: float = 0.2) -> bytes:
        self._ser.reset_input_buffer()
        self._ser.write(pkt)
        self._ser.flush()
        time.sleep(wait)
        return self._ser.read(n)

    def set_enable(self, on: bool) -> None:
        self._ser.write(bytes([CMD_EN, 0x01 if on else 0x00, END]))
        self._ser.flush()
        time.sleep(0.2)

    def status(self) -> PsuStatus:
        who = self._query(bytes([CMD_WHO | CMD_READ, END]))
        ver = self._query(bytes([CMD_VER | CMD_READ, END]))
        sn = self._query(bytes([CMD_SN | CMD_READ, END]))
        en = self._query(bytes([CMD_EN | CMD_READ, END]))
        st = self._query(bytes([CMD_DATA | CMD_READ, 0x00, END]))
        meas = self._query(bytes([CMD_DISP | CMD_READ, END]))
        inn = self._query(bytes([CMD_IN | CMD_READ, END]))
        return PsuStatus(
            who=_text_payload(who),
            version=_text_payload(ver),
            serial=_text_payload(sn),
            enable=en[1] if len(en) > 1 else None,
            set_mV=_u16(st[2], st[3]) if len(st) >= 6 else None,
            set_mA=_u16(st[4], st[5]) if len(st) >= 6 else None,
            meas_mV=_u16(meas[1], meas[2]) if len(meas) >= 5 else None,
            meas_mA=_u16(meas[3], meas[4]) if len(meas) >= 5 else None,
            input_state=inn[1] if len(inn) > 1 else None,
            input_mV=_u16(inn[2], inn[3]) if len(inn) >= 6 else None,
            pd_V=_u16(inn[4], inn[5]) / 10.0 if len(inn) >= 6 else None,
        )
