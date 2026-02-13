//! Top-level entry point: open and read Thermo RAW files.

use crate::chromatogram;
use crate::file_header::FileHeader;
use crate::io_utils::BinaryReader;
use crate::metadata;
use crate::progress::{self, ProgressCounter};
use crate::raw_file_info::RawFileInfo;
use crate::run_header::RunHeader;
use crate::scan_data;
use crate::scan_event::{self, ScanEvent};
use crate::scan_filter;
use crate::scan_index::{self, ScanIndexEntry};
use crate::trailer::{self, TrailerLayout};
use crate::types::{Chromatogram, FileMetadata, MsLevel, PrecursorInfo, Scan};
use crate::version;
use crate::RawError;
use std::collections::HashMap;
use std::ops::Deref;
use std::path::Path;
use std::sync::OnceLock;

/// Diagnostic information for debugging address resolution.
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

/// Abstraction over file data sources (owned bytes or memory-mapped).
enum FileData {
    Owned(Vec<u8>),
    Mapped(memmap2::Mmap),
}

impl Deref for FileData {
    type Target = [u8];
    fn deref(&self) -> &[u8] {
        match self {
            FileData::Owned(v) => v,
            FileData::Mapped(m) => m,
        }
    }
}

/// A Thermo RAW file opened for reading.
pub struct RawFile {
    /// Raw file bytes (owned or memory-mapped).
    data: FileData,
    /// RAW file format version.
    version: u32,
    /// File-level metadata.
    file_metadata: FileMetadata,
    /// Parsed run header.
    run_header: RunHeader,
    /// Scan index (one entry per scan).
    scan_index: Vec<ScanIndexEntry>,
    /// Base address of the data stream.
    data_addr: u64,
    /// Pre-computed trailer layout (eagerly parsed on open).
    trailer_layout: Option<TrailerLayout>,
    /// Address of the scan events stream (for lazy parsing).
    scan_events_addr: u64,
    /// Lazily parsed scan events (unique event templates, indexed by scan_event field).
    scan_events: OnceLock<Vec<ScanEvent>>,
}

impl RawFile {
    /// Open a Thermo RAW file, reading it entirely into memory.
    ///
    /// Parses the Finnigan file header, RawFileInfo, RunHeader, ScanIndex,
    /// and trailer layout. Scan data is decoded lazily on demand.
    pub fn open(path: impl AsRef<Path>) -> Result<Self, RawError> {
        let data = std::fs::read(path.as_ref())?;
        Self::from_data(FileData::Owned(data))
    }

    /// Open a Thermo RAW file using memory-mapping.
    ///
    /// More memory-efficient for large files — the OS pages data on demand.
    ///
    /// # Safety
    /// The file must not be modified while the RawFile is open.
    pub fn open_mmap(path: impl AsRef<Path>) -> Result<Self, RawError> {
        let file = std::fs::File::open(path.as_ref())?;
        let mmap = unsafe { memmap2::Mmap::map(&file)? };
        Self::from_data(FileData::Mapped(mmap))
    }

    /// Parse RAW file structures from raw data.
    fn from_data(data: FileData) -> Result<Self, RawError> {
        let finnigan_offset = find_finnigan_magic(&data).ok_or(RawError::NotRawFile)?;

        let file_header = FileHeader::parse(&data[finnigan_offset..])
            .map_err(|e| parse_error("FileHeader", finnigan_offset as u64, None, e))?;
        let ver = file_header.version;

        if !version::is_supported(ver) {
            return Err(RawError::UnsupportedVersion(ver));
        }

        let info_base = finnigan_offset as u64 + FileHeader::size() as u64;
        let raw_file_info = find_raw_file_info_sequential(&data, info_base, ver)
            .or_else(|_| find_raw_file_info(&data, info_base, ver))
            .map_err(|e| parse_error("RawFileInfo", info_base, Some(ver), e))?;

        let rh_addr = raw_file_info.run_header_addr();
        if rh_addr == 0 {
            return Err(RawError::StreamNotFound(
                "File has no data controllers (empty/blank acquisition)".to_string(),
            ));
        }
        let run_header = RunHeader::parse(&data, rh_addr, ver)
            .map_err(|e| parse_error("RunHeader", rh_addr, Some(ver), e))?;

        let n_scans = run_header.n_scans();
        let si_addr = run_header.scan_index_addr();
        let scan_index_entries = scan_index::parse_scan_index(&data, si_addr, ver, n_scans)
            .map_err(|e| parse_error("ScanIndex", si_addr, Some(ver), e))?;

        // DataOffset (both 32-bit and 64-bit) is relative to PacketPos (the data stream base).
        // Absolute scan data offset = PacketPos + DataOffset.
        let data_addr = run_header.data_addr();
        let spect_pos = run_header.scan_index_addr();
        let trailer_extra_pos = run_header.scan_params_addr();

        // Build metadata
        let file_metadata = metadata::build_metadata(&file_header, &raw_file_info, &run_header);

        // Eagerly parse trailer layout (header only, not all records).
        // In v66, the GenericDataHeader (field descriptors) is stored before SpectPos,
        // NOT at TrailerScanEventsPos or TrailerExtraPos (which are flat record arrays).
        // We search backward from SpectPos to find the GDH, then point its records_offset
        // at TrailerExtraPos where the actual per-scan records live.
        let trailer_layout = if trailer_extra_pos > 0 && spect_pos > 0 {
            trailer::find_generic_data_header(&data, spect_pos)
                .map(|header| header.with_records_offset(trailer_extra_pos))
                .map(TrailerLayout::from_header)
                .ok()
                .or_else(|| {
                    // Fallback: try legacy approach (GDH at scan_trailer_addr)
                    let trailer_addr = run_header.scan_trailer_addr();
                    if trailer_addr > 0 {
                        trailer::parse_generic_data_header(&data, trailer_addr)
                            .map(TrailerLayout::from_header)
                            .ok()
                    } else {
                        None
                    }
                })
        } else {
            None
        };

        // Store scan events address for lazy parsing (deferred until first access).
        let scan_events_addr = run_header.scan_params_addr();

        Ok(Self {
            data,
            version: ver,
            file_metadata,
            run_header,
            scan_index: scan_index_entries,
            data_addr,
            trailer_layout,
            scan_events_addr,
            scan_events: OnceLock::new(),
        })
    }

    /// RAW file format version.
    pub fn version(&self) -> u32 {
        self.version
    }

    /// File-level metadata.
    pub fn metadata(&self) -> &FileMetadata {
        &self.file_metadata
    }

    /// Total number of scans.
    pub fn n_scans(&self) -> u32 {
        self.scan_index.len() as u32
    }

    /// First scan number.
    pub fn first_scan(&self) -> u32 {
        self.run_header.first_scan
    }

    /// Last scan number.
    pub fn last_scan(&self) -> u32 {
        self.run_header.last_scan
    }

    /// Acquisition start time in minutes.
    pub fn start_time(&self) -> f64 {
        self.run_header.start_time
    }

    /// Acquisition end time in minutes.
    pub fn end_time(&self) -> f64 {
        self.run_header.end_time
    }

    /// Low mass range.
    pub fn low_mass(&self) -> f64 {
        self.run_header.low_mass
    }

    /// High mass range.
    pub fn high_mass(&self) -> f64 {
        self.run_header.high_mass
    }

