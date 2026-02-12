using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// A collection of element subsets.
/// Used to supply data to the elemental composition algorithm.
/// </summary>
[Serializable]
[CollectionDataContract]
public class ElementsSubsetCollection : ItemCollection<ElementSubset>
{
	/// <summary>
	/// Add an element to the end of the collection.
	/// </summary>
	/// <param name="element">
	/// Element to add
	/// </param>
	public void AddElement(ElementSubset element)
	{
		Add(element);
	}

	/// <summary>
	/// Add an element, keeping the list in nominal mass order.
	/// </summary>
	/// <param name="element">
	/// Element to add
	/// </param>
	public void AddElementByOrder(ElementSubset element)
	{
		bool flag = false;
		int num = 0;
		using (IEnumerator<ElementSubset> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.NominalMass > element.NominalMass)
				{
					Insert(num, element);
					flag = true;
					break;
				}
				num++;
			}
		}
		if (!flag)
		{
			Add(element);
		}
	}

	/// <summary>
	/// Remove all element subsets with a given sign
	/// </summary>
	/// <param name="sign">
	/// If the subset has this sign, it is removed
	/// </param>
	public void Remove(string sign)
	{
		using IEnumerator<ElementSubset> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			ElementSubset current = enumerator.Current;
			if (current.Sign.Equals(sign))
			{
				Remove(current);
				break;
			}
		}
	}

	/// <summary>
	/// Reset the "UseRatio" flag for all subsets in the collection to a given value.
	/// </summary>
	/// <param name="set">
	/// Value to set all "UseRatio" flags.
	/// </param>
	public void ResetUseRatio(bool set)
	{
		using IEnumerator<ElementSubset> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.UseRatio = set;
		}
	}

	/// <summary>
	/// Test if there is any <see cref="T:ThermoFisher.CommonCore.Data.Business.ElementSubset" /> in the collection with a given sign
	/// </summary>
	/// <param name="sign">
	/// Sign to look for
	/// </param>
	/// <returns>
	/// true if at least one <see cref="T:ThermoFisher.CommonCore.Data.Business.ElementSubset" /> in this collection has the given Sign
	/// </returns>
	public bool Contains(string sign)
	{
		using (IEnumerator<ElementSubset> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.Sign.Equals(sign))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Reset the "in use" flag for all subsets in the collection to a given value.
	/// </summary>
	/// <param name="set">
	/// Value to set all "in use" flags.
	/// </param>
	public void ResetInUse(bool set)
	{
		using IEnumerator<ElementSubset> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.InUse = set;
		}
	}
}
