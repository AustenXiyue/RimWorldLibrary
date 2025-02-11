using System.Collections.Generic;
using System.Windows.Documents.MsSpellCheckLib;
using MS.Internal.WindowsRuntime.Windows.Data.Text;

namespace System.Windows.Documents;

internal static class WinRTSpellerInteropExtensions
{
	public static IReadOnlyList<WinRTSpellerInterop.SpellerSegment> ComprehensiveGetTokens(this WordsSegmenter segmenter, string text, SpellChecker spellChecker, WinRTSpellerInterop owner)
	{
		IReadOnlyList<WordSegment> readOnlyList = segmenter?.GetTokens(text) ?? Array.Empty<WordSegment>();
		if (readOnlyList.Count == 0)
		{
			return Array.Empty<WinRTSpellerInterop.SpellerSegment>();
		}
		List<WinRTSpellerInterop.SpellerSegment> list = new List<WinRTSpellerInterop.SpellerSegment>();
		int num = 0;
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			int startPosition = (int)readOnlyList[i].SourceTextSegment.StartPosition;
			int length = (int)readOnlyList[i].SourceTextSegment.Length;
			if (spellChecker != null && startPosition > num)
			{
				WinRTSpellerInterop.SpellerSegment missingFragment = new WinRTSpellerInterop.SpellerSegment(text, new WinRTSpellerInterop.TextRange(num, startPosition - num), spellChecker, owner);
				if (list.Count > 0)
				{
					WinRTSpellerInterop.TextRange? spellCheckCleanSubstitutionToken = GetSpellCheckCleanSubstitutionToken(spellChecker, text, list[list.Count - 1], missingFragment);
					if (spellCheckCleanSubstitutionToken.HasValue)
					{
						list[list.Count - 1] = new WinRTSpellerInterop.SpellerSegment(text, spellCheckCleanSubstitutionToken.Value, spellChecker, owner);
					}
				}
			}
			list.Add(new WinRTSpellerInterop.SpellerSegment(text, new WinRTSpellerInterop.TextRange(startPosition, length), spellChecker, owner));
			num = startPosition + length;
		}
		if (readOnlyList.Count > 0 && spellChecker != null && spellChecker.HasErrors(readOnlyList[readOnlyList.Count - 1].Text) && num < text.Length)
		{
			WinRTSpellerInterop.SpellerSegment missingFragment2 = new WinRTSpellerInterop.SpellerSegment(text, new WinRTSpellerInterop.TextRange(num, text.Length - num), spellChecker, owner);
			if (list.Count > 0)
			{
				WinRTSpellerInterop.TextRange? spellCheckCleanSubstitutionToken2 = GetSpellCheckCleanSubstitutionToken(spellChecker, text, list[list.Count - 1], missingFragment2);
				if (spellCheckCleanSubstitutionToken2.HasValue)
				{
					list[list.Count - 1] = new WinRTSpellerInterop.SpellerSegment(text, spellCheckCleanSubstitutionToken2.Value, spellChecker, owner);
				}
			}
		}
		return list.AsReadOnly();
	}

	private static WinRTSpellerInterop.TextRange? GetSpellCheckCleanSubstitutionToken(SpellChecker spellChecker, string documentText, WinRTSpellerInterop.SpellerSegment lastToken, WinRTSpellerInterop.SpellerSegment missingFragment)
	{
		string text = lastToken?.Text;
		string text2 = missingFragment?.Text.TrimEnd('\0');
		if (string.IsNullOrWhiteSpace(text2) || string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(documentText) || spellChecker == null || !spellChecker.HasErrors(text))
		{
			return null;
		}
		string text3 = text;
		int num = Math.Min(text2.Length, 4);
		for (int i = 1; i <= num; i++)
		{
			string text4 = documentText.Substring(lastToken.TextRange.Start, text.Length + i).TrimEnd('\0').TrimEnd();
			if (text4.Length > text3.Length)
			{
				text3 = text4;
				if (!spellChecker.HasErrors(text4))
				{
					return new WinRTSpellerInterop.TextRange(lastToken.TextRange.Start, text4.Length);
				}
			}
		}
		return null;
	}
}
