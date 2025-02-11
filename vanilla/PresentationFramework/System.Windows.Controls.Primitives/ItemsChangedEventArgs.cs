using System.Collections.Specialized;

namespace System.Windows.Controls.Primitives;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.ItemContainerGenerator.ItemsChanged" /> event.</summary>
public class ItemsChangedEventArgs : EventArgs
{
	private NotifyCollectionChangedAction _action;

	private GeneratorPosition _position;

	private GeneratorPosition _oldPosition;

	private int _itemCount;

	private int _itemUICount;

	/// <summary>Gets the action that occurred on the items collection.</summary>
	/// <returns>Returns the action that occurred.</returns>
	public NotifyCollectionChangedAction Action => _action;

	/// <summary>Gets the position in the collection where the change occurred.</summary>
	/// <returns>Returns a <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" />.</returns>
	public GeneratorPosition Position => _position;

	/// <summary>Gets the position in the collection before the change occurred.</summary>
	/// <returns>Returns a <see cref="T:System.Windows.Controls.Primitives.GeneratorPosition" />.</returns>
	public GeneratorPosition OldPosition => _oldPosition;

	/// <summary>Gets the number of items that were involved in the change.</summary>
	/// <returns>Integer that represents the number of items involved in the change.</returns>
	public int ItemCount => _itemCount;

	/// <summary>Gets the number of user interface (UI) elements involved in the change.</summary>
	/// <returns>Integer that represents the number of UI elements involved in the change.</returns>
	public int ItemUICount => _itemUICount;

	internal ItemsChangedEventArgs(NotifyCollectionChangedAction action, GeneratorPosition position, GeneratorPosition oldPosition, int itemCount, int itemUICount)
	{
		_action = action;
		_position = position;
		_oldPosition = oldPosition;
		_itemCount = itemCount;
		_itemUICount = itemUICount;
	}

	internal ItemsChangedEventArgs(NotifyCollectionChangedAction action, GeneratorPosition position, int itemCount, int itemUICount)
		: this(action, position, new GeneratorPosition(-1, 0), itemCount, itemUICount)
	{
	}
}
