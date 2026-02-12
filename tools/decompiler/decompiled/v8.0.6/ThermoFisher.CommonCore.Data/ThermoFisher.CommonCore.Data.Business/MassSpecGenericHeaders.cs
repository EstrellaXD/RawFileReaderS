using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Contains the generic header definitions of the Status log header, Trailer extra header and Tune header.
/// If caller is not intended to use any one of these headers, caller should either pass a null argument or zero length array.<para />
/// i.e. TrailerExtraHeaders = null or TrailerExtraHeaders = new IHeaderItem[0]<para />
/// </summary>
/// <seealso cref="T:ThermoFisher.CommonCore.Data.Interfaces.IMassSpecGenericHeaders" />
public class MassSpecGenericHeaders : IMassSpecGenericHeaders
{
	/// <summary>
	/// Gets or sets the status log headers.
	/// Optional property, it has a default value of no header items (zero length array)
	/// </summary>
	/// <value>
	/// The status log headers.
	/// </value>
	public IHeaderItem[] StatusLogHeader { get; set; }

	/// <summary>
	/// Gets or sets the trailer extra headers.
	/// Optional property, it has a default value of no header items (zero length array)
	/// </summary>
	/// <value>
	/// The trailer extra headers.
	/// </value>
	public IHeaderItem[] TrailerExtraHeader { get; set; }

	/// <summary>
	/// Gets or sets the tune headers.
	/// Optional property, it has a default value of no header items (zero length array)
	/// </summary>
	/// <value>
	/// The tune headers.
	/// </value>
	public IHeaderItem[] TuneHeader { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.MassSpecGenericHeaders" /> class.
	/// </summary>
	public MassSpecGenericHeaders()
	{
		StatusLogHeader = new IHeaderItem[0];
		TrailerExtraHeader = new IHeaderItem[0];
		TuneHeader = new IHeaderItem[0];
	}
}
