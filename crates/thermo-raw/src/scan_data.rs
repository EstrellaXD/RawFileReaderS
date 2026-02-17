//! ScanData decoding -- the core difficulty of the project.
//!
//! Each scan's raw data is stored as a ScanDataPacket at an offset in the
//! data stream. The packet has a 40-byte header followed by profile data,
//! peak list (centroids), peak descriptors, and additional streams.

use crate::io_utils::BinaryReader;
use crate::scan_data_centroid;
use crate::scan_data_ftlt;
use crate::scan_data_profile;
use crate::scan_index::ScanIndexEntry;
use crate::types::{MsLevel, Polarity, Scan};
use crate::RawError;

/// Parsed ScanDataPacket header (40 bytes).
#[derive(Debug, Clone)]
pub struct PacketHeader {
    pub unknown1: u32,
    /// Number of 4-byte words of profile data.
    pub profile_size: u32,
    /// Number of 4-byte words of peak list data.
    pub peak_list_size: u32,
    /// Layout flag: 0 = no fudge, >0 = with fudge per profile chunk.
    pub layout: u32,
    /// Number of peak descriptor entries.
    pub descriptor_list_size: u32,
    /// Size of unknown stream.
    pub unknown_stream_size: u32,
    /// Size of triplet stream.
    pub triplet_stream_size: u32,
    pub unknown2: u32,
    pub low_mz: f32,
    pub high_mz: f32,
}

impl PacketHeader {
    pub fn parse(reader: &mut BinaryReader) -> Result<Self, RawError> {
        Ok(Self {
            unknown1: reader.read_u32()?,
            profile_size: reader.read_u32()?,
            peak_list_size: reader.read_u32()?,
            layout: reader.read_u32()?,
            descriptor_list_size: reader.read_u32()?,
            unknown_stream_size: reader.read_u32()?,
            triplet_stream_size: reader.read_u32()?,
            unknown2: reader.read_u32()?,
            low_mz: reader.read_f32()?,
            high_mz: reader.read_f32()?,
        })
    }

    pub const SIZE: usize = 40;
}

/// Decode only centroid m/z + intensity from a scan, skipping profile data.
///
/// Returns `(mz_array, intensity_array)`. Used by XIC extraction to avoid
/// decoding expensive profile data and allocating full `Scan` structs.
pub fn decode_centroids_only(
    data: &[u8],
    data_addr: usize,
    entry: &ScanIndexEntry,
) -> Result<(Vec<f64>, Vec<f64>), RawError> {
    let abs_offset = data_addr as u64 + entry.offset;

    if entry.data_size > 0 {
        if abs_offset as usize + entry.data_size as usize > data.len() {
            return Ok((vec![], vec![]));
        }
    } else if abs_offset as usize >= data.len() {
        return Ok((vec![], vec![]));
    }

    if entry.number_packets == 0 && entry.data_size == 0 {
        return Ok((vec![], vec![]));
    }

    let packet_type_id = (entry.packet_type & 0xFFFF) as u16;

    match packet_type_id {
        18..=21 => scan_data_ftlt::decode_ftlt_centroids_only(data, abs_offset),
        0..=5 | 14..=17 => decode_legacy_centroids_only(data, abs_offset),
        _ => Ok((vec![], vec![])),
    }
}

/// Extract only centroid data from a legacy packet, skipping profile.
fn decode_legacy_centroids_only(
    data: &[u8],
    abs_offset: u64,
) -> Result<(Vec<f64>, Vec<f64>), RawError> {
    let mut reader = BinaryReader::at_offset(data, abs_offset);
    let header = PacketHeader::parse(&mut reader)?;

    // Skip profile data
    if header.profile_size > 0 {
        reader.skip(header.profile_size as usize * 4)?;
    }

    // Read centroid data
    if header.peak_list_size > 0 {
        let peak_start = reader.position();
        scan_data_centroid::decode_centroid(data, peak_start as usize)
    } else {
        Ok((vec![], vec![]))
    }
}

/// Sum centroid intensities within [mz_low, mz_high] directly from raw scan bytes.
///
/// Zero allocations: dispatches to the appropriate decoder which reads bytes in-place.
/// Returns the total intensity sum for peaks in the m/z window.
pub fn sum_centroids_in_mz_range(
    data: &[u8],
    data_addr: usize,
    entry: &ScanIndexEntry,
    mz_low: f64,
    mz_high: f64,
) -> Result<f64, RawError> {
    let abs_offset = data_addr as u64 + entry.offset;

    if entry.data_size > 0 {
        if abs_offset as usize + entry.data_size as usize > data.len() {
            return Ok(0.0);
        }
    } else if abs_offset as usize >= data.len() {
        return Ok(0.0);
    }

    if entry.number_packets == 0 && entry.data_size == 0 {
        return Ok(0.0);
    }

    let packet_type_id = (entry.packet_type & 0xFFFF) as u16;

    match packet_type_id {
        18..=21 => scan_data_ftlt::sum_centroids_in_range_ftlt(data, abs_offset, mz_low, mz_high),
        0..=5 | 14..=17 => sum_legacy_centroids_in_range(data, abs_offset, mz_low, mz_high),
        _ => Ok(0.0),
    }
}

