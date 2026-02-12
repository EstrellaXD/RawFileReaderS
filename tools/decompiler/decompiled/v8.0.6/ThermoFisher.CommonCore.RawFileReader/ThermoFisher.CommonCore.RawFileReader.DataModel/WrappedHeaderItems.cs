using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

namespace ThermoFisher.CommonCore.RawFileReader.DataModel;

/// <summary>
/// The wrapped header items.
/// Converts internal header items (for generic data types)
/// to public format.
/// </summary>
internal class WrappedHeaderItems : List<HeaderItem>
{
	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.DataModel.WrappedHeaderItems" /> class.
	/// </summary>
	/// <param name="dataDescriptors">The data descriptors.</param>
	/// <exception cref="T:System.ArgumentNullException">Null data descriptor argument</exception>
	public WrappedHeaderItems(IEnumerable<DataDescriptor> dataDescriptors)
	{
		if (dataDescriptors == null)
		{
			throw new ArgumentNullException("dataDescriptors");
		}
		CopyFrom(dataDescriptors);
	}

	/// <summary>
	/// Copies from.
	/// </summary>
	/// <param name="dataDescriptors">The data descriptors.</param>
	private void CopyFrom(IEnumerable<DataDescriptor> dataDescriptors)
	{
		foreach (DataDescriptor dataDescriptor in dataDescriptors)
		{
			Add(new HeaderItem(dataDescriptor.Label, (GenericDataTypes)dataDescriptor.DataType, (int)dataDescriptor.LengthOrPrecision)
			{
				IsScientificNotation = dataDescriptor.IsScientificNotation
			});
		}
	}
}
