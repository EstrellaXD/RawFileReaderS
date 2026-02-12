namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Information about this device stream
/// </summary>
public interface IRunHeader
{
	/// <summary>
	/// Gets the count of recorded spectra
	/// </summary>
	int SpectraCount { get; }

	/// <summary>
	/// Gets the count of status log entries
	/// </summary>
	int StatusLogCount { get; }

	/// <summary>
	/// Gets the count of tune data entries
	/// </summary>
	int TuneDataCount { get; }

	/// <summary>
	/// Gets the count of error log entries
	/// </summary>
	int ErrorLogCount { get; }

	/// <summary>
	/// Gets the count of "scan events"
	/// </summary>
	int TrailerScanEventCount { get; }

	/// <summary>
	/// Gets the count of "trailer extra" records.
	/// Typically, same as the count of scans.
	/// </summary>
	int TrailerExtraCount { get; }

	/// <summary>
	/// Gets a value indicating whether this file is being created.
	/// 0: File is complete. All other positive values: The file is in acquisition.
	/// Negative values are undefined.
	/// </summary>
	int InAcquisition { get; }

	/// <summary>
	/// Gets the mass resolution of this instrument.
	/// </summary>
	double MassResolution { get; }

	/// <summary>
	/// Gets the expected data acquisition time.
	/// </summary>
	double ExpectedRunTime { get; }

	/// <summary>
	/// Gets the tolerance units
	/// </summary>
	ToleranceUnits ToleranceUnit { get; }

	/// <summary>
	/// Gets the number of digits of precision suggested for formatting masses
	/// in the filters.
	/// </summary>
	int FilterMassPrecision { get; }

	/// <summary>
	/// Gets the first comment about this data stream.
	/// </summary>
	string Comment1 { get; }

	/// <summary>
	/// Gets the second comment about this data stream.
	/// </summary>
	string Comment2 { get; }

	/// <summary>
	/// Gets the first spectrum (scan) number (typically 1).
	/// </summary>
	int FirstSpectrum { get; }

	/// <summary>
	/// Gets the last spectrum (scan) number.
	/// If this is less than 1, then there are no scans acquired yet.
	/// </summary>
	int LastSpectrum { get; }

	/// <summary>
	/// Gets the time of first scan in file
	/// </summary>
	double StartTime { get; }

	/// <summary>
	/// Gets the time of last scan in file
	/// </summary>
	double EndTime { get; }

	/// <summary>
	/// Gets the lowest recorded mass in file
	/// </summary>
	double LowMass { get; }

	/// <summary>
	/// Gets the highest recorded mass in file
	/// </summary>
	double HighMass { get; }

	/// <summary>
	/// Gets the max intensity.
	/// </summary>
	double MaxIntensity { get; }

	/// <summary>
	/// Gets the max integrated intensity.
	/// </summary>
	double MaxIntegratedIntensity { get; }

	/// <summary>
	/// Gets the protocol used to create this file.
	/// </summary>
	string WriterProtocol => string.Empty;

	/// <summary>
	/// Gets the Data Domain used to create this data channel.
	/// </summary>
	RawDataDomain DeviceDataDomain => RawDataDomain.Legacy;
}
