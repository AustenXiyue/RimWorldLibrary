namespace System.Windows.Controls;

/// <summary>Represents an item in a <see cref="T:System.Windows.Controls.ListView" /> control.</summary>
public class ListViewItem : ListBoxItem
{
	internal void SetDefaultStyleKey(object key)
	{
		base.DefaultStyleKey = key;
	}

	internal void ClearDefaultStyleKey()
	{
		ClearValue(FrameworkElement.DefaultStyleKeyProperty);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ListViewItem" /> class.</summary>
	public ListViewItem()
	{
	}
}
