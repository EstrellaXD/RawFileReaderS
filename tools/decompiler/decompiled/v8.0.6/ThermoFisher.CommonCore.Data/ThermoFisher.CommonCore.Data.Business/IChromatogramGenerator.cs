namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The ChromatogramGenerator interface.
/// </summary>
public interface IChromatogramGenerator
{
	/// <summary>
	/// Create a chromatogram.
	/// </summary>
	/// <param name="scans">
	/// The scans.
	/// </param>
	/// <returns>
	/// An interface to create a signal from the intensities which are calculated.
	/// Null if no scans passed the filter.
	/// </returns>
	ISignalConvert CreatePartialChromatogram(ScanAndIndex[] scans);
}
