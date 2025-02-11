using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Provides a standard set of application related commands.</summary>
public static class ApplicationCommands
{
	private enum CommandId : byte
	{
		Cut,
		Copy,
		Paste,
		Undo,
		Redo,
		Delete,
		Find,
		Replace,
		Help,
		SelectAll,
		New,
		Open,
		Save,
		SaveAs,
		Print,
		CancelPrint,
		PrintPreview,
		Close,
		Properties,
		ContextMenu,
		CorrectionList,
		Stop,
		NotACommand,
		Last
	}

	private static RoutedUICommand[] _internalCommands = new RoutedUICommand[23];

	private const string ContextMenuKey = "Shift+F10;Apps";

	private const string CopyKey = "Ctrl+C;Ctrl+Insert";

	private const string CutKey = "Ctrl+X;Shift+Delete";

	private const string DeleteKey = "Del";

	private const string FindKey = "Ctrl+F";

	private const string HelpKey = "F1";

	private const string NewKey = "Ctrl+N";

	private const string OpenKey = "Ctrl+O";

	private const string PasteKey = "Ctrl+V;Shift+Insert";

	private const string PrintKey = "Ctrl+P";

	private const string PrintPreviewKey = "Ctrl+F2";

	private const string PropertiesKey = "F4";

	private const string RedoKey = "Ctrl+Y";

	private const string ReplaceKey = "Ctrl+H";

	private const string SaveKey = "Ctrl+S";

	private const string SelectAllKey = "Ctrl+A";

	private const string StopKey = "Esc";

	private const string UndoKey = "Ctrl+Z";

	/// <summary> Gets the value that represents the Cut command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+XShift+DeleteUI TextCut</returns>
	public static RoutedUICommand Cut => _EnsureCommand(CommandId.Cut);

	/// <summary>Gets the value that represents the Copy command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+CCtrl+InsertUI TextCopy</returns>
	public static RoutedUICommand Copy => _EnsureCommand(CommandId.Copy);

	/// <summary> Gets the value that represents the Paste command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+VShift+InsertUI TextPaste</returns>
	public static RoutedUICommand Paste => _EnsureCommand(CommandId.Paste);

	/// <summary> Gets the value that represents the Delete command. </summary>
	/// <returns>The command.Default ValuesKey GestureDelUI TextDelete</returns>
	public static RoutedUICommand Delete => _EnsureCommand(CommandId.Delete);

	/// <summary>Gets the value that represents the Undo command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl-ZUI TextUndo</returns>
	public static RoutedUICommand Undo => _EnsureCommand(CommandId.Undo);

	/// <summary> Gets the value that represents the Redo command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+YUI TextRedo</returns>
	public static RoutedUICommand Redo => _EnsureCommand(CommandId.Redo);

	/// <summary> Gets the value that represents the Find command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+FUI TextFind</returns>
	public static RoutedUICommand Find => _EnsureCommand(CommandId.Find);

	/// <summary> Gets the value that represents the Replace command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+HUI TextReplace</returns>
	public static RoutedUICommand Replace => _EnsureCommand(CommandId.Replace);

	/// <summary>Gets the value that represents the Select All command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+AUI TextSelect All</returns>
	public static RoutedUICommand SelectAll => _EnsureCommand(CommandId.SelectAll);

	/// <summary> Gets the value that represents the Help command. </summary>
	/// <returns>The command.Default ValuesKey GestureF1UI TextHelp</returns>
	public static RoutedUICommand Help => _EnsureCommand(CommandId.Help);

	/// <summary> Gets the value that represents the New command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+NUI TextNew</returns>
	public static RoutedUICommand New => _EnsureCommand(CommandId.New);

	/// <summary> Gets the value that represents the Open command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+OUI TextOpen</returns>
	public static RoutedUICommand Open => _EnsureCommand(CommandId.Open);

	/// <summary>Gets the value that represents the Close command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextClose</returns>
	public static RoutedUICommand Close => _EnsureCommand(CommandId.Close);

