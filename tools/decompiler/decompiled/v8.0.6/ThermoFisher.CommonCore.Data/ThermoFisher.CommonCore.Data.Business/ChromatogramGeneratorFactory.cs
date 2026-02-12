using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The chromatogram generator factory.
/// </summary>
public static class ChromatogramGeneratorFactory
{
	/// <summary>
	/// The compressed chromatogram.
	/// This holds a "double intensity" and a "shot index offset" for each scan
	/// in the chromatogram, that is 10 bytes.
	/// Al alternative (scan number 4 bytes. plus RT 8 bytes, plus intensity 8 bytes is twice as much memory 20bytes).
	/// The data is expanded up on request.
	/// With 1k unfiltered chromatograms, over 100k scans, this saves 1k*100k*10=1GB ram.
	/// </summary>
	private class CompressedChromatogram : ISignalConvert
	{
		private readonly int _firstIndex;

		private readonly int[] _indexOffsets;

		private readonly double[] _intensities;

		/// <summary>
		/// Gets the length.
		/// </summary>
		public int Length => _intensities.Length;

		/// <summary>
		/// Convert to signal.
		/// The object implementing this would have the required intensity information,
		/// but limited other data (such as RT values) which can be pulled from "scans".
		/// </summary>
		/// <param name="scans">
		/// The scans.
		/// </param>
		/// <returns>
		/// The <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.IChromatogramSignalAccess" />.
		/// </returns>
		public ChromatogramSignal ToSignal(IList<ISimpleScanHeader> scans)
		{
			int num = _intensities.Length;
			int[] array = new int[num];
			double[] array2 = new double[num];
			int firstIndex = _firstIndex;
			int[] indexOffsets = _indexOffsets;
			int i = 0;
			int num2 = num - 2;
			while (i < num2)
			{
				ISimpleScanHeader simpleScanHeader = scans[firstIndex + indexOffsets[i]];
				array[i] = simpleScanHeader.ScanNumber;
				array2[i] = simpleScanHeader.RetentionTime;
				simpleScanHeader = scans[firstIndex + indexOffsets[++i]];
				array[i] = simpleScanHeader.ScanNumber;
				array2[i] = simpleScanHeader.RetentionTime;
				simpleScanHeader = scans[firstIndex + indexOffsets[++i]];
				array[i] = simpleScanHeader.ScanNumber;
				array2[i] = simpleScanHeader.RetentionTime;
				i++;
			}
			for (; i < num; i++)
			{
				ISimpleScanHeader simpleScanHeader2 = scans[firstIndex + indexOffsets[i]];
				array[i] = simpleScanHeader2.ScanNumber;
				array2[i] = simpleScanHeader2.RetentionTime;
			}
			return ChromatogramSignal.FromTimeIntensityScan(array2, _intensities, array);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramGeneratorFactory.CompressedChromatogram" /> class.
		/// </summary>
		/// <param name="firstIndex">
		/// The first index.
		/// </param>
		/// <param name="indexOffsets">
		/// The index offsets.
		/// </param>
		/// <param name="intensities">
		/// The intensities.
		/// </param>
		public CompressedChromatogram(int firstIndex, int[] indexOffsets, double[] intensities)
		{
			_firstIndex = firstIndex;
			_indexOffsets = indexOffsets;
			_intensities = intensities;
		}
	}

	/// <summary>
	/// Create a chromatogram from a set of scans, with no filtering
	/// </summary>
	private class UnfilteredChromatogramGenerator : IChromatogramGenerator
	{
		private readonly Func<ISimpleScanWithHeader, double> _valueForScan;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramGeneratorFactory.UnfilteredChromatogramGenerator" /> class.
		/// </summary>
		/// <param name="request">
		/// The request.
		/// </param>
		public UnfilteredChromatogramGenerator(IChromatogramRequest request)
		{
			_valueForScan = request.ValueForScan;
		}

		/// <summary>
		/// Create a chromatogram.
		/// </summary>
		/// <param name="scans">
		/// The scans.
		/// </param>
		/// <returns>
		/// An interface to create a signal from the intensities which are calculated.
		/// Null if no scans.
		/// </returns>
		public ISignalConvert CreatePartialChromatogram(ScanAndIndex[] scans)
		{
			int num = scans.Length;
			if (num == 0)
			{
				return null;
			}
			double[] array = new double[num];
			int[] array2 = new int[num];
			int index = scans[0].Index;
			for (int i = 0; i < scans.Length; i++)
			{
				ScanAndIndex scanAndIndex = scans[i];
				array2[i] = scanAndIndex.Index - index;
				array[i] = _valueForScan(scanAndIndex);
			}
			return new CompressedChromatogram(index, array2, array);
		}
	}

	/// <summary>
	/// Create a chromatogram from a set of scans
	/// </summary>
	private class ChromatogramGeneratorWithNameFilter : IChromatogramGenerator
	{
		private readonly IList<string> _names;

		private readonly Func<ISimpleScanWithHeader, double> _valueForScan;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramGeneratorFactory.ChromatogramGeneratorWithNameFilter" /> class.
		/// </summary>
		/// <param name="request">
		/// The request.
		/// </param>
		public ChromatogramGeneratorWithNameFilter(IChromatogramRequest request)
		{
			_names = request.ScanSelector.Names;
			_valueForScan = request.ValueForScan;
		}

