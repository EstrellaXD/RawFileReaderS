namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Read only access to run header
/// </summary>
public interface IRunHeaderAccess
{
	/// <summary>
	/// Gets the number for the first scan in this stream (usually 1)
	/// </summary>
	int FirstSpectrum { get; }

	/// <summary>
	/// Gets the number for the last scan in this stream
	/// </summary>
	int LastSpectrum { get; }

	/// <summary>
	/// Gets the time of first scan in file
	/// </summary>
	double StartTime { get; }

	/// <summary>
	/// Gets the time of last scan in file
	/// </summary>
	double EndTime { get; }

	/// <summary>
	/// Gets the lowest recorded mass in file
	/// </summary>
	double LowMass { get; }

	/// <summary>
	/// Gets the highest recorded mass in file
	/// </summary>
	double HighMass { get; }

	/// <summary>
	/// Gets the mass resolution value recorded for the current instrument. 
	/// The value is returned as one half of the mass resolution. 
	/// For example, a unit resolution controller would return a value of 0.5.
	/// </summary>
	double MassResolution { get; }

	/// <summary>
	/// Gets the expected acquisition run time for the current instrument.
	/// </summary>double ExpectedRunTime { get; }
	double ExpectedRuntime { get; }

	/// <summary>
	/// Gets the max integrated intensity.
	/// </summary>
	double MaxIntegratedIntensity { get; }

	/// <summary>
	/// Gets the max intensity.
	/// </summary>
	int MaxIntensity { get; }

	/// <summary>
	/// Gets or the tolerance unit.
	/// </summary>
	ToleranceUnits ToleranceUnit { get; }
}
