using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Factory to produce immutable ranges of double
/// </summary>
public static class RangeFactory
{
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
	public static IRangeAccess Create(double low, double high)
	{
		return new Range(low, high);
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
	public static IRangeAccess CreateFromCenterAndDelta(double center, double delta)
	{
		return new Range(center - delta, center + delta);
	}

	/// <summary>
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
	public static IRangeAccess CreateFromRangeAndTolerance(IRangeAccess from, MassOptions toleranceOptions)
	{
		return new Range(from, toleranceOptions);
	}

	/// <summary>
	/// Construct a range from another range, adding a tolerance if ends are the same
	/// </summary>
	/// <param name="from">
	/// range to copy
	/// </param>
	/// <param name="tolerance">
	/// If limits are same (with 1e-10)
	/// this is subtracted from low and added to high of the new range
	/// </param>
	public static IRangeAccess CreateFromRangeAndTolerance(IRangeAccess from, double tolerance)
	{
		return new Range(from, tolerance);
	}
}
