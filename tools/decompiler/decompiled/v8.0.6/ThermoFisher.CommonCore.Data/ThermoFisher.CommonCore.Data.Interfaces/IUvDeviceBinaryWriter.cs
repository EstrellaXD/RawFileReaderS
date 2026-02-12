using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines a writer for UV data, which may include binary data formats
/// </summary>
public interface IUvDeviceBinaryWriter : IUvDeviceWriter, IBaseDeviceWriter, IDisposable, IFileError, IBinaryBaseDataWriter
{
}
