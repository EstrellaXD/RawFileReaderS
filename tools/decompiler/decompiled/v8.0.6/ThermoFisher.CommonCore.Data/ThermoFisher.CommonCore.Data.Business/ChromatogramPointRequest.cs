using System.Linq;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Request to create a point in a chromatogram, based on mass/intensity data in a scan.
/// Use the static methods to make common chromatogram types.
/// </summary>
public class ChromatogramPointRequest : IChromatogramPointRequest
{
	/// <summary>
	/// unknown calculation for a chromatogram point (delegated)
	/// </summary>
	private class CustomPointRequest : IChromatogramPointRequest
	{
		/// <summary>
		/// get or sets an injected object to provide a custom calculated value for a scan
		/// </summary>
		public IScanValueProvider ValueProvider { get; internal set; }

		/// <summary>
		/// Gets or sets the scale factor for this point (needs to be 1.0)
		/// </summary>
		public double Scale { get; set; } = 1.0;

		public bool AllData { get; set; }

		public IRangeAccess MassRange { get; set; }

		public ChromatogramPointMode PointMode { get; set; }

		/// <inheritdoc />
		public double DataForPoint(ISimpleScanWithHeader scanWithHeader)
		{
			return ValueProvider.ValueForScan(scanWithHeader);
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether all data
	/// in the scan is used, or just a mass range.
	/// </summary>
	public bool AllData { get; set; }

	/// <summary>
	/// Gets or sets the scale.
	/// This can be 1 to "add data in a mass range" or
	/// -1 to "subtract data a mass range",
	/// or any other value to apply scaling to a range.
	/// </summary>
	public double Scale { get; set; }

	/// <summary>
	/// Gets or sets the mass range.
	/// If an application has a center mass +/ tolerance,
	/// then the static method <see cref="M:ThermoFisher.CommonCore.Data.Business.ChromatogramPointRequest.SingleIonRequest(System.Double,System.Double,ThermoFisher.CommonCore.Data.ToleranceMode)" /> may be useful to format the range.
	/// </summary>
	public IRangeAccess MassRange { get; set; }

	/// <summary>
	/// Gets or sets the rule for how a chromatogram point is created from a mass range.
	/// </summary>
	public ChromatogramPointMode PointMode { get; set; }

	/// <inheritdoc />
	public virtual double DataForPoint(ISimpleScanWithHeader scanWithHeader)
	{
		ISimpleScanAccess data = scanWithHeader.Data;
		if (AllData)
		{
			switch (PointMode)
			{
			case ChromatogramPointMode.Sum:
				return data.Intensities.Sum();
			case ChromatogramPointMode.Max:
			{
				double[] intensities = data.Intensities;
				if (intensities != null && intensities.Length != 0)
				{
					return intensities.Max();
				}
				return 0.0;
			}
			case ChromatogramPointMode.Mass:
				return data.MassAtLargestIntensity();
			}
		}
		else
		{
			IRangeAccess massRange = MassRange;
			switch (PointMode)
			{
			case ChromatogramPointMode.Sum:
				return data.IntensitySum(massRange.Low, massRange.High);
			case ChromatogramPointMode.Max:
				return data.LargestIntensity(massRange.Low, massRange.High);
			case ChromatogramPointMode.Mass:
				return data.MassAtLargestIntensity(massRange.Low, massRange.High);
			}
		}
		return 0.0;
	}

	/// <summary>
	/// Create a request to make an "XIC" chromatogram, based on one ion plus mass tolerance.
	/// </summary>
	/// <param name="mass">
	/// The mass.
	/// </param>
	/// <param name="tolerance">
	/// The tolerance.
	/// </param>
	/// <param name="mode">
	/// The tolerance mode.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramPointRequest" />.
	/// </returns>
	public static ChromatogramPointRequest SingleIonRequest(double mass, double tolerance, ToleranceMode mode)
	{
		return new ChromatogramPointRequest
		{
			AllData = false,
			PointMode = ChromatogramPointMode.Sum,
			MassRange = RangeFactory.CreateFromCenterAndDelta(mass, tolerance * ToleranceFactor(mass, mode))
		};
	}

	/// <summary>
	/// Create a request to make an "XIC" chromatogram, based on a mass range
	/// </summary>
	/// <param name="lowMass">
	/// The low mass.
	/// </param>
	/// <param name="highMass">
	/// The high mass.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramPointRequest" />.
	/// </returns>
	public static ChromatogramPointRequest MassRangeRequest(double lowMass, double highMass)
	{
		return new ChromatogramPointRequest
		{
			AllData = false,
			PointMode = ChromatogramPointMode.Sum,
			MassRange = RangeFactory.Create(lowMass, highMass)
		};
	}

	/// <summary>
	/// Create a request to make a "TIC" chromatogram.
	/// </summary>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramPointRequest" />.
	/// </returns>
	public static ChromatogramPointRequest TotalIonRequest()
	{
		return new ChromatogramPointRequest
		{
			AllData = true,
			PointMode = ChromatogramPointMode.Sum
		};
	}

	/// <summary>
	/// Create a request to make a "base peak in scan" chromatogram.
	/// </summary>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramPointRequest" />.
	/// </returns>
	public static ChromatogramPointRequest BasePeakRequest()
	{
		return new ChromatogramPointRequest
		{
			AllData = true,
			PointMode = ChromatogramPointMode.Max
		};
	}

	/// <summary>
	/// Create a request to make an "base peak" chromatogram, based on a mass range.
	/// That is: returns the most intense peak over a given range.
	/// </summary>
	/// <param name="lowMass">
	/// The low mass.
	/// </param>
	/// <param name="highMass">
	/// The high mass.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramPointRequest" />.
	/// </returns>
	public static ChromatogramPointRequest BasePeakOverMassRangeRequest(double lowMass, double highMass)
	{
		return new ChromatogramPointRequest
		{
			AllData = false,
			PointMode = ChromatogramPointMode.Max,
			MassRange = RangeFactory.Create(lowMass, highMass)
		};
	}

	/// <summary>
	/// Create a request to make an "base peak" mass chromatogram, based on a mass range.
	/// That is: returns the mass of most intense peak over a given range.
	/// </summary>
	/// <param name="lowMass">
	/// The low mass.
	/// </param>
	/// <param name="highMass">
	/// The high mass.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramPointRequest" />.
	/// </returns>
	public static ChromatogramPointRequest BasePeakMassOverMassRangeRequest(double lowMass, double highMass)
	{
		return new ChromatogramPointRequest
		{
			AllData = false,
			PointMode = ChromatogramPointMode.Mass,
			MassRange = RangeFactory.Create(lowMass, highMass)
		};
	}

	/// <summary>
	/// Create a request to make a "mass of base peak in scan" chromatogram.
	/// </summary>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramPointRequest" />.
	/// </returns>
	public static ChromatogramPointRequest BasePeakMassRequest()
	{
		return new ChromatogramPointRequest
		{
			AllData = true,
			PointMode = ChromatogramPointMode.Mass
		};
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramPointRequest" /> class. 
	/// </summary>
	public ChromatogramPointRequest()
	{
		Scale = 1.0;
	}

	/// <summary>
	/// Calculate tolerance factor, applied to supplied tolerance value, based on mode.
	/// </summary>
	/// <param name="mass">
	/// The mass.
	/// </param>
	/// <param name="mode">
	/// The mode.
	/// </param>
	/// <returns>
	/// The factor to apply.
	/// </returns>
	private static double ToleranceFactor(double mass, ToleranceMode mode)
	{
		return mode switch
		{
			ToleranceMode.Mmu => 0.001, 
			ToleranceMode.Ppm => mass / 1000000.0, 
			_ => 1.0, 
		};
	}

	/// <summary>
	/// Create a request to make a "neutral fragment" chromatogram.
	/// If this is a "fixed fragment" based on a given MS/MS scan filter,
	/// the lowMass is positive, and the range is assumed to be "filter MS/MS mass - neutral mass".
	/// If the chromatogram has no MS/MS mass, then
	/// the lowMass = -neural -0.5 and the highMass is -neutral +0.5
	/// To get amass range for a given scan,
	/// the MS/MS mass from that scan's event must be added to both low and high masses
	/// </summary>
	/// <param name="lowMass">Low mass of range</param>
	/// <param name="highMass">High mass of range</param>
	/// <param name="toleranceOptions">Mass tolerance (applied to precursor in target scan). Defaults to 0.5 amu when null</param>
	/// <returns>Interface to generate a neural fragment chromatogram point</returns>
	public static IChromatogramPointRequest FragmentRequest(double lowMass, double highMass, MassOptions toleranceOptions = null)
	{
		return new NeutralChromatogramPointRequest
		{
			AllData = false,
			PointMode = ChromatogramPointMode.Fragment,
			MassRange = RangeFactory.Create(lowMass, highMass),
			Tolerance = (toleranceOptions ?? new MassOptions
			{
				Tolerance = 0.5,
				Precision = 2,
				ToleranceUnits = ToleranceUnits.amu
			})
		};
	}

	/// <summary>
	/// Create a chromatogram point using a custom algorithm
	/// </summary>
	/// <param name="provider">An algorithm which calculates a value from a scan</param>
	/// <returns>An object to calculate a custom value for a scan</returns>
	public static IChromatogramPointRequest CustomRequest(IScanValueProvider provider)
	{
		return new CustomPointRequest
		{
			ValueProvider = provider
		};
	}
}
