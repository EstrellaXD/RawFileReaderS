using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.ASR;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.PDAUV;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

/// <summary>
///     The UV device.
/// </summary>
internal class UvDevice : DeviceBase
{
	/// <summary>
	/// The empty packet.
	/// For cases where there is no matching data in the file.
	/// </summary>
	private class EmptyPacket : IPacket
	{
		/// <summary>
		/// Gets the segmented peaks.
		/// </summary>
		public List<SegmentData> SegmentPeaks { get; }

		/// <summary>
		/// Gets the scan index.
		/// </summary>
		public IScanIndex Index { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.UvDevice.EmptyPacket" /> class.
		/// </summary>
		/// <param name="index">
		/// The index.
		/// </param>
		public EmptyPacket(IScanIndex index)
		{
			Index = index;
			SegmentPeaks = new List<SegmentData>();
		}
	}

	private readonly bool _isInAcquisition;

	private readonly bool _oldRev;

	private readonly string _rawFileName;

	private IReadWriteAccessor _acqDataViewer;

	private UvScanIndices _scanIndices;

	private bool _disposed;

	private bool _hwtPckDadFix;

	private BufferInfo _peakDataBufferInfo;

	private BufferInfo _scanIndexBufferInfo;

	/// <summary>
	/// Gets or sets the _packet viewer.
	/// </summary>
	internal IReadWriteAccessor UvPacketViewer { private get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.UvDevice" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">raw file loader ID</param>
	/// <param name="deviceInfo">
	/// The device info.
	/// </param>
	/// <param name="rawFileName">Raw file name</param>
	/// <param name="fileVersion">The file version. </param>
	/// <param name="isInAcquisition">Flag indicates that it's in acquisition or not</param>
	/// <param name="oldRev">Flag indicates that it is an old LCQ data</param>
	public UvDevice(IViewCollectionManager manager, Guid loaderId, VirtualControllerInfo deviceInfo, string rawFileName, int fileVersion, bool isInAcquisition, bool oldRev)
		: base(manager, loaderId, deviceInfo, rawFileName, fileVersion, isInAcquisition, oldRev)
	{
		_isInAcquisition = isInAcquisition;
		_oldRev = oldRev;
		_rawFileName = rawFileName;
	}

	/// <summary>
	/// get the scan index.
	/// </summary>
	/// <param name="spectrum">
	/// The spectrum.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IScanIndex" />.
	/// </returns>
	public override IScanIndex GetScanIndex(int spectrum)
	{
		int validIndexIntoScanIndices = GetValidIndexIntoScanIndices(spectrum);
		return _scanIndices[validIndexIntoScanIndices];
	}

	/// <summary>
	/// The method gets the retention time for the scan number.
	/// </summary>
	/// <param name="spectrum">
	/// The scan number.
	/// </param>
	/// <returns>
	/// The retention time for the scan number.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// If the scan number is not in range.
	/// </exception>
	public override double GetRetentionTime(int spectrum)
	{
		return GetScanIndex(spectrum).StartTime;
	}

	/// <summary>
	/// get the UV index.
	/// </summary>
	/// <param name="spectrum">
	/// The spectrum number.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex" />.
	/// </returns>
	public ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex GetUvIndex(int spectrum)
	{
		int validIndexIntoScanIndices = GetValidIndexIntoScanIndices(spectrum);
		return _scanIndices[validIndexIntoScanIndices];
	}

	/// <summary>
	/// The method gets the packet.
	/// </summary>
	/// <param name="scanNumber">
	/// The scan number.
	/// </param>
	/// <param name="includeReferenceAndExceptionData">Flag indicates that it should include reference and exception data or not</param>
	/// <param name="channelNumber">For UV device only, negative one (-1) for getting all the channel data by the given scan number</param>
	/// <param name="packetScanDataFeatures">True if noise and baseline data is required. Not used for UV types</param>
	/// <returns>Peak data packet </returns>
	public override IPacket GetPacket(int scanNumber, bool includeReferenceAndExceptionData, int channelNumber = -1, PacketFeatures packetScanDataFeatures = PacketFeatures.All)
	{
		ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex uvIndex = GetUvIndex(scanNumber);
		if (uvIndex == null)
		{
			throw new ArgumentException($"Scan index for scan {scanNumber}  is null!");
		}
		SpectrumPacketType packetType = uvIndex.PacketType;
		if (packetType == SpectrumPacketType.PdaUvDiscreteChannel || (uint)(packetType - 12) <= 1u)
		{
			return GetChannelPacket(uvIndex, channelNumber);
		}
		return GetPdaPacket(UvPacketViewer, uvIndex);
	}

	/// <summary>
	/// The method loads the UV device information.
	/// </summary>
	/// <param name="viewer">Memory map accessor</param>
	/// <param name="dataOffset">Data offset</param>
	/// <param name="fileRevision">Raw file version</param>
	/// <returns>The number of read bytes </returns>
	public new long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = base.RunHeader.SpectrumPos - viewer.InitialOffset;
		int numSpectra = base.RunHeader.NumSpectra;
		if (base.RunHeader.NumSpectra == 0 && base.RunHeader.SpectrumPos == 0L)
		{
			startPos = base.OffsetOfTheEndOfDeviceCommonInfo;
		}
		_scanIndices = viewer.LoadRawFileObjectExt(() => new UvScanIndices(base.Manager, numSpectra), fileRevision, ref startPos);
		if (base.RunHeader.NumSpectra > 0)
		{
			FixUpUnkonwnAbsorbanceUnit(_scanIndices[0]);
		}
		OffsetOfEndOfDevice = startPos + viewer.InitialOffset;
		return startPos - dataOffset;
	}

