using System;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The IRawDataExtended interface. Provides access to raw data.
/// This extends IRawDataPlus, adding "IRawDataExtensions".
/// This is a disposable interface, as an implementation may hold an active file,
/// database or other protocol connection.
/// Many interfaces returned from implementations of this interface may rely
/// on an active file, database or protocol connection.
/// Do not dispose of this object until interfaces (especially enumerators)
/// returned from it are no longer in use.
/// All value type returns from this interface are safe against the underlying file being closed.
/// </summary>
public interface IRawDataExtended : IRawDataPlus, IRawData, IDetectorReaderBase, IRawDataProperties, IDisposable, IRawCache, ISimplifiedScanReader, IDetectorReaderPlus, IRawDataExtensions
{
	/// <summary>
	/// Gets an Object which operates on a specific detector.
	/// This gets an additional accessor to this raw data, which will not be valid
	/// after you dispose of the this IRawDataExtended interface.
	/// This permits you to access data from different detectors on separate threads.
	/// No state is shared between instances of IDetectorReader.
	/// </summary>
	/// <param name="detector">Defines the type of detector and instance of that type (starting from 1)</param>
	/// <param name="includeReferenceAndExceptionPeaks">Optional: When selecting an MS detector, this will return
	/// data from the reference compound when requesting scans. Default "false" as most
	/// analysis will not examine reference data</param>
	/// <returns>Access to method to read from 1 detector</returns>
	IDetectorReader GetDetectorReader(IInstrumentSelectionAccess detector, bool includeReferenceAndExceptionPeaks = false);
}
