using System;
using System.Globalization;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers.Scan;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Tool to create a parser, for validating and filter strings
/// creating an interface to access filter features,
/// for a given locale
/// </summary>
public static class FilterParserFactory
{
	/// <summary>
	/// Wrapper class, to ensure state is reset on each parse call.
	/// See notes in GetFilterFromString
	/// </summary>
	internal class FilterParserWrapper : IFilterParser
	{
		/// <summary>
		/// Gets or sets the precision expected for collision energy
		/// </summary>
		public int EnergyPrecision { get; set; }

		/// <summary>
		/// Gets or sets the precision expected for mass values
		/// </summary>
		public int MassPrecision { get; set; }

		public string Culture { get; set; }

		/// <summary>
		/// Format for localization
		/// </summary>
		public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

		public string ListSeparator { get; private set; } = ",";

		public string DecimalSeparator { get; private set; } = ".";

		/// <summary>
		/// Parse a string, returning the scan filter codes.
		/// </summary>
		/// <param name="text">String to parse</param>
		/// <returns>Parsed filter, or null if invalid</returns>
		public IScanFilterPlus GetFilterFromString(string text)
		{
			if (!string.IsNullOrEmpty(Culture))
			{
				FormatProvider = new CultureInfo(Culture);
				CultureInfo cultureInfo = CultureInfo.GetCultureInfo(Culture);
				ListSeparator = cultureInfo.TextInfo.ListSeparator;
				DecimalSeparator = cultureInfo.NumberFormat.NumberDecimalSeparator;
			}
			return new FilterStringParser
			{
				MassPrecision = MassPrecision,
				EnergyPrecision = EnergyPrecision,
				FormatProvider = FormatProvider,
				ListSeparator = ListSeparator,
				DecimalSeparator = DecimalSeparator
			}.GetFilterFromString(text);
		}
	}

	/// <summary>
	/// Create a parser, for validating filter strings
	/// </summary>
	/// <returns>An interface to validate filter strings</returns>
	public static IFilterParser CreateFilterParser()
	{
		return new FilterParserWrapper
		{
			MassPrecision = 6,
			EnergyPrecision = 2,
			FormatProvider = CultureInfo.InvariantCulture
		};
	}

	/// <summary>
	/// Create a parser, for validating filter strings
	/// </summary>
	/// <returns>An interface to validate filter strings</returns>
	public static IFilterParser CreateFilterParser(string culture)
	{
		return new FilterParserWrapper
		{
			MassPrecision = 6,
			EnergyPrecision = 2,
			Culture = culture
		};
	}
}
