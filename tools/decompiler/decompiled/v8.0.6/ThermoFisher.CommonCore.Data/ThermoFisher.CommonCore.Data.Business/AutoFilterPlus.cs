using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Auto filter extensions for raw files
/// </summary>
public static class AutoFilterPlus
{
	/// <summary>
	/// This class extends the functions of "auto filter".
	/// On construction, the "auto filter" list is obtained from the MS detector.
	/// methods are then called to construct a list of filters based on this data.
	/// The list can be searched for "items matching a given filter rule".
	/// </summary>
	private class AutoFilterEnhanced : IEnhancedAutoFilter
	{
		private ReadOnlyCollection<IScanFilter> _allFilter;

		private IRawDataPlus _rawData;

		public List<IFilterWithString> FilterList { get; internal set; } = new List<IFilterWithString>();

		/// <summary>
		/// Gets or sets a value indicating whether compound names should be included.
		/// This only has an effect if the "Name" property is set for at least one filter.
		/// Results which have names are shown in sorted (alpha) order.
		/// Any other filters, which do not have a name, are then added after the 
		/// named items, in the original "auto filter" order.
		/// If a name appears for more than one filter, then an entry is created
		/// which only contains a compound name, such that a chromatogram
		/// can be created based on all data identified for that compound.
		/// Note that this "name only" list is excluded when a "unique filter list" is
		/// requested with a specific subset filter.
		/// </summary>
		public bool IncludeCompoundNames { get; set; } = true;

		/// <summary>
		/// Gets or sets a separator which appears between a compound name and a filter.
		/// </summary>
		public string CompoundNameSeparator { get; set; } = ": ";

		/// <summary>
		/// Adds the "empty filter" to the list.
		/// </summary>
		public void AddEmptyFilter()
		{
			if (_rawData.SelectMsData())
			{
				FilterList.Add(new FilterWithString(_rawData, string.Empty));
			}
		}

		/// <summary>
		/// Searches for the activation types used.
		/// Adds "Activation type" to the list, for MS/MS data (MS2 or above).
		/// This can find CID, HCD, ETD, UVPD, EID
		/// </summary>
		public void AddActivationTypeFilters()
		{
			if (!_rawData.SelectMsData())
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			foreach (IScanFilter item in _allFilter)
			{
				if (item.MSOrder < MSOrderType.Ms2)
				{
					continue;
				}
				foreach (MsStage stage in new MsOrderTable(item).Stages)
				{
					foreach (IReaction reaction in stage.Reactions)
					{
						if (reaction.ActivationType == ActivationType.CollisionInducedDissociation)
						{
							flag = true;
						}
						if (reaction.ActivationType == ActivationType.HigherEnergyCollisionalDissociation)
						{
							flag2 = true;
						}
						if (reaction.ActivationType == ActivationType.ElectronTransferDissociation)
						{
							flag3 = true;
						}
						if (reaction.ActivationType == ActivationType.UltraVioletPhotoDissociation)
						{
							flag4 = true;
						}
						if (reaction.ActivationType == ActivationType.ElectronInducedDissociation)
						{
							flag5 = true;
						}
					}
				}
			}
			if (flag)
			{
				AddFilter("CID");
			}
			if (flag3)
			{
				AddFilter("ETD");
			}
			if (flag2)
			{
				AddFilter("HCD");
			}
			if (flag4)
			{
				AddFilter("UVPD");
			}
			if (flag5)
			{
				AddFilter("EID");
			}
			void AddFilter(string filter)
			{
				FilterList.Add(new FilterWithString(_rawData, filter));
			}
		}

		/// <summary>
		/// Creates a filter from a string and adds it
		/// </summary>
		/// <param name="filter"></param>
		private void Add(string filter)
		{
			FilterList.Add(new FilterWithString(_rawData, filter));
		}

