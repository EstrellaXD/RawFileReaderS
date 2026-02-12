using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Class to read data from an SLD file
/// </summary>
public static class SequenceFileReader
{
	/// <summary>
	/// Create an ISequenceFileAccess interface to
	/// read data from a sequence (SLD) file
	/// </summary>
	/// <param name="fileName">File to open</param>
	/// <returns>Interface to read data from file</returns>
	public static ISequenceFileAccess OpenSequence(string fileName)
	{
		if (fileName == null)
		{
			throw new ArgumentNullException("fileName");
		}
		return new SequenceFileAccess(fileName);
	}
}