    /// Read a single scan by scan number.
    ///
    /// Decodes the scan data packet and enriches with trailer-derived metadata
    /// (filter string, MS level, polarity, precursor info).
    pub fn scan(&self, scan_number: u32) -> Result<Scan, RawError> {
        let idx = scan_number
            .checked_sub(self.run_header.first_scan)
            .ok_or(RawError::ScanOutOfRange(scan_number))? as usize;
        let entry = self
            .scan_index
            .get(idx)
            .ok_or(RawError::ScanOutOfRange(scan_number))?;

        // Look up conversion params from the scan event for FT frequency-to-m/z
        let conversion_params = self.get_conversion_params(entry);

        let mut scan = scan_data::decode_scan(
            &self.data,
            self.data_addr as usize,
            entry,
            scan_number,
            conversion_params,
        )?;

        // Enrich with trailer-derived metadata
        self.enrich_scan(&mut scan, idx as u32);

        Ok(scan)
    }

    /// Read multiple scans in parallel using rayon.
    ///
    /// Each scan is enriched with trailer-derived metadata.
    pub fn scans_parallel(&self, range: std::ops::Range<u32>) -> Result<Vec<Scan>, RawError> {
        use rayon::prelude::*;
        let first = self.run_header.first_scan;
        let entries: Vec<_> = range
            .map(|n| ((n - first) as usize, n))
            .filter_map(|(idx, n)| self.scan_index.get(idx).map(|e| (e, n, idx as u32)))
            .collect();

        entries
            .par_iter()
            .map(|(entry, scan_num, scan_idx)| {
                let conversion_params = self.get_conversion_params(entry);
                let mut scan = scan_data::decode_scan(
                    &self.data,
                    self.data_addr as usize,
                    entry,
                    *scan_num,
                    conversion_params,
                )?;
                self.enrich_scan(&mut scan, *scan_idx);
                Ok(scan)
            })
            .collect()
    }

    /// TIC chromatogram (fast: extracted from scan index, no scan data decoding).
    pub fn tic(&self) -> Chromatogram {
        chromatogram::build_tic(&self.scan_index)
    }

    /// Base peak chromatogram.
    pub fn bpc(&self) -> Chromatogram {
        chromatogram::build_bpc(&self.scan_index)
    }

    /// Extracted ion chromatogram for a target m/z with tolerance in ppm.
    ///
    /// Processes all scans (MS1 + MS2). Use [`xic_ms1`] to restrict to MS1 only.
    /// Uses scan index m/z ranges to skip scans that cannot contain the target,
    /// avoiding expensive scan data decoding for irrelevant scans.
    pub fn xic(&self, target_mz: f64, tolerance_ppm: f64) -> Result<Chromatogram, RawError> {
        self.xic_inner(target_mz, tolerance_ppm, false)
    }

    /// Extracted ion chromatogram restricted to MS1 scans only.
    ///
    /// Uses the trailer's "Master Scan Number" field to skip MS2+ scans without
    /// decoding their data. For DDA data this typically reduces scans to process
    /// by 85-95%, making it competitive with or faster than the .NET library.
    pub fn xic_ms1(&self, target_mz: f64, tolerance_ppm: f64) -> Result<Chromatogram, RawError> {
        self.xic_inner(target_mz, tolerance_ppm, true)
    }

    /// Batch extracted ion chromatograms for multiple targets (MS1 only, single pass).
    ///
    /// Decodes each MS1 scan once and extracts intensities for all targets,
    /// avoiding redundant scan decoding when extracting multiple XICs.
    pub fn xic_batch_ms1(&self, targets: &[(f64, f64)]) -> Result<Vec<Chromatogram>, RawError> {
        use rayon::prelude::*;

        let ranges: Vec<(f64, f64)> = targets
            .iter()
            .map(|&(mz, ppm)| {
                let half = mz * ppm * 1e-6;
                (mz - half, mz + half)
            })
            .collect();
        let n_targets = targets.len();

        // Each MS1 scan produces (rt, Vec<intensity_per_target>); MS2 scans produce None.
        let per_scan: Vec<Option<(f64, Vec<f64>)>> = self
            .scan_index
            .par_iter()
            .enumerate()
            .map(|(idx, entry)| {
                if !self.is_ms1_scan(idx as u32) {
                    return None;
                }

                // Check if any target overlaps this scan's m/z range
                let any_overlap = if entry.low_mz > 0.0 && entry.high_mz > 0.0 {
                    ranges
                        .iter()
                        .any(|&(low, high)| entry.high_mz >= low && entry.low_mz <= high)
                } else {
                    true
                };

                if !any_overlap {
                    return Some((entry.rt, vec![0.0; n_targets]));
                }

                let (cmz, cint) = match scan_data::decode_centroids_only(
                    &self.data,
                    self.data_addr as usize,
                    entry,
                ) {
                    Ok(pair) => pair,
                    Err(_) => return Some((entry.rt, vec![0.0; n_targets])),
                };

                let intensities: Vec<f64> = ranges
                    .iter()
                    .map(|&(low, high)| {
                        cmz.iter()
                            .zip(cint.iter())
                            .filter(|(&mz, _)| mz >= low && mz <= high)
                            .map(|(_, &int)| int)
                            .sum()
                    })
                    .collect();

                Some((entry.rt, intensities))
            })
            .collect();

        // Transpose: Vec<(rt, Vec<intensity>)> → Vec<Chromatogram>
        let filtered: Vec<_> = per_scan.into_iter().flatten().collect();
        let mut rts: Vec<f64> = filtered.iter().map(|(rt, _)| *rt).collect();

        // Build chromatograms: clone rts for all but the last, move for the last (saves one alloc)
        let mut chromatograms = Vec::with_capacity(n_targets);
        for t in 0..n_targets {
            let intensity = filtered.iter().map(|(_, ints)| ints[t]).collect();
            let rt = if t + 1 < n_targets {
                rts.clone()
            } else {
                std::mem::take(&mut rts)
            };
            chromatograms.push(Chromatogram { rt, intensity });
        }

        Ok(chromatograms)
    }

    /// Like [`scans_parallel`], but increments the progress counter after each scan.
    pub fn scans_parallel_with_progress(
        &self,
        range: std::ops::Range<u32>,
        counter: &ProgressCounter,
    ) -> Result<Vec<Scan>, RawError> {
        use rayon::prelude::*;
        let first = self.run_header.first_scan;
        let entries: Vec<_> = range
            .map(|n| ((n - first) as usize, n))
            .filter_map(|(idx, n)| self.scan_index.get(idx).map(|e| (e, n, idx as u32)))
            .collect();

        entries
            .par_iter()
            .map(|(entry, scan_num, scan_idx)| {
                let conversion_params = self.get_conversion_params(entry);
                let mut scan = scan_data::decode_scan(
                    &self.data,
                    self.data_addr as usize,
                    entry,
                    *scan_num,
                    conversion_params,
                )?;
                self.enrich_scan(&mut scan, *scan_idx);
                progress::tick(counter);
                Ok(scan)
            })
            .collect()
    }

    /// Like [`xic`], but increments the progress counter per scan index entry.
    pub fn xic_with_progress(
        &self,
        target_mz: f64,
        tolerance_ppm: f64,
        counter: &ProgressCounter,
    ) -> Result<Chromatogram, RawError> {
        self.xic_inner_with_progress(target_mz, tolerance_ppm, false, counter)
    }

    /// Like [`xic_ms1`], but increments the progress counter per scan index entry.
    pub fn xic_ms1_with_progress(
        &self,
        target_mz: f64,
        tolerance_ppm: f64,
        counter: &ProgressCounter,
    ) -> Result<Chromatogram, RawError> {
        self.xic_inner_with_progress(target_mz, tolerance_ppm, true, counter)
    }

