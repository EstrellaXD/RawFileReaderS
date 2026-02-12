using System;
using System.Collections.Generic;
using System.Globalization;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
/// The filter scan event class is an internal class which hosts certain features
/// of both filters and scan events.
/// It supports the sorting code needing to "auto filter" a set of scan events.
/// Scan events are conceptually immutable (records of what occurred).
/// This class is mutable.
/// The class is created internally by the "Filter string parser" and by "Scan Event".
/// The class is exposed externally though "WrappedScanFilter" which has the (limited) public
/// interface to read and modify a filter.
/// </summary>
internal class FilterScanEvent : ScanEventDecorator, IComparable<FilterScanEvent>, IEquatable<FilterScanEvent>, IFilterScanEvent, IScanEventEdit, IRawFileReaderScanEvent, IFilterExtensions
{
	private static readonly double[] ResolutionLookup = new double[6] { 1.0, 0.1, 0.01, 0.001, 0.0001, 5E-05 };

	private double[] _compensationVoltages = Array.Empty<double>();

	private ScanFilterEnums.SourceCIDValidTypes[] _compensationVoltagesValid = Array.Empty<ScanFilterEnums.SourceCIDValidTypes>();

	private double[] _masses;

	private double[] _precursorEnergies;

	private uint[] _precursorEnergiesValid;

	private double[] _sourceCidValues = Array.Empty<double>();

	private ScanFilterEnums.SourceCIDValidTypes[] _sourceCidVoltagesValid = Array.Empty<ScanFilterEnums.SourceCIDValidTypes>();

	/// <summary>
	/// Gets the table of compensation voltages valid.
	/// </summary>
	public ScanFilterEnums.SourceCIDValidTypes[] CompensationVoltagesValid => _compensationVoltagesValid;

	/// <summary>
	/// Gets or sets the filter mass precision.
	/// </summary>
	/// <value>
	/// The filter mass precision.
	/// </value>
	public int FilterMassPrecision { get; set; }

	/// <summary>
	/// Gets or sets the filter mass resolution.
	/// </summary>
	/// <value>
	/// The filter mass resolution.
	/// </value>
	public double FilterMassResolution { get; set; }

	/// <summary>
	/// Gets or sets the name of the locale.
	/// </summary>
	/// <value>
	/// The name of the locale.
	/// </value>
	public string LocaleName { get; set; }

	/// <summary>
	/// Gets or sets the meta filters. <c>{hcd, etd, cid}</c>
	/// </summary>
	public MetaFilterType MetaFilters { get; set; }

	/// <summary>
	/// Gets the table of source CID valid.
	/// </summary>
	public ScanFilterEnums.SourceCIDValidTypes[] SourceCidVoltagesValid => _sourceCidVoltagesValid;

	/// <summary>
	/// Gets the total source values, which is all CID and CV.
	/// </summary>
	public int TotalSourceValues => _sourceCidVoltagesValid.Length + _compensationVoltagesValid.Length;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.FilterScanEvent" /> class.
	/// </summary>
	/// <param name="scanEvent">
	/// The scan event.
	/// </param>
	/// <param name="fromScan">
	/// The from scan.
	/// </param>
	public FilterScanEvent(IRawFileReaderScanEvent scanEvent, bool fromScan = true)
		: base(scanEvent)
	{
		LocaleName = "en-US";
		FilterMassPrecision = 2;
		FilterMassResolution = 0.01;
		MetaFilters = MetaFilterType.None;
		ScanEventToFilter(scanEvent, fromScan);
	}

