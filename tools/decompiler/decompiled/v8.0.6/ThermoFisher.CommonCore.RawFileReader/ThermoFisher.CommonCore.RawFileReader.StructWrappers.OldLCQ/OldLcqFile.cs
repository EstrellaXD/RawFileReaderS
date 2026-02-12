using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.RunHeaders;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

/// <summary>
/// The old LCQ file.
/// </summary>
internal sealed class OldLcqFile : IDisposable
{
	private readonly IRawFileLoader _rawFileLoader;

	private readonly IReadWriteAccessor[] _oldLcqDataAccessors;

	private bool _disposed;

	public IViewCollectionManager Manager { get; }

	/// <summary>
	/// Gets the audit trail information.
	/// </summary>
	public AuditTrail AuditTrailInfo { get; private set; }

	/// <summary>
	/// Prevents a default instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ.OldLcqFile" /> class from being created.
	/// </summary>
	private OldLcqFile()
	{
		_disposed = false;
		AuditTrailInfo = new AuditTrail();
		_oldLcqDataAccessors = new IReadWriteAccessor[6];
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ.OldLcqFile" /> class.
	/// </summary>
	/// <param name="manager"></param>
	/// <param name="rawFileLoader">The raw file loader.</param>
	/// <exception cref="T:System.ArgumentNullException">raw File Loader</exception>
	public OldLcqFile(IViewCollectionManager manager, IRawFileLoader rawFileLoader)
		: this()
	{
		if (rawFileLoader == null)
		{
			throw new ArgumentNullException("rawFileLoader");
		}
		if (manager == null)
		{
			throw new ArgumentNullException("manager");
		}
		Manager = manager;
		_rawFileLoader = rawFileLoader;
	}

	/// <summary>
	/// Decodes the old LCQ file.
	/// This object is being converted or read from disk and therefore the data needs
	/// needs to read from either the mapped file or some other mechanism. This function
	/// is only intended for use with old LCQ data files.        
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <returns>True able to decode the old LCQ file, false otherwise</returns>
	/// <exception cref="T:System.NotImplementedException">This is an old file type, the file version is less than 25</exception>
	public bool DecodeOldLcqFile(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		InstrumentFile instrumentFile = null;
		IViewCollectionManager instance = Manager;
		RunHeader lcqRunHeader = viewer.LoadRawFileObjectExt(() => new RunHeader(instance, _rawFileLoader.Id), fileRevision, ref startPos);
		_rawFileLoader.Sequence = viewer.LoadRawFileObjectExt(() => new SequenceRow(), fileRevision, ref startPos);
		IRawFileInfo rawFileInfo = (_rawFileLoader.RawFileInformation = viewer.LoadRawFileObjectExt(() => new RawFileInfo(instance, _rawFileLoader.Id, _rawFileLoader.RawFileName, fileRevision), fileRevision, ref startPos));
		IRawFileInfo rawFileInfo3 = rawFileInfo;
		bool hasExpMethod = rawFileInfo3.HasExpMethod;
		viewer.LoadRawFileObjectExt(() => new IcisStatusLog(), fileRevision, ref startPos);
		if (hasExpMethod)
		{
			instrumentFile = viewer.LoadRawFileObjectExt(() => new InstrumentFile(), fileRevision, ref startPos);
			if (instrumentFile?.AuditTrailInfo != null)
			{
				AuditTrailInfo = instrumentFile.AuditTrailInfo;
			}
		}
		TuneData[] array = viewer.LoadRawFileObjectArray<TuneData>(fileRevision, ref startPos);
		long num = startPos;
		startPos += startPos;
		long startPos2 = lcqRunHeader.SpectrumPos;
		LcqScanHeader lcqScanHeader = viewer.LoadRawFileObjectExt(() => new LcqScanHeader(lcqRunHeader.NumSpectra), fileRevision, ref startPos2);
		startPos += lcqRunHeader.SpectrumPos;
		startPos2 = lcqRunHeader.StatusLogPos;
		InstrumentStatusLog instrumentStatusLog = viewer.LoadRawFileObjectExt(() => new InstrumentStatusLog(lcqRunHeader.NumStatusLog), fileRevision, ref startPos2);
		startPos += lcqRunHeader.StatusLogPos;
		int massSpecAnalogChannelsUsedCount = 0;
		byte massSpecAnalogChannelsUsed = 0;
		massSpecAnalogChannelsUsedCount = GetMsAnalogChannelsUsedCount(hasExpMethod, instrumentFile, massSpecAnalogChannelsUsedCount, ref massSpecAnalogChannelsUsed);
		rawFileInfo3.UpdateVirtualController(1, 0L, startPos, 0, VirtualDeviceTypes.MsDevice);
		((RawFileInfo)rawFileInfo3).IsInAcquisition = false;
		_rawFileLoader.RefreshDevices(rawFileInfo3.NumberOfVirtualControllers);
		DeviceContainer[] devices = _rawFileLoader.Devices;
		MassSpecDevice massSpecDevice = null;
		if (devices != null && devices.Any() && devices[0].FullDevice.Value.DeviceType == VirtualDeviceTypes.MsDevice)
		{
			massSpecDevice = devices[0].FullDevice.Value as MassSpecDevice;
		}
		if (massSpecDevice != null)
		{
			IRunHeader runHeader = massSpecDevice.UpdateRunHeaderStruct(lcqRunHeader);
			string text = _rawFileLoader.DataFileMapName + "_MS_0_";
			int numSpectra = runHeader.NumSpectra;
			DataDescriptors dataDescriptors = LcqConverter.LcqCreateXcalTrailerExtraHeader();
			bool flag = dataDescriptors?.Any() ?? false;
			int num2 = dataDescriptors?.CalcBufferSize() ?? 0;
			int num3 = num2 * numSpectra;
			_oldLcqDataAccessors[2] = instance.GetRandomAccessViewer(_rawFileLoader.Id, text + "TRAILEREXTRA_DATA_READWRITE_INFO", 0L, num3, inAcquisition: false, DataFileAccessMode.OpenCreateReadWriteLoaderId, PersistenceMode.NonPersisted);
			int num4 = Marshal.SizeOf(typeof(ScanIndexStruct1));
			int num5 = num4 * numSpectra;
			_oldLcqDataAccessors[0] = instance.GetRandomAccessViewer(_rawFileLoader.Id, text + "SCANINDEX_DATA_READWRITE_INFO", 0L, num5, inAcquisition: false, DataFileAccessMode.OpenCreateReadWriteLoaderId, PersistenceMode.NonPersisted);
			ScanEvent[] array2 = new ScanEvent[numSpectra];
			for (int num6 = 0; num6 < numSpectra; num6++)
			{
				TrailerStruct trailerInfo = lcqScanHeader.TrailerStructInfo[num6];
				LcqConverter.LcqWriteXcalScanIndex(_oldLcqDataAccessors[0], ref trailerInfo, num6, hasExpMethod, num4 * num6);
				array2[num6] = LcqConverter.LcqCreateXcalTrailerScanEvent(runHeader.FilterMassPrecision, ref trailerInfo, num6);
				if (flag)
				{
					LcqConverter.LcqWriteXcalTrailerExtra(_oldLcqDataAccessors[2], ref trailerInfo, num2 * num6);
				}
				if ((massSpecAnalogChannelsUsed & 1) <= 0 && (double)trailerInfo.UVAnalogInput[0] >= 3.0 / 256.0)
				{
					massSpecAnalogChannelsUsedCount++;
					massSpecAnalogChannelsUsed++;
				}
				if ((massSpecAnalogChannelsUsed & 2) <= 0 && (double)trailerInfo.UVAnalogInput[1] >= 3.0 / 256.0)
				{
					massSpecAnalogChannelsUsedCount++;
					massSpecAnalogChannelsUsed += 2;
				}
				if ((massSpecAnalogChannelsUsed & 4) <= 0 && (double)trailerInfo.UVAnalogInput[2] >= 3.0 / 256.0)
				{
					massSpecAnalogChannelsUsedCount++;
					massSpecAnalogChannelsUsed += 4;
				}
				if ((massSpecAnalogChannelsUsed & 8) <= 0 && (double)trailerInfo.UVAnalogInput[3] >= 3.0 / 256.0)
				{
					massSpecAnalogChannelsUsedCount++;
					massSpecAnalogChannelsUsed += 8;
				}
			}
			massSpecDevice.ReadScanIndices(_oldLcqDataAccessors[0], 0L, fileRevision);
			massSpecDevice.TrailerExtras = new GenericDataCollection(instance, _rawFileLoader.Id, dataDescriptors);
			massSpecDevice.TrailerExtras.LoadGenericDataEntries(_oldLcqDataAccessors[2], 0L, numSpectra);
			RunHeader obj = (RunHeader)runHeader;
			int numTrailerExtra = (((RunHeader)runHeader).NumTrailerScanEvents = numSpectra);
			obj.NumTrailerExtra = numTrailerExtra;
			massSpecDevice.TrailerScanEvents = new TrailerScanEvents(instance, _rawFileLoader.Id, runHeader);
			massSpecDevice.TrailerScanEvents.Load(array2);
			if (hasExpMethod)
			{
				ConstructScanEventSegments(instrumentFile, array, massSpecDevice);
			}
			else
			{
				massSpecDevice.ScanEventSegments = new List<List<ScanEvent>>(0);
			}
			if (array.Any())
			{
				DataDescriptors dataDescriptors2 = LcqConverter.LcqCreateXcalTuneMethodHeader();
				int num8 = dataDescriptors2.CalcBufferSize();
				int num9 = array.Length;
				int num10 = num8 * num9;
				_oldLcqDataAccessors[3] = instance.GetRandomAccessViewer(_rawFileLoader.Id, text + "TUNEDATA_DATA_READWRITE_INFO", 0L, num10, inAcquisition: false, DataFileAccessMode.OpenCreateReadWriteLoaderId, PersistenceMode.NonPersisted);
				for (int num11 = 0; num11 < num9; num11++)
				{
					TuneDataStruct tuneDatastruct = array[num11].TuneDataStructInfo;
					LcqConverter.LcqWriteXcalTuneData(_oldLcqDataAccessors[3], ref tuneDatastruct, num8 * num11);
				}
				massSpecDevice.TuneData = new GenericDataCollection(instance, _rawFileLoader.Id, dataDescriptors2);
				massSpecDevice.TuneData.LoadGenericDataEntries(_oldLcqDataAccessors[3], 0L, num9);
				((RunHeader)runHeader).NumTuneData = num9;
			}
			if (instrumentStatusLog != null && instrumentStatusLog.InstStatusStructInfo.Any())
			{
				InstStatusStruct[] instStatusStructInfo = instrumentStatusLog.InstStatusStructInfo;
				bool incAs = instStatusStructInfo[0].AutoSamplerStatus.Status != 0;
				bool incLc = instStatusStructInfo[0].LcStatus.Status != 0;
				DataDescriptors dataDescriptors3 = LcqConverter.LcqCreateXcalStatusLogHeader(incAs, incLc);
				int numStatusLog = runHeader.NumStatusLog;
				int num12 = dataDescriptors3.CalcBufferSize() + 4;
				int num13 = num12 * numStatusLog;
				_oldLcqDataAccessors[1] = instance.GetRandomAccessViewer(_rawFileLoader.Id, text + "STATUSLOGS_DATA_READWRITE_INFO", 0L, num13, inAcquisition: false, DataFileAccessMode.OpenCreateReadWriteLoaderId, PersistenceMode.NonPersisted);
				for (int num14 = 0; num14 < numStatusLog; num14++)
				{
					LcqConverter.LcqWriteXcalStatusLog(_oldLcqDataAccessors[1], ref instStatusStructInfo[num14], incAs, incLc, num12, num12 * num14);
				}
				massSpecDevice.StatusLogEntries = new StatusLog(instance, _rawFileLoader.Id, runHeader);
				((StatusLog)massSpecDevice.StatusLogEntries).DataDescriptors = dataDescriptors3;
				((StatusLog)massSpecDevice.StatusLogEntries).LoadStatusLogEntries(_oldLcqDataAccessors[1], 0L, fileRevision);
			}
			IReadWriteAccessor readWriteAccessor = null;
			if (viewer.SupportsSubViews)
			{
				long blobSize = lcqRunHeader.SpectrumPos - num;
				readWriteAccessor = viewer.CreateSubView(num, blobSize);
			}
			if (readWriteAccessor == null)
			{
				readWriteAccessor = instance.GetRandomAccessViewer(_rawFileLoader.Id, _rawFileLoader.RawFileName, num, 0L, inAcquisition: false);
			}
			massSpecDevice.PacketViewer = readWriteAccessor;
		}
		if (massSpecAnalogChannelsUsedCount > 0)
		{
			string text2 = _rawFileLoader.DataFileMapName + "_UV_1_";
			rawFileInfo3.UpdateVirtualController(2, 0L, 0L, 1, VirtualDeviceTypes.MsAnalogDevice);
			_rawFileLoader.RefreshDevices(rawFileInfo3.NumberOfVirtualControllers);
			devices = _rawFileLoader.Devices;
			UvDevice uvDevice = null;
			if (devices != null && devices.Length > 1)
			{
				DeviceContainer deviceContainer = devices[1];
				if (deviceContainer != null && deviceContainer.FullDevice.Value.DeviceType == VirtualDeviceTypes.MsAnalogDevice)
				{
					uvDevice = deviceContainer.FullDevice.Value as UvDevice;
				}
			}
			if (uvDevice == null)
			{
				return false;
			}
			VirtualControllerInfoStruct controllerInfo = ((RunHeader)uvDevice.RunHeader).RunHeaderStruct.ControllerInfo;
			uvDevice.RunHeader.Copy(lcqRunHeader);
			RunHeaderStruct runHeaderStruct = uvDevice.RunHeader.RunHeaderStruct;
			runHeaderStruct.NumStatusLog = 0;
			runHeaderStruct.NumErrorLog = 0;
			runHeaderStruct.MaxIntegIntensity = 0.0;
			runHeaderStruct.LowMass = 0.0;
			runHeaderStruct.HighMass = 0.0;
			runHeaderStruct.MaxIntensity = 0;
			runHeaderStruct.RunHeaderPos = 0L;
			runHeaderStruct.NumTuneData = 0;
			runHeaderStruct.NumTrailerExtra = 0;
			runHeaderStruct.TrailerScanEventsPos = 0L;
			runHeaderStruct.SpectPos = 0L;
			runHeaderStruct.SpectPos32Bit = 0;
			runHeaderStruct.ControllerInfo = controllerInfo;
			((RunHeader)uvDevice.RunHeader).CopyRunHeaderStruct(ref runHeaderStruct);
			int numSpectra2 = uvDevice.RunHeader.NumSpectra;
			int num15 = massSpecAnalogChannelsUsedCount * 8;
			int num16 = numSpectra2 * num15;
			_oldLcqDataAccessors[5] = instance.GetRandomAccessViewer(_rawFileLoader.Id, text2 + "UVPEAKDATA_DATA_READWRITE_INFO", 0L, num16, inAcquisition: false, DataFileAccessMode.OpenCreateReadWriteLoaderId, PersistenceMode.NonPersisted);
			int num17 = Marshal.SizeOf(typeof(UvScanIndexStructOld));
			int num18 = num17 * numSpectra2;
			_oldLcqDataAccessors[4] = instance.GetRandomAccessViewer(_rawFileLoader.Id, text2 + "UVSCANINDEX_DATA_READWRITE_INFO", 0L, num18, inAcquisition: false, DataFileAccessMode.OpenCreateReadWriteLoaderId, PersistenceMode.NonPersisted);
			for (int num19 = 0; num19 < numSpectra2; num19++)
			{
				TrailerStruct trailerInfo2 = lcqScanHeader.TrailerStructInfo[num19];
				LcqConverter.LcqWriteXcalUvScanIndex(_oldLcqDataAccessors[4], ref trailerInfo2, massSpecAnalogChannelsUsedCount, num19, num17 * num19);
				LcqConverter.LcqWriteXcalUvChannelData(_oldLcqDataAccessors[5], trailerInfo2.UVAnalogInput, massSpecAnalogChannelsUsed, num19 * num15);
			}
			uvDevice.Load(_oldLcqDataAccessors[4], 0L, rawFileInfo3.FileRevision);
			uvDevice.UvPacketViewer = _oldLcqDataAccessors[5];
		}
		return true;
	}

	/// <summary>
	/// Constructs the scan event segments.
	/// </summary>
	/// <param name="lcqInstFile">The LCQ instrument file.</param>
	/// <param name="lcqTuneData">The LCQ tune data.</param>
	/// <param name="massSpecDevice">The mass spec device.</param>
	private static void ConstructScanEventSegments(InstrumentFile lcqInstFile, TuneData[] lcqTuneData, MassSpecDevice massSpecDevice)
	{
		List<List<ScanEvent>> list = new List<List<ScanEvent>>();
		NonItcl nonItclInfo = lcqInstFile.MsMethodInfo.NonItclInfo;
		for (int i = 0; i < nonItclInfo.NumSegments; i++)
		{
			MsSegment msSegment = nonItclInfo.MsSegments[i];
			int num = msSegment.MsScanEvents.Length;
			List<ScanEvent> list2 = new List<ScanEvent>(num);
			for (int j = 0; j < num; j++)
			{
				ScanEvent scanEvent = LcqConverter.LcqCreateXcalScanEvent(ref msSegment.MsScanEvents[j]);
				double dataType = lcqTuneData[i].TuneDataStructInfo.DataType;
				scanEvent.ScanDataTypeAsByte = ((dataType < 0.5) ? ((byte)1) : ((byte)0));
				list2.Add(scanEvent);
			}
			list.Add(list2);
		}
		massSpecDevice.ScanEventSegments = list;
	}

	/// <summary>
	/// Gets the mass spec analog channels used count.
	/// </summary>
	/// <param name="hasExperimentMethod">if set to <c>true</c> [has experiment method].</param>
	/// <param name="lcqInstFile">The LCQ instrument file.</param>
	/// <param name="massSpecAnalogChannelsUsedCount">The mass spec analog channels used count.</param>
	/// <param name="massSpecAnalogChannelsUsed">The mass spec analog channels used.</param>
	/// <returns>Used count</returns>
	private static int GetMsAnalogChannelsUsedCount(bool hasExperimentMethod, InstrumentFile lcqInstFile, int massSpecAnalogChannelsUsedCount, ref byte massSpecAnalogChannelsUsed)
	{
		if (hasExperimentMethod)
		{
			InstrumentConfig instConfig = lcqInstFile.InstConfig;
			for (int i = 0; i < 4; i++)
			{
				if (instConfig.ExtDetInUse(i))
				{
					massSpecAnalogChannelsUsedCount++;
					switch (i)
					{
					case 0:
						massSpecAnalogChannelsUsed++;
						break;
					case 1:
						massSpecAnalogChannelsUsed += 2;
						break;
					case 2:
						massSpecAnalogChannelsUsed += 4;
						break;
					case 3:
						massSpecAnalogChannelsUsed += 8;
						break;
					}
				}
			}
		}
		else
		{
			massSpecAnalogChannelsUsedCount = 4;
			massSpecAnalogChannelsUsed = 15;
		}
		return massSpecAnalogChannelsUsedCount;
	}

	/// <summary>
	/// Releases unmanaged and - optionally - managed resources.
	/// </summary>
	/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
	private void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}
		if (disposing)
		{
			foreach (IReadWriteAccessor item in _oldLcqDataAccessors.Where((IReadWriteAccessor accessor) => accessor != null))
			{
				item.ReleaseAndCloseMemoryMappedFile(Manager);
			}
		}
		_disposed = true;
	}

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
