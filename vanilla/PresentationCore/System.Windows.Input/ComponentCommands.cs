using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Provides a standard set of component-related commands, which have predefined key input gestures and <see cref="P:System.Windows.Input.RoutedUICommand.Text" /> properties.</summary>
public static class ComponentCommands
{
	private enum CommandId : byte
	{
		ScrollPageUp = 1,
		ScrollPageDown,
		ScrollPageLeft,
		ScrollPageRight,
		ScrollByLine,
		MoveLeft,
		MoveRight,
		MoveUp,
		MoveDown,
		MoveToHome,
		MoveToEnd,
		MoveToPageUp,
		MoveToPageDown,
		SelectToHome,
		SelectToEnd,
		SelectToPageUp,
		SelectToPageDown,
		MoveFocusUp,
		MoveFocusDown,
		MoveFocusForward,
		MoveFocusBack,
		MoveFocusPageUp,
		MoveFocusPageDown,
		ExtendSelectionLeft,
		ExtendSelectionRight,
		ExtendSelectionUp,
		ExtendSelectionDown,
		Last
	}

	private static RoutedUICommand[] _internalCommands = new RoutedUICommand[28];

	private const string ExtendSelectionDownKey = "Shift+Down";

	private const string ExtendSelectionLeftKey = "Shift+Left";

	private const string ExtendSelectionRightKey = "Shift+Right";

	private const string ExtendSelectionUpKey = "Shift+Up";

	private const string MoveDownKey = "Down";

	private const string MoveFocusBackKey = "Ctrl+Left";

	private const string MoveFocusDownKey = "Ctrl+Down";

	private const string MoveFocusForwardKey = "Ctrl+Right";

	private const string MoveFocusPageDownKey = "Ctrl+PageDown";

	private const string MoveFocusPageUpKey = "Ctrl+PageUp";

	private const string MoveFocusUpKey = "Ctrl+Up";

	private const string MoveLeftKey = "Left";

	private const string MoveRightKey = "Right";

	private const string MoveToEndKey = "End";

	private const string MoveToHomeKey = "Home";

	private const string MoveToPageDownKey = "PageDown";

	private const string MoveToPageUpKey = "PageUp";

	private const string MoveUpKey = "Up";

	private const string ScrollPageDownKey = "PageDown";

	private const string ScrollPageUpKey = "PageUp";

	private const string SelectToEndKey = "Shift+End";

	private const string SelectToHomeKey = "Shift+Home";

	private const string SelectToPageDownKey = "Shift+PageDown";

	private const string SelectToPageUpKey = "Shift+PageUp";

	/// <summary>Gets the value that represents the Scroll Page Up command. </summary>
	/// <returns>The command.Default ValuesKey GesturePageUpUI TextScroll Page Up</returns>
	public static RoutedUICommand ScrollPageUp => _EnsureCommand(CommandId.ScrollPageUp);

	/// <summary>Gets the value that represents the Scroll Page Down command. </summary>
	/// <returns>The command.Default ValuesKey GesturePageDownUI TextScroll Page Down</returns>
	public static RoutedUICommand ScrollPageDown => _EnsureCommand(CommandId.ScrollPageDown);

	/// <summary>Gets the value that represents the Scroll Page Left command.</summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextScroll Page Left</returns>
	public static RoutedUICommand ScrollPageLeft => _EnsureCommand(CommandId.ScrollPageLeft);

	/// <summary>Gets the value that represents the Scroll Page Right command.</summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextScroll Page Right</returns>
	public static RoutedUICommand ScrollPageRight => _EnsureCommand(CommandId.ScrollPageRight);

	/// <summary>Gets the value that represents the Scroll By Line command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture definedUI TextScroll By Line</returns>
	public static RoutedUICommand ScrollByLine => _EnsureCommand(CommandId.ScrollByLine);

	/// <summary>Gets the value that represents the Move Left command. </summary>
	/// <returns>The command.Default ValuesKey GestureLeftUI TextMove Left</returns>
	public static RoutedUICommand MoveLeft => _EnsureCommand(CommandId.MoveLeft);

	/// <summary>Gets the value that represents the Move Right command. </summary>
	/// <returns>The command.Default ValuesKey GestureRightUI TextMove Right</returns>
	public static RoutedUICommand MoveRight => _EnsureCommand(CommandId.MoveRight);

	/// <summary>Gets the value that represents the Move Up command. </summary>
	/// <returns>The command.Default ValuesKey GestureUpUI TextMove Up</returns>
	public static RoutedUICommand MoveUp => _EnsureCommand(CommandId.MoveUp);

	/// <summary>Gets the value that represents the Move Down command. </summary>
	/// <returns>The command.Default ValuesKey GestureDownUI TextMove Down</returns>
	public static RoutedUICommand MoveDown => _EnsureCommand(CommandId.MoveDown);

