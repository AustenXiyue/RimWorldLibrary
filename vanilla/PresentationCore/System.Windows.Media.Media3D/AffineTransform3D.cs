namespace System.Windows.Media.Media3D;

/// <summary> Base class from which all concrete affine 3-D transforms—translations, rotations, and scale transformations—derive.</summary>
public abstract class AffineTransform3D : Transform3D
{
	/// <summary>Gets a value that indicates whether the transformation is affine. </summary>
	/// <returns>True if the transformation is affine, false otherwise.</returns>
	public override bool IsAffine
	{
		get
		{
			ReadPreamble();
			return true;
		}
	}

	internal AffineTransform3D()
	{
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.AffineTransform3D" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new AffineTransform3D Clone()
	{
		return (AffineTransform3D)base.Clone();
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.Media3D.AffineTransform3D" /> object, making deep copies of this object's current values. Resource references, data bindings, and animations are not copied, but their current values are. </summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new AffineTransform3D CloneCurrentValue()
	{
		return (AffineTransform3D)base.CloneCurrentValue();
	}
}
