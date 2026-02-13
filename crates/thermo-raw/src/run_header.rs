//! RunHeader stream parsing.
//!
//! The RunHeader is the primary index structure for each instrument.
//! It contains SampleInfo (scan range, time/mass range) and addresses
//! to ScanIndex, DataStream, TrailerExtra, etc.
//!
//! From decompiled RunHeader.Load version dispatch:
//! - v66+: RunHeaderStruct  (current, has InstrumentType at end)
//! - v64-65: RunHeaderStruct5 (64-bit offsets, Extra0-5, no InstrumentType)
//! - v49-63: RunHeaderStruct4 (ends at FilterMassPrecision, 32-bit offsets only)
//! - v40-48: RunHeaderStruct3
//! - v25-39: RunHeaderStruct2
//! - v<25: RunHeaderStruct1
//!
//! For v<=63, 32-bit offsets are promoted to 64-bit via ConvertFrom32Bit.
//!
//! ## v64+ Address Block Discovery
//!
//! The area between SampleInfo and the 64-bit address block contains variable-size
//! fields (sample tags, filename strings, unknown padding) whose exact sizes differ
//! across instrument types and versions. Rather than guessing field sizes, we exploit
//! a self-referential invariant: `RunHeaderPos` (the 5th i64 in the address block)
//! equals the RunHeader's own start address from VCI. By scanning for this known
//! value, we locate the address block reliably regardless of intermediate layout.

use crate::io_utils::BinaryReader;
use crate::RawError;

/// Parsed RunHeader data.
#[derive(Debug, Clone)]
pub struct RunHeader {
    pub first_scan: u32,
    pub last_scan: u32,
    pub start_time: f64,
    pub end_time: f64,
    pub low_mass: f64,
    pub high_mass: f64,
    pub max_ion_current: f64,
    /// 32-bit addresses (for v < 64).
    pub scan_index_addr_32: u32,
    pub data_addr_32: u32,
    pub scan_trailer_addr_32: u32,
    pub scan_params_addr_32: u32,
    /// 64-bit addresses (for v >= 64).
    pub scan_index_addr_64: Option<u64>,
    pub data_addr_64: Option<u64>,
    pub scan_trailer_addr_64: Option<u64>,
    pub scan_params_addr_64: Option<u64>,
    /// Instrument info strings (from PascalStringWin32 at end of RunHeader).
    pub device_name: String,
    pub model: String,
    pub serial_number: String,
    pub software_version: String,
    /// Sample info tags.
    pub sample_tag1: String,
    pub sample_tag2: String,
    pub sample_tag3: String,
    /// Instrument type identifier (v66+ only, 0 for older versions).
    pub instrument_type: i32,
    /// RunHeader's own start address (from VCI, stored for diagnostics).
    pub start_offset: u64,
    /// Byte offset after parsing.
    pub end_offset: u64,
}

