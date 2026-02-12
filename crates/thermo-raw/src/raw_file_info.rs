//! RawFileInfoPreamble parsing.
//!
//! Contains acquisition date and pointers to RunHeaders.
//!
//! From decompiled RawFileInfo.Load version dispatch:
//! - v65+: RawFileInfoStruct  (current, with VirtualControllerInfoStruct[64] + BlobOffset + BlobSize)
//! - v64:  RawFileInfoStruct4 (with VirtualControllerInfoStruct[64], no blob)
//! - v25-63: RawFileInfoStruct3 (OldVirtualControllerInfo[64] only, no 64-bit fields)
//! - v7-24: RawFileInfoStruct2
//! - v<7:  RawFileInfoStruct1
//!
//! Common preamble fields (all versions):
//!   IsExpMethodPresent (bool/u32, 4 bytes), SystemTimeStruct (16 bytes),
//!   IsInAcquisition (bool/u32, 4 bytes), VirtualDataOffset32 (u32, 4 bytes),
//!   NumberOfVirtualControllers (i32, 4 bytes), NextAvailableControllerIndex (i32, 4 bytes)
//!
//! VirtualControllerInfo arrays:
//!   - OldVirtualControllerInfo (12 bytes each): VirtualDeviceType(i32) + VirtualDeviceIndex(i32) + Offset(i32)
//!   - VirtualControllerInfoStruct (16 bytes each): VirtualDeviceType(i32) + VirtualDeviceIndex(i32) + Offset(i64)
//!
//! After the struct: 5 user label strings (PascalStringWin32), then ComputerName (v7+).

use crate::io_utils::BinaryReader;
use crate::RawError;

/// Virtual controller info entry from the VCI array.
///
/// Each entry describes a data controller (MS, UV, Analog, etc.)
/// and its RunHeader location in the file.
///
/// Device types (from Thermo .NET Device enum, confirmed via pythonnet):
///   -1 = None, 0 = MS, 1 = MSAnalog, 2 = Analog, 3 = UV, 4 = Pda, 5 = Other
#[derive(Debug, Clone, Copy, Default)]
pub struct VirtualControllerInfo {
    pub device_type: i32,
    pub device_index: i32,
    pub offset: i64,
}

/// Parsed RawFileInfo with addresses to key data structures.
#[derive(Debug, Clone)]
pub struct RawFileInfo {
    pub year: u16,
    pub month: u16,
    pub day: u16,
    pub hour: u16,
    pub minute: u16,
    pub second: u16,
    pub millisecond: u16,
    /// All virtual controller info entries (v64+ uses 64-bit offsets).
    pub controllers: Vec<VirtualControllerInfo>,
    /// Number of data controllers (from preamble field).
    pub n_controllers: u32,
    /// Heading strings (user labels + computer name).
    pub headings: Vec<String>,
    /// Blob offset (v65+ only, -1 if no blob).
    pub blob_offset: i64,
    /// Blob size in bytes (v65+ only, 0 if no blob).
    pub blob_size: u32,
    /// Byte offset after parsing (where the next structure begins).
    pub end_offset: u64,
}

