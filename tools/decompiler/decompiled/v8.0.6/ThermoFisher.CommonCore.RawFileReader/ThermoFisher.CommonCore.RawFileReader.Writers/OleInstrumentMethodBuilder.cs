using System;
using System.Collections.Generic;
using System.IO;
using OpenMcdf;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Instrument method file is a compound document file (contains a ROOT Storage) which are organized in a 
/// hierarchy of storages. The names of all device storages and streams that are
/// direct members of a storage must be different (unique).  Names of device streams or
/// storages that are members of different storages may be equal.
/// Example.
/// Instrument method file 
///     Root storage
///         |-stream0
///         |-storage1 
///             |-stream1
///             |-stream2
///         |-storage2
///             |-stream1
/// ----
/// </summary>
/// <seealso cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.DeviceErrors" />
internal class OleInstrumentMethodBuilder : DeviceErrors, IInstrumentMethodBuilder
{
	private readonly Dictionary<string, IDeviceMethod> _devices;

	private readonly AuditTrail _auditTrail;

	private ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader _fileHeader;

	private bool _isWhoModifiedChanged;

	/// <summary>
	/// Gets the name of the instrument method file.
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Gets the file header for the instrument method file
	/// </summary>
	public IFileHeader FileHeader => new ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader(_fileHeader);

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.OleInstrumentMethodBuilder" /> class.<para />
	/// Opens the instrument method file for editing if the file exists; otherwise, creates an in-memory
	/// file. After you've finished editing, calls the "Save" or "SaveAs" method to persist the data to the file.
	/// </summary>
	/// <param name="fileName">The instrument method file Name. </param>
	public OleInstrumentMethodBuilder(string fileName)
	{
		Name = fileName;
		_devices = new Dictionary<string, IDeviceMethod>();
		_auditTrail = new AuditTrail
		{
			AuditDataInfo = Array.Empty<AuditData>()
		};
		_fileHeader = ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader.CreateFileHeader(FileType.MethodFile);
		if (!string.IsNullOrWhiteSpace(fileName) && File.Exists(fileName))
		{
			OpenInstrumentMethodFile(fileName);
		}
	}

	/// <summary>
	/// Creates a device method storage.
	/// </summary>
	/// <param name="storage"> The storage (root storage). </param>
	/// <param name="methodName"> Name of the device method storage. </param>
	/// <param name="errors">Stores the error information if error occurs.</param>
	/// <returns>Device method storage. </returns>
	private static CFStorage CreateDeviceMethodStorage(CFStorage storage, string methodName, DeviceErrors errors)
	{
		CFStorage result = null;
		try
		{
			result = storage.AddStorage(methodName);
		}
		catch (Exception ex)
		{
			errors.UpdateError(ex);
		}
		return result;
	}

	/// <summary>
	/// Saves the audit trail. 
	/// </summary>
	/// <param name="storage">The storage (root storage).</param>
	/// <param name="auditTrail">The audit trail.</param>
	/// <param name="errors">Stores the error information if error occurs.</param>
	/// <returns>True if audit trail is saved successfully; otherwise false. </returns>
	private static bool SaveAuditTrail(CFStorage storage, AuditTrail auditTrail, DeviceErrors errors)
	{
		return OleInstrumentMethodHelper.TryCatchSaveStreamData(auditTrail.Save, storage, "AuditData", errors);
	}

	/// <summary>
	/// Saves the file header to stream.
	/// </summary>
	/// <param name="storage">The storage.</param>
	/// <param name="fileHeader">The file header.</param>
	/// <param name="dt">The current date time.</param>
	/// <param name="errors">Stores the error information if error occurs.</param>
	/// <returns>True if file header saved successfully; otherwise false. </returns>
	private static bool SaveFileHeader(CFStorage storage, ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader fileHeader, DateTime dt, DeviceErrors errors)
	{
		fileHeader.NumberOfTimesModified++;
		fileHeader.ModifiedDate = dt;
		return OleInstrumentMethodHelper.TryCatchSaveStreamData(fileHeader.Save, storage, "LCQ Header", errors);
	}

	/// <summary>
	/// Saves the device method to stream.
	/// </summary>
	/// <param name="deviceMethod">The device method.</param>
	/// <param name="storage">The storage.</param>
	/// <param name="errors">Stores the error information if error occurs.</param>
	/// <returns>True if the device method saved successfully; otherwise false.</returns>
	private static bool SaveDeviceMethod(IDeviceMethod deviceMethod, CFStorage storage, DeviceErrors errors)
	{
		return SaveMethodStreams(storage, deviceMethod.GetStreamBytes(), errors);
	}

	/// <summary>
	/// Saves the device method streams, i.e. "Text", "Data, etc. <para />
	/// If the stream value is NULL, it will save an empty stream.
	/// </summary>
	/// <param name="storage">The storage.</param>
	/// <param name="streamBytes">The list of streams which are going to save. </param>
	/// <param name="errors">Stores the error information if error occurs.</param>
	/// <returns>True if method streams are saved successfully; otherwise false. </returns>
	private static bool SaveMethodStreams(CFStorage storage, Dictionary<string, byte[]> streamBytes, DeviceErrors errors)
	{
		foreach (KeyValuePair<string, byte[]> streamByte in streamBytes)
		{
			byte[] bytes = streamByte.Value;
			if (!OleInstrumentMethodHelper.TryCatchSaveStreamData(streamName: streamByte.Key, method: delegate(Stream streamer, DeviceErrors error)
			{
				if (bytes != null)
				{
					streamer.Write(bytes, 0, bytes.Length);
				}
				return true;
			}, storage: storage, error: errors))
			{
				break;
			}
		}
		return !errors.HasError;
	}

