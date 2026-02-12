using System;
using System.Runtime.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// class to represent calibration data, as may be drawn on a plot.
/// </summary>
[Serializable]
[DataContract]
public class CalibrationCurvePoint : ICloneable, ICalibrationCurvePointAccess
{
	/// <summary>
	/// Gets or sets the amount (x) on calibration curve
	/// </summary>
	[DataMember]
	public double Amount { get; set; }

	/// <summary>
	/// Gets or sets the response (y) for the amount
	/// </summary>
	[DataMember]
	public double Response { get; set; }

	/// <summary>
	/// Gets or sets the a key to identify this point. For example, a file name.
	/// </summary>
	[DataMember]
	public string Key { get; set; }

	/// <summary>
	/// Gets or sets the a second key to identify this point. For example, a compound name.
	/// </summary>
	[DataMember]
	public string PeakKey { get; set; }

	/// <summary>
	/// Test that two points are equal
	/// </summary>
	/// <param name="left">first point to compare</param>
	/// <param name="right">second point to compare</param>
	/// <returns>true if they have the same contents</returns>
	public static bool operator ==(CalibrationCurvePoint left, CalibrationCurvePoint right)
	{
		return left?.Equals(right) ?? ((object)right == null);
	}

	/// <summary>
	/// Test that two spec points are not equal
	/// </summary>
	/// <param name="left">first point to compare</param>
	/// <param name="right">second point to compare</param>
	/// <returns>true if they do not have the same contents</returns>
	public static bool operator !=(CalibrationCurvePoint left, CalibrationCurvePoint right)
	{
		return !(left == right);
	}

	/// <summary>
	/// Test two points for equality
	/// </summary>
	/// <param name="obj">
	/// point to compare
	/// </param>
	/// <returns>
	/// true if they are equal
	/// </returns>
	public override bool Equals(object obj)
	{
		if (obj != null)
		{
			CalibrationCurvePoint calibrationCurvePoint = (CalibrationCurvePoint)obj;
			if (Math.Abs(Response - calibrationCurvePoint.Response) < 1E-08 && Math.Abs(Amount - calibrationCurvePoint.Amount) < 1E-08 && Key == calibrationCurvePoint.Key)
			{
				return PeakKey == calibrationCurvePoint.PeakKey;
			}
			return false;
		}
		return false;
	}

	/// <summary>
	/// Gets a hash code <see>Object.GetHashCode</see>
	/// </summary>
	/// <returns>
	/// The has code <see>Object.GetHashCode</see>
	/// </returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>
	/// Creates a new object that is a copy of the current instance.
	/// </summary>
	/// <returns>
	/// A new object that is a copy of this instance.
	/// </returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		return MemberwiseClone();
	}
}
