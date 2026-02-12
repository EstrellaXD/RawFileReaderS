using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
///     The raw file info.
/// </summary>
internal class RawFileInfo : IRawFileInfo, IRealTimeAccess, IDisposable, IRawObjectBase, IErrors
{
	private static readonly int RawFileInfoStructSize = Utilities.StructSizeLookup.Value[1];

	private readonly DeviceErrors _errors;

	private readonly Guid _loaderId;

	private IReadWriteAccessor _acqDataViewer;

	private bool _disposed;

	private RawFileInfoStruct _info;

	/// <summary>
	///     Gets the blob size.
	/// </summary>
	public uint BlobSize => _info.BlobSize;

	/// <summary>
	///     Gets the blob start.
	/// </summary>
	public long BlobStart
	{
		get
		{
			if (_info.BlobSize != 0)
			{
				return _info.BlobOffset;
			}
			return -1L;
		}
	}

	/// <summary>
	///     Gets or sets the computer name.
	/// </summary>
	public string ComputerName { get; set; }

	/// <summary>
	/// Gets the data file map name.
	/// </summary>
	public string DataFileMapName { get; }

	/// <summary>
	/// Gets the error message.
	/// </summary>
	public string ErrorMessage => _errors.ErrorMessage;

	/// <summary>
	/// Gets the file revision.
	/// </summary>
	public int FileRevision { get; }

	/// <summary>
	/// Gets a value indicating whether this file has detected an error.
	/// If this is false: Other error properties in this interface have no meaning.
	/// </summary>
	public bool HasError => _errors.HasError;

	/// <summary>
	///     Gets a value indicating whether has experiment method.
	/// </summary>
	public bool HasExpMethod => _info.IsExpMethodPresent;

	/// <summary>
	/// Gets the header file map name.
	/// </summary>
	public string HeaderFileMapName { get; }

	/// <summary>
	///     Gets or sets a value indicating whether is in acquisition.
	/// </summary>
	public bool IsInAcquisition
	{
		get
		{
			return _info.IsInAcquisition;
		}
		set
		{
			_info.IsInAcquisition = value;
		}
	}

	/// <summary>
	///     Gets or sets the MS data offset.
	/// </summary>
	public long MsDataOffset
	{
		get
		{
			return _info.VirtualDataOffset;
		}
		set
		{
			_info.VirtualDataOffset = value;
		}
	}

	/// <summary>
	///     Gets the next available controller index.
	/// </summary>
	public int NextAvailableControllerIndex => _info.NextAvailableControllerIndex;

	/// <summary>
	///     Gets the number of virtual controllers.
	/// </summary>
	public int NumberOfVirtualControllers => VirtualControllers.Count;

	/// <summary>
	/// Gets or sets the raw file info struct.
	/// Setting only needed during raw file save. Refreshes controller data on set.
	/// </summary>
	public RawFileInfoStruct RawFileInfoStruct
	{
		get
		{
			return _info;
		}
		internal set
		{
			_info = value;
			PopulateVirtualControllerData();
		}
	}

	/// <summary>
	///     Gets the time stamp.
	/// </summary>
	public DateTime TimeStamp { get; private set; }

	public IViewCollectionManager Manager { get; }

	/// <summary>
	///     Gets the user labels.
	/// </summary>
	public string[] UserLabels { get; }