/// Sum centroid intensities for multiple m/z ranges in a single scan pass.
///
/// `sorted_ranges` must be sorted by low bound. `out[i]` receives the total
/// intensity for `sorted_ranges[i]`. Zero allocations.
pub fn sum_centroids_multi_target(
    data: &[u8],
    data_addr: usize,
    entry: &ScanIndexEntry,
    sorted_ranges: &[(f64, f64)],
    out: &mut [f64],
) -> Result<(), RawError> {
    let abs_offset = data_addr as u64 + entry.offset;

    let mut empty = || {
        for v in out.iter_mut().take(sorted_ranges.len()) {
            *v = 0.0;
        }
    };

    if entry.data_size > 0 {
        if abs_offset as usize + entry.data_size as usize > data.len() {
            empty();
            return Ok(());
        }
    } else if abs_offset as usize >= data.len() {
        empty();
        return Ok(());
    }

    if entry.number_packets == 0 && entry.data_size == 0 {
        empty();
        return Ok(());
    }

    let packet_type_id = (entry.packet_type & 0xFFFF) as u16;

    match packet_type_id {
        18..=21 => scan_data_ftlt::sum_centroids_multi_target_ftlt(
            data,
            abs_offset,
            sorted_ranges,
            out,
        ),
        0..=5 | 14..=17 => {
            sum_legacy_centroids_multi_target(data, abs_offset, sorted_ranges, out)
        }
        _ => {
            empty();
            Ok(())
        }
    }
}

/// Sum legacy centroid intensities in a single m/z range (skip profile, read centroids).
fn sum_legacy_centroids_in_range(
    data: &[u8],
    abs_offset: u64,
    mz_low: f64,
    mz_high: f64,
) -> Result<f64, RawError> {
    let mut reader = BinaryReader::at_offset(data, abs_offset);
    let header = PacketHeader::parse(&mut reader)?;

    if header.profile_size > 0 {
        reader.skip(header.profile_size as usize * 4)?;
    }

    if header.peak_list_size > 0 {
        let peak_start = reader.position();
        scan_data_centroid::sum_centroids_in_range(data, peak_start as usize, mz_low, mz_high)
    } else {
        Ok(0.0)
    }
}

/// Sum legacy centroid intensities for multiple m/z ranges (skip profile, read centroids).
fn sum_legacy_centroids_multi_target(
    data: &[u8],
    abs_offset: u64,
    sorted_ranges: &[(f64, f64)],
    out: &mut [f64],
) -> Result<(), RawError> {
    let mut reader = BinaryReader::at_offset(data, abs_offset);
    let header = PacketHeader::parse(&mut reader)?;

    if header.profile_size > 0 {
        reader.skip(header.profile_size as usize * 4)?;
    }

    if header.peak_list_size > 0 {
        let peak_start = reader.position();
        scan_data_centroid::sum_centroids_multi_target(
            data,
            peak_start as usize,
            sorted_ranges,
            out,
        )
    } else {
        for v in out.iter_mut().take(sorted_ranges.len()) {
            *v = 0.0;
        }
        Ok(())
    }
}

