using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Class containing business logic specific to loading an instrument method file contents
/// </summary>
internal class InstrumentMethodFileLoader : IInstrumentMethodFileAccess
{
	/// <summary>
	/// my errors.
	/// </summary>
	private class MyErrors : IFileError
	{
		/// <summary>
		/// Gets or sets a value indicating whether this file has detected an error.
		/// If this is false: Other error properties in this interface have no meaning.
		/// </summary>
		public bool HasError { get; set; }

		/// <summary>
		/// Gets a value indicating whether this file has detected a warning.
		/// If this is false: Other warning properties in this interface have no meaning.
		/// </summary>
		public bool HasWarning => false;

		/// <summary>
		/// Gets or sets the error code number.
		/// Typically this is a windows system error number.
		/// </summary>
		public int ErrorCode { get; internal set; }

		/// <summary>
		/// Gets or sets the error message.
		/// </summary>
		public string ErrorMessage { get; internal set; }

		/// <summary>
		/// Gets the warning message.
		/// </summary>
		public string WarningMessage { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.InstrumentMethodFileLoader.MyErrors" /> class.
		/// </summary>
		public MyErrors()
		{
			ErrorMessage = string.Empty;
			WarningMessage = string.Empty;
		}
	}

	/// <summary>
	/// Gets the data for of all devices in this method.
	/// Keys are the registered device names.
	/// A method contains only the "registered device name"
	/// which may not be the same as the "device display name" (product name).
	/// Instrument methods do not contain device product names.
	/// </summary>
	public ReadOnlyDictionary<string, IInstrumentMethodDataAccess> Devices { get; private set; }

	/// <summary>
	/// Gets the file header for the method
	/// </summary>
	public IFileHeader FileHeader { get; private set; }

	/// <summary>
	/// Gets the file error state.
	/// </summary>
	public IFileError FileError { get; }

	/// <summary>
	/// Gets a value indicating whether the last file operation caused a recorded error.
	/// If so, there may be additional information in FileError
	/// </summary>
	/// <value></value>
	public bool IsError { get; }

	/// <summary>
	/// Gets a value indicating whether a file was successfully opened.
	/// Inspect "FileError" when false
	/// </summary>
	public bool IsOpen { get; }

	/// <summary>
	/// Gets or sets the descriptions.
	/// </summary>
	private List<StorageDescription> Descriptions { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.InstrumentMethodFileLoader" /> class. 
	/// Constructor: Load data from file
	/// </summary>
	/// <param name="fileName">
	/// File to load
	/// </param>
	public InstrumentMethodFileLoader(string fileName)
	{
		MyErrors myErrors = new MyErrors();
		using (DeviceStorage deviceStorage = new DeviceStorage(fileName))
		{
			if (string.IsNullOrEmpty(deviceStorage.ErrorMessage))
			{
				Tuple<string, DeviceStorage.FileHeaderErrors> tuple = LoadDataFromStorage(deviceStorage);
				if (string.IsNullOrEmpty(tuple.Item1))
				{
					IsOpen = true;
				}
				else
				{
					IsError = true;
					myErrors.HasError = true;
					myErrors.ErrorMessage = tuple.Item1;
					myErrors.ErrorCode = (int)(1 + tuple.Item2);
				}
			}
			else
			{
				IsError = true;
				myErrors.HasError = true;
				myErrors.ErrorMessage = deviceStorage.ErrorMessage;
				myErrors.ErrorCode = 1;
				Descriptions = new List<StorageDescription>();
				Devices = new ReadOnlyDictionary<string, IInstrumentMethodDataAccess>(new Dictionary<string, IInstrumentMethodDataAccess>());
				FileHeader = new FileHeader();
			}
		}
		FileError = myErrors;
	}

	/// <summary>
	/// Get data from all instruments
	/// </summary>
	/// <param name="storage">Root storage of the method file</param>
	/// <param name="names">Names of the instruments</param>
	/// <returns>Data for each instrument</returns>
	private static Dictionary<string, IInstrumentMethodDataAccess> GetDataForAllInstruments(DeviceStorage storage, string[] names)
	{
		Dictionary<string, IInstrumentMethodDataAccess> dictionary = new Dictionary<string, IInstrumentMethodDataAccess>();
		foreach (string text in names)
		{
			IInstrumentMethodDataAccess value = storage.OpenDeviceComponent(text);
			dictionary.Add(text, value);
		}
		return dictionary;
	}

	/// <summary>
	/// Load data from storage.
	/// </summary>
	/// <param name="storage">
	/// The storage.
	/// </param>
	/// <returns>
	/// Error information
	/// </returns>
	private Tuple<string, DeviceStorage.FileHeaderErrors> LoadDataFromStorage(DeviceStorage storage)
	{
		Descriptions = new List<StorageDescription>();
		storage.EnumSubStgsNoRecursion(Descriptions);
		DeviceStorage.ReadHeaderResult readHeaderResult = storage.ReadFileHeader();
		FileHeader = readHeaderResult.Header;
		DeviceStorage.FileHeaderErrors errorClass = readHeaderResult.ErrorClass;
		if ((uint)(errorClass - 1) <= 2u)
		{
			return new Tuple<string, DeviceStorage.FileHeaderErrors>(readHeaderResult.ErrorMessage, readHeaderResult.ErrorClass);
		}
		string[] deviceNames = GetDeviceNames();
		Devices = new ReadOnlyDictionary<string, IInstrumentMethodDataAccess>(GetDataForAllInstruments(storage, deviceNames));
		return new Tuple<string, DeviceStorage.FileHeaderErrors>(string.Empty, DeviceStorage.FileHeaderErrors.NoError);
	}

	/// <summary>
	/// Get the list of device names in the method file
	/// </summary>
	/// <returns>The list of names</returns>
	private string[] GetDeviceNames()
	{
		string[] array = new string[Descriptions.Count];
		int num = 0;
		foreach (StorageDescription description in Descriptions)
		{
			array[num++] = description.StorageName;
		}
		return array;
	}
}
