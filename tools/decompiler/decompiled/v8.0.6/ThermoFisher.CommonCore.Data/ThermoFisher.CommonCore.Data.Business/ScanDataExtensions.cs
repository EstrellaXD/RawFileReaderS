using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ThermoFisher.CommonCore.Data.Interfaces;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This class can be ued to decode instrument specific additional data saved with a scan.
/// </summary>
public static class ScanDataExtensions
{
	internal class ChargeEnvelope : IChargeEnvelope, IChargeEnvelopeSummary
	{
		public double MonoisotopicMass { get; }

		public double CrossCorrelation { get; }

		public List<int> Peaks { get; set; }

		public int TopPeakCentroidId { get; set; }

		public bool IsIsotopicallyResolved { get; }

		public double AverageMass { get; }

		public ChargeEnvelope(IChargeEnvelopeSummary envelope)
		{
			MonoisotopicMass = envelope.MonoisotopicMass;
			CrossCorrelation = envelope.CrossCorrelation;
			TopPeakCentroidId = envelope.TopPeakCentroidId;
			IsIsotopicallyResolved = envelope.IsIsotopicallyResolved;
			AverageMass = envelope.AverageMass;
		}

		public static ChargeEnvelope DeepClone(IChargeEnvelope other)
		{
			return new ChargeEnvelope(other)
			{
				Peaks = new List<int>(other.Peaks)
			};
		}
	}

	/// <summary>
	/// An extension of centroids which includes:
	/// Peak charge envelopes.
	/// Annotations for each centroid
	/// </summary>
	internal class ExtendedCentroids : CentroidStream, IExtendedCentroids, ICentroidStreamAccess, ISimpleScanAccess
	{
		/// <summary>
		/// Gets or sets the annotation about charge envelopes for each peak
		/// </summary>
		public IApdPeakAnnotation[] Annotations { get; set; }

		public IChargeEnvelope[] ChargeEnvelopes { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether charge envelope data 
		/// was recorded for this scan
		/// </summary>
		public bool HasChargeEnvelopes { get; set; }

		private ExtendedCentroids()
		{
		}

		/// <summary>
		/// Created an extended centroid stream by making a copy of the base data
		/// </summary>
		/// <param name="labelData"></param>
		public ExtendedCentroids(ICentroidStreamAccess labelData)
			: base(labelData)
		{
		}

		internal static ExtendedCentroids CopyFrom(ExtendedCentroids extended)
		{
			ExtendedCentroids extendedCentroids = new ExtendedCentroids();
			extendedCentroids.DeepCopyFrom(extended);
			extendedCentroids.Annotations = (IApdPeakAnnotation[])extended.Annotations.Clone();
			IChargeEnvelope[] array = new IChargeEnvelope[extended.ChargeEnvelopes.Length];
			for (int i = 0; i < extended.ChargeEnvelopes.Length; i++)
			{
				array[i] = ChargeEnvelope.DeepClone(extended.ChargeEnvelopes[i]);
			}
			extended.ChargeEnvelopes = array;
			return extended;
		}
	}

