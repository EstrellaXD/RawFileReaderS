using System;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// Class to import PMD file Options
/// </summary>
internal class ProcessingMethodOptions : IProcessingMethodOptionsAccess, IRawObjectBase
{
	/// <summary>
	/// The options version 1.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct OptionsVersion1
	{
		public CalStandards CalibrationStandards;

		public CalibrateAs CalibrateAs;

		public VoidTime VoidTime;

		public ReportAs ReportAs;

		public bool RejectOutliers;

		public double VoidTimeValue;

		public double AllowedDevPercent;

		public double SearchWindow;

		public int MinScansInBaseline;

		public double InitialNoiseScale;

		public double BaseNoiseLimit;

		public int BkgWidth;
	}

	/// <summary>
	/// The options version 12.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct OptionsVersion12
	{
		public CalStandards CalibrationStandards;

		public CalibrateAs CalibrateAs;

		public VoidTime VoidTime;

		public ReportAs ReportAs;

		public bool RejectOutliers;

		public double VoidTimeValue;

		public double AllowedDevPercent;

		public double SearchWindow;

		public int MinScansInBaseline;

		public double InitialNoiseScale;

		public double BaseNoiseLimit;

		public int BkgWidth;

		public double BaseNoiseRejectionFactor;
	}

	/// <summary>
	/// The options.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct Options
	{
		public CalStandards CalibrationStandards;

		public CalibrateAs CalibrateAs;

		public VoidTime VoidTime;

		public ReportAs ReportAs;

		public bool RejectOutliers;

		public double VoidTimeValue;

		public double AllowedDevPercent;

		public double SearchWindow;

		public int MinScansInBaseline;

		public double InitialNoiseScale;

		public double BaseNoiseLimit;

		public int BkgWidth;

		public double BaseNoiseRejectionFactor;

		public ChromatographyType ChromatographyType;

		public bool UseAltPercentRSDCalc;

		public bool CalLevelsManuallyChanged;

		public int LowIntensityCutoff;
	}

	/// <summary>
	/// The (calibration) level info.
	/// </summary>
	private struct LevelInfo : IDilutionLevelAccess
	{
		/// <summary>
		/// Gets or sets the base amount.
		/// </summary>
		public double BaseAmount { get; internal set; }

		/// <summary>
		/// Gets or sets the test percent.
		/// </summary>
		public double TestPercent { get; internal set; }

		/// <summary>
		/// Gets or sets the level name
		/// </summary>
		public string LevelName { get; internal set; }
	}

	/// <summary>
	/// The target component factor.
	/// </summary>
	private struct TargetComponentFactor : IDilutionTargetCompFactorAccess
	{
		/// <summary>
		/// Gets or sets the base amount.
		/// </summary>
		public double BaseAmount { get; internal set; }

		/// <summary>
		/// Gets or sets the target component name.
		/// </summary>
		public string TargetComponentName { get; internal set; }
	}

	private static readonly int[,] MarshalledSizes = new int[3, 2]
	{
		{
			25,
			Marshal.SizeOf(typeof(Options))
		},
		{
			12,
			Marshal.SizeOf(typeof(OptionsVersion12))
		},
		{
			0,
			Marshal.SizeOf(typeof(OptionsVersion1))
		}
	};

	private Options _options;

	private IDilutionLevelAccess[] _dilutionLevels = Array.Empty<IDilutionLevelAccess>();

	private IDilutionTargetCompFactorAccess[] _dilutionTargetFactors = Array.Empty<IDilutionTargetCompFactorAccess>();

	/// <summary>
	/// Gets a value indicating whether the standards are internal or external.
	/// </summary>
	public CalStandards CalibrationType => _options.CalibrationStandards;

	/// <summary>
	/// Gets a value indicating whether calibration is performed on concentration or amount
	/// </summary>
	public CalibrateAs CalibrateAs => _options.CalibrateAs;

	/// <summary>
	/// Gets a value determining how void time is calculated.
	/// </summary>
	public VoidTime VoidTime => _options.VoidTime;

	/// <summary>
	/// Gets a value determining whether amounts or concentrations are reported.
	/// </summary>
	public ReportAs ReportAs => _options.ReportAs;

	/// <summary>
	/// Gets a value determining how chromatography was performed.
	/// </summary>
	public ChromatographyType ChromatographyType
	{
		get
		{
			return _options.ChromatographyType;
		}
		private set
		{
			_options.ChromatographyType = value;
		}
	}

	/// <summary>
	/// Gets a value indicating whether outliers (on a cal curve) should be rejected.
	/// </summary>
	public bool RejectOutliers => _options.RejectOutliers;

	/// <summary>
	/// Gets the added time of void volume, where void time is set to "Value"
	/// </summary>
	public double VoidTimeValue => _options.VoidTimeValue;

