namespace ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

/// <summary>
///     The raw file constants.
/// </summary>
internal static class RawFileConstants
{
	public const int XcalInitialVersion = 25;

	/// <summary>
	///     Raw file versions below 30 are not supported.
	/// </summary>
	public const int NotSupportedVersion = 30;

	/// <summary>
	/// The current version.
	/// </summary>
	public const int FormatCurrentVersion = 66;

	/// <summary>
	/// The format version03
	/// </summary>
	public const int FormatVersion03 = 3;

	/// <summary>
	/// The format version06
	/// </summary>
	public const int FormatVersion06 = 6;

	/// <summary>
	///     The format version 7.
	/// </summary>
	public const int FormatVersion07 = 7;

	/// <summary>
	/// The format version8
	/// </summary>
	public const int FormatVersion08 = 8;

	/// <summary>
	/// The format version9
	/// </summary>
	public const int FormatVersion09 = 9;

	/// <summary>
	/// The format version11
	/// </summary>
	public const int FormatVersion11 = 11;

	/// <summary>
	/// The format version12
	/// </summary>
	public const int FormatVersion12 = 12;

	/// <summary>
	/// The format version13
	/// </summary>
	public const int FormatVersion13 = 13;

	/// <summary>
	/// The format version14
	/// </summary>
	public const int FormatVersion14 = 14;

	/// <summary>
	/// The format version15
	/// </summary>
	public const int FormatVersion15 = 15;

	/// <summary>
	/// The format version30
	/// </summary>
	public const int FormatVersion30 = 30;

	/// <summary>
	/// The format version 31.
	/// </summary>
	public const int FormatVersion31 = 31;

	/// <summary>
	/// The format version32
	/// </summary>
	public const int FormatVersion32 = 32;

	/// <summary>
	/// The format version34
	/// </summary>
	public const int FormatVersion34 = 34;

	/// <summary>
	/// The format version36
	/// </summary>
	public const int FormatVersion36 = 36;

	/// <summary>
	/// The format version37
	/// </summary>
	public const int FormatVersion37 = 37;

	/// <summary>
	/// The format version39
	/// </summary>
	public const int FormatVersion39 = 39;

	/// <summary>
	/// The format version 40.
	/// </summary>
	public const int FormatVersion40 = 40;

	/// <summary>
	/// The format version 41.
	/// </summary>
	public const int FormatVersion41 = 41;

	/// <summary>
	/// The format version 44.
	/// </summary>
	public const int FormatVersion44 = 44;

	/// <summary>
	/// The format version 45.
	/// </summary>
	public const int FormatVersion45 = 45;

	/// <summary>
	/// The format version 48.
	/// </summary>
	public const int FormatVersion48 = 48;

	/// <summary>
	/// The format version 49.
	/// </summary>
	public const int FormatVersion49 = 49;

	/// <summary>
	/// The format version 51.
	/// </summary>
	public const int FormatVersion51 = 51;

	/// <summary>
	/// The format version 52.
	/// </summary>
	public const int FormatVersion52 = 52;

	/// <summary>
	/// The format version 54.
	/// </summary>
	public const int FormatVersion54 = 54;

	/// <summary>
	/// The format version 58.
	/// </summary>
	public const int FormatVersion58 = 58;

	/// <summary>
	///     The format version 62.
	/// </summary>
	public const int FormatVersion62 = 62;

	/// <summary>
	///     The format version 63.
	/// </summary>
	public const int FormatVersion63 = 63;

	/// <summary>
	///     The format version 65.
	/// </summary>
	public const int FormatVersion65 = 65;

	/// <summary>
	///     The format version 66.
	/// </summary>
	public const int FormatVersion66 = 66;

	/// <summary>
	///     The post 64 bit file size allowance version.
	/// </summary>
	public const int Post64BitFileSizeAllowanceVersion = 64;

	/// <summary>
	///     The pre 64 bit file size allowance version.
	/// </summary>
	public const int Pre64BitFileSizeAllowanceVersion = 63;

	/// <summary>
	///     The max controllers.
	/// </summary>
	public const int MaxControllers = 64;

	/// <summary>
	///     The max user labels.
	/// </summary>
	public const int MaxUserLabels = 5;

	/// <summary>
	///     The original vial string length.
	/// </summary>
	public const int OriginalVialStringLength = 4;

	/// <summary>
	///     The path length.
	/// </summary>
	public const int PathLength = 256;

	/// <summary>
	///     The raw file signature.
	/// </summary>
	public const string RawFileSignature = "Finnigan";

	/// <summary>
	/// The finn identifier
	/// 2-byte ID for Finnigan / Enterprise file
	/// </summary>
	public const ushort FinnId = 41217;

	/// <summary>
	/// The tolerance for filter energy.
	/// </summary>
	public const double FilterEnergyTolerance = 0.01;

	/// <summary>
	/// Used to allow small differences in comparisons (e.g. mass difference).
	/// </summary>
	public const double ToleranceEpsilon = 1E-06;

	/// <summary>
	/// If the scan type is not specified.
	/// </summary>
	public const int ScanTypeIndexNotSpecified = -1;

	public const int DefaultMassPrecision = 3;

	public const string DefaultMassPrecisioFormat = "f3";

	public const int DefaultEnergyPrecision = 2;

	public const string DefaultEnergyPrecisionFormat = "f2";

	public const uint MultipleActivationMask = 4096u;

	public const int MaxComment1StringLength = 40;

	public const int MaxComment2StringLength = 64;

	public const double DefaultFilterMassResolution = 0.4;

	public const int DefaultFilterScanMaxPrecursorMasses = 100;

	public const int DefaultFilterScanMaxMassRangePairs = 100;

	public const int DefaultFilterScanDoubleLen = 9;

	public const int DefaultFilterScanDoublePrecision = 2;

	public const int DefaultFilterScanMassPrecision = 2;

	public const double DefaultMinMass = 0.5;

	public const double DefaultMaxMass = 999999.0;

	public const int DefaultMultipleActivationSize = 10;

	public const int DefaultMulitipleActivation = 4096;

	public const int NoMsOrderSpecified = -10;

	public const int MaxFilterMsOrder = 100;
}