/// Decode a single scan from the data stream.
///
/// `data` is the full file data. `data_addr` is the base address of the data
/// stream. `entry` provides the scan's offset and data size.
/// `conversion_params` are the Hz-to-m/z coefficients from the ScanEvent,
/// needed for FT profile frequency conversion.
pub fn decode_scan(
    data: &[u8],
    data_addr: usize,
    entry: &ScanIndexEntry,
    scan_number: u32,
    conversion_params: &[f64],
) -> Result<Scan, RawError> {
    let abs_offset = data_addr as u64 + entry.offset;
    // Bounds check: for v65+ we have DataSize; for v<65 just verify offset is valid
    if entry.data_size > 0 {
        if abs_offset as usize + entry.data_size as usize > data.len() {
            return Err(RawError::ScanDecodeError {
                offset: abs_offset as usize,
                reason: format!(
                    "scan {} data extends beyond file (offset={}, size={}, file_len={})",
                    scan_number,
                    abs_offset,
                    entry.data_size,
                    data.len()
                ),
            });
        }
    } else if abs_offset as usize >= data.len() {
        return Err(RawError::ScanDecodeError {
            offset: abs_offset as usize,
            reason: format!(
                "scan {} data offset beyond file (offset={}, file_len={})",
                scan_number,
                abs_offset,
                data.len()
            ),
        });
    }

    // Empty scan: no packets and no data size means nothing to decode
    if entry.number_packets == 0 && entry.data_size == 0 {
        return Ok(Scan {
            scan_number,
            rt: entry.rt,
            ms_level: MsLevel::Ms1,
            polarity: Polarity::Unknown,
            tic: entry.tic,
            base_peak_mz: entry.base_peak_mz,
            base_peak_intensity: entry.base_peak_intensity,
            centroid_mz: vec![],
            centroid_intensity: vec![],
            profile_mz: None,
            profile_intensity: None,
            precursor: None,
            filter_string: None,
        });
    }

    // Dispatch on packet type: LOWORD selects the decoder class
    let packet_type_id = (entry.packet_type & 0xFFFF) as u16;

    match packet_type_id {
        // FT/LT packet types (modern instruments)
        18..=21 => decode_scan_ftlt(
            data,
            abs_offset,
            entry,
            scan_number,
            packet_type_id,
            conversion_params,
        ),
        // Legacy packet types
        0..=5 | 14..=17 => decode_scan_legacy(data, abs_offset, entry, scan_number),
        // Unknown packet type: return empty scan
        _ => Ok(Scan {
            scan_number,
            rt: entry.rt,
            ms_level: MsLevel::Ms1,
            polarity: Polarity::Unknown,
            tic: entry.tic,
            base_peak_mz: entry.base_peak_mz,
            base_peak_intensity: entry.base_peak_intensity,
            centroid_mz: vec![],
            centroid_intensity: vec![],
            profile_mz: None,
            profile_intensity: None,
            precursor: None,
            filter_string: None,
        }),
    }
}

/// Decode a scan using the FT/LT packet format (packet types 18-21).
fn decode_scan_ftlt(
    data: &[u8],
    abs_offset: u64,
    entry: &ScanIndexEntry,
    scan_number: u32,
    packet_type_id: u16,
    conversion_params: &[f64],
) -> Result<Scan, RawError> {
    let result =
        scan_data_ftlt::decode_ftlt_scan(data, abs_offset, packet_type_id, conversion_params)?;

    Ok(Scan {
        scan_number,
        rt: entry.rt,
        ms_level: MsLevel::Ms1,
        polarity: Polarity::Unknown,
        tic: entry.tic,
        base_peak_mz: entry.base_peak_mz,
        base_peak_intensity: entry.base_peak_intensity,
        centroid_mz: result.centroid_mz,
        centroid_intensity: result.centroid_intensity,
        profile_mz: result.profile_mz,
        profile_intensity: result.profile_intensity,
        precursor: None,
        filter_string: None,
    })
}

/// Decode a scan using the legacy 40-byte packet header (packet types 0-5, 14-17).
fn decode_scan_legacy(
    data: &[u8],
    abs_offset: u64,
    entry: &ScanIndexEntry,
    scan_number: u32,
) -> Result<Scan, RawError> {
    let mut reader = BinaryReader::at_offset(data, abs_offset);
    let header = PacketHeader::parse(&mut reader)?;

    // Read profile data
    let (profile_mz, profile_intensity) = if header.profile_size > 0 {
        let profile_bytes = header.profile_size as usize * 4;
        let profile_start = reader.position();
        let result = scan_data_profile::decode_profile(data, profile_start as usize, header.layout);
        reader.set_position(profile_start + profile_bytes as u64);
        match result {
            Ok((mz, int)) => (Some(mz), Some(int)),
            Err(_) => (None, None),
        }
    } else {
        (None, None)
    };

    // Read peak list (centroid data)
    let (centroid_mz, centroid_intensity) = if header.peak_list_size > 0 {
        let peak_bytes = header.peak_list_size as usize * 4;
        let peak_start = reader.position();
        let result = scan_data_centroid::decode_centroid(data, peak_start as usize);
        reader.set_position(peak_start + peak_bytes as u64);
        match result {
            Ok((mz, int)) => (mz, int),
            Err(_) => (vec![], vec![]),
        }
    } else {
        (vec![], vec![])
    };

    Ok(Scan {
        scan_number,
        rt: entry.rt,
        ms_level: MsLevel::Ms1,
        polarity: Polarity::Unknown,
        tic: entry.tic,
        base_peak_mz: entry.base_peak_mz,
        base_peak_intensity: entry.base_peak_intensity,
        centroid_mz,
        centroid_intensity,
        profile_mz,
        profile_intensity,
        precursor: None,
        filter_string: None,
    })
}
