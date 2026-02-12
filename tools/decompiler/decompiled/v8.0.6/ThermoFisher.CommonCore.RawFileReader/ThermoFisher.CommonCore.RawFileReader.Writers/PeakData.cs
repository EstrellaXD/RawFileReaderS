using System;
using System.IO;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.ExtraScanInfo;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The peak data.
/// </summary>
internal class PeakData
{
	private static readonly int DataSizeLowResSpType3 = 8;

	private static readonly int DataSizeLowResSpType4 = 9;

	private readonly long _asrDataPositionOffset;

	private readonly long _profileDataPacket64DataPositionOffset;

	private readonly int _asrProfileIndexStructSize;

	private readonly int _profileDataPacket64Size;

	private byte[] _indexBuffer;

	/// <summary>
	/// The _end of position.
	/// </summary>
	private long _endOfPosition;

	/// <summary>
	/// The _current start of data.
	/// </summary>
	private long _currentStartOfData;

	/// <summary>
	/// The _next start of data.
	/// </summary>
	private long _nextStartOfData;

	/// <summary>
	/// The _current index.
	/// </summary>
	private int _currentIndex;

	/// <summary>
	/// Gets or sets the packet type.
	/// </summary>
	public SpectrumPacketType PacketType { get; set; }

	/// <summary>
	/// Gets or sets the current start of data.
	/// </summary>
	public long CurrentStartOfData
	{
		get
		{
			return _currentStartOfData;
		}
		set
		{
			_currentStartOfData = value;
		}
	}

	/// <summary>
	/// Gets the next start of data.
	/// </summary>
	public long NextStartOfData => _nextStartOfData;