    /// Like [`xic_batch_ms1`], but increments the progress counter per scan index entry.
    pub fn xic_batch_ms1_with_progress(
        &self,
        targets: &[(f64, f64)],
        counter: &ProgressCounter,
    ) -> Result<Vec<Chromatogram>, RawError> {
        use rayon::prelude::*;

        let ranges: Vec<(f64, f64)> = targets
            .iter()
            .map(|&(mz, ppm)| {
                let half = mz * ppm * 1e-6;
                (mz - half, mz + half)
            })
            .collect();
        let n_targets = targets.len();

        let per_scan: Vec<Option<(f64, Vec<f64>)>> = self
            .scan_index
            .par_iter()
            .enumerate()
            .map(|(idx, entry)| {
                let result = if !self.is_ms1_scan(idx as u32) {
                    None
                } else {
                    let any_overlap = if entry.low_mz > 0.0 && entry.high_mz > 0.0 {
                        ranges
                            .iter()
                            .any(|&(low, high)| entry.high_mz >= low && entry.low_mz <= high)
                    } else {
                        true
                    };

                    if !any_overlap {
                        Some((entry.rt, vec![0.0; n_targets]))
                    } else {
                        let (cmz, cint) = match scan_data::decode_centroids_only(
                            &self.data,
                            self.data_addr as usize,
                            entry,
                        ) {
                            Ok(pair) => pair,
                            Err(_) => {
                                return {
                                    progress::tick(counter);
                                    Some((entry.rt, vec![0.0; n_targets]))
                                }
                            }
                        };

                        let intensities: Vec<f64> = ranges
                            .iter()
                            .map(|&(low, high)| {
                                cmz.iter()
                                    .zip(cint.iter())
                                    .filter(|(&mz, _)| mz >= low && mz <= high)
                                    .map(|(_, &int)| int)
                                    .sum()
                            })
                            .collect();

                        Some((entry.rt, intensities))
                    }
                };
                progress::tick(counter);
                result
            })
            .collect();

        let filtered: Vec<_> = per_scan.into_iter().flatten().collect();
        let mut rts: Vec<f64> = filtered.iter().map(|(rt, _)| *rt).collect();

        let mut chromatograms = Vec::with_capacity(n_targets);
        for t in 0..n_targets {
            let intensity = filtered.iter().map(|(_, ints)| ints[t]).collect();
            let rt = if t + 1 < n_targets {
                rts.clone()
            } else {
                std::mem::take(&mut rts)
            };
            chromatograms.push(Chromatogram { rt, intensity });
        }

        Ok(chromatograms)
    }

    /// Internal XIC with progress, shared between `xic_with_progress()` and `xic_ms1_with_progress()`.
    fn xic_inner_with_progress(
        &self,
        target_mz: f64,
        tolerance_ppm: f64,
        ms1_only: bool,
        counter: &ProgressCounter,
    ) -> Result<Chromatogram, RawError> {
        use rayon::prelude::*;
        let half_width = target_mz * tolerance_ppm * 1e-6;
        let low = target_mz - half_width;
        let high = target_mz + half_width;

        let results: Vec<Option<(f64, f64)>> = self
            .scan_index
            .par_iter()
            .enumerate()
            .map(|(idx, entry)| {
                let result = if ms1_only && !self.is_ms1_scan(idx as u32) {
                    None
                } else if entry.low_mz > 0.0
                    && entry.high_mz > 0.0
                    && (entry.high_mz < low || entry.low_mz > high)
                {
                    Some((entry.rt, 0.0))
                } else {
                    let (cmz, cint) = match scan_data::decode_centroids_only(
                        &self.data,
                        self.data_addr as usize,
                        entry,
                    ) {
                        Ok(pair) => pair,
                        Err(_) => {
                            return {
                                progress::tick(counter);
                                Some((entry.rt, 0.0))
                            }
                        }
                    };
                    let intensity: f64 = cmz
                        .iter()
                        .zip(cint.iter())
                        .filter(|(&mz, _)| mz >= low && mz <= high)
                        .map(|(_, &int)| int)
                        .sum();
                    Some((entry.rt, intensity))
                };
                progress::tick(counter);
                result
            })
            .collect();

        let filtered: Vec<_> = results.into_iter().flatten().collect();
        Ok(Chromatogram {
            rt: filtered.iter().map(|(rt, _)| *rt).collect(),
            intensity: filtered.iter().map(|(_, int)| *int).collect(),
        })
    }

    /// Internal XIC implementation shared between `xic()` and `xic_ms1()`.
    fn xic_inner(
        &self,
        target_mz: f64,
        tolerance_ppm: f64,
        ms1_only: bool,
    ) -> Result<Chromatogram, RawError> {
        use rayon::prelude::*;
        let half_width = target_mz * tolerance_ppm * 1e-6;
        let low = target_mz - half_width;
        let high = target_mz + half_width;

        let results: Vec<Option<(f64, f64)>> = self
            .scan_index
            .par_iter()
            .enumerate()
            .map(|(idx, entry)| {
                if ms1_only && !self.is_ms1_scan(idx as u32) {
                    return None;
                }

                // Pre-filter: skip scans whose m/z range doesn't overlap the target
                if entry.low_mz > 0.0
                    && entry.high_mz > 0.0
                    && (entry.high_mz < low || entry.low_mz > high)
                {
                    return Some((entry.rt, 0.0));
                }

                let (cmz, cint) = match scan_data::decode_centroids_only(
                    &self.data,
                    self.data_addr as usize,
                    entry,
                ) {
                    Ok(pair) => pair,
                    Err(_) => return Some((entry.rt, 0.0)),
                };
                let intensity: f64 = cmz
                    .iter()
                    .zip(cint.iter())
                    .filter(|(&mz, _)| mz >= low && mz <= high)
                    .map(|(_, &int)| int)
                    .sum();
                Some((entry.rt, intensity))
            })
            .collect();

        let filtered: Vec<_> = results.into_iter().flatten().collect();
        Ok(Chromatogram {
            rt: filtered.iter().map(|(rt, _)| *rt).collect(),
            intensity: filtered.iter().map(|(_, int)| *int).collect(),
        })
    }

    /// Fast MS1 check using trailer metadata (no scan data decoding).
    ///
    /// Reads only the "Master Scan Number" i32 field from the trailer record.
    /// Returns `true` if the scan is MS1 (master == 0) or if MS level cannot
    /// be determined (no trailer data).
    pub fn is_ms1_scan(&self, scan_idx: u32) -> bool {
        if let Some(layout) = &self.trailer_layout {
            if let Some(master_idx) = layout.master_scan_idx {
                if let Ok(master) = layout.read_i32(&self.data, scan_idx, master_idx) {
                    return master == 0;
                }
            }
            // Fallback: check filter text if available
            if let Some(fi) = layout.filter_text_idx {
                if let Ok(filter_str) = layout.read_string(&self.data, scan_idx, fi) {
                    return filter_str.contains(" ms ")
                        || filter_str.starts_with("ms ")
                        || filter_str.contains(" Full ms ");
                }
            }
        }
        // No trailer: can't determine MS level, assume MS1
        true
    }

    /// Get trailer extra data for a specific scan as a HashMap.
    pub fn trailer_extra(&self, scan_number: u32) -> Result<HashMap<String, String>, RawError> {
        let layout = self
            .trailer_layout
            .as_ref()
            .ok_or_else(|| RawError::StreamNotFound("trailer extra".to_string()))?;

        let scan_idx = scan_number
            .checked_sub(self.run_header.first_scan)
            .ok_or(RawError::ScanOutOfRange(scan_number))?;

        trailer::parse_trailer_extra(&self.data, &layout.header, scan_idx)
    }

