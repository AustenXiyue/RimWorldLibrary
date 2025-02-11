namespace System.Windows;

internal static class LayoutDoubleUtil
{
	private const double eps = 1.53E-06;

	internal static bool AreClose(double value1, double value2)
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

	internal static bool LessThan(double value1, double value2)
	{
		if (value1 < value2)
		{
			return !AreClose(value1, value2);
		}
		return false;
	}
}
