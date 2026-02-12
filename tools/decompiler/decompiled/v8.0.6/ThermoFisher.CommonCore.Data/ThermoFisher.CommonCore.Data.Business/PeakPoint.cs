using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// A record of the signal at a certain point in a detected peak.
/// For example, the Apex.
/// </summary>
[Serializable]
public struct PeakPoint
{
	private double _rT;

	private double _peakHeight;

	private double _baselineHeight;

	/// <summary>
	/// Gets or sets the retention time at this point, in minutes.
	/// </summary>
	public double RetentionTime
	{
		get
		{
			return _rT;
		}
		set
		{
			_rT = value;
		}
	}

	/// <summary>
	/// Gets or sets the intensity minus baseline at RT
	/// </summary>
	public double HeightAboveBaseline
	{
		get
		{
			return _peakHeight;
		}
		set
		{
			_peakHeight = value;
		}
	}

	/// <summary>
	/// Gets or sets the baseline height at RT
	/// </summary>
	public double BaselineHeight
	{
		get
		{
			return _baselineHeight;
		}
		set
		{
			_baselineHeight = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.PeakPoint" /> struct. 
	/// Create A new point on a detected peak
	/// </summary>
	/// <param name="retentionTime">
	/// Retention time at this point
	/// </param>
	/// <param name="height">
	/// The signal height at this point above the baseline (signal.intensity - baseline)
	/// </param>
	/// <param name="baseline">
	/// baseline height at retentionTime
	/// </param>
	public PeakPoint(double retentionTime, double height, double baseline)
	{
		_rT = retentionTime;
		_peakHeight = height;
		_baselineHeight = baseline;
	}

	/// <summary>
	/// Test that two peak points are equal
	/// </summary>
	/// <param name="left">first point to compare</param>
	/// <param name="right">second point to compare</param>
	/// <returns>true if they have the same contents</returns>
	public static bool operator ==(PeakPoint left, PeakPoint right)
	{
		return left.Equals(right);
	}

	/// <summary>
	/// Test that two peak points are not equal
	/// </summary>
	/// <param name="left">first point to compare</param>
	/// <param name="right">second point to compare</param>
	/// <returns>true if they do not have the same contents</returns>
	public static bool operator !=(PeakPoint left, PeakPoint right)
	{
		return !left.Equals(right);
	}

	/// <summary>
	/// Test two peaks for equality
	/// </summary>
	/// <param name="obj">
	/// peak to compare
	/// </param>
	/// <returns>
	/// true if they are equal
	/// </returns>
	public override bool Equals(object obj)
	{
		PeakPoint peakPoint = (PeakPoint)obj;
		if (Math.Abs(_rT - peakPoint._rT) < 1E-30 && Math.Abs(_peakHeight - peakPoint._peakHeight) < 1E-30)
		{
			return Math.Abs(_baselineHeight - peakPoint._baselineHeight) < 1E-30;
		}
		return false;
	}

	/// <summary>
	/// <see>Object.GetHashCode</see>
	/// </summary>
	/// <returns>The hash code</returns>
	public override int GetHashCode()
	{
		return (int)(Math.Min(_rT * 50.0, 10000.0) + Math.Min(_peakHeight / 100.0, 100000.0) * 10000.0);
	}
}
