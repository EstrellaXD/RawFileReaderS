using System;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Defines the PDA Device Writer type.
/// </summary>
internal class PdaDeviceWriter : UvDeviceWriter, IPdaDeviceWriter, IBaseDeviceWriter, IDisposable, IFileError, IPdaDeviceBinaryWriter, IBinaryBaseDataWriter
{
	private bool _disposed;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.PdaDeviceWriter" /> class.
	/// </summary>
	/// <param name="deviceType">Type of the device.</param>
	/// <param name="deviceIndex">Index of the device.</param>
	/// <param name="rawFileName">Name of the raw file.</param>
	/// <param name="inAcquisition">if set to <c>true</c> [in acquisition].</param>
	/// <param name="domain">Determines the format of this channel, such as Xcalibur or Chromeleon</param>
	/// <param name="fileRevision">The file revision.</param>
	public PdaDeviceWriter(Device deviceType, int deviceIndex, string rawFileName, bool inAcquisition, RawDataDomain domain = RawDataDomain.MassSpectrometry, int fileRevision = 66)
		: base(deviceType, deviceIndex, rawFileName, inAcquisition, domain, fileRevision)
	{
	}

	/// <summary>
	/// Writes both the PDA instrument data and index into the disk. This is the
	/// simplest format of data we write to a raw file.
	/// </summary>
	/// <param name="instData">The PDA UV instrument data.</param>
	/// <param name="instDataIndex">Index of the PDA instrument scan.</param>
	/// <returns>
	/// True if scan data and index are written to disk successfully, False otherwise
	/// </returns>
	/// <exception cref="T:System.ArgumentNullException">
	/// Instrument Data
	/// or
	/// PDA Scan Index
	/// </exception>
	public bool WriteInstData(double[] instData, IPdaScanIndex instDataIndex)
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
		if (DeviceAcqStatus.CanDeviceAcquireData)
		{
			byte[] array = instData.ToIntArray().ToByteArray();
			int numData = instData.Length;
			ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.UvScanIndex uvScanIndex = instDataIndex.ConvertToUvScanIndex();
			uvScanIndex.PacketType = SpectrumPacketType.PdaUvScannedSpectrum;
			bool flag = WriteInstData(array, array.Length, uvScanIndex.PacketType, 0, isUniformTime: true);
			if (flag)
			{
				byte[] array2 = instDataIndex.AsrProfileIndexDataPktsToByteArray(numData);
				flag = WriteInstData(array2, array2.Length, SpectrumPacketType.PdaUvScannedSpectrumIndex, 0, isUniformTime: false);
			}
			if (flag)
			{
				flag = WriteInstScanIndex(uvScanIndex);
			}
			if (flag && DeviceAcqStatus.IsDeviceReady)
			{
				DeviceAcqStatus.DeviceStatus = VirtualDeviceAcquireStatus.DeviceStatusAcquiring;
			}
			return flag;
		}
		DevErrors.UpdateError("Device is not ready for acquiring data.");
		return false;
	}

	/// <summary>
	/// PDA writer doesn't support this method.
	/// This method is intended for UV type of device writers
	/// </summary>
	/// <param name="instData">The UV type of the scan data.</param>
	/// <param name="scanIndex">The UV type of the scan index.</param>
	/// <returns>
	/// Not supported exception will be thrown.
	/// </returns>
	/// <exception cref="T:System.NotSupportedException">PDA writer doesn't support.</exception>
	public override bool WriteInstData(double[] instData, IUvScanIndex scanIndex)
	{
		throw new NotSupportedException("PDA writer doesn't support.");
	}

	/// <summary>
	/// Releases unmanaged and - optionally - managed resources.
	/// </summary>
	public override void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			try
			{
				base.Dispose();
			}
			catch (Exception ex)
			{
				DevErrors.UpdateError(ex);
			}
		}
	}
}
