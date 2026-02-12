using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Class to create accessors for raw a raw file, for multiple threads.
/// </summary>
public class ThreadSafeRawFileAccess : IRawCache, IRawFileThreadAccessor
{
	/// <summary>
	/// The raw file thread accessor.
	/// This class is a private thread safe wrapper.
	/// It works by using the "parent" object as a lock object for all threads.
	/// In addition, the parent manages all state of the underlying wrapped interface.
	/// This object also wraps enumerators and iterators fully, to protect against detector changes
	/// from other threads.
	/// </summary>
	private class RawFileThreadAccessor : IRawDataExtended, IRawDataPlus, IRawData, IDetectorReaderBase, IRawDataProperties, IDisposable, IRawCache, ISimplifiedScanReader, IDetectorReaderPlus, IRawDataExtensions
	{
		/// <summary>
		/// The filtered scan iterator wrapper.
		/// must wrap this, to set device correctly each call
		/// </summary>
		private class FilteredScanIteratorWrapper : IFilteredScanIterator
		{
			private readonly RawFileThreadAccessor _owner;

			private readonly IFilteredScanIterator _iterator;

			private readonly ThreadSafeRawFileAccess _lockObject;

			/// <summary>
			/// Gets the filter which was used to construct this
			/// </summary>
			public string Filter
			{
				get
				{
					lock (_lockObject)
					{
						_owner.SetDevice();
						return _iterator.Filter;
					}
				}
			}

			/// <summary>
			/// Gets the previous scan number, which matches the filter.
			/// Returns 0 if there is no open file.
			/// </summary>
			public int PreviousScan
			{
				get
				{
					lock (_lockObject)
					{
						_owner.SetDevice();
						return _iterator.PreviousScan;
					}
				}
			}

			/// <summary>
			/// Gets the next scan number, which matches the filter.
			/// Returns 0 if there is no open file.
			/// </summary>
			public int NextScan
			{
				get
				{
					lock (_lockObject)
					{
						_owner.SetDevice();
						return _iterator.NextScan;
					}
				}
			}

			/// <summary>
			/// Sets the iterator's position.
			/// This scan number does not have to match the given filter.
			/// This can be used to find next or previous matching scan, from a given scan.
			/// </summary>
			public int SpectrumPosition
			{
				set
				{
					lock (_lockObject)
					{
						_owner.SetDevice();
						_iterator.SpectrumPosition = value;
					}
				}
			}

			/// <summary>
			/// Gets a value indicating whether there are possible previous scans before the current scan.
			/// This does not guarantee that another matching scan exists. It simply tests that the current iterator position
			/// is not the first scan in the file.
			/// </summary>
			public bool MayHavePrevious
			{
				get
				{
					lock (_lockObject)
					{
						_owner.SetDevice();
						return _iterator.MayHavePrevious;
					}
				}
			}

			/// <summary>
			/// Gets a value indicating whether there are possible next scans after the current scan.
			/// This does not guarantee that another matching scan exists. It simply tests that the current iterator position
			/// is not the last scan in the file.
			/// </summary>
			public bool MayHaveNext
			{
				get
				{
					lock (_lockObject)
					{
						_owner.SetDevice();
						return _iterator.MayHavePrevious;
					}
				}
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.RawFileThreadAccessor.FilteredScanIteratorWrapper" /> class.
			/// </summary>
			/// <param name="rawFileThreadAccessor">
			/// The raw file thread accessor.
			/// </param>
			/// <param name="filter">
			/// The filter.
			/// </param>
			public FilteredScanIteratorWrapper(RawFileThreadAccessor rawFileThreadAccessor, IScanFilter filter)
			{
				_owner = rawFileThreadAccessor;
				_lockObject = _owner._parent;
				_owner.SetDevice();
				_iterator = _owner._rawFile.GetFilteredScanIterator(filter);
			}
		}

		private readonly IRawDataPlus _rawFile;

		private readonly IRawDataExtended _rawFileExtended;

		private readonly ThreadSafeRawFileAccess _parent;

		private Device _instrumentType = Device.Other;

		private int _instrumentIndex = -1;

		private bool _includeReferenceAndExceptionData;

		/// <inheritdoc />
		public IConfiguredDetector ConfiguredDetector
		{
			get
			{
				lock (_parent)
				{
					SetDevice();
					return _rawFile.ConfiguredDetector;
				}
			}
		}

		/// <summary>
		/// Gets the current instrument's run header
		/// </summary>
		public IRunHeaderAccess RunHeader
		{
			get
			{
				lock (_parent)
				{
					SetDevice();
					return _rawFile.RunHeader;
				}
			}
		}

