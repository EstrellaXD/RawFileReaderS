namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Label Peak Information.
/// </summary>
public class LabelPeak : ICentroidPeak
{
	/// <summary>
	/// Gets or sets mass.
	/// </summary>
	public double Mass { get; set; }

	/// <summary>
	/// Gets or sets intensity.
	/// </summary>
	public double Intensity { get; set; }

	/// <summary>
	/// Gets or sets resolution.
	/// </summary>
	public double Resolution { get; set; }

	/// <summary>
	/// Gets or sets base Line.
	/// </summary>
	public double Baseline { get; set; }

	/// <summary>
	/// Gets or sets noise.
	/// </summary>
	public double Noise { get; set; }

	/// <summary>
	/// Gets or sets charge.
	/// </summary>
	public double Charge { get; set; }

	/// <summary>
	/// Gets or sets Peak Options Flag.
	/// </summary>
	public PeakOptions Flag { get; set; }

	/// <summary>
	/// Gets or sets the signal to noise.
	/// </summary>
	public double SignalToNoise { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.LabelPeak" /> class. 
	/// Default Constructor.
	/// </summary>
	public LabelPeak()
	{
		Flag = PeakOptions.None;
	}
}
