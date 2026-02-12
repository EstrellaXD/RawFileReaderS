using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Methods which act on a selected instrument
/// </summary>
public interface IDetectorReaderPlus : IDetectorReaderBase
{
	/// <summary>
	/// Implements IIndexAndEvent
	/// </summary>
	public class IndexAndEvent : IIndexAndEvent
	{
		/// <inheritdoc />
		public IMsScanIndexAccess ScanIndex { get; set; }

		/// <inheritdoc />
		public IScanEvent ScanEvent { get; set; }
	}

	/// <summary>
	/// Gets the detector which was configured when this interface was constructed (immutable).
	/// To work on a different device, obtain a new reader interface from the raw data.
	/// </summary>
	IConfiguredDetector ConfiguredDetector
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>
	/// Gets extended the run header details from the selected device.
	/// All properties of the returned interface which end in "Count" may return -1        
	/// if that counter is not valid for the selected device type. For example: TuneDataCount when called on Device.Other
	/// </summary>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	IRunHeader RunHeaderEx { get; }

	/// <summary>
	/// Gets the scan events.
	/// This is the set of events which have been programmed in advance of
	/// collecting data (based on the MS method).
	/// This does not analyze any scan data.
	/// </summary>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	IScanEvents ScanEvents { get; }

	/// <summary>
	/// Gets the set of user labels. These are labels for "user columns" in the sample information.
	/// </summary>
	string[] UserLabel { get; }

	/// <summary>
	/// Gets the labels and index positions of the status log items which may be plotted.
	/// That is, the numeric items.
	/// Index is a zero based index into the log record (the array returned by GetStatusLogHeaderInformation)
	/// Labels names are returned by "Key" and the index into the log record is "Value".
	/// </summary>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	KeyValuePair<string, int>[] StatusLogPlottableData { get; }

	/// <summary>
	/// Calculate the filters for this raw file, and return as an array.
	/// </summary>
	/// <returns>Auto generated list of unique filters</returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	ReadOnlyCollection<IScanFilter> GetFilters();

	/// <summary>
	/// Calculate the filters for this raw file within the range of scans supplied, and return as an array.
	/// </summary>
	/// <param name="startScan">First scan to analyze</param>
	/// <param name="endScan">Last scan to analyze</param>
	/// <returns>
	/// Auto generated list of unique filters
	/// </returns>
	ReadOnlyCollection<IScanFilter> GetFiltersForScanRange(int startScan, int endScan);

	/// <summary>
	/// Get the filter (scanning method) for a scan number.
	/// This returns the scanning method in the form of a filter rule set, so
	/// that it can be used to select similar scans (for example in a chromatogram).
	/// This method is only defined for MS detectors.
	/// Calling for other detectors or with no selected detector is a coding
	/// error which may result in a null return or exceptions, depending on the implementation.
	/// </summary>
	/// <param name="scan">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.IScanFilter" />.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	IScanFilter GetFilterForScanNumber(int scan);

	/// <summary>
	/// Get a filter interface from a string. Parses the supplied string to a set of filtering rules.
	/// </summary>
	/// <param name="filter">
	/// The filter string.
	/// </param>
	/// <returns>
	/// An interface representing the filter fields, converted from the supplied string.
	/// If the string is not a valid format, this may return null.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	IScanFilter GetFilterFromString(string filter);

	/// <summary>
	/// Get a filter interface from a scan event interface.
	/// Permits filtering to be done based on programmed events, such as
	/// an item from the "ScanEvents" table,
	/// or from constructed data using <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanEventBuilder" />.
	/// This method initializes the filter based on the current raw file
	/// (for example: mass precision)
	/// </summary>
	/// <param name="scanEvent">
	/// The event data.
	/// </param>
	/// <returns>
	/// An interface representing the filter fields, converted from the supplied event.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	IScanFilter CreateFilterFromScanEvent(IScanEvent scanEvent);

