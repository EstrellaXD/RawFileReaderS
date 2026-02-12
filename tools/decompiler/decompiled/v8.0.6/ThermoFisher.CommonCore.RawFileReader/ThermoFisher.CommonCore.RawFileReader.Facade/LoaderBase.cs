using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.Facade;

/// <summary>
/// Class for various file loaders, with common
/// file header format
/// </summary>
internal class LoaderBase : DeviceErrors
{
	private static readonly int FileHeaderSize = Marshal.SizeOf(typeof(FileHeaderStruct));

	/// <summary>
	/// Gets or sets the file header.
	/// </summary>
	public ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader Header { get; protected set; }

	/// <summary>
	/// Check for valid version of the file
	/// </summary>
	/// <param name="fileType">
	/// The file Type.
	/// </param>
	/// <returns>
	/// The version number
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// If version is not valid
	/// </exception>
	protected int CheckForValidVersion(string fileType)
	{
		int revision = Header.Revision;
		if (!Header.IsValidRevision())
		{
			AppendError($"The file is not a recognized format! The file is either not a Thermo Fisher {fileType} file or the version is {revision}.");
			throw new Exception(base.ErrorMessage);
		}
		if (Header.IsNewerRevision())
		{
			AppendError($"The file the version {revision} is newer than this application can decode. An upgarded application may be needed");
			throw new NewerFileFormatException(base.ErrorMessage);
		}
		return revision;
	}

	/// <summary>
	/// Test that the raw file checksum is valid
	/// </summary>
	/// <param name="viewer">Bytes of the raw file</param>
	/// <returns>True on passing test</returns>
	protected bool ValidCrc(IMemoryReader viewer)
	{
		if (Header.Revision < 50)
		{
			return true;
		}
		if (Header.CheckSum == 0)
		{
			return true;
		}
		long length = viewer.Length;
		byte[] data = viewer.ReadBytes(0L, FileHeaderSize);
		Adler32 adler = new Adler32();
		adler.CalcFileHeader(data);
		long num = ((length <= 10485760) ? length : 10485760) - FileHeaderSize;
		uint checksum = adler.Checksum;
		long offset = FileHeaderSize;
		byte[] data2;
		if (num > 500000 && !viewer.PreferLargeReads)
		{
			data2 = new byte[num];
			long chunks = num / 500000;
			long num2 = chunks * 500000;
			long finalChunkSize;
			if (num2 < num)
			{
				chunks++;
				finalChunkSize = num - num2;
			}
			else
			{
				finalChunkSize = 500000L;
			}
			Parallel.For(0L, chunks, delegate(long chunk)
			{
				long num3 = chunk * 500000;
				long startPos = num3 + offset;
				long num4 = ((chunk >= chunks - 1) ? finalChunkSize : 500000);
				Array.Copy(viewer.ReadLargeData(ref startPos, (int)num4), 0L, data2, num3, num4);
			});
		}
		else
		{
			data2 = viewer.ReadLargeData(ref offset, (int)num);
		}
		adler.Calc(checksum, data2);
		return adler.Checksum == Header.CheckSum;
	}
}
