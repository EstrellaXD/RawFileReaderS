using System;
using System.Collections.Generic;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// The comparer.
/// </summary>
/// <typeparam name="T">
/// Type of objects compared
/// </typeparam>
internal class Comparer<T> : IComparer<T>
{
	private readonly Comparison<T> _comparison;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Comparer`1" /> class.
	/// </summary>
	/// <param name="comparison">
	/// The comparison.
	/// </param>
	public Comparer(Comparison<T> comparison)
	{
		_comparison = comparison;
	}

	/// <summary>
	/// Compare x and y.
	/// </summary>
	/// <param name="x">
	/// The x.
	/// </param>
	/// <param name="y">
	/// The y.
	/// </param>
	/// <returns>
	/// The standard comparison result (from IComparer).
	/// </returns>
	public int Compare(T x, T y)
	{
		if ((object)x != (object)y)
		{
			return _comparison(x, y);
		}
		return 0;
	}
}
