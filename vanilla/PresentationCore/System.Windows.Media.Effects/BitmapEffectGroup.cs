using System.Runtime.InteropServices;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using MS.Internal;

namespace System.Windows.Media.Effects;

/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Represents a group of <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> objects that is used to apply multiple effects to a visible object.</summary>
[ContentProperty("Children")]
public sealed class BitmapEffectGroup : BitmapEffect
{
	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Identifies the <see cref="P:System.Windows.Media.Effects.BitmapEffectGroup.Children" /> dependency property.</summary>
	public static readonly DependencyProperty ChildrenProperty;

	internal static BitmapEffectCollection s_Children;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Gets or sets the children of the <see cref="T:System.Windows.Media.Effects.BitmapEffectGroup" />.  </summary>
	/// <returns>The children of the effects group as a <see cref="T:System.Windows.Media.Effects.BitmapEffectCollection" />.</returns>
	public BitmapEffectCollection Children
	{
		get
		{
			return (BitmapEffectCollection)GetValue(ChildrenProperty);
		}
		set
		{
			SetValueInternal(ChildrenProperty, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 1;

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.BitmapEffectGroup" /> class.</summary>
	public BitmapEffectGroup()
	{
	}

	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	protected override void UpdateUnmanagedPropertyState(SafeHandle unmanagedEffect)
	{
	}

	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	protected override SafeHandle CreateUnmanagedEffect()
	{
		return null;
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.BitmapEffectGroup" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BitmapEffectGroup Clone()
	{
		return (BitmapEffectGroup)base.Clone();
	}

	/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.BitmapEffectGroup" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BitmapEffectGroup CloneCurrentValue()
	{
		return (BitmapEffectGroup)base.CloneCurrentValue();
	}

	protected override Freezable CreateInstanceCore()
	{
		return new BitmapEffectGroup();
	}

	static BitmapEffectGroup()
	{
		s_Children = BitmapEffectCollection.Empty;
		Type typeFromHandle = typeof(BitmapEffectGroup);
		ChildrenProperty = Animatable.RegisterProperty("Children", typeof(BitmapEffectCollection), typeFromHandle, new FreezableDefaultValueFactory(BitmapEffectCollection.Empty), null, null, isIndependentlyAnimated: false, null);
	}
}
