using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Determine how the calibration curve is weighted
/// </summary>
[DataContract]
public enum Weighting
{
	/// <summary>
	/// This allows you to weight all calibration data points equally during the
	/// least-squares regression calculation of the calibration curve. 
	/// </summary>
	[EnumMember]
	EqualWeights,
	/// <summary>
	/// This allows you to specify a weighting of 1/X for
	/// all calibration data points during 
	/// the least-squares regression calculation of the calibration curve.
	/// Calibrants are weighted by the inverse of their quantity.
	/// </summary>
	[EnumMember]
	OneOverX,
	/// <summary>
	/// This allows you to specify a weighting of 1/X^2 for
	/// all calibration data points during the
	/// least-squares regression calculation of the calibration curve.
	/// Calibrants are weighted by the inverse of  the square of their quantity.
	/// </summary>
	[EnumMember]
	OneOverX2,
	/// <summary>
	/// This allows you to specify a weighting of 1/Y for
	/// all calibration data points during the least-squares 
	/// regression calculation of the calibration curve.
	/// Calibrants are weighted by the inverse of their response (or response ratio).
	/// </summary>
	[EnumMember]
	OneOverY,
	/// <summary>
	/// This allows you to specify a weighting of 1/Y^2 for
	/// all calibration data points during the least-squares
	/// regression calculation of the calibration curve.
	/// Calibrants are weighted by the inverse of
	/// the square of their response (or response ratio).
	/// </summary>
	[EnumMember]
	OneOverY2,
	/// <summary>
	/// This allows you to specify a weighting of 1/s^2 for all calibration
	/// data points during the least-squares regression calculation of the calibration curve.
	/// Calibrants at a given level are weighted by the inverse of the standard deviation of
	/// their responses (or response ratios). For this weighting factor to be used,
	/// there must be two or more replicates at each level. 
	/// If only one calibrant is available for any level, 1/s^2 weighting cannot be used.
	/// </summary>
	[EnumMember]
	OneOverSigma2
}
