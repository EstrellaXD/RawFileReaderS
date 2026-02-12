using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;

/// <summary>
/// Tune data structure version 1
/// </summary>
internal struct TuneDataStruct1
{
	internal double CapTemperature;

	internal double APCIVapTemp;

	internal double SourceHighVoltage;

	internal double SourceCurrent;

	internal double SheathGasFlow;

	internal double AuxGasFlow;

	internal double CapVoltage;

	internal double OctapoleRFAmp;

	internal double Octapole1Offset;

	internal double Octapole2Offset;

	internal double InterOctapoleLensVoltage;

	internal double TrapOffsetVoltage;

	internal double MultiplierVoltage;

	internal double TubeAdjust;

	internal double DataType;

	internal double FSRFMass1;

	internal double FSRFMass2;

	internal double FSRFMass3;

	internal TuneDataEnums.SourceType SourceType;

	internal ScanFilterEnums.PolarityTypes Polarity;

	internal int UsZoom;

	internal int UsFull;

	internal int UsSIM;

	internal int UsMSn;

	internal double AGCZoom;

	internal double AGCFull;

	internal double AGCSIM;

	internal double AGCMSn;

	internal double MaxIonTime;

	internal double IonTime;
}
