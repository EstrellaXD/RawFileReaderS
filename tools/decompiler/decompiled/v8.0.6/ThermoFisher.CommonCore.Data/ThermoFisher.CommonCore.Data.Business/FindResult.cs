using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Results of the find algorithm
/// </summary>
[Serializable]
[DataContract]
public class FindResult : ICloneable, IFindResultAccess
{
	/// <summary>
	/// Gets or sets the scan number for this result
	/// </summary>
	[DataMember]
	public int Scan { get; set; }

	/// <summary>
	/// Gets or sets the scan number predicted for this peak
	/// </summary>
	[DataMember]
	public int PredictedScan { get; set; }

	/// <summary>
	/// Gets or sets the retention time of the peak which has been found
	/// </summary>
	[DataMember]
	public double FoundRT { get; set; }

	/// <summary>
	/// Gets or sets a score based on both forward and reverse matching factors
	/// </summary>
	[DataMember]
	public double FindScore { get; set; }

	/// <summary>
	/// Gets or sets the score from forward search
	/// </summary>
	[DataMember]
	public double ForwardScore { get; set; }

	/// <summary>
	/// Gets or sets the score from reverse search
	/// </summary>
	[DataMember]
	public double ReverseScore { get; set; }

	/// <summary>
	/// Gets or sets the intensity of the supplied chromatogram at this result
	/// </summary>
	[DataMember]
	public double ChromatogramIntensity { get; set; }

	/// <summary>
	/// Gets or sets the score from Match algorithm.
	/// </summary>
	[DataMember]
	public double MatchScore { get; set; }

	/// <summary>
	/// Gets or sets the peak found for this result
	/// </summary>
	[DataMember]
	public Peak FoundPeak { get; set; }

	/// <summary>
	/// Implementation of <c>ICloneable.Clone</c> method.
	/// Creates deep copy of this instance.
	/// </summary>
	/// <returns>
	/// An exact copy of the current collection.
	/// </returns>
	public virtual object Clone()
	{
		FindResult obj = (FindResult)MemberwiseClone();
		obj.FoundPeak = (Peak)FoundPeak.Clone();
		return obj;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.FindResult" /> class. 
	/// Default constructor
	/// </summary>
	public FindResult()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.FindResult" /> class. 
	/// Copy constructor
	/// </summary>
	/// <param name="access">
	/// object to copy
	/// </param>
	public FindResult(IFindResultAccess access)
	{
		if (access != null)
		{
			ChromatogramIntensity = access.ChromatogramIntensity;
			FindScore = access.FindScore;
			ForwardScore = access.ForwardScore;
			FoundPeak = access.FoundPeak;
			FoundRT = access.FoundRT;
			MatchScore = access.MatchScore;
			PredictedScan = access.PredictedScan;
			ReverseScore = access.ReverseScore;
			Scan = access.Scan;
		}
	}
}
