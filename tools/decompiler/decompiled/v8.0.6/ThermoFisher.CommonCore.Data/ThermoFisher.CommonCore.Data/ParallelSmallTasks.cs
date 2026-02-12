using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Run many very small tasks in parallel. 
/// faster than TPL for very granular tasks, such as 2mS to 10mS per task.
/// Avoids overheads of "queuing tasks to workers".
/// Instead: All workers pull from a common queue.
/// The algorithm works by queuing a limited number of worker tasks, which the task pool distributes between
/// worker threads. Each worker task can run until every job is pulled from the queue.
/// The reason this is more efficient than TPL is that TPL will queue a work item for every item in 
/// the work list, causing overheads within queuing work items, and overheads when the work items complete.
/// To use this class, create an array (or List) of objects, each of which is self contained (thread safe) and
/// has an Execute method, which performs the required processing. Pass this to RunInParallel.
/// </summary>
public class ParallelSmallTasks
{
	/// <summary>
	/// Default value for MaxWorkers property
	/// </summary>
	public const int DefaultMaxWorkers = 12;

	private Semaphore _workItemCounter;

	/// <summary>
	/// The number of worker threads.
	/// This is initialized to the total number of workers needed.
	/// Each worker counts it down when completed, and frees the main thread when zero.
	/// </summary>
	private int _workersToComplete;

	private int _nextJobIndex;

	private IList<IExecute> _runners;

	/// <summary>
	/// Gets or sets the maximum number of worker jobs to queue.
	/// This typically does not need to be set (can be left at default).
	/// Setting this to 1 is useful for basic performance tuning, as all work
	/// will be done by the current thread (single threaded).
	/// Other settings could be used for fine tuning and research.
	/// Worker jobs do not imply threads. Each worker is able to process the entire job queue.
	/// The number of threads available is automatically set by the .net thread pool.
	/// Suggested range of this: 10 to 20. Default 12.
	/// </summary>
	public int MaxWorkers { get; set; }

	/// <summary>
	/// Gets or sets the batch size.
	/// When task times are very small (less than 1 mS), it may be more efficient to ensure each worker has more than one task.
	/// By default (BatchSize=0) the class could schedule 12 workers to run 12 tasks (assuming MaxWorkers is still the default, 12).
	/// But: if the tasks are very short, this may add some overheads. Setting "BatchSize" to 2 would limit the number of workers to 6 in this case,
	/// with the Max (12) workers being used when there are 24 or more tasks in the list
	/// </summary>
	public int BatchSize { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to use TPL for the parallel execution.
	/// There can be trade-offs between overheads, such as initialization or thread locking, when comparing
	/// the TPL model and the model within this class. Applications can research which model gives higher performance.
	/// All other settings are ignored if this is set.
	/// </summary>
	public bool UseTpl { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.ParallelSmallTasks" /> class. 
	/// Default constructor
	/// </summary>
	public ParallelSmallTasks()
	{
		MaxWorkers = 12;
	}

	/// <summary>
	/// Run tasks in parallel with default settings
	/// </summary>
	/// <param name="runners">tasks to execute</param>
	public static void ExecuteInParallel(IList<IExecute> runners)
	{
		new ParallelSmallTasks().RunInParallel(runners);
	}

	/// <summary>
	/// Each item in the list has a "Execute" method.
	/// The methods are called on each object using a parallel scheme
	/// which permits each worker thread to act on any item in the list.
	/// The objects in the list should save results of their respective execute methods.
	/// Calling code can then iterate through the runners as soon as this call completes,
	/// and use the generated results within the objects.
	/// </summary>
	/// <param name="runners">
	/// The objects which need to have an "Execute" method called on them.
	/// For example "array of T, where T implements IExecute"/&gt;
	/// </param>
	public void RunInParallel(IList<IExecute> runners)
	{
		if (runners == null)
		{
			throw new ArgumentNullException("runners");
		}
		if (UseTpl)
		{
			Parallel.ForEach(runners, delegate(IExecute runner)
			{
				runner.Execute();
			});
			return;
		}
		int num = ((MaxWorkers >= 0) ? Math.Max(1, MaxWorkers) : 12);
		int num2 = runners.Count;
		if (BatchSize > 1)
		{
			num2 = (num2 + BatchSize - 1) / BatchSize;
		}
		if (num2 > num)
		{
			num2 = num;
		}
		if (num2 <= 1)
		{
			int count = runners.Count;
			for (int num3 = 0; num3 < count; num3++)
			{
				runners[num3].Execute();
			}
			return;
		}
		_nextJobIndex = 0;
		_runners = runners;
		_workersToComplete = num2 - 1;
		using (_workItemCounter = new Semaphore(0, 1))
		{
			for (int num4 = 0; num4 < num2 - 1; num4++)
			{
				ThreadPool.QueueUserWorkItem(JobPoolCallBack);
			}
			ProcessAllAvailableJobs();
			_workItemCounter.WaitOne();
		}
	}

	/// <summary>
	/// Worker method for threads
	/// </summary>
	/// <param name="state">
	/// not used
	/// </param>
	private void JobPoolCallBack(object state)
	{
		ProcessAllAvailableJobs();
		WorkerComplete();
	}

	/// <summary>
	/// The worker complete.
	/// </summary>
	private void WorkerComplete()
	{
		if (Interlocked.Decrement(ref _workersToComplete) <= 0)
		{
			_workItemCounter.Release();
		}
	}

	/// <summary>
	/// The process all available jobs.
	/// </summary>
	private void ProcessAllAvailableJobs()
	{
		while (true)
		{
			int num = Interlocked.Increment(ref _nextJobIndex) - 1;
			if (num < _runners.Count)
			{
				_runners[num].Execute();
				continue;
			}
			break;
		}
	}
}
