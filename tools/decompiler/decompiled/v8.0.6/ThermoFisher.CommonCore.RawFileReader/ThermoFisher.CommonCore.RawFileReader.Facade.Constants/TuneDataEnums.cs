namespace ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

/// <summary>
/// The tune data enumerations, for legacy LCQ files.
/// </summary>
internal static class TuneDataEnums
{
	/// <summary>
	/// The source type.
	/// </summary>
	public enum SourceType
	{
		/// <summary>
		/// The unknown.
		/// </summary>
		Unknown,
		/// <summary>
		/// electron impact.
		/// </summary>
		Ei,
		/// <summary>
		/// chemical ionization.
		/// </summary>
		Ci,
		/// <summary>
		/// electro-spray ionization.
		/// </summary>
		Esi,
		/// <summary>
		/// atmospheric pressure chemical ionization.
		/// </summary>
		Apci,
		/// <summary>
		/// laser desorption.
		/// </summary>
		Ld,
		/// <summary>
		/// fast atom bombardment.
		/// </summary>
		Fab,
		/// <summary>
		/// particle beam.
		/// </summary>
		Pb,
		/// <summary>
		/// thermo-spray ionization.
		/// </summary>
		Tms
	}
}
