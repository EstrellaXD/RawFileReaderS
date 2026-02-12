using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Information available from Xcalibur file headers..
/// </summary>
public interface IFileHeader
{
	/// <summary>
	/// Gets or sets the creator Id. The creator Id is the full text user name of the user
	/// when the file is created.
	/// </summary>
	string WhoCreatedId { get; set; }

	/// <summary>
	/// Gets or sets the creator Login name.
	/// The creator login name is the user name of the user
	/// when the file is created, as entered at the "user name, password" screen in windows.
	/// </summary>
	string WhoCreatedLogon { get; set; }

	/// <summary>
	/// Gets or sets the creator Id. The creator Id is the full text user name of the user
	/// when the file is created.
	/// </summary>
	string WhoModifiedId { get; set; }

	/// <summary>
	/// Gets or sets the creator Login name.
	/// The creator login name is the user name of the user
	/// when the file is created, as entered at the "user name, password" screen in windows.
	/// </summary>
	string WhoModifiedLogon { get; set; }

	/// <summary>
	/// Gets or sets the type of the file.
	/// If the file is not recognized, the value of the FileType will be set to "Not Supported" 
	/// </summary>
	FileType FileType { get; set; }

	/// <summary>
	/// Gets or sets the file format revision
	/// Note: this does not refer to revisions of the content.
	/// It defines revisions of the binary files structure.
	/// </summary>
	int Revision { get; set; }

	/// <summary>
	/// Gets or sets the file creation date.
	/// </summary>
	DateTime CreationDate { get; set; }

	/// <summary>
	/// Gets or sets the modified date.
	/// File changed audit information (most recent change)
	/// </summary>
	/// <value>
	/// The modified date.
	/// </value>
	DateTime ModifiedDate { get; set; }

	/// <summary>
	/// Gets or sets the number of times modified.
	/// </summary>
	/// <value>
	/// The number of times the file has been modified.
	/// </value>
	int NumberOfTimesModified { get; set; }

	/// <summary>
	/// Gets or sets the number of times calibrated.
	/// </summary>
	/// <value>
	/// The number of times calibrated.
	/// </value>
	int NumberOfTimesCalibrated { get; set; }

	/// <summary>
	/// Gets or sets the file description.
	/// User's narrative description of the file, 512 unicode characters (1024 bytes)
	/// </summary>
	/// <value>
	/// The file description.
	/// </value>
	string FileDescription { get; set; }
}
