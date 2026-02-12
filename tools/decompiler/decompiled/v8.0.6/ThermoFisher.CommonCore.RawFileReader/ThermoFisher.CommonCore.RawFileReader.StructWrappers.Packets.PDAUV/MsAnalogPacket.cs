using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.PDAUV;

/// <summary>
/// The MS analog packet.
/// </summary>
internal sealed class MsAnalogPacket : IPacket, IRawObjectBase
{
	private readonly Lazy<List<SegmentData>> _lazySegmentPeaks;

	private readonly int _channelNum;

	private readonly int _numChannels;

	private byte[] _profileBlob;

	/// <summary>
	/// Gets the segmented peaks.
	/// </summary>
	public List<SegmentData> SegmentPeaks
	{
		get
		{
			if (_lazySegmentPeaks == null)
			{
				return new List<SegmentData>();
			}
			return _lazySegmentPeaks.Value;
		}
	}

	/// <summary>
	/// Gets the scan index.
	/// </summary>
	public IScanIndex Index { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.PDAUV.MsAnalogPacket" /> class.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="scanIndex">Index of the scan.</param>
	/// <param name="channelNum">The channel number.</param>
	public MsAnalogPacket(IDisposableReader viewer, int fileRevision, UvScanIndex scanIndex, int channelNum)
	{
		MsAnalogPacket msAnalogPacket = this;
		long dataOffset = scanIndex.DataOffset;
		_numChannels = scanIndex.NumberOfChannels;
		_channelNum = channelNum;
		_lazySegmentPeaks = new Lazy<List<SegmentData>>(() => msAnalogPacket.ExpandProfileBlob(scanIndex.ShortWavelength, scanIndex.LongWavelength));
		Index = scanIndex;
		Load(viewer, dataOffset, fileRevision);
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
		int num = _numChannels * 8;
		_profileBlob = viewer.ReadBytes(dataOffset, num);
		return num;
	}

	/// <summary>
	/// Expands the profile BLOB.
	/// </summary>
	/// <param name="shortWave">The short wave.</param>
	/// <param name="longWave">The long wave.</param>
	/// <returns>Data for the scan</returns>
	private List<SegmentData> ExpandProfileBlob(double shortWave, double longWave)
	{
		if (_profileBlob == null || _profileBlob.Length == 0)
		{
			return new List<SegmentData>(1);
		}
		int num = 0;
		int num2 = _numChannels;
		if (_channelNum > -1)
		{
			num = _channelNum * 8;
			num2 = 1;
		}
		List<SegmentData> list = new List<SegmentData>(num2);
		for (int i = 0; i < num2; i++)
		{
			DataPeak item = new DataPeak(Index.StartTime, BitConverter.ToDouble(_profileBlob, num));
			list.Add(new SegmentData
			{
				DataPeaks = new List<DataPeak> { item },
				MassRange = new MassRangeStruct(shortWave, longWave)
			});
			num += 8;
		}
		_profileBlob = null;
		return list;
	}
}
