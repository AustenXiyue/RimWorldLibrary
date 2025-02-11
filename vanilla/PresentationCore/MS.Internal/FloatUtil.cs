using System;

namespace MS.Internal;

internal static class FloatUtil
{
	internal static float FLT_EPSILON = 1.1920929E-07f;

	internal static float FLT_MAX_PRECISION = 16777215f;

	internal static float INVERSE_FLT_MAX_PRECISION = 1f / FLT_MAX_PRECISION;

	public static bool AreClose(float a, float b)
	{
		if (a == b)
		{
			return true;
		}
		float num = (Math.Abs(a) + Math.Abs(b) + 10f) * FLT_EPSILON;
		float num2 = a - b;
		if (0f - num < num2)
		{
			return num > num2;
		}
		return false;
	}

	public static bool IsOne(float a)
	{
		return Math.Abs(a - 1f) < 10f * FLT_EPSILON;
	}

	public static bool IsZero(float a)
	{
		return Math.Abs(a) < 10f * FLT_EPSILON;
	}

	public static bool IsCloseToDivideByZero(float numerator, float denominator)
	{
		return Math.Abs(denominator) <= Math.Abs(numerator) * INVERSE_FLT_MAX_PRECISION;
	}
}
