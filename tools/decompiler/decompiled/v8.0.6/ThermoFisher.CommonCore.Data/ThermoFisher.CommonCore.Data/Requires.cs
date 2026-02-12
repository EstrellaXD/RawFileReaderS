using System;
using System.Collections;
using System.Diagnostics;

namespace ThermoFisher.CommonCore.Data;

/// <summary>
/// Contains generic validation methods.
/// </summary>
public static class Requires
{
	/// <summary>
	/// Validates that the specified <paramref name="value" /> is not null.
	/// </summary>
	/// <typeparam name="T">type of the parameter</typeparam>
	/// <param name="parameterName">Name of the parameter.</param>
	/// <param name="value">The value to check.</param>
	/// <exception cref="T:System.ArgumentNullException"> is thrown if argument is null.</exception>
	[DebuggerStepThrough]
	public static void NotNull<T>(string parameterName, T value) where T : class
	{
		if (value == null)
		{
			throw new ArgumentNullException(parameterName);
		}
	}

	/// <summary>
	/// Validates that the specified <paramref name="value" /> is not null or empty.
	/// </summary>
	/// <param name="parameterName">Name of the parameter.</param>
	/// <param name="value">The value to check.</param>
	/// <exception cref="T:System.ArgumentNullException"> is thrown if argument is null.</exception>
	/// <exception cref="T:System.ArgumentException"> is thrown if argument is empty.</exception>
	[DebuggerStepThrough]
	public static void NotNullOrEmpty(string parameterName, IEnumerable value)
	{
		NotNull(parameterName, value);
		if (!value.GetEnumerator().MoveNext())
		{
			throw new ArgumentException("Argument cannot be empty.", parameterName);
		}
	}
}
