using MS.Internal;

namespace System.Windows.Input;

internal sealed class CommandDevice : InputDevice
{
	internal static readonly RoutedEvent CommandDeviceEvent = EventManager.RegisterRoutedEvent("CommandDevice", RoutingStrategy.Bubble, typeof(CommandDeviceEventHandler), typeof(CommandDevice));

	private SecurityCriticalData<InputManager> _inputManager;

	public override IInputElement Target
	{
		get
		{
			VerifyAccess();
			return Keyboard.FocusedElement;
		}
	}

	public override PresentationSource ActiveSource => null;

	internal CommandDevice(InputManager inputManager)
	{
		_inputManager = new SecurityCriticalData<InputManager>(inputManager);
		_inputManager.Value.PreProcessInput += PreProcessInput;
		_inputManager.Value.PostProcessInput += PostProcessInput;
	}

	private void PreProcessInput(object sender, PreProcessInputEventArgs e)
	{
		if (e.StagingItem.Input is InputReportEventArgs inputReportEventArgs && inputReportEventArgs.Report.Type == InputType.Command && inputReportEventArgs.Report is RawAppCommandInputReport rawAppCommandInputReport)
		{
			inputReportEventArgs.Device = this;
			inputReportEventArgs.Source = GetSourceFromDevice(rawAppCommandInputReport.Device);
		}
	}

	private void PostProcessInput(object sender, ProcessInputEventArgs e)
	{
		if (e.StagingItem.Input.RoutedEvent == InputManager.InputReportEvent)
		{
			if (!e.StagingItem.Input.Handled && e.StagingItem.Input is InputReportEventArgs { Report: RawAppCommandInputReport report } && e.StagingItem.Input.OriginalSource is IInputElement source)
			{
				RoutedCommand routedCommand = GetRoutedCommand(report.AppCommand);
				if (routedCommand != null)
				{
					CommandDeviceEventArgs commandDeviceEventArgs = new CommandDeviceEventArgs(this, report.Timestamp, routedCommand);
					commandDeviceEventArgs.RoutedEvent = CommandDeviceEvent;
					commandDeviceEventArgs.Source = source;
					e.PushInput(commandDeviceEventArgs, e.StagingItem);
				}
			}
		}
		else if (e.StagingItem.Input.RoutedEvent == Keyboard.KeyUpEvent || e.StagingItem.Input.RoutedEvent == Mouse.MouseUpEvent || e.StagingItem.Input.RoutedEvent == Keyboard.GotKeyboardFocusEvent || e.StagingItem.Input.RoutedEvent == Keyboard.LostKeyboardFocusEvent)
		{
			CommandManager.InvalidateRequerySuggested();
		}
	}

	private RoutedCommand GetRoutedCommand(int appCommandId)
	{
		RoutedCommand result = null;
		switch (appCommandId)
		{
		case 1:
			result = NavigationCommands.BrowseBack;
			break;
		case 2:
			result = NavigationCommands.BrowseForward;
			break;
		case 3:
			result = NavigationCommands.Refresh;
			break;
		case 4:
			result = NavigationCommands.BrowseStop;
			break;
		case 5:
			result = NavigationCommands.Search;
			break;
		case 6:
			result = NavigationCommands.Favorites;
			break;
		case 7:
			result = NavigationCommands.BrowseHome;
			break;
		case 8:
			result = MediaCommands.MuteVolume;
			break;
		case 9:
			result = MediaCommands.DecreaseVolume;
			break;
		case 10:
			result = MediaCommands.IncreaseVolume;
			break;
		case 11:
			result = MediaCommands.NextTrack;
			break;
		case 12:
			result = MediaCommands.PreviousTrack;
			break;
		case 13:
			result = MediaCommands.Stop;
			break;
		case 14:
			result = MediaCommands.TogglePlayPause;
			break;
		case 16:
			result = MediaCommands.Select;
			break;
		case 19:
			result = MediaCommands.DecreaseBass;
			break;
		case 20:
			result = MediaCommands.BoostBass;
			break;
		case 21:
			result = MediaCommands.IncreaseBass;
			break;
		case 22:
			result = MediaCommands.DecreaseTreble;
			break;
		case 23:
			result = MediaCommands.IncreaseTreble;
			break;
		case 24:
			result = MediaCommands.MuteMicrophoneVolume;
			break;
		case 25:
			result = MediaCommands.DecreaseMicrophoneVolume;
			break;
		case 26:
			result = MediaCommands.IncreaseMicrophoneVolume;
			break;
		case 27:
			result = ApplicationCommands.Help;
			break;
		case 28:
			result = ApplicationCommands.Find;
			break;
		case 29:
			result = ApplicationCommands.New;
			break;
		case 30:
			result = ApplicationCommands.Open;
			break;
		case 31:
			result = ApplicationCommands.Close;
			break;
		case 32:
			result = ApplicationCommands.Save;
			break;
		case 33:
			result = ApplicationCommands.Print;
			break;
		case 34:
			result = ApplicationCommands.Undo;
			break;
		case 35:
			result = ApplicationCommands.Redo;
			break;
		case 36:
			result = ApplicationCommands.Copy;
			break;
		case 37:
			result = ApplicationCommands.Cut;
			break;
		case 38:
			result = ApplicationCommands.Paste;
			break;
		case 44:
			result = MediaCommands.ToggleMicrophoneOnOff;
			break;
		case 45:
			result = ApplicationCommands.CorrectionList;
			break;
		case 46:
			result = MediaCommands.Play;
			break;
		case 47:
			result = MediaCommands.Pause;
			break;
		case 48:
			result = MediaCommands.Record;
			break;
		case 49:
			result = MediaCommands.FastForward;
			break;
		case 50:
			result = MediaCommands.Rewind;
			break;
		case 51:
			result = MediaCommands.ChannelUp;
			break;
		case 52:
			result = MediaCommands.ChannelDown;
			break;
		}
		return result;
	}

	private IInputElement GetSourceFromDevice(InputType device)
	{
		if (device == InputType.Mouse)
		{
			return Mouse.DirectlyOver;
		}
		return Keyboard.FocusedElement;
	}
}
