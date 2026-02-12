using System;
using System.Threading;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.RunHeaders;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

/// <summary>
/// The run header.
/// </summary>
internal sealed class RunHeader : ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader, IRealTimeAccess, IRunHeaderAccess, IDisposable, IRawObjectBase, ThermoFisher.CommonCore.Data.Interfaces.IRunHeader
{
	private readonly Guid _loaderId;

	private int _numSpectra;

	private RunHeaderStruct _runHeaderStruct;

	private bool _disposed;

	private IReadWriteAccessor _acqHeaderViewer;

	public IViewCollectionManager Manager { get; }

	/// <summary>
	/// Gets the software revision.
	/// </summary>
	public int Revision => _runHeaderStruct.Revision;

	/// <summary>
	/// Gets or sets the start time.
	/// </summary>
	public double StartTime
	{
		get
		{
			return _runHeaderStruct.StartTime;
		}
		set
		{
			_runHeaderStruct.StartTime = value;
		}
	}

	/// <summary>
	/// Gets or sets the end time.
	/// </summary>
	public double EndTime
	{
		get
		{
			return _runHeaderStruct.EndTime;
		}
		set
		{
			_runHeaderStruct.EndTime = value;
		}
	}

	/// <summary>
	/// Gets the first spectrum.
	/// </summary>
	public int FirstSpectrum => _runHeaderStruct.FirstSpectrum;

	/// <summary>
	/// Gets the last spectrum.
	/// </summary>
	public int LastSpectrum => _runHeaderStruct.LastSpectrum;

	/// <summary>
	/// Gets or sets the low mass.
	/// </summary>
	public double LowMass
	{
		get
		{
			return _runHeaderStruct.LowMass;
		}
		set
		{
			_runHeaderStruct.LowMass = value;
		}
	}

	/// <summary>
	/// Gets or sets the high mass.
	/// </summary>
	public double HighMass
	{
		get
		{
			return _runHeaderStruct.HighMass;
		}
		set
		{
			_runHeaderStruct.HighMass = value;
		}
	}

	/// <summary>
	/// Gets the maximum intensity.
	/// </summary>
	public int MaxIntensity => _runHeaderStruct.MaxIntensity;

	/// <summary>
	/// Gets or sets the max integrated intensity.
	/// </summary>
	public double MaxIntegratedIntensity
	{
		get
		{
			return _runHeaderStruct.MaxIntegIntensity;
		}
		set
		{
			_runHeaderStruct.MaxIntegIntensity = value;
		}
	}

	/// <summary>
	/// Gets or sets the device file offset.
	/// </summary>
	public long DeviceFileOffset
	{
		get
		{
			return _runHeaderStruct.ControllerInfo.Offset;
		}
		set
		{
			_runHeaderStruct.ControllerInfo.Offset = value;
		}
	}

	/// <summary>
	/// Gets or sets the device index.
	/// </summary>
	public int DeviceIndex
	{
		get
		{
			return _runHeaderStruct.ControllerInfo.VirtualDeviceIndex;
		}
		set
		{
			_runHeaderStruct.ControllerInfo.VirtualDeviceIndex = value;
		}
	}

	/// <summary>
	/// Gets or sets the device type.
	/// </summary>
	public VirtualDeviceTypes DeviceType
	{
		get
		{
			return _runHeaderStruct.ControllerInfo.VirtualDeviceType;
		}
		set
		{
			_runHeaderStruct.ControllerInfo.VirtualDeviceType = value;
		}
	}

	/// <summary>
	/// Gets the error log filename.
	/// </summary>
	public string ErrorLogFilename => _runHeaderStruct.ErrorLogFile;

	/// <summary>
	/// Gets or sets the error log position.
	/// </summary>
	public long ErrorLogPos
	{
		get
		{
			return _runHeaderStruct.ErrorLogPos;
		}
		set
		{
			_runHeaderStruct.ErrorLogPos = value;
		}
	}

