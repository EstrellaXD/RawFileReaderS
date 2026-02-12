namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
///     The status log key.
/// </summary>
internal sealed class StatusLogKey
{
	/// <summary>
	///     Gets the blob index.
	/// </summary>
	internal int BlobIndex { get; private set; }

	/// <summary>
	///     Gets the retention time.
	/// </summary>
	internal double RetentionTime { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.StatusLogKey" /> class.
	/// </summary>
	/// <param name="rt">
	/// The retention time.
	/// </param>
	/// <param name="index">
	/// The index.
	/// </param>
	internal StatusLogKey(double rt, int index)
	{
		RetentionTime = rt;
		BlobIndex = index;
	}
}
