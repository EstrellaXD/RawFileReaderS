using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Access to data in a scan
/// </summary>
public interface IScanAccess
{
	/// <summary>
	/// Gets or sets a value indicating whether, when requesting "Preferred data", the centroid stream will be returned.
	/// For example "<see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanAccess.PreferredMasses" />", "<see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanAccess.PreferredIntensities" />".
	/// If this property is false, or there is no centroid stream, then these methods will return
	/// the data from <see cref="T:ThermoFisher.CommonCore.Data.Business.SegmentedScan" />. For greater efficiency, callers should cache the return of "<see cref="P:ThermoFisher.CommonCore.Data.Interfaces.IScanAccess.PreferredMasses" />".
	/// Typically data processing, such as elemental compositions, should use these methods.
	/// </summary>
	bool PreferCentroids { get; set; }

	/// <summary>
	/// Gets the Mass for default data stream (usually centroid stream, if present).
	/// Falls back to <see cref="T:ThermoFisher.CommonCore.Data.Business.SegmentedScan" /> data if centroid stream is not preferred or not present 
	/// </summary>
	double[] PreferredMasses { get; }

	/// <summary>
	/// Gets Resolutions for default data stream (usually centroid stream, if present).
	/// Returns an empty array if centroid stream is not preferred or not present 
	/// </summary>
	double[] PreferredResolutions { get; }

	/// <summary>
	/// Gets Noises for default data stream (usually centroid stream, if present).
	/// Returns an empty array if centroid stream is not preferred or not present 
	/// </summary>
	double[] PreferredNoises { get; }

	/// <summary>
	/// Gets Intensity for default data stream (usually centroid stream, if present).
	/// Falls back to <see cref="T:ThermoFisher.CommonCore.Data.Business.SegmentedScan" /> data if centroid stream is not preferred or not present 
	/// </summary>
	double[] PreferredIntensities { get; }

	/// <summary>
	/// Gets peak flags (such as saturated) for default data stream (usually centroid stream, if present).
	/// Falls back to <see cref="T:ThermoFisher.CommonCore.Data.Business.SegmentedScan" /> data if centroid stream is not preferred or not present 
	/// </summary>
	PeakOptions[] PreferredFlags { get; }

	/// <summary>
	/// Gets Mass of base peak default data stream (usually centroid stream, if present).
	/// Falls back to <see cref="T:ThermoFisher.CommonCore.Data.Business.SegmentedScan" /> data if centroid stream is not preferred or not present 
	/// </summary>
	double PreferredBasePeakMass { get; }

	/// <summary>
	/// Gets Resolution of base peak for default data stream (usually centroid stream, if present).
	/// Falls back to zero if centroid stream is not preferred or not present 
	/// </summary>
	double PreferredBasePeakResolution { get; }

	/// <summary>
	/// Gets Noise of base peak for default data stream (usually centroid stream, if present).
	/// Falls back to zero if centroid stream is not preferred or not present 
	/// </summary>
	double PreferredBasePeakNoise { get; }

	/// <summary>
	/// Gets peak flags (such as saturated) for default data stream (usually centroid stream, if present).
	/// Falls back to <see cref="T:ThermoFisher.CommonCore.Data.Business.SegmentedScan" /> data if centroid stream is not preferred or not present 
	/// </summary>
	[XmlIgnore]
	double PreferredBasePeakIntensity { get; }

	/// <summary>
	/// Gets The data for the scan
	/// </summary>
	ISegmentedScanAccess SegmentedScanAccess { get; }

	/// <summary>
	/// Gets A second data stream for the scan
	/// </summary>
	ICentroidStreamAccess CentroidStreamAccess { get; }

	/// <summary>
	/// Gets a value indicating whether this scan has a centroid stream.
	/// </summary>
	bool HasCentroidStream { get; }

	/// <summary>
	/// Gets or Header information for the scan
	/// </summary>
	IScanStatisticsAccess ScanStatisticsAccess { get; }

	/// <summary>
	/// Gets Type of scan (for filtering)
	/// </summary>
	string ScanType { get; }

	/// <summary>
	/// Gets the Tolerance value.
	/// </summary>
	ToleranceMode ToleranceUnit { get; }

	/// <summary>
	/// Gets the mass resolution for all scan arithmetic operations
	/// </summary>
	double MassResolution { get; }

	/// <summary>
	/// Gets a value indicating whether the User Tolerance value is being used.
	/// </summary>
	bool IsUserTolerance { get; }
}
