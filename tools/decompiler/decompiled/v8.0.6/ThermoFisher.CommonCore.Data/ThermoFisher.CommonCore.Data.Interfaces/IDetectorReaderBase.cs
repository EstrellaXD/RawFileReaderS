using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Methods which act on a selected instrument
/// </summary>
public interface IDetectorReaderBase
{
	/// <summary>
	/// Gets the definition of the selected instrument.
	/// </summary>
	/// <returns>data about the selected instrument, for example the instrument name</returns>
	InstrumentData GetInstrumentData();

	/// <summary>
	/// Get the nearest scan number to a retention time
	/// </summary>
	/// <param name="time">
	/// Retention time (minutes)
	/// </param>
	/// <returns>
	/// Scan number in the selected instrument which is closest to this time.
	/// If there are no scans, -1 is returned.
	/// </returns>
	int ScanNumberFromRetentionTime(double time);

	/// <summary>
	/// Get the nearest scan statistic to a retention time
	/// </summary>
	/// <param name="time">
	/// Retention time (minutes)
	/// </param>
	/// <returns>
	/// Scan statistic in the selected instrument which is closest to this time.
	/// </returns>
	ScanStatistics GetScanStatsForRetentionTime(double time)
	{
		int scanNumber = ScanNumberFromRetentionTime(time);
		return GetScanStatsForScanNumber(scanNumber);
	}

	/// <summary>
	/// Get the scan statistics for a scan.
	/// For example: The retention time of the scan.
	/// </summary>
	/// <param name="scanNumber">
	/// scan number
	/// </param>
	/// <returns>
	/// Statistics for scan
	/// </returns>
	ScanStatistics GetScanStatsForScanNumber(int scanNumber);

	/// <summary>
	/// Gets scan data for the given scan number. It will also fill <paramref name="stats" /> object, if any supplied.
	/// For  most detector types, this is the only data for the scan, and contains either
	/// profile or centroid information (depending on the type of scan performed).
	/// For <c>Orbitrap</c> data (FT packet formats), this returns the first set of data for the scan (typically profile).
	/// The second set of data (centroids) are available from the GetCentroidStream method.
	/// The "Segmented" format is used for SIM and SRM modes, where there may be multiple
	/// mass ranges (segments) of a scan.
	/// Full scan data has only one segment.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="stats">
	/// statistics for the scan
	/// </param>
	/// <returns>
	/// The segmented scan
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	SegmentedScan GetSegmentedScanFromScanNumber(int scanNumber, ScanStatistics stats);

	/// <summary>
	/// Gets scan data for the given scan number.
	/// For  most detector types, this is the only data for the scan, and contains either
	/// profile or centroid information (depending on the type of scan performed).
	/// For <c>Orbitrap</c> data (FT packet formats), this returns the first set of data for the scan (typically profile).
	/// The second set of data (centroids) are available from the GetCentroidStream method.
	/// The "Segmented" format is used for SIM and SRM modes, where there may be multiple
	/// mass ranges (segments) of a scan.
	/// Full scan data has only one segment.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The segmented scan
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	SegmentedScan GetSegmentedScanFromScanNumber(int scanNumber)
	{
		return GetSegmentedScanFromScanNumber(scanNumber, null);
	}

	/// <summary>
	/// Gets scan data for the given scan number and scan stats
	/// For  most detector types, this is the only data for the scan, and contains either
	/// profile or centroid information (depending on the type of scan performed).
	/// For <c>Orbitrap</c> data (FT packet formats), this returns the first set of data for the scan (typically profile).
	/// The second set of data (centroids) are available from the GetCentroidStream method.
	/// The "Segmented" format is used for SIM and SRM modes, where there may be multiple
	/// mass ranges (segments) of a scan.
	/// Full scan data has only one segment.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="stats">
	/// statistics for the scan
	/// </param>
	/// <returns>
	/// The segmented scan
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	SegmentedScan GetSegmentedScanFromScanNumberWithStats(int scanNumber, out ScanStatistics stats)
	{
		stats = new ScanStatistics();
		return GetSegmentedScanFromScanNumber(scanNumber, stats);
	}

	/// <summary>
	/// Test if a scan is centroid format
	/// </summary>
	/// <param name="scanNumber">
	/// Number of the scan
	/// </param>
	/// <returns>
	/// True if the scan is centroid format
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	bool IsCentroidScanFromScanNumber(int scanNumber);

