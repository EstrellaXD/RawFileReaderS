using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Setting to define a chromatogram Trace.
/// </summary>
[Serializable]
[DataContract]
[KnownType("KnownTypes")]
public class ChromatogramTraceSettings : CommonCoreDataObject, IChromatogramSettingsEx, IChromatogramSettings, IChromatogramTraceSettingsAccess, ICloneable
{
	private string[] _compoundNames;

	/// <summary>
	/// The delay in minutes.
	/// </summary>
	private double _delayInMin;

	/// <summary>
	/// The scan filter.
	/// </summary>
	private string _filter;

	/// <summary>
	/// The fragment mass.
	/// </summary>
	private double _fragmentMass;

	/// <summary>
	/// Flag for including reference and exception peaks.
	/// </summary>
	private bool _includeReference;

	/// <summary>
	/// The mass ranges.
	/// </summary>
	private Range[] _massRanges;

	/// <summary>
	/// The trace.
	/// </summary>
	private TraceType _trace;

	/// <summary>
	/// Gets or sets the compound names.
	/// </summary>
	[DataMember]
	public string[] CompoundNames
	{
		get
		{
			return _compoundNames;
		}
		set
		{
			_compoundNames = value;
		}
	}

	/// <summary>
	/// Gets or sets the delay in minutes.
	/// </summary>
	/// <value>Floating point delay in minutes</value>
	[DataMember]
	public double DelayInMin
	{
		get
		{
			return _delayInMin;
		}
		set
		{
			_delayInMin = value;
		}
	}

	/// <summary>
	/// Gets or sets the filter used in searching scans during trace build
	/// </summary>
	[DataMember]
	public string Filter
	{
		get
		{
			return _filter;
		}
		set
		{
			_filter = value;
		}
	}

