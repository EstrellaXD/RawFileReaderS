using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provides methods to create, update and write samples to sequence file.
/// </summary>
internal class SequenceFileWriter : ISequenceFileWriter, IDisposable
{
	private readonly int _totalUserLabels = 20;

	private readonly DeviceErrors _errors;

	private readonly ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader _fileHeader;

	private readonly string _fileName;

	private bool _openExisting;

	private SequenceFileInfoStruct _seqInfo;

	private BinaryWriter _seqFileBinaryWriter;

	private bool _disposed;

	/// <summary>
	/// Gets the file header for the sequence
	/// </summary>
	public IFileHeader FileHeader => _fileHeader;

	/// <summary>
	/// Gets or sets additional information about a sequence
	/// </summary>
	public ISequenceInfo Info
	{
		get
		{
			return _seqInfo;
		}
		set
		{
			CopySeqInfo(value);
		}
	}

	/// <summary>
	/// Gets the set of samples in the sequence
	/// </summary>
	public List<SampleInformation> Samples { get; private set; }

	/// <summary>
	/// Gets the file error state.
	/// </summary>
	public IFileError FileError => _errors;

	/// <summary>
	/// Gets a value indicating whether the last file operation caused an error
	/// </summary>
	public bool IsError => _errors.HasError;

	/// <summary>
	/// Gets the name of the sequence file.
	/// </summary>
	public string FileName => _fileName;

	/// <summary>
	/// Gets or sets the sequence bracket type.
	/// This determines which groups of samples use the same calibration curve.
	/// </summary>
	public BracketType Bracket
	{
		get
		{
			return _seqInfo.Bracket;
		}
		set
		{
			_seqInfo.Bracket = value;
		}
	}

