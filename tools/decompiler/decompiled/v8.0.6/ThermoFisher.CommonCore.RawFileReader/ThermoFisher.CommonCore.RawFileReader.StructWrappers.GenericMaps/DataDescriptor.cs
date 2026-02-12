using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.GenericItems;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

/// <summary>
/// The descriptor for a generic data item. It contains information 
/// (e.g. label, type, size, item offset, etc) for loading the 
/// data from the raw file.
/// </summary>
internal sealed class DataDescriptor : IRawObjectBase
{
	private GenericDataItemStruct _dataItemStruct;

	/// <summary>
	/// Gets the data type.
	/// </summary>
	internal DataTypes DataType => _dataItemStruct.DataType;

	/// <summary>
	/// Gets a value indicating whether to use scientific notation.
	/// </summary>
	internal bool IsScientificNotation { get; private set; }

	/// <summary>
	/// Gets the size of the data item in bytes.
	/// </summary>
	internal uint ItemSize { get; private set; }

	/// <summary>
	/// Gets the label for the data item.
	/// </summary>
	internal string Label { get; private set; }

	/// <summary>
	/// Gets the length (for strings) or precision (for floats and doubles).
	/// </summary>
	internal uint LengthOrPrecision { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.DataDescriptor" /> class.
	/// </summary>
	public DataDescriptor()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.DataDescriptor" /> class.
	/// </summary>
	/// <param name="label">
	/// The label.
	/// </param>
	/// <param name="dataType">
	/// The data type.
	/// </param>
	/// <param name="stringLengthOrPrecision">
	/// The string length or precision.
	/// </param>
	public DataDescriptor(string label, DataTypes dataType, uint stringLengthOrPrecision)
	{
		Label = label;
		_dataItemStruct.DataType = dataType;
		_dataItemStruct.StringLengthOrPrecision = stringLengthOrPrecision;
		SetScientificNotation();
		SetSizeAndPrecision();
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
		long startPos = dataOffset;
		DataTypes dataType = (DataTypes)viewer.ReadIntExt(ref startPos);
		uint stringLengthOrPrecision = viewer.ReadUnsignedIntExt(ref startPos);
		_dataItemStruct = new GenericDataItemStruct
		{
			DataType = dataType,
			StringLengthOrPrecision = stringLengthOrPrecision
		};
		Label = viewer.ReadStringExt(ref startPos);
		SetScientificNotation();
		SetSizeAndPrecision();
		return startPos - dataOffset;
	}

	/// <summary>
	/// The get internal generic data item struct.
	/// </summary>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.GenericItems.GenericDataItemStruct" />.
	/// </returns>
	public GenericDataItemStruct GetInternalGenericDataItemStruct()
	{
		return _dataItemStruct;
	}

	/// <summary>
	/// The method sets the <see cref="P:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.DataDescriptor.IsScientificNotation" /> flag.
	/// </summary>
	private void SetScientificNotation()
	{
		IsScientificNotation = (_dataItemStruct.DataType == DataTypes.Float || _dataItemStruct.DataType == DataTypes.Double) && _dataItemStruct.StringLengthOrPrecision >> 16 != 0;
	}

	/// <summary>
	/// The method sets the size and and the precision/length of the data.
	/// </summary>
	private void SetSizeAndPrecision()
	{
		uint count = 1u;
		switch (_dataItemStruct.DataType)
		{
		case DataTypes.Float:
		case DataTypes.Double:
			LengthOrPrecision = _dataItemStruct.StringLengthOrPrecision & 0xFFFF;
			break;
		case DataTypes.CharString:
		case DataTypes.WideCharString:
			LengthOrPrecision = _dataItemStruct.StringLengthOrPrecision;
			count = LengthOrPrecision;
			break;
		}
		ItemSize = DataSizes.Instance.SizeInByes(_dataItemStruct.DataType, count);
	}
}
