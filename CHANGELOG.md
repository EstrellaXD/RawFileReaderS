# Changelog

## v0.3.0

### Added

- **MS2 DDA/DIA API**: Complete convenience API for MS2 scan analysis without scan data decoding
  - `ms_level_of_scan()`, `is_ms2_scan()`, `scan_numbers_by_level()` — O(1) per-scan MS level classification via ScanEvent preamble
  - `all_ms2_scan_info()` — Cached lightweight MS2 metadata (precursor m/z, isolation width, collision energy, RT, TIC) from ScanIndex + ScanEvent
  - `ms2_scans_for_precursor()` — Filter MS2 scans by precursor m/z with ppm tolerance
  - `precursor_list()` — Sorted, deduplicated unique precursor m/z values (0.01 Da dedup)
  - `parent_ms1_scan()` — Find parent MS1 via trailer "Master Scan Number" or backward walk
  - `acquisition_type()` — Classify as DDA/DIA/Mixed/MS1-only from ScanEvent `dependent` flag
  - `isolation_windows()` — Unique DIA isolation windows from MS2 ScanEvent reactions
  - `scans_for_window()` — Filter MS2 scans by isolation window membership
  - `xic_ms2_window()` — XIC within a specific DIA isolation window (rayon-parallelized)
- New types: `AcquisitionType`, `IsolationWindow`, `Ms2ScanInfo`
- Python bindings: all new methods exposed via PyO3 with `IsolationWindow` and `Ms2ScanInfo` pyclasses
- 8 new synthetic tests for serde roundtrips, boundary math, deduplication, and type equality

## v0.2.0

- Centroid-only decode path for ~2x faster XIC extraction
- Fix v66 RAW file parsing and add progress tracking
- Python and Rust API reference documentation

## v0.1.1

- Sequential SequenceRow + AutoSamplerConfig reading for RawFileInfo
- Fix A549.RAW parsing: bounds-checked reads, OldVCI fallback, diagnose CLI

## v0.1.0

- Initial release: Pure Rust Thermo RAW file reader
- OLE2 container parsing, scan data decoding (centroid/profile/FT/LT)
- TIC, BPC, XIC chromatogram extraction with rayon parallelism
- mzML conversion, Python bindings via PyO3, CLI tool
