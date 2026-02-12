using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using OpenMcdf;
using OpenMcdf.Extensions;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Data for a particular device, from an instrument method.
/// </summary>
internal class InstrumentMethodDataAccess : IInstrumentMethodDataAccess
{
	/// <summary>
	/// Gets the plain text form of an instrument method
	/// </summary>
	public string MethodText { get; private set; }

	/// <summary>
	/// Gets all streams for this instrument, apart from the "Text" stream.
	/// Typically an instrument has a stream called "Data" containing the method in binary or XML.
	/// Other streams (private to the instrument) may also be created.
	/// </summary>
	public IReadOnlyDictionary<string, byte[]> StreamBytes { get; private set; }

	/// <summary>
	/// Gets or sets the available streams.
	/// </summary>
	private List<string> AvailableStreams { get; set; }

	/// <summary>
	/// Opens a stream on created storage
	/// </summary>
	/// <param name="stgDevice">The storage containing data for a device</param>
	/// <param name="streamName">The stream within the device</param>
	/// <returns>Access to the requested stream</returns>
	private Stream OpenStream(CFStorage stgDevice, string streamName)
	{
		Stream result = null;
		if (stgDevice != null)
		{
			result = stgDevice.GetStream(streamName).AsIOStream();
		}
		return result;
	}

	/// <summary>
	/// get stream names.
	/// </summary>
	/// <param name="stgDevice">
	/// The storage device.
	/// </param>
	/// <returns>
	/// The names.
	/// </returns>
	private List<string> GetStreamNames(CFStorage stgDevice)
	{
		if (stgDevice != null)
		{
			return DeviceStorage.GetStorageNames(stgDevice, StgType.Stream);
		}
		return null;
	}

	/// <summary>
	/// Open the streams for the device, and retrieve the text description.
	/// </summary>
	/// <param name="stgDevice">Storage containing device data</param>
	internal void Open(CFStorage stgDevice)
	{
		try
		{
			AvailableStreams = GetStreamNames(stgDevice);
			if (AvailableStreams.Contains("Text"))
			{
				Stream stream = OpenStream(stgDevice, "Text");
				if (stream != null)
				{
					long offset = 0L;
					long length = stream.Length;
					stream.Seek(offset, SeekOrigin.Begin);
					StreamIo streamIo = new StreamIo(stream);
					MethodText = streamIo.ReadCharArray((int)length);
				}
			}
			Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>();
			foreach (string availableStream in AvailableStreams)
			{
				if (!(availableStream == "Text"))
				{
					Stream stream2 = OpenStream(stgDevice, availableStream);
					if (stream2 != null)
					{
						long offset2 = 0L;
						long length2 = stream2.Length;
						stream2.Seek(offset2, SeekOrigin.Begin);
						StreamIo streamIo2 = new StreamIo(stream2);
						byte[] array = new byte[length2];
						streamIo2.Read(array);
						dictionary.Add(availableStream, array);
					}
				}
			}
			StreamBytes = new ReadOnlyDictionary<string, byte[]>(dictionary);
		}
		catch (Exception)
		{
		}
	}
}
