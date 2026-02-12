using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// Wrapped log entry. Presents a log entry, as defined in the Data DLL.
/// </summary>
internal class WrappedLogEntry : LogEntry
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedLogEntry" /> class.
	/// </summary>
	/// <param name="statusLog">
	/// The status log.
	/// </param>
	/// <exception cref="T:System.ArgumentNullException">statusLog is null
	/// </exception>
	public WrappedLogEntry(StatusLogEntry statusLog)
	{
		if (statusLog == null)
		{
			throw new ArgumentNullException("statusLog");
		}
		List<LabelValuePair> valuePairs = statusLog.ValuePairs;
		int num = (base.Length = valuePairs.Count);
		base.Labels = new string[num];
		base.Values = new string[num];
		for (int i = 0; i < num; i++)
		{
			base.Labels[i] = valuePairs[i].Label;
			base.Values[i] = valuePairs[i].Value.ToString();
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedLogEntry" /> class.
	/// </summary>
	/// <param name="trailerExtra">
	/// The trailer extra.
	/// </param>
	/// <exception cref="T:System.ArgumentNullException">trailerExtra is null
	/// </exception>
	public WrappedLogEntry(List<LabelValuePair> trailerExtra)
	{
		if (trailerExtra == null)
		{
			throw new ArgumentNullException("trailerExtra");
		}
		int num = (base.Length = trailerExtra.Count);
		base.Labels = new string[num];
		base.Values = new string[num];
		for (int i = 0; i < num; i++)
		{
			base.Labels[i] = trailerExtra[i].Label;
			base.Values[i] = trailerExtra[i].Value.ToString();
		}
	}
}
