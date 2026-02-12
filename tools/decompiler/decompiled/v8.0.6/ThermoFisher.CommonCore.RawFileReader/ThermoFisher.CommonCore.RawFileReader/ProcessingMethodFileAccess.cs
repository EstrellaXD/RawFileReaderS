using System.Collections.ObjectModel;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Loads a processing method (PMD) file.
/// </summary>
internal class ProcessingMethodFileAccess : IProcessingMethodFileAccess
{
	private readonly ProcessingMethodFileLoader _fileLoader;

	/// <summary>
	/// Gets the file header for the processing method
	/// </summary>
	public IFileHeader FileHeader => _fileLoader.Header;

	/// <summary>
	/// Gets a value indicating whether the last file operation caused an error
	/// </summary>
	/// <value></value>
	public bool IsError => _fileLoader.HasError;

	/// <summary>
	/// Gets the file error state.
	/// </summary>
	public IFileError FileError => _fileLoader;

	/// <summary>
	/// Gets a value indicating whether a file was successfully opened.
	/// Inspect "FileError" when false
	/// </summary>
	public bool IsOpen => _fileLoader.IsOpen;

	/// <summary>
	/// Gets some global settings from a PMD
	/// These settings apply to all components in the quantitation section.
	/// Some settings affect qualitative processing.
	/// </summary>
	public IProcessingMethodOptionsAccess MethodOptions => _fileLoader.ProcessingMethodOptions;

	/// <summary>
	/// Gets the "Standard report" settings from a processing method
	/// </summary>
	public IProcessingMethodStandardReportAccess StandardReport => _fileLoader.StandardReport;

	/// <summary>
	/// Gets peak detection settings (Qualitative processing)
	/// </summary>
	public IQualitativePeakDetectionAccess PeakDetection => _fileLoader.PeakDetection;

	/// <summary>
	/// Gets Spectrum Enhancement settings (Qualitative processing)
	/// </summary>
	public ISpectrumEnhancementAccess SpectrumEnhancement => _fileLoader.SpecEnhancementOptions;

	/// <summary>
	/// Gets options for NIST library search
	/// </summary>
	public ILibrarySearchOptionsAccess LibrarySearch => _fileLoader.LibrarySearch;

	/// <summary>
	/// Gets constraints for NIST library search
	/// </summary>
	public ILibrarySearchConstraintsAccess LibrarySearchConstraints => _fileLoader.LibraryConstraints;

	/// <summary>
	/// Gets the list of reports
	/// </summary>
	public ReadOnlyCollection<IXcaliburSampleReportAccess> SampleReports => _fileLoader.SampleReports;

	/// <summary>
	/// Gets the list of programs
	/// </summary>
	public ReadOnlyCollection<IXcaliburProgramAccess> Programs => _fileLoader.Programs;

	/// <summary>
	/// Gets the list of reports
	/// </summary>
	public ReadOnlyCollection<IXcaliburReportAccess> SummaryReports => _fileLoader.SummaryReports;

	/// <summary>
	/// Gets additional options about the peak display (peak labels etc).
	/// </summary>
	public IPeakDisplayOptions PeakDisplayOptions => _fileLoader.PeakDisplayOptions;

	/// <summary>
	/// Gets setting for PDA peak purity
	/// </summary>
	public IPeakPuritySettingsAccess PeakPuritySettings => _fileLoader.PeakPurity;

	/// <summary>
	/// Gets the list of compounds.
	/// This includes all integration, calibration and other settings 
	/// which are specific to each component.
	/// </summary>
	public ReadOnlyCollection<IXcaliburComponentAccess> Components => _fileLoader.Components;

	/// <summary>
	/// Gets the raw file name, which was used to design this method
	/// </summary>
	public string RawFileName => _fileLoader.RawFileName;

	/// <summary>
	/// Gets the "View type" saved in a PMD file
	/// </summary>
	public ProcessingMethodViewType ViewType => _fileLoader.ViewType;

	/// <summary>
	/// Gets or sets the (global) mass tolerance and precision settings for the method
	/// </summary>
	public IMassOptionsAccess MassOptions
	{
		get
		{
			return _fileLoader.MassOptions;
		}
		set
		{
			_fileLoader.MassOptions = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.ProcessingMethodFileAccess" /> class.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	internal ProcessingMethodFileAccess(string fileName)
	{
		if (!fileName.ToUpperInvariant().EndsWith(".PMD"))
		{
			fileName += ".pmd";
		}
		_fileLoader = new ProcessingMethodFileLoader(fileName);
	}
}
