using System.ComponentModel;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents an individual <see cref="T:System.Windows.Controls.DataGrid" /> column header.</summary>
[TemplatePart(Name = "PART_LeftHeaderGripper", Type = typeof(Thumb))]
[TemplatePart(Name = "PART_RightHeaderGripper", Type = typeof(Thumb))]
public class DataGridColumnHeader : ButtonBase, IProvideDataGridColumn
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DataGridColumnHeader.SeparatorBrush" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DataGridColumnHeader.SeparatorBrush" /> dependency property.</returns>
	public static readonly DependencyProperty SeparatorBrushProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DataGridColumnHeader.SeparatorVisibility" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DataGridColumnHeader.SeparatorVisibility" /> dependency property.</returns>
	public static readonly DependencyProperty SeparatorVisibilityProperty;

	private static readonly DependencyPropertyKey DisplayIndexPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DataGridColumnHeader.DisplayIndex" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DataGridColumnHeader.DisplayIndex" /> dependency property.</returns>
	public static readonly DependencyProperty DisplayIndexProperty;

	private static readonly DependencyPropertyKey CanUserSortPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DataGridColumnHeader.CanUserSort" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DataGridColumnHeader.CanUserSort" /> dependency property.</returns>
	public static readonly DependencyProperty CanUserSortProperty;

	private static readonly DependencyPropertyKey SortDirectionPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DataGridColumnHeader.SortDirection" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DataGridColumnHeader.SortDirection" /> dependency property.</returns>
	public static readonly DependencyProperty SortDirectionProperty;

	private static readonly DependencyPropertyKey IsFrozenPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DataGridColumnHeader.IsFrozen" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DataGridColumnHeader.IsFrozen" /> dependency property.</returns>
	public static readonly DependencyProperty IsFrozenProperty;

	private DataGridColumn _column;

	private ContainerTracking<DataGridColumnHeader> _tracker;

	private DataGridColumnHeadersPresenter _parentPresenter;

	private Thumb _leftGripper;

	private Thumb _rightGripper;

	private bool _suppressClickEvent;

	private const string LeftHeaderGripperTemplateName = "PART_LeftHeaderGripper";

	private const string RightHeaderGripperTemplateName = "PART_RightHeaderGripper";

	/// <summary>Gets the <see cref="T:System.Windows.Controls.DataGridColumn" /> associated with this column header.</summary>
	/// <returns>The column associated with this column header.</returns>
	public DataGridColumn Column => _column;

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> used to paint the column header separator lines. </summary>
	/// <returns>The brush used to paint column header separator lines. </returns>
	public Brush SeparatorBrush
	{
		get
		{
			return (Brush)GetValue(SeparatorBrushProperty);
		}
		set
		{
			SetValue(SeparatorBrushProperty, value);
		}
	}

	/// <summary>Gets or sets the user interface (UI) visibility of the column header separator lines. </summary>
	/// <returns>The UI visibility of the column header separator lines. The default is <see cref="F:System.Windows.Visibility.Visible" />.</returns>
	public Visibility SeparatorVisibility
	{
		get
		{
			return (Visibility)GetValue(SeparatorVisibilityProperty);
		}
		set
		{
			SetValue(SeparatorVisibilityProperty, value);
		}
	}

	internal ContainerTracking<DataGridColumnHeader> Tracker => _tracker;

	/// <summary>Gets the display position of the column associated with this column header relative to the other columns in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>The display position of associated column relative to the other columns in the <see cref="T:System.Windows.Controls.DataGrid" />.</returns>
	public int DisplayIndex => (int)GetValue(DisplayIndexProperty);

	private DataGridLength ColumnWidth
	{
		get
		{
			if (Column == null)
			{
				return DataGridLength.Auto;
			}
			return Column.Width;
		}
	}

	private double ColumnActualWidth
	{
		get
		{
			if (Column == null)
			{
				return base.ActualWidth;
			}
			return Column.ActualWidth;
		}
	}

	/// <summary>Gets a value that indicates whether the user can click this column header to sort the <see cref="T:System.Windows.Controls.DataGrid" /> by the associated column.</summary>
	/// <returns>true if the user can click this column header to sort the <see cref="T:System.Windows.Controls.DataGrid" /> by the associated column; otherwise, false. </returns>
	public bool CanUserSort => (bool)GetValue(CanUserSortProperty);

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.DataGrid" /> is sorted by the associated column and whether the column values are in ascending or descending order.</summary>
	/// <returns>The sort direction of the column or null if unsorted.</returns>
	public ListSortDirection? SortDirection => (ListSortDirection?)GetValue(SortDirectionProperty);

	/// <summary>Gets a value that indicates whether the column associated with this column header is prevented from scrolling horizontally.</summary>
	/// <returns>true if the associated column is prevented from scrolling horizontally; otherwise, false.</returns>
	public bool IsFrozen => (bool)GetValue(IsFrozenProperty);

	internal DataGridColumnHeadersPresenter ParentPresenter
	{
		get
		{
			if (_parentPresenter == null)
			{
				_parentPresenter = ItemsControl.ItemsControlFromItemContainer(this) as DataGridColumnHeadersPresenter;
			}
			return _parentPresenter;
		}
	}

	internal bool SuppressClickEvent
	{
		get
		{
			return _suppressClickEvent;
		}
		set
		{
			_suppressClickEvent = value;
		}
	}

	/// <summary>Gets the key that references the style for the drop location indicator during a header drag operation. </summary>
	/// <returns>The style key for the drop location indicator. </returns>
	public static ComponentResourceKey ColumnHeaderDropSeparatorStyleKey => SystemResourceKey.DataGridColumnHeaderColumnHeaderDropSeparatorStyleKey;

	/// <summary>Gets the key that references the style for displaying column headers during a header drag operation. </summary>
	/// <returns>The style key for floating column headers. </returns>
	public static ComponentResourceKey ColumnFloatingHeaderStyleKey => SystemResourceKey.DataGridColumnHeaderColumnFloatingHeaderStyleKey;

	DataGridColumn IProvideDataGridColumn.Column => _column;

	private Panel ParentPanel => base.VisualParent as Panel;

	private DataGridColumnHeader PreviousVisibleHeader
	{
		get
		{
			DataGridColumn column = Column;
			if (column != null)
			{
				DataGrid dataGridOwner = column.DataGridOwner;
				if (dataGridOwner != null)
				{
					for (int num = DisplayIndex - 1; num >= 0; num--)
					{
						if (dataGridOwner.ColumnFromDisplayIndex(num).IsVisible)
						{
							return dataGridOwner.ColumnHeaderFromDisplayIndex(num);
						}
					}
				}
			}
			return null;
		}
	}

	static DataGridColumnHeader()
	{
		SeparatorBrushProperty = DependencyProperty.Register("SeparatorBrush", typeof(Brush), typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(null));
		SeparatorVisibilityProperty = DependencyProperty.Register("SeparatorVisibility", typeof(Visibility), typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(Visibility.Visible));
		DisplayIndexPropertyKey = DependencyProperty.RegisterReadOnly("DisplayIndex", typeof(int), typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(-1, OnDisplayIndexChanged, OnCoerceDisplayIndex));
		DisplayIndexProperty = DisplayIndexPropertyKey.DependencyProperty;
		CanUserSortPropertyKey = DependencyProperty.RegisterReadOnly("CanUserSort", typeof(bool), typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(true, null, OnCoerceCanUserSort));
		CanUserSortProperty = CanUserSortPropertyKey.DependencyProperty;
		SortDirectionPropertyKey = DependencyProperty.RegisterReadOnly("SortDirection", typeof(ListSortDirection?), typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(null, Control.OnVisualStatePropertyChanged, OnCoerceSortDirection));
		SortDirectionProperty = SortDirectionPropertyKey.DependencyProperty;
		IsFrozenPropertyKey = DependencyProperty.RegisterReadOnly("IsFrozen", typeof(bool), typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(false, null, OnCoerceIsFrozen));
		IsFrozenProperty = IsFrozenPropertyKey.DependencyProperty;
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(typeof(DataGridColumnHeader)));
		ContentControl.ContentProperty.OverrideMetadata(typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(OnNotifyPropertyChanged, OnCoerceContent));
		ContentControl.ContentTemplateProperty.OverrideMetadata(typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(null, OnNotifyPropertyChanged, OnCoerceContentTemplate));
		ContentControl.ContentTemplateSelectorProperty.OverrideMetadata(typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(null, OnNotifyPropertyChanged, OnCoerceContentTemplateSelector));
		ContentControl.ContentStringFormatProperty.OverrideMetadata(typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(null, OnNotifyPropertyChanged, OnCoerceStringFormat));
		FrameworkElement.StyleProperty.OverrideMetadata(typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(null, OnNotifyPropertyChanged, OnCoerceStyle));
		FrameworkElement.HeightProperty.OverrideMetadata(typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(OnNotifyPropertyChanged, OnCoerceHeight));
		UIElement.FocusableProperty.OverrideMetadata(typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(false));
		UIElement.ClipProperty.OverrideMetadata(typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(null, OnCoerceClip));
		AutomationProperties.IsOffscreenBehaviorProperty.OverrideMetadata(typeof(DataGridColumnHeader), new FrameworkPropertyMetadata(IsOffscreenBehavior.FromClip));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.DataGridColumnHeader" /> class. </summary>
	public DataGridColumnHeader()
	{
		_tracker = new ContainerTracking<DataGridColumnHeader>(this);
	}

	internal void PrepareColumnHeader(object item, DataGridColumn column)
	{
		_column = column;
		base.TabIndex = column.DisplayIndex;
		DataGridHelper.TransferProperty(this, ContentControl.ContentProperty);
		DataGridHelper.TransferProperty(this, ContentControl.ContentTemplateProperty);
		DataGridHelper.TransferProperty(this, ContentControl.ContentTemplateSelectorProperty);
		DataGridHelper.TransferProperty(this, ContentControl.ContentStringFormatProperty);
		DataGridHelper.TransferProperty(this, FrameworkElement.StyleProperty);
		DataGridHelper.TransferProperty(this, FrameworkElement.HeightProperty);
		CoerceValue(CanUserSortProperty);
		CoerceValue(SortDirectionProperty);
		CoerceValue(IsFrozenProperty);
		CoerceValue(UIElement.ClipProperty);
		CoerceValue(DisplayIndexProperty);
	}

	internal void ClearHeader()
	{
		_column = null;
	}

	private static object OnCoerceDisplayIndex(DependencyObject d, object baseValue)
	{
		DataGridColumn column = ((DataGridColumnHeader)d).Column;
		if (column != null)
		{
			return column.DisplayIndex;
		}
		return -1;
	}

	private static void OnDisplayIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGridColumnHeader dataGridColumnHeader = (DataGridColumnHeader)d;
		DataGridColumn column = dataGridColumnHeader.Column;
		if (column != null)
		{
			DataGrid dataGridOwner = column.DataGridOwner;
			if (dataGridOwner != null)
			{
				dataGridColumnHeader.SetLeftGripperVisibility();
				dataGridOwner.ColumnHeaderFromDisplayIndex(dataGridColumnHeader.DisplayIndex + 1)?.SetLeftGripperVisibility(column.CanUserResize);
			}
		}
	}

	/// <summary>Builds the visual tree for the column header when a new template is applied. </summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		HookupGripperEvents();
	}

	private void HookupGripperEvents()
	{
		UnhookGripperEvents();
		_leftGripper = GetTemplateChild("PART_LeftHeaderGripper") as Thumb;
		_rightGripper = GetTemplateChild("PART_RightHeaderGripper") as Thumb;
		if (_leftGripper != null)
		{
			_leftGripper.DragStarted += OnColumnHeaderGripperDragStarted;
			_leftGripper.DragDelta += OnColumnHeaderResize;
			_leftGripper.DragCompleted += OnColumnHeaderGripperDragCompleted;
			_leftGripper.MouseDoubleClick += OnGripperDoubleClicked;
			SetLeftGripperVisibility();
		}
		if (_rightGripper != null)
		{
			_rightGripper.DragStarted += OnColumnHeaderGripperDragStarted;
			_rightGripper.DragDelta += OnColumnHeaderResize;
			_rightGripper.DragCompleted += OnColumnHeaderGripperDragCompleted;
			_rightGripper.MouseDoubleClick += OnGripperDoubleClicked;
			SetRightGripperVisibility();
		}
	}

	private void UnhookGripperEvents()
	{
		if (_leftGripper != null)
		{
			_leftGripper.DragStarted -= OnColumnHeaderGripperDragStarted;
			_leftGripper.DragDelta -= OnColumnHeaderResize;
			_leftGripper.DragCompleted -= OnColumnHeaderGripperDragCompleted;
			_leftGripper.MouseDoubleClick -= OnGripperDoubleClicked;
			_leftGripper = null;
		}
		if (_rightGripper != null)
		{
			_rightGripper.DragStarted -= OnColumnHeaderGripperDragStarted;
			_rightGripper.DragDelta -= OnColumnHeaderResize;
			_rightGripper.DragCompleted -= OnColumnHeaderGripperDragCompleted;
			_rightGripper.MouseDoubleClick -= OnGripperDoubleClicked;
			_rightGripper = null;
		}
	}

	private DataGridColumnHeader HeaderToResize(object gripper)
	{
		if (gripper != _rightGripper)
		{
			return PreviousVisibleHeader;
		}
		return this;
	}

	private void OnColumnHeaderGripperDragStarted(object sender, DragStartedEventArgs e)
	{
		DataGridColumnHeader dataGridColumnHeader = HeaderToResize(sender);
		if (dataGridColumnHeader != null)
		{
			if (dataGridColumnHeader.Column != null)
			{
				dataGridColumnHeader.Column.DataGridOwner?.InternalColumns.OnColumnResizeStarted();
			}
			e.Handled = true;
		}
	}

	private void OnColumnHeaderResize(object sender, DragDeltaEventArgs e)
	{
		DataGridColumnHeader dataGridColumnHeader = HeaderToResize(sender);
		if (dataGridColumnHeader != null)
		{
			RecomputeColumnWidthsOnColumnResize(dataGridColumnHeader, e.HorizontalChange);
			e.Handled = true;
		}
	}

	private static void RecomputeColumnWidthsOnColumnResize(DataGridColumnHeader header, double horizontalChange)
	{
		DataGridColumn column = header.Column;
		column?.DataGridOwner?.InternalColumns.RecomputeColumnWidthsOnColumnResize(column, horizontalChange, retainAuto: false);
	}

	private void OnColumnHeaderGripperDragCompleted(object sender, DragCompletedEventArgs e)
	{
		DataGridColumnHeader dataGridColumnHeader = HeaderToResize(sender);
		if (dataGridColumnHeader != null)
		{
			if (dataGridColumnHeader.Column != null)
			{
				dataGridColumnHeader.Column.DataGridOwner?.InternalColumns.OnColumnResizeCompleted(e.Canceled);
			}
			e.Handled = true;
		}
	}

	private void OnGripperDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		DataGridColumnHeader dataGridColumnHeader = HeaderToResize(sender);
		if (dataGridColumnHeader != null && dataGridColumnHeader.Column != null)
		{
			dataGridColumnHeader.Column.Width = DataGridLength.Auto;
			e.Handled = true;
		}
	}

	private static void OnNotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGridColumnHeader)d).NotifyPropertyChanged(d, e);
	}

	internal void NotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGridColumn dataGridColumn = d as DataGridColumn;
		if (dataGridColumn == null || dataGridColumn == Column)
		{
			if (e.Property == DataGridColumn.WidthProperty)
			{
				DataGridHelper.OnColumnWidthChanged(this, e);
			}
			else if (e.Property == DataGridColumn.HeaderProperty || e.Property == ContentControl.ContentProperty)
			{
				DataGridHelper.TransferProperty(this, ContentControl.ContentProperty);
			}
			else if (e.Property == DataGridColumn.HeaderTemplateProperty || e.Property == ContentControl.ContentTemplateProperty)
			{
				DataGridHelper.TransferProperty(this, ContentControl.ContentTemplateProperty);
			}
			else if (e.Property == DataGridColumn.HeaderTemplateSelectorProperty || e.Property == ContentControl.ContentTemplateSelectorProperty)
			{
				DataGridHelper.TransferProperty(this, ContentControl.ContentTemplateSelectorProperty);
			}
			else if (e.Property == DataGridColumn.HeaderStringFormatProperty || e.Property == ContentControl.ContentStringFormatProperty)
			{
				DataGridHelper.TransferProperty(this, ContentControl.ContentStringFormatProperty);
			}
			else if (e.Property == DataGrid.ColumnHeaderStyleProperty || e.Property == DataGridColumn.HeaderStyleProperty || e.Property == FrameworkElement.StyleProperty)
			{
				DataGridHelper.TransferProperty(this, FrameworkElement.StyleProperty);
			}
			else if (e.Property == DataGrid.ColumnHeaderHeightProperty || e.Property == FrameworkElement.HeightProperty)
			{
				DataGridHelper.TransferProperty(this, FrameworkElement.HeightProperty);
			}
			else if (e.Property == DataGridColumn.DisplayIndexProperty)
			{
				CoerceValue(DisplayIndexProperty);
				base.TabIndex = dataGridColumn.DisplayIndex;
			}
			else if (e.Property == DataGrid.CanUserResizeColumnsProperty)
			{
				OnCanUserResizeColumnsChanged();
			}
			else if (e.Property == DataGridColumn.CanUserSortProperty)
			{
				CoerceValue(CanUserSortProperty);
			}
			else if (e.Property == DataGridColumn.SortDirectionProperty)
			{
				CoerceValue(SortDirectionProperty);
			}
			else if (e.Property == DataGridColumn.IsFrozenProperty)
			{
				CoerceValue(IsFrozenProperty);
			}
			else if (e.Property == DataGridColumn.CanUserResizeProperty)
			{
				OnCanUserResizeChanged();
			}
			else if (e.Property == DataGridColumn.VisibilityProperty)
			{
				OnColumnVisibilityChanged(e);
			}
		}
	}

	private void OnCanUserResizeColumnsChanged()
	{
		if (Column.DataGridOwner != null)
		{
			SetLeftGripperVisibility();
			SetRightGripperVisibility();
		}
	}

	private void OnCanUserResizeChanged()
	{
		if (Column.DataGridOwner != null)
		{
			SetNextHeaderLeftGripperVisibility(Column.CanUserResize);
			SetRightGripperVisibility();
		}
	}

	private void SetLeftGripperVisibility()
	{
		if (_leftGripper == null || Column == null)
		{
			return;
		}
		DataGrid dataGridOwner = Column.DataGridOwner;
		bool leftGripperVisibility = false;
		for (int num = DisplayIndex - 1; num >= 0; num--)
		{
			DataGridColumn dataGridColumn = dataGridOwner.ColumnFromDisplayIndex(num);
			if (dataGridColumn.IsVisible)
			{
				leftGripperVisibility = dataGridColumn.CanUserResize;
				break;
			}
		}
		SetLeftGripperVisibility(leftGripperVisibility);
	}

	private void SetLeftGripperVisibility(bool canPreviousColumnResize)
	{
		if (_leftGripper != null && Column != null)
		{
			if ((Column.DataGridOwner?.CanUserResizeColumns ?? false) && canPreviousColumnResize)
			{
				_leftGripper.Visibility = Visibility.Visible;
			}
			else
			{
				_leftGripper.Visibility = Visibility.Collapsed;
			}
		}
	}

	private void SetRightGripperVisibility()
	{
		if (_rightGripper != null && Column != null)
		{
			DataGrid dataGridOwner = Column.DataGridOwner;
			if (dataGridOwner != null && dataGridOwner.CanUserResizeColumns && Column.CanUserResize)
			{
				_rightGripper.Visibility = Visibility.Visible;
			}
			else
			{
				_rightGripper.Visibility = Visibility.Collapsed;
			}
		}
	}

	private void SetNextHeaderLeftGripperVisibility(bool canUserResize)
	{
		DataGrid dataGridOwner = Column.DataGridOwner;
		int count = dataGridOwner.Columns.Count;
		for (int i = DisplayIndex + 1; i < count; i++)
		{
			if (dataGridOwner.ColumnFromDisplayIndex(i).IsVisible)
			{
				dataGridOwner.ColumnHeaderFromDisplayIndex(i)?.SetLeftGripperVisibility(canUserResize);
				break;
			}
		}
	}

	private void OnColumnVisibilityChanged(DependencyPropertyChangedEventArgs e)
	{
		DataGrid dataGridOwner = Column.DataGridOwner;
		if (dataGridOwner == null)
		{
			return;
		}
		bool num = (Visibility)e.OldValue == Visibility.Visible;
		bool flag = (Visibility)e.NewValue == Visibility.Visible;
		if (num == flag)
		{
			return;
		}
		if (flag)
		{
			SetLeftGripperVisibility();
			SetRightGripperVisibility();
			SetNextHeaderLeftGripperVisibility(Column.CanUserResize);
			return;
		}
		bool nextHeaderLeftGripperVisibility = false;
		for (int num2 = DisplayIndex - 1; num2 >= 0; num2--)
		{
			DataGridColumn dataGridColumn = dataGridOwner.ColumnFromDisplayIndex(num2);
			if (dataGridColumn.IsVisible)
			{
				nextHeaderLeftGripperVisibility = dataGridColumn.CanUserResize;
				break;
			}
		}
		SetNextHeaderLeftGripperVisibility(nextHeaderLeftGripperVisibility);
	}

	private static object OnCoerceContent(DependencyObject d, object baseValue)
	{
		DataGridColumnHeader dataGridColumnHeader = d as DataGridColumnHeader;
		object coercedTransferPropertyValue = DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumnHeader, baseValue, ContentControl.ContentProperty, dataGridColumnHeader.Column, DataGridColumn.HeaderProperty);
		FrameworkObject frameworkObject = new FrameworkObject(coercedTransferPropertyValue as DependencyObject);
		if (frameworkObject.Parent != null && frameworkObject.Parent != dataGridColumnHeader)
		{
			frameworkObject.ChangeLogicalParent(null);
		}
		return coercedTransferPropertyValue;
	}

	private static object OnCoerceContentTemplate(DependencyObject d, object baseValue)
	{
		DataGridColumnHeader dataGridColumnHeader = d as DataGridColumnHeader;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumnHeader, baseValue, ContentControl.ContentTemplateProperty, dataGridColumnHeader.Column, DataGridColumn.HeaderTemplateProperty);
	}

	private static object OnCoerceContentTemplateSelector(DependencyObject d, object baseValue)
	{
		DataGridColumnHeader dataGridColumnHeader = d as DataGridColumnHeader;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumnHeader, baseValue, ContentControl.ContentTemplateSelectorProperty, dataGridColumnHeader.Column, DataGridColumn.HeaderTemplateSelectorProperty);
	}

	private static object OnCoerceStringFormat(DependencyObject d, object baseValue)
	{
		DataGridColumnHeader dataGridColumnHeader = d as DataGridColumnHeader;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumnHeader, baseValue, ContentControl.ContentStringFormatProperty, dataGridColumnHeader.Column, DataGridColumn.HeaderStringFormatProperty);
	}

	private static object OnCoerceStyle(DependencyObject d, object baseValue)
	{
		DataGridColumnHeader dataGridColumnHeader = (DataGridColumnHeader)d;
		DataGridColumn column = dataGridColumnHeader.Column;
		DataGrid grandParentObject = null;
		if (column == null)
		{
			if (dataGridColumnHeader.TemplatedParent is DataGridColumnHeadersPresenter dataGridColumnHeadersPresenter)
			{
				grandParentObject = dataGridColumnHeadersPresenter.ParentDataGrid;
			}
		}
		else
		{
			grandParentObject = column.DataGridOwner;
		}
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumnHeader, baseValue, FrameworkElement.StyleProperty, column, DataGridColumn.HeaderStyleProperty, grandParentObject, DataGrid.ColumnHeaderStyleProperty);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> event and initiates sorting. </summary>
	protected override void OnClick()
	{
		if (!SuppressClickEvent)
		{
			if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
			{
				UIElementAutomationPeer.CreatePeerForElement(this)?.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
			}
			base.OnClick();
			if (Column != null && Column.DataGridOwner != null)
			{
				Column.DataGridOwner.PerformSort(Column);
			}
		}
	}

	private static object OnCoerceHeight(DependencyObject d, object baseValue)
	{
		DataGridColumnHeader dataGridColumnHeader = (DataGridColumnHeader)d;
		DataGridColumn column = dataGridColumnHeader.Column;
		DataGrid parentObject = null;
		if (column == null)
		{
			if (dataGridColumnHeader.TemplatedParent is DataGridColumnHeadersPresenter dataGridColumnHeadersPresenter)
			{
				parentObject = dataGridColumnHeadersPresenter.ParentDataGrid;
			}
		}
		else
		{
			parentObject = column.DataGridOwner;
		}
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumnHeader, baseValue, FrameworkElement.HeightProperty, parentObject, DataGrid.ColumnHeaderHeightProperty);
	}

	private static object OnCoerceCanUserSort(DependencyObject d, object baseValue)
	{
		DataGridColumn column = ((DataGridColumnHeader)d).Column;
		if (column != null)
		{
			return column.CanUserSort;
		}
		return baseValue;
	}

	private static object OnCoerceSortDirection(DependencyObject d, object baseValue)
	{
		DataGridColumn column = ((DataGridColumnHeader)d).Column;
		if (column != null)
		{
			return column.SortDirection;
		}
		return baseValue;
	}

	/// <summary>Returns a new <see cref="T:System.Windows.Automation.Peers.DataGridColumnHeaderAutomationPeer" /> for this column header.</summary>
	/// <returns>A new automation peer for this column header.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DataGridColumnHeaderAutomationPeer(this);
	}

	internal void Invoke()
	{
		OnClick();
	}

	private static object OnCoerceIsFrozen(DependencyObject d, object baseValue)
	{
		DataGridColumn column = ((DataGridColumnHeader)d).Column;
		if (column != null)
		{
			return column.IsFrozen;
		}
		return baseValue;
	}

	private static object OnCoerceClip(DependencyObject d, object baseValue)
	{
		DataGridColumnHeader cell = (DataGridColumnHeader)d;
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

	/// <param name="e">The event data. </param>
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonDown(e);
		DataGridColumnHeadersPresenter parentPresenter = ParentPresenter;
		if (parentPresenter != null)
		{
			if (base.ClickMode == ClickMode.Hover && e.ButtonState == MouseButtonState.Pressed)
			{
				CaptureMouse();
			}
			parentPresenter.OnHeaderMouseLeftButtonDown(e);
			e.Handled = true;
		}
	}

	/// <param name="e">The event data.</param>
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		DataGridColumnHeadersPresenter parentPresenter = ParentPresenter;
		if (parentPresenter != null)
		{
			parentPresenter.OnHeaderMouseMove(e);
			e.Handled = true;
		}
	}

	/// <param name="e">The event data.</param>
	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonUp(e);
		DataGridColumnHeadersPresenter parentPresenter = ParentPresenter;
		if (parentPresenter != null)
		{
			if (base.ClickMode == ClickMode.Hover && base.IsMouseCaptured)
			{
				ReleaseMouseCapture();
			}
			parentPresenter.OnHeaderMouseLeftButtonUp(e);
			e.Handled = true;
		}
	}

	/// <param name="e">The event data for the <see cref="E:System.Windows.Input.Mouse.LostMouseCapture" /> event.</param>
	protected override void OnLostMouseCapture(MouseEventArgs e)
	{
		base.OnLostMouseCapture(e);
		DataGridColumnHeadersPresenter parentPresenter = ParentPresenter;
		if (parentPresenter != null)
		{
			parentPresenter.OnHeaderLostMouseCapture(e);
			e.Handled = true;
		}
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (base.IsPressed)
		{
			VisualStates.GoToState(this, useTransitions, "Pressed", "MouseOver", "Normal");
		}
		else if (base.IsMouseOver)
		{
			VisualStates.GoToState(this, useTransitions, "MouseOver", "Normal");
		}
		else
		{
			VisualStateManager.GoToState(this, "Normal", useTransitions);
		}
		ListSortDirection? sortDirection = SortDirection;
		if (sortDirection.HasValue)
		{
			if (sortDirection == ListSortDirection.Ascending)
			{
				VisualStates.GoToState(this, useTransitions, "SortAscending", "Unsorted");
			}
			if (sortDirection == ListSortDirection.Descending)
			{
				VisualStates.GoToState(this, useTransitions, "SortDescending", "Unsorted");
			}
		}
		else
		{
			VisualStateManager.GoToState(this, "Unsorted", useTransitions);
		}
		ChangeValidationVisualState(useTransitions);
	}
}
