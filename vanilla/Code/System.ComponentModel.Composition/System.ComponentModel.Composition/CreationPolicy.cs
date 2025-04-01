namespace System.ComponentModel.Composition;

/// <summary>Specifies when and how a part will be instantiated.</summary>
public enum CreationPolicy
{
	/// <summary>Specifies that the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> will use the most appropriate <see cref="T:System.ComponentModel.Composition.CreationPolicy" /> for the part given the current context. This is the default <see cref="T:System.ComponentModel.Composition.CreationPolicy" />. By default, <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> will use <see cref="F:System.ComponentModel.Composition.CreationPolicy.Shared" />, unless the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> or importer requests <see cref="F:System.ComponentModel.Composition.CreationPolicy.NonShared" />.</summary>
	Any,
	/// <summary>Specifies that a single shared instance of the associated <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> will be created by the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> and shared by all requestors.</summary>
	Shared,
	/// <summary>Specifies that a new non-shared instance of the associated <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> will be created by the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> for every requestor.</summary>
	NonShared,
	NewScope
}
