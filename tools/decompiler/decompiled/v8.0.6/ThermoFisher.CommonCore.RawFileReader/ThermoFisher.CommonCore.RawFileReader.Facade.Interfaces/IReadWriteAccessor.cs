using System;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The Read Write Accessor interface that has read and write capabilities.
/// </summary>
public interface IReadWriteAccessor : IMemMapWriter, IDisposable, IDisposableReader, IMemoryReader
{
	/// <summary>
	/// Gets a suggested minimum amount of memory to read and write
	/// </summary>
	int SuggestedChunkSize { get; }
}
