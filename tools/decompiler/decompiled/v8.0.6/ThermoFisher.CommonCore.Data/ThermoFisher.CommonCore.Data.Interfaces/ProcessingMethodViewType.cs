namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// View to show when opening a PMD file.
/// Part of PMD file data, but may not be currently used
/// </summary>
public enum ProcessingMethodViewType
{
	/// <summary>
	/// Show method summary
	/// </summary>
	MethodSummary,
	/// <summary>
	/// Show component identification
	/// </summary>
	ComponentIdentification,
	/// <summary>
	/// Show calibration review
	/// </summary>
	CalibrationReview,
	/// <summary>
	/// Show peak detection
	/// </summary>
	PeakDetection
}
