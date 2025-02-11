using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Runtime.InteropServices;
using System.Windows.Navigation;
using MS.Internal.PresentationCore;
using MS.Win32;

namespace MS.Internal.FontCache;

[FriendAccessAllowed]
internal static class Util
{
	internal const int nullOffset = -1;

	private static readonly string[] SupportedExtensions;

	private static readonly char[] InvalidFileNameChars;

	internal const UriComponents UriWithoutFragment = UriComponents.HttpRequestUrl | UriComponents.UserInfo;

	private const string WinDir = "windir";

	private const string EmptyFontFamilyReference = "#";

	private const string EmptyCanonicalName = "";

	private static readonly object _dpiLock;

	private static int _dpi;

	private static bool _dpiInitialized;

	private static readonly string _windowsFontsLocalPath;

	private static readonly Uri _windowsFontsUriObject;

	private static readonly string _windowsFontsUriString;

	internal static string CompositeFontExtension => SupportedExtensions[0];

	internal static string WindowsFontsLocalPath => _windowsFontsLocalPath;

	internal static float PixelsPerDip => (float)Dpi / 96f;

	internal static int Dpi
	{
		get
		{
			if (!_dpiInitialized)
			{
				lock (_dpiLock)
				{
					if (!_dpiInitialized)
					{
						HandleRef hWnd = new HandleRef(null, IntPtr.Zero);
						nint dC = MS.Win32.UnsafeNativeMethods.GetDC(hWnd);
						if (dC == IntPtr.Zero)
						{
							throw new Win32Exception();
						}
						try
						{
							_dpi = MS.Win32.UnsafeNativeMethods.GetDeviceCaps(new HandleRef(null, dC), 90);
							_dpiInitialized = true;
						}
						finally
						{
							MS.Win32.UnsafeNativeMethods.ReleaseDC(hWnd, new HandleRef(null, dC));
						}
					}
				}
			}
			return _dpi;
		}
	}

	internal static Uri WindowsFontsUriObject => _windowsFontsUriObject;

	internal static string WindowsFontsUriString => _windowsFontsUriString;

	static Util()
	{
		SupportedExtensions = new string[5] { ".COMPOSITEFONT", ".OTF", ".TTC", ".TTF", ".TTE" };
		InvalidFileNameChars = Path.GetInvalidFileNameChars();
		_dpiLock = new object();
		_dpiInitialized = false;
		_windowsFontsLocalPath = (Environment.GetEnvironmentVariable("windir") + "\\Fonts\\").ToUpperInvariant();
		_windowsFontsUriObject = new Uri(_windowsFontsLocalPath, UriKind.Absolute);
		_windowsFontsUriString = _windowsFontsUriObject.GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
	}

	internal static bool IsReferenceToWindowsFonts(string s)
	{
		if (string.IsNullOrEmpty(s) || s[0] == '#')
		{
			return true;
		}
		int num = s.IndexOf('#');
		if (num < 0)
		{
			num = s.Length;
		}
		if (s.IndexOfAny(InvalidFileNameChars, 0, num) >= 0)
		{
			return false;
		}
		for (int index = s.IndexOf('%', 0, num); index >= 0; index = s.IndexOf('%', index, num - index))
		{
			char value = Uri.HexUnescape(s, ref index);
			if (Array.IndexOf(InvalidFileNameChars, value) >= 0)
			{
				return false;
			}
		}
		if (s[0] == '.')
		{
			int i;
			for (i = 1; i < num && s[i] == '.'; i++)
			{
			}
			for (; i < num && char.IsWhiteSpace(s[i]); i++)
			{
			}
			if (i == num)
			{
				return false;
			}
		}
		return true;
	}

	internal static bool IsSupportedSchemeForAbsoluteFontFamilyUri(Uri absoluteUri)
	{
		return absoluteUri.IsFile;
	}

	internal static void SplitFontFaceIndex(Uri fontUri, out Uri fontSourceUri, out int faceIndex)
	{
		string components = fontUri.GetComponents(UriComponents.Fragment, UriFormat.SafeUnescaped);
		if (!string.IsNullOrEmpty(components))
		{
			if (!int.TryParse(components, NumberStyles.None, CultureInfo.InvariantCulture, out faceIndex))
			{
				throw new ArgumentException(SR.FaceIndexMustBePositiveOrZero, "fontUri");
			}
			fontSourceUri = new Uri(fontUri.GetComponents(UriComponents.HttpRequestUrl | UriComponents.UserInfo, UriFormat.SafeUnescaped));
		}
		else
		{
			faceIndex = 0;
			fontSourceUri = fontUri;
		}
	}

