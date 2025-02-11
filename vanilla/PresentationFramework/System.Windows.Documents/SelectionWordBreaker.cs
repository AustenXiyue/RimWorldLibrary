using MS.Internal;
using MS.Win32;

namespace System.Windows.Documents;

internal static class SelectionWordBreaker
{
	[Flags]
	private enum CharClass : byte
	{
		Alphanumeric = 0,
		Punctuation = 1,
		Blank = 2,
		WhiteSpace = 4,
		WBF_CLASS = 0xF,
		WBF_ISWHITE = 0x10,
		WBF_BREAKAFTER = 0x40
	}

	private const char LineFeedChar = '\n';

	private const char CarriageReturnChar = '\r';

	private const char QuotationMarkChar = '"';

	private const char ApostropheChar = '\'';

	private const char SoftHyphenChar = '\u00ad';

	private const char RightSingleQuotationChar = '’';

	private const char ObjectReplacementChar = '￼';

	private static readonly byte[] _latinClasses = new byte[256]
	{
		0, 0, 0, 0, 0, 0, 0, 20, 0, 19,
		20, 20, 20, 20, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 50, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 65, 1, 1, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
		1, 1, 1, 1, 1, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 1, 1, 1, 1, 1, 1, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 1, 1, 1, 1, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		18, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
		1, 1, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0
	};

	internal static int MinContextLength => 2;

	internal static bool IsAtWordBoundary(char[] text, int position, LogicalDirection insideWordDirection)
	{
		CharClass[] classes = GetClasses(text);
		if (insideWordDirection == LogicalDirection.Backward)
		{
			if (position == text.Length)
			{
				return true;
			}
			if (position == 0 || IsWhiteSpace(text[position - 1], classes[position - 1]))
			{
				return false;
			}
		}
		else
		{
			if (position == 0)
			{
				return true;
			}
			if (position == text.Length || IsWhiteSpace(text[position], classes[position]))
			{
				return false;
			}
		}
		ushort[] array = new ushort[2];
		SafeNativeMethods.GetStringTypeEx(0u, 4u, new char[2]
		{
			text[position - 1],
			text[position]
		}, 2, array);
		if (!IsWordBoundary(text[position - 1], text[position]))
		{
			if (!IsSameClass(array[0], classes[position - 1], array[1], classes[position]) && !IsMidLetter(text, position - 1, classes))
			{
				return !IsMidLetter(text, position, classes);
			}
			return false;
		}
		return true;
	}

	private static bool IsWordBoundary(char previousChar, char followingChar)
	{
		bool result = false;
		if (followingChar == '\r')
		{
			result = true;
		}
		return result;
	}

	private static bool IsMidLetter(char[] text, int index, CharClass[] classes)
	{
		Invariant.Assert(text.Length == classes.Length);
		if ((text[index] == '\'' || text[index] == '’' || text[index] == '\u00ad') && index > 0 && index + 1 < classes.Length)
		{
			if (classes[index - 1] != 0 || classes[index + 1] != 0)
			{
				if (text[index] == '"' && IsHebrew(text[index - 1]))
				{
					return IsHebrew(text[index + 1]);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private static bool IsIdeographicCharType(ushort charType3)
	{
		return (charType3 & 0x130) != 0;
	}

	private static bool IsSameClass(ushort preceedingType3, CharClass preceedingClass, ushort followingType3, CharClass followingClass)
	{
		bool result = false;
		if (IsIdeographicCharType(preceedingType3) && IsIdeographicCharType(followingType3))
		{
			ushort num = (ushort)((preceedingType3 & 0x1F0) ^ (followingType3 & 0x1F0));
			result = (preceedingType3 & 0xF0) != 0 && (num == 0 || num == 128 || num == 32 || num == 160);
		}
		else if (!IsIdeographicCharType(preceedingType3) && !IsIdeographicCharType(followingType3))
		{
			result = (preceedingClass & CharClass.WBF_CLASS) == (followingClass & CharClass.WBF_CLASS);
		}
		return result;
	}

	private static bool IsWhiteSpace(char ch, CharClass charClass)
	{
		if ((charClass & CharClass.WBF_CLASS) == CharClass.Blank)
		{
			return ch != '￼';
		}
		return false;
	}

	private static CharClass[] GetClasses(char[] text)
	{
		CharClass[] array = new CharClass[text.Length];
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			CharClass charClass;
			if (c < 'Ā')
			{
				charClass = (CharClass)_latinClasses[(uint)c];
			}
			else if (IsKorean(c))
			{
				charClass = CharClass.Alphanumeric;
			}
			else if (IsThai(c))
			{
				charClass = CharClass.Alphanumeric;
			}
			else if (c == '￼')
			{
				charClass = CharClass.Blank | CharClass.WBF_BREAKAFTER;
			}
			else
			{
				ushort[] array2 = new ushort[1];
				SafeNativeMethods.GetStringTypeEx(0u, 1u, new char[1] { c }, 1, array2);
				charClass = (((array2[0] & 8) == 0) ? (((array2[0] & 0x10) != 0 && !IsDiacriticOrKashida(c)) ? CharClass.Punctuation : CharClass.Alphanumeric) : (((array2[0] & 0x40) == 0) ? (CharClass.WhiteSpace | CharClass.WBF_ISWHITE) : (CharClass.Blank | CharClass.WBF_ISWHITE)));
			}
			array[i] = charClass;
		}
		return array;
	}

	private static bool IsDiacriticOrKashida(char ch)
	{
		ushort[] array = new ushort[1];
		SafeNativeMethods.GetStringTypeEx(0u, 4u, new char[1] { ch }, 1, array);
		return (array[0] & 0x207) != 0;
	}

	private static bool IsInRange(uint lower, char ch, uint upper)
	{
		if (lower <= ch)
		{
			return ch <= upper;
		}
		return false;
	}

	private static bool IsKorean(char ch)
	{
		return IsInRange(44032u, ch, 55295u);
	}

	private static bool IsThai(char ch)
	{
		return IsInRange(3584u, ch, 3711u);
	}

	private static bool IsHebrew(char ch)
	{
		return IsInRange(1488u, ch, 1522u);
	}
}
