# Thermo RAW Binary Format Findings

Discoveries from reverse-engineering v66 Thermo RAW files. Cross-validated against two files and the Thermo .NET RawFileReader library (via pythonnet):
- **OLD**: `exp005_250130_andrii_ex5_blank_002.raw` (413MB, v66, 34475 MS scans, RT 0-21.94 min)
- **NEW**: `exp246_260209_SK_A1B1_SAH_5nM_07.raw` (928MB, v66, 226918 MS scans, Orbitrap Astral)

---

## 1. Device Type Enum (CRITICAL)

The VCI DeviceType values match the Thermo .NET `Device` enum (confirmed via pythonnet):

| Value | Device Type | Notes |
|-------|-------------|-------|
| -1    | None        | |
| **0** | **MS**      | Mass spectrometer |
| 1     | MSAnalog    | |
| 2     | Analog      | e.g., LC pump pressure, TCC temperature |
| 3     | UV          | |
| 4     | Pda         | |
| 5     | Other       | |

**WARNING**: The decompiled VirtualDeviceType enum had WRONG values (0=NoDevice, 2=MS). The correct mapping is 0=MS.

### Evidence

Both files have VCI entries with DevType=0 for MS data:
- OLD: VCI[3] DevType=0, DevIdx=3, Offset=357186332 → 34475 MS scans
- NEW: VCI[3] DevType=0, DevIdx=3, Offset=562179414 → 226918 MS scans

DevType=2 entries are Analog controllers (LC pump, TCC), confirmed via Thermo .NET:
- OLD: 2 Analog controllers (223 scans each, TIC=0 for blank)
- NEW: Analog[1] = TCC (83 scans), Analog[2] = BinaryPump (108001 scans)

---

## 2. RunHeader Address Block (v64+)

The 64-bit address block is 7 consecutive i64 values at RunHeader + 7408:

| Offset | Field | Notes |
|--------|-------|-------|
| rh+7408 | SpectPos | Scan index stream address |
| rh+7416 | PacketPos | Data stream base address |
| rh+7424 | StatusLogPos | |
| rh+7432 | ErrorLogPos | |
| rh+7440 | RunHeaderPos | **Often 0 in v66 files** |
| rh+7448 | TrailerScanEventsPos | Non-zero for MS controllers |
| rh+7456 | TrailerExtraPos | Non-zero for MS controllers |

Immediately after (rh+7464): VCI struct = DeviceType(i32) + DeviceIndex(i32) + Offset(i64).
**VCI.Offset always contains the correct RunHeader start address**, even when RunHeaderPos=0.

### Finding: RunHeaderPos=0 is common

Both test files have RunHeaderPos=0 for their MS controllers. The `find_address_block` function uses VCI.Offset (at block+64) as a fallback.

---

## 3. ScanIndex Entry Size

### MS Controllers: 88 bytes (documented is correct)

Tested stride values 72, 80, 88 on the MS ScanIndex:
- **88-byte stride**: Perfect RT progression, sequential scan numbers, all fields valid
- **72-byte stride**: Entry[0] ok but Entry[1+] are garbage
- **80-byte stride**: Garbage

### Analog Controllers: 72 bytes (documented is wrong)

Analog controllers use 72-byte entries even for v66.

### Conclusion

Entry size is controller-dependent. The `detect_entry_size()` function validates stride by checking RT monotonicity at offset +24.

---

## 4. ScanIndex Entry Field Layout (88-byte entries, MS controller)

