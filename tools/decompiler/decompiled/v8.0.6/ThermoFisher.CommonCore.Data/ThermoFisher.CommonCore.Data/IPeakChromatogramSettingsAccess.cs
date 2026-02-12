using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to peak chromatogram settings
/// </summary>
public interface IPeakChromatogramSettingsAccess
{
	/// <summary>
	/// Gets the scan filter.
	/// This determines which scans are included in the chromatogram.
	/// </summary>
	string Filter { get; }

	/// <summary>
	/// Gets the chromatogram settings.
	/// This defines how data for a chromatogram point is constructed from a scan.
	/// </summary>
	IChromatogramTraceSettingsAccess ChroSettings { get; }

	/// <summary>
	/// Gets the chromatogram settings
	/// When there is a trace operator set,
	/// This defines how data for a chromatogram point is constructed from a scan for the chromatogram
	/// to be added or subtracted.
	/// </summary>
	IChromatogramTraceSettingsAccess ChroSettings2 { get; }

	/// <summary>
	/// Gets the device type.
	/// This defines which data stream within the raw file is used. 
	/// </summary>
	Device Instrument { get; }

	/// <summary>
	/// Gets the instrument index (starting from 1).
	/// For example: "3" for the third UV detector.
	/// </summary>
	int InstrumentIndex { get; }

	/// <summary>
	/// Gets the trace operator.
	/// If the operator is not "None" then a second chromatogram can be added to or subtracted from the first.
	/// </summary>
	TraceOperator TraceOperator { get; }
}
