using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Represents a node in a tree of scoped catalogs, reflecting an underlying catalog and its child scopes.</summary>
[DebuggerTypeProxy(typeof(CompositionScopeDefinitionDebuggerProxy))]
public class CompositionScopeDefinition : ComposablePartCatalog, INotifyComposablePartCatalogChanged
{
	private ComposablePartCatalog _catalog;

	private IEnumerable<ExportDefinition> _publicSurface;

	private IEnumerable<CompositionScopeDefinition> _children = Enumerable.Empty<CompositionScopeDefinition>();

	private volatile int _isDisposed;

	/// <summary>Gets the child scopes of this catalog.</summary>
	/// <returns>A collection of the child scopes of this catalog.</returns>
	public virtual IEnumerable<CompositionScopeDefinition> Children
	{
		get
		{
			ThrowIfDisposed();
			return _children;
		}
	}

	/// <summary>Gets a collection of parts visible to the parent scope of this catalog.</summary>
	/// <returns>A collection of parts visible to the parent scope of this catalog.</returns>
	public virtual IEnumerable<ExportDefinition> PublicSurface
	{
		get
		{
			ThrowIfDisposed();
			if (_publicSurface == null)
			{
				return this.SelectMany((ComposablePartDefinition p) => p.ExportDefinitions);
			}
			return _publicSurface;
		}
	}

	/// <summary>Occurs when the underlying catalog has changed, if that catalog supports notifications.</summary>
	public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed;

	/// <summary>Occurs when the underlying catalog is changing, if that catalog supports notifications.</summary>
	public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionScopeDefinition" /> class.</summary>
	protected CompositionScopeDefinition()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionScopeDefinition" /> class with the specified underlying catalog and children.</summary>
	/// <param name="catalog">The underlying catalog for this catalog.</param>
	/// <param name="children">A collection of the child scopes of this catalog.</param>
	public CompositionScopeDefinition(ComposablePartCatalog catalog, IEnumerable<CompositionScopeDefinition> children)
	{
		Requires.NotNull(catalog, "catalog");
		Requires.NullOrNotNullElements(children, "children");
		InitializeCompositionScopeDefinition(catalog, children, null);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionScopeDefinition" /> class with the specified underlying catalog, children, and public surface.</summary>
	/// <param name="catalog">The underlying catalog for this catalog.</param>
	/// <param name="children">A collection of the child scopes of this catalog.</param>
	/// <param name="publicSurface">The public surface for this catalog.</param>
	public CompositionScopeDefinition(ComposablePartCatalog catalog, IEnumerable<CompositionScopeDefinition> children, IEnumerable<ExportDefinition> publicSurface)
	{
		Requires.NotNull(catalog, "catalog");
		Requires.NullOrNotNullElements(children, "children");
		Requires.NullOrNotNullElements(publicSurface, "publicSurface");
		InitializeCompositionScopeDefinition(catalog, children, publicSurface);
	}

	private void InitializeCompositionScopeDefinition(ComposablePartCatalog catalog, IEnumerable<CompositionScopeDefinition> children, IEnumerable<ExportDefinition> publicSurface)
	{
		_catalog = catalog;
		if (children != null)
		{
			_children = children.ToArray();
		}
		if (publicSurface != null)
		{
			_publicSurface = publicSurface;
		}
		if (_catalog is INotifyComposablePartCatalogChanged notifyComposablePartCatalogChanged)
		{
			notifyComposablePartCatalogChanged.Changed += OnChangedInternal;
			notifyComposablePartCatalogChanged.Changing += OnChangingInternal;
		}
	}

	/// <summary>Called by the Dispose() and Finalize() methods to release the managed and unmanaged resources used by the current instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionScopeDefinition" /> class.</summary>
	/// <param name="disposing">true to release managed and unmanaged resources; false to release only unmanaged resources.</param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0 && _catalog is INotifyComposablePartCatalogChanged notifyComposablePartCatalogChanged)
			{
				notifyComposablePartCatalogChanged.Changed -= OnChangedInternal;
				notifyComposablePartCatalogChanged.Changing -= OnChangingInternal;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	/// <summary>Returns an enumerator that iterates through the catalog.</summary>
	/// <returns>An enumerator that can be used to iterate through the catalog.</returns>
	public override IEnumerator<ComposablePartDefinition> GetEnumerator()
	{
		return _catalog.GetEnumerator();
	}

	/// <summary>Gets a collection of exports that match the conditions specified by the import definition.</summary>
	/// <returns>A collection of exports that match the specified conditions.</returns>
	/// <param name="definition">Conditions that specify which exports to match.</param>
	public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
	{
		ThrowIfDisposed();
		return _catalog.GetExports(definition);
	}

	internal IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExportsFromPublicSurface(ImportDefinition definition)
	{
		Assumes.NotNull(definition, "definition");
		List<Tuple<ComposablePartDefinition, ExportDefinition>> list = new List<Tuple<ComposablePartDefinition, ExportDefinition>>();
		foreach (ExportDefinition item in PublicSurface)
		{
			if (!definition.IsConstraintSatisfiedBy(item))
			{
				continue;
			}
			foreach (Tuple<ComposablePartDefinition, ExportDefinition> export in GetExports(definition))
			{
				if (export.Item2 == item)
				{
					list.Add(export);
					break;
				}
			}
		}
		return list;
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.Composition.Hosting.CompositionScopeDefinition.Changed" /> event.</summary>
	/// <param name="e">Contains data for the <see cref="E:System.ComponentModel.Composition.Hosting.CompositionScopeDefinition.Changed" /> event.</param>
	protected virtual void OnChanged(ComposablePartCatalogChangeEventArgs e)
	{
		this.Changed?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.Composition.Hosting.CompositionScopeDefinition.Changing" /> event.</summary>
	/// <param name="e">Contains data for the <see cref="E:System.ComponentModel.Composition.Hosting.CompositionScopeDefinition.Changing" /> event.</param>
	protected virtual void OnChanging(ComposablePartCatalogChangeEventArgs e)
	{
		this.Changing?.Invoke(this, e);
	}

	private void OnChangedInternal(object sender, ComposablePartCatalogChangeEventArgs e)
	{
		OnChanged(e);
	}

	private void OnChangingInternal(object sender, ComposablePartCatalogChangeEventArgs e)
	{
		OnChanging(e);
	}

	[DebuggerStepThrough]
	private void ThrowIfDisposed()
	{
		if (_isDisposed == 1)
		{
			throw ExceptionBuilder.CreateObjectDisposed(this);
		}
	}
}
