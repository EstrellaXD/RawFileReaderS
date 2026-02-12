namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// RunHeader structure
/// The offset where the field begins
/// </summary>
internal enum RunHeaderFieldOffset
{
	/// <summary>
	/// The last spectrum.
	/// </summary>
	LastSpectrum = 12,
	/// <summary>
	/// The number status log.
	/// </summary>
	NumStatusLog = 16,
	/// <summary>
	/// The number error log.
	/// </summary>
	NumErrorLog = 20,
	/// <summary>
	/// The comment1       
	/// </summary>
	Comment1 = 352,
	/// <summary>
	/// Is in acquisition flag
	/// c# bool is an alias for System.Boolean just as integer
	/// </summary>
	IsInAcquisition = 584,
	/// <summary>
	/// The mass resolution.
	/// </summary>
	MassResolution = 3712,
	/// <summary>
	/// The expected run time.
	/// </summary>
	ExpectedRunTime = 3720,
	/// <summary>
	/// The number trailer scan events
	/// </summary>
	NumTrailerScanEvents = 7376,
	/// <summary>
	/// The number trailer extra
	/// </summary>
	NumTrailerExtra = 7380,
	/// <summary>
	/// The number tune data
	/// </summary>
	NumTuneData = 7384,
	/// <summary>
	/// The filter mass precision
	/// </summary>
	FilterMassPrecision = 7404
}
