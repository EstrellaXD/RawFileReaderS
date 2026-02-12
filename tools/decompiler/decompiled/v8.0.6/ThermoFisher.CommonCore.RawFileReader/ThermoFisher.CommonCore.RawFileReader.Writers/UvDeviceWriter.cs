using System;
using System.IO;
using System.Threading;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.RunHeaders;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides methods to write UV Device data.<para />
/// Note: The following functions should be called before acquisition begins:<para />
/// 1. Write Instrument Information<para />
/// 2. Write Instrument Expected Run Time<para />
/// 3. Write Status Log Header
/// <para>Supports all simple devices (such as analog inputs)</para>
/// </summary>
internal class UvDeviceWriter : BaseDeviceWriter, INonScanningDeviceWriter, IUvDeviceWriter, IBaseDeviceWriter, IDisposable, IFileError, IAnalogDeviceWriter, IOtherDeviceWriter, IUvDeviceBinaryWriter, IBinaryBaseDataWriter, IAnalogDeviceBinaryWriter, IOtherDeviceBinaryWriter
{
	private readonly BinaryWriter _scanDataWriter;

	private readonly BinaryWriter _scanIndexWriter;

	private readonly BufferInfo _peakDataBufferInfo;

	private readonly BufferInfo _scanIndexBufferInfo;

	private readonly PeakData _peakData;

	private readonly SpectrumPacketType _specPacketType;

	private bool _disposed;

	private ScanIndexStruct _currentScanIndex;

	private Mutex _noNameDataPktFileMutex;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.UvDeviceWriter" /> class.
	/// </summary>
	/// <param name="deviceType">Type of the device.</param>
	/// <param name="deviceIndex">Index of the device.</param>
	/// <param name="rawFileName">Name of the raw file.</param>
	/// <param name="inAcquisition">if set to <c>true</c> [in acquisition].</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="domain">Determines the format of this channel (such as Xcalibur or chromeleon)</param>
	public UvDeviceWriter(Device deviceType, int deviceIndex, string rawFileName, bool inAcquisition, RawDataDomain domain = RawDataDomain.MassSpectrometry, int fileRevision = 66)
		: base(deviceType, deviceIndex, rawFileName, fileRevision, inAcquisition, 1, domain)
	{
		if (base.HasError)
		{
			return;
		}
		try
		{
			bool flag = false;
			_currentScanIndex = default(ScanIndexStruct);
			_peakData = new PeakData();
			_noNameDataPktFileMutex = Utilities.CreateNoNameMutex();
			switch (base.DeviceRunHeader.DeviceType)
			{
			case VirtualDeviceTypes.UvDevice:
				_specPacketType = SpectrumPacketType.UvChannel;
				break;
			case VirtualDeviceTypes.MsAnalogDevice:
			case VirtualDeviceTypes.AnalogDevice:
				_specPacketType = SpectrumPacketType.MassSpecAnalog;
				break;
			case VirtualDeviceTypes.PdaDevice:
				_specPacketType = SpectrumPacketType.PdaUvScannedSpectrum;
				break;
			case VirtualDeviceTypes.StatusDevice:
				_specPacketType = SpectrumPacketType.InvalidPacket;
				flag = true;
				break;
			default:
				_specPacketType = SpectrumPacketType.InvalidPacket;
				DevErrors.UpdateError("Unknown device type.");
				return;
			}
			if (!inAcquisition || !base.InCreation)
			{
				return;
			}
			RunHeaderStruct runHeaderStruct = base.DeviceRunHeader.RunHeaderStruct;
			string text = Utilities.BuildUniqueVirtualDeviceStreamTempFileName(base.DeviceRunHeader.DeviceType, base.DeviceRunHeader.DeviceIndex);
			if (TempFileHelper.CreateTempFile(rawFileName, out base.DataStreamWriters[4], out runHeaderStruct.DataPktFile, text + "PEAKDATA", flag, flag) && TempFileHelper.CreateTempFile(rawFileName, out base.DataStreamWriters[5], out runHeaderStruct.SpectrumFile, text + "SPECTRUM", flag, flag))
			{
				_scanDataWriter = base.DataStreamWriters[4];
				_scanIndexWriter = base.DataStreamWriters[5];
				_peakDataBufferInfo = new BufferInfo(DeviceId, base.DataFileMapName, "PEAKDATA", creatable: true).UpdateErrors(DevErrors);
				_scanIndexBufferInfo = new BufferInfo(DeviceId, base.DataFileMapName, "UVSCANINDEX", creatable: true, Utilities.StructSizeLookup.Value[5]).UpdateErrors(DevErrors);
				base.DeviceRunHeader.CopyRunHeaderStruct(ref runHeaderStruct);
				if (base.DeviceRunHeader.SaveRunHeader(base.RunHeaderMemMapAccessor, DevErrors))
				{
					DeviceAcqStatus.DeviceStatus = VirtualDeviceAcquireStatus.DeviceStatusSetup;
				}
			}
		}
		catch (Exception ex)
		{
			DevErrors.UpdateError(ex.ToMessageAndCompleteStacktrace(), ex.HResult);
		}
	}

