using System;
using System.Collections.ObjectModel;
using System.Threading;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.OldLCQ;

namespace ThermoFisher.CommonCore.RawFileReader.Facade;

/// <summary>
/// The processing method file loader.
/// Loads data from PMD file
/// </summary>
internal class ProcessingMethodFileLoader : LoaderBase
{
	private readonly string _streamId;

	/// <summary>
	/// Gets the id.
	/// </summary>
	private Guid Id { get; }

	/// <summary>
	/// Gets a value indicating whether this instance is open.
	/// </summary>
	/// <value>
	///   <c>true</c> if this instance is open; otherwise, <c>false</c>.
	/// </value>
	public bool IsOpen { get; private set; }

	/// <summary>
	/// Gets the processing method file name.
	/// </summary>
	public string ProcessingMethodFileName { get; }

	/// <summary>
	/// Gets the view type.
	/// </summary>
	public ProcessingMethodViewType ViewType { get; private set; }

	/// <summary>
	/// Gets the raw file name.
	/// </summary>
	public string RawFileName { get; private set; }

	/// <summary>
	/// Gets the processing method options.
	/// </summary>
	public IProcessingMethodOptionsAccess ProcessingMethodOptions { get; private set; }

	/// <summary>
	/// Gets Options for "standard reports"
	/// </summary>
	public ProcessingMethodStandardReport StandardReport { get; private set; }

	/// <summary>
	/// Gets the peak detection.
	/// </summary>
	public QualitativePeakDetection PeakDetection { get; private set; }

	/// <summary>
	/// Gets the spec enhancement options.
	/// </summary>
	public SpectrumEnhancementOptions SpecEnhancementOptions { get; private set; }

	/// <summary>
	/// Gets the library search.
	/// </summary>
	public LibrarySearchOptions LibrarySearch { get; private set; }

	/// <summary>
	/// Gets the library constraints.
	/// </summary>
	public LibrarySearchConstraints LibraryConstraints { get; private set; }

	/// <summary>
	/// Gets the sample reports.
	/// </summary>
	public ReadOnlyCollection<IXcaliburSampleReportAccess> SampleReports { get; private set; }

	/// <summary>
	/// Gets the programs.
	/// </summary>
	public ReadOnlyCollection<IXcaliburProgramAccess> Programs { get; private set; }

	/// <summary>
	/// Gets the summary reports.
	/// </summary>
	public ReadOnlyCollection<IXcaliburReportAccess> SummaryReports { get; private set; }

	/// <summary>
	/// Gets the peak display options.
	/// </summary>
	public IPeakDisplayOptions PeakDisplayOptions { get; private set; }

	/// <summary>
	/// Gets the peak purity.
	/// </summary>
	public IPeakPuritySettingsAccess PeakPurity { get; private set; }

	/// <summary>
	/// Gets the components.
	/// </summary>
	public ReadOnlyCollection<IXcaliburComponentAccess> Components { get; private set; }

