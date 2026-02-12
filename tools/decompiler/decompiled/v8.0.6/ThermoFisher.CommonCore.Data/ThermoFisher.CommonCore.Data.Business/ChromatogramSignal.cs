using System;
using System.Collections.Generic;
using System.Linq;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
///     This represents the data for a chromatogram
/// </summary>
[Serializable]
public sealed class ChromatogramSignal : ICloneable, IChromatogramData, IChromatogramSignalAccess
{
	/// <summary>
	///     The wrapped chromatogram data.
	/// </summary>
	private class WrappedChromatogramData : IChromatogramDataPlus, IChromatogramData, IChromatogramBasePeaks
	{
		/// <summary>
		///     Gets the base peak array.
		/// </summary>
		public double[][] BasePeakArray { get; }

		/// <summary>
		///     Gets the intensities array.
		/// </summary>
		public double[][] IntensitiesArray { get; }

		/// <summary>
		///     Gets the length.
		/// </summary>
		public int Length { get; }

		/// <summary>
		///     Gets the positions array.
		/// </summary>
		public double[][] PositionsArray { get; }

		/// <summary>
		///     Gets the scan numbers array.
		/// </summary>
		public int[][] ScanNumbersArray { get; }

		/// <summary>
		///     Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramSignal.WrappedChromatogramData" /> class.
		/// </summary>
		/// <param name="signals">
		///     The data to wrap
		/// </param>
		internal WrappedChromatogramData(ChromatogramSignal[] signals)
		{
			if (signals != null)
			{
				Length = signals.Length;
				PositionsArray = new double[Length][];
				ScanNumbersArray = new int[Length][];
				IntensitiesArray = new double[Length][];
				BasePeakArray = new double[Length][];
				for (int i = 0; i < Length; i++)
				{
					ChromatogramSignal chromatogramSignal = signals[i];
					PositionsArray[i] = chromatogramSignal._signalTimes;
					ScanNumbersArray[i] = chromatogramSignal._signalScans;
					IntensitiesArray[i] = chromatogramSignal._signalIntensities;
					BasePeakArray[i] = chromatogramSignal._signalBasePeakMasses;
				}
			}
			else
			{
				Length = 0;
				PositionsArray = new double[0][];
				ScanNumbersArray = new int[0][];
				IntensitiesArray = new double[0][];
				BasePeakArray = new double[0][];
			}
		}
	}

	/// <summary>
	///     The signal times.
	/// </summary>
	private double[] _signalTimes;

	/// <summary>
	///     The signal base peak masses.
	/// </summary>
	private double[] _signalBasePeakMasses;

	/// <summary>
	///     The signal intensities.
	/// </summary>
	private double[] _signalIntensities;

	/// <summary>
	///     The signal scans.
	/// </summary>
	private int[] _signalScans;

	/// <summary>
	///     Gets or sets the signal times.
	/// </summary>
	/// <value>The signal times.</value>
	public double[] SignalTimes
	{
		get
		{
			return _signalTimes;
		}
		set
		{
			_signalTimes = value;
		}
	}

	/// <summary>
	///     Gets the times.
	/// </summary>
	public IList<double> Times => _signalTimes;

	/// <summary>
	///     Gets or sets the signal intensities.
	/// </summary>
	/// <value>The signal intensities.</value>
	public double[] SignalIntensities
	{
		get
		{
			return _signalIntensities;
		}
		set
		{
			_signalIntensities = value;
		}
	}

	/// <summary>
	///     Gets the intensities.
	/// </summary>
	/// <value>The signal intensities.</value>
	public IList<double> Intensities => _signalIntensities;

	/// <summary>
	///     Gets or sets the signal scans.
	/// </summary>
	/// <value>The signal scans.</value>
	public int[] SignalScans
	{
		get
		{
			return _signalScans;
		}
		set
		{
			_signalScans = value;
		}
	}

	/// <summary>
	///     Gets the signal scans.
	/// </summary>
	/// <value>The signal scans.</value>
	public IList<int> Scans => _signalScans;

	/// <summary>
	///     Gets or sets the signal base peak masses.
	/// </summary>
	/// <value>The signal times.</value>
	public double[] SignalBasePeakMasses
	{
		get
		{
			return _signalBasePeakMasses;
		}
		set
		{
			_signalBasePeakMasses = value;
		}
	}

