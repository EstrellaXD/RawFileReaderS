using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// A replicate is a measurement of a single calibration peak in 1 file.
/// </summary>
[Serializable]
[DataContract]
public class Replicate : IReplicate, ICloneable
{
	/// <summary>
	/// Gets or sets the amount of target compound in calibration or QC standard.
	/// </summary>
	[DataMember]
	public double Amount { get; set; }

	/// <summary>
	/// Gets or sets the response of this sample, for example: Ratio of target peak area to ISTD peak area
	/// </summary>
	[DataMember]
	public double Response { get; set; }

	/// <summary>
	/// Gets or sets the retention time of this replicate, for drift calculations
	/// </summary>
	[DataMember]
	public double RetentionTime { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to exclude this data point from the calibration curve.
	/// </summary>
	[DataMember]
	public bool ExcludeFromCalibration { get; set; }

	/// <summary>
	/// Gets or sets the first key name associated with this replicate (for example a file name)
	/// </summary>
	[DataMember]
	public string Key { get; set; }

	/// <summary>
	/// Gets or sets the second key name associated with this replicate (for example a peak or compound name)
	/// </summary>
	[DataMember]
	public string PeakKey { get; set; }

	/// <summary>
	/// Implementation of <c>ICloneable.Clone</c> method.
	/// Creates deep copy of this instance.
	/// </summary>
	/// <returns>An exact copy of the current Replicate.</returns>
	public virtual object Clone()
	{
		return (Replicate)MemberwiseClone();
	}
}
