namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines all information which would be needed to decode a scan.
/// This includes the scan binary data, the scan "packet format" to decode and any mass calibrators needed
/// </summary>
public interface IEncodedScan
{
	/// <summary>
	/// Gets the binary form of the scan's data such as (m/z, intensity).
	/// All parts of the scan may be decoded from this (centroid, profile etc.)
	/// </summary>
	byte[] ScanData { get; }

	/// <summary>
	/// Gets the mass calibrator table, which is used by some of the scan formats
	/// that do not directly record masses.
	/// </summary>
	double[] MassCalibrators { get; }

	/// <summary>
	/// Gets information about the scan, including te format code which needs to be decoded.
	/// </summary>
	IMsScanIndexAccess ScanIndex { get; }
}
