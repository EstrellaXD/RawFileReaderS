using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interfaces to read an instrument method from a raw file and to export it
/// to a file.
/// </summary>
public interface IInstrumentMethodExporter : IDisposable
{
	/// <summary>
	/// Gets a value indicating whether the underlying raw file has instrument method.
	/// </summary>
	bool HasInstrumentMethod { get; }

	/// <summary>
	/// Gets the errors.
	/// </summary>
	string ErrorMessage { get; }

	/// <summary>
	/// Gets a value indicating whether any error occurred at open/read the raw file.
	/// </summary>
	bool HasError { get; }

	/// <summary>
	/// Export the instrument method to a file.
	/// Because of the many potential issues with this, use with care, especially if
	/// adding to a customer workflow.
	/// Try catch should be used with this method.
	/// Not all implementations may support this (some may throw NotImplementedException).
	/// .Net exceptions may be thrown, for example if the path is not valid.
	/// Not all instrument methods can be exported, depending on raw file version, and how
	/// the file was acquired. If the "instrument method file name" is not present in the sample information,
	/// then the exported data may not be a complete method file.
	/// Not all exported files can be read by an instrument method editor.
	/// Instrument method editors may only be able to open methods when the exact same list
	/// of instruments is configured.
	/// Code using this feature should handle all cases.
	/// </summary>
	/// <param name="methodFilePath">
	/// The output instrument method file path.
	/// </param>
	/// <param name="forceOverwrite">
	/// Force over write. If true, and file already exists, attempt to delete existing file first.
	/// If false: UnauthorizedAccessException will occur if there is an existing read only file.
	/// </param>
	/// <returns>True if the file was saved. False, if no file was saved, for example,
	/// because there is no instrument method saved in this raw file.</returns>
	bool ExportInstrumentMethod(string methodFilePath, bool forceOverwrite);

	/// <summary>
	/// Gets names of all instruments, which have a method stored in the raw file's copy of the instrument method file.
	/// These names are "Device internal names" which map to storage names within
	/// an instrument method, and other instrument data (such as registry keys).
	/// Use "GetAllInstrumentFriendlyNamesFromInstrumentMethod" to get display names for instruments.
	/// </summary>
	/// <returns>
	/// The instrument names.
	/// </returns>
	string[] GetAllInstrumentNamesFromInstrumentMethod();
}
