using System;
using System.Runtime.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines the analog instrument data header for analog devices
/// </summary>
/// <seealso cref="T:ThermoFisher.CommonCore.Data.Interfaces.IAnalogScanIndex" />
[Serializable]
[DataContract]
public class AnalogScanIndex : IAnalogScanIndex, IBaseScanIndex
{
	/// <summary>
	/// Gets or sets the number of channels.
	/// </summary>
	[DataMember]
	public int NumberOfChannels { get; set; }

	/// <summary>
	/// Gets or sets the start time.
	/// </summary>
	[DataMember]
	public double StartTime { get; set; }

	/// <summary>
	/// Gets or sets the tic.
	/// </summary>
	[DataMember]
	public double TIC { get; set; }
}
