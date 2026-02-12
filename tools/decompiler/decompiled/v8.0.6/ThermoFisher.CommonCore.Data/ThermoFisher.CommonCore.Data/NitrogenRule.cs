namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// The N-Rule builds on the fact that any compound
/// containing C,H,O,N,S elements has an even number
/// of N atoms if its molecular weight is even and
/// has an odd number of N atoms if its molecular
/// weight is odd. So if you want possible elemental
/// compositions for an molecular ion that has an even
/// mass, it makes no sense to display elemental composition
/// candidates with an odd number of N-atoms.
/// </summary>
public enum NitrogenRule
{
	/// <summary>
	/// No limitations are imposed on results based on this rule.
	/// </summary>
	DoNotUse,
	/// <summary>
	/// For the EvenElectronIons mode, elemental compositions
	/// containing N-atoms are only displayed if their RDB
	/// ( Ring and double bond equivalent ) has an integer value
	/// ( -1.0 0.0 1.0 2.0 … ); compositions containing N-atoms
	/// with an RDB value of -0.5 0.5 1.5.. are not displayed.
	/// </summary>
	EvenElectronIons,
	/// <summary>
	/// For the OddElectronIons mode, elemental compositions
	/// containing N-atoms are only displayed if their RDB
	/// ( Ring and double bond equivalent ) has an non-integer value
	/// ( -0.5 0.5 1.5.. ); compositions containing N-atoms
	/// with an RDB value of -1.0 0.0 1.0 2.0 … are not displayed.
	/// </summary>
	OddElectronIons
}
