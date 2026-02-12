using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to get information about a sequence
/// </summary>
public interface ISequenceInfo
{
	/// <summary>
	/// Gets the display width of each sequence column
	/// </summary>
	short[] ColumnWidth { get; }

	/// <summary>
	/// Gets the column order (see home page?)
	/// </summary>
	short[] TypeToColumnPosition { get; }

	/// <summary>
	/// Gets the sequence bracket type.
	/// This determines which groups of samples use the same calibration curve.
	/// </summary>
	BracketType Bracket { get; }

	/// <summary>
	/// Gets the set of column names for application specific columns
	/// </summary>
	string[] UserPrivateLabel { get; }

	/// <summary>
	/// Gets a description of the auto-sampler tray
	/// </summary>
	string TrayConfiguration { get; }

	/// <summary>
	/// Gets the user configurable column names
	/// </summary>
	string[] UserLabel { get; }
}
