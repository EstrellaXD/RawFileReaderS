use serde::{Deserialize, Serialize};

/// Mass spectrometry polarity.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum Polarity {
    Positive,
    Negative,
    Unknown,
}

/// MS scan level.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum MsLevel {
    Ms1,
    Ms2,
    Ms3,
    Other(u8),
}

/// A single scan with all associated data.
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Scan {
    pub scan_number: u32,
    /// Retention time in minutes.
    pub rt: f64,
    pub ms_level: MsLevel,
    pub polarity: Polarity,
    pub tic: f64,
    pub base_peak_mz: f64,
    pub base_peak_intensity: f64,
    pub centroid_mz: Vec<f64>,
    pub centroid_intensity: Vec<f64>,
    pub profile_mz: Option<Vec<f64>>,
    pub profile_intensity: Option<Vec<f64>>,
    pub precursor: Option<PrecursorInfo>,
    pub filter_string: Option<String>,
}

/// MS2+ precursor ion information.
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PrecursorInfo {
    pub mz: f64,
    pub charge: Option<i32>,
    pub isolation_width: Option<f64>,
    pub activation_type: Option<String>,
    pub collision_energy: Option<f64>,
}

/// A chromatogram (TIC, BPC, XIC, etc.).
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Chromatogram {
    /// Retention times in minutes.
    pub rt: Vec<f64>,
    pub intensity: Vec<f64>,
}

/// File-level metadata.
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct FileMetadata {
    pub creation_date: String,
    pub instrument_model: String,
    pub instrument_name: String,
    pub serial_number: String,
    pub software_version: String,
    pub sample_name: String,
    pub comment: String,
}

/// Acquisition type classification based on MS2 scan event properties.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum AcquisitionType {
    Ms1Only,
    Dda,
    Dia,
    Mixed,
}

/// A unique DIA isolation window derived from MS2 scan events.
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
pub struct IsolationWindow {
    pub center_mz: f64,
    pub isolation_width: f64,
    pub low_mz: f64,
    pub high_mz: f64,
    pub collision_energy: f64,
    pub activation: String,
}

/// Lightweight MS2 scan metadata derived from ScanIndex + ScanEvent (no scan data decode).
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Ms2ScanInfo {
    pub scan_number: u32,
    pub rt: f64,
    pub precursor_mz: f64,
    pub isolation_width: f64,
    pub collision_energy: f64,
    pub activation: String,
    pub scan_event_index: u16,
    pub tic: f64,
}
