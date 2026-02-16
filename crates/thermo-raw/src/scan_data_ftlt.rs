//! FT/LT packet decoding for modern instruments (packet types 18-21).
//!
//! Modern Thermo instruments (Orbitrap, Exploris, Q Exactive, LTQ) use a
//! 32-byte PacketHeaderStruct instead of the legacy 40-byte header.
//! The data is organized by segments, each with profile sub-segments
//! and centroid peaks.
//!
//! From decompiled v8.0.6: AdvancedPacketBase, FtProfilePacket,
//! FtCentroidPacket, LinearTrapProfilePacket, LinearTrapCentroidPacket.

use crate::io_utils::BinaryReader;
use crate::scan_event::frequency_to_mz;
use crate::RawError;

/// FT/LT packet header (PacketHeaderStruct, 32 bytes).
#[derive(Debug, Clone)]
pub struct FtLtPacketHeader {
    pub num_segments: u32,
    pub num_profile_words: u32,
    pub num_centroid_words: u32,
    pub default_feature_word: u32,
    pub num_non_default_feature_words: u32,
    pub num_expansion_words: u32,
    pub num_noise_info_words: u32,
    pub num_debug_info_words: u32,
}

impl FtLtPacketHeader {
    pub const SIZE: usize = 32;

    pub fn parse(reader: &mut BinaryReader) -> Result<Self, RawError> {
        Ok(Self {
            num_segments: reader.read_u32()?,
            num_profile_words: reader.read_u32()?,
            num_centroid_words: reader.read_u32()?,
            default_feature_word: reader.read_u32()?,
            num_non_default_feature_words: reader.read_u32()?,
            num_expansion_words: reader.read_u32()?,
            num_noise_info_words: reader.read_u32()?,
            num_debug_info_words: reader.read_u32()?,
        })
    }

    /// Whether this is an LT (Linear Trap) packet (bit 0x40 set).
    /// If clear, it's an FT (Fourier Transform) packet.
    pub fn is_lt_mode(&self) -> bool {
        self.default_feature_word & 0x40 != 0
    }

    /// Whether centroids use accurate mass (f64 instead of f32).
    pub fn is_accurate_mass(&self) -> bool {
        self.default_feature_word & 0x10000 != 0
    }

    /// Bytes per centroid peak based on accuracy mode.
    pub fn bytes_per_centroid_peak(&self) -> usize {
        if self.is_accurate_mass() {
            12 // f64 mass + f32 intensity
        } else {
            8 // f32 mass + f32 intensity
        }
    }
}

/// Segment mass range (2 x f32 = 8 bytes per segment).
#[derive(Debug, Clone)]
pub struct SegmentMassRange {
    pub low: f32,
    pub high: f32,
}

/// Decoded result from an FT/LT scan.
#[derive(Debug)]
pub struct FtLtScanResult {
    pub centroid_mz: Vec<f64>,
    pub centroid_intensity: Vec<f64>,
    pub profile_mz: Option<Vec<f64>>,
    pub profile_intensity: Option<Vec<f64>>,
}

/// Decode a complete FT/LT scan packet.
///
/// `data`: full file data
/// `abs_offset`: absolute byte offset of the packet start
/// `packet_type_id`: LOWORD of ScanIndexEntry.PacketType (18-21)
/// `conversion_params`: from ScanEvent, needed for FT frequency-to-m/z conversion
pub fn decode_ftlt_scan(
    data: &[u8],
    abs_offset: u64,
    packet_type_id: u16,
    conversion_params: &[f64],
) -> Result<FtLtScanResult, RawError> {
    let mut reader = BinaryReader::at_offset(data, abs_offset);
    let header = FtLtPacketHeader::parse(&mut reader)?;

    // Read segment mass ranges
    let mut _segment_ranges = Vec::with_capacity(header.num_segments as usize);
    for _ in 0..header.num_segments {
        let low = reader.read_f32()?;
        let high = reader.read_f32()?;
        _segment_ranges.push(SegmentMassRange { low, high });
    }

    // Mark the start of profile data
    let profile_start = reader.position();
    let profile_bytes = header.num_profile_words as u64 * 4;

    // Decode profile data if this is a profile packet type (19 or 21)
    let (profile_mz, profile_intensity) =
        if (packet_type_id == 19 || packet_type_id == 21) && header.num_profile_words > 0 {
            let is_ft = !header.is_lt_mode();
            match decode_ftlt_profile(&mut reader, &header, conversion_params, is_ft) {
                Ok((mz, int)) => {
                    // Ensure reader is past the profile section
                    reader.set_position(profile_start + profile_bytes);
                    (Some(mz), Some(int))
                }
                Err(_) => {
                    // Skip profile data on decode failure
                    reader.set_position(profile_start + profile_bytes);
                    (None, None)
                }
            }
        } else {
            // Skip profile data for centroid-only packets (18, 20)
            reader.set_position(profile_start + profile_bytes);
            (None, None)
        };

    // Mark the start of centroid data
    let centroid_start = reader.position();
    let centroid_bytes = header.num_centroid_words as u64 * 4;

    // Decode centroid data
    let (centroid_mz, centroid_intensity) = if header.num_centroid_words > 0 {
        match decode_ftlt_centroids(&mut reader, &header) {
            Ok((mz, int)) => {
                reader.set_position(centroid_start + centroid_bytes);
                (mz, int)
            }
            Err(_) => {
                reader.set_position(centroid_start + centroid_bytes);
                (vec![], vec![])
            }
        }
    } else {
        (vec![], vec![])
    };

    // Skip remaining sections (features, expansion, noise, debug)
    // We don't need them for basic m/z + intensity extraction

    Ok(FtLtScanResult {
        centroid_mz,
        centroid_intensity,
        profile_mz,
        profile_intensity,
    })
}

