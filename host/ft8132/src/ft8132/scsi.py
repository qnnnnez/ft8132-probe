"""Linux SG_IO transport and official vendor CDB builders."""

from __future__ import annotations

import ctypes
import fcntl
import os
import secrets
from pathlib import Path

from . import INQ_MAGIC, USB_PID, USB_VID

SG_IO = 0x2285
SG_DXFER_NONE = -1
SG_DXFER_TO_DEV = -2
SG_DXFER_FROM_DEV = -3

INQUIRY = 0x12
SENSE_LEN = 32
DEFAULT_TIMEOUT_MS = 10_000
MTP_TIMEOUT_MS = 30_000


class SgIoHdr(ctypes.Structure):
    _fields_ = [
        ("interface_id", ctypes.c_int),
        ("dxfer_direction", ctypes.c_int),
        ("cmd_len", ctypes.c_ubyte),
        ("mx_sb_len", ctypes.c_ubyte),
        ("iovec_count", ctypes.c_ushort),
        ("dxfer_len", ctypes.c_uint),
        ("dxferp", ctypes.c_void_p),
        ("cmdp", ctypes.c_void_p),
        ("sbp", ctypes.c_void_p),
        ("timeout", ctypes.c_uint),
        ("flags", ctypes.c_uint),
        ("pack_id", ctypes.c_int),
        ("usr_ptr", ctypes.c_void_p),
        ("status", ctypes.c_ubyte),
        ("masked_status", ctypes.c_ubyte),
        ("msg_status", ctypes.c_ubyte),
        ("sb_len_wr", ctypes.c_ubyte),
        ("host_status", ctypes.c_ushort),
        ("driver_status", ctypes.c_ushort),
        ("resid", ctypes.c_int),
        ("duration", ctypes.c_uint),
        ("info", ctypes.c_uint),
    ]


class ScsiError(RuntimeError):
    def __init__(self, message: str, status: int = 0, sense: bytes = b""):
        super().__init__(message)
        self.status = status
        self.sense = sense


def _gp_cdb() -> bytearray:
    cdb = bytearray(16)
    cdb[14:16] = b"GP"
    return cdb


def cdb_auth(challenge: bytes | None = None) -> bytes:
    cdb = _gp_cdb()
    cdb[0] = 0xF0
    cdb[1] = 0xF0
    rnd = challenge if challenge is not None else secrets.token_bytes(7)
    if len(rnd) != 7:
        raise ValueError("auth challenge must be 7 bytes")
    cdb[2:9] = rnd
    return bytes(cdb)


def expected_auth(cdb: bytes) -> bytes:
    total = sum(cdb[2:9]) ^ 0x4750
    return total.to_bytes(2, "big")


def cdb_enter(*, opt: int = 49, acc: int = 2) -> bytes:
    cdb = _gp_cdb()
    cdb[0] = 0xFD
    cdb[1] = 0x00
    cdb[6] = acc
    cdb[7] = 6 if opt == 49 else 7
    cdb[9] = opt
    return bytes(cdb)


def cdb_scan(*, opt: int = 4, val0: int = 4, val1: int = 0) -> bytes:
    """I2C scan: CDB[6]=optType, CDB[9]=val[0], CDB[10]=val[1] (Find uses 4, 0)."""
    cdb = _gp_cdb()
    cdb[0] = 0xFF
    cdb[1] = 0xFF
    cdb[6] = opt
    cdb[9] = val0
    cdb[10] = val1
    return bytes(cdb)


def cdb_exit(*, val0: int = 0, val1: int = 0) -> bytes:
    cdb = _gp_cdb()
    cdb[0] = 0xFF
    cdb[1] = 0xFF
    cdb[9] = val1
    cdb[10] = val0
    return bytes(cdb)


def cdb_lab_load(offset: int, length: int) -> bytes:
    """FE 10: copy OUT bytes into the Blue Pill SRAM image. No I2C."""
    cdb = _gp_cdb()
    cdb[0] = 0xFE
    cdb[1] = 0x10
    cdb[4:6] = int(offset).to_bytes(2, "big")
    cdb[7:9] = int(length).to_bytes(2, "big")
    return bytes(cdb)


def cdb_lab_read(offset: int, length: int) -> bytes:
    """FE 11: read the SRAM image back."""
    cdb = _gp_cdb()
    cdb[0] = 0xFE
    cdb[1] = 0x11
    cdb[4:6] = int(offset).to_bytes(2, "big")
    cdb[7:9] = int(length).to_bytes(2, "big")
    return bytes(cdb)


def cdb_lab_clamp(low: bool) -> bytes:
    """FE 30: drive SCL/SDA low, or release them when low is false."""
    cdb = _gp_cdb()
    cdb[0] = 0xFE
    cdb[1] = 0x30
    cdb[2] = 1 if low else 0
    return bytes(cdb)


def cdb_lab_run(op: int, gap_us: int) -> bytes:
    """FE 20: program the SRAM image. gap_us is the idle between slices."""
    cdb = _gp_cdb()
    cdb[0] = 0xFE
    cdb[1] = 0x20
    cdb[2] = op & 0xFF
    cdb[4:6] = int(gap_us).to_bytes(2, "big")
    return bytes(cdb)


