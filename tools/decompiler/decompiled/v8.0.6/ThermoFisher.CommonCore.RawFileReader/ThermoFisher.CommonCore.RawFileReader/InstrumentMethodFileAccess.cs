using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Class containing logic to open an instrument method, and to
/// pass required interface methods down to business logic.
/// </summary>
internal class InstrumentMethodFileAccess
{
	private readonly InstrumentMethodFileLoader _fileLoader;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.InstrumentMethodFileAccess" /> class.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	private InstrumentMethodFileAccess(string fileName)
	{
		if (!fileName.ToUpperInvariant().EndsWith(".METH"))
		{
			fileName += ".meth";
		}
		_fileLoader = new InstrumentMethodFileLoader(fileName);
	}

	/// <summary>
	/// Create access to method data
	/// </summary>
	/// <param name="fileName">
	/// The file name.
	/// </param>
	/// <returns>
	/// Interface to read method
	/// </returns>
	internal static IInstrumentMethodFileAccess Create(string fileName)
	{
		return new InstrumentMethodFileAccess(fileName)._fileLoader;
	}
}
