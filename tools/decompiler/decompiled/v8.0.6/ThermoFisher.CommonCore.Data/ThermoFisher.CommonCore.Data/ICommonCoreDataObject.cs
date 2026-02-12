namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// ICommonCoreDataObject is an interface that for CommonCoreData objects.  It allows for clearing the dirty flags during a save operation. 
/// For example, before saving your data object that implements CommonCoreDataObject, cast it to a ICommonCoreDataObject and set
/// IsDirty to false.  All data objects that implement the abstract class CommonCoreDataObject implement the ICommonCoreDataObject interface as well.
/// </summary>
public interface ICommonCoreDataObject
{
	/// <summary>
	/// Provides a custom deep equality operation when checking for equality.
	/// </summary>
	/// <param name="valueToCompare">The value to compare.</param>
	/// <returns>True if objects are equal</returns>
	bool DeepEquals(object valueToCompare);

	/// <summary>
	/// Performs the default settings for the data object.  This can be overridden in each data object that implements the interface to perform
	/// initialization settings.
	/// </summary>
	void PerformDefaultSettings();
}
