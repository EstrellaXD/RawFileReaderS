namespace ThermoFisher.CommonCore.RawFileReader.FileIoStructs;

/// <summary>
/// Defines the BufferInfo type for backward compatibility with FileIO.<para />
/// Ex. HomePage real time plot checks the Number Element field in scan index buffer info,<para />
/// to determine whether the scans data is available for display and <para />
/// raw file stitching is also using it. 
/// </summary>
internal struct BufferInfoStruct
{
	internal int NumElements;

	internal int StartIndex;

	internal int EndIndex;

	internal int StartOffset;

	internal int EndOffset;

	internal int Current;

	internal int Size;

	internal int Block;

	internal int FileHandle;

	internal int RandomOrder;

	internal int ReferenceCount;

	internal int InAcquisition;
}
