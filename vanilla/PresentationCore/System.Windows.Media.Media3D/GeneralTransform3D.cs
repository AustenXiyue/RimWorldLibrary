using System.Windows.Media.Animation;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Media3D;

/// <summary>Provides generalized transformation support for 3-D objects. </summary>
[Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
public abstract class GeneralTransform3D : Animatable, IFormattable
{
	/// <summary>Gets the inverse transformation of this <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" />, if possible.</summary>
	/// <returns>An inverse of this instance, if possible; otherwise, null.</returns>
	public abstract GeneralTransform3D Inverse { get; }

	internal abstract Transform3D AffineTransform
	{
		[FriendAccessAllowed]
		get;
	}

	internal GeneralTransform3D()
	{
	}

	/// <summary>When overridden in a derived class, attempts to transform the specified 3-D point and returns a value that indicates whether the transformation was successful.</summary>
	/// <returns>true if <paramref name="inPoint" /> was transformed; otherwise, false.</returns>
	/// <param name="inPoint">The 3-D point to transform.</param>
	/// <param name="result">The result of transforming <paramref name="inPoint" />.</param>
	public abstract bool TryTransform(Point3D inPoint, out Point3D result);

	/// <summary>Transforms the specified 3-D point and returns the result.</summary>
	/// <returns>The result of transforming <paramref name="point" />.</returns>
	/// <param name="point">The 3-D point to transform.</param>
	/// <exception cref="T:System.InvalidOperationException">The transform did not succeed.</exception>
	public Point3D Transform(Point3D point)
	{
		if (!TryTransform(point, out var result))
		{
			throw new InvalidOperationException(SR.Format(SR.GeneralTransform_TransformFailed, null));
		}
		return result;
	}

	/// <summary>When overridden in a derived class, transforms the specified 3-D bounding box and returns an axis-aligned 3-D bounding box that is exactly large enough to contain it.</summary>
	/// <returns>The smallest axis-aligned 3-D bounding box possible that contains the transformed <paramref name="rect" />.</returns>
	/// <param name="rect">The 3-D bounding box to transform.</param>
	public abstract Rect3D TransformBounds(Rect3D rect);

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" />, making deep copies of this object's values. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new GeneralTransform3D Clone()
	{
		return (GeneralTransform3D)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.GeneralTransform3D" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property is true.</returns>
	public new GeneralTransform3D CloneCurrentValue()
	{
		return (GeneralTransform3D)base.CloneCurrentValue();
	}

	/// <summary>Creates a string representation of this instance.</summary>
	/// <returns>A string representation of this instance.</returns>
	public override string ToString()
	{
		ReadPreamble();
		return ConvertToString(null, null);
	}

	/// <summary>Creates a string representation of this instance, based on the passed <see cref="T:System.IFormatProvider" /> parameter.</summary>
	/// <returns>The value of the current instance in the specified format.</returns>
	/// <param name="provider">
	///   <see cref="T:System.IFormatProvider" /> instance used to process this instance.</param>
	public string ToString(IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(null, provider);
	}

	/// <summary>For a description of this member, see <see cref="M:System.IFormattable.ToString(System.String,System.IFormatProvider)" />.</summary>
	/// <returns>The value of the current instance in the specified format.</returns>
	/// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation.</param>
	/// <param name="provider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system.</param>
	string IFormattable.ToString(string format, IFormatProvider provider)
	{
		ReadPreamble();
		return ConvertToString(format, provider);
	}

	internal virtual string ConvertToString(string format, IFormatProvider provider)
	{
		return base.ToString();
	}
}
