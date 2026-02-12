using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Provides a means of averaging scans
/// </summary>
public static class ScanAveragerFactory
{
	private static readonly ObjectFactory<IScanAveragePlus> AverageObjectFactory = CreateAverager();

	/// <summary>
	/// Create a factory, which can be used to average scans.
	/// </summary>
	/// <returns>
	/// The tool to average
	/// </returns>
	private static ObjectFactory<IScanAveragePlus> CreateAverager()
	{
		return new ObjectFactory<IScanAveragePlus>("ThermoFisher.CommonCore.BackgroundSubtraction.ScanAveragerPlus", "ThermoFisher.CommonCore.BackgroundSubtraction.dll", "FromFile", new Type[1] { typeof(IDetectorReaderPlus) }, initialize: true);
	}

	/// <summary>
	/// Create a scan averaging tool, for use with a specific raw file.
	/// This can then be use to average selected scans from the file.
	/// Processing of scan filters, and default values for tolerance
	/// also use the supplied raw data interface.
	/// </summary>
	/// <param name="data">Access to data to be averaged</param>
	/// <returns>Interface to perform averaging</returns>
	public static IScanAveragePlus GetScanAverager(IDetectorReaderPlus data)
	{
		return AverageObjectFactory.SpecifiedMethod(new object[1] { data });
	}
}
