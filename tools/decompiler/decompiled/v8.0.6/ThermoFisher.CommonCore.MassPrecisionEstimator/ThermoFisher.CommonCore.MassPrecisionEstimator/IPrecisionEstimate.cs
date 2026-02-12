using System;
using System.Collections.Generic;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.FilterEnums;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.MassPrecisionEstimator;

/// <summary>
/// The interface class for the Mass Precision Estimate (MPE) code
/// </summary>
public interface IPrecisionEstimate : IDisposable
{
	/// <summary> 
	/// Sets the RAW file. 
	/// </summary>
	IRawDataPlus Rawfile { set; }

	/// <summary> 
	/// Sets the scan number. 
	/// </summary>
	/// <value> The scan number. </value>
	int ScanNumber { set; }

	/// <summary>
	/// Calculate the ion time - fill (traps and FT) or dwell time (quads)
	/// based upon the type of instrument.
	/// </summary>
	/// <param name="analyzerType">The analyzer type</param>
	/// <param name="scan">The scan to process</param>
	/// <param name="trailerHeadings">The trailer headings.</param>
	/// <param name="trailerValues">The trailer values.</param>
	/// <returns>The calculated ion time</returns>
	double GetIonTime(MassAnalyzerType analyzerType, Scan scan, List<string> trailerHeadings, List<string> trailerValues);

	/// <summary> 
	/// Gets the mass precision estimate for the data in the provided scan. The information is passed through the
	/// arguments in this method.
	/// </summary>
	/// <param name="scan">The scan to process</param>
	/// <param name="analyzerType">The analyzer type for the provided scan</param>
	/// <param name="ionTime">The ion time for the provided scan</param>
	/// <param name="resolution">the resolution for the provided scan</param>
	/// <returns>The list of EstimatorResults objects</returns>
	List<EstimatorResults> GetMassPrecisionEstimate(Scan scan, MassAnalyzerType analyzerType, double ionTime, double resolution);

	/// <summary> 
	/// Gets mass precision estimate for the provided scan number.  The information needed to process the scan is passed through
	/// the properties in the class when using this method. 
	/// </summary>
	/// <returns>The list of EstimatorResults objects</returns>
	List<EstimatorResults> GetMassPrecisionEstimate();
}
