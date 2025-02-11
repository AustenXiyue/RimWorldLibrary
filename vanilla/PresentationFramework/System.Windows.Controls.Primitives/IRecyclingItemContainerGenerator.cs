namespace System.Windows.Controls.Primitives;

/// <summary>Extends the <see cref="T:System.Windows.Controls.Primitives.IItemContainerGenerator" /> interface to reuse the UI content it generates. Classes that are responsible for generating user interface (UI) content on behalf of a host implement this interface.</summary>
public interface IRecyclingItemContainerGenerator : IItemContainerGenerator
{
	/// <summary>Disassociates item containers from their data items and saves the containers so they can be reused later for other data items.</summary>
	/// <param name="position">The zero-based index of the first element to reuse. <paramref name="position" /> must refer to a previously generated (realized) item.</param>
	/// <param name="count">The number of elements to reuse, starting at <paramref name="position" />.</param>
	void Recycle(GeneratorPosition position, int count);
}
