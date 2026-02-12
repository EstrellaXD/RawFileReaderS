namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to return base peak data with a chromatogram.
/// </summary>
public interface IChromatogramBasePeaks
{
	/// <summary>
	/// Gets Base Peak masses for each chromatogram
	/// </summary>
	double[][] BasePeakArray { get; }
}