	/// <summary>
	/// This class converts the byte array within the debug section of an Orbitrap scan to a list
	/// of centroid annotations. Those annotations may point to charge envelopes, they also become
	/// available.
	/// </summary>
	private class UnfoldedApdData : ICentroidAnnotations
	{
		/// <summary>
		/// This class defines the characteristics of an Orbitrap debug sub-segment.
		/// <para>
		/// The debug section according to unified_data_format.html has the format:
		/// <list type="table">
		///   <listheader>
		///     <term>  Type         </term><description>     Meaning                                                                </description>
		///   </listheader>
		///   <item>
		///     <term>  Uint32       </term><description>     Or'd combination of all debug sub-section flags + some extra bits      </description>
		///     <term>  SubSection   </term><description>     defined below                                                          </description>
		///     <term>  SubSection   </term><description>     further sub-sections, end defined by overall length of debug section   </description>
		///   </item>
		/// </list>
		/// </para>
		/// <para>
		/// The debug sub-section according to unified_data_format.html has the format:
		/// <list type="table">
		///   <listheader>
		///     <term>  Type         </term><description>     Meaning                                                                </description>
		///   </listheader>
		///   <item>
		///     <term>  Uint32       </term><description>     Content flags (0x100 = transients, 0x80000 for extensions, see below)  </description>
		///     <term>  UInt32       </term><description>     Count of 32-bit word of this sub-section                               </description>
		///     <term>  UInt32[]     </term><description>     Content depending on sub-section                                       </description>
		///   </item>
		/// </list>
		/// </para>
		/// <para>
		/// The APD extensions and all other extensions have an UInt32 specifier as first 32-bit word what data follows. The specifiers are:
		/// <list type="table">
		///   <listheader>
		///     <term>Value </term><description>     Meaning                                                                </description>
		///   </listheader>
		///   <item>
		///     <term>  1   </term><description>     transients (effectively not in use)                                    </description>
		///     <term>  2   </term><description>     Annotations (array of 16-bit values for each centroid)                 </description>
		///     <term>  3   </term><description>     ChargeEnvelopes (array of those)                                       </description>
		///   </item>
		/// </list>
		/// </para>
		/// <para>
		/// The APD annotations section is an array or annotation elements, each consisting of 16-bit values, one for each centroid. For an
		/// odd number of centroids, there exists a further 16-bit value of unknown content. Each annotation has a format described in
		/// class description of <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanDataExtensions.UnfoldedApdData.Annotation" />.
		/// </para>
		/// <para>
		/// The APD charge envelopes section is an array or charge envelopes. Each charge envelopes has a format described in
		/// class description of <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanDataExtensions.UnfoldedApdData.ChargeEnvelopeSummary" />.
		/// </para>
		/// </summary>
		private class DebugSubSegment
		{
			/// <summary>
			/// What type of sub-segment do we have?
			/// </summary>
			internal enum DebugSegmentType
			{
				Unknown,
				Transients,
				Annotations,
				ChargeEnvelopes
			}

			/// <summary>
			/// According to unified_data_format.html, DBUGCON definition, the 9th LSB bit
			/// is a send_transient section.
			/// </summary>
			private const uint TransientsDebugHeaderBit = 256u;

			/// <summary>
			/// This value is reserved for extensions of the debug section. Sub-segments have a further indicator
			/// describing the packet type.
			/// </summary>
			private const uint OrbitrapExtensionDebugHeaderBit = 524288u;

			/// <summary>
			/// This debug segment extension is an array of <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanDataExtensions.UnfoldedApdData.Annotation" />s.
			/// </summary>
			private const int OrbitrapAnnotationSegmentIdentifier = 0;

			/// <summary>
			/// This debug segment extension is an array of <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanDataExtensions.UnfoldedApdData.ChargeEnvelopeSummary" />s.
			/// </summary>
			private const int OrbitrapChargeEnvelopeSegmentIdentifier = 1;

			public IDataSegment Segment { get; }

			/// <summary>
			/// This is the offset into the byte array to which this structure belongs.
			/// </summary>
			internal long ArrayOffset { get; }

			/// <summary>
			/// Access to the size of data in section without headers in 32-bit words.
			/// </summary>
			public uint Size { get; }

			/// <summary>
			/// What kind of debug sub-segment is contained in this sub-segment?
			/// </summary>
			public DebugSegmentType SegmentType { get; }

			public DebugSubSegment(IDataSegment dataSegment)
			{
				Segment = dataSegment;
				byte[] bytes = dataSegment.Bytes;
				Size = (uint)bytes.Length;
				ArrayOffset = 0L;
				if ((long)dataSegment.Header == 256)
				{
					SegmentType = DebugSegmentType.Transients;
					return;
				}
				SegmentType = DebugSegmentType.Unknown;
				if ((long)dataSegment.Header == 524288 && Size > 4)
				{
					switch (BitConverter.ToInt32(bytes, (int)ArrayOffset))
					{
					case 0:
						SegmentType = DebugSegmentType.Annotations;
						Size -= 4;
						ArrayOffset += 4;
						break;
					case 1:
						SegmentType = DebugSegmentType.ChargeEnvelopes;
						Size -= 4;
						ArrayOffset += 4;
						break;
					}
				}
			}
		}

