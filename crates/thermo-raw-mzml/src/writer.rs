//! Streaming indexed mzML XML writer.
//!
//! Writes scans one-by-one to avoid buffering all data in memory.
//! Tracks byte offsets for the indexed mzML offset index.

use crate::binary;
use crate::cv;
use crate::{Compression, MzmlConfig, MzmlError, Precision};
use quick_xml::events::{BytesDecl, BytesEnd, BytesStart, BytesText, Event};
use quick_xml::Writer;
use sha1::{Digest, Sha1};
use std::io::Write;
use thermo_raw::scan_event::{AnalyzerType, IonizationType, ScanMode};
use thermo_raw::types::MsLevel;
use thermo_raw::RawFile;

/// Byte-counting writer wrapper for tracking XML offsets.
struct CountingWriter<W: Write> {
    inner: W,
    bytes_written: u64,
    hasher: Sha1,
}

impl<W: Write> CountingWriter<W> {
    fn new(inner: W) -> Self {
        Self {
            inner,
            bytes_written: 0,
            hasher: Sha1::new(),
        }
    }

    fn position(&self) -> u64 {
        self.bytes_written
    }

    fn finish_hash(&self) -> String {
        let result = self.hasher.clone().finalize();
        hex::encode(&result)
    }

    fn into_inner(self) -> W {
        self.inner
    }
}

impl<W: Write> Write for CountingWriter<W> {
    fn write(&mut self, buf: &[u8]) -> std::io::Result<usize> {
        let n = self.inner.write(buf)?;
        self.bytes_written += n as u64;
        self.hasher.update(&buf[..n]);
        Ok(n)
    }

    fn flush(&mut self) -> std::io::Result<()> {
        self.inner.flush()
    }
}

/// Hex encoding for SHA-1 (minimal, avoids pulling in the `hex` crate).
mod hex {
    pub fn encode(bytes: &[u8]) -> String {
        let mut s = String::with_capacity(bytes.len() * 2);
        for b in bytes {
            s.push_str(&format!("{:02x}", b));
        }
        s
    }
}

/// Determine instrument configuration from scan events.
struct InstrumentInfo {
    analyzer: AnalyzerType,
    ionization: IonizationType,
}

fn detect_instrument(raw: &RawFile) -> InstrumentInfo {
    let events = raw.scan_events();
    let (analyzer, ionization) = if let Some(first) = events.first() {
        (first.preamble.analyzer, first.preamble.ionization)
    } else {
        (AnalyzerType::Ftms, IonizationType::Esi)
    };
    InstrumentInfo {
        analyzer,
        ionization,
    }
}

/// Determine if scan data includes profile mode from scan events.
fn has_profile_data(raw: &RawFile) -> bool {
    raw.scan_events()
        .iter()
        .any(|e| matches!(e.preamble.scan_mode, ScanMode::Profile))
}

/// Write a complete indexed mzML document.
pub fn write_mzml<W: Write>(
    raw: &RawFile,
    output: W,
    config: &MzmlConfig,
    source_filename: &str,
) -> Result<(), MzmlError> {
    let n_scans = raw.n_scans();
    let instrument = detect_instrument(raw);
    let _has_profile = has_profile_data(raw);

    // Count chromatograms (TIC + BPC)
    let n_chromatograms = 2u32;

    if config.write_index {
        write_indexed_mzml(
            raw,
            output,
            config,
            source_filename,
            n_scans,
            n_chromatograms,
            &instrument,
        )
    } else {
        write_plain_mzml(
            raw,
            output,
            config,
            source_filename,
            n_scans,
            n_chromatograms,
            &instrument,
        )
    }
}

fn write_plain_mzml<W: Write>(
    raw: &RawFile,
    output: W,
    config: &MzmlConfig,
    source_filename: &str,
    n_scans: u32,
    n_chromatograms: u32,
    instrument: &InstrumentInfo,
) -> Result<(), MzmlError> {
    // Use CountingWriter even in plain mode (negligible overhead, simpler code)
    let counting = CountingWriter::new(output);
    let mut writer = Writer::new_with_indent(counting, b' ', 2);

    // XML declaration
    writer.write_event(Event::Decl(BytesDecl::new("1.0", Some("utf-8"), None)))?;

    write_mzml_body(
        &mut writer,
        raw,
        config,
        source_filename,
        n_scans,
        n_chromatograms,
        instrument,
        &mut Vec::new(),
        &mut Vec::new(),
        false,
    )?;

    Ok(())
}

