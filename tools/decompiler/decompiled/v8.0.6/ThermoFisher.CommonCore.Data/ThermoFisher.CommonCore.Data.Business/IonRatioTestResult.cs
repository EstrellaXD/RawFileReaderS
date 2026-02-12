using System;
using System.Runtime.Serialization;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// This class encapsulates the results of an ion ratio test for one ion.
/// </summary>
[Serializable]
[DataContract]
public class IonRatioTestResult : ICloneable, IIonRatioTestResultAccess
{
	/// <summary>
	/// Gets or sets a value indicating whether the coelution test has passed for this ion
	/// </summary>
	[DataMember]
	public bool PassedIonCoelutionTest { get; set; }

	/// <summary>
	/// Gets or sets the results of the coelution test
	/// targetCompoundPeak.Apex.RetentionTime - ion.Apex.RetentionTime;
	/// </summary>
	[DataMember]
	public double MeasuredCoelution { get; set; }

	/// <summary>
	/// Gets or sets the measured ion ratio, as a percentage
	/// <code>(qualifierIonResponse * 100) / targetCoumpoundResponce</code>
	/// </summary>
	[DataMember]
	public double MeasuredRatio { get; set; }

	/// <summary>
	/// Gets or sets the Window in absolute % used to bound this test
	/// </summary>
	[DataMember]
	public double AbsWindow { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the ratio test passed for this ion
	/// </summary>
	[DataMember]
	public bool PassedIonRatioTest { get; set; }

	/// <summary>
	/// Gets or sets the Mass which was tested
	/// </summary>
	[DataMember]
	public double Mass { get; set; }

	/// <summary>
	/// Gets or sets The peak which was found in the IRC chromatogram
	/// </summary>
	[DataMember]
	public Peak DetectedPeak { get; set; }

	/// <summary>
	/// Implementation of <c>ICloneable.Clone</c> method.
	/// Creates deep copy of this instance.
	/// </summary>
	/// <returns>An exact copy of the current Result.</returns>
	public virtual object Clone()
	{
		IonRatioTestResult obj = (IonRatioTestResult)MemberwiseClone();
		obj.DetectedPeak = (Peak)DetectedPeak.Clone();
		return obj;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IonRatioTestResult" /> class. 
	/// Default constructor
	/// </summary>
	public IonRatioTestResult()
	{
		DetectedPeak = new Peak();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:ThermoFisher.CommonCore.Data.Business.IonRatioTestResult" /> class. 
	/// Copy constructor
	/// </summary>
	/// <param name="access">
	/// Object to copy
	/// </param>
	public IonRatioTestResult(IIonRatioTestResultAccess access)
	{
		if (access != null)
		{
			AbsWindow = access.AbsWindow;
			DetectedPeak = access.DetectedPeak.Clone() as Peak;
			Mass = access.Mass;
			MeasuredCoelution = access.MeasuredCoelution;
			MeasuredRatio = access.MeasuredRatio;
			PassedIonCoelutionTest = access.PassedIonCoelutionTest;
			PassedIonRatioTest = access.PassedIonRatioTest;
		}
	}
}
