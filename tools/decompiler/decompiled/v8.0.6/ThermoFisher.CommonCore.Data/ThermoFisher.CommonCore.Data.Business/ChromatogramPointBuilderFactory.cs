using System;
using System.Collections.Generic;
using System.Linq;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Class to create objects to efficiently generate chromatogram points.
/// </summary>
public static class ChromatogramPointBuilderFactory
{
	/// <summary>
	/// Standard properties for chromatogram point builders
	/// </summary>
	public class ChromatogramPointBuilderBase
	{
		/// <summary>
		/// Gets or sets the retention time range.
		/// Only scans within this range are included.
		/// </summary>
		public IRangeAccess RetentionTimeRange { get; set; }

		/// <summary>
		/// Gets or sets the scan selector, which determines if a scan is in the chromatogram, or not
		/// </summary>
		public IScanSelect ScanSelector { get; set; }
	}

	/// <summary>
	/// Implementation of IChromatogramRequest,
	/// which can generate chromatogram points for mass ranges.
	/// Version which supports multiple mass ranges.
	/// This version sums values for the listed ranges
	/// </summary>
	public class ChromatogramPointBuilder : ChromatogramPointBuilderBase, IChromatogramRequest
	{
		/// <summary>
		/// Gets a value indicating whether this point type needs "scan data".
		/// Always true for this type.
		/// </summary>
		public bool RequiresScanData => true;

		/// <summary>
		/// Gets or sets the point requests.
		/// These determine how chromatogram points are created from a scan,
		/// by adding or subtracting data form multiple mass ranges.
		/// </summary>
		public IList<IChromatogramPointRequest> PointRequests { get; set; }

		/// <summary>
		/// Gets the value for scan.
		/// This function returns the chromatogram value for a scan.
		/// For example: An XIC from the scan data.
		/// </summary>
		/// <param name="scan">
		/// The scan.
		/// </param>
		/// <returns>
		/// The chromatogram value of this scan.
		/// </returns>
		public double ValueForScan(ISimpleScanWithHeader scan)
		{
			return PointRequests.Sum((IChromatogramPointRequest pointRequest) => pointRequest.Scale * pointRequest.DataForPoint(scan));
		}
	}

	/// <summary>
	/// This chromatogram request finds base peak mass and intensity data.
	/// It can return data for the full file or a mass range
	/// </summary>
	public class ChromatogramBasePeakMassRequest
	{
		/// <summary>
		/// Gets or sets a value indicating whether all data in the scan is used
		/// </summary>
		public bool AllData { get; set; }

		/// <summary>
		/// Get os sets the mass range analyzed, when not "AllData"
		/// </summary>
		public IRangeAccess MassRange { get; set; }

		/// <summary>
		/// Finds the Mass and Intensity of the base peak
		/// </summary>
		/// <param name="scanWithHeader"></param>
		/// <returns>touple containing mass, intensity</returns>
		public Tuple<double, double> DataForPoint(ISimpleScanWithHeader scanWithHeader)
		{
			ISimpleScanAccess data = scanWithHeader.Data;
			if (AllData)
			{
				return data.MassAndIntensityAtLargestIntensity();
			}
			IRangeAccess massRange = MassRange;
			return data.MassAndIntensityAtLargestIntensity(massRange.Low, massRange.High);
		}

		/// <summary>
		/// Create a request to make an "base peak" mass chromatogram, based on a mass range.
		/// That is: returns the mass of most intense peak over a given range.
		/// </summary>
		/// <param name="lowMass">
		/// The low mass.
		/// </param>
		/// <param name="highMass">
		/// The high mass.
		/// </param>
		/// <returns>
		/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ChromatogramPointRequest" />.
		/// </returns>
		public static ChromatogramBasePeakMassRequest BasePeakMassOverMassRangeRequest(double lowMass, double highMass)
		{
			return new ChromatogramBasePeakMassRequest
			{
				AllData = false,
				MassRange = RangeFactory.Create(lowMass, highMass)
			};
		}
	}

	/// <summary>
	/// Implementation of IChromatogramRequest,
	/// which can generate chromatogram points for mass ranges.
	/// Version which supports multiple mass ranges.
	/// This version finds the Max of the values for all range (base peak)
	/// </summary>
	public class ChromatogramPointBuilderMax : ChromatogramPointBuilderBase, IChromatogramRequest
	{
		/// <summary>
		/// Gets a value indicating whether this point type needs "scan data".
		/// Always true for this type.
		/// </summary>
		public bool RequiresScanData => true;

