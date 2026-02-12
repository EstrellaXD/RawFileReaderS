using System;
using System.Runtime.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines the UV instrument data header for UV type devices
/// </summary>
[Serializable]
[DataContract]
public class UvScanIndex : IUvScanIndex, IBaseScanIndex
{
	/// <summary>
	/// Gets or sets the frequency.
	/// <para>The Frequency will be ignored by Analog devices</para>
	/// </summary>
	[DataMember]
	public double Frequency { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether is uniform time.
	/// <para>If the Uniform flag is TRUE (time is uniform by frequency ) then there is NO time value. ex. "intensity, time"</para>
	/// <para>The IsUniformTime will be ignored by Analog devices</para>
	/// </summary>
	[DataMember]
	public bool IsUniformTime { get; set; }

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

	/// <summary>
	/// Copies the specified source.
	/// Copy all the non static fields of the current object to the new object.
	/// Since all the fields are value type, a bit-by-bit copy of the field is performed.
	/// </summary>
	/// <returns>Create a copy of the same object type</returns>
	public IUvScanIndex DeepClone()
	{
		return (IUvScanIndex)MemberwiseClone();
	}
}
