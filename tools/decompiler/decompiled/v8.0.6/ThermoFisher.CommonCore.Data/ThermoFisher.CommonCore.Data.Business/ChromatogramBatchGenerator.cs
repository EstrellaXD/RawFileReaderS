using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The chromatogram batch generator.
/// This provides any number of chromatograms to a caller,
/// using multiple threads.
/// </summary>
public class ChromatogramBatchGenerator : IChromatogramBatchGenerator
{
	/// <summary>
	/// The work item.
	/// This consists of:
	/// The requested chromatogram.
	/// The set of scans needed to make a chromatogram.
	/// </summary>
	private class WorkItem
	{
		/// <summary>
		/// Gets the fragment.
		/// </summary>
		public ChromatogramFragmentRequest Fragment { get; private set; }

		/// <summary>
		/// Gets or sets the scans.
		/// </summary>
		private ScanAndIndex[] Scans { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramBatchGenerator.WorkItem" /> class. 
		/// Create a work item, ready to make chromatograms from scans
		/// </summary>
		/// <param name="buffer">
		/// The tool to read scans
		/// </param>
		/// <param name="request">
		/// The parameters for the chromatogram chunk
		/// </param>
		public WorkItem(SimpleScanWindow buffer, ChromatogramFragmentRequest request)
		{
			if (!request.Empty)
			{
				Scans = buffer.ReturnScansWithin(request.StartIndex, request.EndIndex);
			}
			Fragment = request;
		}

		/// <summary>
		/// Process this work
		/// </summary>
		public void Process()
		{
			Fragment.ProcessScans(Scans);
		}
	}

	/// <summary>
	/// The minimum permitted number of threads to work on generating chromatograms.
	/// </summary>
	public const int MinConsumerThreads = 2;

	private int _consumerThreads;

	private List<Task> _taskList = new List<Task>();

	private int _activeWorkers;

	/// <summary>
	/// Gets or sets the available scans.
	/// </summary>
	public IList<ISimpleScanHeader> AvailableScans { get; set; }

	/// <summary>
	/// Gets or sets the scan reader.
	/// Given a scan number,
	/// Return the spectral data and scan event
	/// </summary>
	public Func<ISimpleScanHeader, SimpleScanWithHeader> ScanReader { get; set; }

	/// <summary>
	/// Gets or sets the scan reader, for multiple scans
	/// Given a scan number array,
	/// Return the spectral data and scan event.
	/// Note: May be null, in which case "ScanReader" will be used.
	/// </summary>
	public Func<ISimpleScanHeader[], SimpleScanWithHeader[]> ParallelScanReader { get; set; }

