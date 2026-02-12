using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// It's used internally to create an instrument methods writer object.
/// </summary>
internal class InstrumentMethodWriterAdapter
{
	/// <summary>
	/// Creates the instrument method writer.
	/// The instrument method writer should open a file if it exists; otherwise, a in-memory method file should be created.
	/// If the input file name is empty, a in-memory method file should be created. When trying to save the "Unnamed" in-memory
	/// method file, caller should use "SaveAs( fileName )" method with a specified file name.
	/// </summary>
	/// <param name="fileName">Name of the instrument method file.</param>
	/// <returns>Instrument method writer object.</returns>
	public static IInstrumentMethodWriter CreateInstrumentMethodWriter(string fileName)
	{
		return new InstrumentMethodWriter(new OleInstrumentMethodBuilder(fileName));
	}
}
