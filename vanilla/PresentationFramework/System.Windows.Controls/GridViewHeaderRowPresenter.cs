using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Represents an object that is used to define the layout of a row of column headers. </summary>
[StyleTypedProperty(Property = "ColumnHeaderContainerStyle", StyleTargetType = typeof(GridViewColumnHeader))]
public class GridViewHeaderRowPresenter : GridViewRowPresenterBase
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewHeaderRowPresenter.ColumnHeaderContainerStyle" /> dependency property. </summary>
	public static readonly DependencyProperty ColumnHeaderContainerStyleProperty = GridView.ColumnHeaderContainerStyleProperty.AddOwner(typeof(GridViewHeaderRowPresenter), new FrameworkPropertyMetadata(PropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewHeaderRowPresenter.ColumnHeaderTemplate" /> dependency property. </summary>
	public static readonly DependencyProperty ColumnHeaderTemplateProperty = GridView.ColumnHeaderTemplateProperty.AddOwner(typeof(GridViewHeaderRowPresenter), new FrameworkPropertyMetadata(PropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewHeaderRowPresenter.ColumnHeaderTemplateSelector" /> dependency property.</summary>
	public static readonly DependencyProperty ColumnHeaderTemplateSelectorProperty = GridView.ColumnHeaderTemplateSelectorProperty.AddOwner(typeof(GridViewHeaderRowPresenter), new FrameworkPropertyMetadata(PropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewHeaderRowPresenter.ColumnHeaderStringFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.GridViewHeaderRowPresenter.ColumnHeaderStringFormat" /> dependency property.</returns>
	public static readonly DependencyProperty ColumnHeaderStringFormatProperty = GridView.ColumnHeaderStringFormatProperty.AddOwner(typeof(GridViewHeaderRowPresenter), new FrameworkPropertyMetadata(PropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewHeaderRowPresenter.AllowsColumnReorder" /> dependency property. </summary>
	public static readonly DependencyProperty AllowsColumnReorderProperty = GridView.AllowsColumnReorderProperty.AddOwner(typeof(GridViewHeaderRowPresenter));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewHeaderRowPresenter.ColumnHeaderContextMenu" /> dependency property. </summary>
	public static readonly DependencyProperty ColumnHeaderContextMenuProperty = GridView.ColumnHeaderContextMenuProperty.AddOwner(typeof(GridViewHeaderRowPresenter), new FrameworkPropertyMetadata(PropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewHeaderRowPresenter.ColumnHeaderToolTip" /> dependency property. </summary>
	public static readonly DependencyProperty ColumnHeaderToolTipProperty = GridView.ColumnHeaderToolTipProperty.AddOwner(typeof(GridViewHeaderRowPresenter), new FrameworkPropertyMetadata(PropertyChanged));

	private bool _gvHeadersValid;

	private List<GridViewColumnHeader> _gvHeaders;

	private List<Rect> _headersPositionList;

	private ScrollViewer _mainSV;

	private ScrollViewer _headerSV;

	private GridViewColumnHeader _paddingHeader;

	private GridViewColumnHeader _floatingHeader;

	private Separator _indicator;

	private ItemsControl _itemsControl;

	private GridViewColumnHeader _draggingSrcHeader;

	private Point _startPos;

	private Point _relativeStartPos;

	private Point _currentPos;

	private int _startColumnIndex;

	private int _desColumnIndex;

	private bool _isHeaderDragging;

	private bool _isColumnChangedOrCreated;

	private bool _prepareDragging;

	private const double c_thresholdX = 4.0;

	private static readonly DependencyProperty[][] s_DPList = new DependencyProperty[3][]
	{
		new DependencyProperty[6] { ColumnHeaderContainerStyleProperty, ColumnHeaderTemplateProperty, ColumnHeaderTemplateSelectorProperty, ColumnHeaderStringFormatProperty, ColumnHeaderContextMenuProperty, ColumnHeaderToolTipProperty },
		new DependencyProperty[6]
		{
			GridViewColumn.HeaderContainerStyleProperty,
			GridViewColumn.HeaderTemplateProperty,
			GridViewColumn.HeaderTemplateSelectorProperty,
			GridViewColumn.HeaderStringFormatProperty,
			null,
			null
		},
		new DependencyProperty[6]
		{
			FrameworkElement.StyleProperty,
			ContentControl.ContentTemplateProperty,
			ContentControl.ContentTemplateSelectorProperty,
			ContentControl.ContentStringFormatProperty,
			FrameworkElement.ContextMenuProperty,
			FrameworkElement.ToolTipProperty
		}
	};

	/// <summary>Gets or sets the <see cref="T:System.Windows.Style" /> to use for the column headers. </summary>
	/// <returns>The <see cref="T:System.Windows.Style" /> to use for the column header container. The default is null.</returns>
	public Style ColumnHeaderContainerStyle
	{
		get
		{
			return (Style)GetValue(ColumnHeaderContainerStyleProperty);
		}
		set
		{
			SetValue(ColumnHeaderContainerStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the template to use to display the column headers. </summary>
	/// <returns>The <see cref="T:System.Windows.DataTemplate" /> that is used to display the column header content. The default is null.</returns>
	public DataTemplate ColumnHeaderTemplate
	{
		get
		{
			return (DataTemplate)GetValue(ColumnHeaderTemplateProperty);
		}
		set
		{
			SetValue(ColumnHeaderTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Controls.DataTemplateSelector" /> that provides logic that selects the data template to use to display a column header. </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.DataTemplateSelector" /> that chooses the <see cref="T:System.Windows.DataTemplate" /> to use to display each column header. The default is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public DataTemplateSelector ColumnHeaderTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(ColumnHeaderTemplateSelectorProperty);
		}
		set
		{
			SetValue(ColumnHeaderTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets a composite string that specifies how to format the column headers if they are displayed as strings.</summary>
	/// <returns>A composite string that specifies how to format the column headers if they are displayed as strings. The default is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public string ColumnHeaderStringFormat
	{
		get
		{
			return (string)GetValue(ColumnHeaderStringFormatProperty);
		}
		set
		{
			SetValue(ColumnHeaderStringFormatProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether columns can change positions. </summary>
	/// <returns>true if columns can be moved by the drag-and-drop operation of a column header; otherwise, false. The default is true.</returns>
	public bool AllowsColumnReorder
	{
		get
		{
			return (bool)GetValue(AllowsColumnReorderProperty);
		}
		set
		{
			SetValue(AllowsColumnReorderProperty, value);
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Controls.ContextMenu" /> for the column headers.   </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ContextMenu" /> for the column header row. The default is null.</returns>
	public ContextMenu ColumnHeaderContextMenu
	{
		get
		{
			return (ContextMenu)GetValue(ColumnHeaderContextMenuProperty);
		}
		set
		{
			SetValue(ColumnHeaderContextMenuProperty, value);
		}
	}

	/// <summary>Gets or sets the content for a tooltip for the column header row. </summary>
	/// <returns>An object that represents the content of a tooltip for the column headers.</returns>
	public object ColumnHeaderToolTip
	{
		get
		{
			return GetValue(ColumnHeaderToolTipProperty);
		}
		set
		{
			SetValue(ColumnHeaderToolTipProperty, value);
		}
	}

	internal List<GridViewColumnHeader> ActualColumnHeaders
	{
		get
		{
			if (_gvHeaders == null || !_gvHeadersValid)
			{
				_gvHeadersValid = true;
				_gvHeaders = new List<GridViewColumnHeader>();
				if (base.Columns != null)
				{
					UIElementCollection internalChildren = base.InternalChildren;
					int i = 0;
					for (int count = base.Columns.Count; i < count; i++)
					{
						if (internalChildren[GetVisualIndex(i)] is GridViewColumnHeader item)
						{
							_gvHeaders.Add(item);
						}
					}
				}
			}
			return _gvHeaders;
		}
	}

	private List<Rect> HeadersPositionList
	{
		get
		{
			if (_headersPositionList == null)
			{
				_headersPositionList = new List<Rect>();
			}
			return _headersPositionList;
		}
	}

	private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		GridViewHeaderRowPresenter gridViewHeaderRowPresenter = (GridViewHeaderRowPresenter)d;
		if (e.Property == ColumnHeaderTemplateProperty || e.Property == ColumnHeaderTemplateSelectorProperty)
		{
			Helper.CheckTemplateAndTemplateSelector("GridViewHeaderRowPresenter", ColumnHeaderTemplateProperty, ColumnHeaderTemplateSelectorProperty, gridViewHeaderRowPresenter);
		}
		gridViewHeaderRowPresenter.UpdateAllHeaders(e.Property);
	}

	/// <summary>Determines the area that is required to display the column header row.</summary>
	/// <returns>The required <see cref="T:System.Windows.Size" /> for the column header row.</returns>
	/// <param name="constraint">The amount of area that is available to display the column header row.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		GridViewColumnCollection columns = base.Columns;
		UIElementCollection internalChildren = base.InternalChildren;
		double val = 0.0;
		double num = 0.0;
		double height = constraint.Height;
		bool flag = false;
		if (columns != null)
		{
			for (int i = 0; i < columns.Count; i++)
			{
				UIElement uIElement = internalChildren[GetVisualIndex(i)];
				if (uIElement == null)
				{
					continue;
				}
				double num2 = Math.Max(0.0, constraint.Width - num);
				GridViewColumn gridViewColumn = columns[i];
				if (gridViewColumn.State == ColumnMeasureState.Init)
				{
					if (!flag)
					{
						EnsureDesiredWidthList();
						base.LayoutUpdated += OnLayoutUpdated;
						flag = true;
					}
					uIElement.Measure(new Size(num2, height));
					base.DesiredWidthList[gridViewColumn.ActualIndex] = gridViewColumn.EnsureWidth(uIElement.DesiredSize.Width);
					num += gridViewColumn.DesiredWidth;
				}
				else if (gridViewColumn.State == ColumnMeasureState.Headered || gridViewColumn.State == ColumnMeasureState.Data)
				{
					num2 = Math.Min(num2, gridViewColumn.DesiredWidth);
					uIElement.Measure(new Size(num2, height));
					num += gridViewColumn.DesiredWidth;
				}
				else
				{
					num2 = Math.Min(num2, gridViewColumn.Width);
					uIElement.Measure(new Size(num2, height));
					num += gridViewColumn.Width;
				}
				val = Math.Max(val, uIElement.DesiredSize.Height);
			}
		}
		_paddingHeader.Measure(new Size(0.0, height));
		val = Math.Max(val, _paddingHeader.DesiredSize.Height);
		num += 2.0;
		if (_isHeaderDragging)
		{
			_indicator.Measure(constraint);
			_floatingHeader.Measure(constraint);
		}
		return new Size(num, val);
	}

	/// <summary>Arranges the content of the header row elements, and computes the actual size of the header row.</summary>
	/// <returns>The actual <see cref="T:System.Windows.Size" /> for the column header row.</returns>
	/// <param name="arrangeSize">The area that is available for the column header row.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		GridViewColumnCollection columns = base.Columns;
		UIElementCollection internalChildren = base.InternalChildren;
		double num = 0.0;
		double num2 = arrangeSize.Width;
		HeadersPositionList.Clear();
		Rect rect;
		if (columns != null)
		{
			for (int i = 0; i < columns.Count; i++)
			{
				UIElement uIElement = internalChildren[GetVisualIndex(i)];
				if (uIElement != null)
				{
					GridViewColumn gridViewColumn = columns[i];
					double num3 = Math.Min(num2, (gridViewColumn.State == ColumnMeasureState.SpecificWidth) ? gridViewColumn.Width : gridViewColumn.DesiredWidth);
					rect = new Rect(num, 0.0, num3, arrangeSize.Height);
					uIElement.Arrange(rect);
					HeadersPositionList.Add(rect);
					num2 -= num3;
					num += num3;
				}
			}
			if (_isColumnChangedOrCreated)
			{
				for (int j = 0; j < columns.Count; j++)
				{
					(internalChildren[GetVisualIndex(j)] as GridViewColumnHeader).CheckWidthForPreviousHeaderGripper();
				}
				_paddingHeader.CheckWidthForPreviousHeaderGripper();
				_isColumnChangedOrCreated = false;
			}
		}
		rect = new Rect(num, 0.0, Math.Max(num2, 0.0), arrangeSize.Height);
		_paddingHeader.Arrange(rect);
		HeadersPositionList.Add(rect);
		if (_isHeaderDragging)
		{
			_floatingHeader.Arrange(new Rect(new Point(_currentPos.X - _relativeStartPos.X, 0.0), HeadersPositionList[_startColumnIndex].Size));
			Point location = FindPositionByIndex(_desColumnIndex);
			_indicator.Arrange(new Rect(location, new Size(_indicator.DesiredSize.Width, arrangeSize.Height)));
		}
		return arrangeSize;
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" /> event that occurs when the user presses the left mouse button inside a <see cref="T:System.Windows.Controls.GridViewColumnHeader" />. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		if (e.Source is GridViewColumnHeader gridViewColumnHeader && AllowsColumnReorder)
		{
			PrepareHeaderDrag(gridViewColumnHeader, e.GetPosition(this), e.GetPosition(gridViewColumnHeader), cancelInvoke: false);
			MakeParentItemsControlGotFocus();
		}
		e.Handled = true;
		base.OnMouseLeftButtonDown(e);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseLeftButtonUp" /> event that occurs when the user releases the left mouse button. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		_prepareDragging = false;
		if (_isHeaderDragging)
		{
			FinishHeaderDrag(isCancel: false);
		}
		e.Handled = true;
		base.OnMouseLeftButtonUp(e);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseMove" /> event that occurs when the user moves the mouse.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (e.LeftButton == MouseButtonState.Pressed && _prepareDragging)
		{
			_currentPos = e.GetPosition(this);
			_desColumnIndex = FindIndexByPosition(_currentPos, findNearestColumn: true);
			if (!_isHeaderDragging)
			{
				if (CheckStartHeaderDrag(_currentPos, _startPos))
				{
					StartHeaderDrag();
					InvalidateMeasure();
				}
			}
			else
			{
				bool flag = IsMousePositionValid(_floatingHeader, _currentPos, 2.0);
				Separator indicator = _indicator;
				Visibility visibility2 = (_floatingHeader.Visibility = ((!flag) ? Visibility.Hidden : Visibility.Visible));
				indicator.Visibility = visibility2;
				InvalidateArrange();
			}
		}
		e.Handled = true;
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.LostMouseCapture" /> event for the <see cref="T:System.Windows.Controls.GridViewColumnHeader" />.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnLostMouseCapture(MouseEventArgs e)
	{
		base.OnLostMouseCapture(e);
		if (e.LeftButton == MouseButtonState.Pressed && _isHeaderDragging)
		{
			FinishHeaderDrag(isCancel: true);
		}
		_prepareDragging = false;
	}

	internal override void OnPreApplyTemplate()
	{
		base.OnPreApplyTemplate();
		if (!base.NeedUpdateVisualTree)
		{
			return;
		}
		UIElementCollection internalChildren = base.InternalChildren;
		GridViewColumnCollection columns = base.Columns;
		RenewEvents();
		if (internalChildren.Count == 0)
		{
			AddPaddingColumnHeader();
			AddIndicator();
			AddFloatingHeader(null);
		}
		else if (internalChildren.Count > 3)
		{
			int num = internalChildren.Count - 3;
			for (int i = 0; i < num; i++)
			{
				RemoveHeader(null, 1);
			}
		}
		UpdatePaddingHeader(_paddingHeader);
		if (columns != null)
		{
			int num2 = 1;
			for (int num3 = columns.Count - 1; num3 >= 0; num3--)
			{
				GridViewColumn column = columns[num3];
				CreateAndInsertHeader(column, num2++);
			}
		}
		BuildHeaderLinks();
		base.NeedUpdateVisualTree = false;
		_isColumnChangedOrCreated = true;
	}

	internal override void OnColumnPropertyChanged(GridViewColumn column, string propertyName)
	{
		if (column.ActualIndex < 0)
		{
			return;
		}
		GridViewColumnHeader gridViewColumnHeader = FindHeaderByColumn(column);
		if (gridViewColumnHeader == null)
		{
			return;
		}
		if (GridViewColumn.WidthProperty.Name.Equals(propertyName) || "ActualWidth".Equals(propertyName))
		{
			InvalidateMeasure();
		}
		else if (GridViewColumn.HeaderProperty.Name.Equals(propertyName))
		{
			if (!gridViewColumnHeader.IsInternalGenerated || column.Header is GridViewColumnHeader)
			{
				int index = base.InternalChildren.IndexOf(gridViewColumnHeader);
				RemoveHeader(gridViewColumnHeader, -1);
				CreateAndInsertHeader(column, index);
				BuildHeaderLinks();
			}
			else
			{
				UpdateHeaderContent(gridViewColumnHeader);
			}
		}
		else
		{
			DependencyProperty columnDPFromName = GetColumnDPFromName(propertyName);
			if (columnDPFromName != null)
			{
				UpdateHeaderProperty(gridViewColumnHeader, columnDPFromName);
			}
		}
	}

	internal override void OnColumnCollectionChanged(GridViewColumnCollectionChangedEventArgs e)
	{
		base.OnColumnCollectionChanged(e);
		UIElementCollection internalChildren = base.InternalChildren;
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Move:
		{
			int visualIndex = GetVisualIndex(e.OldStartingIndex);
			int visualIndex2 = GetVisualIndex(e.NewStartingIndex);
			GridViewColumnHeader element = (GridViewColumnHeader)internalChildren[visualIndex];
			internalChildren.RemoveAt(visualIndex);
			internalChildren.InsertInternal(visualIndex2, element);
			break;
		}
		case NotifyCollectionChangedAction.Add:
		{
			int visualIndex3 = GetVisualIndex(e.NewStartingIndex);
			GridViewColumn column = (GridViewColumn)e.NewItems[0];
			CreateAndInsertHeader(column, visualIndex3 + 1);
			break;
		}
		case NotifyCollectionChangedAction.Remove:
			RemoveHeader(null, GetVisualIndex(e.OldStartingIndex));
			break;
		case NotifyCollectionChangedAction.Replace:
		{
			int visualIndex3 = GetVisualIndex(e.OldStartingIndex);
			RemoveHeader(null, visualIndex3);
			GridViewColumn column = (GridViewColumn)e.NewItems[0];
			CreateAndInsertHeader(column, visualIndex3);
			break;
		}
		case NotifyCollectionChangedAction.Reset:
		{
			int count = e.ClearedColumns.Count;
			for (int i = 0; i < count; i++)
			{
				RemoveHeader(null, 1);
			}
			break;
		}
		}
		BuildHeaderLinks();
		_isColumnChangedOrCreated = true;
	}

	internal void MakeParentItemsControlGotFocus()
	{
		if (_itemsControl != null && !_itemsControl.IsKeyboardFocusWithin)
		{
			if (_itemsControl is ListBox { LastActionItem: not null } listBox)
			{
				listBox.LastActionItem.Focus();
			}
			else
			{
				_itemsControl.Focus();
			}
		}
	}

	internal void UpdateHeaderProperty(GridViewColumnHeader header, DependencyProperty property)
	{
		GetMatchingDPs(property, out var gvDP, out var columnDP, out var headerDP);
		UpdateHeaderProperty(header, headerDP, columnDP, gvDP);
	}

	/// <summary>Creates an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> object for the column header row.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.GridViewHeaderRowPresenterAutomationPeer" /> object for this column header row. </returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new GridViewHeaderRowPresenterAutomationPeer(this);
	}

	private void OnLayoutUpdated(object sender, EventArgs e)
	{
		bool flag = false;
		GridViewColumnCollection columns = base.Columns;
		if (columns != null)
		{
			foreach (GridViewColumn item in columns)
			{
				if (item.State != ColumnMeasureState.SpecificWidth)
				{
					if (item.State == ColumnMeasureState.Init)
					{
						item.State = ColumnMeasureState.Headered;
					}
					if (base.DesiredWidthList == null || item.ActualIndex >= base.DesiredWidthList.Count)
					{
						flag = true;
						break;
					}
					if (!DoubleUtil.AreClose(item.DesiredWidth, base.DesiredWidthList[item.ActualIndex]))
					{
						base.DesiredWidthList[item.ActualIndex] = item.DesiredWidth;
						flag = true;
					}
				}
			}
		}
		if (flag)
		{
			InvalidateMeasure();
		}
		base.LayoutUpdated -= OnLayoutUpdated;
	}

	private int GetVisualIndex(int columnIndex)
	{
		return base.InternalChildren.Count - 3 - columnIndex;
	}

	private void BuildHeaderLinks()
	{
		GridViewColumnHeader previousVisualHeader = null;
		if (base.Columns != null)
		{
			for (int i = 0; i < base.Columns.Count; i++)
			{
				GridViewColumnHeader obj = (GridViewColumnHeader)base.InternalChildren[GetVisualIndex(i)];
				obj.PreviousVisualHeader = previousVisualHeader;
				previousVisualHeader = obj;
			}
		}
		if (_paddingHeader != null)
		{
			_paddingHeader.PreviousVisualHeader = previousVisualHeader;
		}
	}

	private GridViewColumnHeader CreateAndInsertHeader(GridViewColumn column, int index)
	{
		object header = column.Header;
		GridViewColumnHeader gridViewColumnHeader = header as GridViewColumnHeader;
		if (header != null && header is DependencyObject dependencyObject)
		{
			if (dependencyObject is Visual reference && VisualTreeHelper.GetParent(reference) is Visual visual)
			{
				if (gridViewColumnHeader != null)
				{
					if (visual is GridViewHeaderRowPresenter gridViewHeaderRowPresenter)
					{
						gridViewHeaderRowPresenter.InternalChildren.RemoveNoVerify(gridViewColumnHeader);
					}
				}
				else if (visual is GridViewColumnHeader gridViewColumnHeader2)
				{
					gridViewColumnHeader2.ClearValue(ContentControl.ContentProperty);
				}
			}
			DependencyObject parent = LogicalTreeHelper.GetParent(dependencyObject);
			if (parent != null)
			{
				LogicalTreeHelper.RemoveLogicalChild(parent, header);
			}
		}
		if (gridViewColumnHeader == null)
		{
			gridViewColumnHeader = new GridViewColumnHeader();
			gridViewColumnHeader.IsInternalGenerated = true;
		}
		gridViewColumnHeader.SetValue(GridViewColumnHeader.ColumnPropertyKey, column);
		HookupItemsControlKeyboardEvent(gridViewColumnHeader);
		base.InternalChildren.InsertInternal(index, gridViewColumnHeader);
		UpdateHeader(gridViewColumnHeader);
		_gvHeadersValid = false;
		return gridViewColumnHeader;
	}

	private void RemoveHeader(GridViewColumnHeader header, int index)
	{
		_gvHeadersValid = false;
		if (header != null)
		{
			base.InternalChildren.Remove(header);
		}
		else
		{
			header = (GridViewColumnHeader)base.InternalChildren[index];
			base.InternalChildren.RemoveAt(index);
		}
		UnhookItemsControlKeyboardEvent(header);
	}

	private void RenewEvents()
	{
		ScrollViewer headerSV = _headerSV;
		_headerSV = base.Parent as ScrollViewer;
		if (headerSV != _headerSV)
		{
			if (headerSV != null)
			{
				headerSV.ScrollChanged -= OnHeaderScrollChanged;
			}
			if (_headerSV != null)
			{
				_headerSV.ScrollChanged += OnHeaderScrollChanged;
			}
		}
		ScrollViewer mainSV = _mainSV;
		_mainSV = base.TemplatedParent as ScrollViewer;
		if (mainSV != _mainSV)
		{
			if (mainSV != null)
			{
				mainSV.ScrollChanged -= OnMasterScrollChanged;
			}
			if (_mainSV != null)
			{
				_mainSV.ScrollChanged += OnMasterScrollChanged;
			}
		}
		ItemsControl itemsControl = _itemsControl;
		_itemsControl = FindItemsControlThroughTemplatedParent(this);
		if (itemsControl != _itemsControl)
		{
			if (itemsControl != null)
			{
				itemsControl.KeyDown -= OnColumnHeadersPresenterKeyDown;
			}
			if (_itemsControl != null)
			{
				_itemsControl.KeyDown += OnColumnHeadersPresenterKeyDown;
			}
		}
		if (_itemsControl is ListView { View: not null } listView && listView.View is GridView)
		{
			((GridView)listView.View).HeaderRowPresenter = this;
		}
	}

	private void UnhookItemsControlKeyboardEvent(GridViewColumnHeader header)
	{
		if (_itemsControl != null)
		{
			_itemsControl.KeyDown -= header.OnColumnHeaderKeyDown;
		}
	}

	private void HookupItemsControlKeyboardEvent(GridViewColumnHeader header)
	{
		if (_itemsControl != null)
		{
			_itemsControl.KeyDown += header.OnColumnHeaderKeyDown;
		}
	}

	private void OnMasterScrollChanged(object sender, ScrollChangedEventArgs e)
	{
		if (_headerSV != null && _mainSV == e.OriginalSource)
		{
			_headerSV.ScrollToHorizontalOffset(e.HorizontalOffset);
		}
	}

	private void OnHeaderScrollChanged(object sender, ScrollChangedEventArgs e)
	{
		if (_mainSV != null && _headerSV == e.OriginalSource)
		{
			_mainSV.ScrollToHorizontalOffset(e.HorizontalOffset);
		}
	}

	private void AddPaddingColumnHeader()
	{
		GridViewColumnHeader gridViewColumnHeader = new GridViewColumnHeader();
		gridViewColumnHeader.IsInternalGenerated = true;
		gridViewColumnHeader.SetValue(GridViewColumnHeader.RolePropertyKey, GridViewColumnHeaderRole.Padding);
		gridViewColumnHeader.Content = null;
		gridViewColumnHeader.ContentTemplate = null;
		gridViewColumnHeader.ContentTemplateSelector = null;
		gridViewColumnHeader.MinWidth = 0.0;
		gridViewColumnHeader.Padding = new Thickness(0.0);
		gridViewColumnHeader.Width = double.NaN;
		gridViewColumnHeader.HorizontalAlignment = HorizontalAlignment.Stretch;
		if (!AccessibilitySwitches.UseNetFx472CompatibleAccessibilityFeatures)
		{
			gridViewColumnHeader.Focusable = false;
		}
		base.InternalChildren.AddInternal(gridViewColumnHeader);
		_paddingHeader = gridViewColumnHeader;
	}

	private void AddIndicator()
	{
		Separator separator = new Separator();
		separator.Visibility = Visibility.Hidden;
		separator.Margin = new Thickness(0.0);
		separator.Width = 2.0;
		FrameworkElementFactory frameworkElementFactory = new FrameworkElementFactory(typeof(Border));
		frameworkElementFactory.SetValue(Border.BackgroundProperty, new SolidColorBrush(Color.FromUInt32(4278190208u)));
		ControlTemplate controlTemplate = new ControlTemplate(typeof(Separator));
		controlTemplate.VisualTree = frameworkElementFactory;
		controlTemplate.Seal();
		separator.Template = controlTemplate;
		base.InternalChildren.AddInternal(separator);
		_indicator = separator;
	}

	private void AddFloatingHeader(GridViewColumnHeader srcHeader)
	{
		Type type = ((srcHeader != null) ? srcHeader.GetType() : typeof(GridViewColumnHeader));
		GridViewColumnHeader gridViewColumnHeader;
		try
		{
			gridViewColumnHeader = Activator.CreateInstance(type) as GridViewColumnHeader;
		}
		catch (MissingMethodException innerException)
		{
			throw new ArgumentException(SR.Format(SR.ListView_MissingParameterlessConstructor, type), innerException);
		}
		gridViewColumnHeader.IsInternalGenerated = true;
		gridViewColumnHeader.SetValue(GridViewColumnHeader.RolePropertyKey, GridViewColumnHeaderRole.Floating);
		gridViewColumnHeader.Visibility = Visibility.Hidden;
		base.InternalChildren.AddInternal(gridViewColumnHeader);
		_floatingHeader = gridViewColumnHeader;
	}

	private void UpdateFloatingHeader(GridViewColumnHeader srcHeader)
	{
		_floatingHeader.Style = srcHeader.Style;
		_floatingHeader.FloatSourceHeader = srcHeader;
		_floatingHeader.Width = srcHeader.ActualWidth;
		_floatingHeader.Height = srcHeader.ActualHeight;
		_floatingHeader.SetValue(GridViewColumnHeader.ColumnPropertyKey, srcHeader.Column);
		_floatingHeader.Visibility = Visibility.Hidden;
		_floatingHeader.MinWidth = srcHeader.MinWidth;
		_floatingHeader.MinHeight = srcHeader.MinHeight;
		object obj = srcHeader.ReadLocalValue(ContentControl.ContentTemplateProperty);
		if (obj != DependencyProperty.UnsetValue && obj != null)
		{
			_floatingHeader.ContentTemplate = srcHeader.ContentTemplate;
		}
		object obj2 = srcHeader.ReadLocalValue(ContentControl.ContentTemplateSelectorProperty);
		if (obj2 != DependencyProperty.UnsetValue && obj2 != null)
		{
			_floatingHeader.ContentTemplateSelector = srcHeader.ContentTemplateSelector;
		}
		if (!(srcHeader.Content is Visual))
		{
			_floatingHeader.Content = srcHeader.Content;
		}
	}

	private bool CheckStartHeaderDrag(Point currentPos, Point originalPos)
	{
		return DoubleUtil.GreaterThan(Math.Abs(currentPos.X - originalPos.X), 4.0);
	}

	private static ItemsControl FindItemsControlThroughTemplatedParent(GridViewHeaderRowPresenter presenter)
	{
		FrameworkElement frameworkElement = presenter.TemplatedParent as FrameworkElement;
		ItemsControl itemsControl = null;
		while (frameworkElement != null)
		{
			itemsControl = frameworkElement as ItemsControl;
			if (itemsControl != null)
			{
				break;
			}
			frameworkElement = frameworkElement.TemplatedParent as FrameworkElement;
		}
		return itemsControl;
	}

	private void OnColumnHeadersPresenterKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape && _isHeaderDragging)
		{
			GridViewColumnHeader draggingSrcHeader = _draggingSrcHeader;
			FinishHeaderDrag(isCancel: true);
			PrepareHeaderDrag(draggingSrcHeader, _currentPos, _relativeStartPos, cancelInvoke: true);
			InvalidateArrange();
		}
	}

	private GridViewColumnHeader FindHeaderByColumn(GridViewColumn column)
	{
		GridViewColumnCollection columns = base.Columns;
		UIElementCollection internalChildren = base.InternalChildren;
		if (columns != null && internalChildren.Count > columns.Count)
		{
			int num = columns.IndexOf(column);
			if (num != -1)
			{
				int visualIndex = GetVisualIndex(num);
				GridViewColumnHeader gridViewColumnHeader = internalChildren[visualIndex] as GridViewColumnHeader;
				if (gridViewColumnHeader.Column == column)
				{
					return gridViewColumnHeader;
				}
				for (int i = 1; i < internalChildren.Count; i++)
				{
					if (internalChildren[i] is GridViewColumnHeader gridViewColumnHeader2 && gridViewColumnHeader2.Column == column)
					{
						return gridViewColumnHeader2;
					}
				}
			}
		}
		return null;
	}

	private int FindIndexByPosition(Point startPos, bool findNearestColumn)
	{
		int num = -1;
		if (startPos.X < 0.0)
		{
			return 0;
		}
		for (int i = 0; i < HeadersPositionList.Count; i++)
		{
			num++;
			Rect rect = HeadersPositionList[i];
			double x = rect.X;
			double num2 = x + rect.Width;
			if (!DoubleUtil.GreaterThanOrClose(startPos.X, x) || !DoubleUtil.LessThanOrClose(startPos.X, num2))
			{
				continue;
			}
			if (findNearestColumn)
			{
				double value = (x + num2) * 0.5;
				if (DoubleUtil.GreaterThanOrClose(startPos.X, value) && i != HeadersPositionList.Count - 1)
				{
					num++;
				}
			}
			break;
		}
		return num;
	}

	private Point FindPositionByIndex(int index)
	{
		return new Point(HeadersPositionList[index].X, 0.0);
	}

	private void UpdateHeader(GridViewColumnHeader header)
	{
		UpdateHeaderContent(header);
		int i = 0;
		for (int num = s_DPList[0].Length; i < num; i++)
		{
			UpdateHeaderProperty(header, s_DPList[2][i], s_DPList[1][i], s_DPList[0][i]);
		}
	}

	private void UpdateHeaderContent(GridViewColumnHeader header)
	{
		if (header == null || !header.IsInternalGenerated)
		{
			return;
		}
		GridViewColumn column = header.Column;
		if (column != null)
		{
			if (column.Header == null)
			{
				header.ClearValue(ContentControl.ContentProperty);
			}
			else
			{
				header.Content = column.Header;
			}
		}
	}

	private void UpdatePaddingHeader(GridViewColumnHeader header)
	{
		UpdateHeaderProperty(header, ColumnHeaderContainerStyleProperty);
		UpdateHeaderProperty(header, ColumnHeaderContextMenuProperty);
		UpdateHeaderProperty(header, ColumnHeaderToolTipProperty);
	}

	private void UpdateAllHeaders(DependencyProperty dp)
	{
		GetMatchingDPs(dp, out var gvDP, out var columnDP, out var headerDP);
		GetIndexRange(dp, out var iStart, out var iEnd);
		UIElementCollection internalChildren = base.InternalChildren;
		for (int i = iStart; i <= iEnd; i++)
		{
			if (internalChildren[i] is GridViewColumnHeader header)
			{
				UpdateHeaderProperty(header, headerDP, columnDP, gvDP);
			}
		}
	}

	private void GetIndexRange(DependencyProperty dp, out int iStart, out int iEnd)
	{
		iStart = ((dp == ColumnHeaderTemplateProperty || dp == ColumnHeaderTemplateSelectorProperty || dp == ColumnHeaderStringFormatProperty) ? 1 : 0);
		iEnd = base.InternalChildren.Count - 3;
	}

	private void UpdateHeaderProperty(GridViewColumnHeader header, DependencyProperty targetDP, DependencyProperty columnDP, DependencyProperty gvDP)
	{
		if (gvDP == ColumnHeaderContainerStyleProperty && header.Role == GridViewColumnHeaderRole.Padding)
		{
			Style columnHeaderContainerStyle = ColumnHeaderContainerStyle;
			if (columnHeaderContainerStyle != null && !columnHeaderContainerStyle.TargetType.IsAssignableFrom(typeof(GridViewColumnHeader)))
			{
				header.Style = null;
				return;
			}
		}
		GridViewColumn column = header.Column;
		object obj = null;
		if (column != null && columnDP != null)
		{
			obj = column.GetValue(columnDP);
		}
		if (obj == null)
		{
			obj = GetValue(gvDP);
		}
		header.UpdateProperty(targetDP, obj);
	}

	private void PrepareHeaderDrag(GridViewColumnHeader header, Point pos, Point relativePos, bool cancelInvoke)
	{
		if (header.Role == GridViewColumnHeaderRole.Normal)
		{
			_prepareDragging = true;
			_isHeaderDragging = false;
			_draggingSrcHeader = header;
			_startPos = pos;
			_relativeStartPos = relativePos;
			if (!cancelInvoke)
			{
				_startColumnIndex = FindIndexByPosition(_startPos, findNearestColumn: false);
			}
		}
	}

	private void StartHeaderDrag()
	{
		_startPos = _currentPos;
		_isHeaderDragging = true;
		_draggingSrcHeader.SuppressClickEvent = true;
		if (base.Columns != null)
		{
			base.Columns.BlockWrite();
		}
		base.InternalChildren.Remove(_floatingHeader);
		AddFloatingHeader(_draggingSrcHeader);
		UpdateFloatingHeader(_draggingSrcHeader);
	}

	private void FinishHeaderDrag(bool isCancel)
	{
		_prepareDragging = false;
		_isHeaderDragging = false;
		_draggingSrcHeader.SuppressClickEvent = false;
		_floatingHeader.Visibility = Visibility.Hidden;
		_floatingHeader.ResetFloatingHeaderCanvasBackground();
		_indicator.Visibility = Visibility.Hidden;
		if (base.Columns != null)
		{
			base.Columns.UnblockWrite();
		}
		if (!isCancel)
		{
			bool num = IsMousePositionValid(_floatingHeader, _currentPos, 2.0);
			int newIndex = ((_startColumnIndex >= _desColumnIndex) ? _desColumnIndex : (_desColumnIndex - 1));
			if (num)
			{
				base.Columns.Move(_startColumnIndex, newIndex);
			}
		}
	}

	private static bool IsMousePositionValid(FrameworkElement floatingHeader, Point currentPos, double arrange)
	{
		if (DoubleUtil.LessThanOrClose((0.0 - floatingHeader.Height) * arrange, currentPos.Y))
		{
			return DoubleUtil.LessThanOrClose(currentPos.Y, floatingHeader.Height * (arrange + 1.0));
		}
		return false;
	}

	private static DependencyProperty GetColumnDPFromName(string dpName)
	{
		DependencyProperty[] array = s_DPList[1];
		foreach (DependencyProperty dependencyProperty in array)
		{
			if (dependencyProperty != null && dpName.Equals(dependencyProperty.Name))
			{
				return dependencyProperty;
			}
		}
		return null;
	}

	private static void GetMatchingDPs(DependencyProperty indexDP, out DependencyProperty gvDP, out DependencyProperty columnDP, out DependencyProperty headerDP)
	{
		for (int i = 0; i < s_DPList.Length; i++)
		{
			for (int j = 0; j < s_DPList[i].Length; j++)
			{
				if (indexDP == s_DPList[i][j])
				{
					gvDP = s_DPList[0][j];
					columnDP = s_DPList[1][j];
					headerDP = s_DPList[2][j];
					return;
				}
			}
		}
		gvDP = (columnDP = (headerDP = null));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.GridViewHeaderRowPresenter" /> class.</summary>
	public GridViewHeaderRowPresenter()
	{
	}
}
