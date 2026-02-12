using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

/// <summary>
/// The device base class for all type of devices - UV, PDA, MS, Analog, and Status.
/// </summary>
internal abstract class DeviceBase : IDevice, IRealTimeAccess, IDisposable, IRawObjectBase
{
	private readonly VirtualControllerInfo _deviceInfo;

	protected readonly DeviceErrors DevErrors;

	private bool _disposed;

	private BufferInfo _instrumentIdBufferInfo;

	private BufferInfo _statusLogHeaderBufferInfo;

	private BufferInfo _statusLogBufferInfo;

	private BufferInfo _errorLogBufferInfo;

	private bool _isInAcquisition;

	private bool _oldRev;

	private string _rawFileName;

	private int _fileRevision;

	public IViewCollectionManager Manager { get; }

	/// <summary>
	/// Gets or sets a value indicating whether this was initialized when the file was in acquisition.
	/// </summary>
	public bool InAcquisition { get; set; }

	/// <summary>
	/// Gets or sets the absolute position of the end of this device data.
	/// </summary>
	public virtual long OffsetOfEndOfDevice { get; internal set; }

	/// <summary>
	/// Gets the device type.
	/// </summary>
	public VirtualDeviceTypes DeviceType => _deviceInfo.VirtualDeviceType;

	/// <summary>
	/// Gets the error log entries.
	/// </summary>
	public IErrorLog ErrorLogEntries { get; private set; }

	/// <summary>
	///     Gets the instrument id.
	/// </summary>
	public IInstrumentId InstrumentId { get; private set; }

	/// <summary>
	/// Gets or sets the run header.
	/// </summary>
	public IRunHeader RunHeader { get; internal set; }

	/// <summary>
	/// Gets or sets the status log entries.
	/// </summary>
	public IStatusLog StatusLogEntries { get; internal set; }

	/// <summary>
	/// Gets the file revision.
	/// </summary>
	public int FileRevision { get; }

	/// <summary>
	/// Gets the header file map name.
	/// </summary>
	public string HeaderFileMapName { get; }

	/// <summary>
	/// Gets the data file map name.
	/// </summary>
	public string DataFileMapName { get; }

	/// <summary>
	/// Gets the raw file information.
	/// </summary>
	protected IRawFileInfo RawFileInformation { get; private set; }

	/// <summary>
	/// Gets the loader id.
	/// </summary>
	protected Guid LoaderId { get; }

	/// <summary>
	/// Gets the raw data viewer.
	/// </summary>
	protected IMemoryReader RawDataViewer { get; private set; }