	/// <summary>Gets the value that represents the Move To Home command. </summary>
	/// <returns>The command.Default ValuesKey GestureHomeUI TextMove To Home</returns>
	public static RoutedUICommand MoveToHome => _EnsureCommand(CommandId.MoveToHome);

	/// <summary>Gets the value that represents the Move To End command. </summary>
	/// <returns>The command.Default ValuesKey GestureEndUI TextMove To End</returns>
	public static RoutedUICommand MoveToEnd => _EnsureCommand(CommandId.MoveToEnd);

	/// <summary>Gets the value that represents the Move To Page Up command. </summary>
	/// <returns>The command.Default ValuesKey GesturePageUpUI TextMove To Page Up</returns>
	public static RoutedUICommand MoveToPageUp => _EnsureCommand(CommandId.MoveToPageUp);

	/// <summary>Gets the value that represents the Move To Page Down command. </summary>
	/// <returns>The command.Default ValuesKey GesturePageDownUI TextMove To Page Down</returns>
	public static RoutedUICommand MoveToPageDown => _EnsureCommand(CommandId.MoveToPageDown);

	/// <summary>Gets the value that represents the Extend Selection Up command. </summary>
	/// <returns>The command.Default ValuesKey GestureShift+UpUI TextExtend Selection Up</returns>
	public static RoutedUICommand ExtendSelectionUp => _EnsureCommand(CommandId.ExtendSelectionUp);

	/// <summary>Gets the value that represents the Extend Selection Down command. </summary>
	/// <returns>The command.Default ValuesKey GestureShift+DownUI TextExtend Selection Down</returns>
	public static RoutedUICommand ExtendSelectionDown => _EnsureCommand(CommandId.ExtendSelectionDown);

	/// <summary>Gets the value that represents the Extend Selection Left command. </summary>
	/// <returns>The command.Default ValuesKey GestureShift+LeftUI TextExtend Selection Left</returns>
	public static RoutedUICommand ExtendSelectionLeft => _EnsureCommand(CommandId.ExtendSelectionLeft);

	/// <summary>Gets the value that represents the Extend Selection Right command. </summary>
	/// <returns>The command.Default ValuesKey GestureShift+RightUI TextExtend Selection Right</returns>
	public static RoutedUICommand ExtendSelectionRight => _EnsureCommand(CommandId.ExtendSelectionRight);

	/// <summary>Gets the value that represents the Select To Home command. </summary>
	/// <returns>The command.Default ValuesKey GestureShift+HomeUI TextSelect To Home</returns>
	public static RoutedUICommand SelectToHome => _EnsureCommand(CommandId.SelectToHome);

	/// <summary>Gets the value that represents the Select To End command. </summary>
	/// <returns>The command.Default ValuesKey GestureShift+EndUI TextSelect To End</returns>
	public static RoutedUICommand SelectToEnd => _EnsureCommand(CommandId.SelectToEnd);

	/// <summary>Gets the value that represents the Select To Page Up command. </summary>
	/// <returns>The command.Default ValuesKey GestureShift+PageUpUI TextSelect To Page Up</returns>
	public static RoutedUICommand SelectToPageUp => _EnsureCommand(CommandId.SelectToPageUp);

	/// <summary>Gets the value that represents the Select To Page Down command. </summary>
	/// <returns>The command.Default ValuesKey GestureShift+PageDownUI TextSelect To Page Down</returns>
	public static RoutedUICommand SelectToPageDown => _EnsureCommand(CommandId.SelectToPageDown);

	/// <summary>Gets the value that represents the Move Focus Up command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+UpUI TextMove Focus Up</returns>
	public static RoutedUICommand MoveFocusUp => _EnsureCommand(CommandId.MoveFocusUp);

	/// <summary>Gets the value that represents the Move Focus Down command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+DownUI TextMove Focus Down</returns>
	public static RoutedUICommand MoveFocusDown => _EnsureCommand(CommandId.MoveFocusDown);

	/// <summary>Gets the value that represents the Move Focus Forward command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+RightUI TextMove Focus Forward</returns>
	public static RoutedUICommand MoveFocusForward => _EnsureCommand(CommandId.MoveFocusForward);

	/// <summary>Gets the value that represents the Move Focus Back command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+LeftUI TextMove Focus Back</returns>
	public static RoutedUICommand MoveFocusBack => _EnsureCommand(CommandId.MoveFocusBack);

	/// <summary>Gets the value that represents the Move Focus Page Up command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+PageUpUI TextMove Focus Page Up</returns>
	public static RoutedUICommand MoveFocusPageUp => _EnsureCommand(CommandId.MoveFocusPageUp);

