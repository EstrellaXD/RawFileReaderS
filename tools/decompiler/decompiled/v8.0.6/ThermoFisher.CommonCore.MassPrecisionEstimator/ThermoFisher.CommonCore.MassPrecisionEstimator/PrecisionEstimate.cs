using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.MassPrecisionEstimator;

/// <summary>
/// Class to calculate the mass precision estimates (MPE) for a specific scan
/// </summary>
public class PrecisionEstimate : IPrecisionEstimate, IDisposable
{
	/// <summary>
	/// Values that represent the ion trap scan types.
	/// </summary>
	private enum TrapScanType
	{
		/// <summary>
		/// The type for a normal scan
		/// </summary>
		Normal,
		/// <summary>
		/// The type for an enhanced scan
		/// </summary>
		Enhanced,
		/// <summary>
		/// The type for a zoom scan
		/// </summary>
		Zoom,
		/// <summary>
		/// The type for an ultra zoom scan
		/// </summary>
		UltraZoom
	}

	/// <summary>
	/// List containing the results from the mass precision estimate calculation.
	/// </summary>
	private readonly List<EstimatorResults> _estimatorResults;

	/// <summary>
	/// The RAW file object.
	/// </summary>
	private IRawDataPlus _rawFile;

	/// <summary>
	/// The scan resolution.
	/// </summary>
	private double _scanResolution;

	/// <summary>
	/// The ion time.
	/// </summary>
	private double _ionTime;

	/// <summary>
	/// The scan number.
	/// </summary>
	private int _scanNumber;

	/// <summary>
	/// Sets the raw file as an open IRawData objects.
	/// </summary>
	public IRawDataPlus Rawfile
	{
		set
		{
			_rawFile = value;
		}
	}

	/// <summary>
	/// Sets the scan number of the scan to be analyzed
	/// </summary>
	/// <value> The scan number. </value>
	public int ScanNumber
	{
		set
		{
			_scanNumber = value;
		}
	}

