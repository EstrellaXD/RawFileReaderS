using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.DataModel;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.RunHeaders;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.PROFSP;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

/// <summary>
/// The mass spectrometer device.
/// </summary>
internal class MassSpecDevice : DeviceBase, IRecordRangeProvider
{
	/// <summary>
	/// Advanced LT/FT formats only.
	/// </summary>
	/// <seealso cref="T:ThermoFisher.CommonCore.Data.Interfaces.IAdvancedPacketData" />
	internal class AdvancedPacketData : IAdvancedPacketData
	{
		private readonly Lazy<CentroidStream> _centroidData;

		private readonly Lazy<double[]> _frequencies;

		/// <summary>
		/// Gets the centroid data information.
		/// </summary>
		public CentroidStream CentroidData => _centroidData.Value;

		/// <summary>
		/// Gets the frequencies
		/// </summary>
		public double[] Frequencies => _frequencies.Value;

		/// <summary>
		/// Gets or sets the noise data - noise and baseline.
		/// </summary>
		public NoiseAndBaseline[] NoiseData { get; internal set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.MassSpecDevice.AdvancedPacketData" /> class.
		/// </summary>
		/// <param name="centroidReader">
		/// The centroid reader.
		/// </param>
		/// <param name="frequencies">
		/// The frequencies.
		/// </param>
		public AdvancedPacketData(Lazy<CentroidStream> centroidReader, Lazy<double[]> frequencies)
		{
			_centroidData = centroidReader;
			_frequencies = frequencies;
		}
	}

	/// <summary>
	/// Provides additional rules to compare events, for auto filter.
	/// </summary>
	internal class SmartFilterComparer : IComparer<FilterScanEvent>
	{
		public bool UsePrecursorTolerance { get; set; }

		public double ToleranceFactor { get; set; }

		public SmartFilterComparer(bool usePrecursorTolerance, double toleranceFactor)
		{
			UsePrecursorTolerance = usePrecursorTolerance;
			ToleranceFactor = toleranceFactor;
		}

		public int Compare(FilterScanEvent x, FilterScanEvent y)
		{
			return x.CompareSmart(y, UsePrecursorTolerance, ToleranceFactor);
		}
	}

	/// <summary>
	/// Class to permit parallel conversion of scan events.
	/// (For research: Can be made to implement IExecute to compare performance of
	/// parallel small tasks)
	/// </summary>
	private class EventConvert
	{
		/// <summary>
		/// Gets or sets the event to be converted.
		/// </summary>
		public ScanEvent TheEvent { get; set; }

		/// <summary>
		/// Gets or sets the converted filter.
		/// </summary>
		public FilterScanEvent TheFilter { get; set; }
	}

	/// <summary>
	/// Class to convert filters into strings.
	/// Designed to permit parallel conversions.
	/// </summary>
	private class FilterConvert : IExecute
	{
		private readonly IFilterScanEvent _originalFilter;

		/// <summary>
		/// Gets the converted filter.
		/// </summary>
		public string ConvertedFilter { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.MassSpecDevice.FilterConvert" /> class.
		/// </summary>
		/// <param name="toConvert">
		/// The event to convert.
		/// </param>
		public FilterConvert(IFilterScanEvent toConvert)
		{
			_originalFilter = toConvert;
		}

		/// <summary>
		/// execute the conversion
		/// </summary>
		public void Execute()
		{
			ConvertedFilter = _originalFilter.ToString();
		}
	}

	private IReadWriteAccessor _acqScanEventViewer;

	private IReadWriteAccessor _acqScanIndexViewer;

	private bool _disposed;

	private TrailerScanEvents _externalEvents;

	private bool _externalSetScanEvents;

	private Lazy<ScanIndex[]> _massSpecScanList;

	private BufferInfo _peakDataBufferInfo;

	private RunHeaderStruct _runHeaderStruct;

	private BufferInfo _scanEventsBufferInfo;

	private List<List<ScanEvent>> _scanEventSegments = new List<List<ScanEvent>>();

	private BufferInfo _scanHeaderBufferInfo;

	private ScanIndices _scanIndices;

	private BufferInfo _trailerEventBufferInfo;

	private BufferInfo _trailerExtraBufferInfo;

	private GenericDataCollection _trailerExtras;

	private BufferInfo _trailerHeaderBufferInfo;

	private Lazy<TrailerScanEvents> _trailerScanEventsLazy;

	private GenericDataCollection _tuneData;

	private BufferInfo _tuneDataBufferInfo;

	private BufferInfo _tuneHeaderBufferInfo;

	private ScanIndex[] _previousMsScanList;

	private RecordBufferManager _bufferManager;

	private readonly bool _inAcquisition;

	private readonly bool _oldRev;

	private readonly string _rawFileName;

	public RawDataDomain DataDomain { get; }

	/// <summary>
	/// Gets the number scan events.
	/// </summary>
	/// <value>
	/// The number scan events.
	/// </value>
	public int NumScanEvents
	{
		get
		{
			int num = 0;
			if (NumScanEventSegments > 0)
			{
				foreach (List<ScanEvent> scanEventSegment in _scanEventSegments)
				{
					if (scanEventSegment != null && scanEventSegment.Any())
					{
						num += scanEventSegment.Count;
					}
				}
			}
			return num;
		}
	}

	/// <summary>
	/// Gets the number scan event segments.
	/// </summary>
	/// <value>
	/// The number scan event segments.
	/// </value>
	public int NumScanEventSegments
	{
		get
		{
			if (_scanEventSegments != null && _scanEventSegments.Any())
			{
				return _scanEventSegments.Count;
			}
			return 0;
		}
	}

	/// <summary>
	/// Sets the scan event segments.
	/// </summary>
	public List<List<ScanEvent>> ScanEventSegments
	{
		set
		{
			_scanEventSegments = value;
		}
	}

	/// <summary>
	/// Gets or sets the trailer extras.
	/// </summary>
	/// <value>
	/// The trailer extras.
	/// </value>
	public GenericDataCollection TrailerExtras
	{
		get
		{
			return _trailerExtras;
		}
		set
		{
			_trailerExtras = value;
		}
	}

	/// <summary>
	/// Gets the trailer extras data descriptors.
	/// </summary>
	public DataDescriptors TrailerExtrasDataDescriptors => _trailerExtras?.DataDescriptors;

	/// <summary>
	/// Gets or sets the trailer scan events.
	/// </summary>
	public TrailerScanEvents TrailerScanEvents
	{
		get
		{
			if (_externalSetScanEvents)
			{
				return _externalEvents;
			}
			return _trailerScanEventsLazy?.Value;
		}
		set
		{
			_externalSetScanEvents = true;
			_externalEvents = value;
		}
	}

	/// <summary>
	/// Gets or sets the tune data.
	/// </summary>
	/// <value>
	/// The tune data.
	/// </value>
	public GenericDataCollection TuneData
	{
		get
		{
			return _tuneData;
		}
		set
		{
			_tuneData = value;
		}
	}

	/// <summary>
	/// Gets the tune method data descriptors.
	/// </summary>
	public DataDescriptors TuneMethodDataDescriptors => _tuneData?.DataDescriptors;

	/// <summary>
	/// Gets or sets the packet viewer.
	/// </summary>
	internal IReadWriteAccessor PacketViewer { get; set; }

	public bool ShouldBuffer
	{
		get
		{
			if (PacketViewer.PreferLargeReads && _bufferManager != null)
			{
				return !_bufferManager.SingleBuffer;
			}
			return false;
		}
	}

	/// <summary>
	/// Create a reader to access a large block of scans
	/// using just 1 read from the initial view, for better efficiency
	/// </summary>
	/// <param name="lowScan">first scan needed</param>
	/// <param name="highScan">last scan needed</param>
	/// <returns>A memory reader to decode the selected scans</returns>
	public IMemoryReader CreateSubRangeReader(int lowScan, int highScan)
	{
		return CreateRentableSubRangeReader(lowScan, highScan);
	}

	/// <summary>
	/// Create a reader to access a large block of scans
	/// using just 1 read from the initial view, for better efficiency
	/// </summary>
	/// <param name="firstScanIndex">first scan needed</param>
	/// <param name="secondScanIndex">last scan needed</param>
	/// <param name="pool">Buffer pool, which reduces garbage collection</param>
	/// <returns>A memory reader to decode the selected scans</returns>
	public IMemoryReader CreateRentableSubRangeReader(ScanIndex firstScanIndex, ScanIndex secondScanIndex, IBufferPool pool = null)
	{
		long length = firstScanIndex.ScanByteLength;
		long address = firstScanIndex.DataOffset;
		length = CalculateScanSize(firstScanIndex, firstScanIndex.PacketType, PacketViewer, DataDomain, length, ref address);
		long length2 = secondScanIndex.ScanByteLength;
		long address2 = secondScanIndex.DataOffset;
		length2 = CalculateScanSize(secondScanIndex, secondScanIndex.PacketType, PacketViewer, DataDomain, length2, ref address2);
		byte[] data = ((pool == null) ? PacketViewer.ReadBytes(address, (int)(address2 - address + length2)) : PacketViewer.RentBytes(address, (int)(address2 - address + length2), pool));
		return new MemoryArrayReader(data, address);
	}

	/// <summary>
	/// Create a reader to access a large block of scans
	/// using just 1 read from the initial view, for better efficiency
	/// </summary>
	/// <param name="lowScan">first scan needed</param>
	/// <param name="highScan">last scan needed</param>
	/// <param name="pool">Buffer pool, which reduces garbage collection</param>
	/// <returns>A memory reader to decode the selected scans</returns>
	public IMemoryReader CreateRentableSubRangeReader(int lowScan, int highScan, IBufferPool pool = null)
	{
		ScanIndex msScanIndex = GetMsScanIndex(lowScan);
		ScanIndex msScanIndex2 = GetMsScanIndex(highScan);
		return CreateRentableSubRangeReader(msScanIndex, msScanIndex2, pool);
	}

	public void ReleaseSubRangeReader(IMemoryReader reader, IBufferPool pool)
	{
		if (reader is MemoryArrayReader memoryArrayReader)
		{
			PacketViewer.ReturnRentedBytes(memoryArrayReader.Data, pool);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.MassSpecDevice" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">raw file loader ID</param>
	/// <param name="deviceInfo">
	/// The device info.
	/// </param>
	/// <param name="rawFileName">
	/// The viewer.
	/// </param>
	/// <param name="fileRevision">
	/// The file version.
	/// </param>
	/// <param name="isInAcquisition">Set if this file is being created</param>
	/// <param name="oldRev">Set if this file version is from a legacy data system</param>
	public MassSpecDevice(IViewCollectionManager manager, Guid loaderId, VirtualControllerInfo deviceInfo, string rawFileName, int fileRevision, bool isInAcquisition, bool oldRev)
		: base(manager, loaderId, deviceInfo, rawFileName, fileRevision, isInAcquisition, oldRev)
	{
		BaseDataInitialization();
		DataDomain = base.RunHeader.DeviceDataDomain;
		_runHeaderStruct = base.RunHeader.RunHeaderStruct;
		_inAcquisition = isInAcquisition;
		_oldRev = oldRev;
		_rawFileName = rawFileName;
		if (fileRevision >= 66 && _runHeaderStruct.Revision >= 25)
		{
			throw new NewerFileFormatException("This mass spectrometer has a newer raw format which the current application cannot decode. An upgraded application may resolve this");
		}
	}

	internal bool SupportsSimplifiedPacket(ScanIndex scanIndex)
	{
		SpectrumPacketType packetType = scanIndex.PacketType;
		if (packetType != SpectrumPacketType.LinearTrapProfile)
		{
			return packetType == SpectrumPacketType.FtCentroid;
		}
		return true;
	}

	/// <summary>
	/// Initializer, designed for use by Lazy load pattern.
	/// Base object can be created to perform version checks.
	/// When device needs to be used for the first time, Initialize is called.
	/// </summary>
	/// <returns>The initialized device</returns>
	public override IDevice Initialize()
	{
		_massSpecScanList = new Lazy<ScanIndex[]>(MsScanList);
		if (_inAcquisition)
		{
			InAcquisitionInitializer(base.FileRevision);
			return this;
		}
		if (_oldRev)
		{
			return this;
		}
		NonAcquisitionInitializer(_rawFileName, base.FileRevision);
		long length = CalculatePacketLength();
		_bufferManager = new RecordBufferManager(PacketViewer, length, _runHeaderStruct.FirstSpectrum, _runHeaderStruct.LastSpectrum, this, zeroBased: false, singleBufferPermitted: true);
		return this;
	}

	/// <summary>
	/// Sets the precursor ion tolerance.
	/// </summary>
	/// <param name="filterScanEvent">The scan event.</param>
	/// <param name="isQuantum">if set to <c>true</c> [b quantum].</param>
	/// <param name="filterMassPrecision">The filter mass precision.</param>
	public static void SetPrecursorIonTolerance(IFilterScanEvent filterScanEvent, bool isQuantum, int filterMassPrecision)
	{
		if (!isQuantum)
		{
			filterScanEvent.SetFilterMassResolution(0.4);
		}
		else
		{
			filterScanEvent.SetFilterMassResolutionByMassPrecision(filterMassPrecision);
		}
	}

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public override void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		if (_scanIndices != null)
		{
			_scanIndices.Dispose();
			_scanIndices = null;
		}
		if (_trailerExtras != null)
		{
			_trailerExtras.Dispose();
			_trailerExtras = null;
		}
		if (_tuneData != null)
		{
			_tuneData.Dispose();
			_tuneData = null;
		}
		if (_externalSetScanEvents)
		{
			_externalEvents?.Dispose();
			TrailerScanEvents = null;
		}
		else
		{
			Lazy<TrailerScanEvents> trailerScanEventsLazy = _trailerScanEventsLazy;
			if (trailerScanEventsLazy != null && trailerScanEventsLazy.IsValueCreated)
			{
				_trailerScanEventsLazy?.Value?.Dispose();
			}
		}
		if (PacketViewer != null)
		{
			PacketViewer.ReleaseAndCloseMemoryMappedFile(base.Manager);
			PacketViewer = null;
		}
		_acqScanEventViewer.ReleaseAndCloseMemoryMappedFile(base.Manager);
		_acqScanIndexViewer.ReleaseAndCloseMemoryMappedFile(base.Manager);
		if (_bufferManager != null)
		{
			_bufferManager.Dispose();
			_bufferManager = null;
		}
		base.Dispose();
		DisposeBufferInfoAndTempFiles();
	}

	/// <summary>
	/// This method get the noise, baselines and frequencies data.
	/// This will typically used by the application when exporting mass spec data to a raw file.<para />
	/// The advanced packet data is for LT/FT formats only.<para />
	/// </summary>
	/// <param name="scanNumber">The scan number.</param>
	/// <param name="includeReferenceAndExceptionData">Set if centroid data should include the reference and exception peaks</param>
	/// <returns>Returns the IAdvancedPacketData object.</returns>
	/// <exception cref="T:System.Exception">Thrown if encountered an error while retrieving LT/FT's data, i.e. noise data and frequencies.</exception>
	public IAdvancedPacketData GetAdvancedPacketData(int scanNumber, bool includeReferenceAndExceptionData)
	{
		PacketFeatures packetScanDataFeatures = PacketFeatures.All;
		try
		{
			ScanIndex msScanIndex = GetMsScanIndex(scanNumber);
			IMsPacket packet = GetMsPacket(scanNumber, includeReferenceAndExceptionData, packetScanDataFeatures, msScanIndex);
			if (packet == null)
			{
				return new AdvancedPacketData(new Lazy<CentroidStream>(new CentroidStream()), new Lazy<double[]>(Array.Empty<double>()));
			}
			Lazy<double[]> frequencies = new Lazy<double[]>(() => GetFrequencies(packet));
			return new AdvancedPacketData(new Lazy<CentroidStream>(() => CentroidStreamFactory.CreateCentroidStream(packet.LabelPeaks)), frequencies)
			{
				NoiseData = (msScanIndex.PacketType.HasLabelPeaks() ? packet.NoiseAndBaselines : Array.Empty<NoiseAndBaseline>())
			};
		}
		catch (Exception ex)
		{
			throw new Exception($"Error while retrieving noise and baseline data for {scanNumber}. {ex.Message}", ex);
		}
	}

	/// <summary>
	/// The method gets the auto filters.
	/// </summary>
	/// <returns>
	/// The collection of auto filters.
	/// </returns>
	public string[] GetAutoFilters()
	{
		IFilterScanEvent[] filters = GetFilters();
		FilterConvert[] array = new FilterConvert[filters.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new FilterConvert(filters[i]);
		}
		ParallelSmallTasks.ExecuteInParallel(array);
		string[] array2 = new string[array.Length];
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = array[j].ConvertedFilter;
		}
		return array2;
	}

