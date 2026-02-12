using System;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The UserIdStamp interface.
/// </summary>
internal interface IUserIdStamp
{
	/// <summary>
	///     Gets or sets the time when this stamp was created
	/// </summary>
	DateTime DateAndTime { get; set; }

	/// <summary>
	///     Gets or sets the name of the user who acquired the file. For example "John Smith"
	/// </summary>
	string UserName { get; set; }

	/// <summary>
	///     Gets or sets the login name of the operator who acquired the file. For example "jsmith"
	/// </summary>
	string WindowsLogin { get; set; }
}
