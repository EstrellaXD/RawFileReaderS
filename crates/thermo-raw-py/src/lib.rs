use numpy::{IntoPyArray, PyArray1, PyArray3, PyArrayMethods};
use pyo3::exceptions::PyValueError;
use pyo3::prelude::*;
use ::thermo_raw::{MsLevel, RawFile as InnerRawFile, AcquisitionType};
use ::thermo_raw::progress::{self, ProgressCounter};
use std::path::Path;
use std::sync::Arc;
use std::sync::atomic::{AtomicBool, Ordering};

/// Try to create a tqdm progress bar, returning None if tqdm is not installed.
fn try_create_tqdm(py: Python<'_>, total: u64, desc: &str) -> Option<PyObject> {
    let tqdm = py
        .import("tqdm.auto")
        .or_else(|_| py.import("tqdm"))
        .ok()?;
    let bar = tqdm
        .call_method1("tqdm", (total,))
        .ok()?;
    let _ = bar.setattr("desc", desc);
    Some(bar.unbind())
}

/// Spawn a background thread that polls the atomic counter and updates tqdm.
///
/// Returns `(done_flag, join_handle)`. Set `done_flag` to true when the
/// operation completes, then call `handle.join()`.
fn spawn_tqdm_updater(
    py: Python<'_>,
    counter: &ProgressCounter,
    bar: PyObject,
) -> (Arc<AtomicBool>, std::thread::JoinHandle<()>) {
    let done = Arc::new(AtomicBool::new(false));
    let done_clone = Arc::clone(&done);
    let counter_clone = Arc::clone(counter);
    let bar_clone = bar.clone_ref(py);

    let handle = std::thread::spawn(move || {
        let mut last = 0u64;
        while !done_clone.load(Ordering::Relaxed) {
            std::thread::sleep(std::time::Duration::from_millis(100));
            let current = counter_clone.load(Ordering::Relaxed);
            if current > last {
                let delta = current - last;
                last = current;
                Python::with_gil(|py| {
                    let _ = bar_clone.call_method1(py, "update", (delta,));
                });
            }
        }
        // Final flush
        let current = counter_clone.load(Ordering::Relaxed);
        if current > last {
            let delta = current - last;
            Python::with_gil(|py| {
                let _ = bar_clone.call_method1(py, "update", (delta,));
                let _ = bar_clone.call_method0(py, "close");
            });
        } else {
            Python::with_gil(|py| {
                let _ = bar_clone.call_method0(py, "close");
            });
        }
    });

    (done, handle)
}

#[pyclass]
struct RawFile {
    inner: InnerRawFile,
}

#[pymethods]
impl RawFile {
    #[new]
    #[pyo3(signature = (path, mmap=false))]
    fn new(path: &str, mmap: bool) -> PyResult<Self> {
        let inner = if mmap {
            InnerRawFile::open_mmap(path)
        } else {
            InnerRawFile::open(path)
        }
        .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        Ok(Self { inner })
    }

    #[getter]
    fn n_scans(&self) -> u32 {
        self.inner.n_scans()
    }

    #[getter]
    fn first_scan(&self) -> u32 {
        self.inner.first_scan()
    }

    #[getter]
    fn last_scan(&self) -> u32 {
        self.inner.last_scan()
    }

    #[getter]
    fn start_time(&self) -> f64 {
        self.inner.start_time()
    }

    #[getter]
    fn end_time(&self) -> f64 {
        self.inner.end_time()
    }

    #[getter]
    fn instrument_model(&self) -> String {
        self.inner.metadata().instrument_model.clone()
    }

    #[getter]
    fn sample_name(&self) -> String {
        self.inner.metadata().sample_name.clone()
    }

    #[getter]
    fn version(&self) -> u32 {
        self.inner.version()
    }