	/// <summary>Gets the value that represents the Move Focus Page Down command. </summary>
	/// <returns>The command.Default ValuesKey GestureCtrl+PageDownUI TextMove Focus Page Down</returns>
	public static RoutedUICommand MoveFocusPageDown => _EnsureCommand(CommandId.MoveFocusPageDown);

	private static string GetPropertyName(CommandId commandId)
	{
		string result = string.Empty;
		switch (commandId)
		{
		case CommandId.ScrollPageUp:
			result = "ScrollPageUp";
			break;
		case CommandId.ScrollPageDown:
			result = "ScrollPageDown";
			break;
		case CommandId.ScrollPageLeft:
			result = "ScrollPageLeft";
			break;
		case CommandId.ScrollPageRight:
			result = "ScrollPageRight";
			break;
		case CommandId.ScrollByLine:
			result = "ScrollByLine";
			break;
		case CommandId.MoveLeft:
			result = "MoveLeft";
			break;
		case CommandId.MoveRight:
			result = "MoveRight";
			break;
		case CommandId.MoveUp:
			result = "MoveUp";
			break;
		case CommandId.MoveDown:
			result = "MoveDown";
			break;
		case CommandId.ExtendSelectionUp:
			result = "ExtendSelectionUp";
			break;
		case CommandId.ExtendSelectionDown:
			result = "ExtendSelectionDown";
			break;
		case CommandId.ExtendSelectionLeft:
			result = "ExtendSelectionLeft";
			break;
		case CommandId.ExtendSelectionRight:
			result = "ExtendSelectionRight";
			break;
		case CommandId.MoveToHome:
			result = "MoveToHome";
			break;
		case CommandId.MoveToEnd:
			result = "MoveToEnd";
			break;
		case CommandId.MoveToPageUp:
			result = "MoveToPageUp";
			break;
		case CommandId.MoveToPageDown:
			result = "MoveToPageDown";
			break;
		case CommandId.SelectToHome:
			result = "SelectToHome";
			break;
		case CommandId.SelectToEnd:
			result = "SelectToEnd";
			break;
		case CommandId.SelectToPageDown:
			result = "SelectToPageDown";
			break;
		case CommandId.SelectToPageUp:
			result = "SelectToPageUp";
			break;
		case CommandId.MoveFocusUp:
			result = "MoveFocusUp";
			break;
		case CommandId.MoveFocusDown:
			result = "MoveFocusDown";
			break;
		case CommandId.MoveFocusBack:
			result = "MoveFocusBack";
			break;
		case CommandId.MoveFocusForward:
			result = "MoveFocusForward";
			break;
		case CommandId.MoveFocusPageUp:
			result = "MoveFocusPageUp";
			break;
		case CommandId.MoveFocusPageDown:
			result = "MoveFocusPageDown";
			break;
		}
		return result;
	}

	internal static string GetUIText(byte commandId)
	{
		string result = string.Empty;
		switch ((CommandId)commandId)
		{
		case CommandId.ScrollPageUp:
			result = SR.ScrollPageUpText;
			break;
		case CommandId.ScrollPageDown:
			result = SR.ScrollPageDownText;
			break;
		case CommandId.ScrollPageLeft:
			result = SR.ScrollPageLeftText;
			break;
		case CommandId.ScrollPageRight:
			result = SR.ScrollPageRightText;
			break;
		case CommandId.ScrollByLine:
			result = SR.ScrollByLineText;
			break;
		case CommandId.MoveLeft:
			result = SR.MoveLeftText;
			break;
		case CommandId.MoveRight:
			result = SR.MoveRightText;
			break;
		case CommandId.MoveUp:
			result = SR.MoveUpText;
			break;
		case CommandId.MoveDown:
			result = SR.MoveDownText;
			break;
		case CommandId.ExtendSelectionUp:
			result = SR.ExtendSelectionUpText;
			break;
		case CommandId.ExtendSelectionDown:
			result = SR.ExtendSelectionDownText;
			break;
		case CommandId.ExtendSelectionLeft:
			result = SR.ExtendSelectionLeftText;
			break;
		case CommandId.ExtendSelectionRight:
			result = SR.ExtendSelectionRightText;
			break;
		case CommandId.MoveToHome:
			result = SR.MoveToHomeText;
			break;
		case CommandId.MoveToEnd:
			result = SR.MoveToEndText;
			break;
		case CommandId.MoveToPageUp:
			result = SR.MoveToPageUpText;
			break;
		case CommandId.MoveToPageDown:
			result = SR.MoveToPageDownText;
			break;
		case CommandId.SelectToHome:
			result = SR.SelectToHomeText;
			break;
		case CommandId.SelectToEnd:
			result = SR.SelectToEndText;
			break;
		case CommandId.SelectToPageDown:
			result = SR.SelectToPageDownText;
			break;
		case CommandId.SelectToPageUp:
			result = SR.SelectToPageUpText;
			break;
		case CommandId.MoveFocusUp:
			result = SR.MoveFocusUpText;
			break;
		case CommandId.MoveFocusDown:
			result = SR.MoveFocusDownText;
			break;
		case CommandId.MoveFocusBack:
			result = SR.MoveFocusBackText;
			break;
		case CommandId.MoveFocusForward:
			result = SR.MoveFocusForwardText;
			break;
		case CommandId.MoveFocusPageUp:
			result = SR.MoveFocusPageUpText;
			break;
		case CommandId.MoveFocusPageDown:
			result = SR.MoveFocusPageDownText;
			break;
		}
		return result;
	}

