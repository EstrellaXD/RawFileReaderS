namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read only access to scan statistics
/// Extends data from the index.
/// </summary>
public interface IScanStatisticsAccess : IMsScanIndexAccess
{
	/// <summary>
	/// Gets the longest wavelength in PDA scan
	/// </summary>
	double LongWavelength { get; }

	/// <summary>
	/// Gets the shortest wavelength in PDA scan
	/// </summary>
	double ShortWavelength { get; }

	/// <summary>
	/// Gets the number of
	/// channels acquired in this scan, if this is UV or analog data,
	/// </summary>
	int NumberOfChannels { get; }

	/// <summary>
	/// Gets the frequency.
	/// </summary>
	double Frequency { get; }

	/// <summary>
	/// Gets a value indicating whether is uniform time.
	/// </summary>
	bool IsUniformTime { get; }

	/// <summary>
	/// Gets the absorbance unit scale.
	/// </summary>
	double AbsorbanceUnitScale { get; }

	/// <summary>
	/// Gets the wave length step.
	/// </summary>
	double WavelengthStep { get; }

	/// <summary>
	/// Gets a String defining the scan type, for filtering
	/// </summary>
	string ScanType { get; }
}
