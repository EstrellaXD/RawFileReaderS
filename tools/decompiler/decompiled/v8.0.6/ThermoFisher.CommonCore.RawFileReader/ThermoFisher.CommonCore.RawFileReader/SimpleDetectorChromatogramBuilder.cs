using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// The simple detector chromatogram builder.
/// Makes chromatograms from devices other than MS
/// </summary>
internal class SimpleDetectorChromatogramBuilder
{
	/// <summary>
	/// The chromatogram result (1 point in chromatogram).
	/// </summary>
	private class ChroResult
	{
		/// <summary>
		/// Gets or sets the base peak.
		/// </summary>
		public double BasePeak { get; set; }

		/// <summary>
		/// Gets or sets the intensity.
		/// </summary>
		public double Intensity { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this point is saturated.
		/// </summary>
		public bool IsSaturated { get; set; }

		/// <summary>
		/// Gets or sets the time.
		/// </summary>
		public double Time { get; set; }

		/// <summary>
		/// Gets or sets the scan number.
		/// </summary>
		public int ScanNumber { get; set; }
	}

	private readonly IDevice _selectedDevice;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.SimpleDetectorChromatogramBuilder" /> class.
	/// </summary>
	/// <param name="device">
	/// The device (to create chromatograms from).
	/// </param>
	public SimpleDetectorChromatogramBuilder(IDevice device)
	{
		_selectedDevice = device;
	}

	/// <summary>
	/// Generate chromatograms for all detectors, except for MS
	/// </summary>
	/// <param name="settings">Input parameters</param>
	/// <param name="startScan">first scan in chromatogram</param>
	/// <param name="endScan">last scan in chromatogram</param>
	/// <returns>The Chromatogram</returns>
	public IChromatogramDataPlus CreateChromatograms(IChromatogramSettings[] settings, int startScan, int endScan)
	{
		int num = settings.Length;
		int num2 = 0;
		int scans = endScan - startScan + 1;
		for (int i = 0; i < num; i++)
		{
			IChromatogramSettings obj = settings[i];
			int massRangeCount = obj.MassRangeCount;
			switch (obj.Trace)
			{
			case TraceType.WavelengthRange:
			case TraceType.SpectrumMax:
				num2 += ((massRangeCount <= 0) ? 1 : massRangeCount);
				break;
			}
		}
		KeyValuePair<int, IRangeAccess>[] array = new KeyValuePair<int, IRangeAccess>[num2];
		int num3 = 0;
		int num4 = 0;
		for (int j = 0; j < num; j++)
		{
			IChromatogramSettings chromatogramSettings = settings[j];
			int massRangeCount2 = chromatogramSettings.MassRangeCount;
			TraceType trace = chromatogramSettings.Trace;
			if (trace == TraceType.WavelengthRange || trace == TraceType.SpectrumMax)
			{
				if (massRangeCount2 != 0)
				{
					for (int k = 0; k < massRangeCount2; k++)
					{
						array[num3++] = new KeyValuePair<int, IRangeAccess>(j, chromatogramSettings.MassRanges[k]);
					}
				}
				else
				{
					ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader runHeader = _selectedDevice.RunHeader;
					array[num3++] = new KeyValuePair<int, IRangeAccess>(j, ThermoFisher.CommonCore.Data.Business.Range.Create(runHeader.LowMass, runHeader.HighMass));
				}
			}
			else
			{
				num4++;
			}
		}
		return BuildNonMsChromatograms(settings, num, scans, startScan, endScan, num4, array, num2);
	}

