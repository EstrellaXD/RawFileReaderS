using System;
using System.Collections.Generic;
using System.Linq;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Class to analyze chromatogram requests, creating an ordered table
/// of partial chromatograms, which can be used to optimize data reading for
/// chromatogram generation.
/// </summary>
public class ChromatogramFragmentCreator : ChromatogramBoundsFinder
{
	/// <summary>
	/// Gets or sets the scans in chromatogram batch.
	/// An optimal value will depend on the experiment type.
	/// For example: for SRM, with 20 or less centroid peaks per scan, this can be larger.
	/// For full scan experiments, with 500 or more centroids per scan, this may need to be smaller, especially on
	/// a 32 bit process.
	/// Default 400 for 32 bit, 1600 for 64 bit. Suggested range 100 to 3000.
	/// Setting this larger may increase efficiency (fewer small tasks to perform, in raw data with 100k scans),
	/// but is at the cost of possible increased memory usage for scans.
	/// This is intentionally "short". Values above 10,000 for this are discouraged, as that could
	/// lead to significant memory overheads, and possibly less efficient thread management.
	/// </summary>
	public int ScansInChromatogramBatch { get; set; }

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
	/// Create ordered chromatograms.
	/// Creates a "time ordered" set of partial chromatogram requests,
	/// which can then be used for efficient chromatogram generation,
	/// by making a single pass over raw data.
	/// </summary>
	/// <param name="chromatogramDeliveries">
	/// The chromatograms to be delivered.
	/// </param>
	/// <returns>
	/// The ordered set of chromatogram requests.
	/// </returns>
	public IOrderedEnumerable<ChromatogramFragmentRequest> CreateOrderedChromatograms(IEnumerable<IChromatogramDelivery> chromatogramDeliveries)
	{
		if (chromatogramDeliveries == null)
		{
			throw new ArgumentNullException("chromatogramDeliveries");
		}
		List<ChromatogramFragmentRequest> list = new List<ChromatogramFragmentRequest>();
		foreach (IChromatogramDelivery chromatogramDelivery in chromatogramDeliveries)
		{
			CreateRequestsForOneChromatogram(list, chromatogramDelivery);
		}
		return from request in list
			orderby request.StartIndex, request.EndIndex
			select request;
	}

	/// <summary>
	/// Create requests for one chromatogram.
	/// </summary>
	/// <param name="requests">
	/// The requests.
	/// </param>
	/// <param name="delivery">
	/// The delivery.
	/// </param>
	private void CreateRequestsForOneChromatogram(List<ChromatogramFragmentRequest> requests, IChromatogramDelivery delivery)
	{
		ChromatogramAggregator chromatogramAggregator = new ChromatogramAggregator(ChromatogramGeneratorFactory.CreateChromatogramGenerator(delivery.Request, AccuratePrecursors, FilterMassPrecision, AllEvents), base.AvailableScans, delivery.Process);
		Tuple<int, int> tuple = FindIndexRangeForChromatogram(delivery.Request.RetentionTimeRange);
		int num = tuple.Item1;
		int item = tuple.Item2;
		if (num >= base.AvailableScans.Count || item < num)
		{
			requests.Add(new ChromatogramFragmentRequest
			{
				Empty = true,
				Aggregator = chromatogramAggregator,
				Batch = 0
			});
		}
		else
		{
			int num2 = ScansInChromatogramBatch * 3 / 2;
			int num3 = item - num + 1;
			while (num3 > num2)
			{
				AddBatch(requests, chromatogramAggregator, num, num + ScansInChromatogramBatch - 1, isFinal: false);
				num3 -= ScansInChromatogramBatch;
				num += ScansInChromatogramBatch;
			}
			AddBatch(requests, chromatogramAggregator, num, item, isFinal: true);
		}
		chromatogramAggregator.Initialize();
	}

	/// <summary>
	/// Add a batch of scans.
	/// </summary>
	/// <param name="requests">
	///     The requests.
	/// </param>
	/// <param name="aggregator">
	///     The aggregator.
	/// </param>
	/// <param name="startIndex">
	///     The start scan index.
	/// </param>
	/// <param name="endIndex">
	///     The end scan index.
	/// </param>
	/// <param name="isFinal">Set if this is the final batch for a chromatogram</param>
	private void AddBatch(List<ChromatogramFragmentRequest> requests, ChromatogramAggregator aggregator, int startIndex, int endIndex, bool isFinal)
	{
		requests.Add(new ChromatogramFragmentRequest
		{
			StartIndex = startIndex,
			EndIndex = endIndex,
			Empty = false,
			Aggregator = aggregator,
			Batch = aggregator.Batches,
			IsFinal = isFinal
		});
		aggregator.Batches++;
	}
}
