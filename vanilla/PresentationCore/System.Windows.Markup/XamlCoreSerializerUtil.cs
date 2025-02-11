using MS.Internal.PresentationCore;

namespace System.Windows.Markup;

internal static class XamlCoreSerializerUtil
{
	static XamlCoreSerializerUtil()
	{
		ThrowIfIAddChildInternal("not IAddChildInternal");
	}

	internal static void ThrowIfIAddChildInternal(object o)
	{
		if (o is IAddChildInternal)
		{
			throw new InvalidOperationException();
		}
	}

	internal static void ThrowIfNonWhiteSpaceInAddText(string s)
	{
		if (s == null)
		{
			return;
		}
		for (int i = 0; i < s.Length; i++)
		{
			if (!char.IsWhiteSpace(s[i]))
			{
				throw new ArgumentException(SR.Format(SR.NonWhiteSpaceInAddText, s));
			}
		}
	}
}
