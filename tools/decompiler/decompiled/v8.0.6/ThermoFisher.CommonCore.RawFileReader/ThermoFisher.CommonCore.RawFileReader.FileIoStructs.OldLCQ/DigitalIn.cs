using System.Collections;
using System.Linq;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The digital in (legacy LCQ).
/// </summary>
internal struct DigitalIn
{
	private readonly int[] _bits;

	/// <summary>
	/// Gets the user digital in 2.
	/// </summary>
	public byte UserDin2 => (byte)_bits[0];

	/// <summary>
	/// Gets the user digital in 1.
	/// </summary>
	public byte UserDin1 => (byte)_bits[1];

	/// <summary>
	/// Gets the divert inject bit 1.
	/// </summary>
	public byte DivertInjectBit1 => (byte)_bits[2];

	/// <summary>
	/// Gets the APC ITC fail.
	/// </summary>
	public byte ApcItcFail => (byte)_bits[3];

	/// <summary>
	/// Gets the capillary RTD fail.
	/// </summary>
	public byte CapillaryRtdFail => (byte)_bits[4];

	/// <summary>
	/// Gets the ion gauge pressure OK.
	/// </summary>
	public byte IonGaugePressureOk => (byte)_bits[5];

	/// <summary>
	/// Gets the load inject.
	/// </summary>
	public byte LoadInject => (byte)_bits[6];

	/// <summary>
	/// Gets the KVCL 8.
	/// </summary>
	public byte Kvcl8 => (byte)_bits[7];

	/// <summary>
	/// Gets the <c>octapole</c> frequency on.
	/// </summary>
	public byte OctFreqOn => (byte)_bits[8];

	/// <summary>
	/// Gets the vacuum OK.
	/// </summary>
	public byte VacuumOk => (byte)_bits[9];

	/// <summary>
	/// Gets the <c>conv</c> pressure OK.
	/// </summary>
	public byte ConvPressOk => (byte)_bits[10];

	/// <summary>
	/// Gets the ion gauge on.
	/// </summary>
	public byte IonGaugeOn => (byte)_bits[11];

	/// <summary>
	/// Gets the ref sine on.
	/// </summary>
	public byte RefSineOn => (byte)_bits[12];

	/// <summary>
	/// Gets the SWR fail.
	/// </summary>
	public byte SwrFail => (byte)_bits[13];

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ.DigitalIn" /> struct.
	/// </summary>
	/// <param name="din">
	/// The digital in.
	/// </param>
	public DigitalIn(float din)
	{
		int num = (int)din;
		BitArray source = new BitArray(new int[1] { num });
		_bits = (from bool bit in source
			select bit ? 1 : 0).ToArray();
	}
}
