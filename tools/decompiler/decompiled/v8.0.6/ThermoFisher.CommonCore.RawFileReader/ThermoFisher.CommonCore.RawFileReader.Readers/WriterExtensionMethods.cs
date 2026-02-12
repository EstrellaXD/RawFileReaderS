using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

namespace ThermoFisher.CommonCore.RawFileReader.Readers;

/// <summary>
/// The writer extension methods.
/// </summary>
internal static class WriterExtensionMethods
{
	/// <summary>
	/// Writes the float.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="startPos">The start position.</param>
	public static void WriteFloat(this IMemMapWriter writer, float value, ref long startPos)
	{
		startPos += writer.WriteFloat(startPos, value);
	}

	/// <summary>
	/// Writes the double.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="startPos">The start position.</param>
	public static void WriteDouble(this IMemMapWriter writer, double value, ref long startPos)
	{
		startPos += writer.WriteDouble(startPos, value);
	}

	/// <summary>
	/// Writes the byte.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="startPos">The start position.</param>
	public static void WriteByte(this IMemMapWriter writer, byte value, ref long startPos)
	{
		startPos += writer.WriteByte(startPos, value);
	}

	/// <summary>
	/// Writes the bytes.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="startPos">The start position.</param>
	public static void WriteBytes(this IMemMapWriter writer, byte[] value, ref long startPos)
	{
		startPos += writer.WriteBytes(startPos, value);
	}

	/// <summary>
	/// Writes the short.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <param name="value">The value.</param>
	/// <param name="startPos">The start position.</param>
	public static void WriteShort(this IMemMapWriter writer, short value, ref long startPos)
	{
		startPos += writer.WriteShort(startPos, value);
	}
}