		/// <summary>
		/// This structure is one element of the annotations <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanDataExtensions.UnfoldedApdData.DebugSubSegment" />. The format is:
		/// <para>
		/// <list type="table">
		///   <listheader>
		///     <term>  Bit          </term><description>     Meaning                                                     </description>
		///   </listheader>
		///   <item>
		///     <term>  0-12 (LSB)   </term><description>     Index into array of charge envelopes (0x1FFF=no envelope)   </description>
		///     <term>  13           </term><description>     Flag IsNonIsotopicallyResolved                              </description>
		///     <term>  14           </term><description>     Flag IsClusterTop                                           </description>
		///     <term>  15           </term><description>     Flag IsMonoisotopic                                         </description>
		///   </item>
		/// </list>
		/// </para>
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 2)]
		private struct Annotation : IApdPeakAnnotation, IChargeEnvelopePeak
		{
			private readonly ushort _annotation;

			/// <summary>
			/// Implementation of <see cref="P:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopePeak.IsMonoisotopic" />
			/// </summary>
			public bool IsMonoisotopic => (_annotation & 0x8000) != 0;

			/// <summary>
			/// Implementation of <see cref="P:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopePeak.IsClusterTop" />
			/// </summary>
			public bool IsClusterTop => (_annotation & 0x4000) != 0;

			/// <summary>
			/// Implementation of <see cref="P:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopePeak.IsIsotopicallyResolved" />
			/// </summary>
			public bool IsIsotopicallyResolved => (_annotation & 0x2000) == 0;

			/// <summary>
			/// Implementation of <see cref="P:ThermoFisher.CommonCore.Data.Business.IApdPeakAnnotation.ChargeEnvelopeIndex" />
			/// </summary>
			public int? ChargeEnvelopeIndex
			{
				get
				{
					if ((_annotation & 0x1FFF) != 8191)
					{
						return _annotation & 0x1FFF;
					}
					return null;
				}
			}

			public Annotation(ushort s)
			{
				_annotation = s;
			}
		}

		/// <summary>
		/// This structure is one element of the charge envelopes <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanDataExtensions.UnfoldedApdData.DebugSubSegment" />. The format is:
		/// <para>
		/// <list type="table">
		///   <listheader>
		///     <term>  Type       </term><description>     Meaning                                                                                </description>
		///   </listheader>
		///   <item>
		///     <term>  double     </term><description>     Usually monoisotopic mass (with respect to instrument accuracy), can be average mass   </description>
		///     <term>  float      </term><description>     Cross-correlation of applied model (like averagine), can be 0 for various reasons      </description>
		///     <term>  (U)Int32   </term><description>     Index into centroid list of peak being most abundant in this envelope                  </description>
		///   </item>
		/// </list>
		/// </para>
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
		private struct ChargeEnvelopeStored
		{
			/// <summary>
			/// Access to the stored mass, which is either the monoisotopic mass or rge average mass, depending on flags of the annotation section.
			/// </summary>
			internal readonly double Mass;

			/// <summary>
			/// Cross-correlation in the range [0 - 1] of the fitting of the envelope.
			/// </summary>
			internal readonly float Xcorr;

			/// <summary>
			/// Index into the array of centroids. The indexed centroid is the most abundant of all centroids belonging to this envelope.
			/// <para>
			/// Orbitrap format never uses addresses more than 2**30 peaks, so no need for uint.
			/// </para>
			/// </summary>
			internal int Index;

			internal ChargeEnvelopeStored(double mass, float xcorr, int index)
			{
				Mass = mass;
				Xcorr = xcorr;
				Index = index;
			}
		}

		/// <summary>
		/// This class implements <see cref="T:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopeSummary" />. It bases on a stored Orbitrap data block.
		/// </summary>
		private class ChargeEnvelopeSummary : IChargeEnvelopeSummary
		{
			private readonly ChargeEnvelopeStored _stored;

			/// <summary>
			/// Returns the monoisotopic mass for a model-fitted envelope and all species belonging to
			/// this envelope with respect to the mass accuracy or 0 if resolved non-isotopically.
			/// See <see cref="P:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopeSummary.AverageMass" />.
			/// </summary>
			public double MonoisotopicMass
			{
				get
				{
					if (!IsNonIsotopicallyResolved)
					{
						return _stored.Mass;
					}
					return 0.0;
				}
			}

