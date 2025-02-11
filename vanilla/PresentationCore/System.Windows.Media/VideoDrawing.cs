using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>Plays a media file. If the media is a video file, the <see cref="T:System.Windows.Media.VideoDrawing" /> draws it to the specified rectangle.</summary>
public sealed class VideoDrawing : Drawing
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.VideoDrawing.Player" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.VideoDrawing.Player" /> dependency property.</returns>
	public static readonly DependencyProperty PlayerProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.VideoDrawing.Rect" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.VideoDrawing.Rect" /> dependency property.</returns>
	public static readonly DependencyProperty RectProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Rect s_Rect;

	/// <summary>Gets or sets the media player associated with the drawing. </summary>
	/// <returns>The media player associated with the drawing.</returns>
	public MediaPlayer Player
	{
		get
		{
			return (MediaPlayer)GetValue(PlayerProperty);
		}
		set
		{
			SetValueInternal(PlayerProperty, value);
		}
	}

	/// <summary>Gets or sets the rectangular area in which the video is drawn. </summary>
	/// <returns>The rectangle in which the video is drawn.</returns>
	public Rect Rect
	{
		get
		{
			return (Rect)GetValue(RectProperty);
		}
		set
		{
			SetValueInternal(RectProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the VideoDrawing class.</summary>
	public VideoDrawing()
	{
	}

	internal override void WalkCurrentValue(DrawingContextWalker ctx)
	{
		ctx.DrawVideo(Player, Rect);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.VideoDrawing" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new VideoDrawing Clone()
	{
		return (VideoDrawing)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.VideoDrawing" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new VideoDrawing CloneCurrentValue()
	{
		return (VideoDrawing)base.CloneCurrentValue();
	}

	private static void PlayerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		VideoDrawing videoDrawing = (VideoDrawing)d;
		MediaPlayer resource = (MediaPlayer)e.OldValue;
		MediaPlayer resource2 = (MediaPlayer)e.NewValue;
		if (videoDrawing.Dispatcher != null)
		{
			DUCE.IResource resource3 = videoDrawing;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					videoDrawing.ReleaseResource(resource, channel);
					videoDrawing.AddRefResource(resource2, channel);
				}
			}
		}
		videoDrawing.PropertyChanged(PlayerProperty);
	}

	private static void RectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((VideoDrawing)d).PropertyChanged(RectProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new VideoDrawing();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle hPlayer = ((DUCE.IResource)Player)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(RectProperty, channel);
			DUCE.MILCMD_VIDEODRAWING mILCMD_VIDEODRAWING = default(DUCE.MILCMD_VIDEODRAWING);
			mILCMD_VIDEODRAWING.Type = MILCMD.MilCmdVideoDrawing;
			mILCMD_VIDEODRAWING.Handle = _duceResource.GetHandle(channel);
			mILCMD_VIDEODRAWING.hPlayer = hPlayer;
			if (animationResourceHandle.IsNull)
			{
				mILCMD_VIDEODRAWING.Rect = Rect;
			}
			mILCMD_VIDEODRAWING.hRectAnimations = animationResourceHandle;
			channel.SendCommand((byte*)(&mILCMD_VIDEODRAWING), sizeof(DUCE.MILCMD_VIDEODRAWING));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_VIDEODRAWING))
		{
			((DUCE.IResource)Player)?.AddRefOnChannel(channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)Player)?.ReleaseOnChannel(channel);
			ReleaseOnChannelAnimations(channel);
		}
	}

	internal override DUCE.ResourceHandle GetHandleCore(DUCE.Channel channel)
	{
		return _duceResource.GetHandle(channel);
	}

	internal override int GetChannelCountCore()
	{
		return _duceResource.GetChannelCount();
	}

	internal override DUCE.Channel GetChannelCore(int index)
	{
		return _duceResource.GetChannel(index);
	}

	static VideoDrawing()
	{
		s_Rect = Rect.Empty;
		Type typeFromHandle = typeof(VideoDrawing);
		PlayerProperty = Animatable.RegisterProperty("Player", typeof(MediaPlayer), typeFromHandle, null, PlayerPropertyChanged, null, isIndependentlyAnimated: false, null);
		RectProperty = Animatable.RegisterProperty("Rect", typeof(Rect), typeFromHandle, Rect.Empty, RectPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
