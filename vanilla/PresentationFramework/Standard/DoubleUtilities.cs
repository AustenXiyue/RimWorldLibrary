namespace Standard;

internal static class DoubleUtilities
{
	private const double Epsilon = 1.53E-06;

	public static bool AreClose(double value1, double value2)
	{
		if (value1 == value2)
		{
			return true;
		}
		double num = value1 - value2;
		if (num < 1.53E-06)
		{
			return num > -1.53E-06;
		}
		return false;
	}

	public static bool LessThan(double value1, double value2)
	{
		if (value1 < value2)
		{
			return !AreClose(value1, value2);
		}
		return false;
	}

	public static bool GreaterThan(double value1, double value2)
	{
		if (value1 > value2)
		{
			return !AreClose(value1, value2);
		}
		return false;
	}

	public static bool LessThanOrClose(double value1, double value2)
	{
		if (!(value1 < value2))
		{
			return AreClose(value1, value2);
		}
		return true;
	}

	public static bool GreaterThanOrClose(double value1, double value2)
	{
		if (!(value1 > value2))
		{
			return AreClose(value1, value2);
		}
		return true;
	}

	public static bool IsFinite(double value)
	{
		if (!double.IsNaN(value))
		{
			return !double.IsInfinity(value);
		}
		return false;
	}

	public static bool IsValidSize(double value)
	{
		if (IsFinite(value))
		{
			return GreaterThanOrClose(value, 0.0);
		}
		return false;
	}
}