	/// <summary>
	/// Gets or sets the mass options.
	/// </summary>
	public IMassOptionsAccess MassOptions
	{
		get
		{
			return PeakDetection.MassOptions;
		}
		set
		{
			PeakDetection.MassOptions = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Facade.ProcessingMethodFileLoader" /> class. 
	/// Default constructor initializes a new instance of ProcessingMethodFileLoader class.
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
	public ProcessingMethodFileLoader(string fileName)
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
				ProcessingMethodFileName = fileName;
				_streamId = StreamHelper.ConstructStreamId(Id, fileName);
				readWriteAccessor = MemoryMappedRawFileManager.Instance.GetRandomAccessViewer(Id, fileName, inAcquisition: false, DataFileAccessMode.OpenCreateReadLoaderId);
				LoadProcessingMethodFile(readWriteAccessor);
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
	/// Load data from a processing method file.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	private void LoadProcessingMethodFile(IReadWriteAccessor viewer)
	{
		long startPos = 0L;
		if (viewer == null)
		{
			AppendError(string.IsNullOrWhiteSpace(base.ErrorMessage) ? string.Empty : Environment.NewLine);
			AppendError($"{MemoryMappedRawFileManager.Instance.GetErrors(_streamId)} {ProcessingMethodFileName}");
			return;
		}
		base.Header = viewer.LoadRawFileObjectExt<FileHeader>(0, ref startPos);
		int num = CheckForValidVersion("Processing Method");
		startPos = ReadProcessingMethodOptions(viewer, startPos, num);
		startPos = ReadStandardReport(viewer, startPos, num);
		startPos = ReadPeakDetection(viewer, startPos, num);
		startPos = ReadRtChro(viewer, startPos, num);
		if (num >= 25)
		{
			startPos = ReadSpecEnhancement(viewer, startPos, num);
			startPos = ReadLibrarySearch(viewer, startPos, num);
			startPos = ReadReports(viewer, startPos, num);
			startPos = ReadPrograms(viewer, startPos, num);
			startPos = ReadSummaryReports(viewer, startPos, num);
		}
		else
		{
			SpecEnhancementOptions = new SpectrumEnhancementOptions();
			LibrarySearch = new LibrarySearchOptions();
			LibraryConstraints = new LibrarySearchConstraints();
			SampleReports = new ReadOnlyCollection<IXcaliburSampleReportAccess>(Array.Empty<IXcaliburSampleReportAccess>());
			Programs = new ReadOnlyCollection<IXcaliburProgramAccess>(Array.Empty<IXcaliburProgramAccess>());
			SummaryReports = new ReadOnlyCollection<IXcaliburReportAccess>(Array.Empty<IXcaliburReportAccess>());
		}
		if (num >= 7)
		{
			PeakDisplayOptions = viewer.LoadRawFileObjectExt<PeakDisplayOptions>(num, ref startPos);
		}
		else
		{
			PeakDisplayOptions = new PeakDisplayOptions();
		}
		PeakPurity = ((num >= 38) ? viewer.LoadRawFileObjectExt<PeakPurity>(num, ref startPos) : new PeakPurity());
		startPos = SkipLegacyCustomReports(viewer, startPos, num);
		int num2 = viewer.ReadIntExt(ref startPos);
		IXcaliburComponentAccess[] array = new IXcaliburComponentAccess[num2];
		for (int i = 0; i < num2; i++)
		{
			QuanComponent quanComponent = viewer.LoadRawFileObjectExt<QuanComponent>(num, ref startPos);
			quanComponent.Options = ProcessingMethodOptions;
			quanComponent.PeakDetection = PeakDetection;
			array[i] = quanComponent;
		}
		Components = new ReadOnlyCollection<IXcaliburComponentAccess>(array);
		RawFileName = viewer.ReadStringExt(ref startPos);
		ViewType = (ProcessingMethodViewType)viewer.ReadIntExt(ref startPos);
	}

	/// <summary>
	/// skip any legacy custom reports (not imported)
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position in the view.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The updated map offset
	/// </returns>
	private long SkipLegacyCustomReports(IReadWriteAccessor viewer, long startPos, int fileRevision)
	{
		viewer.LoadRawFileObjectArray<CustomReport>(fileRevision, ref startPos);
		return startPos;
	}

	/// <summary>
	/// Read the summary reports
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position in the view.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The updated map offset
	/// </returns>
	private long ReadSummaryReports(IReadWriteAccessor viewer, long startPos, int fileRevision)
	{
		int num = viewer.ReadIntExt(ref startPos);
		IXcaliburReportAccess[] array = new IXcaliburReportAccess[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = viewer.LoadRawFileObjectExt<SummaryReport>(fileRevision, ref startPos);
		}
		SummaryReports = new ReadOnlyCollection<IXcaliburReportAccess>(array);
		return startPos;
	}

	/// <summary>
	/// Read the programs (EXE).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position in the view.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The updated map offset
	/// </returns>
	private long ReadPrograms(IReadWriteAccessor viewer, long startPos, int fileRevision)
	{
		int num = viewer.ReadIntExt(ref startPos);
		IXcaliburProgramAccess[] array = new IXcaliburProgramAccess[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = viewer.LoadRawFileObjectExt<Program>(fileRevision, ref startPos);
		}
		Programs = new ReadOnlyCollection<IXcaliburProgramAccess>(array);
		return startPos;
	}

	/// <summary>
	/// Read the report names.
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position in the view.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The updated map offset
	/// </returns>
	private long ReadReports(IReadWriteAccessor viewer, long startPos, int fileRevision)
	{
		int num = viewer.ReadIntExt(ref startPos);
		IXcaliburSampleReportAccess[] array = new IXcaliburSampleReportAccess[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = viewer.LoadRawFileObjectExt<SampleReport>(fileRevision, ref startPos);
		}
		SampleReports = new ReadOnlyCollection<IXcaliburSampleReportAccess>(array);
		return startPos;
	}

	/// <summary>
	/// Read library search settings
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position, in the map
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The updated start, for the next object
	/// </returns>
	private long ReadLibrarySearch(IReadWriteAccessor viewer, long startPos, int fileRevision)
	{
		LibrarySearch = viewer.LoadRawFileObjectExt<LibrarySearchOptions>(fileRevision, ref startPos);
		LibraryConstraints = viewer.LoadRawFileObjectExt<LibrarySearchConstraints>(fileRevision, ref startPos);
		return startPos;
	}

	/// <summary>
	/// Read spec enhancement settings
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map).
	/// </param>
	/// <param name="startPos">
	/// The start position, in the map.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The updated start, for the next object
	/// </returns>
	private long ReadSpecEnhancement(IReadWriteAccessor viewer, long startPos, int fileRevision)
	{
		SpecEnhancementOptions = viewer.LoadRawFileObjectExt<SpectrumEnhancementOptions>(fileRevision, ref startPos);
		return startPos;
	}

	/// <summary>
	/// read processing method options.
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map)
	/// </param>
	/// <param name="startPos">
	/// The start position, in the map.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The updated start, for the next object
	/// </returns>
	private long ReadProcessingMethodOptions(IReadWriteAccessor viewer, long startPos, int fileRevision)
	{
		ProcessingMethodOptions = viewer.LoadRawFileObjectExt<ProcessingMethodOptions>(fileRevision, ref startPos);
		return startPos;
	}

