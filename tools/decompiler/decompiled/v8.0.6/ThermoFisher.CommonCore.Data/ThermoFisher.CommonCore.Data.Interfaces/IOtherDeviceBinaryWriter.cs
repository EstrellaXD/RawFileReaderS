using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
///  Defines a writer Other data, which may include binary data formats
/// </summary>
public interface IOtherDeviceBinaryWriter : IOtherDeviceWriter, IBaseDeviceWriter, IDisposable, IFileError, IBinaryBaseDataWriter
{
}