    /// Get the list of trailer extra field labels.
    pub fn trailer_fields(&self) -> Vec<String> {
        match &self.trailer_layout {
            Some(layout) => layout.field_labels(),
            None => vec![],
        }
    }

    /// Get the raw scan index entries.
    pub fn scan_index(&self) -> &[ScanIndexEntry] {
        &self.scan_index
    }

    /// Get the parsed scan events (lazily parsed on first access).
    pub fn scan_events(&self) -> &[ScanEvent] {
        self.scan_events_lazy()
    }

    /// Lazily parse scan events on first access.
    fn scan_events_lazy(&self) -> &Vec<ScanEvent> {
        self.scan_events.get_or_init(|| {
            if self.scan_events_addr > 0 {
                scan_event::parse_scan_events(&self.data, self.scan_events_addr, self.version)
                    .unwrap_or_default()
            } else {
                vec![]
            }
        })
    }

    /// Get the parsed RunHeader (for diagnostics).
    pub fn run_header(&self) -> &RunHeader {
        &self.run_header
    }

    /// File size in bytes.
    pub fn file_size(&self) -> usize {
        self.data.len()
    }

    /// Diagnostic info for debugging address resolution.
    pub fn debug_info(&self) -> DebugInfo {
        let rh = &self.run_header;
        let first_entries: Vec<_> = self.scan_index.iter().take(3).cloned().collect();

        DebugInfo {
            file_size: self.data.len() as u64,
            version: self.version,
            run_header_start: rh.start_offset,
            scan_index_addr_32: rh.scan_index_addr_32,
            data_addr_32: rh.data_addr_32,
            scan_trailer_addr_32: rh.scan_trailer_addr_32,
            scan_params_addr_32: rh.scan_params_addr_32,
            scan_index_addr_64: rh.scan_index_addr_64,
            data_addr_64: rh.data_addr_64,
            scan_trailer_addr_64: rh.scan_trailer_addr_64,
            scan_params_addr_64: rh.scan_params_addr_64,
            effective_data_addr: self.data_addr,
            first_scan_entries: first_entries,
            n_scans: self.scan_index.len() as u32,
            n_scan_events: self.scan_events_lazy().len() as u32,
            instrument_type: rh.instrument_type,
        }
    }

    /// List OLE2 streams in the file (uses cfb-reader).
    pub fn list_streams(path: impl AsRef<Path>) -> Result<Vec<String>, RawError> {
        let container =
            cfb_reader::Ole2Container::open(path).map_err(|e| RawError::CfbError(e.to_string()))?;
        Ok(container.list_streams())
    }

    /// Look up conversion parameters for a scan from its ScanEvent.
    fn get_conversion_params(&self, entry: &ScanIndexEntry) -> &[f64] {
        self.scan_events_lazy()
            .get(entry.scan_event as usize)
            .map(|e| e.conversion_params.as_slice())
            .unwrap_or(&[])
    }

    /// Enrich a scan with trailer-derived metadata.
    ///
    /// Uses three strategies in order of preference:
    /// 1. Filter text from trailer (most complete: MS level, polarity, activation)
    /// 2. Trailer metadata fields (Master Scan Number → MS level, precursor info)
    /// 3. ScanEvent preamble (fallback when trailer unavailable)
    fn enrich_scan(&self, scan: &mut Scan, scan_idx: u32) {
        let mut enriched = false;

        if let Some(layout) = &self.trailer_layout {
            // Strategy 1: Filter text (provides MS level, polarity, activation type)
            if let Some(fi) = layout.filter_text_idx {
                if let Ok(filter_str) = layout.read_string(&self.data, scan_idx, fi) {
                    if !filter_str.is_empty() {
                        let filter = scan_filter::parse_filter(&filter_str);
                        scan.ms_level = filter.ms_level;
                        scan.polarity = filter.polarity;
                        scan.filter_string = Some(filter_str);

                        if !matches!(scan.ms_level, MsLevel::Ms1) {
                            scan.precursor = self.build_precursor_info(layout, scan_idx, &filter);
                        }
                        enriched = true;
                    }
                }
            }

            // Strategy 2: Trailer metadata fields (Master Scan Number → MS2 detection)
            if !enriched {
                if let Some(master_idx) = layout.master_scan_idx {
                    if let Ok(master) = layout.read_i32(&self.data, scan_idx, master_idx) {
                        if master > 0 {
                            scan.ms_level = MsLevel::Ms2;
                            scan.precursor =
                                self.build_precursor_info_from_trailer(layout, scan_idx);
                        }
                        // master == 0 → MS1 (the default)
                        enriched = true;
                    }
                }
            }
        }

        // Strategy 3: ScanEvent preamble (when trailer unavailable)
        if !enriched {
            self.enrich_from_scan_event(scan, scan_idx);
        }
    }

    /// Enrich scan metadata from parsed ScanEvent (fallback when trailer unavailable).
    fn enrich_from_scan_event(&self, scan: &mut Scan, scan_idx: u32) {
        let entry = match self.scan_index.get(scan_idx as usize) {
            Some(e) => e,
            None => return,
        };
        let event = match self.scan_events_lazy().get(entry.scan_event as usize) {
            Some(e) => e,
            None => return,
        };
        let preamble = &event.preamble;

        scan.ms_level = preamble.ms_level;
        scan.polarity = preamble.polarity;

        // Build precursor info from scan event reactions for MS2+ scans
        if !matches!(scan.ms_level, MsLevel::Ms1) {
            if let Some(reaction) = event.reactions.last() {
                // Derive activation type from the Reaction's CollisionEnergyValid field
                let activation_str = format!("{}", reaction.activation_type());
                scan.precursor = Some(PrecursorInfo {
                    mz: reaction.precursor_mz,
                    charge: None, // Not available from scan event
                    isolation_width: Some(reaction.isolation_width).filter(|&w| w > 0.0),
                    activation_type: Some(activation_str),
                    collision_energy: Some(reaction.collision_energy),
                });
            }
        }
    }

    /// Build PrecursorInfo from trailer metadata fields alone (no filter string).
    ///
    /// Used when filter text is unavailable. Reads Monoisotopic M/Z, Charge State,
    /// and MS2 Isolation Width directly from the trailer record.
    fn build_precursor_info_from_trailer(
        &self,
        layout: &TrailerLayout,
        scan_idx: u32,
    ) -> Option<PrecursorInfo> {
        let mono_mz = layout
            .mono_mz_idx
            .and_then(|idx| layout.read_f64(&self.data, scan_idx, idx).ok())
            .filter(|&mz| mz > 0.0);

        let charge = layout
            .charge_state_idx
            .and_then(|idx| layout.read_i32(&self.data, scan_idx, idx).ok())
            .filter(|&c| c != 0);

        let isolation_width = layout
            .isolation_width_idx
            .and_then(|idx| layout.read_f64(&self.data, scan_idx, idx).ok())
            .filter(|&w| w > 0.0 && w < 100.0); // exclude full-scan-range widths

        let mz = mono_mz?;

        Some(PrecursorInfo {
            mz,
            charge,
            isolation_width,
            activation_type: None,
            collision_energy: None,
        })
    }

