namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines what happens to a program in a PMD file
/// </summary>
public enum ProgramAction
{
	/// <summary>
	/// The program is run (exe)
	/// </summary>
	RunProgram,
	/// <summary>
	/// The program is run as an excel macro
	/// </summary>
	RunExcelMacro,
	/// <summary>
	/// No action
	/// </summary>
	DoNothing,
	/// <summary>
	/// Data is exported, using the specified export type
	/// </summary>
	ExportOnly
}
