using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The sample report, from PMD files.
/// </summary>
internal class SampleReport : IXcaliburSampleReportAccess, IXcaliburReportAccess, IXcaliburReportSampleTypes, IRawObjectBase
{
	/// <summary>
	/// The report template info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct ReportTemplateInfo
	{
		public bool Enabled;

		public bool Standards;

		public bool QCs;

		public bool Unknowns;

		public bool Other;

		public ReportTemplateType SaveAsType;
	}

	private static readonly int[,] MarshalledSizes = new int[1, 2] { 
	{
		0,
		Marshal.SizeOf(typeof(ReportTemplateInfo))
	} };

	private ReportTemplateInfo _info;

	/// <summary>
	/// Gets the name of the report
	/// </summary>
	public string ReportName { get; private set; }

	/// <summary>
	/// Gets a value indicating whether report is enabled
	/// </summary>
	public bool Enabled => _info.Enabled;

	/// <summary>
	/// Gets a value indicating whether report is enabled for standards
	/// </summary>
	public bool Standards => _info.Standards;

	/// <summary>
	/// Gets a value indicating whether report is enabled for QCs
	/// </summary>
	public bool Qcs => _info.QCs;

	/// <summary>
	/// Gets a value indicating whether report is enabled for Unknowns
	/// </summary>
	public bool Unknowns => _info.Unknowns;

	/// <summary>
	/// Gets a value indicating whether report is enabled for Other sample types
	/// </summary>
	public bool Other => _info.Other;

	/// <summary>
	/// Gets the file save format of the report
	/// </summary>
	public ReportTemplateType SaveAsType => _info.SaveAsType;

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		_info = Utilities.ReadStructure<ReportTemplateInfo>(viewer, ref startPos, fileRevision, MarshalledSizes);
		ReportName = viewer.ReadStringExt(ref startPos);
		return startPos - dataOffset;
	}
}
