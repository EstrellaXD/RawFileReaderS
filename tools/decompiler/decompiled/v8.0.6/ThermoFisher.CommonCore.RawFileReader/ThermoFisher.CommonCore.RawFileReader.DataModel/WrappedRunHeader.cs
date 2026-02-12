using System;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// The wrapped run header.
/// Converts internal run header to public interface
/// </summary>
internal class WrappedRunHeader : ThermoFisher.CommonCore.Data.Interfaces.IRunHeader
{
	private readonly ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader _runHeader;

	/// <summary>
	/// Gets the first comment about this data stream.
	/// </summary>
	public string Comment1 => _runHeader.Comment1;

	/// <summary>
	/// Gets the second comment about this data stream.
	/// </summary>
	public string Comment2 => _runHeader.Comment2;

	/// <summary>
	/// Gets the time of last scan in file
	/// </summary>
	public double EndTime => _runHeader.EndTime;

	/// <summary>
	/// Gets the count of error log entries
	/// </summary>
	public int ErrorLogCount => _runHeader.NumErrorLog;

	/// <summary>
	/// Gets the expected data acquisition time.
	/// </summary>
	public double ExpectedRunTime => _runHeader.ExpectedRunTime;

	/// <summary>
	/// Gets the number of digits of precision suggested for formatting masses
	/// in the filters.
	/// </summary>
	public int FilterMassPrecision => _runHeader.FilterMassPrecision;

	/// <summary>
	/// Gets the first spectrum (scan) number (typically 1).
	/// </summary>
	public int FirstSpectrum => _runHeader.FirstSpectrum;

	/// <summary>
	/// Gets the highest recorded mass in file
	/// </summary>
	public double HighMass => _runHeader.HighMass;

	/// <summary>
	/// Gets a value indicating whether this file is being created.
	/// </summary>
	public int InAcquisition => _runHeader.IsInAcquisition ? 1 : 0;

	/// <summary>
	/// Gets the last spectrum (scan) number.
	/// If this is less than 1, then there are no scans acquired yet.
	/// </summary>
	public int LastSpectrum => _runHeader.LastSpectrum;

	/// <summary>
	/// Gets the lowest recorded mass in file
	/// </summary>
	public double LowMass => _runHeader.LowMass;

	/// <summary>
	/// Gets the mass resolution of this instrument.
	/// </summary>
	public double MassResolution => _runHeader.MassResolution;

	/// <summary>
	/// Gets the max integrated intensity.
	/// </summary>
	public double MaxIntegratedIntensity => _runHeader.MaxIntegratedIntensity;

	/// <summary>
	/// Gets the max intensity.
	/// </summary>
	public double MaxIntensity => _runHeader.MaxIntensity;

	/// <summary>
	/// Gets the count of recorded spectra
	/// </summary>
	public int SpectraCount => _runHeader.NumSpectra;

	/// <summary>
	/// Gets the time of first scan in file
	/// </summary>
	public double StartTime => _runHeader.StartTime;

	/// <summary>
	/// Gets the count of status log entries
	/// </summary>
	public int StatusLogCount => _runHeader.NumStatusLog;

	/// <summary>
	/// Gets the tolerance units
	/// </summary>
	public ToleranceUnits ToleranceUnit => _runHeader.ToleranceUnit;

	/// <summary>
	/// Gets the count of "trailer extra" records.
	/// Typically, same as the count of scans.
	/// </summary>
	public int TrailerExtraCount => _runHeader.NumTrailerExtra;

	/// <summary>
	/// Gets the count of "scan events"
	/// </summary>
	public int TrailerScanEventCount => _runHeader.NumTrailerScanEvents;

	/// <summary>
	/// Gets the count of tune data entries
	/// </summary>
	public int TuneDataCount => _runHeader.NumTuneData;

	public string WriterProtocol => _runHeader.WriterProtocol;

	public RawDataDomain DeviceDataDomain => _runHeader.DeviceDataDomain;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedRunHeader" /> class.
	/// </summary>
	/// <param name="runHeader">The run header.</param>
	/// <exception cref="T:System.ArgumentNullException">run header is null</exception>
	public WrappedRunHeader(ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRunHeader runHeader)
	{
		if (runHeader == null)
		{
			throw new ArgumentNullException("runHeader");
		}
		_runHeader = runHeader;
	}
}
