using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Extends the "Scan" object to provide charge envolope information,
/// which is available from certain instrumnets.
/// </summary>
public class ExtendedScan : Scan, IExtendedScanAccess, IScanAccess, IExtendedCentroids, ICentroidStreamAccess, ISimpleScanAccess
{
	/// <summary>
	/// Gets centroids with additional charge envelope information (when available)
	/// </summary>
	public IExtendedCentroids ExtendedCentroidsAccess => this;

	/// <summary>
	/// Gets a value indicating whether charge envelope data 
	/// was recorded for this scan
	/// </summary>
	public bool HasChargeEnvelopes { get; set; }

	/// <summary>
	/// Gets additional annotations per peak, related to change envelopes
	/// </summary>
	public IApdPeakAnnotation[] Annotations { get; set; }

	/// <summary>
	/// Gets the change envelopes. This include overall information
	/// about the envelope, plus the set of included peaks, 
	/// </summary>
	public IChargeEnvelope[] ChargeEnvelopes { get; set; }

	int ICentroidStreamAccess.ScanNumber => base.CentroidStreamAccess.ScanNumber;

	int ICentroidStreamAccess.Length => base.CentroidStreamAccess.Length;

	int ICentroidStreamAccess.CoefficientsCount => base.CentroidStreamAccess.CoefficientsCount;

	double[] ICentroidStreamAccess.Coefficients => base.CentroidStreamAccess.Coefficients;

	double[] ICentroidStreamAccess.Resolutions => base.CentroidStreamAccess.Resolutions;

	double[] ICentroidStreamAccess.Baselines => base.CentroidStreamAccess.Baselines;

	double[] ICentroidStreamAccess.Noises => base.CentroidStreamAccess.Noises;

	double[] ICentroidStreamAccess.Charges => base.CentroidStreamAccess.Charges;

	PeakOptions[] ICentroidStreamAccess.Flags => base.CentroidStreamAccess.Flags;

	double[] ISimpleScanAccess.Masses => base.CentroidStreamAccess.Masses;

	double[] ISimpleScanAccess.Intensities => base.CentroidStreamAccess.Intensities;

	/// <summary>
	/// Creates a new ExtendedScan from a Scan.
	/// All extended data is empty
	/// </summary>
	/// <param name="scan"></param>
	public ExtendedScan(Scan scan)
		: base(scan)
	{
	}

	/// <summary>
	/// Make a deep clone of this extended scan.
	/// </summary>
	/// <returns>
	/// An object containing all data in the input, and no shared references
	/// </returns>
	public new ExtendedScan DeepClone()
	{
		ExtendedScan extendedScan = new ExtendedScan();
		extendedScan.DeepCopyFrom(this);
		if (base.CentroidScan is ScanDataExtensions.ExtendedCentroids extended)
		{
			ScanDataExtensions.ExtendedCentroids extendedCentroids = (ScanDataExtensions.ExtendedCentroids)(extendedScan.CentroidScan = ScanDataExtensions.ExtendedCentroids.CopyFrom(extended));
			extendedScan.Annotations = extendedCentroids.Annotations;
			extendedScan.ChargeEnvelopes = extendedCentroids.ChargeEnvelopes;
			extendedScan.HasChargeEnvelopes = extendedCentroids.HasChargeEnvelopes;
		}
		return extendedScan;
	}

	/// <summary>
	/// Default constructor for Extended Scan
	/// </summary>
	public ExtendedScan()
	{
	}

	/// <summary>
	/// Create an extended scan object from a file and a scan number.
	/// </summary>
	/// <param name="rawFile">
	/// File to read from
	/// </param>
	/// <param name="scanNumber">
	/// Scan number to read
	/// </param>
	/// <returns>
	/// The scan read, or null of the scan number if not valid
	/// </returns>
	public static ExtendedScan FromFile(IRawDataExtended rawFile, int scanNumber)
	{
		if (rawFile == null)
		{
			throw new ArgumentNullException("rawFile");
		}
		ExtendedScan extendedScan = null;
		if (scanNumber > 0)
		{
			ScanStatistics scanStatsForScanNumber = rawFile.GetScanStatsForScanNumber(scanNumber);
			if (scanStatsForScanNumber != null)
			{
				extendedScan = new ExtendedScan();
				extendedScan.ReadFromFileExtended(rawFile, scanNumber, scanStatsForScanNumber);
			}
		}
		return extendedScan;
	}

