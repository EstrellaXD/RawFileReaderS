using System;
using System.Collections.Generic;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The file writer.
/// This type controls creation of a raw file.
/// </summary>
internal sealed class RawFileWriter : IRawFileWriter, IDisposable, IFileError
{
	private const string CorruptedFilePostFix = "_CORRUPTED";

	private const int SavedTooEarly = -100;

	private const int MaxFileNameLength = 256;

	private static readonly int RawFileInfoStructSize = Utilities.StructSizeLookup.Value[1];

	private static readonly FileSystemAccessRule FullFileAccessRule = ((Utilities.IsRunningMono.Value || Utilities.IsRunningUnderLinux.Value) ? null : CreateFullFileAccess());

	private readonly ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader _fileHeader;

	private readonly Guid _instanceGuid;

	private readonly DeviceErrors _errors;

	private BinaryWriter _binaryFileWriter;

	private bool _disposed;

	private EmbeddedMethod _embeddedInstrumentMethod;

	private SequenceRow _sequenceRow;

	private AutoSamplerConfig _autoSamplerConfig;

	private IReadWriteAccessor _rawFileInfoAccessor;

	private RawFileInfo _rawFileInfo;

	private IRawFileDeviceWriter[] _deviceWriterList;

	private AuditTrail _auditTrail;

	private List<AuditData> _auditDataListItems;

	private FileSecurity _defaultRawFileSecurity;

	private bool _partialSave;

	private long _rawFileInfoPosition;

	/// <summary>
	/// Gets a value indicating whether this file has detected an error.
	/// If this is false: Other error properties in this interface have no meaning.
	/// </summary>
	public bool HasError => _errors.HasError;

	/// <summary>
	/// Gets a value indicating whether this file has detected a warning.
	/// If this is false: Other warning properties in this interface have no meaning.
	/// </summary>
	public bool HasWarning => _errors.HasWarning;

	/// <summary>
	/// Gets a value indicating whether an error occurred.
	/// </summary>
	public bool IsError => _errors.HasError;

	/// <summary>
	/// Gets the error code, 0 if no error has occurred. A non 0 value is an error. Can not set to 0 and can not overwrite existing error code. 
	/// The acquisition service might set an error code due to errors in other libraries/devices.
	/// </summary>
	public int ErrorCode => _errors.ErrorCode;

	/// <summary>
	/// Gets the error message.
	/// </summary>
	public string ErrorMessage => _errors.ErrorMessage;

	/// <summary>
	/// Gets the warning message if warning has occurred. Empty string if no warning.
	/// </summary>
	public string WarningMessage => _errors?.WarningMessage ?? string.Empty;

	/// <summary>
	/// Gets or sets a value indicating whether should save changes.
	/// </summary>
	public bool ShouldSaveChanges { get; set; }

	/// <summary>
	/// Gets the raw file name.
	/// </summary>
	public string RawFileName { get; private set; }

	/// <summary>
	/// Gets the calculated checksum for this raw file. Only calculated after a save has occurred.
	/// </summary>
	public long CheckSum => _fileHeader.CheckSum;

	/// <summary>
	/// Gets the memory mapped raw file name.
	/// </summary>
	private string MemoryMappedRawFileName => Utilities.CorrectNameForEnvironment(RawFileName);

