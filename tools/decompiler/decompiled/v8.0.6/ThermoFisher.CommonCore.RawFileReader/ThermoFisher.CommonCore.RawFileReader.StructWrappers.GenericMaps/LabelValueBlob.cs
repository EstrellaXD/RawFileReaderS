using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

/// <summary>
/// The class encapsulates a blob that contains a list of label value pairs.
/// This holds information about where to find the binary record.
/// the records can be "decoded once" similar to a Lazy pattern, and saved in a cache.
/// </summary>
internal class LabelValueBlob
{
	private readonly long _viewOffset;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.LabelValueBlob" /> class.
	/// </summary>
	/// <param name="offset">The offset.</param>
	public LabelValueBlob(long offset)
	{
		_viewOffset = offset;
	}

	/// <summary>
	/// Returns all the values, no formatting.
	/// </summary>
	/// <param name="decoder">
	/// The log reader.
	/// </param>
	/// <returns>The values, as the nearest .net type</returns>
	public object[] GetAllValues(ILogDecoder decoder)
	{
		DataDescriptors dataDescriptors = ValidateDescriptors(decoder);
		int count = dataDescriptors.Count;
		object[] array = new object[count];
		for (int i = 0; i < count; i++)
		{
			long dataOffset = _viewOffset + dataDescriptors.FieldOffset[i];
			array[i] = decoder.GetValue(ref dataOffset, dataDescriptors[i]);
		}
		return array;
	}

	/// <summary>
	/// The method gets the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.LabelValuePair" /> object at the specified index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <param name="decoder">
	/// The log reader.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.LabelValuePair" /> object or null if the index exceeds the
	/// number of elements in the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.LabelValuePair" /> collection.
	/// </returns>
	public LabelValuePair GetItemAt(int index, ILogDecoder decoder)
	{
		List<LabelValuePair> list = ReadLabelValuePairs(decoder);
		if (list.Count == 0 || index >= list.Count)
		{
			return null;
		}
		return list[index];
	}

	/// <summary>
	/// Returns the value of a specific field.
	/// </summary>
	/// <param name="index">Field number</param>
	/// <param name="decoder">
	/// The log reader.
	/// </param>
	/// <returns>The value, as the nearest .net type</returns>
	public object GetValueAt(ILogDecoder decoder, int index)
	{
		DataDescriptors dataDescriptors = ValidateDescriptors(decoder);
		long dataOffset = _viewOffset + dataDescriptors.FieldOffset[index];
		return decoder.GetValue(ref dataOffset, dataDescriptors[index]);
	}

	/// <summary>
	///     The method checks the cache to see if the label value pairs have been read. If not, it will read them and
	///     use the Data Descriptors to decipher them, cache them, and returns them.
	/// </summary>
	/// <param name="decoder">
	/// The log reader.
	/// </param>
	/// <param name="validateReads">data stream may not contain all records. Validate each read</param>
	/// <returns>
	///     The <see cref="T:System.Collections.Generic.List`1" /> of label value pairs.
	/// </returns>
	/// <exception cref="T:System.Exception">
	///     Thrown if the descriptors are empty - there is no way to interpret the blob.
	/// </exception>
	public virtual List<LabelValuePair> ReadLabelValuePairs(ILogDecoder decoder, bool validateReads = false)
	{
		DataDescriptors dataDescriptors = ValidateDescriptors(decoder);
		List<LabelValuePair> list = new List<LabelValuePair>(dataDescriptors.Count);
		long viewOffset = _viewOffset;
		bool[] array = ValidItems(dataDescriptors, viewOffset, decoder);
		if (validateReads && viewOffset + dataDescriptors.TotalDataSize > decoder.Available)
		{
			decoder = new LogDecoder(new MemoryArrayReader(new byte[dataDescriptors.TotalDataSize], viewOffset), dataDescriptors);
		}
		for (int i = 0; i < dataDescriptors.Count; i++)
		{
			DataDescriptor dataDescriptor = dataDescriptors[i];
			if (array[i])
			{
				long dataOffset = viewOffset + dataDescriptors.FieldOffset[i];
				object value = decoder.DecipherValue(ref dataOffset, dataDescriptor);
				list.Add(new LabelValuePair(dataDescriptor.Label, new GenericValue(dataDescriptor, value)));
			}
		}
		return list;
	}

	/// <summary>
	/// Test if this is a valid item number.
	/// </summary>
	/// <param name="index">
	/// The index of the required item.
	/// </param>
	/// <param name="decoder">
	/// The log reader.
	/// </param>
	/// <returns>
	/// True if this is a valid item.
	/// </returns>
	public bool ValidItem(int index, ILogDecoder decoder)
	{
		DataDescriptors dataDescriptors = ValidateDescriptors(decoder);
		if (index >= 0)
		{
			return index < dataDescriptors.Count;
		}
		return false;
	}

	/// <summary>
	/// Validate descriptors.
	/// </summary>
	/// <exception cref="T:System.Exception">on null descriptors
	/// </exception>
	private DataDescriptors ValidateDescriptors(ILogDecoder decoder)
	{
		DataDescriptors descriptors = decoder.Descriptors;
		if (descriptors == null || descriptors.Count == 0)
		{
			throw new Exception("The descriptors for the blob cannot be null or empty!");
		}
		return descriptors;
	}

	/// <summary>
	/// decode which log items are valid.
	/// </summary>
	/// <param name="descriptors">data types of fields</param>
	/// <param name="startPos">
	/// The start position on the view.
	/// </param>
	/// <param name="decoder">
	/// The log reader.
	/// </param>
	/// <returns>
	/// .// array of valid item flags
	/// </returns>
	private bool[] ValidItems(DataDescriptors descriptors, long startPos, ILogDecoder decoder)
	{
		int count = descriptors.Count;
		if (count > 0)
		{
			DataDescriptor dataDescriptor = descriptors[0];
			string label = dataDescriptor.Label;
			if (label.Length == 1 && label[0] == '\u0001')
			{
				DataTypes dataType = dataDescriptor.DataType;
				if (dataType != DataTypes.CharString && dataType != DataTypes.WideCharString)
				{
					return descriptors.AllValid;
				}
				if (decoder.DecipherValue(ref startPos, dataDescriptor) is string text && text.Length >= count - 1)
				{
					bool[] array = new bool[count];
					array[0] = false;
					for (int i = 1; i < count; i++)
					{
						array[i] = text[i - 1] == '\t';
					}
					return array;
				}
			}
		}
		return descriptors.AllValid;
	}
}
