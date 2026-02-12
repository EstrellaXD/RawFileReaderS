using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Describes a scan, based on interpreting filter strings.
/// This class need not be serialized or cloned, as it is intended to be "read only"
/// All information can be kept as a string. Call FromString to initialize this.
/// This class is designed for use where filter information is
/// only available as a string (such as with implementations of IRawData)
/// Newer code (with IRawDataPlus) should use IScanFilter for details about filters.
/// </summary>
public class ScanDefinition : IComparable<ScanDefinition>
{
	private readonly List<TokenAndType> _tokens = new List<TokenAndType>();

	private readonly List<Precursor> _precursors = new List<Precursor>();

	private static readonly double[] PrecisionMatchingTable = new double[8] { 0.5, 0.05, 0.005, 0.0005, 5E-05, 5E-06, 5E-07, 5E-08 };

	/// <summary>
	/// Gets or sets the Polarity, Positive or Negative ions
	/// </summary>
	public Polarity Polarity { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this scan depends on data in previous scans
	/// </summary>
	public bool DataDependent { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the scan is centroid format (else profile)
	/// </summary>
	public bool IsCentroid { get; set; }

	/// <summary>
	/// Gets the precursor masses and their means of activation
	/// </summary>
	public List<Precursor> Precursors => _precursors;

	/// <summary>
	/// Gets or sets the MS or MS/MS order (for example 2 for MS/MS)
	/// </summary>
	public int MsOrder { get; set; }

	/// <summary>
	/// Gets or sets the Mass Ranges scanned
	/// </summary>
	public Range[] Ranges { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether this is an MSX scan type
	/// </summary>
	public bool Msx { get; set; }

	/// <summary>
	/// Gets or sets the tolerance used when matching mass ranges.
	/// It is initialized to 0.0005.
	/// On parsing a string (using the FromString method), this is set based on the precision of the "low mass value" from the string.
	/// For example, for a mass range of "34.256 - 41.365" the precision is determined to be 3 decimal places
	/// and the tolerance is initialized to "0.0005" which will match items which have the same "3 decimals places".
	/// It is assumed that all values in a parsed string have the same mass precision.
	/// </summary>
	public double MassRangeMatchingTolerance { get; set; }

	/// <summary>
	/// Create a scan definition from a filter strings. The string is formatted as a list of text tokens, separated by spaces.
	/// Most tokens can be any text, apart from some specifically parsed items:
	/// <c>
	/// + Positive
	/// - Negative
	/// d data dependant
	/// c Centroid
	/// p Profile
	/// [Ranges]    scanned ranges, for example [100.1-300.2,556.8-901.4]
	/// </c>
	/// </summary>
	/// <param name="from">
	/// String to parse
	/// </param>
	/// <returns>
	/// Scan definition which matches the string
	/// </returns>
	public static ScanDefinition FromString(string from)
	{
		ScanDefinition scanDefinition = new ScanDefinition();
		scanDefinition.CreateFromString(from);
		return scanDefinition;
	}

	/// <summary>
	/// Sort the filters in alphabetic and numeric order of appropriate fields.
	/// It is assumed that the supplied list has been automatically generated, and
	/// therefore filters with the same parameters will have those in the same order
	/// in the string.
	/// </summary>
	/// <param name="filters">
	/// Items to sort
	/// </param>
	/// <returns>
	/// A sorted list of the filters
	/// </returns>
	public static string[] Sort(string[] filters)
	{
		ScanDefinition[] array = ParseDefinitions(filters);
		Array.Sort(array);
		return ConvertDefinitionsToStrings(array);
	}

	/// <summary>
	/// Sort the filters in alphabetic and numeric order of appropriate fields.
	/// Compact the filters, so that filters which match within tolerance are eliminated.
	/// This is designed to take input from "Auto filter" or similar methods.
	/// It is not designed to take a "user generated list".
	/// It is assumed that the supplied list has been automatically generated, and
	/// therefore filters with the same parameters will have those in the same order
	/// in the string.
	/// </summary>
	/// <param name="filters">
	/// Items to sort
	/// </param>
	/// <param name="precursorMassTolerance">
	/// Tolerance for matching precursor masses.
	/// If this is zero, then the tolerance is based on the number of precision digits entered in the text
	/// </param>
	/// <returns>
	/// A sorted list of the filters
	/// </returns>
	public static string[] SortAndCompact(string[] filters, IMassOptionsAccess precursorMassTolerance)
	{
		ScanDefinition[] array = ParseDefinitions(filters);
		Array.Sort(array);
		return ConvertDefinitionsToStrings(CreateUniqueList(array, precursorMassTolerance));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanDefinition" /> class. 
	/// Default constructor
	/// </summary>
	public ScanDefinition()
	{
		Ranges = new Range[0];
		MassRangeMatchingTolerance = 0.0005;
	}

	/// <summary>
	/// The list of sub strings (tokens) in the supplied filter text
	/// </summary>
	/// <returns>
	/// The tokens.
	/// </returns>
	public string[] ToTokens()
	{
		string[] array = new string[_tokens.Count];
		for (int i = 0; i < _tokens.Count; i++)
		{
			array[i] = _tokens[i].Token;
		}
		return array;
	}

	/// <summary>
	/// Convert to a text string
	/// </summary>
	/// <returns>
	/// String form of the filter
	/// </returns>
	public new string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(100);
		for (int i = 0; i < _tokens.Count; i++)
		{
			stringBuilder.Append(_tokens[i].Token);
			if (i < _tokens.Count - 1)
			{
				stringBuilder.Append(' ');
			}
		}
		return stringBuilder.ToString();
	}

	/// <summary>
	/// Strip a number. Removes a number from the start of the supplied string.
	/// </summary>
	/// <param name="stringWithNumber">
	/// The string, which should start with a number.
	/// </param>
	/// <param name="decimals">
	/// The number of decimal places found in the string.
	/// </param>
	/// <returns>
	/// The stripped number.
	/// </returns>
	private static string StripNumber(StringBuilder stringWithNumber, out int decimals)
	{
		decimals = 0;
		if (stringWithNumber.Length > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			while (stringWithNumber.Length > num && char.IsDigit(stringWithNumber[num]))
			{
				stringBuilder.Append(stringWithNumber[num++]);
			}
			if (stringWithNumber.Length > num && stringWithNumber[num] == '.')
			{
				stringBuilder.Append(stringWithNumber[num++]);
			}
			while (stringWithNumber.Length > num && char.IsDigit(stringWithNumber[num]))
			{
				decimals++;
				stringBuilder.Append(stringWithNumber[num++]);
			}
			stringWithNumber.Remove(0, num);
			return stringBuilder.ToString();
		}
		return string.Empty;
	}

	/// <summary>
	/// Parse definitions.
	/// </summary>
	/// <param name="filters">
	/// The filters.
	/// </param>
	/// <returns>
	/// The parsed scan definitions
	/// </returns>
	private static ScanDefinition[] ParseDefinitions(string[] filters)
	{
		ScanDefinition[] array = new ScanDefinition[filters.Length];
		for (int i = 0; i < filters.Length; i++)
		{
			array[i] = FromString(filters[i]);
		}
		return array;
	}

	/// <summary>
	/// Convert definitions to strings.
	/// </summary>
	/// <param name="definitions">
	/// The definitions.
	/// </param>
	/// <returns>
	/// The strings for each scan definition
	/// </returns>
	private static string[] ConvertDefinitionsToStrings(IList<ScanDefinition> definitions)
	{
		string[] array = new string[definitions.Count];
		for (int i = 0; i < definitions.Count; i++)
		{
			array[i] = definitions[i].ToString();
		}
		return array;
	}

	/// <summary>
	/// Create a unique list of definitions, from a list which may have duplicates.
	/// </summary>
	/// <param name="definitions">
	/// The definitions.
	/// </param>
	/// <param name="precursorMassTolerance">
	/// The precursor mass tolerance.
	/// </param>
	/// <returns>
	/// The list of unique definitions
	/// </returns>
	private static List<ScanDefinition> CreateUniqueList(IEnumerable<ScanDefinition> definitions, IMassOptionsAccess precursorMassTolerance)
	{
		List<ScanDefinition> list = new List<ScanDefinition>();
		ScanDefinition scanDefinition = null;
		foreach (ScanDefinition definition in definitions)
		{
			if (scanDefinition == null || !scanDefinition.Match(definition, precursorMassTolerance))
			{
				list.Add(definition);
				scanDefinition = definition;
			}
		}
		return list;
	}

	/// <summary>
	/// Match a generic token
	/// </summary>
	/// <param name="token">
	/// Token to look for
	/// </param>
	/// <param name="scanFilterTokens">
	/// List of all tokens in the scan filter
	/// </param>
	/// <returns>
	/// True if the token is found
	/// </returns>
	private static bool MatchGeneric(TokenAndType token, IEnumerable<TokenAndType> scanFilterTokens)
	{
		if (token.Token[0] == '!')
		{
			string token2 = token.Token;
			token2 = token2.TrimStart('!');
			foreach (TokenAndType scanFilterToken in scanFilterTokens)
			{
				if (scanFilterToken.TokenClass == TokenClass.Generic && !(token2 != scanFilterToken.Token))
				{
					return false;
				}
			}
			return true;
		}
		foreach (TokenAndType scanFilterToken2 in scanFilterTokens)
		{
			if (scanFilterToken2.TokenClass == TokenClass.Generic && !(token.Token != scanFilterToken2.Token))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Read the activation code string.
	/// </summary>
	/// <param name="builder">
	/// The string being parsed.
	/// </param>
	/// <param name="precursor">
	/// The precursor to update.
	/// </param>
	private static void ReadActivationString(StringBuilder builder, Precursor precursor)
	{
		int i = 1;
		int num = 0;
		for (; i < builder.Length && char.IsLetter(builder[i]); i++)
		{
			num++;
		}
		if (num > 0)
		{
			precursor.ActivationCode = builder.ToString().Substring(1, num);
			builder.Remove(0, num + 1);
			if (double.TryParse(StripNumber(builder, out var _), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				precursor.ActivationEnergy = result;
			}
		}
	}

	/// <summary>
	/// Find ranges from token.
	/// </summary>
	/// <param name="item">
	/// The item.
	/// </param>
	/// <param name="parsed">
	/// The parsed.
	/// </param>
	/// <returns>
	/// The mass ranges
	/// </returns>
	private IEnumerable<Range> RangesFromToken(string item, StringBuilder parsed)
	{
		StringBuilder stringBuilder = new StringBuilder(item);
		List<Range> list = new List<Range>();
		bool flag = false;
		if (stringBuilder.Length > 0 && stringBuilder[0] == '[')
		{
			parsed.Append('[');
			stringBuilder.Remove(0, 1);
		}
		if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] == ']')
		{
			flag = true;
			stringBuilder.Remove(stringBuilder.Length - 1, 1);
		}
		while (stringBuilder.Length > 0 && char.IsDigit(stringBuilder[0]))
		{
			int decimals;
			string text = StripNumber(stringBuilder, out decimals);
			if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
			{
				break;
			}
			if (decimals >= 0 && decimals < PrecisionMatchingTable.Length)
			{
				MassRangeMatchingTolerance = PrecisionMatchingTable[decimals];
			}
			parsed.Append(text);
			if (stringBuilder.Length > 0 && stringBuilder[0] == '-')
			{
				parsed.Append('-');
				stringBuilder.Remove(0, 1);
				int decimals2;
				string text2 = StripNumber(stringBuilder, out decimals2);
				if (!double.TryParse(text2, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
				{
					parsed.Append(text);
					list.Add(new Range(result, result));
					break;
				}
				parsed.Append(text2);
				list.Add(new Range(result, result2));
			}
			else
			{
				parsed.Append('-');
				parsed.Append(text);
				list.Add(new Range(result, result));
			}
			if (stringBuilder.Length > 0 && stringBuilder[0] == ',')
			{
				parsed.Append(',');
				stringBuilder.Remove(0, 1);
			}
		}
		if (flag)
		{
			parsed.Append(']');
		}
		return list;
	}

	/// <summary>
	/// Create from a string.
	/// </summary>
	/// <param name="from">
	/// The string to create from.
	/// </param>
	private void CreateFromString(string from)
	{
		int num = 0;
		bool flag = false;
		List<Range> massRanges = new List<Range>();
		MsOrder = 1;
		if (string.IsNullOrEmpty(from))
		{
			return;
		}
		string[] array = from.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			string nextToken = null;
			if (i < array.Length - 1)
			{
				nextToken = array[i + 1];
			}
			if (num > 0)
			{
				num = AddParentMass(num, text, nextToken);
			}
			else if (flag)
			{
				flag = AddRange(text, massRanges);
			}
			else if (text.Length == 1)
			{
				AddSingleCharItem(text);
			}
			else if (text.Contains("["))
			{
				flag = AddInitialRange(text, massRanges);
			}
			else if (text.Length >= 2 && text[0] == 'm' && text[1] == 's')
			{
				if (text.Length == 3 && text[2] == 'x')
				{
					Msx = true;
					AddToken(text, TokenClass.Generic);
				}
				else
				{
					num = AddMsOrder(num, text);
				}
			}
			else
			{
				AddToken(text, TokenClass.Generic);
			}
		}
	}

	/// <summary>
	/// Add the MS order.
	/// </summary>
	/// <param name="parentMassesExpected">
	/// The number of parent masses expected before this call.
	/// </param>
	/// <param name="item">
	/// The item.
	/// </param>
	/// <returns>
	/// The number of parent masses expected after parsing the MS order.
	/// </returns>
	private int AddMsOrder(int parentMassesExpected, string item)
	{
		if (item.Length > 2)
		{
			if (int.TryParse(item.Substring(2), NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				MsOrder = result;
				if (MsOrder >= 1)
				{
					parentMassesExpected = MsOrder - 1;
				}
				AddToken(item, TokenClass.MsOrder);
			}
			else
			{
				AddToken(item, TokenClass.Generic);
			}
		}
		else
		{
			AddToken(item, TokenClass.MsOrder);
		}
		return parentMassesExpected;
	}

	/// <summary>
	/// Add initial mass range.
	/// </summary>
	/// <param name="item">
	/// The item to parse.
	/// </param>
	/// <param name="massRanges">
	/// The mass ranges found so far.
	/// </param>
	/// <returns>
	/// True if more mass ranges are expected.
	/// </returns>
	private bool AddInitialRange(string item, List<Range> massRanges)
	{
		bool num = !item.Contains("]");
		StringBuilder stringBuilder = new StringBuilder(0);
		massRanges.AddRange(RangesFromToken(item, stringBuilder));
		AddToken(stringBuilder.ToString(), TokenClass.RangeToken);
		if (!num)
		{
			Ranges = massRanges.ToArray();
		}
		return num;
	}

	/// <summary>
	/// Add a single char item.
	/// </summary>
	/// <param name="item">
	/// The item to add.
	/// </param>
	private void AddSingleCharItem(string item)
	{
		switch (item[0])
		{
		case '+':
			Polarity = Polarity.Positive;
			AddToken(item, TokenClass.Polarity);
			break;
		case '-':
			Polarity = Polarity.Negative;
			AddToken(item, TokenClass.Polarity);
			break;
		case 'd':
			DataDependent = true;
			AddToken(item, TokenClass.DataDependent);
			break;
		case 'c':
			IsCentroid = true;
			AddToken(item, TokenClass.DataFormat);
			break;
		case 'p':
			IsCentroid = false;
			AddToken(item, TokenClass.DataFormat);
			break;
		default:
			AddToken(item, TokenClass.Generic);
			break;
		}
	}

	/// <summary>
	/// Add a mass range.
	/// </summary>
	/// <param name="item">
	/// The item containing a range.
	/// </param>
	/// <param name="massRanges">
	/// The mass ranges found so far.
	/// </param>
	/// <returns>
	/// True if more mass ranges are expected.
	/// </returns>
	private bool AddRange(string item, List<Range> massRanges)
	{
		bool num = !item.Contains("]");
		StringBuilder stringBuilder = new StringBuilder(0);
		massRanges.AddRange(RangesFromToken(item, stringBuilder));
		AddToken(stringBuilder.ToString(), TokenClass.RangeToken);
		if (!num)
		{
			Ranges = massRanges.ToArray();
		}
		return num;
	}

	/// <summary>
	/// Add a parent mass.
	/// </summary>
	/// <param name="parentMassesExpected">
	/// The number of parent masses expected.
	/// </param>
	/// <param name="item">
	/// The item.
	/// </param>
	/// <param name="nextToken">
	/// The next token.
	/// </param>
	/// <returns>
	/// True if more parent masses are expected.
	/// </returns>
	private int AddParentMass(int parentMassesExpected, string item, string nextToken)
	{
		AddToken(item, TokenClass.ParentMass);
		StringBuilder stringBuilder = new StringBuilder(item);
		if (double.TryParse(StripNumber(stringBuilder, out var decimals), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			Precursor precursor = new Precursor
			{
				Mass = result,
				Precision = decimals,
				ActivationCode = string.Empty
			};
			parentMassesExpected--;
			if (stringBuilder.Length > 0 && stringBuilder[0] == '@')
			{
				ReadActivationString(stringBuilder, precursor);
			}
			Precursors.Add(precursor);
			if (Msx && nextToken != null && nextToken.Contains('@'))
			{
				parentMassesExpected++;
			}
		}
		return parentMassesExpected;
	}

	/// <summary>
	/// Add a token.
	/// </summary>
	/// <param name="token">
	/// The token.
	/// </param>
	/// <param name="tokenClass">
	/// The token class.
	/// </param>
	private void AddToken(string token, TokenClass tokenClass)
	{
		_tokens.Add(new TokenAndType
		{
			Token = token,
			TokenClass = tokenClass
		});
	}

	/// <summary>
	/// Test if a scan type "passes the filter", that is the scan
	/// type is the same as, or is a superset of the scan definition.
	/// </summary>
	/// <param name="scanType">
	/// The type of the scan which needs to be checked
	/// </param>
	/// <returns>
	/// true if the supplied scan has passed a filter against this scan definition
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// <c></c> is out of range.
	/// </exception>
	public bool MatchToScanType(string scanType)
	{
		return Match(FromString(scanType), new MassOptions
		{
			Tolerance = 0.0
		});
	}

	/// <summary>
	/// Test if a scan type "passes the filter", that is the scan
	/// type is the same as, or is a superset of this scan definition.
	/// The exact test is: All tokens in "this" must be present in "scanTypeFilter".
	/// </summary>
	/// <param name="scanTypeToCheck">
	/// The type of the scan which needs to be checked
	/// </param>
	/// <param name="precursorMassTolerance">
	/// Mass tolerance used to match MS/MS precursors
	/// </param>
	/// <returns>
	/// true if the supplied scan has passed a filter against this scan definition
	/// </returns>
	public bool Match(ScanDefinition scanTypeToCheck, IMassOptionsAccess precursorMassTolerance)
	{
		List<TokenAndType> scanToCheckTokens = scanTypeToCheck._tokens;
		return _tokens.All((TokenAndType token) => TestForMatchingToken(scanTypeToCheck, token, scanToCheckTokens, precursorMassTolerance));
	}

	/// <summary>
	/// Test for a matching token.
	/// Test that the token from "the tokens in this" is contained within "scan type to check"
	/// </summary>
	/// <param name="scanTypeToCheck">
	/// The scan type to check (for being a superset of "this").
	/// </param>
	/// <param name="token">
	/// The token to look for, in "scan type to check"
	/// </param>
	/// <param name="scanToCheckTokens">
	/// The tokens of the scan being tested.
	/// </param>
	/// <param name="precursorMassTolerance">
	/// The precursor mass tolerance.
	/// </param>
	/// <returns>
	/// True if there is a match
	/// </returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// thrown if the token is not valid
	/// </exception>
	private bool TestForMatchingToken(ScanDefinition scanTypeToCheck, TokenAndType token, IEnumerable<TokenAndType> scanToCheckTokens, IMassOptionsAccess precursorMassTolerance)
	{
		return token.TokenClass switch
		{
			TokenClass.RangeToken => MatchRanges(scanTypeToCheck, MassRangeMatchingTolerance), 
			TokenClass.Generic => MatchGeneric(token, scanToCheckTokens), 
			TokenClass.DataFormat => IsCentroid == scanTypeToCheck.IsCentroid, 
			TokenClass.DataDependent => DataDependent == scanTypeToCheck.DataDependent, 
			TokenClass.Polarity => Polarity == scanTypeToCheck.Polarity, 
			TokenClass.MsOrder => MatchPrecursors(scanTypeToCheck, precursorMassTolerance), 
			TokenClass.ParentMass => true, 
			_ => throw new ArgumentOutOfRangeException("token"), 
		};
	}

	/// <summary>
	/// Match precursors.
	/// </summary>
	/// <param name="scanTypeFilter">
	/// The scan type filter.
	/// </param>
	/// <param name="precursorMassTolerance">
	/// The precursor mass tolerance.
	/// </param>
	/// <returns>
	/// True if precursors match.
	/// </returns>
	private bool MatchPrecursors(ScanDefinition scanTypeFilter, IMassOptionsAccess precursorMassTolerance)
	{
		if (MsOrder != scanTypeFilter.MsOrder)
		{
			return false;
		}
		if (Precursors.Count > scanTypeFilter.Precursors.Count)
		{
			return false;
		}
		for (int i = 0; i < Precursors.Count; i++)
		{
			Precursor precursor = Precursors[i];
			Precursor precursor2 = scanTypeFilter.Precursors[i];
			double comparisonTolerance = GetComparisonTolerance(precursorMassTolerance, precursor, precursor2);
			if (Math.Abs(precursor.Mass - precursor2.Mass) > comparisonTolerance)
			{
				return false;
			}
			if (!string.IsNullOrEmpty(precursor.ActivationCode) && precursor.ActivationCode != precursor2.ActivationCode)
			{
				return false;
			}
			if (precursor.ActivationEnergy != 0.0 && Math.Abs(precursor.ActivationEnergy - precursor2.ActivationEnergy) > 0.001)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Get comparison tolerance.
	/// </summary>
	/// <param name="precursorMassTolerance">
	/// The precursor mass tolerance.
	/// </param>
	/// <param name="precursor">
	/// The precursor.
	/// </param>
	/// <param name="otherPrecursor">
	/// The other precursor.
	/// </param>
	/// <returns>
	/// The comparison tolerance.
	/// </returns>
	private double GetComparisonTolerance(IMassOptionsAccess precursorMassTolerance, Precursor precursor, Precursor otherPrecursor)
	{
		if (DataDependent)
		{
			return 0.2;
		}
		double result;
		if (precursorMassTolerance.Tolerance == 0.0)
		{
			int num = Math.Max(precursor.Precision, otherPrecursor.Precision);
			result = 0.001;
			if (num >= 0 && num < PrecisionMatchingTable.Length)
			{
				result = PrecisionMatchingTable[num];
			}
		}
		else
		{
			result = precursorMassTolerance.GetToleranceAtMass(precursor.Mass);
		}
		return result;
	}

	/// <summary>
	/// match the ranges.
	/// </summary>
	/// <param name="scanTypeToCheck">
	/// The filter.
	/// </param>
	/// <param name="tolerance">
	/// The tolerance.
	/// </param>
	/// <returns>
	/// True if ranges match.
	/// </returns>
	private bool MatchRanges(ScanDefinition scanTypeToCheck, double tolerance)
	{
		Range[] ranges = scanTypeToCheck.Ranges;
		if (Ranges == null)
		{
			return true;
		}
		if (Ranges.Length > ranges.Length)
		{
			return false;
		}
		if (scanTypeToCheck.DataDependent)
		{
			tolerance = 0.2;
		}
		bool[] array = new bool[ranges.Length];
		for (int i = 0; i < Ranges.Length; i++)
		{
			Range obj = Ranges[i];
			double low = obj.Low;
			double high = obj.High;
			bool flag = false;
			for (int j = 0; j < ranges.Length; j++)
			{
				if (!array[j] && Math.Abs(low - ranges[j].Low) <= tolerance && Math.Abs(high - ranges[j].High) <= tolerance)
				{
					flag = (array[j] = true);
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>
	/// Compares the current object with another object of the same type.
	/// </summary>
	/// <returns>
	/// A 32-bit signed integer that indicates the relative order of the objects being compared.
	///  The return value has the following meanings: 
	///  Value              Meaning 
	///  Less than zero     This object is less than the <paramref name="other" /> parameter.
	///  Zero               This object is equal to <paramref name="other" />. 
	///  Greater than zero  This object is greater than <paramref name="other" />. 
	/// </returns>
	/// <param name="other">
	/// An object to compare with this object.
	/// </param>
	public int CompareTo(ScanDefinition other)
	{
		int num;
		for (int i = 0; i < _tokens.Count; i++)
		{
			if (i >= other._tokens.Count)
			{
				return 1;
			}
			num = _tokens[i].CompareTo(other._tokens[i]);
			if (num != 0)
			{
				return num;
			}
		}
		num = ComparePrecursors(other);
		if (num != 0)
		{
			return num;
		}
		num = CompareMassRanges(other);
		if (num != 0)
		{
			return num;
		}
		if (other._tokens.Count > _tokens.Count)
		{
			return -1;
		}
		return 0;
	}

	/// <summary>
	/// Compares the current Ranges with another object's Ranges.
	/// </summary>
	/// <returns>
	/// A 32-bit signed integer that indicates the relative order of the objects being compared.
	/// The return value has the following meanings: 
	/// Value               Meaning 
	/// Less than zero      This object is less than the <paramref name="other" /> parameter.
	/// Zero                This object is equal to <paramref name="other" />. 
	/// Greater than zero   This object is greater than <paramref name="other" />. 
	/// </returns>
	/// <param name="other">
	/// An object to compare with this object.
	/// </param>
	private int CompareMassRanges(ScanDefinition other)
	{
		if (Ranges.Length > other.Ranges.Length)
		{
			return 1;
		}
		if (Ranges.Length < other.Ranges.Length)
		{
			return -1;
		}
		for (int i = 0; i < Ranges.Length; i++)
		{
			int num = Ranges[i].CompareTo(other.Ranges[i]);
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	/// <summary>
	/// Compares the current object's Precursors with another object's Precursors.
	/// </summary>
	/// <returns>
	/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: 
	/// Value               Meaning 
	/// Less than zero      This object is less than the <paramref name="other" /> parameter.
	/// Zero                This object is equal to <paramref name="other" />. 
	/// Greater than zero   This object is greater than <paramref name="other" />. 
	/// </returns>
	/// <param name="other">
	/// An object to compare with this object.
	/// </param>
	private int ComparePrecursors(ScanDefinition other)
	{
		if (MsOrder > other.MsOrder)
		{
			return 1;
		}
		if (MsOrder < other.MsOrder)
		{
			return -1;
		}
		if (Precursors.Count > 0 && Precursors.Count == other.Precursors.Count)
		{
			for (int i = 0; i < Precursors.Count; i++)
			{
				int num = Precursors[i].CompareTo(other.Precursors[i]);
				if (num != 0)
				{
					return num;
				}
			}
		}
		return 0;
	}
}
