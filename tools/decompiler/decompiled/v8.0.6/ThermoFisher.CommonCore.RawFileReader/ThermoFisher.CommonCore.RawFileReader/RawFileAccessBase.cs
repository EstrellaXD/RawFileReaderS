using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.DataModel;
using ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.ASR;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// The raw file access base.
/// This provides an implementation of the IRawDataPlus against ".raw" files.
/// Derived classes add specific (internal) functionality, such as thread handling.
/// </summary>
internal class RawFileAccessBase : IRawDataExtended, IRawDataPlus, IRawData, IDetectorReaderBase, IRawDataProperties, IDisposable, IRawCache, ISimplifiedScanReader, IDetectorReaderPlus, IRawDataExtensions, IDetectorReader, IConfiguredDetector, IInstrumentSelectionAccess
{
	/// <summary>
	/// Implements IIndexAndEvent
	/// </summary>
	private class IndexAndEvent : IIndexAndEvent
	{
		/// <inheritdoc />
		public IMsScanIndexAccess ScanIndex { get; set; }

		/// <inheritdoc />
		public IScanEvent ScanEvent { get; set; }
	}

	private class EncodedScan : IEncodedScan
	{
		public byte[] ScanData { get; set; }

		public double[] MassCalibrators { get; set; }

		public IMsScanIndexAccess ScanIndex { get; set; }
	}

	private IRawFileLoader _rawFileLoader;

	private Device _selectedInstrumentType;

	private int _selectedInstrumentIndex;

	private IDevice _selectedDevice;

	private VirtualControllerInfo _selectedVirtualController;

	private bool _includeReferenceAndExceptionData;

	private bool _disposePermitted;

	/// <summary>
	/// Gets the auto sampler information.
	/// </summary>
	public IAutoSamplerInformation AutoSamplerInformation
	{
		get
		{
			ValidateOpenFile();
			if (_rawFileLoader.AutoSamplerConfig != null)
			{
				return new WrappedAutoSamplerInformation(_rawFileLoader.AutoSamplerConfig);
			}
			return new WrappedAutoSamplerInformation();
		}
	}

	/// <summary>
	/// Gets the file error state.
	/// </summary>
	public IFileError FileError => _rawFileLoader;

	/// <summary>
	/// Gets the raw file header.
	/// </summary>
	public IFileHeader FileHeader
	{
		get
		{
			ValidateOpenFile();
			return _rawFileLoader.Header;
		}
	}

	/// <summary>
	/// Gets the name of the computer, used to create this file.
	/// </summary>
	public string ComputerName => _rawFileLoader.RawFileInformation.ComputerName;

	/// <summary>
	/// Gets a value indicating whether this file has MS data.
	/// </summary>
	public bool HasMsData => GetInstrumentCountOfType(Device.MS) != 0;

	/// <summary>
	/// Gets the scan events.
	/// </summary>
	public IScanEvents ScanEvents => new WrappedScanEvents(RequireMassSpec("ScanEvents"));

	/// <summary>
	/// Gets the labels and index positions of the status log items which may be plotted.
	/// That is, the numeric items.
	/// Labels names are returned by "Key" and the index into the log is "Value".
	/// </summary>
	public KeyValuePair<string, int>[] StatusLogPlottableData
	{
		get
		{
			RequireDevice("StatusLogPlottableData");
			IStatusLog statusLogEntries = _selectedDevice.StatusLogEntries;
			if (CheckEmptyStatusLog(statusLogEntries))
			{
				return new KeyValuePair<string, int>[0];
			}
			return statusLogEntries.StatusLogPlottableData().ToArray();
		}
	}

	/// <summary>
	/// Gets the set of user labels
	/// </summary>
	public string[] UserLabel
	{
		get
		{
			ValidateOpenFile();
			string[] userLabels = _rawFileLoader.RawFileInformation.UserLabels;
			string[] array = Enumerable.Repeat(string.Empty, 5).ToArray();
			int num = 0;
			string[] array2 = userLabels;
			foreach (string text in array2)
			{
				array[num++] = text;
			}
			return array;
		}
	}

	/// <summary>
	/// Gets the date when this data was created.
	/// </summary>
	public DateTime CreationDate => FileHeader.CreationDate;

	/// <summary>
	/// Gets the name of person creating data.
	/// </summary>
	public string CreatorId => FileHeader.WhoCreatedId;