	/// <summary> Gets the value that represents the Save command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+SUI TextSave</returns>
	public static RoutedUICommand Save => _EnsureCommand(CommandId.Save);

	/// <summary> Gets the value that represents the Save As command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextSave As</returns>
	public static RoutedUICommand SaveAs => _EnsureCommand(CommandId.SaveAs);

	/// <summary> Gets the value that represents the Print command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+PUI TextPrint</returns>
	public static RoutedUICommand Print => _EnsureCommand(CommandId.Print);

	/// <summary>Gets the value that represents the Cancel Print command.</summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextCancel Print</returns>
	public static RoutedUICommand CancelPrint => _EnsureCommand(CommandId.CancelPrint);

	/// <summary> Gets the value that represents the Print Preview command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+F2UI TextPrint Preview</returns>
	public static RoutedUICommand PrintPreview => _EnsureCommand(CommandId.PrintPreview);

	/// <summary>Gets the value that represents the Properties command. </summary>
	/// <returns>The command.Default ValuesKey GestureF4UI TextProperties</returns>
	public static RoutedUICommand Properties => _EnsureCommand(CommandId.Properties);

	/// <summary>Gets the value that represents the Context Menu command. </summary>
	/// <returns>The command.Default ValuesKey GestureShift+F10AppsMouse GestureA Mouse Gesture is not attached to this command, but most applications follow the convention of using the Right Click gesture to invoke the context menu.UI TextContext Menu</returns>
	public static RoutedUICommand ContextMenu => _EnsureCommand(CommandId.ContextMenu);

	/// <summary> Gets the value that represents the Stop command. </summary>
	/// <returns>The command.Default ValuesKey GestureEscUI TextStop</returns>
	public static RoutedUICommand Stop => _EnsureCommand(CommandId.Stop);

	/// <summary> Gets the value that represents the Correction List command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextCorrection List</returns>
	public static RoutedUICommand CorrectionList => _EnsureCommand(CommandId.CorrectionList);

	/// <summary>Represents a command which is always ignored.</summary>
	/// <returns>The command.</returns>
	public static RoutedUICommand NotACommand => _EnsureCommand(CommandId.NotACommand);

	private static string GetPropertyName(CommandId commandId)
	{
		string result = string.Empty;
		switch (commandId)
		{
		case CommandId.Cut:
			result = "Cut";
			break;
		case CommandId.Copy:
			result = "Copy";
			break;
		case CommandId.Paste:
			result = "Paste";
			break;
		case CommandId.Undo:
			result = "Undo";
			break;
		case CommandId.Redo:
			result = "Redo";
			break;
		case CommandId.Delete:
			result = "Delete";
			break;
		case CommandId.Find:
			result = "Find";
			break;
		case CommandId.Replace:
			result = "Replace";
			break;
		case CommandId.Help:
			result = "Help";
			break;
		case CommandId.New:
			result = "New";
			break;
		case CommandId.Open:
			result = "Open";
			break;
		case CommandId.Save:
			result = "Save";
			break;
		case CommandId.SaveAs:
			result = "SaveAs";
			break;
		case CommandId.Close:
			result = "Close";
			break;
		case CommandId.Print:
			result = "Print";
			break;
		case CommandId.CancelPrint:
			result = "CancelPrint";
			break;
		case CommandId.PrintPreview:
			result = "PrintPreview";
			break;
		case CommandId.Properties:
			result = "Properties";
			break;
		case CommandId.ContextMenu:
			result = "ContextMenu";
			break;
		case CommandId.CorrectionList:
			result = "CorrectionList";
			break;
		case CommandId.SelectAll:
			result = "SelectAll";
			break;
		case CommandId.Stop:
			result = "Stop";
			break;
		case CommandId.NotACommand:
			result = "NotACommand";
			break;
		}
		return result;
	}

