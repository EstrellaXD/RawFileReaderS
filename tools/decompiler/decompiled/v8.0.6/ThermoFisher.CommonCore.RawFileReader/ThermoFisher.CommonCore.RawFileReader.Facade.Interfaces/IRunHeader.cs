using System;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.RunHeaders;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The RunHeader interface.
/// </summary>
internal interface IRunHeader : IRealTimeAccess, IRunHeaderAccess, IDisposable
{
	/// <summary>
	/// Gets the software revision.
	/// </summary>
	int Revision { get; }

	/// <summary>
	/// Gets the comment 1.
	/// </summary>
	string Comment1 { get; }

	/// <summary>
	/// Gets the comment 2.
	/// </summary>
	string Comment2 { get; }

	/// <summary>
	/// Gets the data Packet filename.
	/// </summary>
	string DataPktFilename { get; }

	/// <summary>
	/// Gets the error log filename.
	/// </summary>
	string ErrorLogFilename { get; }

	/// <summary>
	/// Gets or sets the error log position.
	/// </summary>
	long ErrorLogPos { get; set; }

	/// <summary>
	/// Gets the expected run time.
	/// </summary>
	double ExpectedRunTime { get; }

	/// <summary>
	/// Gets the filter mass precision.
	/// </summary>
	int FilterMassPrecision { get; }

	/// <summary>
	/// Gets the instrument id file name.
	/// </summary>
	string InstIdFilename { get; }

	/// <summary>
	/// Gets the instrument scan events file name.
	/// </summary>
	string InstScanEventsFilename { get; }

	/// <summary>
	/// Gets a value indicating whether the device is in acquisition.
	/// </summary>
	bool IsInAcquisition { get; }

	/// <summary>
	/// Gets the number of error logs.
	/// </summary>
	int NumErrorLog { get; }

	/// <summary>
	/// Gets the number of spectra.
	/// </summary>
	int NumSpectra { get; }

	/// <summary>
	/// Gets the number of  status log.
	/// </summary>
	int NumStatusLog { get; }

	/// <summary>
	/// Gets the number of  trailer extra.
	/// </summary>
	int NumTrailerExtra { get; }

	/// <summary>
	/// Gets the number of  trailer scan events.
	/// </summary>
	int NumTrailerScanEvents { get; }

	/// <summary>
	/// Gets the number of  tune data.
	/// </summary>
	int NumTuneData { get; }

	/// <summary>
	/// Gets or sets the packet position.
	/// </summary>
	long PacketPos { get; set; }

	/// <summary>
	/// Gets the run header position.
	/// </summary>
	long RunHeaderPos { get; }

	/// <summary>
	/// Gets the scan events file name.
	/// </summary>
	string ScanEventsFilename { get; }

	/// <summary>
	/// Gets the spectra file name.
	/// </summary>
	string SpectFilename { get; }

	/// <summary>
	/// Gets or sets the spectrum position.
	/// </summary>
	long SpectrumPos { get; set; }

	/// <summary>
	/// Gets the status log filename.
	/// </summary>
	string StatusLogFilename { get; }

	/// <summary>
	/// Gets the status log header filename.
	/// </summary>
	string StatusLogHeaderFilename { get; }

	/// <summary>
	/// Gets or sets the status log position.
	/// </summary>
	long StatusLogPos { get; set; }

	/// <summary>
	/// Gets the trailer extra filename.
	/// </summary>
	string TrailerExtraFilename { get; }

	/// <summary>
	/// Gets or sets the trailer extra position.
	/// </summary>
	long TrailerExtraPos { get; set; }

	/// <summary>
	/// Gets the trailer header filename.
	/// </summary>
	string TrailerHeaderFilename { get; }

	/// <summary>
	/// Gets the trailer scan events filename.
	/// </summary>
	string TrailerScanEventsFilename { get; }

	/// <summary>
	/// Gets or sets the trailer scan events position.
	/// </summary>
	long TrailerScanEventsPos { get; set; }

	/// <summary>
	/// Gets the tune data filename.
	/// </summary>
	string TuneDataFilename { get; }

	/// <summary>
	/// Gets the tune data header filename.
	/// </summary>
	string TuneDataHeaderFilename { get; }

	/// <summary>
	/// Gets the internal run header struct.
	/// </summary>
	RunHeaderStruct RunHeaderStruct { get; }

	/// <summary>
	/// Gets the protocol used to create this file.
	/// </summary>
	string WriterProtocol { get; }

	/// <summary>
	/// Gets the kind of equipment which logged this data
	/// </summary>
	RawDataDomain DeviceDataDomain { get; }

	/// <summary>
	/// Copies the specified source.
	/// </summary>
	/// <param name="src">The source.</param>
	void Copy(IRunHeader src);
}
