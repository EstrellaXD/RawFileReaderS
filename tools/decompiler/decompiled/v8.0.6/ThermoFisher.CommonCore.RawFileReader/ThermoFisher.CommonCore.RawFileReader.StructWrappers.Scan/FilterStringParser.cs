using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.DataModel;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
/// This class parses a filter string.
/// </summary>
internal class FilterStringParser : ScanEventDecorator
{
	/// <summary>
	/// Parsing states for a mass range
	/// </summary>
	private enum ParseMassRangesState
	{
		/// <summary>
		/// Expect low mass
		/// </summary>
		LoMass,
		/// <summary>
		/// Expect comma or dash (separator)
		/// </summary>
		CommaDash,
		/// <summary>
		/// Expect high mass
		/// </summary>
		HiMass,
		/// <summary>
		/// Expect an energy value (after "@")
		/// </summary>
		SidEnergy,
		/// <summary>
		/// Expect a common or "]"
		/// </summary>
		CommaBracket,
		/// <summary>
		/// Found "]"
		/// </summary>
		RBracket
	}

	/// <summary>
	/// Parsing states for a simple double "123.456"
	/// </summary>
	private enum ParseSimpleDoubleState
	{
		/// <summary>
		/// Digits before decimal point
		/// </summary>
		DigitsBeforeDecimal,
		/// <summary>
		/// Decimal point
		/// </summary>
		Decimal,
		/// <summary>
		/// Digits after decimal point
		/// </summary>
		DigitsAfterDecimal
	}

	/// <summary>
	/// to build a lookup dictionary of various tables
	/// An enum is used to indicate which table a string belongs to (such as a "detector type" or a "source type").
	/// Only needed where:
	/// <c>
	/// * A list of exclusive items maps to an enum
	/// * A single flag has multiple characters (such as lock or sps)
	/// </c>
	/// Some (legacy) names me be obsoleted by a list.
	/// Most items with only 1 string act the same as single char On/Off types, allowing a "!' prefix
	/// </summary>
	private enum TokenCategory
	{
		IonizationMode,
		MassAnalyzer,
		SectorScan,
		Lock,
		FreeRegion,
		SpsMultiNotch,
		PhotoIonization,
		Polarity,
		ScanDataType,
		Corona,
		DataDependent,
		SupplementalActivation,
		MultiStateActivation,
		AccurateMass,
		ScanMode,
		Multiplex,
		MsOrder,
		Detector,
		MultiPhotonDissociation,
		ElectronCaptureDissociation,
		SourceFragmentation,
		CompensationVoltage
	}

	private class TokenWithCategory
	{
		/// <summary>
		/// which list does ths belong to?
		/// (such as the string Corona may be in the Beer category.)
		/// </summary>
		public TokenCategory Category { get; set; }

		/// <summary>
		/// Enum numerical value within this category. For example suppose we have "enum Beer: Bud, Corona"
		/// then Bud would be EnumValue 0 and Corona EnumValue 1
		/// </summary>
		public int EnumValue { get; set; }

		public int StringIndex { get; internal set; }
	}

	private static class FilterParserDictionary
	{
		private static Dictionary<string, TokenWithCategory> _tokensDictionary;

		public static Dictionary<string, TokenWithCategory> Tokens => _tokensDictionary;

		static FilterParserDictionary()
		{
			_tokensDictionary = new Dictionary<string, TokenWithCategory>();
			AddTokenSet<ScanFilterEnums.IonizationModes>(TokenCategory.IonizationMode, FilterStringTokens.IonizationModeTokenNames);
			AddTokenSet<MassAnalyzerType>(TokenCategory.MassAnalyzer, FilterStringTokens.MassAnalyzerTokenNames);
			AddTokenSet<SectorScanType>(TokenCategory.SectorScan, FilterStringTokens.SectorScanTokenNames);
			AddTokenSet<ScanFilterEnums.OnOffTypes>(TokenCategory.Lock, FilterStringTokens.LockTokenNames);
			AddTokenSet<ScanFilterEnums.FreeRegions>(TokenCategory.FreeRegion, FilterStringTokens.FreeRegionTokenNames);
			AddTokenSet<ScanFilterEnums.OffOnTypes>(TokenCategory.SpsMultiNotch, FilterStringTokens.SpsMultiNotchTokenNames);
			AddTokenSet<ScanFilterEnums.OnOffTypes>(TokenCategory.PhotoIonization, FilterStringTokens.PhotoIonizationTokenNames);
			AddTokenSet<ScanFilterEnums.PolarityTypes>(TokenCategory.Polarity, FilterStringTokens.PolarityTokenNames);
			AddTokenSet<ScanFilterEnums.ScanDataTypes>(TokenCategory.ScanDataType, FilterStringTokens.ScanDataTypeTokenNames);
			AddTokenSet<ScanFilterEnums.OnOffTypes>(TokenCategory.Corona, FilterStringTokens.CoronaTokenNames);
			AddTokenSet<ScanFilterEnums.IsDependent>(TokenCategory.DataDependent, FilterStringTokens.DataDependentTokenNames);
			AddTokenSet<ScanFilterEnums.OffOnTypes>(TokenCategory.SupplementalActivation, FilterStringTokens.SupplementalActivationTokenNames);
			AddTokenSet<ScanFilterEnums.OffOnTypes>(TokenCategory.MultiStateActivation, FilterStringTokens.MultiStateActivationTokenNames);
			AddTokenSet<ScanFilterEnums.AccurateMassTypes>(TokenCategory.AccurateMass, FilterStringTokens.AccurateMassTokenNames);
			AddTokenSet<ScanFilterEnums.ScanTypes>(TokenCategory.ScanMode, FilterStringTokens.ScanModeTokenNames);
			AddTokenSet<ScanFilterEnums.OffOnTypes>(TokenCategory.Multiplex, FilterStringTokens.MultiplexTokenNames);
			AddTokenSet2<ScanFilterEnums.MSOrderTypes>(TokenCategory.MsOrder, MsOrderNames, FilterStructValues);
			AddTokenSet<ScanFilterEnums.DetectorType>(TokenCategory.Detector, FilterStringTokens.DetectorTokenNames);
			AddTokenSet<ScanFilterEnums.OnAnyOffTypes>(TokenCategory.MultiPhotonDissociation, FilterStringTokens.MultiPhotonDissociationTokenNames);
			AddTokenSet<ScanFilterEnums.OnAnyOffTypes>(TokenCategory.ElectronCaptureDissociation, FilterStringTokens.ElectronCaptureDissociationTokenNames);
			AddTokenSet<ScanFilterEnums.OnOffTypes>(TokenCategory.SourceFragmentation, FilterStringTokens.SourceFragmentationTokenNames);
			AddTokenSet<ScanFilterEnums.OnOffTypes>(TokenCategory.CompensationVoltage, FilterStringTokens.CompensationVoltageTokenNames);
			static void AddTokenSet<T>(TokenCategory category, string[] names, bool filterAny = true) where T : Enum
			{
				Array values = Enum.GetValues(typeof(T));
				int num = 0;
				foreach (object item in values)
				{
					string text = names[num];
					if (!string.IsNullOrEmpty(text))
					{
						bool flag = false;
						if (filterAny)
						{
							flag = text.Contains("Any");
						}
						if (!flag)
						{
							_tokensDictionary.Add(text.ToLower(), new TokenWithCategory
							{
								Category = category,
								EnumValue = (int)item,
								StringIndex = num
							});
						}
					}
					num++;
				}
			}
			static void AddTokenSet2<T>(TokenCategory category, string[] names, int[] values) where T : Enum
			{
				int num = 0;
				foreach (int enumValue in values)
				{
					string text = names[num];
					if (!string.IsNullOrEmpty(text))
					{
						_tokensDictionary.Add(text.ToLower(), new TokenWithCategory
						{
							Category = category,
							EnumValue = enumValue,
							StringIndex = num
						});
					}
					num++;
				}
			}
		}
	}

	/// <summary>
	/// List of available single character flag names
	/// Names which are part of a another list are commented out
	/// (not available to be parsed as a letter code with "!" option)
	/// for example, c=centroid is part of the list "c,p" where p is Profile
	/// so you cannot have "both c and p" or "!c" etc.
	/// </summary>
	private enum FlagNames
	{
		None,
		ParamA,
		ParamB,
		LowerE,
		ParamF,
		LowerG,
		LowerH,
		LowerI,
		LowerJ,
		LowerK,
		LowerL,
		LowerM,
		LowerN,
		LowerO,
		LowerQ,
		ParamR,
		LowerS,
		TurboScan,
		Ultra,
		ParamV,
		Wideband,
		LowerX,
		LowerY,
		UpperA,
		UpperB,
		Enhanced,
		UpperF,
		UpperG,
		UpperH,
		UpperI,
		UpperJ,
		UpperK,
		UpperL,
		UpperM,
		UpperN,
		UpperO,
		UpperQ,
		UpperR,
		UpperS,
		UpperT,
		UpperU,
		UpperV,
		UpperW,
		UpperX,
		UpperY
	}

	/// <summary>
	/// The precursor activation.
	/// </summary>
	private class PrecursorActivation
	{
		/// <summary>
		/// Gets or sets a value indicating whether it is a multiple activation.
		/// </summary>
		public bool IsMultiple { private get; set; }

		/// <summary>
		/// Gets or sets the mode of activation.
		/// </summary>
		public ActivationType Activation { private get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the precursor energy is valid.
		/// If it is valid, it is shown as "@energy"
		/// </summary>
		public bool PrecursorEnergyIsValid { private get; set; }

		/// <summary>
		/// Gets or sets the precursor energy.
		/// </summary>
		public double PrecursorEnergy { get; set; }

		/// <summary>
		/// Gets or sets the MS order.
		/// </summary>
		public int MsOrder { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.FilterStringParser.PrecursorActivation" /> class.
		/// </summary>
		public PrecursorActivation()
		{
			MsOrder = -1;
			Activation = ActivationType.Any;
		}

		/// <summary>
		/// Convert to energy ex.
		/// </summary>
		/// <returns>
		/// The converted value
		/// </returns>
		internal uint ToEnergyEx()
		{
			return (uint)((int)Activation << 1) | (PrecursorEnergyIsValid ? 1u : 0u) | (uint)(IsMultiple ? 4096 : 0);
		}
	}

	private char CommaSep = ',';

	private char DecimalPt = '.';

	private static readonly string[] MsOrderNames = new string[103]
	{
		"ms", "ms2", "ms3", "ms4", "ms5", "ms6", "ms7", "ms8", "ms9", "ms10",
		"ms11", "ms12", "ms13", "ms14", "ms15", "ms16", "ms17", "ms18", "ms19", "ms20",
		"ms21", "ms22", "ms23", "ms24", "ms25", "ms26", "ms27", "ms28", "ms29", "ms30",
		"ms31", "ms32", "ms33", "ms34", "ms35", "ms36", "ms37", "ms38", "ms39", "ms40",
		"ms41", "ms42", "ms43", "ms44", "ms45", "ms46", "ms47", "ms48", "ms49", "ms50",
		"ms51", "ms52", "ms53", "ms54", "ms55", "ms56", "ms57", "ms58", "ms59", "ms60",
		"ms61", "ms62", "ms63", "ms64", "ms65", "ms66", "ms67", "ms68", "ms69", "ms70",
		"ms71", "ms72", "ms73", "ms74", "ms75", "ms76", "ms77", "ms78", "ms79", "ms80",
		"ms81", "ms82", "ms83", "ms84", "ms85", "ms86", "ms87", "ms88", "ms89", "ms90",
		"ms91", "ms92", "ms93", "ms94", "ms95", "ms96", "ms97", "ms98", "ms99", "ms100",
		"cng", "cnl", "pr"
	};

	private static readonly int[] FilterStructValues = new int[103]
	{
		1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
		11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
		21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
		31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
		41, 42, 43, 44, 45, 46, 47, 48, 49, 50,
		51, 52, 53, 54, 55, 56, 57, 58, 59, 60,
		61, 62, 63, 64, 65, 66, 67, 68, 69, 70,
		71, 72, 73, 74, 75, 76, 77, 78, 79, 80,
		81, 82, 83, 84, 85, 86, 87, 88, 89, 90,
		91, 92, 93, 94, 95, 96, 97, 98, 99, 100,
		-3, -2, -1
	};

	private static readonly int[] PrecursorValues = new int[103]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
		10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
		20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
		30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
		40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
		50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
		60, 61, 62, 63, 64, 65, 66, 67, 68, 69,
		70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
		80, 81, 82, 83, 84, 85, 86, 87, 88, 89,
		90, 91, 92, 93, 94, 95, 96, 97, 98, 99,
		1, 1, 1
	};

