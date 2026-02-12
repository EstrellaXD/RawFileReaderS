using System;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// CoreDataElementAttribute is used in the custom deep equals operation. This attribute allows a user to specify whether or not to include a 
/// property in the equals operation.
/// </summary>
[Serializable]
public sealed class CoreDataElementAttribute : Attribute
{
	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="T:ThermoFisher.CommonCore.Data.CoreDataElementAttribute" /> is ignore.
	/// </summary>
	/// <value><c>true</c> if ignore; otherwise, <c>false</c>.</value>
	public bool Ignore { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.CoreDataElementAttribute" /> class and sets the Ignore property to false.
	/// </summary>
	public CoreDataElementAttribute()
	{
		Ignore = false;
	}
}
