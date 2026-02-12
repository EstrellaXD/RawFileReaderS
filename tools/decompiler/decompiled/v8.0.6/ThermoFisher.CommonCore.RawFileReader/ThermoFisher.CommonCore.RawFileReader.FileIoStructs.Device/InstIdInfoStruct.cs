using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device;

/// <summary>
///     Structure for Instrument Id Information.
/// </summary>
internal struct InstIdInfoStruct
{
	internal bool IsValid;

	internal AbsorbanceUnits AbsorbanceUnit;
}
