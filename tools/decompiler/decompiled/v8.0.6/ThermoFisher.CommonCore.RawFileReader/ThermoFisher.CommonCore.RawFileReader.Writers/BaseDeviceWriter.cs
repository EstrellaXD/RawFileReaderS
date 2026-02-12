using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.RunHeaders;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides methods to write common device information<para />
/// 1. Instrument information<para />
/// 2. Instrument expected run time<para />
/// 3. Status log header<para />
/// 4. Status log<para />
/// 5. Error log
/// </summary>
internal class BaseDeviceWriter : IBinaryBaseDataWriter
{
	private const string ErrorMsgDeviceNotReadyOrHeaderDefSaved = "Either the device is not ready for acquiring data or the header definition has already been created.";

	protected readonly Guid DeviceId;

	protected readonly DeviceErrors DevErrors;

	protected readonly DeviceAcquireStatus DeviceAcqStatus;

	private readonly BufferInfo _errorLogBufferInfo;

	private readonly BufferInfo _instrumentIdBufferInfo;

	private BufferInfo _statusLogHeaderBufferInfo;

	private BufferInfo _statusLogBufferInfo;

	private Mutex _noNamedStatusLogMutex;

	private Mutex _noNamedErrorLogMutex;

	private bool _readOnly;

	private int _registeredDeviceIndex;

	private bool _disposed;

	private int _writtenInstId;

	private int _writtenExpectedRunTime;

	private int _writtenRunHeaderComments;

	/// <summary>
	/// Generic type headers.
	/// </summary>
	private DataDescriptors _statusLogHeader;

	private string _rawFileInformationMapName;

	private IReadWriteAccessor _rawFileInfoMemMapAccessor;

	private IReadWriteAccessor _runHeaderMemMapAccessor;

	/// <summary>
	/// Gets a value indicating whether this file has detected an error.
	/// If this is false: Other error properties in this interface have no meaning.
	/// </summary>
	public bool HasError => DevErrors.HasError;

	/// <summary>
	/// Gets a value indicating whether this file has detected a warning.
	/// If this is false: Other warning properties in this interface have no meaning.
	/// </summary>
	public bool HasWarning => DevErrors.HasWarning;

	/// <summary>
	/// Gets the error code number.
	/// Typically this is a windows system error number.
	/// </summary>
	public int ErrorCode => DevErrors.ErrorCode;

	/// <summary>
	/// Gets the error message.
	/// </summary>
	public string ErrorMessage => DevErrors.ErrorMessage;

	/// <summary>
	/// Gets the warning message.
	/// </summary>
	public string WarningMessage => DevErrors.WarningMessage;

	/// <summary>
	/// Gets the file revision.
	/// </summary>
	/// <value>
	/// The file revision.
	/// </value>
	private int FileRevision { get; }

	/// <summary>
	/// Gets the device type.
	/// </summary>
	protected Device DeviceType { get; }

	/// <summary>
	/// Gets the data stream writers.
	/// Used for writing device data, each stream responsible for different data set,
	/// i.e. instrument ID, status log, error log, etc.
	/// </summary>
	/// <value>
	/// The stream writers.
	/// </value>
	protected BinaryWriter[] DataStreamWriters { get; private set; }

	/// <summary>
	/// Gets the device run header.
	/// </summary>
	/// <value>
	/// The device run header.
	/// </value>
	protected ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.RunHeader DeviceRunHeader { get; private set; }

	/// <summary>
	/// Gets the data file map name.
	/// </summary>
	protected string DataFileMapName { get; }

	/// <summary>
	/// Gets a value indicating whether [in creation].
	/// </summary>
	/// <value>
	///   <c>true</c> if [in creation]; otherwise, <c>false</c>.
	/// </value>
	protected bool InCreation { get; private set; }

	/// <summary>
	/// Gets the run header memory map accessor.
	/// </summary>
	/// <value>
	/// The run header memory map accessor.
	/// </value>
	protected IReadWriteAccessor RunHeaderMemMapAccessor => _runHeaderMemMapAccessor;

