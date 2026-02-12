namespace ThermoFisher.CommonCore.RawFileReader.Facade.Constants;

/// <summary>
/// The old LCQ constants.
/// </summary>
internal static class OldLcqConstants
{
	/// <summary>
	/// The maximum ext detector channels
	/// External detector channels in use -- 1 to 4
	/// </summary>
	public const int MaxExtDetectorChannels = 4;

	public const int MaxTurboStatusLength = 14;

	public const int MaxInstStatusLength = 10;

	public const int MaxAnalysisLength = 15;

	public const int MaxSyringePumpStatusLength = 14;

	public const int MaxDivertValvePositionLength = 7;

	public const int MaxAutosamplerStatusLength = 21;

	public const int MaxLCStatusLength = 22;

	public const int MaxUVDetectorStatusLength = 14;

	public const int MaxDataTypeLength = 9;

	public const int MaxSourceTypeLength = 16;

	public const int MaxPolarityLength = 9;

	public const int AsNotConnected = 0;

	public const int LcNotConnected = 0;

	public const double MinAnalogSignal = 3.0 / 256.0;

	public const byte Channel0 = 1;

	public const byte Channel1 = 2;

	public const byte Channel2 = 4;

	public const byte Channel3 = 8;

	public const byte SMUP = 1;

	public const byte SMDOWN = 2;

	public const byte SMBAD = 4;

	public const byte SMPLOS = 8;

	public const byte SMTLR = 16;

	public const byte SMTHR = 32;

	public const byte SMTPF = 64;

	public const byte SMTPF2 = 128;

	public const byte SMA1PO = 1;

	public const byte SMA1NE = 2;

	public const byte SMA1LM = 4;

	public const byte SMA1HM = 8;

	public const byte SMA1AB = 16;

	public const byte SMA2PO = 32;

	public const byte SMA2NE = 64;

	public const byte SMA2LM = 128;

	public const byte SMA2HM = 1;

	public const byte SMA2AB = 2;

	public const byte SMA3PO = 4;

	public const byte SMA3NE = 8;

	public const byte SMA3LM = 16;

	public const byte SMA3HM = 32;

	public const byte SMA3AB = 64;

	public const byte SMA4PO = 128;

	public const byte SMA4NE = 1;

	public const byte SMA4LM = 2;

	public const byte SMA4HM = 4;

	public const byte SMA4AB = 8;

	public const byte SMBSCN = 16;

	public const byte SMESCN = 32;

	public const byte SMEXPL = 64;

	public const byte SMLINR = 128;

	public const byte SMSEI = 1;

	public const byte SMSCI = 2;

	public const byte SMSPOS = 4;

	public const byte SMSNEG = 8;

	public const byte SMSFD = 16;

	public const byte SMSFAB = 32;

	public const byte SMSTSP = 64;

	public const byte SMSESI = 128;

	public const byte SMAPCI = 1;

	public const byte SMPAR = 2;

	public const byte SMDAU = 4;

	public const byte SMQ3MS = 8;

	public const byte SMQ1MS = 16;

	public const byte SMNGN = 32;

	public const byte SMNLO = 64;

	public const byte SMFF1 = 128;

	public const byte SmFf2 = 1;

	public const byte SmCgas = 2;

	public const byte SmPrf = 4;

	public const byte SmBeqq = 8;

	public const byte SmPmdt = 16;

	public const byte SmLasr = 32;

	public const byte SmMlti = 64;

	public const byte SmAver = 128;

	public const byte SmBsub = 1;

	public const byte SmSynt = 2;

	public const byte SmNgap = 4;

	public const byte SmAcid = 8;

	public const byte SmPatd = 16;

	public const byte SmNct = 32;

	private const string EI = "Ei";

	private const string CI = "Ci";

	private const string ESI = "Esi";

	private const string APCI = "Apci";

	private const string LD = "Ld";

	private const string FAB = "Fab";

