namespace ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

/// <summary>
/// The raw file writer constants.  
/// Imported legacy error and severity values from platform to maintain values.
/// </summary>
internal static class RawFileWriterConstants
{
	public const int RawFileWriterErrorNoMutex = 50;

	public const int RawFileWriterSeverityBits = 2;

	public const int RawFileWriterSeverityShift = 30;

	public const int RawFileWriterSeveritySuccess = 0;

	public const int RawFileWriterSeverityInfo = 1;

	public const int RawFileWriterSeverityWarning = 2;

	public const int RawFileWriterSeverityError = 3;

	public const int MaxUserLabels = 5;

	public const int MaxExtraUserColumns = 15;
}