	internal static Uri CombineUriWithFaceIndex(string fontUri, int faceIndex)
	{
		if (faceIndex == 0)
		{
			return new Uri(fontUri);
		}
		string components = new Uri(fontUri).GetComponents(UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
		string text = faceIndex.ToString(CultureInfo.InvariantCulture);
		return new Uri(components + "#" + text);
	}

	internal static bool IsSupportedFontExtension(string extension, out bool isComposite)
	{
		for (int i = 0; i < SupportedExtensions.Length; i++)
		{
			string b = SupportedExtensions[i];
			if (string.Equals(extension, b, StringComparison.OrdinalIgnoreCase))
			{
				isComposite = i == 0;
				return true;
			}
		}
		isComposite = false;
		return false;
	}

	internal static bool IsCompositeFont(string extension)
	{
		return string.Equals(extension, CompositeFontExtension, StringComparison.OrdinalIgnoreCase);
	}

	internal static bool IsEnumerableFontUriScheme(Uri fontLocation)
	{
		bool result = false;
		if (fontLocation.IsAbsoluteUri)
		{
			Uri result2;
			if (fontLocation.IsFile)
			{
				result = true;
			}
			else if (fontLocation.Scheme == PackUriHelper.UriSchemePack && Uri.TryCreate(fontLocation, "X", out result2))
			{
				result = BaseUriHelper.IsPackApplicationUri(result2);
			}
		}
		return result;
	}

	internal static bool IsAppSpecificUri(Uri fontLocation)
	{
		if (fontLocation.IsAbsoluteUri && fontLocation.IsFile)
		{
			return fontLocation.IsUnc;
		}
		return true;
	}

	internal static string GetUriExtension(Uri uri)
	{
		return Path.GetExtension(uri.GetComponents(UriComponents.Path, UriFormat.Unescaped));
	}

	internal static string GetNormalizedFontFamilyReference(string friendlyName, int startIndex, int length)
	{
		if (friendlyName.IndexOf(',', startIndex, length) < 0)
		{
			return NormalizeFontFamilyReference(friendlyName, startIndex, length);
		}
		return NormalizeFontFamilyReference(friendlyName.Substring(startIndex, length).Replace(",,", ","));
	}

	private static string NormalizeFontFamilyReference(string fontFamilyReference)
	{
		return NormalizeFontFamilyReference(fontFamilyReference, 0, fontFamilyReference.Length);
	}

	private static string NormalizeFontFamilyReference(string fontFamilyReference, int startIndex, int length)
	{
		if (length == 0)
		{
			return "#";
		}
		int num = fontFamilyReference.IndexOf('#', startIndex, length);
		if (num < 0)
		{
			return string.Concat("#", fontFamilyReference.AsSpan(startIndex, length)).ToUpperInvariant();
		}
		if (num + 1 == startIndex + length)
		{
			return "#";
		}
		if (num == startIndex)
		{
			return fontFamilyReference.Substring(startIndex, length).ToUpperInvariant();
		}
		string text = fontFamilyReference.Substring(startIndex, num - startIndex);
		string text2 = fontFamilyReference.Substring(num, startIndex + length - num);
		return text + text2.ToUpperInvariant();
	}

	internal static string ConvertFamilyNameAndLocationToFontFamilyReference(string familyName, string location)
	{
		string text = familyName.Replace("%", "%25").Replace("#", "%23");
		if (!string.IsNullOrEmpty(location))
		{
			text = location + "#" + text;
		}
		return text;
	}

	internal static string ConvertFontFamilyReferenceToFriendlyName(string fontFamilyReference)
	{
		return fontFamilyReference.Replace(",", ",,");
	}

	internal static int CompareOrdinalIgnoreCase(string a, string b)
	{
		int length = a.Length;
		int length2 = b.Length;
		int num = Math.Min(length, length2);
		for (int i = 0; i < num; i++)
		{
			int num2 = CompareOrdinalIgnoreCase(a[i], b[i]);
			if (num2 != 0)
			{
				return num2;
			}
		}
		return length - length2;
	}

	private static int CompareOrdinalIgnoreCase(char a, char b)
	{
		char num = char.ToUpperInvariant(a);
		char c = char.ToUpperInvariant(b);
		return num - c;
	}

	internal static void ThrowWin32Exception(int errorCode, string fileName)
	{
		switch (errorCode)
		{
		case 2:
			throw new FileNotFoundException(SR.Format(SR.FileNotFoundExceptionWithFileName, fileName), fileName);
		case 3:
			throw new DirectoryNotFoundException(SR.Format(SR.DirectoryNotFoundExceptionWithFileName, fileName));
		case 5:
			throw new UnauthorizedAccessException(SR.Format(SR.UnauthorizedAccessExceptionWithFileName, fileName));
		case 206:
			throw new PathTooLongException(SR.Format(SR.PathTooLongExceptionWithFileName, fileName));
		default:
			throw new IOException(SR.Format(SR.IOExceptionWithFileName, fileName), MS.Win32.NativeMethods.MakeHRFromErrorCode(errorCode));
		}
	}

	internal static Exception ConvertInPageException(FontSource fontSource, SEHException e)
	{
		return new IOException(SR.Format(p1: (!fontSource.IsFile) ? fontSource.GetUriString() : fontSource.Uri.LocalPath, resourceFormat: SR.IOExceptionWithFileName), e);
	}
}
