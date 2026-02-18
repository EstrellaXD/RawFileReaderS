use std::path::PathBuf;
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::Arc;

use thermo_raw::ProgressCounter;
use thermo_raw_mzml::MzmlConfig;

pub struct ConversionResult {
    pub index: usize,
    pub result: Result<(), String>,
}

/// Run conversion of multiple files on a background thread.
/// Returns a JoinHandle that yields per-file results.
pub fn spawn_conversion(
    files: Vec<(usize, PathBuf)>,
    output_dir: PathBuf,
    config: MzmlConfig,
    counter: ProgressCounter,
    cancel: Arc<AtomicBool>,
) -> std::thread::JoinHandle<Vec<ConversionResult>> {
    std::thread::spawn(move || {
        let mut results = Vec::with_capacity(files.len());
        for (index, raw_path) in files {
            if cancel.load(Ordering::Relaxed) {
                break;
            }

            let stem = raw_path
                .file_stem()
                .unwrap_or_default()
                .to_string_lossy()
                .into_owned();
            let out_path = output_dir.join(format!("{stem}.mzML"));

            let result = thermo_raw_mzml::convert_file_with_progress(
                &raw_path,
                &out_path,
                &config,
                &counter,
            );

            results.push(ConversionResult {
                index,
                result: result.map_err(|e| e.to_string()),
            });
        }
        results
    })
}
