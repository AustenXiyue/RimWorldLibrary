using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>Paints an area with a radial gradient. A focal point defines the beginning of the gradient, and a circle defines the end point of the gradient. </summary>
public sealed class RadialGradientBrush : GradientBrush
{
	/// <summary> Identifies the <see cref="P:System.Windows.Media.RadialGradientBrush.Center" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.RadialGradientBrush.Center" /> dependency property identifier.</returns>
	public static readonly DependencyProperty CenterProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.RadialGradientBrush.RadiusX" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.RadialGradientBrush.RadiusX" /> dependency property identifier.</returns>
	public static readonly DependencyProperty RadiusXProperty;

	/// <summary> Identifies the <see cref="P:System.Windows.Media.RadialGradientBrush.RadiusY" /> dependency property. </summary>
	/// <returns>The <see cref="P:System.Windows.Media.RadialGradientBrush.RadiusY" /> dependency property identifier.</returns>
	public static readonly DependencyProperty RadiusYProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.RadialGradientBrush.GradientOrigin" /> dependency property.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.RadialGradientBrush.GradientOrigin" /> dependency property identifier.</returns>
	public static readonly DependencyProperty GradientOriginProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Point s_Center;

	internal const double c_RadiusX = 0.5;

	internal const double c_RadiusY = 0.5;

	internal static Point s_GradientOrigin;

	/// <summary> Gets or sets the center of the outermost circle of the radial gradient. </summary>
	/// <returns>The two-dimensional point located at the center of the radial gradient.</returns>
	public Point Center
	{
		get
		{
			return (Point)GetValue(CenterProperty);
		}
		set
		{
			SetValueInternal(CenterProperty, value);
		}
	}

	/// <summary> Gets or sets the horizontal radius of the outermost circle of the radial gradient. </summary>
	/// <returns>The horizontal radius of the outermost circle of the radial gradient. The default is 0.5.</returns>
	public double RadiusX
	{
		get
		{
			return (double)GetValue(RadiusXProperty);
		}
		set
		{
			SetValueInternal(RadiusXProperty, value);
		}
	}

	/// <summary> Gets or sets the vertical radius of the outermost circle of a radial gradient. </summary>
	/// <returns>The vertical radius of the outermost circle of a radial gradient. The default is 0.5.</returns>
	public double RadiusY
	{
		get
		{
			return (double)GetValue(RadiusYProperty);
		}
		set
		{
			SetValueInternal(RadiusYProperty, value);
		}
	}

