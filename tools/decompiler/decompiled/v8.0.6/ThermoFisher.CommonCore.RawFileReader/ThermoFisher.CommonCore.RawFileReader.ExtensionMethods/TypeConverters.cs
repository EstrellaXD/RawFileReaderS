using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices.ComTypes;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.DataModel;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

namespace ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;

/// <summary>
/// Provides static methods to convert enumeration between the legacy FileIO and CommonCore.data
/// </summary>
internal static class TypeConverters
{
	private static readonly PeakOptions[] FlagConvert = MakeFlagsTable();

	/// <summary>
	/// Common Core Devices from VirtualDeviceTypes.
	/// </summary>
	/// <param name="value">The VirtualDeviceTypes.</param>
	/// <returns>The converted type</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException"><c>type</c> is out of range.</exception>
	public static Device ToDevice(this VirtualDeviceTypes value)
	{
		return value switch
		{
			VirtualDeviceTypes.AnalogDevice => Device.Analog, 
			VirtualDeviceTypes.MsDevice => Device.MS, 
			VirtualDeviceTypes.MsAnalogDevice => Device.MSAnalog, 
			VirtualDeviceTypes.PdaDevice => Device.Pda, 
			VirtualDeviceTypes.UvDevice => Device.UV, 
			_ => Device.Other, 
		};
	}

	/// <summary>
	/// Convert common core Device type to the virtual device types.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The converted value</returns>
	public static VirtualDeviceTypes ToVirtualDeviceType(this Device value)
	{
		return value switch
		{
			Device.Analog => VirtualDeviceTypes.AnalogDevice, 
			Device.MS => VirtualDeviceTypes.MsDevice, 
			Device.MSAnalog => VirtualDeviceTypes.MsAnalogDevice, 
			Device.Pda => VirtualDeviceTypes.PdaDevice, 
			Device.UV => VirtualDeviceTypes.UvDevice, 
			_ => VirtualDeviceTypes.StatusDevice, 
		};
	}

	/// <summary>
	/// To the scan filter.
	/// </summary>
	/// <param name="values">The values.</param>
	/// <returns>List of scan filer</returns>
	public static ReadOnlyCollection<IScanFilter> ToScanFilter(this IEnumerable<IFilterScanEvent> values)
	{
		List<IScanFilter> list = new List<IScanFilter>();
		foreach (IFilterScanEvent value in values)
		{
			list.Add(new WrappedScanFilter(value));
		}
		return new ReadOnlyCollection<IScanFilter>(list);
	}

	/// <summary>
	/// Convert an event accurate mass.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The converted value</returns>
	public static EventAccurateMass ToEventAccurateMass(this ScanFilterEnums.AccurateMassTypes value)
	{
		return value switch
		{
			ScanFilterEnums.AccurateMassTypes.Internal => EventAccurateMass.Internal, 
			ScanFilterEnums.AccurateMassTypes.External => EventAccurateMass.External, 
			_ => EventAccurateMass.Off, 
		};
	}

	/// <summary>
	/// Convert an accurate mass to event value.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The converted value</returns>
	public static ScanFilterEnums.ScanEventAccurateMassTypes ToAccurateMass(this EventAccurateMass value)
	{
		return value switch
		{
			EventAccurateMass.Internal => ScanFilterEnums.ScanEventAccurateMassTypes.Internal, 
			EventAccurateMass.External => ScanFilterEnums.ScanEventAccurateMassTypes.External, 
			_ => ScanFilterEnums.ScanEventAccurateMassTypes.Off, 
		};
	}

	/// <summary>
	/// Map flag bit to peak options, with a static table
	/// </summary>
	/// <returns>
	/// The table of options for each flag
	/// </returns>
	private static PeakOptions[] MakeFlagsTable()
	{
		PeakOptions[] array = new PeakOptions[64];
		for (int i = 0; i < 64; i++)
		{
			PeakOptions peakOptions = PeakOptions.None;
			if ((i & 1) != 0)
			{
				peakOptions |= PeakOptions.Fragmented;
			}
			if ((i & 2) != 0)
			{
				peakOptions |= PeakOptions.Merged;
			}
			if ((i & 0x20) != 0)
			{
				peakOptions |= PeakOptions.Saturated;
			}
			if ((i & 8) != 0)
			{
				peakOptions |= PeakOptions.Exception;
			}
			if ((i & 4) != 0)
			{
				peakOptions |= PeakOptions.Reference;
			}
			if ((i & 0x10) != 0)
			{
				peakOptions |= PeakOptions.Modified;
			}
			array[i] = peakOptions;
		}
		return array;
	}

	/// <summary>
	/// Convert flags (such as "reference peak) from FT format to "PeakOptions" format
	/// </summary>
	/// <param name="value">The peak with flags to be converted.</param>
	/// <returns>The converted flags</returns>
	/// <exception cref="T:System.ArgumentNullException">Null label peak type argument.</exception>
	public static PeakOptions ToPeakOptions(this ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LabelPeak value)
	{
		return FlagConvert[value.Flags & 0x3F];
	}