    /// Build PrecursorInfo from trailer fields and filter string.
    ///
    /// Prefers trailer-derived monoisotopic m/z (more accurate) over filter m/z.
    fn build_precursor_info(
        &self,
        layout: &TrailerLayout,
        scan_idx: u32,
        filter: &scan_filter::ScanFilter,
    ) -> Option<PrecursorInfo> {
        let filter_precursor = filter.precursor.as_ref();

        // Get monoisotopic m/z from trailer (more accurate than filter string)
        let mono_mz = layout
            .mono_mz_idx
            .and_then(|idx| layout.read_f64(&self.data, scan_idx, idx).ok())
            .filter(|&mz| mz > 0.0);

        // Get charge state from trailer
        let charge = layout
            .charge_state_idx
            .and_then(|idx| layout.read_i32(&self.data, scan_idx, idx).ok())
            .filter(|&c| c != 0);

        // Get isolation width from trailer
        let isolation_width = layout
            .isolation_width_idx
            .and_then(|idx| layout.read_f64(&self.data, scan_idx, idx).ok())
            .filter(|&w| w > 0.0);

        // Prefer monoisotopic m/z from trailer; fall back to filter string
        let mz = mono_mz.or_else(|| filter_precursor.map(|p| p.mz))?;

        Some(PrecursorInfo {
            mz,
            charge,
            isolation_width,
            activation_type: filter_precursor.map(|p| p.activation.clone()),
            collision_energy: filter_precursor.map(|p| p.collision_energy),
        })
    }
}

/// Stage-by-stage diagnostic result for a RAW file.
pub struct DiagnosticReport {
    pub file_size: u64,
    pub stages: Vec<DiagnosticStage>,
}

pub struct DiagnosticStage {
    pub name: String,
    pub success: bool,
    pub detail: String,
}

impl DiagnosticReport {
    pub fn print(&self) {
        println!("=== RAW File Diagnostic Report ===");
        println!(
            "File size: {} bytes ({:.1} MB)\n",
            self.file_size,
            self.file_size as f64 / 1e6
        );
        for stage in &self.stages {
            let status = if stage.success { "OK" } else { "FAIL" };
            println!("[{:>4}] {}", status, stage.name);
            for line in stage.detail.lines() {
                println!("       {}", line);
            }
        }
    }
}

/// Run stage-by-stage diagnostics on raw file data without cascading failures.
pub fn diagnose(data: &[u8]) -> DiagnosticReport {
    let file_size = data.len() as u64;
    let mut stages = Vec::new();

    // Stage 1: Find Finnigan magic
    let finnigan_offset = match find_finnigan_magic(data) {
        Some(off) => {
            stages.push(DiagnosticStage {
                name: "Finnigan magic".to_string(),
                success: true,
                detail: format!("Found at offset {}", off),
            });
            off
        }
        None => {
            stages.push(DiagnosticStage {
                name: "Finnigan magic".to_string(),
                success: false,
                detail: "Not found in first 64KB".to_string(),
            });
            return DiagnosticReport { file_size, stages };
        }
    };

    // Stage 2: Parse FileHeader
    let file_header = match FileHeader::parse(&data[finnigan_offset..]) {
        Ok(h) => {
            stages.push(DiagnosticStage {
                name: "FileHeader".to_string(),
                success: true,
                detail: format!("Version: {}, signature: {:?}", h.version, h.tag),
            });
            h
        }
        Err(e) => {
            stages.push(DiagnosticStage {
                name: "FileHeader".to_string(),
                success: false,
                detail: format!("Parse error: {}", e),
            });
            return DiagnosticReport { file_size, stages };
        }
    };
    let ver = file_header.version;

    if !version::is_supported(ver) {
        stages.push(DiagnosticStage {
            name: "Version check".to_string(),
            success: false,
            detail: format!("Version {} not supported (need 57-66)", ver),
        });
        return DiagnosticReport { file_size, stages };
    }

    // Stage 3: Find RawFileInfo (sequential then fallback to search)
    let info_base = finnigan_offset as u64 + FileHeader::size() as u64;
    let (raw_file_info, rfi_method) = match find_raw_file_info_sequential(data, info_base, ver) {
        Ok(info) => (info, "sequential"),
        Err(seq_err) => match find_raw_file_info(data, info_base, ver) {
            Ok(info) => (info, "search"),
            Err(e) => {
                stages.push(DiagnosticStage {
                    name: "RawFileInfo".to_string(),
                    success: false,
                    detail: format!(
                        "Sequential failed: {}\nSearch failed from offset {}: {}",
                        seq_err, info_base, e
                    ),
                });
                return DiagnosticReport { file_size, stages };
            }
        },
    };
    {
        let n_active = raw_file_info
            .controllers
            .iter()
            .filter(|c| c.offset > 0)
            .count();
        stages.push(DiagnosticStage {
            name: "RawFileInfo".to_string(),
            success: true,
            detail: format!(
                "Found via {} reading\nDate: {}, n_controllers: {} ({} active), end_offset: {}",
                rfi_method,
                raw_file_info.acquisition_date(),
                raw_file_info.n_controllers,
                n_active,
                raw_file_info.end_offset
            ),
        });
    }

    // Stage 4: Parse RunHeader
    let rh_addr = raw_file_info.run_header_addr();
    if rh_addr == 0 {
        stages.push(DiagnosticStage {
            name: "RunHeader".to_string(),
            success: false,
            detail: "No data controllers (run_header_addr = 0)".to_string(),
        });
        return DiagnosticReport { file_size, stages };
    }

    let run_header = match RunHeader::parse(data, rh_addr, ver) {
        Ok(rh) => {
            stages.push(DiagnosticStage {
                name: "RunHeader".to_string(),
                success: true,
                detail: format!(
                    "Scans: {}-{}, RT: {:.2}-{:.2} min, mass: {:.1}-{:.1}\n\
                     ScanIndex64: {:?}, DataAddr64: {:?}\n\
                     TrailerAddr64: {:?}, ParamsAddr64: {:?}\n\
                     Device: {}, Model: {}",
                    rh.first_scan,
                    rh.last_scan,
                    rh.start_time,
                    rh.end_time,
                    rh.low_mass,
                    rh.high_mass,
                    rh.scan_index_addr_64,
                    rh.data_addr_64,
                    rh.scan_trailer_addr_64,
                    rh.scan_params_addr_64,
                    rh.device_name,
                    rh.model,
                ),
            });
            rh
        }
        Err(e) => {
            stages.push(DiagnosticStage {
                name: "RunHeader".to_string(),
                success: false,
                detail: format!("Parse error at offset {}: {}", rh_addr, e),
            });
            return DiagnosticReport { file_size, stages };
        }
    };

    // Stage 5: Parse ScanIndex
    let n_scans = run_header.n_scans();
    let si_addr = run_header.scan_index_addr();
    let scan_index_entries = match scan_index::parse_scan_index(data, si_addr, ver, n_scans) {
        Ok(entries) => {
            let sample = entries
                .iter()
                .take(3)
                .map(|e| format!("offset={}, size={}, rt={:.4}", e.offset, e.data_size, e.rt))
                .collect::<Vec<_>>()
                .join(", ");
            stages.push(DiagnosticStage {
                name: "ScanIndex".to_string(),
                success: true,
                detail: format!(
                    "{} entries parsed at offset {}\nFirst entries: [{}]",
                    entries.len(),
                    si_addr,
                    sample
                ),
            });
            entries
        }
        Err(e) => {
            stages.push(DiagnosticStage {
                name: "ScanIndex".to_string(),
                success: false,
                detail: format!(
                    "Parse error at offset {} ({} scans): {}",
                    si_addr, n_scans, e
                ),
            });
            return DiagnosticReport { file_size, stages };
        }
    };

    // Stage 6: TrailerLayout
    let spect_pos = run_header.scan_index_addr();
    let trailer_extra_pos = run_header.scan_params_addr();
    if trailer_extra_pos > 0 && spect_pos > 0 {
        let layout = trailer::find_generic_data_header(data, spect_pos)
            .map(|h| h.with_records_offset(trailer_extra_pos))
            .and_then(|h| Ok(TrailerLayout::from_header(h)))
            .or_else(|_| {
                let addr = run_header.scan_trailer_addr();
                if addr > 0 {
                    trailer::parse_generic_data_header(data, addr).map(TrailerLayout::from_header)
                } else {
                    Err(RawError::StreamNotFound("No trailer address".to_string()))
                }
            });

        match layout {
            Ok(layout) => stages.push(DiagnosticStage {
                name: "TrailerLayout".to_string(),
                success: true,
                detail: format!(
                    "{} fields, record_size={}, filter_idx={:?}, master_scan_idx={:?}",
                    layout.header.descriptors.len(),
                    layout.record_size,
                    layout.filter_text_idx,
                    layout.master_scan_idx
                ),
            }),
            Err(e) => stages.push(DiagnosticStage {
                name: "TrailerLayout".to_string(),
                success: false,
                detail: format!("GDH search failed: {}", e),
            }),
        }
    } else {
        stages.push(DiagnosticStage {
            name: "TrailerLayout".to_string(),
            success: false,
            detail: format!(
                "Skipped (trailer_extra_pos={}, spect_pos={})",
                trailer_extra_pos, spect_pos
            ),
        });
    }

    // Stage 7: Try decoding scan 1
    if let Some(first_entry) = scan_index_entries.first() {
        let data_addr = run_header.data_addr();
        let scan_num = run_header.first_scan;
        let result = scan_data::decode_scan(data, data_addr as usize, first_entry, scan_num, &[]);

        let (success, detail) = match result {
            Ok(scan) => (
                true,
                format!(
                    "{} centroids, tic={:.2e}, base_peak_mz={:.4}",
                    scan.centroid_mz.len(),
                    scan.tic,
                    scan.base_peak_mz
                ),
            ),
            Err(e) => (
                false,
                format!(
                    "Abs offset={}, data_size={}: {}",
                    data_addr + first_entry.offset,
                    first_entry.data_size,
                    e
                ),
            ),
        };

        stages.push(DiagnosticStage {
            name: "Scan decode (first)".to_string(),
            success,
            detail,
        });
    }

    DiagnosticReport { file_size, stages }
}

