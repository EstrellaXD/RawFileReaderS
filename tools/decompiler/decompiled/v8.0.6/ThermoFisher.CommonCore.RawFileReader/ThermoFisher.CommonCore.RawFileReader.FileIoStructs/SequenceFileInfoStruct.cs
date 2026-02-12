using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

/// <summary>
/// The sequence file info struct.
/// Implements the public interface ISequenceInfo against
/// an SLD file.
/// </summary>
internal class SequenceFileInfoStruct : IRawObjectBase, ISequenceInfo
{
	/// <summary>
	/// The processing mode, of an Xcalibur sequence.
	/// </summary>
	internal enum ProcessingMode
	{
		/// <summary>
		/// This sequence designed to run with Xcalibur
		/// </summary>
		XcaliburProcessing,
		/// <summary>
		/// This sequence designed to run with Target.
		/// Value, for legacy import only
		/// </summary>
		TargetProcessing
	}

	/// <summary>
	/// Sequence file information: LCQ version
	/// </summary>
	private struct SequenceInfoLcq
	{
		private readonly int numRows;

		private readonly BracketType bracketType;

		private readonly int colConfig;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
		private readonly short[] columnWidth;
	}

	/// <summary>
	/// Sequence file information: Original Xcalibur version
	/// </summary>
	private struct SequenceInfo1
	{
		private readonly int numRows;

		private readonly BracketType bracketType;

		private readonly int colConfig;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
		private readonly short[] columnWidth;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
		private readonly short[] typeToColumnPosition;
	}

	/// <summary>
	/// Sequence file information: Current Xcalibur version
	/// </summary>
	internal struct SequenceInfo
	{
		internal int NumRows;

		internal BracketType BracketType;

		internal int ColConfig;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
		internal short[] ColumnWidth;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)]
		internal short[] TypeToColumnPosition;

		internal ProcessingMode ProcessingMode;
	}

	private const int MaxColumnWidthsLcq = 20;

	private const int MaxColumnWidthsXcal = 21;

	private const int MaxUserLabels = 5;

	private const int NumExtraUserCols = 15;

	private SequenceInfo _info;

	private readonly string[] _userPrivateLabel;

	/// <summary>
	/// Gets the user private label.
	/// </summary>
	public string[] UserPrivateLabel => _userPrivateLabel;

	/// <summary>
	/// Gets or sets a description of the auto sampler tray
	/// </summary>
	public string TrayConfiguration { get; internal set; }

	/// <summary>
	/// Gets or sets the sequence bracket type.
	/// This determines which groups of samples use the same calibration curve.
	/// </summary>
	public BracketType Bracket
	{
		get
		{
			return _info.BracketType;
		}
		internal set
		{
			_info.BracketType = value;
		}
	}

	/// <summary>
	/// Gets the user configurable column names
	/// </summary>
	public string[] UserLabel { get; private set; }

	/// <summary>
	/// Gets the display width of each sequence column
	/// </summary>
	public short[] ColumnWidth => _info.ColumnWidth;

	/// <summary>
	/// Gets the column order (see home page?)
	/// </summary>
	public short[] TypeToColumnPosition => _info.TypeToColumnPosition;

	/// <summary>
	/// Gets or sets the number of samples.
	/// </summary>
	internal int NumRows
	{
		get
		{
			return _info.NumRows;
		}
		set
		{
			_info.NumRows = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.SequenceFileInfoStruct" /> class.
	/// </summary>
	public SequenceFileInfoStruct()
	{
		UserLabel = new string[5];
		_userPrivateLabel = new string[15];
	}

	/// <summary>
	/// Load the sequence information from the current file
	/// </summary>
	/// <param name="viewer">View into memory mapped file</param>
	/// <param name="dataOffset">offset for this object</param>
	/// <param name="fileRevision">file version</param>
	/// <returns>The number of bytes read</returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		if (fileRevision < 25)
		{
			_info = viewer.ReadPreviousRevisionAndConvertExt<SequenceInfo, SequenceInfoLcq>(ref startPos);
		}
		else if (fileRevision < 42)
		{
			_info = viewer.ReadPreviousRevisionAndConvertExt<SequenceInfo, SequenceInfo1>(ref startPos);
		}
		else
		{
			_info = viewer.ReadStructureExt<SequenceInfo>(ref startPos);
		}
		if (fileRevision < 25)
		{
			for (int num = 20; num > 5; num--)
			{
				_info.ColumnWidth[num] = _info.ColumnWidth[num - 1];
			}
			for (short num2 = 0; num2 < 21; num2++)
			{
				_info.TypeToColumnPosition[num2] = num2;
			}
		}
		if (Bracket != BracketType.None && _info.ColumnWidth[6] > 0)
		{
			_info.ColumnWidth[6] = (short)(-_info.ColumnWidth[6]);
		}
		for (int i = 0; i < 5; i++)
		{
			UserLabel[i] = viewer.ReadString(startPos, out var numOfBytesRead);
			startPos += numOfBytesRead;
		}
		if (fileRevision < 25)
		{
			TrayConfiguration = string.Empty;
		}
		else
		{
			TrayConfiguration = viewer.ReadString(startPos, out var numOfBytesRead2);
			startPos += numOfBytesRead2;
		}
		if (fileRevision < 42)
		{
			_info.ProcessingMode = ProcessingMode.XcaliburProcessing;
		}
		if (fileRevision >= 58)
		{
			for (int j = 0; j < 15; j++)
			{
				_userPrivateLabel[j] = viewer.ReadString(startPos, out var numOfBytesRead3);
				startPos += numOfBytesRead3;
			}
		}
		else
		{
			for (int k = 0; k < 15; k++)
			{
				_userPrivateLabel[k] = string.Empty;
			}
		}
		return startPos - dataOffset;
	}

	/// <summary>
	/// Initializes this instance object with default values.
	/// </summary>
	internal void Initialization()
	{
		_info.ProcessingMode = ProcessingMode.XcaliburProcessing;
		_info.BracketType = BracketType.Open;
		_info.ColumnWidth = new short[21];
		_info.TypeToColumnPosition = new short[21];
		for (int i = 0; i < 21; i++)
		{
			_info.ColumnWidth[i] = 80;
			_info.TypeToColumnPosition[i] = (short)i;
		}
		if (Bracket != BracketType.None && _info.ColumnWidth[6] > 0)
		{
			_info.ColumnWidth[6] = (short)(-_info.ColumnWidth[6]);
		}
		for (int j = 0; j < 15; j++)
		{
			_userPrivateLabel[j] = string.Empty;
		}
		for (int k = 0; k < 5; k++)
		{
			UserLabel[k] = string.Empty;
		}
	}

	/// <summary>
	/// Gets the sequence information.
	/// Convert the sequence info struct to byte array.
	/// </summary>
	/// <returns>Byte array of the sequence info structure</returns>
	internal byte[] GetSequenceInfo()
	{
		return WriterHelper.StructToByteArray(_info, Utilities.StructSizeLookup.Value[17]);
	}
}
