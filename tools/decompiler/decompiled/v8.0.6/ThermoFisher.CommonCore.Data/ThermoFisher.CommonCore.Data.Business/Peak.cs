using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This class encapsulates a peak. (the result of peak detection)
/// </summary>
[Serializable]
[DataContract]
public class Peak : ICloneable, IPeakAccess
{
	private bool _isValid = true;

	/// <summary>
	/// Gets or sets the list of peaks that have been merged
	/// </summary>
	[DataMember]
	public ItemCollection<Peak> MergePeaks { get; set; }

	/// <summary>
	/// Gets the list of peaks that have been merged
	/// </summary>
	[XmlIgnore]
	public ReadOnlyCollection<IPeakAccess> MergedPeaks
	{
		get
		{
			Collection<IPeakAccess> collection = new Collection<IPeakAccess>();
			if (MergePeaks != null)
			{
				foreach (Peak mergePeak in MergePeaks)
				{
					collection.Add(mergePeak);
				}
			}
			return new ReadOnlyCollection<IPeakAccess>(collection);
		}
	}

	/// <summary>
	/// Gets a value which determines how signal to noise has been calculated.
	/// When this returns <see cref="T:ThermoFisher.CommonCore.Data.NoiseClassification" />.Value, a numeric value can
	/// be obtained from <see cref="P:ThermoFisher.CommonCore.Data.Business.Peak.SignalToNoise" />.
	/// </summary>
	public NoiseClassification NoiseResult
	{
		get
		{
			if (Noise == -1.0)
			{
				return NoiseClassification.NotAvailable;
			}
			if (Noise <= 0.0)
			{
				return NoiseClassification.Infinite;
			}
			return NoiseClassification.Value;
		}
	}

	/// <summary>
	/// Gets the signal to noise ratio. If <see cref="P:ThermoFisher.CommonCore.Data.Business.Peak.NoiseResult" /> is <see cref="T:ThermoFisher.CommonCore.Data.NoiseClassification" />.Value, then this property returns the signal to noise ratio.
	/// Otherwise this should not be used. Use <see cref="T:ThermoFisher.CommonCore.Data.EnumFormat" />.ToString(<see cref="P:ThermoFisher.CommonCore.Data.Business.Peak.NoiseResult" />) instead.
	/// </summary>
	public double SignalToNoise => Apex.HeightAboveBaseline / Noise;

	/// <summary>
	/// Gets or sets the position, height, baseline at left limit
	/// </summary>
	[DataMember]
	public PeakPoint Left { get; set; }

	/// <summary>
	/// Gets or sets the position, height, baseline  at peak apex
	/// </summary>
	[DataMember]
	public PeakPoint Apex { get; set; }

	/// <summary>
	/// Gets or sets the position, height, baseline at right limit
	/// </summary>
	[DataMember]
	public PeakPoint Right { get; set; }

	/// <summary>
	/// Gets or sets the Integrated peak area
	/// </summary>
	[DataMember]
	public double Area { get; set; }

	/// <summary>
	/// Gets or sets the Mass of the base peak from the apex scan.
	/// </summary>
	[DataMember]
	public double BasePeakMass { get; set; }

	/// <summary>
	/// Gets or sets the Mass to charge ratio of peak.
	/// </summary>
	[DataMember]
	public double MassToCharge { get; set; }

	/// <summary>
	/// Gets or sets the expected RT after making any RT adjustments.
	/// </summary>
	[DataMember]
	public double ExpectedRT { get; set; }

