using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
/// Provides methods to read scan indices from raw file.
/// Scan index serves as the key to retrieve the scan data packet.
/// </summary>
internal class ScanIndices : IRawObjectBase, IDisposable, IRecordRangeProvider
{
	private RecordBufferManager _bufferManager;

	private bool _usesRecordBuffer;

	private const int StartTimeOffset = 24;

	private readonly int _numSpectra;

	private IReadWriteAccessor _indexViewer;

	private int _sizeOfScanIndexStruct;

	private Func<byte[], ScanIndexStruct> _decoder;

	private bool _disposed;

	public IViewCollectionManager Manager { get; }

	/// <summary>
	/// Gets the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanIndex" /> at the specified index.
	/// </summary>
	/// <value>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanIndex" />.
	/// </value>
	/// <param name="index">The index.</param>
	/// <returns>Scan index</returns>
	public ScanIndex this[int index]
	{
		get
		{
			int num = index * _sizeOfScanIndexStruct;
			if (_usesRecordBuffer)
			{
				return new ScanIndex(_bufferManager.FindReader(index), _sizeOfScanIndexStruct, num, _decoder);
			}
			return new ScanIndex(_indexViewer, _sizeOfScanIndexStruct, num, _decoder);
		}
	}

	/// <summary>
	/// Gets the number of spectrum.
	/// </summary>
	public int Count => _numSpectra;

	public bool HasRecordBuffer => _usesRecordBuffer;

	public int RecordsPerBatch => _bufferManager?.RecordsPerBatch ?? 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.ScanIndices" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="size">The number of spectrum.</param>
	public ScanIndices(IViewCollectionManager manager, int size)
	{
		Manager = manager;
		_numSpectra = size;
	}

