namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The RealTimeAccess interface.
/// </summary>
internal interface IRealTimeAccess
{
	/// <summary>
	/// Gets the file revision.
	/// </summary>
	int FileRevision { get; }

	/// <summary>
	/// Gets the header file map name. <para />
	/// It's only meaningful in Generic data.
	/// </summary>
	string HeaderFileMapName { get; }

	/// <summary>
	/// Gets the data file map name.
	/// </summary>
	string DataFileMapName { get; }

	/// <summary>
	/// Close and reopen all of the file mapping objects for this file object
	/// </summary>
	/// <returns>True refresh succeed, false otherwise </returns>
	bool RefreshViewOfFile();
}