	/// <summary>
	/// Writes the analog instrument data.
	/// </summary>
	/// <param name="instData">The analog instrument data.</param>
	/// <param name="instDataIndex">Index of the analog instrument data.</param>
	/// <returns>True instrument data write to the disk file; otherwise False.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	/// Instrument Data
	/// or
	/// Analog Scan Index
	/// </exception>
	public bool WriteInstData(double[] instData, IAnalogScanIndex instDataIndex)
	{
		if (base.HasError)
		{
			return false;
		}
		if (instData == null)
		{
			throw new ArgumentNullException("instData").UpdateErrorsException(DevErrors);
		}
		if (instDataIndex == null)
		{
			throw new ArgumentNullException("instDataIndex").UpdateErrorsException(DevErrors);
		}
		ThermoFisher.CommonCore.Data.Business.UvScanIndex instDataIndex2 = new ThermoFisher.CommonCore.Data.Business.UvScanIndex
		{
			TIC = instDataIndex.TIC,
			StartTime = instDataIndex.StartTime,
			NumberOfChannels = instDataIndex.NumberOfChannels,
			IsUniformTime = true
		};
		if (ValidateInstData(instData, instDataIndex2))
		{
			return WriteInstDataToFile(instData, instDataIndex2);
		}
		return false;
	}

	/// <summary>
	/// Writes the UV instrument data.
	/// </summary>
	/// <param name="instData">The UV instrument data.</param>
	/// <param name="instDataIndex">Index of the UV instrument data.</param>
	/// <returns>True instrument data write to the disk file; otherwise False.</returns>
	/// <exception cref="T:System.ArgumentNullException">
	/// Instrument Data
	/// or
	/// UV Scan Index
	/// </exception>
	public virtual bool WriteInstData(double[] instData, IUvScanIndex instDataIndex)
	{
		if (base.HasError)
		{
			return false;
		}
		if (instData == null)
		{
			throw new ArgumentNullException("instData").UpdateErrorsException(DevErrors);
		}
		if (instDataIndex == null)
		{
			throw new ArgumentNullException("instDataIndex").UpdateErrorsException(DevErrors);
		}
		if (ValidateInstData(instData, instDataIndex))
		{
			return WriteInstDataToFile(instData, instDataIndex);
		}
		return false;
	}