    /// Return a dict with scan data including ms_level, polarity, precursor info.
    fn scan_info(&self, scan_number: u32) -> PyResult<ScanInfo> {
        let scan = self
            .inner
            .scan(scan_number)
            .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        Ok(ScanInfo {
            scan_number: scan.scan_number,
            rt: scan.rt,
            ms_level: match scan.ms_level {
                MsLevel::Ms1 => 1,
                MsLevel::Ms2 => 2,
                MsLevel::Ms3 => 3,
                MsLevel::Other(n) => n as u8,
            },
            polarity: match scan.polarity {
                ::thermo_raw::Polarity::Positive => "positive".to_string(),
                ::thermo_raw::Polarity::Negative => "negative".to_string(),
                ::thermo_raw::Polarity::Unknown => "unknown".to_string(),
            },
            tic: scan.tic,
            base_peak_mz: scan.base_peak_mz,
            base_peak_intensity: scan.base_peak_intensity,
            filter_string: scan.filter_string,
            precursor_mz: scan.precursor.as_ref().map(|p| p.mz),
            precursor_charge: scan.precursor.as_ref().and_then(|p| p.charge),
        })
    }

    /// Return (mz_array, intensity_array) as numpy arrays.
    fn scan<'py>(
        &self,
        py: Python<'py>,
        scan_number: u32,
    ) -> PyResult<(Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)> {
        let scan = self
            .inner
            .scan(scan_number)
            .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        let mz = scan.centroid_mz.into_pyarray(py);
        let intensity = scan.centroid_intensity.into_pyarray(py);
        Ok((mz, intensity))
    }

    /// TIC: return (rt_array, intensity_array) as numpy arrays.
    fn tic<'py>(
        &self,
        py: Python<'py>,
    ) -> (Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>) {
        let chrom = self.inner.tic();
        let rt = chrom.rt.into_pyarray(py);
        let intensity = chrom.intensity.into_pyarray(py);
        (rt, intensity)
    }

    /// XIC (all scans): return (rt_array, intensity_array) as numpy arrays.
    #[pyo3(signature = (mz, ppm=None, progress=false))]
    fn xic<'py>(
        &self,
        py: Python<'py>,
        mz: f64,
        ppm: Option<f64>,
        progress: bool,
    ) -> PyResult<(Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)> {
        let ppm = ppm.unwrap_or(5.0);
        let chrom = if progress {
            let total = self.inner.n_scans() as u64;
            let counter = ::thermo_raw::new_counter();
            let bar = try_create_tqdm(py, total, "XIC");
            let updater = bar.map(|b| spawn_tqdm_updater(py, &counter, b));
            let result = py.allow_threads(|| self.inner.xic_with_progress(mz, ppm, &counter));
            if let Some((done, handle)) = updater {
                done.store(true, Ordering::Relaxed);
                handle.join().unwrap();
            }
            result
        } else {
            self.inner.xic(mz, ppm)
        }
        .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        let rt = chrom.rt.into_pyarray(py);
        let intensity = chrom.intensity.into_pyarray(py);
        Ok((rt, intensity))
    }

    /// XIC restricted to MS1 scans only. Much faster for DDA data.
    #[pyo3(signature = (mz, ppm=None, progress=false))]
    fn xic_ms1<'py>(
        &self,
        py: Python<'py>,
        mz: f64,
        ppm: Option<f64>,
        progress: bool,
    ) -> PyResult<(Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)> {
        let ppm = ppm.unwrap_or(5.0);
        let chrom = if progress {
            let total = self.inner.n_scans() as u64;
            let counter = ::thermo_raw::new_counter();
            let bar = try_create_tqdm(py, total, "XIC MS1");
            let updater = bar.map(|b| spawn_tqdm_updater(py, &counter, b));
            let result = py.allow_threads(|| self.inner.xic_ms1_with_progress(mz, ppm, &counter));
            if let Some((done, handle)) = updater {
                done.store(true, Ordering::Relaxed);
                handle.join().unwrap();
            }
            result
        } else {
            self.inner.xic_ms1(mz, ppm)
        }
        .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        let rt = chrom.rt.into_pyarray(py);
        let intensity = chrom.intensity.into_pyarray(py);
        Ok((rt, intensity))
    }

    /// Batch XIC for multiple targets in a single pass (MS1 only).
    ///
    /// Args:
    ///     targets: list of (mz, ppm) tuples
    ///     progress: show tqdm progress bar (default: False)
    ///
    /// Returns:
    ///     list of (rt_array, intensity_array) tuples, one per target
    #[pyo3(signature = (targets, progress=false))]
    fn xic_batch_ms1<'py>(
        &self,
        py: Python<'py>,
        targets: Vec<(f64, f64)>,
        progress: bool,
    ) -> PyResult<Vec<(Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)>> {
        let chroms = if progress {
            let total = self.inner.n_scans() as u64;
            let counter = ::thermo_raw::new_counter();
            let bar = try_create_tqdm(py, total, "Batch XIC MS1");
            let updater = bar.map(|b| spawn_tqdm_updater(py, &counter, b));
            let result = py.allow_threads(|| {
                self.inner.xic_batch_ms1_with_progress(&targets, &counter)
            });
            if let Some((done, handle)) = updater {
                done.store(true, Ordering::Relaxed);
                handle.join().unwrap();
            }
            result
        } else {
            self.inner.xic_batch_ms1(&targets)
        }
        .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        let results = chroms
            .into_iter()
            .map(|c| {
                let rt = c.rt.into_pyarray(py);
                let intensity = c.intensity.into_pyarray(py);
                (rt, intensity)
            })
            .collect();
        Ok(results)
    }

    /// Read all MS1 scans in parallel, return list of (mz, intensity) tuples.
    #[pyo3(signature = (progress=false))]
    fn all_ms1_scans<'py>(
        &self,
        py: Python<'py>,
        progress: bool,
    ) -> PyResult<Vec<(Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)>> {
        let first = self.inner.first_scan();
        let last = self.inner.last_scan();
        let scans = if progress {
            let total = self.inner.n_scans() as u64;
            let counter = ::thermo_raw::new_counter();
            let bar = try_create_tqdm(py, total, "Reading scans");
            let updater = bar.map(|b| spawn_tqdm_updater(py, &counter, b));
            let result = py.allow_threads(|| {
                self.inner
                    .scans_parallel_with_progress(first..last + 1, &counter)
            });
            if let Some((done, handle)) = updater {
                done.store(true, Ordering::Relaxed);
                handle.join().unwrap();
            }
            result
        } else {
            self.inner.scans_parallel(first..last + 1)
        }
        .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        let results: Vec<_> = scans
            .into_iter()
            .filter(|s| matches!(s.ms_level, MsLevel::Ms1))
            .map(|s| {
                let mz = s.centroid_mz.into_pyarray(py);
                let int = s.centroid_intensity.into_pyarray(py);
                (mz, int)
            })
            .collect();
        Ok(results)
    }

    // ─── MS2/DDA/DIA API ───

    /// Get the MS level of a scan (by scan number). Returns 1, 2, 3, etc.
    fn ms_level_of_scan(&self, scan_number: u32) -> PyResult<u8> {
        let idx = scan_number
            .checked_sub(self.inner.first_scan())
            .ok_or_else(|| PyValueError::new_err(format!("Scan {} out of range", scan_number)))?;
        Ok(match self.inner.ms_level_of_scan(idx) {
            MsLevel::Ms1 => 1,
            MsLevel::Ms2 => 2,
            MsLevel::Ms3 => 3,
            MsLevel::Other(n) => n,
        })
    }

    /// Check if a scan is MS2.
    fn is_ms2_scan(&self, scan_number: u32) -> PyResult<bool> {
        let idx = scan_number
            .checked_sub(self.inner.first_scan())
            .ok_or_else(|| PyValueError::new_err(format!("Scan {} out of range", scan_number)))?;
        Ok(self.inner.is_ms2_scan(idx))
    }

    /// Get all scan numbers at a given MS level (1, 2, 3, ...).
    fn scan_numbers_by_level(&self, level: u8) -> Vec<u32> {
        let ms_level = match level {
            1 => MsLevel::Ms1,
            2 => MsLevel::Ms2,
            3 => MsLevel::Ms3,
            n => MsLevel::Other(n),
        };
        self.inner.scan_numbers_by_level(ms_level)
    }

    /// Get lightweight metadata for all MS2 scans (no scan data decoding).
    fn all_ms2_scan_info(&self) -> Vec<PyMs2ScanInfo> {
        self.inner
            .all_ms2_scan_info()
            .iter()
            .map(PyMs2ScanInfo::from)
            .collect()
    }

    /// Find MS2 scans matching a precursor m/z within ppm tolerance.
    #[pyo3(signature = (precursor_mz, tolerance_ppm=10.0))]
    fn ms2_scans_for_precursor(
        &self,
        precursor_mz: f64,
        tolerance_ppm: f64,
    ) -> Vec<PyMs2ScanInfo> {
        self.inner
            .ms2_scans_for_precursor(precursor_mz, tolerance_ppm)
            .iter()
            .map(PyMs2ScanInfo::from)
            .collect()
    }

    /// Get sorted, deduplicated list of unique precursor m/z values as a numpy array.
    fn precursor_list<'py>(&self, py: Python<'py>) -> Bound<'py, PyArray1<f64>> {
        self.inner.precursor_list().into_pyarray(py)
    }

    /// Find the parent MS1 scan number for a given scan.
    fn parent_ms1_scan(&self, scan_number: u32) -> Option<u32> {
        self.inner.parent_ms1_scan(scan_number)
    }

    /// Classify the acquisition type: "ms1_only", "dda", "dia", or "mixed".
    fn acquisition_type(&self) -> &'static str {
        match self.inner.acquisition_type() {
            AcquisitionType::Ms1Only => "ms1_only",
            AcquisitionType::Dda => "dda",
            AcquisitionType::Dia => "dia",
            AcquisitionType::Mixed => "mixed",
        }
    }

    /// Get unique DIA isolation windows.
    fn isolation_windows(&self) -> Vec<PyIsolationWindow> {
        self.inner
            .isolation_windows()
            .iter()
            .map(PyIsolationWindow::from)
            .collect()
    }

    /// Get MS2 scans belonging to a specific isolation window.
    fn scans_for_window(&self, window: &PyIsolationWindow) -> Vec<PyMs2ScanInfo> {
        let w = ::thermo_raw::IsolationWindow {
            center_mz: window.center_mz,
            isolation_width: window.isolation_width,
            low_mz: window.low_mz,
            high_mz: window.high_mz,
            collision_energy: window.collision_energy,
            activation: window.activation.clone(),
        };
        self.inner
            .scans_for_window(&w)
            .iter()
            .map(PyMs2ScanInfo::from)
            .collect()
    }

    /// XIC within a specific DIA isolation window.
    fn xic_ms2_window<'py>(
        &self,
        py: Python<'py>,
        mz: f64,
        ppm: f64,
        window: &PyIsolationWindow,
    ) -> PyResult<(Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)> {
        let w = ::thermo_raw::IsolationWindow {
            center_mz: window.center_mz,
            isolation_width: window.isolation_width,
            low_mz: window.low_mz,
            high_mz: window.high_mz,
            collision_energy: window.collision_energy,
            activation: window.activation.clone(),
        };
        let chrom = self
            .inner
            .xic_ms2_window(mz, ppm, &w)
            .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        let rt = chrom.rt.into_pyarray(py);
        let intensity = chrom.intensity.into_pyarray(py);
        Ok((rt, intensity))
    }

    /// Get trailer extra fields for a scan as a dict.
    fn trailer_extra(&self, scan_number: u32) -> PyResult<std::collections::HashMap<String, String>> {
        self.inner
            .trailer_extra(scan_number)
            .map_err(|e| PyValueError::new_err(format!("{}", e)))
    }

    /// Get list of trailer field names.
    fn trailer_fields(&self) -> Vec<String> {
        self.inner.trailer_fields()
    }
}

