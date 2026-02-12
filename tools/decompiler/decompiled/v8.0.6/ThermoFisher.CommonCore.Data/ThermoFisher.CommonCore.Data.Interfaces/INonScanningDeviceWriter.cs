using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines a writer for all detectors which do not collect scans
/// </summary>
public interface INonScanningDeviceWriter : IUvDeviceWriter, IBaseDeviceWriter, IDisposable, IFileError, IAnalogDeviceWriter, IOtherDeviceWriter
{
}
