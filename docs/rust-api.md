# Rust API Reference

## Crate: `thermo-raw`

```toml
[dependencies]
thermo-raw = { path = "crates/thermo-raw" }
```

---

## `RawFile`

Primary entry point for reading Thermo RAW files.

### Constructors

```rust
pub fn open(path: impl AsRef<Path>) -> Result<Self, RawError>
```

Opens a RAW file by reading it entirely into memory. Parses header, scan index, and trailer layout eagerly. Scan data is decoded lazily on demand.

```rust
pub fn open_mmap(path: impl AsRef<Path>) -> Result<Self, RawError>
```

Opens a RAW file using memory-mapped I/O. More memory-efficient for large files. The file must not be modified while the `RawFile` is open.

```rust
use thermo_raw::RawFile;

let raw = RawFile::open("sample.RAW")?;
let raw = RawFile::open_mmap("sample.RAW")?;  // ~2x faster for large files
```

### Metadata

```rust
pub fn version(&self) -> u32
pub fn metadata(&self) -> &FileMetadata
pub fn n_scans(&self) -> u32
pub fn first_scan(&self) -> u32
pub fn last_scan(&self) -> u32
pub fn start_time(&self) -> f64       // minutes
pub fn end_time(&self) -> f64         // minutes
pub fn low_mass(&self) -> f64
pub fn high_mass(&self) -> f64
pub fn file_size(&self) -> usize
```

```rust
let meta = raw.metadata();
println!("{} — {} scans, RT {:.2}–{:.2} min",
    meta.instrument_model, raw.n_scans(), raw.start_time(), raw.end_time());
```

### Scan Reading

```rust
pub fn scan(&self, scan_number: u32) -> Result<Scan, RawError>
```

Decodes a single scan by number. Returns centroid/profile data, MS level, polarity, precursor info, and filter string.

```rust
pub fn scans_parallel(&self, range: Range<u32>) -> Result<Vec<Scan>, RawError>
```

Decodes multiple scans in parallel via rayon. Range is scan numbers (e.g., `1..101`).

```rust
pub fn is_ms1_scan(&self, scan_idx: u32) -> bool
```

Fast MS1 check using trailer metadata only (no scan data decoding). Returns `true` if the scan's "Master Scan Number" field is 0, or if the field cannot be read.

```rust
let scan = raw.scan(1)?;
println!("RT={:.2} MS{:?} {} peaks",
    scan.rt, scan.ms_level, scan.centroid_mz.len());

let scans = raw.scans_parallel(1..1001)?;
let ms1_count = scans.iter().filter(|s| s.ms_level == MsLevel::Ms1).count();
```

### Chromatograms

```rust
pub fn tic(&self) -> Chromatogram
```

TIC from scan index. Sub-millisecond, no scan decoding.

```rust
pub fn bpc(&self) -> Chromatogram
```

Base peak chromatogram from scan index.

```rust
pub fn xic(&self, target_mz: f64, tolerance_ppm: f64) -> Result<Chromatogram, RawError>
```

Extracted ion chromatogram across all scans (MS1 + MS2).

```rust
pub fn xic_ms1(&self, target_mz: f64, tolerance_ppm: f64) -> Result<Chromatogram, RawError>
```

XIC restricted to MS1 scans. Skips MS2+ scans without decoding them.

```rust
pub fn xic_batch_ms1(&self, targets: &[(f64, f64)]) -> Result<Vec<Chromatogram>, RawError>
```

Batch XIC for multiple `(mz, ppm)` targets in a single pass over MS1 scans.

```rust
let tic = raw.tic();
let xic = raw.xic_ms1(760.5851, 5.0)?;

let targets = &[(760.5851, 5.0), (788.6164, 5.0)];
let chroms = raw.xic_batch_ms1(targets)?;
```

### Trailer Access

```rust
pub fn trailer_extra(&self, scan_number: u32) -> Result<HashMap<String, String>, RawError>
pub fn trailer_fields(&self) -> Vec<String>
```

```rust
let fields = raw.trailer_fields();
let trailer = raw.trailer_extra(1)?;
if let Some(charge) = trailer.get("Charge State:") {
    println!("Charge: {charge}");
}
```

### Index & Event Access

```rust
pub fn scan_index(&self) -> &[ScanIndexEntry]
pub fn scan_events(&self) -> &[ScanEvent]
pub fn run_header(&self) -> &RunHeader
```

Low-level access to parsed internal structures.

### Diagnostics

