using System;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
/// The UV scan index.
/// </summary>
internal sealed class UvScanIndex : IScanIndex, ISimpleScanHeader
{
	private UvScanIndexStruct _uvScanIndexStructInfo;

	/// <summary>
	/// Gets or sets the UV scan index structure information.
	/// </summary>
	public UvScanIndexStruct UvScanIndexStructInfo
	{
		get
		{
			return _uvScanIndexStructInfo;
		}
		set
		{
			_uvScanIndexStructInfo = value;
		}
	}

	/// <summary>
	///     Gets the data offset.
	/// </summary>
	public long DataOffset => _uvScanIndexStructInfo.DataOffset;

	/// <summary>
	/// Gets the frequency.
	/// </summary>
	public double Frequency => _uvScanIndexStructInfo.Frequency;

	/// <summary>
	/// Gets a value indicating whether is uniform time.
	/// </summary>
	public bool IsUniformTime => _uvScanIndexStructInfo.UniformTime != 0;

	/// <summary>
	/// Gets the long wavelength.
	/// </summary>
	public double LongWavelength => _uvScanIndexStructInfo.LongWavelength;

	/// <summary>
	/// Gets the number of channels.
	/// </summary>
	public int NumberOfChannels => _uvScanIndexStructInfo.NumberOfChannels;

	/// <summary>
	///     Gets the number packets.
	/// </summary>
	public int NumberPackets => _uvScanIndexStructInfo.NumberPackets;

	/// <summary>
	///     Gets or sets the packet type.
	/// </summary>
	public SpectrumPacketType PacketType
	{
		get
		{
			return (SpectrumPacketType)(_uvScanIndexStructInfo.PacketType & 0xFFFF);
		}
		set
		{
			_uvScanIndexStructInfo.PacketType = (int)value;
		}
	}

	public double RetentionTime => _uvScanIndexStructInfo.StartTime;

	/// <summary>
	///     Gets the scan number.
	/// </summary>
	public int ScanNumber => _uvScanIndexStructInfo.ScanNumber;

	/// <summary>
	/// Gets the short wavelength.
	/// </summary>
	public double ShortWavelength => _uvScanIndexStructInfo.ShortWavelength;

	/// <summary>
	///     Gets the start time.
	/// </summary>
	public double StartTime => _uvScanIndexStructInfo.StartTime;

	/// <summary>
	///     Gets or sets the TIC.
	/// </summary>
	public double Tic
	{
		get
		{
			return _uvScanIndexStructInfo.TIC;
		}
		set
		{
			_uvScanIndexStructInfo.TIC = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex" /> class.
	/// </summary>
	public UvScanIndex()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex" /> class.
	/// </summary>
	/// <param name="viewer">
	/// The device' memory mapped stream viewer.
	/// </param>
	/// <param name="size">Size of the UV scan index struct</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <param name="decoder">A method that will reads the UV scan index struct from a byte array and convert it back to struct.</param>
	public UvScanIndex(IMemoryReader viewer, int size, long dataOffset, Func<byte[], UvScanIndexStruct> decoder)
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
	private void Load(IMemoryReader viewer, int size, long dataOffset, Func<byte[], UvScanIndexStruct> decoder)
	{
		_uvScanIndexStructInfo = decoder(viewer.ReadLargeData(dataOffset, size));
	}
}