fn write_indexed_mzml<W: Write>(
    raw: &RawFile,
    output: W,
    config: &MzmlConfig,
    source_filename: &str,
    n_scans: u32,
    n_chromatograms: u32,
    instrument: &InstrumentInfo,
) -> Result<(), MzmlError> {
    let counting = CountingWriter::new(output);
    let mut writer = Writer::new_with_indent(counting, b' ', 2);

    // XML declaration
    writer.write_event(Event::Decl(BytesDecl::new("1.0", Some("utf-8"), None)))?;
    // Newline after declaration
    writer
        .get_mut()
        .write_all(b"\n")
        .map_err(quick_xml::Error::from)?;

    // <indexedmzML>
    let mut indexed_start = BytesStart::new("indexedmzML");
    indexed_start.push_attribute(("xmlns", "http://psi.hupo.org/ms/mzml"));
    indexed_start.push_attribute((
        "xmlns:xsi",
        "http://www.w3.org/2001/XMLSchema-instance",
    ));
    indexed_start.push_attribute((
        "xsi:schemaLocation",
        "http://psi.hupo.org/ms/mzml http://psidev.info/files/ms/mzML/xsd/mzML1.1.2_idx.xsd",
    ));
    writer.write_event(Event::Start(indexed_start))?;

    let mut spectrum_offsets: Vec<(String, u64)> = Vec::with_capacity(n_scans as usize);
    let mut chromatogram_offsets: Vec<(String, u64)> = Vec::with_capacity(n_chromatograms as usize);

    write_mzml_body(
        &mut writer,
        raw,
        config,
        source_filename,
        n_scans,
        n_chromatograms,
        instrument,
        &mut spectrum_offsets,
        &mut chromatogram_offsets,
        true,
    )?;

    // <indexList>
    let index_list_offset = writer.get_ref().position();

    let mut idx_list = BytesStart::new("indexList");
    idx_list.push_attribute(("count", "2"));
    writer.write_event(Event::Start(idx_list))?;

    // Spectrum index
    write_index(&mut writer, "spectrum", &spectrum_offsets)?;

    // Chromatogram index
    write_index(&mut writer, "chromatogram", &chromatogram_offsets)?;

    writer.write_event(Event::End(BytesEnd::new("indexList")))?;

    // <indexListOffset>
    writer.write_event(Event::Start(BytesStart::new("indexListOffset")))?;
    writer.write_event(Event::Text(BytesText::new(&index_list_offset.to_string())))?;
    writer.write_event(Event::End(BytesEnd::new("indexListOffset")))?;

    // <fileChecksum> - SHA-1 of everything written so far
    // We need to finalize the hash BEFORE writing the checksum element itself.
    // The spec says the checksum covers everything up to (but not including)
    // the <fileChecksum> element. To do this precisely, we compute the hash
    // at this point, then write it.
    let hash = writer.get_ref().finish_hash();
    writer.write_event(Event::Start(BytesStart::new("fileChecksum")))?;
    writer.write_event(Event::Text(BytesText::new(&hash)))?;
    writer.write_event(Event::End(BytesEnd::new("fileChecksum")))?;

    // </indexedmzML>
    writer.write_event(Event::End(BytesEnd::new("indexedmzML")))?;

    // Final newline
    writer
        .into_inner()
        .into_inner()
        .write_all(b"\n")
        .map_err(quick_xml::Error::from)?;

    Ok(())
}

/// Write the <index> element with offset entries.
fn write_index<W: Write>(
    writer: &mut Writer<W>,
    name: &str,
    offsets: &[(String, u64)],
) -> Result<(), MzmlError> {
    let mut idx = BytesStart::new("index");
    idx.push_attribute(("name", name));
    writer.write_event(Event::Start(idx))?;

    for (id, offset) in offsets {
        let mut elem = BytesStart::new("offset");
        elem.push_attribute(("idRef", id.as_str()));
        writer.write_event(Event::Start(elem))?;
        writer.write_event(Event::Text(BytesText::new(&offset.to_string())))?;
        writer.write_event(Event::End(BytesEnd::new("offset")))?;
    }

    writer.write_event(Event::End(BytesEnd::new("index")))?;
    Ok(())
}