def cdb_mtp_erase(*, acc: int = 5, opt: int = 40) -> bytes:
    """FD 20: chip erase (GUI ChipErase1 / write-path erase)."""
    cdb = _gp_cdb()
    cdb[0] = 0xFD
    cdb[1] = 0x20
    cdb[6] = acc
    cdb[9] = opt
    return bytes(cdb)


def cdb_reg_rw(write: bool, addr: int, length: int, *, acc: int = 2, opt: int = 1) -> bytes:
    cdb = _gp_cdb()
    cdb[0] = 0xFD
    cdb[1] = 0x2A if write else 0x28
    cdb[4:6] = int(addr).to_bytes(2, "big")
    cdb[6] = acc
    cdb[7:9] = int(length).to_bytes(2, "big")
    cdb[9] = opt
    return bytes(cdb)


def cdb_inquiry(alloc_len: int = 44) -> bytes:
    return bytes([INQUIRY, 0, 0, 0, alloc_len & 0xFF, 0])


def discover_sg_devices(vid: int = USB_VID, pid: int = USB_PID) -> list[str]:
    root = Path("/sys/bus/usb/devices")
    found: list[str] = []
    if not root.is_dir():
        return found
    for dev in root.iterdir():
        try:
            v = int((dev / "idVendor").read_text().strip(), 16)
            p = int((dev / "idProduct").read_text().strip(), 16)
        except (FileNotFoundError, ValueError):
            continue
        if v != vid or p != pid:
            continue
        names: set[str] = set()
        for sg in dev.rglob("scsi_generic"):
            if not sg.is_dir():
                continue
            for child in sg.iterdir():
                if child.name.startswith("sg"):
                    names.add(child.name)
        for name in sorted(names):
            path = f"/dev/{name}"
            if path not in found:
                found.append(path)
    return found


class ScsiProbe:
    def __init__(self, fd: int, path: str):
        self.fd = fd
        self.path = path

    @classmethod
    def open(cls, path: str) -> ScsiProbe:
        fd = os.open(path, os.O_RDWR)
        return cls(fd, path)

    def close(self) -> None:
        if self.fd >= 0:
            os.close(self.fd)
            self.fd = -1

    def __enter__(self) -> ScsiProbe:
        return self

    def __exit__(self, *exc: object) -> None:
        self.close()

    def sg_io(
        self,
        cdb: bytes,
        *,
        data_in: int = 0,
        data_out: bytes = b"",
        timeout_ms: int = DEFAULT_TIMEOUT_MS,
    ) -> bytes:
        if data_in and data_out:
            raise ValueError("SG_IO: data_in and data_out are mutually exclusive")

        cmd = (ctypes.c_ubyte * len(cdb)).from_buffer_copy(cdb)
        sense = (ctypes.c_ubyte * SENSE_LEN)()
        hdr = SgIoHdr()
        hdr.interface_id = ord("S")
        hdr.cmd_len = len(cdb)
        hdr.mx_sb_len = SENSE_LEN
        hdr.cmdp = ctypes.addressof(cmd)
        hdr.sbp = ctypes.addressof(sense)
        hdr.timeout = timeout_ms

        dxfer: ctypes.Array[ctypes.c_ubyte] | None = None
        if data_out:
            hdr.dxfer_direction = SG_DXFER_TO_DEV
            hdr.dxfer_len = len(data_out)
            dxfer = (ctypes.c_ubyte * len(data_out)).from_buffer_copy(data_out)
            hdr.dxferp = ctypes.addressof(dxfer)
        elif data_in:
            hdr.dxfer_direction = SG_DXFER_FROM_DEV
            hdr.dxfer_len = data_in
            dxfer = (ctypes.c_ubyte * data_in)()
            hdr.dxferp = ctypes.addressof(dxfer)
        else:
            hdr.dxfer_direction = SG_DXFER_NONE
            hdr.dxfer_len = 0

        try:
            fcntl.ioctl(self.fd, SG_IO, hdr)
        except OSError as e:
            raise ScsiError(f"SG_IO ioctl failed on {self.path}: {e}") from e

        if hdr.status or hdr.host_status or hdr.driver_status:
            sb = bytes(sense[: hdr.sb_len_wr])
            raise ScsiError(
                f"SCSI status=0x{hdr.status:02x} host=0x{hdr.host_status:x} "
                f"driver=0x{hdr.driver_status:x} cdb={cdb.hex()}",
                status=hdr.status,
                sense=sb,
            )

        if data_in and dxfer is not None:
            n = data_in - max(hdr.resid, 0)
            return bytes(dxfer[: max(n, 0)])
        return b""

    def inquiry(self, alloc_len: int = 44) -> bytes:
        return self.sg_io(cdb_inquiry(alloc_len), data_in=alloc_len)

    def check_inquiry(self) -> bytes:
        data = self.inquiry()
        magic = data[36:44] if len(data) >= 44 else b""
        if magic != INQ_MAGIC:
            raise ScsiError(
                f"{self.path} INQUIRY magic {magic!r} != {INQ_MAGIC!r} (not usbdisk2?)"
            )
        return data

    def auth(self) -> bytes:
        cdb = cdb_auth()
        resp = self.sg_io(cdb, data_in=2)
        want = expected_auth(cdb)
        if resp != want:
            raise ScsiError(f"auth mismatch: got {resp.hex()} want {want.hex()}")
        return resp
