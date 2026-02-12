using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using ThermoFisher.CommonCore.RawFileReader.Facade.Constants;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.GenericItems;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Device.RunHeaders;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.MSReaction;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.OldLCQ;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.ExtraScanInfo;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.FTProfile;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.HRLRSPPackets;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.UVPackets;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanEventInfo;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.ScanIndex;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader;

/// <summary>
/// Provides commonly used functions for raw file reader and writer.
/// </summary>
internal static class Utilities
{
	public const int OneMb = 1048576;

	public const int TwoFiftySixBytes = 256;

	public const int FiveTwelveBytes = 512;

	public const int TwoKb = 2048;

	/// <summary>
	/// The struct size lookup.
	/// </summary>
	public static readonly Lazy<int[]> StructSizeLookup = new Lazy<int[]>(StructSizeLookupInitializer);

	public static readonly Lazy<bool> IsRunningMono = new Lazy<bool>(() => Type.GetType("Mono.Runtime") != null);

	public static readonly Lazy<bool> IsRunningUnderLinux = new Lazy<bool>(() => Environment.OSVersion.Platform == PlatformID.Unix);

	public static readonly Lazy<bool> IsRunningUnitTest = new Lazy<bool>(() => IsInTesting("Microsoft.VisualStudio.TestPlatform.TestFramework"));

	private static bool IsInTesting(string assemblyName)
	{
		return AppDomain.CurrentDomain.GetAssemblies().Any((Assembly a) => a.FullName.StartsWith(assemblyName));
	}

	/// <summary>
	/// Compares the doubles.
	/// </summary>
	/// <param name="valA">value A</param>
	/// <param name="valB">value B</param>
	/// <param name="tolerance">The tolerance.</param>
	/// <returns>
	///     Return an <see cref="T:System.Int32" /> that has one of three values:
	///     <list type="table">
	///         <listheader>
	///             <term>Value</term>
	///             <description>Meaning</description>
	///         </listheader>
	///         <term>Less than zero</term>
	///         <description>The current instance precedes the object specified by the CompareTo method in the sort order.</description>
	///         <term>Zero</term>
	///         <description>
	///             This current instance occurs in the same position in the sort order as the object specified by the
	///             CompareTo method.
	///         </description>
	///         <term>Greater than zero</term>
	///         <description>This current instance follows the object specified by the CompareTo method in the sort order.</description>
	///     </list>
	/// </returns>
	public static int CompareDoubles(double valA, double valB, double tolerance = double.Epsilon)
	{
		double num = valA - valB;
		if (Math.Abs(num) < tolerance)
		{
			return 0;
		}
		if (!(num > 0.0))
		{
			return -1;
		}
		return 1;
	}

	/// <summary>
	/// Loads the bytes from file.
	/// </summary>
	/// <param name="viewer">The viewer.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="startPos">The start position.</param>
	/// <param name="sizeOfObject">The size of object.</param>
	/// <param name="sizes">The sizes.</param>
	/// <returns>results in byte array</returns>
	private static byte[] LoadBytesFromFile(IMemoryReader viewer, int fileRevision, ref long startPos, int sizeOfObject, int[,] sizes)
	{
		byte[] array = null;
		int num = 0;
		do
		{
			if (fileRevision >= sizes[num, 0])
			{
				array = new byte[sizeOfObject];
				int count = sizes[num, 1];
				Buffer.BlockCopy(viewer.ReadBytesExt(ref startPos, count), 0, array, 0, count);
				break;
			}
		}
		while (++num < sizes.Length);
		return array;
	}

	/// <summary>
	/// Reads the structure.
	/// </summary>
	/// <typeparam name="T">The type of the parameter of the method that this delegate encapsulates.</typeparam>
	/// <param name="viewer">The viewer.</param>
	/// <param name="startPos">The start position.</param>
	/// <param name="fileRevision">The file revision.</param>
	/// <param name="sizes">The sizes.</param>
	/// <returns>The type of the return value of the method that this delegate encapsulates.</returns>
	public static T ReadStructure<T>(IMemoryReader viewer, ref long startPos, int fileRevision, int[,] sizes)
	{
		T result = default(T);
		int sizeOfObject = sizes[0, 1];
		byte[] array = LoadBytesFromFile(viewer, fileRevision, ref startPos, sizeOfObject, sizes);
		if (array != null)
		{
			GCHandle gCHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
			result = (T)Marshal.PtrToStructure(gCHandle.AddrOfPinnedObject(), typeof(T));
			gCHandle.Free();
		}
		return result;
	}

