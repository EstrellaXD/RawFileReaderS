using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Settings for the elemental compositions (formula searching) algorithm
/// </summary>
[Serializable]
[DataContract]
public class ElementalCompositionParameters : CommonCoreDataObject
{
	private bool _doElementalComposition = true;

	[DataMember]
	private double _amuValue = 1.003;

	private int _charge = 1;

	private int _keepBest = 10;

	private CentroidAlgorithm _centroidAlgorithm = CentroidAlgorithm.FTORBITRAP;

	private double _minRdbLimit = -1.0;

	private double _maxRdbLimit = 100.0;

	private ToleranceMode _massToleranceMode = ToleranceMode.Mmu;

	private ToleranceMode _msMsMassToleranceMode = ToleranceMode.Mmu;

	private double _massToleranceValue = 5.0;

	private double _msMsMassToleranceValue = 5.0;

	private int _msMsCharge;

	private int _resolution = 100000;

	private NitrogenRule _nitrogenRule;

	private PatternNormalizationMode _normalizationMode = PatternNormalizationMode.NormModeQuadratic;

	private double _expectedMassError = 1000.0;

	private double _expectedIntensityError = 1000.0;

	private double _zeroFitExpectedError = 1.0;

	private double _hundredFitExpectedError;

	private bool _weightIntensityErrorByAbundance = true;

	private bool _absoluteIntensityError = true;

	private double _intensityThresholdPercentTheory;

	private bool _automaticIntensityThreshold = true;

	private MissingPacketPenaltyMode _missingPacketPenaltyMode = MissingPacketPenaltyMode.PenaltyAutomaticMode;

	private bool _useSpectralFitting = true;

	private bool _useRepresentativeElements = true;

	private ElementsSubsetCollection _elementSubsetCollection = new ElementsSubsetCollection();

	private bool _weightMassErrorByAbundance = true;

	private bool _includeReferenceExceptionPeaks = true;

	private double _monoIsotopicSearchIntensityThreshold;

	private double _monoIsotopicSearchMassThreshold;

	private bool _useMonoIsotopicMass = true;

	private int _monoIsotopicMassPacketLowLimit = 10;

	private int _monoIsotopicMassPacketHighLimit = 20;

	private bool _automaticDynamicRange = true;

	private bool _dynamicA0Recalibration;

	private double _msMsSignalToNoiseThreshold = 50.0;

	private double _msMsSfitCutoff = 1.0;

	private double _msMsShiftMeasureWeight = 0.25;

	private double _spectralFitWeight = 0.25;

	private double _msCoverageWeight = 0.25;

	private double _patternCoverageWeight;

	private double _msMsCoverageWeight = 0.25;

	/// <summary>
	/// Gets or sets a value indicating whether all features of elemental composition should be used. When false, all other settings are ignored
	/// and results of searches should be returned as an empty collection.
	/// </summary>
	[DataMember]
	public bool DoElementalComposition
	{
		get
		{
			return _doElementalComposition;
		}
		set
		{
			_doElementalComposition = value;
		}
	}

	/// <summary>
	/// Gets or sets the Lowest mass for use in mono isotopic mass searching
	/// </summary>
	[DataMember]
	public int MonoIsotopicMassPacketLowLimit
	{
		get
		{
			return _monoIsotopicMassPacketLowLimit;
		}
		set
		{
			_monoIsotopicMassPacketLowLimit = value;
		}
	}

