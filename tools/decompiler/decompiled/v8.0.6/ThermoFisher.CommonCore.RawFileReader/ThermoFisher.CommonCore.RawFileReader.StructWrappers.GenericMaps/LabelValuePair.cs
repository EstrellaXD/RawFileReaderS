namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

/// <summary>
///     The class that represents the label value pair - normally used in status log entries.
/// </summary>
internal sealed class LabelValuePair
{
	/// <summary>
	///     Gets the label.
	/// </summary>
	public string Label { get; }

	/// <summary>
	///     Gets the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.GenericValue" /> object containing the value.
	/// </summary>
	public GenericValue Value { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps.LabelValuePair" /> class.
	/// </summary>
	/// <param name="label">
	/// The label.
	/// </param>
	/// <param name="value">
	/// The value.
	/// </param>
	public LabelValuePair(string label, GenericValue value)
	{
		Label = label;
		Value = value ?? new GenericValue();
	}

	/// <summary>
	///     The to string.
	/// </summary>
	/// <returns>
	///     The <see cref="T:System.String" />.
	/// </returns>
	public override string ToString()
	{
		return $"{Label}: {Value}";
	}
}
