using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using MS.Internal.Automation;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.DataGridRow" /> types to UI Automation. The <see cref="T:System.Windows.Controls.DataGridRow" /> may or may not actually exist in memory.</summary>
public sealed class DataGridItemAutomationPeer : ItemAutomationPeer, IInvokeProvider, IScrollItemProvider, ISelectionItemProvider, ISelectionProvider, IItemContainerProvider
{
	private AutomationPeer _dataGridAutomationPeer;

	private ItemPeersStorage<DataGridCellItemAutomationPeer> _dataChildren = new ItemPeersStorage<DataGridCellItemAutomationPeer>();

	private ItemPeersStorage<WeakReference> _weakRefElementProxyStorage = new ItemPeersStorage<WeakReference>();

	/// <summary>Gets a value that indicates whether an item is selected. </summary>
	/// <returns>true if the element is selected; otherwise, false.</returns>
	bool ISelectionItemProvider.IsSelected => OwningDataGrid.SelectedItems.Contains(base.Item);

	/// <summary>Gets the UI Automation provider that implements <see cref="T:System.Windows.Automation.Provider.ISelectionProvider" /> and acts as the container for the calling object.</summary>
	/// <returns>The provider for the <see cref="T:System.Windows.Controls.DataGrid" /> control. </returns>
	IRawElementProviderSimple ISelectionItemProvider.SelectionContainer => ProviderFromPeer(_dataGridAutomationPeer);

	/// <summary>Gets a value that indicates whether the UI Automation provider allows more than one child element to be selected concurrently.</summary>
	/// <returns>true if multiple selection is allowed; otherwise, false.</returns>
	bool ISelectionProvider.CanSelectMultiple => OwningDataGrid.SelectionMode == DataGridSelectionMode.Extended;

	/// <summary>Gets a value that specifies whether the UI Automation provider requires at least one child element to be selected.</summary>
	/// <returns>false in all cases.</returns>
	bool ISelectionProvider.IsSelectionRequired => false;

	private bool IsRowSelectionUnit
	{
		get
		{
			if (OwningDataGrid != null)
			{
				if (OwningDataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
				{
					return OwningDataGrid.SelectionUnit == DataGridSelectionUnit.CellOrRowHeader;
				}
				return true;
			}
			return false;
		}
	}

	private bool IsNewItemPlaceholder
	{
		get
		{
			object item = base.Item;
			if (item != CollectionView.NewItemPlaceholder)
			{
				return item == DataGrid.NewItemPlaceholder;
			}
			return true;
		}
	}

	internal AutomationPeer RowHeaderAutomationPeer
	{
		get
		{
			if (!(GetWrapperPeer() is DataGridRowAutomationPeer dataGridRowAutomationPeer))
			{
				return null;
			}
			return dataGridRowAutomationPeer.RowHeaderAutomationPeer;
		}
	}

	private DataGrid OwningDataGrid => (DataGrid)(_dataGridAutomationPeer as DataGridAutomationPeer).Owner;

	private ItemPeersStorage<DataGridCellItemAutomationPeer> CellItemPeers
	{
		get
		{
			return _dataChildren;
		}
		set
		{
			_dataChildren = value;
		}
	}

	private ItemPeersStorage<WeakReference> WeakRefElementProxyStorage => _weakRefElementProxyStorage;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.DataGridItemAutomationPeer" /> class. </summary>
	/// <param name="item">The data item in the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection that is associated with this <see cref="T:System.Windows.Automation.Peers.DataGridItemAutomationPeer" />.</param>
	/// <param name="dataGridPeer">The <see cref="T:System.Windows.Automation.Peers.DataGridAutomationPeer" /> that is associated with the <see cref="T:System.Windows.Controls.DataGrid" /> that holds the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection.</param>
	public DataGridItemAutomationPeer(object item, DataGridAutomationPeer dataGridPeer)
		: base(item, dataGridPeer)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (dataGridPeer == null)
		{
			throw new ArgumentNullException("dataGridPeer");
		}
		_dataGridAutomationPeer = dataGridPeer;
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.DataItem;
	}

