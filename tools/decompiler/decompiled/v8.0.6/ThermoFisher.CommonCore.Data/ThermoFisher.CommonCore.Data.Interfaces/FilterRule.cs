namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The filter rule.
/// A scan filter has a list of rules to apply.
/// </summary>
public enum FilterRule
{
	/// <summary>
	/// Apply the meta filter.
	/// </summary>
	MetaFilter,
	/// <summary>
	/// Apply the data dependent On filter.
	/// </summary>
	DependentOn,
	/// <summary>
	/// Apply the data dependent Off filter.
	/// </summary>
	DependentOff,
	/// <summary>
	/// Apply the supplemental activation filter.
	/// </summary>
	SupplementalActivation,
	/// <summary>
	/// Apply the multi state activation filter.
	/// </summary>
	MultiStateActivation,
	/// <summary>
	/// Apply the wideband filter.
	/// </summary>
	Wideband,
	/// <summary>
	/// Apply the polarity filter.
	/// </summary>
	Polarity,
	/// <summary>
	/// Apply the scan data filter.
	/// </summary>
	ScanData,
	/// <summary>
	/// Apply the ionization mode filter.
	/// </summary>
	IonizationMode,
	/// <summary>
	/// Apply the corona filter.
	/// </summary>
	Corona,
	/// <summary>
	/// Apply the lock filter.
	/// </summary>
	Lock,
	/// <summary>
	/// Apply the field free region filter.
	/// </summary>
	FieldFreeRegion,
	/// <summary>
	/// Apply the ultra filter.
	/// </summary>
	Ultra,
	/// <summary>
	/// Apply the enhanced filter.
	/// </summary>
	Enhanced,
	/// <summary>
	/// Apply the parameter a filter.
	/// </summary>
	ParamA,
	/// <summary>
	/// Apply the parameter b filter.
	/// </summary>
	ParamB,
	/// <summary>
	/// Apply the parameter f filter.
	/// </summary>
	ParamF,
	/// <summary>
	/// Apply the multi notch filter.
	/// </summary>
	MultiNotch,
	/// <summary>
	/// Apply the multiple photon dissociation filter.
	/// </summary>
	MultiplePhotonDissociation,
	/// <summary>
	/// Apply the parameter v filter.
	/// </summary>
	ParamV,
	/// <summary>
	/// Apply the parameter R filter.
	/// </summary>
	ParamR,
	/// <summary>
	/// Apply the electron capture dissociation filter.
	/// </summary>
	ElectronCaptureDissociation,
	/// <summary>
	/// Apply the photo ionization filter.
	/// </summary>
	PhotoIonization,
	/// <summary>
	/// Apply the source fragmentation filter.
	/// </summary>
	SourceFragmentation,
	/// <summary>
	/// Apply the source fragmentation type filter.
	/// </summary>
	SourceFragmentationType,
	/// <summary>
	/// Apply the compensation voltage filter.
	/// </summary>
	CompensationVoltage,
	/// <summary>
	/// Apply the compensation volt type filter.
	/// </summary>
	CompensationVoltType,
	/// <summary>
	/// Apply the detector filter.
	/// </summary>
	Detector,
	/// <summary>
	/// Apply the mass analyzer type filter.
	/// </summary>
	MassAnalyzerType,
	/// <summary>
	/// Apply the sector scan filter.
	/// </summary>
	SectorScan,
	/// <summary>
	/// Apply the turbo scan filter.
	/// </summary>
	TurboScan,
	/// <summary>
	/// Apply the scan mode filter.
	/// </summary>
	ScanMode,
	/// <summary>
	/// Apply the multiplex filter.
	/// </summary>
	Multiplex,
	/// <summary>
	/// Apply The MS order filter.
	/// </summary>
	MsOrder,
	/// <summary>
	/// Apply the scan type index filter.
	/// </summary>
	ScanTypeIndex,
	/// <summary>
	/// Apply the accurate mass filter.
	/// </summary>
	AccurateMass,
	/// <summary>
	/// Apply the lower case E filter
	/// </summary>
	LowerE,
	/// <summary>
	/// Apply the lower case G filter
	/// </summary>
	LowerG,
	/// <summary>
	/// Apply the lower case H filter
	/// </summary>
	LowerH,
	/// <summary>
	/// Apply the lower case I filter
	/// </summary>
	LowerI,
	/// <summary>
	/// Apply the lower case J filter
	/// </summary>
	LowerJ,
	/// <summary>
	/// Apply the lower case K filter
	/// </summary>
	LowerK,
	/// <summary>
	/// Apply the lower case L filter
	/// </summary>
	LowerL,
	/// <summary>
	/// Apply the lower case M filter
	/// </summary>
	LowerM,
	/// <summary>
	/// Apply the lower case N filter
	/// </summary>
	LowerN,
	/// <summary>
	/// Apply the lower case O filter
	/// </summary>
	LowerO,
	/// <summary>
	/// Apply the lower case Q filter
	/// </summary>
	LowerQ,
	/// <summary>
	/// Apply the lower case S filter
	/// </summary>
	LowerS,
	/// <summary>
	/// Apply the lower case X filter
	/// </summary>
	LowerX,
	/// <summary>
	/// Apply the lower case Y filter
	/// </summary>
	LowerY,
	/// <summary>
	/// Apply the upper case A filter
	/// </summary>
	UpperA,
	/// <summary>
	/// Apply the upper case B filter
	/// </summary>
	UpperB,
	/// <summary>
	/// Apply the upper case F filter
	/// </summary>
	UpperF,
	/// <summary>
	/// Apply the upper case G filter
	/// </summary>
	UpperG,
	/// <summary>
	/// Apply the upper case H filter
	/// </summary>
	UpperH,
	/// <summary>
	/// Apply the upper case I filter
	/// </summary>
	UpperI,
	/// <summary>
	/// Apply the upper case J filter
	/// </summary>
	UpperJ,
	/// <summary>
	/// Apply the upper case K filter
	/// </summary>
	UpperK,
	/// <summary>
	/// Apply the upper case L filter
	/// </summary>
	UpperL,
	/// <summary>
	/// Apply the upper case M filter
	/// </summary>
	UpperM,
	/// <summary>
	/// Apply the upper case N filter
	/// </summary>
	UpperN,
	/// <summary>
	/// Apply the upper case O filter
	/// </summary>
	UpperO,
	/// <summary>
	/// Apply the upper case Q filter
	/// </summary>
	UpperQ,
	/// <summary>
	/// Apply the upper case R filter
	/// </summary>
	UpperR,
	/// <summary>
	/// Apply the upper case S filter
	/// </summary>
	UpperS,
	/// <summary>
	/// Apply the upper case T filter
	/// </summary>
	UpperT,
	/// <summary>
	/// Apply the upper case U filter
	/// </summary>
	UpperU,
	/// <summary>
	/// Apply the upper case V filter
	/// </summary>
	UpperV,
	/// <summary>
	/// Apply the upper case W filter
	/// </summary>
	UpperW,
	/// <summary>
	/// Apply the upper case X filter
	/// </summary>
	UpperX,
	/// <summary>
	/// Apply the upper case Y filter
	/// </summary>
	UpperY
}
