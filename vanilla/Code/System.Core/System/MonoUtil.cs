namespace System;

internal static class MonoUtil
{
	public static readonly bool IsUnix;

	static MonoUtil()
	{
		int platform = (int)Environment.OSVersion.Platform;
		IsUnix = platform == 4 || platform == 128 || platform == 6;
	}
}
