using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Factory to create objects which make MS chromatograms in parallel.
/// Chromatograms are delivered on threads, such that action methods to perform
/// peak integration etc. can be performed in parallel, and while other chromatograms,
/// if they end at a later time, are still being generated from the raw file
/// Chromatograms are generated from scans read from the raw file.
/// Each scan is read only once.
/// Data from the scans is reduced to mass and intensity information only, to reduce memory consumed.
/// If the data is "FT format" with a set of supplied centroids, then the data is generated from
/// the centroids only (no profiles are read from the raw file).
/// If the data is not FT format then the regular "segmented scan" data is used.
/// Scans are read in increasing scan number order.
/// Scans which are outside of the required ranges to build the chromatograms are not read.
/// Methods to "get chromatograms" are not called directly on supplied raw data interfaces.
/// This also supports async reading from multiple raw files, as the returned information is a thread array,
/// which the caller must sync (with "WaitAll") to complete operations on a given file.
/// </summary>
public static class ParallelChromatogramFactory
{
	/// <summary>
	/// Defines an implementation of "IScanWithSimpleData",
	/// </summary>
	private class ScanWithSimpleDataLocal : IScanWithSimpleData
	{
		/// <summary>
		/// Gets or sets the scan event.
		/// </summary>
		public ScanEventHelper ScanEvent { get; set; }

		/// <summary>
		/// Gets or sets the data.
		/// </summary>
		public ISimpleScanAccess Data { get; set; }
	}

	/// <summary>
	/// The fast scan from raw data class, which supplies code needed for the
	/// chromatogram batch generator to get from the IRawDataPlus interface.
	/// Enhanced version uses methods which only return mass and intensity data.
	/// </summary>
	private class FastScanFromRawData
	{
		private readonly IRawDataPlus _rawFile;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ParallelChromatogramFactory.FastScanFromRawData" /> class.
		/// </summary>
		/// <param name="rawData">
		///   The raw data.
		/// </param>
		public FastScanFromRawData(IRawDataPlus rawData)
		{
			_rawFile = rawData;
		}

		/// <summary>
		/// The scan reader.
		/// </summary>
		/// <param name="scanIndex">
		/// The scan index, to the available scans.
		/// </param>
		/// <returns>
		/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.SimpleScanWithHeader" />.
		/// </returns>
		public SimpleScanWithHeader Reader(ISimpleScanHeader scanIndex)
		{
			int scanNumber = scanIndex.ScanNumber;
			IScanEvent scanEventForScanNumber = _rawFile.GetScanEventForScanNumber(scanNumber);
			ISimpleScanAccess simpleScanAccess = null;
			if (scanEventForScanNumber.MassAnalyzer == MassAnalyzerType.MassAnalyzerFTMS && scanEventForScanNumber.MassCalibratorCount > 0)
			{
				ISimpleScanAccess simplifiedCentroids = _rawFile.GetSimplifiedCentroids(scanNumber);
				if (simplifiedCentroids?.Masses != null)
				{
					simpleScanAccess = simplifiedCentroids;
				}
			}
			if (simpleScanAccess == null)
			{
				simpleScanAccess = _rawFile.GetSimplifiedScan(scanNumber);
			}
			ScanEventHelper scanEvent = ScanEventHelper.ScanEventHelperFactory(scanEventForScanNumber);
			return new SimpleScanWithHeader
			{
				Header = scanIndex,
				Scan = new ScanWithSimpleDataLocal
				{
					Data = simpleScanAccess,
					ScanEvent = scanEvent
				}
			};
		}
	}

	/// <summary>
	/// Attach data sources to a Chromatogram batch generator (for MS chromatograms),
	/// based in data available via IRawDataPlus.
	/// This method has an IO overhead, as it configures a ChromatogramBatchGenerator, with a table
	/// of "scan number and RT", which is read from the raw data.
	/// It requires that the "rawData" interface supplied has an efficient implementation of RetentionTimeFromScanNumber.
	/// </summary>
	/// <param name="generator">
	/// Tool to generate chromatograms
	/// </param>
	/// <param name="rawData">
	/// The raw data.
	/// </param>
	/// <exception cref="T:System.ArgumentException">
	/// Thrown if there is no available MS data.
	/// </exception>
	public static void FromRawData(IChromatogramBatchGenerator generator, IRawDataPlus rawData)
	{
		if (!rawData.HasMsData)
		{
			throw new ArgumentException("Supplied raw data has no MS data", "rawData");
		}
		rawData.SelectInstrument(Device.MS, 1);
		InstrumentData instrumentData = rawData.GetInstrumentData();
		generator.AccuratePrecursors = instrumentData.IsTsqQuantumFile();
		int filterMassPrecision = rawData.RunHeaderEx.FilterMassPrecision;
		generator.FilterMassPrecision = filterMassPrecision;
		generator.ScanReader = new FastScanFromRawData(rawData).Reader;
		generator.AvailableScans = MsScanList(rawData);
	}

	/// <summary>
	/// Create the list of all MS scans.
	/// </summary>
	/// <param name="data">
	/// The data.
	/// </param>
	/// <returns>
	/// The list of scan headers.
	/// </returns>
	private static ISimpleScanHeader[] MsScanList(IRawDataPlus data)
	{
		IRunHeaderAccess runHeader = data.RunHeader;
		int first = runHeader.FirstSpectrum;
		int lastSpectrum = runHeader.LastSpectrum;
		if (first >= 1 && lastSpectrum >= first)
		{
			ISimpleScanHeader[] toReturn = new ISimpleScanHeader[lastSpectrum - first + 1];
			int rangeSize = Math.Max(10, lastSpectrum / 50);
			Parallel.ForEach(Partitioner.Create(first, lastSpectrum + 1, rangeSize), delegate(Tuple<int, int> range)
			{
				for (int i = range.Item1; i < range.Item2; i++)
				{
					toReturn[i - first] = new SimpleScanHeader
					{
						RetentionTime = data.RetentionTimeFromScanNumber(i),
						ScanNumber = i
					};
				}
			});
			return toReturn;
		}
		return new ISimpleScanHeader[0];
	}
}
