using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Class to hold chromatogram settings of a peak
/// </summary>
[Serializable]
[DataContract]
public class PeakChromatogramSettings : CommonCoreDataObject, ICloneable, IPeakChromatogramSettingsAccess
{
	private string _filter;

	private ChromatogramTraceSettings _chroSettings;

	private ChromatogramTraceSettings _chroSettings2;

	private Device _instrument;

	private int _instrumentIndex = 1;

	private TraceOperator _traceOperator;

	/// <summary>
	/// Gets or sets the scan filter.
	/// This determines which scans are included in the chromatogram.
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
	/// Gets the chromatogram settings.
	/// This defines how data for a chromatogram point is constructed from a scan.
	/// </summary>
	IChromatogramTraceSettingsAccess IPeakChromatogramSettingsAccess.ChroSettings => ChroSettings;

	/// <summary>
	/// Gets the chromatogram settings
	/// When there is a trace operator set,
	/// This defines how data for a chromatogram point is constructed from a scan for the chromatogram
	/// to be added or subtracted.
	/// </summary>
	IChromatogramTraceSettingsAccess IPeakChromatogramSettingsAccess.ChroSettings2 => ChroSettings2;

	/// <summary>
	/// Gets or sets the chromatogram settings.
	/// This defines how data for a chromatogram point is constructed from a scan.
	/// </summary>
	[DataMember]
	public ChromatogramTraceSettings ChroSettings
	{
		get
		{
			return _chroSettings;
		}
		set
		{
			_chroSettings = value;
		}
	}

	/// <summary>
	/// Gets or sets the chromatogram settings
	/// When there is a trace operator set,
	/// This defines how data for a chromatogram point is constructed from a scan for the chromatogram
	/// to be added or subtracted.
	/// </summary>
	[DataMember]
	public ChromatogramTraceSettings ChroSettings2
	{
		get
		{
			return _chroSettings2;
		}
		set
		{
			_chroSettings2 = value;
		}
	}

	/// <summary>
	/// Gets or sets the device type.
	/// This defines which data stream within the raw file is used. 
	/// </summary>
	[DataMember]
	public Device Instrument
	{
		get
		{
			return _instrument;
		}
		set
		{
			_instrument = value;
		}
	}

	/// <summary>
	/// Gets the instrument index (starting from 1).
	/// For example: "3" for the third UV detector.
	/// </summary>
	[DataMember]
	public int InstrumentIndex
	{
		get
		{
			return _instrumentIndex;
		}
		set
		{
			_instrumentIndex = value;
		}
	}

	/// <summary>
	/// Gets or sets the trace operator.
	/// If the operator is not "None" then a second chromatogram can be added to or subtracted from the first.
	/// </summary>
	[DataMember]
	public TraceOperator TraceOperator
	{
		get
		{
			return _traceOperator;
		}
		set
		{
			_traceOperator = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.PeakChromatogramSettings" /> class. 
	/// Constructor for PeakChromatogramSettings
	/// </summary>
	public PeakChromatogramSettings()
	{
		_filter = string.Empty;
		_chroSettings = new ChromatogramTraceSettings();
		_chroSettings2 = new ChromatogramTraceSettings();
		Instrument = Device.MS;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.PeakChromatogramSettings" /> class. 
	/// Construct this object from read only interface
	/// </summary>
	/// <param name="settings">The settings to copy from
	/// </param>
	public PeakChromatogramSettings(IPeakChromatogramSettingsAccess settings)
	{
		if (settings != null)
		{
			ChroSettings2 = new ChromatogramTraceSettings(settings.ChroSettings2);
			ChroSettings = new ChromatogramTraceSettings(settings.ChroSettings);
			Filter = settings.Filter;
			Instrument = settings.Instrument;
			TraceOperator = settings.TraceOperator;
		}
	}

	/// <summary>
	/// Create a deep copy of this object
	/// </summary>
	/// <returns>Copy of object</returns>
	public object Clone()
	{
		PeakChromatogramSettings obj = (PeakChromatogramSettings)MemberwiseClone();
		obj._chroSettings = (ChromatogramTraceSettings)_chroSettings.Clone();
		obj._chroSettings2 = (ChromatogramTraceSettings)_chroSettings2.Clone();
		obj._filter = _filter;
		return obj;
	}
}
