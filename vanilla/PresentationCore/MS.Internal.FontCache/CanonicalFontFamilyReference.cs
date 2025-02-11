using System;

namespace MS.Internal.FontCache;

internal sealed class CanonicalFontFamilyReference
{
	private Uri _absoluteLocationUri;

	private string _familyName;

	private static readonly CanonicalFontFamilyReference _unresolved = new CanonicalFontFamilyReference((Uri)null, string.Empty);

	public static CanonicalFontFamilyReference Unresolved => _unresolved;

	public string FamilyName => _familyName;

	public string EscapedFileName { get; private set; }

	public Uri LocationUri => _absoluteLocationUri;

	public static CanonicalFontFamilyReference Create(Uri baseUri, string normalizedString)
	{
		if (SplitFontFamilyReference(normalizedString, out var locationString, out var escapedFamilyName))
		{
			Uri result = null;
			string text = null;
			bool flag = false;
			if (locationString == null || Util.IsReferenceToWindowsFonts(locationString))
			{
				text = locationString;
				flag = true;
			}
			else if (Uri.TryCreate(locationString, UriKind.Absolute, out result))
			{
				flag = Util.IsSupportedSchemeForAbsoluteFontFamilyUri(result);
			}
			else if (baseUri != null && Util.IsEnumerableFontUriScheme(baseUri))
			{
				flag = Uri.TryCreate(baseUri, locationString, out result);
			}
			if (flag)
			{
				string familyName = Uri.UnescapeDataString(escapedFamilyName);
				if (text != null)
				{
					return new CanonicalFontFamilyReference(text, familyName);
				}
				return new CanonicalFontFamilyReference(result, familyName);
			}
		}
		return _unresolved;
	}

	public bool Equals(CanonicalFontFamilyReference other)
	{
		if (other != null && other._absoluteLocationUri == _absoluteLocationUri && other.EscapedFileName == EscapedFileName)
		{
			return other._familyName == _familyName;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as CanonicalFontFamilyReference);
	}

	public override int GetHashCode()
	{
		if (_absoluteLocationUri == null && EscapedFileName == null)
		{
			return _familyName.GetHashCode();
		}
		return HashFn.HashScramble(HashFn.HashMultiply((_absoluteLocationUri != null) ? _absoluteLocationUri.GetHashCode() : EscapedFileName.GetHashCode()) + _familyName.GetHashCode());
	}

	private CanonicalFontFamilyReference(string escapedFileName, string familyName)
	{
		EscapedFileName = escapedFileName;
		_familyName = familyName;
	}

	private CanonicalFontFamilyReference(Uri absoluteLocationUri, string familyName)
	{
		_absoluteLocationUri = absoluteLocationUri;
		_familyName = familyName;
	}

	private static bool SplitFontFamilyReference(string normalizedString, out string locationString, out string escapedFamilyName)
	{
		int num;
		if (normalizedString[0] == '#')
		{
			locationString = null;
			num = 1;
		}
		else
		{
			int num2 = normalizedString.IndexOf('#');
			locationString = normalizedString.Substring(0, num2);
			num = num2 + 1;
		}
		if (num < normalizedString.Length)
		{
			escapedFamilyName = normalizedString.Substring(num);
			return true;
		}
		escapedFamilyName = null;
		return false;
	}
}
