using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This class defines table of replicates for a calibration level
/// </summary>
[Serializable]
[DataContract]
public class LevelReplicates : CalibrationLevel, ILevelReplicates, ILevelReplicatesAccess, ICalibrationLevelAccess
{
	/// <summary>
	/// Gets or sets the Replicate Collection.
	/// </summary>
	[DataMember]
	public ItemCollection<Replicate> ReplicateCollection { get; set; }

	/// <summary>
	/// Gets the number of replicates for this level
	/// </summary>
	public int Replicates => ReplicateCollection.Count;

	/// <summary>
	/// Gets the replicates of this calibration level
	/// </summary>
	[XmlIgnore]
	ReadOnlyCollection<IReplicate> ILevelReplicatesAccess.ReplicateCollection
	{
		get
		{
			Collection<IReplicate> collection = new Collection<IReplicate>();
			foreach (Replicate item in ReplicateCollection)
			{
				collection.Add(item);
			}
			return new ReadOnlyCollection<IReplicate>(collection);
		}
	}

	/// <summary>
	/// array access operator to return Replicate array element.
	/// </summary>
	/// <param name="index">Index into the array</param>
	/// <returns>The requested replicate</returns>
	public Replicate this[int index]
	{
		get
		{
			return ReplicateCollection[index];
		}
		set
		{
			ReplicateCollection[index] = value;
		}
	}

