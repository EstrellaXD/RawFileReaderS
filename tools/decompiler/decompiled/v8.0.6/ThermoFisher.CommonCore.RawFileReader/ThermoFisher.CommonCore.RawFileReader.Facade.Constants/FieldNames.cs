namespace ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

/// <summary>
/// The field names.
/// </summary>
internal static class FieldNames
{
	/// <summary>
	/// The global mutex name prefix
	/// </summary>
	public const string GlobalMutexNamePrefix = "Global\\CBaseIO";

	public const string GlobalNameSpacePrefix = "Global\\";

	public const string RawFileInfoMutexPrefix = "RFIMutex";

	public const string MapNameRawFileInfoPostfix = "FMAT_RAWFILEINFO";

	public const string MapNameRunHeaderPostfix = "FMAT_RUNHEADER";

	public const string MapNameInsrumentIdPostfix = "INSTID";

	public const string MapNameStatusLogHeaderPostfix = "STATUSLOGHEADER";

	public const string MapNameStatusLogEntryPostfix = "STATUS_LOG";

	public const string MapNameErrorLogEntryPostfix = "ERROR_LOG";

	public const string MapNamePeakDataPostfix = "PEAKDATA";

	public const string MapNameUvScanIndexPostfix = "UVSCANINDEX";

	public const string MapNameMsScanEventsPostfix = "SCANEVENTS";

	public const string MapNameTrailerHeaderPostfix = "TRAILERHEADER";

	public const string MapNameTuneDataHeaderPostfix = "TUNEDATAHEADER";

	public const string MapNameTuneDataPostfix = "TUNEDATA_FILEMAP";

	public const string MapNameScanHeaderPostfix = "SCANHEADER";

	public const string MapNameTrailerScanEventPostfix = "TRAILER_EVENTS";

	public const string MapNameTrailerExtraPostfix = "TRAILEREXTRA";

	public const string StreamNameSpectrumFilePostfix = "SPECTRUM";
}