		/// <summary>
		/// Gets or sets the point requests.
		/// These determine how chromatogram points are created from a scan,
		/// by adding or subtracting data form multiple mass ranges.
		/// </summary>
		public IList<IChromatogramPointRequest> PointRequests { get; set; }

		/// <summary>
		/// Gets the value for scan.
		/// This function returns the chromatogram value for a scan.
		/// For example: An XIC from the scan data.
		/// </summary>
		/// <param name="scan">
		/// The scan.
		/// </param>
		/// <returns>
		/// The chromatogram value of this scan.
		/// </returns>
		public double ValueForScan(ISimpleScanWithHeader scan)
		{
			return PointRequests.Max((IChromatogramPointRequest pointRequest) => pointRequest.Scale * pointRequest.DataForPoint(scan));
		}
	}

	/// <summary>
	/// Implementation of IChromatogramRequest,
	/// which can generate chromatogram points for mass ranges.
	/// Version which supports multiple mass ranges.
	/// This version finds the Max of the values for all range (base peak)
	/// </summary>
	public class ChromatogramPointBuilderMassOfMax : ChromatogramPointBuilderBase, IChromatogramRequest
	{
		/// <summary>
		/// Gets a value indicating whether this point type needs "scan data".
		/// Always true for this type.
		/// </summary>
		public bool RequiresScanData => true;

		/// <summary>
		/// Gets or sets the point requests.
		/// These determine how chromatogram points are created from a scan,
		/// by adding or subtracting data form multiple mass ranges.
		/// </summary>
		public IList<ChromatogramBasePeakMassRequest> PointRequests { get; set; }

		/// <summary>
		/// Gets the value for scan.
		/// This function returns the chromatogram value for a scan.
		/// For example: An XIC from the scan data.
		/// </summary>
		/// <param name="scan">
		/// The scan.
		/// </param>
		/// <returns>
		/// The chromatogram value of this scan.
		/// </returns>
		public double ValueForScan(ISimpleScanWithHeader scan)
		{
			if (PointRequests.Count == 0)
			{
				return 0.0;
			}
			if (PointRequests.Count == 1)
			{
				return PointRequests[0].DataForPoint(scan).Item1;
			}
			Tuple<double, double> tuple = PointRequests[0].DataForPoint(scan);
			double item = tuple.Item1;
			double item2 = tuple.Item2;
			for (int i = 1; i < PointRequests.Count; i++)
			{
				Tuple<double, double> tuple2 = PointRequests[i].DataForPoint(scan);
				if (tuple2.Item2 > item2)
				{
					item2 = tuple2.Item2;
					item = tuple2.Item1;
				}
			}
			return item;
		}
	}

	/// <summary>
	/// Implementation of IChromatogramRequest,
	/// which can generate chromatogram points for mass ranges.
	/// Simplified version for one mass range.
	/// </summary>
	public class SimpleChromatogramPointBuilder : ChromatogramPointBuilderBase, IChromatogramRequest
	{
		/// <summary>
		/// Gets a value indicating whether this point type needs "scan data".
		/// Always true for this type.
		/// </summary>
		public bool RequiresScanData => true;

		/// <summary>
		/// Gets or sets the point requests
		/// Determine how a chromatogram point is created from a scan.
		/// </summary>
		public IChromatogramPointRequest PointRequest { get; set; }

		/// <summary>
		/// Gets the value for scan.
		/// This function returns the chromatogram value for a scan.
		/// For example: An XIC from the scan data.
		/// </summary>
		/// <param name="scan">
		/// The scan.
		/// </param>
		/// <returns>
		/// The chromatogram value of this scan.
		/// </returns>
		public double ValueForScan(ISimpleScanWithHeader scan)
		{
			return PointRequest.Scale * PointRequest.DataForPoint(scan);
		}
	}

	/// <summary>
	/// Implementation of IChromatogramRequest,
	/// which can generate chromatogram points for mass ranges.
	/// Simplified version for one mass range, and no scaling
	/// </summary>
	public class SimpleChromatogramPointBuilderUnscaled : ChromatogramPointBuilderBase, IChromatogramRequest
	{
		/// <summary>
		/// Gets a value indicating whether this point type needs "scan data".
		/// Always true for this type.
		/// </summary>
		public bool RequiresScanData => true;

		/// <summary>
		/// Gets or sets the point requests
		/// Determine how a chromatogram point is created from a scan.
		/// </summary>
		public IChromatogramPointRequest PointRequest { get; set; }

