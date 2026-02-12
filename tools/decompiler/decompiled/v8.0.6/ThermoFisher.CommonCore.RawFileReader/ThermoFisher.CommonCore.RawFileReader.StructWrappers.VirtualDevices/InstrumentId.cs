using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.RawFileReader.Facade;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices;

/// <summary>
/// The instrument id.
/// This records data about an instrument (such as data units).
/// It does not contain any scans.
/// There is one record, written before data acquisition starts.
/// It never changes.
/// </summary>
internal class InstrumentId : IInstrumentId, IRealTimeAccess, IDisposable, IRawObjectBase, IErrors
{
	private readonly DeviceErrors _errors;

	private readonly Guid _loaderId;

	private InstIdInfoStruct _instrumentId;

	private bool _disposed;

	private IReadWriteAccessor _acqDataViewer;

	private bool _loaded;

	/// <summary>
	/// Gets the error message.
	/// </summary>
	public string ErrorMessage => _errors.ErrorMessage;

	/// <summary>
	/// Gets a value indicating whether this file has detected an error.
	/// If this is false: Other error properties in this interface have no meaning.
	/// </summary>
	public bool HasError => _errors.HasError;

	/// <summary>
	/// Gets or sets the absorbance unit.
	/// </summary>
	public AbsorbanceUnits AbsorbanceUnit
	{
		get
		{
			return _instrumentId.AbsorbanceUnit;
		}
		set
		{
			_instrumentId.AbsorbanceUnit = value;
		}
	}

	public IViewCollectionManager Manager { get; }

	/// <summary>
	/// Gets the channel labels.
	/// </summary>
	public List<KeyValuePair<int, string>> ChannelLabels { get; }

	/// <summary>
	/// Gets the flags.
	/// The purpose of this field is to contain flags separated by ';' that
	/// denote experiment information, etc. For example, if a file is acquired
	/// under instrument control based on an experiment protocol like an ion
	/// mapping experiment, an appropriate flag can be set here.
	/// Legacy flags (from LCQ system)
	///     1. TIM  - total ion map
	///     2. NLM  - neutral loss map
	///     3. PIM  - parent ion map
	///     4. DDZMAP - data dependent zoom map
	/// Newer flags proposed:
	/// high_res_precursors - MS has accurate mass precursors
	/// </summary>
	public string Flags { get; private set; }

	/// <summary>
	/// Gets the hardware rev.
	/// </summary>
	public string HardwareVersion { get; private set; }

	/// <summary>
	/// Gets a value indicating whether the ID is valid.
	/// </summary>
	public bool IsValid
	{
		get
		{
			return _instrumentId.IsValid;
		}
		private set
		{
			_instrumentId.IsValid = value;
		}
	}

	/// <summary>
	/// Gets the model.
	/// </summary>
	public string Model { get; private set; }

	/// <summary>
	/// Gets the name.
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Gets the serial number.
	/// </summary>
	public string SerialNumber { get; private set; }

	/// <summary>
	/// Gets the software rev.
	/// </summary>
	public string SoftwareVersion { get; private set; }

	/// <summary>
	/// Gets the x axis.
	/// </summary>
	public string AxisLabelX { get; private set; }

	/// <summary>
	/// Gets the y axis.
	/// </summary>
	public string AxisLabelY { get; private set; }

	/// <summary>
	/// Gets a value indicating whether this is a TSQ quantum file.
	/// </summary>
	public bool IsTsqQuantumFile
	{
		get
		{
			if (Flags != null && Flags.Contains("high_res_precursors"))
			{
				return true;
			}
			if (string.IsNullOrEmpty(Name))
			{
				return false;
			}
			if (Name.Contains("Quantum"))
			{
				return true;
			}
			if (Name.Contains("TSQ") && Model != "Standard")
			{
				return true;
			}
			if (Name.Contains("Endura"))
			{
				return true;
			}
			return false;
		}
	}

	/// <summary>
	/// Gets the file revision.
	/// </summary>
	public int FileRevision { get; }

	/// <summary>
	/// Gets the header file map name.
	/// </summary>
	public string HeaderFileMapName { get; }

