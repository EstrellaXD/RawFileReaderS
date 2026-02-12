using System;
using ThermoFisher.CommonCore.Data.Business;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Provides methods to write mass spec device data.
/// The "PrepareForRun" method should be called during the prepare for run state, before the data acquisition begins. <para />
/// The rest of the methods will be used for data logging.
/// </summary>
public interface IMassSpecDeviceWriter : IDisposable, IFileError
{
	/// <summary>
	/// Gets a value indicating whether the PrepareForRun method has been called.
	/// </summary>
	/// <value>True if the PrepareForRun method has been called; otherwise, false.
	/// </value>
	bool IsPreparedForRun { get; }

	/// <summary>
	/// This method should be called (when creating an acquisition file) during the "Prepare for run" state.<para />
	/// It may not be called multiple times for one device. It may not be called after any of the data logging calls have been made.<para />
	/// It will perform the following operations:<para />
	/// 1. Write instrument information<para />
	/// 2. Write run header information<para />
	/// 3. Write status log header <para />
	/// 4. Write trailer extra header <para />
	/// 5. Write tune data header <para />     
	/// 6. Write run header information - expected run time, comments, mass resolution and precision.<para />
	/// 7. Write method scan events.
	/// </summary>
	/// <param name="instrumentId">The instrument ID.</param>
	/// <param name="headers">The generic data headers.</param>
	/// <param name="runHeaderInfo">The run header information.</param>
	/// <param name="methodScanEvents">Method scan events</param>
	/// <returns>True if all the values are written to disk successfully, false otherwise.</returns>
	bool PrepareForRun(IInstrumentDataAccess instrumentId, IMassSpecGenericHeaders headers, IMassSpecRunHeaderInfo runHeaderInfo, IScanEvents methodScanEvents);

	/// <summary>
	/// Write an error log to a raw file.
	/// </summary>
	/// <param name="retentionTime">The retention time.</param>
	/// <param name="errorLog">The error log.</param>
	/// <returns>True if error log entry is written to disk successfully, False otherwise</returns>
	bool WriteErrorLog(float retentionTime, string errorLog);

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
	/// If any trailer extra details are to be written to the raw data file
	/// then the format this data will take must be written to the file while
	/// setting up i.e. prior to acquiring any data.<para />
	/// The order and types of the data elements in the object array parameter 
	/// to the method needs to be the same as the order and types that are defined in the header. 
	/// </summary>
	/// <param name="data">The trailer extra data stores in object array.</param>
	/// <returns>True if trailer extra data is written to disk successfully, False otherwise</returns>
	bool WriteTrailerExtraData(object[] data);

	/// <summary>
	/// If any Trailer Extra details are to be written to the raw data file
	/// then the format this data will take must be written to the file while
	/// setting up i.e. prior to acquiring any data.<para />
	/// The order and types of the data elements in the byte array parameter 
	/// to the method needs to be the same as the order and types that are defined in the header. 
	/// </summary>
	/// <param name="data">The trailer extra data stores in byte array.</param>
	/// <returns>True if trailer extra entry is written to disk successfully, False otherwise</returns>
	bool WriteTrailerExtraData(byte[] data);

	/// <summary>
	/// If any tune details are to be written to the raw data file
	/// then the format this data will take must be written to the file while
	/// setting up i.e. prior to acquiring any data.<para />
	/// The order and types of the data elements in the object array parameter 
	/// to the method needs to be the same as the order and types that are defined in the header. 
	/// </summary>
	/// <param name="data">The tune data stores in object array.</param>
	/// <returns>True if tune data is written to disk successfully, False otherwise</returns>
	bool WriteTuneData(object[] data);

	/// <summary>
	/// If any tune data details are to be written to the raw data file
	/// then the format this data will take must be written to the file while
	/// setting up i.e. prior to acquiring any data.<para />
	/// The order and types of the data elements in the byte array parameter 
	/// to the method needs to be the same as the order and types that are defined in the header. 
	/// </summary>
	/// <param name="data">The tune data stores in byte array.</param>
	/// <returns>True if tune data entry is written to disk successfully, False otherwise</returns>
	bool WriteTuneData(byte[] data);

	/// <summary>
	/// This method is designed for exporting mass spec scanned data to a file (mostly used by the Application). <para />
	/// It converts the input scanned data into the compressed packet format and also generates a profile index 
	///  if needed by the specified packet type. <para />
	/// Overall, it writes the mass spec data packets, scan index (scan header) and trailer scan event if it is provided,
	/// to a file. <para />
	/// This method will branch to the appropriate packet methods to compress the data block before being written to disk.
	/// </summary>       
	/// <param name="instData">The transferring data that are going to be saved to a file.</param>
	/// <returns>True if mass spec data packets are written to disk successfully; false otherwise.</returns>
	bool WriteInstData(IMsInstrumentData instData);

	/// <summary>
	/// This method is designed for mass spec device data writing. <para />
	/// To provide fast data writing, this method writes the mass spec data packets directly to file (without performing <para />
	/// any data validation and data compression) by the specified packet type. <para />
	/// All data validation and data compression currently are done in the instrument driver. <para />
	/// </summary>
	/// <param name="dataBlock">The binary block of data to write.</param>
	/// <param name="packetType">Type of the packet.</param>
	/// <returns>True if mass spec data packets are written to disk successfully, false otherwise.</returns>
	bool WriteInstData(byte[] dataBlock, SpectrumPacketType packetType);

	/// <summary>
	/// This method is designed for mass spec device data writing. <para />
	/// It writes the mass spec scan index (a.k.a scan header) and trailer scan event (if it's available) to the disk.
	/// </summary>
	/// <param name="scanIndex">Index of the mass spec scan.</param>
	/// <param name="trailerScanEvent">The trailer scan event [optional].</param>
	/// <returns>True if scan index and trailer scan event (if it's available) are written to disk successfully, false otherwise.</returns>
	bool WriteInstScanIndex(IScanStatisticsAccess scanIndex, IScanEvent trailerScanEvent = null);
}
