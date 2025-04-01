namespace System.ComponentModel.Composition;

/// <summary>Specifies that this type’s exports won’t be included in a <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" />.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PartNotDiscoverableAttribute : Attribute
{
	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.PartNotDiscoverableAttribute" /> class.</summary>
	public PartNotDiscoverableAttribute()
	{
	}
}
