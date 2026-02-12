using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// This class partially unpacks MS instruments data after being transmitted (usually by binary message protocol)
/// </summary>
internal class UnpackedMsInstrumentData : IBinaryMsInstrumentData
{
	/// <inheritdoc />
	public byte[] PackedScanEvent { get; internal set; }

	/// <inheritdoc />
	public IScanStatisticsAccess StatisticsData { get; internal set; }

	/// <inheritdoc />
	public byte[] ScanData { get; internal set; }

	/// <inheritdoc />
	public int ProfileIndexCount { get; internal set; }

	/// <inheritdoc />
	public byte[] ProfileData { get; internal set; }
}
