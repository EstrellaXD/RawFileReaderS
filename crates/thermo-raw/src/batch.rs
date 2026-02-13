//! Batch EIC extraction across multiple RAW files.
//!
//! Produces a tensor (samples x targets x timepoints) aligned to a common RT grid.
//! Used for cross-sample analysis in proteomics/metabolomics experiments.

use crate::progress::{self, ProgressCounter};
use crate::raw_file::RawFile;
use crate::types::Chromatogram;
use crate::RawError;
use rayon::prelude::*;
use std::path::Path;

/// Result of batch EIC extraction across multiple files.
pub struct BatchXicResult {
    /// Common RT axis in minutes (n_timepoints).
    pub rt_grid: Vec<f64>,
    /// Flat data: [sample][target][timepoint] row-major.
    /// Length = n_samples * n_targets * n_timepoints.
    pub data: Vec<f64>,
    /// File stem or identifier for each sample.
    pub sample_names: Vec<String>,
    pub n_samples: usize,
    pub n_targets: usize,
    pub n_timepoints: usize,
}

impl BatchXicResult {
    /// Get the intensity slice for a specific (sample, target) pair.
    pub fn get(&self, sample: usize, target: usize) -> &[f64] {
        let start = (sample * self.n_targets + target) * self.n_timepoints;
        &self.data[start..start + self.n_timepoints]
    }
}

/// Extract batch EICs from multiple RAW files onto a common RT grid.
///
/// Opens all files in parallel, extracts per-target chromatograms, then
/// interpolates onto a shared time axis (intersection of all RT ranges).
///
/// # Arguments
/// * `paths` - RAW file paths
/// * `targets` - (mz, ppm) pairs for each extraction target
/// * `rt_range` - Optional (start, end) in minutes to restrict the RT window
/// * `rt_resolution` - Grid spacing in minutes (default: 0.01 = 0.6s)
pub fn batch_xic_ms1(
    paths: &[&Path],
    targets: &[(f64, f64)],
    rt_range: Option<(f64, f64)>,
    rt_resolution: f64,
) -> Result<BatchXicResult, RawError> {
    if paths.is_empty() {
        return Err(RawError::CorruptedData("No files provided".to_string()));
    }
    if targets.is_empty() {
        return Err(RawError::CorruptedData("No targets provided".to_string()));
    }

    type FileResult = Result<(String, Vec<Chromatogram>), (String, RawError)>;

    // Open all files in parallel and extract chromatograms.
    // Files that fail to open (e.g. blank acquisitions) are skipped with a warning.
    let results: Vec<FileResult> = paths
        .par_iter()
        .map(|path| {
            let name = path
                .file_stem()
                .map(|s| s.to_string_lossy().to_string())
                .unwrap_or_else(|| "unknown".to_string());
            match RawFile::open_mmap(path) {
                Ok(raw) => match raw.xic_batch_ms1(targets) {
                    Ok(chroms) => Ok((name, chroms)),
                    Err(e) => Err((name, e)),
                },
                Err(e) => Err((name, e)),
            }
        })
        .collect();

    let mut per_file = Vec::new();
    let mut skipped = Vec::new();
    for r in results {
        match r {
            Ok(item) => per_file.push(item),
            Err((name, err)) => skipped.push((name, err)),
        }
    }

    // Report skipped files to stderr
    for (name, err) in &skipped {
        eprintln!("Warning: skipping '{}': {}", name, err);
    }

    if per_file.is_empty() {
        return Err(RawError::CorruptedData(
            "All files failed to open".to_string(),
        ));
    }

    let n_samples = per_file.len();
    let n_targets = targets.len();

    // Compute the common RT grid: intersection of all files' RT ranges
    let (grid_start, grid_end) = compute_rt_bounds(&per_file, rt_range);

    if grid_start >= grid_end {
        return Err(RawError::CorruptedData(
            "No overlapping RT range across files".to_string(),
        ));
    }

    let rt_grid = build_rt_grid(grid_start, grid_end, rt_resolution);
    let n_timepoints = rt_grid.len();

    // Interpolate all chromatograms onto the common grid
    let mut data = vec![0.0f64; n_samples * n_targets * n_timepoints];

    for (s, (_name, chroms)) in per_file.iter().enumerate() {
        let chroms: &Vec<Chromatogram> = chroms;
        for (t, chrom) in chroms.iter().enumerate() {
            let offset = (s * n_targets + t) * n_timepoints;
            interpolate_onto_grid(
                &chrom.rt,
                &chrom.intensity,
                &rt_grid,
                &mut data[offset..offset + n_timepoints],
            );
        }
    }

    let sample_names = per_file.into_iter().map(|(name, _)| name).collect();

    Ok(BatchXicResult {
        rt_grid,
        data,
        sample_names,
        n_samples,
        n_targets,
        n_timepoints,
    })
}