/// Decode FT/LT centroid data.
///
/// Layout: for each segment, a u32 peak count followed by that many peaks.
/// Peak format depends on the accurate mass flag:
/// - Standard: f32 mass + f32 intensity = 8 bytes
/// - Accurate: f64 mass + f32 intensity = 12 bytes
fn decode_ftlt_centroids(
    reader: &mut BinaryReader,
    header: &FtLtPacketHeader,
) -> Result<(Vec<f64>, Vec<f64>), RawError> {
    let accurate = header.is_accurate_mass();
    let total_centroid_bytes = header.num_centroid_words as usize * 4;

    // Estimate capacity from total byte count
    let bytes_per_peak = header.bytes_per_centroid_peak();
    let estimated_peaks = if bytes_per_peak > 0 {
        total_centroid_bytes / bytes_per_peak
    } else {
        0
    };

    let mut all_mz = Vec::with_capacity(estimated_peaks);
    let mut all_intensity = Vec::with_capacity(estimated_peaks);

    for _ in 0..header.num_segments {
        let count = reader.read_u32()?;
        if count > 10_000_000 {
            return Err(RawError::CorruptedData(format!(
                "FT/LT centroid: unreasonable peak count {} in segment",
                count
            )));
        }

        for _ in 0..count {
            let mz = if accurate {
                reader.read_f64()?
            } else {
                reader.read_f32()? as f64
            };
            let intensity = reader.read_f32()? as f64;
            all_mz.push(mz);
            all_intensity.push(intensity);
        }
    }

    Ok((all_mz, all_intensity))
}

/// Decode FT/LT profile data.
///
/// Layout per segment:
///   ProfileSegmentStruct (32 bytes): base_abscissa, spacing, n_subsegments, n_expanded, padding
///   For each sub-segment:
///     ProfileSubsegmentStruct (8 bytes): start_index, word_count
///     Intensity values: u32[word_count] (read as f32 bit pattern)
///
/// For FT mode, base_abscissa is a frequency that must be converted using conversion_params.
/// For LT mode, base_abscissa is m/z directly.
fn decode_ftlt_profile(
    reader: &mut BinaryReader,
    header: &FtLtPacketHeader,
    conversion_params: &[f64],
    is_ft: bool,
) -> Result<(Vec<f64>, Vec<f64>), RawError> {
    let mut all_mz = Vec::new();
    let mut all_intensity = Vec::new();

    for _ in 0..header.num_segments {
        // ProfileSegmentStruct (32 bytes)
        let base_abscissa = reader.read_f64()?;
        let abscissa_spacing = reader.read_f64()?;
        let num_subsegments = reader.read_u32()?;
        let _num_expanded_words = reader.read_u32()?;
        // 8 bytes padding
        reader.skip(8)?;

        if num_subsegments > 100_000 {
            return Err(RawError::CorruptedData(format!(
                "FT/LT profile: unreasonable subsegment count {}",
                num_subsegments
            )));
        }

        for _ in 0..num_subsegments {
            // ProfileSubsegmentStruct (8 bytes)
            let start_index = reader.read_u32()?;
            let word_count = reader.read_u32()?;

            if word_count > 10_000_000 {
                return Err(RawError::CorruptedData(format!(
                    "FT/LT profile: unreasonable word count {}",
                    word_count
                )));
            }

            // Read intensity values (u32 interpreted as f32)
            for i in 0..word_count {
                let raw_bits = reader.read_u32()?;
                let intensity = f32::from_bits(raw_bits) as f64;

                let idx = start_index + i;
                let abscissa = base_abscissa + (idx as f64) * abscissa_spacing;

                let mz = if is_ft && !conversion_params.is_empty() {
                    frequency_to_mz(abscissa, conversion_params)
                } else {
                    abscissa
                };

                all_mz.push(mz);
                all_intensity.push(intensity);
            }
        }
    }

    Ok((all_mz, all_intensity))
}

