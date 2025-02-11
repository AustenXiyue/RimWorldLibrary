using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Controls.Primitives;

/// <summary>Used within the template of a <see cref="T:System.Windows.Controls.DataGrid" /> to specify the location in the control's visual tree where the cells are to be added. </summary>
public class DataGridCellsPresenter : ItemsControl
{
	private object _item;

	private ContainerTracking<DataGridCell> _cellTrackingRoot;

	private Panel _internalItemsHost;

	/// <summary>Gets the data item that the row represents. </summary>
	/// <returns>The data item that the row represents. </returns>
	public object Item
	{
		get
		{
			return _item;
		}
		internal set
		{
			if (_item != value)
			{
				object item = _item;
				_item = value;
				OnItemChanged(item, _item);
			}
		}
	}

	internal Panel InternalItemsHost
	{
		get
		{
			return _internalItemsHost;
		}
		set
		{
			_internalItemsHost = value;
		}
	}

	internal DataGrid DataGridOwner => DataGridRowOwner?.DataGridOwner;

	internal DataGridRow DataGridRowOwner => DataGridHelper.FindParent<DataGridRow>(this);

	private ObservableCollection<DataGridColumn> Columns => (DataGridRowOwner?.DataGridOwner)?.Columns;

	internal ContainerTracking<DataGridCell> CellTrackingRoot => _cellTrackingRoot;

