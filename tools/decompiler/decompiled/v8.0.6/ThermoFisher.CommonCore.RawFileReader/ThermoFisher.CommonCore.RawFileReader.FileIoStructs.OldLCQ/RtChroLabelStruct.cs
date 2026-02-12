namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The real time chromatogram label struct.
/// </summary>
internal struct RtChroLabelStruct
{
	internal bool LabelTime;

	internal bool LabelScan;

	internal bool LabelBase;

	internal bool LabelFlags;

	internal bool LabelOffsetOn;

	internal bool Rotate;

	internal bool Boxed;

	internal double LabelOffset;

	internal double LabelThreshold;

	internal bool LabelArea;

	internal bool LabelSN;

	internal bool LabelHeight;

	internal int RTPrecision;
}