/// Find RawFileInfo by sequentially reading SequenceRow + AutoSamplerConfig first.
///
/// The .NET DLL reads these structures in order: FileHeader -> SequenceRow ->
/// AutoSamplerConfig -> RawFileInfo. Each consumes a known number of bytes,
/// so the cursor always lands at the correct offset. This avoids the fragile
/// search approach needed when intermediate structures have unknown sizes.
fn find_raw_file_info_sequential(
    data: &[u8],
    info_base: u64,
    version: u32,
) -> Result<RawFileInfo, RawError> {
    let mut reader = BinaryReader::at_offset(data, info_base);

    skip_sequence_row(&mut reader, version)?;
    skip_auto_sampler_config(&mut reader, version)?;

    let rfi_offset = reader.position();
    let info = RawFileInfo::parse(data, rfi_offset, version)?;
    let file_size = data.len() as u64;
    if info.has_valid_controllers(file_size) {
        Ok(info)
    } else {
        Err(RawError::CorruptedData(format!(
            "Sequential reading: RawFileInfo at offset {} has no valid controllers",
            rfi_offset
        )))
    }
}

/// Skip past the SequenceRow structure (variable-length).
///
/// Layout: 60-byte fixed struct + version-dependent PascalStrings.
/// We don't need the data, just need to advance the cursor correctly.
fn skip_sequence_row(reader: &mut BinaryReader, version: u32) -> Result<(), RawError> {
    // SeqRowInfoStruct: 60 bytes fixed
    // Revision(i32) + RowNumber(i32) + SampleType(i32) + VialName(UTF16[4]=8 bytes)
    // + InjectionVolume(f64) + SampleWeight(f64) + SampleVolume(f64)
    // + ISTDAmount(f64) + DilutionFactor(f64)
    reader.skip(60)?;

    // 13 base PascalStrings:
    // CalLevel, SampleName, SampleId, Comment (4)
    // UserTexts[5] (5)
    // Inst, Method, RawFileName, Path (4)
    for _ in 0..13 {
        reader.skip_pascal_string()?;
    }

    // Version-dependent strings
    if version >= 25 {
        // Vial, CalibFile
        reader.skip_pascal_string()?;
        reader.skip_pascal_string()?;
    }

    if version >= 41 {
        // Barcode (string) + BarcodeStatus (i32)
        reader.skip_pascal_string()?;
        reader.skip(4)?;
    }

    if version >= 58 {
        // ExtraUserColumns[15]
        for _ in 0..15 {
            reader.skip_pascal_string()?;
        }
    }

    // Note: v66+ files written by newer acquisition software may have additional
    // PascalStrings after ExtraUserColumns (e.g., SampleExtensionInfo JSON blobs).
    // These extra strings are NOT part of the standard SequenceRow.Load code in
    // Thermo's v8.0.6 library. We don't try to consume them here because it's
    // impossible to reliably distinguish extra PascalStrings from the start of
    // AutoSamplerConfig (whose TrayIndex=0 looks like an empty PascalString).
    // The fallback scanner in find_raw_file_info() handles these files correctly.

    Ok(())
}

/// Skip past the AutoSamplerConfig structure (version-dependent).
///
/// Only present for version >= 36. Layout: 24-byte fixed struct + TrayName PascalString.
fn skip_auto_sampler_config(reader: &mut BinaryReader, version: u32) -> Result<(), RawError> {
    if version < 36 {
        return Ok(());
    }

    // AutoSamplerConfigStruct: 24 bytes
    // TrayIndex(i32) + VialIndex(i32) + VialsPerTray(i32)
    // + VialsPerTrayX(i32) + VialsPerTrayY(i32) + TrayShape(i32)
    reader.skip(24)?;

    // TrayName (PascalString)
    reader.skip_pascal_string()?;

    Ok(())
}

/// Search for a valid RawFileInfo by scanning forward from the given offset.
///
/// In v66+ files, .NET serialized metadata blobs (SequencerRow, AutoSamplerInfo)
/// sit between the FileHeader and RawFileInfo. This function scans forward in
/// 2-byte steps, attempting to parse RawFileInfo at each candidate offset and
/// validating the result by checking both VCI entries and RunHeader reachability.
fn find_raw_file_info(data: &[u8], start: u64, version: u32) -> Result<RawFileInfo, RawError> {
    let file_size = data.len() as u64;
    // Search up to 16KB past the FileHeader (more than enough for any blob)
    let search_limit = (start + 16384).min(file_size);

    let mut offset = start;
    while offset < search_limit {
        if let Ok(info) = RawFileInfo::parse(data, offset, version) {
            if info.has_valid_controllers(file_size) {
                // Additional validation: verify the RunHeader at the MS address
                // is actually parseable. This eliminates false positives where
                // random data in SequenceRow/ASC looks like valid VCI entries.
                let rh_addr = info.run_header_addr();
                if rh_addr > 0 && RunHeader::parse(data, rh_addr, version).is_ok() {
                    return Ok(info);
                }
            }
        }
        offset += 2;
    }

    Err(RawError::StreamNotFound(
        "RawFileInfo: no valid VCI controllers found within search range".to_string(),
    ))
}

