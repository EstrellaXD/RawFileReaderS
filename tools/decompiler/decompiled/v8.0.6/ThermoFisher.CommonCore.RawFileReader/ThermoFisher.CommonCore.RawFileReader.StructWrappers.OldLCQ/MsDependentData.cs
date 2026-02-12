using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The MS dependent data, for legacy LCQ files
/// </summary>
internal sealed class MsDependentData : IRawObjectBase
{
	private MsDependentDataStruct _msDependentDataStructInfo;

	/// <summary>
	/// Gets or sets the mode or the largest.
	/// </summary>
	public OldLcqEnums.ModeLargest Mode
	{
		get
		{
			return _msDependentDataStructInfo.Mode;
		}
		set
		{
			_msDependentDataStructInfo.Mode = value;
		}
	}

	/// <summary>
	/// Gets or sets the index of the largest.
	/// </summary>
	public int Largest
	{
		get
		{
			return _msDependentDataStructInfo.Largest;
		}
		set
		{
			_msDependentDataStructInfo.Largest = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ.MsDependentData" /> class.
	/// </summary>
	public MsDependentData()
	{
		_msDependentDataStructInfo = new MsDependentDataStruct
		{
			Mode = OldLcqEnums.ModeLargest.NthLargestIntensity,
			Largest = 1,
			IsolationWidth = 1.0,
			UsePreviousList = false
		};
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
		bool flag = fileRevision < 13;
		long startPos = dataOffset;
		int count = (flag ? Marshal.SizeOf(typeof(MsDependentDataStruct2)) : Marshal.SizeOf(typeof(MsDependentDataStruct)));
		byte[] value = viewer.ReadBytesExt(ref startPos, count);
		_msDependentDataStructInfo = new MsDependentDataStruct
		{
			Mode = (OldLcqEnums.ModeLargest)BitConverter.ToInt32(value, 0),
			Largest = BitConverter.ToInt32(value, 4),
			IsolationWidth = BitConverter.ToDouble(value, 8),
			UsePreviousList = (!flag && BitConverter.ToInt32(value, 16) > 0)
		};
		return startPos - dataOffset;
	}
}