			/// <summary>
			/// Cross-correlation value of the model fitting in the range 0-1. Value remains 0 for non-isotopically
			/// resolved peaks or unsuccessful fittings. 
			/// </summary>
			public double CrossCorrelation => _stored.Xcorr;

			/// <summary>
			/// Index into centroid list to the most abundant centroid within this envelope. The value can be used
			/// for grouping.
			/// </summary>
			int IChargeEnvelopeSummary.TopPeakCentroidId => _stored.Index;

			/// <summary>
			/// Returns true if at least one peak belonging to the charge envelope was non-isotopically resolved.
			/// <para>
			/// Call the setter for each <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanDataExtensions.UnfoldedApdData.Annotation" /> in order to detect non-isotopically resolved peaks.
			/// </para>
			/// </summary>
			public bool IsNonIsotopicallyResolved { get; internal set; }

			/// <summary>
			/// Returns true if at least one peak belonging to the charge envelope was non-isotopically resolved.
			/// <para>
			/// Call the setter for each <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanDataExtensions.UnfoldedApdData.Annotation" /> in order to detect non-isotopically resolved peaks.
			/// </para>
			/// </summary>
			public bool IsIsotopicallyResolved => !IsNonIsotopicallyResolved;

			/// <summary>
			/// Returns 0 for a model-fitted envelope or the average mass if resolved non-isotopically.
			/// See <see cref="P:ThermoFisher.CommonCore.Data.Business.IChargeEnvelopeSummary.AverageMass" />.
			/// </summary>
			public double AverageMass
			{
				get
				{
					if (!IsNonIsotopicallyResolved)
					{
						return 0.0;
					}
					return _stored.Mass;
				}
			}

			/// <summary>
			/// Initialize a <see cref="T:ThermoFisher.CommonCore.Data.Business.ScanDataExtensions.UnfoldedApdData.ChargeEnvelopeSummary" /> and assign the structure on which it bases.
			/// </summary>
			/// <param name="stored">Data structure containing its core values</param>
			internal ChargeEnvelopeSummary(ChargeEnvelopeStored stored)
			{
				_stored = stored;
			}
		}

