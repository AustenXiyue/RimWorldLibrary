using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;

namespace System.Windows.Documents;

internal abstract class SpellerInteropBase : IDisposable
{
	internal delegate bool EnumSentencesCallback(ISpellerSentence sentence, object data);

	internal delegate bool EnumTextSegmentsCallback(ISpellerSegment textSegment, object data);

	internal interface ITextRange
	{
		int Start { get; }

		int Length { get; }
	}

	internal interface ISpellerSegment
	{
		string SourceString { get; }

		IReadOnlyList<ISpellerSegment> SubSegments { get; }

		ITextRange TextRange { get; }

		string Text { get; }

		IReadOnlyList<string> Suggestions { get; }

		bool IsClean { get; }

		void EnumSubSegments(EnumTextSegmentsCallback segmentCallback, object data);
	}

	internal interface ISpellerSentence
	{
		IReadOnlyList<ISpellerSegment> Segments { get; }

		int EndOffset { get; }
	}

	[Flags]
	internal enum SpellerMode
	{
		None = 0,
		WordBreaking = 1,
		SpellingErrors = 2,
		Suggestions = 4,
		SpellingErrorsWithSuggestions = 6,
		All = 7
	}

	internal abstract SpellerMode Mode { set; }

	internal abstract bool MultiWordMode { set; }

	public abstract void Dispose();

	protected abstract void Dispose(bool disposing);

	public static SpellerInteropBase CreateInstance()
	{
		SpellerInteropBase result = null;
		bool flag = false;
		try
		{
			result = new WinRTSpellerInterop();
			flag = true;
		}
		catch (PlatformNotSupportedException)
		{
			flag = false;
		}
		catch (NotSupportedException)
		{
			flag = true;
		}
		if (!flag)
		{
			try
			{
				result = new NLGSpellerInterop();
			}
			catch (Exception ex3) when (ex3 is DllNotFoundException || ex3 is EntryPointNotFoundException)
			{
				return null;
			}
		}
		return result;
	}

	internal abstract void SetLocale(CultureInfo culture);

	internal abstract int EnumTextSegments(char[] text, int count, EnumSentencesCallback sentenceCallback, EnumTextSegmentsCallback segmentCallback, object data);

	internal abstract void UnloadDictionary(object dictionary);

	internal abstract object LoadDictionary(string lexiconFilePath);

	internal abstract object LoadDictionary(Uri item, string trustedFolder);

	internal abstract void ReleaseAllLexicons();

	internal abstract void SetReformMode(CultureInfo culture, SpellingReform spellingReform);

	internal abstract bool CanSpellCheck(CultureInfo culture);
}
