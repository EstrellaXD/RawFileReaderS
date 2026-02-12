namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// The chromatogram fragment request.
/// A request to make a section of the overall chromatogram.
/// </summary>
public class ChromatogramFragmentRequest
{
	/// <summary>
	/// Gets or sets a value indicating whether this is empty.
	/// Empty implies that the raw file has no scans in the RT range.
	/// </summary>
	public bool Empty { get; set; }

	/// <summary>
	/// Gets or sets the aggregator, which collates a set of partial chromatograms.
	/// </summary>
	public ChromatogramAggregator Aggregator { get; set; }

	/// <summary>
	/// Gets or sets the batch.
	/// This is the batch index with the results table of the aggregator
	/// </summary>
	public int Batch { get; set; }

	/// <summary>
	/// Gets or sets the start scan index.
	/// </summary>
	public int StartIndex { get; set; }

	/// <summary>
	/// Gets or sets the end scan index.
	/// </summary>
	public int EndIndex { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this is a final section of a chromatogram.
	/// </summary>
	public bool IsFinal { get; set; }

	/// <summary>
	/// Make a chromatogram from a batch of scans
	/// </summary>
	/// <param name="scans">The scans</param>
	public void ProcessScans(ScanAndIndex[] scans)
	{
		ISignalConvert signal = (Empty ? null : Aggregator.Generator.CreatePartialChromatogram(scans));
		Aggregator.SignalReady(Batch, signal);
	}
}