	/// <summary>
	/// Gets the filter strings for this file.
	/// This analyses all scans types in the file.
	/// It may take some time, especially with data dependent files.
	/// Filters are grouped, within tolerance (as defined by the MS detector).
	/// </summary>
	/// <returns>A string for each auto filter from the raw file</returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if this is called without first selecting an MS detector</exception>
	string[] GetAutoFilters();

	/// <summary>
	/// Get a string representing the scan type (for filtering). 
	/// For more complete tests on filters, consider using the IScanFilter interface.
	/// If reading data using IRawDataPlus, you may use <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.GetFilterForScanNumber(System.Int32)" />
	/// A filter string (possibly entered from the UI) may be parsed using <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.GetFilterFromString(System.String)" /> 
	/// If the RT is known, and not the scan number, use ScanNumberFromRetentionTime
	/// to convert the time to a scan number.
	/// </summary>
	/// <param name="scanNumber">
	/// Scan number whose type is needed
	/// </param>
	/// <returns>
	/// Type of scan, in string format.
	/// To compare individual filter fields, the ScanDefinition class can be used.
	/// </returns>
	/// <seealso cref="T:ThermoFisher.CommonCore.Data.Interfaces.IScanFilter" />
	/// <seealso cref="M:ThermoFisher.CommonCore.Data.Interfaces.IDetectorReaderPlus.GetFilterForScanNumber(System.Int32)" />
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	string GetScanType(int scanNumber);

	/// <summary>
	/// Get the retention time (minutes) from a scan number
	/// </summary>
	/// <param name="scanNumber">
	/// Scan number
	/// </param>
	/// <returns>
	/// Retention time (start time) of scan
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	double RetentionTimeFromScanNumber(int scanNumber);

	/// <summary>
	/// Get the centroids saved with a profile scan.
	/// This is only valid for data types which support
	/// multiple sets of data per scan (such as <c>Orbitrap</c> data).
	/// This method does not "Centroid profile data".
	/// </summary>
	/// <param name="scanNumber">
	/// Scan number
	/// </param>
	/// <param name="includeReferenceAndExceptionPeaks">
	/// determines if peaks flagged as ref should be returned
	/// </param>
	/// <returns>
	/// centroid stream for specified <paramref name="scanNumber" />.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	CentroidStream GetCentroidStream(int scanNumber, bool includeReferenceAndExceptionPeaks);

	/// <summary>
	/// Gets the status log nearest to a retention time.
	/// </summary>
	/// <param name="retentionTime">
	/// The retention time.
	/// </param>
	/// <returns>
	/// <see cref="T:ThermoFisher.CommonCore.Data.Business.LogEntry" /> object containing status log information.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	ILogEntryAccess GetStatusLogForRetentionTime(double retentionTime);

	/// <summary>
	/// returns the number of entries in the current instrument's status log
	/// </summary>
	/// <returns>
	/// The number of available status log entries.
	/// </returns>
	int GetStatusLogEntriesCount();

	/// <summary>
	/// Returns the header information for the current instrument's status log.
	/// This defines the format of the log entries.
	/// </summary>
	/// <returns>
	/// The headers (list of prefixes for the strings).
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	HeaderItem[] GetStatusLogHeaderInformation();

	/// <summary>
	/// Returns the Status log values for the current instrument,
	/// for the given status record.
	/// This is most likely for diagnostics or archiving.
	/// Applications which need logged data near a scan should use “GetStatusLogForRetentionTime”.
	/// Note that this does not return the “labels” for the fields.
	/// </summary>
	/// <param name="statusLogIndex">Index into table of status logs</param>
	/// <param name="ifFormatted">true if they should be formatted as per the 
	/// data definition for this field (recommended for display).
	/// Unformatted values may be returned with default precision (for float or double)
	/// Which may be better for graphing or archiving</param>
	/// <returns>
	/// The status log values.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown if no device has been selected</exception>
	StatusLogValues GetStatusLogValues(int statusLogIndex, bool ifFormatted);

	/// <summary>
	/// Gets a text form of the instrument tuning method, at a given index.
	/// The number of available tune methods can be obtained from GetTuneDataCount.
	/// </summary>
	/// <param name="tuneDataIndex">
	/// tune data index
	/// </param>
	/// <returns>
	/// <see cref="T:ThermoFisher.CommonCore.Data.Business.LogEntry" /> object containing tune data for specified <paramref name="tuneDataIndex" />.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	ILogEntryAccess GetTuneData(int tuneDataIndex);