impl RawFileInfo {
    /// Parse RawFileInfo starting at the given offset in the data stream.
    ///
    /// Handles all supported versions (v57-v66) following the decompiled
    /// RawFileInfo.Load version dispatch logic.
    /// Parse RawFileInfo starting at the given offset in the data stream.
    ///
    /// Handles all supported versions (v57-v66) following the decompiled
    /// RawFileInfo.Load version dispatch logic.
    pub fn parse(data: &[u8], offset: u64, version: u32) -> Result<Self, RawError> {
        let mut reader = BinaryReader::at_offset(data, offset);

        // IsExpMethodPresent (bool marshalled as i32 = 4 bytes)
        let _method_file_present = reader.read_u32()?;

        // SystemTimeStruct (8 x u16 = 16 bytes)
        let year = reader.read_u16()?;
        let month = reader.read_u16()?;
        let _day_of_week = reader.read_u16()?;
        let day = reader.read_u16()?;
        let hour = reader.read_u16()?;
        let minute = reader.read_u16()?;
        let second = reader.read_u16()?;
        let millisecond = reader.read_u16()?;

        // IsInAcquisition (bool as i32 = 4 bytes)
        let _is_in_acquisition = reader.read_u32()?;

        // VirtualDataOffset32 (u32)
        let _data_addr_32 = reader.read_u32()?;

        // NumberOfVirtualControllers (i32)
        let n_controllers = reader.read_u32()?;

        // NextAvailableControllerIndex (i32)
        let _n_controllers_2 = reader.read_u32()?;

        // OldVirtualControllerInfo[64] (12 bytes each = 768 bytes)
        // Each: VirtualDeviceType(i32) + VirtualDeviceIndex(i32) + Offset(i32)
        let mut controllers = Vec::new();
        for _ in 0..64 {
            let device_type = reader.read_i32()?;
            let device_index = reader.read_i32()?;
            let offset32 = reader.read_i32()?;
            controllers.push(VirtualControllerInfo {
                device_type,
                device_index,
                offset: offset32 as i64,
            });
        }

        // Version-dependent extended fields
        let mut blob_offset = -1i64;
        let mut blob_size = 0u32;

        if version >= 64 {
            // VirtualDataOffset (i64) - 64-bit version of VirtualDataOffset32
            let _data_addr_64 = reader.read_u64()?;

            // VirtualControllerInfoStruct[64] (16 bytes each = 1024 bytes)
            // Each: VirtualDeviceType(i32) + VirtualDeviceIndex(i32) + Offset(i64)
            // These replace the OldVCI entries with 64-bit offsets
            controllers.clear();
            for _ in 0..64 {
                let device_type = reader.read_i32()?;
                let device_index = reader.read_i32()?;
                let offset64 = reader.read_u64()? as i64;
                controllers.push(VirtualControllerInfo {
                    device_type,
                    device_index,
                    offset: offset64,
                });
            }

            if version >= 65 {
                // BlobOffset (i64) + BlobSize (u32)
                blob_offset = reader.read_u64()? as i64;
                blob_size = reader.read_u32()?;
            }
        }

        // Read heading strings: 5 user labels (PascalStringWin32)
        let mut headings = Vec::new();
        for _ in 0..5 {
            match reader.read_pascal_string() {
                Ok(s) => headings.push(s),
                Err(_) => break,
            }
        }

        // Computer name (v7+)
        if version >= 7 {
            if let Ok(s) = reader.read_pascal_string() {
                headings.push(s);
            }
        }

        Ok(Self {
            year,
            month,
            day,
            hour,
            minute,
            second,
            millisecond,
            controllers,
            n_controllers,
            headings,
            blob_offset,
            blob_size,
            end_offset: reader.position(),
        })
    }

    /// Find the RunHeader address for the first MS controller (device_type=0).
    ///
    /// Falls back to the first controller with a non-zero offset if no
    /// MS-specific controller is found.
    pub fn run_header_addr(&self) -> u64 {
        // Prefer device_type=0 (MS controller per Thermo Device enum)
        if let Some(ms) = self.controllers.iter().find(|c| c.device_type == 0 && c.offset > 0) {
            return ms.offset as u64;
        }
        // Fallback: first controller with non-zero offset
        self.controllers
            .iter()
            .find(|c| c.offset > 0)
            .map(|c| c.offset as u64)
            .unwrap_or(0)
    }

    /// Get controller info for a specific device type and index.
    pub fn controller(&self, device_type: i32, device_index: i32) -> Option<&VirtualControllerInfo> {
        self.controllers
            .iter()
            .find(|c| c.device_type == device_type && c.device_index == device_index)
    }

    /// Check if this parsed RawFileInfo appears to contain valid VCI data.
    ///
    /// Used during the search phase to validate candidate offsets.
    /// Only examines the first `n_controllers` entries and verifies entries
    /// beyond that are zero (catching misaligned parses that produce spurious matches).
    ///
    /// Accepts `n_controllers == 0` (blank/empty acquisitions) as long as the
    /// controller array beyond that is all zeros (proving correct alignment).
    pub fn has_valid_controllers(&self, file_size: u64) -> bool {
        if self.n_controllers > 16 {
            return false;
        }
        let n = self.n_controllers as usize;
        if n > self.controllers.len() {
            return false;
        }

        if n == 0 {
            // Zero controllers: accept if the first few VCI slots are all zero
            // (proves we're aligned correctly, not reading random data)
            let all_zero = self.controllers.iter().take(4).all(|c| {
                c.device_type == 0 && c.device_index == 0 && c.offset == 0
            });
            return all_zero;
        }

        // All declared controllers must have recognized types and plausible offsets.
        let valid_count = self.controllers[..n]
            .iter()
            .filter(|c| {
                (0..=5).contains(&c.device_type)
                    && c.offset > 4096
                    && (c.offset as u64) < file_size
            })
            .count();
        if valid_count < n {
            return false;
        }
        // Verify entries beyond n_controllers are empty (catches misaligned parses).
        let extra_populated = self.controllers[n..]
            .iter()
            .take(4) // Only check next few entries for efficiency
            .filter(|c| c.device_type != 0 || c.offset != 0)
            .count();
        extra_populated == 0
    }

    /// Format the acquisition date as ISO 8601.
    pub fn acquisition_date(&self) -> String {
        format!(
            "{:04}-{:02}-{:02}T{:02}:{:02}:{:02}",
            self.year, self.month, self.day, self.hour, self.minute, self.second
        )
    }
}