	/// <summary>
	///     Gets the virtual controller data.
	/// </summary>
	public List<VirtualControllerInfo> VirtualControllers { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.RawFileInfo" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">Unique id for this instance</param>
	/// <param name="fileName">The file path.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="inAcquisition">True if in the process of acquiring raw file</param>
	public RawFileInfo(IViewCollectionManager manager, Guid loaderId, string fileName, int fileRevision, bool inAcquisition = false)
		: this(manager, loaderId)
	{
		FileRevision = fileRevision;
		HeaderFileMapName = string.Empty;
		DataFileMapName = Utilities.MapName(fileName, "FMAT_RAWFILEINFO");
		if (inAcquisition)
		{
			RefreshViewOfFile();
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.RawFileInfo" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">
	/// The loader id.
	/// </param>
	private RawFileInfo(IViewCollectionManager manager, Guid loaderId)
	{
		Manager = manager;
		UserLabels = new string[5];
		VirtualControllers = new List<VirtualControllerInfo>(64);
		_loaderId = loaderId;
		_acqDataViewer = null;
		_errors = new DeviceErrors();
	}

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_acqDataViewer.ReleaseAndCloseMemoryMappedFile(Manager, forceToCloseMmf: true);
		}
	}

	/// <summary>
	/// Loads the raw file info structure from the mapped memory raw file using the viewer.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="dataOffset">
	/// The data offset.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int64" />.
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		Array.Clear(UserLabels, 0, 5);
		if (fileRevision >= 65)
		{
			_info = viewer.ReadStructureExt<RawFileInfoStruct>(ref startPos);
		}
		else if (fileRevision >= 64)
		{
			_info = viewer.ReadPreviousRevisionAndConvertExt<RawFileInfoStruct, RawFileInfoStruct4>(ref startPos);
		}
		else if (fileRevision >= 25)
		{
			_info = viewer.ReadPreviousRevisionAndConvertExt<RawFileInfoStruct, RawFileInfoStruct3>(ref startPos);
			_info = StructureConversion.ConvertFrom32Bit(_info);
		}
		else if (fileRevision >= 7)
		{
			_info = viewer.ReadPreviousRevisionAndConvertExt<RawFileInfoStruct, RawFileInfoStruct2>(ref startPos);
			_info = StructureConversion.ConvertFrom32Bit(_info);
		}
		else
		{
			_info = viewer.ReadPreviousRevisionAndConvertExt<RawFileInfoStruct, RawFileInfoStruct1>(ref startPos);
			_info = StructureConversion.ConvertFrom32Bit(_info);
		}
		PopulateVirtualControllerData();
		if (fileRevision < 7)
		{
			_info.TimeStructStamp = default(SystemTimeStruct);
		}
		if (_info.TimeStructStamp.Year == 0)
		{
			TimeStamp = DateTime.Now;
		}
		else
		{
			TimeStamp = new DateTime(_info.TimeStructStamp.Year, _info.TimeStructStamp.Month, _info.TimeStructStamp.Day, _info.TimeStructStamp.Hour, _info.TimeStructStamp.Minute, _info.TimeStructStamp.Second, _info.TimeStructStamp.Milliseconds);
		}
		if (fileRevision < 65)
		{
			_info.BlobOffset = -1L;
		}
		for (int i = 0; i < 5; i++)
		{
			UserLabels[i] = viewer.ReadStringExt(ref startPos);
		}
		if (fileRevision >= 7)
		{
			ComputerName = viewer.ReadStringExt(ref startPos);
		}
		return startPos - dataOffset;
	}

	/// <summary>
	/// The number of virtual controllers of type.
	/// </summary>
	/// <param name="type">
	/// The type.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	public int NumberOfVirtualControllersOfType(VirtualDeviceTypes type)
	{
		if (VirtualControllers.Count <= 0)
		{
			return 0;
		}
		return VirtualControllers.Count((VirtualControllerInfo controllerInfo) => controllerInfo.VirtualDeviceType == type);
	}

