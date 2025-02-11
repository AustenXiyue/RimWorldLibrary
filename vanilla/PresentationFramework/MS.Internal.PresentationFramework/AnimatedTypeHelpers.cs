using System;
using System.Windows;

namespace MS.Internal.PresentationFramework;

internal static class AnimatedTypeHelpers
{
	private static double InterpolateDouble(double from, double to, double progress)
	{
		return from + (to - from) * progress;
	}

	internal static Thickness InterpolateThickness(Thickness from, Thickness to, double progress)
	{
		return new Thickness(InterpolateDouble(from.Left, to.Left, progress), InterpolateDouble(from.Top, to.Top, progress), InterpolateDouble(from.Right, to.Right, progress), InterpolateDouble(from.Bottom, to.Bottom, progress));
	}

	private static double AddDouble(double value1, double value2)
	{
		return value1 + value2;
	}

	internal static Thickness AddThickness(Thickness value1, Thickness value2)
	{
		return new Thickness(AddDouble(value1.Left, value2.Left), AddDouble(value1.Top, value2.Top), AddDouble(value1.Right, value2.Right), AddDouble(value1.Bottom, value2.Bottom));
	}

	internal static Thickness SubtractThickness(Thickness value1, Thickness value2)
	{
		return new Thickness(value1.Left - value2.Left, value1.Top - value2.Top, value1.Right - value2.Right, value1.Bottom - value2.Bottom);
	}

	private static double GetSegmentLengthDouble(double from, double to)
	{
		return Math.Abs(to - from);
	}

	internal static double GetSegmentLengthThickness(Thickness from, Thickness to)
	{
		return Math.Sqrt(Math.Pow(GetSegmentLengthDouble(from.Left, to.Left), 2.0) + Math.Pow(GetSegmentLengthDouble(from.Top, to.Top), 2.0) + Math.Pow(GetSegmentLengthDouble(from.Right, to.Right), 2.0) + Math.Pow(GetSegmentLengthDouble(from.Bottom, to.Bottom), 2.0));
	}

	private static double ScaleDouble(double value, double factor)
	{
		return value * factor;
	}

	internal static Thickness ScaleThickness(Thickness value, double factor)
	{
		return new Thickness(ScaleDouble(value.Left, factor), ScaleDouble(value.Top, factor), ScaleDouble(value.Right, factor), ScaleDouble(value.Bottom, factor));
	}

	private static bool IsValidAnimationValueDouble(double value)
	{
		if (IsInvalidDouble(value))
		{
			return false;
		}
		return true;
	}

	internal static bool IsValidAnimationValueThickness(Thickness value)
	{
		if (IsValidAnimationValueDouble(value.Left) || IsValidAnimationValueDouble(value.Top) || IsValidAnimationValueDouble(value.Right) || IsValidAnimationValueDouble(value.Bottom))
		{
			return true;
		}
		return false;
	}

	private static double GetZeroValueDouble(double baseValue)
	{
		return 0.0;
	}

	internal static Thickness GetZeroValueThickness(Thickness baseValue)
	{
		return new Thickness(GetZeroValueDouble(baseValue.Left), GetZeroValueDouble(baseValue.Top), GetZeroValueDouble(baseValue.Right), GetZeroValueDouble(baseValue.Bottom));
	}

	private static bool IsInvalidDouble(double value)
	{
		if (!double.IsInfinity(value))
		{
			return double.IsNaN(value);
		}
		return true;
	}
}
