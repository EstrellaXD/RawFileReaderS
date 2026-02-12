using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to chromatogram trace settings
/// </summary>
public interface IChromatogramTraceSettingsAccess
{
	/// <summary>
	/// Gets the type of trace to construct
	/// </summary>
	/// <value>see <see cref="T:ThermoFisher.CommonCore.Data.Business.TraceType" /> for more details</value>
	TraceType Trace { get; }

	/// <summary>
	/// Gets a value indicating whether reference and exception peaks are included.
	/// in this chromatogram trace
	/// </summary>
	bool IncludeReference { get; }

	/// <summary>
	/// Gets the filter used in searching scans during trace build
	/// </summary>
	string Filter { get; }

	/// <summary>
	/// Gets the delay in minutes.
	/// </summary>
	/// <value>Floating point delay in minutes</value>
	double DelayInMin { get; }

	/// <summary>
	/// Gets the fragment mass for neutral fragment filters.
	/// </summary>
	/// <value>Floating point fragment mass for neutral fragment filters</value>
	double FragmentMass { get; }

	/// <summary>
	/// Gets the mass ranges.
	/// </summary>
	/// <remarks>
	/// If <see cref="P:ThermoFisher.CommonCore.Data.IChromatogramTraceSettingsAccess.Trace" /> is MassRange then mass range values are used to build trace.
	/// </remarks>
	/// <value>Array of mass ranges</value>
	IRangeAccess[] MassRanges { get; }

	/// <summary>
	/// Gets the number of mass ranges.
	/// </summary>
	/// <remarks>
	/// If <see cref="P:ThermoFisher.CommonCore.Data.IChromatogramTraceSettingsAccess.Trace" /> is MassRange then mass range values are used to build trace.
	/// </remarks>
	/// <value>Numeric count of mass ranges</value>
	int MassRangeCount { get; }

	/// <summary>
	/// Gets the Compound Names.
	/// </summary>
	string[] CompoundNames { get; }

	/// <summary>
	/// Gets a range value at 0-based index.
	/// </summary>
	/// <remarks>
	/// Use <see cref="P:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings.MassRangeCount" /> to find out the count of mass ranges.
	/// <para>
	/// </para>
	/// If <see cref="P:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings.Trace" /> is MassRange then mass range values are used to build trace.
	/// </remarks>
	/// <param name="index">
	/// Index at which to retrieve the range
	/// </param>
	/// <returns>
	/// <see cref="T:ThermoFisher.CommonCore.Data.Business.Range" /> value at give index
	/// </returns>
	IRangeAccess GetMassRange(int index);
}