	/// <summary>
	/// Gets the compound name by scan filter.
	/// </summary>
	/// <param name="scanFilterString">The scan filter string.</param>
	/// <returns>The compound names</returns>
	public IEnumerable<string> GetCompoundNameByScanFilter(string scanFilterString)
	{
		SortedSet<string> sortedSet = new SortedSet<string>();
		FilterStringParser obj = new FilterStringParser
		{
			MassPrecision = 10
		};
		bool flag = obj.ParseFilterStructString(scanFilterString);
		ScanFilterHelper scanFilterHelper = new ScanFilterHelper(new WrappedScanFilter(obj.ToFilterScanEvent()), accuratePrecursors: false, base.RunHeader.FilterMassPrecision);
		if (!flag)
		{
			return sortedSet;
		}
		int numScanEventSegments = NumScanEventSegments;
		if (numScanEventSegments > 0)
		{
			for (int i = 0; i < numScanEventSegments; i++)
			{
				List<ScanEvent> scanEvents = GetScanEvents(i);
				if (scanEvents != null && scanEvents.Any())
				{
					int count = scanEvents.Count;
					for (int j = 0; j < count; j++)
					{
						ScanEvent scanEvent = scanEvents[j];
						AddCompoundName(sortedSet, scanFilterHelper, scanEvent);
					}
				}
			}
		}
		else
		{
			for (int k = _runHeaderStruct.FirstSpectrum; k <= _runHeaderStruct.LastSpectrum; k++)
			{
				ScanEvent scanEventWithValidScanNumber = GetScanEventWithValidScanNumber(GetValidIndexIntoScanIndices(k));
				AddCompoundName(sortedSet, scanFilterHelper, scanEventWithValidScanNumber);
			}
		}
		return sortedSet;
	}

	/// <summary>
	/// Gets the compound names.
	/// </summary>
	/// <returns>The compound names</returns>
	public IEnumerable<string> GetCompoundNames()
	{
		int numScanEventSegments = NumScanEventSegments;
		SortedSet<string> sortedSet = new SortedSet<string>();
		if (numScanEventSegments > 0)
		{
			for (int i = 0; i < numScanEventSegments; i++)
			{
				List<ScanEvent> scanEvents = GetScanEvents(i);
				if (scanEvents == null || scanEvents.Count == 0)
				{
					continue;
				}
				int count = scanEvents.Count;
				for (int j = 0; j < count; j++)
				{
					string name = scanEvents[j].Name;
					if (!string.IsNullOrWhiteSpace(name))
					{
						sortedSet.Add(name);
					}
				}
			}
		}
		else if (_scanIndices.Count > 0)
		{
			int firstSpectrum = base.RunHeader.FirstSpectrum;
			int lastSpectrum = base.RunHeader.LastSpectrum;
			AddNamesInScanRangeToSet(sortedSet, firstSpectrum, lastSpectrum);
		}
		return sortedSet;
	}

	/// <summary>
	/// Add the names of all compounds within a scan number range to a set.
	/// </summary>
	/// <param name="sortedCompoundNamesSet">Set to update</param>
	/// <param name="firstSpec">First scan to examine</param>
	/// <param name="lastSpec">Last scan to examine</param>
	private void AddNamesInScanRangeToSet(SortedSet<string> sortedCompoundNamesSet, int firstSpec, int lastSpec)
	{
		for (int i = firstSpec; i <= lastSpec; i++)
		{
			int validIndexIntoScanIndices = GetValidIndexIntoScanIndices(i);
			string name = GetScanEventWithValidScanNumber(validIndexIntoScanIndices).Name;
			if (!string.IsNullOrWhiteSpace(name))
			{
				sortedCompoundNamesSet.Add(name);
			}
		}
	}

	/// <summary>
	/// Gets the table of unique trailer scan events.
	/// </summary>
	/// <returns></returns>
	public IFilterScanEvent[] GetUniqueEvents()
	{
		if (TrailerScanEvents != null)
		{
			IReadOnlyCollection<ScanEvent> uniqueEvents = TrailerScanEvents.UniqueEvents;
			if (uniqueEvents != null)
			{
				IFilterScanEvent[] array = new IFilterScanEvent[uniqueEvents.Count];
				int num = 0;
				{
					foreach (ScanEvent item in uniqueEvents)
					{
						array[num++] = new FilterScanEvent(item.CreateEditor());
					}
					return array;
				}
			}
		}
		return Array.Empty<IFilterScanEvent>();
	}

