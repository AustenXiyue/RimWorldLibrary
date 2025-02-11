using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Internal;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition.Primitives;

/// <summary>Represents the abstract base class for composable part catalogs, which collect and return <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects.</summary>
[DebuggerTypeProxy(typeof(ComposablePartCatalogDebuggerProxy))]
public abstract class ComposablePartCatalog : IEnumerable<ComposablePartDefinition>, IEnumerable, IDisposable
{
	private bool _isDisposed;

	private volatile IQueryable<ComposablePartDefinition> _queryableParts;

	private static readonly List<Tuple<ComposablePartDefinition, ExportDefinition>> _EmptyExportsList = new List<Tuple<ComposablePartDefinition, ExportDefinition>>();

	/// <summary>Gets the part definitions that are contained in the catalog.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> contained in the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" />.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> object has been disposed of.</exception>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public virtual IQueryable<ComposablePartDefinition> Parts
	{
		get
		{
			ThrowIfDisposed();
			if (_queryableParts == null)
			{
				IQueryable<ComposablePartDefinition> value = this.AsQueryable();
				Interlocked.CompareExchange(ref _queryableParts, value, null);
				Assumes.NotNull(_queryableParts);
			}
			return _queryableParts;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> class.</summary>
	protected ComposablePartCatalog()
	{
	}

	/// <summary>Gets a list of export definitions that match the constraint defined by the specified <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> object.</summary>
	/// <returns>A collection of <see cref="T:System.Tuple`2" /> containing the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> objects and their associated <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartDefinition" /> objects for objects that match the constraint specified by <paramref name="definition" />.</returns>
	/// <param name="definition">The conditions of the <see cref="T:System.ComponentModel.Composition.Primitives.ExportDefinition" /> objects to be returned.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> object has been disposed of.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="definition" /> is null.</exception>
	public virtual IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
	{
		ThrowIfDisposed();
		Requires.NotNull(definition, "definition");
		List<Tuple<ComposablePartDefinition, ExportDefinition>> list = null;
		IEnumerable<ComposablePartDefinition> candidateParts = GetCandidateParts(definition);
		if (candidateParts != null)
		{
			foreach (ComposablePartDefinition item in candidateParts)
			{
				IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> exports = item.GetExports(definition);
				if (exports != ComposablePartDefinition._EmptyExports)
				{
					list = list.FastAppendToListAllowNulls(exports);
				}
			}
		}
		return list ?? _EmptyExportsList;
	}

	internal virtual IEnumerable<ComposablePartDefinition> GetCandidateParts(ImportDefinition definition)
	{
		return this;
	}

	/// <summary>Releases all resources used by the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" />.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		_isDisposed = true;
	}

	[DebuggerStepThrough]
	private void ThrowIfDisposed()
	{
		if (_isDisposed)
		{
			throw ExceptionBuilder.CreateObjectDisposed(this);
		}
	}

	/// <summary>Returns an enumerator that iterates through the catalog.</summary>
	/// <returns>An enumerator that can be used to iterate through the catalog.</returns>
	public virtual IEnumerator<ComposablePartDefinition> GetEnumerator()
	{
		IQueryable<ComposablePartDefinition> parts = Parts;
		if (parts == _queryableParts)
		{
			return Enumerable.Empty<ComposablePartDefinition>().GetEnumerator();
		}
		return parts.GetEnumerator();
	}

	/// <summary>Returns an enumerator that iterates through the catalog.</summary>
	/// <returns>An enumerator that can be used to iterate through the catalog.</returns>
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
