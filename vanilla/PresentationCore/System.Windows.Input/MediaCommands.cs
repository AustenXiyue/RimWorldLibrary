using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Provides a standard set of media related commands.</summary>
public static class MediaCommands
{
	private enum CommandId : byte
	{
		Play = 1,
		Pause,
		Stop,
		Record,
		NextTrack,
		PreviousTrack,
		FastForward,
		Rewind,
		ChannelUp,
		ChannelDown,
		TogglePlayPause,
		IncreaseVolume,
		DecreaseVolume,
		MuteVolume,
		IncreaseTreble,
		DecreaseTreble,
		IncreaseBass,
		DecreaseBass,
		BoostBass,
		IncreaseMicrophoneVolume,
		DecreaseMicrophoneVolume,
		MuteMicrophoneVolume,
		ToggleMicrophoneOnOff,
		Select,
		Last
	}

	private static RoutedUICommand[] _internalCommands = new RoutedUICommand[25];

	/// <summary> Gets the value that represents the Play command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextPlay</returns>
	public static RoutedUICommand Play => _EnsureCommand(CommandId.Play);

	/// <summary> Gets the value that represents the Pause command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextPause</returns>
	public static RoutedUICommand Pause => _EnsureCommand(CommandId.Pause);

	/// <summary> Gets the value that represents the Stop command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextStop</returns>
	public static RoutedUICommand Stop => _EnsureCommand(CommandId.Stop);

	/// <summary> Gets the value that represents the Record command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextRecord</returns>
	public static RoutedUICommand Record => _EnsureCommand(CommandId.Record);

	/// <summary> Gets the value that represents the Next Track command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextNext Track</returns>
	public static RoutedUICommand NextTrack => _EnsureCommand(CommandId.NextTrack);

	/// <summary> Gets the value that represents the Previous Track command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextPrevious Track</returns>
	public static RoutedUICommand PreviousTrack => _EnsureCommand(CommandId.PreviousTrack);

	/// <summary> Gets the value that represents the Fast Forward command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextFast Forward</returns>
	public static RoutedUICommand FastForward => _EnsureCommand(CommandId.FastForward);

	/// <summary> Gets the value that represents the Rewind command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextRewind</returns>
	public static RoutedUICommand Rewind => _EnsureCommand(CommandId.Rewind);

	/// <summary> Gets the value that represents the Channel Up command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextChannel Up</returns>
	public static RoutedUICommand ChannelUp => _EnsureCommand(CommandId.ChannelUp);

	/// <summary> Gets the value that represents the Channel Down command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextChannel Down</returns>
	public static RoutedUICommand ChannelDown => _EnsureCommand(CommandId.ChannelDown);

	/// <summary> Gets the value that represents the Toggle Play Pause command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextToggle Play Pause</returns>
	public static RoutedUICommand TogglePlayPause => _EnsureCommand(CommandId.TogglePlayPause);

	/// <summary> Gets the value that represents the Select command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextSelect</returns>
	public static RoutedUICommand Select => _EnsureCommand(CommandId.Select);

	/// <summary> Gets the value that represents the Increase Volume command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextIncrease Volume</returns>
	public static RoutedUICommand IncreaseVolume => _EnsureCommand(CommandId.IncreaseVolume);

	/// <summary> Gets the value that represents the Decrease Volume command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextDecrease Volume</returns>
	public static RoutedUICommand DecreaseVolume => _EnsureCommand(CommandId.DecreaseVolume);

	/// <summary> Gets the value that represents the Mute Volume command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextMute Volume</returns>
	public static RoutedUICommand MuteVolume => _EnsureCommand(CommandId.MuteVolume);

	/// <summary> Gets the value that represents the Increase Treble command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextIncrease Treble</returns>
	public static RoutedUICommand IncreaseTreble => _EnsureCommand(CommandId.IncreaseTreble);

	/// <summary> Gets the value that represents the Decrease Treble command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextDecrease Treble</returns>
	public static RoutedUICommand DecreaseTreble => _EnsureCommand(CommandId.DecreaseTreble);

	/// <summary> Gets the value that represents the Increase Bass command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextIncrease Bass</returns>
	public static RoutedUICommand IncreaseBass => _EnsureCommand(CommandId.IncreaseBass);

	/// <summary> Gets the value that represents the Decrease Bass command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextDecrease Bass</returns>
	public static RoutedUICommand DecreaseBass => _EnsureCommand(CommandId.DecreaseBass);

	/// <summary>Gets the value that represents the Boost Base command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextBoost Bass</returns>
	public static RoutedUICommand BoostBass => _EnsureCommand(CommandId.BoostBass);

