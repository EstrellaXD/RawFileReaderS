using System;

namespace ThermoFisher.CommonCore.Data.Interfaces;

/// <summary>
/// Single letter flags, upper case
/// note that E (Enhanced) and Z (ZoomScan) are previously allocated
/// p,c,d are critical flags and so should have alternative meaning with different casing.
/// so 21 are used
/// 32 available flags (8 groups of 4 in hex)
/// </summary>
[Flags]
public enum UpperCaseFilterFlags
{
	/// <summary>
	/// Upper case A Flag
	/// </summary>
	UpperA = 1,
	/// <summary>
	/// Upper case B Flag
	/// </summary>
	UpperB = 2,
	/// <summary>
	/// Upper case F Flag
	/// </summary>
	UpperF = 4,
	/// <summary>
	/// Upper case G Flag
	/// </summary>
	UpperG = 8,
	/// <summary>
	/// Upper case H Flag
	/// </summary>
	UpperH = 0x10,
	/// <summary>
	/// Upper case I Flag
	/// </summary>
	UpperI = 0x20,
	/// <summary>
	/// Upper case J Flag
	/// </summary>
	UpperJ = 0x40,
	/// <summary>
	/// Upper case K Flag
	/// </summary>
	UpperK = 0x80,
	/// <summary>
	/// Upper case L Flag
	/// </summary>
	UpperL = 0x100,
	/// <summary>
	/// Upper case M Flag
	/// </summary>
	UpperM = 0x200,
	/// <summary>
	/// Upper case N Flag
	/// </summary>
	UpperN = 0x400,
	/// <summary>
	/// Upper case O Flag
	/// </summary>
	UpperO = 0x800,
	/// <summary>
	/// Upper case Q Flag
	/// </summary>
	UpperQ = 0x1000,
	/// <summary>
	/// Upper case R Flag
	/// </summary>
	UpperR = 0x2000,
	/// <summary>
	/// Upper case S Flag
	/// </summary>
	UpperS = 0x4000,
	/// <summary>
	/// Upper case T Flag
	/// </summary>
	UpperT = 0x8000,
	/// <summary>
	/// Upper case U Flag
	/// </summary>
	UpperU = 0x10000,
	/// <summary>
	/// Upper case V Flag
	/// </summary>
	UpperV = 0x20000,
	/// <summary>
	/// Upper case W Flag
	/// </summary>
	UpperW = 0x40000,
	/// <summary>
	/// Upper case X Flag
	/// </summary>
	UpperX = 0x80000,
	/// <summary>
	/// Upper case Y Flag
	/// </summary>
	UpperY = 0x100000
}
