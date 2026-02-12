using System;
using System.IO;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The writer for saving MS device data into raw files.
/// </summary>
internal class RawFileMsDeviceWriter : RawFileBaseDeviceWriter, IRawFileDeviceWriter, IDisposable
{
	private readonly BufferInfo _scanEventBufferInfo;

	private readonly BufferInfo _trailerHeaderBufferInfo;

	private readonly BufferInfo _tuneDataHeaderBufferInfo;

	private readonly BufferInfo _tuneDataBufferInfo;

	private readonly BufferInfo _peakDataBufferInfo;

	private readonly BufferInfo _scanHeaderBufferInfo;

	private readonly BufferInfo _trailerScanEventBufferInfo;

	private readonly BufferInfo _trailerExtraBufferInfo;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.RawFileMsDeviceWriter" /> class.
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
	public RawFileMsDeviceWriter(Guid loaderId, VirtualControllerInfo deviceInfo, string rawFileName, int fileRevision, DeviceErrors errors)
		: base(loaderId, deviceInfo, rawFileName, fileRevision, errors)
	{
		_scanEventBufferInfo = new BufferInfo(loaderId, DataFileMapName, "SCANEVENTS", creatable: false).UpdateErrors(errors);
		_trailerHeaderBufferInfo = new BufferInfo(loaderId, DataFileMapName, "TRAILERHEADER", creatable: false).UpdateErrors(errors);
		_tuneDataHeaderBufferInfo = new BufferInfo(loaderId, DataFileMapName, "TUNEDATAHEADER", creatable: false).UpdateErrors(errors);
		_tuneDataBufferInfo = new BufferInfo(loaderId, DataFileMapName, "TUNEDATA_FILEMAP", creatable: false).UpdateErrors(errors);
		_peakDataBufferInfo = new BufferInfo(loaderId, DataFileMapName, "PEAKDATA", creatable: false).UpdateErrors(errors);
		_scanHeaderBufferInfo = new BufferInfo(loaderId, DataFileMapName, "SCANHEADER", creatable: false).UpdateErrors(errors);
		_trailerScanEventBufferInfo = new BufferInfo(loaderId, DataFileMapName, "TRAILER_EVENTS", creatable: false).UpdateErrors(errors);
		_trailerExtraBufferInfo = new BufferInfo(loaderId, DataFileMapName, "TRAILEREXTRA", creatable: false).UpdateErrors(errors);
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
		bool flag = base.Save(writer, errors, packetDataOffset);
		if (flag)
		{
			flag = WriteTempFileData("Scan Events", writer, errors, base.RunHeader.ScanEventsFilename);
		}
		if (flag)
		{
			flag = WriteTempFileData("Trailer Header", writer, errors, base.RunHeader.TrailerHeaderFilename);
		}
		if (flag)
		{
			flag = WriteTempFileData("Tune Data Header", writer, errors, base.RunHeader.TuneDataHeaderFilename);
		}
		if (flag)
		{
			int maxBytesCopy = _tuneDataBufferInfo.Size * base.RunHeader.NumTuneData;
			flag = WriteTempFileData("Tune Data", writer, errors, base.RunHeader.TuneDataFilename, maxBytesCopy);
		}
		if (flag)
		{
			base.RunHeader.SpectrumPos = writer.BaseStream.Position;
			int maxBytesCopy2 = Marshal.SizeOf<ScanIndexStruct>() * base.RunHeader.NumSpectra;
			flag = WriteTempFileData("Spectrum Data", writer, errors, base.RunHeader.SpectFilename, maxBytesCopy2);
		}
		if (flag)
		{
			base.RunHeader.TrailerScanEventsPos = writer.BaseStream.Position;
			flag = WriteTempFileData("Trailer Scan Events", writer, errors, base.RunHeader.TrailerScanEventsFilename);
		}
		if (flag)
		{
			base.RunHeader.TrailerExtraPos = writer.BaseStream.Position;
			int bufferDataSize = GetBufferDataSize(_trailerExtraBufferInfo.Size, base.RunHeader.NumTrailerExtra, base.RunHeader.TrailerExtraFilename);
			flag = WriteTempFileData("Trailer Extra", writer, errors, base.RunHeader.TrailerExtraFilename, bufferDataSize);
		}
		if (flag)
		{
			writer.BaseStream.Position = base.RunHeader.DeviceFileOffset;
			base.RunHeader.SaveToRawFile(writer, errors);
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
		_scanEventBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.ScanEventsFilename).DisposeBufferInfo();
		_trailerHeaderBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.TrailerHeaderFilename).DisposeBufferInfo();
		_tuneDataHeaderBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.TuneDataHeaderFilename).DisposeBufferInfo();
		_tuneDataBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.TuneDataFilename).DisposeBufferInfo();
		_peakDataBufferInfo.DecrementReferenceCount();
		_peakDataBufferInfo.Dispose();
		_scanHeaderBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.SpectFilename).DisposeBufferInfo();
		_trailerScanEventBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.TrailerScanEventsFilename).DisposeBufferInfo();
		_trailerExtraBufferInfo.DeleteTempFileOnlyIfNoReference(base.RunHeader.TrailerExtraFilename).DisposeBufferInfo();
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
			if (_scanEventBufferInfo.Refresh() && _trailerHeaderBufferInfo.Refresh() && _tuneDataHeaderBufferInfo.Refresh() && _tuneDataBufferInfo.Refresh() && _peakDataBufferInfo.Refresh() && _scanHeaderBufferInfo.Refresh() && _trailerScanEventBufferInfo.Refresh())
			{
				return _trailerExtraBufferInfo.Refresh();
			}
			return false;
		}
		return false;
	}
}
