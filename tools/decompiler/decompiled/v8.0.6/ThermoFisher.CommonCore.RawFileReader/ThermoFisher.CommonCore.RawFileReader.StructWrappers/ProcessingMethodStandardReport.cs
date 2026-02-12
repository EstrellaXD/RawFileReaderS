using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// Class to import PMD file "standard reports"
/// </summary>
internal class ProcessingMethodStandardReport : IProcessingMethodStandardReportAccess, IRawObjectBase
{
	/// <summary>
	/// The standard report info version 1.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct StandardReportInfoVersion1
	{
		public bool AnalysisUnknown;

		public bool ComponentUnknown;

		public bool MethodUnknown;

		public bool LogUnknown;

		public bool AnalysisCalibration;

		public bool ComponentCalibration;

		public bool MethodCalibration;

		public bool LogCalibration;

		public bool AnalysisQC;

		public bool ComponentQC;

		public bool MethodQC;

		public bool LogQC;

		public bool AnalysisOther;

		public bool ComponentOther;

		public bool MethodOther;

		public bool LogOther;

		public bool SampleInformation;

		public bool RunInformation;

		public bool Chromatogram;

		public bool PeakComponent;

		public bool Tune;

		public bool Experiment;

		public bool Processing;

		public bool Status;

		public bool Error;

		public bool Audit;

		public bool OpenAccess;
	}

	/// <summary>
	/// Current structure for standard report data
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct StandardReportInfo
	{
		public bool AnalysisUnknown;

		public bool ComponentUnknown;

		public bool MethodUnknown;

		public bool LogUnknown;

		public bool AnalysisCalibration;

		public bool ComponentCalibration;

		public bool MethodCalibration;

		public bool LogCalibration;

		public bool AnalysisQC;

		public bool ComponentQC;

		public bool MethodQC;

		public bool LogQC;

		public bool AnalysisOther;

		public bool ComponentOther;

		public bool MethodOther;

		public bool LogOther;

		public bool SampleInformation;

		public bool RunInformation;

		public bool Chromatogram;

		public bool PeakComponent;

		public bool Tune;

		public bool Experiment;

		public bool Processing;

		public bool Status;

		public bool Error;

		public bool Audit;

		public bool OpenAccess;

		public ChroAnalysisReport ChroAnalysisReport;

		public bool Survey;

		public bool PrintSignatureLine;
	}

	private StandardReportInfo _reports;

	private const int SizeCurrentIndex = 0;

	private const int SizeOldIndex = 1;

	private static readonly int[] MarshalledSizes = new int[2]
	{
		Marshal.SizeOf(typeof(StandardReportInfo)),
		Marshal.SizeOf(typeof(StandardReportInfoVersion1))
	};

	/// <summary>
	/// Gets a value indicating whether the Analysis Unknown report is needed
	/// </summary>
	public bool AnalysisUnknown => _reports.AnalysisUnknown;

	/// <summary>
	/// Gets a value indicating whether the Component Unknown report is needed
	/// </summary>
	public bool ComponentUnknown => _reports.ComponentUnknown;

	/// <summary>
	/// Gets a value indicating whether the Method Unknown report is needed
	/// </summary>
	public bool MethodUnknown => _reports.MethodUnknown;

	/// <summary>
	/// Gets a value indicating whether the Log Unknown report is needed
	/// </summary>
	public bool LogUnknown => _reports.LogUnknown;

	/// <summary>
	/// Gets a value indicating whether the Analysis Calibration report is needed
	/// </summary>
	public bool AnalysisCalibration => _reports.AnalysisCalibration;

	/// <summary>
	/// Gets a value indicating whether the Component Calibration report is needed
	/// </summary>
	public bool ComponentCalibration => _reports.ComponentCalibration;

	/// <summary>
	/// Gets a value indicating whether the Method Calibration report is needed
	/// </summary>
	public bool MethodCalibration => _reports.MethodCalibration;

	/// <summary>
	/// Gets a value indicating whether the Log Calibration report is needed
	/// </summary>
	public bool LogCalibration => _reports.LogCalibration;

