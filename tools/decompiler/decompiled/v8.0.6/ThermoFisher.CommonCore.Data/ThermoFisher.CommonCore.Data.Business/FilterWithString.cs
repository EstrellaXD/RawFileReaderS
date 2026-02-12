using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines a combination of plain text and interface defintions of filters.
/// </summary>
public class FilterWithString : IFilterWithString
{
	/// <summary>
	/// Gets a Standard (parsable) text form of the filter. This does not include any compound names.
	/// </summary>
	public string Filter { get; set; }

	/// <summary>
	/// Gets the scan filer (as accessable fields)
	/// </summary>
	public IScanFilter ScanFilter { get; set; }

	/// <summary>
	/// Gets a string which combines the compound name and the filter text.
	/// </summary>
	public string FilterWithName { get; internal set; }

	/// <summary>
	/// Gets the compound name
	/// </summary>
	public string Name { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether this object only has a compound name,
	/// and does not have any filter text or filter interface defined.
	/// When this is set: Only the Name property should be used.
	/// </summary>
	public bool NameOnly
	{
		get
		{
			if (!string.IsNullOrEmpty(Name))
			{
				return string.IsNullOrEmpty(FilterWithName);
			}
			return false;
		}
	}

	/// <summary>
	/// Creates a new instance of FilterWithString, parsing the filter based on 
	/// a supplied raw file.
	/// </summary>
	/// <param name="rawData">The raw data, used to parse this filter</param>
	/// <param name="text">The text form of the filter</param>
	public FilterWithString(IRawDataPlus rawData, string text)
	{
		Filter = text;
		ScanFilter = rawData.GetFilterFromString(text);
	}

	/// <summary>
	/// Creates a new instance of FilterWithString.
	/// </summary>
	public FilterWithString()
	{
	}
}
