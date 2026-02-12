using System;
using System.IO;
using System.Text;
using System.Threading;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.RunHeaders;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.ExtraScanInfo;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides methods to write mass spec device data.<para />
/// The "PrepareForRun" method should be called during the prepare for run state, before the data acquisition begins. <para />
/// The rest of the methods will be used for data logging.
/// </summary>
internal sealed class MassSpecDeviceWriter : BaseDeviceWriter, IMassSpecDeviceBinaryWriter, IMassSpecDeviceWriter, IDisposable, IFileError, IBinaryBaseDataWriter
{
	private const string ErrorMsgDeviceNotReadyOrDataSaved = "Either the device writer is not ready or the data has already been written to the file.";

	private const string ErrorMsgDeviceNotReadyOrFileInError = "Either the device writer is not ready for acquiring data or the file is in error.";

	/// <summary>
	/// The data initial offset
	/// Mass spec data is written directly to the raw file after the meta-data sections.
	/// </summary>
	private readonly long _dataInitialOffset;

	private readonly int _highResDataStructSize = Utilities.StructSizeLookup.Value[14];

	private readonly int _lowResDataStructSize = Utilities.StructSizeLookup.Value[15];

	private readonly PeakData _peakData;

	private readonly int _profileDataPacket64Type = Utilities.StructSizeLookup.Value[9];

	private readonly BinaryWriter _scanDataWriter;

	private readonly BinaryWriter _scanIndexWriter;

	private readonly BinaryWriter _trailerScanEventWriter;

	private ScanIndexStruct _currentScanIndex;

	private bool _disposed;

	private Mutex _noNamePeakDataMutex;

	private Mutex _noNameScanEventsMutex;

	private Mutex _noNameTrailerExtraDataMutex;

	private Mutex _noNameTuneDataMutex;

	private BufferInfo _peakDataBufferInfo;

	private BufferInfo _scanEventsBufferInfo;

	private BufferInfo _scanIndexBufferInfo;

	private BufferInfo _trailerExtraBufferInfo;

	/// <summary>
	/// Generic type headers.
	/// </summary>
	private DataDescriptors _trailerExtraHeader;

	private BufferInfo _trailerExtraHeaderBufferInfo;

	private BufferInfo _trailerScanEventsBufferInfo;

	private BufferInfo _tuneDataBufferInfo;

	private DataDescriptors _tuneHeader;

	private BufferInfo _tuneHeaderBufferInfo;

	private int _writtenFilterMassPrecision;

	private int _writtenMassResolution;

