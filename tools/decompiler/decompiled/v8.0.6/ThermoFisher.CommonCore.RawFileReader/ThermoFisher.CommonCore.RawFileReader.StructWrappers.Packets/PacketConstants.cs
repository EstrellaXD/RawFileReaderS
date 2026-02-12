namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

/// <summary>
/// The packet constants.
/// </summary>
internal static class PacketConstants
{
	internal const float AppCreatedScan = -1f;

	internal const uint CIndexMask = 262143u;

	internal const uint ModifiedMask = 524288u;

	internal const uint ExceptionMask = 1048576u;

	internal const uint ReferenceMask = 2097152u;

	internal const uint MergedMask = 4194304u;

	internal const uint FragmentedMask = 8388608u;

	internal const uint AnyFlagMask = 16252928u;

	internal const uint ChargeStateMask = 4278190080u;

	internal const uint ChargeLabelInvalid = 1u;

	internal const uint ReferenceLabelInvalid = 2u;

	internal const uint MergedLabelInvalid = 4u;

	internal const uint FragmentedLabelInvalid = 8u;

	internal const uint ExceptionLabelInvalid = 16u;

	internal const uint ModifiedLabelInvalid = 32u;

	internal const uint ReservedMask = 64u;

	internal const uint ProfSubsegOffsetMask = 128u;

	internal const uint TreatModifiedAsSaturated = 256u;

	internal const uint HmaCentroidsMask = 65536u;

	internal const uint LabelBitsValidCheck = 131072u;

	internal const byte LabelFragmentedMask = 1;

	internal const byte LabelMergedMask = 2;

	internal const byte LabelReferenceMask = 4;

	internal const byte LabelExceptionMask = 8;

	internal const byte LabelModifiedMask = 16;

	internal const byte LabelSaturatedMask = 32;

	internal const byte LabelRefOrExcMask = 12;

	internal const byte TsqModbit = 64;

	internal const byte TsqRefPk = 8;

	internal const byte TsqMergbit = 4;

	internal const byte TsqFragbit = 2;

	internal const byte TsqSatbit = 1;

	internal const byte AnyTsqFlag = 79;

	internal const uint Fm3Typ = 2147483648u;

	internal const uint Fm3Int = 268435455u;

	internal const uint SatFlag = 1073741824u;

	internal const uint SampleMask = 268435455u;

	internal const uint SampleFlag = 2147483648u;

	internal const uint ScaleFlag = 128u;

	internal const byte Pmsatp = 1;

	internal const byte Pmfrag = 2;

	internal const byte Pmmerg = 4;

	internal const byte Pmmath = 8;

	internal const byte Pmdref = 16;

	internal const byte Pmdexc = 32;

	internal const byte AllSupportedPacketFlags = 63;

	internal const byte Pmnoca = 64;

	internal const byte Pmnegi = 128;

	internal const uint ScaleMask = 805306368u;

	internal const uint ScaleMult1 = 0u;

	internal const uint ScaleMult8 = 268435456u;

	internal const uint ScaleMult64 = 536870912u;

	internal const uint ScaleMult512 = 805306368u;

	internal const byte Lr2Saturation = 128;

	internal const byte Lr2Fragment = 64;

	internal const byte Lr2Merge = 32;

	internal const byte AnyLr2Flag = 224;

	internal const byte Lr2EndOfData = 16;

	internal const byte Lr2ScaleBit2 = 4;

	internal const byte Lr2ScaleBit1 = 2;

	internal const byte Lr2ScaleBit0 = 1;

	internal const byte Lr2ScaleMask = 7;

	internal const uint Lr2Mult0 = 1u;

	internal const uint Lr2Mult1 = 8u;

	internal const uint Lr2Mult2 = 64u;

	internal const uint Lr2Mult3 = 512u;

	internal const uint Lr2Mult4 = 4096u;

	internal const uint Lr2Mult5 = 32768u;

	internal const uint Lr2Mult6 = 262144u;

	internal const uint Lr2Mult7 = 2097152u;

	/// <summary>
	/// The number of zero intensity packets to restore depends 
	/// on the smoothing limits in Xcalibur (currently 15).
	/// </summary>
	internal const int MaxZeroPackets = 8;

	internal const int NumberOfMassCalibrationCoefficients = 4;
}