	/// <summary>
	/// Read standard report options
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map).
	/// </param>
	/// <param name="startPos">
	/// The start position, in the map.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The updated start, for the next object.
	/// </returns>
	private long ReadStandardReport(IReadWriteAccessor viewer, long startPos, int fileRevision)
	{
		StandardReport = viewer.LoadRawFileObjectExt<ProcessingMethodStandardReport>(fileRevision, ref startPos);
		return startPos;
	}

	/// <summary>
	/// Read peak detection settings
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map).
	/// </param>
	/// <param name="startPos">
	/// The start position, in the map.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The updated start, for the next object.
	/// </returns>
	private long ReadPeakDetection(IReadWriteAccessor viewer, long startPos, int fileRevision)
	{
		PeakDetection = viewer.LoadRawFileObjectExt<QualitativePeakDetection>(fileRevision, ref startPos);
		PeakDetection.Options = ProcessingMethodOptions;
		return startPos;
	}

	/// <summary>
	/// Read and skip real time report settings
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map).
	/// </param>
	/// <param name="startPos">
	/// The start position, in the map.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The updated start, for the next object.
	/// </returns>
	private long ReadRtChro(IReadWriteAccessor viewer, long startPos, int fileRevision)
	{
		viewer.LoadRawFileObjectExt<RealTimeChroInformation>(fileRevision, ref startPos);
		return startPos;
	}
}
