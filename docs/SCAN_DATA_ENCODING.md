# Scan Data Encoding

> **Source**: Decompiled ThermoFisher.CommonCore.RawFileReader v8.0.6 (.NET 8).
> Classes: `MsScanDecoder`, `AdvancedPacketBase`, `FtProfilePacket`, `FtCentroidPacket`,
> `LinearTrapProfilePacket`, `LinearTrapCentroidPacket`, `HrSpDataPkt`, `LowResSpDataPkt*`,
> `ProfSpPkt*`, `PacketHeaderStruct`, `ProfileSegmentStruct`, `ProfileSubsegmentStruct`.

This document describes how spectral data (profile and centroid) is encoded within
Thermo RAW files. The packet type determines which decoding class is used.

---

## 1. Packet Type Dispatch

Each scan's `ScanIndexEntry.PacketType` field (LOWORD) selects the decoder class.
The full enum is `SpectrumPacketType` (26 values):

| Value | Name | Profile | Centroid | Notes |
|-------|------|---------|----------|-------|
| 0 | ProfileSpectrum | Yes | No | Legacy profile (ProfSpPkt) |
| 1 | LowResolutionSpectrum | No | Yes | Low-res centroids (LowResSpDataPkt) |
| 2 | HighResolutionSpectrum | No | Yes | High-res centroids (HrSpDataPkt) |
| 3 | ProfileIndex | -- | -- | Legacy index-only |
| 4 | CompressedAccurateSpectrum | -- | -- | Not implemented |
| 5 | StandardAccurateSpectrum | No | Yes | Legacy mass lab |
| 6 | StandardUncalibratedSpectrum | -- | -- | Not implemented |
| 7 | AccurateMassProfileSpectrum | -- | -- | Not implemented |
| 8-13 | PDA/UV types | -- | -- | Photodiode array / UV detector |
| 14 | ProfileSpectrumType2 | Yes | No | LCQ profile (ProfSpPkt2) |
| 15 | LowResolutionSpectrumType2 | No | Yes | LCQ centroids (LowResSpDataPkt2) |
| 16 | ProfileSpectrumType3 | Yes | No | Quantum profile (ProfSpPkt3) |
| 17 | LowResolutionSpectrumType3 | No | Yes | Quantum centroids (LowResSpDataPkt3) |
| 18 | LinearTrapCentroid | No | Yes | LT centroid (AdvancedPacketBase) |
| 19 | LinearTrapProfile | Yes | Yes | LT profile + centroid |
| 20 | FtCentroid | No | Yes | FTMS centroid (AdvancedPacketBase) |
| 21 | FtProfile | Yes | Yes | FTMS profile + centroid |
| 22 | HighResolutionCompressedProfile | -- | -- | MAT95 high-res (not implemented) |
| 23 | LowResolutionCompressedProfile | -- | -- | MAT95 low-res (not implemented) |
| 24 | LowResolutionSpectrumType4 | No | Yes | Quantum centroid+flags |
| 25 | InvalidPacket | -- | -- | Sentinel |

Modern instruments (Orbitrap, Exploris, Q Exactive) use types **20** (FtCentroid) and
**21** (FtProfile). Linear trap instruments use types **18** and **19**.
Legacy instruments use types 0-2, 14-17.

Extraction: `packet_type = scan_index.PacketType & 0xFFFF`

---

## 2. Packet Header Formats

Two distinct header formats exist depending on packet type.

### 2a. Legacy Packet Header (40 bytes)

Used by packet types 0-5, 14-17 (ProfSpPkt, HrSpDataPkt, LowResSpDataPkt families).
This is the format described in older documentation and currently implemented in the
Rust reader.

| Offset | Type | Size | Field |
|--------|------|------|-------|
| 0 | u32 | 4 | unknown1 |
| 4 | u32 | 4 | ProfileSize (in 4-byte words) |
| 8 | u32 | 4 | PeakListSize (in 4-byte words) |
| 12 | u32 | 4 | Layout (0=no fudge, >0=with fudge per chunk) |
| 16 | u32 | 4 | DescriptorListSize |
| 20 | u32 | 4 | UnknownStreamSize |
| 24 | u32 | 4 | TripletStreamSize |
| 28 | u32 | 4 | unknown2 |
| 32 | f32 | 4 | LowMZ |
| 36 | f32 | 4 | HighMZ |