	/// <summary>
	/// Gets the filters.
	/// </summary>
	/// <param name="firstSpectrum">The first spectrum.</param>
	/// <param name="lastSpectrum">The last spectrum.</param>
	/// <param name="mode">If set to instrument: Then precursors are always matched based on
	/// the instrument mass resolution. 
	/// If set to "Auto": Data dependent precursors are matched with a wide tolerance.
	/// If set to Specified: value must be set to the mass precision (decimal places) needed</param>
	/// <param name="decimalPlaces">Number of decimal places for precursor matches, when mode is "Specified"</param>
	/// <returns>The list of filters for the given scan range</returns>
	public IFilterScanEvent[] GetFilters(int firstSpectrum = -1, int lastSpectrum = -1, FilterPrecisionMode mode = FilterPrecisionMode.Auto, int decimalPlaces = 2)
	{
		bool flag = (mode & FilterPrecisionMode.ExtendedDataDependentMatch) != 0;
		bool flag2 = (mode & FilterPrecisionMode.FindPrecursorMidPoint) != 0;
		ExtendToleranceBy extendToleranceBy = (ExtendToleranceBy)((int)(mode & FilterPrecisionMode.ScanRangeMatchExpansionMask) >> 4);
		double toleranceFactor = 1.0;
		switch (extendToleranceBy)
		{
		case ExtendToleranceBy.Percent10:
		case ExtendToleranceBy.Percent20:
		case ExtendToleranceBy.Percent30:
		case ExtendToleranceBy.Percent40:
		case ExtendToleranceBy.Percent50:
		case ExtendToleranceBy.Percent60:
		case ExtendToleranceBy.Percent70:
		case ExtendToleranceBy.Percent80:
		case ExtendToleranceBy.Percent90:
		case ExtendToleranceBy.Percent100:
			toleranceFactor = 1.0 + (double)extendToleranceBy / 10.0;
			break;
		case ExtendToleranceBy.Factor3:
			toleranceFactor = 3.0;
			break;
		case ExtendToleranceBy.Factor4:
			toleranceFactor = 4.0;
			break;
		case ExtendToleranceBy.Factor5:
			toleranceFactor = 5.0;
			break;
		}
		bool flag3 = base.RunHeader.FileRevision < 57;
		bool flag4 = true;
		bool hasAccurateMassPrecursors = base.InstrumentId.IsTsqQuantumFile || (mode & FilterPrecisionMode.SpecifiedPrecisionMask) != 0;
		int num = 0;
		if (base.RunHeader.IsInAcquisition && NumScanEvents > 0)
		{
			int numScanEventSegments = NumScanEventSegments;
			for (int i = 0; i < numScanEventSegments; i++)
			{
				List<ScanEvent> list = _scanEventSegments[i];
				if (list == null)
				{
					continue;
				}
				int count = list.Count;
				for (int j = 0; j < count; j++)
				{
					if (list[j].DependentDataFlag != ScanFilterEnums.IsDependent.Yes && !list[j].IsCustom)
					{
						num++;
					}
				}
			}
		}
		firstSpectrum = ((firstSpectrum == -1) ? _runHeaderStruct.FirstSpectrum : firstSpectrum);
		lastSpectrum = ((lastSpectrum == -1) ? _runHeaderStruct.LastSpectrum : lastSpectrum);
		if (firstSpectrum == 0 && lastSpectrum == 0)
		{
			return Array.Empty<IFilterScanEvent>();
		}
		bool num2 = firstSpectrum == _runHeaderStruct.FirstSpectrum && lastSpectrum == _runHeaderStruct.LastSpectrum;
		int numScanEvents = NumScanEvents;
		int numScanEventSegments2 = NumScanEventSegments;
		TrailerScanEvents trailerScanEvents = TrailerScanEvents;
		int scanIndexCount = trailerScanEvents.ScanIndexCount;
		int uniqueEventsCount = trailerScanEvents.UniqueEventsCount;
		int filterMassPrecision = 2;
		if ((mode & FilterPrecisionMode.SpecifiedPrecisionMask) != FilterPrecisionMode.Specified)
		{
			filterMassPrecision = base.RunHeader.FilterMassPrecision;
		}
		else
		{
			filterMassPrecision = decimalPlaces;
		}
		int num3 = 0;
		List<FilterScanEvent> list2 = new List<FilterScanEvent>(numScanEvents + num);
		if (num2 && scanIndexCount == 0 && numScanEvents > 0)
		{
			list2.Clear();
			for (int k = 0; k < numScanEventSegments2; k++)
			{
				foreach (ScanEvent item in _scanEventSegments[k])
				{
					if (!item.IsCustom && item.DependentDataFlag != ScanFilterEnums.IsDependent.Yes)
					{
						list2.Add(new FilterScanEvent(item.CreateEditor()));
						num3++;
					}
				}
			}
		}
		else
		{
			List<bool[]> list3 = null;
			int[] array = null;
			if (numScanEventSegments2 > 0)
			{
				list3 = new List<bool[]>(numScanEventSegments2);
				array = new int[numScanEventSegments2];
				for (int l = 0; l < numScanEventSegments2; l++)
				{
					array[l] = _scanEventSegments[l].Count;
					list3.Add(new bool[array[l]]);
				}
			}
			int num4 = lastSpectrum - firstSpectrum + 1;
			int[] array2 = null;
			int[] array3 = null;
			ScanIndex[] array4 = (_massSpecScanList.IsValueCreated ? _massSpecScanList.Value : null);
			if (list3 != null)
			{
				int num5 = 0;
				array2 = new int[lastSpectrum + 1];
				array3 = new int[lastSpectrum + 1];
				for (int m = firstSpectrum; m <= lastSpectrum; m++)
				{
					int validIndexIntoScanIndices = GetValidIndexIntoScanIndices(m);
					ScanIndex scanIndex = ((array4 != null) ? array4[validIndexIntoScanIndices] : null) ?? _scanIndices[validIndexIntoScanIndices];
					int num6 = (array2[m] = scanIndex.ScanTypeRawIndex);
					array3[m] = scanIndex.TrailerOffset;
					if (num6 == -1)
					{
						continue;
					}
					int scanSegment = scanIndex.ScanSegment;
					int scanTypeIndex = scanIndex.ScanTypeIndex;
					ScanEvent scanEvent = GetScanEvents(scanSegment)[scanTypeIndex];
					if (!scanEvent.IsCustom && scanEvent.DependentDataFlag != ScanFilterEnums.IsDependent.Yes && scanSegment < numScanEvents && scanTypeIndex < array[scanSegment])
					{
						if (list3[scanSegment][scanTypeIndex])
						{
							num5++;
						}
						else
						{
							list3[scanSegment][scanTypeIndex] = true;
						}
					}
				}
				num4 -= num5;
				for (int n = 0; n < numScanEventSegments2; n++)
				{
					bool[] array5 = list3[n];
					Array.Clear(array5, 0, array5.Length);
				}
			}
			else
			{
				num4 = ((uniqueEventsCount > 0) ? uniqueEventsCount : scanIndexCount);
			}
			HashSet<int> hashSet = new HashSet<int>();
			list2.Clear();
			list2 = new List<FilterScanEvent>(num4 + num);
			List<EventConvert> rawEventsList = new List<EventConvert>();
			for (int num7 = firstSpectrum; num7 <= lastSpectrum; num7++)
			{
				bool flag5 = true;
				int validIndexIntoScanIndices2 = GetValidIndexIntoScanIndices(num7);
				int num8;
				int index;
				if (array2 != null)
				{
					num8 = array2[num7];
					index = array3[num7];
				}
				else
				{
					ScanIndex scanIndex = ((array4 != null) ? array4[validIndexIntoScanIndices2] : null) ?? _scanIndices[validIndexIntoScanIndices2];
					num8 = scanIndex.ScanTypeRawIndex;
					index = scanIndex.TrailerOffset;
				}
				ScanEvent scanEvent = null;
				if (num8 != -1)
				{
					int num9 = num8 >> 16;
					int num10 = num8 & 0xFFFF;
					scanEvent = GetScanEvents(num9)[num10];
					bool isCustom = scanEvent.IsCustom;
					if (isCustom || scanEvent.DependentDataFlag == ScanFilterEnums.IsDependent.Yes)
					{
						scanEvent = trailerScanEvents.GetEvent(index);
					}
					if (list3 != null && !isCustom && !scanEvent.IsCustom && scanEvent.DependentDataFlag != ScanFilterEnums.IsDependent.Yes && num9 < numScanEvents && num10 < array[num9])
					{
						if (list3[num9][num10])
						{
							flag5 = false;
						}
						else
						{
							list3[num9][num10] = true;
						}
					}
				}
				else if (uniqueEventsCount > 0)
				{
					int uniqueEventsIndex = trailerScanEvents.GetUniqueEventsIndex(index);
					if (uniqueEventsIndex >= 0 && hashSet.Add(uniqueEventsIndex))
					{
						scanEvent = trailerScanEvents.GetEvent(index);
					}
					else
					{
						flag5 = false;
					}
				}
				else
				{
					scanEvent = GetScanEventWithValidScanNumber(validIndexIntoScanIndices2);
				}
				if (flag5)
				{
					rawEventsList.Add(new EventConvert
					{
						TheEvent = scanEvent
					});
				}
			}
			if (rawEventsList.Count > 1)
			{
				bool[] parentTrap = new bool[40000];
				bool trapOk = true;
				ConcurrentBag<ThermoFisher.CommonCore.Data.Business.Range> parentRanges = new ConcurrentBag<ThermoFisher.CommonCore.Data.Business.Range>();
				Parallel.ForEach(Partitioner.Create(0, rawEventsList.Count, 200), delegate(Tuple<int, int> range)
				{
					bool flag10 = false;
					double num28 = 0.0;
					double num29 = 0.0;
					for (int num30 = range.Item1; num30 < range.Item2; num30++)
					{
						EventConvert eventConvert = rawEventsList[num30];
						FilterScanEvent filterScanEvent7 = new FilterScanEvent(eventConvert.TheEvent.CreateEditor());
						SetPrecursorIonTolerance(filterScanEvent7, hasAccurateMassPrecursors, filterMassPrecision);
						eventConvert.TheFilter = filterScanEvent7;
						if (filterScanEvent7.MsOrder >= ScanFilterEnums.MSOrderTypes.MS2)
						{
							double precursorMass = filterScanEvent7.Reactions[0].PrecursorMass;
							int num31 = (int)(precursorMass * 10.0);
							if (num31 < 0 || num31 >= parentTrap.Length)
							{
								trapOk = false;
							}
							else
							{
								parentTrap[num31] = true;
							}
							if (flag10)
							{
								if (precursorMass < num28)
								{
									num28 = precursorMass;
								}
								if (precursorMass > num29)
								{
									num29 = precursorMass;
								}
							}
							else
							{
								flag10 = true;
								num28 = precursorMass;
								num29 = precursorMass;
							}
						}
					}
					if (flag10)
					{
						parentRanges.Add(ThermoFisher.CommonCore.Data.Business.Range.Create(num28, num29));
					}
				});
				foreach (EventConvert item2 in rawEventsList)
				{
					list2.Add(item2.TheFilter);
					num3++;
				}
				bool flag6 = true;
				double num11 = 0.0;
				double num12 = 0.0;
				foreach (ThermoFisher.CommonCore.Data.Business.Range item3 in parentRanges)
				{
					if (flag6)
					{
						flag6 = false;
						num11 = item3.Low;
						num12 = item3.High;
						continue;
					}
					if (item3.Low < num11)
					{
						num11 = item3.Low;
					}
					if (item3.Low > num12)
					{
						num12 = item3.High;
					}
				}
				if (!flag6 && num12 - num11 > 0.2)
				{
					if (trapOk)
					{
						int num13 = (int)(num11 * 10.0);
						int num14 = (int)(num12 * 10.0);
						int num15 = 0;
						for (int num16 = num13; num16 <= num14; num16++)
						{
							if (parentTrap[num16])
							{
								if (++num15 >= 3)
								{
									flag4 = false;
									break;
								}
							}
							else
							{
								num15 = 0;
							}
						}
					}
					else
					{
						flag4 = false;
					}
				}
			}
			else
			{
				foreach (EventConvert item4 in rawEventsList)
				{
					FilterScanEvent filterScanEvent = new FilterScanEvent(item4.TheEvent.CreateEditor());
					SetPrecursorIonTolerance(filterScanEvent, hasAccurateMassPrecursors, filterMassPrecision);
					list2.Add(filterScanEvent);
					num3++;
				}
			}
			list3?.Clear();
		}
		if (base.RunHeader.IsInAcquisition && NumScanEvents > 0)
		{
			int numScanEventSegments3 = NumScanEventSegments;
			for (int num17 = 0; num17 < numScanEventSegments3; num17++)
			{
				List<ScanEvent> list4 = _scanEventSegments[num17];
				if (list4 == null)
				{
					continue;
				}
				foreach (ScanEvent item5 in list4)
				{
					if (item5.DependentDataFlag != ScanFilterEnums.IsDependent.Yes && !item5.IsCustom)
					{
						list2.Add(new FilterScanEvent(item5.CreateEditor()));
						num3++;
					}
				}
			}
		}
		int num18 = 0;
		int num19 = 0;
		int num20 = 0;
		bool flag7 = false;
		SortedSet<FilterScanEvent> sortHeadFilterSet;
		int sortHeadIdentical;
		bool simpleParentGroup;
		if (num3 > 0)
		{
			if (flag3 || flag4)
			{
				LegacyFilterSorter.Qsort(list2);
			}
			else
			{
				FilterSorter.Qsort(list2);
				flag7 = true;
				FilterScanEvent filterScanEvent2 = list2[0];
				int num21 = 0;
				int num22 = 0;
				for (int num23 = 1; num23 < num3; num23++)
				{
					FilterScanEvent filterScanEvent3 = list2[num23];
					bool num24 = filterScanEvent2.SameFirstParentMass(filterScanEvent3);
					if (num24)
					{
						num22 = num23;
					}
					if (!num24 || num23 == num3 - 1)
					{
						if (num22 > num21)
						{
							LegacyFilterSorter.QsortSlice(list2, num21, num22);
						}
						num21 = num23;
						filterScanEvent2 = filterScanEvent3;
					}
				}
			}
			bool flag8 = flag || flag2;
			SmartFilterComparer smartFilterComparer = new SmartFilterComparer(flag, toleranceFactor);
			sortHeadFilterSet = (flag8 ? new SortedSet<FilterScanEvent>(smartFilterComparer) : new SortedSet<FilterScanEvent>());
			FilterScanEvent filterScanEvent4 = list2[0];
			InitMassRanges(filterScanEvent4, out var rangeLo, out var rangeHi);
			sortHeadFilterSet.Add(filterScanEvent4);
			sortHeadIdentical = 0;
			simpleParentGroup = true;
			FilterScanEvent filterScanEvent5 = filterScanEvent4;
			if (!flag7)
			{
				flag2 = false;
			}
			for (int num25 = 1; num25 < num3; num25++)
			{
				bool flag9 = false;
				FilterScanEvent filterScanEvent6 = list2[num25];
				if (filterScanEvent4.SameParentMass(filterScanEvent6))
				{
					if (sortHeadFilterSet.Add(filterScanEvent6))
					{
						simpleParentGroup = false;
						list2[++num19 + num20] = list2[num25];
						continue;
					}
					num18++;
					sortHeadIdentical++;
					filterScanEvent5 = filterScanEvent6;
					flag9 |= ExpandMassRange(rangeLo, rangeHi, filterScanEvent6);
					continue;
				}
				if (flag2 && sortHeadIdentical > 0 && simpleParentGroup)
				{
					simpleParentGroup = false;
					if (smartFilterComparer.Compare(filterScanEvent5, filterScanEvent6) == 0)
					{
						num18++;
						flag9 |= ExpandMassRange(rangeLo, rangeHi, filterScanEvent6);
						list2[num19 + num20] = filterScanEvent5;
						ResetSortHead(filterScanEvent5);
						filterScanEvent5 = filterScanEvent6;
						continue;
					}
				}
				ResetSortHead(filterScanEvent4);
				if (filterScanEvent4.Equals(filterScanEvent6))
				{
					num18++;
					continue;
				}
				if (num18 > 0)
				{
					list2[++num19 + num20] = list2[num25];
				}
				else
				{
					num20++;
				}
				filterScanEvent4 = filterScanEvent6;
				ResetSortHead(filterScanEvent4);
				InitMassRanges(filterScanEvent4, out rangeLo, out rangeHi);
			}
			sortHeadFilterSet.Clear();
			sortHeadIdentical = 0;
		}
		int num26 = ((num18 == 0) ? (num3 - 1) : (num19 + num20));
		IFilterScanEvent[] array6 = new IFilterScanEvent[num26 + 1];
		for (int num27 = 0; num27 <= num26; num27++)
		{
			array6[num27] = list2[num27];
		}
		return array6;
		static bool ExpandMassRange(double[] array7, double[] array8, FilterScanEvent testFilter2)
		{
			bool result = false;
			for (int num28 = 0; num28 < array7.Length; num28++)
			{
				if (testFilter2.MassRanges[num28].LowMass < array7[num28])
				{
					array7[num28] = testFilter2.MassRanges[num28].LowMass;
					result = true;
				}
				if (testFilter2.MassRanges[num28].HighMass > array8[num28])
				{
					array8[num28] = testFilter2.MassRanges[num28].HighMass;
					result = true;
				}
			}
			return result;
		}
		static void InitMassRanges(FilterScanEvent testFilter1, out double[] reference, out double[] reference2)
		{
			MassRangeStruct[] massRanges = testFilter1.MassRanges;
			int num28 = massRanges.Length;
			reference = new double[num28];
			reference2 = new double[num28];
			for (int num29 = 0; num29 < num28; num29++)
			{
				reference[num29] = massRanges[num29].LowMass;
				reference2[num29] = massRanges[num29].HighMass;
			}
		}
		void ResetSortHead(FilterScanEvent filter)
		{
			sortHeadFilterSet.Clear();
			sortHeadFilterSet.Add(filter);
			simpleParentGroup = true;
			sortHeadIdentical = 0;
		}
	}

	/// <summary>
	/// The method gets the label peaks.
	/// Caller may have already obtained some items (scan index and event)
	/// If not: these are passed as null, and will be obtained within this
	/// call as needed.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="includeReferenceAndExceptionPeaks">set if calibration reference and exception peaks should be returned</param>
	/// <param name="packetScanDataFeatures">True if noise and baseline data is required</param>
	/// <param name="scanEvent">optional: scan event (to save reading twice)</param>
	/// <param name="scanIndex">optional: data about this scan</param>
	/// <param name="subRangeReader">(fast) reader for a batch of scans: optional</param>
	/// <returns>
	/// The collection of <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LabelPeak" />s.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// Thrown if encountered an error while retrieving label peaks.
	/// </exception>
	public ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LabelPeak[] GetLabelPeaks(int scanNumber, bool includeReferenceAndExceptionPeaks = true, PacketFeatures packetScanDataFeatures = PacketFeatures.All, ScanEvent scanEvent = null, ScanIndex scanIndex = null, IMemoryReader subRangeReader = null)
	{
		try
		{
			if (scanIndex == null)
			{
				scanIndex = GetMsScanIndex(scanNumber);
			}
			if (scanIndex.PacketType.HasLabelPeaks())
			{
				IMsPacket msPacket = GetMsPacket(scanNumber, includeReferenceAndExceptionPeaks, packetScanDataFeatures, scanIndex, scanEvent, subRangeReader);
				if (msPacket != null)
				{
					return msPacket.LabelPeaks;
				}
			}
			return Array.Empty<ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LabelPeak>();
		}
		catch (Exception ex)
		{
			throw new Exception($"Error while retrieving centroid peaks for {scanNumber}. {ex.Message}", ex);
		}
	}

	/// <summary>
	/// get scan data from mass spec.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="includeReferenceAndExceptionData">
	/// The include reference and exception data.
	/// </param>
	/// <param name="packetScanDataFeatures">
	/// The additional scan data features.
	/// </param>
	/// <param name="scanIndex">
	/// The scan index.
	/// </param>
	/// <param name="scanEvent">
	/// The scan event.
	/// </param>
	/// <param name="subRangeReader">(fast) reader for a batch of scans: optional</param>
	/// <param name="zeroPadding">forces zero values between profile peaks</param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.IMsPacket" />.
	/// </returns>
	/// <exception cref="T:System.NotImplementedException">For data types which we cannot decode
	/// </exception>
	public IMsPacket GetMsPacket(int scanNumber, bool includeReferenceAndExceptionData, PacketFeatures packetScanDataFeatures, ScanIndex scanIndex, ScanEvent scanEvent = null, IMemoryReader subRangeReader = null, bool zeroPadding = true)
	{
		SpectrumPacketType packetType = scanIndex.PacketType;
		if (packetType >= SpectrumPacketType.InvalidPacket)
		{
			return null;
		}
		if (subRangeReader == null && PacketViewer.PreferLargeReads && _bufferManager != null)
		{
			subRangeReader = _bufferManager.FindReader(scanNumber);
		}
		return ReadMsPacket(includeReferenceAndExceptionData, packetScanDataFeatures, scanIndex, Calibrators, packetType, subRangeReader ?? PacketViewer, zeroPadding);
		double[] Calibrators()
		{
			return (scanEvent ?? GetScanEvent(scanNumber)).MassCalibrators;
		}
	}