	/// <summary>
	/// Gets or sets the fragment mass for neutral fragment filters.
	/// </summary>
	/// <value>Floating point fragment mass for neutral fragment filters</value>
	[DataMember]
	public double FragmentMass
	{
		get
		{
			return _fragmentMass;
		}
		set
		{
			_fragmentMass = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether reference and exception peaks are included
	/// in this chromatogram trace
	/// </summary>
	[DataMember]
	public bool IncludeReference
	{
		get
		{
			return _includeReference;
		}
		set
		{
			_includeReference = value;
		}
	}

	/// <summary>
	/// Gets or sets the number of mass ranges, or wavelength ranges for PDA.
	/// </summary>
	/// <remarks>
	/// If <see cref="P:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings.Trace" /> is MassRange then mass range values are used to build trace.
	/// </remarks>
	/// <value>Numeric count of mass ranges</value>
	public int MassRangeCount
	{
		get
		{
			if (_massRanges == null)
			{
				return 0;
			}
			return _massRanges.Length;
		}
		set
		{
			if (_massRanges == null)
			{
				_massRanges = new Range[value];
			}
			else
			{
				Array.Resize(ref _massRanges, value);
			}
		}
	}

	/// <summary>
	/// Gets or sets the mass ranges.
	/// </summary>
	/// <remarks>
	/// If <see cref="P:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings.Trace" /> is MassRange then mass range values are used to build trace.
	/// </remarks>
	/// <value>Array of mass ranges</value>
	[DataMember]
	public Range[] MassRanges
	{
		get
		{
			return _massRanges;
		}
		set
		{
			_massRanges = value;
		}
	}

	IRangeAccess[] IChromatogramTraceSettingsAccess.MassRanges => _massRanges;

	IRangeAccess[] IChromatogramSettings.MassRanges => _massRanges;

	/// <summary>
	/// Gets or sets the type of trace to construct
	/// </summary>
	/// <value>see <see cref="T:ThermoFisher.CommonCore.Data.Business.TraceType" /> for more details</value>
	[DataMember]
	public TraceType Trace
	{
		get
		{
			return _trace;
		}
		set
		{
			_trace = value;
		}
	}

	/// <summary>
	/// Gets or sets a provider for a value for a scan, used with Custom trace type.
	/// This caculates a value from the mass, intensity and (where available) charge data in a scan
	/// </summary>
	[XmlIgnore]
	public IScanValueProvider CustomValueProvider { get; set; }

	/// <summary>
	/// Optional RT range. If empty (0,0), "all trace" range is used
	/// </summary>
	[DataMember]
	public Range RtRange { get; set; }

	IRangeAccess IChromatogramSettingsEx.RtRange => RtRange;

	private static Type[] KnownTypes()
	{
		return new Type[1] { typeof(Range) };
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings" /> class.
	/// Default constructor creates a new instance of ChromatogramSettings
	/// </summary>
	public ChromatogramTraceSettings()
	{
		Trace = TraceType.TIC;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings" /> class.
	/// Constructor creates a new instance of ChromatogramSettings and
	/// sets the type of trace to construct.
	/// </summary>
	/// <param name="traceType">
	/// The type of trace to construct;  see <see cref="T:ThermoFisher.CommonCore.Data.Business.TraceType" /> for possible values
	/// </param>
	public ChromatogramTraceSettings(TraceType traceType)
	{
		Trace = traceType;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings" /> class.
	/// Constructor creates a new instance of ChromatogramSettings based on a read only interface
	/// </summary>
	/// <param name="access">
	/// Access to the read only parameters
	/// </param>
	public ChromatogramTraceSettings(IChromatogramTraceSettingsAccess access)
	{
		if (access == null)
		{
			throw new ArgumentNullException("access");
		}
		DelayInMin = access.DelayInMin;
		Filter = access.Filter;
		FragmentMass = access.FragmentMass;
		IncludeReference = access.IncludeReference;
		Trace = access.Trace;
		int massRangeCount = access.MassRangeCount;
		_massRanges = new Range[massRangeCount];
		for (int i = 0; i < massRangeCount; i++)
		{
			CopyRange(access.GetMassRange(i), i);
		}
		string[] compoundNames = access.CompoundNames;
		if (compoundNames != null)
		{
			CompoundNames = compoundNames.Clone() as string[];
		}
		else
		{
			CompoundNames = new string[0];
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings" /> class.
	/// Initialize settings for a mass chromatogram. Makes "TraceType.MassRange"
	/// </summary>
	/// <param name="filter">
	/// scan filter
	/// </param>
	/// <param name="ranges">
	/// The mass range(s)
	/// </param>
	public ChromatogramTraceSettings(string filter, IRangeAccess[] ranges)
	{
		_trace = TraceType.MassRange;
		_filter = filter;
		_massRanges = new Range[ranges.Length];
		for (int i = 0; i < ranges.Length; i++)
		{
			_massRanges[i] = new Range(ranges[i]);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings" /> class.
	/// Clones available data from the supplied interface.
	/// </summary>
	/// <param name="settings">
	/// The chromatogram settings.
	/// </param>
	private ChromatogramTraceSettings(IChromatogramSettings settings)
	{
		DelayInMin = settings.DelayInMin;
		Filter = settings.Filter;
		FragmentMass = settings.FragmentMass;
		IncludeReference = settings.IncludeReference;
		Trace = settings.Trace;
		int massRangeCount = settings.MassRangeCount;
		_massRanges = new Range[massRangeCount];
		IRangeAccess[] massRanges = settings.MassRanges;
		for (int i = 0; i < massRangeCount; i++)
		{
			CopyRange(massRanges[i], i);
		}
		CompoundNames = new string[0];
	}

	/// <summary>
	/// copy range.
	/// </summary>
	/// <param name="old">
	/// from (old value).
	/// </param>
	/// <param name="i">
	/// The index.
	/// </param>
	private void CopyRange(IRangeAccess old, int i)
	{
		_massRanges[i] = ((old != null) ? new Range
		{
			High = old.High,
			Low = old.Low
		} : new Range());
	}

	/// <summary>
	/// Convert from interface chromatogram settings.
	/// Note: This method is available as static converter, not
	/// a constructor overload, to avoid ambiguity with exiting overloads.
	/// </summary>
	/// <param name="settings">
	/// The settings.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings" />.
	/// </returns>
	public static ChromatogramTraceSettings FromChromatogramSettings(IChromatogramSettings settings)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		return new ChromatogramTraceSettings(settings);
	}

	/// <summary>
	/// Copies all of the items to from this object into the returned object.
	/// </summary>
	/// <returns>
	/// The clone.
	/// </returns>
	public object Clone()
	{
		ChromatogramTraceSettings chromatogramTraceSettings = (ChromatogramTraceSettings)MemberwiseClone();
		if (_massRanges != null)
		{
			chromatogramTraceSettings._massRanges = (Range[])_massRanges.Clone();
		}
		if (_compoundNames != null)
		{
			chromatogramTraceSettings._compoundNames = (string[])_compoundNames.Clone();
		}
		return chromatogramTraceSettings;
	}

	/// <summary>
	/// Gets a range value at 0-based index.
	/// </summary>
	/// <remarks>
	/// Use <see cref="P:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings.MassRangeCount" /> to find out the count of mass ranges.
	/// <para>
	/// </para>
	/// If <see cref="P:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings.Trace" /> is MassRange then mass range values are used to build trace.
	/// </remarks>
	/// <param name="index">
	/// Index at which to retrieve the range
	/// </param>
	/// <returns>
	/// <see cref="T:ThermoFisher.CommonCore.Data.Business.Range" /> value at give index
	/// </returns>
	public IRangeAccess GetMassRange(int index)
	{
		if (_massRanges == null || index < 0 || index >= _massRanges.Length)
		{
			return new Range
			{
				High = -1.0,
				Low = -1.0
			};
		}
		return _massRanges[index];
	}

	/// <summary>
	/// Sets a range value at 0-based index.
	/// </summary>
	/// <remarks>
	/// Set count of mass ranges using <see cref="P:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings.MassRangeCount" /> before setting any mass ranges.
	/// <para>
	/// </para>
	/// If <see cref="P:ThermoFisher.CommonCore.Data.Business.ChromatogramTraceSettings.Trace" /> is MassRange then mass range values are used to build trace.
	/// </remarks>
	/// <param name="index">
	/// Index at which new range value is to be set
	/// </param>
	/// <param name="range">
	/// New <see cref="T:ThermoFisher.CommonCore.Data.Business.Range" /> value to be set
	/// </param>
	public void SetMassRange(int index, IRangeAccess range)
	{
		if (_massRanges != null && index >= 0 && index < _massRanges.Length)
		{
			_massRanges[index] = new Range(range);
		}
	}
}
