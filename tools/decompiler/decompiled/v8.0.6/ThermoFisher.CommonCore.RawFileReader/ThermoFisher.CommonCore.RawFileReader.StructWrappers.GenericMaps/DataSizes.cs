using System;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

/// <summary>
/// The singleton class to calculate data sizes.
/// </summary>
internal class DataSizes
{
	private static readonly Lazy<DataSizes> LazyInstance = new Lazy<DataSizes>(() => new DataSizes());

	private readonly uint[] _dataSizes;

	/// <summary>
	/// Gets the instance.
	/// </summary>
	public static DataSizes Instance => LazyInstance.Value;

	/// <summary>
	/// Prevents a default instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.DataSizes" /> class from being created.
	/// </summary>
	private DataSizes()
	{
		_dataSizes = new uint[14];
		_dataSizes[0] = 0u;
		_dataSizes[1] = 1u;
		_dataSizes[2] = 1u;
		_dataSizes[3] = 1u;
		_dataSizes[4] = 1u;
		_dataSizes[5] = 1u;
		_dataSizes[6] = 2u;
		_dataSizes[7] = 2u;
		_dataSizes[8] = 4u;
		_dataSizes[9] = 4u;
		_dataSizes[10] = 4u;
		_dataSizes[11] = 8u;
		_dataSizes[12] = 1u;
		_dataSizes[13] = 2u;
	}

	/// <summary>
	/// The method calculates the of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.Constants.DataTypes" /> type size in byes.
	/// </summary>
	/// <param name="type">
	/// The type.
	/// </param>
	/// <param name="count">
	/// The count - optional for string types.
	/// </param>
	/// <returns>
	/// The size in bytes.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// Thrown if there is an error calculating size (e.g. the type does not exist).
	/// </exception>
	public uint SizeInByes(DataTypes type, uint count = 1u)
	{
		try
		{
			return _dataSizes[(int)type] * count;
		}
		catch (Exception ex)
		{
			throw new Exception($"Error calculating size for {type}. Count = {count}.\n{ex.Message}");
		}
	}
}
