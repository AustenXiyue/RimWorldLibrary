using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>Draws a <see cref="T:System.Windows.Media.Geometry" /> using the specified <see cref="P:System.Windows.Media.GeometryDrawing.Brush" /> and <see cref="P:System.Windows.Media.GeometryDrawing.Pen" />.  </summary>
public sealed class GeometryDrawing : Drawing
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.GeometryDrawing.Brush" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.GeometryDrawing.Brush" /> dependency property.</returns>
	public static readonly DependencyProperty BrushProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.GeometryDrawing.Pen" /> dependency property.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.GeometryDrawing.Pen" /> dependency property identifier.</returns>
	public static readonly DependencyProperty PenProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.GeometryDrawing.Geometry" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.GeometryDrawing.Geometry" /> dependency property.</returns>
	public static readonly DependencyProperty GeometryProperty;

	internal DUCE.MultiChannelResource _duceResource;

	/// <summary> Gets or sets the <see cref="T:System.Windows.Media.Brush" /> used to fill the interior of the shape described by this <see cref="T:System.Windows.Media.GeometryDrawing" />.   </summary>
	/// <returns>The brush used to fill this <see cref="T:System.Windows.Media.GeometryDrawing" />. The default value is null.</returns>
	public Brush Brush
	{
		get
		{
			return (Brush)GetValue(BrushProperty);
		}
		set
		{
			SetValueInternal(BrushProperty, value);
		}
	}

	/// <summary> Gets or sets the <see cref="T:System.Windows.Media.Pen" /> used to stroke this <see cref="T:System.Windows.Media.GeometryDrawing" />.   </summary>
	/// <returns>The pen used to stroke this <see cref="T:System.Windows.Media.GeometryDrawing" />. The default value is null.</returns>
	public Pen Pen
	{
		get
		{
			return (Pen)GetValue(PenProperty);
		}
		set
		{
			SetValueInternal(PenProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Geometry" /> that describes the shape of this <see cref="T:System.Windows.Media.GeometryDrawing" />.   </summary>
	/// <returns>The shape described by this <see cref="T:System.Windows.Media.GeometryDrawing" />. The default value is null.</returns>
	public Geometry Geometry
	{
		get
		{
			return (Geometry)GetValue(GeometryProperty);
		}
		set
		{
			SetValueInternal(GeometryProperty, value);
		}
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.GeometryDrawing" /> class. </summary>
	public GeometryDrawing()
	{
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.GeometryDrawing" /> class with the specified <see cref="T:System.Windows.Media.Brush" />, <see cref="T:System.Windows.Media.Pen" />, and <see cref="T:System.Windows.Media.Geometry" />. </summary>
	/// <param name="brush">The brush used to fill this <see cref="T:System.Windows.Media.GeometryDrawing" />.</param>
	/// <param name="pen">The pen used to stroke this <see cref="T:System.Windows.Media.GeometryDrawing" />.</param>
	/// <param name="geometry">The geometry </param>
	public GeometryDrawing(Brush brush, Pen pen, Geometry geometry)
	{
		Brush = brush;
		Pen = pen;
		Geometry = geometry;
	}

	internal override void WalkCurrentValue(DrawingContextWalker ctx)
	{
		ctx.DrawGeometry(Brush, Pen, Geometry);
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GeometryDrawing" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeometryDrawing Clone()
	{
		return (GeometryDrawing)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GeometryDrawing" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeometryDrawing CloneCurrentValue()
	{
		return (GeometryDrawing)base.CloneCurrentValue();
	}

	private static void BrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		GeometryDrawing geometryDrawing = (GeometryDrawing)d;
		Brush resource = (Brush)e.OldValue;
		Brush resource2 = (Brush)e.NewValue;
		if (geometryDrawing.Dispatcher != null)
		{
			DUCE.IResource resource3 = geometryDrawing;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					geometryDrawing.ReleaseResource(resource, channel);
					geometryDrawing.AddRefResource(resource2, channel);
				}
			}
		}
		geometryDrawing.PropertyChanged(BrushProperty);
	}

	private static void PenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		GeometryDrawing geometryDrawing = (GeometryDrawing)d;
		Pen resource = (Pen)e.OldValue;
		Pen resource2 = (Pen)e.NewValue;
		if (geometryDrawing.Dispatcher != null)
		{
			DUCE.IResource resource3 = geometryDrawing;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					geometryDrawing.ReleaseResource(resource, channel);
					geometryDrawing.AddRefResource(resource2, channel);
				}
			}
		}
		geometryDrawing.PropertyChanged(PenProperty);
	}

	private static void GeometryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.IsASubPropertyChange && e.OldValueSource == e.NewValueSource)
		{
			return;
		}
		GeometryDrawing geometryDrawing = (GeometryDrawing)d;
		Geometry resource = (Geometry)e.OldValue;
		Geometry resource2 = (Geometry)e.NewValue;
		if (geometryDrawing.Dispatcher != null)
		{
			DUCE.IResource resource3 = geometryDrawing;
			using (CompositionEngineLock.Acquire())
			{
				int channelCount = resource3.GetChannelCount();
				for (int i = 0; i < channelCount; i++)
				{
					DUCE.Channel channel = resource3.GetChannel(i);
					geometryDrawing.ReleaseResource(resource, channel);
					geometryDrawing.AddRefResource(resource2, channel);
				}
			}
		}
		geometryDrawing.PropertyChanged(GeometryProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new GeometryDrawing();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			Brush brush = Brush;
			Pen pen = Pen;
			Geometry geometry = Geometry;
			DUCE.ResourceHandle hBrush = ((DUCE.IResource)brush)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle hPen = ((DUCE.IResource)pen)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.ResourceHandle hGeometry = ((DUCE.IResource)geometry)?.GetHandle(channel) ?? DUCE.ResourceHandle.Null;
			DUCE.MILCMD_GEOMETRYDRAWING mILCMD_GEOMETRYDRAWING = default(DUCE.MILCMD_GEOMETRYDRAWING);
			mILCMD_GEOMETRYDRAWING.Type = MILCMD.MilCmdGeometryDrawing;
			mILCMD_GEOMETRYDRAWING.Handle = _duceResource.GetHandle(channel);
			mILCMD_GEOMETRYDRAWING.hBrush = hBrush;
			mILCMD_GEOMETRYDRAWING.hPen = hPen;
			mILCMD_GEOMETRYDRAWING.hGeometry = hGeometry;
			channel.SendCommand((byte*)(&mILCMD_GEOMETRYDRAWING), sizeof(DUCE.MILCMD_GEOMETRYDRAWING));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_GEOMETRYDRAWING))
		{
			((DUCE.IResource)Brush)?.AddRefOnChannel(channel);
			((DUCE.IResource)Pen)?.AddRefOnChannel(channel);
			((DUCE.IResource)Geometry)?.AddRefOnChannel(channel);
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
			((DUCE.IResource)Brush)?.ReleaseOnChannel(channel);
			((DUCE.IResource)Pen)?.ReleaseOnChannel(channel);
			((DUCE.IResource)Geometry)?.ReleaseOnChannel(channel);
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

	static GeometryDrawing()
	{
		Type typeFromHandle = typeof(GeometryDrawing);
		BrushProperty = Animatable.RegisterProperty("Brush", typeof(Brush), typeFromHandle, null, BrushPropertyChanged, null, isIndependentlyAnimated: false, null);
		PenProperty = Animatable.RegisterProperty("Pen", typeof(Pen), typeFromHandle, null, PenPropertyChanged, null, isIndependentlyAnimated: false, null);
		GeometryProperty = Animatable.RegisterProperty("Geometry", typeof(Geometry), typeFromHandle, null, GeometryPropertyChanged, null, isIndependentlyAnimated: false, null);
	}
}
