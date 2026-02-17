//! Centroid mode scan data decoding.
//!
//! Peak list format:
//! - count (u32): number of peaks
//! - For each peak: mz (f32), intensity (f32) -- interleaved pairs

use crate::io_utils::BinaryReader;
use crate::RawError;

/// Decode centroid data from a scan data packet.
///
/// Returns (mz_array, intensity_array).
/// Uses batch slice read for peak data to minimize per-element overhead.
pub fn decode_centroid(data: &[u8], offset: usize) -> Result<(Vec<f64>, Vec<f64>), RawError> {
    let mut reader = BinaryReader::at_offset(data, offset as u64);

    let count = reader.read_u32()?;

    // Sanity check
    if count > 10_000_000 {
        return Err(RawError::ScanDecodeError {
            offset,
            reason: format!("centroid data has unreasonable peak count: {}", count),
        });
    }

    if count == 0 {
        return Ok((vec![], vec![]));
    }

    let mut mz_values = Vec::with_capacity(count as usize);
    let mut intensities = Vec::with_capacity(count as usize);

    // Batch read: get all peak data as a single slice (8 bytes per peak: f32 mz + f32 int)
    let total_bytes = count as usize * 8;
    let raw_slice = reader.slice(total_bytes)?;

    for i in 0..count as usize {
        let base = i * 8;
        let mz_bytes = [
            raw_slice[base],
            raw_slice[base + 1],
            raw_slice[base + 2],
            raw_slice[base + 3],
        ];
        let int_bytes = [
            raw_slice[base + 4],
            raw_slice[base + 5],
            raw_slice[base + 6],
            raw_slice[base + 7],
        ];
        mz_values.push(f32::from_le_bytes(mz_bytes) as f64);
        intensities.push(f32::from_le_bytes(int_bytes) as f64);
    }

    Ok((mz_values, intensities))
}

/// Sum centroid intensities within [mz_low, mz_high] from legacy packet centroid bytes.
///
/// Zero allocations: reads raw bytes in-place, accumulates a running sum.
/// Legacy centroids use f32 mz + f32 intensity (8 bytes/peak) and are sorted by m/z.
pub fn sum_centroids_in_range(
    data: &[u8],
    offset: usize,
    mz_low: f64,
    mz_high: f64,
) -> Result<f64, RawError> {
    let mut reader = BinaryReader::at_offset(data, offset as u64);
    let count = reader.read_u32()?;

    if count > 10_000_000 {
        return Err(RawError::ScanDecodeError {
            offset,
            reason: format!("centroid data has unreasonable peak count: {}", count),
        });
    }

    if count == 0 {
        return Ok(0.0);
    }

    let total_bytes = count as usize * 8;
    let raw_slice = reader.slice(total_bytes)?;
    let mut sum = 0.0f64;

    for i in 0..count as usize {
        let base = i * 8;
        let mz = f32::from_le_bytes(raw_slice[base..base + 4].try_into().unwrap()) as f64;
        if mz > mz_high {
            break;
        }
        if mz >= mz_low {
            let intensity = f32::from_le_bytes(raw_slice[base + 4..base + 8].try_into().unwrap());
            sum += intensity as f64;
        }
    }

    Ok(sum)
}

/// Sum centroid intensities for multiple m/z ranges in a single pass over legacy centroid data.
///
/// `ranges` must be sorted by low bound. `out` must have length >= ranges.len().
/// Zero allocations beyond the caller-provided output slice.
pub fn sum_centroids_multi_target(
    data: &[u8],
    offset: usize,
    ranges: &[(f64, f64)],
    out: &mut [f64],
) -> Result<(), RawError> {
    let mut reader = BinaryReader::at_offset(data, offset as u64);
    let count = reader.read_u32()?;

    // Initialize output to zero
    for v in out.iter_mut().take(ranges.len()) {
        *v = 0.0;
    }

    if count > 10_000_000 {
        return Err(RawError::ScanDecodeError {
            offset,
            reason: format!("centroid data has unreasonable peak count: {}", count),
        });
    }

    if count == 0 {
        return Ok(());
    }

    let total_bytes = count as usize * 8;
    let raw_slice = reader.slice(total_bytes)?;
    let n_ranges = ranges.len();
    let mut range_start = 0usize;

    for i in 0..count as usize {
        let base = i * 8;
        let mz = f32::from_le_bytes(raw_slice[base..base + 4].try_into().unwrap()) as f64;
        let intensity =
            f32::from_le_bytes(raw_slice[base + 4..base + 8].try_into().unwrap()) as f64;

        // Advance range_start past ranges whose high < mz
        while range_start < n_ranges && ranges[range_start].1 < mz {
            range_start += 1;
        }

        if range_start >= n_ranges {
            break;
        }

        for r in range_start..n_ranges {
            let (low, high) = ranges[r];
            if low > mz {
                break;
            }
            if mz <= high {
                out[r] += intensity;
            }
        }
    }

    Ok(())
}