		/// <summary>
		/// Adds "ms order filters" to the list
		/// this includes ms, ms2 etc up to ms10
		/// if any ms/ms is found msn is also added.
		/// </summary>
		/// <param name="addMsn">When true (default): if any ms/ms is found "MSn" is also added.</param>
		public void AddMsOrderFilters(bool addMsn = true)
		{
			if (!_rawData.SelectMsData())
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			bool flag7 = false;
			bool flag8 = false;
			bool flag9 = false;
			bool flag10 = false;
			bool flag11 = false;
			foreach (IScanFilter item in _allFilter)
			{
				MSOrderType mSOrder = item.MSOrder;
				switch (mSOrder)
				{
				case MSOrderType.Ms:
					flag = true;
					break;
				case MSOrderType.Ms2:
					flag2 = true;
					break;
				case MSOrderType.Ms3:
					flag3 = true;
					break;
				case MSOrderType.Ms4:
					flag4 = true;
					break;
				case MSOrderType.Ms5:
					flag5 = true;
					break;
				case MSOrderType.Ms6:
					flag6 = true;
					break;
				case MSOrderType.Ms7:
					flag7 = true;
					break;
				case MSOrderType.Ms8:
					flag8 = true;
					break;
				case MSOrderType.Ms9:
					flag9 = true;
					break;
				case MSOrderType.Ms10:
					flag10 = true;
					break;
				}
				if (mSOrder >= MSOrderType.Ms2)
				{
					flag11 = true;
				}
			}
			if (flag)
			{
				Add("MS");
			}
			if (flag2)
			{
				Add("MS2");
			}
			if (flag3)
			{
				Add("MS3");
			}
			if (flag4)
			{
				Add("MS4");
			}
			if (flag5)
			{
				Add("MS5");
			}
			if (flag6)
			{
				Add("MS6");
			}
			if (flag7)
			{
				Add("MS7");
			}
			if (flag8)
			{
				Add("MS8");
			}
			if (flag9)
			{
				Add("MS9");
			}
			if (flag10)
			{
				Add("MS10");
			}
			if (flag11 && addMsn)
			{
				Add("MSn");
			}
		}

		/// <summary>
		/// Add all unique filter groups (auto filter).
		/// </summary>
		/// <param name="mustContain">If this is not empty: The filters must all contain this sub-filter.
		/// For example "d" for "only return data dependent"</param>
		public void AddUniqueFilters(string mustContain)
		{
			bool hasSubset = !string.IsNullOrWhiteSpace(mustContain);
			bool flag = false;
			if (!_rawData.SelectMsData())
			{
				return;
			}
			List<FilterWithString> unique = new List<FilterWithString>();
			for (int i = 0; i < _allFilter.Count; i++)
			{
				IScanFilter scanFilter = _allFilter[i];
				string text = scanFilter.ToString();
				FilterWithString filterWithString = new FilterWithString
				{
					ScanFilter = scanFilter,
					Filter = text
				};
				if (IncludeCompoundNames && !string.IsNullOrEmpty(scanFilter.Name))
				{
					flag = true;
					filterWithString.Name = scanFilter.Name;
					filterWithString.FilterWithName = scanFilter.Name + CompoundNameSeparator + text;
				}
				unique.Add(filterWithString);
			}
			if (hasSubset && !ApplyMustContain(_rawData, unique, mustContain))
			{
				throw new ArgumentException("Not a valid filter", "mustContain");
			}
			if (flag)
			{
				SortListByCompound();
			}
			FilterList.AddRange(unique);
			static int CompareByName(FilterWithString x, FilterWithString y)
			{
				return x.FilterWithName.CompareTo(y.FilterWithName);
			}
			void SortListByCompound()
			{
				List<FilterWithString> list = new List<FilterWithString>();
				List<FilterWithString> list2 = new List<FilterWithString>();
				for (int j = 0; j < unique.Count; j++)
				{
					FilterWithString filterWithString2 = unique[j];
					if (string.IsNullOrEmpty(filterWithString2.FilterWithName))
					{
						list2.Add(filterWithString2);
					}
					else
					{
						list.Add(filterWithString2);
					}
				}
				if (list.Count > 0)
				{
					list.Sort(CompareByName);
				}
				unique.Clear();
				if (!hasSubset)
				{
					string text2 = string.Empty;
					int num = 0;
					List<string> list3 = new List<string>();
					for (int k = 0; k < list.Count; k++)
					{
						FilterWithString filterWithString3 = list[k];
						if (k == 0)
						{
							text2 = filterWithString3.Name;
						}
						else if (filterWithString3.Name == text2)
						{
							if (num == 0)
							{
								list3.Add(text2);
							}
							num++;
						}
						else
						{
							num = 0;
							text2 = filterWithString3.Name;
						}
					}
					foreach (string item in list3)
					{
						unique.Add(new FilterWithString
						{
							Name = item
						});
					}
				}
				unique.AddRange(list);
				unique.AddRange(list2);
			}
		}