	/// <summary>
	/// Gets the permitted % deviation from an expected standard amount.
	/// </summary>
	public double AllowedDevPercent => _options.AllowedDevPercent;

	/// <summary>
	/// Gets the search window for the expected time of a peak.
	/// </summary>
	public double SearchWindow => _options.SearchWindow;

	/// <summary>
	/// Gets the minimum number of expected scans in a baseline
	/// Genesis: MinScansInBaseline
	/// </summary>
	public int MinScansInBaseline => _options.MinScansInBaseline;

	/// <summary>
	/// Gets a scale factor for the noise level in chromatographic peaks.
	/// </summary>
	public double InitialNoiseScale => _options.InitialNoiseScale;

	/// <summary>
	/// Gets the limit on baseline noise
	/// </summary>
	public double BaseNoiseLimit => _options.BaseNoiseLimit;

	/// <summary>
	/// Gets the background width (scans)
	/// </summary>
	public int BackgroundWidth => _options.BkgWidth;

	/// <summary>
	/// Gets the baseline noise rejection factor
	/// </summary>
	public double BaseNoiseRejectionFactor
	{
		get
		{
			return _options.BaseNoiseRejectionFactor;
		}
		private set
		{
			_options.BaseNoiseRejectionFactor = value;
		}
	}

	/// <summary>
	/// Gets a value indicating whether the "alternate Percent RDS calculation" should be performed.
	/// </summary>
	public bool UseAltPercentRsdCalc
	{
		get
		{
			return _options.UseAltPercentRSDCalc;
		}
		private set
		{
			_options.UseAltPercentRSDCalc = value;
		}
	}

	/// <summary>
	/// Gets a value indicating whether there was a "manual change" to calibration levels.
	/// </summary>
	public bool CalLevelsManuallyChanged
	{
		get
		{
			return _options.CalLevelsManuallyChanged;
		}
		private set
		{
			_options.CalLevelsManuallyChanged = value;
		}
	}

	/// <summary>
	/// Gets the low intensity cutoff
	/// </summary>
	public int LowIntensityCutoff
	{
		get
		{
			return _options.LowIntensityCutoff;
		}
		private set
		{
			_options.LowIntensityCutoff = value;
		}
	}

	/// <summary>
	/// Read the table of dilution levels
	/// </summary>
	/// <returns>The dilution levels</returns>
	public ReadOnlyCollection<IDilutionLevelAccess> GetDilutionLevels()
	{
		return new ReadOnlyCollection<IDilutionLevelAccess>(_dilutionLevels);
	}

	/// <summary>
	/// Gets a copy of the dilution target component factors table
	/// </summary>
	/// <returns>The dilution target component factors table</returns>
	public ReadOnlyCollection<IDilutionTargetCompFactorAccess> GetDilutionFactors()
	{
		return new ReadOnlyCollection<IDilutionTargetCompFactorAccess>(_dilutionTargetFactors);
	}

	/// <summary>
	/// Load (from file)
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="dataOffset">
	/// The data offset.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes loaded.
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		_options = Utilities.ReadStructure<Options>(viewer, ref startPos, fileRevision, MarshalledSizes);
		if (fileRevision < 60)
		{
			UseAltPercentRsdCalc = false;
		}
		if (fileRevision < 25)
		{
			ChromatographyType = ChromatographyType.Lc;
			CalLevelsManuallyChanged = false;
			LowIntensityCutoff = 0;
		}
		else if (fileRevision >= 25)
		{
			int num = viewer.ReadIntExt(ref startPos);
			_dilutionLevels = new IDilutionLevelAccess[num];
			for (int i = 0; i < num; i++)
			{
				double baseAmount = viewer.ReadDoubleExt(ref startPos);
				double testPercent = viewer.ReadDoubleExt(ref startPos);
				string levelName = viewer.ReadStringExt(ref startPos);
				_dilutionLevels[i] = new LevelInfo
				{
					BaseAmount = baseAmount,
					TestPercent = testPercent,
					LevelName = levelName
				};
				viewer.ReadIntExt(ref startPos);
			}
			int num2 = viewer.ReadIntExt(ref startPos);
			_dilutionTargetFactors = new IDilutionTargetCompFactorAccess[num2];
			for (int j = 0; j < num2; j++)
			{
				double baseAmount2 = viewer.ReadDoubleExt(ref startPos);
				string targetComponentName = viewer.ReadStringExt(ref startPos);
				_dilutionTargetFactors[j] = new TargetComponentFactor
				{
					BaseAmount = baseAmount2,
					TargetComponentName = targetComponentName
				};
			}
		}
		if (fileRevision < 12)
		{
			BaseNoiseRejectionFactor = 2.0;
		}
		return startPos - dataOffset;
	}
}