After the header, data sections are read sequentially:

1. **Profile data** (`ProfileSize * 4` bytes)
2. **Peak list / centroids** (`PeakListSize * 4` bytes)
3. **Peak descriptors** (`DescriptorListSize` entries)
4. **Unknown stream** (`UnknownStreamSize` entries)
5. **Triplet stream** (`TripletStreamSize` entries)

### 2b. FT/LT Packet Header (PacketHeaderStruct, 32 bytes)

Used by packet types 18-21 (LinearTrap*, FtCentroid, FtProfile).
This is the modern format used by all current instruments.

| Offset | Type | Size | Field |
|--------|------|------|-------|
| 0 | u32 | 4 | NumSegments |
| 4 | u32 | 4 | NumProfileWords (in 4-byte words) |
| 8 | u32 | 4 | NumCentroidWords (in 4-byte words) |
| 12 | u32 | 4 | DefaultFeatureWord (bit flags) |
| 16 | u32 | 4 | NumNonDefaultFeatureWords |
| 20 | u32 | 4 | NumExpansionWords |
| 24 | u32 | 4 | NumNoiseInfoWords |
| 28 | u32 | 4 | NumDebugInfoWords |

After the header:

1. **Segment mass ranges** (`NumSegments * 8` bytes: 2 x f32 per segment)
2. **Profile data** (`NumProfileWords * 4` bytes)
3. **Centroid data** (`NumCentroidWords * 4` bytes)
4. **Non-default features** (`NumNonDefaultFeatureWords * 4` bytes)
5. **Expansion data** (`NumExpansionWords * 4` bytes)
6. **Noise/baseline info** (`NumNoiseInfoWords * 4` bytes)
7. **Debug info** (`NumDebugInfoWords * 4` bytes)

#### DefaultFeatureWord Bit Flags

| Bit | Meaning |
|-----|---------|
| 0x00001 | Charge labels disabled |
| 0x00002 | Reference labels disabled |
| 0x00004 | Merged labels disabled |
| 0x00008 | Fragmented labels disabled |
| 0x00010 | Exception labels disabled |
| 0x00020 | Modified labels disabled |
| 0x00040 | LT (not FT) profile format |
| 0x00080 | FT profile sub-segment format enabled |
| 0x10000 | Accurate mass centroids (f64 precision) |
| 0x20000 | Extended label data format |

---

## 3. FT/LT Profile Encoding

FT and LT profile data use the same structural format, differing only in mass
calibration (FT requires frequency-to-m/z conversion; LT stores m/z directly).

### ProfileSegmentStruct (32 bytes)

One per segment (count from `PacketHeaderStruct.NumSegments`):

| Offset | Type | Size | Field |
|--------|------|------|-------|
| 0 | f64 | 8 | BaseAbscissa (segment start mass or frequency) |
| 8 | f64 | 8 | AbscissaSpacing (mass/frequency step size) |
| 16 | u32 | 4 | NumSubSegments |
| 20 | u32 | 4 | NumExpandedWords |
| 24 | -- | 8 | (padding to 32 bytes) |

### ProfileSubsegmentStruct (8 bytes)

One per sub-segment within a segment:

| Offset | Type | Size | Field |
|--------|------|------|-------|
| 0 | u32 | 4 | StartIndex (first profile word index) |
| 4 | u32 | 4 | WordCount (number of u32 intensity words) |

### Profile Reading Sequence

```
For each segment:
  Read ProfileSegmentStruct (32 bytes)
  For each sub-segment (NumSubSegments):
    Read ProfileSubsegmentStruct (8 bytes)
    Read intensity values: u32[WordCount]
    m/z for point i = BaseAbscissa + (StartIndex + i) * AbscissaSpacing
```

