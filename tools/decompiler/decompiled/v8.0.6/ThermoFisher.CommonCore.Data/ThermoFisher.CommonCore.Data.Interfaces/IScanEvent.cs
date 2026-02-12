using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The ScanEvent interface.
/// Determines how scans are done.
/// </summary>
public interface IScanEvent : IScanEventBase
{
	/// <summary>
	/// Gets the accurate mass setting.
	/// </summary>
	EventAccurateMass AccurateMass { get; }

	/// <summary>
	/// Gets a value indicating whether this event is valid.
	/// </summary>
	bool IsValid { get; }

	/// <summary>
	/// Gets a value indicating whether this is a custom event.
	/// A custom event implies that any scan derived from this event could be different.
	/// The scan type must be inspected to determine the scanning mode, and not the event.
	/// </summary>
	bool IsCustom { get; }

	/// <summary>
	/// Gets the source fragmentation mass range count.
	/// </summary>
	int SourceFragmentationMassRangeCount { get; }

	/// <summary>
	/// Gets the mass calibrator count.
	/// </summary>
	int MassCalibratorCount { get; }

	/// <summary>
	/// Convert to string.
	/// </summary>
	/// <returns>
	/// The converted scanning method.
	/// </returns>
	new string ToString();

	/// <summary>
	/// Get the source fragmentation mass range, at a given index.
	/// </summary>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The mass range.
	/// </returns>
	Range GetSourceFragmentationMassRange(int index);

	/// <summary>
	/// Get the mass calibrator, at a given index.
	/// </summary>
	/// <param name="index">
	/// The index, which should be from 0 to MassCalibratorCount -1
	/// </param>
	/// <returns>
	/// The mass calibrator.
	/// </returns>
	/// <exception cref="T:System.IndexOutOfRangeException">Thrown when requesting calibrator above count</exception>
	double GetMassCalibrator(int index);
}
