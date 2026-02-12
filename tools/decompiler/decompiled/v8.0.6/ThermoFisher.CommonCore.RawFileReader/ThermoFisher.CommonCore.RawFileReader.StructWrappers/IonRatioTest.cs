using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The ion ratio test settings
/// </summary>
internal class IonRatioTest : IIonRatioConfirmationTestAccess, IRawObjectBase
{
	/// <summary>
	/// The ion ratio test info. struct when file rev &gt;=56
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct IonRatioTestInfo
	{
		public double Mass;

		public int UnusedTargetPercent;

		public int UnusedWindowPercent;

		public double TargetPercent;

		public double WindowPercent;
	}

	/// <summary>
	/// The ion ratio test 55. struct when file rev less than 56
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct IonRatioTest55
	{
		public double Mass;

		public int TargetPercent;

		public int WindowPercent;
	}

	private static readonly int[,] MarshalledSizes = new int[2, 2]
	{
		{
			56,
			Marshal.SizeOf(typeof(IonRatioTestInfo))
		},
		{
			0,
			Marshal.SizeOf(typeof(IonRatioTest55))
		}
	};

	private IonRatioTestInfo _info;

	/// <summary>
	/// Gets the Mass to be tested
	/// </summary>
	public double MZ => _info.Mass;

	/// <summary>
	/// Gets the Expected ratio 
	/// The ratio of the qualifier ion response to the component ion response. 
	/// Range: 0 - 200%
	/// </summary>
	public double TargetRatio => _info.TargetPercent;

	/// <summary>
	/// Gets the Window to determine how accurate the match must be
	/// The ratio must be +/- this percentage.
	/// </summary>
	public double WindowPercent => _info.WindowPercent;

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
		_info = Utilities.ReadStructure<IonRatioTestInfo>(viewer, ref startPos, fileRevision, MarshalledSizes);
		if (fileRevision < 56)
		{
			_info.TargetPercent = _info.UnusedTargetPercent;
			_info.WindowPercent = _info.UnusedWindowPercent;
		}
		return startPos - dataOffset;
	}
}
