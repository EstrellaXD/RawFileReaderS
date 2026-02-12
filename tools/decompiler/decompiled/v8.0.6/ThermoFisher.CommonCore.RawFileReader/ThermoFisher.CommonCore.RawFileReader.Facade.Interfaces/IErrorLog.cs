using System;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The Error Log interface.
/// </summary>
internal interface IErrorLog : IRealTimeAccess, IDisposable
{
	/// <summary>
	///     Gets the number of entries in the error log.
	/// </summary>
	int Count { get; }

	/// <summary>
	/// The method gets an error log item by looking up the index
	/// </summary>
	/// <param name="index">The index.</param>
	/// <returns>return the entries in the log by index</returns>
	ErrorLogItem GetItem(int index);
}
