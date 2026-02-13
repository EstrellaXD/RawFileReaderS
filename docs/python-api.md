# Python API Reference

## Installation

```bash
cd crates/thermo-raw-py
maturin develop --release
```

## Module: `RawFileReaderS`

```python
from RawFileReaderS import RawFile, ScanInfo, batch_xic
```

---

## `RawFile`

Primary entry point for reading Thermo RAW files.

### Constructor

```python
RawFile(path: str, mmap: bool = False)
```

Opens a Thermo RAW file.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `path` | `str` | required | Path to `.RAW` file |
| `mmap` | `bool` | `False` | Use memory-mapped I/O (faster for large files) |

```python
raw = RawFile("sample.RAW")
raw = RawFile("sample.RAW", mmap=True)  # memory-mapped, ~2x faster
```

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `n_scans` | `int` | Total number of scans |
| `first_scan` | `int` | First scan number |
| `last_scan` | `int` | Last scan number |
| `start_time` | `float` | Acquisition start time (minutes) |
| `end_time` | `float` | Acquisition end time (minutes) |
| `instrument_model` | `str` | Instrument model name |
| `sample_name` | `str` | Sample name from file metadata |
| `version` | `int` | RAW file format version (57-66) |

```python
raw = RawFile("sample.RAW")
print(f"Scans: {raw.first_scan}..{raw.last_scan} ({raw.n_scans} total)")
print(f"RT range: {raw.start_time:.2f} - {raw.end_time:.2f} min")
print(f"Instrument: {raw.instrument_model}")
```

### Methods

#### `scan(scan_number) -> (ndarray, ndarray)`

Returns centroid m/z and intensity arrays for a single scan.

```python
mz, intensity = raw.scan(1)
```

#### `scan_info(scan_number) -> ScanInfo`

Returns scan metadata without decoding peak arrays.

```python
info = raw.scan_info(1)
print(f"RT={info.rt:.2f} MS{info.ms_level} {info.polarity}")
```

#### `tic() -> (ndarray, ndarray)`

Returns TIC (Total Ion Current) as `(rt, intensity)` arrays. Sub-millisecond; reads from scan index without decoding scans.

```python
rt, intensity = raw.tic()
```

#### `xic(mz, ppm=5.0) -> (ndarray, ndarray)`

Extracted ion chromatogram across all scans (MS1 + MS2).

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `mz` | `float` | required | Target m/z value |
| `ppm` | `float` | `5.0` | Mass tolerance in ppm |

```python
rt, intensity = raw.xic(760.5851, ppm=5.0)
```

#### `xic_ms1(mz, ppm=5.0) -> (ndarray, ndarray)`

Extracted ion chromatogram restricted to MS1 scans only. Much faster for DDA data (skips 85-95% of scans).

```python
rt, intensity = raw.xic_ms1(760.5851, ppm=5.0)
```

#### `xic_batch_ms1(targets) -> list[(ndarray, ndarray)]`

Batch XIC for multiple targets in a single pass over MS1 scans. Decodes each scan once for all targets.

| Parameter | Type | Description |
|-----------|------|-------------|
| `targets` | `list[(float, float)]` | List of `(mz, ppm)` tuples |

```python
targets = [(760.5851, 5.0), (788.6164, 5.0), (810.6007, 10.0)]
results = raw.xic_batch_ms1(targets)
for rt, intensity in results:
    print(f"  {len(rt)} points")
```

#### `all_ms1_scans() -> list[(ndarray, ndarray)]`

Decodes all MS1 scans in parallel (rayon). Returns list of `(mz, intensity)` tuples.

```python
ms1_scans = raw.all_ms1_scans()
print(f"Got {len(ms1_scans)} MS1 scans")
```

#### `trailer_extra(scan_number) -> dict[str, str]`

Returns trailer extra fields for a scan as a string dictionary.

```python
trailer = raw.trailer_extra(1)
print(trailer.get("Charge State:"))
```

#### `trailer_fields() -> list[str]`

Returns available trailer field names.

```python
for field in raw.trailer_fields():
    print(field)
```

---

## `ScanInfo`

Scan metadata object returned by `RawFile.scan_info()`. All fields are read-only.

| Field | Type | Description |
|-------|------|-------------|
| `scan_number` | `int` | Scan number |
| `rt` | `float` | Retention time (minutes) |
| `ms_level` | `int` | MS level (1, 2, 3, ...) |
| `polarity` | `str` | `"positive"`, `"negative"`, or `"unknown"` |
| `tic` | `float` | Total ion current |
| `base_peak_mz` | `float` | Base peak m/z |
| `base_peak_intensity` | `float` | Base peak intensity |
| `filter_string` | `str \| None` | Thermo filter string |
| `precursor_mz` | `float \| None` | Precursor m/z (MS2+ only) |
| `precursor_charge` | `int \| None` | Precursor charge (MS2+ only) |

---

## `batch_xic()`

Module-level function for multi-file batch XIC extraction with RT alignment.

```python
batch_xic(
    file_paths: list[str],
    targets: list[tuple[float, float]],
    rt_range: tuple[float, float] | None = None,
    rt_resolution: float = 0.01,
) -> tuple[ndarray, ndarray, list[str]]
```

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `file_paths` | `list[str]` | required | Paths to RAW files |
| `targets` | `list[(float, float)]` | required | `(mz, ppm)` tuples |
| `rt_range` | `(float, float) \| None` | `None` | Optional RT window (minutes) |
| `rt_resolution` | `float` | `0.01` | Grid spacing in minutes |

**Returns:** `(tensor, rt_grid, sample_names)`

| Return | Type | Description |
|--------|------|-------------|
| `tensor` | `ndarray` | Shape `(n_samples, n_targets, n_timepoints)` |
| `rt_grid` | `ndarray` | Common RT axis (minutes) |
| `sample_names` | `list[str]` | File stems (e.g., `"sample_01"`) |

All files are opened and processed in parallel. XICs are interpolated onto a common RT grid spanning the intersection of all files' RT ranges.

```python
from RawFileReaderS import batch_xic
import matplotlib.pyplot as plt

files = ["ctrl_01.RAW", "ctrl_02.RAW", "treated_01.RAW"]
targets = [(760.5851, 5.0), (788.6164, 5.0)]

tensor, rt_grid, names = batch_xic(files, targets, rt_range=(5.0, 25.0))

# Plot one target across all samples
for i, name in enumerate(names):
    plt.plot(rt_grid, tensor[i, 0, :], label=name)
plt.xlabel("RT (min)")
plt.ylabel("Intensity")
plt.legend()
plt.show()
```
