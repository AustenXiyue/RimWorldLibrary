using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;

namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.Primitives.Selector.SelectionChanged" /> event. </summary>
public class SelectionChangedEventArgs : RoutedEventArgs
{
	private object[] _addedItems;

	private object[] _removedItems;

	private List<ItemsControl.ItemInfo> _addedInfos;

	private List<ItemsControl.ItemInfo> _removedInfos;

	/// <summary>Gets a list that contains the items that were unselected. </summary>
	/// <returns>The items that were unselected since the last time the <see cref="E:System.Windows.Controls.Primitives.Selector.SelectionChanged" /> event occurred.</returns>
	public IList RemovedItems => _removedItems;

	/// <summary>Gets a list that contains the items that were selected. </summary>
	/// <returns>The items that were selected since the last time the <see cref="E:System.Windows.Controls.Primitives.Selector.SelectionChanged" /> event occurred.</returns>
	public IList AddedItems => _addedItems;

	internal List<ItemsControl.ItemInfo> RemovedInfos => _removedInfos;

	internal List<ItemsControl.ItemInfo> AddedInfos => _addedInfos;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.SelectionChangedEventArgs" /> class. </summary>
	/// <param name="id">The event identifier (ID).</param>
	/// <param name="removedItems">The items that were unselected during this event.</param>
	/// <param name="addedItems">The items that were selected during this event.</param>
	public SelectionChangedEventArgs(RoutedEvent id, IList removedItems, IList addedItems)
	{
		if (id == null)
		{
			throw new ArgumentNullException("id");
		}
		if (removedItems == null)
		{
			throw new ArgumentNullException("removedItems");
		}
		if (addedItems == null)
		{
			throw new ArgumentNullException("addedItems");
		}
		base.RoutedEvent = id;
		_removedItems = new object[removedItems.Count];
		removedItems.CopyTo(_removedItems, 0);
		_addedItems = new object[addedItems.Count];
		addedItems.CopyTo(_addedItems, 0);
	}

	internal SelectionChangedEventArgs(List<ItemsControl.ItemInfo> unselectedInfos, List<ItemsControl.ItemInfo> selectedInfos)
	{
		base.RoutedEvent = Selector.SelectionChangedEvent;
		_removedItems = new object[unselectedInfos.Count];
		for (int i = 0; i < unselectedInfos.Count; i++)
		{
			_removedItems[i] = unselectedInfos[i].Item;
		}
		_addedItems = new object[selectedInfos.Count];
		for (int j = 0; j < selectedInfos.Count; j++)
		{
			_addedItems[j] = selectedInfos[j].Item;
		}
		_removedInfos = unselectedInfos;
		_addedInfos = selectedInfos;
	}

	/// <summary>Performs the proper type casting to call the type-safe <see cref="T:System.Windows.Controls.SelectionChangedEventHandler" /> delegate for the <see cref="E:System.Windows.Controls.Primitives.Selector.SelectionChanged" /> event. </summary>
	/// <param name="genericHandler">The handler to invoke.</param>
	/// <param name="genericTarget">The current object along the event's route.</param>
	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((SelectionChangedEventHandler)genericHandler)(genericTarget, this);
	}
}