/// Scan metadata (without raw array data).
#[pyclass]
#[derive(Debug, Clone)]
struct ScanInfo {
    #[pyo3(get)]
    scan_number: u32,
    #[pyo3(get)]
    rt: f64,
    #[pyo3(get)]
    ms_level: u8,
    #[pyo3(get)]
    polarity: String,
    #[pyo3(get)]
    tic: f64,
    #[pyo3(get)]
    base_peak_mz: f64,
    #[pyo3(get)]
    base_peak_intensity: f64,
    #[pyo3(get)]
    filter_string: Option<String>,
    #[pyo3(get)]
    precursor_mz: Option<f64>,
    #[pyo3(get)]
    precursor_charge: Option<i32>,
}

/// DIA isolation window metadata.
#[pyclass]
#[derive(Debug, Clone)]
struct PyIsolationWindow {
    #[pyo3(get)]
    center_mz: f64,
    #[pyo3(get)]
    isolation_width: f64,
    #[pyo3(get)]
    low_mz: f64,
    #[pyo3(get)]
    high_mz: f64,
    #[pyo3(get)]
    collision_energy: f64,
    #[pyo3(get)]
    activation: String,
}

#[pymethods]
impl PyIsolationWindow {
    fn __repr__(&self) -> String {
        format!(
            "IsolationWindow(center_mz={:.4}, width={:.1}, ce={:.1}, activation={})",
            self.center_mz, self.isolation_width, self.collision_energy, self.activation
        )
    }
}

