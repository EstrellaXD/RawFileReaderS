using System;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Top level properties for raw dta (not specific to any detector)
/// </summary>
public interface IRawDataProperties
{
	/// <summary>
	/// Gets the path to original data.
	/// A raw file may have been moved or translated to other formats.
	/// This property always returns the path (folder) where the file was created (acquired)
	/// </summary>
	string Path { get; }

	/// <summary>
	/// Gets the name of acquired file (excluding path).
	/// </summary>
	string FileName { get; }

	/// <summary>
	/// Gets the date when this data was created.
	/// </summary>
	DateTime CreationDate { get; }

	/// <summary>
	/// Gets the name of person creating data.
	/// </summary>
	string CreatorId { get; }

	/// <summary>
	/// Gets various details about the sample (such as comments).
	/// </summary>
	SampleInformation SampleInformation { get; }

	/// <summary>
	/// Gets a value indicating whether the last file operation caused an error.
	/// </summary>
	bool IsError { get; }

	/// <summary>
	/// Gets a value indicating whether the file is being acquired (not complete).
	/// </summary>
	bool InAcquisition { get; }

	/// <summary>
	/// Gets a value indicating whether the data file was successfully opened.
	/// </summary>
	bool IsOpen { get; }
}
