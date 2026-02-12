using System;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.MSReaction;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

/// <summary>
///     The reaction class defines one reaction in a chain of MS/MS reactions.
/// A reaction describes which ion was acted on, with what methods, to produce the
/// next level MS/MS fragments.
/// </summary>
internal sealed class Reaction : IEquatable<Reaction>, IComparable<Reaction>, IRawObjectBase, IReaction
{
	private const int SizeCurrentIndex = 0;

	private const int Size65Index = 1;

	private const int Size31Index = 2;

	private const int SizeOldIndex = 3;

	private static readonly int[] MarshalledSizes = new int[4]
	{
		Marshal.SizeOf(typeof(MsReactionStruct)),
		Marshal.SizeOf(typeof(MsReactionStruct3)),
		Marshal.SizeOf(typeof(MsReactionStruct2)),
		Marshal.SizeOf(typeof(MsReactionStruct1))
	};

	/// <summary>
	/// Gets a value indicating whether is multiple activation.
	/// </summary>
	public bool MultipleActivation { get; private set; }

	/// <summary>
	///     Gets the collision energy valid value from the structure.
	/// </summary>
	public uint CollisionEnergyValidEx { get; private set; }

	/// <summary>
	///     Gets a value indicating whether the collision energy is valid. this is set to true
	///     to use in scan filtering.
	/// </summary>
	public bool CollisionEnergyValid { get; private set; }

	/// <summary>
	/// Gets a value indicating whether is precursor energies valid.
	/// </summary>
	public bool IsPrecursorEnergiesValid { get; private set; }

	/// <summary>
	/// Gets a value indicating whether use named activation.
	/// </summary>
	public bool UseNamedActivation { get; private set; }

	/// <summary>
	/// Gets the named activation type.
	/// </summary>
	public ActivationType ActivationType { get; private set; }

	/// <summary>
	///     Gets the collision energy.
	/// </summary>
	public double CollisionEnergy { get; private set; }

	/// <summary>
	///     Gets the first precursor mass. If <see cref="P:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.Reaction.PrecursorRangeIsValid" /> == TRUE, this value defines the start of the
	///     precursor isolation range.
	/// </summary>
	public double FirstPrecursorMass { get; private set; }

	/// <summary>
	///     Gets a value indicating whether is range valid. If TRUE, <see cref="P:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.Reaction.PrecursorMass" /> is still the center mass but the
	///     <see cref="P:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.Reaction.FirstPrecursorMass" /> and <see cref="P:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.Reaction.LastPrecursorMass" />
	///     are also valid.
	/// </summary>
	public bool PrecursorRangeIsValid { get; private set; }

	/// <summary>
	///     Gets the isolation width.
	/// </summary>
	public double IsolationWidth { get; private set; }

	/// <summary>
	///     Gets the isolation width offset.
	/// </summary>
	public double IsolationWidthOffset { get; private set; }

	/// <summary>
	///     Gets the last precursor mass.
	/// </summary>
	public double LastPrecursorMass { get; private set; }

