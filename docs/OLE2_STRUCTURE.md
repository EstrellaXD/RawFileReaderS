# OLE2 Container Structure

> **Source**: Decompiled ThermoFisher.CommonCore.RawFileReader v8.0.6 (.NET 8).
> Classes: `DataStreamType`, `FieldNames`, `StreamIo`, `StreamSeek`, `DeviceStorage`.

Thermo RAW files use the Microsoft Compound Binary File Format (OLE2/CFBF) as
their outer container. The OLE2 library used is OpenMcdf.

---

## 1. OLE2 Identification

**Magic bytes** (offset 0): `D0 CF 11 E0 A1 B1 1A E1`

The file is a standard OLE2 compound document. The Finnigan binary structures
are stored within the root entry stream and various named sub-streams.

---

## 2. DataStreamType Enum

The internal `DataStreamType` enum defines the logical data streams within
the file. Each maps to a named memory-mapped region or file section:

| Value | Name | Purpose |
|-------|------|---------|
| 0 | InstrumentIdFile | Instrument identification data |
| 1 | StatusLogHeaderFile | Status log field descriptors |
| 2 | StatusLogFile | Status log entries (per-scan instrument state) |
| 3 | ErrorLogFile | Error log entries |
| 4 | DataPacketFile | Raw scan data packets |
| 5 | SpectrumFile | Spectrum/scan index data |
| 6 | ScanEventsFile | Instrument scan event definitions |
| 7 | TrailerExtraHeaderFile | Trailer extra field descriptors |
| 8 | TrailerExtraDataFile | Trailer extra per-scan records |
| 9 | TuneDataHeaderFile | Tune data field descriptors |
| 10 | TuneDataFile | Tune method data |
| 11 | TrailerScanEventsFile | Scan event templates (unique events) |
| 12 | EndOfType | Sentinel |

---

## 3. Stream Name Postfixes

The DLL constructs stream names by combining a device prefix with these
postfixes (from `FieldNames` constants):

| Constant | Postfix String | Maps To |
|----------|---------------|---------|
| MapNameRawFileInfoPostfix | `FMAT_RAWFILEINFO` | RawFileInfo struct |
| MapNameRunHeaderPostfix | `FMAT_RUNHEADER` | RunHeader struct |
| MapNameInsrumentIdPostfix | `INSTID` | Instrument ID |
| MapNameStatusLogHeaderPostfix | `STATUSLOGHEADER` | Status log header |
| MapNameStatusLogEntryPostfix | `STATUS_LOG` | Status log entries |
| MapNameErrorLogEntryPostfix | `ERROR_LOG` | Error log entries |
| MapNamePeakDataPostfix | `PEAKDATA` | Peak/scan data packets |
| MapNameUvScanIndexPostfix | `UVSCANINDEX` | UV detector scan index |
| MapNameMsScanEventsPostfix | `SCANEVENTS` | MS scan events |
| MapNameTrailerHeaderPostfix | `TRAILERHEADER` | Trailer field descriptors |
| MapNameTuneDataHeaderPostfix | `TUNEDATAHEADER` | Tune data header |
| MapNameTuneDataPostfix | `TUNEDATA_FILEMAP` | Tune data records |
| MapNameScanHeaderPostfix | `SCANHEADER` | Scan index header |
| MapNameTrailerScanEventPostfix | `TRAILER_EVENTS` | Scan event templates |
| MapNameTrailerExtraPostfix | `TRAILEREXTRA` | Trailer extra data |
| StreamNameSpectrumFilePostfix | `SPECTRUM` | Spectrum data |

---

## 4. Storage Hierarchy

### Root Entry

The root entry stream contains the main Finnigan data structures in sequence:

```
FileHeader -> SequencerRow -> AutoSamplerInfo -> RawFileInfo ->
  [per-device RunHeaders with embedded stream offsets]
```

### Instrument Storages

Each instrument has a named OLE2 storage (directory) containing:

| Stream | Purpose |
|--------|---------|
| `<InstrumentName>/Data` | Binary instrument method data |
| `<InstrumentName>/Text` | Human-readable method text |
| `<InstrumentName>/Header` | Metadata header (mass analyzer devices) |

Instrument names are device-specific: `LTQ`, `Orbitrap`, `Q Exactive`,
`EksigentNanoLcCom_DLL`, `NanoLC-AS1 Autosampler`, etc.

### Virtual Device Types

Devices are categorized by the `VirtualDeviceTypes` enum:

| Value | Name | Description |
|-------|------|-------------|
| -1 | NoDevice | No device |
| 0 | MsDevice | Mass spectrometer |
| 1 | MsAnalogDevice | MS analog channels |
| 2 | AnalogDevice | Analog input device |
| 3 | PdaDevice | Photodiode array detector |
| 4 | UvDevice | UV/Vis detector |
| 5 | StatusDevice | Instrument status |

The `RawFileInfo.VirtualControllerInfo[]` array maps each virtual device to its
RunHeader offset. Device index 0 is typically the MS device.

---

## 5. Data Stream Layout

Within the main data stream, all Finnigan structures use absolute byte offsets.
The RunHeader contains pointers to the major data sections:

| RunHeader Field | Points To |
|----------------|-----------|
| SpectPos / SpectPos32Bit | ScanIndex array |
| PacketPos / PacketPos32Bit | Scan data packets |
| StatusLogPos / StatusLogPos32Bit | Status log stream |
| ErrorLogPos / ErrorLogPos32Bit | Error log stream |
| TrailerScanEventsPos | Trailer scan event templates |
| TrailerExtraPos | Trailer extra per-scan records |

For v64+, the 64-bit offset fields (`SpectPos`, `PacketPos`, etc.) are
authoritative. For v<=63, 32-bit offsets are used and internally promoted
to 64-bit by `StructureConversion.ConvertFrom32Bit()`.

---

## 6. Memory-Mapped Access (v8.0.6 Architecture)

The v8.0.6 DLL provides two file access strategies:

### RandomAccessRawFileLoader
- Opens file with standard file I/O
- Creates `IRandomAccessViewer` instances for each data section
- Supports sub-views for efficient partial reads
- Used for sequential or selective scan access

### MemoryMappingRawFileLoader
- Maps entire file into virtual memory
- Requires 64-bit process (validated at startup)
- Creates memory-mapped views for each data section
- Optimal for random access patterns across many scans
- Supports real-time acquisition access

Both loaders share the same `LoaderBase` interface and produce identical
decoded results. The memory-mapped loader is preferred for 64-bit applications
accessing large files.
