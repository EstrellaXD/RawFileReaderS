using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Class to read data from a meth file
/// </summary>
public static class InstrumentMethodFileReader
{
	/// <summary>
	/// Create an IInstrumentMethodFileAccess interface to
	/// read data from a method (.meth) file
	/// </summary>
	/// <param name="fileName">File to open</param>
	/// <returns>Interface to read data from file</returns>
	public static IInstrumentMethodFileAccess OpenMethod(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName", "Instrument method file name cannot be empty.");
		}
		return InstrumentMethodFileAccess.Create(fileName);
	}
}
