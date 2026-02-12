namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines data read for a set of chromatograms
/// First index = chromatogram number.
/// Second index = data value within chromatogram.
/// </summary>
public interface IChromatogramData
{
	/// <summary>
	/// Gets Times in minutes for each chromatogram
	/// </summary>
	double[][] PositionsArray { get; }

	/// <summary>
	/// Gets Scan numbers for data points in each chromatogram
	/// </summary>
	int[][] ScanNumbersArray { get; }

	/// <summary>
	/// Gets Intensities for each chromatogram
	/// </summary>
	double[][] IntensitiesArray { get; }

	/// <summary>
	/// Gets The number of chromatograms in this object
	/// </summary>
	int Length { get; }
}