For FTMS instruments, `BaseAbscissa` is a frequency value and must be converted
to m/z using the ScanEvent's mass calibrators (see FORMAT_SPEC.md section 8).

---

## 4. Legacy Profile Encoding

Used by packet types 0, 14, 16 (ProfSpPkt family) with the 40-byte legacy header.

### Profile Header (24 bytes, within profile data region)

| Offset | Type | Size | Field |
|--------|------|------|-------|
| 0 | f64 | 8 | first_value (starting frequency or m/z) |
| 8 | f64 | 8 | step (bin spacing) |
| 16 | u32 | 4 | peak_count (number of chunks) |
| 20 | u32 | 4 | nbins (total bins across all chunks) |

### ProfileChunk (layout == 0, no fudge)

| Offset | Type | Size | Field |
|--------|------|------|-------|
| 0 | u32 | 4 | first_bin |
| 4 | u32 | 4 | nbins |
| 8 | f32[] | 4*nbins | signal (intensities) |

### ProfileChunk (layout > 0, with fudge)

| Offset | Type | Size | Field |
|--------|------|------|-------|
| 0 | u32 | 4 | first_bin |
| 4 | u32 | 4 | nbins |
| 8 | f32 | 4 | fudge (instrument drift correction) |
| 12 | f32[] | 4*nbins | signal (intensities) |

### m/z Reconstruction

For each bin `i` within a chunk:
```
frequency = first_value + (first_bin + i) * step
m/z = frequency_to_mz(frequency, calibrators)
```

Conversion depends on calibrator count:
- **0 params**: frequency IS m/z (direct readout)
- **4 params** (LTQ-FT): `m/z = A / (freq/1e6 + B)`
- **7 params** (Orbitrap): polynomial `m/z = p[0]/f^2 + p[1]/f + p[2] + p[3]*f + ...`

---

## 5. Centroid Encoding

### FT/LT Centroid (packet types 18-21)

For each segment, centroids are stored as label peaks. The format depends on
the `DefaultFeatureWord` flags:

**Standard accuracy** (DefaultFeatureWord & 0x10000 == 0):
- Per peak: `f32 mass` (4 bytes) + `f32 intensity` (4 bytes) = 8 bytes

**Accurate mass** (DefaultFeatureWord & 0x10000 != 0):
- Per peak: `f64 mass` (8 bytes) + `f32 intensity` (4 bytes) = 12 bytes

Centroid count is derived from `NumCentroidWords * 4 / bytes_per_peak`.

#### Non-Default Feature Words (peak annotations)

When `NumNonDefaultFeatureWords > 0`, each feature word encodes per-peak overrides:

| Bits | Field |
|------|-------|
| 0-17 | Peak index (18-bit) |
| 19-23 | Charge state (5-bit) |
| 24+ | Flag bits: Saturated, Fragmented, Merged, Reference, Exception, Modified |

### Legacy Centroid (packet types 1, 15, 17, 24)

**Type 1 (LowResSpDataPkt)** -- 8 bytes per peak:
- Byte 0: Flags (0x80 = intensity overflow)
- Bytes 1-3: Intensity (24-bit unsigned; shifted left 8 if overflow)
- Bytes 4-5: Mass integer part (u16)
- Bytes 6-7: Mass fractional part (u16, divide by 65536.0)
- Mass = `integer + fractional / 65536.0`

**Type 2 (HighResSpDataPkt, HrSpDataPkt)** -- 12 bytes per peak:
- Byte 0: Flags (0x80 = intensity overflow, 0x01 = saturated)
- Bytes 1-3: Intensity (24-bit unsigned; shifted left 8 if overflow)
- Bytes 4-9: Mass (6-byte value decoded via BitConverter.ToDouble after zeroing bytes 4-5)
- Byte 10: Peak width
- Byte 11: Peak options (0x01=saturated, 0x02=fragmented, 0x04=merged, 0x08=modified,
  0x10=reference, 0x20=exception)

**Type 15 (LowResSpDataPkt2, LCQ)** -- 8 bytes per peak:
- Same structure as Type 1 but with intensity scaling
- Bytes 0 bits 0-2: Scale selector (3-bit index)
- Scale array: `[1, 8, 64, 512, 4096, 32768, 262144, 2097152]`
- Intensity = `raw_intensity * Scales[scale_bits]`

