namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// The auto sampler info struct, from legacy LCQ files.
/// </summary>
internal struct AutoSamplerInfoStruct
{
	internal double InjectionSpeed;

	internal double InjectionSampleVol;

	internal double InjectionAirVol;

	internal double InjectionDelayTime;

	internal double InjectionHoldTime;

	internal double SamplePullupDelay;

	internal double SampleRinseVol;

	internal int InjectionOnColumn;

	internal int SampleRinseCycles;

	internal int SamplePullupCycles;

	internal int SolventRinseCycles;
}