	/// <summary>
	/// Convert to 3 state variable (on, off any).
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The converted value</returns>
	public static TriState ToTriState(this ScanFilterEnums.IsDependent value)
	{
		return value switch
		{
			ScanFilterEnums.IsDependent.No => TriState.Off, 
			ScanFilterEnums.IsDependent.Yes => TriState.On, 
			_ => TriState.Any, 
		};
	}

	/// <summary>
	///  Convert to 3 state variable (on, off any).
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>Converted flag.</returns>
	public static TriState ToTriState(this ScanFilterEnums.OnAnyOffTypes value)
	{
		return value switch
		{
			ScanFilterEnums.OnAnyOffTypes.Off => TriState.Off, 
			ScanFilterEnums.OnAnyOffTypes.On => TriState.On, 
			_ => TriState.Any, 
		};
	}

	/// <summary>
	///  Convert to 3 state variable (on, off any).
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The converted value</returns>
	public static TriState ToTriState(this ScanFilterEnums.OffOnTypes value)
	{
		return value switch
		{
			ScanFilterEnums.OffOnTypes.Off => TriState.Off, 
			ScanFilterEnums.OffOnTypes.On => TriState.On, 
			_ => TriState.Any, 
		};
	}

	/// <summary>
	///  Convert from Tri-state to "Off On Any"to 3.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The converted value</returns>
	public static ScanFilterEnums.OffOnTypes ToOffOnType(this TriState value)
	{
		return value switch
		{
			TriState.Off => ScanFilterEnums.OffOnTypes.Off, 
			TriState.On => ScanFilterEnums.OffOnTypes.On, 
			_ => ScanFilterEnums.OffOnTypes.Any, 
		};
	}

	/// <summary>
	///  Convert from Tri-state to "On Any off".
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The converted value</returns>
	public static ScanFilterEnums.OnAnyOffTypes ToOnAnyOffType(this TriState value)
	{
		return value switch
		{
			TriState.Off => ScanFilterEnums.OnAnyOffTypes.Off, 
			TriState.On => ScanFilterEnums.OnAnyOffTypes.On, 
			_ => ScanFilterEnums.OnAnyOffTypes.Any, 
		};
	}

	/// <summary>
	/// To the type of the detector.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The converted value</returns>
	public static DetectorType ToDetectorType(this ScanFilterEnums.DetectorType value)
	{
		return value switch
		{
			ScanFilterEnums.DetectorType.IsInValid => DetectorType.NotValid, 
			ScanFilterEnums.DetectorType.IsValid => DetectorType.Valid, 
			_ => DetectorType.Any, 
		};
	}

	/// <summary>
	/// To the type of the detector.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The converted value</returns>
	public static ScanFilterEnums.DetectorType ToDetectorType(this DetectorType value)
	{
		return value switch
		{
			DetectorType.NotValid => ScanFilterEnums.DetectorType.IsInValid, 
			DetectorType.Valid => ScanFilterEnums.DetectorType.IsValid, 
			_ => ScanFilterEnums.DetectorType.Any, 
		};
	}

	/// <summary>
	/// To the type of the scan mode.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>Converted flag.</returns>
	/// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">Invalid scan data types type.</exception>
	public static ScanModeType ToScanModeType(this ScanFilterEnums.ScanTypes value)
	{
		return value switch
		{
			ScanFilterEnums.ScanTypes.Any => ScanModeType.Any, 
			ScanFilterEnums.ScanTypes.CRM => ScanModeType.Crm, 
			ScanFilterEnums.ScanTypes.Full => ScanModeType.Full, 
			ScanFilterEnums.ScanTypes.Q1MS => ScanModeType.Q1Ms, 
			ScanFilterEnums.ScanTypes.Q3MS => ScanModeType.Q3Ms, 
			ScanFilterEnums.ScanTypes.SIM => ScanModeType.Sim, 
			ScanFilterEnums.ScanTypes.SRM => ScanModeType.Srm, 
			ScanFilterEnums.ScanTypes.Zoom => ScanModeType.Zoom, 
			_ => ScanModeType.Any, 
		};
	}

	/// <summary>
	/// From mode to the type of the scan.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>Converted flag.</returns>
	public static ScanFilterEnums.ScanTypes ToScanType(this ScanModeType value)
	{
		return value switch
		{
			ScanModeType.Any => ScanFilterEnums.ScanTypes.Any, 
			ScanModeType.Crm => ScanFilterEnums.ScanTypes.CRM, 
			ScanModeType.Full => ScanFilterEnums.ScanTypes.Full, 
			ScanModeType.Q1Ms => ScanFilterEnums.ScanTypes.Q1MS, 
			ScanModeType.Q3Ms => ScanFilterEnums.ScanTypes.Q3MS, 
			ScanModeType.Sim => ScanFilterEnums.ScanTypes.SIM, 
			ScanModeType.Srm => ScanFilterEnums.ScanTypes.SRM, 
			ScanModeType.Zoom => ScanFilterEnums.ScanTypes.Zoom, 
			_ => ScanFilterEnums.ScanTypes.Any, 
		};
	}

