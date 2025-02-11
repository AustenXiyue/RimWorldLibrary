using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Documents;

internal static class TextFindEngine
{
	private const int TextBlockLength = 64;

	private const char UnicodeBidiStart = '\u0590';

	private const char UnicodeBidiEnd = '\u07bf';

	private const char UnicodeArabicKashida = 'ـ';

	private const char UnicodeArabicAlefMaddaAbove = 'آ';

	private const char UnicodeArabicAlefHamzaAbove = 'أ';

	private const char UnicodeArabicAlefHamzaBelow = 'إ';

	private const char UnicodeArabicAlef = 'ا';

	public static ITextRange Find(ITextPointer findContainerStartPosition, ITextPointer findContainerEndPosition, string findPattern, FindFlags flags, CultureInfo cultureInfo)
	{
		if (findContainerStartPosition == null || findContainerEndPosition == null || findContainerStartPosition.CompareTo(findContainerEndPosition) == 0 || findPattern == null || findPattern == string.Empty)
		{
			return null;
		}
		bool matchCase = (flags & FindFlags.MatchCase) != 0;
		bool flag = (flags & FindFlags.FindWholeWordsOnly) != 0;
		bool matchLast = (flags & FindFlags.FindInReverse) != 0;
		bool matchDiacritics = (flags & FindFlags.MatchDiacritics) != 0;
		bool matchKashida = (flags & FindFlags.MatchKashida) != 0;
		bool matchAlefHamza = (flags & FindFlags.MatchAlefHamza) != 0;
		if (flag)
		{
			ushort[] array = new ushort[1];
			ushort[] array2 = new ushort[1];
			char[] array3 = findPattern.ToCharArray();
			SafeNativeMethods.GetStringTypeEx(0u, 1u, new char[1] { array3[0] }, 1, array);
			SafeNativeMethods.GetStringTypeEx(0u, 1u, new char[1] { array3[findPattern.Length - 1] }, 1, array2);
			if ((array[0] & 8) != 0 || (array[0] & 0x40) != 0 || (array2[0] & 8) != 0 || (array2[0] & 0x40) != 0)
			{
				flag = false;
			}
		}
		if (findContainerStartPosition is DocumentSequenceTextPointer || findContainerStartPosition is FixedTextPointer)
		{
			return FixedFindEngine.Find(findContainerStartPosition, findContainerEndPosition, findPattern, cultureInfo, matchCase, flag, matchLast, matchDiacritics, matchKashida, matchAlefHamza);
		}
		return InternalFind(findContainerStartPosition, findContainerEndPosition, findPattern, cultureInfo, matchCase, flag, matchLast, matchDiacritics, matchKashida, matchAlefHamza);
	}

