using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines the header for a 3D field signal which comes from a chromatography detector.
/// </summary>
public class ChannelHeader3D : ChannelHeaderBase, IChannelHeader3DAccess, IChannelHeaderBase
{
	private const int Num3DSpecificProperties = 3;

	/// <summary>
	/// The unit of the scan axis. This is the device specific y-axis. It might be e.g. the wavelength axis in
	/// case of an UV detector.
	/// </summary>
	public string ScanUnit { get; set; }

	/// <summary>
	/// The acquisition state of a signal or spectral field.
	///    Unknown,
	///    NotStarted,
	///    Acquiring,
	///    Finished,
	///    Error
	/// </summary>
	public string State { get; set; }

	/// <summary>
	/// The kind of detector that is used to record the spectral field. It corresponds to the kind of measuring
	/// principle that is used by the detector.
	///     Unknown
	///     UV
	///     MS
	///     Amperometry
	/// </summary>
	public string DetectorKind { get; set; }

	/// <summary>
	/// Default constructor
	/// </summary>
	public ChannelHeader3D()
	{
	}

	/// <summary>
	/// Constructor, based on a log record
	/// </summary>
	/// <param name="log">The data logged (must match the header)</param>
	/// <param name="logHeader">The header, which must be of a specific format</param>
	public ChannelHeader3D(IHeaderItem[] logHeader, object[] log)
	{
		if (logHeader == null)
		{
			throw new ArgumentNullException("logHeader");
		}
		if (log == null)
		{
			throw new ArgumentNullException("log");
		}
		if (logHeader.Length != log.Length || logHeader.Length == 0)
		{
			throw new ArgumentException("Log Header and Log Record must have same number of entries");
		}
		int num = 11;
		int index = 0;
		if (logHeader[0].Label == "Version")
		{
			num++;
			base.Version = (string)log[index++];
		}
		if (logHeader.Length < num)
		{
			throw new ArgumentException("Incorrect log format");
		}
		try
		{
			index = ImportCommonLogItems(logHeader, log, index);
			ChannelHeaderBase.ValidateString(logHeader[index], "Detector kind");
			DetectorKind = (string)log[index++];
			ChannelHeaderBase.ValidateString(logHeader[index], "Scan unit");
			ScanUnit = (string)log[index++];
			ChannelHeaderBase.ValidateString(logHeader[index], "State");
			State = (string)log[index++];
			base.ExtendedProperties = ChannelHeaderBase.ImportExtendedProperties(logHeader, log, index);
			if (base.ExtendedProperties == null)
			{
				base.ExtendedProperties = new ChannelProperty[0];
			}
		}
		catch (FormatException ex)
		{
			throw new ArgumentException("Log Header is not in the expected format: " + ex.Message);
		}
	}

	/// <summary>
	/// Create the log headers for a generic log, which match this channel header
	/// </summary>
	/// <returns></returns> 
	public override Tuple<IHeaderItem[], object[]> CreateLog()
	{
		List<IHeaderItem> list = new List<IHeaderItem>();
		List<object> list2 = new List<object>();
		CreateCommonLogItems(list, list2);
		ChannelHeaderBase.AddString(list, list2, "Detector Kind", DetectorKind ?? string.Empty);
		ChannelHeaderBase.AddString(list, list2, "Scan Unit", ScanUnit ?? string.Empty);
		ChannelHeaderBase.AddString(list, list2, "State", State ?? string.Empty);
		ChannelHeaderBase.CreateExtendedLogItems(list, list2, base.ExtendedProperties);
		return new Tuple<IHeaderItem[], object[]>(list.ToArray(), list2.ToArray());
	}
}
