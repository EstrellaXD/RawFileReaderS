using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// The fast scan from raw data class, which supplies code needed for the
/// chromatogram batch generator to get data from the IRawDataPlus interface.
/// Enhanced version uses methods which only return mass and intensity data.
/// </summary>
internal class FastScanFromMsData
{
	private readonly MassSpecDevice _msDevice;

	private readonly ScanFilterHelper _helper;

	private readonly bool _includeReferenceAndExceptionData;

	private readonly IBufferPool _pool;

	public bool ExtendedData { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.FastScanFromMsData" /> class.
	/// </summary>
	/// <param name="msData">
	/// The MS data reader.
	/// </param>
	/// <param name="helper">Scan filter, which may be null.
	///     When not null: All data needing these scans has the same filter</param>
	/// <param name="includeReferenceAndExceptionData">True if ref and exception peaks should be used</param>
	/// <param name="pool">Buffer pool, which reduces garbage collection</param>
	public FastScanFromMsData(MassSpecDevice msData, ScanFilterHelper helper, bool includeReferenceAndExceptionData, IBufferPool pool)
	{
		_msDevice = msData;
		_helper = helper;
		_includeReferenceAndExceptionData = includeReferenceAndExceptionData;
		_pool = pool;
	}

	/// <summary>
	/// The parallel reader.
	/// </summary>
	/// <param name="scanIndex">
	/// The scan index.
	/// </param>
	/// <returns>
	/// The array of scans, read in parallel.
	/// </returns>
	public SimpleScanWithHeader[] ParallelReader(ISimpleScanHeader[] scanIndex)
	{
		IMemoryReader subRangeReader = null;
		int num = scanIndex.Length;
		SimpleScanWithHeader[] toReturn = new SimpleScanWithHeader[num];
		if (_msDevice.ShouldBuffer && num > 0)
		{
			if (scanIndex is ScanIndex[] array)
			{
				ScanIndex firstScanIndex = array[0];
				ScanIndex secondScanIndex = array[^1];
				subRangeReader = _msDevice.CreateRentableSubRangeReader(firstScanIndex, secondScanIndex, _pool);
			}
			else
			{
				ISimpleScanHeader simpleScanHeader = scanIndex[0];
				ISimpleScanHeader obj = scanIndex[^1];
				int scanNumber = simpleScanHeader.ScanNumber;
				int scanNumber2 = obj.ScanNumber;
				subRangeReader = _msDevice.CreateRentableSubRangeReader(scanNumber, scanNumber2, _pool);
			}
		}
		int rangeSize = Math.Max(3, num / 20);
		Parallel.ForEach(Partitioner.Create(0, num, rangeSize), delegate(Tuple<int, int> range)
		{
			for (int i = range.Item1; i < range.Item2; i++)
			{
				toReturn[i] = Reader(scanIndex[i], subRangeReader);
			}
		});
		_msDevice.ReleaseSubRangeReader(subRangeReader, _pool);
		return toReturn;
	}

	/// <summary>
	/// The scan reader.
	/// </summary>
	/// <param name="simpleScanHeader">
	/// Data about the scan.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.SimpleScanWithHeader" />.
	/// </returns>
	public SimpleScanWithHeader Reader(ISimpleScanHeader simpleScanHeader)
	{
		return Reader(simpleScanHeader, null);
	}

	private SimpleScanWithHeader Reader(ISimpleScanHeader simpleScanHeader, IMemoryReader subRangeReader)
	{
		ISimpleScanAccess simpleScanAccess = null;
		ScanEventHelper scanEventHelper = null;
		if (simpleScanHeader is ScanIndex scanIndex)
		{
			try
			{
				ScanEvent scanEvent = _msDevice.ScanEventWithValidScanNumber(scanIndex);
				scanEventHelper = ScanEventHelper.ScanEventHelperFactory(scanEvent, scanEvent.ScanTypeLocation);
				if (_helper != null && !scanEventHelper.TestScanAgainstFilter(_helper))
				{
					return new SimpleScanWithHeader
					{
						Header = simpleScanHeader
					};
				}
				int scanNumber = scanIndex.ScanNumber;
				if (scanIndex.PacketType.HasLabelPeaks() && scanEvent.MassCalibratorCount > 0)
				{
					if (_msDevice.SupportsSimplifiedPacket(scanIndex))
					{
						ISimpleMsPacket simplifiedMsPacket = _msDevice.GetSimplifiedMsPacket(scanNumber, _includeReferenceAndExceptionData, PacketFeatures.None, scanIndex, scanEvent, subRangeReader, zeroPadding: false);
						simpleScanAccess = new SimpleScan
						{
							Masses = simplifiedMsPacket.Mass,
							Intensities = simplifiedMsPacket.Intensity
						};
					}
					else
					{
						ISimpleScanAccess simplifiedLabels = _msDevice.GetSimplifiedLabels(scanNumber, _includeReferenceAndExceptionData, scanEvent, scanIndex, ExtendedData, subRangeReader);
						if (simplifiedLabels?.Masses != null)
						{
							simpleScanAccess = simplifiedLabels;
						}
					}
				}
				if (simpleScanAccess == null)
				{
					if (_msDevice.SupportsSimplifiedPacket(scanIndex))
					{
						ISimpleMsPacket simplifiedMsPacket2 = _msDevice.GetSimplifiedMsPacket(scanNumber, _includeReferenceAndExceptionData, PacketFeatures.None, scanIndex, scanEvent, subRangeReader, zeroPadding: false);
						simpleScanAccess = new SimpleScan
						{
							Masses = simplifiedMsPacket2.Mass,
							Intensities = simplifiedMsPacket2.Intensity
						};
					}
					else
					{
						IMsPacket msPacket = _msDevice.GetMsPacket(scanNumber, _includeReferenceAndExceptionData, PacketFeatures.None, scanIndex, scanEvent, subRangeReader, zeroPadding: false);
						List<SegmentData> list = ((msPacket == null) ? new List<SegmentData>() : msPacket.SegmentPeaks);
						int count = list.Count;
						int num = 0;
						for (int i = 0; i < count; i++)
						{
							num += list[i].DataPeaks.Count;
						}
						double[] array = new double[num];
						double[] array2 = new double[num];
						if (num > 0)
						{
							int num2 = 0;
							for (int j = 0; j < count; j++)
							{
								foreach (DataPeak dataPeak in list[j].DataPeaks)
								{
									array[num2] = dataPeak.Position;
									array2[num2] = dataPeak.Intensity;
									num2++;
								}
							}
						}
						simpleScanAccess = new SimpleScan
						{
							Masses = array,
							Intensities = array2
						};
					}
				}
			}
			catch
			{
			}
		}
		if (simpleScanAccess == null)
		{
			simpleScanAccess = new SimpleScan
			{
				Masses = Array.Empty<double>(),
				Intensities = Array.Empty<double>()
			};
		}
		if (scanEventHelper == null)
		{
			scanEventHelper = ScanEventHelper.ScanEventHelperFactory(new ScanEvent());
		}
		return new SimpleScanWithHeader
		{
			Header = simpleScanHeader,
			Scan = new ScanWithSimpleDataLocal
			{
				Data = simpleScanAccess,
				ScanEvent = scanEventHelper
			}
		};
	}
}
