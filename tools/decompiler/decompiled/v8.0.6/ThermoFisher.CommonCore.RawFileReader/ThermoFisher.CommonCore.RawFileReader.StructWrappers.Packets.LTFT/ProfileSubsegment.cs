namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT;

/// <summary>
/// The profile sub-segment.
/// </summary>
internal sealed class ProfileSubsegment
{
	/// <summary>
	/// Gets or sets the profile points.
	/// </summary>
	internal uint ProfilePoints { get; set; }

	/// <summary>
	/// Gets or sets the mass offset.
	/// </summary>
	internal float MassOffset { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT.ProfileSubsegment" /> class. 
	/// Create a sub-segment from data read from raw file
	/// </summary>
	/// <param name="profilePoints">
	/// Number of profile points
	/// </param>
	/// <param name="massOffset">
	/// Segment mass offset
	/// </param>
	internal ProfileSubsegment(uint profilePoints, float massOffset)
	{
		ProfilePoints = profilePoints;
		MassOffset = massOffset;
	}
}
