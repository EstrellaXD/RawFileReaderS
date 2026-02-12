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

Target: 524.2648 m/z, 5 ppm tolerance, NEW file. Wall time includes file open + I/O.

| Mode | Wall time | Scans decoded | vs .NET |
|------|-----------|---------------|---------|
| All scans | 1.21 s | 226,918 | 4.4x |
| **MS1 only** | **0.64 s** | **2,663** | **8.3x** |
| .NET extract_eic | 5.3 s | 2,663 | 1x |

### Batch XIC (MS1 only, single pass)

Multiple m/z targets extracted in one scan pass. File opened once.

| Targets | Wall time | vs .NET | Notes |
|---------|-----------|---------|-------|
| 1 | 0.64 s | 8x | Baseline |
| 3 | 0.65 s | 10x | +2% |
| 10 | 0.66 s | — | +3% |
| .NET (3 targets) | 6.3 s | 1x | 3 sequential API calls |

Batch XIC amortizes scan decode cost: adding targets is nearly free because the scan data decode (the bottleneck) happens once per scan, and the per-target m/z range check is trivially cheap.

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
| Batch single-pass | N targets for ~1x cost | `xic_batch_ms1()`: decode once, extract all targets |
| Eager trailer layout | O(1) field access | `TrailerLayout` caches byte offsets at file open |

---

*Last updated: 2026-02-12*
