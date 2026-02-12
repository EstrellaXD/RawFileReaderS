using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenMcdf;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
///     The file header.
/// </summary>
internal class FileHeader : IFileHeader, IRawObjectBase, IFileHeaderUpdate
{
	public const string FileHeaderStreamName = "LCQ Header";

	private UserIdStamp _creator;

	private UserIdStamp _modifier;

	private FileType _fileType;

	private FileHeaderStruct _header;

	/// <summary>
	///     Gets or sets the format revision of this file.
	///     Note: this does not refer to revisions of the content.
	///     It defines revisions of the binary files structure.
	/// </summary>
	public int Revision
	{
		get
		{
			return _header.FileRev;
		}
		set
		{
			_header.FileRev = (ushort)value;
		}
	}

	/// <summary>
	///     Gets or sets the file description.
	/// </summary>
	public string FileDescription
	{
		get
		{
			return _header.FileDescription;
		}
		set
		{
			_header.FileDescription = value;
		}
	}

	/// <summary>
	/// Gets the check sum.
	/// </summary>
	public uint CheckSum => _header.CheckSum;

	/// <summary>
	/// Gets or sets the number of times modified.
	/// </summary>
	/// <value>
	/// The number of times the file has been modified.
	/// </value>
	public int NumberOfTimesModified
	{
		get
		{
			return _header.TimesEdited;
		}
		set
		{
			_header.TimesEdited = (ushort)value;
		}
	}

	/// <summary>
	/// Gets or sets the number of times calibrated.
	/// </summary>
	/// <value>
	/// The number of times calibrated.
	/// </value>
	public int NumberOfTimesCalibrated
	{
		get
		{
			return _header.TimesCalibrated;
		}
		set
		{
			_header.TimesCalibrated = (ushort)value;
		}
	}

	/// <summary>
	/// Gets or sets the who created id.
	/// </summary>
	public string WhoCreatedId
	{
		get
		{
			return _creator.UserName;
		}
		set
		{
			_header.Created.UserName = value;
			_creator.UserName = value;
		}
	}

	/// <summary>
	/// Gets or sets the who created logon.
	/// </summary>
	public string WhoCreatedLogon
	{
		get
		{
			return _creator.WindowsLogin;
		}
		set
		{
			_header.Created.WindowsLogin = value;
			_creator.WindowsLogin = value;
		}
	}

	/// <summary>
	/// Gets or sets the who modified id.
	/// </summary>
	public string WhoModifiedId
	{
		get
		{
			return _modifier.UserName;
		}
		set
		{
			_header.Changed.UserName = value;
			_modifier.UserName = value;
		}
	}

	/// <summary>
	/// Gets or sets the who modified logon.
	/// </summary>
	public string WhoModifiedLogon
	{
		get
		{
			return _modifier.WindowsLogin;
		}
		set
		{
			_header.Changed.WindowsLogin = value;
			_modifier.WindowsLogin = value;
		}
	}

	/// <summary>
	/// Gets or sets the file type.
	/// </summary>
	public FileType FileType
	{
		get
		{
			return _fileType;
		}
		set
		{
			_fileType = value;
			_header.FileType = (ushort)value;
		}
	}

	/// <summary>
	/// Gets or sets the creation date.
	/// </summary>
	public DateTime CreationDate
	{
		get
		{
			return _creator.DateAndTime;
		}
		set
		{
			_header.Created.TimeStamp = TypeConverters.DateTimeToFileTime(value);
			_creator.DateAndTime = value;
		}
	}

	/// <summary>
	/// Gets or sets the modified date.
	/// </summary>
	public DateTime ModifiedDate
	{
		get
		{
			return _modifier.DateAndTime;
		}
		set
		{
			_header.Changed.TimeStamp = TypeConverters.DateTimeToFileTime(value);
			_modifier.DateAndTime = value;
		}
	}

	/// <summary>
	/// Gets the file header struct, only needed internally for writing file header.
	/// </summary>
	internal FileHeaderStruct FileHeaderStruct => _header;

	/// <summary>
	/// Initializes a new instance of the FileHeader class.
	/// Sets default file type to raw file and some other defaults.
	/// </summary>
	public FileHeader()
	{
		_header = new FileHeaderStruct
		{
			FileType = 8,
			FinnSig = "Finnigan",
			FinnID = 41217,
			FileRev = 66,
			TimesEdited = 1
		};
		Initialize();
	}

	/// <summary>
	/// Copy constructor
	/// Initializes a new instance of the FileHeader class to the values of an existing IFileHeader object.
	/// </summary>
	/// <param name="fromFileHeader">The IFileHeader object.</param>
	public FileHeader(IFileHeader fromFileHeader)
		: this()
	{
		FileType = fromFileHeader.FileType;
		FileDescription = fromFileHeader.FileDescription;
		NumberOfTimesCalibrated = fromFileHeader.NumberOfTimesCalibrated;
		NumberOfTimesModified = fromFileHeader.NumberOfTimesModified;
		Revision = fromFileHeader.Revision;
		CreationDate = fromFileHeader.CreationDate;
		ModifiedDate = fromFileHeader.ModifiedDate;
		WhoCreatedId = fromFileHeader.WhoCreatedId;
		WhoCreatedLogon = fromFileHeader.WhoCreatedLogon;
		WhoModifiedId = fromFileHeader.WhoModifiedId;
		WhoModifiedLogon = fromFileHeader.WhoModifiedLogon;
	}

	/// <summary>
	/// Wrap a file header struct.
	/// </summary>
	/// <param name="header">File header to wrap.</param>
	/// <returns>An object which wraps the given struct.</returns>
	public static FileHeader FromHeader(FileHeaderStruct header)
	{
		FileHeader fileHeader = new FileHeader();
		fileHeader._header = header;
		fileHeader.Initialize();
		return fileHeader;
	}

