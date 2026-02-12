using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Data for the calibration curve.
/// Included and excluded points, fitted curve and equation text.
/// This is designed to help creating calibration curve plots.
/// </summary>
[Serializable]
public class CalibrationCurveData : ICloneable, ICalibrationCurveDataAccess
{
	/// <summary>
	/// The _excluded.
	/// </summary>
	private readonly List<CalibrationCurvePoint> _excluded = new List<CalibrationCurvePoint>();

	/// <summary>
	/// The _external excluded.
	/// </summary>
	private readonly List<CalibrationCurvePoint> _externalExcluded = new List<CalibrationCurvePoint>();

	/// <summary>
	/// The _external included.
	/// </summary>
	private readonly List<CalibrationCurvePoint> _externalIncluded = new List<CalibrationCurvePoint>();

	/// <summary>
	/// The included.
	/// </summary>
	private readonly List<CalibrationCurvePoint> _included = new List<CalibrationCurvePoint>();

	/// <summary>
	/// The points.
	/// </summary>
	private readonly List<CalibrationCurvePoint> _points = new List<CalibrationCurvePoint>();

	/// <summary>
	/// The equation.
	/// </summary>
	private string _equation;

	/// <summary>
	/// The percent cv.
	/// </summary>
	private double _percentCv;

	/// <summary>
	/// The coefficient of determination (R squared).
	/// </summary>
	private double _coefficientOfDetermination;

	/// <summary>
	/// Gets or sets the equation text from the regression calculation.
	/// </summary>
	public string Equation
	{
		get
		{
			return _equation;
		}
		set
		{
			_equation = value;
		}
	}

	/// <summary>
	/// Gets the excluded replicates from current sequence data
	/// </summary>
	public List<CalibrationCurvePoint> Excluded => _excluded;

	/// <summary>
	/// Gets the excluded replicates from current sequence data
	/// </summary>
	[XmlIgnore]
	public ReadOnlyCollection<ICalibrationCurvePointAccess> ExcludedPoints => ToReadOnly(_excluded);

	/// <summary>
	/// Gets the excluded replicates from previously acquired  data
	/// </summary>
	public List<CalibrationCurvePoint> ExternalExcluded => _externalExcluded;

	/// <summary>
	/// Gets the excluded replicates from previously acquired  data
	/// </summary>
	[XmlIgnore]
	public ReadOnlyCollection<ICalibrationCurvePointAccess> ExternalExcludedPoints => ToReadOnly(_externalExcluded);

	/// <summary>
	/// Gets the included replicates from previously acquired data
	/// </summary>
	public List<CalibrationCurvePoint> ExternalIncluded => _externalIncluded;

	/// <summary>
	/// Gets the included replicates from previously acquired data
	/// </summary>
	[XmlIgnore]
	public ReadOnlyCollection<ICalibrationCurvePointAccess> ExternalIncludedPoints => ToReadOnly(_externalIncluded);

	/// <summary>
	/// Gets the fitted line
	/// </summary>
	[XmlIgnore]
	public ReadOnlyCollection<ICalibrationCurvePointAccess> FittedLinePoints => ToReadOnly(_points);

	/// <summary>
	/// Gets the included replicates from current sequence data
	/// </summary>
	public List<CalibrationCurvePoint> Included => _included;

	/// <summary>
	/// Gets the included replicates from current sequence data
	/// </summary>
	[XmlIgnore]
	public ReadOnlyCollection<ICalibrationCurvePointAccess> IncludedPoints => ToReadOnly(_included);

	/// <summary>
	/// Gets or sets a value indicating whether the fitted line is empty
	/// The curve data needs to be plotted as appropriate for
	/// an internal standard: Centered on the set of points,
	/// </summary>
	public bool IsInternalStandard { get; set; }

	/// <summary>
	/// Gets or sets the percentage coefficient of variance from the first calibration level.
	/// </summary>
	public double PercentCv
	{
		get
		{
			return _percentCv;
		}
		set
		{
			_percentCv = value;
		}
	}

	/// <summary>
	/// Gets or sets the percentage relative standard deviation from the first calibration level.
	/// </summary>
	public double PercentRsd { get; set; }

	/// <summary>
	/// Gets the fitted line
	/// </summary>
	public List<CalibrationCurvePoint> Points => _points;

