namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines the output of an Xcalibur report
/// </summary>
public enum ReportTemplateType
{
	/// <summary>
	/// No report output
	/// </summary>
	None,
	/// <summary>
	/// Report creates txt
	/// </summary>
	Text,
	/// <summary>
	/// Report creates word doc
	/// </summary>
	Doc,
	/// <summary>
	/// Report creates HTML
	/// </summary>
	Html,
	/// <summary>
	/// Report creates PDF file
	/// </summary>
	Pdf,
	/// <summary>
	/// Report creates Rtf file
	/// </summary>
	Rtf,
	/// <summary>
	/// Report creates XLS file (spreadsheet)
	/// </summary>
	Xls
}