	/// <summary>
	/// Gets or sets the expected run time.
	/// </summary>
	public double ExpectedRunTime
	{
		get
		{
			return _runHeaderStruct.ExpectedRunTime;
		}
		set
		{
			_runHeaderStruct.ExpectedRunTime = value;
		}
	}

	/// <summary>
	/// Gets or the expected run time (Data.Interfaces.IRunHeader version)
	/// </summary>
	public double ExpectedRuntime => _runHeaderStruct.ExpectedRunTime;

	/// <summary>
	/// Gets or sets the filter mass precision.
	/// </summary>
	public int FilterMassPrecision
	{
		get
		{
			return _runHeaderStruct.FilterMassPrecision;
		}
		set
		{
			_runHeaderStruct.FilterMassPrecision = value;
		}
	}

	/// <summary>
	/// Gets the instrument id file name.
	/// </summary>
	public string InstIdFilename => _runHeaderStruct.InstIDFile;

	/// <summary>
	/// Gets the instrument scan events file name.
	/// </summary>
	public string InstScanEventsFilename => _runHeaderStruct.InstScanEventsFile;

	/// <summary>
	/// Gets or sets a value indicating whether is in acquisition.
	/// </summary>
	public bool IsInAcquisition
	{
		get
		{
			return _runHeaderStruct.IsInAcquisition;
		}
		set
		{
			_runHeaderStruct.IsInAcquisition = value;
		}
	}

	/// <summary>
	/// Gets or sets the mass resolution.
	/// </summary>
	public double MassResolution
	{
		get
		{
			return _runHeaderStruct.MassResolution;
		}
		set
		{
			_runHeaderStruct.MassResolution = value;
		}
	}

	/// <summary>
	/// Gets the protocol used to create this file.
	/// </summary>
	public string WriterProtocol => _runHeaderStruct.InstDesc;

	/// <summary>
	/// Gets the number of error log entries.
	/// </summary>
	public int NumErrorLog => _runHeaderStruct.NumErrorLog;

	/// <summary>
	/// Gets or sets the number of spectra.
	/// </summary>
	public int NumSpectra
	{
		get
		{
			return _numSpectra;
		}
		set
		{
			if (_runHeaderStruct.IsInAcquisition)
			{
				_numSpectra = (_runHeaderStruct.LastSpectrum = _runHeaderStruct.FirstSpectrum + value - 1);
			}
		}
	}

	/// <summary>
	/// Gets the number of status log entries.
	/// </summary>
	public int NumStatusLog => _runHeaderStruct.NumStatusLog;

	/// <summary>
	/// Gets or sets the number of trailer extras.
	/// </summary>
	public int NumTrailerExtra
	{
		get
		{
			return _runHeaderStruct.NumTrailerExtra;
		}
		set
		{
			_runHeaderStruct.NumTrailerExtra = value;
		}
	}

	/// <summary>
	/// Gets or sets the number of trailer scan events.
	/// </summary>
	public int NumTrailerScanEvents
	{
		get
		{
			return _runHeaderStruct.NumTrailerScanEvents;
		}
		set
		{
			_runHeaderStruct.NumTrailerScanEvents = value;
		}
	}

	/// <summary>
	/// Gets or sets the number of tune data.
	/// </summary>
	public int NumTuneData
	{
		get
		{
			return _runHeaderStruct.NumTuneData;
		}
		set
		{
			_runHeaderStruct.NumTuneData = value;
		}
	}

	/// <summary>
	/// Gets or sets the packet position.
	/// </summary>
	public long PacketPos
	{
		get
		{
			return _runHeaderStruct.PacketPos;
		}
		set
		{
			_runHeaderStruct.PacketPos = value;
		}
	}

	/// <summary>
	/// Gets the run header position.
	/// </summary>
	public long RunHeaderPos => _runHeaderStruct.RunHeaderPos;

	/// <summary>
	/// Gets the scan events filename.
	/// </summary>
	public string ScanEventsFilename => _runHeaderStruct.ScanEventsFile;