	/// <summary>
	/// array access operator to return Replicate array element.
	/// </summary>
	/// <param name="index">Index into the array</param>
	/// <returns>The requested replicate</returns>
	IReplicate ILevelReplicatesAccess.this[int index] => ReplicateCollection[index];

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.LevelReplicates" /> class. 
	/// Default construction of replicates
	/// </summary>
	public LevelReplicates()
	{
		ReplicateCollection = new ItemCollection<Replicate>();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.LevelReplicates" /> class. 
	/// Create a new replicate table for a calibration level.
	/// </summary>
	/// <param name="level">
	/// Level to base this replicate table on
	/// </param>
	public LevelReplicates(ICalibrationLevelAccess level)
		: base(level)
	{
		ReplicateCollection = new ItemCollection<Replicate>();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.LevelReplicates" /> class. 
	/// Create a new replicate table for a calibration level.
	/// </summary>
	/// <param name="level">
	/// Level to base this replicate table on
	/// </param>
	public LevelReplicates(ICalibrationLevel level)
		: base(level)
	{
		ReplicateCollection = new ItemCollection<Replicate>();
	}

	/// <summary>
	/// Add a replicate to the list of replicates.
	/// If the supplied replicate has keys which match a
	/// keyed item in the current collection, then the matching record is replaced.
	/// </summary>
	/// <param name="replicate">An additional replicate of this calibration level</param>
	public void AddReplicate(Replicate replicate)
	{
		Replicate replicate2 = FindReplicateWithSameKeys(replicate);
		if (replicate2 != null)
		{
			ReplicateCollection.Remove(replicate2);
		}
		ReplicateCollection.Add(replicate);
	}

	/// <summary>
	/// Add a replicate collection to the list of replicates.
	/// If any of the supplied replicates has keys which match a
	/// keyed item in the current collection, then the matching record is replaced.
	/// </summary>
	/// <param name="collection">The collection to add</param>
	public void AddReplicates(ItemCollection<Replicate> collection)
	{
		foreach (Replicate item in collection)
		{
			AddReplicate(item);
		}
	}

	/// <summary>
	/// Add all replicates from this collection to the list of replicates.
	/// This addition only succeeds if the collections have the same calibration level
	/// If any of the supplied replicates has keys which match a
	/// keyed item in the current collection, then the matching record is replaced.
	/// </summary>
	/// <param name="levelReplicates">The replicates to add</param>
	/// <returns>
	/// true if the collection was added,
	/// false if the calibration levels do not match.
	/// </returns>
	public bool AddReplicates(LevelReplicates levelReplicates)
	{
		if (levelReplicates == null)
		{
			throw new ArgumentNullException("levelReplicates");
		}
		if (base.BaseAmount == levelReplicates.BaseAmount && base.Name == levelReplicates.Name)
		{
			AddReplicates(levelReplicates.ReplicateCollection);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Implementation of <c>ICloneable.Clone</c> method.
	/// Creates deep copy of this instance.
	/// </summary>
	/// <returns>An exact copy of the current sample.</returns>
	public override object Clone()
	{
		LevelReplicates levelReplicates = (LevelReplicates)MemberwiseClone();
		levelReplicates.ReplicateCollection = new ItemCollection<Replicate>();
		for (int i = 0; i < ReplicateCollection.Count; i++)
		{
			levelReplicates.ReplicateCollection.Add(ReplicateCollection[i].Clone() as Replicate);
		}
		return levelReplicates;
	}

	/// <summary>
	/// Count all included/excluded replicates.
	/// <para>
	///     The included and excluded counts are incremented by the number of included
	///     and excluded points. These counters are not set to zero,
	///     allowing this method to be called repeatedly, for example to count
	///     replicates for all levels.
	/// </para>
	/// </summary>
	/// <param name="included">(updated) included counter</param>
	/// <param name="excluded">(updated) excluded counter</param>
	public void CountReplicates(ref int included, ref int excluded)
	{
		foreach (Replicate item in ReplicateCollection)
		{
			CountReplicates(item, ref included, ref excluded);
		}
	}

	/// <summary>
	/// delete all replicates from the list of replicates.
	/// </summary>
	public void DeleteAllReplicates()
	{
		ReplicateCollection.Clear();
	}

	/// <summary>
	/// Finds the replicate with amount and response.
	/// </summary>
	/// <param name="amount">Amount to match</param>
	/// <param name="response">Response to match</param>
	/// <returns>the matching replicate</returns>
	public Replicate FindReplicateWithAmountAndResponse(double amount, double response)
	{
		foreach (Replicate item in ReplicateCollection)
		{
			if (Math.Abs(item.Amount - amount) < 0.0001 && Math.Abs(item.Response - response) < 0.0001)
			{
				return item;
			}
		}
		return null;
	}

	/// <summary>
	/// Find the first replicate which matches the given key, and has no second key
	/// </summary>
	/// <param name="key">Key to find</param>
	/// <returns>Found replicate, or null</returns>
	public Replicate FindReplicateWithKey(string key)
	{
		return FindReplicateWithKeys(key, string.Empty) ?? FindReplicateWithKeys(key, null);
	}

	/// <summary>
	/// Find the replicate which matches a given key and <paramref name="peakKey" />
	/// </summary>
	/// <param name="key">First key (for example file or sample name)</param>
	/// <param name="peakKey">Key for this peak (for example component name)</param>
	/// <returns>The replicate which matches a given key and peak key, or null if not found.</returns>
	public Replicate FindReplicateWithKeys(string key, string peakKey)
	{
		foreach (Replicate item in ReplicateCollection)
		{
			if (item.Key == key && item.PeakKey == peakKey)
			{
				return item;
			}
		}
		return null;
	}

	/// <summary>
	/// Find the replicate which matches a the keys in a given replicate
	/// </summary>
	/// <param name="replicate">replicate to match</param>
	/// <returns>The matching replicate, or null if not found.</returns>
	public Replicate FindReplicateWithSameKeys(Replicate replicate)
	{
		return FindReplicateWithKeys(replicate.Key, replicate.PeakKey);
	}

	/// <summary>
	/// Return the highest included response. Any excluded points are ignored.
	/// </summary>
	/// <returns>The highest response included in the calibration curve</returns>
	public double GetResponseHighBound()
	{
		double num = 0.0;
		foreach (Replicate item in ReplicateCollection)
		{
			if (!((IReplicate)item).ExcludeFromCalibration)
			{
				num = Math.Max(num, ((IReplicate)item).Response);
			}
		}
		return num;
	}

	/// <summary>
	/// Return the lowest included response. Any excluded points are ignored.
	/// </summary>
	/// <returns>The lowest response included in the calibration curve</returns>
	public double GetResponseLowBound()
	{
		double num = double.MaxValue;
		foreach (Replicate item in ReplicateCollection)
		{
			if (!((IReplicate)item).ExcludeFromCalibration)
			{
				num = Math.Min(num, ((IReplicate)item).Response);
			}
		}
		return num;
	}

	/// <summary>
	/// Update the count of included and excluded replicates.
	/// <para>
	///     The included counter is incremented if this is an included
	///     point, otherwise the excluded counter is incremented. These counters are not set to zero,
	///     allowing this method to be called repeatedly, for example to count
	///     replicates for all calibration levels.
	/// </para>
	/// </summary>
	/// <param name="replicate">Replicate to count</param>
	/// <param name="included">(updated) included counter</param>
	/// <param name="excluded">(updated) excluded counter</param>
	private static void CountReplicates(IReplicate replicate, ref int included, ref int excluded)
	{
		if (replicate.ExcludeFromCalibration)
		{
			excluded++;
		}
		else
		{
			included++;
		}
	}
}
