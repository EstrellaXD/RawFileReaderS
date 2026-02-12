"""Export ground truth data from a Thermo RAW file using the official library.

Works with either coreclr (.NET Core) or Mono (macOS):
    # macOS (Mono via rfr conda env):
    conda activate rfr
    python export_ground_truth.py <raw_file> <output_dir>

    # Windows/Linux (.NET Core):
    python export_ground_truth.py <raw_file> <output_dir>

Exports JSON files for Rust integration test validation:
    - metadata.json: file version, scan counts, time/mass ranges
    - scan_index.json: per-scan RT, TIC, base peak, packet type, ms_level, polarity
    - scan_events.json: per-event activation type, analyzer, ionization, ms_level
    - centroids/scan_{N}.json: m/z + intensity arrays for representative scans
"""

from __future__ import annotations

import json
import os
import sys
from pathlib import Path

# Load pythonnet runtime: prefer coreclr, fall back to mono
try:
    from pythonnet import load
    load("coreclr")
except Exception:
    pass  # Mono-based pythonnet loads automatically

import clr

# Search for Thermo DLLs in known locations
LIB_SEARCH_PATHS = [
    Path(__file__).resolve().parents[2] / "src" / "RawFileReader" / "lib",
    Path(__file__).resolve().parents[3] / "src" / "RawFileReader" / "lib",
    Path(os.path.expanduser("~/Developer/MorscherLab/MassSpec/RawFileReader/Libs/Net471")),
]

for lib_path in LIB_SEARCH_PATHS:
    if lib_path.exists():
        sys.path.append(str(lib_path))
        break
else:
    print("Warning: Could not find Thermo DLL directory")

clr.AddReference("ThermoFisher.CommonCore.Data")
clr.AddReference("ThermoFisher.CommonCore.RawFileReader")

from System import *
from ThermoFisher.CommonCore.Data.Business import Device
from ThermoFisher.CommonCore.Data.FilterEnums import (
    IonizationModeType,
    MassAnalyzerType,
    MSOrderType,
)
from ThermoFisher.CommonCore.Data.Interfaces import IScanEventBase, IScanFilter
from ThermoFisher.CommonCore.RawFileReader import RawFileReaderAdapter


def export_metadata(raw_file, output_dir: Path) -> dict:
    """Export file-level metadata."""
    run_header = raw_file.RunHeaderEx
    file_header = raw_file.FileHeader

    metadata = {
        "file_version": int(file_header.Revision),
        "first_scan": int(run_header.FirstSpectrum),
        "last_scan": int(run_header.LastSpectrum),
        "n_scans": int(run_header.LastSpectrum - run_header.FirstSpectrum + 1),
        "start_time": float(run_header.StartTime),
        "end_time": float(run_header.EndTime),
        "low_mass": float(run_header.LowMass),
        "high_mass": float(run_header.HighMass),
        "mass_resolution": float(run_header.MassResolution),
        "instrument_model": str(raw_file.GetInstrumentData().Model),
        "instrument_name": str(raw_file.GetInstrumentData().Name),
        "serial_number": str(raw_file.GetInstrumentData().SerialNumber),
    }

    with open(output_dir / "metadata.json", "w") as f:
        json.dump(metadata, f, indent=2)
    print(f"  Exported metadata: {metadata['n_scans']} scans, v{metadata['file_version']}")
    return metadata


