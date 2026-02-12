using System;
using System.Collections.Generic;
using System.Threading;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.RawFileReader.DataModel;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Facade;

/// <summary>
/// The sequence file loader. Loads data from SLD file.
/// </summary>
internal class SequenceFileLoader : LoaderBase
{
	private readonly string _streamId;

	/// <summary>
	/// Gets or sets the id.
	/// </summary>
	private Guid Id { get; set; }

	/// <summary>
	/// Gets a value indicating whether this instance is open.
	/// </summary>
	/// <value>
	///   <c>true</c> if this instance is open; otherwise, <c>false</c>.
	/// </value>
	public bool IsOpen { get; private set; }

	/// <summary>
	/// Gets the sequence info.
	/// </summary>
	public SequenceFileInfoStruct SequenceInfo { get; private set; }

	/// <summary>
	/// Gets or sets the sequence file name.
	/// </summary>
	private string SequenceFileName { get; set; }

	/// <summary>
	/// Gets the samples.
	/// </summary>
	public List<SampleInformation> Samples { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.SequenceFileLoader" /> class. 
	/// Must be called prior to data access
	/// </summary>
	/// <param name="fileName">
	/// The file path.
	/// </param>
	/// <exception cref="T:System.ArgumentException">
	/// The file path is empty or null.
	/// </exception>
	/// <exception cref="T:System.Exception">
	/// A problem encountered when reading the raw file.
	/// </exception>
	public SequenceFileLoader(string fileName)
	{
		ClearAllErrorsAndWarnings();
		Id = Guid.NewGuid();
		IReadWriteAccessor readWriteAccessor = null;
		Mutex mutex = null;
		try
		{
			mutex = Utilities.CreateNamedMutexAndWait(fileName);
			if (mutex != null)
			{
				SequenceFileName = fileName;
				_streamId = StreamHelper.ConstructStreamId(Id, fileName);
				readWriteAccessor = MemoryMappedRawFileManager.Instance.GetRandomAccessViewer(Id, fileName, inAcquisition: false, DataFileAccessMode.OpenCreateReadLoaderId);
				LoadSequenceFile(readWriteAccessor);
			}
		}
		catch (Exception exception)
		{
			UpdateError($"Encountered problems while trying to read '{fileName}' as a Sequence File!{Environment.NewLine}{exception.ToMessageAndCompleteStacktrace()}");
		}
		finally
		{
			try
			{
				IViewCollectionManager instance = MemoryMappedRawFileManager.Instance;
				if (instance != null && readWriteAccessor != null)
				{
					string streamId = readWriteAccessor.StreamId;
					AppendError(instance.GetErrors(streamId));
					IsOpen = instance.IsOpen(streamId);
				}
			}
			catch
			{
			}
			if (mutex != null)
			{
				mutex.ReleaseMutex();
				mutex.Close();
			}
			IViewCollectionManager instance2 = MemoryMappedRawFileManager.Instance;
			readWriteAccessor?.ReleaseAndCloseMemoryMappedFile(instance2);
			instance2.Close(_streamId);
		}
	}

	/// <summary>
	/// Load sequence file.
	/// </summary>
	/// <param name="viewer">
	/// The viewer. (memory mapped file)
	/// </param>
	private void LoadSequenceFile(IReadWriteAccessor viewer)
	{
		long startPos = 0L;
		if (viewer == null)
		{
			AppendError($"{MemoryMappedRawFileManager.Instance.GetErrors(_streamId)} {SequenceFileName}");
			return;
		}
		base.Header = viewer.LoadRawFileObjectExt(() => new ThermoFisher.CommonCore.RawFileReader.StructWrappers.FileHeader(), 0, ref startPos);
		int num = CheckForValidVersion("Sequence");
		startPos = ReadMetaData(viewer, startPos, num);
		int num2 = viewer.ReadIntExt(ref startPos);
		Samples = new List<SampleInformation>(num2);
		for (int num3 = 0; num3 < num2; num3++)
		{
			Samples.Add(new WrappedSequenceRow(viewer.LoadRawFileObjectExt(() => new SequenceRow(), num, ref startPos)));
		}
	}

	/// <summary>
	/// Read meta data.
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start of the data (index into map)
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The offset into the map after this data
	/// </returns>
	private long ReadMetaData(IReadWriteAccessor viewer, long startPos, int fileRevision)
	{
		SequenceInfo = viewer.LoadRawFileObjectExt(() => new SequenceFileInfoStruct(), fileRevision, ref startPos);
		return startPos;
	}
}