	/// <summary>
	/// Gets the data file map name.
	/// </summary>
	public string DataFileMapName { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.InstrumentId" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">
	/// The loader id.
	/// </param>
	public InstrumentId(IViewCollectionManager manager, Guid loaderId)
	{
		Manager = manager;
		ChannelLabels = new List<KeyValuePair<int, string>>();
		IsValid = false;
		AbsorbanceUnit = AbsorbanceUnits.Unknown;
		Name = string.Empty;
		Model = string.Empty;
		SerialNumber = string.Empty;
		SoftwareVersion = string.Empty;
		HardwareVersion = string.Empty;
		Flags = string.Empty;
		AxisLabelX = string.Empty;
		AxisLabelY = string.Empty;
		_acqDataViewer = null;
		_loaderId = loaderId;
		_errors = new DeviceErrors();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.VirtualDevices.InstrumentId" /> class.
	/// </summary>
	/// <param name="manager">tool to create "views" into the data (to read data)</param>
	/// <param name="loaderId">
	/// The loader id.
	/// </param>
	/// <param name="mapName">
	/// The map name.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	public InstrumentId(IViewCollectionManager manager, Guid loaderId, string mapName, int fileRevision)
		: this(manager, loaderId)
	{
		FileRevision = fileRevision;
		HeaderFileMapName = string.Empty;
		DataFileMapName = mapName;
	}

	/// <summary>
	/// load (from file)
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="dataOffset">
	/// The data offset.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes loaded.
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		ChannelLabels.Clear();
		if (fileRevision >= 45)
		{
			_instrumentId = viewer.ReadStructureExt<InstIdInfoStruct>(ref startPos);
		}
		else
		{
			_instrumentId = viewer.ReadPreviousRevisionAndConvertExt<InstIdInfoStruct, OldInstidinfo1>(ref startPos);
			_instrumentId.AbsorbanceUnit = AbsorbanceUnits.Unknown;
		}
		string[] array = viewer.ReadStringsExt(ref startPos);
		for (int i = 0; i < array.Length; i++)
		{
			ChannelLabels.Add(new KeyValuePair<int, string>(i + 1, array[i]));
		}
		Name = viewer.ReadStringExt(ref startPos);
		Model = viewer.ReadStringExt(ref startPos);
		SerialNumber = viewer.ReadStringExt(ref startPos);
		SoftwareVersion = viewer.ReadStringExt(ref startPos);
		HardwareVersion = viewer.ReadStringExt(ref startPos);
		Flags = ((fileRevision >= 32) ? viewer.ReadStringExt(ref startPos) : string.Empty);
		if (fileRevision >= 37)
		{
			AxisLabelX = viewer.ReadStringExt(ref startPos);
			AxisLabelY = viewer.ReadStringExt(ref startPos);
		}
		_loaded = true;
		return startPos - dataOffset;
	}

	/// <summary>
	/// Re-read the current file, to get the latest data.
	/// Only meaningful if the object has an implied backing file (such as IO.DLL and .raw files)
	/// No-op otherwise
	/// </summary>
	/// <returns>True refresh succeed, false otherwise </returns>
	public bool RefreshViewOfFile()
	{
		if (_loaded && _instrumentId.IsValid)
		{
			return true;
		}
		try
		{
			_acqDataViewer = _acqDataViewer.GetMemoryMappedViewer(_loaderId, DataFileMapName, inAcquisition: true, DataFileAccessMode.OpenCreateReadLoaderId);
			if (_acqDataViewer != null)
			{
				Load(_acqDataViewer, 0L, FileRevision);
				return true;
			}
		}
		catch (Exception ex)
		{
			_errors.UpdateError(ex.ToMessageAndCompleteStacktrace(), ex.HResult);
		}
		return false;
	}

	/// <summary>
	/// The method disposes all the memory mapped files still being tracked (i.e. still open).
	/// </summary>
	public void Dispose()
	{
		if (!_disposed)
		{
			_disposed = true;
			_acqDataViewer.ReleaseAndCloseMemoryMappedFile(Manager);
		}
	}
}
