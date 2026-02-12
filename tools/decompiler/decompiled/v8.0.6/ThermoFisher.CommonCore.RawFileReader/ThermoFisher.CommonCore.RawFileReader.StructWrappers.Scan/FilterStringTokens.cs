using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
/// The filter string tokens, as used by filer parser.
/// </summary>
internal static class FilterStringTokens
{
	private static readonly string[] MetaFilterNames;

	private static readonly string[] IonizationModeNames;

	private static readonly string[] MassAnalyzerNames;

	private static readonly string[] SectorScanNames;

	private static readonly string[] LockNames;

	private static readonly string[] FreeRegionNames;

	private static readonly string[] UltraNames;

	private static readonly string[] EnhancedNames;

	private static readonly string[] ParamANames;

	private static readonly string[] ParamBNames;

	private static readonly string[] ParamFNames;

	private static readonly string[] SpsMultiNotchNames;

	private static readonly string[] ParamRNames;

	private static readonly string[] ParamVNames;

	private static readonly string[] MultiPhotonDissociationNames;

	private static readonly string[] ElectronCaptureDissociationNames;

	private static readonly string[] PhotoIonizationNames;

	private static readonly string[] PolarityNames;

	private static readonly string[] ScanDataTypeNames;

	private static readonly string[] CoronaNames;

	private static readonly string[] DataDependentNames;

	private static readonly string[] WideBandNames;

	private static readonly string[] SupplementalActivationNames;

	private static readonly string[] MultiStateActivationNames;

	private static readonly string[] AccurateMassNames;

	private static readonly string[] TurboScanNames;

	private static readonly string[] ScanModeNames;

	private static readonly string[] MultiplexNames;

	private static readonly string[] DetectorNames;

	/// <summary>
	/// Gets the meta filter token values.
	/// </summary>
	public static int[] MetaFilterTokenValues { get; }

	/// <summary>
	/// Gets the meta filter token names.
	/// </summary>
	public static string[] MetaFilterTokenNames => MetaFilterNames;

	/// <summary>
	/// Gets the ionization mode token names.
	/// </summary>
	public static string[] IonizationModeTokenNames => IonizationModeNames;

	/// <summary>
	/// Gets the ionization mode token values.
	/// </summary>
	public static ScanFilterEnums.IonizationModes[] IonizationModeTokenValues { get; }

	/// <summary>
	/// Gets the mass analyzer token names.
	/// </summary>
	public static string[] MassAnalyzerTokenNames => MassAnalyzerNames;

	/// <summary>
	/// Gets the mass analyzer token values.
	/// </summary>
	public static ScanFilterEnums.MassAnalyzerTypes[] MassAnalyzerTokenValues { get; }

	/// <summary>
	/// Gets the sector scan token names.
	/// </summary>
	public static string[] SectorScanTokenNames => SectorScanNames;

	/// <summary>
	/// Gets the sector scan token values.
	/// </summary>
	public static ScanFilterEnums.SectorScans[] SectorScanTokenValues { get; }

	/// <summary>
	/// Gets the lock token names.
	/// </summary>
	public static string[] LockTokenNames => LockNames;

	/// <summary>
	/// Gets the lock token values.
	/// </summary>
	public static ScanFilterEnums.OnOffTypes[] LockTokenValues { get; }

	/// <summary>
	/// Gets the free region token names.
	/// </summary>
	public static string[] FreeRegionTokenNames => FreeRegionNames;

	/// <summary>
	/// Gets the free region token values.
	/// </summary>
	public static ScanFilterEnums.FreeRegions[] FreeRegionTokenValues { get; }

	/// <summary>
	/// Gets the ultra token names.
	/// </summary>
	public static string[] UltraTokenNames => UltraNames;

	/// <summary>
	/// Gets the ultra token values.
	/// </summary>
	public static ScanFilterEnums.OnOffTypes[] UltraTokenValues { get; }

	/// <summary>
	/// Gets the enhanced token names.
	/// </summary>
	public static string[] EnhancedTokenNames => EnhancedNames;

	/// <summary>
	/// Gets the enhanced token values.
	/// </summary>
	public static ScanFilterEnums.OnOffTypes[] EnhancedTokenValues { get; }

	/// <summary>
	/// Gets the parameter a token names.
	/// </summary>
	public static string[] ParamATokenNames => ParamANames;

