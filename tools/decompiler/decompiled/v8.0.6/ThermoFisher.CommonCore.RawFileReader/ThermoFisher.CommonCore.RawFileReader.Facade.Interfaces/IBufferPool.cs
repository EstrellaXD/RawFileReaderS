namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// Interface to rent/release memory to a pool.
/// Rented memory will have an array length which is at least the requested size.
/// After a buffer has been released, calling code cannot depend on the contents of the array
/// and must not modify the data.
/// </summary>
public interface IBufferPool
{
	/// <summary>
	/// Allocates (rents) memory
	/// </summary>
	/// <param name="count">requested size. The returned array may be larger than this</param>
	/// <returns>an array of bytes, with a length which is at least the requested count</returns>
	byte[] Rent(int count);

	/// <summary>
	/// Releases a buffer which has been rented.
	/// </summary>
	/// <param name="rented">the allocated buffer</param>
	void Release(byte[] rented);
}
