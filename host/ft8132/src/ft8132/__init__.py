"""Host CLI for the FT8132 USB SCSI probe.

Talks to ``usbdisk2`` with the same vendor CDBs as FT8132Debugger.
Linux only (SG_IO on /dev/sg*). The sg node needs read/write access;
a typical udev rule is:

    SUBSYSTEM=="scsi_generic", ATTRS{idVendor}=="1b3f", ATTRS{idProduct}=="8000", MODE="0660", GROUP="plugdev"
"""

__version__ = "0.1.0"

USB_VID = 0x1B3F
USB_PID = 0x8000
INQ_MAGIC = b"GP-PROD."