	internal static TextRange InternalFind(ITextPointer startPosition, ITextPointer endPosition, string findPattern, CultureInfo cultureInfo, bool matchCase, bool matchWholeWord, bool matchLast, bool matchDiacritics, bool matchKashida, bool matchAlefHamza)
	{
		Invariant.Assert(startPosition.CompareTo(endPosition) <= 0);
		LogicalDirection direction;
		ITextPointer textPointer;
		if (matchLast)
		{
			textPointer = endPosition;
			direction = LogicalDirection.Backward;
		}
		else
		{
			textPointer = startPosition;
			direction = LogicalDirection.Forward;
		}
		int num = Math.Max(64, findPattern.Length * 2 * 2);
		textPointer = textPointer.CreatePointer();
		while ((matchLast ? startPosition.CompareTo(textPointer) : textPointer.CompareTo(endPosition)) < 0)
		{
			ITextPointer textPointer2 = textPointer.CreatePointer();
			char[] array = new char[num];
			int[] array2 = new int[num + 1];
			int num2 = SetFindTextAndFindTextPositionMap(startPosition, endPosition, textPointer, direction, matchLast, array, array2);
			if (!matchDiacritics || num2 >= findPattern.Length)
			{
				int num3 = (matchLast ? (array.Length - num2) : 0);
				bool hasPreceedingSeparatorChar = false;
				bool hasFollowingSeparatorChar = false;
				if (matchWholeWord)
				{
					GetContextualInformation(textPointer2, matchLast ? (-array2[array2.Length - num2 - 1]) : array2[num2], out hasPreceedingSeparatorChar, out hasFollowingSeparatorChar);
				}
				int matchLength;
				int num4 = FindMatchIndexFromFindContent(new string(array, num3, num2), findPattern, cultureInfo, matchCase, matchWholeWord, matchLast, matchDiacritics, matchKashida, matchAlefHamza, hasPreceedingSeparatorChar, hasFollowingSeparatorChar, out matchLength);
				if (num4 != -1)
				{
					ITextPointer textPointer3 = textPointer2.CreatePointer();
					textPointer3.MoveByOffset(matchLast ? (-array2[num3 + num4]) : array2[num4]);
					ITextPointer textPointer4 = textPointer2.CreatePointer();
					textPointer4.MoveByOffset(matchLast ? (-array2[num3 + num4 + matchLength]) : array2[num4 + matchLength]);
					return new TextRange(textPointer3, textPointer4);
				}
				if (num2 > findPattern.Length)
				{
					textPointer = textPointer2.CreatePointer();
					textPointer.MoveByOffset(matchLast ? (-array2[array.Length - num2 + findPattern.Length]) : array2[num2 - findPattern.Length]);
				}
			}
		}
		return null;
	}

	private static void GetContextualInformation(ITextPointer position, int oppositeEndOffset, out bool hasPreceedingSeparatorChar, out bool hasFollowingSeparatorChar)
	{
		ITextPointer position2 = position.CreatePointer(oppositeEndOffset, position.LogicalDirection);
		if (oppositeEndOffset < 0)
		{
			hasPreceedingSeparatorChar = HasNeighboringSeparatorChar(position2, LogicalDirection.Backward);
			hasFollowingSeparatorChar = HasNeighboringSeparatorChar(position, LogicalDirection.Forward);
		}
		else
		{
			hasPreceedingSeparatorChar = HasNeighboringSeparatorChar(position, LogicalDirection.Backward);
			hasFollowingSeparatorChar = HasNeighboringSeparatorChar(position2, LogicalDirection.Forward);
		}
	}

	private static bool HasNeighboringSeparatorChar(ITextPointer position, LogicalDirection direction)
	{
		ITextPointer textPointer = position.GetNextInsertionPosition(direction);
		if (textPointer == null)
		{
			return true;
		}
		if (position.CompareTo(textPointer) > 0)
		{
			ITextPointer textPointer2 = position;
			position = textPointer;
			textPointer = textPointer2;
		}
		int offsetToPosition = position.GetOffsetToPosition(textPointer);
		char[] array = new char[offsetToPosition];
		int[] findTextPositionMap = new int[offsetToPosition + 1];
		int num = SetFindTextAndFindTextPositionMap(position, textPointer, position.CreatePointer(), LogicalDirection.Forward, matchLast: false, array, findTextPositionMap);
		if (num == 0)
		{
			return true;
		}
		if (direction == LogicalDirection.Forward)
		{
			return IsSeparatorChar(array[0]);
		}
		return IsSeparatorChar(array[num - 1]);
	}

	private static int FindMatchIndexFromFindContent(string textString, string findPattern, CultureInfo cultureInfo, bool matchCase, bool matchWholeWord, bool matchLast, bool matchDiacritics, bool matchKashida, bool matchAlefHamza, bool hasPreceedingSeparatorChar, bool hasFollowingSeparatorChar, out int matchLength)
	{
		InitializeBidiFlags(findPattern, out var stringContainedBidiCharacter, out var _);
		CompareInfo compareInfo = cultureInfo.CompareInfo;
		if (!matchDiacritics && stringContainedBidiCharacter)
		{
			return BidiIgnoreDiacriticsMatchIndexCalculation(textString, findPattern, matchKashida, matchAlefHamza, matchWholeWord, matchLast, !matchCase, compareInfo, hasPreceedingSeparatorChar, hasFollowingSeparatorChar, out matchLength);
		}
		return StandardMatchIndexCalculation(textString, findPattern, matchWholeWord, matchLast, !matchCase, compareInfo, hasPreceedingSeparatorChar, hasFollowingSeparatorChar, out matchLength);
	}

