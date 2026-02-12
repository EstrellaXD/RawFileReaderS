using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Static class used to convert enumeration values to custom strings.
/// This class is necessary because there is no mechanism to override the <c>ToString()</c> method
/// for enumerations.
/// </summary>
/// <remarks>
/// To get the customized string for an enumeration value, a program must call the static method
/// <c>EnumFormat.ToString(value)</c> and not <c>value.ToString()</c>.
/// </remarks>
public static class EnumFormat
{
	private static readonly Dictionary<Enum, string> DictCustomString = new Dictionary<Enum, string>();

	/// <summary>
	/// Gets the display text for an enumeration value.
	/// </summary>
	/// <remarks>
	/// A string is searched for in three places, in this order:
	/// <para>A custom string specified by <c>SetCustomString()</c>.</para>
	/// <para>A string resource in the resource file <c>(enum namespace).Enumerations</c> named
	/// <c>(type name).(enum name)</c>.</para>
	/// <para>From the <c>DisplayTextAttribute</c> for the enumeration value.</para>
	/// If no value is found, the standard string representation of the enumeration is returned.
	/// </remarks>
	/// <param name="enumeration">The enumeration value for which to obtain the text string.</param>
	/// <returns>The string for the enumeration value.</returns>
	public static string ToString(Enum enumeration)
	{
		if (enumeration == null)
		{
			return string.Empty;
		}
		if (DictCustomString.ContainsKey(enumeration))
		{
			return DictCustomString[enumeration];
		}
		string text = enumeration.ToString();
		Type type = enumeration.GetType();
		FieldInfo? field = type.GetField(text);
		string text2 = text;
		string baseName = type.Namespace + ".Enumerations";
		string name = type.Name + "." + text;
		object[] customAttributes = field.GetCustomAttributes(typeof(ResourceTextAttribute), inherit: true);
		if (customAttributes != null && customAttributes.Length != 0)
		{
			ResourceTextAttribute resourceTextAttribute = (ResourceTextAttribute)customAttributes[0];
			baseName = type.Namespace + "." + resourceTextAttribute.BaseName;
			name = resourceTextAttribute.ResourceName;
		}
		ResourceManager resourceManager = new ResourceManager(baseName, type.GetTypeInfo().Assembly);
		try
		{
			string text3 = resourceManager.GetString(name);
			if (text3 != null)
			{
				text2 = text3;
			}
		}
		catch (InvalidOperationException)
		{
		}
		catch (MissingManifestResourceException)
		{
		}
		DictCustomString[enumeration] = text2;
		return text2;
	}

	/// <summary>
	/// Sets a custom string for an enumeration value.
	/// This will override any string set with a <c>DisplayTextAttribute</c>.
	/// </summary>
	/// <param name="enumeration">The enumeration value for which to set the custom text string.</param>
	/// <param name="text">The custom text string for this enumeration value.</param>
	public static void SetCustomString(Enum enumeration, string text)
	{
		if (enumeration != null && text != null)
		{
			DictCustomString[enumeration] = text;
		}
	}
}
