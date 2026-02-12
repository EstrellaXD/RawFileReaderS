namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines the instrument data index for UV type devices
/// </summary>
public interface IUvScanIndex : IBaseScanIndex
{
	/// <summary>
	/// Gets the number of channels.
	/// </summary>
	int NumberOfChannels { get; }

	/// <summary>
	/// Gets the frequency.
	/// <para>For UV device only, it will be ignored by Analog devices</para>
	/// </summary>
	double Frequency { get; }

	/// <summary>
	/// Gets a value indicating whether is uniform time.
	/// <para>For UV device only, it will be ignored by Analog devices</para>
	/// </summary>
	bool IsUniformTime { get; }

	/// <summary>
	/// Copies the specified source.
	/// Copy all the non-static fields of the current object to the new object.
	/// Since all the fields are value type, a bit-by-bit copy of the field is performed.
	/// </summary>
	/// <returns>Create a copy of the same object type</returns>
	IUvScanIndex DeepClone();
}
