//! TrailerExtra parsing (scan-level metadata).
//!
//! The trailer extra uses a self-describing format:
//! 1. GenericDataHeader: count + array of GenericDataDescriptor
//! 2. GenericRecord[n_scans]: one per scan, fields match the header descriptors

use std::collections::HashMap;

use crate::io_utils::BinaryReader;
use crate::RawError;

/// Parsed trailer extra data for a single scan.
pub type TrailerExtra = HashMap<String, String>;

/// A field descriptor in the GenericDataHeader.
#[derive(Debug, Clone)]
pub struct GenericDataDescriptor {
    /// Data type code (see FORMAT_SPEC.md Section 9).
    pub type_code: u32,
    /// Field length in bytes.
    pub length: u32,
    /// Human-readable label.
    pub label: String,
}

/// Parsed GenericDataHeader (template for all trailer records).
#[derive(Debug, Clone)]
pub struct GenericDataHeader {
    pub descriptors: Vec<GenericDataDescriptor>,
    /// Byte offset after the header (where records begin).
    pub records_offset: u64,
}

/// Type codes for GenericDataDescriptor.
///
/// Empirically confirmed for v66 files: the active codes are
/// SEPARATOR(0x00), BOOL_V66(0x03), FLAG(0x04), I32(0x08), F64_ALT(0x0B), ASCII(0x0C).
pub mod type_codes {
    pub const SEPARATOR: u32 = 0x00;
    pub const BOOL: u32 = 0x01;
    pub const I8: u32 = 0x02;
    pub const BOOL_V66: u32 = 0x03;
    pub const FLAG: u32 = 0x04;
    pub const F32: u32 = 0x05;
    pub const F64: u32 = 0x06;
    pub const U8: u32 = 0x07;
    pub const I32: u32 = 0x08;
    pub const U32: u32 = 0x09;
    pub const F32_ALT: u32 = 0x0A;
    pub const F64_ALT: u32 = 0x0B;
    pub const ASCII: u32 = 0x0C;
    pub const WIDE_STRING: u32 = 0x0D;
}

/// Get the byte size of a field based on its type code and declared length.
fn field_byte_size(desc: &GenericDataDescriptor) -> usize {
    match desc.type_code {
        type_codes::SEPARATOR => 0,
        type_codes::BOOL | type_codes::I8 | type_codes::U8
        | type_codes::BOOL_V66 | type_codes::FLAG => 1,
        type_codes::I32 | type_codes::U32 | type_codes::F32 | type_codes::F32_ALT => 4,
        type_codes::F64 | type_codes::F64_ALT => 8,
        type_codes::ASCII | type_codes::WIDE_STRING => desc.length as usize,
        _ => desc.length as usize,
    }
}

impl GenericDataHeader {
    /// Override the records_offset (where per-scan records begin).
    ///
    /// In v66, the GDH is stored before SpectPos but records live at TrailerExtraPos.
    pub fn with_records_offset(mut self, offset: u64) -> Self {
        self.records_offset = offset;
        self
    }
}

/// Pre-computed layout for fast trailer field access.
///
/// Caches field byte offsets and indices of commonly-used fields for O(1) lookup
/// instead of re-computing on every scan.
#[derive(Debug, Clone)]
pub struct TrailerLayout {
    pub header: GenericDataHeader,
    pub record_size: usize,
    /// Byte offset of each field within a record.
    pub field_offsets: Vec<usize>,
    /// Index of "Filter Text" field (if present).
    pub filter_text_idx: Option<usize>,
    /// Index of "Charge State" field.
    pub charge_state_idx: Option<usize>,
    /// Index of "Monoisotopic M/Z" field.
    pub mono_mz_idx: Option<usize>,
    /// Index of "Ion Injection Time (ms)" field.
    pub injection_time_idx: Option<usize>,
    /// Index of "Master Scan Number" field.
    pub master_scan_idx: Option<usize>,
    /// Index of "MS2 Isolation Width" field.
    pub isolation_width_idx: Option<usize>,
}

