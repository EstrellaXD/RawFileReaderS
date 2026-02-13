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
            // .NET RawFileInfoStruct uses LayoutKind.Sequential with natural alignment.
            // After OldVCI[64] (768 bytes starting at struct+36), the reader is at struct+804.
            // VirtualDataOffset (long, 8-byte alignment) needs 4 bytes of padding to reach
            // struct+808. NewVCI then starts at struct+816.
            reader.skip(4)?; // alignment padding for i64

            // VirtualDataOffset (i64) - 64-bit version of VirtualDataOffset32
            let _data_addr_64 = reader.read_u64()?;

            // VirtualControllerInfoStruct[64] (16 bytes each = 1024 bytes)
            // Each: VirtualDeviceType(i32) + VirtualDeviceIndex(i32) + Offset(i64)
            // Normally these replace OldVCI with 64-bit offsets, but some files
            // (e.g., 2018-era Exactive v66) have valid OldVCI with zeroed NewVCI.
            let old_controllers = controllers.clone();
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

            // If NewVCI has no valid entries but OldVCI does, fall back to OldVCI.
            // Some files (e.g., 2018-era Exactive v66) have valid 32-bit offsets
            // but zeroed/garbage 64-bit VCI area.
            let file_size = data.len() as u64;
            if !controllers
                .iter()
                .any(|c| Self::is_valid_controller(c, file_size))
                && old_controllers
                    .iter()
                    .any(|c| Self::is_valid_controller(c, file_size))
            {
                controllers = old_controllers;
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
        if let Some(ms) = self
            .controllers
            .iter()
            .find(|c| c.device_type == 0 && c.offset > 0)
        {
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
    pub fn controller(
        &self,
        device_type: i32,
        device_index: i32,
    ) -> Option<&VirtualControllerInfo> {
        self.controllers
            .iter()
            .find(|c| c.device_type == device_type && c.device_index == device_index)
    }

    /// Check if this parsed RawFileInfo appears to contain valid VCI data.
    ///
    /// Uses a two-pass approach:
    /// 1. **Strict**: Valid date + valid n_controllers + matching VCI entries
    /// 2. **VCI-only fallback**: Ignores date/n_controllers, validates VCI structure directly.
    ///    Used for files with zeroed-out preamble fields but intact VCI arrays.
    ///
    /// The VCI-only fallback counts valid entries (device_type 0-5, offset within file)
    /// and zero entries. If all 64 entries are either valid or empty (no garbage),
    /// and at least one is valid, the alignment is correct.
    pub fn has_valid_controllers(&self, file_size: u64) -> bool {
        // Quick reject: n_controllers must be in a plausible range (0-16).
        // This prevents false positives when scanning through garbage data.
        // n_controllers=0 is allowed (zeroed preamble files with intact VCI arrays).
        if self.n_controllers > 16 {
            return false;
        }

        // Pass 1: Strict validation (date + n_controllers + VCI)
        if self.has_valid_controllers_strict(file_size) {
            return true;
        }

        // Pass 2: VCI-only fallback for files with zeroed preamble fields
        self.has_valid_controllers_vci_only(file_size)
    }

    fn has_valid_controllers_strict(&self, file_size: u64) -> bool {
        if self.n_controllers > 16 || !self.has_valid_date() {
            return false;
        }

        let n = self.n_controllers as usize;
        if n > self.controllers.len() {
            return false;
        }

        if n == 0 {
            return self
                .controllers
                .iter()
                .take(16)
                .all(Self::is_zero_controller);
        }

        let valid_count = self.controllers[..n]
            .iter()
            .filter(|c| Self::is_valid_controller(c, file_size))
            .count();

        valid_count == n
            && self.controllers[n..]
                .iter()
                .take(4)
                .all(Self::is_zero_controller)
    }

    /// VCI-only validation: ignores date and n_controllers fields.
    ///
    /// Validates alignment by checking that ALL 64 VCI entries are either:
    /// - Valid: device_type 0-5, device_index 0-7, offset within file bounds
    /// - Empty: all three fields are zero
    ///
    /// Requires at least 1 valid entry and no garbage entries.
    fn has_valid_controllers_vci_only(&self, file_size: u64) -> bool {
        let mut valid_count = 0usize;
        let mut zero_count = 0usize;

        for c in &self.controllers {
            if Self::is_zero_controller(c) {
                zero_count += 1;
            } else if Self::is_valid_controller_strict(c, file_size) {
                valid_count += 1;
            } else {
                return false;
            }
        }

        valid_count >= 1 && valid_count + zero_count == self.controllers.len()
    }

    fn has_valid_date(&self) -> bool {
        (2000..=2100).contains(&self.year)
            && (1..=12).contains(&self.month)
            && (1..=31).contains(&self.day)
            && self.hour <= 23
            && self.minute <= 59
            && self.second <= 59
    }

    fn is_zero_controller(c: &VirtualControllerInfo) -> bool {
        c.device_type == 0 && c.device_index == 0 && c.offset == 0
    }

    fn is_valid_controller(c: &VirtualControllerInfo, file_size: u64) -> bool {
        (0..=5).contains(&c.device_type) && c.offset > 4096 && (c.offset as u64) < file_size
    }

    fn is_valid_controller_strict(c: &VirtualControllerInfo, file_size: u64) -> bool {
        (0..=5).contains(&c.device_type)
            && (0..=7).contains(&c.device_index)
            && c.offset > 4096
            && (c.offset as u64) < file_size
    }

    /// Format the acquisition date as ISO 8601.
    pub fn acquisition_date(&self) -> String {
        format!(
            "{:04}-{:02}-{:02}T{:02}:{:02}:{:02}",
            self.year, self.month, self.day, self.hour, self.minute, self.second
        )
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// Create a test RawFileInfo with specified parameters for validation testing.
    fn make_test_info(
        year: u16,
        month: u16,
        day: u16,
        n_controllers: u32,
        controllers: Vec<VirtualControllerInfo>,
    ) -> RawFileInfo {
        RawFileInfo {
            year,
            month,
            day,
            hour: 12,
            minute: 30,
            second: 45,
            millisecond: 123,
            controllers,
            n_controllers,
            headings: vec![],
            blob_offset: -1,
            blob_size: 0,
            end_offset: 0,
        }
    }

    // Tests for has_valid_controllers_strict()

    #[test]
    fn test_strict_validation_valid_date_and_controllers() {
        // Valid date (2020-05-15) + 2 valid controllers
        let controllers = vec![
            VirtualControllerInfo {
                device_type: 0, // MS
                device_index: 0,
                offset: 10000,
            },
            VirtualControllerInfo {
                device_type: 1, // MSAnalog
                device_index: 0,
                offset: 20000,
            },
        ];
        // Pad to 64 entries with zeros
        let mut full_controllers = controllers.clone();
        full_controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 5, 15, 2, full_controllers);
        assert!(info.has_valid_controllers_strict(100000));
    }

    #[test]
    fn test_strict_validation_rejects_invalid_year() {
        // Year 1999 is invalid (< 2000)
        let mut controllers = vec![VirtualControllerInfo {
            device_type: 0,
            device_index: 0,
            offset: 10000,
        }];
        controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(1999, 5, 15, 1, controllers);
        assert!(!info.has_valid_controllers_strict(100000));
    }

    #[test]
    fn test_strict_validation_rejects_future_year() {
        // Year 2101 is invalid (> 2100)
        let mut controllers = vec![VirtualControllerInfo {
            device_type: 0,
            device_index: 0,
            offset: 10000,
        }];
        controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2101, 5, 15, 1, controllers);
        assert!(!info.has_valid_controllers_strict(100000));
    }

    #[test]
    fn test_strict_validation_rejects_invalid_month() {
        // Month 0 is invalid
        let mut controllers = vec![VirtualControllerInfo {
            device_type: 0,
            device_index: 0,
            offset: 10000,
        }];
        controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 0, 15, 1, controllers.clone());
        assert!(!info.has_valid_controllers_strict(100000));

        // Month 13 is invalid
        let info2 = make_test_info(2020, 13, 15, 1, controllers);
        assert!(!info2.has_valid_controllers_strict(100000));
    }

    #[test]
    fn test_strict_validation_rejects_invalid_day() {
        // Day 0 is invalid
        let mut controllers = vec![VirtualControllerInfo {
            device_type: 0,
            device_index: 0,
            offset: 10000,
        }];
        controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 5, 0, 1, controllers.clone());
        assert!(!info.has_valid_controllers_strict(100000));

        // Day 32 is invalid
        let info2 = make_test_info(2020, 5, 32, 1, controllers);
        assert!(!info2.has_valid_controllers_strict(100000));
    }

    #[test]
    fn test_strict_validation_rejects_too_many_controllers() {
        // n_controllers > 16 is rejected
        let controllers = vec![VirtualControllerInfo::default(); 64];
        let info = make_test_info(2020, 5, 15, 17, controllers);
        assert!(!info.has_valid_controllers_strict(100000));
    }

    // Tests for has_valid_controllers_vci_only()

    #[test]
    fn test_vci_only_rejects_all_zero() {
        // All 64 entries are zero (no valid controllers)
        let controllers = vec![VirtualControllerInfo::default(); 64];
        let info = make_test_info(2020, 5, 15, 0, controllers);
        assert!(!info.has_valid_controllers_vci_only(100000));
    }

    #[test]
    fn test_vci_only_accepts_one_valid_rest_zero() {
        // One valid entry + 63 zero entries
        let mut controllers = vec![VirtualControllerInfo {
            device_type: 0,
            device_index: 0,
            offset: 10000,
        }];
        controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 5, 15, 1, controllers);
        assert!(info.has_valid_controllers_vci_only(100000));
    }

    #[test]
    fn test_vci_only_rejects_garbage_entry() {
        // First entry is valid, second is garbage (invalid device_type)
        let controllers = vec![
            VirtualControllerInfo {
                device_type: 0,
                device_index: 0,
                offset: 10000,
            },
            VirtualControllerInfo {
                device_type: 99, // Invalid device_type (must be 0-5)
                device_index: 0,
                offset: 10000,
            },
        ];
        let mut full_controllers = controllers;
        full_controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 5, 15, 2, full_controllers);
        assert!(!info.has_valid_controllers_vci_only(100000));
    }

    #[test]
    fn test_vci_only_rejects_invalid_device_index() {
        // device_index must be 0-7
        let mut controllers = vec![VirtualControllerInfo {
            device_type: 0,
            device_index: 10, // Invalid (> 7)
            offset: 10000,
        }];
        controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 5, 15, 1, controllers);
        assert!(!info.has_valid_controllers_vci_only(100000));
    }

    #[test]
    fn test_vci_only_rejects_offset_too_small() {
        // offset must be > 4096
        let mut controllers = vec![VirtualControllerInfo {
            device_type: 0,
            device_index: 0,
            offset: 4000, // Too small
        }];
        controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 5, 15, 1, controllers);
        assert!(!info.has_valid_controllers_vci_only(100000));
    }

    #[test]
    fn test_vci_only_rejects_offset_beyond_file() {
        // offset must be < file_size
        let mut controllers = vec![VirtualControllerInfo {
            device_type: 0,
            device_index: 0,
            offset: 200000, // Beyond file size
        }];
        controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 5, 15, 1, controllers);
        assert!(!info.has_valid_controllers_vci_only(100000)); // file_size = 100000
    }

    #[test]
    fn test_vci_only_accepts_all_device_types() {
        // Test all valid device types: 0=MS, 1=MSAnalog, 2=Analog, 3=UV, 4=Pda, 5=Other
        for device_type in 0..=5 {
            let mut controllers = vec![VirtualControllerInfo {
                device_type,
                device_index: 0,
                offset: 10000,
            }];
            controllers.resize(64, VirtualControllerInfo::default());

            let info = make_test_info(2020, 5, 15, 1, controllers);
            assert!(
                info.has_valid_controllers_vci_only(100000),
                "device_type {} should be valid",
                device_type
            );
        }
    }

    #[test]
    fn test_vci_only_accepts_multiple_valid_entries() {
        // Multiple valid entries + zeros
        let controllers = vec![
            VirtualControllerInfo {
                device_type: 0,
                device_index: 0,
                offset: 10000,
            },
            VirtualControllerInfo {
                device_type: 1,
                device_index: 0,
                offset: 20000,
            },
            VirtualControllerInfo {
                device_type: 3,
                device_index: 1,
                offset: 30000,
            },
        ];
        let mut full_controllers = controllers;
        full_controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 5, 15, 3, full_controllers);
        assert!(info.has_valid_controllers_vci_only(100000));
    }

    // Tests for has_valid_controllers() (two-pass validation)

    #[test]
    fn test_two_pass_strict_succeeds() {
        // Strict validation passes
        let mut controllers = vec![VirtualControllerInfo {
            device_type: 0,
            device_index: 0,
            offset: 10000,
        }];
        controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 5, 15, 1, controllers);
        assert!(info.has_valid_controllers(100000));
    }

    #[test]
    fn test_two_pass_falls_back_to_vci_only() {
        // Strict fails (invalid year), but VCI-only passes
        let mut controllers = vec![VirtualControllerInfo {
            device_type: 0,
            device_index: 0,
            offset: 10000,
        }];
        controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(1999, 5, 15, 1, controllers);
        assert!(!info.has_valid_controllers_strict(100000));
        assert!(info.has_valid_controllers_vci_only(100000));
        assert!(info.has_valid_controllers(100000)); // Falls back to VCI-only
    }

    #[test]
    fn test_two_pass_both_fail() {
        // Both strict and VCI-only fail (all zeros)
        let controllers = vec![VirtualControllerInfo::default(); 64];
        let info = make_test_info(1999, 5, 15, 0, controllers);
        assert!(!info.has_valid_controllers_strict(100000));
        assert!(!info.has_valid_controllers_vci_only(100000));
        assert!(!info.has_valid_controllers(100000));
    }

    // Tests for OldVCI → NewVCI fallback logic

    #[test]
    fn test_parse_fallback_scenario_simulation() {
        // Simulate the scenario where NewVCI is all zeros but OldVCI has valid data.
        // We can't easily test the full parse() method without constructing binary data,
        // but we can test the validation logic that would apply.

        // Case 1: NewVCI has valid entries (normal case)
        let new_controllers = vec![VirtualControllerInfo {
            device_type: 0,
            device_index: 0,
            offset: 10000,
        }];
        let mut full_new = new_controllers;
        full_new.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 5, 15, 1, full_new);
        assert!(info.has_valid_controllers(100000));
    }

    #[test]
    fn test_fallback_old_vci_valid_new_vci_invalid() {
        // Simulate OldVCI having valid data when NewVCI doesn't.
        // This tests the logic that would be used in the fallback.
        let old_controllers = vec![VirtualControllerInfo {
            device_type: 0,
            device_index: 0,
            offset: 10000,
        }];
        let mut full_old = old_controllers;
        full_old.resize(64, VirtualControllerInfo::default());

        // OldVCI controllers pass validation
        let info_old = make_test_info(2020, 5, 15, 1, full_old.clone());
        assert!(info_old.has_valid_controllers(100000));

        // NewVCI all zeros would fail VCI-only validation
        let new_controllers_zero = vec![VirtualControllerInfo::default(); 64];
        let info_new = make_test_info(2020, 5, 15, 0, new_controllers_zero);
        assert!(!info_new.has_valid_controllers_vci_only(100000));

        // The actual parse() logic checks `controllers.iter().any(is_valid_entry)`
        // and falls back to old_controllers if new has no valid entries.
        // Here we verify the validation would work on the old controllers.
        let file_size = 100000i64;
        let is_valid_entry = |c: &VirtualControllerInfo| {
            (0..=5).contains(&c.device_type) && c.offset > 4096 && c.offset < file_size
        };

        assert!(full_old.iter().any(is_valid_entry));
    }

    // Additional utility method tests

    #[test]
    fn test_run_header_addr_finds_ms_controller() {
        let controllers = vec![
            VirtualControllerInfo {
                device_type: 2, // Analog (not MS)
                device_index: 0,
                offset: 5000,
            },
            VirtualControllerInfo {
                device_type: 0, // MS
                device_index: 0,
                offset: 10000,
            },
            VirtualControllerInfo {
                device_type: 1, // MSAnalog
                device_index: 0,
                offset: 15000,
            },
        ];
        let mut full_controllers = controllers;
        full_controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 5, 15, 3, full_controllers);
        assert_eq!(info.run_header_addr(), 10000); // Should find MS controller
    }

    #[test]
    fn test_run_header_addr_fallback_to_first_nonzero() {
        // No MS controller, should fall back to first non-zero offset
        let controllers = vec![
            VirtualControllerInfo {
                device_type: 0,
                device_index: 0,
                offset: 0, // Zero offset
            },
            VirtualControllerInfo {
                device_type: 2, // Analog
                device_index: 0,
                offset: 8000,
            },
            VirtualControllerInfo {
                device_type: 3, // UV
                device_index: 0,
                offset: 12000,
            },
        ];
        let mut full_controllers = controllers;
        full_controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 5, 15, 3, full_controllers);
        assert_eq!(info.run_header_addr(), 8000); // First non-zero
    }

    #[test]
    fn test_run_header_addr_returns_zero_when_all_zero() {
        let controllers = vec![VirtualControllerInfo::default(); 64];
        let info = make_test_info(2020, 5, 15, 0, controllers);
        assert_eq!(info.run_header_addr(), 0);
    }

    #[test]
    fn test_controller_method_finds_by_type_and_index() {
        let controllers = vec![
            VirtualControllerInfo {
                device_type: 0,
                device_index: 0,
                offset: 10000,
            },
            VirtualControllerInfo {
                device_type: 0,
                device_index: 1,
                offset: 15000,
            },
            VirtualControllerInfo {
                device_type: 3,
                device_index: 0,
                offset: 20000,
            },
        ];
        let mut full_controllers = controllers;
        full_controllers.resize(64, VirtualControllerInfo::default());

        let info = make_test_info(2020, 5, 15, 3, full_controllers);

        let c = info.controller(0, 1);
        assert!(c.is_some());
        assert_eq!(c.unwrap().offset, 15000);

        let c2 = info.controller(3, 0);
        assert!(c2.is_some());
        assert_eq!(c2.unwrap().offset, 20000);

        let c3 = info.controller(5, 0);
        assert!(c3.is_none()); // Not found
    }

    #[test]
    fn test_acquisition_date_formatting() {
        let controllers = vec![VirtualControllerInfo::default(); 64];
        let info = RawFileInfo {
            year: 2023,
            month: 7,
            day: 4,
            hour: 14,
            minute: 30,
            second: 15,
            millisecond: 123,
            controllers,
            n_controllers: 0,
            headings: vec![],
            blob_offset: -1,
            blob_size: 0,
            end_offset: 0,
        };

        assert_eq!(info.acquisition_date(), "2023-07-04T14:30:15");
    }
}
