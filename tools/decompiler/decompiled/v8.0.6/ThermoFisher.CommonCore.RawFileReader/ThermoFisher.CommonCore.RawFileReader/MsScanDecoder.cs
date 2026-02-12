using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.DataModel;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.PROFSP;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Decode scan data, from an array of bytes
/// </summary>
internal class MsScanDecoder : IMsScanDecoder
{
	private readonly IMsScanIndexAccess _index;

	private readonly MemoryArrayReader _reader;

	private readonly int _version;

	private readonly long _startAddress;

	/// <summary>
	/// Create an MS scan decoder
	/// </summary>
	/// <param name="data">data array to decode</param>
	/// <param name="startAddress">start of scan data (the byte array may have other data before the scan)
	///             </param>
	/// <param name="index"></param>
	/// <param name="version">raw data version</param>
	public MsScanDecoder(byte[] data, long startAddress, IMsScanIndexAccess index, int version)
	{
		_index = index;
		_reader = new MemoryArrayReader(data, 0L);
		_version = version;
		_startAddress = startAddress;
	}

	public ISimpleScanAccess DecodeSimplifiedCentroids(bool includeReferenceAndExceptionData)
	{
		return MassSpecDevice.LabelPeaksToSimpleScan(ReadMsPacket(includeReferenceAndExceptionData).LabelPeaks);
	}

	public ISimpleScanAccess DecodeSimplifiedScan(bool includeReferenceAndExceptionData, double[] calibrators)
	{
		IPacket packet = ReadMsPacket(includeReferenceAndExceptionData, PacketFeatures.Profile, calibrators);
		int num = 0;
		List<SegmentData> dataPeaks;
		if (packet == null)
		{
			dataPeaks = new List<SegmentData>();
		}
		else
		{
			List<SegmentData> segmentPeaks = packet.SegmentPeaks;
			int count = segmentPeaks.Count;
			for (int i = 0; i < count; i++)
			{
				num += segmentPeaks[i].DataPeaks.Count;
			}
			dataPeaks = segmentPeaks;
		}
		return RawFileAccessBase.SegmentedDataToSimpleScan(num, dataPeaks);
	}

	/// <summary>
	/// This method get the noise, baselines and frequencies data.
	/// This will typically used by the application when exporting mass spec data to a raw file.<para />
	/// The advanced packet data is for LT/FT formats only.<para />
	/// </summary>
	/// <param name="includeReferenceAndExceptionData">Set if centroid data should include the reference and exception peaks</param>
	/// <param name="calibrators">Mass calibration tables needed to decode profiles</param>
	/// <returns>Returns the IAdvancedPacketData object.</returns>
	/// <exception cref="T:System.Exception">Thrown if encountered an error while retrieving LT/FT's data, i.e. noise data and frequencies.</exception>
	public IAdvancedPacketData DecodeAdvancedPacketData(bool includeReferenceAndExceptionData, double[] calibrators)
	{
		try
		{
			IMsPacket packet = ReadMsPacket(includeReferenceAndExceptionData, PacketFeatures.All, calibrators);
			if (packet == null)
			{
				return new MassSpecDevice.AdvancedPacketData(new Lazy<CentroidStream>(new CentroidStream()), new Lazy<double[]>(Array.Empty<double>()));
			}
			Lazy<double[]> frequencies = new Lazy<double[]>(() => MassSpecDevice.GetFrequencies(packet));
			Lazy<CentroidStream> centroidReader = new Lazy<CentroidStream>(() => CentroidStreamFactory.CreateCentroidStream(packet.LabelPeaks));
			SpectrumPacketType packetType = (SpectrumPacketType)(_index.PacketType & 0xFFFF);
			return new MassSpecDevice.AdvancedPacketData(centroidReader, frequencies)
			{
				NoiseData = (packetType.HasLabelPeaks() ? packet.NoiseAndBaselines : Array.Empty<NoiseAndBaseline>())
			};
		}
		catch (Exception ex)
		{
			throw new Exception($"Error while retrieving noise and baseline data. {ex.Message}", ex);
		}
	}

	/// <summary>
	/// Get the centroids saved with a profile scan.
	/// This is only valid for data types which support
	/// multiple sets of data per scan (such as <c>Orbitrap</c> data).
	/// This method does not "Centroid profile data".
	/// This method does not return a "Coefficients" table.
	/// </summary>
	/// <param name="includeReferenceAndExceptionPeaks">
	/// determines if peaks flagged as ref should be returned
	/// </param>
	/// <returns>
	/// centroid stream from the data to be decoded"/&gt;.
	/// </returns>
	public ICentroidStreamAccess DecodeCentroidStream(bool includeReferenceAndExceptionPeaks)
	{
		PacketFeatures packetFeatures = PacketFeatures.All;
		packetFeatures -= 8;
		return CentroidStreamFactory.CreateCentroidStream(ReadMsPacket(includeReferenceAndExceptionPeaks, packetFeatures).LabelPeaks);
	}

