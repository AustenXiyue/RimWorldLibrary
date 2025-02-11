using System.Collections.ObjectModel;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Controls;

/// <summary>Represents a cell of a <see cref="T:System.Windows.Controls.DataGrid" /> control.</summary>
public class DataGridCell : ContentControl, IProvideDataGridColumn
{
	private static readonly bool IsDataGridKeyboardSortDisabled;

	private static readonly bool OptOutOfGridColumnResizeUsingKeyboard;

	private static readonly DependencyPropertyKey ColumnPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridCell.Column" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridCell.Column" /> dependency property.</returns>
	public static readonly DependencyProperty ColumnProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridCell.IsEditing" /> dependency property.</summary>
	/// <returns>Identifier for the <see cref="P:System.Windows.Controls.DataGridCell.IsEditing" /> dependency property.</returns>
	public static readonly DependencyProperty IsEditingProperty;

	private static readonly DependencyPropertyKey IsReadOnlyPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridCell.IsReadOnly" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridCell.IsReadOnly" /> dependency property.</returns>
	public static readonly DependencyProperty IsReadOnlyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridCell.IsSelected" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridCell.IsSelected" /> dependency property.</returns>
	public static readonly DependencyProperty IsSelectedProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.DataGridCell.Selected" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.DataGridCell.Selected" /> routed event.</returns>
	public static readonly RoutedEvent SelectedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.DataGridCell.Unselected" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.DataGridCell.Unselected" /> routed event.</returns>
	public static readonly RoutedEvent UnselectedEvent;

	private DataGridRow _owner;

	private ContainerTracking<DataGridCell> _tracker;

	private bool _syncingIsSelected;

	private const double ColumnWidthStepSize = 10.0;

	private const ModifierKeys ModifierMask = ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Windows;

	internal ContainerTracking<DataGridCell> Tracker => _tracker;

