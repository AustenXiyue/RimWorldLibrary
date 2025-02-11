namespace System.Windows.Documents;

internal static class Validators
{
	internal static bool IsValidFontSize(long fs)
	{
		if (fs >= 0)
		{
			return fs <= 32767;
		}
		return false;
	}

	internal static bool IsValidWidthType(long wt)
	{
		if (wt >= 0)
		{
			return wt <= 3;
		}
		return false;
	}

	internal static long MakeValidShading(long s)
	{
		if (s > 10000)
		{
			s = 10000L;
		}
		return s;
	}

	internal static long MakeValidBorderWidth(long w)
	{
		if (w < 0)
		{
			w = 0L;
		}
		if (w > 1440)
		{
			w = 1440L;
		}
		return w;
	}
}