/// Decode only centroid data from an FT/LT scan packet, skipping profile entirely.
///
/// Returns `(mz_array, intensity_array)`. Used by XIC extraction where profile
/// data is not needed, avoiding the expensive frequency-to-m/z conversion.
pub fn decode_ftlt_centroids_only(
    data: &[u8],
    abs_offset: u64,
) -> Result<(Vec<f64>, Vec<f64>), RawError> {
    let mut reader = BinaryReader::at_offset(data, abs_offset);
    let header = FtLtPacketHeader::parse(&mut reader)?;

    // Skip segment mass ranges (8 bytes each)
    reader.skip(header.num_segments as usize * 8)?;

    // Skip profile data entirely
    let profile_bytes = header.num_profile_words as usize * 4;
    if profile_bytes > 0 {
        reader.skip(profile_bytes)?;
    }

    // Decode centroids using batch slice reads
    if header.num_centroid_words == 0 {
        return Ok((vec![], vec![]));
    }

    let accurate = header.is_accurate_mass();
    let bytes_per_peak = header.bytes_per_centroid_peak();
    let total_centroid_bytes = header.num_centroid_words as usize * 4;
    let estimated_peaks = if bytes_per_peak > 0 {
        total_centroid_bytes / bytes_per_peak
    } else {
        0
    };

    let mut all_mz = Vec::with_capacity(estimated_peaks);
    let mut all_intensity = Vec::with_capacity(estimated_peaks);

    for _ in 0..header.num_segments {
        let count = reader.read_u32()?;
        if count > 10_000_000 {
            return Err(RawError::CorruptedData(format!(
                "FT/LT centroid: unreasonable peak count {} in segment",
                count
            )));
        }
        if count == 0 {
            continue;
        }

        let peak_bytes = count as usize * bytes_per_peak;
        let raw = reader.slice(peak_bytes)?;
        reader.skip(peak_bytes)?;

        if accurate {
            for i in 0..count as usize {
                let base = i * 12;
                let mz = f64::from_le_bytes(raw[base..base + 8].try_into().unwrap());
                let intensity =
                    f32::from_le_bytes(raw[base + 8..base + 12].try_into().unwrap()) as f64;
                all_mz.push(mz);
                all_intensity.push(intensity);
            }
        } else {
            for i in 0..count as usize {
                let base = i * 8;
                let mz = f32::from_le_bytes(raw[base..base + 4].try_into().unwrap()) as f64;
                let intensity =
                    f32::from_le_bytes(raw[base + 4..base + 8].try_into().unwrap()) as f64;
                all_mz.push(mz);
                all_intensity.push(intensity);
            }
        }
    }

    Ok((all_mz, all_intensity))
}

#[cfg(test)]
mod tests {
    use super::*;

    fn build_ftlt_header_bytes(
        num_segments: u32,
        num_profile_words: u32,
        num_centroid_words: u32,
        default_feature_word: u32,
    ) -> Vec<u8> {
        let mut buf = Vec::new();
        buf.extend_from_slice(&num_segments.to_le_bytes());
        buf.extend_from_slice(&num_profile_words.to_le_bytes());
        buf.extend_from_slice(&num_centroid_words.to_le_bytes());
        buf.extend_from_slice(&default_feature_word.to_le_bytes());
        buf.extend_from_slice(&0u32.to_le_bytes()); // non-default features
        buf.extend_from_slice(&0u32.to_le_bytes()); // expansion
        buf.extend_from_slice(&0u32.to_le_bytes()); // noise
        buf.extend_from_slice(&0u32.to_le_bytes()); // debug
        buf
    }

