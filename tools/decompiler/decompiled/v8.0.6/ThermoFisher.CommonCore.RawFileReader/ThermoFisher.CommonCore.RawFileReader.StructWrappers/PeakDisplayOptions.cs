using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The peak display options (as used in PMD files).
/// </summary>
internal class PeakDisplayOptions : IPeakDisplayOptions, IRawObjectBase
{
	/// <summary>
	/// The peak options info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct PeakOptionsInfo
	{
		public XcaliburDisplayFlags Flags;

		public double ExcessPeakWidth;
	}

	private static readonly int[,] MarshalledSizes = new int[1, 2] { 
	{
		0,
		Marshal.SizeOf(typeof(PeakOptionsInfo))
	} };

	private PeakOptionsInfo _info;

	/// <summary>
	/// Gets a value which extends (display) width so that peak is shown "not at edge"
	/// </summary>
	public double ExcessWidth
	{
		get
		{
			return _info.ExcessPeakWidth;
		}
		private set
		{
			_info.ExcessPeakWidth = value;
		}
	}

	/// <summary>
	/// Gets a value indicating whether to label peaks with RT
	/// </summary>
	public bool LabelWithRetentionTime => (_info.Flags & XcaliburDisplayFlags.LabelWithRetentionTime) != 0;

	/// <summary>
	/// Gets a value indicating whether to label peaks with scan number
	/// </summary>
	public bool LabelWithScanNumber => (_info.Flags & XcaliburDisplayFlags.LabelWithScanNumber) != 0;

	/// <summary>
	/// Gets a value indicating whether to label peaks with area
	/// </summary>
	public bool LabelWithArea => (_info.Flags & XcaliburDisplayFlags.LabelWithArea) != 0;

	/// <summary>
	/// Gets a value indicating whether to label peaks with base peak
	/// </summary>
	public bool LabelWithBasePeak => (_info.Flags & XcaliburDisplayFlags.LabelWithBasePeak) != 0;

	/// <summary>
	/// Gets a value indicating whether to label peaks with height
	/// </summary>
	public bool LabelWithHeight => (_info.Flags & XcaliburDisplayFlags.LabelWithHeight) != 0;

	/// <summary>
	/// Gets a value indicating whether to label peaks with internal standard response
	/// </summary>
	public bool LabelWithIstdResp => (_info.Flags & XcaliburDisplayFlags.LabelWithIstdResp) != 0;

	/// <summary>
	/// Gets a value indicating whether to label peaks with signal to noise
	/// </summary>
	public bool LabelWithSignalToNoise => (_info.Flags & XcaliburDisplayFlags.LabelWithSignalToNoise) != 0;

	/// <summary>
	/// Gets a value indicating whether to label peaks with saturation
	/// </summary>
	public bool LabelWithSaturationFlag => (_info.Flags & XcaliburDisplayFlags.LabelWithSaturation) != 0;

	/// <summary>
	/// Gets a value indicating whether to rotate peak label text
	/// </summary>
	public bool LabelRotated => ((uint)_info.Flags & 0x80000000u) != 0;

	/// <summary>
	/// Gets a value indicating whether to draw a box around peak labels
	/// </summary>
	public bool LabelBoxed => (_info.Flags & XcaliburDisplayFlags.LabelBoxed) != 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.PeakDisplayOptions" /> class.
	/// </summary>
	public PeakDisplayOptions()
	{
		_info.Flags |= XcaliburDisplayFlags.LabelWithRetentionTime;
		ExcessWidth = 0.01;
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
		_info = Utilities.ReadStructure<PeakOptionsInfo>(viewer, ref startPos, fileRevision, MarshalledSizes);
		return startPos - dataOffset;
	}
}
