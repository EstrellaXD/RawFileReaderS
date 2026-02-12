using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines a writer Analog data, which may include binary data formats
/// </summary>
public interface IAnalogDeviceBinaryWriter : IAnalogDeviceWriter, IBaseDeviceWriter, IDisposable, IFileError, IBinaryBaseDataWriter
{
}
