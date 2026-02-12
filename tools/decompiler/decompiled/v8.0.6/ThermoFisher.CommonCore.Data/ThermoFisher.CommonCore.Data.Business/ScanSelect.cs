using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This class holds information needed to select a scan, using either a filer or a compound name.
/// Static methods are available to construct common versions.
/// </summary>
public class ScanSelect : IScanSelect
{
	/// <summary>
	/// Gets or sets a value indicating whether the "scan filter" will be used as a selection mechanism.
	/// ScanFilter must never be null when this returns true.
	/// </summary>
	public bool UseFilter { get; set; }

	/// <summary>
	/// Gets or sets the scan filter.
	/// If UseFilter is false, or this is null, it is not used as a selection mechanism.
	/// </summary>
	public IScanFilter ScanFilter { get; set; }

	/// <summary>
	/// Gets or sets the name.
	/// This is the component or compound name list to filter against.
	/// If this is null or empty, it is not used as a selection mechanism.
	/// </summary>
	public IList<string> Names { get; set; }

	/// <summary>
	/// Create a selector which selects by filter string, based on a specific data source
	/// </summary>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanSelect" />.
	/// </returns>
	public static ScanSelect SelectByFilter(IScanFilter filter)
	{
		return new ScanSelect
		{
			UseFilter = true,
			ScanFilter = filter
		};
	}

	/// <summary>
	/// Create a selector which selects by filter string, based on a specific compound name
	/// </summary>
	/// <param name="compound">
	/// The compound name.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanSelect" />.
	/// </returns>
	public static ScanSelect SelectByCompound(string compound)
	{
		return new ScanSelect
		{
			UseFilter = false,
			Names = new List<string> { compound }
		};
	}

	/// <summary>
	/// Create a selector which selects all scans (no filtering)
	/// </summary>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanSelect" />.
	/// </returns>
	public static IScanSelect SelectAll()
	{
		return new ScanSelect
		{
			UseFilter = false,
			Names = null
		};
	}
}