	/// <summary>
	/// Determines whether this instance is any.
	/// </summary>
	/// <typeparam name="T">The type of objects to enumerate</typeparam>
	/// <param name="data">The data.</param>
	/// <returns>True has data, otherwise False.</returns>
	public static bool IsAny<T>(this IEnumerable<T> data)
	{
		return data?.Any() ?? false;
	}

	/// <summary>
	/// Gets the enumeration values.
	/// </summary>
	/// <typeparam name="T">Type of the enumeration</typeparam>
	/// <param name="enumType">System type of the enumeration.</param>
	/// <param name="descriptions">The descriptions.</param>
	/// <param name="preFix">The pre fix.</param>
	/// <returns>Array of enumeration descriptions</returns>
	public static T[] GetEnumValues<T>(Type enumType, out string[] descriptions, string preFix = "") where T : struct
	{
		List<T> list = new List<T>();
		List<string> list2 = new List<string>();
		foreach (object value in Enum.GetValues(enumType))
		{
			list.Add((T)value);
			DescriptionAttribute descriptionAttribute = (DescriptionAttribute)Attribute.GetCustomAttribute(value.GetType().GetField(value.ToString()), typeof(DescriptionAttribute));
			list2.Add((descriptionAttribute != null) ? (descriptionAttribute.Description + preFix) : string.Empty);
		}
		descriptions = list2.ToArray();
		return list.ToArray();
	}

	/// <summary>
	/// Gets the name of the file map.
	/// </summary>
	/// <param name="fileName">The filename.</param>
	/// <param name="embeddedFile">if set to <c>true</c> [b embedded file].</param>
	/// <returns>A memory mapped file name without slashes.</returns>
	public static string GetFileMapName(string fileName, bool embeddedFile = false)
	{
		string fileName2 = CorrectNameForEnvironment(fileName);
		if (embeddedFile)
		{
			return RemoveSlashes(fileName2) + "EMBEDDED";
		}
		return RemoveSlashes(fileName2);
	}

	/// <summary>
	/// Create a shared memory mapped object name
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="mappedObjName">Name of the mapped object.</param>
	/// <returns>Memory mapped file name without slashes</returns>
	public static string MapName(string fileName, string mappedObjName)
	{
		return RemoveSlashes(fileName.ToLowerInvariant() + mappedObjName);
	}

