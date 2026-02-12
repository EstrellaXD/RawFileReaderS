using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Provides a delegate to open files for specific extensions
/// given the 'type' and 'assembly'.
/// The class calls the method that has a single string parameter
/// and returns T.
/// This class can also be used in conjunction with the 
/// '<see cref="T:ThermoFisher.CommonCore.Data.ObjectFactoryCollection`1" />'. In such a situation, it is not 
/// required that Initialize' is called. 
/// </summary>
/// <typeparam name="T">This should be an interface to data read from a file.
/// </typeparam>
public class ObjectFactory<T> where T : class
{
	/// <summary>
	/// The default name of a method which can return {T}
	/// </summary>
	protected const string DefaultMethodName = "FileFactory";

	private readonly string _assemblyToLoad = string.Empty;

	private readonly string _typeToInit = string.Empty;

	private readonly string _methodName = "FileFactory";

	private MethodInfo _methodInfo;

	private readonly Type[] _defaultMethodParameterTypes = new Type[1] { typeof(string) };

	/// <summary>
	/// Gets the delegate to invoke.
	/// </summary>
	public Func<string, T> OpenFile { get; private set; }

	/// <summary>
	/// Gets the delegate to invoke a specified method.
	/// </summary>
	/// <value>
	/// The specified method.
	/// </value>
	public Func<object[], T> SpecifiedMethod { get; private set; }

