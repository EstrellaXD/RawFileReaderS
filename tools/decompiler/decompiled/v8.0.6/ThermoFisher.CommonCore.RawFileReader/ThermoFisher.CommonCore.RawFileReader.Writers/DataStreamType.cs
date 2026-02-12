namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Enumeration of possible data streams during acquisition.
/// </summary>
internal enum DataStreamType
{
	/// <summary>
	/// The instrument id file.
	/// </summary>
	InstrumentIdFile,
	/// <summary>
	/// The status log header file.
	/// </summary>
	StatusLogHeaderFile,
	/// <summary>
	/// The status log file.
	/// </summary>
	StatusLogFile,
	/// <summary>
	/// The error log file.
	/// </summary>
	ErrorLogFile,
	/// <summary>
	/// The data (peak) packet file.
	/// </summary>
	DataPacketFile,
	/// <summary>
	/// The spectrum file (scan index file).
	/// </summary>
	SpectrumFile,
	/// <summary>
	/// The scan events file
	/// </summary>
	ScanEventsFile,
	/// <summary>
	/// The trailer header file
	/// </summary>
	TrailerExtraHeaderFile,
	/// <summary>
	/// The trailer extra file
	/// </summary>
	TrailerExtraDataFile,
	/// <summary>
	/// The tune data header file
	/// </summary>
	TuneDataHeaderFile,
	/// <summary>
	/// The tune data file
	/// </summary>
	TuneDataFile,
	/// <summary>
	/// The trailer scan events file
	/// </summary>
	TrailerScanEventsFile,
	/// <summary>
	/// The end of type.
	/// </summary>
	EndOfType
}