	/// <summary> Gets or sets the location of the two-dimensional focal point that defines the beginning of the gradient. </summary>
	/// <returns>The location of the two-dimensional focal point of the gradient. The default is (0.5, 0.5).</returns>
	public Point GradientOrigin
	{
		get
		{
			return (Point)GetValue(GradientOriginProperty);
		}
		set
		{
			SetValueInternal(GradientOriginProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.RadialGradientBrush" /> class. </summary>
	public RadialGradientBrush()
	{
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.RadialGradientBrush" /> class with the specified start and stop colors. </summary>
	/// <param name="startColor">Color value at the focus (<see cref="P:System.Windows.Media.RadialGradientBrush.GradientOrigin" />) of the radial gradient.</param>
	/// <param name="endColor">Color value at the outer edge of the radial gradient.</param>
	public RadialGradientBrush(Color startColor, Color endColor)
	{
		base.GradientStops.Add(new GradientStop(startColor, 0.0));
		base.GradientStops.Add(new GradientStop(endColor, 1.0));
	}

	/// <summary> Initializes a new instance of the <see cref="T:System.Windows.Media.RadialGradientBrush" /> class that has the specified gradient stops. </summary>
	/// <param name="gradientStopCollection">The gradient stops to set on this brush.</param>
	public RadialGradientBrush(GradientStopCollection gradientStopCollection)
		: base(gradientStopCollection)
	{
	}

	private unsafe void ManualUpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			Transform transform = base.Transform;
			Transform relativeTransform = base.RelativeTransform;
			GradientStopCollection gradientStops = base.GradientStops;
			DUCE.ResourceHandle hTransform = ((transform != null && transform != Transform.Identity) ? ((DUCE.IResource)transform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle hRelativeTransform = ((relativeTransform != null && relativeTransform != Transform.Identity) ? ((DUCE.IResource)relativeTransform).GetHandle(channel) : DUCE.ResourceHandle.Null);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(Brush.OpacityProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(CenterProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(RadiusXProperty, channel);
			DUCE.ResourceHandle animationResourceHandle4 = GetAnimationResourceHandle(RadiusYProperty, channel);
			DUCE.ResourceHandle animationResourceHandle5 = GetAnimationResourceHandle(GradientOriginProperty, channel);
			DUCE.MILCMD_RADIALGRADIENTBRUSH mILCMD_RADIALGRADIENTBRUSH = default(DUCE.MILCMD_RADIALGRADIENTBRUSH);
			mILCMD_RADIALGRADIENTBRUSH.Type = MILCMD.MilCmdRadialGradientBrush;
			mILCMD_RADIALGRADIENTBRUSH.Handle = _duceResource.GetHandle(channel);
			double opacity = base.Opacity;
			DUCE.CopyBytes((byte*)(&mILCMD_RADIALGRADIENTBRUSH.Opacity), (byte*)(&opacity), 8);
			mILCMD_RADIALGRADIENTBRUSH.hOpacityAnimations = animationResourceHandle;
			mILCMD_RADIALGRADIENTBRUSH.hTransform = hTransform;
			mILCMD_RADIALGRADIENTBRUSH.hRelativeTransform = hRelativeTransform;
			mILCMD_RADIALGRADIENTBRUSH.ColorInterpolationMode = base.ColorInterpolationMode;
			mILCMD_RADIALGRADIENTBRUSH.MappingMode = base.MappingMode;
			mILCMD_RADIALGRADIENTBRUSH.SpreadMethod = base.SpreadMethod;
			Point center = Center;
			DUCE.CopyBytes((byte*)(&mILCMD_RADIALGRADIENTBRUSH.Center), (byte*)(&center), 16);
			mILCMD_RADIALGRADIENTBRUSH.hCenterAnimations = animationResourceHandle2;
			double radiusX = RadiusX;
			DUCE.CopyBytes((byte*)(&mILCMD_RADIALGRADIENTBRUSH.RadiusX), (byte*)(&radiusX), 8);
			mILCMD_RADIALGRADIENTBRUSH.hRadiusXAnimations = animationResourceHandle3;
			double radiusY = RadiusY;
			DUCE.CopyBytes((byte*)(&mILCMD_RADIALGRADIENTBRUSH.RadiusY), (byte*)(&radiusY), 8);
			mILCMD_RADIALGRADIENTBRUSH.hRadiusYAnimations = animationResourceHandle4;
			Point gradientOrigin = GradientOrigin;
			DUCE.CopyBytes((byte*)(&mILCMD_RADIALGRADIENTBRUSH.GradientOrigin), (byte*)(&gradientOrigin), 16);
			mILCMD_RADIALGRADIENTBRUSH.hGradientOriginAnimations = animationResourceHandle5;
			int num = gradientStops?.Count ?? 0;
			mILCMD_RADIALGRADIENTBRUSH.GradientStopsSize = (uint)(sizeof(DUCE.MIL_GRADIENTSTOP) * num);
			channel.BeginCommand((byte*)(&mILCMD_RADIALGRADIENTBRUSH), sizeof(DUCE.MILCMD_RADIALGRADIENTBRUSH), sizeof(DUCE.MIL_GRADIENTSTOP) * num);
			DUCE.MIL_GRADIENTSTOP mIL_GRADIENTSTOP = default(DUCE.MIL_GRADIENTSTOP);
			for (int i = 0; i < num; i++)
			{
				GradientStop gradientStop = gradientStops.Internal_GetItem(i);
				double offset = gradientStop.Offset;
				DUCE.CopyBytes((byte*)(&mIL_GRADIENTSTOP.Position), (byte*)(&offset), 8);
				mIL_GRADIENTSTOP.Color = CompositionResourceManager.ColorToMilColorF(gradientStop.Color);
				channel.AppendCommandData((byte*)(&mIL_GRADIENTSTOP), sizeof(DUCE.MIL_GRADIENTSTOP));
			}
			channel.EndCommand();
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.RadialGradientBrush" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new RadialGradientBrush Clone()
	{
		return (RadialGradientBrush)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.RadialGradientBrush" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new RadialGradientBrush CloneCurrentValue()
	{
		return (RadialGradientBrush)base.CloneCurrentValue();
	}

	private static void CenterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((RadialGradientBrush)d).PropertyChanged(CenterProperty);
	}

	private static void RadiusXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((RadialGradientBrush)d).PropertyChanged(RadiusXProperty);
	}

	private static void RadiusYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((RadialGradientBrush)d).PropertyChanged(RadiusYProperty);
	}

	private static void GradientOriginPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((RadialGradientBrush)d).PropertyChanged(GradientOriginProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new RadialGradientBrush();
	}

	internal override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		ManualUpdateResource(channel, skipOnChannelCheck);
		base.UpdateResource(channel, skipOnChannelCheck);
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_RADIALGRADIENTBRUSH))
		{
			((DUCE.IResource)base.Transform)?.AddRefOnChannel(channel);
			((DUCE.IResource)base.RelativeTransform)?.AddRefOnChannel(channel);
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

	static RadialGradientBrush()
	{
		s_Center = new Point(0.5, 0.5);
		s_GradientOrigin = new Point(0.5, 0.5);
		Type typeFromHandle = typeof(RadialGradientBrush);
		CenterProperty = Animatable.RegisterProperty("Center", typeof(Point), typeFromHandle, new Point(0.5, 0.5), CenterPropertyChanged, null, isIndependentlyAnimated: true, null);
		RadiusXProperty = Animatable.RegisterProperty("RadiusX", typeof(double), typeFromHandle, 0.5, RadiusXPropertyChanged, null, isIndependentlyAnimated: true, null);
		RadiusYProperty = Animatable.RegisterProperty("RadiusY", typeof(double), typeFromHandle, 0.5, RadiusYPropertyChanged, null, isIndependentlyAnimated: true, null);
		GradientOriginProperty = Animatable.RegisterProperty("GradientOrigin", typeof(Point), typeFromHandle, new Point(0.5, 0.5), GradientOriginPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
