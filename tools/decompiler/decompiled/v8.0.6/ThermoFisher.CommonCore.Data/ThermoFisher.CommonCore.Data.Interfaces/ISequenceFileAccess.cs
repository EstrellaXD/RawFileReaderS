using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to read a sequence (SLD) file.
/// </summary>
public interface ISequenceFileAccess
{
	/// <summary>
	/// Gets the file header for the sequence
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
	/// Gets a value indicating whether true if a file was successfully opened.
	/// Inspect "FileError" when false
	/// </summary>
	bool IsOpen { get; }

	/// <summary>
	/// Gets additional information about a sequence
	/// </summary>
	ISequenceInfo Info { get; }

	/// <summary>
	/// Gets the set of samples in the sequence
	/// </summary>
	List<SampleInformation> Samples { get; }
}
