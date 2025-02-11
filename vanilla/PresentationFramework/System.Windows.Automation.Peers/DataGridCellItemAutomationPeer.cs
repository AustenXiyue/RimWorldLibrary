using System.Collections.Generic;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;

namespace System.Windows.Automation.Peers;

/// <summary>Exposes <see cref="T:System.Windows.Controls.DataGridCell" /> types to UI Automation.</summary>
public sealed class DataGridCellItemAutomationPeer : AutomationPeer, IGridItemProvider, ITableItemProvider, IInvokeProvider, IScrollItemProvider, ISelectionItemProvider, IValueProvider, IVirtualizedItemProvider
{
	private WeakReference _item;

	private DataGridColumn _column;

	/// <summary>Gets the ordinal number of the column that contains the cell or item.</summary>
	/// <returns>A zero-based ordinal number that identifies the column containing the cell or item.</returns>
	int IGridItemProvider.Column => OwningDataGrid.Columns.IndexOf(_column);

	/// <summary>Gets the number of columns spanned by a cell or item.</summary>
	/// <returns>The number of columns spanned. </returns>
	int IGridItemProvider.ColumnSpan => 1;

	/// <summary>Gets a UI Automation provider that implements <see cref="T:System.Windows.Automation.Provider.IGridProvider" /> and represents the container of the cell or item.</summary>
	/// <returns>A UI Automation provider that implements the <see cref="T:System.Windows.Automation.GridPattern" /> and represents the cell or item container. </returns>
	IRawElementProviderSimple IGridItemProvider.ContainingGrid => ContainingGrid;

	/// <summary>Gets the ordinal number of the row that contains the cell or item.</summary>
	/// <returns>A zero-based ordinal number that identifies the row containing the cell or item. </returns>
	int IGridItemProvider.Row => OwningDataGrid.Items.IndexOf(Item);

	/// <summary>Gets the number of rows spanned by a cell or item.</summary>
	/// <returns>The number of rows spanned. </returns>
	int IGridItemProvider.RowSpan => 1;

	/// <summary>Gets a value that indicates whether an item is selected. </summary>
	/// <returns>true if the element is selected; otherwise, false.</returns>
	bool ISelectionItemProvider.IsSelected => OwningDataGrid.SelectedCellsInternal.Contains(new DataGridCellInfo(Item, _column));

	/// <summary>Gets the UI Automation provider that implements <see cref="T:System.Windows.Automation.Provider.ISelectionProvider" /> and acts as the container for the calling object.</summary>
	/// <returns>The provider that supports <see cref="T:System.Windows.Automation.Provider.ISelectionProvider" />. </returns>
	IRawElementProviderSimple ISelectionItemProvider.SelectionContainer => ContainingGrid;

	/// <summary>Gets a value that specifies whether the value of a control is read-only. </summary>
	/// <returns>true if the value is read-only; false if it can be modified. </returns>
	bool IValueProvider.IsReadOnly => _column.IsReadOnly;

	/// <summary>Gets the value of the control.</summary>
	/// <returns>The value of the control as a string. </returns>
	string IValueProvider.Value
	{
		get
		{
			if (OwningDataGrid != null)
			{
				return OwningDataGrid.GetCellAutomationValue(Item, _column);
			}
			return null;
		}
	}