```rust
pub fn debug_info(&self) -> DebugInfo
pub fn list_streams(path: impl AsRef<Path>) -> Result<Vec<String>, RawError>
```

```rust
pub fn diagnose(data: &[u8]) -> DiagnosticReport
```

Stage-by-stage diagnostics without cascading failures. Tests: magic detection, FileHeader, RawFileInfo, RunHeader, ScanIndex, TrailerLayout, and first scan decode.

```rust
let data = std::fs::read("sample.RAW")?;
let report = thermo_raw::diagnose(&data);
report.print();
```

---

## Core Types

### `Scan`

Fully decoded scan with all data.

```rust
pub struct Scan {
    pub scan_number: u32,
    pub rt: f64,                              // minutes
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
```

Derives: `Debug, Clone, Serialize, Deserialize`

### `Chromatogram`

Paired retention time and intensity vectors.

```rust
pub struct Chromatogram {
    pub rt: Vec<f64>,          // minutes
    pub intensity: Vec<f64>,
}
```

Derives: `Debug, Clone, Serialize, Deserialize`

### `FileMetadata`

File-level metadata from the acquisition.

```rust
pub struct FileMetadata {
    pub creation_date: String,
    pub instrument_model: String,
    pub instrument_name: String,
    pub serial_number: String,
    pub software_version: String,
    pub sample_name: String,
    pub comment: String,
}
```

Derives: `Debug, Clone, Serialize, Deserialize`

### `PrecursorInfo`

Precursor ion information for MS2+ scans.

```rust
pub struct PrecursorInfo {
    pub mz: f64,
    pub charge: Option<i32>,
    pub isolation_width: Option<f64>,
    pub activation_type: Option<String>,
    pub collision_energy: Option<f64>,
}
```

Derives: `Debug, Clone, Serialize, Deserialize`

---

## Enums

### `MsLevel`

```rust
pub enum MsLevel {
    Ms1,
    Ms2,
    Ms3,
    Other(u8),
}
```

### `Polarity`

```rust
pub enum Polarity {
    Positive,
    Negative,
    Unknown,
}
```

### `ScanMode`

```rust
pub enum ScanMode {
    Centroid,
    Profile,
    Unknown,
}
```

### `ScanType`

```rust
pub enum ScanType {
    Full, Zoom, Sim, Srm, Crm, Q1Ms, Q3Ms, Unknown(u8),
}
```

### `AnalyzerType`

```rust
pub enum AnalyzerType {
    Itms,    // Ion Trap
    Tqms,    // Triple Quad
    Sqms,    // Single Quad
    Tofms,   // Time of Flight
    Ftms,    // Orbitrap / FT
    Sector,
    Any,
    Astms,   // Advanced Segmented Trap
    Unknown(u8),
}
```

### `IonizationType`

```rust
pub enum IonizationType {
    Ei, Ci, Fab, Esi, Apci, Nsi, Tsi, Fdi, Maldi, Gd, Any, Psi, Cnsi, Unknown(u8),
}
```

### `ActivationType`

```rust
pub enum ActivationType {
    Cid, Mpd, Ecd, Pqd, Etd, Hcd, Any, Sa, Ptr, Netd, Nptr, Uvpd, Eid, Unknown(u8),
}
```

All enums derive: `Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize`

---

## Scan Event Types

### `ScanEvent`

Complete scan event with preamble, reactions, and conversion parameters.

```rust
pub struct ScanEvent {
    pub preamble: ScanEventPreamble,
    pub reactions: Vec<Reaction>,
    pub conversion_params: Vec<f64>,
}
```

### `ScanEventPreamble`

```rust
pub struct ScanEventPreamble {
    pub polarity: Polarity,
    pub scan_mode: ScanMode,
    pub ms_level: MsLevel,
    pub scan_type: ScanType,
    pub dependent: bool,
    pub ionization: IonizationType,
    pub activation: ActivationType,
    pub analyzer: AnalyzerType,
}
```

### `Reaction`

Precursor fragmentation parameters.

```rust
pub struct Reaction {
    pub precursor_mz: f64,
    pub isolation_width: f64,
    pub collision_energy: f64,
    pub collision_energy_valid: u32,
    pub precursor_range_valid: bool,
    pub first_precursor_mass: f64,
    pub last_precursor_mass: f64,
    pub isolation_width_offset: f64,
}

impl Reaction {
    pub fn activation_type(&self) -> ActivationType
}
```

---

## Scan Index

### `ScanIndexEntry`

Per-scan index entry with offsets, RT, TIC, and mass range.