/// Like [`batch_xic_ms1`], but increments the progress counter after each file.
pub fn batch_xic_ms1_with_progress(
    paths: &[&Path],
    targets: &[(f64, f64)],
    rt_range: Option<(f64, f64)>,
    rt_resolution: f64,
    counter: &ProgressCounter,
) -> Result<BatchXicResult, RawError> {
    if paths.is_empty() {
        return Err(RawError::CorruptedData("No files provided".to_string()));
    }
    if targets.is_empty() {
        return Err(RawError::CorruptedData("No targets provided".to_string()));
    }

    type FileResult = Result<(String, Vec<Chromatogram>), (String, RawError)>;

    let results: Vec<FileResult> = paths
        .par_iter()
        .map(|path| {
            let name = path
                .file_stem()
                .map(|s| s.to_string_lossy().to_string())
                .unwrap_or_else(|| "unknown".to_string());
            let result = match RawFile::open_mmap(path) {
                Ok(raw) => match raw.xic_batch_ms1(targets) {
                    Ok(chroms) => Ok((name, chroms)),
                    Err(e) => Err((name, e)),
                },
                Err(e) => Err((name, e)),
            };
            progress::tick(counter);
            result
        })
        .collect();

    let mut per_file = Vec::new();
    let mut skipped = Vec::new();
    for r in results {
        match r {
            Ok(item) => per_file.push(item),
            Err((name, err)) => skipped.push((name, err)),
        }
    }

    for (name, err) in &skipped {
        eprintln!("Warning: skipping '{}': {}", name, err);
    }

    if per_file.is_empty() {
        return Err(RawError::CorruptedData(
            "All files failed to open".to_string(),
        ));
    }

    let n_samples = per_file.len();
    let n_targets = targets.len();

    let (grid_start, grid_end) = compute_rt_bounds(&per_file, rt_range);

    if grid_start >= grid_end {
        return Err(RawError::CorruptedData(
            "No overlapping RT range across files".to_string(),
        ));
    }

    let rt_grid = build_rt_grid(grid_start, grid_end, rt_resolution);
    let n_timepoints = rt_grid.len();

    let mut data = vec![0.0f64; n_samples * n_targets * n_timepoints];

    for (s, (_name, chroms)) in per_file.iter().enumerate() {
        let chroms: &Vec<Chromatogram> = chroms;
        for (t, chrom) in chroms.iter().enumerate() {
            let offset = (s * n_targets + t) * n_timepoints;
            interpolate_onto_grid(
                &chrom.rt,
                &chrom.intensity,
                &rt_grid,
                &mut data[offset..offset + n_timepoints],
            );
        }
    }

    let sample_names = per_file.into_iter().map(|(name, _)| name).collect();

    Ok(BatchXicResult {
        rt_grid,
        data,
        sample_names,
        n_samples,
        n_targets,
        n_timepoints,
    })
}

/// Compute the intersection of all RT ranges (max of starts, min of ends).
fn compute_rt_bounds(
    per_file: &[(String, Vec<Chromatogram>)],
    rt_range: Option<(f64, f64)>,
) -> (f64, f64) {
    let mut global_start = f64::NEG_INFINITY;
    let mut global_end = f64::INFINITY;

    for (_name, chroms) in per_file {
        for chrom in chroms {
            if let (Some(&first), Some(&last)) = (chrom.rt.first(), chrom.rt.last()) {
                if first > global_start {
                    global_start = first;
                }
                if last < global_end {
                    global_end = last;
                }
            }
        }
    }

    // Apply user-specified RT range constraint
    if let Some((user_start, user_end)) = rt_range {
        if user_start > global_start {
            global_start = user_start;
        }
        if user_end < global_end {
            global_end = user_end;
        }
    }

    (global_start, global_end)
}

/// Build a uniform RT grid from start to end with the given spacing.
fn build_rt_grid(start: f64, end: f64, resolution: f64) -> Vec<f64> {
    let n = ((end - start) / resolution).ceil() as usize + 1;
    (0..n).map(|i| start + i as f64 * resolution).collect()
}

