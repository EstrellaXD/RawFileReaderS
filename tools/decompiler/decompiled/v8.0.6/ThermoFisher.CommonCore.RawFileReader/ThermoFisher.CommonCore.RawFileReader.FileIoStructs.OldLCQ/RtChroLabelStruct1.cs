namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The real time chromatogram label struct version 1.
/// </summary>
internal struct RtChroLabelStruct1
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
}
