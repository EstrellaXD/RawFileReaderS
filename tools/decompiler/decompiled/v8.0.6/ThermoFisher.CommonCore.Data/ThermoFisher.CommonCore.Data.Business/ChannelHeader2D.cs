using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines the header for a signal which comes from a chromatography detector.
/// </summary>
public class ChannelHeader2D : ChannelHeaderBase, IChannelHeader2DAccess, IChannelHeaderBase
{
	private const int Num2DSpecificProperties = 2;

	/// <summary>
	/// Gets or sets a value indicating whether this data can be evaluated
	/// When not set, this is a diagnostic value (such as pressure)
	/// </summary>
	public bool NeedsEvaluation { get; set; }

	/// <summary>
	/// Gets or sets a label for the data (a title)
	/// </summary>
	public string Label { get; set; } = string.Empty;

	/// <summary>
	/// Default constructor
	/// </summary>
	public ChannelHeader2D()
	{
	}

	/// <summary>
	/// Constructor, based on a log record
	/// </summary>
	/// <param name="log">The data logged (must match the header)</param>
	/// <param name="logHeader">The header, which must be of a specific format</param>
	public ChannelHeader2D(IHeaderItem[] logHeader, object[] log)
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
		int num = 10;
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
			ChannelHeaderBase.ValidateBool(logHeader[index], "Needs evaluation");
			NeedsEvaluation = (bool)log[index++];
			ChannelHeaderBase.ValidateString(logHeader[index], "Label");
			Label = (string)log[index++];
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
		ChannelHeaderBase.AddBool(list, list2, "Needs evaluation", NeedsEvaluation);
		ChannelHeaderBase.AddString(list, list2, "Label", Label ?? string.Empty);
		ChannelHeaderBase.CreateExtendedLogItems(list, list2, base.ExtendedProperties);
		return new Tuple<IHeaderItem[], object[]>(list.ToArray(), list2.ToArray());
	}
}
