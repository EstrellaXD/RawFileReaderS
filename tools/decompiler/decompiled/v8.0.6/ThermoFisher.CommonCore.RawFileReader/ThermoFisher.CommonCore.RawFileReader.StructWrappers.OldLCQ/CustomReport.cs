using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// Defines a custom report, as in an Xcalibur PMD file
/// </summary>
internal class CustomReport : IRawObjectBase
{
	/// <summary>
	/// Determines when a report is active
	/// </summary>
	private enum ConditionCode
	{
		/// <summary>
		/// Never make report.
		/// </summary>
		Never,
		/// <summary>
		/// Always make report.
		/// </summary>
		Always,
		/// <summary>
		/// Report when blank sample.
		/// </summary>
		Blank,
		/// <summary>
		/// Report when calibration sample.
		/// </summary>
		Calibration,
		/// <summary>
		/// Report when unknown sample.
		/// </summary>
		Unknown,
		/// <summary>
		/// Report when QC or standard.
		/// </summary>
		QcStandard
	}

	/// <summary>
	/// The action code.
	/// </summary>
	private enum ActionCode
	{
		/// <summary>
		/// Do nothing.
		/// </summary>
		DoNothing,
		/// <summary>
		/// Run an excel macro.
		/// </summary>
		RunExcelMacro,
		/// <summary>
		/// Run a program.
		/// </summary>
		RunProgram,
		/// <summary>
		/// Export only.
		/// </summary>
		ExportOnly
	}

	/// <summary>
	/// The report export types.
	/// </summary>
	private enum ExportTypes
	{
		/// <summary>
		/// No export.
		/// </summary>
		Nothing,
		/// <summary>
		/// Export as XLS.
		/// </summary>
		Xls,
		/// <summary>
		/// Export as text.
		/// </summary>
		Txt,
		/// <summary>
		/// Export as CSV.
		/// </summary>
		Csv
	}

	/// <summary>
	/// The export content.
	/// </summary>
	private enum ExportContent
	{
		/// <summary>
		/// Do not export
		/// </summary>
		Nothing,
		/// <summary>
		/// Export the results.
		/// </summary>
		Results,
		/// <summary>
		/// Export the quantitation.
		/// </summary>
		Quantitation
	}

	/// <summary>
	/// The custom report info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct CustomReportInfo
	{
		public ConditionCode Condition;

		public ActionCode Action;

		public ExportTypes Export;

		public ExportContent ExportContent;
	}

	private static readonly int[,] MarshalledSizes = new int[1, 2] { 
	{
		0,
		Marshal.SizeOf(typeof(CustomReportInfo))
	} };

	/// <summary>
	/// Load data
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="dataOffset">
	/// The data offset into the map.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int64" />.
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		Utilities.ReadStructure<CustomReportInfo>(viewer, ref startPos, fileRevision, MarshalledSizes);
		return startPos - dataOffset;
	}
}