	/// <summary>
	/// get scan data from mass spec.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="includeReferenceAndExceptionData">
	/// The include reference and exception data.
	/// </param>
	/// <param name="packetScanDataFeatures">
	/// The additional scan data features.
	/// </param>
	/// <param name="scanIndex">
	/// The scan index.
	/// </param>
	/// <param name="scanEvent">
	/// The scan event.
	/// </param>
	/// <param name="subRangeReader">(fast) reader for a batch of scans: optional</param>
	/// <param name="zeroPadding">forces zero values between profile peaks</param>
	/// <returns>
	/// The array of mass, intensity
	/// </returns>
	/// <exception cref="T:System.NotImplementedException">For data types which we cannot decode
	/// </exception>
	public ISimpleMsPacket GetSimplifiedMsPacket(int scanNumber, bool includeReferenceAndExceptionData, PacketFeatures packetScanDataFeatures, ScanIndex scanIndex, ScanEvent scanEvent = null, IMemoryReader subRangeReader = null, bool zeroPadding = true)
	{
		SpectrumPacketType packetType = scanIndex.PacketType;
		if (packetType >= SpectrumPacketType.InvalidPacket)
		{
			return null;
		}
		if (subRangeReader == null && PacketViewer.PreferLargeReads && _bufferManager != null)
		{
			subRangeReader = _bufferManager.FindReader(scanNumber);
		}
		return ReadSimplifiedMsPacket(includeReferenceAndExceptionData, packetScanDataFeatures, scanIndex, Calibrators, packetType, subRangeReader ?? PacketViewer, zeroPadding);
		double[] Calibrators()
		{
			return (scanEvent ?? GetScanEvent(scanNumber)).MassCalibrators;
		}
	}

	/// <summary>Gets all indices to unique trailer scan events.</summary>
	/// <returns>
	///   <br />
	/// </returns>
	public int[] GetAllIndicesToUniqueTrailerScanEvents()
	{
		return TrailerScanEvents.IndexToUniqueScanEvents;
	}

	/// <summary>Gets all unique trailer scan event indices.</summary>
	/// <returns>
	///   <br />
	/// </returns>
	public IReadOnlyCollection<(int index, long startOffset, long endOffset)> GetAllUniqueTrailerScanEventIndices()
	{
		return TrailerScanEvents.UniqueScanEventIndices;
	}

	/// <summary>Gets all index to unique events start offset.</summary>
	/// <returns>
	///   <br />
	/// </returns>
	public (long, long)[] GetEventAddressTable()
	{
		return TrailerScanEvents.EventAddressTable;
	}

	private ISimpleMsPacket ReadSimplifiedMsPacket(bool includeReferenceAndExceptionData, PacketFeatures packetScanDataFeatures, ScanIndex scanIndex, Func<double[]> calibrators, SpectrumPacketType packetType, IMemoryReader reader, bool zeroPadding)
	{
		return packetType switch
		{
			SpectrumPacketType.LinearTrapProfile => new SimplifiedLinearTrapProfilePacket(reader, scanIndex.DataOffset, base.FileRevision, includeReferenceAndExceptionData, packetScanDataFeatures | PacketFeatures.Profile), 
			SpectrumPacketType.FtCentroid => new SimplifiedFtCentroidPacket(reader, scanIndex.DataOffset, base.FileRevision, includeReferenceAndExceptionData, packetScanDataFeatures), 
			_ => throw new NotImplementedException(), 
		};
	}

	private IMsPacket ReadMsPacket(bool includeReferenceAndExceptionData, PacketFeatures packetScanDataFeatures, ScanIndex scanIndex, Func<double[]> calibrators, SpectrumPacketType packetType, IMemoryReader reader, bool zeroPadding)
	{
		switch (packetType)
		{
		case SpectrumPacketType.FtCentroid:
			return new FtCentroidPacket(reader, scanIndex.DataOffset, base.FileRevision, includeReferenceAndExceptionData, packetScanDataFeatures);
		case SpectrumPacketType.FtProfile:
			return new FtProfilePacket(reader, scanIndex.DataOffset, calibrators, base.FileRevision, includeReferenceAndExceptionData, packetScanDataFeatures);
		case SpectrumPacketType.LinearTrapProfile:
			return new LinearTrapProfilePacket(reader, scanIndex.DataOffset, base.FileRevision, includeReferenceAndExceptionData, packetScanDataFeatures | PacketFeatures.Profile, zeroPadding);
		case SpectrumPacketType.LinearTrapCentroid:
			return new LinearTrapCentroidPacket(reader, scanIndex.DataOffset, base.FileRevision, includeReferenceAndExceptionData, PacketFeatures.None);
		case SpectrumPacketType.LowResolutionSpectrum:
			return new LowResSpDataPkt(reader, scanIndex.DataOffset, scanIndex, base.FileRevision);
		case SpectrumPacketType.LowResolutionSpectrumType2:
			return new LowResSpDataPkt2(reader, scanIndex.DataOffset, scanIndex, base.FileRevision);
		case SpectrumPacketType.LowResolutionSpectrumType3:
			return new LowResSpDataPkt3(reader, scanIndex.DataOffset, scanIndex, base.FileRevision);
		case SpectrumPacketType.LowResolutionSpectrumType4:
			return new LowResSpDataPkt4(reader, scanIndex.DataOffset, scanIndex, base.FileRevision);
		case SpectrumPacketType.HighResolutionSpectrum:
			return new HrSpDataPkt(reader, scanIndex.DataOffset, scanIndex, base.FileRevision, includeReferenceAndExceptionData);
		case SpectrumPacketType.StandardAccurateSpectrum:
			return new StandardAccuracyPacket(reader, scanIndex.DataOffset, scanIndex, base.FileRevision, calibrators());
		case SpectrumPacketType.CompressedAccurateSpectrum:
		case SpectrumPacketType.StandardUncalibratedSpectrum:
		case SpectrumPacketType.AccurateMassProfileSpectrum:
		case SpectrumPacketType.HighResolutionCompressedProfile:
		case SpectrumPacketType.LowResolutionCompressedProfile:
			throw new NotImplementedException(scanIndex.PacketType.ToString() + " is not yet implemented");
		case SpectrumPacketType.ProfileSpectrumType2:
			return new ProfSpPkt2(reader, scanIndex.DataOffset, scanIndex, base.FileRevision);
		case SpectrumPacketType.ProfileSpectrumType3:
			return new ProfSpPkt3(reader, scanIndex.DataOffset, scanIndex, base.FileRevision);
		default:
			return new ProfSpPkt(reader, scanIndex.DataOffset, scanIndex, base.FileRevision);
		}
	}

	/// <summary>
	/// Calculate the number of bytes used by a scan. This can be used to transmit a scan as a binary record.
	/// </summary>
	/// <param name="scanIndex"></param>
	/// <param name="packetType"></param>
	/// <param name="reader"></param>
	/// <param name="deviceDataDomain"></param>
	/// <param name="length"></param>
	/// <param name="address"></param>
	/// <returns></returns>
	private long CalculateScanSize(ScanIndex scanIndex, SpectrumPacketType packetType, IMemoryReader reader, RawDataDomain deviceDataDomain, long length, ref long address)
	{
		bool flag = deviceDataDomain == RawDataDomain.MassSpectrometry;
		switch (packetType)
		{
		case SpectrumPacketType.LinearTrapCentroid:
		case SpectrumPacketType.LinearTrapProfile:
		case SpectrumPacketType.FtCentroid:
		case SpectrumPacketType.FtProfile:
		{
			if (flag)
			{
				return length;
			}
			int packetCount = scanIndex.PacketCount;
			if (packetCount > 0)
			{
				return packetCount;
			}
			return AdvancedPacketBase.Size(reader, scanIndex.DataOffset, base.FileRevision);
		}
		case SpectrumPacketType.LowResolutionSpectrum:
			return LowResSpDataPkt.Size(scanIndex);
		case SpectrumPacketType.LowResolutionSpectrumType2:
			return LowResSpDataPkt2.Size(scanIndex);
		case SpectrumPacketType.LowResolutionSpectrumType3:
			return LowResSpDataPkt3.Size(reader, scanIndex.DataOffset, scanIndex, base.FileRevision, out address);
		case SpectrumPacketType.LowResolutionSpectrumType4:
			return LowResSpDataPkt4.Size(reader, scanIndex.DataOffset, scanIndex, base.FileRevision, out address);
		case SpectrumPacketType.HighResolutionSpectrum:
			return HrSpDataPkt.Size(scanIndex);
		case SpectrumPacketType.StandardAccurateSpectrum:
			return StandardAccuracyPacket.Size(scanIndex);
		case SpectrumPacketType.CompressedAccurateSpectrum:
		case SpectrumPacketType.StandardUncalibratedSpectrum:
		case SpectrumPacketType.AccurateMassProfileSpectrum:
		case SpectrumPacketType.HighResolutionCompressedProfile:
		case SpectrumPacketType.LowResolutionCompressedProfile:
			throw new NotImplementedException(scanIndex.PacketType.ToString() + " is not yet implemented");
		case SpectrumPacketType.ProfileSpectrumType2:
			return ProfSpPkt2.Size(reader, scanIndex.DataOffset, scanIndex, base.FileRevision, out address);
		case SpectrumPacketType.ProfileSpectrumType3:
			return ProfSpPkt3.Size(reader, scanIndex.DataOffset, scanIndex, base.FileRevision, out address);
		case SpectrumPacketType.ProfileSpectrum:
			return ProfSpPkt.Size(reader, scanIndex.DataOffset, scanIndex, base.FileRevision, out address);
		default:
			return length;
		}
	}

	/// <summary>
	/// Find the next scan matching a filter
	/// </summary>
	/// <param name="currentPosition">start to start from</param>
	/// <param name="scanFilterHelper">Scan must pass this filter</param>
	/// <returns>the next scan matching the filter criteria from the data file.
	/// If the number of filters passed in is zero, it simply returns the next scan
	/// after position. The search for the next scan always starts at scan position + 1.</returns>
	public int GetNextScanIndex(int currentPosition, ScanFilterHelper scanFilterHelper)
	{
		int lastSpectrum = base.RunHeader.LastSpectrum;
		if (currentPosition == lastSpectrum)
		{
			return -1;
		}
		currentPosition++;
		while (currentPosition <= lastSpectrum && !ScanEventHelper.ScanEventHelperFactory(GetScanEvent(currentPosition)).TestScanAgainstFilter(scanFilterHelper))
		{
			currentPosition++;
		}
		if (currentPosition > lastSpectrum)
		{
			return -1;
		}
		return currentPosition;
	}

	/// <summary>
	/// The method gets the object that implements <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.IPacket" /> for the scan number.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="includeReferenceAndExceptionData">true if (calibration) ref and exception peaks should be included</param>
	/// <param name="channelNumber">For UV device only, negative one (-1) for getting all the channel data by the given scan number</param>
	/// <param name="packetScanDataFeatures">True if noise and baseline data is required</param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.IPacket" /> implementation.
	/// </returns>
	/// <exception cref="T:System.NotImplementedException">
	/// Thrown if the packet type is not implemented.
	/// </exception>
	public override IPacket GetPacket(int scanNumber, bool includeReferenceAndExceptionData, int channelNumber = -1, PacketFeatures packetScanDataFeatures = PacketFeatures.All)
	{
		ScanIndex scanIndex = GetScanIndex(scanNumber) as ScanIndex;
		return GetMsPacket(scanNumber, includeReferenceAndExceptionData, packetScanDataFeatures, scanIndex);
	}

	/// <summary>
	/// Get the previous scan index.
	/// </summary>
	/// <param name="currentPosition">
	/// The cur position (scan).
	/// </param>
	/// <param name="scanFilterHelper">
	/// The scan filter helper.
	/// </param>
	/// <returns>
	/// The previous scan matching filter. -1 if no matching scans.
	/// </returns>
	public int GetPreviousScanIndex(int currentPosition, ScanFilterHelper scanFilterHelper)
	{
		int firstSpectrum = base.RunHeader.FirstSpectrum;
		if (currentPosition == firstSpectrum)
		{
			return -1;
		}
		currentPosition--;
		while (currentPosition >= firstSpectrum && !ScanEventHelper.ScanEventHelperFactory(GetScanEvent(currentPosition)).TestScanAgainstFilter(scanFilterHelper))
		{
			currentPosition--;
		}
		if (currentPosition < firstSpectrum)
		{
			return -1;
		}
		return currentPosition;
	}

