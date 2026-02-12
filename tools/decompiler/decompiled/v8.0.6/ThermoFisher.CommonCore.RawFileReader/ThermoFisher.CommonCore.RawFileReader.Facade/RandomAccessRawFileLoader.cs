using System;
using System.Collections.Generic;
using System.Threading;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader.Facade;

/// <summary>
/// Loads raw data using random access file reading
/// </summary>
internal class RandomAccessRawFileLoader : LoaderBase, IRawFileLoader, IFileError, IDisposable
{
	private bool _disposedValue;

	private int _useCount;

	private IDisposableReader _randViewer;

	private OldLcqFile _oldLcqFile;

	private Lazy<AuditTrail> _auditTrailLazy;

	/// <summary>
	/// Gets the file revision.
	/// </summary>
	private int FileRevision { get; set; }

	public IViewCollectionManager Manager { get; }

	public Guid Id { get; private set; }

	public string RawFileName { get; private set; }

	public string DataFileMapName { get; }

	public string StreamId { get; }

	public IAutoSamplerConfig AutoSamplerConfig { get; private set; }

	public IMethod MethodInfo { get; private set; }

	public ISequenceRow Sequence { get; set; }

	public IRawFileInfo RawFileInformation { get; set; }

	public bool IsOpen { get; private set; }

	/// <summary>
	/// Gets the audit trail information.
	/// </summary>
	public AuditTrail AuditTrailInfo
	{
		get
		{
			if (_auditTrailLazy != null)
			{
				return _auditTrailLazy.Value;
			}
			return new AuditTrail();
		}
	}

	public DeviceContainer[] Devices { get; private set; }