impl From<&::thermo_raw::IsolationWindow> for PyIsolationWindow {
    fn from(w: &::thermo_raw::IsolationWindow) -> Self {
        Self {
            center_mz: w.center_mz,
            isolation_width: w.isolation_width,
            low_mz: w.low_mz,
            high_mz: w.high_mz,
            collision_energy: w.collision_energy,
            activation: w.activation.clone(),
        }
    }
}

/// Lightweight MS2 scan metadata.
#[pyclass]
#[derive(Debug, Clone)]
struct PyMs2ScanInfo {
    #[pyo3(get)]
    scan_number: u32,
    #[pyo3(get)]
    rt: f64,
    #[pyo3(get)]
    precursor_mz: f64,
    #[pyo3(get)]
    isolation_width: f64,
    #[pyo3(get)]
    collision_energy: f64,
    #[pyo3(get)]
    activation: String,
    #[pyo3(get)]
    scan_event_index: u16,
    #[pyo3(get)]
    tic: f64,
}

#[pymethods]
impl PyMs2ScanInfo {
    fn __repr__(&self) -> String {
        format!(
            "Ms2ScanInfo(scan={}, rt={:.2}, precursor={:.4}, ce={:.1})",
            self.scan_number, self.rt, self.precursor_mz, self.collision_energy
        )
    }
}

