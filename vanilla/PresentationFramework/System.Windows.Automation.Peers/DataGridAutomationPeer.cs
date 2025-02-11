using System.Collections;
using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.DataGrid" /> types to UI Automation.</summary>
public sealed class DataGridAutomationPeer : ItemsControlAutomationPeer, IGridProvider, ISelectionProvider, ITableProvider
{
	/// <summary>Gets the total number of columns in a grid.</summary>
	/// <returns>The total number of columns in a grid.</returns>
	int IGridProvider.ColumnCount => OwningDataGrid.Columns.Count;

	/// <summary>Gets the total number of rows in a grid.</summary>
	/// <returns>The total number of rows in a grid.</returns>
	int IGridProvider.RowCount => OwningDataGrid.Items.Count;

	/// <summary>Gets a value that specifies whether the UI Automation provider allows more than one child element to be selected concurrently.</summary>
	/// <returns>true if multiple selection is allowed; otherwise false.</returns>
	bool ISelectionProvider.CanSelectMultiple => OwningDataGrid.SelectionMode == DataGridSelectionMode.Extended;

	/// <summary>Gets a value that specifies whether the UI Automation provider requires at least one child element to be selected.</summary>
	/// <returns>false in all cases.</returns>
	bool ISelectionProvider.IsSelectionRequired => false;

	/// <summary>Retrieves the primary direction of traversal for the table.</summary>
	/// <returns>The primary direction of traversal. </returns>
	RowOrColumnMajor ITableProvider.RowOrColumnMajor => RowOrColumnMajor.RowMajor;

