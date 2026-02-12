using ThermoFisher.CommonCore.Data.FilterEnums;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The MS reaction. 
/// This is reaction to fragment a precursor mass.
/// Reactions are used for MS/MS, Parent and Neutral scan types.
/// </summary>
public class MsReaction : IReaction
{
	/// <summary>
	/// Gets or sets the precursor mass (mass acted on).
	/// For a product ion scan, this would be a parent mass.
	/// For a parent ion scan, this would be the fragment mass.
	/// If this is a multiple reaction, this value is not used.
	/// </summary>
	public double PrecursorMass { get; set; }

	/// <summary>
	/// Gets or sets the collision energy of this reaction
	/// </summary>
	public double CollisionEnergy { get; set; }

	/// <summary>
	/// Gets or sets the isolation width of the precursor mass
	/// </summary>
	public double IsolationWidth { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether precursor range is valid.
	/// If this is true, then <see cref="P:ThermoFisher.CommonCore.Data.Business.IReaction.PrecursorMass" /> is still the center of
	/// the range, but the values <see cref="P:ThermoFisher.CommonCore.Data.Business.IReaction.FirstPrecursorMass" /> and <see cref="P:ThermoFisher.CommonCore.Data.Business.IReaction.LastPrecursorMass" />
	/// define the limits of the precursor mass range
	/// </summary>
	public bool PrecursorRangeIsValid { get; set; }

	/// <summary>
	/// Gets or sets the start of the precursor mass range (only if <see cref="P:ThermoFisher.CommonCore.Data.Business.IReaction.PrecursorRangeIsValid" />)
	/// </summary>
	public double FirstPrecursorMass { get; set; }

	/// <summary>
	/// Gets or sets the end of the precursor mass range (only if <see cref="P:ThermoFisher.CommonCore.Data.Business.IReaction.PrecursorRangeIsValid" />)
	/// </summary>
	public double LastPrecursorMass { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether collision energy is valid.
	/// When not valid "CollisionEnergy" should not be tested or displayed.
	/// </summary>
	public bool CollisionEnergyValid { get; set; }

	/// <summary>
	/// Gets or sets the activation type.
	/// This defines how an ion is fragmented.
	/// </summary>
	public ActivationType ActivationType { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this is a multiple activation.
	/// In a table of reactions, a multiple activation is a second, or further,
	/// activation (fragmentation method) applied to the same precursor mass.
	/// Precursor mass values should be obtained from the original activation, and may not
	/// be returned by subsequent multiple activations.
	/// </summary>
	public bool MultipleActivation { get; set; }

	/// <summary>
	/// Gets or sets the isolation width offset.
	/// </summary>
	public double IsolationWidthOffset { get; set; }
}
