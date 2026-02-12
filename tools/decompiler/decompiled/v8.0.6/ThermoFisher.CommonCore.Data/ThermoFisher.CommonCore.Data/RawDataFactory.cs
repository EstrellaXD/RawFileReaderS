using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Delegate defining a method which can create an instance of IRawData, based
/// on a string (usually a file name). 
/// </summary>
/// <param name="fileName">
/// file name
/// </param>
/// <returns>
/// Interface to read raw data
/// </returns>
public delegate IRawData RawDataFactory(string fileName);
