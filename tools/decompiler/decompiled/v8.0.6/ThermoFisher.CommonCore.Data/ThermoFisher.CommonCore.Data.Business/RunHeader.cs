using System;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The run header.
/// </summary>
[Serializable]
public class RunHeader : IRunHeaderAccess
{
	/// <summary>
	/// Gets or sets the number for the first scan in this stream (usually 1)
	/// </summary>
	public int FirstSpectrum { get; set; }

	/// <summary>
	/// Gets or sets the number for the last scan in this stream
	/// </summary>
	public int LastSpectrum { get; set; }

	/// <summary>
	/// Gets or sets the time of first scan in file
	/// </summary>
	public double StartTime { get; set; }

	/// <summary>
	/// Gets or sets the time of last scan in file
	/// </summary>
	public double EndTime { get; set; }

	/// <summary>
	/// Gets or sets the lowest recorded mass in file
	/// </summary>
	public double LowMass { get; set; }

	/// <summary>
	/// Gets or sets the highest recorded mass in file
	/// </summary>
	public double HighMass { get; set; }

	/// <summary>
	/// Gets or sets the mass resolution value recorded for the current instrument. 
	/// The value is returned as one half of the mass resolution. 
	/// For example, a unit resolution controller would return a value of 0.5.
	/// </summary>
	public double MassResolution { get; set; }

	/// <summary>
	/// Gets or sets the expected acquisition run time for the current instrument.
	/// </summary>double ExpectedRunTime { get; }
	public double ExpectedRuntime { get; set; }

	/// <summary>
	/// Gets or sets the max integrated intensity.
	/// </summary>
	public double MaxIntegratedIntensity { get; set; }

	/// <summary>
	/// Gets or sets the max intensity.
	/// </summary>
	public int MaxIntensity { get; set; }

	/// <summary>
	/// Gets or sets the tolerance unit.
	/// </summary>
	public ToleranceUnits ToleranceUnit { get; set; }

	/// <summary>
	/// construct object
	/// </summary>
	public RunHeader()
	{
	}

	/// <summary>
	/// construct object (clone from interface)
	/// </summary>
	/// <param name="runHeader">clone this interface</param>
	public RunHeader(IRunHeaderAccess runHeader)
	{
		if (runHeader == null)
		{
			throw new ArgumentNullException("runHeader");
		}
		FirstSpectrum = runHeader.FirstSpectrum;
		LastSpectrum = runHeader.LastSpectrum;
		StartTime = runHeader.StartTime;
		EndTime = runHeader.EndTime;
		LowMass = runHeader.LowMass;
		HighMass = runHeader.HighMass;
		MassResolution = runHeader.MassResolution;
		ExpectedRuntime = runHeader.ExpectedRuntime;
		MaxIntegratedIntensity = runHeader.MaxIntegratedIntensity;
		MaxIntensity = runHeader.MaxIntensity;
		ToleranceUnit = runHeader.ToleranceUnit;
	}
}
