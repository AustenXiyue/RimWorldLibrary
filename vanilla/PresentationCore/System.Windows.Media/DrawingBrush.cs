using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>Paints an area with a <see cref="T:System.Windows.Media.Drawing" />, which can include shapes, text, video, images, or other drawings. </summary>
public sealed class DrawingBrush : TileBrush
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.DrawingBrush.Drawing" /> dependency property. </summary>
	/// <returns>The identifer for the <see cref="P:System.Windows.Media.DrawingBrush.Drawing" /> dependency property.</returns>
	public static readonly DependencyProperty DrawingProperty;

	internal DUCE.MultiChannelResource _duceResource;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Drawing" /> that describes the contents of this <see cref="T:System.Windows.Media.DrawingBrush" />.   </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Drawing" /> that describes the contents of this <see cref="T:System.Windows.Media.DrawingBrush" />. The default is null reference (Nothing in Visual Basic).</returns>
	public Drawing Drawing
	{
		get
		{
			return (Drawing)GetValue(DrawingProperty);
		}
		set
		{
			SetValueInternal(DrawingProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.DrawingBrush" /> class. The resulting brush has no content. </summary>
	public DrawingBrush()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.DrawingBrush" /> class that contains the specified <see cref="T:System.Windows.Media.Drawing" />. </summary>
	/// <param name="drawing">The <see cref="T:System.Windows.Media.Drawing" /> that describes the contents of the brush.</param>
	public DrawingBrush(Drawing drawing)
	{
		Drawing = drawing;
	}

	protected override void GetContentBounds(out Rect contentBounds)
	{
		contentBounds = Drawing.GetBounds();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.DrawingBrush" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DrawingBrush Clone()
	{
		return (DrawingBrush)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.DrawingBrush" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new DrawingBrush CloneCurrentValue()
	{
		return (DrawingBrush)base.CloneCurrentValue();
	}

	private static void DrawingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		DrawingBrush drawingBrush = (DrawingBrush)d;
		Drawing resource = (Drawing)e.OldValue;
		Drawing resource2 = (Drawing)e.NewValue;
		if (drawingBrush.Dispatcher != null)
		{
			DUCE.IResource resource3 = drawingBrush;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					drawingBrush.ReleaseResource(resource, channel);
					drawingBrush.AddRefResource(resource2, channel);
				}
			}
		}
		drawingBrush.PropertyChanged(DrawingProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new DrawingBrush();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Transform transform = base.Transform;
			Transform relativeTransform = base.RelativeTransform;
			Drawing drawing = Drawing;
			DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hRelativeTransform = ((relativeTransform != null && relativeTransform != Transform.Identity) ? ((DUCE.IResource)relativeTransform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hDrawing = ((DUCE.IResource)drawing)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(Brush.OpacityProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(TileBrush.ViewportProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(TileBrush.ViewboxProperty, channel);
			DUCE.MILCMD_DRAWINGBRUSH mILCMD_DRAWINGBRUSH = default(DUCE.MILCMD_DRAWINGBRUSH);
			mILCMD_DRAWINGBRUSH.Type = MILCMD.MilCmdDrawingBrush;
			mILCMD_DRAWINGBRUSH.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_DRAWINGBRUSH.Opacity = base.Opacity;
			}
			mILCMD_DRAWINGBRUSH.hOpacityAnimations = animationResourceHandle;
			mILCMD_DRAWINGBRUSH.hTransform = hTransform;
			mILCMD_DRAWINGBRUSH.hRelativeTransform = hRelativeTransform;
			mILCMD_DRAWINGBRUSH.ViewportUnits = base.ViewportUnits;
			mILCMD_DRAWINGBRUSH.ViewboxUnits = base.ViewboxUnits;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_DRAWINGBRUSH.Viewport = base.Viewport;
			}
			mILCMD_DRAWINGBRUSH.hViewportAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_DRAWINGBRUSH.Viewbox = base.Viewbox;
			}
			mILCMD_DRAWINGBRUSH.hViewboxAnimations = animationResourceHandle3;
			mILCMD_DRAWINGBRUSH.Stretch = base.Stretch;
			mILCMD_DRAWINGBRUSH.TileMode = base.TileMode;
			mILCMD_DRAWINGBRUSH.AlignmentX = base.AlignmentX;
			mILCMD_DRAWINGBRUSH.AlignmentY = base.AlignmentY;
			mILCMD_DRAWINGBRUSH.CachingHint = (CachingHint)GetValue(RenderOptions.CachingHintProperty);
			mILCMD_DRAWINGBRUSH.CacheInvalidationThresholdMinimum = (double)GetValue(RenderOptions.CacheInvalidationThresholdMinimumProperty);
			mILCMD_DRAWINGBRUSH.CacheInvalidationThresholdMaximum = (double)GetValue(RenderOptions.CacheInvalidationThresholdMaximumProperty);
			mILCMD_DRAWINGBRUSH.hDrawing = hDrawing;
			channel.SendCommand((byte*)(&mILCMD_DRAWINGBRUSH), sizeof(DUCE.MILCMD_DRAWINGBRUSH));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_DRAWINGBRUSH))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			((DUCE.IResource)base.RelativeTransform)?.AddRefOnChannel(channel);
			((DUCE.IResource)Drawing)?.AddRefOnChannel(channel);
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
			((DUCE.IResource)Drawing)?.ReleaseOnChannel(channel);
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

	static DrawingBrush()
	{
		Type typeFromHandle = typeof(DrawingBrush);
		DrawingProperty = Animatable.RegisterProperty("Drawing", typeof(Drawing), typeFromHandle, null, DrawingPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
