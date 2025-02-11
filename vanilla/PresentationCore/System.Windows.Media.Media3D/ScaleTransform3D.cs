using System.Windows.Media.Animation;
using System.Windows.Media.Composition;

namespace System.Windows.Media.Media3D;

/// <summary>Scales an object in the three-dimensional x-y-z plane, starting from a defined center point. Scale factors are defined in x-, y-, and z- directions from this center point. </summary>
public sealed class ScaleTransform3D : AffineTransform3D
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ScaleTransform3D.ScaleX" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.ScaleTransform3D.ScaleX" /> dependency property.</returns>
	public static readonly DependencyProperty ScaleXProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ScaleTransform3D.ScaleY" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.ScaleTransform3D.ScaleY" /> dependency property.</returns>
	public static readonly DependencyProperty ScaleYProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ScaleTransform3D.ScaleZ" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.ScaleTransform3D.ScaleZ" /> dependency property.</returns>
	public static readonly DependencyProperty ScaleZProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ScaleTransform3D.CenterX" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.ScaleTransform3D.CenterX" /> dependency property.</returns>
	public static readonly DependencyProperty CenterXProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ScaleTransform3D.CenterY" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.ScaleTransform3D.CenterY" /> dependency property.</returns>
	public static readonly DependencyProperty CenterYProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.ScaleTransform3D.CenterZ" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.ScaleTransform3D.CenterZ" /> dependency property.</returns>
	public static readonly DependencyProperty CenterZProperty;

	private double _cachedScaleXValue = 1.0;

	private double _cachedScaleYValue = 1.0;

	private double _cachedScaleZValue = 1.0;

	private double _cachedCenterXValue;

	private double _cachedCenterYValue;

	private double _cachedCenterZValue;

	internal DUCE.MultiChannelResource _duceResource;

	internal const double c_ScaleX = 1.0;

	internal const double c_ScaleY = 1.0;

	internal const double c_ScaleZ = 1.0;

	internal const double c_CenterX = 0.0;

	internal const double c_CenterY = 0.0;

	internal const double c_CenterZ = 0.0;

	/// <summary>Gets a <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> representation of this transformation. </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Matrix3D" /> representation of this transformation.</returns>
	public override Matrix3D Value
	{
		get
		{
			ReadPreamble();
			Matrix3D matrix = default(Matrix3D);
			Append(ref matrix);
			return matrix;
		}
	}

	/// <summary>Gets or sets the scale factor in the x-direction.  </summary>
	/// <returns>Scale factor in the x-direction. The default value is 1.</returns>
	public double ScaleX
	{
		get
		{
			ReadPreamble();
			return _cachedScaleXValue;
		}
		set
		{
			SetValueInternal(ScaleXProperty, value);
		}
	}

	/// <summary>Gets or sets the scale factor in the y-direction.  </summary>
	/// <returns>Scale factor in the y-direction. The default value is 1.</returns>
	public double ScaleY
	{
		get
		{
			ReadPreamble();
			return _cachedScaleYValue;
		}
		set
		{
			SetValueInternal(ScaleYProperty, value);
		}
	}

	/// <summary>Gets or sets the scale factor in the z-direction.  </summary>
	/// <returns>Scale factor in the z-direction. The default value is 1.</returns>
	public double ScaleZ
	{
		get
		{
			ReadPreamble();
			return _cachedScaleZValue;
		}
		set
		{
			SetValueInternal(ScaleZProperty, value);
		}
	}

	/// <summary>Gets or sets the x-coordinate of the transform's center point.  </summary>
	/// <returns>The x-coordinate of the transform's center point. The default value is 0.</returns>
	public double CenterX
	{
		get
		{
			ReadPreamble();
			return _cachedCenterXValue;
		}
		set
		{
			SetValueInternal(CenterXProperty, value);
		}
	}

	/// <summary>Gets or sets the z-coordinate of the transform's center point.  </summary>
	/// <returns>The y-coordinate of the transform's center point. The default value is 0.</returns>
	public double CenterY
	{
		get
		{
			ReadPreamble();
			return _cachedCenterYValue;
		}
		set
		{
			SetValueInternal(CenterYProperty, value);
		}
	}

	/// <summary>Gets or sets the z-coordinate of the transform's center point.  </summary>
	/// <returns>The z-coordinate of the transform's center point. The default value is 0.</returns>
	public double CenterZ
	{
		get
		{
			ReadPreamble();
			return _cachedCenterZValue;
		}
		set
		{
			SetValueInternal(CenterZProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.ScaleTransform3D" /> class. </summary>
	public ScaleTransform3D()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.ScaleTransform3D" /> class using the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" />. </summary>
	/// <param name="scale">Vector3D along which the transformation scales.</param>
	public ScaleTransform3D(Vector3D scale)
	{
		ScaleX = scale.X;
		ScaleY = scale.Y;
		ScaleZ = scale.Z;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.ScaleTransform3D" /> class using the specified scale factors.</summary>
	/// <param name="scaleX">Factor by which to scale the X value.</param>
	/// <param name="scaleY">Factor by which to scale the Y value.</param>
	/// <param name="scaleZ">Factor by which to scale the Z value.</param>
	public ScaleTransform3D(double scaleX, double scaleY, double scaleZ)
	{
		ScaleX = scaleX;
		ScaleY = scaleY;
		ScaleZ = scaleZ;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.ScaleTransform3D" /> class using the specified <see cref="T:System.Windows.Media.Media3D.Vector3D" /> and <see cref="T:System.Windows.Media.Media3D.Point3D" />. </summary>
	/// <param name="scale">Vector3D along which the transformation scales.</param>
	/// <param name="center">Center around which the transformation scales.</param>
	public ScaleTransform3D(Vector3D scale, Point3D center)
	{
		ScaleX = scale.X;
		ScaleY = scale.Y;
		ScaleZ = scale.Z;
		CenterX = center.X;
		CenterY = center.Y;
		CenterZ = center.Z;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Media3D.ScaleTransform3D" /> class using the specified center coordinates and scale factors.</summary>
	/// <param name="scaleX">Factor by which to scale the X value.</param>
	/// <param name="scaleY">Factor by which to scale the Y value.</param>
	/// <param name="scaleZ">Factor by which to scale the Z value.</param>
	/// <param name="centerX">X coordinate of the center point from which to scale.</param>
	/// <param name="centerY">Y coordinate of the center point from which to scale.</param>
	/// <param name="centerZ">Z coordinate of the center point from which to scale.</param>
	public ScaleTransform3D(double scaleX, double scaleY, double scaleZ, double centerX, double centerY, double centerZ)
	{
		ScaleX = scaleX;
		ScaleY = scaleY;
		ScaleZ = scaleZ;
		CenterX = centerX;
		CenterY = centerY;
		CenterZ = centerZ;
	}

	internal override void Append(ref Matrix3D matrix)
	{
		Vector3D scale = new Vector3D(_cachedScaleXValue, _cachedScaleYValue, _cachedScaleZValue);
		if (_cachedCenterXValue == 0.0 && _cachedCenterYValue == 0.0 && _cachedCenterZValue == 0.0)
		{
			matrix.Scale(scale);
		}
		else
		{
			matrix.ScaleAt(scale, new Point3D(_cachedCenterXValue, _cachedCenterYValue, _cachedCenterZValue));
		}
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.ScaleTransform3D" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ScaleTransform3D Clone()
	{
		return (ScaleTransform3D)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.ScaleTransform3D" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ScaleTransform3D CloneCurrentValue()
	{
		return (ScaleTransform3D)base.CloneCurrentValue();
	}

	private static void ScaleXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ScaleTransform3D obj = (ScaleTransform3D)d;
		obj._cachedScaleXValue = (double)e.NewValue;
		obj.PropertyChanged(ScaleXProperty);
	}

	private static void ScaleYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ScaleTransform3D obj = (ScaleTransform3D)d;
		obj._cachedScaleYValue = (double)e.NewValue;
		obj.PropertyChanged(ScaleYProperty);
	}

	private static void ScaleZPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ScaleTransform3D obj = (ScaleTransform3D)d;
		obj._cachedScaleZValue = (double)e.NewValue;
		obj.PropertyChanged(ScaleZProperty);
	}

	private static void CenterXPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ScaleTransform3D obj = (ScaleTransform3D)d;
		obj._cachedCenterXValue = (double)e.NewValue;
		obj.PropertyChanged(CenterXProperty);
	}

	private static void CenterYPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ScaleTransform3D obj = (ScaleTransform3D)d;
		obj._cachedCenterYValue = (double)e.NewValue;
		obj.PropertyChanged(CenterYProperty);
	}

	private static void CenterZPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ScaleTransform3D obj = (ScaleTransform3D)d;
		obj._cachedCenterZValue = (double)e.NewValue;
		obj.PropertyChanged(CenterZProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new ScaleTransform3D();
	}

	internal unsafe override void UpdateResource(DUCE.Channel channel, bool skipOnChannelCheck)
	{
		if (skipOnChannelCheck || _duceResource.IsOnChannel(channel))
		{
			base.UpdateResource(channel, skipOnChannelCheck);
			DUCE.ResourceHandle animationResourceHandle = GetAnimationResourceHandle(ScaleXProperty, channel);
			DUCE.ResourceHandle animationResourceHandle2 = GetAnimationResourceHandle(ScaleYProperty, channel);
			DUCE.ResourceHandle animationResourceHandle3 = GetAnimationResourceHandle(ScaleZProperty, channel);
			DUCE.ResourceHandle animationResourceHandle4 = GetAnimationResourceHandle(CenterXProperty, channel);
			DUCE.ResourceHandle animationResourceHandle5 = GetAnimationResourceHandle(CenterYProperty, channel);
			DUCE.ResourceHandle animationResourceHandle6 = GetAnimationResourceHandle(CenterZProperty, channel);
			DUCE.MILCMD_SCALETRANSFORM3D mILCMD_SCALETRANSFORM3D = default(DUCE.MILCMD_SCALETRANSFORM3D);
			mILCMD_SCALETRANSFORM3D.Type = MILCMD.MilCmdScaleTransform3D;
			mILCMD_SCALETRANSFORM3D.Handle = _duceResource.GetHandle(channel);
			if (animationResourceHandle.IsNull)
			{
				mILCMD_SCALETRANSFORM3D.scaleX = ScaleX;
			}
			mILCMD_SCALETRANSFORM3D.hScaleXAnimations = animationResourceHandle;
			if (animationResourceHandle2.IsNull)
			{
				mILCMD_SCALETRANSFORM3D.scaleY = ScaleY;
			}
			mILCMD_SCALETRANSFORM3D.hScaleYAnimations = animationResourceHandle2;
			if (animationResourceHandle3.IsNull)
			{
				mILCMD_SCALETRANSFORM3D.scaleZ = ScaleZ;
			}
			mILCMD_SCALETRANSFORM3D.hScaleZAnimations = animationResourceHandle3;
			if (animationResourceHandle4.IsNull)
			{
				mILCMD_SCALETRANSFORM3D.centerX = CenterX;
			}
			mILCMD_SCALETRANSFORM3D.hCenterXAnimations = animationResourceHandle4;
			if (animationResourceHandle5.IsNull)
			{
				mILCMD_SCALETRANSFORM3D.centerY = CenterY;
			}
			mILCMD_SCALETRANSFORM3D.hCenterYAnimations = animationResourceHandle5;
			if (animationResourceHandle6.IsNull)
			{
				mILCMD_SCALETRANSFORM3D.centerZ = CenterZ;
			}
			mILCMD_SCALETRANSFORM3D.hCenterZAnimations = animationResourceHandle6;
			channel.SendCommand((byte*)(&mILCMD_SCALETRANSFORM3D), sizeof(DUCE.MILCMD_SCALETRANSFORM3D));
		}
	}

	internal override DUCE.ResourceHandle AddRefOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.CreateOrAddRefOnChannel(this, channel, DUCE.ResourceType.TYPE_SCALETRANSFORM3D))
		{
			AddRefOnChannelAnimations(channel);
			UpdateResource(channel, skipOnChannelCheck: true);
		}
		return _duceResource.GetHandle(channel);
	}

	internal override void ReleaseOnChannelCore(DUCE.Channel channel)
	{
		if (_duceResource.ReleaseOnChannel(channel))
		{
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

	static ScaleTransform3D()
	{
		Type typeFromHandle = typeof(ScaleTransform3D);
		ScaleXProperty = Animatable.RegisterProperty("ScaleX", typeof(double), typeFromHandle, 1.0, ScaleXPropertyChanged, null, isIndependentlyAnimated: true, null);
		ScaleYProperty = Animatable.RegisterProperty("ScaleY", typeof(double), typeFromHandle, 1.0, ScaleYPropertyChanged, null, isIndependentlyAnimated: true, null);
		ScaleZProperty = Animatable.RegisterProperty("ScaleZ", typeof(double), typeFromHandle, 1.0, ScaleZPropertyChanged, null, isIndependentlyAnimated: true, null);
		CenterXProperty = Animatable.RegisterProperty("CenterX", typeof(double), typeFromHandle, 0.0, CenterXPropertyChanged, null, isIndependentlyAnimated: true, null);
		CenterYProperty = Animatable.RegisterProperty("CenterY", typeof(double), typeFromHandle, 0.0, CenterYPropertyChanged, null, isIndependentlyAnimated: true, null);
		CenterZProperty = Animatable.RegisterProperty("CenterZ", typeof(double), typeFromHandle, 0.0, CenterZPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
