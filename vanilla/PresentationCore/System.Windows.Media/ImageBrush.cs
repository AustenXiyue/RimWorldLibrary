using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>Paints an area with an image. </summary>
public sealed class ImageBrush : TileBrush
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.ImageBrush.ImageSource" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.ImageBrush.ImageSource" /> dependency property identifier.</returns>
	public static readonly DependencyProperty ImageSourceProperty;

	internal DUCE.MultiChannelResource _duceResource;

	/// <summary>Gets or sets the image displayed by this <see cref="T:System.Windows.Media.ImageBrush" />.</summary>
	/// <returns>The image displayed by this <see cref="T:System.Windows.Media.ImageBrush" />.</returns>
	public ImageSource ImageSource
	{
		get
		{
			return (ImageSource)GetValue(ImageSourceProperty);
		}
		set
		{
			SetValueInternal(ImageSourceProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.ImageBrush" /> class with no content. </summary>
	public ImageBrush()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.ImageBrush" /> class that paints an area with the specified image. </summary>
	/// <param name="image">The image to display.</param>
	public ImageBrush(ImageSource image)
	{
		ImageSource = image;
	}

	protected override void GetContentBounds(out Rect contentBounds)
	{
		contentBounds = Rect.Empty;
		if (ImageSource is DrawingImage { Drawing: { } drawing })
		{
			contentBounds = drawing.Bounds;
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.ImageBrush" />, making deep copies of this object's values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new ImageBrush Clone()
	{
		return (ImageBrush)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.ImageBrush" /> object, making deep copies of this object's current values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new ImageBrush CloneCurrentValue()
	{
		return (ImageBrush)base.CloneCurrentValue();
	}

	private static void ImageSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		ImageBrush imageBrush = (ImageBrush)d;
		ImageSource resource = (ImageSource)e.OldValue;
		ImageSource resource2 = (ImageSource)e.NewValue;
		if (imageBrush.Dispatcher != null)
		{
			DUCE.IResource resource3 = imageBrush;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					imageBrush.ReleaseResource(resource, channel);
					imageBrush.AddRefResource(resource2, channel);
				}
			}
		}
		imageBrush.PropertyChanged(ImageSourceProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new ImageBrush();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform transform = base.Transform;
			Transform relativeTransform = base.RelativeTransform;
			ImageSource imageSource = ImageSource;
			DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hRelativeTransform = ((relativeTransform != null && relativeTransform != Transform.Identity) ? ((DUCE.IResource)relativeTransform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hImageSource = ((DUCE.IResource)imageSource)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(Brush.OpacityProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(TileBrush.ViewportProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(TileBrush.ViewboxProperty, channel);
			DUCE.MILCMD_IMAGEBRUSH mILCMD_IMAGEBRUSH = default(DUCE.MILCMD_IMAGEBRUSH);
			mILCMD_IMAGEBRUSH.Type = MILCMD.MilCmdImageBrush;
			mILCMD_IMAGEBRUSH.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_IMAGEBRUSH.Opacity = base.Opacity;
			}
			mILCMD_IMAGEBRUSH.hOpacityAnimations = animationResourceHandle;
			mILCMD_IMAGEBRUSH.hTransform = hTransform;
			mILCMD_IMAGEBRUSH.hRelativeTransform = hRelativeTransform;
			mILCMD_IMAGEBRUSH.ViewportUnits = base.ViewportUnits;
			mILCMD_IMAGEBRUSH.ViewboxUnits = base.ViewboxUnits;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_IMAGEBRUSH.Viewport = base.Viewport;
			}
			mILCMD_IMAGEBRUSH.hViewportAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_IMAGEBRUSH.Viewbox = base.Viewbox;
			}
			mILCMD_IMAGEBRUSH.hViewboxAnimations = animationResourceHandle3;
			mILCMD_IMAGEBRUSH.Stretch = base.Stretch;
			mILCMD_IMAGEBRUSH.TileMode = base.TileMode;
			mILCMD_IMAGEBRUSH.AlignmentX = base.AlignmentX;
			mILCMD_IMAGEBRUSH.AlignmentY = base.AlignmentY;
			mILCMD_IMAGEBRUSH.CachingHint = (CachingHint)GetValue(RenderOptions.CachingHintProperty);
			mILCMD_IMAGEBRUSH.CacheInvalidationThresholdMinimum = (double)GetValue(RenderOptions.CacheInvalidationThresholdMinimumProperty);
			mILCMD_IMAGEBRUSH.CacheInvalidationThresholdMaximum = (double)GetValue(RenderOptions.CacheInvalidationThresholdMaximumProperty);
			mILCMD_IMAGEBRUSH.hImageSource = hImageSource;
			channel.SendCommand((byte*)(&mILCMD_IMAGEBRUSH), sizeof(DUCE.MILCMD_IMAGEBRUSH));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_IMAGEBRUSH))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			((DUCE.IResource)base.RelativeTransform)?.AddRefOnChannel(channel);
			((DUCE.IResource)ImageSource)?.AddRefOnChannel(channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)base.Transform)?.ReleaseOnChannel(channel);
			((DUCE.IResource)base.RelativeTransform)?.ReleaseOnChannel(channel);
			((DUCE.IResource)ImageSource)?.ReleaseOnChannel(channel);
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

	static ImageBrush()
	{
		Type typeFromHandle = typeof(ImageBrush);
		ImageSourceProperty = Animatable.RegisterProperty("ImageSource", typeof(ImageSource), typeFromHandle, null, ImageSourcePropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
