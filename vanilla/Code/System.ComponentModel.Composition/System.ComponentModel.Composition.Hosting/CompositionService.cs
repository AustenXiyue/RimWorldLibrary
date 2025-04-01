using System.ComponentModel.Composition.Primitives;
using Microsoft.Internal;
using Unity;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Provides methods to satisfy imports on an existing part instance.</summary>
public class CompositionService : ICompositionService, IDisposable
{
	private CompositionContainer _compositionContainer;

	private INotifyComposablePartCatalogChanged _notifyCatalog;

	internal CompositionService(ComposablePartCatalog composablePartCatalog)
	{
		Assumes.NotNull(composablePartCatalog);
		_notifyCatalog = composablePartCatalog as INotifyComposablePartCatalogChanged;
		try
		{
			if (_notifyCatalog != null)
			{
				_notifyCatalog.Changing += OnCatalogChanging;
			}
			CompositionOptions compositionOptions = CompositionOptions.DisableSilentRejection | CompositionOptions.IsThreadSafe | CompositionOptions.ExportCompositionService;
			CompositionContainer compositionContainer = new CompositionContainer(composablePartCatalog, compositionOptions);
			_compositionContainer = compositionContainer;
		}
		catch
		{
			if (_notifyCatalog != null)
			{
				_notifyCatalog.Changing -= OnCatalogChanging;
			}
			throw;
		}
	}

	/// <summary>Composes the specified part, with recomposition and validation disabled.</summary>
	/// <param name="part">The part to compose.</param>
	public void SatisfyImportsOnce(ComposablePart part)
	{
		Requires.NotNull(part, "part");
		Assumes.NotNull(_compositionContainer);
		_compositionContainer.SatisfyImportsOnce(part);
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.ComponentModel.Composition.Hosting.CompositionContainer" /> class.</summary>
	public void Dispose()
	{
		Assumes.NotNull(_compositionContainer);
		if (_notifyCatalog != null)
		{
			_notifyCatalog.Changing -= OnCatalogChanging;
		}
		_compositionContainer.Dispose();
	}

	private void OnCatalogChanging(object sender, ComposablePartCatalogChangeEventArgs e)
	{
		throw new ChangeRejectedException(Strings.NotSupportedCatalogChanges);
	}

	internal CompositionService()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
