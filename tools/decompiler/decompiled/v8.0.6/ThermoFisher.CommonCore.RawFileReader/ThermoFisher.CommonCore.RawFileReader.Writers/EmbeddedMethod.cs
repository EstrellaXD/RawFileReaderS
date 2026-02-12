using System;
using System.IO;
using ThermoFisher.CommonCore.RawFileReader.StructWrappers;

namespace ThermoFisher.CommonCore.RawFileReader.Writers;

/// <summary>
/// The instrument method will be saved in the raw file.
/// </summary>
internal class EmbeddedMethod : IDisposable
{
	private readonly bool _shouldBeDeleted;

	private bool _isDisposed;

	/// <summary>
	/// Gets the instrument method.
	/// </summary>
	public Method InstrumentMethod { get; }

	/// <summary>
	/// Gets or sets the end offset.
	/// </summary>
	public long EndOffset { get; set; }

	/// <summary>
	/// Gets a value indicating whether this is partial save or full save.
	/// </summary>
	public bool IsPartialSave { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.Writers.EmbeddedMethod" /> class.
	/// </summary>
	/// <param name="instrumentMethod"> The instrument Method. </param>
	/// <param name="shouldBeDeleted">
	/// The should dispose flag indicating whether the embedded instrument method should be removed after the raw file writer is closed.
	/// </param>
	public EmbeddedMethod(Method instrumentMethod, bool shouldBeDeleted)
	{
		InstrumentMethod = instrumentMethod;
		_shouldBeDeleted = shouldBeDeleted;
		IsPartialSave = true;
		EndOffset = -1L;
	}

	/// <summary>
	/// The save method.
	/// </summary>
	/// <param name="binaryWriter"> The binary writer. </param>
	/// <param name="errors"> The errors. </param>
	/// <param name="partialSave"> The partial save. </param>
	/// <returns>True is successful</returns>
	public bool SaveMethod(BinaryWriter binaryWriter, DeviceErrors errors, bool partialSave)
	{
		errors.AppendInformataion("Start SaveMethod");
		IsPartialSave = partialSave;
		bool result = this.Save(binaryWriter, errors);
		errors.AppendInformataion("End SaveMethod");
		return result;
	}

	/// <summary>
	/// The dispose.
	/// </summary>
	public void Dispose()
	{
		if (_isDisposed)
		{
			return;
		}
		_isDisposed = true;
		if (!_shouldBeDeleted)
		{
			return;
		}
		string fileName = InstrumentMethod.OriginalStorageName;
		if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
		{
			return;
		}
		try
		{
			Utilities.RetryMethod(delegate
			{
				File.Delete(fileName);
				return true;
			}, 2, 100);
		}
		catch
		{
		}
	}
}
