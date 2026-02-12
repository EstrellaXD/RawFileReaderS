using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This class defines a QC level.
/// This is based on a calibration level, with a name and expected amount (base amount)
/// The QC level add a TestPercent and a means of testing
/// </summary>
[Serializable]
[DataContract]
public class QualityControlLevel : CalibrationLevel, IQualityControlLevelAccess, ICalibrationLevelAccess
{
	private double _testPercent;

	/// <summary>
	/// Gets or sets the QC test standard: <code>100 * (yobserved-ypredicted)/ypreditced</code>
	/// </summary>
	[DataMember]
	public double TestPercent
	{
		get
		{
			return _testPercent;
		}
		set
		{
			_testPercent = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.QualityControlLevel" /> class. 
	/// Create a copy of a QC level
	/// </summary>
	/// <param name="level">
	/// Level to copy
	/// </param>
	public QualityControlLevel(IQualityControlLevelAccess level)
		: base(level)
	{
		if (level != null)
		{
			_testPercent = level.TestPercent;
		}
	}

	/// <summary>
	/// Test if an amount passes the QC test for this level
	/// </summary>
	/// <param name="amount">
	/// The calculated amount for the QC
	/// </param>
	/// <returns>
	/// true if the QC test passes, within tolerance
	/// </returns>
	public bool Passes(double amount)
	{
		return Math.Abs(100.0 * (amount - base.BaseAmount) / base.BaseAmount) <= _testPercent;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.QualityControlLevel" /> class. 
	/// Default construction of QC level
	/// </summary>
	public QualityControlLevel()
	{
	}

	/// <summary>
	/// Implementation of <c>ICloneable.Clone</c> method.
	/// Creates deep copy of this instance.
	/// </summary>
	/// <returns>An exact copy of the current level.</returns>
	public override object Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.QualityControlLevel" /> class. 
	/// Create a quality control level
	/// </summary>
	/// <param name="name">
	/// A name associated with the level
	/// </param>
	/// <param name="baseAmount">
	/// The amount of calibration compound (usually a concentration) for this level
	/// </param>
	/// <param name="testPercent">
	/// QC test standard: <code>100 * (yobserved-ypredicted)/ypreditced</code>
	/// </param>
	public QualityControlLevel(string name, double baseAmount, double testPercent)
	{
		base.Name = name;
		base.BaseAmount = baseAmount;
		TestPercent = testPercent;
	}
}
