namespace System.Windows.Controls;

/// <summary>Specifies the states that can be applied to a <see cref="T:System.Windows.Controls.MediaElement" /> for the <see cref="P:System.Windows.Controls.MediaElement.LoadedBehavior" /> and <see cref="P:System.Windows.Controls.MediaElement.UnloadedBehavior" /> properties.</summary>
public enum MediaState
{
	/// <summary>The state used to control a <see cref="T:System.Windows.Controls.MediaElement" /> manually. Interactive methods like <see cref="M:System.Windows.Controls.MediaElement.Play" /> and <see cref="M:System.Windows.Controls.MediaElement.Pause" /> can be used. Media will preroll but not play when the <see cref="T:System.Windows.Controls.MediaElement" /> is assigned valid media source.</summary>
	Manual,
	/// <summary>The state used to play the media. . Media will preroll automatically being playback when the <see cref="T:System.Windows.Controls.MediaElement" /> is assigned valid media source.</summary>
	Play,
	/// <summary>The state used to close the media. All media resources are released (including video memory).</summary>
	Close,
	/// <summary>The state used to pause the media. Media will preroll but remains paused when the <see cref="T:System.Windows.Controls.MediaElement" /> is assigned valid media source.</summary>
	Pause,
	/// <summary>The state used to stop the media. Media will preroll but not play when the <see cref="T:System.Windows.Controls.MediaElement" /> is assigned valid media source. Media resources are not released.</summary>
	Stop
}
