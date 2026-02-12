using System.Collections.ObjectModel;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Defines ion ratio settings, as imported from Xcalibur PMD file
/// </summary>
public interface IXcaliburIonRatioTestSettingsAccess
{
	/// <summary>
	/// Gets a value indicating whether IRC tests are enabled
	/// </summary>
	bool Enabled { get; }

	/// <summary>
	/// Gets the "standard" used 
	/// </summary>
	int Standard { get; }

	/// <summary>
	/// Gets the Ion Ratio method
	/// </summary>
	IonRatioMethod Method { get; }

	/// <summary>
	/// Gets the ion ratio window type
	/// </summary>
	XcaliburIonRatioWindowType WindowType { get; }

	/// <summary>
	/// Gets the qualifier ion coelution limits (minutes)
	/// </summary>
	double QualifierIonCoelution { get; }

	/// <summary>
	/// Gets the table of masses for ion ratio testing
	/// </summary>
	ReadOnlyCollection<IIonRatioConfirmationTestAccess> IonRatioConfirmationTests { get; }
}
