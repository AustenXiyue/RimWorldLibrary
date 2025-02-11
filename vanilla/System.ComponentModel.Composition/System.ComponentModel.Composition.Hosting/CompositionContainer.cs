using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Threading;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Manages the composition of parts.</summary>
public class CompositionContainer : ExportProvider, ICompositionService, IDisposable
{
	private class CompositionServiceShim : ICompositionService
	{
		private CompositionContainer _innerContainer;

		public CompositionServiceShim(CompositionContainer innerContainer)
		{
			Assumes.NotNull(innerContainer);
			_innerContainer = innerContainer;
		}

		void ICompositionService.SatisfyImportsOnce(ComposablePart part)
		{
			_innerContainer.SatisfyImportsOnce(part);
		}
	}

	private CompositionOptions _compositionOptions;

	private ImportEngine _importEngine;

	private ComposablePartExportProvider _partExportProvider;

	private ExportProvider _rootProvider;

	private CatalogExportProvider _catalogExportProvider;

	private AggregateExportProvider _localExportProvider;

	private AggregateExportProvider _ancestorExportProvider;

	private readonly ReadOnlyCollection<ExportProvider> _providers;

	private volatile bool _isDisposed;

	private object _lock = new object();

	private static ReadOnlyCollection<ExportProvider> EmptyProviders = new ReadOnlyCollection<ExportProvider>(new ExportProvider[0]);

	internal CompositionOptions CompositionOptions
	{
		get
		{
			ThrowIfDisposed();
			return _compositionOptions;
		}
	}

	/// <summary>Gets the <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> that provides the container access to <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects.</summary>
	/// <returns>The catalog that provides the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> access to exports produced from <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> objects. The default is null.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	public ComposablePartCatalog Catalog
	{
		get
		{
			ThrowIfDisposed();
			if (_catalogExportProvider == null)
			{
				return null;
			}
			return _catalogExportProvider.Catalog;
		}
	}

	internal CatalogExportProvider CatalogExportProvider
	{
		get
		{
			ThrowIfDisposed();
			return _catalogExportProvider;
		}
	}

