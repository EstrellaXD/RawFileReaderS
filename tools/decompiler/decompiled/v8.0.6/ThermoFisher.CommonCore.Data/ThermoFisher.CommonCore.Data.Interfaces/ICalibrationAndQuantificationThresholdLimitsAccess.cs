namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read only access to Calibration And Quantification Threshold Limits
/// </summary>
public interface ICalibrationAndQuantificationThresholdLimitsAccess
{
	/// <summary>
	/// Gets the carry over limit threshold.
	/// </summary>
	/// <value>The carry over limit threshold.</value>
	double CarryoverLimitThreshold { get; }

	/// <summary>
	/// Gets the detection limit threshold.
	/// </summary>
	/// <value>The detection limit threshold.</value>
	double DetectionLimitThreshold { get; }

	/// <summary>
	/// Gets the linearity limit threshold.
	/// </summary>
	/// <value>The linearity limit threshold.</value>
	double LinearityLimitThreshold { get; }

	/// <summary>
	/// Gets the quantitation limit threshold.
	/// </summary>
	/// <value>The quantitation limit threshold.</value>
	double QuantitationLimitThreshold { get; }

	/// <summary>
	/// Gets the R squared threshold.
	/// </summary>
	/// <value>The R squared threshold.</value>
	double RSquaredThreshold { get; }

	/// <summary>
	/// Gets the limit of reporting
	/// A value should only be reported if it is &gt;= the limit of reporting.
	/// This value is used to calculate the ReportingLimitPassed flag.
	/// </summary>
	double LimitOfReporting { get; }
}