	/// <summary>
	/// Gets a value indicating whether the Analysis QC report is needed
	/// </summary>
	public bool AnalysisQc => _reports.AnalysisQC;

	/// <summary>
	/// Gets a value indicating whether the Component QC report is needed
	/// </summary>
	public bool ComponentQc => _reports.ComponentQC;

	/// <summary>
	/// Gets a value indicating whether the Method QC report is needed
	/// </summary>
	public bool MethodQc => _reports.MethodQC;

	/// <summary>
	/// Gets a value indicating whether the Log QC report is needed
	/// </summary>
	public bool LogQc => _reports.LogQC;

	/// <summary>
	/// Gets a value indicating whether the Analysis Other report is needed
	/// </summary>
	public bool AnalysisOther => _reports.AnalysisOther;

	/// <summary>
	/// Gets a value indicating whether the Component Other report is needed
	/// </summary>
	public bool ComponentOther => _reports.ComponentOther;

	/// <summary>
	/// Gets a value indicating whether the Method Other report is needed
	/// </summary>
	public bool MethodOther => _reports.MethodOther;

	/// <summary>
	/// Gets a value indicating whether the Log Other report is needed
	/// </summary>
	public bool LogOther => _reports.LogOther;

	/// <summary>
	/// Gets a value indicating whether the Sample Information report is needed
	/// </summary>
	public bool SampleInformation => _reports.SampleInformation;

	/// <summary>
	/// Gets a value indicating whether the Run Information report is needed
	/// </summary>
	public bool RunInformation => _reports.RunInformation;

	/// <summary>
	/// Gets a value indicating whether the Chromatogram report is needed
	/// </summary>
	public bool Chromatogram => _reports.Chromatogram;

	/// <summary>
	/// Gets a value indicating whether the PeakComponent report is needed
	/// </summary>
	public bool PeakComponent => _reports.PeakComponent;

	/// <summary>
	/// Gets a value indicating whether the Tune report is needed
	/// </summary>
	public bool Tune => _reports.Tune;

	/// <summary>
	/// Gets a value indicating whether the Experiment report is needed
	/// </summary>
	public bool Experiment => _reports.Experiment;

	/// <summary>
	/// Gets a value indicating whether the Processing report is needed
	/// </summary>
	public bool Processing => _reports.Processing;

	/// <summary>
	/// Gets a value indicating whether the Status report is needed
	/// </summary>
	public bool Status => _reports.Status;

	/// <summary>
	/// Gets a value indicating whether the Error report is needed
	/// </summary>
	public bool Error => _reports.Error;

	/// <summary>
	/// Gets a value indicating whether the Audit report is needed
	/// </summary>
	public bool Audit => _reports.Audit;

	/// <summary>
	/// Gets a value indicating whether the Open Access report is needed
	/// </summary>
	public bool OpenAccess => _reports.OpenAccess;

	/// <summary>
	/// Gets a value indicating which of the two types of chromatogram analysis report is needed.
	/// </summary>
	public ChroAnalysisReport ChroAnalysisReport => _reports.ChroAnalysisReport;

	/// <summary>
	/// Gets a value indicating whether the Survey report is needed
	/// </summary>
	public bool Survey => _reports.Survey;

	/// <summary>
	/// Gets a value indicating whether to include a signature line in reports
	/// </summary>
	public bool PrintSignatureLine => _reports.PrintSignatureLine;

	/// <summary>
	/// Load (from file)
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
		int num = MarshalledSizes[0];
		byte[] array;
		if (fileRevision >= 11)
		{
			array = viewer.ReadBytesExt(ref startPos, num);
		}
		else
		{
			array = new byte[num];
			int count = MarshalledSizes[1];
			Buffer.BlockCopy(viewer.ReadBytesExt(ref startPos, count), 0, array, 0, count);
		}
		GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
		_reports = (StandardReportInfo)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(StandardReportInfo));
		gCHandle.Free();
		if (fileRevision >= 2 && fileRevision < 11)
		{
			int chroAnalysisReport = viewer.ReadIntExt(ref startPos);
			_reports.ChroAnalysisReport = (ChroAnalysisReport)chroAnalysisReport;
		}
		return startPos - dataOffset;
	}
}
