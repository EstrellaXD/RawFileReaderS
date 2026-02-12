using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using OpenMcdf;
using OpenMcdf.Extensions;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Reader for instrument method device data.
/// Class is a wrapper over Win32 API's.
/// Deals with IOleStorage and IOleStream
/// </summary>
internal class DeviceStorage : IDisposable
{
	/// <summary>
	/// The file header errors.
	/// </summary>
	internal enum FileHeaderErrors
	{
		/// <summary>
		/// There was no error
		/// </summary>
		NoError,
		/// <summary>
		/// There was a COM error
		/// </summary>
		ComError,
		/// <summary>
		/// There was a CRC error
		/// </summary>
		CrcError,
		/// <summary>
		/// Cannot open file stream
		/// </summary>
		OpenStreamError
	}

	/// <summary>
	/// The read header result.
	/// </summary>
	internal class ReadHeaderResult
	{
		/// <summary>
		/// Gets or sets the error class.
		/// </summary>
		public FileHeaderErrors ErrorClass { get; set; }

		/// <summary>
		/// Gets or sets the error message.
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Gets or sets the header.
		/// </summary>
		public IFileHeader Header { get; set; }
	}

	private readonly string _fileName;

	private CFStorage _storage;

	private CompoundFile _compoundFile;

	private bool _isDisposed;

	/// <summary>
	/// Gets the error message.
	/// </summary>
	public string ErrorMessage { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DeviceStorage" /> class. 
	/// Constructor with filename as parameter
	/// </summary>
	/// <param name="fileName">
	/// method data file
	/// </param>
	public DeviceStorage(string fileName)
	{
		_fileName = fileName;
		ErrorMessage = OpenStorage(StgMode.ShareExclusive);
		if (!string.IsNullOrWhiteSpace(ErrorMessage))
		{
			ErrorMessage = OpenStorage(StgMode.ShareDenyWrite);
		}
	}

	private static bool IsType(CFItem cfItem, StgType storageType)
	{
		if ((storageType == StgType.Storage && cfItem.IsStorage) || (storageType == StgType.Stream && cfItem.IsStream))
		{
			return true;
		}
		return false;
	}

	public static List<string> GetStorageNames(CFStorage storage, StgType storageType)
	{
		List<string> names = new List<string>();
		storage.VisitEntries(delegate(CFItem cfItem)
		{
			if (IsType(cfItem, storageType))
			{
				names.Add(cfItem.Name);
			}
		}, recursive: true);
		return names;
	}

	/// <summary>
	/// Reads the header from the instrument method file
	/// </summary>
	/// <returns>The file header</returns>
	public ReadHeaderResult ReadFileHeader()
	{
		Stream stream = null;
		try
		{
			stream = _storage.GetStream("LCQ Header").AsIOStream();
			if (stream == null)
			{
				return new ReadHeaderResult
				{
					ErrorMessage = "Unable to open Header stream.",
					ErrorClass = FileHeaderErrors.OpenStreamError
				};
			}
			long offset = 0L;
			stream.Seek(offset, SeekOrigin.Begin);
			StreamIo streamIo = new StreamIo(stream);
			byte[] array = new byte[Utilities.StructSizeLookup.Value[3]];
			streamIo.Read(array);
			FileHeaderStruct header = MemMapReader.ConvertBytesToStructure<FileHeaderStruct>(array);
			stream.Close();
			if (header.FileRev < 50 || (_storage.CalcChecksum(out var checksum, out var _) && checksum == header.CheckSum))
			{
				return new ReadHeaderResult
				{
					Header = FileHeader.FromHeader(header)
				};
			}
			return new ReadHeaderResult
			{
				ErrorMessage = "Checksum value does not match; the file may be corrupt.",
				ErrorClass = FileHeaderErrors.CrcError
			};
		}
		catch (COMException ex)
		{
			return new ReadHeaderResult
			{
				ErrorMessage = ex.Message,
				ErrorClass = FileHeaderErrors.ComError
			};
		}
		finally
		{
			stream.Close();
		}
	}

	/// <summary>
	/// Loads component details from storage.
	/// </summary>
	/// <param name="deviceName">
	/// Name of component
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.InstrumentMethodDataAccess" />.
	/// </returns>
	public InstrumentMethodDataAccess OpenDeviceComponent(string deviceName)
	{
		InstrumentMethodDataAccess instrumentMethodDataAccess = new InstrumentMethodDataAccess();
		CFStorage cFStorage = null;
		try
		{
			if (_storage != null)
			{
				cFStorage = _storage.GetStorage(deviceName);
			}
			instrumentMethodDataAccess.Open(cFStorage);
			return instrumentMethodDataAccess;
		}
		finally
		{
			_ = cFStorage != null;
		}
	}

	/// <summary>
	/// Enumerates the sub STGS no recursion.
	/// </summary>
	/// <param name="storageDescriptions">The storage descriptions.</param>
	/// <returns>true if successful</returns>
	public bool EnumSubStgsNoRecursion(List<StorageDescription> storageDescriptions)
	{
		List<string> storageNames = GetStorageNames(_storage, StgType.Storage);
		if (storageNames == null)
		{
			return false;
		}
		foreach (string name in storageNames)
		{
			if (storageDescriptions.All((StorageDescription s) => string.CompareOrdinal(s.StorageName, name) != 0))
			{
				storageDescriptions.Add(new StorageDescription(name, name));
			}
		}
		return true;
	}

	/// <summary>
	/// Open a storage file
	/// </summary>
	/// <param name="accessMode">The access mode.</param>
	/// <returns>
	/// The error message, or empty string if OK
	/// </returns>
	private string OpenStorage(StgMode accessMode)
	{
		try
		{
			_compoundFile = new CompoundFile(_fileName);
			_storage = _compoundFile.RootStorage;
		}
		catch (COMException ex)
		{
			return ex.Message;
		}
		catch (Exception ex2)
		{
			return ex2.Message;
		}
		return string.Empty;
	}

	/// <summary>
	/// Cleans up the resources 
	/// </summary>
	/// <param name="isDisposable">bool, flag whether .Net resources needs to be released or not</param>
	private void Dispose(bool isDisposable)
	{
		if (!_isDisposed)
		{
			if (isDisposable && _storage != null)
			{
				_compoundFile.Close();
			}
			_isDisposed = true;
		}
	}

	/// <summary>
	/// Releases all resources 
	/// </summary>
	public void Dispose()
	{
		Dispose(isDisposable: true);
		GC.SuppressFinalize(this);
	}
}