impl TrailerLayout {
    /// Build a TrailerLayout from a parsed GenericDataHeader.
    pub fn from_header(header: GenericDataHeader) -> Self {
        let mut field_offsets = Vec::with_capacity(header.descriptors.len());
        let mut offset = 0usize;
        for desc in &header.descriptors {
            field_offsets.push(offset);
            offset += field_byte_size(desc);
        }
        let record_size = offset;

        let find_field = |name: &str| -> Option<usize> {
            header.descriptors.iter().position(|d| {
                d.label
                    .trim_end_matches(':')
                    .trim()
                    .eq_ignore_ascii_case(name)
            })
        };

        let filter_text_idx = find_field("Filter Text");
        let charge_state_idx = find_field("Charge State");
        let mono_mz_idx = find_field("Monoisotopic M/Z");
        let injection_time_idx = find_field("Ion Injection Time (ms)");
        let master_scan_idx =
            find_field("Master Scan Number").or_else(|| find_field("Master Index"));
        let isolation_width_idx = find_field("MS2 Isolation Width");

        Self {
            header,
            record_size,
            field_offsets,
            filter_text_idx,
            charge_state_idx,
            mono_mz_idx,
            injection_time_idx,
            master_scan_idx,
            isolation_width_idx,
        }
    }

    /// Get the absolute byte offset of a field for a given scan.
    fn field_offset(&self, scan_index: u32, field_idx: usize) -> u64 {
        self.header.records_offset
            + (scan_index as u64) * (self.record_size as u64)
            + self.field_offsets[field_idx] as u64
    }

    /// Read a specific field as f64.
    pub fn read_f64(
        &self,
        data: &[u8],
        scan_index: u32,
        field_idx: usize,
    ) -> Result<f64, RawError> {
        let offset = self.field_offset(scan_index, field_idx);
        let mut reader = BinaryReader::at_offset(data, offset);
        let desc = &self.header.descriptors[field_idx];
        match desc.type_code {
            type_codes::F64 | type_codes::F64_ALT => reader.read_f64(),
            type_codes::F32 | type_codes::F32_ALT => Ok(reader.read_f32()? as f64),
            type_codes::I32 | type_codes::U32 => Ok(reader.read_i32()? as f64),
            type_codes::FLAG | type_codes::BOOL_V66 | type_codes::I8 | type_codes::U8 => {
                Ok(reader.read_u8()? as f64)
            }
            _ => Err(RawError::CorruptedData(format!(
                "Cannot read field '{}' as f64 (type_code=0x{:X})",
                desc.label, desc.type_code
            ))),
        }
    }

    /// Read a specific field as i32.
    pub fn read_i32(
        &self,
        data: &[u8],
        scan_index: u32,
        field_idx: usize,
    ) -> Result<i32, RawError> {
        let offset = self.field_offset(scan_index, field_idx);
        let mut reader = BinaryReader::at_offset(data, offset);
        let desc = &self.header.descriptors[field_idx];
        match desc.type_code {
            type_codes::I32 => reader.read_i32(),
            type_codes::U32 => Ok(reader.read_u32()? as i32),
            type_codes::FLAG | type_codes::BOOL_V66 | type_codes::I8 | type_codes::U8 => {
                Ok(reader.read_u8()? as i32)
            }
            _ => Err(RawError::CorruptedData(format!(
                "Cannot read field '{}' as i32 (type_code=0x{:X})",
                desc.label, desc.type_code
            ))),
        }
    }

    /// Read a specific field as string.
    pub fn read_string(
        &self,
        data: &[u8],
        scan_index: u32,
        field_idx: usize,
    ) -> Result<String, RawError> {
        let offset = self.field_offset(scan_index, field_idx);
        let mut reader = BinaryReader::at_offset(data, offset);
        read_field_as_string(&mut reader, &self.header.descriptors[field_idx])
    }

    /// Get field labels.
    pub fn field_labels(&self) -> Vec<String> {
        self.header
            .descriptors
            .iter()
            .map(|d| d.label.trim_end_matches(':').trim().to_string())
            .collect()
    }
}

/// Parse the GenericDataHeader at the given offset.
pub fn parse_generic_data_header(data: &[u8], offset: u64) -> Result<GenericDataHeader, RawError> {
    let mut reader = BinaryReader::at_offset(data, offset);

    let n_fields = reader.read_u32()?;
    if n_fields > 10_000 {
        return Err(RawError::CorruptedData(format!(
            "GenericDataHeader has unreasonable field count: {}",
            n_fields
        )));
    }

    let mut descriptors = Vec::with_capacity(n_fields as usize);
    for _ in 0..n_fields {
        let type_code = reader.read_u32()?;
        let length = reader.read_u32()?;
        let label = reader.read_pascal_string()?;
        descriptors.push(GenericDataDescriptor {
            type_code,
            length,
            label,
        });
    }

    Ok(GenericDataHeader {
        descriptors,
        records_offset: reader.position(),
    })
}