	/// <summary>
	/// Gets the name of acquired file (excluding path).
	/// </summary>
	public string FileName
	{
		get
		{
			ValidateOpenFile();
			return _rawFileLoader.RawFileName;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the file is being acquired (not complete).
	/// </summary>
	public bool InAcquisition
	{
		get
		{
			ValidateOpenFile();
			return _rawFileLoader.RawFileInformation.IsInAcquisition;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether reference and exception peaks should be returned (by default they are not)
	/// </summary>
	/// <value></value>
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
	/// Gets the number of instruments (data streams) in this file.
	/// </summary>
	public int InstrumentCount
	{
		get
		{
			ValidateOpenFile();
			return _rawFileLoader.RawFileInformation.NumberOfVirtualControllers;
		}
	}

	/// <summary>
	/// Gets the number of instrument methods in this file.
	/// </summary>
	public int InstrumentMethodsCount
	{
		get
		{
			ValidateOpenFile();
			if (_rawFileLoader.RawFileInformation.HasExpMethod && _rawFileLoader.Header.Revision >= 25 && _rawFileLoader.MethodInfo?.StorageDescriptions != null && _rawFileLoader.MethodInfo.StorageDescriptions.Any())
			{
				return _rawFileLoader.MethodInfo.StorageDescriptions.Count;
			}
			return 0;
		}
	}

	/// <summary>
	/// Gets a value indicating whether this file has an instrument method.
	/// </summary>
	public bool HasInstrumentMethod
	{
		get
		{
			ValidateOpenFile();
			return _rawFileLoader.RawFileInformation.HasExpMethod;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the last file operation caused an error
	/// </summary>
	/// <value></value>
	public bool IsError
	{
		get
		{
			if (_rawFileLoader != null)
			{
				return _rawFileLoader.HasError;
			}
			return false;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the data file was successfully opened
	/// </summary>
	public bool IsOpen
	{
		get
		{
			if (_rawFileLoader != null)
			{
				return _rawFileLoader.IsOpen;
			}
			return false;
		}
	}

	/// <summary>
	/// Gets the path to original data. (path set when acquired)
	/// </summary>
	public string Path
	{
		get
		{
			ValidateOpenFile();
			return _rawFileLoader.Sequence.Path;
		}
	}

	/// <summary>
	/// Gets extended the run header details.
	/// </summary>
	public ThermoFisher.CommonCore.Data.Interfaces.IRunHeader RunHeaderEx
	{
		get
		{
			RequireDevice("RunHeaderEx");
			return new WrappedRunHeader(RawFileLoaderHelper.GetDeviceRunHeader(Manager, _rawFileLoader.Id, _selectedDevice));
		}
	}

	/// <summary>
	/// Gets the current instrument's run header
	/// </summary>
	public IRunHeaderAccess RunHeader
	{
		get
		{
			RequireDevice("RunHeader");
			return RawFileLoaderHelper.GetDeviceRunHeader(Manager, _rawFileLoader.Id, _selectedDevice);
		}
	}

	/// <summary>
	/// Gets various details about the sample (such as comments).
	/// </summary>
	public SampleInformation SampleInformation
	{
		get
		{
			ValidateOpenFile();
			return new WrappedSequenceRow(_rawFileLoader.Sequence);
		}
	}

	/// <summary>
	/// Gets the instrument as last set by a call to <see cref="M:ThermoFisher.CommonCore.RawFileReader.RawFileAccessBase.SelectInstrument(ThermoFisher.CommonCore.Data.Business.Device,System.Int32)" /> method.
	/// </summary>
	public InstrumentSelection SelectedInstrument
	{
		get
		{
			ValidateOpenFile();
			if (_selectedDevice != null)
			{
				return new InstrumentSelection(_selectedInstrumentIndex, _selectedInstrumentType);
			}
			return new InstrumentSelection(-1, Device.None);
		}
	}

	/// <inheritdoc />
	public IConfiguredDetector ConfiguredDetector => this;

	/// <inheritdoc />
	public bool UseReferenceAndExceptionData => _includeReferenceAndExceptionData;

	/// <inheritdoc />
	public int InstrumentIndex => _selectedInstrumentIndex;

	/// <inheritdoc />
	public Device DeviceType => _selectedInstrumentType;

	public IViewCollectionManager Manager { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.RawFileAccessBase" /> class.
	/// </summary>
	private RawFileAccessBase()
	{
		_selectedInstrumentType = Device.Other;
		_selectedInstrumentIndex = -1;
		_selectedDevice = null;
		_selectedVirtualController = null;
		_includeReferenceAndExceptionData = false;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.RawFileAccessBase" /> class.
	/// </summary>
	/// <param name="loader">The loader.</param>
	internal RawFileAccessBase(IRawFileLoader loader)
		: this()
	{
		loader.AddUse();
		_rawFileLoader = loader;
	}

	/// <summary>
	/// Save instrument method file.
	/// </summary>
	/// <param name="methodFilePath">
	/// The method file path.
	/// </param>
	/// <param name="forceOverwrite">
	/// Force over write. If true, and file already exists, attempt to delete existing file first.
	/// If false: UnauthorizedAccessException will occur if there is an existing read only file.
	/// </param>
	/// <returns>
	/// True if export was achieved
	/// </returns>
	public bool ExportInstrumentMethod(string methodFilePath, bool forceOverwrite)
	{
		ValidateOpenFile();
		if (ReallyHasInstrumentMethod())
		{
			_rawFileLoader.ExportInstrumentMethod(methodFilePath, forceOverwrite);
			return true;
		}
		return false;
	}

	/// <summary>
	/// get scan type as string.
	/// </summary>
	/// <param name="scanNum">
	/// The scan number.
	/// </param>
	/// <param name="massSpecDevice">
	/// The mass spec device.
	/// </param>
	/// <returns>
	/// The scan type.
	/// </returns>
	private static string GetScanTypeAsString(int scanNum, MassSpecDevice massSpecDevice)
	{
		return FilterScanEventForScan(scanNum, massSpecDevice).ToString();
	}

	/// <summary>
	/// Gets The filter scan event for scan.
	/// </summary>
	/// <param name="scanNum">
	/// The scan number.
	/// </param>
	/// <param name="massSpecDevice">
	/// The mass spec device.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.FilterScanEvent" />.
	/// </returns>
	private static FilterScanEvent FilterScanEventForScan(int scanNum, MassSpecDevice massSpecDevice)
	{
		ScanEvent scanEvent = massSpecDevice.GetScanEvent(scanNum);
		return new FilterScanEvent(scanEvent.CreateEditor())
		{
			FilterMassPrecision = scanEvent.HeaderFilterMassPrecision
		};
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
		MassSpecDevice massSpecDevice = RequireMassSpec("FilterFromScanEvent");
		return new WrappedScanFilter(new FilterScanEvent(ScanEvent.FromEvent(scanEvent))
		{
			FilterMassPrecision = massSpecDevice.RunHeader.FilterMassPrecision
		});
	}

	/// <summary>
	/// Validate that the raw file is open and loaded OK.
	/// </summary>
	/// <exception cref="T:System.Exception">If there was some problem opening the file
	/// </exception>
	private void ValidateOpenFile()
	{
		if (_rawFileLoader == null || !_rawFileLoader.IsOpen)
		{
			throw new Exception(Resources.ErrorNoOpenRawFile);
		}
	}

	/// <summary>
	/// Test if we really have an instrument method.
	/// </summary>
	/// <returns>
	/// True if there is a valid method.
	/// </returns>
	private bool ReallyHasInstrumentMethod()
	{
		if (_rawFileLoader.RawFileInformation.HasExpMethod && _rawFileLoader.Header.Revision >= 25)
		{
			return _rawFileLoader.MethodInfo != null;
		}
		return false;
	}

	/// <summary>
	/// Get all instrument friendly names from the instrument method.
	/// </summary>
	/// <returns>
	/// The instrument friendly names"/&gt;.
	/// </returns>
	public string[] GetAllInstrumentFriendlyNamesFromInstrumentMethod()
	{
		ValidateOpenFile();
		if (ReallyHasInstrumentMethod() && _rawFileLoader.MethodInfo.StorageDescriptions != null && _rawFileLoader.MethodInfo.StorageDescriptions.Any())
		{
			return _rawFileLoader.MethodInfo.StorageDescriptions.Select((StorageDescription sd) => sd.Description).ToArray();
		}
		return new string[0];
	}

	/// <summary>
	/// Create a chromatogram from the data stream.
	///             Extended version:
	///             Parameters include option for component names.
	///             Includes base peak data for each scan.
	/// </summary>
	/// <param name="settings">Definition of how the chromatogram is read
	///             </param><param name="startScan">First scan to read from. -1 for "all data"
	///             </param><param name="endScan">Last scan to read from. -1 for "all data"
	///             </param>
	/// <returns>
	/// Chromatogram points
	/// </returns>
	public IChromatogramDataPlus GetChromatogramDataEx(IChromatogramSettingsEx[] settings, int startScan, int endScan)
	{
		return GetChromatogramDataEx(settings, startScan, endScan, new MassOptions(0.0, ThermoFisher.CommonCore.Data.ToleranceUnits.amu));
	}

	/// <summary>
	/// Convert from a range of scans to a time range.
	/// </summary>
	/// <param name="startScan">First scan to read from. -1 for "from start of data"</param>
	/// <param name="endScan">Last scan to read from. -1 for "to end of data"</param>
	/// <returns>The retention time range of the supplied scans</returns>
	private ThermoFisher.CommonCore.Data.Business.Range RetentionTimeRangeFromScans(int startScan, int endScan)
	{
		return ThermoFisher.CommonCore.Data.Business.Range.Create(RetentionTimeFromScanNumber(startScan), RetentionTimeFromScanNumber(endScan));
	}

	/// <summary>
	/// Handle rule that either start or end can be -1, for "use limits from header"
	/// </summary>
	/// <param name="info">
	/// run header info
	/// </param>
	/// <param name="startScan">
	/// start scan
	/// </param>
	/// <param name="endScan">
	/// end scan
	/// </param>
	/// <returns>
	/// true if the range is valid
	/// </returns>
	private bool FixScanLimits(ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader info, ref int startScan, ref int endScan)
	{
		bool num = startScan == -1;
		bool flag = endScan == -1;
		if (num)
		{
			startScan = info.FirstSpectrum;
			if (flag)
			{
				endScan = info.LastSpectrum;
				return true;
			}
			if (startScan > 0 && endScan >= startScan)
			{
				return endScan <= info.LastSpectrum;
			}
			return false;
		}
		if (startScan > info.LastSpectrum)
		{
			return false;
		}
		if (flag)
		{
			endScan = info.LastSpectrum;
		}
		if (startScan > 0 && endScan >= startScan)
		{
			return endScan <= info.LastSpectrum;
		}
		return false;
	}

	/// <inheritdoc />
	public IChromatogramDataPlus GetChromatogramDataEx(IChromatogramSettingsEx[] settings, int startScan, int endScan, MassOptions toleranceOptions, bool alwaysUseAccuratePrecursors)
	{
		if (settings == null || settings.Contains(null))
		{
			throw new ArgumentNullException("settings");
		}
		RequireChromatographicDevice("GetChromatogramDataEx");
		ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader deviceRunHeader = RawFileLoaderHelper.GetDeviceRunHeader(Manager, _rawFileLoader.Id, _selectedDevice);
		if (deviceRunHeader.LastSpectrum < 1)
		{
			return ChromatogramSignal.ToChromatogramDataPlus(null);
		}
		if (!FixScanLimits(deviceRunHeader, ref startScan, ref endScan))
		{
			throw new ArgumentOutOfRangeException("startScan", "Scan limits (startScan and endScan) out of range");
		}
		switch (_selectedInstrumentType)
		{
		case Device.MS:
			return CreateMsChromatograms(settings, toleranceOptions, startScan, endScan, alwaysUseAccuratePrecursors);
		case Device.MSAnalog:
		case Device.Analog:
		case Device.UV:
		case Device.Pda:
			return new SimpleDetectorChromatogramBuilder(_selectedDevice).CreateChromatograms(settings, startScan, endScan);
		default:
			return null;
		}
	}

	/// <summary>
	/// Create a chromatogram from the data stream.
	///             Extended version:
	///             Parameters include option for component names.
	///             Includes base peak data for each scan.
	/// </summary>
	/// <param name="settings">Definition of how the chromatogram is read
	///             </param><param name="startScan">First scan to read from. -1 for "all data"
	///             </param><param name="endScan">Last scan to read from. -1 for "all data"
	///             </param><param name="toleranceOptions">For mass range or base peak chromatograms,
	///             if the ranges have equal mass values,
	///             then <paramref name="toleranceOptions" /> are used to determine a band
	///             subtracted from low and added to high to search for matching masses.
	///             For example: with 5 ppm tolerance, the caller can pass a single mass value (same low and high) for each mass range,
	///             and get chromatograms of those masses +/- 5 ppm.
	///             </param>
	/// <returns>
	/// Chromatogram points
	/// </returns>
	public IChromatogramDataPlus GetChromatogramDataEx(IChromatogramSettingsEx[] settings, int startScan, int endScan, MassOptions toleranceOptions)
	{
		return GetChromatogramDataEx(settings, startScan, endScan, toleranceOptions, alwaysUseAccuratePrecursors: false);
	}

	/// <summary>
	/// The create MS chromatograms.
	/// </summary>
	/// <param name="settings">_s
	/// The settings.
	/// </param>
	/// <param name="toleranceOptions">
	/// The tolerance options.
	/// </param>
	/// <param name="startScan">
	/// The start scan.
	/// </param>
	/// <param name="endScan">
	/// The end scan.
	/// </param>
	/// <param name="alwaysUseAccuratePrecursors">If set: then precursor tolerance is based on
	/// the precision of the scan filters supplied
	/// (+/- half of the final digit).
	/// If not set, then precursors are matched based on settings logged by the device in the raw data</param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.IChromatogramDataPlus" />.
	/// </returns>
	private IChromatogramDataPlus CreateMsChromatograms(IChromatogramSettingsEx[] settings, MassOptions toleranceOptions, int startScan, int endScan, bool alwaysUseAccuratePrecursors)
	{
		MassSpecDevice massSpecDevice = RequireMassSpec("CreateMsChromatograms");
		ThermoFisher.CommonCore.Data.Business.Range timeRange = RetentionTimeRangeFromScans(startScan, endScan);
		ChromatogramDelivery[] array = massSpecDevice.CreateChromatograms(settings, timeRange, toleranceOptions, addBasePeaks: true, IncludeReferenceAndExceptionData, alwaysUseAccuratePrecursors);
		int num = array.Length;
		ChromatogramSignal[] array2 = new ChromatogramSignal[num];
		for (int i = 0; i < num; i++)
		{
			array2[i] = array[i].DeliveredSignal;
		}
		ApplyDelays(settings, array2);
		return ChromatogramSignal.ToChromatogramDataPlus(array2);
	}

	/// <summary>
	/// Gets the unique compound names as arrays of strings by given filter.
	/// </summary>
	/// <param name="scanFilter">The scan Filter.</param>
	/// <returns>
	/// The compound names
	/// </returns>
	public string[] GetCompoundNames(string scanFilter)
	{
		return RequireMassSpec("GetCompoundNames").GetCompoundNameByScanFilter(scanFilter).ToArray();
	}

	/// <summary>
	/// Gets the unique compound names as arrays of strings.
	/// </summary>
	/// <returns>
	/// The Compound Names.
	/// </returns>
	public string[] GetCompoundNames()
	{
		return RequireMassSpec("GetCompoundNames").GetCompoundNames().ToArray();
	}

	/// <summary>
	/// Gets an entry from the instrument error log.
	/// </summary>
	/// <param name="index">Zero based index.
	/// The number of records available is RunHeaderEx.ErrorLogCount</param>
	/// <returns>
	/// An interface to read a specific log entry
	/// </returns>
	public IErrorLogEntry GetErrorLogItem(int index)
	{
		RequireDevice("GetErrorLogItem");
		if (_selectedDevice.ErrorLogEntries == null || _selectedDevice.ErrorLogEntries.Count == 0)
		{
			return new WrappedErrorLogEntry();
		}
		if (index >= _selectedDevice.ErrorLogEntries.Count)
		{
			return new WrappedErrorLogEntry();
		}
		return new WrappedErrorLogEntry(_selectedDevice.ErrorLogEntries.GetItem(index));
	}

	/// <summary>
	/// Get the filter (scanning method) for a scan number. This method is only defined for MS detectors.
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
	public IScanFilter GetFilterForScanNumber(int scan)
	{
		ValidateOpenFile();
		MassSpecDevice massSpecDevice = RequireMassSpec("GetFilterForScanNumber");
		if (!massSpecDevice.ScanNumberIsValid(scan))
		{
			throw new ArgumentOutOfRangeException("scan");
		}
		return new WrappedScanFilter(FilterScanEventForScan(scan, massSpecDevice));
	}

	/// <summary>
	/// Requires the MS Device
	/// </summary>
	/// <param name="reason">The reason. (Calling method)</param>
	/// <returns>The mass spec device</returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if not an mass spec</exception>
	private MassSpecDevice RequireMassSpec(string reason)
	{
		return (_selectedDevice as MassSpecDevice) ?? throw new NoSelectedMsDeviceException(reason);
	}

	/// <summary>
	/// Get a filter interface from a string, with a given mass precisions
	/// </summary>
	/// <param name="filter">The filter string.</param>
	/// <param name="precision">Precisions of masses (number of decimal places)</param>
	/// <returns>
	/// An interface representing the filter fields, converted from the supplied string.
	/// </returns>
	public IScanFilter GetFilterFromString(string filter, int precision)
	{
		return new FilterStringParser
		{
			MassPrecision = precision
		}.GetFilterFromString(filter);
	}

	/// <summary>
	/// Get a filter interface from a string.
	/// </summary>
	/// <param name="filter">The filter string.</param>
	/// <returns>
	/// An interface representing the filter fields, converted from the supplied string.
	/// </returns>
	public IScanFilter GetFilterFromString(string filter)
	{
		FilterStringParser filterStringParser = new FilterStringParser
		{
			MassPrecision = 10
		};
		if (filterStringParser.ParseFilterStructString(filter))
		{
			return new WrappedScanFilter(filterStringParser.ToFilterScanEvent(fromScan: false));
		}
		return null;
	}

	/// <summary>
	/// Get filtered scan enumerator.
	/// </summary>
	/// <param name="filter">The filter.
	///             </param>
	/// <returns>
	/// An enumerator which can be used to <c>foreach</c> over all scans in a file, which match a given filter.
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
	/// The filter.
	///             </param>
	/// <param name="startTime">
	/// The start Time.
	/// </param>
	/// <param name="endTime">
	/// The End Time.
	/// </param>
	/// <returns>
	/// An enumerator which can be used to <c>foreach</c> over all scans in a time range, which match a given filter.
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
	/// Obtain an interface to iterate over a scans which match a specified filter.
	///             The iterator is initialized at "scan 0" such that "GetNext" will return the first matching scan in the file.
	///             This is a low level version of <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawDataPlus.GetFilteredScanEnumerator(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter)" />
	/// </summary>
	/// <param name="filter">The filter to match scans against</param>
	/// <returns>The iterator</returns>
	public IFilteredScanIterator GetFilteredScanIterator(IScanFilter filter)
	{
		if (filter == null)
		{
			throw new ArgumentNullException("filter");
		}
		if (!(filter is WrappedScanFilter wrappedScanFilter))
		{
			throw new ArgumentException("Only filter types from this DLL are supported", "filter");
		}
		return new WrappedFilteredScanIterator(RequireMassSpec("GetFilteredScanIterator"), wrappedScanFilter.Filter);
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
	/// Information about how data dependent scanning was performed.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	public IScanDependents GetScanDependents(int scanNumber, int filterPrecisionDecimals)
	{
		MassSpecDevice massSpecDevice = RequireMassSpec("GetScanDependents");
		if (!massSpecDevice.ScanNumberIsValid(scanNumber))
		{
			throw new ArgumentOutOfRangeException("scanNumber");
		}
		if (filterPrecisionDecimals < 0)
		{
			throw new ArgumentOutOfRangeException("filterPrecisionDecimals");
		}
		return massSpecDevice.GetScanDependents(scanNumber, filterPrecisionDecimals);
	}

	/// <summary>
	/// Gets the scan event for scan number.
	/// </summary>
	/// <param name="scanNum">The scan number.</param>
	/// <returns>The scan event for the given scan</returns>
	public IScanEvent GetScanEventForScanNumber(int scanNum)
	{
		ScanEvent scanEvent;
		if ((scanEvent = RequireMassSpec("GetScanEventForScanNumber").GetScanEvent(scanNum)) == null)
		{
			return new ScanEvent();
		}
		return scanEvent;
	}

	/// <summary>
	/// Gets the scan event as a string for a scam
	/// </summary>
	/// <param name="scan">The scan number.</param>
	/// <returns>
	/// The event as a string.
	/// </returns>
	public string GetScanEventStringForScanNumber(int scan)
	{
		IScanEvent scanEventForScanNumber;
		if ((scanEventForScanNumber = GetScanEventForScanNumber(scan)) != null)
		{
			return scanEventForScanNumber.ToString();
		}
		return string.Empty;
	}

	/// <summary>
	/// Get the scan filters which match a compound name.
	/// When implemented against raw files, this may have a performance impact on applications.
	/// For files which have a programmed event table, this will be fast,
	/// as the information can be taken directly from the events.
	/// If there is no event table, then event data is checked for every scan in the file (slower).
	/// </summary>
	/// <param name="compoundName">The compound name.</param>
	/// <returns>
	/// The array of matching scan filters (in string format).
	/// </returns>
	public string[] GetScanFiltersFromCompoundName(string compoundName)
	{
		return RequireMassSpec("GetScanFiltersFromCompoundName").GetScanFiltersFromCompoundName(compoundName).ToArray();
	}

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
	public string[][] GetScanFiltersFromCompoundNames(string[] compoundNames)
	{
		return RequireMassSpec("GetScanFiltersFromCompoundName").GetScanFiltersFromCompoundNames(compoundNames);
	}

	/// <summary>
	/// Gets the status log data, from all log entries, based on a specific position in the log.
	/// For example: "position" may be selected from one of the key value pairs returned from <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IRawDataPlus.StatusLogPlottableData" />
	/// in order to create a trend plot of a particular value.
	/// The interface returned has an array of retention times and strings.
	/// If the position was selected by using <see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IRawDataPlus.StatusLogPlottableData" />, then the strings may be converted "ToDouble" to get
	/// the set of numeric values to plot.
	/// </summary>
	/// <param name="position">The position within the list of available status log values.</param>
	/// <returns>
	/// An interface containing the times and logged values for the selected status log field.
	/// </returns>
	public ISingleValueStatusLog GetStatusLogAtPosition(int position)
	{
		RequireDevice("GetStatusLogAtPosition");
		return new WrappedSingleValueStatusLog(_selectedDevice.StatusLogEntries.GetItemValues(position));
	}

	/// <summary>
	/// Test if a scan passes a filter.
	/// If all matching scans in a file are required, consider using <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawDataPlus.GetFilteredScanEnumerator(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter)" /> or <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawDataPlus.GetFilteredScanEnumeratorOverTime(ThermoFisher.CommonCore.Data.Interfaces.IScanFilter,System.Double,System.Double)" />
	/// </summary>
	/// <param name="scan">the scan number</param>
	/// <param name="filter">the filter to test</param>
	/// <returns>
	/// True if this scan passes the filter
	/// </returns>
	public bool TestScan(int scan, string filter)
	{
		RequireMassSpec("TestScan");
		IScanFilter filterFromString = GetFilterFromString(filter);
		return TestScanInternal(scan, filterFromString);
	}

	/// <summary>
	/// Test if a scan passes a filter.
	/// </summary>
	/// <param name="scan">the scan number</param>
	/// <param name="filterFields">the filter to test</param>
	/// <returns>
	/// True if this scan passes the filter
	/// </returns>
	private bool TestScanInternal(int scan, IScanFilter filterFields)
	{
		bool accuratePrecursors = IsTsqQuantumFile();
		ScanFilterHelper filterHelper = new ScanFilterHelper(filterFields, accuratePrecursors, RunHeaderEx.FilterMassPrecision);
		return ScanEventHelper.ScanEventHelperFactory(GetScanEventForScanNumber(scan)).TestScanAgainstFilter(filterHelper);
	}

	/// <summary>
	/// Test if this file is from the Quantum family of MS
	/// </summary>
	/// <returns>true if quantum style file</returns>
	private bool IsTsqQuantumFile()
	{
		return GetInstrumentData().IsTsqQuantumFile();
	}

	/// <summary>
	/// Gets names of all instruments stored in the raw file's copy of the instrument method file.
	/// </summary>
	/// <returns>
	/// The instrument names.
	/// </returns>
	public string[] GetAllInstrumentNamesFromInstrumentMethod()
	{
		ValidateOpenFile();
		if (_rawFileLoader.RawFileInformation.HasExpMethod && _rawFileLoader.Header.Revision >= 25 && _rawFileLoader.MethodInfo?.StorageDescriptions != null && _rawFileLoader.MethodInfo.StorageDescriptions.Any())
		{
			return _rawFileLoader.MethodInfo.StorageDescriptions.Select((StorageDescription sd) => sd.StorageName).ToArray();
		}
		return new string[0];
	}

	/// <summary>
	/// Calculate the filters for this raw file, and return as an array
	/// </summary>
	/// <returns>
	/// Auto generated list of unique filters
	/// </returns>
	public ReadOnlyCollection<IScanFilter> GetFilters()
	{
		ValidateOpenFile();
		return RequireMassSpec("GetFilters").GetFilters().ToScanFilter();
	}

	/// <inheritdoc />
	public ReadOnlyCollection<IScanFilter> GetAccurateFilters(FilterPrecisionMode mode = FilterPrecisionMode.Instrument, int value = 2)
	{
		ValidateOpenFile();
		return RequireMassSpec("GetFilters").GetFilters(-1, -1, mode, value).ToScanFilter();
	}

	/// <summary>
	/// Calculate the filters for this raw file within the range of scans supplied, and return as an array
	/// </summary>
	/// <param name="startScan">First scan to analyze</param>
	/// <param name="endScan">Last scan to analyze</param>
	/// <returns>
	/// Auto generated list of unique filters
	/// </returns>
	public ReadOnlyCollection<IScanFilter> GetFiltersForScanRange(int startScan, int endScan)
	{
		ValidateOpenFile();
		return RequireMassSpec("GetFilters").GetFilters(startScan, endScan).ToScanFilter();
	}

	/// <inheritdoc />
	public ReadOnlyCollection<IScanFilter> GetAccurateFiltersForScanRange(int startScan, int endScan, FilterPrecisionMode mode = FilterPrecisionMode.Instrument, int value = 2)
	{
		ValidateOpenFile();
		return RequireMassSpec("GetFilters").GetFilters(startScan, endScan, mode, value).ToScanFilter();
	}

	/// <summary>
	/// Return the filter strings for this file
	/// </summary>
	/// <returns>
	/// A string for each auto filter from the raw file
	/// </returns>
	public string[] GetAutoFilters()
	{
		ValidateOpenFile();
		return RequireMassSpec("GetAutoFilters").GetAutoFilters();
	}

	/// <summary>
	/// Get the centroids saved with a profile scan
	/// </summary>
	/// <param name="scanNumber">Scan number</param>
	/// <param name="includeReferenceAndExceptionPeaks">determines if peaks flagged as ref should be returned</param>
	/// <returns>
	/// centroid stream for specified <paramref name="scanNumber" />.
	/// </returns>
	public CentroidStream GetCentroidStream(int scanNumber, bool includeReferenceAndExceptionPeaks)
	{
		MassSpecDevice massSpecDevice = RequireMassSpec("GetCentroidStream");
		PacketFeatures packetFeatures = PacketFeatures.All;
		packetFeatures -= 8;
		CentroidStream centroidStream = CentroidStreamFactory.CreateCentroidStream(massSpecDevice.GetLabelPeaks(scanNumber, includeReferenceAndExceptionPeaks, packetFeatures));
		centroidStream.ScanNumber = scanNumber;
		ScanEvent scanEvent = massSpecDevice.GetScanEvent(scanNumber);
		double[] array;
		if (scanEvent != null)
		{
			double[] massCalibrators = scanEvent.MassCalibrators;
			int num = massCalibrators.Length;
			array = new double[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = massCalibrators[i];
			}
		}
		else
		{
			array = Array.Empty<double>();
		}
		centroidStream.Coefficients = array;
		centroidStream.CoefficientsCount = array.Length;
		return centroidStream;
	}

	/// <summary>
	/// Get the advanced LT/FT formats data, such as the noise data, baseline data, label peaks and frequencies
	/// </summary>
	/// <param name="scanNumber">The scan number.</param>
	/// <returns>
	/// Returns an IAdvancedPacketData object which contains noise data, baseline data, label peaks and frequencies for specified <paramref name="scanNumber" />.
	/// It might return empty arrays for scans which do not have these data.
	/// </returns>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedMsDeviceException">Thrown if the selected device is not of type MS</exception>
	public IAdvancedPacketData GetAdvancedPacketData(int scanNumber)
	{
		return RequireMassSpec("GetAdvancedPacketData").GetAdvancedPacketData(scanNumber, _includeReferenceAndExceptionData);
	}

	/// <summary>
	/// Create a chromatogram from the data stream
	/// </summary>
	/// <param name="settings">Definition of how the chromatogram is read</param>
	/// <param name="startScan">First scan to read from. -1 for "all data"</param>
	/// <param name="endScan">Last scan to read from. -1 for "all data"</param>
	/// <param name="toleranceOptions">For mass range or base peak chromatograms,
	/// if the ranges have equal mass values,
	/// then <paramref name="toleranceOptions" /> are used to determine a band
	/// subtracted from low and added to high to search for matching masses</param>
	/// <returns>
	/// Chromatogram points
	/// </returns>
	public IChromatogramData GetChromatogramData(IChromatogramSettings[] settings, int startScan, int endScan, MassOptions toleranceOptions)
	{
		if (settings == null || settings.Contains(null))
		{
			throw new ArgumentNullException("settings");
		}
		RequireChromatographicDevice("GetChromatogramData");
		ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader deviceRunHeader = RawFileLoaderHelper.GetDeviceRunHeader(Manager, _rawFileLoader.Id, _selectedDevice);
		if (deviceRunHeader.LastSpectrum < 1)
		{
			return ChromatogramSignal.ToChromatogramData(null);
		}
		if (!FixScanLimits(deviceRunHeader, ref startScan, ref endScan))
		{
			throw new ArgumentOutOfRangeException("startScan", "Scan limits (startScan and endScan) out of range");
		}
		switch (_selectedInstrumentType)
		{
		case Device.MS:
			return CreateSimpleMsChromatogramsWithTolerance(settings, startScan, endScan, toleranceOptions, alwaysUseAccuratePrecursors: false);
		case Device.MSAnalog:
		case Device.Analog:
		case Device.UV:
		case Device.Pda:
			return new SimpleDetectorChromatogramBuilder(_selectedDevice).CreateChromatograms(settings, startScan, endScan);
		default:
			return null;
		}
	}

	/// <summary>
	/// Generate MS chromatograms, not returning base peak data.
	/// </summary>
	/// <param name="settings">Chromatogram settings</param>
	/// <param name="startScan">First scan in chromatogram</param>
	/// <param name="endScan">Last scan in chromatogram</param>
	/// <param name="toleranceOptions">Mass tolerance</param>
	/// <param name="alwaysUseAccuratePrecursors">If set: then precursor tolerance is based on
	/// the precision of the scan filters supplied
	/// (+/- half of the final digit).
	/// If not set, then precursors are matched based on settings logged by the device in the raw data</param>
	/// <returns>The chromatogram</returns>
	private IChromatogramData CreateSimpleMsChromatogramsWithTolerance(IChromatogramSettings[] settings, int startScan, int endScan, MassOptions toleranceOptions, bool alwaysUseAccuratePrecursors)
	{
		MassSpecDevice massSpecDevice = RequireMassSpec("CreateSimpleMsChromatogramsWithTolerance");
		ThermoFisher.CommonCore.Data.Business.Range timeRange = RetentionTimeRangeFromScans(startScan, endScan);
		ChromatogramDelivery[] array = massSpecDevice.CreateChromatograms(settings, timeRange, toleranceOptions, IncludeReferenceAndExceptionData, alwaysUseAccuratePrecursors);
		int num = array.Length;
		ChromatogramSignal[] array2 = new ChromatogramSignal[num];
		for (int i = 0; i < num; i++)
		{
			array2[i] = array[i].DeliveredSignal;
		}
		ApplyDelays(settings, array2);
		return ChromatogramSignal.ToChromatogramData(array2);
	}

	/// <summary>
	/// Create a chromatogram from the data stream
	/// </summary>
	/// <param name="settings">Definition of how the chromatogram is read</param>
	/// <param name="startScan">First scan to read from. -1 for "all data"</param>
	/// <param name="endScan">Last scan to read from. -1 for "all data"</param>
	/// <returns>
	/// Chromatogram points
	/// </returns>
	/// <exception cref="T:System.ArgumentNullException">Null settings argument.</exception>
	public IChromatogramData GetChromatogramData(IChromatogramSettings[] settings, int startScan, int endScan)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		RequireChromatographicDevice("GetChromatogramData");
		ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader deviceRunHeader = RawFileLoaderHelper.GetDeviceRunHeader(Manager, _rawFileLoader.Id, _selectedDevice);
		if (deviceRunHeader.LastSpectrum < 1)
		{
			return ChromatogramSignal.ToChromatogramData(null);
		}
		if (!FixScanLimits(deviceRunHeader, ref startScan, ref endScan))
		{
			throw new ArgumentOutOfRangeException("startScan", "Scan limits (startScan and endScan) out of range");
		}
		switch (_selectedInstrumentType)
		{
		case Device.MS:
			return CreateSimpleMsChromatograms(settings, startScan, endScan);
		case Device.MSAnalog:
		case Device.Analog:
		case Device.UV:
		case Device.Pda:
			return new SimpleDetectorChromatogramBuilder(_selectedDevice).CreateChromatograms(settings, startScan, endScan);
		default:
			return null;
		}
	}

	/// <summary>
	/// Create simple MS chromatograms.
	/// </summary>
	/// <param name="settings">
	/// The settings.
	/// </param>
	/// <param name="startScan">
	/// The start scan.
	/// </param>
	/// <param name="endScan">
	/// The end scan.
	/// </param>
	/// <returns>
	/// The chromatogram data
	/// </returns>
	private IChromatogramData CreateSimpleMsChromatograms(IChromatogramSettings[] settings, int startScan, int endScan)
	{
		MassSpecDevice massSpecDevice = RequireMassSpec("CreateSimpleMsChromatograms");
		ThermoFisher.CommonCore.Data.Business.Range timeRange = RetentionTimeRangeFromScans(startScan, endScan);
		ChromatogramDelivery[] array = massSpecDevice.CreateChromatograms(settings, timeRange, IncludeReferenceAndExceptionData);
		int num = array.Length;
		ChromatogramSignal[] array2 = new ChromatogramSignal[num];
		for (int i = 0; i < num; i++)
		{
			array2[i] = array[i].DeliveredSignal;
		}
		ApplyDelays(settings, array2);
		return ChromatogramSignal.ToChromatogramData(array2);
	}

	/// <summary>
	/// Apply delays to chromatograms, where specified.
	/// </summary>
	/// <param name="settings">
	/// The settings.
	/// </param>
	/// <param name="signal">
	/// The signal.
	/// </param>
	private void ApplyDelays(IChromatogramSettings[] settings, ChromatogramSignal[] signal)
	{
		int num = settings.Length;
		for (int i = 0; i < num; i++)
		{
			double delayInMin = settings[i].DelayInMin;
			if (Math.Abs(delayInMin) > 1E-10)
			{
				signal[i].Delay(delayInMin);
			}
		}
	}

	/// <summary>
	/// Get the number of instruments (data streams) of a certain classification.
	/// For example: the number of UV devices which logged data into this file.
	/// </summary>
	/// <param name="type">The device type to count</param>
	/// <returns>
	/// The number of devices of this type
	/// </returns>
	public int GetInstrumentCountOfType(Device type)
	{
		ValidateOpenFile();
		VirtualDeviceTypes type2 = type.ToVirtualDeviceType();
		return _rawFileLoader.RawFileInformation?.NumberOfVirtualControllersOfType(type2) ?? 0;
	}

	/// <summary>
	/// Gets the definition of the selected instrument.
	/// </summary>
	/// <returns>
	/// data about the selected instrument, for example the instrument name
	/// </returns>
	public InstrumentData GetInstrumentData()
	{
		RequireDevice("GetInstrumentData");
		return InstrumentDataConverter.CopyFrom(_selectedDevice.InstrumentId);
	}

	/// <summary>
	/// Get the device type for an instrument data stream
	/// </summary>
	/// <param name="index">The data stream</param>
	/// <returns>The device at type the index</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">thrown if index is negative or beyond
	/// the device count</exception>
	public Device GetInstrumentType(int index)
	{
		ValidateOpenFile();
		IRawFileInfo rawFileInformation = _rawFileLoader.RawFileInformation;
		if (index >= 0 && index < rawFileInformation.NumberOfVirtualControllers)
		{
			return rawFileInformation.VirtualControllers[index].VirtualDeviceType.ToDevice();
		}
		throw new ArgumentOutOfRangeException("index");
	}

	public IMsScanIndexAccess GetMsScanIndex(int scanNumber)
	{
		try
		{
			RequireMassSpec("GetMsScanIndexForScan");
			if (_selectedDevice.DeviceType == VirtualDeviceTypes.MsDevice && _selectedDevice.GetScanIndex(scanNumber) is ScanIndex result)
			{
				return result;
			}
		}
		catch (Exception)
		{
		}
		return GetScanStatsForScanNumber(scanNumber);
	}

	/// <summary>
	/// Get the scan statistics for a scan
	/// </summary>
	/// <param name="scanNumber">scan number</param>
	/// <returns>Statistics for scan</returns>
	public ScanStatistics GetScanStatsForScanNumber(int scanNumber)
	{
		try
		{
			RequireDevice("GetScanStatsForScanNumber");
			IScanIndex scanIndex = _selectedDevice.GetScanIndex(scanNumber);
			if (scanIndex == null)
			{
				return new ScanStatistics();
			}
			if (_selectedDevice.DeviceType == VirtualDeviceTypes.MsDevice)
			{
				IDevice selectedDevice = _selectedDevice;
				MassSpecDevice massSpecDevice = selectedDevice as MassSpecDevice;
				if (massSpecDevice != null)
				{
					Lazy<string> lazyScanType = new Lazy<string>(() => GetScanTypeAsString(scanNumber, massSpecDevice));
					return new WrappedScanStatistics(scanIndex as ScanIndex)
					{
						LazyScanType = lazyScanType
					};
				}
			}
			if (_selectedDevice.DeviceType == VirtualDeviceTypes.PdaDevice && _selectedDevice.GetPacket(scanNumber, _includeReferenceAndExceptionData) is AdjustableScanRateProfilePacket adjustableScanRateProfilePacket && adjustableScanRateProfilePacket.Indices.Count > 0)
			{
				AdjustableScanRateProfileIndex adjustableScanRateProfileIndex = adjustableScanRateProfilePacket.Indices[0];
				return new WrappedScanStatistics(scanIndex as ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex)
				{
					WavelengthStep = adjustableScanRateProfileIndex.TimeWavelengthStep,
					AbsorbanceUnitScale = adjustableScanRateProfileIndex.AbsorbanceUnitScale
				};
			}
			return new WrappedScanStatistics(scanIndex as ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex);
		}
		catch (Exception)
		{
		}
		return new ScanStatistics();
	}

	/// <summary>
	/// Gets the type of the scan.
	/// </summary>
	/// <param name="scanNum">The scan number.</param>
	/// <returns>The scan type, as string</returns>
	public string GetScanType(int scanNum)
	{
		ValidateOpenFile();
		MassSpecDevice massSpecDevice = RequireMassSpec("GetScanType");
		return GetScanTypeAsString(scanNum, massSpecDevice);
	}

	/// <summary>
	/// Gets the segment event table for the current instrument
	/// </summary>
	/// <returns>
	/// A two dimensional array of events. The first index is segment index (segment number-1).
	/// The second is event index (event number -1) within the segment.
	/// </returns>
	public string[][] GetSegmentEventTable()
	{
		MassSpecDevice massSpecDevice = RequireMassSpec("GetSegmentEventTable");
		if (massSpecDevice.NumScanEventSegments == 0)
		{
			return new string[0][];
		}
		int numScanEventSegments = massSpecDevice.NumScanEventSegments;
		string[][] array = new string[numScanEventSegments][];
		for (int i = 0; i < numScanEventSegments; i++)
		{
			List<ScanEvent> scanEvents = massSpecDevice.GetScanEvents(i);
			if (scanEvents != null && scanEvents.Any())
			{
				int count = scanEvents.Count;
				array[i] = new string[count];
				for (int j = 0; j < count; j++)
				{
					array[i][j] = scanEvents[j].ToString();
				}
			}
		}
		return array;
	}

	/// <summary>
	/// Get a segmented scan. This is the primary scan from the raw file.
	/// FT instrument files (such as Calcium) will have a second format of the scan (a centroid stream)
	/// </summary>
	/// <param name="scanNumber">Scan number to read</param>
	/// <param name="stats">statistics for the scan</param>
	/// <returns>The segmented scan</returns>
	public SegmentedScan GetSegmentedScanFromScanNumber(int scanNumber, ScanStatistics stats)
	{
		try
		{
			if (stats != null)
			{
				GetScanStatsForScanNumber(scanNumber).CopyTo(stats);
			}
			RequireDevice("GetSegmentedScanFromScanNumber");
			IReadOnlyList<SegmentData> segmentPeaks;
			int numSegments;
			int numAllPeaks;
			IPacket packet;
			switch (_selectedVirtualController.VirtualDeviceType)
			{
			case VirtualDeviceTypes.MsDevice:
			case VirtualDeviceTypes.MsAnalogDevice:
			case VirtualDeviceTypes.AnalogDevice:
			case VirtualDeviceTypes.UvDevice:
				segmentPeaks = _selectedDevice.GetSegmentPeaks(scanNumber, out numSegments, out numAllPeaks, out packet, _includeReferenceAndExceptionData);
				break;
			case VirtualDeviceTypes.PdaDevice:
				segmentPeaks = _selectedDevice.GetSegmentPeaks(scanNumber, out numSegments, out numAllPeaks, out packet, _includeReferenceAndExceptionData);
				if (packet is AdjustableScanRateProfilePacket adjustableScanRateProfilePacket && stats != null && adjustableScanRateProfilePacket.Indices.Count > 0)
				{
					stats.WavelengthStep = adjustableScanRateProfilePacket.Indices[0].TimeWavelengthStep;
					stats.AbsorbanceUnitScale = adjustableScanRateProfilePacket.Indices[0].AbsorbanceUnitScale;
				}
				break;
			default:
				throw new ArgumentException("settings are not valid for selected instrument");
			}
			return FormatSegmentedScan(scanNumber, numSegments, numAllPeaks, segmentPeaks);
		}
		catch (Exception)
		{
		}
		return new SegmentedScan();
	}

	internal static SegmentedScan FormatSegmentedScan(int scanNumber, int numSegments, int numAllPeaks, IReadOnlyList<SegmentData> segPeaks)
	{
		int[] array = new int[numSegments];
		ThermoFisher.CommonCore.Data.Business.Range[] array2 = new ThermoFisher.CommonCore.Data.Business.Range[numSegments];
		double[] array3 = new double[numAllPeaks];
		double[] array4 = new double[numAllPeaks];
		PeakOptions[] array5 = new PeakOptions[numAllPeaks];
		int num = 0;
		if (numAllPeaks <= 0)
		{
			for (int i = 0; i < numSegments; i++)
			{
				SegmentData segmentData = segPeaks[i];
				MassRangeStruct massRange = segmentData.MassRange;
				array[i] = segmentData.DataPeaks.Count;
				array2[i] = new ThermoFisher.CommonCore.Data.Business.Range(massRange.LowMass, massRange.HighMass);
			}
			array3 = Array.Empty<double>();
			array4 = Array.Empty<double>();
		}
		else
		{
			for (int j = 0; j < numSegments; j++)
			{
				SegmentData segmentData2 = segPeaks[j];
				MassRangeStruct massRange2 = segmentData2.MassRange;
				List<DataPeak> dataPeaks = segmentData2.DataPeaks;
				int num2 = (array[j] = dataPeaks.Count);
				array2[j] = new ThermoFisher.CommonCore.Data.Business.Range(massRange2.LowMass, massRange2.HighMass);
				int num3 = num2 - 2;
				int k;
				for (k = 0; k < num3; k += 3)
				{
					DataPeak dataPeak = dataPeaks[k];
					array3[num] = dataPeak.Position;
					array4[num] = dataPeak.Intensity;
					array5[num] = dataPeak.Options;
					dataPeak = dataPeaks[k + 1];
					array3[++num] = dataPeak.Position;
					array4[num] = dataPeak.Intensity;
					array5[num] = dataPeak.Options;
					dataPeak = dataPeaks[k + 2];
					array3[++num] = dataPeak.Position;
					array4[num] = dataPeak.Intensity;
					array5[num++] = dataPeak.Options;
				}
				for (; k < num2; k++)
				{
					DataPeak dataPeak2 = dataPeaks[k];
					array3[num] = dataPeak2.Position;
					array4[num] = dataPeak2.Intensity;
					array5[num] = dataPeak2.Options;
					num++;
				}
			}
		}
		return new SegmentedScan
		{
			SegmentCount = numSegments,
			SegmentSizes = array,
			Ranges = array2,
			Positions = array3,
			Intensities = array4,
			Flags = array5,
			PositionCount = numAllPeaks,
			ScanNumber = scanNumber
		};
	}

	/// <summary>
	/// Require a device to have been selected.
	/// </summary>
	/// <param name="reason">
	/// The reason.
	/// </param>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown in no device has been selected
	/// </exception>
	private void RequireDevice(string reason)
	{
		if (_selectedDevice == null || _selectedVirtualController.VirtualDeviceType == VirtualDeviceTypes.NoDevice)
		{
			throw new NoSelectedDeviceException(reason);
		}
	}

	/// <summary>
	/// Require a chromatographic device to have been selected.
	/// </summary>
	/// <param name="reason">
	/// The reason.
	/// </param>
	/// <exception cref="T:ThermoFisher.CommonCore.Data.Business.NoSelectedDeviceException">Thrown in no chromatographic device has been selected
	/// </exception>
	private void RequireChromatographicDevice(string reason)
	{
		if (_selectedDevice == null || _selectedInstrumentType == Device.None || _selectedInstrumentType == Device.Other)
		{
			throw new RequiresChromatographicDeviceException(reason);
		}
	}

	/// <summary>
	/// returns the number of entries in the current instrument's status log
	/// </summary>
	/// <returns>the number of entries in the current instrument's status log</returns>
	public int GetStatusLogEntriesCount()
	{
		RequireDevice("GetStatusLogEntriesCount");
		return _selectedDevice.StatusLogEntries?.Count ?? 0;
	}

	/// <summary>
	/// Gets the status log for retention time.
	/// </summary>
	/// <param name="retentionTime">The retention time.</param>
	/// <returns>
	/// <see cref="T:ThermoFisher.CommonCore.Data.Business.LogEntry" /> object containing status log information.
	/// </returns>
	public ILogEntryAccess GetStatusLogForRetentionTime(double retentionTime)
	{
		RequireDevice("GetStatusLogForRetentionTime");
		return new WrappedLogEntry(_selectedDevice.StatusLogEntries.GetItem(retentionTime));
	}

	/// <summary>
	/// Returns the header information for the current instrument's status log
	/// </summary>
	/// <returns>
	/// The headers (list of prefixes for the strings).
	/// </returns>
	public HeaderItem[] GetStatusLogHeaderInformation()
	{
		RequireDevice("GetStatusLogHeaderInformation");
		IStatusLog statusLogEntries = _selectedDevice.StatusLogEntries;
		if (CheckEmptyStatusLog(statusLogEntries))
		{
			return new HeaderItem[0];
		}
		return new WrappedHeaderItems(statusLogEntries.DataDescriptors).ToArray();
	}

	/// <summary>
	/// check for empty status log.
	/// </summary>
	/// <param name="logEntries">
	/// The log entries.
	/// </param>
	/// <returns>
	/// true if empty
	/// </returns>
	private bool CheckEmptyStatusLog(IStatusLog logEntries)
	{
		if (logEntries.DataDescriptors != null)
		{
			return logEntries.DataDescriptors.Count == 0;
		}
		return true;
	}

	/// <summary>
	/// Returns the Status log values for the current instrument
	/// </summary>
	/// <param name="statusLogIndex">Index into table of status logs</param>
	/// <param name="ifFormatted">true if they should be formatted (recommended for display).
	/// Unformatted values can be returned with default precision (for float or double)
	/// Which may be better for graphing</param>
	/// <returns>
	/// The status log values.
	/// </returns>
	public StatusLogValues GetStatusLogValues(int statusLogIndex, bool ifFormatted)
	{
		RequireDevice("GetStatusLogValues");
		StatusLogEntry statusRecordByIndex = _selectedDevice.StatusLogEntries.GetStatusRecordByIndex(statusLogIndex);
		List<LabelValuePair> valuePairs = statusRecordByIndex.ValuePairs;
		int count = valuePairs.Count;
		string[] array = new string[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = valuePairs[i].Value.ToString(ifFormatted);
		}
		return new StatusLogValues
		{
			RetentionTime = statusRecordByIndex.RetentionTime,
			Values = array
		};
	}

	/// <summary>
	/// Gets the trailer extra header information. This is common across all scan numbers
	/// </summary>
	/// <returns>
	/// The headers.
	/// </returns>
	public HeaderItem[] GetTrailerExtraHeaderInformation()
	{
		DataDescriptors trailerExtrasDataDescriptors = RequireMassSpec("GetTrailerExtraHeaderInformation").TrailerExtrasDataDescriptors;
		if (trailerExtrasDataDescriptors == null || trailerExtrasDataDescriptors.Count == 0)
		{
			return new HeaderItem[0];
		}
		return new WrappedHeaderItems(trailerExtrasDataDescriptors).ToArray();
	}

	/// <summary>
	/// Gets the array of headers and values for this scan number. The values are formatted as per the header settings.
	/// </summary>
	/// <param name="scanNumber">The scan for which this information is needed</param>
	/// <returns>
	/// Extra information about the scan
	/// </returns>
	public ILogEntryAccess GetTrailerExtraInformation(int scanNumber)
	{
		return new WrappedLogEntry(RequireMassSpec("GetTrailerExtraInformation").GetTrailerExtra(scanNumber));
	}

	/// <summary>
	/// returns the Trailer Extra values for the specified scan number. 
	/// </summary>
	/// <param name="scanNumber">scan who's data is needed</param>
	/// <param name="ifFormatted">If true, then the values will be formatted as per the header settings.
	/// If false, then numeric values have default formatting (generally more precision)</param>
	/// <returns>string representation of the scan trailer information</returns>
	public string[] GetTrailerExtraValues(int scanNumber, bool ifFormatted)
	{
		List<LabelValuePair> trailerExtra = RequireMassSpec("GetTrailerExtraValues").GetTrailerExtra(scanNumber);
		int count = trailerExtra.Count;
		string[] array = new string[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = trailerExtra[i].Value.ToString(ifFormatted);
		}
		return array;
	}

	/// <summary>
	/// returns the Trailer Extra value for a specific field in the specified scan number. 
	/// The object type depends on the field type.
	/// </summary>
	/// <param name="scanNumber">scan who's data is needed</param>
	/// <param name="field">zero based filed number in the record, as per header </param>
	/// <returns>Value of requested field</returns>
	public object GetTrailerExtraValue(int scanNumber, int field)
	{
		return RequireMassSpec("GetTrailerExtraValue").GetTrailerExtraValue(scanNumber, field);
	}

	/// <summary>
	/// returns the Trailer Extra values for all fields in the specified scan number. 
	/// The object types depend on the field types.
	/// </summary>
	/// <param name="scanNumber">scan who's data is needed</param>
	/// <returns>Value of requested field</returns>
	public object[] GetTrailerExtraValues(int scanNumber)
	{
		return RequireMassSpec("GetTrailerExtraValue").GetTrailerExtraValues(scanNumber);
	}

	/// <summary>
	/// Gets the tune data.
	/// </summary>
	/// <param name="tuneDataIndex">Index of the tune data.</param>
	/// <returns>The tune data log</returns>
	public ILogEntryAccess GetTuneData(int tuneDataIndex)
	{
		return new WrappedLogEntry(RequireMassSpec("GetTuneData").GetTuneData(tuneDataIndex));
	}

	/// <summary>
	/// Gets the tune data values, as an array of objects
	/// with types as per the tune data header.
	/// </summary>
	/// <param name="tuneDataIndex">Index of the tune data.</param>
	/// <returns>The tune data values (as objects)</returns>
	public object[] GetTuneDataValues(int tuneDataIndex)
	{
		return RequireMassSpec("GetTuneData").GetTuneDataValues(tuneDataIndex);
	}

	/// <summary>
	/// Gets the (raw) status log data at a given index in the log.
	/// Designed for efficiency, this method does not convert logs to display string format.
	/// </summary>
	/// <param name="index">Index (from 0 to "RunHeaderEx.StatusLogCount"-1)</param>
	/// <returns>Log data at the given index</returns>
	public IStatusLogEntry GetStatusLogEntry(int index)
	{
		RequireDevice("GetStatusLogEntry");
		return _selectedDevice.StatusLogEntries.GetStatusLogEntryByIndex(index);
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
		RequireDevice("GetStatusLogEntry");
		return _selectedDevice.StatusLogEntries.GetSortedStatusLogEntryByIndex(index);
	}

	/// <summary>
	/// Gets the (raw) status log data at a given retention time in the log.
	/// Designed for efficiency, this method does not convert logs to display string format.
	/// </summary>
	/// <param name="retentionTime">Retention time/</param>
	/// <returns>Log data at the given time</returns>
	public IStatusLogEntry GetStatusLogEntry(double retentionTime)
	{
		RequireDevice("GetStatusLogEntry");
		return _selectedDevice.StatusLogEntries.GetStatusLogEntryByRetentionTime(retentionTime);
	}

	/// <summary>
	/// return the number of tune data entries
	/// </summary>
	/// <returns>
	/// The number of tune methods.
	/// </returns>
	public int GetTuneDataCount()
	{
		RequireDevice("GetTuneDataCount");
		return RunHeaderEx.TuneDataCount;
	}

	/// <summary>
	/// Return the header information for the current instrument's tune data
	/// </summary>
	/// <returns>
	/// The format definition for tune data.
	/// </returns>
	public HeaderItem[] GetTuneDataHeaderInformation()
	{
		DataDescriptors tuneMethodDataDescriptors = RequireMassSpec("GetTuneDataHeaderInformation").TuneMethodDataDescriptors;
		if (tuneMethodDataDescriptors == null || tuneMethodDataDescriptors.Count == 0)
		{
			return new HeaderItem[0];
		}
		return new WrappedHeaderItems(tuneMethodDataDescriptors).ToArray();
	}

	/// <summary>
	/// return tune data values for the specified index
	/// </summary>
	/// <param name="tuneDataIndex">index into tune tables</param>
	/// <param name="ifFormatted">true if formatting should be done</param>
	/// <returns>
	/// The tune data.
	/// </returns>
	public TuneDataValues GetTuneDataValues(int tuneDataIndex, bool ifFormatted)
	{
		List<LabelValuePair> tuneData = RequireMassSpec("GetTuneDataValues").GetTuneData(tuneDataIndex);
		int count = tuneData.Count;
		string[] array = new string[count];
		for (int i = 0; i < count; i++)
		{
			array[i] = tuneData[i].Value.ToString(ifFormatted);
		}
		return new TuneDataValues
		{
			ID = tuneDataIndex,
			Values = array
		};
	}

	/// <summary>
	/// Gets an instrument method.
	/// </summary>
	/// <param name="index">The index.</param>
	/// <returns>
	/// A text version of the method
	/// </returns>
	public string GetInstrumentMethod(int index)
	{
		ValidateOpenFile();
		if (_rawFileLoader.RawFileInformation.HasExpMethod && _rawFileLoader.Header.Revision >= 25 && _rawFileLoader.MethodInfo?.StorageDescriptions != null && index < _rawFileLoader.MethodInfo.StorageDescriptions.Count)
		{
			return _rawFileLoader.MethodInfo.StorageDescriptions[index].MethodText;
		}
		return string.Empty;
	}

	/// <summary>
	/// Test if a scan is centroid format
	/// </summary>
	/// <param name="scanNumber">Number of the scan</param>
	/// <returns>
	/// True if the scan is centroid format
	/// </returns>
	public bool IsCentroidScanFromScanNumber(int scanNumber)
	{
		RequireDevice("IsCentroidScanFromScanNumber");
		return GetScanStatsForScanNumber(scanNumber).IsCentroidScan;
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
		if (_rawFileLoader != null)
		{
			return _rawFileLoader.RefreshViewOfFile();
		}
		return false;
	}

	/// <summary>
	/// Get the retention time (minutes) from a scan number
	/// </summary>
	/// <param name="scanNumber">Scan number</param>
	/// <returns>
	/// Retention time (start time) of scan
	/// </returns>
	public double RetentionTimeFromScanNumber(int scanNumber)
	{
		RequireDevice("RetentionTimeFromScanNumber");
		return _selectedDevice.GetRetentionTime(scanNumber);
	}

	/// <summary>
	/// Get a scan number from a retention time
	/// </summary>
	/// <param name="time">Retention time (minutes)</param>
	/// <returns>
	/// Scan number in the data stream for this time.
	/// </returns>
	public int ScanNumberFromRetentionTime(double time)
	{
		try
		{
			int firstSpectrum = RunHeader.FirstSpectrum;
			int lastSpectrum = RunHeader.LastSpectrum;
			if (firstSpectrum >= 1 && lastSpectrum >= firstSpectrum)
			{
				double num = _selectedDevice.GetRetentionTime(firstSpectrum);
				if (num >= time)
				{
					return firstSpectrum;
				}
				double num2 = _selectedDevice.GetRetentionTime(lastSpectrum);
				if (num2 <= time)
				{
					return lastSpectrum;
				}
				int num3 = lastSpectrum;
				int num4 = firstSpectrum;
				while (num3 > num4 + 1)
				{
					int num5 = (num3 + num4) / 2;
					double retentionTime = _selectedDevice.GetRetentionTime(num5);
					if (time >= retentionTime)
					{
						num4 = num5;
						num = retentionTime;
					}
					else
					{
						num3 = num5;
						num2 = retentionTime;
					}
				}
				double num6 = Math.Abs(time - num2);
				double num7 = Math.Abs(time - num);
				if (num6 > num7)
				{
					return num4;
				}
				return num3;
			}
			return -1;
		}
		catch (Exception)
		{
			return -1;
		}
	}

	/// <summary>
	/// Sets the current instrument in the raw file.
	/// This method must be called before subsequent calls to access data specific 
	/// to an instrument (e.g. MS or UV data) may be made. All requests for data specific 
	/// to an instrument will be forwarded to the current instrument until the current 
	/// instrument is changed. The instrument number is used to indicate which instrument 
	/// to use if there are more than one registered instruments of the same type (e.g. multiple UV detectors). 
	/// Instrument numbers for each type are numbered starting at 1. 
	/// </summary>
	/// <param name="instrumentType">Type of instrument</param>
	/// <param name="instrumentIndex">Stream number</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException"><c></c> is out of range.</exception>
	public void SelectInstrument(Device instrumentType, int instrumentIndex)
	{
		if (instrumentIndex < 1)
		{
			throw new ArgumentOutOfRangeException("instrumentIndex", "Instrument index must be >= 1");
		}
		if (_selectedInstrumentIndex != instrumentIndex || _selectedInstrumentType != instrumentType)
		{
			IDevice selectedDevice;
			VirtualControllerInfo virtualControllerInfo = _rawFileLoader.GetVirtualControllerInfo(instrumentType, instrumentIndex, _rawFileLoader.RawFileInformation.VirtualControllers, _rawFileLoader.Devices, out selectedDevice);
			if (virtualControllerInfo == null || selectedDevice == null)
			{
				throw new ArgumentOutOfRangeException("instrumentIndex", "Instrument index not available for requested device");
			}
			_selectedInstrumentIndex = instrumentIndex;
			_selectedInstrumentType = instrumentType;
			_selectedVirtualController = virtualControllerInfo;
			_selectedDevice = selectedDevice;
		}
	}

	/// <summary>
	/// Count the number currently in the cache
	/// </summary>
	/// <param name="item">Item type to count</param>
	/// <returns>
	/// The number of items in this cache
	/// </returns>
	public int Cached(RawCacheItem item)
	{
		return 0;
	}

	/// <summary>
	/// Clear items in the cache
	/// </summary>
	/// <param name="item">item type to clear</param>
	public void ClearCache(RawCacheItem item)
	{
	}

	/// <summary>
	/// Request the object to keep a cache of the listed item.
	/// Setting the caching to "zero" disables further caching.
	/// </summary>
	/// <param name="item">Item to cache</param>
	/// <param name="limit">Limit of number of items to cache</param>
	/// <param name="useCloning">(optional, default false) if set True, all values returned from the cache are unique  (cloned) references.
	/// By default, the cache just keeps references to the objects</param>
	public void SetCaching(RawCacheItem item, int limit, bool useCloning = false)
	{
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
	/// <returns>
	/// An array of scan events
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">firstScanNumber;First scan must not be below Last scan</exception>
	public IScanEvent[] GetScanEvents(int firstScanNumber, int lastScanNumber)
	{
		RequireMassSpec("GetScanEvents");
		ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader deviceRunHeader = RawFileLoaderHelper.GetDeviceRunHeader(Manager, _rawFileLoader.Id, _selectedDevice);
		if (deviceRunHeader.LastSpectrum < 1)
		{
			return Array.Empty<IScanEvent>();
		}
		if (!FixScanLimits(deviceRunHeader, ref firstScanNumber, ref lastScanNumber))
		{
			throw new ArgumentOutOfRangeException("firstScanNumber", "First scan must not be above Last scan");
		}
		int num = lastScanNumber + 1 - firstScanNumber;
		IScanEvent[] toReturn = new IScanEvent[num];
		Parallel.For(firstScanNumber, lastScanNumber + 1, delegate(int index)
		{
			toReturn[index - firstScanNumber] = GetScanEventForScanNumber(index);
		});
		return toReturn;
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
		return RequireMassSpec("GetSimplifiedCentroids").GetSimplifiedLabels(scanNumber, IncludeReferenceAndExceptionData);
	}

	/// <summary>
	/// This method is similar to GetSegmentedScanFromScanNumber in the IRawData interface.
	/// The method returns only the mass and intensity values from
	/// the scan data for a scan. 
	/// Values for flags etc. are not returned, saving data space and improving efficiency.
	/// This method never returns "reference and exception peak" data.
	/// The method is designed for improved performance in custom XIC generators.
	/// </summary>
	/// <param name="scanNumber">The scan who's mass intensity data are needed</param>
	/// <returns>Mass and intensity values from the scan.</returns>
	public ISimpleScanAccess GetSimplifiedScan(int scanNumber)
	{
		int numSegments;
		int numAllPeaks;
		IPacket packet;
		IReadOnlyList<SegmentData> segmentPeaks = RequireMassSpec("GetSimplifiedScan").GetSegmentPeaks(scanNumber, out numSegments, out numAllPeaks, out packet, includeReferenceAndExceptionData: false);
		return SegmentedDataToSimpleScan(numAllPeaks, segmentPeaks);
	}

	internal static ISimpleScanAccess SegmentedDataToSimpleScan(int numAllPeaks, IReadOnlyList<SegmentData> dataPeaks)
	{
		double[] array = Array.Empty<double>();
		double[] array2 = Array.Empty<double>();
		if (numAllPeaks > 0)
		{
			array = new double[numAllPeaks];
			array2 = new double[numAllPeaks];
			int num = 0;
			foreach (SegmentData dataPeak2 in dataPeaks)
			{
				List<DataPeak> dataPeaks2 = dataPeak2.DataPeaks;
				int num2 = dataPeaks2.Count - 4;
				int i;
				for (i = 0; i < num2; i += 5)
				{
					DataPeak dataPeak = dataPeaks2[i];
					array[num] = dataPeak.Position;
					array2[num++] = dataPeak.Intensity;
					dataPeak = dataPeaks2[i + 1];
					array[num] = dataPeak.Position;
					array2[num++] = dataPeak.Intensity;
					dataPeak = dataPeaks2[i + 2];
					array[num] = dataPeak.Position;
					array2[num++] = dataPeak.Intensity;
					dataPeak = dataPeaks2[i + 3];
					array[num] = dataPeak.Position;
					array2[num++] = dataPeak.Intensity;
					dataPeak = dataPeaks2[i + 4];
					array[num] = dataPeak.Position;
					array2[num++] = dataPeak.Intensity;
				}
				for (; i < dataPeaks2.Count; i++)
				{
					DataPeak dataPeak = dataPeaks2[i];
					array[num] = dataPeak.Position;
					array2[num] = dataPeak.Intensity;
					num++;
				}
			}
		}
		return new SimpleScan
		{
			Masses = array,
			Intensities = array2
		};
	}

	/// <summary>
	/// Gets additional (binary) data from a scan.
	/// The format of this data is custom (per instrument) and can be decoded into
	/// objects by a specific decoder for the detector type.
	/// </summary>
	/// <param name="scan">Scan whose data is needed</param>
	public byte[] GetAdditionalScanData(int scan)
	{
		return RequireMassSpec("GetAdditionalScanData").GetAdditionalScanData(scan);
	}

	/// <summary>
	/// Gets additional (binary) data from a scan.
	/// The format of this data is custom (per instrument) and can be decoded into
	/// objects by a specific decoder for the detector type.
	/// </summary>
	/// <param name="scan">Scan whose data is needed</param>
	public IExtendedScanData GetExtendedScanData(int scan)
	{
		return RequireMassSpec("GetExtendedScanData").GetExtendedScanData(scan);
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public virtual void Dispose()
	{
		if (_rawFileLoader != null)
		{
			int num = _rawFileLoader.RemoveUse();
			if (_disposePermitted && num <= 0)
			{
				_rawFileLoader.Dispose();
				_rawFileLoader = null;
			}
		}
	}

	/// <summary>
	/// Permits derived (internal) classes to indicate that the loader is no longer needed,
	/// without having to create public or protected access to the loader.
	/// </summary>
	protected void DisposeOfLoader()
	{
		if (_rawFileLoader != null)
		{
			_disposePermitted = true;
			if (_rawFileLoader.RemoveUse() <= 0)
			{
				_rawFileLoader.Dispose();
				_rawFileLoader = null;
			}
		}
	}

	public string[] GetCompoundNamesForScanRange(int startScan, int endScan)
	{
		return RequireMassSpec("GetCompoundNamesForScanRange").GetCompoundNamesForScanRange(startScan, endScan).ToArray();
	}

	/// <summary>
	/// Gets a scan index and the event for a scan
	/// </summary>
	/// <param name="scan">scan number</param>
	/// <returns>scan index and scan event</returns>
	public IIndexAndEvent ReadEventAndIndex(int scan)
	{
		MassSpecDevice massSpecDevice = RequireMassSpec("GetMsScanIndexForScan");
		if (massSpecDevice.GetScanIndex(scan) is ScanIndex scanIndex)
		{
			return new IndexAndEvent
			{
				ScanIndex = scanIndex,
				ScanEvent = massSpecDevice.ScanEventWithValidScanNumber(scanIndex)
			};
		}
		return null;
	}

	/// <inheritdoc />
	public IDetectorReader GetDetectorReader(IInstrumentSelectionAccess detector, bool includeReferenceAndExceptionPeaks)
	{
		RawFileAccessBase rawFileAccessBase = new RawFileAccessBase(_rawFileLoader);
		rawFileAccessBase.SelectInstrument(detector.DeviceType, detector.InstrumentIndex);
		rawFileAccessBase.IncludeReferenceAndExceptionData = includeReferenceAndExceptionPeaks;
		return rawFileAccessBase;
	}

	/// <inheritdoc />
	public byte[] ReadScanBinaryData(int scan)
	{
		return RequireMassSpec("ReadScanBinaryData").GetBinaryScanData(scan);
	}

	/// <inheritdoc />
	public IEncodedScan ReadEncodedScan(int scan)
	{
		IIndexAndEvent indexAndEvent = ReadEventAndIndex(scan);
		IScanEvent scanEvent = indexAndEvent.ScanEvent;
		int massCalibratorCount = scanEvent.MassCalibratorCount;
		double[] array = new double[massCalibratorCount];
		for (int i = 0; i < massCalibratorCount; i++)
		{
			array[i] = scanEvent.GetMassCalibrator(i);
		}
		return new EncodedScan
		{
			ScanData = ReadScanBinaryData(scan),
			ScanIndex = indexAndEvent.ScanIndex,
			MassCalibrators = array
		};
	}

	/// <summary>Gets the trailer scan event indices information.</summary>
	/// <returns>
	///   List of trailer scan event indices information
	/// </returns>
	public List<object> GetTrailerScanEventIndicesInfo()
	{
		ValidateOpenFile();
		MassSpecDevice massSpecDevice = RequireMassSpec("GetTrailerScanEventIndicesInfo");
		return new List<object>
		{
			massSpecDevice.GetAllUniqueTrailerScanEventIndices(),
			massSpecDevice.GetAllIndicesToUniqueTrailerScanEvents(),
			massSpecDevice.GetEventAddressTable()
		};
	}
}