/// Write the <mzML> body (shared between plain and indexed modes).
#[allow(clippy::too_many_arguments)]
fn write_mzml_body<W: Write>(
    writer: &mut Writer<CountingWriter<W>>,
    raw: &RawFile,
    config: &MzmlConfig,
    source_filename: &str,
    n_scans: u32,
    n_chromatograms: u32,
    instrument: &InstrumentInfo,
    spectrum_offsets: &mut Vec<(String, u64)>,
    chromatogram_offsets: &mut Vec<(String, u64)>,
    track_offsets: bool,
) -> Result<(), MzmlError> {
    // <mzML>
    let mut mzml_start = BytesStart::new("mzML");
    mzml_start.push_attribute(("xmlns", "http://psi.hupo.org/ms/mzml"));
    mzml_start.push_attribute((
        "xmlns:xsi",
        "http://www.w3.org/2001/XMLSchema-instance",
    ));
    mzml_start.push_attribute((
        "xsi:schemaLocation",
        "http://psi.hupo.org/ms/mzml http://psidev.info/files/ms/mzML/xsd/mzML1.1.0.xsd",
    ));
    mzml_start.push_attribute(("id", source_filename));
    writer.write_event(Event::Start(mzml_start))?;

    // <cvList>
    write_cv_list(writer)?;

    // <fileDescription>
    write_file_description(writer, source_filename)?;

    // <softwareList>
    write_software_list(writer)?;

    // <instrumentConfigurationList>
    write_instrument_config(writer, raw, instrument)?;

    // <dataProcessingList>
    write_data_processing(writer)?;

    // <run>
    let meta = raw.metadata();
    let mut run = BytesStart::new("run");
    run.push_attribute(("id", source_filename));
    run.push_attribute(("defaultInstrumentConfigurationRef", "IC1"));
    run.push_attribute(("startTimeStamp", meta.creation_date.as_str()));
    run.push_attribute(("defaultSourceFileRef", "sf1"));
    writer.write_event(Event::Start(run))?;

    // <spectrumList>
    let mut spec_list = BytesStart::new("spectrumList");
    spec_list.push_attribute(("count", n_scans.to_string().as_str()));
    spec_list.push_attribute(("defaultDataProcessingRef", "dp1"));
    writer.write_event(Event::Start(spec_list))?;

    // Write each spectrum
    for scan_num in raw.first_scan()..=raw.last_scan() {
        let scan_idx = (scan_num - raw.first_scan()) as usize;
        let spectrum_id = format!("scan={}", scan_num);

        if track_offsets {
            let offset = writer.get_ref().position();
            spectrum_offsets.push((spectrum_id.clone(), offset));
        }

        match raw.scan(scan_num) {
            Ok(scan) => {
                write_spectrum(writer, &scan, scan_idx, &spectrum_id, config)?;
            }
            Err(_) => {
                write_empty_spectrum(writer, scan_num, scan_idx, &spectrum_id)?;
            }
        }
    }

    writer.write_event(Event::End(BytesEnd::new("spectrumList")))?;

    // <chromatogramList>
    let mut chrom_list = BytesStart::new("chromatogramList");
    chrom_list.push_attribute(("count", n_chromatograms.to_string().as_str()));
    chrom_list.push_attribute(("defaultDataProcessingRef", "dp1"));
    writer.write_event(Event::Start(chrom_list))?;

    // TIC
    {
        let tic_id = "TIC";
        if track_offsets {
            let offset = writer.get_ref().position();
            chromatogram_offsets.push((tic_id.to_string(), offset));
        }
        let tic = raw.tic();
        write_chromatogram(writer, tic_id, 0, &tic.rt, &tic.intensity, cv::TIC_CHROMATOGRAM, config)?;
    }

    // BPC
    {
        let bpc_id = "BPC";
        if track_offsets {
            let offset = writer.get_ref().position();
            chromatogram_offsets.push((bpc_id.to_string(), offset));
        }
        let bpc = raw.bpc();
        write_chromatogram(writer, bpc_id, 1, &bpc.rt, &bpc.intensity, cv::BPC_CHROMATOGRAM, config)?;
    }

    writer.write_event(Event::End(BytesEnd::new("chromatogramList")))?;
    writer.write_event(Event::End(BytesEnd::new("run")))?;
    writer.write_event(Event::End(BytesEnd::new("mzML")))?;

    Ok(())
}


