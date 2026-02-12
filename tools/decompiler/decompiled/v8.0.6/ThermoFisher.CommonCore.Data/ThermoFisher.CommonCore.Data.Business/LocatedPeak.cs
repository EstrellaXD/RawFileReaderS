using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The results of finding a single peak from a list.
/// This includes the peak that is found, how it was found,
/// and in the case of spectral searching, the spectral search results.
/// </summary>
[Serializable]
[DataContract]
public class LocatedPeak : ICloneable, ILocatedPeakAccess
{
	/// <summary>
	/// Gets or sets the peak which best matches the location rules.
	/// </summary>
	[DataMember]
	public Peak DetectedPeak { get; set; }

	/// <summary>
	/// Gets the peak which best matches the location rules.
	/// </summary>
	IPeakAccess ILocatedPeakAccess.DetectedPeak => DetectedPeak;

	/// <summary>
	/// Gets or sets a record of how this peak was found.
	/// The find results are only valid when this is set to "Spectrum".
	/// </summary>
	[DataMember]
	public PeakMethod Method { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether RT adjustments could be made to the RT reference.
	/// This flag is only meaningful when RT reference adjustments are made based on
	/// a reference peak (see the locate class).
	/// If a valid reference peak is supplied, then the expected RT can be adjusted based on the reference.
	/// If no reference peak is found (a null peak) then the expected RT cannot be adjusted, and this flag will be false.
	/// </summary>
	[DataMember]
	public bool ValidRTReference { get; set; }

	/// <summary>
	/// Gets or sets the Find Results.
	/// When using spectrum LocateMethod this will contain the best matching peaks and find scores.
	/// </summary>
	[DataMember]
	public Collection<FindResult> FindResults { get; set; }

	/// <summary>
	/// Gets the peak which best matches the location rules.
	/// </summary>
	/// <returns>The results for this peak, or empty list</returns>
	[XmlIgnore]
	ReadOnlyCollection<FindResult> ILocatedPeakAccess.FindResults
	{
		get
		{
			Collection<FindResult> collection = new Collection<FindResult>();
			if (FindResults != null)
			{
				foreach (FindResult findResult in FindResults)
				{
					collection.Add(findResult);
				}
			}
			return new ReadOnlyCollection<FindResult>(collection);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.LocatedPeak" /> class. 
	/// Default constructor
	/// </summary>
	public LocatedPeak()
	{
		FindResults = new Collection<FindResult>();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.LocatedPeak" /> class. 
	/// Copy constructor
	/// </summary>
	/// <param name="access">
	/// The object to copy
	/// </param>
	public LocatedPeak(ILocatedPeakAccess access)
	{
		if (access != null)
		{
			DetectedPeak = new Peak(access.DetectedPeak);
			FindResults = new Collection<FindResult>(access.FindResults);
			Method = access.Method;
			ValidRTReference = access.ValidRTReference;
		}
	}

	/// <summary>
	/// Implementation of <c>ICloneable.Clone</c> method.
	/// Creates deep copy of this instance.
	/// </summary>
	/// <returns>An exact copy of the current collection.</returns>
	public virtual object Clone()
	{
		LocatedPeak locatedPeak = (LocatedPeak)MemberwiseClone();
		if (DetectedPeak != null)
		{
			locatedPeak.DetectedPeak = (Peak)DetectedPeak.Clone();
		}
		return locatedPeak;
	}
}
