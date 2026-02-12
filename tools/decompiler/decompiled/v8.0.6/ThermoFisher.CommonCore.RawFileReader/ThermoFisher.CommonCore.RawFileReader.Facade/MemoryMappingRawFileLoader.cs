using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
/// The raw file loader.
/// </summary>
internal class MemoryMappingRawFileLoader : LoaderBase, IDisposable, IRealTimeAccess, IRawFileLoader, IFileError
{
	private int _useCount;

	private readonly IReadWriteAccessor _randViewer;

	private OldLcqFile _oldLcqFile;

	private Lazy<AuditTrail> _auditTrailLazy;

	private bool _disposed;

	/// <summary>
	/// Gets the data file map name.
	/// </summary>
	public string DataFileMapName { get; }

	/// <summary>
	/// Gets the identifier.
	/// </summary>
	/// <value>
	/// The identifier.
	/// </value>
	public Guid Id { get; }

	/// <summary>
	/// Gets the raw file path.
	/// </summary>
	/// <value>
	/// The raw file path.
	/// </value>
	public string RawFileName { get; }

	/// <summary>
	/// Gets a value indicating whether this instance is open.
	/// </summary>
	/// <value>
	///   <c>true</c> if this instance is open; otherwise, <c>false</c>.
	/// </value>
	public bool IsOpen { get; }

	/// <summary>
	/// Gets the auto sampler config.
	/// </summary>
	/// <value>
	/// The automatic sampler configuration.
	/// </value>
	public IAutoSamplerConfig AutoSamplerConfig { get; private set; }

	/// <summary>
	/// Gets the method info.
	/// </summary>
	/// <value>
	/// The method information.
	/// </value>
	public IMethod MethodInfo { get; private set; }

	/// <summary>
	/// Gets or sets the raw file information.
	/// </summary>
	/// <value>
	/// The raw file information.
	/// </value>
	public IRawFileInfo RawFileInformation { get; set; }

	/// <summary>
	/// Gets the devices.
	/// </summary>
	/// <value>
	/// The devices.
	/// </value>
	public DeviceContainer[] Devices { get; private set; }

	/// <summary>
	/// Gets or sets the sequence.
	/// </summary>
	/// <value>
	/// The sequence.
	/// </value>
	public ISequenceRow Sequence { get; set; }

	/// <summary>
	/// Gets the stream id.
	/// </summary>
	private string StreamId { get; }

	/// <summary>
	/// Gets the file revision.
	/// </summary>
	public int FileRevision { get; private set; }

	/// <summary>
	/// Gets the header file map name.
	/// </summary>
	public string HeaderFileMapName { get; }

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

	public IViewCollectionManager Manager { get; }

