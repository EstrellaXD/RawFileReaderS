using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines interfaces for a calibration level.
/// </summary>
public interface ICalibrationLevel : ICloneable, ICalibrationLevelAccess
{
	/// <summary>
	/// Gets or sets the name for this calibration level
	/// </summary>
	new string Name { get; set; }

	/// <summary>
	/// Gets or sets the amount of calibration compound (usually a concentration) for this level
	/// </summary>
	new double BaseAmount { get; set; }

	/// <summary>
	/// Implementation of <c>ICloneable.Clone</c> method.
	/// Creates deep copy of this instance.
	/// </summary>
	/// <returns>An exact copy of the current level.</returns>
	new object Clone();
}
