using System.Windows.Media.Animation;

namespace System.Windows.Media.Media3D;

/// <summary>
///   <see cref="T:System.Windows.Media.Media3D.Model3D" /> object that represents lighting applied to a 3-D scene.  </summary>
public abstract class Light : Model3D
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.Media3D.Light.Color" /> dependency property.</summary>
	/// <returns>The <see cref="P:System.Windows.Media.Media3D.Light.Color" /> dependency property identifier.</returns>
	public static readonly DependencyProperty ColorProperty;

	internal static Color s_Color;

	/// <summary> Gets or sets the color of the light. </summary>
	/// <returns>Color of the light.</returns>
	public Color Color
	{
		get
		{
			return (Color)GetValue(ColorProperty);
		}
		set
		{
			SetValueInternal(ColorProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	internal Light()
	{
	}

	internal override void RayHitTestCore(RayHitTestParameters rayParams)
	{
	}

	internal override Rect3D CalculateSubgraphBoundsInnerSpace()
	{
		return Rect3D.Empty;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Light" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Light Clone()
	{
		return (Light)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.Light" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new Light CloneCurrentValue()
	{
		return (Light)base.CloneCurrentValue();
	}

	private static void ColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((Light)d).PropertyChanged(ColorProperty);
	}

	static Light()
	{
		s_Color = Colors.White;
		Type typeFromHandle = typeof(Light);
		ColorProperty = Animatable.RegisterProperty("Color", typeof(Color), typeFromHandle, Colors.White, ColorPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