	/// <summary>
	/// Get a filter interface from a string, with a given mass precision.
	/// Parses the supplied string.
	/// </summary>
	/// <param name="filter">
	/// The filter string.
	/// </param>
	/// <param name="precision">Precisions of masses (number of decimal places)</param>
	/// <returns>
	/// An interface representing the filter fields, converted from the supplied string.
	/// If the string is not a valid format, this may return null.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	IScanFilter GetFilterFromString(string filter, int precision);

	/// <summary>
	/// Obtain an interface to iterate over a scans which match a specified filter.
	/// The iterator is initialized at "scan 0" such that "GetNext" will return the first matching scan in the file.
	/// This is a low level version of <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.GetFilteredScanEnumerator(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter)" />
	/// </summary>
	/// <param name="filter">Filter, which all returned scans match.
	/// This filter may be created from a string using <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.GetFilterFromString(System.String,System.Int32)" /></param>
	/// <returns>An iterator which can step back and forth over scans matching a given filter.</returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	IFilteredScanIterator GetFilteredScanIterator(IScanFilter filter);

	/// <summary>
	/// Get a filtered scan enumerator, to obtain the collection of scans matching given filter rules.
	/// </summary>
	/// <param name="filter">The filter, which all enumerated scans match.
	/// This filter may be created from a string using <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.GetFilterFromString(System.String,System.Int32)" />
	/// </param>
	/// <returns>
	/// An enumerator which can be used to "foreach" over all scans in a file, which match a given filter.
	/// Note that each "step" through the enumerator will access further data from the file.
	/// To get a complete list of matching scans in one call, the "ToArray" extension can be called,
	/// but this will result in a delay as all scans in the file are analyzed to return this array.
	/// For fine grained iterator control, including "back stepping" use <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawDataPlus.GetFilteredScanIterator(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter)" />
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	IEnumerable<int> GetFilteredScanEnumerator(IScanFilter filter);

	/// <summary>
	/// Get a filtered scan enumerator, to obtain the collection of scans matching given filter rules,
	/// over a given time range.
	/// </summary>
	/// <param name="filter">
	/// The filter, which all enumerated scans must match.
	/// This filter may be created from a string using <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.GetFilterFromString(System.String,System.Int32)" />
	/// </param>
	/// <param name="startTime">
	/// The start Time.
	/// </param>
	/// <param name="endTime">
	/// The End Time.
	/// </param>
	/// <returns>
	/// An enumerator which can be used to "foreach" over all scans in a time range, which match a given filter.
	/// Note that each "step" through the enumerator will access further data from the file.
	/// To get a complete list of matching scans in one call, the "ToArray" extension can be called,
	/// but this will result in a delay as all scans in the time range are analyzed to return this array.
	/// For fine grained iterator control, including "back stepping" use <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawDataPlus.GetFilteredScanIterator(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter)" />
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	IEnumerable<int> GetFilteredScanEnumeratorOverTime(IScanFilter filter, double startTime, double endTime);

	/// <summary>
	/// Gets the scan event details for a scan. Determines how this scan was programmed.
	/// </summary>
	/// <param name="scan">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.IScanEvent" /> interface, to get detailed information about a scan.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	IScanEvent GetScanEventForScanNumber(int scan);

	/// <summary>
	/// Gets the scan event as a string for a scan.
	/// </summary>
	/// <param name="scan">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The event as a string.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	string GetScanEventStringForScanNumber(int scan);

	/// <summary>
	/// Gets an entry from the instrument error log.
	/// </summary>
	/// <param name="index">
	/// Zero based index.
	/// The number of records available is RunHeaderEx.ErrorLogCount </param>
	/// <returns>An interface to read a specific log entry</returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	IErrorLogEntry GetErrorLogItem(int index);

