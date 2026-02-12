using System.Collections.ObjectModel;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to read a processing method (PMD) file.
/// </summary>
public interface IProcessingMethodFileAccess
{
	/// <summary>
	/// Gets the file header for the processing method
	/// </summary>
	IFileHeader FileHeader { get; }

	/// <summary>
	/// Gets the file error state.
	/// </summary>
	IFileError FileError { get; }

	/// <summary>
	/// Gets a value indicating whether the last file operation caused an error
	/// </summary>
	/// <value></value>
	bool IsError { get; }

	/// <summary>
	/// Gets a value indicating whether a file was successfully opened.
	/// Inspect "FileError" when false
	/// </summary>
	bool IsOpen { get; }

	/// <summary>
	/// Gets some global settings from a PMD
	/// These settings apply to all components in the quantitation section.
	/// Some settings affect qualitative processing.
	/// </summary>
	IProcessingMethodOptionsAccess MethodOptions { get; }

	/// <summary>
	/// Gets the "Standard report" settings from a processing method
	/// </summary>
	IProcessingMethodStandardReportAccess StandardReport { get; }

	/// <summary>
	/// Gets peak detection settings (Qualitative processing)
	/// </summary>
	IQualitativePeakDetectionAccess PeakDetection { get; }

	/// <summary>
	/// Gets Spectrum Enhancement settings (Qualitative processing)
	/// </summary>
	ISpectrumEnhancementAccess SpectrumEnhancement { get; }

	/// <summary>
	/// Gets options for NIST library search
	/// </summary>
	ILibrarySearchOptionsAccess LibrarySearch { get; }

	/// <summary>
	/// Gets constraints for NIST library search
	/// </summary>
	ILibrarySearchConstraintsAccess LibrarySearchConstraints { get; }

	/// <summary>
	/// Gets the list of reports
	/// </summary>
	ReadOnlyCollection<IXcaliburSampleReportAccess> SampleReports { get; }

	/// <summary>
	/// Gets the list of programs
	/// </summary>
	ReadOnlyCollection<IXcaliburProgramAccess> Programs { get; }

	/// <summary>
	/// Gets the list of reports
	/// </summary>
	ReadOnlyCollection<IXcaliburReportAccess> SummaryReports { get; }

	/// <summary>
	/// Gets additional options about the peak display (peak labels etc).
	/// </summary>
	IPeakDisplayOptions PeakDisplayOptions { get; }

	/// <summary>
	/// Gets setting for PDA peak purity
	/// </summary>
	IPeakPuritySettingsAccess PeakPuritySettings { get; }

	/// <summary>
	/// Gets the list of compounds.
	/// This includes all integration, calibration and other settings 
	/// which are specific to each component.
	/// </summary>
	ReadOnlyCollection<IXcaliburComponentAccess> Components { get; }

	/// <summary>
	/// Gets the raw file name, which was used to design this method
	/// </summary>
	string RawFileName { get; }

	/// <summary>
	/// Gets or sets the (global) mass tolerance and precision settings for the method.
	/// When reading a file "get" will return the values saved in a PMD
	/// Set can be used to override values, such that when a filter
	/// is presented as text, alternative (detector) mass precision is used.
	/// </summary>
	IMassOptionsAccess MassOptions { get; set; }

	/// <summary>
	/// Gets the "View type" saved in a PMD file
	/// </summary>
	ProcessingMethodViewType ViewType { get; }
}
