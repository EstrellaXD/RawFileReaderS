using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The chromatogram aggregator.
/// This can join together chromatograms form batches of scans,
/// and return to caller when complete
/// </summary>
public class ChromatogramAggregator
{
	private readonly Action<ChromatogramSignal> _deliveryAction;

	private readonly IList<ISimpleScanHeader> _scans;

	private ISignalConvert[] _resultsTable;

	private int _resultsDelivered;

	/// <summary>
	/// Gets or sets the number of batches of scans, that sub-divide this chromatogram.
	/// </summary>
	public int Batches { get; set; }

	/// <summary>
	/// Gets the generator.
	/// </summary>
	public IChromatogramGenerator Generator { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramAggregator" /> class.
	/// </summary>
	/// <param name="generator">
	/// The tool to create chromatograms.
	/// </param>
	/// <param name="availableScans">The scan numbers and retention times</param>
	/// <param name="deliveryAction">Method called when the chromatogram is complete</param>
	public ChromatogramAggregator(IChromatogramGenerator generator, IList<ISimpleScanHeader> availableScans, Action<ChromatogramSignal> deliveryAction)
	{
		_deliveryAction = deliveryAction;
		_scans = availableScans;
		Generator = generator;
	}

	/// <summary>
	/// Get the aggregator ready to hold result for "Batches" of data
	/// </summary>
	public void Initialize()
	{
		_resultsTable = new ISignalConvert[Batches];
		_resultsDelivered = 0;
	}

	/// <summary>
	/// Called when a signal is ready.
	/// That is: A set of scans has been combined to make a chromatogram.
	/// When the final batch is delivered, the chromatogram is sent to the client.
	/// </summary>
	/// <param name="batch">
	/// The batch number (which part of the overall chromatogram is this?).
	/// </param>
	/// <param name="signal">
	/// The signal.
	/// </param>
	public void SignalReady(int batch, ISignalConvert signal)
	{
		_resultsTable[batch] = signal;
		if (Interlocked.Increment(ref _resultsDelivered) == Batches)
		{
			_deliveryAction(Collate());
		}
	}

	/// <summary>
	/// Collate the signals
	/// </summary>
	/// <returns>
	/// The joined signals
	/// </returns>
	private ChromatogramSignal Collate()
	{
		ChromatogramSignal[] convertedResults = new ChromatogramSignal[_resultsTable.Length];
		int[] offsets = new int[_resultsTable.Length];
		int num = 0;
		for (int i = 0; i < _resultsTable.Length; i++)
		{
			offsets[i] = num;
			ISignalConvert signalConvert = _resultsTable[i];
			if (signalConvert != null)
			{
				num += signalConvert.Length;
			}
		}
		double[] time = new double[num];
		double[] intensity = new double[num];
		int[] scan = new int[num];
		Parallel.For(0, _resultsTable.Length, delegate(int num2)
		{
			ISignalConvert signalConvert2 = _resultsTable[num2];
			ChromatogramSignal chromatogramSignal = ((signalConvert2 != null) ? signalConvert2.ToSignal(_scans) : ChromatogramSignal.FromTimeIntensityScan(new double[0], new double[0], new int[0]));
			convertedResults[num2] = chromatogramSignal;
			int length = chromatogramSignal.Length;
			int destinationIndex = offsets[num2];
			Array.Copy(chromatogramSignal.SignalTimes, 0, time, destinationIndex, length);
			Array.Copy(chromatogramSignal.SignalIntensities, 0, intensity, destinationIndex, length);
			Array.Copy(chromatogramSignal.SignalScans, 0, scan, destinationIndex, length);
		});
		return ChromatogramSignal.FromTimeIntensityScan(time, intensity, scan);
	}
}