	/// <summary>
	/// Gets the ion trap scan type.
	/// </summary>
	/// <param name="scanFilter">
	/// The scan filter to process
	/// </param>
	/// <returns>
	/// The ion trap scan type as an enumerated value.
	/// </returns>
	private static TrapScanType GetIonTrapScanType(string scanFilter)
	{
		if (string.IsNullOrEmpty(scanFilter))
		{
			return TrapScanType.Normal;
		}
		if (scanFilter.ToLowerInvariant().Contains(" e "))
		{
			return TrapScanType.Enhanced;
		}
		if (scanFilter.ToLowerInvariant().Contains(" z ") && scanFilter.ToLowerInvariant().Contains(" u "))
		{
			return TrapScanType.UltraZoom;
		}
		if (!scanFilter.ToLowerInvariant().Contains(" z "))
		{
			return TrapScanType.Normal;
		}
		return TrapScanType.Zoom;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.MassPrecisionEstimator.PrecisionEstimate" /> class. 
	/// </summary>
	public PrecisionEstimate()
	{
		_rawFile = null;
		_scanNumber = 0;
		_estimatorResults = new List<EstimatorResults>();
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting
	/// unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		_estimatorResults.Clear();
	}

	/// <summary>
	/// Calculate the ion time (fill (traps and FT) or dwell time (quads))
	/// </summary>
	/// <param name="analyzerType">The analyzer type</param>
	/// <param name="scan">The scan to process</param>
	/// <param name="trailerHeadings">The trailer extra data headings</param>
	/// <param name="trailerValues">The trailer extra data values</param>
	/// <returns> The calculated ion time</returns>
	public double GetIonTime(MassAnalyzerType analyzerType, Scan scan, List<string> trailerHeadings, List<string> trailerValues)
	{
		if (trailerValues.Count < 1)
		{
			throw new ArgumentException("No trailer data to process!");
		}
		if (analyzerType == MassAnalyzerType.MassAnalyzerFTMS || analyzerType == MassAnalyzerType.MassAnalyzerITMS)
		{
			for (int i = 0; i < trailerValues.Count; i++)
			{
				string text = trailerHeadings[i];
				string value = trailerValues[i];
				if (text.ToLowerInvariant().Contains("ion injection time"))
				{
					return Convert.ToDouble(value) / 1000.0;
				}
			}
		}
		double lowMass = scan.ScanStatistics.LowMass;
		double highMass = scan.ScanStatistics.HighMass;
		double num = 0.0;
		for (int j = 0; j < trailerValues.Count; j++)
		{
			string text2 = trailerHeadings[j];
			string value2 = trailerValues[j];
			if (text2.ToLowerInvariant().Contains("elapsed scan time"))
			{
				num = Convert.ToDouble(value2);
				break;
			}
		}
		if (num < 1E-05)
		{
			num = 1.0;
		}
		return num / (highMass - lowMass);
	}

	/// <summary>
	/// Gets mass precision estimate and stores them in a class property list of classes
	/// This method will throw an Exception or ArgumentException if a problem occurs
	/// during processing.
	/// </summary>
	/// <param name="scan">
	/// The scan to process
	/// </param>
	/// <param name="analyzerType">
	/// The analyzer type for the scan
	/// </param>
	/// <param name="ionTime">
	/// The ion time for the scan
	/// </param>
	/// <param name="resolution">
	/// The resolution for the scan
	/// </param>
	/// <returns>
	/// Returns the list of Mass Precision Estimation results
	/// </returns>
	public List<EstimatorResults> GetMassPrecisionEstimate(Scan scan, MassAnalyzerType analyzerType, double ionTime, double resolution)
	{
		_scanResolution = resolution;
		_ionTime = ionTime;
		if (!PopulateScanData(scan, analyzerType))
		{
			throw new Exception("Unable to populate the scan data!");
		}
		CalculateMassPrecisionEstimateFromFormula();
		CompareMassAccuracyToTables(analyzerType, scan.ScanType);
		return _estimatorResults;
	}

	/// <summary>
	/// Gets mass precision estimate and stores them in a class property list of classes
	/// This method will throw an Exception or ArgumentException if a problem occurs
	/// during processing.
	/// </summary>
	/// <returns>
	/// Returns the list of Mass Precision Estimation results
	/// </returns>
	public List<EstimatorResults> GetMassPrecisionEstimate()
	{
		if (_rawFile == null || !_rawFile.IsOpen)
		{
			throw new Exception("No open RAW file to process");
		}
		_rawFile.SelectInstrument(Device.MS, 1);
		int firstSpectrum = _rawFile.RunHeader.FirstSpectrum;
		int lastSpectrum = _rawFile.RunHeader.LastSpectrum;
		if (_scanNumber < firstSpectrum || _scanNumber > lastSpectrum)
		{
			throw new ArgumentException("The specified scan number was out of range for the RAW file");
		}
		_scanResolution = GetScanResolution();
		IScanEvent scanEventForScanNumber = _rawFile.GetScanEventForScanNumber(_scanNumber);
		ILogEntryAccess trailerExtraInformation = _rawFile.GetTrailerExtraInformation(_scanNumber);
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		for (int i = 0; i < trailerExtraInformation.Length; i++)
		{
			list.Add(trailerExtraInformation.Labels[i]);
			list2.Add(trailerExtraInformation.Values[i]);
		}
		Scan scan = Scan.FromFile(_rawFile, _scanNumber);
		_ionTime = GetIonTime(scanEventForScanNumber.MassAnalyzer, scan, list, list2);
		return GetMassPrecisionEstimate(scan, scanEventForScanNumber.MassAnalyzer, _ionTime, _scanResolution);
	}

	/// <summary>
	/// Populate the mass precision estimator data list from raw or label data 
	/// as appropriate for the scan data.
	/// </summary>
	/// <param name="scan">The scan</param>
	/// <param name="analyzerType"> The analyzer type </param>
	/// <returns>Returns true or false based upon if the scan could be filled in</returns>
	private bool PopulateScanData(Scan scan, MassAnalyzerType analyzerType)
	{
		bool flag = false;
		_estimatorResults.Clear();
		if (analyzerType != MassAnalyzerType.MassAnalyzerFTMS)
		{
			if (scan.PreferredMasses == null || scan.PreferredMasses.Length == 0)
			{
				return false;
			}
			if (_scanResolution < 0.1 || _scanResolution > 10000000.0)
			{
				_scanResolution = 0.1;
			}
			for (int i = 0; i < scan.PreferredMasses.Length; i++)
			{
				double resolution = scan.PreferredMasses[i] / _scanResolution;
				EstimatorResults item = new EstimatorResults
				{
					Mass = scan.PreferredMasses[i],
					Intensity = scan.PreferredIntensities[i],
					Resolution = resolution,
					MassAccuracyInMmu = 0.0,
					MassAccuracyInPpm = 0.0
				};
				_estimatorResults.Add(item);
			}
		}
		else
		{
			if (analyzerType == MassAnalyzerType.MassAnalyzerFTMS && !scan.HasCentroidStream)
			{
				return false;
			}
			if (scan.CentroidScan.Masses == null || scan.CentroidScan.Masses.Length == 0)
			{
				return false;
			}
			for (int j = 0; j < scan.CentroidScan.Length; j++)
			{
				EstimatorResults estimatorResults = new EstimatorResults
				{
					Mass = scan.CentroidScan.Masses[j],
					Intensity = scan.CentroidScan.Intensities[j],
					Resolution = scan.CentroidScan.Resolutions[j],
					MassAccuracyInMmu = 0.0,
					MassAccuracyInPpm = 0.0
				};
				if (estimatorResults.Resolution < 0.01)
				{
					flag = true;
				}
				_estimatorResults.Add(estimatorResults);
			}
		}
		return !flag;
	}

	/// <summary>
	/// Calculate the MPE for the scan from formula.
	/// </summary>
	private void CalculateMassPrecisionEstimateFromFormula()
	{
		foreach (EstimatorResults estimatorResult in _estimatorResults)
		{
			double mass = estimatorResult.Mass;
			double intensity = estimatorResult.Intensity;
			double ionTime = _ionTime;
			double resolution = estimatorResult.Resolution;
			double num = intensity * ionTime;
			if (num < 1.0)
			{
				num = 1.0;
			}
			double num2 = mass / (2.0 * resolution * Math.Sqrt(num));
			estimatorResult.MassAccuracyInMmu = num2 * 1000.0;
			estimatorResult.MassAccuracyInPpm = num2 * 1000000.0 / mass;
		}
	}

	/// <summary>
	/// Compare the calculated MPE to tables of information from Alexander and Jae
	/// </summary>
	/// <param name="analyzerType"> the analyzer type. </param>
	/// <param name="scanFilter"> the scan filter. </param>
	private void CompareMassAccuracyToTables(MassAnalyzerType analyzerType, string scanFilter)
	{
		switch (analyzerType)
		{
		case MassAnalyzerType.MassAnalyzerFTMS:
		case MassAnalyzerType.MassAnalyzerASTMS:
		{
			foreach (EstimatorResults estimatorResult in _estimatorResults)
			{
				double num3 = 0.0;
				double mass2 = estimatorResult.Mass;
				double intensity2 = estimatorResult.Intensity;
				double massAccuracyInPpm = estimatorResult.MassAccuracyInPpm;
				double num4 = intensity2 * _ionTime;
				if (num4 < 1.0)
				{
					num4 = 1.0;
				}
				if (mass2 < 125.1)
				{
					num3 += 5.0;
				}
				num3 = ((num4 < 11.0) ? (num3 + 5.0) : ((!(num4 < 101.0)) ? (num3 + 3.0) : (num3 + 4.0)));
				if (massAccuracyInPpm < num3)
				{
					estimatorResult.MassAccuracyInPpm = num3;
					estimatorResult.MassAccuracyInMmu = num3 * mass2 / 1000.0;
				}
			}
			return;
		}
		case MassAnalyzerType.MassAnalyzerITMS:
		{
			TrapScanType ionTrapScanType = GetIonTrapScanType(scanFilter);
			{
				foreach (EstimatorResults estimatorResult2 in _estimatorResults)
				{
					double num = 0.0;
					double mass = estimatorResult2.Mass;
					double intensity = estimatorResult2.Intensity;
					double massAccuracyInMmu = estimatorResult2.MassAccuracyInMmu;
					double num2 = intensity * _ionTime;
					if (num2 < 1.0)
					{
						num2 = 1.0;
					}
					if (num2 >= 100.0)
					{
						num = ionTrapScanType switch
						{
							TrapScanType.Enhanced => 100.0, 
							TrapScanType.Zoom => 50.0, 
							TrapScanType.UltraZoom => 50.0, 
							_ => 100.0, 
						};
					}
					else if (num2 > 10.0 && num2 < 100.0)
					{
						num = ionTrapScanType switch
						{
							TrapScanType.Enhanced => 150.0, 
							TrapScanType.Zoom => 87.0, 
							TrapScanType.UltraZoom => 60.0, 
							_ => 300.0, 
						};
					}
					else if (num2 <= 10.0)
					{
						num = ionTrapScanType switch
						{
							TrapScanType.Enhanced => 200.0, 
							TrapScanType.Zoom => 125.0, 
							TrapScanType.UltraZoom => 70.0, 
							_ => 500.0, 
						};
					}
					if (massAccuracyInMmu < num)
					{
						estimatorResult2.MassAccuracyInPpm = num * 1000.0 / mass;
						estimatorResult2.MassAccuracyInMmu = num;
					}
				}
				return;
			}
		}
		}
		foreach (EstimatorResults estimatorResult3 in _estimatorResults)
		{
			double num5 = 0.0;
			double mass3 = estimatorResult3.Mass;
			double intensity3 = estimatorResult3.Intensity;
			double massAccuracyInMmu2 = estimatorResult3.MassAccuracyInMmu;
			double num6 = intensity3 * _ionTime;
			if (num6 < 1.0)
			{
				num6 = 1.0;
			}
			if (num6 >= 100.0)
			{
				num5 = 100.0;
			}
			else if (num6 > 10.0 && num6 < 100.0)
			{
				num5 = 200.0;
			}
			else if (num6 <= 10.0)
			{
				num5 = 300.0;
			}
			if (massAccuracyInMmu2 < num5)
			{
				estimatorResult3.MassAccuracyInPpm = num5 * 1000.0 / mass3;
				estimatorResult3.MassAccuracyInMmu = num5;
			}
		}
	}

	/// <summary>
	/// Gets the scan resolution from the file header 
	/// </summary>
	/// <returns>The resolution from the run header.</returns>
	private double GetScanResolution()
	{
		return Math.Max(0.0, _rawFile.RunHeader.MassResolution);
	}
}