	/// <summary>
	/// Gets the scan dependents for the specified scan number and filter precision
	/// </summary>
	/// <param name="scanNumber">The scan number.</param>
	/// <param name="filterPrecision">The filter precision.</param>
	/// <returns><see cref="T:ThermoFisher.CommonCore.RawFileReader.ScanDependents" /> containing master scan dependents information</returns>
	public ScanDependents GetScanDependents(int scanNumber, int filterPrecision)
	{
		int numTrailerExtra = _runHeaderStruct.NumTrailerExtra;
		try
		{
			if (numTrailerExtra <= 0 || scanNumber < 0 || scanNumber + 1 > numTrailerExtra)
			{
				return null;
			}
			DataDescriptors trailerExtrasDataDescriptors = TrailerExtrasDataDescriptors;
			int count = trailerExtrasDataDescriptors.Count;
			if (count <= 0)
			{
				return null;
			}
			RawFileClassification rawFileClassification = RawFileClassification.Indeterminate;
			bool flag = true;
			int num = 0;
			int index = 0;
			ScanFilterEnums.MSOrderTypes mSOrderTypes = ScanFilterEnums.MSOrderTypes.AcceptAnyMSorder;
			List<ScanDependentDetails> list = new List<ScanDependentDetails>();
			ScanEvent scanEvent = GetScanEvent(scanNumber);
			for (int i = scanNumber; i <= numTrailerExtra && flag; i++)
			{
				LabelValueBlob labelValueBlob = _trailerExtras[i - 1];
				if (rawFileClassification == RawFileClassification.Indeterminate)
				{
					for (int j = 0; j < count; j++)
					{
						DataDescriptor dataDescriptor = trailerExtrasDataDescriptors[j];
						if ((dataDescriptor.Label.Equals("Master Scan Number:") || dataDescriptor.Label.Equals("Master Scan Number")) && dataDescriptor.DataType == DataTypes.Long)
						{
							rawFileClassification = RawFileClassification.MasterScanNumberRaw;
							index = j;
							break;
						}
					}
					if (RawFileClassification.MasterScanNumberRaw == rawFileClassification)
					{
						continue;
					}
					if (rawFileClassification == RawFileClassification.Indeterminate)
					{
						rawFileClassification = RawFileClassification.StandardRaw;
					}
				}
				ScanEvent scanEvent2 = null;
				if (RawFileClassification.MasterScanNumberRaw == rawFileClassification)
				{
					int num2 = (int)labelValueBlob.GetValueAt(_trailerExtras.Decoder, index);
					if (num2 == 0)
					{
						num++;
						flag = num < 5;
					}
					if (num2 != scanNumber)
					{
						continue;
					}
					scanEvent2 = GetScanEvent(i);
				}
				else if (RawFileClassification.StandardRaw == rawFileClassification)
				{
					scanEvent2 = GetScanEvent(i);
					if (scanNumber == i)
					{
						mSOrderTypes = scanEvent2.MsOrder;
						continue;
					}
					ScanFilterEnums.MSOrderTypes msOrder = scanEvent2.MsOrder;
					if (mSOrderTypes > ScanFilterEnums.MSOrderTypes.MS2)
					{
						if (msOrder < mSOrderTypes)
						{
							break;
						}
					}
					else if (msOrder <= mSOrderTypes)
					{
						break;
					}
					if (msOrder > mSOrderTypes + 1)
					{
						continue;
					}
				}
				if (scanEvent2 == null || (GetScanEvent(i).DependentDataFlag != ScanFilterEnums.IsDependent.Yes && RawFileClassification.StandardRaw == rawFileClassification))
				{
					continue;
				}
				Reaction[] reactions = scanEvent2.Reactions;
				List<double> list2 = new List<double>(reactions.Length);
				List<double> list3 = new List<double>(reactions.Length);
				bool flag2 = true;
				if (scanEvent.MsOrder > ScanFilterEnums.MSOrderTypes.MS2)
				{
					if (scanEvent2.MsOrder > scanEvent.MsOrder)
					{
						for (int k = 0; k < reactions.Length - 1; k++)
						{
							if (scanEvent.Reactions[k].PrecursorMass != scanEvent2.Reactions[k].PrecursorMass)
							{
								flag2 = false;
							}
						}
					}
					else
					{
						flag2 = false;
					}
				}
				Reaction[] array = reactions;
				foreach (Reaction reaction in array)
				{
					bool flag3 = reaction.ActivationType == ActivationType.Any || !reaction.MultipleActivation;
					if (flag3 && flag2)
					{
						list2.Add(reaction.PrecursorMass);
						list3.Add(reaction.IsolationWidth);
					}
				}
				if (list2.Count > 0)
				{
					list.Add(new ScanDependentDetails
					{
						ScanIndex = i,
						FilterData = scanEvent2,
						IsolationWidthArray = list3.ToArray(),
						PrecursorMassArray = list2.ToArray()
					});
				}
			}
			ScanDependents obj = new ScanDependents
			{
				RawFileInstrumentType = rawFileClassification
			};
			IScanDependentDetails[] scanDependentDetailArray = list.ToArray();
			obj.ScanDependentDetailArray = scanDependentDetailArray;
			return obj;
		}
		catch (Exception)
		{
			return null;
		}
	}

	/// <summary>
	/// The method gets the scan event for the scan number.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEvent" /> object for the scan number.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// If there are issues retrieving the scan event (e.g. the scan number is out of range).
	/// </exception>
	public ScanEvent GetScanEvent(int scanNumber)
	{
		try
		{
			int validIndexIntoScanIndices = GetValidIndexIntoScanIndices(scanNumber);
			return GetScanEventWithValidScanNumber(validIndexIntoScanIndices);
		}
		catch (Exception ex)
		{
			throw new Exception($"Cannot get scan event for {scanNumber}: {ex.Message}.", ex);
		}
	}

	/// <summary>
	/// The method gets the scan events for a segment.
	/// </summary>
	/// <param name="segment">
	/// The segment.
	/// </param>
	/// <returns>
	/// The list of scan events.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// Specified segment is out of range.
	/// </exception>
	public List<ScanEvent> GetScanEvents(int segment)
	{
		if (segment < 0 || segment >= _scanEventSegments.Count)
		{
			throw new Exception($"Specified segment ({segment}) is out of range. The number of segments for this device is {_scanEventSegments.Count}!");
		}
		return _scanEventSegments[segment];
	}

	/// <summary>
	/// Gets the scan filters from compound names.
	/// </summary>
	/// <param name="compoundName">Name of the compound.</param>
	/// <returns>The filters for the given compound</returns>
	public IEnumerable<string> GetScanFiltersFromCompoundName(string compoundName)
	{
		int filterMassPrecision = base.RunHeader.FilterMassPrecision;
		int numScanEventSegments = NumScanEventSegments;
		SortedSet<string> sortedSet = new SortedSet<string>();
		if (string.IsNullOrWhiteSpace(compoundName))
		{
			yield break;
		}
		if (numScanEventSegments > 0)
		{
			for (int i = 0; i < numScanEventSegments; i++)
			{
				List<ScanEvent> scanEvents = GetScanEvents(i);
				if (scanEvents == null || !scanEvents.Any())
				{
					continue;
				}
				int count = scanEvents.Count;
				for (int j = 0; j < count; j++)
				{
					ScanEvent scanEvent = scanEvents[j];
					if (string.Compare(scanEvent.Name, compoundName, StringComparison.OrdinalIgnoreCase) == 0)
					{
						string filterString = new FilterScanEvent(scanEvent.CreateEditor()).GetFilterString(filterMassPrecision);
						if (!string.IsNullOrWhiteSpace(filterString))
						{
							sortedSet.Add(filterString);
						}
					}
				}
			}
		}
		else
		{
			int firstSpectrum = base.RunHeader.FirstSpectrum;
			int lastSpectrum = base.RunHeader.LastSpectrum;
			for (int k = firstSpectrum; k <= lastSpectrum; k++)
			{
				ScanEvent scanEvent2 = GetScanEvent(k);
				if (string.Compare(scanEvent2.Name, compoundName, StringComparison.OrdinalIgnoreCase) == 0)
				{
					string filterString2 = new FilterScanEvent(scanEvent2.CreateEditor()).GetFilterString(filterMassPrecision);
					if (!sortedSet.Contains(filterString2))
					{
						sortedSet.Add(filterString2);
					}
				}
			}
		}
		foreach (string item in sortedSet)
		{
			yield return item;
		}
	}

	/// <summary>
	/// The method gets the scan index for the scan number.
	/// Override of base class
	/// </summary>
	/// <param name="spectrum">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IScanIndex" /> for the scan number.
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">
	/// If the scan number is not in range.
	/// </exception>
	public override IScanIndex GetScanIndex(int spectrum)
	{
		int validIndexIntoScanIndices = GetValidIndexIntoScanIndices(spectrum);
		if (_massSpecScanList.IsValueCreated)
		{
			return _massSpecScanList.Value[validIndexIntoScanIndices];
		}
		return _scanIndices[validIndexIntoScanIndices];
	}

	/// <summary>
	/// The method gets the scan index for the scan number.
	/// </summary>
	/// <param name="spectrum">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanIndex" /> for the scan number.
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">
	/// If the scan number is not in range.
	/// </exception>
	public ScanIndex GetMsScanIndex(int spectrum)
	{
		int validIndexIntoScanIndices = GetValidIndexIntoScanIndices(spectrum);
		if (_massSpecScanList.IsValueCreated)
		{
			return _massSpecScanList.Value[validIndexIntoScanIndices];
		}
		return _scanIndices[validIndexIntoScanIndices];
	}

	/// <summary>
	/// The method gets the retention time for the scan number.
	/// </summary>
	/// <param name="spectrum">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The retention time for the scan number.
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">
	/// If the scan number is not in range.
	/// </exception>
	public override double GetRetentionTime(int spectrum)
	{
		int validIndexIntoScanIndices = GetValidIndexIntoScanIndices(spectrum);
		if (_massSpecScanList.IsValueCreated)
		{
			return _massSpecScanList.Value[validIndexIntoScanIndices].RetentionTime;
		}
		return _scanIndices.GetRetentionTime(validIndexIntoScanIndices);
	}

	/// <summary>
	/// Get simplified labels.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="includeRefPeaks">
	/// The include ref peaks.
	/// </param>
	/// <param name="internalEvent">
	/// The internal event.
	/// </param>
	/// <param name="scanIndex">
	/// The scan index.
	/// </param>
	/// <param name="subRangeReader">(fast) reader for a batch of scans: optional</param>
	/// <param name="includeChargeAndResolution">if set, add extended data</param>
	/// <returns>
	/// The simplified (mass/intensity) data.
	/// </returns>
	public ISimpleScanAccess GetSimplifiedLabels(int scanNumber, bool includeRefPeaks, ScanEvent internalEvent = null, ScanIndex scanIndex = null, bool includeChargeAndResolution = false, IMemoryReader subRangeReader = null)
	{
		PacketFeatures packetScanDataFeatures = (includeChargeAndResolution ? (PacketFeatures.NoiseAndBaseline | PacketFeatures.Chagre | PacketFeatures.Resolution) : PacketFeatures.None);
		return LabelPeaksToSimpleScan(GetLabelPeaks(scanNumber, includeRefPeaks, packetScanDataFeatures, internalEvent, scanIndex, subRangeReader), includeChargeAndResolution);
	}

	/// <summary>
	/// Convert data from "Label peaks" format to "simple scan" format
	/// </summary>
	/// <param name="centroids">Data to convert</param>
	/// <param name="includedChargeAndResolution">true if the scan needs more than minimal data</param>
	/// <returns>converted data</returns>
	internal static ISimpleScanAccess LabelPeaksToSimpleScan(ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LabelPeak[] centroids, bool includedChargeAndResolution = true)
	{
		int num = centroids.Length;
		double[] array = new double[num];
		double[] array2 = new double[num];
		if (includedChargeAndResolution)
		{
			int[] array3 = new int[num];
			float[] array4 = new float[num];
			float[] array5 = new float[num];
			int num2 = num - 3;
			int i;
			for (i = 0; i < num2; i++)
			{
				ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LabelPeak labelPeak = centroids[i];
				array[i] = labelPeak.Mass;
				array3[i] = labelPeak.Charge;
				array4[i] = labelPeak.Resolution;
				array5[i] = labelPeak.Noise;
				array2[i++] = labelPeak.Intensity;
				labelPeak = centroids[i];
				array[i] = labelPeak.Mass;
				array3[i] = labelPeak.Charge;
				array4[i] = labelPeak.Resolution;
				array5[i] = labelPeak.Noise;
				array2[i++] = labelPeak.Intensity;
				labelPeak = centroids[i];
				array[i] = labelPeak.Mass;
				array3[i] = labelPeak.Charge;
				array4[i] = labelPeak.Resolution;
				array5[i] = labelPeak.Noise;
				array2[i++] = labelPeak.Intensity;
				labelPeak = centroids[i];
				array[i] = labelPeak.Mass;
				array3[i] = labelPeak.Charge;
				array4[i] = labelPeak.Resolution;
				array5[i] = labelPeak.Noise;
				array2[i] = labelPeak.Intensity;
			}
			for (; i < num; i++)
			{
				ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LabelPeak labelPeak2 = centroids[i];
				array[i] = labelPeak2.Mass;
				array3[i] = labelPeak2.Charge;
				array2[i] = labelPeak2.Intensity;
				array4[i] = labelPeak2.Resolution;
				array5[i] = labelPeak2.Noise;
			}
			return new SimpleScanPlus
			{
				Masses = array,
				Intensities = array2,
				Charge = array3,
				Resolution = array4,
				Noise = array5
			};
		}
		int num3 = num - 3;
		int j;
		for (j = 0; j < num3; j++)
		{
			ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LabelPeak labelPeak3 = centroids[j];
			array[j] = labelPeak3.Mass;
			array2[j++] = labelPeak3.Intensity;
			labelPeak3 = centroids[j];
			array[j] = labelPeak3.Mass;
			array2[j++] = labelPeak3.Intensity;
			labelPeak3 = centroids[j];
			array[j] = labelPeak3.Mass;
			array2[j++] = labelPeak3.Intensity;
			labelPeak3 = centroids[j];
			array[j] = labelPeak3.Mass;
			array2[j] = labelPeak3.Intensity;
		}
		for (; j < num; j++)
		{
			array[j] = centroids[j].Mass;
			array2[j] = centroids[j].Intensity;
		}
		return new SimpleScan
		{
			Masses = array,
			Intensities = array2
		};
	}

	internal byte[] GetAdditionalScanData(int scanNumber)
	{
		try
		{
			ScanIndex msScanIndex = GetMsScanIndex(scanNumber);
			if (msScanIndex.PacketType.HasLabelPeaks())
			{
				IMsPacket msPacket = GetMsPacket(scanNumber, includeReferenceAndExceptionData: false, PacketFeatures.Debug, msScanIndex);
				if (msPacket != null)
				{
					return msPacket.DebugData;
				}
			}
			return Array.Empty<byte>();
		}
		catch (Exception ex)
		{
			throw new Exception($"Error while retrieving centroid peaks for {scanNumber}. {ex.Message}", ex);
		}
	}

	internal byte[] GetBinaryScanData(int scanNumber)
	{
		try
		{
			int validIndexIntoScanIndices = GetValidIndexIntoScanIndices(scanNumber);
			ScanIndex scanIndex = _scanIndices[validIndexIntoScanIndices];
			long length = scanIndex.ScanByteLength;
			long address = scanIndex.DataOffset;
			length = CalculateScanSize(scanIndex, scanIndex.PacketType, PacketViewer, DataDomain, length, ref address);
			if (length > 0)
			{
				return PacketViewer.ReadLargeData(address, (int)length);
			}
			return Array.Empty<byte>();
		}
		catch (Exception ex)
		{
			throw new Exception($"Error while retrieving scan data for scan {scanNumber}. {ex.Message}", ex);
		}
	}

	/// <summary>
	/// Gets the extended scan data for a scan
	/// </summary>
	/// <param name="scanNumber">scan number who's data is needed</param>
	/// <returns>The extended data</returns>
	internal IExtendedScanData GetExtendedScanData(int scanNumber)
	{
		try
		{
			int validIndexIntoScanIndices = GetValidIndexIntoScanIndices(scanNumber);
			ScanIndex scanIndex = _scanIndices[validIndexIntoScanIndices];
			if (scanIndex.PacketType.HasLabelPeaks())
			{
				IMsPacket msPacket = GetMsPacket(scanNumber, includeReferenceAndExceptionData: false, PacketFeatures.Debug, scanIndex);
				if (msPacket != null)
				{
					return msPacket.ExtendedData;
				}
			}
			return new AdvancedPacketBase.EmptyExtendedScanData();
		}
		catch (Exception ex)
		{
			throw new Exception($"Error while retrieving centroid peaks for {scanNumber}. {ex.Message}", ex);
		}
	}