	static DataGridCellsPresenter()
	{
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridCellsPresenter), new FrameworkPropertyMetadata(typeof(DataGridCellsPresenter)));
		ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(DataGridCellsPresenter), new FrameworkPropertyMetadata(new ItemsPanelTemplate(new FrameworkElementFactory(typeof(DataGridCellsPanel)))));
		UIElement.FocusableProperty.OverrideMetadata(typeof(DataGridCellsPresenter), new FrameworkPropertyMetadata(false));
		FrameworkElement.HeightProperty.OverrideMetadata(typeof(DataGridCellsPresenter), new FrameworkPropertyMetadata(OnNotifyHeightPropertyChanged, OnCoerceHeight));
		FrameworkElement.MinHeightProperty.OverrideMetadata(typeof(DataGridCellsPresenter), new FrameworkPropertyMetadata(OnNotifyHeightPropertyChanged, OnCoerceMinHeight));
		VirtualizingPanel.IsVirtualizingProperty.OverrideMetadata(typeof(DataGridCellsPresenter), new FrameworkPropertyMetadata(false, OnIsVirtualizingPropertyChanged, OnCoerceIsVirtualizingProperty));
		VirtualizingPanel.VirtualizationModeProperty.OverrideMetadata(typeof(DataGridCellsPresenter), new FrameworkPropertyMetadata(VirtualizationMode.Recycling));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.DataGridCellsPresenter" /> class. </summary>
	public DataGridCellsPresenter()
	{
	}

	public override void OnApplyTemplate()
	{
		if (InternalItemsHost != null && !IsAncestorOf(InternalItemsHost))
		{
			InternalItemsHost = null;
		}
		base.OnApplyTemplate();
		DataGridRow dataGridRowOwner = DataGridRowOwner;
		if (dataGridRowOwner != null)
		{
			dataGridRowOwner.CellsPresenter = this;
			Item = dataGridRowOwner.Item;
		}
		SyncProperties(forcePrepareCells: false);
	}

	internal void SyncProperties(bool forcePrepareCells)
	{
		DataGrid dataGridOwner = DataGridOwner;
		if (dataGridOwner == null)
		{
			return;
		}
		DataGridHelper.TransferProperty(this, FrameworkElement.HeightProperty);
		DataGridHelper.TransferProperty(this, FrameworkElement.MinHeightProperty);
		DataGridHelper.TransferProperty(this, VirtualizingPanel.IsVirtualizingProperty);
		NotifyPropertyChanged(this, new DependencyPropertyChangedEventArgs(DataGrid.CellStyleProperty, null, null), DataGridNotificationTarget.Cells);
		if (!(base.ItemsSource is MultipleCopiesCollection multipleCopiesCollection))
		{
			return;
		}
		ObservableCollection<DataGridColumn> columns = dataGridOwner.Columns;
		int count = columns.Count;
		int count2 = multipleCopiesCollection.Count;
		int num = 0;
		bool flag = false;
		if (count != count2)
		{
			multipleCopiesCollection.SyncToCount(count);
			num = Math.Min(count, count2);
		}
		else if (forcePrepareCells)
		{
			num = count;
		}
		if (InternalItemsHost is DataGridCellsPanel dataGridCellsPanel)
		{
			if (dataGridCellsPanel.HasCorrectRealizedColumns)
			{
				dataGridCellsPanel.InvalidateArrange();
			}
			else
			{
				InvalidateDataGridCellsPanelMeasureAndArrange();
				flag = true;
			}
		}
		DataGridRow dataGridRowOwner = DataGridRowOwner;
		for (int i = 0; i < num; i++)
		{
			DataGridCell dataGridCell = (DataGridCell)base.ItemContainerGenerator.ContainerFromIndex(i);
			if (dataGridCell != null)
			{
				dataGridCell.PrepareCell(dataGridRowOwner.Item, this, dataGridRowOwner);
				if (!flag && !DoubleUtil.AreClose(dataGridCell.ActualWidth, columns[i].Width.DisplayValue))
				{
					InvalidateDataGridCellsPanelMeasureAndArrange();
					flag = true;
				}
			}
		}
		if (flag)
		{
			return;
		}
		for (int j = num; j < count; j++)
		{
			DataGridCell dataGridCell = (DataGridCell)base.ItemContainerGenerator.ContainerFromIndex(j);
			if (dataGridCell != null && !DoubleUtil.AreClose(dataGridCell.ActualWidth, columns[j].Width.DisplayValue))
			{
				InvalidateDataGridCellsPanelMeasureAndArrange();
				break;
			}
		}
	}

	private static object OnCoerceHeight(DependencyObject d, object baseValue)
	{
		DataGridCellsPresenter dataGridCellsPresenter = d as DataGridCellsPresenter;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridCellsPresenter, baseValue, FrameworkElement.HeightProperty, dataGridCellsPresenter.DataGridOwner, DataGrid.RowHeightProperty);
	}

	private static object OnCoerceMinHeight(DependencyObject d, object baseValue)
	{
		DataGridCellsPresenter dataGridCellsPresenter = d as DataGridCellsPresenter;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridCellsPresenter, baseValue, FrameworkElement.MinHeightProperty, dataGridCellsPresenter.DataGridOwner, DataGrid.MinRowHeightProperty);
	}

	/// <summary>Updates the displayed cells when the <see cref="P:System.Windows.Controls.Primitives.DataGridCellsPresenter.Item" /> property value has changed. </summary>
	/// <param name="oldItem">The previous value of the <see cref="P:System.Windows.Controls.Primitives.DataGridCellsPresenter.Item" /> property.</param>
	/// <param name="newItem">The new value of the <see cref="P:System.Windows.Controls.Primitives.DataGridCellsPresenter.Item" /> property.</param>
	protected virtual void OnItemChanged(object oldItem, object newItem)
	{
		ObservableCollection<DataGridColumn> columns = Columns;
		if (columns != null)
		{
			if (!(base.ItemsSource is MultipleCopiesCollection multipleCopiesCollection))
			{
				MultipleCopiesCollection itemsSource = new MultipleCopiesCollection(newItem, columns.Count);
				base.ItemsSource = itemsSource;
			}
			else
			{
				multipleCopiesCollection.CopiedItem = newItem;
			}
		}
	}

	/// <returns>true if the item is (or is eligible to be) its own container; otherwise, false.</returns>
	/// <param name="item">The item to check.</param>
	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		return item is DataGridCell;
	}

	internal bool IsItemItsOwnContainerInternal(object item)
	{
		return IsItemItsOwnContainerOverride(item);
	}

	/// <summary>Returns a new <see cref="T:System.Windows.Controls.DataGridCell" /> instance to contain a cell value.</summary>
	/// <returns>A new <see cref="T:System.Windows.Controls.DataGridCell" /> instance.</returns>
	protected override DependencyObject GetContainerForItemOverride()
	{
		return new DataGridCell();
	}

	/// <summary>Prepares the cell to display the specified item.</summary>
	/// <param name="element">The cell to prepare.</param>
	/// <param name="item">The item to display.</param>
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		DataGridCell dataGridCell = (DataGridCell)element;
		DataGridRow dataGridRowOwner = DataGridRowOwner;
		if (dataGridCell.RowOwner != dataGridRowOwner)
		{
			dataGridCell.Tracker.StartTracking(ref _cellTrackingRoot);
		}
		dataGridCell.PrepareCell(item, this, dataGridRowOwner);
	}

	/// <summary>Clears the container reference for the specified cell.</summary>
	/// <param name="element">The <see cref="T:System.Windows.Controls.DataGridCell" /> to clear.</param>
	/// <param name="item">The data item. This value is not used.</param>
	protected override void ClearContainerForItemOverride(DependencyObject element, object item)
	{
		DataGridCell dataGridCell = (DataGridCell)element;
		DataGridRow dataGridRowOwner = DataGridRowOwner;
		if (dataGridCell.RowOwner == dataGridRowOwner)
		{
			dataGridCell.Tracker.StopTracking(ref _cellTrackingRoot);
		}
		dataGridCell.ClearCell(dataGridRowOwner);
	}

	/// <summary>Updates the displayed cells when the <see cref="P:System.Windows.Controls.DataGrid.Columns" /> collection has changed. </summary>
	/// <param name="columns">The <see cref="P:System.Windows.Controls.DataGrid.Columns" /> collection.</param>
	/// <param name="e">The event data from the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged" /> event of the <see cref="P:System.Windows.Controls.DataGrid.Columns" /> collection.</param>
	protected internal virtual void OnColumnsChanged(ObservableCollection<DataGridColumn> columns, NotifyCollectionChangedEventArgs e)
	{
		if (base.ItemsSource is MultipleCopiesCollection multipleCopiesCollection)
		{
			multipleCopiesCollection.MirrorCollectionChange(e);
		}
	}

	private static void OnNotifyHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGridCellsPresenter)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.CellsPresenter);
	}

	internal void NotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e, DataGridNotificationTarget target)
	{
		NotifyPropertyChanged(d, string.Empty, e, target);
	}

	internal void NotifyPropertyChanged(DependencyObject d, string propertyName, DependencyPropertyChangedEventArgs e, DataGridNotificationTarget target)
	{
		if (DataGridHelper.ShouldNotifyCellsPresenter(target))
		{
			if (e.Property == DataGridColumn.WidthProperty || e.Property == DataGridColumn.DisplayIndexProperty)
			{
				if (((DataGridColumn)d).IsVisible)
				{
					InvalidateDataGridCellsPanelMeasureAndArrangeImpl(e.Property == DataGridColumn.WidthProperty);
				}
			}
			else if (e.Property == DataGrid.FrozenColumnCountProperty || e.Property == DataGridColumn.VisibilityProperty || e.Property == DataGrid.CellsPanelHorizontalOffsetProperty || e.Property == DataGrid.HorizontalScrollOffsetProperty || string.Compare(propertyName, "ViewportWidth", StringComparison.Ordinal) == 0 || string.Compare(propertyName, "DelayedColumnWidthComputation", StringComparison.Ordinal) == 0)
			{
				InvalidateDataGridCellsPanelMeasureAndArrange();
			}
			else if (string.Compare(propertyName, "RealizedColumnsBlockListForNonVirtualizedRows", StringComparison.Ordinal) == 0)
			{
				InvalidateDataGridCellsPanelMeasureAndArrange(withColumnVirtualization: false);
			}
			else if (string.Compare(propertyName, "RealizedColumnsBlockListForVirtualizedRows", StringComparison.Ordinal) == 0)
			{
				InvalidateDataGridCellsPanelMeasureAndArrange(withColumnVirtualization: true);
			}
			else if (e.Property == DataGrid.RowHeightProperty || e.Property == FrameworkElement.HeightProperty)
			{
				DataGridHelper.TransferProperty(this, FrameworkElement.HeightProperty);
			}
			else if (e.Property == DataGrid.MinRowHeightProperty || e.Property == FrameworkElement.MinHeightProperty)
			{
				DataGridHelper.TransferProperty(this, FrameworkElement.MinHeightProperty);
			}
			else if (e.Property == DataGrid.EnableColumnVirtualizationProperty)
			{
				DataGridHelper.TransferProperty(this, VirtualizingPanel.IsVirtualizingProperty);
			}
		}
		if (DataGridHelper.ShouldNotifyCells(target) || DataGridHelper.ShouldRefreshCellContent(target))
		{
			for (ContainerTracking<DataGridCell> containerTracking = _cellTrackingRoot; containerTracking != null; containerTracking = containerTracking.Next)
			{
				containerTracking.Container.NotifyPropertyChanged(d, propertyName, e, target);
			}
		}
	}

	/// <returns>The size of the control, up to the maximum specified by <paramref name="constraint" />.</returns>
	/// <param name="availableSize">The available size that this element can give to child elements.</param>
	protected override Size MeasureOverride(Size availableSize)
	{
		return base.MeasureOverride(availableSize);
	}

	/// <returns>The size of the control.</returns>
	/// <param name="finalSize">The final area within the parent that this object should use to arrange itself and its children.</param>
	protected override Size ArrangeOverride(Size finalSize)
	{
		return base.ArrangeOverride(finalSize);
	}

	/// <summary>Called by the layout system to draw a horizontal line below the cells if horizontal grid lines are visible.</summary>
	/// <param name="drawingContext">The drawing instructions for the cells. This context is provided to the layout system.</param>
	protected override void OnRender(DrawingContext drawingContext)
	{
		base.OnRender(drawingContext);
		DataGridRow dataGridRowOwner = DataGridRowOwner;
		if (dataGridRowOwner != null)
		{
			DataGrid dataGridOwner = dataGridRowOwner.DataGridOwner;
			if (dataGridOwner != null && DataGridHelper.IsGridLineVisible(dataGridOwner, isHorizontal: true))
			{
				double horizontalGridLineThickness = dataGridOwner.HorizontalGridLineThickness;
				Rect rectangle = new Rect(new Size(base.RenderSize.Width, horizontalGridLineThickness));
				rectangle.Y = base.RenderSize.Height - horizontalGridLineThickness;
				drawingContext.DrawRectangle(dataGridOwner.HorizontalGridLinesBrush, null, rectangle);
			}
		}
	}

	private static void OnIsVirtualizingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGridCellsPresenter dataGridCellsPresenter = (DataGridCellsPresenter)d;
		DataGridHelper.TransferProperty(dataGridCellsPresenter, VirtualizingPanel.IsVirtualizingProperty);
		if (e.OldValue != dataGridCellsPresenter.GetValue(VirtualizingPanel.IsVirtualizingProperty))
		{
			dataGridCellsPresenter.InvalidateDataGridCellsPanelMeasureAndArrange();
		}
	}

	private static object OnCoerceIsVirtualizingProperty(DependencyObject d, object baseValue)
	{
		DataGridCellsPresenter dataGridCellsPresenter = d as DataGridCellsPresenter;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridCellsPresenter, baseValue, VirtualizingPanel.IsVirtualizingProperty, dataGridCellsPresenter.DataGridOwner, DataGrid.EnableColumnVirtualizationProperty);
	}

	internal void InvalidateDataGridCellsPanelMeasureAndArrange()
	{
		InvalidateDataGridCellsPanelMeasureAndArrangeImpl(invalidateMeasureUptoRowsPresenter: false);
	}

	private void InvalidateDataGridCellsPanelMeasureAndArrangeImpl(bool invalidateMeasureUptoRowsPresenter)
	{
		if (_internalItemsHost == null)
		{
			return;
		}
		_internalItemsHost.InvalidateMeasure();
		_internalItemsHost.InvalidateArrange();
		if (invalidateMeasureUptoRowsPresenter)
		{
			DataGrid dataGridOwner = DataGridOwner;
			if (dataGridOwner != null && dataGridOwner.InternalItemsHost != null)
			{
				Helper.InvalidateMeasureOnPath(_internalItemsHost, dataGridOwner.InternalItemsHost, duringMeasure: false, includePathEnd: true);
			}
		}
	}

	private void InvalidateDataGridCellsPanelMeasureAndArrange(bool withColumnVirtualization)
	{
		if (withColumnVirtualization == VirtualizingPanel.GetIsVirtualizing(this))
		{
			InvalidateDataGridCellsPanelMeasureAndArrange();
		}
	}

	internal void ScrollCellIntoView(int index)
	{
		if (InternalItemsHost is DataGridCellsPanel dataGridCellsPanel)
		{
			dataGridCellsPanel.InternalBringIndexIntoView(index);
		}
	}
}
