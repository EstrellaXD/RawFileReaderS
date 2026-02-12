using System;
using System.Runtime.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines the PDA scan header and wavelength settings
/// </summary>
[Serializable]
[DataContract]
public class PdaScanIndex : IPdaScanIndex, IBaseScanIndex
{
	/// <summary>
	/// Gets or sets the start time.
	/// </summary>
	[DataMember]
	public double StartTime { get; set; }

	/// <summary>
	/// Gets or sets the TIC.
	/// </summary>
	[DataMember]
	public double TIC { get; set; }

	/// <summary>
	/// Gets or sets the long wavelength.
	/// </summary>
	[DataMember]
	public double LongWavelength { get; set; }

	/// <summary>
	/// Gets or sets the short wavelength.
	/// </summary>
	[DataMember]
	public double ShortWavelength { get; set; }

	/// <summary>
	/// Gets or sets the wave length step.
	/// </summary>
	[DataMember]
	public double WavelengthStep { get; set; }

	/// <summary>
	/// Gets or sets the Absorbance Unit's scale.
	/// </summary>
	[DataMember]
	public double AUScale { get; set; }
}
