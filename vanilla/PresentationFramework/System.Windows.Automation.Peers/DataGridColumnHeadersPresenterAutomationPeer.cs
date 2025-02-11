using System.Collections;
using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.Primitives.DataGridColumnHeadersPresenter" /> types to UI Automation.</summary>
public sealed class DataGridColumnHeadersPresenterAutomationPeer : ItemsControlAutomationPeer, IItemContainerProvider
{
	private DataGrid OwningDataGrid => ((DataGridColumnHeadersPresenter)base.Owner).ParentDataGrid;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.DataGridColumnHeadersPresenterAutomationPeer" /> class. </summary>
	/// <param name="owner">The element associated with this automation peer.</param>
	public DataGridColumnHeadersPresenterAutomationPeer(DataGridColumnHeadersPresenter owner)
		: base(owner)
	{
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Header;
	}

	protected override string GetClassNameCore()
	{
		return base.Owner.GetType().Name;
	}

	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> list = null;
		ItemPeersStorage<ItemAutomationPeer> itemPeers = base.ItemPeers;
		base.ItemPeers = new ItemPeersStorage<ItemAutomationPeer>();
		ItemsControl itemsControl = (ItemsControl)base.Owner;
		DataGrid owningDataGrid = OwningDataGrid;
		if (owningDataGrid != null && owningDataGrid.Columns.Count > 0)
		{
			IList list2 = null;
			Panel itemsHost = itemsControl.ItemsHost;
			if (IsVirtualized)
			{
				if (itemsHost == null)
				{
					return null;
				}
				list2 = itemsHost.Children;
			}
			else
			{
				list2 = OwningDataGrid.Columns;
			}
			list = new List<AutomationPeer>(list2.Count);
			{
				foreach (object item in list2)
				{
					DataGridColumn dataGridColumn = ((!(item is DataGridColumnHeader)) ? (item as DataGridColumn) : ((DataGridColumnHeader)item).Column);
					ItemAutomationPeer itemAutomationPeer = itemPeers[dataGridColumn];
					if (itemAutomationPeer == null)
					{
						itemAutomationPeer = GetPeerFromWeakRefStorage(dataGridColumn);
						if (itemAutomationPeer != null)
						{
							itemAutomationPeer.AncestorsInvalid = false;
							itemAutomationPeer.ChildrenValid = false;
						}
					}
					object o = dataGridColumn?.Header;
					if (itemAutomationPeer == null || !ItemsControl.EqualsEx(itemAutomationPeer.Item, o))
					{
						itemAutomationPeer = CreateItemAutomationPeer(dataGridColumn);
					}
					if (itemAutomationPeer != null)
					{
						AutomationPeer wrapperPeer = itemAutomationPeer.GetWrapperPeer();
						if (wrapperPeer != null)
						{
							wrapperPeer.EventsSource = itemAutomationPeer;
						}
					}
					if (itemAutomationPeer != null && base.ItemPeers[dataGridColumn] == null)
					{
						list.Add(itemAutomationPeer);
						base.ItemPeers[dataGridColumn] = itemAutomationPeer;
					}
				}
				return list;
			}
		}
		return null;
	}

	/// <summary>Retrieves an element with the specified property value.</summary>
	/// <returns>The first item that matches the search criterion; otherwise, null if no items match.</returns>
	/// <param name="startAfter">The item in the container after which to begin the search.</param>
	/// <param name="propertyId">The property that contains the value to retrieve.</param>
	/// <param name="value">The value to retrieve.</param>
	IRawElementProviderSimple IItemContainerProvider.FindItemByProperty(IRawElementProviderSimple startAfter, int propertyId, object value)
	{
		ResetChildrenCache();
		if (propertyId != 0 && !IsPropertySupportedByControlForFindItem(propertyId))
		{
			throw new ArgumentException(SR.PropertyNotSupported);
		}
		ItemsControl obj = (ItemsControl)base.Owner;
		IList list = null;
		if (obj != null)
		{
			list = OwningDataGrid.Columns;
		}
		if (list != null && list.Count > 0)
		{
			DataGridColumnHeaderItemAutomationPeer dataGridColumnHeaderItemAutomationPeer = null;
			if (startAfter != null)
			{
				dataGridColumnHeaderItemAutomationPeer = PeerFromProvider(startAfter) as DataGridColumnHeaderItemAutomationPeer;
				if (dataGridColumnHeaderItemAutomationPeer == null)
				{
					return null;
				}
			}
			int num = 0;
			if (dataGridColumnHeaderItemAutomationPeer != null)
			{
				if (dataGridColumnHeaderItemAutomationPeer.Item == null)
				{
					throw new InvalidOperationException(SR.InavalidStartItem);
				}
				num = list.IndexOf(dataGridColumnHeaderItemAutomationPeer.Column) + 1;
				if (num == 0 || num == list.Count)
				{
					return null;
				}
			}
			if (propertyId == 0)
			{
				for (int i = num; i < list.Count; i++)
				{
					if (list.IndexOf(list[i]) == i)
					{
						return ProviderFromPeer(FindOrCreateItemAutomationPeer(list[i]));
					}
				}
			}
			object obj2 = null;
			for (int j = num; j < list.Count; j++)
			{
				ItemAutomationPeer itemAutomationPeer = FindOrCreateItemAutomationPeer(list[j]);
				if (itemAutomationPeer == null)
				{
					continue;
				}
				try
				{
					obj2 = GetSupportedPropertyValue(itemAutomationPeer, propertyId);
				}
				catch (Exception ex)
				{
					if (ex is ElementNotAvailableException)
					{
						continue;
					}
				}
				if (value == null || obj2 == null)
				{
					if (obj2 == null && value == null && list.IndexOf(list[j]) == j)
					{
						return ProviderFromPeer(itemAutomationPeer);
					}
				}
				else if (value.Equals(obj2) && list.IndexOf(list[j]) == j)
				{
					return ProviderFromPeer(itemAutomationPeer);
				}
			}
		}
		return null;
	}

	protected override bool IsContentElementCore()
	{
		return false;
	}

	protected override ItemAutomationPeer CreateItemAutomationPeer(object column)
	{
		DataGridColumn dataGridColumn = column as DataGridColumn;
		if (column != null)
		{
			return new DataGridColumnHeaderItemAutomationPeer(dataGridColumn.Header, dataGridColumn, this);
		}
		return null;
	}
}
