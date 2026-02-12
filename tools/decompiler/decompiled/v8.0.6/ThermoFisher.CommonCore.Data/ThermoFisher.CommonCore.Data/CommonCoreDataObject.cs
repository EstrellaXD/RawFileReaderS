using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// CommonCoreData object is an abstract class. It includes a deep equals feature/&gt;
/// </summary>
[Serializable]
[DataContract]
public abstract class CommonCoreDataObject : ICommonCoreDataObject
{
	/// <summary>
	/// Serves as a hash function for a particular type.
	/// </summary>
	/// <returns>
	/// A hash code for the current <see cref="T:System.Object" />.
	/// </returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>
	/// Compares this object with another.
	/// Traverse the set of member variables to compare against the object that was passed in.
	/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />.</param>
	/// <returns>
	/// true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.
	/// </returns>
	/// <exception cref="T:System.NullReferenceException">The <paramref name="obj" /> parameter is null.</exception>
	public override bool Equals(object obj)
	{
		return PerformEquals(obj, deep: false);
	}

	/// <summary>
	/// Provides a custom deep equality operation when checking for equality.
	/// </summary>
	/// <param name="valueToCompare">The value to compare.</param>
	/// <returns>True if the items are equal, false if they are not.</returns>
	public virtual bool DeepEquals(object valueToCompare)
	{
		return PerformEquals(valueToCompare, deep: true);
	}

	/// <summary>
	/// Compares this object with another.
	/// Traverse the set of member variables to compare against the object that was passed in.
	/// </summary>
	/// <param name="obj">object to compare with</param>
	/// <param name="deep">if true, compare sub-objects</param>
	/// <returns>true if equal</returns>
	protected bool PerformEquals(object obj, bool deep)
	{
		if (obj == null)
		{
			return false;
		}
		if (!obj.GetType().IsAssignableFrom(GetType()))
		{
			return false;
		}
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		foreach (FieldInfo fieldInfo in fields)
		{
			object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(CoreDataElementAttribute), inherit: true);
			if (customAttributes != null && customAttributes.Length != 0 && customAttributes[0] is CoreDataElementAttribute { Ignore: not false })
			{
				continue;
			}
			object value = fieldInfo.GetValue(this);
			object value2 = fieldInfo.GetValue(obj);
			ICommonCoreDataObject commonCoreDataObject = value as ICommonCoreDataObject;
			ICommonCoreDataObject commonCoreDataObject2 = value2 as ICommonCoreDataObject;
			if (commonCoreDataObject != null)
			{
				if (commonCoreDataObject != commonCoreDataObject2)
				{
					if (!deep)
					{
						return false;
					}
					if (!commonCoreDataObject.DeepEquals(commonCoreDataObject2))
					{
						return false;
					}
				}
			}
			else if (value != null)
			{
				if (!value.Equals(value2))
				{
					return false;
				}
			}
			else if (value2 != null)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Performs the default settings for the data object.  This can overridden in each data object that implements the interface to perform
	/// initialization settings.
	/// </summary>
	public virtual void PerformDefaultSettings()
	{
	}
}
