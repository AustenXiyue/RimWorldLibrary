using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Controls.Primitives;

/// <summary>Used within the template of a <see cref="T:System.Windows.Controls.DataGrid" /> to specify the location in the control's visual tree where the column headers are to be added.</summary>
[TemplatePart(Name = "PART_FillerColumnHeader", Type = typeof(DataGridColumnHeader))]
public class DataGridColumnHeadersPresenter : ItemsControl
{
	private const string ElementFillerColumnHeader = "PART_FillerColumnHeader";

	private ContainerTracking<DataGridColumnHeader> _headerTrackingRoot;

	private DataGrid _parentDataGrid;

	private bool _prepareColumnHeaderDragging;

	private bool _isColumnHeaderDragging;

	private DataGridColumnHeader _draggingSrcColumnHeader;

	private Point _columnHeaderDragStartPosition;

	private Point _columnHeaderDragStartRelativePosition;

	private Point _columnHeaderDragCurrentPosition;

	private Control _columnHeaderDropLocationIndicator;

	private Control _columnHeaderDragIndicator;

	private Panel _internalItemsHost;

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

	/// <summary>Gets the number of <see cref="T:System.Windows.Media.Visual" /> children in this presenter.</summary>
	/// <returns>The number of visual children. </returns>
	protected override int VisualChildrenCount
	{
		get
		{
			int num = base.VisualChildrenCount;
			if (_columnHeaderDragIndicator != null)
			{
				num++;
			}
			if (_columnHeaderDropLocationIndicator != null)
			{
				num++;
			}
			return num;
		}
	}

	private DataGridColumnHeaderCollection HeaderCollection => base.ItemsSource as DataGridColumnHeaderCollection;

	internal DataGrid ParentDataGrid
	{
		get
		{
			if (_parentDataGrid == null)
			{
				_parentDataGrid = DataGridHelper.FindParent<DataGrid>(this);
			}
			return _parentDataGrid;
		}
	}

	internal ContainerTracking<DataGridColumnHeader> HeaderTrackingRoot => _headerTrackingRoot;

