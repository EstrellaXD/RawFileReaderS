using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.BackgroundSubtraction;

/// <summary>
/// Class to support background subtract, which uses a scan list, instead of a raw file for a data
/// source. 
/// </summary>
internal class ScanFromListCreator
{
	private static List<Scan> _scanList = new List<Scan>();

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.BackgroundSubtraction.ScanFromListCreator" /> class.
	/// </summary>
	/// <param name="scanList">
	/// The scan list.
	/// </param>
	public ScanFromListCreator(List<Scan> scanList)
	{
		_scanList = scanList;
	}

	/// <summary>
	/// create segmented scan.
	/// </summary>
	/// <param name="index">
	/// The index into the scan table.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.SegmentedScan" />.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">when index is out of range for the scan list
	/// </exception>
	public SegmentedScan CreateSegmentedScan(int index)
	{
		if (index < _scanList.Count)
		{
			return _scanList[index].SegmentedScan;
		}
		throw new ArgumentOutOfRangeException("index");
	}

	/// <summary>
	/// create centroid stream.
	/// </summary>
	/// <param name="index">
	/// The index into the scan list.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.CentroidStream" />.
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">when index is out of range
	/// </exception>
	public CentroidStream CreateCentroidStream(int index)
	{
		if (index < _scanList.Count)
		{
			return _scanList[index].CentroidScan;
		}
		throw new ArgumentOutOfRangeException("index");
	}
}
