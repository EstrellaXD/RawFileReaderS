using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This static factory class provides methods for exporting instrument method from a raw file
/// </summary>
public static class InstrumentMethodExporterFactory
{
	private static readonly ObjectFactory<IInstrumentMethodExporter> Exporter = CreateExporter();

	/// <summary>
	/// Creates the instrument method exporter with an input raw file name.<para />
	/// It returns an interface which can be used to export an instrument method from a raw file.<para />
	/// <c>
	/// <para />Example:
	/// <para />using (var exporter = InstrumentMethodExporterFactory.ReadFile(rawFile))
	/// <para />{
	/// <para />    if (!exporter.HasError &amp;&amp; exporter.HasInstrumentMethod)
	/// <para />    {
	/// <para />        exporter.ExportInstrumentMethod("Export instrument method file name", false);
	/// <para />    }
	/// <para />}
	/// </c>
	/// </summary>
	/// <param name="fileName">Name of the raw file.</param>
	/// <returns>Interface object for exporting the instrument method.</returns>
	public static IInstrumentMethodExporter ReadFile(string fileName)
	{
		return Exporter.SpecifiedMethod(new object[1] { fileName });
	}

	/// <summary>
	/// create a reader for instrument methods
	/// </summary>
	/// <returns>
	/// A factory to read instrument methods.
	/// </returns>
	private static ObjectFactory<IInstrumentMethodExporter> CreateExporter()
	{
		return new ObjectFactory<IInstrumentMethodExporter>("ThermoFisher.CommonCore.RawFileReader.Writers.InstrumentMethodExporterAdapter", "ThermoFisher.CommonCore.RawFileReader.dll", "OpenRawFile", new Type[1] { typeof(string) }, initialize: true);
	}
}
