using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// class to represent spectrum data
/// </summary>
[Serializable]
public class SpectrumPoint
{
	private double _intensity;

	private double _mass;

	/// <summary>
	/// Gets or sets Intensity of spectral peak
	/// </summary>
	public double Intensity
	{
		get
		{
			return _intensity;
		}
		set
		{
			_intensity = value;
		}
	}

	/// <summary>
	/// Gets or sets Mass of spectral peak
	/// </summary>
	public double Mass
	{
		get
		{
			return _mass;
		}
		set
		{
			_mass = value;
		}
	}

	/// <summary>
	/// Test that two spec points are equal
	/// </summary>
	/// <param name="left">first point to compare</param>
	/// <param name="right">second point to compare</param>
	/// <returns>true if they have the same contents</returns>
	public static bool operator ==(SpectrumPoint left, SpectrumPoint right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Test that two spec points are not equal
	/// </summary>
	/// <param name="left">first point to compare</param>
	/// <param name="right">second point to compare</param>
	/// <returns>true if they do not have the same contents</returns>
	public static bool operator !=(SpectrumPoint left, SpectrumPoint right)
	{
		return !left.Equals(right);
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
		SpectrumPoint spectrumPoint = (SpectrumPoint)obj;
		if (_intensity == spectrumPoint.Intensity)
		{
			return _mass == spectrumPoint.Mass;
		}
		return false;
	}

	/// <summary>
	/// <see>Object.GetHashCode</see>
	/// </summary>
	/// <returns><see>Object.GetHashCode</see></returns>
	public override int GetHashCode()
	{
		return (int)(Math.Min(_mass * 50.0, 10000.0) + Math.Min(_intensity / 1000.0, 100000.0) * 10000.0);
	}
}
