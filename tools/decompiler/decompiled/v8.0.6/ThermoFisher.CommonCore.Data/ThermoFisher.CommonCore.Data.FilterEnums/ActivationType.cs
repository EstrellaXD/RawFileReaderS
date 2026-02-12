using System.ComponentModel;

namespace ThermoFisher.CommonCore.Data.FilterEnums;

/// <summary>
/// The activation types are used to link a specific precursor mass with an activation type.
/// There are 26 possible mode values, including some reserved values.
/// </summary>
public enum ActivationType
{
	/// <summary>
	/// Collision induced dissociation
	/// </summary>
	CollisionInducedDissociation,
	/// <summary>
	/// Multi-photon dissociation.
	/// </summary>
	MultiPhotonDissociation,
	/// <summary>
	/// Electron-capture dissociation (ECD) is a method of fragmenting gas phase ions
	/// for tandem mass spectrometric analysis (structural elucidation).
	/// </summary>
	ElectronCaptureDissociation,
	/// <summary>
	/// Pulsed-Q Dissociation (PQD) is a proprietary fragmentation technique that eliminates the low mass cut-off
	/// for Thermo Scientific™ linear ion trap mass spectrometers,
	/// facilitating the use of isobaric mass tags for quantitation of proteins.
	/// </summary>
	PQD,
	/// <summary>
	/// Electron transfer dissociation (ETD).
	/// ETD induces fragmentation of cations (e.g. peptides or proteins) by transferring electrons to them.
	/// </summary>
	ElectronTransferDissociation,
	/// <summary>
	/// Higher-energy collisional dissociation.
	/// </summary>
	[Description("hcd")]
	HigherEnergyCollisionalDissociation,
	/// <summary>
	/// Match any activation type
	/// </summary>
	Any,
	/// <summary>
	/// SA activation
	/// </summary>
	SAactivation,
	/// <summary>
	/// Proton transfer reaction
	/// </summary>
	ProtonTransferReaction,
	/// <summary>
	/// Negative electron transfer dissociation
	/// </summary>
	NegativeElectronTransferDissociation,
	/// <summary>
	/// Negative Proton-transfer-reaction
	/// </summary>
	NegativeProtonTransferReaction,
	/// <summary>
	/// Ultra Violet Photo Dissociation
	/// </summary>
	UltraVioletPhotoDissociation,
	/// <summary>
	/// Electron Induced Dissociation.
	/// </summary>
	ElectronInducedDissociation,
	/// <summary>
	/// Electron Energy (an additional value for EID) .
	/// </summary>
	ElectronEnergy,
	/// <summary>
	/// Mode c (reserved) .
	/// </summary>
	ModeC,
	/// <summary>
	/// Mode d (reserved) .
	/// </summary>
	ModeD,
	/// <summary>
	/// Mode e (reserved) .
	/// </summary>
	ModeE,
	/// <summary>
	/// Mode a (reserved) .
	/// </summary>
	ModeF,
	/// <summary>
	/// Mode g (reserved) .
	/// </summary>
	ModeG,
	/// <summary>
	/// Mode h (reserved) .
	/// </summary>
	ModeH,
	/// <summary>
	/// Mode i (reserved) .
	/// </summary>
	ModeI,
	/// <summary>
	/// Mode J (reserved) .
	/// </summary>
	ModeJ,
	/// <summary>
	/// Mode K (defined as NegativeProtonTransferReaction).
	/// </summary>
	ModeK,
	/// <summary>
	/// Mode L (reserved).
	/// </summary>
	ModeL,
	/// <summary>
	/// Mode M (reserved).
	/// </summary>
	ModeM,
	/// <summary>
	/// Mode N (reserved).
	/// </summary>
	ModeN,
	/// <summary>
	/// Mode O (reserved).
	/// </summary>
	ModeO,
	/// <summary>
	/// Mode P (reserved).
	/// </summary>
	ModeP,
	/// <summary>
	/// Mode Q (reserved).
	/// </summary>
	ModeQ,
	/// <summary>
	/// Mode R (reserved).
	/// </summary>
	ModeR,
	/// <summary>
	/// Mode S (reserved).
	/// </summary>
	ModeS,
	/// <summary>
	/// Mode T (reserved).
	/// </summary>
	ModeT,
	/// <summary>
	/// Mode U (reserved).
	/// </summary>
	ModeU,
	/// <summary>
	/// Mode V (reserved).
	/// </summary>
	ModeV,
	/// <summary>
	/// Mode W (reserved).
	/// </summary>
	ModeW,
	/// <summary>
	/// Mode X (reserved).
	/// </summary>
	ModeX,
	/// <summary>
	/// Mode Y (reserved).
	/// </summary>
	ModeY,
	/// <summary>
	/// Mode Z (reserved).
	/// </summary>
	ModeZ,
	/// <summary>
	/// end of list
	/// </summary>
	LastActivation
}