	/// <summary>
	/// Writes the instrument data to file.
	/// </summary>
	/// <param name="instData">The instrument data.</param>
	/// <param name="instDataIndex">Index of the instrument data.</param>
	/// <returns>True instrument data write to the disk file; otherwise False.</returns>
	private bool WriteInstDataToFile(double[] instData, IUvScanIndex instDataIndex)
	{
		if (DeviceAcqStatus.CanDeviceAcquireData)
		{
			byte[] data = instData.ToByteArray();
			int dataSize = instData.Length * 8;
			ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex uvScanIndex = instDataIndex.ConvertToUvScanIndex();
			uvScanIndex.PacketType = _specPacketType;
			if (WriteInstData(data, dataSize, uvScanIndex.PacketType, uvScanIndex.NumberOfChannels, uvScanIndex.IsUniformTime))
			{
				bool num = WriteInstScanIndex(uvScanIndex);
				if (num && DeviceAcqStatus.IsDeviceReady)
				{
					DeviceAcqStatus.DeviceStatus = VirtualDeviceAcquireStatus.DeviceStatusAcquiring;
				}
				return num;
			}
		}
		else
		{
			DevErrors.UpdateError("Device is not ready for acquiring data.");
		}
		return false;
	}

	/// <summary>
	/// Validates the UV and Analog instrument data.
	/// </summary>
	/// <param name="instData">The instrument data.</param>
	/// <param name="instDataIndex">Index of the UV instrument data.</param>
	/// <returns>True if the input data has the correct format; otherwise false.</returns>
	private bool ValidateInstData(double[] instData, IUvScanIndex instDataIndex)
	{
		try
		{
			int num = (instDataIndex.IsUniformTime ? 1 : 2);
			int numberOfChannels = instDataIndex.NumberOfChannels;
			int num2 = ((base.DeviceType == Device.UV) ? (numberOfChannels * num) : numberOfChannels);
			if (instData.Length != num2)
			{
				string message = $"You're using Device type: {base.DeviceType}, Packet type: {_specPacketType}, {numberOfChannels} channels and IsUniformTime flag: {instDataIndex.IsUniformTime}, which needs {num2} data value(s), but {instData.Length} value(s) is/are included.";
				throw new ArgumentOutOfRangeException("instData", message);
			}
			return true;
		}
		catch (Exception ex)
		{
			return DevErrors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Writes the instrument data packets to the file.<para />
	/// This routine will branch to the appropriate packet routines.
	/// </summary>
	/// <param name="data">Block of a formatted data to write.</param>
	/// <param name="dataSize">Size of the data block.</param>
	/// <param name="packetType">Type of the packet.</param>
	/// <param name="numChannels">The number channels.</param>
	/// <param name="isUniformTime">if set to <c>true</c> [is uniform time].</param>
	/// <returns> True if packets are written to disk successfully, False otherwise.</returns>
	/// <exception cref="T:System.NotSupportedException">Not Supported Exception</exception>
	protected bool WriteInstData(byte[] data, int dataSize, SpectrumPacketType packetType, int numChannels, bool isUniformTime)
	{
		try
		{
			_peakData.PacketType = packetType;
			return packetType switch
			{
				SpectrumPacketType.MassSpecAnalog => WriteInstDataPackets(data, dataSize, 8 * numChannels, updateNumPackets: true), 
				SpectrumPacketType.UvChannel => WriteInstDataPackets(data, dataSize, 8 * (isUniformTime ? 1 : 2) * numChannels, updateNumPackets: true), 
				SpectrumPacketType.PdaUvScannedSpectrum => WriteInstDataPackets(data, dataSize, 4, updateNumPackets: false), 
				SpectrumPacketType.PdaUvScannedSpectrumIndex => _peakData.WriteInstIndex(data, dataSize), 
				_ => throw new NotSupportedException(packetType.ToString() + " is not supported"), 
			};
		}
		catch (Exception ex)
		{
			return DevErrors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Write instrument data packets to the file.
	/// This routine will branch to the appropriate packet routines to compress the data block before being written to disk.
	/// </summary>
	/// <param name="data">The data to write to file.</param>
	/// <param name="dataSize">The data size.</param>
	/// <param name="packetSize">Size of each packet in the data block.</param>
	/// <param name="updateNumPackets">The update number of packets.</param>
	/// <returns>TRUE if packets are written to disk successfully, FALSE otherwise.</returns>
	private bool WriteInstDataPackets(byte[] data, int dataSize, int packetSize, bool updateNumPackets)
	{
		return WriterHelper.CritSec(delegate(DeviceErrors err)
		{
			bool result = false;
			if (_peakData.WriteInstData(_scanDataWriter, data, err))
			{
				if (updateNumPackets)
				{
					_currentScanIndex.NumberPackets = dataSize / packetSize;
				}
				result = true;
			}
			return result;
		}, DevErrors, _noNameDataPktFileMutex);
	}

	/// <summary>
	/// Writes instrument scan index (header) to a file.
	/// </summary>
	/// <param name="scanIndex">Index of the UV scan.</param>
	/// <returns>True if scan index is written to disk successfully, False otherwise</returns>
	protected bool WriteInstScanIndex(ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex scanIndex)
	{
		bool result = false;
		if (!base.HasError)
		{
			result = WriterHelper.CritSec(delegate(DeviceErrors err)
			{
				bool result2 = false;
				UvScanIndexStruct uvScanIndexStructInfo = scanIndex.UvScanIndexStructInfo;
				if (_peakData.PacketType == SpectrumPacketType.PdaUvDiscreteChannelIndex || _peakData.PacketType == SpectrumPacketType.PdaUvScannedSpectrumIndex)
				{
					_currentScanIndex.NumberPackets += _peakData.WriteIndices(_scanDataWriter);
				}
				uvScanIndexStructInfo.DataOffset = _peakData.CurrentStartOfData;
				uvScanIndexStructInfo.NumberPackets = _currentScanIndex.NumberPackets;
				_peakData.CurrentStartOfData = _peakData.NextStartOfData;
				uvScanIndexStructInfo.ScanNumber = base.DeviceRunHeader.NumSpectra + 1;
				if (uvScanIndexStructInfo.Save(_scanIndexWriter, err))
				{
					base.DeviceRunHeader.LowMass = Math.Min(base.DeviceRunHeader.LowMass, uvScanIndexStructInfo.ShortWavelength);
					base.DeviceRunHeader.HighMass = Math.Max(base.DeviceRunHeader.HighMass, uvScanIndexStructInfo.LongWavelength);
					base.DeviceRunHeader.MaxIntegratedIntensity = Math.Max(base.DeviceRunHeader.MaxIntegratedIntensity, uvScanIndexStructInfo.TIC);
					base.DeviceRunHeader.StartTime = Math.Min(base.DeviceRunHeader.StartTime, uvScanIndexStructInfo.StartTime);
					base.DeviceRunHeader.EndTime = Math.Max(base.DeviceRunHeader.EndTime, uvScanIndexStructInfo.StartTime);
					if (base.DeviceRunHeader.SaveRunHeader(base.RunHeaderMemMapAccessor, err))
					{
						base.DeviceRunHeader.NumSpectra = uvScanIndexStructInfo.ScanNumber;
						if (RunHeaderExtension.SaveScanNum(base.RunHeaderMemMapAccessor, base.DeviceRunHeader.NumSpectra, err))
						{
							_scanIndexBufferInfo.IncrementNumElements();
							result2 = true;
						}
					}
				}
				return result2;
			}, DevErrors, _noNameDataPktFileMutex);
			_currentScanIndex.NumberPackets = 0;
		}
		return result;
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public override void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		try
		{
			Utilities.ReleaseMutex(ref _noNameDataPktFileMutex);
			string tempFileName = string.Empty;
			string tempFileName2 = string.Empty;
			if (base.DeviceRunHeader != null)
			{
				tempFileName = base.DeviceRunHeader.DataPktFilename;
				tempFileName2 = base.DeviceRunHeader.SpectFilename;
			}
			base.Dispose();
			_peakDataBufferInfo.DeleteTempFileOnlyIfNoReference(tempFileName).DisposeBufferInfo();
			_scanIndexBufferInfo.DeleteTempFileOnlyIfNoReference(tempFileName2).DisposeBufferInfo();
		}
		catch (Exception ex)
		{
			DevErrors.UpdateError(ex);
		}
	}
}
