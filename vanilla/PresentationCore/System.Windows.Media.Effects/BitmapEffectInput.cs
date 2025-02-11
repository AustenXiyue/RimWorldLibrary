using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace System.Windows.Media.Effects;

/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Applies the <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> given in the <see cref="P:System.Windows.UIElement.BitmapEffect" /> property to a specified region of the visual object.</summary>
public sealed class BitmapEffectInput : Animatable
{
	private static BitmapSource s_defaultInputSource;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Identifies the <see cref="P:System.Windows.Media.Effects.BitmapEffectInput.Input" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.BitmapEffectInput.Input" /> dependency property.</returns>
	public static readonly DependencyProperty InputProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Identifies the <see cref="P:System.Windows.Media.Effects.BitmapEffectInput.AreaToApplyEffectUnits" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.BitmapEffectInput.AreaToApplyEffectUnits" /> dependency property.</returns>
	public static readonly DependencyProperty AreaToApplyEffectUnitsProperty;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Identifies the <see cref="P:System.Windows.Media.Effects.BitmapEffectInput.AreaToApplyEffect" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Media.Effects.BitmapEffectInput.AreaToApplyEffect" /> dependency property.</returns>
	public static readonly DependencyProperty AreaToApplyEffectProperty;

	internal static BitmapSource s_Input;

	internal const BrushMappingMode c_AreaToApplyEffectUnits = BrushMappingMode.RelativeToBoundingBox;

	internal static Rect s_AreaToApplyEffect;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets a value representing the <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that is derived from the context.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that is derived from the context.</returns>
	public static BitmapSource ContextInputSource
	{
		get
		{
			if (s_defaultInputSource == null)
			{
				UnmanagedBitmapWrapper unmanagedBitmapWrapper = new UnmanagedBitmapWrapper(initialize: true);
				unmanagedBitmapWrapper.Freeze();
				s_defaultInputSource = unmanagedBitmapWrapper;
			}
			return s_defaultInputSource;
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets or sets the <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that is used for the input for the object.  </summary>
	/// <returns>The <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that is used as the input for the object. The default value is null.</returns>
	public BitmapSource Input
	{
		get
		{
			return (BitmapSource)GetValue(InputProperty);
		}
		set
		{
			SetValueInternal(InputProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets or sets the method in which to interpret the rectangle provided by <see cref="P:System.Windows.Media.Effects.BitmapEffectInput.AreaToApplyEffect" />.  </summary>
	/// <returns>The method in which to interpret the rectangle provided by the <see cref="P:System.Windows.Media.Effects.BitmapEffectInput.AreaToApplyEffectUnits" /> property. The default value is <see cref="F:System.Windows.Media.BrushMappingMode.RelativeToBoundingBox" />.</returns>
	public BrushMappingMode AreaToApplyEffectUnits
	{
		get
		{
			return (BrushMappingMode)GetValue(AreaToApplyEffectUnitsProperty);
		}
		set
		{
			SetValueInternal(AreaToApplyEffectUnitsProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets or sets a rectangular region on the visual to which the <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> is applied.  </summary>
	/// <returns>The rectangular region of the visual to which the effect is applied. The default value is <see cref="P:System.Windows.Rect.Empty" />.</returns>
	public Rect AreaToApplyEffect
	{
		get
		{
			return (Rect)GetValue(AreaToApplyEffectProperty);
		}
		set
		{
			SetValueInternal(AreaToApplyEffectProperty, value);
		}
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.BitmapEffectInput" /> class.</summary>
	public BitmapEffectInput()
	{
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.BitmapEffectInput" /> class using the <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> as the source for this input.</summary>
	/// <param name="input">The bitmap source to be used by this input object.</param>
	public BitmapEffectInput(BitmapSource input)
	{
		Input = input;
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Determines if <see cref="P:System.Windows.Media.Effects.BitmapEffectInput.Input" /> should be serialized.</summary>
	/// <returns>true if <see cref="P:System.Windows.Media.Effects.BitmapEffectInput.Input" /> should be serialized; otherwise false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeInput()
	{
		return Input != ContextInputSource;
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.BitmapEffectInput" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BitmapEffectInput Clone()
	{
		return (BitmapEffectInput)base.Clone();
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.BitmapEffectInput" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BitmapEffectInput CloneCurrentValue()
	{
		return (BitmapEffectInput)base.CloneCurrentValue();
	}

	private static void AreaToApplyEffectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((BitmapEffectInput)d).PropertyChanged(AreaToApplyEffectProperty);
	}

	protected override Freezable CreateInstanceCore()
	{
		return new BitmapEffectInput();
	}

	static BitmapEffectInput()
	{
		s_Input = ContextInputSource;
		s_AreaToApplyEffect = Rect.Empty;
		Type typeFromHandle = typeof(BitmapEffectInput);
		InputProperty = Animatable.RegisterProperty("Input", typeof(BitmapSource), typeFromHandle, ContextInputSource, null, null, isIndependentlyAnimated: false, null);
		AreaToApplyEffectUnitsProperty = Animatable.RegisterProperty("AreaToApplyEffectUnits", typeof(BrushMappingMode), typeFromHandle, BrushMappingMode.RelativeToBoundingBox, null, System.Windows.Media.ValidateEnums.IsBrushMappingModeValid, isIndependentlyAnimated: false, null);
		AreaToApplyEffectProperty = Animatable.RegisterProperty("AreaToApplyEffect", typeof(Rect), typeFromHandle, Rect.Empty, AreaToApplyEffectPropertyChanged, null, isIndependentlyAnimated: true, null);
	}
}
