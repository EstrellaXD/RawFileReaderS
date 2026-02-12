namespace ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

/// <summary>
/// Enumerated types for old LCQ files
/// </summary>
internal static class OldLcqEnums
{
	/// <summary>
	/// The instrument control.
	/// </summary>
	public enum InstrumentControl
	{
		/// <summary>
		/// Start direct.
		/// </summary>
		Direct,
		/// <summary>
		/// Start by contact closure.
		/// </summary>
		ContactClosure
	}

	/// <summary>
	/// The syringe unit.
	/// </summary>
	public enum SyringeUnit
	{
		/// <summary>
		/// ml per min.
		/// </summary>
		MlMin,
		/// <summary>
		/// ml per hour.
		/// </summary>
		MlHr,
		/// <summary>
		/// micro l per min.
		/// </summary>
		UlMin,
		/// <summary>
		/// micro l per hour.
		/// </summary>
		UlHr
	}

	/// <summary>
	/// The syringe brand.
	/// </summary>
	public enum SyringeBrand
	{
		/// <summary>
		/// Made by Hamilton.
		/// </summary>
		Hamilton,
		/// <summary>
		/// Made by <c>Unimetrics</c>.
		/// </summary>
		Unimetrics,
		/// <summary>
		/// Some other make
		/// </summary>
		Other
	}

	/// <summary>
	/// The MS programming type.
	/// </summary>
	public enum MsType
	{
		/// <summary>
		/// is non ITCL.
		/// </summary>
		IsNonItcl,
		/// <summary>
		/// is ITCL. (Ion Trap Control Language)
		/// </summary>
		IsItcl
	}

	/// <summary>
	/// The divert valve state between runs.
	/// </summary>
	public enum DivertBetweenRuns
	{
		/// <summary>
		/// Divert to waste.
		/// </summary>
		DivertToWaste,
		/// <summary>
		/// Divert to source.
		/// </summary>
		DivertToSource
	}

	/// <summary>
	/// The fraction collector choice.
	/// </summary>
	public enum FractionCollectorChoice
	{
		/// <summary>
		/// fraction collector trigger by base peak.
		/// </summary>
		FcBasePeak,
		/// <summary>
		/// fraction collector trigger by molecular weight.
		/// </summary>
		FcMolecularWeight
	}

	/// <summary>
	/// The data mode.
	/// </summary>
	public enum DataMode
	{
		/// <summary>
		/// Scan is profile.
		/// </summary>
		Profile,
		/// <summary>
		/// Scan is centroid.
		/// </summary>
		Centroid
	}

	/// <summary>
	/// The MS scan mode.
	/// </summary>
	public enum MsScanMode
	{
		/// <summary>
		/// The MS mode.
		/// </summary>
		MsMode,
		/// <summary>
		/// The MS/MS mode.
		/// </summary>
		MsMs,
		/// <summary>
		/// CID mode.
		/// </summary>
		Cid,
		/// <summary>
		/// The MS to the nth mode.
		/// </summary>
		MSn
	}

	/// <summary>
	/// The MS scan type.
	/// </summary>
	public enum MsScanType
	{
		/// <summary>
		/// Scan type full.
		/// </summary>
		ScanTypeFull,
		/// <summary>
		/// Scan type SIM.
		/// </summary>
		ScanTypeSim,
		/// <summary>
		/// Scan type zoom.
		/// </summary>
		ScanTypeZoom,
		/// <summary>
		/// The scan type SRM.
		/// </summary>
		ScanTypeSrm
	}

	/// <summary>
	/// The mode of largest.
	/// </summary>
	public enum ModeLargest
	{
		/// <summary>
		/// The nth largest intensity.
		/// </summary>
		NthLargestIntensity,
		/// <summary>
		/// The nth largest intensity precursor.
		/// </summary>
		NthLargestIntensityPrecursor,
		/// <summary>
		/// use previous mass.
		/// </summary>
		UsePreviousMass,
		/// <summary>
		/// Use the previous list of ions, then select the ion from the list
		/// </summary>
		UsePreviousList
	}

	/// <summary>
	/// The show axis label.
	/// </summary>
	public enum ShowAxisLabel
	{
		/// <summary>
		/// show always.
		/// </summary>
		ShowAlways,
		/// <summary>
		/// show on print.
		/// </summary>
		ShowOnPrint,
		/// <summary>
		/// show never.
		/// </summary>
		ShowNever
	}

