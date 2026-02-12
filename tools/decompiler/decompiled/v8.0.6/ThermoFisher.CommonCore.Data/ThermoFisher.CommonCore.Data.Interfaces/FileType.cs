using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The type of the file.
/// </summary>
[Flags]
public enum FileType
{
	/// <summary>
	/// The unknown file type
	/// </summary>
	NotSupported = 0,
	/// <summary>
	/// The experiment method
	/// </summary>
	ExperimentMethod = 1,
	/// <summary>
	/// The sample list (sequence)
	/// </summary>
	SampleList = 2,
	/// <summary>
	/// The processing method
	/// </summary>
	ProcessingMethod = 4,
	/// <summary>
	/// The raw file
	/// </summary>
	RawFile = 8,
	/// <summary>
	/// The tune method
	/// </summary>
	TuneMethod = 0x10,
	/// <summary>
	/// The results file
	/// </summary>
	ResultsFile = 0x20,
	/// <summary>
	/// The Quan file
	/// </summary>
	QuanFile = 0x40,
	/// <summary>
	/// The calibration file
	/// </summary>
	CalibrationFile = 0x80,
	/// <summary>
	/// The instrument method file
	/// </summary>
	MethodFile = 0x100,
	/// <summary>
	/// The XQN file
	/// </summary>
	XqnFile = 0x200,
	/// <summary>
	/// The layout file (may be combined with other file type)
	/// </summary>
	LayoutFile = 0x1000,
	/// <summary>
	/// The method editor layout
	/// </summary>
	MethodEditorLayout = 0x1040,
	/// <summary>
	/// The sample list editor layout
	/// </summary>
	SampleListEditorLayout = 0x1080,
	/// <summary>
	/// The processing method edit layout
	/// </summary>
	ProcessingMethodEditLayout = 0x1100,
	/// <summary>
	/// The Qual Browser layout
	/// </summary>
	QualBrowserLayout = 0x1200,
	/// <summary>
	/// The tune layout
	/// </summary>
	TuneLayout = 0x1400,
	/// <summary>
	/// The results layout
	/// </summary>
	ResultsLayout = 0x1800
}
