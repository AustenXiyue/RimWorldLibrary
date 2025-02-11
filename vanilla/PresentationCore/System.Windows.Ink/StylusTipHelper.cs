namespace System.Windows.Ink;

internal static class StylusTipHelper
{
	internal static bool IsDefined(StylusTip stylusTip)
	{
		if (stylusTip < StylusTip.Rectangle || stylusTip > StylusTip.Ellipse)
		{
			return false;
		}
		return true;
	}
}
