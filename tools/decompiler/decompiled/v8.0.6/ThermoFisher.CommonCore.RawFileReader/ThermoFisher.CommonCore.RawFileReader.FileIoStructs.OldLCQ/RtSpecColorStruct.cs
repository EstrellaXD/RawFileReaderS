namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The real time spectrum color struct.
/// </summary>
internal struct RtSpecColorStruct
{
	internal uint RegularColor;

	internal uint SaturatedColor;

	internal uint ProfileColor;

	internal uint ZeroColor;

	internal uint HundredColor;

	internal uint ExceptionColor;

	internal uint ReferenceColor;
}