	/// <summary>
	///     Gets the signal base peak masses.
	/// </summary>
	/// <value>The signal base peak masses. May be null (should not be used) when HasBasePeakData returns false</value>
	public IList<double> BasePeakMasses => _signalBasePeakMasses;

	/// <summary>
	///     Gets the time at the end of the signal
	/// </summary>
	public double EndTime
	{
		get
		{
			if (_signalTimes.Length != 0)
			{
				return _signalTimes[^1];
			}
			return 0.0;
		}
	}

	/// <summary>
	///     Gets the time at the start of the signal
	/// </summary>
	public double StartTime
	{
		get
		{
			if (_signalTimes.Length != 0)
			{
				return _signalTimes[0];
			}
			return 0.0;
		}
	}

	/// <summary>
	/// Gets the time range.
	/// </summary>
	public IRangeAccess TimeRange => RangeFactory.Create(StartTime, EndTime);

	/// <summary>
	///     Gets the number of points in the signal
	/// </summary>
	public int Length => _signalTimes.Length;

	/// <summary>
	///     Gets a value indicating whether there is any base peak data in this signal
	/// </summary>
	public bool HasBasePeakData
	{
		get
		{
			if (_signalBasePeakMasses != null)
			{
				return _signalBasePeakMasses.Length == Length;
			}
			return false;
		}
	}

	/// <summary>
	///     Gets times in minutes for each chromatogram
	/// </summary>
	double[][] IChromatogramData.PositionsArray => ToMulti(SignalTimes);

	/// <summary>
	///     Gets the scan numbers for data points in each chromatogram
	/// </summary>
	int[][] IChromatogramData.ScanNumbersArray => ToMulti(SignalScans);

	/// <summary>
	///     Gets the intensities for each chromatogram
	/// </summary>
	double[][] IChromatogramData.IntensitiesArray => ToMulti(SignalIntensities);

	/// <summary>
	///     Gets the number of chromatograms in this object
	/// </summary>
	int IChromatogramData.Length => 1;

	/// <summary>
	///     Convert chromatogram signals to ChromatogramDataPlus interface
	/// </summary>
	/// <param name="signals">An array of signals</param>
	/// <returns>interface to chromatogram data</returns>
	public static IChromatogramDataPlus ToChromatogramDataPlus(ChromatogramSignal[] signals)
	{
		return new WrappedChromatogramData(signals);
	}

	/// <summary>
	/// Create a Chromatogram signal, from time and intensity arrays
	/// </summary>
	/// <param name="time">array of retention times</param>
	/// <param name="intensity">array of intensities at each time</param>
	/// <returns>The constructed signal, or null if either of the inputs are null, or the inputs are not the same length</returns>
	public static ChromatogramSignal FromTimeAndIntensity(double[] time, double[] intensity)
	{
		if (!ValidateData(time, intensity))
		{
			return null;
		}
		return new ChromatogramSignal(time, intensity);
	}

	/// <summary>
	/// Create a Chromatogram signal, from time,  intensity and scan arrays
	/// </summary>
	/// <param name="time">array of retention times</param>
	/// <param name="intensity">array of intensities at each time</param>
	/// <param name="scan">array of scan numbers for each time</param>
	/// <returns>The constructed signal, or null if either of the inputs are null, or the inputs are not the same length</returns>
	public static ChromatogramSignal FromTimeIntensityScan(double[] time, double[] intensity, int[] scan)
	{
		if (!ValidateData(time, intensity, scan))
		{
			return null;
		}
		return new ChromatogramSignal(time, intensity, scan);
	}

	/// <summary>
	///     Create a Chromatogram signal, from time,  intensity, scan and base peak arrays
	/// </summary>
	/// <param name="time">array of retention times</param>
	/// <param name="intensity">array of intensities at each time</param>
	/// <param name="scan">array of scan numbers for each time</param>
	/// <param name="basePeak">Array of base peak masses for each time</param>
	/// <returns>The constructed signal, or null if either of the inputs are null, or the inputs are not the same length</returns>
	public static ChromatogramSignal FromTimeIntensityScanBasePeak(double[] time, double[] intensity, int[] scan, double[] basePeak)
	{
		if (!ValidateData(time, intensity, scan, basePeak))
		{
			return null;
		}
		return new ChromatogramSignal(time, intensity, scan, basePeak);
	}

