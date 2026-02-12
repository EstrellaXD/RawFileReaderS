using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// Represent a "program" to be called (as part of reporting) from an Xcalibur PMD file
/// </summary>
internal class Program : IRawObjectBase, IXcaliburProgramAccess, IXcaliburReportSampleTypes
{
	/// <summary>
	/// The program info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct ProgramInfo
	{
		public bool Enabled;

		public bool Stds;

		public bool QCs;

		public bool Unknowns;

		public bool Other;

		public bool Sync;

		public ProgramAction Action;

		public ProgramExportType ExportType;
	}

	private static readonly int[,] MarshalledSizes = new int[1, 2] { 
	{
		0,
		Marshal.SizeOf(typeof(ProgramInfo))
	} };

	private ProgramInfo _info;

	/// <summary>
	/// Gets the name of the program
	/// </summary>
	public string ProgramName { get; private set; }

	/// <summary>
	/// Gets parameters to the program
	/// </summary>
	public string Parameters { get; private set; }

	/// <summary>
	/// Gets the action of this program (such as run exe, or export)
	/// </summary>
	public ProgramAction Action => _info.Action;

	/// <summary>
	/// Gets the file save format of the export
	/// </summary>
	public ProgramExportType ExportType => _info.ExportType;

	/// <summary>
	/// Gets a value indicating whether report is enabled
	/// </summary>
	public bool Enabled => _info.Enabled;

	/// <summary>
	/// Gets a value indicating whether report is enabled for standards
	/// </summary>
	public bool Standards => _info.Stds;

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
	/// Gets a value indicating whether to synchronize this action.
	/// If false, other programs may be run in parallel with this.
	/// </summary>
	public bool Synchronize => _info.Sync;

	/// <summary>
	/// Load data (from file).
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
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		_info = Utilities.ReadStructure<ProgramInfo>(viewer, ref startPos, fileRevision, MarshalledSizes);
		ProgramName = viewer.ReadStringExt(ref startPos);
		Parameters = viewer.ReadStringExt(ref startPos);
		return startPos - dataOffset;
	}
}