	/// <summary>
	/// Gets or sets the file extension associated with this instance.
	/// </summary>
	public string FileExtension { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether Initialized.
	/// </summary>
	private bool Initialized { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.ObjectFactory`1" /> class. 
	/// Associates the extension, class type and the assembly
	/// with this instance. 
	/// The default method with name "FileFactory" will be invoked.
	/// </summary>
	/// <param name="extension">
	/// file extension
	/// </param>
	/// <param name="typeToInitialize">
	/// fully qualified class name to initialize
	/// </param>
	/// <param name="assemblyToLoad">
	/// fully qualified assembly name
	/// </param>
	public ObjectFactory(string extension, string typeToInitialize, string assemblyToLoad)
	{
		FileExtension = extension;
		_assemblyToLoad = assemblyToLoad;
		_typeToInit = typeToInitialize;
		OpenFile = FileFactory;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.ObjectFactory`1" /> class. 
	/// Associates the extension, class type and the assembly
	/// with this instance. 
	/// The method name can also be passed if it is different. 
	/// However, for the method to be invoked it needs to match the 
	/// delegate's signature.
	/// </summary>
	/// <param name="extension">
	/// file extension
	/// </param>
	/// <param name="typeToInitialize">
	/// fully qualified class name to initialize
	/// </param>
	/// <param name="assemblyToLoad">
	/// fully qualified assembly name
	/// </param>
	/// <param name="methodName">
	/// method name
	/// </param>
	/// <param name="initialize">initialize, but loading the required assembly and type</param>
	public ObjectFactory(string extension, string typeToInitialize, string assemblyToLoad, string methodName, bool initialize = false)
		: this(extension, typeToInitialize, assemblyToLoad)
	{
		if (!string.IsNullOrEmpty(methodName))
		{
			_methodName = methodName;
		}
		if (initialize)
		{
			Initialize();
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.ObjectFactory`1" /> class. 
	/// </summary>
	/// <param name="method">
	/// The method to open a file which returns {T}, given a file name.
	/// </param>
	/// <param name="extension">
	/// The file extension.
	/// </param>
	public ObjectFactory(Func<string, T> method, string extension)
	{
		OpenFile = method;
		FileExtension = extension;
		Initialized = true;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.ObjectFactory`1" /> class. 
	/// This version permits a method to be specified, which is called with
	/// parameters of the given types.
	/// </summary>
	/// <param name="typeToInitialize">
	/// The type to initialize.
	/// </param>
	/// <param name="assemblyToLoad">
	/// The assembly to load.
	/// </param>
	/// <param name="methodName">
	/// Name of the method.
	/// </param>
	/// <param name="paramTypes">
	/// Parameter types of the specified method 
	/// </param>
	/// <param name="initialize">
	/// if set to <c>true</c> [initialize].
	/// </param>
	public ObjectFactory(string typeToInitialize, string assemblyToLoad, string methodName, Type[] paramTypes, bool initialize = false)
	{
		if (!string.IsNullOrEmpty(methodName))
		{
			_methodName = methodName;
		}
		_defaultMethodParameterTypes = paramTypes;
		_assemblyToLoad = assemblyToLoad;
		_typeToInit = typeToInitialize;
		SpecifiedMethod = SpecifiedMethodFactory;
		if (initialize)
		{
			Initialize();
		}
	}

	/// <summary>
	/// Loads the assembly, class and determines the method to invoke.
	/// Needs to be called immediately after the constructor. This call is
	/// required when the class is used as a stand-alone class. 
	/// If used with <see cref="T:ThermoFisher.CommonCore.Data.ObjectFactoryCollection`1" />, then constructing
	/// <see cref="T:ThermoFisher.CommonCore.Data.ObjectFactoryCollection`1" /> will initialize the <see cref="T:ThermoFisher.CommonCore.Data.ObjectFactory`1" />.
	/// </summary>
	/// <returns>
	/// The initialize.
	/// </returns>
	public bool Initialize()
	{
		return Initialize(throwExceptions: true);
	}

	/// <summary>
	/// Load the assembly
	/// </summary>
	/// <param name="throwExceptions">
	/// Permit exceptions to be thrown
	/// </param>
	/// <returns>
	/// True is successful
	/// </returns>
	/// <exception cref="T:System.ArgumentException">
	/// <c>ArgumentException</c>.
	/// </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	/// <c>FileLoadException</c>.
	/// </exception>
	/// <exception cref="T:System.IO.FileLoadException">
	/// <c>NotSupportedException</c> Thrown when the expected method signature is not found.
	/// </exception>
	internal bool Initialize(bool throwExceptions)
	{
		if (!Initialized)
		{
			string text = string.Empty;
			try
			{
				text = Path.GetDirectoryName(typeof(ObjectFactory<>).GetTypeInfo().Assembly.Location);
				if (text != null)
				{
					string text2 = Path.Combine(text, _assemblyToLoad);
					Assembly assembly = Assembly.LoadFrom(text2);
					AssemblyName[] referencedAssemblies = assembly.GetReferencedAssemblies();
					for (int i = 0; i < referencedAssemblies.Length; i++)
					{
						Assembly.Load(referencedAssemblies[i]);
					}
					Type type = assembly.GetType(_typeToInit);
					if (type == null)
					{
						throw new ArgumentException(_typeToInit + " Type not found in the assembly " + text2);
					}
					CreateMethodInfoObject(type);
					Initialized = true;
				}
			}
			catch (Exception ex)
			{
				if (throwExceptions)
				{
					if (ex is ArgumentException || ex is NotSupportedException)
					{
						throw;
					}
					throw new FileLoadException(_assemblyToLoad + " is not found in " + text, ex);
				}
			}
		}
		return Initialized;
	}

	/// <summary>
	/// Checks for a specific method signature as below.
	/// <see cref="M:ThermoFisher.CommonCore.Data.ObjectFactory`1.FileFactory(System.String)" />(string)
	/// </summary>
	/// <param name="assemblyType">
	/// Type loaded from assembly (by reflection)
	/// </param>
	/// <exception cref="T:System.NotSupportedException">
	/// <c>NotSupportedException</c>.
	/// </exception>
	private void CreateMethodInfoObject(Type assemblyType)
	{
		MethodInfo[] methods = assemblyType.GetMethods(BindingFlags.Static | BindingFlags.Public);
		foreach (MethodInfo methodInfo in methods)
		{
			if (!methodInfo.Name.Equals(_methodName))
			{
				continue;
			}
			ParameterInfo[] parameters = methodInfo.GetParameters();
			int num = parameters.Length;
			if (num == _defaultMethodParameterTypes.Length)
			{
				int j;
				for (j = 0; j < num && object.Equals(parameters[j].ParameterType, _defaultMethodParameterTypes[j]); j++)
				{
				}
				if (j == num)
				{
					_methodInfo = methodInfo;
					return;
				}
			}
		}
		throw new NotSupportedException("Method " + _methodName + "is not found in " + _typeToInit);
	}

	/// <summary>
	/// Opens the named file, returning an interface to access the file.
	/// In error conditions, some adapters may throw exceptions
	/// Caller should handle exceptions (typically "file not valid").
	/// </summary>
	/// <param name="fileName">
	/// Name of file to open
	/// </param>
	/// <returns>
	/// The opened file, or null if no file can be opened
	/// </returns>
	/// <exception cref="T:System.Reflection.TargetInvocationException">
	/// Sent if the DLL fails to return {T}. This may be thrown for a missing or corrupt file.
	/// </exception>
	/// <exception cref="T:System.InvalidCastException">
	/// Failed to cast to specified type {T}.
	/// This will be thrown if either, the return from the invoked factory method was null
	/// or the returned object converts to a null {T} with an "as" operator.
	/// </exception>
	private T FileFactory(string fileName)
	{
		try
		{
			return (_methodInfo.Invoke(null, new object[1] { fileName }) as T) ?? throw new InvalidCastException("Failed to cast returned object to T.");
		}
		catch (Exception ex)
		{
			Exception innerException = ex.InnerException;
			if (innerException is ArgumentException)
			{
				throw innerException;
			}
			throw new TargetInvocationException(string.Format(CultureInfo.CurrentCulture, "Method invocation failed on Method[{0}] Type[{1}] Assembly[{2}]", _methodName, _typeToInit, _assemblyToLoad), innerException);
		}
	}

	/// <summary>
	/// Invoke the specified method.
	/// </summary>
	/// <param name="parameters">The parameters.</param>
	/// <returns>An object of the expected type</returns>
	/// <exception cref="T:System.InvalidCastException">Failed to cast returned object to T.</exception>
	/// <exception cref="T:System.Reflection.TargetInvocationException"></exception>
	private T SpecifiedMethodFactory(object[] parameters)
	{
		try
		{
			return (_methodInfo.Invoke(null, parameters) as T) ?? throw new InvalidCastException("Failed to cast returned object to T.");
		}
		catch (Exception ex)
		{
			Exception innerException = ex.InnerException;
			if (innerException is ArgumentException)
			{
				throw innerException;
			}
			throw new TargetInvocationException(string.Format(CultureInfo.CurrentCulture, "Method invocation failed on Method[{0}] Type[{1}] Assembly[{2}]", _methodName, _typeToInit, _assemblyToLoad), innerException);
		}
	}
}
