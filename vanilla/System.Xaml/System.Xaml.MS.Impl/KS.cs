namespace System.Xaml.MS.Impl;

internal static class KS
{
	public static bool Eq(string a, string b)
	{
		return string.Equals(a, b, StringComparison.Ordinal);
	}

	public static bool Eq(ReadOnlySpan<char> a, ReadOnlySpan<char> b)
	{
		return MemoryExtensions.Equals(a, b, StringComparison.Ordinal);
	}

	public static int IndexOf(string src, string chars)
	{
		return src.IndexOf(chars, StringComparison.Ordinal);
	}

	public static int IndexOf(string src, char ch)
	{
		return src.IndexOf(ch, StringComparison.Ordinal);
	}

	public static bool EndsWith(string src, string target)
	{
		return src.EndsWith(target, StringComparison.Ordinal);
	}

	public static bool StartsWith(string src, string target)
	{
		return src.StartsWith(target, StringComparison.Ordinal);
	}

	public static string Fmt(string formatString, params object[] otherArgs)
	{
		return string.Format(TypeConverterHelper.InvariantEnglishUS, formatString, otherArgs);
	}
}