	/// <summary>
	///     Create an array of signals from <paramref name="chromatogramData" />. The Interface
	///     <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.IChromatogramData" />
	///     describes data read from a file (if using IRawData). This constructor converts to an array of type Signal,
	///     simplifying use of individual chromatograms with Peak integration.
	/// </summary>
	/// <param name="chromatogramData">data (usually read from file) to convert into signals</param>
	/// <returns>The constructed signals, or null if the input is null</returns>
	public static ChromatogramSignal[] FromChromatogramData(IChromatogramData chromatogramData)
	{
		if (chromatogramData == null)
		{
			throw new ArgumentNullException("chromatogramData");
		}
		int length = chromatogramData.Length;
		ChromatogramSignal[] array = new ChromatogramSignal[length];
		IChromatogramBasePeaks chromatogramBasePeaks = chromatogramData as IChromatogramBasePeaks;
		bool flag = chromatogramBasePeaks?.BasePeakArray != null;
		for (int i = 0; i < length; i++)
		{
			if (flag && chromatogramBasePeaks.BasePeakArray[i] != null)
			{
				array[i] = FromTimeIntensityScanBasePeak(chromatogramData.PositionsArray[i], chromatogramData.IntensitiesArray[i], chromatogramData.ScanNumbersArray[i], chromatogramBasePeaks.BasePeakArray[i]);
			}
			else
			{
				array[i] = FromTimeIntensityScan(chromatogramData.PositionsArray[i], chromatogramData.IntensitiesArray[i], chromatogramData.ScanNumbersArray[i]);
			}
		}
		return array;
	}

	/// <summary>
	///     Create chromatogram data interface from <paramref name="signals" />. The Interface <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.IChromatogramData" />
	///     describes data read from a file (if using IRawData).
	/// </summary>
	/// <param name="signals">data (usually read from file) to convert into signals</param>
	/// <returns>The constructed signals, or null if the input is null</returns>
	public static IChromatogramData ToChromatogramData(ChromatogramSignal[] signals)
	{
		return new WrappedChromatogramData(signals);
	}

	/// <summary>
	///     Creates a new object that is a (deep) copy of the current instance.
	/// </summary>
	/// <returns>
	///     A new object that is a copy of this instance.
	/// </returns>
	public object Clone()
	{
		ChromatogramSignal chromatogramSignal = new ChromatogramSignal();
		if (_signalTimes != null)
		{
			chromatogramSignal._signalTimes = _signalTimes.Clone() as double[];
		}
		if (_signalIntensities != null)
		{
			chromatogramSignal._signalIntensities = _signalIntensities.Clone() as double[];
		}
		if (_signalScans != null)
		{
			chromatogramSignal._signalScans = _signalScans.Clone() as int[];
		}
		return chromatogramSignal;
	}

	/// <summary>
	/// covert array to multi dimension.
	/// </summary>
	/// <param name="data">
	/// The data.
	/// </param>
	/// <returns>
	/// The converted array.
	/// </returns>
	private static double[][] ToMulti(double[] data)
	{
		return new double[1][] { data };
	}

	/// <summary>
	/// covert array to multi dimension.
	/// </summary>
	/// <param name="data">
	/// The data.
	/// </param>
	/// <returns>
	/// The converted array.
	/// </returns>
	private static int[][] ToMulti(int[] data)
	{
		return new int[1][] { data };
	}

