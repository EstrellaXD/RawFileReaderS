using System;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

namespace ThermoFisher.CommonCore.RawFileReader.Facade;

/// <summary>
/// Defines an interface to access data from a raw file.
/// Implementations may obtain data using different file access methods.
/// </summary>
internal interface IRawFileLoader : IFileError, IDisposable
{
	/// <summary>
	/// Gets a toll for managing views into the file
	/// </summary>
	IViewCollectionManager Manager { get; }

	/// <summary>
	/// Gets the identifier.
	/// </summary>
	/// <value>
	/// The identifier.
	/// </value>
	Guid Id { get; }

	/// <summary>
	/// Gets the raw file path.
	/// </summary>
	/// <value>
	/// The raw file path.
	/// </value>
	string RawFileName { get; }

	/// <summary>
	/// Gets the auto sampler config.
	/// </summary>
	/// <value>
	/// The automatic sampler configuration.
	/// </value>
	IAutoSamplerConfig AutoSamplerConfig { get; }

	/// <summary>
	/// Gets the (instrument) method info.
	/// </summary>
	/// <value>
	/// The method information.
	/// </value>
	IMethod MethodInfo { get; }

	/// <summary>
	/// Gets the sequence.
	/// </summary>
	/// <value>
	/// The sequence.
	/// </value>
	ISequenceRow Sequence { get; set; }

	/// <summary>
	/// Gets the raw file information.
	/// </summary>
	/// <value>
	/// The raw file information.
	/// </value>
	IRawFileInfo RawFileInformation { get; set; }

	/// <summary>
	/// Gets the file header
	/// </summary>
	FileHeader Header { get; }

	bool IsOpen { get; }

	/// <summary>
	/// Gets the audit trail information (legacy LCQ feature)
	/// </summary>
	AuditTrail AuditTrailInfo { get; }

	/// <summary>
	/// Gets the devices.
	/// </summary>
	/// <value>
	/// The devices.
	/// </value>
	DeviceContainer[] Devices { get; }

	string DataFileMapName { get; }

	/// <summary>
	/// Add a user of this loader
	/// </summary>
	/// <returns>the number of active users</returns>
	int AddUse();

	/// <summary>
	/// export the instrument method.
	/// </summary>
	/// <param name="methodFilePath">
	/// The method file path.
	/// </param>
	/// <param name="forceOverwrite">
	/// if set: force overwrite of existing files.
	/// </param>
	void ExportInstrumentMethod(string methodFilePath, bool forceOverwrite);

	/// <summary>
	/// Remove a user of this loader
	/// </summary>
	/// <returns>the number of active users</returns>
	int RemoveUse();

	/// <summary>
	/// Re-read the current file, to get the latest data.
	/// Only meaningful if the object has an implied backing file (such as IO.DLL and .raw files)
	/// No-op otherwise
	/// </summary>
	/// <returns>
	/// True on success&gt;.
	/// </returns>
	bool RefreshViewOfFile();

	/// <summary>Append an error message.</summary>
	/// <param name="ex">The error exception. </param>
	/// <exception cref="T:System.ArgumentException">The zero value is intended for no error and should not be used for clearing error here.</exception>
	/// <returns>Always false.</returns>
	bool AppendError(Exception ex);

	void RefreshDevices(int numberOfVirtualControllers);
}
