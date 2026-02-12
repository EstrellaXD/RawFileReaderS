using System;
using System.Xml.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Summary information about a scan.
/// </summary>
[Serializable]
public class ScanStatistics : CommonCoreDataObject, IDeepCloneable<ScanStatistics>, IScanStatisticsAccess, IMsScanIndexAccess, ICloneable
{
	private Lazy<string> _lazyScanType;

	private string _scanType;

	/// <summary>
	/// Gets or sets the indication of data format used by this scan.
	/// See also SpectrumPacketType for decoding to an <c>enum</c>.
	/// </summary>
	public int PacketType { get; set; }

	/// <summary>
	/// Gets the packet format used in this scan. (read only).
	/// Value can be set using the "PacketType" property.
	/// </summary>
	[XmlIgnore]
	public SpectrumPacketType SpectrumPacketType
	{
		get
		{
			int num = CommonData.LOWord(PacketType);
			if (num >= 0 && num < 25)
			{
				return (SpectrumPacketType)num;
			}
			return SpectrumPacketType.ProfileSpectrum;
		}
	}

	/// <summary>
	/// Gets or sets the highest mass in scan
	/// </summary>
	public double HighMass { get; set; }

	/// <summary>
	/// Gets or sets the lowest mass in scan
	/// </summary>
	public double LowMass { get; set; }

	/// <summary>
	/// Gets or sets the longest wavelength in PDA scan
	/// </summary>
	public double LongWavelength { get; set; }

	/// <summary>
	/// Gets or sets the shortest wavelength in PDA scan
	/// </summary>
	public double ShortWavelength { get; set; }

	/// <summary>
	/// Gets or sets the intensity of highest peak in scan
	/// </summary>
	public double BasePeakIntensity { get; set; }

	/// <summary>
	/// Gets or sets the mass of largest peak in scan
	/// </summary>
	public double BasePeakMass { get; set; }

	/// <summary>
	/// Gets or sets the total Ion Current for scan
	/// </summary>
	public double TIC { get; set; }

	/// <summary>
	/// Gets or sets the time at start of scan (minutes)
	/// </summary>
	public double StartTime { get; set; }

	/// <summary>
	/// Gets or sets the Number of point in scan
	/// </summary>
	public int PacketCount { get; set; }

	/// <summary>
	/// Gets or sets  the number of
	/// channels acquired in this scan, if this is UV or analog data,
	/// </summary>
	public int NumberOfChannels { get; set; }

	/// <summary>
	/// Gets or sets the number of the scan
	/// </summary>
	public int ScanNumber { get; set; }

	/// <summary>
	/// Gets or sets the event (scan type) number within segment
	/// </summary>
	public int ScanEventNumber { get; set; }

	/// <summary>
	/// Gets or sets the time segment number for this event
	/// </summary>
	public int SegmentNumber { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this scan contains centroid data (else profile)
	/// </summary>
	public bool IsCentroidScan { get; set; }

	/// <summary>
	/// Gets or sets the frequency.
	/// </summary>
	public double Frequency { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether is uniform time.
	/// </summary>
	public bool IsUniformTime { get; set; }

	/// <summary>
	/// Gets or sets the absorbance unit scale.
	/// </summary>
	public double AbsorbanceUnitScale { get; set; }

	/// <summary>
	/// Gets or sets the wave length step.
	/// </summary>
	public double WavelengthStep { get; set; }

	/// <summary>
	/// Gets or sets a lazy mechanism returning a string defining the scan type.
	/// </summary>
	protected Lazy<string> LazyScanType
	{
		get
		{
			return _lazyScanType;
		}
		set
		{
			_lazyScanType = value;
		}
	}

	/// <summary>
	/// Gets or sets a String defining the scan type, for filtering
	/// </summary>
	public virtual string ScanType
	{
		get
		{
			if (_lazyScanType == null)
			{
				return _scanType;
			}
			return _scanType = _lazyScanType.Value;
		}
		set
		{
			_scanType = value;
			_lazyScanType = null;
		}
	}

	/// <summary>
	///     Gets or sets the cycle number.
	///     <remarks>
	///         Cycle number used to associate events within a scan event cycle.
	///         For example, on the first cycle of scan events, all the events
	///         would set this to '1'. On the second cycle, all the events would
	///         set this to '2'. This field must be set by devices if supporting
	///         compound names for filtering. However, it may be set in all
	///         acquisitions to help processing algorithms.
	///     </remarks>
	/// </summary>
	public int CycleNumber { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanStatistics" /> class.
	/// </summary>
	/// <param name="source">
	/// The source to copy the values from.
	/// </param>
	/// <param name="deep">If set, make a "deep copy" which will evaluate any lazy items and ensure no internal source references</param>
	/// <exception cref="T:System.ArgumentNullException">
	/// <c>source</c> is null.
	/// </exception>
	public ScanStatistics(ScanStatistics source, bool deep = false)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		source.CopyTo(this, deep);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanStatistics" /> class.
	/// </summary>
	public ScanStatistics()
	{
	}

	/// <summary>
	/// Creates a new object that is a copy of the current instance.
	/// </summary>
	/// <returns>
	/// A new object that is a copy of this instance.
	/// </returns>
	public object Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>
	/// Copy all fields
	/// </summary>
	/// <param name="stats">
	/// Copy into this object
	/// </param>
	/// <param name="deep">If set, make a "deep copy" which will evaluate any lazy items and ensure no internal source references</param>
	public void CopyTo(ScanStatistics stats, bool deep = false)
	{
		if (stats != null)
		{
			stats.BasePeakIntensity = BasePeakIntensity;
			stats.BasePeakMass = BasePeakMass;
			stats.HighMass = HighMass;
			stats.IsCentroidScan = IsCentroidScan;
			stats.LowMass = LowMass;
			stats.PacketCount = PacketCount;
			stats.PacketType = PacketType;
			stats.ScanEventNumber = ScanEventNumber;
			stats.ScanNumber = ScanNumber;
			stats.SegmentNumber = SegmentNumber;
			stats.StartTime = StartTime;
			stats.TIC = TIC;
			stats.NumberOfChannels = NumberOfChannels;
			stats.ShortWavelength = ShortWavelength;
			stats.LongWavelength = LongWavelength;
			if (deep)
			{
				stats.ScanType = ScanType;
			}
			else
			{
				stats._lazyScanType = _lazyScanType;
				stats._scanType = _scanType;
			}
			stats.CycleNumber = CycleNumber;
			stats.Frequency = Frequency;
			stats.IsUniformTime = IsUniformTime;
			stats.WavelengthStep = WavelengthStep;
			stats.AbsorbanceUnitScale = AbsorbanceUnitScale;
		}
	}

	/// <summary>
	/// Produce a deep copy of an object.
	/// Must not contain any references into the original.
	/// </summary>
	/// <returns>
	/// A deep clone of all objects in this
	/// </returns>
	public ScanStatistics DeepClone()
	{
		return (ScanStatistics)MemberwiseClone();
	}
}