/// Known type codes found in v66 GenericDataHeaders.
const VALID_V66_TYPE_CODES: [u32; 6] = [0x00, 0x03, 0x04, 0x08, 0x0B, 0x0C];

/// Search backward from `spect_pos` to find the GenericDataHeader.
///
/// In v66 files, the GDH (field descriptors for trailer records) is stored
/// several KB before SpectPos in the data stream, NOT at TrailerScanEventsPos
/// or TrailerExtraPos (which point to flat record arrays with no header).
pub fn find_generic_data_header(data: &[u8], spect_pos: u64) -> Result<GenericDataHeader, RawError> {
    let search_window = 20480u64; // 20KB before SpectPos
    let search_start = spect_pos.saturating_sub(search_window) as usize;
    let search_end = spect_pos as usize;

    // Try 4-byte aligned steps first (the u32 n_fields count is almost certainly aligned).
    // This reduces iterations from ~20K to ~5K for the common case.
    let aligned_start = (search_start + 3) & !3; // round up to next 4-byte boundary
    let mut pos = aligned_start;
    while pos + 4 <= search_end {
        let n_fields = u32::from_le_bytes(data[pos..pos + 4].try_into().unwrap());
        if (10..=300).contains(&n_fields) {
            if let Ok(header) = parse_generic_data_header(data, pos as u64) {
                let all_valid = header.descriptors.iter().all(|d| {
                    VALID_V66_TYPE_CODES.contains(&d.type_code)
                });
                if all_valid && header.descriptors.len() >= 5 {
                    return Ok(header);
                }
            }
        }
        pos += 4;
    }

    // Fallback: 1-byte steps for unaligned cases.
    pos = search_start;
    while pos + 4 <= search_end {
        // Skip positions already checked in aligned pass
        if pos >= aligned_start && (pos - aligned_start) % 4 == 0 {
            pos += 1;
            continue;
        }
        let n_fields = u32::from_le_bytes(data[pos..pos + 4].try_into().unwrap());
        if (10..=300).contains(&n_fields) {
            if let Ok(header) = parse_generic_data_header(data, pos as u64) {
                let all_valid = header.descriptors.iter().all(|d| {
                    VALID_V66_TYPE_CODES.contains(&d.type_code)
                });
                if all_valid && header.descriptors.len() >= 5 {
                    return Ok(header);
                }
            }
        }
        pos += 1;
    }

    Err(RawError::StreamNotFound(
        "GenericDataHeader not found before SpectPos".to_string(),
    ))
}

/// Parse trailer extra data for a specific scan.
///
/// `scan_index` is 0-based (scan_number - first_scan).
pub fn parse_trailer_extra(
    data: &[u8],
    header: &GenericDataHeader,
    scan_index: u32,
) -> Result<TrailerExtra, RawError> {
    let rec_size: usize = header.descriptors.iter().map(|d| field_byte_size(d)).sum();
    let rec_offset = header.records_offset + (scan_index as u64) * (rec_size as u64);

    let mut reader = BinaryReader::at_offset(data, rec_offset);
    let mut result = HashMap::new();

    for desc in &header.descriptors {
        let label = desc.label.trim_end_matches(':').trim().to_string();
        let value = read_field_as_string(&mut reader, desc)?;
        result.insert(label, value);
    }

    Ok(result)
}

