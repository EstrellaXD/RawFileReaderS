using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.PDAUV;

/// <summary>
/// The channel UV packet. (one or more channels of UV data)
/// </summary>
internal sealed class ChannelUvPacket : IPacket, IRawObjectBase
{
	private readonly int _channelNum;

	private readonly int _numChannels;

	private readonly int _numDataPktsPerChannel;

	private readonly bool _uniformTime;

	private double[] _profileBlob;

	/// <summary>
	/// Gets the index.
	/// </summary>
	/// <value>
	/// The index.
	/// </value>
	public IScanIndex Index { get; }

	/// <summary>
	/// Gets the segmented peaks.
	/// </summary>
	public List<SegmentData> SegmentPeaks { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.PDAUV.ChannelUvPacket" /> class.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="scanIndex">Index of the scan.</param>
	/// <param name="channelNum">The channel number.</param>
	public ChannelUvPacket(IDisposableReader viewer, int fileRevision, UvScanIndex scanIndex, int channelNum)
	{
		long dataOffset = scanIndex.DataOffset;
		_uniformTime = scanIndex.IsUniformTime;
		_numChannels = scanIndex.NumberOfChannels;
		_channelNum = channelNum;
		_numDataPktsPerChannel = (_uniformTime ? 1 : 2);
		Index = scanIndex;
		Load(viewer, dataOffset, fileRevision);
		SegmentPeaks = ExpandChannelBlob(scanIndex);
	}

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		int num = _numDataPktsPerChannel * _numChannels;
		_profileBlob = viewer.ReadDoubles(dataOffset, num);
		return num * 8;
	}

	/// <summary>
	/// Expands the profile BLOB.
	/// </summary>
	/// <param name="scanIndex">The index.</param>
	/// <returns>The data for the scan</returns>
	private List<SegmentData> ExpandChannelBlob(UvScanIndex scanIndex)
	{
		double shortWavelength = scanIndex.ShortWavelength;
		double longWavelength = scanIndex.LongWavelength;
		int scanNumber = scanIndex.ScanNumber;
		double frequency = scanIndex.Frequency;
		if (_profileBlob == null || _profileBlob.Length == 0)
		{
			return new List<SegmentData>(1);
		}
		int num = 0;
		int num2 = _numChannels;
		if (_channelNum > -1)
		{
			num = _numDataPktsPerChannel * _channelNum;
			num2 = 1;
		}
		List<SegmentData> list = new List<SegmentData>(num2);
		for (int i = 0; i < num2; i++)
		{
			DataPeak item = new DataPeak((!_uniformTime) ? _profileBlob[num + 1] : ((double)scanNumber / frequency / 60.0), _profileBlob[num]);
			list.Add(new SegmentData
			{
				DataPeaks = new List<DataPeak> { item },
				MassRange = new MassRangeStruct(shortWavelength, longWavelength)
			});
			num += _numDataPktsPerChannel;
		}
		_profileBlob = null;
		return list;
	}
}
