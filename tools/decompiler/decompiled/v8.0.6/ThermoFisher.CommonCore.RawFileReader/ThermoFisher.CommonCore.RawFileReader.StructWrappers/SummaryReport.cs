using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The summary report, as contained in Xcalibur PMD file
/// </summary>
internal class SummaryReport : IRawObjectBase, IXcaliburReportAccess
{
	/// <summary>
	/// The summary report template info.
	/// Defines the name of a report, and file format type of the report.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct SummaryReportTemplateInfo
	{
		public bool Enabled;

		public ReportTemplateType SaveAsType;
	}

	private static readonly int[,] MarshalledSizes = new int[1, 2] { 
	{
		0,
		Marshal.SizeOf(typeof(SummaryReportTemplateInfo))
	} };

	private SummaryReportTemplateInfo _info;

	/// <summary>
	/// Gets or sets the name of the report
	/// </summary>
	public string ReportName { get; set; }

	/// <summary>
	/// Gets a value indicating whether report is enabled
	/// </summary>
	public bool Enabled => _info.Enabled;

	/// <summary>
	/// Gets the file save format of the report
	/// </summary>
	public ReportTemplateType SaveAsType => _info.SaveAsType;

	/// <summary>
	/// Load report data
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="dataOffset">
	/// The data offset.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The length of data loaded
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		_info = Utilities.ReadStructure<SummaryReportTemplateInfo>(viewer, ref startPos, fileRevision, MarshalledSizes);
		ReportName = viewer.ReadStringExt(ref startPos);
		return startPos - dataOffset;
	}
}
