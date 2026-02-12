using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The (NIST) library search constraints.
/// </summary>
internal class LibrarySearchConstraints : ILibrarySearchConstraintsAccess, IRawObjectBase
{
	/// <summary>
	/// The constraints info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct ConstraintsInfo
	{
		public bool MolWtEnabled;

		public int MnMolWt;

		public int MaxMolWt;

		public bool NameFragmentEnabled;

		public bool DBEnabled;

		public bool FineEnabled;

		public bool EPAEnabled;

		public bool NIHEnabled;

		public bool TSCAEnabled;

		public bool USPEnabled;

		public bool EINECSEnabled;

		public bool RTECSEnabled;

		public bool HODOCEnabled;

		public bool IREnabled;

		public bool ElementsEnabled;

		public ElementsInCompound ElementsMethod;

		public bool IonConstraintsEnabled;

		public IonConstraints IonConstraintMethod;
	}

	/// <summary>
	/// The individual constraints info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct IndividualConstraintsInfo
	{
		public char Element0;

		public char Element1;

		private char element2;

		public ElementConditions Condition;

		public int Value;
	}

	/// <summary>
	/// The individual constraint wrapper, to return data as required interface.
	/// </summary>
	private class IndividualConstraintWrapper : IIndividualConstraintAccess
	{
		/// <summary>
		/// Gets the condition on this element (greater, less or equal to value)
		/// </summary>
		public ElementConditions ElementCondition { get; }

		/// <summary>
		/// Gets the comparison value for this element constraint.
		/// Used in a a test as per "ElementCondition"
		/// </summary>
		public int Value { get; }

		/// <summary>
		/// Gets the element to constrain
		/// </summary>
		public string Element { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.LibrarySearchConstraints.IndividualConstraintWrapper" /> class.
		/// </summary>
		/// <param name="info">
		/// The info.
		/// </param>
		public IndividualConstraintWrapper(IndividualConstraintsInfo info)
		{
			ElementCondition = info.Condition;
			Value = info.Value;
			Element = ((info.Element0 == '\0') ? string.Empty : ((info.Element1 == '\0') ? new string(info.Element0, 1) : new string(new char[2] { info.Element0, info.Element1 })));
		}
	}

	/// <summary>
	/// The ion constraint info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct IonConstraintInfo
	{
		public IonConstraintTypes ConstraintTypes;

		public int MassToCharge;

		public int From;

		public int To;
	}

	/// <summary>
	/// The ion constraint info wrapper, to implement the required interface
	/// </summary>
	private class IonConstraintInfoWrapper : IIonConstraintAccess
	{
		/// <summary>
		/// Gets the method of ion constraint
		/// </summary>
		public IonConstraintTypes Constraint { get; }

		/// <summary>
		/// Gets the mass to charge ratio of the constraint
		/// </summary>
		public int MassToCharge { get; }

		/// <summary>
		/// Gets the from value of the constraint
		/// </summary>
		public int From { get; }

		/// <summary>
		/// Gets the To value of the constraint
		/// </summary>
		public int To { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.LibrarySearchConstraints.IonConstraintInfoWrapper" /> class.
		/// </summary>
		/// <param name="info">
		/// The info.
		/// </param>
		public IonConstraintInfoWrapper(IonConstraintInfo info)
		{
			Constraint = info.ConstraintTypes;
			MassToCharge = info.MassToCharge;
			From = info.From;
			To = info.To;
		}
	}

	private static readonly int[,] MarshalledSizes = new int[1, 2] { 
	{
		0,
		Marshal.SizeOf(typeof(ConstraintsInfo))
	} };

	private ConstraintsInfo _info;

	/// <summary>
	/// Gets the Ion Constraints (see NIST documentation for details)
	/// </summary>
	public ReadOnlyCollection<IIonConstraintAccess> IonConstraints { get; private set; }

	/// <summary>
	/// Gets the individual element constraints (limits on specific elements)
	/// </summary>
	public ReadOnlyCollection<IIndividualConstraintAccess> IndivdualConstraints { get; private set; }

	/// <summary>
	/// Gets a value indicating whether molecular weight constraint is enabled
	/// </summary>
	public bool MolecularWeightEnabled => _info.MolWtEnabled;

	/// <summary>
	/// Gets the minimum molecular weight
	/// </summary>
	public int MinMolecularWeight => _info.MnMolWt;

	/// <summary>
	/// Gets the maximum molecular weight
	/// </summary>
	public int MaximumMolecularWeight => _info.MaxMolWt;

	/// <summary>
	/// Gets a value indicating whether name fragment constraint is enabled
	/// </summary>
	public bool NameFragmentEnabled => _info.NameFragmentEnabled;

