using System.Collections.Generic;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The Method interface, for instrument method files
/// </summary>
internal interface IMethod
{
	/// <summary>
	/// Gets the method size.
	/// </summary>
	int MethodSize { get; }

	/// <summary>
	/// Gets the starting offset.
	/// </summary>
	long StartingOffset { get; }

	/// <summary>
	/// Gets the original storage name.
	/// </summary>
	string OriginalStorageName { get; }

	/// <summary>
	/// Gets the storage descriptions.
	/// </summary>
	List<StorageDescription> StorageDescriptions { get; }

	/// <summary>
	/// save a method file.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="methodFilePath">
	/// The method file path.
	/// </param>
	/// <param name="forceOverWrite">
	/// The force over write.
	/// </param>
	void SaveMethodFile(IDisposableReader viewer, string methodFilePath, bool forceOverWrite);
}