/// Write the <cvList> element.
fn write_cv_list<W: Write>(writer: &mut Writer<W>) -> Result<(), MzmlError> {
    let mut cv_list = BytesStart::new("cvList");
    cv_list.push_attribute(("count", "2"));
    writer.write_event(Event::Start(cv_list))?;

    let mut cv_ms = BytesStart::new("cv");
    cv_ms.push_attribute(("id", "MS"));
    cv_ms.push_attribute(("fullName", "Proteomics Standards Initiative Mass Spectrometry Ontology"));
    cv_ms.push_attribute(("version", "4.1.30"));
    cv_ms.push_attribute(("URI", "https://raw.githubusercontent.com/HUPO-PSI/psi-ms-CV/master/psi-ms.obo"));
    writer.write_event(Event::Empty(cv_ms))?;

    let mut cv_uo = BytesStart::new("cv");
    cv_uo.push_attribute(("id", "UO"));
    cv_uo.push_attribute(("fullName", "Unit Ontology"));
    cv_uo.push_attribute(("version", "09:04:2014"));
    cv_uo.push_attribute(("URI", "https://raw.githubusercontent.com/bio-ontology-research-group/unit-ontology/master/unit.obo"));
    writer.write_event(Event::Empty(cv_uo))?;

    writer.write_event(Event::End(BytesEnd::new("cvList")))?;
    Ok(())
}

/// Write the <fileDescription> element.
fn write_file_description<W: Write>(
    writer: &mut Writer<W>,
    source_filename: &str,
) -> Result<(), MzmlError> {
    writer.write_event(Event::Start(BytesStart::new("fileDescription")))?;

    // <fileContent>
    writer.write_event(Event::Start(BytesStart::new("fileContent")))?;
    write_cv_param(writer, cv::MS1_CONTENT, "MS1 spectrum", None, None)?;
    write_cv_param(writer, cv::MSN_CONTENT, "MSn spectrum", None, None)?;
    write_cv_param(writer, cv::CENTROID_CONTENT, "centroid spectrum", None, None)?;
    writer.write_event(Event::End(BytesEnd::new("fileContent")))?;

    // <sourceFileList>
    let mut sf_list = BytesStart::new("sourceFileList");
    sf_list.push_attribute(("count", "1"));
    writer.write_event(Event::Start(sf_list))?;

    let mut sf = BytesStart::new("sourceFile");
    sf.push_attribute(("id", "sf1"));
    sf.push_attribute(("name", source_filename));
    sf.push_attribute(("location", "file:///"));
    writer.write_event(Event::Start(sf))?;
    write_cv_param(writer, cv::THERMO_RAW_FORMAT, "Thermo RAW format", None, None)?;
    writer.write_event(Event::End(BytesEnd::new("sourceFile")))?;

    writer.write_event(Event::End(BytesEnd::new("sourceFileList")))?;
    writer.write_event(Event::End(BytesEnd::new("fileDescription")))?;
    Ok(())
}

/// Write the <softwareList> element.
fn write_software_list<W: Write>(writer: &mut Writer<W>) -> Result<(), MzmlError> {
    let mut sw_list = BytesStart::new("softwareList");
    sw_list.push_attribute(("count", "1"));
    writer.write_event(Event::Start(sw_list))?;

    let mut sw = BytesStart::new("software");
    sw.push_attribute(("id", "thermo-raw-mzml"));
    sw.push_attribute(("version", env!("CARGO_PKG_VERSION")));
    writer.write_event(Event::Start(sw))?;
    write_cv_param(writer, "MS:1000799", "custom unreleased software tool", Some("thermo-raw-mzml"), None)?;
    writer.write_event(Event::End(BytesEnd::new("software")))?;

    writer.write_event(Event::End(BytesEnd::new("softwareList")))?;
    Ok(())
}

