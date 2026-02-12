using System;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers;

/// <summary>
/// The error log item.
/// </summary>
internal class ErrorLogItem : IComparable<ErrorLogItem>
{
	/// <summary>
	/// Gets the retention time.
	/// </summary>
	public double RetentionTime { get; }

	/// <summary>
	/// Gets the error text.
	/// </summary>
	public string ErrorText { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.ErrorLogItem" /> class.
	/// The error is a list of text strings indexed by the retention time.
	/// </summary>
	/// <param name="retentionTime">
	/// The retention time.
	/// </param>
	/// <param name="text">
	/// The text.
	/// </param>
	public ErrorLogItem(double retentionTime, string text)
	{
		RetentionTime = retentionTime;
		ErrorText = text;
	}

	/// <summary>
	/// Compare the current instance's retention time with another object of the same type.
	/// </summary>
	/// <param name="other">
	/// The other instance.
	/// </param>
	/// <returns>
	/// -1 indicates the RT of the current instance is less than another object, 
	/// 0 indicates same, 
	/// 1 indicates the RT of the current instance is greater than another object.</returns>
	public int CompareTo(ErrorLogItem other)
	{
		double num = RetentionTime - other.RetentionTime;
		if (Math.Abs(num) < double.Epsilon)
		{
			return 0;
		}
		if (!(num > 0.0))
		{
			return -1;
		}
		return 1;
	}
}
