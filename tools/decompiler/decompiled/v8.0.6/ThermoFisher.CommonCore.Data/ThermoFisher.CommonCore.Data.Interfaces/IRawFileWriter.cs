using System;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The Raw File Writer interface.
/// Used for writing raw files to disk.
/// Upon creating an instance of IRawFileWriter, the raw file name is provided and checked for existence.  
/// If that file name already exists, the new instance creates a raw file name appended with a unique date time stamp.
/// Many methods return true on success, and false on fail.
/// The IFileError interface can be used to get error information.
/// HasError is set on that interface for a non recoverable error, and the ErrorMessage is set
/// to one or more lines of error data.
/// If there is an error, the WarningMessage contains a multiple line diagnostic log.
/// This may contain information and warning lines.
/// HasWarning is set if at least one line of the log contains a warning.
/// </summary>
public interface IRawFileWriter : IDisposable, IFileError
{
	/// <summary>
	/// Gets a value indicating whether an error occurred during writing.
	/// </summary>
	bool IsError { get; }

	/// <summary>
	/// Gets or sets a value indicating whether the writer should save changes.  True by default.
	/// If false when a save is called, data will not be written to disk and any data written previously will remain unchanged.
	/// The writer will still need to be disposed regardless of ShouldSaveChanges status.
	/// </summary>
	bool ShouldSaveChanges { get; set; }

	/// <summary>
	/// Gets the raw file name being written.
	/// </summary>
	string RawFileName { get; }

	/// <summary>
	/// Gets the calculated checksum for this raw file. Only calculated after a save has occurred.
	/// </summary>
	long CheckSum { get; }

	/// <summary>
	/// Refresh view of the raw file.  Reloads the RawFileInfo object and the virtual controllers run headers from shared memory.  
	/// Does not read actual data from any temporary files.
	/// </summary>
	/// <returns>
	/// True if successfully reloaded internal memory buffers.
	/// </returns>
	bool RefreshViewOfFile();

	/// <summary>
	/// Gets the set of devices currently connected to the raw file.
	/// This data is only updated after calling "RefreshViewOfFile".
	/// Devices may be added by external processes (especially during data acquisition).
	/// </summary>
	/// <returns>An array of device types. The length of the array is equal to the number of detectors
	/// which have registered with the raw file.
	/// Some device may have incomplete registration at the time of the call.
	/// In this case "Device.None" is recorded</returns>
	Device[] GetConnectedDevices();

	/// <summary>
	/// Update the raw file header with the file header values passed in.  
	/// Only updates object values in memory, does not write to disk.
	/// A default FileHeader is created with every new writer instance.
	/// </summary>
	/// <param name="fileHeader">
	/// The file header object with values to use.
	/// </param>
	void UpdateFileHeader(IFileHeaderUpdate fileHeader);

	/// <summary>
	/// Update the raw file auto sampler information with the auto sampler information values passed in.
	/// Only updates object values in memory, does not write to disk.
	/// </summary>
	/// <param name="autoSamplerInformation">
	/// The auto sampler information.
	/// </param>
	/// <returns>
	/// True if successfully updated auto sampler information values.
	/// If false: Check errors.
	/// </returns>
	bool UpdateAutoSampler(IAutoSamplerInformation autoSamplerInformation);

	/// <summary>
	/// Update the raw file sequence row with the sample information values passed in.
	/// Only updates object values in memory, does not write to disk.
	/// </summary>
	/// <param name="sequenceRowSampleInfo">
	/// The sequence Row Sample Information.
	/// </param>
	/// <returns>
	/// True if successfully updated raw file values.
	/// If false: Check errors (IFileError).
	/// </returns>
	bool UpdateSequenceRow(ISampleInformation sequenceRowSampleInfo);

	/// <summary>
	/// Update creator logon value for the raw file being written.
	/// </summary>
	/// <param name="creatorLogon">
	/// The creator logon.
	/// </param>
	/// <returns>
	/// True if successfully updated value.
	/// If false: Check errors (IFileError).
	/// </returns>
	bool UpdateCreatorLogon(string creatorLogon);

	/// <summary>
	/// Update creator id value for the raw file being written.
	/// </summary>
	/// <param name="creatorId">
	/// The creator id.
	/// </param>
	/// <returns>
	/// True if successfully updated value.
	/// If false: Check errors (IFileError).
	/// </returns>
	bool UpdateCreatorId(string creatorId);

	/// <summary>
	/// Update all user labels for the raw file being written.
	/// Max 5 labels. If array parameter is &gt; 5 then all values are ignored.
	/// </summary>
	/// <param name="userLabels">
	/// The user label.
	/// </param>
	/// <returns>
	/// True if successfully updated value.
	/// If false: Check errors (IFileError).
	/// </returns>
	bool UpdateUserLabels(string[] userLabels);

	/// <summary>
	/// Update specific user label value at index for the raw file being written.
	/// </summary>
	/// <param name="userLabel">
	/// The user label.
	/// </param>
	/// <param name="index">
	/// The index into labels array to update.
	/// </param>
	/// <returns>
	/// True if successfully updated value.
	/// If false: Check errors (IFileError).
	/// </returns>
	bool UpdateUserLabel(string userLabel, int index);

	/// <summary>
	/// Update creation date for the raw file being written.
	/// </summary>
	/// <param name="creationDate">
	/// The creation date.
	/// </param>
	/// <returns>
	/// True if successfully updated value.
	/// If false: Check errors (IFileError).
	/// </returns>
	bool UpdateCreationDate(DateTime creationDate);

	/// <summary>
	/// Adds an audit data entry to the audit trail list of the raw file.
	/// </summary>
	/// <param name="timeChanged">
	/// The time changed.
	/// </param>
	/// <param name="whatChanged">
	/// What changed by integer value.
	/// </param>
	/// <param name="comment">
	/// The comment.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// True if successfully updated value.
	/// If false: Check errors (IFileError).
	/// </returns>
	bool AddAuditEntry(DateTime timeChanged, long whatChanged, string comment);

	/// <summary>
	/// Saves the instrument method into the raw file
	/// </summary>
	/// <param name="instrumentMethodPath">The instrument method file path.</param>
	/// <param name="storageNames">List of virtual instrument storage names.</param>
	/// <param name="descriptions">List of virtual instrument descriptions.</param>
	/// <param name="shouldBeDeleted">
	/// [Optional] it has a default value of true.<para />
	/// The should be deleted flag indicating whether the temporary instrument method should be removed after the raw file writer is closed.<para />
	/// </param>
	/// <returns>True if instrument method successfully saved to raw file.
	/// If false: Check errors (IFileError).</returns>         
	bool StoreInstrumentMethod(string instrumentMethodPath, string[] storageNames, string[] descriptions, bool shouldBeDeleted = true);

	/// <summary>
	/// Saves raw file to disk with current values.  Should be called after all updates are finished.  
	/// </summary>
	/// <param name="partialSave">
	/// True only if saving part of the raw file for the acquisition raw file.  
	/// On partial save, only raw file header, auto sampler information, sequence row information, and raw file info are written.  No scan data written.
	/// </param>
	/// <returns>
	/// True if file successfully written to disk.
	/// If false: Check errors (IFileError).
	/// </returns>
	bool SaveRawFile(bool partialSave);
}
