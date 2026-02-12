using System.Collections.Generic;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Provides methods to create/update an instrument method file. <para />
/// Instrument method file contains one or more device methods. Each device creates 
/// create its own storage (here's called Device Method) for storing device specific information.
///  ----
/// Existing layout of an instrument method file.
/// Instrument Method File
/// ---
/// --- Device Methods
/// ---
///     SIIXcalibur     (IDeviceMethod)
///         Data            (Method stream - byte array)
///         Text            (string)
///     SimulationMS    (IDeviceMethod)
///         Data            (Method stream - byte array)
///         Text            (string)
///     TNG-Calcium    (IDeviceMethod)
///         Data            (Method stream - byte array)
///         Text            (string)
///         Header          (Method stream - byte array)
/// ----
/// Example of creating an instrument method file:
/// 1. Create a new instrument method file with an input file name (The writer will create a "Named" in-memory instrument method file.)
///     writer = InstrumentMethodWriterFactory.CreateInstrumentMethodWriter("NewInstrumentMethodFileName.meth");
/// --    
///     writer.UpdateFileHeaderDescription("A new instrument method file.");
/// --
/// --  Calls the "GetDevices" method to get an empty list of devices (a dictionary object).
/// --  Once you receive the list, you can start adding new device method.
///     devices = writer.GetDevices();
/// --
/// --  Create a device method object (IDeviceMethod)
///     newDeviceMethod = DeviceMethodFactory.CreateDeviceMethod();
/// --
/// --  Adds method streams - Text, Data, etc.
/// --  Call the "GetStreamBytes" method to get an empty list of streams (a dictionary object).
/// --  Once you receive the list, you can start adding new method stream.
/// --
///     newStreams = newDeviceMethod.GetStreamBytes();
///     newStreams.Add("Text", stream value in byte array);
///     newStreams["Data"] = stream value in byte array;
/// --
/// --  Here's a shortcut for adding a "Text" stream, use the "MethodText" property.
///     newDeviceMethod.MethodText = "Test string.";
/// --
/// --  adds the newly created device method to the list of devices.
///     devices.Add(name, newDeviceMethod);
/// --
/// --  persists the data to a file. The file name is given during the writer creation.
///     writer.Save();
/// </summary>
public interface IInstrumentMethodWriter
{
	/// <summary>
	/// Gets a value indicating whether this instrument method file has detected an error.
	/// </summary>
	bool HasError { get; }

	/// <summary>
	/// Gets the error message.
	/// </summary>
	string ErrorMessage { get; }

	/// <summary>
	/// Gets the instrument method file name.
	/// </summary>
	string FileName { get; }

	/// <summary>
	/// Gets the file header for the instrument method file
	/// </summary>
	IFileHeader FileHeader { get; }

	/// <summary>
	/// Updates the file header field - "Description".
	/// </summary>
	/// <param name="description">The description.</param>
	void UpdateFileHeaderDescription(string description);

	/// <summary>
	/// Update the instrument method file header with the file header values passed in.  
	/// Only updates object values in memory, does not write to disk.
	/// A default FileHeader is created with every new writer instance.
	/// Possible to only update creator and user values.
	/// </summary>
	/// <param name="fileHeader">
	/// The file header object with values to use.
	/// </param>
	void UpdateFileHeader(IFileHeader fileHeader);

	/// <summary>
	/// Get the list of device methods which are currently defined in this instrument method.<para />
	/// Returns an empty list, if this is a newly created instrument method.<para />
	/// ---
	/// In order to add/update device method, caller should first call this to get the list of devices.<para />
	/// Once you've the list, you can start adding a new device method or editing/removing an existing device method.
	/// </summary>
	/// <returns>The list of device methods.</returns>
	Dictionary<string, IDeviceMethod> GetDevices();

	/// <summary>
	/// Saves the instrument methods to the file.
	/// If this is an "Unnamed" instrument method writer, caller should use "SaveAs" method with the output 
	/// file name; otherwise ArgumentNullException will be thrown.
	/// </summary>
	/// <returns>True if save successfully; otherwise false.</returns>
	/// <exception cref="T:System.ArgumentNullException">name;@The name cannot be empty.</exception>
	bool Save();

	/// <summary>
	/// Save this instrument methods to a file.<para />
	/// It should overwrite the instrument methods file if the file exists; otherwise, a 
	/// new file should be created.
	/// </summary>
	/// <param name="fileName">File name of the instrument method.</param>
	/// <returns>True if save successfully; otherwise false.</returns>
	/// <exception cref="T:System.ArgumentNullException">name;@The name cannot be empty.</exception>
	bool SaveAs(string fileName);
}
