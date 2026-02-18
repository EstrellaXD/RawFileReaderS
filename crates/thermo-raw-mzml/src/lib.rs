//! Convert Thermo RAW files to indexed mzML format.
//!
//! Provides streaming conversion from Thermo RAW binary format to PSI-MS
//! standard mzML/indexed mzML. Uses `thermo-raw` for parsing and `quick-xml`
//! for efficient XML generation.
//!
//! # Example
//!
//! ```no_run
//! use thermo_raw_mzml::{convert_file, MzmlConfig};
//! use std::path::Path;
//!
//! let config = MzmlConfig::default();
//! convert_file(Path::new("sample.RAW"), Path::new("sample.mzML"), &config).unwrap();
//! ```

mod binary;
mod cv;
mod writer;

use std::path::{Path, PathBuf};
use thiserror::Error;

#[derive(Error, Debug)]
pub enum MzmlError {
    #[error("RAW file error: {0}")]
    Raw(#[from] thermo_raw::RawError),
    #[error("I/O error: {0}")]
    Io(#[from] std::io::Error),
    #[error("XML write error: {0}")]
    Xml(#[from] quick_xml::Error),
    #[error("Conversion error: {0}")]
    Conversion(String),
}

/// Numeric precision for binary data arrays.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Precision {
    F32,
    F64,
}

/// Compression mode for binary data arrays.
#[derive(Debug, Clone, Copy, PartialEq, Eq)]
pub enum Compression {
    None,
    Zlib,
}

/// Configuration for mzML conversion.
#[derive(Debug, Clone)]
pub struct MzmlConfig {
    /// Precision for m/z arrays (default: F64).
    pub mz_precision: Precision,
    /// Precision for intensity arrays (default: F32).
    pub intensity_precision: Precision,
    /// Compression for binary arrays (default: Zlib).
    pub compression: Compression,
    /// Write indexed mzML with offset index (default: true).
    pub write_index: bool,
    /// Include MS2+ scans (default: true).
    pub include_ms2: bool,
    /// Minimum intensity threshold; peaks at or below this value are excluded (default: 0.0 = keep all).
    pub intensity_threshold: f64,
}

impl Default for MzmlConfig {
    fn default() -> Self {
        Self {
            mz_precision: Precision::F64,
            intensity_precision: Precision::F32,
            compression: Compression::Zlib,
            write_index: true,
            include_ms2: true,
            intensity_threshold: 0.0,
        }
    }
}

/// Convert a single RAW file to mzML.
pub fn convert_file(
    raw_path: &Path,
    output_path: &Path,
    config: &MzmlConfig,
) -> Result<(), MzmlError> {
    let raw = thermo_raw::RawFile::open_mmap(raw_path)?;
    let output = std::io::BufWriter::new(std::fs::File::create(output_path)?);
    let source_name = raw_path
        .file_name()
        .map(|n| n.to_string_lossy().into_owned())
        .unwrap_or_default();
    writer::write_mzml(&raw, output, config, &source_name)?;
    Ok(())
}

/// Convert a single RAW file to mzML, ticking the counter after each scan.
pub fn convert_file_with_progress(
    raw_path: &Path,
    output_path: &Path,
    config: &MzmlConfig,
    counter: &thermo_raw::ProgressCounter,
) -> Result<(), MzmlError> {
    let raw = thermo_raw::RawFile::open_mmap(raw_path)?;
    let output = std::io::BufWriter::new(std::fs::File::create(output_path)?);
    let source_name = raw_path
        .file_name()
        .map(|n| n.to_string_lossy().into_owned())
        .unwrap_or_default();
    writer::write_mzml_with_progress(&raw, output, config, &source_name, counter)?;
    Ok(())
}

/// Convert all RAW files in a folder to mzML (parallel).
pub fn convert_folder(
    input_dir: &Path,
    output_dir: &Path,
    config: &MzmlConfig,
) -> Result<Vec<PathBuf>, MzmlError> {
    use rayon::prelude::*;

    std::fs::create_dir_all(output_dir)?;

    let entries: Vec<_> = std::fs::read_dir(input_dir)?
        .filter_map(|e| e.ok())
        .filter(|e| {
            e.path()
                .extension()
                .is_some_and(|ext| ext.eq_ignore_ascii_case("raw"))
        })
        .collect();

    let results: Vec<Result<PathBuf, MzmlError>> = entries
        .par_iter()
        .map(|entry| {
            let raw_path = entry.path();
            let stem = raw_path.file_stem().unwrap_or_default();
            let out_path = output_dir.join(format!("{}.mzML", stem.to_string_lossy()));
            convert_file(&raw_path, &out_path, config)?;
            Ok(out_path)
        })
        .collect();

    results.into_iter().collect()
}

/// Convert all RAW files in a folder to mzML (parallel), ticking per file.
pub fn convert_folder_with_progress(
    input_dir: &Path,
    output_dir: &Path,
    config: &MzmlConfig,
    counter: &thermo_raw::ProgressCounter,
) -> Result<Vec<PathBuf>, MzmlError> {
    use rayon::prelude::*;

    std::fs::create_dir_all(output_dir)?;

    let entries: Vec<_> = std::fs::read_dir(input_dir)?
        .filter_map(|e| e.ok())
        .filter(|e| {
            e.path()
                .extension()
                .is_some_and(|ext| ext.eq_ignore_ascii_case("raw"))
        })
        .collect();

    let results: Vec<Result<PathBuf, MzmlError>> = entries
        .par_iter()
        .map(|entry| {
            let raw_path = entry.path();
            let stem = raw_path.file_stem().unwrap_or_default();
            let out_path = output_dir.join(format!("{}.mzML", stem.to_string_lossy()));
            convert_file(&raw_path, &out_path, config)?;
            thermo_raw::progress::tick(counter);
            Ok(out_path)
        })
        .collect();

    results.into_iter().collect()
}
