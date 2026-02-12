using System.Collections.ObjectModel;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to a located peak
/// </summary>
public interface ILocatedPeakAccess
{
	/// <summary>
	/// Gets the peak which best matches the location rules.
	/// </summary>
	IPeakAccess DetectedPeak { get; }

	/// <summary>
	/// Gets how this peak was found.
	/// The find results are only valid when this is set to "Spectrum".
	/// </summary>
	PeakMethod Method { get; }

	/// <summary>
	/// Gets a value indicating whether RT adjustments could be made to the RT reference.
	/// This flag is only meaningful when RT reference adjustments are made based on
	/// a reference peak (see the locate class).
	/// If a valid reference peak is supplied, then the expected RT can be adjusted based on the reference.
	/// If no reference peak is found (a null peak) then the expected RT cannot be adjusted, and this flag will be false.
	/// </summary>
	bool ValidRTReference { get; }

	/// <summary>
	/// Gets the find results. When using spectrum LocateMethod this will contain the best matching peaks and find scores.
	/// </summary>
	ReadOnlyCollection<FindResult> FindResults { get; }
}