	private bool[] _upperSet = new bool[26];

	private bool[] _lowerSet = new bool[26];

	/// <summary>
	/// Lookup table of valid precursor activation codes
	/// </summary>
	private static readonly Dictionary<string, ActivationType> IonModeSet = new Dictionary<string, ActivationType>
	{
		{
			"sa",
			ActivationType.SAactivation
		},
		{
			"ecd",
			ActivationType.ElectronCaptureDissociation
		},
		{
			"pqd",
			ActivationType.PQD
		},
		{
			"mpd",
			ActivationType.MultiPhotonDissociation
		},
		{
			"etd",
			ActivationType.ElectronTransferDissociation
		},
		{
			"hcd",
			ActivationType.HigherEnergyCollisionalDissociation
		},
		{
			"ptr",
			ActivationType.ProtonTransferReaction
		},
		{
			"cid",
			ActivationType.CollisionInducedDissociation
		},
		{
			"netd",
			ActivationType.NegativeElectronTransferDissociation
		},
		{
			"nptr",
			ActivationType.NegativeProtonTransferReaction
		},
		{
			"uvpd",
			ActivationType.UltraVioletPhotoDissociation
		},
		{
			"eid",
			ActivationType.ElectronInducedDissociation
		},
		{
			"ee",
			ActivationType.ElectronEnergy
		},
		{
			"modeC",
			ActivationType.ModeC
		},
		{
			"modeD",
			ActivationType.ModeD
		},
		{
			"modeE",
			ActivationType.ModeE
		},
		{
			"modeF",
			ActivationType.ModeF
		},
		{
			"modeG",
			ActivationType.ModeG
		},
		{
			"modeH",
			ActivationType.ModeH
		},
		{
			"modeI",
			ActivationType.ModeI
		},
		{
			"modeJ",
			ActivationType.ModeJ
		},
		{
			"modeK",
			ActivationType.ModeK
		},
		{
			"modeL",
			ActivationType.ModeL
		},
		{
			"modeM",
			ActivationType.ModeM
		},
		{
			"modeN",
			ActivationType.ModeN
		},
		{
			"modeO",
			ActivationType.ModeO
		},
		{
			"modeP",
			ActivationType.ModeP
		},
		{
			"modeQ",
			ActivationType.ModeQ
		},
		{
			"modeR",
			ActivationType.ModeR
		},
		{
			"modeS",
			ActivationType.ModeS
		},
		{
			"modeT",
			ActivationType.ModeT
		},
		{
			"modeU",
			ActivationType.ModeU
		},
		{
			"modeV",
			ActivationType.ModeV
		},
		{
			"modeW",
			ActivationType.ModeW
		},
		{
			"modeX",
			ActivationType.ModeX
		},
		{
			"modeY",
			ActivationType.ModeY
		},
		{
			"modeZ",
			ActivationType.ModeZ
		}
	};

	private readonly int _doubleLength;

	private int _massPrecision;

	private ScanFilterEnums.SegmentScanEventType _segmentScanEvent;

	private double[] _simSourceCidArray;

	private uint[] _simSourceCidValidArray;

	private double[,] _massRangeArray;

	private double[] _massArray;

	private int _massRangeCount;

	private double[] _precEnergyArray;

	private PrecursorActivation[] _precEnergyValidArray;

	private List<PrecursorActivation> _precursorActivations;

	private double _minMass;

	private double _maxMass;

	private int _scanSegmentNum;

	private int _scanScanEventNum;

	private ScanFilterEnums.FilterSourceLowVal _filterSourceLowValState;

	private ScanFilterEnums.FilterSourceHighVal _filterSourceHighValState;

	private double _sourceCidLowVal;

	private double _sourceCidHighVal;

	private ScanFilterEnums.FilterSourceLowVal _compensationVoltageLowValState;

	private ScanFilterEnums.FilterSourceHighVal _compensationVoltageHighValState;

	private double _compensationVoltageLowVal;

	private double _compensationVoltageHighVal;

	private int _currentPrecursor;

	private int _msOrder;

	private int _precMassesCount;

	private bool _ionizationModeSet;

	private bool _massAnalyzerSet;

	private bool _sectorScanSet;

	private bool _lockSet;

	private bool _freeRegionSet;

	private bool _ultraSet;

	private bool _enhancedSet;

	private bool _paramASet;

	private bool _paramBSet;

	private bool _paramFSet;

	private bool _spsMultiNotchSet;

	private bool _paramRSet;

	private bool _paramVSet;

	private bool _multiPhotonDissociationSet;

	private bool _electronCaptureDissociationSet;

	private bool _photoIonizationSet;

	private bool _polaritySet;

	private bool _scanDataTypeSet;

	private bool _coronaSet;

	private bool _sourceFragmentationSet;

	private bool _compensationVoltageSet;

	private bool _dataDependentSet;

	private bool _wideBandSet;

	private bool _supplementalActivationSet;

	private bool _multiStateActivationSet;

	private bool _accurateMassSet;

	private bool _turboScanSet;

	private bool _scanModeSet;

	private bool _multiplexSet;

	private bool _msOrderSet;

	private bool _detectorSet;

	private bool _detectorValueSet;

	private bool _hasSimModeCid;

	private readonly FlagNames[] _lowerCaseMapping = new FlagNames[26]
	{
		FlagNames.ParamA,
		FlagNames.ParamB,
		FlagNames.None,
		FlagNames.None,
		FlagNames.LowerE,
		FlagNames.ParamF,
		FlagNames.LowerG,
		FlagNames.LowerH,
		FlagNames.LowerI,
		FlagNames.LowerJ,
		FlagNames.LowerK,
		FlagNames.LowerL,
		FlagNames.LowerM,
		FlagNames.LowerN,
		FlagNames.LowerO,
		FlagNames.None,
		FlagNames.LowerQ,
		FlagNames.ParamR,
		FlagNames.LowerS,
		FlagNames.TurboScan,
		FlagNames.Ultra,
		FlagNames.ParamV,
		FlagNames.Wideband,
		FlagNames.LowerX,
		FlagNames.LowerY,
		FlagNames.None
	};

	/// <summary>
	/// Flag lookup table for "character offset from upper case A".
	/// </summary>
	private readonly FlagNames[] _upperCaseMapping = new FlagNames[26]
	{
		FlagNames.UpperA,
		FlagNames.UpperB,
		FlagNames.None,
		FlagNames.None,
		FlagNames.Enhanced,
		FlagNames.UpperF,
		FlagNames.UpperG,
		FlagNames.UpperH,
		FlagNames.UpperI,
		FlagNames.UpperJ,
		FlagNames.UpperK,
		FlagNames.UpperL,
		FlagNames.UpperM,
		FlagNames.UpperN,
		FlagNames.UpperO,
		FlagNames.None,
		FlagNames.UpperQ,
		FlagNames.UpperR,
		FlagNames.UpperS,
		FlagNames.UpperT,
		FlagNames.UpperU,
		FlagNames.UpperV,
		FlagNames.UpperW,
		FlagNames.UpperX,
		FlagNames.UpperY,
		FlagNames.None
	};

	/// <summary>
	/// Gets or sets the energy precision.
	/// </summary>
	public int EnergyPrecision { get; set; }

	/// <summary>
	/// Gets or sets the meta filters. (ETD, CID, HCD, UVPD, EID)
	/// </summary>
	private MetaFilterType MetaFilters { get; set; }

	/// <summary>
	/// Sets the mass precision.
	/// </summary>
	public int MassPrecision
	{
		set
		{
			_massPrecision = value;
		}
	}

	/// <summary>
	/// format for localization
	/// </summary>
	public IFormatProvider FormatProvider { get; internal set; }

	/// <summary>
	/// Separator for list of values such as "cats, dogs"
	/// </summary>
	public string ListSeparator { get; internal set; }

