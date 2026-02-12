namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Subset of the IFileHeader interface, it's for restricting
/// changes to the file header object during raw file creation.<para />
/// This will prevent errors, such as changing the FileType to other 
/// instead of RawFile and Revision # to 0 instead of 66 (file struct version).<para />
/// This would allow application to change the creator name/id and file
/// description values, instead of the default value. <para />
/// The current default values for:<para />
/// File description = string.Empty<para />
/// WhoCreatedId = Environment.UserName (who is currently logged on to the Wins)<para />
/// WhoCreatedLogon = Environment.UserName
/// </summary>
public interface IFileHeaderUpdate
{
	/// <summary>
	/// Gets the creator Id. The creator Id is the full text user name of the user
	/// when the file is created.
	/// </summary>
	string WhoCreatedId { get; }

	/// <summary>
	/// Gets the creator Login name.
	/// The creator login name is the user name of the user
	/// when the file is created, as entered at the "user name, password" screen in windows.
	/// </summary>
	string WhoCreatedLogon { get; }

	/// <summary>
	/// Gets the file description.
	/// User's narrative description of the file, 512 unicode characters (1024 bytes)
	/// </summary>
	/// <value>
	/// The file description.
	/// </value>
	string FileDescription { get; }
}