	/// <summary>
	/// create full file access.
	/// </summary>
	/// <returns>
	/// The <see cref="T:System.Security.AccessControl.FileSystemAccessRule" />.
	/// </returns>
	private static FileSystemAccessRule CreateFullFileAccess()
	{
		return new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, AccessControlType.Allow);
	}

	/// <summary>
	/// Fixes the invalid index of the virtual device and updates the shared memory mapped RawFileInfo object
	///  if the device index is out of range.
	/// </summary>
	/// <returns>RawFileInfoStruct object</returns>
	private RawFileInfoStruct FixVirtualDeviceIndex()
	{
		_errors.AppendInformataion("Start FixVirtualDeviceIndex");
		RawFileInfoStruct rawFileInfoStruct = _rawFileInfoAccessor.ReadStructure<RawFileInfoStruct>(0L, RawFileInfoStructSize);
		bool flag = false;
		int numberOfVirtualControllers = rawFileInfoStruct.NumberOfVirtualControllers;
		_errors.AppendInformataion("Devices: " + numberOfVirtualControllers);
		if (numberOfVirtualControllers <= 0)
		{
			_errors.AppendInformataion("End FixVirtualDeviceIndex. No Devices");
			return rawFileInfoStruct;
		}
		for (int i = 0; i < numberOfVirtualControllers; i++)
		{
			_errors.AppendInformataion("Device: " + i);
			int virtualDeviceIndex = rawFileInfoStruct.VirtualControllerInfoStruct[i].VirtualDeviceIndex;
			if (virtualDeviceIndex > 64 || virtualDeviceIndex < 0 || virtualDeviceIndex != i)
			{
				flag = true;
				_errors.AppendInformataion("Device Index Invalid: " + i + " Value Found: " + virtualDeviceIndex);
			}
			rawFileInfoStruct.VirtualControllerInfoStruct[i].VirtualDeviceIndex = i;
		}
		if (flag)
		{
			_rawFileInfoAccessor.WriteStruct(0L, rawFileInfoStruct);
		}
		_errors.AppendInformataion("End FixVirtualDeviceIndex");
		return rawFileInfoStruct;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.RawFileWriter" /> class.
	/// This uses a mutex on the file name to prevent multiple instances trying to create the same raw file at 1 time.
	/// Subsequent instances will get a unique time stamp file name.
	/// </summary>
	/// <param name="fullFileName"> The full raw file name. </param>
	/// <param name="writerId">
	/// The writer Id.
	/// </param>
	/// <param name="permitRenaming">[Optional] it has a default value of true.<para />
	/// True to specify that the raw file writer should create a raw file if the file isn't exist; however, if the file already exists, 
	/// the writer should create a raw file with the time stamp appended to the end of the file name.<para />
	/// False to specify that the raw file writer should not create a file and throw an IO exception if the file already exists.
	/// </param>
	/// <param name="enableFileStitch">If false, the raw file writer would not perform file stitching at the end of acquisition.
	/// The raw file and device stream data would be kept within a specific folder, only used for that sample. 
	/// </param>
	public RawFileWriter(string fullFileName, Guid writerId, bool permitRenaming = true, bool enableFileStitch = true)
	{
		Mutex mutex = null;
		try
		{
			_errors = new DeviceErrors();
			mutex = Utilities.CreateNamedMutexAndWait(fullFileName, useGlobalNamespace: true, _errors);
			if (mutex == null)
			{
				Utilities.UpdateErrors(_errors, "The code wasn't already set.", 50);
				return;
			}
			_instanceGuid = writerId;
			RawFileName = fullFileName;
			_deviceWriterList = Array.Empty<IRawFileDeviceWriter>();
			if (!enableFileStitch)
			{
				SetupStreamFolder(fullFileName, permitRenaming);
			}
			else if (File.Exists(fullFileName))
			{
				if (!permitRenaming)
				{
					throw new IOException("The file exists.");
				}
				RawFileName = WriterHelper.GetTimeStampFileName(fullFileName);
			}
			_fileHeader = ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader.CreateFileHeader(FileType.RawFile);
			InitializeWriter();
		}
		catch (Exception ex)
		{
			DisposeBinaryFileWriter(_binaryFileWriter);
			_errors.UpdateError(ex.ToMessageAndCompleteStacktrace(), ex.HResult);
			throw;
		}
		finally
		{
			if (mutex != null)
			{
				mutex.ReleaseMutex();
				mutex.Close();
			}
		}
	}

	/// <summary>
	/// Disposes the raw file for writing.  Flushes all streams to disk and closes them.  Releases all shared memory objects.
	/// If last reference for temporary device files, those temp files will be deleted.
	/// </summary>
	public void Dispose()
	{
		_errors.ClearAllErrorsAndWarnings();
		InternalDispose();
	}

	/// <summary>Setups the stream folder.</summary>
	/// <param name="fullFileName">Full name of the file.</param>
	/// <param name="permitRenaming">if set to <c>true</c> [permit renaming].</param>
	/// <exception cref="T:System.IO.IOException">The file folder exists.</exception>
	/// <exception cref="T:System.IO.FileNotFoundException">The invalid path for file: {fullFileName}</exception>
	private void SetupStreamFolder(string fullFileName, bool permitRenaming)
	{
		string directoryName = Path.GetDirectoryName(fullFileName);
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fullFileName);
		string extension = Path.GetExtension(fullFileName);
		if (!string.IsNullOrEmpty(fileNameWithoutExtension) && !string.IsNullOrEmpty(directoryName) && !string.IsNullOrEmpty(extension))
		{
			string text = Path.Combine(directoryName, fileNameWithoutExtension);
			if (Directory.Exists(text))
			{
				if (!permitRenaming)
				{
					throw new IOException("The file folder exists.");
				}
				while (Directory.Exists(text))
				{
					Thread.Sleep(1000);
					string path = $"{fileNameWithoutExtension}_{DateTime.Now:yyyyMMddHHmmss}";
					text = Path.Combine(directoryName, path);
				}
			}
			TempFileHelper.ValidatePathLength(text);
			TempFileHelper.CreateDirectoryIfDoesNotExist(text);
			RawFileName = Path.Combine(text, fileNameWithoutExtension) + extension;
			TempFileHelper.SetStreamDataFolder(RawFileName, text);
			return;
		}
		throw new FileNotFoundException("The invalid path for file: " + fullFileName);
	}

	/// <summary>
	/// The internal dispose.
	/// Does not clear the log
	/// </summary>
	private void InternalDispose()
	{
		_errors.AppendInformataion("Begin Dispose");
		if (_disposed)
		{
			_errors.AppendInformataion("End Dispose: Already Disposed");
			return;
		}
		Mutex writerMutex = Utilities.CreateNamedMutexAndWait(RawFileName, useGlobalNamespace: true, _errors);
		WriterHelper.CritSec(delegate
		{
			TempFileHelper.RemoveStreamDataFolder(RawFileName);
			if (writerMutex == null)
			{
				if (_errors.ErrorCode == 0)
				{
					_errors.UpdateError("The code wasn't already set.", 50);
				}
				return false;
			}
			_embeddedInstrumentMethod?.Dispose();
			DisposeBinaryFileWriter(_binaryFileWriter);
			_rawFileInfoAccessor?.ReleaseAndCloseMemoryMappedFile(MemoryMappedRawFileManager.Instance);
			if (_deviceWriterList != null)
			{
				IRawFileDeviceWriter[] deviceWriterList = _deviceWriterList;
				for (int i = 0; i < deviceWriterList.Length; i++)
				{
					deviceWriterList[i]?.Dispose();
				}
			}
			else
			{
				_errors.AppendWarning("Dispose: Device Writer List is null");
			}
			_errors.AppendInformataion("Setting Disposed");
			_disposed = true;
			return _disposed;
		}, _errors, writerMutex);
		writerMutex?.Close();
	}

	/// <summary>
	/// Disposes the binary file writer.
	/// </summary>
	/// <param name="binFileBinaryWriter">
	/// The bin File Binary Writer.
	/// </param>
	private void DisposeBinaryFileWriter(BinaryWriter binFileBinaryWriter)
	{
		if (binFileBinaryWriter != null)
		{
			binFileBinaryWriter.Flush();
			binFileBinaryWriter.Close();
			binFileBinaryWriter.Dispose();
		}
	}

	/// <summary>
	/// Update the raw file header with the file header values passed in.  
	/// Only updates object values in memory, does not write to disk.
	/// A default FileHeader is created with every new writer instance.
	/// </summary>
	/// <param name="fileHeader">
	/// The file header object with values to use.
	/// </param>
	public void UpdateFileHeader(IFileHeaderUpdate fileHeader)
	{
		if (fileHeader == null)
		{
			throw new ArgumentNullException("fileHeader");
		}
		if (PublicMethodEntry("UpdateFileHeader"))
		{
			_fileHeader.FileDescription = fileHeader.FileDescription;
			_fileHeader.WhoCreatedLogon = fileHeader.WhoCreatedLogon;
			_fileHeader.WhoCreatedId = fileHeader.WhoCreatedId;
			PublicMethodSuccess("UpdateFileHeader");
		}
	}

	/// <summary>
	/// The update auto sampler.
	/// </summary>
	/// <param name="autoSamplerInformation">
	/// The auto sampler information.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public bool UpdateAutoSampler(IAutoSamplerInformation autoSamplerInformation)
	{
		if (autoSamplerInformation == null)
		{
			throw new ArgumentNullException("autoSamplerInformation");
		}
		if (PublicMethodEntry("UpdateAutoSampler"))
		{
			try
			{
				_autoSamplerConfig = new AutoSamplerConfig
				{
					TrayName = autoSamplerInformation.TrayName,
					TrayShape = autoSamplerInformation.TrayShape,
					VialIndex = autoSamplerInformation.VialIndex,
					TrayIndex = autoSamplerInformation.TrayIndex,
					VialsPerTray = autoSamplerInformation.VialsPerTray,
					VialsPerTrayX = autoSamplerInformation.VialsPerTrayX,
					VialsPerTrayY = autoSamplerInformation.VialsPerTrayY
				};
				return PublicMethodSuccess("UpdateAutoSampler");
			}
			catch (Exception ex)
			{
				return _errors.UpdateError(ex);
			}
		}
		return false;
	}

	/// <summary>
	/// Format a log message for public method success.
	/// </summary>
	/// <param name="name">
	/// The name.
	/// </param>
	/// <returns>
	/// Always true (success)
	/// </returns>
	private bool PublicMethodSuccess(string name)
	{
		_errors.AppendInformataion("Exit: " + name + " (success)");
		return true;
	}

	/// <summary>
	/// validate a public method entry.
	/// Entry is not valid if object is disposed.
	/// </summary>
	/// <param name="name">
	/// The name.
	/// </param>
	/// <returns>
	/// True if method can proceed
	/// </returns>
	private bool PublicMethodEntry(string name)
	{
		_errors.ClearAllErrorsAndWarnings();
		_errors.AppendInformataion("Enter: " + name);
		if (_disposed)
		{
			_errors.UpdateError(_errors.ErrorMessage + Environment.NewLine + "Raw file writer is disposed.");
			return false;
		}
		return true;
	}

	/// <summary>
	/// The update sequence row.
	/// </summary>
	/// <param name="sequenceRowSampleInfo">
	/// The sequence row sample info.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public bool UpdateSequenceRow(ISampleInformation sequenceRowSampleInfo)
	{
		if (sequenceRowSampleInfo == null)
		{
			throw new ArgumentNullException("sequenceRowSampleInfo");
		}
		if (PublicMethodEntry("UpdateSequenceRow"))
		{
			try
			{
				_sequenceRow = new SequenceRow
				{
					RowNumber = sequenceRowSampleInfo.RowNumber,
					SampleType = (int)sequenceRowSampleInfo.SampleType,
					Path = sequenceRowSampleInfo.Path,
					RawFileName = sequenceRowSampleInfo.RawFileName,
					SampleName = sequenceRowSampleInfo.SampleName,
					SampleId = sequenceRowSampleInfo.SampleId,
					Comment = sequenceRowSampleInfo.Comment,
					CalLevel = sequenceRowSampleInfo.CalibrationLevel,
					Barcode = sequenceRowSampleInfo.Barcode,
					BarcodeStatus = (int)sequenceRowSampleInfo.BarcodeStatus,
					Vial = sequenceRowSampleInfo.Vial,
					InjectionVolume = sequenceRowSampleInfo.InjectionVolume,
					SampleWeight = sequenceRowSampleInfo.SampleWeight,
					SampleVolume = sequenceRowSampleInfo.SampleVolume,
					InternalStandardAmount = sequenceRowSampleInfo.IstdAmount,
					ConcentrationDilutionFactor = sequenceRowSampleInfo.DilutionFactor,
					CalibFile = sequenceRowSampleInfo.CalibrationFile,
					Inst = sequenceRowSampleInfo.InstrumentMethodFile,
					Method = sequenceRowSampleInfo.ProcessingMethodFile
				};
				for (int i = 0; i < 5; i++)
				{
					_sequenceRow.UserTexts[i] = sequenceRowSampleInfo.UserText[i];
				}
				for (int j = 0; j < 15; j++)
				{
					_sequenceRow.ExtraUserColumns[j] = sequenceRowSampleInfo.UserText[j + 5];
				}
				return PublicMethodSuccess("UpdateSequenceRow");
			}
			catch (Exception ex)
			{
				return _errors.UpdateError(ex);
			}
		}
		return false;
	}

	/// <summary>
	/// Helper for small methods which perform a field update.
	/// Validates that the object has not been disposed.
	/// </summary>
	/// <param name="method">
	/// The method.
	/// </param>
	/// <param name="act">
	/// The act.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	private bool PublicFieldUpdate(string method, Action act)
	{
		if (PublicMethodEntry(method))
		{
			act();
			return PublicMethodSuccess(method);
		}
		return false;
	}

	/// <summary>
	/// update the creator logon.
	/// </summary>
	/// <param name="creatorLogon">
	/// The creator logon.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public bool UpdateCreatorLogon(string creatorLogon)
	{
		return PublicFieldUpdate("UpdateCreatorLogon", delegate
		{
			_fileHeader.WhoCreatedLogon = creatorLogon;
		});
	}

	/// <summary>
	///  update the creator id.
	/// </summary>
	/// <param name="creatorId">
	/// The creator id.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public bool UpdateCreatorId(string creatorId)
	{
		return PublicFieldUpdate("UpdateCreatorId", delegate
		{
			_fileHeader.WhoCreatedId = creatorId;
		});
	}

	/// <summary>
	/// The update user label.
	/// </summary>
	/// <param name="userLabels">
	/// The user label.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public bool UpdateUserLabels(string[] userLabels)
	{
		if (PublicMethodEntry("UpdateUserLabels"))
		{
			try
			{
				if (userLabels.Length <= 5)
				{
					for (int i = 0; i < userLabels.Length; i++)
					{
						_rawFileInfo.UserLabels[i] = userLabels[i];
					}
				}
				return PublicMethodSuccess("UpdateUserLabels");
			}
			catch (Exception ex)
			{
				return _errors.UpdateError(ex);
			}
		}
		return false;
	}

	/// <summary>
	/// The update user label.
	/// </summary>
	/// <param name="userLabel">
	/// The user label.
	/// </param>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public bool UpdateUserLabel(string userLabel, int index)
	{
		if (PublicMethodEntry("UpdateUserLabel"))
		{
			try
			{
				if (index < _rawFileInfo.UserLabels.Length)
				{
					_rawFileInfo.UserLabels[index] = userLabel;
					return PublicMethodSuccess("UpdateUserLabel");
				}
			}
			catch (Exception ex)
			{
				return _errors.UpdateError(ex);
			}
		}
		return false;
	}

	/// <summary>
	/// update the creation date.
	/// </summary>
	/// <param name="creationDate">
	/// The creation date.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	public bool UpdateCreationDate(DateTime creationDate)
	{
		return PublicFieldUpdate("UpdateCreationDate", delegate
		{
			_fileHeader.CreationDate = creationDate;
		});
	}

	/// <summary>
	/// Adds an audit data entry to the audit trail list of the raw file.
	/// </summary>
	/// <param name="timeChanged">
	/// The time changed.
	/// </param>
	/// <param name="whatChanged">
	/// What changed by integer value.
	/// </param>
	/// <param name="comment">
	/// The comment.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// True if successfully updated value.
	/// </returns>
	public bool AddAuditEntry(DateTime timeChanged, long whatChanged, string comment)
	{
		if (PublicMethodEntry("AddAuditEntry"))
		{
			try
			{
				AuditData item = new AuditData
				{
					TimeChanged = timeChanged,
					WhatChanged = whatChanged,
					Comment = comment
				};
				_auditDataListItems.Add(item);
				_auditTrail.AuditDataInfo = _auditDataListItems.ToArray();
				return PublicMethodSuccess("AddAuditEntry");
			}
			catch (Exception ex)
			{
				return _errors.UpdateError(ex);
			}
		}
		return false;
	}

	/// <summary>
	/// perform saving.
	/// </summary>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <returns>
	/// true on success
	/// </returns>
	private bool PerformSaving(DeviceErrors errors)
	{
		_errors.AppendInformataion("Start PerformSaving");
		bool flag = PerformPartialSave(errors);
		if (_partialSave)
		{
			_errors.AppendInformataion("Save Was Partial: Ending PerformSaving");
			return flag;
		}
		if (flag)
		{
			flag = StitchFiles(errors);
		}
		else
		{
			_errors.AppendInformataion("Partial Save Failed, not performing full save");
		}
		_errors.AppendInformataion("End PerformSaving");
		return flag;
	}

	/// <summary>
	/// Perform partial save.
	/// Saves information into the raw file,
	/// but leaves the raw file open for creation (as several temp files).
	/// </summary>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <returns>
	/// true on success
	/// </returns>
	private bool PerformPartialSave(DeviceErrors errors)
	{
		_errors.AppendInformataion("Start PerformPartialSave");
		bool flag = false;
		_errors.AppendInformataion("Performing Refresh");
		if (RemapViewOfFile())
		{
			flag = SaveHeader(errors);
		}
		if (flag)
		{
			flag = _sequenceRow.Save(_binaryFileWriter, errors);
		}
		if (flag)
		{
			flag = _autoSamplerConfig.Save(_binaryFileWriter, errors);
		}
		if (flag)
		{
			flag = SaveRawFileInfo(errors);
		}
		if (flag && _rawFileInfo.HasExpMethod)
		{
			flag = _embeddedInstrumentMethod.SaveMethod(_binaryFileWriter, errors, _partialSave);
		}
		if (flag)
		{
			_errors.AppendInformataion("Start UpdateSharedMemoryOfRawFileInfo");
			UpdateSharedMemoryOfRawFileInfo();
			_errors.AppendInformataion("End UpdateSharedMemoryOfRawFileInfo");
		}
		_errors.AppendInformataion("End PerformPartialSave");
		return flag;
	}

	/// <summary>
	/// stitch temp files (end of acquisition).
	/// </summary>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <returns>
	/// true on success
	/// </returns>
	private bool StitchFiles(DeviceErrors errors)
	{
		errors.AppendInformataion("Start StitchFiles");
		bool flag = CanSaveAllControllers();
		if (flag)
		{
			flag = SaveAllControllers(errors);
		}
		if (flag)
		{
			errors.AppendInformataion("Start SaveAuditTrail");
			SaveAuditTrail();
			errors.AppendInformataion("Completed SaveAuditTrail");
		}
		if (flag)
		{
			errors.AppendInformataion("Start MarkAcquisitionComplete");
			flag = MarkAcquisitionComplete();
			errors.AppendInformataion("Completed MarkAcquisitionComplete");
		}
		if (flag)
		{
			errors.AppendInformataion("UpdateFileHeaderCheckSum");
			flag = _fileHeader.UpdateFileHeaderCheckSum(_binaryFileWriter, errors);
		}
		if (flag)
		{
			flag = SaveUpdatedFileHeader(errors);
		}
		if (flag)
		{
			errors.AppendInformataion("SetAccessControl to default");
			if (!Utilities.IsRunningUnderLinux.Value)
			{
				new FileInfo(RawFileName).SetAccessControl(_defaultRawFileSecurity);
			}
		}
		errors.AppendInformataion("End StitchFiles");
		return flag;
	}

	/// <summary>
	/// save the updated file header.
	/// </summary>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <returns>
	/// true on success
	/// </returns>
	private bool SaveUpdatedFileHeader(DeviceErrors errors)
	{
		errors.AppendInformataion("Begin SaveUpdatedFileHeader");
		_binaryFileWriter.Seek(0, SeekOrigin.Begin);
		bool result = _fileHeader.Save(_binaryFileWriter, errors);
		errors.AppendInformataion("End SaveUpdatedFileHeader");
		return result;
	}

	/// <summary>
	/// mark acquisition complete.
	/// </summary>
	/// <returns>
	/// true on success
	/// </returns>
	private bool MarkAcquisitionComplete()
	{
		_rawFileInfo.IsInAcquisition = false;
		_binaryFileWriter.BaseStream.Position = _rawFileInfoPosition;
		bool result = _rawFileInfo.Save(_binaryFileWriter, _errors);
		_rawFileInfoAccessor.WriteStruct(0L, _rawFileInfo.RawFileInfoStruct);
		return result;
	}

	/// <summary>
	/// save the audit trail.
	/// </summary>
	private void SaveAuditTrail()
	{
		_binaryFileWriter.BaseStream.Seek(0L, SeekOrigin.End);
		_auditTrail.Save(_binaryFileWriter, _errors);
	}

	/// <summary>
	/// save data from all controllers.
	/// </summary>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <returns>
	/// true on success
	/// </returns>
	private bool SaveAllControllers(DeviceErrors errors)
	{
		bool flag = true;
		errors.AppendInformataion("Start SaveAllControllers: " + _rawFileInfo.NumberOfVirtualControllers + " Devices");
		for (int i = 0; i < _rawFileInfo.NumberOfVirtualControllers; i++)
		{
			if (flag)
			{
				flag = SaveControllerData(errors, i);
			}
		}
		errors.AppendInformataion("End SaveAllControllers");
		return flag;
	}

	/// <summary>
	/// Test if we can save all controllers.
	/// </summary>
	/// <returns>
	/// true if we can save
	/// </returns>
	private bool CanSaveAllControllers()
	{
		if (_deviceWriterList != null)
		{
			IRawFileDeviceWriter[] deviceWriterList = _deviceWriterList;
			foreach (IRawFileDeviceWriter rawFileDeviceWriter in deviceWriterList)
			{
				if (rawFileDeviceWriter != null)
				{
					ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.RunHeader runHeader = rawFileDeviceWriter.RunHeader;
					if (runHeader != null && runHeader.InstrumentDescription.StartsWith("C#") && runHeader.IsInAcquisition)
					{
						_errors.UpdateError($"Instrument still in acquisition: {runHeader.DeviceType}-{runHeader.DeviceIndex}", -100);
						return false;
					}
				}
			}
		}
		return true;
	}

	/// <summary>
	/// save data from on "controller" (device).
	/// </summary>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <param name="i">
	/// The index to the controller.
	/// </param>
	/// <returns>
	/// true on success
	/// </returns>
	private bool SaveControllerData(DeviceErrors errors, int i)
	{
		errors.AppendInformataion("Start SaveControllerData, device index: " + i);
		bool result = true;
		long packetDataOffset = FindDataOffset(i);
		IRawFileDeviceWriter rawFileDeviceWriter = _deviceWriterList[i];
		if (rawFileDeviceWriter != null)
		{
			result = rawFileDeviceWriter.Save(_binaryFileWriter, errors, packetDataOffset, out var controllerHeaderOffset);
			_rawFileInfo.RawFileInfoStruct.VirtualControllerInfoStruct[i].Offset = controllerHeaderOffset;
			errors.AppendInformataion("SaveControllerData, device type: " + _rawFileInfo.RawFileInfoStruct.VirtualControllerInfoStruct[i].VirtualDeviceType);
		}
		else
		{
			errors.AppendWarning("Null Device Detected");
		}
		errors.AppendInformataion("End SaveControllerData, device index: " + i);
		return result;
	}

	/// <summary>
	/// find data offset for the current controller.
	/// Determines where the scan data begins in the final file, for this detector.
	/// </summary>
	/// <param name="i">
	/// The i.
	/// </param>
	/// <returns>
	/// The offset into the raw file for this controller (detector)
	/// </returns>
	private long FindDataOffset(int i)
	{
		_binaryFileWriter.BaseStream.Seek(0L, SeekOrigin.End);
		if (_rawFileInfo.VirtualControllers[i].VirtualDeviceType == VirtualDeviceTypes.MsDevice)
		{
			return _rawFileInfo.MsDataOffset;
		}
		return _binaryFileWriter.BaseStream.Position;
	}

	/// <summary>
	/// update shared memory of raw file info.
	/// </summary>
	private void UpdateSharedMemoryOfRawFileInfo()
	{
		long position = _binaryFileWriter.BaseStream.Position;
		_rawFileInfo.MsDataOffset = position;
		_rawFileInfoAccessor.WriteStruct(0L, _rawFileInfo.RawFileInfoStruct);
	}

	/// <summary>
	/// save raw file info.
	/// </summary>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <returns>
	/// true on success
	/// </returns>
	private bool SaveRawFileInfo(DeviceErrors errors)
	{
		errors.AppendInformataion("Start SaveRawFileInfo");
		_rawFileInfoPosition = _binaryFileWriter.BaseStream.Position;
		RawFileInfoStruct rawFileInfoStruct = _rawFileInfoAccessor.ReadStructure<RawFileInfoStruct>(0L, RawFileInfoStructSize);
		_rawFileInfo.RawFileInfoStruct = rawFileInfoStruct;
		rawFileInfoStruct.BlobOffset = -1L;
		bool result = _rawFileInfo.Save(_binaryFileWriter, errors);
		errors.AppendInformataion("Start SaveRawFileInfo");
		return result;
	}

	/// <summary>
	/// save header.
	/// </summary>
	/// <param name="errors">
	/// The errors.
	/// </param>
	/// <returns>
	/// true on success
	/// </returns>
	private bool SaveHeader(DeviceErrors errors)
	{
		_errors.AppendInformataion("Start SaveHeader");
		_binaryFileWriter.Seek(0, SeekOrigin.Begin);
		_fileHeader.ResetChecksum();
		bool result = _fileHeader.Save(_binaryFileWriter, errors);
		_errors.AppendInformataion("End SaveHeader");
		return result;
	}

	/// <summary>
	/// Save the raw file.
	/// The raw file name is guaranteed to be unique at this point, create a mutex to prevent multiple save calls interfering.
	/// </summary>
	/// <param name="partialSave">
	/// True only if saving part of the raw file for the acquisition raw file.  
	/// On partial save, only raw file header, auto sampler information, sequence row information, and raw file info are written.  No scan data written.
	/// </param>
	/// <returns>
	/// True if file successfully written to disk.
	/// </returns>
	public bool SaveRawFile(bool partialSave)
	{
		bool flag = false;
		if (PublicMethodEntry("SaveRawFile"))
		{
			try
			{
				_partialSave = partialSave;
				_rawFileInfoPosition = 0L;
				if (!IsError && ShouldSaveChanges)
				{
					Mutex mutex = Utilities.CreateNamedMutexAndWait(RawFileName, useGlobalNamespace: true, _errors);
					try
					{
						if (mutex == null || !mutex.WaitOne(5000))
						{
							if (!_errors.HasError)
							{
								_errors.UpdateError("Cannot get the mutex in time.");
							}
							_errors.AppendInformataion("End SaveRawFile: Mutex issue");
							return false;
						}
						flag = PerformSaving(_errors);
					}
					catch (Exception ex)
					{
						_errors.UpdateError(ex);
					}
					finally
					{
						mutex?.ReleaseMutex();
					}
					mutex?.Close();
				}
				if (!flag && ShouldSaveChanges)
				{
					if (_errors.ErrorCode != -100)
					{
						RenameCorruptFile();
					}
				}
				else
				{
					_errors.AppendInformataion("End SaveRawFile (success)");
				}
			}
			catch (Exception ex2)
			{
				return _errors.UpdateError(ex2);
			}
		}
		return flag;
	}

	/// <summary>
	/// Saves the instrument method into the raw file
	/// </summary>
	/// <param name="instrumentMethodPath">The instrument method file path.</param>
	/// <param name="storageNames">List of virtual instrument storage names.</param>
	/// <param name="descriptions">List of virtual instrument descriptions.</param>
	/// <param name="shouldBeDeleted">
	/// [Optional] it has a default value of true.<para />
	/// The should be deleted flag indicating whether the temporary instrument method should be removed after the raw file writer is closed.</param>
	/// <returns>True instrument method saved to the disk, false otherwise.</returns>
	/// <exception cref="T:System.IO.FileNotFoundException">invalid instrument method path.</exception>
	public bool StoreInstrumentMethod(string instrumentMethodPath, string[] storageNames, string[] descriptions, bool shouldBeDeleted = true)
	{
		if (PublicMethodEntry("StoreInstrumentMethod"))
		{
			if (string.IsNullOrEmpty(instrumentMethodPath) || !File.Exists(instrumentMethodPath))
			{
				throw new FileNotFoundException(instrumentMethodPath);
			}
			try
			{
				string userName = Environment.UserName;
				DateTime now = DateTime.Now;
				Method method = new Method(new ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader
				{
					CreationDate = now,
					FileType = FileType.MethodFile,
					ModifiedDate = now,
					NumberOfTimesModified = 1,
					WhoCreatedId = userName,
					WhoCreatedLogon = userName,
					WhoModifiedId = userName,
					WhoModifiedLogon = userName
				}, instrumentMethodPath);
				if (storageNames != null && descriptions != null && storageNames.Length != 0 && descriptions.Length != 0)
				{
					int num = Math.Min(storageNames.Length, descriptions.Length);
					List<StorageDescription> list = new List<StorageDescription>(num);
					for (int i = 0; i < num; i++)
					{
						string storageName = (string.IsNullOrEmpty(storageNames[i]) ? string.Empty : storageNames[i]);
						string description = (string.IsNullOrEmpty(descriptions[i]) ? string.Empty : descriptions[i]);
						StorageDescription item = new StorageDescription(storageName, description);
						list.Add(item);
					}
					method.StorageDescriptions = list;
				}
				else
				{
					method.StorageDescriptions = new List<StorageDescription>();
				}
				_embeddedInstrumentMethod = new EmbeddedMethod(method, shouldBeDeleted);
				RawFileInfoStruct rawFileInfoStruct = _rawFileInfoAccessor.ReadStructure<RawFileInfoStruct>(0L, RawFileInfoStructSize);
				rawFileInfoStruct.IsExpMethodPresent = true;
				_rawFileInfo.RawFileInfoStruct = rawFileInfoStruct;
				_rawFileInfoAccessor.WriteStruct(0L, _rawFileInfo.RawFileInfoStruct);
				_fileHeader.ModifiedDate = DateTime.Now;
				_fileHeader.NumberOfTimesModified++;
				return SaveRawFile(partialSave: true);
			}
			catch (Exception ex)
			{
				return _errors.UpdateError(ex);
			}
		}
		return false;
	}

	/// <summary>
	/// Refreshes view of the raw file.  Reloads the RawFileInfo object and the virtual controllers run headers from shared memory.  
	/// Does not read actual data from any temporary files.
	/// </summary>
	/// <returns>
	/// True if successfully reloaded internal memory buffers.
	/// </returns>
	public bool RefreshViewOfFile()
	{
		if (PublicMethodEntry("RefreshViewOfFile"))
		{
			return RemapViewOfFile();
		}
		return false;
	}

	/// <summary>
	/// The initialize writer.
	/// </summary>
	private void InitializeWriter()
	{
		ShouldSaveChanges = true;
		_sequenceRow = new SequenceRow();
		_embeddedInstrumentMethod = new EmbeddedMethod(new Method(new ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader(), string.Empty), shouldBeDeleted: true);
		_autoSamplerConfig = new AutoSamplerConfig();
		_auditTrail = new AuditTrail();
		_auditDataListItems = new List<AuditData>();
		IViewCollectionManager instance = MemoryMappedRawFileManager.Instance;
		_rawFileInfo = new RawFileInfo(instance, _instanceGuid, MemoryMappedRawFileName, 66)
		{
			ComputerName = Environment.MachineName
		};
		_rawFileInfoAccessor = SharedMemHelper.CreateSharedBufferAccessor(_instanceGuid, _rawFileInfo.DataFileMapName, RawFileInfoStructSize + 20 + 4, creatable: true, _errors);
		_rawFileInfo.IsInAcquisition = true;
		_rawFileInfoAccessor.WriteStruct(0L, _rawFileInfo.RawFileInfoStruct);
		FileStream output = new FileStream(RawFileName, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
		ApplySecuritySettingsToRawFile();
		_binaryFileWriter = new BinaryWriter(output, Encoding.Unicode);
	}

	/// <summary>
	/// Remaps/refreshes the device list from the shared memory raw file info object.
	/// Instantiates new objects based on the data from the shared memory raw file info.
	/// </summary>
	/// <returns>
	/// The true if file/device list successfully remapped.
	/// </returns>
	private bool RemapViewOfFile()
	{
		_errors.AppendInformataion("Start RemapViewOfFile");
		try
		{
			RawFileInfoStruct rawFileInfoStruct = FixVirtualDeviceIndex();
			rawFileInfoStruct.BlobOffset = -1L;
			_rawFileInfo.RawFileInfoStruct = rawFileInfoStruct;
			List<VirtualControllerInfo> virtualControllers = _rawFileInfo.VirtualControllers;
			CreateDeviceList(virtualControllers);
			_errors.AppendInformataion("End RemapViewOfFile: " + (_errors.HasError ? "Fail" : "Success"));
			return !_errors.HasError;
		}
		catch (Exception ex)
		{
			_errors.AppendInformataion("Trapping Exception in RemapViewOfFile");
			return _errors.UpdateError(ex);
		}
	}

	/// <summary>
	/// Create device list.
	/// Attaches to memory maps of all known devices
	/// Needed to add a reference count to all temp files, and to get latest data about device.
	/// </summary>
	/// <param name="virtualControllerInfoList">
	/// The virtual controller info list.
	/// </param>
	private void CreateDeviceList(List<VirtualControllerInfo> virtualControllerInfoList)
	{
		bool flag = false;
		IRawFileDeviceWriter[] deviceWriterList = _deviceWriterList;
		if (deviceWriterList != null && deviceWriterList.Length != 0)
		{
			flag = UpdateExisitingDeviceList(virtualControllerInfoList);
		}
		if (!_errors.HasError && !flag)
		{
			RegenerateDeviceList(virtualControllerInfoList);
		}
	}

	/// <summary>
	/// Update the previously found list
	/// Device list me be already populated by previous refresh calls
	/// Because this list is made based on shared memory, there can be issues
	/// with
	/// 1: Items not being ready for use
	/// 2: Items being added
	/// In these cases, attempt to keep what we have, and add or fix as needed
	/// Avoid regenerating "known good" devices, and just refresh data for these.
	/// In any cases where device data cannot be initialized completely, dispose of that data
	/// and set the device to null, to be fixed by a future refresh.
	/// </summary>
	/// <param name="virtualControllerInfoList">data for all device</param>
	/// <returns>true if list is valid</returns>
	private bool UpdateExisitingDeviceList(List<VirtualControllerInfo> virtualControllerInfoList)
	{
		int count = virtualControllerInfoList.Count;
		bool flag = false;
		if (_deviceWriterList.Length <= count)
		{
			flag = ValidateExistingDeviceList(virtualControllerInfoList);
		}
		if (flag && _deviceWriterList.Length < count)
		{
			flag = AppendToExistingDeviceList(virtualControllerInfoList);
		}
		return flag;
	}

	/// <summary>
	/// make a new list, and completely replace any existing list.
	/// </summary>
	/// <param name="virtualControllerInfoList">
	/// The virtual controller info list.
	/// </param>
	private void RegenerateDeviceList(List<VirtualControllerInfo> virtualControllerInfoList)
	{
		int count = virtualControllerInfoList.Count;
		IRawFileDeviceWriter[] array = new IRawFileDeviceWriter[count];
		_errors.AppendInformataion("Creating new device list");
		for (int i = 0; i < count && (array[i] = RawFileDeviceWriter(virtualControllerInfoList, i)) != null; i++)
		{
		}
		if (_deviceWriterList != null)
		{
			_errors.AppendInformataion("Disposing Previous Devices");
			IRawFileDeviceWriter[] deviceWriterList = _deviceWriterList;
			for (int j = 0; j < deviceWriterList.Length; j++)
			{
				deviceWriterList[j]?.Dispose();
			}
		}
		else
		{
			_errors.AppendInformataion("Device list null");
		}
		_deviceWriterList = array;
	}

	/// <summary>
	/// append to existing device list.
	/// </summary>
	/// <param name="virtualControllerInfoList">
	/// The virtual controller info list.
	/// </param>
	/// <returns>
	/// true if list is valid
	/// </returns>
	private bool AppendToExistingDeviceList(List<VirtualControllerInfo> virtualControllerInfoList)
	{
		int count = virtualControllerInfoList.Count;
		_errors.AppendWarning("Previous device count: " + _deviceWriterList.Length + " New device count " + count);
		int num = _deviceWriterList.Length;
		Array.Resize(ref _deviceWriterList, count);
		for (int i = num; i < count; i++)
		{
			IRawFileDeviceWriter rawFileDeviceWriter = RawFileDeviceWriter(virtualControllerInfoList, i);
			_deviceWriterList[i] = rawFileDeviceWriter;
			if (rawFileDeviceWriter == null)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// validate the existing device list.
	/// </summary>
	/// <param name="virtualControllerInfoList">
	/// The virtual controller info list.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Boolean" />.
	/// </returns>
	private bool ValidateExistingDeviceList(List<VirtualControllerInfo> virtualControllerInfoList)
	{
		_errors.AppendInformataion("Device list already populated");
		for (int i = 0; i < _deviceWriterList.Length; i++)
		{
			IRawFileDeviceWriter rawFileDeviceWriter = _deviceWriterList[i];
			if (rawFileDeviceWriter != null)
			{
				if (!rawFileDeviceWriter.Refresh())
				{
					_errors.AppendWarning("Previous Device list cannot be refreshed");
					return false;
				}
				continue;
			}
			IRawFileDeviceWriter rawFileDeviceWriter2 = RawFileDeviceWriter(virtualControllerInfoList, i);
			_deviceWriterList[i] = rawFileDeviceWriter2;
			if (rawFileDeviceWriter2 == null)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Generate a raw file device writer.
	/// </summary>
	/// <param name="virtualControllerInfoList">
	/// The virtual controller info list.
	/// </param>
	/// <param name="index">
	/// The index.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRawFileDeviceWriter" />.
	/// </returns>
	private IRawFileDeviceWriter RawFileDeviceWriter(List<VirtualControllerInfo> virtualControllerInfoList, int index)
	{
		_errors.AppendInformataion("GetRawFileDeviceWriter: " + index);
		VirtualControllerInfo virtualControllerInfo = virtualControllerInfoList[index];
		_errors.AppendInformataion("Device: " + virtualControllerInfo.VirtualDeviceType.ToString() + " " + virtualControllerInfo.VirtualDeviceIndex);
		IRawFileDeviceWriter rawFileDeviceWriter = GetRawFileDeviceWriter(virtualControllerInfo);
		if (_errors.HasError)
		{
			_errors.AppendInformataion("Failed to add device, stopping");
			rawFileDeviceWriter?.Dispose();
			return null;
		}
		return rawFileDeviceWriter;
	}

	/// <summary>
	/// The apply security settings to raw file.
	/// </summary>
	private void ApplySecuritySettingsToRawFile()
	{
		if (!Utilities.IsRunningUnderLinux.Value)
		{
			FileInfo fileInfo = new FileInfo(RawFileName);
			_defaultRawFileSecurity = fileInfo.GetAccessControl();
			FileSecurity accessControl = fileInfo.GetAccessControl();
			accessControl.AddAccessRule(FullFileAccessRule);
			fileInfo.SetAccessControl(accessControl);
		}
	}

	/// <summary>
	/// Get raw file device writer.
	/// </summary>
	/// <param name="controllerInfo">
	/// The controller info.
	/// </param>
	/// <returns>
	/// The <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces.IRawFileDeviceWriter" />.
	/// </returns>
	private IRawFileDeviceWriter GetRawFileDeviceWriter(VirtualControllerInfo controllerInfo)
	{
		_errors.AppendInformataion("Start: GetRawFileDeviceWriter");
		switch (controllerInfo.VirtualDeviceType)
		{
		case VirtualDeviceTypes.MsDevice:
			return new RawFileMsDeviceWriter(Guid.NewGuid(), controllerInfo, MemoryMappedRawFileName, _fileHeader.Revision, _errors);
		case VirtualDeviceTypes.MsAnalogDevice:
		case VirtualDeviceTypes.AnalogDevice:
		case VirtualDeviceTypes.PdaDevice:
		case VirtualDeviceTypes.UvDevice:
		case VirtualDeviceTypes.StatusDevice:
			return new RawFileUvDeviceWriter(Guid.NewGuid(), controllerInfo, MemoryMappedRawFileName, _fileHeader.Revision, _errors);
		default:
			return null;
		}
	}

	/// <summary>
	/// Handles a corrupt file by closing writers and renaming file.
	/// </summary>
	private void RenameCorruptFile()
	{
		try
		{
			string? fileNameWithoutExtension = Path.GetFileNameWithoutExtension(RawFileName);
			string extension = Path.GetExtension(RawFileName);
			string directoryName = Path.GetDirectoryName(RawFileName);
			string text = fileNameWithoutExtension + "_CORRUPTED";
			if (text.Length > 256)
			{
				text = text.Substring(0, 256);
			}
			text += extension;
			string text2 = Path.Combine(directoryName, text);
			InternalDispose();
			File.Move(RawFileName, text2);
			RawFileName = text2;
		}
		catch (Exception ex)
		{
			_errors.AppendError(ex);
		}
	}

	/// <inheritdoc />
	public Device[] GetConnectedDevices()
	{
		if (_deviceWriterList == null || _deviceWriterList.Length == 0)
		{
			return Array.Empty<Device>();
		}
		int num = _deviceWriterList.Length;
		Device[] array = new Device[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = _deviceWriterList[i]?.RunHeader.DeviceType.ToDevice() ?? Device.None;
		}
		return array;
	}
}
