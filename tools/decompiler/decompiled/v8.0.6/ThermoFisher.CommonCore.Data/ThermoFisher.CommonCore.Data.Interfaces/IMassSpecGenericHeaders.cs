namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// The IMassSpecGenericDataHeaders interface which contains the generic headers definitions that are going to be written to a raw file.<para />
/// If caller is not intended to use any one of these headers, caller should either pass a null argument or zero length array.<para />
/// i.e. TrailerExtraHeaders = null or TrailerExtraHeaders = new IHeaderItem[0]<para />
/// </summary>
public interface IMassSpecGenericHeaders
{
	/// <summary>
	/// Gets the trailer extra headers.
	/// </summary>
	/// <value>
	/// The trailer extra headers.
	/// </value>
	IHeaderItem[] TrailerExtraHeader { get; }

	/// <summary>
	/// Gets the status log headers.
	/// </summary>
	/// <value>
	/// The status log headers.
	/// </value>
	IHeaderItem[] StatusLogHeader { get; }

	/// <summary>
	/// Gets the tune headers.
	/// </summary>
	/// <value>
	/// The tune headers.
	/// </value>
	IHeaderItem[] TuneHeader { get; }
}
