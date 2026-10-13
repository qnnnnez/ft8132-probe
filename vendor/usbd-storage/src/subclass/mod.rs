//! USB Mass Storage subclasses
#[cfg(all(feature = "bbb", feature = "scsi"))]
use crate::subclass::scsi::{Scsi, ScsiCommand};
#[cfg(all(feature = "bbb", feature = "ufi"))]
use crate::subclass::ufi::{Ufi, UfiCommand};
#[cfg(all(any(feature = "scsi", feature = "ufi"), feature = "bbb"))]
use {
    crate::transport::bbb::{BulkOnly, BulkOnlyError},
    crate::transport::{CommandStatus, TransportError},
    core::borrow::BorrowMut,
    usb_device::bus::UsbBus,
};

#[cfg(feature = "scsi")]
pub mod scsi;
#[cfg(feature = "ufi")]
pub mod ufi;

/// The subclass' command and a LUN it is addressed to
pub struct Command<'a, Kind, Class> {
    #[allow(dead_code)]
    pub class: &'a mut Class,
    pub kind: Kind,
    pub lun: u8,
    /// Raw CDB bytes (up to 16 bytes for BOT CBW), for debugging/inspection.
    pub cdb: [u8; 16],
    /// Number of valid bytes in `cdb`.
    pub cdb_len: u8,
}

impl<Kind, Class> Command<'_, Kind, Class> {
    /// Returns the raw CDB bytes for this command.
    pub fn cdb_bytes(&self) -> &[u8] {
        &self.cdb[..self.cdb_len as usize]
    }
}

/// [UFI] over [Bulk Only Transport] command
///
/// [UFI]: crate::subclass::ufi::Ufi
/// [Bulk Only Transport]: crate::transport::bbb::BulkOnly
#[cfg(all(feature = "bbb", feature = "ufi"))]
impl<'a, 'alloc, Bus: UsbBus + 'alloc, Buf: BorrowMut<[u8]>>
    Command<'a, UfiCommand, Ufi<BulkOnly<'alloc, Bus, Buf>>>
{
    /// [crate::transport::bbb::BulkOnly::read_data]
    pub fn read_data(&mut self, dst: &mut [u8]) -> Result<usize, TransportError<BulkOnlyError>> {
        self.class.transport.read_data(dst)
    }

    /// [crate::transport::bbb::BulkOnly::write_data]
    pub fn write_data(&mut self, src: &[u8]) -> Result<usize, TransportError<BulkOnlyError>> {
        self.class.transport.write_data(src)
    }

    /// [crate::transport::bbb::BulkOnly::try_write_data_all]
    pub fn try_write_data_all(&mut self, src: &[u8]) -> Result<(), TransportError<BulkOnlyError>> {
        self.class.transport.try_write_data_all(src)
    }

    pub fn pass(self) {
        self.class.transport.set_status(CommandStatus::Passed);
    }

    pub fn fail(self) {
        self.class.transport.set_status(CommandStatus::Failed);
    }

    pub fn fail_phase(self) {
        self.class.transport.set_status(CommandStatus::PhaseError);
    }
}

/// [SCSI] over [Bulk Only Transport] command
///
/// [SCSI]: crate::subclass::scsi::Scsi
/// [Bulk Only Transport]: crate::transport::bbb::BulkOnly
#[cfg(all(feature = "bbb", feature = "scsi"))]
impl<'a, 'alloc, Bus: UsbBus + 'alloc, Buf: BorrowMut<[u8]>>
    Command<'a, ScsiCommand, Scsi<BulkOnly<'alloc, Bus, Buf>>>
{
    /// [crate::transport::bbb::BulkOnly::read_data]
    pub fn read_data(&mut self, dst: &mut [u8]) -> Result<usize, TransportError<BulkOnlyError>> {
        self.class.transport.read_data(dst)
    }

    /// [crate::transport::bbb::BulkOnly::write_data]
    pub fn write_data(&mut self, src: &[u8]) -> Result<usize, TransportError<BulkOnlyError>> {
        self.class.transport.write_data(src)
    }

    /// [crate::transport::bbb::BulkOnly::try_write_data_all]
    pub fn try_write_data_all(&mut self, src: &[u8]) -> Result<(), TransportError<BulkOnlyError>> {
        self.class.transport.try_write_data_all(src)
    }

    /// Host-to-device BOT data phase.
    pub fn is_data_out(&self) -> bool {
        self.class.transport.is_data_out()
    }

    /// Device-to-host BOT data phase.
    pub fn is_data_in(&self) -> bool {
        self.class.transport.is_data_in()
    }

    /// CBW `dCBWDataTransferLength`.
    pub fn data_transfer_len(&self) -> u32 {
        self.class.transport.data_transfer_len()
    }

    pub fn pass(self) {
        self.class.transport.set_status(CommandStatus::Passed);
    }

    pub fn fail(self) {
        self.class.transport.set_status(CommandStatus::Failed);
    }

    pub fn fail_phase(self) {
        self.class.transport.set_status(CommandStatus::PhaseError);
    }
}
