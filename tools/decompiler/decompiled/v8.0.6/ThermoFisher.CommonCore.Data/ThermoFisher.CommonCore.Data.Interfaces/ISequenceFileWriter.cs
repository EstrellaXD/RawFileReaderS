using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Provides methods to create and write samples to a sequence file.
/// </summary>
public interface ISequenceFileWriter : IDisposable
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
	/// Gets or sets additional information about a sequence
	/// </summary>
	ISequenceInfo Info { get; set; }

	/// <summary>
	/// Gets the set of samples in the sequence
	/// </summary>
	List<SampleInformation> Samples { get; }

	/// <summary>
	/// Gets the name of the sequence file.
	/// </summary>
	string FileName { get; }

	/// <summary>
	/// Gets or sets the sequence bracket type.
	/// This determines which groups of samples use the same calibration curve.
	/// </summary>
	BracketType Bracket { get; set; }

	/// <summary>
	/// Gets or sets a description of the auto-sampler tray
	/// </summary>
	string TrayConfiguration { get; set; }

	/// <summary>
	/// Sets the user label at given 0-based label index.
	/// </summary>
	/// <remarks>
	/// SampleInformation.MaxUserTextColumnCount determines the maximum number of user
	/// column labels.
	/// </remarks>
	/// <param name="index">Index of user label to be set</param>
	/// <param name="label">New string value for user label to be set</param>
	/// <returns>true if successful;  false otherwise</returns>
	bool SetUserColumnLabel(int index, string label);

	/// <summary>
	/// Retrieves the user label at given 0-based label index.
	/// </summary>
	/// <remarks>
	/// SampleInformation.MaxUserTextColumnCount determines the maximum number of user
	/// column labels.
	/// </remarks>
	/// <param name="index">Index of user label to be retrieved</param>
	/// <returns>String containing the user label at given index</returns>
	string GetUserColumnLabel(int index);

	/// <summary>
	/// Update the sequence file header with the file header values passed in.  
	/// Only updates object values in memory, does not write to disk.
	/// A default FileHeader is created with every new writer instance.
	/// </summary>
	/// <param name="fileHeader">
	/// The file header object with values to use.
	/// </param>
	void UpdateFileHeader(IFileHeaderUpdate fileHeader);

	/// <summary>
	/// Saves Sequence data to disk.
	/// </summary>
	/// <returns>True saved data to disk; false otherwise.</returns>
	bool Save();
}
