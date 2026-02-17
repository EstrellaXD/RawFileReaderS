# thermo-raw-rs Benchmarks

All benchmarks on Apple Silicon (M-series), `--release` build.
Compared against Thermo .NET RawFileReader 5.x via Mono/pythonnet (x64 Rosetta).

## Test Files

| File | Size | Scans | MS1 Scans | Instrument | Description |
|------|------|-------|-----------|------------|-------------|
| NEW (exp246) | 885 MB | 226,918 | 2,663 | Orbitrap Astral | DDA, 27 min run |
| OLD (exp005) | 413 MB | 34,475 | — | — | Blank run |

---

## 1. Full Scan Decode

Reading and decoding all scan data (centroid m/z + intensity arrays) with trailer enrichment (MS level, precursor info).

### NEW file (226,918 scans)

| Mode | Time | Scans/sec | vs .NET |
|------|------|-----------|---------|
| Sequential, read | 111 ms | 2.0M | 64x |
| Sequential, mmap | 91 ms | 2.5M | 78x |
| Parallel, read | 74 ms | 3.1M | 96x |
| **Parallel, mmap** | **45 ms** | **5.0M** | **157x** |
| .NET GetCentroidStream | 7,067 ms | 32K | 1x |

### OLD file (34,475 scans)

| Mode | Time | Scans/sec |
|------|------|-----------|
| Sequential, mmap | 365 ms | 94K |
| Parallel, mmap | 13 ms | 2.6M |

---

## 2. XIC (Extracted Ion Chromatogram)

Target m/z range: 70-1050 Da, 5 ppm tolerance, NEW file (228,790 scans, 2,663 MS1).
Internally timed (excludes file open). mmap mode, release build.

### v0.4 (binary search + sweep-line)

| Targets | Time | Per-target | Throughput |
|---------|------|------------|------------|
| 1 | 3.1 ms | 3.1 ms | 323/s |
| 10 | 5.7 ms | 0.57 ms | 1,754/s |
| 100 | 6.8 ms | 0.07 ms | 14,706/s |
| 500 | 10.8 ms | 0.02 ms | 46,296/s |
| **2,000** | **23.4 ms** | **0.01 ms** | **85,444/s** |

### v0.3 (linear scan, previous)

| Targets | Time | Per-target | Speedup (v0.4) |
|---------|------|------------|----------------|
| 1 | 3.5 ms | 3.5 ms | 1.1x |
| 10 | 5.6 ms | 0.56 ms | ~1x |
| 100 | 27.0 ms | 0.27 ms | **4.0x** |
| 500 | 112.1 ms | 0.22 ms | **10.4x** |
| **2,000** | **739.4 ms** | **0.37 ms** | **31.6x** |

The speedup scales with target count because the old code was O(centroids) per target
per scan (linear scan through ~5000 peaks), while the new code uses:
- **Binary search** O(log n + k) for <= 64 targets
- **Sweep-line** O(centroids + targets) for > 64 targets

### Comparison with Thermo .NET

| Operation | Rust v0.4 | .NET | Speedup |
|-----------|-----------|------|---------|
| Single XIC MS1 | 3.1 ms | ~5.3 s | ~1,700x |
| 2000 targets batch | 23.4 ms | ~6+ min (est.) | ~15,000x+ |

Note: .NET timings include file open overhead; Rust timings are internally timed.

---

## 3. TIC / BPC

TIC and BPC are extracted directly from the scan index (no scan data decoding required).

| Operation | Time | Notes |
|-----------|------|-------|
| TIC (index-based) | < 1 ms | 226,918 entries, in-memory |
| BPC (index-based) | < 1 ms | Same source |
| CLI wall time | ~1.2 s | Dominated by file I/O (885 MB read) |

---

## 4. File Open

Time to open a file, parse headers, scan index, and trailer layout.

| Mode | Wall time | Notes |
|------|-----------|-------|
| `RawFile::open()` | ~0.5 s | Read 885 MB into memory |
| `RawFile::open_mmap()` | ~0.3 s | Memory-map, pages on demand |

---

## How to Reproduce

```bash
# Build release
cargo build --release

# Full scan benchmark
cargo run --release -p thermo-raw-cli -- benchmark FILE.raw --parallel --mmap

# XIC benchmark (2000 targets, internally timed)
cargo run --release -p thermo-raw-cli -- benchmark FILE.raw --mmap --xic

# Single XIC
cargo run --release -p thermo-raw-cli -- xic FILE.raw --mz 524.2648 --ppm 5 --ms1-only

# Batch XIC (3 targets)
cargo run --release -p thermo-raw-cli -- xic FILE.raw \
  --mz 524.2648 --mz 445.12 --mz 383.1522 --ppm 5 --ms1-only

# TIC export
cargo run --release -p thermo-raw-cli -- tic FILE.raw -o tic.csv
```

---

## Key Optimizations

| Technique | Impact | Where |
|-----------|--------|-------|
| Memory-mapped I/O | 2x for scan decode | `open_mmap()` via memmap2 |
| Parallel scan decode | 2-3x | rayon `par_iter` in `scans_parallel()`, `xic()` |
| Scan index pre-filter | Skip scans outside m/z range | `xic()`: check `low_mz`/`high_mz` before decode |
| MS1 filter via trailer | 85x fewer scans to decode | `xic_ms1()`: read "Master Scan Number" (1 i32) |
| **Binary search m/z filter** | **Up to 200x fewer comparisons per scan** | `sum_intensity_in_range()`: `partition_point` + walk |
| **Sweep-line multi-target** | **O(peaks + targets) vs O(targets * log peaks)** | `xic_batch_ms1()`: sorted ranges walk with centroids |
| **Flat output array** | **Zero per-scan allocation** | `xic_batch_ms1()`: pre-allocated `n_ms1 * n_targets` |
| Batch single-pass | N targets for ~1x cost | `xic_batch_ms1()`: decode once, extract all targets |
| Eager trailer layout | O(1) field access | `TrailerLayout` caches byte offsets at file open |

---

*Last updated: 2026-02-17*