		/// <summary>
		/// Gets or sets a flag indicating whether there are charge envelopes.
		/// </summary>
		public bool HasChargeEnvelopes { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this is valid data.
		/// It is valid, even if there is no data available.
		/// It is invalid if data has been found, but cannot be decoded.
		/// </summary>
		public bool IsValid { get; }

		/// <summary>
		/// Get access to the charge envelopes. It is an empty array if no APD data has been calculated
		/// or if unfolding the binary data shows errors.
		/// </summary>
		/// <returns>Returns the charge envelopes of the scan</returns>
		public IChargeEnvelopeSummary[] ChargeEnvelopes { get; private set; } = Array.Empty<IChargeEnvelopeSummary>();

		/// <summary>
		/// Get access to the centroid peak annotations. It is an empty array if no APD data has been calculated
		/// or if unfolding the binary data shows errors.
		/// </summary>
		/// <returns>Returns the peak annotations of the scan</returns>
		public IApdPeakAnnotation[] CentroidAnnotations { get; private set; } = Array.Empty<IApdPeakAnnotation>();

		public UnfoldedApdData(IExtendedScanData extendedScanData, int centroidCount)
		{
			IsValid = Initialize(extendedScanData, centroidCount);
		}

		private bool Initialize(IExtendedScanData extendedScanData, int centroidCount)
		{
			if (extendedScanData == null || extendedScanData.DataSegments.Count == 0)
			{
				return true;
			}
			List<DebugSubSegment> list = new List<DebugSubSegment>();
			foreach (IDataSegment dataSegment in extendedScanData.DataSegments)
			{
				list.Add(new DebugSubSegment(dataSegment));
			}
			return TestSubSegmentsAndAssignData(list, in centroidCount);
		}

		private bool TestSubSegmentsAndAssignData(List<DebugSubSegment> debugSegments, in int centroidCount)
		{
			DebugSubSegment debugSubSegment = null;
			DebugSubSegment debugSubSegment2 = null;
			foreach (DebugSubSegment debugSegment in debugSegments)
			{
				if (debugSegment.SegmentType == DebugSubSegment.DebugSegmentType.Annotations)
				{
					if (debugSubSegment != null)
					{
						return false;
					}
					debugSubSegment = debugSegment;
				}
				else if (debugSegment.SegmentType == DebugSubSegment.DebugSegmentType.ChargeEnvelopes)
				{
					if (debugSubSegment2 != null)
					{
						return false;
					}
					debugSubSegment2 = debugSegment;
					HasChargeEnvelopes = true;
				}
			}
			if (debugSubSegment == null)
			{
				return debugSubSegment2 == null;
			}
			int num = Marshal.SizeOf<Annotation>();
			long num2 = debugSubSegment.Size / num;
			if (num2 < centroidCount || num2 > centroidCount + 1 || debugSubSegment.Size % num != 0L)
			{
				return false;
			}
			IApdPeakAnnotation[] array = new IApdPeakAnnotation[centroidCount];
			int num3 = (int)debugSubSegment.ArrayOffset;
			byte[] bytes = debugSubSegment.Segment.Bytes;
			int num4 = 0;
			while (num4 < centroidCount)
			{
				array[num4] = new Annotation(BitConverter.ToUInt16(bytes, num3));
				num4++;
				num3 += 2;
			}
			int num5 = 0;
			ChargeEnvelopeSummary[] array2;
			if (debugSubSegment2 == null)
			{
				array2 = new ChargeEnvelopeSummary[num5];
			}
			else
			{
				int num6 = Marshal.SizeOf<ChargeEnvelopeStored>();
				if (debugSubSegment2.Size % num6 != 0L)
				{
					return false;
				}
				num5 = (int)(debugSubSegment2.Size / num6);
				array2 = new ChargeEnvelopeSummary[num5];
				ChargeEnvelopes = new IChargeEnvelopeSummary[num5];
				byte[] bytes2 = debugSubSegment2.Segment.Bytes;
				num3 = (int)debugSubSegment2.ArrayOffset;
				for (int i = 0; i < num5; i++)
				{
					double num7 = BitConverter.ToDouble(bytes2, num3);
					num3 += 8;
					float num8 = BitConverter.ToSingle(bytes2, num3);
					num3 += 4;
					int num9 = BitConverter.ToInt32(bytes2, num3);
					num3 += 4;
					ChargeEnvelopes[i] = (array2[i] = new ChargeEnvelopeSummary(new ChargeEnvelopeStored(num7, num8, num9)));
					if ((num7 <= 1.0 && num7 != 0.0) || num7 > 1E+20)
					{
						return false;
					}
					if (num8 < 0f || num8 > 1.0000001f)
					{
						return false;
					}
					if (num9 < 0 || num9 >= centroidCount)
					{
						return false;
					}
				}
			}
			IApdPeakAnnotation[] array3 = array;
			foreach (IApdPeakAnnotation apdPeakAnnotation in array3)
			{
				int? chargeEnvelopeIndex = apdPeakAnnotation.ChargeEnvelopeIndex;
				if (chargeEnvelopeIndex.HasValue)
				{
					if (chargeEnvelopeIndex >= num5 || chargeEnvelopeIndex < 0)
					{
						return false;
					}
					if (!apdPeakAnnotation.IsIsotopicallyResolved)
					{
						array2[chargeEnvelopeIndex.Value].IsNonIsotopicallyResolved = true;
					}
				}
			}
			CentroidAnnotations = array;
			return true;
		}
	}

	/// <summary>
	/// Obtain the annotations for a specific scan
	/// </summary>
	/// <param name="data">The raw file</param>
	/// <param name="scan">scan number</param>
	/// <returns>Annotations for this scan</returns>
	public static ICentroidAnnotations ScanAnnotations(this IRawDataExtended data, int scan)
	{
		CentroidStream centroidStream = data.GetCentroidStream(scan, includeReferenceAndExceptionPeaks: true);
		return new UnfoldedApdData(data.GetExtendedScanData(scan), centroidStream.Length);
	}

