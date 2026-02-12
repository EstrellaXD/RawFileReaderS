using System;
using System.Linq;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The non ITCL data from legacy LCQ files.
/// </summary>
internal sealed class NonItcl : IRawObjectBase
{
	private readonly NonItclStruct _defaultNonItclStruct;

	private NonItclStruct _nonItclStructInfo;

	/// <summary>
	/// Gets or sets the minimum MS run time.
	/// Minimum MS run time (to correct old methods)
	/// </summary>
	/// <value>
	/// The minimum MS run time.
	/// </value>
	private double MinMsRunTime { get; set; }

	/// <summary>
	/// Gets the number segments.
	/// </summary>
	/// <value>
	/// The number segments.
	/// </value>
	public int NumSegments { get; private set; }

	/// <summary>
	/// Gets the mass rejects.
	/// </summary>
	public double[] MassRejects { get; private set; }

	/// <summary>
	/// Gets the mass precursors.
	/// </summary>
	public double[] MassPrecursors { get; private set; }

	/// <summary>
	/// Gets the MS segments.
	/// </summary>
	public MsSegment[] MsSegments { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ.NonItcl" /> class.
	/// </summary>
	public NonItcl()
	{
		_defaultNonItclStruct.TotalAcqTime = 10.0;
		_defaultNonItclStruct.TotalScanTime = 20.0;
		_defaultNonItclStruct.PreSignalThreshold = 100000.0;
		_defaultNonItclStruct.Mode = OldLcqEnums.DataMode.Centroid;
		_defaultNonItclStruct.SubtractedMass = 0.0;
		_defaultNonItclStruct.DefaultChargeState = 2;
		_defaultNonItclStruct.DepCollisionEnergy = 35.0;
		_defaultNonItclStruct.ParentSignalThreshold = 100000.0;
		_defaultNonItclStruct.DepIsolationWidth = 2.0;
		MinMsRunTime = 0.1;
		NumSegments = 1;
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
		bool flag = fileRevision < 9;
		long startPos = dataOffset;
		int count = (flag ? Marshal.SizeOf(typeof(NonItclStruct1)) : Marshal.SizeOf(typeof(NonItclStruct)));
		byte[] value = viewer.ReadBytesExt(ref startPos, count);
		_nonItclStructInfo = new NonItclStruct
		{
			TotalAcqTime = BitConverter.ToDouble(value, 0),
			TotalScanTime = BitConverter.ToDouble(value, 8),
			PreSignalThreshold = BitConverter.ToDouble(value, 16),
			Mode = (OldLcqEnums.DataMode)BitConverter.ToInt32(value, 24),
			SubtractedMass = BitConverter.ToDouble(value, 32),
			DefaultChargeState = BitConverter.ToInt32(value, 40),
			DepCollisionEnergy = BitConverter.ToDouble(value, 48),
			ParentSignalThreshold = BitConverter.ToDouble(value, 56),
			DepIsolationWidth = (flag ? _defaultNonItclStruct.DepIsolationWidth : BitConverter.ToDouble(value, 64))
		};
		MassRejects = viewer.ReadDoublesExt(ref startPos);
		MassPrecursors = viewer.ReadDoublesExt(ref startPos);
		MsSegments = viewer.LoadRawFileObjectArray<MsSegment>(fileRevision, ref startPos);
		NumSegments = MsSegments.Length;
		if (_nonItclStructInfo.TotalAcqTime < MinMsRunTime)
		{
			_nonItclStructInfo.TotalAcqTime = MinMsRunTime;
		}
		if (fileRevision < 7)
		{
			double num = 0.0;
			int num2 = MsSegments.Length;
			for (int i = 0; i < num2; i++)
			{
				num += MsSegments[i].AcquisitionTime;
			}
			_nonItclStructInfo.TotalAcqTime = num;
		}
		ForceMassesInRange();
		return startPos - dataOffset;
	}

	/// <summary>
	/// Force all specified masses to be within the mass range of the instrument
	/// </summary>
	private void ForceMassesInRange()
	{
		MassPrecursors = Array.FindAll(MassPrecursors, (double val) => val >= 50.0 && val <= 2000.0).ToArray();
		MassRejects = Array.FindAll(MassRejects, (double val) => val >= 50.0 && val <= 2000.0).ToArray();
		MsScanEvent msScanEvent = new MsScanEvent();
		int num = MsSegments.Length;
		for (int num2 = 0; num2 < num; num2++)
		{
			MsSegment obj = MsSegments[num2];
			int num3 = obj.MsScanEvents.Length;
			MsScanEvent[] msScanEvents = obj.MsScanEvents;
			for (int num4 = 0; num4 < num3; num4++)
			{
				bool flag = false;
				MsScanEvent msScanEvent2 = msScanEvents[num4];
				int num5 = msScanEvent2.MassRangesInfo.Length;
				for (int num6 = 0; num6 < num5; num6++)
				{
					MassRangeStruct massRangeStruct = msScanEvent2.MassRangesInfo[num6];
					if (massRangeStruct.LowMass < 50.0 || massRangeStruct.HighMass > 2000.0)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					int num7 = msScanEvent2.ReactionsInfo.Length;
					Reaction[] reactionsInfo = msScanEvent2.ReactionsInfo;
					for (int num8 = 0; num8 < num7; num8++)
					{
						double precursorMass = reactionsInfo[num8].PrecursorMass;
						if (precursorMass < 50.0 || precursorMass > 2000.0)
						{
							flag = true;
						}
					}
				}
				if (flag)
				{
					msScanEvent2 = msScanEvent;
				}
				if (msScanEvent2.MsScanEventStructInfo.DepDataFlag > 0 && msScanEvent2.MsDependentDataInfo.Mode == OldLcqEnums.ModeLargest.NthLargestIntensityPrecursor && msScanEvent2.MsDependentDataInfo.Largest > MassPrecursors.Length)
				{
					msScanEvent2.MsDependentDataInfo.Largest = 1;
					msScanEvent2.MsDependentDataInfo.Mode = OldLcqEnums.ModeLargest.NthLargestIntensity;
				}
			}
		}
	}
}