	/// <summary>
	/// Gets the spectra filename.
	/// </summary>
	public string SpectFilename => _runHeaderStruct.SpectrumFile;

	/// <summary>
	/// Gets or sets the spectrum position.
	/// </summary>
	public long SpectrumPos
	{
		get
		{
			return _runHeaderStruct.SpectPos;
		}
		set
		{
			_runHeaderStruct.SpectPos = value;
		}
	}

	/// <summary>
	/// Gets the status log filename.
	/// </summary>
	public string StatusLogFilename => _runHeaderStruct.StatusLogFile;

	/// <summary>
	/// Gets the status log header filename.
	/// </summary>
	public string StatusLogHeaderFilename => _runHeaderStruct.StatusLogHeaderFile;

	/// <summary>
	/// Gets or sets the status log position.
	/// </summary>
	public long StatusLogPos
	{
		get
		{
			return _runHeaderStruct.StatusLogPos;
		}
		set
		{
			_runHeaderStruct.StatusLogPos = value;
		}
	}

	/// <summary>
	/// Gets the tolerance unit.
	/// </summary>
	public ThermoFisher.CommonCore.RawFileReader.Facade.Constants.ToleranceUnits ToleranceUnit => (ThermoFisher.CommonCore.RawFileReader.Facade.Constants.ToleranceUnits)_runHeaderStruct.ToleranceUnit;

	/// <summary>
	/// Gets the tolerance unit.
	/// </summary>
	ThermoFisher.CommonCore.Data.ToleranceUnits IRunHeaderAccess.ToleranceUnit => (ThermoFisher.CommonCore.Data.ToleranceUnits)_runHeaderStruct.ToleranceUnit;

	/// <summary>
	/// Gets the trailer extra filename.
	/// </summary>
	public string TrailerExtraFilename => _runHeaderStruct.TrailerExtraFile;

	/// <summary>
	/// Gets or sets the trailer extra position.
	/// </summary>
	public long TrailerExtraPos
	{
		get
		{
			return _runHeaderStruct.TrailerExtraPos;
		}
		set
		{
			_runHeaderStruct.TrailerExtraPos = value;
		}
	}

	/// <summary>
	/// Gets the trailer header filename. (MS)
	/// </summary>
	public string TrailerHeaderFilename => _runHeaderStruct.TrailerHeaderFile;

	/// <summary>
	/// Gets the channel header filename. (Channel device)
	/// Note that this reused the same slot as "TrailerHeader" as differnt
	/// device types need differnt sub files
	/// </summary>
	public string ChannelHeaderFilename => _runHeaderStruct.TrailerHeaderFile;

	/// <summary>
	/// Gets the trailer scan events filename.
	/// </summary>
	public string TrailerScanEventsFilename => _runHeaderStruct.TrailerScanEventsFile;

	/// <summary>
	/// Gets or sets the trailer scan events position.
	/// </summary>
	public long TrailerScanEventsPos
	{
		get
		{
			return _runHeaderStruct.TrailerScanEventsPos;
		}
		set
		{
			_runHeaderStruct.TrailerScanEventsPos = value;
		}
	}

	/// <summary>
	/// Gets the tune data filename.
	/// </summary>
	public string TuneDataFilename => _runHeaderStruct.TuneDataFile;

	/// <summary>
	/// Gets the tune data header filename.
	/// </summary>
	public string TuneDataHeaderFilename => _runHeaderStruct.TuneDataHeaderFile;

	/// <summary>
	/// Gets or sets the comment 1.
	/// </summary>
	public string Comment1
	{
		get
		{
			return _runHeaderStruct.Comment1;
		}
		set
		{
			_runHeaderStruct.Comment1 = value;
		}
	}

	/// <summary>
	/// Gets the (internal) description tag.
	/// </summary>
	internal string InstrumentDescription => _runHeaderStruct.InstDesc;

	/// <summary>
	/// Gets or sets the comment 2.
	/// </summary>
	public string Comment2
	{
		get
		{
			return _runHeaderStruct.Comment2;
		}
		set
		{
			_runHeaderStruct.Comment2 = value;
		}
	}

