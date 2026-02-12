namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Calculates checksum (also called a CRC) using Adler-32 algorithm.
/// </summary>
internal class Adler32
{
	private const uint Base = 65521u;

	private const int Nmax = 5552;

	private const int CrcStartAddress = 148;

	private const int CrcEndAddress = 151;

	private uint _checksum;

	/// <summary>
	/// Gets the checksum of all added items
	/// </summary>
	public uint Checksum => _checksum;

	/// <summary>
	/// Calculate the CRC for an array
	/// </summary>
	/// <param name="seed">
	/// Initial checksum value (often this object.Checksum)
	/// </param>
	/// <param name="data">
	/// Data to check
	/// </param>
	public void Calc(uint seed, byte[] data)
	{
		_checksum = CalcAdler32(seed, data);
	}

	/// <summary>
	/// Calculate the CRC for an file header array.
	/// This sets the value of "CRC" to zero first in the supplied data
	/// </summary>
	/// <param name="data">Bytes copied from the file header</param>
	public void CalcFileHeader(byte[] data)
	{
		if (data.Length > 151)
		{
			for (int i = 148; i <= 151; i++)
			{
				data[i] = 0;
			}
			_checksum = CalcAdler32(0u, data);
		}
	}

	/// <summary>
	/// Calculate CRC of a block of data
	/// </summary>
	/// <param name="seed">CRC of previous blocks, 0 if first block</param>
	/// <param name="data">Data to check</param>
	/// <returns>CRC of this block</returns>
	private uint CalcAdler32(uint seed, byte[] data)
	{
		long num = data.Length;
		uint num2 = seed & 0xFFFF;
		uint num3 = (seed >> 16) & 0xFFFF;
		int num4 = 0;
		while (num > 0)
		{
			int num5 = (int)((num < 5552) ? num : 5552);
			num -= num5;
			while (num5 >= 8)
			{
				num3 += (num2 += data[num4]);
				num3 += (num2 += data[num4 + 1]);
				num3 += (num2 += data[num4 + 2]);
				num3 += (num2 += data[num4 + 3]);
				num3 += (num2 += data[num4 + 4]);
				num3 += (num2 += data[num4 + 5]);
				num3 += (num2 += data[num4 + 6]);
				num3 += (num2 += data[num4 + 7]);
				num4 += 8;
				num5 -= 8;
			}
			if (num5 != 0)
			{
				do
				{
					num2 += data[num4++];
					num3 += num2;
				}
				while (--num5 > 0);
			}
			num2 %= 65521;
			num3 %= 65521;
		}
		return (num3 << 16) | num2;
	}
}
