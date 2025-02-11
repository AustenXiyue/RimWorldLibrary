namespace System.ComponentModel.Composition.Hosting;

/// <summary>Provides notifications when a <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> changes.</summary>
public interface INotifyComposablePartCatalogChanged
{
	/// <summary>Occurs when a <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> has changed.</summary>
	event EventHandler<ComposablePartCatalogChangeEventArgs> Changed;

	/// <summary>Occurs when a <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> is changing.</summary>
	event EventHandler<ComposablePartCatalogChangeEventArgs> Changing;
}
