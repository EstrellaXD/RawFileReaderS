using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read only access to a calibration curve point
/// </summary>
public interface ICalibrationCurvePointAccess
{
	/// <summary>
	/// Gets the amount (x) on calibration curve
	/// </summary>
	double Amount { get; }

	/// <summary>
	/// Gets the response (y) for the amount
	/// </summary>
	double Response { get; }

	/// <summary>
	/// Gets the a key to identify this point. For example, a file name.
	/// </summary>
	[DataMember]
	string Key { get; }

	/// <summary>
	/// Gets the a second key to identify this point. For example, a compound name.
	/// </summary>
	[DataMember]
	string PeakKey { get; }
}