	/// <summary>
	/// Read centroid data for a scan, with annotations (where available).
	/// These annotations are set by the charge calculator in some instruments.
	/// </summary>
	/// <param name="data">The raw file</param>
	/// <param name="scan">scan number</param>
	/// <param name="includeReferenceAndExceptionPeaks">set if reference and exception peaks should be returned</param>
	/// <returns>The extended scan</returns>
	public static IExtendedCentroids CentroidsWithAnnotations(this IRawDataExtended data, int scan, bool includeReferenceAndExceptionPeaks = false)
	{
		IExtendedScanData extendedScanData = data.GetExtendedScanData(scan);
		return GetExtendedCentroids(data, extendedScanData, scan, includeReferenceAndExceptionPeaks);
	}

	/// <summary>
	/// Read centroid data for a scan, with annotations (where available).
	/// These annotations are set by the charge calculator in some instruments.
	/// </summary>
	/// <param name="data">The raw file</param>
	/// <param name="scan">scan number</param>
	/// <param name="includeReferenceAndExceptionPeaks">set if reference and exception peaks should be returned</param>
	/// <returns>The extended scan</returns>
	public static IExtendedCentroids CentroidsWithAnnotations(this IDetectorReader data, int scan, bool includeReferenceAndExceptionPeaks = false)
	{
		IExtendedScanData extendedScanData = data.GetExtendedScanData(scan);
		return GetExtendedCentroids(data, extendedScanData, scan, includeReferenceAndExceptionPeaks);
	}

	/// <summary>
	/// Read centroid data for a scan, with annotations (where available).
	/// These annotations are set by the charge calculator in some instruments.
	/// </summary>
	/// <param name="decoder">decoder for 1 scan</param>
	/// <param name="includeReferenceAndExceptionPeaks">set if reference and exception peaks should be returned</param>
	/// <returns>The extended centroids</returns>
	public static IExtendedCentroids CentroidsWithAnnotations(this IMsScanDecoder decoder, bool includeReferenceAndExceptionPeaks = false)
	{
		IExtendedScanData extendedScanData = decoder.DecodeExtendedScanData();
		ICentroidStreamAccess centroidStreamAccess = decoder.DecodeCentroidStream(includeReferenceAndExceptionPeaks: true);
		UnfoldedApdData unfoldedApdData = new UnfoldedApdData(extendedScanData, centroidStreamAccess.Length);
		IChargeEnvelopeSummary[] chargeEnvelopes = unfoldedApdData.ChargeEnvelopes;
		IChargeEnvelope[] array = new IChargeEnvelope[chargeEnvelopes.Length];
		for (int i = 0; i < chargeEnvelopes.Length; i++)
		{
			IChargeEnvelopeSummary envelope = chargeEnvelopes[i];
			array[i] = new ChargeEnvelope(envelope);
		}
		IApdPeakAnnotation[] annotations = unfoldedApdData.CentroidAnnotations;
		if (!includeReferenceAndExceptionPeaks && centroidStreamAccess is CentroidStream labelData)
		{
			annotations = StripRefPeaks(labelData, array, annotations);
		}
		FindChargeGroups(unfoldedApdData.CentroidAnnotations, array);
		return new ExtendedCentroids(centroidStreamAccess)
		{
			ChargeEnvelopes = array,
			Annotations = annotations,
			HasChargeEnvelopes = unfoldedApdData.HasChargeEnvelopes
		};
	}

	/// <summary>
	/// Read centroid data for a scan, with annotations (where available).
	/// These annotations are set by the charge calculator in some instruments.
	/// This version returns an internal type
	/// </summary>
	/// <param name="data">The raw file</param>
	/// <param name="extendedScanData">scan "extended data" which contains APD items</param>
	/// <param name="scan">scan number</param>
	/// <param name="includeReferenceAndExceptionPeaks">set if reference and exception peaks should be returned</param>
	/// <returns>The extended scan</returns>
	internal static ExtendedCentroids GetExtendedCentroids(IDetectorReaderPlus data, IExtendedScanData extendedScanData, int scan, bool includeReferenceAndExceptionPeaks)
	{
		CentroidStream centroidStream = data.GetCentroidStream(scan, includeReferenceAndExceptionPeaks: true);
		UnfoldedApdData unfoldedApdData = new UnfoldedApdData(extendedScanData, centroidStream.Length);
		IChargeEnvelopeSummary[] chargeEnvelopes = unfoldedApdData.ChargeEnvelopes;
		IChargeEnvelope[] array = new IChargeEnvelope[chargeEnvelopes.Length];
		for (int i = 0; i < chargeEnvelopes.Length; i++)
		{
			IChargeEnvelopeSummary envelope = chargeEnvelopes[i];
			array[i] = new ChargeEnvelope(envelope);
		}
		IApdPeakAnnotation[] annotations = unfoldedApdData.CentroidAnnotations;
		if (!includeReferenceAndExceptionPeaks)
		{
			annotations = StripRefPeaks(centroidStream, array, annotations);
		}
		FindChargeGroups(unfoldedApdData.CentroidAnnotations, array);
		return new ExtendedCentroids(centroidStream)
		{
			ChargeEnvelopes = array,
			Annotations = annotations,
			HasChargeEnvelopes = unfoldedApdData.HasChargeEnvelopes
		};
	}

