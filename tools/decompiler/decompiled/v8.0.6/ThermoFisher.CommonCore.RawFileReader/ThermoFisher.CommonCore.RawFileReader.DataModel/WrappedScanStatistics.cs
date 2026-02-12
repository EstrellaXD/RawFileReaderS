using System;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// The wrapped scan statistics.
/// Converts from internal data to public type.
/// </summary>
internal class WrappedScanStatistics : ScanStatistics
{
	/// <summary>
	/// Sets the lazy scan type.
	/// </summary>
	public new Lazy<string> LazyScanType
	{
		set
		{
			base.LazyScanType = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedScanStatistics" /> class.
	/// </summary>
	/// <param name="scanIndex">Index of the scan.</param>
	/// <exception cref="T:System.ArgumentNullException">Thrown if scanIndex is null</exception>
	public WrappedScanStatistics(ScanIndex scanIndex)
	{
		if (scanIndex == null)
		{
			throw new ArgumentNullException("scanIndex");
		}
		CopyFrom(scanIndex);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedScanStatistics" /> class.
	/// </summary>
	/// <param name="scanIndex">Index of the scan.</param>
	/// <exception cref="T:System.ArgumentNullException">Thrown if scanIndex is null</exception>
	public WrappedScanStatistics(ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex scanIndex)
	{
		if (scanIndex == null)
		{
			throw new ArgumentNullException("scanIndex");
		}
		CopyFrom(scanIndex);
	}

	/// <summary>
	/// Copies from.
	/// </summary>
	/// <param name="scanIndex">Index of the scan.</param>
	private void CopyFrom(ScanIndex scanIndex)
	{
		ScanIndexStruct scanIndexStructInfo = scanIndex.ScanIndexStructInfo;
		base.PacketType = (int)scanIndexStructInfo.PacketType;
		base.HighMass = scanIndexStructInfo.HighMass;
		base.LowMass = scanIndexStructInfo.LowMass;
		base.BasePeakIntensity = scanIndexStructInfo.BasePeakIntensity;
		base.BasePeakMass = scanIndexStructInfo.BasePeakMass;
		base.TIC = scanIndexStructInfo.TIC;
		base.StartTime = scanIndexStructInfo.StartTime;
		base.PacketCount = scanIndexStructInfo.NumberPackets;
		base.ScanNumber = scanIndexStructInfo.ScanNumber;
		base.IsCentroidScan = scanIndex.PacketType.IsCentroidScan(scanIndex.HighPacketTypeWord);
		base.SegmentNumber = scanIndex.ScanSegment;
		base.ScanEventNumber = scanIndexStructInfo.ScanTypeIndex;
		base.CycleNumber = scanIndexStructInfo.CycleNumber;
		base.Frequency = 0.0;
		base.IsUniformTime = false;
		base.WavelengthStep = 0.0;
		base.AbsorbanceUnitScale = 0.0;
	}

	/// <summary>
	/// Copies from.
	/// </summary>
	/// <param name="scanIndex">Index of the scan.</param>
	private void CopyFrom(ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex scanIndex)
	{
		UvScanIndexStruct uvScanIndexStructInfo = scanIndex.UvScanIndexStructInfo;
		base.PacketCount = uvScanIndexStructInfo.NumberPackets;
		base.PacketType = uvScanIndexStructInfo.PacketType;
		base.ScanNumber = uvScanIndexStructInfo.ScanNumber;
		base.StartTime = uvScanIndexStructInfo.StartTime;
		base.TIC = uvScanIndexStructInfo.TIC;
		base.NumberOfChannels = uvScanIndexStructInfo.NumberOfChannels;
		base.ShortWavelength = uvScanIndexStructInfo.ShortWavelength;
		base.LongWavelength = uvScanIndexStructInfo.LongWavelength;
		base.IsCentroidScan = scanIndex.PacketType.IsCentroidScan(0u);
		base.Frequency = scanIndex.Frequency;
		base.IsUniformTime = scanIndex.IsUniformTime;
		base.WavelengthStep = 0.0;
		base.AbsorbanceUnitScale = 0.0;
	}
}
