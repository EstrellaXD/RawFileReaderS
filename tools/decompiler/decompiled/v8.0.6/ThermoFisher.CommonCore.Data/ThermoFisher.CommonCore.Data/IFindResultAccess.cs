using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to find results
/// </summary>
public interface IFindResultAccess
{
	/// <summary>
	/// Gets the scan number for this result
	/// </summary>
	int Scan { get; }

	/// <summary>
	/// Gets the scan number predicted for this peak
	/// </summary>
	int PredictedScan { get; }

	/// <summary>
	/// Gets the retention time of the peak which has been found
	/// </summary>
	double FoundRT { get; }

	/// <summary>
	/// Gets the score based on both forward and reverse matching factors
	/// </summary>
	double FindScore { get; }

	/// <summary>
	/// Gets score from forward search
	/// </summary>
	double ForwardScore { get; }

	/// <summary>
	/// Gets score from reverse search
	/// </summary>
	double ReverseScore { get; }

	/// <summary>
	/// Gets the intensity of the supplied chromatogram at this result
	/// </summary>
	double ChromatogramIntensity { get; }

	/// <summary>
	/// Gets score from Match algorithm.
	/// </summary>
	double MatchScore { get; }

	/// <summary>
	/// Gets the peak found for this result
	/// </summary>
	Peak FoundPeak { get; }
}
