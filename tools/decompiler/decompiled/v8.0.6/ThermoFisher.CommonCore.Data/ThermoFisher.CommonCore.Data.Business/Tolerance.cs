using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines how mass tolerance is measured
/// </summary>
[Serializable]
public class Tolerance
{
	/// <summary>
	/// The default mass tolerance value.
	/// </summary>
	private const double DefaultToleranceValue = 10.0;

	/// <summary>
	/// The default mass tolerance mode.
	/// </summary>
	private const ToleranceMode DefaultToleranceMode = ToleranceMode.Mmu;

	/// <summary>
	/// Gets or sets a value which determines how the masses are compared.
	/// See <see cref="T:ThermoFisher.CommonCore.Data.Business.Tolerance" /> for details..
	/// </summary>
	public ToleranceMode Mode { get; set; }

	/// <summary>
	/// Gets or sets a value which determines the maximum error in mass for something to pass tolerance, in the selected units.
	/// </summary>
	public double Value { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Tolerance" /> class. 
	/// Create a mass tolerance object.
	/// This holds a tolerance method (such as mmu or ppm), and tolerance value.
	/// This is used, for example, to return only formulate within a certain tolerance of
	/// a measured mass.
	/// </summary>
	public Tolerance()
	{
		Mode = ToleranceMode.Mmu;
		Value = 10.0;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Tolerance" /> class. 
	/// Make a copy of a mass tolerance.
	/// </summary>
	/// <param name="from">
	/// Tolerance to copy from
	/// </param>
	public Tolerance(Tolerance from)
	{
		if (from != null)
		{
			Mode = from.Mode;
			Value = from.Value;
		}
	}

	/// <summary>
	/// Get the mass limits.
	/// </summary>
	/// <param name="myMass">
	/// The my mass.
	/// </param>
	/// <param name="lowMassLimit">
	/// The low mass limit.
	/// </param>
	/// <param name="highMassLimit">
	/// The high mass limit.
	/// </param>
	public void GetMassLimits(double myMass, out double lowMassLimit, out double highMassLimit)
	{
		double num = DeltaAtMass(myMass);
		lowMassLimit = myMass - num;
		highMassLimit = myMass + num;
	}

	/// <summary>
	/// Get the delta mass at a given mass. For example: If a mass window "within tolerance" is needed, that 
	/// window would be calculated as follows:
	/// <c> double difference = DeltaAtMass(myMass);
	/// lowMassLimit = myMass - difference;
	/// highMassLimit = myMass + difference;
	/// </c>
	/// </summary>
	/// <param name="mass">
	/// Mass at which delta should be calculated.
	/// </param>
	/// <returns>
	/// The delta from the given mass, which corresponds to the tolerance settings
	/// </returns>
	public double DeltaAtMass(double mass)
	{
		double result = 0.0;
		switch (Mode)
		{
		case ToleranceMode.Amu:
			result = Value;
			break;
		case ToleranceMode.Mmu:
			result = Value / 1000.0;
			break;
		case ToleranceMode.Ppm:
			result = mass * Value / 1000000.0;
			break;
		}
		return result;
	}
}
