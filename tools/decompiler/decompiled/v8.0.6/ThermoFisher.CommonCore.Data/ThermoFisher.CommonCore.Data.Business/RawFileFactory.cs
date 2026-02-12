using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Class to read raw files
/// New code should not use this class directly.
/// Use only the static class: "RawFileReaderFactory" 
/// Catch exceptions from the construction, which will be thrown if
/// the DLLs are missing.
/// Call the "OpenFile" method to open a raw file.
/// The returned interface from OpenFile is "IRawDataPlus".
/// The IRawDataPlus interface implements the IRawData interface,
/// so the returned object can also be passed to code expecting IRawData.
/// </summary>
public class RawFileFactory
{
	private ObjectFactory<IRawDataPlus> _rawFileFactory;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.RawFileFactory" /> class. 
	/// </summary>
	public RawFileFactory()
	{
		Initialize();
	}

	/// <summary>
	/// Initialize: Load the required DLLs.
	/// </summary>
	private void Initialize()
	{
		_rawFileFactory = new ObjectFactory<IRawDataPlus>(".raw", "ThermoFisher.CommonCore.RawFileReader.RawFileReaderAdapter", "ThermoFisher.CommonCore.RawFileReader.dll");
		_rawFileFactory.Initialize();
	}

	/// <summary>
	/// Open the requested raw file
	/// </summary>
	/// <param name="rawFileName">Name of file to open</param>
	/// <returns>Interface to read the raw data</returns>
	public IRawDataPlus OpenFile(string rawFileName)
	{
		if (_rawFileFactory != null)
		{
			try
			{
				return _rawFileFactory.OpenFile(rawFileName);
			}
			catch (Exception innerException)
			{
				throw new ApplicationException("Unable to open any files", innerException);
			}
		}
		throw new ApplicationException("Unable to open any files");
	}
}
