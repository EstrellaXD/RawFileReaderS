using System.Collections.Generic;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// This interface permits access data for a particular instrument in an instrument method.
/// </summary>
public interface IInstrumentMethodDataAccess
{
	/// <summary>
	/// Gets the plain text form of an instrument method
	/// </summary>
	string MethodText { get; }

	/// <summary>
	/// Gets all streams for this instrument, apart from the "Text" stream.
	/// Typically an instrument has a stream called "Data" containing the method in binary or XML.
	/// Other streams (private to the instrument) may also be created.
	/// </summary>
	IReadOnlyDictionary<string, byte[]> StreamBytes { get; }
}