```rust
pub struct ScanIndexEntry {
    pub offset: u64,
    pub trailer_offset: i32,
    pub scan_event: u16,
    pub scan_segment: u16,
    pub scan_number: i32,
    pub packet_type: u32,
    pub number_packets: i32,
    pub data_size: u32,           // v65+ only
    pub rt: f64,
    pub tic: f64,
    pub base_peak_intensity: f64,
    pub base_peak_mz: f64,
    pub low_mz: f64,
    pub high_mz: f64,
    pub cycle_number: i32,        // v65+ only
}

impl ScanIndexEntry {
    pub fn index(&self) -> u32    // scan_number as u32
}
```

---

## Batch Processing

### `batch_xic_ms1()`

Multi-file batch XIC with RT alignment.

```rust
pub fn batch_xic_ms1(
    paths: &[&Path],
    targets: &[(f64, f64)],
    rt_range: Option<(f64, f64)>,
    rt_resolution: f64,
) -> Result<BatchXicResult, RawError>
```

| Parameter | Description |
|-----------|-------------|
| `paths` | RAW file paths |
| `targets` | `(mz, ppm)` pairs |
| `rt_range` | Optional `(start, end)` in minutes |
| `rt_resolution` | Grid spacing in minutes |

Opens all files in parallel, extracts per-target chromatograms, and interpolates onto a shared RT grid. Files that fail to open are skipped.

### `BatchXicResult`

```rust
pub struct BatchXicResult {
    pub rt_grid: Vec<f64>,          // common RT axis (n_timepoints)
    pub data: Vec<f64>,             // flat row-major [sample][target][timepoint]
    pub sample_names: Vec<String>,
    pub n_samples: usize,
    pub n_targets: usize,
    pub n_timepoints: usize,
}

impl BatchXicResult {
    pub fn get(&self, sample: usize, target: usize) -> &[f64]
}
```

```rust
use std::path::Path;
use thermo_raw::batch_xic_ms1;

let paths: Vec<&Path> = vec![Path::new("a.RAW"), Path::new("b.RAW")];
let targets = &[(760.5851, 5.0), (788.6164, 5.0)];

let result = batch_xic_ms1(&paths, targets, Some((5.0, 25.0)), 0.01)?;
let trace = result.get(0, 1);  // sample 0, target 1
```

---

## Error Handling

### `RawError`

```rust
pub enum RawError {
    Io(std::io::Error),
    NotRawFile,
    UnsupportedVersion(u32),
    StreamNotFound(String),
    ScanOutOfRange(u32),
    ScanDecodeError { offset: usize, reason: String },
    CorruptedData(String),
    CfbError(String),
}
```

Implements `std::error::Error` via `thiserror`. All variants have human-readable `Display` messages.

---

## Version Support

Supports RAW format versions 57-66.

```rust
pub const MIN_SUPPORTED_VERSION: u32 = 57;
pub const MAX_SUPPORTED_VERSION: u32 = 66;
pub const FINNIGAN_MAGIC: u16 = 0xA101;

pub fn is_supported(version: u32) -> bool
pub fn scan_index_entry_size(version: u32) -> usize    // 72, 80, or 88 bytes
pub fn uses_64bit_addresses(version: u32) -> bool       // true for v64+
pub fn scan_event_preamble_size(version: u32) -> usize
pub fn reaction_size(version: u32) -> usize             // 24, 32, 48, or 56 bytes
```

---

## Diagnostics

### `DiagnosticReport`

```rust
pub struct DiagnosticReport {
    pub file_size: u64,
    pub stages: Vec<DiagnosticStage>,
}

impl DiagnosticReport {
    pub fn print(&self)   // prints to stdout
}
```

### `DiagnosticStage`

```rust
pub struct DiagnosticStage {
    pub name: String,
    pub success: bool,
    pub detail: String,
}
```

### `DebugInfo`

```rust
pub struct DebugInfo {
    pub file_size: u64,
    pub version: u32,
    pub run_header_start: u64,
    pub scan_index_addr_32: u32,
    pub data_addr_32: u32,
    pub scan_trailer_addr_32: u32,
    pub scan_params_addr_32: u32,
    pub scan_index_addr_64: Option<u64>,
    pub data_addr_64: Option<u64>,
    pub scan_trailer_addr_64: Option<u64>,
    pub scan_params_addr_64: Option<u64>,
    pub effective_data_addr: u64,
    pub first_scan_entries: Vec<ScanIndexEntry>,
    pub n_scans: u32,
    pub n_scan_events: u32,
    pub instrument_type: i32,
}
```
