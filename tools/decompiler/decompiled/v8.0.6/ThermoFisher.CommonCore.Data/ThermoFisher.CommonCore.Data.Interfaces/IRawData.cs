using System;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Interface to supply data from a raw data stream.
/// This is intended to permit the data to be read from any file format.
/// </summary>
public interface IRawData : IDetectorReaderBase, IRawDataProperties, IDisposable
{
	/// <summary>
	/// Gets the current instrument's run header.
	/// The run header records information related to all data acquired by
	/// this instrument (such as the highest scan number "LastSpectrum")
	/// </summary>
	IRunHeaderAccess RunHeader { get; }

	/// <summary>
	/// Gets the number of instruments which have saved method data, within the
	/// instrument method embedded in this file.
	/// </summary>
	int InstrumentMethodsCount { get; }

	/// <summary>
	/// Gets or sets a value indicating whether reference and exception peaks
	/// should be returned (by default they are not).
	/// Reference and exception peaks are internal mass calibration data within a scan.
	/// </summary>
	bool IncludeReferenceAndExceptionData { get; set; }

	/// <summary>
	/// Gets the instrument as last set by a call to <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawData.SelectInstrument(ThermoFisher.CommonCore.Data.Business.Device,System.Int32)" />.
	/// If this has never been set, returns null.
	/// </summary>
	InstrumentSelection SelectedInstrument { get; }

	/// <summary>
	/// Gets the number of instruments (data streams) in this file.
	/// For example, a file with an MS detector and a 4 channel UV may have an instrument
	/// count of 2. To find out how many instruments there are of a particular category
	/// call <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawData.GetInstrumentCountOfType(ThermoFisher.CommonCore.Data.Business.Device)" /> with the desired instrument type.
	/// Instrument count related methods could, for example, be used to format
	/// a list of instruments available to select in the UI of an application.
	/// To start reading data from a particular instrument, call <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawData.SelectInstrument(ThermoFisher.CommonCore.Data.Business.Device,System.Int32)" />.
	/// </summary>
	/// <seealso cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawData.GetInstrumentType(System.Int32)" />
	int InstrumentCount { get; }

	/// <summary>
	/// Gets a text form of an instrument method, for a specific instrument.
	/// </summary>
	/// <param name="index">
	/// The index into the count of available instruments.
	/// The property "InstrumentMethodsCount",
	/// determines the valid range of "index" for this call.
	/// </param>
	/// <returns>
	/// A text version of the method. Some instruments do not log this data.
	/// Always test "string.IsNullOrEmpty" on the returned value.
	/// </returns>
	string GetInstrumentMethod(int index);

	/// <summary>
	/// Choose the data stream from the data source.
	/// This must be called before reading data from a detector (such as chromatograms or scans).
	/// You may call <see cref="M:ThermoFisher.CommonCore.Data.Interfaces.IRawData.GetInstrumentCountOfType(ThermoFisher.CommonCore.Data.Business.Device)" /> to determine if there is at
	/// least one instrument of the required device type.
	/// </summary>
	/// <param name="instrumentType">
	/// Type of instrument
	/// </param>
	/// <param name="instrumentIndex">
	/// Stream number (1 based)
	/// </param>
	void SelectInstrument(Device instrumentType, int instrumentIndex);

	/// <summary>
	/// Get the number of instruments (data streams) of a certain classification.
	/// For example: the number of UV devices which logged data into this file.
	/// </summary>
	/// <param name="type">
	/// The device type to count
	/// </param>
	/// <returns>
	/// The number of devices of this type
	/// </returns>
	int GetInstrumentCountOfType(Device type);

	/// <summary>
	/// Gets the device type for an instrument data stream
	/// </summary>
	/// <param name="index">
	/// The data stream
	/// </param>
	/// <returns>
	/// The device at type the index
	/// </returns>
	Device GetInstrumentType(int index);

	/// <summary>
	/// Gets names of all instruments, which have a method stored in the raw file's copy of the instrument method file.
	/// These names are "Device internal names" which map to storage names within
	/// an instrument method, and other instrument data (such as registry keys).
	/// Use "GetAllInstrumentFriendlyNamesFromInstrumentMethod" (in IRawDataPlus) to get display names
	/// for instruments.
	/// </summary>
	/// <returns>
	/// The instrument names.
	/// </returns>
	string[] GetAllInstrumentNamesFromInstrumentMethod();

	/// <summary>
	/// Re-read the current file, to get the latest data.
	/// Only meaningful when the raw file is InAcquisition when opened,
	/// or on the last refresh call. After acquisition is completed
	/// further calls have no effect.
	/// <para>
	/// For example, the value of "LastSpectrum" in the Run Header of a detector may be 60 after a refresh call.
	/// Even after new scans become acquired, this value will remain at 60, from the application's view of the data,
	/// until RefreshViewOfFile is called again. If GetRunHeader is called again, the number of scans may now be
	/// a larger value, such as 100</para>
	/// </summary>
	/// <returns>
	/// true, if refresh was OK.
	/// </returns>
	bool RefreshViewOfFile();
}