		/// <summary>
		/// Gets the number of instrument methods in this file.
		/// </summary>
		public int InstrumentMethodsCount
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.InstrumentMethodsCount;
				}
			}
		}

		/// <summary>
		/// Gets the name of the computer, used to create this file.
		/// </summary>
		public string ComputerName
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.ComputerName;
				}
			}
		}

		/// <summary>
		/// Gets the path to original data.
		/// </summary>
		public string Path
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.Path;
				}
			}
		}

		/// <summary>
		/// Gets the name of acquired file (excluding path).
		/// </summary>
		public string FileName
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.FileName;
				}
			}
		}

		/// <summary>
		/// Gets the instrument as last set by a call to <see cref="M:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.RawFileThreadAccessor.SelectInstrument(ThermoFisher.CommonCore.Data.Business.Device,System.Int32)" />.
		/// If this has never been set, returns null.
		/// </summary>
		public InstrumentSelection SelectedInstrument
		{
			get
			{
				if (_instrumentIndex <= 0)
				{
					return null;
				}
				return new InstrumentSelection(_instrumentIndex, _instrumentType);
			}
		}

		/// <summary>
		/// Gets the date when this data was created.
		/// </summary>
		public DateTime CreationDate
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.CreationDate;
				}
			}
		}

		/// <summary>
		/// Gets the name of person creating data.
		/// </summary>
		public string CreatorId
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.CreatorId;
				}
			}
		}

		/// <summary>
		/// Gets various details about the sample (such as comments).
		/// </summary>
		public SampleInformation SampleInformation
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.SampleInformation;
				}
			}
		}

		/// <summary>
		/// Gets the number of instruments (data streams) in this file.
		/// </summary>
		public int InstrumentCount
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.InstrumentCount;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the last file operation caused an error.
		/// </summary>
		public bool IsError
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.IsError;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the file is being acquired (not complete).
		/// </summary>
		public bool InAcquisition
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.InAcquisition;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the data file was successfully opened
		/// </summary>
		public bool IsOpen
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.IsOpen;
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether reference and exception peaks should be returned (by default they are not)
		/// </summary>
		public bool IncludeReferenceAndExceptionData
		{
			get
			{
				return _includeReferenceAndExceptionData;
			}
			set
			{
				_includeReferenceAndExceptionData = value;
			}
		}

		/// <summary>
		/// Gets the raw file header.
		/// </summary>
		public IFileHeader FileHeader
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.FileHeader;
				}
			}
		}

		/// <summary>
		/// Gets the file error state.
		/// </summary>
		public IFileError FileError
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.FileError;
				}
			}
		}

		/// <summary>
		/// Gets extended the run header details.
		/// </summary>
		public IRunHeader RunHeaderEx
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.RunHeaderEx;
				}
			}
		}

		/// <summary>
		/// Gets the auto sampler information.
		/// </summary>
		public IAutoSamplerInformation AutoSamplerInformation
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.AutoSamplerInformation;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this file has an instrument method.
		/// </summary>
		public bool HasInstrumentMethod
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.HasInstrumentMethod;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this file has MS data.
		/// </summary>
		public bool HasMsData
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.HasMsData;
				}
			}
		}

		/// <summary>
		/// Gets the scan events.
		/// </summary>
		public IScanEvents ScanEvents
		{
			get
			{
				lock (_parent)
				{
					SetDevice();
					return _rawFile.ScanEvents;
				}
			}
		}

		/// <summary>
		/// Gets the set of user labels
		/// </summary>
		public string[] UserLabel
		{
			get
			{
				lock (_parent)
				{
					return _rawFile.UserLabel;
				}
			}
		}

		/// <summary>
		/// Gets the labels and index positions of the status log items which may be plotted.
		/// That is, the numeric items.
		/// Labels names are returned by "Key" and the index into the log is "Value".
		/// </summary>
		public KeyValuePair<string, int>[] StatusLogPlottableData
		{
			get
			{
				lock (_parent)
				{
					SetDevice();
					return _rawFile.StatusLogPlottableData;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.RawFileThreadAccessor" /> class.
		/// </summary>
		/// <param name="threadSafeRawFileAccess">
		/// The thread safe raw file access.
		/// </param>
		public RawFileThreadAccessor(ThreadSafeRawFileAccess threadSafeRawFileAccess)
		{
			_parent = threadSafeRawFileAccess;
			_rawFile = threadSafeRawFileAccess._rawFile;
			_rawFileExtended = threadSafeRawFileAccess._rawFileExtended;
			lock (_parent)
			{
				GetDevice();
			}
		}

		/// <summary>
		/// Capture that the parent object's selected device
		/// </summary>
		private void GetDevice()
		{
			InstrumentSelection selectedInstrument = _rawFile.SelectedInstrument;
			_instrumentIndex = selectedInstrument.InstrumentIndex;
			_instrumentType = selectedInstrument.DeviceType;
			_includeReferenceAndExceptionData = _rawFile.IncludeReferenceAndExceptionData;
		}

		/// <summary>
		/// Ensure that the parent object has the expected device selected
		/// </summary>
		private void SetDevice()
		{
			_parent.SelectInstrument(_instrumentType, _instrumentIndex);
			_parent.IncludeReferenceAndExceptionData(_includeReferenceAndExceptionData);
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <filterpriority>2</filterpriority>
		public void Dispose()
		{
		}

		/// <summary>
		/// Get a scan number from a retention time
		/// </summary>
		/// <param name="time">
		/// Retention time (minutes)
		/// </param>
		/// <returns>
		/// Scan number in the data stream for this time
		/// </returns>
		public int ScanNumberFromRetentionTime(double time)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.ScanNumberFromRetentionTime(time);
			}
		}

		/// <summary>
		/// Gets additional (binary) data from a scan.
		/// The format of this data is custom (per instrument) and can be decoded into
		/// objects by a specific decoder for the detector type.
		/// </summary>
		/// <param name="scan">Scan whose data is needed</param>
		public byte[] GetAdditionalScanData(int scan)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFileExtended?.GetAdditionalScanData(scan);
			}
		}

		/// <inheritdoc />
		public IExtendedScanData GetExtendedScanData(int scan)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFileExtended?.GetExtendedScanData(scan);
			}
		}

		/// <summary>
		/// Get the scan statistics for a scan.
		/// </summary>
		/// <param name="scanNumber">
		/// scan number
		/// </param>
		/// <returns>
		/// Statistics for scan
		/// </returns>
		public ScanStatistics GetScanStatsForScanNumber(int scanNumber)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetScanStatsForScanNumber(scanNumber);
			}
		}

		/// <summary>
		/// Gets the segmented scan from scan number. It will also fill <paramref name="stats" /> object, if any supplied. 
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
		public SegmentedScan GetSegmentedScanFromScanNumber(int scanNumber, ScanStatistics stats)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetSegmentedScanFromScanNumber(scanNumber, stats);
			}
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
		public bool IsCentroidScanFromScanNumber(int scanNumber)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.IsCentroidScanFromScanNumber(scanNumber);
			}
		}

		/// <summary>
		/// Gets the filter strings for this file.
		/// </summary>
		/// <returns>A string for each auto filter from the raw file</returns>
		public string[] GetAutoFilters()
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetAutoFilters();
			}
		}

		/// <summary>
		/// Gets an instrument method.
		/// </summary>
		/// <param name="index">
		/// The index.
		/// </param>
		/// <returns>
		/// A text version of the method
		/// </returns>
		public string GetInstrumentMethod(int index)
		{
			lock (_parent)
			{
				return _rawFile.GetInstrumentMethod(index);
			}
		}

		/// <summary>
		/// Gets the definition of the selected instrument.
		/// </summary>
		/// <returns>data about the selected instrument, for example the instrument name</returns>
		public InstrumentData GetInstrumentData()
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetInstrumentData();
			}
		}

		/// <summary>
		/// Get a string representing the scan type (for filtering). 
		/// For more complete tests on filters, the returned string
		/// can be converted to a ScanDefinition,
		/// by using the static constructor ScanDefinition.FromString(string scanType).
		/// If the RT is known, and not the scan number, use ScanNumberFromRetentionTime
		/// to convert the time to a scan number.
		/// Example:
		/// ScanDefinition definition=ScanDefinition.FromString(GetScanType(ScanNumberFromRetentionTime(time));
		/// </summary>
		/// <param name="scanNumber">
		/// Scan number whose type is needed
		/// </param>
		/// <returns>
		/// Type of scan, in string format.
		/// To compare individual filter fields, the ScanDefinition class can be used.
		/// </returns>
		/// <see>ScanDefinition</see>
		public string GetScanType(int scanNumber)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetScanType(scanNumber);
			}
		}

		/// <summary>
		/// Get the retention time (minutes) from a scan number
		/// </summary>
		/// <param name="scanNumber">
		/// Scan number
		/// </param>
		/// <returns>
		/// Retention time (start time) of scan
		/// </returns>
		public double RetentionTimeFromScanNumber(int scanNumber)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.RetentionTimeFromScanNumber(scanNumber);
			}
		}

		/// <summary>
		/// Get the centroids saved with a profile scan
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
		public CentroidStream GetCentroidStream(int scanNumber, bool includeReferenceAndExceptionPeaks)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetCentroidStream(scanNumber, includeReferenceAndExceptionPeaks);
			}
		}

		/// <summary>
		/// Gets the status log for retention time.
		/// </summary>
		/// <param name="retentionTime">
		/// The retention time.
		/// </param>
		/// <returns>
		/// <see cref="T:ThermoFisher.CommonCore.Data.Business.LogEntry" /> object containing status log information.
		/// </returns>
		public ILogEntryAccess GetStatusLogForRetentionTime(double retentionTime)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetStatusLogForRetentionTime(retentionTime);
			}
		}

		/// <summary>
		/// returns the number of entries n the current instrument's status log
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Int32" />.
		/// </returns>
		public int GetStatusLogEntriesCount()
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetStatusLogEntriesCount();
			}
		}

		/// <summary>
		/// Returns the header information for the current instrument's status log
		/// </summary>
		/// <returns>
		/// The headers (list of prefixes for the strings).
		/// </returns>
		public HeaderItem[] GetStatusLogHeaderInformation()
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetStatusLogHeaderInformation();
			}
		}

		/// <summary>
		/// Returns the Status log values for the current instrument
		/// </summary>
		/// <param name="statusLogIndex">
		/// Index into table of status logs
		/// </param>
		/// <param name="ifFormatted">
		/// true if they should be formatted (recommended)
		/// </param>
		/// <returns>
		/// The status log values.
		/// </returns>
		public StatusLogValues GetStatusLogValues(int statusLogIndex, bool ifFormatted)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetStatusLogValues(statusLogIndex, ifFormatted);
			}
		}

		/// <summary>
		/// Gets the tune data.
		/// </summary>
		/// <param name="tuneDataIndex">
		/// tune data index
		/// </param>
		/// <returns>
		/// <see cref="T:ThermoFisher.CommonCore.Data.Business.LogEntry" /> object containing tune data for specified <paramref name="tuneDataIndex" />.
		/// </returns>
		public ILogEntryAccess GetTuneData(int tuneDataIndex)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetTuneData(tuneDataIndex);
			}
		}

		/// <summary>
		/// return the number of tune data entries
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Int32" />.
		/// </returns>
		public int GetTuneDataCount()
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetTuneDataCount();
			}
		}

		/// <summary>
		/// Return the header information for the current instrument's tune data
		/// </summary>
		/// <returns>
		/// The headers/&gt;.
		/// </returns>
		public HeaderItem[] GetTuneDataHeaderInformation()
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetTuneDataHeaderInformation();
			}
		}

		/// <summary>
		/// return tune data values for the specified index
		/// </summary>
		/// <param name="tuneDataIndex">
		/// index into tune tables
		/// </param>
		/// <param name="ifFormatted">
		/// true if formatting should be done
		/// </param>
		/// <returns>
		/// The tune data&gt;.
		/// </returns>
		public TuneDataValues GetTuneDataValues(int tuneDataIndex, bool ifFormatted)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetTuneDataValues(tuneDataIndex, ifFormatted);
			}
		}

		/// <summary>
		/// Choose the data stream from the data source.
		/// </summary>
		/// <param name="instrumentType">
		/// Type of instrument
		/// </param>
		/// <param name="instrumentIndex">
		/// Stream number
		/// </param>
		public void SelectInstrument(Device instrumentType, int instrumentIndex)
		{
			lock (_parent)
			{
				_instrumentIndex = instrumentIndex;
				_instrumentType = instrumentType;
				_rawFile.SelectInstrument(instrumentType, instrumentIndex);
			}
		}

		/// <summary>
		/// get the number of instruments (data streams) of a certain classification.
		/// For example: the number of UV devices which logged data into this file.
		/// </summary>
		/// <param name="type">
		/// The device type to count
		/// </param>
		/// <returns>
		/// The number of devices of this type
		/// </returns>
		public int GetInstrumentCountOfType(Device type)
		{
			lock (_parent)
			{
				return _rawFile.GetInstrumentCountOfType(type);
			}
		}

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
		public IChromatogramData GetChromatogramData(IChromatogramSettings[] settings, int startScan, int endScan)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetChromatogramData(settings, startScan, endScan);
			}
		}

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
		/// if the ranges have equal mass values,
		/// then <paramref name="toleranceOptions" /> are used to determine a band
		/// subtracted from low and added to high to search for matching masses
		/// </param>
		/// <returns>
		/// Chromatogram points
		/// </returns>
		public IChromatogramData GetChromatogramData(IChromatogramSettings[] settings, int startScan, int endScan, MassOptions toleranceOptions)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetChromatogramData(settings, startScan, endScan, toleranceOptions);
			}
		}

		/// <summary>
		/// The the device type for an instrument data stream
		/// </summary>
		/// <param name="index">
		/// The data stream
		/// </param>
		/// <returns>
		/// The device at type the index
		/// </returns>
		public Device GetInstrumentType(int index)
		{
			lock (_parent)
			{
				return _rawFile.GetInstrumentType(index);
			}
		}

		/// <summary>
		/// Gets names of all instruments stored in the raw file's copy of the instrument method file.
		/// </summary>
		/// <returns>
		/// The instrument names.
		/// </returns>
		public string[] GetAllInstrumentNamesFromInstrumentMethod()
		{
			lock (_parent)
			{
				return _rawFile.GetAllInstrumentNamesFromInstrumentMethod();
			}
		}

		/// <summary>
		/// Re-read the current file, to get the latest data.
		/// Only meaningful if the object has an implied backing file (such as IO.DLL and .raw files)
		/// No-op otherwise
		/// </summary>
		/// <returns>
		/// The <see cref="T:System.Boolean" />.
		/// </returns>
		public bool RefreshViewOfFile()
		{
			lock (_parent)
			{
				return _rawFile.RefreshViewOfFile();
			}
		}

		/// <summary>
		/// Gets the trailer extra header information. This is common across all scan numbers
		/// </summary>
		/// <returns>
		/// The headers.
		/// </returns>
		public HeaderItem[] GetTrailerExtraHeaderInformation()
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetTrailerExtraHeaderInformation();
			}
		}

		/// <summary>
		/// Gets the Trailer Extra values for the specified scan number. 
		/// If <paramref name="ifFormatted" /> = true, then the values will be formatted as per the header settings
		/// </summary>
		/// <param name="scanNumber">
		/// scan whose trailer data is needed
		/// </param>
		/// <param name="ifFormatted">
		/// true if the data should be formatted
		/// </param>
		/// <returns>
		/// The strings representing trailer data&gt;.
		/// </returns>
		public string[] GetTrailerExtraValues(int scanNumber, bool ifFormatted)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetTrailerExtraValues(scanNumber, ifFormatted);
			}
		}

		/// <summary>
		/// returns the (unformatted) Trailer Extra value for a specific field in the specified scan number. 
		/// The object type depends on the field type, as returned by 
		/// <see cref="M:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.RawFileThreadAccessor.GetTrailerExtraHeaderInformation" />
		/// Numeric values (where the header for this filed returns "True" for IsNumeric
		/// can always be cast up to double.
		/// The integer numeric types SHORT and USHORT are returned as <c>short and ushort</c>.
		/// The integer numeric types LONG and ULONG are returned as <c>int and uint</c>.
		/// All logical values (Yes/No, True/false, On/Off) are returned as <c>"bool"</c>,
		/// where "true" implies "yes", "true" or "on".
		/// CHAR and UCHAR types are returned as "byte".
		/// String types WCHAR_STRING and CHAR_STRING types are returned as "string".
		/// </summary>
		/// <param name="scanNumber">scan who's data is needed</param>
		/// <param name="field">zero based filed number in the record, as per header </param>
		/// <returns>Value of requested field</returns>
		public object GetTrailerExtraValue(int scanNumber, int field)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetTrailerExtraValue(scanNumber, field);
			}
		}

		/// <summary>
		/// Returns the (unformatted) Trailer Extra values for all  field in the specified scan number. 
		/// The object types depend on the field types, as returned by 
		/// GetTrailerExtraHeaderInformation.
		/// This offers higher performance, where numeric values are needed,
		/// as it avoids translation to and from strings.
		/// It is also designed for efficient copy of data from one file to another.
		/// This can also be used for apps that need to localize numeric values.
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
		public object[] GetTrailerExtraValues(int scanNumber)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetTrailerExtraValues(scanNumber);
			}
		}

		/// <summary>
		/// Returns the (unformatted) Tune Data values for all fields in the specified record number. 
		/// The object types depend on the field types, as returned by 
		/// GetTuneDataHeaderInformation.
		/// This is  designed for efficient copy of data from one file to another.
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
		/// <param name="index">zero bases index into tune records who's data is needed</param>
		/// <returns>Values of all fields</returns>
		/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
		public object[] GetTuneDataValues(int index)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetTuneDataValues(index);
			}
		}

		/// <summary>
		/// Gets the (raw) status log data at a given index in the log.
		/// Deigned for efficiency, this method does not convert logs to display string format.
		/// </summary>
		/// <param name="index">Index (from 0 to "GetStatusLogEntriesCount() -1")</param>
		/// <returns>Log data at the given index</returns>
		public IStatusLogEntry GetStatusLogEntry(int index)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetStatusLogEntry(index);
			}
		}

		/// <summary>
		/// Gets the (raw) status log data at a given retention time in the log.
		/// Designed for efficiency, this method does not convert logs to display string format.
		/// </summary>
		/// <param name="retentionTime">Retention time/</param>
		/// <returns>Log data at the given time</returns>
		public IStatusLogEntry GetStatusLogEntry(double retentionTime)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetStatusLogEntry(retentionTime);
			}
		}

		/// <summary>
		/// Get the advanced LT/FT formats data, such as the noise data, baseline data and frequencies
		/// </summary>
		/// <param name="scanNumber">The scan number.</param>
		/// <returns>
		/// Returns an IAdvancedPacketData object which contains noise data, baseline data and frequencies for specified <paramref name="scanNumber" />.
		/// It might return empty arrays for scans which do not have these data.
		/// </returns>
		/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
		public IAdvancedPacketData GetAdvancedPacketData(int scanNumber)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetAdvancedPacketData(scanNumber);
			}
		}

		/// <summary>
		/// Gets the array of headers and values for this scan number. The values are formatted as per the header settings.
		/// </summary>
		/// <param name="scanNumber">
		/// The scan for which this information is needed
		/// </param>
		/// <returns>
		/// Extra information about the scan
		/// </returns>
		public ILogEntryAccess GetTrailerExtraInformation(int scanNumber)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetTrailerExtraInformation(scanNumber);
			}
		}

		/// <summary>
		/// Gets the segment event table for the current instrument
		/// </summary>
		/// <returns>A two dimensional array of events. The first index is segment index (segment number-1).
		/// The second is event index (event number -1) within the segment.</returns>
		public string[][] GetSegmentEventTable()
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetSegmentEventTable();
			}
		}

		/// <summary>
		/// Request the object to keep a cache of the listed item.
		/// Setting the caching to "zero" disables further caching.
		/// </summary>
		/// <param name="item">
		/// Item to cache
		/// </param>
		/// <param name="limit">
		/// Limit of number of items to cache
		/// </param>
		/// <param name="useCloning">
		/// (optional, default false) if set True, all values returned from the cache are unique  (cloned) references. 
		/// By default, the cache just keeps references to the objects 
		/// </param>
		public void SetCaching(RawCacheItem item, int limit, bool useCloning = false)
		{
			lock (_parent)
			{
				_rawFile.SetCaching(item, limit, useCloning);
			}
		}

		/// <summary>
		/// Clear items in the cache
		/// </summary>
		/// <param name="item">
		/// item type to clear
		/// </param>
		public void ClearCache(RawCacheItem item)
		{
			lock (_parent)
			{
				_rawFile.ClearCache(item);
			}
		}

		/// <summary>
		/// Count the number currently in the cache
		/// </summary>
		/// <param name="item">
		/// Item type to count
		/// </param>
		/// <returns>
		/// The number of items in this cache
		/// </returns>
		public int Cached(RawCacheItem item)
		{
			lock (_parent)
			{
				return _rawFile.Cached(item);
			}
		}

		/// <summary>
		/// Calculate the filters for this raw file, and return as an array
		/// </summary>
		/// <returns>Auto generated list of unique filters</returns>
		public ReadOnlyCollection<IScanFilter> GetFilters()
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetFilters();
			}
		}

		/// <summary>
		/// Calculate the filters for this raw file within the range of scans supplied, and return as an array.
		/// </summary>
		/// <param name="startScan">First scan to analyze</param>
		/// <param name="endScan">Last scan to analyze</param>
		/// <returns>
		/// Auto generated list of unique filters
		/// </returns>
		public ReadOnlyCollection<IScanFilter> GetFiltersForScanRange(int startScan, int endScan)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetFiltersForScanRange(startScan, endScan);
			}
		}

		/// <summary>
		/// Get the filter for a scan number.
		/// </summary>
		/// <param name="scan">
		/// The scan number.
		/// </param>
		/// <returns>
		/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.IScanFilter" />.
		/// </returns>
		public IScanFilter GetFilterForScanNumber(int scan)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetFilterForScanNumber(scan);
			}
		}

		/// <summary>
		/// Get a filter interface from a string.
		/// </summary>
		/// <param name="filter">
		/// The filter string.
		/// </param>
		/// <returns>
		/// An interface representing the filter fields, converted from the supplied string.
		/// </returns>
		public IScanFilter GetFilterFromString(string filter)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetFilterFromString(filter);
			}
		}

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
		public IScanFilter CreateFilterFromScanEvent(IScanEvent scanEvent)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.CreateFilterFromScanEvent(scanEvent);
			}
		}

		/// <summary>
		/// Get a filter interface from a string, with a given mass precisions
		/// </summary>
		/// <param name="filter">
		/// The filter string.
		/// </param>
		/// <param name="precision">Precisions of masses (number of decimal places)</param>
		/// <returns>
		/// An interface representing the filter fields, converted from the supplied string.
		/// </returns>
		public IScanFilter GetFilterFromString(string filter, int precision)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetFilterFromString(filter, precision);
			}
		}

		/// <summary>
		/// Obtain an interface to iterate over a scans which match a specified filter.
		/// The iterator is initialized at "scan 0" such that "GetNext" will return the first matching scan in the file.
		/// This is a low level version of <see cref="M:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.RawFileThreadAccessor.GetFilteredScanEnumerator(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter)" />
		/// </summary>
		/// <param name="filter">Filter, which all returned scans match.
		/// This filter may be created from a string using <see cref="M:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.RawFileThreadAccessor.GetFilterFromString(System.String,System.Int32)" /></param>
		/// <returns>An iterator which can step back and forth over scans matching a given filter.</returns>
		public IFilteredScanIterator GetFilteredScanIterator(IScanFilter filter)
		{
			lock (_parent)
			{
				SetDevice();
				return new FilteredScanIteratorWrapper(this, filter);
			}
		}

		/// <summary>
		/// Get filtered scan enumerator.
		/// </summary>
		/// <param name="filter">The filter, which all enumerated scans match.
		/// This filter may be created from a string using <see cref="M:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.RawFileThreadAccessor.GetFilterFromString(System.String,System.Int32)" />
		/// </param>
		/// <returns>
		/// An enumerator which can be used to "foreach" over all scans in a file, which match a given filter.
		///             Note that each "step" through the enumerator will access further data from the file.
		///             To get a complete list of matching scans in one call, the "ToArray" extension can be called,
		///             but this will result in a delay as all scans in the file are analyzed to return this array.
		///             For fine grained iterator control, including "back stepping" use <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawDataPlus.GetFilteredScanIterator(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter)" />
		/// </returns>
		public IEnumerable<int> GetFilteredScanEnumerator(IScanFilter filter)
		{
			IFilteredScanIterator iterator = GetFilteredScanIterator(filter);
			int nextScan;
			while ((nextScan = iterator.NextScan) > 0)
			{
				iterator.SpectrumPosition = nextScan;
				yield return nextScan;
			}
		}

		/// <summary>
		/// Get filtered scan enumerator.
		/// </summary>
		/// <param name="filter">
		/// The filter, which all enumerated scans match.
		/// This filter may be created from a string using <see cref="M:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.RawFileThreadAccessor.GetFilterFromString(System.String,System.Int32)" />
		/// </param>
		/// <param name="startTime">
		/// The start Time.
		/// </param>
		/// <param name="endTime">
		/// The End Time.
		/// </param>
		/// <returns>
		/// An enumerator which can be used to "foreach" over all scans in a time range, which match a given filter.
		///             Note that each "step" through the enumerator will access further data from the file.
		///             To get a complete list of matching scans in one call, the "ToArray" extension can be called,
		///             but this will result in a delay as all scans in the time range are analyzed to return this array.
		///             For fine grained iterator control, including "back stepping" use <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawDataPlus.GetFilteredScanIterator(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter)" />
		/// </returns>
		public IEnumerable<int> GetFilteredScanEnumeratorOverTime(IScanFilter filter, double startTime, double endTime)
		{
			IFilteredScanIterator iterator = GetFilteredScanIterator(filter);
			int num = ScanNumberFromRetentionTime(startTime);
			if (RetentionTimeFromScanNumber(num) >= startTime)
			{
				if (num >= 1)
				{
					iterator.SpectrumPosition = num - 1;
				}
			}
			else
			{
				iterator.SpectrumPosition = num;
			}
			int nextScan;
			while ((nextScan = iterator.NextScan) > 0 && !(RetentionTimeFromScanNumber(nextScan) > endTime))
			{
				iterator.SpectrumPosition = nextScan;
				yield return nextScan;
			}
		}

		/// <summary>
		/// Export the instrument method to a file.
		/// </summary>
		/// <param name="methodFilePath">
		/// The method file path.
		/// </param>
		/// <param name="forceOverwrite">
		/// Force over write. If true, and file already exists, attempt to delete existing file first.
		/// If false: UnauthorizedAccessException will occur if there is an existing read only file.
		/// </param>
		/// <returns>True if the file was saved. False, if no file was saved, for example,
		/// because there is no instrument method saved in this raw file.</returns>
		/// <seealso cref="P:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.RawFileThreadAccessor.HasInstrumentMethod" />
		public bool ExportInstrumentMethod(string methodFilePath, bool forceOverwrite)
		{
			lock (_parent)
			{
				return _rawFile.ExportInstrumentMethod(methodFilePath, forceOverwrite);
			}
		}

		/// <summary>
		/// Gets the scan event details for a scan
		/// </summary>
		/// <param name="scan">
		/// The scan number.
		/// </param>
		/// <returns>
		/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.IScanEvent" /> interface, to get detailed information about a scan.
		/// </returns>
		public IScanEvent GetScanEventForScanNumber(int scan)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetScanEventForScanNumber(scan);
			}
		}

		/// <summary>
		/// Gets the scan event as a string for a scam
		/// </summary>
		/// <param name="scan">
		/// The scan number.
		/// </param>
		/// <returns>
		/// The event as a string.
		/// </returns>
		public string GetScanEventStringForScanNumber(int scan)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetScanEventStringForScanNumber(scan);
			}
		}

		/// <summary>
		/// Gets an entry from the instrument error log.
		/// </summary>
		/// <param name="index">
		/// Zero based index.
		/// The number of records available is RunHeaderEx.ErrorLogCount </param>
		/// <returns>An interface to read a specific log entry</returns>
		public IErrorLogEntry GetErrorLogItem(int index)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetErrorLogItem(index);
			}
		}

		/// <summary>
		/// Gets the status log data, from all log entries, based on a specific position in the log.
		/// For example: "position" may be selected from one of the key value pairs returned from <see cref="P:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.RawFileThreadAccessor.StatusLogPlottableData" />
		/// in order to create a trend plot of a particular value.
		/// The interface returned has an array of retention times and strings.
		/// If the position was selected by using <see cref="P:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.RawFileThreadAccessor.StatusLogPlottableData" />, then the strings may be converted "ToDouble" to get
		/// the set of numeric values to plot.
		/// </summary>
		/// <param name="position">
		/// The position within the list of available status log values.
		/// </param>
		/// <returns>
		/// An interface containing the times and logged values for the selected status log field.
		/// </returns>
		public ISingleValueStatusLog GetStatusLogAtPosition(int position)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetStatusLogAtPosition(position);
			}
		}

		/// <summary>
		/// Get all instrument friendly names from the instrument method.
		/// </summary>
		/// <returns>
		/// The instrument friendly names"/&gt;.
		/// </returns>
		public string[] GetAllInstrumentFriendlyNamesFromInstrumentMethod()
		{
			lock (_parent)
			{
				return _rawFile.GetAllInstrumentFriendlyNamesFromInstrumentMethod();
			}
		}

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
		/// Information about how data dependant scanning was performed.
		/// </returns>
		public IScanDependents GetScanDependents(int scanNumber, int filterPrecisionDecimals)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetScanDependents(scanNumber, filterPrecisionDecimals);
			}
		}

		/// <summary>
		/// Gets the unique compound names as arrays of strings.
		/// </summary>
		/// <returns>
		/// The Compound Names.
		/// </returns>
		public string[] GetCompoundNames()
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetCompoundNames();
			}
		}

		/// <summary>
		/// Gets the unique compound names as arrays of strings by given filter.
		/// </summary>
		/// <param name="scanFilter">
		/// The scan Filter.
		/// </param>
		/// <returns>
		/// The compound names"/&gt;.
		/// </returns>
		public string[] GetCompoundNames(string scanFilter)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetCompoundNames(scanFilter);
			}
		}

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
		public string[] GetScanFiltersFromCompoundName(string compoundName)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetScanFiltersFromCompoundName(compoundName);
			}
		}

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
		public IChromatogramDataPlus GetChromatogramDataEx(IChromatogramSettingsEx[] settings, int startScan, int endScan, MassOptions toleranceOptions)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetChromatogramDataEx(settings, startScan, endScan, toleranceOptions);
			}
		}

		/// <inheritdoc />
		public IChromatogramDataPlus GetChromatogramDataEx(IChromatogramSettingsEx[] settings, int startScan, int endScan, MassOptions toleranceOptions, bool alwaysUseAccuratePrecursors)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetChromatogramDataEx(settings, startScan, endScan, toleranceOptions, alwaysUseAccuratePrecursors);
			}
		}

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
		public IChromatogramDataPlus GetChromatogramDataEx(IChromatogramSettingsEx[] settings, int startScan, int endScan)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetChromatogramDataEx(settings, startScan, endScan);
			}
		}

		/// <summary>
		/// Test if a scan passes a filter.
		/// If all matching scans in a file are required, consider using <see cref="M:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.RawFileThreadAccessor.GetFilteredScanEnumerator(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter)" /> or <see cref="M:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.RawFileThreadAccessor.GetFilteredScanEnumeratorOverTime(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter,System.Double,System.Double)" />
		/// </summary>
		/// <param name="scan">the scan number</param>
		/// <param name="filter">the filter to test</param>
		/// <returns>True if this scan passes the filter</returns>
		public bool TestScan(int scan, string filter)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.TestScan(scan, filter);
			}
		}

		/// <summary>
		/// This method is similar to GetSegmentedScanFromScanNumber in the IRawData interface.
		/// The method returns only the mass and intensity values from
		/// the scan data for a scan. 
		/// Values for flags etc. are not returned, saving data space and improving efficiency.
		/// This method never returns "reference and exception peak" data.
		/// The method is designed for improved performance in custom XIC generators.
		/// </summary>
		/// <param name="scanNumber">The scan whose mass intensity data are needed</param>
		/// <returns>Mass and intensity values from the scan.</returns>
		public ISimpleScanAccess GetSimplifiedScan(int scanNumber)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetSimplifiedScan(scanNumber);
			}
		}

		/// <summary>
		/// This method is similar to GetCentroidStream in the IRawData interface.
		/// The method returns only the mass and intensity values from
		/// the "centroid stream" data for a scan. This is also known as "Label Stream"
		/// Values for flags etc. are not returned, saving data space and improving efficiency.
		/// This method never returns "reference and exception peak" data.
		/// The method is designed for improved performance in custom XIC generators.
		/// </summary>
		/// <param name="scanNumber">The scan who's mass intensity data are needed</param>
		/// <returns>Mass and intensity values from the scan "centroid data".</returns>
		public ISimpleScanAccess GetSimplifiedCentroids(int scanNumber)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetSimplifiedCentroids(scanNumber);
			}
		}

		/// <summary>
		/// When deciding what data should be read from a scan, centroids or regular scan
		/// (or if the data is needed at all)
		/// scan event data is needed.
		/// This method permits events to be read as a block for a range of scans,
		/// which may reduce overheads involved in requesting one by one.
		/// Potentially, in some data models, the same "event" may apply to several scans
		/// so it is permissible for the same reference to appear multiple times.
		/// </summary>
		/// <param name="firstScanNumber">The first scan whose event is needed</param>
		/// <param name="lastScanNumber">The last scan</param>
		/// <returns>An array of scan events</returns>
		public IScanEvent[] GetScanEvents(int firstScanNumber, int lastScanNumber)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetScanEvents(firstScanNumber, lastScanNumber);
			}
		}

		/// <inheritdoc />
		public string[][] GetScanFiltersFromCompoundNames(string[] compoundNames)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetScanFiltersFromCompoundNames(compoundNames);
			}
		}

		/// <inheritdoc />
		public string[] GetCompoundNamesForScanRange(int startScan, int endScan)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetCompoundNamesForScanRange(startScan, endScan);
			}
		}

		/// <summary>
		/// Gets the (raw) status log data at a given index in the sorted log.
		/// The form of the log removes duplicate and out of order times
		/// Designed for efficiency, this method does not convert logs to display string format.
		/// </summary>
		/// <param name="index">Index (from 0 to "GetStatusLogEntriesCount() -1")</param>
		/// <returns>Log data at the given index</returns>
		public IStatusLogEntry GetSortedStatusLogEntry(int index)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFileExtended.GetSortedStatusLogEntry(index);
			}
		}

		/// <inheritdoc />
		public IMsScanIndexAccess GetMsScanIndex(int scanNumber)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.GetMsScanIndex(scanNumber);
			}
		}

		/// <inheritdoc />
		public IIndexAndEvent ReadEventAndIndex(int scan)
		{
			lock (_parent)
			{
				SetDevice();
				return _rawFile.ReadEventAndIndex(scan);
			}
		}

		/// <inheritdoc />
		public IDetectorReader GetDetectorReader(IInstrumentSelectionAccess detector, bool includeReferenceAndExceptionPeaks)
		{
			lock (_parent)
			{
				return _rawFileExtended.GetDetectorReader(detector, includeReferenceAndExceptionPeaks);
			}
		}

		public ReadOnlyCollection<IScanFilter> GetAccurateFiltersForScanRange(int startScan, int endScan, FilterPrecisionMode mode = FilterPrecisionMode.Instrument, int decimalPlaces = 2)
		{
			lock (_parent)
			{
				return _rawFileExtended.GetAccurateFiltersForScanRange(startScan, endScan, mode, decimalPlaces);
			}
		}
	}

	private readonly IRawDataExtended _rawFileExtended;

	private readonly IRawDataPlus _rawFile;

	private Device _instrumentType = Device.Other;

	private int _instrumentIndex = -1;

	private bool _includeReferenceAndExceptionData;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess" /> class.
	/// To use this: Construct an interface "IRawDataPlus".
	/// Construct this object from the IRawDataPlus.
	/// Call <see cref="M:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.CreateThreadAccessor" /> to make a separate object for each thread to use.
	/// After all threads are completed,
	/// and the raw file is no longer needed, dispose of the original IRawDataPlus. 
	/// </summary>
	/// <param name="file">
	/// The file.
	/// </param>
	public ThreadSafeRawFileAccess(IRawDataExtended file)
	{
		_rawFile = file;
		_rawFileExtended = file;
		_includeReferenceAndExceptionData = _rawFile.IncludeReferenceAndExceptionData;
		InstrumentSelection selectedInstrument = _rawFile.SelectedInstrument;
		if (selectedInstrument != null)
		{
			_instrumentType = selectedInstrument.DeviceType;
			_instrumentIndex = selectedInstrument.InstrumentIndex;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess" /> class.
	/// To use this: Construct an interface "IRawDataPlus".
	/// Construct this object from the IRawDataPlus.
	/// Call <see cref="M:ThermoFisher.CommonCore.Data.Business.ThreadSafeRawFileAccess.CreateThreadAccessor" /> to make a separate object for each thread to use.
	/// After all threads are completed,
	/// and the raw file is no longer needed, dispose of the original IRawDataPlus. 
	/// </summary>
	/// <param name="file">
	/// The file.
	/// </param>
	public ThreadSafeRawFileAccess(IRawDataPlus file)
	{
		_rawFile = file;
		_rawFileExtended = null;
		_includeReferenceAndExceptionData = _rawFile.IncludeReferenceAndExceptionData;
		InstrumentSelection selectedInstrument = _rawFile.SelectedInstrument;
		if (selectedInstrument != null)
		{
			_instrumentType = selectedInstrument.DeviceType;
			_instrumentIndex = selectedInstrument.InstrumentIndex;
		}
	}

	/// <summary>
	/// Create an accessor for one thread to use. Call this method once for each thread you plan to create.
	/// </summary>
	/// <returns>
	/// An interface to read data, which should be used by a single thread.
	/// </returns>
	public IRawDataExtended CreateThreadAccessor()
	{
		return new RawFileThreadAccessor(this);
	}

	/// <summary>
	/// Select instrument.
	/// This private method keeps track of the selected instrument within the raw data.
	/// When a thread is using a different instrument than the last thread's call,
	/// then the IO library selected instrument is changed.
	/// If all threads use one instrument (such as MS), then this will
	/// not result in multiple calls to the IO library.
	/// </summary>
	/// <param name="instrumentType">
	/// The instrument type.
	/// </param>
	/// <param name="instrumentIndex">
	/// The instrument index.
	/// </param>
	private void SelectInstrument(Device instrumentType, int instrumentIndex)
	{
		if (instrumentType != _instrumentType || instrumentIndex != _instrumentIndex)
		{
			_instrumentType = instrumentType;
			_instrumentIndex = instrumentIndex;
			_rawFile.SelectInstrument(instrumentType, instrumentIndex);
		}
	}

	/// <summary>
	/// Include reference and exception data.
	/// Remembers the state of this from previous calls, and
	/// only sets this to the IO library when changed from the last call.
	/// </summary>
	/// <param name="includeReferenceAndExceptionData">
	/// The include reference and exception data.
	/// </param>
	private void IncludeReferenceAndExceptionData(bool includeReferenceAndExceptionData)
	{
		if (includeReferenceAndExceptionData != _includeReferenceAndExceptionData)
		{
			_includeReferenceAndExceptionData = includeReferenceAndExceptionData;
			_rawFile.IncludeReferenceAndExceptionData = includeReferenceAndExceptionData;
		}
	}

	/// <summary>
	/// Request the object to keep a cache of the listed item.
	/// Setting the caching to "zero" disables further caching.
	/// </summary>
	/// <param name="item">
	/// Item to cache
	/// </param>
	/// <param name="limit">
	/// Limit of number of items to cache
	/// </param>
	/// <param name="useCloning">
	/// (optional, default false) if set True, all values returned from the cache are unique  (cloned) references. 
	/// By default, the cache just keeps references to the objects 
	/// </param>
	public void SetCaching(RawCacheItem item, int limit, bool useCloning = false)
	{
		_rawFile.SetCaching(item, limit, useCloning);
	}

	/// <summary>
	/// Clear items in the cache
	/// </summary>
	/// <param name="item">
	/// item type to clear
	/// </param>
	public void ClearCache(RawCacheItem item)
	{
		_rawFile.ClearCache(item);
	}

	/// <summary>
	/// Count the number currently in the cache
	/// </summary>
	/// <param name="item">
	/// Item type to count
	/// </param>
	/// <returns>
	/// The number of items in this cache
	/// </returns>
	public int Cached(RawCacheItem item)
	{
		return _rawFile.Cached(item);
	}
}
