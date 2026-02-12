using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.DataModel;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Class to convert from "user" settings for a chromatogram
/// into objects ready for use by the chromatogram generator
/// </summary>
internal class MsChromatogramSettingsConverter : IExecute
{
	/// <summary>
	/// Gets the delivery, configured to generate a chromatogram
	/// </summary>
	public ChromatogramDelivery[] Delivery { get; private set; }

	/// <summary>
	/// Gets or sets the file mass precision.
	/// </summary>
	public int FileMassPrecision { private get; set; }

	/// <summary>
	/// Gets a filter string which failed to parse.
	/// If this is "not empty" there was a parsing error on a supplied
	/// filter string, and the "Delivery" will be set to null.
	/// </summary>
	public string ParseError { get; private set; }

	/// <summary>
	/// Gets or sets the settings.
	/// </summary>
	public IChromatogramSettingsEx Settings { private get; set; }

	/// <summary>
	/// Gets or sets the (global) time range.
	/// </summary>
	public ThermoFisher.CommonCore.Data.Business.Range TimeRange { private get; set; }

	/// <summary>
	/// Gets or sets the mass tolerance.
	/// </summary>
	public MassOptions Tolerance { private get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to add base masses.
	/// </summary>
	public bool AddBasePeakMasses { get; set; }

	/// <summary>
	/// Create chromatogram construction tool for one trace
	/// </summary>
	public void Execute()
	{
		try
		{
			Delivery = CreateDeliveryForTrace(TimeRange, Tolerance, Settings);
		}
		catch (InvalidFilterFormatException ex)
		{
			Delivery = null;
			ParseError = ex.Message;
		}
	}

	/// <summary>
	/// calculate (final) mass range for chromatogram, by adding tolerance for single ion mode.
	/// Single ion mode is detected as "high mass == 0.0" or "low and high are same".
	/// </summary>
	/// <param name="toleranceOptions">
	/// The tolerance options.
	/// </param>
	/// <param name="massRanges">
	/// The mass ranges.
	/// </param>
	/// <param name="range">
	/// The range.
	/// </param>
	/// <param name="hasTolerance">
	/// Set if the class has a tolerance to be applied
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.Range" />.
	/// </returns>
	private static IRangeAccess CalculateMassRange(MassOptions toleranceOptions, IRangeAccess[] massRanges, int range, bool hasTolerance)
	{
		IRangeAccess rangeAccess = massRanges[range];
		if (Math.Abs(rangeAccess.High) < 1E-06)
		{
			double num = (hasTolerance ? toleranceOptions.GetToleranceAtMass(rangeAccess.Low) : 0.5);
			if (Math.Abs(num) < 1E-06)
			{
				num = 0.5;
			}
			rangeAccess = ThermoFisher.CommonCore.Data.Business.Range.CreateFromCenterAndDelta(rangeAccess.Low, num);
		}
		else if (rangeAccess.High - rangeAccess.Low < 1E-06)
		{
			double delta = (hasTolerance ? toleranceOptions.GetToleranceAtMass(rangeAccess.Low) : 0.5);
			rangeAccess = ThermoFisher.CommonCore.Data.Business.Range.CreateFromCenterAndDelta(rangeAccess.Low, delta);
		}
		return rangeAccess;
	}

	/// <summary>
	/// Create a chromatogram point generator which can generate the mass with the largest
	/// intensity of of all data in a scan.
	/// </summary>
	/// <param name="startTime">
	/// Start time of chromatogram
	/// </param>
	/// <param name="endTime">
	/// End time of chromatogram
	/// </param>
	/// <param name="toleranceOptions">
	/// Mass tolerance settings
	/// </param>
	/// <param name="thisTrace">
	/// Settings for this chromatogram trace
	/// </param>
	/// <returns>
	/// Tool to sum all data in a scan
	/// </returns>
	private IChromatogramRequest CreateBaseMassOfFullMassRangeChromatogram(double startTime, double endTime, MassOptions toleranceOptions, IChromatogramSettings thisTrace)
	{
		ScanSelect scanSelector = CreateScanSelect(toleranceOptions, thisTrace);
		IList<IChromatogramPointRequest> pointRequests = new List<IChromatogramPointRequest> { ChromatogramPointRequest.BasePeakMassRequest() };
		return ChromatogramPointBuilderFactory.CreatePointBuilder(ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime), scanSelector, pointRequests);
	}