	/// <summary>
	/// Gets the parameter a token values.
	/// </summary>
	public static ScanFilterEnums.OffOnTypes[] ParamATokenValues { get; }

	/// <summary>
	/// Gets the parameter b token names.
	/// </summary>
	public static string[] ParamBTokenNames => ParamBNames;

	/// <summary>
	/// Gets the parameter b token values.
	/// </summary>
	public static ScanFilterEnums.OffOnTypes[] ParamBTokenValues { get; }

	/// <summary>
	/// Gets the parameter f token names.
	/// </summary>
	public static string[] ParamFTokenNames => ParamFNames;

	/// <summary>
	/// Gets the parameter f token values.
	/// </summary>
	public static ScanFilterEnums.OffOnTypes[] ParamFTokenValues { get; }

	/// <summary>
	/// Gets the SPS multi notch token names.
	/// </summary>
	public static string[] SpsMultiNotchTokenNames => SpsMultiNotchNames;

	/// <summary>
	/// Gets the SPS multi notch token values.
	/// </summary>
	public static ScanFilterEnums.OffOnTypes[] SpsMultiNotchTokenValues { get; }

	/// <summary>
	/// Gets the parameter r token names.
	/// </summary>
	public static string[] ParamRTokenNames => ParamRNames;

	/// <summary>
	/// Gets the parameter r token values.
	/// </summary>
	public static ScanFilterEnums.OffOnTypes[] ParamRTokenValues { get; }

	/// <summary>
	/// Gets the parameter v token names.
	/// </summary>
	public static string[] ParamVTokenNames => ParamVNames;

	/// <summary>
	/// Gets the parameter v token values.
	/// </summary>
	public static ScanFilterEnums.OffOnTypes[] ParamVTokenValues { get; }

	/// <summary>
	/// Gets the multi photon dissociation token names.
	/// </summary>
	public static string[] MultiPhotonDissociationTokenNames => MultiPhotonDissociationNames;

	/// <summary>
	/// Gets the multi photon dissociation token values.
	/// </summary>
	public static ScanFilterEnums.OnAnyOffTypes[] MultiPhotonDissociationTokenValues { get; }

	/// <summary>
	/// Gets the electron capture dissociation token names.
	/// </summary>
	public static string[] ElectronCaptureDissociationTokenNames => ElectronCaptureDissociationNames;

	/// <summary>
	/// Gets the electron capture dissociation token values.
	/// </summary>
	public static ScanFilterEnums.OnAnyOffTypes[] ElectronCaptureDissociationTokenValues { get; }

	/// <summary>
	/// Gets the photo ionization token names.
	/// </summary>
	public static string[] PhotoIonizationTokenNames => PhotoIonizationNames;

	/// <summary>
	/// Gets the photo ionization token values.
	/// </summary>
	public static ScanFilterEnums.OnOffTypes[] PhotoIonizationTokenValues { get; }

	/// <summary>
	/// Gets the polarity token names.
	/// </summary>
	public static string[] PolarityTokenNames => PolarityNames;

	/// <summary>
	/// Gets the polarity token values.
	/// </summary>
	public static ScanFilterEnums.PolarityTypes[] PolarityTokenValues { get; }

	/// <summary>
	/// Gets the scan data type token names.
	/// </summary>
	public static string[] ScanDataTypeTokenNames => ScanDataTypeNames;

	/// <summary>
	/// Gets the scan data type token values.
	/// </summary>
	public static ScanFilterEnums.ScanDataTypes[] ScanDataTypeTokenValues { get; }

	/// <summary>
	/// Gets the corona token names.
	/// </summary>
	public static string[] CoronaTokenNames => CoronaNames;

	/// <summary>
	/// Gets the corona token values.
	/// </summary>
	public static ScanFilterEnums.OnOffTypes[] CoronaTokenValues { get; }

	/// <summary>
	/// Gets the source fragmentation token names.
	/// </summary>
	public static string[] SourceFragmentationTokenNames { get; }

	/// <summary>
	/// Gets the source fragmentation token values.
	/// </summary>
	public static ScanFilterEnums.OnOffTypes[] SourceFragmentationTokenValues { get; }

	/// <summary>
	/// Gets the compensation voltage token names.
	/// </summary>
	public static string[] CompensationVoltageTokenNames { get; }

