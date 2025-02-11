namespace System.Windows.Input;

internal static class AngleUtil
{
	public static double DegreesToRadians(double degrees)
	{
		return degrees * Math.PI / 180.0;
	}

	public static double RadiansToDegrees(double radians)
	{
		return radians * 180.0 / Math.PI;
	}
}
