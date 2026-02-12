using System.IO;
using System.Text;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Wrapper class for using IOleStream.
/// For additional read/write examples see version in "foundation Apps" project.
/// </summary>
internal class StreamIo
{
	private readonly Stream _stream;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StreamIo" /> class.
	/// </summary>
	/// <param name="stream">
	/// The stream.
	/// </param>
	internal StreamIo(Stream stream)
	{
		_stream = stream;
	}

	/// <summary>
	/// read char array from stream.
	/// The stream is "wide characters" (unicode)
	/// </summary>
	/// <param name="bytesLength"> The number of bytes to read from stream.   </param>
	/// <returns> The stream of bytes into a string  </returns>
	internal string ReadCharArray(int bytesLength)
	{
		if (bytesLength > 0)
		{
			byte[] array = new byte[bytesLength];
			Read(array);
			return Encoding.Unicode.GetString(array, 0, bytesLength).Trim('\0');
		}
		return string.Empty;
	}

	/// <summary>
	/// read an array from the stream.
	/// </summary>
	/// <param name="data">
	/// The data.
	/// </param>
	/// <exception cref="T:System.IO.IOException">Thrown when not able to read data
	/// </exception>
	internal void Read(byte[] data)
	{
		int num = ((data != null) ? data.Length : 0);
		if (num == 0 || _stream.Read(data, 0, num) == num)
		{
			return;
		}
		throw new IOException("Unable to read from stream");
	}
}
