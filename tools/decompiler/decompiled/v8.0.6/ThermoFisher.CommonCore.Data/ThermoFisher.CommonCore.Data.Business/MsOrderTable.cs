using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Class to manage the MS order information in a filter.
/// Simplifies the reaction tables into one entry per MS/MS stage.
/// </summary>
public class MsOrderTable
{
	/// <summary>
	/// Gets the reaction data for all MS/MS stages
	/// </summary>
	public List<MsStage> Stages { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.MsOrderTable" /> class. 
	/// Construct an MS Order table from a filter or event
	/// </summary>
	/// <param name="filter">
	/// filter or event to analyze
	/// </param>
	public MsOrderTable(IScanEventBase filter)
	{
		Stages = new List<MsStage>();
		FormatMsOrder(filter);
	}

	/// <summary>
	/// create reactions table
	/// </summary>
	/// <param name="filter">Data from raw file</param>
	/// <returns>Table of reactions</returns>
	private static IReaction[] CreateReactions(IScanEventBase filter)
	{
		int massCount = filter.MassCount;
		IReaction[] array = new IReaction[massCount];
		for (int i = 0; i < massCount; i++)
		{
			array[i] = filter.GetReaction(i);
		}
		return array;
	}

	/// <summary>
	/// Create a list of MS/MS stages (one per parent)
	/// </summary>
	/// <param name="reactions">Reactions, which may have multiple per stage</param>
	private void CreateStages(IReaction[] reactions)
	{
		List<IReaction> list = null;
		foreach (IReaction reaction in reactions)
		{
			if (reaction.MultipleActivation)
			{
				if (list == null)
				{
					list = new List<IReaction>();
				}
				list.Add(reaction);
				continue;
			}
			if (list != null)
			{
				Stages.Add(new MsStage(list));
			}
			list = new List<IReaction> { reaction };
		}
		if (list != null)
		{
			Stages.Add(new MsStage(list));
		}
	}

	/// <summary>
	/// format the MS order.
	/// </summary>
	/// <param name="scanFilter">
	/// The scan filter.
	/// </param>
	private void FormatMsOrder(IScanEventBase scanFilter)
	{
		if (scanFilter.ScanMode != ScanModeType.Q1Ms && scanFilter.ScanMode != ScanModeType.Q3Ms)
		{
			IReaction[] reactions = CreateReactions(scanFilter);
			CreateStages(reactions);
		}
	}
}
