using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

/// <summary>
/// Helper functions for peak packet types, such as:
/// identifying profile versus centroid scans
/// testing if label data exists
/// </summary>
internal static class PacketHelper
{
	/// <summary>
	/// Helper functions for identifying profile versus centroid scans
	/// </summary>
	/// <param name="packetType">Type of the packet.</param>
	/// <param name="packetTypeFlags">Additional flags about the packet type</param>
	/// <returns>True if this is a centroid scan</returns>
	public static bool IsCentroidScan(this SpectrumPacketType packetType, uint packetTypeFlags)
	{
		switch (packetTypeFlags)
		{
		case 0u:
			switch (packetType)
			{
			case SpectrumPacketType.LowResolutionSpectrum:
			case SpectrumPacketType.HighResolutionSpectrum:
			case SpectrumPacketType.CompressedAccurateSpectrum:
			case SpectrumPacketType.StandardAccurateSpectrum:
			case SpectrumPacketType.StandardUncalibratedSpectrum:
			case SpectrumPacketType.UvChannel:
			case SpectrumPacketType.MassSpecAnalog:
			case SpectrumPacketType.LowResolutionSpectrumType2:
			case SpectrumPacketType.LowResolutionSpectrumType3:
			case SpectrumPacketType.LinearTrapCentroid:
			case SpectrumPacketType.FtCentroid:
			case SpectrumPacketType.LowResolutionSpectrumType4:
				return true;
			case SpectrumPacketType.ProfileSpectrum:
			case SpectrumPacketType.ProfileIndex:
			case SpectrumPacketType.AccurateMassProfileSpectrum:
			case SpectrumPacketType.PdaUvDiscreteChannel:
			case SpectrumPacketType.PdaUvDiscreteChannelIndex:
			case SpectrumPacketType.PdaUvScannedSpectrum:
			case SpectrumPacketType.PdaUvScannedSpectrumIndex:
			case SpectrumPacketType.ProfileSpectrumType2:
			case SpectrumPacketType.ProfileSpectrumType3:
			case SpectrumPacketType.LinearTrapProfile:
			case SpectrumPacketType.FtProfile:
			case SpectrumPacketType.HighResolutionCompressedProfile:
			case SpectrumPacketType.LowResolutionCompressedProfile:
				return false;
			default:
				return false;
			}
		case 1u:
			return true;
		default:
			return false;
		}
	}

	/// <summary>
	/// Determines whether [has label peaks].
	/// </summary>
	/// <param name="packetType">Type of the packet.</param>
	/// <returns>true if the scan has label peaks</returns>
	public static bool HasLabelPeaks(this SpectrumPacketType packetType)
	{
		if ((uint)(packetType - -1) <= 20u || (uint)(packetType - 22) <= 3u)
		{
			return false;
		}
		return true;
	}
}
