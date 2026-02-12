using System.Runtime.CompilerServices;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanEventInfo;

/// <summary>
/// The scan event info struct.
/// </summary>
internal struct ScanEventInfoStruct
{
	internal byte IsValid;

	/// <summary>
	/// Set to TRUE if trailer scan event should be used.
	/// </summary>
	internal byte IsCustom;

	internal byte Corona;

	/// <summary>
	/// Set to SFDetectorValid if detector value is valid.
	/// </summary>
	internal byte Detector;

	internal byte Polarity;

	internal byte ScanDataType;

	internal sbyte MSOrder;

	internal byte ScanType;

	internal byte SourceFragmentation;

	internal byte TurboScan;

	internal byte DependentData;

	internal byte IonizationMode;

	internal UpperCaseFilterFlags UpperFlags;

	internal double DetectorValue;

	/// <summary>
	/// Indicates how source fragmentation values are interpreted
	/// </summary>
	internal byte SourceFragmentationType;

	internal ushort LowerFlags;

	/// <summary>
	///     Scan Type Index indicates the segment/scan event for this filter scan event.
	///     HIWORD == segment, LOWORD == scan type
	/// </summary>
	internal int ScanTypeIndex;

	internal byte Wideband;

	/// <summary>
	/// Will be translated to Scan Filter's Accurate Mass enumeration.
	/// </summary>
	internal ScanFilterEnums.ScanEventAccurateMassTypes AccurateMassType;

	internal byte MassAnalyzerType;

	internal byte SectorScan;

	internal byte Lock;

	internal byte FreeRegion;

	internal byte Ultra;

	internal byte Enhanced;

	internal byte MultiPhotonDissociationType;

	internal double MultiPhotonDissociation;

	internal byte ElectronCaptureDissociationType;

	internal double ElectronCaptureDissociation;

	internal byte PhotoIonization;

	internal byte PulsedQDissociationType;

	internal double PulsedQDissociation;

	internal byte ElectronTransferDissociationType;

	internal double ElectronTransferDissociation;

	internal byte HigherEnergyCIDType;

	internal double HigherEnergyCID;

	internal byte SupplementalActivation;

	internal byte MultiStateActivation;

	internal byte CompensationVoltage;

	internal byte CompensationVoltageType;

	internal byte Multiplex;

	internal byte ParamA;

	internal byte ParamB;

	internal byte ParamF;

	internal byte SpsMultiNotch;

	internal byte ParamR;

	internal byte ParamV;

	/// <summary>
	/// Compares items in this event, up to the reactions tests.
	/// Reaction tests are done differently by various callers.
	/// </summary>
	/// <param name="other">The other.</param>
	/// <returns>
	/// A 32-bit signed integer that indicates the relative order of the objects being compared.
	///  The return value has the following meanings: 
	///  Value              Meaning 
	///  Less than zero     This object is less than the <paramref name="other" /> parameter.
	///  Zero               This object is equal to <paramref name="other" />. 
	///  Greater than zero  This object is greater than <paramref name="other" />. 
	/// </returns>
	internal int ComparePart1(ScanEventInfoStruct other)
	{
		int num = ScanDataType - other.ScanDataType;
		if (num != 0)
		{
			return num;
		}
		num = MSOrder - other.MSOrder;
		if (num != 0)
		{
			return num;
		}
		num = Polarity - other.Polarity;
		if (num != 0)
		{
			return num;
		}
		num = DependentData - other.DependentData;
		if (num != 0)
		{
			return num;
		}
		num = Multiplex - other.Multiplex;
		if (num != 0)
		{
			return num;
		}
		num = ParamA - other.ParamA;
		if (num != 0)
		{
			return num;
		}
		num = ParamB - other.ParamB;
		if (num != 0)
		{
			return num;
		}
		num = ParamF - other.ParamF;
		if (num != 0)
		{
			return num;
		}
		num = SpsMultiNotch - other.SpsMultiNotch;
		if (num != 0)
		{
			return num;
		}
		num = ParamR - other.ParamR;
		if (num != 0)
		{
			return num;
		}
		num = ParamV - other.ParamV;
		if (num != 0)
		{
			return num;
		}
		num = Detector - other.Detector;
		if (num != 0)
		{
			return num;
		}
		if (Detector == 0 && other.Detector == 0)
		{
			if (DetectorValue > other.DetectorValue + 0.001)
			{
				return 1;
			}
			if (DetectorValue < other.DetectorValue - 0.001)
			{
				return -1;
			}
		}
		num = CompensationVoltage - other.CompensationVoltage;
		if (num != 0)
		{
			return num;
		}
		num = ScanType - other.ScanType;
		if (num != 0)
		{
			return num;
		}
		num = Wideband - other.Wideband;
		if (num != 0)
		{
			return num;
		}
		num = SupplementalActivation - other.SupplementalActivation;
		if (num != 0)
		{
			return num;
		}
		num = MultiStateActivation - other.MultiStateActivation;
		if (num != 0)
		{
			return num;
		}
		num = AccurateMassType - other.AccurateMassType;
		if (num != 0)
		{
			return num;
		}
		num = IonizationMode - other.IonizationMode;
		if (num != 0)
		{
			return num;
		}
		return Corona - other.Corona;
	}