	/// <summary>
	/// Compares the current object with another object of the same type.
	/// Exact compare for specialized qsort. Standard CompareTo considers items within an "instrument tolerance" as the same.
	/// This version uses 1.0E-6 for the tolerance.
	/// </summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>
	/// A value that indicates the relative order of the objects being compared. The return value has the following
	/// meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This
	/// object is equal to <paramref name="other" />. Greater than zero This object is greater than
	/// <paramref name="other" />.
	/// </returns>
	public int CompareExact(FilterScanEvent other)
	{
		ScanEvent scanEvent = (ScanEvent)base.ReaderScanEvent;
		ScanEvent other2 = (ScanEvent)other.BaseScanEvent;
		int num = scanEvent.ComparePart1(scanEvent, other2);
		if (num != 0)
		{
			return num;
		}
		num = CompareReactions(other, 1E-06, 1E-06);
		if (num != 0)
		{
			return num;
		}
		num = scanEvent.ComparePart2(other2, compareNames: false);
		if (num != 0)
		{
			return num;
		}
		num = base.Lock - other.Lock;
		if (num != 0)
		{
			return num;
		}
		num = base.TurboScan - other.TurboScan;
		if (num != 0)
		{
			return num;
		}
		num = base.UpperCaseApplied - other.UpperCaseApplied;
		if (num != 0)
		{
			return num;
		}
		num = base.UpperCaseFlags - other.UpperCaseFlags;
		if (num != 0)
		{
			return num;
		}
		num = base.LowerCaseApplied - other.LowerCaseApplied;
		if (num != 0)
		{
			return num;
		}
		return base.LowerCaseFlags - other.LowerCaseFlags;
	}

	/// <summary>
	/// Compares the current object with another object of the same type.
	///
	/// </summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <param name="usePrecursorTolerance">tolerance applied to mass ranges</param>
	/// <param name="toleranceFactor">when "usePrecursorTolerance" the tolerance is multiplied by this factor</param>
	/// <returns>
	/// A value that indicates the relative order of the objects being compared. The return value has the following
	/// meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This
	/// object is equal to <paramref name="other" />. Greater than zero This object is greater than
	/// <paramref name="other" />.
	/// </returns>
	public int CompareSmart(FilterScanEvent other, bool usePrecursorTolerance, double toleranceFactor)
	{
		double num = FilterMassResolution / 2.0;
		bool num2 = base.ReaderScanEvent.DependentDataFlag == ScanFilterEnums.IsDependent.Yes && other.DependentDataFlag == ScanFilterEnums.IsDependent.Yes;
		if (base.ReaderScanEvent.DependentDataFlag == ScanFilterEnums.IsDependent.Yes)
		{
			_ = 1;
		}
		else
			_ = other.DependentDataFlag == ScanFilterEnums.IsDependent.Yes;
		if (num2 && num > 0.2)
		{
			num = 0.2;
		}
		double massRangeTolerance = ((!usePrecursorTolerance) ? (num / 2.0) : (num * toleranceFactor));
		ScanEvent scanEvent = (ScanEvent)base.ReaderScanEvent;
		ScanEvent other2 = (ScanEvent)other.BaseScanEvent;
		int num3 = scanEvent.ComparePart1(scanEvent, other2);
		if (num3 != 0)
		{
			return num3;
		}
		num3 = CompareReactions(other, num, massRangeTolerance);
		if (num3 != 0)
		{
			return num3;
		}
		num3 = scanEvent.ComparePart2(other2, compareNames: false);
		if (num3 != 0)
		{
			return num3;
		}
		num3 = base.Lock - other.Lock;
		if (num3 != 0)
		{
			return num3;
		}
		num3 = base.TurboScan - other.TurboScan;
		if (num3 != 0)
		{
			return num3;
		}
		num3 = base.UpperCaseApplied - other.UpperCaseApplied;
		if (num3 != 0)
		{
			return num3;
		}
		num3 = base.UpperCaseFlags - other.UpperCaseFlags;
		if (num3 != 0)
		{
			return num3;
		}
		num3 = base.LowerCaseApplied - other.LowerCaseApplied;
		if (num3 != 0)
		{
			return num3;
		}
		return base.LowerCaseFlags - other.LowerCaseFlags;
	}

	/// <summary>
	/// Compares the current object with another object of the same type.
	/// </summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>
	/// A value that indicates the relative order of the objects being compared. The return value has the following
	/// meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This
	/// object is equal to <paramref name="other" />. Greater than zero This object is greater than
	/// <paramref name="other" />.
	/// </returns>
	public int CompareTo(FilterScanEvent other)
	{
		double num = FilterMassResolution / 2.0;
		if ((base.ReaderScanEvent.DependentDataFlag == ScanFilterEnums.IsDependent.Yes || other.DependentDataFlag == ScanFilterEnums.IsDependent.Yes) && num > 0.2)
		{
			num = 0.2;
		}
		ScanEvent scanEvent = (ScanEvent)base.ReaderScanEvent;
		ScanEvent other2 = (ScanEvent)other.BaseScanEvent;
		int num2 = scanEvent.ComparePart1(scanEvent, other2);
		if (num2 != 0)
		{
			return num2;
		}
		num2 = CompareReactions(other, num, num / 2.0);
		if (num2 != 0)
		{
			return num2;
		}
		num2 = scanEvent.ComparePart2(other2, compareNames: false);
		if (num2 != 0)
		{
			return num2;
		}
		num2 = base.Lock - other.Lock;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = base.TurboScan - other.TurboScan;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = base.UpperCaseApplied - other.UpperCaseApplied;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = base.UpperCaseFlags - other.UpperCaseFlags;
		if (num2 != 0)
		{
			return num2;
		}
		num2 = base.LowerCaseApplied - other.LowerCaseApplied;
		if (num2 != 0)
		{
			return num2;
		}
		return base.LowerCaseFlags - other.LowerCaseFlags;
	}

