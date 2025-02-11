using System.Windows.Input;
using MS.Internal;
using MS.Internal.Commands;

namespace System.Windows.Documents;

internal static class TextEditorCharacters
{
	internal const double OneFontPoint = 0.75;

	internal const double MaxFontPoint = 1638.0;

	private const string KeyDecreaseFontSize = "Ctrl+OemOpenBrackets";

	private const string KeyIncreaseFontSize = "Ctrl+OemCloseBrackets";

	private const string KeyResetFormat = "Ctrl+Space";

	private const string KeyToggleBold = "Ctrl+B";

	private const string KeyToggleItalic = "Ctrl+I";

	private const string KeyToggleSubscript = "Ctrl+OemPlus";

	private const string KeyToggleSuperscript = "Ctrl+Shift+OemPlus";

	private const string KeyToggleUnderline = "Ctrl+U";

	internal static void _RegisterClassHandlers(Type controlType, bool registerEventListeners)
	{
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = OnQueryStatusNYI;
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ResetFormat, OnResetFormat, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+Space", "KeyResetFormatDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ToggleBold, OnToggleBold, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+B", "KeyToggleBoldDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ToggleItalic, OnToggleItalic, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+I", "KeyToggleItalicDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ToggleUnderline, OnToggleUnderline, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+U", "KeyToggleUnderlineDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ToggleSubscript, OnToggleSubscript, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+OemPlus", "KeyToggleSubscriptDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ToggleSuperscript, OnToggleSuperscript, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+Shift+OemPlus", "KeyToggleSuperscriptDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.IncreaseFontSize, OnIncreaseFontSize, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+OemCloseBrackets", "KeyIncreaseFontSizeDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.DecreaseFontSize, OnDecreaseFontSize, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Ctrl+OemOpenBrackets", "KeyDecreaseFontSizeDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ApplyFontSize, OnApplyFontSize, canExecuteRoutedEventHandler, "KeyApplyFontSize", "KeyApplyFontSizeDisplayString");
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ApplyFontFamily, OnApplyFontFamily, canExecuteRoutedEventHandler, "KeyApplyFontFamily", "KeyApplyFontFamilyDisplayString");
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ApplyForeground, OnApplyForeground, canExecuteRoutedEventHandler, "KeyApplyForeground", "KeyApplyForegroundDisplayString");
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ApplyBackground, OnApplyBackground, canExecuteRoutedEventHandler, "KeyApplyBackground", "KeyApplyBackgroundDisplayString");
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ToggleSpellCheck, OnToggleSpellCheck, canExecuteRoutedEventHandler, "KeyToggleSpellCheck", "KeyToggleSpellCheckDisplayString");
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ApplyInlineFlowDirectionRTL, OnApplyInlineFlowDirectionRTL, OnQueryStatusNYI);
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ApplyInlineFlowDirectionLTR, OnApplyInlineFlowDirectionLTR, OnQueryStatusNYI);
	}

	internal static void _OnApplyProperty(TextEditor This, DependencyProperty formattingProperty, object propertyValue)
	{
		_OnApplyProperty(This, formattingProperty, propertyValue, applyToParagraphs: false, PropertyValueAction.SetValue);
	}

	internal static void _OnApplyProperty(TextEditor This, DependencyProperty formattingProperty, object propertyValue, bool applyToParagraphs)
	{
		_OnApplyProperty(This, formattingProperty, propertyValue, applyToParagraphs, PropertyValueAction.SetValue);
	}

	internal static void _OnApplyProperty(TextEditor This, DependencyProperty formattingProperty, object propertyValue, bool applyToParagraphs, PropertyValueAction propertyValueAction)
	{
		if (This == null || !This._IsEnabled || This.IsReadOnly || !This.AcceptsRichContent || !(This.Selection is TextSelection))
		{
			return;
		}
		if (!TextSchema.IsParagraphProperty(formattingProperty) && !TextSchema.IsCharacterProperty(formattingProperty))
		{
			Invariant.Assert(condition: false, "The property '" + formattingProperty.Name + "' is unknown to TextEditor");
			return;
		}
		TextSelection textSelection = (TextSelection)This.Selection;
		if (!TextSchema.IsStructuralCharacterProperty(formattingProperty) || TextRangeEdit.CanApplyStructuralInlineProperty(textSelection.Start, textSelection.End))
		{
			TextEditorTyping._FlushPendingInputItems(This);
			TextEditorSelection._ClearSuggestedX(This);
			TextEditorTyping._BreakTypingSequence(This);
			textSelection.ApplyPropertyValue(formattingProperty, propertyValue, applyToParagraphs, propertyValueAction);
		}
	}

	private static void OnResetFormat(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly || !textEditor.AcceptsRichContent || !(textEditor.Selection.Start is TextPointer))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		using (textEditor.Selection.DeclareChangeBlock())
		{
			TextPointer start = (TextPointer)textEditor.Selection.Start;
			TextPointer end = (TextPointer)textEditor.Selection.End;
			if (textEditor.Selection.IsEmpty)
			{
				TextSegment autoWord = TextRangeBase.GetAutoWord(textEditor.Selection);
				if (autoWord.IsNull)
				{
					((TextSelection)textEditor.Selection).ClearSpringloadFormatting();
					return;
				}
				start = (TextPointer)autoWord.Start;
				end = (TextPointer)autoWord.End;
			}
			TextEditorSelection._ClearSuggestedX(textEditor);
			TextRangeEdit.CharacterResetFormatting(start, end);
		}
	}

	private static void OnToggleBold(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly && textEditor.AcceptsRichContent && textEditor.Selection is TextSelection)
		{
			TextEditorTyping._FlushPendingInputItems(textEditor);
			object currentValue = ((TextSelection)textEditor.Selection).GetCurrentValue(TextElement.FontWeightProperty);
			FontWeight fontWeight = ((currentValue != DependencyProperty.UnsetValue && (FontWeight)currentValue == FontWeights.Bold) ? FontWeights.Normal : FontWeights.Bold);
			_OnApplyProperty(textEditor, TextElement.FontWeightProperty, fontWeight);
		}
	}

	private static void OnToggleItalic(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly && textEditor.AcceptsRichContent && textEditor.Selection is TextSelection)
		{
			TextEditorTyping._FlushPendingInputItems(textEditor);
			object currentValue = ((TextSelection)textEditor.Selection).GetCurrentValue(TextElement.FontStyleProperty);
			FontStyle fontStyle = ((currentValue != DependencyProperty.UnsetValue && (FontStyle)currentValue == FontStyles.Italic) ? FontStyles.Normal : FontStyles.Italic);
			_OnApplyProperty(textEditor, TextElement.FontStyleProperty, fontStyle);
			textEditor.Selection.RefreshCaret();
		}
	}

	private static void OnToggleUnderline(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly && textEditor.AcceptsRichContent && textEditor.Selection is TextSelection)
		{
			TextEditorTyping._FlushPendingInputItems(textEditor);
			object currentValue = ((TextSelection)textEditor.Selection).GetCurrentValue(Inline.TextDecorationsProperty);
			TextDecorationCollection textDecorationCollection = ((currentValue != DependencyProperty.UnsetValue) ? ((TextDecorationCollection)currentValue) : null);
			TextDecorationCollection result;
			if (!TextSchema.HasTextDecorations(textDecorationCollection))
			{
				result = TextDecorations.Underline;
			}
			else if (!textDecorationCollection.TryRemove(TextDecorations.Underline, out result))
			{
				result.Add(TextDecorations.Underline);
			}
			_OnApplyProperty(textEditor, Inline.TextDecorationsProperty, result);
		}
	}

	private static void OnToggleSubscript(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly && textEditor.AcceptsRichContent && textEditor.Selection is TextSelection)
		{
			TextEditorTyping._FlushPendingInputItems(textEditor);
			FontVariants fontVariants = (FontVariants)((TextSelection)textEditor.Selection).GetCurrentValue(Typography.VariantsProperty);
			fontVariants = ((fontVariants != FontVariants.Subscript) ? FontVariants.Subscript : FontVariants.Normal);
			_OnApplyProperty(textEditor, Typography.VariantsProperty, fontVariants);
		}
	}

	private static void OnToggleSuperscript(object sender, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly && textEditor.AcceptsRichContent && textEditor.Selection is TextSelection)
		{
			TextEditorTyping._FlushPendingInputItems(textEditor);
			FontVariants fontVariants = (FontVariants)((TextSelection)textEditor.Selection).GetCurrentValue(Typography.VariantsProperty);
			fontVariants = ((fontVariants != FontVariants.Superscript) ? FontVariants.Superscript : FontVariants.Normal);
			_OnApplyProperty(textEditor, Typography.VariantsProperty, fontVariants);
		}
	}

	private static void OnIncreaseFontSize(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly || !textEditor.AcceptsRichContent || !(textEditor.Selection is TextSelection))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		if (textEditor.Selection.IsEmpty)
		{
			double num = (double)((TextSelection)textEditor.Selection).GetCurrentValue(TextElement.FontSizeProperty);
			if (num != 0.0 && num < 1638.0)
			{
				num += 0.75;
				if (num > 1638.0)
				{
					num = 1638.0;
				}
				_OnApplyProperty(textEditor, TextElement.FontSizeProperty, num);
			}
		}
		else
		{
			_OnApplyProperty(textEditor, TextElement.FontSizeProperty, 0.75, applyToParagraphs: false, PropertyValueAction.IncreaseByAbsoluteValue);
		}
	}

	private static void OnDecreaseFontSize(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly || !textEditor.AcceptsRichContent || !(textEditor.Selection is TextSelection))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		if (textEditor.Selection.IsEmpty)
		{
			double num = (double)((TextSelection)textEditor.Selection).GetCurrentValue(TextElement.FontSizeProperty);
			if (num != 0.0 && num > 0.75)
			{
				num -= 0.75;
				if (num < 0.75)
				{
					num = 0.75;
				}
				_OnApplyProperty(textEditor, TextElement.FontSizeProperty, num);
			}
		}
		else
		{
			_OnApplyProperty(textEditor, TextElement.FontSizeProperty, 0.75, applyToParagraphs: false, PropertyValueAction.DecreaseByAbsoluteValue);
		}
	}

	private static void OnApplyFontSize(object target, ExecutedRoutedEventArgs args)
	{
		if (args.Parameter != null)
		{
			_OnApplyProperty(TextEditor._GetTextEditor(target), TextElement.FontSizeProperty, args.Parameter);
		}
	}

	private static void OnApplyFontFamily(object target, ExecutedRoutedEventArgs args)
	{
		if (args.Parameter != null)
		{
			_OnApplyProperty(TextEditor._GetTextEditor(target), TextElement.FontFamilyProperty, args.Parameter);
		}
	}

	private static void OnApplyForeground(object target, ExecutedRoutedEventArgs args)
	{
		if (args.Parameter != null)
		{
			_OnApplyProperty(TextEditor._GetTextEditor(target), TextElement.ForegroundProperty, args.Parameter);
		}
	}

	private static void OnApplyBackground(object target, ExecutedRoutedEventArgs args)
	{
		if (args.Parameter != null)
		{
			_OnApplyProperty(TextEditor._GetTextEditor(target), TextElement.BackgroundProperty, args.Parameter);
		}
	}

	private static void OnToggleSpellCheck(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly)
		{
			textEditor.IsSpellCheckEnabled = !textEditor.IsSpellCheckEnabled;
		}
	}

	private static void OnApplyInlineFlowDirectionRTL(object target, ExecutedRoutedEventArgs args)
	{
		_OnApplyProperty(TextEditor._GetTextEditor(target), Inline.FlowDirectionProperty, FlowDirection.RightToLeft);
	}

	private static void OnApplyInlineFlowDirectionLTR(object target, ExecutedRoutedEventArgs args)
	{
		_OnApplyProperty(TextEditor._GetTextEditor(target), Inline.FlowDirectionProperty, FlowDirection.LeftToRight);
	}

	private static void OnQueryStatusNYI(object target, CanExecuteRoutedEventArgs args)
	{
		if (TextEditor._GetTextEditor(target) != null)
		{
			args.CanExecute = true;
		}
	}
}
