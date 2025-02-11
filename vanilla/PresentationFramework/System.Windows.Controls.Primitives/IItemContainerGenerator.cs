namespace System.Windows.Controls.Primitives;

/// <summary>An interface that is implemented by classes which are responsible for generating user interface (UI) content on behalf of a host.</summary>
public interface IItemContainerGenerator
{
	/// <summary>Returns the <see cref="T:System.Windows.Controls.ItemContainerGenerator" /> appropriate for use by the specified panel.</summary>
	/// <returns>An <see cref="T:System.Windows.Controls.ItemContainerGenerator" /> appropriate for use by the specified panel.</returns>
	/// <param name="panel">The panel for which to return an appropriate <see cref="T:System.Windows.Controls.ItemContainerGenerator" />.</param>
	ItemContainerGenerator GetItemContainerGeneratorForPanel(Panel panel);

	/// <summary>Prepares the generator to generate items, starting at the specified <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" />, and in the specified <see cref="T:System.Windows.Controls.Primitives.GeneratorDirection" />.</summary>
	/// <returns>An <see cref="T:System.IDisposable" /> object that tracks the lifetime of the generation process.</returns>
	/// <param name="position">A <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" />, that specifies the position of the item to start generating items at.</param>
	/// <param name="direction">A <see cref="T:System.Windows.Controls.Primitives.GeneratorDirection" /> that specifies the direction which to generate items.</param>
	IDisposable StartAt(GeneratorPosition position, GeneratorDirection direction);

	/// <summary>Prepares the generator to generate items, starting at the specified <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" />, and in the specified <see cref="T:System.Windows.Controls.Primitives.GeneratorDirection" />, and controlling whether or not to start at a generated (realized) item.</summary>
	/// <returns>An <see cref="T:System.IDisposable" /> object that tracks the lifetime of the generation process.</returns>
	/// <param name="position">A <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" />, that specifies the position of the item to start generating items at.</param>
	/// <param name="direction">Specifies the position of the item to start generating items at.</param>
	/// <param name="allowStartAtRealizedItem">A <see cref="T:System.Boolean" /> that specifies whether to start at a generated (realized) item.</param>
	IDisposable StartAt(GeneratorPosition position, GeneratorDirection direction, bool allowStartAtRealizedItem);

	/// <summary>Returns the container element used to display the next item.</summary>
	/// <returns>A <see cref="T:System.Windows.DependencyObject" /> that is the container element which is used to display the next item.</returns>
	DependencyObject GenerateNext();

	/// <summary>Returns the container element used to display the next item, and whether the container element has been newly generated (realized).</summary>
	/// <returns>A <see cref="T:System.Windows.DependencyObject" /> that is the container element which is used to display the next item.</returns>
	/// <param name="isNewlyRealized">Is true if the returned <see cref="T:System.Windows.DependencyObject" /> is newly generated (realized); otherwise, false.</param>
	DependencyObject GenerateNext(out bool isNewlyRealized);

	/// <summary>Prepares the specified element as the container for the corresponding item.</summary>
	/// <param name="container">The container to prepare. Normally, <paramref name="container" /> is the result of the previous call to <see cref="Overload:System.Windows.Controls.Primitives.IItemContainerGenerator.GenerateNext" />.</param>
	void PrepareItemContainer(DependencyObject container);

	/// <summary>Removes all generated (realized) items.</summary>
	void RemoveAll();

	/// <summary>Removes one or more generated (realized) items.</summary>
	/// <param name="position">The <see cref="T:System.Int32" /> index of the element to remove. <paramref name="position" /> must refer to a previously generated (realized) item, which means its offset must be zero.</param>
	/// <param name="count">The <see cref="T:System.Int32" /> number of elements to remove, starting at <paramref name="position" />.</param>
	void Remove(GeneratorPosition position, int count);

	/// <summary>Returns the <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" /> object that maps to the item at the specified index.</summary>
	/// <returns>An <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" /> object that maps to the item at the specified index.</returns>
	/// <param name="itemIndex">The index of desired item. </param>
	GeneratorPosition GeneratorPositionFromIndex(int itemIndex);

	/// <summary>Returns the index that maps to the specified <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" />.</summary>
	/// <returns>An <see cref="T:System.Int32" /> that is the index which maps to the specified <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" />.</returns>
	/// <param name="position">The index of desired item.The <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" />  for the desired index.</param>
	int IndexFromGeneratorPosition(GeneratorPosition position);
}
