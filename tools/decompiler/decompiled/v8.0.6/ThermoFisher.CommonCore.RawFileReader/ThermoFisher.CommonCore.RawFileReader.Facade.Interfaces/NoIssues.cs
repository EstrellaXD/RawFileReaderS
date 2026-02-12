namespace ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;

internal class NoIssues : IReaderIssues
{
	public bool FileSizeExceeded => false;

	public long MaxSize => 0L;
}
