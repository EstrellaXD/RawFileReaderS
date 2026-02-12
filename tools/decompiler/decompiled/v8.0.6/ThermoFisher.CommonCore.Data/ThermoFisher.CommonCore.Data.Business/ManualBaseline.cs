using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Information returned when a peak baseline is manually adjusted on a chromatogram
/// </summary>
[Serializable]
[DataContract]
public class ManualBaseline : CommonCoreDataObject, ICloneable
{
	/// <summary>
	/// Gets or sets x (time) of new baseline start point (left of peak)
	/// </summary>
	[DataMember]
	public double LeftTime { get; set; }

	/// <summary>
	/// Gets or sets x (time) of new baseline end point (right of peak)
	/// </summary>
	[DataMember]
	public double RightTime { get; set; }

	/// <summary>
	/// Gets or sets y (intensity) of new baseline start point (left of peak)
	/// </summary>
	[DataMember]
	public double LeftIntensity { get; set; }

	/// <summary>
	/// Gets or sets y (intensity) of new baseline end point (right of peak)
	/// </summary>
	[DataMember]
	public double RightIntensity { get; set; }

	/// <summary>
	/// Copy this object
	/// </summary>
	/// <returns>copy of object</returns>
	public object Clone()
	{
		return MemberwiseClone();
	}
}
