using System.Windows.Media.Animation;

namespace System.Windows.Media.Media3D;

/// <summary>Abstract base class that represents a light object that has a position in space and projects its light in all directions. </summary>
public abstract class PointLightBase : Light
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.PointLightBase.Position" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.PointLightBase.Position" /> dependency property.</returns>
	public static readonly DependencyProperty PositionProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.PointLightBase.Range" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.PointLightBase.Range" /> dependency property.</returns>
	public static readonly DependencyProperty RangeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.PointLightBase.ConstantAttenuation" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.PointLightBase.ConstantAttenuation" /> dependency property.</returns>
	public static readonly DependencyProperty ConstantAttenuationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.PointLightBase.LinearAttenuation" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.PointLightBase.LinearAttenuation" /> dependency property.</returns>
	public static readonly DependencyProperty LinearAttenuationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.PointLightBase.QuadraticAttenuation" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Media3D.PointLightBase.QuadraticAttenuation" /> dependency property.</returns>
	public static readonly DependencyProperty QuadraticAttenuationProperty;

	internal static Point3D s_Position;

	internal const double c_Range = double.PositiveInfinity;

	internal const double c_ConstantAttenuation = 1.0;

	internal const double c_LinearAttenuation = 0.0;

	internal const double c_QuadraticAttenuation = 0.0;

	/// <summary>Gets or sets a <see cref="T:System.Windows.Media.Media3D.Point3D" /> that specifies the light's position in world space.  </summary>
	/// <returns>
	///   <see cref="T:System.Windows.Media.Media3D.Point3D" /> that specifies the light's position in world space.</returns>
	public Point3D Position
	{
		get
		{
			return (Point3D)GetValue(PositionProperty);
		}
		set
		{
			SetValueInternal(PositionProperty, value);
		}
	}

	/// <summary>Gets or sets the distance beyond which the light has no effect.  </summary>
	/// <returns>Double that specifies the distance beyond which the light has no effect.</returns>
	public double Range
	{
		get
		{
			return (double)GetValue(RangeProperty);
		}
		set
		{
			SetValueInternal(RangeProperty, value);
		}
	}

	/// <summary>Gets or sets a constant value by which the intensity of the light diminishes over distance.  </summary>
	/// <returns>Double by which the intensity of the light diminishes over distance.</returns>
	public double ConstantAttenuation
	{
		get
		{
			return (double)GetValue(ConstantAttenuationProperty);
		}
		set
		{
			SetValueInternal(ConstantAttenuationProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the linear diminution of the light's intensity over distance.  </summary>
	/// <returns>Double that specifies the linear diminution of the light's intensity over distance.</returns>
	public double LinearAttenuation
	{
		get
		{
			return (double)GetValue(LinearAttenuationProperty);
		}
		set
		{
			SetValueInternal(LinearAttenuationProperty, value);
		}
	}

	/// <summary>Gets or sets a value that specifies the diminution of the light's effect over distance, calculated by a quadratic operation.  </summary>
	/// <returns>Double that specifies the diminution of the light's effect over distance, calculated by a quadratic operation.</returns>
	public double QuadraticAttenuation
	{
		get
		{
			return (double)GetValue(QuadraticAttenuationProperty);
		}
		set
		{
			SetValueInternal(QuadraticAttenuationProperty, value);
		}
	}

	internal PointLightBase()
	{
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.PointLightBase" /> object, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PointLightBase Clone()
	{
		return (PointLightBase)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Animation.ByteAnimationUsingKeyFrames" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new PointLightBase CloneCurrentValue()
	{
		return (PointLightBase)base.CloneCurrentValue();
	}

	private static void PositionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((PointLightBase)d).PropertyChanged(PositionProperty);
	}

	private static void RangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((PointLightBase)d).PropertyChanged(RangeProperty);
	}

	private static void ConstantAttenuationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((PointLightBase)d).PropertyChanged(ConstantAttenuationProperty);
	}

	private static void LinearAttenuationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((PointLightBase)d).PropertyChanged(LinearAttenuationProperty);
	}

	private static void QuadraticAttenuationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((PointLightBase)d).PropertyChanged(QuadraticAttenuationProperty);
	}

	static PointLightBase()
	{
		Type typeFromHandle = typeof(PointLightBase);
		PositionProperty = Animatable.RegisterProperty("Position", typeof(Point3D), typeFromHandle, default(Point3D), PositionPropertyChanged, null, isIndependentlyAnimated: true, null);
		RangeProperty = Animatable.RegisterProperty("Range", typeof(double), typeFromHandle, double.PositiveInfinity, RangePropertyChanged, null, isIndependentlyAnimated: true, null);
		ConstantAttenuationProperty = Animatable.RegisterProperty("ConstantAttenuation", typeof(double), typeFromHandle, 1.0, ConstantAttenuationPropertyChanged, null, isIndependentlyAnimated: true, null);
		LinearAttenuationProperty = Animatable.RegisterProperty("LinearAttenuation", typeof(double), typeFromHandle, 0.0, LinearAttenuationPropertyChanged, null, isIndependentlyAnimated: true, null);
		QuadraticAttenuationProperty = Animatable.RegisterProperty("QuadraticAttenuation", typeof(double), typeFromHandle, 0.0, QuadraticAttenuationPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