/// Linearly interpolate a chromatogram onto a target RT grid.
///
/// TODO(human): Implement linear interpolation from source (rt_src, int_src)
/// onto grid points (grid), writing results into out[..grid.len()].
/// Points outside the source range should be 0.0.
/// Linearly interpolate source chromatogram onto target grid points.
///
/// Uses a sliding index to avoid O(n*m) binary searches — since both
/// the source RTs and grid are sorted, we walk forward through both.
/// Points outside the source RT range get 0.0 (no extrapolation).
fn interpolate_onto_grid(rt_src: &[f64], int_src: &[f64], grid: &[f64], out: &mut [f64]) {
    if rt_src.is_empty() || int_src.is_empty() {
        for v in out.iter_mut() {
            *v = 0.0;
        }
        return;
    }

    let n_src = rt_src.len();
    let mut j = 0usize; // sliding index into source

    for (i, &t) in grid.iter().enumerate() {
        // Outside source range: zero
        if t < rt_src[0] || t > rt_src[n_src - 1] {
            out[i] = 0.0;
            continue;
        }

        // Advance j so that rt_src[j] <= t < rt_src[j+1] (or j is at the last point)
        while j + 1 < n_src && rt_src[j + 1] < t {
            j += 1;
        }

        if j + 1 >= n_src {
            // At or past the last source point
            out[i] = int_src[n_src - 1];
        } else {
            let dt = rt_src[j + 1] - rt_src[j];
            if dt <= 0.0 {
                out[i] = int_src[j];
            } else {
                let frac = (t - rt_src[j]) / dt;
                out[i] = int_src[j] + frac * (int_src[j + 1] - int_src[j]);
            }
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_build_rt_grid() {
        let grid = build_rt_grid(1.0, 2.0, 0.5);
        assert_eq!(grid.len(), 3);
        assert!((grid[0] - 1.0).abs() < 1e-10);
        assert!((grid[1] - 1.5).abs() < 1e-10);
        assert!((grid[2] - 2.0).abs() < 1e-10);
    }

    #[test]
    fn test_interpolate_linear() {
        let rt = vec![0.0, 1.0, 2.0, 3.0];
        let intensity = vec![0.0, 10.0, 20.0, 30.0];
        let grid = vec![0.5, 1.5, 2.5];
        let mut out = vec![0.0; 3];
        interpolate_onto_grid(&rt, &intensity, &grid, &mut out);
        assert!((out[0] - 5.0).abs() < 1e-10);
        assert!((out[1] - 15.0).abs() < 1e-10);
        assert!((out[2] - 25.0).abs() < 1e-10);
    }

    #[test]
    fn test_interpolate_outside_range() {
        let rt = vec![1.0, 2.0, 3.0];
        let intensity = vec![10.0, 20.0, 30.0];
        let grid = vec![0.0, 1.5, 4.0];
        let mut out = vec![0.0; 3];
        interpolate_onto_grid(&rt, &intensity, &grid, &mut out);
        assert_eq!(out[0], 0.0); // before range
        assert!((out[1] - 15.0).abs() < 1e-10); // interpolated
        assert_eq!(out[2], 0.0); // after range
    }

    #[test]
    fn test_interpolate_exact_points() {
        let rt = vec![1.0, 2.0, 3.0];
        let intensity = vec![100.0, 200.0, 300.0];
        let grid = vec![1.0, 2.0, 3.0];
        let mut out = vec![0.0; 3];
        interpolate_onto_grid(&rt, &intensity, &grid, &mut out);
        assert!((out[0] - 100.0).abs() < 1e-10);
        assert!((out[1] - 200.0).abs() < 1e-10);
        assert!((out[2] - 300.0).abs() < 1e-10);
    }

    #[test]
    fn test_interpolate_empty_source() {
        let grid = vec![0.5, 1.5];
        let mut out = vec![99.0; 2];
        interpolate_onto_grid(&[], &[], &grid, &mut out);
        assert_eq!(out, vec![0.0, 0.0]);
    }

    #[test]
    fn test_batch_xic_result_get() {
        let result = BatchXicResult {
            rt_grid: vec![1.0, 2.0, 3.0],
            data: vec![
                // sample 0, target 0
                1.0, 2.0, 3.0, // sample 0, target 1
                4.0, 5.0, 6.0, // sample 1, target 0
                7.0, 8.0, 9.0, // sample 1, target 1
                10.0, 11.0, 12.0,
            ],
            sample_names: vec!["s0".to_string(), "s1".to_string()],
            n_samples: 2,
            n_targets: 2,
            n_timepoints: 3,
        };
        assert_eq!(result.get(0, 0), &[1.0, 2.0, 3.0]);
        assert_eq!(result.get(0, 1), &[4.0, 5.0, 6.0]);
        assert_eq!(result.get(1, 0), &[7.0, 8.0, 9.0]);
        assert_eq!(result.get(1, 1), &[10.0, 11.0, 12.0]);
    }
}