	/// <summary>
	/// Gets or sets the Highest mass for mono isotopic mass searching
	/// </summary>
	[DataMember]
	public int MonoIsotopicMassPacketHighLimit
	{
		get
		{
			return _monoIsotopicMassPacketHighLimit;
		}
		set
		{
			_monoIsotopicMassPacketHighLimit = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether to use the calculated mono isotopic mass, otherwise use the mass passed in.
	/// Applies when "CalculateCompositionForMass" is called.
	/// </summary>
	[DataMember]
	public bool UseMonoIsotopicMass
	{
		get
		{
			return _useMonoIsotopicMass;
		}
		set
		{
			_useMonoIsotopicMass = value;
		}
	}

	/// <summary>
	/// Gets or sets the minimum percentage threshold from the base peak for the mono isotopic mass
	/// </summary>
	[DataMember]
	public double MonoIsotopicSearchIntensityThreshold
	{
		get
		{
			return _monoIsotopicSearchIntensityThreshold;
		}
		set
		{
			_monoIsotopicSearchIntensityThreshold = value;
		}
	}

	/// <summary>
	/// Gets or sets the tolerance (in amu) around the value <see cref="P:ThermoFisher.CommonCore.Data.Business.ElementalCompositionParameters.AmuValue" /> to set limits when searching for mono isotopic mass.
	/// Mass of mono isotopic peak must be within a limit of:
	/// <see cref="P:ThermoFisher.CommonCore.Data.Business.ElementalCompositionParameters.AmuValue" />-MonoIsotopicSearchMassThreshold to <see cref="P:ThermoFisher.CommonCore.Data.Business.ElementalCompositionParameters.AmuValue" />+MonoIsotopicSearchMassThreshold
	/// from the mass of a spectral peak.
	/// </summary>
	[DataMember]
	public double MonoIsotopicSearchMassThreshold
	{
		get
		{
			return _monoIsotopicSearchMassThreshold;
		}
		set
		{
			_monoIsotopicSearchMassThreshold = value;
		}
	}

	/// <summary>
	/// Gets or sets the center of search range for mono isotopic mass
	/// as a delta from the mass of a peak being analyzed
	/// </summary>
	[DataMember]
	public double AmuValue
	{
		get
		{
			return _amuValue;
		}
		set
		{
			_amuValue = value;
		}
	}

	/// <summary>
	/// Gets or sets a string representation of the centroiding algorithm (for use on the UI)
	/// </summary>
	[DataMember]
	public string SpectralFitCentroidAlgorithmAsString
	{
		get
		{
			return EnumFormat.ToString(_centroidAlgorithm);
		}
		set
		{
			List<string> list = new List<string>(Enum.GetNames(typeof(CentroidAlgorithm)));
			for (int i = 0; i < list.Count; i++)
			{
				CentroidAlgorithm centroidAlgorithm = (CentroidAlgorithm)i;
				if (EnumFormat.ToString(centroidAlgorithm).Equals(value))
				{
					CentroidAlgorithm = centroidAlgorithm;
					break;
				}
			}
		}
	}

	/// <summary>
	/// Gets or sets a string representation of Missing Penalty Mode (for use in UI)
	/// </summary>
	[DataMember]
	public string SpectralFitMissingPeakPenaltyModeAsString
	{
		get
		{
			return EnumFormat.ToString(_missingPacketPenaltyMode);
		}
		set
		{
			List<string> list = new List<string>(Enum.GetNames(typeof(MissingPacketPenaltyMode)));
			for (int i = 0; i < list.Count; i++)
			{
				MissingPacketPenaltyMode missingPacketPenaltyMode = (MissingPacketPenaltyMode)i;
				if (EnumFormat.ToString(missingPacketPenaltyMode).Equals(value))
				{
					MissingPacketPenaltyMode = missingPacketPenaltyMode;
					break;
				}
			}
		}
	}

	/// <summary>
	/// Gets or sets String representation of the SpectralFitNormalizationMode (for use in UI)
	/// </summary>
	[DataMember]
	public string SpectralFitNormalizationModeAsString
	{
		get
		{
			return EnumFormat.ToString(_normalizationMode);
		}
		set
		{
			List<string> list = new List<string>(Enum.GetNames(typeof(PatternNormalizationMode)));
			for (int i = 0; i < list.Count; i++)
			{
				PatternNormalizationMode patternNormalizationMode = (PatternNormalizationMode)i;
				if (EnumFormat.ToString(patternNormalizationMode).Equals(value))
				{
					NormalizationMode = patternNormalizationMode;
					break;
				}
			}
		}
	}

	/// <summary>
	/// Gets or sets a string representation of <see cref="P:ThermoFisher.CommonCore.Data.Business.ElementalCompositionParameters.MassToleranceMode" /> (for use in UI)
	/// </summary>
	public string MassToleranceModeAsString
	{
		get
		{
			return EnumFormat.ToString(_massToleranceMode);
		}
		set
		{
			List<string> list = new List<string>(Enum.GetNames(typeof(ToleranceMode)));
			for (int i = 0; i < list.Count; i++)
			{
				ToleranceMode toleranceMode = (ToleranceMode)i;
				if (EnumFormat.ToString(toleranceMode).Equals(value))
				{
					_massToleranceMode = toleranceMode;
					break;
				}
			}
		}
	}

	/// <summary>
	/// Gets or sets a string representation of <see cref="P:ThermoFisher.CommonCore.Data.Business.ElementalCompositionParameters.MassToleranceMode" /> (for use in UI)
	/// </summary>
	public string MsMsMassToleranceModeAsString
	{
		get
		{
			return EnumFormat.ToString(_msMsMassToleranceMode);
		}
		set
		{
			List<string> list = new List<string>(Enum.GetNames(typeof(ToleranceMode)));
			for (int i = 0; i < list.Count; i++)
			{
				ToleranceMode toleranceMode = (ToleranceMode)i;
				if (EnumFormat.ToString(toleranceMode).Equals(value))
				{
					_msMsMassToleranceMode = toleranceMode;
					break;
				}
			}
		}
	}

	/// <summary>
	/// Gets or sets a value which determines how the masses are compared. 
	/// </summary>
	[DataMember]
	public ToleranceMode MassToleranceMode
	{
		get
		{
			return _massToleranceMode;
		}
		set
		{
			_massToleranceMode = value;
		}
	}

	/// <summary>
	/// Gets or sets a value which determines the maximum error in mass for something to pass tolerance, in the selected units.
	/// </summary>
	[DataMember]
	public double MassToleranceValue
	{
		get
		{
			return _massToleranceValue;
		}
		set
		{
			_massToleranceValue = value;
		}
	}

	/// <summary>
	/// Gets or sets a value which determines how the fragment masses are compared. 
	/// </summary>
	[DataMember]
	public ToleranceMode MsMsMassToleranceMode
	{
		get
		{
			return _msMsMassToleranceMode;
		}
		set
		{
			_msMsMassToleranceMode = value;
		}
	}

	/// <summary>
	/// Gets or sets a value which determines the maximum error in MSMS mass for something to pass tolerance, in the selected units.
	/// </summary>
	[DataMember]
	public double MsMsMassToleranceValue
	{
		get
		{
			return _msMsMassToleranceValue;
		}
		set
		{
			_msMsMassToleranceValue = value;
		}
	}

	/// <summary>
	/// Gets or sets the charge state of the fragmented ion
	/// </summary>
	[DataMember]
	public int MsMsCharge
	{
		get
		{
			return _msMsCharge;
		}
		set
		{
			_msMsCharge = value;
		}
	}

	/// <summary>
	/// Gets or sets the String representation of <see cref="P:ThermoFisher.CommonCore.Data.Business.ElementalCompositionParameters.NitrogenRule" /> (for use in UI)
	/// </summary>
	[DataMember]
	public string NitrogenRuleForBinding
	{
		get
		{
			return EnumFormat.ToString(_nitrogenRule);
		}
		set
		{
			List<string> list = new List<string>(Enum.GetNames(typeof(NitrogenRule)));
			for (int i = 0; i < list.Count; i++)
			{
				NitrogenRule nitrogenRule = (NitrogenRule)i;
				if (EnumFormat.ToString(nitrogenRule).Equals(value))
				{
					NitrogenRule = nitrogenRule;
					break;
				}
			}
		}
	}

	/// <summary>
	/// Gets or sets a collection of elemental isotopes, and mix/max expected counts in formulae
	/// </summary>
	[DataMember]
	public ElementsSubsetCollection ElementsSubset
	{
		get
		{
			return _elementSubsetCollection;
		}
		set
		{
			_elementSubsetCollection = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the element abundance table "Representative" should be use.
	/// If it is not set, the "Protein" abundance table should be used.
	/// </summary>
	[DataMember]
	public bool UseRepresentativeElements
	{
		get
		{
			return _useRepresentativeElements;
		}
		set
		{
			_useRepresentativeElements = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether elemental compositions to be based on fitting against the supplied scan
	/// </summary>
	[DataMember]
	public bool UseSpectralFitting
	{
		get
		{
			return _useSpectralFitting;
		}
		set
		{
			_useSpectralFitting = value;
			if (_useSpectralFitting)
			{
				UseMonoIsotopicMass = true;
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether calibration reference data should be included in the scan.
	/// </summary>
	[DataMember]
	public bool IncludeReferenceExceptionPeaks
	{
		get
		{
			return _includeReferenceExceptionPeaks;
		}
		set
		{
			_includeReferenceExceptionPeaks = value;
		}
	}

	/// <summary>
	/// Gets or sets the penalty (in units of spectral distance) that is
	/// applied to a missing packet. A packet is missing if it exists in the theoretical pattern (above threshold)
	/// but is not found in the measured spectrum (or too far away)
	/// </summary>
	[DataMember]
	public MissingPacketPenaltyMode MissingPacketPenaltyMode
	{
		get
		{
			return _missingPacketPenaltyMode;
		}
		set
		{
			_missingPacketPenaltyMode = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the distances are normalized to have a max of 1.0 per packet (TrueDistance=false)
	/// or of 1.414 (<c>sqrt(2.0)</c> per packet (TrueDistance=true) 
	/// </summary>
	[DataMember]
	public bool AutomaticIntensityThreshold
	{
		get
		{
			return _automaticIntensityThreshold;
		}
		set
		{
			_automaticIntensityThreshold = value;
		}
	}

	/// <summary>
	/// Gets or sets a threshold which determines the smallest packets that are processed. It is applied to the
	/// theoretical isotope pattern of an elemental composition candidate. Note that it is
	/// not applied to the input spectrum.
	/// Therefore a small pattern (BPI of pattern is smaller than 100% of input pattern
	/// intensity) may still give good fits.
	/// </summary>
	[DataMember]
	public double IntensityThresholdPercentTheory
	{
		get
		{
			return _intensityThresholdPercentTheory;
		}
		set
		{
			_intensityThresholdPercentTheory = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the intensity error is taken absolute. This means that an intensity
	/// that is expected to be 10% but is 9% has an error of 1%. If false, the
	/// intensity error is taken relative. This means that an intensity that is
	/// expected to be 10% but is 9% has an error of 10%.
	/// </summary>
	[DataMember]
	public bool AbsoluteIntensityError
	{
		get
		{
			return _absoluteIntensityError;
		}
		set
		{
			_absoluteIntensityError = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the spectral distance of an isotope packet is scaled with its intensity.
	/// I.e: the intensity deviation of a small packet counts less
	/// than the same deviation of a large packet.
	/// see also <c>WeightMassErrorByAbundance</c> property.
	/// </summary>
	[DataMember]
	public bool WeightIntensityErrorByAbundance
	{
		get
		{
			return _weightIntensityErrorByAbundance;
		}
		set
		{
			_weightIntensityErrorByAbundance = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the spectral distance of an isotope packet is scaled with its intensity.
	/// I.e: the mass deviation of a small packet counts less
	/// than the same deviation of a large packet.
	/// See also <c>WeightIntensityErrorByAbundance</c> property.
	/// </summary>
	[DataMember]
	public bool WeightMassErrorByAbundance
	{
		get
		{
			return _weightMassErrorByAbundance;
		}
		set
		{
			_weightMassErrorByAbundance = value;
		}
	}

	/// <summary>
	/// Gets or sets the error in units of standard deviation that is defined to give a 100% Spectral
	/// Identity value. Together with the property ZeroFitExpectedError, these
	/// two values define the scale of a 0 to 100 % error.
	/// </summary>
	[DataMember]
	public double HundredFitExpectedError
	{
		get
		{
			return _hundredFitExpectedError;
		}
		set
		{
			_hundredFitExpectedError = value;
		}
	}

	/// <summary>
	/// Gets or sets the error in units of standard deviation that is defined to give a 0% Spectral
	/// Identity value. Together with the property HundredFitExpectedError, these
	/// two values define the scale of a 0 to 100 % error.
	/// </summary>
	[DataMember]
	public double ZeroFitExpectedError
	{
		get
		{
			return _zeroFitExpectedError;
		}
		set
		{
			_zeroFitExpectedError = value;
		}
	}

	/// <summary>
	/// Gets or sets The expected intensity error in units of standard deviation.
	/// A standard deviation is defined such that 68% of all events are in the range <c>X ± stddev</c>.
	/// 95% of all events are in the range <c>X± 2*stddev</c>
	/// </summary>
	[DataMember]
	public double ExpectedIntensityError
	{
		get
		{
			return _expectedIntensityError;
		}
		set
		{
			_expectedIntensityError = value;
		}
	}

	/// <summary>
	/// Gets or sets the expected mass error in units of standard deviation.
	/// A standard deviation is defined such that 68% of all events are in the range <c>X ± stddev</c>.
	/// 95% of all events are in the range <c>X± 2*stddev</c>.
	/// </summary>
	[DataMember]
	public double ExpectedMassError
	{
		get
		{
			return _expectedMassError;
		}
		set
		{
			_expectedMassError = value;
		}
	}

	/// <summary>
	/// Gets or sets the charge of the mass for which an
	/// elemental composition is to be calculated.
	/// </summary>
	[DataMember]
	public int Charge
	{
		get
		{
			return _charge;
		}
		set
		{
			_charge = value;
		}
	}

	/// <summary>
	/// Gets or sets the number of elemental compositions
	/// that are to be saved. If the number of fits
	/// found is bigger than "Best", only the best
	/// "Best" ( judged by their deviation from the
	/// specified mass) are saved in the result array.
	/// </summary>
	[DataMember]
	public int KeepBest
	{
		get
		{
			return _keepBest;
		}
		set
		{
			_keepBest = value;
		}
	}

	/// <summary>
	/// Gets or sets the centroid algorithm that is used to calculate isotope
	/// pattern masses from their isotopic distribution
	/// </summary>
	[DataMember]
	public CentroidAlgorithm CentroidAlgorithm
	{
		get
		{
			return _centroidAlgorithm;
		}
		set
		{
			_centroidAlgorithm = value;
		}
	}

	/// <summary>
	/// Gets or sets the number of samples to simulate a peak, at the set resolution.
	/// For example: For C1, at resolution of 1 FWHM, this is the number of samples between mass 11.5 and 12.5, at 50% abundance.
	/// For C1, at resolution of 1000 FWHM, this is the number of samples between mass 11.9995 and 12.0005, at 50% abundance.
	/// This is used when spectral distance calculates theoretical masses of a formula.
	/// Default: 8
	/// </summary>
	[DataMember]
	public int SamplesPerPeak { get; set; } = 8;

	/// <summary>
	/// Gets or sets The lowest acceptable RDB for a formula
	/// Note that this property returns the RDB value for instances that contain
	/// absolute RDB values. It returns RDB / 100 amu for instances that contain relative values.
	/// Rather use GetRDB_LimitLow(). That one works for both absolute and relative limits
	/// </summary>
	[DataMember]
	public double MinRdbLimit
	{
		get
		{
			return _minRdbLimit;
		}
		set
		{
			_minRdbLimit = value;
		}
	}

	/// <summary>
	/// Gets or sets maximum RDB (ring double bond equivalence) for returned formulae.
	/// </summary>
	[DataMember]
	public double MaxRdbLimit
	{
		get
		{
			return _maxRdbLimit;
		}
		set
		{
			_maxRdbLimit = value;
		}
	}

	/// <summary>
	/// Gets or sets the resolution that is used to calculate the theoretical isotope patterns.
	/// This should be the same resolution with which the measured spectrum was acquired.
	/// </summary>
	[DataMember]
	public int Resolution
	{
		get
		{
			return _resolution;
		}
		set
		{
			_resolution = value;
		}
	}

	/// <summary>
	/// Gets or sets a value which determines if the nitrogen rule should be used. <see>NitrogenRule</see> enum for details
	/// </summary>
	[DataMember]
	public NitrogenRule NitrogenRule
	{
		get
		{
			return _nitrogenRule;
		}
		set
		{
			_nitrogenRule = value;
		}
	}

	/// <summary>
	/// Gets or sets the theoretical and measured isotope for the Spectral Distance calculation, 
	/// patterns must be normalized. There are three different normalization modes available.
	/// </summary>
	[DataMember]
	public PatternNormalizationMode NormalizationMode
	{
		get
		{
			return _normalizationMode;
		}
		set
		{
			_normalizationMode = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether to use auto dynamic range, for spectral distance
	/// isotope simulations. If this is true (default) then the property "DynamicRange" is automatically adjusted,
	/// each time spectral distance is calculated, based on the value of the theoretical peak intensity threshold,
	/// which may also be auto calculated, or set to <see cref="P:ThermoFisher.CommonCore.Data.Business.ElementalCompositionParameters.IntensityThresholdPercentTheory" />
	/// depending on the value of <see cref="P:ThermoFisher.CommonCore.Data.Business.ElementalCompositionParameters.AutomaticIntensityThreshold" />.
	/// </summary>
	public bool AutomaticDynamicRange
	{
		get
		{
			return _automaticDynamicRange;
		}
		set
		{
			_automaticDynamicRange = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether to apply dynamic A0 recalibration for spectral distance.  
	/// It is set to false by default. 
	/// </summary>
	public bool DynamicA0Recalibration
	{
		get
		{
			return _dynamicA0Recalibration;
		}
		set
		{
			_dynamicA0Recalibration = value;
		}
	}

	/// <summary>
	/// Gets or sets MSMS SignalToNoise threshold for MSMS spectral processing
	/// </summary>
	public double MsMsSignalToNoiseThreshold
	{
		get
		{
			return _msMsSignalToNoiseThreshold;
		}
		set
		{
			_msMsSignalToNoiseThreshold = value;
		}
	}

	/// <summary>
	/// Gets or sets SpectralFit score cutoff for which formulas MSMS ranking will not be computed - will be set as 1
	/// </summary>
	public double MsMsSFitCutoff
	{
		get
		{
			return _msMsSfitCutoff;
		}
		set
		{
			_msMsSfitCutoff = value;
		}
	}

	/// <summary>
	/// Gets or sets which function will be used for isotope consolidation - 0 - RemoveAllIsotopes, 1 -KeepNonBaseIsotopes
	/// </summary>
	public IsotopeConsolidation SetIsotopeConsolidation { get; set; }

	/// <summary>
	/// Gets or sets the weight applied to Spectral Fit in the re-ranking algorithm
	/// </summary>
	public double SpectralFitWeight
	{
		get
		{
			return _spectralFitWeight;
		}
		set
		{
			_spectralFitWeight = value;
		}
	}

	/// <summary>
	/// Gets or sets the weight applied to MsCoverage in the re-ranking algorithm
	/// </summary>
	public double MsCoverageWeight
	{
		get
		{
			return _msCoverageWeight;
		}
		set
		{
			_msCoverageWeight = value;
		}
	}

	/// <summary>
	/// Gets or sets the weight applied to MsMsCoverage in the re-ranking algorithm
	/// </summary>
	public double MsMsCoverageWeight
	{
		get
		{
			return _msMsCoverageWeight;
		}
		set
		{
			_msMsCoverageWeight = value;
		}
	}

	/// <summary>
	/// Gets or sets the weight applied to PatternCoverageWeight in the re-ranking algorithm
	/// </summary>
	public double PatternCoverageWeight
	{
		get
		{
			return _patternCoverageWeight;
		}
		set
		{
			_patternCoverageWeight = value;
		}
	}

	/// <summary>
	/// Gets or sets the weight applied to MsMsShiftMeasure in the re-ranking algorithm
	/// </summary>
	public double MsMsShiftMeasureWeight
	{
		get
		{
			return _msMsShiftMeasureWeight;
		}
		set
		{
			_msMsShiftMeasureWeight = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether or not re-ranking will be performed
	/// </summary>
	public bool ReRankSpectralDistanceResults { get; set; }

	/// <summary>
	/// Validates the ElementalCompositionParameters values
	/// </summary>
	/// <exception cref="T:System.InvalidOperationException">
	/// if parameters are invalid
	/// </exception>
	public void Validate()
	{
		if (ZeroFitExpectedError <= HundredFitExpectedError)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, "ZeroFitExpectedError must be greater than HundredFitExpectedError. Actual values are ZeroFitExpectedError='{0}', HundredFitExpectedError='{1}'.", ZeroFitExpectedError, HundredFitExpectedError));
		}
		if (MaxRdbLimit <= MinRdbLimit)
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, "MaxRdbLimit must be greater than MinRdbLimit. Actual values are MaxRdbLimit='{0}', MinRdbLimit='{1}'.", MaxRdbLimit, MinRdbLimit));
		}
	}

	/// <summary>
	/// Creates a default list of elements for searching
	/// </summary>
	public override void PerformDefaultSettings()
	{
		base.PerformDefaultSettings();
		if (_elementSubsetCollection.Count == 0)
		{
			_elementSubsetCollection.Add(new ElementSubset
			{
				Sign = "C",
				Mass = 12.011,
				NominalMass = 12,
				MinimumAbs = 0.0,
				MaximumAbs = 30.0,
				MinimumRelative = 0.0,
				MaximumRelative = 10.0,
				UseRatio = false,
				InUse = true
			});
			_elementSubsetCollection.Add(new ElementSubset
			{
				Sign = "H",
				Mass = 1.008,
				NominalMass = 1,
				MinimumAbs = 0.0,
				MaximumAbs = 60.0,
				MinimumRelative = 0.0,
				MaximumRelative = 10.0,
				UseRatio = false,
				InUse = true
			});
			_elementSubsetCollection.Add(new ElementSubset
			{
				Sign = "O",
				Mass = 15.995,
				NominalMass = 16,
				MinimumAbs = 0.0,
				MaximumAbs = 15.0,
				MinimumRelative = 0.0,
				MaximumRelative = 10.0,
				UseRatio = false,
				InUse = true
			});
			_elementSubsetCollection.Add(new ElementSubset
			{
				Sign = "N",
				Mass = 14.007,
				NominalMass = 14,
				MinimumAbs = 0.0,
				MaximumAbs = 10.0,
				MinimumRelative = 0.0,
				MaximumRelative = 10.0,
				UseRatio = false,
				InUse = true
			});
		}
		ElementsSubset = _elementSubsetCollection;
	}
}