| Byte Offset | Type | Field | Evidence |
|-------------|------|-------|----------|
| +0  | u32 | DataSize | Always 0 for v66 files tested |
| +4  | i32 | TrailerOffset | Incrementing 0, 1, 2, ... |
| +8  | u16 | ScanEvent | Scan type index (0=MS1, 1=MS2, etc.) |
| +10 | u16 | ScanSegment | Usually 0 |
| +12 | i32 | ScanNumber | 1, 2, 3, ... |
| +16 | u32 | PacketType | 0x15 for FT, 0x14 for LT |
| +20 | i32 | NumberPackets | Data word count |
| +24 | f64 | RetentionTime | Monotonically increasing (minutes) |
| +32 | f64 | TIC | Total ion current |
| +40 | f64 | BasePeakIntensity | |
| +48 | f64 | BasePeakMass | |
| +56 | f64 | LowMass | |
| +64 | f64 | HighMass | |
| +72 | i64 | DataOffset64 | **Relative to PacketPos** |
| +80 | i32 | CycleNumber | Groups scans into acquisition cycles |
| +84 | 4 bytes | Padding | Struct alignment |

### Key confirmation

- OLD file: Scan 1 TIC=8.40e7, BPM=74.096, mass 70-1000 → matches Thermo .NET exactly
- NEW file: Scan 1 TIC=3.00e8, m/z 70.009-959.884 → matches Thermo .NET exactly
- DataOffset64 is RELATIVE to PacketPos: abs_offset = PacketPos + DataOffset64

---

## 5. DataOffset Interpretation

For ALL ScanIndex entry sizes:
- DataOffset is **relative to PacketPos**, not absolute
- Absolute scan data offset = PacketPos + DataOffset
- For 72-byte entries: DataOffset is u32 at +0
- For 88-byte entries: DataOffset is i64 at +72

---

## 6. Multi-Controller Files

Both files have 4 controllers. Controller info from Thermo .NET:

**OLD file** (blank run):
- MS[1]: 34475 scans (mass 70-1000) — **DevType=0, VCI[3]**
- Analog[1]: 223 scans (TIC=0) — DevType=2, VCI[0]
- Analog[2]: 223 scans (TIC=0) — DevType=2, VCI[1]
- Other[1]: dummy — DevType=5, VCI[2]

**NEW file** (Orbitrap Astral):
- MS[1]: Orbitrap Astral, 226918 scans (mass 40-1000) — **DevType=0, VCI[3]**
- Analog[1]: TCC VH-C10-A, 83 scans — DevType=2, VCI[0]
- Analog[2]: BinaryPump VH-P10-A, 108001 scans — DevType=2, VCI[1]
- Other[1]: dummy — DevType=5, VCI[2]

---

## 7. .NET Blob Artifacts

Between FileHeader (2384 bytes) and RawFileInfo, there are .NET serialized blobs of variable size:
- OLD file: 1104 bytes of blobs (RawFileInfo at offset 3488)
- NEW file: 1034 bytes of blobs (RawFileInfo at offset 3418)

The blob size varies between files. The RawFileInfo search must use 2-byte alignment steps (not 4-byte) because blob sizes are not aligned.

---

## 8. File Structure (v66, non-OLE2)

v66 files start with "Finnigan" magic directly (not OLE2 container). Layout:

```
[0]         FileHeader (2384 bytes)
[2384]      .NET serialized blobs (variable size, ~1KB)
[varies]    RawFileInfo (contains VCI array, NewVCI[64])
[varies]    RunHeader(s) for each controller (near end of file, ~85%)
[SpectPos]  ScanIndex stream (n_scans * entry_size bytes)
[PacketPos] Data stream (scan packets)
```

RunHeaders are typically near the end of the file (85-90% of file size).

---

## 9. Trailer Stream Layout (v66)

TrailerScanEventsPos and TrailerExtraPos point to **flat per-scan arrays** (no count header):

- **TrailerScanEventsPos** = SpectPos + ScanIndex size (immediately after ScanIndex)
  - Contains per-scan ScanEvent structures (~272 bytes/scan for v66)
  - NOT preceded by a u32 count; length is implicit from n_scans
- **TrailerExtraPos** = after TrailerScanEvents area
  - Contains per-scan trailer records (filter text, charge state, etc.)
  - NOT preceded by a GenericDataHeader; the header is stored separately

