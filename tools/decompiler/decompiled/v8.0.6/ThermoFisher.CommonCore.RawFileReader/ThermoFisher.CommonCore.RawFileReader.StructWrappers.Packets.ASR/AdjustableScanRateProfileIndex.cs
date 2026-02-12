using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.UVPackets;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.ASR;

/// <summary>
/// The Adjustable Scan Rate profile index.
/// For scans (especially PDA) when the data is only intensity values
/// and the scan rate (x axis step, such as time, wavelength etc.) between values
/// is "adjusted" to the rate indicated by the index record.
/// </summary>
internal sealed class AdjustableScanRateProfileIndex : IRawObjectBase
{
	private AsrProfileIndexStruct _profileIndex;

	/// <summary>
	/// Gets or sets the Absorbance Unit's scale.
	/// </summary>
	internal double AbsorbanceUnitScale
	{
		get
		{
			return _profileIndex.AUScale;
		}
		set
		{
			_profileIndex.AUScale = value;
		}
	}

	/// <summary>
	/// Gets the Absorbance Unit's offset.
	/// </summary>
	internal double AbsorbanceUnitOffset => _profileIndex.AUOffset;

	/// <summary>
	/// Gets the data position.
	/// </summary>
	internal long DataPosition => _profileIndex.DataPosition;

	/// <summary>
	/// Gets a value indicating whether is valid scan.
	/// </summary>
	internal bool IsValidScan => _profileIndex.IsValidScan > 0;

	/// <summary>
	/// Gets the number of packets.
	/// </summary>
	internal uint NumberOfPackets => _profileIndex.NumberOfPackets;

	/// <summary>
	/// Gets the time wave length start.
	/// </summary>
	internal double TimeWavelengthStart => _profileIndex.TimeWavelengthStart;

	/// <summary>
	/// Gets the time wave length step.
	/// </summary>
	internal double TimeWavelengthStep => _profileIndex.TimeWavelengthStep;

	/// <summary>
	/// Gets the wave length end.
	/// </summary>
	internal uint WavelengthEnd => _profileIndex.WavelengthEnd;

	/// <summary>
	/// Gets the wave length start.
	/// </summary>
	internal uint WavelengthStart => _profileIndex.WavelengthStart;

	/// <summary>
	/// Load ASR Profile index data (from file).
	/// </summary>
	/// <param name="viewer">The viewer (memory map into file).</param>
	/// <param name="dataOffset">The data offset (into the memory map).</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		_profileIndex = ((fileRevision <= 63) ? viewer.ReadPreviousRevisionAndConvertExt<AsrProfileIndexStruct, AsrProfileIndexStructOld>(ref startPos) : viewer.ReadStructureExt<AsrProfileIndexStruct>(ref startPos));
		return startPos - dataOffset;
	}
}
