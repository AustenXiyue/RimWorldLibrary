using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Internal.Collections;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Retrieves exports provided by a collection of <see cref="T:System.ComponentModel.Composition.Hosting.ExportProvider" /> objects.</summary>
public class AggregateExportProvider : ExportProvider, IDisposable
{
	private readonly ReadOnlyCollection<ExportProvider> _readOnlyProviders;

	private readonly ExportProvider[] _providers;

	private volatile int _isDisposed;

	/// <summary>Gets a collection that contains the providers that the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateExportProvider" /> object aggregates.</summary>
	/// <returns>A collection of the <see cref="T:System.ComponentModel.Composition.Hosting.ExportProvider" /> objects that the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateExportProvider" /> aggregates.</returns>
	/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.ComponentModel.Composition.Hosting.AggregateExportProvider" /> object has been disposed of.</exception>
	public ReadOnlyCollection<ExportProvider> Providers
	{
		get
		{
			ThrowIfDisposed();
			return _readOnlyProviders;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateExportProvider" /> class.</summary>
	/// <param name="providers">The prioritized list of export providers.</param>
	public AggregateExportProvider(params ExportProvider[] providers)
	{
		ExportProvider[] array = null;
		if (providers != null)
		{
			array = new ExportProvider[providers.Length];
			for (int i = 0; i < providers.Length; i++)
			{
				ExportProvider exportProvider = providers[i];
				if (exportProvider == null)
				{
					throw ExceptionBuilder.CreateContainsNullElement("providers");
				}
				array[i] = exportProvider;
				exportProvider.ExportsChanged += OnExportChangedInternal;
				exportProvider.ExportsChanging += OnExportChangingInternal;
			}
		}
		else
		{
			array = new ExportProvider[0];
		}
		_providers = array;
		_readOnlyProviders = new ReadOnlyCollection<ExportProvider>(_providers);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateExportProvider" /> class.</summary>
	/// <param name="providers">The prioritized list of export providers. The providers are consulted in the order in which they are supplied.</param>
	/// <exception cref="T:System.ArgumentException">One or more elements of <paramref name="providers" /> are null.</exception>
	public AggregateExportProvider(IEnumerable<ExportProvider> providers)
		: this(providers?.AsArray())
	{
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateExportProvider" /> class. </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>Releases the unmanaged resources used by the <see cref="T:System.ComponentModel.Composition.Hosting.AggregateExportProvider" /> class and optionally releases the managed resources. </summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
	protected virtual void Dispose(bool disposing)
	{
		if (disposing && Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
		{
			ExportProvider[] providers = _providers;
			foreach (ExportProvider obj in providers)
			{
				obj.ExportsChanged -= OnExportChangedInternal;
				obj.ExportsChanging -= OnExportChangingInternal;
			}
		}
	}

	/// <summary>Gets all the exports that match the conditions of the specified import.</summary>
	/// <returns>A collection that contains all the exports that match the specified condition.</returns>
	/// <param name="definition">The conditions of the <see cref="T:System.ComponentModel.Composition.Primitives.Export" /> objects to be returned.</param>
	/// <param name="atomicComposition">The transactional container for the composition.</param>
	protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
	{
		ThrowIfDisposed();
		ExportProvider[] providers;
		if (definition.Cardinality == ImportCardinality.ZeroOrMore)
		{
			List<Export> list = new List<Export>();
			providers = _providers;
			for (int i = 0; i < providers.Length; i++)
			{
				foreach (Export export in providers[i].GetExports(definition, atomicComposition))
				{
					list.Add(export);
				}
			}
			return list;
		}
		IEnumerable<Export> enumerable = null;
		providers = _providers;
		for (int i = 0; i < providers.Length; i++)
		{
			IEnumerable<Export> exports;
			bool num = providers[i].TryGetExports(definition, atomicComposition, out exports);
			bool flag = exports.FastAny();
			if (num && flag)
			{
				return exports;
			}
			if (flag)
			{
				enumerable = ((enumerable != null) ? enumerable.Concat(exports) : exports);
			}
		}
		return enumerable;
	}

	private void OnExportChangedInternal(object sender, ExportsChangeEventArgs e)
	{
		OnExportsChanged(e);
	}

	private void OnExportChangingInternal(object sender, ExportsChangeEventArgs e)
	{
		OnExportsChanging(e);
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
