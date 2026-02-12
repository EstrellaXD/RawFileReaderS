namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines access to the signal header, from a chromatography device
/// </summary>
public interface IChannelHeader2DAccess : IChannelHeaderBase
{
	/// <summary>
	/// Gets a value indicating whether this data can be evaluated
	/// When not set, this is a diagnostic value (such as pressure)
	/// </summary>
	bool NeedsEvaluation { get; }

	/// <summary>
	/// Gets a label for the data (a title)
	/// </summary>
	string Label { get; }
}
