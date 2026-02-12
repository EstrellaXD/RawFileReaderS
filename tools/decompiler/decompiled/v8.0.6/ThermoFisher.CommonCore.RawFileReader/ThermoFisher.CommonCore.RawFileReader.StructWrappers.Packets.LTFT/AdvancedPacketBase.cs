using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ThermoFisher.CommonCore.Data;
using ThermoFisher.CommonCore.Data.Business;
using ThermoFisher.CommonCore.Data.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.Facade.Interfaces;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.ExtraScanInfo;
using ThermoFisher.CommonCore.RawFileReader.FileIoStructs.Packets.FTProfile;
using ThermoFisher.CommonCore.RawFileReader.Readers;
using ThermoFisher.CommonCore.RawFileReader.Writers;

namespace ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT;

/// <summary>
/// The Advanced packet base.
/// This class defines base features of a mass spectrometry scan format
/// used by advanced instruments, such as "Linear Trap" or <c>Orbitrap</c>.
/// These detectors may store much more than just mass and intensity.
/// They may store both profile and centroid data for the same scans.
/// Centroid data may have additional fields, such as "resolution".
/// </summary>
internal abstract class AdvancedPacketBase : IMsPacket, IPacket, IRawObjectBase
{
	/// <summary>
	/// class to define a safe data structure for "no data"
	/// </summary>
	internal class EmptyExtendedScanData : IExtendedScanData
	{
		public long Header { get; }

		public ReadOnlyCollection<ITransientSegment> Transients { get; }

		public ReadOnlyCollection<IDataSegment> DataSegments { get; }

		public EmptyExtendedScanData()
		{
			Header = 0L;
			Transients = new ReadOnlyCollection<ITransientSegment>(Array.Empty<ITransientSegment>());
			DataSegments = new ReadOnlyCollection<IDataSegment>(Array.Empty<IDataSegment>());
		}
	}

	/// <summary>
	/// Class to decode additional data saved with a scan
	/// </summary>
	private class ExtendedScanDataClass : IExtendedScanData
	{
		/// <summary>
		/// Debug segment contains sub segments.
		/// Each has a header, followed by a block of data
		/// </summary>
		private class DataSegment : IDataSegment
		{
			/// <summary>
			/// Gets the header, which can be ued in an instrument specific way to decode this data
			/// </summary>
			public int Header { get; internal set; }

			/// <summary>
			/// Gets the data in this segment
			/// </summary>
			public byte[] Bytes => FileBytes?.Value ?? _emptyByteArray;

			/// <summary>
			/// Gets the data from the raw file.
			/// </summary>
			private Lazy<byte[]> FileBytes { get; set; }

			/// <summary>
			/// Initialize this object, to lazy read the data
			/// </summary>
			/// <param name="viewer"></param>
			/// <param name="offset"></param>
			/// <param name="length"></param>
			public void Init(IMemoryReader viewer, long offset, int length)
			{
				FileBytes = new Lazy<byte[]>(() => viewer.ReadLargeData(offset, length));
			}
		}

		/// <summary>
		/// Gets a transient from the raw file
		/// </summary>
		private class Transient : ITransientSegment
		{
			/// <summary>
			/// Gets an instrument specific transient header
			/// </summary>
			public int Header { get; set; }

			/// <summary>
			/// Gets the data for this transient
			/// </summary>
			public int[] Data => FileData?.Value ?? Array.Empty<int>();

			/// <summary>
			/// Gets transient data from the raw file.
			/// </summary>
			private Lazy<int[]> FileData { get; set; }

			/// <summary>
			/// Create a lazy loader for this data
			/// </summary>
			/// <param name="viewer">access to the raw file</param>
			/// <param name="offset">offset into the view</param>
			/// <param name="blockWords">Size of the data in 32 bit words.</param>
			public void Init(IMemoryReader viewer, long offset, int blockWords)
			{
				FileData = new Lazy<int[]>(() => viewer.ReadInts(offset, blockWords));
			}
		}

		private readonly List<IDataSegment> _dataSubSegments = new List<IDataSegment>();

		private readonly List<ITransientSegment> _transientSegments = new List<ITransientSegment>();

		public long Header { get; set; }

		/// <summary>
		/// Gets the transients
		/// </summary>
		public ReadOnlyCollection<ITransientSegment> Transients { get; }

		/// <summary>
		/// Gets the data segments
		/// </summary>
		public ReadOnlyCollection<IDataSegment> DataSegments { get; }

		/// <summary>
		/// constructs a class to get extended scan data
		/// </summary>
		/// <param name="viewer">view itto the raw file</param>
		/// <param name="offset">offset into the view</param>
		/// <param name="words">length of the data in 32 bit words</param>
		public ExtendedScanDataClass(IMemoryReader viewer, long offset, uint words)
		{
			if (!InitializeDebugBlocks(viewer, offset, words))
			{
				throw new ArgumentException("Invalid debug data format");
			}
			Transients = new ReadOnlyCollection<ITransientSegment>(_transientSegments);
			DataSegments = new ReadOnlyCollection<IDataSegment>(_dataSubSegments);
		}

		/// <summary>
		/// Prepare to return charge envelopes and centroid annotations. Call this once and in advance of accessing properties.
		/// </summary>
		/// <param name="viewer">The byte buffer being marked as debug section in the orbitrap scan</param>
		/// <param name="offset">offset into view for this data</param>
		/// <param name="words">number of 32 bit words in this data</param>
		/// <returns>Returns true on success, false if the data structure doesn't match the demands. Keep in mind that missing APD info is fine.</returns>
		private bool InitializeDebugBlocks(IMemoryReader viewer, long offset, uint words)
		{
			long num = words * 4;
			long num2 = num + offset;
			if (num < 4)
			{
				return false;
			}
			viewer.ReadUnsignedInt(offset);
			offset += 4;
			while (offset < num2 && offset < num2 - 8)
			{
				int num3 = viewer.ReadInt(offset);
				uint num4 = viewer.ReadUnsignedInt(offset + 4);
				uint num5 = num4 * 4;
				offset += 8;
				if (offset + num5 > num2)
				{
					break;
				}
				if ((num3 & 0x100) != 0)
				{
					Transient transient = new Transient();
					_transientSegments.Add(transient);
					transient.Header = num3;
					transient.Init(viewer, offset, (int)num4);
				}
				else
				{
					DataSegment dataSegment = new DataSegment
					{
						Header = num3
					};
					_dataSubSegments.Add(dataSegment);
					dataSegment.Init(viewer, offset, (int)num5);
				}
				offset += num5;
			}
			if (offset == num2)
			{
				return true;
			}
			return false;
		}
	}

	protected const int SizeOf2Uints = 8;

	protected const int SizeOfFloat = 4;

	protected const int SizeOfFtProfileSubSegmentStruct = 12;

	protected const int SizeOfLtProfileSubSegmentStruct = 8;

	private const int SizeOf2Floats = 8;

	private const int SizeOf3Floats = 12;

	private const int SizeOfDouble = 8;

	private const int SizeOfDword = 4;

	private const int SizeOfInt = 4;

	protected static readonly int ProfileSegmentStructSize = Utilities.StructSizeLookup.Value[22];

	private static readonly byte[] ExpansionFullWidthHalfHeight = BitConverter.GetBytes(1);

	private static readonly int NoiseInfoStructSize = Utilities.StructSizeLookup.Value[20];