	/// <summary> Gets the value that represents the Increase Microphone Volume command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextIncrease Microphone Volume</returns>
	public static RoutedUICommand IncreaseMicrophoneVolume => _EnsureCommand(CommandId.IncreaseMicrophoneVolume);

	/// <summary> Gets the value that represents the Decrease Microphone Volume command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextDecrease Microphone Volume</returns>
	public static RoutedUICommand DecreaseMicrophoneVolume => _EnsureCommand(CommandId.DecreaseMicrophoneVolume);

	/// <summary> Gets the value that represents the Mute Microphone Volume command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextMute Microphone Volume</returns>
	public static RoutedUICommand MuteMicrophoneVolume => _EnsureCommand(CommandId.MuteMicrophoneVolume);

	/// <summary> Gets the value that represents the Toggle Microphone On Off command. </summary>
	/// <returns>The command.Default ValuesKey GestureNo gesture defined.UI TextToggle Microphone OnOff</returns>
	public static RoutedUICommand ToggleMicrophoneOnOff => _EnsureCommand(CommandId.ToggleMicrophoneOnOff);

	private static string GetPropertyName(CommandId commandId)
	{
		string result = string.Empty;
		switch (commandId)
		{
		case CommandId.Play:
			result = "Play";
			break;
		case CommandId.Pause:
			result = "Pause";
			break;
		case CommandId.Stop:
			result = "Stop";
			break;
		case CommandId.Record:
			result = "Record";
			break;
		case CommandId.NextTrack:
			result = "NextTrack";
			break;
		case CommandId.PreviousTrack:
			result = "PreviousTrack";
			break;
		case CommandId.FastForward:
			result = "FastForward";
			break;
		case CommandId.Rewind:
			result = "Rewind";
			break;
		case CommandId.ChannelUp:
			result = "ChannelUp";
			break;
		case CommandId.ChannelDown:
			result = "ChannelDown";
			break;
		case CommandId.TogglePlayPause:
			result = "TogglePlayPause";
			break;
		case CommandId.IncreaseVolume:
			result = "IncreaseVolume";
			break;
		case CommandId.DecreaseVolume:
			result = "DecreaseVolume";
			break;
		case CommandId.MuteVolume:
			result = "MuteVolume";
			break;
		case CommandId.IncreaseTreble:
			result = "IncreaseTreble";
			break;
		case CommandId.DecreaseTreble:
			result = "DecreaseTreble";
			break;
		case CommandId.IncreaseBass:
			result = "IncreaseBass";
			break;
		case CommandId.DecreaseBass:
			result = "DecreaseBass";
			break;
		case CommandId.BoostBass:
			result = "BoostBass";
			break;
		case CommandId.IncreaseMicrophoneVolume:
			result = "IncreaseMicrophoneVolume";
			break;
		case CommandId.DecreaseMicrophoneVolume:
			result = "DecreaseMicrophoneVolume";
			break;
		case CommandId.MuteMicrophoneVolume:
			result = "MuteMicrophoneVolume";
			break;
		case CommandId.ToggleMicrophoneOnOff:
			result = "ToggleMicrophoneOnOff";
			break;
		case CommandId.Select:
			result = "Select";
			break;
		}
		return result;
	}

