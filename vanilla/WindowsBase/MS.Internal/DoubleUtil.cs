using System;
using System.Windows;
using MS.Internal.WindowsBase;

namespace MS.Internal;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal static class DoubleUtil
{
	internal const double DBL_EPSILON = 2.220446049250313E-16;

	internal const float FLT_MIN = 1.1754944E-38f;

	public static bool AreClose(double value1, double value2)
	{
		if (value1 == value2)
		{
			return true;
		}
		double num = (Math.Abs(value1) + Math.Abs(value2) + 10.0) * 2.220446049250313E-16;
		double num2 = value1 - value2;
		if (0.0 - num < num2)
		{
			return num > num2;
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

	public static bool GreaterThanZero(double value)
	{
		return value >= 2.220446049250313E-15;
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

	public static bool IsOne(double value)
	{
		return Math.Abs(value - 1.0) < 2.220446049250313E-15;
	}

	public static bool IsZero(double value)
	{
		return Math.Abs(value) < 2.220446049250313E-15;
	}

	public static bool AreClose(Point point1, Point point2)
	{
		if (AreClose(point1.X, point2.X))
		{
			return AreClose(point1.Y, point2.Y);
		}
		return false;
	}

	public static bool AreClose(Size size1, Size size2)
	{
		if (AreClose(size1.Width, size2.Width))
		{
			return AreClose(size1.Height, size2.Height);
		}
		return false;
	}

	public static bool AreClose(Vector vector1, Vector vector2)
	{
		if (AreClose(vector1.X, vector2.X))
		{
			return AreClose(vector1.Y, vector2.Y);
		}
		return false;
	}

	public static bool AreClose(Rect rect1, Rect rect2)
	{
		if (rect1.IsEmpty)
		{
			return rect2.IsEmpty;
		}
		if (!rect2.IsEmpty && AreClose(rect1.X, rect2.X) && AreClose(rect1.Y, rect2.Y) && AreClose(rect1.Height, rect2.Height))
		{
			return AreClose(rect1.Width, rect2.Width);
		}
		return false;
	}

	public static bool IsBetweenZeroAndOne(double val)
	{
		if (GreaterThanOrClose(val, 0.0))
		{
			return LessThanOrClose(val, 1.0);
		}
		return false;
	}

	public static int DoubleToInt(double val)
	{
		if (!(0.0 < val))
		{
			return (int)(val - 0.5);
		}
		return (int)(val + 0.5);
	}

	public static bool RectHasNaN(Rect r)
	{
		if (double.IsNaN(r.X) || double.IsNaN(r.Y) || double.IsNaN(r.Height) || double.IsNaN(r.Width))
		{
			return true;
		}
		return false;
	}
}
