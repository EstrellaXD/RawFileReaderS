using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Readers;

/// <summary>
/// Interface to permit multiple (reference counted) access into the same file
/// </summary>
public interface IViewCollectionManager
{
	/// <summary>
	/// Custom data extension that gives additional information to the reader plugin about
	/// extra data/info for the plugin code to use at runtime.
	/// </summary>
	Dictionary<string, string> ExtensionAttributes { get; }

	/// <summary>
	/// The method disposes the specified stream or file and stops tracking it.
	/// </summary>
	/// <param name="streamId">
	/// The stream id - serves as the key.
	/// </param>
	/// <param name="forceToClose">True close the specified stream or file even if it's reference count is non zero;
	/// false skip it if the reference count is more than zero.</param>
	void Close(string streamId, bool forceToClose = false);

	/// <summary>
	/// Gets the random access viewer.
	/// </summary>
	/// <param name="id">The identifier.</param>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="inAcquisition">if set to <c>true</c> [refresh file or stream].</param>
	/// <param name="accessMode">The access mode.</param>
	/// <param name="type">The type.</param>
	/// <returns>Read write data accessor.</returns>
	IReadWriteAccessor GetRandomAccessViewer(Guid id, string fileName, bool inAcquisition, DataFileAccessMode accessMode = DataFileAccessMode.OpenCreateRead, PersistenceMode type = PersistenceMode.Persisted);

	/// <summary>
	/// Determines whether the specified file path is open.
	/// </summary>
	/// <param name="streamId">The file path.</param>
	/// <returns>true if open</returns>
	bool IsOpen(string streamId);

	/// <summary>
	/// Gets the errors.
	/// </summary>
	/// <param name="streamId">Name of the file.</param>
	/// <returns>Error Message</returns>
	string GetErrors(string streamId);

	/// <summary>
	/// Gets the random access viewer.
	/// </summary>
	/// <param name="loaderId">The loader identifier.</param>
	/// <param name="dataName">Name of the shared data (such as a file name or memory map name).</param>
	/// <param name="offset">The offset from the start of the data.</param>
	/// <param name="size">The size (length) of this view in bytes.</param>
	/// <param name="inAcquisition">if set to <c>true</c> file is refreshed so that recently acquired data is visable.</param>
	/// <param name="accessMode">The access mode.</param>
	/// <param name="type">The type.</param>
	/// <returns>Data accessor.</returns>
	IReadWriteAccessor GetRandomAccessViewer(Guid loaderId, string dataName, long offset, long size, bool inAcquisition, DataFileAccessMode accessMode = DataFileAccessMode.OpenCreateRead, PersistenceMode type = PersistenceMode.Persisted);

	/// <summary>Gets the ignore platform keep name case intact flag.</summary>
	/// <returns>
	///   True to keep the original string case intact
	/// </returns>
	bool GetIgnorePlatformKeepNameCaseIntactFlag()
	{
		return false;
	}
}
