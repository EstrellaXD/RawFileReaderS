using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Access to extdnded scans, which may conain charge envelope data.
/// </summary>
public interface IExtendedScanAccess : IScanAccess
{
	/// <summary>
	/// Gets centroids with additional charge envelope information (when available)
	/// </summary>
	IExtendedCentroids ExtendedCentroidsAccess { get; }
}
