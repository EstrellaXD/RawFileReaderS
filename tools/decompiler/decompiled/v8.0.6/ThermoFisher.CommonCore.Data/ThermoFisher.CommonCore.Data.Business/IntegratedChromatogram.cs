using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Holds the results of integrating a single chromatogram.
/// The results are a peak list and the chromatogram data.
/// </summary>
[Serializable]
[DataContract]
public class IntegratedChromatogram : ICloneable
{
	/// <summary>
	/// Gets or sets the data read from the raw file
	/// </summary>
	[DataMember]
	public ChromatogramSignal Chromatogram { get; set; }

	/// <summary>
	/// Gets or sets the peaks found in the chromatogram
	/// </summary>
	[DataMember]
	public ItemCollection<Peak> Peaks { get; set; }

	/// <summary>
	/// Creates a new object that is a copy of the current instance.
	/// </summary>
	/// <returns>
	/// A new object that is a copy of this instance.
	/// </returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		return new IntegratedChromatogram
		{
			Chromatogram = (ChromatogramSignal)Chromatogram.Clone(),
			Peaks = (ItemCollection<Peak>)Peaks.Clone()
		};
	}
}
