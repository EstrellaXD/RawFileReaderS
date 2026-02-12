namespace ThermoFisher.CommonCore.Data.FilterEnums;

/// <summary>
/// Specifies scan mode in scans.
/// </summary>
public enum ScanModeType
{
	/// <summary>
	/// A full scan.
	/// </summary>
	Full,
	/// <summary>
	/// A zoom scan.
	/// </summary>
	Zoom,
	/// <summary>
	/// A SIM (selected Ion Monitoring) scan.
	/// </summary>
	Sim,
	/// <summary>
	/// A SRM (Selected Reaction Monitoring) scan.
	/// </summary>
	Srm,
	/// <summary>
	/// A CRM (Continuous Reaction Monitoring) scan.
	/// </summary>
	Crm,
	/// <summary>
	/// any scan.
	/// </summary>
	Any,
	/// <summary>
	/// A Q1 MS scan (first quad of triple).
	/// </summary>
	Q1Ms,
	/// <summary>
	/// A Q3 MS scan  (third quad of triple).
	/// </summary>
	Q3Ms
}