	/// <summary>Gets or sets the column that the cell is in.</summary>
	/// <returns>The column that the cell is in. </returns>
	public DataGridColumn Column
	{
		get
		{
			return (DataGridColumn)GetValue(ColumnProperty);
		}
		internal set
		{
			SetValue(ColumnPropertyKey, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the cell is in edit mode.</summary>
	/// <returns>true if the cell is in edit mode; otherwise, false. The registered default is false. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool IsEditing
	{
		get
		{
			return (bool)GetValue(IsEditingProperty);
		}
		set
		{
			SetValue(IsEditingProperty, value);
		}
	}

	private bool IsCurrent
	{
		get
		{
			DataGridRow rowOwner = RowOwner;
			DataGridColumn column = Column;
			if (rowOwner != null && column != null)
			{
				DataGrid dataGridOwner = rowOwner.DataGridOwner;
				if (dataGridOwner != null)
				{
					return dataGridOwner.IsCurrent(rowOwner, column);
				}
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether the cell can be put in edit mode.</summary>
	/// <returns>true if the cell cannot be put in edit mode; otherwise, false. The registered default is false. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />. </returns>
	public bool IsReadOnly => (bool)GetValue(IsReadOnlyProperty);

	internal FrameworkElement EditingElement => base.Content as FrameworkElement;

	/// <summary>Gets or sets a value that indicates whether the cell is selected.</summary>
	/// <returns>true if the cell is selected; otherwise, false. The registered default is false. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool IsSelected
	{
		get
		{
			return (bool)GetValue(IsSelectedProperty);
		}
		set
		{
			SetValue(IsSelectedProperty, value);
		}
	}

	internal DataGrid DataGridOwner
	{
		get
		{
			if (_owner != null)
			{
				DataGrid dataGrid = _owner.DataGridOwner;
				if (dataGrid == null)
				{
					dataGrid = ItemsControl.ItemsControlFromItemContainer(_owner) as DataGrid;
				}
				return dataGrid;
			}
			return null;
		}
	}

	private Panel ParentPanel => base.VisualParent as Panel;

	internal DataGridRow RowOwner => _owner;

	internal object RowDataItem
	{
		get
		{
			DataGridRow rowOwner = RowOwner;
			if (rowOwner != null)
			{
				return rowOwner.Item;
			}
			return base.DataContext;
		}
	}

	private DataGridCellsPresenter CellsPresenter => ItemsControl.ItemsControlFromItemContainer(this) as DataGridCellsPresenter;

	private bool NeedsVisualTree
	{
		get
		{
			if (base.ContentTemplate == null)
			{
				return base.ContentTemplateSelector == null;
			}
			return false;
		}
	}

	/// <summary>Occurs when the cell is selected.</summary>
	public event RoutedEventHandler Selected
	{
		add
		{
			AddHandler(SelectedEvent, value);
		}
		remove
		{
			RemoveHandler(SelectedEvent, value);
		}
	}

	/// <summary>Occurs when the cell selection is cleared.</summary>
	public event RoutedEventHandler Unselected
	{
		add
		{
			AddHandler(UnselectedEvent, value);
		}
		remove
		{
			RemoveHandler(UnselectedEvent, value);
		}
	}

	static DataGridCell()
	{
		ColumnPropertyKey = DependencyProperty.RegisterReadOnly("Column", typeof(DataGridColumn), typeof(DataGridCell), new FrameworkPropertyMetadata(null, OnColumnChanged));
		ColumnProperty = ColumnPropertyKey.DependencyProperty;
		IsEditingProperty = DependencyProperty.Register("IsEditing", typeof(bool), typeof(DataGridCell), new FrameworkPropertyMetadata(false, OnIsEditingChanged));
		IsReadOnlyPropertyKey = DependencyProperty.RegisterReadOnly("IsReadOnly", typeof(bool), typeof(DataGridCell), new FrameworkPropertyMetadata(false, OnNotifyIsReadOnlyChanged, OnCoerceIsReadOnly));
		IsReadOnlyProperty = IsReadOnlyPropertyKey.DependencyProperty;
		IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(DataGridCell), new FrameworkPropertyMetadata(false, OnIsSelectedChanged));
		SelectedEvent = EventManager.RegisterRoutedEvent("Selected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DataGridCell));
		UnselectedEvent = EventManager.RegisterRoutedEvent("Unselected", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(DataGridCell));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridCell), new FrameworkPropertyMetadata(typeof(DataGridCell)));
		FrameworkElement.StyleProperty.OverrideMetadata(typeof(DataGridCell), new FrameworkPropertyMetadata(null, OnNotifyPropertyChanged, OnCoerceStyle));
		UIElement.ClipProperty.OverrideMetadata(typeof(DataGridCell), new FrameworkPropertyMetadata(null, OnCoerceClip));
		KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(DataGridCell), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
		AutomationProperties.IsOffscreenBehaviorProperty.OverrideMetadata(typeof(DataGridCell), new FrameworkPropertyMetadata(IsOffscreenBehavior.FromClip));
		UIElement.SnapsToDevicePixelsProperty.OverrideMetadata(typeof(DataGridCell), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange));
		EventManager.RegisterClassHandler(typeof(DataGridCell), UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnAnyMouseLeftButtonDownThunk), handledEventsToo: true);
		UIElement.IsMouseOverPropertyKey.OverrideMetadata(typeof(DataGridCell), new UIPropertyMetadata(Control.OnVisualStatePropertyChanged));
		EventManager.RegisterClassHandler(typeof(DataGridCell), UIElement.LostFocusEvent, new RoutedEventHandler(OnAnyLostFocus), handledEventsToo: true);
		EventManager.RegisterClassHandler(typeof(DataGridCell), UIElement.GotFocusEvent, new RoutedEventHandler(OnAnyGotFocus), handledEventsToo: true);
		AppContext.TryGetSwitch("System.Windows.Controls.DisableDataGridKeyboardSort", out IsDataGridKeyboardSortDisabled);
		AppContext.TryGetSwitch("System.Windows.Controls.OptOutOfGridColumnResizeUsingKeyboard", out OptOutOfGridColumnResizeUsingKeyboard);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridCell" /> class.</summary>
	public DataGridCell()
	{
		_tracker = new ContainerTracking<DataGridCell>(this);
	}

	/// <summary>Returns the automation peer for this <see cref="T:System.Windows.Controls.DataGridCell" />.</summary>
	/// <returns>The automation peer for this <see cref="T:System.Windows.Controls.DataGridCell" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DataGridCellAutomationPeer(this);
	}

	internal void PrepareCell(object item, ItemsControl cellsPresenter, DataGridRow ownerRow)
	{
		PrepareCell(item, ownerRow, cellsPresenter.ItemContainerGenerator.IndexFromContainer(this));
	}

	internal void PrepareCell(object item, DataGridRow ownerRow, int index)
	{
		_owner = ownerRow;
		DataGrid dataGridOwner = _owner.DataGridOwner;
		if (dataGridOwner != null)
		{
			if (index >= 0 && index < dataGridOwner.Columns.Count)
			{
				DataGridColumn dataGridColumn2 = (Column = dataGridOwner.Columns[index]);
				base.TabIndex = dataGridColumn2.DisplayIndex;
			}
			if (IsEditing)
			{
				IsEditing = false;
			}
			else if (!(base.Content is FrameworkElement))
			{
				BuildVisualTree();
				if (!NeedsVisualTree)
				{
					base.Content = item;
				}
			}
			bool isSelected = dataGridOwner.SelectedCellsInternal.Contains(this);
			SyncIsSelected(isSelected);
		}
		DataGridHelper.TransferProperty(this, FrameworkElement.StyleProperty);
		DataGridHelper.TransferProperty(this, IsReadOnlyProperty);
		CoerceValue(UIElement.ClipProperty);
	}

	internal void ClearCell(DataGridRow ownerRow)
	{
		_owner = null;
	}

	private static void OnColumnChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is DataGridCell dataGridCell)
		{
			dataGridCell.OnColumnChanged((DataGridColumn)e.OldValue, (DataGridColumn)e.NewValue);
		}
	}

