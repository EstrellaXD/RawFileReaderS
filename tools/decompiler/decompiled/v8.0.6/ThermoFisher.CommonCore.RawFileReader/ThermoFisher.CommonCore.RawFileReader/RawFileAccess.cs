using System;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// The raw file access.
/// This class adds functionality of "validating a disk file name" to the base.
/// </summary>
internal class RawFileAccess : RawFileAccessBase
{
	/// <summary>
	/// Create access to a file
	/// </summary>
	/// <param name="fileName">
	/// The file name.
	/// </param>
	/// <param name="preferRandomAccess">true if random access is preferred (some files may get memory mapped).</param>
	/// <param name="manager">data maanger for random aceess mode</param>
	/// <returns>
	/// The RawFileAccess
	/// </returns>
	/// <exception cref="T:System.ArgumentException">on null file
	/// </exception>
	internal static IRawDataExtended Create(string fileName, bool preferRandomAccess, IViewCollectionManager manager)
	{
		if (string.IsNullOrWhiteSpace(fileName))
		{
			throw new ArgumentException(Resources.ErrorEmptyNullFileName);
		}
		if (!fileName.ToUpperInvariant().Contains(".RAW"))
		{
			fileName += ".raw";
		}
		Utilities.Validate64Bit();
		return new RawFileAccess(fileName, preferRandomAccess, manager);
	}

	/// <summary>
	/// Create access to a file
	/// </summary>
	/// <param name="loader">
	/// The file loader.
	/// </param>
	/// <returns>
	/// The RawFileAccess
	/// </returns>
	/// <exception cref="T:System.ArgumentException">on null file
	/// </exception>
	internal static IRawDataExtended Create(IRawFileLoader loader)
	{
		return new RawFileAccess(loader);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.RawFileAccess" /> class. 
	/// </summary>
	/// <param name="loader">
	/// Loader of the file.
	/// </param>
	private RawFileAccess(IRawFileLoader loader)
		: base(loader)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.RawFileAccess" /> class. 
	/// </summary>
	/// <param name="fileName">
	/// Name of the file.
	/// </param>
	/// <param name="preferRandomAccess">true if random access is preferred (some files may get memory mapped).</param>
	/// <param name="manager">data reader for random aceess mode</param>
	private RawFileAccess(string fileName, bool preferRandomAccess, IViewCollectionManager manager)
		: base(RawFileLoaderFactory.CreateLoader(fileName, preferRandomAccess, manager))
	{
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	/// <exception cref="T:System.Exception">Thrown if dispose fails</exception>
	public override void Dispose()
	{
		try
		{
			DisposeOfLoader();
		}
		catch (Exception ex)
		{
			throw new Exception($"Error: {ex.Message} StackTrace: {ex.StackTrace}");
		}
	}
}