	internal static InputGestureCollection LoadDefaultGestureFromResource(byte commandId)
	{
		InputGestureCollection inputGestureCollection = new InputGestureCollection();
		switch ((CommandId)commandId)
		{
		case CommandId.ScrollPageUp:
			KeyGesture.AddGesturesFromResourceStrings("PageUp", SR.ScrollPageUpKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.ScrollPageDown:
			KeyGesture.AddGesturesFromResourceStrings("PageDown", SR.ScrollPageDownKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.ScrollPageLeft:
			KeyGesture.AddGesturesFromResourceStrings(SR.ScrollPageLeftKey, SR.ScrollPageLeftKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.ScrollPageRight:
			KeyGesture.AddGesturesFromResourceStrings(SR.ScrollPageRightKey, SR.ScrollPageRightKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.ScrollByLine:
			KeyGesture.AddGesturesFromResourceStrings(SR.ScrollByLineKey, SR.ScrollByLineKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveLeft:
			KeyGesture.AddGesturesFromResourceStrings("Left", SR.MoveLeftKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveRight:
			KeyGesture.AddGesturesFromResourceStrings("Right", SR.MoveRightKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveUp:
			KeyGesture.AddGesturesFromResourceStrings("Up", SR.MoveUpKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveDown:
			KeyGesture.AddGesturesFromResourceStrings("Down", SR.MoveDownKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.ExtendSelectionUp:
			KeyGesture.AddGesturesFromResourceStrings("Shift+Up", SR.ExtendSelectionUpKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.ExtendSelectionDown:
			KeyGesture.AddGesturesFromResourceStrings("Shift+Down", SR.ExtendSelectionDownKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.ExtendSelectionLeft:
			KeyGesture.AddGesturesFromResourceStrings("Shift+Left", SR.ExtendSelectionLeftKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.ExtendSelectionRight:
			KeyGesture.AddGesturesFromResourceStrings("Shift+Right", SR.ExtendSelectionRightKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveToHome:
			KeyGesture.AddGesturesFromResourceStrings("Home", SR.MoveToHomeKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveToEnd:
			KeyGesture.AddGesturesFromResourceStrings("End", SR.MoveToEndKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveToPageUp:
			KeyGesture.AddGesturesFromResourceStrings("PageUp", SR.MoveToPageUpKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveToPageDown:
			KeyGesture.AddGesturesFromResourceStrings("PageDown", SR.MoveToPageDownKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.SelectToHome:
			KeyGesture.AddGesturesFromResourceStrings("Shift+Home", SR.SelectToHomeKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.SelectToEnd:
			KeyGesture.AddGesturesFromResourceStrings("Shift+End", SR.SelectToEndKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.SelectToPageDown:
			KeyGesture.AddGesturesFromResourceStrings("Shift+PageDown", SR.SelectToPageDownKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.SelectToPageUp:
			KeyGesture.AddGesturesFromResourceStrings("Shift+PageUp", SR.SelectToPageUpKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveFocusUp:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+Up", SR.MoveFocusUpKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveFocusDown:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+Down", SR.MoveFocusDownKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveFocusBack:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+Left", SR.MoveFocusBackKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveFocusForward:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+Right", SR.MoveFocusForwardKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveFocusPageUp:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+PageUp", SR.MoveFocusPageUpKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MoveFocusPageDown:
			KeyGesture.AddGesturesFromResourceStrings("Ctrl+PageDown", SR.MoveFocusPageDownKeyDisplayString, inputGestureCollection);
			break;
		}
		return inputGestureCollection;
	}

	private static RoutedUICommand _EnsureCommand(CommandId idCommand)
	{
		if ((int)idCommand >= 0 && (int)idCommand < 28)
		{
			lock (_internalCommands.SyncRoot)
			{
				if (_internalCommands[(uint)idCommand] == null)
				{
					RoutedUICommand routedUICommand = new RoutedUICommand(GetPropertyName(idCommand), typeof(ComponentCommands), (byte)idCommand);
					routedUICommand.AreInputGesturesDelayLoaded = true;
					_internalCommands[(uint)idCommand] = routedUICommand;
				}
			}
			return _internalCommands[(uint)idCommand];
		}
		return null;
	}
}
