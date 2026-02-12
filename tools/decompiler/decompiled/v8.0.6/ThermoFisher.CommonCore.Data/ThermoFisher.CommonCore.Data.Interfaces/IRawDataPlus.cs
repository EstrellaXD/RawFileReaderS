using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The RawDataPlus interface. Provides access to raw data.
/// This extends IRawData, adding new features which were introduced at "Platform 3.0"
/// plus access to some additional content from earlier raw file versions.
/// This is a disposable interface, as an implementation may hold an active file,
/// database or other protocol connection.
/// Many interfaces returned from implementations of this interface may rely
/// on an active file, database or protocol connection.
/// Do not dispose of this object until interfaces (especially enumerators)
/// returned from it are no longer in use.
/// All value type returns from this interface are safe against the underlying file being closed.
/// </summary>
public interface IRawDataPlus : IRawData, IDetectorReaderBase, IRawDataProperties, IDisposable, IRawCache, ISimplifiedScanReader, IDetectorReaderPlus
{
	/// <summary>
	/// Gets the raw file header.
	/// </summary>
	IFileHeader FileHeader { get; }

	/// <summary>
	/// Gets the file error state.
	/// </summary>
	IFileError FileError { get; }

	/// <summary>
	/// Gets the auto sampler (tray) information.
	/// </summary>
	IAutoSamplerInformation AutoSamplerInformation { get; }

	/// <summary>
	/// Gets a value indicating whether this file has an instrument method.
	/// </summary>
	bool HasInstrumentMethod { get; }

	/// <summary>
	/// Gets a value indicating whether this file has MS data.
	/// </summary>
	bool HasMsData { get; }

	/// <summary>
	/// Gets the name of the computer, used to create this file.
	/// </summary>
	string ComputerName { get; }

	/// <summary>
	/// Get all instrument friendly names from the instrument method.
	/// These are the "display names" for the instruments.
	/// </summary>
	/// <returns>
	/// The instrument friendly names.
	/// </returns>
	string[] GetAllInstrumentFriendlyNamesFromInstrumentMethod();

	/// <summary>
	/// For Xcalibur Data System Only:
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
	/// The method file path.
	/// </param>
	/// <param name="forceOverwrite">
	/// Force over write. If true, and file already exists, attempt to delete existing file first.
	/// If false: UnauthorizedAccessException will occur if there is an existing read only file.
	/// </param>
	/// <returns>True if the file was saved. False, if no file was saved, for example,
	/// because there is no instrument method saved in this raw file.</returns>
	/// <seealso cref="P:ThermoFisher.CommonCore.Data.Interfaces.IRawDataPlus.HasInstrumentMethod" />
	bool ExportInstrumentMethod(string methodFilePath, bool forceOverwrite);
}