	/// <summary>
	/// Gets or sets the data initial offset.
	/// </summary>
	/// <value>
	/// The data initial offset.
	/// </value>
	public long DataInitialOffset { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.PeakData" /> class.
	/// </summary>
	public PeakData()
	{
		_currentStartOfData = 0L;
		_nextStartOfData = 0L;
		_currentIndex = 0;
		_endOfPosition = 0L;
		_asrDataPositionOffset = 72L;
		_profileDataPacket64DataPositionOffset = 20L;
		_asrProfileIndexStructSize = Utilities.StructSizeLookup.Value[8];
		_profileDataPacket64Size = Utilities.StructSizeLookup.Value[9];
	}

	/// <summary>
	/// Converts the raw scan into packets.
	/// </summary>
	/// <param name="instData">The mass spec data.</param>
	/// <param name="profileDataPacket">Mass spec profile data packets.</param>
	/// <returns>The converted packets in compressed form</returns>
	/// <exception cref="T:System.NotSupportedException">Not Supported Exception</exception>
	/// <exception cref="T:System.ArgumentException">The instrument scan data cannot be null.</exception>
	public static byte[] ConvertRawScanIntoPackets(IMsInstrumentData instData, out ProfileDataPacket64[] profileDataPacket)
	{
		ISegmentedScanAccess scanData = instData.ScanData;
		if (scanData == null)
		{
			throw new ArgumentException("Scan data cannot be null.");
		}
		SpectrumPacketType spectrumPacketType = (SpectrumPacketType)(instData.StatisticsData.PacketType & 0xFFFF);
		byte[] result = Array.Empty<byte>();
		profileDataPacket = null;
		switch (spectrumPacketType)
		{
		case SpectrumPacketType.LowResolutionSpectrum:
			result = LowResSpDataPkt.Compress(scanData);
			break;
		case SpectrumPacketType.LowResolutionSpectrumType2:
			result = LowResSpDataPkt2.Compress(scanData);
			break;
		case SpectrumPacketType.HighResolutionSpectrum:
			result = HrSpDataPkt.Compress(scanData);
			break;
		case SpectrumPacketType.LowResolutionSpectrumType3:
			profileDataPacket = GenerateProfileIndexDataPackets(instData, DataSizeLowResSpType3);
			result = LowResSpDataPkt3.Compress(scanData);
			break;
		case SpectrumPacketType.LowResolutionSpectrumType4:
			profileDataPacket = GenerateProfileIndexDataPackets(instData, DataSizeLowResSpType4);
			result = LowResSpDataPkt4.Compress(scanData);
			break;
		case SpectrumPacketType.LinearTrapCentroid:
			result = AdvancedPacketBase.CompressCentroids(instData);
			break;
		case SpectrumPacketType.LinearTrapProfile:
			result = LinearTrapProfilePacket.CompressProfiles(instData);
			break;
		case SpectrumPacketType.FtProfile:
			result = FtProfilePacket.CompressProfiles(instData);
			break;
		case SpectrumPacketType.FtCentroid:
			result = AdvancedPacketBase.CompressCentroids(instData);
			break;
		case SpectrumPacketType.ProfileSpectrum:
		case SpectrumPacketType.CompressedAccurateSpectrum:
		case SpectrumPacketType.StandardAccurateSpectrum:
		case SpectrumPacketType.StandardUncalibratedSpectrum:
		case SpectrumPacketType.AccurateMassProfileSpectrum:
		case SpectrumPacketType.ProfileSpectrumType2:
		case SpectrumPacketType.ProfileSpectrumType3:
		case SpectrumPacketType.HighResolutionCompressedProfile:
		case SpectrumPacketType.LowResolutionCompressedProfile:
			throw new NotSupportedException(spectrumPacketType.ToString() + " is not supported");
		}
		return result;
	}

	/// <summary>
	/// The below packet types are not supported for exporting.
	/// </summary>
	/// <param name="packetType">Type of the packet.</param>
	/// <returns>True if the packet type is supported, otherwise, throw not implemented exception.</returns>
	/// <exception cref="T:System.NotImplementedException">The packet type is supported.</exception>
	public static bool IsSupportedPacketTypes(SpectrumPacketType packetType)
	{
		switch (packetType)
		{
		case SpectrumPacketType.ProfileSpectrum:
		case SpectrumPacketType.CompressedAccurateSpectrum:
		case SpectrumPacketType.StandardAccurateSpectrum:
		case SpectrumPacketType.StandardUncalibratedSpectrum:
		case SpectrumPacketType.AccurateMassProfileSpectrum:
		case SpectrumPacketType.ProfileSpectrumType2:
		case SpectrumPacketType.ProfileSpectrumType3:
		case SpectrumPacketType.HighResolutionCompressedProfile:
		case SpectrumPacketType.LowResolutionCompressedProfile:
			throw new NotSupportedException(packetType.ToString() + " is not supported");
		default:
			return true;
		}
	}

	/// <summary>
	/// Writes the instrument data.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="data">The data.</param>
	/// <param name="errors">The errors.</param>
	/// <returns>True instrument data saved to the disk, false otherwise</returns>
	public bool WriteInstData(BinaryWriter writer, byte[] data, DeviceErrors errors)
	{
		try
		{
			writer.Write(data);
			writer.Flush();
			_nextStartOfData = (_endOfPosition = writer.BaseStream.Position);
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Writes the profile index information to disk
	/// </summary>
	/// <param name="data">The data.</param>
	/// <param name="dataSize">Size of the data.</param>
	/// <returns>True the indexes stored to disk, false otherwise.</returns>
	/// <exception cref="T:System.NotSupportedException">This format type does not support.</exception>
	public bool WriteInstIndex(byte[] data, int dataSize)
	{
		switch (PacketType)
		{
		case SpectrumPacketType.ProfileSpectrum:
		case SpectrumPacketType.ProfileIndex:
		case SpectrumPacketType.ProfileSpectrumType2:
		case SpectrumPacketType.ProfileSpectrumType3:
		case SpectrumPacketType.LowResolutionSpectrumType3:
		case SpectrumPacketType.LowResolutionSpectrumType4:
			Buffer.BlockCopy(BitConverter.GetBytes(_currentStartOfData - DataInitialOffset), 0, data, (int)_profileDataPacket64DataPositionOffset, 8);
			_indexBuffer = data;
			break;
		case SpectrumPacketType.PdaUvScannedSpectrum:
		case SpectrumPacketType.PdaUvScannedSpectrumIndex:
			Buffer.BlockCopy(BitConverter.GetBytes(_currentStartOfData), 0, data, (int)_asrDataPositionOffset, 8);
			_indexBuffer = data;
			break;
		default:
			throw new NotSupportedException(PacketType.ToString() + " is not supported");
		}
		_currentStartOfData = _nextStartOfData;
		_currentIndex += dataSize;
		return true;
	}

	/// <summary>
	/// Writes the indices.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <returns>The number of profile indexes </returns>
	/// <exception cref="T:System.NotSupportedException">This format type does not support.</exception>
	public int WriteIndices(BinaryWriter writer)
	{
		int num;
		switch (PacketType)
		{
		case SpectrumPacketType.ProfileSpectrum:
		case SpectrumPacketType.ProfileIndex:
		case SpectrumPacketType.ProfileSpectrumType2:
		case SpectrumPacketType.ProfileSpectrumType3:
		case SpectrumPacketType.LowResolutionSpectrumType3:
		case SpectrumPacketType.LowResolutionSpectrumType4:
			num = _profileDataPacket64Size;
			break;
		case SpectrumPacketType.PdaUvScannedSpectrum:
		case SpectrumPacketType.PdaUvScannedSpectrumIndex:
			num = _asrProfileIndexStructSize;
			break;
		default:
			throw new NotSupportedException(PacketType.ToString() + " is not supported");
		}
		writer.Write(_indexBuffer);
		writer.Flush();
		_endOfPosition = writer.BaseStream.Position;
		_nextStartOfData = _endOfPosition;
		int result = _currentIndex / num;
		_currentIndex = 0;
		return result;
	}

	/// <summary>
	/// Generates the profile index data packets.
	/// </summary>
	/// <param name="instData">The instrument data.</param>
	/// <param name="dataSize">Size of the data.</param>
	/// <returns>ProfileDataPacket64 array object.</returns>
	private static ProfileDataPacket64[] GenerateProfileIndexDataPackets(IMsInstrumentData instData, int dataSize)
	{
		ISegmentedScanAccess scanData = instData.ScanData;
		int segmentCount = scanData.SegmentCount;
		ProfileDataPacket64[] array = new ProfileDataPacket64[Math.Max(segmentCount, 0) + 1];
		int num = 0;
		for (int i = 0; i < segmentCount; i++)
		{
			int num2 = scanData.SegmentLengths[i];
			IRangeAccess rangeAccess = scanData.MassRanges[i];
			array[i] = new ProfileDataPacket64
			{
				DataPos = 0u,
				DataPosOffSet = num,
				LowMass = (float)rangeAccess.Low,
				HighMass = (float)rangeAccess.High,
				MassTick = 0.0
			};
			num += dataSize * num2;
		}
		array[segmentCount] = new ProfileDataPacket64
		{
			DataPos = (uint)num,
			DataPosOffSet = num,
			LowMass = -1f,
			HighMass = 0f,
			MassTick = 0.0
		};
		return array;
	}
}
