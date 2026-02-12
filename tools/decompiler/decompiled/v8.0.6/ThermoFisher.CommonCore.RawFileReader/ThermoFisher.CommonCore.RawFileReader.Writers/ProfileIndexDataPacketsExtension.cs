using System;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.ExtraScanInfo;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides extension methods for Profile Index Data Packets
/// </summary>
internal static class ProfileIndexDataPacketsExtension
{
	private static readonly int ProfileDataPacket64Size = Utilities.StructSizeLookup.Value[9];

	/// <summary>
	/// Convert the profile index data packets structure to a byte array.
	/// </summary>
	/// <param name="profileDataPackets">The profile data packets.</param>
	/// <returns>Profile index data packets in byte array form.</returns>
	/// <exception cref="T:System.ArgumentNullException">profile Data Packets</exception>
	public static byte[] MassSpecProfileIndexDataPktsToByteArray(this ProfileDataPacket64[] profileDataPackets)
	{
		if (profileDataPackets == null)
		{
			throw new ArgumentNullException("profileDataPackets");
		}
		int num = profileDataPackets.Length;
		byte[] array = new byte[ProfileDataPacket64Size * num];
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			Buffer.BlockCopy(WriterHelper.StructToByteArray(profileDataPackets[i], ProfileDataPacket64Size), 0, array, num2, ProfileDataPacket64Size);
			num2 += ProfileDataPacket64Size;
		}
		return array;
	}
}