	private static int StandardMatchIndexCalculation(string textString, string findPattern, bool matchWholeWord, bool matchLast, bool ignoreCase, CompareInfo compareInfo, bool hasPreceedingSeparatorChar, bool hasFollowingSeparatorChar, out int matchLength)
	{
		CompareOptions options = (ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None);
		int num = -1;
		int num2 = 0;
		int num3 = textString.Length;
		matchLength = 0;
		while (num3 > 0)
		{
			num = (matchLast ? compareInfo.LastIndexOf(textString, findPattern, num2 + num3 - 1, num3, options) : compareInfo.IndexOf(textString, findPattern, num2, num3, options));
			matchLength = findPattern.Length;
			if (num == -1 || !matchWholeWord || IsAtWordBoundary(textString, num, matchLength, hasPreceedingSeparatorChar, hasFollowingSeparatorChar))
			{
				break;
			}
			if (matchLast)
			{
				num2 = 0;
				num3 = num + matchLength - 1;
			}
			else
			{
				num2 = num + 1;
				num3 = textString.Length - num2;
			}
			num = -1;
		}
		return num;
	}

	private static int BidiIgnoreDiacriticsMatchIndexCalculation(string textString, string findPattern, bool matchKashida, bool matchAlefHamza, bool matchWholeWord, bool matchLast, bool ignoreCase, CompareInfo compareInfo, bool hasPreceedingSeparatorChar, bool hasFollowingSeparatorChar, out int matchLength)
	{
		int num = -1;
		int num2 = (matchLast ? (textString.Length - 1) : 0);
		int num3 = (matchLast ? (-1) : textString.Length);
		int num4 = ((!matchLast) ? 1 : (-1));
		if (Environment.OSVersion.Version.Major >= 6)
		{
			uint num5 = 2u;
			if (ignoreCase)
			{
				num5 |= 1;
			}
			if (matchLast)
			{
				num5 |= 0x800000;
			}
			if (matchKashida)
			{
				textString = textString.Replace('ـ', '0');
				findPattern = findPattern.Replace('ـ', '0');
			}
			if (matchAlefHamza)
			{
				textString = textString.Replace('آ', '0');
				textString = textString.Replace('أ', '1');
				textString = textString.Replace('إ', '2');
				findPattern = findPattern.Replace('آ', '0');
				findPattern = findPattern.Replace('أ', '1');
				findPattern = findPattern.Replace('إ', '2');
			}
			matchLength = 0;
			if (matchWholeWord)
			{
				int num6 = num2;
				while (num == -1 && num6 != num3)
				{
					for (int i = num6; i < textString.Length; i++)
					{
						string sourceString = textString.Substring(num6, i - num6 + 1);
						int num7 = FindNLSString(compareInfo.LCID, num5, sourceString, findPattern, out matchLength);
						if (num7 >= 0 && IsAtWordBoundary(textString, num6 + num7, matchLength, hasPreceedingSeparatorChar, hasFollowingSeparatorChar))
						{
							num = num6 + num7;
							break;
						}
					}
					num6 += num4;
				}
			}
			else
			{
				num = FindNLSString(compareInfo.LCID, num5, textString, findPattern, out matchLength);
			}
		}
		else
		{
			CompareOptions options = (CompareOptions)(2 | (ignoreCase ? 1 : 0));
			matchLength = 0;
			int num8 = num2;
			while (num == -1 && num8 != num3)
			{
				for (int j = num8; j < textString.Length; j++)
				{
					if (compareInfo.Compare(textString, num8, j - num8 + 1, findPattern, 0, findPattern.Length, options) == 0 && (!matchWholeWord || IsAtWordBoundary(textString, num8, j - num8 + 1, hasPreceedingSeparatorChar, hasFollowingSeparatorChar)) && (!matchKashida || IsKashidaMatch(textString.Substring(num8, j - num8 + 1), findPattern, compareInfo)) && (!matchAlefHamza || IsAlefHamzaMatch(textString.Substring(num8, j - num8 + 1), findPattern, compareInfo)))
					{
						num = num8;
						matchLength = j - num8 + 1;
						break;
					}
				}
				num8 += num4;
			}
		}
		return num;
	}

