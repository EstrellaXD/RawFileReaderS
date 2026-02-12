using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Provides methods to log information from "other" devices.
/// That is: devices which have no scan data, just status (diagnostics) or errors.
/// For example: pump pressure, sampled every second.<para />
/// Note: The following functions should be called before acquisition begins:<para />
/// 1. Write Instrument Info<para />
/// 2. Write Instrument Expected Run Time<para />
/// 3. Write Status Log Header <para />
/// If caller is not intended to use the status log data, pass a null argument or zero length array.<para />
/// ex. WriteStatusLogHeader(null) or WriteStatusLogHeader(new IHeaderItem[0])
/// </summary>    
public interface IOtherDeviceWriter : IBaseDeviceWriter, IDisposable, IFileError
{
}
