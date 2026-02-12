using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.RawFileReader.ExtensionMethods;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

/// <summary>
/// Peak information (can be profile point, centroid etc).
/// Because a raw file can contain over 100 million profile points
/// this must be a "struct" not a "class", or we drive the garbage collector wild
/// and massively degrade performance on large files.
/// </summary>
internal struct DataPeak
{
	private readonly double _position;

	private readonly double _frequency;

	private double _intensity;

	private PeakOptions _options;

	/// <summary>
	/// Gets the position. This value must be set by a constructor
	/// </summary>
	public double Position => _position;

	/// <summary>
	/// Gets or sets the intensity.
	/// </summary>
	public double Intensity
	{
		get
		{
			return _intensity;
		}
		set
		{
			_intensity = value;
		}
	}

	/// <summary>
	/// Gets or sets the options.
	/// </summary>
	public PeakOptions Options
	{
		get
		{
			return _options;
		}
		set
		{
			_options = value;
		}
	}

	/// <summary>
	/// Gets the frequency. Must be set by constitution.
	/// </summary>
	public double Frequency => _frequency;

	/// <summary>
	/// Gets a value indicating whether this peak is reference or exception.
	/// </summary>
	public bool IsReferenceOrException => (Options & (PeakOptions.Exception | PeakOptions.Reference)) != 0;

	/// <summary>
	/// Gets a value indicating whether this peak is saturated.
	/// </summary>
	public bool IsSaturated => (Options & PeakOptions.Saturated) != 0;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.DataPeak" /> struct. 
	/// </summary>
	/// <param name="labelPeak">
	/// The label peak.
	/// </param>
	public DataPeak(LabelPeak labelPeak)
	{
		this = default(DataPeak);
		_position = labelPeak.Mass;
		_intensity = labelPeak.Intensity;
		_options = labelPeak.ToPeakOptions();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.DataPeak" /> struct. 
	/// With a given mass and intensity.
	/// </summary>
	/// <param name="mass">
	/// The mass.
	/// </param>
	/// <param name="intensity">
	/// The intensity.
	/// </param>
	public DataPeak(double mass, double intensity)
	{
		this = default(DataPeak);
		_position = mass;
		_intensity = intensity;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.DataPeak" /> struct. 
	/// With a given mass and 0 intensity.
	/// </summary>
	/// <param name="mass">
	/// The mass.
	/// </param>
	public DataPeak(double mass)
	{
		this = default(DataPeak);
		_position = mass;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.DataPeak" /> struct. 
	/// With a given mass intensity and frequency.
	/// </summary>
	/// <param name="mass">
	/// The mass.
	/// </param>
	/// <param name="intensity">
	/// The intensity.
	/// </param>
	/// <param name="frequency">
	/// The frequency.
	/// </param>
	public DataPeak(double mass, float intensity, double frequency)
	{
		this = default(DataPeak);
		_position = mass;
		_intensity = intensity;
		_frequency = frequency;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.DataPeak" /> struct. 
	/// With a given mass and frequency.
	/// The "bool" parameter is a performance trick needed in C#.
	/// Since there is already a version with "double, double" adding a third (unused parameter)
	/// creates an overload.
	/// This is more efficient that using the 3 parameter constructor and
	/// passing "0.0" for intensity, as an additional assignment is needed.
	/// This overload is called millions of times from AddZeroPackets
	/// </summary>
	/// <param name="mass">
	/// The mass.
	/// </param>
	/// <param name="frequency">
	/// The frequency.
	/// </param>
	/// <param name="noIntensityOverload">
	/// The intensity.
	/// </param>
	public DataPeak(double mass, double frequency, bool noIntensityOverload)
	{
		this = default(DataPeak);
		_position = mass;
		_frequency = frequency;
	}
}
