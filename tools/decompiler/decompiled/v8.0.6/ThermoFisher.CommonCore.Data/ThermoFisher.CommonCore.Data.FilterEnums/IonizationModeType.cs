using System.ComponentModel;

namespace ThermoFisher.CommonCore.Data.FilterEnums;

/// <summary>
/// Specifies ionization mode in scans.
/// </summary>
public enum IonizationModeType
{
	/// <summary>
	/// Mode is electron impact.
	/// </summary>
	ElectronImpact,
	/// <summary>
	/// Mode is chemical ionization.
	/// </summary>
	ChemicalIonization,
	/// <summary>
	/// Mode is fast atom bombardment.
	/// </summary>
	FastAtomBombardment,
	/// <summary>
	/// Mode is electro spray.
	/// </summary>
	ElectroSpray,
	/// <summary>
	/// Mode is atmospheric pressure chemical ionization.
	/// </summary>
	AtmosphericPressureChemicalIonization,
	/// <summary>
	/// Mode is <c>nano spray</c>.
	/// </summary>
	NanoSpray,
	/// <summary>
	/// Mode is thermo spray.
	/// </summary>
	ThermoSpray,
	/// <summary>
	/// Mode is field desorption.
	/// </summary>
	FieldDesorption,
	/// <summary>
	/// Mode is matrix assisted laser desorption ionization.
	/// </summary>
	MatrixAssistedLaserDesorptionIonization,
	/// <summary>
	/// Mode is glow discharge.
	/// </summary>
	GlowDischarge,
	/// <summary>
	/// Mode is any (For filtering only).
	/// If reported by an instrument: Mode was not recorded by instrument.
	/// </summary>
	Any,
	/// <summary>
	/// Paper spray ionization.
	/// </summary>
	[Description("PSI")]
	PaperSprayIonization,
	/// <summary>
	/// Card <c>nanospray</c> ionization.
	/// </summary>
	[Description("cNSI")]
	CardNanoSprayIonization,
	/// <summary>
	/// The extension ionization mode 1.
	/// </summary>
	[Description("IM1")]
	IonizationMode1,
	/// <summary>
	/// The extension ionization mode 2.
	/// </summary>
	[Description("IM2")]
	IonizationMode2,
	/// <summary>
	/// The extension ionization mode 3.
	/// </summary>
	[Description("IM3")]
	IonizationMode3,
	/// <summary>
	/// The extension ionization mode 4.
	/// </summary>
	[Description("IM4")]
	IonizationMode4,
	/// <summary>
	/// The extension ionization mode 5.
	/// </summary>
	[Description("IM5")]
	IonizationMode5,
	/// <summary>
	/// The extension ionization mode 6.
	/// </summary>
	[Description("IM6")]
	IonizationMode6,
	/// <summary>
	/// The extension ionization mode 7.
	/// </summary>
	[Description("IM7")]
	IonizationMode7,
	/// <summary>
	/// The extension ionization mode 8.
	/// </summary>
	[Description("IM8")]
	IonizationMode8,
	/// <summary>
	/// The extension ionization mode 9.
	/// </summary>
	[Description("IM9")]
	IonizationMode9,
	/// <summary>
	/// The ion mode is beyond known types.
	/// </summary>
	IonModeBeyondKnown
}