	/// <summary>
	/// Gets the channel scan packet.
	/// </summary>
	/// <param name="scanIndex">Index of the scan.</param>
	/// <param name="channelNum">The channel number.</param>
	/// <returns>Data from the channel</returns>
	public IPacket GetChannelPacket(ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex scanIndex, int channelNum)
	{
		IDisposableReader uvPacketViewer = UvPacketViewer;
		if (channelNum >= scanIndex.NumberOfChannels)
		{
			return new EmptyPacket(scanIndex);
		}
		IPacket packet;
		switch (scanIndex.PacketType)
		{
		case SpectrumPacketType.MassSpecAnalog:
			packet = new MsAnalogPacket(uvPacketViewer, base.FileRevision, scanIndex, channelNum);
			break;
		case SpectrumPacketType.PdaUvDiscreteChannel:
		case SpectrumPacketType.UvChannel:
			packet = new ChannelUvPacket(uvPacketViewer, base.FileRevision, scanIndex, channelNum);
			break;
		default:
			throw new ArgumentException("Unknown UV/PDA packet type");
		}
		double num = 1.0;
		switch (base.InstrumentId.AbsorbanceUnit)
		{
		case AbsorbanceUnits.Au:
			num = 1000000.0;
			break;
		case AbsorbanceUnits.MilliAu:
			num = 1000.0;
			break;
		case AbsorbanceUnits.MicroAu:
		case AbsorbanceUnits.OtherAu:
		case AbsorbanceUnits.None:
		case AbsorbanceUnits.OtherUnits:
			num = 1.0;
			break;
		}
		foreach (SegmentData segmentPeak in packet.SegmentPeaks)
		{
			int num2 = 0;
			if (Math.Abs(num - 1.0) > double.Epsilon)
			{
				List<DataPeak> dataPeaks = segmentPeak.DataPeaks;
				for (int i = 0; i < dataPeaks.Count; i++)
				{
					DataPeak value = dataPeaks[i];
					value.Intensity *= num;
					dataPeaks[i] = value;
					num2++;
				}
			}
			double tic = scanIndex.Tic;
			tic *= num;
			scanIndex.Tic = ((num2 > 0) ? (tic / (double)num2) : tic);
		}
		return packet;
	}

