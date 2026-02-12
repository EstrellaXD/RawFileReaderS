using System;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

/// <summary>
///     The mass range structure.
/// </summary>
internal struct MassRangeStruct
{
	/// <summary>
	///     Gets or sets the low end of the mass range.
	/// </summary>
	public double LowMass;

	/// <summary>
	///     Gets or sets the high end of the mass range.
	/// </summary>
	public double HighMass;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.MassRangeStruct" /> struct - copy constructor.
	/// </summary>
	/// <param name="lowMass">
	/// The low end of the mass range.
	/// </param>
	/// <param name="highMass">
	/// The high end of the mass range.
	/// </param>
	public MassRangeStruct(double lowMass, double highMass)
	{
		LowMass = lowMass;
		HighMass = highMass;
	}

	/// <summary>
	/// Loads the array.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="startPos">The start position.</param>
	/// <returns>Array of mass range</returns>
	public static MassRangeStruct[] LoadArray(IMemoryReader viewer, ref long startPos)
	{
		int num = viewer.ReadIntExt(ref startPos);
		if (num == 0)
		{
			return Array.Empty<MassRangeStruct>();
		}
		MassRangeStruct[] array = new MassRangeStruct[num];
		for (int i = 0; i < num; i++)
		{
			double lowMass = viewer.ReadDoubleExt(ref startPos);
			double highMass = viewer.ReadDoubleExt(ref startPos);
			array[i] = new MassRangeStruct(lowMass, highMass);
		}
		return array;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.MassRangeStruct" /> struct.
	/// </summary>
	/// <param name="range">
	/// The range (to copy from).
	/// </param>
	public MassRangeStruct(IRangeAccess range)
	{
		LowMass = range.Low;
		HighMass = range.High;
	}
}
