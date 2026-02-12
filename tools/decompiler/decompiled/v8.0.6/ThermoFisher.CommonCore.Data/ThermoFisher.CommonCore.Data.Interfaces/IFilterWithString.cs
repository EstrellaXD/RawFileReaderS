namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines a combination of plain text and interface definitions of filters.
/// </summary>
public interface IFilterWithString
{
	/// <summary>
	/// Gets a Standard (parsable) text form of the filter. This does not include any compound names.
	/// </summary>
	string Filter { get; }

	/// <summary>
	/// Gets the scan filer (as accessible fields)
	/// </summary>
	IScanFilter ScanFilter { get; }

	/// <summary>
	/// Gets a value indicating whether this object only has a compound name,
	/// and does not have any filter text or filter interface defined.
	/// When this is set: Only the Name property should be used.
	/// </summary>
	bool NameOnly { get; }

	/// <summary>
	/// Gets the compound name
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Gets a string which combines the compound name and the filter text.
	/// </summary>
	string FilterWithName { get; }
}
