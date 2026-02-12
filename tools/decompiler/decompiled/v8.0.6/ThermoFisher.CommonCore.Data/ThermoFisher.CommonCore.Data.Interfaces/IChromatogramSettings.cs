using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Settings required to read a chromatogram from a data stream
/// </summary>
public interface IChromatogramSettings
{
	/// <summary>
	/// Gets the detector delay
	/// </summary>
	double DelayInMin { get; }

	/// <summary>
	/// Gets A text definition of the scan filter
	/// </summary>
	string Filter { get; }

	/// <summary>
	/// Gets the Neutral fragment mass
	/// </summary>
	double FragmentMass { get; }

	/// <summary>
	/// Gets a value indicating whether to Include reference peaks in the chromatogram
	/// </summary>
	bool IncludeReference { get; }

	/// <summary>
	/// Gets the Number of mass ranges
	/// </summary>
	int MassRangeCount { get; }

	/// <summary>
	/// Gets the Mass ranges
	/// </summary>
	IRangeAccess[] MassRanges { get; }

	/// <summary>
	/// Gets a value which determines where the chromatogram comes from (TIC, mass range) etc.
	/// </summary>
	TraceType Trace { get; }
}