	/// <summary>
	/// Return the number of tune data entries.
	/// Each entry describes MS tuning conditions, used to acquire this file.
	/// </summary>
	/// <returns>
	/// The number of tune methods saved in the raw file&gt;.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	int GetTuneDataCount();

	/// <summary>
	/// Return the header information for the current instrument's tune data.
	/// This defines the fields used for a record which defines how the instrument was tuned.
	/// This method only applies to MS detectors. These items can be paired with the "TuneDataValues"
	/// to correctly display each tune record in the file.
	/// </summary>
	/// <returns>
	/// The headers/&gt;.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	HeaderItem[] GetTuneDataHeaderInformation();

	/// <summary>
	/// Return tune data values for the specified index.
	/// This method only applies to MS detectors.
	/// This contains only the data values, and not the headers.
	/// </summary>
	/// <param name="tuneDataIndex">
	/// index into tune tables
	/// </param>
	/// <param name="ifFormatted">
	/// true if formatting should be done.
	/// Normally you would set “ifFormatted” to true,
	/// to format based on the precision defined in the header.
	/// Setting this to false uses default number formatting.
	/// This may be better for diagnostic charting,
	/// as numbers may have higher precision than the default format.
	/// </param>
	/// <returns>
	/// The tune data
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	TuneDataValues GetTuneDataValues(int tuneDataIndex, bool ifFormatted);

	/// <summary>
	/// Create a chromatogram from the data stream
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
	IChromatogramData GetChromatogramData(IChromatogramSettings[] settings, int startScan, int endScan);

	/// <summary>
	/// Create a chromatogram from the data stream
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
	/// if the ranges have equal low and high mass values (within 1.0E-6),
	/// then <paramref name="toleranceOptions" /> are used to determine a band
	/// subtracted from low and added to high to search for matching masses.
	/// if this is set to "null" then the tolerance is defaulted to +/- 0.5.
	/// </param>
	/// <returns>
	/// Chromatogram points
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.RequiresChromatographicDeviceException">
	/// Thrown if the selected device is of a type that does not support chromatogram generation</exception>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.InvalidFilterFormatException">
	/// Thrown if filters are sent (for MS chromatograms) which cannot be parsed</exception>
	IChromatogramData GetChromatogramData(IChromatogramSettings[] settings, int startScan, int endScan, MassOptions toleranceOptions);

	/// <summary>
	/// Gets the trailer extra header information. This is common across all scan numbers.
	/// This defines the format of additional data logged by an MS detector, at each scan.
	/// For example, a particular detector may wish to record "analyzer 3 temperature" at each scan,
	/// for diagnostic purposes. Since this is not a defined field in "ScanHeader" it would be created
	/// as a custom "trailer" field for a given instrument. The field definitions occur only once,
	/// and apply to all trailer extra records in the file. In the example given,
	/// only the numeric value of "analyzer 3 temperature" would be logged with each scan,
	/// without repeating the label.
	/// </summary>
	/// <returns>
	/// The headers defining the "trailer extra" record format.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	HeaderItem[] GetTrailerExtraHeaderInformation();

	/// <summary>
	/// Gets the Trailer Extra values for the specified scan number. 
	/// If <paramref name="ifFormatted" /> = true, then the values will be formatted as per the header settings.
	/// </summary>
	/// <param name="scanNumber">
	/// scan whose trailer data is needed
	/// </param>
	/// <param name="ifFormatted">
	/// true if the data should be formatted
	/// </param>
	/// <returns>
	/// The strings representing trailer data.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	string[] GetTrailerExtraValues(int scanNumber, bool ifFormatted);

	/// <summary>
	/// Gets the array of headers and values for this scan number.
	/// The values are formatted as per the header settings.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan for which this information is needed
	/// </param>
	/// <returns>
	/// Extra information about the scan
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	ILogEntryAccess GetTrailerExtraInformation(int scanNumber);

	/// <summary>
	/// Gets the segment event table for the current instrument.
	/// This table indicates planned scan types for the MS detector.
	/// It is usually created from an instrument method, by the detector.
	/// With data dependent or custom scan types, this will not be a complete
	/// list of scan types used within the file.
	/// If this object implements the derived IRawDataPlus interface, then
	/// This same data can be obtained in object format (instead of string) with the IRawDataPlus
	/// property "ScanEvents"
	/// </summary>
	/// <returns>A two dimensional array of events. The first index is segment index (segment number-1).
	/// The second is event index (event number -1) within the segment.</returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	string[][] GetSegmentEventTable();
}
