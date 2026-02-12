namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// Raw File Info structure
/// The offset where the field begins
/// </summary>
internal enum RawFileInfoFieldOffset
{
	NumberOfVirtualControllers = 28,
	NextAvailableControllerIndex = 32,
	VirtualControllerInfo = 816
}