	/// <summary>
	/// Gets or sets the RSquared value from the regression calculation (-1 if not valid)
	/// </summary>
	public double RSquared
	{
		get
		{
			return _coefficientOfDetermination;
		}
		set
		{
			_coefficientOfDetermination = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.CalibrationCurveData" /> class. 
	/// Default constructor
	/// </summary>
	public CalibrationCurveData()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.CalibrationCurveData" /> class.
	/// </summary>
	/// <param name="regression">
	/// The regression.
	/// </param>
	/// <param name="currentCalibrationLevels">
	/// The current calibration levels.
	/// </param>
	/// <param name="historicCalibrationLevels">
	/// The historic calibration levels.
	/// </param>
	/// <param name="isInternalStandard">
	/// The is internal standard.
	/// </param>
	private CalibrationCurveData(IRegression regression, IList<ILevelReplicates> currentCalibrationLevels, IList<ILevelReplicates> historicCalibrationLevels, bool isInternalStandard)
	{
		CalibrationCurvePoint[] array = null;
		if (!FindRsd(currentCalibrationLevels))
		{
			FindRsd(historicCalibrationLevels);
		}
		if (regression != null)
		{
			_coefficientOfDetermination = regression.RSquared;
			_equation = regression.Equation;
			_included = MakePoints(currentCalibrationLevels, included: true);
			_excluded = MakePoints(currentCalibrationLevels, included: false);
			double val = FindMax(_included);
			val = Math.Max(val, FindMax(_excluded));
			if (historicCalibrationLevels != null)
			{
				_externalIncluded = MakePoints(historicCalibrationLevels, included: true);
				val = Math.Max(val, FindMax(_externalIncluded));
				_externalExcluded = MakePoints(historicCalibrationLevels, included: false);
				val = Math.Max(val, FindMax(_externalExcluded));
			}
			if (isInternalStandard)
			{
				array = regression.PreparePlotData(2, useMaxValue: true, regression.MaxXValue * 2.0);
			}
			else
			{
				double maxXValue = regression.MaxXValue;
				maxXValue = 1.1 * Math.Max(val, maxXValue);
				array = regression.PreparePlotData(100, useMaxValue: true, maxXValue);
			}
		}
		_points = ((array != null) ? new List<CalibrationCurvePoint>(array) : new List<CalibrationCurvePoint>());
		IsInternalStandard = isInternalStandard;
	}

	/// <summary>
	/// Covert points to read only.
	/// </summary>
	/// <param name="points">
	/// The points.
	/// </param>
	/// <returns>
	/// The collection of points
	/// </returns>
	private static ReadOnlyCollection<ICalibrationCurvePointAccess> ToReadOnly(List<CalibrationCurvePoint> points)
	{
		return new ReadOnlyCollection<ICalibrationCurvePointAccess>(points.ToArray());
	}

	/// <summary>
	/// Create data for a calibration curve report from a replicate table.
	/// </summary>
	/// <param name="regression">
	/// The replicates for one component
	/// </param>
	/// <param name="calibrationLevels">
	/// Calibration replicates from the current sequence
	/// </param>
	/// <param name="isInternalStandard">
	/// If set: the data is for an internal standard,
	/// and does not contain a line of fit
	/// </param>
	/// <returns>
	/// X,Y points for included, excluded and fitted line
	/// </returns>
	public static CalibrationCurveData FromReplicates(IRegression regression, IList<ILevelReplicates> calibrationLevels, bool isInternalStandard)
	{
		if (regression == null)
		{
			return null;
		}
		return new CalibrationCurveData(regression, calibrationLevels, null, isInternalStandard);
	}

	/// <summary>
	/// Create data for a calibration curve report from a replicate table.
	/// </summary>
	/// <param name="regression">
	/// The replicates for one component
	/// </param>
	/// <param name="calibrationLevels">
	/// Calibration replicates from the current sequence
	/// </param>
	/// <returns>
	/// X,Y points for included, excluded and fitted line
	/// </returns>
	public static CalibrationCurveData FromReplicates(IRegression regression, IList<ILevelReplicates> calibrationLevels)
	{
		if (regression == null)
		{
			return null;
		}
		return new CalibrationCurveData(regression, calibrationLevels, null, isInternalStandard: false);
	}

	/// <summary>
	/// Create data for a calibration curve report from a replicate table.
	/// </summary>
	/// <param name="regression">
	/// The replicates for one component
	/// </param>
	/// <param name="currentCalibrationReplicates">
	/// Calibration replicates from the current sequence
	/// </param>
	/// <param name="historicCalibrationReplicates">
	/// Calibration replicates from previous sequences
	/// </param>
	/// <returns>
	/// X,Y points for included, excluded and fitted line
	/// </returns>
	public static CalibrationCurveData FromReplicates(IRegression regression, IList<ILevelReplicates> currentCalibrationReplicates, IList<ILevelReplicates> historicCalibrationReplicates)
	{
		if (regression == null)
		{
			return null;
		}
		return new CalibrationCurveData(regression, currentCalibrationReplicates, historicCalibrationReplicates, isInternalStandard: false);
	}

	/// <summary>
	/// Create data for a calibration curve report from a replicate table.
	/// </summary>
	/// <param name="regression">
	/// The replicates for one component
	/// </param>
	/// <param name="currentCalibrationReplicates">
	/// Calibration replicates from the current sequence
	/// </param>
	/// <param name="historicCalibrationReplicates">
	/// Calibration replicates from previous sequences
	/// </param>
	/// <param name="isInternalStandard">
	/// If set: the data is for an internal standard,
	/// and does not contain a line of fit
	/// </param>
	/// <returns>
	/// X,Y points for included, excluded and fitted line
	/// </returns>
	public static CalibrationCurveData FromReplicates(IRegression regression, IList<ILevelReplicates> currentCalibrationReplicates, IList<ILevelReplicates> historicCalibrationReplicates, bool isInternalStandard)
	{
		if (regression == null)
		{
			return null;
		}
		return new CalibrationCurveData(regression, currentCalibrationReplicates, historicCalibrationReplicates, isInternalStandard);
	}

	/// <summary>
	/// Create data for a calibration curve report from a replicate table.
	/// This version does not support "stats" for ISTD curves.
	/// </summary>
	/// <param name="regression">
	/// The replicates for one component
	/// </param>
	/// <param name="calibrationLevels">
	/// Calibration replicates from the current sequence
	/// </param>
	/// <returns>
	/// X,Y points for included, excluded and fitted line
	/// </returns>
	public static CalibrationCurveData FromReplicates(IRegression regression, ItemCollection<LevelReplicates> calibrationLevels)
	{
		IList<ILevelReplicates> calibrationLevels2 = new List<ILevelReplicates>(calibrationLevels);
		return FromReplicates(regression, calibrationLevels2, isInternalStandard: false);
	}

	/// <summary>
	/// Create data for a calibration curve report from a replicate table.
	/// This version does not support "stats" for ISTD curves.
	/// </summary>
	/// <param name="regression">
	/// The replicates for one component
	/// </param>
	/// <param name="currentCalibrationReplicates">
	/// Calibration replicates from the current sequence
	/// </param>
	/// <param name="historicCalibrationReplicates">
	/// Calibration replicates from previous sequences
	/// </param>
	/// <param name="isInternalStandard">
	/// If set: the data is for an internal standard,
	/// and does not contain a line of fit
	/// </param>
	/// <returns>
	/// X,Y points for included, excluded and fitted line
	/// </returns>
	public static CalibrationCurveData FromReplicates(IRegression regression, ItemCollection<ILevelReplicates> currentCalibrationReplicates, ItemCollection<ILevelReplicates> historicCalibrationReplicates, bool isInternalStandard)
	{
		return FromReplicates(regression, currentCalibrationReplicates.ToArray(), historicCalibrationReplicates.ToArray(), isInternalStandard);
	}

	/// <summary>
	/// Create a table of points to draw on a calibration curve
	/// </summary>
	/// <param name="calibrationLevels">
	/// Replicate table to convert into points
	/// </param>
	/// <param name="included">
	/// if true, create points for the included data, else create points for the excluded data
	/// </param>
	/// <returns>
	/// The list of points to plot
	/// </returns>
	public static List<CalibrationCurvePoint> MakePoints(IEnumerable<ILevelReplicates> calibrationLevels, bool included)
	{
		if (calibrationLevels == null)
		{
			throw new ArgumentNullException("calibrationLevels");
		}
		List<CalibrationCurvePoint> list = new List<CalibrationCurvePoint>();
		foreach (ILevelReplicates calibrationLevel in calibrationLevels)
		{
			foreach (Replicate item2 in calibrationLevel.ReplicateCollection)
			{
				if (((IReplicate)item2).ExcludeFromCalibration != included)
				{
					double response = ((IReplicate)item2).Response;
					CalibrationCurvePoint item = new CalibrationCurvePoint
					{
						Amount = ((IReplicate)item2).Amount,
						Response = response,
						Key = ((IReplicate)item2).Key,
						PeakKey = ((IReplicate)item2).PeakKey
					};
					list.Add(item);
				}
			}
		}
		return list;
	}

	/// <summary>
	/// Creates a new object that is a copy of the current instance.
	/// </summary>
	/// <returns>
	/// A new object that is a copy of this instance.
	/// </returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		CalibrationCurveData calibrationCurveData = new CalibrationCurveData
		{
			_equation = (_equation ?? string.Empty),
			_coefficientOfDetermination = _coefficientOfDetermination
		};
		foreach (CalibrationCurvePoint item in _excluded)
		{
			calibrationCurveData._excluded.Add(item);
		}
		foreach (CalibrationCurvePoint item2 in _included)
		{
			calibrationCurveData._included.Add(item2);
		}
		foreach (CalibrationCurvePoint point in _points)
		{
			calibrationCurveData._points.Add(point);
		}
		return calibrationCurveData;
	}

	/// <summary>
	/// Find the max amount in the supplied points
	/// </summary>
	/// <param name="points">
	/// The points.
	/// </param>
	/// <returns>
	/// The max amount.
	/// </returns>
	private static double FindMax(IEnumerable<CalibrationCurvePoint> points)
	{
		return points.Aggregate(0.0, (double current, CalibrationCurvePoint point) => Math.Max(point.Amount, current));
	}

	/// <summary>
	/// Find the relative standard deviation
	/// </summary>
	/// <param name="calibrationLevels">
	/// The calibration levels.
	/// </param>
	/// <returns>
	/// The RSD.
	/// </returns>
	private bool FindRsd(IList<ILevelReplicates> calibrationLevels)
	{
		if (calibrationLevels != null && calibrationLevels.Count >= 1 && calibrationLevels[0] is ILevelReplicatesWithStatistics levelReplicatesWithStatistics)
		{
			PercentRsd = levelReplicatesWithStatistics.PercentRSD;
			_percentCv = levelReplicatesWithStatistics.PercentCV;
			return true;
		}
		return false;
	}
}