	/// <summary>
	/// A method which merges 8 byte sized flags into a unique 64 bit code
	/// For faster sorting
	/// </summary>
	/// <returns>Has of first 8 items</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal long GetHash1()
	{
		int num = ((byte)MSOrder << 24) | (ScanDataType << 16) | (Polarity << 8) | DependentData;
		int num2 = (Multiplex << 24) | (ParamA << 16) | (ParamB << 8) | ParamF;
		return (uint)num | ((long)num2 << 32);
	}

	/// <summary>
	/// A method which precicely merges 8 byte sized flags into a unique 64 bit code
	/// For faster sorting
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal long GetHash2()
	{
		int num = (SpsMultiNotch << 24) | (ParamR << 16) | (ParamV << 8) | Detector;
		int num2 = (CompensationVoltage << 24) | (ScanType << 16) | (Wideband << 8) | SupplementalActivation;
		return (uint)num | ((long)num2 << 32);
	}

	/// <summary>
	/// A method which precicely merges 8 byte sized flags into a unique 64 bit code
	/// For faster sorting
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal long GetHash3()
	{
		int num = (SourceFragmentationType << 24) | (CompensationVoltageType << 16) | (MassAnalyzerType << 8) | SectorScan;
		int num2 = (FreeRegion << 24) | (Ultra << 16) | (Enhanced << 8) | MultiPhotonDissociationType;
		return (uint)num | ((long)num2 << 32);
	}

	/// <summary>
	/// A method which precisely merges 5 small (less than 6 bit) flags into a unique 32 bit code
	/// Then adds the "scan type index" as a second 32 bits
	/// For faster sorting
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal long GetHash4()
	{
		int num = (ElectronCaptureDissociationType << 24) | (PulsedQDissociationType << 18) | (ElectronTransferDissociationType << 12) | (HigherEnergyCIDType << 6) | PhotoIonization;
		return ((long)ScanTypeIndex << 32) | (uint)num;
	}

	/// <summary>
	/// Compares items in this event, up to the reactions tests.
	/// Reaction tests are done differently by various callers.
	/// Skips 8 items, which are tested by hash1
	/// </summary>
	/// <param name="other">The other.</param>
	/// <returns>
	/// A 32-bit signed integer that indicates the relative order of the objects being compared.
	///  The return value has the following meanings: 
	///  Value              Meaning 
	///  Less than zero     This object is less than the <paramref name="other" /> parameter.
	///  Zero               This object is equal to <paramref name="other" />. 
	///  Greater than zero  This object is greater than <paramref name="other" />. 
	/// </returns>
	internal int ComparePart1Hash1(ScanEventInfoStruct other)
	{
		int num = SpsMultiNotch - other.SpsMultiNotch;
		if (num != 0)
		{
			return num;
		}
		num = ParamR - other.ParamR;
		if (num != 0)
		{
			return num;
		}
		num = ParamV - other.ParamV;
		if (num != 0)
		{
			return num;
		}
		num = Detector - other.Detector;
		if (num != 0)
		{
			return num;
		}
		if (Detector == 0 && other.Detector == 0)
		{
			if (DetectorValue > other.DetectorValue + 0.001)
			{
				return 1;
			}
			if (DetectorValue < other.DetectorValue - 0.001)
			{
				return -1;
			}
		}
		num = CompensationVoltage - other.CompensationVoltage;
		if (num != 0)
		{
			return num;
		}
		num = ScanType - other.ScanType;
		if (num != 0)
		{
			return num;
		}
		num = Wideband - other.Wideband;
		if (num != 0)
		{
			return num;
		}
		num = SupplementalActivation - other.SupplementalActivation;
		if (num != 0)
		{
			return num;
		}
		num = MultiStateActivation - other.MultiStateActivation;
		if (num != 0)
		{
			return num;
		}
		num = AccurateMassType - other.AccurateMassType;
		if (num != 0)
		{
			return num;
		}
		num = IonizationMode - other.IonizationMode;
		if (num != 0)
		{
			return num;
		}
		return Corona - other.Corona;
	}

	/// <summary>
	/// Compares items in this event, up to the reactions tests.
	/// Reaction tests are done differently by various callers.
	/// Skips 8 items, which are tested by hash1 and  8 tested by hash2
	/// </summary>
	/// <param name="other">The other.</param>
	/// <returns>
	/// A 32-bit signed integer that indicates the relative order of the objects being compared.
	///  The return value has the following meanings: 
	///  Value              Meaning 
	///  Less than zero     This object is less than the <paramref name="other" /> parameter.
	///  Zero               This object is equal to <paramref name="other" />. 
	///  Greater than zero  This object is greater than <paramref name="other" />. 
	/// </returns>
	internal int ComparePart1Hash1Hash2(ScanEventInfoStruct other)
	{
		if (Detector == 0 && other.Detector == 0)
		{
			if (DetectorValue > other.DetectorValue + 0.001)
			{
				return 1;
			}
			if (DetectorValue < other.DetectorValue - 0.001)
			{
				return -1;
			}
		}
		int num = MultiStateActivation - other.MultiStateActivation;
		if (num != 0)
		{
			return num;
		}
		num = AccurateMassType - other.AccurateMassType;
		if (num != 0)
		{
			return num;
		}
		num = IonizationMode - other.IonizationMode;
		if (num != 0)
		{
			return num;
		}
		return Corona - other.Corona;
	}
}
