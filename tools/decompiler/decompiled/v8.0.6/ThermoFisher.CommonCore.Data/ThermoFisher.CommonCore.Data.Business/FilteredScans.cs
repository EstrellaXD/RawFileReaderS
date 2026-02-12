using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This class is designed to allow iteration
/// through MS data in a file, based on a filter.
/// To use this, first open a raw file then
/// create this object based on the open file.
/// To process all the matching scans:
/// <code>
/// FilteredScans scans=new FilteredScans(myFile,"MS2");
/// int next=scans.NextSpectrumNumber;
/// while(next &gt; 0)
/// {
///    scans.MoveSpectrumPosition (next);
///    var scan=myFile.GetSegmentedScanFromScanNumber(next,null);
///    //process the scan, then move to the next
///    next=scans.NextSpectrumNumber;
/// }
/// </code>
/// </summary>
public class FilteredScans
{
	private readonly IRawDataPlus _file;

	private string _spectrumFilter;

	private ScanFilterHelper _helper;

	private int _currentSpectrumNumber;

	/// <summary>
	/// Gets the filter used for iterating over spectra, in string form. 
	/// This can be used with the <see cref="P:ThermoFisher.CommonCore.Data.Business.FilteredScans.NextSpectrumNumber" /> method to iterate over scans with a certain filter
	/// </summary>
	public string SpectrumFilterString
	{
		get
		{
			if (!IsOpen())
			{
				return string.Empty;
			}
			return _spectrumFilter ?? string.Empty;
		}
	}

	/// <summary>
	/// Gets the Previous Spectrum, matching the filter
	/// <returns>returns the previous spectrum number in file. 0 if there is no file open</returns>
	/// </summary>
	public int PreviousSpectrumNumber
	{
		get
		{
			if (!IsOpen())
			{
				return 0;
			}
			int num = _currentSpectrumNumber - 1;
			int firstSpectrum = _file.RunHeader.FirstSpectrum;
			while (num >= firstSpectrum)
			{
				if (_file.TestScan(num, _helper))
				{
					return num;
				}
				num--;
			}
			return -1;
		}
	}

	/// <summary>
	/// Gets the Next Spectrum, matching the filter
	/// <returns>returns the next spectrum number in file. 0 if there is no file open</returns>
	/// </summary>
	public int NextSpectrumNumber
	{
		get
		{
			if (!IsOpen())
			{
				return 0;
			}
			int i = _currentSpectrumNumber + 1;
			for (int lastSpectrum = _file.RunHeader.LastSpectrum; i <= lastSpectrum; i++)
			{
				if (_file.TestScan(i, _helper))
				{
					return i;
				}
			}
			return -1;
		}
	}

	/// <summary>
	/// Sets the CurrentSpectrumNumber to the new Spectrum Number
	/// <returns>The Set property has no return value</returns>
	/// </summary>
	public int SpectrumPosition
	{
		set
		{
			if (IsOpen())
			{
				IRunHeaderAccess runHeader = _file.RunHeader;
				if (value >= runHeader.FirstSpectrum && value <= runHeader.LastSpectrum)
				{
					_currentSpectrumNumber = value;
				}
			}
		}
	}

	/// <summary>
	/// Gets a value indicating whether there may be spectra before the current scan.
	/// Note that this does not check if this scan matches the filter.
	/// <returns>returns true if there is an Previous spectrum in file. Otherwise false</returns>
	/// </summary>
	public bool HasPreviousSpectrum
	{
		get
		{
			if (!IsOpen())
			{
				return false;
			}
			return _currentSpectrumNumber > _file.RunHeader.FirstSpectrum;
		}
	}

	/// <summary>
	/// Gets a value indicating whether there are more scans after the current scan
	/// Note that this does not check if this scan matches the filter.
	/// <returns>returns true if there is an Next spectrum in file. Otherwise false</returns>
	/// </summary>
	public bool HasNextSpectrum
	{
		get
		{
			if (!IsOpen())
			{
				return false;
			}
			return _currentSpectrumNumber < _file.RunHeader.LastSpectrum;
		}
	}

	/// <summary>
	/// Test if the file is open
	/// </summary>
	/// <returns>
	/// True if it is open.
	/// </returns>
	private bool IsOpen()
	{
		if (_file != null)
		{
			return _file.IsOpen;
		}
		return false;
	}

	/// <summary>
	/// Set the filter used for iterating over spectra. 
	/// This can be used with the <see cref="P:ThermoFisher.CommonCore.Data.Business.FilteredScans.NextSpectrumNumber" /> method to iterate over scans with a certain filter
	/// </summary>
	/// <param name="filter">an object containing the Filter to be used for iterating over spectra</param>
	private void SetFilter(string filter)
	{
		_spectrumFilter = filter;
		IScanFilter filterFromString = _file.GetFilterFromString(_spectrumFilter);
		_helper = _file.BuildFilterHelper(filterFromString);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.FilteredScans" /> class. 
	/// Creates an iterator to step through the selected file
	/// </summary>
	/// <param name="file">
	/// The file.
	/// </param>
	/// <param name="filter">
	/// The scan filter.
	/// </param>
	public FilteredScans(IRawDataPlus file, string filter)
	{
		_file = file ?? throw new ArgumentNullException("file");
		_currentSpectrumNumber = 0;
		SetFilter(filter);
		if (file != null)
		{
			_file.SelectInstrument(Device.MS, 1);
		}
	}

	/// <summary>
	/// Move the iterator to the start of the list. 
	/// Permitting this loop to process all matching scans
	/// <c>
	/// int next = iterate.MoveFirst();
	/// int count = 0;
	///
	/// while (next &gt; 0)
	/// {
	///    // add your application code to 
	///    // fetch and process scan "next" 
	///    // end of application code block
	///
	///    iterate.SpectrumPosition = next;
	///    count++;
	///    next = iterate.NextSpectrumNumber;
	/// }
	/// </c>
	/// </summary>
	/// <returns>The first valid spectrum number, or -1 if not scans match the given filter</returns>
	public int MoveFirst()
	{
		_currentSpectrumNumber = 0;
		return _currentSpectrumNumber = NextSpectrumNumber;
	}

	/// <summary>
	/// Move the iterator to the end of the list
	/// </summary>
	/// <returns>The last valid spectrum number, or -1 if not scans match the given filter</returns>
	public int MoveLast()
	{
		_currentSpectrumNumber = _file.RunHeader.LastSpectrum + 1;
		return _currentSpectrumNumber = PreviousSpectrumNumber;
	}
}
