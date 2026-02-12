namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Methods added to support binary formatted data, in base writer
/// </summary>
public interface IBinaryBaseDataWriter
{
	/// <summary>
	/// Write an error message, where the message is already packed as byte array.
	/// </summary>
	/// <param name="retentionTime">RT of the error</param>
	/// <param name="packedLogMessage">Error (byte packed string)</param>
	/// <returns>True on success</returns>
	bool WriteBinaryErrorLog(float retentionTime, byte[] packedLogMessage);
}
