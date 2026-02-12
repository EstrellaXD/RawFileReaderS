using System;
using System.IO;
using System.Linq;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Provide method to export an instrument method from a raw file
/// </summary>
/// <seealso cref="T:System.IDisposable" />
/// <seealso cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.IErrors" />
internal class RawFileInstrumentMethodAccessor : LoaderBase, IInstrumentMethodExporter, IDisposable
{
	private const int MaxSize80Mb = 83886080;

	private readonly IReadWriteAccessor _accessor;

	private readonly int _fileRevision;

	private IMethod _methodInfo;

	private bool _disposed;

	/// <summary>
	/// Gets a value indicating whether the underlying raw file has instrument method.
	/// </summary>
	public bool HasInstrumentMethod { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.RawFileInstrumentMethodAccessor" /> class.
	/// </summary>
	/// <param name="rawFileName">Name of the raw file.</param>
	public RawFileInstrumentMethodAccessor(string rawFileName)
	{
		try
		{
			IViewCollectionManager instance = MemoryMappedRawFileManager.Instance;
			FileInfo fileInfo = new FileInfo(rawFileName);
			long size = (Environment.Is64BitProcess ? 0 : Math.Min(fileInfo.Length, 83886080L));
			Guid guid = Guid.NewGuid();
			string text = Utilities.CorrectNameForEnvironment(rawFileName);
			_accessor = ((IReadWriteAccessor)null).GetMemoryMappedViewer(guid, text, 0L, size, inAcquisition: false, DataFileAccessMode.OpenCreateReadLoaderId);
			if (IsMemoryMappedAccessorValid(_accessor, text, guid))
			{
				long startPos = 0L;
				base.Header = _accessor.LoadRawFileObjectExt<FileHeader>(0, ref startPos);
				_fileRevision = CheckForValidVersion("Instrument Method");
				if (_fileRevision < 25)
				{
					AppendError("File version less than 25 does not support.");
				}
				else
				{
					LoadInstrumentMethod(instance, _accessor, guid, Utilities.GetFileMapName(rawFileName), startPos);
				}
			}
		}
		catch (Exception exception)
		{
			AppendError(exception.ToMessageAndCompleteStacktrace());
		}
	}

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			AppendError("Instrument method exporter has been disposed");
			_accessor.ReleaseAndCloseMemoryMappedFile(MemoryMappedRawFileManager.Instance, forceToCloseMmf: true);
		}
	}

	/// <summary>
	/// Export the instrument method to a file.
	/// Because of the many potential issues with this, use with care, especially if
	/// adding to a customer workflow.
	/// Try catch should be used with this method.
	/// Not all implementations may support this (some may throw NotImplementedException).
	/// .Net exceptions may be thrown, for example if the path is not valid.
	/// Not all instrument methods can be exported, depending on raw file version, and how
	/// the file was acquired. If the "instrument method file name" is not present in the sample information,
	/// then the exported data may not be a complete method file.
	/// Not all exported files can be read by an instrument method editor.
	/// Instrument method editors may only be able to open methods when the exact same list
	/// of instruments is configured.
	/// Code using this feature should handle all cases.
	/// </summary>
	/// <param name="methodFilePath">The output instrument method file path.</param>
	/// <param name="forceOverwrite">Force over write. If true, and file already exists, attempt to delete existing file first.
	/// If false: UnauthorizedAccessException will occur if there is an existing read only file.</param>
	/// <returns>
	/// True if the file was saved. False, if no file was saved, for example,
	/// because there is no instrument method saved in this raw file.
	/// </returns>
	/// <exception cref="T:System.Exception">No open raw file.</exception>
	public bool ExportInstrumentMethod(string methodFilePath, bool forceOverwrite)
	{
		if (base.HasError)
		{
			throw new Exception(base.ErrorMessage);
		}
		try
		{
			if (HasInstrumentMethod)
			{
				_methodInfo.SaveMethodFile(_accessor, methodFilePath, forceOverwrite);
				return true;
			}
		}
		catch (Exception ex)
		{
			AppendError(ex);
		}
		return false;
	}

	/// <summary>
	/// Gets names of all instruments, which have a method stored in the raw file's copy of the instrument method file.
	/// These names are "Device internal names" which map to storage names within
	/// an instrument method, and other instrument data (such as registry keys).
	/// Use "GetAllInstrumentFriendlyNamesFromInstrumentMethod" (in IRawDataPlus) to get display names
	/// for instruments.
	/// </summary>
	/// <returns>
	/// The instrument names.
	/// </returns>
	public string[] GetAllInstrumentNamesFromInstrumentMethod()
	{
		if (base.HasError)
		{
			throw new Exception(base.ErrorMessage);
		}
		if (HasInstrumentMethod && _methodInfo.StorageDescriptions != null && _methodInfo.StorageDescriptions.Any())
		{
			return _methodInfo.StorageDescriptions.Select((StorageDescription sd) => sd.StorageName).ToArray();
		}
		return new string[0];
	}

	/// <summary>
	/// load the instrument method.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="viewer">View into file</param>
	/// <param name="id">RawFileLoader instance ID</param>
	/// <param name="dataFileMapName">Memory mapped file name for Raw file information.</param>
	/// <param name="startPos">offset into view</param>
	private void LoadInstrumentMethod(IViewCollectionManager manager, IDisposableReader viewer, Guid id, string dataFileMapName, long startPos)
	{
		viewer.LoadRawFileObjectExt<SequenceRow>(_fileRevision, ref startPos);
		viewer.LoadRawFileObjectExt<AutoSamplerConfig>(_fileRevision, ref startPos);
		RawFileInfo rawFileInfo = viewer.LoadRawFileObjectExt(() => new RawFileInfo(manager, id, dataFileMapName, _fileRevision), _fileRevision, ref startPos);
		if (rawFileInfo.HasExpMethod)
		{
			_methodInfo = viewer.LoadRawFileObjectExt<Method>(_fileRevision, ref startPos);
		}
		HasInstrumentMethod = rawFileInfo.HasExpMethod && base.Header.Revision >= 25 && _methodInfo != null;
	}

	/// <summary>
	/// Validates the memory mapped accessor.
	/// </summary>
	/// <param name="accessor">Memory mapped file accessor</param>
	/// <param name="rawFileName">Name of the raw file.</param>
	/// <param name="id">The identifier.</param>
	/// <returns>True is </returns>
	private bool IsMemoryMappedAccessorValid(IReadWriteAccessor accessor, string rawFileName, Guid id)
	{
		if (accessor != null)
		{
			return true;
		}
		string streamId = StreamHelper.ConstructStreamId(id, rawFileName);
		string errors = MemoryMappedRawFileManager.Instance.GetErrors(streamId);
		AppendError($"{(string.IsNullOrWhiteSpace(base.ErrorMessage) ? string.Empty : Environment.NewLine)}{errors} {rawFileName}");
		return false;
	}
}