		/// <summary>
		/// Gets the value for scan.
		/// This function returns the chromatogram value for a scan.
		/// For example: An XIC from the scan data.
		/// </summary>
		/// <param name="scan">
		/// The scan.
		/// </param>
		/// <returns>
		/// The chromatogram value of this scan.
		/// </returns>
		public double ValueForScan(ISimpleScanWithHeader scan)
		{
			return PointRequest.DataForPoint(scan);
		}
	}

	/// <summary>
	/// create an object to efficiently generate chromatogram points.
	/// This analyzes the given arguments, and returns an interface which can most efficiently
	/// generate the chromatograms
	/// </summary>
	/// <param name="retentionTimeRange">Retention time range for this chromatogram</param>
	/// <param name="scanSelector">Method of selecting scans to be included</param>
	/// <param name="pointRequests">Methods to generate points from scans</param>
	/// <returns>Object to make chromatogram points</returns>
	public static IChromatogramRequest CreatePointBuilder(IRangeAccess retentionTimeRange, IScanSelect scanSelector, IList<IChromatogramPointRequest> pointRequests)
	{
		if (pointRequests.Count > 1)
		{
			return new ChromatogramPointBuilder
			{
				PointRequests = pointRequests,
				RetentionTimeRange = retentionTimeRange,
				ScanSelector = scanSelector
			};
		}
		if (pointRequests.Count == 1)
		{
			IChromatogramPointRequest chromatogramPointRequest = pointRequests[0];
			if (Math.Abs(chromatogramPointRequest.Scale - 1.0) < 1E-08)
			{
				return new SimpleChromatogramPointBuilderUnscaled
				{
					PointRequest = chromatogramPointRequest,
					RetentionTimeRange = retentionTimeRange,
					ScanSelector = scanSelector
				};
			}
			return new SimpleChromatogramPointBuilder
			{
				PointRequest = chromatogramPointRequest,
				RetentionTimeRange = retentionTimeRange,
				ScanSelector = scanSelector
			};
		}
		return null;
	}

	/// <summary>
	/// create an object to efficiently generate chromatogram points.
	/// This analyzes the given arguments, and returns an interface which can most efficiently
	/// generate the chromatograms.
	/// This version will find the "max value" from each of the supplied point requests (base peak)
	/// </summary>
	/// <param name="retentionTimeRange">Retention time range for this chromatogram</param>
	/// <param name="scanSelector">Method of selecting scans to be included</param>
	/// <param name="pointRequests">Methods to generate points from scans</param>
	/// <returns>Object to make chromatogram points</returns>
	public static IChromatogramRequest CreatePointBuilderMax(IRangeAccess retentionTimeRange, IScanSelect scanSelector, IList<IChromatogramPointRequest> pointRequests)
	{
		if (pointRequests.Count > 1)
		{
			return new ChromatogramPointBuilderMax
			{
				PointRequests = pointRequests,
				RetentionTimeRange = retentionTimeRange,
				ScanSelector = scanSelector
			};
		}
		if (pointRequests.Count == 1)
		{
			IChromatogramPointRequest chromatogramPointRequest = pointRequests[0];
			if (Math.Abs(chromatogramPointRequest.Scale - 1.0) < 1E-08)
			{
				return new SimpleChromatogramPointBuilderUnscaled
				{
					PointRequest = chromatogramPointRequest,
					RetentionTimeRange = retentionTimeRange,
					ScanSelector = scanSelector
				};
			}
			return new SimpleChromatogramPointBuilder
			{
				PointRequest = chromatogramPointRequest,
				RetentionTimeRange = retentionTimeRange,
				ScanSelector = scanSelector
			};
		}
		return null;
	}

	/// <summary>
	/// create an object to efficiently generate chromatogram points.
	/// This analyzes the given arguments, and returns an interface which can most efficiently
	/// generate the chromatograms.
	/// This version will find the "max value" from each of the supplied point requests (base peak)
	/// </summary>
	/// <param name="retentionTimeRange">Retention time range for this chromatogram</param>
	/// <param name="scanSelector">Method of selecting scans to be included</param>
	/// <param name="pointRequests">Methods to generate points from scans</param>
	/// <returns>Object to make chromatogram points</returns>
	public static IChromatogramRequest CreatePointBuilderMassOfMax(IRangeAccess retentionTimeRange, IScanSelect scanSelector, IList<ChromatogramBasePeakMassRequest> pointRequests)
	{
		if (pointRequests.Count >= 1)
		{
			return new ChromatogramPointBuilderMassOfMax
			{
				PointRequests = pointRequests,
				RetentionTimeRange = retentionTimeRange,
				ScanSelector = scanSelector
			};
		}
		return null;
	}
}
