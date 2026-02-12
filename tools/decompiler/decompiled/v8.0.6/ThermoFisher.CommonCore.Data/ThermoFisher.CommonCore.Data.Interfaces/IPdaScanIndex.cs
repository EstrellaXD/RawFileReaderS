namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines the scan header for PDA
/// </summary>
public interface IPdaScanIndex : IBaseScanIndex
{
	/// <summary>
	/// Gets the long wavelength.
	/// <para>For UV device only, it will be ignored by Analog devices</para>
	/// </summary>
	double LongWavelength { get; }

	/// <summary>
	/// Gets the short wavelength.
	/// <para>For UV device only, it will be ignored by Analog devices</para>
	/// </summary>
	double ShortWavelength { get; }

	/// <summary>
	/// Gets the wave length step.
	/// </summary>
	double WavelengthStep { get; }

	/// <summary>
	/// Gets the Absorbance Unit's scale.
	/// </summary>
	double AUScale { get; }
}
