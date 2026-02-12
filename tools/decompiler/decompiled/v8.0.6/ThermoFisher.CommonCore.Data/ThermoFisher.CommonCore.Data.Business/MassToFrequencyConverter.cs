using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Class which includes the coefficients for
/// mass calibration from a particular scan, and means to convert between mass and frequency
/// </summary>
public class MassToFrequencyConverter
{
	private double _coefficient1;

	private double _coefficient2;

	private double _coefficient2Squared;

	private double _coefficient3;

	private double _coefficient3Times4;

	private double _coefficient1Squared;

	/// <summary>
	/// Gets or sets the coefficient 1.
	/// </summary>
	public double Coefficient1
	{
		get
		{
			return _coefficient1;
		}
		set
		{
			_coefficient1 = value;
			_coefficient1Squared = value * value;
		}
	}

	/// <summary>
	/// Gets or sets the coefficient 2.
	/// </summary>
	public double Coefficient2
	{
		get
		{
			return _coefficient2;
		}
		set
		{
			_coefficient2 = value;
			_coefficient2Squared = value * value;
		}
	}

	/// <summary>
	/// Gets or sets the coefficient 3.
	/// </summary>
	public double Coefficient3
	{
		get
		{
			return _coefficient3;
		}
		set
		{
			_coefficient3 = value;
			_coefficient3Times4 = value * 4.0;
		}
	}

	/// <summary>
	/// Gets or sets the base frequency.
	/// </summary>
	public double BaseFrequency { get; set; }

	/// <summary>
	/// Gets or sets the delta frequency.
	/// </summary>
	public double DeltaFrequency { get; set; }

	/// <summary>
	/// Gets or sets the highest mass.
	/// </summary>
	public double HighestMass { get; set; }

	/// <summary>
	/// Gets or sets the (largest) segment range of the scans processed.
	/// </summary>
	public IRangeAccess SegmentRange { get; set; }

	/// <summary>
	/// Converts the given frequency to it's corresponding mass.
	/// </summary>
	/// <param name="sample">sample number to convert</param>
	/// <returns>converted mass.</returns>
	public double ConvertFrequenceToMass(int sample)
	{
		double num = BaseFrequency - (double)sample * DeltaFrequency;
		double num2 = num * num;
		double num3 = num2 * num2;
		return _coefficient1 / num + _coefficient2 / num2 + _coefficient3 / num3;
	}

	/// <summary>
	/// Converts the given mass to frequency.
	/// </summary>
	/// <param name="mass">The mass to convert</param>
	/// <returns>converted frequency</returns>
	public double ConvertMassToFrequency(double mass)
	{
		if (_coefficient1 != 0.0)
		{
			return (_coefficient1 + Math.Sqrt(_coefficient1Squared + 4.0 * _coefficient2 * mass)) / (2.0 * mass);
		}
		return Math.Sqrt((_coefficient2 + Math.Sqrt(_coefficient2Squared + _coefficient3Times4 * mass)) / (2.0 * mass));
	}
}
