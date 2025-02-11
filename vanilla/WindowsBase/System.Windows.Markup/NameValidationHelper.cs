using System.Globalization;
using MS.Internal.WindowsBase;

namespace System.Windows.Markup;

internal static class NameValidationHelper
{
	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static bool NameValidationCallback(object candidateName)
	{
		if (candidateName is string name)
		{
			return IsValidIdentifierName(name);
		}
		if (candidateName == null)
		{
			return true;
		}
		return false;
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static bool IsValidIdentifierName(string name)
	{
		for (int i = 0; i < name.Length; i++)
		{
			UnicodeCategory unicodeCategory = char.GetUnicodeCategory(name[i]);
			bool flag = unicodeCategory == UnicodeCategory.UppercaseLetter || unicodeCategory == UnicodeCategory.LowercaseLetter || unicodeCategory == UnicodeCategory.TitlecaseLetter || unicodeCategory == UnicodeCategory.OtherLetter || unicodeCategory == UnicodeCategory.LetterNumber || name[i] == '_';
			bool flag2 = unicodeCategory == UnicodeCategory.NonSpacingMark || unicodeCategory == UnicodeCategory.SpacingCombiningMark || unicodeCategory == UnicodeCategory.ModifierLetter || unicodeCategory == UnicodeCategory.DecimalDigitNumber;
			if (i == 0)
			{
				if (!flag)
				{
					return false;
				}
			}
			else if (!(flag || flag2))
			{
				return false;
			}
		}
		return true;
	}
}
