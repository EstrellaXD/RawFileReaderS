using System;
using System.Collections.Generic;
using System.IO;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The instrument method.
/// </summary>
internal sealed class Method : IMethod, IRawObjectBase
{
	private readonly List<StorageDescription> _storageDesc;

	private Lazy<List<StorageDescription>> _getStorageDescription;

	private MethodInfoStruct _methodInformation;

	private string _methodFileDiskLocation;

	/// <summary>
	/// Gets the file header.
	/// </summary>
	internal FileHeader FileHeader { get; private set; }

	/// <summary>
	/// Gets the method size.
	/// </summary>
	public int MethodSize => _methodInformation.MethodFileSize;

	/// <summary>
	/// Gets the original storage name.
	/// </summary>
	public string OriginalStorageName
	{
		get
		{
			return _methodFileDiskLocation;
		}
		private set
		{
			_methodFileDiskLocation = value;
			if (File.Exists(_methodFileDiskLocation))
			{
				FileInfo fileInfo = new FileInfo(_methodFileDiskLocation);
				_methodInformation.MethodFileSize = (int)fileInfo.Length;
			}
			else
			{
				_methodInformation.MethodFileSize = 0;
			}
		}
	}

	/// <summary>
	/// Gets the starting offset.
	/// </summary>
	public long StartingOffset { get; private set; }

	/// <summary>
	/// Gets or sets the storage descriptions.
	/// </summary>
	public List<StorageDescription> StorageDescriptions
	{
		get
		{
			if (_getStorageDescription != null)
			{
				return _getStorageDescription.Value;
			}
			return new List<StorageDescription>();
		}
		set
		{
			_getStorageDescription = new Lazy<List<StorageDescription>>(() => value);
		}
	}

	/// <summary>
	/// Gets the method info struct, only needed internally for writing raw file.
	/// </summary>
	internal MethodInfoStruct MethodInfoStruct => _methodInformation;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Method" /> class.
	/// </summary>
	public Method()
	{
		_getStorageDescription = null;
		_storageDesc = new List<StorageDescription>();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Method" /> class.
	/// </summary>
	/// <param name="header">
	/// The method header (not the raw file header).
	/// </param>
	/// <param name="methodFileLocation">
	/// The method file location.
	/// </param>
	public Method(FileHeader header, string methodFileLocation)
		: this()
	{
		OriginalStorageName = methodFileLocation;
		FileHeader = header;
	}

	/// <summary>
	/// Save instrument method file.
	/// </summary>
	/// <param name="viewer">Access to the raw file bytes</param>
	/// <param name="methodFilePath">
	/// The method file path.
	/// </param>
	/// <param name="forceOverWrite">
	/// Force over write. If true, and file already exists, attempt to delete existing file first.
	/// If false: UnauthorizedAccessException will occur if there is an existing read only file.
	/// </param>
	public void SaveMethodFile(IDisposableReader viewer, string methodFilePath, bool forceOverWrite)
	{
		byte[] buffer = viewer.ReadBytes(StartingOffset, MethodSize);
		if (forceOverWrite && File.Exists(methodFilePath))
		{
			File.Delete(methodFilePath);
		}
		using FileStream fileStream = File.Create(methodFilePath);
		fileStream.Write(buffer, 0, MethodSize);
	}

	/// <summary>
	/// Loads the instrument method from the memory mapped raw file viewer passed in.
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
		startPos = (viewer.PreferLargeReads ? Utilities.LoadDataFromInternalMemoryArrayReader(GetMethodInfo, viewer, startPos, 1048576) : GetMethodInfo(viewer, startPos));
		StartingOffset = startPos;
		_getStorageDescription = new Lazy<List<StorageDescription>>(() => GetMethodData(viewer, startPos, _storageDesc));
		return MethodSize + (startPos - dataOffset);
		long GetMethodInfo(IMemoryReader reader, long offset)
		{
			FileHeader = reader.LoadRawFileObjectExt(() => new FileHeader(), fileRevision, ref offset);
			if (FileHeader == null)
			{
				throw new Exception("Cannot read method file header!");
			}
			if (!FileHeader.IsSignatureValid())
			{
				throw new Exception("This is not a Thermo Fisher Method File!");
			}
			FileHeader.FileType = FileType.MethodFile;
			_methodInformation = reader.ReadStructureExt<MethodInfoStruct>(ref offset);
			_methodFileDiskLocation = reader.ReadStringExt(ref offset);
			if (FileHeader.Revision >= 44)
			{
				int num = reader.ReadIntExt(ref offset);
				for (int num2 = 0; num2 < num; num2++)
				{
					string description = reader.ReadStringExt(ref offset);
					string storageName = reader.ReadStringExt(ref offset);
					_storageDesc.Add(new StorageDescription(storageName, description));
				}
			}
			return offset;
		}
	}

	/// <summary>
	/// Gets the storage description.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="startPos">The start position.</param>
	/// <param name="storageDesc">The storage description</param>
	/// <returns>List of StorageDescription</returns>
	private List<StorageDescription> GetMethodData(IMemoryReader viewer, long startPos, List<StorageDescription> storageDesc)
	{
		byte[] map = viewer.ReadBytesExt(ref startPos, MethodSize);
		string text = SaveMethodBytesToFile(map, 0);
		if (!string.IsNullOrEmpty(text))
		{
			using (DeviceStorage deviceStorage = new DeviceStorage(text))
			{
				deviceStorage.EnumSubStgsNoRecursion(_storageDesc);
				foreach (StorageDescription item in _storageDesc)
				{
					try
					{
						InstrumentMethodDataAccess instrumentMethodDataAccess = deviceStorage.OpenDeviceComponent(item.StorageName);
						item.MethodText = instrumentMethodDataAccess.MethodText;
					}
					catch
					{
					}
				}
			}
			File.Delete(text);
		}
		return storageDesc;
	}

	/// <summary>
	/// The save method bytes to file.
	/// </summary>
	/// <param name="map">
	/// The map.
	/// </param>
	/// <param name="offset">
	/// The offset.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.String" />.
	/// </returns>
	private string SaveMethodBytesToFile(byte[] map, int offset)
	{
		string tempFileName = Path.GetTempFileName();
		if (string.IsNullOrEmpty(tempFileName))
		{
			return tempFileName;
		}
		using FileStream fileStream = File.Create(tempFileName);
		fileStream.Write(map, offset, MethodSize);
		return tempFileName;
	}
}
