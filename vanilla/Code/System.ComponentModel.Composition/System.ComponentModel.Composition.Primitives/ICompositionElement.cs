namespace System.ComponentModel.Composition.Primitives;

/// <summary>Represents an element that participates in composition.</summary>
public interface ICompositionElement
{
	/// <summary>Gets the display name of the composition element.</summary>
	/// <returns>The human-readable display name of the <see cref="T:System.ComponentModel.Composition.Primitives.ICompositionElement" />.</returns>
	string DisplayName { get; }

	/// <summary>Gets the composition element from which the current composition element originated.</summary>
	/// <returns>The composition element from which the current <see cref="T:System.ComponentModel.Composition.Primitives.ICompositionElement" /> originated, or null if the <see cref="T:System.ComponentModel.Composition.Primitives.ICompositionElement" /> is the root composition element.</returns>
	ICompositionElement Origin { get; }
}
