namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.VirtualizingStackPanel.CleanUpVirtualizedItem" /> event.</summary>
public class CleanUpVirtualizedItemEventArgs : RoutedEventArgs
{
	private object _value;

	private UIElement _element;

	private bool _cancel;

	/// <summary>Gets an <see cref="T:System.Object" /> that represents the original data value.</summary>
	/// <returns>The <see cref="T:System.Object" /> that represents the original data value.</returns>
	public object Value => _value;

	/// <summary>Gets an instance of the visual element that represents the data value.</summary>
	/// <returns>The <see cref="T:System.Windows.UIElement" /> that represents the data value.</returns>
	public UIElement UIElement => _element;

	/// <summary>Gets or sets a value that indicates whether this item should not be re-virtualized.</summary>
	/// <returns>true if you want to prevent revirtualization of this item; otherwise false.</returns>
	public bool Cancel
	{
		get
		{
			return _cancel;
		}
		set
		{
			_cancel = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.CleanUpVirtualizedItemEventArgs" /> class.</summary>
	public CleanUpVirtualizedItemEventArgs(object value, UIElement element)
		: base(VirtualizingStackPanel.CleanUpVirtualizedItemEvent)
	{
		_value = value;
		_element = element;
	}
}
