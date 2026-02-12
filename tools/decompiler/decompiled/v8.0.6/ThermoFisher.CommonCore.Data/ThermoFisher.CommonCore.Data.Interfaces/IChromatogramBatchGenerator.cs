using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The ChromatogramBatchGenerator interface.
/// Defines a way for an application to request multiple chromatograms.
/// </summary>
public interface IChromatogramBatchGenerator
{
	/// <summary>
	/// Gets or sets a value indicating whether the chromatograms have a strict time range.
	/// By default, the retention time range of a chromatogram is considered as a "display range",
	/// such that the first value before the range, and the first value after the range is included
	/// in the data, permitting a continuous line, if plotted, to the edge of the time window.
	/// If this property is true, then only points which are within the supplied RT range are returned.
	/// </summary>
	bool StrictTimeRange { get; set; }

	/// <summary>
	/// Gets or sets the number of consumer threads to make.
	/// In other words: The maximum number of chromatograms the caller will
	/// be given to process in parallel.
	/// Default: Environment.ProcessorCount
	/// </summary>
	int ConsumerThreads { get; set; }

	/// <summary>
	/// Gets or sets the maximum work backlog, which controls how much
	/// work is kept in the pipeline, waiting for consumer threads to become available.
	/// Suggested default: 10.
	/// Setting a larger value will cause data reading from a raw file to be completed earlier,
	/// but at the expense of more memory overheads.
	/// </summary>
	int MaxWorkBacklog { get; set; }

	/// <summary>
	/// Gets or sets the scan reader.
	/// Given a scan number,
	/// Return the spectral data and scan event
	/// </summary>
	Func<ISimpleScanHeader, SimpleScanWithHeader> ScanReader { get; set; }

	/// <summary>
	/// Gets or sets the parallel scan reader.
	/// If null, then ScanReader will always be used.
	/// Given a scan number,
	/// Return the spectral data and scan event
	/// </summary>
	Func<ISimpleScanHeader[], SimpleScanWithHeader[]> ParallelScanReader { get; set; }

	/// <summary>
	/// Gets or sets the available scans.
	/// </summary>
	IList<ISimpleScanHeader> AvailableScans { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether accurate precursor mass testing is done.
	/// If not: a tolerance of 0.4 AMU is used.
	/// All data dependent tests are done with 0.4 AMU tolerance.
	/// </summary>
	bool AccuratePrecursors { get; set; }

	/// <summary>
	/// Gets or sets the precision (decimal places).
	/// If set to "-1" (default) then the precision values must be
	/// set individually (in the scan selectors)
	/// </summary>
	int FilterMassPrecision { get; set; }

	/// <summary>
	/// Gets or sets the scan events interface, which can be used to optimize filtering, if provided.
	/// This interface may be obtained from IRawDataPlus (ScanEvents property).
	/// </summary>
	IScanEvents AllEvents { get; set; }

	/// <summary>
	/// Gets or sets the number of scans to process in one worker.
	/// </summary>
	int ScansInChromatogramBatch { get; set; }

	/// <summary>
	/// Gets or sets an option to request bufers of scans ahead of time (async)
	/// This may be of use with data readers that must make large block reads.
	/// </summary>
	bool ReadAhead { get; set; }

	/// <summary>
	/// Generate chromatograms, returning the in progress tasks,
	/// which are processing the chromatograms.
	/// This permits async generation of chromatograms.
	/// This method returns after all required scan data has been
	/// read, and all work to process the chromatograms is queued.
	/// </summary>
	/// <param name="chromatogramDeliveries">
	/// The chromatogram deliveries.
	/// These define the chromatogram settings, and a callback which will occur
	/// as soon as the data for that chromatogram is ready.
	/// </param>
	/// <returns>
	/// The in progress tasks. Assuming the result is saved as "taskList" Use "Task.WaitAll(taskList)"
	/// to wait for processing of these chromatograms to complete.
	/// </returns>
	Task[] GenerateChromatograms(IEnumerable<IChromatogramDelivery> chromatogramDeliveries);
}