		/// <summary>
		/// Remove "CV" from filters, such that filters which have CV at different values are merged.
		/// </summary>
		public void MergeCvValues()
		{
			FilterList = MergeCV(FilterList);
		}

		/// <summary>
		/// Create an enhanced auto filter for a raw file
		/// </summary>
		/// <param name="rawData"></param>
		/// <param name="applyTimeLimits">indicates whether time limits are used for the
		///  "auto filter" request from the raw file.
		///  If this is true:
		///  Only scans from minTime to maxTime are used
		///  else: all scans are used</param>
		/// <param name="minTime">the minimum retention time for getting a set of filters from the raw file.
		/// Only applied when applyTimeLimits is set.</param>
		/// <param name="maxTime">the maximum retention time for getting a set of filters from the raw file.
		/// Only applied when applyTimeLimits is set.</param>
		/// <param name="mode">Optional: Determine how precursor tolerance is handled.
		/// Default: Use auto calculated tolerance</param>
		/// <param name="decimalPlaces">Optional: When a specified tolerance is specified, then a number of matched decimal places 
		/// can be specified (default 2)</param>
		public AutoFilterEnhanced(IRawDataPlus rawData, bool applyTimeLimits = false, double minTime = 0.0, double maxTime = 0.0, FilterPrecisionMode mode = FilterPrecisionMode.Auto, int decimalPlaces = 2)
		{
			_rawData = rawData;
			if (!rawData.SelectMsData())
			{
				return;
			}
			if (mode == FilterPrecisionMode.Auto)
			{
				if (applyTimeLimits)
				{
					_allFilter = rawData.GetFiltersForTimeRange(minTime, maxTime);
				}
				else
				{
					_allFilter = rawData.GetFilters();
				}
			}
			else if (applyTimeLimits)
			{
				_allFilter = rawData.GetAccurateFiltersForTimeRange(minTime, maxTime, mode, decimalPlaces);
			}
			else
			{
				_allFilter = rawData.GetAccurateFilters(mode, decimalPlaces);
			}
			FilterList = new List<IFilterWithString>();
		}

		private bool ApplyMustContain(IRawDataPlus rawData, List<FilterWithString> filterList, string text)
		{
			IScanFilter filterFromString = rawData.GetFilterFromString(text);
			if (filterFromString == null)
			{
				return false;
			}
			bool hasAccurateMassPrecursors = rawData.GetInstrumentData().HasAccurateMassPrecursors;
			int filterMassPrecision = rawData.RunHeaderEx.FilterMassPrecision;
			ScanFilterHelper filterHelper = new ScanFilterHelper(filterFromString, hasAccurateMassPrecursors, filterMassPrecision);
			List<FilterWithString> list = new List<FilterWithString>();
			foreach (FilterWithString filter in filterList)
			{
				if (ScanEventHelper.ScanEventHelperFactory(filter.ScanFilter).TestScanAgainstFilter(filterHelper))
				{
					list.Add(filter);
				}
			}
			filterList.Clear();
			filterList.AddRange(list);
			return true;
		}

