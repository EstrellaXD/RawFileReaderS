using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
///   A strongly-typed resource class, for looking up localized strings, etc.
/// </summary>
[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	/// <summary>
	///   Returns the cached ResourceManager instance used by this class.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("ThermoFisher.CommonCore.RawFileReader.Resources", typeof(Resources).Assembly);
			}
			return resourceMan;
		}
	}

	/// <summary>
	///   Overrides the current thread's CurrentUICulture property for all
	///   resource lookups using this strongly typed resource class.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	/// <summary>
	///   Looks up a localized string similar to Cannot convert device to MS Device!.
	/// </summary>
	internal static string ErrorCannotConvertToMsDevice => ResourceManager.GetString("ErrorCannotConvertToMsDevice", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to File path is empty or null!.
	/// </summary>
	internal static string ErrorEmptyNullFileName => ResourceManager.GetString("ErrorEmptyNullFileName", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Invalid Accurate mass Type..
	/// </summary>
	internal static string ErrorInvalidAccurateMassType => ResourceManager.GetString("ErrorInvalidAccurateMassType", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Invalid Barcode Status Type..
	/// </summary>
	internal static string ErrorInvalidBarcodeStatusType => ResourceManager.GetString("ErrorInvalidBarcodeStatusType", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Invalid Device Type..
	/// </summary>
	internal static string ErrorInvalidDeviceType => ResourceManager.GetString("ErrorInvalidDeviceType", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to The file name is invalid.
	/// </summary>
	internal static string ErrorInvalidFileName => ResourceManager.GetString("ErrorInvalidFileName", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Invalid Instrument type..
	/// </summary>
	internal static string ErrorInvalidInstrumentType => ResourceManager.GetString("ErrorInvalidInstrumentType", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Invalid instrument type index..
	/// </summary>
	internal static string ErrorInvalidInstrumentTypeIndex => ResourceManager.GetString("ErrorInvalidInstrumentTypeIndex", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Invalid Packet Type..
	/// </summary>
	internal static string ErrorInvalidPacketType => ResourceManager.GetString("ErrorInvalidPacketType", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Invalid Sample Type..
	/// </summary>
	internal static string ErrorInvalidSampleType => ResourceManager.GetString("ErrorInvalidSampleType", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Invalid tolerance unit..
	/// </summary>
	internal static string ErrorInvalidToleranceUnit => ResourceManager.GetString("ErrorInvalidToleranceUnit", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Invalid Virtual Device Type.
	/// </summary>
	internal static string ErrorInvalidVirtualDeviceType => ResourceManager.GetString("ErrorInvalidVirtualDeviceType", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Cannot map a zero-length file..
	/// </summary>
	internal static string ErrorMapAZeroLenghtFile => ResourceManager.GetString("ErrorMapAZeroLenghtFile", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Missing instrument id information.
	/// </summary>
	internal static string ErrorMissingInstrumentIdInfo => ResourceManager.GetString("ErrorMissingInstrumentIdInfo", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Missing raw file information..
	/// </summary>
	internal static string ErrorMissingRawFileInfo => ResourceManager.GetString("ErrorMissingRawFileInfo", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to No instrument selected..
	/// </summary>
	internal static string ErrorNoInstrumentSelected => ResourceManager.GetString("ErrorNoInstrumentSelected", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to This is not MS device..
	/// </summary>
	internal static string ErrorNonMsDevice => ResourceManager.GetString("ErrorNonMsDevice", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to No open raw file..
	/// </summary>
	internal static string ErrorNoOpenRawFile => ResourceManager.GetString("ErrorNoOpenRawFile", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Null chromatogram settings argument.
	/// </summary>
	internal static string ErrorNullChromatogramSettingsArgrument => ResourceManager.GetString("ErrorNullChromatogramSettingsArgrument", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Null file header argument..
	/// </summary>
	internal static string ErrorNullFileHeaderArgument => ResourceManager.GetString("ErrorNullFileHeaderArgument", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Null instrument id argument..
	/// </summary>
	internal static string ErrorNullInstrumentIdArgument => ResourceManager.GetString("ErrorNullInstrumentIdArgument", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Null run header returned from RawFileReader.
	/// </summary>
	internal static string ErrorNullRunHeader => ResourceManager.GetString("ErrorNullRunHeader", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Null run header argument..
	/// </summary>
	internal static string ErrorNullRunHeaderArgument => ResourceManager.GetString("ErrorNullRunHeaderArgument", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Null scan index argument..
	/// </summary>
	internal static string ErrorNullScanIndexArgument => ResourceManager.GetString("ErrorNullScanIndexArgument", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Null sequence row argument..
	/// </summary>
	internal static string ErrorNullSequenceRowArgument => ResourceManager.GetString("ErrorNullSequenceRowArgument", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to Null UV scan index argument..
	/// </summary>
	internal static string ErrorNullUvScanIndexArgument => ResourceManager.GetString("ErrorNullUvScanIndexArgument", resourceCulture);

	/// <summary>
	///   Looks up a localized string similar to RawFileReaderTracer.
	/// </summary>
	internal static string TraceSourceName => ResourceManager.GetString("TraceSourceName", resourceCulture);

	internal Resources()
	{
	}
}