	private DataGrid OwningDataGrid => (DataGrid)base.Owner;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.DataGridAutomationPeer" /> class. </summary>
	/// <param name="owner">The element associated with this automation peer.</param>
	public DataGridAutomationPeer(DataGrid owner)
		: base(owner)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.DataGrid;
	}

	protected override List<AutomationPeer> GetChildrenCore()
	{
		List<AutomationPeer> list = base.GetChildrenCore();
		DataGridColumnHeadersPresenter columnHeadersPresenter = OwningDataGrid.ColumnHeadersPresenter;
		if (columnHeadersPresenter != null && columnHeadersPresenter.IsVisible)
		{
			AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(columnHeadersPresenter);
			if (automationPeer != null)
			{
				if (list == null)
				{
					list = new List<AutomationPeer>(1);
				}
				list.Insert(0, automationPeer);
			}
		}
		return list;
	}

	protected override string GetClassNameCore()
	{
		return base.Owner.GetType().Name;
	}

	/// <summary>Returns the object that supports the specified control pattern of the element that is associated with this automation peer.</summary>
	/// <returns>If <paramref name="patternInterface" /> is <see cref="F:System.Windows.Automation.Peers.PatternInterface.Grid" />, <see cref="F:System.Windows.Automation.Peers.PatternInterface.Selection" />, or <see cref="F:System.Windows.Automation.Peers.PatternInterface.Table" />, this method returns a this pointer; otherwise, this method returns null.</returns>
	/// <param name="patternInterface">A value in the enumeration.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		switch (patternInterface)
		{
		case PatternInterface.Selection:
		case PatternInterface.Grid:
		case PatternInterface.Table:
			return this;
		case PatternInterface.Scroll:
		{
			ScrollViewer internalScrollHost = OwningDataGrid.InternalScrollHost;
			if (internalScrollHost != null)
			{
				AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(internalScrollHost);
				IScrollProvider scrollProvider = automationPeer as IScrollProvider;
				if (automationPeer != null && scrollProvider != null)
				{
					automationPeer.EventsSource = this;
					return scrollProvider;
				}
			}
			break;
		}
		}
		return base.GetPattern(patternInterface);
	}

	protected override ItemAutomationPeer CreateItemAutomationPeer(object item)
	{
		return new DataGridItemAutomationPeer(item, this);
	}

	internal override bool IsPropertySupportedByControlForFindItem(int id)
	{
		return SelectorAutomationPeer.IsPropertySupportedByControlForFindItemInternal(id);
	}

	internal override object GetSupportedPropertyValue(ItemAutomationPeer itemPeer, int propertyId)
	{
		return SelectorAutomationPeer.GetSupportedPropertyValueInternal(itemPeer, propertyId);
	}

	/// <summary>Retrieves the UI Automation provider for the specified cell.</summary>
	/// <returns>The UI Automation provider for the specified cell.</returns>
	/// <param name="row">The ordinal number of the row of interest.</param>
	/// <param name="column">The ordinal number of the column of interest.</param>
	IRawElementProviderSimple IGridProvider.GetItem(int row, int column)
	{
		if (row >= 0 && row < OwningDataGrid.Items.Count && column >= 0 && column < OwningDataGrid.Columns.Count)
		{
			object item = OwningDataGrid.Items[row];
			DataGridColumn column2 = OwningDataGrid.Columns[column];
			OwningDataGrid.ScrollIntoView(item, column2);
			OwningDataGrid.UpdateLayout();
			if (FindOrCreateItemAutomationPeer(item) is DataGridItemAutomationPeer dataGridItemAutomationPeer)
			{
				DataGridCellItemAutomationPeer orCreateCellItemPeer = dataGridItemAutomationPeer.GetOrCreateCellItemPeer(column2);
				if (orCreateCellItemPeer != null)
				{
					return ProviderFromPeer(orCreateCellItemPeer);
				}
			}
		}
		return null;
	}

	/// <summary>Retrieves a UI Automation provider for each child element that is selected.</summary>
	/// <returns>A collection of UI Automation providers. </returns>
	IRawElementProviderSimple[] ISelectionProvider.GetSelection()
	{
		List<IRawElementProviderSimple> list = new List<IRawElementProviderSimple>();
		switch (OwningDataGrid.SelectionUnit)
		{
		case DataGridSelectionUnit.Cell:
			AddSelectedCells(list);
			break;
		case DataGridSelectionUnit.FullRow:
			AddSelectedRows(list);
			break;
		case DataGridSelectionUnit.CellOrRowHeader:
			AddSelectedRows(list);
			AddSelectedCells(list);
			break;
		}
		return list.ToArray();
	}

	/// <summary>Gets a collection of UI Automation providers that represents all the column headers in a table.</summary>
	/// <returns>A collection of UI Automation providers. </returns>
	IRawElementProviderSimple[] ITableProvider.GetColumnHeaders()
	{
		if ((OwningDataGrid.HeadersVisibility & DataGridHeadersVisibility.Column) == DataGridHeadersVisibility.Column)
		{
			List<IRawElementProviderSimple> list = new List<IRawElementProviderSimple>();
			DataGridColumnHeadersPresenter columnHeadersPresenter = OwningDataGrid.ColumnHeadersPresenter;
			if (columnHeadersPresenter != null && columnHeadersPresenter.GetAutomationPeer() is DataGridColumnHeadersPresenterAutomationPeer dataGridColumnHeadersPresenterAutomationPeer)
			{
				for (int i = 0; i < OwningDataGrid.Columns.Count; i++)
				{
					AutomationPeer automationPeer = dataGridColumnHeadersPresenterAutomationPeer.FindOrCreateItemAutomationPeer(OwningDataGrid.Columns[i]);
					if (automationPeer != null)
					{
						list.Add(ProviderFromPeer(automationPeer));
					}
				}
				if (list.Count > 0)
				{
					return list.ToArray();
				}
			}
		}
		return null;
	}

	/// <summary>Retrieves a collection of UI Automation providers that represents all row headers in the table.</summary>
	/// <returns>A collection of UI Automation providers.</returns>
	IRawElementProviderSimple[] ITableProvider.GetRowHeaders()
	{
		if ((OwningDataGrid.HeadersVisibility & DataGridHeadersVisibility.Row) == DataGridHeadersVisibility.Row)
		{
			List<IRawElementProviderSimple> list = new List<IRawElementProviderSimple>();
			foreach (object item in (IEnumerable)OwningDataGrid.Items)
			{
				AutomationPeer rowHeaderAutomationPeer = (FindOrCreateItemAutomationPeer(item) as DataGridItemAutomationPeer).RowHeaderAutomationPeer;
				if (rowHeaderAutomationPeer != null)
				{
					list.Add(ProviderFromPeer(rowHeaderAutomationPeer));
				}
			}
			if (list.Count > 0)
			{
				return list.ToArray();
			}
		}
		return null;
	}

	private DataGridCellItemAutomationPeer GetCellItemPeer(DataGridCellInfo cellInfo)
	{
		if (cellInfo.IsValid && FindOrCreateItemAutomationPeer(cellInfo.Item) is DataGridItemAutomationPeer dataGridItemAutomationPeer)
		{
			return dataGridItemAutomationPeer.GetOrCreateCellItemPeer(cellInfo.Column);
		}
		return null;
	}

	internal void RaiseAutomationCellSelectedEvent(SelectedCellsChangedEventArgs e)
	{
		if (AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected) && OwningDataGrid.SelectedCells.Count == 1 && e.AddedCells.Count == 1)
		{
			GetCellItemPeer(e.AddedCells[0])?.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
			return;
		}
		if (AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection))
		{
			for (int i = 0; i < e.AddedCells.Count; i++)
			{
				GetCellItemPeer(e.AddedCells[i])?.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementAddedToSelection);
			}
		}
		if (AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection))
		{
			for (int i = 0; i < e.RemovedCells.Count; i++)
			{
				GetCellItemPeer(e.RemovedCells[i])?.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection);
			}
		}
	}

	internal void RaiseAutomationRowInvokeEvents(DataGridRow row)
	{
		if (FindOrCreateItemAutomationPeer(row.Item) is DataGridItemAutomationPeer dataGridItemAutomationPeer)
		{
			dataGridItemAutomationPeer.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
		}
	}

	internal void RaiseAutomationCellInvokeEvents(DataGridColumn column, DataGridRow row)
	{
		if (FindOrCreateItemAutomationPeer(row.Item) is DataGridItemAutomationPeer dataGridItemAutomationPeer)
		{
			dataGridItemAutomationPeer.GetOrCreateCellItemPeer(column)?.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
		}
	}

	internal void RaiseAutomationSelectionEvents(SelectionChangedEventArgs e)
	{
		int count = OwningDataGrid.SelectedItems.Count;
		int count2 = e.AddedItems.Count;
		if (AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected) && count == 1 && count2 == 1)
		{
			FindOrCreateItemAutomationPeer(OwningDataGrid.SelectedItem)?.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
			return;
		}
		if (AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection))
		{
			for (int i = 0; i < e.AddedItems.Count; i++)
			{
				FindOrCreateItemAutomationPeer(e.AddedItems[i])?.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementAddedToSelection);
			}
		}
		if (AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection))
		{
			for (int i = 0; i < e.RemovedItems.Count; i++)
			{
				FindOrCreateItemAutomationPeer(e.RemovedItems[i])?.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection);
			}
		}
	}

	private void AddSelectedCells(List<IRawElementProviderSimple> cellProviders)
	{
		if (cellProviders == null)
		{
			throw new ArgumentNullException("cellProviders");
		}
		if (OwningDataGrid.SelectedCells == null)
		{
			return;
		}
		foreach (DataGridCellInfo selectedCell in OwningDataGrid.SelectedCells)
		{
			if (FindOrCreateItemAutomationPeer(selectedCell.Item) is DataGridItemAutomationPeer dataGridItemAutomationPeer)
			{
				IRawElementProviderSimple rawElementProviderSimple = ProviderFromPeer(dataGridItemAutomationPeer.GetOrCreateCellItemPeer(selectedCell.Column));
				if (rawElementProviderSimple != null)
				{
					cellProviders.Add(rawElementProviderSimple);
				}
			}
		}
	}

	private void AddSelectedRows(List<IRawElementProviderSimple> itemProviders)
	{
		if (itemProviders == null)
		{
			throw new ArgumentNullException("itemProviders");
		}
		if (OwningDataGrid.SelectedItems == null)
		{
			return;
		}
		foreach (object selectedItem in OwningDataGrid.SelectedItems)
		{
			IRawElementProviderSimple rawElementProviderSimple = ProviderFromPeer(FindOrCreateItemAutomationPeer(selectedItem));
			if (rawElementProviderSimple != null)
			{
				itemProviders.Add(rawElementProviderSimple);
			}
		}
	}
}
