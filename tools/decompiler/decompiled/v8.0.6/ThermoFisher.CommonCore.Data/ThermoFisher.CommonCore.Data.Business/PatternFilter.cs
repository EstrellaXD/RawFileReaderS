using System.Collections.Generic;
using System.Linq;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The isotope pattern filter class.
/// </summary>
internal class PatternFilter
{
	/// <summary>
	/// The filter reference mass (absolute mass value from scan).
	/// </summary>
	public double ReferenceMass { get; private set; }

	/// <summary>
	/// The filter reference intensity (absolute intensity value from scan).
	/// </summary>
	public double ReferenceIntensity { get; private set; }

	/// <summary>
	/// List of mass ranges (calculated from mass offset and mass tolerance) for each filter item.
	/// </summary>
	public List<Range> MassRanges { get; private set; }

	/// <summary>
	/// List of intensity ranges (calculated from expected intensity and intensity tolerance) for each filter item.
	/// </summary>
	public List<Range> IntensityRanges { get; private set; }

	/// <summary>
	/// List of mass offset values for each filter item.
	/// </summary>
	public List<double> MassOffsetList { get; }

	/// <summary>
	/// List of expected intensity values (relative %) for each filter item.
	/// </summary>
	public List<double> IntensityList { get; }

	/// <summary>
	/// List indicating if item is the reference item.
	/// </summary>
	public List<bool> IsReferenceList { get; set; }

	/// <summary>
	/// List of mass tolerances for each filter item.
	/// </summary>
	public List<Tolerance> MassTolerances { get; }

	/// <summary>
	/// List of intensity tolerances (%) for each filter item.
	/// </summary>
	public List<double> IntensityTolerances { get; }

	/// <summary>
	/// Minimum mass value in filter.
	/// </summary>
	public double LowMass => MassRanges.Min((Range x) => x.Low);

	/// <summary>
	/// Maximum mass value in filter.
	/// </summary>
	public double HighMass => MassRanges.Max((Range x) => x.High);

	/// <summary>
	/// Number of items in filter.
	/// </summary>
	public int Length => MassOffsetList.Count;

	/// <summary>
	/// Intialize the filter class.
	/// </summary>
	/// <param name="patternList"></param>
	public PatternFilter(IReadOnlyCollection<MassPattern> patternList)
	{
		MassOffsetList = patternList.Select((MassPattern x) => x.MassOffset).ToList();
		IntensityList = patternList.Select((MassPattern x) => x.Intensity).ToList();
		IsReferenceList = patternList.Select((MassPattern x) => x.IsReference).ToList();
		MassTolerances = patternList.Select((MassPattern x) => x.MassTolerance).ToList();
		IntensityTolerances = patternList.Select((MassPattern x) => x.IntensityTolerance).ToList();
		ValidateIsReferenceList();
		ValidateMassOffsetList();
		ValidateIntensityList();
	}

	/// <summary>
	/// Validate the reference flag list.
	/// </summary>
	private void ValidateIsReferenceList()
	{
		if (IsReferenceList.Count > 1 && IsReferenceList.Count((bool x) => x) != 1)
		{
			for (int num = 0; num < IsReferenceList.Count; num++)
			{
				IsReferenceList[num] = false;
			}
			int num2 = IntensityList.IndexOf(IntensityList.Max());
			if (num2 == -1)
			{
				num2 = 0;
			}
			IsReferenceList[num2] = true;
		}
	}

	/// <summary>
	/// Validate mass offset list values.
	/// </summary>
	private void ValidateMassOffsetList()
	{
		if (MassOffsetList.Count > 1)
		{
			int num = IsReferenceList.IndexOf(item: true);
			if (num == -1)
			{
				num = 0;
			}
			double num2 = MassOffsetList[num];
			for (int i = 0; i < MassOffsetList.Count; i++)
			{
				MassOffsetList[i] -= num2;
			}
		}
	}

	/// <summary>
	/// Validate intensity list values.
	/// </summary>
	private void ValidateIntensityList()
	{
		if (IntensityList.Count > 1)
		{
			int num = IsReferenceList.IndexOf(item: true);
			if (num == -1)
			{
				num = 0;
			}
			double num2 = IntensityList[num];
			for (int i = 0; i < IntensityList.Count; i++)
			{
				IntensityList[i] /= num2;
			}
		}
	}

	/// <summary>
	/// Compile the filter for a reference mass/intensity ion.
	/// </summary>
	/// <param name="referenceMass"></param>
	/// <param name="referenceIntensity"></param>
	public void CompileFilter(double referenceMass, double referenceIntensity)
	{
		ReferenceMass = referenceMass;
		ReferenceIntensity = referenceIntensity;
		MassRanges = new List<Range>();
		IntensityRanges = new List<Range>();
		for (int i = 0; i < MassOffsetList.Count; i++)
		{
			double myMass = ReferenceMass + MassOffsetList[i];
			double intensity = ReferenceIntensity * IntensityList[i];
			MassTolerances[i].GetMassLimits(myMass, out var lowMassLimit, out var highMassLimit);
			MassRanges.Add(new Range(lowMassLimit, highMassLimit));
			GetIntensityLimits(intensity, IntensityTolerances[i], out var minIntensity, out var maxIntensity);
			IntensityRanges.Add(new Range(minIntensity, maxIntensity));
		}
	}

	/// <summary>
	/// Get intensity limits for provided values.
	/// </summary>
	/// <param name="intensity"></param>
	/// <param name="intensityTolerance"></param>
	/// <param name="minIntensity"></param>
	/// <param name="maxIntensity"></param>
	private static void GetIntensityLimits(double intensity, double intensityTolerance, out double minIntensity, out double maxIntensity)
	{
		minIntensity = intensity - intensity * intensityTolerance;
		maxIntensity = intensity + intensity * intensityTolerance;
		if (minIntensity < 0.0)
		{
			minIntensity = 0.0;
		}
		if (maxIntensity < 0.0)
		{
			maxIntensity = 0.0;
		}
	}
}
