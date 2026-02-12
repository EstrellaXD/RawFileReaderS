using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The tune data, from legacy LCQ files.
/// </summary>
internal sealed class TuneData : IRawObjectBase
{
	private TuneDataStruct _tuneDataStructInfo;

	/// <summary>
	/// Gets the tune data struct info.
	/// </summary>
	public TuneDataStruct TuneDataStructInfo => _tuneDataStructInfo;

	/// <summary>
	/// Gets the norm. UNUSED -- RF DAC for normal scan rate
	/// </summary>
	public SlopeInterceptStruct Norm { get; private set; }

	/// <summary>
	/// Gets the AGC. UNUSED -- RF DAC for AGC scan rate
	/// </summary>
	public SlopeInterceptStruct Agc { get; private set; }

	/// <summary>
	/// Gets the high res. UNUSED -- RF DAC for high-res scan rate
	/// </summary>
	public SlopeInterceptStruct HighRes { get; private set; }

	/// <summary>
	/// Gets the fast. UNUSED -- RF DAC for fast scan rate
	/// </summary>
	public SlopeInterceptStruct Fast { get; private set; }

	/// <summary>
	/// Gets the <c>vernier</c>. UNUSED -- RF DAC for <c>vernier</c>
	/// </summary>
	public SlopeInterceptStruct Vernier { get; private set; }

	/// <summary>
	/// Gets the cal q 1. UNUSED -- RF DAC for static operation at cal q 1
	/// </summary>
	public SlopeInterceptStruct CalQ1 { get; private set; }

	/// <summary>
	/// Gets the cal q 2.  UNUSED -- RF DAC for static operation at cal q 2
	/// </summary>
	public SlopeInterceptStruct CalQ2 { get; private set; }

	/// <summary>
	/// Gets the norm Resonance ejection amplitude. UNUSED -- Resonance ejection amplitude for normal scan rate
	/// </summary>
	public SlopeInterceptStruct NormRea { get; private set; }

	/// <summary>
	/// Gets the AGC REA. UNUSED -- Resonance ejection amplitude for AGC scan rate
	/// </summary>
	public SlopeInterceptStruct AgcRea { get; private set; }

	/// <summary>
	/// Gets the high res REA. UNUSED -- Resonance ejection amplitude for High Res scan rate
	/// </summary>
	public SlopeInterceptStruct HighResRea { get; private set; }

	/// <summary>
	/// Gets the fast REA. UNUSED -- Resonance ejection amplitude for Fast scan rate
	/// </summary>
	public SlopeInterceptStruct FastRea { get; private set; }

	/// <summary>
	/// Gets the isolation waveform amp. UNUSED -- Isolation waveform amplitude
	/// </summary>
	public SlopeInterceptStruct IsolationWfAmp { get; private set; }

	/// <summary>
	/// Gets the injection RF. UNUSED: Ion injection RF frequency slope and intercept
	/// </summary>
	public SlopeInterceptStruct InjectionRf { get; private set; }

	/// <summary>
	/// Gets the tube cal voltages. UNUSED -- Tube lens calibration voltages.  Presented to the user as 2x10 2-d array.
	/// </summary>
	public double[] TubeCalVoltages { get; private set; }

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
		if (fileRevision >= 6)
		{
			_tuneDataStructInfo = viewer.ReadStructureExt<TuneDataStruct>(ref startPos);
		}
		else if (fileRevision >= 3)
		{
			_tuneDataStructInfo = viewer.ReadPreviousRevisionAndConvertExt<TuneDataStruct, TuneDataStruct3>(ref startPos);
		}
		else
		{
			_tuneDataStructInfo = viewer.ReadPreviousRevisionAndConvertExt<TuneDataStruct, TuneDataStruct1>(ref startPos);
		}
		FixUpTuneDataDefaults(fileRevision);
		Norm = viewer.ReadStructureExt<SlopeInterceptStruct>(ref startPos);
		Agc = viewer.ReadStructureExt<SlopeInterceptStruct>(ref startPos);
		HighRes = viewer.ReadStructureExt<SlopeInterceptStruct>(ref startPos);
		Fast = viewer.ReadStructureExt<SlopeInterceptStruct>(ref startPos);
		Vernier = viewer.ReadStructureExt<SlopeInterceptStruct>(ref startPos);
		CalQ1 = viewer.ReadStructureExt<SlopeInterceptStruct>(ref startPos);
		CalQ2 = viewer.ReadStructureExt<SlopeInterceptStruct>(ref startPos);
		NormRea = viewer.ReadStructureExt<SlopeInterceptStruct>(ref startPos);
		AgcRea = viewer.ReadStructureExt<SlopeInterceptStruct>(ref startPos);
		HighResRea = viewer.ReadStructureExt<SlopeInterceptStruct>(ref startPos);
		FastRea = viewer.ReadStructureExt<SlopeInterceptStruct>(ref startPos);
		IsolationWfAmp = viewer.ReadStructureExt<SlopeInterceptStruct>(ref startPos);
		InjectionRf = viewer.ReadStructureExt<SlopeInterceptStruct>(ref startPos);
		TubeCalVoltages = viewer.ReadDoublesExt(ref startPos);
		return startPos - dataOffset;
	}

	/// <summary>
	/// fix up tune data defaults.
	/// </summary>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	private void FixUpTuneDataDefaults(int fileRevision)
	{
		if (fileRevision < 3)
		{
			_tuneDataStructInfo.InjWaveformFlag = 0;
		}
		if (fileRevision < 6)
		{
			_tuneDataStructInfo.AGCOn = true;
		}
		if (fileRevision < 10 && _tuneDataStructInfo.Polarity == ScanFilterEnums.PolarityTypes.Negative)
		{
			_tuneDataStructInfo.TubeAdjust = -1.0 * _tuneDataStructInfo.TubeAdjust;
		}
		if (_tuneDataStructInfo.SheathGasFlow < 20.0)
		{
			_tuneDataStructInfo.SheathGasFlow = 20.0;
		}
	}
}
