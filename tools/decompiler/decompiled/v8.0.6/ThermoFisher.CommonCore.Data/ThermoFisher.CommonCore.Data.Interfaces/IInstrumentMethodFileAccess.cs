using System.Collections.ObjectModel;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to read an instrument method (meth) file.
/// </summary>
public interface IInstrumentMethodFileAccess
{
	/// <summary>
	/// Gets the file header for the method
	/// </summary>
	IFileHeader FileHeader { get; }

	/// <summary>
	/// Gets the data for of all devices in this method.
	/// Keys are the registered device names.
	/// A method contains only the "registered device name"
	/// which may not be the same as the "device display name" (product name).
	/// Instrument methods do not contain device product names.
	/// </summary>
	ReadOnlyDictionary<string, IInstrumentMethodDataAccess> Devices { get; }

	/// <summary>
	/// Gets the file error state.
	/// </summary>
	IFileError FileError { get; }

	/// <summary>
	/// Gets a value indicating whether the last file operation caused a recorded error.
	/// If so, there may be additional information in FileError
	/// </summary>
	/// <value></value>
	bool IsError { get; }

	/// <summary>
	/// Gets a value indicating whether a file was successfully opened.
	/// Inspect "FileError" when false
	/// </summary>
	bool IsOpen { get; }
}
