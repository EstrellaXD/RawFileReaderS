using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Readers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The library search options, as contained in an Xcalibur PMD file.
/// </summary>
internal class LibrarySearchOptions : ILibrarySearchOptionsAccess, IRawObjectBase
{
	/// <summary>
	/// The lib search info version 1.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct LibSearchInfo1
	{
		public SimilarityMode SimilarityMode;

		public IdentityMode IdentityMode;

		public LibrarySearchType LibrarySearchType;

		public int MolWt;

		public bool SearchMWEnabled;

		public bool ReverseSearch;

		public bool AppendUserLib;

		public int SearchMW;

		public double MaxHits;

		public int NumSearchListItems;

		public int MatchFactor;

		public int ReverseMatchFactor;

		public int ProbabilityPercent;
	}

	/// <summary>
	/// The (NIST) lib search info.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct LibSearchInfo
	{
		public SimilarityMode SimilarityMode;

		public IdentityMode IdentityMode;

		public LibrarySearchType LibrarySearchType;

		public int MolWt;

		public bool SearchMWEnabled;

		public bool ReverseSearch;

		public bool AppendUserLib;

		public int SearchMW;

		public double MaxHits;

		public int NumSearchListItems;

		public int MatchFactor;

		public int ReverseMatchFactor;

		public int ProbabilityPercent;

		public bool ApplyMassDefect;

		public int Defect1;

		public int Defect2;

		public double Mass1;

		public double Mass2;
	}

	private static readonly int[,] MarshalledSizes = new int[2, 2]
	{
		{
			46,
			Marshal.SizeOf(typeof(LibSearchInfo))
		},
		{
			0,
			Marshal.SizeOf(typeof(LibSearchInfo1))
		}
	};

	private LibSearchInfo _info;

	/// <summary>
	/// Gets the list of libraries to search
	/// </summary>
	public ReadOnlyCollection<string> SearchList { get; private set; }

	/// <summary>
	/// Gets the name of the user library (for append operation)
	/// </summary>
	public string UserLibrary { get; private set; }

	/// <summary>
	/// Gets the similarity setting for NIST search
	/// </summary>
	public SimilarityMode SimilarityMode => _info.SimilarityMode;

	/// <summary>
	/// Gets the identity mode for NIST search
	/// </summary>
	public IdentityMode IdentityMode => _info.IdentityMode;

	/// <summary>
	/// Gets the type of NIST search
	/// </summary>
	public LibrarySearchType LibrarySearchType => _info.LibrarySearchType;

	/// <summary>
	/// Gets the molecular weight
	/// </summary>
	public int MolecularWeight => _info.MolWt;

	/// <summary>
	/// Gets a value indicating whether search with Molecular Weight is enabled
	/// </summary>
	public bool SearchMolecularWeightEnabled => _info.SearchMWEnabled;

	/// <summary>
	/// Gets a value indicating whether reverse search is enabled
	/// </summary>
	public bool ReverseSearch => _info.ReverseSearch;

	/// <summary>
	/// Gets a value indicating whether to append to the user library
	/// </summary>
	public bool AppendUserLibrary => _info.AppendUserLib;

	/// <summary>
	/// Gets the search molecular weight
	/// </summary>
	public int SearchMolecularWeight => _info.SearchMW;

	/// <summary>
	/// Gets the maximum number of reported search hits
	/// </summary>
	public double MaxHits => _info.MaxHits;

	/// <summary>
	/// Gets the match factor
	/// </summary>
	public int MatchFactor => _info.MatchFactor;

	/// <summary>
	/// Gets the reverse match factor
	/// </summary>
	public int ReverseMatchFactor => _info.ReverseMatchFactor;

	/// <summary>
	/// Gets the Probability Percent (match limit)
	/// </summary>
	public int ProbabilityPercent => _info.ProbabilityPercent;

	/// <summary>
	/// Gets a value indicating whether mass defect should be applied
	/// </summary>
	public bool ApplyMassDefect => _info.ApplyMassDefect;

	/// <summary>
	/// Gets the mass defect for the low mass
	/// </summary>
	public int DefectAtMass1 => _info.Defect1;

	/// <summary>
	/// Gets the mass defect for the High mass
	/// </summary>
	public int DefectAtMass2 => _info.Defect2;

	/// <summary>
	/// Gets the mass at which "DefectAtMass1" applies
	/// </summary>
	public double Mass1 => _info.Mass1;

	/// <summary>
	/// Gets the mass at which "DefectAtMass2" applies
	/// </summary>
	public double Mass2 => _info.Mass2;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.LibrarySearchOptions" /> class.
	/// </summary>
	public LibrarySearchOptions()
	{
		UserLibrary = string.Empty;
		SearchList = new ReadOnlyCollection<string>(Array.Empty<string>());
	}

	/// <summary>
	/// Load, (from file)
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="dataOffset">
	/// The data offset.
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes loaded.
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long startPos = dataOffset;
		_info = Utilities.ReadStructure<LibSearchInfo>(viewer, ref startPos, fileRevision, MarshalledSizes);
		UserLibrary = viewer.ReadStringExt(ref startPos);
		List<string> list = new List<string>();
		for (int i = 0; i < _info.NumSearchListItems; i++)
		{
			list.Add(viewer.ReadStringExt(ref startPos));
		}
		SearchList = new ReadOnlyCollection<string>(list);
		if (fileRevision < 46)
		{
			_info.ApplyMassDefect = false;
			_info.Defect1 = 0;
			_info.Defect2 = 300;
			_info.Mass1 = 1.0;
			_info.Mass2 = 1000.0;
		}
		return startPos - dataOffset;
	}
}
