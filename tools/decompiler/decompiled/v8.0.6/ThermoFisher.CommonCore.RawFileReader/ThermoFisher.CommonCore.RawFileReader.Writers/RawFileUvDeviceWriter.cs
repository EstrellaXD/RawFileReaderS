using System;
using System.IO;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The writer for saving UV device data into raw files.
/// </summary>
internal class RawFileUvDeviceWriter : RawFileBaseDeviceWriter, IRawFileDeviceWriter, IDisposable
{
	private readonly BufferInfo _peakDataBufferInfo;

	private readonly BufferInfo _scanHeaderBufferInfo;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.RawFileUvDeviceWriter" /> class.
	/// </summary>
	/// <param name="loaderId">
	/// The loader id.
	/// </param>
	/// <param name="deviceInfo">
	/// The device info.
	/// </param>
	/// <param name="rawFileName">
	/// The raw file name.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <param name="errors">
	/// The device errors.
	/// </param>
	public RawFileUvDeviceWriter(Guid loaderId, VirtualControllerInfo deviceInfo, string rawFileName, int fileRevision, DeviceErrors errors)
		: base(loaderId, deviceInfo, rawFileName, fileRevision, errors)
	{
		if (!errors.HasError)
		{
			_peakDataBufferInfo = new BufferInfo(loaderId, DataFileMapName, "PEAKDATA", creatable: false).UpdateErrors(errors);
			_scanHeaderBufferInfo = new BufferInfo(loaderId, DataFileMapName, "UVSCANINDEX", creatable: false, Utilities.StructSizeLookup.Value[5]).UpdateErrors(errors);
		}
	}

	/// <summary>
	/// The save.
	/// </summary>
	/// <param name="writer">
	/// The writer.
	/// </param>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <param name="packetDataOffset">
	/// The packet data offset.
	/// </param>
	/// <param name="controllerHeaderOffset">
	/// The controller header offset.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public bool Save(BinaryWriter writer, DeviceErrors errors, long packetDataOffset, out long controllerHeaderOffset)
	{
		bool flag = false;
		try
		{
			flag = WriteTempFileData("Data Pkt", writer, errors, base.RunHeader.DataPktFilename);
			if (flag)
			{
				flag = Save(writer, errors, packetDataOffset);
			}
			if (flag)
			{
				base.RunHeader.SpectrumPos = writer.BaseStream.Position;
				int maxBytesCopy = _scanHeaderBufferInfo.Size * base.RunHeader.NumSpectra;
				flag = WriteTempFileData("UV Spectrum", writer, errors, base.RunHeader.SpectFilename, maxBytesCopy);
			}
			if (flag)
			{
				writer.BaseStream.Position = base.RunHeader.DeviceFileOffset;
				base.RunHeader.SaveToRawFile(writer, errors);
			}
			writer.BaseStream.Seek(0L, SeekOrigin.End);
		}
		catch (Exception ex)
		{
			errors.UpdateError(ex);
		}
		controllerHeaderOffset = base.RunHeader.DeviceFileOffset;
		return flag;
	}

	/// <summary>
	/// The dispose.
	/// </summary>
	public override void Dispose()
	{
		base.Dispose();
		_peakDataBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.DataPktFilename).DisposeBufferInfo();
		_scanHeaderBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.SpectFilename).DisposeBufferInfo();
	}

	/// <summary>
	/// Refresh data from maps
	/// </summary>
	/// <returns>
	/// True if OK
	/// </returns>
	public bool Refresh()
	{
		if (RefreshMaps())
		{
			if (_peakDataBufferInfo.Refresh())
			{
				return _scanHeaderBufferInfo.Refresh();
			}
			return false;
		}
		return false;
	}
}
