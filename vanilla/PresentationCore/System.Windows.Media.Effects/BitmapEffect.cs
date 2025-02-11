using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Effects;

/// <summary>Note: This API is now obsolete. The non-obsolete alternative is <see cref="T:System.Windows.Media.Effects.Effect" />. Defines a bitmap effect. Derived classes define effects that can be applied to a <see cref="T:System.Windows.Media.Visual" /> object, such as a <see cref="T:System.Windows.Controls.Button" /> or an <see cref="T:System.Windows.Controls.Image" />.</summary>
public abstract class BitmapEffect : Animatable
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> class.</summary>
	protected BitmapEffect()
	{
		if (Thread.CurrentThread.GetApartmentState() != 0)
		{
			throw new InvalidOperationException(SR.RequiresSTA);
		}
	}

	/// <summary>When overridden in a derived class, updates the property states of the unmanaged properties of the effect.</summary>
	/// <param name="unmanagedEffect">The handle to the effect that contains the properties to update.</param>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	protected abstract void UpdateUnmanagedPropertyState(SafeHandle unmanagedEffect);

	/// <summary>When overridden in a derived class, creates a clone of the unmanaged effect.</summary>
	/// <returns>A handle to the unmanaged effect clone.</returns>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	protected abstract SafeHandle CreateUnmanagedEffect();

	/// <summary>Sets the specified property to the given value.</summary>
	/// <param name="effect">The handle to the effect that contains the property to change.</param>
	/// <param name="propertyName">The name of the property to change.</param>
	/// <param name="value">The value to use to set the property.</param>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	protected static void SetValue(SafeHandle effect, string propertyName, object value)
	{
	}

	/// <summary>Creates a handle to an IMILBitmapEffect object that is used to initialize a custom effect.</summary>
	/// <returns>A handle to an IMILBitmapEffect object.</returns>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	protected static SafeHandle CreateBitmapEffectOuter()
	{
		return null;
	}

	/// <summary>Initializes an IMILBitmapEffect handle obtained from <see cref="M:System.Windows.Media.Effects.BitmapEffect.CreateBitmapEffectOuter" /> with the given IMILBitmapEffectPrimitive.</summary>
	/// <param name="outerObject">The outer IMILBitmapEffect wrapper to initialize. </param>
	/// <param name="innerObject">The inner IMILBitmapEffectPrimitive.</param>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	protected static void InitializeBitmapEffect(SafeHandle outerObject, SafeHandle innerObject)
	{
	}

	/// <summary>Returns the <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> that results when the effect is applied to the specified <see cref="T:System.Windows.Media.Effects.BitmapEffectInput" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Imaging.BitmapSource" /> with the effect applied to the input.</returns>
	/// <param name="input">The input to apply the effect to.</param>
	[Obsolete("BitmapEffects are deprecated and no longer function.  Consider using Effects where appropriate instead.")]
	public BitmapSource GetOutput(BitmapEffectInput input)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (input.Input == null)
		{
			throw new ArgumentException(SR.Effect_No_InputSource, "input");
		}
		if (input.Input == BitmapEffectInput.ContextInputSource)
		{
			throw new InvalidOperationException(SR.Format(SR.Effect_No_ContextInputSource, null));
		}
		return input.Input.Clone();
	}

	internal virtual bool CanBeEmulatedUsingEffectPipeline()
	{
		return false;
	}

	internal virtual Effect GetEmulatingEffect()
	{
		throw new NotImplementedException();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.BitmapEffect" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BitmapEffect Clone()
	{
		return (BitmapEffect)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Effects.BitmapEffect" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new BitmapEffect CloneCurrentValue()
	{
		return (BitmapEffect)base.CloneCurrentValue();
	}
}
