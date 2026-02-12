namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Enumeration of trace types, for chromatograms.
/// Note: legacy C++ file reader does not support
/// analog trace numbers above "4" or UV above "channel D".
/// Traces are organized in blocks of 10
/// For example: 
/// <c>
/// StartAnalogChromatogramTraces=10    (not a valid trace type, just a limit)
/// Analog1 to Analog8 = 11 to 18
/// EndAnalogChromatogramTraces =19     (not a valid trace type, just a limit)
/// Next block:
/// StartPDAChromatogramTraces = 20
/// Etc.
/// </c>
/// </summary>
public enum TraceType
{
	/// <summary>
	/// Marks the start of the MS trace types (this enum value +1)
	/// </summary>
	StartMSChromatogramTraces = -1,
	/// <summary>
	/// Chromatogram summing between two masses
	/// </summary>
	MassRange = 0,
	/// <summary>
	/// Total Ion Current
	/// </summary>
	TIC = 1,
	/// <summary>
	/// Largest peak in scan (or mass range in scan)
	/// </summary>
	BasePeak = 2,
	/// <summary>
	/// Neutral fragment 
	/// </summary>
	Fragment = 3,
	/// <summary>
	/// An algorithm is provided by the caller
	/// </summary>
	Custom = 4,
	/// <summary>
	/// Returns the precursor mass as a given index
	/// </summary>
	PrecursorMass = 5,
	/// <summary>
	/// Marks the end of the MS trace types (this enum value -1)
	/// </summary>
	EndMSChromatogramTraces = 6,
	/// <summary>
	/// Marks the start of the Analog trace types (this enum value +1)
	/// </summary>
	StartAnalogChromatogramTraces = 10,
	/// <summary>
	/// Analog channel 1
	/// </summary>
	Analog1 = 11,
	/// <summary>
	/// Analog channel 2
	/// </summary>
	Analog2 = 12,
	/// <summary>
	/// Analog channel 3
	/// </summary>
	Analog3 = 13,
	/// <summary>
	/// Analog channel 4
	/// </summary>
	Analog4 = 14,
	/// <summary>
	/// Analog channel 5
	/// </summary>
	Analog5 = 15,
	/// <summary>
	/// Analog channel 6
	/// </summary>
	Analog6 = 16,
	/// <summary>
	/// Analog channel 6
	/// </summary>
	Analog7 = 17,
	/// <summary>
	/// Analog channel 6
	/// </summary>
	Analog8 = 18,
	/// <summary>
	/// Marks the end of the Analog trace types (this enum value -1)
	/// </summary>
	EndAnalogChromatogramTraces = 19,
	/// <summary>
	/// Marks the start of the PDA trace types (this enum value +1)
	/// </summary>
	StartPDAChromatogramTraces = 20,
	/// <summary>
	/// Sum of values over a range of wavelengths
	/// </summary>
	WavelengthRange = 21,
	/// <summary>
	/// Average of all values in PDA scan
	/// </summary>
	TotalAbsorbance = 22,
	/// <summary>
	/// Largest value in scan (or wavelength range in scan)
	/// </summary>
	SpectrumMax = 23,
	/// <summary>
	/// Marks the end of the PDA trace types (this enum value -1)
	/// </summary>
	EndPDAChromatogramTraces = 24,
	/// <summary>
	/// Marks the start of the UV trace types (this enum value +1)
	/// </summary>
	StartUVChromatogramTraces = 30,
	/// <summary>
	/// UV Channel A
	/// </summary>
	ChannelA = 31,
	/// <summary>
	/// UV Channel B
	/// </summary>
	ChannelB = 32,
	/// <summary>
	/// UV Channel C
	/// </summary>
	ChannelC = 33,
	/// <summary>
	/// UV Channel D
	/// </summary>
	ChannelD = 34,
	/// <summary>
	/// UV Channel E
	/// </summary>
	ChannelE = 35,
	/// <summary>
	/// UV Channel F
	/// </summary>
	ChannelF = 36,
	/// <summary>
	/// UV Channel G
	/// </summary>
	ChannelG = 37,
	/// <summary>
	/// UV Channel H
	/// </summary>
	ChannelH = 38,
	/// <summary>
	/// Marks the end of the UV trace types (this enum value -1)
	/// </summary>
	EndUVChromatogramTraces = 39,
	/// <summary>
	/// A to D converter channels start at this +1
	/// </summary>
	StartPCA2DChromatogramTraces = 40,
	/// <summary>
	/// A to D channel 1
	/// </summary>
	A2DChannel1 = 41,
	/// <summary>
	/// A to D channel 2
	/// </summary>
	A2DChannel2 = 42,
	/// <summary>
	/// A to D channel 3
	/// </summary>
	A2DChannel3 = 43,
	/// <summary>
	/// A to D channel 3 (old naming convention)
	/// use "A2DChannel3" in new code
	/// </summary>
	ChromatogramA2DChannel3 = 43,
	/// <summary>
	/// A to D channel 4
	/// </summary>
	A2DChannel4 = 44,
	/// <summary>
	/// A to D channel 4 (old naming convention)
	/// use "A2DChannel4" in new code
	/// </summary>
	ChromatogramA2DChannel4 = 44,
	/// <summary>
	/// A to D channel 5
	/// </summary>
	A2DChannel5 = 45,
	/// <summary>
	/// A to D channel 6
	/// </summary>
	A2DChannel6 = 46,
	/// <summary>
	/// A to D channel 7
	/// </summary>
	A2DChannel7 = 47,
	/// <summary>
	/// A to D channel 8
	/// </summary>
	A2DChannel8 = 48,
	/// <summary>
	/// A to D converter channels end at this -1
	/// </summary>
	EndPCA2DChromatogramTraces = 49,
	/// <summary>
	/// Marks the start of all trace types (this enum value -1)
	/// </summary>
	EndAllChromatogramTraces = 50
}
