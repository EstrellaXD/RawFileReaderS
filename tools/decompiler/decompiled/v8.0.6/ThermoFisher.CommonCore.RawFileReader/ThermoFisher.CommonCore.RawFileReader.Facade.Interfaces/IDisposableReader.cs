using System;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The Disposable Reader interface.
/// Extends the reader with IDisposable, 
/// for readers which are mapped to a disposable object, such as a file.
/// </summary>
public interface IDisposableReader : IMemoryReader, IDisposable
{
	/// <summary>
	/// Optional, report any issues when attempting to create a reader for data
	/// </summary>
	/// <returns>Issues found with the read request.
	/// By default, this should have "FileSizeExceeded" false.</returns>
	IReaderIssues ReaderIssues()
	{
		return new NoIssues();
	}
}