	/// <summary>
	/// The method gets a scan of PDA data.
	/// Intensities are scaled (to micro AU) depending on logged units of the device.
	/// The Tic value is also adjusted in the index.
	/// </summary>
	/// <param name="viewer">Memory map into file</param>
	/// <param name="scanIndex">
	/// The scan index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.IPacket" /> Scaled PDA scan data.
	/// </returns>
	private IPacket GetPdaPacket(IDisposableReader viewer, ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex scanIndex)
	{
		long dataOffset = scanIndex.DataOffset;
		AdjustableScanRateProfilePacket adjustableScanRateProfilePacket = new AdjustableScanRateProfilePacket(viewer, dataOffset, base.FileRevision, scanIndex);
		for (int i = 0; i < adjustableScanRateProfilePacket.Indices.Count; i++)
		{
			AbsorbanceUnits absorbanceUnit = base.InstrumentId.AbsorbanceUnit;
			if (_hwtPckDadFix)
			{
				adjustableScanRateProfilePacket.Indices[i].AbsorbanceUnitScale = 1.0;
			}
			double num = ((adjustableScanRateProfilePacket.Indices[i].AbsorbanceUnitScale <= 0.0) ? 1.0 : adjustableScanRateProfilePacket.Indices[i].AbsorbanceUnitScale);
			switch (absorbanceUnit)
			{
			case AbsorbanceUnits.Au:
			case AbsorbanceUnits.OtherAu:
			case AbsorbanceUnits.OtherUnits:
				num *= 1E-06;
				break;
			case AbsorbanceUnits.MilliAu:
			case AbsorbanceUnits.MilliUnits:
				num *= 0.001;
				break;
			}
			if (Math.Abs(num - 1.0) > double.Epsilon)
			{
				List<DataPeak> dataPeaks = adjustableScanRateProfilePacket.SegmentPeaks[i].DataPeaks;
				if (dataPeaks != null)
				{
					for (int j = 0; j < dataPeaks.Count; j++)
					{
						DataPeak value = dataPeaks[j];
						value.Intensity /= num;
						dataPeaks[j] = value;
					}
				}
			}
			List<DataPeak> dataPeaks2 = adjustableScanRateProfilePacket.SegmentPeaks[i].DataPeaks;
			if (dataPeaks2 != null && dataPeaks2.Count > 0)
			{
				scanIndex.Tic /= (double)adjustableScanRateProfilePacket.SegmentPeaks[i].DataPeaks.Count * num;
			}
		}
		return adjustableScanRateProfilePacket;
	}

	/// <summary>
	/// The method gets a valid index into scan indices.
	/// </summary>
	/// <param name="spectrum">
	/// The spectrum.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	/// <exception cref="T:System.Exception">
	/// Thrown if the spectrum is out of range.
	/// </exception>
	private int GetValidIndexIntoScanIndices(int spectrum)
	{
		int num = spectrum - base.RunHeader.FirstSpectrum;
		if (spectrum >= base.RunHeader.FirstSpectrum && spectrum <= base.RunHeader.LastSpectrum && num < _scanIndices.Count)
		{
			return num;
		}
		throw new Exception($"The scan nummber must be >= {base.RunHeader.FirstSpectrum} and <= {base.RunHeader.LastSpectrum}.");
	}

	/// <summary>
	/// Fixes up unknown absorbance unit.
	/// </summary>
	/// <param name="scanIndex">Index of the scan.</param>
	private void FixUpUnkonwnAbsorbanceUnit(IScanIndex scanIndex)
	{
		_ = scanIndex.PacketType;
		VirtualDeviceTypes deviceType = base.DeviceType;
		if ((uint)(deviceType - 1) <= 1u || deviceType == VirtualDeviceTypes.UvDevice)
		{
			FixUpUnknownAbsorbanceUnitForChannel();
		}
		else
		{
			FixUpUnknownAbsorbanceUnitForPda();
		}
	}

	/// <summary>
	/// Fixes up unknown absorbance unit for channel.
	/// </summary>
	private void FixUpUnknownAbsorbanceUnitForChannel()
	{
		if (base.InstrumentId.AbsorbanceUnit != AbsorbanceUnits.Unknown)
		{
			return;
		}
		string model = base.InstrumentId.Model;
		if (model.Equals("UV2000"))
		{
			base.InstrumentId.AbsorbanceUnit = AbsorbanceUnits.Au;
			return;
		}
		if (model.StartsWith("HP"))
		{
			base.InstrumentId.AbsorbanceUnit = AbsorbanceUnits.Au;
			return;
		}
		if (model.StartsWith("SurveyorPDA"))
		{
			base.InstrumentId.AbsorbanceUnit = AbsorbanceUnits.MicroAu;
			return;
		}
		if (base.RunHeader.Revision > 0)
		{
			base.InstrumentId.AbsorbanceUnit = AbsorbanceUnits.MicroAu;
			return;
		}
		string softwareVersion = base.InstrumentId.SoftwareVersion;
		if (softwareVersion.Equals("99", StringComparison.OrdinalIgnoreCase))
		{
			base.InstrumentId.AbsorbanceUnit = AbsorbanceUnits.MilliAu;
		}
		else if (softwareVersion.IsNullOrEmpty())
		{
			base.InstrumentId.AbsorbanceUnit = AbsorbanceUnits.MilliAu;
		}
		else
		{
			base.InstrumentId.AbsorbanceUnit = AbsorbanceUnits.MicroAu;
		}
	}

