using System;
using System.IO;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.GenericItems;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides methods to write generic header item to disk file.
/// </summary>
internal static class GenericHeaderItemExtension
{
	/// <summary>
	/// Saves the generic header item.
	/// </summary>
	/// <param name="dataItem">The generic header item.</param>
	/// <param name="writer">The writer.</param>
	/// <param name="errors">store errors information.</param>
	/// <returns>True if the generic data item is written to disk, otherwise False</returns>
	public static bool SaveGenericHeaderItem(this DataDescriptor dataItem, BinaryWriter writer, DeviceErrors errors)
	{
		try
		{
			GenericDataItemStruct internalGenericDataItemStruct = dataItem.GetInternalGenericDataItemStruct();
			writer.Write((int)internalGenericDataItemStruct.DataType);
			writer.Write(internalGenericDataItemStruct.StringLengthOrPrecision);
			writer.StringWrite(dataItem.Label);
			writer.Flush();
			return true;
		}
		catch (Exception ex)
		{
			errors.UpdateError(ex);
		}
		return false;
	}

	/// <summary>
	/// Loads the generic header item.
	/// </summary>
	/// <param name="data">The generic header item as a byte array.</param>
	public static DataDescriptor LoadGenericHeaderItem(byte[] data)
	{
		GenericDataItemStruct genericDataItemStruct = new GenericDataItemStruct
		{
			DataType = (DataTypes)BitConverter.ToInt32(data, 0),
			StringLengthOrPrecision = BitConverter.ToUInt32(data, 4)
		};
		int num = BitConverter.ToInt32(data, 8);
		int num2 = 12;
		char[] array = new char[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = BitConverter.ToChar(data, num2);
			num2 += 2;
		}
		return new DataDescriptor(new string(array), genericDataItemStruct.DataType, genericDataItemStruct.StringLengthOrPrecision);
	}
}