	/// <summary>
	/// Gets the status log data, from all log entries, based on a specific position in the log record.
	/// For example: "position" may be selected from one of the key value pairs returned from <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.StatusLogPlottableData" />
	/// in order to create a trend plot of a particular value.
	/// The interface returned has an array of retention times and strings.
	/// If the position was selected by using <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.StatusLogPlottableData" />, then the strings may be converted "ToDouble" to get
	/// the set of numeric values to plot.
	/// </summary>
	/// <param name="position">
	/// The position within the list of available status log values.
	/// </param>
	/// <returns>
	/// An interface containing the times and logged values for the selected status log field.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	ISingleValueStatusLog GetStatusLogAtPosition(int position);

	/// <summary>
	/// Get scan dependents.
	/// Returns a list of scans, for which this scan was the parent.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="filterPrecisionDecimals">
	/// The filter precision decimals.
	/// </param>
	/// <returns>
	/// Information about how data dependent scanning was performed.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	IScanDependents GetScanDependents(int scanNumber, int filterPrecisionDecimals);

	/// <summary>
	/// Gets the unique compound names as arrays of strings.
	/// </summary>
	/// <returns>
	/// The Compound Names.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	string[] GetCompoundNames();

	/// <summary>
	/// Gets the unique compound names within the range of scans supplied, and return as an array.
	/// </summary>
	/// <param name="startScan">First scan to analyze</param>
	/// <param name="endScan">Last scan to analyze</param>
	/// <returns>
	/// The Compound Names.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	string[] GetCompoundNamesForScanRange(int startScan, int endScan);

	/// <summary>
	/// Gets the unique compound names as arrays of strings by given filter.
	/// </summary>
	/// <param name="scanFilter">
	/// The scan Filter.
	/// </param>
	/// <returns>
	/// The compound names.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	string[] GetCompoundNames(string scanFilter);

	/// <summary>
	/// Get the scan filters which match a compound name.
	/// When implemented against raw files, this may have a performance impact on applications.
	/// For files which have a programmed event table, this will be fast,
	/// as the information can be taken directly from the events.
	/// If there is no event table, then event data is checked for every scan in the file (slower).
	/// </summary>
	/// <param name="compoundName">
	/// The compound name.
	/// </param>
	/// <returns>
	/// The array of matching scan filters (in string format).
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	string[] GetScanFiltersFromCompoundName(string compoundName);

	/// <summary>
	/// Get the scan filters which match each compound name.
	/// When implemented against raw files, this may have a performance impact on applications.
	/// For files which have a programmed event table, this will be fast,
	/// as the information can be taken directly from the events.
	/// If there is no event table, then event data is checked for every scan in the file (slower).
	/// </summary>
	/// <param name="compoundNames">The compound names.</param>
	/// <returns>
	/// The arrays of matching scan filters (in string format) for each compound.
	/// </returns>
	string[][] GetScanFiltersFromCompoundNames(string[] compoundNames);

	/// <summary>
	/// Create a chromatogram from the data stream.
	/// Extended version:
	/// Parameters include option for component names.
	/// Includes base peak data for each scan.
	/// </summary>
	/// <param name="settings">
	/// Definition of how the chromatogram is read
	/// </param>
	/// <param name="startScan">
	/// First scan to read from. -1 for "all data"
	/// </param>
	/// <param name="endScan">
	/// Last scan to read from. -1 for "all data"
	/// </param>
	/// <param name="toleranceOptions">
	/// For mass range or base peak chromatograms,
	/// if the ranges have equal mass values,
	/// then <paramref name="toleranceOptions" /> are used to determine a band
	/// subtracted from low and added to high to search for matching masses.
	/// For example: with 5 ppm tolerance, the caller can pass a single mass value (same low and high) for each mass range,
	/// and get chromatograms of those masses +/- 5 ppm.
	/// </param>
	/// <returns>
	/// Chromatogram points
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.RequiresChromatographicDeviceException">
	/// Thrown if the selected device is of a type that does not support chromatogram generation</exception>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.InvalidFilterFormatException">
	/// Thrown if filters are sent (for MS chromatograms) which cannot be parsed</exception>
	IChromatogramDataPlus GetChromatogramDataEx(IChromatogramSettingsEx[] settings, int startScan, int endScan, MassOptions toleranceOptions);