	private static IApdPeakAnnotation[] StripRefPeaks(CentroidStream labelData, IChargeEnvelope[] envelopes, IApdPeakAnnotation[] annotations)
	{
		int[] array = new int[labelData.Length];
		int num = 0;
		PeakOptions[] flags = labelData.Flags;
		for (int i = 0; i < labelData.Length; i++)
		{
			if ((flags[i] & (PeakOptions.Exception | PeakOptions.Reference)) != PeakOptions.None)
			{
				array[i] = -1;
			}
			else
			{
				array[i] = num++;
			}
		}
		bool flag = annotations != null && annotations.Length == labelData.Length;
		IApdPeakAnnotation[] array2 = annotations;
		if (num != labelData.Length)
		{
			double[] array3 = new double[num];
			double[] array4 = new double[num];
			PeakOptions[] array5 = new PeakOptions[num];
			double[] array6 = new double[num];
			double[] array7 = new double[num];
			double[] array8 = new double[num];
			double[] array9 = new double[num];
			if (flag)
			{
				array2 = new IApdPeakAnnotation[num];
			}
			double[] masses = labelData.Masses;
			double[] intensities = labelData.Intensities;
			PeakOptions[] flags2 = labelData.Flags;
			double[] charges = labelData.Charges;
			double[] noises = labelData.Noises;
			double[] baselines = labelData.Baselines;
			double[] resolutions = labelData.Resolutions;
			for (int j = 0; j < labelData.Length; j++)
			{
				int num2 = array[j];
				if (num2 >= 0)
				{
					array3[num2] = masses[j];
					array4[num2] = intensities[j];
					array5[num2] = flags2[j];
					array6[num2] = charges[j];
					array7[num2] = noises[j];
					array8[num2] = baselines[j];
					array9[num2] = resolutions[j];
					if (flag)
					{
						array2[num2] = annotations[j];
					}
				}
			}
			labelData.Masses = array3;
			labelData.Intensities = array4;
			labelData.Flags = array5;
			labelData.Charges = array6;
			labelData.Noises = array7;
			labelData.Baselines = array8;
			labelData.Resolutions = array9;
			labelData.Length = array3.Length;
			foreach (IChargeEnvelope chargeEnvelope in envelopes)
			{
				int topPeakCentroidId = chargeEnvelope.TopPeakCentroidId;
				if (topPeakCentroidId >= 0 && topPeakCentroidId < array.Length)
				{
					chargeEnvelope.TopPeakCentroidId = array[topPeakCentroidId];
				}
			}
		}
		return array2;
	}

	private static void FindChargeGroups(IApdPeakAnnotation[] annotations, IChargeEnvelope[] envelopes)
	{
		for (int i = 0; i < envelopes.Length; i++)
		{
			envelopes[i].Peaks = new List<int>();
		}
		int num = 0;
		for (int j = 0; j < annotations.Length; j++)
		{
			int? chargeEnvelopeIndex = annotations[j].ChargeEnvelopeIndex;
			if (chargeEnvelopeIndex.HasValue)
			{
				int value = chargeEnvelopeIndex.Value;
				if (value >= 0 && value < envelopes.Length)
				{
					envelopes[value].Peaks.Add(num);
				}
			}
			num++;
		}
	}
}