	/// <summary>
	/// Separator for decimal point, such as "34.67"
	/// </summary>
	public string DecimalSeparator { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.FilterStringParser" /> class.
	/// </summary>
	public FilterStringParser()
		: this(new ScanEvent())
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.FilterStringParser" /> class.
	/// </summary>
	/// <param name="scanEvent">
	/// The scan event.
	/// </param>
	private FilterStringParser(IRawFileReaderScanEvent scanEvent)
		: base(scanEvent)
	{
		_massPrecision = 2;
		_doubleLength = 9;
		EnergyPrecision = 2;
		InitializeInternalProperties(scanEvent);
		InitializeParser();
	}

	/// <summary>
	/// Get filter from string (by parsing a string)
	/// </summary>
	/// <param name="filter">
	/// The filter sting to parse
	/// </param>
	/// <returns>
	/// The parsed filter
	/// </returns>
	public IScanFilterPlus GetFilterFromString(string filter)
	{
		if (FormatProvider == null)
		{
			_ = CultureInfo.InvariantCulture;
		}
		if (!string.IsNullOrEmpty(ListSeparator))
		{
			CommaSep = ListSeparator[0];
		}
		if (!string.IsNullOrEmpty(DecimalSeparator))
		{
			DecimalPt = DecimalSeparator[0];
		}
		if (ParseFilterStructString(filter))
		{
			return new WrappedScanFilter(ToFilterScanEvent(fromScan: false));
		}
		return null;
	}

	/// <summary>
	/// Parses the filter structure string.
	/// </summary>
	/// <param name="filterString">The filter string.</param>
	/// <returns>
	/// true if parse is valid</returns>
	public bool ParseFilterStructString(string filterString)
	{
		return ParseFilter(filterString) != ScanFilterEnums.FilterStringParseState.Bad;
	}

	/// <summary>
	/// Convert to type IFilterScanEvent
	/// </summary>
	/// <param name="fromScan">
	/// true if this came from a scan's event.
	/// </param>
	/// <returns>
	/// Converted object
	/// </returns>
	public IFilterScanEvent ToFilterScanEvent(bool fromScan = true)
	{
		if (_segmentScanEvent == ScanFilterEnums.SegmentScanEventType.SegmentScanEventSet)
		{
			base.ScanTypeIndex = (ushort)_scanScanEventNum | (_scanSegmentNum << 16);
		}
		else
		{
			base.ScanTypeIndex = -1;
		}
		base.MsOrder = (ScanFilterEnums.MSOrderTypes)_msOrder;
		if (_massRangeCount > 0)
		{
			base.MassRanges = new MassRangeStruct[_massRangeCount];
			for (int i = 0; i < _massRangeCount; i++)
			{
				base.MassRanges[i] = new MassRangeStruct(_massRangeArray[i, 0], _massRangeArray[i, 1]);
			}
		}
		else
		{
			base.MassRanges = Array.Empty<MassRangeStruct>();
		}
		FilterScanEvent filterScanEvent = new FilterScanEvent(this, fromScan)
		{
			MetaFilters = MetaFilters,
			LowerCaseFlags = base.LowerCaseFlags,
			UpperCaseFlags = base.UpperCaseFlags,
			LowerCaseApplied = base.LowerCaseApplied,
			UpperCaseApplied = base.UpperCaseApplied
		};
		List<double> list = new List<double>();
		base.SourceFragmentations = Array.Empty<double>();
		int num = 0;
		filterScanEvent.SourceFragmentation = base.SourceFragmentation;
		filterScanEvent.SetSourceFragmentationType(base.SourceFragmentationType);
		switch (base.SourceFragmentationType)
		{
		case ScanFilterEnums.VoltageTypes.NoValue:
			filterScanEvent.NumSourceFragmentationInfoValues(0);
			break;
		case ScanFilterEnums.VoltageTypes.SingleValue:
			filterScanEvent.NumSourceFragmentationInfoValues(1);
			filterScanEvent.SourceCidInfoValue(0, _sourceCidLowVal);
			list.Add(_sourceCidLowVal);
			num = 1;
			break;
		case ScanFilterEnums.VoltageTypes.Ramp:
			filterScanEvent.NumSourceFragmentationInfoValues(2);
			filterScanEvent.SourceCidInfoValue(0, _sourceCidLowVal);
			filterScanEvent.SourceCidInfoValue(1, _sourceCidHighVal);
			num = 2;
			list.Add(_sourceCidLowVal);
			list.Add(_sourceCidHighVal);
			break;
		case ScanFilterEnums.VoltageTypes.SIM:
		{
			list.Clear();
			int massRangeCount = _massRangeCount;
			filterScanEvent.NumSourceFragmentationInfoValues(massRangeCount);
			num = massRangeCount;
			for (int j = 0; j < massRangeCount; j++)
			{
				if (GetSimSourceCid(j, out var energy) == ScanFilterEnums.SIMSourceCIDEnergy.SIMSourceCIDEnergySet)
				{
					filterScanEvent.SourceCidInfoValue(j, energy);
					list.Add(energy);
				}
				else
				{
					list.Add(0.0);
				}
			}
			break;
		}
		case ScanFilterEnums.VoltageTypes.Any:
			filterScanEvent.SetSourceFragmentationType(ScanFilterEnums.VoltageTypes.Any);
			filterScanEvent.NumSourceFragmentationInfoValues(0);
			break;
		}
		filterScanEvent.CompensationVoltage = base.CompensationVoltage;
		filterScanEvent.SetCompensationVoltageType(base.CompensationVoltageType);
		switch (base.CompensationVoltageType)
		{
		case ScanFilterEnums.VoltageTypes.SingleValue:
			filterScanEvent.NumCompensationVoltageInfoValues(1 + num);
			filterScanEvent.CompensationVoltageInfoValue(0, _compensationVoltageLowVal);
			list.Add(_compensationVoltageLowVal);
			break;
		case ScanFilterEnums.VoltageTypes.Ramp:
			filterScanEvent.NumCompensationVoltageInfoValues(2 + num);
			filterScanEvent.CompensationVoltageInfoValue(0, _compensationVoltageLowVal);
			filterScanEvent.CompensationVoltageInfoValue(1, _compensationVoltageHighVal);
			list.Add(_compensationVoltageLowVal);
			list.Add(_compensationVoltageHighVal);
			break;
		case ScanFilterEnums.VoltageTypes.SIM:
		{
			int massRangeCount2 = _massRangeCount;
			filterScanEvent.NumCompensationVoltageInfoValues(massRangeCount2 + num);
			for (int k = 0; k < massRangeCount2; k++)
			{
				if (GetSimCompensationVoltage(k, out var energy2) == ScanFilterEnums.SIMCompensationVoltageEnergy.SIMCompensationVoltageEnergySet)
				{
					filterScanEvent.CompensationVoltageInfoValue(k, energy2);
					list.Add(energy2);
				}
				else
				{
					list.Add(0.0);
				}
			}
			break;
		}
		}
		filterScanEvent.SourceFragmentations = list.ToArray();
		int totalNumberActivations = GetTotalNumberActivations();
		int num2 = 0;
		filterScanEvent.SetNumMasses(totalNumberActivations);
		base.Reactions = Array.Empty<Reaction>();
		if (totalNumberActivations > 0)
		{
			int num3 = 0;
			while (num3 < _precMassesCount)
			{
				uint precursorEnergyEx = GetPrecursorEnergyEx(num3, out var energy3);
				double value = _massArray[num3];
				filterScanEvent.SetMasses(num2, value);
				filterScanEvent.SetPrecursorEnergyIsValidEx(num2, precursorEnergyEx);
				filterScanEvent.SetPrecursorEnergy(num2, energy3);
				int num4 = MultipleActivationsForMsnOrder(num3);
				if (num4 > 0)
				{
					int lastMultiActivation = -1;
					for (int l = 0; l < num4; l++)
					{
						int num5 = NextMultipleActivationForOrder(num3, lastMultiActivation);
						precursorEnergyEx = GetPrecursorEnergyExtra(num5, out energy3);
						num2++;
						lastMultiActivation = num5;
						filterScanEvent.SetPrecursorEnergyIsValidEx(num2, precursorEnergyEx);
						filterScanEvent.SetPrecursorEnergy(num2, energy3);
					}
				}
				num3++;
				num2++;
			}
		}
		filterScanEvent.CreateReactions();
		filterScanEvent.SetFilterMassResolutionByMassPrecision(_massPrecision);
		return filterScanEvent;
	}

	/// <summary>
	/// Search for open and close parenthesis, or a given style
	/// </summary>
	/// <param name="filterString">
	/// The filter string.
	/// </param>
	/// <param name="openParenthesisIndex">
	/// The open parenthesis index.
	/// </param>
	/// <param name="closeParenthesisIndex">
	/// The close parenthesis index.
	/// </param>
	/// <param name="openMatch">
	/// The open match count.
	/// </param>
	/// <param name="closeMatch">
	/// The close match count.
	/// </param>
	/// <param name="filterLength">
	/// The filter length.
	/// </param>
	/// <param name="openParenthesisCharacter">
	/// The open parenthesis character.
	/// </param>
	/// <param name="closeParenthesisCharacter">
	/// The close parenthesis character.
	/// </param>
	private static void SearchForOpenAndCloseParenthesis(char[] filterString, ref int openParenthesisIndex, ref int closeParenthesisIndex, ref int openMatch, ref int closeMatch, int filterLength, char openParenthesisCharacter, char closeParenthesisCharacter)
	{
		for (int i = 0; i < filterLength; i++)
		{
			if (filterString[i] == openParenthesisCharacter && ++openMatch == 1)
			{
				openParenthesisIndex = i;
			}
			if (filterString[i] == closeParenthesisCharacter && ++closeMatch == 1)
			{
				closeParenthesisIndex = i;
			}
			if (openMatch > 1 || closeMatch > 1)
			{
				break;
			}
		}
	}

	/// <summary>
	/// Parse The ion mode.
	/// </summary>
	/// <param name="token">
	/// The token.
	/// </param>
	/// <param name="i">
	/// The index into the token.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.String" />.
	/// </returns>
	private static string IonMode(string token, int i)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		int length = token.Length;
		while (i < length && char.IsLetter(token[i]) && num < 5)
		{
			stringBuilder.Append(token[i]);
			num++;
			i++;
		}
		return stringBuilder.ToString();
	}

	/// <summary>
	/// Parse one token
	/// </summary>
	/// <param name="token">The token.</param>
	/// <param name="featureAlreadySet">This is "true" if this feature has already been set</param>
	/// <param name="names">The names.</param>
	/// <param name="setMode">The set mode.</param>
	/// <returns>updated parse state</returns>
	private static ScanFilterEnums.FilterStringParseState FieldParser(ref string token, ref bool featureAlreadySet, string[] names, Action<int> setMode)
	{
		if (featureAlreadySet)
		{
			return ScanFilterEnums.FilterStringParseState.Good;
		}
		int num = names.Length;
		for (int i = 0; i < num; i++)
		{
			if (string.Compare(names[i], token, StringComparison.OrdinalIgnoreCase) == 0)
			{
				featureAlreadySet = true;
				setMode(i);
				token = string.Empty;
				return ScanFilterEnums.FilterStringParseState.Good;
			}
		}
		return ScanFilterEnums.FilterStringParseState.Incomplete;
	}

	/// <summary>
	/// Initialize internal properties.
	/// </summary>
	/// <param name="scanEvent">
	/// The scan event.
	/// </param>
	private void InitializeInternalProperties(IRawFileReaderScanEvent scanEvent)
	{
		base.DependentDataFlag = scanEvent.DependentDataFlag;
		base.Wideband = scanEvent.Wideband;
		base.SupplementalActivation = scanEvent.SupplementalActivation;
		base.MultiStateActivation = scanEvent.MultiStateActivation;
		base.AccurateMassType = scanEvent.AccurateMassType;
		base.Detector = scanEvent.Detector;
		base.SourceFragmentation = scanEvent.SourceFragmentation;
		base.SourceFragmentationType = scanEvent.SourceFragmentationType;
		base.CompensationVoltage = scanEvent.CompensationVoltage;
		base.CompensationVoltageType = scanEvent.CompensationVoltageType;
		base.TurboScan = scanEvent.TurboScan;
		base.Lock = scanEvent.Lock;
		base.Multiplex = scanEvent.Multiplex;
		base.ParamA = scanEvent.ParamA;
		base.ParamB = scanEvent.ParamB;
		base.ParamF = scanEvent.ParamF;
		base.SpsMultiNotch = scanEvent.SpsMultiNotch;
		base.ParamR = scanEvent.ParamR;
		base.ParamV = scanEvent.ParamV;
		base.Ultra = scanEvent.Ultra;
		base.Enhanced = scanEvent.Enhanced;
		base.ElectronCaptureDissociationType = scanEvent.ElectronCaptureDissociationType;
		base.MultiPhotonDissociationType = scanEvent.MultiPhotonDissociationType;
		base.Corona = scanEvent.Corona;
		base.DetectorValue = scanEvent.DetectorValue;
		base.ElectronCaptureDissociation = scanEvent.ElectronCaptureDissociation;
		base.ElectronTransferDissociation = scanEvent.ElectronTransferDissociation;
		base.ElectronTransferDissociationType = scanEvent.ElectronTransferDissociationType;
		base.FreeRegion = scanEvent.FreeRegion;
		base.HigherEnergyCid = scanEvent.HigherEnergyCid;
		base.HigherEnergyCidType = scanEvent.HigherEnergyCidType;
		base.IonizationMode = scanEvent.IonizationMode;
		base.IsCustom = scanEvent.IsCustom;
		base.IsValid = scanEvent.IsValid;
		base.MsOrder = scanEvent.MsOrder;
		base.MassAnalyzerType = scanEvent.MassAnalyzerType;
		base.MassCalibrators = scanEvent.MassCalibrators;
		base.MassRanges = scanEvent.MassRanges;
		base.MultiPhotonDissociation = scanEvent.MultiPhotonDissociation;
		base.Name = scanEvent.Name;
		base.PhotoIonization = scanEvent.PhotoIonization;
		base.Polarity = scanEvent.Polarity;
		base.PulsedQDissociation = scanEvent.PulsedQDissociation;
		base.PulsedQDissociationType = scanEvent.PulsedQDissociationType;
		base.Reactions = scanEvent.Reactions;
		base.ScanDataType = scanEvent.ScanDataType;
		base.ScanType = scanEvent.ScanType;
		base.ScanTypeIndex = scanEvent.ScanTypeIndex;
		base.SectorScan = scanEvent.SectorScan;
		base.SourceFragmentationMassRanges = scanEvent.SourceFragmentationMassRanges;
		base.SourceFragmentations = scanEvent.SourceFragmentations;
	}

	/// <summary>
	/// Initialize the parser (before reading the filter string)
	/// </summary>
	private void InitializeParser()
	{
		_simSourceCidArray = new double[100];
		_simSourceCidValidArray = new uint[100];
		_massRangeArray = new double[100, 2];
		_massArray = new double[100];
		_precEnergyArray = new double[100];
		_precEnergyValidArray = new PrecursorActivation[100];
		_minMass = 0.5;
		_maxMass = 999999.0;
		_massRangeCount = -1;
		for (int i = 0; i < 100; i++)
		{
			_simSourceCidValidArray[i] = 3u;
			_massRangeArray[i, 1] = -1.0;
		}
		for (int j = 0; j < 100; j++)
		{
			_massArray[j] = -1.0;
			_precEnergyValidArray[j] = new PrecursorActivation();
		}
		_precursorActivations = new List<PrecursorActivation>(10);
		_ionizationModeSet = false;
		_massAnalyzerSet = false;
		_sectorScanSet = false;
		_lockSet = false;
		_freeRegionSet = false;
		_ultraSet = false;
		_enhancedSet = false;
		_paramASet = false;
		_paramBSet = false;
		_paramFSet = false;
		_spsMultiNotchSet = false;
		_paramRSet = false;
		_paramVSet = false;
		_multiPhotonDissociationSet = false;
		_electronCaptureDissociationSet = false;
		_photoIonizationSet = false;
		_polaritySet = false;
		_scanDataTypeSet = false;
		_coronaSet = false;
		_sourceFragmentationSet = false;
		_compensationVoltageSet = false;
		_dataDependentSet = false;
		_wideBandSet = false;
		_supplementalActivationSet = false;
		_multiStateActivationSet = false;
		_accurateMassSet = false;
		_turboScanSet = false;
		_scanModeSet = false;
		_multiplexSet = false;
		_detectorSet = false;
		_detectorValueSet = false;
		MetaFilters = MetaFilterType.None;
		_filterSourceLowValState = ScanFilterEnums.FilterSourceLowVal.AcceptAnySourceCIDLow;
		_filterSourceHighValState = ScanFilterEnums.FilterSourceHighVal.AcceptAnySourceCIDHigh;
		_sourceCidLowVal = 0.0;
		_sourceCidHighVal = 0.0;
		_compensationVoltageLowValState = ScanFilterEnums.FilterSourceLowVal.AcceptAnySourceCIDLow;
		_compensationVoltageHighValState = ScanFilterEnums.FilterSourceHighVal.AcceptAnySourceCIDHigh;
		_compensationVoltageLowVal = 0.0;
		_compensationVoltageHighVal = 0.0;
		_segmentScanEvent = ScanFilterEnums.SegmentScanEventType.AcceptAnySegmentScanEvent;
	}