	/// <summary>
	/// create reactions table
	/// </summary>
	public void CreateReactions()
	{
		double[] masses = _masses;
		int num = ((masses != null) ? masses.Length : 0);
		if (num <= 0)
		{
			base.Reactions = Array.Empty<Reaction>();
			return;
		}
		base.Reactions = new Reaction[num];
		for (int i = 0; i < num; i++)
		{
			double precursorMass = _masses[i];
			double collisionEnergy = _precursorEnergies[i];
			uint collisionEnergyValid = _precursorEnergiesValid[i];
			int num2 = 1;
			double isolationWidthOffset = 0.0;
			double firstPrecursorMass = 0.0;
			double lastPrecursorMass = 0.0;
			base.Reactions[i] = new Reaction(precursorMass, num2, collisionEnergy, collisionEnergyValid, rangeIsValid: false, firstPrecursorMass, lastPrecursorMass, isolationWidthOffset);
		}
	}

	/// <summary>
	/// Indicates whether the current object is equal to another object of the same type.
	/// </summary>
	/// <param name="other">An object to compare with this object.</param>
	/// <returns>
	/// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
	/// </returns>
	public bool Equals(FilterScanEvent other)
	{
		return CompareTo(other) == 0;
	}

	/// <summary>
	/// Gets the filter string.
	/// </summary>
	/// <param name="filterMassPrecision">The filter mass precision.</param>
	/// <returns>The filter as a string</returns>
	public string GetFilterString(int filterMassPrecision)
	{
		return base.ReaderScanEvent.ToAutoFilterString(this, filterMassPrecision);
	}

