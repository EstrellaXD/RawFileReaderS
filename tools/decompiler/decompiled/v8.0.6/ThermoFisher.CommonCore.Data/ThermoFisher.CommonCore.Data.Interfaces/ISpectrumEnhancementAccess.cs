namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read access to parameters for spectrum enhancements
/// </summary>
public interface ISpectrumEnhancementAccess
{
	/// <summary>
	/// Gets a value indicating whether the refine (combine) section enabled
	/// </summary>
	bool Enabled { get; }

	/// <summary>
	/// Gets a value indicating we using refine, combine or threshold
	/// </summary>
	SpectrumEnhanceMode SpectrumMethod { get; }

	/// <summary>
	/// Gets the refine window size
	/// </summary>
	double RefineWindowSize { get; }

	/// <summary>
	/// Gets he refine noise threshold
	/// </summary>
	int RefineNoiseThreshold { get; }

	/// <summary>
	/// Gets the combine take points across peak top
	/// </summary>
	double TopRegionWidth { get; }

	/// <summary>
	/// Gets the combine background scaling factor
	/// </summary>
	double BackgroundScalingFactor { get; }

	/// <summary>
	/// Gets the Region 1 method: at peak or use previous
	/// </summary>
	SpectrumSubtractMethod LeftRegionMethod { get; }

	/// <summary>
	/// Gets the start point before the peak top
	/// </summary>
	double PointsBeforePeakTop { get; }

	/// <summary>
	/// Gets the previous points in background
	/// </summary>
	double LeftRegionPoints { get; }

	/// <summary>
	/// Gets Region 2 method: at peak or use next
	/// </summary>
	SpectrumSubtractMethod RightRegionMethod { get; }

	/// <summary>
	/// Gets Points after peak top
	/// </summary>
	double PointsAfterPeakTop { get; }

	/// <summary>
	/// Gets the nNext point in background
	/// </summary>
	double RightRegionPoints { get; }

	/// <summary>
	/// Gets the cut off threshold
	/// </summary>
	double CutOffThreshold { get; }
}