	private static readonly PeakOptions[] OptionsLookup = new PeakOptions[32]
	{
		PeakOptions.None,
		PeakOptions.Modified,
		PeakOptions.Exception,
		PeakOptions.Exception | PeakOptions.Modified,
		PeakOptions.Reference,
		PeakOptions.Reference | PeakOptions.Modified,
		PeakOptions.Exception | PeakOptions.Reference,
		PeakOptions.Exception | PeakOptions.Reference | PeakOptions.Modified,
		PeakOptions.Merged,
		PeakOptions.Merged | PeakOptions.Modified,
		PeakOptions.Merged | PeakOptions.Exception,
		PeakOptions.Merged | PeakOptions.Exception | PeakOptions.Modified,
		PeakOptions.Merged | PeakOptions.Reference,
		PeakOptions.Merged | PeakOptions.Reference | PeakOptions.Modified,
		PeakOptions.Merged | PeakOptions.Exception | PeakOptions.Reference,
		PeakOptions.Merged | PeakOptions.Exception | PeakOptions.Reference | PeakOptions.Modified,
		PeakOptions.Fragmented,
		PeakOptions.Fragmented | PeakOptions.Modified,
		PeakOptions.Fragmented | PeakOptions.Exception,
		PeakOptions.Fragmented | PeakOptions.Exception | PeakOptions.Modified,
		PeakOptions.Fragmented | PeakOptions.Reference,
		PeakOptions.Fragmented | PeakOptions.Reference | PeakOptions.Modified,
		PeakOptions.Fragmented | PeakOptions.Exception | PeakOptions.Reference,
		PeakOptions.Fragmented | PeakOptions.Exception | PeakOptions.Reference | PeakOptions.Modified,
		PeakOptions.Fragmented | PeakOptions.Merged,
		PeakOptions.Fragmented | PeakOptions.Merged | PeakOptions.Modified,
		PeakOptions.Fragmented | PeakOptions.Merged | PeakOptions.Exception,
		PeakOptions.Fragmented | PeakOptions.Merged | PeakOptions.Exception | PeakOptions.Modified,
		PeakOptions.Fragmented | PeakOptions.Merged | PeakOptions.Reference,
		PeakOptions.Fragmented | PeakOptions.Merged | PeakOptions.Reference | PeakOptions.Modified,
		PeakOptions.Fragmented | PeakOptions.Merged | PeakOptions.Exception | PeakOptions.Reference,
		PeakOptions.Fragmented | PeakOptions.Merged | PeakOptions.Exception | PeakOptions.Reference | PeakOptions.Modified
	};

	private static readonly int PacketHeaderStructSize = Utilities.StructSizeLookup.Value[21];

	private readonly LabelPeaks _labelStreamData = new LabelPeaks();

	private byte[] _centroidBlob;

	private uint[] _nonDefaultFeatures;

	private bool _hasContinueProcessingLabelData;

	private bool _isExpandChargeLabels;

	private bool _isExpandExceptionLabels;

	private bool _isExpandFragmentedLabels;

	private bool _isExpandMergedLabels;

	private bool _isExpandModifiedLabels;

	private bool _isExpandReferenceLabels;

	private NoiseInfoPacketStruct[] _noisePackets;

	private static LabelPeak[] _emptyLabelPeakArray = Array.Empty<LabelPeak>();

	private static NoiseAndBaseline[] _emptyNoiseArray = Array.Empty<NoiseAndBaseline>();

	private static float[] _emptytFloatArray = Array.Empty<float>();

	private static double[] _emptyDoubletArray = Array.Empty<double>();

	private static uint[] _emptytUintArray = Array.Empty<uint>();

	private static byte[] _emptyByteArray = Array.Empty<byte>();

	private LabelPeak[] _referencePeaks = _emptyLabelPeakArray;

	private float[] _widths;

	private bool _profileIsLazy;

	private static LabelPeak[] EmptyLabels = Array.Empty<LabelPeak>();

	private int _centroidBlobOffset;

	/// <summary>
	/// Gets the label peaks.
	/// </summary>
	public LabelPeak[] LabelPeaks => _labelStreamData.Peaks;

	/// <summary>
	/// Gets the noise and baselines.
	/// </summary>
	public NoiseAndBaseline[] NoiseAndBaselines
	{
		get
		{
			if (_noisePackets != null)
			{
				NoiseAndBaseline[] array = new NoiseAndBaseline[_noisePackets.Length];
				for (int i = 0; i < _noisePackets.Length; i++)
				{
					NoiseInfoPacketStruct noiseInfoPacketStruct = _noisePackets[i];
					array[i] = new NoiseAndBaseline
					{
						Noise = noiseInfoPacketStruct.Noise,
						Baseline = noiseInfoPacketStruct.Baseline,
						Mass = noiseInfoPacketStruct.Mass
					};
				}
				return array;
			}
			return _emptyNoiseArray;
		}
	}

	/// <summary>
	/// Gets the segment peaks.
	/// </summary>
	public virtual List<SegmentData> SegmentPeaks => SegmentPeakList;

	/// <summary>
	/// Gets the centroid counts.
	/// </summary>
	protected uint[] CentroidCounts { get; private set; }

	/// <summary>
	/// Gets the header.
	/// </summary>
	protected PacketHeaderStruct Header { get; private set; }

	/// <summary>
	/// Gets a value indicating whether to include ref peaks.
	/// </summary>
	protected bool IncludeRefPeaks { get; }

	private Lazy<byte[]> ProfileLazy { get; set; }

	private byte[] ProfileArray { get; set; }

	protected int ProfileOffset { get; set; }

	/// <summary>
	/// Gets the profile data.
	/// </summary>
	protected byte[] ProfileData
	{
		get
		{
			if (_profileIsLazy)
			{
				ProfileOffset = 0;
				return ProfileLazy.Value;
			}
			return ProfileArray;
		}
	}

	/// <summary>
	/// Gets the Debug blob.
	/// </summary>
	protected Lazy<byte[]> DebugBlob { get; private set; }

	/// <summary>
	/// Gets the extended scan data
	/// </summary>
	protected Lazy<IExtendedScanData> ExtendedDataBlob { get; private set; }

	/// <summary>
	/// Gets the reference peak array.
	/// </summary>
	protected LabelPeak[] ReferencePeakArray => _referencePeaks;

	/// <summary>
	/// Gets the segment peak list.
	/// </summary>
	protected List<SegmentData> SegmentPeakList { get; private set; }

	/// <summary>
	/// Gets a value indicating whether use ft profile sub segment.
	/// </summary>
	protected bool UseFtProfileSubSegment { get; private set; }

	/// <summary>
	/// Gets the default feature word.
	/// </summary>
	private uint DefaultFeatureWord => Header.DefaultFeatureWord;

	/// <summary>
	/// Gets a value indicating whether the data has accurate mass centroids.
	/// </summary>
	private bool HasAccurateMassCentroids { get; }

	/// <summary>
	/// Gets or sets a value indicating whether the data has peak widths.
	/// </summary>
	private bool HasWidths { get; set; }

	/// <summary>
	/// Gets the packet scan data features.
	/// </summary>
	private PacketFeatures PacketScanDataFeatures { get; }

	/// <summary>
	/// Gets the debug data for this scan.
	/// If there is no data, returns an empty array.
	/// </summary>
	public byte[] DebugData => DebugBlob?.Value ?? _emptyByteArray;

	public IExtendedScanData ExtendedData => ExtendedDataBlob?.Value ?? new EmptyExtendedScanData();

