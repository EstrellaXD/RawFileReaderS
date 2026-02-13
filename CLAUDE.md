# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

RawFileReaderS is a pure Rust reader for Thermo Scientific RAW mass spectrometry files. It parses the OLE2 binary container format without requiring .NET, achieving 100-150x faster scan decoding than the official library.

## Build & Development Commands

```bash
# Build
cargo build --release                    # Full workspace (release required for perf)
cargo build -p thermo-raw-cli --release  # CLI only

# Test
cargo test --all                         # All workspace tests
cargo test -p thermo-raw                 # Core library tests
cargo test -p thermo-raw --test synthetic_tests  # No test data needed
cargo test -p thermo-raw --test ground_truth_tests -- --ignored --nocapture  # Requires RAW files in test-data/

# Lint
cargo clippy --all --all-targets
cargo fmt --check
cargo fmt --all

# Python bindings (excluded from workspace, built separately)
cd crates/thermo-raw-py && maturin develop --release

# CLI usage
cargo run --release -p thermo-raw-cli -- <subcommand> [args]
# Subcommands: info, scan, tic, xic, batch-xic, convert, streams, trailer, validate, benchmark, debug, diagnose
```

## Workspace Structure

```
crates/
  cfb-reader/          # Thin wrapper over `cfb` crate for OLE2 compound file access
  thermo-raw/          # Core library - all parsing logic lives here
  thermo-raw-cli/      # CLI binary (clap subcommands in single main.rs)
  thermo-raw-mzml/     # RAW-to-mzML conversion (quick-xml writer + base64/zlib encoding)
  thermo-raw-py/       # PyO3 Python bindings (EXCLUDED from workspace, uses maturin)
tools/
  hex-analyzer/        # Binary format reverse engineering helper
```

## Architecture

### Binary Format Parsing Pipeline

RAW files are OLE2 compound containers. Parsing follows this chain:

1. **FileHeader** (2402 bytes at offset 0) - magic `0xA101`, "Finnigan" signature, version, checksums
2. **RawFileInfo** - acquisition metadata, VirtualControllerInfo array pointing to RunHeaders
3. **RunHeader** - per-instrument index: addresses to ScanIndex, DataStream, TrailerExtra, ScanParams
4. **ScanIndex** (eagerly parsed) - array of ScanIndexEntry with RT, TIC, offsets per scan
5. **TrailerLayout** (eagerly parsed) - field offset cache for O(1) per-scan metadata access
6. **ScanData** (lazily decoded per scan) - dispatched by PacketType to centroid/profile/FT/LT decoders
7. **ScanEvent** (lazily parsed) - polarity, analyzer, MS level, precursor, activation info

### Version-Aware Parsing

Supports RAW format versions 57-66. Key version boundaries:
- **v64+**: 64-bit addresses (vs 32-bit), larger ScanIndexEntry (80 bytes vs 72)
- **v65+**: 88-byte ScanIndexEntry, extra RawFileInfo fields, larger ScanEvent/Reaction structs

Version dispatch is centralized in `version.rs` (`scan_index_entry_size()`, `uses_64bit_addresses()`, etc.).

### Key Design Patterns

- **`RawFile`** is the main entry point. Supports two modes: `open()` (read into memory) and `open_mmap()` (memory-mapped, 2x faster for large files)
- **Scan data decoding** routes through `scan_data.rs` which dispatches to `scan_data_centroid.rs`, `scan_data_ftlt.rs`, or `scan_data_profile.rs` based on PacketType
- **`BinaryReader`** (`io_utils.rs`) provides bounds-checked binary reading with UTF-16LE PascalString support
- **Parallel processing** via rayon: `scans_parallel()`, `batch_xic_ms1()`, mzML folder conversion
- **TIC is index-based** (sub-millisecond, no scan decode needed); XIC requires scanning through data

### Error Handling

`RawError` enum in `error.rs`. Uses `thiserror` for derivation, `anyhow` in CLI/tools for context.

## CI/CD

GitHub Actions release workflow (`.github/workflows/release.yml`) triggers on `v*` tags:
- Builds CLI binaries for 5 platforms (linux x86_64/aarch64, macOS x86_64/aarch64, Windows x86_64)
- Builds Python wheels via maturin for Python 3.9-3.13
- Pre-release tags: `-beta`, `-rc`

## Core Types

All in `thermo-raw/src/types.rs`:
- `Scan` - fully decoded scan with centroid/profile arrays, precursor info, filter string
- `Chromatogram` - paired rt/intensity vectors
- `FileMetadata` - instrument model, serial, software version, sample name
- `PrecursorInfo` - m/z, charge, isolation width, activation, collision energy
- `MsLevel` (Ms1/Ms2/Ms3/Other), `Polarity` (Positive/Negative/Unknown)

## Workspace Clippy Lints

`redundant_clone`, `large_enum_variant`, `needless_collect` are warned. The `perf` lint group is enabled.
