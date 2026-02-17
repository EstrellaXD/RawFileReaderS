use clap::{Parser, Subcommand};
use indicatif::{ProgressBar, ProgressStyle};
use std::path::PathBuf;
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::Arc;
use thermo_raw::RawFile;

#[derive(Parser)]
#[command(name = "thermo-raw", about = "Thermo RAW file reader CLI")]
struct Cli {
    #[command(subcommand)]
    command: Commands,
}

#[derive(Subcommand)]
enum Commands {
    /// Show basic RAW file information.
    Info { file: PathBuf },

    /// List OLE2 container streams.
    Streams { file: PathBuf },

    /// Export a single scan as JSON.
    Scan {
        file: PathBuf,
        #[arg(short, long)]
        number: u32,
    },

    /// Export TIC as CSV.
    Tic {
        file: PathBuf,
        #[arg(short, long)]
        output: Option<PathBuf>,
    },

    /// Export XIC as CSV.
    ///
    /// Supports single or multiple target m/z values. Use --ms1-only to restrict
    /// to MS1 scans (much faster for DDA data). Multiple targets in one call
    /// share a single scan pass for better performance.
    Xic {
        file: PathBuf,
        /// Target m/z value(s). Repeat for batch XIC: --mz 524.26 --mz 445.12
        #[arg(short, long, required = true, num_args = 1..)]
        mz: Vec<f64>,
        #[arg(short, long, default_value = "5.0")]
        ppm: f64,
        /// Only include MS1 scans (skips MS2+ for faster extraction).
        #[arg(long)]
        ms1_only: bool,
        #[arg(short, long)]
        output: Option<PathBuf>,
    },

    /// Show trailer extra data for a scan.
    Trailer {
        file: PathBuf,
        #[arg(short, long)]
        number: u32,
    },

    /// Validate against ground truth data.
    Validate {
        file: PathBuf,
        #[arg(short, long)]
        truth_dir: PathBuf,
    },

    /// Benchmark: read all scans (performance test).
    Benchmark {
        file: PathBuf,
        #[arg(long)]
        parallel: bool,
        #[arg(long)]
        mmap: bool,
        /// Also benchmark XIC extraction (internally timed).
        #[arg(long)]
        xic: bool,
    },

    /// Debug: dump internal addresses and sanity checks.
    Debug { file: PathBuf },

    /// Diagnose: stage-by-stage parsing report (non-cascading).
    Diagnose { file: PathBuf },

    /// Convert RAW file(s) to mzML format.
    Convert {
        /// Input RAW file or folder of RAW files.
        input: PathBuf,
        /// Output path (file for single, directory for folder conversion).
        #[arg(short, long)]
        output: Option<PathBuf>,
        /// m/z precision: 32 or 64 (default: 64).
        #[arg(long, default_value = "64")]
        mz_bits: u8,
        /// Intensity precision: 32 or 64 (default: 32).
        #[arg(long, default_value = "32")]
        intensity_bits: u8,
        /// Compression: none, zlib (default).
        #[arg(long, default_value = "zlib")]
        compression: String,
        /// Skip index generation (plain mzML instead of indexed).
        #[arg(long)]
        no_index: bool,
    },

    /// Batch EIC extraction across multiple RAW files.
    ///
    /// Extracts chromatograms for target m/z values from multiple files,
    /// aligns them to a common RT grid, and outputs a CSV tensor.
    BatchXic {
        /// RAW files to process.
        #[arg(short, long, required = true, num_args = 1..)]
        files: Vec<PathBuf>,
        /// Target m/z value(s). Repeat for multiple: --mz 524.26 --mz 445.12
        #[arg(short, long, required = true, num_args = 1..)]
        mz: Vec<f64>,
        /// Mass tolerance in ppm.
        #[arg(short, long, default_value = "5.0")]
        ppm: f64,
        /// RT grid resolution in minutes. Default: auto (matches native scan density).
        #[arg(long, default_value = "0")]
        rt_resolution: f64,
        /// Optional RT range (start,end in minutes). Format: "1.5,30.0"
        #[arg(long)]
        rt_range: Option<String>,
        /// Output file (CSV). Defaults to stdout.
        #[arg(short, long)]
        output: Option<PathBuf>,
    },
}

