using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// A range of double Low, High.
/// </summary>
[Serializable]
public class Range : IComparable<Range>, IRangeAccess
{
	private const double VerrySmall = 1E-10;

	/// <summary>
	/// Gets or sets the low end of range
	/// </summary>
	public double Low { get; set; }

	/// <summary>
	/// Gets or sets the high end of range
	/// </summary>
	public double High { get; set; }

	/// <summary>
	/// Implements the operator ==.
	/// </summary>
	/// <param name="first">The first.</param>
	/// <param name="second">The second.</param>
	/// <returns>The result of the operator.</returns>
	public static bool operator ==(Range first, Range second)
	{
		return first?.Equals(second) ?? ((object)second == null);
	}

	/// <summary>
	/// Implements the operator !=.
	/// </summary>
	/// <param name="first">The first.</param>
	/// <param name="second">The second.</param>
	/// <returns>The result of the operator.</returns>
	public static bool operator !=(Range first, Range second)
	{
		return !(first == second);
	}

	/// <summary>
	/// Create an immutable (constant) range from center and delta, such that the range is center +/- delta.
	/// </summary>
	/// <param name="center">
	/// The center.
	/// </param>
	/// <param name="delta">
	/// The delta.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.Range" />.
	/// </returns>
	public static Range CreateFromCenterAndDelta(double center, double delta)
	{
		return new Range(center - delta, center + delta);
	}

	/// <summary>
	/// Create an immutable (constant) range from low and high.
	/// </summary>
	/// <param name="low">
	/// The low.
	/// </param>
	/// <param name="high">
	/// The high.
	/// </param>
	/// <returns>
	/// The range.
	/// </returns>
	public static Range Create(double low, double high)
	{
		return new Range(low, high);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Range" /> class.
	/// </summary>
	public Range()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Range" /> class. 
	/// Construct a range from limits
	/// </summary>
	/// <param name="low">
	/// low limit of range
	/// </param>
	/// <param name="high">
	/// High limit of range
	/// </param>
	public Range(double low, double high)
	{
		Low = low;
		High = high;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Range" /> class. 
	/// Construct a range from another range.
	/// </summary>
	/// <param name="from">
	/// range to copy
	/// </param>
	public Range(IRangeAccess from)
	{
		Low = from.Low;
		High = from.High;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Range" /> class. 
	/// Construct a range from another range, adding a tolerance if ends are the same
	/// </summary>
	/// <param name="from">
	/// range to copy
	/// </param>
	/// <param name="tolerance">
	/// If limits are same (with 1e-10)
	/// this is subtracted from low and added to high of the new range
	/// </param>
	public Range(IRangeAccess from, double tolerance)
	{
		Low = from.Low;
		High = from.High;
		if (High - Low < 1E-10)
		{
			Low -= tolerance;
			High += tolerance;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Range" /> class. 
	/// Construct a range from another range, adding a tolerance if ends are the same
	/// (within 1.0E-10).
	/// </summary>
	/// <param name="from">
	/// range to copy
	/// </param>
	/// <param name="toleranceOptions">
	/// If limits are same (within 1e-10)
	/// the tolerance is subtracted from low and added to high of the new range
	/// </param>
	public Range(IRangeAccess from, MassOptions toleranceOptions)
	{
		if (from == null)
		{
			throw new ArgumentNullException("from");
		}
		if (toleranceOptions == null)
		{
			throw new ArgumentNullException("toleranceOptions");
		}
		Low = from.Low;
		High = from.High;
		if (High - Low < 1E-10)
		{
			double toleranceAtMass = toleranceOptions.GetToleranceAtMass(Low);
			Low -= toleranceAtMass;
			High += toleranceAtMass;
		}
	}

	/// <summary>
	/// Test for inclusion.
	/// </summary>
	/// <param name="value">
	/// The value.
	/// </param>
	/// <returns>
	/// True if in range
	/// </returns>
	public bool Includes(double value)
	{
		if (value >= Low)
		{
			return value <= High;
		}
		return false;
	}

	/// <summary>
	/// Compares the current object with another object of the same type.
	/// </summary>
	/// <returns>
	/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: 
	///                     Value 
	///                     Meaning 
	///                     Less than zero 
	///                     This object is less than the <paramref name="other" /> parameter.
	///                     Zero 
	///                     This object is equal to <paramref name="other" />. 
	///                     Greater than zero 
	///                     This object is greater than <paramref name="other" />. 
	/// </returns>
	/// <param name="other">An object to compare with this object.
	/// </param>
	public int CompareTo(Range other)
	{
		if (Low > other.Low)
		{
			return 1;
		}
		if (Low < other.Low)
		{
			return -1;
		}
		if (High > other.High)
		{
			return 1;
		}
		if (High < other.High)
		{
			return -1;
		}
		return 0;
	}

	/// <summary>
	/// Indicates whether this instance and a specified object are equal.
	/// </summary>
	/// <param name="obj">Another object to compare to.</param>
	/// <returns>
	/// true if <paramref name="obj" /> and this instance are the same type and represent the same value; otherwise, false.
	/// </returns>
	public override bool Equals(object obj)
	{
		if (obj != null && obj is IRangeAccess rangeAccess)
		{
			if (rangeAccess.High == High)
			{
				return rangeAccess.Low == Low;
			}
			return false;
		}
		return false;
	}

	/// <summary>
	/// Returns the hash code for this instance.
	/// </summary>
	/// <returns>
	/// A 32-bit signed integer that is the hash code for this instance.
	/// </returns>
	public override int GetHashCode()
	{
		return High.GetHashCode() ^ Low.GetHashCode();
	}
}