/// Search for the Finnigan magic (0xA101) in the file data.
/// Returns the byte offset of the magic, or None if not found.
fn find_finnigan_magic(data: &[u8]) -> Option<usize> {
    let magic_le = 0xA101u16.to_le_bytes();
    let search_limit = data.len().min(65536);

    for i in 0..search_limit.saturating_sub(1) {
        if data[i] == magic_le[0] && data[i + 1] == magic_le[1] {
            // Verify: version u32 lives at offset +36 from magic
            // (2 magic + 18 signature + 16 unknowns = 36).
            if i + 40 <= data.len() {
                let ver = u32::from_le_bytes(data[i + 36..i + 40].try_into().ok()?);
                if ver > 0 && ver <= 200 {
                    return Some(i);
                }
            }
        }
    }
    None
}

/// Helper to format parse errors with consistent context.
fn parse_error(
    component: &str,
    offset: u64,
    version: Option<u32>,
    error: impl std::fmt::Display,
) -> RawError {
    let version_str = version.map_or(String::new(), |v| format!(" (v{})", v));
    RawError::CorruptedData(format!(
        "{} parsing failed at offset {}{}: {}",
        component, offset, version_str, error
    ))
}

#[cfg(test)]
mod tests {
    use super::*;

    /// Helper to create a PascalStringWin32 in bytes (i32 length + UTF-16LE chars).
    fn make_pascal_string(s: &str) -> Vec<u8> {
        let mut bytes = Vec::new();
        let len = s.len() as i32;
        bytes.extend(&len.to_le_bytes());
        for c in s.encode_utf16() {
            bytes.extend(&c.to_le_bytes());
        }
        bytes
    }

    // Tests for skip_sequence_row()

    #[test]
    fn test_skip_sequence_row_version_57() {
        // Version 57: 60 bytes + 13 base strings + 2 extra strings (version >= 25) + 2 extra (version >= 41)
        let mut data = Vec::new();

        // 60-byte fixed struct
        data.extend(vec![0u8; 60]);

        // 13 base PascalStrings (empty strings for simplicity)
        for _ in 0..13 {
            data.extend(&make_pascal_string(""));
        }

        // Version >= 25: Vial, CalibFile
        data.extend(&make_pascal_string("Vial1"));
        data.extend(&make_pascal_string("calib.cal"));

        // Version >= 41: Barcode (string) + BarcodeStatus (i32)
        data.extend(&make_pascal_string("BC12345"));
        data.extend(&[0x01, 0x00, 0x00, 0x00]); // BarcodeStatus: 1

        // Trailing marker
        data.extend(&[0xFF, 0x00, 0x00, 0x00]); // u32: 255

        let mut reader = BinaryReader::new(&data);

        skip_sequence_row(&mut reader, 57).unwrap();

        // Verify cursor advanced correctly (60 + 13*4 + Vial + CalibFile + Barcode + 4)
        let expected_pos = 60 + 13 * 4 + (4 + 10) + (4 + 18) + (4 + 14) + 4;
        assert_eq!(reader.position(), expected_pos as u64);

        // Verify we can read trailing marker
        assert_eq!(reader.read_u32().unwrap(), 255);
    }

    #[test]
    fn test_skip_sequence_row_version_58_with_extra_columns() {
        // Version 58: includes ExtraUserColumns[15]
        let mut data = Vec::new();

        // 60-byte fixed struct
        data.extend(vec![0u8; 60]);

        // 13 base PascalStrings
        for i in 0..13 {
            data.extend(&make_pascal_string(&format!("str{}", i)));
        }

        // Version >= 25: Vial, CalibFile
        data.extend(&make_pascal_string("VialA"));
        data.extend(&make_pascal_string("cal.dat"));

        // Version >= 41: Barcode + BarcodeStatus
        data.extend(&make_pascal_string("XYZ789"));
        data.extend(&[0x02, 0x00, 0x00, 0x00]); // BarcodeStatus: 2

        // Version >= 58: ExtraUserColumns[15]
        for i in 0..15 {
            data.extend(&make_pascal_string(&format!("col{}", i)));
        }

        // Trailing marker
        data.extend(&[0xAA, 0x00, 0x00, 0x00]); // u32: 170

        let mut reader = BinaryReader::new(&data);
        skip_sequence_row(&mut reader, 58).unwrap();

        // Verify we can read trailing marker
        assert_eq!(reader.read_u32().unwrap(), 170);
    }

    #[test]
    fn test_skip_sequence_row_version_24_minimal() {
        // Version 24: only 60 bytes + 13 base strings (no version-dependent strings)
        let mut data = Vec::new();

        // 60-byte fixed struct
        data.extend(vec![0u8; 60]);

        // 13 base PascalStrings (empty)
        for _ in 0..13 {
            data.extend(&make_pascal_string(""));
        }

        // Trailing marker
        data.extend(&[0x42, 0x00, 0x00, 0x00]); // u32: 66

        let mut reader = BinaryReader::new(&data);
        skip_sequence_row(&mut reader, 24).unwrap();

        // Should have skipped exactly 60 + 13*4 = 112 bytes
        assert_eq!(reader.position(), 112);
        assert_eq!(reader.read_u32().unwrap(), 66);
    }

    #[test]
    fn test_skip_sequence_row_version_25_with_vial() {
        // Version 25: 60 bytes + 13 base + 2 extra (Vial, CalibFile), but not Barcode
        let mut data = Vec::new();

        data.extend(vec![0u8; 60]);
        for _ in 0..13 {
            data.extend(&make_pascal_string(""));
        }

        // Version >= 25
        data.extend(&make_pascal_string("V1"));
        data.extend(&make_pascal_string("C1"));

        // Trailing
        data.extend(&[0x99, 0x00, 0x00, 0x00]);

        let mut reader = BinaryReader::new(&data);
        skip_sequence_row(&mut reader, 25).unwrap();

        assert_eq!(reader.read_u32().unwrap(), 153);
    }

    #[test]
    fn test_skip_sequence_row_version_41_with_barcode() {
        // Version 41: includes Barcode but not ExtraUserColumns
        let mut data = Vec::new();

        data.extend(vec![0u8; 60]);
        for _ in 0..13 {
            data.extend(&make_pascal_string(""));
        }

        // Version >= 25
        data.extend(&make_pascal_string(""));
        data.extend(&make_pascal_string(""));

        // Version >= 41: Barcode + BarcodeStatus
        data.extend(&make_pascal_string("ABC"));
        data.extend(&[0x05, 0x00, 0x00, 0x00]); // BarcodeStatus: 5

        // Trailing
        data.extend(&[0x77, 0x00, 0x00, 0x00]);

        let mut reader = BinaryReader::new(&data);
        skip_sequence_row(&mut reader, 41).unwrap();

        assert_eq!(reader.read_u32().unwrap(), 119);
    }