fn ms_level_str(level: &thermo_raw::MsLevel) -> &'static str {
    match level {
        thermo_raw::MsLevel::Ms1 => "MS1",
        thermo_raw::MsLevel::Ms2 => "MS2",
        thermo_raw::MsLevel::Ms3 => "MS3",
        thermo_raw::MsLevel::Other(_) => "Other",
    }
}

fn polarity_str(p: &thermo_raw::Polarity) -> &'static str {
    match p {
        thermo_raw::Polarity::Positive => "Positive",
        thermo_raw::Polarity::Negative => "Negative",
        thermo_raw::Polarity::Unknown => "Unknown",
    }
}

/// Spawn a progress bar backed by an atomic counter.
///
/// Returns `(counter, done_flag)`. The caller increments `counter` from worker
/// threads; a background thread polls it every 50ms to update the bar. Set
/// `done_flag` to `true` and join the returned handle to finish cleanly.
fn spawn_progress_bar(
    total: u64,
    msg: &str,
) -> (
    thermo_raw::ProgressCounter,
    Arc<AtomicBool>,
    std::thread::JoinHandle<()>,
) {
    let counter = thermo_raw::new_counter();
    let done = Arc::new(AtomicBool::new(false));

    let bar = ProgressBar::new(total);
    bar.set_style(
        ProgressStyle::with_template("{msg} [{bar:40.cyan/blue}] {pos}/{len} ({eta})")
            .unwrap()
            .progress_chars("=> "),
    );
    bar.set_message(msg.to_string());

    let counter_clone = Arc::clone(&counter);
    let done_clone = Arc::clone(&done);
    let handle = std::thread::spawn(move || {
        while !done_clone.load(Ordering::Relaxed) {
            bar.set_position(counter_clone.load(Ordering::Relaxed));
            std::thread::sleep(std::time::Duration::from_millis(50));
        }
        bar.set_position(counter_clone.load(Ordering::Relaxed));
        bar.finish();
    });

    (counter, done, handle)
}