	private static int FindNLSString(int locale, uint flags, string sourceString, string findString, out int found)
	{
		int num = MS.Win32.UnsafeNativeMethods.FindNLSString(locale, flags, sourceString, sourceString.Length, findString, findString.Length, out found);
		if (num == -1)
		{
			int lastWin32Error = Marshal.GetLastWin32Error();
			if (lastWin32Error != 0)
			{
				throw new Win32Exception(lastWin32Error);
			}
		}
		return num;
	}

	private static bool IsKashidaMatch(string text, string pattern, CompareInfo compareInfo)
	{
		text = text.Replace('ـ', '0');
		pattern = pattern.Replace('ـ', '0');
		return compareInfo.Compare(text, pattern, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.StringSort) == 0;
	}

	private static bool IsAlefHamzaMatch(string text, string pattern, CompareInfo compareInfo)
	{
		text = text.Replace('آ', '0');
		text = text.Replace('أ', '1');
		text = text.Replace('إ', '2');
		pattern = pattern.Replace('آ', '0');
		pattern = pattern.Replace('أ', '1');
		pattern = pattern.Replace('إ', '2');
		return compareInfo.Compare(text, pattern, CompareOptions.IgnoreNonSpace | CompareOptions.IgnoreSymbols | CompareOptions.StringSort) == 0;
	}

	private static int SetFindTextAndFindTextPositionMap(ITextPointer startPosition, ITextPointer endPosition, ITextPointer navigator, LogicalDirection direction, bool matchLast, char[] findText, int[] findTextPositionMap)
	{
		Invariant.Assert(startPosition.CompareTo(navigator) <= 0);
		Invariant.Assert(endPosition.CompareTo(navigator) >= 0);
		int num = 0;
		int num2 = 0;
		if (matchLast && num2 == 0)
		{
			findTextPositionMap[^1] = 0;
		}
		while ((matchLast ? startPosition.CompareTo(navigator) : navigator.CompareTo(endPosition)) < 0)
		{
			switch (navigator.GetPointerContext(direction))
			{
			case TextPointerContext.Text:
			{
				int textRunLength = navigator.GetTextRunLength(direction);
				textRunLength = Math.Min(textRunLength, findText.Length - num2);
				if (!matchLast)
				{
					textRunLength = Math.Min(textRunLength, navigator.GetOffsetToPosition(endPosition));
					navigator.GetTextInRun(direction, findText, num2, textRunLength);
					for (int i = num2; i < num2 + textRunLength; i++)
					{
						findTextPositionMap[i] = i + num;
					}
				}
				else
				{
					textRunLength = Math.Min(textRunLength, startPosition.GetOffsetToPosition(navigator));
					navigator.GetTextInRun(direction, findText, findText.Length - num2 - textRunLength, textRunLength);
					int num3 = findText.Length - num2 - 1;
					for (int j = num2; j < num2 + textRunLength; j++)
					{
						findTextPositionMap[num3--] = j + num + 1;
					}
				}
				navigator.MoveByOffset(matchLast ? (-textRunLength) : textRunLength);
				num2 += textRunLength;
				break;
			}
			case TextPointerContext.None:
			case TextPointerContext.ElementStart:
			case TextPointerContext.ElementEnd:
				if (IsAdjacentToFormatElement(navigator, direction))
				{
					num++;
				}
				else if (!matchLast)
				{
					findText[num2] = '\n';
					findTextPositionMap[num2] = num2 + num;
					num2++;
				}
				else
				{
					num2++;
					findText[^num2] = '\n';
					findTextPositionMap[findText.Length - num2] = num2 + num;
				}
				navigator.MoveToNextContextPosition(direction);
				break;
			case TextPointerContext.EmbeddedElement:
				if (!matchLast)
				{
					findText[num2] = '\uf8ff';
					findTextPositionMap[num2] = num2 + num;
					num2++;
				}
				else
				{
					num2++;
					findText[^num2] = '\uf8ff';
					findTextPositionMap[findText.Length - num2] = num2 + num;
				}
				navigator.MoveToNextContextPosition(direction);
				break;
			}
			if (num2 >= findText.Length)
			{
				break;
			}
		}
		if (!matchLast)
		{
			if (num2 > 0)
			{
				findTextPositionMap[num2] = findTextPositionMap[num2 - 1] + 1;
			}
			else
			{
				findTextPositionMap[0] = 0;
			}
		}
		return num2;
	}