	/// <summary>
	/// Gets a value indicating whether the PrepareForRun method has been called.
	/// </summary>
	/// <value>
	/// True if the PrepareForRun method has been called; otherwise, false.
	/// </value>
	public bool IsPreparedForRun => DeviceAcqStatus.CanDeviceAcquireData;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.MassSpecDeviceWriter" /> class.
	/// </summary>
	/// <param name="deviceType">Type of the device.</param>
	/// <param name="deviceIndex">Index of the device.</param>
	/// <param name="rawFileName">Name of the raw file.</param>
	/// <param name="inAcquisition">if set to <c>true</c> [in acquisition].</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="finalPacketType">The final known packet type (at the time of writing). 
	/// For example: If the MS is compiled against a newer raw file DLL, needed for it's new format,
	/// then the value saved in a file could be a higher value than that compiled into some data processing application
	/// built 2 years earlier. On opening the MS data, the raw file reader returns an Error to that application.
	/// The application should then issue a message telling the customer to upgrade.
	/// An instrument may set this to a lower value (the highest format required for the current acquisition method),
	/// to signal that older format data is in the file, even if the device was compiled against a newer DLL.</param>
	/// <param name="domain">raw data domain, by default "Legacy" new files with other domains may have additional features</param>
	public MassSpecDeviceWriter(Device deviceType, int deviceIndex, string rawFileName, bool inAcquisition, int fileRevision, SpectrumPacketType finalPacketType, RawDataDomain domain = RawDataDomain.Legacy)
		: base(deviceType, deviceIndex, rawFileName, fileRevision, inAcquisition, (short)finalPacketType, domain)
	{
		if (base.HasError)
		{
			return;
		}
		_noNameTrailerExtraDataMutex = Utilities.CreateNoNameMutex();
		_noNameTuneDataMutex = Utilities.CreateNoNameMutex();
		_noNameScanEventsMutex = Utilities.CreateNoNameMutex();
		_noNamePeakDataMutex = Utilities.CreateNoNameMutex();
		_peakData = new PeakData();
		_currentScanIndex = default(ScanIndexStruct);
		base.WrittenGenericHeadersFlags = new int[3];
		try
		{
			if (!inAcquisition || !base.InCreation)
			{
				return;
			}
			RunHeaderStruct runHeaderStruct = base.DeviceRunHeader.RunHeaderStruct;
			if (AttachMassSpecPacketStreamToRawFile(rawFileName, ref runHeaderStruct) && InitializeTemporaryFiles(ref runHeaderStruct, rawFileName, base.DeviceRunHeader.DeviceType, base.DeviceRunHeader.DeviceIndex))
			{
				_scanDataWriter = base.DataStreamWriters[4];
				PeakData peakData = _peakData;
				long currentStartOfData = (_peakData.DataInitialOffset = (_dataInitialOffset = _scanDataWriter.BaseStream.Position));
				peakData.CurrentStartOfData = currentStartOfData;
				_scanIndexWriter = base.DataStreamWriters[5];
				_trailerScanEventWriter = base.DataStreamWriters[11];
				InitializeBufferInfo();
				base.DeviceRunHeader.CopyRunHeaderStruct(ref runHeaderStruct);
				if (base.DeviceRunHeader.SaveRunHeader(base.RunHeaderMemMapAccessor, DevErrors))
				{
					DeviceAcqStatus.DeviceStatus = VirtualDeviceAcquireStatus.DeviceStatusSetup;
				}
			}
		}
		catch (Exception ex)
		{
			DevErrors.UpdateError(ex);
		}
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
			Utilities.ReleaseMutex(ref _noNameScanEventsMutex);
			Utilities.ReleaseMutex(ref _noNameTrailerExtraDataMutex);
			Utilities.ReleaseMutex(ref _noNameTuneDataMutex);
			Utilities.ReleaseMutex(ref _noNamePeakDataMutex);
			string tempFileName = string.Empty;
			string tempFileName2 = string.Empty;
			string tempFileName3 = string.Empty;
			string tempFileName4 = string.Empty;
			string tempFileName5 = string.Empty;
			string tempFileName6 = string.Empty;
			string tempFileName7 = string.Empty;
			string tempFileName8 = string.Empty;
			if (base.DeviceRunHeader != null)
			{
				tempFileName = base.DeviceRunHeader.DataPktFilename;
				tempFileName2 = base.DeviceRunHeader.SpectFilename;
				tempFileName3 = base.DeviceRunHeader.ScanEventsFilename;
				tempFileName4 = base.DeviceRunHeader.TuneDataHeaderFilename;
				tempFileName5 = base.DeviceRunHeader.TuneDataFilename;
				tempFileName6 = base.DeviceRunHeader.TrailerHeaderFilename;
				tempFileName7 = base.DeviceRunHeader.TrailerExtraFilename;
				tempFileName8 = base.DeviceRunHeader.TrailerScanEventsFilename;
			}
			base.Dispose();
			_peakDataBufferInfo.DeleteTempFileOnlyIfNoReference(tempFileName).DisposeBufferInfo();
			_scanIndexBufferInfo.DeleteTempFileOnlyIfNoReference(tempFileName2).DisposeBufferInfo();
			_scanEventsBufferInfo.DeleteTempFileOnlyIfNoReference(tempFileName3).DisposeBufferInfo();
			_trailerExtraHeaderBufferInfo.DeleteTempFileOnlyIfNoReference(tempFileName6).DisposeBufferInfo();
			_trailerExtraBufferInfo.DeleteTempFileOnlyIfNoReference(tempFileName7).DisposeBufferInfo();
			_tuneHeaderBufferInfo.DeleteTempFileOnlyIfNoReference(tempFileName4).DisposeBufferInfo();
			_tuneDataBufferInfo.DeleteTempFileOnlyIfNoReference(tempFileName5).DisposeBufferInfo();
			_trailerScanEventsBufferInfo.DeleteTempFileOnlyIfNoReference(tempFileName8).DisposeBufferInfo();
		}
		catch (Exception ex)
		{
			DevErrors.UpdateError(ex);
		}
	}

	/// <inheritdoc />
	public bool PrepareForRun(byte[] packedInstrumentData, IPackedMassSpecHeaders packedHeaders, byte[] packedRunHeaderInfo, byte[] packedMsScanEvents)
	{
		if (packedInstrumentData == null || packedInstrumentData.Length == 0)
		{
			throw new ArgumentNullException("packedInstrumentData");
		}
		if (packedHeaders == null)
		{
			throw new ArgumentNullException("packedHeaders");
		}
		if (packedRunHeaderInfo == null || packedRunHeaderInfo.Length == 0)
		{
			throw new ArgumentNullException("packedRunHeaderInfo");
		}
		if (packedMsScanEvents == null || packedMsScanEvents.Length == 0)
		{
			throw new ArgumentNullException("packedMsScanEvents");
		}
		IMassSpecRunHeaderInfo massSpecRunHeaderInfo = MsDataPacker.UnpackRunHeader(packedRunHeaderInfo);
		IMassSpecGenericHeaders massSpecGenericHeaders = packedHeaders.Unpack();
		IInstrumentDataAccess instId = packedInstrumentData.UnpackInstrumentData();
		if (WriteInstrumentInfo(instId) && WriteInstExpectedRunTime(massSpecRunHeaderInfo.ExpectedRunTime) && WriteStatusLogHeader(massSpecGenericHeaders.StatusLogHeader) && WriteTrailerExtraHeader(massSpecGenericHeaders.TrailerExtraHeader) && WriteTuneHeader(massSpecGenericHeaders.TuneHeader) && WriteInstMassResolution(massSpecRunHeaderInfo.MassResolution) && WriterInstFilterMassPrecision(massSpecRunHeaderInfo.Precision) && WriteInstComments(massSpecRunHeaderInfo.Comment1, massSpecRunHeaderInfo.Comment2))
		{
			return WriteMethodScanEvents(packedMsScanEvents);
		}
		return false;
	}

	/// <summary>
	/// This method should be called (when creating an acquisition file) during the "Prepare for run" state.<para />
	/// It may not be called multiple times for one device. It may not be called after any of the data logging calls have been made.<para />
	/// It will perform the following operations:
	/// 1. Write instrument information<para />
	/// 2. Write run header information
	/// 3. Write status log header <para />
	/// 4. Write trailer extra header <para />
	/// 5. Write tune data header <para />     
	/// 6. Write run header information - expected run time, comments, mass resolution and precision.<para />
	/// 7. Write method scan events.
	/// </summary>
	/// <param name="instrumentId">The instrument ID.</param>
	/// <param name="headers">The generic data headers.</param>
	/// <param name="runHeaderInfo">The run header information.</param>
	/// <param name="methodScanEvents">Method scan events</param>
	/// <returns>True if all the values are written to disk successfully, false otherwise.</returns>
	public bool PrepareForRun(IInstrumentDataAccess instrumentId, IMassSpecGenericHeaders headers, IMassSpecRunHeaderInfo runHeaderInfo, IScanEvents methodScanEvents)
	{
		if (instrumentId == null)
		{
			throw new ArgumentNullException("instrumentId");
		}
		if (headers == null)
		{
			throw new ArgumentNullException("headers");
		}
		if (runHeaderInfo == null)
		{
			throw new ArgumentNullException("runHeaderInfo");
		}
		if (methodScanEvents == null)
		{
			throw new ArgumentNullException("methodScanEvents");
		}
		if (WriteInstrumentInfo(instrumentId) && WriteInstExpectedRunTime(runHeaderInfo.ExpectedRunTime) && WriteStatusLogHeader(headers.StatusLogHeader) && WriteTrailerExtraHeader(headers.TrailerExtraHeader) && WriteTuneHeader(headers.TuneHeader) && WriteInstMassResolution(runHeaderInfo.MassResolution) && WriterInstFilterMassPrecision(runHeaderInfo.Precision) && WriteInstComments(runHeaderInfo.Comment1, runHeaderInfo.Comment2))
		{
			return WriteMethodScanEvents(methodScanEvents);
		}
		return false;
	}

	/// <summary>
	/// This method is designed for exporting mass spec scanned data to a file (mostly used by the Application). <para />
	/// It converts the input scanned data into the compressed packet format and also generates a profile index 
	///  if needed by the specified packet type. <para />
	/// Overall, it writes the mass spec data packets, scan index (scan header) and trailer scan event if it is provided,
	/// to a file. <para />
	/// This method will branch to the appropriate packet methods to compress the data block before being written to disk.
	/// </summary>       
	/// <param name="instData">The transferring data that are going to be saved to a file.</param>
	/// <returns>True if mass spec data packets are written to disk successfully; false otherwise.</returns>
	/// <exception cref="T:System.ArgumentNullException">Instrument data cannot be null.</exception>
	/// <exception cref="T:System.NotImplementedException">The packet type is not yet implement.</exception>
	public bool WriteInstData(IMsInstrumentData instData)
	{
		if (instData == null)
		{
			throw new ArgumentNullException("instData");
		}
		IScanStatisticsAccess statisticsData = instData.StatisticsData;
		SpectrumPacketType packetType = (SpectrumPacketType)(statisticsData.PacketType & 0xFFFF);
		if (!CannotAcquire("Either the device writer is not ready for acquiring data or the file is in error.") && PeakData.IsSupportedPacketTypes(packetType))
		{
			try
			{
				bool flag = false;
				ProfileDataPacket64[] profileDataPacket;
				byte[] dataBlock = PeakData.ConvertRawScanIntoPackets(instData, out profileDataPacket);
				if (WriteInstData(dataBlock, packetType))
				{
					flag = true;
					if (profileDataPacket != null)
					{
						byte[] dataBlock2 = profileDataPacket.MassSpecProfileIndexDataPktsToByteArray();
						flag = WriteInstData(dataBlock2, SpectrumPacketType.ProfileIndex);
					}
					if (flag)
					{
						flag = WriteInstScanIndex(statisticsData, instData.EventData);
					}
				}
				return flag;
			}
			catch (Exception ex)
			{
				DevErrors.UpdateError(ex);
			}
		}
		return false;
	}

	/// <summary>
	/// This method is designed for exporting mass spec scanned data to a file (mostly used by a messaging system).
	/// </summary>
	/// <param name="instData">Data, which is packed as binary "byte arrays"</param>
	/// <returns>true on success</returns>
	public bool WriteInstData(IBinaryMsInstrumentData instData)
	{
		if (instData == null)
		{
			throw new ArgumentNullException("instData");
		}
		IScanStatisticsAccess statisticsData = instData.StatisticsData;
		SpectrumPacketType packetType = (SpectrumPacketType)(statisticsData.PacketType & 0xFFFF);
		if (!CannotAcquire("Either the device writer is not ready for acquiring data or the file is in error.") && PeakData.IsSupportedPacketTypes(packetType))
		{
			try
			{
				bool flag = false;
				int profileIndexCount = instData.ProfileIndexCount;
				byte[] scanData = instData.ScanData;
				ProfileDataPacket64[] array = new ProfileDataPacket64[profileIndexCount];
				if (profileIndexCount > 0)
				{
					int num = 0;
					byte[] profileData = instData.ProfileData;
					for (int i = 0; i < array.Length; i++)
					{
						ProfileDataPacket64 profileDataPacket = new ProfileDataPacket64
						{
							DataPos = BitConverter.ToUInt32(profileData, num)
						};
						num += 4;
						profileDataPacket.LowMass = BitConverter.ToSingle(profileData, num);
						num += 4;
						profileDataPacket.HighMass = BitConverter.ToSingle(profileData, num);
						num += 4;
						profileDataPacket.MassTick = BitConverter.ToDouble(profileData, num);
						num += 8;
						profileDataPacket.DataPosOffSet = BitConverter.ToInt64(profileData, num);
						num += 8;
						array[i] = profileDataPacket;
					}
				}
				if (WriteInstData(scanData, packetType))
				{
					flag = true;
					if (profileIndexCount > 0)
					{
						byte[] dataBlock = array.MassSpecProfileIndexDataPktsToByteArray();
						flag = WriteInstData(dataBlock, SpectrumPacketType.ProfileIndex);
					}
					if (flag)
					{
						flag = WriteInstScanIndex(statisticsData, instData.PackedScanEvent);
					}
				}
				return flag;
			}
			catch (Exception ex)
			{
				DevErrors.UpdateError(ex);
			}
		}
		return false;
	}

	/// <summary>
	/// This method is designed for mass spec device data writing. <para />
	/// To provide fast data writing, this method writes the mass spec data packets directly to file (without performing <para />
	/// any data validation and data compression) by the specified packet type. <para />
	/// All data validation and data compression currently are done in the device driver code. <para />
	/// </summary>
	/// <param name="dataBlock">The binary block of data to write.</param>
	/// <param name="packetType">Type of the packet.</param>
	/// <returns>True if mass spec data packets are written to disk successfully, false otherwise.</returns>
	/// <exception cref="T:System.NotSupportedException">Not Supported Exception</exception>
	public bool WriteInstData(byte[] dataBlock, SpectrumPacketType packetType)
	{
		if (!CannotAcquire("Either the device writer is not ready for acquiring data or the file is in error."))
		{
			return WriterHelper.CritSec(delegate
			{
				bool result = false;
				int dataSize = dataBlock.Length;
				_peakData.PacketType = packetType;
				switch (packetType)
				{
				case SpectrumPacketType.LinearTrapCentroid:
					result = WriteInstDataPackets(dataBlock, dataSize, 1, updateNumPackets: true);
					break;
				case SpectrumPacketType.LinearTrapProfile:
					result = WriteInstDataPackets(dataBlock, dataSize, 1, updateNumPackets: true);
					break;
				case SpectrumPacketType.FtCentroid:
					result = WriteInstDataPackets(dataBlock, dataSize, 1, updateNumPackets: true);
					break;
				case SpectrumPacketType.FtProfile:
					result = WriteInstDataPackets(dataBlock, dataSize, 1, updateNumPackets: true);
					break;
				case SpectrumPacketType.LowResolutionSpectrum:
					result = WriteInstDataPackets(dataBlock, dataSize, _lowResDataStructSize, updateNumPackets: true);
					break;
				case SpectrumPacketType.LowResolutionSpectrumType2:
					result = WriteInstDataPackets(dataBlock, dataSize, _lowResDataStructSize, updateNumPackets: true);
					break;
				case SpectrumPacketType.HighResolutionSpectrum:
					result = WriteInstDataPackets(dataBlock, dataSize, _highResDataStructSize, updateNumPackets: true);
					break;
				case SpectrumPacketType.ProfileSpectrum:
				case SpectrumPacketType.ProfileSpectrumType2:
				case SpectrumPacketType.ProfileSpectrumType3:
				case SpectrumPacketType.LowResolutionSpectrumType3:
				case SpectrumPacketType.LowResolutionSpectrumType4:
					result = WriteInstDataPackets(dataBlock, dataSize, _profileDataPacket64Type, updateNumPackets: false);
					break;
				case SpectrumPacketType.ProfileIndex:
					result = _peakData.WriteInstIndex(dataBlock, dataSize);
					break;
				case SpectrumPacketType.CompressedAccurateSpectrum:
				case SpectrumPacketType.StandardAccurateSpectrum:
				case SpectrumPacketType.StandardUncalibratedSpectrum:
				case SpectrumPacketType.AccurateMassProfileSpectrum:
				case SpectrumPacketType.HighResolutionCompressedProfile:
				case SpectrumPacketType.LowResolutionCompressedProfile:
					throw new NotSupportedException(packetType.ToString() + " is not supported");
				}
				return result;
			}, DevErrors, _noNamePeakDataMutex);
		}
		return false;
	}

	/// <summary>
	/// This method is designed for mass spec device data writing. <para />
	/// It writes the mass spec scan index (a.k.a scan header) and trailer scan event (if it's available) to the disk.
	/// </summary>
	/// <param name="scanStatistics">Index of the mass spec scan.</param>
	/// <param name="trailerScanEvent">The trailer scan event [optional].</param>
	/// <returns>True if scan index and trailer scan event (if it's available) are written to disk successfully, false otherwise.</returns>
	public bool WriteInstScanIndex(IScanStatisticsAccess scanStatistics, IScanEvent trailerScanEvent = null)
	{
		if (!CannotAcquire("Either the device writer is not ready for acquiring data or the file is in error."))
		{
			return WriterHelper.CritSec(delegate
			{
				bool flag = true;
				if (trailerScanEvent != null)
				{
					flag = WriteTrailerScanEvent(trailerScanEvent);
					if (flag)
					{
						Interlocked.Increment(ref _currentScanIndex.TrailerOffset);
					}
				}
				if (flag)
				{
					ScanIndex scanIndex = scanStatistics.ConvertToScanIndex();
					flag = WriteInstScanIndex(scanIndex);
				}
				return flag;
			}, DevErrors, _noNamePeakDataMutex);
		}
		return false;
	}

	/// <summary>
	/// This method is designed for mass spec device data writing. <para />
	/// It writes the mass spec scan index (a.k.a scan header) and trailer scan event (if it's available) to the disk.
	/// </summary>
	/// <param name="scanStatistics">Index of the mass spec scan.</param>
	/// <param name="trailerScanEvent">The trailer scan event [optional].</param>
	/// <returns>True if scan index and trailer scan event (if it's available) are written to disk successfully, false otherwise.</returns>
	public bool WriteInstScanIndex(IScanStatisticsAccess scanStatistics, byte[] trailerScanEvent = null)
	{
		if (!CannotAcquire("Either the device writer is not ready for acquiring data or the file is in error."))
		{
			return WriterHelper.CritSec(delegate
			{
				bool flag = true;
				if (trailerScanEvent != null && trailerScanEvent.Length > 0)
				{
					flag = WriteTrailerScanEvent(trailerScanEvent);
					if (flag)
					{
						Interlocked.Increment(ref _currentScanIndex.TrailerOffset);
					}
				}
				if (flag)
				{
					ScanIndex scanIndex = scanStatistics.ConvertToScanIndex();
					flag = WriteInstScanIndex(scanIndex);
				}
				return flag;
			}, DevErrors, _noNamePeakDataMutex);
		}
		return false;
	}

	private bool WriteTrailerScanEvent(byte[] trailerScanEvent)
	{
		if (!CannotAcquire("Either the device writer is not ready for acquiring data or the file is in error."))
		{
			return WriterHelper.CritSec(delegate(DeviceErrors err)
			{
				bool result = false;
				_trailerScanEventWriter.Write(trailerScanEvent);
				_trailerScanEventWriter.Flush();
				if (RunHeaderExtension.SaveNumTrailerScanEvents(base.RunHeaderMemMapAccessor, base.DeviceRunHeader.IncrementTrailerScanEvents(), err))
				{
					_trailerScanEventsBufferInfo.IncrementNumElements();
					result = true;
				}
				return result;
			}, DevErrors, _noNameScanEventsMutex);
		}
		return false;
	}

	/// <summary>
	/// If any trailer extra details are to be written to the raw data file
	/// then the format this data will take must be written to the file while
	/// setting up i.e. prior to acquiring any data.<para />
	/// The order and types of the data elements in the object array parameter
	/// to the method needs to be the same as the order and types that are defined in the header.
	/// </summary>
	/// <param name="data">The trailer extra data stores in object array.</param>
	/// <returns> True if trailer extra data is written to disk successfully, False otherwise. </returns>
	public bool WriteTrailerExtraData(object[] data)
	{
		if (!CannotAcquire("Either the device writer is not ready for acquiring data or the file is in error."))
		{
			return WriterHelper.CritSec(delegate(DeviceErrors errors)
			{
				_trailerExtraHeader.ConvertDataEntryToByteArray(data, out var buffer);
				bool flag = WriteGenericData(base.DataStreamWriters[8], buffer, _trailerExtraHeader.TotalDataSize, errors);
				if (flag && data.IsAny())
				{
					flag = RunHeaderExtension.SaveNumTrailerExtra(base.RunHeaderMemMapAccessor, base.DeviceRunHeader.IncrementNumTrailerExtra(), errors);
					if (flag)
					{
						_trailerExtraBufferInfo.IncrementNumElements();
					}
				}
				return flag;
			}, DevErrors, _noNameTrailerExtraDataMutex);
		}
		return false;
	}

	/// <summary>
	/// If any Trailer Extra details are to be written to the raw data file
	/// then the format this data will take must be written to the file while
	/// setting up i.e. prior to acquiring any data.<para />
	/// The order and types of the data elements in the byte array parameter
	/// to the method needs to be the same as the order and types that are defined in the header.
	/// </summary>
	/// <param name="data">The trailer extra data stores in byte array.</param>
	/// <returns>True if trailer extra entry is written to disk successfully, False otherwise </returns>
	public bool WriteTrailerExtraData(byte[] data)
	{
		if (!CannotAcquire("Either the device writer is not ready for acquiring data or the file is in error."))
		{
			return WriterHelper.CritSec(delegate(DeviceErrors errors)
			{
				byte[] array = new byte[data.Length];
				Buffer.BlockCopy(data, 0, array, 0, data.Length);
				bool flag = WriteGenericData(base.DataStreamWriters[8], array, _trailerExtraHeader.TotalDataSize, errors);
				if (flag && data.IsAny())
				{
					flag = RunHeaderExtension.SaveNumTrailerExtra(base.RunHeaderMemMapAccessor, base.DeviceRunHeader.IncrementNumTrailerExtra(), errors);
					if (flag)
					{
						_trailerExtraBufferInfo.IncrementNumElements();
					}
				}
				return flag;
			}, DevErrors, _noNameTrailerExtraDataMutex);
		}
		return false;
	}

	/// <summary>
	/// If any tune details are to be written to the raw data file
	/// then the format this data will take must be written to the file while
	/// setting up i.e. prior to acquiring any data.<para />
	/// The order and types of the data elements in the object array parameter
	/// to the method needs to be the same as the order and types that are defined in the header.
	/// </summary>
	/// <param name="data">The tune data stores in object array.</param>
	/// <returns>True if tune data is written to disk successfully, False otherwise.</returns>
	public bool WriteTuneData(object[] data)
	{
		if (!CannotAcquire("Either the device writer is not ready for acquiring data or the file is in error."))
		{
			return WriterHelper.CritSec(delegate(DeviceErrors errors)
			{
				_tuneHeader.ConvertDataEntryToByteArray(data, out var buffer);
				bool flag = WriteGenericData(base.DataStreamWriters[10], buffer, _tuneHeader.TotalDataSize, errors);
				if (flag && data.IsAny())
				{
					flag = RunHeaderExtension.SaveNumTuneData(base.RunHeaderMemMapAccessor, base.DeviceRunHeader.IncrementNumTuneData(), errors);
					if (flag)
					{
						_tuneDataBufferInfo.IncrementNumElements();
					}
				}
				return flag;
			}, DevErrors, _noNameTuneDataMutex);
		}
		return false;
	}

	/// <summary>
	/// If any tune data details are to be written to the raw data file
	/// then the format this data will take must be written to the file while
	/// setting up i.e. prior to acquiring any data.<para />
	/// The order and types of the data elements in the byte array parameter
	/// to the method needs to be the same as the order and types that are defined in the header.
	/// </summary>
	/// <param name="data">The tune data stores in byte array.</param>
	/// <returns>True if tune data entry is written to disk successfully, False otherwise.</returns>
	public bool WriteTuneData(byte[] data)
	{
		if (!CannotAcquire("Either the device writer is not ready for acquiring data or the file is in error."))
		{
			return WriterHelper.CritSec(delegate(DeviceErrors errors)
			{
				byte[] array = new byte[data.Length];
				Buffer.BlockCopy(data, 0, array, 0, data.Length);
				bool flag = WriteGenericData(base.DataStreamWriters[10], array, _tuneHeader.TotalDataSize, errors);
				if (flag && data.IsAny())
				{
					flag = RunHeaderExtension.SaveNumTuneData(base.RunHeaderMemMapAccessor, base.DeviceRunHeader.IncrementNumTuneData(), errors);
					if (flag)
					{
						_tuneDataBufferInfo.IncrementNumElements();
					}
				}
				return flag;
			}, DevErrors, _noNameTuneDataMutex);
		}
		return false;
	}

	/// <summary>
	/// Writes the generic data.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="data">The data.</param>
	/// <param name="headerSize">Size of the header.</param>
	/// <param name="errors">The error object which stores errors information.</param>
	/// <returns>True if generic data (trailer extra data and tune data) is written to disk; false otherwise.</returns>
	private static bool WriteGenericData(BinaryWriter writer, byte[] data, uint headerSize, DeviceErrors errors)
	{
		bool result = false;
		if (headerSize == data.Length)
		{
			result = data.Length == 0 || writer.SaveGenericDataItem(data, errors);
		}
		else
		{
			errors.UpdateError($"Header size:{headerSize} and input data size:{data.Length} not matching.");
		}
		return result;
	}

	/// <summary>
	/// Writes the run header information.
	/// </summary>
	/// <param name="deviceAcquireStatus">The device acquire status.</param>
	/// <param name="errors">The errors object which stores errors information.</param>
	/// <param name="writtenFlag">The written flag.</param>
	/// <param name="method">The method.</param>
	/// <returns> True if the run header information is written to disk successfully, False otherwise. </returns>
	private static bool WriteRunHeaderInformation(DeviceAcquireStatus deviceAcquireStatus, DeviceErrors errors, ref int writtenFlag, Func<DeviceErrors, bool> method)
	{
		bool result = false;
		if (!errors.HasError)
		{
			if (deviceAcquireStatus.CanDeviceBeSetup && Interlocked.CompareExchange(ref writtenFlag, 1, 0) == 0)
			{
				result = method(errors);
			}
			else
			{
				errors.UpdateError("Either the device writer is not ready or the data has already been written to the file.");
			}
		}
		return result;
	}

	/// <summary>
	/// Attaches the mass spec packet stream to raw file.
	/// </summary>
	/// <param name="rawFileName">Name of the in-acquisition raw file.</param>
	/// <param name="runHeaderStruct">The mass spec device run header structure.</param>
	/// <returns>True if mass spec packet stream is attached to in-acquisition raw file; false otherwise. </returns>
	private bool AttachMassSpecPacketStreamToRawFile(string rawFileName, ref RunHeaderStruct runHeaderStruct)
	{
		FileStream fileStream = new FileStream(rawFileName, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
		runHeaderStruct.DataPktFile = rawFileName;
		fileStream.Seek(0L, SeekOrigin.End);
		base.DataStreamWriters[4] = new BinaryWriter(fileStream, Encoding.Unicode, leaveOpen: false);
		return true;
	}

	/// <summary>
	/// Initializes the buffer information shared memory objects.
	/// </summary>
	private void InitializeBufferInfo()
	{
		_peakDataBufferInfo = new BufferInfo(DeviceId, base.DataFileMapName, "PEAKDATA", creatable: true).UpdateErrors(DevErrors);
		_scanIndexBufferInfo = new BufferInfo(DeviceId, base.DataFileMapName, "SCANHEADER", creatable: true, Utilities.StructSizeLookup.Value[19]).UpdateErrors(DevErrors);
		_scanEventsBufferInfo = new BufferInfo(DeviceId, base.DataFileMapName, "SCANEVENTS", creatable: true, -1).UpdateErrors(DevErrors);
		_trailerScanEventsBufferInfo = new BufferInfo(DeviceId, base.DataFileMapName, "TRAILER_EVENTS", creatable: true, -1).UpdateErrors(DevErrors);
		_tuneHeaderBufferInfo = new BufferInfo(DeviceId, base.DataFileMapName, "TUNEDATAHEADER", creatable: true).UpdateErrors(DevErrors);
		_tuneDataBufferInfo = new BufferInfo(DeviceId, base.DataFileMapName, "TUNEDATA_FILEMAP", creatable: true).UpdateErrors(DevErrors);
		_trailerExtraHeaderBufferInfo = new BufferInfo(DeviceId, base.DataFileMapName, "TRAILERHEADER", creatable: true).UpdateErrors(DevErrors);
		_trailerExtraBufferInfo = new BufferInfo(DeviceId, base.DataFileMapName, "TRAILEREXTRA", creatable: true).UpdateErrors(DevErrors);
	}

	/// <summary>
	/// Initializes the temporary files.
	/// </summary>
	/// <param name="runHeaderStruct">The run header structure.</param>
	/// <param name="rawFileName">Name of the raw file.</param>
	/// <param name="virtualDeviceTypes">Device type</param>
	/// <param name="registeredIndex">Device registered index</param>
	/// <returns>True if all temporary files are created successfully; false otherwise.</returns>
	private bool InitializeTemporaryFiles(ref RunHeaderStruct runHeaderStruct, string rawFileName, VirtualDeviceTypes virtualDeviceTypes, int registeredIndex)
	{
		string text = Utilities.BuildUniqueVirtualDeviceStreamTempFileName(virtualDeviceTypes, registeredIndex);
		if (TempFileHelper.CreateTempFile(rawFileName, out base.DataStreamWriters[5], out runHeaderStruct.SpectrumFile, text + "SPECTRUM") && TempFileHelper.CreateTempFile(rawFileName, out base.DataStreamWriters[6], out runHeaderStruct.ScanEventsFile, text + "SCANEVENTS", addZeroValueLengthField: true) && TempFileHelper.CreateTempFile(rawFileName, out base.DataStreamWriters[9], out runHeaderStruct.TuneDataHeaderFile, text + "TUNEDATAHEADER") && TempFileHelper.CreateTempFile(rawFileName, out base.DataStreamWriters[10], out runHeaderStruct.TuneDataFile, text + "TUNEDATA_FILEMAP") && TempFileHelper.CreateTempFile(rawFileName, out base.DataStreamWriters[7], out runHeaderStruct.TrailerHeaderFile, text + "TRAILERHEADER") && TempFileHelper.CreateTempFile(rawFileName, out base.DataStreamWriters[8], out runHeaderStruct.TrailerExtraFile, text + "TRAILEREXTRA"))
		{
			return TempFileHelper.CreateTempFile(rawFileName, out base.DataStreamWriters[11], out runHeaderStruct.TrailerScanEventsFile, text + "TRAILER_EVENTS", addZeroValueLengthField: true);
		}
		return false;
	}

	/// <summary>
	/// Writes the instrument data packets to the file.
	/// </summary>
	/// <param name="data">The block of data to write.</param>
	/// <param name="dataSize">Size of the data block.</param>
	/// <param name="packetSize">Size of each packet in the data block.</param>
	/// <param name="updateNumPackets">if set to true to update the number of packets; false to not update.</param>
	/// <returns>True if packets are written to disk successfully, false otherwise.</returns>
	private bool WriteInstDataPackets(byte[] data, int dataSize, int packetSize, bool updateNumPackets)
	{
		bool num = _peakData.WriteInstData(_scanDataWriter, data, DevErrors);
		_currentScanIndex.DataSize = (uint)data.Length;
		if (num && updateNumPackets)
		{
			_currentScanIndex.NumberPackets = dataSize / packetSize;
		}
		return num;
	}

	/// <summary>
	/// Writes the mass resolution for the mass spec device to file.<para />
	/// This field can be set only once. The default mass resolution value is 0.5<para />
	/// </summary>
	/// <param name="halfPeakWidth">Width of the half peak.</param>
	/// <returns> True if mass resolution is written to disk successfully, False otherwise </returns>
	private bool WriteInstMassResolution(double halfPeakWidth)
	{
		return WriteRunHeaderInformation(DeviceAcqStatus, DevErrors, ref _writtenMassResolution, (DeviceErrors errors) => base.DeviceRunHeader.SaveMassResolution(base.RunHeaderMemMapAccessor, halfPeakWidth, errors));
	}

	/// <summary>
	/// Writes the mass spec scan index (header) to disk
	/// </summary>
	/// <param name="scanIndex">Index of the mass spec scan.</param>
	/// <returns>True if the scan index is written to disk successfully, false otherwise.</returns>
	private bool WriteInstScanIndex(ScanIndex scanIndex)
	{
		bool result = WriterHelper.TryCatch(delegate(DeviceErrors errors)
		{
			ScanIndexStruct scanIndexStructInfo = scanIndex.ScanIndexStructInfo;
			bool result2 = false;
			if (_peakData.PacketType == SpectrumPacketType.ProfileIndex)
			{
				_currentScanIndex.NumberPackets += _peakData.WriteIndices(_scanDataWriter);
			}
			scanIndexStructInfo.DataOffset = _peakData.CurrentStartOfData - _dataInitialOffset;
			scanIndexStructInfo.NumberPackets = _currentScanIndex.NumberPackets;
			long num = _peakData.NextStartOfData - _peakData.CurrentStartOfData;
			scanIndexStructInfo.DataSize = (uint)num;
			_peakData.CurrentStartOfData = _peakData.NextStartOfData;
			scanIndexStructInfo.ScanNumber = base.DeviceRunHeader.NumSpectra + 1;
			scanIndexStructInfo.TrailerOffset = _currentScanIndex.TrailerOffset - 1;
			if (scanIndexStructInfo.Save(_scanIndexWriter, errors))
			{
				base.DeviceRunHeader.LowMass = Math.Min(base.DeviceRunHeader.LowMass, scanIndexStructInfo.LowMass);
				base.DeviceRunHeader.HighMass = Math.Max(base.DeviceRunHeader.HighMass, scanIndexStructInfo.HighMass);
				base.DeviceRunHeader.MaxIntegratedIntensity = Math.Max(base.DeviceRunHeader.MaxIntegratedIntensity, scanIndexStructInfo.TIC);
				base.DeviceRunHeader.StartTime = Math.Min(base.DeviceRunHeader.StartTime, scanIndexStructInfo.StartTime);
				base.DeviceRunHeader.EndTime = Math.Max(base.DeviceRunHeader.EndTime, scanIndexStructInfo.StartTime);
				if (base.DeviceRunHeader.SaveRunHeader(base.RunHeaderMemMapAccessor, errors))
				{
					base.DeviceRunHeader.NumSpectra = scanIndexStructInfo.ScanNumber;
					if (RunHeaderExtension.SaveScanNum(base.RunHeaderMemMapAccessor, base.DeviceRunHeader.NumSpectra, errors))
					{
						_scanIndexBufferInfo.IncrementNumElements();
						result2 = true;
					}
				}
			}
			return result2;
		}, DevErrors);
		_currentScanIndex.NumberPackets = 0;
		return result;
	}

	/// <summary>
	/// Writes the mass spec scan events table.
	/// </summary>
	/// <param name="massSpecScanEvents">The mass spec scan events table.</param>
	/// <returns>
	/// True if mass scan events table is written to disk successfully; false otherwise.
	/// </returns>
	/// <exception cref="T:System.ArgumentNullException">mass Spec Scan Events</exception>
	private bool WriteMethodScanEvents(IScanEvents massSpecScanEvents)
	{
		if (!CannotAcquire("Either the device writer is not ready for acquiring data or the file is in error."))
		{
			return WriterHelper.CritSec(delegate
			{
				BinaryWriter writer = base.DataStreamWriters[6];
				return SaveScanEvents(massSpecScanEvents, writer, DevErrors);
			}, DevErrors, _noNameScanEventsMutex);
		}
		return false;
	}

	/// <summary>
	/// Writes the mass spec scan events table.
	/// </summary>
	/// <param name="massSpecScanEvents">The mass spec scan events table.</param>
	/// <returns>
	/// True if mass scan events table is written to disk successfully; false otherwise.
	/// </returns>
	/// <exception cref="T:System.ArgumentNullException">mass Spec Scan Events</exception>
	private bool WriteMethodScanEvents(byte[] massSpecScanEvents)
	{
		if (!CannotAcquire("Either the device writer is not ready for acquiring data or the file is in error."))
		{
			return WriterHelper.CritSec(delegate
			{
				BinaryWriter obj = base.DataStreamWriters[6];
				obj.Seek(0, SeekOrigin.Begin);
				obj.Write(massSpecScanEvents);
				obj.Flush();
				return true;
			}, DevErrors, _noNameScanEventsMutex);
		}
		return false;
	}

	/// <summary>
	/// Write scan events to a stream
	/// </summary>
	/// <param name="massSpecScanEvents">events to encode</param>
	/// <param name="writer">stream to save bytes</param>
	/// <param name="devErrors">error messages</param>
	/// <returns></returns>
	internal static bool SaveScanEvents(IScanEvents massSpecScanEvents, BinaryWriter writer, DeviceErrors devErrors)
	{
		bool flag = true;
		writer.Seek(0, SeekOrigin.Begin);
		int segments = massSpecScanEvents.Segments;
		writer.Write(segments);
		for (int i = 0; i < segments && flag; i++)
		{
			int eventCount = massSpecScanEvents.GetEventCount(i);
			writer.Write(eventCount);
			for (int j = 0; j < eventCount && flag; j++)
			{
				flag = massSpecScanEvents.GetEvent(i, j).SaveScanEvent(writer, devErrors);
			}
		}
		writer.BaseStream.SetLength(writer.BaseStream.Position);
		writer.Flush();
		return flag;
	}

	/// <summary>
	/// Writers the filter mass precision for the mass spec device to file.<para />
	/// This field can be set only once. The default precision value is 2.
	/// </summary>
	/// <param name="precision">The precision.</param>
	/// <returns>
	/// True if filter mass precision is written to disk successfully, False otherwise
	/// </returns>
	private bool WriterInstFilterMassPrecision(int precision)
	{
		return WriteRunHeaderInformation(DeviceAcqStatus, DevErrors, ref _writtenFilterMassPrecision, (DeviceErrors errors) => base.DeviceRunHeader.SaveFilterMassPrecision(base.RunHeaderMemMapAccessor, precision, errors));
	}

	/// <summary>
	/// Write the Trailer Extra Header (format) info to the raw data file. <para />
	/// If caller is not intended to use the trailer extra data, pass a null argument or zero length array.<para />
	/// ex. WriteTrailerExtraHeader(null) or WriteTrailerExtraHeader(new IHeaderItem[0])
	/// </summary>
	/// <param name="headerItems">The trailer extra header.</param>
	/// <returns>
	/// True if trailer extra header is written to disk successfully, False otherwise
	/// </returns>
	private bool WriteTrailerExtraHeader(IHeaderItem[] headerItems)
	{
		if (!base.HasError)
		{
			return WriteGenericHeader(headerItems, GenericHeaderWrittenFlag.WrittenTrailerExtraHeader, DataStreamType.TrailerExtraHeaderFile, ref _trailerExtraHeaderBufferInfo, ref _trailerExtraBufferInfo, out _trailerExtraHeader);
		}
		return false;
	}

	/// <summary>
	/// Writes the trailer scan event to disk.
	/// </summary>
	/// <param name="trailerScanEvent">The trailer scan event.</param>
	/// <returns>True if trailer scan event is written to disk, false otherwise.</returns>
	private bool WriteTrailerScanEvent(IScanEvent trailerScanEvent)
	{
		if (!CannotAcquire("Either the device writer is not ready for acquiring data or the file is in error."))
		{
			return WriterHelper.CritSec(delegate(DeviceErrors err)
			{
				bool result = false;
				if (trailerScanEvent.SaveScanEvent(_trailerScanEventWriter, err) && RunHeaderExtension.SaveNumTrailerScanEvents(base.RunHeaderMemMapAccessor, base.DeviceRunHeader.IncrementTrailerScanEvents(), err))
				{
					_trailerScanEventsBufferInfo.IncrementNumElements();
					result = true;
				}
				return result;
			}, DevErrors, _noNameScanEventsMutex);
		}
		return false;
	}

	/// <summary>
	/// Write the Tune Header (format) info to the raw data file. <para />
	/// If caller is not intended to use the tune data, pass a null argument or zero length array.<para />
	/// ex. WriteTuneHeader(null) or WriteTuneHeader(new IHeaderItem[0])
	/// </summary>
	/// <param name="headerItems">The tune header.</param>
	/// <returns>
	/// True if tune header is written to disk successfully, False otherwise
	/// </returns>
	private bool WriteTuneHeader(IHeaderItem[] headerItems)
	{
		if (!base.HasError)
		{
			return WriteGenericHeader(headerItems, GenericHeaderWrittenFlag.WrittenTuneHeader, DataStreamType.TuneDataHeaderFile, ref _tuneHeaderBufferInfo, ref _tuneDataBufferInfo, out _tuneHeader);
		}
		return false;
	}
}
