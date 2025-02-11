using System.ComponentModel;
using MS.Internal.WindowsRuntime.Windows.Data.Text;

namespace MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;

[EditorBrowsable(EditorBrowsableState.Never)]
internal static class IUnicodeCharactersStatics_Delegates
{
	public delegate int GetCodepointFromSurrogatePair_0(nint thisPtr, uint highSurrogate, uint lowSurrogate, out uint codepoint);

	public delegate int GetSurrogatePairFromCodepoint_1(nint thisPtr, uint codepoint, out ushort highSurrogate, out ushort lowSurrogate);

	public delegate int IsHighSurrogate_2(nint thisPtr, uint codepoint, out byte value);

	public delegate int IsLowSurrogate_3(nint thisPtr, uint codepoint, out byte value);

	public delegate int IsSupplementary_4(nint thisPtr, uint codepoint, out byte value);

	public delegate int IsNoncharacter_5(nint thisPtr, uint codepoint, out byte value);

	public delegate int IsWhitespace_6(nint thisPtr, uint codepoint, out byte value);

	public delegate int IsAlphabetic_7(nint thisPtr, uint codepoint, out byte value);

	public delegate int IsCased_8(nint thisPtr, uint codepoint, out byte value);

	public delegate int IsUppercase_9(nint thisPtr, uint codepoint, out byte value);

	public delegate int IsLowercase_10(nint thisPtr, uint codepoint, out byte value);

	public delegate int IsIdStart_11(nint thisPtr, uint codepoint, out byte value);

	public delegate int IsIdContinue_12(nint thisPtr, uint codepoint, out byte value);

	public delegate int IsGraphemeBase_13(nint thisPtr, uint codepoint, out byte value);

	public delegate int IsGraphemeExtend_14(nint thisPtr, uint codepoint, out byte value);

	public delegate int GetNumericType_15(nint thisPtr, uint codepoint, out UnicodeNumericType value);

	public delegate int GetGeneralCategory_16(nint thisPtr, uint codepoint, out UnicodeGeneralCategory value);
}
