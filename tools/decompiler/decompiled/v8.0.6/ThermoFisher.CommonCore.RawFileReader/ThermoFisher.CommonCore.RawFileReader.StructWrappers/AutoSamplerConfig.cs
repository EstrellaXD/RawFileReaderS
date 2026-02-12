using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The auto sampler config.
/// </summary>
internal sealed class AutoSamplerConfig : IAutoSamplerConfig, IRawObjectBase
{
	private AutoSamplerConfigStruct _autoSamplerConfig;

	/// <summary>
	/// Gets or sets the tray index.
	/// </summary>
	public int TrayIndex
	{
		get
		{
			return _autoSamplerConfig.TrayIndex;
		}
		set
		{
			_autoSamplerConfig.TrayIndex = value;
		}
	}

	/// <summary>
	/// Gets or sets the tray name.
	/// </summary>
	public string TrayName { get; set; }

	/// <summary>
	/// Gets or sets the tray shape.
	/// </summary>
	public TrayShape TrayShape
	{
		get
		{
			return (TrayShape)_autoSamplerConfig.TrayShape;
		}
		set
		{
			_autoSamplerConfig.TrayShape = (int)value;
		}
	}

	/// <summary>
	/// Gets or sets the vial index.
	/// </summary>
	public int VialIndex
	{
		get
		{
			return _autoSamplerConfig.VialIndex;
		}
		set
		{
			_autoSamplerConfig.VialIndex = value;
		}
	}

	/// <summary>
	/// Gets or sets the vials per tray.
	/// </summary>
	public int VialsPerTray
	{
		get
		{
			return _autoSamplerConfig.VialsPerTray;
		}
		set
		{
			_autoSamplerConfig.VialsPerTray = value;
		}
	}

	/// <summary>
	/// Gets or sets the vials per tray x.
	/// </summary>
	public int VialsPerTrayX
	{
		get
		{
			return _autoSamplerConfig.VialsPerTrayX;
		}
		set
		{
			_autoSamplerConfig.VialsPerTrayX = value;
		}
	}

	/// <summary>
	/// Gets or sets the vials per tray y.
	/// </summary>
	public int VialsPerTrayY
	{
		get
		{
			return _autoSamplerConfig.VialsPerTrayY;
		}
		set
		{
			_autoSamplerConfig.VialsPerTrayY = value;
		}
	}

	/// <summary>
	/// Gets the auto sampler configuration struct.
	/// </summary>
	public AutoSamplerConfigStruct AutoSamplerConfigStruct => _autoSamplerConfig;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.AutoSamplerConfig" /> class.
	/// </summary>
	public AutoSamplerConfig()
	{
		TrayName = string.Empty;
		Initialization();
	}

	/// <summary>
	/// Loads the specified viewer.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <returns>The number of bytes read</returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		if (fileRevision >= 36)
		{
			_autoSamplerConfig = viewer.ReadStructureExt<AutoSamplerConfigStruct>(ref startPos);
			TrayName = viewer.ReadStringExt(ref startPos);
		}
		else
		{
			Initialization();
		}
		TrayShape = (TrayShape)_autoSamplerConfig.TrayShape;
		return startPos - dataOffset;
	}

	/// <summary>
	/// Initializations this instance.
	/// </summary>
	private void Initialization()
	{
		_autoSamplerConfig = new AutoSamplerConfigStruct
		{
			TrayIndex = -1,
			VialIndex = -1,
			VialsPerTray = -1,
			VialsPerTrayX = -1,
			VialsPerTrayY = -1,
			TrayShape = 4
		};
	}
}
