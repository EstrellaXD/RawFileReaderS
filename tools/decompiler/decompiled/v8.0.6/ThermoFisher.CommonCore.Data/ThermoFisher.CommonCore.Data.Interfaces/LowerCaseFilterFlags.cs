using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// define a set of additional flags
/// These map out to previously unused lower case letters
/// These can either be off (0) or on (1)
/// </summary>
[Flags]
public enum LowerCaseFilterFlags
{
	/// <summary>
	/// Lower case E flag
	/// </summary>
	LowerE = 1,
	/// <summary>
	/// Lower case G flag
	/// </summary>
	LowerG = 2,
	/// <summary>
	/// Lower case H flag
	/// </summary>
	LowerH = 4,
	/// <summary>
	/// Lower case I flag
	/// </summary>
	LowerI = 8,
	/// <summary>
	/// Lower case J flag
	/// </summary>
	LowerJ = 0x10,
	/// <summary>
	///             Lower case K flag
	/// </summary>
	LowerK = 0x20,
	/// <summary>
	/// Lower case L flag
	/// </summary>
	LowerL = 0x40,
	/// <summary>
	/// Lower case M flag
	/// </summary>
	LowerM = 0x80,
	/// <summary>
	/// Lower case N flag
	/// </summary>
	LowerN = 0x100,
	/// <summary>
	/// Lower case O flag
	/// </summary>
	LowerO = 0x200,
	/// <summary>
	/// Lower case Q flag
	/// </summary>
	LowerQ = 0x4800,
	/// <summary>
	/// Lower case S flag
	/// </summary>
	LowerS = 0x800,
	/// <summary>
	/// Lower case X flag
	/// </summary>
	LowerX = 0x1000,
	/// <summary>
	/// Lower case Y flag
	/// </summary>
	LowerY = 0x2000
}
