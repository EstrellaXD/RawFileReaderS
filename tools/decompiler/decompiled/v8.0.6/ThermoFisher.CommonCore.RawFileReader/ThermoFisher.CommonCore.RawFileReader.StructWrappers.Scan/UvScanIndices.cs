using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
/// Provides methods to read UV scan indices from raw file.
/// UV scan index serves as the key to retrieve the scan data packet.
/// </summary>
internal class UvScanIndices : IRawObjectBase, IDisposable, IRecordRangeProvider
{
	private RecordBufferManager _bufferManager;

	private bool _usesRecordBuffer;

	private readonly int _numSpectra;

	private IReadWriteAccessor _indexViewer;

	private int _sizeOfUvScanIndexStruct;

	private Func<byte[], UvScanIndexStruct> _decoder;

	private bool _disposed;

	public IViewCollectionManager Manager { get; }

	/// <summary>
	/// Gets the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex" /> at the specified index.
	/// </summary>
	/// <param name="index">The index.</param>
	/// <returns>UV scan index</returns>
	public UvScanIndex this[int index]
	{
		get
		{
			int num = index * _sizeOfUvScanIndexStruct;
			if (_usesRecordBuffer)
			{
				return new UvScanIndex(_bufferManager.FindReader(index), _sizeOfUvScanIndexStruct, num, _decoder);
			}
			return new UvScanIndex(_indexViewer, _sizeOfUvScanIndexStruct, num, _decoder);
		}
	}

	/// <summary>
	/// Gets the number of spectrum.
	/// </summary>
	public int Count => _numSpectra;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndices" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="size">The size.</param>
	public UvScanIndices(IViewCollectionManager manager, int size)
	{
		Manager = manager;
		_numSpectra = size;
	}

	/// <summary>
	/// Load UV scan indices from raw file.
	/// </summary>
	/// <param name="viewer">The viewer (memory map into file).</param>
	/// <param name="dataOffset">The data offset (into the memory map).</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		_sizeOfUvScanIndexStruct = GetSizeOfUvScanIndexStructByFileVersion(fileRevision);
		_decoder = GetDecoder(fileRevision);
		int num = _numSpectra * _sizeOfUvScanIndexStruct;
		_indexViewer = null;
		if (viewer is MemoryArrayReader { SupportsSubViews: not false } memoryArrayReader)
		{
			_indexViewer = memoryArrayReader.CreateSubView(dataOffset, num);
		}
		if (_indexViewer == null && num <= 2097152 && viewer.SupportsSubViews)
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
	/// Gets the size of UV scan index structure by file version.
	/// </summary>
	/// <param name="fileVersion">The file version.</param>
	/// <returns>Struct size specified by the file version</returns>
	private int GetSizeOfUvScanIndexStructByFileVersion(int fileVersion)
	{
		if (fileVersion >= 64)
		{
			return Marshal.SizeOf(typeof(UvScanIndexStruct));
		}
		return Marshal.SizeOf(typeof(UvScanIndexStructOld));
	}

	/// <summary>
	/// Gets the decoder.
	/// </summary>
	/// <param name="fileVersion">The file version.</param>
	/// <returns>A method that will reads the UV scan index struct from a byte array and convert it back to struct. </returns>
	private Func<byte[], UvScanIndexStruct> GetDecoder(int fileVersion)
	{
		if (fileVersion >= 64)
		{
			return ReadUvScanIndexStruct;
		}
		return ReadUvScanIndexStructOld;
	}

	/// <summary>
	/// Reads the UV scan index structure old from a byte array and 
	/// convert it back to struct.
	/// </summary>
	/// <param name="bytes">UV Scan index struct in byte array.</param>
	/// <returns>UV scan index struct.</returns>
	private UvScanIndexStruct ReadUvScanIndexStruct(byte[] bytes)
	{
		return new UvScanIndexStruct
		{
			DataOffset32Bit = BitConverter.ToUInt32(bytes, 0),
			ScanNumber = BitConverter.ToInt32(bytes, 4),
			PacketType = BitConverter.ToInt32(bytes, 8),
			NumberPackets = BitConverter.ToInt32(bytes, 12),
			NumberOfChannels = BitConverter.ToInt32(bytes, 16),
			UniformTime = BitConverter.ToInt32(bytes, 20),
			Frequency = BitConverter.ToDouble(bytes, 24),
			StartTime = BitConverter.ToDouble(bytes, 32),
			ShortWavelength = BitConverter.ToDouble(bytes, 40),
			LongWavelength = BitConverter.ToDouble(bytes, 48),
			TIC = BitConverter.ToDouble(bytes, 56),
			DataOffset = BitConverter.ToInt64(bytes, 64)
		};
	}

	/// <summary>
	/// Reads the UV scan index structure old from a byte array and 
	/// convert it back to struct.
	/// </summary>
	/// <param name="bytes">UV Scan index struct in byte array.</param>
	/// <returns>In struct format</returns>
	private UvScanIndexStruct ReadUvScanIndexStructOld(byte[] bytes)
	{
		UvScanIndexStruct result = new UvScanIndexStruct
		{
			DataOffset32Bit = BitConverter.ToUInt32(bytes, 0),
			ScanNumber = BitConverter.ToInt32(bytes, 4),
			PacketType = BitConverter.ToInt32(bytes, 8),
			NumberPackets = BitConverter.ToInt32(bytes, 12),
			NumberOfChannels = BitConverter.ToInt32(bytes, 16),
			UniformTime = BitConverter.ToInt32(bytes, 20),
			Frequency = BitConverter.ToDouble(bytes, 24),
			StartTime = BitConverter.ToDouble(bytes, 32),
			ShortWavelength = BitConverter.ToDouble(bytes, 40),
			LongWavelength = BitConverter.ToDouble(bytes, 48),
			TIC = BitConverter.ToDouble(bytes, 56)
		};
		result.DataOffset = result.DataOffset32Bit;
		return result;
	}

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_indexViewer.ReleaseAndCloseMemoryMappedFile(Manager);
		}
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
		int num = lowRecord * _sizeOfUvScanIndexStruct;
		return new MemoryArrayReader(_indexViewer.ReadBytes(num, (highRecord - lowRecord + 1) * _sizeOfUvScanIndexStruct), num);
	}
}