		/// <summary>
		/// remove "CV" from all filters
		/// </summary>
		/// <param name="filterList">Filter to consolidate</param>
		/// <returns>Updated list</returns>
		private List<IFilterWithString> MergeCV(List<IFilterWithString> filterList)
		{
			RemoveCv();
			SortedDictionary<string, IFilterWithString> uniqueFilters = new SortedDictionary<string, IFilterWithString>();
			List<IFilterWithString> list = new List<IFilterWithString>();
			for (int i = 0; i < filterList.Count; i++)
			{
				AddToList(i);
			}
			foreach (KeyValuePair<string, IFilterWithString> item in uniqueFilters)
			{
				list.Add(item.Value);
			}
			return list;
			void AddToList(int oldListIndex)
			{
				IFilterWithString filterWithString = filterList[oldListIndex];
				string key = (string.IsNullOrEmpty(filterWithString.Name) ? filterWithString.Filter : (filterWithString.Name + ":" + filterWithString.Filter));
				if (!uniqueFilters.ContainsKey(key))
				{
					uniqueFilters.Add(key, filterWithString);
				}
			}
			void RemoveCv()
			{
				for (int j = 0; j < filterList.Count; j++)
				{
					IFilterWithString filterWithString = filterList[j];
					if (!filterWithString.NameOnly)
					{
						IScanFilter scanFilter = filterWithString.ScanFilter;
						if (scanFilter.CompensationVoltage == TriState.On)
						{
							scanFilter.CompensationVoltage = TriState.Any;
							string text = scanFilter.ToString();
							string filterWithName = (string.IsNullOrEmpty(filterWithString.Name) ? string.Empty : (filterWithString.Name + CompoundNameSeparator + text));
							filterList[j] = new FilterWithString
							{
								Filter = scanFilter.ToString(),
								ScanFilter = scanFilter,
								Name = filterWithString.Name,
								FilterWithName = filterWithName
							};
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Create an enhanced auto filter for this raw file
	/// </summary>
	/// <param name="file">raw file</param>
	/// <returns>enhanced auto filter</returns>
	public static IEnhancedAutoFilter EnhancedAutoFilter(this IRawDataPlus file)
	{
		return new AutoFilterEnhanced(file);
	}

	/// <summary>
	/// Create an enhanced auto filter for this raw file, using scans over a specific time range.
	/// </summary>
	/// <param name="file">raw file</param>
	/// <param name="startTime">start retention time</param>
	/// <param name="endTime">end retention time</param>
	/// <returns>enhanced auto filter</returns>
	public static IEnhancedAutoFilter EnhancedAutoFilter(this IRawDataPlus file, double startTime, double endTime)
	{
		return new AutoFilterEnhanced(file, applyTimeLimits: true, startTime, endTime);
	}

	/// <summary>
	/// Create an enhanced auto filter for this raw file, using scans over a specific time range.
	/// </summary>
	/// <param name="file">raw file</param>
	/// <param name="startTime">start retention time</param>
	/// <param name="endTime">end retention time</param>
	/// <param name="mode">Optional: Determine how precursor tolerance is handled.
	/// Default: Use auto calculated tolerance</param>
	/// <param name="decimalPlaces">Optional: When a specified tolerance is specified, then a number of matched decimal places 
	/// can be specified (default 2)</param>
	/// <returns>enhanced auto filter</returns>
	public static IEnhancedAutoFilter EnhancedAutoFilter(this IRawDataPlus file, double startTime, double endTime, FilterPrecisionMode mode = FilterPrecisionMode.Auto, int decimalPlaces = 2)
	{
		return new AutoFilterEnhanced(file, applyTimeLimits: true, startTime, endTime, mode, decimalPlaces);
	}

	/// <summary>
	/// Create an enhanced auto filter for this raw file, using scans over a specific time range.
	/// </summary>
	/// <param name="file">raw file</param>
	/// <param name="mode">Optional: Determine how precursor tolerance is handled.
	/// Default: Use auto calculated tolerance</param>
	/// <param name="decimalPlaces">Optional: When a specified tolerance is specified, then a number of matched decimal places 
	/// can be specified (default 2)</param>
	/// <returns>enhanced auto filter</returns>
	public static IEnhancedAutoFilter EnhancedAutoFilter(this IRawDataPlus file, FilterPrecisionMode mode = FilterPrecisionMode.Auto, int decimalPlaces = 2)
	{
		return new AutoFilterEnhanced(file, applyTimeLimits: false, 0.0, 0.0, mode, decimalPlaces);
	}
}
