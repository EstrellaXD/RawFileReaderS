using System.Collections.ObjectModel;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to
/// </summary>
public interface IFindSettingsAccess
{
	/// <summary>
	/// Gets the forward threshold for find algorithm.
	/// </summary>
	int ForwardThreshold { get; }

	/// <summary>
	/// Gets the match threshold for find algorithm
	/// </summary>
	int MatchThreshold { get; }

	/// <summary>
	/// Gets the reverse threshold for find algorithm
	/// </summary>
	int ReverseThreshold { get; }

	/// <summary>
	/// Gets the spec points.
	/// </summary>
	/// <value>The spec points.</value>
	ReadOnlyCollection<SpectrumPoint> SpecPoints { get; }

	/// <summary>
	/// Get a copy of the find spectrum
	/// </summary>
	/// <returns>
	/// The spectrum to find
	/// </returns>
	SpectrumPoint[] GetFindSpectrum();
}