	/// <summary>
	/// Gets the retention time from the index, with minimal decoding
	/// </summary>
	/// <param name="index"></param>
	/// <returns>The retention time</returns>
	public double GetRetentionTime(int index)
	{
		int num = index * _sizeOfScanIndexStruct + 24;
		return _indexViewer.ReadDouble(num);
	}

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">The viewer (access to bytes in the file or memory).</param>
	/// <param name="dataOffset">The data offset (into the memory).</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		_sizeOfScanIndexStruct = GetSizeOfScanIndexStructByFileVersion(fileRevision);
		_decoder = GetDecoder(fileRevision);
		int num = _numSpectra * _sizeOfScanIndexStruct;
		_indexViewer = null;
		if (viewer is MemoryArrayReader { SupportsSubViews: not false } memoryArrayReader)
		{
			_indexViewer = memoryArrayReader.CreateSubView(dataOffset, num);
		}
		if (_indexViewer == null && num <= 4194304 && viewer.SupportsSubViews)
		{
			_indexViewer = viewer.CreateSubView(dataOffset, num);
		}
		if (_indexViewer == null)
		{
			_indexViewer = Manager.GetRandomAccessViewer(Guid.Empty, viewer.StreamId, viewer.InitialOffset + dataOffset, num, inAcquisition: false);
			if (_indexViewer.PreferLargeReads)
			{
				_usesRecordBuffer = true;
				_bufferManager = new RecordBufferManager(_indexViewer, num, 0, _numSpectra - 1, this, zeroBased: true);
			}
		}
		return num;
	}

	/// <summary>
	/// Create a reader to access a large block of index records
	/// using just 1 read from the initial view, for better efficiency
	/// </summary>
	/// <param name="lowRecord">first record needed</param>
	/// <param name="highRecord">last record needed</param>
	/// <returns>A memory reader to decode the selected records</returns>
	public IMemoryReader CreateSubRangeReader(int lowRecord, int highRecord)
	{
		int num = lowRecord * _sizeOfScanIndexStruct;
		return new MemoryArrayReader(_indexViewer.ReadBytes(num, (highRecord - lowRecord + 1) * _sizeOfScanIndexStruct), num);
	}

	/// <summary>
	/// Gets the size of scan index structure by file version.
	/// </summary>
	/// <param name="fileVersion">The file version.</param>
	/// <returns>Struct size specified by the file version.</returns>
	private int GetSizeOfScanIndexStructByFileVersion(int fileVersion)
	{
		if (fileVersion >= 65)
		{
			return Marshal.SizeOf(typeof(ScanIndexStruct));
		}
		if (fileVersion >= 64)
		{
			return Marshal.SizeOf(typeof(ScanIndexStruct2));
		}
		return Marshal.SizeOf(typeof(ScanIndexStruct1));
	}

	/// <summary>
	/// Gets the decoder.
	/// </summary>
	/// <param name="fileVersion">The file version.</param>
	/// <returns>A method that will reads the scan index struct from a byte array and convert it back to struct.</returns>
	private Func<byte[], ScanIndexStruct> GetDecoder(int fileVersion)
	{
		if (fileVersion >= 65)
		{
			return ReadScanIndexStruct;
		}
		if (fileVersion >= 64)
		{
			return ReadScanIndexStruct2;
		}
		return ReadScanIndexStruct1;
	}

	/// <summary>
	/// Reads the scan index structure from a byte array and convert it to struct.
	/// </summary>
	/// <param name="bytes">Scan index in byte array.</param>
	/// <returns>Scan index struct.</returns>
	private ScanIndexStruct ReadScanIndexStruct(byte[] bytes)
	{
		return new ScanIndexStruct
		{
			DataSize = BitConverter.ToUInt32(bytes, 0),
			TrailerOffset = BitConverter.ToInt32(bytes, 4),
			ScanTypeIndex = BitConverter.ToInt32(bytes, 8),
			ScanNumber = BitConverter.ToInt32(bytes, 12),
			PacketType = BitConverter.ToUInt32(bytes, 16),
			NumberPackets = BitConverter.ToInt32(bytes, 20),
			StartTime = BitConverter.ToDouble(bytes, 24),
			TIC = BitConverter.ToDouble(bytes, 32),
			BasePeakIntensity = BitConverter.ToDouble(bytes, 40),
			BasePeakMass = BitConverter.ToDouble(bytes, 48),
			LowMass = BitConverter.ToDouble(bytes, 56),
			HighMass = BitConverter.ToDouble(bytes, 64),
			DataOffset = BitConverter.ToInt64(bytes, 72),
			CycleNumber = BitConverter.ToInt32(bytes, 80)
		};
	}

	/// <summary>
	/// Reads the scan index struct2 from a byte array and convert it to struct.
	/// </summary>
	/// <param name="bytes">Scan index in byte array.</param>
	/// <returns>Scan index struct.</returns>
	private ScanIndexStruct ReadScanIndexStruct2(byte[] bytes)
	{
		return new ScanIndexStruct
		{
			TrailerOffset = BitConverter.ToInt32(bytes, 4),
			ScanTypeIndex = BitConverter.ToInt32(bytes, 8),
			ScanNumber = BitConverter.ToInt32(bytes, 12),
			PacketType = BitConverter.ToUInt32(bytes, 16),
			NumberPackets = BitConverter.ToInt32(bytes, 20),
			StartTime = BitConverter.ToDouble(bytes, 24),
			TIC = BitConverter.ToDouble(bytes, 32),
			BasePeakIntensity = BitConverter.ToDouble(bytes, 40),
			BasePeakMass = BitConverter.ToDouble(bytes, 48),
			LowMass = BitConverter.ToDouble(bytes, 56),
			HighMass = BitConverter.ToDouble(bytes, 64),
			DataOffset = BitConverter.ToInt64(bytes, 72),
			DataSize = 0u
		};
	}

	/// <summary>
	/// Reads the scan index struct1 from a byte array and convert it to struct.
	/// </summary>
	/// <param name="bytes">Scan index in byte array.</param>
	/// <returns>Scan index struct.</returns>
	private ScanIndexStruct ReadScanIndexStruct1(byte[] bytes)
	{
		ScanIndexStruct result = new ScanIndexStruct
		{
			DataOffset = BitConverter.ToUInt32(bytes, 0),
			TrailerOffset = BitConverter.ToInt32(bytes, 4),
			ScanTypeIndex = BitConverter.ToInt32(bytes, 8),
			ScanNumber = BitConverter.ToInt32(bytes, 12),
			PacketType = BitConverter.ToUInt32(bytes, 16),
			NumberPackets = BitConverter.ToInt32(bytes, 20),
			StartTime = BitConverter.ToDouble(bytes, 24),
			TIC = BitConverter.ToDouble(bytes, 32),
			BasePeakIntensity = BitConverter.ToDouble(bytes, 40),
			BasePeakMass = BitConverter.ToDouble(bytes, 48),
			LowMass = BitConverter.ToDouble(bytes, 56),
			HighMass = BitConverter.ToDouble(bytes, 64)
		};
		result.DataSize = 0u;
		return result;
	}

	/// <summary>
	/// Releases unmanaged and - optionally - managed resources.
	/// </summary>
	/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
	private void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				_indexViewer.ReleaseAndCloseMemoryMappedFile(Manager);
			}
			_disposed = true;
		}
	}

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}

	/// <summary>
	/// Forger any cached buffers so that they cab be garbage collected
	/// </summary>
	public void ClearCaches()
	{
		_bufferManager?.Clean();
	}
}
