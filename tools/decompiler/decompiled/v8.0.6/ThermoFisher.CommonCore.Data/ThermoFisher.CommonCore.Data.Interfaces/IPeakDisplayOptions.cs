namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Additional display options (for Xcalibur PMD file)
/// </summary>
public interface IPeakDisplayOptions
{
	/// <summary>
	/// Gets a value which extends (display) width so that peak is shown "not at edge"
	/// </summary>
	double ExcessWidth { get; }

	/// <summary>
	/// Gets a value indicating whether to label peaks with RT
	/// </summary>
	bool LabelWithRetentionTime { get; }

	/// <summary>
	/// Gets a value indicating whether to label peaks with scan number
	/// </summary>
	bool LabelWithScanNumber { get; }

	/// <summary>
	/// Gets a value indicating whether to label peaks with area
	/// </summary>
	bool LabelWithArea { get; }

	/// <summary>
	/// Gets a value indicating whether to label peaks with base peak
	/// </summary>
	bool LabelWithBasePeak { get; }

	/// <summary>
	/// Gets a value indicating whether to label peaks with height
	/// </summary>
	bool LabelWithHeight { get; }

	/// <summary>
	/// Gets a value indicating whether to label peaks with internal standard response
	/// </summary>
	bool LabelWithIstdResp { get; }

	/// <summary>
	/// Gets a value indicating whether to label peaks with signal to noise
	/// </summary>
	bool LabelWithSignalToNoise { get; }

	/// <summary>
	/// Gets a value indicating whether to label peaks with saturation
	/// </summary>
	bool LabelWithSaturationFlag { get; }

	/// <summary>
	/// Gets a value indicating whether to rotate peak label text
	/// </summary>
	bool LabelRotated { get; }

	/// <summary>
	/// Gets a value indicating whether to draw a box around peak labels
	/// </summary>
	bool LabelBoxed { get; }
}