	/// <summary>
	/// Calculate the total number of activations.
	/// </summary>
	/// <returns>
	/// The total.
	/// </returns>
	private int GetTotalNumberActivations()
	{
		int num = _precMassesCount + _precursorActivations.Count;
		if (num < 0)
		{
			num = 0;
		}
		return num;
	}

	/// <summary>
	/// Gets the SIM compensation voltage, at a given index
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <param name="energy">
	/// The energy.
	/// </param>
	/// <returns>
	/// The energy type
	/// </returns>
	private ScanFilterEnums.SIMCompensationVoltageEnergy GetSimCompensationVoltage(int index, out double energy)
	{
		energy = _simSourceCidArray[index];
		return (ScanFilterEnums.SIMCompensationVoltageEnergy)((_simSourceCidValidArray[index] & 2) >> 1);
	}

	/// <summary>
	/// Get SIM source CID at a given index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <param name="energy">
	/// The energy.
	/// </param>
	/// <returns>
	/// The energy type
	/// </returns>
	private ScanFilterEnums.SIMSourceCIDEnergy GetSimSourceCid(int index, out double energy)
	{
		energy = _simSourceCidArray[index];
		return (ScanFilterEnums.SIMSourceCIDEnergy)(_simSourceCidValidArray[index] & 1);
	}

