using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.ASR;

/// <summary>
/// The AdjustableScanRate profile packet.
/// This defines a simple scan type, where the scan data is just an "array of <c>int</c>".
/// This is used for PDA detector data, where the "intensity" data is absorbance values
/// The index indicates the start wavelength (wavelength of the first intensity)
/// and the wavelength step, between readings.
/// </summary>
internal sealed class AdjustableScanRateProfilePacket : IPacket
{
	/// <summary>
	/// Gets the scan index.
	/// </summary>
	public IScanIndex Index { get; }

	/// <summary>
	/// Gets the segment peaks.
	/// </summary>
	public List<SegmentData> SegmentPeaks { get; }

	/// <summary>
	/// Gets the indices.
	/// </summary>
	public List<AdjustableScanRateProfileIndex> Indices { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.ASR.AdjustableScanRateProfilePacket" /> class.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="dataOffset">offset from start of memory map</param>
	/// <param name="fileRevision">
	/// The file format version.
	/// </param>
	/// <param name="scanIndex">
	/// The scan index.
	/// </param>
	internal AdjustableScanRateProfilePacket(IDisposableReader viewer, long dataOffset, int fileRevision, UvScanIndex scanIndex)
	{
		Index = scanIndex;
		SegmentPeaks = new List<SegmentData>();
		Indices = new List<AdjustableScanRateProfileIndex>();
		for (int i = 0; i < scanIndex.NumberPackets; i++)
		{
			SegmentPeaks.Add(CreatePacket(viewer, dataOffset, fileRevision, scanIndex.ScanNumber));
		}
	}

	/// <summary>
	/// create packet (scan data).
	/// </summary>
	/// <param name="viewer">
	///     The viewer.
	/// </param>
	/// <param name="dataOffset">Offset into memory map</param>
	/// <param name="fileRevision">
	///     The file format version.
	/// </param>
	/// <param name="scanIndexScanNumber">The scan number</param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.SegmentData" />.
	/// </returns>
	private SegmentData CreatePacket(IDisposableReader viewer, long dataOffset, int fileRevision, int scanIndexScanNumber)
	{
		long startPos = dataOffset;
		AdjustableScanRateProfileIndex adjustableScanRateProfileIndex = viewer.LoadRawFileObjectExt(() => new AdjustableScanRateProfileIndex(), fileRevision, ref startPos);
		startPos = adjustableScanRateProfileIndex.DataPosition;
		if (startPos == 0L && scanIndexScanNumber != 1)
		{
			startPos = dataOffset - adjustableScanRateProfileIndex.NumberOfPackets * 4;
		}
		SegmentData segmentData = new SegmentData
		{
			MassRange = new MassRangeStruct(adjustableScanRateProfileIndex.WavelengthStart, adjustableScanRateProfileIndex.WavelengthEnd)
		};
		uint numberOfPackets = adjustableScanRateProfileIndex.NumberOfPackets;
		if (numberOfPackets != 0)
		{
			int[] array = viewer.ReadInts(startPos, (int)numberOfPackets);
			int num = array.Length;
			List<DataPeak> list = new List<DataPeak>(num);
			double num2 = adjustableScanRateProfileIndex.TimeWavelengthStart;
			for (int num3 = 0; num3 < num; num3++)
			{
				DataPeak item = new DataPeak(num2, array[num3]);
				num2 += adjustableScanRateProfileIndex.TimeWavelengthStep;
				list.Add(item);
			}
			segmentData.DataPeaks = list;
		}
		else
		{
			segmentData.DataPeaks = new List<DataPeak>();
		}
		Indices.Add(adjustableScanRateProfileIndex);
		return segmentData;
	}
}
