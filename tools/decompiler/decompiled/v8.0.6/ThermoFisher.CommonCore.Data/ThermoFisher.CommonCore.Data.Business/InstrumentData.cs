using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Data about an instrument, for example, instrument name
/// </summary>
[Serializable]
public class InstrumentData : ICloneable, IInstrumentDataAccess
{
	/// <summary>
	/// Gets or sets the name of the instrument
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Gets or sets the model of instrument
	/// </summary>
	public string Model { get; set; }

	/// <summary>
	/// Gets or sets the serial number of instrument
	/// </summary>
	public string SerialNumber { get; set; }

	/// <summary>
	/// Gets or sets the software version of instrument
	/// </summary>
	public string SoftwareVersion { get; set; }

	/// <summary>
	/// Gets or sets the hardware version of instrument
	/// </summary>
	public string HardwareVersion { get; set; }

	/// <summary>
	/// Gets or sets the Names for the channels, for UV or analog data: 
	/// </summary>
	public string[] ChannelLabels { get; set; }

	/// <summary>
	/// Gets or sets the units of the Signal, for UV or analog
	/// </summary>
	public DataUnits Units { get; set; }

	/// <summary>
	/// Gets or sets additional information about this instrument.
	/// </summary>
	public string Flags { get; set; }

	/// <summary>
	/// Gets or sets Device suggested label of X axis
	/// </summary>
	public string AxisLabelX { get; set; }

	/// <summary>
	/// Gets or sets Device suggested label of Y axis (name for units of data, such as "°C")
	/// </summary>
	public string AxisLabelY { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the instrument is valid.
	/// </summary>
	public bool IsValid { get; set; }

	/// <summary>
	/// Gets a value indicating whether this file has accurate mass precursors
	/// </summary>
	public bool HasAccurateMassPrecursors => IsTsqQuantumFile();

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.InstrumentData" /> class. 
	/// Default constructor
	/// </summary>
	public InstrumentData()
	{
		Flags = string.Empty;
		AxisLabelX = string.Empty;
		AxisLabelY = string.Empty;
		ChannelLabels = new string[0];
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.InstrumentData" /> class. 
	/// Construct by copying fields from interface
	/// </summary>
	/// <param name="from">
	/// Interface to copy from
	/// </param>
	public InstrumentData(IInstrumentDataAccess from)
		: this()
	{
		if (from == null)
		{
			return;
		}
		Name = from.Name;
		Model = from.Model;
		SerialNumber = from.SerialNumber;
		SoftwareVersion = from.SoftwareVersion;
		HardwareVersion = from.HardwareVersion;
		Flags = from.Flags;
		AxisLabelX = from.AxisLabelX;
		AxisLabelY = from.AxisLabelY;
		Units = from.Units;
		IsValid = from.IsValid;
		if (from.ChannelLabels != null)
		{
			ChannelLabels = new string[from.ChannelLabels.Length];
			for (int i = 0; i < ChannelLabels.Length; i++)
			{
				ChannelLabels[i] = from.ChannelLabels[i];
			}
		}
	}

	/// <summary>
	/// Creates a new object that is a copy of the current instance.
	/// </summary>
	/// <returns>
	/// A new object that is a copy of this instance.
	/// </returns>
	/// <filterpriority>2</filterpriority>
	public object Clone()
	{
		InstrumentData instrumentData = (InstrumentData)MemberwiseClone();
		if (ChannelLabels != null)
		{
			instrumentData.ChannelLabels = new string[ChannelLabels.Length];
			for (int i = 0; i < ChannelLabels.Length; i++)
			{
				instrumentData.ChannelLabels[i] = ChannelLabels[i];
			}
		}
		return instrumentData;
	}

	/// <summary>
	/// Test if this is a TSQ quantum series file.
	/// Such files may have more accurate precursor mass selection.
	/// </summary>
	/// <returns>True if this is a raw file from a TSQ Quantum</returns>
	public bool IsTsqQuantumFile()
	{
		if (Flags.Contains("high_res_precursors"))
		{
			return true;
		}
		if (Name.Contains("Quantum"))
		{
			return true;
		}
		if (Name.Contains("TSQ") && Model != "Standard")
		{
			return true;
		}
		if (Name.Contains("Endura"))
		{
			return true;
		}
		return false;
	}
}