	/// <summary>
	///     Gets the precursor mass.
	/// </summary>
	public double PrecursorMass { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.Reaction" /> class.
	/// </summary>
	public Reaction()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.Reaction" /> class.
	/// </summary>
	/// <param name="precursorMass">
	/// The precursor mass.
	/// </param>
	/// <param name="isolationWidth">
	/// The isolation width.
	/// </param>
	/// <param name="collisionEnergy">
	/// The collision energy.
	/// </param>
	/// <param name="collisionEnergyValid">
	/// The collision energy valid.
	/// </param>
	/// <param name="rangeIsValid">
	/// The range is valid.
	/// </param>
	/// <param name="firstPrecursorMass">
	/// The first precursor mass.
	/// </param>
	/// <param name="lastPrecursorMass">
	/// The last precursor mass.
	/// </param>
	/// <param name="isolationWidthOffset">
	/// The isolation width offset.
	/// </param>
	public Reaction(double precursorMass = 50.0, double isolationWidth = 1.0, double collisionEnergy = 25.0, uint collisionEnergyValid = 1u, bool rangeIsValid = false, double firstPrecursorMass = 0.0, double lastPrecursorMass = 0.0, double isolationWidthOffset = 0.0)
	{
		MsReactionStruct reaction = new MsReactionStruct
		{
			CollisionEnergy = collisionEnergy,
			CollisionEnergyValid = collisionEnergyValid,
			FirstPrecursorMass = firstPrecursorMass,
			IsolationWidth = isolationWidth,
			IsolationWidthOffset = isolationWidthOffset,
			LastPrecursorMass = lastPrecursorMass,
			PrecursorMass = precursorMass,
			RangeIsValid = rangeIsValid
		};
		InitialFields(reaction);
		InitialValidFlags();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan.Reaction" /> class.
	/// </summary>
	/// <param name="from">
	/// Clone from interface.
	/// </param>
	public Reaction(IReaction from)
	{
		uint collisionEnergyValid = (from.CollisionEnergyValid ? 1u : 0u) | (uint)((int)from.ActivationType << 1);
		MsReactionStruct reaction = new MsReactionStruct
		{
			CollisionEnergy = from.CollisionEnergy,
			CollisionEnergyValid = collisionEnergyValid,
			FirstPrecursorMass = from.FirstPrecursorMass,
			IsolationWidth = from.IsolationWidth,
			IsolationWidthOffset = from.IsolationWidthOffset,
			LastPrecursorMass = from.LastPrecursorMass,
			PrecursorMass = from.PrecursorMass,
			RangeIsValid = from.PrecursorRangeIsValid
		};
		InitialFields(reaction);
		InitialValidFlags();
	}

	/// <summary>
	/// compare details (excludes parent mass).
	/// </summary>
	/// <param name="y">
	/// The y.
	/// </param>
	/// <returns>
	/// less than zero if before, zero if same, positive if after.
	/// </returns>
	public int CompareDetails(Reaction y)
	{
		int num = Utilities.CompareDoubles(IsolationWidth, y.IsolationWidth, 1E-06);
		if (num != 0)
		{
			return num;
		}
		num = Utilities.CompareDoubles(CollisionEnergy, y.CollisionEnergy, 1E-06);
		if (num != 0)
		{
			return num;
		}
		if (CollisionEnergyValidEx != y.CollisionEnergyValidEx)
		{
			uint num2 = (CollisionEnergyValidEx >> 1) & 0xFF;
			uint num3 = (y.CollisionEnergyValidEx >> 1) & 0xFF;
			if (num2 != num3)
			{
				if (num2 <= num3)
				{
					return -1;
				}
				return 1;
			}
		}
		if (PrecursorRangeIsValid)
		{
			if (!y.PrecursorRangeIsValid)
			{
				return 1;
			}
			num = Utilities.CompareDoubles(FirstPrecursorMass, y.FirstPrecursorMass, 1E-06);
			if (num != 0)
			{
				return num;
			}
			num = Utilities.CompareDoubles(LastPrecursorMass, y.LastPrecursorMass, 1E-06);
			if (num != 0)
			{
				return num;
			}
		}
		else if (y.PrecursorRangeIsValid)
		{
			return -1;
		}
		return Utilities.CompareDoubles(IsolationWidthOffset, y.IsolationWidthOffset, 1E-06);
	}

	/// <summary>
	/// The comparison implementation of <see cref="T:System.IComparable`1" />.
	/// </summary>
	/// <param name="other">
	/// The object to compare.
	/// </param>
	/// <returns>
	/// Return an <see cref="T:System.Int32" /> that has one of three values:
	///     <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <term>Less than zero</term>
	/// <description>The current instance precedes the object specified by the CompareTo method in the sort order.</description>
	/// <term>Zero</term>
	/// <description>This current instance occurs in the same position in the sort order as the object specified by the CompareTo method.</description>
	/// <term>Greater than zero</term>
	/// <description>This current instance follows the object specified by the CompareTo method in the sort order.</description>
	/// </list>
	/// </returns>
	public int CompareTo(Reaction other)
	{
		int num = Utilities.CompareDoubles(PrecursorMass, other.PrecursorMass, 1E-06);
		if (num != 0)
		{
			return num;
		}
		return CompareDetails(other);
	}

	/// <summary>
	/// The comparison implementation of <see cref="T:System.IEquatable`1" />.
	/// </summary>
	/// <param name="other">
	/// The object to compare.
	/// </param>
	/// <returns>
	/// True if the objects are equal.
	/// </returns>
	public bool Equals(Reaction other)
	{
		return CompareTo(other) == 0;
	}

	/// <summary>
	/// Deeps the clone.
	/// </summary>
	/// <returns>copy of this object</returns>
	public Reaction DeepClone()
	{
		return (Reaction)MemberwiseClone();
	}

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		int num = MarshalledSizes[0];
		byte[] array;
		if (fileRevision >= 66)
		{
			array = viewer.ReadLargeData(startPos, num);
			startPos += num;
		}
		else
		{
			array = new byte[num];
			int count = ((fileRevision >= 65) ? MarshalledSizes[1] : ((fileRevision < 31) ? MarshalledSizes[3] : MarshalledSizes[2]));
			Buffer.BlockCopy(viewer.ReadBytesExt(ref startPos, count), 0, array, 0, count);
		}
		MsReactionStruct reaction = new MsReactionStruct
		{
			PrecursorMass = BitConverter.ToDouble(array, 0),
			IsolationWidth = BitConverter.ToDouble(array, 8),
			CollisionEnergy = BitConverter.ToDouble(array, 16),
			CollisionEnergyValid = ((fileRevision < 31) ? 1u : BitConverter.ToUInt32(array, 24)),
			RangeIsValid = (BitConverter.ToInt32(array, 28) > 0),
			FirstPrecursorMass = BitConverter.ToDouble(array, 32),
			LastPrecursorMass = BitConverter.ToDouble(array, 40),
			IsolationWidthOffset = ((fileRevision < 66) ? 0.0 : BitConverter.ToDouble(array, 48))
		};
		if (fileRevision < 65)
		{
			reaction.RangeIsValid = false;
			reaction.FirstPrecursorMass = (reaction.LastPrecursorMass = 0.0);
		}
		InitialFields(reaction);
		InitialValidFlags();
		return startPos - dataOffset;
	}

	/// <summary>
	/// initial fields from a reaction.
	/// </summary>
	/// <param name="reaction">
	/// The reaction to copy from.
	/// </param>
	private void InitialFields(MsReactionStruct reaction)
	{
		PrecursorMass = reaction.PrecursorMass;
		LastPrecursorMass = reaction.LastPrecursorMass;
		IsolationWidthOffset = reaction.IsolationWidthOffset;
		IsolationWidth = reaction.IsolationWidth;
		PrecursorRangeIsValid = reaction.RangeIsValid;
		FirstPrecursorMass = reaction.FirstPrecursorMass;
		CollisionEnergyValidEx = reaction.CollisionEnergyValid;
		CollisionEnergy = reaction.CollisionEnergy;
	}

	/// <summary>
	/// initialize valid flags.
	/// </summary>
	private void InitialValidFlags()
	{
		uint num;
		if ((CollisionEnergyValidEx & 1) != 0)
		{
			CollisionEnergyValid = true;
			num = CollisionEnergyValidEx & 0xFFFE;
		}
		else
		{
			CollisionEnergyValid = false;
			num = CollisionEnergyValidEx | 1;
		}
		IsPrecursorEnergiesValid = false;
		if (num == 0)
		{
			UseNamedActivation = false;
			IsPrecursorEnergiesValid = true;
			return;
		}
		UseNamedActivation = true;
		uint num2 = (num >> 1) & 0xFF;
		ActivationType = ((num2 < 38) ? ((ActivationType)num2) : ActivationType.CollisionInducedDissociation);
		MultipleActivation = (num & 0x1000) != 0;
		if ((num & 1) == 0)
		{
			IsPrecursorEnergiesValid = true;
		}
	}
}
