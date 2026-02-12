using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Class to read data from Xcalibur PMD files
/// </summary>
public static class ProcessingMethodFileReader
{
	/// <summary>
	/// Create an IProcessingMethodFileAccess interface to
	/// read data from a processing method (PMD) file.
	/// The entire contents of the file are loaded into memory objects.
	/// The file is not kept open on disk, and so this interfaces has no 
	/// "Save", "Close" or "Dispose" methods.
	/// </summary>
	/// <param name="fileName">File to open</param>
	/// <returns>Interface to read data from the processing method file</returns>
	public static IProcessingMethodFileAccess OpenProcessingMethod(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		return new ProcessingMethodFileAccess(fileName);
	}
}
