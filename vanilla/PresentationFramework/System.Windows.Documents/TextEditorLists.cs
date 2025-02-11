using System.Windows.Input;
using MS.Internal;
using MS.Internal.Commands;

namespace System.Windows.Documents;

internal static class TextEditorLists
{
	private const string KeyDecreaseIndentation = "Ctrl+Shift+T";

	private const string KeyToggleBullets = "Ctrl+Shift+L";

	private const string KeyToggleNumbering = "Ctrl+Shift+N";

	private const string KeyRemoveListMarkers = "Ctrl+Shift+R";

	private const string KeyIncreaseIndentation = "Ctrl+T";

	internal static void _RegisterClassHandlers(Type controlType, bool registerEventListeners)
	{
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.RemoveListMarkers, OnListCommand, OnQueryStatusNYI, KeyGesture.CreateFromResourceStrings("Ctrl+Shift+R", "KeyRemoveListMarkersDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ToggleBullets, OnListCommand, OnQueryStatusNYI, KeyGesture.CreateFromResourceStrings("Ctrl+Shift+L", "KeyToggleBulletsDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.ToggleNumbering, OnListCommand, OnQueryStatusNYI, KeyGesture.CreateFromResourceStrings("Ctrl+Shift+N", "KeyToggleNumberingDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.IncreaseIndentation, OnListCommand, OnQueryStatusTab, KeyGesture.CreateFromResourceStrings("Ctrl+T", "KeyIncreaseIndentationDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.DecreaseIndentation, OnListCommand, OnQueryStatusTab, KeyGesture.CreateFromResourceStrings("Ctrl+Shift+T", "KeyDecreaseIndentationDisplayString"));
	}

	internal static void DecreaseIndentation(TextEditor This)
	{
		TextSelection obj = (TextSelection)This.Selection;
		ListItem listItem = TextPointerBase.GetListItem(obj.Start);
		ListItem immediateListItem = TextPointerBase.GetImmediateListItem(obj.Start);
		DecreaseIndentation(obj, listItem, immediateListItem);
	}

	private static TextEditor IsEnabledNotReadOnlyIsTextSegment(object sender)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(sender);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly && !textEditor.Selection.IsTableCellRange)
		{
			return textEditor;
		}
		return null;
	}

	private static void OnQueryStatusTab(object sender, CanExecuteRoutedEventArgs args)
	{
		TextEditor textEditor = IsEnabledNotReadOnlyIsTextSegment(sender);
		if (textEditor != null && textEditor.AcceptsTab)
		{
			args.CanExecute = true;
		}
	}

	private static void OnQueryStatusNYI(object target, CanExecuteRoutedEventArgs args)
	{
		if (TextEditor._GetTextEditor(target) != null)
		{
			args.CanExecute = true;
		}
	}

	private static void OnListCommand(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor == null || !textEditor._IsEnabled || textEditor.IsReadOnly || !textEditor.AcceptsRichContent || !(textEditor.Selection is TextSelection))
		{
			return;
		}
		TextEditorTyping._FlushPendingInputItems(textEditor);
		if (!TextRangeEditLists.IsListOperationApplicable((TextSelection)textEditor.Selection))
		{
			return;
		}
		using (textEditor.Selection.DeclareChangeBlock())
		{
			TextSelection textSelection = (TextSelection)textEditor.Selection;
			ListItem listItem = TextPointerBase.GetListItem(textSelection.Start);
			ListItem immediateListItem = TextPointerBase.GetImmediateListItem(textSelection.Start);
			List list = ((listItem == null) ? null : ((List)listItem.Parent));
			TextEditorSelection._ClearSuggestedX(textEditor);
			if (args.Command == EditingCommands.ToggleBullets)
			{
				ToggleBullets(textSelection, listItem, immediateListItem, list);
			}
			else if (args.Command == EditingCommands.ToggleNumbering)
			{
				ToggleNumbering(textSelection, listItem, immediateListItem, list);
			}
			else if (args.Command == EditingCommands.RemoveListMarkers)
			{
				TextRangeEditLists.ConvertListItemsToParagraphs(textSelection);
			}
			else if (args.Command == EditingCommands.IncreaseIndentation)
			{
				IncreaseIndentation(textSelection, listItem, immediateListItem);
			}
			else if (args.Command == EditingCommands.DecreaseIndentation)
			{
				DecreaseIndentation(textSelection, listItem, immediateListItem);
			}
			else
			{
				Invariant.Assert(condition: false);
			}
		}
	}

	private static void ToggleBullets(TextSelection thisSelection, ListItem parentListItem, ListItem immediateListItem, List list)
	{
		if (immediateListItem != null && HasBulletMarker(list))
		{
			if (list.Parent is ListItem)
			{
				TextRangeEditLists.UnindentListItems(thisSelection);
				TextRangeEditLists.ConvertListItemsToParagraphs(thisSelection);
			}
			else
			{
				TextRangeEditLists.UnindentListItems(thisSelection);
			}
		}
		else if (immediateListItem != null)
		{
			list.MarkerStyle = TextMarkerStyle.Disc;
		}
		else if (parentListItem != null)
		{
			TextRangeEditLists.ConvertParagraphsToListItems(thisSelection, TextMarkerStyle.Disc);
			TextRangeEditLists.IndentListItems(thisSelection);
		}
		else
		{
			TextRangeEditLists.ConvertParagraphsToListItems(thisSelection, TextMarkerStyle.Disc);
		}
	}

	private static void ToggleNumbering(TextSelection thisSelection, ListItem parentListItem, ListItem immediateListItem, List list)
	{
		if (immediateListItem != null && HasNumericMarker(list))
		{
			if (list.Parent is ListItem)
			{
				TextRangeEditLists.UnindentListItems(thisSelection);
				TextRangeEditLists.ConvertListItemsToParagraphs(thisSelection);
			}
			else
			{
				TextRangeEditLists.UnindentListItems(thisSelection);
			}
		}
		else if (immediateListItem != null)
		{
			list.MarkerStyle = TextMarkerStyle.Decimal;
		}
		else if (parentListItem != null)
		{
			TextRangeEditLists.ConvertParagraphsToListItems(thisSelection, TextMarkerStyle.Decimal);
			TextRangeEditLists.IndentListItems(thisSelection);
		}
		else
		{
			TextRangeEditLists.ConvertParagraphsToListItems(thisSelection, TextMarkerStyle.Decimal);
		}
	}

	private static void IncreaseIndentation(TextSelection thisSelection, ListItem parentListItem, ListItem immediateListItem)
	{
		if (immediateListItem != null)
		{
			TextRangeEditLists.IndentListItems(thisSelection);
		}
		else if (parentListItem != null)
		{
			TextRangeEditLists.ConvertParagraphsToListItems(thisSelection, TextMarkerStyle.Decimal);
			TextRangeEditLists.IndentListItems(thisSelection);
		}
		else if (thisSelection.IsEmpty)
		{
			if (thisSelection.Start.ParagraphOrBlockUIContainer is BlockUIContainer)
			{
				TextRangeEdit.IncrementParagraphLeadingMargin(thisSelection, 20.0, PropertyValueAction.IncreaseByAbsoluteValue);
				return;
			}
			CreateImplicitParagraphIfNeededAndUpdateSelection(thisSelection);
			Paragraph paragraph = thisSelection.Start.Paragraph;
			Invariant.Assert(paragraph != null, "EnsureInsertionPosition must guarantee a position in text content");
			if (paragraph.TextIndent < 0.0)
			{
				TextRangeEdit.SetParagraphProperty(thisSelection.Start, thisSelection.End, Paragraph.TextIndentProperty, 0.0, PropertyValueAction.SetValue);
			}
			else if (paragraph.TextIndent < 20.0)
			{
				TextRangeEdit.SetParagraphProperty(thisSelection.Start, thisSelection.End, Paragraph.TextIndentProperty, 20.0, PropertyValueAction.SetValue);
			}
			else
			{
				TextRangeEdit.IncrementParagraphLeadingMargin(thisSelection, 20.0, PropertyValueAction.IncreaseByAbsoluteValue);
			}
		}
		else
		{
			TextRangeEdit.IncrementParagraphLeadingMargin(thisSelection, 20.0, PropertyValueAction.IncreaseByAbsoluteValue);
		}
	}

	private static void DecreaseIndentation(TextSelection thisSelection, ListItem parentListItem, ListItem immediateListItem)
	{
		if (immediateListItem != null)
		{
			TextRangeEditLists.UnindentListItems(thisSelection);
		}
		else if (parentListItem != null)
		{
			TextRangeEditLists.ConvertParagraphsToListItems(thisSelection, TextMarkerStyle.Disc);
			TextRangeEditLists.UnindentListItems(thisSelection);
		}
		else if (thisSelection.IsEmpty)
		{
			if (thisSelection.Start.ParagraphOrBlockUIContainer is BlockUIContainer)
			{
				TextRangeEdit.IncrementParagraphLeadingMargin(thisSelection, 20.0, PropertyValueAction.DecreaseByAbsoluteValue);
				return;
			}
			CreateImplicitParagraphIfNeededAndUpdateSelection(thisSelection);
			Paragraph paragraph = thisSelection.Start.Paragraph;
			Invariant.Assert(paragraph != null, "EnsureInsertionPosition must guarantee a position in text content");
			if (paragraph.TextIndent > 20.0)
			{
				TextRangeEdit.SetParagraphProperty(thisSelection.Start, thisSelection.End, Paragraph.TextIndentProperty, 20.0, PropertyValueAction.SetValue);
			}
			else if (paragraph.TextIndent > 0.0)
			{
				TextRangeEdit.SetParagraphProperty(thisSelection.Start, thisSelection.End, Paragraph.TextIndentProperty, 0.0, PropertyValueAction.SetValue);
			}
			else
			{
				TextRangeEdit.IncrementParagraphLeadingMargin(thisSelection, 20.0, PropertyValueAction.DecreaseByAbsoluteValue);
			}
		}
		else
		{
			TextRangeEdit.IncrementParagraphLeadingMargin(thisSelection, 20.0, PropertyValueAction.DecreaseByAbsoluteValue);
		}
	}

	private static void CreateImplicitParagraphIfNeededAndUpdateSelection(TextSelection thisSelection)
	{
		TextPointer start = thisSelection.Start;
		if (TextPointerBase.IsAtPotentialParagraphPosition(start))
		{
			start = TextRangeEditTables.EnsureInsertionPosition(start);
			thisSelection.Select(start, start);
		}
	}

	private static bool HasBulletMarker(List list)
	{
		if (list == null)
		{
			return false;
		}
		TextMarkerStyle markerStyle = list.MarkerStyle;
		if (TextMarkerStyle.Disc <= markerStyle)
		{
			return markerStyle <= TextMarkerStyle.Box;
		}
		return false;
	}

	private static bool HasNumericMarker(List list)
	{
		if (list == null)
		{
			return false;
		}
		TextMarkerStyle markerStyle = list.MarkerStyle;
		if (TextMarkerStyle.LowerRoman <= markerStyle)
		{
			return markerStyle <= TextMarkerStyle.Decimal;
		}
		return false;
	}
}