	internal static string GetUIText(byte commandId)
	{
		string result = string.Empty;
		switch ((CommandId)commandId)
		{
		case CommandId.Cut:
			result = SR.CutText;
			break;
		case CommandId.Copy:
			result = SR.CopyText;
			break;
		case CommandId.Paste:
			result = SR.PasteText;
			break;
		case CommandId.Undo:
			result = SR.UndoText;
			break;
		case CommandId.Redo:
			result = SR.RedoText;
			break;
		case CommandId.Delete:
			result = SR.DeleteText;
			break;
		case CommandId.Find:
			result = SR.FindText;
			break;
		case CommandId.Replace:
			result = SR.ReplaceText;
			break;
		case CommandId.SelectAll:
			result = SR.SelectAllText;
			break;
		case CommandId.Help:
			result = SR.HelpText;
			break;
		case CommandId.New:
			result = SR.NewText;
			break;
		case CommandId.Open:
			result = SR.OpenText;
			break;
		case CommandId.Save:
			result = SR.SaveText;
			break;
		case CommandId.SaveAs:
			result = SR.SaveAsText;
			break;
		case CommandId.Print:
			result = SR.PrintText;
			break;
		case CommandId.CancelPrint:
			result = SR.CancelPrintText;
			break;
		case CommandId.PrintPreview:
			result = SR.PrintPreviewText;
			break;
		case CommandId.Close:
			result = SR.CloseText;
			break;
		case CommandId.ContextMenu:
			result = SR.ContextMenuText;
			break;
		case CommandId.CorrectionList:
			result = SR.CorrectionListText;
			break;
		case CommandId.Properties:
			result = SR.PropertiesText;
			break;
		case CommandId.Stop:
			result = SR.StopText;
			break;
		case CommandId.NotACommand:
			result = SR.NotACommandText;
			break;
		}
		return result;
	}

	internal static InputGestureCollection LoadDefaultGestureFromResource(byte commandId)
	{
		InputGestureCollection inputGestureCollection = new InputGestureCollection();
		switch ((CommandId)commandId)
		{
		case CommandId.Cut:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+X;Shift+Delete", SR.CutKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Copy:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+C;Ctrl+Insert", SR.CopyKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Paste:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+V;Shift+Insert", SR.PasteKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Undo:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+Z", SR.UndoKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Redo:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+Y", SR.RedoKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Delete:
			KeyGesture.AddGesturesFromResourceStrings("Del", SR.DeleteKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Find:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+F", SR.FindKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Replace:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+H", SR.ReplaceKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.SelectAll:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+A", SR.SelectAllKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Help:
			KeyGesture.AddGesturesFromResourceStrings("F1", SR.HelpKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.New:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+N", SR.NewKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Open:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+O", SR.OpenKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Save:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+S", SR.SaveKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Print:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+P", SR.PrintKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.PrintPreview:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+F2", SR.PrintPreviewKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.ContextMenu:
			KeyGesture.AddGesturesFromResourceStrings("Shift+F10;Apps", SR.ContextMenuKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.CorrectionList:
			KeyGesture.AddGesturesFromResourceStrings(SR.CorrectionListKey, SR.CorrectionListKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Properties:
			KeyGesture.AddGesturesFromResourceStrings("F4", SR.PropertiesKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Stop:
			KeyGesture.AddGesturesFromResourceStrings("Esc", SR.StopKeyDisplayString, inputGestureCollection);
			break;
		}
		return inputGestureCollection;
	}

	private static RoutedUICommand _EnsureCommand(CommandId idCommand)
	{
		if ((int)idCommand >= 0 && (int)idCommand < 23)
		{
			lock (_internalCommands.SyncRoot)
			{
				if (_internalCommands[(uint)idCommand] == null)
				{
					RoutedUICommand routedUICommand = CommandLibraryHelper.CreateUICommand(GetPropertyName(idCommand), typeof(ApplicationCommands), (byte)idCommand);
					_internalCommands[(uint)idCommand] = routedUICommand;
				}
			}
			return _internalCommands[(uint)idCommand];
		}
		return null;
	}
}
