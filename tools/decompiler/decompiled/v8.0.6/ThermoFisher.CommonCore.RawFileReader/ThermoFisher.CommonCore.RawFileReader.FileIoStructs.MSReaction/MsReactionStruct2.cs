namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.MSReaction;

/// <summary>
/// The MS reaction struct version 2.
/// </summary>
internal struct MsReactionStruct2
{
	internal double PrecursorMass;

	internal double IsolationWidth;

	internal double CollisionEnergy;

	/// <summary>
	/// Set to 1 to use in scan filtering. High order bits hold the
	/// activation type enumeration bits 0xffe, and the flag for multiple
	/// activation (bit 0x1000).
	/// these features can be WST individually with the new access
	/// functions, or as a UINT with the new CollisionEnergyValidEx
	/// function
	/// </summary>
	internal uint CollisionEnergyValid;
}
