namespace System.Windows.Media;

/// <summary>Defines a base class for all metadata operations on imaging related APIs. This is an abstract class.</summary>
public abstract class ImageMetadata : Freezable
{
	internal ImageMetadata()
	{
	}

	/// <summary>Creates a modifiable clone of this <see cref="T:System.Windows.Media.ImageMetadata" />, making deep copies of this object's values. When copying dependency properties, this method copies resource references and data bindings (but they might no longer resolve) but not animations or their current values.</summary>
	/// <returns>A modifiable clone of the current object. The cloned object's <see cref="P:System.Windows.Freezable.IsFrozen" /> property will be false even if the source's <see cref="P:System.Windows.Freezable.IsFrozen" /> property was true.</returns>
	public new ImageMetadata Clone()
	{
		return (ImageMetadata)base.Clone();
	}
}
