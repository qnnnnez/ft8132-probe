"""Find / scan / register R/W matching FT8132Debugger + usbdisk2."""

from __future__ import annotations

from .scsi import ScsiProbe, cdb_enter, cdb_exit, cdb_reg_rw, cdb_scan

CHIP_ID = 0x02
BFLASH_LEN = 128
STATUS_ADDR = 0x40
STATUS_LEN = 44
MAX_XFER = 512


class ProtocolError(RuntimeError):
    pass


class Protocol:
    def __init__(self, scsi: ScsiProbe):
        self.scsi = scsi

    def enter(self, *, opt: int = 49, acc: int = 2) -> None:
        self.scsi.sg_io(cdb_enter(opt=opt, acc=acc))

    def exit_i2c(self) -> None:
        self.scsi.sg_io(cdb_exit())

    def scan(self) -> tuple[int, int]:
        data = self.scsi.sg_io(cdb_scan(), data_in=2)
        if len(data) < 2:
            raise ProtocolError(f"short scan response: {data.hex()}")
        return data[0], data[1]

    def read_regs(self, addr: int, length: int, *, acc: int = 5, opt: int = 1) -> bytes:
        if length <= 0:
            raise ValueError("length must be > 0")
        out = bytearray()
        remaining = length
        cur = addr & 0xFF
        while remaining:
            n = min(remaining, MAX_XFER)
            # Probe treats addr=0 len=128 as bflash; split around that for generic dumps.
            if cur == 0 and n == BFLASH_LEN and length != BFLASH_LEN:
                n = BFLASH_LEN - 1
            use_acc = 5 if n > 1 else acc
            data = self.scsi.sg_io(
                cdb_reg_rw(False, cur, n, acc=use_acc, opt=opt),
                data_in=n,
            )
            if len(data) < n:
                raise ProtocolError(f"short read at 0x{cur:02x}: {len(data)}/{n}")
            out.extend(data)
            remaining -= n
            cur = (cur + n) & 0xFF
        return bytes(out)

    def write_regs(self, addr: int, data: bytes, *, acc: int = 2, opt: int = 1) -> None:
        """Write one byte per CDB so the probe can apply per-register handshake."""
        for i, byte in enumerate(data):
            self.scsi.sg_io(
                cdb_reg_rw(True, (addr + i) & 0xFF, 1, acc=acc, opt=opt),
                data_out=bytes([byte]),
            )

    def read_reg(self, addr: int) -> int:
        return self.read_regs(addr, 1, acc=2)[0]

    def write_reg(self, addr: int, value: int) -> None:
        self.write_regs(addr, bytes([value & 0xFF]), acc=2)

    def read_bflash(self) -> bytes:
        return self.read_regs(0, BFLASH_LEN, acc=5, opt=5)

    def read_status(self) -> bytes:
        return self.read_regs(STATUS_ADDR, STATUS_LEN, acc=5, opt=1)

    def find(self) -> dict:
        self.enter(opt=49, acc=2)
        slave, comm = self.scan()
        if comm != 0:
            raise ProtocolError(f"scan communication fail (slave=0x{slave:02x} status={comm})")
        ident = self.read_reg(0x00)
        chip_id = self.read_reg(0x01)
        slave_raw = self.read_reg(0x88)
        if chip_id != CHIP_ID:
            raise ProtocolError(f"chip id 0x{chip_id:02x} != 0x{CHIP_ID:02x}")
        bflash = self.read_bflash()
        if len(bflash) == BFLASH_LEN and bflash[111] != 0:
            raise ProtocolError(
                f"failed to enter I2C: bflash[111]=0x{bflash[111]:02x} (want 0)"
            )
        return {
            "ident": ident,
            "chip_id": chip_id,
            "slave_raw": slave_raw,
            "slave_id": (slave_raw >> 1) & 0x7F,
            "scan_slave": slave,
            "comm_status": comm,
            "bflash": bflash,
        }
