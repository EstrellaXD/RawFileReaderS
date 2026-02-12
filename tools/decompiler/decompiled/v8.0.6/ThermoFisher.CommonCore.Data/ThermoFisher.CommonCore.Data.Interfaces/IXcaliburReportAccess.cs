namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to read Xcalibur report settings
/// </summary>
public interface IXcaliburReportAccess
{
	/// <summary>
	/// Gets a value indicating whether report is enabled
	/// </summary>
	bool Enabled { get; }

	/// <summary>
	/// Gets the file save format of the report
	/// </summary>
	ReportTemplateType SaveAsType { get; }

	/// <summary>
	/// Gets the name of the report
	/// </summary>
	string ReportName { get; }
}