	internal static string GetUIText(byte commandId)
	{
		string result = string.Empty;
		switch ((CommandId)commandId)
		{
		case CommandId.Play:
			result = SR.MediaPlayText;
			break;
		case CommandId.Pause:
			result = SR.MediaPauseText;
			break;
		case CommandId.Stop:
			result = SR.MediaStopText;
			break;
		case CommandId.Record:
			result = SR.MediaRecordText;
			break;
		case CommandId.NextTrack:
			result = SR.MediaNextTrackText;
			break;
		case CommandId.PreviousTrack:
			result = SR.MediaPreviousTrackText;
			break;
		case CommandId.FastForward:
			result = SR.MediaFastForwardText;
			break;
		case CommandId.Rewind:
			result = SR.MediaRewindText;
			break;
		case CommandId.ChannelUp:
			result = SR.MediaChannelUpText;
			break;
		case CommandId.ChannelDown:
			result = SR.MediaChannelDownText;
			break;
		case CommandId.TogglePlayPause:
			result = SR.MediaTogglePlayPauseText;
			break;
		case CommandId.IncreaseVolume:
			result = SR.MediaIncreaseVolumeText;
			break;
		case CommandId.DecreaseVolume:
			result = SR.MediaDecreaseVolumeText;
			break;
		case CommandId.MuteVolume:
			result = SR.MediaMuteVolumeText;
			break;
		case CommandId.IncreaseTreble:
			result = SR.MediaIncreaseTrebleText;
			break;
		case CommandId.DecreaseTreble:
			result = SR.MediaDecreaseTrebleText;
			break;
		case CommandId.IncreaseBass:
			result = SR.MediaIncreaseBassText;
			break;
		case CommandId.DecreaseBass:
			result = SR.MediaDecreaseBassText;
			break;
		case CommandId.BoostBass:
			result = SR.MediaBoostBassText;
			break;
		case CommandId.IncreaseMicrophoneVolume:
			result = SR.MediaIncreaseMicrophoneVolumeText;
			break;
		case CommandId.DecreaseMicrophoneVolume:
			result = SR.MediaDecreaseMicrophoneVolumeText;
			break;
		case CommandId.MuteMicrophoneVolume:
			result = SR.MediaMuteMicrophoneVolumeText;
			break;
		case CommandId.ToggleMicrophoneOnOff:
			result = SR.MediaToggleMicrophoneOnOffText;
			break;
		case CommandId.Select:
			result = SR.MediaSelectText;
			break;
		}
		return result;
	}

	internal static InputGestureCollection LoadDefaultGestureFromResource(byte commandId)
	{
		InputGestureCollection inputGestureCollection = new InputGestureCollection();
		switch ((CommandId)commandId)
		{
		case CommandId.Play:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaPlayKey, SR.MediaPlayKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Pause:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaPauseKey, SR.MediaPauseKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Stop:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaStopKey, SR.MediaStopKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Record:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaRecordKey, SR.MediaRecordKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.NextTrack:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaNextTrackKey, SR.MediaNextTrackKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.PreviousTrack:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaPreviousTrackKey, SR.MediaPreviousTrackKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.FastForward:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaFastForwardKey, SR.MediaFastForwardKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Rewind:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaRewindKey, SR.MediaRewindKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.ChannelUp:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaChannelUpKey, SR.MediaChannelUpKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.ChannelDown:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaChannelDownKey, SR.MediaChannelDownKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.TogglePlayPause:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaTogglePlayPauseKey, SR.MediaTogglePlayPauseKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.IncreaseVolume:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaIncreaseVolumeKey, SR.MediaIncreaseVolumeKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.DecreaseVolume:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaDecreaseVolumeKey, SR.MediaDecreaseVolumeKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MuteVolume:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaMuteVolumeKey, SR.MediaMuteVolumeKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.IncreaseTreble:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaIncreaseTrebleKey, SR.MediaIncreaseTrebleKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.DecreaseTreble:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaDecreaseTrebleKey, SR.MediaDecreaseTrebleKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.IncreaseBass:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaIncreaseBassKey, SR.MediaIncreaseBassKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.DecreaseBass:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaDecreaseBassKey, SR.MediaDecreaseBassKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.BoostBass:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaBoostBassKey, SR.MediaBoostBassKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.IncreaseMicrophoneVolume:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaIncreaseMicrophoneVolumeKey, SR.MediaIncreaseMicrophoneVolumeKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.DecreaseMicrophoneVolume:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaDecreaseMicrophoneVolumeKey, SR.MediaDecreaseMicrophoneVolumeKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.MuteMicrophoneVolume:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaMuteMicrophoneVolumeKey, SR.MediaMuteMicrophoneVolumeKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.ToggleMicrophoneOnOff:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaToggleMicrophoneOnOffKey, SR.MediaToggleMicrophoneOnOffKeyDisplayString, inputGestureCollection);
			break;
		case CommandId.Select:
			KeyGesture.AddGesturesFromResourceStrings(SR.MediaSelectKey, SR.MediaSelectKeyDisplayString, inputGestureCollection);
			break;
		}
		return inputGestureCollection;
	}

	private static RoutedUICommand _EnsureCommand(CommandId idCommand)
	{
		if ((int)idCommand >= 0 && (int)idCommand < 25)
		{
			lock (_internalCommands.SyncRoot)
			{
				if (_internalCommands[(uint)idCommand] == null)
				{
					RoutedUICommand routedUICommand = new RoutedUICommand(GetPropertyName(idCommand), typeof(MediaCommands), (byte)idCommand);
					routedUICommand.AreInputGesturesDelayLoaded = true;
					_internalCommands[(uint)idCommand] = routedUICommand;
				}
			}
			return _internalCommands[(uint)idCommand];
		}
		return null;
	}
}
