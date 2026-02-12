using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Settings for the spectrum find algorithm.
/// This algorithm validates a detected peak,
/// by proving that the masses in the supplied spectrum for the peak
/// maximize near the peak.
/// </summary>
[Serializable]
[DataContract]
public class FindSettings : CommonCoreDataObject, IFindSettingsAccess, ICloneable
{
	/// <summary>
	/// The forward threshold.
	/// </summary>
	private int _forwardThreshold;

	/// <summary>
	/// The match threshold.
	/// </summary>
	private int _matchThreshold;

	/// <summary>
	/// The reverse threshold.
	/// </summary>
	private int _reverseThreshold;

	/// <summary>
	/// The find spectrum.
	/// </summary>
	private ItemCollection<SpectrumPoint> _findSpectrum = new ItemCollection<SpectrumPoint>();

	/// <summary>
	/// Gets or sets the forward threshold for find algorithm.
	/// </summary>
	[DataMember]
	public int ForwardThreshold
	{
		get
		{
			return _forwardThreshold;
		}
		set
		{
			_forwardThreshold = value;
		}
	}

	/// <summary>
	/// Gets or sets the match threshold for find algorithm
	/// </summary>
	[DataMember]
	public int MatchThreshold
	{
		get
		{
			return _matchThreshold;
		}
		set
		{
			_matchThreshold = value;
		}
	}

	/// <summary>
	/// Gets or sets the reverse threshold for find algorithm
	/// </summary>
	[DataMember]
	public int ReverseThreshold
	{
		get
		{
			return _reverseThreshold;
		}
		set
		{
			_reverseThreshold = value;
		}
	}

	/// <summary>
	/// Gets or sets the spec points.
	/// </summary>
	/// <value>The spec points.</value>
	[DataMember]
	public ItemCollection<SpectrumPoint> SpecPoints
	{
		get
		{
			return _findSpectrum;
		}
		set
		{
			_findSpectrum = value;
		}
	}

	/// <summary>
	/// Gets the spec points.
	/// </summary>
	/// <value>The spec points.</value>
	[XmlIgnore]
	ReadOnlyCollection<SpectrumPoint> IFindSettingsAccess.SpecPoints
	{
		get
		{
			Collection<SpectrumPoint> collection = new Collection<SpectrumPoint>();
			if (_findSpectrum != null)
			{
				foreach (SpectrumPoint item in _findSpectrum)
				{
					collection.Add(item);
				}
			}
			return new ReadOnlyCollection<SpectrumPoint>(collection);
		}
	}

	/// <summary>
	/// Get a copy of the find spectrum
	/// </summary>
	/// <returns>
	/// A copy of the find spectrum.
	/// </returns>
	public SpectrumPoint[] GetFindSpectrum()
	{
		return _findSpectrum.ToArray();
	}

	/// <summary>
	/// Update the spectrum (for the find algorithm)
	/// </summary>
	/// <param name="spectrum">The spectrum to find
	/// </param>
	public void SetFindSpectrum(IEnumerable<SpectrumPoint> spectrum)
	{
		if (spectrum != null)
		{
			_findSpectrum = new ItemCollection<SpectrumPoint>();
			_findSpectrum.AddRange(spectrum);
		}
		else
		{
			_findSpectrum = new ItemCollection<SpectrumPoint>();
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.FindSettings" /> class. 
	/// Default constructor
	/// </summary>
	public FindSettings()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.FindSettings" /> class. 
	/// Construct instance from interface, by cloning all settings 
	/// </summary>
	/// <param name="access">
	/// Interface to clone
	/// </param>
	public FindSettings(IFindSettingsAccess access)
	{
		if (access == null)
		{
			return;
		}
		ForwardThreshold = access.ForwardThreshold;
		MatchThreshold = access.MatchThreshold;
		ReverseThreshold = access.ReverseThreshold;
		_findSpectrum = new ItemCollection<SpectrumPoint>();
		ReadOnlyCollection<SpectrumPoint> specPoints = access.SpecPoints;
		if (specPoints == null)
		{
			return;
		}
		foreach (SpectrumPoint item in specPoints)
		{
			_findSpectrum.Add(item);
		}
	}

	/// <summary>
	/// Make a copy of this object
	/// </summary>
	/// <returns>a copy of this object</returns>
	public object Clone()
	{
		return new FindSettings(this);
	}
}