	/// <summary>
	/// Count the number of compensation voltage values.
	/// </summary>
	/// <returns>The number of compensation voltage values</returns>
	public int NumCompensationVoltageValues()
	{
		int num = _compensationVoltagesValid.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (_compensationVoltagesValid[i] == ScanFilterEnums.SourceCIDValidTypes.CompensationVoltageEnergyValid)
			{
				num2++;
			}
		}
		return num2;
	}

	/// <summary>
	/// Numbers the source fragmentation information values.
	/// </summary>
	/// <returns>the number of values</returns>
	public int NumSourceCidInfoValues()
	{
		int num = _sourceCidVoltagesValid.Length;
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			if (_sourceCidVoltagesValid[i] == ScanFilterEnums.SourceCIDValidTypes.SourceCIDEnergyValid)
			{
				num2++;
			}
		}
		return num2;
	}

	/// <summary>
	/// Check if this object has the parent mass as another.
	/// </summary>
	/// <param name="other">The other.</param>
	/// <returns>true if the same</returns>
	public bool SameParentMass(ScanEventDecorator other)
	{
		double num = FilterMassResolution / 2.0;
		if ((base.DependentDataFlag == ScanFilterEnums.IsDependent.Yes || other.DependentDataFlag == ScanFilterEnums.IsDependent.Yes) && num > 0.2)
		{
			num = 0.2;
		}
		if (this == other)
		{
			return true;
		}
		int num2 = base.Reactions.Length;
		Reaction[] reactions = other.Reactions;
		if (num2 == 0 || num2 != reactions.Length)
		{
			return false;
		}
		for (int i = 0; i < num2; i++)
		{
			if (Math.Abs(base.Reactions[i].PrecursorMass - reactions[i].PrecursorMass) > num)
			{
				return false;
			}
			if (reactions[i].IsPrecursorEnergiesValid && Math.Abs(base.Reactions[i].CollisionEnergy - reactions[i].CollisionEnergy) > num)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Check if this object has the parent mass as another.
	/// </summary>
	/// <param name="other">The other.</param>
	/// <returns>true if the same</returns>
	public bool SameFirstParentMass(ScanEventDecorator other)
	{
		if (this == other)
		{
			return true;
		}
		int num = base.Reactions.Length;
		Reaction[] reactions = other.Reactions;
		if (num == 0 || num != reactions.Length)
		{
			return false;
		}
		double num2 = FilterMassResolution / 2.0;
		if ((base.DependentDataFlag == ScanFilterEnums.IsDependent.Yes || other.DependentDataFlag == ScanFilterEnums.IsDependent.Yes) && num2 > 0.2)
		{
			num2 = 0.2;
		}
		if (Math.Abs(base.Reactions[0].PrecursorMass - reactions[0].PrecursorMass) > num2)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// set compensation voltage type.
	/// </summary>
	/// <param name="type">
	/// The type.
	/// </param>
	public void SetCompensationVoltageType(ScanFilterEnums.VoltageTypes type)
	{
		base.CompensationVoltageType = type;
	}

	/// <summary>
	/// when we set the number of digits to display in the filter masses,
	/// let's also set the comparison value to use in comparing filters.
	/// This value used to be 0.4 AMU
	/// Sets the filter mass resolution.
	/// </summary>
	/// <param name="massResolution">
	/// Mass Resolution to apply
	/// </param>
	public void SetFilterMassResolution(double massResolution)
	{
		FilterMassResolution = massResolution;
	}

	/// <summary>
	/// Sets the filter mass resolution by mass precision.
	/// </summary>
	/// <param name="n">The n.</param>
	public void SetFilterMassResolutionByMassPrecision(int n)
	{
		if (n < 0 || n >= 6)
		{
			n = 2;
		}
		FilterMassPrecision = n;
		FilterMassResolution = ResolutionLookup[n];
	}

	/// <summary>
	/// set (precursor) masses.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <param name="value">
	/// The value.
	/// </param>
	public void SetMasses(int index, double value)
	{
		_masses[index] = value;
	}

	/// <summary>
	/// Set the number of precursor masses.
	/// </summary>
	/// <param name="n">
	/// The number of masses.
	/// </param>
	public void SetNumMasses(int n)
	{
		if (n == 0)
		{
			_masses = Array.Empty<double>();
			_precursorEnergies = Array.Empty<double>();
			_precursorEnergiesValid = Array.Empty<uint>();
			return;
		}
		int num = _precursorEnergiesValid.Length;
		Array.Resize(ref _masses, n);
		Array.Resize(ref _precursorEnergies, n);
		Array.Resize(ref _precursorEnergiesValid, n);
		if (n > num)
		{
			for (int i = num; i < n; i++)
			{
				_precursorEnergiesValid[i] = 1u;
			}
		}
	}

	/// <summary>
	/// set precursor energy.
	/// </summary>
	/// <param name="n">
	/// The index to the precursor table.
	/// </param>
	/// <param name="energy">
	/// The energy.
	/// </param>
	public void SetPrecursorEnergy(int n, double energy)
	{
		_precursorEnergies[n] = energy;
	}

	/// <summary>
	/// set precursor energy is valid ex.
	/// </summary>
	/// <param name="n">
	/// The index to the precursor table.
	/// </param>
	/// <param name="ie">
	/// The extended energy valid flags.
	/// </param>
	public void SetPrecursorEnergyIsValidEx(int n, uint ie)
	{
		_precursorEnergiesValid[n] = ie;
	}

	/// <summary>
	/// set source fragmentation type.
	/// </summary>
	/// <param name="type">
	/// The type.
	/// </param>
	public void SetSourceFragmentationType(ScanFilterEnums.VoltageTypes type)
	{
		base.SourceFragmentationType = type;
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> that represents this instance.
	/// </summary>
	/// <returns>
	/// A <see cref="T:System.String" /> that represents this instance.
	/// </returns>
	public override string ToString()
	{
		IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
		if (!string.IsNullOrWhiteSpace(LocaleName))
		{
			invariantCulture = new CultureInfo(LocaleName);
			string listSeparator = CultureInfo.GetCultureInfo(LocaleName).TextInfo.ListSeparator;
			return base.ReaderScanEvent.ToAutoFilterString(this, FilterMassPrecision, -1, -1, invariantCulture, listSeparator);
		}
		return base.ReaderScanEvent.ToAutoFilterString(this, FilterMassPrecision);
	}

	/// <summary>
	/// Sets the "i" indexed Compensations voltage information value.
	/// Skips over any items which have already been set for SID mode.
	/// </summary>
	/// <param name="i">The i.</param>
	/// <param name="value">The d value.</param>
	internal void CompensationVoltageInfoValue(int i, double value)
	{
		_compensationVoltages[i] = value;
		_compensationVoltagesValid[i] = ScanFilterEnums.SourceCIDValidTypes.CompensationVoltageEnergyValid;
	}

	/// <summary>
	/// Sets the number of compensation voltage info values.
	/// </summary>
	/// <param name="n">
	/// The number.
	/// </param>
	internal void NumCompensationVoltageInfoValues(int n)
	{
		int num = _compensationVoltagesValid.Length;
		Array.Resize(ref _compensationVoltagesValid, n);
		for (int i = num; i < n; i++)
		{
			_compensationVoltagesValid[i] = ScanFilterEnums.SourceCIDValidTypes.AcceptAnySourceCIDEnergy;
		}
		Array.Resize(ref _compensationVoltages, n);
	}

	/// <summary>
	/// Sets The number of source fragmentation info values.
	/// </summary>
	/// <param name="n">
	/// The number.
	/// </param>
	internal void NumSourceFragmentationInfoValues(int n)
	{
		int num = _sourceCidVoltagesValid.Length;
		Array.Resize(ref _sourceCidVoltagesValid, n);
		for (int i = num; i < n; i++)
		{
			_sourceCidVoltagesValid[i] = ScanFilterEnums.SourceCIDValidTypes.AcceptAnySourceCIDEnergy;
		}
		Array.Resize(ref _sourceCidValues, n);
	}

	/// <summary>
	/// Sets the "i" indexed Compensations voltage information value.
	/// Skips over any items which have already been set for SID mode.
	/// </summary>
	/// <param name="i">The i.</param>
	/// <param name="value">The d value.</param>
	internal void SourceCidInfoValue(int i, double value)
	{
		_sourceCidValues[i] = value;
		_sourceCidVoltagesValid[i] = ScanFilterEnums.SourceCIDValidTypes.SourceCIDEnergyValid;
	}

	/// <summary>
	/// Compares the reactions.
	/// </summary>
	/// <param name="other">The other reactions.</param>
	/// <param name="filterMassResolution">The filter mass resolution.</param>
	/// <param name="massRangeTolerance">tolerance applied to mass ranges</param>
	/// <returns>Standard comparison result</returns>
	private int CompareReactions(FilterScanEvent other, double filterMassResolution, double massRangeTolerance)
	{
		IList<Reaction> reactions = other.Reactions;
		IList<Reaction> reactions2 = base.Reactions;
		int count;
		int num = (count = reactions2.Count) - reactions.Count;
		if (num != 0)
		{
			return num;
		}
		for (int i = 0; i < count; i++)
		{
			num = Utilities.CompareDoubles(reactions2[i].PrecursorMass, reactions[i].PrecursorMass, filterMassResolution);
			if (num != 0)
			{
				return num;
			}
		}
		num = ScanEvent.CompareMassRanges(base.MassRanges, other.MassRanges, massRangeTolerance);
		if (num != 0)
		{
			return num;
		}
		for (int j = 0; j < count; j++)
		{
			num = reactions2[j].CompareDetails(reactions[j]);
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	/// <summary>
	/// converts the event to filter.
	/// </summary>
	/// <param name="scanEvent">The scan event.</param>
	/// <param name="fromScan">True when this is data from a scan. False when it is typed filter text</param>
	private void ScanEventToFilter(IRawFileReaderScanEvent scanEvent, bool fromScan = true)
	{
		SetFilterMassResolutionByMassPrecision(scanEvent.GetRunHeaderFilterMassPrecision());
		if (fromScan)
		{
			base.DependentDataFlag = ((scanEvent.DependentDataFlag == ScanFilterEnums.IsDependent.No) ? ScanFilterEnums.IsDependent.Any : scanEvent.DependentDataFlag);
			base.Wideband = ((scanEvent.Wideband == ScanFilterEnums.OffOnTypes.Off) ? ScanFilterEnums.OffOnTypes.Any : scanEvent.Wideband);
			base.SupplementalActivation = ((scanEvent.SupplementalActivation == ScanFilterEnums.OffOnTypes.Off) ? ScanFilterEnums.OffOnTypes.Any : scanEvent.SupplementalActivation);
			base.MultiStateActivation = ((scanEvent.MultiStateActivation == ScanFilterEnums.OffOnTypes.Off) ? ScanFilterEnums.OffOnTypes.Any : scanEvent.MultiStateActivation);
			base.AccurateMassType = ((scanEvent.AccurateMassType == ScanFilterEnums.AccurateMassTypes.Off) ? ScanFilterEnums.AccurateMassTypes.AcceptAnyAccurateMass : scanEvent.AccurateMassType);
			base.Detector = ((scanEvent.Detector == ScanFilterEnums.DetectorType.IsInValid) ? ScanFilterEnums.DetectorType.Any : scanEvent.Detector);
		}
		else
		{
			base.DependentDataFlag = scanEvent.DependentDataFlag;
			base.Wideband = scanEvent.Wideband;
			base.SupplementalActivation = scanEvent.SupplementalActivation;
			base.MultiStateActivation = scanEvent.MultiStateActivation;
			base.AccurateMassType = scanEvent.AccurateMassType;
			base.Detector = scanEvent.Detector;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		double[] sourceFragmentations = scanEvent.SourceFragmentations;
		if (scanEvent.SourceFragmentation == ScanFilterEnums.OnOffTypes.Off || (sourceFragmentations != null && sourceFragmentations.Length == 0))
		{
			base.SourceFragmentation = ScanFilterEnums.OnOffTypes.Any;
			base.SourceFragmentationType = ScanFilterEnums.VoltageTypes.Any;
		}
		else
		{
			base.SourceFragmentation = scanEvent.SourceFragmentation;
			base.SourceFragmentationType = scanEvent.SourceFragmentationType;
			num2 = VoltageCount(scanEvent.SourceFragmentationType, sourceFragmentations.Length);
		}
		if (scanEvent.CompensationVoltage == ScanFilterEnums.OnOffTypes.Off || scanEvent.SourceFragmentations.Length == 0)
		{
			base.CompensationVoltage = ScanFilterEnums.OnOffTypes.Any;
			base.CompensationVoltageType = ScanFilterEnums.VoltageTypes.Any;
		}
		else
		{
			base.CompensationVoltage = scanEvent.CompensationVoltage;
			base.CompensationVoltageType = scanEvent.CompensationVoltageType;
			num3 = VoltageCount(scanEvent.CompensationVoltageType, sourceFragmentations.Length);
		}
		if (num3 + num2 <= scanEvent.SourceFragmentations.Length)
		{
			if (num2 > 0)
			{
				NumSourceFragmentationInfoValues(num2);
				for (int i = 0; i < num2; i++)
				{
					if (num < sourceFragmentations.Length)
					{
						SourceCidInfoValue(i, sourceFragmentations[num++]);
					}
					else
					{
						SourceCidInfoValue(i, 0.0);
					}
				}
			}
			if (num3 > 0)
			{
				NumCompensationVoltageInfoValues(num3);
				for (int j = 0; j < num3; j++)
				{
					if (num < sourceFragmentations.Length)
					{
						CompensationVoltageInfoValue(j, sourceFragmentations[num++]);
					}
					else
					{
						CompensationVoltageInfoValue(j, 0.0);
					}
				}
			}
		}
		else
		{
			if (num2 > 0)
			{
				base.SourceFragmentation = ScanFilterEnums.OnOffTypes.On;
				base.SourceFragmentationType = ScanFilterEnums.VoltageTypes.NoValue;
			}
			if (num3 > 0)
			{
				NumCompensationVoltageInfoValues(num3);
				for (int k = 0; k < num3; k++)
				{
					if (num < sourceFragmentations.Length)
					{
						CompensationVoltageInfoValue(k, sourceFragmentations[num++]);
					}
					else
					{
						CompensationVoltageInfoValue(k, 0.0);
					}
				}
			}
		}
		base.Reactions = scanEvent.Reactions;
		int num4 = base.Reactions.Length;
		if (num4 <= 0)
		{
			_precursorEnergiesValid = Array.Empty<uint>();
			_masses = Array.Empty<double>();
			_precursorEnergies = Array.Empty<double>();
		}
		else
		{
			_precursorEnergiesValid = new uint[num4];
			_masses = new double[num4];
			_precursorEnergies = new double[num4];
			for (int l = 0; l < num4; l++)
			{
				Reaction reaction = base.Reactions[l];
				_masses[l] = reaction.PrecursorMass;
				_precursorEnergies[l] = reaction.CollisionEnergy;
				_precursorEnergiesValid[l] = reaction.CollisionEnergyValidEx;
			}
		}
		base.LowerCaseFlags = scanEvent.LowerCaseFlags;
		base.UpperCaseFlags = scanEvent.UpperCaseFlags;
		if (fromScan)
		{
			base.LowerCaseApplied = scanEvent.LowerCaseFlags;
			base.UpperCaseApplied = scanEvent.UpperCaseFlags;
			base.TurboScan = ((scanEvent.TurboScan == ScanFilterEnums.OnOffTypes.Off) ? ScanFilterEnums.OnOffTypes.Any : scanEvent.TurboScan);
			base.Lock = ((scanEvent.Lock == ScanFilterEnums.OnOffTypes.Off) ? ScanFilterEnums.OnOffTypes.Any : scanEvent.Lock);
			base.Multiplex = ((scanEvent.Multiplex == ScanFilterEnums.OffOnTypes.Off) ? ScanFilterEnums.OffOnTypes.Any : scanEvent.Multiplex);
			base.ParamA = ((scanEvent.ParamA == ScanFilterEnums.OffOnTypes.Off) ? ScanFilterEnums.OffOnTypes.Any : scanEvent.ParamA);
			base.ParamB = ((scanEvent.ParamB == ScanFilterEnums.OffOnTypes.Off) ? ScanFilterEnums.OffOnTypes.Any : scanEvent.ParamB);
			base.ParamF = ((scanEvent.ParamF == ScanFilterEnums.OffOnTypes.Off) ? ScanFilterEnums.OffOnTypes.Any : scanEvent.ParamF);
			base.SpsMultiNotch = ((scanEvent.SpsMultiNotch == ScanFilterEnums.OffOnTypes.Off) ? ScanFilterEnums.OffOnTypes.Any : scanEvent.SpsMultiNotch);
			base.ParamR = ((scanEvent.ParamR == ScanFilterEnums.OffOnTypes.Off) ? ScanFilterEnums.OffOnTypes.Any : scanEvent.ParamR);
			base.ParamV = ((scanEvent.ParamV == ScanFilterEnums.OffOnTypes.Off) ? ScanFilterEnums.OffOnTypes.Any : scanEvent.ParamV);
			base.Ultra = ((scanEvent.Ultra == ScanFilterEnums.OnOffTypes.Off) ? ScanFilterEnums.OnOffTypes.Any : scanEvent.Ultra);
			base.Enhanced = ((scanEvent.Enhanced == ScanFilterEnums.OnOffTypes.Off) ? ScanFilterEnums.OnOffTypes.Any : scanEvent.Enhanced);
			base.ElectronCaptureDissociationType = ((scanEvent.ElectronCaptureDissociationType == ScanFilterEnums.OnAnyOffTypes.Off) ? ScanFilterEnums.OnAnyOffTypes.Any : scanEvent.ElectronCaptureDissociationType);
			base.MultiPhotonDissociationType = ((scanEvent.MultiPhotonDissociationType == ScanFilterEnums.OnAnyOffTypes.Off) ? ScanFilterEnums.OnAnyOffTypes.Any : scanEvent.MultiPhotonDissociationType);
		}
		else
		{
			base.LowerCaseApplied = scanEvent.LowerCaseApplied;
			base.UpperCaseApplied = scanEvent.UpperCaseApplied;
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
		}
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
	/// Count the number of volt values used for a voltage type
	/// </summary>
	/// <param name="voltageType">Style of voltage values</param>
	/// <param name="all">All voltages in the scan event</param>
	/// <returns>The number of voltages</returns>
	private int VoltageCount(ScanFilterEnums.VoltageTypes voltageType, int all)
	{
		return voltageType switch
		{
			ScanFilterEnums.VoltageTypes.SingleValue => 1, 
			ScanFilterEnums.VoltageTypes.Ramp => 2, 
			ScanFilterEnums.VoltageTypes.SIM => all, 
			_ => 0, 
		};
	}
}