	protected override List<AutomationPeer> GetChildrenCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			wrapperPeer.ForceEnsureChildren();
			return wrapperPeer.GetChildren();
		}
		return GetCellItemPeers();
	}

	protected override string GetClassNameCore()
	{
		AutomationPeer wrapperPeer = GetWrapperPeer();
		if (wrapperPeer != null)
		{
			return wrapperPeer.GetClassName();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	/// <summary>Returns the object that supports the specified control pattern of the element that is associated with this automation peer.</summary>
	/// <returns>The current <see cref="T:System.Windows.Automation.Peers.DataGridCellItemAutomationPeer" /> object, if <paramref name="patternInterface" /> is a supported value; otherwise, null. For more information, see Remarks.</returns>
	/// <param name="patternInterface">An enumeration value that specifies the control pattern.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		switch (patternInterface)
		{
		case PatternInterface.Invoke:
			if (!OwningDataGrid.IsReadOnly)
			{
				return this;
			}
			break;
		case PatternInterface.Selection:
		case PatternInterface.ScrollItem:
		case PatternInterface.ItemContainer:
			return this;
		case PatternInterface.SelectionItem:
			if (IsRowSelectionUnit)
			{
				return this;
			}
			break;
		}
		return base.GetPattern(patternInterface);
	}

	protected override AutomationPeer GetPeerFromPointCore(Point point)
	{
		if (!IsOffscreen())
		{
			AutomationPeer rowHeaderAutomationPeer = RowHeaderAutomationPeer;
			if (rowHeaderAutomationPeer != null)
			{
				AutomationPeer peerFromPoint = rowHeaderAutomationPeer.GetPeerFromPoint(point);
				if (peerFromPoint != null)
				{
					return peerFromPoint;
				}
			}
		}
		return base.GetPeerFromPointCore(point);
	}

	/// <summary>Retrieves an element by the specified property value.</summary>
	/// <returns>The first item that matches the search criterion; otherwise, null if no items match.</returns>
	/// <param name="startAfter">The item in the container after which to begin the search.</param>
	/// <param name="propertyId">The property that contains the value to retrieve.</param>
	/// <param name="value">The value to retrieve.</param>
	IRawElementProviderSimple IItemContainerProvider.FindItemByProperty(IRawElementProviderSimple startAfter, int propertyId, object value)
	{
		ResetChildrenCache();
		if (propertyId != 0 && !SelectorAutomationPeer.IsPropertySupportedByControlForFindItemInternal(propertyId))
		{
			throw new ArgumentException(SR.PropertyNotSupported);
		}
		IList<DataGridColumn> columns = OwningDataGrid.Columns;
		if (columns != null && columns.Count > 0)
		{
			DataGridCellItemAutomationPeer dataGridCellItemAutomationPeer = null;
			if (startAfter != null)
			{
				dataGridCellItemAutomationPeer = PeerFromProvider(startAfter) as DataGridCellItemAutomationPeer;
			}
			int num = 0;
			if (dataGridCellItemAutomationPeer != null)
			{
				if (dataGridCellItemAutomationPeer.Column == null)
				{
					throw new InvalidOperationException(SR.InavalidStartItem);
				}
				num = columns.IndexOf(dataGridCellItemAutomationPeer.Column) + 1;
				if (num == 0 || num == columns.Count)
				{
					return null;
				}
			}
			if (propertyId == 0 && num < columns.Count)
			{
				return ProviderFromPeer(GetOrCreateCellItemPeer(columns[num]));
			}
			object obj = null;
			for (int i = num; i < columns.Count; i++)
			{
				DataGridCellItemAutomationPeer orCreateCellItemPeer = GetOrCreateCellItemPeer(columns[i]);
				if (orCreateCellItemPeer == null)
				{
					continue;
				}
				try
				{
					obj = SelectorAutomationPeer.GetSupportedPropertyValueInternal(orCreateCellItemPeer, propertyId);
				}
				catch (Exception ex)
				{
					if (ex is ElementNotAvailableException)
					{
						continue;
					}
				}
				if (value == null || obj == null)
				{
					if (obj == null && value == null)
					{
						return ProviderFromPeer(orCreateCellItemPeer);
					}
				}
				else if (value.Equals(obj))
				{
					return ProviderFromPeer(orCreateCellItemPeer);
				}
			}
		}
		return null;
	}

	/// <summary>Sends a request to activate a control and initiate its single, unambiguous action.</summary>
	void IInvokeProvider.Invoke()
	{
		EnsureEnabled();
		object item = base.Item;
		if (GetWrapperPeer() == null)
		{
			OwningDataGrid.ScrollIntoView(item);
		}
		bool flag = false;
		if (GetWrapper() != null)
		{
			if (((IEditableCollectionView)OwningDataGrid.Items).CurrentEditItem == item)
			{
				flag = OwningDataGrid.CommitEdit();
			}
			else if (OwningDataGrid.Columns.Count > 0)
			{
				DataGridCell dataGridCell = OwningDataGrid.TryFindCell(item, OwningDataGrid.Columns[0]);
				if (dataGridCell != null)
				{
					OwningDataGrid.UnselectAll();
					dataGridCell.Focus();
					flag = OwningDataGrid.BeginEdit();
				}
			}
		}
		if (!flag && !IsNewItemPlaceholder)
		{
			throw new InvalidOperationException(SR.DataGrid_AutomationInvokeFailed);
		}
	}

	/// <summary>Scrolls the content area of a container object to display the control within the visible region (viewport) of the container.</summary>
	void IScrollItemProvider.ScrollIntoView()
	{
		OwningDataGrid.ScrollIntoView(base.Item);
	}

	/// <summary>Adds the current element to the collection of selected items.</summary>
	void ISelectionItemProvider.AddToSelection()
	{
		if (!IsRowSelectionUnit)
		{
			throw new InvalidOperationException(SR.DataGridRow_CannotSelectRowWhenCells);
		}
		object item = base.Item;
		if (!OwningDataGrid.SelectedItems.Contains(item))
		{
			EnsureEnabled();
			if (OwningDataGrid.SelectionMode == DataGridSelectionMode.Single && OwningDataGrid.SelectedItems.Count > 0)
			{
				throw new InvalidOperationException();
			}
			if (OwningDataGrid.Items.Contains(item))
			{
				OwningDataGrid.SelectedItems.Add(item);
			}
		}
	}

	/// <summary>Removes the current element from the collection of selected items.</summary>
	void ISelectionItemProvider.RemoveFromSelection()
	{
		if (!IsRowSelectionUnit)
		{
			throw new InvalidOperationException(SR.DataGridRow_CannotSelectRowWhenCells);
		}
		EnsureEnabled();
		object item = base.Item;
		if (OwningDataGrid.SelectedItems.Contains(item))
		{
			OwningDataGrid.SelectedItems.Remove(item);
		}
	}

	/// <summary>Clears any selected items and then selects the current element.</summary>
	void ISelectionItemProvider.Select()
	{
		if (!IsRowSelectionUnit)
		{
			throw new InvalidOperationException(SR.DataGridRow_CannotSelectRowWhenCells);
		}
		EnsureEnabled();
		OwningDataGrid.SelectedItem = base.Item;
	}

	/// <summary>Retrieves a UI Automation provider for each child element that is selected.</summary>
	/// <returns>A collection of UI Automation providers. </returns>
	IRawElementProviderSimple[] ISelectionProvider.GetSelection()
	{
		DataGrid owningDataGrid = OwningDataGrid;
		if (owningDataGrid == null)
		{
			return null;
		}
		int num = owningDataGrid.Items.IndexOf(base.Item);
		if (num > -1 && owningDataGrid.SelectedCellsInternal.Intersects(num))
		{
			List<IRawElementProviderSimple> list = new List<IRawElementProviderSimple>();
			for (int i = 0; i < OwningDataGrid.Columns.Count; i++)
			{
				if (owningDataGrid.SelectedCellsInternal.Contains(num, i))
				{
					DataGridColumn column = owningDataGrid.ColumnFromDisplayIndex(i);
					DataGridCellItemAutomationPeer orCreateCellItemPeer = GetOrCreateCellItemPeer(column);
					if (orCreateCellItemPeer != null)
					{
						list.Add(ProviderFromPeer(orCreateCellItemPeer));
					}
				}
			}
			if (list.Count > 0)
			{
				return list.ToArray();
			}
		}
		return null;
	}

	internal List<AutomationPeer> GetCellItemPeers()
	{
		List<AutomationPeer> list = null;
		ItemPeersStorage<DataGridCellItemAutomationPeer> itemPeersStorage = new ItemPeersStorage<DataGridCellItemAutomationPeer>();
		IList list2 = null;
		bool flag = false;
		if (GetWrapper() is DataGridRow { CellsPresenter: not null } dataGridRow)
		{
			Panel itemsHost = dataGridRow.CellsPresenter.ItemsHost;
			if (itemsHost != null)
			{
				list2 = itemsHost.Children;
				flag = true;
			}
		}
		if (!flag)
		{
			list2 = OwningDataGrid.Columns;
		}
		if (list2 != null)
		{
			list = new List<AutomationPeer>(list2.Count);
			foreach (object item in list2)
			{
				DataGridColumn dataGridColumn = null;
				dataGridColumn = ((!flag) ? (item as DataGridColumn) : (item as DataGridCell).Column);
				if (dataGridColumn != null)
				{
					DataGridCellItemAutomationPeer orCreateCellItemPeer = GetOrCreateCellItemPeer(dataGridColumn, addParentInfo: false);
					list.Add(orCreateCellItemPeer);
					itemPeersStorage[dataGridColumn] = orCreateCellItemPeer;
				}
			}
		}
		CellItemPeers = itemPeersStorage;
		return list;
	}

	internal DataGridCellItemAutomationPeer GetOrCreateCellItemPeer(DataGridColumn column)
	{
		return GetOrCreateCellItemPeer(column, addParentInfo: true);
	}

	private DataGridCellItemAutomationPeer GetOrCreateCellItemPeer(DataGridColumn column, bool addParentInfo)
	{
		DataGridCellItemAutomationPeer dataGridCellItemAutomationPeer = CellItemPeers[column];
		if (dataGridCellItemAutomationPeer == null)
		{
			dataGridCellItemAutomationPeer = GetPeerFromWeakRefStorage(column);
			if (dataGridCellItemAutomationPeer != null && !addParentInfo)
			{
				dataGridCellItemAutomationPeer.AncestorsInvalid = false;
				dataGridCellItemAutomationPeer.ChildrenValid = false;
			}
		}
		if (dataGridCellItemAutomationPeer == null)
		{
			dataGridCellItemAutomationPeer = new DataGridCellItemAutomationPeer(base.Item, column);
			if (addParentInfo)
			{
				dataGridCellItemAutomationPeer?.TrySetParentInfo(this);
			}
		}
		AutomationPeer owningCellPeer = dataGridCellItemAutomationPeer.OwningCellPeer;
		if (owningCellPeer != null)
		{
			owningCellPeer.EventsSource = dataGridCellItemAutomationPeer;
		}
		return dataGridCellItemAutomationPeer;
	}

	private DataGridCellItemAutomationPeer GetPeerFromWeakRefStorage(object column)
	{
		DataGridCellItemAutomationPeer dataGridCellItemAutomationPeer = null;
		WeakReference weakReference = WeakRefElementProxyStorage[column];
		if (weakReference != null)
		{
			if (weakReference.Target is ElementProxy provider)
			{
				dataGridCellItemAutomationPeer = PeerFromProvider(provider) as DataGridCellItemAutomationPeer;
				if (dataGridCellItemAutomationPeer == null)
				{
					WeakRefElementProxyStorage.Remove(column);
				}
			}
			else
			{
				WeakRefElementProxyStorage.Remove(column);
			}
		}
		return dataGridCellItemAutomationPeer;
	}

	internal void AddProxyToWeakRefStorage(WeakReference wr, DataGridCellItemAutomationPeer cellItemPeer)
	{
		IList<DataGridColumn> columns = OwningDataGrid.Columns;
		if (columns != null && columns.Contains(cellItemPeer.Column) && GetPeerFromWeakRefStorage(cellItemPeer.Column) == null)
		{
			WeakRefElementProxyStorage[cellItemPeer.Column] = wr;
		}
	}

	private void EnsureEnabled()
	{
		if (!_dataGridAutomationPeer.IsEnabled())
		{
			throw new ElementNotEnabledException();
		}
	}
}