    #[test]
    fn test_skip_sequence_row_insufficient_data_for_fixed_struct() {
        // Only 50 bytes available, need 60 for fixed struct
        let data = vec![0u8; 50];
        let mut reader = BinaryReader::new(&data);

        let err = skip_sequence_row(&mut reader, 57).unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("skip"));
                assert!(msg.contains("need 60 bytes"));
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_skip_sequence_row_insufficient_data_for_strings() {
        // Fixed struct OK, but not enough data for all strings
        let mut data = Vec::new();
        data.extend(vec![0u8; 60]);

        // Only 5 strings instead of 13
        for _ in 0..5 {
            data.extend(&make_pascal_string("test"));
        }

        let mut reader = BinaryReader::new(&data);

        let err = skip_sequence_row(&mut reader, 24).unwrap_err();
        assert!(err.to_string().contains("CorruptedData") || err.to_string().contains("read_i32"));
    }

    // Tests for skip_auto_sampler_config()

    #[test]
    fn test_skip_auto_sampler_config_version_35_noop() {
        // Version < 36: should be a no-op
        let data = vec![0xFF, 0x00, 0x00, 0x00]; // u32: 255
        let mut reader = BinaryReader::new(&data);

        skip_auto_sampler_config(&mut reader, 35).unwrap();

        // Position should not have changed
        assert_eq!(reader.position(), 0);
        assert_eq!(reader.read_u32().unwrap(), 255);
    }

    #[test]
    fn test_skip_auto_sampler_config_version_24_noop() {
        // Version < 36: should be a no-op
        let data = vec![0xAA, 0x00, 0x00, 0x00];
        let mut reader = BinaryReader::new(&data);

        skip_auto_sampler_config(&mut reader, 24).unwrap();

        assert_eq!(reader.position(), 0);
        assert_eq!(reader.read_u32().unwrap(), 170);
    }

    #[test]
    fn test_skip_auto_sampler_config_version_36_with_tray_name() {
        // Version 36: 24 bytes + TrayName PascalString
        let mut data = Vec::new();

        // 24-byte fixed struct (TrayIndex, VialIndex, VialsPerTray, VialsPerTrayX, VialsPerTrayY, TrayShape)
        data.extend(vec![0u8; 24]);

        // TrayName
        data.extend(&make_pascal_string("Tray96"));

        // Trailing marker
        data.extend(&[0x42, 0x00, 0x00, 0x00]);

        let mut reader = BinaryReader::new(&data);
        skip_auto_sampler_config(&mut reader, 36).unwrap();

        // Should have skipped 24 + (4 + 12) = 40 bytes
        assert_eq!(reader.position(), 40);
        assert_eq!(reader.read_u32().unwrap(), 66);
    }

    #[test]
    fn test_skip_auto_sampler_config_version_57_with_empty_tray_name() {
        // Version 57: 24 bytes + empty TrayName
        let mut data = Vec::new();

        data.extend(vec![0u8; 24]);
        data.extend(&make_pascal_string("")); // empty string

        data.extend(&[0x99, 0x00, 0x00, 0x00]);

        let mut reader = BinaryReader::new(&data);
        skip_auto_sampler_config(&mut reader, 57).unwrap();

        // Should have skipped 24 + 4 = 28 bytes
        assert_eq!(reader.position(), 28);
        assert_eq!(reader.read_u32().unwrap(), 153);
    }

    #[test]
    fn test_skip_auto_sampler_config_version_66_with_long_tray_name() {
        // Version 66: 24 bytes + long TrayName
        let mut data = Vec::new();

        data.extend(vec![0u8; 24]);
        data.extend(&make_pascal_string("VeryLongTrayNameForTesting123"));

        data.extend(&[0xFF, 0xFF, 0x00, 0x00]);

        let mut reader = BinaryReader::new(&data);
        skip_auto_sampler_config(&mut reader, 66).unwrap();

        assert_eq!(reader.read_u32().unwrap(), 65535);
    }

    #[test]
    fn test_skip_auto_sampler_config_insufficient_data_for_fixed_struct() {
        // Only 20 bytes available, need 24
        let data = vec![0u8; 20];
        let mut reader = BinaryReader::new(&data);

        let err = skip_auto_sampler_config(&mut reader, 36).unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("skip"));
                assert!(msg.contains("need 24 bytes"));
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_skip_auto_sampler_config_insufficient_data_for_tray_name() {
        // Fixed struct OK, but not enough data for TrayName
        let mut data = Vec::new();
        data.extend(vec![0u8; 24]);
        data.extend(&[0x0A, 0x00, 0x00, 0x00]); // length: 10
        data.extend(vec![0x41, 0x00]); // Only 1 char instead of 10

        let mut reader = BinaryReader::new(&data);

        let err = skip_auto_sampler_config(&mut reader, 36).unwrap_err();
        match err {
            RawError::CorruptedData(msg) => {
                assert!(msg.contains("skip"));
                assert!(msg.contains("need 20 bytes")); // 10 * 2
            }
            _ => panic!("Expected CorruptedData error, got {:?}", err),
        }
    }

    #[test]
    fn test_skip_auto_sampler_config_version_boundary() {
        // Test exactly at version boundary (35 vs 36)
        let data = vec![0x42, 0x00, 0x00, 0x00];

        // Version 35: no-op
        let mut reader = BinaryReader::new(&data);
        skip_auto_sampler_config(&mut reader, 35).unwrap();
        assert_eq!(reader.position(), 0);

        // Version 36: should skip
        let mut data_v36 = Vec::new();
        data_v36.extend(vec![0u8; 24]);
        data_v36.extend(&make_pascal_string(""));
        data_v36.extend(&[0x42, 0x00, 0x00, 0x00]);

        let mut reader_v36 = BinaryReader::new(&data_v36);
        skip_auto_sampler_config(&mut reader_v36, 36).unwrap();
        assert_eq!(reader_v36.position(), 28); // 24 + 4 (empty string)
    }

    // Integration test: skip_sequence_row + skip_auto_sampler_config

    #[test]
    fn test_skip_sequence_row_and_auto_sampler_config_v57() {
        // Simulate sequential reading for version 57
        let mut data = Vec::new();

        // SequenceRow
        data.extend(vec![0u8; 60]);
        for _ in 0..13 {
            data.extend(&make_pascal_string(""));
        }
        data.extend(&make_pascal_string("V1"));
        data.extend(&make_pascal_string("C1"));
        data.extend(&make_pascal_string("BC123"));
        data.extend(&[0x01, 0x00, 0x00, 0x00]);

        // AutoSamplerConfig
        data.extend(vec![0u8; 24]);
        data.extend(&make_pascal_string("Tray"));

        // Trailing marker (simulates RawFileInfo start)
        data.extend(&[0xFF, 0xFF, 0xFF, 0xFF]);

        let mut reader = BinaryReader::new(&data);

        // Skip both structures
        skip_sequence_row(&mut reader, 57).unwrap();
        skip_auto_sampler_config(&mut reader, 57).unwrap();

        // Verify we're at the correct position to read RawFileInfo
        assert_eq!(reader.read_u32().unwrap(), 0xFFFFFFFF);
    }

    #[test]
    fn test_skip_sequence_row_and_auto_sampler_config_v24() {
        // Version 24: minimal SequenceRow, no AutoSamplerConfig
        let mut data = Vec::new();

        // SequenceRow (minimal)
        data.extend(vec![0u8; 60]);
        for _ in 0..13 {
            data.extend(&make_pascal_string(""));
        }

        // No AutoSamplerConfig for version < 36

        // Trailing marker
        data.extend(&[0xAA, 0xBB, 0xCC, 0xDD]);

        let mut reader = BinaryReader::new(&data);

        skip_sequence_row(&mut reader, 24).unwrap();
        skip_auto_sampler_config(&mut reader, 24).unwrap(); // Should be no-op

        assert_eq!(reader.read_u32().unwrap(), 0xDDCCBBAA);
    }
}