	/// <summary>
	/// Prevents a default instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.MemoryMappingRawFileLoader" /> class from being created.
	/// </summary>
	private MemoryMappingRawFileLoader()
	{
		Utilities.Validate64Bit(": 32 bit process may be able to use RandomAccessFileManager");
		ClearAllErrorsAndWarnings();
		Devices = Array.Empty<DeviceContainer>();
		_oldLcqFile = null;
		Id = Guid.NewGuid();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.MemoryMappingRawFileLoader" /> class. 
	/// Default constructor initializes a new instance of RawFile class.
	/// Must be called prior to data access
	/// </summary>
	/// <param name="fileName">
	/// The file path.
	/// </param>
	/// <exception cref="T:System.ArgumentException">
	/// The file path is empty or null.
	/// </exception>
	/// <exception cref="T:System.Exception">
	/// A problem encountered when reading the raw file.
	/// </exception>
	internal MemoryMappingRawFileLoader(string fileName)
		: this()
	{
		bool isCreateMutexFailed = false;
		Manager = MemoryMappedRawFileManager.Instance;
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
			HeaderFileMapName = string.Empty;
			DataFileMapName = Utilities.GetFileMapName(RawFileName);
			StreamId = StreamHelper.ConstructStreamId(Id, RawFileName);
			string fileName2 = Utilities.CorrectNameForEnvironment(RawFileName);
			_randViewer = Manager.GetRandomAccessViewer(Id, fileName2, inAcquisition: false, DataFileAccessMode.OpenCreateReadLoaderId);
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
				if (Manager != null && _randViewer != null)
				{
					string streamId = _randViewer.StreamId;
					AppendError(Manager.GetErrors(streamId));
					IsOpen = Manager.IsOpen(streamId);
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
	/// Add a user of this loader
	/// </summary>
	/// <returns>the number of active users</returns>
	public int AddUse()
	{
		lock (this)
		{
			return ++_useCount;
		}
	}

	/// <summary>
	/// Remove a user of this loader
	/// </summary>
	/// <returns>the number of active users</returns>
	public int RemoveUse()
	{
		lock (this)
		{
			return --_useCount;
		}
	}

	/// <summary>
	/// Test if the file is still in acquisition.
	/// </summary>
	/// <returns>
	/// True if the file is still in acquisition.
	/// </returns>
	private bool TestIfStillInAcquisition()
	{
		bool flag = RawFileInformation?.IsInAcquisition ?? false;
		if (flag)
		{
			IRawFileInfo rawFileInformation = RawFileInformation;
			if (rawFileInformation != null && rawFileInformation.RefreshViewOfFile())
			{
				flag = RawFileInformation.IsInAcquisition;
			}
		}
		return flag;
	}

	/// <summary>
	/// The dispose.
	/// </summary>
	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		bool inAcquisition = TestIfStillInAcquisition();
		_disposed = true;
		RawFileInformation?.Dispose();
		if (Devices.IsAny())
		{
			DeviceContainer[] devices = Devices;
			foreach (DeviceContainer deviceContainer in devices)
			{
				if (deviceContainer != null)
				{
					IDevice partialDevice = deviceContainer.PartialDevice;
					partialDevice.InAcquisition = inAcquisition;
					partialDevice.Dispose();
				}
			}
			Devices = Array.Empty<DeviceContainer>();
		}
		IViewCollectionManager instance = MemoryMappedRawFileManager.Instance;
		_randViewer?.ReleaseAndCloseMemoryMappedFile(instance);
		_oldLcqFile?.Dispose();
		MemoryMappedRawFileManager.Instance.Close(StreamId);
	}

	/// <summary>
	/// The export instrument method.
	/// </summary>
	/// <param name="methodFilePath">
	/// The method file path.
	/// </param>
	/// <param name="forceOverwrite">
	/// The force overwrite.
	/// </param>
	public void ExportInstrumentMethod(string methodFilePath, bool forceOverwrite)
	{
		MethodInfo.SaveMethodFile(_randViewer, methodFilePath, forceOverwrite);
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
	private void LoadRawFile(IDisposableReader viewer, bool isCreateMutexFailed)
	{
		long startPos = 0L;
		if (viewer == null)
		{
			if (!string.IsNullOrWhiteSpace(base.ErrorMessage))
			{
				AppendError(Environment.NewLine);
			}
			AppendError($"{MemoryMappedRawFileManager.Instance.GetErrors(StreamId)} {RawFileName}");
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
	/// Read various blocks describing the raw file
	/// </summary>
	/// <param name="viewer">View into file</param>
	/// <param name="startPos">offset into view</param>
	/// <returns>Updated position</returns>
	private long ReadMetaData(IMemoryReader viewer, long startPos)
	{
		Sequence = viewer.LoadRawFileObjectExt<SequenceRow>(FileRevision, ref startPos);
		AutoSamplerConfig = viewer.LoadRawFileObjectExt<AutoSamplerConfig>(FileRevision, ref startPos);
		RawFileInformation = viewer.LoadRawFileObjectExt(() => new RawFileInfo(Manager, Id, DataFileMapName, FileRevision), FileRevision, ref startPos);
		return startPos;
	}

	/// <summary>
	/// Initialize device data, for "complete file" or "real time"
	/// Reform CRC check on completed files.
	/// </summary>
	/// <param name="viewer">view into file</param>
	private void InitializeDevices(IDisposableReader viewer)
	{
		if (RawFileInformation.IsInAcquisition)
		{
			RefreshViewOfFile();
			return;
		}
		ValidateCrc(viewer);
		InitialDeviceLists();
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
			_oldLcqFile = new OldLcqFile(MemoryMappedRawFileManager.Instance, this);
			_oldLcqFile.DecodeOldLcqFile(viewer, startPos, FileRevision);
		}
		return num;
	}

	/// <summary>
	/// The validate CRC.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <exception cref="T:System.Exception">Thrown if CRC is no valid
	/// </exception>
	private void ValidateCrc(IDisposableReader viewer)
	{
		if (!ValidCrc(viewer))
		{
			AppendError($"CRC failed [file revision #: {FileRevision}]");
			throw new Exception(base.ErrorMessage);
		}
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
				IDevice device = DeviceFactory.GetDevice(MemoryMappedRawFileManager.Instance, Id, virtualControllerInfo, Utilities.CorrectNameForEnvironment(RawFileName), base.Header.Revision, isInAcquisition);
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
		bool result = false;
		try
		{
			if (RawFileInformation.IsInAcquisition && RawFileInformation.RefreshViewOfFile() && RawFileInformation.IsInAcquisition)
			{
				int refreshFailCounter;
				int num = CreateInAcquisitionDeviceList(out refreshFailCounter);
				result = RawFileInformation.IsInAcquisition && num > 0 && refreshFailCounter == 0;
			}
		}
		catch (Exception)
		{
		}
		return result;
	}

	/// <summary>
	/// create the list of devices when in acquisition.
	/// Cannot "lazy load" here, as we need to "claim" a reference count
	/// to all devices, and all of their temp files.
	/// </summary>
	/// <param name="refreshFailCounter">
	/// The refresh fail counter.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	private int CreateInAcquisitionDeviceList(out int refreshFailCounter)
	{
		int count = RawFileInformation.VirtualControllers.Count;
		refreshFailCounter = 0;
		if (count > 0)
		{
			RefreshDevices(count);
			bool[] refreshedOk = new bool[count];
			Parallel.For(0, count, delegate(int index)
			{
				DeviceContainer deviceContainer = Devices[index];
				if (deviceContainer != null)
				{
					refreshedOk[index] = deviceContainer.FullDevice.Value.RefreshViewOfFile();
				}
			});
			refreshFailCounter += refreshedOk.Count((bool deviceOk) => !deviceOk);
		}
		else
		{
			Devices = Array.Empty<DeviceContainer>();
		}
		return count;
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
				IDevice device = DeviceFactory.GetDevice(MemoryMappedRawFileManager.Instance, Id, virtualControllerInfo, DataFileMapName, revision, isInAcquisition);
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
}