	/// <summary>
	/// Removes the slashes.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <returns>A new string without any slashes</returns>
	public static string RemoveSlashes(string fileName)
	{
		StringBuilder stringBuilder = new StringBuilder();
		char[] array = fileName.ToCharArray();
		foreach (char c in array)
		{
			if (c != '\\' && c != '/')
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	/// <summary>
	/// Builds the name of the unique virtual device file map.
	/// </summary>
	/// <param name="virtualDeviceType">Type of the virtual device.</param>
	/// <param name="registeredIndex">Index of the registered.</param>
	/// <param name="fileMapName">Name of the file map.</param>
	/// <returns>A unique memory mapped file name</returns>
	public static string BuildUniqueVirtualDeviceFileMapName(VirtualDeviceTypes virtualDeviceType, int registeredIndex, string fileMapName)
	{
		fileMapName += virtualDeviceType switch
		{
			VirtualDeviceTypes.UvDevice => $"_UV_{registeredIndex}_", 
			VirtualDeviceTypes.PdaDevice => $"_PDA_{registeredIndex}_", 
			VirtualDeviceTypes.MsAnalogDevice => $"_MSA_{registeredIndex}_", 
			VirtualDeviceTypes.AnalogDevice => $"_AD_{registeredIndex}_", 
			_ => $"_MS_{registeredIndex}_", 
		};
		return fileMapName;
	}

	/// <summary>
	/// Builds the name of the unique virtual device stream temporary file.
	/// </summary>
	/// <param name="virtualDeviceType">Type of the virtual device.</param>
	/// <param name="registeredIndex">Index of the registered.</param>
	/// <returns>A unique virtual device stream temporary file name</returns>
	public static string BuildUniqueVirtualDeviceStreamTempFileName(VirtualDeviceTypes virtualDeviceType, int registeredIndex)
	{
		return virtualDeviceType switch
		{
			VirtualDeviceTypes.UvDevice => $"UV_{registeredIndex}_", 
			VirtualDeviceTypes.PdaDevice => $"PDA_{registeredIndex}_", 
			VirtualDeviceTypes.MsAnalogDevice => $"MSA_{registeredIndex}_", 
			VirtualDeviceTypes.AnalogDevice => $"AD_{registeredIndex}_", 
			VirtualDeviceTypes.MsDevice => $"MS_{registeredIndex}_", 
			VirtualDeviceTypes.StatusDevice => $"OTR_{registeredIndex}_", 
			_ => $"UKW_{registeredIndex}_", 
		};
	}

	/// <summary>
	/// Creates the no name mutex.
	/// <para />
	/// The mutex class enforces thread identity, so a mutex can be released only by the thread that acquired it.
	/// Mutex are of two types: local mutex, which are unnamed, and named system mutex.
	/// <para />
	/// A local mutex exists only within your process. It can be used by any thread in your process that has a reference 
	/// to the mutex object that represents the mutex. 
	/// <para />
	/// Each unnamed mutex object represents a separate local mutex.
	/// <para />
	/// On a server that is running Terminal Services, a named system mutex can have two levels of visibility. If its name 
	/// begins with the prefix "Global\", the mutex is visible in all terminal server sessions. If its name begins with the 
	/// prefix "Local\", the mutex is visible only in the terminal server session where it was created. In that case, a separate 
	/// mutex with the same name can exist in each of the other terminal server sessions on the server. If you do not specify a prefix 
	/// when you create a named mutex, it takes the prefix "Local\". Within a terminal server session, two mutex whose names differ 
	/// only by their prefixes are separate mutex, and both are visible to all processes in the terminal server session. That is, 
	/// the prefix names "Global\" and "Local\" describe the scope of the mutex name relative to terminal server sessions, 
	/// not relative to processes.
	/// </summary>
	/// <returns>A new instance of Mutex object.</returns>
	public static Mutex CreateNoNameMutex()
	{
		bool createdNew;
		return new Mutex(initiallyOwned: false, string.Empty, out createdNew);
	}

	/// <summary>
	/// Creates a named mutex then waits on it if it is busy.<para />
	/// Since we have an multithreading issue here (see OpenFileMapping and RunHeader bNewMapObject),<para />
	/// a Mutex is introduced here to ensure that only one thread gets access at a time.<para />
	/// This will create a mutex name based on the file that is being used and not just a global<para />
	/// mutex for all files
	/// </summary>
	/// <param name="fileName">Filename of the file.</param>
	/// <param name="useGlobalNamespace">True add Global prefix to the file name; otherwise no.</param>
	/// <param name="errors">Store errors information</param>
	/// <returns>
	/// The handle of the mutex or NULL if an error occurs.
	/// </returns>
	public static Mutex CreateNamedMutexAndWait(string fileName, bool useGlobalNamespace = true, DeviceErrors errors = null)
	{
		bool granted = false;
		Mutex mutex = null;
		errors?.AppendInformataion("Start CreateNamedMutexAndWait");
		string text = (useGlobalNamespace ? string.Format("{0}_{1}", "Global\\CBaseIO", GetFileMapName(fileName)) : GetFileMapName(fileName));
		try
		{
			errors?.AppendInformataion("CreateNamedMutexAndWait: mutex name " + text);
			bool createdNew = false;
			mutex = new Mutex(initiallyOwned: false, text, out createdNew);
			if (!mutex.WaitOne())
			{
				UpdateErrors(errors, "CreateNamedMutexAndWait: Not able to obtain the mutex.");
			}
			else
			{
				granted = true;
			}
		}
		catch (UnauthorizedAccessException exception)
		{
			errors?.AppendWarning("CreateNamedMutexAndWait: The named mutex exists and has access control security, but the user does not have right.");
			errors?.AppendWarning(exception.ToMessageAndCompleteStacktrace());
			mutex = RetryOpenMutex(text, out granted, errors);
		}
		catch (AbandonedMutexException ex)
		{
			errors?.UpdateError("CreateNamedMutexAndWait: it's okay to accept it, " + ex.Message, ex.HResult);
		}
		catch (Exception ex2)
		{
			UpdateErrors(errors, ex2.ToMessageAndCompleteStacktrace(), ex2.HResult);
		}
		finally
		{
			if (!granted && mutex != null)
			{
				mutex.ReleaseMutex();
				mutex.Close();
				mutex = null;
				errors?.AppendInformataion("CreateNamedMutexAndWait: release mutex due to fail to grant access to mutex");
			}
		}
		errors?.AppendInformataion("End CreateNamedMutexAndWait");
		return mutex;
	}

	/// <summary>
	/// Releases the mutex.
	/// </summary>
	/// <param name="mutex">The mutex.</param>
	public static void ReleaseMutex(ref Mutex mutex)
	{
		if (mutex == null)
		{
			return;
		}
		bool flag = false;
		try
		{
			mutex.WaitOne();
			flag = true;
		}
		finally
		{
			if (flag && mutex != null)
			{
				mutex.ReleaseMutex();
				mutex.Close();
				mutex = null;
			}
		}
	}

	/// <summary>
	/// Retries the method if it fails.
	/// </summary>
	/// <typeparam name="T">The type of the return parameter of the method that this delegate encapsulates.</typeparam>
	/// <param name="method">The method.</param>
	/// <param name="name">Memory mapped file name</param>
	/// <param name="numRetries">The number retries.</param>
	/// <param name="msToWaitBeforeRetry">The millisecond to wait before retry.</param>
	/// <returns>True the method get executed, false otherwise. </returns>
	public static T RetryMethod<T>(Func<string[], T> method, string[] name, int numRetries, int msToWaitBeforeRetry)
	{
		T result = default(T);
		do
		{
			try
			{
				result = method(name);
				return result;
			}
			catch (Exception)
			{
				if (numRetries <= 0)
				{
					throw;
				}
				Thread.Sleep(msToWaitBeforeRetry);
			}
		}
		while (numRetries-- > 0);
		return result;
	}

	/// <summary>
	/// Retries the method.
	/// </summary>
	/// <typeparam name="T">The type of the parameter of the method that this delegate encapsulates.</typeparam>
	/// <param name="method">The method.</param>
	/// <param name="numRetries">The number retries.</param>
	/// <param name="timeToWaitBeforeRetry">The millisecond to wait before retry.</param>
	/// <returns>The type of the return value of the method that this delegate encapsulates.</returns>
	public static T RetryMethod<T>(Func<T> method, int numRetries, int timeToWaitBeforeRetry)
	{
		T result = default(T);
		do
		{
			try
			{
				result = method();
				return result;
			}
			catch (Exception)
			{
				if (numRetries <= 0)
				{
					throw;
				}
				Thread.Sleep(timeToWaitBeforeRetry);
			}
		}
		while (numRetries-- > 0);
		return result;
	}

	/// <summary>
	/// Makes the long.
	/// </summary>
	/// <param name="lowPart">The low part.</param>
	/// <param name="highPart">The high part.</param>
	/// <returns>Combine the low and high part to an integer</returns>
	internal static int MakeLong(short lowPart, short highPart)
	{
		return (ushort)lowPart | (highPart << 16);
	}

	/// <summary>
	/// Validates the name of the file.
	/// </summary>
	/// <param name="fileName">Name of the file.</param>
	/// <param name="errors">Error message</param>
	/// <returns>True if file is existed; otherwise false.</returns>
	/// <exception cref="T:System.ArgumentException">
	/// File doesn't exist:  + fileName
	/// </exception>
	internal static bool ValidateFileName(string fileName, out string errors)
	{
		errors = string.Empty;
		if (string.IsNullOrWhiteSpace(fileName))
		{
			errors = Resources.ErrorEmptyNullFileName;
			return false;
		}
		if (!File.Exists(fileName))
		{
			errors = "File doesn't exist: " + fileName;
			return false;
		}
		return true;
	}

	/// <summary>
	/// Validate64s the bit.
	/// </summary>
	/// <exception cref="T:System.ApplicationException">Only 64 bit applications are supported by this project</exception>
	internal static void Validate64Bit(string message = "")
	{
		if (!Environment.Is64BitProcess && !IsInUnitTest())
		{
			throw new ApplicationException("Only 64 bit applications are supported by this project" + message);
		}
		static bool IsInUnitTest()
		{
			return AppDomain.CurrentDomain.GetAssemblies().Any((Assembly a) => a.FullName.StartsWith("Microsoft.VisualStudio.QualityTools.UnitTestFramework"));
		}
	}

	/// <summary>
	/// Determines whether the current windows user is Administrator.
	/// </summary>
	/// <returns>True the current user has Administrator access</returns>
	public static bool IsAdministrator()
	{
		if (IsRunningUnderLinux.Value)
		{
			return false;
		}
		return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
	}

	/// <summary>
	/// To the message and complete stack trace.
	/// </summary>
	/// <param name="exception">The exception.</param>
	/// <returns>Exception message, including the stack trace.</returns>
	public static string ToMessageAndCompleteStacktrace(this Exception exception)
	{
		Exception ex = exception;
		StringBuilder stringBuilder = new StringBuilder();
		while (ex != null)
		{
			stringBuilder.AppendLine();
			stringBuilder.AppendLine("Exception type: " + ex.GetType().FullName);
			stringBuilder.AppendLine("Message       : " + ex.Message);
			stringBuilder.AppendLine("Stacktrace:");
			stringBuilder.AppendLine(ex.StackTrace);
			ex = ex.InnerException;
		}
		return stringBuilder.ToString();
	}

	/// <summary>
	/// Updates the errors.
	/// </summary>
	/// <param name="errors">The errors.</param>
	/// <param name="message">The message.</param>
	/// <param name="errorCode">The error code.</param>
	public static void UpdateErrors(DeviceErrors errors, string message, int errorCode = -1)
	{
		if (errors != null && errors.ErrorCode == 0)
		{
			errors.UpdateError(message, errorCode);
		}
	}

	/// <summary>
	/// Corrects the raw file name for environment. 
	/// If the application is running on Unix environment, don't adjust the file name. 
	/// File and directory names are case sensitive in Unix.  Convert it to lowercase when on 
	/// Windows OS. 
	/// </summary>
	/// <param name="name">Name of the raw file.</param>
	public static string CorrectNameForEnvironment(string name)
	{
		if (!IsRunningMono.Value && !IsRunningUnderLinux.Value)
		{
			return name.ToLowerInvariant();
		}
		return name;
	}

	/// <summary>
	/// Corrects the raw file name for environment. 
	/// If the application is running on Unix environment, don't adjust the file name. 
	/// File and directory names are case sensitive in Unix.  Convert it to lowercase when on 
	/// Windows OS. 
	/// </summary>
	/// <param name="name">Name of the raw file.</param>
	/// <param name="ignorePlatformKeepNameCaseIntact">The flag to indicate whether should keep the original name case intact</param>
	/// <returns>Default: in Windows OS, convert the raw file name to lowercase; otherwise, no changes need.</returns>
	public static string CorrectNameForEnvironment(string name, bool ignorePlatformKeepNameCaseIntact)
	{
		if (!ignorePlatformKeepNameCaseIntact)
		{
			return CorrectNameForEnvironment(name);
		}
		return name;
	}

	/// <summary>
	/// Structures the size lookup initializer.
	/// </summary>
	/// <returns>An array of internal structures size</returns>
	private static int[] StructSizeLookupInitializer()
	{
		return new int[26]
		{
			Marshal.SizeOf(typeof(GenericDataItemStruct)),
			Marshal.SizeOf(typeof(RawFileInfoStruct)),
			Marshal.SizeOf(typeof(RunHeaderStruct)),
			Marshal.SizeOf(typeof(FileHeaderStruct)),
			Marshal.SizeOf(typeof(SeqRowInfoStruct)),
			Marshal.SizeOf(typeof(UvScanIndexStruct)),
			Marshal.SizeOf(typeof(InstIdInfoStruct)),
			Marshal.SizeOf(typeof(AutoSamplerConfigStruct)),
			Marshal.SizeOf(typeof(AsrProfileIndexStruct)),
			Marshal.SizeOf(typeof(ProfileDataPacket64)),
			Marshal.SizeOf(typeof(MsReactionStruct)),
			Marshal.SizeOf(typeof(MsReactionStruct1)),
			Marshal.SizeOf(typeof(MsReactionStruct2)),
			Marshal.SizeOf(typeof(MsReactionStruct3)),
			Marshal.SizeOf(typeof(HighResSpTypeStruct)),
			Marshal.SizeOf(typeof(LowResSpTypeStruct)),
			Marshal.SizeOf(typeof(BufferInfoStruct)),
			Marshal.SizeOf(typeof(SequenceFileInfoStruct.SequenceInfo)),
			Marshal.SizeOf(typeof(ScanEventInfoStruct)),
			Marshal.SizeOf(typeof(ScanIndexStruct)),
			Marshal.SizeOf(typeof(NoiseInfoPacketStruct)),
			Marshal.SizeOf(typeof(PacketHeaderStruct)),
			Marshal.SizeOf(typeof(ProfileSegmentStruct)),
			Marshal.SizeOf(typeof(AuditDataStruct)),
			Marshal.SizeOf(typeof(StandardAccuracyStruct)),
			Marshal.SizeOf(typeof(VirtualControllerInfoStruct))
		};
	}

	/// <summary>
	/// The try reopen mutex.
	///  </summary>
	/// <param name="mutexName"> The mutex name.
	/// </param>
	/// <param name="granted">Grant access to the named mutex.</param>
	/// <param name="errors"> The errors. </param>
	/// <returns> The <see cref="T:System.Threading.Mutex" />. </returns>
	private static Mutex RetryOpenMutex(string mutexName, out bool granted, DeviceErrors errors)
	{
		errors?.AppendInformataion("Enter: RetryOpenMutex()");
		Mutex mutex = null;
		int num = 25;
		int millisecondsTimeout = 200;
		granted = false;
		do
		{
			try
			{
				errors?.AppendInformataion($"RetryOpenMutex: num retries: {25 - num}");
				mutex = new Mutex(initiallyOwned: false, mutexName, out var _);
				if (!mutex.WaitOne())
				{
					errors?.AppendInformataion("RetryOpenMutex() : Not able to obtain the mutex.");
				}
				else
				{
					granted = true;
					errors?.AppendInformataion("RetryOpenMutex() : We got access to mutex.");
				}
			}
			catch (WaitHandleCannotBeOpenedException exception)
			{
				errors?.AppendInformataion(exception.ToMessageAndCompleteStacktrace() ?? "");
			}
			catch (UnauthorizedAccessException ex)
			{
				errors?.AppendInformataion("RetryOpenMutex: It fails to open a mutex again, " + ex.Message);
			}
			catch (AbandonedMutexException ex2)
			{
				errors?.AppendWarning("RetryOpenMutex: it's okay to accept it, " + ex2.Message);
			}
			catch (Exception exception2)
			{
				errors?.AppendInformataion(exception2.ToMessageAndCompleteStacktrace() ?? "");
			}
			finally
			{
				if (!granted && mutex != null)
				{
					mutex.ReleaseMutex();
					mutex.Close();
					mutex = null;
					errors?.AppendInformataion("RetryOpenMutex: release mutex due to fail to grant access to mutex");
				}
			}
			if (granted)
			{
				break;
			}
			Thread.Sleep(millisecondsTimeout);
		}
		while (num-- > 0);
		errors?.AppendInformataion("Exit: RetryOpenMutex()");
		return mutex;
	}

	/// <summary>Loads the data to internal memory array reader that reduce small IO calls.</summary>
	/// <param name="func">The function.</param>
	/// <param name="memReader">The memory reader.</param>
	/// <param name="startPos">The start position.</param>
	/// <param name="bufferSize">Size of the buffer.</param>
	/// <returns>
	///   The next offset
	/// </returns>
	public static long LoadDataFromInternalMemoryArrayReader(Func<IMemoryReader, long, long> func, IMemoryReader memReader, long startPos, int bufferSize)
	{
		int num = 0;
		MemoryArrayReader arg = new MemoryArrayReader(memReader.SafeReadLargeData(startPos, bufferSize), num);
		long num2 = func(arg, num);
		return startPos + num2;
	}
}