	/// <summary>
	/// read extended scan data from file.
	/// </summary>
	/// <param name="rawFile">
	/// The raw file.
	/// </param>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="stats">
	/// The stats.
	/// </param>
	private void ReadFromFileExtended(IRawDataExtended rawFile, int scanNumber, ScanStatistics stats)
	{
		InstrumentSelection selectedInstrument = rawFile.SelectedInstrument;
		if (selectedInstrument == null)
		{
			base.SegmentedScan = new SegmentedScan();
			base.CentroidScan = new CentroidStream();
			base.ScanStatistics = new ScanStatistics();
			base.ScanType = string.Empty;
			return;
		}
		base.SegmentedScan = rawFile.GetSegmentedScanFromScanNumber(scanNumber) ?? new SegmentedScan();
		if (selectedInstrument.DeviceType == Device.MS)
		{
			IExtendedScanData extendedScanData = rawFile.GetExtendedScanData(scanNumber);
			ScanDataExtensions.ExtendedCentroids extendedCentroids = (ScanDataExtensions.ExtendedCentroids)(base.CentroidScan = ScanDataExtensions.GetExtendedCentroids(rawFile, extendedScanData, scanNumber, rawFile.IncludeReferenceAndExceptionData));
			HasChargeEnvelopes = extendedCentroids.HasChargeEnvelopes;
			Annotations = extendedCentroids.Annotations;
			ChargeEnvelopes = extendedCentroids.ChargeEnvelopes;
		}
		else
		{
			base.CentroidScan = new CentroidStream();
		}
		base.ScanStatistics = new ScanStatistics(stats);
		base.ScanType = stats.ScanType;
		base.ScanStatistics.ScanType = base.ScanType;
		IRunHeader runHeaderEx = rawFile.RunHeaderEx;
		switch (runHeaderEx.ToleranceUnit)
		{
		case ToleranceUnits.mmu:
			base.ToleranceUnit = ToleranceMode.Mmu;
			break;
		case ToleranceUnits.ppm:
			base.ToleranceUnit = ToleranceMode.Ppm;
			break;
		case ToleranceUnits.amu:
			base.ToleranceUnit = ToleranceMode.Amu;
			break;
		default:
			base.ToleranceUnit = ToleranceMode.None;
			break;
		}
		base.MassResolution = runHeaderEx.MassResolution;
	}

	/// <summary>
	/// Return a slice of a scan which only contains data within the supplied mass Range or ranges.
	/// For example: For a scan with data from m/z 200 to 700, and a single mass range of 300 to 400:
	/// This returns a new scan containing all data with the range 300 to 400.
	/// All annotaion and charge envolope data is discarded, as peaks whcih make up envelopes are 
	/// not guranteed to be in any slice.
	/// </summary>
	/// <param name="massRanges">The mass ranges, where data should be retained. When multiple ranges are supplied,
	/// all data which is in at least one range is included in the returned scan</param>
	/// <param name="trimMassRange">If this is true, then the scan will reset the
	/// scan's mass range to the bounds of the supplied mass ranges </param>
	/// <param name="expandProfiles">This setting only applies when the scan has both profile and centroid data.
	/// If true: When there isa centroid near the start or end of a range, and the first or
	/// final "above zero" section of the profile includes that peak, then the profile is extended, to include the points
	/// which contribute to that peak. A maximum of 10 points may be added</param>
	/// <returns>A copy of the scan, with only the data in the supplied ranges</returns>
	public new ExtendedScan Slice(IRangeAccess[] massRanges, bool trimMassRange = false, bool expandProfiles = true)
	{
		return new ExtendedScan(base.Slice(massRanges, trimMassRange, expandProfiles));
	}

	IList<ICentroidPeak> ICentroidStreamAccess.GetCentroids()
	{
		return base.CentroidStreamAccess.GetCentroids();
	}
}