	/// <summary>
	/// Gets or sets the written generic headers flags.
	/// Flag so we know if a generic header has been written
	/// There are 3 types of generic header - Status log, Trailer extra and Tune
	/// </summary>
	protected int[] WrittenGenericHeadersFlags { private get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.BaseDeviceWriter" /> class.
	/// </summary>
	/// <param name="deviceType">Type of the device.</param>
	/// <param name="deviceIndex">Index of the device.</param>
	/// <param name="rawFileName">Name of the raw file.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="inAcquisition">if set to <c>true</c> [in acquisition].</param>
	/// <param name="runHeaderRevision">Revision code for the device (set in run header). Default 1</param>
	/// <param name="domain">Which data system this device come from (Xcalibur, Chromeleon etc)</param>
	protected BaseDeviceWriter(Device deviceType, int deviceIndex, string rawFileName, int fileRevision, bool inAcquisition, short runHeaderRevision = 1, RawDataDomain domain = RawDataDomain.MassSpectrometry)
	{
		string text = rawFileName.ToLowerInvariant();
		DeviceType = deviceType;
		VirtualControllerInfo virtualControllerInfo = new VirtualControllerInfo(new VirtualControllerInfoStruct
		{
			Offset = 0L,
			VirtualDeviceIndex = deviceIndex,
			VirtualDeviceType = deviceType.ToVirtualDeviceType()
		});
		DevErrors = new DeviceErrors();
		DeviceId = Guid.NewGuid();
		if (InitializeDeviceIndex(DeviceId, virtualControllerInfo.VirtualDeviceIndex, text, fileRevision, inAcquisition))
		{
			DeviceAcqStatus = new DeviceAcquireStatus();
			WrittenGenericHeadersFlags = new int[3] { 0, 1, 1 };
			FileRevision = fileRevision;
			DataFileMapName = Utilities.BuildUniqueVirtualDeviceFileMapName(virtualControllerInfo.VirtualDeviceType, _registeredDeviceIndex, Utilities.MapName(text, string.Empty));
			if (inAcquisition && InCreation && !HasError && InitializeRunHeader(virtualControllerInfo, _registeredDeviceIndex, runHeaderRevision, domain, rawFileName) && !RawFileInfoExtension.AddControllerInfo(DeviceId, DeviceRunHeader.DeviceType, DeviceRunHeader.DeviceIndex, _rawFileInformationMapName, DevErrors))
			{
				return;
			}
		}
		_instrumentIdBufferInfo = new BufferInfo(DeviceId, DataFileMapName, "INSTID", creatable: true).UpdateErrors(DevErrors);
		_statusLogHeaderBufferInfo = new BufferInfo(DeviceId, DataFileMapName, "STATUSLOGHEADER", creatable: true).UpdateErrors(DevErrors);
		_statusLogBufferInfo = new BufferInfo(DeviceId, DataFileMapName, "STATUS_LOG", creatable: true).UpdateErrors(DevErrors);
		_errorLogBufferInfo = new BufferInfo(DeviceId, DataFileMapName, "ERROR_LOG", creatable: true).UpdateErrors(DevErrors);
	}

	/// <summary>
	/// Write the Instrument ID info to the raw data file. The
	/// Instrument ID must be written to the raw file before any data can be
	/// acquired.
	/// </summary>
	/// <param name="instId">The instrument identifier.</param>
	/// <returns>True written instrument information to disk file; otherwise False.</returns>
	public bool WriteInstrumentInfo(IInstrumentDataAccess instId)
	{
		bool result = false;
		if (!HasError)
		{
			if (DeviceAcqStatus.IsDeviceSetup && Interlocked.CompareExchange(ref _writtenInstId, 1, 0) == 0)
			{
				if (instId.Save(DataStreamWriters[0], DevErrors))
				{
					_instrumentIdBufferInfo.IncrementNumElements();
					UpdateReady();
					result = true;
				}
			}
			else
			{
				DevErrors.UpdateError("Either the device writer is not ready or the instrument data has already been written to the file.");
			}
		}
		return result;
	}

	/// <summary>
	/// Write the Instrument ID info to the raw data file. The
	/// Instrument ID must be written to the raw file before any data can be
	/// acquired.
	/// </summary>
	/// <param name="instId">The instrument identifier.</param>
	/// <returns>True written instrument information to disk file; otherwise False.</returns>
	public bool WriteInstrumentInfo(byte[] instId)
	{
		bool result = false;
		if (!HasError)
		{
			if (DeviceAcqStatus.IsDeviceSetup && Interlocked.CompareExchange(ref _writtenInstId, 1, 0) == 0)
			{
				if (instId.Save(DataStreamWriters[0], DevErrors))
				{
					_instrumentIdBufferInfo.IncrementNumElements();
					UpdateReady();
					result = true;
				}
			}
			else
			{
				DevErrors.UpdateError("Either the device writer is not ready or the instrument data has already been written to the file.");
			}
		}
		return result;
	}

	/// <summary>
	/// Wrapper to write the expected run time. All devices MUST do this so
	/// that the real-time update can display a sensible Axis.
	/// </summary>
	/// <param name="runTime">The expected Run Time.</param>
	/// <returns> True if data valid. </returns>
	public bool WriteInstExpectedRunTime(double runTime)
	{
		bool result = false;
		if (!HasError)
		{
			if (DeviceAcqStatus.IsDeviceSetup && Interlocked.CompareExchange(ref _writtenExpectedRunTime, 1, 0) == 0)
			{
				if (DeviceRunHeader.SaveExpectedRunime(_runHeaderMemMapAccessor, runTime, DevErrors))
				{
					UpdateReady();
					result = true;
				}
			}
			else
			{
				DevErrors.UpdateError("Either the device writer is not ready or the expected run time has already been written to the file.");
			}
		}
		return result;
	}

	/// <summary>
	/// update to ready state, if all prerequisites have been met
	/// </summary>
	private void UpdateReady()
	{
		if (_writtenExpectedRunTime > 0 && _writtenInstId > 0 && WrittenGenericHeadersFlags.IsWrittenGenericHeaderDone())
		{
			DeviceAcqStatus.DeviceStatus = VirtualDeviceAcquireStatus.DeviceStatusReady;
		}
	}

	/// <summary>
	/// Writes the instrument comments.<para />
	/// These are device run header fields - comment1 and comment2.  They are part of the Chromatogram view title (Sample Name and Comment).<para />
	/// These fields can be set only once. 
	/// </summary>
	/// <param name="comment1">The comment1 for "Sample Name" in Chromatogram view title (max 39 chars).</param>
	/// <param name="comment2">The comment2 for "Comment" in Chromatogram view title (max 63 chars).</param>
	/// <returns>True if comment1 and comment2 are written to disk successfully, false otherwise.</returns>
	public bool WriteInstComments(string comment1, string comment2)
	{
		bool result = false;
		if (!HasError)
		{
			if (DeviceAcqStatus.CanDeviceBeSetup && Interlocked.CompareExchange(ref _writtenRunHeaderComments, 1, 0) == 0)
			{
				result = DeviceRunHeader.SaveComment1AndComment2(_runHeaderMemMapAccessor, comment1 ?? string.Empty, comment2 ?? string.Empty, DevErrors);
			}
			else
			{
				DevErrors.UpdateError("Either the device writer is not ready or the expected run time has already been written to the file.");
			}
		}
		return result;
	}

	/// <summary>
	/// Writes the status log header.
	/// </summary>
	/// <param name="headerItems">The header items.</param>
	/// <returns>true written status log header to the disk file; otherwise False.</returns>
	public bool WriteStatusLogHeader(IHeaderItem[] headerItems)
	{
		if (!HasError)
		{
			return WriteGenericHeader(headerItems, GenericHeaderWrittenFlag.WrittenStatusLogHeader, DataStreamType.StatusLogHeaderFile, ref _statusLogHeaderBufferInfo, ref _statusLogBufferInfo, out _statusLogHeader);
		}
		return false;
	}

	/// <summary>
	/// If any Status Log details are to be written to the raw data file
	/// then the format this data will take must be written to the file while
	/// setting up i.e. prior to acquiring any data.
	/// </summary>
	/// <param name="retentionTime">The retention time.</param>
	/// <param name="data">The status data.</param>
	/// <returns>true written status log entry to the disk file; otherwise false.</returns>
	public bool WriteStatusLog(float retentionTime, byte[] data)
	{
		bool result = false;
		if (!HasError)
		{
			if (DeviceAcqStatus.CanDeviceAcquireData)
			{
				try
				{
					byte[] array = new byte[data.Length];
					Buffer.BlockCopy(data, 0, array, 0, data.Length);
					bool num = WriteStatusLog(DataStreamWriters[2], retentionTime, array);
					if (num && data.IsAny())
					{
						_statusLogBufferInfo.IncrementNumElements();
					}
					result = num;
				}
				catch (Exception ex)
				{
					DevErrors.UpdateError(ex);
				}
			}
			else
			{
				DevErrors.UpdateError("Either the device writer is not ready for acquiring data or the header must be written to the file before data entry can be written");
			}
		}
		return result;
	}

	/// <summary>
	/// If any Status Log details are to be written to the raw data file
	/// then the format this data will take must be written to the file while
	/// setting up i.e. prior to acquiring any data.
	/// </summary>
	/// <param name="retentionTime">The retention time.</param>
	/// <param name="data">The data.</param>
	/// <returns>true written status log entry to the disk file; otherwise false.</returns>
	public bool WriteStatusLog(float retentionTime, object[] data)
	{
		if (!CannotAcquire("Either the device is not ready for acquiring data or the header definition has already been created."))
		{
			try
			{
				_statusLogHeader.ConvertDataEntryToByteArray(data, out var buffer);
				bool num = WriteStatusLog(DataStreamWriters[2], retentionTime, buffer);
				if (num && data.IsAny())
				{
					_statusLogBufferInfo.IncrementNumElements();
				}
				return num;
			}
			catch (Exception ex)
			{
				DevErrors.UpdateError(ex);
			}
		}
		return false;
	}

	/// <summary>
	/// Write an error log to the raw data file.
	/// </summary>
	/// <param name="retentionTime">The retention time.</param>
	/// <param name="packedLogMessage">The error message, packed as a byte array.</param>
	/// <returns>True error log write to the disk file, otherwise False.</returns>
	public bool WriteBinaryErrorLog(float retentionTime, byte[] packedLogMessage)
	{
		bool result = false;
		if (!HasError)
		{
			if (DeviceAcqStatus.CanDeviceAcquireData)
			{
				result = WriterHelper.CritSec(delegate(DeviceErrors err)
				{
					bool flag = DataStreamWriters[3].SaveErrorLogItem(retentionTime, packedLogMessage, err);
					if (flag)
					{
						flag = RunHeaderExtension.SaveNumErrorLog(_runHeaderMemMapAccessor, DeviceRunHeader.IncrementNumErrorLog(), err);
						if (flag)
						{
							_errorLogBufferInfo.IncrementNumElements();
						}
					}
					return flag;
				}, DevErrors, _noNamedErrorLogMutex);
			}
			else
			{
				DevErrors.UpdateError("Device is not ready for acquiring data.");
			}
		}
		return result;
	}

	/// <summary>
	/// Write an error log to the raw data file.
	/// </summary>
	/// <param name="retentionTime">The retention time.</param>
	/// <param name="errorLog">The error log.</param>
	/// <returns>True error log write to the disk file, otherwise False.</returns>
	public bool WriteErrorLog(float retentionTime, string errorLog)
	{
		bool result = false;
		if (!HasError)
		{
			if (DeviceAcqStatus.CanDeviceAcquireData)
			{
				result = WriterHelper.CritSec(delegate(DeviceErrors err)
				{
					bool flag = DataStreamWriters[3].SaveErrorLogItem(retentionTime, errorLog, err);
					if (flag)
					{
						flag = RunHeaderExtension.SaveNumErrorLog(_runHeaderMemMapAccessor, DeviceRunHeader.IncrementNumErrorLog(), err);
						if (flag)
						{
							_errorLogBufferInfo.IncrementNumElements();
						}
					}
					return flag;
				}, DevErrors, _noNamedErrorLogMutex);
			}
			else
			{
				DevErrors.UpdateError("Device is not ready for acquiring data.");
			}
		}
		return result;
	}

	/// <summary>
	/// Writes the generic header to a file.
	/// </summary>
	/// <param name="headerItems">The header items.</param>
	/// <param name="writtenFlag">The written flag, i.e. status log header.</param>
	/// <param name="writerType">Binary writer id</param>
	/// <param name="headerBufferInfo">The shared memory for header buffer information.</param>
	/// <param name="dataBufferInfo">The shared memory for data buffer information.</param>
	/// <param name="headerDescriptors">return a header descriptors object.</param>
	/// <returns>True if the generic header is written to the file; false otherwise.</returns>
	protected bool WriteGenericHeader(IHeaderItem[] headerItems, GenericHeaderWrittenFlag writtenFlag, DataStreamType writerType, ref BufferInfo headerBufferInfo, ref BufferInfo dataBufferInfo, out DataDescriptors headerDescriptors)
	{
		DataDescriptors dataDescriptors = (headerDescriptors = new DataDescriptors(0));
		bool flag = false;
		if (!HasError)
		{
			if (DeviceAcqStatus.CanDeviceBeSetup && Interlocked.Exchange(ref WrittenGenericHeadersFlags[(int)writtenFlag], 1) == 0)
			{
				int num = ((headerItems != null) ? headerItems.Length : 0);
				try
				{
					headerItems.ConvertHeaderItemsToGenericHeader(out dataDescriptors);
					flag = dataDescriptors.SaveGenericHeader(DataStreamWriters[(int)writerType], num, DevErrors);
				}
				catch (Exception ex)
				{
					DevErrors.UpdateError(ex);
				}
				if (flag)
				{
					headerDescriptors = dataDescriptors;
					if (num > 0)
					{
						headerBufferInfo.IncrementNumElements();
						if (writtenFlag == GenericHeaderWrittenFlag.WrittenStatusLogHeader)
						{
							dataBufferInfo.SetDataBlockSize(dataDescriptors.CalcBufferSize() + 4);
						}
						else
						{
							dataBufferInfo.SetDataBlockSize(dataDescriptors.CalcBufferSize());
						}
					}
					UpdateReady();
				}
			}
			else
			{
				DevErrors.UpdateError("Either the device is not ready for acquiring data or the header definition has already been created.");
			}
		}
		return flag;
	}

	/// <summary>
	/// if the file is in error or not ready to acquire yet, fail.
	/// </summary>
	/// <param name="message">The error message. </param>
	/// <returns>True if it cannot be acquired due to an error.</returns>
	protected bool CannotAcquire(string message)
	{
		bool result = true;
		if (!HasError)
		{
			if (DeviceAcqStatus.CanDeviceAcquireData)
			{
				result = false;
			}
			else
			{
				DevErrors.UpdateError(message);
			}
		}
		return result;
	}

	/// <summary>
	/// Initializes the index of the device.
	/// </summary>
	/// <param name="loaderId">The loader identifier.</param>
	/// <param name="registeredIndex">Index of the registered.</param>
	/// <param name="rawFileName">Name of the raw file.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="inAcquisition">if set to <c>true</c> [in acquisition].</param>
	/// <returns>True obtained a validate control index; otherwise False.</returns>
	private bool InitializeDeviceIndex(Guid loaderId, int registeredIndex, string rawFileName, int fileRevision, bool inAcquisition)
	{
		_noNamedErrorLogMutex = Utilities.CreateNoNameMutex();
		_noNamedStatusLogMutex = Utilities.CreateNoNameMutex();
		InCreation = registeredIndex == -1;
		_readOnly = !InCreation;
		if (_readOnly)
		{
			_readOnly = !inAcquisition;
		}
		using (RawFileInfo rawFileInfo = new RawFileInfo(MemoryMappedRawFileManager.Instance, loaderId, rawFileName, fileRevision, inAcquisition))
		{
			_rawFileInformationMapName = rawFileInfo.DataFileMapName;
			_rawFileInfoMemMapAccessor = SharedMemHelper.CreateSharedBufferAccessor(DeviceId, rawFileInfo.DataFileMapName, (!Utilities.IsRunningUnderLinux.Value) ? Utilities.StructSizeLookup.Value[1] : 0, creatable: true, DevErrors);
		}
		bool result = false;
		if (_rawFileInfoMemMapAccessor != null)
		{
			_registeredDeviceIndex = (InCreation ? RawFileInfoExtension.GetAvailableControllerIndex(DeviceId, _rawFileInformationMapName, DevErrors) : registeredIndex);
			if (_registeredDeviceIndex >= 0)
			{
				result = true;
			}
		}
		return result;
	}

	/// <summary>
	/// Initializes the run header.
	/// </summary>
	/// <param name="deviceInfo">The device information.</param>
	/// <param name="registeredDeviceIndex">Index of the registered device.</param>
	/// <param name="revision">Revision code</param>
	/// <param name="domain">which data system this devyuec belongs to (Xcalibur, Chromeleon etc.)</param>
	/// <param name="rawFileName">Name of the raw file.</param>
	/// <returns>true if OK</returns>
	private bool InitializeRunHeader(VirtualControllerInfo deviceInfo, int registeredDeviceIndex, short revision, RawDataDomain domain, string rawFileName)
	{
		try
		{
			DeviceRunHeader = new ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.RunHeader(MemoryMappedRawFileManager.Instance, DeviceId, DataFileMapName, FileRevision, InCreation, _readOnly)
			{
				DeviceType = deviceInfo.VirtualDeviceType,
				DeviceIndex = registeredDeviceIndex
			};
			_runHeaderMemMapAccessor = SharedMemHelper.CreateSharedBufferAccessor(DeviceId, DeviceRunHeader.DataFileMapName, Utilities.StructSizeLookup.Value[2], creatable: true, DevErrors);
			string text = Utilities.BuildUniqueVirtualDeviceStreamTempFileName(deviceInfo.VirtualDeviceType, registeredDeviceIndex);
			if (_runHeaderMemMapAccessor != null)
			{
				DeviceRunHeader.Initialize(revision, domain);
				RunHeaderStruct runHeaderStruct = DeviceRunHeader.RunHeaderStruct;
				DataStreamWriters = new BinaryWriter[12];
				if (TempFileHelper.CreateTempFile(rawFileName, out DataStreamWriters[0], out runHeaderStruct.InstIDFile, text + "INSTID") && TempFileHelper.CreateTempFile(rawFileName, out DataStreamWriters[1], out runHeaderStruct.StatusLogHeaderFile, text + "STATUSLOGHEADER") && TempFileHelper.CreateTempFile(rawFileName, out DataStreamWriters[2], out runHeaderStruct.StatusLogFile, text + "STATUS_LOG", addZeroValueLengthField: true, resetStreamPosition: true) && TempFileHelper.CreateTempFile(rawFileName, out DataStreamWriters[3], out runHeaderStruct.ErrorLogFile, text + "ERROR_LOG", addZeroValueLengthField: true))
				{
					DeviceRunHeader.CopyRunHeaderStruct(ref runHeaderStruct);
					return true;
				}
			}
		}
		catch (Exception ex)
		{
			return DevErrors.UpdateError(ex);
		}
		return false;
	}

	/// <summary>
	/// This method will use the binary stream object to writes the status log entry to the file.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="retentionTime">The retention time.</param>
	/// <param name="data">The data.</param>
	/// <returns>True the data write to the disk file; otherwise False.</returns>
	private bool WriteStatusLog(BinaryWriter writer, float retentionTime, byte[] data)
	{
		bool result = false;
		if (_statusLogHeader.TotalDataSize == data.Length)
		{
			result = data.Length == 0 || WriterHelper.CritSec((DeviceErrors err) => writer.SaveGenericDataItem(retentionTime, data, err) && RunHeaderExtension.SaveNumStatusLog(_runHeaderMemMapAccessor, DeviceRunHeader.IncrementNumStatusLog(), err), DevErrors, _noNamedStatusLogMutex);
		}
		else
		{
			DevErrors.UpdateError($"Header size:{_statusLogHeader.TotalDataSize} and input data size:{data.Length} not matching.");
		}
		return result;
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public virtual void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		if (_runHeaderMemMapAccessor != null)
		{
			RunHeaderExtension.SaveIsInAcquisition(_runHeaderMemMapAccessor, isInAcq: false, DevErrors);
		}
		DevErrors.UpdateError(DevErrors.ErrorMessage + Environment.NewLine + "Device writer has been disposed");
		Utilities.ReleaseMutex(ref _noNamedErrorLogMutex);
		Utilities.ReleaseMutex(ref _noNamedStatusLogMutex);
		if (DataStreamWriters.IsAny())
		{
			Parallel.ForEach(DataStreamWriters, delegate(BinaryWriter streamWriter)
			{
				if (streamWriter != null)
				{
					streamWriter.Flush();
					streamWriter.Dispose();
				}
			});
		}
		IViewCollectionManager instance = MemoryMappedRawFileManager.Instance;
		_runHeaderMemMapAccessor?.ReleaseAndCloseMemoryMappedFile(instance);
		_rawFileInfoMemMapAccessor?.ReleaseAndCloseMemoryMappedFile(instance);
		string tempFileName = string.Empty;
		string tempFileName2 = string.Empty;
		string tempFileName3 = string.Empty;
		string tempFileName4 = string.Empty;
		if (DeviceRunHeader != null)
		{
			tempFileName = DeviceRunHeader.InstIdFilename;
			tempFileName2 = DeviceRunHeader.StatusLogHeaderFilename;
			tempFileName3 = DeviceRunHeader.StatusLogFilename;
			tempFileName4 = DeviceRunHeader.ErrorLogFilename;
			DeviceRunHeader.Dispose();
		}
		_instrumentIdBufferInfo?.DeleteTempFileOnlyIfNoReference(tempFileName).DisposeBufferInfo();
		_statusLogHeaderBufferInfo?.DeleteTempFileOnlyIfNoReference(tempFileName2).DisposeBufferInfo();
		_statusLogBufferInfo?.DeleteTempFileOnlyIfNoReference(tempFileName3).DisposeBufferInfo();
		_errorLogBufferInfo?.DeleteTempFileOnlyIfNoReference(tempFileName4).DisposeBufferInfo();
	}
}
