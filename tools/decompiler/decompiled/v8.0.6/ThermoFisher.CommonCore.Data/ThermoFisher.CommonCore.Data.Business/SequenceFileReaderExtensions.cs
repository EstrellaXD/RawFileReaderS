using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Sequence File Writer Extension Methods
/// </summary>
public static class SequenceFileReaderExtensions
{
	/// <summary>
	/// Retrieves the user label at given 0-based label index.
	/// </summary>
	/// <param name="sequenceFileAccess">The sequence file access interface object.</param>
	/// <param name="index">Index of user label to be retrieved</param>
	/// <returns>String containing the user label at given index</returns>
	public static string GetUserColumnLabel(this ISequenceFileAccess sequenceFileAccess, int index)
	{
		if (sequenceFileAccess == null || sequenceFileAccess.Info == null || index < 0 || index > 20)
		{
			return string.Empty;
		}
		ISequenceInfo info = sequenceFileAccess.Info;
		if (index >= 5)
		{
			index -= 5;
			return info.UserPrivateLabel[index];
		}
		return info.UserLabel[index];
	}
}
