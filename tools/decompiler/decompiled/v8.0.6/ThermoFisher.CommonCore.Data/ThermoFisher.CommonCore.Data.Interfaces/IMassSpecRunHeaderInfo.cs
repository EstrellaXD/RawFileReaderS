namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Information about the mass spec device data stream.
/// </summary>
public interface IMassSpecRunHeaderInfo
{
	/// <summary>
	/// Gets or sets the expected run time.
	/// The expected run time. All devices MUST do this so that the real-time update can display a sensible Axis.
	/// </summary>
	/// <value>
	/// The expected run time.
	/// </value>
	double ExpectedRunTime { get; set; }

	/// <summary>
	/// Gets the mass resolution (width of the half peak).
	/// Optional field, it has a default value of 0.5.
	/// </summary>
	/// <value>
	/// The mass resolution.
	/// </value>
	double MassResolution { get; }

	/// <summary>
	/// Gets the first comment about this data stream.
	/// Optional field, it has a default value of empty string.
	/// The comment is for "Sample Name" in Chromatogram view title (max 39 chars).
	/// </summary>
	/// <value>
	/// The comment1.
	/// </value>
	string Comment1 { get; }

	/// <summary>
	/// Gets the second comment about this data stream.
	/// Optional field, it has a default value of empty string.
	/// This comment is for "Comment" in Chromatogram view title (max 63 chars).
	/// </summary>
	/// <value>
	/// The comment2.
	/// </value>
	string Comment2 { get; }

	/// <summary>
	/// Gets the number of digits of precision suggested for formatting masses.
	/// Optional field, it has a default value of 2.
	/// </summary>
	/// <value>
	/// The precision.
	/// </value>
	int Precision { get; }
}
