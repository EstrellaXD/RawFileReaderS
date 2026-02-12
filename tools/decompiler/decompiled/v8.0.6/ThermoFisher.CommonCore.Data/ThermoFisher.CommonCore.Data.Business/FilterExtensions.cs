using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The filter extensions.
/// These are internal, as they are only
/// supported for use by the ScanEventHelper class.
/// </summary>
internal static class FilterExtensions
{
	/// <summary>
	/// Calculate the filter mass resolution.
	/// </summary>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Double" />.
	/// </returns>
	public static double FilterMassResolution(this IScanFilter filter)
	{
		return filter.MassPrecision switch
		{
			0 => 1.0, 
			1 => 0.1, 
			2 => 0.01, 
			3 => 0.001, 
			4 => 0.0001, 
			5 => 5E-05, 
			_ => 0.001, 
		};
	}

	/// <summary>
	/// Calculates the number of masses, corrected for multiple activations.
	/// </summary>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	public static int NumMassesEx(this IScanFilter filter)
	{
		int massCount = filter.MassCount;
		int num = massCount;
		for (int i = 0; i < massCount; i++)
		{
			if (filter.GetIsMultipleActivation(i))
			{
				num--;
			}
		}
		return num;
	}

	/// <summary>
	/// Test if the compensation voltage value is valid.
	/// </summary>
	/// <param name="filter">
	/// The filter.
	/// </param>
	/// <param name="i">
	/// The i.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public static bool CompensationVoltageValueIsValid(this IScanFilter filter, int i)
	{
		return i < filter.CompensationVoltageCount;
	}
}