	static DataGridColumnHeadersPresenter()
	{
		Type typeFromHandle = typeof(DataGridColumnHeadersPresenter);
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(typeFromHandle));
		UIElement.FocusableProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(false));
		FrameworkElementFactory root = new FrameworkElementFactory(typeof(DataGridCellsPanel));
		ItemsControl.ItemsPanelProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(new ItemsPanelTemplate(root)));
		VirtualizingPanel.IsVirtualizingProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(false, OnIsVirtualizingPropertyChanged, OnCoerceIsVirtualizingProperty));
		VirtualizingPanel.VirtualizationModeProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(VirtualizationMode.Recycling));
	}

	public override void OnApplyTemplate()
	{
		if (InternalItemsHost != null && !IsAncestorOf(InternalItemsHost))
		{
			InternalItemsHost = null;
		}
		base.OnApplyTemplate();
		DataGrid parentDataGrid = ParentDataGrid;
		if (parentDataGrid != null)
		{
			base.ItemsSource = new DataGridColumnHeaderCollection(parentDataGrid.Columns);
			parentDataGrid.ColumnHeadersPresenter = this;
			DataGridHelper.TransferProperty(this, VirtualizingPanel.IsVirtualizingProperty);
			if (GetTemplateChild("PART_FillerColumnHeader") is DataGridColumnHeader d)
			{
				DataGridHelper.TransferProperty(d, FrameworkElement.StyleProperty);
				DataGridHelper.TransferProperty(d, FrameworkElement.HeightProperty);
			}
		}
		else
		{
			base.ItemsSource = null;
		}
	}

	/// <summary>Returns a new <see cref="T:System.Windows.Automation.Peers.DataGridColumnHeadersPresenterAutomationPeer" /> for this presenter.</summary>
	/// <returns>A new automation peer for this presenter.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DataGridColumnHeadersPresenterAutomationPeer(this);
	}

	/// <returns>The size of the control, up to the maximum specified by <paramref name="constraint" />.</returns>
	/// <param name="availableSize">The available size that this element can give to child elements.</param>
	protected override Size MeasureOverride(Size availableSize)
	{
		Size size = availableSize;
		size.Width = double.PositiveInfinity;
		Size result = base.MeasureOverride(size);
		if (_columnHeaderDragIndicator != null && _isColumnHeaderDragging)
		{
			_columnHeaderDragIndicator.Measure(size);
			Size desiredSize = _columnHeaderDragIndicator.DesiredSize;
			result.Width = Math.Max(result.Width, desiredSize.Width);
			result.Height = Math.Max(result.Height, desiredSize.Height);
		}
		if (_columnHeaderDropLocationIndicator != null && _isColumnHeaderDragging)
		{
			_columnHeaderDropLocationIndicator.Measure(availableSize);
			Size desiredSize = _columnHeaderDropLocationIndicator.DesiredSize;
			result.Width = Math.Max(result.Width, desiredSize.Width);
			result.Height = Math.Max(result.Height, desiredSize.Height);
		}
		result.Width = Math.Min(availableSize.Width, result.Width);
		return result;
	}

	/// <returns>The size of the control.</returns>
	/// <param name="finalSize">The final area within the parent that this element should use to arrange itself and its children.</param>
	protected override Size ArrangeOverride(Size finalSize)
	{
		UIElement uIElement = ((VisualTreeHelper.GetChildrenCount(this) > 0) ? (VisualTreeHelper.GetChild(this, 0) as UIElement) : null);
		if (uIElement != null)
		{
			Rect finalRect = new Rect(finalSize);
			DataGrid parentDataGrid = ParentDataGrid;
			if (parentDataGrid != null)
			{
				finalRect.X = 0.0 - parentDataGrid.HorizontalScrollOffset;
				finalRect.Width = Math.Max(finalSize.Width, parentDataGrid.CellsPanelActualWidth);
			}
			uIElement.Arrange(finalRect);
		}
		if (_columnHeaderDragIndicator != null && _isColumnHeaderDragging)
		{
			_columnHeaderDragIndicator.Arrange(new Rect(new Point(_columnHeaderDragCurrentPosition.X - _columnHeaderDragStartRelativePosition.X, 0.0), new Size(_columnHeaderDragIndicator.Width, _columnHeaderDragIndicator.Height)));
		}
		if (_columnHeaderDropLocationIndicator != null && _isColumnHeaderDragging)
		{
			Point location = FindColumnHeaderPositionByCurrentPosition(_columnHeaderDragCurrentPosition, findNearestColumn: true);
			double width = _columnHeaderDropLocationIndicator.Width;
			location.X -= width * 0.5;
			_columnHeaderDropLocationIndicator.Arrange(new Rect(location, new Size(width, _columnHeaderDropLocationIndicator.Height)));
		}
		return finalSize;
	}

	/// <summary>Returns a geometry for a clipping mask. The mask applies if the layout system attempts to arrange an element that is larger than the available display space.</summary>
	/// <returns>The clipping geometry.</returns>
	/// <param name="layoutSlotSize">The rendered size of the column header.</param>
	protected override Geometry GetLayoutClip(Size layoutSlotSize)
	{
		RectangleGeometry rectangleGeometry = new RectangleGeometry(new Rect(base.RenderSize));
		rectangleGeometry.Freeze();
		return rectangleGeometry;
	}

	/// <summary>Returns a new <see cref="T:System.Windows.Controls.Primitives.DataGridColumnHeader" /> instance to contain a column header value.</summary>
	/// <returns>A new <see cref="T:System.Windows.Controls.Primitives.DataGridColumnHeader" /> instance. </returns>
	protected override DependencyObject GetContainerForItemOverride()
	{
		return new DataGridColumnHeader();
	}

	/// <returns>true if the item is (or is eligible to be) its own container; otherwise, false.</returns>
	/// <param name="item">The item to check.</param>
	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		return item is DataGridColumnHeader;
	}

	internal bool IsItemItsOwnContainerInternal(object item)
	{
		return IsItemItsOwnContainerOverride(item);
	}

	/// <param name="element">Element used to display the specified item.</param>
	/// <param name="item">Specified item.</param>
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		if (element is DataGridColumnHeader dataGridColumnHeader)
		{
			DataGridColumn column = ColumnFromContainer(dataGridColumnHeader);
			if (dataGridColumnHeader.Column == null)
			{
				dataGridColumnHeader.Tracker.StartTracking(ref _headerTrackingRoot);
			}
			dataGridColumnHeader.PrepareColumnHeader(item, column);
		}
	}

	/// <param name="element">The container element.</param>
	/// <param name="item">The item.</param>
	protected override void ClearContainerForItemOverride(DependencyObject element, object item)
	{
		DataGridColumnHeader dataGridColumnHeader = element as DataGridColumnHeader;
		base.ClearContainerForItemOverride(element, item);
		if (dataGridColumnHeader != null)
		{
			dataGridColumnHeader.Tracker.StopTracking(ref _headerTrackingRoot);
			dataGridColumnHeader.ClearHeader();
		}
	}

	private DataGridColumn ColumnFromContainer(DataGridColumnHeader container)
	{
		int index = base.ItemContainerGenerator.IndexFromContainer(container);
		return HeaderCollection.ColumnFromIndex(index);
	}

	internal void NotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e, DataGridNotificationTarget target)
	{
		NotifyPropertyChanged(d, string.Empty, e, target);
	}

	internal void NotifyPropertyChanged(DependencyObject d, string propertyName, DependencyPropertyChangedEventArgs e, DataGridNotificationTarget target)
	{
		DataGridColumn dataGridColumn = d as DataGridColumn;
		if (DataGridHelper.ShouldNotifyColumnHeadersPresenter(target))
		{
			if (e.Property == DataGridColumn.WidthProperty || e.Property == DataGridColumn.DisplayIndexProperty)
			{
				if (dataGridColumn.IsVisible)
				{
					InvalidateDataGridCellsPanelMeasureAndArrange();
				}
			}
			else if (e.Property == DataGrid.FrozenColumnCountProperty || e.Property == DataGridColumn.VisibilityProperty || e.Property == DataGrid.CellsPanelHorizontalOffsetProperty || string.Compare(propertyName, "ViewportWidth", StringComparison.Ordinal) == 0 || string.Compare(propertyName, "DelayedColumnWidthComputation", StringComparison.Ordinal) == 0)
			{
				InvalidateDataGridCellsPanelMeasureAndArrange();
			}
			else if (e.Property == DataGrid.HorizontalScrollOffsetProperty)
			{
				InvalidateArrange();
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
			else if (e.Property == DataGrid.CellsPanelActualWidthProperty)
			{
				InvalidateArrange();
			}
			else if (e.Property == DataGrid.EnableColumnVirtualizationProperty)
			{
				DataGridHelper.TransferProperty(this, VirtualizingPanel.IsVirtualizingProperty);
			}
		}
		if (!DataGridHelper.ShouldNotifyColumnHeaders(target))
		{
			return;
		}
		if (e.Property == DataGridColumn.HeaderProperty)
		{
			if (HeaderCollection != null)
			{
				HeaderCollection.NotifyHeaderPropertyChanged(dataGridColumn, e);
			}
			return;
		}
		for (ContainerTracking<DataGridColumnHeader> containerTracking = _headerTrackingRoot; containerTracking != null; containerTracking = containerTracking.Next)
		{
			containerTracking.Container.NotifyPropertyChanged(d, e);
		}
		if (d is DataGrid && (e.Property == DataGrid.ColumnHeaderStyleProperty || e.Property == DataGrid.ColumnHeaderHeightProperty) && GetTemplateChild("PART_FillerColumnHeader") is DataGridColumnHeader dataGridColumnHeader)
		{
			dataGridColumnHeader.NotifyPropertyChanged(d, e);
		}
	}

	private static void OnIsVirtualizingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGridColumnHeadersPresenter dataGridColumnHeadersPresenter = (DataGridColumnHeadersPresenter)d;
		DataGridHelper.TransferProperty(dataGridColumnHeadersPresenter, VirtualizingPanel.IsVirtualizingProperty);
		if (e.OldValue != dataGridColumnHeadersPresenter.GetValue(VirtualizingPanel.IsVirtualizingProperty))
		{
			dataGridColumnHeadersPresenter.InvalidateDataGridCellsPanelMeasureAndArrange();
		}
	}

	private static object OnCoerceIsVirtualizingProperty(DependencyObject d, object baseValue)
	{
		DataGridColumnHeadersPresenter dataGridColumnHeadersPresenter = d as DataGridColumnHeadersPresenter;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumnHeadersPresenter, baseValue, VirtualizingPanel.IsVirtualizingProperty, dataGridColumnHeadersPresenter.ParentDataGrid, DataGrid.EnableColumnVirtualizationProperty);
	}

	private void InvalidateDataGridCellsPanelMeasureAndArrange()
	{
		if (_internalItemsHost != null)
		{
			_internalItemsHost.InvalidateMeasure();
			_internalItemsHost.InvalidateArrange();
		}
	}

	private void InvalidateDataGridCellsPanelMeasureAndArrange(bool withColumnVirtualization)
	{
		if (withColumnVirtualization == VirtualizingPanel.GetIsVirtualizing(this))
		{
			InvalidateDataGridCellsPanelMeasureAndArrange();
		}
	}

	/// <summary>Returns the <see cref="T:System.Windows.Media.Visual" /> child at the specified index.</summary>
	/// <returns>The child. </returns>
	/// <param name="index">The index of the <see cref="T:System.Windows.Media.Visual" /> child to return.</param>
	protected override Visual GetVisualChild(int index)
	{
		int visualChildrenCount = base.VisualChildrenCount;
		if (index == visualChildrenCount)
		{
			if (_columnHeaderDragIndicator != null)
			{
				return _columnHeaderDragIndicator;
			}
			if (_columnHeaderDropLocationIndicator != null)
			{
				return _columnHeaderDropLocationIndicator;
			}
		}
		if (index == visualChildrenCount + 1 && _columnHeaderDragIndicator != null && _columnHeaderDropLocationIndicator != null)
		{
			return _columnHeaderDropLocationIndicator;
		}
		return base.GetVisualChild(index);
	}

	internal void OnHeaderMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		if (ParentDataGrid == null)
		{
			return;
		}
		if (_columnHeaderDragIndicator != null)
		{
			RemoveVisualChild(_columnHeaderDragIndicator);
			_columnHeaderDragIndicator = null;
		}
		if (_columnHeaderDropLocationIndicator != null)
		{
			RemoveVisualChild(_columnHeaderDropLocationIndicator);
			_columnHeaderDropLocationIndicator = null;
		}
		Point position = e.GetPosition(this);
		DataGridColumnHeader dataGridColumnHeader = FindColumnHeaderByPosition(position);
		if (dataGridColumnHeader != null)
		{
			DataGridColumn column = dataGridColumnHeader.Column;
			if (ParentDataGrid.CanUserReorderColumns && column.CanUserReorder)
			{
				PrepareColumnHeaderDrag(dataGridColumnHeader, e.GetPosition(this), e.GetPosition(dataGridColumnHeader));
			}
		}
		else
		{
			_isColumnHeaderDragging = false;
			_prepareColumnHeaderDragging = false;
			_draggingSrcColumnHeader = null;
			InvalidateArrange();
		}
	}

	internal void OnHeaderMouseMove(MouseEventArgs e)
	{
		if (e.LeftButton != MouseButtonState.Pressed || !_prepareColumnHeaderDragging)
		{
			return;
		}
		_columnHeaderDragCurrentPosition = e.GetPosition(this);
		if (!_isColumnHeaderDragging)
		{
			if (CheckStartColumnHeaderDrag(_columnHeaderDragCurrentPosition, _columnHeaderDragStartPosition))
			{
				StartColumnHeaderDrag();
			}
			return;
		}
		Visibility visibility = ((!IsMousePositionValidForColumnDrag(2.0)) ? Visibility.Collapsed : Visibility.Visible);
		if (_columnHeaderDragIndicator != null)
		{
			_columnHeaderDragIndicator.Visibility = visibility;
		}
		if (_columnHeaderDropLocationIndicator != null)
		{
			_columnHeaderDropLocationIndicator.Visibility = visibility;
		}
		InvalidateArrange();
		DragDeltaEventArgs e2 = new DragDeltaEventArgs(_columnHeaderDragCurrentPosition.X - _columnHeaderDragStartPosition.X, _columnHeaderDragCurrentPosition.Y - _columnHeaderDragStartPosition.Y);
		_columnHeaderDragStartPosition = _columnHeaderDragCurrentPosition;
		ParentDataGrid.OnColumnHeaderDragDelta(e2);
	}

	internal void OnHeaderMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		if (_isColumnHeaderDragging)
		{
			_columnHeaderDragCurrentPosition = e.GetPosition(this);
			FinishColumnHeaderDrag(isCancel: false);
		}
		else
		{
			ClearColumnHeaderDragInfo();
		}
	}

	internal void OnHeaderLostMouseCapture(MouseEventArgs e)
	{
		if (_isColumnHeaderDragging && Mouse.LeftButton == MouseButtonState.Pressed)
		{
			FinishColumnHeaderDrag(isCancel: true);
		}
	}

	private void ClearColumnHeaderDragInfo()
	{
		_isColumnHeaderDragging = false;
		_prepareColumnHeaderDragging = false;
		_draggingSrcColumnHeader = null;
		if (_columnHeaderDragIndicator != null)
		{
			RemoveVisualChild(_columnHeaderDragIndicator);
			_columnHeaderDragIndicator = null;
		}
		if (_columnHeaderDropLocationIndicator != null)
		{
			RemoveVisualChild(_columnHeaderDropLocationIndicator);
			_columnHeaderDropLocationIndicator = null;
		}
	}

	private void PrepareColumnHeaderDrag(DataGridColumnHeader header, Point pos, Point relativePos)
	{
		_prepareColumnHeaderDragging = true;
		_isColumnHeaderDragging = false;
		_draggingSrcColumnHeader = header;
		_columnHeaderDragStartPosition = pos;
		_columnHeaderDragStartRelativePosition = relativePos;
	}

	private static bool CheckStartColumnHeaderDrag(Point currentPos, Point originalPos)
	{
		return DoubleUtil.GreaterThan(Math.Abs(currentPos.X - originalPos.X), SystemParameters.MinimumHorizontalDragDistance);
	}

	private bool IsMousePositionValidForColumnDrag(double dragFactor)
	{
		int nearestDisplayIndex = -1;
		return IsMousePositionValidForColumnDrag(dragFactor, out nearestDisplayIndex);
	}

	private bool IsMousePositionValidForColumnDrag(double dragFactor, out int nearestDisplayIndex)
	{
		nearestDisplayIndex = -1;
		bool flag = false;
		if (_draggingSrcColumnHeader.Column != null)
		{
			flag = _draggingSrcColumnHeader.Column.IsFrozen;
		}
		int num = 0;
		if (ParentDataGrid != null)
		{
			num = ParentDataGrid.FrozenColumnCount;
		}
		nearestDisplayIndex = FindDisplayIndexByPosition(_columnHeaderDragCurrentPosition, findNearestColumn: true);
		if (flag && nearestDisplayIndex >= num)
		{
			return false;
		}
		if (!flag && nearestDisplayIndex < num)
		{
			return false;
		}
		double num2 = 0.0;
		num2 = ((_columnHeaderDragIndicator != null) ? Math.Max(_draggingSrcColumnHeader.RenderSize.Height, _columnHeaderDragIndicator.Height) : _draggingSrcColumnHeader.RenderSize.Height);
		if (DoubleUtil.LessThanOrClose((0.0 - num2) * dragFactor, _columnHeaderDragCurrentPosition.Y))
		{
			return DoubleUtil.LessThanOrClose(_columnHeaderDragCurrentPosition.Y, num2 * (dragFactor + 1.0));
		}
		return false;
	}

	private void StartColumnHeaderDrag()
	{
		_columnHeaderDragStartPosition = _columnHeaderDragCurrentPosition;
		DragStartedEventArgs e = new DragStartedEventArgs(_columnHeaderDragStartPosition.X, _columnHeaderDragStartPosition.Y);
		ParentDataGrid.OnColumnHeaderDragStarted(e);
		DataGridColumnReorderingEventArgs dataGridColumnReorderingEventArgs = new DataGridColumnReorderingEventArgs(_draggingSrcColumnHeader.Column);
		_columnHeaderDragIndicator = CreateColumnHeaderDragIndicator();
		_columnHeaderDropLocationIndicator = CreateColumnHeaderDropIndicator();
		dataGridColumnReorderingEventArgs.DragIndicator = _columnHeaderDragIndicator;
		dataGridColumnReorderingEventArgs.DropLocationIndicator = _columnHeaderDropLocationIndicator;
		ParentDataGrid.OnColumnReordering(dataGridColumnReorderingEventArgs);
		if (!dataGridColumnReorderingEventArgs.Cancel)
		{
			_isColumnHeaderDragging = true;
			_columnHeaderDragIndicator = dataGridColumnReorderingEventArgs.DragIndicator;
			_columnHeaderDropLocationIndicator = dataGridColumnReorderingEventArgs.DropLocationIndicator;
			if (_columnHeaderDragIndicator != null)
			{
				SetDefaultsOnDragIndicator();
				AddVisualChild(_columnHeaderDragIndicator);
			}
			if (_columnHeaderDropLocationIndicator != null)
			{
				SetDefaultsOnDropIndicator();
				AddVisualChild(_columnHeaderDropLocationIndicator);
			}
			_draggingSrcColumnHeader.SuppressClickEvent = true;
			InvalidateMeasure();
		}
		else
		{
			FinishColumnHeaderDrag(isCancel: true);
		}
	}

	private Control CreateColumnHeaderDragIndicator()
	{
		return new DataGridColumnFloatingHeader
		{
			ReferenceHeader = _draggingSrcColumnHeader
		};
	}

	private void SetDefaultsOnDragIndicator()
	{
		DataGridColumn column = _draggingSrcColumnHeader.Column;
		Style style = null;
		if (column != null)
		{
			style = column.DragIndicatorStyle;
		}
		_columnHeaderDragIndicator.Style = style;
		_columnHeaderDragIndicator.CoerceValue(FrameworkElement.WidthProperty);
		_columnHeaderDragIndicator.CoerceValue(FrameworkElement.HeightProperty);
	}

	private Control CreateColumnHeaderDropIndicator()
	{
		return new DataGridColumnDropSeparator
		{
			ReferenceHeader = _draggingSrcColumnHeader
		};
	}

	private void SetDefaultsOnDropIndicator()
	{
		Style style = null;
		if (ParentDataGrid != null)
		{
			style = ParentDataGrid.DropLocationIndicatorStyle;
		}
		_columnHeaderDropLocationIndicator.Style = style;
		_columnHeaderDropLocationIndicator.CoerceValue(FrameworkElement.WidthProperty);
		_columnHeaderDropLocationIndicator.CoerceValue(FrameworkElement.HeightProperty);
	}

	private void FinishColumnHeaderDrag(bool isCancel)
	{
		_prepareColumnHeaderDragging = false;
		_isColumnHeaderDragging = false;
		_draggingSrcColumnHeader.SuppressClickEvent = false;
		if (_columnHeaderDragIndicator != null)
		{
			_columnHeaderDragIndicator.Visibility = Visibility.Collapsed;
			if (_columnHeaderDragIndicator is DataGridColumnFloatingHeader dataGridColumnFloatingHeader)
			{
				dataGridColumnFloatingHeader.ClearHeader();
			}
			RemoveVisualChild(_columnHeaderDragIndicator);
		}
		if (_columnHeaderDropLocationIndicator != null)
		{
			_columnHeaderDropLocationIndicator.Visibility = Visibility.Collapsed;
			if (_columnHeaderDropLocationIndicator is DataGridColumnDropSeparator dataGridColumnDropSeparator)
			{
				dataGridColumnDropSeparator.ReferenceHeader = null;
			}
			RemoveVisualChild(_columnHeaderDropLocationIndicator);
		}
		DragCompletedEventArgs e = new DragCompletedEventArgs(_columnHeaderDragCurrentPosition.X - _columnHeaderDragStartPosition.X, _columnHeaderDragCurrentPosition.Y - _columnHeaderDragStartPosition.Y, isCancel);
		ParentDataGrid.OnColumnHeaderDragCompleted(e);
		_draggingSrcColumnHeader.InvalidateArrange();
		if (!isCancel)
		{
			int nearestDisplayIndex = -1;
			bool flag = IsMousePositionValidForColumnDrag(2.0, out nearestDisplayIndex);
			DataGridColumn column = _draggingSrcColumnHeader.Column;
			if (column != null && flag && nearestDisplayIndex != column.DisplayIndex)
			{
				column.DisplayIndex = nearestDisplayIndex;
				DataGridColumnEventArgs e2 = new DataGridColumnEventArgs(_draggingSrcColumnHeader.Column);
				ParentDataGrid.OnColumnReordered(e2);
			}
		}
		_draggingSrcColumnHeader = null;
		_columnHeaderDragIndicator = null;
		_columnHeaderDropLocationIndicator = null;
	}

	private int FindDisplayIndexByPosition(Point startPos, bool findNearestColumn)
	{
		FindDisplayIndexAndHeaderPosition(startPos, findNearestColumn, out var displayIndex, out var _, out var _);
		return displayIndex;
	}

	private DataGridColumnHeader FindColumnHeaderByPosition(Point startPos)
	{
		FindDisplayIndexAndHeaderPosition(startPos, findNearestColumn: false, out var _, out var _, out var header);
		return header;
	}

	private Point FindColumnHeaderPositionByCurrentPosition(Point startPos, bool findNearestColumn)
	{
		FindDisplayIndexAndHeaderPosition(startPos, findNearestColumn, out var _, out var headerPos, out var _);
		return headerPos;
	}

	private static double GetColumnEstimatedWidth(DataGridColumn column, double averageColumnWidth)
	{
		double num = column.Width.DisplayValue;
		if (double.IsNaN(num))
		{
			num = Math.Max(averageColumnWidth, column.MinWidth);
			num = Math.Min(num, column.MaxWidth);
		}
		return num;
	}

	private void FindDisplayIndexAndHeaderPosition(Point startPos, bool findNearestColumn, out int displayIndex, out Point headerPos, out DataGridColumnHeader header)
	{
		Point point = (headerPos = new Point(0.0, 0.0));
		displayIndex = -1;
		header = null;
		if (startPos.X < 0.0)
		{
			if (findNearestColumn)
			{
				displayIndex = 0;
			}
			return;
		}
		double num = 0.0;
		double num2 = 0.0;
		int num3 = 0;
		DataGrid parentDataGrid = ParentDataGrid;
		double averageColumnWidth = parentDataGrid.InternalColumns.AverageColumnWidth;
		bool flag = false;
		for (num3 = 0; num3 < parentDataGrid.Columns.Count; num3++)
		{
			displayIndex++;
			DataGridColumnHeader dataGridColumnHeader = parentDataGrid.ColumnHeaderFromDisplayIndex(num3);
			if (dataGridColumnHeader == null)
			{
				DataGridColumn dataGridColumn = parentDataGrid.ColumnFromDisplayIndex(num3);
				if (!dataGridColumn.IsVisible)
				{
					continue;
				}
				num = num2;
				if (num3 >= parentDataGrid.FrozenColumnCount && !flag)
				{
					num -= parentDataGrid.HorizontalScrollOffset;
					flag = true;
				}
				num2 = num + GetColumnEstimatedWidth(dataGridColumn, averageColumnWidth);
			}
			else
			{
				num = dataGridColumnHeader.TransformToAncestor(this).Transform(point).X;
				num2 = num + dataGridColumnHeader.RenderSize.Width;
			}
			if (DoubleUtil.LessThanOrClose(startPos.X, num))
			{
				break;
			}
			if (!DoubleUtil.GreaterThanOrClose(startPos.X, num) || !DoubleUtil.LessThanOrClose(startPos.X, num2))
			{
				continue;
			}
			if (findNearestColumn)
			{
				double value = (num + num2) * 0.5;
				if (DoubleUtil.GreaterThanOrClose(startPos.X, value))
				{
					num = num2;
					displayIndex++;
				}
				if (_draggingSrcColumnHeader != null && _draggingSrcColumnHeader.Column != null && _draggingSrcColumnHeader.Column.DisplayIndex < displayIndex)
				{
					displayIndex--;
				}
			}
			else
			{
				header = dataGridColumnHeader;
			}
			break;
		}
		if (num3 == parentDataGrid.Columns.Count)
		{
			displayIndex = parentDataGrid.Columns.Count - 1;
			num = num2;
		}
		headerPos.X = num;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.DataGridColumnHeadersPresenter" /> class. </summary>
	public DataGridColumnHeadersPresenter()
	{
	}
}