/// Write the <instrumentConfigurationList> element.
fn write_instrument_config<W: Write>(
    writer: &mut Writer<W>,
    raw: &RawFile,
    instrument: &InstrumentInfo,
) -> Result<(), MzmlError> {
    let meta = raw.metadata();

    let mut ic_list = BytesStart::new("instrumentConfigurationList");
    ic_list.push_attribute(("count", "1"));
    writer.write_event(Event::Start(ic_list))?;

    let mut ic = BytesStart::new("instrumentConfiguration");
    ic.push_attribute(("id", "IC1"));
    writer.write_event(Event::Start(ic))?;

    // Instrument model
    write_cv_param(writer, cv::INSTRUMENT_MODEL, &meta.instrument_model, None, None)?;
    // Serial number
    write_cv_param(writer, cv::INSTRUMENT_SERIAL, "instrument serial number", Some(&meta.serial_number), None)?;

    // <componentList>
    let mut comp_list = BytesStart::new("componentList");
    comp_list.push_attribute(("count", "3"));
    writer.write_event(Event::Start(comp_list))?;

    // Source
    let mut source = BytesStart::new("source");
    source.push_attribute(("order", "1"));
    writer.write_event(Event::Start(source))?;
    write_cv_param(
        writer,
        cv::ionization_accession(&instrument.ionization),
        cv::ionization_name(&instrument.ionization),
        None,
        None,
    )?;
    writer.write_event(Event::End(BytesEnd::new("source")))?;

    // Analyzer
    let mut analyzer = BytesStart::new("analyzer");
    analyzer.push_attribute(("order", "2"));
    writer.write_event(Event::Start(analyzer))?;
    write_cv_param(
        writer,
        cv::analyzer_accession(&instrument.analyzer),
        cv::analyzer_name(&instrument.analyzer),
        None,
        None,
    )?;
    writer.write_event(Event::End(BytesEnd::new("analyzer")))?;

    // Detector
    let mut detector = BytesStart::new("detector");
    detector.push_attribute(("order", "3"));
    writer.write_event(Event::Start(detector))?;
    write_cv_param(writer, "MS:1000624", "inductive detector", None, None)?;
    writer.write_event(Event::End(BytesEnd::new("detector")))?;

    writer.write_event(Event::End(BytesEnd::new("componentList")))?;
    writer.write_event(Event::End(BytesEnd::new("instrumentConfiguration")))?;
    writer.write_event(Event::End(BytesEnd::new("instrumentConfigurationList")))?;
    Ok(())
}

/// Write the <dataProcessingList> element.
fn write_data_processing<W: Write>(writer: &mut Writer<W>) -> Result<(), MzmlError> {
    let mut dp_list = BytesStart::new("dataProcessingList");
    dp_list.push_attribute(("count", "1"));
    writer.write_event(Event::Start(dp_list))?;

    let mut dp = BytesStart::new("dataProcessing");
    dp.push_attribute(("id", "dp1"));
    writer.write_event(Event::Start(dp))?;

    let mut pm = BytesStart::new("processingMethod");
    pm.push_attribute(("order", "1"));
    pm.push_attribute(("softwareRef", "thermo-raw-mzml"));
    writer.write_event(Event::Start(pm))?;
    write_cv_param(writer, cv::CONVERSION_TO_MZML, "Conversion to mzML", None, None)?;
    writer.write_event(Event::End(BytesEnd::new("processingMethod")))?;

    writer.write_event(Event::End(BytesEnd::new("dataProcessing")))?;
    writer.write_event(Event::End(BytesEnd::new("dataProcessingList")))?;
    Ok(())
}

