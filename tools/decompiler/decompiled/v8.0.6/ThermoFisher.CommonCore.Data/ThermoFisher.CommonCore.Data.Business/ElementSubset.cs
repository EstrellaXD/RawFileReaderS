using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// Defines an isotope of an element, and quantity limits of that isotope.
/// This is an input parameter to elemental composition searching.
/// <see cref="T:ThermoFisher.CommonCore.Data.Business.ElementsSubsetCollection" />
/// </summary>
[Serializable]
[DataContract]
public class ElementSubset : CommonCoreDataObject
{
	/// <summary>
	/// The chemical symbol.
	/// </summary>
	private string _sign;

	/// <summary>
	/// The min abs count of this isotope.
	/// </summary>
	private double _minAbs;

	/// <summary>
	/// The max abs count of this isotope
	/// </summary>
	private double _maxAbs;

	/// <summary>
	/// The min relative count of this isotope.
	/// </summary>
	private double _minRel;

	/// <summary>
	/// The max relative count of this isotope
	/// </summary>
	private double _maxRel;

	/// <summary>
	/// If "relative" values (ratios) should be used.
	/// </summary>
	private bool _useRatio;

	/// <summary>
	/// Is this isotope used?
	/// </summary>
	private bool _inUse;

	/// <summary>
	/// The nominal mass
	/// </summary>
	private int _nominal;

	/// <summary>
	/// The isotope mass.
	/// </summary>
	private double _mass;

	/// <summary>
	/// Gets or sets the chemical symbol for the element
	/// </summary>
	[DataMember]
	public string Sign
	{
		get
		{
			return _sign;
		}
		set
		{
			_sign = value;
		}
	}

	/// <summary>
	/// Gets or sets the maximum count of this isotope in returned formula
	/// </summary>
	[XmlIgnore]
	public double Maximum
	{
		get
		{
			if (!_useRatio)
			{
				return MaximumAbs;
			}
			return MaximumRelative;
		}
		set
		{
			if (value >= 0.0 && value <= 10000.0)
			{
				if (_useRatio)
				{
					MaximumRelative = value;
				}
				else
				{
					MaximumAbs = value;
				}
			}
		}
	}

	/// <summary>
	/// Gets or sets the maximum count of this element in the returned formula
	/// </summary>
	[XmlIgnore]
	public double Minimum
	{
		get
		{
			if (!_useRatio)
			{
				return MinimumAbs;
			}
			return MinimumRelative;
		}
		set
		{
			if (value >= 0.0 && value <= 10000.0)
			{
				if (_useRatio)
				{
					MinimumRelative = value;
				}
				else
				{
					MinimumAbs = value;
				}
			}
		}
	}

	/// <summary>
	/// Gets or sets the maximum count of this isotope in returned formulae
	/// </summary>
	[DataMember]
	public double MaximumAbs
	{
		get
		{
			return _maxAbs;
		}
		set
		{
			_maxAbs = value;
		}
	}

	/// <summary>
	/// Gets or sets the minimum count of this isotope in returned formulae
	/// </summary>
	[DataMember]
	public double MinimumAbs
	{
		get
		{
			return _minAbs;
		}
		set
		{
			_minAbs = value;
		}
	}

	/// <summary>
	/// Gets or sets the minimum relative amount of this isotope
	/// </summary>
	[DataMember]
	public double MaximumRelative
	{
		get
		{
			return _maxRel;
		}
		set
		{
			_maxRel = value;
		}
	}

	/// <summary>
	/// Gets or sets the maximum relative amount of this isotope
	/// </summary>
	[DataMember]
	public double MinimumRelative
	{
		get
		{
			return _minRel;
		}
		set
		{
			_minRel = value;
		}
	}

	/// <summary>
	/// Gets or sets the nominal mass of this isotope
	/// </summary>
	[DataMember]
	public int NominalMass
	{
		get
		{
			return _nominal;
		}
		set
		{
			_nominal = value;
		}
	}

	/// <summary>
	/// Gets the isotope including its nominal mass. For example "12 C" or "13 C".
	/// </summary>
	public string SymbolForBinding => NominalMass + " " + Sign;

	/// <summary>
	/// Gets or sets the exact mass of the isotope
	/// </summary>
	[DataMember]
	public double Mass
	{
		get
		{
			return _mass;
		}
		set
		{
			_mass = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether this element is used for searching.
	/// </summary>
	[DataMember]
	public bool InUse
	{
		get
		{
			return _inUse;
		}
		set
		{
			_inUse = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the "Relative" min and max count of isotopes is used.
	/// </summary>
	[DataMember]
	public bool UseRatio
	{
		get
		{
			return _useRatio;
		}
		set
		{
			_useRatio = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ElementSubset" /> class. 
	/// Default constructor
	/// </summary>
	public ElementSubset()
	{
		_sign = "-";
		_maxAbs = 200.0;
		_maxRel = 1.0;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.ElementSubset" /> class. 
	/// Construct from all possible settings
	/// (Obsolete form, use initializer, as many of the values have the same type,
	/// and this is not "order safe").
	/// </summary>
	/// <param name="sign">
	/// The sign.
	/// </param>
	/// <param name="mass">
	/// The mass.
	/// </param>
	/// <param name="nominal">
	/// The nominal.
	/// </param>
	/// <param name="minAbs">
	/// The min Abs.
	/// </param>
	/// <param name="maxAbs">
	/// The max Abs.
	/// </param>
	/// <param name="minRelative">
	/// The min Relative.
	/// </param>
	/// <param name="maxRelative">
	/// The max Relative.
	/// </param>
	/// <param name="useRatio">
	/// The use Ratio.
	/// </param>
	/// <param name="inUse">
	/// The in Use.
	/// </param>
	[Obsolete("Construct and set properties")]
	public ElementSubset(string sign, double mass, int nominal, double minAbs, double maxAbs, double minRelative, double maxRelative, bool useRatio, bool inUse)
	{
		_sign = sign;
		_minAbs = minAbs;
		_maxAbs = maxAbs;
		_minRel = minRelative;
		_maxRel = maxRelative;
		_useRatio = useRatio;
		_inUse = inUse;
		_nominal = nominal;
		_mass = mass;
	}
}