	/// <summary>
	/// Create delivery, which will generate a Base Peak Chromatogram
	/// </summary>
	/// <param name="startTime">
	/// The start time.
	/// </param>
	/// <param name="endTime">
	/// The end time.
	/// </param>
	/// <param name="toleranceOptions">
	/// The tolerance options.
	/// </param>
	/// <param name="thisTrace">
	/// The this trace.
	/// </param>
	/// <returns>
	/// An object to create the desired chromatogram.
	/// </returns>
	private ChromatogramDelivery CreateBasePeakDelivery(double startTime, double endTime, MassOptions toleranceOptions, IChromatogramSettings thisTrace)
	{
		ScanSelect scanSelector = CreateScanSelect(toleranceOptions, thisTrace);
		IRangeAccess[] massRanges = thisTrace.MassRanges;
		int rangeCount;
		if (massRanges != null && (rangeCount = massRanges.Length) >= 1)
		{
			return CreateBasePeakOverMassRangeChromatogram(startTime, endTime, toleranceOptions, thisTrace, massRanges, rangeCount);
		}
		InstrumentBasePeakIntensityChromatogramPoint request = new InstrumentBasePeakIntensityChromatogramPoint
		{
			RetentionTimeRange = ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime),
			ScanSelector = scanSelector
		};
		return new ChromatogramDelivery
		{
			Request = request
		};
	}

	/// <summary>
	/// Create delivery, which will generate a chromatogram with the base peak mass over a Mass Range
	/// </summary>
	/// <param name="startTime">
	/// The start time.
	/// </param>
	/// <param name="endTime">
	/// The end time.
	/// </param>
	/// <param name="toleranceOptions">
	/// The tolerance options.
	/// </param>
	/// <param name="thisTrace">
	/// The this trace.
	/// </param>
	/// <param name="massRanges">Mass ranges for this chromatogram</param>
	/// <param name="rangeCount">Number of mass ranges to use</param>
	/// <returns>
	/// An object to create the desired chromatogram.
	/// </returns>
	private ChromatogramDelivery CreateBasePeakMassOverMassRangeChromatogram(double startTime, double endTime, MassOptions toleranceOptions, IChromatogramSettings thisTrace, IRangeAccess[] massRanges, int rangeCount)
	{
		List<ChromatogramPointBuilderFactory.ChromatogramBasePeakMassRequest> list = new List<ChromatogramPointBuilderFactory.ChromatogramBasePeakMassRequest>(rangeCount);
		bool hasTolerance = toleranceOptions != null;
		for (int i = 0; i < rangeCount; i++)
		{
			IRangeAccess rangeAccess = CalculateMassRange(toleranceOptions, massRanges, i, hasTolerance);
			list.Add(ChromatogramPointBuilderFactory.ChromatogramBasePeakMassRequest.BasePeakMassOverMassRangeRequest(rangeAccess.Low, rangeAccess.High));
		}
		ScanSelect scanSelector = CreateScanSelect(toleranceOptions, thisTrace);
		ChromatogramDelivery chromatogramDelivery = new ChromatogramDelivery();
		IList<ChromatogramPointBuilderFactory.ChromatogramBasePeakMassRequest> pointRequests = list;
		chromatogramDelivery.Request = ChromatogramPointBuilderFactory.CreatePointBuilderMassOfMax(ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime), scanSelector, pointRequests);
		return chromatogramDelivery;
	}

	/// <summary>
	/// Create delivery, which will generate a chromatogram with base peak intensity over a Mass Range
	/// </summary>
	/// <param name="startTime">
	/// The start time.
	/// </param>
	/// <param name="endTime">
	/// The end time.
	/// </param>
	/// <param name="toleranceOptions">
	/// The tolerance options.
	/// </param>
	/// <param name="thisTrace">
	/// The this trace.
	/// </param>
	/// <param name="massRanges">Mass ranges for this chromatogram</param>
	/// <param name="rangeCount">Number of mass ranges to use</param>
	/// <returns>
	/// An object to create the desired chromatogram.
	/// </returns>
	private ChromatogramDelivery CreateBasePeakOverMassRangeChromatogram(double startTime, double endTime, MassOptions toleranceOptions, IChromatogramSettings thisTrace, IRangeAccess[] massRanges, int rangeCount)
	{
		List<IChromatogramPointRequest> list = new List<IChromatogramPointRequest>(rangeCount);
		bool hasTolerance = toleranceOptions != null;
		for (int i = 0; i < rangeCount; i++)
		{
			IRangeAccess rangeAccess = CalculateMassRange(toleranceOptions, massRanges, i, hasTolerance);
			list.Add(ChromatogramPointRequest.BasePeakOverMassRangeRequest(rangeAccess.Low, rangeAccess.High));
		}
		ScanSelect scanSelector = CreateScanSelect(toleranceOptions, thisTrace);
		ChromatogramDelivery chromatogramDelivery = new ChromatogramDelivery();
		IList<IChromatogramPointRequest> pointRequests = list;
		chromatogramDelivery.Request = ChromatogramPointBuilderFactory.CreatePointBuilderMax(ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime), scanSelector, pointRequests);
		return chromatogramDelivery;
	}

	/// <summary>
	/// Create tools to calculate each chromatogram data point
	/// based on the mode.
	/// For example: Sum of masses in a range (XIC).
	/// Sum of all data in scan (Tic).
	/// </summary>
	/// <param name="timeRange">Time limits of this chromatogram</param>
	/// <param name="toleranceOptions">Mass tolerance settings</param>
	/// <param name="thisTrace">Chromatogram settings</param>
	/// <returns>Data for chromatogram generator</returns>
	private ChromatogramDelivery[] CreateDeliveryForTrace(ThermoFisher.CommonCore.Data.Business.Range timeRange, MassOptions toleranceOptions, IChromatogramSettingsEx thisTrace)
	{
		IRangeAccess rtRange = thisTrace.RtRange;
		double low;
		double high;
		if (rtRange != null && rtRange.High > rtRange.Low)
		{
			low = rtRange.Low;
			high = rtRange.High;
		}
		else
		{
			low = timeRange.Low;
			high = timeRange.High;
		}
		return thisTrace.Trace switch
		{
			TraceType.MassRange => CreateMassRangeDelivery(low, high, toleranceOptions, thisTrace), 
			TraceType.BasePeak => new ChromatogramDelivery[1] { CreateBasePeakDelivery(low, high, toleranceOptions, thisTrace) }, 
			TraceType.Fragment => new ChromatogramDelivery[1] { CreateFragmentChromatogramDelivery(low, high, toleranceOptions, thisTrace) }, 
			TraceType.PrecursorMass => new ChromatogramDelivery[1] { CreatePrecursorChromatogramDelivery(low, high, toleranceOptions, thisTrace) }, 
			TraceType.Custom => new ChromatogramDelivery[1] { CreateCustomChromatogramDelivery(low, high, toleranceOptions, thisTrace) }, 
			_ => new ChromatogramDelivery[1] { CreateTicDelivery(low, high, toleranceOptions, thisTrace) }, 
		};
	}

	/// <summary>
	/// Create delivery, which will generate a Fragment Chromatogram
	/// </summary>
	/// <param name="startTime">
	/// The start time.
	/// </param>
	/// <param name="endTime">
	/// The end time.
	/// </param>
	/// <param name="toleranceOptions">
	/// The mass tolerance options.
	/// </param>
	/// <param name="thisTrace">
	/// The this trace.
	/// </param>
	/// <returns>
	/// An object to create the desired chromatogram.
	/// </returns>
	private ChromatogramDelivery CreateFragmentChromatogramDelivery(double startTime, double endTime, MassOptions toleranceOptions, IChromatogramSettings thisTrace)
	{
		IScanFilter filterFromString = GetFilterFromString(thisTrace.Filter);
		if (filterFromString == null)
		{
			throw new InvalidFilterFormatException(thisTrace.Filter);
		}
		filterFromString.MassPrecision = toleranceOptions?.Precision ?? FileMassPrecision;
		double lowMass = 0.0;
		double highMass = 0.0;
		bool flag = false;
		if (filterFromString.MSOrder >= MSOrderType.Ms2)
		{
			int massCount = filterFromString.MassCount;
			if (massCount > 0)
			{
				double mass = filterFromString.GetMass(massCount - 1);
				double num = toleranceOptions?.GetToleranceAtMass(thisTrace.FragmentMass) ?? 0.5;
				lowMass = mass - thisTrace.FragmentMass - num;
				highMass = mass - thisTrace.FragmentMass + num;
				flag = true;
			}
		}
		if (!flag)
		{
			lowMass = (highMass = 0.0 - thisTrace.FragmentMass);
		}
		return new ChromatogramDelivery
		{
			Request = ChromatogramPointBuilderFactory.CreatePointBuilder(pointRequests: new List<IChromatogramPointRequest> { ChromatogramPointRequest.FragmentRequest(lowMass, highMass, toleranceOptions) }, retentionTimeRange: ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime), scanSelector: ScanSelect.SelectByFilter(filterFromString))
		};
	}

	/// <summary>
	/// Create delivery, which will return the precursor mass at a given index
	/// </summary>
	/// <param name="startTime">
	/// The start time.
	/// </param>
	/// <param name="endTime">
	/// The end time.
	/// </param>
	/// <param name="toleranceOptions">
	/// The mass tolerance options.
	/// </param>
	/// <param name="thisTrace">
	/// The this trace.
	/// </param>
	/// <returns>
	/// An object to create the desired chromatogram.
	/// </returns>
	private ChromatogramDelivery CreatePrecursorChromatogramDelivery(double startTime, double endTime, MassOptions toleranceOptions, IChromatogramSettings thisTrace)
	{
		ScanSelect scanSelector = CreateScanSelect(toleranceOptions, thisTrace);
		IRangeAccess[] massRanges = thisTrace.MassRanges;
		double num = 0.0;
		if (massRanges != null && massRanges.Length > 0)
		{
			num = massRanges[0].Low;
		}
		int index = 0;
		if (num >= 0.0 && num <= 99.1)
		{
			index = (int)Math.Round(num);
		}
		return new ChromatogramDelivery
		{
			Request = new PrecursorChromatogramRequest
			{
				Index = index,
				RetentionTimeRange = ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime),
				ScanSelector = scanSelector
			}
		};
	}

	/// <summary>
	/// Create delivery, which will generate a Custom Chromatogram
	/// </summary>
	/// <param name="startTime">
	/// The start time.
	/// </param>
	/// <param name="endTime">
	/// The end time.
	/// </param>
	/// <param name="toleranceOptions">
	/// The mass tolerance options.
	/// </param>
	/// <param name="thisTrace">
	/// The this trace.
	/// </param>
	/// <returns>
	/// An object to create the desired chromatogram.
	/// </returns>
	private ChromatogramDelivery CreateCustomChromatogramDelivery(double startTime, double endTime, MassOptions toleranceOptions, IChromatogramSettingsEx thisTrace)
	{
		ScanSelect scanSelector = CreateScanSelect(toleranceOptions, thisTrace);
		return new ChromatogramDelivery
		{
			Request = ChromatogramPointBuilderFactory.CreatePointBuilder(pointRequests: new List<IChromatogramPointRequest> { ChromatogramPointRequest.CustomRequest(thisTrace.CustomValueProvider) }, retentionTimeRange: ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime), scanSelector: scanSelector)
		};
	}

	/// <summary>
	/// Design a chromatogram point builder which just gets the Tic value from each scan header.
	/// </summary>
	/// <param name="startTime">
	/// Start time of chromatogram.
	/// </param>
	/// <param name="endTime">
	/// End time of chromatogram.
	/// </param>
	/// <param name="toleranceOptions">
	/// Mass tolerance settings
	/// </param>
	/// <param name="thisTrace">
	/// Trace settings
	/// </param>
	/// <returns>
	/// Tic chromatogram generator
	/// </returns>
	private IChromatogramRequest CreateInstrumentTicChromatogram(double startTime, double endTime, MassOptions toleranceOptions, IChromatogramSettings thisTrace)
	{
		ScanSelect scanSelector = CreateScanSelect(toleranceOptions, thisTrace);
		return new InstrumentTicChromatogramPoint
		{
			RetentionTimeRange = ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime),
			ScanSelector = scanSelector
		};
	}

	/// <summary>
	/// Create delivery, which will generate a Mass Range Chromatogram (XIC)
	/// </summary>
	/// <param name="startTime">
	/// The start time.
	/// </param>
	/// <param name="endTime">
	/// The end time.
	/// </param>
	/// <param name="toleranceOptions">
	/// The tolerance options.
	/// </param>
	/// <param name="thisTrace">
	/// The this trace.
	/// </param>
	/// <param name="massRanges">Mass ranges for this chromatogram</param>
	/// <param name="rangeCount">Number of mass ranges to use</param>
	/// <returns>
	/// An object to create the desired chromatogram.
	/// </returns>
	private ChromatogramDelivery CreateMassRangeChromatogram(double startTime, double endTime, MassOptions toleranceOptions, IChromatogramSettings thisTrace, IRangeAccess[] massRanges, int rangeCount)
	{
		List<IChromatogramPointRequest> list = new List<IChromatogramPointRequest>(rangeCount);
		bool hasTolerance = toleranceOptions != null;
		for (int i = 0; i < rangeCount; i++)
		{
			IRangeAccess rangeAccess = CalculateMassRange(toleranceOptions, massRanges, i, hasTolerance);
			list.Add(ChromatogramPointRequest.MassRangeRequest(rangeAccess.Low, rangeAccess.High));
		}
		ScanSelect scanSelector = CreateScanSelect(toleranceOptions, thisTrace);
		ChromatogramDelivery chromatogramDelivery = new ChromatogramDelivery();
		IList<IChromatogramPointRequest> pointRequests = list;
		chromatogramDelivery.Request = ChromatogramPointBuilderFactory.CreatePointBuilder(ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime), scanSelector, pointRequests);
		return chromatogramDelivery;
	}

	/// <summary>
	/// Create delivery, which will generate a Mass Range Chromatogram (XIC)
	/// </summary>
	/// <param name="startTime">
	/// The start time.
	/// </param>
	/// <param name="endTime">
	/// The end time.
	/// </param>
	/// <param name="toleranceOptions">
	/// The tolerance options.
	/// </param>
	/// <param name="thisTrace">
	/// The this trace.
	/// </param>
	/// <returns>
	/// An object to create the desired chromatogram.
	/// </returns>
	private ChromatogramDelivery[] CreateMassRangeDelivery(double startTime, double endTime, MassOptions toleranceOptions, IChromatogramSettings thisTrace)
	{
		IRangeAccess[] massRanges = thisTrace.MassRanges;
		int num = ((massRanges != null) ? massRanges.Length : 0);
		bool flag = num > 0;
		ChromatogramDelivery chromatogramDelivery = (flag ? CreateMassRangeChromatogram(startTime, endTime, toleranceOptions, thisTrace, massRanges, num) : new ChromatogramDelivery
		{
			Request = CreateTotalOfMassRangeChromatogram(startTime, endTime, toleranceOptions, thisTrace)
		});
		if (AddBasePeakMasses)
		{
			ChromatogramDelivery chromatogramDelivery2 = (flag ? CreateBasePeakMassOverMassRangeChromatogram(startTime, endTime, toleranceOptions, thisTrace, massRanges, num) : new ChromatogramDelivery
			{
				Request = CreateBaseMassOfFullMassRangeChromatogram(startTime, endTime, toleranceOptions, thisTrace)
			});
			return new ChromatogramDelivery[2] { chromatogramDelivery, chromatogramDelivery2 };
		}
		return new ChromatogramDelivery[1] { chromatogramDelivery };
	}

	/// <summary>
	/// create a scan selection method.
	/// </summary>
	/// <param name="toleranceOptions">
	/// The tolerance options.
	/// </param>
	/// <param name="thisTrace">
	/// The this trace.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanSelect" />.
	/// </returns>
	private ScanSelect CreateScanSelect(MassOptions toleranceOptions, IChromatogramSettings thisTrace)
	{
		if (Settings.CompoundNames != null && Settings.CompoundNames.Length != 0)
		{
			return new ScanSelect
			{
				UseFilter = false,
				Names = new List<string>(Settings.CompoundNames)
			};
		}
		IScanFilter obj = GetFilterFromString(thisTrace.Filter) ?? throw new InvalidFilterFormatException(thisTrace.Filter);
		obj.MassPrecision = toleranceOptions?.Precision ?? FileMassPrecision;
		return ScanSelect.SelectByFilter(obj);
	}

	/// <summary>
	/// Create Tic delivery, which will generate a Total Ion Chromatogram
	/// </summary>
	/// <param name="startTime">
	/// The start time.
	/// </param>
	/// <param name="endTime">
	/// The end time.
	/// </param>
	/// <param name="toleranceOptions">
	/// The tolerance options.
	/// </param>
	/// <param name="thisTrace">
	/// The this trace.
	/// </param>
	/// <returns>
	/// An object to create the desired chromatogram.
	/// </returns>
	private ChromatogramDelivery CreateTicDelivery(double startTime, double endTime, MassOptions toleranceOptions, IChromatogramSettings thisTrace)
	{
		return new ChromatogramDelivery
		{
			Request = CreateInstrumentTicChromatogram(startTime, endTime, toleranceOptions, thisTrace)
		};
	}

	/// <summary>
	/// Create a chromatogram point generator which can sum all data in a scan.
	/// </summary>
	/// <param name="startTime">
	/// Start time of chromatogram
	/// </param>
	/// <param name="endTime">
	/// End time of chromatogram
	/// </param>
	/// <param name="toleranceOptions">
	/// Mass tolerance settings
	/// </param>
	/// <param name="thisTrace">
	/// Settings for this chromatogram trace
	/// </param>
	/// <returns>
	/// Tool to sum all data in a scan
	/// </returns>
	private IChromatogramRequest CreateTotalOfMassRangeChromatogram(double startTime, double endTime, MassOptions toleranceOptions, IChromatogramSettings thisTrace)
	{
		ScanSelect scanSelector = CreateScanSelect(toleranceOptions, thisTrace);
		IList<IChromatogramPointRequest> pointRequests = new List<IChromatogramPointRequest> { ChromatogramPointRequest.TotalIonRequest() };
		return ChromatogramPointBuilderFactory.CreatePointBuilder(ThermoFisher.CommonCore.Data.Business.Range.Create(startTime, endTime), scanSelector, pointRequests);
	}

	/// <summary>
	/// Get a filter interface from a string.
	/// </summary>
	/// <param name="filter">The filter string.</param>
	/// <returns>
	/// An interface representing the filter fields, converted from the supplied string.
	/// </returns>
	private IScanFilter GetFilterFromString(string filter)
	{
		FilterStringParser filterStringParser = new FilterStringParser
		{
			MassPrecision = 10
		};
		if (filterStringParser.ParseFilterStructString(filter))
		{
			return new WrappedScanFilter(filterStringParser.ToFilterScanEvent(fromScan: false));
		}
		return null;
	}
}