/// Write a single <spectrum> element.
fn write_spectrum<W: Write>(
    writer: &mut Writer<W>,
    scan: &thermo_raw::Scan,
    index: usize,
    spectrum_id: &str,
    config: &MzmlConfig,
) -> Result<(), MzmlError> {
    let n_peaks = scan.centroid_mz.len();
    let default_array_length = n_peaks.to_string();

    let mut spec = BytesStart::new("spectrum");
    spec.push_attribute(("index", index.to_string().as_str()));
    spec.push_attribute(("id", spectrum_id));
    spec.push_attribute(("defaultArrayLength", default_array_length.as_str()));
    writer.write_event(Event::Start(spec))?;

    // Spectrum type CV params
    write_cv_param(writer, cv::spectrum_type(&scan.ms_level), "", None, None)?;
    write_cv_param(writer, cv::MS_LEVEL, "ms level", Some(cv::ms_level_value(&scan.ms_level)), None)?;
    write_cv_param(writer, cv::CENTROID_SPECTRUM, "centroid spectrum", None, None)?;

    // Polarity
    if let Some(pol_acc) = cv::polarity_accession(&scan.polarity) {
        write_cv_param(writer, pol_acc, "", None, None)?;
    }

    // Base peak and TIC
    write_cv_param(writer, cv::BASE_PEAK_MZ, "base peak m/z", Some(&format!("{:.10}", scan.base_peak_mz)), None)?;
    write_cv_param(writer, cv::BASE_PEAK_INTENSITY, "base peak intensity", Some(&format!("{:.4}", scan.base_peak_intensity)), None)?;
    write_cv_param(writer, cv::TOTAL_ION_CURRENT, "total ion current", Some(&format!("{:.4}", scan.tic)), None)?;

    // m/z range
    if !scan.centroid_mz.is_empty() {
        let low = scan.centroid_mz.first().unwrap();
        let high = scan.centroid_mz.last().unwrap();
        write_cv_param(writer, cv::LOWEST_MZ, "lowest observed m/z", Some(&format!("{:.10}", low)), None)?;
        write_cv_param(writer, cv::HIGHEST_MZ, "highest observed m/z", Some(&format!("{:.10}", high)), None)?;
    }

    // <scanList>
    let mut scan_list = BytesStart::new("scanList");
    scan_list.push_attribute(("count", "1"));
    writer.write_event(Event::Start(scan_list))?;

    writer.write_event(Event::Start(BytesStart::new("scan")))?;
    write_cv_param_with_unit(
        writer,
        cv::SCAN_START_TIME,
        "scan start time",
        &format!("{:.10}", scan.rt),
        cv::MINUTE,
        "minute",
    )?;
    // Filter string as userParam
    if let Some(ref filter) = scan.filter_string {
        let mut up = BytesStart::new("userParam");
        up.push_attribute(("name", "filter string"));
        up.push_attribute(("value", filter.as_str()));
        up.push_attribute(("type", "xsd:string"));
        writer.write_event(Event::Empty(up))?;
    }
    writer.write_event(Event::End(BytesEnd::new("scan")))?;
    writer.write_event(Event::End(BytesEnd::new("scanList")))?;

    // <precursorList> for MS2+ scans
    if !matches!(scan.ms_level, MsLevel::Ms1) {
        if let Some(ref precursor) = scan.precursor {
            write_precursor(writer, precursor, scan.scan_number)?;
        }
    }

    // <binaryDataArrayList>
    write_binary_data_arrays(writer, &scan.centroid_mz, &scan.centroid_intensity, config)?;

    writer.write_event(Event::End(BytesEnd::new("spectrum")))?;
    Ok(())
}

/// Write an empty spectrum for scans that failed to decode.
fn write_empty_spectrum<W: Write>(
    writer: &mut Writer<W>,
    scan_num: u32,
    index: usize,
    spectrum_id: &str,
) -> Result<(), MzmlError> {
    let mut spec = BytesStart::new("spectrum");
    spec.push_attribute(("index", index.to_string().as_str()));
    spec.push_attribute(("id", spectrum_id));
    spec.push_attribute(("defaultArrayLength", "0"));
    writer.write_event(Event::Start(spec))?;

    write_cv_param(writer, cv::MS1_SPECTRUM, "MS1 spectrum", None, None)?;
    write_cv_param(writer, cv::MS_LEVEL, "ms level", Some("1"), None)?;
    write_cv_param(writer, cv::CENTROID_SPECTRUM, "centroid spectrum", None, None)?;

    // userParam noting the error
    let mut up = BytesStart::new("userParam");
    up.push_attribute(("name", "decode error"));
    up.push_attribute(("value", format!("Failed to decode scan {}", scan_num).as_str()));
    writer.write_event(Event::Empty(up))?;

    // Empty binary data arrays
    let mut bdal = BytesStart::new("binaryDataArrayList");
    bdal.push_attribute(("count", "2"));
    writer.write_event(Event::Start(bdal))?;
    write_empty_binary_array(writer, cv::MZ_ARRAY, "m/z array", cv::FLOAT_64)?;
    write_empty_binary_array(writer, cv::INTENSITY_ARRAY, "intensity array", cv::FLOAT_32)?;
    writer.write_event(Event::End(BytesEnd::new("binaryDataArrayList")))?;

    writer.write_event(Event::End(BytesEnd::new("spectrum")))?;
    Ok(())
}