Size calculations confirm this:
- OLD: TrailerScanEventsPos = 360501617 = SpectPos(357467817) + 34475*88
- NEW: TrailerScanEventsPos = 582478155 = SpectPos(562509371) + 226918*88

### GenericDataHeader Location (CONFIRMED)

The GenericDataHeader (field descriptors for trailer records) is stored **~18KB before SpectPos**, NOT at TrailerScanEventsPos or TrailerExtraPos. For the NEW file:

```
[562491092]    GenericDataHeader (86 field descriptors, ~4KB)
[~562495xxx]   Tune data (~14KB)
[562509371]    SpectPos → ScanIndex
[582478155]    TrailerScanEventsPos → per-scan ScanEvent records (flat, 272B each)
[644199855]    TrailerExtraPos → per-scan trailer records (flat, 1252B each)
```

**Discovery method**: Scan forward from (SpectPos - 20KB) looking for a u32 in [10, 300] followed by valid field descriptors (type codes in {0x00, 0x03, 0x04, 0x08, 0x0B, 0x0C}).

**GDH type codes (v66)**:

| Code | Type | Size | Notes |
|------|------|------|-------|
| 0x00 | Separator | 0B | Section label only (e.g. "=== Mass Calibration: ===") |
| 0x03 | Boolean | 1B | u8 |
| 0x04 | Flag | 1B | u8 small int |
| 0x08 | Integer | 4B | i32 |
| 0x0B | Double | 8B | f64 |
| 0x0C | ASCII | variable | Length from descriptor |

**Status**: Trailer extra parsing is WORKING. MS level determination uses "Master Scan Number" field (>0 → MS2).
Per-scan ScanEvent parsing from TrailerScanEventsPos is not yet implemented.

---

## 10. Performance Benchmarks

Tested on Apple Silicon (M-series), release build, comparing Rust parser vs Thermo .NET RawFileReader (via Mono/pythonnet):

### NEW file: 928MB, 226,918 scans (Orbitrap Astral)

| Mode | Time | Scans/sec | vs .NET |
|------|------|-----------|---------|
| Rust sequential, read | 76ms | 2,984K | 93x |
| Rust sequential, mmap | 64ms | 3,569K | 111x |
| Rust parallel, read | 69ms | 3,287K | 102x |
| Rust parallel, mmap | **47ms** | **4,826K** | **150x** |
| .NET GetCentroidStream | 7,067ms | 32K | 1x |
| .NET GetScanStats (headers only) | 2,672ms | 85K | -- |

### OLD file: 413MB, 34,475 scans (blank run)

| Mode | Time | Scans/sec |
|------|------|-----------|
| Rust sequential, mmap | 365ms | 94K |
| Rust parallel, mmap | **13ms** | **2,600K** |

**Summary**: Rust parser is 100-150x faster than the official .NET library for full scan data reads.

### XIC (Extracted Ion Chromatogram) benchmarks

Target: 524.2648 m/z @ 5 ppm, NEW file (928MB, 226918 total scans, 2663 MS1 scans).

| Mode | Wall time | Notes | vs .NET |
|------|-----------|-------|---------|
| Rust XIC (all scans) | 1.19s | Decodes all 226K scans | 4.5x |
| **Rust XIC (MS1 only)** | **0.65s** | Skips MS2 via trailer | **8x** |
| **Rust batch XIC (3 targets, MS1)** | **0.64s** | Single-pass decode | **10x** |
| .NET extract_eic (1 target) | 5.3s | MS1 only (filter="ms") | 1x |
| .NET extract_eic (3 targets) | 6.3s | Sequential, 3 API calls | 1x |

MS1 detection uses trailer "Master Scan Number" field (one i32 read per scan, no data decoding).
Batch XIC amortizes scan decoding: 3 targets cost the same as 1 target.

---

*Last updated: 2026-02-12*
