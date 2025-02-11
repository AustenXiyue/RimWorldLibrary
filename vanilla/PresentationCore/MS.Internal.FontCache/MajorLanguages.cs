using System.Globalization;

namespace MS.Internal.FontCache;

internal static class MajorLanguages
{
	private struct MajorLanguageDesc
	{
		internal readonly CultureInfo Culture;

		internal readonly ScriptTags Script;

		internal readonly LanguageTags LangSys;

		internal MajorLanguageDesc(CultureInfo culture, ScriptTags script, LanguageTags langSys)
		{
			Culture = culture;
			Script = script;
			LangSys = langSys;
		}
	}

	private static readonly MajorLanguageDesc[] majorLanguages = new MajorLanguageDesc[4]
	{
		new MajorLanguageDesc(new CultureInfo("en"), ScriptTags.Latin, LanguageTags.English),
		new MajorLanguageDesc(new CultureInfo("de"), ScriptTags.Latin, LanguageTags.German),
		new MajorLanguageDesc(new CultureInfo("ja"), ScriptTags.CJKIdeographic, LanguageTags.Japanese),
		new MajorLanguageDesc(new CultureInfo("ja"), ScriptTags.Hiragana, LanguageTags.Japanese)
	};

	internal static bool Contains(ScriptTags script, LanguageTags langSys)
	{
		for (int i = 0; i < majorLanguages.Length; i++)
		{
			if (script == majorLanguages[i].Script && (langSys == LanguageTags.Default || langSys == majorLanguages[i].LangSys))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool Contains(CultureInfo culture)
	{
		if (culture == null)
		{
			return false;
		}
		if (culture == CultureInfo.InvariantCulture)
		{
			return true;
		}
		for (int i = 0; i < majorLanguages.Length; i++)
		{
			if (majorLanguages[i].Culture.Equals(culture) || majorLanguages[i].Culture.Equals(culture.Parent))
			{
				return true;
			}
		}
		return false;
	}
}
