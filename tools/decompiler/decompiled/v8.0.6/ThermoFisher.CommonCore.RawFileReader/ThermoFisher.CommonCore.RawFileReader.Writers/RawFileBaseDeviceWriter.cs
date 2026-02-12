using System;
using System.IO;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The device temp file writer.
/// </summary>
internal class RawFileBaseDeviceWriter
{
	protected readonly string DataFileMapName;

	private readonly BufferInfo _instrumentIdBufferInfo;

	private readonly BufferInfo _statusLogHeaderBufferInfo;

	private readonly BufferInfo _statusLogBufferInfo;

	private readonly BufferInfo _errorLogBufferInfo;

	private bool _disposed;

	/// <summary>
	/// Gets the run header.
	/// </summary>
	public RunHeader RunHeader { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.RawFileBaseDeviceWriter" /> class.
	/// Creates objects which are used for all device types
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
	public RawFileBaseDeviceWriter(Guid loaderId, VirtualControllerInfo deviceInfo, string rawFileName, int fileRevision, DeviceErrors errors)
	{
		VirtualDeviceTypes virtualDeviceType = deviceInfo.VirtualDeviceType;
		int virtualDeviceIndex = deviceInfo.VirtualDeviceIndex;
		errors.AppendInformataion("Start: RawFileBaseDeviceWriter " + virtualDeviceType.ToString() + " " + virtualDeviceIndex);
		DataFileMapName = Utilities.BuildUniqueVirtualDeviceFileMapName(virtualDeviceType, virtualDeviceIndex, Utilities.MapName(rawFileName, string.Empty));
		errors.AppendInformataion("Create run header");
		IViewCollectionManager instance = MemoryMappedRawFileManager.Instance;
		RunHeader = new RunHeader(instance, loaderId, DataFileMapName, fileRevision);
		errors.AppendInformataion("Access instrument ID");
		_instrumentIdBufferInfo = new BufferInfo(loaderId, DataFileMapName, "INSTID", creatable: false).UpdateErrors(errors);
		errors.AppendInformataion("Access instrument ID Complete, Error State: " + errors.HasError);
		errors.AppendInformataion("Access status log header");
		_statusLogHeaderBufferInfo = new BufferInfo(loaderId, DataFileMapName, "STATUSLOGHEADER", creatable: false).UpdateErrors(errors);
		errors.AppendInformataion("Access status log header Complete, Error State: " + errors.HasError);
		errors.AppendInformataion("Access status log");
		_statusLogBufferInfo = new BufferInfo(loaderId, DataFileMapName, "STATUS_LOG", creatable: false).UpdateErrors(errors);
		errors.AppendInformataion("Access status log Complete, Error State: " + errors.HasError);
		errors.AppendInformataion("Access error log");
		_errorLogBufferInfo = new BufferInfo(loaderId, DataFileMapName, "ERROR_LOG", creatable: false).UpdateErrors(errors);
		errors.AppendInformataion("Access error log Complete, Error State: " + errors.HasError);
		errors.AppendInformataion("End: RawFileBaseDeviceWriter");
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
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	protected virtual bool Save(BinaryWriter writer, DeviceErrors errors, long packetDataOffset)
	{
		try
		{
			RunHeader.PacketPos = packetDataOffset;
			RunHeader.DeviceFileOffset = writer.BaseStream.Position;
			bool flag = RunHeader.SaveToRawFile(writer, errors);
			if (flag)
			{
				flag = WriteTempFileData("Instrument ID", writer, errors, RunHeader.InstIdFilename);
			}
			if (flag)
			{
				flag = WriteTempFileData("Status Log Header", writer, errors, RunHeader.StatusLogHeaderFilename);
			}
			if (flag)
			{
				RunHeader.StatusLogPos = writer.BaseStream.Position;
				int bufferDataSize = GetBufferDataSize(_statusLogBufferInfo.Size, RunHeader.NumStatusLog, RunHeader.StatusLogFilename);
				flag = WriteTempFileData("Status Log", writer, errors, RunHeader.StatusLogFilename, bufferDataSize);
			}
			if (flag)
			{
				RunHeader.ErrorLogPos = writer.BaseStream.Position;
				flag = WriteTempFileData("Error Log", writer, errors, RunHeader.ErrorLogFilename);
			}
			return flag;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// The write temp file data.
	/// </summary>
	/// <param name="purpose">Meaning of this file</param>
	/// <param name="writer">
	/// The writer.
	/// </param>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <param name="fileName">
	/// The file name.
	/// </param>
	/// <param name="maxBytesCopy">
	/// The max Bytes Copy.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	protected bool WriteTempFileData(string purpose, BinaryWriter writer, DeviceErrors errors, string fileName, int maxBytesCopy = -1)
	{
		try
		{
			if (!File.Exists(fileName))
			{
				throw new FileNotFoundException(purpose + ": " + fileName);
			}
			errors.AppendInformataion("Start WriteTempFileData: " + purpose);
			using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				if (maxBytesCopy > -1)
				{
					errors.AppendInformataion("WriteTempFileData: [" + purpose + "] maxBytesCopy: " + maxBytesCopy);
					long length = fileStream.Length;
					if (maxBytesCopy == length)
					{
						fileStream.CopyTo(writer.BaseStream);
					}
					else if (maxBytesCopy < length)
					{
						int num = maxBytesCopy;
						if (num > 81920)
						{
							byte[] buffer = new byte[81920];
							while (num >= 81920)
							{
								fileStream.Read(buffer, 0, 81920);
								writer.Write(buffer);
								num -= 81920;
							}
						}
						if (num > 0)
						{
							byte[] buffer2 = new byte[num];
							fileStream.Read(buffer2, 0, num);
							writer.Write(buffer2);
						}
					}
					else
					{
						fileStream.CopyTo(writer.BaseStream);
						errors.AppendWarning("Requested size larger than stream: " + length);
					}
				}
				else
				{
					errors.AppendInformataion("WriteTempFileData: [" + purpose + "] temp file length: " + fileStream.Length);
					fileStream.CopyTo(writer.BaseStream);
				}
			}
			writer.Flush();
			errors.AppendInformataion("End WriteTempFileData: " + purpose);
			return true;
		}
		catch (Exception ex)
		{
			return errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Gets the size of the data from the buffer and compares to size of the temp file.  Only copy minimum bytes.
	/// Used by some temp files that write garbage data to temp file.
	/// </summary>
	/// <param name="size">
	/// The size.
	/// </param>
	/// <param name="numberEntries">
	/// The number Entries.
	/// </param>
	/// <param name="tempFileName">
	/// The temp file name.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	protected int GetBufferDataSize(int size, int numberEntries, string tempFileName)
	{
		long num = (long)size * (long)numberEntries;
		long num2 = ((num < 0) ? long.MaxValue : num);
		FileInfo fileInfo = new FileInfo(tempFileName);
		num2 = ((fileInfo.Length > num2) ? num2 : fileInfo.Length);
		return (int)num2;
	}

	/// <summary>
	/// Disposes the shared memory resources used by the device.
	/// </summary>
	public virtual void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			RunHeader.Dispose();
			_instrumentIdBufferInfo.DeleteTempFileOnlyIfNoReference(RunHeader.InstIdFilename).DisposeBufferInfo();
			_statusLogHeaderBufferInfo.DeleteTempFileOnlyIfNoReference(RunHeader.StatusLogHeaderFilename).DisposeBufferInfo();
			_statusLogBufferInfo.DeleteTempFileOnlyIfNoReference(RunHeader.StatusLogFilename).DisposeBufferInfo();
			_errorLogBufferInfo.DeleteTempFileOnlyIfNoReference(RunHeader.ErrorLogFilename).DisposeBufferInfo();
		}
	}

	/// <summary>
	/// Refresh all objects from data in memory maps.
	/// Do not create new maps.
	/// </summary>
	/// <returns>
	/// True if OK
	/// </returns>
	protected bool RefreshMaps()
	{
		RunHeader.Refresh();
		if (_instrumentIdBufferInfo.Refresh() && _statusLogHeaderBufferInfo.Refresh() && _statusLogBufferInfo.Refresh())
		{
			return _errorLogBufferInfo.Refresh();
		}
		return false;
	}
}
