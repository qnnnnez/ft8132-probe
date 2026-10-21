"""Find / scan / register R/W matching FT8132Debugger + usbdisk2."""

from __future__ import annotations

from .scsi import (
    MTP_TIMEOUT_MS,
    ScsiProbe,
    cdb_enter,
    cdb_exit,
    cdb_lab_clamp,
    cdb_lab_load,
    cdb_lab_read,
    cdb_lab_run,
    cdb_mtp_erase,
    cdb_reg_rw,
    cdb_scan,
)

CHIP_ID = 0x02
BFLASH_LEN = 128
STATUS_ADDR = 0x40
STATUS_LEN = 44
MAX_XFER = 512
MTP_BITS = 1024
BMTP_BITS = 110
CMTP_BITS = 748
CMTP_OFF = 128
BKMTP_OFF = 896
STAT_REG = 0xF4
STAT_BUSY = 0x20
CRC_SEED = 0xFFFF
CRC_POLY = 0x1021


def crc16_gui(buf: bytes, start: int, length: int, seed: int = CRC_SEED) -> int:
    """MainWindow.cs `crc16` (~6090): CRC-16-CCITT over `length` stored bytes."""
    acc = seed
    idx = start
    cur = 0
    for i in range(length * 8):
        if i % 8 == 0:
            cur = buf[idx] << 8
            idx += 1
        mixed = acc ^ cur
        acc = (acc << 1) & 0xFFFF
        cur = (cur << 1) & 0xFFFF
        if mixed & 0x8000:
            acc ^= CRC_POLY
    return acc


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

    def read_mtp_bits(self, addr: int, nbits: int, *, opt: int) -> bytes:
        if nbits <= 0 or nbits > MAX_XFER:
            raise ValueError("nbits out of range")
        data = self.scsi.sg_io(
            cdb_reg_rw(False, addr, nbits, acc=5, opt=opt),
            data_in=nbits,
            timeout_ms=MTP_TIMEOUT_MS,
        )
        if len(data) < nbits:
            raise ProtocolError(f"short MTP read at {addr}: {len(data)}/{nbits}")
        return data

    def prime_mtp(self) -> None:
        """Clock out the empty first F6 window after enter."""
        self.read_mtp_bits(0, BMTP_BITS, opt=5)

    def write_mtp_bits(self, addr: int, bits: bytes, *, opt: int = 133) -> None:
        if not bits or len(bits) > MAX_XFER:
            raise ValueError("MTP write length out of range")
        # Do not emit extra MTP FD 28 reads between c1 and c2. The dongle
        # re-selects on each program CDB; a host verify-read in between is
        # not in write.sr.
        self.scsi.sg_io(
            cdb_reg_rw(True, addr, len(bits), acc=5, opt=opt),
            data_out=bytes(bits),
            timeout_ms=MTP_TIMEOUT_MS,
        )

    def erase_mtp(self) -> None:
        self.scsi.sg_io(cdb_mtp_erase(), timeout_ms=MTP_TIMEOUT_MS)

    def _read_mtp_image(self) -> bytes:
        self.prime_mtp()
        img = bytearray(MTP_BITS)
        img[0:BMTP_BITS] = self.read_mtp_bits(0, BMTP_BITS, opt=5)
        half = CMTP_BITS // 2
        img[CMTP_OFF : CMTP_OFF + half] = self.read_mtp_bits(CMTP_OFF, half, opt=6)
        img[CMTP_OFF + half : CMTP_OFF + CMTP_BITS] = self.read_mtp_bits(
            CMTP_OFF + half, half, opt=6
        )
        img[BKMTP_OFF : BKMTP_OFF + BMTP_BITS] = self.read_mtp_bits(
            BKMTP_OFF, BMTP_BITS, opt=5
        )
        return bytes(img)

    def dump_mtp(self) -> bytes:
        """1024-byte 0/1 image (GUI `m_byAdminREfuseBitArr`)."""
        a = self._read_mtp_image()
        b = self._read_mtp_image()
        if sum(x & 1 for x in b) > sum(x & 1 for x in a):
            return b
        return a

    def _clear_mtp_busy(self) -> None:
        st = self.read_reg(STAT_REG)
        if st & STAT_BUSY:
            self.write_reg(STAT_REG, st & ~STAT_BUSY)

    def write_mtp(self, img: bytes) -> None:
        if len(img) != MTP_BITS:
            raise ValueError(f"MTP image must be {MTP_BITS} bytes, got {len(img)}")
        self._clear_mtp_busy()
        self.prime_mtp()
        self.erase_mtp()
        # GUI polls 0xF4 after ChipErase1 before the first program CDB.
        self._clear_mtp_busy()
        self.write_mtp_bits(BKMTP_OFF, img[BKMTP_OFF : BKMTP_OFF + BMTP_BITS], opt=133)
        self._clear_mtp_busy()
        half = CMTP_BITS // 2
        self.write_mtp_bits(CMTP_OFF, img[CMTP_OFF : CMTP_OFF + half], opt=133)
        self.write_mtp_bits(
            CMTP_OFF + half, img[CMTP_OFF + half : CMTP_OFF + CMTP_BITS], opt=133
        )

    def lab_clamp(self, low: bool) -> None:
        """Drive SCL and SDA low, or release them. No I2C transaction."""
        self.scsi.sg_io(cdb_lab_clamp(low), timeout_ms=MTP_TIMEOUT_MS)

    def lab_load(self, img: bytes) -> None:
        """Copy a 1024-byte image into Blue Pill SRAM. Does not touch I2C."""
        if len(img) != MTP_BITS:
            raise ValueError(f"lab image must be {MTP_BITS} bytes, got {len(img)}")
        for off in range(0, MTP_BITS, 512):
            chunk = img[off : off + 512]
            self.scsi.sg_io(
                cdb_lab_load(off, len(chunk)),
                data_out=chunk,
                timeout_ms=MTP_TIMEOUT_MS,
            )

    def lab_read(self) -> bytes:
        """Read the SRAM image back."""
        out = bytearray()
        for off in range(0, MTP_BITS, 512):
            n = min(512, MTP_BITS - off)
            out += self.scsi.sg_io(
                cdb_lab_read(off, n),
                data_in=n,
                timeout_ms=MTP_TIMEOUT_MS,
            )
        return bytes(out)

    def lab_run(self, op: int, gap_us: int = 1800) -> None:
        """Program the SRAM image with dialect `op`. No USB inside the dialect."""
        self.scsi.sg_io(
            cdb_lab_run(op, gap_us),
            timeout_ms=MTP_TIMEOUT_MS,
        )

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
