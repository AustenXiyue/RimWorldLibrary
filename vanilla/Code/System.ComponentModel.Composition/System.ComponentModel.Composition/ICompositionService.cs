using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition;

/// <summary>Provides methods to satisfy imports on an existing part instance.</summary>
public interface ICompositionService
{
	/// <summary>Composes the specified part, with recomposition and validation disabled.</summary>
	/// <param name="part">The part to compose.</param>
	void SatisfyImportsOnce(ComposablePart part);
}