	internal static void InitializeBidiFlags(string textString, out bool stringContainedBidiCharacter, out bool stringContainedAlefCharacter)
	{
		stringContainedBidiCharacter = false;
		stringContainedAlefCharacter = false;
		foreach (char c in textString)
		{
			if (c >= '\u0590' && c <= '\u07bf')
			{
				stringContainedBidiCharacter = true;
				if (c == 'آ' || c == 'أ' || c == 'إ' || c == 'ا')
				{
					stringContainedAlefCharacter = true;
					break;
				}
			}
		}
	}

	internal static string ReplaceAlefHamzaWithAlef(string textString)
	{
		textString = textString.Replace('آ', 'ا');
		textString = textString.Replace('أ', 'ا');
		textString = textString.Replace('إ', 'ا');
		return textString;
	}

	private static bool IsAtWordBoundary(string textString, int matchIndex, int matchLength, bool hasPreceedingSeparatorChar, bool hasFollowingSeparatorChar)
	{
		bool result = false;
		int length = textString.Length;
		Invariant.Assert(matchIndex + matchLength <= length);
		if (matchIndex == 0)
		{
			if (hasPreceedingSeparatorChar)
			{
				if (matchIndex + matchLength < length)
				{
					if (IsSeparatorChar(textString[matchIndex + matchLength]))
					{
						result = true;
					}
				}
				else if (hasFollowingSeparatorChar)
				{
					result = true;
				}
			}
		}
		else if (matchIndex + matchLength == length)
		{
			if (IsSeparatorChar(textString[matchIndex - 1]) && hasFollowingSeparatorChar)
			{
				result = true;
			}
		}
		else if (IsSeparatorChar(textString[matchIndex - 1]) && IsSeparatorChar(textString[matchIndex + matchLength]))
		{
			result = true;
		}
		return result;
	}

	private static bool IsSeparatorChar(char separatorChar)
	{
		if (char.IsWhiteSpace(separatorChar) || char.IsPunctuation(separatorChar) || char.IsSymbol(separatorChar) || char.IsSeparator(separatorChar))
		{
			return true;
		}
		return false;
	}

	private static bool IsAdjacentToFormatElement(ITextPointer pointer, LogicalDirection direction)
	{
		bool result = false;
		if (direction == LogicalDirection.Forward)
		{
			TextPointerContext pointerContext = pointer.GetPointerContext(LogicalDirection.Forward);
			if (pointerContext == TextPointerContext.ElementStart && TextSchema.IsFormattingType(pointer.GetElementType(LogicalDirection.Forward)))
			{
				result = true;
			}
			else if (pointerContext == TextPointerContext.ElementEnd && TextSchema.IsFormattingType(pointer.ParentType))
			{
				result = true;
			}
		}
		else
		{
			TextPointerContext pointerContext = pointer.GetPointerContext(LogicalDirection.Backward);
			if (pointerContext == TextPointerContext.ElementEnd && TextSchema.IsFormattingType(pointer.GetElementType(LogicalDirection.Backward)))
			{
				result = true;
			}
			else if (pointerContext == TextPointerContext.ElementStart && TextSchema.IsFormattingType(pointer.ParentType))
			{
				result = true;
			}
		}
		return result;
	}
}
