using System.Xaml.MS.Impl;

namespace System.Xaml.Schema;

internal static class ClrNamespaceUriParser
{
	public static string GetUri(string clrNs, string assemblyName)
	{
		return string.Format(TypeConverterHelper.InvariantEnglishUS, "clr-namespace:{0};assembly={1}", clrNs, assemblyName);
	}

	public static bool TryParseUri(string uriInput, out string clrNs, out string assemblyName)
	{
		clrNs = null;
		assemblyName = null;
		int num = KS.IndexOf(uriInput, ':');
		if (num == -1)
		{
			return false;
		}
		if (!KS.Eq(uriInput.AsSpan(0, num), "clr-namespace"))
		{
			return false;
		}
		int num2 = num + 1;
		int num3 = KS.IndexOf(uriInput, ';');
		if (num3 == -1)
		{
			clrNs = uriInput.Substring(num2);
			assemblyName = null;
			return true;
		}
		int length = num3 - num2;
		clrNs = uriInput.Substring(num2, length);
		int num4 = num3 + 1;
		int num5 = KS.IndexOf(uriInput, '=');
		if (num5 == -1)
		{
			return false;
		}
		if (!KS.Eq(uriInput.AsSpan(num4, num5 - num4), "assembly"))
		{
			return false;
		}
		assemblyName = uriInput.Substring(num5 + 1);
		return true;
	}
}
