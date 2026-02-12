namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

/// <summary>
/// The system time structure from C++.
/// </summary>
internal struct SystemTimeStruct
{
	internal ushort Year;

	internal ushort Month;

	internal ushort DayOfWeek;

	internal ushort Day;

	internal ushort Hour;

	internal ushort Minute;

	internal ushort Second;

	internal ushort Milliseconds;
}
