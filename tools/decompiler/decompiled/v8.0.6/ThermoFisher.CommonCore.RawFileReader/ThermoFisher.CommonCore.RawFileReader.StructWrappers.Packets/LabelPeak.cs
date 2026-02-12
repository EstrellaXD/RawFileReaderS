namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets;

/// <summary>
/// The label peak information.
/// Use struct here for performance:
/// Since there are "many per scan" and "many scans per file"
/// Over 10 million of these objects can be created per second
/// when processing raw files, so if "class" is used
/// This would cause frequent garbage collection, and kills parallel code
/// </summary>
internal struct LabelPeak
{
	private byte _flags;

	/// <summary>
	/// Gets or sets the mass.
	/// </summary>
	public double Mass { get; set; }

	/// <summary>
	/// Gets or sets the intensity.
	/// </summary>
	public float Intensity { get; set; }

	/// <summary>
	/// Gets or sets the resolution.
	/// </summary>
	public float Resolution { get; set; }

	/// <summary>
	/// Gets or sets the baseline.
	/// </summary>
	public float Baseline { get; set; }

	/// <summary>
	/// Gets or sets the noise.
	/// </summary>
	public float Noise { get; set; }

	/// <summary>
	/// Gets or sets the charge.
	/// </summary>
	public byte Charge { get; set; }

	/// <summary>
	/// Gets a value indicating whether the peak is an exception.
	/// </summary>
	public bool IsException => (_flags & 8) > 0;

	/// <summary>
	/// Gets a value indicating whether the peak is a reference.
	/// </summary>
	public bool IsReference => (_flags & 4) > 0;

	/// <summary>
	/// Gets or sets the flags.
	/// </summary>
	public byte Flags
	{
		get
		{
			return _flags;
		}
		set
		{
			_flags = value;
		}
	}
}
