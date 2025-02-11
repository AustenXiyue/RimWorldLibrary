using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>Represents a <see cref="T:System.Windows.Media.Drawing" /> object that renders a <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
public sealed class GlyphRunDrawing : Drawing
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.GlyphRunDrawing.GlyphRun" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.GlyphRunDrawing.GlyphRun" /> dependency property.</returns>
	public static readonly DependencyProperty GlyphRunProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.GlyphRunDrawing.ForegroundBrush" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.GlyphRunDrawing.ForegroundBrush" /> dependency property.</returns>
	public static readonly DependencyProperty ForegroundBrushProperty;

	internal DUCE.MultiChannelResource _duceResource;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.GlyphRun" /> that describes the text to draw.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.GlyphRun" /> that describes the text to draw. The default value is null.</returns>
	public GlyphRun GlyphRun
	{
		get
		{
			return (GlyphRun)GetValue(GlyphRunProperty);
		}
		set
		{
			SetValueInternal(GlyphRunProperty, value);
		}
	}

	/// <summary>Gets or sets the foreground brush of the <see cref="T:System.Windows.Media.GlyphRunDrawing" />.  </summary>
	/// <returns>The brush that paints the <see cref="T:System.Windows.Media.GlyphRun" />. The default value is null. </returns>
	public Brush ForegroundBrush
	{
		get
		{
			return (Brush)GetValue(ForegroundBrushProperty);
		}
		set
		{
			SetValueInternal(ForegroundBrushProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GlyphRunDrawing" /> class.</summary>
	public GlyphRunDrawing()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GlyphRunDrawing" /> class by specifying the foreground brush and <see cref="T:System.Windows.Media.GlyphRun" />.</summary>
	/// <param name="foregroundBrush">The foreground brush to use for the <see cref="T:System.Windows.Media.GlyphRunDrawing" />.</param>
	/// <param name="glyphRun">The <see cref="T:System.Windows.Media.GlyphRun" /> to use in this <see cref="T:System.Windows.Media.GlyphRunDrawing" />.</param>
	public GlyphRunDrawing(Brush foregroundBrush, GlyphRun glyphRun)
	{
		GlyphRun = glyphRun;
		ForegroundBrush = foregroundBrush;
	}

	internal override void WalkCurrentValue(DrawingContextWalker ctx)
	{
		ctx.DrawGlyphRun(ForegroundBrush, GlyphRun);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GlyphRunDrawing" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GlyphRunDrawing Clone()
	{
		return (GlyphRunDrawing)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GlyphRunDrawing" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GlyphRunDrawing CloneCurrentValue()
	{
		return (GlyphRunDrawing)base.CloneCurrentValue();
	}

	private static void GlyphRunPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		GlyphRunDrawing glyphRunDrawing = (GlyphRunDrawing)d;
		GlyphRun resource = (GlyphRun)e.OldValue;
		GlyphRun resource2 = (GlyphRun)e.NewValue;
		if (glyphRunDrawing.Dispatcher != null)
		{
			DUCE.IResource resource3 = glyphRunDrawing;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					glyphRunDrawing.ReleaseResource(resource, channel);
					glyphRunDrawing.AddRefResource(resource2, channel);
				}
			}
		}
		glyphRunDrawing.PropertyChanged(GlyphRunProperty);
	}

	private static void ForegroundBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		GlyphRunDrawing glyphRunDrawing = (GlyphRunDrawing)d;
		Brush resource = (Brush)e.OldValue;
		Brush resource2 = (Brush)e.NewValue;
		if (glyphRunDrawing.Dispatcher != null)
		{
			DUCE.IResource resource3 = glyphRunDrawing;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					glyphRunDrawing.ReleaseResource(resource, channel);
					glyphRunDrawing.AddRefResource(resource2, channel);
				}
			}
		}
		glyphRunDrawing.PropertyChanged(ForegroundBrushProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new GlyphRunDrawing();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			GlyphRun glyphRun = GlyphRun;
			Brush foregroundBrush = ForegroundBrush;
			DUCE.ResourceHandle hGlyphRun = ((DUCE.IResource)glyphRun)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle hForegroundBrush = ((DUCE.IResource)foregroundBrush)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.MILCMD_GLYPHRUNDRAWING mILCMD_GLYPHRUNDRAWING = default(DUCE.MILCMD_GLYPHRUNDRAWING);
			mILCMD_GLYPHRUNDRAWING.Type = MILCMD.MilCmdGlyphRunDrawing;
			mILCMD_GLYPHRUNDRAWING.Handle = _duceResource.GetHandle(channel);
			mILCMD_GLYPHRUNDRAWING.hGlyphRun = hGlyphRun;
			mILCMD_GLYPHRUNDRAWING.hForegroundBrush = hForegroundBrush;
			channel.SendCommand((byte*)(&mILCMD_GLYPHRUNDRAWING), sizeof(DUCE.MILCMD_GLYPHRUNDRAWING));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_GLYPHRUNDRAWING))
		{
			((DUCE.IResource)GlyphRun)?.AddRefOnChannel(channel);
			((DUCE.IResource)ForegroundBrush)?.AddRefOnChannel(channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)GlyphRun)?.ReleaseOnChannel(channel);
			((DUCE.IResource)ForegroundBrush)?.ReleaseOnChannel(channel);
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

	static GlyphRunDrawing()
	{
		Type typeFromHandle = typeof(GlyphRunDrawing);
		GlyphRunProperty = Animatable.RegisterProperty("GlyphRun", typeof(GlyphRun), typeFromHandle, null, GlyphRunPropertyChanged, null, isIndependentlyAnimated: false, null);
		ForegroundBrushProperty = Animatable.RegisterProperty("ForegroundBrush", typeof(Brush), typeFromHandle, null, ForegroundBrushPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