	/// <summary>
	/// The normalize type.
	/// </summary>
	public enum NormalizeType
	{
		/// <summary>
		/// Normalize to largest in section.
		/// </summary>
		LargestInSection,
		/// <summary>
		/// Normalize to  largest in display.
		/// </summary>
		LargestInDisplay,
		/// <summary>
		/// Normalize to  largest in all data
		/// </summary>
		LargestInAll
	}

	/// <summary>
	/// The spectrum style.
	/// </summary>
	public enum SpectrumStyle
	{
		/// <summary>
		/// auto style based on data
		/// </summary>
		AutoStyle,
		/// <summary>
		/// Use profile style.
		/// </summary>
		ProfileStyle,
		/// <summary>
		/// Use stick style.
		/// </summary>
		StickStyle,
		/// <summary>
		/// Use shade style.
		/// </summary>
		ShadeStyle
	}

	/// <summary>
	/// The chromatogram trace types.
	/// </summary>
	public enum ChroTraceTypes
	{
		/// <summary>
		/// The start of MS chromatogram traces.
		/// </summary>
		StartMsChroTraces = -1,
		/// <summary>
		/// Chromatogram mass range trace.
		/// </summary>
		ChroMassRangeTrace = 0,
		/// <summary>
		/// Chromatogram tic trace.
		/// </summary>
		ChroTicTrace = 1,
		/// <summary>
		/// Chromatogram base peak trace.
		/// </summary>
		ChroBasePeakTrace = 2,
		/// <summary>
		/// Chromatogram fragment trace.
		/// </summary>
		ChroFragmentTrace = 3,
		/// <summary>
		/// The end of MS Chromatogram traces.
		/// </summary>
		EndMsChroTraces = 4,
		/// <summary>
		/// The start analog Chromatogram traces.
		/// </summary>
		StartAnalogChroTraces = 10,
		/// <summary>
		/// Chromatogram analog 1 trace.
		/// </summary>
		ChroAnalog1Trace = 11,
		/// <summary>
		/// Chromatogram analog 2 trace.
		/// </summary>
		ChroAnalog2Trace = 12,
		/// <summary>
		/// Chromatogram analog 3 trace.
		/// </summary>
		ChroAnalog3Trace = 13,
		/// <summary>
		/// Chromatogram analog 4 trace.
		/// </summary>
		ChroAnalog4Trace = 14,
		/// <summary>
		/// The end of analog Chromatogram traces.
		/// </summary>
		EndAnalogChroTraces = 15,
		/// <summary>
		/// The start of PDA Chromatogram traces.
		/// </summary>
		StartPdaChroTraces = 20,
		/// <summary>
		/// Chromatogram wavelength range trace.
		/// </summary>
		ChroWavelengthRangeTrace = 21,
		/// <summary>
		/// Chromatogram total absorbance trace.
		/// </summary>
		ChroTotalAbsorbanceTrace = 22,
		/// <summary>
		/// Chromatogram spectrum max trace.
		/// </summary>
		ChroSpectrumMaxTrace = 23,
		/// <summary>
		/// The end of PDA Chromatogram traces.
		/// </summary>
		EndPdaChroTraces = 24,
		/// <summary>
		/// The start of UV Chromatogram traces.
		/// </summary>
		StartUvChroTraces = 30,
		/// <summary>
		/// Chromatogram channel a trace.
		/// </summary>
		ChroChannelATrace = 31,
		/// <summary>
		/// Chromatogram channel b trace.
		/// </summary>
		ChroChannelBTrace = 32,
		/// <summary>
		/// Chromatogram channel c trace.
		/// </summary>
		ChroChannelCTrace = 33,
		/// <summary>
		/// Chromatogram channel d trace.
		/// </summary>
		ChroChannelDTrace = 34,
		/// <summary>
		/// The end of UV Chromatogram traces.
		/// </summary>
		EndUvChroTraces = 35,
		/// <summary>
		/// The start a2d Chromatogram traces.
		/// </summary>
		StartA2DChroTraces = 40,
		/// <summary>
		/// Chromatogram Analog to Digital channel 1 trace.
		/// </summary>
		ChroA2DChannel1Trace = 41,
		/// <summary>
		/// Chromatogram Analog to Digital channel 2 trace.
		/// </summary>
		ChroA2DChannel2Trace = 42,
		/// <summary>
		/// Chromatogram Analog to Digital channel 3 trace.
		/// </summary>
		ChroA2DChannel3Trace = 43,
		/// <summary>
		/// Chromatogram Analog to Digital channel 4 trace.
		/// </summary>
		ChroA2DChannel4Trace = 44,
		/// <summary>
		/// The end of a2d Chromatogram traces.
		/// </summary>
		EndA2DChroTraces = 45,
		/// <summary>
		/// The end of all Chromatogram traces.
		/// </summary>
		EndAllChroTraces = 46
	}

