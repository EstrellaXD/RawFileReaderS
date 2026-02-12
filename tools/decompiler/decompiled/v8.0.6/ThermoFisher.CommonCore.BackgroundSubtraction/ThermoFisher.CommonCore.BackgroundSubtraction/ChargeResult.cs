using System.Collections.Generic;

namespace ThermoFisher.CommonCore.BackgroundSubtraction;

/// <summary>
/// The charge result, represents a proposed set of isotopes 
/// which have a charge
/// </summary>
internal class ChargeResult
{
	/// <summary>
	/// Gets or sets the charge.
	/// </summary>
	public int Charge { get; set; }

	/// <summary>
	/// Gets or sets the isotopes.
	/// </summary>
	public List<int> Isotopes { get; set; }
}
