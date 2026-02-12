using System;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// A replicate is a measurement of a single calibration peak in 1 file.
/// This interfaces defines required features of a replicate for the regression code.
/// A calibration system needs to implement at least this information
/// (typically from a calibration table and peak integration results).
/// The real class "Replicate" implements this.
/// </summary>
public interface IReplicate : ICloneable
{
	/// <summary>
	/// Gets the amount of target compound in calibration or QC standard.
	/// </summary>
	double Amount { get; }

	/// <summary>
	/// Gets the response of this sample, for example: Ratio of target peak area to ISTD peak area
	/// </summary>
	double Response { get; }

	/// <summary>
	/// Gets or sets a value indicating whether this data point should be excluded from the calibration curve.
	/// </summary>
	bool ExcludeFromCalibration { get; set; }

	/// <summary>
	/// Gets the key name associated with this replicate (for example a file name)
	/// </summary>
	string Key { get; }

	/// <summary>
	/// Gets the second key name associated with this replicate (for example a compound name)
	/// </summary>
	string PeakKey { get; }

	/// <summary>
	/// Implementation of <c>ICloneable.Clone</c> method.
	/// Creates deep copy of this instance.
	/// </summary>
	/// <returns>
	/// An exact copy of the current Replicate.
	/// </returns>
	new object Clone();
}