	/// <summary>
	/// The Chromatogram trace operator. When math is done on two traces, here is the operations allowed
	/// </summary>
	public enum ChroTraceOperator
	{
		/// <summary>
		/// No operator: blank.
		/// </summary>
		Blank,
		/// <summary>
		/// The minus operator.
		/// </summary>
		Minus,
		/// <summary>
		/// The plus operator.
		/// </summary>
		Plus
	}

	/// <summary>
	/// The smoothing types.
	/// Used to describe the types of smoothing available for chromatograms and spectra
	/// </summary>
	public enum SmoothingTypes
	{
		/// <summary>
		/// The box (moving mean) smoothing.
		/// </summary>
		BoxSmoothing,
		/// <summary>
		/// Gaussian smoothing.
		/// </summary>
		GaussSmoothing
	}

	/// <summary>
	/// The tolerance unit.
	/// Used to describe the types of mass tolerance units  for  spectra
	/// </summary>
	public enum ToleranceUnit
	{
		/// <summary>
		/// units MMU.
		/// </summary>
		Mmu,
		/// <summary>
		/// Parts Per Million.
		/// </summary>
		Ppm,
		/// <summary>
		/// Atomic Mass Units (Dalton).
		/// </summary>
		Amu
	}

	/// <summary>
	/// The style.
	/// </summary>
	public enum Style
	{
		/// <summary>
		/// point to point (connected points), profile.
		/// </summary>
		PointToPoint,
		/// <summary>
		/// Stick (vertical lines), centroid data
		/// </summary>
		Stick
	}

	/// <summary>
	/// The scan mode.
	/// </summary>
	public enum ScanMode
	{
		/// <summary>
		/// The no scan.
		/// </summary>
		NoScan = 0,
		/// <summary>
		/// The full scan.
		/// </summary>
		FullScan = 1,
		/// <summary>
		/// Scan is SIM, SRM or CRM.
		/// </summary>
		SimSrmCrm = 2,
		/// <summary>
		/// A zoom scan.
		/// </summary>
		ZoomScan = 3,
		/// <summary>
		/// The MS to MS to the nth.
		/// </summary>
		MstoMsn = 10,
		/// <summary>
		/// The MS to the nth to MS to the nth.
		/// </summary>
		MsntoMsn = 11,
		/// <summary>
		/// The zoom MS to MS to the nth..
		/// </summary>
		ZMstoMsn = 12,
		/// <summary>
		/// The zoom MS to the nth to MS to the nth.
		/// </summary>
		ZMsntoMsn = 14,
		/// <summary>
		/// The MS to zoom MS.
		/// </summary>
		MstozMs = 15,
		/// <summary>
		/// The MS to the nth to zoom MS to the nth.
		/// </summary>
		MsntozMsn = 16,
		/// <summary>
		/// neutral loss scan.
		/// </summary>
		NeutralLoss = 24,
		/// <summary>
		/// neutral gain scan.
		/// </summary>
		NeutralGain = 25,
		/// <summary>
		/// parent scan.
		/// </summary>
		Parent = 26
	}

	/// <summary>
	/// The map view accessor.
	/// </summary>
	public enum MapViewAccessor
	{
		/// <summary>
		/// Map scan index.
		/// </summary>
		ScanIndex,
		/// <summary>
		/// Map status log.
		/// </summary>
		StatusLog,
		/// <summary>
		/// Map trailer extra.
		/// </summary>
		TrailerExtra,
		/// <summary>
		/// Map tune data.
		/// </summary>
		TuneData,
		/// <summary>
		/// Ma UV scan index.
		/// </summary>
		UvScanIndex,
		/// <summary>
		/// Map UV peak data.
		/// </summary>
		UvPeakData,
		/// <summary>
		/// The end of map view accessor.
		/// </summary>
		EndOfMapViewAccessor
	}
}
