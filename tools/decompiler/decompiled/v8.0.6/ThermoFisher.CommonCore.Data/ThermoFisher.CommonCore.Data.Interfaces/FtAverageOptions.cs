namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Options which can be used to control the Ft / Orbitrap averaging
/// </summary>
public class FtAverageOptions
{
	/// <summary>
	/// The default maximum number of peaks that may have charge determinations.
	/// </summary>
	private const int DefaultMaxChargeDeterminations = 500;

	/// <summary>
	/// Gets or sets the maximum number of ions which are sent to the charge pattern calculation (starting from most intense)
	/// </summary>
	public int MaxChargeDeterminations { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether parallel code may be used for resampling and merging scans.
	/// Tuning option: Permit separate threads to be used for resampling profiles.
	/// </summary>
	public bool MergeInParallel { get; set; }

	/// <summary>
	/// Gets or sets the maximum number of scans which can be merged at once.
	/// This feature is currently not yet implemented, and the value is ignored.
	/// When MergeInParallel is enabled: this restricts the number of scans which are merged in each group.
	/// Setting this too large may result in more memory allocation for "arrays of results to merge"
	/// Default: 10
	/// </summary>
	public int MaxScansMerged { get; set; }

	/// <summary>
	/// Gets or sets the minimum number of Re-sample tasks per thread.
	/// Tuning parameter when MergeInParallel is set.
	/// Each scan is analyzed: Determining mass regions which contain non-zero data,
	/// and re-sampling the intensity data aligned to a set of output bins.
	/// After all scans have been re-sampled, the re-sampled data has to be merged into the final output.
	/// Creating re-sampled data for profiles is a fairly fast task. It may be inefficient to queue workers to
	/// created the merged data for each scan in the batch.
	/// Setting this &gt;1 will reduce threading overheads, when averaging small batches of scans with low intensity peaks.
	/// Default: 2.
	/// This feature only affects the re-sampling, as the final merge of the re-sampled data is single threaded.
	/// </summary>
	public int MergeTaskBatching { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to use the noise and baseline table.
	/// When set: The averaging algorithm calculates average noise based
	/// on a noise table obtained (separately) from the raw file.
	/// The "IRawData" interface doe not have methods to obtain
	/// this "noise and baseline table" from the raw file.
	/// So: The scan averaging algorithm (by default) uses noise information
	/// saved with centroid peaks when calculating the averaged noise.
	/// This option is only effective when data is read via the IRawDataPlus interface.
	/// </summary>
	public bool UseNoiseTableWhenAvailable { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Interfaces.FtAverageOptions" /> class. 
	/// default constructor
	/// </summary>
	public FtAverageOptions()
	{
		MaxChargeDeterminations = 500;
		MergeInParallel = true;
		MaxScansMerged = 10;
		MergeTaskBatching = 2;
		UseNoiseTableWhenAvailable = true;
	}
}