	/// <summary>
	/// Parse a filter string
	/// </summary>
	/// <param name="filterString">
	/// The filter string.
	/// </param>
	/// <returns>
	/// The state (Bad on failure to parse)
	/// </returns>
	private ScanFilterEnums.FilterStringParseState ParseFilter(string filterString)
	{
		int num = 0;
		string[] array = Array.Empty<string>();
		if (string.IsNullOrWhiteSpace(filterString))
		{
			return ScanFilterEnums.FilterStringParseState.Good;
		}
		char[] array2 = filterString.ToCharArray();
		ScanFilterEnums.FilterStringParseState filterStringParseState = ParseSegmentScanEvent(array2);
		if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
		{
			filterStringParseState = ParseMassRanges(array2);
		}
		if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
		{
			array = new string(array2).Split(new char[2] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			bool flag = false;
			num = array.Length;
			int num2 = 0;
			while (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad && num2 < num)
			{
				if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad && !string.IsNullOrWhiteSpace(array[num2]))
				{
					string next = string.Empty;
					if (num2 < num - 1)
					{
						next = array[num2 + 1];
					}
					filterStringParseState = ParseMetaFilters(array[num2], next, out var count, out var dependent);
					if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Good)
					{
						flag = true;
					}
					if (flag && MetaFilters != MetaFilterType.None)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
					if (count >= 0)
					{
						int num3 = count << 8;
						MetaFilters |= (MetaFilterType)num3;
						if (dependent)
						{
							MetaFilters |= MetaFilterType.MsndMask;
						}
						num2++;
					}
				}
				num2++;
			}
			if (MetaFilters != MetaFilterType.None)
			{
				return ScanFilterEnums.FilterStringParseState.Good;
			}
			int num4 = 0;
			while (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad && MetaFilters == MetaFilterType.None && num4 < num)
			{
				string token = array[num4];
				if (!string.IsNullOrWhiteSpace(token))
				{
					filterStringParseState = ParseToken(filterStringParseState, ref token);
					if (!string.IsNullOrWhiteSpace(token))
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
					array[num4] = string.Empty;
				}
				num4++;
			}
		}
		if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
		{
			ScanFilterEnums.ScanTypes scanType = base.ScanType;
			if (scanType == ScanFilterEnums.ScanTypes.SIM || scanType == ScanFilterEnums.ScanTypes.Q1MS || scanType == ScanFilterEnums.ScanTypes.Q3MS)
			{
				if (!_msOrderSet)
				{
					_msOrderSet = true;
					SetMsOrderAndNumPrecMasses(1, 0);
				}
				else if (_msOrder >= 2)
				{
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
			}
		}
		if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
		{
			for (int i = 0; i < num; i++)
			{
				if (array[i].Length != 0)
				{
					return ScanFilterEnums.FilterStringParseState.Bad;
				}
			}
			UpdateMassRanges2();
		}
		if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
		{
			if (base.SourceFragmentation != ScanFilterEnums.OnOffTypes.On && base.SourceFragmentationType != ScanFilterEnums.VoltageTypes.Any && base.CompensationVoltage != ScanFilterEnums.OnOffTypes.On && base.CompensationVoltageType != ScanFilterEnums.VoltageTypes.Any)
			{
				filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
			}
			if (base.SourceFragmentation != ScanFilterEnums.OnOffTypes.Off && base.SourceFragmentationType == ScanFilterEnums.VoltageTypes.SIM)
			{
				filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
			}
		}
		if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad && _hasSimModeCid)
		{
			for (int j = 0; j < 100; j++)
			{
				if (_simSourceCidValidArray[j] == 0)
				{
					if (base.SourceFragmentation == ScanFilterEnums.OnOffTypes.On)
					{
						_simSourceCidValidArray[j] = 2u;
					}
					else if (base.CompensationVoltage == ScanFilterEnums.OnOffTypes.On)
					{
						_simSourceCidValidArray[j] = 1u;
					}
					else if ((base.SourceFragmentation == ScanFilterEnums.OnOffTypes.Off || base.SourceFragmentation == ScanFilterEnums.OnOffTypes.Any) && base.SourceFragmentationType == ScanFilterEnums.VoltageTypes.SIM)
					{
						_simSourceCidValidArray[j] = 2u;
					}
					else
					{
						_simSourceCidValidArray[j] = 3u;
					}
				}
			}
		}
		if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
		{
			bool flag2 = false;
			if (_hasSimModeCid)
			{
				for (int k = 0; k < 100; k++)
				{
					if ((_simSourceCidValidArray[k] & 1) == 0)
					{
						flag2 = true;
						break;
					}
				}
			}
			switch (base.SourceFragmentationType)
			{
			case ScanFilterEnums.VoltageTypes.SingleValue:
				if (_filterSourceLowValState != ScanFilterEnums.FilterSourceLowVal.SourceCIDLow || _filterSourceHighValState == ScanFilterEnums.FilterSourceHighVal.SourceCIDHigh || flag2)
				{
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				break;
			case ScanFilterEnums.VoltageTypes.Ramp:
				if (_filterSourceLowValState != ScanFilterEnums.FilterSourceLowVal.SourceCIDLow || _filterSourceHighValState != ScanFilterEnums.FilterSourceHighVal.SourceCIDHigh || flag2)
				{
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				break;
			case ScanFilterEnums.VoltageTypes.NoValue:
			case ScanFilterEnums.VoltageTypes.Any:
				if (_filterSourceLowValState == ScanFilterEnums.FilterSourceLowVal.SourceCIDLow || _filterSourceHighValState == ScanFilterEnums.FilterSourceHighVal.SourceCIDHigh || flag2)
				{
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				break;
			case ScanFilterEnums.VoltageTypes.SIM:
				if (_filterSourceLowValState == ScanFilterEnums.FilterSourceLowVal.SourceCIDLow || _filterSourceHighValState == ScanFilterEnums.FilterSourceHighVal.SourceCIDHigh)
				{
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				break;
			}
			bool flag3 = false;
			if (_hasSimModeCid)
			{
				for (int l = 0; l < 100; l++)
				{
					if ((_simSourceCidValidArray[l] & 2) >> 1 == 0)
					{
						flag3 = true;
						break;
					}
				}
			}
			switch (base.CompensationVoltageType)
			{
			case ScanFilterEnums.VoltageTypes.SingleValue:
				if (_compensationVoltageLowValState != ScanFilterEnums.FilterSourceLowVal.SourceCIDLow || _compensationVoltageHighValState == ScanFilterEnums.FilterSourceHighVal.SourceCIDHigh || flag3)
				{
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				break;
			case ScanFilterEnums.VoltageTypes.Ramp:
				if (_compensationVoltageLowValState != ScanFilterEnums.FilterSourceLowVal.SourceCIDLow || _compensationVoltageHighValState != ScanFilterEnums.FilterSourceHighVal.SourceCIDHigh || flag3)
				{
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				break;
			case ScanFilterEnums.VoltageTypes.NoValue:
			case ScanFilterEnums.VoltageTypes.Any:
				if (_compensationVoltageLowValState == ScanFilterEnums.FilterSourceLowVal.SourceCIDLow || _compensationVoltageHighValState == ScanFilterEnums.FilterSourceHighVal.SourceCIDHigh || flag3)
				{
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				break;
			case ScanFilterEnums.VoltageTypes.SIM:
				if (_compensationVoltageLowValState == ScanFilterEnums.FilterSourceLowVal.SourceCIDLow || _compensationVoltageHighValState == ScanFilterEnums.FilterSourceHighVal.SourceCIDHigh)
				{
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				break;
			}
		}
		return filterStringParseState;
	}

	/// <summary>
	/// Parse the next (white space separated) token.
	/// </summary>
	/// <param name="parseState">
	/// The parse state.
	/// </param>
	/// <param name="token">
	/// The token.
	/// </param>
	/// <returns>
	/// The updated parse state.
	/// </returns>
	private ScanFilterEnums.FilterStringParseState ParseToken(ScanFilterEnums.FilterStringParseState parseState, ref string token)
	{
		Dictionary<string, TokenWithCategory> tokens = FilterParserDictionary.Tokens;
		string text = token.ToLowerInvariant();
		int num = -1;
		int length;
		if ((length = text.IndexOf('@')) > 0)
		{
			text = text.Substring(0, length);
		}
		else if ((num = text.IndexOf('=')) > 0)
		{
			text = text.Substring(0, num);
		}
		if (tokens.TryGetValue(text, out var value))
		{
			TokenCategory category = value.Category;
			int enumValue = value.EnumValue;
			int stringIndex = value.StringIndex;
			switch (category)
			{
			case TokenCategory.IonizationMode:
				base.IonizationMode = (ScanFilterEnums.IonizationModes)enumValue;
				Validate(ref _ionizationModeSet);
				break;
			case TokenCategory.MassAnalyzer:
				base.MassAnalyzerType = (ScanFilterEnums.MassAnalyzerTypes)enumValue;
				Validate(ref _massAnalyzerSet);
				break;
			case TokenCategory.SectorScan:
				base.SectorScan = (ScanFilterEnums.SectorScans)enumValue;
				Validate(ref _sectorScanSet);
				break;
			case TokenCategory.Lock:
				base.Lock = (ScanFilterEnums.OnOffTypes)enumValue;
				Validate(ref _lockSet);
				break;
			case TokenCategory.FreeRegion:
				base.FreeRegion = (ScanFilterEnums.FreeRegions)enumValue;
				Validate(ref _freeRegionSet);
				break;
			case TokenCategory.SpsMultiNotch:
				base.SpsMultiNotch = (ScanFilterEnums.OffOnTypes)enumValue;
				Validate(ref _spsMultiNotchSet);
				break;
			case TokenCategory.PhotoIonization:
				base.PhotoIonization = (ScanFilterEnums.OnOffTypes)enumValue;
				Validate(ref _photoIonizationSet);
				break;
			case TokenCategory.Polarity:
				base.Polarity = (ScanFilterEnums.PolarityTypes)enumValue;
				Validate(ref _polaritySet);
				break;
			case TokenCategory.ScanDataType:
				base.ScanDataType = (ScanFilterEnums.ScanDataTypes)enumValue;
				Validate(ref _scanDataTypeSet);
				break;
			case TokenCategory.Corona:
				base.Corona = (ScanFilterEnums.OnOffTypes)enumValue;
				Validate(ref _coronaSet);
				break;
			case TokenCategory.DataDependent:
				base.DependentDataFlag = (ScanFilterEnums.IsDependent)enumValue;
				Validate(ref _dataDependentSet);
				break;
			case TokenCategory.SupplementalActivation:
				base.SupplementalActivation = (ScanFilterEnums.OffOnTypes)enumValue;
				Validate(ref _supplementalActivationSet);
				break;
			case TokenCategory.MultiStateActivation:
				base.MultiStateActivation = (ScanFilterEnums.OffOnTypes)enumValue;
				Validate(ref _multiStateActivationSet);
				break;
			case TokenCategory.AccurateMass:
				base.AccurateMassType = (ScanFilterEnums.AccurateMassTypes)enumValue;
				Validate(ref _accurateMassSet);
				break;
			case TokenCategory.ScanMode:
				if (Validate(ref _scanModeSet))
				{
					base.ScanType = (ScanFilterEnums.ScanTypes)enumValue;
					if (base.ScanType == ScanFilterEnums.ScanTypes.Q1MS || base.ScanType == ScanFilterEnums.ScanTypes.Q3MS)
					{
						parseState = ScanFilterEnums.FilterStringParseState.Good;
						SetMsOrderAndNumPrecMasses(1, 0);
					}
				}
				break;
			case TokenCategory.Multiplex:
				if (Validate(ref _multiplexSet))
				{
					base.Multiplex = (ScanFilterEnums.OffOnTypes)enumValue;
					if (_msOrderSet && (_msOrder < 0 || _msOrder > 2))
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
					parseState = ScanFilterEnums.FilterStringParseState.Good;
					_precMassesCount = 100;
				}
				break;
			case TokenCategory.MsOrder:
				if (Validate(ref _msOrderSet))
				{
					base.MsOrder = (ScanFilterEnums.MSOrderTypes)enumValue;
					parseState = ScanFilterEnums.FilterStringParseState.Good;
					SetMsOrderAndNumPrecMasses(FilterStructValues[stringIndex], PrecursorValues[stringIndex]);
					token = string.Empty;
					if (_multiplexSet && (_msOrder < 0 || _msOrder > 2))
					{
						parseState = ScanFilterEnums.FilterStringParseState.Bad;
						return parseState;
					}
				}
				break;
			case TokenCategory.Detector:
			{
				if (!Validate(ref _detectorSet))
				{
					break;
				}
				base.Detector = (ScanFilterEnums.DetectorType)enumValue;
				if (num <= 0)
				{
					break;
				}
				int startPos = num + 1;
				if (value.EnumValue == 0 && Validate(ref _detectorValueSet) && token.Length > startPos)
				{
					parseState = SimpleSignedDouble(token.ToCharArray(), ref startPos, out var value2, whiteSpaceOk: false, EnergyPrecision);
					if (parseState != ScanFilterEnums.FilterStringParseState.Bad && value2 > 0.0)
					{
						base.DetectorValue = value2;
						parseState = ScanFilterEnums.FilterStringParseState.Good;
					}
				}
				else
				{
					parseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				break;
			}
			case TokenCategory.MultiPhotonDissociation:
				parseState = ParseDissociation(ref token, ref _multiPhotonDissociationSet, FilterStringTokens.MultiPhotonDissociationTokenNames, delegate(int i, double dissociation)
				{
					base.MultiPhotonDissociation = dissociation;
					base.MultiPhotonDissociationType = FilterStringTokens.MultiPhotonDissociationTokenValues[i];
				});
				break;
			case TokenCategory.ElectronCaptureDissociation:
				parseState = ParseDissociation(ref token, ref _electronCaptureDissociationSet, FilterStringTokens.ElectronCaptureDissociationTokenNames, delegate(int i, double dissociation)
				{
					base.ElectronCaptureDissociation = dissociation;
					base.ElectronCaptureDissociationType = FilterStringTokens.ElectronCaptureDissociationTokenValues[i];
				});
				break;
			case TokenCategory.SourceFragmentation:
				parseState = ParseSourceFragmentation(ref token, ref _sourceFragmentationSet, FilterStringTokens.SourceFragmentationTokenNames);
				break;
			case TokenCategory.CompensationVoltage:
				parseState = ParseCompensationVoltage(ref token, ref _compensationVoltageSet, FilterStringTokens.CompensationVoltageTokenNames);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			token = string.Empty;
			return parseState;
		}
		if (parseState != ScanFilterEnums.FilterStringParseState.Bad && !string.IsNullOrWhiteSpace(token) && ParseSingleCharacterToken(token))
		{
			token = string.Empty;
			return parseState;
		}
		if (parseState != ScanFilterEnums.FilterStringParseState.Bad && !string.IsNullOrWhiteSpace(token))
		{
			parseState = ParsePrecursorMass(ref token);
		}
		return parseState;
		bool ParseSingleCharacterToken(string text2)
		{
			FlagNames flagNames = FlagNames.None;
			bool flag = false;
			if (text2.Length == 2)
			{
				if (text2[0] == '!')
				{
					flag = true;
					char c = text2[1];
					if (c >= 'a' && c <= 'z')
					{
						int num2 = c - 97;
						if (_lowerSet[num2])
						{
							parseState = ScanFilterEnums.FilterStringParseState.Bad;
							return true;
						}
						_lowerSet[num2] = true;
						flagNames = _lowerCaseMapping[c - 97];
					}
					else if (c >= 'A' && c <= 'Z')
					{
						int num3 = c - 65;
						if (_upperSet[num3])
						{
							parseState = ScanFilterEnums.FilterStringParseState.Bad;
							return true;
						}
						_upperSet[num3] = true;
						flagNames = _upperCaseMapping[num3];
					}
				}
			}
			else
			{
				if (text2.Length != 1)
				{
					return false;
				}
				char c2 = text2[0];
				if (c2 >= 'a' && c2 <= 'z')
				{
					int num4 = c2 - 97;
					if (_lowerSet[num4])
					{
						parseState = ScanFilterEnums.FilterStringParseState.Bad;
						return true;
					}
					_lowerSet[num4] = true;
					flagNames = _lowerCaseMapping[num4];
				}
				else if (c2 >= 'A' && c2 <= 'Z')
				{
					int num5 = c2 - 65;
					if (_upperSet[num5])
					{
						parseState = ScanFilterEnums.FilterStringParseState.Bad;
						return true;
					}
					flagNames = _upperCaseMapping[c2 - 65];
				}
			}
			TriState triState = (flag ? TriState.Off : TriState.On);
			switch (flagNames)
			{
			case FlagNames.None:
				return false;
			case FlagNames.ParamA:
				if (Validate(ref _paramASet))
				{
					base.ParamA = ((!flag) ? ScanFilterEnums.OffOnTypes.On : ScanFilterEnums.OffOnTypes.Off);
				}
				break;
			case FlagNames.ParamB:
				if (Validate(ref _paramBSet))
				{
					base.ParamB = ((!flag) ? ScanFilterEnums.OffOnTypes.On : ScanFilterEnums.OffOnTypes.Off);
				}
				break;
			case FlagNames.LowerE:
				base.LowerE = triState;
				break;
			case FlagNames.ParamF:
				if (Validate(ref _paramFSet))
				{
					base.ParamF = ((!flag) ? ScanFilterEnums.OffOnTypes.On : ScanFilterEnums.OffOnTypes.Off);
				}
				break;
			case FlagNames.LowerG:
				base.LowerG = triState;
				break;
			case FlagNames.LowerH:
				base.LowerH = triState;
				break;
			case FlagNames.LowerI:
				base.LowerI = triState;
				break;
			case FlagNames.LowerJ:
				base.LowerJ = triState;
				break;
			case FlagNames.LowerK:
				base.LowerK = triState;
				break;
			case FlagNames.LowerL:
				base.LowerL = triState;
				break;
			case FlagNames.LowerM:
				base.LowerM = triState;
				break;
			case FlagNames.LowerN:
				base.LowerN = triState;
				break;
			case FlagNames.LowerO:
				base.LowerO = triState;
				break;
			case FlagNames.LowerQ:
				base.LowerQ = triState;
				break;
			case FlagNames.ParamR:
				if (Validate(ref _paramRSet))
				{
					base.ParamR = ((!flag) ? ScanFilterEnums.OffOnTypes.On : ScanFilterEnums.OffOnTypes.Off);
				}
				break;
			case FlagNames.LowerS:
				base.LowerS = triState;
				break;
			case FlagNames.TurboScan:
				if (Validate(ref _turboScanSet))
				{
					base.TurboScan = (flag ? ScanFilterEnums.OnOffTypes.Off : ScanFilterEnums.OnOffTypes.On);
				}
				break;
			case FlagNames.Ultra:
				if (Validate(ref _ultraSet))
				{
					base.Ultra = (flag ? ScanFilterEnums.OnOffTypes.Off : ScanFilterEnums.OnOffTypes.On);
				}
				break;
			case FlagNames.ParamV:
				if (Validate(ref _paramVSet))
				{
					base.ParamV = ((!flag) ? ScanFilterEnums.OffOnTypes.On : ScanFilterEnums.OffOnTypes.Off);
				}
				break;
			case FlagNames.Wideband:
				if (Validate(ref _wideBandSet))
				{
					base.Wideband = ((!flag) ? ScanFilterEnums.OffOnTypes.On : ScanFilterEnums.OffOnTypes.Off);
				}
				break;
			case FlagNames.LowerX:
				base.LowerX = triState;
				break;
			case FlagNames.LowerY:
				base.LowerY = triState;
				break;
			case FlagNames.UpperA:
				base.UpperA = triState;
				break;
			case FlagNames.UpperB:
				base.UpperB = triState;
				break;
			case FlagNames.Enhanced:
				if (Validate(ref _enhancedSet))
				{
					base.Enhanced = (flag ? ScanFilterEnums.OnOffTypes.Off : ScanFilterEnums.OnOffTypes.On);
				}
				break;
			case FlagNames.UpperF:
				base.UpperF = triState;
				break;
			case FlagNames.UpperG:
				base.UpperG = triState;
				break;
			case FlagNames.UpperH:
				base.UpperH = triState;
				break;
			case FlagNames.UpperI:
				base.UpperI = triState;
				break;
			case FlagNames.UpperJ:
				base.UpperJ = triState;
				break;
			case FlagNames.UpperK:
				base.UpperK = triState;
				break;
			case FlagNames.UpperL:
				base.UpperL = triState;
				break;
			case FlagNames.UpperM:
				base.UpperM = triState;
				break;
			case FlagNames.UpperN:
				base.UpperN = triState;
				break;
			case FlagNames.UpperO:
				base.UpperO = triState;
				break;
			case FlagNames.UpperQ:
				base.UpperQ = triState;
				break;
			case FlagNames.UpperR:
				base.UpperR = triState;
				break;
			case FlagNames.UpperS:
				base.UpperS = triState;
				break;
			case FlagNames.UpperT:
				base.UpperT = triState;
				break;
			case FlagNames.UpperU:
				base.UpperU = triState;
				break;
			case FlagNames.UpperV:
				base.UpperV = triState;
				break;
			case FlagNames.UpperW:
				base.UpperW = triState;
				break;
			case FlagNames.UpperX:
				base.UpperX = triState;
				break;
			case FlagNames.UpperY:
				base.UpperY = triState;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return true;
		}
		bool Validate(ref bool flag)
		{
			if (flag)
			{
				parseState = ScanFilterEnums.FilterStringParseState.Bad;
				return false;
			}
			return flag = true;
		}
	}

	/// <summary>
	/// Parse for segment and scan event.
	/// </summary>
	/// <param name="filterStringArr">
	/// The filter string characters.
	/// </param>
	/// <returns>
	/// The parse state
	/// </returns>
	private ScanFilterEnums.FilterStringParseState ParseSegmentScanEvent(char[] filterStringArr)
	{
		int openParenthesisIndex = -1;
		int closeParenthesisIndex = -1;
		int num = filterStringArr.Length;
		int openMatch = 0;
		int closeMatch = 0;
		SearchForOpenAndCloseParenthesis(filterStringArr, ref openParenthesisIndex, ref closeParenthesisIndex, ref openMatch, ref closeMatch, num, '{', '}');
		if (openMatch == 0 && closeMatch == 0)
		{
			return ScanFilterEnums.FilterStringParseState.Incomplete;
		}
		if (openMatch > 1 || closeMatch > 1)
		{
			return ScanFilterEnums.FilterStringParseState.Bad;
		}
		if (openParenthesisIndex >= 0 && closeParenthesisIndex >= 0 && closeParenthesisIndex <= openParenthesisIndex)
		{
			return ScanFilterEnums.FilterStringParseState.Bad;
		}
		if (openParenthesisIndex >= 0)
		{
			if (closeParenthesisIndex < 0)
			{
				return ScanFilterEnums.FilterStringParseState.Bad;
			}
			int num2 = closeParenthesisIndex - openParenthesisIndex + 1;
			if (openParenthesisIndex < num - 1 && closeParenthesisIndex >= 0)
			{
				bool flag = false;
				bool flag2 = true;
				int num3 = -1;
				for (int i = openParenthesisIndex + 1; i < closeParenthesisIndex; i++)
				{
					if (!char.IsWhiteSpace(filterStringArr[i]) && !char.IsDigit(filterStringArr[i]))
					{
						if (filterStringArr[i] != CommaSep)
						{
							flag2 = false;
							break;
						}
						if (flag)
						{
							flag2 = false;
							break;
						}
						num3 = i;
						flag = true;
					}
				}
				if (!flag2 || !flag)
				{
					return ScanFilterEnums.FilterStringParseState.Bad;
				}
				int num4 = openParenthesisIndex;
				for (int j = openParenthesisIndex + 1; j < closeParenthesisIndex; j++)
				{
					if (char.IsWhiteSpace(filterStringArr[j]))
					{
						num4 = j;
						continue;
					}
					num4 = ((!char.IsDigit(filterStringArr[j])) ? openParenthesisIndex : j);
					break;
				}
				if (num4 == openParenthesisIndex || num4 == closeParenthesisIndex - 1)
				{
					return ScanFilterEnums.FilterStringParseState.Bad;
				}
				int num5 = DecodeSimpleInt(filterStringArr, num4);
				if (num5 < 0)
				{
					return ScanFilterEnums.FilterStringParseState.Bad;
				}
				int num6 = num3;
				for (int k = num3 + 1; k <= closeParenthesisIndex; k++)
				{
					if (char.IsWhiteSpace(filterStringArr[k]))
					{
						num6 = k;
						continue;
					}
					num6 = ((!char.IsDigit(filterStringArr[k])) ? num3 : k);
					break;
				}
				if (num6 == num3 || num6 == closeParenthesisIndex)
				{
					return ScanFilterEnums.FilterStringParseState.Bad;
				}
				int num7 = DecodeSimpleInt(filterStringArr, num6);
				if (num7 < 0)
				{
					return ScanFilterEnums.FilterStringParseState.Bad;
				}
				_segmentScanEvent = ScanFilterEnums.SegmentScanEventType.SegmentScanEventSet;
				_scanSegmentNum = num5;
				_scanScanEventNum = num7;
			}
			int num8 = openParenthesisIndex + num2;
			for (int l = openParenthesisIndex; l < num8; l++)
			{
				filterStringArr[l] = ' ';
			}
		}
		return ScanFilterEnums.FilterStringParseState.Incomplete;
	}

	/// <summary>
	/// Decode an integer. Must be less than 8 digits
	/// </summary>
	/// <param name="token">the text to parse</param>
	/// <param name="index">index into token</param>
	/// <returns>positive value or zero, if no digits. -1 on &gt;=8 digits</returns>
	private int DecodeSimpleInt(char[] token, int index)
	{
		int num = 0;
		int num2 = 0;
		while (index < token.Length)
		{
			char c = token[index++];
			if (c < '0' || c > '9')
			{
				break;
			}
			num = num * 10 + (c - 48);
			if (++num2 >= 8)
			{
				return -1;
			}
		}
		return num;
	}

	/// <summary>
	/// Parse a mass ranges.
	/// </summary>
	/// <param name="token">
	/// The token to parse
	/// </param>
	/// <returns>
	/// The parse state.
	/// </returns>
	private ScanFilterEnums.FilterStringParseState ParseMassRanges(char[] token)
	{
		int openParenthesisIndex = -1;
		int closeParenthesisIndex = -1;
		int openMatch = 0;
		int closeMatch = 0;
		int num = token.Length;
		ScanFilterEnums.FilterStringParseState result = ScanFilterEnums.FilterStringParseState.Incomplete;
		SearchForOpenAndCloseParenthesis(token, ref openParenthesisIndex, ref closeParenthesisIndex, ref openMatch, ref closeMatch, num, '[', ']');
		if (openMatch == 0 && closeMatch == 0)
		{
			return ScanFilterEnums.FilterStringParseState.Incomplete;
		}
		if (openMatch > 1 || closeMatch > 1)
		{
			return ScanFilterEnums.FilterStringParseState.Bad;
		}
		if (openParenthesisIndex >= 0)
		{
			if (closeParenthesisIndex < 0)
			{
				return ScanFilterEnums.FilterStringParseState.Bad;
			}
			int num2 = closeParenthesisIndex - openParenthesisIndex + 1;
			if (openParenthesisIndex < num - 1)
			{
				int num3 = num2 - 1;
				char[] array = new char[num3];
				Array.Copy(token, openParenthesisIndex + 1, array, 0, num3);
				result = ParseMassRangesPlusEnergy(array);
			}
			int num4 = openParenthesisIndex + num2;
			for (int i = openParenthesisIndex; i < num4; i++)
			{
				token[i] = ' ';
			}
		}
		return result;
	}

	/// <summary>
	/// Parses the mass ranges, plus energy
	/// parse mass ranges -- double or double-double separated by ','
	/// max of 50 ranges
	/// <c>'['  nnn.n [- nnn.n] [, [nnn.n [- nnn.n] ....] ']'</c>
	/// </summary>
	/// <param name="massRangesArr">The mass ranges array.</param>
	/// <returns>parse state</returns>
	private ScanFilterEnums.FilterStringParseState ParseMassRangesPlusEnergy(char[] massRangesArr)
	{
		bool flag = true;
		int num = 0;
		ParseMassRangesState parseMassRangesState = ParseMassRangesState.LoMass;
		ScanFilterEnums.FilterStringParseState filterStringParseState = ScanFilterEnums.FilterStringParseState.Incomplete;
		int num2 = massRangesArr.Length;
		int startPos = 0;
		while (flag && startPos < num2)
		{
			char c = massRangesArr[startPos++];
			double value2;
			switch (parseMassRangesState)
			{
			case ParseMassRangesState.LoMass:
				if (char.IsWhiteSpace(c))
				{
					break;
				}
				if (num >= 100)
				{
					flag = false;
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				else if (char.IsDigit(c))
				{
					startPos--;
					filterStringParseState = SimpleDouble(massRangesArr, ref startPos, out value2, _massPrecision);
					if (filterStringParseState == ScanFilterEnums.FilterStringParseState.Bad)
					{
						flag = false;
						break;
					}
					if (!MassRangeInLimits(num, 0, value2))
					{
						flag = false;
						filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
					}
					if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
					{
						num++;
						filterStringParseState = ScanFilterEnums.FilterStringParseState.Incomplete;
						parseMassRangesState = ParseMassRangesState.CommaDash;
					}
				}
				else if (c == ']')
				{
					flag = false;
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				else
				{
					flag = false;
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				break;
			case ParseMassRangesState.CommaDash:
				if (char.IsWhiteSpace(c))
				{
					break;
				}
				if (c == CommaSep)
				{
					parseMassRangesState = ParseMassRangesState.LoMass;
					break;
				}
				switch (c)
				{
				case '-':
					parseMassRangesState = ParseMassRangesState.HiMass;
					break;
				case '@':
					parseMassRangesState = ParseMassRangesState.SidEnergy;
					break;
				case ']':
					parseMassRangesState = ParseMassRangesState.RBracket;
					break;
				default:
					flag = false;
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
					break;
				}
				break;
			case ParseMassRangesState.CommaBracket:
				if (char.IsWhiteSpace(c))
				{
					break;
				}
				if (c == CommaSep)
				{
					parseMassRangesState = ParseMassRangesState.LoMass;
					break;
				}
				switch (c)
				{
				case '@':
					parseMassRangesState = ParseMassRangesState.SidEnergy;
					break;
				case ']':
					parseMassRangesState = ParseMassRangesState.RBracket;
					break;
				default:
					flag = false;
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
					break;
				}
				break;
			case ParseMassRangesState.HiMass:
				if (char.IsWhiteSpace(c))
				{
					break;
				}
				if (char.IsDigit(c))
				{
					startPos--;
					filterStringParseState = SimpleDouble(massRangesArr, ref startPos, out value2, _massPrecision);
					if (filterStringParseState == ScanFilterEnums.FilterStringParseState.Bad)
					{
						flag = false;
					}
					else if (!MassRangeInLimits(num - 1, 1, value2))
					{
						flag = false;
						filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
					}
					if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
					{
						filterStringParseState = ScanFilterEnums.FilterStringParseState.Incomplete;
						parseMassRangesState = ParseMassRangesState.CommaBracket;
					}
				}
				else
				{
					flag = false;
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				break;
			case ParseMassRangesState.SidEnergy:
				if (char.IsWhiteSpace(c))
				{
					break;
				}
				if (c == '-' || char.IsDigit(c))
				{
					startPos--;
					filterStringParseState = SimpleSignedDouble(massRangesArr, ref startPos, out var value);
					if (filterStringParseState == ScanFilterEnums.FilterStringParseState.Bad)
					{
						flag = false;
					}
					if (flag)
					{
						SetSimSourceCidn(num - 1, value);
					}
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Incomplete;
					parseMassRangesState = ParseMassRangesState.CommaBracket;
				}
				else
				{
					flag = false;
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				}
				break;
			case ParseMassRangesState.RBracket:
				flag = false;
				filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
				break;
			}
		}
		if (parseMassRangesState == ParseMassRangesState.RBracket && filterStringParseState == ScanFilterEnums.FilterStringParseState.Incomplete)
		{
			filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
		}
		return filterStringParseState;
	}

	/// <summary>
	/// parse a "simple" double value: no e-notation
	/// "123.4"  or "123".
	/// Optionally permit leading sign
	/// </summary>
	/// <param name="filterString">The mass ranges array.</param>
	/// <param name="startPos">The start position.</param>
	/// <param name="value">The value.</param>
	/// <param name="precision">Mass precision</param>
	/// <param name="permitSign">True if value may have + or -</param>
	/// <returns>Parse state: Bad if too many digits, or Next otherwise</returns>
	private ScanFilterEnums.FilterStringParseState SimpleDouble(char[] filterString, ref int startPos, out double value, int precision, bool permitSign = false)
	{
		int num = 0;
		int num2 = 0;
		ParseSimpleDoubleState parseSimpleDoubleState = ParseSimpleDoubleState.DigitsBeforeDecimal;
		ScanFilterEnums.FilterStringParseState result = ScanFilterEnums.FilterStringParseState.Good;
		bool flag = true;
		int num3 = filterString.Length;
		StringBuilder stringBuilder = new StringBuilder();
		value = 0.0;
		if (permitSign && startPos < num3)
		{
			char c = filterString[startPos];
			if (c == '+' || c == '-')
			{
				startPos++;
				stringBuilder.Append(c);
			}
		}
		while (flag && startPos < num3)
		{
			char c2 = filterString[startPos];
			switch (parseSimpleDoubleState)
			{
			case ParseSimpleDoubleState.DigitsBeforeDecimal:
				if (char.IsDigit(c2))
				{
					if (++num > _doubleLength)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
				}
				else if (c2 == DecimalPt)
				{
					parseSimpleDoubleState = ParseSimpleDoubleState.Decimal;
				}
				else
				{
					flag = false;
					result = ScanFilterEnums.FilterStringParseState.Next;
				}
				break;
			case ParseSimpleDoubleState.Decimal:
				if (char.IsDigit(c2))
				{
					parseSimpleDoubleState = ParseSimpleDoubleState.DigitsAfterDecimal;
					if (++num > _doubleLength)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
					if (++num2 > precision)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
				}
				else
				{
					flag = false;
					result = ScanFilterEnums.FilterStringParseState.Next;
				}
				break;
			case ParseSimpleDoubleState.DigitsAfterDecimal:
				if (char.IsWhiteSpace(c2))
				{
					flag = false;
					result = ScanFilterEnums.FilterStringParseState.Next;
				}
				else if (char.IsDigit(c2))
				{
					if (++num > _doubleLength)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
					if (++num2 > precision)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
				}
				else
				{
					flag = false;
					result = ScanFilterEnums.FilterStringParseState.Next;
				}
				break;
			}
			if (flag)
			{
				stringBuilder.Append(c2);
			}
			startPos++;
		}
		double.TryParse(stringBuilder.ToString(), NumberStyles.Any, FormatProvider, out value);
		startPos--;
		return result;
	}

	/// <summary>
	/// parse a double value, i.e., no e-notation
	/// Always permits sign. "123.4", "+123.4"  or "-123".
	/// Only permits trailing sign if it is specified as a "range separator".
	/// </summary>
	/// <param name="filterString">Text to parse</param>
	/// <param name="startPos">Index of first char to parse</param>
	/// <param name="value">Value decoded</param>
	/// <param name="whiteSpaceOk">True if trailing space is permitted</param>
	/// <param name="precision">Permitted precision</param>
	/// <param name="rangeSeparator">Character which may appear between two numbers representing a range</param>
	/// <returns>Parse state, which can be bad on invalid, Next otherwise</returns>
	private ScanFilterEnums.FilterStringParseState SimpleSignedDouble(char[] filterString, ref int startPos, out double value, bool whiteSpaceOk = true, int precision = 2, char rangeSeparator = '#')
	{
		ScanFilterEnums.FilterStringParseState filterStringParseState = SimpleDouble(filterString, ref startPos, out value, precision, permitSign: true);
		if (filterStringParseState == ScanFilterEnums.FilterStringParseState.Next)
		{
			int num = filterString.Length;
			if (startPos < num)
			{
				char c = filterString[startPos];
				if ((c == '+' || c == '-') && c != rangeSeparator)
				{
					return ScanFilterEnums.FilterStringParseState.Bad;
				}
				if (c == ' ' && !whiteSpaceOk)
				{
					return ScanFilterEnums.FilterStringParseState.Bad;
				}
			}
		}
		return filterStringParseState;
	}

	/// <summary>
	/// parse a double value range, i.e., no e-notation
	/// "+123.4@200.34"  or "-123@-345"
	/// First value is a mass, second is energy.
	/// </summary>
	/// <param name="token">
	/// text to parse
	/// </param>
	/// <param name="startPos">
	/// next char to parse
	/// </param>
	/// <param name="separatorPos">
	/// set if '@; found
	/// </param>
	/// <param name="mass">
	/// Precursor mass
	/// </param>
	/// <param name="energy">
	/// Activation energy
	/// </param>
	/// <param name="doubleSignedValuesOk">
	/// set is '-' '+' permitted
	/// </param>
	/// <param name="prefixedByAtDoubleSignedValuesOk">
	/// Set if number after '@' permitted to be signed
	/// </param>
	/// <returns> The updated parse state
	/// </returns>
	private ScanFilterEnums.FilterStringParseState SimpleDoubleAtDouble(char[] token, ref int startPos, ref int separatorPos, out double mass, out double energy, bool doubleSignedValuesOk, bool prefixedByAtDoubleSignedValuesOk)
	{
		ScanFilterEnums.FilterStringParseState filterStringParseState = SimpleDouble(token, ref startPos, out mass, _massPrecision, doubleSignedValuesOk);
		energy = 0.0;
		if (filterStringParseState == ScanFilterEnums.FilterStringParseState.Bad)
		{
			return ScanFilterEnums.FilterStringParseState.Bad;
		}
		int num = token.Length;
		if (startPos < num && token[startPos] == '@')
		{
			separatorPos = startPos;
			startPos++;
			filterStringParseState = SimpleDouble(token, ref startPos, out energy, EnergyPrecision, prefixedByAtDoubleSignedValuesOk);
		}
		return filterStringParseState;
	}

	/// <summary>
	/// Test that the mass range in limits.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <param name="highOrLow">
	/// The high or low. (0 for low, 1 for high)
	/// </param>
	/// <param name="mass">
	/// The mass.
	/// </param>
	/// <returns>
	/// true when valid.
	/// </returns>
	private bool MassRangeInLimits(int index, int highOrLow, double mass)
	{
		if (mass >= _minMass && mass <= _maxMass)
		{
			switch (highOrLow)
			{
			case 0:
				_massRangeArray[index, 0] = mass;
				_massRangeArray[index, 1] = -1.0;
				break;
			case 1:
				if (mass >= _massRangeArray[index, 0])
				{
					_massRangeArray[index, 1] = mass;
					break;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Parse meta filters.
	/// </summary>
	/// <param name="token">
	/// The token.
	/// </param>
	/// <param name="next">next sting (modifer)</param>
	/// <param name="count">ms order count, after MSn</param>
	/// <param name="dependent">dependent flag for MSn</param>
	/// <returns>
	/// The parse state.
	/// </returns>
	private ScanFilterEnums.FilterStringParseState ParseMetaFilters(string token, string next, out int count, out bool dependent)
	{
		count = -1;
		dependent = false;
		string[] metaFilterTokenNames = FilterStringTokens.MetaFilterTokenNames;
		int num = metaFilterTokenNames.Length;
		token = token.ToLowerInvariant();
		for (int i = 1; i < num; i++)
		{
			if (metaFilterTokenNames[i] != token)
			{
				continue;
			}
			MetaFilterType metaFilterType = (MetaFilterType)FilterStringTokens.MetaFilterTokenValues[i];
			if ((MetaFilters & metaFilterType) != MetaFilterType.None)
			{
				return ScanFilterEnums.FilterStringParseState.Incomplete;
			}
			MetaFilters |= metaFilterType;
			if (metaFilterType == MetaFilterType.Msn)
			{
				int length = next.Length;
				if (length > 0 && length <= 3 && char.IsDigit(next[0]))
				{
					count = next[0] - 48;
					if (length > 1)
					{
						char c = next[1];
						if (char.IsDigit(c))
						{
							int num2 = c - 48;
							count = count * 10 + num2;
							if (count == 3)
							{
								if (next[2] != 'd' && next[2] != 'D')
								{
									return ScanFilterEnums.FilterStringParseState.Incomplete;
								}
								dependent = true;
							}
						}
						else if (c == 'd' || c == 'D')
						{
							dependent = true;
							if (length >= 3)
							{
								return ScanFilterEnums.FilterStringParseState.Incomplete;
							}
						}
					}
					if (count < 2 || count > 15)
					{
						return ScanFilterEnums.FilterStringParseState.Incomplete;
					}
				}
			}
			return ScanFilterEnums.FilterStringParseState.Good;
		}
		return ScanFilterEnums.FilterStringParseState.Incomplete;
	}

	/// <summary>
	/// set SIM source CID for a given index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <param name="value">
	/// The value.
	/// </param>
	private void SetSimSourceCidn(int index, double value)
	{
		_simSourceCidArray[index] = value;
		_simSourceCidValidArray[index] = 0u;
		_hasSimModeCid = true;
	}

	/// <summary>
	/// parse dissociation, such as <c>mpd@value</c>
	/// </summary>
	/// <param name="token">
	/// The token.
	/// </param>
	/// <param name="setFlag">
	/// The set flag.
	/// </param>
	/// <param name="names">
	/// The names.
	/// </param>
	/// <param name="setMode">
	/// The set mode.
	/// </param>
	/// <returns>
	/// The parse state
	/// </returns>
	private ScanFilterEnums.FilterStringParseState ParseDissociation(ref string token, ref bool setFlag, string[] names, Action<int, double> setMode)
	{
		if (setFlag)
		{
			return ScanFilterEnums.FilterStringParseState.Good;
		}
		ScanFilterEnums.FilterStringParseState result = ScanFilterEnums.FilterStringParseState.Incomplete;
		int num = names.Length;
		for (int i = 0; i < num; i++)
		{
			if (string.Compare(token, names[i], StringComparison.OrdinalIgnoreCase) == 0)
			{
				if (setFlag)
				{
					result = ScanFilterEnums.FilterStringParseState.Bad;
					break;
				}
				setFlag = true;
				if ((i == 0 && token.Length == 3) || i == 2)
				{
					result = ScanFilterEnums.FilterStringParseState.Good;
					setMode(i, 0.0);
					token = string.Empty;
					break;
				}
			}
			else
			{
				if (i != 0 || token.Length <= 4 || string.Compare(names[i], 0, token, 0, 3, ignoreCase: true) != 0)
				{
					continue;
				}
				if (setFlag)
				{
					result = ScanFilterEnums.FilterStringParseState.Bad;
					break;
				}
				setFlag = true;
				if (token[3] == '@')
				{
					int startPos = 3;
					if ((result = SimpleSignedDouble(token.ToCharArray(), ref startPos, out var _, whiteSpaceOk: false, EnergyPrecision)) == ScanFilterEnums.FilterStringParseState.Next && double.TryParse(token.Substring(4), NumberStyles.Any, FormatProvider, out var result2))
					{
						setMode(i, result2);
						result = ScanFilterEnums.FilterStringParseState.Good;
						token = string.Empty;
						break;
					}
				}
			}
		}
		return result;
	}

	/// <summary>
	/// Parse source fragmentation (SID)
	/// </summary>
	/// <param name="token">
	/// The token.
	/// </param>
	/// <param name="setFlag">
	/// in: True if this has been set already
	/// out: set to true when token is found
	/// </param>
	/// <param name="names">
	/// The names.
	/// </param>
	/// <returns>
	/// the parse state
	/// </returns>
	private ScanFilterEnums.FilterStringParseState ParseSourceFragmentation(ref string token, ref bool setFlag, string[] names)
	{
		ScanFilterEnums.FilterStringParseState filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
		int num = names.Length;
		for (int i = 0; i < num; i++)
		{
			string obj = names[i];
			int length = obj.Length;
			if (string.Compare(obj, 0, token, 0, length) != 0)
			{
				continue;
			}
			if (setFlag)
			{
				filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				break;
			}
			switch (i)
			{
			case 2:
				if (token.Length != length)
				{
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
					break;
				}
				base.SourceFragmentation = FilterStringTokens.SourceFragmentationTokenValues[i];
				filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
				token = string.Empty;
				break;
			case 0:
			{
				int startPos = length;
				filterStringParseState = SimpleSignedDouble(token.ToCharArray(), ref startPos, out var value, whiteSpaceOk: false, EnergyPrecision);
				if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
				{
					if (token.Length == length)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
					_filterSourceLowValState = ScanFilterEnums.FilterSourceLowVal.SourceCIDLow;
					_sourceCidLowVal = value;
					setFlag = true;
					base.SourceFragmentation = FilterStringTokens.SourceFragmentationTokenValues[i];
					base.SourceFragmentationType = ScanFilterEnums.VoltageTypes.SingleValue;
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
					token = string.Empty;
					break;
				}
				int separator = -1;
				startPos = length;
				filterStringParseState = SimpleDoubleRange(token.ToCharArray(), ref startPos, ref separator, out var rangeLow, out var rangeHigh, whiteSpaceOk: false, EnergyPrecision);
				if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
				{
					if (separator <= 0)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
					_filterSourceLowValState = ScanFilterEnums.FilterSourceLowVal.SourceCIDLow;
					_sourceCidLowVal = rangeLow;
					if (separator <= 0)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
					_filterSourceHighValState = ScanFilterEnums.FilterSourceHighVal.SourceCIDHigh;
					_sourceCidHighVal = rangeHigh;
					setFlag = true;
					base.SourceFragmentation = FilterStringTokens.SourceFragmentationTokenValues[i];
					base.SourceFragmentationType = ScanFilterEnums.VoltageTypes.Ramp;
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
					token = string.Empty;
				}
				break;
			}
			default:
				setFlag = true;
				base.SourceFragmentation = FilterStringTokens.SourceFragmentationTokenValues[i];
				base.SourceFragmentationType = (_hasSimModeCid ? ScanFilterEnums.VoltageTypes.SIM : ScanFilterEnums.VoltageTypes.Any);
				filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
				token = string.Empty;
				break;
			}
			break;
		}
		return filterStringParseState;
	}

	/// <summary>
	/// parse compensation voltage.
	/// </summary>
	/// <param name="token">
	/// The token.
	/// </param>
	/// <param name="setFlag">
	/// True if this item has already been parsed
	/// </param>
	/// <param name="names">
	/// The names.
	/// </param>
	/// <returns>
	/// The parse result
	/// </returns>
	private ScanFilterEnums.FilterStringParseState ParseCompensationVoltage(ref string token, ref bool setFlag, string[] names)
	{
		ScanFilterEnums.FilterStringParseState filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
		int num = names.Length;
		for (int i = 0; i < num; i++)
		{
			string obj = names[i];
			int length = obj.Length;
			if (string.Compare(obj, 0, token, 0, length) != 0)
			{
				continue;
			}
			if (setFlag)
			{
				filterStringParseState = ScanFilterEnums.FilterStringParseState.Bad;
				break;
			}
			switch (i)
			{
			case 2:
				if (token.Length != length)
				{
					return ScanFilterEnums.FilterStringParseState.Bad;
				}
				base.CompensationVoltage = FilterStringTokens.CompensationVoltageTokenValues[i];
				filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
				token = string.Empty;
				break;
			case 0:
			{
				int startPos = length;
				filterStringParseState = SimpleSignedDouble(token.ToCharArray(), ref startPos, out var value, whiteSpaceOk: false, EnergyPrecision);
				if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
				{
					if (token.Length == length)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
					_compensationVoltageLowValState = ScanFilterEnums.FilterSourceLowVal.SourceCIDLow;
					_compensationVoltageLowVal = value;
					setFlag = true;
					base.CompensationVoltage = FilterStringTokens.CompensationVoltageTokenValues[i];
					base.CompensationVoltageType = ScanFilterEnums.VoltageTypes.SingleValue;
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
					token = string.Empty;
					break;
				}
				int separator = -1;
				startPos = length;
				filterStringParseState = SimpleDoubleRange(token.ToCharArray(), ref startPos, ref separator, out var rangeLow, out var rangeHigh, whiteSpaceOk: false, EnergyPrecision);
				if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
				{
					if (separator <= 0)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
					_compensationVoltageLowValState = ScanFilterEnums.FilterSourceLowVal.SourceCIDLow;
					_compensationVoltageLowVal = rangeLow;
					if (separator <= 0)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
					_compensationVoltageHighValState = ScanFilterEnums.FilterSourceHighVal.SourceCIDHigh;
					_compensationVoltageHighVal = rangeHigh;
					setFlag = true;
					base.CompensationVoltage = FilterStringTokens.CompensationVoltageTokenValues[i];
					base.CompensationVoltageType = ScanFilterEnums.VoltageTypes.Ramp;
					filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
					token = string.Empty;
				}
				break;
			}
			default:
				setFlag = true;
				base.CompensationVoltage = FilterStringTokens.CompensationVoltageTokenValues[i];
				base.CompensationVoltageType = (_hasSimModeCid ? ScanFilterEnums.VoltageTypes.SIM : ScanFilterEnums.VoltageTypes.Any);
				filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
				token = string.Empty;
				break;
			}
			break;
		}
		return filterStringParseState;
	}

	/// <summary>
	/// parse a simplified double value range, i.e., no e-notation.
	/// "+123.4-200.34"  or "-123--345"
	/// </summary>
	/// <param name="token">text to parse</param>
	/// <param name="startPos">start of text to parse (updated to next unparsed char)</param>
	/// <param name="separator">index into text of separator</param>
	/// <param name="rangeLow">Low value found in range</param>
	/// <param name="rangeHigh">High value found in range (same as low, when no second value)</param>
	/// <param name="whiteSpaceOk">True if trailing spaces are valid</param>
	/// <param name="precision">Decimal precision</param>
	/// <returns>Parse state. Will be Bad on parse error, Next on valid parse</returns>
	private ScanFilterEnums.FilterStringParseState SimpleDoubleRange(char[] token, ref int startPos, ref int separator, out double rangeLow, out double rangeHigh, bool whiteSpaceOk, int precision)
	{
		ScanFilterEnums.FilterStringParseState filterStringParseState = SimpleSignedDouble(token, ref startPos, out rangeLow, whiteSpaceOk, precision, '-');
		rangeHigh = rangeLow;
		if (filterStringParseState == ScanFilterEnums.FilterStringParseState.Bad)
		{
			return ScanFilterEnums.FilterStringParseState.Bad;
		}
		int num = token.Length;
		if (startPos < num && token[startPos] == '-')
		{
			separator = startPos;
			startPos++;
			filterStringParseState = SimpleSignedDouble(token, ref startPos, out rangeHigh, whiteSpaceOk, precision);
		}
		return filterStringParseState;
	}

	/// <summary>
	/// set MS order and number of precursor masses.
	/// </summary>
	/// <param name="order">
	/// The order.
	/// </param>
	/// <param name="masses">
	/// The masses.
	/// </param>
	private void SetMsOrderAndNumPrecMasses(int order, int masses)
	{
		_msOrder = order;
		if (!_multiplexSet)
		{
			_precMassesCount = masses;
		}
	}

	/// <summary>
	/// Look for a precursor mass, plus activation codes
	/// </summary>
	/// <param name="token">token like <c>"1234.56@ecd35.6@hcd44.4"</c></param>
	/// <returns>Parse private state</returns>
	private ScanFilterEnums.FilterStringParseState ParsePrecursorMass(ref string token)
	{
		if (_currentPrecursor == 100)
		{
			return ScanFilterEnums.FilterStringParseState.Bad;
		}
		ScanFilterEnums.FilterStringParseState filterStringParseState = ScanFilterEnums.FilterStringParseState.Incomplete;
		if (!string.IsNullOrWhiteSpace(token))
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = true;
			for (int i = 0; i < token.Length; i++)
			{
				char c = token[i];
				if (char.IsDigit(c))
				{
					if (!flag2)
					{
						flag = true;
					}
					continue;
				}
				if (c == DecimalPt)
				{
					if (flag2)
					{
						flag5 = false;
						break;
					}
					flag2 = true;
					continue;
				}
				switch (c)
				{
				case '+':
				case '-':
					if (flag3)
					{
						flag5 = false;
						break;
					}
					flag3 = true;
					continue;
				case '@':
					if (!flag4)
					{
						flag4 = true;
						flag2 = false;
						flag3 = false;
						continue;
					}
					break;
				default:
				{
					if (!flag)
					{
						flag5 = false;
						break;
					}
					string key = IonMode(token, i);
					if (!IonModeSet.ContainsKey(key))
					{
						flag5 = false;
					}
					break;
				}
				}
				break;
			}
			if (!flag5)
			{
				return filterStringParseState;
			}
			if (!_msOrderSet)
			{
				return ScanFilterEnums.FilterStringParseState.Bad;
			}
			int num = token.IndexOf("@", StringComparison.Ordinal);
			if (num < 0)
			{
				int startPos = 0;
				filterStringParseState = SimpleSignedDouble(token.ToCharArray(), ref startPos, out var value, whiteSpaceOk: false, _massPrecision);
				if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
				{
					if (_currentPrecursor >= _precMassesCount)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
					SetPrecursorMassN(_currentPrecursor, value);
					token = string.Empty;
					_currentPrecursor++;
				}
			}
			else
			{
				int separatorPos = -1;
				int startPos2 = 0;
				filterStringParseState = SimpleDoubleAtDouble(token.ToCharArray(), ref startPos2, ref separatorPos, out var mass, out var energy, doubleSignedValuesOk: false, prefixedByAtDoubleSignedValuesOk: true);
				if (filterStringParseState != ScanFilterEnums.FilterStringParseState.Bad)
				{
					if (separatorPos <= 0)
					{
						return ScanFilterEnums.FilterStringParseState.Bad;
					}
					bool flag6 = false;
					bool flag7 = true;
					int length = token.Length;
					while (num + 1 < length && char.IsLetter(token[num + 1]))
					{
						num++;
						string text = IonMode(token, num);
						if (!IonModeSet.TryGetValue(text, out var value2))
						{
							return ScanFilterEnums.FilterStringParseState.Bad;
						}
						int length2 = text.Length;
						if (flag7)
						{
							_precEnergyValidArray[_currentPrecursor].Activation = value2;
							flag7 = false;
							int startPos3 = num + length2;
							if (startPos3 < length && token[startPos3] != '@')
							{
								if ((filterStringParseState = SimpleSignedDouble(token.ToCharArray(), ref startPos3, out var value3, whiteSpaceOk: false, EnergyPrecision)) == ScanFilterEnums.FilterStringParseState.Bad)
								{
									return filterStringParseState;
								}
								SetPrecursorEnergy(_currentPrecursor, value3);
								if (filterStringParseState == ScanFilterEnums.FilterStringParseState.Next && token[startPos3] != '@')
								{
									return ScanFilterEnums.FilterStringParseState.Bad;
								}
							}
							SetPrecursorMassN(_currentPrecursor, mass);
							_currentPrecursor++;
							if (_currentPrecursor > _precMassesCount)
							{
								_precMassesCount = _currentPrecursor;
							}
						}
						else
						{
							PrecursorActivation precursorActivation = new PrecursorActivation
							{
								Activation = value2,
								MsOrder = _currentPrecursor - 1,
								IsMultiple = true
							};
							int startPos4 = num + length2;
							if (startPos4 < length && token[startPos4] != '@')
							{
								if ((filterStringParseState = SimpleSignedDouble(token.ToCharArray(), ref startPos4, out var value4, whiteSpaceOk: false, EnergyPrecision)) == ScanFilterEnums.FilterStringParseState.Bad)
								{
									return filterStringParseState;
								}
								precursorActivation.PrecursorEnergy = value4;
								precursorActivation.PrecursorEnergyIsValid = true;
								if (filterStringParseState == ScanFilterEnums.FilterStringParseState.Next && token[startPos4] != '@')
								{
									return ScanFilterEnums.FilterStringParseState.Bad;
								}
							}
							_precursorActivations.Add(precursorActivation);
						}
						flag6 = true;
						filterStringParseState = ScanFilterEnums.FilterStringParseState.Good;
						int num2 = token.IndexOf("@", num, StringComparison.Ordinal);
						if (num2 < 0)
						{
							token = string.Empty;
							break;
						}
						num = num2;
					}
					if (separatorPos > 0 && !flag6)
					{
						if (_currentPrecursor >= _precMassesCount)
						{
							return ScanFilterEnums.FilterStringParseState.Bad;
						}
						SetPrecursorMassN(_currentPrecursor, mass);
						SetPrecursorEnergy(_currentPrecursor, energy);
						token = string.Empty;
					}
				}
			}
		}
		return filterStringParseState;
	}

	/// <summary>
	/// Set a precursor mass.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <param name="mass">
	/// The mass.
	/// </param>
	private void SetPrecursorMassN(int index, double mass)
	{
		_massArray[index] = mass;
	}

	/// <summary>
	/// Set precursor energy at a given index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <param name="energy">
	/// The energy.
	/// </param>
	private void SetPrecursorEnergy(int index, double energy)
	{
		_precEnergyArray[index] = energy;
		_precEnergyValidArray[index].PrecursorEnergyIsValid = true;
	}

	/// <summary>
	/// The update mass ranges 2.
	/// </summary>
	private void UpdateMassRanges2()
	{
		int num = 0;
		if (_precMassesCount > 100)
		{
			_precMassesCount = 100;
		}
		while (num < _precMassesCount)
		{
			if (_massArray[num++] < 0.0)
			{
				_precMassesCount = num - 1;
			}
		}
		_massRangeCount = 0;
		for (num = 0; _massRangeArray[num, 0] > 0.0 && num < 100; num++)
		{
			_massRangeCount++;
			if (_massRangeArray[num, 1] < 0.0)
			{
				_massRangeArray[num, 1] = _massRangeArray[num, 0];
			}
		}
	}

	/// <summary>
	/// Get precursor energy ex.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <param name="energy">
	/// The activation energy
	/// </param>
	/// <returns>
	/// The 'valid' bit plus the activation mode <see cref="T:System.UInt32" />.
	/// </returns>
	private uint GetPrecursorEnergyEx(int index, out double energy)
	{
		energy = _precEnergyArray[index];
		return _precEnergyValidArray[index].ToEnergyEx();
	}

	/// <summary>
	/// The multiple activations for MS n order.
	/// </summary>
	/// <param name="i">
	/// The i.
	/// </param>
	/// <returns>
	/// The number of MA in the extra array for this MS n Order
	/// </returns>
	private int MultipleActivationsForMsnOrder(int i)
	{
		int num = 0;
		for (int j = 0; j < _precursorActivations.Count; j++)
		{
			if (_precursorActivations[j].MsOrder == i)
			{
				num++;
			}
		}
		return num;
	}

	/// <summary>
	/// given the previous MA for this MS/MS Order, find the next
	/// </summary>
	/// <param name="order">
	/// The order.
	/// </param>
	/// <param name="lastMultiActivation">
	/// The last multi activation.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	private int NextMultipleActivationForOrder(int order, int lastMultiActivation)
	{
		int result = -1;
		int count = _precursorActivations.Count;
		if (lastMultiActivation + 1 < count)
		{
			for (int i = lastMultiActivation + 1; i < count; i++)
			{
				if (_precursorActivations[i].MsOrder == order)
				{
					result = i;
					break;
				}
			}
		}
		return result;
	}

	/// <summary>
	/// get precursor energy extra.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <param name="energy">
	/// The energy.
	/// </param>
	/// <returns>
	/// The energy code
	/// </returns>
	private uint GetPrecursorEnergyExtra(int index, out double energy)
	{
		PrecursorActivation precursorActivation = _precursorActivations[index];
		energy = precursorActivation.PrecursorEnergy;
		return precursorActivation.ToEnergyEx();
	}
}
