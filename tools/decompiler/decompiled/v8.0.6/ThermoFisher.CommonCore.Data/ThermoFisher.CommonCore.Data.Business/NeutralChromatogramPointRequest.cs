using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// A request for a Neutral loss chromatogram
/// </summary>
public class NeutralChromatogramPointRequest : ChromatogramPointRequest
{
	/// <summary>
	/// Gets or sets tolerance applied to precursor mass
	/// </summary>
	public MassOptions Tolerance { get; set; }

	/// <summary>
	/// Find the data for one scan.
	/// </summary>
	/// <param name="scanWithHeader">
	/// The scan, including header and scan event.
	/// </param>
	/// <returns>
	/// The chromatogram point value for this scan.
	/// </returns>
	public override double DataForPoint(ISimpleScanWithHeader scanWithHeader)
	{
		ISimpleScanAccess data = scanWithHeader.Data;
		IRangeAccess massRange = base.MassRange;
		if (massRange.Low < 0.0)
		{
			IScanEvent scanEvent = scanWithHeader.Event;
			double num = 0.0 - massRange.Low;
			if (scanEvent.MSOrder >= MSOrderType.Ms2 && scanEvent.MassCount >= 1)
			{
				double mass = scanEvent.GetMass(scanEvent.MassCount - 1);
				double toleranceAtMass = Tolerance.GetToleranceAtMass(mass);
				double num2 = mass - num;
				return data.LargestIntensity(num2 - toleranceAtMass, num2 + toleranceAtMass);
			}
			return 0.0;
		}
		return data.LargestIntensity(massRange.Low, massRange.High);
	}
}
