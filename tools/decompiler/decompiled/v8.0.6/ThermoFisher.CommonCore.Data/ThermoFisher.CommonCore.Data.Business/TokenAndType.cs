using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Data structure used internally by scan definition
/// </summary>
internal struct TokenAndType : IComparable<TokenAndType>
{
	/// <summary>
	/// Gets or sets the token.
	/// </summary>
	public string Token { get; set; }

	/// <summary>
	/// Gets or sets the token class.
	/// </summary>
	public TokenClass TokenClass { get; set; }

	/// <summary>
	/// Compares the current token with another token
	/// </summary>
	/// <returns>
	/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: 
	/// <list type="table">
	///   <listheader>
	///     <term>Value</term>
	///     <description>Meaning</description>
	///   </listheader>
	///   <item>
	///     <term>Less than zero </term>
	///     <description>This object is less than the <paramref name="other" /> parameter.</description>
	///   </item>
	///   <item>
	///     <term>Zero</term>
	///     <description>This object is equal to <paramref name="other" />.</description>
	///   </item>
	///   <item>
	///     <term>Greater than zero</term>
	///     <description>This object is greater than <paramref name="other" />. 
	/// </description>
	///   </item>
	/// </list>               
	/// </returns>
	/// <param name="other">An object to compare with this object.
	///                 </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException"><c>TokenClass</c> is out of range.</exception>
	public int CompareTo(TokenAndType other)
	{
		if (TokenClass == other.TokenClass)
		{
			switch (TokenClass)
			{
			case TokenClass.RangeToken:
				return 0;
			case TokenClass.Generic:
			case TokenClass.DataFormat:
			case TokenClass.Polarity:
				return CompareGeneric(other);
			case TokenClass.MsOrder:
				return 0;
			case TokenClass.ParentMass:
				return 0;
			case TokenClass.DataDependent:
				return 0;
			default:
				throw new ArgumentOutOfRangeException("other");
			}
		}
		if (TokenClass <= other.TokenClass)
		{
			return -1;
		}
		return 1;
	}

	/// <summary>
	/// The compare generic.
	/// </summary>
	/// <param name="other">
	/// The other.
	/// </param>
	/// <returns>
	/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: 
	/// <list type="table">
	///   <listheader>
	///     <term>Value</term>
	///     <description>Meaning</description>
	///   </listheader>
	///   <item>
	///     <term>-1</term>
	///     <description>This object is less than the <paramref name="other" /> parameter.</description>
	///   </item>
	///   <item>
	///     <term>Zero</term>
	///     <description>This object is equal to <paramref name="other" />.</description>
	///   </item>
	///   <item>
	///     <term>1</term>
	///     <description>This object is greater than <paramref name="other" />. 
	/// </description>
	///   </item>
	/// </list>               
	/// </returns>
	private int CompareGeneric(TokenAndType other)
	{
		int num = string.Compare(Token, other.Token, StringComparison.Ordinal);
		if (num <= 0)
		{
			if (num >= 0)
			{
				return 0;
			}
			return -1;
		}
		return 1;
	}
}