	/// <summary>
	/// Create a chromatogram from the data stream.
	/// Extended version:
	/// Parameters include option for component names.
	/// Includes base peak data for each scan.
	/// </summary>
	/// <param name="settings">
	/// Definition of how the chromatogram is read
	/// </param>
	/// <param name="startScan">
	/// First scan to read from. -1 for "all data"
	/// </param>
	/// <param name="endScan">
	/// Last scan to read from. -1 for "all data"
	/// </param>
	/// <returns>
	/// Chromatogram points
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.RequiresChromatographicDeviceException">
	/// Thrown if the selected device is of a type that does not support chromatogram generation</exception>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.InvalidFilterFormatException">
	/// Thrown if filters are sent (for MS chromatograms) which cannot be parsed</exception>
	IChromatogramDataPlus GetChromatogramDataEx(IChromatogramSettingsEx[] settings, int startScan, int endScan);

	/// <summary>
	/// Create a chromatogram from the data stream.
	/// Extended version:
	/// Parameters include option for component names.
	/// Includes base peak data for each scan.
	/// </summary>
	/// <param name="settings">
	/// Definition of how the chromatogram is read
	/// </param>
	/// <param name="startScan">
	/// First scan to read from. -1 for "all data"
	/// </param>
	/// <param name="endScan">
	/// Last scan to read from. -1 for "all data"
	/// </param>
	/// <param name="toleranceOptions">
	/// For mass range or base peak chromatograms,
	/// if the ranges have equal mass values,
	/// then <paramref name="toleranceOptions" /> are used to determine a band
	/// subtracted from low and added to high to search for matching masses.
	/// For example: with 5 ppm tolerance, the caller can pass a single mass value (same low and high) for each mass range,
	/// and get chromatograms of those masses +/- 5 ppm.
	/// </param>
	/// <param name="alwaysUseAccuratePrecursors">If set: then precursor tolerance is based on
	/// the precision of the scan filters supplied
	/// (+/- half of the final digit).
	/// If not set, then precursors are matched based on settings logged by the device in the raw data</param>
	/// <returns>
	/// Chromatogram points
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.RequiresChromatographicDeviceException">
	/// Thrown if the selected device is of a type that does not support chromatogram generation</exception>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.InvalidFilterFormatException">
	/// Thrown if filters are sent (for MS chromatograms) which cannot be parsed</exception>
	IChromatogramDataPlus GetChromatogramDataEx(IChromatogramSettingsEx[] settings, int startScan, int endScan, MassOptions toleranceOptions, bool alwaysUseAccuratePrecursors)
	{
		return GetChromatogramDataEx(settings, startScan, endScan, toleranceOptions);
	}

	/// <summary>
	/// Test if a scan passes a filter.
	/// If the set of "all matching scans in a file" is required, consider using <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.GetFilteredScanEnumerator(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter)" /> or <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.GetFilteredScanEnumeratorOverTime(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter,System.Double,System.Double)" />
	/// </summary>
	/// <param name="scan">the scan number</param>
	/// <param name="filter">the filter to test</param>
	/// <returns>True if this scan passes the filter</returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	bool TestScan(int scan, string filter);