impl RunHeader {
    /// Parse RunHeader from the data stream at the given absolute offset.
    pub fn parse(data: &[u8], offset: u64, version: u32) -> Result<Self, RawError> {
        let mut reader = BinaryReader::at_offset(data, offset);

        // === SampleInfo (nested at start) ===
        // Layout with default .NET struct alignment:
        //   Revision(i16,2) + pad(2) + DataSetID(i32,4) = 8 bytes
        let _revision_and_pad = reader.read_u32()?;
        let _dataset_id = reader.read_u32()?;
        let first_scan = reader.read_u32()?;
        let last_scan = reader.read_u32()?;
        let _inst_log_length = reader.read_u32()?;
        let _error_log_length = reader.read_u32()?;
        let _file_flag = reader.read_u32()?;

        let scan_index_addr_32 = reader.read_u32()?;
        let data_addr_32 = reader.read_u32()?;
        let _inst_log_addr_32 = reader.read_u32()?;
        let _error_log_addr_32 = reader.read_u32()?;
        let _max_packet_and_pad = reader.read_u32()?; // MaxPacket(i16) + padding(2)

        let max_ion_current = reader.read_f64()?;
        let low_mass = reader.read_f64()?;
        let high_mass = reader.read_f64()?;
        let start_time = reader.read_f64()?;
        let end_time = reader.read_f64()?;

        // SampleInfo is now at relative offset 88 from RunHeader start.

        let mut scan_index_addr_64 = None;
        let mut data_addr_64 = None;
        let mut scan_trailer_addr_64 = None;
        let mut scan_params_addr_64 = None;
        let mut scan_trailer_addr_32 = 0u32;
        let mut scan_params_addr_32 = 0u32;
        let mut instrument_type = 0i32;
        let mut sample_tag1 = String::new();
        let mut sample_tag2 = String::new();
        let mut sample_tag3 = String::new();

        if version >= 64 {
            // For v64+, scan for the 64-bit address block using RunHeaderPos invariant.
            let search_from = reader.position();
            let addr_block_start = find_address_block(data, search_from, offset)?;
            reader.set_position(addr_block_start);

            scan_index_addr_64 = Some(reader.read_u64()?); // SpectPos
            data_addr_64 = Some(reader.read_u64()?); // PacketPos
            let _inst_log_addr_64 = reader.read_u64()?; // StatusLogPos
            let _error_log_addr_64 = reader.read_u64()?; // ErrorLogPos
            let _run_header_addr_64 = reader.read_u64()?; // RunHeaderPos (== offset)
            scan_trailer_addr_64 = Some(reader.read_u64()?); // TrailerScanEventsPos
            scan_params_addr_64 = Some(reader.read_u64()?); // TrailerExtraPos

            // VirtualControllerInfoStruct: VirtualDeviceType(4) + VirtualDeviceIndex(4) + Offset(8) = 16
            reader.skip(16)?;
            // Extra0..5: each is Pos(i64) + Count(i32) = 12 bytes, 6 pairs = 72 bytes
            reader.skip(72)?;

            if version >= 66 {
                instrument_type = reader.read_i32()?;
            }
        } else {
            // For v<64, parse through the traditional fixed-size layout.
            reader.skip(56)?; // unknown_area after SampleInfo

            // Sample info tags (fixed-size UTF-16)
            sample_tag1 = reader.read_utf16_fixed(88)?; // 44 chars
            sample_tag2 = reader.read_utf16_fixed(40)?; // 20 chars
            sample_tag3 = reader.read_utf16_fixed(320)?; // 160 chars

            // 13 filename strings (each 260 UTF-16 chars = 520 bytes)
            for _ in 0..13 {
                reader.skip(520)?;
            }

            let _unknown_double1 = reader.read_f64()?;
            let _unknown_double2 = reader.read_f64()?;

            scan_trailer_addr_32 = reader.read_u32()?;
            scan_params_addr_32 = reader.read_u32()?;
            reader.skip(8)?; // unknown_lengths
            let _n_segments = reader.read_u32()?;
            reader.skip(16)?; // unknown4..7
            let _own_addr_32 = reader.read_u32()?;
        }

        // PascalStringWin32 strings at end of RunHeader:
        //   DeviceName, Model(?), SerialNumber(?), SoftwareVersion, Tag1-Tag4
        let device_name = reader.read_pascal_string().unwrap_or_default();
        let model = reader.read_pascal_string().unwrap_or_default();
        let serial_number = reader.read_pascal_string().unwrap_or_default();
        let software_version = reader.read_pascal_string().unwrap_or_default();

        // Tag strings (4 more PascalStrings)
        let pascal_tag1 = reader.read_pascal_string().unwrap_or_default();
        let pascal_tag2 = reader.read_pascal_string().unwrap_or_default();
        let pascal_tag3 = reader.read_pascal_string().unwrap_or_default();
        let _pascal_tag4 = reader.read_pascal_string().unwrap_or_default();

        // For v64+, we skipped the fixed-size tag area; use PascalString tags instead
        if version >= 64 {
            sample_tag1 = pascal_tag1;
            sample_tag2 = pascal_tag2;
            sample_tag3 = pascal_tag3;
        }

        Ok(Self {
            first_scan,
            last_scan,
            start_time,
            end_time,
            low_mass,
            high_mass,
            max_ion_current,
            scan_index_addr_32,
            data_addr_32,
            scan_trailer_addr_32,
            scan_params_addr_32,
            scan_index_addr_64,
            data_addr_64,
            scan_trailer_addr_64,
            scan_params_addr_64,
            device_name,
            model,
            serial_number,
            software_version,
            sample_tag1,
            sample_tag2,
            sample_tag3,
            instrument_type,
            start_offset: offset,
            end_offset: reader.position(),
        })
    }

    /// Get the best available scan index address.
    pub fn scan_index_addr(&self) -> u64 {
        self.scan_index_addr_64
            .unwrap_or(self.scan_index_addr_32 as u64)
    }

