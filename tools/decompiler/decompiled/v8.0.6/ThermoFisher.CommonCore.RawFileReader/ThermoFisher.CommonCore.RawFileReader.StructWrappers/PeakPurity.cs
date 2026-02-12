using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// Defines the peak purity settings, as used in an Xcalibur PMD.
/// </summary>
internal class PeakPurity : IPeakPuritySettingsAccess, IRawObjectBase
{
	/// <summary>
	/// The peak purity info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct PeakPurityInfo
	{
		public bool EnableDetection;

		public int ScanThreshold;

		public double DesiredPeakCoverage;

		public bool LimitWaveRange;

		public double LowWavelength;

		public double HighWavelength;
	}

	private static readonly int[,] MarshalledSizes = new int[1, 2] { 
	{
		0,
		Marshal.SizeOf(typeof(PeakPurityInfo))
	} };

	private PeakPurityInfo _info;

	/// <summary>
	/// Gets the % of the detected baseline for which we want to compute PeakPurity
	/// </summary>
	public double DesiredPeakCoverage
	{
		get
		{
			return _info.DesiredPeakCoverage;
		}
		private set
		{
			_info.DesiredPeakCoverage = value;
		}
	}

	/// <summary>
	/// Gets a value indicating whether we want to compute Peak Purity
	/// </summary>
	public bool EnableDetection
	{
		get
		{
			return _info.EnableDetection;
		}
		private set
		{
			_info.EnableDetection = value;
		}
	}

	/// <summary>
	/// Gets a value indicating whether we want to use
	/// the enclosed wavelength range, not the total scan
	/// </summary>
	public bool LimitWavelengthRange => _info.LimitWaveRange;

	/// <summary>
	/// Gets the high limit of the scan over which to compute
	/// </summary>
	public double MaximumWavelength
	{
		get
		{
			return _info.HighWavelength;
		}
		private set
		{
			_info.HighWavelength = value;
		}
	}

	/// <summary>
	/// Gets the low limit of the scan over which to compute
	/// </summary>
	public double MinimumWavelength
	{
		get
		{
			return _info.LowWavelength;
		}
		private set
		{
			_info.LowWavelength = value;
		}
	}

	/// <summary>
	/// Gets the max of a scan must be greater than this to be included
	/// </summary>
	public int ScanThreshold
	{
		get
		{
			return _info.ScanThreshold;
		}
		private set
		{
			_info.ScanThreshold = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.PeakPurity" /> class.
	/// </summary>
	public PeakPurity()
	{
		EnableDetection = false;
		ScanThreshold = 3;
		DesiredPeakCoverage = 90.0;
		MinimumWavelength = 190.0;
		MaximumWavelength = 800.0;
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
	/// The number of bytes loaded
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		_info = Utilities.ReadStructure<PeakPurityInfo>(viewer, ref startPos, fileRevision, MarshalledSizes);
		return startPos - dataOffset;
	}
}
