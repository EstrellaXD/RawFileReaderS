namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.MSReaction;

/// <summary>
/// The MS reaction struct version 3.
/// </summary>
internal struct MsReactionStruct3
{
	internal double PrecursorMass;

	internal double IsolationWidth;

	internal double CollisionEnergy;

	/// <summary>
	///     Set to 1 to use in scan filtering. High order bits hold the
	///     activation type enumeration bits 0xffe, and the flag for multiple
	///     activation (bit 0x1000).
	/// </summary>
	internal uint CollisionEnergyValid;

	/// <summary>
	///     If TRUE, <see cref="F:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.MSReaction.MsReactionStruct3.PrecursorMass" /> is still the center mass but the
	///     <see cref="F:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.MSReaction.MsReactionStruct3.FirstPrecursorMass" /> and <see cref="F:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.MSReaction.MsReactionStruct3.LastPrecursorMass" />
	///     are also valid.
	/// </summary>
	internal bool RangeIsValid;

	/// <summary>
	///     if <see cref="F:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.MSReaction.MsReactionStruct3.RangeIsValid" /> == TRUE, this value defines the start of the
	///     precursor isolation range.
	/// </summary>
	internal double FirstPrecursorMass;

	/// <summary>
	///     if <see cref="F:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.MSReaction.MsReactionStruct3.RangeIsValid" /> == TRUE, this value defines the end of the
	///     precursor isolation range
	/// </summary>
	internal double LastPrecursorMass;
}
