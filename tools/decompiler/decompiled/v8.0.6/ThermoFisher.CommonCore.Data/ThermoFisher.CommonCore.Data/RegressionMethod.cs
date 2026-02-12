using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// The Regression algorithm used
/// </summary>
[DataContract]
public enum RegressionMethod
{
	/// <summary>
	/// A linear (first order) polynomial least-squares fit of the experimental data using slope and intercept coefficients: 
	/// <code>Y = mX + B </code>
	/// where m is the slope of the curve and B is the intercept point on the Y-axis. 
	/// </summary>
	[EnumMember]
	FirstOrder,
	/// <summary>
	/// A linear polynomial least-squares fit of the experimental data of the following mathematical form: 
	/// <code>log10[Y] = m log10[X] + B</code>
	/// where m is the slope of the curve and B is the intercept point on the Y-axis.
	/// </summary>
	[EnumMember]
	FirstOrderLogLog,
	/// <summary>
	/// A calibration curve which calculates a quadratic (second order) polynomial
	/// least-squares fit of the experimental data using the following mathematical form:
	/// <code>Y = AX2 + BX + C</code>, where A, B, and C are the polynomial coefficients.
	/// At least three calibration levels are required for this curve fit type.
	/// </summary>
	[EnumMember]
	SecondOrder,
	/// <summary>
	/// A calibration curve which calculates a quadratic (second order)
	/// polynomial least-squares fit of the experimental data using the following mathematical form: 
	/// <code>log [Y] = A log [X2] + B log [X] + C</code>
	/// where A, B, and C are the polynomial coefficients.
	/// Note: ignore/force/include origin options are not used with this Regression algorithm type. 
	/// </summary>
	[EnumMember]
	SecondOrderLogLog,
	/// <summary>
	/// Locally Weighted regression always kicks out replicates at
	/// either the lowest or highest level. With points
	/// at only three levels, a loess regression is
	/// discontinuous at the mid-point between the
	/// lowest and highest level.
	/// </summary>
	[EnumMember]
	LocallyWeighted,
	/// <summary>
	/// A calibration type in which the response factor is calculated
	/// for replicates at all calibration levels and then averaged.
	/// The amount in a sample can then be calculated by dividing
	/// the response by the average response factor. 
	/// </summary>
	[EnumMember]
	AverageResponseFactor,
	/// <summary>
	/// A calibration curve in which data at each calibration level are averaged.
	/// This averaging results in a single averaged data point at each calibration level.
	/// Averaged calibration points are plotted by connecting adjacent points with straight lines.
	/// This calibration curve can be used with one or more calibration levels.
	/// Calibration supports  ignore/force origin options for this calibration curve type.
	/// </summary>
	[EnumMember]
	PointToPoint,
	/// <summary>
	/// A calibration curve in which a cubic polynomial curve is fit between each
	/// pair of calibration levels such that the slopes of the separate cubic polynomial
	/// curves match at common calibration curve points.
	/// Calibration supports ignore/force origin options for this calibration curve type. 
	/// Note: At least four calibration levels are required for this type of curve fit.
	/// If the origin is forced, only three calibration levels are required. 
	/// </summary>
	[EnumMember]
	CubicSpline
}
