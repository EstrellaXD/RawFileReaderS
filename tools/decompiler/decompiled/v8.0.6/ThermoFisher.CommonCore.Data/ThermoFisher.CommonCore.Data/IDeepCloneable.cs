namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Interface supporting type safe deep cloning of objects
/// </summary>
/// <typeparam name="T">Type to clone
/// </typeparam>
public interface IDeepCloneable<out T>
{
	/// <summary>
	/// Produce a deep copy of an object.
	/// Must not contain any references into the original.
	/// </summary>
	/// <returns>The deep cloned object
	/// </returns>
	T DeepClone();
}
