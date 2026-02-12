using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines a precursor mass, and means of fragmenting it
/// </summary>
public class Precursor : IComparable<Precursor>
{
	/// <summary>
	/// Gets or sets the mass of the precursor
	/// </summary>
	public double Mass { get; set; }

	/// <summary>
	/// Gets or sets the method used to fragment the precursor
	/// </summary>
	public string ActivationCode { get; set; }

	/// <summary>
	/// Gets or sets the level of activation to fragment the precursor
	/// </summary>
	public double ActivationEnergy { get; set; }

	/// <summary>
	/// Gets or sets the precision of this value (number of decimal places found when parsing)
	/// </summary>
	public int Precision { get; set; }

	/// <summary>
	/// Compares the current object with another object of the same type.
	/// </summary>
	/// <returns>
	/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: 
	///                     Value 
	///                     Meaning 
	///                     Less than zero 
	///                     This object is less than the <paramref name="other" /> parameter.
	///                     Zero 
	///                     This object is equal to <paramref name="other" />. 
	///                     Greater than zero 
	///                     This object is greater than <paramref name="other" />. 
	/// </returns>
	/// <param name="other">
	/// An object to compare with this object.
	/// </param>
	public int CompareTo(Precursor other)
	{
		int num = string.CompareOrdinal(ActivationCode, other.ActivationCode);
		if (num != 0)
		{
			return num;
		}
		num = ActivationEnergy.CompareTo(other.ActivationEnergy);
		if (num != 0)
		{
			return num;
		}
		return Mass.CompareTo(other.Mass);
	}
}