	/// <summary>
	/// Gets the data packet filename.
	/// </summary>
	public string DataPktFilename => _runHeaderStruct.DataPktFile;

	/// <summary>
	/// Gets the file revision.
	/// </summary>
	public int FileRevision { get; private set; }

	/// <summary>
	/// Gets the header file map name. <para />
	/// It's only meaningful in Generic data.
	/// </summary>
	public string HeaderFileMapName { get; private set; }

	/// <summary>
	/// Gets the data file map name.
	/// </summary>
	public string DataFileMapName { get; private set; }

	/// <summary>
	/// Gets the internal run header structure.
	/// </summary>
	/// <returns></returns>
	public RunHeaderStruct RunHeaderStruct => _runHeaderStruct;

	public int SpectraCount => _numSpectra;

	public int StatusLogCount => NumStatusLog;

	public int TuneDataCount => NumTuneData;

	public int ErrorLogCount => NumErrorLog;

	public int TrailerScanEventCount => NumTrailerScanEvents;

	public int TrailerExtraCount => NumTrailerExtra;

	public int InAcquisition => _runHeaderStruct.IsInAcquisition ? 1 : 0;

	ThermoFisher.CommonCore.Data.ToleranceUnits ThermoFisher.CommonCore.Data.Interfaces.IRunHeader.ToleranceUnit => (ThermoFisher.CommonCore.Data.ToleranceUnits)_runHeaderStruct.ToleranceUnit;

	double ThermoFisher.CommonCore.Data.Interfaces.IRunHeader.MaxIntensity => _runHeaderStruct.MaxIntensity;