	/// <summary>
	/// try to reload, without remapping.
	/// </summary>
	/// <param name="thorwError">
	/// throw exception on Error (else return false)
	/// </param>
	/// <returns>
	/// true if can reload
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// Bad data loaded
	/// </exception>
	private bool TryReload(bool thorwError = false)
	{
		if (_acqDataViewer != null)
		{
			Load(_acqDataViewer, 0L, FileRevision);
			if (!ValidateVirtualControllerData())
			{
				if (thorwError)
				{
					throw new ArgumentOutOfRangeException("Invalid virtual controllers (ex. VirtualDeviceIndex out of range)");
				}
				return false;
			}
			return true;
		}
		return false;
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
		try
		{
			if (TryReload())
			{
				return true;
			}
			Tuple<string, bool> tuple = Utilities.RetryMethod(delegate
			{
				_acqDataViewer = _acqDataViewer.GetMemoryMappedViewer(_loaderId, DataFileMapName, inAcquisition: true, DataFileAccessMode.OpenRead, PersistenceMode.NonPersisted);
				if (TryReload(thorwError: true))
				{
					return new Tuple<string, bool>(string.Empty, item2: true);
				}
				string errors;
				bool item = MemoryMappedFileHelper.IsFailedToMapAZeroLengthFile(StreamHelper.ConstructStreamId(_loaderId, DataFileMapName), out errors);
				return new Tuple<string, bool>(errors, item);
			}, 5, 100);
			if (tuple.Item2)
			{
				return true;
			}
			_errors.UpdateError(tuple.Item1);
		}
		catch (Exception ex)
		{
			_errors.UpdateError(ex.ToMessageAndCompleteStacktrace(), ex.HResult);
		}
		_info.NumberOfVirtualControllers = 0;
		VirtualControllers.Clear();
		return false;
	}

	/// <summary>
	/// Updates the virtual controller.
	/// </summary>
	/// <param name="numVirControllers">The number of virtual controllers.</param>
	/// <param name="virOffset">The virtual data offset</param>
	/// <param name="offset">The offset.</param>
	/// <param name="virDeviceIndex">Index of the virtual device.</param>
	/// <param name="virDeviceType">Type of the virtual device.</param>
	public void UpdateVirtualController(int numVirControllers, long virOffset, long offset, int virDeviceIndex, VirtualDeviceTypes virDeviceType)
	{
		_info.VirtualDataOffset = virOffset;
		_info.NextAvailableControllerIndex = (_info.NumberOfVirtualControllers = numVirControllers);
		_info.VirtualControllerInfoStruct[virDeviceIndex].Offset = offset;
		_info.VirtualControllerInfoStruct[virDeviceIndex].VirtualDeviceIndex = virDeviceIndex;
		_info.VirtualControllerInfoStruct[virDeviceIndex].VirtualDeviceType = virDeviceType;
		PopulateVirtualControllerData();
	}

	/// <summary>
	/// Saves the RawFileInfo to the provided binary writer.
	/// </summary>
	/// <param name="binaryWriter">Writer to use to save</param>
	/// <param name="errors">Any error information that occurred</param>
	/// <returns>True is successful</returns>
	public bool Save(BinaryWriter binaryWriter, DeviceErrors errors)
	{
		errors.AppendInformataion("Start: Save RawFileInfo");
		try
		{
			binaryWriter.Write(WriterHelper.StructToByteArray(RawFileInfoStruct, RawFileInfoStructSize));
			for (int i = 0; i < 5; i++)
			{
				binaryWriter.StringWrite(UserLabels[i]);
			}
			binaryWriter.StringWrite(ComputerName);
			binaryWriter.Flush();
			errors.AppendInformataion("End: Save RawFileInfo (success)");
			return true;
		}
		catch (Exception ex)
		{
			errors.AppendWarning("End: Save RawFileInfo (exception)");
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	///     The populate virtual controller data.
	/// </summary>
	private void PopulateVirtualControllerData()
	{
		VirtualControllers.Clear();
		for (int i = 0; i < _info.NumberOfVirtualControllers; i++)
		{
			_info.VirtualControllerInfoStruct[i].VirtualDeviceIndex = i;
			VirtualControllers.Add(new VirtualControllerInfo(_info.VirtualControllerInfoStruct[i]));
		}
	}

	/// <summary>
	/// Validates the virtual controller data.
	/// </summary>
	/// <returns>True if valid</returns>
	private bool ValidateVirtualControllerData()
	{
		foreach (VirtualControllerInfo virtualController in VirtualControllers)
		{
			if (virtualController.VirtualDeviceIndex > 64 || virtualController.VirtualDeviceIndex < 0)
			{
				return false;
			}
		}
		return true;
	}
}