	/// <summary>
	/// Gets or sets a description of the auto sampler tray
	/// </summary>
	public string TrayConfiguration
	{
		get
		{
			return _seqInfo.TrayConfiguration;
		}
		set
		{
			_seqInfo.TrayConfiguration = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.SequenceFileWriter" /> class.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="openExisting">True open an existing sequence file with read/write privilege; false to create a new unique sequence file</param>
	public SequenceFileWriter(string fileName, bool openExisting)
	{
		_errors = new DeviceErrors();
		_fileName = fileName;
		_openExisting = openExisting;
		if (openExisting)
		{
			SequenceFileLoader sequenceFileLoader = new SequenceFileLoader(fileName);
			if (sequenceFileLoader.HasError)
			{
				_errors.UpdateError(sequenceFileLoader.ErrorMessage);
				return;
			}
			_fileHeader = sequenceFileLoader.Header;
			_seqInfo = sequenceFileLoader.SequenceInfo;
			Samples = sequenceFileLoader.Samples;
			return;
		}
		if (!fileName.ToUpperInvariant().EndsWith(".SLD"))
		{
			fileName += ".sld";
		}
		if (File.Exists(fileName))
		{
			_fileName = WriterHelper.GetTimeStampFileName(fileName);
		}
		_fileHeader = ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader.CreateFileHeader(FileType.SampleList);
		InitialSequenceInfo();
	}

	/// <summary>
	/// Retrieves the user label at given 0-based label index.
	/// </summary>
	/// <param name="index">Index of user label to be retrieved</param>
	/// <returns>String containing the user label at given index</returns>
	public string GetUserColumnLabel(int index)
	{
		if (index < 0 || index > _totalUserLabels || _seqInfo == null)
		{
			_errors.UpdateError("Index out of range.");
			return string.Empty;
		}
		if (index >= 5)
		{
			index -= 5;
			return _seqInfo.UserPrivateLabel[index];
		}
		return _seqInfo.UserLabel[index];
	}

	/// <summary>
	/// Sets the user label at given 0-based label index.
	/// </summary>
	/// <param name="index">Index of user label to be set</param>
	/// <param name="label">New string value for user label to be set</param>
	/// <returns>true if successful;  false otherwise</returns>
	public bool SetUserColumnLabel(int index, string label)
	{
		if (index < 0 || index > _totalUserLabels || _seqInfo == null)
		{
			_errors.UpdateError("Index out of range.");
			return false;
		}
		if (index >= 5)
		{
			index -= 5;
			_seqInfo.UserPrivateLabel[index] = label;
		}
		else
		{
			_seqInfo.UserLabel[index] = label;
		}
		return true;
	}

	/// <summary>
	/// Update the instrument method file header with the file header values passed in.  
	/// Only updates object values in memory, does not write to disk.
	/// </summary>
	/// <param name="fileHeader">The file header.</param>
	/// <exception cref="T:System.ArgumentNullException">File header cannot be null.</exception>
	public void UpdateFileHeader(IFileHeaderUpdate fileHeader)
	{
		if (fileHeader == null)
		{
			throw new ArgumentNullException("fileHeader");
		}
		_fileHeader.FileDescription = fileHeader.FileDescription;
		_fileHeader.WhoCreatedLogon = fileHeader.WhoCreatedLogon;
		_fileHeader.WhoCreatedId = fileHeader.WhoCreatedId;
	}

	/// <summary>
	/// Saves Sequence data to file.
	/// </summary>
	/// <returns>True saved to file; false otherwise. </returns>
	public bool Save()
	{
		_errors.ClearAllErrorsAndWarnings();
		return WriterHelper.CritSec(delegate(DeviceErrors err)
		{
			bool result = false;
			if (_seqFileBinaryWriter == null)
			{
				_seqFileBinaryWriter = InitialWriter(_fileName);
			}
			else if (!_seqFileBinaryWriter.BaseStream.CanWrite)
			{
				_seqFileBinaryWriter.Dispose();
				_seqFileBinaryWriter = InitialWriter(_fileName);
			}
			if (_seqFileBinaryWriter == null || _errors.HasError)
			{
				return false;
			}
			DateTime now = DateTime.Now;
			_seqFileBinaryWriter.Seek(0, SeekOrigin.Begin);
			if (!_openExisting)
			{
				_openExisting = true;
				_fileHeader.CreationDate = now;
				_fileHeader.NumberOfTimesModified = 0;
			}
			ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader fileHeader = _fileHeader;
			string whoModifiedLogon = (_fileHeader.WhoModifiedId = Environment.UserName);
			fileHeader.WhoModifiedLogon = whoModifiedLogon;
			_fileHeader.Revision = 66;
			_fileHeader.ModifiedDate = now;
			_fileHeader.NumberOfTimesModified++;
			_fileHeader.ResetChecksum();
			if (_fileHeader.Save(_seqFileBinaryWriter, err))
			{
				_seqInfo.NumRows = Samples?.Count ?? 0;
				if (_seqInfo.Save(_seqFileBinaryWriter, err) && Samples.Save(_seqFileBinaryWriter, err))
				{
					_seqFileBinaryWriter.BaseStream.SetLength(_seqFileBinaryWriter.BaseStream.Position);
					if (_fileHeader.UpdateFileHeaderCheckSum(_seqFileBinaryWriter, err))
					{
						_seqFileBinaryWriter.Seek(0, SeekOrigin.Begin);
						if (_fileHeader.Save(_seqFileBinaryWriter, err))
						{
							result = true;
						}
					}
				}
			}
			_seqFileBinaryWriter.Flush();
			return result;
		}, _errors, _fileName, useGlobalNamespace: true);
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_errors.UpdateError("Device writer has been disposed");
			if (_seqFileBinaryWriter != null)
			{
				_seqFileBinaryWriter.Flush();
				_seqFileBinaryWriter.Dispose();
			}
		}
	}

	/// <summary>
	/// Initials the writer.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <returns>True sequence file writer created; false otherwise.</returns>
	private BinaryWriter InitialWriter(string fileName)
	{
		try
		{
			return new BinaryWriter(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite), Encoding.Unicode, leaveOpen: false);
		}
		catch (Exception ex)
		{
			_errors.UpdateError(ex);
		}
		return null;
	}

	/// <summary>
	/// Initials the sequence file.
	/// </summary>
	private void InitialSequenceInfo()
	{
		_seqInfo = new SequenceFileInfoStruct();
		_seqInfo.Initialization();
		Samples = new List<SampleInformation>(0);
	}

	/// <summary>
	/// Copies the sequence information data.
	/// </summary>
	/// <param name="srcSeqInfo">The source sequence information.</param>
	private void CopySeqInfo(ISequenceInfo srcSeqInfo)
	{
		if (srcSeqInfo != null)
		{
			_seqInfo.Bracket = srcSeqInfo.Bracket;
			_seqInfo.TrayConfiguration = srcSeqInfo.TrayConfiguration;
			int num = srcSeqInfo.ColumnWidth.Length;
			for (int i = 0; i < num; i++)
			{
				_seqInfo.ColumnWidth[i] = srcSeqInfo.ColumnWidth[i];
				_seqInfo.TypeToColumnPosition[i] = srcSeqInfo.TypeToColumnPosition[i];
			}
			for (int j = 0; j < 15; j++)
			{
				_seqInfo.UserPrivateLabel[j] = srcSeqInfo.UserPrivateLabel[j];
			}
			for (int k = 0; k < 5; k++)
			{
				_seqInfo.UserLabel[k] = srcSeqInfo.UserLabel[k];
			}
		}
		else
		{
			_seqInfo.Initialization();
		}
	}
}