**Type 24 (LowResSpDataPkt4, Quantum)** -- 9 bytes per peak:
- Bytes 0-3: Mass (f32)
- Bytes 4-7: Intensity (f32)
- Byte 8: Flags (0x01=saturated, 0x02=fragmented, 0x04=merged, 0x08=reference, 0x40=modified)

### Simple Centroid (from legacy 40-byte header)

Used by the Rust reader's current implementation:

| Offset | Type | Size | Field |
|--------|------|------|-------|
| 0 | u32 | 4 | count |
| 4+ | f32 | 4 | mz (per peak) |
| 4+ | f32 | 4 | intensity (per peak) |

Peaks stored as interleaved (mz, intensity) pairs, each f32.

---

## 6. Noise and Baseline Data

Available in FT/LT packets when `NumNoiseInfoWords > 0`.

### NoiseInfoPacketStruct (12 bytes)

| Offset | Type | Size | Field |
|--------|------|------|-------|
| 0 | f32 | 4 | Mass |
| 4 | f32 | 4 | Noise |
| 8 | f32 | 4 | Baseline |

Count: `NumNoiseInfoWords * 4 / 12` records.

Noise and baseline values are interpolated between measurement points
to produce per-peak noise estimates.

---

## 7. Peak Descriptors

### PeakDescriptor (4 bytes each, legacy format)

| Offset | Type | Size | Field |
|--------|------|------|-------|
| 0 | u16 | 2 | index |
| 2 | u8 | 1 | flags |
| 3 | u8 | 1 | charge |

### Peak Flags

| Bit | Meaning |
|-----|---------|
| 0x01 | Saturated |
| 0x02 | Fragmented |
| 0x04 | Merged |
| 0x08 | Modified |
| 0x10 | Reference peak |
| 0x20 | Exception |

---

## 8. Profile Index (for indexed packet types)

Used by packet types 3, 17, 24 for segment-based profile data.

### ProfileDataPacket64 (version-dependent size)

**v <= 63** (20 bytes):

| Offset | Type | Size | Field |
|--------|------|------|-------|
| 0 | u32 | 4 | DataPos (32-bit position) |
| 4 | f32 | 4 | LowMass |
| 8 | f32 | 4 | HighMass |
| 12 | f64 | 8 | MassTick (spacing between points) |

**v >= 64** (28 bytes):

| Offset | Type | Size | Field |
|--------|------|------|-------|
| 0 | u32 | 4 | DataPos (32-bit, legacy) |
| 4 | f32 | 4 | LowMass |
| 8 | f32 | 4 | HighMass |
| 12 | f64 | 8 | MassTick |
| 20 | i64 | 8 | DataPosOffset (64-bit position) |

---

## 9. Extended Scan Data (v8.0.6 new)

FT/LT packets can include extended data blocks (transient waveforms,
instrument-specific debug data) after the main packet sections.

### Layout

```
[4 bytes] Header (long, count/flags info)
[repeated blocks]:
  [4 bytes] Block header (int; bit 0x100 = transient data)
  [4 bytes] Block size (uint, in bytes for data blocks, in ints for transient)
  [variable] Block data
```

### Transient Blocks (block_header & 0x100)

Raw time-domain detector data. Format is instrument-specific.
Data: `int[block_size]` (32-bit samples).

### Data Blocks (block_header & 0x100 == 0)

Arbitrary instrument data. Format is instrument-specific.
Data: `byte[block_size]`.

---

## 10. Compression

The decompiled v8.0.6 source contains no LZF, Zlib, or other decompression logic
for scan data packets. Several packet types are marked "compressed" in the enum
(types 4, 6, 7, 22, 23) but throw `NotImplementedException` when accessed.

Intensity overflow is handled via a simple bit flag (0x80 in the flags byte)
that causes the 24-bit intensity value to be left-shifted by 8 bits, effectively
extending the dynamic range to 32 bits.