	/// <summary>
	/// Gets the compensation voltage token values.
	/// </summary>
	public static ScanFilterEnums.OnOffTypes[] CompensationVoltageTokenValues { get; }

	/// <summary>
	/// Gets the data dependent token names.
	/// </summary>
	public static string[] DataDependentTokenNames => DataDependentNames;

	/// <summary>
	/// Gets the data dependent token values.
	/// </summary>
	public static ScanFilterEnums.IsDependent[] DataDependentTokenValues { get; }

	/// <summary>
	/// Gets the wide band token names.
	/// </summary>
	public static string[] WideBandTokenNames => WideBandNames;

	/// <summary>
	/// Gets the wide band token values.
	/// </summary>
	public static ScanFilterEnums.OffOnTypes[] WideBandTokenValues { get; }

	/// <summary>
	/// Gets the supplemental activation token names.
	/// </summary>
	public static string[] SupplementalActivationTokenNames => SupplementalActivationNames;

	/// <summary>
	/// Gets the supplemental activation token values.
	/// </summary>
	public static ScanFilterEnums.OffOnTypes[] SupplementalActivationTokenValues { get; }

	/// <summary>
	/// Gets the multi state activation token names.
	/// </summary>
	public static string[] MultiStateActivationTokenNames => MultiStateActivationNames;

	/// <summary>
	/// Gets the multi state activation token values.
	/// </summary>
	public static ScanFilterEnums.OffOnTypes[] MultiStateActivationTokenValues { get; }

	/// <summary>
	/// Gets the accurate mass token names.
	/// </summary>
	public static string[] AccurateMassTokenNames => AccurateMassNames;

	/// <summary>
	/// Gets the accurate mass token values.
	/// </summary>
	public static ScanFilterEnums.AccurateMassTypes[] AccurateMassTokenValues { get; }

	/// <summary>
	/// Gets the turbo scan token names.
	/// </summary>
	public static string[] TurboScanTokenNames => TurboScanNames;

	/// <summary>
	/// Gets the turbo scan token values.
	/// </summary>
	public static ScanFilterEnums.OnOffTypes[] TurboScanTokenValues { get; }

	/// <summary>
	/// Gets the scan mode token names.
	/// </summary>
	public static string[] ScanModeTokenNames => ScanModeNames;

	/// <summary>
	/// Gets the scan mode token values.
	/// </summary>
	public static ScanFilterEnums.ScanTypes[] ScanModeTokenValues { get; }

	/// <summary>
	/// Gets the multiplex token names.
	/// </summary>
	public static string[] MultiplexTokenNames => MultiplexNames;

	/// <summary>
	/// Gets the multiplex token values.
	/// </summary>
	public static ScanFilterEnums.OffOnTypes[] MultiplexTokenValues { get; }

	/// <summary>
	/// Gets the detector token names.
	/// </summary>
	public static string[] DetectorTokenNames => DetectorNames;

	/// <summary>
	/// Gets the detector token values.
	/// </summary>
	public static ScanFilterEnums.DetectorType[] DetectorTokenValues { get; }

