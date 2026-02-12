namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Determines what data types are enabled for a report
/// </summary>
public interface IXcaliburReportSampleTypes
{
	/// <summary>
	/// Gets a value indicating whether report is enabled for standards
	/// </summary>
	bool Standards { get; }

	/// <summary>
	/// Gets a value indicating whether report is enabled for QCs
	/// </summary>
	bool Qcs { get; }

	/// <summary>
	/// Gets a value indicating whether report is enabled for Unknowns
	/// </summary>
	bool Unknowns { get; }

	/// <summary>
	/// Gets a value indicating whether report is enabled for Other sample types
	/// </summary>
	bool Other { get; }
}
