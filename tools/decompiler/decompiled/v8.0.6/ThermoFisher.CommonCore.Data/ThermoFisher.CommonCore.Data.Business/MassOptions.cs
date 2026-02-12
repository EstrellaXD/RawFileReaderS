using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Contains the options for displaying and calculating the masses.
/// </summary>
[Serializable]
[DataContract]
public class MassOptions : CommonCoreDataObject, ICloneable, IMassOptionsAccess
{
	/// <summary>
	/// The initial tolerance.
	/// </summary>
	private const double IntialTolerance = 500.0;

	/// <summary>
	/// The initial precision.
	/// </summary>
	private const int IntialPrecision = 1;

	/// <summary>
	/// The initial tolerance units.
	/// </summary>
	private const ToleranceUnits IntialToleranceUnits = ToleranceUnits.mmu;

	private double _tolerance = 500.0;

	private int _precision = 1;

	private ToleranceUnits _toleranceUnits;

	/// <summary>
	/// Gets or sets the tolerance value.
	/// </summary>
	/// <value>The tolerance.</value>
	[DataMember]
	public double Tolerance
	{
		get
		{
			return _tolerance;
		}
		set
		{
			_tolerance = value;
		}
	}

	/// <summary>
	/// Gets or sets the precision (decimal places).
	/// </summary>
	[DataMember]
	public int Precision
	{
		get
		{
			return _precision;
		}
		set
		{
			_precision = value;
		}
	}

	/// <summary>
	/// Gets or sets the tolerance units.
	/// </summary>
	[DataMember]
	public ToleranceUnits ToleranceUnits
	{
		get
		{
			return _toleranceUnits;
		}
		set
		{
			_toleranceUnits = value;
		}
	}

	/// <summary>
	/// Gets the tolerance string of the current toleranceUnits setting.
	/// </summary>
	public string ToleranceString => GetToleranceString(_toleranceUnits);

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.MassOptions" /> class. 
	/// Default Constructor
	/// </summary>
	public MassOptions()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.MassOptions" /> class. 
	/// Parameter Constructor
	/// </summary>
	/// <param name="tolerance">
	/// tolerance value
	/// </param>
	/// <param name="toleranceUnits">
	/// units of tolerance value
	/// </param>
	/// <param name="precision">
	/// precision (decimal places)
	/// </param>
	public MassOptions(double tolerance, ToleranceUnits toleranceUnits = ToleranceUnits.mmu, int precision = 1)
	{
		_precision = precision;
		_tolerance = tolerance;
		_toleranceUnits = toleranceUnits;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.MassOptions" /> class. 
	/// Construct instance from interface, by cloning all settings 
	/// </summary>
	/// <param name="access">
	/// Interface to clone
	/// </param>
	public MassOptions(IMassOptionsAccess access)
	{
		if (access != null)
		{
			_precision = access.Precision;
			_tolerance = access.Tolerance;
			_toleranceUnits = access.ToleranceUnits;
		}
	}

	/// <summary>
	/// Gets the tolerance string from the enumeration strings resource.
	/// </summary>
	/// <param name="toleranceUnits">
	/// The tolerance units.
	/// </param>
	/// <returns>
	/// The tolerance units as a string.
	/// </returns>
	public static string GetToleranceString(ToleranceUnits toleranceUnits)
	{
		return EnumFormat.ToString(toleranceUnits);
	}

	/// <summary>
	/// Implementation of <c>ICloneable.Clone</c> method.
	/// Creates deep copy of this instance.
	/// </summary>
	/// <returns>An exact copy of the current object.</returns>
	public object Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>
	/// Get the tolerance window around a specific mass
	/// </summary>
	/// <param name="mass">
	/// Mass about which window is needed
	/// </param>
	/// <returns>
	/// The distance (in amu) from the mass which is within tolerance.
	/// For example: myWindow=GetToleranceAtMass(myMass);
	/// accept data between "myMass-myWindow" and "myMass+myWindow"
	/// </returns>
	public double GetToleranceAtMass(double mass)
	{
		return _toleranceUnits switch
		{
			ToleranceUnits.amu => _tolerance, 
			ToleranceUnits.mmu => _tolerance / 1000.0, 
			ToleranceUnits.ppm => mass * _tolerance / 1000000.0, 
			_ => 0.5, 
		};
	}
}
