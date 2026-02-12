use numpy::{IntoPyArray, PyArray1, PyArray3, PyArrayMethods};
use pyo3::exceptions::PyValueError;
use pyo3::prelude::*;
use ::thermo_raw::{MsLevel, RawFile as InnerRawFile};
use std::path::Path;

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
    #[pyo3(signature = (mz, ppm=None))]
    fn xic<'py>(
        &self,
        py: Python<'py>,
        mz: f64,
        ppm: Option<f64>,
    ) -> PyResult<(Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)> {
        let ppm = ppm.unwrap_or(5.0);
        let chrom = self
            .inner
            .xic(mz, ppm)
            .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        let rt = chrom.rt.into_pyarray(py);
        let intensity = chrom.intensity.into_pyarray(py);
        Ok((rt, intensity))
    }

    /// XIC restricted to MS1 scans only. Much faster for DDA data.
    #[pyo3(signature = (mz, ppm=None))]
    fn xic_ms1<'py>(
        &self,
        py: Python<'py>,
        mz: f64,
        ppm: Option<f64>,
    ) -> PyResult<(Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)> {
        let ppm = ppm.unwrap_or(5.0);
        let chrom = self
            .inner
            .xic_ms1(mz, ppm)
            .map_err(|e| PyValueError::new_err(format!("{}", e)))?;
        let rt = chrom.rt.into_pyarray(py);
        let intensity = chrom.intensity.into_pyarray(py);
        Ok((rt, intensity))
    }

    /// Batch XIC for multiple targets in a single pass (MS1 only).
    ///
    /// Args:
    ///     targets: list of (mz, ppm) tuples
    ///
    /// Returns:
    ///     list of (rt_array, intensity_array) tuples, one per target
    fn xic_batch_ms1<'py>(
        &self,
        py: Python<'py>,
        targets: Vec<(f64, f64)>,
    ) -> PyResult<Vec<(Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)>> {
        let chroms = self
            .inner
            .xic_batch_ms1(&targets)
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
    fn all_ms1_scans<'py>(
        &self,
        py: Python<'py>,
    ) -> PyResult<Vec<(Bound<'py, PyArray1<f64>>, Bound<'py, PyArray1<f64>>)>> {
        let first = self.inner.first_scan();
        let last = self.inner.last_scan();
        let scans = self
            .inner
            .scans_parallel(first..last + 1)
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
///
/// Returns:
///     (tensor, rt_grid, sample_names) where:
///         tensor: numpy array of shape (n_samples, n_targets, n_timepoints)
///         rt_grid: numpy array of RT values
///         sample_names: list of file stem strings
#[pyfunction]
#[pyo3(signature = (file_paths, targets, rt_range=None, rt_resolution=0.01))]
fn batch_xic<'py>(
    py: Python<'py>,
    file_paths: Vec<String>,
    targets: Vec<(f64, f64)>,
    rt_range: Option<(f64, f64)>,
    rt_resolution: f64,
) -> PyResult<(
    Bound<'py, PyArray3<f64>>,
    Bound<'py, PyArray1<f64>>,
    Vec<String>,
)> {
    let paths: Vec<&Path> = file_paths.iter().map(|s| Path::new(s.as_str())).collect();

    let result = ::thermo_raw::batch_xic_ms1(&paths, &targets, rt_range, rt_resolution)
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

#[pymodule]
fn thermo_raw(m: &Bound<'_, PyModule>) -> PyResult<()> {
    m.add_class::<RawFile>()?;
    m.add_class::<ScanInfo>()?;
    m.add_function(wrap_pyfunction!(batch_xic, m)?)?;
    Ok(())
}