	/// <summary>
	/// Gets the domain of the device logging the data.
	/// For legacy Xcalibur raw data, up to and including V 66, this is defined as "Legacy data".
	/// At V 66 or later, this is 0 for Legacy data, and other values for data from other various luna systems.
	/// </summary>
	public RawDataDomain DeviceDataDomain
	{
		get
		{
			if (FileRevision < 66)
			{
				return RawDataDomain.Legacy;
			}
			return (RawDataDomain)_runHeaderStruct.Flags[0];
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.RunHeader" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">Loader instance Id. </param>
	public RunHeader(IViewCollectionManager manager, Guid loaderId)
	{
		Manager = manager;
		_acqHeaderViewer = null;
		_loaderId = loaderId;
		_runHeaderStruct = default(RunHeaderStruct);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.RunHeader" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">Loader instance Id</param>
	/// <param name="runHeader">The run header.</param>
	public RunHeader(IViewCollectionManager manager, Guid loaderId, ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader runHeader)
		: this(manager, loaderId)
	{
		Copy(runHeader);
	}

	/// <summary>
	/// Initializes a new instance of the RunHeader class for real time reading.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">Loader instance Id</param>
	/// <param name="mapName">Name of the map.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="inCreation">True if it's in device writer creation, otherwise false</param>
	/// <param name="readOnly">True if it's in reading mode, otherwise false</param>
	public RunHeader(IViewCollectionManager manager, Guid loaderId, string mapName, int fileRevision, bool inCreation = false, bool readOnly = true)
		: this(manager, loaderId)
	{
		FileRevision = fileRevision;
		HeaderFileMapName = string.Empty;
		DataFileMapName = mapName + "FMAT_RUNHEADER";
		if (!(inCreation && !readOnly))
		{
			RefreshViewOfFile();
		}
	}

	/// <summary>
	/// Loads the specified viewer.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <returns>The number of read bytes </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		if (fileRevision >= 66)
		{
			_runHeaderStruct = viewer.ReadStructureExt<RunHeaderStruct>(ref startPos);
		}
		else if (fileRevision >= 64)
		{
			_runHeaderStruct = viewer.ReadPreviousRevisionAndConvertExt<RunHeaderStruct, RunHeaderStruct5>(ref startPos);
		}
		else if (fileRevision >= 49)
		{
			_runHeaderStruct = viewer.ReadPreviousRevisionAndConvertExt<RunHeaderStruct, RunHeaderStruct4>(ref startPos);
		}
		else if (fileRevision >= 40)
		{
			_runHeaderStruct = viewer.ReadPreviousRevisionAndConvertExt<RunHeaderStruct, RunHeaderStruct3>(ref startPos);
		}
		else if (fileRevision >= 25)
		{
			_runHeaderStruct = viewer.ReadPreviousRevisionAndConvertExt<RunHeaderStruct, RunHeaderStruct2>(ref startPos);
		}
		else
		{
			_runHeaderStruct = viewer.ReadPreviousRevisionAndConvertExt<RunHeaderStruct, RunHeaderStruct1>(ref startPos);
		}
		if (fileRevision < 25)
		{
			_runHeaderStruct.DataPktFile = viewer.ReadStringExt(ref startPos);
			_runHeaderStruct.SpectrumFile = viewer.ReadStringExt(ref startPos);
			_runHeaderStruct.StatusLogFile = viewer.ReadStringExt(ref startPos);
			_runHeaderStruct.ErrorLogFile = viewer.ReadStringExt(ref startPos);
			_runHeaderStruct.RunHeaderPos = 0L;
			_runHeaderStruct.ScanEventsFile = string.Empty;
			_runHeaderStruct.TuneDataFile = string.Empty;
			_runHeaderStruct.InstIDFile = string.Empty;
			_runHeaderStruct.InstScanEventsFile = string.Empty;
			_runHeaderStruct.ExpectedRunTime = _runHeaderStruct.EndTime;
			_runHeaderStruct.MassResolution = 0.5;
			_runHeaderStruct.ControllerInfo.VirtualDeviceType = VirtualDeviceTypes.MsDevice;
			_runHeaderStruct.ControllerInfo.VirtualDeviceIndex = 0;
			_runHeaderStruct.ControllerInfo.Offset = 0L;
			_runHeaderStruct.TrailerScanEventsFile = string.Empty;
			_runHeaderStruct.TrailerHeaderFile = string.Empty;
			_runHeaderStruct.TrailerExtraFile = string.Empty;
			_runHeaderStruct.StatusLogHeaderFile = string.Empty;
			_runHeaderStruct.TuneDataHeaderFile = string.Empty;
			_runHeaderStruct.TrailerScanEventsPos32Bit = 0;
			_runHeaderStruct.TrailerExtraPos32Bit = 0;
			_runHeaderStruct.NumTrailerScanEvents = 0;
			_runHeaderStruct.NumTrailerExtra = 0;
			_runHeaderStruct.NumTuneData = 0;
		}
		FileRevision = fileRevision;
		if (fileRevision < 40)
		{
			_runHeaderStruct.ToleranceUnit = 2;
		}
		if (fileRevision < 49)
		{
			_runHeaderStruct.FilterMassPrecision = 2;
		}
		if (fileRevision <= 63)
		{
			_runHeaderStruct = StructureConversion.ConvertFrom32Bit(_runHeaderStruct);
		}
		_numSpectra = ((_runHeaderStruct.LastSpectrum > 0) ? (_runHeaderStruct.LastSpectrum - _runHeaderStruct.FirstSpectrum + 1) : 0);
		return startPos - dataOffset;
	}

	/// <summary>
	/// Copies the specified source.
	/// </summary>
	/// <param name="src">The source.</param>
	public void Copy(ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader src)
	{
		CopyRunHeaderStruct(ref ((RunHeader)src)._runHeaderStruct);
		_numSpectra = ((RunHeader)src)._numSpectra;
		FileRevision = src.FileRevision;
		HeaderFileMapName = src.HeaderFileMapName;
		DataFileMapName = src.DataFileMapName;
	}

	/// <summary>
	/// Copies the run header structure.
	/// </summary>
	/// <param name="runHeaderStruct">The run header structure.</param>
	public void CopyRunHeaderStruct(ref RunHeaderStruct runHeaderStruct)
	{
		_runHeaderStruct = runHeaderStruct;
		Array.Copy(_runHeaderStruct.Flags, runHeaderStruct.Flags, 8);
	}

	/// <summary>
	/// Initializes this instance.
	/// </summary>
	/// <param name="revision">sets the revision of the device data format</param>
	/// <param name="dataDomain">Examples are Xcalibur, Chromeleon etc.
	/// some data for devices have additional features depending on the domain.</param>
	public void Initialize(short revision = 1, RawDataDomain dataDomain = RawDataDomain.MassSpectrometry)
	{
		_runHeaderStruct.IsInAcquisition = true;
		_runHeaderStruct.FirstSpectrum = 1;
		_runHeaderStruct.Revision = revision;
		_runHeaderStruct.InstDesc = "C#000";
		_runHeaderStruct.MassResolution = 0.5;
		_runHeaderStruct.ToleranceUnit = 2;
		_runHeaderStruct.FilterMassPrecision = 2;
		_runHeaderStruct.Flags = new byte[8];
		_runHeaderStruct.Flags[0] = (byte)dataDomain;
		_runHeaderStruct.LowMass = double.MaxValue;
		_runHeaderStruct.HighMass = 0.0;
		_runHeaderStruct.MaxIntegIntensity = 0.0;
		_runHeaderStruct.StartTime = double.MaxValue;
		_runHeaderStruct.EndTime = 0.0;
	}

	/// <summary>
	/// Increments the number of status log.
	/// </summary>
	/// <returns>The incremented value</returns>
	public int IncrementNumStatusLog()
	{
		return Interlocked.Increment(ref _runHeaderStruct.NumStatusLog);
	}

	/// <summary>
	/// Increments the number of error log.
	/// </summary>
	/// <returns>The incremented value</returns>
	public int IncrementNumErrorLog()
	{
		return Interlocked.Increment(ref _runHeaderStruct.NumErrorLog);
	}

	/// <summary>
	/// Increments the number of trailer extra.
	/// </summary>
	/// <returns>The incremented value</returns>
	public int IncrementNumTrailerExtra()
	{
		return Interlocked.Increment(ref _runHeaderStruct.NumTrailerExtra);
	}

	/// <summary>
	/// Increments the number of tune data.
	/// </summary>
	/// <returns>The incremented value</returns>
	public int IncrementNumTuneData()
	{
		return Interlocked.Increment(ref _runHeaderStruct.NumTuneData);
	}

	/// <summary>
	/// Increments the trailer scan events.
	/// </summary>
	/// <returns>The incremented value</returns>
	public int IncrementTrailerScanEvents()
	{
		return Interlocked.Increment(ref _runHeaderStruct.NumTrailerScanEvents);
	}

	/// <summary>
	/// Re-read the current file, to get the latest data.
	/// Only meaningful if the object has an implied backing file (such as IO.DLL and .raw files)
	/// No-op otherwise
	/// </summary>
	/// <returns>True all in-acquisition data are remapped successfully; false otherwise. </returns>
	public bool RefreshViewOfFile()
	{
		if (Refresh())
		{
			return true;
		}
		bool result = false;
		try
		{
			_acqHeaderViewer = _acqHeaderViewer.GetMemoryMappedViewer(_loaderId, DataFileMapName, inAcquisition: true, DataFileAccessMode.OpenRead);
			result = Refresh();
		}
		catch (Exception)
		{
		}
		return result;
	}

	/// <summary>
	/// Refresh data from existing map
	/// </summary>
	/// <returns>true if OK</returns>
	public bool Refresh()
	{
		if (_acqHeaderViewer != null)
		{
			Load(_acqHeaderViewer, 0L, FileRevision);
			return true;
		}
		return false;
	}

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_acqHeaderViewer.ReleaseAndCloseMemoryMappedFile(Manager, forceToCloseMmf: true);
		}
	}
}
