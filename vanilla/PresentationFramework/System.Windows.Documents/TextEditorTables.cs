using System.Windows.Input;
using MS.Internal.Commands;

namespace System.Windows.Documents;

internal static class TextEditorTables
{
	private const string KeyDeleteColumns = "Alt+Ctrl+Shift+D";

	private const string KeyInsertColumns = "Alt+Ctrl+Shift+C";

	private const string KeyInsertRows = "Alt+Ctrl+Shift+R";

	private const string KeyInsertTable = "Alt+Ctrl+Shift+T";

	private const string KeyMergeCells = "Alt+Ctrl+Shift+M";

	private const string KeySplitCell = "Alt+Ctrl+Shift+S";

	internal static void _RegisterClassHandlers(Type controlType, bool registerEventListeners)
	{
		ExecutedRoutedEventHandler executedRoutedEventHandler = OnTableCommand;
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = OnQueryStatusNYI;
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.InsertTable, executedRoutedEventHandler, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Alt+Ctrl+Shift+T", "KeyInsertTableDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.InsertRows, executedRoutedEventHandler, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Alt+Ctrl+Shift+R", "KeyInsertRowsDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.InsertColumns, executedRoutedEventHandler, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Alt+Ctrl+Shift+C", "KeyInsertColumnsDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.DeleteRows, executedRoutedEventHandler, canExecuteRoutedEventHandler, "KeyDeleteRows", "KeyDeleteRowsDisplayString");
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.DeleteColumns, executedRoutedEventHandler, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Alt+Ctrl+Shift+D", "KeyDeleteColumnsDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.MergeCells, executedRoutedEventHandler, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Alt+Ctrl+Shift+M", "KeyMergeCellsDisplayString"));
		CommandHelpers.RegisterCommandHandler(controlType, EditingCommands.SplitCell, executedRoutedEventHandler, canExecuteRoutedEventHandler, KeyGesture.CreateFromResourceStrings("Alt+Ctrl+Shift+S", "KeySplitCellDisplayString"));
	}

	private static void OnTableCommand(object target, ExecutedRoutedEventArgs args)
	{
		TextEditor textEditor = TextEditor._GetTextEditor(target);
		if (textEditor != null && textEditor._IsEnabled && !textEditor.IsReadOnly && textEditor.AcceptsRichContent && textEditor.Selection is TextSelection)
		{
			TextEditorTyping._FlushPendingInputItems(textEditor);
			TextEditorSelection._ClearSuggestedX(textEditor);
			if (args.Command == EditingCommands.InsertTable)
			{
				((TextSelection)textEditor.Selection).InsertTable(4, 4);
			}
			else if (args.Command == EditingCommands.InsertRows)
			{
				((TextSelection)textEditor.Selection).InsertRows(1);
			}
			else if (args.Command == EditingCommands.InsertColumns)
			{
				((TextSelection)textEditor.Selection).InsertColumns(1);
			}
			else if (args.Command == EditingCommands.DeleteRows)
			{
				((TextSelection)textEditor.Selection).DeleteRows();
			}
			else if (args.Command == EditingCommands.DeleteColumns)
			{
				((TextSelection)textEditor.Selection).DeleteColumns();
			}
			else if (args.Command == EditingCommands.MergeCells)
			{
				((TextSelection)textEditor.Selection).MergeCells();
			}
			else if (args.Command == EditingCommands.SplitCell)
			{
				((TextSelection)textEditor.Selection).SplitCell(1000, 1000);
			}
		}
	}

	private static void OnQueryStatusNYI(object target, CanExecuteRoutedEventArgs args)
	{
		if (TextEditor._GetTextEditor(target) != null)
		{
			args.CanExecute = true;
		}
	}
}
