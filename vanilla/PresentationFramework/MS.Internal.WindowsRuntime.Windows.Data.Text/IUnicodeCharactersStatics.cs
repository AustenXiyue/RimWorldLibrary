using System.Runtime.InteropServices;
using WinRT;

namespace MS.Internal.WindowsRuntime.Windows.Data.Text;

[WindowsRuntimeType]
[Guid("97909E87-9291-4F91-B6C8-B6E359D7A7FB")]
internal interface IUnicodeCharactersStatics
{
	uint GetCodepointFromSurrogatePair(uint highSurrogate, uint lowSurrogate);

	void GetSurrogatePairFromCodepoint(uint codepoint, out char highSurrogate, out char lowSurrogate);

	bool IsHighSurrogate(uint codepoint);

	bool IsLowSurrogate(uint codepoint);

	bool IsSupplementary(uint codepoint);

	bool IsNoncharacter(uint codepoint);

	bool IsWhitespace(uint codepoint);

	bool IsAlphabetic(uint codepoint);

	bool IsCased(uint codepoint);

	bool IsUppercase(uint codepoint);

	bool IsLowercase(uint codepoint);

	bool IsIdStart(uint codepoint);

	bool IsIdContinue(uint codepoint);

	bool IsGraphemeBase(uint codepoint);

	bool IsGraphemeExtend(uint codepoint);

	UnicodeNumericType GetNumericType(uint codepoint);

	UnicodeGeneralCategory GetGeneralCategory(uint codepoint);
}
