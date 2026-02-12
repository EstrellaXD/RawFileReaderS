using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.GenericItems;

/// <summary>
///     Generic Data Item structure.
/// </summary>
internal struct GenericDataItemStruct
{
	internal DataTypes DataType;

	/// <summary>
	/// String length for <see cref="F:ThermoFisher.CommonCore.RawFileReader.Facade.Constants.DataTypes.CharString" />
	/// <see cref="F:ThermoFisher.CommonCore.RawFileReader.Facade.Constants.DataTypes.WideCharString" />
	/// or precision for FLOAT and DOUBLE.
	/// For FLOAT and DOUBLE, set LOWORD(n) = precision, 
	/// and HIWORD(n) = 0 for normal format and = 1 for 
	/// scientific notation (e.g. 1.056e-7)
	/// </summary>
	internal uint StringLengthOrPrecision;
}
