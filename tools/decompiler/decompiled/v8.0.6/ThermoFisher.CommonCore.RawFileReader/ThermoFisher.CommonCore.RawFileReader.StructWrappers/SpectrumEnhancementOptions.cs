using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The spectrum enhancement options.
/// </summary>
internal class SpectrumEnhancementOptions : ISpectrumEnhancementAccess, IRawObjectBase
{
	/// <summary>
	/// Spectrum enhancement binary data.
	/// Defined a public fields, matching C++ definition
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct SpecEnhancementInfo
	{
		public bool SpecEnabled;

		public SpectrumEnhanceMode SpectrumMethod;

		public double WindowSize;

		public int NoiseThreshold;

		public double Take;

		public double BgScalingFactor;

		public SpectrumSubtractMethod Region1Method;

		public double PtsPkTop;

		public double Previous;

		public SpectrumSubtractMethod Region2Method;

		public double PtsAfterPkTop;

		public double NextPt;

		public double CutOffThreshold;
	}

	private SpecEnhancementInfo _info;

	/// <summary>
	/// Gets a value indicating whether the refine (combine) section enabled
	/// </summary>
	public bool Enabled
	{
		get
		{
			return _info.SpecEnabled;
		}
		private set
		{
			_info.SpecEnabled = value;
		}
	}

	/// <summary>
	/// Gets a value indicating we using refine, combine or threshold
	/// </summary>
	public SpectrumEnhanceMode SpectrumMethod
	{
		get
		{
			return _info.SpectrumMethod;
		}
		private set
		{
			_info.SpectrumMethod = value;
		}
	}

	/// <summary>
	/// Gets the refine window size
	/// </summary>
	public double RefineWindowSize
	{
		get
		{
			return _info.WindowSize;
		}
		private set
		{
			_info.WindowSize = value;
		}
	}

	/// <summary>
	/// Gets he refine noise threshold
	/// </summary>
	public int RefineNoiseThreshold
	{
		get
		{
			return _info.NoiseThreshold;
		}
		private set
		{
			_info.NoiseThreshold = value;
		}
	}

	/// <summary>
	/// Gets the combine take points across peak top
	/// </summary>
	public double TopRegionWidth
	{
		get
		{
			return _info.Take;
		}
		private set
		{
			_info.Take = value;
		}
	}

	/// <summary>
	/// Gets the combine background scaling factor
	/// </summary>
	public double BackgroundScalingFactor
	{
		get
		{
			return _info.BgScalingFactor;
		}
		private set
		{
			_info.BgScalingFactor = value;
		}
	}

	/// <summary>
	/// Gets the Region 1 method: at peak or use previous
	/// </summary>
	public SpectrumSubtractMethod LeftRegionMethod
	{
		get
		{
			return _info.Region1Method;
		}
		private set
		{
			_info.Region1Method = value;
		}
	}

	/// <summary>
	/// Gets the start point before the peak top
	/// </summary>
	public double PointsBeforePeakTop
	{
		get
		{
			return _info.PtsPkTop;
		}
		private set
		{
			_info.PtsPkTop = value;
		}
	}

	/// <summary>
	/// Gets the previous points in background
	/// </summary>
	public double LeftRegionPoints
	{
		get
		{
			return _info.Previous;
		}
		private set
		{
			_info.Previous = value;
		}
	}

	/// <summary>
	/// Gets Region 2 method: at peak or use next
	/// </summary>
	public SpectrumSubtractMethod RightRegionMethod
	{
		get
		{
			return _info.Region2Method;
		}
		private set
		{
			_info.Region2Method = value;
		}
	}

	/// <summary>
	/// Gets Points after peak top
	/// </summary>
	public double PointsAfterPeakTop
	{
		get
		{
			return _info.PtsAfterPkTop;
		}
		private set
		{
			_info.PtsAfterPkTop = value;
		}
	}

	/// <summary>
	/// Gets the next point in background
	/// </summary>
	public double RightRegionPoints
	{
		get
		{
			return _info.NextPt;
		}
		private set
		{
			_info.NextPt = value;
		}
	}

	/// <summary>
	/// Gets the cut off threshold
	/// </summary>
	public double CutOffThreshold
	{
		get
		{
			return _info.CutOffThreshold;
		}
		private set
		{
			_info.CutOffThreshold = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.SpectrumEnhancementOptions" /> class.
	/// </summary>
	public SpectrumEnhancementOptions()
	{
		Enabled = false;
		SpectrumMethod = SpectrumEnhanceMode.Refine;
		RefineWindowSize = 6.0;
		RefineNoiseThreshold = 3;
		TopRegionWidth = 4.0;
		BackgroundScalingFactor = 1.0;
		LeftRegionMethod = SpectrumSubtractMethod.AtPeak;
		PointsBeforePeakTop = 4.0;
		LeftRegionPoints = 5.0;
		RightRegionMethod = SpectrumSubtractMethod.AtPeak;
		PointsAfterPeakTop = 6.0;
		RightRegionPoints = 5.0;
		CutOffThreshold = 0.0;
	}

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		_info = viewer.ReadStructureExt<SpecEnhancementInfo>(ref startPos);
		return startPos - dataOffset;
	}
}