	/// <summary>Gets the export providers that provide the container access to additional <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePartCatalog" /> objects.</summary>
	/// <returns>A collection of <see cref="T:System.ComponentModel.Composition.Hosting.ExportProvider" /> objects that provide the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> access to additional <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects. The default is an empty <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> has been disposed of.</exception>
	public ReadOnlyCollection<ExportProvider> Providers
	{
		get
		{
			ThrowIfDisposed();
			return _providers;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> class.</summary>
	public CompositionContainer()
		: this(null, Array.Empty<ExportProvider>())
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> class with the specified export providers.</summary>
	/// <param name="providers">An array of <see cref="T:System.ComponentModel.Composition.Hosting.ExportProvider" /> objects that provide the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> access to <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects, or null to set <see cref="P:System.ComponentModel.Composition.Hosting.CompositionContainer.Providers" /> to an empty <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="providers" /> contains an element that is null.</exception>
	public CompositionContainer(params ExportProvider[] providers)
		: this(null, providers)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> class with the specified export providers and options.</summary>
	/// <param name="compositionOptions">An object that specifies the behavior of this container.</param>
	/// <param name="providers">An array of <see cref="T:System.ComponentModel.Composition.Hosting.ExportProvider" /> objects that provide the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> access to <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects, or null to set <see cref="P:System.ComponentModel.Composition.Hosting.CompositionContainer.Providers" /> to an empty <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="providers" /> contains an element that is null.</exception>
	public CompositionContainer(CompositionOptions compositionOptions, params ExportProvider[] providers)
		: this(null, compositionOptions, providers)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> class with the specified catalog and export providers.</summary>
	/// <param name="catalog">A catalog that provides <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects to the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.</param>
	/// <param name="providers">An array of <see cref="T:System.ComponentModel.Composition.Hosting.ExportProvider" /> objects that provide the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> access to <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects, or null to set <see cref="P:System.ComponentModel.Composition.Hosting.CompositionContainer.Providers" /> to an empty <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="providers" /> contains an element that is null.</exception>
	public CompositionContainer(ComposablePartCatalog catalog, params ExportProvider[] providers)
		: this(catalog, isThreadSafe: false, providers)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> class with the specified catalog, thread-safe mode, and export providers. </summary>
	/// <param name="catalog">A catalog that provides <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects to the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.</param>
	/// <param name="isThreadSafe">true if this <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object must be thread-safe; otherwise, false.</param>
	/// <param name="providers">An array of <see cref="T:System.ComponentModel.Composition.Hosting.ExportProvider" /> objects that provide the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> access to <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects, or null to set the <see cref="P:System.ComponentModel.Composition.Hosting.CompositionContainer.Providers" /> property to an empty <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.</param>
	/// <exception cref="T:System.ArgumentException">One or more elements of <paramref name="providers" /> are null.</exception>
	public CompositionContainer(ComposablePartCatalog catalog, bool isThreadSafe, params ExportProvider[] providers)
		: this(catalog, isThreadSafe ? CompositionOptions.IsThreadSafe : CompositionOptions.Default, providers)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> class with the specified catalog, options, and export providers.</summary>
	/// <param name="catalog">A catalog that provides <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects to the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />.</param>
	/// <param name="compositionOptions">An object that specifies options that affect the behavior of the container.</param>
	/// <param name="providers">An array of <see cref="T:System.ComponentModel.Composition.Hosting.ExportProvider" /> objects that provide the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> access to <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects, or null to set <see cref="P:System.ComponentModel.Composition.Hosting.CompositionContainer.Providers" /> to an empty <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="providers" /> contains an element that is null.</exception>
	public CompositionContainer(ComposablePartCatalog catalog, CompositionOptions compositionOptions, params ExportProvider[] providers)
	{
		if (compositionOptions > (CompositionOptions.DisableSilentRejection | CompositionOptions.IsThreadSafe | CompositionOptions.ExportCompositionService))
		{
			throw new ArgumentOutOfRangeException("compositionOptions");
		}
		_compositionOptions = compositionOptions;
		_partExportProvider = new ComposablePartExportProvider(compositionOptions);
		_partExportProvider.SourceProvider = this;
		if (catalog != null || providers.Length != 0)
		{
			if (catalog != null)
			{
				_catalogExportProvider = new CatalogExportProvider(catalog, compositionOptions);
				_catalogExportProvider.SourceProvider = this;
				_localExportProvider = new AggregateExportProvider(_partExportProvider, _catalogExportProvider);
			}
			else
			{
				_localExportProvider = new AggregateExportProvider(_partExportProvider);
			}
			if (providers != null && providers.Length != 0)
			{
				_ancestorExportProvider = new AggregateExportProvider(providers);
				_rootProvider = new AggregateExportProvider(_localExportProvider, _ancestorExportProvider);
			}
			else
			{
				_rootProvider = _localExportProvider;
			}
		}
		else
		{
			_rootProvider = _partExportProvider;
		}
		if (compositionOptions.HasFlag(CompositionOptions.ExportCompositionService))
		{
			this.ComposeExportedValue((ICompositionService)new CompositionServiceShim(this));
		}
		_rootProvider.ExportsChanged += OnExportsChangedInternal;
		_rootProvider.ExportsChanging += OnExportsChangingInternal;
		_providers = ((providers != null) ? new ReadOnlyCollection<ExportProvider>((ExportProvider[])providers.Clone()) : EmptyProviders);
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> class.</summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposing || _isDisposed)
		{
			return;
		}
		ExportProvider exportProvider = null;
		AggregateExportProvider aggregateExportProvider = null;
		AggregateExportProvider aggregateExportProvider2 = null;
		ComposablePartExportProvider composablePartExportProvider = null;
		CatalogExportProvider catalogExportProvider = null;
		ImportEngine importEngine = null;
		lock (_lock)
		{
			if (!_isDisposed)
			{
				exportProvider = _rootProvider;
				_rootProvider = null;
				aggregateExportProvider2 = _localExportProvider;
				_localExportProvider = null;
				aggregateExportProvider = _ancestorExportProvider;
				_ancestorExportProvider = null;
				composablePartExportProvider = _partExportProvider;
				_partExportProvider = null;
				catalogExportProvider = _catalogExportProvider;
				_catalogExportProvider = null;
				importEngine = _importEngine;
				_importEngine = null;
				_isDisposed = true;
			}
		}
		if (exportProvider != null)
		{
			exportProvider.ExportsChanged -= OnExportsChangedInternal;
			exportProvider.ExportsChanging -= OnExportsChangingInternal;
		}
		aggregateExportProvider?.Dispose();
		aggregateExportProvider2?.Dispose();
		catalogExportProvider?.Dispose();
		composablePartExportProvider?.Dispose();
		importEngine?.Dispose();
	}

	/// <summary>Adds or removes the parts in the specified <see cref="T:System.ComponentModel.Composition.Hosting.CompositionBatch" /> from the container and executes composition.</summary>
	/// <param name="batch">Changes to the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> to include during the composition.</param>
	public void Compose(CompositionBatch batch)
	{
		Requires.NotNull(batch, "batch");
		ThrowIfDisposed();
		_partExportProvider.Compose(batch);
	}

	/// <summary>Releases the specified <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> object from the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />. </summary>
	/// <param name="export">The <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> that needs to be released.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="export" /> is null.</exception>
	public void ReleaseExport(Export export)
	{
		Requires.NotNull(export, "export");
		if (export is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}

	/// <summary>Removes the specified export from composition and releases its resources if possible.</summary>
	/// <param name="export">An indirect reference to the export to remove.</param>
	/// <typeparam name="T">The type of the export.</typeparam>
	public void ReleaseExport<T>(Lazy<T> export)
	{
		Requires.NotNull(export, "export");
		if (export is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}

	/// <summary>Releases a set of <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects from the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" />. </summary>
	/// <param name="exports">A collection of <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects to be released.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="exports" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="exports" /> contains an element that is null.</exception>
	public void ReleaseExports(IEnumerable<Export> exports)
	{
		Requires.NotNullOrNullElements(exports, "exports");
		foreach (Export export in exports)
		{
			ReleaseExport(export);
		}
	}

	/// <summary>Removes a collection of exports from composition and releases their resources if possible.</summary>
	/// <param name="exports">A collection of indirect references to the exports to be removed.</param>
	/// <typeparam name="T">The type of the exports.</typeparam>
	public void ReleaseExports<T>(IEnumerable<Lazy<T>> exports)
	{
		Requires.NotNullOrNullElements(exports, "exports");
		foreach (Lazy<T> export in exports)
		{
			ReleaseExport(export);
		}
	}

	/// <summary>Removes a collection of exports from composition and releases their resources if possible.</summary>
	/// <param name="exports">A collection of indirect references to the exports to be removed and their metadata.</param>
	/// <typeparam name="T">The type of the exports.</typeparam>
	/// <typeparam name="TMetadataView">The type of the exports' metadata view.</typeparam>
	public void ReleaseExports<T, TMetadataView>(IEnumerable<Lazy<T, TMetadataView>> exports)
	{
		Requires.NotNullOrNullElements(exports, "exports");
		foreach (Lazy<T, TMetadataView> export in exports)
		{
			ReleaseExport(export);
		}
	}

	/// <summary>Satisfies the imports of the specified <see cref="T:System.ComponentModel.Composition.Primitives.ComposablePart" /> object without registering it for recomposition.</summary>
	/// <param name="part">The part to satisfy the imports of.</param>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object has been disposed of.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="part" /> is null.</exception>
	/// <exception cref="T:System.ComponentModel.Composition.CompositionException">An error occurred during composition. <see cref="P:System.ComponentModel.Composition.CompositionException.Errors" /> will contain a collection of the errors that occurred.</exception>
	public void SatisfyImportsOnce(ComposablePart part)
	{
		ThrowIfDisposed();
		if (_importEngine == null)
		{
			ImportEngine importEngine = new ImportEngine(this, _compositionOptions);
			lock (_lock)
			{
				if (_importEngine == null)
				{
					Thread.MemoryBarrier();
					_importEngine = importEngine;
					importEngine = null;
				}
			}
			importEngine?.Dispose();
		}
		_importEngine.SatisfyImportsOnce(part);
	}

	internal void OnExportsChangedInternal(object sender, ExportsChangeEventArgs e)
	{
		OnExportsChanged(e);
	}

	internal void OnExportsChangingInternal(object sender, ExportsChangeEventArgs e)
	{
		OnExportsChanging(e);
	}

	/// <summary>Returns a collection of all exports that match the conditions in the specified <see cref="T:System.ComponentModel.Composition.Primitives.ImportDefinition" /> object.</summary>
	/// <returns>A collection of all the <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects in this <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> object that match the conditions specified by <paramref name="definition" />.</returns>
	/// <param name="definition">The object that defines the conditions of the <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects to get.</param>
	/// <param name="atomicComposition">The composition transaction to use, or null to disable transactional composition.</param>
	protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
	{
		ThrowIfDisposed();
		IEnumerable<Export> exports = null;
		if (!definition.Metadata.TryGetValue("System.ComponentModel.Composition.ImportSource", out var value))
		{
			value = ImportSource.Any;
		}
		switch ((ImportSource)value)
		{
		case ImportSource.Any:
			Assumes.NotNull(_rootProvider);
			_rootProvider.TryGetExports(definition, atomicComposition, out exports);
			break;
		case ImportSource.Local:
			Assumes.NotNull(_localExportProvider);
			_localExportProvider.TryGetExports(definition.RemoveImportSource(), atomicComposition, out exports);
			break;
		case ImportSource.NonLocal:
			if (_ancestorExportProvider != null)
			{
				_ancestorExportProvider.TryGetExports(definition.RemoveImportSource(), atomicComposition, out exports);
			}
			break;
		}
		return exports;
	}

	[DebuggerStepThrough]
	private void ThrowIfDisposed()
	{
		if (_isDisposed)
		{
			throw ExceptionBuilder.CreateObjectDisposed(this);
		}
	}
}