	/// <summary>
	/// Returns the (unformatted) Trailer Extra value for a specific field in the specified scan number. 
	/// The object type depends on the field type, as returned by 
	/// GetTrailerExtraHeaderInformation.
	/// This offers higher performance, where numeric values are needed,
	/// as it avoids translation to and from strings.
	/// <c>
	/// Numeric values (where the header for this field returns "True" for IsNumeric)
	/// can always be cast up to double.
	/// The integer numeric types SHORT and USHORT are returned as short and ushort.
	/// The integer numeric types LONG and ULONG are returned as int and uint.
	/// All logical values (Yes/No, True/false, On/Off) are returned as "bool",
	/// where "true" implies "yes", "true" or "on".
	/// Char type is returned as "sbyte".
	/// Uchar type is returned as "byte".
	/// String types WCHAR_STRING and CHAR_STRING types are returned as "string".
	/// </c>
	/// </summary>
	/// <param name="scanNumber">scan who's data is needed</param>
	/// <param name="field">zero based field number in the record, as per header </param>
	/// <returns>Value of requested field</returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	object GetTrailerExtraValue(int scanNumber, int field);

	/// <summary>
	/// Returns the (unformatted) Trailer Extra values for all  field in the specified scan number. 
	/// The object types depend on the field types, as returned by 
	/// GetTrailerExtraHeaderInformation.
	/// Uses for this include efficient copy of data from one file to another, as
	/// it eliminates translation of numeric data to and from strings.
	/// <c>
	/// Numeric values (where the header for this field returns "True" for IsNumeric)
	/// can always be cast up to double.
	/// The integer numeric types SHORT and USHORT are returned as short and ushort.
	/// The integer numeric types LONG and ULONG are returned as int and uint.
	/// All logical values (Yes/No, True/false, On/Off) are returned as "bool",
	/// where "true" implies "yes", "true" or "on".
	/// Char type is returned as "sbyte".
	/// Uchar type is returned as "byte".
	/// String types WCHAR_STRING and CHAR_STRING types are returned as "string".
	/// </c>
	/// </summary>
	/// <param name="scanNumber">scan who's data is needed</param>
	/// <returns>Values of all fields</returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	object[] GetTrailerExtraValues(int scanNumber);

	/// <summary>
	/// Returns the (unformatted) Tune Data values for all fields in the specified record number. 
	/// The object types depend on the field types, as returned by 
	/// GetTuneDataHeaderInformation.
	/// Uses for this include efficient copy of data from one file to another, as
	/// it eliminates translation of numeric data to and from strings.
	/// <c>
	/// Numeric values (where the header for this field returns "True" for IsNumeric)
	/// can always be cast up to double.
	/// The integer numeric types SHORT and USHORT are returned as short and ushort.
	/// The integer numeric types LONG and ULONG are returned as int and uint.
	/// All logical values (Yes/No, True/false, On/Off) are returned as "bool",
	/// where "true" implies "yes", "true" or "on".
	/// Char type is returned as "sbyte".
	/// Uchar type is returned as "byte".
	/// String types WCHAR_STRING and CHAR_STRING types are returned as "string".
	/// </c>
	/// </summary>
	/// <param name="index">zero based index into tune records who's data is needed</param>
	/// <returns>Values of all fields</returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	object[] GetTuneDataValues(int index);

	/// <summary>
	/// Gets the (raw) status log data at a given index in the log.
	/// Deigned for efficiency, this method does not convert logs to display string format.
	/// </summary>
	/// <param name="index">Index (from 0 to "RunHeaderEx.StatusLogCount -1 ))</param>
	/// <returns>Log data at the given index</returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	IStatusLogEntry GetStatusLogEntry(int index);

	/// <summary>
	/// Gets the (raw) status log data at a given retention time in the log.
	/// Designed for efficiency, this method does not convert logs to display string format.
	/// </summary>
	/// <param name="retentionTime">Retention time/</param>
	/// <returns>Log data at the given time</returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	IStatusLogEntry GetStatusLogEntry(double retentionTime);

	/// <summary>
	/// Get the advanced LT/FT formats data, such as the noise data, baseline data, label peaks and frequencies
	/// Common uses include:
	/// An application exporting mass spectra to a new raw file.
	/// Special calculations on scans (including averaging, recalibration etc.)
	/// </summary>
	/// <param name="scanNumber">The scan number.</param>
	/// <returns>
	/// Returns an IAdvancedPacketData object which contains noise data, baseline data, label peaks and frequencies for specified <paramref name="scanNumber" />.
	/// It might return empty arrays for scans which do not have these data.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	IAdvancedPacketData GetAdvancedPacketData(int scanNumber);