	private const string PB = "Pb";

	private const string TMS = "Tms";

	public const string Unknown = "Unknown";

	private const string Acquiring = "Acquiring";

	private const string StopPending = "Stop Pending";

	private const string Pending = "Pending";

	private const string Paused = "Pause";

	public const string Load = "Load";

	public const string Inject = "Inject";

	public const string Error = "Error";

	private const string LcWaitingForInjection = "Waiting for Injection";

	private const string LcInLocal = "In Local";

	private const string LcInitializing = "Initializing";

	private const string AsHasInjected = "Has Injected";

	private const string AsStopping = "Stopping";

	private const string AsStopped = "Stopped";

	private const string AsSearching = "Searching";

	private const string AsCleaningWithSolvent = "Cleaning with Solvent";

	private const string AsCleaningWithSample = "Cleaning with Sample";

	public const uint Fm3Typ = 2147483648u;

	public const uint Fm3Int = 268435455u;

	public const uint SatFlag = 1073741824u;

	public const uint SampleMask = 268435455u;

	public const uint SampleFlag = 2147483648u;

	public const uint ScaleFlag = 128u;

	public const byte ScanRateNormal = 0;

	public const byte ScanRateZoom = 1;

	public const byte ScanRateHighMass = 2;

	public const byte ScanRateTurbo = 3;

	public const byte ScanRateTurboAgc = 4;

	public const byte MinNonDependentScanMode = 0;

	public const byte MaxNonDependentScanMode = 9;

	public const byte MinDependentScanMode = 10;

	public const byte MaxDependentScanMode = 19;

	public const byte MinIcisScanMode = 24;

	public const byte MaxIcisScanMode = 31;

	public const double SourceCidThesold = 0.001;

	public const uint InstStatMask = 15u;

	public const uint InstOff = 0u;

	public const uint InstOn = 1u;

	public const uint InstBadStat = 2u;

	public const uint InstStdby = 3u;

	private const string NotConnected = "Not Connected";

	private const string NotReady = "Not Ready";

	private const string Ready = "Ready";

	private const string Running = "Running";

	private const string Connected = "Connected";

	private const string Off = "Off";

	private const string On = "On";

	private const string BadState = "Bad State";

	private const string StandBy = "Standby";

	private const string Idling = "Idle";

	private const string WaitingForInlet = "Wait for Inlet";

	private const string AsInjectiongSample = "Injecting Sample";

	public const string Profile = "Profile";

	public const string Centroid = "Centroid";

	public const string Positive = "Positive";

	public const string Negative = "Negative";

	public static readonly string[] SystemStatusStrings = new string[4] { "Off", "On", "Bad State", "Standby" };

	public static readonly string[] AnalysisStatusStrings = new string[6] { "Idle", "Wait for Inlet", "Acquiring", "Stop Pending", "Pending", "Pause" };

	public static readonly string[] AsStatusStrings = new string[10] { "Not Connected", "Not Ready", "Ready", "Has Injected", "Stopping", "Stopped", "Searching", "Cleaning with Solvent", "Cleaning with Sample", "Injecting Sample" };

	public static readonly string[] LcStatusStrings = new string[8] { "Not Connected", "Not Ready", "Ready", "Running", "Waiting for Injection", "In Local", "Initializing", "Connected" };

	public static readonly string[] UvStatusStrings = new string[5] { "Not Connected", "Not Ready", "Ready", "Running", "Connected" };

	public static readonly string[] SyringeStatusStrings = new string[5] { "Not Connected", "Not Ready", "Ready", "Running", "Connected" };

	public static readonly string[] TurboStatusStrings = new string[5] { "Not Connected", "Not Ready", "Ready", "Running", "Connected" };

	public static readonly string[] SourceTypeStrings = new string[9] { "Unknown", "Ei", "Ci", "Esi", "Apci", "Ld", "Fab", "Pb", "Tms" };
}