def export_scan_index(raw_file, first_scan: int, last_scan: int, output_dir: Path):
    """Export per-scan index data (RT, TIC, base peak, ms_level, polarity)."""
    entries = []
    for scan_num in range(first_scan, last_scan + 1):
        stats = raw_file.GetScanStatsForScanNumber(scan_num)
        scan_filter = IScanFilter(raw_file.GetFilterForScanNumber(scan_num))

        ms_order_enum = scan_filter.MSOrder
        ms_order = int(ms_order_enum)
        # MSOrderType.Ms = 1, Ms2 = 2, etc.
        if ms_order_enum == MSOrderType.Ms:
            ms_level = 1
        elif ms_order_enum == MSOrderType.Ms2:
            ms_level = 2
        elif ms_order_enum == MSOrderType.Ms3:
            ms_level = 3
        else:
            ms_level = ms_order

        polarity_str = str(scan_filter.Polarity)
        polarity = "positive" if polarity_str == "Positive" else "negative"

        analyzer_str = str(scan_filter.MassAnalyzer) if hasattr(scan_filter, "MassAnalyzer") else "Unknown"
        is_centroid = bool(stats.IsCentroidScan)

        entry = {
            "scan_number": scan_num,
            "rt": float(raw_file.RetentionTimeFromScanNumber(scan_num)),
            "tic": float(stats.TIC),
            "base_peak_mz": float(stats.BasePeakMass),
            "base_peak_intensity": float(stats.BasePeakIntensity),
            "ms_level": ms_level,
            "polarity": polarity,
            "analyzer": analyzer_str,
            "is_centroid": is_centroid,
            "filter_string": str(scan_filter.ToString()) if scan_filter else "",
        }
        entries.append(entry)

    with open(output_dir / "scan_index.json", "w") as f:
        json.dump(entries, f, indent=2)
    print(f"  Exported scan index: {len(entries)} entries")


def export_scan_events(raw_file, first_scan: int, last_scan: int, output_dir: Path):
    """Export scan event metadata (activation, analyzer, ionization per scan)."""
    events = []
    for scan_num in range(first_scan, last_scan + 1):
        scan_filter = IScanFilter(raw_file.GetFilterForScanNumber(scan_num))
        scan_event = IScanEventBase(raw_file.GetScanEventForScanNumber(scan_num))

        ms_order_enum = scan_filter.MSOrder
        if ms_order_enum == MSOrderType.Ms:
            ms_level = 1
        elif ms_order_enum == MSOrderType.Ms2:
            ms_level = 2
        elif ms_order_enum == MSOrderType.Ms3:
            ms_level = 3
        else:
            ms_level = int(ms_order_enum)

        # Get activation type from reaction if MS2+
        activation_type = "None"
        collision_energy = 0.0
        precursor_mz = 0.0
        isolation_width = 0.0
        if ms_level > 1 and scan_event is not None:
            try:
                reaction = scan_event.GetReaction(0)
                activation_type = str(reaction.ActivationType) if reaction else "Unknown"
                collision_energy = float(reaction.CollisionEnergy) if reaction else 0.0
                precursor_mz = float(reaction.PrecursorMass) if reaction else 0.0
                isolation_width = float(reaction.IsolationWidth) if reaction else 0.0
            except Exception:
                pass

        event = {
            "scan_number": scan_num,
            "ms_level": ms_level,
            "activation_type": activation_type,
            "collision_energy": collision_energy,
            "precursor_mz": precursor_mz,
            "isolation_width": isolation_width,
            "analyzer": str(scan_filter.MassAnalyzer) if hasattr(scan_filter, "MassAnalyzer") else "Unknown",
            "ionization": str(scan_filter.IonizationMode) if hasattr(scan_filter, "IonizationMode") else "Unknown",
            "polarity": str(scan_filter.Polarity),
        }
        events.append(event)

    with open(output_dir / "scan_events.json", "w") as f:
        json.dump(events, f, indent=2)
    print(f"  Exported scan events: {len(events)} entries")


