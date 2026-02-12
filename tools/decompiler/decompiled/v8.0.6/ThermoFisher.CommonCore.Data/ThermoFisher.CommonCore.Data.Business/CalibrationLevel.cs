using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This class defines a calibration level.
/// Each level has an amount BaseAmount and a name
/// </summary>
[Serializable]
[DataContract]
public class CalibrationLevel : CommonCoreDataObject, ICalibrationLevel, ICloneable, ICalibrationLevelAccess
{
	/// <summary>
	/// A values very close to zero
	/// </summary>
	private const double NearZero = 1E-06;

	/// <summary>
	/// Anticipated amount of target compound in calibration of QC standard.
	/// </summary>
	private double _baseAmount;

	/// <summary>
	/// Name for this calibration level
	/// </summary>
	private string _name;

	/// <summary>
	/// Gets or sets the name for this calibration level
	/// </summary>
	[DataMember]
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	/// <summary>
	/// Gets or sets the amount of calibration compound (usually a concentration) for this level
	/// </summary>
	[DataMember]
	public double BaseAmount
	{
		get
		{
			return _baseAmount;
		}
		set
		{
			double baseAmount = ((value <= 1E-06) ? 1E-06 : value);
			_baseAmount = baseAmount;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.CalibrationLevel" /> class. 
	/// Create a copy of a calibration level
	/// </summary>
	/// <param name="level">
	/// Level to copy
	/// </param>
	public CalibrationLevel(ICalibrationLevelAccess level)
	{
		if (level != null)
		{
			_baseAmount = level.BaseAmount;
			_name = level.Name;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.CalibrationLevel" /> class. 
	/// Default construction of calibration level
	/// </summary>
	public CalibrationLevel()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.CalibrationLevel" /> class. 
	/// </summary>
	/// <param name="name">
	/// A name associated with the level
	/// </param>
	/// <param name="amount">
	/// The amount of calibration compound (usually a concentration) for this level
	/// </param>
	public CalibrationLevel(string name, double amount)
	{
		Name = name;
		BaseAmount = amount;
	}

	/// <summary>
	/// Implementation of <c>ICloneable.Clone</c> method.
	/// Creates deep copy of this instance.
	/// </summary>
	/// <returns>An exact copy of the current level.</returns>
	public virtual object Clone()
	{
		return (CalibrationLevel)MemberwiseClone();
	}
}
