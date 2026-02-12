using System.Collections.Generic;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Provides functions to create/update a device method. 
/// The device methods are for each configured instrument and stored in instrument method file. 
/// Each device method contains two or more streams.  
///     One calls "Data", stores a private representation of the method, in a binary or XML format.
///     Second calls "Text, stores an Unicode text description of the method.
///     Other streams may also be created, that are private to the device, i.e. TNG-Calcium has "Header" stream.
/// Format:
///     TNG-Calcium    (IDeviceMethod)
///         Data            (Method stream - byte array)
///         Text            (string)
///         Header          (Method stream - byte array)
/// </summary>
public interface IDeviceMethod : IFileError
{
	/// <summary>
	/// Gets or sets the "Text" plain text (unicode) form of an device method.
	/// This property provides a shortcut to get/set the "Text" stream. <para />
	/// Accessing the property, returns empty string if the "Text" stream does not exist; otherwise, it will retrieve the stream
	/// from the StreamBytes list.<para />
	/// Updating the property, overwrite the existing content of the "Text stream if it already exists; otherwise, it
	/// will add the "Text" stream to the StreamBytes list.
	/// </summary>
	string MethodText { get; set; }

	/// <summary>
	/// Gets all stream data names for this device storage.
	/// Typically an instrument has a stream called "Data" containing the method in binary or XML 
	/// and "Text" contains the plain text form of the method.
	/// Other streams (private to the instrument) may also be created.
	/// ---
	/// In order to add/update the stream, caller should first call this to get the list of streams.<para />
	/// Once you've the list, you can start adding a new stream or editing/removing an existing method stream.
	/// ---
	/// If the stream is set to null, during the save operation, it'll save as zero length stream.
	/// </summary>
	/// <returns>Device stream names</returns>
	Dictionary<string, byte[]> GetStreamBytes();
}
