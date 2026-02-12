using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// For advanced data LT/FT formats only. It currently uses for exporting mass spec data to raw file.
/// This will typically be used from an application.
/// </summary>
public interface IAdvancedPacketData
{
	/// <summary>
	/// Gets the noise data
	/// </summary>
	NoiseAndBaseline[] NoiseData { get; }

	/// <summary>
	/// Gets the frequencies.  
	/// The values are for computing mass from frequency during exporting mass spec on compressing.
	/// </summary>
	double[] Frequencies { get; }

	/// <summary>
	/// Gets the centroid data (label peaks) information. Only FT type packets have label peaks, others no.
	/// </summary>
	CentroidStream CentroidData { get; }
}