	public bool HasExtendedData { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT.AdvancedPacketBase" /> class.
	/// </summary>
	/// <param name="viewer">
	/// The viewer.
	/// </param>
	/// <param name="dataOffset">offset from start of the memory reader for this scan</param>
	/// <param name="fileRevision">Raw file version</param>
	/// <param name="includeRefPeaks">
	/// The include ref peaks.
	/// </param>
	/// <param name="packetScanDataFeatures">Defines what optional data should be decoded</param>
	/// <param name="expandLabels">If true (default) the centroid data is decoded to type LabelPeak.
	/// This may not be needed in some workflows, such as XIC, that only need simpler mass, intensity arrays</param>
	protected AdvancedPacketBase(IMemoryReader viewer, long dataOffset, int fileRevision, bool includeRefPeaks, PacketFeatures packetScanDataFeatures, bool expandLabels = true)
	{
		IncludeRefPeaks = includeRefPeaks;
		PacketScanDataFeatures = packetScanDataFeatures;
		Load(viewer, dataOffset, fileRevision);
		uint defaultFeatureWord = Header.DefaultFeatureWord;
		HasAccurateMassCentroids = (defaultFeatureWord & 0x40) == 0 && (defaultFeatureWord & 0x10000) != 0;
		UseFtProfileSubSegment = (defaultFeatureWord & 0x40) == 0 && (defaultFeatureWord & 0x80) != 0;
		if (expandLabels)
		{
			ExpandLabelData();
		}
	}

	/// <summary>
	/// Calculate scan size
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes used by this scan.
	/// </returns>
	public static long Size(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long num = dataOffset;
		uint[] array = viewer.ReadUnsignedInts(num, 8);
		PacketHeaderStruct packetHeaderStruct = new PacketHeaderStruct
		{
			NumSegments = array[0],
			NumProfileWords = array[1],
			NumCentroidWords = array[2],
			DefaultFeatureWord = array[3],
			NumNonDefaultFeatureWords = array[4],
			NumExpansionWords = array[5],
			NumNoiseInfoWords = array[6],
			NumDebugInfoWords = array[7]
		};
		num += PacketHeaderStructSize;
		num += 8 * packetHeaderStruct.NumSegments;
		uint numProfileWords = packetHeaderStruct.NumProfileWords;
		numProfileWords += packetHeaderStruct.NumCentroidWords;
		numProfileWords += packetHeaderStruct.NumNonDefaultFeatureWords;
		numProfileWords += packetHeaderStruct.NumExpansionWords;
		numProfileWords += packetHeaderStruct.NumNoiseInfoWords;
		numProfileWords += packetHeaderStruct.NumDebugInfoWords;
		num += (int)(numProfileWords * 4);
		return num - dataOffset;
	}

	/// <summary>
	/// Load (from file).
	/// </summary>
	/// <param name="viewer">
	/// The viewer (memory map into file).
	/// </param>
	/// <param name="dataOffset">
	/// The data offset (into the memory map).
	/// </param>
	/// <param name="fileRevision">
	/// The file revision.
	/// </param>
	/// <returns>
	/// The number of bytes read
	/// </returns>
	public long Load(IMemoryReader viewer, long dataOffset, int fileRevision)
	{
		long num = dataOffset;
		uint[] array = viewer.ReadUnsignedInts(num, 8);
		Header = new PacketHeaderStruct
		{
			NumSegments = array[0],
			NumProfileWords = array[1],
			NumCentroidWords = array[2],
			DefaultFeatureWord = array[3],
			NumNonDefaultFeatureWords = array[4],
			NumExpansionWords = array[5],
			NumNoiseInfoWords = array[6],
			NumDebugInfoWords = array[7]
		};
		num += PacketHeaderStructSize;
		SegmentPeakList = new List<SegmentData>((int)Header.NumSegments);
		for (int i = 0; i < Header.NumSegments; i++)
		{
			float num2 = viewer.ReadFloat(num);
			float num3 = viewer.ReadFloat(num + 4);
			num += 8;
			SegmentPeakList.Add(new SegmentData
			{
				MassRange = new MassRangeStruct(num2, num3),
				DataPeaks = new List<DataPeak>(0)
			});
		}
		int totalBlobSize = (int)(Header.NumProfileWords * 4);
		long offset = num;
		if ((PacketScanDataFeatures & PacketFeatures.Profile) != PacketFeatures.None)
		{
			if (viewer is MemoryArrayReader memoryArrayReader)
			{
				ProfileOffset = (int)(offset - memoryArrayReader.InitialOffset);
				_profileIsLazy = false;
				ProfileArray = memoryArrayReader.Data;
			}
			else
			{
				ProfileLazy = new Lazy<byte[]>(() => viewer.ReadLargeData(offset, totalBlobSize));
				_profileIsLazy = true;
				ProfileOffset = 0;
			}
		}
		num += totalBlobSize;
		int numCentroidWords = (int)Header.NumCentroidWords;
		if (viewer is MemoryArrayReader memoryArrayReader2)
		{
			_centroidBlob = memoryArrayReader2.Data;
			_centroidBlobOffset = (int)(num - memoryArrayReader2.InitialOffset);
			num += numCentroidWords * 4;
		}
		else
		{
			_centroidBlob = viewer.ReadLargeData(ref num, numCentroidWords * 4);
			_centroidBlobOffset = 0;
		}
		_nonDefaultFeatures = viewer.ReadUnsignedIntsExt(ref num, (int)Header.NumNonDefaultFeatureWords);
		if (Header.NumExpansionWords != 0 && viewer.ReadIntExt(ref num) > 0)
		{
			HasWidths = true;
			int num4 = (int)(Header.NumExpansionWords - 1);
			if ((PacketScanDataFeatures & PacketFeatures.Resolution) != PacketFeatures.None)
			{
				_widths = viewer.ReadFloatsExt(ref num, num4);
			}
			else
			{
				HasWidths = false;
				num += num4 * 4;
			}
		}
		int totalNoiseSize;
		int numberOfPackets;
		if (Header.NumNoiseInfoWords != 0)
		{
			totalNoiseSize = (int)(Header.NumNoiseInfoWords * 4);
			numberOfPackets = totalNoiseSize / NoiseInfoStructSize;
			if ((PacketScanDataFeatures & PacketFeatures.NoiseAndBaseline) != PacketFeatures.None)
			{
				if (!viewer.PreferLargeReads)
				{
					GetNoisePacketsData(viewer, num);
				}
				else
				{
					Utilities.LoadDataFromInternalMemoryArrayReader(GetNoisePacketsData, viewer, num, 1048576);
				}
			}
			num += totalNoiseSize;
		}
		if (Header.NumDebugInfoWords != 0)
		{
			int debugBlobSize = (int)(Header.NumDebugInfoWords * 4);
			HasExtendedData = debugBlobSize > 0;
			long debugOffset = num;
			DebugBlob = new Lazy<byte[]>(() => viewer.ReadLargeData(debugOffset, debugBlobSize));
			ExtendedDataBlob = new Lazy<IExtendedScanData>(() => new ExtendedScanDataClass(viewer, debugOffset, Header.NumDebugInfoWords));
			num += debugBlobSize;
		}
		return num - dataOffset;
		long GetNoisePacketsData(IMemoryReader reader, long pos)
		{
			_noisePackets = reader.ReadSimpleStructureArray<NoiseInfoPacketStruct>(pos, numberOfPackets);
			_labelStreamData.SetNoiseInfoPackets(_noisePackets);
			return totalNoiseSize;
		}
	}

	/// <summary>
	/// Compresses the centroid data into packet buffer.
	/// For LT certroid format, there may not be a "full" centroid data. For any other format:
	/// The assumption of this routine is that the IMsInstrumentData that is passed in does contain
	/// centroid data. The profile information (if any) will be dropped from the writing
	/// in this routine.
	/// </summary>
	/// <param name="instData">The mass spec instrument data.</param>
	/// <returns>The compressed packet in byte array. </returns>
	internal static byte[] CompressCentroids(IMsInstrumentData instData)
	{
		ISegmentedScanAccess scanData = instData.ScanData;
		uint segmentCount = (uint)scanData.SegmentCount;
		ReadOnlyCollection<IRangeAccess> massRanges = ((segmentCount != 0) ? scanData.MassRanges : new ReadOnlyCollection<IRangeAccess>(new List<IRangeAccess>(0)));
		NoiseAndBaseline[] noiseData = instData.NoiseData;
		int numNoisePackets = ((noiseData != null) ? noiseData.Length : 0);
		int num = ((segmentCount != 0) ? scanData.SegmentLengths.Sum() : 0);
		CentroidStream centroidData = instData.CentroidData;
		ReadOnlyCollection<int> centroidSegmentsCounter = ((segmentCount != 0) ? scanData.SegmentLengths : new ReadOnlyCollection<int>(new List<int>(0)));
		SimpleScan centroidPeaks = ((num > 0) ? new SimpleScan
		{
			Intensities = scanData.Intensities,
			Masses = scanData.Positions
		} : new SimpleScan
		{
			Intensities = _emptyDoubletArray,
			Masses = _emptyDoubletArray
		});
		ThermoFisher.CommonCore.Data.Business.LabelPeak[] array = (ThermoFisher.CommonCore.Data.Business.LabelPeak[])(((centroidData == null) ? ((object)Array.Empty<ThermoFisher.CommonCore.Data.Business.LabelPeak>()) : ((object)centroidData.GetLabelPeaks())) ?? Array.Empty<ThermoFisher.CommonCore.Data.Business.LabelPeak>());
		int numLabelPeaks = array.Length;
		bool hasWidths = array.IsAny() && array[0].Resolution > 0.0;
		uint[] features;
		float[] widths;
		bool num2 = ExtractLabelsInfo(array, hasWidths, out features, out widths);
		uint num3 = 65536u;
		if (num2)
		{
			num3 |= 0x100;
		}
		PacketHeaderStruct packetHeaderInfo = CreatePacketHeader(segmentCount, numLabelPeaks, num, numNoisePackets, 0, 0, hasWidths, num3, 0, instData.ExtendedData);
		byte[] array2 = CreatePacketBuffer(segmentCount, packetHeaderInfo);
		int num4 = 0;
		num4 += CopyPacketHeaderToPacketBuffer(packetHeaderInfo, array2, num4);
		num4 += CopyMassRangesToPacketBuffer(massRanges, array2, num4);
		num4 += CopyCentroidToPacketBuffer(centroidSegmentsCounter, segmentCount, centroidPeaks, array2, num4);
		num4 += CopyLabelsToPacketBuffer(numLabelPeaks, array2, num4, features, hasWidths, widths);
		num4 += CopyNoiseInfoToPacketBuffer(numNoisePackets, noiseData, array2, num4);
		CopyExtensions(instData.ExtendedData, array2, num4);
		return array2;
	}

	/// <summary>
	/// Calculates the labels per segment.
	/// </summary>
	/// <param name="numSegments">The number segments.</param>
	/// <param name="labelPeaks">The label peaks.</param>
	/// <param name="massRanges">The mass ranges.</param>
	/// <param name="centroidSegmentsCounter">The centroid segments counter.</param>
	protected static void CalculateLabelsPerSegment(uint numSegments, IReadOnlyList<ThermoFisher.CommonCore.Data.Business.LabelPeak> labelPeaks, IReadOnlyList<IRangeAccess> massRanges, int[] centroidSegmentsCounter)
	{
		int count = labelPeaks.Count;
		if (count <= 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		while (num2 < count && num < numSegments)
		{
			if (labelPeaks[num2].Mass <= massRanges[num].High)
			{
				centroidSegmentsCounter[num]++;
				num2++;
			}
			else
			{
				num++;
			}
		}
		if (num2 < count)
		{
			centroidSegmentsCounter[numSegments - 1] += count - num2;
		}
	}

	/// <summary>
	/// Copies the centroid to packet buffer.
	/// </summary>
	/// <param name="centroidSegmentsCounter">The centroid segments counter.</param>
	/// <param name="numSegments">The number segments.</param>
	/// <param name="centroidPeaks">The centroid peaks.</param>
	/// <param name="bytes">The bytes.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <returns>The number of bytes copied to the packet buffer.</returns>
	protected static int CopyCentroidToPacketBuffer(ReadOnlyCollection<int> centroidSegmentsCounter, uint numSegments, SimpleScan centroidPeaks, byte[] bytes, int dataOffset)
	{
		int num = dataOffset;
		if (centroidSegmentsCounter.Count > 0)
		{
			int num2 = 0;
			for (int i = 0; i < numSegments; i++)
			{
				int num3 = centroidSegmentsCounter[i];
				double[] intensities = centroidPeaks.Intensities;
				double[] masses = centroidPeaks.Masses;
				Buffer.BlockCopy(BitConverter.GetBytes(num3), 0, bytes, num, 4);
				num += 4;
				int num4 = 0;
				while (num4 < num3)
				{
					Buffer.BlockCopy(BitConverter.GetBytes(masses[num2]), 0, bytes, num, 8);
					num += 8;
					Buffer.BlockCopy(BitConverter.GetBytes((float)intensities[num2]), 0, bytes, num, 4);
					num += 4;
					num4++;
					num2++;
				}
			}
		}
		return num - dataOffset;
	}

	/// <summary>
	/// Copies the labels to packet buffer.
	/// </summary>
	/// <param name="numLabelPeaks">The number label peaks.</param>
	/// <param name="bytes">The bytes.</param>
	/// <param name="dataOffset">The data offset.</param>
	/// <param name="features">The features.</param>
	/// <param name="hasWidths">if set to <c>true</c> [has widths].</param>
	/// <param name="widths">The widths.</param>
	/// <returns>The number of bytes copied to the packet buffer.</returns>
	protected static int CopyLabelsToPacketBuffer(int numLabelPeaks, byte[] bytes, int dataOffset, uint[] features, bool hasWidths, float[] widths)
	{
		int num = dataOffset;
		if (numLabelPeaks > 0)
		{
			Buffer.BlockCopy(features, 0, bytes, num, numLabelPeaks * 4);
			num += numLabelPeaks * 4;
			if (hasWidths)
			{
				Buffer.BlockCopy(ExpansionFullWidthHalfHeight, 0, bytes, num, 4);
				num += 4;
				int num2 = numLabelPeaks * 4;
				Buffer.BlockCopy(widths, 0, bytes, num, num2);
				num += num2;
			}
		}
		return num - dataOffset;
	}

	/// <summary>
	/// Copies the mass ranges to packet buffer.
	/// </summary>
	/// <param name="massRanges">The mass ranges.</param>
	/// <param name="bytes">The bytes.</param>
	/// <param name="dataOffset">The start position.</param>
	/// <returns>The number of bytes copied to the packet buffer.</returns>
	protected static int CopyMassRangesToPacketBuffer(ReadOnlyCollection<IRangeAccess> massRanges, byte[] bytes, int dataOffset)
	{
		int num = dataOffset;
		float[] array = new float[2];
		foreach (IRangeAccess massRange in massRanges)
		{
			array[0] = (float)massRange.Low;
			array[1] = (float)massRange.High;
			Buffer.BlockCopy(array, 0, bytes, num, 8);
			num += 8;
		}
		return num - dataOffset;
	}

	protected static int CopyExtensions(IExtendedScanData extensions, byte[] bytes, int dataOffset)
	{
		int num = dataOffset;
		if (extensions != null)
		{
			ReadOnlyCollection<IDataSegment> dataSegments = extensions.DataSegments;
			ReadOnlyCollection<ITransientSegment> transients = extensions.Transients;
			long header = extensions.Header;
			if (dataSegments.Count > 0 || transients.Count > 0)
			{
				Buffer.BlockCopy(BitConverter.GetBytes(header), 0, bytes, num, 4);
				num += 4;
				foreach (ITransientSegment item in transients)
				{
					Buffer.BlockCopy(BitConverter.GetBytes(item.Header), 0, bytes, num, 4);
					num += 4;
					int[] data = item.Data;
					Buffer.BlockCopy(BitConverter.GetBytes(data.Length / 4), 0, bytes, num, 4);
					num += 4;
					Buffer.BlockCopy(data, 0, bytes, num, data.Length);
					num += data.Length;
				}
				foreach (IDataSegment item2 in dataSegments)
				{
					Buffer.BlockCopy(BitConverter.GetBytes(item2.Header), 0, bytes, num, 4);
					num += 4;
					byte[] bytes2 = item2.Bytes;
					Buffer.BlockCopy(BitConverter.GetBytes(bytes2.Length / 4), 0, bytes, num, 4);
					num += 4;
					Buffer.BlockCopy(bytes2, 0, bytes, num, bytes2.Length);
					num += bytes2.Length;
				}
			}
			return num - dataOffset;
		}
		return 0;
	}

	/// <summary>
	/// Copies the noise information to packet buffer.
	/// </summary>
	/// <param name="numNoisePackets">The number noise packets.</param>
	/// <param name="noiseData">The noise data.</param>
	/// <param name="bytes">The packet buffer.</param>
	/// <param name="dataOffset">The start position.</param>
	/// <returns>The number of bytes copied to the packet buffer</returns>
	protected static int CopyNoiseInfoToPacketBuffer(int numNoisePackets, NoiseAndBaseline[] noiseData, byte[] bytes, int dataOffset)
	{
		int num = dataOffset;
		if (numNoisePackets > 0)
		{
			int num2 = numNoisePackets * 12;
			float[] array = new float[num2];
			int num3 = 0;
			for (int i = 0; i < numNoisePackets; i++)
			{
				NoiseAndBaseline noiseAndBaseline = noiseData[i];
				array[num3++] = noiseAndBaseline.Mass;
				array[num3++] = noiseAndBaseline.Noise;
				array[num3++] = noiseAndBaseline.Baseline;
			}
			Buffer.BlockCopy(array, 0, bytes, num, num2);
			num += num2;
		}
		return num - dataOffset;
	}

	/// <summary>
	/// Copies the packet header to packet buffer.
	/// </summary>
	/// <param name="packetHeaderInfo">The packet header information.</param>
	/// <param name="bytes">The bytes.</param>
	/// <param name="startPos">The start position.</param>
	/// <returns>The number of bytes copied to the packet buffer.</returns>
	protected static int CopyPacketHeaderToPacketBuffer(PacketHeaderStruct packetHeaderInfo, byte[] bytes, int startPos)
	{
		Buffer.BlockCopy(WriterHelper.StructToByteArray(packetHeaderInfo, PacketHeaderStructSize), 0, bytes, startPos, PacketHeaderStructSize);
		return PacketHeaderStructSize;
	}

	/// <summary>
	/// Creates an empty segmented scan object.
	/// </summary>
	/// <returns>An empty segmented scan with one profile point with zero values.</returns>
	protected static ISegmentedScanAccess CreateAnEmptySegmentedScan()
	{
		SegmentedScan segmentedScan = new SegmentedScan();
		segmentedScan.Intensities = new double[1];
		segmentedScan.Positions = new double[1];
		segmentedScan.Flags = new PeakOptions[1];
		segmentedScan.PositionCount = 1;
		segmentedScan.SegmentCount = 1;
		segmentedScan.SegmentSizes = new int[1];
		segmentedScan.Ranges = new ThermoFisher.CommonCore.Data.Business.Range[1]
		{
			new ThermoFisher.CommonCore.Data.Business.Range(0.0, 0.0)
		};
		return segmentedScan;
	}

	/// <summary>
	/// Creates the packet buffer.
	/// </summary>
	/// <param name="numSegments">The number segments.</param>
	/// <param name="packetHeaderInfo">The packet header information.</param>
	/// <returns>A byte array for storing packet data.</returns>
	protected static byte[] CreatePacketBuffer(uint numSegments, PacketHeaderStruct packetHeaderInfo)
	{
		return new byte[PacketHeaderStructSize + numSegments * 8 + (packetHeaderInfo.NumProfileWords + packetHeaderInfo.NumCentroidWords + packetHeaderInfo.NumNonDefaultFeatureWords + packetHeaderInfo.NumExpansionWords + packetHeaderInfo.NumNoiseInfoWords + packetHeaderInfo.NumDebugInfoWords) * 4];
	}

	/// <summary>
	/// Initializes the packet header.
	/// </summary>
	/// <param name="numSegments">The number segments.</param>
	/// <param name="numLabelPeaks">The number label peaks.</param>
	/// <param name="totalCentroids">The number centroid data.</param>
	/// <param name="numNoisePackets">The number noise packets.</param>
	/// <param name="totalProfilePoints">The total profile points.</param>
	/// <param name="totalSubSegments">The total sub segments.</param>
	/// <param name="hasWidths">if set to <c>true</c> [has widths].</param>
	/// <param name="defaultFeatureWord">Default feature word.</param>
	/// <param name="sizeOfProfileSubSegmentStruct">Size of profile sub-segment struct - FT Profile is 12 and LT Profile is 8</param>
	/// <param name="extensions">the (optional) extension blocks</param>
	/// <returns>The packet header struct object.</returns>
	protected static PacketHeaderStruct CreatePacketHeader(uint numSegments, int numLabelPeaks, int totalCentroids, int numNoisePackets, int totalProfilePoints, int totalSubSegments, bool hasWidths, uint defaultFeatureWord, int sizeOfProfileSubSegmentStruct, IExtendedScanData extensions = null)
	{
		int num = 3;
		PacketHeaderStruct result = new PacketHeaderStruct
		{
			NumSegments = numSegments,
			NumDebugInfoWords = 0u,
			DefaultFeatureWord = defaultFeatureWord,
			NumNonDefaultFeatureWords = (uint)numLabelPeaks,
			NumNoiseInfoWords = (uint)(numNoisePackets * num)
		};
		if (totalProfilePoints > 0)
		{
			int num2 = 4;
			result.NumProfileWords = (uint)(numSegments * Utilities.StructSizeLookup.Value[22]);
			result.NumProfileWords += (uint)(totalSubSegments * sizeOfProfileSubSegmentStruct);
			result.NumProfileWords += (uint)(totalProfilePoints * num2);
			result.NumProfileWords /= 4u;
		}
		if (totalCentroids > 0 || numSegments != 0)
		{
			result.NumCentroidWords = numSegments * 4;
			result.NumCentroidWords += (uint)(totalCentroids * 12);
			result.NumCentroidWords /= 4u;
		}
		if (numLabelPeaks > 0 && hasWidths)
		{
			result.NumExpansionWords = 4u;
			result.NumExpansionWords += (uint)(numLabelPeaks * 4);
			result.NumExpansionWords /= 4u;
		}
		if (extensions != null)
		{
			ReadOnlyCollection<IDataSegment> dataSegments = extensions.DataSegments;
			ReadOnlyCollection<ITransientSegment> transients = extensions.Transients;
			long num3 = 0L;
			if (dataSegments.Count > 0 || transients.Count > 0)
			{
				num3 = 4L;
				foreach (ITransientSegment item in transients)
				{
					num3 += 4;
					num3 += 4;
					num3 += item.Data.Length;
				}
				foreach (IDataSegment item2 in dataSegments)
				{
					num3 += 4;
					num3 += 4;
					num3 += item2.Bytes.Length;
				}
			}
			result.NumDebugInfoWords = (uint)(num3 / 4);
		}
		return result;
	}

	/// <summary>
	/// Extracts the labels information.
	/// </summary>
	/// <param name="labelPeaks">The label peaks.</param>
	/// <param name="hasWidths">if set to <c>true</c> [has widths].</param>
	/// <param name="features">The features.</param>
	/// <param name="widths">The widths.</param>
	/// <returns>true if this is detected as MRTOF data (has saturated flags)</returns>
	protected static bool ExtractLabelsInfo(ThermoFisher.CommonCore.Data.Business.LabelPeak[] labelPeaks, bool hasWidths, out uint[] features, out float[] widths)
	{
		bool result = false;
		if (!labelPeaks.IsAny())
		{
			features = _emptytUintArray;
			widths = _emptytFloatArray;
			return result;
		}
		int num = labelPeaks.Length;
		features = new uint[num];
		widths = new float[num];
		for (int i = 0; i < num; i++)
		{
			ThermoFisher.CommonCore.Data.Business.LabelPeak labelPeak = labelPeaks[i];
			if (hasWidths)
			{
				widths[i] = (float)labelPeak.Resolution;
			}
			features[i] = ((uint)labelPeak.Charge & 0xFF) << 24;
			if ((labelPeak.Flag & PeakOptions.Fragmented) > PeakOptions.None)
			{
				features[i] |= 8388608u;
			}
			if ((labelPeak.Flag & PeakOptions.Merged) > PeakOptions.None)
			{
				features[i] |= 4194304u;
			}
			if ((labelPeak.Flag & PeakOptions.Saturated) > PeakOptions.None)
			{
				features[i] |= 524288u;
				result = true;
			}
			if ((labelPeak.Flag & PeakOptions.Reference) > PeakOptions.None)
			{
				features[i] |= 2097152u;
			}
			if ((labelPeak.Flag & PeakOptions.Exception) > PeakOptions.None)
			{
				features[i] |= 1048576u;
			}
			if ((labelPeak.Flag & PeakOptions.Modified) > PeakOptions.None)
			{
				features[i] |= 524288u;
			}
			features[i] |= (uint)(i & 0x3FFFF);
		}
		return result;
	}

	/// <summary>
	/// Gets the coefficient values.
	/// </summary>
	/// <param name="scanEvent">The scan event.</param>
	/// <param name="coeff0">The coefficient value 0.</param>
	/// <param name="coeff1">The coefficient value 1.</param>
	/// <param name="coeff2">The coefficient value 2.</param>
	/// <param name="coeff3">The coefficient value 3.</param>
	protected static void GetCoeffValues(IScanEvent scanEvent, ref double coeff0, ref double coeff1, ref double coeff2, ref double coeff3)
	{
		int massCalibratorCount = scanEvent.MassCalibratorCount;
		if (massCalibratorCount >= 4)
		{
			coeff0 = scanEvent.GetMassCalibrator(1);
			coeff1 = scanEvent.GetMassCalibrator(2);
			coeff2 = scanEvent.GetMassCalibrator(3);
			coeff3 = ((massCalibratorCount >= 5) ? scanEvent.GetMassCalibrator(4) : 0.0);
		}
	}

	/// <summary>
	/// Gets the label peaks.
	/// </summary>
	/// <param name="centroidData">The centroid data.</param>
	/// <param name="hasWidths">if set to <c>true</c> [has widths].</param>
	/// <returns>The label peaks.</returns>
	protected static ThermoFisher.CommonCore.Data.Business.LabelPeak[] GetLabelPeaks(CentroidStream centroidData, out bool hasWidths)
	{
		ThermoFisher.CommonCore.Data.Business.LabelPeak[] array = (ThermoFisher.CommonCore.Data.Business.LabelPeak[])(((centroidData == null) ? ((object)Array.Empty<ThermoFisher.CommonCore.Data.Business.LabelPeak>()) : ((object)centroidData.GetLabelPeaks())) ?? Array.Empty<ThermoFisher.CommonCore.Data.Business.LabelPeak>());
		hasWidths = array.IsAny() && array[0].Resolution > 0.0;
		return array;
	}

	/// <summary>
	/// The method reads the centroid "blob" and transforms them to <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.DataPeak" /> objects.
	/// </summary>
	protected void ExpandCentroidData()
	{
		if (Header.NumCentroidWords == 0)
		{
			return;
		}
		if (_hasContinueProcessingLabelData)
		{
			LabelPeak[] peaks = _labelStreamData.Peaks;
			int num = peaks.Length;
			List<DataPeak> list = new List<DataPeak>(num);
			for (int i = 0; i < num; i++)
			{
				list.Add(new DataPeak(peaks[i]));
			}
			SegmentPeakList[0].DataPeaks = list;
		}
		else
		{
			if (Header.NumCentroidWords == 0)
			{
				return;
			}
			int centroidBlobOffset = _centroidBlobOffset;
			uint numSegments = Header.NumSegments;
			uint defaultFeatureWord = Header.DefaultFeatureWord;
			int num2 = (HasAccurateMassCentroids ? 8 : 4);
			int num3 = num2 + 4;
			int[] array = new int[numSegments];
			CentroidCounts = new uint[numSegments];
			uint num4 = 0u;
			int num5 = centroidBlobOffset;
			for (int j = 0; j < numSegments; j++)
			{
				uint num6 = BitConverter.ToUInt32(_centroidBlob, num5);
				CentroidCounts[j] = num6;
				num5 = (array[j] = num5 + 4) + num3 * (int)num6;
				num4 += num6;
			}
			DataPeak[] array2 = new DataPeak[num4];
			int num7 = 0;
			byte[] centroidBlob = _centroidBlob;
			PeakOptions peakOptions = CreateDefaultFlagSet(defaultFeatureWord);
			if (peakOptions == PeakOptions.None)
			{
				for (int k = 0; k < numSegments; k++)
				{
					centroidBlobOffset = array[k];
					uint num8 = CentroidCounts[k];
					if (HasAccurateMassCentroids)
					{
						for (int l = 0; l < num8; l++)
						{
							array2[num7++] = new DataPeak(BitConverter.ToDouble(centroidBlob, centroidBlobOffset), BitConverter.ToSingle(centroidBlob, centroidBlobOffset + 8));
							centroidBlobOffset += num3;
						}
					}
					else
					{
						for (int m = 0; m < num8; m++)
						{
							array2[num7++] = new DataPeak(BitConverter.ToSingle(centroidBlob, centroidBlobOffset), BitConverter.ToSingle(centroidBlob, centroidBlobOffset + 4));
							centroidBlobOffset += num3;
						}
					}
				}
			}
			else
			{
				for (int n = 0; n < numSegments; n++)
				{
					centroidBlobOffset = array[n];
					uint num9 = CentroidCounts[n];
					for (int num10 = 0; num10 < num9; num10++)
					{
						double mass = (HasAccurateMassCentroids ? BitConverter.ToDouble(centroidBlob, centroidBlobOffset) : ((double)BitConverter.ToSingle(centroidBlob, centroidBlobOffset)));
						float num11 = BitConverter.ToSingle(_centroidBlob, centroidBlobOffset + num2);
						DataPeak dataPeak = new DataPeak(mass, num11);
						dataPeak.Options = peakOptions;
						array2[num7++] = dataPeak;
						centroidBlobOffset += num3;
					}
				}
			}
			uint numNonDefaultFeatureWords = Header.NumNonDefaultFeatureWords;
			if (numNonDefaultFeatureWords != 0)
			{
				for (int num12 = 0; num12 < numNonDefaultFeatureWords; num12++)
				{
					int num13 = (int)(_nonDefaultFeatures[num12] & 0x3FFFF);
					DataPeak dataPeak2 = array2[num13];
					dataPeak2.Options = peakOptions;
					if (!IncludeRefPeaks && dataPeak2.IsReferenceOrException)
					{
						dataPeak2.Intensity = 0.0;
					}
					array2[num13] = dataPeak2;
				}
			}
			int num14 = 0;
			for (int num15 = 0; num15 < numSegments; num15++)
			{
				uint num16 = CentroidCounts[num15];
				List<DataPeak> list2 = new List<DataPeak>((int)num16);
				for (int num17 = 0; num17 < num16; num17++)
				{
					list2.Add(array2[num14++]);
				}
				SegmentPeakList[num15].DataPeaks = list2;
			}
		}
	}

	/// <summary>
	/// The method reads the centroid "blob" and transforms them to <see cref="T:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.DataPeak" /> objects.
	/// </summary>
	protected (double[], double[]) ExpandSimplifiedCentroidData()
	{
		double[] array = Array.Empty<double>();
		double[] array2 = Array.Empty<double>();
		if (Header.NumCentroidWords != 0)
		{
			if (_hasContinueProcessingLabelData)
			{
				LabelPeak[] peaks = _labelStreamData.Peaks;
				int num = peaks.Length;
				array = new double[num];
				array2 = new double[num];
				for (int i = 0; i < num; i++)
				{
					LabelPeak labelPeak = peaks[i];
					array[i] = labelPeak.Mass;
					array2[i] = labelPeak.Intensity;
				}
			}
			else if (Header.NumCentroidWords != 0)
			{
				int centroidBlobOffset = _centroidBlobOffset;
				uint numSegments = Header.NumSegments;
				uint defaultFeatureWord = Header.DefaultFeatureWord;
				int num2 = (HasAccurateMassCentroids ? 8 : 4) + 4;
				Span<int> span = stackalloc int[(int)numSegments];
				Span<uint> span2 = stackalloc uint[(int)numSegments];
				uint num3 = 0u;
				int num4 = centroidBlobOffset;
				for (int j = 0; j < numSegments; j++)
				{
					uint num5 = BitConverter.ToUInt32(_centroidBlob, num4);
					span2[j] = num5;
					num4 += 4;
					span[j] = num4;
					num4 += num2 * (int)num5;
					num3 += num5;
				}
				array = new double[num3];
				array2 = new double[num3];
				int num6 = 0;
				byte[] centroidBlob = _centroidBlob;
				PeakOptions peakOptions = CreateDefaultFlagSet(defaultFeatureWord);
				for (int k = 0; k < numSegments; k++)
				{
					centroidBlobOffset = span[k];
					uint num7 = span2[k];
					if (HasAccurateMassCentroids)
					{
						for (int l = 0; l < num7; l++)
						{
							array[num6] = BitConverter.ToDouble(centroidBlob, centroidBlobOffset);
							array2[num6++] = BitConverter.ToSingle(centroidBlob, centroidBlobOffset + 8);
							centroidBlobOffset += num2;
						}
					}
					else
					{
						for (int m = 0; m < num7; m++)
						{
							array[num6] = BitConverter.ToSingle(centroidBlob, centroidBlobOffset);
							array2[num6++] = BitConverter.ToSingle(centroidBlob, centroidBlobOffset + 4);
							centroidBlobOffset += num2;
						}
					}
				}
				uint numNonDefaultFeatureWords = Header.NumNonDefaultFeatureWords;
				if (numNonDefaultFeatureWords != 0)
				{
					for (int n = 0; n < numNonDefaultFeatureWords; n++)
					{
						int num8 = (int)(_nonDefaultFeatures[n] & 0x3FFFF);
						if (!IncludeRefPeaks && (peakOptions & (PeakOptions.Exception | PeakOptions.Reference)) != PeakOptions.None)
						{
							array2[num8] = 0.0;
						}
					}
				}
			}
		}
		return (array, array2);
	}

	/// <summary>
	/// create default flag set, from the packed bits.
	/// </summary>
	/// <param name="defaultFlags">
	/// The default flags (bit fields).
	/// </param>
	/// <returns>
	/// A DataPeak which has been initialized with the default peak flags
	/// </returns>
	private static PeakOptions CreateDefaultFlagSet(uint defaultFlags)
	{
		uint num = (defaultFlags & 0xF80000) >> 19;
		return OptionsLookup[num];
	}

	/// <summary>
	/// apply flags to peak.
	/// </summary>
	/// <param name="feature">
	/// The feature.
	/// </param>
	/// <param name="limit">
	/// The limit.
	/// </param>
	/// <param name="centroidPeaks">
	/// The centroid peaks.
	/// </param>
	/// <param name="refsFound">
	/// The refs found so far.
	/// </param>
	/// <param name="isRefPeak">
	/// The is ref peak.
	/// </param>
	/// <returns>
	/// The (updated) number of reference peaks found
	/// </returns>
	private int ApplyFlags(uint feature, int limit, LabelPeak[] centroidPeaks, int refsFound, bool[] isRefPeak)
	{
		int num;
		if ((num = (int)(feature & 0x3FFFF)) < limit)
		{
			byte flags = centroidPeaks[num].Flags;
			if (SetFlags(feature, ref flags))
			{
				refsFound++;
				isRefPeak[num] = true;
			}
			centroidPeaks[num].Flags = flags;
		}
		return refsFound;
	}

	/// <summary>
	/// The method checks the flags in the default
	/// feature word to determine if we should
	/// continue processing label data.
	/// </summary>
	/// <returns>
	/// True to continue processing.
	/// </returns>
	private bool ContinueProcessingLabelData()
	{
		uint defaultFeatureWord = Header.DefaultFeatureWord;
		_isExpandChargeLabels = true;
		_isExpandReferenceLabels = true;
		_isExpandMergedLabels = true;
		_isExpandFragmentedLabels = true;
		_isExpandExceptionLabels = true;
		_isExpandModifiedLabels = true;
		if ((defaultFeatureWord & 0x20000) == 0)
		{
			if (Header.NumProfileWords == 0 && !HasWidths && defaultFeatureWord == 0 && Header.NumNonDefaultFeatureWords == 0)
			{
				return false;
			}
		}
		else
		{
			if (Header.NumProfileWords == 0 && !HasWidths && (defaultFeatureWord & 0x3FFFF) == 262143)
			{
				return false;
			}
			if ((defaultFeatureWord & 1) != 0)
			{
				_isExpandChargeLabels = false;
			}
			if ((defaultFeatureWord & 2) != 0)
			{
				_isExpandReferenceLabels = false;
			}
			if ((defaultFeatureWord & 4) != 0)
			{
				_isExpandMergedLabels = false;
			}
			if ((defaultFeatureWord & 8) != 0)
			{
				_isExpandFragmentedLabels = false;
			}
			if ((defaultFeatureWord & 0x10) != 0)
			{
				_isExpandExceptionLabels = false;
			}
			if ((defaultFeatureWord & 0x20) != 0)
			{
				_isExpandModifiedLabels = false;
			}
		}
		return true;
	}

	/// <summary>
	/// The method converts a set of centroid structures to label peaks.
	/// </summary>
	/// <param name="centroidCount">
	/// The centroid structures count.
	/// </param>
	/// <param name="blobIndex">
	/// The blob index.
	/// </param>
	/// <param name="labelPeaks">
	/// The label peaks.
	/// </param>
	/// <param name="startIndex">index for this segment in returned data</param>
	/// <param name="defaultChargeState">
	/// The default charge state.
	/// </param>
	/// <param name="defaultFlags">
	/// The default flags.
	/// </param>
	/// <param name="resolutionIndex">
	/// The resolution index.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	private int ConvertCentroidsToLabelPeaks(uint centroidCount, int blobIndex, LabelPeak[] labelPeaks, int startIndex, byte defaultChargeState, byte defaultFlags, ref int resolutionIndex)
	{
		bool hasWidths = HasWidths;
		long num = centroidCount + startIndex;
		long num2 = num - 3;
		byte[] centroidBlob = _centroidBlob;
		float[] array = new float[num * 2];
		int num3 = 0;
		int num4 = (int)num * 8;
		Buffer.BlockCopy(centroidBlob, blobIndex, array, 0, num4);
		if (hasWidths)
		{
			float[] widths = _widths;
			int num5 = resolutionIndex;
			int i = startIndex;
			if (defaultChargeState == 0 && defaultFlags == 0)
			{
				for (; i < num2; i++)
				{
					labelPeaks[i++] = new LabelPeak
					{
						Mass = array[num3],
						Intensity = array[num3 + 1],
						Resolution = widths[num5]
					};
					labelPeaks[i++] = new LabelPeak
					{
						Mass = array[num3 + 2],
						Intensity = array[num3 + 3],
						Resolution = widths[num5 + 1]
					};
					labelPeaks[i++] = new LabelPeak
					{
						Mass = array[num3 + 4],
						Intensity = array[num3 + 5],
						Resolution = widths[num5 + 2]
					};
					labelPeaks[i] = new LabelPeak
					{
						Mass = array[num3 + 6],
						Intensity = array[num3 + 7],
						Resolution = widths[num5 + 3]
					};
					num5 += 4;
					num3 += 8;
				}
			}
			for (; i < num; i++)
			{
				labelPeaks[i] = new LabelPeak
				{
					Mass = array[num3++],
					Intensity = array[num3++],
					Resolution = widths[num5++],
					Charge = defaultChargeState,
					Flags = defaultFlags
				};
			}
			resolutionIndex = num5;
		}
		else
		{
			int j = startIndex;
			if (defaultChargeState == 0 && defaultFlags == 0)
			{
				while (j < num2)
				{
					labelPeaks[j].Mass = array[num3];
					labelPeaks[j++].Intensity = array[num3 + 1];
					labelPeaks[j].Mass = array[num3 + 2];
					labelPeaks[j++].Intensity = array[num3 + 3];
					labelPeaks[j].Mass = array[num3 + 4];
					labelPeaks[j++].Intensity = array[num3 + 5];
					labelPeaks[j].Mass = array[num3 + 6];
					labelPeaks[j++].Intensity = array[num3 + 7];
					num3 += 8;
				}
			}
			for (; j < num; j++)
			{
				labelPeaks[j] = new LabelPeak
				{
					Mass = array[num3++],
					Intensity = array[num3++],
					Charge = defaultChargeState,
					Flags = defaultFlags
				};
			}
		}
		blobIndex += num4;
		return blobIndex;
	}

	/// <summary>
	/// The method converts a set of centroid structures to label peaks.
	/// </summary>
	/// <param name="centroidCount">
	/// The centroid structures count.
	/// </param>
	/// <param name="blobIndex">
	/// The blob index.
	/// </param>
	/// <param name="labelPeaks">
	/// The label peaks.
	/// </param>
	/// <param name="startIndex">index for this segment in returned data</param>
	/// <param name="defaultChargeState">
	/// The default charge state.
	/// </param>
	/// <param name="defaultFlags">
	/// The default flags.
	/// </param>
	/// <param name="resolutionIndex">
	/// The resolution index.
	/// </param>
	/// <returns>
	/// The <see cref="T:System.Int32" />.
	/// </returns>
	private int ConvertHighMassAccuracyCentroidsToLabelPeaks(uint centroidCount, int blobIndex, LabelPeak[] labelPeaks, int startIndex, byte defaultChargeState, byte defaultFlags, ref int resolutionIndex)
	{
		bool hasWidths = HasWidths;
		long num = centroidCount + startIndex;
		for (int i = startIndex; i < num; i++)
		{
			labelPeaks[i] = new LabelPeak
			{
				Mass = BitConverter.ToDouble(_centroidBlob, blobIndex),
				Intensity = BitConverter.ToSingle(_centroidBlob, blobIndex + 8),
				Resolution = (hasWidths ? _widths[resolutionIndex++] : 0f),
				Charge = defaultChargeState,
				Flags = defaultFlags
			};
			blobIndex += 12;
		}
		return blobIndex;
	}

	/// <summary>
	/// Count the total number of peaks in all segments
	/// </summary>
	/// <returns>total number of peaks</returns>
	private uint CountLabels()
	{
		int num = _centroidBlobOffset;
		uint num2 = 0u;
		int num3 = (HasAccurateMassCentroids ? 12 : 8);
		for (int i = 0; i < Header.NumSegments; i++)
		{
			uint num4 = BitConverter.ToUInt32(_centroidBlob, num);
			num2 += num4;
			num += 4;
			num += (int)(num3 * num4);
		}
		return num2;
	}

	/// <summary>
	/// The expand label peaks - the label peak structure is actually the
	/// Centroid structure. If this is a profile packet type, the centroid data
	/// peaks are stored in the <see cref="F:ThermoFisher.CommonCore.RawFileReader.StructWrappers.Packets.LTFT.AdvancedPacketBase._labelStreamData" /> object.
	/// </summary>
	private void ExpandLabelData()
	{
		if (Header.NumCentroidWords == 0)
		{
			return;
		}
		_hasContinueProcessingLabelData = ContinueProcessingLabelData();
		if (!_hasContinueProcessingLabelData)
		{
			return;
		}
		byte flags = 0;
		byte chargeState = 0;
		SetFlagsAndChargeState(DefaultFeatureWord, ref flags, ref chargeState);
		uint num = CountLabels();
		int num2 = _centroidBlobOffset;
		int resolutionIndex = 0;
		int num3 = 0;
		LabelPeak[] array = ((num == 0) ? EmptyLabels : new LabelPeak[num]);
		CentroidCounts = new uint[Header.NumSegments];
		for (int i = 0; i < Header.NumSegments; i++)
		{
			uint num4 = BitConverter.ToUInt32(_centroidBlob, num2);
			CentroidCounts[i] = num4;
			num2 += 4;
			num2 = (HasAccurateMassCentroids ? ConvertHighMassAccuracyCentroidsToLabelPeaks(num4, num2, array, num3, chargeState, flags, ref resolutionIndex) : ConvertCentroidsToLabelPeaks(num4, num2, array, num3, chargeState, flags, ref resolutionIndex));
			num3 += (int)num4;
		}
		bool flag = _isExpandChargeLabels && (PacketScanDataFeatures & PacketFeatures.Chagre) != 0;
		int num5 = array.Length;
		uint[] nonDefaultFeatures = _nonDefaultFeatures;
		int num6 = nonDefaultFeatures.Length;
		if (IncludeRefPeaks)
		{
			List<LabelPeak> list = new List<LabelPeak>();
			for (int j = 0; j < num6; j++)
			{
				uint num7 = nonDefaultFeatures[j];
				int num8 = (int)(num7 & 0x3FFFF);
				if (num8 >= num5)
				{
					continue;
				}
				if (flag)
				{
					array[num8].Charge = (byte)((num7 & 0xFF000000u) >> 24);
				}
				if ((num7 & 0xF80000) != 0)
				{
					byte flags2 = array[num8].Flags;
					bool num9 = SetFlags(num7, ref flags2);
					array[num8].Flags = flags2;
					if (num9)
					{
						list.Add(array[num8]);
					}
				}
			}
			_referencePeaks = list.ToArray();
			_labelStreamData.SetLabelPeaks(array);
			return;
		}
		bool[] array2 = new bool[num5];
		int num10 = 0;
		if (flag)
		{
			for (int k = 0; k < num6; k++)
			{
				uint num11 = nonDefaultFeatures[k];
				int num12 = (int)(num11 & 0x3FFFF);
				if (num12 >= num5)
				{
					continue;
				}
				array[num12].Charge = (byte)((num11 & 0xFF000000u) >> 24);
				if ((num11 & 0xF80000) != 0)
				{
					byte flags3 = array[num12].Flags;
					if (SetFlags(num11, ref flags3))
					{
						num10++;
						array2[num12] = true;
					}
					array[num12].Flags = flags3;
				}
			}
		}
		else
		{
			int num13 = num6 - 2;
			int l;
			for (l = 0; l < num13; l++)
			{
				if ((nonDefaultFeatures[l] & 0xF80000) != 0)
				{
					num10 = ApplyFlags(nonDefaultFeatures[l], num5, array, num10, array2);
				}
				if ((nonDefaultFeatures[++l] & 0xF80000) != 0)
				{
					num10 = ApplyFlags(nonDefaultFeatures[l], num5, array, num10, array2);
				}
				if ((nonDefaultFeatures[++l] & 0xF80000) != 0)
				{
					num10 = ApplyFlags(nonDefaultFeatures[l], num5, array, num10, array2);
				}
			}
			for (; l < num6; l++)
			{
				if ((nonDefaultFeatures[l] & 0xF80000) != 0)
				{
					num10 = ApplyFlags(nonDefaultFeatures[l], num5, array, num10, array2);
				}
			}
		}
		LabelPeak[] array3 = (_referencePeaks = new LabelPeak[num10]);
		if (num10 == 0)
		{
			_labelStreamData.SetLabelPeaks(array);
			return;
		}
		LabelPeak[] array4 = new LabelPeak[num5 - num10];
		int num14 = 0;
		int num15 = 0;
		int num16 = num5 - 2;
		int m;
		for (m = 0; m < num16; m++)
		{
			if (array2[m])
			{
				array3[num15++] = array[m];
			}
			else
			{
				array4[num14++] = array[m];
			}
			if (array2[++m])
			{
				array3[num15++] = array[m];
			}
			else
			{
				array4[num14++] = array[m];
			}
			if (array2[++m])
			{
				array3[num15++] = array[m];
			}
			else
			{
				array4[num14++] = array[m];
			}
		}
		for (; m < num5; m++)
		{
			if (array2[m])
			{
				array3[num15++] = array[m];
			}
			else
			{
				array4[num14++] = array[m];
			}
		}
		_labelStreamData.SetLabelPeaks(array4);
	}

	/// <summary>
	/// The method sets flags.
	/// </summary>
	/// <param name="feature">
	/// The feature.
	/// </param>
	/// <param name="flags">
	/// The flags.
	/// </param>
	/// <returns>
	/// True if the feature is a reference or exception.
	/// </returns>
	private bool SetFlags(uint feature, ref byte flags)
	{
		bool result = false;
		if (_isExpandModifiedLabels && (feature & 0x80000) != 0)
		{
			if ((DefaultFeatureWord & 0x100) != 0)
			{
				flags |= 32;
			}
			else
			{
				flags |= 16;
			}
		}
		if (_isExpandExceptionLabels && (feature & 0x100000) != 0)
		{
			flags |= 8;
			result = true;
		}
		if (_isExpandReferenceLabels && (feature & 0x200000) != 0)
		{
			flags |= 4;
			result = true;
		}
		if (_isExpandMergedLabels && (feature & 0x400000) != 0)
		{
			flags |= 2;
		}
		if (_isExpandFragmentedLabels && (feature & 0x800000) != 0)
		{
			flags |= 1;
		}
		return result;
	}

	/// <summary>
	/// The method sets the flags and the charge state.
	/// </summary>
	/// <param name="feature">
	/// The feature.
	/// </param>
	/// <param name="flags">
	/// The flags.
	/// </param>
	/// <param name="chargeState">
	/// nThe charge state.
	/// </param>
	private void SetFlagsAndChargeState(uint feature, ref byte flags, ref byte chargeState)
	{
		if (_isExpandChargeLabels)
		{
			chargeState = (byte)((feature & 0xFF000000u) >> 24);
		}
		if ((feature & 0xF80000) != 0)
		{
			SetFlags(feature, ref flags);
		}
	}
}
