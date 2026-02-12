using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Determines how a component peak has been selected from a set of possible peaks
/// </summary>
[DataContract]
public enum PeakMethod
{
	/// <summary>
	/// search was by spectral fit.
	/// </summary>
	[EnumMember]
	SpectralFit,
	/// <summary>
	/// search was by largest peak in the search window.
	/// </summary>
	[EnumMember]
	Highest,
	/// <summary>
	/// search was by nearest retention time.
	/// </summary>
	[EnumMember]
	Nearest,
	/// <summary>
	/// search was by spectral purity.
	/// </summary>
	[EnumMember]
	Purity,
	/// <summary>
	/// peak was detected by the generic chromatogram peak detector.
	/// </summary>
	[EnumMember]
	Generic,
	/// <summary>
	/// Peak was reworked from the original (for example manual)
	/// </summary>
	[EnumMember]
	Reworked
}