/// Write precursor information for MS2+ scans.
fn write_precursor<W: Write>(
    writer: &mut Writer<W>,
    precursor: &thermo_raw::PrecursorInfo,
    _scan_number: u32,
) -> Result<(), MzmlError> {
    let mut pl = BytesStart::new("precursorList");
    pl.push_attribute(("count", "1"));
    writer.write_event(Event::Start(pl))?;

    writer.write_event(Event::Start(BytesStart::new("precursor")))?;

    // <isolationWindow>
    writer.write_event(Event::Start(BytesStart::new("isolationWindow")))?;
    write_cv_param(
        writer,
        cv::ISOLATION_WINDOW_TARGET,
        "isolation window target m/z",
        Some(&format!("{:.10}", precursor.mz)),
        None,
    )?;
    if let Some(width) = precursor.isolation_width {
        let half = width / 2.0;
        write_cv_param(
            writer,
            cv::ISOLATION_WINDOW_LOWER,
            "isolation window lower offset",
            Some(&format!("{:.6}", half)),
            None,
        )?;
        write_cv_param(
            writer,
            cv::ISOLATION_WINDOW_UPPER,
            "isolation window upper offset",
            Some(&format!("{:.6}", half)),
            None,
        )?;
    }
    writer.write_event(Event::End(BytesEnd::new("isolationWindow")))?;

    // <selectedIonList>
    let mut sil = BytesStart::new("selectedIonList");
    sil.push_attribute(("count", "1"));
    writer.write_event(Event::Start(sil))?;

    writer.write_event(Event::Start(BytesStart::new("selectedIon")))?;
    write_cv_param(
        writer,
        cv::SELECTED_ION_MZ,
        "selected ion m/z",
        Some(&format!("{:.10}", precursor.mz)),
        None,
    )?;
    if let Some(charge) = precursor.charge {
        write_cv_param(
            writer,
            cv::CHARGE_STATE,
            "charge state",
            Some(&charge.to_string()),
            None,
        )?;
    }
    writer.write_event(Event::End(BytesEnd::new("selectedIon")))?;
    writer.write_event(Event::End(BytesEnd::new("selectedIonList")))?;

    // <activation>
    writer.write_event(Event::Start(BytesStart::new("activation")))?;
    if let Some(ref act_type) = precursor.activation_type {
        if let Some(acc) = cv::activation_str_to_accession(act_type) {
            write_cv_param(writer, acc, act_type, None, None)?;
        }
    }
    if let Some(ce) = precursor.collision_energy {
        write_cv_param(
            writer,
            cv::COLLISION_ENERGY,
            "collision energy",
            Some(&format!("{:.4}", ce)),
            None,
        )?;
    }
    writer.write_event(Event::End(BytesEnd::new("activation")))?;

    writer.write_event(Event::End(BytesEnd::new("precursor")))?;
    writer.write_event(Event::End(BytesEnd::new("precursorList")))?;
    Ok(())
}

/// Write binary data arrays (m/z + intensity).
fn write_binary_data_arrays<W: Write>(
    writer: &mut Writer<W>,
    mz: &[f64],
    intensity: &[f64],
    config: &MzmlConfig,
) -> Result<(), MzmlError> {
    let mut bdal = BytesStart::new("binaryDataArrayList");
    bdal.push_attribute(("count", "2"));
    writer.write_event(Event::Start(bdal))?;

    // m/z array
    write_binary_array(
        writer,
        mz,
        config.mz_precision,
        config.compression,
        cv::MZ_ARRAY,
        "m/z array",
    )?;

    // Intensity array
    write_binary_array(
        writer,
        intensity,
        config.intensity_precision,
        config.compression,
        cv::INTENSITY_ARRAY,
        "intensity array",
    )?;

    writer.write_event(Event::End(BytesEnd::new("binaryDataArrayList")))?;
    Ok(())
}

/// Write a single binary data array element.
fn write_binary_array<W: Write>(
    writer: &mut Writer<W>,
    data: &[f64],
    precision: Precision,
    compression: Compression,
    array_accession: &str,
    array_name: &str,
) -> Result<(), MzmlError> {
    let encoded = binary::encode_array(data, precision, compression);
    let encoded_length = base64::engine::general_purpose::STANDARD
        .decode(&encoded)
        .map(|v| v.len())
        .unwrap_or(0);

    let mut bda = BytesStart::new("binaryDataArray");
    bda.push_attribute(("encodedLength", encoded_length.to_string().as_str()));
    writer.write_event(Event::Start(bda))?;

    // Precision CV param
    let (prec_acc, prec_name) = match precision {
        Precision::F64 => (cv::FLOAT_64, "64-bit float"),
        Precision::F32 => (cv::FLOAT_32, "32-bit float"),
    };
    write_cv_param(writer, prec_acc, prec_name, None, None)?;

    // Compression CV param
    let (comp_acc, comp_name) = match compression {
        Compression::Zlib => (cv::ZLIB_COMPRESSION, "zlib compression"),
        Compression::None => (cv::NO_COMPRESSION, "no compression"),
    };
    write_cv_param(writer, comp_acc, comp_name, None, None)?;

    // Array type
    write_cv_param(writer, array_accession, array_name, None, None)?;

    // <binary>
    writer.write_event(Event::Start(BytesStart::new("binary")))?;
    writer.write_event(Event::Text(BytesText::new(&encoded)))?;
    writer.write_event(Event::End(BytesEnd::new("binary")))?;

    writer.write_event(Event::End(BytesEnd::new("binaryDataArray")))?;
    Ok(())
}