		/// <summary>
		/// Create a chromatogram.
		/// </summary>
		/// <param name="scans">
		/// The scans.
		/// </param>
		/// <returns>
		/// An interface to create a signal from the intensities which are calculated.
		/// Null if no scans passed the filter.
		/// </returns>
		public ISignalConvert CreatePartialChromatogram(ScanAndIndex[] scans)
		{
			List<double> list = null;
			List<int> list2 = null;
			IList<string> names = _names;
			bool flag = true;
			int num = 0;
			foreach (ScanAndIndex scanAndIndex in scans)
			{
				ScanEventHelper scanEvent = scanAndIndex.ScanEvent;
				if (scanEvent != null && scanEvent.TestScanAgainstNames(names))
				{
					if (flag)
					{
						list = new List<double>(100);
						list2 = new List<int>(100);
						flag = false;
						num = scanAndIndex.Index;
					}
					list2.Add(scanAndIndex.Index - num);
					list.Add(_valueForScan(scanAndIndex));
				}
			}
			if (!flag)
			{
				return new CompressedChromatogram(num, list2.ToArray(), list.ToArray());
			}
			return null;
		}
	}

	/// <summary>
	/// Create a chromatogram from a set of scans, where a filter is used for scan selection.
	/// </summary>
	private class ChromatogramGeneratorWithFilter : IChromatogramGenerator
	{
		private readonly Func<ISimpleScanWithHeader, double> _valueForScan;

		private readonly ScanFilterHelper _filter;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramGeneratorFactory.ChromatogramGeneratorWithFilter" /> class.
		/// </summary>
		/// <param name="request">
		/// The request.
		/// </param>
		/// <param name="accuratePrecursors">In this mode a tolerance matching for precursors is "accurate" (based on the precision value),
		/// except for data dependent scans.
		/// For all "non accurate" precursor selections, and for all data dependent scans a default (wide) tolerance is used.</param>
		/// <param name="filterMassPrecision">number of decimal places for masses in filter</param>
		/// <param name="allEvents">scan events interface, which can be used to optimize filtering, if provided.
		/// This interface may be obtained from IRawDataPlus (ScanEvents property).</param>
		public ChromatogramGeneratorWithFilter(IChromatogramRequest request, bool accuratePrecursors, int filterMassPrecision, IScanEvents allEvents)
		{
			IScanFilter scanFilter = request.ScanSelector.ScanFilter;
			if (filterMassPrecision < 0)
			{
				filterMassPrecision = scanFilter.MassPrecision;
			}
			_filter = new ScanFilterHelper(request.ScanSelector.ScanFilter, accuratePrecursors, filterMassPrecision);
			if (allEvents != null)
			{
				_filter.InitializeAvailableEvents(allEvents);
			}
			_valueForScan = request.ValueForScan;
		}

		/// <summary>
		/// Create a chromatogram.
		/// </summary>
		/// <param name="scans">
		/// The scans.
		/// </param>
		/// <returns>
		/// An interface to create a signal from the intensities which are calculated.
		/// Null if no scans passed the filter.
		/// </returns>
		public ISignalConvert CreatePartialChromatogram(ScanAndIndex[] scans)
		{
			List<double> list = null;
			List<int> list2 = null;
			bool flag = true;
			int num = 0;
			int num2 = scans.Length;
			ScanFilterHelper filter = _filter;
			for (int i = 0; i < num2; i++)
			{
				ScanAndIndex scanAndIndex = scans[i];
				ScanEventHelper scanEvent = scanAndIndex.ScanEvent;
				if (scanEvent != null && scanEvent.TestScanAgainstFilter(filter))
				{
					if (flag)
					{
						list = new List<double>(100);
						list2 = new List<int>(100);
						flag = false;
						num = scanAndIndex.Index;
					}
					list2.Add(scanAndIndex.Index - num);
					list.Add(_valueForScan(scanAndIndex));
				}
			}
			if (!flag)
			{
				return new CompressedChromatogram(num, list2.ToArray(), list.ToArray());
			}
			return null;
		}
	}

	/// <summary>
	/// Create a chromatogram generator.
	/// This configures the generator for optimal performance in checking scans.
	/// as there may be over 1000 components over 100k scans, that is 10m or more scan
	/// filtering tests (most of which fail). So: optimize that test
	/// use "for" and arrays (not foreach and IEnumerable) etc.
	/// </summary>
	/// <param name="request">
	/// The request.
	/// </param>
	/// <param name="accuratePrecursors">In this mode a tolerance matching for precursors is "accurate" (based on the precision value),
	/// except for data dependent scans.
	/// For all "non accurate" precursor selections, and for all data dependent scans a default (wide) tolerance is used.</param>
	/// <param name="filterMassPrecision">number of decimal places for masses in filter</param>
	/// <param name="allEvents">scan events interface, which can be used to optimize filtering, if provided.
	/// This interface may be obtained from IRawDataPlus (ScanEvents property).</param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.IChromatogramGenerator" />.
	/// </returns>
	public static IChromatogramGenerator CreateChromatogramGenerator(IChromatogramRequest request, bool accuratePrecursors, int filterMassPrecision, IScanEvents allEvents = null)
	{
		IScanSelect scanSelector = request.ScanSelector;
		if (request.ScanSelector.UseFilter)
		{
			return new ChromatogramGeneratorWithFilter(request, accuratePrecursors, filterMassPrecision, allEvents);
		}
		IList<string> names = scanSelector.Names;
		if (names != null && names.Count > 0)
		{
			return new ChromatogramGeneratorWithNameFilter(request);
		}
		return new UnfilteredChromatogramGenerator(request);
	}
}