	private bool IsCellSelectionUnit
	{
		get
		{
			if (OwningDataGrid != null)
			{
				if (OwningDataGrid.SelectionUnit != 0)
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
			object item = Item;
			if (item != CollectionView.NewItemPlaceholder)
			{
				return item == DataGrid.NewItemPlaceholder;
			}
			return true;
		}
	}

	private DataGrid OwningDataGrid => _column.DataGridOwner;

	private DataGridCell OwningCell => OwningDataGrid?.TryFindCell(Item, _column);

	internal DataGridCellAutomationPeer OwningCellPeer
	{
		get
		{
			DataGridCellAutomationPeer dataGridCellAutomationPeer = null;
			DataGridCell owningCell = OwningCell;
			if (owningCell != null)
			{
				dataGridCellAutomationPeer = UIElementAutomationPeer.CreatePeerForElement(owningCell) as DataGridCellAutomationPeer;
				dataGridCellAutomationPeer.EventsSource = this;
			}
			return dataGridCellAutomationPeer;
		}
	}

	private IRawElementProviderSimple ContainingGrid
	{
		get
		{
			AutomationPeer automationPeer = UIElementAutomationPeer.CreatePeerForElement(OwningDataGrid);
			if (automationPeer != null)
			{
				return ProviderFromPeer(automationPeer);
			}
			return null;
		}
	}

	internal DataGridColumn Column => _column;

	internal object Item
	{
		get
		{
			if (_item != null)
			{
				return _item.Target;
			}
			return null;
		}
	}

	private DataGridItemAutomationPeer OwningItemPeer
	{
		get
		{
			if (OwningDataGrid != null && UIElementAutomationPeer.CreatePeerForElement(OwningDataGrid) is DataGridAutomationPeer dataGridAutomationPeer)
			{
				return dataGridAutomationPeer.GetExistingPeerByItem(Item, checkInWeakRefStorage: true) as DataGridItemAutomationPeer;
			}
			return null;
		}
	}

	internal override bool AncestorsInvalid
	{
		get
		{
			return base.AncestorsInvalid;
		}
		set
		{
			base.AncestorsInvalid = value;
			if (!value)
			{
				AutomationPeer owningCellPeer = OwningCellPeer;
				if (owningCellPeer != null)
				{
					owningCellPeer.AncestorsInvalid = false;
				}
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Automation.Peers.DataGridCellItemAutomationPeer" /> class. </summary>
	/// <param name="item">The element that is associated with this automation peer.</param>
	/// <param name="dataGridColumn">The <see cref="T:System.Windows.Controls.DataGrid" /> column that <paramref name="item" /> is in. </param>
	public DataGridCellItemAutomationPeer(object item, DataGridColumn dataGridColumn)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		if (dataGridColumn == null)
		{
			throw new ArgumentNullException("dataGridColumn");
		}
		_item = new WeakReference(item);
		_column = dataGridColumn;
	}

	protected override string GetAcceleratorKeyCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.GetAcceleratorKey();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	protected override string GetAccessKeyCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.GetAccessKey();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	protected override AutomationControlType GetAutomationControlTypeCore()
	{
		return AutomationControlType.Custom;
	}

	protected override string GetAutomationIdCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.GetAutomationId();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	protected override Rect GetBoundingRectangleCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.GetBoundingRectangle();
		}
		ThrowElementNotAvailableException();
		return default(Rect);
	}

	protected override List<AutomationPeer> GetChildrenCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			owningCellPeer.ForceEnsureChildren();
			return owningCellPeer.GetChildren();
		}
		return null;
	}

	protected override string GetClassNameCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.GetClassName();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	protected override Point GetClickablePointCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.GetClickablePoint();
		}
		ThrowElementNotAvailableException();
		return new Point(double.NaN, double.NaN);
	}

	protected override string GetHelpTextCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.GetHelpText();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	protected override string GetItemStatusCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.GetItemStatus();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	protected override string GetItemTypeCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.GetItemType();
		}
		ThrowElementNotAvailableException();
		return string.Empty;
	}

	protected override AutomationPeer GetLabeledByCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.GetLabeledBy();
		}
		ThrowElementNotAvailableException();
		return null;
	}

	protected override string GetLocalizedControlTypeCore()
	{
		if (!AccessibilitySwitches.UseNetFx47CompatibleAccessibilityFeatures)
		{
			return SR.DataGridCellItemAutomationPeer_LocalizedControlType;
		}
		return base.GetLocalizedControlTypeCore();
	}

	protected override AutomationLiveSetting GetLiveSettingCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		AutomationLiveSetting result = AutomationLiveSetting.Off;
		if (owningCellPeer != null)
		{
			result = owningCellPeer.GetLiveSetting();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
		return result;
	}

	protected override string GetNameCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		string text = null;
		if (owningCellPeer != null)
		{
			text = owningCellPeer.GetName();
		}
		if (string.IsNullOrEmpty(text))
		{
			text = SR.Format(SR.DataGridCellItemAutomationPeer_NameCoreFormat, Item, _column.DisplayIndex);
		}
		return text;
	}

	protected override AutomationOrientation GetOrientationCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.GetOrientation();
		}
		ThrowElementNotAvailableException();
		return AutomationOrientation.None;
	}

	/// <summary>Returns the object that supports the specified control pattern of the element that is associated with this automation peer.</summary>
	/// <returns>The current <see cref="T:System.Windows.Automation.Peers.DataGridCellItemAutomationPeer" /> object, if <paramref name="patternInterface" /> is a supported value; otherwise, null. For more information, see Remarks.</returns>
	/// <param name="patternInterface">An enumeration that specifies the control pattern.</param>
	public override object GetPattern(PatternInterface patternInterface)
	{
		switch (patternInterface)
		{
		case PatternInterface.Invoke:
			if (!OwningDataGrid.IsReadOnly && !_column.IsReadOnly)
			{
				return this;
			}
			break;
		case PatternInterface.Value:
			if (!IsNewItemPlaceholder)
			{
				return this;
			}
			break;
		case PatternInterface.SelectionItem:
			if (IsCellSelectionUnit)
			{
				return this;
			}
			break;
		case PatternInterface.ScrollItem:
		case PatternInterface.GridItem:
		case PatternInterface.TableItem:
			return this;
		case PatternInterface.VirtualizedItem:
			if (VirtualizedItemPatternIdentifiers.Pattern != null)
			{
				if (OwningCellPeer == null)
				{
					return this;
				}
				if (OwningItemPeer != null && !IsItemInAutomationTree())
				{
					return this;
				}
				if (OwningItemPeer == null)
				{
					return this;
				}
			}
			break;
		}
		return null;
	}

	protected override int GetPositionInSetCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		int result = -1;
		if (owningCellPeer != null)
		{
			result = owningCellPeer.GetPositionInSet();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
		return result;
	}

	protected override int GetSizeOfSetCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		int result = -1;
		if (owningCellPeer != null)
		{
			result = owningCellPeer.GetSizeOfSet();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
		return result;
	}

	protected override AutomationHeadingLevel GetHeadingLevelCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		AutomationHeadingLevel result = AutomationHeadingLevel.None;
		if (owningCellPeer != null)
		{
			result = owningCellPeer.GetHeadingLevel();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
		return result;
	}

	internal override Rect GetVisibleBoundingRectCore()
	{
		return OwningCellPeer?.GetVisibleBoundingRectCore() ?? GetBoundingRectangle();
	}

	protected override bool HasKeyboardFocusCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.HasKeyboardFocus();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	protected override bool IsContentElementCore()
	{
		return OwningCellPeer?.IsContentElement() ?? true;
	}

	protected override bool IsControlElementCore()
	{
		return OwningCellPeer?.IsControlElement() ?? true;
	}

	protected override bool IsDialogCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.IsDialog();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	protected override bool IsEnabledCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.IsEnabled();
		}
		ThrowElementNotAvailableException();
		return true;
	}

	protected override bool IsKeyboardFocusableCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.IsKeyboardFocusable();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	protected override bool IsOffscreenCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.IsOffscreen();
		}
		ThrowElementNotAvailableException();
		return true;
	}

	protected override bool IsPasswordCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.IsPassword();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	protected override bool IsRequiredForFormCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			return owningCellPeer.IsRequiredForForm();
		}
		ThrowElementNotAvailableException();
		return false;
	}

	protected override void SetFocusCore()
	{
		AutomationPeer owningCellPeer = OwningCellPeer;
		if (owningCellPeer != null)
		{
			owningCellPeer.SetFocus();
		}
		else
		{
			ThrowElementNotAvailableException();
		}
	}

	internal override bool IsDataItemAutomationPeer()
	{
		return true;
	}

	internal override void AddToParentProxyWeakRefCache()
	{
		OwningItemPeer?.AddProxyToWeakRefStorage(base.ElementProxyWeakReference, this);
	}

	/// <summary>Retrieves a collection of UI Automation providers representing all the column headers associated with a table item or cell.</summary>
	/// <returns>A collection of UI Automation providers. </returns>
	IRawElementProviderSimple[] ITableItemProvider.GetColumnHeaderItems()
	{
		if (OwningDataGrid != null && (OwningDataGrid.HeadersVisibility & DataGridHeadersVisibility.Column) == DataGridHeadersVisibility.Column && OwningDataGrid.ColumnHeadersPresenter != null && UIElementAutomationPeer.CreatePeerForElement(OwningDataGrid.ColumnHeadersPresenter) is DataGridColumnHeadersPresenterAutomationPeer dataGridColumnHeadersPresenterAutomationPeer)
		{
			AutomationPeer automationPeer = dataGridColumnHeadersPresenterAutomationPeer.FindOrCreateItemAutomationPeer(_column);
			if (automationPeer != null)
			{
				return new List<IRawElementProviderSimple>(1) { ProviderFromPeer(automationPeer) }.ToArray();
			}
		}
		return null;
	}

	/// <summary>Retrieves a collection of UI Automation providers representing all the row headers associated with a table item or cell.</summary>
	/// <returns>A collection of UI Automation providers. </returns>
	IRawElementProviderSimple[] ITableItemProvider.GetRowHeaderItems()
	{
		if (OwningDataGrid != null && (OwningDataGrid.HeadersVisibility & DataGridHeadersVisibility.Row) == DataGridHeadersVisibility.Row && (UIElementAutomationPeer.CreatePeerForElement(OwningDataGrid) as DataGridAutomationPeer).FindOrCreateItemAutomationPeer(Item) is DataGridItemAutomationPeer { RowHeaderAutomationPeer: { } rowHeaderAutomationPeer })
		{
			return new List<IRawElementProviderSimple>(1) { ProviderFromPeer(rowHeaderAutomationPeer) }.ToArray();
		}
		return null;
	}

	/// <summary>Sends a request to activate a control and initiate its single, unambiguous action.</summary>
	void IInvokeProvider.Invoke()
	{
		if (OwningDataGrid.IsReadOnly || _column.IsReadOnly)
		{
			return;
		}
		EnsureEnabled();
		bool flag = false;
		if (OwningCell == null)
		{
			OwningDataGrid.ScrollIntoView(Item, _column);
		}
		DataGridCell owningCell = OwningCell;
		if (owningCell != null)
		{
			if (!owningCell.IsEditing)
			{
				if (!owningCell.IsKeyboardFocusWithin)
				{
					owningCell.Focus();
				}
				OwningDataGrid.HandleSelectionForCellInput(owningCell, startDragging: false, allowsExtendSelect: false, allowsMinimalSelect: false);
				flag = OwningDataGrid.BeginEdit();
			}
			else
			{
				flag = true;
			}
		}
		if (flag || IsNewItemPlaceholder)
		{
			return;
		}
		throw new InvalidOperationException(SR.DataGrid_AutomationInvokeFailed);
	}

	/// <summary>Scrolls the content area of a container object in order to display the control within the visible region (viewport) of the container.</summary>
	void IScrollItemProvider.ScrollIntoView()
	{
		OwningDataGrid.ScrollIntoView(Item, _column);
	}

	/// <summary>Adds the current element to the collection of selected items.</summary>
	void ISelectionItemProvider.AddToSelection()
	{
		if (!IsCellSelectionUnit)
		{
			throw new InvalidOperationException(SR.DataGrid_CannotSelectCell);
		}
		DataGridCellInfo cell = new DataGridCellInfo(Item, _column);
		if (!OwningDataGrid.SelectedCellsInternal.Contains(cell))
		{
			EnsureEnabled();
			if (OwningDataGrid.SelectionMode == DataGridSelectionMode.Single && OwningDataGrid.SelectedCells.Count > 0)
			{
				throw new InvalidOperationException();
			}
			OwningDataGrid.SelectedCellsInternal.Add(cell);
		}
	}

	/// <summary>Removes the current element from the collection of selected items.</summary>
	void ISelectionItemProvider.RemoveFromSelection()
	{
		if (!IsCellSelectionUnit)
		{
			throw new InvalidOperationException(SR.DataGrid_CannotSelectCell);
		}
		EnsureEnabled();
		DataGridCellInfo cell = new DataGridCellInfo(Item, _column);
		if (OwningDataGrid.SelectedCellsInternal.Contains(cell))
		{
			OwningDataGrid.SelectedCellsInternal.Remove(cell);
		}
	}

	/// <summary>Deselects any selected items and then selects the current element.</summary>
	void ISelectionItemProvider.Select()
	{
		if (!IsCellSelectionUnit)
		{
			throw new InvalidOperationException(SR.DataGrid_CannotSelectCell);
		}
		EnsureEnabled();
		DataGridCellInfo currentCellInfo = new DataGridCellInfo(Item, _column);
		OwningDataGrid.SelectOnlyThisCell(currentCellInfo);
	}

	/// <summary>Sets the value of a control.</summary>
	/// <param name="value">The value to set. The provider is responsible for converting the value to the appropriate data type.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Controls.DataGridCell" /> object that is associated with this <see cref="T:System.Windows.Automation.Peers.DataGridCellItemAutomationPeer" /> object is read-only.</exception>
	void IValueProvider.SetValue(string value)
	{
		if (_column.IsReadOnly)
		{
			throw new InvalidOperationException(SR.DataGrid_ColumnIsReadOnly);
		}
		if (OwningDataGrid != null)
		{
			OwningDataGrid.SetCellAutomationValue(Item, _column, value);
		}
	}

	/// <summary>Makes the virtual item fully accessible as a UI Automation element.</summary>
	void IVirtualizedItemProvider.Realize()
	{
		OwningDataGrid.ScrollIntoView(Item, _column);
	}

	private void EnsureEnabled()
	{
		if (!OwningDataGrid.IsEnabled)
		{
			throw new ElementNotEnabledException();
		}
	}

	private void ThrowElementNotAvailableException()
	{
		if (VirtualizedItemPatternIdentifiers.Pattern != null && !IsItemInAutomationTree())
		{
			throw new ElementNotAvailableException(SR.VirtualizedElement);
		}
	}

	private bool IsItemInAutomationTree()
	{
		AutomationPeer parent = GetParent();
		if (base.Index != -1 && parent != null && parent.Children != null && base.Index < parent.Children.Count && parent.Children[base.Index] == this)
		{
			return true;
		}
		return false;
	}
}
