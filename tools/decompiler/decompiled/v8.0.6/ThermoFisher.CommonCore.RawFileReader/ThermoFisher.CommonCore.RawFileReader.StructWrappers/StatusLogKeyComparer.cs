using System;
using System.Collections.Generic;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
///     The status log key comparer.
/// </summary>
internal sealed class StatusLogKeyComparer : IComparer<StatusLogKey>
{
	/// <summary>
	/// The compare.
	/// </summary>
	/// <param name="x">
	/// The x.
	/// </param>
	/// <param name="y">
	/// The y.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	public int Compare(StatusLogKey x, StatusLogKey y)
	{
		double num = x.RetentionTime - y.RetentionTime;
		if (Math.Abs(num) < double.Epsilon)
		{
			return x.BlobIndex - y.BlobIndex;
		}
		if (!(num > 0.0))
		{
			return -1;
		}
		return 1;
	}
}