	/// <summary>Called when the cell's <see cref="P:System.Windows.Controls.DataGridCell.Column" /> property changes. </summary>
	/// <param name="oldColumn">The old column definition.</param>
	/// <param name="newColumn">The new column definition.</param>
	protected virtual void OnColumnChanged(DataGridColumn oldColumn, DataGridColumn newColumn)
	{
		base.Content = null;
		DataGridHelper.TransferProperty(this, FrameworkElement.StyleProperty);
		DataGridHelper.TransferProperty(this, IsReadOnlyProperty);
	}

	private static void OnNotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGridCell)d).NotifyPropertyChanged(d, string.Empty, e, DataGridNotificationTarget.Cells);
	}

	private static void OnNotifyIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGridCell dataGridCell = (DataGridCell)d;
		DataGrid dataGridOwner = dataGridCell.DataGridOwner;
		if ((bool)e.NewValue)
		{
			dataGridOwner?.CancelEdit(dataGridCell);
		}
		CommandManager.InvalidateRequerySuggested();
		dataGridCell.NotifyPropertyChanged(d, string.Empty, e, DataGridNotificationTarget.Cells);
	}

	internal void NotifyPropertyChanged(DependencyObject d, string propertyName, DependencyPropertyChangedEventArgs e, DataGridNotificationTarget target)
	{
		DataGridColumn dataGridColumn = d as DataGridColumn;
		if (dataGridColumn != null && dataGridColumn != Column)
		{
			return;
		}
		if (DataGridHelper.ShouldNotifyCells(target))
		{
			if (e.Property == DataGridColumn.WidthProperty)
			{
				DataGridHelper.OnColumnWidthChanged(this, e);
			}
			else if (e.Property == DataGrid.CellStyleProperty || e.Property == DataGridColumn.CellStyleProperty || e.Property == FrameworkElement.StyleProperty)
			{
				DataGridHelper.TransferProperty(this, FrameworkElement.StyleProperty);
			}
			else if (e.Property == DataGrid.IsReadOnlyProperty || e.Property == DataGridColumn.IsReadOnlyProperty || e.Property == IsReadOnlyProperty)
			{
				DataGridHelper.TransferProperty(this, IsReadOnlyProperty);
			}
			else if (e.Property == DataGridColumn.DisplayIndexProperty)
			{
				base.TabIndex = dataGridColumn.DisplayIndex;
			}
			else if (e.Property == UIElement.IsKeyboardFocusWithinProperty)
			{
				UpdateVisualState();
			}
		}
		if (DataGridHelper.ShouldRefreshCellContent(target) && dataGridColumn != null && NeedsVisualTree)
		{
			if (!string.IsNullOrEmpty(propertyName))
			{
				dataGridColumn.RefreshCellContent(this, propertyName);
			}
			else if (e.Property != null)
			{
				dataGridColumn.RefreshCellContent(this, e.Property.Name);
			}
		}
	}

	private static object OnCoerceStyle(DependencyObject d, object baseValue)
	{
		DataGridCell dataGridCell = d as DataGridCell;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridCell, baseValue, FrameworkElement.StyleProperty, dataGridCell.Column, DataGridColumn.CellStyleProperty, dataGridCell.DataGridOwner, DataGrid.CellStyleProperty);
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (DataGridOwner != null)
		{
			if (base.IsMouseOver)
			{
				VisualStates.GoToState(this, useTransitions, "MouseOver", "Normal");
			}
			else
			{
				VisualStateManager.GoToState(this, "Normal", useTransitions);
			}
			if (IsSelected)
			{
				VisualStates.GoToState(this, useTransitions, "Selected", "Unselected");
			}
			else
			{
				VisualStates.GoToState(this, useTransitions, "Unselected");
			}
			if (DataGridOwner.IsKeyboardFocusWithin)
			{
				VisualStates.GoToState(this, useTransitions, "Focused", "Unfocused");
			}
			else
			{
				VisualStateManager.GoToState(this, "Unfocused", useTransitions);
			}
			if (IsCurrent)
			{
				VisualStates.GoToState(this, useTransitions, "Current", "Regular");
			}
			else
			{
				VisualStateManager.GoToState(this, "Regular", useTransitions);
			}
			if (IsEditing)
			{
				VisualStates.GoToState(this, useTransitions, "Editing", "Display");
			}
			else
			{
				VisualStateManager.GoToState(this, "Display", useTransitions);
			}
			base.ChangeVisualState(useTransitions);
		}
	}

	internal void BuildVisualTree()
	{
		if (!NeedsVisualTree)
		{
			return;
		}
		DataGridColumn column = Column;
		if (column == null)
		{
			return;
		}
		DataGridRow rowOwner = RowOwner;
		if (rowOwner != null)
		{
			BindingGroup bindingGroup = rowOwner.BindingGroup;
			if (bindingGroup != null)
			{
				RemoveBindingExpressions(bindingGroup, base.Content as DependencyObject);
			}
		}
		FrameworkElement frameworkElement = column.BuildVisualTree(IsEditing, RowDataItem, this);
		if (base.Content is FrameworkElement frameworkElement2 && frameworkElement2 != frameworkElement)
		{
			if (!(frameworkElement2 is ContentPresenter contentPresenter))
			{
				frameworkElement2.SetValue(FrameworkElement.DataContextProperty, BindingExpressionBase.DisconnectedItem);
			}
			else
			{
				contentPresenter.Content = BindingExpressionBase.DisconnectedItem;
			}
		}
		base.Content = frameworkElement;
	}

	private void RemoveBindingExpressions(BindingGroup bindingGroup, DependencyObject element)
	{
		if (element == null)
		{
			return;
		}
		Collection<BindingExpressionBase> bindingExpressions = bindingGroup.BindingExpressions;
		BindingExpressionBase[] array = new BindingExpressionBase[bindingExpressions.Count];
		bindingExpressions.CopyTo(array, 0);
		for (int i = 0; i < array.Length; i++)
		{
			if (DataGridHelper.BindingExpressionBelongsToElement(array[i], this))
			{
				bindingExpressions.Remove(array[i]);
			}
		}
	}

	private static void OnIsEditingChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		((DataGridCell)sender).OnIsEditingChanged((bool)e.NewValue);
	}

	/// <summary>Called when the <see cref="P:System.Windows.Controls.DataGridCell.IsEditing" /> property changes.</summary>
	/// <param name="isEditing">The new value of the <see cref="P:System.Windows.Controls.DataGridCell.IsEditing" /> property.</param>
	protected virtual void OnIsEditingChanged(bool isEditing)
	{
		if (base.IsKeyboardFocusWithin && !base.IsKeyboardFocused)
		{
			Focus();
		}
		BuildVisualTree();
		UpdateVisualState();
	}

	internal void NotifyCurrentCellContainerChanged()
	{
		UpdateVisualState();
	}

	private static object OnCoerceIsReadOnly(DependencyObject d, object baseValue)
	{
		DataGridCell obj = d as DataGridCell;
		DataGridColumn column = obj.Column;
		DataGrid dataGridOwner = obj.DataGridOwner;
		return DataGridHelper.GetCoercedTransferPropertyValue(column, column.IsReadOnly, DataGridColumn.IsReadOnlyProperty, dataGridOwner, DataGrid.IsReadOnlyProperty);
	}

	private static void OnAnyLostFocus(object sender, RoutedEventArgs e)
	{
		DataGridCell dataGridCell = DataGridHelper.FindVisualParent<DataGridCell>(e.OriginalSource as UIElement);
		if (dataGridCell != null && dataGridCell == sender)
		{
			DataGrid dataGridOwner = dataGridCell.DataGridOwner;
			if (dataGridOwner != null && !dataGridCell.IsKeyboardFocusWithin && dataGridOwner.FocusedCell == dataGridCell)
			{
				dataGridOwner.FocusedCell = null;
			}
		}
	}

	private static void OnAnyGotFocus(object sender, RoutedEventArgs e)
	{
		DataGridCell dataGridCell = DataGridHelper.FindVisualParent<DataGridCell>(e.OriginalSource as UIElement);
		if (dataGridCell != null && dataGridCell == sender)
		{
			DataGrid dataGridOwner = dataGridCell.DataGridOwner;
			if (dataGridOwner != null)
			{
				dataGridOwner.FocusedCell = dataGridCell;
			}
		}
	}

	internal void BeginEdit(RoutedEventArgs e)
	{
		IsEditing = true;
		Column?.BeginEdit(base.Content as FrameworkElement, e);
		RaisePreparingCellForEdit(e);
	}

	internal void CancelEdit()
	{
		Column?.CancelEdit(base.Content as FrameworkElement);
		IsEditing = false;
	}

	internal bool CommitEdit()
	{
		bool flag = true;
		DataGridColumn column = Column;
		if (column != null)
		{
			flag = column.CommitEdit(base.Content as FrameworkElement);
		}
		if (flag)
		{
			IsEditing = false;
		}
		return flag;
	}

	private void RaisePreparingCellForEdit(RoutedEventArgs editingEventArgs)
	{
		DataGrid dataGridOwner = DataGridOwner;
		if (dataGridOwner != null)
		{
			FrameworkElement editingElement = EditingElement;
			DataGridPreparingCellForEditEventArgs e = new DataGridPreparingCellForEditEventArgs(Column, RowOwner, editingEventArgs, editingElement);
			dataGridOwner.OnPreparingCellForEdit(e);
		}
	}

	private static void OnIsSelectedChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		DataGridCell dataGridCell = (DataGridCell)sender;
		bool isSelected = (bool)e.NewValue;
		if (!dataGridCell._syncingIsSelected)
		{
			dataGridCell.DataGridOwner?.CellIsSelectedChanged(dataGridCell, isSelected);
		}
		dataGridCell.RaiseSelectionChangedEvent(isSelected);
		dataGridCell.UpdateVisualState();
	}

	internal void SyncIsSelected(bool isSelected)
	{
		bool syncingIsSelected = _syncingIsSelected;
		_syncingIsSelected = true;
		try
		{
			IsSelected = isSelected;
		}
		finally
		{
			_syncingIsSelected = syncingIsSelected;
		}
	}

	private void RaiseSelectionChangedEvent(bool isSelected)
	{
		if (isSelected)
		{
			OnSelected(new RoutedEventArgs(SelectedEvent, this));
		}
		else
		{
			OnUnselected(new RoutedEventArgs(UnselectedEvent, this));
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGridCell.Selected" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnSelected(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGridCell.Unselected" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnUnselected(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Determines the desired size of the <see cref="T:System.Windows.Controls.DataGridCell" />. </summary>
	/// <returns>The desired size of the <see cref="T:System.Windows.Controls.DataGridCell" />.</returns>
	/// <param name="constraint">The maximum size that the cell can occupy.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		DataGrid dataGridOwner = DataGridOwner;
		bool flag = DataGridHelper.IsGridLineVisible(dataGridOwner, isHorizontal: true);
		bool num = DataGridHelper.IsGridLineVisible(dataGridOwner, isHorizontal: false);
		double num2 = 0.0;
		double num3 = 0.0;
		if (flag)
		{
			num2 = dataGridOwner.HorizontalGridLineThickness;
			constraint = DataGridHelper.SubtractFromSize(constraint, num2, height: true);
		}
		if (num)
		{
			num3 = dataGridOwner.VerticalGridLineThickness;
			constraint = DataGridHelper.SubtractFromSize(constraint, num3, height: false);
		}
		Size result = base.MeasureOverride(constraint);
		if (flag)
		{
			result.Height += num2;
		}
		if (num)
		{
			result.Width += num3;
		}
		return result;
	}

	/// <summary>Determines the final size and placement of the cell content. </summary>
	/// <returns>The final size of the control.</returns>
	/// <param name="arrangeSize">The maximum size that the cell can occupy.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		DataGrid dataGridOwner = DataGridOwner;
		bool flag = DataGridHelper.IsGridLineVisible(dataGridOwner, isHorizontal: true);
		bool num = DataGridHelper.IsGridLineVisible(dataGridOwner, isHorizontal: false);
		double num2 = 0.0;
		double num3 = 0.0;
		if (flag)
		{
			num2 = dataGridOwner.HorizontalGridLineThickness;
			arrangeSize = DataGridHelper.SubtractFromSize(arrangeSize, num2, height: true);
		}
		if (num)
		{
			num3 = dataGridOwner.VerticalGridLineThickness;
			arrangeSize = DataGridHelper.SubtractFromSize(arrangeSize, num3, height: false);
		}
		Size result = base.ArrangeOverride(arrangeSize);
		if (flag)
		{
			result.Height += num2;
		}
		if (num)
		{
			result.Width += num3;
		}
		return result;
	}

	/// <summary>Draws the cell and the right side gridline.</summary>
	/// <param name="drawingContext">The drawing instructions for the cell.</param>
	protected override void OnRender(DrawingContext drawingContext)
	{
		base.OnRender(drawingContext);
		DataGrid dataGridOwner = DataGridOwner;
		if (DataGridHelper.IsGridLineVisible(dataGridOwner, isHorizontal: false))
		{
			double verticalGridLineThickness = DataGridOwner.VerticalGridLineThickness;
			Rect rectangle = new Rect(new Size(verticalGridLineThickness, base.RenderSize.Height));
			rectangle.X = base.RenderSize.Width - verticalGridLineThickness;
			drawingContext.DrawRectangle(DataGridOwner.VerticalGridLinesBrush, null, rectangle);
		}
		if (DataGridHelper.IsGridLineVisible(dataGridOwner, isHorizontal: true))
		{
			double horizontalGridLineThickness = dataGridOwner.HorizontalGridLineThickness;
			Rect rectangle2 = new Rect(new Size(base.RenderSize.Width, horizontalGridLineThickness));
			rectangle2.Y = base.RenderSize.Height - horizontalGridLineThickness;
			drawingContext.DrawRectangle(dataGridOwner.HorizontalGridLinesBrush, null, rectangle2);
		}
	}

	private static void OnAnyMouseLeftButtonDownThunk(object sender, MouseButtonEventArgs e)
	{
		((DataGridCell)sender).OnAnyMouseLeftButtonDown(e);
	}

	private void OnAnyMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		bool isKeyboardFocusWithin = base.IsKeyboardFocusWithin;
		bool flag = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
		if (isKeyboardFocusWithin && !flag && !e.Handled && IsSelected)
		{
			DataGrid dataGridOwner = DataGridOwner;
			if (dataGridOwner != null)
			{
				dataGridOwner.HandleSelectionForCellInput(this, startDragging: false, allowsExtendSelect: true, allowsMinimalSelect: false);
				if (!IsEditing && !IsReadOnly)
				{
					dataGridOwner.BeginEdit(e);
					e.Handled = true;
				}
			}
		}
		else if (!isKeyboardFocusWithin || !IsSelected || flag)
		{
			if (!isKeyboardFocusWithin)
			{
				Focus();
			}
			DataGridOwner?.HandleSelectionForCellInput(this, Mouse.Captured == null, allowsExtendSelect: true, allowsMinimalSelect: true);
			e.Handled = true;
		}
	}

	/// <summary>Reports text composition.</summary>
	/// <param name="e">The data for the event.</param>
	protected override void OnTextInput(TextCompositionEventArgs e)
	{
		SendInputToColumn(e);
	}

	/// <summary>Reports that a key was pressed.</summary>
	/// <param name="e">The data for the event.</param>
	protected override void OnPreviewKeyDown(KeyEventArgs e)
	{
		SendInputToColumn(e);
	}

	/// <summary>Reports that a key was pressed.</summary>
	/// <param name="e">The data for the event.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (!e.Handled)
		{
			if (!OptOutOfGridColumnResizeUsingKeyboard)
			{
				ModifierKeys modifierKeys = Keyboard.Modifiers & (ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Windows);
				if ((e.SystemKey == Key.Right || e.SystemKey == Key.Left) && modifierKeys == ModifierKeys.Alt)
				{
					DataGridLength dataGridLength = ((e.SystemKey != Key.Right) ? new DataGridLength(Column.ActualWidth - 10.0) : new DataGridLength(Column.ActualWidth + 10.0));
					if (Column != null)
					{
						if (Column.CanColumnResize(dataGridLength))
						{
							Column.SetCurrentValueInternal(DataGridColumn.WidthProperty, dataGridLength);
						}
						e.Handled = true;
					}
					return;
				}
			}
			if (!IsDataGridKeyboardSortDisabled && e.Key == Key.F3 && Column != null && Column.CanUserSort)
			{
				Column.DataGridOwner.PerformSort(Column);
				e.Handled = true;
				return;
			}
		}
		SendInputToColumn(e);
	}

	private void SendInputToColumn(InputEventArgs e)
	{
		Column?.OnInput(e);
	}

	private static object OnCoerceClip(DependencyObject d, object baseValue)
	{
		DataGridCell cell = (DataGridCell)d;
		Geometry geometry = baseValue as Geometry;
		Geometry frozenClipForCell = DataGridHelper.GetFrozenClipForCell(cell);
		if (frozenClipForCell != null)
		{
			if (geometry == null)
			{
				return frozenClipForCell;
			}
			geometry = new CombinedGeometry(GeometryCombineMode.Intersect, geometry, frozenClipForCell);
		}
		return geometry;
	}
}
