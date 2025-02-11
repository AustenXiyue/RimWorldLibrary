namespace System.Windows;

/// <summary>Provides static utility methods for getting or setting the position of a <see cref="T:System.Windows.ContentElement" /> in an element tree.</summary>
public static class ContentOperations
{
	/// <summary>Gets the parent element of the specified <see cref="T:System.Windows.ContentElement" />.</summary>
	/// <returns>The parent element in the current tree.</returns>
	/// <param name="reference">The <see cref="T:System.Windows.ContentElement" /> to get the parent from.</param>
	public static DependencyObject GetParent(ContentElement reference)
	{
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		return reference._parent;
	}

	/// <summary>Sets the parent element of the provided <see cref="T:System.Windows.ContentElement" />.</summary>
	/// <param name="reference">The <see cref="T:System.Windows.ContentElement" /> to reparent.</param>
	/// <param name="parent">The new parent element.</param>
	public static void SetParent(ContentElement reference, DependencyObject parent)
	{
		if (reference == null)
		{
			throw new ArgumentNullException("reference");
		}
		DependencyObject parent2 = reference._parent;
		reference._parent = parent;
		reference.OnContentParentChanged(parent2);
	}
}
