using System.Windows.Media.Animation;
using System.Windows.Media.Composition;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Provides media playback for drawings. </summary>
public class MediaPlayer : Animatable, DUCE.IResource
{
	private MediaPlayerState _mediaPlayerState;

	internal DUCE.ShareableDUCEMultiChannelResource _duceResource = new DUCE.ShareableDUCEMultiChannelResource();

	private EventHandler _newFrameHandler;

	private bool _needsUpdate;

	/// <summary>Gets a value that indicates whether the media is buffering.</summary>
	/// <returns>true if the media is buffering; otherwise, false.</returns>
	public bool IsBuffering
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.IsBuffering;
		}
	}

	/// <summary>Gets a value indicating whether the media can be paused.</summary>
	/// <returns>true if the media can be paused; otherwise, false.</returns>
	public bool CanPause
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.CanPause;
		}
	}

	/// <summary>Gets the percentage of download progress for content located at a remote server.</summary>
	/// <returns>The percentage of download progress for content located at a remote server represented by a value between 0 and 1. The default is 0.</returns>
	public double DownloadProgress
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.DownloadProgress;
		}
	}

	/// <summary>Gets the percentage of buffering completed for streaming content.</summary>
	/// <returns>The percentage of buffering completed for streaming content represented in a value between 0 and 1.</returns>
	public double BufferingProgress
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.BufferingProgress;
		}
	}

	/// <summary>Gets the pixel height of the video. </summary>
	/// <returns>The pixel height of the video. </returns>
	public int NaturalVideoHeight
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.NaturalVideoHeight;
		}
	}

	/// <summary>Gets the pixel width of the video.</summary>
	/// <returns>The pixel width of the video.</returns>
	public int NaturalVideoWidth
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.NaturalVideoWidth;
		}
	}

	/// <summary>Gets a value that indicating whether the media has audio output.</summary>
	/// <returns>true if the media has audio output; otherwise, false.</returns>
	public bool HasAudio
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.HasAudio;
		}
	}

	/// <summary>Gets a value that indicates whether the media has video output.</summary>
	/// <returns>true if the media has video output; otherwise, false.</returns>
	public bool HasVideo
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.HasVideo;
		}
	}

	/// <summary>Gets the media <see cref="T:System.Uri" />.</summary>
	/// <returns>The current media <see cref="T:System.Uri" />.</returns>
	public Uri Source
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.Source;
		}
	}

	/// <summary>Gets or sets the media's volume.</summary>
	/// <returns>The media's volume represented on a linear scale between 0 and 1. The default is 0.5.</returns>
	public double Volume
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.Volume;
		}
		set
		{
			WritePreamble();
			_mediaPlayerState.Volume = value;
		}
	}

	/// <summary>Gets or sets the balance between the left and right speaker volumes.</summary>
	/// <returns>The ratio of volume across the left and right speakers in a range between -1 and 1. The default is 0.</returns>
	public double Balance
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.Balance;
		}
		set
		{
			WritePreamble();
			_mediaPlayerState.Balance = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether scrubbing is enabled.</summary>
	/// <returns>true if scrubbing is enabled; otherwise, false.</returns>
	public bool ScrubbingEnabled
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.ScrubbingEnabled;
		}
		set
		{
			WritePreamble();
			_mediaPlayerState.ScrubbingEnabled = value;
		}
	}

	/// <summary>Gets a value that indicates whether the media is muted.</summary>
	/// <returns>true if the media is muted; otherwise, false.</returns>
	public bool IsMuted
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.IsMuted;
		}
		set
		{
			WritePreamble();
			_mediaPlayerState.IsMuted = value;
		}
	}

	/// <summary>Gets the natural duration of the media.</summary>
	/// <returns>The natural duration of the media. The default is <see cref="P:System.Windows.Duration.Automatic" />.</returns>
	public Duration NaturalDuration
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.NaturalDuration;
		}
	}

	/// <summary>Gets or sets the current position of the media.</summary>
	/// <returns>The current position of the media.</returns>
	public TimeSpan Position
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.Position;
		}
		set
		{
			WritePreamble();
			_mediaPlayerState.Position = value;
		}
	}

	/// <summary>Gets or sets the ratio of speed that media is played at.</summary>
	/// <returns>The ratio of speed that media is played back represented by a value between 0 and the largest double value. The default is 1.0.</returns>
	public double SpeedRatio
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.SpeedRatio;
		}
		set
		{
			WritePreamble();
			_mediaPlayerState.SpeedRatio = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.MediaClock" /> associated with the <see cref="T:System.Windows.Media.MediaTimeline" /> to be played.</summary>
	/// <returns>The clock associated with the <see cref="T:System.Windows.Media.MediaTimeline" /> to be played. The default is null.</returns>
	public MediaClock Clock
	{
		get
		{
			ReadPreamble();
			return _mediaPlayerState.Clock;
		}
		set
		{
			WritePreamble();
			_mediaPlayerState.SetClock(value, this);
		}
	}

	/// <summary>Occurs when an error is encountered </summary>
	public event EventHandler<ExceptionEventArgs> MediaFailed
	{
		add
		{
			WritePreamble();
			_mediaPlayerState.MediaFailed += value;
		}
		remove
		{
			WritePreamble();
			_mediaPlayerState.MediaFailed -= value;
		}
	}

	/// <summary>Occurs when the media is opened.</summary>
	public event EventHandler MediaOpened
	{
		add
		{
			WritePreamble();
			_mediaPlayerState.MediaOpened += value;
		}
		remove
		{
			WritePreamble();
			_mediaPlayerState.MediaOpened -= value;
		}
	}

	/// <summary>Occurs when the media has finished playback.</summary>
	public event EventHandler MediaEnded
	{
		add
		{
			WritePreamble();
			_mediaPlayerState.MediaEnded += value;
		}
		remove
		{
			WritePreamble();
			_mediaPlayerState.MediaEnded -= value;
		}
	}

	/// <summary>Occurs when buffering has started.</summary>
	public event EventHandler BufferingStarted
	{
		add
		{
			WritePreamble();
			_mediaPlayerState.BufferingStarted += value;
		}
		remove
		{
			WritePreamble();
			_mediaPlayerState.BufferingStarted -= value;
		}
	}

	/// <summary>Occurs when buffering has finished.</summary>
	public event EventHandler BufferingEnded
	{
		add
		{
			WritePreamble();
			_mediaPlayerState.BufferingEnded += value;
		}
		remove
		{
			WritePreamble();
			_mediaPlayerState.BufferingEnded -= value;
		}
	}

	/// <summary>Occurs when a script command has been encountered within the media.</summary>
	public event EventHandler<MediaScriptCommandEventArgs> ScriptCommand
	{
		add
		{
			WritePreamble();
			_mediaPlayerState.ScriptCommand += value;
		}
		remove
		{
			WritePreamble();
			_mediaPlayerState.ScriptCommand -= value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.MediaPlayer" /> class.</summary>
	public MediaPlayer()
	{
	}

	/// <summary>Opens the given <see cref="T:System.Uri" /> for media playback.</summary>
	/// <param name="source">The media source <see cref="T:System.Uri" />.</param>
	public void Open(Uri source)
	{
		WritePreamble();
		_mediaPlayerState.Open(source);
	}

	/// <summary>Plays media from the current <see cref="P:System.Windows.Media.MediaPlayer.Position" />.</summary>
	public void Play()
	{
		WritePreamble();
		_mediaPlayerState.Play();
	}

	/// <summary>Pauses media playback.</summary>
	public void Pause()
	{
		WritePreamble();
		_mediaPlayerState.Pause();
	}

	/// <summary>Stops media playback.</summary>
	public void Stop()
	{
		WritePreamble();
		_mediaPlayerState.Stop();
	}

	/// <summary>Closes the underlying media. </summary>
	public void Close()
	{
		WritePreamble();
		_mediaPlayerState.Close();
	}

	DUCE.ResourceHandle DUCE.IResource.AddRefOnChannel(DUCE.Channel channel)
	{
		EnsureState();
		using (CompositionEngineLock.Acquire())
		{
			return AddRefOnChannelCore(channel);
		}
	}

	internal DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource._duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_MEDIAPLAYER))
		{
			_needsUpdate = true;
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource._duceResource.GetHandle(channel);
	}

	void DUCE.IResource.ReleaseOnChannel(DUCE.Channel channel)
	{
		EnsureState();
		using (CompositionEngineLock.Acquire())
		{
			ReleaseOnChannelCore(channel);
		}
	}

	internal void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		_duceResource._duceResource.ReleaseOnChannel(channel);
	}

	DUCE.ResourceHandle DUCE.IResource.GetHandle(DUCE.Channel channel)
	{
		EnsureState();
		using (CompositionEngineLock.Acquire())
		{
			return GetHandleCore(channel);
		}
	}

	internal DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel)
	{
		return _duceResource._duceResource.GetHandle(channel);
	}

	int DUCE.IResource.GetChannelCount()
	{
		return _duceResource._duceResource.GetChannelCount();
	}

	DUCE.Channel DUCE.IResource.GetChannel(int index)
	{
		return _duceResource._duceResource.GetChannel(index);
	}

	internal override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource._duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck: true);
			if (_needsUpdate)
			{
				UpdateResourceInternal(channel);
			}
		}
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.MediaPlayer" /> instance.</summary>
	/// <returns>A new <see cref="T:System.Windows.Media.MediaPlayer" /> instance.</returns>
	protected override Freezable CreateInstanceCore()
	{
		return new MediaPlayer();
	}

	/// <summary>Makes this instance a deep copy of the specified <see cref="T:System.Windows.Media.MediaPlayer" />. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.MediaPlayer" /> to clone.</param>
	protected override void CloneCore(Freezable sourceFreezable)
	{
		base.CloneCore(sourceFreezable);
		CloneCommon(sourceFreezable);
	}

	/// <summary>Makes this instance a modifiable deep copy of the specified <see cref="T:System.Windows.Media.MediaPlayer" /> using current property values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.MediaPlayer" /> to clone.</param>
	protected override void CloneCurrentValueCore(Freezable sourceFreezable)
	{
		base.CloneCurrentValueCore(sourceFreezable);
		CloneCommon(sourceFreezable);
	}

	/// <summary>Makes this instance a clone of the specified <see cref="T:System.Windows.Media.MediaPlayer" /> object. </summary>
	/// <param name="sourceFreezable">The <see cref="T:System.Windows.Media.MediaPlayer" /> object to clone and freeze.</param>
	protected override void GetAsFrozenCore(Freezable sourceFreezable)
	{
		base.GetAsFrozenCore(sourceFreezable);
		CloneCommon(sourceFreezable);
	}

	private void CloneCommon(Freezable sourceFreezable)
	{
		MediaPlayer mediaPlayer = (MediaPlayer)sourceFreezable;
		_mediaPlayerState = mediaPlayer._mediaPlayerState;
		_duceResource = mediaPlayer._duceResource;
	}

	private void EnsureState()
	{
		if (_mediaPlayerState == null)
		{
			_mediaPlayerState = new MediaPlayerState(this);
		}
	}

	/// <summary>Ensures that the MediaPlayer is being accessed from a valid thread.</summary>
	protected new void ReadPreamble()
	{
		base.ReadPreamble();
		EnsureState();
	}

	/// <summary>Verifies that the MediaPlayer is not frozen and that it is being accessed from a valid threading context. </summary>
	protected new void WritePreamble()
	{
		base.WritePreamble();
		EnsureState();
	}

	private void OnNewFrame(object sender, EventArgs args)
	{
		_needsUpdate = true;
		RegisterForAsyncUpdateResource();
		FireChanged();
	}

	private void UpdateResourceInternal(DUCE.Channel channel)
	{
		bool flag = false;
		switch (channel.MarshalType)
		{
		case ChannelMarshalType.ChannelMarshalTypeCrossThread:
			flag = true;
			break;
		default:
			throw new NotSupportedException(SR.Media_UnknownChannelType);
		case ChannelMarshalType.ChannelMarshalTypeSameThread:
			break;
		}
		if (!flag)
		{
			if (_newFrameHandler == null)
			{
				_newFrameHandler = OnNewFrame;
				_mediaPlayerState.NewFrame += _newFrameHandler;
			}
		}
		else if (_newFrameHandler != null)
		{
			_mediaPlayerState.NewFrame -= _newFrameHandler;
			_newFrameHandler = null;
		}
		_mediaPlayerState.SendCommandMedia(channel, _duceResource._duceResource.GetHandle(channel), flag);
		_needsUpdate = false;
	}

	internal void SetSpeedRatio(double value)
	{
		_mediaPlayerState.SetSpeedRatio(value);
	}

	internal void SetSource(Uri source)
	{
		_mediaPlayerState.SetSource(source);
	}

	internal void SetPosition(TimeSpan value)
	{
		_mediaPlayerState.SetPosition(value);
	}
}