    #[test]
    fn test_ftlt_header_parse() {
        let data = build_ftlt_header_bytes(1, 100, 50, 0x10000);
        let mut reader = BinaryReader::new(&data);
        let header = FtLtPacketHeader::parse(&mut reader).unwrap();
        assert_eq!(header.num_segments, 1);
        assert_eq!(header.num_profile_words, 100);
        assert_eq!(header.num_centroid_words, 50);
        assert!(header.is_accurate_mass());
        assert!(!header.is_lt_mode());
    }

    #[test]
    fn test_ftlt_header_lt_mode() {
        let data = build_ftlt_header_bytes(1, 0, 10, 0x40);
        let mut reader = BinaryReader::new(&data);
        let header = FtLtPacketHeader::parse(&mut reader).unwrap();
        assert!(header.is_lt_mode());
        assert!(!header.is_accurate_mass());
        assert_eq!(header.bytes_per_centroid_peak(), 8);
    }

    #[test]
    fn test_ftlt_centroid_standard_accuracy() {
        // Build: header + 1 segment mass range + centroid data
        // 1 segment, 3 peaks at standard accuracy (f32 mass + f32 intensity)
        // Centroid data: u32 count=3, then 3 * (f32, f32) = 24 bytes = 6 words
        let mut data = build_ftlt_header_bytes(1, 0, 7, 0); // 7 words = 28 bytes (4 for count + 24 for peaks)
                                                            // Segment mass range
        data.extend_from_slice(&100.0f32.to_le_bytes());
        data.extend_from_slice(&1000.0f32.to_le_bytes());
        // Centroid data: count=3
        data.extend_from_slice(&3u32.to_le_bytes());
        // Peak 1: mz=200.5, int=1000
        data.extend_from_slice(&200.5f32.to_le_bytes());
        data.extend_from_slice(&1000.0f32.to_le_bytes());
        // Peak 2: mz=500.25, int=2000
        data.extend_from_slice(&500.25f32.to_le_bytes());
        data.extend_from_slice(&2000.0f32.to_le_bytes());
        // Peak 3: mz=800.75, int=500
        data.extend_from_slice(&800.75f32.to_le_bytes());
        data.extend_from_slice(&500.0f32.to_le_bytes());

        let result = decode_ftlt_scan(&data, 0, 20, &[]).unwrap();
        assert_eq!(result.centroid_mz.len(), 3);
        assert!((result.centroid_mz[0] - 200.5).abs() < 0.01);
        assert!((result.centroid_mz[1] - 500.25).abs() < 0.01);
        assert!((result.centroid_mz[2] - 800.75).abs() < 0.01);
        assert!((result.centroid_intensity[0] - 1000.0).abs() < 0.1);
        assert!(result.profile_mz.is_none());
    }

    #[test]
    fn test_ftlt_centroid_accurate_mass() {
        // Accurate mass: f64 mass + f32 intensity = 12 bytes per peak
        // 2 peaks = 24 bytes = 6 words, plus 4 bytes for count = 7 words
        let mut data = build_ftlt_header_bytes(1, 0, 7, 0x10000);
        // Segment mass range
        data.extend_from_slice(&100.0f32.to_le_bytes());
        data.extend_from_slice(&1000.0f32.to_le_bytes());
        // Centroid: count=2
        data.extend_from_slice(&2u32.to_le_bytes());
        // Peak 1: f64 mz=524.264837, f32 int=50000
        data.extend_from_slice(&524.264837f64.to_le_bytes());
        data.extend_from_slice(&50000.0f32.to_le_bytes());
        // Peak 2: f64 mz=612.123456, f32 int=30000
        data.extend_from_slice(&612.123456f64.to_le_bytes());
        data.extend_from_slice(&30000.0f32.to_le_bytes());

        let result = decode_ftlt_scan(&data, 0, 20, &[]).unwrap();
        assert_eq!(result.centroid_mz.len(), 2);
        assert!((result.centroid_mz[0] - 524.264837).abs() < 1e-5);
        assert!((result.centroid_mz[1] - 612.123456).abs() < 1e-5);
        assert!((result.centroid_intensity[0] - 50000.0).abs() < 0.1);
    }

    #[test]
    fn test_ftlt_empty_scan() {
        // Header with 0 segments, 0 profile, 0 centroid
        let data = build_ftlt_header_bytes(0, 0, 0, 0);
        let result = decode_ftlt_scan(&data, 0, 20, &[]).unwrap();
        assert!(result.centroid_mz.is_empty());
        assert!(result.profile_mz.is_none());
    }
}