	/// <summary>
	/// Updates the file header field - "Description".
	/// </summary>
	/// <param name="description">The description.</param>
	public void UpdateFileHeaderDescription(string description)
	{
		_fileHeader.FileDescription = description;
	}

	/// <summary>
	/// Update the instrument method file header with the file header values passed in.  
	/// Only updates object values in memory, does not write to disk.
	/// </summary>
	/// <param name="fileHeader">The file header.</param>
	public void UpdateFileHeader(IFileHeader fileHeader)
	{
		_isWhoModifiedChanged = _fileHeader.CheckIsWhoModifiedChanged(fileHeader);
		_fileHeader = new ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader(fileHeader);
	}

	/// <summary>
	/// Get the list of device methods which are currently defined in this instrument method.<para />
	/// Returns an empty list, if this is a newly created instrument method.<para />
	/// ---
	/// In order to add/update device method, caller should first call this to get the list of devices.<para />
	/// Once you've the list, you can start adding a new device method or editing/removing an existing device method.
	/// </summary>
	/// <returns>The list of device methods.</returns>
	public Dictionary<string, IDeviceMethod> GetDevices()
	{
		return _devices;
	}

	/// <summary>
	/// Save this instrument methods to a file.<para />
	/// It should overwrite the instrument methods file if the file exists; otherwise, a 
	/// new file should be created.
	/// </summary>
	/// <param name="fileName">File name of the instrument method.</param>
	/// <returns>True if save successfully; otherwise false.</returns>
	/// <exception cref="T:System.ArgumentNullException">name;@The name cannot be empty.</exception>
	public bool SaveAs(string fileName)
	{
		WriterHelper.ValidateName(fileName, "The instrument methods file name cannot be empty.");
		if (base.HasError)
		{
			return false;
		}
		Name = WriterHelper.ValidInstrumentMethodFileExtension(fileName);
		CompoundFile compoundFile = null;
		CFStorage rootStorage = null;
		DateTime localTime = DateTime.Now;
		try
		{
			if (File.Exists(Name))
			{
				File.Delete(Name);
			}
			else
			{
				_fileHeader.CreationDate = localTime;
				_fileHeader.NumberOfTimesModified = 0;
			}
			compoundFile = OleInstrumentMethodHelper.CreateDocFile();
			rootStorage = compoundFile.RootStorage;
			WriterHelper.TryCatch(delegate(DeviceErrors errors)
			{
				bool flag = false;
				if (!_isWhoModifiedChanged)
				{
					ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader fileHeader = _fileHeader;
					string whoModifiedLogon = (_fileHeader.WhoModifiedId = Environment.UserName);
					fileHeader.WhoModifiedLogon = whoModifiedLogon;
				}
				if (rootStorage != null && SaveAuditTrail(rootStorage, _auditTrail, errors) && SaveFileHeader(rootStorage, _fileHeader, localTime, errors))
				{
					flag = _devices.Count == 0;
					foreach (KeyValuePair<string, IDeviceMethod> device in _devices)
					{
						IDeviceMethod value = device.Value;
						CFStorage storage = CreateDeviceMethodStorage(rootStorage, device.Key, errors);
						flag = SaveDeviceMethod(value, storage, errors);
						if (!flag)
						{
							break;
						}
					}
					if (_fileHeader.UpdateFileHeaderChecksum(compoundFile.RootStorage, errors))
					{
						flag = OleInstrumentMethodHelper.TryCatchSaveStreamData(_fileHeader.Save, rootStorage, "LCQ Header", errors);
					}
				}
				return flag;
			}, this);
		}
		finally
		{
			compoundFile.SaveAs(Name);
			compoundFile.Close();
		}
		return !base.HasError;
	}

	/// <summary>
	/// Opens an existing compound file (instrument method).
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	private void OpenInstrumentMethodFile(string fileName)
	{
		try
		{
			IInstrumentMethodFileAccess instrumentMethodFileAccess = InstrumentMethodReaderFactory.ReadFile(fileName);
			if (instrumentMethodFileAccess.IsError)
			{
				UpdateError(instrumentMethodFileAccess.FileError.ErrorMessage, instrumentMethodFileAccess.FileError.ErrorCode);
				return;
			}
			_fileHeader = instrumentMethodFileAccess.FileHeader as ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader;
			foreach (KeyValuePair<string, IInstrumentMethodDataAccess> device in instrumentMethodFileAccess.Devices)
			{
				string key = device.Key;
				IInstrumentMethodDataAccess value = device.Value;
				IReadOnlyDictionary<string, byte[]> streamBytes = value.StreamBytes;
				DeviceMethod deviceMethod = new DeviceMethod();
				_devices.Add(key, deviceMethod);
				if (!string.IsNullOrWhiteSpace(value.MethodText))
				{
					deviceMethod.MethodText = value.MethodText;
				}
				Dictionary<string, byte[]> streamBytes2 = deviceMethod.StreamBytes;
				foreach (KeyValuePair<string, byte[]> item in streamBytes)
				{
					streamBytes2.Add(item.Key, item.Value);
				}
			}
		}
		catch (Exception ex)
		{
			UpdateError(ex);
		}
	}
}
