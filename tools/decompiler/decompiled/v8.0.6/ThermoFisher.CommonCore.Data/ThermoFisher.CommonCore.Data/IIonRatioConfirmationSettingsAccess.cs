using System.Collections.ObjectModel;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Read only access to Ion ratio settings
/// </summary>
public interface IIonRatioConfirmationSettingsAccess
{
	/// <summary>
	/// Gets the time the retention time can vary from the expected retention time for the ion to still be considered confirmed.
	/// Units: minutes
	/// Bounds: 0.000 - 0.100  
	/// </summary>
	double QualifierIonCoelution { get; }

	/// <summary>
	/// Gets a value indicating whether this Ion Ratio Confirmation is enabled.
	/// </summary>
	/// <value><c>true</c> if enable; otherwise, <c>false</c>.</value>
	bool Enable { get; }

	/// <summary>
	/// Gets the type of the windows.
	/// </summary>
	/// <value>The type of the windows.</value>
	IonRatioWindowType WindowsType { get; }

	/// <summary>
	/// Gets the qualifier ions.
	/// </summary>
	/// <value>The qualifier ions.</value>
	ReadOnlyCollection<IIonRatioConfirmationMassSettingsAccess> QualifierIons { get; }
}
