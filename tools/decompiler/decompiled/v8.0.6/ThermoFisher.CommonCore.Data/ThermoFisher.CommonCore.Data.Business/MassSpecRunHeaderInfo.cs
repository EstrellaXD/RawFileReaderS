using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Information about the mass spec device data stream
/// </summary>
public class MassSpecRunHeaderInfo : IMassSpecRunHeaderInfo
{
	/// <summary>
	/// Gets or sets the expected run time. The value should be greater than zero.
	/// The expected run time. All devices MUST do this so that the real-time update can display a sensible Axis.
	/// </summary>
	/// <value>
	/// The expected run time.
	/// </value>
	public double ExpectedRunTime { get; set; }

	/// <summary>
	/// Gets or sets the mass resolution (width of the half peak).
	/// Optional property, it has a default value of 0.5.
	/// </summary>
	/// <value>
	/// The mass resolution.
	/// </value>
	public double MassResolution { get; set; }

	/// <summary>
	/// Gets or sets the first comment about this data stream.
	/// Optional property, it has a default value of empty string.
	/// The comment is for "Sample Name" in Chromatogram view title (max 39 chars).
	/// </summary>
	/// <value>
	/// The comment1.
	/// </value>
	public string Comment1 { get; set; }

	/// <summary>
	/// Gets or sets the second comment about this data stream.
	/// Optional property, it has a default value of empty string.
	/// This comment is for "Comment" in Chromatogram view title (max 63 chars).
	/// </summary>
	/// <value>
	/// The comment2.
	/// </value>
	public string Comment2 { get; set; }

	/// <summary>
	/// Gets or sets the number of digits of precision suggested for formatting masses.
	/// Optional property, it has a default value of 2.
	/// </summary>
	/// <value>
	/// The precision.
	/// </value>
	public int Precision { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.MassSpecRunHeaderInfo" /> class.
	/// Default field values:
	/// Comment1 = "";
	/// Comment2 = "";
	/// ExpectedRunTime = 0;
	/// MassResolution = 0.5;
	/// Precision = 2;
	/// </summary>
	public MassSpecRunHeaderInfo()
	{
		Comment1 = string.Empty;
		Comment2 = string.Empty;
		ExpectedRunTime = 0.0;
		MassResolution = 0.5;
		Precision = 2;
	}
}
