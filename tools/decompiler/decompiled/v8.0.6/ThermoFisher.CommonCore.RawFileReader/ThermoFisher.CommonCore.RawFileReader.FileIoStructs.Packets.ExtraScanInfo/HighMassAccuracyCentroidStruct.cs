using System.Runtime.InteropServices;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.ExtraScanInfo;

/// <summary>
/// The High Mass Accuracy Centroid structure
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct HighMassAccuracyCentroidStruct
{
	/// <summary>
	/// Gets or sets the mass.
	/// </summary>
	internal double Mass { get; set; }

	/// <summary>
	/// Gets or sets the intensity.
	/// </summary>
	internal float Intensity { get; set; }
}
