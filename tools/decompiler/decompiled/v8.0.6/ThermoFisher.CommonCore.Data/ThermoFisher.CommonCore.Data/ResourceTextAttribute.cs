using System;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Specifies the resource text for an enumeration field.
/// This attribute is used by the EnumFormat class to get display strings for enumeration values.
/// </summary>
/// <remarks>
/// The resource manager base name provided here will automatically be prefixed with the namespace
/// of the enumeration type. For example, an enumeration defined in <c>XCL.Acquisition</c> with a
/// <c>ResourceText</c> attribute base name of "EnumBase" will use a resource manager base name of
/// "XCL.Acquisition.EnumBase".
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
public sealed class ResourceTextAttribute : Attribute
{
	private readonly string _baseName;

	private readonly string _resourceName;

	/// <summary>
	/// Gets the resource manager base name for the enumeration field.
	/// </summary>
	public string BaseName => _baseName;

	/// <summary>
	/// Gets the resource name for the enumeration field, without the enumeration type's namespace prefix.
	/// </summary>
	public string ResourceName => _resourceName;

	/// <summary>
	/// Initializes a new instance of the ResourceTextAttribute class with the resource name
	/// for the enumeration field. The default resource base name of "Enumerations" is used.
	/// </summary>
	/// <param name="resourceName">The resource name for the enumeration value.</param>
	public ResourceTextAttribute(string resourceName)
		: this(resourceName, "Enumerations")
	{
	}

	/// <summary>
	/// Initializes a new instance of the ResourceTextAttribute class with the resource name
	/// and resource base name for the enumeration field.
	/// </summary>
	/// <param name="resourceName">The resource name for the enumeration value.</param>
	/// <param name="baseName">The resource manager base name for the enumeration value.</param>
	public ResourceTextAttribute(string resourceName, string baseName)
	{
		_baseName = baseName;
		_resourceName = resourceName;
	}
}
