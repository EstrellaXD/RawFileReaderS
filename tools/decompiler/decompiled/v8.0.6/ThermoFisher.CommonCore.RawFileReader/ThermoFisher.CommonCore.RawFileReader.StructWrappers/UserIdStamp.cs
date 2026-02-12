using System;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The user id stamp wrapper class wraps the user id structure read from the raw file.
/// </summary>
internal class UserIdStamp : IUserIdStamp
{
	private UserIdStampStruct _data;

	/// <summary>
	/// Gets or sets the date and time.
	/// </summary>
	public DateTime DateAndTime { get; set; }

	/// <summary>
	/// Gets or sets the user name.
	/// </summary>
	public string UserName
	{
		get
		{
			return _data.UserName;
		}
		set
		{
			_data.UserName = value;
		}
	}

	/// <summary>
	/// Gets or sets the windows login.
	/// </summary>
	public string WindowsLogin
	{
		get
		{
			return _data.WindowsLogin;
		}
		set
		{
			_data.WindowsLogin = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.UserIdStamp" /> class.
	/// </summary>
	/// <param name="structure">
	/// The structure.
	/// </param>
	public UserIdStamp(UserIdStampStruct structure)
	{
		_data = structure;
		ConvertDateTime();
	}

	/// <summary>
	/// The method converts the raw file time structure to a <see cref="T:System.DateTime" /> object.
	/// </summary>
	private void ConvertDateTime()
	{
		long num = _data.TimeStamp.dwHighDateTime;
		num <<= 32;
		DateAndTime = DateTime.FromFileTimeUtc(num | (uint)_data.TimeStamp.dwLowDateTime);
	}
}
