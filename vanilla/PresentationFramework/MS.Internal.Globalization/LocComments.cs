using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace MS.Internal.Globalization;

internal static class LocComments
{
	private class EnumNameIndexTable
	{
		private string _enumPrefix;

		private string[] _enumNames;

		internal EnumNameIndexTable(string enumPrefix, string[] enumNames)
		{
			_enumPrefix = enumPrefix;
			_enumNames = enumNames;
		}

		internal bool TryGet(string enumName, out int enumIndex)
		{
			enumIndex = 0;
			if (enumName.StartsWith(_enumPrefix, StringComparison.Ordinal))
			{
				enumName = enumName.Substring(_enumPrefix.Length);
			}
			for (int i = 0; i < _enumNames.Length; i++)
			{
				if (string.Compare(enumName, _enumNames[i], StringComparison.Ordinal) == 0)
				{
					enumIndex = i;
					return true;
				}
			}
			return false;
		}
	}

	private const char CommentStart = '(';

	private const char CommentEnd = ')';

	private const char EscapeChar = '\\';

	internal const string LocDocumentRoot = "LocalizableAssembly";

	internal const string LocResourcesElement = "LocalizableFile";

	internal const string LocCommentsElement = "LocalizationDirectives";

	internal const string LocFileNameAttribute = "Name";

	internal const string LocCommentIDAttribute = "Uid";

	internal const string LocCommentsAttribute = "Comments";

	internal const string LocLocalizabilityAttribute = "Attributes";

	private static EnumNameIndexTable ReadabilityIndexTable = new EnumNameIndexTable("Readability.", new string[3] { "Unreadable", "Readable", "Inherit" });

	private static EnumNameIndexTable ModifiabilityIndexTable = new EnumNameIndexTable("Modifiability.", new string[3] { "Unmodifiable", "Modifiable", "Inherit" });

	private static EnumNameIndexTable LocalizationCategoryIndexTable = new EnumNameIndexTable("LocalizationCategory.", new string[18]
	{
		"None", "Text", "Title", "Label", "Button", "CheckBox", "ComboBox", "ListBox", "Menu", "RadioButton",
		"ToolTip", "Hyperlink", "TextFlow", "XmlData", "Font", "Inherit", "Ignore", "NeverLocalize"
	});

	internal static bool IsLocLocalizabilityProperty(string type, string property)
	{
		if ("Attributes" == property)
		{
			return "System.Windows.Localization" == type;
		}
		return false;
	}

	internal static bool IsLocCommentsProperty(string type, string property)
	{
		if ("Comments" == property)
		{
			return "System.Windows.Localization" == type;
		}
		return false;
	}

	internal static PropertyComment[] ParsePropertyLocalizabilityAttributes(string input)
	{
		PropertyComment[] array = ParsePropertyComments(input);
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Value = LookupAndSetLocalizabilityAttribute((string)array[i].Value);
			}
		}
		return array;
	}

	internal static PropertyComment[] ParsePropertyComments(string input)
	{
		if (input == null)
		{
			return null;
		}
		List<PropertyComment> list = new List<PropertyComment>(8);
		StringBuilder stringBuilder = new StringBuilder();
		PropertyComment propertyComment = new PropertyComment();
		bool flag = false;
		for (int i = 0; i < input.Length; i++)
		{
			if (propertyComment.PropertyName == null)
			{
				if (char.IsWhiteSpace(input[i]) && !flag)
				{
					if (stringBuilder.Length > 0)
					{
						propertyComment.PropertyName = stringBuilder.ToString();
						stringBuilder.Clear();
					}
				}
				else if (input[i] == '(' && !flag)
				{
					if (i <= 0)
					{
						throw new FormatException(SR.Format(SR.InvalidLocCommentTarget, input));
					}
					propertyComment.PropertyName = stringBuilder.ToString();
					stringBuilder.Clear();
					i--;
				}
				else if (input[i] == '\\' && !flag)
				{
					flag = true;
				}
				else
				{
					stringBuilder.Append(input[i]);
					flag = false;
				}
			}
			else if (stringBuilder.Length == 0)
			{
				if (input[i] == '(' && !flag)
				{
					stringBuilder.Append(input[i]);
					flag = false;
				}
				else if (!char.IsWhiteSpace(input[i]))
				{
					throw new FormatException(SR.Format(SR.InvalidLocCommentValue, propertyComment.PropertyName, input));
				}
			}
			else if (input[i] == ')')
			{
				if (!flag)
				{
					propertyComment.Value = stringBuilder.ToString(1, stringBuilder.Length - 1);
					list.Add(propertyComment);
					stringBuilder.Clear();
					propertyComment = new PropertyComment();
				}
				else
				{
					stringBuilder.Append(input[i]);
					flag = false;
				}
			}
			else
			{
				if (input[i] == '(' && !flag)
				{
					throw new FormatException(SR.Format(SR.InvalidLocCommentValue, propertyComment.PropertyName, input));
				}
				if (input[i] == '\\' && !flag)
				{
					flag = true;
					continue;
				}
				stringBuilder.Append(input[i]);
				flag = false;
			}
		}
		if (propertyComment.PropertyName != null || stringBuilder.Length != 0)
		{
			throw new FormatException(SR.Format(SR.UnmatchedLocComment, input));
		}
		return list.ToArray();
	}

	private static LocalizabilityGroup LookupAndSetLocalizabilityAttribute(string input)
	{
		LocalizabilityGroup localizabilityGroup = new LocalizabilityGroup();
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < input.Length; i++)
		{
			if (char.IsWhiteSpace(input[i]))
			{
				if (stringBuilder.Length > 0)
				{
					ParseLocalizabilityString(stringBuilder.ToString(), localizabilityGroup);
					stringBuilder.Clear();
				}
			}
			else
			{
				stringBuilder.Append(input[i]);
			}
		}
		if (stringBuilder.Length > 0)
		{
			ParseLocalizabilityString(stringBuilder.ToString(), localizabilityGroup);
		}
		return localizabilityGroup;
	}

	private static void ParseLocalizabilityString(string value, LocalizabilityGroup attributeGroup)
	{
		if (ReadabilityIndexTable.TryGet(value, out var enumIndex))
		{
			attributeGroup.Readability = (Readability)enumIndex;
			return;
		}
		if (ModifiabilityIndexTable.TryGet(value, out enumIndex))
		{
			attributeGroup.Modifiability = (Modifiability)enumIndex;
			return;
		}
		if (LocalizationCategoryIndexTable.TryGet(value, out enumIndex))
		{
			attributeGroup.Category = (LocalizationCategory)enumIndex;
			return;
		}
		throw new FormatException(SR.Format(SR.InvalidLocalizabilityValue, value));
	}
}
