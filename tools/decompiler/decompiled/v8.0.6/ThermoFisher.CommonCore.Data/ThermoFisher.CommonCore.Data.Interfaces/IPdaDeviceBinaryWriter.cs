using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Provides methods to write PDA type devices data, incuding binary formatted logs.<para />
/// Note: The following functions should be called before acquisition begins:<para />
/// 1. Write Instrument Info<para />
/// 2. Write Instrument Expected Run Time<para />
/// 3. Write Status Log Header<para />
/// If caller is not intended to use the status log data, pass a null argument or zero length array.<para />
/// ex. WriteStatusLogHeader(null) or WriteStatusLogHeader(new IHeaderItem[0])
/// </summary>
public interface IPdaDeviceBinaryWriter : IPdaDeviceWriter, IBaseDeviceWriter, IDisposable, IFileError, IBinaryBaseDataWriter
{
}
