using System;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
/// The scan index.
/// </summary>
internal sealed class ScanIndex : IScanIndex, ISimpleScanHeader, IMsScanIndexAccess
{
	private ScanIndexStruct _scanIndexStructInfo;

	/// <summary>
	/// Gets or sets the scan index struct info.
	/// </summary>
	public ScanIndexStruct ScanIndexStructInfo
	{
		get
		{
			return _scanIndexStructInfo;
		}
		set
		{
			_scanIndexStructInfo = value;
		}
	}

	public uint ScanByteLength => _scanIndexStructInfo.DataSize;

	/// <summary>
	/// Gets the base peak intensity.
	/// </summary>
	public double BasePeakIntensity => _scanIndexStructInfo.BasePeakIntensity;

	/// <summary>
	/// Gets the base peak mass.
	/// </summary>
	public double BasePeakMass => _scanIndexStructInfo.BasePeakMass;

	/// <summary>
	/// Gets the data offset.
	/// </summary>
	public long DataOffset => _scanIndexStructInfo.DataOffset;

	/// <summary>
	/// Gets the high mass.
	/// </summary>
	public double HighMass => _scanIndexStructInfo.HighMass;

	/// <summary>
	/// Gets a value indicating whether is scan type index specified.
	/// </summary>
	public bool IsScanTypeIndexSpecified => _scanIndexStructInfo.ScanTypeIndex != -1;

	/// <summary>
	/// Gets the low mass.
	/// </summary>
	public double LowMass => _scanIndexStructInfo.LowMass;

	/// <summary>
	/// Gets the number packets.
	/// </summary>
	public int NumberPackets => _scanIndexStructInfo.NumberPackets;

	/// <summary>
	/// Gets the packet type.
	/// </summary>
	public SpectrumPacketType PacketType => (SpectrumPacketType)(_scanIndexStructInfo.PacketType & 0xFFFF);

	/// <summary>
	/// Gets meta data about the packet type.
	/// </summary>
	public uint HighPacketTypeWord => _scanIndexStructInfo.PacketType >> 16;

	/// <summary>
	/// Gets the scan number.
	/// </summary>
	public int ScanNumber => _scanIndexStructInfo.ScanNumber;

	/// <summary>
	/// Gets the scan segment.
	/// </summary>
	public int ScanSegment
	{
		get
		{
			if (!IsScanTypeIndexSpecified)
			{
				return 0;
			}
			return _scanIndexStructInfo.ScanTypeIndex >> 16;
		}
	}

	/// <summary>
	/// Gets the scan type index.
	/// </summary>
	public int ScanTypeIndex
	{
		get
		{
			if (!IsScanTypeIndexSpecified)
			{
				return 0;
			}
			return _scanIndexStructInfo.ScanTypeIndex & 0xFFFF;
		}
	}

	/// <summary>
	/// Gets the start time.
	/// </summary>
	public double StartTime => _scanIndexStructInfo.StartTime;

	/// <summary>
	/// Gets the tic.
	/// </summary>
	public double Tic => _scanIndexStructInfo.TIC;

	/// <summary>
	/// Gets the trailer offset.
	/// </summary>
	public int TrailerOffset => _scanIndexStructInfo.TrailerOffset;

	/// <summary>
	/// Gets the raw (unprocessed) value of the scan type index.
	/// </summary>
	public int ScanTypeRawIndex => _scanIndexStructInfo.ScanTypeIndex;

	public int PacketCount => _scanIndexStructInfo.NumberPackets;

	int IMsScanIndexAccess.PacketType => (int)_scanIndexStructInfo.PacketType;

	public int ScanEventNumber => _scanIndexStructInfo.ScanTypeIndex;

	public int SegmentNumber => ScanSegment;

	public double TIC => _scanIndexStructInfo.TIC;

	public int CycleNumber => _scanIndexStructInfo.CycleNumber;

	public bool IsCentroidScan => PacketType.IsCentroidScan(HighPacketTypeWord);

	/// <summary>
	/// The retention time of a scan is the same as it's start time
	/// </summary>
	public double RetentionTime => _scanIndexStructInfo.StartTime;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanIndex" /> class.
	/// </summary>
	public ScanIndex()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanIndex" /> class.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="size">The size.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <param name="decoder">The decoder.</param>
	public ScanIndex(IMemoryReader viewer, int size, long dataOffset, Func<byte[], ScanIndexStruct> decoder)
	{
		Load(viewer, size, dataOffset, decoder);
	}

	/// <summary>
	/// Loads the specified viewer.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="size">The size.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <param name="decoder">The decoder.</param>
	private void Load(IMemoryReader viewer, int size, long dataOffset, Func<byte[], ScanIndexStruct> decoder)
	{
		_scanIndexStructInfo = decoder(viewer.ReadLargeData(dataOffset, size));
	}
}
