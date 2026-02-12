**<u>MSFileReader to RawFileReader Transition Document</u>**

This document provides a list of common methods from the MSFileReader
COM library and lists the methods in the RawFileReader library that
should be used in place of these methods.

In addition to this document, the MSFileReader C# and RawFileReader C#
example programs can be used to demonstrate how to access our RAW files
and the changes needed to migrate from MSFileReader to RawFileReader.
This document should be used in conjunction with the MSFileReader Manual
and the RawFileReader library XML documentation if more details about a
function or method are needed.

<table>
<colgroup>
<col style="width: 33%" />
<col style="width: 32%" />
<col style="width: 34%" />
</colgroup>
<thead>
<tr>
<th style="text-align: center;">MSFileReader</th>
<th style="text-align: center;">RawileReader</th>
<th style="text-align: center;">Description</th>
</tr>
</thead>
<tbody>
<tr>
<td><mark>IXRawfile.Open</mark></td>
<td><mark>RawFileReaderAdapter.FileFactory</mark></td>
<td>Initialize and open the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.Close</mark></td>
<td>IRawDataPlus.Close</td>
<td>Close the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.GetFileName</mark></td>
<td>IRawDataPlus.FileName</td>
<td>The name of the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile</mark>.IsError</td>
<td>IRawDataPlus.IsError</td>
<td>Checks for any errors during acquisition</td>
</tr>
<tr>
<td><mark>IXRawFile.GetErrorCode</mark></td>
<td>IRawDataPlus.FileError</td>
<td>Returns the file error</td>
</tr>
<tr>
<td><mark>IXRawfile.GetNumberOfControllers</mark></td>
<td>IRawDataPlus.InstrumentCount</td>
<td>Gets the number of instruments/controllers present in the RAW
file</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark>SetCurrentController</td>
<td>IRawDataPlus.<mark>SelectInstrument</mark></td>
<td>Sets the instrument/controller who’s data should be retrieved from
the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.GetCurrentController</mark></td>
<td>IRawDataPlus.SelectedInstrument</td>
<td>Get the selected instrument/controller type for the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.GetControllerType</mark></td>
<td>IRawDataPlus.GetInstrumentType</td>
<td>Gets type of instrument/controller for the specified item.</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark> GetNumberOfControllersOfType</td>
<td>IRawDataPlus.GetInstrumentCountOfType</td>
<td>Gets the number of instrument/controllers for a specific type</td>
</tr>
<tr>
<td><mark>IXRawFile.IsThereMSData</mark></td>
<td>IRawDataPlus.HasMSData</td>
<td>Flag indicating if MSdata is present in the RAW file</td>
</tr>
<tr>
<td></td>
<td>IRawDataPlus.FileHeader</td>
<td>Get the file data (header) for the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.GetAcquisitionDate</mark></td>
<td>IFileHeader.CreationDate</td>
<td>Acquisition date and time</td>
</tr>
<tr>
<td><mark>IXRawfile.GetCreationDate</mark></td>
<td>IFileHeader.CreationDate</td>
<td>Acquisition date and time</td>
</tr>
<tr>
<td><mark>IXRawfile.GetOperator</mark></td>
<td>IFileHeader.WhoCreatedId</td>
<td>Operator name</td>
</tr>
<tr>
<td><mark>IXRawfile.GetCreatorID</mark></td>
<td>IFileHeader.WhoCreatedId</td>
<td>Operator name</td>
</tr>
<tr>
<td><mark>IRawfile.GetInstrumentDescription</mark></td>
<td>IFileHeader.Description</td>
<td>Description</td>
</tr>
<tr>
<td><mark>IRawfile.GetVersionNumber</mark></td>
<td>IFileHeader.Revision</td>
<td>The RAW file version number</td>
</tr>
<tr>
<td><mark>IXRawfile.GetComment1</mark></td>
<td>IFileHeader.Comment1</td>
<td>Gets the first comment field</td>
</tr>
<tr>
<td><mark>IXRawfile.GetComment2</mark></td>
<td>IFileHeader.Comment2</td>
<td>Gets the second comment field</td>
</tr>
<tr>
<td></td>
<td>IRawDataPlus. <mark>GetInstrumentData</mark>()</td>
<td>Gets the instrument data (header) for the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.GetInstModel</mark></td>
<td>InstrumentData.Model</td>
<td>The instrument model</td>
</tr>
<tr>
<td><mark>IXRawfile</mark>.GetInstName</td>
<td>InstrumentData.Name</td>
<td>The name of the instrument</td>
</tr>
<tr>
<td><mark>IXRawfile.GetInstrumentId</mark></td>
<td>n/a</td>
<td></td>
</tr>
<tr>
<td>IXRawfile.GetInstSerialNumber</td>
<td>InstrumentData.SerialNumber</td>
<td>The serial number of the instrument</td>
</tr>
<tr>
<td>IXRawfile.GetInstSoftwareVersion</td>
<td>InstrumentData.SoftwareVersion</td>
<td>The Xcalibur software version</td>
</tr>
<tr>
<td>IXRawfile.GetInstHardwareVersion</td>
<td>InstrumentData.HardwareVersion</td>
<td>The firmware version for the instrument</td>
</tr>
<tr>
<td></td>
<td>IRawDataPlus.RunHeaderEx</td>
<td>Gets the run header from the RAW file</td>
</tr>
<tr>
<td>IXRawfile.GetNumSpectra</td>
<td>IRunHeader.SpectraCount</td>
<td>The number of spectra in the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.GetFirstSpectrumNumber</mark></td>
<td>IRunHeader.FirstSpectrum</td>
<td>Gets the number for the first spectrum in the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.GetLastSpectrumNumber</mark></td>
<td>IRunHeader.LastSpectrum</td>
<td>Gets the number for the last spectrum in the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.GetStartTime</mark></td>
<td>IRunHeader.StartTime</td>
<td>Gets the start time for the first spectrum in the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.GetEndTime</mark></td>
<td>IRunHeader.EndTime</td>
<td>Gets the end time for the last spectrum in the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark>GetLowMass</td>
<td>IRunHeader.LowMass</td>
<td>Gets the low mass from the mass range</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark>GetHighMass</td>
<td>IRunHeader.HighMass</td>
<td>Gets the high mass from the mass range</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark>GetMassResolution</td>
<td>IRunHeader.MassResolution</td>
<td>Gets the mass resolution from the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.GetChroData</mark></td>
<td>IRawDataPlus.GetChromatogramData</td>
<td>Gets a chromatogram</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark>GetMassListFromScanNum</td>
<td><p>IRawDataPlus.GetCentroidStream</p>
<p>IRawDataPlus.GetSegmentedScan</p>
<p>Scan.FromFile</p></td>
<td>Gets a spectrum from a RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.GetAverageMassList</mark></td>
<td>IRawDataPlus.<mark>AverageScansInScanRange</mark></td>
<td>Averages the scans within the provided scan range</td>
</tr>
<tr>
<td><mark>IXRawfile.GetFilters</mark></td>
<td>IRawDataPlus.GetFilters</td>
<td>The list of scan filters stored in the RAW file</td>
</tr>
<tr>
<td>IXRawfile.GetFilterForScanNum</td>
<td>IRawDataPlus.GetFilterForScanNumber</td>
<td>Gets the scan filter for a specified scan in the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark>GetMSOrderForScanNum</td>
<td>IScanFilter.MSOrder</td>
<td>The MS order for the scan</td>
</tr>
<tr>
<td><mark>IXRawfile.GetActivationTypeForScanNum</mark></td>
<td>IScanFilter.GetActivation</td>
<td>The activation type for the scan</td>
</tr>
<tr>
<td><mark>IXRawfile</mark>.GetMassAnalyzerTypeForScanNum</td>
<td>IScanFilter.MassAnalyzer</td>
<td>The mass analyzer type for the scan</td>
</tr>
<tr>
<td><mark>IXRawfile.GetIsolationWidthForScanNum</mark></td>
<td>IScanFilter.GetIsolationWidth</td>
<td>Get the isolation width for the scan number</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark>GetPrecursorRangeForScanNum</td>
<td>IScanEvent.GetReaction</td>
<td>Get the precursor information from the IReaction object</td>
</tr>
<tr>
<td><mark>IXRawfile.GetNumberOfMSOrdersFromScanNum</mark></td>
<td>IScanFilter.MSOrder</td>
<td>Cast the iScanFilter.MSOrder value to an int</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark>GetScanTypeForScanNum</td>
<td>IScanFilter.ScanMode</td>
<td>The scan mode for the scan</td>
</tr>
<tr>
<td>IXRawfile.GetScanHeaderInfoForScanNum</td>
<td>IRawDataPlus.ScanStatsForScanNum</td>
<td>Get information regarding the specified scan number</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
</tr>
<tr>
<td></td>
<td>IRawDataPlus.GetScanStatsForScanNumber</td>
<td>Gets the Scan Statistics object for the specified scan</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark>IsCentroidScanForScanNum</td>
<td>ScanStatistics.IsCentroidScan</td>
<td>Checks if the scan has centroid data</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark>GetTrailerExtraForScanNum</td>
<td><p>IRawDataPlus.GetTrailerExtraInformation</p>
<p>IRawDataPlus.GetTrailerExtraValues</p>
<p>IRawDataPlus.GetTrailerExtraHeaderInformation</p></td>
<td>Gets the Trailer Extra data section for a scan in the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark>GetTrailerExtraValueForScanNum</td>
<td>IRawDataPlus.GetTrailerExtraValue</td>
<td>Gets the Trailer Extra value for a scan number</td>
</tr>
<tr>
<td><mark>IXRawfile</mark>.RTFromScanNum</td>
<td><mark>IRawDataPlus.RetentionTimeFromScanNumber</mark></td>
<td>Gets the retention time given the scan number</td>
</tr>
<tr>
<td><mark>IXRawfile</mark>.ScanNumFromRT</td>
<td>IRawDataPlus.ScanNumberFromRetentionTime</td>
<td>Gets the scan number given the retention time</td>
</tr>
<tr>
<td></td>
<td>IRawDataPlus.SampleInformation</td>
<td>Gets the Sample information from the RAW file</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark>GetInjectionVolume</td>
<td>SampleInformation.InjectionVolume</td>
<td>The injection volume</td>
</tr>
<tr>
<td><mark>IXRawfile.</mark>GetSampleVolume</td>
<td>SampleInformation.SampleVolume</td>
<td>The sample volume</td>
</tr>
<tr>
<td>IXRawfile.GetSampleWeight</td>
<td>SampleInformation.SampleWeights</td>
<td>The sample weight</td>
</tr>
<tr>
<td>IXRawfile.GetSeqRowDilutionFactor</td>
<td>SampleInformation.DilutionFactor</td>
<td>The sample dilution factor</td>
</tr>
<tr>
<td>IXRawfile.GetSeqRowSampleName</td>
<td>SampleInformation.SampleName</td>
<td>The name of the sample</td>
</tr>
<tr>
<td>IXRawfile.GetSeqRowInjectionVolume</td>
<td>SampleInformation.InjectionVolume</td>
<td>The injection volume for the sample</td>
</tr>
<tr>
<td>IXRawfile.GetSeqRowSampleId</td>
<td>SampleInformation.SampleId</td>
<td>The id of the sample</td>
</tr>
<tr>
<td>IXRawfile.GetSeqRowSampleType</td>
<td>SampleInformation.SampleType</td>
<td>The type of the sample</td>
</tr>
<tr>
<td>IXRawfile.GetSeqRowSampleVolume</td>
<td>SampleInformation.SampleVolume</td>
<td>The volume of the sample</td>
</tr>
<tr>
<td>IXRawfile.GetSeqRowSampleWeight</td>
<td>SampleInformation.SampleWeight</td>
<td>The weight of the sample</td>
</tr>
<tr>
<td>IXRawfile.GetSeqRowSampleComment</td>
<td>SampleInformation.SampleComment</td>
<td>The comment associated with the sample</td>
</tr>
<tr>
<td>IXRawfile.GetSeqRowISTDAmount</td>
<td>SampleInformation.IstdAmount</td>
<td>The ISTD amount for this sample</td>
</tr>
<tr>
<td>IXRawfile.GetSeqRowInstrumentMethod</td>
<td>SampleInformation.InstrumentMethodFile</td>
<td>The instrument method file for this sample</td>
</tr>
<tr>
<td>IXRawfile.GetSeqRowRawFileName</td>
<td>SampleInformation.RawFileName</td>
<td>The RAW file for this sample</td>
</tr>
<tr>
<td>IXRawfile.GetSeqRowUserText</td>
<td>SampleInformation.UserText</td>
<td>The user text field(s) for the sample</td>
</tr>
<tr>
<td>IXRawfile.GetInstMethod</td>
<td>IRawDataPlus.GetInstrumentMethod</td>
<td>Gets an instrument method</td>
</tr>
<tr>
<td>IXRawfile.GetNumInstMethod</td>
<td>IRawDataPlus.InstrumentMethodCount</td>
<td>Gets the count of instrument methods</td>
</tr>
<tr>
<td>IXRawfile.GetInstMethodNames</td>
<td><mark>IRawDataPlus.GetAllInstrumentNamesFromInstrumentMethod</mark></td>
<td>Gets the names for each instrument from the instrument method</td>
</tr>
<tr>
<td>IXRawfile.GetNumTuneData</td>
<td>IRawDataPlus.GetTuneDataCount</td>
<td>Gets the number of tune data</td>
</tr>
<tr>
<td></td>
<td>IRawDataPlus.GetTuneData</td>
<td>Gets the tune data information in the form of LogEntry.</td>
</tr>
<tr>
<td>IXRawfile.GetTuneDataLabels</td>
<td>LogEntry.Labels</td>
<td>Get a tune data label</td>
</tr>
<tr>
<td>IXRawfile.GetTuneDataValue</td>
<td>LogEntry.Values</td>
<td>Gets a tune data value</td>
</tr>
<tr>
<td><p>IXRawfile.GetIsoWidth</p>
<p>IXRawfile.GetIsoWidthFromTrailerExtra</p></td>
<td>IRawDataPlus.GetScanDependants</td>
<td>Gets the scan related acquisition values</td>
</tr>
<tr>
<td>IXRawfile.GetPrecursorMassForScanNum</td>
<td>IScanEvent.GetReaction</td>
<td>Get the precursor information from the IReaction object</td>
</tr>
<tr>
<td>IXRawfile.GetPrecursorInfoFromScanNum</td>
<td>IScanEvent.GetReaction</td>
<td>Get the precursor information from the IReaction object</td>
</tr>
<tr>
<td>IXRawfile.GetStatusLogForScanNum</td>
<td>IRawFile.GetStatusLogForRetentionTime</td>
<td>Gets the status log entry at the specified retention time</td>
</tr>
<tr>
<td>IXRawfile.GetErrorLogItem</td>
<td>IRawFile.GetErrorLogItem</td>
<td>Gets the error log entry at the specified retention time</td>
</tr>
<tr>
<td>IXRawfile.IsQExactiveOrbitrapFile</td>
<td></td>
<td>No direct function but instead get the Instrument Name and check for
"Q Exactive Orbitrap"</td>
</tr>
<tr>
<td>IXRawfile.GetMassPrecisionEstimate</td>
<td></td>
<td>External class that works on top of the RAW file access code (be it
FileIO, IO, or RawFileReader)</td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
</tr>
<tr>
<td>IXRawFile.GetNoiseData</td>
<td>GetAdvancedPacketData</td>
<td></td>
</tr>
<tr>
<td></td>
<td></td>
<td></td>
</tr>
<tr>
<td>IXRawfile. GetAcquisitionFileName</td>
<td></td>
<td>Deprecated in Xcalibur 1.0</td>
</tr>
<tr>
<td>IXRawfile. GetInstrumentID</td>
<td></td>
<td>Deprecated in Xcalibur 1.0</td>
</tr>
<tr>
<td>IXRawfile. GetInletID</td>
<td></td>
<td>Deprecated in Xcalibur 1.0</td>
</tr>
<tr>
<td>IXRawfile. GetSampleAmountUnits</td>
<td></td>
<td>Deprecated in Xcalibur 1.0</td>
</tr>
</tbody>
</table>
