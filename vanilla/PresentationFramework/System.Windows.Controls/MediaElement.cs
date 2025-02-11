using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control that contains audio and/or video. </summary>
[Localizability(LocalizationCategory.NeverLocalize)]
public class MediaElement : FrameworkElement, IUriContext
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.Source" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.Source" /> dependency property.</returns>
	public static readonly DependencyProperty SourceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.Volume" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.Volume" /> dependency property.</returns>
	public static readonly DependencyProperty VolumeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.Balance" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.Balance" /> dependency property.</returns>
	public static readonly DependencyProperty BalanceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.IsMuted" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.IsMuted" /> dependency property.</returns>
	public static readonly DependencyProperty IsMutedProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.ScrubbingEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.ScrubbingEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty ScrubbingEnabledProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.UnloadedBehavior" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.UnloadedBehavior" /> dependency property.</returns>
	public static readonly DependencyProperty UnloadedBehaviorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.LoadedBehavior" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.LoadedBehavior" /> dependency property.</returns>
	public static readonly DependencyProperty LoadedBehaviorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.Stretch" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.Stretch" /> dependency property.</returns>
	public static readonly DependencyProperty StretchProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.MediaElement.StretchDirection" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.MediaElement.StretchDirection" /> dependency property.</returns>
	public static readonly DependencyProperty StretchDirectionProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.MediaElement.MediaFailed" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.MediaElement.MediaFailed" /> dependency property.</returns>
	public static readonly RoutedEvent MediaFailedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.MediaElement.MediaOpened" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.MediaElement.MediaOpened" /> dependency property.</returns>
	public static readonly RoutedEvent MediaOpenedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.MediaElement.BufferingStarted" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.MediaElement.BufferingStarted" /> routed event.</returns>
	public static readonly RoutedEvent BufferingStartedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.MediaElement.BufferingEnded" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.MediaElement.BufferingEnded" /> routed event.</returns>
	public static readonly RoutedEvent BufferingEndedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.MediaElement.ScriptCommand" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.MediaElement.ScriptCommand" /> routed event.</returns>
	public static readonly RoutedEvent ScriptCommandEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.MediaElement.MediaEnded" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.MediaElement.MediaEnded" /> routed event.</returns>
	public static readonly RoutedEvent MediaEndedEvent;

	private AVElementHelper _helper;

	/// <summary>Gets or sets a media source on the <see cref="T:System.Windows.Controls.MediaElement" />.  </summary>
	/// <returns>The URI that specifies the source of the element. The default is null.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.MediaElement.Clock" /> property is not null.</exception>
	public Uri Source
	{
		get
		{
			return (Uri)GetValue(SourceProperty);
		}
		set
		{
			SetValue(SourceProperty, value);
		}
	}

	/// <summary>Gets or sets the clock associated with the <see cref="T:System.Windows.Media.MediaTimeline" /> that controls media playback.</summary>
	/// <returns>A clock associated with the <see cref="T:System.Windows.Media.MediaTimeline" /> that controls media playback. The default value is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public MediaClock Clock
	{
		get
		{
			return _helper.Clock;
		}
		set
		{
			_helper.SetClock(value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Stretch" /> value that describes how a <see cref="T:System.Windows.Controls.MediaElement" /> fills the destination rectangle.  </summary>
	/// <returns>The stretch value for the rendered media. The default is <see cref="F:System.Windows.Media.Stretch.Uniform" />.</returns>
	public Stretch Stretch
	{
		get
		{
			return (Stretch)GetValue(StretchProperty);
		}
		set
		{
			SetValue(StretchProperty, value);
		}
	}

	/// <summary>Gets or sets a value that determines the restrictions on scaling that are applied to the image.  </summary>
	/// <returns>The value that specifies the direction the element is stretched. The default is <see cref="F:System.Windows.Controls.StretchDirection.Both" />.</returns>
	public StretchDirection StretchDirection
	{
		get
		{
			return (StretchDirection)GetValue(StretchDirectionProperty);
		}
		set
		{
			SetValue(StretchDirectionProperty, value);
		}
	}

	/// <summary>Gets or sets the media's volume. </summary>
	/// <returns>The media's volume represented on a linear scale between 0 and 1. The default is 0.5.</returns>
	public double Volume
	{
		get
		{
			return (double)GetValue(VolumeProperty);
		}
		set
		{
			SetValue(VolumeProperty, value);
		}
	}

	/// <summary>Gets or sets a ratio of volume across speakers.  </summary>
	/// <returns>The ratio of volume across speakers in the range between -1 and 1. The default is 0.</returns>
	public double Balance
	{
		get
		{
			return (double)GetValue(BalanceProperty);
		}
		set
		{
			SetValue(BalanceProperty, value);
		}
	}

	/// <summary>Gets or sets a value indicating whether the audio is muted.  </summary>
	/// <returns>true if audio is muted; otherwise, false. The default is false.</returns>
	public bool IsMuted
	{
		get
		{
			return (bool)GetValue(IsMutedProperty);
		}
		set
		{
			SetValue(IsMutedProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.MediaElement" /> will update frames for seek operations while paused. </summary>
	/// <returns>true if frames are updated while paused; otherwise, false. The default is false.</returns>
	public bool ScrubbingEnabled
	{
		get
		{
			return (bool)GetValue(ScrubbingEnabledProperty);
		}
		set
		{
			SetValue(ScrubbingEnabledProperty, value);
		}
	}

	/// <summary>Gets or sets the unload behavior <see cref="T:System.Windows.Controls.MediaState" /> for the media. </summary>
	/// <returns>The unload behavior <see cref="T:System.Windows.Controls.MediaState" /> for the media.</returns>
	public MediaState UnloadedBehavior
	{
		get
		{
			return (MediaState)GetValue(UnloadedBehaviorProperty);
		}
		set
		{
			SetValue(UnloadedBehaviorProperty, value);
		}
	}

	/// <summary>Gets or sets the load behavior <see cref="T:System.Windows.Controls.MediaState" /> for the media. </summary>
	/// <returns>The load behavior <see cref="T:System.Windows.Controls.MediaState" /> set for the media. The default value is <see cref="F:System.Windows.Controls.MediaState.Play" />.</returns>
	public MediaState LoadedBehavior
	{
		get
		{
			return (MediaState)GetValue(LoadedBehaviorProperty);
		}
		set
		{
			SetValue(LoadedBehaviorProperty, value);
		}
	}

	/// <summary>Gets a value indicating whether the media can be paused.</summary>
	/// <returns>true if the media can be paused; otherwise, false.</returns>
	public bool CanPause => _helper.Player.CanPause;

	/// <summary>Get a value indicating whether the media is buffering.</summary>
	/// <returns>true if the media is buffering; otherwise, false.</returns>
	public bool IsBuffering => _helper.Player.IsBuffering;

	/// <summary>Gets a percentage value indicating the amount of download completed for content located on a remote server.</summary>
	/// <returns>A percentage value indicating the amount of download completed for content located on a remote server. The value ranges from 0 to 1. The default value is 0.</returns>
	public double DownloadProgress => _helper.Player.DownloadProgress;

	/// <summary>Gets a value that indicates the percentage of buffering progress made.</summary>
	/// <returns>The percentage of buffering completed for streaming content. The value ranges from 0 to 1.</returns>
	public double BufferingProgress => _helper.Player.BufferingProgress;

	/// <summary>Gets the height of the video associated with the media.</summary>
	/// <returns>The height of the video associated with the media. Audio files will return zero.</returns>
	public int NaturalVideoHeight => _helper.Player.NaturalVideoHeight;

	/// <summary>Gets the width of the video associated with the media.</summary>
	/// <returns>The width of the video associated with the media.</returns>
	public int NaturalVideoWidth => _helper.Player.NaturalVideoWidth;

	/// <summary>Gets a value indicating whether the media has audio.</summary>
	/// <returns>true if the media has audio; otherwise, false.</returns>
	public bool HasAudio => _helper.Player.HasAudio;

	/// <summary>Gets a value indicating whether the media has video.</summary>
	/// <returns>true if the media has video; otherwise, false.</returns>
	public bool HasVideo => _helper.Player.HasVideo;

	/// <summary>Gets the natural duration of the media.</summary>
	/// <returns>The natural duration of the media.</returns>
	public Duration NaturalDuration => _helper.Player.NaturalDuration;

	/// <summary>Gets or sets the current position of progress through the media's playback time.</summary>
	/// <returns>The amount of time since the beginning of the media. The default is 00:00:00.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.MediaElement.Clock" /> property is not null.</exception>
	public TimeSpan Position
	{
		get
		{
			return _helper.Position;
		}
		set
		{
			_helper.SetPosition(value);
		}
	}

	/// <summary>Gets or sets the speed ratio of the media.</summary>
	/// <returns>The speed ratio of the media. The valid range is between 0 (zero) and infinity. Values less than 1 yield slower than normal playback, and values greater than 1 yield faster than normal playback. Negative values are treated as 0. The default value is 1. </returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.MediaElement.Clock" /> property is not null.</exception>
	public double SpeedRatio
	{
		get
		{
			return _helper.SpeedRatio;
		}
		set
		{
			_helper.SetSpeedRatio(value);
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <returns>The base URI of the current context.</returns>
	Uri IUriContext.BaseUri
	{
		get
		{
			return _helper.BaseUri;
		}
		set
		{
			_helper.BaseUri = value;
		}
	}

	internal AVElementHelper Helper => _helper;

	/// <summary>Occurs when an error is encountered.</summary>
	public event EventHandler<ExceptionRoutedEventArgs> MediaFailed
	{
		add
		{
			AddHandler(MediaFailedEvent, value);
		}
		remove
		{
			RemoveHandler(MediaFailedEvent, value);
		}
	}

	/// <summary>Occurs when media loading has finished.</summary>
	public event RoutedEventHandler MediaOpened
	{
		add
		{
			AddHandler(MediaOpenedEvent, value);
		}
		remove
		{
			RemoveHandler(MediaOpenedEvent, value);
		}
	}

	/// <summary>Occurs when media buffering has begun.</summary>
	public event RoutedEventHandler BufferingStarted
	{
		add
		{
			AddHandler(BufferingStartedEvent, value);
		}
		remove
		{
			RemoveHandler(BufferingStartedEvent, value);
		}
	}

	/// <summary>Occurs when media buffering has ended.</summary>
	public event RoutedEventHandler BufferingEnded
	{
		add
		{
			AddHandler(BufferingEndedEvent, value);
		}
		remove
		{
			RemoveHandler(BufferingEndedEvent, value);
		}
	}

	/// <summary>Occurs when a script command is encountered in the media.</summary>
	public event EventHandler<MediaScriptCommandRoutedEventArgs> ScriptCommand
	{
		add
		{
			AddHandler(ScriptCommandEvent, value);
		}
		remove
		{
			RemoveHandler(ScriptCommandEvent, value);
		}
	}

	/// <summary>Occurs when the media has ended.</summary>
	public event RoutedEventHandler MediaEnded
	{
		add
		{
			AddHandler(MediaEndedEvent, value);
		}
		remove
		{
			RemoveHandler(MediaEndedEvent, value);
		}
	}

	/// <summary>Instantiates a new instance of the <see cref="T:System.Windows.Controls.MediaElement" /> class.</summary>
	public MediaElement()
	{
		Initialize();
	}

	static MediaElement()
	{
		SourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(MediaElement), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, AVElementHelper.OnSourceChanged));
		VolumeProperty = DependencyProperty.Register("Volume", typeof(double), typeof(MediaElement), new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.None, VolumePropertyChanged));
		BalanceProperty = DependencyProperty.Register("Balance", typeof(double), typeof(MediaElement), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.None, BalancePropertyChanged));
		IsMutedProperty = DependencyProperty.Register("IsMuted", typeof(bool), typeof(MediaElement), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, IsMutedPropertyChanged));
		ScrubbingEnabledProperty = DependencyProperty.Register("ScrubbingEnabled", typeof(bool), typeof(MediaElement), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None, ScrubbingEnabledPropertyChanged));
		UnloadedBehaviorProperty = DependencyProperty.Register("UnloadedBehavior", typeof(MediaState), typeof(MediaElement), new FrameworkPropertyMetadata(MediaState.Close, FrameworkPropertyMetadataOptions.None, UnloadedBehaviorPropertyChanged));
		LoadedBehaviorProperty = DependencyProperty.Register("LoadedBehavior", typeof(MediaState), typeof(MediaElement), new FrameworkPropertyMetadata(MediaState.Play, FrameworkPropertyMetadataOptions.None, LoadedBehaviorPropertyChanged));
		StretchProperty = Viewbox.StretchProperty.AddOwner(typeof(MediaElement));
		StretchDirectionProperty = Viewbox.StretchDirectionProperty.AddOwner(typeof(MediaElement));
		MediaFailedEvent = EventManager.RegisterRoutedEvent("MediaFailed", RoutingStrategy.Bubble, typeof(EventHandler<ExceptionRoutedEventArgs>), typeof(MediaElement));
		MediaOpenedEvent = EventManager.RegisterRoutedEvent("MediaOpened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaElement));
		BufferingStartedEvent = EventManager.RegisterRoutedEvent("BufferingStarted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaElement));
		BufferingEndedEvent = EventManager.RegisterRoutedEvent("BufferingEnded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaElement));
		ScriptCommandEvent = EventManager.RegisterRoutedEvent("ScriptCommand", RoutingStrategy.Bubble, typeof(EventHandler<MediaScriptCommandRoutedEventArgs>), typeof(MediaElement));
		MediaEndedEvent = EventManager.RegisterRoutedEvent("MediaEnded", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MediaElement));
		Style defaultValue = CreateDefaultStyles();
		FrameworkElement.StyleProperty.OverrideMetadata(typeof(MediaElement), new FrameworkPropertyMetadata(defaultValue));
		StretchProperty.OverrideMetadata(typeof(MediaElement), new FrameworkPropertyMetadata(Stretch.Uniform, FrameworkPropertyMetadataOptions.AffectsMeasure));
		StretchDirectionProperty.OverrideMetadata(typeof(MediaElement), new FrameworkPropertyMetadata(StretchDirection.Both, FrameworkPropertyMetadataOptions.AffectsMeasure));
		ControlsTraceLogger.AddControl(TelemetryControls.MediaElement);
	}

	private static Style CreateDefaultStyles()
	{
		Style style = new Style(typeof(MediaElement), null);
		style.Setters.Add(new Setter(FrameworkElement.FlowDirectionProperty, FlowDirection.LeftToRight));
		style.Seal();
		return style;
	}

	/// <summary>Plays media from the current position.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.MediaElement.Clock" /> property is not null.</exception>
	public void Play()
	{
		_helper.SetState(MediaState.Play);
	}

	/// <summary>Pauses media at the current position.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.MediaElement.Clock" /> property is not null.</exception>
	public void Pause()
	{
		_helper.SetState(MediaState.Pause);
	}

	/// <summary>Stops and resets media to be played from the beginning.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.Controls.MediaElement.Clock" /> property is not null.</exception>
	public void Stop()
	{
		_helper.SetState(MediaState.Stop);
	}

	/// <summary>Closes the media.</summary>
	public void Close()
	{
		_helper.SetState(MediaState.Close);
	}

	/// <summary>Creates and returns an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for this <see cref="T:System.Windows.Controls.MediaElement" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for this <see cref="T:System.Windows.Controls.MediaElement" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new MediaElementAutomationPeer(this);
	}

	/// <summary>Updates the <see cref="P:System.Windows.UIElement.DesiredSize" /> of the <see cref="T:System.Windows.Controls.MediaElement" />. This method is called by a parent <see cref="T:System.Windows.UIElement" />. This is the first pass of layout.</summary>
	/// <returns>The desired size.</returns>
	/// <param name="availableSize">The upper limit the element should not exceed.</param>
	protected override Size MeasureOverride(Size availableSize)
	{
		return MeasureArrangeHelper(availableSize);
	}

	/// <summary>Arranges and sizes a <see cref="T:System.Windows.Controls.MediaElement" /> control.</summary>
	/// <returns>Size of the control.</returns>
	/// <param name="finalSize">Size used to arrange the control.</param>
	protected override Size ArrangeOverride(Size finalSize)
	{
		return MeasureArrangeHelper(finalSize);
	}

	/// <summary>Draws the content of a <see cref="T:System.Windows.Media.DrawingContext" /> object during the render pass of a <see cref="T:System.Windows.Controls.MediaElement" /> control. </summary>
	/// <param name="drawingContext">The <see cref="T:System.Windows.Media.DrawingContext" /> to draw.</param>
	protected override void OnRender(DrawingContext drawingContext)
	{
		if (_helper.Player != null)
		{
			drawingContext.DrawVideo(_helper.Player, new Rect(default(Point), base.RenderSize));
		}
	}

	private void Initialize()
	{
		_helper = new AVElementHelper(this);
	}

	private Size MeasureArrangeHelper(Size inputSize)
	{
		MediaPlayer player = _helper.Player;
		if (player == null)
		{
			return default(Size);
		}
		Size contentSize = new Size(player.NaturalVideoWidth, player.NaturalVideoHeight);
		Size size = Viewbox.ComputeScaleFactor(inputSize, contentSize, Stretch, StretchDirection);
		return new Size(contentSize.Width * size.Width, contentSize.Height * size.Height);
	}

	private static void VolumePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			((MediaElement)d)?._helper.SetVolume((double)e.NewValue);
		}
	}

	private static void BalancePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			((MediaElement)d)?._helper.SetBalance((double)e.NewValue);
		}
	}

	private static void IsMutedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			((MediaElement)d)?._helper.SetIsMuted((bool)e.NewValue);
		}
	}

	private static void ScrubbingEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			((MediaElement)d)?._helper.SetScrubbingEnabled((bool)e.NewValue);
		}
	}

	private static void UnloadedBehaviorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			((MediaElement)d)?._helper.SetUnloadedBehavior((MediaState)e.NewValue);
		}
	}

	private static void LoadedBehaviorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!e.IsASubPropertyChange)
		{
			((MediaElement)d)?._helper.SetLoadedBehavior((MediaState)e.NewValue);
		}
	}

	internal void OnMediaFailed(object sender, ExceptionEventArgs args)
	{
		RaiseEvent(new ExceptionRoutedEventArgs(MediaFailedEvent, this, args.ErrorException));
	}

	internal void OnMediaOpened(object sender, EventArgs args)
	{
		RaiseEvent(new RoutedEventArgs(MediaOpenedEvent, this));
	}

	internal void OnBufferingStarted(object sender, EventArgs args)
	{
		RaiseEvent(new RoutedEventArgs(BufferingStartedEvent, this));
	}

	internal void OnBufferingEnded(object sender, EventArgs args)
	{
		RaiseEvent(new RoutedEventArgs(BufferingEndedEvent, this));
	}

	internal void OnMediaEnded(object sender, EventArgs args)
	{
		RaiseEvent(new RoutedEventArgs(MediaEndedEvent, this));
	}

	internal void OnScriptCommand(object sender, MediaScriptCommandEventArgs args)
	{
		RaiseEvent(new MediaScriptCommandRoutedEventArgs(ScriptCommandEvent, this, args.ParameterType, args.ParameterValue));
	}
}
