using System.Runtime.InteropServices;

namespace System;

/// <summary>Represents a pseudo-random number generator, a device that produces a sequence of numbers that meet certain statistical requirements for randomness.</summary>
/// <filterpriority>1</filterpriority>
[Serializable]
[ComVisible(true)]
public class Random
{
	private const int MBIG = int.MaxValue;

	private const int MSEED = 161803398;

	private const int MZ = 0;

	private int inext;

	private int inextp;

	private int[] SeedArray = new int[56];

	/// <summary>Initializes a new instance of the <see cref="T:System.Random" /> class, using a time-dependent default seed value.</summary>
	public Random()
		: this(Environment.TickCount)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Random" /> class, using the specified seed value.</summary>
	/// <param name="Seed">A number used to calculate a starting value for the pseudo-random number sequence. If a negative number is specified, the absolute value of the number is used. </param>
	public Random(int Seed)
	{
		int num = 161803398 - ((Seed == int.MinValue) ? int.MaxValue : Math.Abs(Seed));
		SeedArray[55] = num;
		int num2 = 1;
		for (int i = 1; i < 55; i++)
		{
			int num3 = 21 * i % 55;
			SeedArray[num3] = num2;
			num2 = num - num2;
			if (num2 < 0)
			{
				num2 += int.MaxValue;
			}
			num = SeedArray[num3];
		}
		for (int j = 1; j < 5; j++)
		{
			for (int k = 1; k < 56; k++)
			{
				SeedArray[k] -= SeedArray[1 + (k + 30) % 55];
				if (SeedArray[k] < 0)
				{
					SeedArray[k] += int.MaxValue;
				}
			}
		}
		inext = 0;
		inextp = 21;
		Seed = 1;
	}

	/// <summary>Returns a random number between 0.0 and 1.0.</summary>
	/// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
	protected virtual double Sample()
	{
		return (double)InternalSample() * 4.656612875245797E-10;
	}

	private int InternalSample()
	{
		int num = inext;
		int num2 = inextp;
		if (++num >= 56)
		{
			num = 1;
		}
		if (++num2 >= 56)
		{
			num2 = 1;
		}
		int num3 = SeedArray[num] - SeedArray[num2];
		if (num3 == int.MaxValue)
		{
			num3--;
		}
		if (num3 < 0)
		{
			num3 += int.MaxValue;
		}
		SeedArray[num] = num3;
		inext = num;
		inextp = num2;
		return num3;
	}

	/// <summary>Returns a nonnegative random number.</summary>
	/// <returns>A 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue" />.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual int Next()
	{
		return InternalSample();
	}

	private double GetSampleForLargeRange()
	{
		int num = InternalSample();
		if (InternalSample() % 2 == 0)
		{
			num = -num;
		}
		return ((double)num + 2147483646.0) / 4294967293.0;
	}

	/// <summary>Returns a random number within a specified range.</summary>
	/// <returns>A 32-bit signed integer greater than or equal to <paramref name="minValue" /> and less than <paramref name="maxValue" />; that is, the range of return values includes <paramref name="minValue" /> but not <paramref name="maxValue" />. If <paramref name="minValue" /> equals <paramref name="maxValue" />, <paramref name="minValue" /> is returned.</returns>
	/// <param name="minValue">The inclusive lower bound of the random number returned. </param>
	/// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue" /> must be greater than or equal to <paramref name="minValue" />. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="minValue" /> is greater than <paramref name="maxValue" />. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual int Next(int minValue, int maxValue)
	{
		if (minValue > maxValue)
		{
			throw new ArgumentOutOfRangeException("minValue", Environment.GetResourceString("'{0}' cannot be greater than {1}.", "minValue", "maxValue"));
		}
		long num = (long)maxValue - (long)minValue;
		if (num <= int.MaxValue)
		{
			return (int)(Sample() * (double)num) + minValue;
		}
		return (int)((long)(GetSampleForLargeRange() * (double)num) + minValue);
	}

	/// <summary>Returns a nonnegative random number less than the specified maximum.</summary>
	/// <returns>A 32-bit signed integer greater than or equal to zero, and less than <paramref name="maxValue" />; that is, the range of return values ordinarily includes zero but not <paramref name="maxValue" />. However, if <paramref name="maxValue" /> equals zero, <paramref name="maxValue" /> is returned.</returns>
	/// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue" /> must be greater than or equal to zero. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maxValue" /> is less than zero. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual int Next(int maxValue)
	{
		if (maxValue < 0)
		{
			throw new ArgumentOutOfRangeException("maxValue", Environment.GetResourceString("'{0}' must be greater than zero.", "maxValue"));
		}
		return (int)(Sample() * (double)maxValue);
	}

	/// <summary>Returns a random number between 0.0 and 1.0.</summary>
	/// <returns>A double-precision floating point number greater than or equal to 0.0, and less than 1.0.</returns>
	/// <filterpriority>1</filterpriority>
	public virtual double NextDouble()
	{
		return Sample();
	}

	/// <summary>Fills the elements of a specified array of bytes with random numbers.</summary>
	/// <param name="buffer">An array of bytes to contain random numbers. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="buffer" /> is null. </exception>
	/// <filterpriority>1</filterpriority>
	public virtual void NextBytes(byte[] buffer)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		for (int i = 0; i < buffer.Length; i++)
		{
			buffer[i] = (byte)(InternalSample() % 256);
		}
	}
}
