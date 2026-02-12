using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

/// <summary>
/// The FilterScanEvent interface.
/// </summary>
internal interface IFilterScanEvent : IScanEventEdit, IRawFileReaderScanEvent
{
	/// <summary>
	/// Gets or sets the meta filters.
	/// </summary>
	MetaFilterType MetaFilters { get; set; }

	/// <summary>
	/// Gets or sets the filter mass precision.
	/// </summary>
	int FilterMassPrecision { get; set; }

	/// <summary>
	/// Gets or sets the locale name.
	/// </summary>
	string LocaleName { get; set; }

	/// <summary>
	/// Gets the total source values, which is all CID and CV.
	/// </summary>
	int TotalSourceValues { get; }

	/// <summary>
	/// Gets the table of compensation voltages valid.
	/// </summary>
	ScanFilterEnums.SourceCIDValidTypes[] CompensationVoltagesValid { get; }

	/// <summary>
	/// Gets the table of source CID valid.
	/// </summary>
	ScanFilterEnums.SourceCIDValidTypes[] SourceCidVoltagesValid { get; }

	/// <summary>
	/// Calculate the number of compensation voltage values.
	/// </summary>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	int NumCompensationVoltageValues();

	/// <summary>
	/// set filter mass resolution by mass precision.
	/// Example: "precision =3", resolution = "0.001"
	/// </summary>
	/// <param name="precision">
	/// The precision.
	/// </param>
	void SetFilterMassResolutionByMassPrecision(int precision);

	/// <summary>
	/// set filter mass resolution.
	/// </summary>
	/// <param name="massResolution">
	/// The mass resolution.
	/// </param>
	void SetFilterMassResolution(double massResolution);

	/// <summary>
	/// convert to string.
	/// </summary>
	/// <returns>
	/// The <see cref="T:System.String" />.
	/// </returns>
	new string ToString();

	/// <summary>
	/// Gets the number of source CID info values.
	/// </summary>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	int NumSourceCidInfoValues();
}