def select_representative_scans(raw_file, first_scan: int, last_scan: int) -> list[int]:
    """Select ~10 representative scans covering different scan types."""
    ms1_scans = []
    ms2_scans = []
    ms2_centroid = []
    ms1_profile = []

    for scan_num in range(first_scan, last_scan + 1):
        stats = raw_file.GetScanStatsForScanNumber(scan_num)
        scan_filter = IScanFilter(raw_file.GetFilterForScanNumber(scan_num))
        ms_order = scan_filter.MSOrder
        is_centroid = bool(stats.IsCentroidScan)

        if ms_order == MSOrderType.Ms:
            ms1_scans.append(scan_num)
            if not is_centroid:
                ms1_profile.append(scan_num)
        elif ms_order == MSOrderType.Ms2:
            ms2_scans.append(scan_num)
            if is_centroid:
                ms2_centroid.append(scan_num)

    selected = set()
    # First and last scan
    selected.add(first_scan)
    selected.add(last_scan)
    # First MS1 and some spread
    if ms1_scans:
        selected.add(ms1_scans[0])
        selected.add(ms1_scans[len(ms1_scans) // 2])
    # First MS1 profile
    if ms1_profile:
        selected.add(ms1_profile[0])
    # A few MS2 scans spread across the run
    if ms2_centroid:
        selected.add(ms2_centroid[0])
        if len(ms2_centroid) > 2:
            selected.add(ms2_centroid[len(ms2_centroid) // 4])
            selected.add(ms2_centroid[len(ms2_centroid) // 2])
            selected.add(ms2_centroid[3 * len(ms2_centroid) // 4])
        selected.add(ms2_centroid[-1])

    result = sorted(selected)
    print(f"  Selected {len(result)} representative scans: {result}")
    return result


def export_centroids(raw_file, scan_numbers: list[int], output_dir: Path):
    """Export centroid m/z and intensity arrays for selected scans."""
    centroids_dir = output_dir / "centroids"
    centroids_dir.mkdir(exist_ok=True)

    for scan_num in scan_numbers:
        stats = raw_file.GetScanStatsForScanNumber(scan_num)
        scan_filter = IScanFilter(raw_file.GetFilterForScanNumber(scan_num))

        mz_array = []
        intensity_array = []

        if stats.IsCentroidScan:
            centroid_stream = raw_file.GetCentroidStream(scan_num, False)
            if centroid_stream is not None and centroid_stream.Length > 0:
                mz_array = [float(m) for m in centroid_stream.Masses]
                intensity_array = [float(i) for i in centroid_stream.Intensities]
        else:
            # For profile scans, get the segmented scan
            seg_scan = raw_file.GetSegmentedScanFromScanNumber(scan_num, stats)
            if seg_scan is not None:
                mz_array = [float(m) for m in seg_scan.Positions]
                intensity_array = [float(i) for i in seg_scan.Intensities]

        ms_order = scan_filter.MSOrder
        if ms_order == MSOrderType.Ms:
            ms_level = 1
        elif ms_order == MSOrderType.Ms2:
            ms_level = 2
        else:
            ms_level = int(ms_order)

        scan_data = {
            "scan_number": scan_num,
            "ms_level": ms_level,
            "is_centroid": bool(stats.IsCentroidScan),
            "n_peaks": len(mz_array),
            "filter_string": str(scan_filter.ToString()),
            "rt": float(raw_file.RetentionTimeFromScanNumber(scan_num)),
            "tic": float(stats.TIC),
            "mz": mz_array,
            "intensity": intensity_array,
        }

        filename = f"scan_{scan_num}.json"
        with open(centroids_dir / filename, "w") as f:
            json.dump(scan_data, f, indent=2)
        print(f"    Scan {scan_num}: {len(mz_array)} peaks, MS{ms_level}, "
              f"{'centroid' if stats.IsCentroidScan else 'profile'}")


def main():
    if len(sys.argv) < 3:
        print(f"Usage: {sys.argv[0]} <raw_file> <output_dir>")
        sys.exit(1)

    raw_path = Path(sys.argv[1]).resolve()
    output_dir = Path(sys.argv[2]).resolve()
    output_dir.mkdir(parents=True, exist_ok=True)

    if not raw_path.exists():
        print(f"Error: RAW file not found: {raw_path}")
        sys.exit(1)

    print(f"Opening: {raw_path}")
    raw_file = RawFileReaderAdapter.FileFactory(str(raw_path))
    if not raw_file.IsOpen:
        print(f"Error: Failed to open RAW file")
        sys.exit(1)

    raw_file.SelectInstrument(Device.MS, 1)

    print("\n1. Exporting metadata...")
    metadata = export_metadata(raw_file, output_dir)

    first_scan = metadata["first_scan"]
    last_scan = metadata["last_scan"]

    print("\n2. Exporting scan index...")
    export_scan_index(raw_file, first_scan, last_scan, output_dir)

    print("\n3. Exporting scan events...")
    export_scan_events(raw_file, first_scan, last_scan, output_dir)

    print("\n4. Selecting representative scans...")
    representative = select_representative_scans(raw_file, first_scan, last_scan)

    print("\n5. Exporting centroid/profile data...")
    export_centroids(raw_file, representative, output_dir)

    raw_file.Dispose()
    print(f"\nDone! Ground truth data exported to: {output_dir}")


if __name__ == "__main__":
    main()