	/// <summary>
	/// Fixes up unknown absorbance unit for PDA.
	/// </summary>
	private void FixUpUnknownAbsorbanceUnitForPda()
	{
		if (base.InstrumentId.AbsorbanceUnit == AbsorbanceUnits.Unknown)
		{
			if (base.InstrumentId.Model.StartsWith("HP"))
			{
				base.InstrumentId.AbsorbanceUnit = AbsorbanceUnits.Au;
			}
			else if (base.InstrumentId.Model.StartsWith("SurveyorPDA"))
			{
				base.InstrumentId.AbsorbanceUnit = AbsorbanceUnits.MilliAu;
			}
			else if (base.RunHeader.Revision == 0 && base.InstrumentId.SoftwareVersion == "99")
			{
				_hwtPckDadFix = true;
				base.InstrumentId.AbsorbanceUnit = AbsorbanceUnits.MilliAu;
			}
			else if (base.DeviceType == VirtualDeviceTypes.PdaDevice || base.DeviceType == VirtualDeviceTypes.UvDevice)
			{
				base.InstrumentId.AbsorbanceUnit = AbsorbanceUnits.Au;
			}
			else
			{
				base.InstrumentId.AbsorbanceUnit = AbsorbanceUnits.OtherUnits;
			}
		}
	}

	/// <summary>
	/// Initializes the buffer information.
	/// </summary>
	/// <param name="loaderId">The loader identifier.</param>
	private void InitializeBufferInfo(Guid loaderId)
	{
		_peakDataBufferInfo = new BufferInfo(loaderId, base.DataFileMapName, "PEAKDATA", creatable: false).UpdateErrors(DevErrors);
		_scanIndexBufferInfo = new BufferInfo(loaderId, base.DataFileMapName, "UVSCANINDEX", creatable: false, Utilities.StructSizeLookup.Value[5]).UpdateErrors(DevErrors);
	}

	/// <summary>
	/// Disposes the buffer information and temporary files.
	/// </summary>
	private void DisposeBufferInfoAndTempFiles()
	{
		bool deletePermitted = !base.InAcquisition;
		_peakDataBufferInfo?.DeleteTempFileOnlyIfNoReference(base.RunHeader.DataPktFilename, deletePermitted).DisposeBufferInfo();
		_scanIndexBufferInfo?.DeleteTempFileOnlyIfNoReference(base.RunHeader.SpectFilename, deletePermitted).DisposeBufferInfo();
	}

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public override void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			if (_scanIndices != null)
			{
				_scanIndices.Dispose();
				_scanIndices = null;
			}
			UvPacketViewer?.ReleaseAndCloseMemoryMappedFile(base.Manager);
			_acqDataViewer?.ReleaseAndCloseMemoryMappedFile(base.Manager);
			base.Dispose();
			DisposeBufferInfoAndTempFiles();
		}
	}

	/// <summary>
	/// Re-read the current file, to get the latest data.
	/// Only meaningful if the object has an implied backing file (such as IO.DLL and .raw files)
	/// No-op otherwise
	/// </summary>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public override bool RefreshViewOfFile()
	{
		try
		{
			if (base.RunHeader.IsInAcquisition && base.RefreshViewOfFile())
			{
				UvPacketViewer = UvPacketViewer.GetMemoryMappedViewer(base.LoaderId, base.RunHeader.DataPktFilename, 0L, 0L, inAcquisition: true, DataFileAccessMode.OpenCreateReadLoaderId);
				if (UvPacketViewer == null && !MemoryMappedFileHelper.IsFailedToMapAZeroLengthFile(StreamHelper.ConstructStreamId(base.LoaderId, base.RunHeader.DataPktFilename)))
				{
					return false;
				}
				_acqDataViewer = _acqDataViewer.GetMemoryMappedViewer(base.LoaderId, base.RunHeader.SpectFilename, inAcquisition: true, DataFileAccessMode.OpenCreateReadLoaderId);
				if (_acqDataViewer != null)
				{
					_scanIndices?.Dispose();
					Load(_acqDataViewer, 0L, base.FileRevision);
					return true;
				}
				if (MemoryMappedFileHelper.IsFailedToMapAZeroLengthFile(StreamHelper.ConstructStreamId(base.LoaderId, base.RunHeader.SpectFilename)))
				{
					return true;
				}
			}
		}
		catch (Exception)
		{
		}
		return false;
	}

	public override IDevice Initialize()
	{
		BaseDataInitialization();
		if (_isInAcquisition)
		{
			InitializeBufferInfo(base.LoaderId);
			return this;
		}
		if (_oldRev)
		{
			return this;
		}
		UvPacketViewer = base.Manager.GetRandomAccessViewer(base.LoaderId, _rawFileName, base.RunHeader.PacketPos, 0L, inAcquisition: false);
		Load(base.RawDataViewer, base.OffsetOfTheEndOfDeviceCommonInfo, base.FileRevision);
		return this;
	}
}
