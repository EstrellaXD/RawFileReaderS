using System;
using System.Text;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanEventInfo;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The LCQ converter. Convert from legacy LCQ format to current raw data.
/// </summary>
internal static class LcqConverter
{
	private const double TicsPerMin = 600000.0;

	private static readonly Func<int, double> MsToMin = (int ms) => (double)ms / 600000.0;

	/// <summary>
	/// LCQs the write Xcalibur trailer header.
	/// </summary>
	/// <returns>The converted data descriptors</returns>
	internal static DataDescriptors LcqCreateXcalTrailerExtraHeader()
	{
		DataDescriptors dataDescriptors = new DataDescriptors(10);
		dataDescriptors.AddItem(new DataDescriptor("Micro Scan Count:", DataTypes.Short, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Ion Injection Time (ms):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Scan Segment:", DataTypes.Char, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Scan Event:", DataTypes.Char, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Elapsed Scan Time (sec):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("API Source CID Energy:", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Resolution:", DataTypes.CharString, 6u));
		dataDescriptors.AddItem(new DataDescriptor("Average Scan by Inst:", DataTypes.YesNo, 0u));
		dataDescriptors.AddItem(new DataDescriptor("BackGd Subtracted by Inst:", DataTypes.YesNo, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Charge State:", DataTypes.Short, 0u));
		return dataDescriptors;
	}

	/// <summary>
	/// converts The LCQ tune method header to Data Descriptor format.
	/// </summary>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.DataDescriptors" />.
	/// </returns>
	internal static DataDescriptors LcqCreateXcalTuneMethodHeader()
	{
		DataDescriptors dataDescriptors = new DataDescriptors(27);
		dataDescriptors.AddItem(new DataDescriptor("Capillary Temp (C):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("APCI Vaporizer Temp (C):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Source Voltage (kV):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Source Current (uA):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Sheath Gas Flow ():", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Aux Gas Flow ():", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Capillary Voltage (V):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Tube Lens Offset (V):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Octapole RF Amplifier (Vp-p):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Octapole 1 Offset (V):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Octapole 2 Offset (V):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("InterOctapole Lens Voltage (V):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Trap DC Offset Voltage (V):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Multiplier Voltage (V):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Maximum Ion Time (ms):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Ion Time (ms):", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Data Type:", DataTypes.WideCharString, 9u));
		dataDescriptors.AddItem(new DataDescriptor("Source Type:", DataTypes.WideCharString, 16u));
		dataDescriptors.AddItem(new DataDescriptor("Polarity:", DataTypes.WideCharString, 9u));
		dataDescriptors.AddItem(new DataDescriptor("Zoom Micro Scans:", DataTypes.Short, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Zoom Agc Target:", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Full Micro Scans:", DataTypes.Short, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Full Agc Target:", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("SIM Micro Scans:", DataTypes.Short, 0u));
		dataDescriptors.AddItem(new DataDescriptor("SIM Agc Target:", DataTypes.Double, 2u));
		dataDescriptors.AddItem(new DataDescriptor("MSn Micro Scans:", DataTypes.Short, 0u));
		dataDescriptors.AddItem(new DataDescriptor("MSn Agc Target:", DataTypes.Double, 2u));
		return dataDescriptors;
	}

	/// <summary>
	/// create xcalibur status log header from LCQ format log.
	/// To generate the trailer extra header for inclusion to the Xcalibur data file.
	/// </summary>
	/// <param name="incAs">if set to <c>true</c> include the auto sampler data.</param>
	/// <param name="incLc">if set to <c>true</c> include the LC data.</param>
	/// <returns>The converted log definition</returns>
	internal static DataDescriptors LcqCreateXcalStatusLogHeader(bool incAs, bool incLc)
	{
		int num = 96;
		if (!incAs)
		{
			num -= 2;
		}
		if (!incLc)
		{
			num -= 8;
		}
		DataDescriptors dataDescriptors = new DataDescriptors(num);
		dataDescriptors.AddItem(new DataDescriptor("API SOURCE", DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Source Voltage (kV):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Source Current (uA):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Vaporizer Thermocouple OK:", DataTypes.YesNo, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Vaporizer Temp (C):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Sheath Gas Flow Rate ():", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Aux Gas Flow Rate():", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Capillary RTD OK:", DataTypes.YesNo, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Capillary Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Capillary Temp (C):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("8 kV supply at limit:", DataTypes.YesNo, 0u));
		dataDescriptors.AddItem(new DataDescriptor(string.Empty, DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("VACUUM", DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Vacuum OK:", DataTypes.YesNo, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Ion Gauge Pressure OK:", DataTypes.YesNo, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Ion Gauge Status:", DataTypes.OnOff, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Ion Gauge (x10e-5 Torr):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Convectron Pressure OK:", DataTypes.YesNo, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Convectron Gauge (Torr):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor(string.Empty, DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("TURBO PUMP", DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Status:", DataTypes.WideCharString, 14u));
		dataDescriptors.AddItem(new DataDescriptor("Life (hours):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Speed (rpm):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Power (Watts):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Temperature (C):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor(string.Empty, DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("ION OPTICS", DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Octapole Frequency On:", DataTypes.YesNo, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Octapole 1 Offset (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Octapole 2 Offset (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Lens Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Trap DC Offset (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor(string.Empty, DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("MAIN RF", DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Reference Sine Wave OK:", DataTypes.YesNo, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Standing Wave Ratio Failed:", DataTypes.YesNo, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Main RF DAC (steps):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Main RF Detected (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("RF Detector Temp (C):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Main RF Modulation (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Main RF Amplifier (Vp/p):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("RF Generator Temp (C):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor(string.Empty, DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("ION DETECTION SYSTEM", DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Multiplier Actual (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor(string.Empty, DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("POWER SUPPLIES", DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("+5V Supply Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("-15V Supply Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("+15V Supply Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("+24V Supply Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("-28V Supply Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("+28V Supply Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("+28V Supply Current (Amps):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("+35V Supply Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("+36V Supply Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("-150V Supply Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("+150V Supply Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("-205V Supply Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("+205V Supply Voltage (V):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Ambient Temp (C):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor(string.Empty, DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("INSTRUMENT STATUS", DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Instrument:", DataTypes.WideCharString, 10u));
		dataDescriptors.AddItem(new DataDescriptor("Analysis:", DataTypes.WideCharString, 15u));
		dataDescriptors.AddItem(new DataDescriptor(string.Empty, DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("AUTOSAMPLER", DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Status:", DataTypes.WideCharString, 21u));
		if (incAs)
		{
			dataDescriptors.AddItem(new DataDescriptor("Current Vial Position:", DataTypes.Short, 0u));
			dataDescriptors.AddItem(new DataDescriptor("Number of Injection(s):", DataTypes.Short, 0u));
		}
		dataDescriptors.AddItem(new DataDescriptor(string.Empty, DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("LC PUMP", DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Status:", DataTypes.WideCharString, 22u));
		if (incLc)
		{
			dataDescriptors.AddItem(new DataDescriptor("Run Time (min):", DataTypes.Float, 2u));
			dataDescriptors.AddItem(new DataDescriptor("Flow Rate (mL/min):", DataTypes.Float, 2u));
			dataDescriptors.AddItem(new DataDescriptor("Pump Pressure (psi):", DataTypes.Float, 2u));
			dataDescriptors.AddItem(new DataDescriptor("Temperature (C):", DataTypes.Float, 2u));
			dataDescriptors.AddItem(new DataDescriptor("Composition A (%):", DataTypes.Float, 2u));
			dataDescriptors.AddItem(new DataDescriptor("Composition B (%):", DataTypes.Float, 2u));
			dataDescriptors.AddItem(new DataDescriptor("Composition C (%):", DataTypes.Float, 2u));
			dataDescriptors.AddItem(new DataDescriptor("Composition D (%):", DataTypes.Float, 2u));
		}
		dataDescriptors.AddItem(new DataDescriptor(string.Empty, DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("SYRINGE PUMP", DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Status:", DataTypes.WideCharString, 14u));
		dataDescriptors.AddItem(new DataDescriptor("Flow Rate (uL/min):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Infused Volume (uL):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor("Syringe Diameter (mm):", DataTypes.Float, 2u));
		dataDescriptors.AddItem(new DataDescriptor(string.Empty, DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("DIGITAL INPUTS", DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("READY IN is active:", DataTypes.TrueFalse, 0u));
		dataDescriptors.AddItem(new DataDescriptor("START IN is active:", DataTypes.TrueFalse, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Divert/Inject valve:", DataTypes.WideCharString, 7u));
		dataDescriptors.AddItem(new DataDescriptor(string.Empty, DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("UV DETECTOR", DataTypes.Empty, 0u));
		dataDescriptors.AddItem(new DataDescriptor("Status:", DataTypes.WideCharString, 14u));
		return dataDescriptors;
	}

	/// <summary>
	/// LCQs the trailer to xcalibur scan event.
	/// </summary>
	/// <param name="filterMassPrecision">precision for formatting mass</param>
	/// <param name="trailerInfo">The trailer information.</param>
	/// <param name="index">index into scans</param>
	/// <returns>The scan event</returns>
	internal static ScanEvent LcqCreateXcalTrailerScanEvent(int filterMassPrecision, ref TrailerStruct trailerInfo, int index)
	{
		ScanEvent scanEvent = new ScanEvent(filterMassPrecision, index);
		ScanEventInfoStruct scanEventInfo = scanEvent.ScanEventInfo;
		scanEventInfo.Polarity = (((trailerInfo.SpectFlags[1] & 1) > 0) ? ((byte)1) : ((byte)0));
		scanEventInfo.ScanDataType = (((trailerInfo.SpectFlags[0] & 0x40) > 0) ? ((byte)1) : ((byte)0));
		scanEventInfo.SourceFragmentation = BoolToOnOffByte((double)Math.Abs(trailerInfo.SourceCidEnergy) > 0.001);
		scanEventInfo.TurboScan = BoolToOnOffByte(trailerInfo.ScanRate == 3);
		if (trailerInfo.ScanMode >= 10 && trailerInfo.ScanMode <= 19)
		{
			scanEventInfo.DependentData = 1;
		}
		else if (trailerInfo.ScanMode <= 9)
		{
			scanEventInfo.DependentData = 0;
		}
		switch ((OldLcqEnums.ScanMode)trailerInfo.ScanMode)
		{
		case OldLcqEnums.ScanMode.FullScan:
		case OldLcqEnums.ScanMode.MstoMsn:
		case OldLcqEnums.ScanMode.MsntoMsn:
		case OldLcqEnums.ScanMode.ZMstoMsn:
		case OldLcqEnums.ScanMode.ZMsntoMsn:
			scanEventInfo.ScanType = 0;
			break;
		case OldLcqEnums.ScanMode.ZoomScan:
		case OldLcqEnums.ScanMode.MstozMs:
		case OldLcqEnums.ScanMode.MsntozMsn:
			scanEventInfo.ScanType = 1;
			break;
		case OldLcqEnums.ScanMode.SimSrmCrm:
			if (trailerInfo.Msn == 1)
			{
				scanEventInfo.ScanType = 2;
			}
			else if (trailerInfo.Msn == 2)
			{
				scanEventInfo.ScanType = 3;
			}
			else if (trailerInfo.Msn > 2)
			{
				scanEventInfo.ScanType = 4;
			}
			break;
		}
		if ((trailerInfo.SpectFlags[5] & 2) > 0)
		{
			scanEventInfo.MSOrder = -1;
		}
		else if ((trailerInfo.SpectFlags[5] & 0x20) > 0)
		{
			scanEventInfo.MSOrder = -3;
		}
		else if ((trailerInfo.SpectFlags[5] & 0x40) > 0)
		{
			scanEventInfo.MSOrder = -2;
		}
		else
		{
			scanEventInfo.MSOrder = (sbyte)trailerInfo.Msn;
		}
		scanEventInfo.IsValid = 1;
		int num = trailerInfo.Msn - 1;
		Reaction[] array = new Reaction[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = new Reaction(trailerInfo.SetMass[i], trailerInfo.NotchWidth[i], trailerInfo.CollisEnergy[i]);
		}
		scanEvent.Reactions = array;
		scanEvent.MassRanges = new MassRangeStruct[1]
		{
			new MassRangeStruct(trailerInfo.LowMassScan, trailerInfo.HighMassScan)
		};
		scanEvent.ScanEventInfo = scanEventInfo;
		scanEvent.CalculateHash();
		return scanEvent;
	}

	/// <summary>
	/// Convert boolean value to "on off" and cast to byte.
	/// </summary>
	/// <param name="value">
	/// The value.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Byte" />.
	/// </returns>
	private static byte BoolToOnOffByte(bool value)
	{
		return (!value) ? ((byte)1) : ((byte)0);
	}

	/// <summary>
	/// Converts the LCQ scan event to Xcalibur scan event.
	/// To construct an Xcalibur scan event from a given LCQ scan event
	/// </summary>
	/// <param name="lcqScanEvent">The LCQ scan event.</param>
	/// <returns>The converted event</returns>
	internal static ScanEvent LcqCreateXcalScanEvent(ref MsScanEvent lcqScanEvent)
	{
		bool flag = true;
		ScanEvent scanEvent = new ScanEvent();
		ScanEventInfoStruct scanEventInfo = scanEvent.ScanEventInfo;
		MsScanEventStruct msScanEventStructInfo = lcqScanEvent.MsScanEventStructInfo;
		scanEventInfo.Polarity = (byte)lcqScanEvent.MsScanEventStructInfo.Polarity;
		int num = 1;
		switch (msScanEventStructInfo.ScanMode)
		{
		case OldLcqEnums.MsScanMode.MsMs:
			num = 2;
			break;
		case OldLcqEnums.MsScanMode.MSn:
			num = lcqScanEvent.ReactionsInfo.Length + 1;
			break;
		}
		if (num > 100)
		{
			flag = false;
		}
		else
		{
			scanEventInfo.MSOrder = (sbyte)num;
		}
		switch (msScanEventStructInfo.ScanType)
		{
		case OldLcqEnums.MsScanType.ScanTypeFull:
			scanEventInfo.ScanType = 0;
			break;
		case OldLcqEnums.MsScanType.ScanTypeSim:
			scanEventInfo.ScanType = 2;
			break;
		case OldLcqEnums.MsScanType.ScanTypeZoom:
			scanEventInfo.ScanType = 1;
			break;
		case OldLcqEnums.MsScanType.ScanTypeSrm:
			scanEventInfo.ScanType = (byte)((msScanEventStructInfo.ScanMode == OldLcqEnums.MsScanMode.MsMs) ? 3 : 4);
			break;
		default:
			flag = false;
			break;
		}
		scanEventInfo.SourceFragmentation = BoolToOnOffByte(msScanEventStructInfo.UseSourceCID);
		scanEventInfo.TurboScan = BoolToOnOffByte(msScanEventStructInfo.TurboScanMode);
		scanEventInfo.DependentData = ((msScanEventStructInfo.DepDataFlag > 0) ? ((byte)1) : ((byte)0));
		int num2 = lcqScanEvent.ReactionsInfo.Length;
		Reaction[] array = new Reaction[num2];
		for (int i = 0; i < num2; i++)
		{
			array[i] = new Reaction(lcqScanEvent.ReactionsInfo[i]);
		}
		num2 = lcqScanEvent.MassRangesInfo.Length;
		MassRangeStruct[] array2 = new MassRangeStruct[num2];
		for (int j = 0; j < num2; j++)
		{
			array2[j] = new MassRangeStruct(lcqScanEvent.MassRangesInfo[j].LowMass, lcqScanEvent.MassRangesInfo[j].HighMass);
		}
		scanEventInfo.IsValid = (flag ? ((byte)1) : ((byte)0));
		scanEvent.MassRanges = array2;
		scanEvent.Reactions = array;
		scanEvent.ScanEventInfo = scanEventInfo;
		scanEvent.CalculateHash();
		return scanEvent;
	}

	/// <summary>
	/// LCQs the index of the write xcalibur scan.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="trailerInfo">The trailer information.</param>
	/// <param name="i">The i.</param>
	/// <param name="hasExpMethod">if set to <c>true</c> [has experiment method].</param>
	/// <param name="offset">The offset.</param>
	internal static void LcqWriteXcalScanIndex(IMemMapWriter writer, ref TrailerStruct trailerInfo, int i, bool hasExpMethod, long offset)
	{
		ScanIndexStruct1 data = new ScanIndexStruct1
		{
			DataOffset32Bit = trailerInfo.DataPos,
			TrailerOffset = i,
			ScanNumber = i + 1,
			NumberPackets = trailerInfo.NumberPackets,
			StartTime = MsToMin(trailerInfo.StartTime),
			TIC = trailerInfo.IntegIntensity,
			BasePeakIntensity = trailerInfo.PeakIntensity,
			BasePeakMass = trailerInfo.PeakMass,
			LowMass = trailerInfo.LowMassScan,
			HighMass = trailerInfo.HighMassScan,
			ScanTypeIndex = (hasExpMethod ? Utilities.MakeLong((short)(trailerInfo.ScanEvent - 1), (short)(trailerInfo.ScanSegment - 1)) : (-1))
		};
		if ((trailerInfo.SpectFlags[0] & 0x10) > 1)
		{
			data.PacketType = 1;
		}
		else if ((trailerInfo.SpectFlags[0] & 0x20) > 1)
		{
			data.PacketType = 2;
		}
		else if ((trailerInfo.SpectFlags[0] & 0x40) > 1)
		{
			data.PacketType = 0;
		}
		writer.WriteStruct(offset, data);
	}

	/// <summary>
	/// LCQs the index of the write xcalibur UV scan.
	/// </summary>
	/// <param name="writer">
	/// The writer.
	/// </param>
	/// <param name="trailerInfo">
	/// The trailer information.
	/// </param>
	/// <param name="msAnalogChannelsUsedCount">
	/// The MS analog channels used count.
	/// </param>
	/// <param name="i">
	/// The i.
	/// </param>
	/// <param name="offset">
	/// The offset.
	/// </param>
	internal static void LcqWriteXcalUvScanIndex(IMemMapWriter writer, ref TrailerStruct trailerInfo, int msAnalogChannelsUsedCount, int i, long offset)
	{
		UvScanIndexStructOld data = new UvScanIndexStructOld
		{
			DataOffset32Bit = (uint)(i * msAnalogChannelsUsedCount * 8),
			ScanNumber = i + 1,
			PacketType = 13,
			NumberPackets = 1,
			NumberOfChannels = msAnalogChannelsUsedCount,
			StartTime = MsToMin(trailerInfo.StartTime)
		};
		writer.WriteStruct(offset, data);
	}

	/// <summary>
	/// write xcalibur UV channel data.
	/// </summary>
	/// <param name="writer">
	/// The writer.
	/// </param>
	/// <param name="uvAnalogInput">
	/// The UV analog input.
	/// </param>
	/// <param name="msAnalogChannelsUsed">
	/// The MS analog channels used.
	/// </param>
	/// <param name="offset">
	/// The offset.
	/// </param>
	internal static void LcqWriteXcalUvChannelData(IMemMapWriter writer, float[] uvAnalogInput, byte msAnalogChannelsUsed, long offset)
	{
		long startPos = offset;
		if ((msAnalogChannelsUsed & 1) > 0)
		{
			writer.WriteDouble(Convert.ToDouble(uvAnalogInput[0]), ref startPos);
		}
		if ((msAnalogChannelsUsed & 2) > 0)
		{
			writer.WriteDouble(Convert.ToDouble(uvAnalogInput[1]), ref startPos);
		}
		if ((msAnalogChannelsUsed & 4) > 0)
		{
			writer.WriteDouble(Convert.ToDouble(uvAnalogInput[2]), ref startPos);
		}
		if ((msAnalogChannelsUsed & 8) > 0)
		{
			writer.WriteDouble(Convert.ToDouble(uvAnalogInput[3]), ref startPos);
		}
	}

	/// <summary>
	/// Write the xcalibur trailer extra.
	/// </summary>
	/// <param name="writer">Where the data is written</param>
	/// <param name="trailerInfo">The trailer information.</param>
	/// <param name="offset">Offset into memory map</param>
	internal static void LcqWriteXcalTrailerExtra(IMemMapWriter writer, ref TrailerStruct trailerInfo, long offset)
	{
		LcqTrailerExtraStruct data = new LcqTrailerExtraStruct
		{
			MicroScanCount = (short)trailerInfo.UScanCount,
			IonInjectionTime = trailerInfo.IonTime,
			ScanSegment = trailerInfo.ScanSegment,
			ScanEvent = trailerInfo.ScanEvent,
			ElapsedScanTime = trailerInfo.ElapsedTime,
			APISourceCIDEnergy = trailerInfo.SourceCidEnergy,
			AverageScanByInst = (((trailerInfo.SpectFlags[6] & 0x80) > 0) ? ((byte)1) : ((byte)0)),
			BackGdSubtractedByInst = (((trailerInfo.SpectFlags[6] & 1) > 0) ? ((byte)1) : ((byte)0)),
			ChargeState = trailerInfo.ChargeState
		};
		if ((trailerInfo.SpectFlags[0] & 0x10) > 0 || (trailerInfo.SpectFlags[0] & 0x20) > 0)
		{
			if ((trailerInfo.SpectFlags[0] & 0x10) != 0 && (trailerInfo.SpectFlags[0] & 0x20) == 0)
			{
				data.Resolution = Encoding.ASCII.GetBytes("Low");
			}
			else if ((trailerInfo.SpectFlags[0] & 0x10) == 0 && (trailerInfo.SpectFlags[0] & 0x20) != 0)
			{
				data.Resolution = Encoding.ASCII.GetBytes("High");
			}
		}
		else
		{
			data.Resolution = Encoding.ASCII.GetBytes("N/A");
		}
		Array.Resize(ref data.Resolution, 6);
		writer.WriteStruct(offset, data);
	}

	/// <summary>
	/// LCQs the write xcalibur tune data.
	/// </summary>
	/// <param name="writer">
	/// The writer.
	/// </param>
	/// <param name="tuneDatastruct">
	/// The tune data struct.
	/// </param>
	/// <param name="offset">
	/// The offset.
	/// </param>
	internal static void LcqWriteXcalTuneData(IMemMapWriter writer, ref TuneDataStruct tuneDatastruct, long offset)
	{
		LcqgTuneDataStruct data = new LcqgTuneDataStruct
		{
			CapillaryTemp = tuneDatastruct.CapTemperature,
			APCIVaporizerTemp = tuneDatastruct.APCIVapTemp,
			SourceVoltage = tuneDatastruct.SourceHighVoltage,
			SourceCurrent = tuneDatastruct.SourceCurrent,
			SheathGasFlow = tuneDatastruct.SheathGasFlow,
			AuxGasFlow = tuneDatastruct.AuxGasFlow,
			CapillaryVoltage = tuneDatastruct.CapVoltage,
			TubeLensOffset = tuneDatastruct.TubeAdjust,
			OctapoleRFAmplifier = tuneDatastruct.OctapoleRFAmp,
			Octapole1Offset = tuneDatastruct.Octapole1Offset,
			Octapole2Offset = tuneDatastruct.Octapole2Offset,
			InterOctapoleLensVoltage = tuneDatastruct.InterOctapoleLensVoltage,
			TrapDCOffsetVoltage = tuneDatastruct.TrapOffsetVoltage,
			MultiplierVoltage = tuneDatastruct.MultiplierVoltage,
			MaxIonTime = tuneDatastruct.MaxIonTime,
			IonTime = tuneDatastruct.IonTime,
			ZoomMicroScans = (short)tuneDatastruct.UsZoom,
			ZoomAGCTarget = tuneDatastruct.AGCZoom,
			FullMicroScans = (short)tuneDatastruct.UsFull,
			FullAGCTarget = tuneDatastruct.AGCFull,
			SIMMicroScans = (short)tuneDatastruct.UsSIM,
			SIMAGCTarget = tuneDatastruct.AGCSIM,
			MSnMicroScans = (short)tuneDatastruct.UsMSn,
			MSnAGCTarget = tuneDatastruct.AGCMSn,
			DataType = Encoding.Unicode.GetBytes((tuneDatastruct.DataType < 0.5) ? "Profile" : "Centroid")
		};
		Array.Resize(ref data.DataType, 18);
		data.SourceType = EncodeStatusMessage((int)tuneDatastruct.SourceType, OldLcqConstants.SourceTypeStrings, 16);
		switch (tuneDatastruct.Polarity)
		{
		case ScanFilterEnums.PolarityTypes.Negative:
			data.Polarity = Encoding.Unicode.GetBytes("Negative");
			break;
		case ScanFilterEnums.PolarityTypes.Positive:
			data.Polarity = Encoding.Unicode.GetBytes("Positive");
			break;
		default:
			data.Polarity = Encoding.Unicode.GetBytes("Unknown");
			break;
		}
		Array.Resize(ref data.Polarity, 18);
		writer.WriteStruct(offset, data);
	}

	/// <summary>
	/// convert byte to zero or one.
	/// </summary>
	/// <param name="b">
	/// The byte.
	/// </param>
	/// <returns>
	/// either 0 or 1
	/// </returns>
	private static byte ByteToZeroOne(byte b)
	{
		return (b <= 0) ? ((byte)1) : ((byte)0);
	}

	/// <summary>
	/// Write the xcalibur status log.
	/// To generate the trailer extra header for inclusion to the Xcalibur data file.
	/// </summary>
	/// <param name="writer">
	/// The writer.
	/// </param>
	/// <param name="instStatusStructInfo">
	/// The instrument status structure information.
	/// </param>
	/// <param name="incAs">
	/// if set to <c>true</c> include the auto sampler data.
	/// </param>
	/// <param name="incLc">
	/// if set to <c>true</c> include the LC data.
	/// </param>
	/// <param name="blockSize">
	/// Size of the block.
	/// </param>
	/// <param name="offset">
	/// The offset.
	/// </param>
	internal static void LcqWriteXcalStatusLog(IMemMapWriter writer, ref InstStatusStruct instStatusStructInfo, bool incAs, bool incLc, int blockSize, long offset)
	{
		ReadBackStruct readBacks = instStatusStructInfo.ReadBacks;
		DigitalIn digitalIn = new DigitalIn(readBacks.RBDIN1);
		long startPos = offset;
		writer.WriteFloat(instStatusStructInfo.RetentionTime, ref startPos);
		writer.WriteFloat(readBacks.RBKVDAC, ref startPos);
		writer.WriteFloat(readBacks.RBKVCDAC, ref startPos);
		writer.WriteByte(digitalIn.ApcItcFail, ref startPos);
		writer.WriteFloat(readBacks.RBAPCIHeat, ref startPos);
		writer.WriteFloat(readBacks.RBSheathFlow, ref startPos);
		writer.WriteFloat(readBacks.RBAuxFlow, ref startPos);
		writer.WriteByte(digitalIn.CapillaryRtdFail, ref startPos);
		writer.WriteFloat(readBacks.RBCapVolt, ref startPos);
		writer.WriteFloat(readBacks.RBCapHeat, ref startPos);
		writer.WriteByte(digitalIn.Kvcl8, ref startPos);
		writer.WriteByte(ByteToZeroOne(digitalIn.VacuumOk), ref startPos);
		writer.WriteByte(ByteToZeroOne(digitalIn.IonGaugePressureOk), ref startPos);
		writer.WriteByte(ByteToZeroOne(digitalIn.IonGaugeOn), ref startPos);
		writer.WriteFloat(readBacks.RBIonGauge, ref startPos);
		writer.WriteByte(ByteToZeroOne(digitalIn.ConvPressOk), ref startPos);
		writer.WriteFloat(readBacks.RBConvectGauge, ref startPos);
		TpStatusStruct turboPumpStatus = instStatusStructInfo.TurboPumpStatus;
		byte[] value = EncodeStatusMessage(turboPumpStatus.Status, OldLcqConstants.TurboStatusStrings, 14);
		writer.WriteBytes(value, ref startPos);
		writer.WriteFloat(turboPumpStatus.Life, ref startPos);
		writer.WriteFloat(turboPumpStatus.Speed * 1000f, ref startPos);
		writer.WriteFloat(turboPumpStatus.Watts, ref startPos);
		writer.WriteFloat(turboPumpStatus.Temp, ref startPos);
		writer.WriteByte(ByteToZeroOne(digitalIn.OctFreqOn), ref startPos);
		writer.WriteFloat(readBacks.RBOct1Offset, ref startPos);
		writer.WriteFloat(readBacks.RBOct2Offset, ref startPos);
		writer.WriteFloat(readBacks.RBOctLens, ref startPos);
		writer.WriteFloat(readBacks.RBTRapOffset, ref startPos);
		writer.WriteByte(ByteToZeroOne(digitalIn.RefSineOn), ref startPos);
		writer.WriteByte(ByteToZeroOne(digitalIn.SwrFail), ref startPos);
		writer.WriteFloat(readBacks.RBRFDac, ref startPos);
		writer.WriteFloat(readBacks.RBDetectRF, ref startPos);
		writer.WriteFloat(readBacks.RBRFDetectTemp, ref startPos);
		writer.WriteFloat(readBacks.RBRFModule, ref startPos);
		writer.WriteFloat(readBacks.RBRFOut, ref startPos);
		writer.WriteFloat(readBacks.RBRFGenTemp, ref startPos);
		writer.WriteFloat(readBacks.RBMultVolt, ref startPos);
		writer.WriteFloat(readBacks.RBPSP5V, ref startPos);
		writer.WriteFloat(readBacks.RBPSM15V, ref startPos);
		writer.WriteFloat(readBacks.RBPSP15V, ref startPos);
		writer.WriteFloat(readBacks.RBPSP24V, ref startPos);
		writer.WriteFloat(readBacks.RBPSM28V, ref startPos);
		writer.WriteFloat(readBacks.RBPSP28V, ref startPos);
		writer.WriteFloat(readBacks.RBPSP28C, ref startPos);
		writer.WriteFloat(readBacks.RBPSP35V, ref startPos);
		writer.WriteFloat(readBacks.RBPSP36V, ref startPos);
		writer.WriteFloat(readBacks.RBPSM150V, ref startPos);
		writer.WriteFloat(readBacks.RBPSP150V, ref startPos);
		writer.WriteFloat(readBacks.RBPSM215V, ref startPos);
		writer.WriteFloat(readBacks.RBPSP215V, ref startPos);
		writer.WriteFloat(readBacks.RBAmbTemp, ref startPos);
		value = EncodeStatusMessage((int)(instStatusStructInfo.SysStatus & 0xF), OldLcqConstants.SystemStatusStrings, 10);
		writer.WriteBytes(value, ref startPos);
		value = EncodeStatusMessage(instStatusStructInfo.AnalState, OldLcqConstants.AnalysisStatusStrings, 15);
		writer.WriteBytes(value, ref startPos);
		AsStatusStruct autoSamplerStatus = instStatusStructInfo.AutoSamplerStatus;
		value = EncodeStatusMessage(autoSamplerStatus.Status, OldLcqConstants.AsStatusStrings, 21);
		writer.WriteBytes(value, ref startPos);
		if (incAs)
		{
			writer.WriteShort(autoSamplerStatus.VialPos, ref startPos);
			writer.WriteShort(autoSamplerStatus.InjNum, ref startPos);
		}
		LcStatusStruct lcStatus = instStatusStructInfo.LcStatus;
		value = EncodeStatusMessage(lcStatus.Status, OldLcqConstants.LcStatusStrings, 22);
		writer.WriteBytes(value, ref startPos);
		if (incLc)
		{
			writer.WriteFloat(lcStatus.RunTime, ref startPos);
			writer.WriteFloat(lcStatus.FlowRate[0], ref startPos);
			writer.WriteFloat(lcStatus.Pressure[0], ref startPos);
			writer.WriteFloat(lcStatus.Temperature, ref startPos);
			writer.WriteFloat(lcStatus.Composition[0], ref startPos);
			writer.WriteFloat(lcStatus.Composition[1], ref startPos);
			writer.WriteFloat(lcStatus.Composition[2], ref startPos);
			writer.WriteFloat(lcStatus.Composition[3], ref startPos);
		}
		SpStatusStruct syringePumpStatus = instStatusStructInfo.SyringePumpStatus;
		value = EncodeStatusMessage(syringePumpStatus.Status, OldLcqConstants.SyringeStatusStrings, 14);
		writer.WriteBytes(value, ref startPos);
		writer.WriteFloat(syringePumpStatus.FlowRate, ref startPos);
		writer.WriteFloat(syringePumpStatus.CurrVol * 1000f, ref startPos);
		writer.WriteFloat(syringePumpStatus.Diameter, ref startPos);
		writer.WriteByte(digitalIn.UserDin2, ref startPos);
		writer.WriteByte(digitalIn.UserDin1, ref startPos);
		bool flag = digitalIn.DivertInjectBit1 > 0;
		bool flag2 = digitalIn.LoadInject > 0;
		value = ((!flag && flag2) ? Encoding.Unicode.GetBytes("Load") : ((!flag || flag2) ? Encoding.Unicode.GetBytes("Error") : Encoding.Unicode.GetBytes("Inject")));
		Array.Resize(ref value, 14);
		writer.WriteBytes(value, ref startPos);
		value = EncodeStatusMessage(instStatusStructInfo.UvStatus.Status, OldLcqConstants.UvStatusStrings, 14);
		writer.WriteBytes(value, ref startPos);
	}

	/// <summary>
	/// encode a status message.
	/// </summary>
	/// <param name="statusCode">
	/// The status code.
	/// </param>
	/// <param name="statusStrings">
	/// The status strings.
	/// </param>
	/// <param name="maxLength">
	/// The max length.
	/// </param>
	/// <returns>
	/// The message as encoded bytes
	/// </returns>
	private static byte[] EncodeStatusMessage(int statusCode, string[] statusStrings, int maxLength)
	{
		string s = ((statusCode < 0 || statusCode > statusStrings.Length) ? "Unknown" : statusStrings[statusCode]);
		byte[] array = Encoding.Unicode.GetBytes(s);
		Array.Resize(ref array, maxLength * 2);
		return array;
	}
}
