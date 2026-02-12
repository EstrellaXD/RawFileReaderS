using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// Provides methods to write an instrument method file.<para />
/// </summary>
internal interface IInstrumentMethodBuilder
{
	/// <summary>
	/// Gets a value indicating whether this file has detected an error.
	/// If this is false: Other error properties in this interface have no meaning.
	/// </summary>
	bool HasError { get; }

	/// <summary>
	/// Gets the error message.
	/// </summary>
	string ErrorMessage { get; }

	/// <summary>
	/// Gets the instrument method file name.
	/// </summary>
	string Name { get; }

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
	/// </summary>
	/// <param name="fileHeader">The file header.</param>
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
	/// Save this instrument methods to a file.<para />
	/// It should overwrite the instrument methods file if the file exists; otherwise, a 
	/// new file should be created.
	/// </summary>
	/// <param name="fileName">File name of the instrument method.</param>
	/// <returns>True if save successfully; otherwise false.</returns>
	/// <exception cref="T:System.ArgumentNullException">name;@The name cannot be empty.</exception>
	bool SaveAs(string fileName);
}
