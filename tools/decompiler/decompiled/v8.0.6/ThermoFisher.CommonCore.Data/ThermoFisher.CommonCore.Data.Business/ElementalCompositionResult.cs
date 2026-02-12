using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// A single possible result (formula) for elemental composition of a mass
/// </summary>
[Serializable]
[DataContract]
public class ElementalCompositionResult : CommonCoreDataObject
{
	/// <summary>
	/// isotope packet 
	/// </summary>
	public class IsotopePacket
	{
		/// <summary>
		/// Gets or sets Mass of theoretical isotope
		/// </summary>
		public double MassTheory { get; set; }

		/// <summary>
		/// Gets or sets Mass of measured centroid
		/// </summary>
		public double MassMeasured { get; set; }

		/// <summary>
		/// Gets or sets Intensity of theoretical isotope
		/// </summary>
		public double IntensityTheory { get; set; }

		/// <summary>
		/// Gets or sets Intensity of measured centroid
		/// </summary>
		public double IntensityMeasured { get; set; }

		/// <summary>
		/// Gets or sets the index into the original scan for the matched peak
		/// </summary>
		public int OriginalPeakIndex { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether Theory has a close match with Measured
		/// </summary>
		public bool Matched { get; set; }

		/// <summary>
		/// Gets or sets the match mode. Defines the quality of the match.
		/// </summary>
		public PacketMatchMode MatchMode { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ElementalCompositionResult.IsotopePacket" /> class. 
		/// default constructor
		/// </summary>
		public IsotopePacket()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ElementalCompositionResult.IsotopePacket" /> class. 
		/// constructor
		/// </summary>
		/// <param name="intensityMeasured">
		/// The intensityMeasured
		/// </param>
		/// <param name="intensityTheory">
		/// The intensityTheory
		/// </param>
		/// <param name="massMeasured">
		/// The massMeasured
		/// </param>
		/// <param name="massTheory">
		/// The massTheory
		/// </param>
		/// <param name="matched">
		/// The matched
		/// </param>
		public IsotopePacket(double intensityMeasured, double intensityTheory, double massMeasured, double massTheory, bool matched)
		{
			IntensityMeasured = intensityMeasured;
			IntensityTheory = intensityTheory;
			MassMeasured = massMeasured;
			MassTheory = massTheory;
			Matched = matched;
		}
	}

	/// <summary>
	/// Class containing assignments of formulas to masses to the fragmentation spectrum
	/// </summary>
	public class MsMsFragment
	{
		/// <summary>
		/// Gets or sets mass in fragmentation spectrum
		/// </summary>
		public double Mass { get; set; }

		/// <summary>
		/// Gets or sets formula matched to the Mass in the fragmentation spectrum
		/// </summary>
		public string Formula { get; set; }

		/// <summary>
		/// Gets or sets DeltaMass of assigned formula from the observed m/z
		/// </summary>
		public double DeltaMass { get; set; }

		/// <summary>
		/// Gets or sets Intensity of the fragment
		/// </summary>
		public double Intensity { get; set; }

		/// <summary>
		/// Gets or sets Signal to noise ratio of the fragment
		/// </summary>
		public double SignalToNoise { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ElementalCompositionResult.MsMsFragment" /> class.
		/// </summary>
		public MsMsFragment()
		{
		}

		/// <summary>
		///  Initializes a new instance of the  <see cref="T:ThermoFisher.CommonCore.Data.Business.ElementalCompositionResult.MsMsFragment" /> class
		/// </summary>
		/// <param name="mass">assigned mass</param>
		/// <param name="formula">assigned formula</param>
		public MsMsFragment(double mass, string formula)
		{
			Mass = mass;
			Formula = formula;
		}

		/// <summary>
		/// Initializes a new instance of the  <see cref="T:ThermoFisher.CommonCore.Data.Business.ElementalCompositionResult.MsMsFragment" /> class
		/// </summary>
		/// <param name="mass">assigned mass</param>
		/// <param name="formula">assigned formula</param>
		/// <param name="deltaMass">observed delta mass</param>
		/// <param name="intensity">observed intensity</param>
		/// <param name="signalToNoise">observed signal to noise</param>
		public MsMsFragment(double mass, string formula, double deltaMass, double intensity, double signalToNoise)
		{
			Mass = mass;
			Formula = formula;
			DeltaMass = deltaMass;
			Intensity = intensity;
			SignalToNoise = signalToNoise;
		}
	}

	/// <summary>
	/// The _mass.
	/// </summary>
	private double _mass;

	/// <summary>
	/// The _combined fit.
	/// </summary>
	private double _combinedFit;

	/// <summary>
	/// The _delta mass.
	/// </summary>
	private double _deltaMass;

	/// <summary>
	/// The _formula.
	/// </summary>
	private string _formula;

	/// <summary>
	/// The _pattern fit.
	/// </summary>
	private double _patternFit;

	/// <summary>
	/// The RDB equivalents.
	/// </summary>
	private double _rdbEquivalents;

	/// <summary>
	/// The _spectral distance.
	/// </summary>
	private double _spectralDistance;

	/// <summary>
	/// The _mass deviation.
	/// </summary>
	private double _massDeviation;

	/// <summary>
	/// The _score.
	/// </summary>
	private double _score;

	/// <summary>
	/// The _composition.
	/// </summary>
	private string _composition;

	/// <summary>
	/// The _isotopePackets
	/// </summary>
	[NonSerialized]
	[XmlIgnore]
	private List<IsotopePacket> _isotopePackets;

	/// <summary>
	/// Gets or sets the fit value in percent.
	/// </summary>
	[DataMember]
	public double Score
	{
		get
		{
			return _score;
		}
		set
		{
			_score = value;
		}
	}

	/// <summary>
	/// Gets or sets the Elemental composition formula, with subscript style digits for elemental quantities.
	/// This formula defines the istope of the peak which was matched with the sample.
	/// </summary>
	[DataMember]
	public string Composition
	{
		get
		{
			return _composition;
		}
		set
		{
			_composition = value;
		}
	}

	/// <summary>
	/// Gets or sets the Elemental composition formula, with subscript style digits for elemental quantities.
	/// This is the formula which contains the peak from the sample as one of it's istopes.
	/// It can be used to redraw the isotope plot.
	/// </summary>
	public string FormulaAsSimulated { get; set; }

	/// <summary>
	/// Gets or sets the mass of Composition (the mass of the calculated formula)
	/// </summary>
	[DataMember]
	public double Mass
	{
		get
		{
			return _mass;
		}
		set
		{
			_mass = value;
		}
	}

	/// <summary>
	/// Gets or sets the combined fit factor [range from 0% to 100%]. This is a linear
	/// combination between pattern fit and mass deviation.
	/// </summary>
	[DataMember]
	public double CombinedFit
	{
		get
		{
			return _combinedFit;
		}
		set
		{
			_combinedFit = value;
		}
	}

	/// <summary>
	/// Gets or sets the difference between the mass of the formula and the mass being searched.
	/// This is the mass difference in amu "mass to search for" - "mass of this formula".
	/// </summary>
	[DataMember]
	public double DeltaMass
	{
		get
		{
			return _deltaMass;
		}
		set
		{
			_deltaMass = value;
		}
	}

	/// <summary>
	/// Gets or sets the elemental formula (plain text)
	/// </summary>
	[DataMember]
	public string Formula
	{
		get
		{
			return _formula;
		}
		set
		{
			_formula = value;
		}
	}

	/// <summary>
	/// Gets or sets the fit factor [range from 0% to 100%] that indicates fit between theoretical and
	/// measured isotope pattern.
	/// </summary>
	[DataMember]
	public double PatternFit
	{
		get
		{
			return _patternFit;
		}
		set
		{
			_patternFit = value;
		}
	}

	/// <summary>
	/// Gets or sets the calculated RDB for the found formula
	/// </summary>
	[DataMember]
	public double RdbEquivalents
	{
		get
		{
			return _rdbEquivalents;
		}
		set
		{
			_rdbEquivalents = value;
		}
	}

	/// <summary>
	/// Gets or sets the Spectral Distance. This is the distance between a theoretical
	/// and a measured isotope pattern of n packets in an n-dimensional space.
	/// </summary>
	[DataMember]
	public double SpectralDistance
	{
		get
		{
			return _spectralDistance;
		}
		set
		{
			_spectralDistance = value;
		}
	}

	/// <summary>
	/// Gets or sets the deviation in mass, using the same tolerance mode used when searching
	/// </summary>
	[DataMember]
	public double MassDeviation
	{
		get
		{
			return _massDeviation;
		}
		set
		{
			_massDeviation = value;
		}
	}

	/// <summary>
	/// Gets or sets the IsotopePackets.
	/// </summary>
	[DataMember]
	public List<IsotopePacket> IsotopePackets
	{
		get
		{
			return _isotopePackets;
		}
		set
		{
			_isotopePackets = value;
		}
	}

	/// <summary>
	/// Gets or sets the number of peaks matched to an isotope by spectral distance algorithm,
	/// that is: All peaks which contributed to the scoring (even if the peak was out of mass and intensity
	/// tolerance box).
	/// </summary>
	public int MatchedPeaks { get; set; }

	/// <summary>
	/// Gets or sets the number of isotopes in the pattern tested for match against sample peaks
	/// </summary>
	public int IsotopesTested { get; set; }

	/// <summary>
	/// Gets or sets the number of isotopes in the pattern which were matched
	/// against a sample peak, and were within mass/intensity tolerance limits
	/// </summary>
	public int NoPenaltyPeaks { get; set; }

	/// <summary>
	/// Gets or sets the number of isotopes where there is at least one sample peak
	/// within 10* mass tolerance
	/// </summary>
	public int BroadSearchMatches { get; set; }

	/// <summary>
	/// Gets or sets the coverage of experimental spectrum by isotopic pattern
	/// </summary>
	public double MsCoverage { get; set; }

	/// <summary>
	/// Gets or sets the coverage of theoretical spectrum by isotopic pattern
	/// </summary>
	public double PatternCoverage { get; set; }

	/// <summary>
	/// Gets or sets the computed coverage of MsMs spectrum
	/// </summary>
	public double MsMsCoverage { get; set; }

	/// <summary>
	/// Gets or sets the computed measure of matches with average shift comparing to the precursor shift
	/// </summary>
	public double MsMsShiftMeasure { get; set; }

	/// <summary>
	/// Gets or sets the number of matched peaks in MSMS spectrum
	/// </summary>
	public int MsMsMatchedPeaks { get; set; }

	/// <summary>
	/// Gets or sets the list of matched fragments
	/// </summary>
	public List<MsMsFragment> MsMsMatchedFragments { get; set; }

	/// <summary>
	/// Gets or sets Combined score used for re-ranking results based on supplied weights
	/// </summary>
	public double CombinedScore { get; set; }

	/// <summary>
	/// Gets or sets Rank of the candidate formula based on Combined score
	/// </summary>
	public int Rank { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ElementalCompositionResult" /> class. 
	/// default constructor
	/// </summary>
	public ElementalCompositionResult()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ElementalCompositionResult" /> class. 
	/// Construct from values. (Faster than constructing and setting properties, as property setters in this
	/// class may be instrumented with change notification).
	/// </summary>
	/// <param name="combinedFit">
	/// Sets CombinedFit property
	/// </param>
	/// <param name="deltaMass">
	/// Sets DeltaMass property
	/// </param>
	/// <param name="formula">
	/// Sets Formula property
	/// </param>
	/// <param name="mass">
	/// Sets Mass property
	/// </param>
	/// <param name="massDeviation">
	/// Sets MassDeviation property
	/// </param>
	/// <param name="patternFit">
	/// Sets PatternFit property
	/// </param>
	/// <param name="rdbEquivalents">
	/// Sets RdbEquivalents property
	/// </param>
	/// <param name="spectralDistance">
	/// Sets SpectralDistance property
	/// </param>
	/// <param name="score">
	/// Sets Score property
	/// </param>
	/// <param name="matchedPeaks">
	/// Sets MatchedPeaks property
	/// </param>
	/// <param name="noPenaltyPeaks">
	/// Sets NoPenaltyPeaks property
	/// </param>
	/// <param name="broadSearchMatches">
	/// Sets BroadSearchMatches property
	/// </param>
	/// <param name="isotopesTested">
	/// Sets IsotopesTested property
	/// </param>
	/// <param name="composition">
	/// Sets Composition property. The is the isotope composition matched to the experminal peak
	/// </param>
	public ElementalCompositionResult(double combinedFit, double deltaMass, string formula, double mass, double massDeviation, double patternFit, double rdbEquivalents, double spectralDistance, double score, int matchedPeaks, int noPenaltyPeaks, int broadSearchMatches, int isotopesTested, string composition)
	{
		_combinedFit = combinedFit;
		_deltaMass = deltaMass;
		_formula = formula;
		_mass = mass;
		_massDeviation = massDeviation;
		_patternFit = patternFit;
		_rdbEquivalents = rdbEquivalents;
		_spectralDistance = spectralDistance;
		_score = score;
		MatchedPeaks = matchedPeaks;
		NoPenaltyPeaks = noPenaltyPeaks;
		BroadSearchMatches = broadSearchMatches;
		IsotopesTested = isotopesTested;
		_composition = composition;
		if (isotopesTested > 0)
		{
			IsotopePackets = new List<IsotopePacket>(isotopesTested);
		}
	}

	/// <summary>
	/// Add isotope packet into the ElementalCompositionResult
	/// </summary>
	/// <param name="intensityMeasured">
	/// The intensityMeasured
	/// </param>
	/// <param name="intensityTheory">
	/// The intensityTheory
	/// </param>
	/// <param name="massMeasured">
	/// The massMeasured
	/// </param>
	/// <param name="massTheory">
	/// The massTheory
	/// </param>
	/// <param name="matched">
	/// The matched
	/// </param>
	/// <param name="matchMode">How the isotope was matched (within the mass and intensity limits)</param>
	/// <param name="originalPeakIndex">Index of the matched peak in the original scan</param>
	public void AddIsotope(double intensityMeasured, double intensityTheory, double massMeasured, double massTheory, bool matched, PacketMatchMode matchMode = PacketMatchMode.Inside, int originalPeakIndex = 0)
	{
		IsotopePackets.Add(new IsotopePacket(intensityMeasured, intensityTheory, massMeasured, massTheory, matched)
		{
			MatchMode = matchMode,
			OriginalPeakIndex = originalPeakIndex
		});
	}

	/// <summary>
	/// Compares only the formula for equality
	/// </summary>
	/// <param name="result">
	/// The Result who's formula is compared.
	/// </param>
	/// <returns>
	/// True if equal
	/// </returns>
	public bool Equals(ElementalCompositionResult result)
	{
		return Formula == result.Formula;
	}
}
