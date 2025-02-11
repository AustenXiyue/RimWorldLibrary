using System.Windows.Media.Animation;
using MS.Internal.PresentationCore;

namespace System.Windows.Media;

/// <summary>Provides generalized transformation support for objects, such as points and rectangles. This is an abstract class. </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public abstract class GeneralTransform : Animatable, IFormattable
{
	/// <summary>Gets the inverse transformation of this <see cref="T:System.Windows.Media.GeneralTransform" />, if possible.</summary>
	/// <returns>An inverse of this instance, if possible; otherwise null.</returns>
	public abstract GeneralTransform Inverse { get; }

	internal virtual Transform AffineTransform
	{
		[FriendAccessAllowed]
		get
		{
			return null;
		}
	}

	/// <summary>When overridden in a derived class, attempts to transform the specified point and returns a value that indicates whether the transformation was successful.</summary>
	/// <returns>true if <paramref name="inPoint" /> was transformed; otherwise, false.</returns>
	/// <param name="inPoint">The point to transform.</param>
	/// <param name="result">The result of transforming <paramref name="inPoint" />.</param>
	public abstract bool TryTransform(Point inPoint, out Point result);

	/// <summary>Transforms the specified point and returns the result.</summary>
	/// <returns>The result of transforming <paramref name="point" />.</returns>
	/// <param name="point">The point to transform.</param>
	/// <exception cref="T:System.InvalidOperationException">The transform did not succeed.</exception>
	public Point Transform(Point point)
	{
		if (!TryTransform(point, out var result))
		{
			throw new InvalidOperationException(SR.Format(SR.GeneralTransform_TransformFailed, null));
		}
		return result;
	}

	/// <summary>When overridden in a derived class, transforms the specified bounding box and returns an axis-aligned bounding box that is exactly large enough to contain it.</summary>
	/// <returns>The smallest axis-aligned bounding box possible that contains the transformed <paramref name="rect" />.</returns>
	/// <param name="rect">The bounding box to transform.</param>
	public abstract Rect TransformBounds(Rect rect);

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GeneralTransform" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeneralTransform Clone()
	{
		return (GeneralTransform)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.GeneralTransform" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new GeneralTransform CloneCurrentValue()
	{
		return (GeneralTransform)base.CloneCurrentValue();
	}

	/// <summary>Creates a string representation of this <see cref="T:System.Windows.Media.GeneralTransform" />.</summary>
	/// <returns>A string representation of this instance.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary>Creates a string representation of this instance, based on the passed <see cref="T:System.IFormatProvider" /> parameter.</summary>
	/// <returns>A string representation of this instance, based on <paramref name="provider" />.</returns>
	/// <param name="provider">
	///   <see cref="T:System.IFormatProvider" /> instance used to process this instance.</param>
	public string ToString(IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(null, provider);
	}

	/// <summary>Formats the value of the current instance using the specified format.</summary>
	/// <returns>The value of the current instance in the specified format.</returns>
	/// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
	/// <param name="provider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(format, provider);
	}

	internal virtual string ConvertToString(string format, IFormatProvider provider)
	{
		return base.ToString();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.GeneralTransform" /> class. </summary>
	protected GeneralTransform()
	{
	}
}