	/// <summary>
	/// Builds the non MS chromatograms.
	/// </summary>
	/// <param name="settings">The settings.</param>
	/// <param name="numberOfChromatograms">The number of chromatograms</param>
	/// <param name="scans">The number of scans.</param>
	/// <param name="firstScan">The first scan.</param>
	/// <param name="lastScan">The last scan.</param>
	/// <param name="numberOfNonMassRangeSumChros">The number of non mass range sum chromatograms.</param>
	/// <param name="massRangeArray">The mass range array.</param>
	/// <param name="massRanges">The number of mass ranges.</param>
	/// <returns>Chromatogram results</returns>
	public IChromatogramDataPlus BuildNonMsChromatograms(IChromatogramSettings[] settings, int numberOfChromatograms, int scans, int firstScan, int lastScan, int numberOfNonMassRangeSumChros, KeyValuePair<int, IRangeAccess>[] massRangeArray, int massRanges)
	{
		int num = settings.Length;
		if (num == 0)
		{
			return null;
		}
		ChroResult[][] array = new ChroResult[num][];
		for (int i = 0; i < num; i++)
		{
			array[i] = new ChroResult[scans];
		}
		switch (settings[0].Trace)
		{
		case TraceType.Analog1:
		case TraceType.Analog2:
		case TraceType.Analog3:
		case TraceType.Analog4:
		case TraceType.Analog5:
		case TraceType.Analog6:
		case TraceType.Analog7:
		case TraceType.Analog8:
		case TraceType.ChannelA:
		case TraceType.ChannelB:
		case TraceType.ChannelC:
		case TraceType.ChannelD:
		case TraceType.ChannelE:
		case TraceType.ChannelF:
		case TraceType.ChannelG:
		case TraceType.ChannelH:
		case TraceType.A2DChannel1:
		case TraceType.A2DChannel2:
		case TraceType.A2DChannel3:
		case TraceType.A2DChannel4:
		case TraceType.A2DChannel5:
		case TraceType.A2DChannel6:
		case TraceType.A2DChannel7:
		case TraceType.A2DChannel8:
			BuildChannelTraces(settings, array, firstScan, lastScan);
			break;
		default:
		{
			for (int j = firstScan; j <= lastScan; j++)
			{
				IScanIndex scanIndex = _selectedDevice.GetScanIndex(j);
				IPacket packet = _selectedDevice.GetPacket(j, includeReferenceAndExceptionData: false);
				ReadDataFromScan(settings, array, massRangeArray, massRanges, packet.SegmentPeaks, scanIndex, j, firstScan);
				BuildTraceTypesFromHeader(settings, array, packet, packet.Index ?? scanIndex, j, firstScan, numberOfNonMassRangeSumChros);
			}
			break;
		}
		}
		for (int k = 0; k < numberOfChromatograms; k++)
		{
			double delayInMin = settings[k].DelayInMin;
			if (!(Math.Abs(delayInMin) > 1E-10))
			{
				continue;
			}
			ChroResult[] array2 = array[k];
			for (int l = firstScan; l < lastScan; l++)
			{
				ChroResult chroResult = array2[l - firstScan];
				if (chroResult != null)
				{
					chroResult.Time += delayInMin;
				}
			}
		}
		ChromatogramSignal[] array3 = new ChromatogramSignal[settings.Length];
		for (int m = 0; m < settings.Length; m++)
		{
			array3[m] = MakeSignal(array[m]);
		}
		return ChromatogramSignal.ToChromatogramDataPlus(array3);
	}

