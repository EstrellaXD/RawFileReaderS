namespace ThermoFisher.CommonCore.RawFileReader.Facade;

/// <summary>
/// The storage description.
/// Describes an instrument method storage,
/// including the device name, device descriptive name (display name),
/// and the text description of the method.
/// </summary>
internal class StorageDescription
{
	/// <summary>
	/// Gets or sets the device descriptive (display) name.
	/// </summary>
	public string Description { get; set; }

	/// <summary>
	/// Gets or sets the storage name. This is the device's "registry name".
	/// This name should not be shown to the operator as "device name".
	/// Use "Description" instead for the devices' display name.
	/// </summary>
	public string StorageName { get; set; }

	/// <summary>
	/// Gets or sets the "plain text" version of the instrument method.
	/// </summary>
	public string MethodText { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.StorageDescription" /> class.
	/// </summary>
	/// <param name="storageName">
	/// The storage name.
	/// </param>
	/// <param name="description">
	/// The description.
	/// </param>
	public StorageDescription(string storageName, string description)
	{
		StorageName = storageName;
		Description = description;
		MethodText = string.Empty;
	}
}
