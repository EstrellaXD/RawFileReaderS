using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Loads a sequence (SLD) file.
/// </summary>
internal class SequenceFileAccess : ISequenceFileAccess
{
	private readonly SequenceFileLoader _fileLoader;

	/// <summary>
	/// Gets the file header for the sequence
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
	/// Gets a value indicating whether true if a file was successfully opened.
	/// Inspect "FileError" when false
	/// </summary>
	public bool IsOpen => _fileLoader.IsOpen;

	/// <summary>
	/// Gets additional information about a sequence
	/// </summary>
	public ISequenceInfo Info => _fileLoader.SequenceInfo;

	/// <summary>
	/// Gets the set of samples in the sequence
	/// </summary>
	public List<SampleInformation> Samples => _fileLoader.Samples;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.SequenceFileAccess" /> class.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	internal SequenceFileAccess(string fileName)
	{
		if (!fileName.ToUpperInvariant().EndsWith(".SLD"))
		{
			fileName += ".sld";
		}
		_fileLoader = new SequenceFileLoader(fileName);
	}
}