	private IMsPacket ReadMsPacket(bool includeReferenceAndExceptionData, PacketFeatures packetScanDataFeatures = PacketFeatures.None, double[] calibrators = null)
	{
		MemoryArrayReader reader = _reader;
		SpectrumPacketType spectrumPacketType = (SpectrumPacketType)(_index.PacketType & 0xFFFF);
		switch (spectrumPacketType)
		{
		case SpectrumPacketType.FtCentroid:
			return new FtCentroidPacket(reader, _startAddress, _version, includeReferenceAndExceptionData, packetScanDataFeatures);
		case SpectrumPacketType.FtProfile:
			return new FtProfilePacket(reader, _startAddress, Calibrators, _version, includeReferenceAndExceptionData, packetScanDataFeatures);
		case SpectrumPacketType.LinearTrapProfile:
			return new LinearTrapProfilePacket(reader, 0L, _version, includeReferenceAndExceptionData, packetScanDataFeatures | PacketFeatures.Profile);
		case SpectrumPacketType.LinearTrapCentroid:
			return new LinearTrapCentroidPacket(reader, _startAddress, _version, includeReferenceAndExceptionData, PacketFeatures.None);
		case SpectrumPacketType.LowResolutionSpectrum:
			return new LowResSpDataPkt(reader, _startAddress, _index, _version);
		case SpectrumPacketType.LowResolutionSpectrumType2:
			return new LowResSpDataPkt2(reader, _startAddress, _index, _version);
		case SpectrumPacketType.LowResolutionSpectrumType3:
		{
			int offset5 = LowResSpDataPkt3.GetOffset(reader.Length, _index.PacketCount, _version);
			return new LowResSpDataPkt3(reader, offset5, _index, _version, isSingleScan: true);
		}
		case SpectrumPacketType.LowResolutionSpectrumType4:
		{
			int offset4 = LowResSpDataPkt4.GetOffset(reader.Length, _index.PacketCount, _version);
			return new LowResSpDataPkt4(reader, offset4, _index, _version, isSingleScan: true);
		}
		case SpectrumPacketType.HighResolutionSpectrum:
			return new HrSpDataPkt(reader, _startAddress, _index, _version, includeReferenceAndExceptionData);
		case SpectrumPacketType.StandardAccurateSpectrum:
			return new StandardAccuracyPacket(reader, _startAddress, _index, _version, Calibrators());
		case SpectrumPacketType.CompressedAccurateSpectrum:
		case SpectrumPacketType.StandardUncalibratedSpectrum:
		case SpectrumPacketType.AccurateMassProfileSpectrum:
		case SpectrumPacketType.HighResolutionCompressedProfile:
		case SpectrumPacketType.LowResolutionCompressedProfile:
			throw new NotImplementedException(spectrumPacketType.ToString() + " is not yet implemented");
		case SpectrumPacketType.ProfileSpectrumType2:
		{
			int offset3 = ProfSpPkt2.GetOffset(reader.Length, _index.PacketCount, _version);
			return new ProfSpPkt2(reader, offset3, _index, _version, isSingleScan: true);
		}
		case SpectrumPacketType.ProfileSpectrumType3:
		{
			int offset2 = ProfSpPkt3.GetOffset(reader.Length, _index.PacketCount, _version);
			return new ProfSpPkt3(reader, offset2, _index, _version, isSingleScan: true);
		}
		case SpectrumPacketType.ProfileSpectrum:
		{
			int offset = ProfSpPkt.GetOffset(reader.Length, _index.PacketCount, _version);
			return new ProfSpPkt(reader, offset, _index, _version, isSingleScan: true);
		}
		default:
			throw new NotImplementedException(spectrumPacketType.ToString() + " not supported");
		}
		double[] Calibrators()
		{
			return calibrators ?? Array.Empty<double>();
		}
	}

	/// <summary>
	/// Get a segmented scan. This is the primary scan from the raw file.
	/// FT instrument files (such as Calcium) will have a second format of the scan (a centroid stream)
	/// </summary>
	/// <returns>The segmented scan.</returns>
	public ISegmentedScanAccess DecodeSegmentedScan(bool includeReferenceAndExceptionData, double[] coefficients)
	{
		try
		{
			int numSegments;
			int numAllPeaks;
			IReadOnlyList<SegmentData> segmentPeaks = GetSegmentPeaks(coefficients, out numSegments, out numAllPeaks, includeReferenceAndExceptionData);
			return RawFileAccessBase.FormatSegmentedScan(_index.ScanNumber, numSegments, numAllPeaks, segmentPeaks);
		}
		catch (Exception)
		{
		}
		return new SegmentedScan();
	}

	/// <summary>
	/// Gets the segment peaks.
	/// </summary>
	/// <param name="calibrators">mass calibration coefficients</param>
	/// <param name="numSegments">
	/// The number segments.
	/// </param>
	/// <param name="numAllPeaks">
	/// The number all peaks.
	/// </param>
	/// <param name="includeReferenceAndExceptionData">
	/// Flag to indicate the returning peak data should include reference and exception data or not.
	/// </param>
	/// <returns>
	/// Segment data
	/// </returns>
	private IReadOnlyList<SegmentData> GetSegmentPeaks(double[] calibrators, out int numSegments, out int numAllPeaks, bool includeReferenceAndExceptionData)
	{
		numAllPeaks = 0;
		numSegments = 0;
		IMsPacket msPacket = ReadMsPacket(includeReferenceAndExceptionData, PacketFeatures.All, calibrators);
		if (msPacket == null)
		{
			return new List<SegmentData>();
		}
		List<SegmentData> segmentPeaks = msPacket.SegmentPeaks;
		numSegments = segmentPeaks.Count;
		for (int i = 0; i < numSegments; i++)
		{
			numAllPeaks += segmentPeaks[i].DataPeaks.Count;
		}
		return segmentPeaks;
	}

	/// <summary>
	/// Gets the extended scan data for a scan
	/// </summary>
	/// <returns>The extended data</returns>
	public IExtendedScanData DecodeExtendedScanData()
	{
		try
		{
			if (((SpectrumPacketType)(_index.PacketType & 0xFFFF)).HasLabelPeaks())
			{
				IMsPacket msPacket = ReadMsPacket(includeReferenceAndExceptionData: false);
				if (msPacket != null)
				{
					return msPacket.ExtendedData;
				}
			}
			return new AdvancedPacketBase.EmptyExtendedScanData();
		}
		catch (Exception ex)
		{
			throw new Exception($"Error while decoding extended data: {ex.Message}", ex);
		}
	}
}