fn main() -> anyhow::Result<()> {
    let cli = Cli::parse();
    match cli.command {
        Commands::Info { file } => {
            let raw = RawFile::open(&file)?;
            let meta = raw.metadata();
            println!("File:        {}", file.display());
            println!("Version:     {}", raw.version());
            println!("Instrument:  {}", meta.instrument_model);
            println!("Serial:      {}", meta.serial_number);
            println!("Software:    {}", meta.software_version);
            println!("Sample:      {}", meta.sample_name);
            println!("Created:     {}", meta.creation_date);
            println!(
                "Scans:       {}-{} ({} total)",
                raw.first_scan(),
                raw.last_scan(),
                raw.n_scans()
            );
            println!(
                "RT range:    {:.4}-{:.4} min",
                raw.start_time(),
                raw.end_time()
            );
            println!(
                "Mass range:  {:.2}-{:.2} Da",
                raw.low_mass(),
                raw.high_mass()
            );

            // Show trailer field names
            let fields = raw.trailer_fields();
            if !fields.is_empty() {
                println!("Trailer fields ({}):", fields.len());
                for f in &fields {
                    println!("  {}", f);
                }
            }
        }

        Commands::Streams { file } => {
            let container = cfb_reader::Ole2Container::open(&file)?;
            let streams = container.list_streams();
            println!("OLE2 streams in {}:", file.display());
            for s in &streams {
                println!("  {}", s);
            }
        }

        Commands::Scan { file, number } => {
            let raw = RawFile::open(&file)?;
            let scan = raw.scan(number)?;

            let precursor_json = if let Some(ref p) = scan.precursor {
                serde_json::json!({
                    "mz": p.mz,
                    "charge": p.charge,
                    "isolationWidth": p.isolation_width,
                    "activationType": p.activation_type,
                    "collisionEnergy": p.collision_energy,
                })
            } else {
                serde_json::Value::Null
            };

            let json = serde_json::json!({
                "scanNumber": scan.scan_number,
                "rt": scan.rt,
                "msLevel": ms_level_str(&scan.ms_level),
                "polarity": polarity_str(&scan.polarity),
                "filterString": scan.filter_string,
                "tic": scan.tic,
                "basePeakMz": scan.base_peak_mz,
                "basePeakIntensity": scan.base_peak_intensity,
                "precursor": precursor_json,
                "centroidCount": scan.centroid_mz.len(),
                "centroidMz": scan.centroid_mz,
                "centroidIntensity": scan.centroid_intensity,
            });
            println!("{}", serde_json::to_string_pretty(&json)?);
        }

        Commands::Tic { file, output } => {
            let raw = RawFile::open(&file)?;
            let chrom = raw.tic();
            let mut writer: Box<dyn std::io::Write> = if let Some(path) = output {
                Box::new(std::fs::File::create(path)?)
            } else {
                Box::new(std::io::stdout())
            };
            writeln!(writer, "rt,intensity")?;
            for (rt, int) in chrom.rt.iter().zip(chrom.intensity.iter()) {
                writeln!(writer, "{:.6},{:.2}", rt, int)?;
            }
        }

        Commands::Xic {
            file,
            mz,
            ppm,
            ms1_only,
            output,
        } => {
            let raw = RawFile::open(&file)?;
            let mut writer: Box<dyn std::io::Write> = if let Some(path) = output {
                Box::new(std::fs::File::create(path)?)
            } else {
                Box::new(std::io::stdout())
            };

            if mz.len() == 1 {
                // Single target
                let chrom = if ms1_only {
                    raw.xic_ms1(mz[0], ppm)?
                } else {
                    raw.xic(mz[0], ppm)?
                };
                writeln!(writer, "rt,intensity")?;
                for (rt, int) in chrom.rt.iter().zip(chrom.intensity.iter()) {
                    writeln!(writer, "{:.6},{:.2}", rt, int)?;
                }
            } else {
                // Batch: multiple targets in a single scan pass
                let targets: Vec<(f64, f64)> = mz.iter().map(|&m| (m, ppm)).collect();
                let chroms = if ms1_only {
                    raw.xic_batch_ms1(&targets)?
                } else {
                    // For non-MS1 batch, run individually (batch_ms1 is the optimized path)
                    targets
                        .iter()
                        .map(|&(m, p)| raw.xic(m, p))
                        .collect::<Result<Vec<_>, _>>()?
                };

                // CSV header: rt, then one column per target m/z
                let header: Vec<String> = std::iter::once("rt".to_string())
                    .chain(mz.iter().map(|m| format!("mz_{:.4}", m)))
                    .collect();
                writeln!(writer, "{}", header.join(","))?;

                // All chromatograms share the same RT axis (from MS1 scans)
                if let Some(first) = chroms.first() {
                    for i in 0..first.rt.len() {
                        write!(writer, "{:.6}", first.rt[i])?;
                        for chrom in &chroms {
                            // Normalize -0.0 to 0.0 for clean output
                            let v = chrom.intensity[i] + 0.0;
                            write!(writer, ",{:.2}", v)?;
                        }
                        writeln!(writer)?;
                    }
                }
            }
        }

        Commands::Trailer { file, number } => {
            let raw = RawFile::open(&file)?;
            let trailer = raw.trailer_extra(number)?;
            println!("Trailer extra for scan {}:", number);
            let mut keys: Vec<_> = trailer.keys().collect();
            keys.sort();
            for key in keys {
                println!("  {}: {}", key, trailer[key]);
            }
        }

        Commands::Validate { file, truth_dir } => {
            let raw = RawFile::open(&file)?;
            let criteria = thermo_raw::validation::ValidationCriteria::default();
            let report = thermo_raw::validation::validate_file(&raw, &truth_dir, &criteria)?;

            println!("Validation Report");
            println!("=================");
            println!("Total scans:  {}", report.total_scans);
            println!(
                "Passed:       {} ({:.1}%)",
                report.passed_scans,
                report.pass_rate * 100.0
            );
            println!("Failed:       {}", report.failed_scans);
            println!("Worst m/z error: {:.4} ppm", report.worst_mz_error_ppm);
            println!(
                "Worst intensity error: {:.2e}",
                report.worst_intensity_error
            );

            if !report.failures.is_empty() {
                println!("\nFailed scans:");
                for fail in &report.failures {
                    println!(
                        "  Scan {}: mz_err={:.4}ppm rt_err={:.4}s peaks_match={}",
                        fail.scan_number,
                        fail.mz_max_error_ppm,
                        fail.rt_error_seconds,
                        fail.peak_count_match
                    );
                    for err in &fail.errors {
                        println!("    {}", err);
                    }
                }
            }
        }

        Commands::Debug { file } => {
            let raw = RawFile::open(&file)?;
            let info = raw.debug_info();

            println!("=== Debug Info: {} ===", file.display());
            println!(
                "File size:       {} bytes ({:.1} MB)",
                info.file_size,
                info.file_size as f64 / 1e6
            );
            println!("Version:         {}", info.version);
            println!("RunHeader start: {}", info.run_header_start);
            println!();

            println!("--- 32-bit addresses ---");
            println!("  ScanIndex:     {}", info.scan_index_addr_32);
            println!("  DataStream:    {}", info.data_addr_32);
            println!("  TrailerExtra:  {}", info.scan_trailer_addr_32);
            println!("  ScanParams:    {}", info.scan_params_addr_32);
            println!();

            if let Some(addr) = info.scan_index_addr_64 {
                println!("--- 64-bit addresses ---");
                println!("  ScanIndex:     {} (SpectPos)", addr);
                println!(
                    "  DataStream:    {} (PacketPos)",
                    info.data_addr_64.unwrap_or(0)
                );
                println!(
                    "  TrailerExtra:  {} (TrailerScanEventsPos)",
                    info.scan_trailer_addr_64.unwrap_or(0)
                );
                println!(
                    "  ScanParams:    {} (TrailerExtraPos)",
                    info.scan_params_addr_64.unwrap_or(0)
                );

                // Validate: all addresses should be < file_size
                let addrs = [
                    addr,
                    info.data_addr_64.unwrap_or(0),
                    info.scan_trailer_addr_64.unwrap_or(0),
                    info.scan_params_addr_64.unwrap_or(0),
                ];
                let all_valid = addrs.iter().all(|&a| a > 0 && a < info.file_size);
                println!(
                    "  All valid:     {}",
                    if all_valid {
                        "YES"
                    } else {
                        "NO -- addresses exceed file size!"
                    }
                );
                println!();
            }

            println!(
                "Effective data_addr: {} (used for scan offset computation)",
                info.effective_data_addr
            );
            println!("Instrument type: {}", info.instrument_type);
            println!(
                "Scans: {}, Scan events: {}",
                info.n_scans, info.n_scan_events
            );
            println!();

            println!("--- First scan index entries ---");
            for (i, e) in info.first_scan_entries.iter().enumerate() {
                println!(
                    "  [{}] offset={}, data_size={}, rt={:.4}, tic={:.2e}, packet_type=0x{:08X}",
                    i, e.offset, e.data_size, e.rt, e.tic, e.packet_type
                );
                let abs_offset = info.effective_data_addr + e.offset;
                println!(
                    "       abs_offset={} (within file: {})",
                    abs_offset,
                    if abs_offset < info.file_size {
                        "YES"
                    } else {
                        "NO"
                    }
                );
            }

            // Try reading scan 1
            println!();
            let first = raw.first_scan();
            match raw.scan(first) {
                Ok(scan) => {
                    println!(
                        "Scan {} decoded OK: {} centroids, tic={:.2e}",
                        first,
                        scan.centroid_mz.len(),
                        scan.tic
                    );
                    if !scan.centroid_mz.is_empty() {
                        println!(
                            "  First m/z: {:.6}, last m/z: {:.6}",
                            scan.centroid_mz[0],
                            scan.centroid_mz[scan.centroid_mz.len() - 1]
                        );
                    }
                }
                Err(e) => {
                    println!("Scan {} FAILED: {}", first, e);
                }
            }
        }

        Commands::BatchXic {
            files,
            mz,
            ppm,
            rt_resolution,
            rt_range,
            output,
        } => {
            let targets: Vec<(f64, f64)> = mz.iter().map(|&m| (m, ppm)).collect();
            let paths: Vec<&std::path::Path> = files.iter().map(|p| p.as_path()).collect();

            let rt_range_parsed = rt_range.as_ref().and_then(|s| {
                let parts: Vec<&str> = s.split(',').collect();
                if parts.len() == 2 {
                    Some((parts[0].parse::<f64>().ok()?, parts[1].parse::<f64>().ok()?))
                } else {
                    None
                }
            });

            // For single-file, use direct XIC (no batch wrapper overhead)
            if files.len() == 1 {
                let raw = thermo_raw::RawFile::open(&files[0])?;
                let chroms = raw.xic_batch_ms1(&targets)?;

                let mut writer: Box<dyn std::io::Write> = if let Some(path) = output {
                    Box::new(std::fs::File::create(path)?)
                } else {
                    Box::new(std::io::stdout())
                };

                let sample_name = files[0]
                    .file_stem()
                    .map(|s| s.to_string_lossy().to_string())
                    .unwrap_or_else(|| "unknown".to_string());

                // CSV header
                let mut header = vec!["rt".to_string()];
                for m in &mz {
                    header.push(format!("{}_{:.4}", sample_name, m));
                }
                writeln!(writer, "{}", header.join(","))?;

                // Data rows (native RT points, no interpolation)
                let n_points = chroms[0].rt.len();
                for i in 0..n_points {
                    write!(writer, "{:.6}", chroms[0].rt[i])?;
                    for chrom in &chroms {
                        let val = chrom.intensity[i] + 0.0; // normalize -0.0
                        write!(writer, ",{:.2}", val)?;
                    }
                    writeln!(writer)?;
                }

            } else {

            let n_files = files.len() as u64;
            let (counter, done, handle) = spawn_progress_bar(n_files, "Batch XIC");
            let start = std::time::Instant::now();
            let result = thermo_raw::batch_xic_ms1_with_progress(
                &paths,
                &targets,
                rt_range_parsed,
                rt_resolution,
                &counter,
            )?;
            done.store(true, Ordering::Relaxed);
            handle.join().unwrap();
            let elapsed = start.elapsed();

            eprintln!(
                "Batch XIC: {} samples x {} targets x {} timepoints in {:.1}ms",
                result.n_samples,
                result.n_targets,
                result.n_timepoints,
                elapsed.as_secs_f64() * 1000.0,
            );

            let mut writer: Box<dyn std::io::Write> = if let Some(path) = output {
                Box::new(std::fs::File::create(path)?)
            } else {
                Box::new(std::io::stdout())
            };

            // CSV header: rt, then sample_mz columns
            let mut header = vec!["rt".to_string()];
            for name in &result.sample_names {
                for m in &mz {
                    header.push(format!("{}_{:.4}", name, m));
                }
            }
            writeln!(writer, "{}", header.join(","))?;

            // Data rows
            for (i, &rt) in result.rt_grid.iter().enumerate() {
                write!(writer, "{:.6}", rt)?;
                for s in 0..result.n_samples {
                    for t in 0..result.n_targets {
                        let val = result.get(s, t)[i] + 0.0; // normalize -0.0
                        write!(writer, ",{:.2}", val)?;
                    }
                }
                writeln!(writer)?;
            }
            } // else (multi-file)
        }

        Commands::Convert {
            input,
            output,
            mz_bits,
            intensity_bits,
            compression,
            no_index,
        } => {
            let mz_precision = match mz_bits {
                32 => thermo_raw_mzml::Precision::F32,
                _ => thermo_raw_mzml::Precision::F64,
            };
            let intensity_precision = match intensity_bits {
                64 => thermo_raw_mzml::Precision::F64,
                _ => thermo_raw_mzml::Precision::F32,
            };
            let comp = match compression.to_lowercase().as_str() {
                "none" => thermo_raw_mzml::Compression::None,
                _ => thermo_raw_mzml::Compression::Zlib,
            };
            let config = thermo_raw_mzml::MzmlConfig {
                mz_precision,
                intensity_precision,
                compression: comp,
                write_index: !no_index,
            };

            if input.is_dir() {
                let out_dir = output.unwrap_or_else(|| input.clone());
                let file_count = std::fs::read_dir(&input)?
                    .filter_map(|e| e.ok())
                    .filter(|e| {
                        e.path()
                            .extension()
                            .is_some_and(|ext| ext.eq_ignore_ascii_case("raw"))
                    })
                    .count() as u64;
                let (counter, done, handle) = spawn_progress_bar(file_count, "Converting folder");
                let start = std::time::Instant::now();
                let files = thermo_raw_mzml::convert_folder_with_progress(
                    &input, &out_dir, &config, &counter,
                )?;
                done.store(true, Ordering::Relaxed);
                handle.join().unwrap();
                let elapsed = start.elapsed();
                println!(
                    "Converted {} files in {:.1}s",
                    files.len(),
                    elapsed.as_secs_f64()
                );
                for f in &files {
                    println!("  {}", f.display());
                }
            } else {
                // Get scan count for progress bar
                let raw_for_count = RawFile::open_mmap(&input)?;
                let n_scans = raw_for_count.n_scans() as u64;
                drop(raw_for_count);

                let (counter, done, handle) = spawn_progress_bar(n_scans, "Converting");
                let out_path = output.unwrap_or_else(|| input.with_extension("mzML"));
                let start = std::time::Instant::now();
                thermo_raw_mzml::convert_file_with_progress(&input, &out_path, &config, &counter)?;
                done.store(true, Ordering::Relaxed);
                handle.join().unwrap();
                let elapsed = start.elapsed();
                println!(
                    "Converted {} -> {} in {:.1}s",
                    input.display(),
                    out_path.display(),
                    elapsed.as_secs_f64()
                );
            }
        }

        Commands::Diagnose { file } => {
            let data = std::fs::read(&file)?;
            let report = thermo_raw::diagnose(&data);
            report.print();
        }

        Commands::Benchmark {
            file,
            parallel,
            mmap,
            xic,
        } => {
            let raw = if mmap {
                RawFile::open_mmap(&file)?
            } else {
                RawFile::open(&file)?
            };
            let mode = if mmap { "mmap" } else { "read" };

            if xic {
                // File info
                let n_scans = raw.n_scans();
                let first = raw.first_scan();
                let last = raw.last_scan();
                let ms1_count = (0..n_scans)
                    .filter(|&i| raw.is_ms1_scan(i))
                    .count();
                println!(
                    "File: {} scans ({} MS1, {} MS2+), range {}..{}",
                    n_scans,
                    ms1_count,
                    n_scans as usize - ms1_count,
                    first,
                    last
                );

                let ppm = 5.0;

                // Generate 2000 evenly-spaced m/z targets across typical metabolomics range
                let n_targets = 2000;
                let mz_start = 70.0;
                let mz_end = 1050.0;
                let step = (mz_end - mz_start) / n_targets as f64;
                let targets_2000: Vec<(f64, f64)> = (0..n_targets)
                    .map(|i| (mz_start + i as f64 * step, ppm))
                    .collect();

                // Warmup run (populates caches)
                let _ = raw.xic_ms1(targets_2000[0].0, ppm)?;

                // --- Single target XIC ---
                let start = std::time::Instant::now();
                let chrom = raw.xic_ms1(targets_2000[0].0, ppm)?;
                let t_single = start.elapsed();
                println!(
                    "XIC MS1 single:      {:>8.1}ms  ({} points)",
                    t_single.as_secs_f64() * 1000.0,
                    chrom.rt.len()
                );

                // --- 10-target batch XIC ---
                let start = std::time::Instant::now();
                let chroms = raw.xic_batch_ms1(&targets_2000[..10])?;
                let t_10 = start.elapsed();
                println!(
                    "XIC MS1 batch 10:    {:>8.1}ms  ({} chroms)",
                    t_10.as_secs_f64() * 1000.0,
                    chroms.len()
                );

                // --- 100-target batch XIC ---
                let start = std::time::Instant::now();
                let chroms = raw.xic_batch_ms1(&targets_2000[..100])?;
                let t_100 = start.elapsed();
                println!(
                    "XIC MS1 batch 100:   {:>8.1}ms  ({} chroms)",
                    t_100.as_secs_f64() * 1000.0,
                    chroms.len()
                );

                // --- 500-target batch XIC ---
                let start = std::time::Instant::now();
                let chroms = raw.xic_batch_ms1(&targets_2000[..500])?;
                let t_500 = start.elapsed();
                println!(
                    "XIC MS1 batch 500:   {:>8.1}ms  ({} chroms)",
                    t_500.as_secs_f64() * 1000.0,
                    chroms.len()
                );

                // --- 2000-target batch XIC ---
                let start = std::time::Instant::now();
                let chroms = raw.xic_batch_ms1(&targets_2000)?;
                let t_2000 = start.elapsed();
                println!(
                    "XIC MS1 batch 2000:  {:>8.1}ms  ({} chroms)",
                    t_2000.as_secs_f64() * 1000.0,
                    chroms.len()
                );

                // Summary
                println!("\n--- Summary ---");
                println!(
                    "Per-target cost (batch 2000): {:.2}ms/target",
                    t_2000.as_secs_f64() * 1000.0 / 2000.0
                );
                println!(
                    "Throughput (batch 2000): {:.0} targets/sec",
                    2000.0 / t_2000.as_secs_f64()
                );
            } else {
                let start = std::time::Instant::now();
                if parallel {
                    let scans = raw.scans_parallel(raw.first_scan()..raw.last_scan() + 1)?;
                    let elapsed = start.elapsed();
                    println!(
                        "{} scans read in {:.1}ms ({:.1} scans/sec) [parallel, {}]",
                        scans.len(),
                        elapsed.as_secs_f64() * 1000.0,
                        scans.len() as f64 / elapsed.as_secs_f64(),
                        mode
                    );
                } else {
                    let mut count = 0u32;
                    for i in raw.first_scan()..=raw.last_scan() {
                        let _ = raw.scan(i)?;
                        count += 1;
                    }
                    let elapsed = start.elapsed();
                    println!(
                        "{} scans read in {:.1}ms ({:.1} scans/sec) [sequential, {}]",
                        count,
                        elapsed.as_secs_f64() * 1000.0,
                        count as f64 / elapsed.as_secs_f64(),
                        mode
                    );
                }
            }
        }
    }
    Ok(())
}
