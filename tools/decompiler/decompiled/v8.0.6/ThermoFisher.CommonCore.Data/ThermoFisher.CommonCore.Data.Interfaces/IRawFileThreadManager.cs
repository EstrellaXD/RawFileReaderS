using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines managed access to raw data, for multiple threads.
/// An object implementing this interface "owns" the underlying raw data, and will close any associated file
/// when disposed. All created thread accessors must no longer be in used, when this object is disposed.
/// </summary>
public interface IRawFileThreadManager : IRawFileThreadAccessor, IDisposable
{
}