	/// <summary>
	/// Gets the offset of the end of device common info.
	/// </summary>
	protected long OffsetOfTheEndOfDeviceCommonInfo { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.DeviceBase" /> class.
	/// </summary>
	/// <param name="deviceType">Type of the device.</param>
	/// <param name="registeredIndex">Index of the registered.</param>
	/// <param name="rawFileName">Name of the raw file.</param>
	/// <param name="fileRevision">The file revision.</param>
	private DeviceBase(VirtualDeviceTypes deviceType, int registeredIndex, string rawFileName, int fileRevision)
	{
		FileRevision = fileRevision;
		HeaderFileMapName = string.Empty;
		DevErrors = new DeviceErrors();
		DataFileMapName = Utilities.BuildUniqueVirtualDeviceFileMapName(deviceType, registeredIndex, Utilities.MapName(rawFileName, string.Empty));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.DeviceBase" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">raw file loader ID</param>
	/// <param name="deviceInfo">
	/// The device info.
	/// </param>
	/// <param name="rawFileName">
	/// The main MMF viewer.
	/// </param>
	/// <param name="fileRevision">
	/// The file version.
	/// </param>
	/// <param name="isInAcquisition">Flag indicates that it's in acquisition or not</param>
	/// <param name="oldRev">Flag indicates that the reading data is old LCQ data or not</param>
	protected DeviceBase(IViewCollectionManager manager, Guid loaderId, VirtualControllerInfo deviceInfo, string rawFileName, int fileRevision, bool isInAcquisition, bool oldRev)
		: this(deviceInfo.VirtualDeviceType, deviceInfo.VirtualDeviceIndex, rawFileName, fileRevision)
	{
		Manager = manager;
		_deviceInfo = deviceInfo;
		LoaderId = loaderId;
		_isInAcquisition = isInAcquisition;
		_oldRev = oldRev;
		_rawFileName = rawFileName;
		_fileRevision = fileRevision;
	}

	/// <summary>
	/// This must be called at the start of any derived class initlaize sequence.
	/// Minimial construction work is done before this initializrion, incase the device never used by this app instance.
	/// </summary>
	protected void BaseDataInitialization()
	{
		if (_isInAcquisition)
		{
			InAcquisitionInitializer(_rawFileName, _fileRevision);
		}
		else if (_oldRev)
		{
			OldLcqXcalFileInitializer();
		}
		else
		{
			NonAcquisitionInitializer(_deviceInfo, _rawFileName, _fileRevision);
		}
	}

	/// <summary>
	/// The method gets the scan index for the spectrum.
	/// </summary>
	/// <param name="spectrum">The spectrum. </param>
	/// <returns>Scan index</returns>
	public abstract IScanIndex GetScanIndex(int spectrum);

	/// <summary>
	/// The method gets the retention time for the scan number.
	/// </summary>
	/// <param name="spectrum">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The retention time for the scan number.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// If the scan number is not in range.
	/// </exception>
	public abstract double GetRetentionTime(int spectrum);

	/// <summary>
	/// Initalize device after construction, for use by Lazy pattern.
	/// Must be implemented in derived devices.
	/// </summary>
	/// <returns>The initialized device</returns>
	public abstract IDevice Initialize();

	/// <summary>
	/// Gets the segment peaks.
	/// </summary>
	/// <param name="scanNum">
	/// The scan number.
	/// </param>
	/// <param name="numSegments">
	/// The number segments.
	/// </param>
	/// <param name="numAllPeaks">
	/// The number all peaks.
	/// </param>
	/// <param name="packet">
	/// The packet.
	/// </param>
	/// <param name="includeReferenceAndExceptionData">
	/// Flag to indicate the returning peak data should include reference and exception data or not.
	/// </param>
	/// <returns>
	/// Segment data
	/// </returns>
	public IReadOnlyList<SegmentData> GetSegmentPeaks(int scanNum, out int numSegments, out int numAllPeaks, out IPacket packet, bool includeReferenceAndExceptionData)
	{
		numAllPeaks = 0;
		numSegments = 0;
		packet = GetPacket(scanNum, includeReferenceAndExceptionData);
		if (packet == null)
		{
			return new List<SegmentData>();
		}
		List<SegmentData> segmentPeaks = packet.SegmentPeaks;
		numSegments = segmentPeaks.Count;
		for (int i = 0; i < numSegments; i++)
		{
			numAllPeaks += segmentPeaks[i].DataPeaks.Count;
		}
		return segmentPeaks;
	}

	/// <summary>
	/// Gets the packet.
	/// </summary>
	/// <param name="scanNumber">The scan number.</param>
	/// <param name="includeReferenceAndExceptionData">Flag to indicate the returning peak data should include reference and exception data or not.</param>
	/// <param name="channelNumber">For UV device only, negative one (-1) for getting all the channel data by the given scan number</param>
	/// <param name="packetScanDataFeatures">True if noise and baseline data is required</param>
	/// <returns>Return the peak data packet</returns>
	public abstract IPacket GetPacket(int scanNumber, bool includeReferenceAndExceptionData, int channelNumber = -1, PacketFeatures packetScanDataFeatures = PacketFeatures.All);

	/// <summary>
	/// The method loads the common device data structures from MMF view stream.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <returns>The number of read bytes</returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long num = dataOffset;
		num = (viewer.PreferLargeReads ? Utilities.LoadDataFromInternalMemoryArrayReader(GetMetaData, viewer, num, 1048576) : GetMetaData(viewer, num));
		StatusLogEntries = viewer.LoadRawFileObjectExt(() => new StatusLog(Manager, LoaderId, RunHeader), fileRevision, ref num);
		ErrorLogEntries = viewer.LoadRawFileObjectExt(() => new ErrorLog(Manager, LoaderId, RunHeader), fileRevision, ref num);
		return num - dataOffset;
		long GetMetaData(IMemoryReader reader, long offset)
		{
			RunHeader = reader.LoadRawFileObjectExt(() => new RunHeader(Manager, LoaderId), fileRevision, ref offset);
			InstrumentId = reader.LoadRawFileObjectExt(() => new InstrumentId(Manager, LoaderId), fileRevision, ref offset);
			return offset;
		}
	}

	/// <summary>
	/// Initialize the internal fields in acquisition mode.
	/// </summary>
	/// <param name="rawFileName">Name of the raw file.</param>
	/// <param name="fileRevision">The file revision.</param>
	private void InAcquisitionInitializer(string rawFileName, int fileRevision)
	{
		InAcquisition = true;
		RawFileInformation = new RawFileInfo(Manager, LoaderId, rawFileName, fileRevision, inAcquisition: true);
		RunHeader = new RunHeader(Manager, LoaderId, DataFileMapName, fileRevision);
		InstrumentId = new InstrumentId(Manager, LoaderId, RunHeader.InstIdFilename, fileRevision);
		StatusLogEntries = new StatusLog(Manager, LoaderId, fileRevision, RunHeader);
		ErrorLogEntries = new ErrorLog(Manager, LoaderId, fileRevision, RunHeader);
		InitializeBufferInfo();
	}

	/// <summary>
	/// Initials the non acquisition.
	/// </summary>
	/// <param name="deviceInfo">The device information.</param>
	/// <param name="rawFileName">Name of the raw file.</param>
	/// <param name="fileRevision">The file revision.</param>
	private void NonAcquisitionInitializer(VirtualControllerInfo deviceInfo, string rawFileName, int fileRevision)
	{
		long num = 0L;
		RawDataViewer = Manager.GetRandomAccessViewer(LoaderId, rawFileName, deviceInfo.Offset, 0L, inAcquisition: false);
		num += Load(RawDataViewer, num, fileRevision);
		OffsetOfTheEndOfDeviceCommonInfo = num;
		if (RunHeader.IsInAcquisition)
		{
			((RunHeader)RunHeader).IsInAcquisition = false;
		}
	}

	/// <summary>
	/// Initials the old LCQ XCALIBUR file.
	/// </summary>
	private void OldLcqXcalFileInitializer()
	{
		RunHeader = new RunHeader(MemoryMappedRawFileManager.Instance, LoaderId);
		InstrumentId = new InstrumentId(Manager, LoaderId);
		StatusLogEntries = new StatusLog(MemoryMappedRawFileManager.Instance, LoaderId);
		ErrorLogEntries = new ErrorLog(Manager, LoaderId);
	}

	/// <summary>
	/// Initializes the buffer information.
	/// </summary>
	private void InitializeBufferInfo()
	{
		_instrumentIdBufferInfo = new BufferInfo(LoaderId, DataFileMapName, "INSTID", creatable: false).UpdateErrors(DevErrors);
		_statusLogHeaderBufferInfo = new BufferInfo(LoaderId, DataFileMapName, "STATUSLOGHEADER", creatable: false).UpdateErrors(DevErrors);
		_statusLogBufferInfo = new BufferInfo(LoaderId, DataFileMapName, "STATUS_LOG", creatable: false).UpdateErrors(DevErrors);
		_errorLogBufferInfo = new BufferInfo(LoaderId, DataFileMapName, "ERROR_LOG", creatable: false).UpdateErrors(DevErrors);
	}

	/// <summary>
	/// Disposes the buffer information and temporary file.
	/// </summary>
	private void DisposeBufferInfoAndTempFile()
	{
		bool deletePermitted = !InAcquisition;
		_instrumentIdBufferInfo.DeleteTempFileOnlyIfNoReference(RunHeader.InstIdFilename, deletePermitted).DisposeBufferInfo();
		_statusLogHeaderBufferInfo.DeleteTempFileOnlyIfNoReference(RunHeader.StatusLogHeaderFilename, deletePermitted).DisposeBufferInfo();
		_statusLogBufferInfo.DeleteTempFileOnlyIfNoReference(RunHeader.StatusLogFilename, deletePermitted).DisposeBufferInfo();
		_errorLogBufferInfo.DeleteTempFileOnlyIfNoReference(RunHeader.ErrorLogFilename, deletePermitted).DisposeBufferInfo();
	}

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public virtual void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			if (RawDataViewer is IDisposableReader viewer)
			{
				viewer.ReleaseAndCloseMemoryMappedFile(Manager);
			}
			InstrumentId?.Dispose();
			StatusLogEntries?.Dispose();
			ErrorLogEntries?.Dispose();
			if (RunHeader != null)
			{
				RunHeader.Dispose();
				DisposeBufferInfoAndTempFile();
			}
			RawFileInformation?.Dispose();
		}
	}

	/// <summary>
	/// Re-read the current file, to get the latest data.
	/// Only meaningful if the object has an implied backing file (such as IO.DLL and .raw files)
	/// No-op otherwise
	/// </summary>
	/// <returns>True refresh succeed, false otherwise. </returns>
	public virtual bool RefreshViewOfFile()
	{
		try
		{
			if (RunHeader.IsInAcquisition && RunHeader.RefreshViewOfFile() && InstrumentId.RefreshViewOfFile() && StatusLogEntries.RefreshViewOfFile() && ErrorLogEntries.RefreshViewOfFile())
			{
				return true;
			}
		}
		catch (Exception)
		{
		}
		return false;
	}
}
