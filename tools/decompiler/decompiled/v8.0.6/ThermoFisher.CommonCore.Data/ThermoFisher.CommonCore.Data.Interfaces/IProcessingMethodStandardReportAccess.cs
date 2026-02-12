namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// This set of flags gives a table of possible "standard" or built in reports.
/// Available choices in the UI, and actual report content depend on the application.
/// Some where probably never offered, or only applied to legacy LCQ data system.
/// </summary>
public interface IProcessingMethodStandardReportAccess
{
	/// <summary>
	/// Gets a value indicating whether the Analysis Unknown report is needed
	/// </summary>
	bool AnalysisUnknown { get; }

	/// <summary>
	/// Gets a value indicating whether the Component Unknown report is needed
	/// </summary>
	bool ComponentUnknown { get; }

	/// <summary>
	/// Gets a value indicating whether the Method Unknown report is needed
	/// </summary>
	bool MethodUnknown { get; }

	/// <summary>
	/// Gets a value indicating whether the Log Unknown report is needed
	/// </summary>
	bool LogUnknown { get; }

	/// <summary>
	/// Gets a value indicating whether the Analysis Calibration report is needed
	/// </summary>
	bool AnalysisCalibration { get; }

	/// <summary>
	/// Gets a value indicating whether the Component Calibration report is needed
	/// </summary>
	bool ComponentCalibration { get; }

	/// <summary>
	/// Gets a value indicating whether the Method Calibration report is needed
	/// </summary>
	bool MethodCalibration { get; }

	/// <summary>
	/// Gets a value indicating whether the Log Calibration report is needed
	/// </summary>
	bool LogCalibration { get; }

	/// <summary>
	/// Gets a value indicating whether the Analysis QC report is needed
	/// </summary>
	bool AnalysisQc { get; }

	/// <summary>
	/// Gets a value indicating whether the Component QC report is needed
	/// </summary>
	bool ComponentQc { get; }

	/// <summary>
	/// Gets a value indicating whether the Method QC report is needed
	/// </summary>
	bool MethodQc { get; }

	/// <summary>
	/// Gets a value indicating whether the Log QC report is needed
	/// </summary>
	bool LogQc { get; }

	/// <summary>
	/// Gets a value indicating whether the Analysis Other report is needed
	/// </summary>
	bool AnalysisOther { get; }

	/// <summary>
	/// Gets a value indicating whether the Component Other report is needed
	/// </summary>
	bool ComponentOther { get; }

	/// <summary>
	/// Gets a value indicating whether the Method Other report is needed
	/// </summary>
	bool MethodOther { get; }

	/// <summary>
	/// Gets a value indicating whether the Log Other report is needed
	/// </summary>
	bool LogOther { get; }

	/// <summary>
	/// Gets a value indicating whether the Sample Information report is needed
	/// </summary>
	bool SampleInformation { get; }

	/// <summary>
	/// Gets a value indicating whether the Run Information report is needed
	/// </summary>
	bool RunInformation { get; }

	/// <summary>
	/// Gets a value indicating whether the Chromatogram report is needed
	/// </summary>
	bool Chromatogram { get; }

	/// <summary>
	/// Gets a value indicating whether the PeakComponent report is needed
	/// </summary>
	bool PeakComponent { get; }

	/// <summary>
	/// Gets a value indicating whether the Tune report is needed
	/// </summary>
	bool Tune { get; }

	/// <summary>
	/// Gets a value indicating whether the Experiment report is needed
	/// </summary>
	bool Experiment { get; }

	/// <summary>
	/// Gets a value indicating whether the Processing report is needed
	/// </summary>
	bool Processing { get; }

	/// <summary>
	/// Gets a value indicating whether the Status report is needed
	/// </summary>
	bool Status { get; }

	/// <summary>
	/// Gets a value indicating whether the Error report is needed
	/// </summary>
	bool Error { get; }

	/// <summary>
	/// Gets a value indicating whether the Audit report is needed
	/// </summary>
	bool Audit { get; }

	/// <summary>
	/// Gets a value indicating whether the Open Access report is needed
	/// </summary>
	bool OpenAccess { get; }

	/// <summary>
	/// Gets a value indicating which of the two types of chromatogram analysis report is needed.
	/// </summary>
	ChroAnalysisReport ChroAnalysisReport { get; }

	/// <summary>
	/// Gets a value indicating whether the Survey report is needed
	/// </summary>
	bool Survey { get; }

	/// <summary>
	/// Gets a value indicating whether to include a signature line in reports
	/// </summary>
	bool PrintSignatureLine { get; }
}