impl From<&::thermo_raw::Ms2ScanInfo> for PyMs2ScanInfo {
    fn from(info: &::thermo_raw::Ms2ScanInfo) -> Self {
        Self {
            scan_number: info.scan_number,
            rt: info.rt,
            precursor_mz: info.precursor_mz,
            isolation_width: info.isolation_width,
            collision_energy: info.collision_energy,
            activation: info.activation.clone(),
            scan_event_index: info.scan_event_index,
            tic: info.tic,
        }
    }
}

/// Batch XIC across multiple RAW files, returning a 3D numpy tensor.
///
/// Opens all files in parallel, extracts EICs for each target,
/// and aligns them to a common RT grid via linear interpolation.
///
/// Args:
///     file_paths: list of paths to RAW files
///     targets: list of (mz, ppm) tuples
///     rt_range: optional (start, end) in minutes
///     rt_resolution: grid spacing in minutes (default: 0.01)
///     progress: show tqdm progress bar (default: False)
///
/// Returns:
///     (tensor, rt_grid, sample_names) where:
///         tensor: numpy array of shape (n_samples, n_targets, n_timepoints)
///         rt_grid: numpy array of RT values
///         sample_names: list of file stem strings
/// Spawn a background thread that polls the atomic counter and calls a Python callback.
///
/// Similar to `spawn_tqdm_updater` but invokes an arbitrary Python callable
/// with the delta count instead of updating a tqdm bar.
fn spawn_callback_updater(
    py: Python<'_>,
    counter: &ProgressCounter,
    callback: PyObject,
) -> (Arc<AtomicBool>, std::thread::JoinHandle<()>) {
    let done = Arc::new(AtomicBool::new(false));
    let done_clone = Arc::clone(&done);
    let counter_clone = Arc::clone(counter);
    let callback_clone = callback.clone_ref(py);

    let handle = std::thread::spawn(move || {
        let mut last = 0u64;
        while !done_clone.load(Ordering::Relaxed) {
            std::thread::sleep(std::time::Duration::from_millis(200));
            let current = counter_clone.load(Ordering::Relaxed);
            if current > last {
                let delta = current - last;
                last = current;
                Python::with_gil(|py| {
                    let _ = callback_clone.call1(py, (delta,));
                });
            }
        }
        // Final flush
        let current = counter_clone.load(Ordering::Relaxed);
        if current > last {
            let delta = current - last;
            Python::with_gil(|py| {
                let _ = callback_clone.call1(py, (delta,));
            });
        }
    });

    (done, handle)
}

