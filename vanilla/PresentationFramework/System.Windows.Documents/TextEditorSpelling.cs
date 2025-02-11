using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.Commands;

namespace System.Windows.Documents;

internal static class TextEditorSpelling
{
	internal static void _RegisterClassHandlers(Type controlType, bool registerEventListeners)
	{
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.CorrectSpellingError, OnCorrectSpellingError, OnQueryStatusSpellingError);
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.IgnoreSpellingError, OnIgnoreSpellingError, OnQueryStatusSpellingError);
	}

	internal static SpellingError GetSpellingErrorAtPosition(TextEditor This, ITextPointer position, LogicalDirection direction)
	{
		if (This.Speller != null)
		{
			return This.Speller.GetError(position, direction, forceEvaluation: true);
		}
		return null;
	}

	internal static SpellingError GetSpellingErrorAtSelection(TextEditor This)
	{
		if (This.Speller == null)
		{
			return null;
		}
		if (IsSelectionIgnoringErrors(This.Selection))
		{
			return null;
		}
		LogicalDirection logicalDirection = ((!This.Selection.IsEmpty) ? LogicalDirection.Forward : This.Selection.Start.LogicalDirection);
		char character;
		ITextPointer textPointer = GetNextTextPosition(This.Selection.Start, null, logicalDirection, out character);
		if (textPointer == null)
		{
			logicalDirection = ((logicalDirection != LogicalDirection.Forward) ? LogicalDirection.Forward : LogicalDirection.Backward);
			textPointer = GetNextTextPosition(This.Selection.Start, null, logicalDirection, out character);
		}
		else if (char.IsWhiteSpace(character))
		{
			if (This.Selection.IsEmpty)
			{
				logicalDirection = ((logicalDirection != LogicalDirection.Forward) ? LogicalDirection.Forward : LogicalDirection.Backward);
				textPointer = GetNextTextPosition(This.Selection.Start, null, logicalDirection, out character);
			}
			else
			{
				logicalDirection = LogicalDirection.Forward;
				textPointer = GetNextNonWhiteSpacePosition(This.Selection.Start, This.Selection.End);
				if (textPointer == null)
				{
					logicalDirection = LogicalDirection.Backward;
					textPointer = GetNextTextPosition(This.Selection.Start, null, logicalDirection, out character);
				}
			}
		}
		if (textPointer != null)
		{
			return This.Speller.GetError(textPointer, logicalDirection, forceEvaluation: false);
		}
		return null;
	}

	internal static ITextPointer GetNextSpellingErrorPosition(TextEditor This, ITextPointer position, LogicalDirection direction)
	{
		if (This.Speller != null)
		{
			return This.Speller.GetNextSpellingErrorPosition(position, direction);
		}
		return null;
	}

	private static void OnCorrectSpellingError(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !(args.Parameter is string text))
		{
			return;
		}
		SpellingError spellingErrorAtSelection = GetSpellingErrorAtSelection(textEditor);
		if (spellingErrorAtSelection == null)
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			ITextPointer textPointer;
			if (IsErrorAtNonMergeableInlineEdge(spellingErrorAtSelection, out var textStart, out var textEnd) && textStart is TextPointer)
			{
				((TextPointer)textStart).DeleteTextInRun(textStart.GetOffsetToPosition(textEnd));
				textStart.InsertTextInRun(text);
				textPointer = textStart.CreatePointer(text.Length, LogicalDirection.Forward);
			}
			else
			{
				textEditor.Selection.Select(spellingErrorAtSelection.Start, spellingErrorAtSelection.End);
				if (textEditor.AcceptsRichContent)
				{
					((TextSelection)textEditor.Selection).SpringloadCurrentFormatting();
				}
				XmlLanguage xmlLanguage = (XmlLanguage)spellingErrorAtSelection.Start.GetValue(FrameworkElement.LanguageProperty);
				textEditor.SetSelectedText(text, xmlLanguage.GetSpecificCulture());
				textPointer = textEditor.Selection.End;
			}
			textEditor.Selection.Select(textPointer, textPointer);
		}
	}

	private static bool IsErrorAtNonMergeableInlineEdge(SpellingError spellingError, out ITextPointer textStart, out ITextPointer textEnd)
	{
		bool result = false;
		textStart = spellingError.Start.CreatePointer(LogicalDirection.Backward);
		while (textStart.CompareTo(spellingError.End) < 0 && textStart.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text)
		{
			textStart.MoveToNextContextPosition(LogicalDirection.Forward);
		}
		textEnd = spellingError.End.CreatePointer();
		while (textEnd.CompareTo(spellingError.Start) > 0 && textEnd.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.Text)
		{
			textEnd.MoveToNextContextPosition(LogicalDirection.Backward);
		}
		if (textStart.GetPointerContext(LogicalDirection.Forward) != TextPointerContext.Text || textStart.CompareTo(spellingError.End) == 0)
		{
			return false;
		}
		Invariant.Assert(textEnd.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text && textEnd.CompareTo(spellingError.Start) != 0);
		if ((TextPointerBase.IsAtNonMergeableInlineStart(textStart) || TextPointerBase.IsAtNonMergeableInlineEnd(textEnd)) && typeof(Run).IsAssignableFrom(textStart.ParentType) && textStart.HasEqualScope(textEnd))
		{
			result = true;
		}
		return result;
	}

	private static void OnIgnoreSpellingError(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null)
		{
			GetSpellingErrorAtSelection(textEditor)?.IgnoreAll();
		}
	}

	private static void OnQueryStatusSpellingError(object target, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null)
		{
			SpellingError spellingErrorAtSelection = GetSpellingErrorAtSelection(textEditor);
			args.CanExecute = spellingErrorAtSelection != null;
		}
	}

	private static ITextPointer GetNextTextPosition(ITextPointer position, ITextPointer limit, LogicalDirection direction, out char character)
	{
		bool flag = false;
		character = '\0';
		while (position != null && !flag && (limit == null || position.CompareTo(limit) < 0))
		{
			switch (position.GetPointerContext(direction))
			{
			case TextPointerContext.Text:
			{
				char[] array = new char[1];
				position.GetTextInRun(direction, array, 0, 1);
				character = array[0];
				flag = true;
				break;
			}
			case TextPointerContext.ElementStart:
			case TextPointerContext.ElementEnd:
				position = ((!TextSchema.IsFormattingType(position.GetElementType(direction))) ? null : position.CreatePointer(1));
				break;
			default:
				position = null;
				break;
			}
		}
		return position;
	}

	private static ITextPointer GetNextNonWhiteSpacePosition(ITextPointer position, ITextPointer limit)
	{
		Invariant.Assert(limit != null);
		while (true)
		{
			if (position.CompareTo(limit) == 0)
			{
				position = null;
				break;
			}
			position = GetNextTextPosition(position, limit, LogicalDirection.Forward, out var character);
			if (position == null || !char.IsWhiteSpace(character))
			{
				break;
			}
			position = position.CreatePointer(1);
		}
		return position;
	}

	private static bool IsSelectionIgnoringErrors(ITextSelection selection)
	{
		bool flag = false;
		if (selection.Start is TextPointer)
		{
			flag = ((TextPointer)selection.Start).ParentBlock != ((TextPointer)selection.End).ParentBlock;
		}
		if (!flag)
		{
			flag = selection.Start.GetOffsetToPosition(selection.End) >= 256;
		}
		if (!flag)
		{
			string text = selection.Text;
			for (int i = 0; i < text.Length; i++)
			{
				if (flag)
				{
					break;
				}
				flag = TextPointerBase.IsCharUnicodeNewLine(text[i]);
			}
		}
		return flag;
	}
}
