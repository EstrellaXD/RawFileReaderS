using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides methods to create/open existing instrument method file for editing, 
/// such as creating device method and saving method data.
///  ----
/// Existing layout of an instrument method file.
/// Instrument Method File
///     Audit Data
///     LCQ Header
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
/// </summary>
internal class InstrumentMethodWriter : IInstrumentMethodWriter
{
	private readonly IInstrumentMethodBuilder _instrumentMethodsBuilder;

	/// <summary>
	/// Gets the instrument method file name.
	/// </summary>
	public string FileName => _instrumentMethodsBuilder.Name;

	/// <summary>
	/// Gets a value indicating whether this file has detected an error.
	/// If this is false: Other error properties in this interface have no meaning.
	/// </summary>
	public bool HasError => _instrumentMethodsBuilder.HasError;

	/// <summary>
	/// Gets the error message.
	/// </summary>
	public string ErrorMessage => _instrumentMethodsBuilder.ErrorMessage;

	/// <summary>
	/// Gets the file header for the instrument method file
	/// </summary>
	public IFileHeader FileHeader => _instrumentMethodsBuilder.FileHeader;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.InstrumentMethodWriter" /> class.
	/// The instrument method writer should open a file if it exists; otherwise, a new method file should be created.
	/// </summary>
	/// <param name="instrumentMethodsBuilder">
	/// The instrument methods builder.<para />
	/// The factory creates an IInstrumentMethodBuilder object and passes it to 
	/// the writer constructor using dependency injection pattern, and the writer uses it to 
	/// open/create/add/update a file, without any knowledge of actual implementation of this 
	/// builder is, i.e. it can be a compound document builder or XML builder.
	/// </param>
	public InstrumentMethodWriter(IInstrumentMethodBuilder instrumentMethodsBuilder)
	{
		_instrumentMethodsBuilder = instrumentMethodsBuilder;
	}

	/// <summary>
	/// Updates the file header field - "Description".
	/// </summary>
	/// <param name="description">The description.</param>
	public void UpdateFileHeaderDescription(string description)
	{
		_instrumentMethodsBuilder.UpdateFileHeaderDescription(description);
	}

	/// <summary>
	/// Update the instrument method file header with the file header values passed in.  
	/// Only updates object values in memory, does not write to disk.
	/// </summary>
	/// <param name="fileHeader">The file header.</param>
	/// <exception cref="T:System.ArgumentNullException">File header cannot be null.</exception>
	public void UpdateFileHeader(IFileHeader fileHeader)
	{
		if (fileHeader == null)
		{
			throw new ArgumentNullException("fileHeader");
		}
		_instrumentMethodsBuilder.UpdateFileHeader(fileHeader);
	}

	/// <summary>
	/// Get the list of device methods which are currently defined in this instrument method.<para />
	/// Returns an empty list, if this is a newly created instrument method.<para />
	/// ---
	/// In order to add/update device method, caller should first call this to get the list of devices.<para />
	/// Once you've the list, you can start adding a new device method or editing/removing an existing device method.
	/// </summary>
	/// <returns> The list of device methods. </returns>
	public Dictionary<string, IDeviceMethod> GetDevices()
	{
		return _instrumentMethodsBuilder.GetDevices();
	}

	/// <summary>
	/// Saves the instrument methods to the file.
	/// If this is an "Unnamed" instrument method writer, caller should use "SaveAs" method with the output 
	/// file name; otherwise ArgumentNullException will be thrown.
	/// </summary>
	/// <returns>
	/// True if save successfully; otherwise false.
	/// </returns>
	/// <exception cref="T:System.ArgumentNullException">name;@The instrument methods file name cannot be empty.</exception>
	public bool Save()
	{
		return _instrumentMethodsBuilder.SaveAs(_instrumentMethodsBuilder.Name);
	}

	/// <summary>
	/// Save this instrument methods to a file.<para />
	/// It should overwrite the instrument methods file if the file exists; otherwise, a 
	/// new file should be created.
	/// </summary>
	/// <param name="fileName">File name of the instrument method.</param>
	/// <returns>True if save successfully; otherwise false.</returns>
	/// <exception cref="T:System.ArgumentNullException">name;@The instrument methods file name cannot be empty.</exception>
	public bool SaveAs(string fileName)
	{
		return _instrumentMethodsBuilder.SaveAs(fileName);
	}
}