#[pyfunction]
#[pyo3(signature = (file_paths, targets, rt_range=None, rt_resolution=0.01, progress=false, progress_callback=None))]
fn batch_xic<'py>(
    py: Python<'py>,
    file_paths: Vec<String>,
    targets: Vec<(f64, f64)>,
    rt_range: Option<(f64, f64)>,
    rt_resolution: f64,
    progress: bool,
    progress_callback: Option<PyObject>,
) -> PyResult<(
    Bound<'py, PyArray3<f64>>,
    Bound<'py, PyArray1<f64>>,
    Vec<String>,
)> {
    let paths: Vec<&Path> = file_paths.iter().map(|s| Path::new(s.as_str())).collect();

    let result = if let Some(callback) = progress_callback {
        // Use the Python callback for progress (preferred for embedding in apps)
        let counter = ::thermo_raw::new_counter();
        let (done, handle) = spawn_callback_updater(py, &counter, callback);
        let r = py.allow_threads(|| {
            ::thermo_raw::batch_xic_ms1_with_progress(
                &paths,
                &targets,
                rt_range,
                rt_resolution,
                &counter,
            )
        });
        done.store(true, Ordering::Relaxed);
        // Release GIL before joining to avoid deadlock: the updater thread's
        // final flush needs the GIL to call the Python callback.
        py.allow_threads(|| handle.join().unwrap());
        r
    } else if progress {
        let total = file_paths.len() as u64;
        let counter = ::thermo_raw::new_counter();
        let bar = try_create_tqdm(py, total, "Batch XIC");
        let updater = bar.map(|b| spawn_tqdm_updater(py, &counter, b));
        let r = py.allow_threads(|| {
            ::thermo_raw::batch_xic_ms1_with_progress(
                &paths,
                &targets,
                rt_range,
                rt_resolution,
                &counter,
            )
        });
        if let Some((done, handle)) = updater {
            done.store(true, Ordering::Relaxed);
            // Release GIL before joining to avoid deadlock with final flush.
            py.allow_threads(|| handle.join().unwrap());
        }
        r
    } else {
        py.allow_threads(|| {
            ::thermo_raw::batch_xic_ms1(&paths, &targets, rt_range, rt_resolution)
        })
    }
    .map_err(|e| PyValueError::new_err(format!("{}", e)))?;

    // Build 3D numpy array (samples x targets x timepoints) from flat data
    let flat = result.data.into_pyarray(py);
    let shape = [result.n_samples, result.n_targets, result.n_timepoints];
    let tensor = flat
        .reshape(shape)
        .map_err(|e| PyValueError::new_err(format!("reshape failed: {}", e)))?;
    let rt_grid = result.rt_grid.into_pyarray(py);

    Ok((tensor, rt_grid, result.sample_names))
}

#[pymodule(name = "RawFileReaderS")]
fn raw_file_reader_s(m: &Bound<'_, PyModule>) -> PyResult<()> {
    m.add_class::<RawFile>()?;
    m.add_class::<ScanInfo>()?;
    m.add_class::<PyIsolationWindow>()?;
    m.add_class::<PyMs2ScanInfo>()?;
    m.add_function(wrap_pyfunction!(batch_xic, m)?)?;
    Ok(())
}
