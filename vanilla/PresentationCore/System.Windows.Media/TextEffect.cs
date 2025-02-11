using System.Windows.Media.Animation;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Represents a text effect that can be applied to text objects.</summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public sealed class TextEffect : Animatable
{
	/// <summary>Identifies the <see cref="P:System.Windows.Media.TextEffect.Transform" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TextEffect.Transform" /> dependency property.</returns>
	public static readonly DependencyProperty TransformProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TextEffect.Clip" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TextEffect.Clip" /> dependency property.</returns>
	public static readonly DependencyProperty ClipProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TextEffect.Foreground" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TextEffect.Foreground" /> dependency property.</returns>
	public static readonly DependencyProperty ForegroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TextEffect.PositionStart" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TextEffect.PositionStart" /> dependency property.</returns>
	public static readonly DependencyProperty PositionStartProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Media.TextEffect.PositionCount" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.TextEffect.PositionCount" /> dependency property.</returns>
	public static readonly DependencyProperty PositionCountProperty;

	internal const int c_PositionStart = 0;

	internal const int c_PositionCount = 0;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Transform" /> that is applied to the <see cref="T:System.Windows.Media.TextEffect" />.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Transform" /> value of the <see cref="T:System.Windows.Media.TextEffect" />.</returns>
	public Transform Transform
	{
		get
		{
			return (Transform)GetValue(TransformProperty);
		}
		set
		{
			SetValueInternal(TransformProperty, value);
		}
	}

	/// <summary>Gets or sets the clipping region of the <see cref="T:System.Windows.Media.TextEffect" />.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Geometry" /> that defines the clipping region.</returns>
	public Geometry Clip
	{
		get
		{
			return (Geometry)GetValue(ClipProperty);
		}
		set
		{
			SetValueInternal(ClipProperty, value);
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> to apply to the content of the <see cref="T:System.Windows.Media.TextEffect" />.  </summary>
	/// <returns>The brush used to apply to the <see cref="T:System.Windows.Media.TextEffect" />.</returns>
	public Brush Foreground
	{
		get
		{
			return (Brush)GetValue(ForegroundProperty);
		}
		set
		{
			SetValueInternal(ForegroundProperty, value);
		}
	}

	/// <summary>Gets or sets the starting position in the text that the <see cref="T:System.Windows.Media.TextEffect" /> applies to.  </summary>
	/// <returns>The <see cref="T:System.Int32" /> value representing the starting position in the text that the <see cref="T:System.Windows.Media.TextEffect" /> applies to.</returns>
	public int PositionStart
	{
		get
		{
			return (int)GetValue(PositionStartProperty);
		}
		set
		{
			SetValueInternal(PositionStartProperty, value);
		}
	}

	/// <summary>Gets or sets the position in the text that the <see cref="T:System.Windows.Media.TextEffect" /> applies to.  </summary>
	/// <returns>The <see cref="T:System.Int32" /> value representing the position in the text that the <see cref="T:System.Windows.Media.TextEffect" /> applies to.</returns>
	public int PositionCount
	{
		get
		{
			return (int)GetValue(PositionCountProperty);
		}
		set
		{
			SetValueInternal(PositionCountProperty, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextEffect" /> class by specifying class property values.</summary>
	/// <param name="transform">The <see cref="T:System.Windows.Media.Transform" /> that is applied to the <see cref="T:System.Windows.Media.TextEffect" />.</param>
	/// <param name="foreground">The <see cref="T:System.Windows.Media.Brush" /> to apply to the content of the <see cref="T:System.Windows.Media.TextEffect" />.</param>
	/// <param name="clip">The clipping region of the <see cref="T:System.Windows.Media.TextEffect" />.</param>
	/// <param name="positionStart">The starting position in the text that the <see cref="T:System.Windows.Media.TextEffect" /> applies to.</param>
	/// <param name="positionCount">The number of positions in the text that the <see cref="T:System.Windows.Media.TextEffect" /> applies to.</param>
	public TextEffect(Transform transform, Brush foreground, Geometry clip, int positionStart, int positionCount)
	{
		if (positionCount < 0)
		{
			throw new ArgumentOutOfRangeException("positionCount", SR.ParameterCannotBeNegative);
		}
		Transform = transform;
		Foreground = foreground;
		Clip = clip;
		PositionStart = positionStart;
		PositionCount = positionCount;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextEffect" /> class.</summary>
	public TextEffect()
	{
	}

	private static bool OnPositionStartChanging(int value)
	{
		return value >= 0;
	}

	private static bool OnPositionCountChanging(int value)
	{
		return value >= 0;
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.TextEffect" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TextEffect Clone()
	{
		return (TextEffect)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.TextEffect" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new TextEffect CloneCurrentValue()
	{
		return (TextEffect)base.CloneCurrentValue();
	}

	private static bool ValidatePositionStartValue(object value)
	{
		if (!OnPositionStartChanging((int)value))
		{
			return false;
		}
		return true;
	}

	private static bool ValidatePositionCountValue(object value)
	{
		if (!OnPositionCountChanging((int)value))
		{
			return false;
		}
		return true;
	}

	protected override Freezable CreateInstanceCore()
	{
		return new TextEffect();
	}

	static TextEffect()
	{
		Type typeFromHandle = typeof(TextEffect);
		TransformProperty = Animatable.RegisterProperty("Transform", typeof(Transform), typeFromHandle, null, null, null, isIndependentlyAnimated: false, null);
		ClipProperty = Animatable.RegisterProperty("Clip", typeof(Geometry), typeFromHandle, null, null, null, isIndependentlyAnimated: false, null);
		ForegroundProperty = Animatable.RegisterProperty("Foreground", typeof(Brush), typeFromHandle, null, null, null, isIndependentlyAnimated: false, null);
		PositionStartProperty = Animatable.RegisterProperty("PositionStart", typeof(int), typeFromHandle, 0, null, ValidatePositionStartValue, isIndependentlyAnimated: false, null);
		PositionCountProperty = Animatable.RegisterProperty("PositionCount", typeof(int), typeFromHandle, 0, null, ValidatePositionCountValue, isIndependentlyAnimated: false, null);
	}
}