	/// <summary>
	/// The method gets trailer extra for the scan number (1 relative)
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number - one relative.
	/// </param>
	/// <returns>
	/// The label value pairs for the trailer extra at the scan number.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// If the scan number is not in range or no data was found.
	/// </exception>
	public List<LabelValuePair> GetTrailerExtra(int scanNumber)
	{
		if (_trailerExtras == null || _trailerExtras.Count == 0)
		{
			return new List<LabelValuePair>();
		}
		bool validateReads;
		return GetValidatedTrailerExtraBlob(scanNumber, out validateReads).ReadLabelValuePairs(_trailerExtras.Decoder, validateReads);
	}

	/// <summary>
	/// This method gets one field from trailer extra for the scan number (1 relative)
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number - one relative.
	/// </param>
	/// <param name="item">index into available items (as per trailer extra header)</param>
	/// <returns>
	/// The value pairs for the trailer extra item at the scan number.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// If the scan number is not in range or no data was found.
	/// </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown when item is not an available filed</exception>
	public object GetTrailerExtraValue(int scanNumber, int item)
	{
		bool validateReads;
		LabelValueBlob validatedTrailerExtraBlob = GetValidatedTrailerExtraBlob(scanNumber, out validateReads);
		ILogDecoder decoder = _trailerExtras.Decoder;
		if (validatedTrailerExtraBlob.ValidItem(item, decoder))
		{
			return validatedTrailerExtraBlob.GetValueAt(decoder, item);
		}
		throw new ArgumentOutOfRangeException("item", "Item is not a valid field offset into the trailer");
	}

	/// <summary>
	/// This method gets all fields from trailer extra for the scan number
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The values for the trailer extra at the scan number.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// If the scan number is not in range or no data was found.
	/// </exception>
	public object[] GetTrailerExtraValues(int scanNumber)
	{
		bool validateReads;
		return GetValidatedTrailerExtraBlob(scanNumber, out validateReads).GetAllValues(_trailerExtras.Decoder);
	}

	/// <summary>
	/// The method gets the tune data at the index (0 relative).
	/// </summary>
	/// <param name="index">
	/// The index - 0 relative.
	/// </param>
	/// <returns>
	/// The list of <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.LabelValuePair" />s for the tune data.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// If the index is out of range.
	/// </exception>
	public List<LabelValuePair> GetTuneData(int index)
	{
		if (_tuneData == null || _tuneData.Count == 0)
		{
			return new List<LabelValuePair>();
		}
		if (index >= _tuneData.Count)
		{
			return new List<LabelValuePair>();
		}
		return _tuneData[index].ReadLabelValuePairs(_tuneData.Decoder);
	}

	/// <summary>
	/// The method gets the tune data at the index (0 relative).
	/// </summary>
	/// <param name="index">
	/// The index - 0 relative.
	/// </param>
	/// <returns>
	/// The list of <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.LabelValuePair" />s for the tune data.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// If the index is out of range.
	/// </exception>
	public object[] GetTuneDataValues(int index)
	{
		if (_tuneData == null || index < 0 || index >= _tuneData.Count)
		{
			return Array.Empty<object>();
		}
		return _tuneData[index].GetAllValues(_tuneData.Decoder);
	}

	/// <summary>
	/// Re-read the current file, to get the latest data.
	/// Only meaningful if the object has an implied backing file (such as IO.DLL and .raw files)
	/// No-op otherwise
	/// </summary>
	/// <returns>
	/// True if refreshed
	/// </returns>
	public override bool RefreshViewOfFile()
	{
		bool refreshed = true;
		try
		{
			if (base.RunHeader.IsInAcquisition)
			{
				if (!base.RefreshViewOfFile())
				{
					refreshed = false;
				}
				_runHeaderStruct = base.RunHeader.RunHeaderStruct;
				_acqScanEventViewer = UpdateMappedFile(ref refreshed, base.RunHeader.ScanEventsFilename, _acqScanEventViewer, 0L, 0L);
				if (_acqScanEventViewer != null)
				{
					ReadSegments(_acqScanEventViewer, 0L, base.FileRevision);
				}
				if (!_tuneData.RefreshViewOfFile())
				{
					refreshed = false;
				}
				PacketViewer = UpdateMappedFile(ref refreshed, base.RunHeader.DataPktFilename, PacketViewer, base.RawFileInformation.MsDataOffset, 0L);
				_acqScanIndexViewer = UpdateMappedFile(ref refreshed, base.RunHeader.SpectFilename, _acqScanIndexViewer, 0L, 0L);
				if (_acqScanIndexViewer != null)
				{
					_scanIndices?.Dispose();
					ReadScanIndices(_acqScanIndexViewer, 0L, base.FileRevision);
				}
				if (!TrailerScanEvents.RefreshViewOfFile())
				{
					refreshed = false;
				}
				if (!_trailerExtras.RefreshViewOfFile())
				{
					refreshed = false;
				}
			}
		}
		catch (Exception)
		{
			refreshed = false;
		}
		if (_massSpecScanList.IsValueCreated)
		{
			_previousMsScanList = _massSpecScanList.Value;
		}
		_massSpecScanList = new Lazy<ScanIndex[]>(MsScanList);
		return refreshed;
	}

	/// <summary>
	/// Update a map accessor
	/// </summary>
	/// <param name="refreshed">set to true if refreshed</param>
	/// <param name="file">name of map</param>
	/// <param name="accessor">current accessor</param>
	/// <param name="offset">offset into file (default 0)</param>
	/// <param name="size">size of map (default 0 = all data)</param>
	/// <returns>The updated accessor</returns>
	private IReadWriteAccessor UpdateMappedFile(ref bool refreshed, string file, IReadWriteAccessor accessor, long offset = 0L, long size = 0L)
	{
		accessor = accessor.GetMemoryMappedViewer(base.LoaderId, file, offset, size, inAcquisition: true, DataFileAccessMode.OpenCreateReadLoaderId);
		if (accessor == null)
		{
			refreshed = MemoryMappedFileHelper.IsFailedToMapAZeroLengthFile(StreamHelper.ConstructStreamId(base.LoaderId, file));
		}
		return accessor;
	}

	/// <summary>
	/// Get scan event with valid scan number.
	/// </summary>
	/// <param name="scanIndex">
	/// The scan index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEvent" />.
	/// </returns>
	/// <exception cref="T:System.Exception">Thrown if event cannot be read
	/// </exception>
	public ScanEvent ScanEventWithValidScanNumber(ScanIndex scanIndex)
	{
		try
		{
			if (scanIndex.IsScanTypeIndexSpecified)
			{
				ScanEvent scanEvent = GetScanEvents(scanIndex.ScanSegment)[scanIndex.ScanTypeIndex];
				bool flag = scanEvent.DependentDataFlag == ScanFilterEnums.IsDependent.Yes;
				if (!(scanEvent.IsCustom || flag))
				{
					return scanEvent;
				}
				ScanEvent scanEvent2 = TrailerScanEvents.GetEvent(scanIndex.TrailerOffset);
				if (flag)
				{
					scanEvent2.DependentDataAsByte = 1;
				}
				return scanEvent2;
			}
			return TrailerScanEvents.GetEvent(scanIndex.TrailerOffset);
		}
		catch (Exception ex)
		{
			throw new Exception($"Cannot get event for {scanIndex.ScanNumber}.\n{ex.Message}");
		}
	}

	/// <summary>
	/// Test if a scan number is valid.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <returns>
	/// True if valid.
	/// </returns>
	public bool ScanNumberIsValid(int scanNumber)
	{
		if (scanNumber >= _runHeaderStruct.FirstSpectrum)
		{
			return scanNumber <= _runHeaderStruct.LastSpectrum;
		}
		return false;
	}

	/// <summary>
	/// create chromatograms.
	/// </summary>
	/// <param name="settings">
	/// The settings.
	/// </param>
	/// <param name="timeRange">
	/// The time range.
	/// </param>
	/// <param name="toleranceOptions">
	/// The tolerance options.
	/// </param>
	/// <param name="includeReferenceAndExceptionData">
	/// set if reference and exception peaks should be included
	/// </param>
	/// <param name="alwaysUseAccuratePrecursors">If set: then precursor tolerance is based on
	/// the precision of the scan filters supplied
	/// (+/- half of the final digit).
	/// If not set, then precursors are matched based on settings logged by the device in the raw data</param>
	/// <returns>
	/// The generated data.
	/// </returns>
	internal ChromatogramDelivery[] CreateChromatograms(IChromatogramSettings[] settings, ThermoFisher.CommonCore.Data.Business.Range timeRange, MassOptions toleranceOptions, bool includeReferenceAndExceptionData, bool alwaysUseAccuratePrecursors)
	{
		return CreateChromatograms(ExtendChromatogramSettings(settings), timeRange, toleranceOptions, addBasePeaks: false, includeReferenceAndExceptionData, alwaysUseAccuratePrecursors);
	}

	/// <summary>
	/// create chromatograms.
	/// </summary>
	/// <param name="settings">
	/// The settings.
	/// </param>
	/// <param name="timeRange">
	/// The time range.
	/// </param>
	/// <param name="includeReferenceAndExceptionData">
	/// set if reference and exception peaks should be included
	/// </param>
	/// <returns>
	/// The generated data.
	/// </returns>
	internal ChromatogramDelivery[] CreateChromatograms(IChromatogramSettings[] settings, ThermoFisher.CommonCore.Data.Business.Range timeRange, bool includeReferenceAndExceptionData)
	{
		return CreateChromatograms(ExtendChromatogramSettings(settings), timeRange, null, addBasePeaks: false, includeReferenceAndExceptionData);
	}

	/// <summary>
	/// Create legacy format chromatograms, where all chromatograms share the same time range
	/// </summary>
	/// <param name="settings">Settings for each chromatogram</param>
	/// <param name="timeRange">Common start and end time</param>
	/// <param name="toleranceOptions">Mass tolerance settings</param>
	/// <param name="addBasePeaks">When true: Include base peak masses for all scans</param>
	/// <param name="includeReferenceAndExceptionData">Set if reference peaks are included in the chromatogram</param>
	/// <param name="alwaysUseAccuratePrecursors">If set: then precursor tolerance is based on
	/// the precision of the scan filters supplied
	/// (+/- half of the final digit).
	/// If not set, then precursors are matched based on settings logged by the device in the raw data</param>
	/// <returns>Definition of chromatograms to build</returns>
	internal ChromatogramDelivery[] CreateChromatograms(IList<IChromatogramSettingsEx> settings, ThermoFisher.CommonCore.Data.Business.Range timeRange, MassOptions toleranceOptions = null, bool addBasePeaks = false, bool includeReferenceAndExceptionData = false, bool alwaysUseAccuratePrecursors = false)
	{
		double low = timeRange.Low;
		double high = timeRange.High;
		int count = settings.Count;
		bool allFiltersSame;
		IScanFilter commonFilter;
		ChromatogramDelivery[][] array = CreateChromatogramJobs(settings, count, low, high, toleranceOptions, addBasePeaks, out allFiltersSame, out commonFilter);
		bool scansNeeded = array.Any((ChromatogramDelivery[] delivery) => delivery[0].Request.RequiresScanData);
		bool extendedScansNeeded = settings.Any((IChromatogramSettingsEx setting) => setting.Trace == TraceType.Custom);
		ChromatogramBatchGenerator chromatogramBatchGenerator = new ChromatogramBatchGenerator();
		IBufferPool pool = new ByteArrayBufferPool();
		ConfigureChromatogramGenerator(chromatogramBatchGenerator, scansNeeded, allFiltersSame, commonFilter, includeReferenceAndExceptionData, alwaysUseAccuratePrecursors, extendedScansNeeded, pool);
		ChromatogramDelivery[] chromatogramDeliveries = CreateFlatChromatogramList(array);
		Task.WaitAll(chromatogramBatchGenerator.GenerateChromatograms(chromatogramDeliveries));
		ChromatogramSignal chromatogramSignal = null;
		int num = array.Length;
		int num2 = num;
		if (addBasePeaks)
		{
			num2 = num - 1;
			chromatogramSignal = array[num2][0].DeliveredSignal;
		}
		ChromatogramDelivery[] array2 = new ChromatogramDelivery[num2];
		for (int num3 = 0; num3 < num2; num3++)
		{
			ChromatogramDelivery[] array3 = array[num3];
			if (array3.Length > 1)
			{
				array3[0].DeliveredSignal.SignalBasePeakMasses = array3[1].DeliveredSignal.Intensities.ToArray();
			}
			else if (chromatogramSignal != null)
			{
				AddBasePeakMasses(array3[0].DeliveredSignal, chromatogramSignal);
			}
			array2[num3] = array3[0];
		}
		return array2;
	}

	/// <summary>
	/// create a flat (1D array) chromatogram list, out of the collections per trace
	/// </summary>
	/// <param name="deliveries">
	/// The deliveries.
	/// </param>
	/// <returns>
	/// The flat list
	/// </returns>
	private static ChromatogramDelivery[] CreateFlatChromatogramList(ChromatogramDelivery[][] deliveries)
	{
		int num = 0;
		foreach (ChromatogramDelivery[] array in deliveries)
		{
			num += array.Length;
		}
		ChromatogramDelivery[] array2 = new ChromatogramDelivery[num];
		int num2 = 0;
		foreach (ChromatogramDelivery[] array3 in deliveries)
		{
			for (int k = 0; k < array3.Length; k++)
			{
				array2[num2++] = array3[k];
			}
		}
		return array2;
	}

	/// <summary>
	/// Gets the compound names, for all scans within range.
	/// </summary>
	/// <param name="startScan">first scan to test</param>
	/// <param name="endScan">last scan to test</param>
	/// <returns>The compound names</returns>
	public IEnumerable<string> GetCompoundNamesForScanRange(int startScan, int endScan)
	{
		SortedSet<string> sortedSet = new SortedSet<string>();
		AddNamesInScanRangeToSet(sortedSet, startScan, endScan);
		return sortedSet;
	}

