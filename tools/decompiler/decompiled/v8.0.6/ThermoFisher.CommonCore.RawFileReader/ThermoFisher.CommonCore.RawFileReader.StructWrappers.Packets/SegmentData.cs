using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

/// <summary>
/// The segment data.
/// </summary>
internal sealed class SegmentData
{
	/// <summary>
	/// Gets or sets the data peaks.
	/// </summary>
	public List<DataPeak> DataPeaks { get; set; }

	/// <summary>
	/// Gets or sets the mass range.
	/// </summary>
	public MassRangeStruct MassRange { get; set; }

	/// <summary>
	/// find peak position.
	/// </summary>
	/// <param name="mass">
	/// The mass.
	/// </param>
	/// <returns>
	/// The index of the mass
	/// </returns>
	public int FindPeakPos(double mass)
	{
		int num = 0;
		int num2 = DataPeaks.Count - 1;
		if (DataPeaks[num].Position >= mass)
		{
			return num;
		}
		if (DataPeaks[num2].Position < mass)
		{
			return -1;
		}
		while (num2 > num + 1)
		{
			int num3 = (num2 + num) / 2;
			if (DataPeaks[num3].Position < mass)
			{
				num = num3;
			}
			else
			{
				num2 = num3;
			}
		}
		return num2;
	}

	/// <summary>
	/// find peak position.
	/// </summary>
	/// <param name="mass">
	/// The mass.
	/// </param>
	/// <returns>
	/// The index of the mass
	/// </returns>
	private int FindPeakPosSimple(double mass)
	{
		int num = 0;
		int num2 = DataPeaks.Count - 1;
		if (DataPeaks[num].Position >= mass)
		{
			return num;
		}
		if (DataPeaks[num2].Position < mass)
		{
			return -1;
		}
		while (num2 > num + 1)
		{
			int num3 = (num2 + num) / 2;
			if (DataPeaks[num3].Position < mass)
			{
				num = num3;
			}
			else
			{
				num2 = num3;
			}
		}
		return num2;
	}
}
