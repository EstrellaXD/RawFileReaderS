namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The spectrum packet types.
/// Internally, within raw files, these are defined simply as "a short integer packet type"
/// These are then mapped to "constants".
/// It is possible that types may be returned by from raw data, or other transmissions, which
/// are outside of this range.
/// These types define original compression formats from instruments.
/// Note that most data values are returned as "double", when using IRawDataPlus
/// regardless of the compressed file format used.
/// </summary>
public enum SpectrumPacketType
{
	/// <summary>
	/// No packet type is being specified.
	/// This can be used as a method parameter to mean "use default".
	/// This value is not valid when in a data record within a raw file.
	/// </summary>
	NoPacketType = -1,
	/// <summary>
	/// Format for basic profiles (especially: 1990s San Jose instruments).
	/// Packet Type 0.
	/// </summary>
	ProfileSpectrum,
	/// <summary>
	/// Format for low resolution centroids (especially: 1990s San Jose instruments).
	/// Packet Type 1.
	/// </summary>
	LowResolutionSpectrum,
	/// <summary>
	/// Format for high resolution centroids (especially: 1990s San Jose instruments).
	/// Packet Type 2.
	/// </summary>
	HighResolutionSpectrum,
	/// <summary>
	/// Profile index.
	/// Index into multiple segment profile data. Legacy flag, not returned within scans.
	/// Packet Type 3.
	/// </summary>
	ProfileIndex,
	/// <summary>
	/// Compressed accurate mass spectrum, legacy mass lab instruments.
	/// Packet type 4
	/// </summary>
	CompressedAccurateSpectrum,
	/// <summary>
	/// Standard accurate mass spectrum, legacy mass lab instruments.
	/// Packet type 5
	/// </summary>
	StandardAccurateSpectrum,
	/// <summary>
	/// Standard uncalibrated spectrum, legacy mass lab instruments.
	/// Packet type 6
	/// </summary>
	StandardUncalibratedSpectrum,
	/// <summary>
	/// Accurate Mass Profile Spectrum, legacy mass lab instruments.
	/// Packet type 7
	/// </summary>
	AccurateMassProfileSpectrum,
	/// <summary>
	/// PDA UV  discrete channel packet type.
	/// Packet type 8
	/// </summary>
	PdaUvDiscreteChannel,
	/// <summary>
	/// PDA UV discrete channel index header type.
	/// (Typical for multi channel UV)
	/// Packet type 9
	/// </summary>
	PdaUvDiscreteChannelIndex,
	/// <summary>
	/// PDA UV scanned spectrum header packet type
	/// (Typical diode array detector format)
	/// Packet type 10
	/// </summary>
	PdaUvScannedSpectrum,
	/// <summary>
	/// PDA UV  scanned spectrum header index header type
	/// Packet type 11.
	/// </summary>
	PdaUvScannedSpectrumIndex,
	/// <summary>
	/// UV  channel packet type
	/// Packet type 12
	/// </summary>
	UvChannel,
	/// <summary>
	/// MS Analog channel packet type
	/// Packet type 13
	/// </summary>
	MassSpecAnalog,
	/// <summary>
	/// Profile spectrum type 2. Older San Jose instruments (LCQ) format.
	/// Packet type 14.
	/// </summary>
	ProfileSpectrumType2,
	/// <summary>
	/// Low resolution spectrum type 2. Older San Jose instruments (LCQ) format.
	/// Packet type 15.
	/// </summary>
	LowResolutionSpectrumType2,
	/// <summary>
	/// Profile spectrum type 2. Quantum format.
	/// Packet type 16.
	/// </summary>
	ProfileSpectrumType3,
	/// <summary>
	/// Low resolution spectrum type 3. Quantum format.
	/// Packet type 17.
	/// </summary>
	LowResolutionSpectrumType3,
	/// <summary>
	/// Linear Trap (centroids).
	/// This format may also return extended data from "Centroid stream".
	/// Packet type 18.
	/// </summary>
	LinearTrapCentroid,
	/// <summary>
	/// Linear Trap (profiles).
	/// This format may also return extended data from "Centroid stream".
	/// Packet type 19.
	/// </summary>
	LinearTrapProfile,
	/// <summary>
	/// FTMS data type (centroids)
	/// This format may also return extended data from "Centroid stream".
	/// Packet type 20.
	/// </summary>
	FtCentroid,
	/// <summary>
	/// FTMS data type (profiles)
	/// This format may also return extended data from "Centroid stream".
	/// Packet type 21.
	/// </summary>
	FtProfile,
	/// <summary>
	/// Compressed profile format for MAT95 (high-res)
	/// Packet type 22.
	/// </summary>
	HighResolutionCompressedProfile,
	/// <summary>
	/// Compressed profile format for MAT95
	/// Packet type 23.
	/// </summary>
	LowResolutionCompressedProfile,
	/// <summary>
	/// Low Resolution Packet type 4 (Quantum) Centroid + Flags
	/// Packet type 24.
	/// </summary>
	LowResolutionSpectrumType4,
	/// <summary>
	/// Not a known type
	/// </summary>
	InvalidPacket
}
