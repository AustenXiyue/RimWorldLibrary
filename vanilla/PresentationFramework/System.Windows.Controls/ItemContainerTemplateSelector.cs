namespace System.Windows.Controls;

/// <summary>Enables you to select an <see cref="T:System.Windows.Controls.ItemContainerTemplate" /> for each item within an <see cref="T:System.Windows.Controls.ItemsControl" />. </summary>
public abstract class ItemContainerTemplateSelector
{
	/// <summary>When overridden in a derived class, returns an <see cref="T:System.Windows.Controls.ItemContainerTemplate" /> based on custom logic.</summary>
	/// <returns>The template. The default implementation returns null.</returns>
	/// <param name="item">The object for which to select the template.</param>
	/// <param name="parentItemsControl">The container for the items.</param>
	public virtual DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
	{
		return null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ItemContainerTemplateSelector" /> class.</summary>
	protected ItemContainerTemplateSelector()
	{
	}
}
