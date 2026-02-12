namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Extended chromatogram data: Includes base peak information.
/// For PDA data: Base value is "Wavelength of max" for a "spectrum max" chromatogram.
/// </summary>
public interface IChromatogramDataPlus : IChromatogramData, IChromatogramBasePeaks
{
}
