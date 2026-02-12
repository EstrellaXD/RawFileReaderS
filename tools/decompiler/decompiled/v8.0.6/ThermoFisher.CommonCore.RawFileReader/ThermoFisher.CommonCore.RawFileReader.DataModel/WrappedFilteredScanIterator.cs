using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// The wrapped filtered scan iterator. Iterates over scans matching a given filter.
/// </summary>
internal class WrappedFilteredScanIterator : IFilteredScanIterator
{
	private readonly MassSpecDevice _deviceInfo;

	private readonly int _firstSpectrumNumber;

	private readonly int _lastSpectrumNumber;

	private readonly ScanFilterHelper _scanFilterHelper;

	private int _currentSpectrumNumber;

	/// <summary>
	/// Gets the filter used for iterating over spectra, in string form. 
	/// </summary>
	public string Filter => _scanFilterHelper.Filter.ToString();

	/// <summary>
	/// Gets a value indicating whether there are more scans after the current scan
	/// Note that this does not check if this scan matches the filter.
	/// <returns>returns true if there is an Next spectrum in file. Otherwise false</returns>
	/// </summary>
	public bool MayHaveNext => _currentSpectrumNumber < _lastSpectrumNumber;

	/// <summary>
	/// Gets a value indicating whether there may be spectra before the current scan.
	/// Note that this does not check if this scan matches the filter.
	/// </summary>
	public bool MayHavePrevious => _currentSpectrumNumber > _firstSpectrumNumber;

	/// <summary>
	/// Gets the Next Spectrum, matching the filter
	/// <returns>returns the next spectrum number in file. 0 if there is no file open</returns>
	/// </summary>
	public int NextScan => _deviceInfo.GetNextScanIndex(_currentSpectrumNumber, _scanFilterHelper);

	/// <summary>
	/// Gets the Previous Spectrum, matching the filter
	/// <returns>returns the previous spectrum number in file. 0 if there is no file open</returns>
	/// </summary>
	public int PreviousScan => _deviceInfo.GetPreviousScanIndex(_currentSpectrumNumber, _scanFilterHelper);

	/// <summary>
	/// Sets the CurrentSpectrumNumber to the new Spectrum Number
	/// </summary>
	public int SpectrumPosition
	{
		set
		{
			if (value >= _firstSpectrumNumber - 1 && value <= _lastSpectrumNumber + 1)
			{
				_currentSpectrumNumber = value;
			}
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedFilteredScanIterator" /> class.
	/// </summary>
	/// <param name="device">
	/// The device.
	/// </param>
	/// <param name="filter">
	/// The filter.
	/// </param>
	public WrappedFilteredScanIterator(MassSpecDevice device, IFilterScanEvent filter)
	{
		_deviceInfo = device;
		_firstSpectrumNumber = _deviceInfo.RunHeader.FirstSpectrum;
		_lastSpectrumNumber = _deviceInfo.RunHeader.LastSpectrum;
		_currentSpectrumNumber = 0;
		_scanFilterHelper = SetFilter(filter);
	}

	/// <summary>
	/// Set the filter used for iterating over spectra. 
	/// This can be used with the NextSpectrumNumber method to iterate over scans with a certain filter
	/// </summary>
	/// <param name="filter">
	/// an object containing the Filter to be used for iterating over spectra
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanFilterHelper" />.
	/// </returns>
	private ScanFilterHelper SetFilter(IFilterScanEvent filter)
	{
		bool isTsqQuantumFile = _deviceInfo.InstrumentId.IsTsqQuantumFile;
		int filterMassPrecision = _deviceInfo.RunHeader.FilterMassPrecision;
		MassSpecDevice.SetPrecursorIonTolerance(filter, isTsqQuantumFile, filterMassPrecision);
		return new ScanFilterHelper(new WrappedScanFilter(filter), accuratePrecursors: false, _deviceInfo.RunHeader.FilterMassPrecision);
	}
}
