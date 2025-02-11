namespace System.Windows.Controls;

internal class DefaultItemContainerTemplateSelector : ItemContainerTemplateSelector
{
	public override DataTemplate SelectTemplate(object item, ItemsControl parentItemsControl)
	{
		return FrameworkElement.FindTemplateResourceInternal(parentItemsControl, item, typeof(ItemContainerTemplate)) as DataTemplate;
	}
}
