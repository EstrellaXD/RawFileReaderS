using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Methods which operate on a detector
/// </summary>
public interface IDetectorReader : IDetectorReaderBase, IDetectorReaderPlus, IRawDataExtensions, IDisposable, ISimplifiedScanReader
{
	/// <summary>
	/// This reads the information within 1 scan as binary data.
	/// It is only available for specific use cases:
	/// <para>Is is designed for the Luna system.</para>
	/// <para>It can only be called for devices which use "Xcalibur" Data domain within luna.</para>
	/// It is not supported by devices under the "Legacy" (Xcalibur + Foundation) data domain.
	/// </summary>
	/// <param name="scan">the sca number needed</param>
	/// <returns>The "binary blob" of data for this scan.
	/// Obtain a scan decoding interface (IScanDecoder) to convert to objects </returns>
	byte[] ReadScanBinaryData(int scan);

	/// <summary>
	/// Returns scan binary data, plus all other information which may be needed to decode that data.
	/// Only available with "Luna system" created files.
	/// </summary>
	/// <param name="scan">scan needed </param>
	/// <returns>data which can be decoded</returns>
	IEncodedScan ReadEncodedScan(int scan);
}
