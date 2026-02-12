using System;
using System.IO;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.GenericItems;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides save extension methods for generic header
/// </summary>
internal static class GenericHeaderExtension
{
	/// <summary>
	/// Write generic header item to the file.
	/// </summary>
	/// <param name="headerItems">Generic header definitions</param>
	/// <param name="writer">The binary writer.</param>
	/// <param name="numHeaderItems">Number of the header items.</param>
	/// <param name="errors">The errors object for storing error information.</param>
	/// <returns>True generic header saved to the disk, false otherwise.</returns>
	public static bool SaveGenericHeader(this DataDescriptors headerItems, BinaryWriter writer, int numHeaderItems, DeviceErrors errors)
	{
		bool result = false;
		try
		{
			writer.Write(numHeaderItems);
			for (int i = 0; i < numHeaderItems && headerItems[i].SaveGenericHeaderItem(writer, errors); i++)
			{
			}
			writer.Flush();
			result = !errors.HasError;
		}
		catch (Exception ex)
		{
			errors.UpdateError(ex);
		}
		return result;
	}

	/// <summary>
	/// Write generic header item to the memory stream for data packer.
	/// The implementation of the header packing is based on the header unpacking item code (UnpackHeaderItem)
	/// </summary>
	/// <param name="headerItems">Generic header definitions</param>
	/// <param name="writer">The memory stream writer.</param>
	/// <param name="numHeaderItems">Number of the header items.</param>
	public static void SaveGenericHeaderToMemoryStream(this DataDescriptors headerItems, MemoryStream writer, int numHeaderItems)
	{
		writer.Write(BitConverter.GetBytes(numHeaderItems));
		for (int i = 0; i < numHeaderItems; i++)
		{
			DataDescriptor dataDescriptor = headerItems[i];
			GenericDataItemStruct internalGenericDataItemStruct = dataDescriptor.GetInternalGenericDataItemStruct();
			writer.Write(BitConverter.GetBytes((int)internalGenericDataItemStruct.DataType));
			writer.Write(BitConverter.GetBytes(internalGenericDataItemStruct.StringLengthOrPrecision));
			string label = dataDescriptor.Label;
			writer.Write((!string.IsNullOrWhiteSpace(label)) ? MsDataPacker.GetBytesWithLength(label) : BitConverter.GetBytes(0));
		}
		writer.Flush();
	}

	/// <summary>
	/// Converts the header items to data descriptors.
	/// </summary>
	/// <param name="headerItems">The header items.</param>
	/// <param name="dataDescriptors">The data descriptors.</param>
	public static void ConvertHeaderItemsToGenericHeader(this IHeaderItem[] headerItems, out DataDescriptors dataDescriptors)
	{
		if (!headerItems.IsAny())
		{
			dataDescriptors = new DataDescriptors(0);
			return;
		}
		int num = headerItems.Length;
		dataDescriptors = new DataDescriptors(num);
		for (int i = 0; i < num; i++)
		{
			IHeaderItem headerItem = headerItems[i];
			uint stringLengthOrPrecision = (uint)(headerItem.IsScientificNotation ? (headerItem.StringLengthOrPrecision | 0x10000) : (headerItem.StringLengthOrPrecision & 0xFFFF));
			dataDescriptors.AddItem(new DataDescriptor(headerItem.Label, (DataTypes)headerItem.DataType, stringLengthOrPrecision));
		}
	}
}
