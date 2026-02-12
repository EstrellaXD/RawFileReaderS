using System;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.ExtraScanInfo;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.HRLRSP;

/// <summary>
/// Profile Index Data Packet
/// </summary>
internal sealed class ProfileIndexDataPacket : IRawObjectBase
{
	private const int SizeOfProfileDataPacket63 = 20;

	private const int SizeOfProfileDataPacket64 = 28;

	private const int Offset32BitDataPos = 0;

	private const int OffsetLowMass = 4;

	private const int OffsetHighMass = 8;

	private const int OffsetMassTick = 12;

	private const int Offset64BitDataPos = 20;

	private ProfileDataPacket64 _profileDataPkt;

	/// <summary>
	/// Gets or sets the data position.
	/// Offset into MS scan data
	/// </summary>
	/// <value>
	/// The data position.
	/// </value>
	public long DataPos { get; set; }

	/// <summary>
	/// Gets the low mass.
	/// </summary>
	/// <value>
	/// The low mass.
	/// </value>
	public float LowMass => _profileDataPkt.LowMass;

	/// <summary>
	/// Gets the high mass.
	/// </summary>
	/// <value>
	/// The high mass.
	/// </value>
	public float HighMass => _profileDataPkt.HighMass;

	/// <summary>
	/// Gets the mass tick.
	/// This is the mass step between profile points.
	/// </summary>
	/// <value>
	/// The mass tick.
	/// </value>
	public double MassTick => _profileDataPkt.MassTick;

	/// <summary>
	/// Gets or sets the number of packets.
	/// </summary>
	/// <value>
	/// The number packets.
	/// </value>
	public int NumberPackets { get; set; }

	/// <summary>
	/// Gets or sets the total packets.
	/// </summary>
	/// <value>
	/// The total packets.
	/// </value>
	public int TotalPackets { get; set; }

	public static int Size(int fileRevision)
	{
		if (fileRevision <= 63)
		{
			return 20;
		}
		return 28;
	}

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		if (fileRevision <= 63)
		{
			byte[] value = viewer.ReadBytesExt(ref startPos, 20);
			uint num = BitConverter.ToUInt32(value, 0);
			_profileDataPkt = new ProfileDataPacket64
			{
				DataPos = num,
				LowMass = BitConverter.ToSingle(value, 4),
				HighMass = BitConverter.ToSingle(value, 8),
				MassTick = BitConverter.ToDouble(value, 12),
				DataPosOffSet = num
			};
		}
		else
		{
			byte[] value2 = viewer.ReadBytesExt(ref startPos, 28);
			BitConverter.ToUInt32(value2, 0);
			_profileDataPkt = new ProfileDataPacket64
			{
				LowMass = BitConverter.ToSingle(value2, 4),
				HighMass = BitConverter.ToSingle(value2, 8),
				MassTick = BitConverter.ToDouble(value2, 12),
				DataPosOffSet = BitConverter.ToInt64(value2, 20)
			};
		}
		DataPos = _profileDataPkt.DataPosOffSet;
		return startPos - dataOffset;
	}
}
