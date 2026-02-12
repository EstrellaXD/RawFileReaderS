using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// The instrument data converter.
/// Converts instrument data from internal interfaces to public format.
/// Note: Created as a static class rather than a wrapper (extending the type)
/// so that the expected type is returned to the caller, supporting serialize etc.
/// </summary>
internal static class InstrumentDataConverter
{
	/// <summary>
	/// Convert the IO format to CommonCore format
	/// </summary>
	/// <param name="instId">The instrument identifier.</param>
	public static InstrumentData CopyFrom(IInstrumentId instId)
	{
		InstrumentData instrumentData = new InstrumentData();
		instrumentData.Name = instId.Name;
		instrumentData.Model = instId.Model;
		instrumentData.SerialNumber = instId.SerialNumber;
		instrumentData.SoftwareVersion = instId.SoftwareVersion;
		instrumentData.HardwareVersion = instId.HardwareVersion;
		instrumentData.Flags = instId.Flags;
		instrumentData.AxisLabelX = instId.AxisLabelX;
		instrumentData.AxisLabelY = instId.AxisLabelY;
		instrumentData.Units = instId.AbsorbanceUnit.ToDataUnits();
		instrumentData.IsValid = instId.IsValid;
		int count = instId.ChannelLabels.Count;
		string[] array = (instrumentData.ChannelLabels = new string[count]);
		string[] array3 = array;
		for (int i = 0; i < count; i++)
		{
			array3[i] = instId.ChannelLabels[i].Value;
		}
		return instrumentData;
	}
}
