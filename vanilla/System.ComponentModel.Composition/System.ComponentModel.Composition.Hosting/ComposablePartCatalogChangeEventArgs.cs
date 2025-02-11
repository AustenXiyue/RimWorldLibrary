using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Provides data for the <see cref="E:System.ComponentModel.Composition.Hosting.INotifyComposablePartCatalogChanged.Changed" /> event.</summary>
public class ComposablePartCatalogChangeEventArgs : EventArgs
{
	private readonly IEnumerable<ComposablePartDefinition> _addedDefinitions;

	private readonly IEnumerable<ComposablePartDefinition> _removedDefinitions;

	/// <summary>Gets a collection of definitions added to the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> in this change.</summary>
	/// <returns>A collection of definitions added to the catalog.</returns>
	public IEnumerable<ComposablePartDefinition> AddedDefinitions => _addedDefinitions;

	/// <summary>Gets a collection of definitions removed from the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> in this change.</summary>
	/// <returns>A collection of definitions removed from the catalog in this change.</returns>
	public IEnumerable<ComposablePartDefinition> RemovedDefinitions => _removedDefinitions;

	/// <summary>Gets the composition transaction for this change.</summary>
	/// <returns>The composition transaction for this change.</returns>
	public AtomicComposition AtomicComposition { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.ComposablePartCatalogChangeEventArgs" /> class with the specified changes.</summary>
	/// <param name="addedDefinitions">The part definitions that were added to the catalog.</param>
	/// <param name="removedDefinitions">The part definitions that were removed from the catalog.</param>
	/// <param name="atomicComposition">The composition transaction to use, or null to disable transactional composition.</param>
	public ComposablePartCatalogChangeEventArgs(IEnumerable<ComposablePartDefinition> addedDefinitions, IEnumerable<ComposablePartDefinition> removedDefinitions, AtomicComposition atomicComposition)
	{
		Requires.NotNull(addedDefinitions, "addedDefinitions");
		Requires.NotNull(removedDefinitions, "removedDefinitions");
		_addedDefinitions = addedDefinitions.AsArray();
		_removedDefinitions = removedDefinitions.AsArray();
		AtomicComposition = atomicComposition;
	}
}
