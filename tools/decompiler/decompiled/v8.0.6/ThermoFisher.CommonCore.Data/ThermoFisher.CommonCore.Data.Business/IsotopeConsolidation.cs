namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// IsotopeConsolidation - which function will be used for isotope consolidation
/// </summary>
public enum IsotopeConsolidation
{
	/// <summary>
	/// RemoveAllIsotopes - use function that strips all the isotopes during processing
	/// </summary>
	RemoveAllIsotopes,
	/// <summary>
	/// KeepNonBaseIsotopes - use function that keeps non-base isotopes in the formula
	/// </summary>
	KeepNonBaseIsotopes
}
