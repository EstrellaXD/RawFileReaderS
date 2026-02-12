namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to read Xcalibur program settings
/// </summary>
public interface IXcaliburProgramAccess : IXcaliburReportSampleTypes
{
	/// <summary>
	/// Gets a value indicating whether report is enabled
	/// </summary>
	bool Enabled { get; }

	/// <summary>
	/// Gets the name of the program
	/// </summary>
	string ProgramName { get; }

	/// <summary>
	/// Gets parameters to the program
	/// </summary>
	string Parameters { get; }

	/// <summary>
	/// Gets the action of this program (such as run exe, or export)
	/// </summary>
	ProgramAction Action { get; }

	/// <summary>
	/// Gets a value indicating whether to synchronize this action.
	/// If false, other programs may be run in parallel with this.
	/// </summary>
	bool Synchronize { get; }

	/// <summary>
	/// Gets the file save format of the export
	/// </summary>
	ProgramExportType ExportType { get; }
}
