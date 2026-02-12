using System;
using System.IO;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// It's used internally to create an instrument methods exporter object.
/// </summary>
internal class InstrumentMethodExporterAdapter
{
	/// <summary>
	/// Create an IInstrumentMethodExporter interface to
	/// read instrument method from a raw (.raw) file and able
	/// to write it to an instrument method (.meth) file.
	/// </summary>
	/// <param name="fileName">Raw file to open</param>
	/// <returns>Interface to read instrument method from file</returns>
	public static IInstrumentMethodExporter OpenRawFile(string fileName)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			throw new ArgumentNullException("fileName", "File name cannot be empty.");
		}
		if (!File.Exists(fileName))
		{
			throw new ArgumentException("File doesn't exist: " + fileName);
		}
		if (!fileName.ToUpperInvariant().EndsWith(".RAW"))
		{
			throw new ArgumentException("Invalid file extension");
		}
		return new RawFileInstrumentMethodAccessor(fileName);
	}
}
