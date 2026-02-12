namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// Flags issues which may occur when a read request cannot be fulfilled
/// </summary>
public interface IReaderIssues
{
	/// <summary>
	/// Does the read request go beyond the end of the file?
	/// </summary>
	bool FileSizeExceeded { get; }

	/// <summary>
	/// Max available data (only valid when FileSizeExceeded)
	/// </summary>
	long MaxSize { get; }
}
