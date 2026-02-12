using System;
using System.Threading;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Defines the BufferInfo type for backward compatibility with FileIO.<para />
/// Ex. HomePage real time plot checks the Number Element field in scan index buffer info,<para />
/// to determine whether the scans data is available for display and <para />
/// raw file stitching is also using it. 
/// </summary>
internal class BufferInfo : IDisposable
{
	private readonly IReadWriteAccessor _memMapAccessor;

	private readonly DeviceErrors _errors;

	private readonly int _bufInfoStructSize = Utilities.StructSizeLookup.Value[16];

	private BufferInfoStruct _bufferInfo;

	private bool _disposed;

	/// <summary>
	/// Gets a value indicating whether this instance has error.
	/// </summary>
	/// <value>
	///   <c>true</c> if this instance has error; otherwise, <c>false</c>.
	/// </value>
	public bool HasError => _errors.HasError;

	/// <summary>
	/// Gets the error message.
	/// </summary>
	/// <value>
	/// The error message.
	/// </value>
	public string ErrorMessage => _errors.ErrorMessage;

	/// <summary>
	/// Gets the error code.
	/// </summary>
	/// <value>
	/// The error code.
	/// </value>
	public int ErrorCode => _errors.ErrorCode;

	/// <summary>
	/// Gets the size of the data written to the buffer.
	/// </summary>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	public int Size => _bufferInfo.Size;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.BufferInfo" /> class.
	/// </summary>
	/// <param name="deviceId">The device identifier.</param>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="objectName">Name of the object.</param>
	/// <param name="creatable">True open an existing shared memory map otherwise create. False only open an existing shared memory map file.</param>        
	/// <param name="elementSize">Size of the element.</param>
	/// <param name="mask">The mask.</param>
	public BufferInfo(Guid deviceId, string fileName, string objectName, bool creatable, int elementSize = 0, string mask = "FMAT_READWRITE_INFO")
	{
		string mapName = fileName + objectName + mask;
		_errors = new DeviceErrors();
		_memMapAccessor = SharedMemHelper.CreateSharedBufferAccessor(deviceId, mapName, _bufInfoStructSize, creatable, _errors);
		if (_memMapAccessor != null)
		{
			IncrementReferenceCount();
			if (!creatable)
			{
				Refresh();
				return;
			}
			_bufferInfo.Size = elementSize;
			_memMapAccessor.WriteStruct(0L, _bufferInfo, _bufInfoStructSize);
		}
	}

	/// <summary>
	/// Determines whether this instance has reference.
	/// </summary>
	/// <returns>True if there's other objects referencing it, otherwise False.</returns>
	public bool HasReference()
	{
		if (!_errors.HasError && _memMapAccessor != null)
		{
			int num = _memMapAccessor.ReadInt(40L);
			_bufferInfo.ReferenceCount = num;
			return num > 0;
		}
		return false;
	}

	/// <summary>
	/// Increment the number of elements by 1 and then updates to the shared memory map.
	/// </summary>
	public void IncrementNumElements()
	{
		try
		{
			_memMapAccessor.WriteInt(0L, Interlocked.Increment(ref _bufferInfo.NumElements));
		}
		catch (Exception ex)
		{
			_errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Increments the reference count and update.
	/// </summary>
	private void IncrementReferenceCount()
	{
		try
		{
			_bufferInfo.ReferenceCount = _memMapAccessor.ReadInt(40L);
			_memMapAccessor.WriteInt(40L, Interlocked.Increment(ref _bufferInfo.ReferenceCount));
		}
		catch (Exception ex)
		{
			_errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Decrements the reference count and update.
	/// </summary>
	public void DecrementReferenceCount()
	{
		try
		{
			_bufferInfo.ReferenceCount = _memMapAccessor.ReadInt(40L);
			_memMapAccessor.WriteInt(40L, Interlocked.Decrement(ref _bufferInfo.ReferenceCount));
		}
		catch (Exception ex)
		{
			_errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Set the size of a data element, i.e. Status Log Header, this will be the total size of the header items in bytes.
	/// </summary>
	/// <param name="dataBlockSize"> The data Block Size. </param>
	public void SetDataBlockSize(int dataBlockSize)
	{
		try
		{
			Interlocked.Exchange(ref _bufferInfo.Size, dataBlockSize);
			_memMapAccessor.WriteInt(24L, _bufferInfo.Size);
		}
		catch (Exception ex)
		{
			_errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			IViewCollectionManager instance = MemoryMappedRawFileManager.Instance;
			_memMapAccessor.ReleaseAndCloseMemoryMappedFile(instance);
		}
	}

	/// <summary>
	/// Refresh local data from the memory maps.
	/// </summary>
	/// <returns>
	/// True if OK
	/// </returns>
	public bool Refresh()
	{
		if (_memMapAccessor != null)
		{
			_bufferInfo = _memMapAccessor.ReadStructure<BufferInfoStruct>(0L, _bufInfoStructSize);
			return true;
		}
		return false;
	}
}