	/// <summary>
	/// Initializes static members of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.FilterStringTokens" /> class.
	/// </summary>
	static FilterStringTokens()
	{
		MetaFilterTokenValues = Utilities.GetEnumValues<int>(typeof(MetaFilterType), out MetaFilterNames);
		IonizationModeTokenValues = Utilities.GetEnumValues<ScanFilterEnums.IonizationModes>(typeof(ScanFilterEnums.IonizationModes), out IonizationModeNames);
		MassAnalyzerTokenValues = Utilities.GetEnumValues<ScanFilterEnums.MassAnalyzerTypes>(typeof(ScanFilterEnums.MassAnalyzerTypes), out MassAnalyzerNames);
		SectorScanTokenValues = Utilities.GetEnumValues<ScanFilterEnums.SectorScans>(typeof(ScanFilterEnums.SectorScans), out SectorScanNames);
		LockTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OnOffTypes>(typeof(ScanFilterEnums.OnOffTypes), out LockNames, "lock");
		FreeRegionTokenValues = Utilities.GetEnumValues<ScanFilterEnums.FreeRegions>(typeof(ScanFilterEnums.FreeRegions), out FreeRegionNames);
		UltraTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OnOffTypes>(typeof(ScanFilterEnums.OnOffTypes), out UltraNames, "u");
		EnhancedTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OnOffTypes>(typeof(ScanFilterEnums.OnOffTypes), out EnhancedNames, "E");
		ParamATokenValues = Utilities.GetEnumValues<ScanFilterEnums.OffOnTypes>(typeof(ScanFilterEnums.OffOnTypes), out ParamANames, "a");
		ParamBTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OffOnTypes>(typeof(ScanFilterEnums.OffOnTypes), out ParamBNames, "b");
		ParamFTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OffOnTypes>(typeof(ScanFilterEnums.OffOnTypes), out ParamFNames, "f");
		SpsMultiNotchTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OffOnTypes>(typeof(ScanFilterEnums.OffOnTypes), out SpsMultiNotchNames, "sps");
		ParamRTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OffOnTypes>(typeof(ScanFilterEnums.OffOnTypes), out ParamRNames, "r");
		ParamVTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OffOnTypes>(typeof(ScanFilterEnums.OffOnTypes), out ParamVNames, "v");
		MultiPhotonDissociationTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OnAnyOffTypes>(typeof(ScanFilterEnums.OnAnyOffTypes), out MultiPhotonDissociationNames, "mpd");
		ElectronCaptureDissociationTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OnAnyOffTypes>(typeof(ScanFilterEnums.OnAnyOffTypes), out ElectronCaptureDissociationNames, "ecd");
		PhotoIonizationTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OnOffTypes>(typeof(ScanFilterEnums.OnOffTypes), out PhotoIonizationNames, "pi");
		PolarityTokenValues = Utilities.GetEnumValues<ScanFilterEnums.PolarityTypes>(typeof(ScanFilterEnums.PolarityTypes), out PolarityNames);
		ScanDataTypeTokenValues = Utilities.GetEnumValues<ScanFilterEnums.ScanDataTypes>(typeof(ScanFilterEnums.ScanDataTypes), out ScanDataTypeNames);
		CoronaTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OnOffTypes>(typeof(ScanFilterEnums.OnOffTypes), out CoronaNames, "corona");
		SourceFragmentationTokenNames = new string[3] { "sid=", "sid", "!sid" };
		SourceFragmentationTokenValues = new ScanFilterEnums.OnOffTypes[3]
		{
			ScanFilterEnums.OnOffTypes.On,
			ScanFilterEnums.OnOffTypes.On,
			ScanFilterEnums.OnOffTypes.Off
		};
		CompensationVoltageTokenNames = new string[3] { "cv=", "cv", "!cv" };
		CompensationVoltageTokenValues = new ScanFilterEnums.OnOffTypes[3]
		{
			ScanFilterEnums.OnOffTypes.On,
			ScanFilterEnums.OnOffTypes.On,
			ScanFilterEnums.OnOffTypes.Off
		};
		DataDependentTokenValues = Utilities.GetEnumValues<ScanFilterEnums.IsDependent>(typeof(ScanFilterEnums.IsDependent), out DataDependentNames, "d");
		WideBandTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OffOnTypes>(typeof(ScanFilterEnums.OffOnTypes), out WideBandNames, "w");
		SupplementalActivationTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OffOnTypes>(typeof(ScanFilterEnums.OffOnTypes), out SupplementalActivationNames, "sa");
		MultiStateActivationTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OffOnTypes>(typeof(ScanFilterEnums.OffOnTypes), out MultiStateActivationNames, "msa");
		AccurateMassTokenValues = Utilities.GetEnumValues<ScanFilterEnums.AccurateMassTypes>(typeof(ScanFilterEnums.AccurateMassTypes), out AccurateMassNames);
		TurboScanTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OnOffTypes>(typeof(ScanFilterEnums.OnOffTypes), out TurboScanNames, "t");
		ScanModeTokenValues = Utilities.GetEnumValues<ScanFilterEnums.ScanTypes>(typeof(ScanFilterEnums.ScanTypes), out ScanModeNames);
		MultiplexTokenValues = Utilities.GetEnumValues<ScanFilterEnums.OffOnTypes>(typeof(ScanFilterEnums.OffOnTypes), out MultiplexNames, "msx");
		DetectorTokenValues = Utilities.GetEnumValues<ScanFilterEnums.DetectorType>(typeof(ScanFilterEnums.DetectorType), out DetectorNames, "det");
	}
}