	/// <summary>
	/// Gets or sets the scans in chromatogram batch.
	/// An optimal value will depend on the experiment type.
	/// For example: for SRM, with 20 or less centroid peaks per scan, this can be larger.
	/// For full scan experiments, with 500 or more centroids per scan, this may need to be smaller, especially on
	/// a 32 bit process.
	/// Default 200 for 32 bit, 800 for 64 bit. Suggested range 100 to 10000.
	/// Setting this larger may increase efficiency (fewer small tasks to perform, in raw data with 100k scans),
	/// but is at the cost of possible increased memory usage for scans.
	/// This is intentionally "short". Values above 10,000 for this are discouraged, as that could
	/// lead to significant memory overheads, and possibly less efficient thread management.
	/// </summary>
	public int ScansInChromatogramBatch { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the chromatograms have a strict time range.
	/// By default, the retention time range of a chromatogram is considered as a "display range",
	/// such that the first value before the range, and the first value after the range is included
	/// in the data, permitting a continuous line, if plotted, to the edge of the time window.
	/// If this property is true, then only points which are within the supplied RT range are returned.
	/// </summary>
	public bool StrictTimeRange { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether blocks of "future scans" get read
	/// The use of this should be optimized based on the reading technolgy.
	/// Although parallel scan readers can be used to read and process multiple scans,
	/// this may not meet the needs of "large batch readers" (su as S3?) where
	/// loading the data for the scans (from disk etc) may take most of the time.
	/// Setting this mode will cause async calls to the scan reader.
	/// Where date can be read quickly (such as data is already in a memory array)
	/// ths mode could simpy increase memory overhgeads. False by default.
	/// </summary>
	public bool ReadAhead { get; set; }

	/// <summary>
	/// Gets or sets the maximum number of consumer threads to make.
	/// In other words: The maximum number of chromatograms the caller will
	/// be given to process in parallel.
	/// Default: Environment.ProcessorCount.
	/// Minimum : MinConsumerThreads
	/// </summary>
	public int ConsumerThreads
	{
		get
		{
			return _consumerThreads;
		}
		set
		{
			_consumerThreads = Math.Max(2, value);
		}
	}

	/// <summary>
	/// Gets or sets the maximum work backlog, which controls how much
	/// work is kept in the pipeline, waiting for threads to become available.
	/// Default: 5 for 32 bit process, 20 for 64 bit.
	/// Setting a larger value may cause data reading from a raw file to be completed earlier,
	/// but at the expense of more memory overheads.
	/// </summary>
	public int MaxWorkBacklog { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether accurate precursor mass testing is done.
	/// If not: a tolerance of 0.4 AMU is used.
	/// All data dependent tests are done with 0.4 AMU tolerance.
	/// </summary>
	public bool AccuratePrecursors { get; set; }

	/// <summary>
	/// Gets or sets the precision (decimal places)
	/// </summary>
	public int FilterMassPrecision { get; set; }

	/// <summary>
	/// Gets or sets the scan events interface, which can be used to optimize filtering, if provided.
	/// This interface may be obtained from IRawDataPlus (ScanEvents property).
	/// </summary>
	public IScanEvents AllEvents { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramBatchGenerator" /> class.
	/// </summary>
	public ChromatogramBatchGenerator()
	{
		FilterMassPrecision = -1;
		bool is64BitProcess = Environment.Is64BitProcess;
		ScansInChromatogramBatch = (short)(is64BitProcess ? 800 : 200);
		ConsumerThreads = Environment.ProcessorCount;
		MaxWorkBacklog = (is64BitProcess ? 20 : 5);
	}

	/// <summary>
	/// Generate chromatograms, returning the in progress tasks,
	/// which are processing the chromatograms.
	/// This permits async generation of chromatograms.
	/// This method returns after all required scan data has been
	/// read, and all work to process the chromatograms is queued.
	/// (The implied raw data file can be closed, as ScanReader will not be called again).
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
	public Task[] GenerateChromatograms(IEnumerable<IChromatogramDelivery> chromatogramDeliveries)
	{
		if (chromatogramDeliveries == null)
		{
			throw new ArgumentNullException("chromatogramDeliveries");
		}
		if (AvailableScans == null || ScanReader == null)
		{
			throw new NoNullAllowedException("Properties must not be null");
		}
		ChromatogramFragmentRequest[] orderedByStartScan = CreateOrderedChromatograms(chromatogramDeliveries);
		BlockingCollection<List<WorkItem>> blockingCollection = new BlockingCollection<List<WorkItem>>(MaxWorkBacklog);
		_taskList = new List<Task>();
		_activeWorkers = 0;
		CreateWorkItems(orderedByStartScan, blockingCollection);
		blockingCollection.CompleteAdding();
		return _taskList.ToArray();
	}

	/// <summary>
	/// Process all items in a work list.
	/// </summary>
	/// <param name="workItemList">
	/// The work item list.
	/// </param>
	private static void ProcessWorkList(List<WorkItem> workItemList)
	{
		foreach (WorkItem workItem in workItemList)
		{
			workItem.Process();
		}
	}

	/// <summary>
	/// Add work to the work list.
	/// If there are insufficient background workers, assign more.
	/// </summary>
	/// <param name="work">Collection of work</param>
	/// <param name="item">New job to add</param>
	private void AddWork(BlockingCollection<List<WorkItem>> work, List<WorkItem> item)
	{
		work.Add(item);
		if (_taskList.Count < 2)
		{
			AddWorker(work);
		}
		else if (_taskList.Count < ConsumerThreads && work.Count > 1)
		{
			AddWorker(work);
		}
	}

	/// <summary>
	/// Add a worker thread.
	/// </summary>
	/// <param name="work">
	/// The work.
	/// </param>
	private void AddWorker(BlockingCollection<List<WorkItem>> work)
	{
		_taskList.Add(Task.Factory.StartNew(delegate
		{
			GenerateChromatogramChunks(work);
		}));
		Interlocked.Increment(ref _activeWorkers);
	}

	/// <summary>
	/// Create partial chromatogram definitions, ordered by start scan.
	/// </summary>
	/// <param name="chromatogramDeliveries">requested chromatograms</param>
	/// <returns>Ordered partial chromatograms</returns>
	private ChromatogramFragmentRequest[] CreateOrderedChromatograms(IEnumerable<IChromatogramDelivery> chromatogramDeliveries)
	{
		return new ChromatogramFragmentCreator
		{
			AvailableScans = AvailableScans,
			StrictTimeRange = StrictTimeRange,
			ScansInChromatogramBatch = ScansInChromatogramBatch,
			AccuratePrecursors = AccuratePrecursors,
			FilterMassPrecision = FilterMassPrecision,
			AllEvents = AllEvents
		}.CreateOrderedChromatograms(chromatogramDeliveries).ToArray();
	}

	/// <summary>
	/// Create work items.
	/// </summary>
	/// <param name="orderedByStartScan">
	/// The work request list, ordered by start scan.
	/// </param>
	/// <param name="work">
	/// The target work list.
	/// </param>
	private void CreateWorkItems(ChromatogramFragmentRequest[] orderedByStartScan, BlockingCollection<List<WorkItem>> work)
	{
		using SimpleScanWindow simpleScanWindow = new SimpleScanWindow(ScanReader, ParallelScanReader, AvailableScans)
		{
			Lookahead = ScansInChromatogramBatch / 2
		};
		List<WorkItem> list = null;
		int num = 0;
		ChromatogramFragmentRequest[] array;
		if (ReadAhead)
		{
			array = orderedByStartScan;
			foreach (ChromatogramFragmentRequest chromatogramFragmentRequest in array)
			{
				if (!chromatogramFragmentRequest.Empty)
				{
					simpleScanWindow.PlanScansWithin(chromatogramFragmentRequest.StartIndex, chromatogramFragmentRequest.EndIndex);
				}
			}
		}
		array = orderedByStartScan;
		foreach (ChromatogramFragmentRequest chromatogramFragmentRequest2 in array)
		{
			WorkItem item = new WorkItem(simpleScanWindow, chromatogramFragmentRequest2);
			if (list != null)
			{
				if (chromatogramFragmentRequest2.StartIndex <= num)
				{
					list.Add(item);
				}
				else
				{
					AddWork(work, list);
					list = null;
				}
			}
			if (list == null)
			{
				list = new List<WorkItem> { item };
				num = chromatogramFragmentRequest2.StartIndex + ScansInChromatogramBatch / 2;
			}
		}
		if (list != null)
		{
			PublishPendingList(list, work);
		}
	}

	/// <summary>
	/// Publish pending list of work, which has the same scan range
	/// </summary>
	/// <param name="pendingItemList">
	/// The pending item list.
	/// </param>
	/// <param name="work">
	/// The work.
	/// </param>
	private void PublishPendingList(List<WorkItem> pendingItemList, BlockingCollection<List<WorkItem>> work)
	{
		int count = pendingItemList.Count;
		List<WorkItem> list = new List<WorkItem>(count);
		int num = 0;
		foreach (WorkItem pendingItem in pendingItemList)
		{
			if (pendingItem.Fragment.IsFinal)
			{
				num++;
			}
		}
		int num2 = num / ConsumerThreads;
		int num3 = 0;
		foreach (WorkItem pendingItem2 in pendingItemList)
		{
			list.Add(pendingItem2);
			if (pendingItem2.Fragment.IsFinal && ++num3 >= num2)
			{
				AddWork(work, list);
				list = new List<WorkItem>(count);
				num3 = 0;
			}
		}
		if (list.Count > 0)
		{
			AddWork(work, list);
		}
	}

	/// <summary>
	/// Generate the chromatogram chunks, for all work items that this thread sees.
	/// </summary>
	/// <param name="work">
	/// The work items.
	/// </param>
	private void GenerateChromatogramChunks(BlockingCollection<List<WorkItem>> work)
	{
		int num = 0;
		bool flag = false;
		while (!work.IsCompleted)
		{
			if (work.TryTake(out var item))
			{
				flag = true;
				ProcessWorkList(item);
			}
			if (_activeWorkers > 2)
			{
				if (num++ > 10)
				{
					break;
				}
				if (work.TryTake(out item, 5))
				{
					flag = true;
					ProcessWorkList(item);
				}
			}
			else if (flag)
			{
				Thread.Sleep(0);
				flag = false;
			}
			else
			{
				Thread.Sleep(work.IsAddingCompleted ? 2 : 5);
				flag = true;
			}
		}
		Interlocked.Decrement(ref _activeWorkers);
	}
}
