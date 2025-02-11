using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MS.Internal.Controls;

namespace System.Windows.Automation.Peers;

internal class RecyclableWrapper : IDisposable
{
	private ItemsControl _itemsControl;

	private DependencyObject _container;

	private object _item;

	public AutomationPeer Peer => UIElementAutomationPeer.CreatePeerForElement((UIElement)_container);

	public RecyclableWrapper(ItemsControl itemsControl, object item)
	{
		_itemsControl = itemsControl;
		_container = ((IGeneratorHost)itemsControl).GetContainerForItem(item);
		LinkItem(item);
	}

	public void LinkItem(object item)
	{
		_item = item;
		ItemContainerGenerator.LinkContainerToItem(_container, _item);
		((IItemContainerGenerator)_itemsControl.ItemContainerGenerator).PrepareItemContainer(_container);
	}

	private void UnlinkItem()
	{
		if (_item != null)
		{
			ItemContainerGenerator.UnlinkContainerFromItem(_container, _item, _itemsControl);
			_item = null;
		}
	}

	void IDisposable.Dispose()
	{
		UnlinkItem();
	}
}
