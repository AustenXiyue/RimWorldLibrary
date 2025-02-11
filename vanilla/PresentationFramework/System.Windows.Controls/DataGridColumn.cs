using System.ComponentModel;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Input;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Represents a <see cref="T:System.Windows.Controls.DataGrid" /> column.</summary>
public abstract class DataGridColumn : DependencyObject
{
	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.Header" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.Header" /> dependency property.</returns>
	public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(DataGridColumn), new FrameworkPropertyMetadata(null, OnNotifyColumnHeaderPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.HeaderStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.HeaderStyle" /> dependency property.</returns>
	public static readonly DependencyProperty HeaderStyleProperty = DependencyProperty.Register("HeaderStyle", typeof(Style), typeof(DataGridColumn), new FrameworkPropertyMetadata(null, OnNotifyColumnHeaderPropertyChanged, OnCoerceHeaderStyle));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.HeaderStringFormat" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.HeaderStringFormat" /> dependency property.</returns>
	public static readonly DependencyProperty HeaderStringFormatProperty = DependencyProperty.Register("HeaderStringFormat", typeof(string), typeof(DataGridColumn), new FrameworkPropertyMetadata(null, OnNotifyColumnHeaderPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.HeaderTemplate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.HeaderTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(DataGridColumn), new FrameworkPropertyMetadata(null, OnNotifyColumnHeaderPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.HeaderTemplateSelector" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.HeaderTemplateSelector" /> dependency property.</returns>
	public static readonly DependencyProperty HeaderTemplateSelectorProperty = DependencyProperty.Register("HeaderTemplateSelector", typeof(DataTemplateSelector), typeof(DataGridColumn), new FrameworkPropertyMetadata(null, OnNotifyColumnHeaderPropertyChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.CellStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.CellStyle" /> dependency property.</returns>
	public static readonly DependencyProperty CellStyleProperty = DependencyProperty.Register("CellStyle", typeof(Style), typeof(DataGridColumn), new FrameworkPropertyMetadata(null, OnNotifyCellPropertyChanged, OnCoerceCellStyle));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.IsReadOnly" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.IsReadOnly" /> dependency property.</returns>
	public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(DataGridColumn), new FrameworkPropertyMetadata(false, OnNotifyCellPropertyChanged, OnCoerceIsReadOnly));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.Width" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.Width" /> dependency property.</returns>
	public static readonly DependencyProperty WidthProperty = DependencyProperty.Register("Width", typeof(DataGridLength), typeof(DataGridColumn), new FrameworkPropertyMetadata(DataGridLength.Auto, OnWidthPropertyChanged, OnCoerceWidth));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.MinWidth" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.MinWidth" /> dependency property.</returns>
	public static readonly DependencyProperty MinWidthProperty = DependencyProperty.Register("MinWidth", typeof(double), typeof(DataGridColumn), new FrameworkPropertyMetadata(20.0, OnMinWidthPropertyChanged, OnCoerceMinWidth), ValidateMinWidth);

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.MaxWidth" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.MaxWidth" /> dependency property.</returns>
	public static readonly DependencyProperty MaxWidthProperty = DependencyProperty.Register("MaxWidth", typeof(double), typeof(DataGridColumn), new FrameworkPropertyMetadata(double.PositiveInfinity, OnMaxWidthPropertyChanged, OnCoerceMaxWidth), ValidateMaxWidth);

	private static readonly DependencyPropertyKey ActualWidthPropertyKey = DependencyProperty.RegisterReadOnly("ActualWidth", typeof(double), typeof(DataGridColumn), new FrameworkPropertyMetadata(0.0, null, OnCoerceActualWidth));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.ActualWidth" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.ActualWidth" /> dependency property.</returns>
	public static readonly DependencyProperty ActualWidthProperty = ActualWidthPropertyKey.DependencyProperty;

	private static readonly DependencyProperty OriginalValueProperty = DependencyProperty.RegisterAttached("OriginalValue", typeof(object), typeof(DataGridColumn), new FrameworkPropertyMetadata(null));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.DisplayIndex" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.DisplayIndex" /> dependency property.</returns>
	public static readonly DependencyProperty DisplayIndexProperty = DependencyProperty.Register("DisplayIndex", typeof(int), typeof(DataGridColumn), new FrameworkPropertyMetadata(-1, DisplayIndexChanged, OnCoerceDisplayIndex));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.SortMemberPath" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.SortMemberPath" /> dependency property.</returns>
	public static readonly DependencyProperty SortMemberPathProperty = DependencyProperty.Register("SortMemberPath", typeof(string), typeof(DataGridColumn), new FrameworkPropertyMetadata(string.Empty));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.CanUserSort" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.CanUserSort" /> dependency property.</returns>
	public static readonly DependencyProperty CanUserSortProperty = DependencyProperty.Register("CanUserSort", typeof(bool), typeof(DataGridColumn), new FrameworkPropertyMetadata(true, OnCanUserSortPropertyChanged, OnCoerceCanUserSort));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.SortDirection" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.SortDirection" /> dependency property.</returns>
	public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.Register("SortDirection", typeof(ListSortDirection?), typeof(DataGridColumn), new FrameworkPropertyMetadata(null, OnNotifySortPropertyChanged));

	private static readonly DependencyPropertyKey IsAutoGeneratedPropertyKey = DependencyProperty.RegisterReadOnly("IsAutoGenerated", typeof(bool), typeof(DataGridColumn), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.IsAutoGenerated" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.IsAutoGenerated" /> dependency property.</returns>
	public static readonly DependencyProperty IsAutoGeneratedProperty = IsAutoGeneratedPropertyKey.DependencyProperty;

	private static readonly DependencyPropertyKey IsFrozenPropertyKey = DependencyProperty.RegisterReadOnly("IsFrozen", typeof(bool), typeof(DataGridColumn), new FrameworkPropertyMetadata(false, OnNotifyFrozenPropertyChanged, OnCoerceIsFrozen));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.IsFrozen" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.IsFrozen" /> dependency property.</returns>
	public static readonly DependencyProperty IsFrozenProperty = IsFrozenPropertyKey.DependencyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.CanUserReorder" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.CanUserReorder" /> dependency property.</returns>
	public static readonly DependencyProperty CanUserReorderProperty = DependencyProperty.Register("CanUserReorder", typeof(bool), typeof(DataGridColumn), new FrameworkPropertyMetadata(true, OnNotifyColumnPropertyChanged, OnCoerceCanUserReorder));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.DragIndicatorStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.DragIndicatorStyle" /> dependency property.</returns>
	public static readonly DependencyProperty DragIndicatorStyleProperty = DependencyProperty.Register("DragIndicatorStyle", typeof(Style), typeof(DataGridColumn), new FrameworkPropertyMetadata(null, OnNotifyColumnPropertyChanged, OnCoerceDragIndicatorStyle));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.CanUserResize" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.CanUserResize" /> dependency property.</returns>
	public static readonly DependencyProperty CanUserResizeProperty = DependencyProperty.Register("CanUserResize", typeof(bool), typeof(DataGridColumn), new FrameworkPropertyMetadata(true, OnNotifyColumnHeaderPropertyChanged, OnCoerceCanUserResize));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridColumn.Visibility" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridColumn.Visibility" /> dependency property.</returns>
	public static readonly DependencyProperty VisibilityProperty = DependencyProperty.Register("Visibility", typeof(Visibility), typeof(DataGridColumn), new FrameworkPropertyMetadata(Visibility.Visible, OnVisibilityPropertyChanged));

	private DataGrid _dataGridOwner;

	private BindingBase _clipboardContentBinding;

	private bool _ignoreRedistributionOnWidthChange;

	private bool _processingWidthChange;

	private const double _starMaxWidth = 10000.0;

	/// <summary>Gets or sets the content of the column header.</summary>
	/// <returns>The column header content. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public object Header
	{
		get
		{
			return GetValue(HeaderProperty);
		}
		set
		{
			SetValue(HeaderProperty, value);
		}
	}

	/// <summary>Gets or sets the style that is used when rendering the column header.</summary>
	/// <returns>The style that is used to render the column header; or null, to use the <see cref="P:System.Windows.Controls.DataGrid.ColumnHeaderStyle" /> setting. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Style HeaderStyle
	{
		get
		{
			return (Style)GetValue(HeaderStyleProperty);
		}
		set
		{
			SetValue(HeaderStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the format pattern to apply to the content of the column header.</summary>
	/// <returns>A string value that represents the formatting pattern. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public string HeaderStringFormat
	{
		get
		{
			return (string)GetValue(HeaderStringFormatProperty);
		}
		set
		{
			SetValue(HeaderStringFormatProperty, value);
		}
	}

	/// <summary>Gets or sets the template that defines the visual representation of the column header.</summary>
	/// <returns>The object that defines the visual representation of the column header. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataTemplate HeaderTemplate
	{
		get
		{
			return (DataTemplate)GetValue(HeaderTemplateProperty);
		}
		set
		{
			SetValue(HeaderTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets the object that selects which template to use for the column header.</summary>
	/// <returns>The object that selects the template. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataTemplateSelector HeaderTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(HeaderTemplateSelectorProperty);
		}
		set
		{
			SetValue(HeaderTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets the style that is used to render cells in the column.</summary>
	/// <returns>The style that is used to render cells in the column. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Style CellStyle
	{
		get
		{
			return (Style)GetValue(CellStyleProperty);
		}
		set
		{
			SetValue(CellStyleProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether cells in the column can be edited.</summary>
	/// <returns>true if cells in the column cannot be edited; otherwise, false. The registered default is false. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool IsReadOnly
	{
		get
		{
			return (bool)GetValue(IsReadOnlyProperty);
		}
		set
		{
			SetValue(IsReadOnlyProperty, value);
		}
	}

	/// <summary>Gets or sets the column width or automatic sizing mode.</summary>
	/// <returns>A structure that represents the column width or automatic sizing mode. The registered default is <see cref="P:System.Windows.Controls.DataGridLength.Auto" />. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataGridLength Width
	{
		get
		{
			return (DataGridLength)GetValue(WidthProperty);
		}
		set
		{
			SetValue(WidthProperty, value);
		}
	}

	/// <summary>Gets or sets the minimum width constraint of the column.</summary>
	/// <returns>The minimum column width, in device-independent units (1/96th inch per unit). The registered default is 20. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public double MinWidth
	{
		get
		{
			return (double)GetValue(MinWidthProperty);
		}
		set
		{
			SetValue(MinWidthProperty, value);
		}
	}

	/// <summary>Gets or sets the maximum width constraint of the column.</summary>
	/// <returns>The maximum column width, in device-independent units (1/96th inch per unit). The registered default is <see cref="F:System.Double.PositiveInfinity" />. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public double MaxWidth
	{
		get
		{
			return (double)GetValue(MaxWidthProperty);
		}
		set
		{
			SetValue(MaxWidthProperty, value);
		}
	}

	/// <summary>Gets the current width of the column, in device-independent units (1/96th inch per unit).</summary>
	/// <returns>The width of the column in device-independent units (1/96th inch per unit). The registered default is 0. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public double ActualWidth
	{
		get
		{
			return (double)GetValue(ActualWidthProperty);
		}
		private set
		{
			SetValue(ActualWidthPropertyKey, value);
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Controls.DataGrid" /> control that contains this column.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.DataGrid" /> control that contains this column.</returns>
	protected internal DataGrid DataGridOwner
	{
		get
		{
			return _dataGridOwner;
		}
		internal set
		{
			_dataGridOwner = value;
		}
	}

	/// <summary>Gets or sets the display position of the column relative to the other columns in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>The zero-based position of the column, as it is displayed in the associated <see cref="T:System.Windows.Controls.DataGrid" />. The registered default is -1. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public int DisplayIndex
	{
		get
		{
			return (int)GetValue(DisplayIndexProperty);
		}
		set
		{
			SetValue(DisplayIndexProperty, value);
		}
	}

	/// <summary>Gets or sets a property name, or a period-delimited hierarchy of property names, that indicates the member to sort by.</summary>
	/// <returns>The path of the data-item member to sort by. The registered default is an empty string (""). For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public string SortMemberPath
	{
		get
		{
			return (string)GetValue(SortMemberPathProperty);
		}
		set
		{
			SetValue(SortMemberPathProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the user can sort the column by clicking the column header.</summary>
	/// <returns>true if the user can sort the column; otherwise, false. The registered default is true. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool CanUserSort
	{
		get
		{
			return (bool)GetValue(CanUserSortProperty);
		}
		set
		{
			SetValue(CanUserSortProperty, value);
		}
	}

	/// <summary>Gets or sets the sort direction (ascending or descending) of the column.</summary>
	/// <returns>A value that represents the direction for sorting. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public ListSortDirection? SortDirection
	{
		get
		{
			return (ListSortDirection?)GetValue(SortDirectionProperty);
		}
		set
		{
			SetValue(SortDirectionProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the column is auto-generated.</summary>
	/// <returns>true if the column is auto-generated; otherwise, false. The registered default is false. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool IsAutoGenerated
	{
		get
		{
			return (bool)GetValue(IsAutoGeneratedProperty);
		}
		internal set
		{
			SetValue(IsAutoGeneratedPropertyKey, value);
		}
	}

	/// <summary>Gets a value that indicates whether the column is prevented from scrolling horizontally.</summary>
	/// <returns>true if the column cannot be scrolled horizontally; otherwise, false. The registered default is false. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool IsFrozen
	{
		get
		{
			return (bool)GetValue(IsFrozenProperty);
		}
		internal set
		{
			SetValue(IsFrozenPropertyKey, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the user can change the column display position by dragging the column header.</summary>
	/// <returns>true if the user can drag the column header to a new position; otherwise, false. The registered default is true. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool CanUserReorder
	{
		get
		{
			return (bool)GetValue(CanUserReorderProperty);
		}
		set
		{
			SetValue(CanUserReorderProperty, value);
		}
	}

	/// <summary>Gets or sets the style object to apply to the column header during a drag operation.</summary>
	/// <returns>The style object to apply to the column header during a drag operation. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Style DragIndicatorStyle
	{
		get
		{
			return (Style)GetValue(DragIndicatorStyleProperty);
		}
		set
		{
			SetValue(DragIndicatorStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the binding object to use when getting or setting cell content for the clipboard.</summary>
	/// <returns>An object that represents the binding.</returns>
	public virtual BindingBase ClipboardContentBinding
	{
		get
		{
			return _clipboardContentBinding;
		}
		set
		{
			_clipboardContentBinding = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the user can adjust the column width by using the mouse.</summary>
	/// <returns>true if the user can resize the column; otherwise, false. The registered default is true. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool CanUserResize
	{
		get
		{
			return (bool)GetValue(CanUserResizeProperty);
		}
		set
		{
			SetValue(CanUserResizeProperty, value);
		}
	}

	/// <summary>Gets or sets the visibility of the column.</summary>
	/// <returns>An enumeration value that specifies the column visibility. The registered default is <see cref="F:System.Windows.Visibility.Visible" />. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Visibility Visibility
	{
		get
		{
			return (Visibility)GetValue(VisibilityProperty);
		}
		set
		{
			SetValue(VisibilityProperty, value);
		}
	}

	internal bool IsVisible => Visibility == Visibility.Visible;

	/// <summary>Occurs after the cell clipboard content is prepared.</summary>
	public event EventHandler<DataGridCellClipboardEventArgs> CopyingCellClipboardContent;

	/// <summary>Occurs before the clipboard content is moved to the cell.</summary>
	public event EventHandler<DataGridCellClipboardEventArgs> PastingCellClipboardContent;

	private static object OnCoerceHeaderStyle(DependencyObject d, object baseValue)
	{
		DataGridColumn dataGridColumn = d as DataGridColumn;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumn, baseValue, HeaderStyleProperty, dataGridColumn.DataGridOwner, DataGrid.ColumnHeaderStyleProperty);
	}

	private static object OnCoerceCellStyle(DependencyObject d, object baseValue)
	{
		DataGridColumn dataGridColumn = d as DataGridColumn;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumn, baseValue, CellStyleProperty, dataGridColumn.DataGridOwner, DataGrid.CellStyleProperty);
	}

	private static object OnCoerceIsReadOnly(DependencyObject d, object baseValue)
	{
		return (d as DataGridColumn).OnCoerceIsReadOnly((bool)baseValue);
	}

	/// <summary>Determines the value of the <see cref="P:System.Windows.Controls.DataGridColumn.IsReadOnly" /> property based on the property rules of the <see cref="T:System.Windows.Controls.DataGrid" /> that contains this column.</summary>
	/// <returns>true if cells in the column cannot be edited based on rules from the <see cref="T:System.Windows.Controls.DataGrid" />; otherwise, false.</returns>
	/// <param name="baseValue">The value that was passed to the delegate.</param>
	protected virtual bool OnCoerceIsReadOnly(bool baseValue)
	{
		return (bool)DataGridHelper.GetCoercedTransferPropertyValue(this, baseValue, IsReadOnlyProperty, DataGridOwner, DataGrid.IsReadOnlyProperty);
	}

	internal void SetWidthInternal(DataGridLength width)
	{
		bool ignoreRedistributionOnWidthChange = _ignoreRedistributionOnWidthChange;
		_ignoreRedistributionOnWidthChange = true;
		try
		{
			Width = width;
		}
		finally
		{
			_ignoreRedistributionOnWidthChange = ignoreRedistributionOnWidthChange;
		}
	}

	private static void OnWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGridColumn dataGridColumn = (DataGridColumn)d;
		DataGridLength dataGridLength = (DataGridLength)e.OldValue;
		DataGridLength dataGridLength2 = (DataGridLength)e.NewValue;
		DataGrid dataGridOwner = dataGridColumn.DataGridOwner;
		if (dataGridOwner != null && !DoubleUtil.AreClose(dataGridLength.DisplayValue, dataGridLength2.DisplayValue))
		{
			dataGridOwner.InternalColumns.InvalidateAverageColumnWidth();
		}
		if (dataGridColumn._processingWidthChange)
		{
			dataGridColumn.CoerceValue(ActualWidthProperty);
			return;
		}
		dataGridColumn._processingWidthChange = true;
		if (dataGridLength.IsStar != dataGridLength2.IsStar)
		{
			dataGridColumn.CoerceValue(MaxWidthProperty);
		}
		try
		{
			if (dataGridOwner != null && (dataGridLength2.IsStar ^ dataGridLength.IsStar))
			{
				dataGridOwner.InternalColumns.InvalidateHasVisibleStarColumns();
			}
			dataGridColumn.NotifyPropertyChanged(d, e, DataGridNotificationTarget.Cells | DataGridNotificationTarget.CellsPresenter | DataGridNotificationTarget.Columns | DataGridNotificationTarget.ColumnCollection | DataGridNotificationTarget.ColumnHeaders | DataGridNotificationTarget.ColumnHeadersPresenter | DataGridNotificationTarget.DataGrid);
			if (dataGridOwner != null && !dataGridColumn._ignoreRedistributionOnWidthChange && dataGridColumn.IsVisible)
			{
				if (!dataGridLength2.IsStar && !dataGridLength2.IsAbsolute)
				{
					DataGridLength width = dataGridColumn.Width;
					double displayValue = DataGridHelper.CoerceToMinMax(width.DesiredValue, dataGridColumn.MinWidth, dataGridColumn.MaxWidth);
					dataGridColumn.SetWidthInternal(new DataGridLength(width.Value, width.UnitType, width.DesiredValue, displayValue));
				}
				dataGridOwner.InternalColumns.RedistributeColumnWidthsOnWidthChangeOfColumn(dataGridColumn, (DataGridLength)e.OldValue);
			}
		}
		finally
		{
			dataGridColumn._processingWidthChange = false;
		}
	}

	private static void OnMinWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGridColumn dataGridColumn = (DataGridColumn)d;
		DataGrid dataGridOwner = dataGridColumn.DataGridOwner;
		dataGridColumn.NotifyPropertyChanged(d, e, DataGridNotificationTarget.Columns);
		if (dataGridOwner != null && dataGridColumn.IsVisible)
		{
			dataGridOwner.InternalColumns.RedistributeColumnWidthsOnMinWidthChangeOfColumn(dataGridColumn, (double)e.OldValue);
		}
	}

	private static void OnMaxWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGridColumn dataGridColumn = (DataGridColumn)d;
		DataGrid dataGridOwner = dataGridColumn.DataGridOwner;
		dataGridColumn.NotifyPropertyChanged(d, e, DataGridNotificationTarget.Columns);
		if (dataGridOwner != null && dataGridColumn.IsVisible)
		{
			dataGridOwner.InternalColumns.RedistributeColumnWidthsOnMaxWidthChangeOfColumn(dataGridColumn, (double)e.OldValue);
		}
	}

	private static double CoerceDesiredOrDisplayWidthValue(double widthValue, double memberValue, DataGridLengthUnitType type)
	{
		if (double.IsNaN(memberValue))
		{
			switch (type)
			{
			case DataGridLengthUnitType.Pixel:
				memberValue = widthValue;
				break;
			case DataGridLengthUnitType.Auto:
			case DataGridLengthUnitType.SizeToCells:
			case DataGridLengthUnitType.SizeToHeader:
				memberValue = 0.0;
				break;
			}
		}
		return memberValue;
	}

	private static object OnCoerceWidth(DependencyObject d, object baseValue)
	{
		DataGridColumn dataGridColumn = d as DataGridColumn;
		DataGridLength dataGridLength = (DataGridLength)DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumn, baseValue, WidthProperty, dataGridColumn.DataGridOwner, DataGrid.ColumnWidthProperty);
		double desiredValue = CoerceDesiredOrDisplayWidthValue(dataGridLength.Value, dataGridLength.DesiredValue, dataGridLength.UnitType);
		double num = CoerceDesiredOrDisplayWidthValue(dataGridLength.Value, dataGridLength.DisplayValue, dataGridLength.UnitType);
		num = (double.IsNaN(num) ? num : DataGridHelper.CoerceToMinMax(num, dataGridColumn.MinWidth, dataGridColumn.MaxWidth));
		if (double.IsNaN(num) || DoubleUtil.AreClose(num, dataGridLength.DisplayValue))
		{
			return dataGridLength;
		}
		return new DataGridLength(dataGridLength.Value, dataGridLength.UnitType, desiredValue, num);
	}

	private static object OnCoerceMinWidth(DependencyObject d, object baseValue)
	{
		DataGridColumn dataGridColumn = d as DataGridColumn;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumn, baseValue, MinWidthProperty, dataGridColumn.DataGridOwner, DataGrid.MinColumnWidthProperty);
	}

	private static object OnCoerceMaxWidth(DependencyObject d, object baseValue)
	{
		DataGridColumn dataGridColumn = d as DataGridColumn;
		double num = (double)DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumn, baseValue, MaxWidthProperty, dataGridColumn.DataGridOwner, DataGrid.MaxColumnWidthProperty);
		if (double.IsPositiveInfinity(num) && dataGridColumn.Width.IsStar)
		{
			return 10000.0;
		}
		return num;
	}

	private static bool ValidateMinWidth(object v)
	{
		double num = (double)v;
		if (!(num < 0.0) && !double.IsNaN(num))
		{
			return !double.IsPositiveInfinity(num);
		}
		return false;
	}

	private static bool ValidateMaxWidth(object v)
	{
		double num = (double)v;
		if (!(num < 0.0))
		{
			return !double.IsNaN(num);
		}
		return false;
	}

	private static object OnCoerceActualWidth(DependencyObject d, object baseValue)
	{
		DataGridColumn obj = (DataGridColumn)d;
		double num = (double)baseValue;
		double minWidth = obj.MinWidth;
		double maxWidth = obj.MaxWidth;
		DataGridLength width = obj.Width;
		if (width.IsAbsolute)
		{
			num = width.DisplayValue;
		}
		if (num < minWidth)
		{
			num = minWidth;
		}
		else if (num > maxWidth)
		{
			num = maxWidth;
		}
		return num;
	}

	internal double GetConstraintWidth(bool isHeader)
	{
		DataGridLength width = Width;
		if (!double.IsNaN(width.DisplayValue))
		{
			return width.DisplayValue;
		}
		if (width.IsAbsolute || width.IsStar || (width.IsSizeToCells && isHeader) || (width.IsSizeToHeader && !isHeader))
		{
			return ActualWidth;
		}
		return double.PositiveInfinity;
	}

	internal void UpdateDesiredWidthForAutoColumn(bool isHeader, double pixelWidth)
	{
		DataGridLength width = Width;
		double minWidth = MinWidth;
		double maxWidth = MaxWidth;
		double num = DataGridHelper.CoerceToMinMax(pixelWidth, minWidth, maxWidth);
		if (!width.IsAuto && (!width.IsSizeToCells || isHeader) && !(width.IsSizeToHeader && isHeader))
		{
			return;
		}
		if (double.IsNaN(width.DesiredValue) || DoubleUtil.LessThan(width.DesiredValue, pixelWidth))
		{
			if (double.IsNaN(width.DisplayValue))
			{
				SetWidthInternal(new DataGridLength(width.Value, width.UnitType, pixelWidth, num));
			}
			else
			{
				double value = DataGridHelper.CoerceToMinMax(width.DesiredValue, minWidth, maxWidth);
				SetWidthInternal(new DataGridLength(width.Value, width.UnitType, pixelWidth, width.DisplayValue));
				if (DoubleUtil.AreClose(value, width.DisplayValue))
				{
					DataGridOwner.InternalColumns.RecomputeColumnWidthsOnColumnResize(this, pixelWidth - width.DisplayValue, retainAuto: true);
				}
			}
			width = Width;
		}
		if (double.IsNaN(width.DisplayValue))
		{
			if (ActualWidth < num)
			{
				ActualWidth = num;
			}
		}
		else if (!DoubleUtil.AreClose(ActualWidth, width.DisplayValue))
		{
			ActualWidth = width.DisplayValue;
		}
	}

	internal void UpdateWidthForStarColumn(double displayWidth, double desiredWidth, double starValue)
	{
		DataGridLength width = Width;
		if (!DoubleUtil.AreClose(displayWidth, width.DisplayValue) || !DoubleUtil.AreClose(desiredWidth, width.DesiredValue) || !DoubleUtil.AreClose(width.Value, starValue))
		{
			SetWidthInternal(new DataGridLength(starValue, width.UnitType, desiredWidth, displayWidth));
			ActualWidth = displayWidth;
		}
	}

	/// <summary>Gets the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property value for the cell at the intersection of this column and the row that represents the specified data item.</summary>
	/// <returns>The cell content; or null, if the cell is not found.</returns>
	/// <param name="dataItem">The data item that is represented by the row that contains the intended cell.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dataItem" /> is null.</exception>
	public FrameworkElement GetCellContent(object dataItem)
	{
		if (dataItem == null)
		{
			throw new ArgumentNullException("dataItem");
		}
		if (_dataGridOwner != null && _dataGridOwner.ItemContainerGenerator.ContainerFromItem(dataItem) is DataGridRow dataGridRow)
		{
			return GetCellContent(dataGridRow);
		}
		return null;
	}

	/// <summary>Retrieves the <see cref="P:System.Windows.Controls.ContentControl.Content" /> property value for the cell at the intersection of this column and the specified row.</summary>
	/// <returns>The cell content; or null, if the cell is not found.</returns>
	/// <param name="dataGridRow">The row that contains the intended cell.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dataGridRow" /> is null.</exception>
	public FrameworkElement GetCellContent(DataGridRow dataGridRow)
	{
		if (dataGridRow == null)
		{
			throw new ArgumentNullException("dataGridRow");
		}
		if (_dataGridOwner != null)
		{
			int num = _dataGridOwner.Columns.IndexOf(this);
			if (num >= 0)
			{
				DataGridCell dataGridCell = dataGridRow.TryGetCell(num);
				if (dataGridCell != null)
				{
					return dataGridCell.Content as FrameworkElement;
				}
			}
		}
		return null;
	}

	internal FrameworkElement BuildVisualTree(bool isEditing, object dataItem, DataGridCell cell)
	{
		if (isEditing)
		{
			return GenerateEditingElement(cell, dataItem);
		}
		return GenerateElement(cell, dataItem);
	}

	/// <summary>When overridden in a derived class, gets a read-only element that is bound to the <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value of the column.</summary>
	/// <returns>A new read-only element that is bound to the <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value of the column.</returns>
	/// <param name="cell">The cell that will contain the generated element.</param>
	/// <param name="dataItem">The data item that is represented by the row that contains the intended cell.</param>
	protected abstract FrameworkElement GenerateElement(DataGridCell cell, object dataItem);

	/// <summary>When overridden in a derived class, gets an editing element that is bound to the <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value of the column.</summary>
	/// <returns>A new editing element that is bound to the <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value of the column.</returns>
	/// <param name="cell">The cell that will contain the generated element.</param>
	/// <param name="dataItem">The data item that is represented by the row that contains the intended cell.</param>
	protected abstract FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem);

	/// <summary>When overridden in a derived class, sets cell content as needed for editing.</summary>
	/// <returns>When returned by a derived class, the unedited cell value. This implementation returns null in all cases.</returns>
	/// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
	/// <param name="editingEventArgs">Information about the user gesture that is causing a cell to enter editing mode.</param>
	protected virtual object PrepareCellForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
	{
		return null;
	}

	/// <summary>Causes the cell being edited to revert to the original, unedited value.</summary>
	/// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
	/// <param name="uneditedValue">The original, unedited value in the cell being edited.</param>
	protected virtual void CancelCellEdit(FrameworkElement editingElement, object uneditedValue)
	{
		DataGridHelper.UpdateTarget(editingElement);
	}

	/// <summary>Performs any required validation before exiting cell editing mode.</summary>
	/// <returns>true if no validation errors are found; otherwise, false.</returns>
	/// <param name="editingElement">The element that the column displays for a cell in editing mode.</param>
	protected virtual bool CommitCellEdit(FrameworkElement editingElement)
	{
		return DataGridHelper.ValidateWithoutUpdate(editingElement);
	}

	internal void BeginEdit(FrameworkElement editingElement, RoutedEventArgs e)
	{
		if (editingElement != null)
		{
			editingElement.UpdateLayout();
			object value = PrepareCellForEdit(editingElement, e);
			SetOriginalValue(editingElement, value);
		}
	}

	internal void CancelEdit(FrameworkElement editingElement)
	{
		if (editingElement != null)
		{
			CancelCellEdit(editingElement, GetOriginalValue(editingElement));
			ClearOriginalValue(editingElement);
		}
	}

	internal bool CommitEdit(FrameworkElement editingElement)
	{
		if (editingElement != null)
		{
			if (CommitCellEdit(editingElement))
			{
				ClearOriginalValue(editingElement);
				return true;
			}
			return false;
		}
		return true;
	}

	private static object GetOriginalValue(DependencyObject obj)
	{
		return obj.GetValue(OriginalValueProperty);
	}

	private static void SetOriginalValue(DependencyObject obj, object value)
	{
		obj.SetValue(OriginalValueProperty, value);
	}

	private static void ClearOriginalValue(DependencyObject obj)
	{
		obj.ClearValue(OriginalValueProperty);
	}

	internal static void OnNotifyCellPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGridColumn)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.Cells | DataGridNotificationTarget.Columns);
	}

	private static void OnNotifyColumnHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGridColumn)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.Columns | DataGridNotificationTarget.ColumnHeaders);
	}

	private static void OnNotifyColumnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGridColumn)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.Columns);
	}

	internal void NotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e, DataGridNotificationTarget target)
	{
		if (DataGridHelper.ShouldNotifyColumns(target))
		{
			target &= ~DataGridNotificationTarget.Columns;
			if (e.Property == DataGrid.MaxColumnWidthProperty || e.Property == MaxWidthProperty)
			{
				DataGridHelper.TransferProperty(this, MaxWidthProperty);
			}
			else if (e.Property == DataGrid.MinColumnWidthProperty || e.Property == MinWidthProperty)
			{
				DataGridHelper.TransferProperty(this, MinWidthProperty);
			}
			else if (e.Property == DataGrid.ColumnWidthProperty || e.Property == WidthProperty)
			{
				DataGridHelper.TransferProperty(this, WidthProperty);
			}
			else if (e.Property == DataGrid.ColumnHeaderStyleProperty || e.Property == HeaderStyleProperty)
			{
				DataGridHelper.TransferProperty(this, HeaderStyleProperty);
			}
			else if (e.Property == DataGrid.CellStyleProperty || e.Property == CellStyleProperty)
			{
				DataGridHelper.TransferProperty(this, CellStyleProperty);
			}
			else if (e.Property == DataGrid.IsReadOnlyProperty || e.Property == IsReadOnlyProperty)
			{
				DataGridHelper.TransferProperty(this, IsReadOnlyProperty);
			}
			else if (e.Property == DataGrid.DragIndicatorStyleProperty || e.Property == DragIndicatorStyleProperty)
			{
				DataGridHelper.TransferProperty(this, DragIndicatorStyleProperty);
			}
			else if (e.Property == DisplayIndexProperty)
			{
				CoerceValue(IsFrozenProperty);
			}
			else if (e.Property == DataGrid.CanUserSortColumnsProperty)
			{
				DataGridHelper.TransferProperty(this, CanUserSortProperty);
			}
			else if (e.Property == DataGrid.CanUserResizeColumnsProperty || e.Property == CanUserResizeProperty)
			{
				DataGridHelper.TransferProperty(this, CanUserResizeProperty);
			}
			else if (e.Property == DataGrid.CanUserReorderColumnsProperty || e.Property == CanUserReorderProperty)
			{
				DataGridHelper.TransferProperty(this, CanUserReorderProperty);
			}
			if (e.Property == WidthProperty || e.Property == MinWidthProperty || e.Property == MaxWidthProperty)
			{
				CoerceValue(ActualWidthProperty);
			}
		}
		if (target != 0)
		{
			((DataGridColumn)d).DataGridOwner?.NotifyPropertyChanged(d, e, target);
		}
	}

	/// <summary>Notifies the <see cref="T:System.Windows.Controls.DataGrid" /> that contains this column that a column property has changed.</summary>
	/// <param name="propertyName">The name of the column property that changed.</param>
	protected void NotifyPropertyChanged(string propertyName)
	{
		if (DataGridOwner != null)
		{
			DataGridOwner.NotifyPropertyChanged(this, propertyName, default(DependencyPropertyChangedEventArgs), DataGridNotificationTarget.RefreshCellContent);
		}
	}

	internal static void NotifyPropertyChangeForRefreshContent(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGridColumn)d).NotifyPropertyChanged(e.Property.Name);
	}

	/// <summary>When overridden in a derived class, updates the contents of a cell in the column in response to a column property value that changed.</summary>
	/// <param name="element">The cell to update.</param>
	/// <param name="propertyName">The name of the column property that changed.</param>
	protected internal virtual void RefreshCellContent(FrameworkElement element, string propertyName)
	{
	}

	internal void SyncProperties()
	{
		DataGridHelper.TransferProperty(this, MinWidthProperty);
		DataGridHelper.TransferProperty(this, MaxWidthProperty);
		DataGridHelper.TransferProperty(this, WidthProperty);
		DataGridHelper.TransferProperty(this, HeaderStyleProperty);
		DataGridHelper.TransferProperty(this, CellStyleProperty);
		DataGridHelper.TransferProperty(this, IsReadOnlyProperty);
		DataGridHelper.TransferProperty(this, DragIndicatorStyleProperty);
		DataGridHelper.TransferProperty(this, CanUserSortProperty);
		DataGridHelper.TransferProperty(this, CanUserReorderProperty);
		DataGridHelper.TransferProperty(this, CanUserResizeProperty);
	}

	private static object OnCoerceDisplayIndex(DependencyObject d, object baseValue)
	{
		DataGridColumn dataGridColumn = (DataGridColumn)d;
		if (dataGridColumn.DataGridOwner != null)
		{
			dataGridColumn.DataGridOwner.ValidateDisplayIndex(dataGridColumn, (int)baseValue);
		}
		return baseValue;
	}

	private static void DisplayIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGridColumn)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.Cells | DataGridNotificationTarget.CellsPresenter | DataGridNotificationTarget.Columns | DataGridNotificationTarget.ColumnCollection | DataGridNotificationTarget.ColumnHeaders | DataGridNotificationTarget.ColumnHeadersPresenter | DataGridNotificationTarget.DataGrid);
	}

	internal static object OnCoerceCanUserSort(DependencyObject d, object baseValue)
	{
		DataGridColumn dataGridColumn = d as DataGridColumn;
		bool hasModifiers;
		BaseValueSourceInternal valueSource = dataGridColumn.GetValueSource(CanUserSortProperty, null, out hasModifiers);
		if (dataGridColumn.DataGridOwner != null && dataGridColumn.DataGridOwner.GetValueSource(DataGrid.CanUserSortColumnsProperty, null, out var hasModifiers2) == valueSource && !hasModifiers && hasModifiers2)
		{
			return dataGridColumn.DataGridOwner.GetValue(DataGrid.CanUserSortColumnsProperty);
		}
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumn, baseValue, CanUserSortProperty, dataGridColumn.DataGridOwner, DataGrid.CanUserSortColumnsProperty);
	}

	private static void OnCanUserSortPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!DataGridHelper.IsPropertyTransferEnabled(d, CanUserSortProperty))
		{
			DataGridHelper.TransferProperty(d, CanUserSortProperty);
		}
		((DataGridColumn)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.ColumnHeaders);
	}

	private static void OnNotifySortPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGridColumn)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.ColumnHeaders);
	}

	internal static DataGridColumn CreateDefaultColumn(ItemPropertyInfo itemProperty)
	{
		DataGridColumn dataGridColumn = null;
		DataGridComboBoxColumn dataGridComboBoxColumn = null;
		Type propertyType = itemProperty.PropertyType;
		if (!propertyType.IsEnum)
		{
			dataGridColumn = (typeof(string).IsAssignableFrom(propertyType) ? new DataGridTextColumn() : (typeof(bool).IsAssignableFrom(propertyType) ? new DataGridCheckBoxColumn() : ((!typeof(Uri).IsAssignableFrom(propertyType)) ? ((DataGridBoundColumn)new DataGridTextColumn()) : ((DataGridBoundColumn)new DataGridHyperlinkColumn()))));
		}
		else
		{
			dataGridComboBoxColumn = new DataGridComboBoxColumn();
			dataGridComboBoxColumn.ItemsSource = Enum.GetValues(propertyType);
			dataGridColumn = dataGridComboBoxColumn;
		}
		if (!typeof(IComparable).IsAssignableFrom(propertyType))
		{
			dataGridColumn.CanUserSort = false;
		}
		dataGridColumn.Header = itemProperty.Name;
		DataGridBoundColumn dataGridBoundColumn = dataGridColumn as DataGridBoundColumn;
		if (dataGridBoundColumn != null || dataGridComboBoxColumn != null)
		{
			Binding binding = new Binding(itemProperty.Name);
			if (dataGridComboBoxColumn != null)
			{
				dataGridComboBoxColumn.SelectedItemBinding = binding;
			}
			else
			{
				dataGridBoundColumn.Binding = binding;
			}
			if (itemProperty.Descriptor is PropertyDescriptor propertyDescriptor)
			{
				if (propertyDescriptor.IsReadOnly)
				{
					binding.Mode = BindingMode.OneWay;
					dataGridColumn.IsReadOnly = true;
				}
			}
			else
			{
				PropertyInfo propertyInfo = itemProperty.Descriptor as PropertyInfo;
				if (propertyInfo != null && !propertyInfo.CanWrite)
				{
					binding.Mode = BindingMode.OneWay;
					dataGridColumn.IsReadOnly = true;
				}
			}
		}
		return dataGridColumn;
	}

	private static void OnNotifyFrozenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGridColumn)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.ColumnHeaders);
	}

	private static object OnCoerceIsFrozen(DependencyObject d, object baseValue)
	{
		DataGridColumn dataGridColumn = (DataGridColumn)d;
		DataGrid dataGridOwner = dataGridColumn.DataGridOwner;
		if (dataGridOwner != null)
		{
			if (dataGridColumn.DisplayIndex < dataGridOwner.FrozenColumnCount)
			{
				return true;
			}
			return false;
		}
		return baseValue;
	}

	private static object OnCoerceCanUserReorder(DependencyObject d, object baseValue)
	{
		DataGridColumn dataGridColumn = d as DataGridColumn;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumn, baseValue, CanUserReorderProperty, dataGridColumn.DataGridOwner, DataGrid.CanUserReorderColumnsProperty);
	}

	private static object OnCoerceDragIndicatorStyle(DependencyObject d, object baseValue)
	{
		DataGridColumn dataGridColumn = d as DataGridColumn;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumn, baseValue, DragIndicatorStyleProperty, dataGridColumn.DataGridOwner, DataGrid.DragIndicatorStyleProperty);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGridColumn.CopyingCellClipboardContent" /> event.</summary>
	/// <returns>An object that represents the content of the cell.</returns>
	/// <param name="item">The data context for the selected element.</param>
	public virtual object OnCopyingCellClipboardContent(object item)
	{
		object obj = DataGridOwner.GetCellClipboardValue(item, this);
		if (this.CopyingCellClipboardContent != null)
		{
			DataGridCellClipboardEventArgs dataGridCellClipboardEventArgs = new DataGridCellClipboardEventArgs(item, this, obj);
			this.CopyingCellClipboardContent(this, dataGridCellClipboardEventArgs);
			obj = dataGridCellClipboardEventArgs.Content;
		}
		return obj;
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGridColumn.PastingCellClipboardContent" /> event.</summary>
	/// <param name="item">The data context for the selected element.</param>
	/// <param name="cellContent">The content to paste into the cell.</param>
	public virtual void OnPastingCellClipboardContent(object item, object cellContent)
	{
		if (ClipboardContentBinding != null)
		{
			if (this.PastingCellClipboardContent != null)
			{
				DataGridCellClipboardEventArgs dataGridCellClipboardEventArgs = new DataGridCellClipboardEventArgs(item, this, cellContent);
				this.PastingCellClipboardContent(this, dataGridCellClipboardEventArgs);
				cellContent = dataGridCellClipboardEventArgs.Content;
			}
			if (cellContent != null)
			{
				DataGridOwner.SetCellClipboardValue(item, this, cellContent);
			}
		}
	}

	internal virtual void OnInput(InputEventArgs e)
	{
	}

	internal void BeginEdit(InputEventArgs e, bool handled)
	{
		DataGrid dataGridOwner = DataGridOwner;
		if (dataGridOwner != null && dataGridOwner.BeginEdit(e))
		{
			e.Handled |= handled;
		}
	}

	private static object OnCoerceCanUserResize(DependencyObject d, object baseValue)
	{
		DataGridColumn dataGridColumn = d as DataGridColumn;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridColumn, baseValue, CanUserResizeProperty, dataGridColumn.DataGridOwner, DataGrid.CanUserResizeColumnsProperty);
	}

	internal bool CanColumnResize(DataGridLength width)
	{
		if (!CanUserResize)
		{
			return false;
		}
		if (width.DisplayValue >= MinWidth)
		{
			return width.DisplayValue <= MaxWidth;
		}
		return false;
	}

	private static void OnVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs eventArgs)
	{
		Visibility num = (Visibility)eventArgs.OldValue;
		Visibility visibility = (Visibility)eventArgs.NewValue;
		if (num == Visibility.Visible || visibility == Visibility.Visible)
		{
			((DataGridColumn)d).NotifyPropertyChanged(d, eventArgs, DataGridNotificationTarget.CellsPresenter | DataGridNotificationTarget.ColumnCollection | DataGridNotificationTarget.ColumnHeaders | DataGridNotificationTarget.ColumnHeadersPresenter | DataGridNotificationTarget.DataGrid);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridColumn" /> class. </summary>
	protected DataGridColumn()
	{
	}
}
