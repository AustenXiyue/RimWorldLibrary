using System.ComponentModel.Composition.Primitives;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting;

/// <summary>Provides extension methods for constructing composition services.</summary>
public static class CatalogExtensions
{
	/// <summary>Creates a new composition service by using the specified catalog as a source for exports.</summary>
	/// <returns>A new composition service.</returns>
	/// <param name="composablePartCatalog">The catalog that will provide exports.</param>
	public static CompositionService CreateCompositionService(this ComposablePartCatalog composablePartCatalog)
	{
		Requires.NotNull(composablePartCatalog, "composablePartCatalog");
		return new CompositionService(composablePartCatalog);
	}
}
