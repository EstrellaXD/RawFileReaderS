namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to define "obtaining a value for a scan".
/// For example: for a "average peak from mass 20-100" a dervied class 
/// would provide the mass range setting needed and an implementaion
/// of ValueForScan which calculates this from the provided scan data
/// </summary>
public interface IScanValueProvider
{
	/// <summary>
	/// provides a calculated value from 
	/// the (mass, intensity) data in a scan
	/// </summary>
	/// <param name="scanData">Scan and it's header</param>
	/// <returns>the chromatogram intensity at this scan</returns>
	double ValueForScan(ISimpleScanWithHeader scanData);
}