	/// <summary>
	/// Gets a value indicating whether DB constraint is enabled
	/// </summary>
	public bool DbEnabled => _info.DBEnabled;

	/// <summary>
	///  Gets a value indicating whether Fine constraint is enabled
	/// </summary>
	public bool FineEnabled => _info.FineEnabled;

	/// <summary>
	/// Gets a value indicating whether EPA (Environmental Protection Agency) constraint is applied
	/// </summary>
	public bool EpaEnabled => _info.EPAEnabled;

	/// <summary>
	/// Gets a value indicating whether NIH (National Institute of Health) constraint is applied
	/// </summary>
	public bool NihEnabled => _info.NIHEnabled;

	/// <summary>
	/// Gets a value indicating whether TSCA (Toxic Substances Control Act) constraint is applied
	/// </summary>
	public bool TscaEnabled => _info.TSCAEnabled;

	/// <summary>
	/// Gets a value indicating whether USP (United States Pharmacopoeia) constraint is applied
	/// </summary>
	public bool UspEnabled => _info.USPEnabled;

	/// <summary>
	/// Gets a value indicating whether EINECS (European Inventory of Existing Commercial Chemical Substances) constraint is applied
	/// </summary>
	public bool EinecsEnabled => _info.EINECSEnabled;

	/// <summary>
	/// Gets a value indicating whether RTECS (Registry of Toxic Effects of Chemical Substances) constraint is applied
	/// </summary>
	public bool RtecsEnabled => _info.RTECSEnabled;

	/// <summary>
	/// Gets a value indicating whether HODOC (Handbook of Data on Organic Compounds) constraint is applied
	/// </summary>
	public bool HodocEnabled => _info.HODOCEnabled;

	/// <summary>
	/// Gets a value indicating whether IR constraint is applied
	/// </summary>
	public bool IrEnabled => _info.IREnabled;

	/// <summary>
	/// Gets a value indicating whether Elements constraint is applied
	/// </summary>
	public bool ElementsEnabled => _info.ElementsEnabled;

	/// <summary>
	/// Gets the element constraint method (used when ElementsEnabled)
	/// </summary>
	public ElementsInCompound ElementsMethod => _info.ElementsMethod;

	/// <summary>
	/// Gets a value indicating whether Ion Constraints are enabled
	/// </summary>
	public bool IonConstraintsEnabled => _info.IonConstraintsEnabled;

	/// <summary>
	/// Gets the method of Ion Constraints (used when IonConstraintsEnabled)
	/// </summary>
	public IonConstraints IonConstraintMethod => _info.IonConstraintMethod;

	/// <summary>
	/// Gets the name fragment constraint
	/// </summary>
	public string NameFragment { get; private set; }

	/// <summary>
	/// Gets the Element constraint
	/// </summary>
	public string Element { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.LibrarySearchConstraints" /> class.
	/// </summary>
	public LibrarySearchConstraints()
	{
		IndivdualConstraints = new ReadOnlyCollection<IIndividualConstraintAccess>(new List<IIndividualConstraintAccess>());
		IonConstraints = new ReadOnlyCollection<IIonConstraintAccess>(new List<IIonConstraintAccess>());
		Element = string.Empty;
		NameFragment = string.Empty;
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
		_info = Utilities.ReadStructure<ConstraintsInfo>(viewer, ref startPos, fileRevision, MarshalledSizes);
		NameFragment = viewer.ReadStringExt(ref startPos);
		Element = viewer.ReadStringExt(ref startPos);
		if (fileRevision >= 25)
		{
			int num = viewer.ReadIntExt(ref startPos);
			IndividualConstraintWrapper[] array = new IndividualConstraintWrapper[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = new IndividualConstraintWrapper(viewer.ReadStructure<IndividualConstraintsInfo>(startPos, out var numberOfBytesRead));
				startPos += numberOfBytesRead;
			}
			IndivdualConstraints = new ReadOnlyCollection<IIndividualConstraintAccess>(array);
			int num2 = viewer.ReadIntExt(ref startPos);
			IonConstraintInfoWrapper[] array2 = new IonConstraintInfoWrapper[num2];
			for (int j = 0; j < num2; j++)
			{
				array2[j] = new IonConstraintInfoWrapper(viewer.ReadStructure<IonConstraintInfo>(startPos, out var numberOfBytesRead2));
				startPos += numberOfBytesRead2;
			}
			IonConstraints = new ReadOnlyCollection<IIonConstraintAccess>(array2);
		}
		return startPos - dataOffset;
	}
}