    /// Get the best available data stream address.
    pub fn data_addr(&self) -> u64 {
        self.data_addr_64.unwrap_or(self.data_addr_32 as u64)
    }

    /// Get the best available trailer extra address.
    pub fn scan_trailer_addr(&self) -> u64 {
        self.scan_trailer_addr_64
            .unwrap_or(self.scan_trailer_addr_32 as u64)
    }

    /// Get the best available scan params address.
    pub fn scan_params_addr(&self) -> u64 {
        self.scan_params_addr_64
            .unwrap_or(self.scan_params_addr_32 as u64)
    }

    /// Number of scans.
    pub fn n_scans(&self) -> u32 {
        if self.last_scan >= self.first_scan {
            self.last_scan - self.first_scan + 1
        } else {
            0
        }
    }
}

/// Find the start of the 64-bit address block within RunHeader data.
///
/// The block layout is 7 consecutive i64s:
///   SpectPos, PacketPos, StatusLogPos, ErrorLogPos,
///   RunHeaderPos, TrailerScanEventsPos, TrailerExtraPos
///
/// After the block: VCI (DeviceType:i32, DeviceIndex:i32, Offset:i64 = 16 bytes).
/// VCI.Offset equals the RunHeader's own start address.
///
/// We search for `run_header_offset` as an i64 and try two interpretations:
/// 1. RunHeaderPos at block+32 (5th i64) -- when RunHeaderPos is populated
/// 2. VCI.Offset at block+64 (8 bytes into the 16-byte VCI after the 56-byte block)
///    -- handles files where RunHeaderPos=0
fn find_address_block(
    data: &[u8],
    search_from: u64,
    run_header_offset: u64,
) -> Result<u64, RawError> {
    let target_bytes = run_header_offset.to_le_bytes();
    let file_size = data.len() as u64;

    let search_start = search_from as usize;
    let search_end = ((search_from + 8192) as usize).min(data.len());

    let mut pos = search_start;
    while pos + 8 <= search_end {
        if data[pos..pos + 8] == target_bytes {
            // Interpretation 1: this is RunHeaderPos (5th i64, at block+32)
            if pos >= 32 {
                let candidate = pos - 32;
                if candidate >= search_start
                    && candidate + 56 <= data.len()
                    && validate_address_block(data, candidate, file_size)
                {
                    return Ok(candidate as u64);
                }
            }

            // Interpretation 2: this is VCI.Offset (at block+56+8 = block+64)
            if pos >= 64 {
                let candidate = pos - 64;
                if candidate >= search_start
                    && candidate + 72 <= data.len()
                    && validate_address_block_with_vci(data, candidate, file_size)
                {
                    return Ok(candidate as u64);
                }
            }
        }
        pos += 4;
    }

    Err(RawError::CorruptedData(format!(
        "RunHeader: could not locate 64-bit address block \
             (RunHeaderPos={} not found in search range {}..{})",
        run_header_offset, search_start, search_end
    )))
}

/// Validate that the first two i64s in a candidate address block are valid file offsets.
fn validate_address_block(data: &[u8], block_start: usize, file_size: u64) -> bool {
    let spect = u64::from_le_bytes(data[block_start..block_start + 8].try_into().unwrap());
    let packet = u64::from_le_bytes(data[block_start + 8..block_start + 16].try_into().unwrap());
    spect > 0 && spect < file_size && packet > 0 && packet < file_size
}

/// Validate address block plus VCI structure (DeviceType 0-5, DeviceIndex 0-7).
/// Device types: 0=MS, 1=MSAnalog, 2=Analog, 3=UV, 4=Pda, 5=Other (from Thermo .NET Device enum).
fn validate_address_block_with_vci(data: &[u8], block_start: usize, file_size: u64) -> bool {
    let spect = u64::from_le_bytes(data[block_start..block_start + 8].try_into().unwrap());
    let packet = u64::from_le_bytes(data[block_start + 8..block_start + 16].try_into().unwrap());
    if !((spect > 0 && spect < file_size) || (packet > 0 && packet < file_size)) {
        return false;
    }
    // VCI starts at block+56: DeviceType(i32) + DeviceIndex(i32) + Offset(i64)
    let vci_start = block_start + 56;
    let device_type = i32::from_le_bytes(data[vci_start..vci_start + 4].try_into().unwrap());
    let device_index = i32::from_le_bytes(data[vci_start + 4..vci_start + 8].try_into().unwrap());
    (0..=5).contains(&device_type) && (0..=7).contains(&device_index)
}