	/// <summary>
	/// Gets or sets the Noise measured in detected peak (for signal to noise calculation)
	/// </summary>
	[DataMember]
	public double Noise { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the "Noise" value was calculated by an RMS algorithm.
	/// </summary>
	[DataMember]
	public bool RmsNoise { get; set; }

	/// <summary>
	/// Gets or sets the scan number at peak apex.
	/// The apex of the peak corresponds to a particular signal.
	/// This gives the scan number of that signal.
	/// If no scan numbers are sent with the peak detection signal, then
	/// the scan number = "signal index at apex +1".
	/// Note that there is no guarantee that left and right edges will always be exactly on a scan, even
	/// though most peak detectors behave that way, so this is not added as a property of <see cref="T:ThermoFisher.CommonCore.Data.Business.PeakPoint" />
	/// </summary>
	[DataMember]
	public int ScanAtApex { get; set; }

	/// <summary>
	/// Gets or sets a Name for this peak (for example, compound name)
	/// </summary>
	[DataMember]
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the number of scans integrated
	/// </summary>
	[DataMember]
	public int Scans { get; set; }

	/// <summary>
	/// Gets or sets a value which describes why the peak started. It is only set by the Genesis Detector.
	/// </summary>
	[DataMember]
	public EdgeType LeftEdge { get; set; }

	/// <summary>
	/// Gets or sets a value which describes why the peak ended. It is only set by the Genesis Detector.
	/// </summary>
	[DataMember]
	public EdgeType RightEdge { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this is a valid peak.
	/// Peaks are assumed to have valid data, but may be marked invalid by
	/// an integrator, if failing certain tests.
	/// Invalid peaks should never be returned to a calling application
	/// by an integrator algorithm.
	/// Invalid peaks must never be: Drawn in a plot, listed in a report etc.
	/// An application should indicate "Peak not found" when a peak is flagged as "Invalid".
	/// </summary>
	[DataMember]
	public bool Valid
	{
		get
		{
			return _isValid;
		}
		set
		{
			_isValid = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="T:ThermoFisher.CommonCore.Data.Business.Peak" /> is saturated.
	/// </summary>
	/// <value>true when integration/mass range has saturation.</value>
	[DataMember]
	public bool Saturated { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether valley detection was used when detecting this peak.
	/// </summary>
	[DataMember]
	public bool ValleyDetect { get; set; }

	/// <summary>
	/// Gets or sets the Direction of peak (Positive or Negative)
	/// </summary>
	[DataMember]
	public PeakDirection Direction { get; set; }

	/// <summary>
	/// Gets or sets the chi-squared error in fitting the peak.
	/// </summary>
	[DataMember]
	public double Fit { get; set; }

	/// <summary>
	/// Gets or sets the calculated width, or 'gamma_r'.
	/// </summary>
	[DataMember]
	public double FittedWidth { get; set; }

	/// <summary>
	/// Gets or sets the calculated intensity, or 'gamma_A'.
	/// </summary>
	[DataMember]
	public double FittedIntensity { get; set; }

	/// <summary>
	/// Gets or sets the calculated position, or 'gamma_t0'.
	/// </summary>
	[DataMember]
	public double FittedRT { get; set; }

	/// <summary>
	/// Gets or sets the calculated fourth parameter for gamma (gamma_M) or EMG functions.
	/// </summary>
	[DataMember]
	public double FittedAsymmetry { get; set; }

	/// <summary>
	/// Gets or sets the peak shape used in the fitting procedure.
	/// </summary>
	[DataMember]
	public int FittedFunction { get; set; }

	/// <summary>
	/// Gets or sets the number of data points used in the fit.
	/// </summary>
	[DataMember]
	public int FittedPoints { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether calculated values (such as height) may return negative numbers.
	/// If false: negative calculated numbers are returned as 0
	/// </summary>
	[DataMember]
	public bool NegativeDataPermitted { get; set; }

	/// <summary>
	/// Gets or sets peak Purity.
	/// </summary>
	[DataMember]
	public double Purity { get; set; }

	/// <summary>
	/// Gets Low time from peak purity calculation
	/// </summary>
	public double PurityLowTime => AmountLow;

	/// <summary>
	/// Gets High time from peak purity calculation
	/// </summary>
	public double PurityHighTime => AmountHigh;

	/// <summary>
	/// Gets or sets the Low value of the retention time range, after a peak purity calculation.
	/// </summary>
	[DataMember]
	public double AmountLow { get; set; }

	/// <summary>
	/// Gets or sets the High value of the retention time range, after a peak purity calculation..
	/// </summary>
	[DataMember]
	public double AmountHigh { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Peak" /> class. 
	/// default constructor
	/// </summary>
	public Peak()
	{
		MergePeaks = new ItemCollection<Peak>();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.Peak" /> class. 
	/// copy constructor
	/// </summary>
	/// <param name="copyFromPeak">
	/// The peak to copy from.
	/// </param>
	public Peak(IPeakAccess copyFromPeak)
	{
		if (copyFromPeak != null)
		{
			CreateFromPeak(copyFromPeak);
		}
	}

	/// <summary>
	/// Create by copying a peak.
	/// </summary>
	/// <param name="copyFromPeak">
	/// The peak to copy.
	/// </param>
	protected void CreateFromPeak(IPeakAccess copyFromPeak)
	{
		Apex = copyFromPeak.Apex;
		Area = copyFromPeak.Area;
		BasePeakMass = copyFromPeak.BasePeakMass;
		MassToCharge = copyFromPeak.MassToCharge;
		Direction = copyFromPeak.Direction;
		ExpectedRT = copyFromPeak.ExpectedRT;
		Fit = copyFromPeak.Fit;
		FittedAsymmetry = copyFromPeak.FittedAsymmetry;
		FittedFunction = copyFromPeak.FittedFunction;
		FittedIntensity = copyFromPeak.FittedIntensity;
		FittedPoints = copyFromPeak.FittedPoints;
		FittedRT = copyFromPeak.FittedRT;
		FittedWidth = copyFromPeak.FittedWidth;
		Left = copyFromPeak.Left;
		LeftEdge = copyFromPeak.LeftEdge;
		MergePeaks = new ItemCollection<Peak>();
		foreach (IPeakAccess mergedPeak in copyFromPeak.MergedPeaks)
		{
			MergePeaks.Add(new Peak(mergedPeak));
		}
		Name = copyFromPeak.Name;
		Noise = copyFromPeak.Noise;
		Purity = copyFromPeak.Purity;
		AmountHigh = copyFromPeak.PurityHighTime;
		AmountLow = copyFromPeak.PurityLowTime;
		Right = copyFromPeak.Right;
		RightEdge = copyFromPeak.RightEdge;
		RmsNoise = copyFromPeak.RmsNoise;
		Saturated = copyFromPeak.Saturated;
		ScanAtApex = copyFromPeak.ScanAtApex;
		Scans = copyFromPeak.Scans;
		Valid = copyFromPeak.Valid;
		ValleyDetect = copyFromPeak.ValleyDetect;
	}

	/// <summary>
	/// Find baseline height for a peak at specified time
	/// </summary>
	/// <param name="retentionTime">Retention time to use for interpolation</param>
	/// <returns>Interpolated baseline height at <paramref name="retentionTime" /></returns>
	public double Baseline(double retentionTime)
	{
		double baselineHeight = Left.BaselineHeight;
		double num = (Right.BaselineHeight - baselineHeight) / (Right.RetentionTime - Left.RetentionTime);
		double num2 = baselineHeight - num * Left.RetentionTime;
		double num3 = num * retentionTime + num2;
		if (!NegativeDataPermitted)
		{
			return Math.Max(0.0, num3);
		}
		return num3;
	}

	/// <summary>
	/// Implementation of <c>ICloneable.Clone</c> method.
	/// Creates deep copy of this instance.
	/// </summary>
	/// <returns>An exact copy of the current Peak.</returns>
	public virtual object Clone()
	{
		Peak peak = (Peak)MemberwiseClone();
		if (MergePeaks != null)
		{
			peak.MergePeaks = MergePeaks.Clone() as ItemCollection<Peak>;
		}
		return peak;
	}
}