	/// <summary>
	/// Convert type of "source fragmentation information valid".
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The converted value</returns>
	public static SourceFragmentationInfoValidType ToSourceFragmentationInfoValidType(this ScanFilterEnums.SourceCIDValidTypes value)
	{
		return value switch
		{
			ScanFilterEnums.SourceCIDValidTypes.SourceCIDEnergyValid => SourceFragmentationInfoValidType.Energy, 
			ScanFilterEnums.SourceCIDValidTypes.AcceptAnySourceCIDEnergy => SourceFragmentationInfoValidType.Any, 
			ScanFilterEnums.SourceCIDValidTypes.CompensationVoltageEnergyValid => SourceFragmentationInfoValidType.Energy, 
			_ => SourceFragmentationInfoValidType.Any, 
		};
	}

	/// <summary>
	/// convert integrator event from PMD format to Data format
	/// </summary>
	/// <param name="old">
	/// The old event.
	/// </param>
	/// <returns>
	/// The converted <see cref="T:ThermoFisher.CommonCore.Data.Business.IntegratorEvent" />.
	/// </returns>
	public static IntegratorEvent ConvertIntegratorEvent(QualitativePeakDetection.FalconEvent old)
	{
		IntegratorEvent obj = new IntegratorEvent
		{
			Kind = (EventKind)old.Kind,
			Time = old.Time,
			Value1 = old.Value1,
			Value2 = old.Value2
		};
		int num = old.OPCode;
		if (num >= 21)
		{
			num -= 20;
		}
		obj.Opcode = (EventCode)num;
		return obj;
	}

	/// <summary>
	/// Convert from C# Reader format to common core format of data units.
	/// Conversion is accurate for UV units, and approximate for others.
	/// in particular "units mUnits and uUnits" are considered "Volts".
	/// "Other" and "Unknown" convert to "None".
	/// </summary>
	/// <param name="value">The value from C# Reader.</param>
	/// <returns>Converted unit</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if the units are not a valid value</exception>
	public static DataUnits ToDataUnits(this AbsorbanceUnits value)
	{
		switch (value)
		{
		case AbsorbanceUnits.Unknown:
		case AbsorbanceUnits.OtherAu:
		case AbsorbanceUnits.None:
			return DataUnits.None;
		case AbsorbanceUnits.OtherUnits:
			return DataUnits.Volts;
		case AbsorbanceUnits.MicroAu:
			return DataUnits.MicroAbsorbanceUnits;
		case AbsorbanceUnits.MilliAu:
			return DataUnits.MilliAbsorbanceUnits;
		case AbsorbanceUnits.Au:
			return DataUnits.AbsorbanceUnits;
		case AbsorbanceUnits.MicroUnits:
			return DataUnits.MicroVolts;
		case AbsorbanceUnits.MilliUnits:
			return DataUnits.MilliVolts;
		default:
			throw new ArgumentOutOfRangeException("value");
		}
	}

	/// <summary>
	/// Convert from CommonCore format to IO format of data units.
	/// Conversion is accurate for UV units, and approximate for others.
	/// in particular "units mUnits and uUnits" are considered "Volts".
	/// "Other" and "Unknown" convert to "None".
	/// </summary>
	/// <param name="value">The value from CommonCore format.</param>
	/// <returns>Converted unit</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">Thrown if the units are not a valid value</exception>
	public static AbsorbanceUnits ToAbsorbanceUnits(this DataUnits value)
	{
		return value switch
		{
			DataUnits.None => AbsorbanceUnits.None, 
			DataUnits.Volts => AbsorbanceUnits.OtherUnits, 
			DataUnits.MicroAbsorbanceUnits => AbsorbanceUnits.MicroAu, 
			DataUnits.MilliAbsorbanceUnits => AbsorbanceUnits.MilliAu, 
			DataUnits.AbsorbanceUnits => AbsorbanceUnits.Au, 
			DataUnits.MicroVolts => AbsorbanceUnits.MicroUnits, 
			DataUnits.MilliVolts => AbsorbanceUnits.MilliUnits, 
			_ => throw new ArgumentOutOfRangeException("value"), 
		};
	}

	/// <summary>
	/// Converts the date time to file time.
	/// </summary>
	/// <param name="dateTime">
	/// The date time.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Runtime.InteropServices.ComTypes.FILETIME" />.
	/// </returns>
	public static FILETIME DateTimeToFileTime(DateTime dateTime)
	{
		FILETIME result = default(FILETIME);
		try
		{
			DateTime dateTime2 = dateTime.ToLocalTime();
			TimeSpan utcOffset = TimeZoneInfo.Local.GetUtcOffset(dateTime2);
			long num = (dateTime + utcOffset).ToFileTime();
			result.dwLowDateTime = (int)(num & 0xFFFFFFFFu);
			result.dwHighDateTime = (int)(num >> 32);
		}
		catch (ArgumentOutOfRangeException)
		{
		}
		return result;
	}
}
