namespace ThermoFisher.CommonCore.BackgroundSubtraction;

/// <summary>
/// Settings for the background subtraction algorithm.
/// </summary>
public class BackgroundSubtractionSettings
{
	/// <summary>
	/// Gets or sets Background range 1 end time.
	/// </summary>
	public double Range1EndTime { get; set; }

	/// <summary>
	/// Gets or sets Background range 1 start time.
	/// </summary>
	public double Range1StartTime { get; set; }

	/// <summary>
	/// Gets or sets Background range 2 end time.
	/// </summary>
	public double Range2EndTime { get; set; }

	/// <summary>
	/// Gets or sets Background range 2 start time.
	/// </summary>
	public double Range2StartTime { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether Background range 1 will be used.
	/// </summary>
	public bool SelectedRange1 { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether Background range 1 will be used.
	/// </summary>
	public bool SelectedRange2 { get; set; }
}