	/// <summary>
	/// The method reads the scan index collection.
	/// </summary>
	/// <param name="viewer">
	/// Memory map of file
	/// </param>
	/// <param name="dataOffset">
	/// Offset into memory map
	/// </param>
	/// <param name="fileRevision">
	/// The file version.
	/// </param>
	/// <exception cref="T:System.Exception">
	/// Thrown if the number of scan index records is less than the number of spectra.
	/// </exception>
	/// <returns>
	/// The number of bytes decoded
	/// </returns>
	internal long ReadScanIndices(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		int numSpectra = base.RunHeader.NumSpectra;
		_scanIndices = viewer.LoadRawFileObjectExt(() => new ScanIndices(base.Manager, numSpectra), fileRevision, ref startPos);
		return startPos - dataOffset;
	}

	/// <summary>
	/// Updates the run header structure.
	/// </summary>
	/// <param name="runHeader">The run header.</param>
	/// <returns>The updated header</returns>
	internal ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader UpdateRunHeaderStruct(ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader runHeader)
	{
		base.RunHeader.Copy(runHeader);
		_runHeaderStruct = runHeader.RunHeaderStruct;
		return base.RunHeader;
	}

	/// <summary>
	/// Add a compound name.
	/// </summary>
	/// <param name="sortedCompoundNamesSet">
	/// The sorted compound names set.
	/// </param>
	/// <param name="scanFilterHelper">
	/// The scan filter helper.
	/// </param>
	/// <param name="scanEvent">
	/// The scan event.
	/// </param>
	private static void AddCompoundName(SortedSet<string> sortedCompoundNamesSet, ScanFilterHelper scanFilterHelper, ScanEvent scanEvent)
	{
		string name = scanEvent.Name;
		if (!string.IsNullOrWhiteSpace(name) && !sortedCompoundNamesSet.Contains(name) && ScanEventHelper.ScanEventHelperFactory(scanEvent).TestScanAgainstFilter(scanFilterHelper))
		{
			sortedCompoundNamesSet.Add(name);
		}
	}

	/// <summary>
	/// get frequencies.
	/// </summary>
	/// <param name="packet">
	/// The packet.
	/// </param>
	/// <returns>
	/// The (FT) frequency data
	/// </returns>
	public static double[] GetFrequencies(IMsPacket packet)
	{
		int num = 0;
		List<SegmentData> list = packet.SegmentPeaks ?? new List<SegmentData>(0);
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			num += list[i].DataPeaks.Count;
		}
		double[] array = new double[num];
		int num2 = 0;
		for (int j = 0; j < count; j++)
		{
			foreach (DataPeak dataPeak in list[j].DataPeaks)
			{
				array[num2++] = dataPeak.Frequency;
			}
		}
		return array;
	}

	/// <summary>
	/// Base peak data has the base peak mass recorded as the "intensity" for each scan
	/// Find the matching scans, and fill in this for the data
	/// </summary>
	/// <param name="data">filtered chromatogram, needing base peaks</param>
	/// <param name="basePeakData">Base peak masses</param>
	private void AddBasePeakMasses(ChromatogramSignal data, ChromatogramSignal basePeakData)
	{
		IList<double> intensities = basePeakData.Intensities;
		if (intensities == null || intensities.Count == 0)
		{
			return;
		}
		IList<int> scans = basePeakData.Scans;
		int count = scans.Count;
		int num = scans[0];
		int num2 = scans[count - 1];
		if (num + count - 1 != num2)
		{
			return;
		}
		int length = data.Length;
		double[] array = new double[length];
		int[] signalScans = data.SignalScans;
		for (int i = 0; i < length; i++)
		{
			int num3 = signalScans[i] - num;
			if (num3 < count)
			{
				array[i] = intensities[num3];
			}
		}
		data.SignalBasePeakMasses = array;
	}

	/// <summary>
	/// Configure the generator to get data from the current file's MS data.
	/// </summary>
	/// <param name="generator">Generator to configure</param>
	/// <param name="scansNeeded">True if this kind of chromatogram will need scan data (for example XIC).
	///     Tic chromatograms do not need the scan data.</param>
	/// <param name="allFiltersSame">All scan filters are the same</param>
	/// <param name="commonFilter">The filter used for all chromatograms</param>
	/// <param name="includeReferenceAndExceptionData">true if (calibration) ref and exception peaks should be included</param>
	/// <param name="alwaysUseAccuratePrecursors">If set: then precursor tolerance is based on
	/// the precision of the scan filters supplied
	/// (+/- half of the final digit).
	/// If not set, then precursors are matched based on settings logged by the device in the raw data</param>
	/// <param name="extendedScansNeeded">set if data such as "charge values" may be needed to support custom chromatograms</param>
	/// <param name="pool">Buffer pool, which reduces garbage collection</param>
	private void ConfigureChromatogramGenerator(IChromatogramBatchGenerator generator, bool scansNeeded, bool allFiltersSame, IScanFilter commonFilter, bool includeReferenceAndExceptionData, bool alwaysUseAccuratePrecursors, bool extendedScansNeeded, IBufferPool pool)
	{
		if (alwaysUseAccuratePrecursors)
		{
			generator.AccuratePrecursors = true;
		}
		else
		{
			bool accuratePrecursors = InstrumentDataConverter.CopyFrom(base.InstrumentId).IsTsqQuantumFile();
			generator.AccuratePrecursors = accuratePrecursors;
		}
		WrappedScanEvents allEvents = new WrappedScanEvents(this);
		if (scansNeeded)
		{
			ScanFilterHelper scanFilterHelper = null;
			if (allFiltersSame && commonFilter != null)
			{
				scanFilterHelper = new ScanFilterHelper(commonFilter, generator.AccuratePrecursors, commonFilter.MassPrecision);
				scanFilterHelper.InitializeAvailableEvents(allEvents);
			}
			FastScanFromMsData fastScanFromMsData = new FastScanFromMsData(this, scanFilterHelper, includeReferenceAndExceptionData, pool)
			{
				ExtendedData = extendedScansNeeded
			};
			generator.ScanReader = fastScanFromMsData.Reader;
			generator.ParallelScanReader = fastScanFromMsData.ParallelReader;
			long packetLength = CalculatePacketLength();
			int num = FindChunkLength(packetLength, _runHeaderStruct.FirstSpectrum, _runHeaderStruct.LastSpectrum);
			if (num > 0)
			{
				generator.ScansInChromatogramBatch = num;
			}
		}
		else
		{
			generator.ScanReader = new NoDataScanFromRawData(this).Reader;
		}
		generator.AvailableScans = _massSpecScanList.Value;
		generator.AllEvents = allEvents;
		generator.ReadAhead = ShouldBuffer;
	}

	private int FindChunkLength(long packetLength, int first, int last)
	{
		int suggestedChunkSize = PacketViewer.SuggestedChunkSize;
		if (packetLength > 0 && suggestedChunkSize > 0 && first > 0 && last > first)
		{
			int num = last - first + 1;
			long num2 = packetLength / num;
			if (num2 > 0)
			{
				int num3 = (int)Math.Min(suggestedChunkSize / num2, 50000L);
				if (num3 >= 10)
				{
					return num3;
				}
			}
		}
		return 0;
	}

	/// <summary>
	/// Create a set of jobs for legacy chromatogram generation,
	/// where all chromatograms share the same start and end time.
	/// </summary>
	/// <param name="settings">Definition of chromatograms needed</param>
	/// <param name="totalChros">Number of chromatograms to create</param>
	/// <param name="startTime">Start time of chromatograms</param>
	/// <param name="endTime">End time of chromatograms</param>
	/// <param name="toleranceOptions">Defines how mass tolerance is applied to mass ranges,
	/// where start and end mass are identical (within 1.0E-6):
	/// Default: null, range is +/- 0.5 from the low mass.
	/// If not null, delta mass value is calculated from low mass and tolerance, and
	/// a range is created as low mass +/- delta.
	/// When low and high mass are different, this parameter is unused.</param>
	/// <param name="addBasePeaks">If true: add a job to get base peak masses</param>
	/// <param name="allFiltersSame">Set if all chromatograms are using the same scan filter</param>
	/// <param name="commonFilter">When not null (when all filter are same), this is the common filter
	/// used by all chromatograms.</param>
	/// <returns>Configured list of chromatograms to be generated</returns>
	private ChromatogramDelivery[][] CreateChromatogramJobs(IList<IChromatogramSettingsEx> settings, int totalChros, double startTime, double endTime, MassOptions toleranceOptions, bool addBasePeaks, out bool allFiltersSame, out IScanFilter commonFilter)
	{
		int num = totalChros;
		if (addBasePeaks)
		{
			num++;
		}
		ChromatogramDelivery[][] array = new ChromatogramDelivery[num][];
		ThermoFisher.CommonCore.Data.Business.Range timeRange = ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime);
		int filterMassPrecision = base.RunHeader.FilterMassPrecision;
		allFiltersSame = true;
		string text = string.Empty;
		commonFilter = null;
		MsChromatogramSettingsConverter[] array2 = new MsChromatogramSettingsConverter[totalChros];
		for (int i = 0; i < totalChros; i++)
		{
			IChromatogramSettingsEx chromatogramSettingsEx = settings[i];
			if (i == 0)
			{
				text = chromatogramSettingsEx.Filter;
			}
			else if (allFiltersSame)
			{
				allFiltersSame = text == chromatogramSettingsEx.Filter;
			}
			array2[i] = new MsChromatogramSettingsConverter
			{
				TimeRange = timeRange,
				Tolerance = toleranceOptions,
				Settings = chromatogramSettingsEx,
				FileMassPrecision = filterMassPrecision,
				AddBasePeakMasses = addBasePeaks
			};
		}
		new ParallelSmallTasks().RunInParallel(array2);
		bool flag = true;
		for (int j = 0; j < totalChros; j++)
		{
			ChromatogramDelivery[] delivery = array2[j].Delivery;
			if (delivery == null || delivery.Length < 1)
			{
				throw new InvalidFilterFormatException(array2[j].ParseError);
			}
			array[j] = delivery;
			flag &= delivery[0].Request.ScanSelector.UseFilter;
		}
		if (((totalChros > 0) & allFiltersSame) && flag && !string.IsNullOrEmpty(text))
		{
			IScanSelect scanSelector = array[0][0].Request.ScanSelector;
			commonFilter = scanSelector.ScanFilter;
		}
		if (addBasePeaks)
		{
			IChromatogramRequest request = CreateInstrumentBasePeakMassChromatogram(startTime, endTime);
			array[num - 1] = new ChromatogramDelivery[1]
			{
				new ChromatogramDelivery
				{
					Request = request
				}
			};
		}
		return array;
	}