/// Read a single field value as a string representation.
fn read_field_as_string(
    reader: &mut BinaryReader,
    desc: &GenericDataDescriptor,
) -> Result<String, RawError> {
    match desc.type_code {
        type_codes::SEPARATOR => Ok(String::new()),
        type_codes::BOOL | type_codes::BOOL_V66 => {
            let v = reader.read_u8()?;
            Ok(if v != 0 { "true" } else { "false" }.to_string())
        }
        type_codes::I8 => {
            let v = reader.read_u8()? as i8;
            Ok(v.to_string())
        }
        type_codes::FLAG | type_codes::U8 => {
            let v = reader.read_u8()?;
            Ok(v.to_string())
        }
        type_codes::I32 => {
            let v = reader.read_i32()?;
            Ok(v.to_string())
        }
        type_codes::U32 => {
            let v = reader.read_u32()?;
            Ok(v.to_string())
        }
        type_codes::F32 | type_codes::F32_ALT => {
            let v = reader.read_f32()?;
            Ok(format!("{}", v))
        }
        type_codes::F64 | type_codes::F64_ALT => {
            let v = reader.read_f64()?;
            Ok(format!("{}", v))
        }
        type_codes::ASCII => {
            let bytes = reader.read_bytes(desc.length as usize)?;
            let s = String::from_utf8_lossy(&bytes)
                .trim_end_matches('\0')
                .to_string();
            Ok(s)
        }
        type_codes::WIDE_STRING => {
            let s = reader.read_utf16_fixed(desc.length as usize)?;
            Ok(s)
        }
        _ => {
            // Unknown type: skip bytes based on declared length
            let skip = field_byte_size(desc);
            reader.skip(skip)?;
            Ok(String::new())
        }
    }
}

/// Get the list of trailer extra field labels.
pub fn parse_trailer_fields(data: &[u8], trailer_addr: u64) -> Result<Vec<String>, RawError> {
    let header = parse_generic_data_header(data, trailer_addr)?;
    Ok(header
        .descriptors
        .iter()
        .map(|d| d.label.trim_end_matches(':').trim().to_string())
        .collect())
}

#[cfg(test)]
mod tests {
    use super::*;

    /// Build a minimal GenericDataHeader + record data for testing TrailerLayout.
    fn build_test_data() -> (Vec<u8>, GenericDataHeader) {
        // Build header with 3 fields using v66-correct type codes:
        // i32 "Charge State" (0x08, 4B), f64 "Monoisotopic M/Z" (0x0B, 8B), flag "Access Id" (0x04, 1B)
        let descriptors = vec![
            GenericDataDescriptor {
                type_code: type_codes::I32,
                length: 4,
                label: "Charge State:".to_string(),
            },
            GenericDataDescriptor {
                type_code: type_codes::F64_ALT,
                length: 8,
                label: "Monoisotopic M/Z:".to_string(),
            },
            GenericDataDescriptor {
                type_code: type_codes::FLAG,
                length: 1,
                label: "Access Id:".to_string(),
            },
        ];
        // record_size = 4 + 8 + 1 = 13 bytes
        let records_offset = 0u64;

        // Build 2 records:
        // Record 0: charge=2, mz=524.2648, access_id=1
        // Record 1: charge=3, mz=445.120, access_id=2
        let mut data = Vec::new();
        // Record 0
        data.extend_from_slice(&2i32.to_le_bytes());
        data.extend_from_slice(&524.2648f64.to_le_bytes());
        data.push(1u8);
        // Record 1
        data.extend_from_slice(&3i32.to_le_bytes());
        data.extend_from_slice(&445.120f64.to_le_bytes());
        data.push(2u8);

        let header = GenericDataHeader {
            descriptors,
            records_offset,
        };

        (data, header)
    }

    #[test]
    fn test_trailer_layout_field_indices() {
        let (_, header) = build_test_data();
        let layout = TrailerLayout::from_header(header);

        assert_eq!(layout.record_size, 13);
        assert_eq!(layout.field_offsets, vec![0, 4, 12]);
        assert_eq!(layout.charge_state_idx, Some(0));
        assert_eq!(layout.mono_mz_idx, Some(1));
        assert!(layout.filter_text_idx.is_none());
    }

    #[test]
    fn test_trailer_layout_read_typed() {
        let (data, header) = build_test_data();
        let layout = TrailerLayout::from_header(header);

        // Record 0
        assert_eq!(layout.read_i32(&data, 0, 0).unwrap(), 2);
        assert!((layout.read_f64(&data, 0, 1).unwrap() - 524.2648).abs() < 1e-4);

        // Record 1
        assert_eq!(layout.read_i32(&data, 1, 0).unwrap(), 3);
        assert!((layout.read_f64(&data, 1, 1).unwrap() - 445.120).abs() < 1e-3);
    }

    #[test]
    fn test_trailer_layout_field_labels() {
        let (_, header) = build_test_data();
        let layout = TrailerLayout::from_header(header);
        let labels = layout.field_labels();
        assert_eq!(labels, vec!["Charge State", "Monoisotopic M/Z", "Access Id"]);
    }
}
