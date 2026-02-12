using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Holds the results of locating a single peak from chromatogram.
/// The results are a <see cref="T:ThermoFisher.CommonCore.Data.Business.LocatedPeak" /> and the chromatogram data.
/// </summary>
[Serializable]
[DataContract]
public class LocatePeakResult : ICloneable
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
	public LocatedPeak Peak { get; set; }

	/// <summary>
	/// Creates a new object that is a copy of the current instance.
	/// </summary>
	/// <returns>
	/// A new object that is a copy of this instance.
	/// </returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		return new LocatePeakResult
		{
			Chromatogram = (ChromatogramSignal)Chromatogram.Clone(),
			Peak = (LocatedPeak)Peak.Clone()
		};
	}

	/// <summary>
	/// Copy these results
	/// </summary>
	/// <param name="results">Object to fill with the results</param>
	public void CopyTo(LocatePeakResult results)
	{
		if (results == null)
		{
			throw new ArgumentNullException("results");
		}
		results.Chromatogram = (ChromatogramSignal)Chromatogram.Clone();
		results.Peak = (LocatedPeak)Peak.Clone();
	}
}