	/// <summary>
	/// Design a chromatogram point builder which just gets the Tic value from each scan header.
	/// </summary>
	/// <param name="startTime">Start time of chromatogram.</param>
	/// <param name="endTime">End time of chromatogram.</param>
	/// <returns>Tic chromatogram generator</returns>
	private IChromatogramRequest CreateInstrumentBasePeakMassChromatogram(double startTime, double endTime)
	{
		return new InstrumentBasePeakMassChromatogramPoint
		{
			RetentionTimeRange = ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime),
			ScanSelector = ScanSelect.SelectAll()
		};
	}

	/// <summary>
	/// Dispose of the buffer info and temp files.
	/// </summary>
	private void DisposeBufferInfoAndTempFiles()
	{
		bool deletePermitted = !base.InAcquisition;
		_trailerEventBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.TrailerScanEventsFilename, deletePermitted).DisposeBufferInfo();
		_trailerHeaderBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.TrailerHeaderFilename, deletePermitted).DisposeBufferInfo();
		_trailerExtraBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.TrailerExtraFilename, deletePermitted).DisposeBufferInfo();
		_tuneHeaderBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.TuneDataHeaderFilename, deletePermitted).DisposeBufferInfo();
		_tuneDataBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.TuneDataFilename, deletePermitted).DisposeBufferInfo();
		_scanHeaderBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.SpectFilename, deletePermitted).DisposeBufferInfo();
		_scanEventsBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.ScanEventsFilename, deletePermitted).DisposeBufferInfo();
		_peakDataBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.DataPktFilename, deletePermitted).DisposeBufferInfo();
	}

	/// <summary>
	/// extend chromatogram settings (to larger interface).
	/// </summary>
	/// <param name="settings">
	/// The settings.
	/// </param>
	/// <returns>
	/// The settings
	/// </returns>
	private IList<IChromatogramSettingsEx> ExtendChromatogramSettings(IChromatogramSettings[] settings)
	{
		if (settings is ChromatogramTraceSettings[] result)
		{
			return result;
		}
		List<IChromatogramSettingsEx> list = new List<IChromatogramSettingsEx>(settings.Length);
		foreach (IChromatogramSettings settings2 in settings)
		{
			list.Add(ChromatogramTraceSettings.FromChromatogramSettings(settings2));
		}
		return list;
	}

	/// <summary>
	/// fix up event codes.
	/// </summary>
	/// <param name="eventsForSegment">
	/// The events for segment.
	/// </param>
	/// <param name="segment">
	/// The segment.
	/// </param>
	private void FixUpEventCodes(List<ScanEvent> eventsForSegment, int segment)
	{
		int count = eventsForSegment.Count;
		int num = segment << 16;
		for (int i = 0; i < count; i++)
		{
			eventsForSegment[i].ScanTypeLocation = num + i;
		}
	}

	/// <summary>
	/// The method get scans event with a validated index to the scan index array.
	/// </summary>
	/// <param name="index">
	/// The index to the scan index array.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanEvent" />.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// Thrown if we cannot get the event.
	/// </exception>
	private ScanEvent GetScanEventWithValidScanNumber(int index)
	{
		return ScanEventWithValidScanNumber(_scanIndices[index]);
	}

	public Tuple<ScanIndex, ScanEvent> EventAndIndex(int index)
	{
		ScanIndex scanIndex = _scanIndices[index];
		return new Tuple<ScanIndex, ScanEvent>(scanIndex, ScanEventWithValidScanNumber(scanIndex));
	}

	/// <summary>
	/// The get validated trailer extra blob.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="validateReads">This is set where the stream doesn't have enough data for the record count</param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.LabelValueBlob" />.
	/// </returns>
	/// <exception cref="T:System.Exception">Thrown on null trailer blob
	/// </exception>
	private LabelValueBlob GetValidatedTrailerExtraBlob(int scanNumber, out bool validateReads)
	{
		int num = scanNumber - _runHeaderStruct.FirstSpectrum;
		if (scanNumber < _runHeaderStruct.FirstSpectrum || scanNumber > _runHeaderStruct.LastSpectrum)
		{
			ThrowScanNumberRangeException();
		}
		if (num >= _trailerExtras.Count)
		{
			throw new ArgumentOutOfRangeException("scanNumber", $"There is no record available for scan {scanNumber}. Available records: {_trailerExtras.Count}. Refer to Run Header");
		}
		validateReads = _trailerExtras.ValidateReads;
		return _trailerExtras[num] ?? throw new Exception($"No data found at {scanNumber}");
	}

	/// <summary>
	/// Validates a scan number
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The validated scan number
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// If the scan number is out of range.
	/// </exception>
	private int GetValidIndexIntoScanIndices(int scanNumber)
	{
		int num = scanNumber - _runHeaderStruct.FirstSpectrum;
		if (ScanNumberIsValid(scanNumber) && num < _scanIndices.Count)
		{
			return num;
		}
		ThrowScanNumberRangeException();
		return 0;
	}

	/// <summary>
	/// Initials the in acquisition.
	/// </summary>
	/// <param name="fileRevision">The file revision.</param>
	private void InAcquisitionInitializer(int fileRevision)
	{
		_trailerExtras = new GenericDataCollection(base.Manager, base.LoaderId, base.RunHeader.TrailerHeaderFilename, base.RunHeader.TrailerExtraFilename, () => base.RunHeader.NumTrailerExtra, fileRevision);
		_tuneData = new GenericDataCollection(base.Manager, base.LoaderId, base.RunHeader.TuneDataHeaderFilename, base.RunHeader.TuneDataFilename, () => base.RunHeader.NumTuneData, fileRevision);
		_trailerScanEventsLazy = new Lazy<TrailerScanEvents>(() => new TrailerScanEvents(base.Manager, base.LoaderId, base.RunHeader, fileRevision));
		PacketViewer = PacketViewer.GetMemoryMappedViewer(base.LoaderId, base.RunHeader.DataPktFilename, base.RawFileInformation.MsDataOffset, 0L, inAcquisition: true, DataFileAccessMode.OpenCreateReadLoaderId);
		InitializeBufferInfo();
	}

	/// <summary>
	/// Initializes the buffer information.
	/// </summary>
	private void InitializeBufferInfo()
	{
		_trailerEventBufferInfo = CreateBufferInfo("TRAILER_EVENTS");
		_trailerHeaderBufferInfo = CreateBufferInfo("TRAILERHEADER");
		_trailerExtraBufferInfo = CreateBufferInfo("TRAILEREXTRA");
		_tuneHeaderBufferInfo = CreateBufferInfo("TUNEDATAHEADER");
		_tuneDataBufferInfo = CreateBufferInfo("TUNEDATA_FILEMAP");
		_scanHeaderBufferInfo = CreateBufferInfo("SCANHEADER");
		_scanEventsBufferInfo = CreateBufferInfo("SCANEVENTS");
		_peakDataBufferInfo = CreateBufferInfo("PEAKDATA");
	}

	/// <summary>
	/// create a buffer info, for a part of this file.
	/// </summary>
	/// <param name="name">
	/// The name of the buffer
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.BufferInfo" />.
	/// </returns>
	private BufferInfo CreateBufferInfo(string name)
	{
		return new BufferInfo(base.LoaderId, base.DataFileMapName, name, creatable: false).UpdateErrors(DevErrors);
	}

	/// <summary>
	/// The method loads the MS device from file.
	/// </summary>
	/// <param name="viewer">Memory map of file</param>
	/// <param name="dataOffset">Offset within map</param>
	/// <param name="fileRevision">The file version.</param>
	/// <exception cref="T:System.Exception">
	/// If the run header is null.
	/// </exception>
	private new void Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		dataOffset += ReadSegments(viewer, dataOffset, fileRevision);
		_trailerExtras = viewer.LoadRawFileObjectExt(() => new GenericDataCollection(base.Manager, base.LoaderId, base.RunHeader.NumTrailerExtra, base.RunHeader.TrailerExtraPos), fileRevision, ref dataOffset);
		OffsetOfEndOfDevice = _trailerExtras.TotalSize + base.RunHeader.TrailerExtraPos;
		ValidateCount(base.RunHeader.NumTrailerExtra, _trailerExtras, "Trailer extra");
		_tuneData = viewer.LoadRawFileObjectExt(() => new GenericDataCollection(base.Manager, base.LoaderId, base.RunHeader.NumTuneData, -1L), fileRevision, ref dataOffset);
		ValidateCount(base.RunHeader.NumTuneData, _tuneData, "Tune data");
		dataOffset += _tuneData.TotalSize;
		dataOffset += ReadScanIndices(viewer, dataOffset, fileRevision);
		_trailerScanEventsLazy = new Lazy<TrailerScanEvents>(() => viewer.LoadRawFileObjectExt(() => new TrailerScanEvents(base.Manager, base.LoaderId, base.RunHeader), fileRevision, ref dataOffset));
	}

	/// <summary>
	/// Create the list of all MS scans.
	/// </summary>
	/// <returns>
	/// The list of scan headers.
	/// </returns>
	private ScanIndex[] MsScanList()
	{
		ScanIndex[] result = GenerateMsScanList();
		_scanIndices.ClearCaches();
		return result;
	}

	/// <summary>
	/// Create the list of all MS scans.
	/// </summary>
	/// <returns>
	/// The list of scan headers.
	/// </returns>
	private ScanIndex[] GenerateMsScanList()
	{
		ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader runHeader = base.RunHeader;
		int first = runHeader.FirstSpectrum;
		int lastSpectrum = runHeader.LastSpectrum;
		if (first >= 1 && lastSpectrum >= first)
		{
			int num = lastSpectrum - first + 1;
			ScanIndex[] previousMsScanList = _previousMsScanList;
			ScanIndex[] toReturn;
			if (previousMsScanList != null && previousMsScanList.Length != 0)
			{
				int fromInclusive = first + _previousMsScanList.Length;
				toReturn = _previousMsScanList;
				Array.Resize(ref toReturn, num);
				Parallel.For(fromInclusive, lastSpectrum + 1, delegate(int i)
				{
					toReturn[i - first] = GetMsScanIndexLocal(i);
				});
				return toReturn;
			}
			toReturn = new ScanIndex[num];
			int num2 = Math.Max(10, lastSpectrum / 50);
			if (_scanIndices.HasRecordBuffer)
			{
				num2 = Math.Min(num2, _scanIndices.RecordsPerBatch / 2);
				ParallelOptions parallelOptions = new ParallelOptions
				{
					MaxDegreeOfParallelism = 3
				};
				Parallel.ForEach(Partitioner.Create(first, lastSpectrum + 1, num2), parallelOptions, delegate(Tuple<int, int> range)
				{
					for (int i = range.Item1; i < range.Item2; i++)
					{
						toReturn[i - first] = GetMsScanIndexLocal(i);
					}
				});
			}
			else
			{
				Parallel.ForEach(Partitioner.Create(first, lastSpectrum + 1, num2), delegate(Tuple<int, int> range)
				{
					for (int i = range.Item1; i < range.Item2; i++)
					{
						toReturn[i - first] = GetMsScanIndexLocal(i);
					}
				});
			}
			return toReturn;
		}
		return Array.Empty<ScanIndex>();
		ScanIndex GetMsScanIndexLocal(int spectrum)
		{
			int validIndexIntoScanIndices = GetValidIndexIntoScanIndices(spectrum);
			return _scanIndices[validIndexIntoScanIndices];
		}
	}

	/// <summary>
	/// Initials the non acquisition.
	/// </summary>
	/// <param name="rawFileName">Name of the raw file.</param>
	/// <param name="fileRevision">The file revision.</param>
	private void NonAcquisitionInitializer(string rawFileName, int fileRevision)
	{
		long size = CalculatePacketLength();
		PacketViewer = base.Manager.GetRandomAccessViewer(base.LoaderId, rawFileName, _runHeaderStruct.PacketPos, size, inAcquisition: false);
		Load(base.RawDataViewer, base.OffsetOfTheEndOfDeviceCommonInfo, fileRevision);
	}

	private long CalculatePacketLength()
	{
		long result = 0L;
		long statusLogPos = _runHeaderStruct.StatusLogPos;
		long packetPos = _runHeaderStruct.PacketPos;
		if (statusLogPos > packetPos)
		{
			result = statusLogPos - packetPos;
		}
		return result;
	}

	/// <summary>
	/// Reads the scan events.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <param name="fileRevision">The file version.</param>
	/// <returns>The scan events</returns>
	private List<ScanEvent> ReadScanEvents(IMemoryReader viewer, ref long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		int numberOfEvents = viewer.ReadIntExt(ref startPos);
		List<ScanEvent> scanEvents = new List<ScanEvent>(numberOfEvents);
		startPos = (viewer.PreferLargeReads ? Utilities.LoadDataFromInternalMemoryArrayReader(GetScanEventsData, viewer, startPos, numberOfEvents * 512) : GetScanEventsData(viewer, startPos));
		dataOffset = startPos;
		return scanEvents;
		long GetScanEventsData(IMemoryReader reader, long offset)
		{
			for (int i = 0; i < numberOfEvents; i++)
			{
				int index = i;
				scanEvents.Add(reader.LoadRawFileObjectExt(() => new ScanEvent(base.RunHeader, index), fileRevision, ref offset));
			}
			return offset;
		}
	}

	/// <summary>
	/// The method reads the segments.
	/// </summary>
	/// <param name="viewer">Memory map of file
	/// </param>
	/// <param name="dataOffset">Offset into memory map
	/// </param>
	/// <param name="fileVersion">
	/// The file version.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	private long ReadSegments(IMemoryReader viewer, long dataOffset, int fileVersion)
	{
		long startPos = dataOffset;
		int num = viewer.ReadIntExt(ref startPos);
		_scanEventSegments.Clear();
		for (int i = 0; i < num; i++)
		{
			List<ScanEvent> list = ReadScanEvents(viewer, ref startPos, fileVersion);
			FixUpEventCodes(list, i);
			_scanEventSegments.Add(list);
		}
		return startPos - dataOffset;
	}

	/// <summary>
	/// throw scan number range exception.
	/// </summary>
	/// <exception cref="T:System.Exception">Always throw scan number range exception.
	/// </exception>
	private void ThrowScanNumberRangeException()
	{
		throw new IndexOutOfRangeException($"The scan number must be >= {_runHeaderStruct.FirstSpectrum} and <= {_runHeaderStruct.LastSpectrum}.");
	}

	/// <summary>
	/// validate count.
	/// </summary>
	/// <param name="expected">
	/// The expected count.
	/// </param>
	/// <param name="collection">
	/// The collection.
	/// </param>
	/// <param name="message">
	/// The message.
	/// </param>
	/// <exception cref="T:System.Exception">Thrown if cunt not as expected
	/// </exception>
	private void ValidateCount(int expected, GenericDataCollection collection, string message)
	{
		if (expected != collection.Count)
		{
			throw new Exception(string.Format("The number of {0} records read ({1}) does not match the number of {0} items recorded in the header({2}).", message, collection.Count, expected));
		}
	}

	/// <summary>
	/// Get the scan filters for each of the compound names.
	/// </summary>
	/// <param name="compoundNames">
	/// The compound names.
	/// </param>
	/// <returns>
	/// The set of filters for each name.
	/// result [n] is the array of filters for compound name [n]
	/// </returns>
	public string[][] GetScanFiltersFromCompoundNames(string[] compoundNames)
	{
		int filterMassPrecision = base.RunHeader.FilterMassPrecision;
		int numScanEventSegments = NumScanEventSegments;
		int compoundCount = compoundNames.Length;
		SortedSet<string>[] sortedFilterStrings = new SortedSet<string>[compoundCount];
		for (int i = 0; i < compoundCount; i++)
		{
			sortedFilterStrings[i] = new SortedSet<string>();
		}
		if (numScanEventSegments > 0)
		{
			for (int j = 0; j < numScanEventSegments; j++)
			{
				List<ScanEvent> scanEvents = GetScanEvents(j);
				if (scanEvents == null || !scanEvents.Any())
				{
					continue;
				}
				int count = scanEvents.Count;
				for (int k = 0; k < count; k++)
				{
					ScanEvent scanEvent = scanEvents[k];
					for (int l = 0; l < compoundCount; l++)
					{
						string strB = compoundNames[l];
						if (string.Compare(scanEvent.Name, strB, StringComparison.OrdinalIgnoreCase) == 0)
						{
							string filterString = new FilterScanEvent(scanEvent.CreateEditor()).GetFilterString(filterMassPrecision);
							if (!string.IsNullOrWhiteSpace(filterString))
							{
								sortedFilterStrings[l].Add(filterString);
							}
						}
					}
				}
			}
		}
		else
		{
			int firstSpectrum = base.RunHeader.FirstSpectrum;
			int lastSpectrum = base.RunHeader.LastSpectrum;
			ConcurrentDictionary<string, string>[] concurrentFilterStrings = new ConcurrentDictionary<string, string>[compoundCount];
			for (int m = 0; m < compoundCount; m++)
			{
				concurrentFilterStrings[m] = new ConcurrentDictionary<string, string>();
			}
			Parallel.ForEach(Partitioner.Create(firstSpectrum, lastSpectrum + 1, 50), delegate(Tuple<int, int> range)
			{
				for (int n = range.Item1; n < range.Item2; n++)
				{
					ScanEvent scanEvent2 = GetScanEvent(n);
					for (int num2 = 0; num2 < compoundCount; num2++)
					{
						string strB2 = compoundNames[num2];
						if (string.Compare(scanEvent2.Name, strB2, StringComparison.OrdinalIgnoreCase) == 0)
						{
							string filterString2 = new FilterScanEvent(scanEvent2.CreateEditor()).GetFilterString(filterMassPrecision);
							concurrentFilterStrings[num2].TryAdd(filterString2, filterString2);
						}
					}
				}
			});
			Parallel.For(0, compoundCount, delegate(int compound)
			{
				ConcurrentDictionary<string, string> obj = concurrentFilterStrings[compound];
				SortedSet<string> sortedSet = sortedFilterStrings[compound];
				foreach (KeyValuePair<string, string> item in obj)
				{
					sortedSet.Add(item.Value);
				}
			});
		}
		string[][] array = new string[compoundCount][];
		for (int num = 0; num < compoundCount; num++)
		{
			array[num] = sortedFilterStrings[num].ToArray();
		}
		return array;
	}
}
