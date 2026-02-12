using System.Collections.Generic;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines one stage of MS/MS, which may have multiple reactions.
/// For a parent or neutral scan, one stage is required.
/// For an MS/MS scan, one stage per MS/MS level is needed.
/// For example: define 2 stages for an MS3 experiment.
/// Each stage must have one or more reactions.
/// </summary>
public class MsStage
{
	/// <summary>
	/// Gets or sets the set of reactions done at an MS stage.
	/// There must be at least one reaction for a valid stage.
	/// The first reaction of a stage must not have the MultipleActivation property set.
	/// Subsequent reactions of a stage must have the MultipleActivation property set.
	/// </summary>
	public List<IReaction> Reactions { get; set; }

	/// <summary>
	/// Gets the precursor mass for this MS/MS stage.
	/// This is the precursor mass of the first reaction, and is defined as "0"
	/// if no reactions have been added yet.
	/// </summary>
	public double PrecursorMass
	{
		get
		{
			if (Reactions != null && Reactions.Count >= 1)
			{
				return Reactions[0].PrecursorMass;
			}
			return 0.0;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.MsStage" /> class. 
	/// Construct an MS stage from a list of reactions
	/// </summary>
	/// <param name="reactions">
	/// Reactions for this stage
	/// </param>
	public MsStage(List<IReaction> reactions)
	{
		Reactions = reactions;
	}
}
