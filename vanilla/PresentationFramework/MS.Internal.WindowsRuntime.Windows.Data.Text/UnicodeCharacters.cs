using MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Data.Text;

internal static class UnicodeCharacters
{
	internal class _IUnicodeCharactersStatics : MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IUnicodeCharactersStatics
	{
		private static WeakLazy<_IUnicodeCharactersStatics> _instance = new WeakLazy<_IUnicodeCharactersStatics>();

		public static IUnicodeCharactersStatics Instance => _instance.Value;

		public _IUnicodeCharactersStatics()
			: base(new BaseActivationFactory("Windows.Data.Text", "Windows.Data.Text.UnicodeCharacters")._As<Vftbl>())
		{
		}
	}

	public static uint GetCodepointFromSurrogatePair(uint highSurrogate, uint lowSurrogate)
	{
		return _IUnicodeCharactersStatics.Instance.GetCodepointFromSurrogatePair(highSurrogate, lowSurrogate);
	}

	public static void GetSurrogatePairFromCodepoint(uint codepoint, out char highSurrogate, out char lowSurrogate)
	{
		_IUnicodeCharactersStatics.Instance.GetSurrogatePairFromCodepoint(codepoint, out highSurrogate, out lowSurrogate);
	}

	public static bool IsHighSurrogate(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.IsHighSurrogate(codepoint);
	}

	public static bool IsLowSurrogate(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.IsLowSurrogate(codepoint);
	}

	public static bool IsSupplementary(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.IsSupplementary(codepoint);
	}

	public static bool IsNoncharacter(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.IsNoncharacter(codepoint);
	}

	public static bool IsWhitespace(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.IsWhitespace(codepoint);
	}

	public static bool IsAlphabetic(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.IsAlphabetic(codepoint);
	}

	public static bool IsCased(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.IsCased(codepoint);
	}

	public static bool IsUppercase(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.IsUppercase(codepoint);
	}

	public static bool IsLowercase(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.IsLowercase(codepoint);
	}

	public static bool IsIdStart(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.IsIdStart(codepoint);
	}

	public static bool IsIdContinue(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.IsIdContinue(codepoint);
	}

	public static bool IsGraphemeBase(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.IsGraphemeBase(codepoint);
	}

	public static bool IsGraphemeExtend(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.IsGraphemeExtend(codepoint);
	}

	public static UnicodeNumericType GetNumericType(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.GetNumericType(codepoint);
	}

	public static UnicodeGeneralCategory GetGeneralCategory(uint codepoint)
	{
		return _IUnicodeCharactersStatics.Instance.GetGeneralCategory(codepoint);
	}
}
