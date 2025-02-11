using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace MS.Internal.FontCache;

internal class LocalizedName
{
	private class NameComparerClass : IComparer<LocalizedName>
	{
		int IComparer<LocalizedName>.Compare(LocalizedName x, LocalizedName y)
		{
			return Util.CompareOrdinalIgnoreCase(x._name, y._name);
		}
	}

	private class LanguageComparerClass : IComparer<LocalizedName>
	{
		int IComparer<LocalizedName>.Compare(LocalizedName x, LocalizedName y)
		{
			return string.Compare(x._language.IetfLanguageTag, y._language.IetfLanguageTag, StringComparison.OrdinalIgnoreCase);
		}
	}

	private XmlLanguage _language;

	private string _name;

	private int _originalLCID;

	private static NameComparerClass _nameComparer = new NameComparerClass();

	private static LanguageComparerClass _languageComparer = new LanguageComparerClass();

	internal XmlLanguage Language => _language;

	internal string Name => _name;

	internal int OriginalLCID => _originalLCID;

	internal static IComparer<LocalizedName> NameComparer => _nameComparer;

	internal static IComparer<LocalizedName> LanguageComparer => _languageComparer;

	internal LocalizedName(XmlLanguage language, string name)
		: this(language, name, language.GetEquivalentCulture().LCID)
	{
	}

	internal LocalizedName(XmlLanguage language, string name, int originalLCID)
	{
		_language = language;
		_name = name;
		_originalLCID = originalLCID;
	}
}
