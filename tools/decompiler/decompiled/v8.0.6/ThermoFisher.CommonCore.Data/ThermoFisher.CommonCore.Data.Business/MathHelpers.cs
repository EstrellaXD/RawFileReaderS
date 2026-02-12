using System;

namespace ThermoFisher.CommonCore.Data.Business;

/// <summary>
/// General math routines
/// </summary>
public static class MathHelpers
{
	/// <summary>
	/// Calculates the discrete Fourier transform of a set of  real-valued data points.
	/// <para>
	/// Calculates the FFT of a set of dataPointsCount real-valued data points.
	/// The routine replaces the input data stored in data[0..dataPointsCount-1]
	/// by the positive frequency half of its complex Fourier transform.
	/// The real-valued first and last components of the complex transform 
	/// are returned as elements data[0] and data[1], respectively.
	/// dataPointsCount MUST be a power of 2. 
	/// This routine also calculates the inverse transform of a complex 
	/// data array if it is the transform of real data (the result in this
	/// case must be multiplied with 2/n).
	/// </para>
	/// </summary>
	/// <param name="data">
	/// The input array of real-valued data points.
	/// </param>
	/// <param name="dataPointsCount">
	/// The number of data points in the array (must be a power of 2).
	/// </param>
	/// <param name="sign">
	/// Flag indicating to calculate the FFT (sign=1) or the inverse FFT (sign=-1).
	/// </param>
	public static void CalculateRealFFT(double[] data, int dataPointsCount, int sign)
	{
		int num = dataPointsCount / 2;
		double num2 = Math.PI / (double)num;
		double num3;
		if (sign == 1)
		{
			num3 = -0.5;
			CalculateFFT(data, num, 1);
		}
		else
		{
			num3 = 0.5;
			num2 = 0.0 - num2;
		}
		double num4 = Math.Sin(0.5 * num2);
		double num5 = -2.0 * num4 * num4;
		double num6 = Math.Sin(num2);
		double num7 = 1.0 + num5;
		double num8 = num6;
		int num9 = dataPointsCount + 3;
		double num10 = 0.5;
		for (int i = 2; i <= dataPointsCount / 4; i++)
		{
			int num11 = i + i - 1;
			int num12 = 1 + num11;
			int num13 = num9 - num12;
			int num14 = 1 + num13;
			double num15 = data[num11 - 1];
			double num16 = data[num12 - 1];
			double num17 = data[num13 - 1];
			double num18 = data[num14 - 1];
			double num19 = num10 * (num15 + num17);
			double num20 = num10 * (num16 - num18);
			double num21 = (0.0 - num3) * (num16 + num18);
			double num22 = num3 * (num15 - num17);
			double num23 = num7 * num21;
			double num24 = num8 * num22;
			double num25 = num7 * num22;
			double num26 = num8 * num21;
			data[num11 - 1] = num19 + num23 - num24;
			data[num12 - 1] = num20 + num25 + num26;
			data[num13 - 1] = num19 - num23 + num24;
			data[num14 - 1] = 0.0 - num20 + num25 + num26;
			num7 = (num4 = num7) * num5 - num8 * num6 + num7;
			num8 = num8 * num5 + num4 * num6 + num8;
		}
		if (sign == 1)
		{
			data[0] += data[1];
			data[1] = data[0];
		}
		else
		{
			data[0] = num10 * (data[0] + data[1]);
			data[1] = data[0];
			CalculateFFT(data, num, -1);
		}
	}

	/// <summary>
	/// Calculates the discrete Fourier transform of a set of data points.
	/// <para>
	/// Calculates the discrete Fourier transform of a set of data points.
	/// The routine replaces data[0..2*dataPointsCount-1] by its discrete Fourier
	/// transform, if sign is input as 1; or replaces data[0..2*dataPointsCount-1]
	/// by dataPointsCount times its inverse discrete Fourier transform, if sign
	/// is input as -1.
	/// data is a complex array of length dataPointsCount or (i.e. dataPointsCount real/imaginary 
	/// pairs, which make up dataPointsCount array elements), equivalently, a real
	/// array of length 2*dataPointsCount. 
	/// dataPointsCount MUST be an integer power of 2.
	/// </para>
	/// </summary>
	/// <param name="data">
	/// Data to transform, as real and imaginary pairs (index 0= real, 1=imaginary etc.)
	/// </param>
	/// <param name="dataPointsCount">
	/// Number of valid points in the data.
	/// </param>
	/// <param name="sign">Sign of the data
	/// </param>
	public static void CalculateFFT(double[] data, int dataPointsCount, int sign)
	{
		int num = dataPointsCount * 2;
		int num2 = 1;
		for (int i = 1; i < num; i += 2)
		{
			if (num2 > i)
			{
				Swap(data, num2 - 1, i - 1);
				Swap(data, num2, i);
			}
			int num3 = dataPointsCount;
			while (num3 >= 2 && num2 > num3)
			{
				num2 -= num3;
				num3 /= 2;
			}
			num2 += num3;
		}
		int num4 = 2;
		while (num > num4)
		{
			int num5 = num4 * 2;
			double num6 = (double)sign * (Math.PI * 2.0 / (double)num4);
			double num7 = Math.Sin(0.5 * num6);
			double num8 = -2.0 * num7 * num7;
			double num9 = Math.Sin(num6);
			double num10 = 1.0;
			double num11 = 0.0;
			for (int num3 = 1; num3 < num4; num3 += 2)
			{
				for (int i = num3; i <= num; i += num5)
				{
					num2 = i + num4;
					double num12 = data[num2 - 1];
					double num13 = data[num2];
					double num14 = num10 * num12 - num11 * num13;
					double num15 = num10 * num13 + num11 * num12;
					data[num2 - 1] = data[i - 1] - num14;
					data[num2] = data[i] - num15;
					data[i - 1] += num14;
					data[i] += num15;
				}
				num10 = (num7 = num10) * num8 - num11 * num9 + num10;
				num11 = num11 * num8 + num7 * num9 + num11;
			}
			num4 = num5;
		}
	}

	/// <summary>
	/// Swap array elements
	/// </summary>
	/// <param name="data">
	/// The array to swap.
	/// </param>
	/// <param name="source">
	/// The source
	/// </param>
	/// <param name="dest">
	/// The destination.
	/// </param>
	private static void Swap(double[] data, int source, int dest)
	{
		double num = data[dest];
		data[dest] = data[source];
		data[source] = num;
	}
}
