using System;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.UVPackets;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides extension methods for Adjustable Scan Rate Profile index data
/// </summary>
internal static class AsrProfileIndexExtension
{
	private static readonly int AsrProfileIndexStructSize = Utilities.StructSizeLookup.Value[8];

	/// <summary>
	/// Convert PDA Profiles index data to byte array.
	/// </summary>
	/// <param name="pdaScanIndex">The packets.</param>
	/// <param name="numData">The number of scan data</param>
	/// <returns>Byte array of ASRProfileIndexStruct</returns>
	public static byte[] AsrProfileIndexDataPktsToByteArray(this IPdaScanIndex pdaScanIndex, int numData)
	{
		byte[] array = new byte[AsrProfileIndexStructSize];
		Buffer.BlockCopy(WriterHelper.StructToByteArray(new AsrProfileIndexStruct
		{
			IsValidScan = 1,
			WavelengthStart = (uint)pdaScanIndex.ShortWavelength,
			WavelengthEnd = (uint)pdaScanIndex.LongWavelength,
			WavelengthStep = 0u,
			TimeWavelengthStart = pdaScanIndex.ShortWavelength,
			TimeWavelengthEnd = pdaScanIndex.LongWavelength,
			TimeWavelengthStep = pdaScanIndex.WavelengthStep,
			TimeWavelengthExpected = 0.0,
			AUOffset = 0.0,
			AUScale = pdaScanIndex.AUScale,
			NumberOfPackets = (uint)numData,
			DataPosition = 0L
		}, AsrProfileIndexStructSize), 0, array, 0, AsrProfileIndexStructSize);
		return array;
	}
}