use base64::Engine;

/// Write an empty binary data array for error scans.
fn write_empty_binary_array<W: Write>(
    writer: &mut Writer<W>,
    array_accession: &str,
    array_name: &str,
    precision_accession: &str,
) -> Result<(), MzmlError> {
    let mut bda = BytesStart::new("binaryDataArray");
    bda.push_attribute(("encodedLength", "0"));
    writer.write_event(Event::Start(bda))?;

    write_cv_param(
        writer,
        precision_accession,
        if precision_accession == cv::FLOAT_64 { "64-bit float" } else { "32-bit float" },
        None,
        None,
    )?;
    write_cv_param(writer, cv::NO_COMPRESSION, "no compression", None, None)?;
    write_cv_param(writer, array_accession, array_name, None, None)?;

    writer.write_event(Event::Start(BytesStart::new("binary")))?;
    writer.write_event(Event::End(BytesEnd::new("binary")))?;

    writer.write_event(Event::End(BytesEnd::new("binaryDataArray")))?;
    Ok(())
}

/// Write a chromatogram element (TIC or BPC).
fn write_chromatogram<W: Write>(
    writer: &mut Writer<W>,
    id: &str,
    index: usize,
    rt: &[f64],
    intensity: &[f64],
    type_accession: &str,
    config: &MzmlConfig,
) -> Result<(), MzmlError> {
    let n_points = rt.len().to_string();

    let mut chrom = BytesStart::new("chromatogram");
    chrom.push_attribute(("index", index.to_string().as_str()));
    chrom.push_attribute(("id", id));
    chrom.push_attribute(("defaultArrayLength", n_points.as_str()));
    writer.write_event(Event::Start(chrom))?;

    write_cv_param(writer, type_accession, "", None, None)?;

    // Binary data arrays: time + intensity
    let mut bdal = BytesStart::new("binaryDataArrayList");
    bdal.push_attribute(("count", "2"));
    writer.write_event(Event::Start(bdal))?;

    // Time array
    write_binary_array(
        writer,
        rt,
        Precision::F64,
        config.compression,
        cv::TIME_ARRAY,
        "time array",
    )?;

    // Intensity array
    write_binary_array(
        writer,
        intensity,
        config.intensity_precision,
        config.compression,
        cv::INTENSITY_ARRAY,
        "intensity array",
    )?;

    writer.write_event(Event::End(BytesEnd::new("binaryDataArrayList")))?;
    writer.write_event(Event::End(BytesEnd::new("chromatogram")))?;
    Ok(())
}

/// Helper to write a <cvParam> element.
fn write_cv_param<W: Write>(
    writer: &mut Writer<W>,
    accession: &str,
    name: &str,
    value: Option<&str>,
    _unit: Option<&str>,
) -> Result<(), MzmlError> {
    let mut param = BytesStart::new("cvParam");
    param.push_attribute(("cvRef", cv::CV_MS));
    param.push_attribute(("accession", accession));
    param.push_attribute(("name", name));
    if let Some(v) = value {
        param.push_attribute(("value", v));
    }
    writer.write_event(Event::Empty(param))?;
    Ok(())
}

/// Helper to write a <cvParam> with unit reference.
fn write_cv_param_with_unit<W: Write>(
    writer: &mut Writer<W>,
    accession: &str,
    name: &str,
    value: &str,
    unit_accession: &str,
    unit_name: &str,
) -> Result<(), MzmlError> {
    let mut param = BytesStart::new("cvParam");
    param.push_attribute(("cvRef", cv::CV_MS));
    param.push_attribute(("accession", accession));
    param.push_attribute(("name", name));
    param.push_attribute(("value", value));
    param.push_attribute(("unitCvRef", cv::CV_UO));
    param.push_attribute(("unitAccession", unit_accession));
    param.push_attribute(("unitName", unit_name));
    writer.write_event(Event::Empty(param))?;
    Ok(())
}
