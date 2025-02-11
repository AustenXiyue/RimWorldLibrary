using System.Windows.Media.Animation;

namespace MS.Internal;

internal static class TimeEnumHelper
{
	private const int c_maxFillBehavior = 1;

	private const int c_maxSlipBehavior = 1;

	private const int _maxTimeSeekOrigin = 1;

	private const byte _maxPathAnimationSource = 2;

	internal static bool IsValidFillBehavior(FillBehavior value)
	{
		if (FillBehavior.HoldEnd <= value)
		{
			return value <= FillBehavior.Stop;
		}
		return false;
	}

	internal static bool IsValidSlipBehavior(SlipBehavior value)
	{
		if (SlipBehavior.Grow <= value)
		{
			return value <= SlipBehavior.Slip;
		}
		return false;
	}

	internal static bool IsValidTimeSeekOrigin(TimeSeekOrigin value)
	{
		if (TimeSeekOrigin.BeginTime <= value)
		{
			return value <= TimeSeekOrigin.Duration;
		}
		return false;
	}

	internal static bool IsValidPathAnimationSource(PathAnimationSource value)
	{
		if (0 <= (int)value)
		{
			return (int)value <= 2;
		}
		return false;
	}
}
