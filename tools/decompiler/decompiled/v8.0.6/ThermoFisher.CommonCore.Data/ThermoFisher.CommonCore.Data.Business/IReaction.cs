using ThermoFisher.CommonCore.Data.FilterEnums;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The Reaction interface.
/// Defines a reaction for fragmenting an ion (an MS/MS stage).
/// </summary>
public interface IReaction
{
	/// <summary>
	/// Gets the precursor mass (mass acted on)
	/// </summary>
	double PrecursorMass { get; }

	/// <summary>
	/// Gets the collision energy of this reaction
	/// </summary>
	double CollisionEnergy { get; }

	/// <summary>
	/// Gets the isolation width of the precursor mass
	/// </summary>
	double IsolationWidth { get; }

	/// <summary>
	/// Gets a value indicating whether precursor range is valid.
	/// If this is true, then <see cref="P:ThermoFisher.CommonCore.Data.Business.IReaction.PrecursorMass" /> is still the center of
	/// the range, but the values <see cref="P:ThermoFisher.CommonCore.Data.Business.IReaction.FirstPrecursorMass" /> and <see cref="P:ThermoFisher.CommonCore.Data.Business.IReaction.LastPrecursorMass" />
	/// define the limits of the precursor mass range
	/// </summary>
	bool PrecursorRangeIsValid { get; }

	/// <summary>
	/// Gets the start of the precursor mass range (only if <see cref="P:ThermoFisher.CommonCore.Data.Business.IReaction.PrecursorRangeIsValid" />)
	/// </summary>
	double FirstPrecursorMass { get; }

	/// <summary>
	/// Gets the end of the precursor mass range (only if <see cref="P:ThermoFisher.CommonCore.Data.Business.IReaction.PrecursorRangeIsValid" />)
	/// </summary>
	double LastPrecursorMass { get; }

	/// <summary>
	/// Gets a value indicating whether collision energy is valid.
	/// </summary>
	bool CollisionEnergyValid { get; }

	/// <summary>
	/// Gets the activation type.
	/// </summary>
	ActivationType ActivationType { get; }

	/// <summary>
	/// Gets a value indicating whether this is a multiple activation.
	/// In a table of reactions, a multiple activation is a second, or further,
	/// activation (fragmentation method) applied to the same precursor mass.
	/// Precursor mass values should be obtained from the original activation, and may not
	/// be returned by subsequent multiple activations.
	/// </summary>
	bool MultipleActivation { get; }

	/// <summary>
	/// Gets the isolation width offset.
	/// </summary>
	double IsolationWidthOffset { get; }
}