	/// <summary>
	/// Prevents a default instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.RandomAccessRawFileLoader" /> class from being created.
	/// </summary>
	private RandomAccessRawFileLoader()
	{
		ClearAllErrorsAndWarnings();
		Devices = Array.Empty<DeviceContainer>();
		_oldLcqFile = null;
		Id = Guid.NewGuid();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.RandomAccessRawFileLoader" /> class. 
	/// Default constructor initializes a new instance of RandomAccessRawFileLoader class.
	/// Must be called prior to data access
	/// </summary>
	/// <param name="fileName">
	/// The file path.
	/// </param>
	/// <param name="manager">data reader</param>
	/// <exception cref="T:System.ArgumentException">
	/// The file path is empty or null.
	/// </exception>
	/// <exception cref="T:System.Exception">
	/// A problem encountered when reading the raw file.
	/// </exception>
	internal RandomAccessRawFileLoader(string fileName, IViewCollectionManager manager)
		: this()
	{
		bool isCreateMutexFailed = false;
		Mutex mutex = null;
		try
		{
			AppendInformataion("Creating mutex for: " + fileName);
			mutex = Utilities.CreateNamedMutexAndWait(fileName);
			if (mutex == null)
			{
				AppendWarning("Null mutex for: " + fileName);
				isCreateMutexFailed = true;
			}
			AppendInformataion("Created mutex for: " + fileName);
			if (string.IsNullOrWhiteSpace(fileName))
			{
				throw new ArgumentException(Resources.ErrorEmptyNullFileName);
			}
			RawFileName = fileName;
			DataFileMapName = RawFileName;
			string text = Utilities.CorrectNameForEnvironment(RawFileName, manager.GetIgnorePlatformKeepNameCaseIntactFlag());
			StreamId = StreamHelper.ConstructStreamId(Id, text);
			Manager = manager;
			_randViewer = Manager.GetRandomAccessViewer(Id, text, inAcquisition: false);
			LoadRawFile(_randViewer, isCreateMutexFailed);
		}
		catch (Exception ex)
		{
			if (ex is NewerFileFormatException)
			{
				UpdateError(ex.Message, 5);
			}
			else
			{
				UpdateError($"Encountered problems while trying to read '{RawFileName}' as a Raw File!{Environment.NewLine}{ex.ToMessageAndCompleteStacktrace()}");
			}
		}
		finally
		{
			try
			{
				if (manager != null && _randViewer != null)
				{
					string streamId = _randViewer.StreamId;
					AppendError(manager.GetErrors(streamId));
					IsOpen = manager.IsOpen(streamId);
				}
				else
				{
					IsOpen = false;
				}
			}
			catch
			{
				AppendWarning("Did not complete open: " + fileName);
				IsOpen = false;
			}
			if (mutex != null)
			{
				AppendInformataion("Release mutex for: " + fileName);
				mutex.ReleaseMutex();
				AppendInformataion("Close mutex for: " + fileName);
				mutex.Close();
			}
		}
	}

	/// <summary>
	/// Load the raw file.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="isCreateMutexFailed">Indicate whether the named mutex is successfully acquired or not</param>
	/// <exception cref="T:System.Exception">
	/// Thrown if the file is not a recognized format. The file is either not a THERMO Fisher
	/// Raw file or the version is less than the initial version of XCALIBUR files.
	/// </exception>
	private void LoadRawFile(IMemoryReader viewer, bool isCreateMutexFailed)
	{
		long startPos = 0L;
		if (viewer == null)
		{
			if (!string.IsNullOrWhiteSpace(base.ErrorMessage))
			{
				AppendError(Environment.NewLine);
			}
			AppendError($"{Manager.GetErrors(RawFileName)} {RawFileName}");
			return;
		}
		base.Header = viewer.LoadRawFileObjectExt<ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader>(0, ref startPos);
		FileRevision = CheckForValidVersion("Raw");
		if (IsLcqFormat(viewer, startPos))
		{
			_auditTrailLazy = new Lazy<AuditTrail>(() => _oldLcqFile.AuditTrailInfo);
			return;
		}
		startPos = ReadMetaData(viewer, startPos);
		LoadInstrumentMethod(viewer, ref startPos);
		if (isCreateMutexFailed)
		{
			if (RawFileInformation.IsInAcquisition)
			{
				AppendError("The file was being created by someone (who failed to release a mutex lock for writing the file) and was never completed.");
				return;
			}
			if (!ValidCrc(viewer))
			{
				AppendError("The file has a “Mutex lock”, but it has completed acquisition and has an invalid checksum.");
				return;
			}
			AppendInformataion("The file has a “Mutex lock” on it’s header, but: has completed acquisition and has a valid checksum.");
		}
		InitializeDevices(viewer);
		_auditTrailLazy = new Lazy<AuditTrail>(GetAuditTrailInfo);
	}

	/// <summary>
	/// Test if the file is old LCQ format
	/// </summary>
	/// <param name="viewer">view into file</param>
	/// <param name="startPos">offset into view</param>
	/// <returns>True if this method decoded the data as LCQ format</returns>
	private bool IsLcqFormat(IMemoryReader viewer, long startPos)
	{
		bool num = FileRevision < 25;
		if (num)
		{
			_oldLcqFile = new OldLcqFile(Manager, this);
			_oldLcqFile.DecodeOldLcqFile(viewer, startPos, FileRevision);
		}
		return num;
	}

	/// <summary>
	/// load the instrument method.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="startPos">
	/// The start position in the view.
	/// </param>
	private void LoadInstrumentMethod(IMemoryReader viewer, ref long startPos)
	{
		if (RawFileInformation.HasExpMethod)
		{
			MethodInfo = viewer.LoadRawFileObjectExt<Method>(FileRevision, ref startPos);
		}
	}

	/// <summary>
	/// Initials the device lists.
	/// </summary>
	/// <exception cref="T:System.Exception">Thrown if a device cannot be added to the list</exception>
	private void InitialDeviceLists()
	{
		int count = RawFileInformation.VirtualControllers.Count;
		List<VirtualControllerInfo> virtualControllers = RawFileInformation.VirtualControllers;
		bool isInAcquisition = RawFileInformation.IsInAcquisition;
		Devices = new DeviceContainer[count];
		for (int i = 0; i < count; i++)
		{
			VirtualControllerInfo virtualControllerInfo = virtualControllers[i];
			try
			{
				IDevice device = DeviceFactory.GetDevice(Manager, Id, virtualControllerInfo, Utilities.CorrectNameForEnvironment(RawFileName, Manager.GetIgnorePlatformKeepNameCaseIntactFlag()), base.Header.Revision, isInAcquisition);
				Devices[i] = new DeviceContainer
				{
					PartialDevice = device,
					FullDevice = new Lazy<IDevice>(() => device.Initialize())
				};
			}
			catch (Exception ex)
			{
				if (ex is NewerFileFormatException)
				{
					throw;
				}
				throw new Exception($"Error Encountered while loading {virtualControllerInfo.VirtualDeviceType} at offset {virtualControllerInfo.Offset}.\n{ex.Message}");
			}
		}
	}

	/// <summary>
	/// Read various blocks describing the raw file
	/// </summary>
	/// <param name="viewer">View into file</param>
	/// <param name="startPos">offset into view</param>
	/// <returns>Updated position</returns>
	private long ReadMetaData(IMemoryReader viewer, long startPos)
	{
		startPos = (viewer.PreferLargeReads ? Utilities.LoadDataFromInternalMemoryArrayReader(GetMetaData, viewer, startPos, 1048576) : GetMetaData(viewer, startPos));
		return startPos;
		long GetMetaData(IMemoryReader reader, long offset)
		{
			Sequence = reader.LoadRawFileObjectExt<SequenceRow>(FileRevision, ref offset);
			AutoSamplerConfig = reader.LoadRawFileObjectExt<AutoSamplerConfig>(FileRevision, ref offset);
			RawFileInformation = reader.LoadRawFileObjectExt(() => new RawFileInfo(Manager, Id, DataFileMapName, FileRevision), FileRevision, ref offset);
			return offset;
		}
	}

	/// <summary>
	/// Initialize device data, for "complete file" or "real time"
	/// Reform CRC check on completed files.
	/// </summary>
	/// <param name="viewer">view into file</param>
	private void InitializeDevices(IMemoryReader viewer)
	{
		if (RawFileInformation.IsInAcquisition)
		{
			Dispose();
			throw new NotSupportedException();
		}
		ValidateCrc(viewer);
		InitialDeviceLists();
	}

	/// <summary>
	/// The validate CRC.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <exception cref="T:System.Exception">Thrown if CRC is no valid
	/// </exception>
	private void ValidateCrc(IMemoryReader viewer)
	{
		if (!ValidCrc(viewer))
		{
			AppendError($"CRC failed [file revision #: {FileRevision}]");
			throw new Exception(base.ErrorMessage);
		}
	}

	/// <summary>
	/// Add ref to this loader
	/// </summary>
	/// <returns>number of refs</returns>
	public int AddUse()
	{
		lock (this)
		{
			return ++_useCount;
		}
	}

	public void ExportInstrumentMethod(string methodFilePath, bool forceOverwrite)
	{
		throw new NotImplementedException();
	}

	public bool RefreshViewOfFile()
	{
		throw new NotImplementedException();
	}

	public int RemoveUse()
	{
		lock (this)
		{
			return --_useCount;
		}
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposedValue)
		{
			return;
		}
		if (disposing)
		{
			if (Devices.IsAny())
			{
				DeviceContainer[] devices = Devices;
				for (int i = 0; i < devices.Length; i++)
				{
					devices[i]?.PartialDevice.Dispose();
				}
				Devices = Array.Empty<DeviceContainer>();
			}
			IViewCollectionManager manager = Manager;
			_randViewer?.ReleaseAndCloseMemoryMappedFile(manager);
			manager.Close(StreamId);
		}
		_disposedValue = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Refreshes the devices.
	/// </summary>
	/// <param name="numVc">
	/// The number of virtual controllers.
	/// </param>
	/// <exception cref="T:System.Exception">Thrown if device entry cannot be created
	/// </exception>
	public void RefreshDevices(int numVc)
	{
		int num = 0;
		if (Devices == null)
		{
			Devices = new DeviceContainer[numVc];
		}
		else
		{
			num = Devices.Length;
			if (num != numVc)
			{
				DeviceContainer[] array = new DeviceContainer[numVc];
				for (int i = 0; i < num; i++)
				{
					array[i] = Devices[i];
				}
				Devices = array;
			}
		}
		List<VirtualControllerInfo> virtualControllers = RawFileInformation.VirtualControllers;
		bool isInAcquisition = RawFileInformation.IsInAcquisition;
		int revision = base.Header.Revision;
		for (int j = num; j < numVc; j++)
		{
			VirtualControllerInfo virtualControllerInfo = virtualControllers[j];
			try
			{
				IDevice device = DeviceFactory.GetDevice(Manager, Id, virtualControllerInfo, DataFileMapName, revision, isInAcquisition);
				Devices[j] = new DeviceContainer
				{
					PartialDevice = device,
					FullDevice = new Lazy<IDevice>(() => device.Initialize())
				};
			}
			catch (Exception ex)
			{
				if (ex is NewerFileFormatException)
				{
					throw;
				}
				throw new Exception($"Error Encountered while loading {virtualControllerInfo.VirtualDeviceType} at offset {virtualControllerInfo.Offset}.\n{ex.Message}");
			}
		}
	}

	/// <summary>
	/// Gets the audit trail information.
	/// </summary>
	/// <returns>Audit trail object</returns>
	private AuditTrail GetAuditTrailInfo()
	{
		AuditTrail result = new AuditTrail();
		if (RawFileInformation.IsInAcquisition || FileRevision < 66)
		{
			return result;
		}
		int count = RawFileInformation.VirtualControllers.Count;
		DeviceContainer deviceContainer = Devices[count - 1];
		if (deviceContainer != null)
		{
			IDevice value = deviceContainer.FullDevice.Value;
			long startPos = value.OffsetOfEndOfDevice;
			VirtualDeviceTypes deviceType = value.DeviceType;
			if ((uint)deviceType <= 5u)
			{
				result = _randViewer.LoadRawFileObjectExt(() => new AuditTrail(), FileRevision, ref startPos);
			}
		}
		return result;
	}
}
