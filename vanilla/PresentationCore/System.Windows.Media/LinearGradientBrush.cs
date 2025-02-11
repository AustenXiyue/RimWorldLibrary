using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media;

/// <summary>Paints an area with a linear gradient. </summary>
public sealed class LinearGradientBrush : GradientBrush
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.LinearGradientBrush.StartPoint" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.LinearGradientBrush.StartPoint" /> dependency property.</returns>
	public static readonly DependencyProperty StartPointProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.LinearGradientBrush.EndPoint" /> dependency property.  </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.LinearGradientBrush.EndPoint" /> dependency property.</returns>
	public static readonly DependencyProperty EndPointProperty;

	internal DUCE.MultiChannelResource _duceResource;

	internal static Point s_StartPoint;

	internal static Point s_EndPoint;

	/// <summary>Gets or sets the starting two-dimensional coordinates of the linear gradient.  </summary>
	/// <returns>The starting two-dimensional coordinates for the linear gradient. The default is (0, 0). </returns>
	public Point StartPoint
	{
		get
		{
			return (Point)GetValue(StartPointProperty);
		}
		set
		{
			SetValueInternal(StartPointProperty, value);
		}
	}

	/// <summary>Gets or sets the ending two-dimensional coordinates of the linear gradient.  </summary>
	/// <returns>The ending two-dimensional coordinates of the linear gradient. The default is (1,1).  </returns>
	public Point EndPoint
	{
		get
		{
			return (Point)GetValue(EndPointProperty);
		}
		set
		{
			SetValueInternal(EndPointProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 3;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.LinearGradientBrush" /> class. </summary>
	public LinearGradientBrush()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.LinearGradientBrush" /> class that has the specified start <see cref="T:System.Windows.Media.Color" />, end <see cref="T:System.Windows.Media.Color" />, and angle. </summary>
	/// <param name="startColor">The <see cref="T:System.Windows.Media.Color" /> at offset 0.0.</param>
	/// <param name="endColor">The <see cref="T:System.Windows.Media.Color" /> at offset 1.0.  </param>
	/// <param name="angle">A <see cref="T:System.Double" /> that represents the angle, in degrees, of the gradient. A value of 0.0 creates a horizontal gradient, and a value of 90.0 creates a vertical gradient.</param>
	public LinearGradientBrush(Color startColor, Color endColor, double angle)
	{
		EndPoint = EndPointFromAngle(angle);
		base.GradientStops.Add(new GradientStop(startColor, 0.0));
		base.GradientStops.Add(new GradientStop(endColor, 1.0));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.LinearGradientBrush" /> class that has the specified start <see cref="T:System.Windows.Media.Color" />, end <see cref="T:System.Windows.Media.Color" />, <see cref="P:System.Windows.Media.LinearGradientBrush.StartPoint" />, and <see cref="P:System.Windows.Media.LinearGradientBrush.EndPoint" />. </summary>
	/// <param name="startColor">The <see cref="T:System.Windows.Media.Color" /> at offset 0.0.</param>
	/// <param name="endColor">The <see cref="T:System.Windows.Media.Color" /> at offset 1.0. </param>
	/// <param name="startPoint">The <see cref="P:System.Windows.Media.LinearGradientBrush.StartPoint" /> of the gradient.</param>
	/// <param name="endPoint">The <see cref="P:System.Windows.Media.LinearGradientBrush.EndPoint" /> of the gradient.</param>
	public LinearGradientBrush(Color startColor, Color endColor, Point startPoint, Point endPoint)
	{
		StartPoint = startPoint;
		EndPoint = endPoint;
		base.GradientStops.Add(new GradientStop(startColor, 0.0));
		base.GradientStops.Add(new GradientStop(endColor, 1.0));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.LinearGradientBrush" /> class that has the specified gradient stops. </summary>
	/// <param name="gradientStopCollection">The <see cref="P:System.Windows.Media.GradientBrush.GradientStops" /> to set on this brush.</param>
	public LinearGradientBrush(GradientStopCollection gradientStopCollection)
		: base(gradientStopCollection)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.LinearGradientBrush" /> class that has the specified <see cref="T:System.Windows.Media.GradientStopCollection" /> and angle. </summary>
	/// <param name="gradientStopCollection">The <see cref="P:System.Windows.Media.GradientBrush.GradientStops" /> to set on this brush.</param>
	/// <param name="angle">A <see cref="T:System.Double" /> that represents the angle, in degrees, of the gradient. A value of 0.0 creates a horizontal gradient, and a value of 90.0 creates a vertical gradient.</param>
	public LinearGradientBrush(GradientStopCollection gradientStopCollection, double angle)
		: base(gradientStopCollection)
	{
		EndPoint = EndPointFromAngle(angle);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.LinearGradientBrush" /> class that has the specified gradient stops, <see cref="P:System.Windows.Media.LinearGradientBrush.StartPoint" />, and <see cref="P:System.Windows.Media.LinearGradientBrush.EndPoint" />. </summary>
	/// <param name="gradientStopCollection">The <see cref="P:System.Windows.Media.GradientBrush.GradientStops" /> to set on this brush.</param>
	/// <param name="startPoint">The <see cref="P:System.Windows.Media.LinearGradientBrush.StartPoint" /> of the gradient.</param>
	/// <param name="endPoint">The <see cref="P:System.Windows.Media.LinearGradientBrush.EndPoint" /> of the gradient.</param>
	public LinearGradientBrush(GradientStopCollection gradientStopCollection, Point startPoint, Point endPoint)
		: base(gradientStopCollection)
	{
		StartPoint = startPoint;
		EndPoint = endPoint;
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
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(StartPointProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(EndPointProperty, channel);
			DUCE.MILCMD_LINEARGRADIENTBRUSH mILCMD_LINEARGRADIENTBRUSH = default(DUCE.MILCMD_LINEARGRADIENTBRUSH);
			mILCMD_LINEARGRADIENTBRUSH.Type = MILCMD.MilCmdLinearGradientBrush;
			mILCMD_LINEARGRADIENTBRUSH.Handle = _duceResource.GetHandle(channel);
			double opacity = base.Opacity;
			DUCE.CopyBytes((byte*)(&mILCMD_LINEARGRADIENTBRUSH.Opacity), (byte*)(&opacity), 8);
			mILCMD_LINEARGRADIENTBRUSH.hOpacityAnimations = animationResourceHandle;
			mILCMD_LINEARGRADIENTBRUSH.hTransform = hTransform;
			mILCMD_LINEARGRADIENTBRUSH.hRelativeTransform = hRelativeTransform;
			mILCMD_LINEARGRADIENTBRUSH.ColorInterpolationMode = base.ColorInterpolationMode;
			mILCMD_LINEARGRADIENTBRUSH.MappingMode = base.MappingMode;
			mILCMD_LINEARGRADIENTBRUSH.SpreadMethod = base.SpreadMethod;
			Point startPoint = StartPoint;
			DUCE.CopyBytes((byte*)(&mILCMD_LINEARGRADIENTBRUSH.StartPoint), (byte*)(&startPoint), 16);
			mILCMD_LINEARGRADIENTBRUSH.hStartPointAnimations = animationResourceHandle2;
			Point endPoint = EndPoint;
			DUCE.CopyBytes((byte*)(&mILCMD_LINEARGRADIENTBRUSH.EndPoint), (byte*)(&endPoint), 16);
			mILCMD_LINEARGRADIENTBRUSH.hEndPointAnimations = animationResourceHandle3;
			int num = gradientStops?.Count ?? 0;
			mILCMD_LINEARGRADIENTBRUSH.GradientStopsSize = (uint)(sizeof(DUCE.MIL_GRADIENTSTOP) * num);
			channel.BeginCommand((byte*)(&mILCMD_LINEARGRADIENTBRUSH), sizeof(DUCE.MILCMD_LINEARGRADIENTBRUSH), sizeof(DUCE.MIL_GRADIENTSTOP) * num);
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

	private Point EndPointFromAngle(double angle)
	{
		angle = angle * (1.0 / 180.0) * Math.PI;
		return new Point(Math.Cos(angle), Math.Sin(angle));
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.LinearGradientBrush" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new LinearGradientBrush Clone()
	{
		return (LinearGradientBrush)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.LinearGradientBrush" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new LinearGradientBrush CloneCurrentValue()
	{
		return (LinearGradientBrush)base.CloneCurrentValue();
	}

	private static void StartPointPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((LinearGradientBrush)d).PropertyChanged(StartPointProperty);
	}

	private static void EndPointPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((LinearGradientBrush)d).PropertyChanged(EndPointProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new LinearGradientBrush();
	}

	internal override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		ManualUpdateResource(channel, skipOnChannelCheck);
		base.UpdateResource(channel, skipOnChannelCheck);
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_LINEARGRADIENTBRUSH))
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

	static LinearGradientBrush()
	{
		s_StartPoint = new Point(0.0, 0.0);
		s_EndPoint = new Point(1.0, 1.0);
		Type typeFromHandle = typeof(LinearGradientBrush);
		StartPointProperty = Animatable.RegisterProperty("StartPoint", typeof(Point), typeFromHandle, new Point(0.0, 0.0), StartPointPropertyChanged, null, isIndependentlyAnimated: true, null);
		EndPointProperty = Animatable.RegisterProperty("EndPoint", typeof(Point), typeFromHandle, new Point(1.0, 1.0), EndPointPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
