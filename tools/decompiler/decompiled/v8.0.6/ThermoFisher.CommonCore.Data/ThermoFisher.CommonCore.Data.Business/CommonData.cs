namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines the definitions required to reading the data files.
/// </summary>
public static class CommonData
{
	/// <summary>
	/// EH - The reason for the separation of packet type into profile/centroid and scan type is to
	/// support legacy LCQ code that utilized the packet type to determine whether or not a scan
	/// is profile or centroid. This becomes a problem with data converted from ICIS, for example,
	/// which may store profile data in the centroid packet format. Setting this value is only necessary
	/// where profile or centroid is written to a packet type that is not assumed to be of that
	/// data type.
	/// </summary>
	private enum ScanData
	{
		/// <summary>
		/// default is to use the packet type to determine profile/centroid
		/// </summary>
		AnyScanData,
		/// <summary>
		/// Centroid type
		/// </summary>
		Centroid,
		/// <summary>
		/// profile type.
		/// </summary>
		Profile
	}

	/// <summary>
	/// Low Resolution spectrum packet type.
	/// </summary>
	public const int LowResolutionSpectrum = 1;

	/// <summary>
	/// Low Resolution spectrum packet type2.
	/// </summary>
	public const int LowResolutionSpectrumType2 = 15;

	/// <summary>
	/// High Resolution spectrum packet type.
	/// </summary>
	public const int HighResolutionSpectrum = 2;

	/// <summary>
	/// Spectrum profileIndex type.
	/// </summary>
	public const int ProfileIndexType = 3;

	/// <summary>
	/// FTMS data type (centroids)
	/// </summary>
	public const int CentroidType = 20;

	/// <summary>
	/// FTMS data type (profiles)
	/// </summary>
	public const int FTProfileType = 21;

	/// <summary>
	///  Compressed profile format for MAT95
	/// </summary>
	public const int LowResolutionCompressedProfile = 23;

	/// <summary>
	/// Compressed profile format for MAT95 (high-res)
	/// </summary>
	public const int HighResolutionCompressedProfile = 22;

	/// <summary>
	/// LowResolution Spectrum Type (early quantum version).
	/// Also referred to as LR_SP_TYPE3 in some parts of FileIO.
	/// Used for centroid data from quantum about 2000, 2001 instruments.
	/// </summary>
	public const int LowResolutionSpectrumType3 = 17;

	/// <summary>
	/// LowResolution Spectrum Type.
	/// Also referred to as LR_SP_TYPE4 in some parts of FileIO.
	/// Used for centroid data from quantum and some other post 2002 instruments.
	/// </summary>
	public const int LowResolutionSpectrumType = 24;

	/// <summary>
	/// Profile Spectrum Type
	/// </summary>
	public const int ProfileSpectrumType = 0;

	/// <summary>
	/// Profile Spectrum Type2
	/// </summary>
	public const int ProfileSpectrumType2 = 14;

	/// <summary>
	/// Profile Spectrum Type3
	/// </summary>
	public const int ProfileSpectrumType3 = 16;

	/// <summary>
	/// Mass profile scan type
	/// </summary>
	public const int AcquisitionMassProfile = 7;

	/// <summary>
	/// PDA UV  discrete channel packet type
	/// </summary>
	public const int ChannelPacketType = 8;

	/// <summary>
	/// PDA UV  discrete channel index header type
	/// </summary>
	public const int ChannelIndexHeader = 9;

	/// <summary>
	/// PDA UV  scanned spectrum header packet type
	/// </summary>
	public const int ScannedSpectrumHeader = 10;

	/// <summary>
	/// PDA UV  scanned spectrum header index header type
	/// </summary>
	public const int ScannedSpectrumHeaderIndex = 11;

	/// <summary>
	/// UV  channel packet type
	/// </summary>
	public const int UVChannelPacket = 12;

	/// <summary>
	/// LT combined data type (profiles).
	/// </summary>
	public const int LTProfile = 19;

	/// <summary>
	/// LT Combined Centroids.
	/// </summary>
	public const int LTCombinedCentroids = 18;

	/// <summary>
	/// Bitwise left shift operator, to get high 16 bits.
	/// </summary>
	/// <param name="number">Number to shift</param>
	/// <returns>the high 16 bits of a 32 bit value</returns>
	public static int HIWord(int number)
	{
		return (number >> 16) & 0xFFFF;
	}

	/// <summary>
	/// Get the low 16 bits of a 32 bit value.
	/// </summary>
	/// <param name="number">Number to mask</param>
	/// <returns>The low 16 bits</returns>
	public static int LOWord(int number)
	{
		return number & 0xFFFF;
	}

	/// <summary>
	/// Helper function to identifying the profile scan based on the given packet type.
	/// </summary>
	/// <param name="packetType">packet type</param>
	/// <returns>True if this is a profile packet type</returns>
	public static bool IsProfileScan(int packetType)
	{
		bool result = false;
		switch (HIWord(packetType))
		{
		case 0:
			switch (LOWord(packetType))
			{
			case 0:
			case 3:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 14:
			case 16:
			case 19:
			case 21:
			case 22:
			case 23:
				result = true;
				break;
			}
			break;
		case 2:
			result = true;
			break;
		}
		return result;
	}
}