	/// <summary>
	/// read data from scan.
	/// </summary>
	/// <param name="settings">
	/// The settings.
	/// </param>
	/// <param name="allData">
	/// All results data.
	/// </param>
	/// <param name="wavelengthRanges">
	/// The wavelength ranges.
	/// </param>
	/// <param name="wavelengthRangeCount">
	/// The wavelength range count.
	/// </param>
	/// <param name="scanData">
	/// The scan data.
	/// </param>
	/// <param name="scanIndex">
	/// The scan index.
	/// </param>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="firstScan">
	/// The first scan.
	/// </param>
	private static void ReadDataFromScan(IChromatogramSettings[] settings, ChroResult[][] allData, KeyValuePair<int, IRangeAccess>[] wavelengthRanges, int wavelengthRangeCount, IReadOnlyList<SegmentData> scanData, IScanIndex scanIndex, int scanNumber, int firstScan)
	{
		for (int i = 0; i < wavelengthRangeCount; i++)
		{
			ChroResult chroResult = null;
			int key = wavelengthRanges[i].Key;
			IChromatogramSettings chromatogramSettings = settings[key];
			int num = 0;
			IRangeAccess value = wavelengthRanges[i].Value;
			int count = scanData.Count;
			chroResult = allData[key][scanNumber - firstScan];
			if (chroResult == null)
			{
				chroResult = new ChroResult
				{
					Time = scanIndex.StartTime,
					ScanNumber = scanNumber
				};
				allData[key][scanNumber - firstScan] = chroResult;
			}
			for (int j = 0; j < count; j++)
			{
				SegmentData segmentData = scanData[j];
				List<DataPeak> dataPeaks = segmentData.DataPeaks;
				int count2 = dataPeaks.Count;
				if (count2 == 0)
				{
					continue;
				}
				int k = segmentData.FindPeakPos(value.Low);
				if (k < 0)
				{
					continue;
				}
				double num2 = 0.0;
				num = 0;
				double high = value.High;
				for (; k < count2; k++)
				{
					DataPeak dataPeak2;
					DataPeak dataPeak = (dataPeak2 = dataPeaks[k]);
					if (!(dataPeak.Position <= high))
					{
						break;
					}
					switch (chromatogramSettings.Trace)
					{
					case TraceType.SpectrumMax:
						if (dataPeak2.Intensity > chroResult.Intensity)
						{
							chroResult.Intensity = dataPeak2.Intensity;
							chroResult.IsSaturated = dataPeak2.IsSaturated;
							chroResult.BasePeak = (float)dataPeak2.Position;
						}
						break;
					case TraceType.WavelengthRange:
						num++;
						chroResult.Intensity += dataPeak2.Intensity;
						chroResult.IsSaturated |= dataPeak2.IsSaturated;
						if (dataPeak2.Intensity > num2)
						{
							num2 = dataPeak2.Intensity;
							chroResult.BasePeak = (float)dataPeak2.Position;
						}
						break;
					}
				}
			}
			if (chromatogramSettings.Trace == TraceType.WavelengthRange && num > 0 && chroResult != null)
			{
				chroResult.Intensity /= num;
			}
		}
	}

	/// <summary>
	/// Builds the channel traces.
	/// UV channels, or analog channels.
	/// </summary>
	/// <param name="settings">Parameters for the chromatograms.</param>
	/// <param name="results">Table of all chromatogram</param>
	/// <param name="firstScan">The first scan.</param>
	/// <param name="lastScan">The last scan.</param>
	private bool BuildChannelTraces(IChromatogramSettings[] settings, ChroResult[][] results, int firstScan, int lastScan)
	{
		int num = settings.Length;
		int[] array = new int[num];
		for (int i = 0; i < num; i++)
		{
			IChromatogramSettings chromatogramSettings = settings[i];
			if ((array[i] = ChannelToIndex(chromatogramSettings.Trace)) == -1)
			{
				return false;
			}
		}
		if (!(_selectedDevice is UvDevice uvDevice))
		{
			return false;
		}
		for (int j = firstScan; j <= lastScan; j++)
		{
			int num2 = j - firstScan;
			ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex uvIndex = uvDevice.GetUvIndex(j);
			int numberOfChannels = uvIndex.NumberOfChannels;
			for (int k = 0; k < num; k++)
			{
				int num3 = array[k];
				IReadOnlyList<SegmentData> segmentPeaks = uvDevice.GetChannelPacket(uvIndex, num3).SegmentPeaks;
				DataPeak dataPeak = ((segmentPeaks.Count > 0) ? segmentPeaks[0].DataPeaks[0] : default(DataPeak));
				if (numberOfChannels > num3)
				{
					results[k][num2] = new ChroResult
					{
						Time = dataPeak.Position,
						ScanNumber = j,
						Intensity = dataPeak.Intensity
					};
				}
				else
				{
					results[k][num2] = new ChroResult
					{
						Time = dataPeak.Position,
						ScanNumber = -1
					};
				}
			}
		}
		return true;
	}

