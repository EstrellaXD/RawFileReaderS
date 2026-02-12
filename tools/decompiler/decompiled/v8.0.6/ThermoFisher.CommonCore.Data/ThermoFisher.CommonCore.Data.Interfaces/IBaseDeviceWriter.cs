using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Provides methods to write common device information<para />
/// 1. Instrument information<para />
/// 2. Instrument expected run time<para />
/// 3. Status log header<para />
/// 4. Status log<para />
/// 5. Error log
/// </summary>
public interface IBaseDeviceWriter : IDisposable, IFileError
{
	/// <summary>
	/// Writes the instrument comments.<para />
	/// These are device run header fields - comment1 and comment2.  They are part of the Chromatogram view title (Sample Name and Comment).<para />
	/// These fields can be set only once. 
	/// </summary>
	/// <param name="comment1">The comment1 for "Sample Name" in Chromatogram view title (max 39 chars).</param>
	/// <param name="comment2">The comment2 for "Comment" in Chromatogram view title (max 63 chars).</param>
	/// <returns>True if comment1 and comment2 are written to disk successfully, false otherwise.</returns>
	bool WriteInstComments(string comment1, string comment2);

	/// <summary>
	/// Write the Instrument ID info to the raw data file. The
	/// Instrument ID must be written to the raw file before any data can be
	/// acquired.
	/// </summary>
	/// <param name="instId">The instrument identifier.</param>
	/// <returns>True if instrument id is written to disk successfully, False otherwise</returns>
	bool WriteInstrumentInfo(IInstrumentDataAccess instId);

	/// <summary>
	/// Write the expected run time. All scanning devices must do this so
	/// that the real-time update can display a sensible Axis.
	/// A device of type "Other" has no scans, and so this
	/// is optional information in that case.
	/// </summary>
	/// <param name="runTime">The run time.</param>
	/// <returns>True if expected run time is written to disk successfully, False otherwise</returns>
	bool WriteInstExpectedRunTime(double runTime);

	/// <summary>
	/// Write the Status Log Header (format) info to the raw data file. <para />
	/// If caller is not intended to use the status log data, pass a null argument or zero length array.<para />
	/// ex. WriteStatusLogHeader(null) or WriteStatusLogHeader(new IHeaderItem[0])
	/// </summary>
	/// <param name="headerItems">The log header.</param>
	/// <returns>True if status log header is written to disk successfully, False otherwise</returns>
	bool WriteStatusLogHeader(IHeaderItem[] headerItems);

	/// <summary>
	/// If any Status Log details are to be written to the raw data file
	/// then the format this data will take must be written to the file while
	/// setting up i.e. prior to acquiring any data.<para />
	/// The order and types of the data elements in the byte array parameter 
	/// to the method needs to be the same as the order and types that are defined in the header. 
	/// </summary>
	/// <param name="retentionTime">The retention time.</param>
	/// <param name="data">The status data stores in byte array.</param>
	/// <returns>True if status log entry is written to disk successfully, False otherwise</returns>
	bool WriteStatusLog(float retentionTime, byte[] data);

	/// <summary>
	/// If any Status Log details are to be written to the raw data file
	/// then the format this data will take must be written to the file while
	/// setting up i.e. prior to acquiring any data.<para />
	/// The order and types of the data elements in the byte array parameter 
	/// to the method needs to be the same as the order and types that are defined in the header. 
	/// </summary>
	/// <param name="retentionTime">The retention time.</param>
	/// <param name="data">The data stores in object array.</param>
	/// <returns>True if status log entry is written to disk successfully, False otherwise</returns>
	bool WriteStatusLog(float retentionTime, object[] data);

	/// <summary>
	/// Write an error log to the raw data file.
	/// </summary>
	/// <param name="retentionTime">The retention time.</param>
	/// <param name="errorLog">The error log.</param>
	/// <returns>True if error log entry is written to disk successfully, False otherwise</returns>
	bool WriteErrorLog(float retentionTime, string errorLog);
}
