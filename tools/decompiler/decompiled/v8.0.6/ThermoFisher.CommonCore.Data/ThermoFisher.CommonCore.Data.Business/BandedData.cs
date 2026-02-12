using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Results of banding algorithm.
/// Data for a bar chart
/// </summary>
public class BandedData
{
	/// <summary>
	/// Gets or sets the number of bands.
	/// </summary>
	public int Bands { get; set; }

	/// <summary>
	/// Gets or sets the width of a band.
	/// </summary>
	public double BandWidth { get; set; }

	/// <summary>
	/// Gets or sets the center of each band
	/// </summary>
	public double[] BandCenters { get; set; }

	/// <summary>
	/// Gets or sets the data (height of the bar) for each band
	/// </summary>
	public double[] BandData { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this data got banded.
	/// For example: If a data set was integer, and contained "charges 1 to 7" then all 7 
	/// original data categories can be returned.
	/// If data is "double from 1..02-9000.6" then there are no fixed categories, and the data
	/// will be "sampled" into bands.
	/// </summary>
	public bool Banded { get; set; }

	/// <summary>
	/// Calculates the range of a given band
	/// </summary>
	/// <param name="band">band whose range is needed</param>
	/// <returns>The range of value in this band</returns>
	public IRangeAccess BandRange(int band)
	{
		return Range.CreateFromCenterAndDelta(BandCenters[band], BandWidth / 2.0);
	}
}