	/// <summary>
	/// For chromatograms which need the scan index data only, fill in the data.
	/// </summary>
	/// <param name="settings">Parameters for the chromatograms</param>
	/// <param name="results">Table of all chromatogram results</param>
	/// <param name="packets">Data in scan</param>
	/// <param name="scanIndex">index for this scan</param>
	/// <param name="scanNumber">The scan number</param>
	/// <param name="firstScan">The first scan</param>
	/// <param name="numberOfNonMassRangeSumChromatograms">The number of non mass range sum chromatograms</param>
	private void BuildTraceTypesFromHeader(IChromatogramSettings[] settings, ChroResult[][] results, IPacket packets, IScanIndex scanIndex, int scanNumber, int firstScan, int numberOfNonMassRangeSumChromatograms)
	{
		int num = scanNumber - firstScan;
		if (numberOfNonMassRangeSumChromatograms <= 0)
		{
			return;
		}
		for (int i = 0; i < settings.Length; i++)
		{
			IChromatogramSettings chromatogramSettings = settings[i];
			if (chromatogramSettings != null)
			{
				TraceType trace = chromatogramSettings.Trace;
				ChroResult chroResult = results[i][num];
				if (chroResult == null)
				{
					chroResult = new ChroResult
					{
						Time = scanIndex.StartTime,
						ScanNumber = scanNumber
					};
					results[i][num] = chroResult;
				}
				switch (trace)
				{
				case TraceType.TIC:
				case TraceType.TotalAbsorbance:
					chroResult.Intensity = scanIndex.Tic;
					break;
				case TraceType.Analog1:
				case TraceType.Analog2:
				case TraceType.Analog3:
				case TraceType.Analog4:
				case TraceType.Analog5:
				case TraceType.Analog6:
				case TraceType.Analog7:
				case TraceType.Analog8:
					chroResult.Intensity = 0.0;
					break;
				}
			}
		}
	}

	/// <summary>
	/// Convert a channel number into a channel index
	/// which is used to offset into a scan
	/// </summary>
	/// <param name="channel">Channel type</param>
	/// <returns>Channel index (0 to 7), or -1 if not a valid channel</returns>
	private int ChannelToIndex(TraceType channel)
	{
		if (channel >= TraceType.ChannelA && channel <= TraceType.ChannelH)
		{
			return (int)(channel - 31);
		}
		if (channel >= TraceType.A2DChannel1 && channel <= TraceType.A2DChannel8)
		{
			return (int)(channel - 41);
		}
		if (channel >= TraceType.Analog1 && channel <= TraceType.Analog8)
		{
			return (int)(channel - 11);
		}
		return -1;
	}

	/// <summary>
	/// Convert a set of data peaks into signal
	/// </summary>
	/// <param name="dataPeak">Data to convert</param>
	/// <returns>Converted Signal</returns>
	private ChromatogramSignal MakeSignal(ChroResult[] dataPeak)
	{
		int num = dataPeak.Length;
		double[] array = new double[num];
		double[] array2 = new double[num];
		int[] array3 = new int[num];
		double[] array4 = new double[num];
		for (int i = 0; i < num; i++)
		{
			ChroResult chroResult = dataPeak[i];
			if (chroResult != null)
			{
				array[i] = chroResult.Time;
				array3[i] = chroResult.ScanNumber;
				array2[i] = chroResult.Intensity;
				array4[i] = chroResult.BasePeak;
			}
		}
		return ChromatogramSignal.FromTimeIntensityScanBasePeak(array, array2, array3, array4);
	}
}
