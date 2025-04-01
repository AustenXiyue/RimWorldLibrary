using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>A catalog that combines the elements of <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> objects. </summary>
public class AggregateCatalog : ComposablePartCatalog, INotifyComposablePartCatalogChanged
{
	private ComposablePartCatalogCollection _catalogs;

	private volatile int _isDisposed;

	/// <summary>Gets the underlying catalogs of the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateCatalog" /> object.</summary>
	/// <returns>A collection of <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> objects that underlie the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateCatalog" /> object.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.AggregateCatalog" /> object has been disposed of.</exception>
	public ICollection<ComposablePartCatalog> Catalogs
	{
		get
		{
			ThrowIfDisposed();
			return _catalogs;
		}
	}

	/// <summary>Occurs when the contents of the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateCatalog" /> object have changed.</summary>
	public event EventHandler<ComposablePartCatalogChangeEventArgs> Changed
	{
		add
		{
			_catalogs.Changed += value;
		}
		remove
		{
			_catalogs.Changed -= value;
		}
	}

	/// <summary>Occurs when the contents of the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateCatalog" /> object are changing.</summary>
	public event EventHandler<ComposablePartCatalogChangeEventArgs> Changing
	{
		add
		{
			_catalogs.Changing += value;
		}
		remove
		{
			_catalogs.Changing -= value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateCatalog" /> class.</summary>
	public AggregateCatalog()
		: this((IEnumerable<ComposablePartCatalog>)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateCatalog" /> class with the specified catalogs.</summary>
	/// <param name="catalogs">A array of <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateCatalog" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="catalogs" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="catalogs" /> contains an element that is null.</exception>
	public AggregateCatalog(params ComposablePartCatalog[] catalogs)
		: this((IEnumerable<ComposablePartCatalog>)catalogs)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateCatalog" /> class with the specified catalogs.</summary>
	/// <param name="catalogs">A collection of <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> objects to add to the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateCatalog" /> or null to create an empty <see cref="T:System.ComponentModel.Composition.Hosting.AggregateCatalog" />. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="catalogs" /> contains an element that is null.</exception>
	public AggregateCatalog(IEnumerable<ComposablePartCatalog> catalogs)
	{
		Requires.NullOrNotNullElements(catalogs, "catalogs");
		_catalogs = new ComposablePartCatalogCollection(catalogs, OnChanged, OnChanging);
	}

	/// <summary>Gets the export definitions that match the constraint expressed by the specified definition.</summary>
	/// <returns>A collection of <see cref="T:System.Tuple`2" /> containing the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> objects and their associated <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects for objects that match the constraint specified by <paramref name="definition" />.</returns>
	/// <param name="definition">The conditions of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> objects to be returned.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.AggregateCatalog" /> object has been disposed of.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="definition" /> is null.</exception>
	public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
	{
		ThrowIfDisposed();
		Requires.NotNull(definition, "definition");
		List<Tuple<ComposablePartDefinition, ExportDefinition>> list = new List<Tuple<ComposablePartDefinition, ExportDefinition>>();
		foreach (ComposablePartCatalog catalog in _catalogs)
		{
			foreach (Tuple<ComposablePartDefinition, ExportDefinition> export in catalog.GetExports(definition))
			{
				list.Add(export);
			}
		}
		return list;
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateCatalog" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
			{
				_catalogs.Dispose();
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
		return _catalogs.SelectMany((ComposablePartCatalog catalog) => catalog).GetEnumerator();
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.Composition.Hosting.AggregateCatalog.Changed" /> event.</summary>
	/// <param name="e">A <see cref="T:System.ComponentModel.Composition.Hosting.ComposablePartCatalogChangeEventArgs" /> object that contains the event data. </param>
	protected virtual void OnChanged(ComposablePartCatalogChangeEventArgs e)
	{
		_catalogs.OnChanged(this, e);
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.Composition.Hosting.AggregateCatalog.Changing" /> event.</summary>
	/// <param name="e">A <see cref="T:System.ComponentModel.Composition.Hosting.ComposablePartCatalogChangeEventArgs" /> object that contains the event data. </param>
	protected virtual void OnChanging(ComposablePartCatalogChangeEventArgs e)
	{
		_catalogs.OnChanging(this, e);
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
