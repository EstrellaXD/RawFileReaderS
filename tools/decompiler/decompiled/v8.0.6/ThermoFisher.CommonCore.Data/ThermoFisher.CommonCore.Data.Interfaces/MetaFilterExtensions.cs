namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Helpers for meta filter encode/decode
/// </summary>
public static class MetaFilterExtensions
{
	/// <summary>
	/// The bit shift for the "count" for the MSn value
	/// </summary>
	public const int CountShift = 8;

	/// <summary>
	/// Gets the MSn order of the meta filter
	/// </summary>
	/// <param name="filterType">Meta filter codes</param>
	/// <returns>MSn order</returns>
	public static int MSnCount(this MetaFilterType filterType)
	{
		return (int)(filterType & MetaFilterType.MSnCountMask) >> 8;
	}
}
