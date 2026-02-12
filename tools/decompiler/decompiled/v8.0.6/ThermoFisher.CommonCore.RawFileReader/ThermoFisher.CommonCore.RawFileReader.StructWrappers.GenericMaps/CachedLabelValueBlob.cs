using System.Collections.Generic;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.GenericMaps;

internal class CachedLabelValueBlob : LabelValueBlob
{
	private List<LabelValuePair> _cachedValues;

	public CachedLabelValueBlob(long offset)
		: base(offset)
	{
	}

	public override List<LabelValuePair> ReadLabelValuePairs(ILogDecoder decoder, bool validateReads = false)
	{
		return _cachedValues ?? (_cachedValues = base.ReadLabelValuePairs(decoder, validateReads));
	}
}