	/// <summary>
	/// Creates an initial header file object for file writing (i.e. raw file, sequence file, method file, etc.) 
	/// with the specified file type and description.
	/// This method will also initialize the internal fields, such as set the creation/modified date to current date time,
	/// and set the who created/modified to be the person who is currently logged on to the Windows OS.
	/// </summary>
	/// <param name="fileType">File type, i.e. raw file, sequence file, method file, etc.</param>
	/// <param name="description">[Optional] The description about the file.</param>
	/// <returns>The file header object.</returns>
	public static FileHeader CreateFileHeader(FileType fileType, string description = "")
	{
		string userName = Environment.UserName;
		DateTime now = DateTime.Now;
		return new FileHeader
		{
			FileType = fileType,
			FileDescription = description,
			CreationDate = now,
			ModifiedDate = now,
			WhoCreatedId = userName,
			WhoCreatedLogon = userName,
			WhoModifiedId = userName,
			WhoModifiedLogon = userName
		};
	}

	/// <summary>
	/// Gets the type of the file.
	/// </summary>
	/// <param name="fileType">Type of the file.</param>
	/// <returns>True the file type is known type; false otherwise.</returns>
	private static bool IsKnownFileType(ushort fileType)
	{
		switch ((FileType)fileType)
		{
		case FileType.ExperimentMethod:
		case FileType.SampleList:
		case FileType.ProcessingMethod:
		case FileType.RawFile:
		case FileType.TuneMethod:
		case FileType.ResultsFile:
		case FileType.QuanFile:
		case FileType.CalibrationFile:
		case FileType.MethodFile:
		case FileType.XqnFile:
		case FileType.LayoutFile:
		case FileType.MethodEditorLayout:
		case FileType.SampleListEditorLayout:
		case FileType.ProcessingMethodEditLayout:
		case FileType.QualBrowserLayout:
		case FileType.TuneLayout:
		case FileType.ResultsLayout:
			return true;
		default:
			return false;
		}
	}

	/// <summary>
	/// The is signature valid.
	/// </summary>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public bool IsSignatureValid()
	{
		return _header.FinnSig == "Finnigan";
	}

	/// <summary>
	/// Calculates and updates the checksum for the file header.  Will calculate using the header written to the stream of the binary writer.  
	/// Requires the header to be previously written to the binary writer stream.
	/// </summary>
	/// <param name="writer">Binary writer with the written file header</param>
	/// <param name="errors">Any errors that occur during calculation</param>
	/// <returns>True if successful</returns>
	public bool UpdateFileHeaderCheckSum(BinaryWriter writer, DeviceErrors errors)
	{
		try
		{
			long length = writer.BaseStream.Length;
			uint checkSumSeed = GetCheckSumSeed();
			int num = Marshal.SizeOf(typeof(FileHeaderStruct));
			long num2 = ((length <= 10485760) ? length : 10485760) - num;
			byte[] array = new byte[num2];
			writer.BaseStream.Position = num;
			writer.BaseStream.Read(array, 0, (int)num2);
			Adler32 adler = new Adler32();
			adler.Calc(checkSumSeed, array);
			_header.CheckSum = adler.Checksum;
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Updates the file header checksum.
	/// </summary>
	/// <param name="rootStorage">The root storage.</param>
	/// <param name="errors">Stores the last error information.</param>
	/// <returns>True if successful; otherwise false.</returns>
	public bool UpdateFileHeaderChecksum(CFStorage rootStorage, DeviceErrors errors)
	{
		if (rootStorage.CalcChecksum(out var checksum, out var errorMessage))
		{
			_header.CheckSum = checksum;
			return true;
		}
		errors.UpdateError(errorMessage);
		return false;
	}

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		_header = viewer.ReadStructureExt<FileHeaderStruct>(ref startPos);
		Initialize();
		return startPos - dataOffset;
	}

	/// <summary>
	/// Initialize fields and properties, on load.
	/// </summary>
	private void Initialize()
	{
		_creator = new UserIdStamp(_header.Created);
		_modifier = new UserIdStamp(_header.Changed);
		if (IsKnownFileType(_header.FileType))
		{
			_fileType = (FileType)_header.FileType;
		}
		else
		{
			_fileType = FileType.NotSupported;
		}
	}

	/// <summary>
	/// Gets the check sum seed.
	/// </summary>
	/// <returns>Check sum seed value.</returns>
	private uint GetCheckSumSeed()
	{
		if (_header.FileRev < 57)
		{
			return 0u;
		}
		byte[] data = WriterHelper.StructToByteArray(FileHeaderStruct, Marshal.SizeOf(FileHeaderStruct));
		Adler32 adler = new Adler32();
		adler.CalcFileHeader(data);
		return adler.Checksum;
	}

	/// <summary>
	/// Determines whether this is a valid revision.
	/// </summary>
	/// <returns>True it's a valid file revision; false otherwise.</returns>
	public bool IsValidRevision()
	{
		int revision = Revision;
		if (!IsSignatureValid() || revision == 0 || revision == 25 || ((FileType.LayoutFile & FileType) > FileType.NotSupported && revision < 12) || (FileType == FileType.QuanFile && revision > 25 && revision < 34))
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Test if this may be a valid file but with a newer format.
	/// </summary>
	/// <returns>true: if the file version &gt; the version complied into of the software that's running.</returns>
	public bool IsNewerRevision()
	{
		return Revision > 66;
	}

	/// <summary>
	/// The resets the checksum in the header to 0.
	/// </summary>
	public void ResetChecksum()
	{
		_header.CheckSum = 0u;
	}
}