	/// <summary>
	/// Gets the scan index for MS
	/// </summary>
	/// <param name="scan">scan number</param>
	/// <returns>index for MS scan</returns>
	IMsScanIndexAccess GetMsScanIndex(int scan)
	{
		return GetScanStatsForScanNumber(scan);
	}

	/// <summary>
	/// Calculate the filters for this raw file, and return as an array.
	/// </summary>
	/// <param name="mode">Optional: Determine how precursor tolerance is handled.
	/// Default: Use value provided by instrument in run header</param>
	/// <param name="decimalPlaces">Optional: When a specified tolerance is specified, then a number of matched decimal places 
	/// can be specified (default 2)</param>
	/// <returns>
	/// Auto generated list of unique filters
	/// </returns>
	ReadOnlyCollection<IScanFilter> GetAccurateFilters(FilterPrecisionMode mode = FilterPrecisionMode.Instrument, int decimalPlaces = 2)
	{
		return GetAccurateFiltersForScanRange(-1, -1, mode, decimalPlaces);
	}

	/// <summary>
	/// Calculate the filters for this raw file within the range of scans supplied, and return as an array.
	/// </summary>
	/// <param name="startScan">First scan to analyze. "-1" will start from "the first scan"</param>
	/// <param name="endScan">Last scan to analyze. "-1" will end at "the last scan"</param>
	/// <param name="mode">Optional: Determine how precursor tolerance is handled.
	/// Default: Use value provided by instrument in run header</param>
	/// <param name="decimalPlaces">Optional: When a specified tolerance is specified, then a number of matched decimal places 
	/// can be specified (default 2)</param>
	/// <returns>
	/// Auto generated list of unique filters
	/// </returns>
	ReadOnlyCollection<IScanFilter> GetAccurateFiltersForScanRange(int startScan, int endScan, FilterPrecisionMode mode = FilterPrecisionMode.Instrument, int decimalPlaces = 2);

	/// <summary>
	/// Gets MS scan index and scan event data in 1 call.
	/// A "scan event" for a scan requires the system to read the "scan index"
	/// Calling code needing details such as "RT of scan" would need to separately request
	/// that index. This leads to inefficacy (especially with a network proxy).
	/// </summary>
	/// <param name="scan">scan number</param>
	/// <returns>Access to a scan's index and scan event</returns>
	IIndexAndEvent ReadEventAndIndex(int scan)
	{
		return new IndexAndEvent
		{
			ScanIndex = GetMsScanIndex(scan),
			ScanEvent = GetScanEventForScanNumber(scan)
		};
	}

	/// <summary>
	/// Gets multiple MS scan index and scan event data in 1 call.
	/// A "scan event" for a scan requires the system to read the "scan index"
	/// Calling code needing details such as "RT of scan" would need to separately request
	/// that index. This leads to inefficacy (especially with a network proxy).
	/// </summary>
	/// <param name="scans">Array of scan numbers.</param>
	/// <returns>Access to a scan's index and scan event.</returns>
	IIndexAndEvent[] ReadEventAndIndex(int[] scans)
	{
		IIndexAndEvent[] array = new IIndexAndEvent[scans.Length];
		for (int i = 0; i < scans.Length; i++)
		{
			array[i] = new IndexAndEvent
			{
				ScanIndex = GetMsScanIndex(scans[i]),
				ScanEvent = GetScanEventForScanNumber(scans[i])
			};
		}
		return array;
	}

	/// <summary>Gets the trailer scan event indices information.</summary>
	/// <returns>
	///   List of trailer scan event indices information
	/// </returns>
	List<object> GetTrailerScanEventIndicesInfo()
	{
		throw new InvalidOperationException("Instrument must be MS for this operation");
	}
}
