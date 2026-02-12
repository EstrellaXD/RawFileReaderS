using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The file header.
/// </summary>
public class FileHeader : IFileHeader, IFileHeaderUpdate
{
	/// <summary>
	/// Gets or sets the creator Id. The creator Id is the full text user name of the user
	/// when the file is created.
	/// </summary>
	public string WhoCreatedId { get; set; }

	/// <summary>
	/// Gets or sets the creator Login name.
	/// The creator login name is the user name of the user
	/// when the file is created, as entered at the "user name, password" screen in windows.
	/// </summary>
	public string WhoCreatedLogon { get; set; }

	/// <summary>
	/// Gets or sets the creator Id. The creator Id is the full text user name of the user
	/// when the file is created.
	/// </summary>
	public string WhoModifiedId { get; set; }

	/// <summary>
	/// Gets or sets the creator Login name.
	/// The creator login name is the user name of the user
	/// when the file is created, as entered at the "user name, password" screen in windows.
	/// </summary>
	public string WhoModifiedLogon { get; set; }

	/// <summary>
	/// Gets or sets the type of the file.
	/// If the file is not recognized, the value of the FileType will be set to "Not Supported" 
	/// </summary>
	public FileType FileType { get; set; }

	/// <summary>
	/// Gets or sets the file format revision
	/// Note: this does not refer to revisions of the content.
	/// It defines revisions of the binary files structure.
	/// </summary>
	public int Revision { get; set; }

	/// <summary>
	/// Gets or sets the file creation date in local time.
	/// </summary>
	public DateTime CreationDate { get; set; }

	/// <summary>
	/// Gets or sets the modified date in local time.
	/// File changed audit information (most recent change)
	/// </summary>
	/// <value>
	/// The modified date.
	/// </value>
	public DateTime ModifiedDate { get; set; }

	/// <summary>
	/// Gets or sets the number of times modified.
	/// </summary>
	/// <value>
	/// The number of times the file has been modified.
	/// </value>
	public int NumberOfTimesModified { get; set; }

	/// <summary>
	/// Gets or sets the number of times calibrated.
	/// </summary>
	/// <value>
	/// The number of times calibrated.
	/// </value>
	public int NumberOfTimesCalibrated { get; set; }

	/// <summary>
	/// Gets or sets the file description.
	/// User's narrative description of the file, 512 unicode characters (1024 bytes)
	/// </summary>
	/// <value>
	/// The file description.
	/// </value>
	public string FileDescription { get; set; }
}
