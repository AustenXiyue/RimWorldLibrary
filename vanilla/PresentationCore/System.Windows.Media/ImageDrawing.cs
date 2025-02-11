using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary> Draws an image within a region defined by a <see cref="T:System.Windows.Rect" />. </summary>
public sealed class ImageDrawing : Drawing
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.ImageDrawing.ImageSource" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.ImageDrawing.ImageSource" /> dependency property.</returns>
	public static readonly DependencyProperty ImageSourceProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.ImageDrawing.Rect" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.ImageDrawing.Rect" /> dependency property.</returns>
	public static readonly DependencyProperty RectProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Rect s_Rect;

	/// <summary> Gets or sets the source of the image </summary>
	/// <returns>The source of the image. The default value is null.</returns>
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

	/// <summary> Gets or sets the region in which the image is drawn. </summary>
	/// <returns>The region in which the image is drawn. The default is Empty.</returns>
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

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.ImageDrawing" /> class.</summary>
	public ImageDrawing()
	{
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.ImageDrawing" /> class that has the specified target <see cref="P:System.Windows.Media.ImageDrawing.ImageSource" /> and <see cref="P:System.Windows.Media.ImageDrawing.Rect" />. </summary>
	/// <param name="imageSource">Source of the image that is drawn.</param>
	/// <param name="rect">Defines the rectangular area in which the image is drawn.</param>
	public ImageDrawing(ImageSource imageSource, Rect rect)
	{
		ImageSource = imageSource;
		Rect = rect;
	}

	internal override void WalkCurrentValue(DrawingContextWalker ctx)
	{
		ctx.DrawImage(ImageSource, Rect);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.ImageDrawing" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ImageDrawing Clone()
	{
		return (ImageDrawing)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.ImageDrawing" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ImageDrawing CloneCurrentValue()
	{
		return (ImageDrawing)base.CloneCurrentValue();
	}

	private static void ImageSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		ImageDrawing imageDrawing = (ImageDrawing)d;
		ImageSource resource = (ImageSource)e.OldValue;
		ImageSource resource2 = (ImageSource)e.NewValue;
		if (imageDrawing.Dispatcher != null)
		{
			DUCE.IResource resource3 = imageDrawing;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					imageDrawing.ReleaseResource(resource, channel);
					imageDrawing.AddRefResource(resource2, channel);
				}
			}
		}
		imageDrawing.PropertyChanged(ImageSourceProperty);
	}

	private static void RectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ImageDrawing)d).PropertyChanged(RectProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new ImageDrawing();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle hImageSource = ((DUCE.IResource)ImageSource)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(RectProperty, channel);
			DUCE.MILCMD_IMAGEDRAWING mILCMD_IMAGEDRAWING = default(DUCE.MILCMD_IMAGEDRAWING);
			mILCMD_IMAGEDRAWING.Type = MILCMD.MilCmdImageDrawing;
			mILCMD_IMAGEDRAWING.Handle = _duceResource.GetHandle(channel);
			mILCMD_IMAGEDRAWING.hImageSource = hImageSource;
			if (animationResourceHandle.IsNull)
			{
				mILCMD_IMAGEDRAWING.Rect = Rect;
			}
			mILCMD_IMAGEDRAWING.hRectAnimations = animationResourceHandle;
			channel.SendCommand((byte*)(&mILCMD_IMAGEDRAWING), sizeof(DUCE.MILCMD_IMAGEDRAWING));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_IMAGEDRAWING))
		{
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

	static ImageDrawing()
	{
		s_Rect = Rect.Empty;
		Type typeFromHandle = typeof(ImageDrawing);
		ImageSourceProperty = Animatable.RegisterProperty("ImageSource", typeof(ImageSource), typeFromHandle, null, ImageSourcePropertyChanged, null, isIndependentlyAnimated: false, null);
		RectProperty = Animatable.RegisterProperty("Rect", typeof(Rect), typeFromHandle, Rect.Empty, RectPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