	/// <summary>
	///     Test if the signal is valid
	/// </summary>
	/// <returns>True if both times and intensities have been set, and are the same length</returns>
	public bool Valid()
	{
		return ValidateData(_signalTimes, _signalIntensities);
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramSignal" /> class.
	/// </summary>
	/// <param name="signal">Clone from this interface</param>
	public ChromatogramSignal(IChromatogramSignalAccess signal)
	{
		_signalTimes = signal.Times.ToArray();
		_signalIntensities = signal.Intensities.ToArray();
		_signalScans = signal.Scans?.ToArray() ?? new int[0];
		_signalBasePeakMasses = (signal.HasBasePeakData ? signal.BasePeakMasses.ToArray() : new double[0]);
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramSignal" /> class.
	/// </summary>
	/// <param name="times">
	///     The times.
	/// </param>
	/// <param name="intensities">
	///     The intensities.
	/// </param>
	internal ChromatogramSignal(double[] times, double[] intensities)
	{
		_signalTimes = times;
		_signalIntensities = intensities;
		_signalBasePeakMasses = new double[0];
		_signalScans = new int[0];
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramSignal" /> class.
	/// </summary>
	/// <param name="times">
	///     The times.
	/// </param>
	/// <param name="intensities">
	///     The intensities.
	/// </param>
	/// <param name="scans">
	///     The scans.
	/// </param>
	internal ChromatogramSignal(double[] times, double[] intensities, int[] scans)
	{
		_signalTimes = times;
		_signalIntensities = intensities;
		_signalScans = scans;
		_signalBasePeakMasses = null;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramSignal" /> class.
	/// </summary>
	/// <param name="times">
	///     The times.
	/// </param>
	/// <param name="intensities">
	///     The intensities.
	/// </param>
	/// <param name="scans">
	///     The scans.
	/// </param>
	/// <param name="basePeaks">
	///     The base peaks.
	/// </param>
	internal ChromatogramSignal(double[] times, double[] intensities, int[] scans, double[] basePeaks)
	{
		_signalTimes = times;
		_signalIntensities = intensities;
		_signalScans = scans;
		_signalBasePeakMasses = basePeaks;
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramSignal" /> class.
	/// </summary>
	public ChromatogramSignal()
	{
		_signalTimes = new double[0];
		_signalBasePeakMasses = new double[0];
		_signalIntensities = new double[0];
		_signalScans = new int[0];
	}

	/// <summary>
	///     Test if the data is contained in valid arrays of the same size
	/// </summary>
	/// <param name="times">
	///     The times.
	/// </param>
	/// <param name="intensities">
	///     The intensities.
	/// </param>
	/// <returns>
	///     True if both times and intensities have been set, and are the same length
	/// </returns>
	private static bool ValidateData(double[] times, double[] intensities)
	{
		if (times == null || intensities == null)
		{
			return false;
		}
		if (times.Length != intensities.Length)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	///     Test if the data is contained in valid arrays of the same size
	/// </summary>
	/// <param name="times">
	///     The times.
	/// </param>
	/// <param name="intensities">
	///     The intensities.
	/// </param>
	/// <param name="scans">
	///     The scans.
	/// </param>
	/// <returns>
	///     True if all of times intensities and scans have been set, and are the same length
	/// </returns>
	private static bool ValidateData(double[] times, double[] intensities, int[] scans)
	{
		if (times == null || intensities == null || scans == null)
		{
			return false;
		}
		if (times.Length != intensities.Length)
		{
			return false;
		}
		if (times.Length != scans.Length)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	///     Test if the data is contained in valid arrays of the same size
	/// </summary>
	/// <param name="times">
	///     The times.
	/// </param>
	/// <param name="intensities">
	///     The intensities.
	/// </param>
	/// <param name="scans">
	///     The scans.
	/// </param>
	/// <param name="basePeaks">
	///     The base Peaks.
	/// </param>
	/// <returns>
	///     True if all of times intensities and scans have been set, and are the same length
	/// </returns>
	private static bool ValidateData(double[] times, double[] intensities, int[] scans, double[] basePeaks)
	{
		if (times == null || intensities == null || scans == null || basePeaks == null)
		{
			return false;
		}
		if (times.Length != intensities.Length)
		{
			return false;
		}
		if (times.Length != scans.Length)
		{
			return false;
		}
		if (times.Length != basePeaks.Length)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Add a delay to all times. This is intended to support "detector delays" where
	/// multiple detector see the same sample at different times.
	/// </summary>
	/// <param name="delay">
	/// The delay.
	/// </param>
	public void Delay(double delay)
	{
		for (int i = 0; i < _signalTimes.Length; i++)
		{
			_signalTimes[i] += delay;
		}
	}
}
