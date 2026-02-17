# thermo-raw-rs Benchmarks

All benchmarks on Apple Silicon (M-series), `--release` build.
Thermo .NET RawFileReader 8.0.6 via x64 .NET 8 (Rosetta 2).

## Test File

| Property | Value |
|----------|-------|
| File | exp210_251218_YS_Metabolites_NEG_A5_Cell_005.raw |
| Size | 796 MB |
| Scans | 228,790 total (2,663 MS1, 226,127 MS2+) |
| Instrument | Orbitrap Astral |
| Acquisition | DDA (negative mode, metabolomics) |

---

## 1. XIC (Extracted Ion Chromatogram) -- Rust vs .NET

2000 evenly-spaced m/z targets across 70-1050 Da, 5 ppm tolerance.
MS1 scans only. Internally timed (excludes file open).

### Head-to-head comparison

| Targets | Rust v0.4 | .NET 8.0.6 | Speedup |
|---------|-----------|------------|---------|
| 1 | 3.3 ms | 224 ms | **68x** |
| 10 | 5.2 ms | 245 ms | **47x** |
| 100 | 6.1 ms | 403 ms | **66x** |
| 500 | 10.5 ms | 1,064 ms | **101x** |
| **2,000** | **21.7 ms** | **2,753 ms** | **127x** |

### Throughput scaling

| Metric | Rust v0.4 | .NET 8.0.6 |
|--------|-----------|------------|
| Per-target cost (2000 batch) | 0.01 ms | 1.38 ms |
| Throughput (2000 batch) | **92,228 targets/sec** | 726 targets/sec |
| Single-target latency | 3.3 ms | 224 ms |

### Rust v0.4 vs v0.3 (internal improvement)

| Targets | v0.3 (linear) | v0.4 (bsearch) | Speedup |
|---------|---------------|-----------------|---------|
| 1 | 3.5 ms | 3.3 ms | 1.1x |
| 10 | 5.6 ms | 5.2 ms | 1.1x |
| 100 | 27.0 ms | 6.1 ms | **4.4x** |
| 500 | 112.1 ms | 10.5 ms | **10.7x** |
| **2,000** | **739.4 ms** | **21.7 ms** | **34x** |

The speedup scales with target count because v0.3 was O(centroids) per target
per scan (linear scan through ~5000 peaks), while v0.4 uses:
- **Binary search** O(log n + k) for <= 64 targets
- **Sweep-line** O(centroids + targets) for > 64 targets

---

## 2. Full Scan Decode

Reading and decoding all 228,790 scan centroid m/z + intensity arrays.

| Mode | Time | Scans/sec | vs .NET |
|------|------|-----------|---------|
| Rust sequential, mmap | 158 ms | 1.45M | **8.7x** |
| Rust parallel, mmap | 367 ms | 624K | 3.8x |
| **.NET GetCentroidStream** | **1,380 ms** | **166K** | **1x** |

Note: For this file, sequential mmap is faster than parallel due to contiguous
scan data layout favoring cache locality over thread parallelism.

---

## 3. TIC / BPC

TIC and BPC are extracted directly from the scan index (no scan data decoding required).

| Operation | Time | Notes |
|-----------|------|-------|
| TIC (index-based) | < 1 ms | 228,790 entries, in-memory |
| BPC (index-based) | < 1 ms | Same source |

---

## 4. File Open

| Mode | Rust | .NET |
|------|------|------|
| Standard | ~0.5 s | 205 ms |
| Memory-mapped | ~0.3 s | — |

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

# .NET benchmark (requires x64 .NET 8)
cd /tmp/claude/XicBenchmark && /usr/local/share/dotnet/x64/dotnet run -c Release
```

---

## Key Optimizations

| Technique | Impact | Where |
|-----------|--------|-------|
| Memory-mapped I/O | 2x for scan decode | `open_mmap()` via memmap2 |
| Parallel scan decode | Variable (depends on data layout) | rayon `par_iter` in `scans_parallel()`, `xic()` |
| Scan index pre-filter | Skip scans outside m/z range | `xic()`: check `low_mz`/`high_mz` before decode |
| MS1 filter via trailer | 85x fewer scans to decode | `xic_ms1()`: read "Master Scan Number" (1 i32) |
| **Binary search m/z** | **Up to 200x fewer comparisons per scan** | `sum_intensity_in_range()`: `partition_point` + walk |
| **Sweep-line multi-target** | **O(peaks + targets) vs O(targets * log peaks)** | `xic_batch_ms1()`: sorted ranges walk with centroids |
| **Flat output array** | **Zero per-scan allocation** | `xic_batch_ms1()`: pre-allocated `n_ms1 * n_targets` |
| Batch single-pass | N targets for ~1x cost | `xic_batch_ms1()`: decode once, extract all targets |
| Eager trailer layout | O(1) field access | `TrailerLayout` caches byte offsets at file open |

---

*Last updated: 2026-02-17*
