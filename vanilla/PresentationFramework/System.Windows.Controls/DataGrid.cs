using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Data;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents a control that displays data in a customizable grid.</summary>
public class DataGrid : MultiSelector
{
	private class ChangingSelectedCellsHelper : IDisposable
	{
		private DataGrid _dataGrid;

		private bool _wasUpdatingSelectedCells;

		internal ChangingSelectedCellsHelper(DataGrid dataGrid)
		{
			_dataGrid = dataGrid;
			_wasUpdatingSelectedCells = _dataGrid.IsUpdatingSelectedCells;
			if (!_wasUpdatingSelectedCells)
			{
				_dataGrid.BeginUpdateSelectedCells();
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			if (!_wasUpdatingSelectedCells)
			{
				_dataGrid.EndUpdateSelectedCells();
			}
		}
	}

	[Flags]
	private enum RelativeMousePositions
	{
		Over = 0,
		Above = 1,
		Below = 2,
		Left = 4,
		Right = 8
	}

	internal class CellAutomationValueHolder
	{
		private static DependencyProperty CellContentProperty = DependencyProperty.RegisterAttached("CellContent", typeof(string), typeof(CellAutomationValueHolder));

		private static DependencyProperty CellClipboardProperty = DependencyProperty.RegisterAttached("CellClipboard", typeof(object), typeof(CellAutomationValueHolder));

		private DataGridCell _cell;

		private DataGridColumn _column;

		private object _item;

		private string _value;

		private bool _inSetValue;

		public string Value => _value;

		public CellAutomationValueHolder(DataGridCell cell)
		{
			_cell = cell;
			Initialize(cell.RowDataItem, cell.Column);
		}

		public CellAutomationValueHolder(object item, DataGridColumn column)
		{
			Initialize(item, column);
		}

		private void Initialize(object item, DataGridColumn column)
		{
			_item = item;
			_column = column;
			_value = GetValue();
		}

		public void TrackValue()
		{
			string value = GetValue();
			if (!(value != _value))
			{
				return;
			}
			if (AutomationPeer.ListenerExists(AutomationEvents.PropertyChanged))
			{
				DataGridColumn dataGridColumn = ((_cell != null) ? _cell.Column : _column);
				if (dataGridColumn.DataGridOwner != null && UIElementAutomationPeer.FromElement(dataGridColumn.DataGridOwner) is DataGridAutomationPeer dataGridAutomationPeer)
				{
					object item = ((_cell != null) ? _cell.DataContext : _item);
					if (dataGridAutomationPeer.FindOrCreateItemAutomationPeer(item) is DataGridItemAutomationPeer dataGridItemAutomationPeer)
					{
						dataGridItemAutomationPeer.GetOrCreateCellItemPeer(dataGridColumn)?.RaisePropertyChangedEvent(ValuePatternIdentifiers.ValueProperty, _value, value);
					}
				}
			}
			_value = value;
		}

		private string GetValue()
		{
			string result;
			if (_column.ClipboardContentBinding == null)
			{
				result = null;
			}
			else if (_inSetValue)
			{
				result = (string)_cell.GetValue(CellContentProperty);
			}
			else
			{
				FrameworkElement frameworkElement;
				if (_cell != null)
				{
					frameworkElement = _cell;
				}
				else
				{
					frameworkElement = new FrameworkElement();
					frameworkElement.DataContext = _item;
				}
				BindingOperations.SetBinding(frameworkElement, CellContentProperty, _column.ClipboardContentBinding);
				result = (string)frameworkElement.GetValue(CellContentProperty);
				BindingOperations.ClearBinding(frameworkElement, CellContentProperty);
			}
			return result;
		}

		public object GetClipboardValue()
		{
			object result;
			if (_column.ClipboardContentBinding == null)
			{
				result = null;
			}
			else
			{
				FrameworkElement frameworkElement;
				if (_cell != null)
				{
					frameworkElement = _cell;
				}
				else
				{
					frameworkElement = new FrameworkElement();
					frameworkElement.DataContext = _item;
				}
				BindingOperations.SetBinding(frameworkElement, CellClipboardProperty, _column.ClipboardContentBinding);
				result = frameworkElement.GetValue(CellClipboardProperty);
				BindingOperations.ClearBinding(frameworkElement, CellClipboardProperty);
			}
			return result;
		}

		public void SetValue(DataGrid dataGrid, object value, bool clipboard)
		{
			if (_column.ClipboardContentBinding != null)
			{
				_inSetValue = true;
				DependencyProperty dp = (clipboard ? CellClipboardProperty : CellContentProperty);
				BindingBase binding = _column.ClipboardContentBinding.Clone(BindingMode.TwoWay);
				BindingOperations.SetBinding(_cell, dp, binding);
				_cell.SetValue(dp, value);
				dataGrid.CommitEdit();
				BindingOperations.ClearBinding(_cell, dp);
				_inSetValue = false;
			}
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.CanUserResizeColumns" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.CanUserResizeColumns" /> dependency property.</returns>
	public static readonly DependencyProperty CanUserResizeColumnsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.ColumnWidth" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.ColumnWidth" /> dependency property.</returns>
	public static readonly DependencyProperty ColumnWidthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.MinColumnWidth" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.MinColumnWidth" /> dependency property.</returns>
	public static readonly DependencyProperty MinColumnWidthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.MaxColumnWidth" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.MaxColumnWidth" /> dependency property.</returns>
	public static readonly DependencyProperty MaxColumnWidthProperty;

	private static readonly UncommonField<int> BringColumnIntoViewRetryCountField;

	private const int MaxBringColumnIntoViewRetries = 4;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.GridLinesVisibility" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.GridLinesVisibility" /> dependency property.</returns>
	public static readonly DependencyProperty GridLinesVisibilityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.HorizontalGridLinesBrush" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.HorizontalGridLinesBrush" /> dependency property.</returns>
	public static readonly DependencyProperty HorizontalGridLinesBrushProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.VerticalGridLinesBrush" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.VerticalGridLinesBrush" /> dependency property.</returns>
	public static readonly DependencyProperty VerticalGridLinesBrushProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.RowStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.RowStyle" /> dependency property.</returns>
	public static readonly DependencyProperty RowStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.RowValidationErrorTemplate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.RowValidationErrorTemplate" /> dependency property. </returns>
	public static readonly DependencyProperty RowValidationErrorTemplateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.RowStyleSelector" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.RowStyleSelector" /> dependency property.</returns>
	public static readonly DependencyProperty RowStyleSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.RowBackground" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.RowBackground" /> dependency property.</returns>
	public static readonly DependencyProperty RowBackgroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.AlternatingRowBackground" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.AlternatingRowBackground" /> dependency property.</returns>
	public static readonly DependencyProperty AlternatingRowBackgroundProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.RowHeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.RowHeight" /> dependency property.</returns>
	public static readonly DependencyProperty RowHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.MinRowHeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.MinRowHeight" /> dependency property.</returns>
	public static readonly DependencyProperty MinRowHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.RowHeaderWidth" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.RowHeaderWidth" /> dependency property.</returns>
	public static readonly DependencyProperty RowHeaderWidthProperty;

	private static readonly DependencyPropertyKey RowHeaderActualWidthPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.RowHeaderActualWidth" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.RowHeaderActualWidth" /> dependency property.</returns>
	public static readonly DependencyProperty RowHeaderActualWidthProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.ColumnHeaderHeight" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.ColumnHeaderHeight" /> dependency property.</returns>
	public static readonly DependencyProperty ColumnHeaderHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.HeadersVisibility" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.HeadersVisibility" /> dependency property.</returns>
	public static readonly DependencyProperty HeadersVisibilityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.CellStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.CellStyle" /> dependency property.</returns>
	public static readonly DependencyProperty CellStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.ColumnHeaderStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.ColumnHeaderStyle" /> dependency property.</returns>
	public static readonly DependencyProperty ColumnHeaderStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.RowHeaderStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.RowHeaderStyle" /> dependency property.</returns>
	public static readonly DependencyProperty RowHeaderStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.RowHeaderTemplate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.RowHeaderTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty RowHeaderTemplateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.RowHeaderTemplateSelector" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.RowHeaderTemplateSelector" /> dependency property.</returns>
	public static readonly DependencyProperty RowHeaderTemplateSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.HorizontalScrollBarVisibility" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.HorizontalScrollBarVisibility" /> dependency property.</returns>
	public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.VerticalScrollBarVisibility" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.VerticalScrollBarVisibility" /> dependency property.</returns>
	public static readonly DependencyProperty VerticalScrollBarVisibilityProperty;

	internal static readonly DependencyProperty HorizontalScrollOffsetProperty;

	/// <summary>Represents the command that indicates the intention to begin editing the current cell or row of the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	public static readonly RoutedCommand BeginEditCommand;

	/// <summary>Represents the command that indicates the intention to commit pending changes to the current cell or row and exit edit mode.</summary>
	public static readonly RoutedCommand CommitEditCommand;

	/// <summary>Represents the command that indicates the intention to cancel any pending changes to the current cell or row and revert to the state before the <see cref="F:System.Windows.Controls.DataGrid.BeginEditCommand" /> command was executed.</summary>
	public static readonly RoutedCommand CancelEditCommand;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.IsReadOnly" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.IsReadOnly" /> dependency property.</returns>
	public static readonly DependencyProperty IsReadOnlyProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.CurrentItem" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.CurrentItem" /> dependency property.</returns>
	public static readonly DependencyProperty CurrentItemProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.CurrentColumn" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.CurrentColumn" /> dependency property.</returns>
	public static readonly DependencyProperty CurrentColumnProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.CurrentCell" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.CurrentCell" /> dependency property.</returns>
	public static readonly DependencyProperty CurrentCellProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.CanUserAddRows" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.CanUserAddRows" /> dependency property.</returns>
	public static readonly DependencyProperty CanUserAddRowsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.CanUserDeleteRows" /> dependency property.</summary>
	/// <returns>Identifier for the <see cref="P:System.Windows.Controls.DataGrid.CanUserDeleteRows" /> dependency property.</returns>
	public static readonly DependencyProperty CanUserDeleteRowsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.RowDetailsVisibilityMode" /> dependency property.</summary>
	/// <returns>Identifier for the <see cref="P:System.Windows.Controls.DataGrid.RowDetailsVisibilityMode" /> dependency property.</returns>
	public static readonly DependencyProperty RowDetailsVisibilityModeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.AreRowDetailsFrozen" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.AreRowDetailsFrozen" /> dependency property.</returns>
	public static readonly DependencyProperty AreRowDetailsFrozenProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.RowDetailsTemplate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.RowDetailsTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty RowDetailsTemplateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.RowDetailsTemplateSelector" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.RowDetailsTemplateSelector" /> dependency property.</returns>
	public static readonly DependencyProperty RowDetailsTemplateSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.CanUserResizeRows" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.CanUserResizeRows" /> dependency property.</returns>
	public static readonly DependencyProperty CanUserResizeRowsProperty;

	private static readonly DependencyPropertyKey NewItemMarginPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.NewItemMargin" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.NewItemMargin" /> dependency property.</returns>
	public static readonly DependencyProperty NewItemMarginProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.SelectionMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.SelectionMode" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionModeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.SelectionUnit" /> dependency property.</summary>
	/// <returns>Identifier for the <see cref="P:System.Windows.Controls.DataGrid.SelectionUnit" /> dependency property.</returns>
	public static readonly DependencyProperty SelectionUnitProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.CanUserSortColumns" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.CanUserSortColumns" /> dependency property.</returns>
	public static readonly DependencyProperty CanUserSortColumnsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.AutoGenerateColumns" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.AutoGenerateColumns" /> dependency property.</returns>
	public static readonly DependencyProperty AutoGenerateColumnsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.FrozenColumnCount" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.FrozenColumnCount" /> dependency property.</returns>
	public static readonly DependencyProperty FrozenColumnCountProperty;

	private static readonly DependencyPropertyKey NonFrozenColumnsViewportHorizontalOffsetPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.NonFrozenColumnsViewportHorizontalOffset" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.NonFrozenColumnsViewportHorizontalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty NonFrozenColumnsViewportHorizontalOffsetProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.EnableRowVirtualization" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.EnableRowVirtualization" /> dependency property.</returns>
	public static readonly DependencyProperty EnableRowVirtualizationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.EnableColumnVirtualization" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.EnableColumnVirtualization" /> dependency property.</returns>
	public static readonly DependencyProperty EnableColumnVirtualizationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.CanUserReorderColumns" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.CanUserReorderColumns" /> dependency property.</returns>
	public static readonly DependencyProperty CanUserReorderColumnsProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.DragIndicatorStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.DragIndicatorStyle" /> dependency property.</returns>
	public static readonly DependencyProperty DragIndicatorStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.DropLocationIndicatorStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.DropLocationIndicatorStyle" /> dependency property.</returns>
	public static readonly DependencyProperty DropLocationIndicatorStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.ClipboardCopyMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.ClipboardCopyMode" /> dependency property.</returns>
	public static readonly DependencyProperty ClipboardCopyModeProperty;

	internal static readonly DependencyProperty CellsPanelActualWidthProperty;

	private static readonly DependencyPropertyKey CellsPanelHorizontalOffsetPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGrid.CellsPanelHorizontalOffset" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGrid.CellsPanelHorizontalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty CellsPanelHorizontalOffsetProperty;

	private static IValueConverter _headersVisibilityConverter;

	private static IValueConverter _rowDetailsScrollingConverter;

	private static object _newItemPlaceholder;

	private DataGridColumnCollection _columns;

	private ContainerTracking<DataGridRow> _rowTrackingRoot;

	private DataGridColumnHeadersPresenter _columnHeadersPresenter;

	private DataGridCell _currentCellContainer;

	private DataGridCell _pendingCurrentCellContainer;

	private SelectedCellsCollection _selectedCells;

	private List<ItemInfo> _pendingInfos;

	private DataGridCellInfo? _selectionAnchor;

	private bool _isDraggingSelection;

	private bool _isRowDragging;

	private Panel _internalItemsHost;

	private ScrollViewer _internalScrollHost;

	private ScrollContentPresenter _internalScrollContentPresenter;

	private DispatcherTimer _autoScrollTimer;

	private bool _hasAutoScrolled;

	private VirtualizedCellInfoCollection _pendingSelectedCells;

	private VirtualizedCellInfoCollection _pendingUnselectedCells;

	private bool _measureNeverInvoked = true;

	private bool _updatingSelectedCells;

	private Visibility _placeholderVisibility = Visibility.Collapsed;

	private Point _dragPoint;

	private List<int> _groupingSortDescriptionIndices;

	private bool _ignoreSortDescriptionsChange;

	private bool _sortingStarted;

	private ObservableCollection<ValidationRule> _rowValidationRules;

	private BindingGroup _defaultBindingGroup;

	private ItemInfo _editingRowInfo;

	private bool _hasCellValidationError;

	private bool _hasRowValidationError;

	private IEnumerable _cachedItemsSource;

	private DataGridItemAttachedStorage _itemAttachedStorage = new DataGridItemAttachedStorage();

	private bool _viewportWidthChangeNotificationPending;

	private double _originalViewportWidth;

	private double _finalViewportWidth;

	private Dictionary<DataGridColumn, CellAutomationValueHolder> _editingCellAutomationValueHolders = new Dictionary<DataGridColumn, CellAutomationValueHolder>();

	private DataGridCell _focusedCell;

	private bool _newItemMarginComputationPending;

	private const string ItemsPanelPartName = "PART_RowsPresenter";

	/// <summary>Gets a collection that contains all the columns in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>The collection of columns in the <see cref="T:System.Windows.Controls.DataGrid" />. </returns>
	public ObservableCollection<DataGridColumn> Columns => _columns;

	internal DataGridColumnCollection InternalColumns => _columns;

	/// <summary>Gets or sets a value that indicates whether the user can adjust the width of columns by using the mouse.</summary>
	/// <returns>true if the user can adjust the column width; otherwise, false. The registered default is true. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool CanUserResizeColumns
	{
		get
		{
			return (bool)GetValue(CanUserResizeColumnsProperty);
		}
		set
		{
			SetValue(CanUserResizeColumnsProperty, value);
		}
	}

	/// <summary>Gets or sets the standard width and sizing mode of columns and headers in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>The width and sizing mode of the columns and headers, in device-independent units (1/96th inch per unit). The registered default is <see cref="P:System.Windows.Controls.DataGridLength.SizeToHeader" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataGridLength ColumnWidth
	{
		get
		{
			return (DataGridLength)GetValue(ColumnWidthProperty);
		}
		set
		{
			SetValue(ColumnWidthProperty, value);
		}
	}

	/// <summary>Gets or sets the minimum width constraint of the columns and headers in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>The minimum width of the columns and headers, in device-independent units (1/96th inch per unit). The registered default is 20. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public double MinColumnWidth
	{
		get
		{
			return (double)GetValue(MinColumnWidthProperty);
		}
		set
		{
			SetValue(MinColumnWidthProperty, value);
		}
	}

	/// <summary>Gets or sets the maximum width constraint of the columns and headers in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>The maximum width of the columns and headers in the <see cref="T:System.Windows.Controls.DataGrid" />, in device-independent units (1/96th inch per unit). The registered default is <see cref="F:System.Double.PositiveInfinity" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public double MaxColumnWidth
	{
		get
		{
			return (double)GetValue(MaxColumnWidthProperty);
		}
		set
		{
			SetValue(MaxColumnWidthProperty, value);
		}
	}

	internal List<int> DisplayIndexMap => InternalColumns.DisplayIndexMap;

	internal DataGridColumnHeadersPresenter ColumnHeadersPresenter
	{
		get
		{
			return _columnHeadersPresenter;
		}
		set
		{
			_columnHeadersPresenter = value;
		}
	}

	/// <summary>Gets or sets a value that indicates which grid lines are shown.</summary>
	/// <returns>One of the enumeration values that specifies which grid lines are shown in the <see cref="T:System.Windows.Controls.DataGrid" />. The registered default is <see cref="F:System.Windows.Controls.DataGridGridLinesVisibility.All" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataGridGridLinesVisibility GridLinesVisibility
	{
		get
		{
			return (DataGridGridLinesVisibility)GetValue(GridLinesVisibilityProperty);
		}
		set
		{
			SetValue(GridLinesVisibilityProperty, value);
		}
	}

	/// <summary>Gets or sets the brush that is used to draw the horizontal grid lines.</summary>
	/// <returns>The brush that is used to draw the horizontal grid lines in the <see cref="T:System.Windows.Controls.DataGrid" />. The registered default is a black <see cref="T:System.Windows.Media.SolidColorBrush" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Brush HorizontalGridLinesBrush
	{
		get
		{
			return (Brush)GetValue(HorizontalGridLinesBrushProperty);
		}
		set
		{
			SetValue(HorizontalGridLinesBrushProperty, value);
		}
	}

	/// <summary>Gets or sets the brush that is used to draw the vertical grid lines.</summary>
	/// <returns>The brush that is used to draw the vertical grid lines in the <see cref="T:System.Windows.Controls.DataGrid" />. The registered default is a black <see cref="T:System.Windows.Media.SolidColorBrush" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Brush VerticalGridLinesBrush
	{
		get
		{
			return (Brush)GetValue(VerticalGridLinesBrushProperty);
		}
		set
		{
			SetValue(VerticalGridLinesBrushProperty, value);
		}
	}

	internal double HorizontalGridLineThickness => 1.0;

	internal double VerticalGridLineThickness => 1.0;

	/// <summary>Gets or sets the style applied to all rows.</summary>
	/// <returns>The style applied to all rows in the <see cref="T:System.Windows.Controls.DataGrid" />. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Style RowStyle
	{
		get
		{
			return (Style)GetValue(RowStyleProperty);
		}
		set
		{
			SetValue(RowStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the template that is used to visually indicate an error in row validation.</summary>
	/// <returns>The template that is used to visually indicate an error in row validation. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public ControlTemplate RowValidationErrorTemplate
	{
		get
		{
			return (ControlTemplate)GetValue(RowValidationErrorTemplateProperty);
		}
		set
		{
			SetValue(RowValidationErrorTemplateProperty, value);
		}
	}

	/// <summary>Gets the rules that are used to validate the data in each row.</summary>
	/// <returns>The rules that are used to validate the data in each row. </returns>
	public ObservableCollection<ValidationRule> RowValidationRules => _rowValidationRules;

	/// <summary>Gets or sets the style selector for the rows.</summary>
	/// <returns>The style selector for the rows. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public StyleSelector RowStyleSelector
	{
		get
		{
			return (StyleSelector)GetValue(RowStyleSelectorProperty);
		}
		set
		{
			SetValue(RowStyleSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets the default brush for the row background.</summary>
	/// <returns>The brush that paints the background of a row. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Brush RowBackground
	{
		get
		{
			return (Brush)GetValue(RowBackgroundProperty);
		}
		set
		{
			SetValue(RowBackgroundProperty, value);
		}
	}

	/// <summary>Gets or sets the background brush for use on alternating rows.</summary>
	/// <returns>The <see cref="T:System.Windows.Media.Brush" /> that paints the background of every nth row where n is defined by the <see cref="P:System.Windows.Controls.ItemsControl.AlternationCount" /> property. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Brush AlternatingRowBackground
	{
		get
		{
			return (Brush)GetValue(AlternatingRowBackgroundProperty);
		}
		set
		{
			SetValue(AlternatingRowBackgroundProperty, value);
		}
	}

	/// <summary>Gets or sets the suggested height for all rows.</summary>
	/// <returns>The height of the rows, in device-independent units (1/96th inch per unit). The registered default is <see cref="F:System.Double.NaN" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public double RowHeight
	{
		get
		{
			return (double)GetValue(RowHeightProperty);
		}
		set
		{
			SetValue(RowHeightProperty, value);
		}
	}

	/// <summary>Gets or sets the minimum height constraint of the rows and headers in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>The minimum height constraint of rows, in device-independent units (1/96th inch per unit). The registered default is 0.0. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public double MinRowHeight
	{
		get
		{
			return (double)GetValue(MinRowHeightProperty);
		}
		set
		{
			SetValue(MinRowHeightProperty, value);
		}
	}

	internal Visibility PlaceholderVisibility => _placeholderVisibility;

	/// <summary>Gets or sets the width of the row header column.</summary>
	/// <returns>The width of the row header column, in device-independent units (1/96th inch per unit). The registered default is <see cref="F:System.Double.NaN" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public double RowHeaderWidth
	{
		get
		{
			return (double)GetValue(RowHeaderWidthProperty);
		}
		set
		{
			SetValue(RowHeaderWidthProperty, value);
		}
	}

	/// <summary>Gets the rendered width of the row headers column.</summary>
	/// <returns>The rendered width of the row header, in device-independent units (1/96th inch per unit). The registered default is 0.0. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public double RowHeaderActualWidth
	{
		get
		{
			return (double)GetValue(RowHeaderActualWidthProperty);
		}
		internal set
		{
			SetValue(RowHeaderActualWidthPropertyKey, value);
		}
	}

	/// <summary>Gets or sets the height of the column headers row.</summary>
	/// <returns>The height of the column headers row, in device-independent units (1/96th inch per unit). The registered default is <see cref="F:System.Double.NaN" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public double ColumnHeaderHeight
	{
		get
		{
			return (double)GetValue(ColumnHeaderHeightProperty);
		}
		set
		{
			SetValue(ColumnHeaderHeightProperty, value);
		}
	}

	/// <summary>Gets or sets the value that specifies the visibility of the row and column headers.</summary>
	/// <returns>One of the enumeration values that indicates the visibility of row and column headers. The registered default is <see cref="F:System.Windows.Controls.DataGridHeadersVisibility.All" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataGridHeadersVisibility HeadersVisibility
	{
		get
		{
			return (DataGridHeadersVisibility)GetValue(HeadersVisibilityProperty);
		}
		set
		{
			SetValue(HeadersVisibilityProperty, value);
		}
	}

	internal DataGridItemAttachedStorage ItemAttachedStorage => _itemAttachedStorage;

	private bool ShouldSelectRowHeader
	{
		get
		{
			if (_selectionAnchor.HasValue && base.SelectedItems.Contains(_selectionAnchor.Value.Item) && SelectionUnit == DataGridSelectionUnit.CellOrRowHeader)
			{
				return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
			}
			return false;
		}
	}

	/// <summary>Gets or sets the style applied to all cells in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>The style applied to the cells in the <see cref="T:System.Windows.Controls.DataGrid" />. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
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

	/// <summary>Gets or sets the style applied to all column headers in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>The style applied to all column headers in the <see cref="T:System.Windows.Controls.DataGrid" />. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Style ColumnHeaderStyle
	{
		get
		{
			return (Style)GetValue(ColumnHeaderStyleProperty);
		}
		set
		{
			SetValue(ColumnHeaderStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the style applied to all row headers.</summary>
	/// <returns>The style applied to all row headers in the <see cref="T:System.Windows.Controls.DataGrid" />. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Style RowHeaderStyle
	{
		get
		{
			return (Style)GetValue(RowHeaderStyleProperty);
		}
		set
		{
			SetValue(RowHeaderStyleProperty, value);
		}
	}

	/// <summary>Gets or set the template for the row headers.</summary>
	/// <returns>The template for the row headers. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataTemplate RowHeaderTemplate
	{
		get
		{
			return (DataTemplate)GetValue(RowHeaderTemplateProperty);
		}
		set
		{
			SetValue(RowHeaderTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets the template selector for row headers.</summary>
	/// <returns>The template selector for row headers. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataTemplateSelector RowHeaderTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(RowHeaderTemplateSelectorProperty);
		}
		set
		{
			SetValue(RowHeaderTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets the key that references the default border brush for a focused cell.</summary>
	/// <returns>The key that references the default brush for a focused cell. </returns>
	public static ComponentResourceKey FocusBorderBrushKey => SystemResourceKey.DataGridFocusBorderBrushKey;

	/// <summary>Gets the converter that converts a <see cref="T:System.Windows.Controls.DataGridHeadersVisibility" /> to a <see cref="T:System.Windows.Visibility" />.</summary>
	/// <returns>The converter that converts a <see cref="T:System.Windows.Controls.DataGridHeadersVisibility" /> to a <see cref="T:System.Windows.Visibility" />.</returns>
	public static IValueConverter HeadersVisibilityConverter
	{
		get
		{
			if (_headersVisibilityConverter == null)
			{
				_headersVisibilityConverter = new DataGridHeadersVisibilityToVisibilityConverter();
			}
			return _headersVisibilityConverter;
		}
	}

	/// <summary>Gets the converter that converts a Boolean value to a <see cref="T:System.Windows.Controls.SelectiveScrollingOrientation" />.</summary>
	/// <returns>The converter that converts a Boolean value to a <see cref="T:System.Windows.Controls.SelectiveScrollingOrientation" />.</returns>
	public static IValueConverter RowDetailsScrollingConverter
	{
		get
		{
			if (_rowDetailsScrollingConverter == null)
			{
				_rowDetailsScrollingConverter = new BooleanToSelectiveScrollingOrientationConverter();
			}
			return _rowDetailsScrollingConverter;
		}
	}

	/// <summary>Gets or sets a value that indicates how horizontal scroll bars are displayed in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>One of the enumeration values that specifies the visibility of horizontal scroll bars in the <see cref="T:System.Windows.Controls.DataGrid" />. The registered default is <see cref="F:System.Windows.Controls.ScrollBarVisibility.Auto" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public ScrollBarVisibility HorizontalScrollBarVisibility
	{
		get
		{
			return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty);
		}
		set
		{
			SetValue(HorizontalScrollBarVisibilityProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates how vertical scroll bars are displayed in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>One of the enumeration values that specifies the visibility of vertical scroll bars in the <see cref="T:System.Windows.Controls.DataGrid" />. The registered default is <see cref="F:System.Windows.Controls.ScrollBarVisibility.Auto" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public ScrollBarVisibility VerticalScrollBarVisibility
	{
		get
		{
			return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty);
		}
		set
		{
			SetValue(VerticalScrollBarVisibilityProperty, value);
		}
	}

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.DataGrid" /> supports custom keyboard scrolling.</summary>
	/// <returns>true in all cases.</returns>
	protected internal override bool HandlesScrolling => true;

	internal Panel InternalItemsHost
	{
		get
		{
			return _internalItemsHost;
		}
		set
		{
			if (_internalItemsHost != value)
			{
				_internalItemsHost = value;
				if (_internalItemsHost != null)
				{
					DetermineItemsHostStarBehavior();
					EnsureInternalScrollControls();
				}
			}
		}
	}

	internal ScrollViewer InternalScrollHost
	{
		get
		{
			EnsureInternalScrollControls();
			return _internalScrollHost;
		}
	}

	internal ScrollContentPresenter InternalScrollContentPresenter
	{
		get
		{
			EnsureInternalScrollControls();
			return _internalScrollContentPresenter;
		}
	}

	internal double HorizontalScrollOffset => (double)GetValue(HorizontalScrollOffsetProperty);

	/// <summary>Represents the command that indicates the intention to delete the current row.</summary>
	/// <returns>The <see cref="P:System.Windows.Input.ApplicationCommands.Delete" /> command that indicates the intention to delete the current row.</returns>
	public static RoutedUICommand DeleteCommand => ApplicationCommands.Delete;

	/// <summary>Gets or sets a value that indicates whether the user can edit values in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>true if the rows and cells are read-only; otherwise, false. The registered default is false. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
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

	/// <summary>Gets the data item bound to the row that contains the current cell.</summary>
	/// <returns>The data item bound to the row that contains the current cell. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public object CurrentItem
	{
		get
		{
			return GetValue(CurrentItemProperty);
		}
		set
		{
			SetValue(CurrentItemProperty, value);
		}
	}

	/// <summary>Gets or sets the column that contains the current cell.</summary>
	/// <returns>The column that contains the current cell. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataGridColumn CurrentColumn
	{
		get
		{
			return (DataGridColumn)GetValue(CurrentColumnProperty);
		}
		set
		{
			SetValue(CurrentColumnProperty, value);
		}
	}

	/// <summary>Gets or sets the cell that has focus.</summary>
	/// <returns>Information about the cell that has focus. </returns>
	public DataGridCellInfo CurrentCell
	{
		get
		{
			return (DataGridCellInfo)GetValue(CurrentCellProperty);
		}
		set
		{
			SetValue(CurrentCellProperty, value);
		}
	}

	internal DataGridCell CurrentCellContainer
	{
		get
		{
			if (_currentCellContainer == null)
			{
				DataGridCellInfo currentCell = CurrentCell;
				if (currentCell.IsValid)
				{
					_currentCellContainer = TryFindCell(currentCell);
				}
			}
			return _currentCellContainer;
		}
		set
		{
			if (_currentCellContainer != value && (value == null || value != _pendingCurrentCellContainer))
			{
				_pendingCurrentCellContainer = value;
				if (value == null)
				{
					SetCurrentValueInternal(CurrentCellProperty, DataGridCellInfo.Unset);
				}
				else
				{
					SetCurrentValueInternal(CurrentCellProperty, new DataGridCellInfo(value));
				}
				_pendingCurrentCellContainer = null;
				_currentCellContainer = value;
				CommandManager.InvalidateRequerySuggested();
			}
		}
	}

	private bool IsEditingCurrentCell => CurrentCellContainer?.IsEditing ?? false;

	private bool IsCurrentCellReadOnly => CurrentCellContainer?.IsReadOnly ?? false;

	internal ItemInfo CurrentInfo => LeaseItemInfo(CurrentCell.ItemInfo);

	private bool HasCellValidationError
	{
		get
		{
			return _hasCellValidationError;
		}
		set
		{
			if (_hasCellValidationError != value)
			{
				_hasCellValidationError = value;
				CommandManager.InvalidateRequerySuggested();
			}
		}
	}

	private bool HasRowValidationError
	{
		get
		{
			return _hasRowValidationError;
		}
		set
		{
			if (_hasRowValidationError != value)
			{
				_hasRowValidationError = value;
				CommandManager.InvalidateRequerySuggested();
			}
		}
	}

	internal DataGridCell FocusedCell
	{
		get
		{
			return _focusedCell;
		}
		set
		{
			if (_focusedCell != value)
			{
				if (_focusedCell != null)
				{
					UpdateCurrentCell(_focusedCell, isFocusWithinCell: false);
				}
				_focusedCell = value;
				if (_focusedCell != null)
				{
					UpdateCurrentCell(_focusedCell, isFocusWithinCell: true);
				}
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether the user can add new rows to the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>true if the user can add new rows; otherwise, false. The registered default is true. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool CanUserAddRows
	{
		get
		{
			return (bool)GetValue(CanUserAddRowsProperty);
		}
		set
		{
			SetValue(CanUserAddRowsProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the user can delete rows from the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>true if the user can delete rows; otherwise, false. The registered default is true. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool CanUserDeleteRows
	{
		get
		{
			return (bool)GetValue(CanUserDeleteRowsProperty);
		}
		set
		{
			SetValue(CanUserDeleteRowsProperty, value);
		}
	}

	private IEditableCollectionView EditableItems => base.Items;

	private bool IsAddingNewItem => EditableItems.IsAddingNew;

	private bool IsEditingRowItem => EditableItems.IsEditingItem;

	private int DataItemsCount
	{
		get
		{
			int num = base.Items.Count;
			if (HasNewItemPlaceholder)
			{
				num--;
			}
			return num;
		}
	}

	private int DataItemsSelected
	{
		get
		{
			int num = base.SelectedItems.Count;
			if (HasNewItemPlaceholder && base.SelectedItems.Contains(CollectionView.NewItemPlaceholder))
			{
				num--;
			}
			return num;
		}
	}

	private bool HasNewItemPlaceholder => EditableItems.NewItemPlaceholderPosition != NewItemPlaceholderPosition.None;

	/// <summary>Gets or sets a value that indicates when the details section of a row is displayed.</summary>
	/// <returns>One of the enumeration values that specifies the visibility of row details. The registered default is <see cref="F:System.Windows.Controls.DataGridRowDetailsVisibilityMode.VisibleWhenSelected" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataGridRowDetailsVisibilityMode RowDetailsVisibilityMode
	{
		get
		{
			return (DataGridRowDetailsVisibilityMode)GetValue(RowDetailsVisibilityModeProperty);
		}
		set
		{
			SetValue(RowDetailsVisibilityModeProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the row details can scroll horizontally.</summary>
	/// <returns>true if row details cannot scroll; otherwise, false. The registered default is false. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool AreRowDetailsFrozen
	{
		get
		{
			return (bool)GetValue(AreRowDetailsFrozenProperty);
		}
		set
		{
			SetValue(AreRowDetailsFrozenProperty, value);
		}
	}

	/// <summary>Gets or sets the template that is used to display the row details.</summary>
	/// <returns>The template that is used to display the row details. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataTemplate RowDetailsTemplate
	{
		get
		{
			return (DataTemplate)GetValue(RowDetailsTemplateProperty);
		}
		set
		{
			SetValue(RowDetailsTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets the template selector that is used for the row details.</summary>
	/// <returns>The template selector that is used for the row details. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataTemplateSelector RowDetailsTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(RowDetailsTemplateSelectorProperty);
		}
		set
		{
			SetValue(RowDetailsTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the user can adjust the height of rows by using the mouse.</summary>
	/// <returns>true if the user can change the height of the rows; otherwise, false. The registered default is true. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool CanUserResizeRows
	{
		get
		{
			return (bool)GetValue(CanUserResizeRowsProperty);
		}
		set
		{
			SetValue(CanUserResizeRowsProperty, value);
		}
	}

	/// <summary>Gets or sets the margin for the new item row.</summary>
	/// <returns>The margin for the new item row.The registered default is 0. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public Thickness NewItemMargin
	{
		get
		{
			return (Thickness)GetValue(NewItemMarginProperty);
		}
		private set
		{
			SetValue(NewItemMarginPropertyKey, value);
		}
	}

	internal SelectedItemCollection SelectedItemCollection => (SelectedItemCollection)base.SelectedItems;

	/// <summary>Gets the list of cells that are currently selected.</summary>
	/// <returns>The list of cells that are currently selected. </returns>
	public IList<DataGridCellInfo> SelectedCells => _selectedCells;

	internal SelectedCellsCollection SelectedCellsInternal => _selectedCells;

	/// <summary>Represents the command that indicates the intention to select all cells in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>The <see cref="P:System.Windows.Input.ApplicationCommands.SelectAll" /> command that indicates the intention to select all cells in the <see cref="T:System.Windows.Controls.DataGrid" />.</returns>
	public static RoutedUICommand SelectAllCommand => ApplicationCommands.SelectAll;

	/// <summary>Gets or sets a value that indicates how rows and cells are selected in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>One of the enumeration values that specifies how rows and cells are selected in the <see cref="T:System.Windows.Controls.DataGrid" />. The registered default is <see cref="F:System.Windows.Controls.DataGridSelectionMode.Extended" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataGridSelectionMode SelectionMode
	{
		get
		{
			return (DataGridSelectionMode)GetValue(SelectionModeProperty);
		}
		set
		{
			SetValue(SelectionModeProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether rows, cells, or both can be selected in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>One of the enumeration values that specifies whether rows, cells, or both can be selected in the <see cref="T:System.Windows.Controls.DataGrid" />. The registered default is <see cref="F:System.Windows.Controls.DataGridSelectionUnit.FullRow" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataGridSelectionUnit SelectionUnit
	{
		get
		{
			return (DataGridSelectionUnit)GetValue(SelectionUnitProperty);
		}
		set
		{
			SetValue(SelectionUnitProperty, value);
		}
	}

	private bool IsUpdatingSelectedCells => _updatingSelectedCells;

	private bool ShouldExtendSelection
	{
		get
		{
			if (base.CanSelectMultipleItems && _selectionAnchor.HasValue)
			{
				if (!_isDraggingSelection)
				{
					return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
				}
				return true;
			}
			return false;
		}
	}

	private static bool ShouldMinimallyModifySelection => (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

	private bool CanSelectRows
	{
		get
		{
			switch (SelectionUnit)
			{
			case DataGridSelectionUnit.FullRow:
			case DataGridSelectionUnit.CellOrRowHeader:
				return true;
			case DataGridSelectionUnit.Cell:
				return false;
			default:
				return false;
			}
		}
	}

	private DataGridRow MouseOverRow
	{
		get
		{
			UIElement uIElement = Mouse.DirectlyOver as UIElement;
			DataGridRow dataGridRow = null;
			while (uIElement != null)
			{
				dataGridRow = DataGridHelper.FindVisualParent<DataGridRow>(uIElement);
				if (dataGridRow == null || dataGridRow.DataGridOwner == this)
				{
					break;
				}
				uIElement = VisualTreeHelper.GetParent(dataGridRow) as UIElement;
			}
			return dataGridRow;
		}
	}

	private DataGridCell MouseOverCell
	{
		get
		{
			UIElement uIElement = Mouse.DirectlyOver as UIElement;
			DataGridCell dataGridCell = null;
			while (uIElement != null)
			{
				dataGridCell = DataGridHelper.FindVisualParent<DataGridCell>(uIElement);
				if (dataGridCell == null || dataGridCell.DataGridOwner == this)
				{
					break;
				}
				uIElement = VisualTreeHelper.GetParent(dataGridCell) as UIElement;
			}
			return dataGridCell;
		}
	}

	private RelativeMousePositions RelativeMousePosition
	{
		get
		{
			RelativeMousePositions relativeMousePositions = RelativeMousePositions.Over;
			Panel internalItemsHost = InternalItemsHost;
			if (internalItemsHost != null)
			{
				Point position = Mouse.GetPosition(internalItemsHost);
				Rect rect = new Rect(default(Point), internalItemsHost.RenderSize);
				if (position.X < rect.Left)
				{
					relativeMousePositions |= RelativeMousePositions.Left;
				}
				else if (position.X > rect.Right)
				{
					relativeMousePositions |= RelativeMousePositions.Right;
				}
				if (position.Y < rect.Top)
				{
					relativeMousePositions |= RelativeMousePositions.Above;
				}
				else if (position.Y > rect.Bottom)
				{
					relativeMousePositions |= RelativeMousePositions.Below;
				}
			}
			return relativeMousePositions;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the user can sort columns by clicking the column header.</summary>
	/// <returns>true if the user can sort the columns; otherwise, false. The registered default is true. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool CanUserSortColumns
	{
		get
		{
			return (bool)GetValue(CanUserSortColumnsProperty);
		}
		set
		{
			SetValue(CanUserSortColumnsProperty, value);
		}
	}

	private List<int> GroupingSortDescriptionIndices
	{
		get
		{
			return _groupingSortDescriptionIndices;
		}
		set
		{
			_groupingSortDescriptionIndices = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the columns are created automatically. </summary>
	/// <returns>true if columns are created automatically; otherwise, false. The registered default is true. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool AutoGenerateColumns
	{
		get
		{
			return (bool)GetValue(AutoGenerateColumnsProperty);
		}
		set
		{
			SetValue(AutoGenerateColumnsProperty, value);
		}
	}

	private bool DeferAutoGeneration { get; set; }

	/// <summary>Gets or sets the number of non-scrolling columns.</summary>
	/// <returns>The number of non-scrolling columns. The registered default is 0. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public int FrozenColumnCount
	{
		get
		{
			return (int)GetValue(FrozenColumnCountProperty);
		}
		set
		{
			SetValue(FrozenColumnCountProperty, value);
		}
	}

	/// <summary>Gets the horizontal offset of the scrollable columns in the view port.</summary>
	/// <returns>The horizontal offset of the scrollable columns. The registered default is 0.0. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public double NonFrozenColumnsViewportHorizontalOffset
	{
		get
		{
			return (double)GetValue(NonFrozenColumnsViewportHorizontalOffsetProperty);
		}
		internal set
		{
			SetValue(NonFrozenColumnsViewportHorizontalOffsetPropertyKey, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether row virtualization is enabled.</summary>
	/// <returns>true if row virtualization is enabled; otherwise, false. The registered default is true. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool EnableRowVirtualization
	{
		get
		{
			return (bool)GetValue(EnableRowVirtualizationProperty);
		}
		set
		{
			SetValue(EnableRowVirtualizationProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether column virtualization is enabled.</summary>
	/// <returns>true if column virtualization is enabled; otherwise, false. The registered default is false. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool EnableColumnVirtualization
	{
		get
		{
			return (bool)GetValue(EnableColumnVirtualizationProperty);
		}
		set
		{
			SetValue(EnableColumnVirtualizationProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the user can change the column display order by dragging column headers with the mouse.</summary>
	/// <returns>true if the user can reorder columns; otherwise, false. The registered default is true. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool CanUserReorderColumns
	{
		get
		{
			return (bool)GetValue(CanUserReorderColumnsProperty);
		}
		set
		{
			SetValue(CanUserReorderColumnsProperty, value);
		}
	}

	/// <summary>Gets or sets the style that is used when rendering the drag indicator that is displayed while dragging a column header.</summary>
	/// <returns>The style applied to a column header when dragging. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
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

	/// <summary>Gets or sets the style that is applied to indicate the drop location when dragging a column header.</summary>
	/// <returns>The style applied to a column header. The registered default is null. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Style DropLocationIndicatorStyle
	{
		get
		{
			return (Style)GetValue(DropLocationIndicatorStyleProperty);
		}
		set
		{
			SetValue(DropLocationIndicatorStyleProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates how content is copied to the clipboard.</summary>
	/// <returns>One of the enumeration values that indicates how content is copied to the clipboard. The registered default is <see cref="F:System.Windows.Controls.DataGridClipboardCopyMode.ExcludeHeader" />. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataGridClipboardCopyMode ClipboardCopyMode
	{
		get
		{
			return (DataGridClipboardCopyMode)GetValue(ClipboardCopyModeProperty);
		}
		set
		{
			SetValue(ClipboardCopyModeProperty, value);
		}
	}

	internal double CellsPanelActualWidth
	{
		get
		{
			return (double)GetValue(CellsPanelActualWidthProperty);
		}
		set
		{
			SetValue(CellsPanelActualWidthProperty, value);
		}
	}

	/// <summary>Gets the horizontal offset for the <see cref="T:System.Windows.Controls.DataGridCellsPanel" />.</summary>
	/// <returns>The horizontal offset for the cells panel. The registered default is 0.0. For more information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public double CellsPanelHorizontalOffset
	{
		get
		{
			return (double)GetValue(CellsPanelHorizontalOffsetProperty);
		}
		private set
		{
			SetValue(CellsPanelHorizontalOffsetPropertyKey, value);
		}
	}

	private bool CellsPanelHorizontalOffsetComputationPending { get; set; }

	internal static object NewItemPlaceholder => _newItemPlaceholder;

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.DataGridColumn.DisplayIndex" /> property on one of the columns changes.</summary>
	public event EventHandler<DataGridColumnEventArgs> ColumnDisplayIndexChanged;

	/// <summary>Occurs after a <see cref="T:System.Windows.Controls.DataGridRow" /> is instantiated, so that you can customize it before it is used.</summary>
	public event EventHandler<DataGridRowEventArgs> LoadingRow;

	/// <summary>Occurs when a <see cref="T:System.Windows.Controls.DataGridRow" /> object becomes available for reuse.</summary>
	public event EventHandler<DataGridRowEventArgs> UnloadingRow;

	/// <summary>Occurs before a row edit is committed or canceled. </summary>
	public event EventHandler<DataGridRowEditEndingEventArgs> RowEditEnding;

	/// <summary>Occurs before a cell edit is committed or canceled. </summary>
	public event EventHandler<DataGridCellEditEndingEventArgs> CellEditEnding;

	/// <summary>Occurs when the value of the <see cref="P:System.Windows.Controls.DataGrid.CurrentCell" /> property has changed.</summary>
	public event EventHandler<EventArgs> CurrentCellChanged;

	/// <summary>Occurs before a row or cell enters edit mode.</summary>
	public event EventHandler<DataGridBeginningEditEventArgs> BeginningEdit;

	/// <summary>Occurs when a cell enters edit mode. </summary>
	public event EventHandler<DataGridPreparingCellForEditEventArgs> PreparingCellForEdit;

	/// <summary>Occurs before a new item is added to the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	public event EventHandler<AddingNewItemEventArgs> AddingNewItem;

	/// <summary>Occurs when a new item is created. </summary>
	public event InitializingNewItemEventHandler InitializingNewItem;

	/// <summary>Occurs when a new row details template is applied to a row.</summary>
	public event EventHandler<DataGridRowDetailsEventArgs> LoadingRowDetails;

	/// <summary>Occurs when a row details element becomes available for reuse.</summary>
	public event EventHandler<DataGridRowDetailsEventArgs> UnloadingRowDetails;

	/// <summary>Occurs when the visibility of a row details element changes.</summary>
	public event EventHandler<DataGridRowDetailsEventArgs> RowDetailsVisibilityChanged;

	/// <summary>Occurs when the <see cref="P:System.Windows.Controls.DataGrid.SelectedCells" /> collection changes.</summary>
	public event SelectedCellsChangedEventHandler SelectedCellsChanged;

	/// <summary>Occurs when a column is being sorted.</summary>
	public event DataGridSortingEventHandler Sorting;

	/// <summary>Occurs when auto generation of all columns is completed.</summary>
	public event EventHandler AutoGeneratedColumns;

	/// <summary>Occurs when an individual column is auto-generated.</summary>
	public event EventHandler<DataGridAutoGeneratingColumnEventArgs> AutoGeneratingColumn;

	/// <summary>Occurs before a column moves to a new position in the display order.</summary>
	public event EventHandler<DataGridColumnReorderingEventArgs> ColumnReordering;

	/// <summary>Occurs when the user begins dragging a column header by using the mouse.</summary>
	public event EventHandler<DragStartedEventArgs> ColumnHeaderDragStarted;

	/// <summary>Occurs every time the mouse position changes while the user drags a column header. </summary>
	public event EventHandler<DragDeltaEventArgs> ColumnHeaderDragDelta;

	/// <summary>Occurs when the user releases a column header after dragging it by using the mouse.</summary>
	public event EventHandler<DragCompletedEventArgs> ColumnHeaderDragCompleted;

	/// <summary>Occurs when a column moves to a new position in the display order.</summary>
	public event EventHandler<DataGridColumnEventArgs> ColumnReordered;

	/// <summary>Occurs after the default row content is prepared. </summary>
	public event EventHandler<DataGridRowClipboardEventArgs> CopyingRowClipboardContent;

	static DataGrid()
	{
		CanUserResizeColumnsProperty = DependencyProperty.Register("CanUserResizeColumns", typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(true, OnNotifyColumnAndColumnHeaderPropertyChanged));
		ColumnWidthProperty = DependencyProperty.Register("ColumnWidth", typeof(DataGridLength), typeof(DataGrid), new FrameworkPropertyMetadata(DataGridLength.SizeToHeader));
		MinColumnWidthProperty = DependencyProperty.Register("MinColumnWidth", typeof(double), typeof(DataGrid), new FrameworkPropertyMetadata(20.0, OnColumnSizeConstraintChanged), ValidateMinColumnWidth);
		MaxColumnWidthProperty = DependencyProperty.Register("MaxColumnWidth", typeof(double), typeof(DataGrid), new FrameworkPropertyMetadata(double.PositiveInfinity, OnColumnSizeConstraintChanged), ValidateMaxColumnWidth);
		BringColumnIntoViewRetryCountField = new UncommonField<int>(0);
		GridLinesVisibilityProperty = DependencyProperty.Register("GridLinesVisibility", typeof(DataGridGridLinesVisibility), typeof(DataGrid), new FrameworkPropertyMetadata(DataGridGridLinesVisibility.All, OnNotifyGridLinePropertyChanged));
		HorizontalGridLinesBrushProperty = DependencyProperty.Register("HorizontalGridLinesBrush", typeof(Brush), typeof(DataGrid), new FrameworkPropertyMetadata(Brushes.Black, OnNotifyGridLinePropertyChanged));
		VerticalGridLinesBrushProperty = DependencyProperty.Register("VerticalGridLinesBrush", typeof(Brush), typeof(DataGrid), new FrameworkPropertyMetadata(Brushes.Black, OnNotifyGridLinePropertyChanged));
		RowStyleProperty = DependencyProperty.Register("RowStyle", typeof(Style), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnRowStyleChanged));
		RowValidationErrorTemplateProperty = DependencyProperty.Register("RowValidationErrorTemplate", typeof(ControlTemplate), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnNotifyRowPropertyChanged));
		RowStyleSelectorProperty = DependencyProperty.Register("RowStyleSelector", typeof(StyleSelector), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnRowStyleSelectorChanged));
		RowBackgroundProperty = DependencyProperty.Register("RowBackground", typeof(Brush), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnNotifyRowPropertyChanged));
		AlternatingRowBackgroundProperty = DependencyProperty.Register("AlternatingRowBackground", typeof(Brush), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnNotifyDataGridAndRowPropertyChanged));
		RowHeightProperty = DependencyProperty.Register("RowHeight", typeof(double), typeof(DataGrid), new FrameworkPropertyMetadata(double.NaN, OnNotifyCellsPresenterPropertyChanged));
		MinRowHeightProperty = DependencyProperty.Register("MinRowHeight", typeof(double), typeof(DataGrid), new FrameworkPropertyMetadata(0.0, OnNotifyCellsPresenterPropertyChanged));
		RowHeaderWidthProperty = DependencyProperty.Register("RowHeaderWidth", typeof(double), typeof(DataGrid), new FrameworkPropertyMetadata(double.NaN, OnNotifyRowHeaderWidthPropertyChanged));
		RowHeaderActualWidthPropertyKey = DependencyProperty.RegisterReadOnly("RowHeaderActualWidth", typeof(double), typeof(DataGrid), new FrameworkPropertyMetadata(0.0, OnNotifyRowHeaderPropertyChanged));
		RowHeaderActualWidthProperty = RowHeaderActualWidthPropertyKey.DependencyProperty;
		ColumnHeaderHeightProperty = DependencyProperty.Register("ColumnHeaderHeight", typeof(double), typeof(DataGrid), new FrameworkPropertyMetadata(double.NaN, OnNotifyColumnHeaderPropertyChanged));
		HeadersVisibilityProperty = DependencyProperty.Register("HeadersVisibility", typeof(DataGridHeadersVisibility), typeof(DataGrid), new FrameworkPropertyMetadata(DataGridHeadersVisibility.All));
		CellStyleProperty = DependencyProperty.Register("CellStyle", typeof(Style), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnNotifyColumnAndCellPropertyChanged));
		ColumnHeaderStyleProperty = DependencyProperty.Register("ColumnHeaderStyle", typeof(Style), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnNotifyColumnAndColumnHeaderPropertyChanged));
		RowHeaderStyleProperty = DependencyProperty.Register("RowHeaderStyle", typeof(Style), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnNotifyRowAndRowHeaderPropertyChanged));
		RowHeaderTemplateProperty = DependencyProperty.Register("RowHeaderTemplate", typeof(DataTemplate), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnNotifyRowAndRowHeaderPropertyChanged));
		RowHeaderTemplateSelectorProperty = DependencyProperty.Register("RowHeaderTemplateSelector", typeof(DataTemplateSelector), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnNotifyRowAndRowHeaderPropertyChanged));
		HorizontalScrollBarVisibilityProperty = ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner(typeof(DataGrid), new FrameworkPropertyMetadata(ScrollBarVisibility.Auto));
		VerticalScrollBarVisibilityProperty = ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner(typeof(DataGrid), new FrameworkPropertyMetadata(ScrollBarVisibility.Auto));
		HorizontalScrollOffsetProperty = DependencyProperty.Register("HorizontalScrollOffset", typeof(double), typeof(DataGrid), new FrameworkPropertyMetadata(0.0, OnNotifyHorizontalOffsetPropertyChanged));
		BeginEditCommand = new RoutedCommand("BeginEdit", typeof(DataGrid));
		CommitEditCommand = new RoutedCommand("CommitEdit", typeof(DataGrid));
		CancelEditCommand = new RoutedCommand("CancelEdit", typeof(DataGrid));
		IsReadOnlyProperty = DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(false, OnIsReadOnlyChanged));
		CurrentItemProperty = DependencyProperty.Register("CurrentItem", typeof(object), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnCurrentItemChanged));
		CurrentColumnProperty = DependencyProperty.Register("CurrentColumn", typeof(DataGridColumn), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnCurrentColumnChanged));
		CurrentCellProperty = DependencyProperty.Register("CurrentCell", typeof(DataGridCellInfo), typeof(DataGrid), new FrameworkPropertyMetadata(DataGridCellInfo.Unset, OnCurrentCellChanged));
		CanUserAddRowsProperty = DependencyProperty.Register("CanUserAddRows", typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(true, OnCanUserAddRowsChanged, OnCoerceCanUserAddRows));
		CanUserDeleteRowsProperty = DependencyProperty.Register("CanUserDeleteRows", typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(true, OnCanUserDeleteRowsChanged, OnCoerceCanUserDeleteRows));
		RowDetailsVisibilityModeProperty = DependencyProperty.Register("RowDetailsVisibilityMode", typeof(DataGridRowDetailsVisibilityMode), typeof(DataGrid), new FrameworkPropertyMetadata(DataGridRowDetailsVisibilityMode.VisibleWhenSelected, OnNotifyRowAndDetailsPropertyChanged));
		AreRowDetailsFrozenProperty = DependencyProperty.Register("AreRowDetailsFrozen", typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(false));
		RowDetailsTemplateProperty = DependencyProperty.Register("RowDetailsTemplate", typeof(DataTemplate), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnNotifyRowAndDetailsPropertyChanged));
		RowDetailsTemplateSelectorProperty = DependencyProperty.Register("RowDetailsTemplateSelector", typeof(DataTemplateSelector), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnNotifyRowAndDetailsPropertyChanged));
		CanUserResizeRowsProperty = DependencyProperty.Register("CanUserResizeRows", typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(true, OnNotifyRowHeaderPropertyChanged));
		NewItemMarginPropertyKey = DependencyProperty.RegisterReadOnly("NewItemMargin", typeof(Thickness), typeof(DataGrid), new FrameworkPropertyMetadata(new Thickness(0.0)));
		NewItemMarginProperty = NewItemMarginPropertyKey.DependencyProperty;
		SelectionModeProperty = DependencyProperty.Register("SelectionMode", typeof(DataGridSelectionMode), typeof(DataGrid), new FrameworkPropertyMetadata(DataGridSelectionMode.Extended, OnSelectionModeChanged));
		SelectionUnitProperty = DependencyProperty.Register("SelectionUnit", typeof(DataGridSelectionUnit), typeof(DataGrid), new FrameworkPropertyMetadata(DataGridSelectionUnit.FullRow, OnSelectionUnitChanged));
		CanUserSortColumnsProperty = DependencyProperty.Register("CanUserSortColumns", typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(true, OnCanUserSortColumnsPropertyChanged, OnCoerceCanUserSortColumns));
		AutoGenerateColumnsProperty = DependencyProperty.Register("AutoGenerateColumns", typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(true, OnAutoGenerateColumnsPropertyChanged));
		FrozenColumnCountProperty = DependencyProperty.Register("FrozenColumnCount", typeof(int), typeof(DataGrid), new FrameworkPropertyMetadata(0, OnFrozenColumnCountPropertyChanged, OnCoerceFrozenColumnCount), ValidateFrozenColumnCount);
		NonFrozenColumnsViewportHorizontalOffsetPropertyKey = DependencyProperty.RegisterReadOnly("NonFrozenColumnsViewportHorizontalOffset", typeof(double), typeof(DataGrid), new FrameworkPropertyMetadata(0.0));
		NonFrozenColumnsViewportHorizontalOffsetProperty = NonFrozenColumnsViewportHorizontalOffsetPropertyKey.DependencyProperty;
		EnableRowVirtualizationProperty = DependencyProperty.Register("EnableRowVirtualization", typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(true, OnEnableRowVirtualizationChanged));
		EnableColumnVirtualizationProperty = DependencyProperty.Register("EnableColumnVirtualization", typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(false, OnEnableColumnVirtualizationChanged));
		CanUserReorderColumnsProperty = DependencyProperty.Register("CanUserReorderColumns", typeof(bool), typeof(DataGrid), new FrameworkPropertyMetadata(true, OnNotifyColumnPropertyChanged));
		DragIndicatorStyleProperty = DependencyProperty.Register("DragIndicatorStyle", typeof(Style), typeof(DataGrid), new FrameworkPropertyMetadata(null, OnNotifyColumnPropertyChanged));
		DropLocationIndicatorStyleProperty = DependencyProperty.Register("DropLocationIndicatorStyle", typeof(Style), typeof(DataGrid), new FrameworkPropertyMetadata(null));
		ClipboardCopyModeProperty = DependencyProperty.Register("ClipboardCopyMode", typeof(DataGridClipboardCopyMode), typeof(DataGrid), new FrameworkPropertyMetadata(DataGridClipboardCopyMode.ExcludeHeader, OnClipboardCopyModeChanged));
		CellsPanelActualWidthProperty = DependencyProperty.Register("CellsPanelActualWidth", typeof(double), typeof(DataGrid), new FrameworkPropertyMetadata(0.0, CellsPanelActualWidthChanged));
		CellsPanelHorizontalOffsetPropertyKey = DependencyProperty.RegisterReadOnly("CellsPanelHorizontalOffset", typeof(double), typeof(DataGrid), new FrameworkPropertyMetadata(0.0, OnNotifyHorizontalOffsetPropertyChanged));
		CellsPanelHorizontalOffsetProperty = CellsPanelHorizontalOffsetPropertyKey.DependencyProperty;
		_newItemPlaceholder = new NamedObject("DataGrid.NewItemPlaceholder");
		Type typeFromHandle = typeof(DataGrid);
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(typeof(DataGrid)));
		FrameworkElementFactory frameworkElementFactory = new FrameworkElementFactory(typeof(DataGridRowsPresenter));
		frameworkElementFactory.SetValue(FrameworkElement.NameProperty, "PART_RowsPresenter");
		ItemsControl.ItemsPanelProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(new ItemsPanelTemplate(frameworkElementFactory)));
		VirtualizingPanel.IsVirtualizingProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(true, null, OnCoerceIsVirtualizingProperty));
		VirtualizingPanel.VirtualizationModeProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(VirtualizationMode.Recycling));
		ItemsControl.ItemContainerStyleProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(null, OnCoerceItemContainerStyle));
		ItemsControl.ItemContainerStyleSelectorProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(null, OnCoerceItemContainerStyleSelector));
		ItemsControl.ItemsSourceProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(null, OnCoerceItemsSourceProperty));
		ItemsControl.AlternationCountProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(0, null, OnCoerceAlternationCount));
		UIElement.IsEnabledProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(OnIsEnabledChanged));
		UIElement.IsKeyboardFocusWithinPropertyKey.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(OnIsKeyboardFocusWithinChanged));
		Selector.IsSynchronizedWithCurrentItemProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(null, OnCoerceIsSynchronizedWithCurrentItem));
		Control.IsTabStopProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(false));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(KeyboardNavigationMode.Contained));
		KeyboardNavigation.ControlTabNavigationProperty.OverrideMetadata(typeFromHandle, new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
		CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(BeginEditCommand, new KeyGesture(Key.F2)));
		CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(BeginEditCommand, OnExecutedBeginEdit, OnCanExecuteBeginEdit));
		CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(CommitEditCommand, OnExecutedCommitEdit, OnCanExecuteCommitEdit));
		CommandManager.RegisterClassInputBinding(typeFromHandle, new InputBinding(CancelEditCommand, new KeyGesture(Key.Escape)));
		CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(CancelEditCommand, OnExecutedCancelEdit, OnCanExecuteCancelEdit));
		CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(SelectAllCommand, OnExecutedSelectAll, OnCanExecuteSelectAll));
		CommandManager.RegisterClassCommandBinding(typeFromHandle, new CommandBinding(DeleteCommand, OnExecutedDelete, OnCanExecuteDelete));
		CommandManager.RegisterClassCommandBinding(typeof(DataGrid), new CommandBinding(ApplicationCommands.Copy, OnExecutedCopy, OnCanExecuteCopy));
		EventManager.RegisterClassHandler(typeof(DataGrid), UIElement.MouseUpEvent, new MouseButtonEventHandler(OnAnyMouseUpThunk), handledEventsToo: true);
		ControlsTraceLogger.AddControl(TelemetryControls.DataGrid);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGrid" /> class. </summary>
	public DataGrid()
	{
		_columns = new DataGridColumnCollection(this);
		_columns.CollectionChanged += OnColumnsChanged;
		_rowValidationRules = new ObservableCollection<ValidationRule>();
		_rowValidationRules.CollectionChanged += OnRowValidationRulesChanged;
		_selectedCells = new SelectedCellsCollection(this);
		((INotifyCollectionChanged)base.Items).CollectionChanged += OnItemsCollectionChanged;
		((INotifyCollectionChanged)base.Items.SortDescriptions).CollectionChanged += OnItemsSortDescriptionsChanged;
		base.Items.GroupDescriptions.CollectionChanged += OnItemsGroupDescriptionsChanged;
		InternalColumns.InvalidateColumnWidthsComputation();
		CellsPanelHorizontalOffsetComputationPending = false;
	}

	private static void OnColumnSizeConstraintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.Columns);
	}

	private static bool ValidateMinColumnWidth(object v)
	{
		double num = (double)v;
		if (!(num < 0.0) && !double.IsNaN(num))
		{
			return !double.IsPositiveInfinity(num);
		}
		return false;
	}

	private static bool ValidateMaxColumnWidth(object v)
	{
		double num = (double)v;
		if (!(num < 0.0))
		{
			return !double.IsNaN(num);
		}
		return false;
	}

	private void OnColumnsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			UpdateDataGridReference(e.NewItems, clear: false);
			UpdateColumnSizeConstraints(e.NewItems);
			break;
		case NotifyCollectionChangedAction.Remove:
			UpdateDataGridReference(e.OldItems, clear: true);
			break;
		case NotifyCollectionChangedAction.Replace:
			UpdateDataGridReference(e.OldItems, clear: true);
			UpdateDataGridReference(e.NewItems, clear: false);
			UpdateColumnSizeConstraints(e.NewItems);
			break;
		case NotifyCollectionChangedAction.Reset:
			_selectedCells.Clear();
			break;
		}
		if (InternalColumns.DisplayIndexMapInitialized)
		{
			CoerceValue(FrozenColumnCountProperty);
		}
		bool num = HasVisibleColumns(e.OldItems) | HasVisibleColumns(e.NewItems) | (e.Action == NotifyCollectionChangedAction.Reset);
		if (num)
		{
			InternalColumns.InvalidateColumnRealization(invalidateForNonVirtualizedRows: true);
		}
		UpdateColumnsOnRows(e);
		if (num && e.Action != NotifyCollectionChangedAction.Move)
		{
			InternalColumns.InvalidateColumnWidthsComputation();
		}
	}

	internal void UpdateDataGridReference(IList list, bool clear)
	{
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			DataGridColumn dataGridColumn = (DataGridColumn)list[i];
			if (clear)
			{
				if (dataGridColumn.DataGridOwner == this)
				{
					dataGridColumn.DataGridOwner = null;
				}
				continue;
			}
			if (dataGridColumn.DataGridOwner != null && dataGridColumn.DataGridOwner != this)
			{
				dataGridColumn.DataGridOwner.Columns.Remove(dataGridColumn);
			}
			dataGridColumn.DataGridOwner = this;
		}
	}

	private static void UpdateColumnSizeConstraints(IList list)
	{
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			((DataGridColumn)list[i]).SyncProperties();
		}
	}

	private static bool HasVisibleColumns(IList columns)
	{
		if (columns != null && columns.Count > 0)
		{
			foreach (DataGridColumn column in columns)
			{
				if (column.IsVisible)
				{
					return true;
				}
			}
		}
		return false;
	}

	internal bool RetryBringColumnIntoView(bool retryRequested)
	{
		if (retryRequested)
		{
			int value = BringColumnIntoViewRetryCountField.GetValue(this);
			if (value < 4)
			{
				BringColumnIntoViewRetryCountField.SetValue(this, value + 1);
				return true;
			}
		}
		BringColumnIntoViewRetryCountField.ClearValue(this);
		return false;
	}

	/// <summary>Gets the <see cref="T:System.Windows.Controls.DataGridColumn" /> at the specified index.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.DataGridColumn" /> at the specified <see cref="P:System.Windows.Controls.DataGridColumn.DisplayIndex" />.</returns>
	/// <param name="displayIndex">The zero-based index of the column to retrieve. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="displayIndex" /> is less than 0, or greater than or equal to the number of columns.</exception>
	public DataGridColumn ColumnFromDisplayIndex(int displayIndex)
	{
		if (displayIndex < 0 || displayIndex >= Columns.Count)
		{
			throw new ArgumentOutOfRangeException("displayIndex", displayIndex, SR.DataGrid_DisplayIndexOutOfRange);
		}
		return InternalColumns.ColumnFromDisplayIndex(displayIndex);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.ColumnDisplayIndexChanged" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected internal virtual void OnColumnDisplayIndexChanged(DataGridColumnEventArgs e)
	{
		if (this.ColumnDisplayIndexChanged != null)
		{
			this.ColumnDisplayIndexChanged(this, e);
		}
	}

	internal void ValidateDisplayIndex(DataGridColumn column, int displayIndex)
	{
		InternalColumns.ValidateDisplayIndex(column, displayIndex);
	}

	internal int ColumnIndexFromDisplayIndex(int displayIndex)
	{
		if (displayIndex >= 0 && displayIndex < DisplayIndexMap.Count)
		{
			return DisplayIndexMap[displayIndex];
		}
		return -1;
	}

	internal DataGridColumnHeader ColumnHeaderFromDisplayIndex(int displayIndex)
	{
		int num = ColumnIndexFromDisplayIndex(displayIndex);
		if (num != -1 && ColumnHeadersPresenter != null && ColumnHeadersPresenter.ItemContainerGenerator != null)
		{
			return (DataGridColumnHeader)ColumnHeadersPresenter.ItemContainerGenerator.ContainerFromIndex(num);
		}
		return null;
	}

	private static void OnNotifyCellsPresenterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.CellsPresenter);
	}

	private static void OnNotifyColumnAndCellPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.Cells | DataGridNotificationTarget.Columns);
	}

	private static void OnNotifyColumnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.Columns);
	}

	private static void OnNotifyColumnAndColumnHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.Columns | DataGridNotificationTarget.ColumnHeaders);
	}

	private static void OnNotifyColumnHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.ColumnHeaders);
	}

	private static void OnNotifyHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.ColumnHeaders | DataGridNotificationTarget.RowHeaders);
	}

	private static void OnNotifyDataGridAndRowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.DataGrid | DataGridNotificationTarget.Rows);
	}

	private static void OnNotifyGridLinePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.OldValue != e.NewValue)
		{
			((DataGrid)d).OnItemTemplateChanged(null, null);
		}
	}

	private static void OnNotifyRowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.Rows);
	}

	private static void OnNotifyRowHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.RowHeaders);
	}

	private static void OnNotifyRowAndRowHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.RowHeaders | DataGridNotificationTarget.Rows);
	}

	private static void OnNotifyRowAndDetailsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.DetailsPresenter | DataGridNotificationTarget.Rows);
	}

	private static void OnNotifyHorizontalOffsetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.CellsPresenter | DataGridNotificationTarget.ColumnCollection | DataGridNotificationTarget.ColumnHeadersPresenter);
	}

	internal void NotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e, DataGridNotificationTarget target)
	{
		NotifyPropertyChanged(d, string.Empty, e, target);
	}

	internal void NotifyPropertyChanged(DependencyObject d, string propertyName, DependencyPropertyChangedEventArgs e, DataGridNotificationTarget target)
	{
		if (DataGridHelper.ShouldNotifyDataGrid(target))
		{
			if (e.Property == AlternatingRowBackgroundProperty)
			{
				CoerceValue(ItemsControl.AlternationCountProperty);
			}
			else if (e.Property == DataGridColumn.VisibilityProperty || e.Property == DataGridColumn.WidthProperty || e.Property == DataGridColumn.DisplayIndexProperty)
			{
				foreach (DependencyObject recyclableContainer in base.ItemContainerGenerator.RecyclableContainers)
				{
					if (recyclableContainer is DataGridRow { CellsPresenter: { } cellsPresenter })
					{
						cellsPresenter.InvalidateDataGridCellsPanelMeasureAndArrange();
					}
				}
			}
		}
		if (DataGridHelper.ShouldNotifyRowSubtree(target))
		{
			for (ContainerTracking<DataGridRow> containerTracking = _rowTrackingRoot; containerTracking != null; containerTracking = containerTracking.Next)
			{
				containerTracking.Container.NotifyPropertyChanged(d, propertyName, e, target);
			}
		}
		if (DataGridHelper.ShouldNotifyColumnCollection(target) || DataGridHelper.ShouldNotifyColumns(target))
		{
			InternalColumns.NotifyPropertyChanged(d, propertyName, e, target);
		}
		if ((DataGridHelper.ShouldNotifyColumnHeadersPresenter(target) || DataGridHelper.ShouldNotifyColumnHeaders(target)) && ColumnHeadersPresenter != null)
		{
			ColumnHeadersPresenter.NotifyPropertyChanged(d, propertyName, e, target);
		}
	}

	internal void UpdateColumnsOnVirtualizedCellInfoCollections(NotifyCollectionChangedAction action, int oldDisplayIndex, DataGridColumn oldColumn, int newDisplayIndex)
	{
		using (UpdateSelectedCells())
		{
			_selectedCells.OnColumnsChanged(action, oldDisplayIndex, oldColumn, newDisplayIndex, base.SelectedItems);
		}
	}

	/// <summary>Called whenever the template of the <see cref="T:System.Windows.Controls.DataGrid" /> changes.</summary>
	/// <param name="oldTemplate">The old template.</param>
	/// <param name="newTemplate">The new template.</param>
	protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
	{
		base.OnTemplateChanged(oldTemplate, newTemplate);
		ColumnHeadersPresenter = null;
	}

	/// <summary>Determines if an item is a <see cref="T:System.Windows.Controls.DataGridRow" />.</summary>
	/// <returns>true if the item is a <see cref="T:System.Windows.Controls.DataGridRow" />; otherwise, false.</returns>
	/// <param name="item">The item to test.</param>
	protected override bool IsItemItsOwnContainerOverride(object item)
	{
		return item is DataGridRow;
	}

	/// <summary>Instantiates a new <see cref="T:System.Windows.Controls.DataGridRow" />.</summary>
	/// <returns>The row that is the container.</returns>
	protected override DependencyObject GetContainerForItemOverride()
	{
		return new DataGridRow();
	}

	/// <summary>Prepares a new row for the specified item.</summary>
	/// <param name="element">The new <see cref="T:System.Windows.Controls.DataGridRow" />.</param>
	/// <param name="item">The data item that the row contains.</param>
	protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
	{
		base.PrepareContainerForItemOverride(element, item);
		DataGridRow dataGridRow = (DataGridRow)element;
		if (dataGridRow.DataGridOwner != this)
		{
			dataGridRow.Tracker.StartTracking(ref _rowTrackingRoot);
			if (item == CollectionView.NewItemPlaceholder || (IsAddingNewItem && item == EditableItems.CurrentAddItem))
			{
				dataGridRow.IsNewItem = true;
			}
			else
			{
				dataGridRow.ClearValue(DataGridRow.IsNewItemPropertyKey);
			}
			EnsureInternalScrollControls();
			EnqueueNewItemMarginComputation();
		}
		dataGridRow.PrepareRow(item, this);
		OnLoadingRow(new DataGridRowEventArgs(dataGridRow));
	}

	/// <summary>Unloads the row for the specified item.</summary>
	/// <param name="element">The <see cref="T:System.Windows.Controls.DataGridRow" /> to clear.</param>
	/// <param name="item">The data item that the row contains.</param>
	protected override void ClearContainerForItemOverride(DependencyObject element, object item)
	{
		base.ClearContainerForItemOverride(element, item);
		DataGridRow dataGridRow = (DataGridRow)element;
		if (dataGridRow.DataGridOwner == this)
		{
			dataGridRow.Tracker.StopTracking(ref _rowTrackingRoot);
			dataGridRow.ClearValue(DataGridRow.IsNewItemPropertyKey);
			EnqueueNewItemMarginComputation();
		}
		OnUnloadingRow(new DataGridRowEventArgs(dataGridRow));
		dataGridRow.ClearRow(this);
	}

	private void UpdateColumnsOnRows(NotifyCollectionChangedEventArgs e)
	{
		for (ContainerTracking<DataGridRow> containerTracking = _rowTrackingRoot; containerTracking != null; containerTracking = containerTracking.Next)
		{
			containerTracking.Container.OnColumnsChanged(_columns, e);
		}
	}

	private static void OnRowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		d.CoerceValue(ItemsControl.ItemContainerStyleProperty);
	}

	private static object OnCoerceItemContainerStyle(DependencyObject d, object baseValue)
	{
		if (!DataGridHelper.IsDefaultValue(d, RowStyleProperty))
		{
			return d.GetValue(RowStyleProperty);
		}
		return baseValue;
	}

	private void OnRowValidationRulesChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		EnsureItemBindingGroup();
		if (_defaultBindingGroup == null)
		{
			return;
		}
		if (base.ItemBindingGroup == _defaultBindingGroup)
		{
			switch (e.Action)
			{
			case NotifyCollectionChangedAction.Add:
			{
				foreach (ValidationRule newItem in e.NewItems)
				{
					_defaultBindingGroup.ValidationRules.Add(newItem);
				}
				break;
			}
			case NotifyCollectionChangedAction.Remove:
			{
				foreach (ValidationRule oldItem in e.OldItems)
				{
					_defaultBindingGroup.ValidationRules.Remove(oldItem);
				}
				break;
			}
			case NotifyCollectionChangedAction.Replace:
				foreach (ValidationRule oldItem2 in e.OldItems)
				{
					_defaultBindingGroup.ValidationRules.Remove(oldItem2);
				}
				{
					foreach (ValidationRule newItem2 in e.NewItems)
					{
						_defaultBindingGroup.ValidationRules.Add(newItem2);
					}
					break;
				}
			case NotifyCollectionChangedAction.Reset:
				_defaultBindingGroup.ValidationRules.Clear();
				break;
			case NotifyCollectionChangedAction.Move:
				break;
			}
		}
		else
		{
			_defaultBindingGroup = null;
		}
	}

	private static void OnRowStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		d.CoerceValue(ItemsControl.ItemContainerStyleSelectorProperty);
	}

	private static object OnCoerceItemContainerStyleSelector(DependencyObject d, object baseValue)
	{
		if (!DataGridHelper.IsDefaultValue(d, RowStyleSelectorProperty))
		{
			return d.GetValue(RowStyleSelectorProperty);
		}
		return baseValue;
	}

	private static object OnCoerceIsSynchronizedWithCurrentItem(DependencyObject d, object baseValue)
	{
		if (((DataGrid)d).SelectionUnit == DataGridSelectionUnit.Cell)
		{
			return false;
		}
		return baseValue;
	}

	private static object OnCoerceAlternationCount(DependencyObject d, object baseValue)
	{
		if ((int)baseValue < 2 && ((DataGrid)d).AlternatingRowBackground != null)
		{
			return 2;
		}
		return baseValue;
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.LoadingRow" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnLoadingRow(DataGridRowEventArgs e)
	{
		if (this.LoadingRow != null)
		{
			this.LoadingRow(this, e);
		}
		DataGridRow row = e.Row;
		if (row.DetailsVisibility == Visibility.Visible && row.DetailsPresenter != null)
		{
			Dispatcher.CurrentDispatcher.BeginInvoke(new DispatcherOperationCallback(DelayedOnLoadingRowDetails), DispatcherPriority.Loaded, row);
		}
	}

	internal static object DelayedOnLoadingRowDetails(object arg)
	{
		DataGridRow dataGridRow = (DataGridRow)arg;
		dataGridRow.DataGridOwner?.OnLoadingRowDetailsWrapper(dataGridRow);
		return null;
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.UnloadingRow" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnUnloadingRow(DataGridRowEventArgs e)
	{
		if (this.UnloadingRow != null)
		{
			this.UnloadingRow(this, e);
		}
		DataGridRow row = e.Row;
		OnUnloadingRowDetailsWrapper(row);
	}

	private static void OnNotifyRowHeaderWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGrid dataGrid = (DataGrid)d;
		double num = (double)e.NewValue;
		if (!double.IsNaN(num))
		{
			dataGrid.RowHeaderActualWidth = num;
		}
		else
		{
			dataGrid.RowHeaderActualWidth = 0.0;
		}
		OnNotifyRowHeaderPropertyChanged(d, e);
	}

	private void ResetRowHeaderActualWidth()
	{
		if (double.IsNaN(RowHeaderWidth))
		{
			RowHeaderActualWidth = 0.0;
		}
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.DataGridRow.DetailsVisibility" /> property for the <see cref="T:System.Windows.Controls.DataGridRow" /> that contains the specified object.</summary>
	/// <param name="item">The object in the row for which <see cref="P:System.Windows.Controls.DataGridRow.DetailsVisibility" /> is being set.</param>
	/// <param name="detailsVisibility">The <see cref="T:System.Windows.Visibility" /> to set for the row that contains the item.</param>
	public void SetDetailsVisibilityForItem(object item, Visibility detailsVisibility)
	{
		_itemAttachedStorage.SetValue(item, DataGridRow.DetailsVisibilityProperty, detailsVisibility);
		DataGridRow dataGridRow = (DataGridRow)base.ItemContainerGenerator.ContainerFromItem(item);
		if (dataGridRow != null)
		{
			dataGridRow.DetailsVisibility = detailsVisibility;
		}
	}

	/// <summary>Gets the <see cref="P:System.Windows.Controls.DataGridRow.DetailsVisibility" /> property for the <see cref="T:System.Windows.Controls.DataGridRow" /> that represents the specified data item.</summary>
	/// <returns>The visibility for the row that contains the <paramref name="item" />.</returns>
	/// <param name="item">The data item in the row for which <see cref="P:System.Windows.Controls.DataGridRow.DetailsVisibility" /> is returned.</param>
	public Visibility GetDetailsVisibilityForItem(object item)
	{
		if (_itemAttachedStorage.TryGetValue(item, DataGridRow.DetailsVisibilityProperty, out var value))
		{
			return (Visibility)value;
		}
		DataGridRow dataGridRow = (DataGridRow)base.ItemContainerGenerator.ContainerFromItem(item);
		if (dataGridRow != null)
		{
			return dataGridRow.DetailsVisibility;
		}
		switch (RowDetailsVisibilityMode)
		{
		case DataGridRowDetailsVisibilityMode.VisibleWhenSelected:
			if (!base.SelectedItems.Contains(item))
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		case DataGridRowDetailsVisibilityMode.Visible:
			return Visibility.Visible;
		default:
			return Visibility.Collapsed;
		}
	}

	/// <summary>Clears the <see cref="P:System.Windows.Controls.DataGridRow.DetailsVisibility" /> property for the <see cref="T:System.Windows.Controls.DataGridRow" /> that represents the specified data item.</summary>
	/// <param name="item">The data item in the row for which <see cref="P:System.Windows.Controls.DataGridRow.DetailsVisibility" /> is cleared.</param>
	public void ClearDetailsVisibilityForItem(object item)
	{
		_itemAttachedStorage.ClearValue(item, DataGridRow.DetailsVisibilityProperty);
		((DataGridRow)base.ItemContainerGenerator.ContainerFromItem(item))?.ClearValue(DataGridRow.DetailsVisibilityProperty);
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.DataGrid" /> vertically to display the row for the specified data item.</summary>
	/// <param name="item">The data item to bring into view.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="item" /> is null.</exception>
	public void ScrollIntoView(object item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		ScrollIntoView(NewItemInfo(item));
	}

	internal void ScrollIntoView(ItemInfo info)
	{
		if (base.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
		{
			OnBringItemIntoView(info);
		}
		else
		{
			base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(base.OnBringItemIntoView), info);
		}
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.DataGrid" /> vertically and horizontally to display a cell for the specified data item and column. </summary>
	/// <param name="item">The data item to bring into view.</param>
	/// <param name="column">The column to bring into view.</param>
	public void ScrollIntoView(object item, DataGridColumn column)
	{
		ItemInfo info = ((item == null) ? null : NewItemInfo(item));
		ScrollIntoView(info, column);
	}

	private void ScrollIntoView(ItemInfo info, DataGridColumn column)
	{
		if (column == null)
		{
			ScrollIntoView(info);
		}
		else
		{
			if (!column.IsVisible)
			{
				return;
			}
			if (base.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
			{
				if (info == null)
				{
					ScrollColumnIntoView(column);
				}
				else
				{
					ScrollCellIntoView(info, column);
				}
			}
			else
			{
				base.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(OnScrollIntoView), new object[2] { info, column });
			}
		}
	}

	private object OnScrollIntoView(object arg)
	{
		if (arg is object[] array)
		{
			if (array[0] != null)
			{
				ScrollCellIntoView((ItemInfo)array[0], (DataGridColumn)array[1]);
			}
			else
			{
				ScrollColumnIntoView((DataGridColumn)array[1]);
			}
		}
		else
		{
			OnBringItemIntoView((ItemInfo)arg);
		}
		return null;
	}

	private void ScrollColumnIntoView(DataGridColumn column)
	{
		if (_rowTrackingRoot != null)
		{
			DataGridRow container = _rowTrackingRoot.Container;
			if (container != null)
			{
				int index = _columns.IndexOf(column);
				container.ScrollCellIntoView(index);
			}
		}
	}

	private void ScrollCellIntoView(ItemInfo info, DataGridColumn column)
	{
		if (column.IsVisible)
		{
			DataGridRow dataGridRow = ContainerFromItemInfo(info) as DataGridRow;
			if (dataGridRow == null)
			{
				OnBringItemIntoView(info);
				UpdateLayout();
				dataGridRow = ContainerFromItemInfo(info) as DataGridRow;
			}
			else
			{
				dataGridRow.BringIntoView();
				UpdateLayout();
			}
			if (dataGridRow != null)
			{
				int index = _columns.IndexOf(column);
				dataGridRow.ScrollCellIntoView(index);
			}
		}
	}

	/// <summary>Called when the <see cref="P:System.Windows.UIElement.IsMouseCaptured" /> property changes on this element.</summary>
	/// <param name="e">The data for the event.</param>
	protected override void OnIsMouseCapturedChanged(DependencyPropertyChangedEventArgs e)
	{
		if (!base.IsMouseCaptured)
		{
			StopAutoScroll();
		}
		base.OnIsMouseCapturedChanged(e);
	}

	private void StartAutoScroll()
	{
		if (_autoScrollTimer == null)
		{
			_hasAutoScrolled = false;
			_autoScrollTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
			_autoScrollTimer.Interval = ItemsControl.AutoScrollTimeout;
			_autoScrollTimer.Tick += OnAutoScrollTimeout;
			_autoScrollTimer.Start();
		}
	}

	private void StopAutoScroll()
	{
		if (_autoScrollTimer != null)
		{
			_autoScrollTimer.Stop();
			_autoScrollTimer = null;
			_hasAutoScrolled = false;
		}
	}

	private void OnAutoScrollTimeout(object sender, EventArgs e)
	{
		if (Mouse.LeftButton == MouseButtonState.Pressed)
		{
			DoAutoScroll();
		}
		else
		{
			StopAutoScroll();
		}
	}

	private new bool DoAutoScroll()
	{
		RelativeMousePositions relativeMousePosition = RelativeMousePosition;
		if (relativeMousePosition != 0)
		{
			DataGridCell cellNearMouse = GetCellNearMouse();
			if (cellNearMouse != null)
			{
				DataGridColumn dataGridColumn = cellNearMouse.Column;
				ItemInfo itemInfo = ItemInfoFromContainer(cellNearMouse.RowOwner);
				if (IsMouseToLeft(relativeMousePosition))
				{
					int displayIndex = dataGridColumn.DisplayIndex;
					if (displayIndex > 0)
					{
						dataGridColumn = ColumnFromDisplayIndex(displayIndex - 1);
					}
				}
				else if (IsMouseToRight(relativeMousePosition))
				{
					int displayIndex2 = dataGridColumn.DisplayIndex;
					if (displayIndex2 < _columns.Count - 1)
					{
						dataGridColumn = ColumnFromDisplayIndex(displayIndex2 + 1);
					}
				}
				if (IsMouseAbove(relativeMousePosition))
				{
					int index = itemInfo.Index;
					if (index > 0)
					{
						itemInfo = ItemInfoFromIndex(index - 1);
					}
				}
				else if (IsMouseBelow(relativeMousePosition))
				{
					int index2 = itemInfo.Index;
					if (index2 < base.Items.Count - 1)
					{
						itemInfo = ItemInfoFromIndex(index2 + 1);
					}
				}
				if (_isRowDragging)
				{
					OnBringItemIntoView(itemInfo);
					DataGridRow dataGridRow = (DataGridRow)base.ItemContainerGenerator.ContainerFromIndex(itemInfo.Index);
					if (dataGridRow != null)
					{
						_hasAutoScrolled = true;
						HandleSelectionForRowHeaderAndDetailsInput(dataGridRow, startDragging: false);
						SetCurrentItem(itemInfo.Item);
						return true;
					}
				}
				else
				{
					ScrollCellIntoView(itemInfo, dataGridColumn);
					cellNearMouse = TryFindCell(itemInfo, dataGridColumn);
					if (cellNearMouse != null)
					{
						_hasAutoScrolled = true;
						HandleSelectionForCellInput(cellNearMouse, startDragging: false, allowsExtendSelect: true, allowsMinimalSelect: true);
						cellNearMouse.Focus();
						return true;
					}
				}
			}
		}
		return false;
	}

	private void DetermineItemsHostStarBehavior()
	{
		if (_internalItemsHost is VirtualizingStackPanel virtualizingStackPanel)
		{
			virtualizingStackPanel.IgnoreMaxDesiredSize = InternalColumns.HasVisibleStarColumns;
		}
	}

	private void EnsureInternalScrollControls()
	{
		if (_internalScrollContentPresenter == null)
		{
			if (_internalItemsHost != null)
			{
				_internalScrollContentPresenter = DataGridHelper.FindVisualParent<ScrollContentPresenter>(_internalItemsHost);
			}
			else if (_rowTrackingRoot != null)
			{
				DataGridRow container = _rowTrackingRoot.Container;
				_internalScrollContentPresenter = DataGridHelper.FindVisualParent<ScrollContentPresenter>(container);
			}
			if (_internalScrollContentPresenter != null)
			{
				_internalScrollContentPresenter.SizeChanged += OnInternalScrollContentPresenterSizeChanged;
			}
		}
		if (_internalScrollHost == null)
		{
			if (_internalItemsHost != null)
			{
				_internalScrollHost = DataGridHelper.FindVisualParent<ScrollViewer>(_internalItemsHost);
			}
			else if (_rowTrackingRoot != null)
			{
				DataGridRow container2 = _rowTrackingRoot.Container;
				_internalScrollHost = DataGridHelper.FindVisualParent<ScrollViewer>(container2);
			}
			if (_internalScrollHost != null)
			{
				Binding binding = new Binding("ContentHorizontalOffset");
				binding.Source = _internalScrollHost;
				SetBinding(HorizontalScrollOffsetProperty, binding);
			}
		}
	}

	private void CleanUpInternalScrollControls()
	{
		BindingOperations.ClearBinding(this, HorizontalScrollOffsetProperty);
		_internalScrollHost = null;
		if (_internalScrollContentPresenter != null)
		{
			_internalScrollContentPresenter.SizeChanged -= OnInternalScrollContentPresenterSizeChanged;
			_internalScrollContentPresenter = null;
		}
	}

	private void OnInternalScrollContentPresenterSizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (_internalScrollContentPresenter != null && !_internalScrollContentPresenter.CanContentScroll)
		{
			OnViewportSizeChanged(e.PreviousSize, e.NewSize);
		}
	}

	internal void OnViewportSizeChanged(Size oldSize, Size newSize)
	{
		if (!InternalColumns.ColumnWidthsComputationPending && !DoubleUtil.AreClose(newSize.Width - oldSize.Width, 0.0))
		{
			_finalViewportWidth = newSize.Width;
			if (!_viewportWidthChangeNotificationPending)
			{
				_originalViewportWidth = oldSize.Width;
				base.Dispatcher.BeginInvoke(new DispatcherOperationCallback(OnDelayedViewportWidthChanged), DispatcherPriority.Loaded, this);
				_viewportWidthChangeNotificationPending = true;
			}
		}
	}

	private object OnDelayedViewportWidthChanged(object args)
	{
		if (!_viewportWidthChangeNotificationPending)
		{
			return null;
		}
		double num = _finalViewportWidth - _originalViewportWidth;
		if (!DoubleUtil.AreClose(num, 0.0))
		{
			NotifyPropertyChanged(this, "ViewportWidth", default(DependencyPropertyChangedEventArgs), DataGridNotificationTarget.CellsPresenter | DataGridNotificationTarget.ColumnCollection | DataGridNotificationTarget.ColumnHeadersPresenter);
			double finalViewportWidth = _finalViewportWidth;
			finalViewportWidth -= CellsPanelHorizontalOffset;
			InternalColumns.RedistributeColumnWidthsOnAvailableSpaceChange(num, finalViewportWidth);
		}
		_viewportWidthChangeNotificationPending = false;
		return null;
	}

	internal void OnHasVisibleStarColumnsChanged()
	{
		DetermineItemsHostStarBehavior();
	}

	private static void OnCanExecuteBeginEdit(object sender, CanExecuteRoutedEventArgs e)
	{
		((DataGrid)sender).OnCanExecuteBeginEdit(e);
	}

	private static void OnExecutedBeginEdit(object sender, ExecutedRoutedEventArgs e)
	{
		((DataGrid)sender).OnExecutedBeginEdit(e);
	}

	/// <summary>Provides handling for the <see cref="E:System.Windows.Input.CommandBinding.CanExecute" /> event associated with the <see cref="F:System.Windows.Controls.DataGrid.BeginEditCommand" /> command.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnCanExecuteBeginEdit(CanExecuteRoutedEventArgs e)
	{
		bool flag = !IsReadOnly && CurrentCellContainer != null && !IsEditingCurrentCell && !IsCurrentCellReadOnly && !HasCellValidationError;
		if (flag && HasRowValidationError)
		{
			DataGridCell eventCellOrCurrentCell = GetEventCellOrCurrentCell(e);
			if (eventCellOrCurrentCell != null)
			{
				object rowDataItem = eventCellOrCurrentCell.RowDataItem;
				flag = IsAddingOrEditingRowItem(rowDataItem);
			}
			else
			{
				flag = false;
			}
		}
		if (flag)
		{
			e.CanExecute = true;
			e.Handled = true;
		}
		else
		{
			e.ContinueRouting = true;
		}
	}

	/// <summary>Provides handling for the <see cref="E:System.Windows.Input.CommandBinding.Executed" /> event associated with the <see cref="F:System.Windows.Controls.DataGrid.BeginEditCommand" /> command.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnExecutedBeginEdit(ExecutedRoutedEventArgs e)
	{
		DataGridCell currentCellContainer = CurrentCellContainer;
		if (currentCellContainer != null && !currentCellContainer.IsReadOnly && !currentCellContainer.IsEditing)
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			List<int> columnIndexRanges = null;
			int num = -1;
			object obj = null;
			bool flag4 = EditableItems.NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning;
			if (IsNewItemPlaceholder(currentCellContainer.RowDataItem))
			{
				if (base.SelectedItems.Contains(CollectionView.NewItemPlaceholder))
				{
					UnselectItem(NewItemInfo(CollectionView.NewItemPlaceholder));
					flag2 = true;
				}
				else
				{
					num = base.ItemContainerGenerator.IndexFromContainer(currentCellContainer.RowOwner);
					flag3 = num >= 0 && _selectedCells.Intersects(num, out columnIndexRanges);
				}
				obj = AddNewItem();
				SetCurrentCellToNewItem(obj);
				currentCellContainer = CurrentCellContainer;
				if (CurrentCellContainer == null)
				{
					UpdateLayout();
					currentCellContainer = CurrentCellContainer;
					if (currentCellContainer != null && !currentCellContainer.IsKeyboardFocusWithin)
					{
						currentCellContainer.Focus();
					}
				}
				if (flag2)
				{
					SelectItem(NewItemInfo(obj));
				}
				else if (flag3)
				{
					using (UpdateSelectedCells())
					{
						int num2 = num;
						if (flag4)
						{
							_selectedCells.RemoveRegion(num, 0, 1, Columns.Count);
							num2++;
						}
						int i = 0;
						for (int count = columnIndexRanges.Count; i < count; i += 2)
						{
							_selectedCells.AddRegion(num2, columnIndexRanges[i], 1, columnIndexRanges[i + 1]);
						}
					}
				}
				flag = true;
			}
			RoutedEventArgs routedEventArgs = e.Parameter as RoutedEventArgs;
			DataGridBeginningEditEventArgs dataGridBeginningEditEventArgs = null;
			if (currentCellContainer != null)
			{
				dataGridBeginningEditEventArgs = new DataGridBeginningEditEventArgs(currentCellContainer.Column, currentCellContainer.RowOwner, routedEventArgs);
				OnBeginningEdit(dataGridBeginningEditEventArgs);
			}
			if (currentCellContainer == null || dataGridBeginningEditEventArgs.Cancel)
			{
				if (flag2)
				{
					UnselectItem(NewItemInfo(obj));
				}
				else if (flag3 && flag4)
				{
					_selectedCells.RemoveRegion(num + 1, 0, 1, Columns.Count);
				}
				if (flag)
				{
					CancelRowItem();
					UpdateNewItemPlaceholder(isAddingNewItem: false);
					SetCurrentItemToPlaceholder();
				}
				if (flag2)
				{
					SelectItem(NewItemInfo(CollectionView.NewItemPlaceholder));
				}
				else if (flag3)
				{
					int j = 0;
					for (int count2 = columnIndexRanges.Count; j < count2; j += 2)
					{
						_selectedCells.AddRegion(num, columnIndexRanges[j], 1, columnIndexRanges[j + 1]);
					}
				}
			}
			else
			{
				if (!flag && !IsEditingRowItem)
				{
					EditRowItem(currentCellContainer.RowDataItem);
					currentCellContainer.RowOwner.BindingGroup?.BeginEdit();
					_editingRowInfo = ItemInfoFromContainer(currentCellContainer.RowOwner);
				}
				currentCellContainer.BeginEdit(routedEventArgs);
				currentCellContainer.RowOwner.IsEditing = true;
				EnsureCellAutomationValueHolder(currentCellContainer);
			}
		}
		CommandManager.InvalidateRequerySuggested();
		e.Handled = true;
	}

	private static void OnCanExecuteCommitEdit(object sender, CanExecuteRoutedEventArgs e)
	{
		((DataGrid)sender).OnCanExecuteCommitEdit(e);
	}

	private static void OnExecutedCommitEdit(object sender, ExecutedRoutedEventArgs e)
	{
		((DataGrid)sender).OnExecutedCommitEdit(e);
	}

	private DataGridCell GetEventCellOrCurrentCell(RoutedEventArgs e)
	{
		UIElement uIElement = e.OriginalSource as UIElement;
		if (uIElement != this && uIElement != null)
		{
			return DataGridHelper.FindVisualParent<DataGridCell>(uIElement);
		}
		return CurrentCellContainer;
	}

	private bool CanEndEdit(CanExecuteRoutedEventArgs e, bool commit)
	{
		DataGridCell eventCellOrCurrentCell = GetEventCellOrCurrentCell(e);
		if (eventCellOrCurrentCell == null)
		{
			return false;
		}
		DataGridEditingUnit editingUnit = GetEditingUnit(e.Parameter);
		_ = EditableItems;
		object rowDataItem = eventCellOrCurrentCell.RowDataItem;
		if (!eventCellOrCurrentCell.IsEditing)
		{
			if (!HasCellValidationError)
			{
				return IsAddingOrEditingRowItem(editingUnit, rowDataItem);
			}
			return false;
		}
		return true;
	}

	/// <summary>Provides handling for the <see cref="E:System.Windows.Input.CommandBinding.CanExecute" /> event associated with the <see cref="F:System.Windows.Controls.DataGrid.CommitEditCommand" /> command.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnCanExecuteCommitEdit(CanExecuteRoutedEventArgs e)
	{
		if (CanEndEdit(e, commit: true))
		{
			e.CanExecute = true;
			e.Handled = true;
		}
		else
		{
			e.ContinueRouting = true;
		}
	}

	/// <summary>Provides handling for the <see cref="E:System.Windows.Input.CommandBinding.Executed" /> event associated with the <see cref="F:System.Windows.Controls.DataGrid.CommitEditCommand" /> command.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnExecutedCommitEdit(ExecutedRoutedEventArgs e)
	{
		DataGridCell currentCellContainer = CurrentCellContainer;
		bool flag = true;
		if (currentCellContainer != null)
		{
			DataGridEditingUnit editingUnit = GetEditingUnit(e.Parameter);
			bool flag2 = false;
			if (currentCellContainer.IsEditing)
			{
				DataGridCellEditEndingEventArgs dataGridCellEditEndingEventArgs = new DataGridCellEditEndingEventArgs(currentCellContainer.Column, currentCellContainer.RowOwner, currentCellContainer.EditingElement, DataGridEditAction.Commit);
				OnCellEditEnding(dataGridCellEditEndingEventArgs);
				flag2 = dataGridCellEditEndingEventArgs.Cancel;
				if (!flag2)
				{
					flag = currentCellContainer.CommitEdit();
					HasCellValidationError = !flag;
					UpdateCellAutomationValueHolder(currentCellContainer);
				}
			}
			if (flag && !flag2 && IsAddingOrEditingRowItem(editingUnit, currentCellContainer.RowDataItem))
			{
				DataGridRowEditEndingEventArgs dataGridRowEditEndingEventArgs = new DataGridRowEditEndingEventArgs(currentCellContainer.RowOwner, DataGridEditAction.Commit);
				OnRowEditEnding(dataGridRowEditEndingEventArgs);
				if (!dataGridRowEditEndingEventArgs.Cancel)
				{
					BindingGroup bindingGroup = currentCellContainer.RowOwner.BindingGroup;
					if (bindingGroup != null)
					{
						base.Dispatcher.Invoke(new DispatcherOperationCallback(DoNothing), DispatcherPriority.DataBind, bindingGroup);
						flag = bindingGroup.CommitEdit();
					}
					HasRowValidationError = !flag;
					if (flag)
					{
						CommitRowItem();
					}
				}
			}
			if (flag)
			{
				UpdateRowEditing(currentCellContainer);
				if (!currentCellContainer.RowOwner.IsEditing)
				{
					ReleaseCellAutomationValueHolders();
				}
			}
			CommandManager.InvalidateRequerySuggested();
		}
		e.Handled = true;
	}

	private static object DoNothing(object arg)
	{
		return null;
	}

	private DataGridEditingUnit GetEditingUnit(object parameter)
	{
		if (parameter == null || !(parameter is DataGridEditingUnit))
		{
			if (!IsEditingCurrentCell)
			{
				return DataGridEditingUnit.Row;
			}
			return DataGridEditingUnit.Cell;
		}
		return (DataGridEditingUnit)parameter;
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.RowEditEnding" /> event. </summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnRowEditEnding(DataGridRowEditEndingEventArgs e)
	{
		if (this.RowEditEnding != null)
		{
			this.RowEditEnding(this, e);
		}
		if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked) && UIElementAutomationPeer.FromElement(this) is DataGridAutomationPeer dataGridAutomationPeer)
		{
			dataGridAutomationPeer.RaiseAutomationRowInvokeEvents(e.Row);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.CellEditEnding" /> event. </summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnCellEditEnding(DataGridCellEditEndingEventArgs e)
	{
		if (this.CellEditEnding != null)
		{
			this.CellEditEnding(this, e);
		}
		if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked) && UIElementAutomationPeer.FromElement(this) is DataGridAutomationPeer dataGridAutomationPeer)
		{
			dataGridAutomationPeer.RaiseAutomationCellInvokeEvents(e.Column, e.Row);
		}
	}

	private static void OnCanExecuteCancelEdit(object sender, CanExecuteRoutedEventArgs e)
	{
		((DataGrid)sender).OnCanExecuteCancelEdit(e);
	}

	private static void OnExecutedCancelEdit(object sender, ExecutedRoutedEventArgs e)
	{
		((DataGrid)sender).OnExecutedCancelEdit(e);
	}

	/// <summary>Provides handling for the <see cref="E:System.Windows.Input.CommandBinding.CanExecute" /> event associated with the <see cref="F:System.Windows.Controls.DataGrid.CancelEditCommand" /> command.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnCanExecuteCancelEdit(CanExecuteRoutedEventArgs e)
	{
		if (CanEndEdit(e, commit: false))
		{
			e.CanExecute = true;
			e.Handled = true;
		}
		else
		{
			e.ContinueRouting = true;
		}
	}

	/// <summary>Provides handling for the <see cref="E:System.Windows.Input.CommandBinding.Executed" /> event associated with the <see cref="F:System.Windows.Controls.DataGrid.CancelEditCommand" /> command.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnExecutedCancelEdit(ExecutedRoutedEventArgs e)
	{
		DataGridCell currentCellContainer = CurrentCellContainer;
		if (currentCellContainer != null)
		{
			DataGridEditingUnit editingUnit = GetEditingUnit(e.Parameter);
			bool flag = false;
			if (currentCellContainer.IsEditing)
			{
				DataGridCellEditEndingEventArgs dataGridCellEditEndingEventArgs = new DataGridCellEditEndingEventArgs(currentCellContainer.Column, currentCellContainer.RowOwner, currentCellContainer.EditingElement, DataGridEditAction.Cancel);
				OnCellEditEnding(dataGridCellEditEndingEventArgs);
				flag = dataGridCellEditEndingEventArgs.Cancel;
				if (!flag)
				{
					currentCellContainer.CancelEdit();
					HasCellValidationError = false;
					UpdateCellAutomationValueHolder(currentCellContainer);
				}
			}
			if (!flag && IsAddingOrEditingRowItem(editingUnit, currentCellContainer.RowDataItem))
			{
				DataGridRowEditEndingEventArgs dataGridRowEditEndingEventArgs = new DataGridRowEditEndingEventArgs(currentCellContainer.RowOwner, DataGridEditAction.Cancel);
				OnRowEditEnding(dataGridRowEditEndingEventArgs);
				if (!dataGridRowEditEndingEventArgs.Cancel)
				{
					currentCellContainer.RowOwner.BindingGroup?.CancelEdit();
					CancelRowItem();
				}
			}
			UpdateRowEditing(currentCellContainer);
			if (!currentCellContainer.RowOwner.IsEditing)
			{
				HasRowValidationError = false;
				ReleaseCellAutomationValueHolders();
			}
			CommandManager.InvalidateRequerySuggested();
		}
		e.Handled = true;
	}

	private static void OnCanExecuteDelete(object sender, CanExecuteRoutedEventArgs e)
	{
		((DataGrid)sender).OnCanExecuteDelete(e);
	}

	private static void OnExecutedDelete(object sender, ExecutedRoutedEventArgs e)
	{
		((DataGrid)sender).OnExecutedDelete(e);
	}

	/// <summary>Provides handling for the <see cref="E:System.Windows.Input.CommandBinding.CanExecute" /> event associated with the <see cref="P:System.Windows.Controls.DataGrid.DeleteCommand" /> command.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnCanExecuteDelete(CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = CanUserDeleteRows && DataItemsSelected > 0 && (_currentCellContainer == null || !_currentCellContainer.IsEditing);
		e.Handled = true;
	}

	/// <summary>Provides handling for the <see cref="E:System.Windows.Input.CommandBinding.Executed" /> event associated with the <see cref="P:System.Windows.Controls.DataGrid.DeleteCommand" /> command.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnExecutedDelete(ExecutedRoutedEventArgs e)
	{
		if (DataItemsSelected > 0)
		{
			bool flag = false;
			bool isEditingRowItem = IsEditingRowItem;
			if (isEditingRowItem || IsAddingNewItem)
			{
				if (CancelEdit(DataGridEditingUnit.Row) && isEditingRowItem)
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				int count = base.SelectedItems.Count;
				int num = -1;
				ItemInfo currentInfo = CurrentInfo;
				if (base.SelectedItems.Contains(currentInfo.Item))
				{
					num = currentInfo.Index;
					if (_selectionAnchor.HasValue)
					{
						int index = _selectionAnchor.Value.ItemInfo.Index;
						if (index >= 0 && index < num)
						{
							num = index;
						}
					}
					num = Math.Min(base.Items.Count - count - 1, num);
				}
				ArrayList arrayList = new ArrayList(base.SelectedItems);
				using (UpdateSelectedCells())
				{
					bool isUpdatingSelectedItems = base.IsUpdatingSelectedItems;
					if (!isUpdatingSelectedItems)
					{
						BeginUpdateSelectedItems();
					}
					try
					{
						_selectedCells.ClearFullRows(base.SelectedItems);
						base.SelectedItems.Clear();
					}
					finally
					{
						if (!isUpdatingSelectedItems)
						{
							EndUpdateSelectedItems();
						}
					}
				}
				for (int i = 0; i < count; i++)
				{
					object obj = arrayList[i];
					if (obj != CollectionView.NewItemPlaceholder)
					{
						EditableItems.Remove(obj);
					}
				}
				if (num >= 0)
				{
					object currentItem = base.Items[num];
					SetCurrentItem(currentItem);
					DataGridCell currentCellContainer = CurrentCellContainer;
					if (currentCellContainer != null)
					{
						_selectionAnchor = null;
						HandleSelectionForCellInput(currentCellContainer, startDragging: false, allowsExtendSelect: false, allowsMinimalSelect: false);
					}
				}
			}
		}
		e.Handled = true;
	}

	private void SetCurrentCellToNewItem(object newItem)
	{
		ItemInfo itemInfo = null;
		switch (EditableItems.NewItemPlaceholderPosition)
		{
		case NewItemPlaceholderPosition.AtEnd:
		{
			int num = base.Items.Count - 2;
			if (num >= 0 && ItemsControl.EqualsEx(newItem, base.Items[num]))
			{
				itemInfo = ItemInfoFromIndex(num);
			}
			break;
		}
		case NewItemPlaceholderPosition.AtBeginning:
		{
			int num = 1;
			if (num < base.Items.Count && ItemsControl.EqualsEx(newItem, base.Items[num]))
			{
				itemInfo = ItemInfoFromIndex(num);
			}
			break;
		}
		}
		if (itemInfo == null)
		{
			itemInfo = ItemInfoFromIndex(base.Items.IndexOf(newItem));
		}
		DataGridCellInfo currentCell = CurrentCell;
		currentCell = ((itemInfo != null) ? new DataGridCellInfo(itemInfo, currentCell.Column, this) : DataGridCellInfo.CreatePossiblyPartialCellInfo(newItem, currentCell.Column, this));
		SetCurrentValueInternal(CurrentCellProperty, currentCell);
	}

	private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue)
		{
			((DataGrid)d).CancelAnyEdit();
		}
		CommandManager.InvalidateRequerySuggested();
		d.CoerceValue(CanUserAddRowsProperty);
		d.CoerceValue(CanUserDeleteRowsProperty);
		OnNotifyColumnAndCellPropertyChanged(d, e);
	}

	private static void OnCurrentItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGrid dataGrid = (DataGrid)d;
		DataGridCellInfo currentCell = dataGrid.CurrentCell;
		object newValue = e.NewValue;
		if (currentCell.Item != newValue)
		{
			dataGrid.SetCurrentValueInternal(CurrentCellProperty, DataGridCellInfo.CreatePossiblyPartialCellInfo(newValue, currentCell.Column, dataGrid));
		}
		OnNotifyRowHeaderPropertyChanged(d, e);
	}

	private void SetCurrentItem(object item)
	{
		if (item == DependencyProperty.UnsetValue)
		{
			item = null;
		}
		SetCurrentValueInternal(CurrentItemProperty, item);
	}

	private static void OnCurrentColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGrid dataGrid = (DataGrid)d;
		DataGridCellInfo currentCell = dataGrid.CurrentCell;
		DataGridColumn dataGridColumn = (DataGridColumn)e.NewValue;
		if (currentCell.Column != dataGridColumn)
		{
			dataGrid.SetCurrentValueInternal(CurrentCellProperty, DataGridCellInfo.CreatePossiblyPartialCellInfo(currentCell.Item, dataGridColumn, dataGrid));
		}
	}

	private static void OnCurrentCellChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGrid dataGrid = (DataGrid)d;
		DataGridCellInfo dataGridCellInfo = (DataGridCellInfo)e.OldValue;
		DataGridCellInfo dataGridCellInfo2 = (DataGridCellInfo)e.NewValue;
		if (dataGrid.CurrentItem != dataGridCellInfo2.Item)
		{
			dataGrid.SetCurrentItem(dataGridCellInfo2.Item);
		}
		if (dataGrid.CurrentColumn != dataGridCellInfo2.Column)
		{
			dataGrid.SetCurrentValueInternal(CurrentColumnProperty, dataGridCellInfo2.Column);
		}
		if (dataGrid._currentCellContainer != null)
		{
			if ((dataGrid.IsAddingNewItem || dataGrid.IsEditingRowItem) && dataGridCellInfo.Item != dataGridCellInfo2.Item)
			{
				dataGrid.EndEdit(CommitEditCommand, dataGrid._currentCellContainer, DataGridEditingUnit.Row, exitEditMode: true);
			}
			else if (dataGrid._currentCellContainer.IsEditing)
			{
				dataGrid.EndEdit(CommitEditCommand, dataGrid._currentCellContainer, DataGridEditingUnit.Cell, exitEditMode: true);
			}
		}
		DataGridCell currentCellContainer = dataGrid._currentCellContainer;
		dataGrid._currentCellContainer = null;
		if (dataGridCellInfo2.IsValid && dataGrid.IsKeyboardFocusWithin)
		{
			DataGridCell dataGridCell = dataGrid._pendingCurrentCellContainer;
			if (dataGridCell == null)
			{
				dataGridCell = dataGrid.CurrentCellContainer;
				if (dataGridCell == null)
				{
					dataGrid.ScrollCellIntoView(dataGridCellInfo2.ItemInfo, dataGridCellInfo2.Column);
					dataGridCell = dataGrid.CurrentCellContainer;
				}
			}
			if (dataGridCell != null)
			{
				if (!dataGridCell.IsKeyboardFocusWithin)
				{
					dataGridCell.Focus();
				}
				if (currentCellContainer != dataGridCell)
				{
					currentCellContainer?.NotifyCurrentCellContainerChanged();
					dataGridCell.NotifyCurrentCellContainerChanged();
				}
			}
			else
			{
				currentCellContainer?.NotifyCurrentCellContainerChanged();
			}
		}
		dataGrid.OnCurrentCellChanged(EventArgs.Empty);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.CurrentCellChanged" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnCurrentCellChanged(EventArgs e)
	{
		if (this.CurrentCellChanged != null)
		{
			this.CurrentCellChanged(this, e);
		}
	}

	private void UpdateCurrentCell(DataGridCell cell, bool isFocusWithinCell)
	{
		if (isFocusWithinCell)
		{
			CurrentCellContainer = cell;
		}
		else if (!base.IsKeyboardFocusWithin)
		{
			CurrentCellContainer = null;
		}
	}

	internal bool IsCurrent(DataGridRow row, DataGridColumn column = null)
	{
		DataGridCellInfo dataGridCellInfo = CurrentCell;
		if (dataGridCellInfo.ItemInfo == null)
		{
			dataGridCellInfo = DataGridCellInfo.Unset;
		}
		DependencyObject container = dataGridCellInfo.ItemInfo.Container;
		int index = dataGridCellInfo.ItemInfo.Index;
		if (column == null || column == dataGridCellInfo.Column)
		{
			if (container == null || container != row)
			{
				if (ItemsControl.EqualsEx(CurrentItem, row.Item))
				{
					if (index >= 0)
					{
						return index == base.ItemContainerGenerator.IndexFromContainer(row);
					}
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.BeginningEdit" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnBeginningEdit(DataGridBeginningEditEventArgs e)
	{
		if (this.BeginningEdit != null)
		{
			this.BeginningEdit(this, e);
		}
		if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked) && UIElementAutomationPeer.FromElement(this) is DataGridAutomationPeer dataGridAutomationPeer)
		{
			dataGridAutomationPeer.RaiseAutomationCellInvokeEvents(e.Column, e.Row);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.PreparingCellForEdit" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected internal virtual void OnPreparingCellForEdit(DataGridPreparingCellForEditEventArgs e)
	{
		if (this.PreparingCellForEdit != null)
		{
			this.PreparingCellForEdit(this, e);
		}
	}

	/// <summary>Invokes the <see cref="M:System.Windows.Controls.DataGrid.BeginEdit" /> command, which will place the current cell or row into edit mode.</summary>
	/// <returns>true if the current cell or row enters edit mode; otherwise, false.</returns>
	public bool BeginEdit()
	{
		return BeginEdit(null);
	}

	/// <summary>Invokes the <see cref="M:System.Windows.Controls.DataGrid.BeginEdit" /> command, which will place the current cell or row into edit mode.</summary>
	/// <returns>true if the current cell or row enters edit mode; otherwise, false.</returns>
	/// <param name="editingEventArgs">If called from an event handler, the event arguments. May be null.</param>
	public bool BeginEdit(RoutedEventArgs editingEventArgs)
	{
		if (!IsReadOnly)
		{
			DataGridCell currentCellContainer = CurrentCellContainer;
			if (currentCellContainer != null)
			{
				if (!currentCellContainer.IsEditing && BeginEditCommand.CanExecute(editingEventArgs, currentCellContainer))
				{
					BeginEditCommand.Execute(editingEventArgs, currentCellContainer);
					currentCellContainer = CurrentCellContainer;
					if (currentCellContainer == null)
					{
						return false;
					}
				}
				return currentCellContainer.IsEditing;
			}
		}
		return false;
	}

	/// <summary>Invokes the <see cref="F:System.Windows.Controls.DataGrid.CancelEditCommand" /> command for the cell or row currently in edit mode.</summary>
	/// <returns>true if the current cell or row exits edit mode, or if no cells or rows are in edit mode; otherwise, false.</returns>
	public bool CancelEdit()
	{
		if (IsEditingCurrentCell)
		{
			return CancelEdit(DataGridEditingUnit.Cell);
		}
		if (IsEditingRowItem || IsAddingNewItem)
		{
			return CancelEdit(DataGridEditingUnit.Row);
		}
		return true;
	}

	internal bool CancelEdit(DataGridCell cell)
	{
		DataGridCell currentCellContainer = CurrentCellContainer;
		if (currentCellContainer != null && currentCellContainer == cell && currentCellContainer.IsEditing)
		{
			return CancelEdit(DataGridEditingUnit.Cell);
		}
		return true;
	}

	/// <summary>Invokes the <see cref="F:System.Windows.Controls.DataGrid.CancelEditCommand" /> command for the specified cell or row in edit mode. </summary>
	/// <returns>true if the current cell or row exits edit mode; otherwise, false.</returns>
	/// <param name="editingUnit">One of the enumeration values that specifies whether to cancel row or cell edits.</param>
	public bool CancelEdit(DataGridEditingUnit editingUnit)
	{
		return EndEdit(CancelEditCommand, CurrentCellContainer, editingUnit, exitEditMode: true);
	}

	private void CancelAnyEdit()
	{
		if (IsAddingNewItem || IsEditingRowItem)
		{
			CancelEdit(DataGridEditingUnit.Row);
		}
		else if (IsEditingCurrentCell)
		{
			CancelEdit(DataGridEditingUnit.Cell);
		}
	}

	/// <summary>Invokes the <see cref="F:System.Windows.Controls.DataGrid.CommitEditCommand" /> command for the cell or row currently in edit mode. </summary>
	/// <returns>true if the current cell or row exits edit mode, or if no cells or rows are in edit mode; otherwise, false.</returns>
	public bool CommitEdit()
	{
		if (IsEditingCurrentCell)
		{
			return CommitEdit(DataGridEditingUnit.Cell, exitEditingMode: true);
		}
		if (IsEditingRowItem || IsAddingNewItem)
		{
			return CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);
		}
		return true;
	}

	/// <summary>Invokes the <see cref="F:System.Windows.Controls.DataGrid.CommitEditCommand" /> command for the specified cell or row currently in edit mode. </summary>
	/// <returns>true if the current cell or row exits edit mode; otherwise, false.</returns>
	/// <param name="editingUnit">One of the enumeration values that specifies whether to commit row or cell edits.</param>
	/// <param name="exitEditingMode">true to exit edit mode; otherwise, false.</param>
	public bool CommitEdit(DataGridEditingUnit editingUnit, bool exitEditingMode)
	{
		return EndEdit(CommitEditCommand, CurrentCellContainer, editingUnit, exitEditingMode);
	}

	private bool CommitAnyEdit()
	{
		if (IsAddingNewItem || IsEditingRowItem)
		{
			return CommitEdit(DataGridEditingUnit.Row, exitEditingMode: true);
		}
		if (IsEditingCurrentCell)
		{
			return CommitEdit(DataGridEditingUnit.Cell, exitEditingMode: true);
		}
		return true;
	}

	private bool EndEdit(RoutedCommand command, DataGridCell cellContainer, DataGridEditingUnit editingUnit, bool exitEditMode)
	{
		bool flag = true;
		bool flag2 = true;
		if (cellContainer != null)
		{
			if (command.CanExecute(editingUnit, cellContainer))
			{
				command.Execute(editingUnit, cellContainer);
			}
			flag = !cellContainer.IsEditing;
			flag2 = !IsEditingRowItem && !IsAddingNewItem;
		}
		if (!exitEditMode)
		{
			if (editingUnit != 0)
			{
				if (flag2)
				{
					object rowDataItem = cellContainer.RowDataItem;
					if (rowDataItem != null)
					{
						EditRowItem(rowDataItem);
						return IsEditingRowItem;
					}
				}
				return false;
			}
			if (cellContainer == null)
			{
				return false;
			}
			if (flag)
			{
				return BeginEdit(null);
			}
		}
		if (flag)
		{
			return editingUnit == DataGridEditingUnit.Cell || flag2;
		}
		return false;
	}

	private static void OnCanUserAddRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).UpdateNewItemPlaceholder(isAddingNewItem: false);
	}

	private static object OnCoerceCanUserAddRows(DependencyObject d, object baseValue)
	{
		return OnCoerceCanUserAddOrDeleteRows((DataGrid)d, (bool)baseValue, canUserAddRowsProperty: true);
	}

	private static bool OnCoerceCanUserAddOrDeleteRows(DataGrid dataGrid, bool baseValue, bool canUserAddRowsProperty)
	{
		if (baseValue)
		{
			if (dataGrid.IsReadOnly || !dataGrid.IsEnabled)
			{
				return false;
			}
			if ((canUserAddRowsProperty && !dataGrid.EditableItems.CanAddNew) || (!canUserAddRowsProperty && !dataGrid.EditableItems.CanRemove))
			{
				return false;
			}
		}
		return baseValue;
	}

	private static void OnCanUserDeleteRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		CommandManager.InvalidateRequerySuggested();
	}

	private static object OnCoerceCanUserDeleteRows(DependencyObject d, object baseValue)
	{
		return OnCoerceCanUserAddOrDeleteRows((DataGrid)d, (bool)baseValue, canUserAddRowsProperty: false);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.AddingNewItem" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnAddingNewItem(AddingNewItemEventArgs e)
	{
		if (this.AddingNewItem != null)
		{
			this.AddingNewItem(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.InitializingNewItem" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnInitializingNewItem(InitializingNewItemEventArgs e)
	{
		if (this.InitializingNewItem != null)
		{
			this.InitializingNewItem(this, e);
		}
	}

	private object AddNewItem()
	{
		UpdateNewItemPlaceholder(isAddingNewItem: true);
		object obj = null;
		IEditableCollectionViewAddNewItem items = base.Items;
		if (items.CanAddNewItem)
		{
			AddingNewItemEventArgs addingNewItemEventArgs = new AddingNewItemEventArgs();
			OnAddingNewItem(addingNewItemEventArgs);
			obj = addingNewItemEventArgs.NewItem;
		}
		obj = ((obj != null) ? items.AddNewItem(obj) : EditableItems.AddNew());
		if (obj != null)
		{
			OnInitializingNewItem(new InitializingNewItemEventArgs(obj));
		}
		CommandManager.InvalidateRequerySuggested();
		return obj;
	}

	private void EditRowItem(object rowItem)
	{
		EditableItems.EditItem(rowItem);
		CommandManager.InvalidateRequerySuggested();
	}

	private void CommitRowItem()
	{
		if (IsEditingRowItem)
		{
			EditableItems.CommitEdit();
			return;
		}
		EditableItems.CommitNew();
		UpdateNewItemPlaceholder(isAddingNewItem: false);
	}

	private void CancelRowItem()
	{
		if (IsEditingRowItem)
		{
			if (EditableItems.CanCancelEdit)
			{
				EditableItems.CancelEdit();
			}
			else
			{
				EditableItems.CommitEdit();
			}
			return;
		}
		object currentAddItem = EditableItems.CurrentAddItem;
		bool flag = currentAddItem == CurrentItem;
		bool num = base.SelectedItems.Contains(currentAddItem);
		bool flag2 = false;
		List<int> columnIndexRanges = null;
		int num2 = -1;
		if (num)
		{
			UnselectItem(NewItemInfo(currentAddItem));
		}
		else
		{
			num2 = base.Items.IndexOf(currentAddItem);
			flag2 = num2 >= 0 && _selectedCells.Intersects(num2, out columnIndexRanges);
		}
		EditableItems.CancelNew();
		UpdateNewItemPlaceholder(isAddingNewItem: false);
		if (flag)
		{
			SetCurrentItem(CollectionView.NewItemPlaceholder);
		}
		if (num)
		{
			SelectItem(NewItemInfo(CollectionView.NewItemPlaceholder));
		}
		else
		{
			if (!flag2)
			{
				return;
			}
			using (UpdateSelectedCells())
			{
				int num3 = num2;
				if (EditableItems.NewItemPlaceholderPosition == NewItemPlaceholderPosition.AtBeginning)
				{
					_selectedCells.RemoveRegion(num2, 0, 1, Columns.Count);
					num3--;
				}
				int i = 0;
				for (int count = columnIndexRanges.Count; i < count; i += 2)
				{
					_selectedCells.AddRegion(num3, columnIndexRanges[i], 1, columnIndexRanges[i + 1]);
				}
			}
		}
	}

	private void UpdateRowEditing(DataGridCell cell)
	{
		object rowDataItem = cell.RowDataItem;
		if (!IsAddingOrEditingRowItem(rowDataItem))
		{
			cell.RowOwner.IsEditing = false;
			_editingRowInfo = null;
		}
	}

	private bool IsAddingOrEditingRowItem(object item)
	{
		if (!IsEditingItem(item))
		{
			if (IsAddingNewItem)
			{
				return EditableItems.CurrentAddItem == item;
			}
			return false;
		}
		return true;
	}

	private bool IsAddingOrEditingRowItem(DataGridEditingUnit editingUnit, object item)
	{
		if (editingUnit == DataGridEditingUnit.Row)
		{
			return IsAddingOrEditingRowItem(item);
		}
		return false;
	}

	private bool IsEditingItem(object item)
	{
		if (IsEditingRowItem)
		{
			return EditableItems.CurrentEditItem == item;
		}
		return false;
	}

	private void UpdateNewItemPlaceholder(bool isAddingNewItem)
	{
		IEditableCollectionView editableItems = EditableItems;
		bool flag = CanUserAddRows;
		if (DataGridHelper.IsDefaultValue(this, CanUserAddRowsProperty))
		{
			flag = OnCoerceCanUserAddOrDeleteRows(this, flag, canUserAddRowsProperty: true);
		}
		if (!isAddingNewItem)
		{
			if (flag)
			{
				if (editableItems.NewItemPlaceholderPosition == NewItemPlaceholderPosition.None)
				{
					editableItems.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtEnd;
				}
				_placeholderVisibility = Visibility.Visible;
			}
			else
			{
				if (editableItems.NewItemPlaceholderPosition != 0)
				{
					editableItems.NewItemPlaceholderPosition = NewItemPlaceholderPosition.None;
				}
				_placeholderVisibility = Visibility.Collapsed;
			}
		}
		else
		{
			_placeholderVisibility = Visibility.Collapsed;
		}
		((DataGridRow)base.ItemContainerGenerator.ContainerFromItem(CollectionView.NewItemPlaceholder))?.CoerceValue(UIElement.VisibilityProperty);
	}

	private void SetCurrentItemToPlaceholder()
	{
		switch (EditableItems.NewItemPlaceholderPosition)
		{
		case NewItemPlaceholderPosition.AtEnd:
		{
			int count = base.Items.Count;
			if (count > 0)
			{
				SetCurrentItem(base.Items[count - 1]);
			}
			break;
		}
		case NewItemPlaceholderPosition.AtBeginning:
			if (base.Items.Count > 0)
			{
				SetCurrentItem(base.Items[0]);
			}
			break;
		}
	}

	private bool IsNewItemPlaceholder(object item)
	{
		if (item != CollectionView.NewItemPlaceholder)
		{
			return item == NewItemPlaceholder;
		}
		return true;
	}

	internal void OnLoadingRowDetailsWrapper(DataGridRow row)
	{
		if (row != null && !row.DetailsLoaded && row.DetailsVisibility == Visibility.Visible && row.DetailsPresenter != null)
		{
			DataGridRowDetailsEventArgs e = new DataGridRowDetailsEventArgs(row, row.DetailsPresenter.DetailsElement);
			OnLoadingRowDetails(e);
			row.DetailsLoaded = true;
		}
	}

	internal void OnUnloadingRowDetailsWrapper(DataGridRow row)
	{
		if (row != null && row.DetailsLoaded && row.DetailsPresenter != null)
		{
			DataGridRowDetailsEventArgs e = new DataGridRowDetailsEventArgs(row, row.DetailsPresenter.DetailsElement);
			OnUnloadingRowDetails(e);
			row.DetailsLoaded = false;
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.LoadingRowDetails" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnLoadingRowDetails(DataGridRowDetailsEventArgs e)
	{
		if (this.LoadingRowDetails != null)
		{
			this.LoadingRowDetails(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.UnloadingRowDetails" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnUnloadingRowDetails(DataGridRowDetailsEventArgs e)
	{
		if (this.UnloadingRowDetails != null)
		{
			this.UnloadingRowDetails(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.RowDetailsVisibilityChanged" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected internal virtual void OnRowDetailsVisibilityChanged(DataGridRowDetailsEventArgs e)
	{
		if (this.RowDetailsVisibilityChanged != null)
		{
			this.RowDetailsVisibilityChanged(this, e);
		}
		DataGridRow row = e.Row;
		OnLoadingRowDetailsWrapper(row);
	}

	private void EnqueueNewItemMarginComputation()
	{
		if (_newItemMarginComputationPending)
		{
			return;
		}
		_newItemMarginComputationPending = true;
		base.Dispatcher.BeginInvoke((Action)delegate
		{
			double left = 0.0;
			if (base.IsGrouping && InternalScrollHost != null)
			{
				for (ContainerTracking<DataGridRow> containerTracking = _rowTrackingRoot; containerTracking != null; containerTracking = containerTracking.Next)
				{
					DataGridRow container = containerTracking.Container;
					if (!container.IsNewItem)
					{
						GeneralTransform generalTransform = container.TransformToAncestor(InternalScrollHost);
						if (generalTransform != null)
						{
							left = generalTransform.Transform(default(Point)).X;
						}
						break;
					}
				}
			}
			NewItemMargin = new Thickness(left, 0.0, 0.0, 0.0);
			_newItemMarginComputationPending = false;
		}, DispatcherPriority.Input);
	}

	internal override void OnIsGroupingChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnIsGroupingChanged(e);
		EnqueueNewItemMarginComputation();
	}

	internal void OnSelectedCellsChanged(NotifyCollectionChangedAction action, VirtualizedCellInfoCollection oldItems, VirtualizedCellInfoCollection newItems)
	{
		DataGridSelectionMode selectionMode = SelectionMode;
		DataGridSelectionUnit selectionUnit = SelectionUnit;
		if (!IsUpdatingSelectedCells && selectionUnit == DataGridSelectionUnit.FullRow)
		{
			throw new InvalidOperationException(SR.DataGrid_CannotSelectCell);
		}
		if (oldItems != null)
		{
			if (_pendingSelectedCells != null)
			{
				VirtualizedCellInfoCollection.Xor(_pendingSelectedCells, oldItems);
			}
			if (_pendingUnselectedCells == null)
			{
				_pendingUnselectedCells = oldItems;
			}
			else
			{
				_pendingUnselectedCells.Union(oldItems);
			}
		}
		if (newItems != null)
		{
			if (_pendingUnselectedCells != null)
			{
				VirtualizedCellInfoCollection.Xor(_pendingUnselectedCells, newItems);
			}
			if (_pendingSelectedCells == null)
			{
				_pendingSelectedCells = newItems;
			}
			else
			{
				_pendingSelectedCells.Union(newItems);
			}
		}
		if (IsUpdatingSelectedCells)
		{
			return;
		}
		using (UpdateSelectedCells())
		{
			if (selectionMode == DataGridSelectionMode.Single && action == NotifyCollectionChangedAction.Add && _selectedCells.Count > 1)
			{
				_selectedCells.RemoveAllButOne(newItems[0]);
			}
			else
			{
				if (action != NotifyCollectionChangedAction.Remove || oldItems == null || selectionUnit != DataGridSelectionUnit.CellOrRowHeader)
				{
					return;
				}
				bool isUpdatingSelectedItems = base.IsUpdatingSelectedItems;
				if (!isUpdatingSelectedItems)
				{
					BeginUpdateSelectedItems();
				}
				try
				{
					object obj = null;
					foreach (DataGridCellInfo oldItem in oldItems)
					{
						object item = oldItem.Item;
						if (item != obj)
						{
							obj = item;
							if (base.SelectedItems.Contains(item))
							{
								base.SelectedItems.Remove(item);
							}
						}
					}
					return;
				}
				finally
				{
					if (!isUpdatingSelectedItems)
					{
						EndUpdateSelectedItems();
					}
				}
			}
		}
	}

	private void NotifySelectedCellsChanged()
	{
		if ((_pendingSelectedCells != null && _pendingSelectedCells.Count > 0) || (_pendingUnselectedCells != null && _pendingUnselectedCells.Count > 0))
		{
			SelectedCellsChangedEventArgs e = new SelectedCellsChangedEventArgs(this, _pendingSelectedCells, _pendingUnselectedCells);
			int count = _selectedCells.Count;
			int num = ((_pendingUnselectedCells != null) ? _pendingUnselectedCells.Count : 0);
			int num2 = ((_pendingSelectedCells != null) ? _pendingSelectedCells.Count : 0);
			int num3 = count - num2 + num;
			_pendingSelectedCells = null;
			_pendingUnselectedCells = null;
			OnSelectedCellsChanged(e);
			if (num3 == 0 || count == 0)
			{
				CommandManager.InvalidateRequerySuggested();
			}
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.SelectedCellsChanged" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnSelectedCellsChanged(SelectedCellsChangedEventArgs e)
	{
		if (this.SelectedCellsChanged != null)
		{
			this.SelectedCellsChanged(this, e);
		}
		if ((AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection)) && UIElementAutomationPeer.FromElement(this) is DataGridAutomationPeer dataGridAutomationPeer)
		{
			dataGridAutomationPeer.RaiseAutomationCellSelectedEvent(e);
		}
	}

	private static void OnCanExecuteSelectAll(object sender, CanExecuteRoutedEventArgs e)
	{
		DataGrid dataGrid = (DataGrid)sender;
		e.CanExecute = dataGrid.SelectionMode == DataGridSelectionMode.Extended && dataGrid.IsEnabled;
		e.Handled = true;
	}

	private static void OnExecutedSelectAll(object sender, ExecutedRoutedEventArgs e)
	{
		DataGrid dataGrid = (DataGrid)sender;
		if (dataGrid.SelectionUnit == DataGridSelectionUnit.Cell)
		{
			dataGrid.SelectAllCells();
		}
		else
		{
			dataGrid.SelectAll();
		}
		e.Handled = true;
	}

	internal override void SelectAllImpl()
	{
		int count = base.Items.Count;
		int count2 = _columns.Count;
		if (count2 > 0 && count > 0)
		{
			using (UpdateSelectedCells())
			{
				_selectedCells.AddRegion(0, 0, count, count2);
				base.SelectAllImpl();
			}
		}
	}

	internal void SelectOnlyThisCell(DataGridCellInfo currentCellInfo)
	{
		using (UpdateSelectedCells())
		{
			_selectedCells.Clear();
			_selectedCells.Add(currentCellInfo);
		}
	}

	/// <summary>Selects all the cells in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	public void SelectAllCells()
	{
		if (SelectionUnit == DataGridSelectionUnit.FullRow)
		{
			SelectAll();
			return;
		}
		int count = base.Items.Count;
		int count2 = _columns.Count;
		if (count <= 0 || count2 <= 0)
		{
			return;
		}
		using (UpdateSelectedCells())
		{
			if (_selectedCells.Count > 0)
			{
				_selectedCells.Clear();
			}
			_selectedCells.AddRegion(0, 0, count, count2);
		}
	}

	/// <summary>Unselects all the cells in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	public void UnselectAllCells()
	{
		using (UpdateSelectedCells())
		{
			_selectedCells.Clear();
			if (SelectionUnit != 0)
			{
				UnselectAll();
			}
		}
	}

	private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGrid dataGrid = (DataGrid)d;
		DataGridSelectionMode dataGridSelectionMode = (DataGridSelectionMode)e.NewValue;
		bool flag = dataGridSelectionMode == DataGridSelectionMode.Single;
		DataGridSelectionUnit selectionUnit = dataGrid.SelectionUnit;
		if (flag && selectionUnit == DataGridSelectionUnit.Cell)
		{
			using (dataGrid.UpdateSelectedCells())
			{
				dataGrid._selectedCells.RemoveAllButOne();
			}
		}
		dataGrid.CanSelectMultipleItems = dataGridSelectionMode != DataGridSelectionMode.Single;
		if (!flag || selectionUnit != DataGridSelectionUnit.CellOrRowHeader)
		{
			return;
		}
		if (dataGrid.SelectedItems.Count > 0)
		{
			using (dataGrid.UpdateSelectedCells())
			{
				dataGrid._selectedCells.RemoveAllButOneRow(dataGrid.InternalSelectedInfo.Index);
				return;
			}
		}
		using (dataGrid.UpdateSelectedCells())
		{
			dataGrid._selectedCells.RemoveAllButOne();
		}
	}

	private static void OnSelectionUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGrid dataGrid = (DataGrid)d;
		DataGridSelectionUnit num = (DataGridSelectionUnit)e.OldValue;
		if (num != 0)
		{
			dataGrid.UnselectAll();
		}
		if (num != DataGridSelectionUnit.FullRow)
		{
			using (dataGrid.UpdateSelectedCells())
			{
				dataGrid._selectedCells.Clear();
			}
		}
		dataGrid.CoerceValue(Selector.IsSynchronizedWithCurrentItemProperty);
	}

	/// <summary>Invoked when the selection changes.</summary>
	/// <param name="e">The data for the event.</param>
	protected override void OnSelectionChanged(SelectionChangedEventArgs e)
	{
		if (!IsUpdatingSelectedCells)
		{
			using (UpdateSelectedCells())
			{
				int count = e.RemovedInfos.Count;
				for (int i = 0; i < count; i++)
				{
					ItemInfo rowInfo = e.RemovedInfos[i];
					UpdateSelectionOfCellsInRow(rowInfo, isSelected: false);
				}
				count = e.AddedInfos.Count;
				for (int j = 0; j < count; j++)
				{
					ItemInfo rowInfo2 = e.AddedInfos[j];
					UpdateSelectionOfCellsInRow(rowInfo2, isSelected: true);
				}
			}
		}
		CommandManager.InvalidateRequerySuggested();
		if ((AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementSelected) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementAddedToSelection) || AutomationPeer.ListenerExists(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection)) && UIElementAutomationPeer.FromElement(this) is DataGridAutomationPeer dataGridAutomationPeer)
		{
			dataGridAutomationPeer.RaiseAutomationSelectionEvents(e);
		}
		base.OnSelectionChanged(e);
	}

	private void UpdateIsSelected()
	{
		UpdateIsSelected(_pendingUnselectedCells, isSelected: false);
		UpdateIsSelected(_pendingSelectedCells, isSelected: true);
	}

	private void UpdateIsSelected(VirtualizedCellInfoCollection cells, bool isSelected)
	{
		if (cells == null)
		{
			return;
		}
		int count = cells.Count;
		if (count <= 0)
		{
			return;
		}
		bool flag = false;
		if (count > 750)
		{
			int num = 0;
			int count2 = _columns.Count;
			for (ContainerTracking<DataGridRow> containerTracking = _rowTrackingRoot; containerTracking != null; containerTracking = containerTracking.Next)
			{
				num += count2;
				if (num >= count)
				{
					break;
				}
			}
			flag = count > num;
		}
		if (flag)
		{
			for (ContainerTracking<DataGridRow> containerTracking2 = _rowTrackingRoot; containerTracking2 != null; containerTracking2 = containerTracking2.Next)
			{
				DataGridCellsPresenter cellsPresenter = containerTracking2.Container.CellsPresenter;
				if (cellsPresenter != null)
				{
					for (ContainerTracking<DataGridCell> containerTracking3 = cellsPresenter.CellTrackingRoot; containerTracking3 != null; containerTracking3 = containerTracking3.Next)
					{
						DataGridCell container = containerTracking3.Container;
						DataGridCellInfo cell = new DataGridCellInfo(container);
						if (cells.Contains(cell))
						{
							container.SyncIsSelected(isSelected);
						}
					}
				}
			}
			return;
		}
		foreach (DataGridCellInfo cell2 in cells)
		{
			TryFindCell(cell2)?.SyncIsSelected(isSelected);
		}
	}

	private void UpdateSelectionOfCellsInRow(ItemInfo rowInfo, bool isSelected)
	{
		int count = _columns.Count;
		if (count <= 0)
		{
			return;
		}
		if (!isSelected && _pendingInfos != null)
		{
			_pendingInfos.Remove(rowInfo);
		}
		int index = rowInfo.Index;
		if (index >= 0)
		{
			if (isSelected)
			{
				_selectedCells.AddRegion(index, 0, 1, count);
			}
			else
			{
				_selectedCells.RemoveRegion(index, 0, 1, count);
			}
		}
		else if (isSelected)
		{
			EnsurePendingInfos();
			_pendingInfos.Add(rowInfo);
		}
	}

	private void EnsurePendingInfos()
	{
		if (_pendingInfos == null)
		{
			_pendingInfos = new List<ItemInfo>();
		}
	}

	internal void CellIsSelectedChanged(DataGridCell cell, bool isSelected)
	{
		if (!IsUpdatingSelectedCells)
		{
			DataGridCellInfo cell2 = new DataGridCellInfo(cell);
			if (isSelected)
			{
				_selectedCells.AddValidatedCell(cell2);
			}
			else if (_selectedCells.Contains(cell2))
			{
				_selectedCells.Remove(cell2);
			}
		}
	}

	internal void HandleSelectionForCellInput(DataGridCell cell, bool startDragging, bool allowsExtendSelect, bool allowsMinimalSelect)
	{
		if (SelectionUnit == DataGridSelectionUnit.FullRow)
		{
			MakeFullRowSelection(ItemInfoFromContainer(cell.RowOwner), allowsExtendSelect, allowsMinimalSelect);
		}
		else
		{
			MakeCellSelection(new DataGridCellInfo(cell), allowsExtendSelect, allowsMinimalSelect);
		}
		if (startDragging)
		{
			BeginDragging();
		}
	}

	internal void HandleSelectionForRowHeaderAndDetailsInput(DataGridRow row, bool startDragging)
	{
		ItemInfo itemInfo = ItemInfoFromContainer(row);
		if (!_isDraggingSelection && _columns.Count > 0)
		{
			if (!base.IsKeyboardFocusWithin)
			{
				Focus();
			}
			if (CurrentCell.ItemInfo != itemInfo)
			{
				SetCurrentValueInternal(CurrentCellProperty, new DataGridCellInfo(itemInfo, ColumnFromDisplayIndex(0), this));
			}
			else if (_currentCellContainer != null && _currentCellContainer.IsEditing)
			{
				EndEdit(CommitEditCommand, _currentCellContainer, DataGridEditingUnit.Cell, exitEditMode: true);
			}
		}
		if (CanSelectRows)
		{
			MakeFullRowSelection(itemInfo, allowsExtendSelect: true, allowsMinimalSelect: true);
			if (startDragging)
			{
				BeginRowDragging();
			}
		}
	}

	private void BeginRowDragging()
	{
		BeginDragging();
		_isRowDragging = true;
	}

	private void BeginDragging()
	{
		if (Mouse.Capture(this, CaptureMode.SubTree))
		{
			_isDraggingSelection = true;
			_dragPoint = Mouse.GetPosition(this);
		}
	}

	private void EndDragging()
	{
		StopAutoScroll();
		if (Mouse.Captured == this)
		{
			ReleaseMouseCapture();
		}
		_isDraggingSelection = false;
		_isRowDragging = false;
	}

	private void MakeFullRowSelection(ItemInfo info, bool allowsExtendSelect, bool allowsMinimalSelect)
	{
		bool flag = allowsExtendSelect && ShouldExtendSelection;
		bool flag2 = allowsMinimalSelect && ShouldMinimallyModifySelection;
		using (UpdateSelectedCells())
		{
			bool isUpdatingSelectedItems = base.IsUpdatingSelectedItems;
			if (!isUpdatingSelectedItems)
			{
				BeginUpdateSelectedItems();
			}
			try
			{
				if (flag)
				{
					if (_columns.Count <= 0)
					{
						return;
					}
					int num = _selectionAnchor.Value.ItemInfo.Index;
					int num2 = info.Index;
					if (num > num2)
					{
						int num3 = num;
						num = num2;
						num2 = num3;
					}
					if (num < 0 || num2 < 0)
					{
						return;
					}
					int count = _selectedItems.Count;
					if (!flag2)
					{
						bool flag3 = false;
						for (int i = 0; i < count; i++)
						{
							ItemInfo itemInfo = _selectedItems[i];
							int index = itemInfo.Index;
							if (index < num || num2 < index)
							{
								base.SelectionChange.Unselect(itemInfo);
								if (!flag3)
								{
									_selectedCells.Clear();
									flag3 = true;
								}
							}
						}
					}
					else
					{
						int index2 = CurrentCell.ItemInfo.Index;
						int num4 = -1;
						int num5 = -1;
						if (index2 < num)
						{
							num4 = index2;
							num5 = num - 1;
						}
						else if (index2 > num2)
						{
							num4 = num2 + 1;
							num5 = index2;
						}
						if (num4 >= 0 && num5 >= 0)
						{
							for (int j = 0; j < count; j++)
							{
								ItemInfo itemInfo2 = _selectedItems[j];
								int index3 = itemInfo2.Index;
								if (num4 <= index3 && index3 <= num5)
								{
									base.SelectionChange.Unselect(itemInfo2);
								}
							}
							_selectedCells.RemoveRegion(num4, 0, num5 - num4 + 1, Columns.Count);
						}
					}
					IEnumerator enumerator = ((IEnumerable)base.Items).GetEnumerator();
					for (int k = 0; k <= num2; k++)
					{
						if (!enumerator.MoveNext())
						{
							break;
						}
						if (k >= num)
						{
							base.SelectionChange.Select(ItemInfoFromIndex(k), assumeInItemsCollection: true);
						}
					}
					if (enumerator is IDisposable disposable)
					{
						disposable.Dispose();
					}
					_selectedCells.AddRegion(num, 0, num2 - num + 1, _columns.Count);
					return;
				}
				if (flag2 && _selectedItems.Contains(info))
				{
					UnselectItem(info);
				}
				else
				{
					if (!flag2 || !base.CanSelectMultipleItems)
					{
						if (_selectedCells.Count > 0)
						{
							_selectedCells.Clear();
						}
						if (base.SelectedItems.Count > 0)
						{
							base.SelectedItems.Clear();
						}
					}
					if (_editingRowInfo == info)
					{
						int count2 = _columns.Count;
						if (count2 > 0)
						{
							_selectedCells.AddRegion(_editingRowInfo.Index, 0, 1, count2);
						}
						SelectItem(info, selectCells: false);
					}
					else
					{
						SelectItem(info);
					}
				}
				_selectionAnchor = new DataGridCellInfo(info.Clone(), ColumnFromDisplayIndex(0), this);
			}
			finally
			{
				if (!isUpdatingSelectedItems)
				{
					EndUpdateSelectedItems();
				}
			}
		}
	}

	private void MakeCellSelection(DataGridCellInfo cellInfo, bool allowsExtendSelect, bool allowsMinimalSelect)
	{
		bool flag = allowsExtendSelect && ShouldExtendSelection;
		bool flag2 = allowsMinimalSelect && ShouldMinimallyModifySelection;
		using (UpdateSelectedCells())
		{
			int displayIndex = cellInfo.Column.DisplayIndex;
			if (flag)
			{
				_ = base.Items;
				int index = _selectionAnchor.Value.ItemInfo.Index;
				int index2 = cellInfo.ItemInfo.Index;
				int displayIndex2 = _selectionAnchor.Value.Column.DisplayIndex;
				int num = displayIndex;
				if (index < 0 || index2 < 0 || displayIndex2 < 0 || num < 0)
				{
					return;
				}
				int num2 = Math.Abs(index2 - index) + 1;
				int num3 = Math.Abs(num - displayIndex2) + 1;
				if (!flag2)
				{
					if (base.SelectedItems.Count > 0)
					{
						UnselectAll();
					}
					_selectedCells.Clear();
				}
				else
				{
					int index3 = CurrentCell.ItemInfo.Index;
					int displayIndex3 = CurrentCell.Column.DisplayIndex;
					int num4 = Math.Min(index, index3);
					int num5 = Math.Abs(index3 - index) + 1;
					int columnIndex = Math.Min(displayIndex2, displayIndex3);
					int num6 = Math.Abs(displayIndex3 - displayIndex2) + 1;
					_selectedCells.RemoveRegion(num4, columnIndex, num5, num6);
					if (SelectionUnit == DataGridSelectionUnit.CellOrRowHeader)
					{
						int num7 = num4;
						int num8 = num4 + num5 - 1;
						if (num6 <= num3)
						{
							if (num5 > num2)
							{
								int num9 = num5 - num2;
								num7 = ((num4 == index3) ? index3 : (index3 - num9 + 1));
								num8 = num7 + num9 - 1;
							}
							else
							{
								num8 = num7 - 1;
							}
						}
						for (int i = num7; i <= num8; i++)
						{
							object value = base.Items[i];
							if (base.SelectedItems.Contains(value))
							{
								base.SelectedItems.Remove(value);
							}
						}
					}
				}
				_selectedCells.AddRegion(Math.Min(index, index2), Math.Min(displayIndex2, num), num2, num3);
				return;
			}
			bool flag3 = _selectedCells.Contains(cellInfo);
			bool flag4 = _editingRowInfo != null && _editingRowInfo.Index == cellInfo.ItemInfo.Index;
			if (!flag3 && flag4)
			{
				flag3 = _selectedCells.Contains(_editingRowInfo.Index, displayIndex);
			}
			if (flag2 && flag3)
			{
				if (flag4)
				{
					_selectedCells.RemoveRegion(_editingRowInfo.Index, displayIndex, 1, 1);
				}
				else
				{
					_selectedCells.Remove(cellInfo);
				}
				if (SelectionUnit == DataGridSelectionUnit.CellOrRowHeader && base.SelectedItems.Contains(cellInfo.Item))
				{
					base.SelectedItems.Remove(cellInfo.Item);
				}
			}
			else
			{
				if (!flag2 || !base.CanSelectMultipleItems)
				{
					if (base.SelectedItems.Count > 0)
					{
						UnselectAll();
					}
					_selectedCells.Clear();
				}
				if (flag4)
				{
					_selectedCells.AddRegion(_editingRowInfo.Index, displayIndex, 1, 1);
				}
				else
				{
					_selectedCells.AddValidatedCell(cellInfo);
				}
			}
			_selectionAnchor = new DataGridCellInfo(cellInfo);
		}
	}

	private void SelectItem(ItemInfo info)
	{
		SelectItem(info, selectCells: true);
	}

	private void SelectItem(ItemInfo info, bool selectCells)
	{
		if (selectCells)
		{
			using (UpdateSelectedCells())
			{
				int index = info.Index;
				int count = _columns.Count;
				if (index >= 0 && count > 0)
				{
					_selectedCells.AddRegion(index, 0, 1, count);
				}
			}
		}
		UpdateSelectedItems(info, add: true);
	}

	private void UnselectItem(ItemInfo info)
	{
		using (UpdateSelectedCells())
		{
			int index = info.Index;
			int count = _columns.Count;
			if (index >= 0 && count > 0)
			{
				_selectedCells.RemoveRegion(index, 0, 1, count);
			}
		}
		UpdateSelectedItems(info, add: false);
	}

	private void UpdateSelectedItems(ItemInfo info, bool add)
	{
		bool isUpdatingSelectedItems = base.IsUpdatingSelectedItems;
		if (!isUpdatingSelectedItems)
		{
			BeginUpdateSelectedItems();
		}
		try
		{
			if (add)
			{
				SelectedItemCollection.Add(info.Clone());
			}
			else
			{
				SelectedItemCollection.Remove(info);
			}
		}
		finally
		{
			if (!isUpdatingSelectedItems)
			{
				EndUpdateSelectedItems();
			}
		}
	}

	private IDisposable UpdateSelectedCells()
	{
		return new ChangingSelectedCellsHelper(this);
	}

	private void BeginUpdateSelectedCells()
	{
		_updatingSelectedCells = true;
	}

	private void EndUpdateSelectedCells()
	{
		UpdateIsSelected();
		_updatingSelectedCells = false;
		NotifySelectedCellsChanged();
	}

	private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		_currentCellContainer = null;
		List<Tuple<int, int>> ranges = null;
		using (UpdateSelectedCells())
		{
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				ranges = new List<Tuple<int, int>>();
				LocateSelectedItems(ranges);
			}
			_selectedCells.OnItemsCollectionChanged(e, ranges);
		}
		if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
		{
			foreach (object oldItem in e.OldItems)
			{
				_itemAttachedStorage.ClearItem(oldItem);
			}
			return;
		}
		if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			_itemAttachedStorage.Clear();
		}
	}

	private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		d.CoerceValue(CanUserAddRowsProperty);
		d.CoerceValue(CanUserDeleteRowsProperty);
		CommandManager.InvalidateRequerySuggested();
		((DataGrid)d).UpdateVisualState();
	}

	private static void OnIsKeyboardFocusWithinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.Cells | DataGridNotificationTarget.RowHeaders | DataGridNotificationTarget.Rows);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.TextInput" /> routed event.</summary>
	protected override void OnTextInput(TextCompositionEventArgs e)
	{
		base.OnTextInput(e);
		if (e.Handled || string.IsNullOrEmpty(e.Text) || !base.IsTextSearchEnabled)
		{
			return;
		}
		bool flag = e.OriginalSource == this;
		if (!flag)
		{
			ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(e.OriginalSource as DependencyObject);
			flag = itemsControl == this;
			if (!flag && itemsControl is DataGridCellsPresenter dataGridCellsPresenter)
			{
				flag = dataGridCellsPresenter.DataGridOwner == this;
			}
		}
		if (flag)
		{
			TextSearch textSearch = TextSearch.EnsureInstance(this);
			if (textSearch != null)
			{
				textSearch.DoSearch(e.Text);
				e.Handled = true;
			}
		}
	}

	internal override bool FocusItem(ItemInfo info, ItemNavigateArgs itemNavigateArgs)
	{
		object item = info.Item;
		if (item != null)
		{
			DataGridColumn currentColumn = CurrentColumn;
			if (currentColumn == null)
			{
				SetCurrentItem(item);
			}
			else
			{
				DataGridCell dataGridCell = TryFindCell(info, currentColumn);
				if (dataGridCell != null)
				{
					dataGridCell.Focus();
					if (ShouldSelectRowHeader)
					{
						HandleSelectionForRowHeaderAndDetailsInput(dataGridCell.RowOwner, startDragging: false);
					}
					else
					{
						HandleSelectionForCellInput(dataGridCell, startDragging: false, allowsExtendSelect: false, allowsMinimalSelect: false);
					}
				}
			}
		}
		if (itemNavigateArgs.DeviceUsed is KeyboardDevice)
		{
			KeyboardNavigation.ShowFocusVisual();
		}
		return false;
	}

	/// <param name="e">The keyboard data that specifies which keys are pressed.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		switch (e.Key)
		{
		case Key.Tab:
			OnTabKeyDown(e);
			break;
		case Key.Return:
			OnEnterKeyDown(e);
			break;
		case Key.Left:
		case Key.Up:
		case Key.Right:
		case Key.Down:
			OnArrowKeyDown(e);
			break;
		case Key.End:
		case Key.Home:
			OnHomeOrEndKeyDown(e);
			break;
		case Key.Prior:
		case Key.Next:
			OnPageUpOrDownKeyDown(e);
			break;
		}
		if (!e.Handled)
		{
			base.OnKeyDown(e);
		}
	}

	private static FocusNavigationDirection KeyToTraversalDirection(Key key)
	{
		return key switch
		{
			Key.Left => FocusNavigationDirection.Left, 
			Key.Right => FocusNavigationDirection.Right, 
			Key.Up => FocusNavigationDirection.Up, 
			_ => FocusNavigationDirection.Down, 
		};
	}

	private void OnArrowKeyDown(KeyEventArgs e)
	{
		DataGridCell currentCellContainer = CurrentCellContainer;
		if (currentCellContainer == null)
		{
			return;
		}
		e.Handled = true;
		bool isEditing = currentCellContainer.IsEditing;
		KeyboardNavigation current = KeyboardNavigation.Current;
		UIElement uIElement = Keyboard.FocusedElement as UIElement;
		ContentElement contentElement = ((uIElement == null) ? (Keyboard.FocusedElement as ContentElement) : null);
		if (uIElement == null && contentElement == null)
		{
			return;
		}
		bool flag = e.OriginalSource == currentCellContainer;
		if (flag)
		{
			KeyboardNavigationMode directionalNavigation = KeyboardNavigation.GetDirectionalNavigation(this);
			if (directionalNavigation == KeyboardNavigationMode.Once)
			{
				DependencyObject dependencyObject = PredictFocus(KeyToTraversalDirection(e.Key));
				if (dependencyObject != null && !current.IsAncestorOfEx(this, dependencyObject))
				{
					Keyboard.Focus(dependencyObject as IInputElement);
				}
				return;
			}
			int displayIndex = CurrentColumn.DisplayIndex;
			ItemInfo currentInfo = CurrentInfo;
			int index = currentInfo.Index;
			int i = displayIndex;
			int num = index;
			bool flag2 = (e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
			if (!flag2 && (e.Key == Key.Up || e.Key == Key.Down))
			{
				bool flag3 = false;
				if (currentInfo.Item == CollectionView.NewItemPlaceholder)
				{
					flag3 = true;
				}
				else if (base.IsGrouping)
				{
					GroupItem groupItem = DataGridHelper.FindVisualParent<GroupItem>(currentCellContainer);
					if (groupItem != null && base.ItemContainerGenerator.ItemFromContainer(groupItem) is CollectionViewGroupInternal collectionViewGroupInternal && collectionViewGroupInternal.Items.Count > 0 && ((e.Key == Key.Up && ItemsControl.EqualsEx(collectionViewGroupInternal.Items[0], currentInfo.Item)) || (e.Key == Key.Down && ItemsControl.EqualsEx(collectionViewGroupInternal.Items[collectionViewGroupInternal.Items.Count - 1], currentInfo.Item))))
					{
						int num2 = collectionViewGroupInternal.LeafIndexFromItem(null, 0);
						if (e.Key == Key.Down)
						{
							num2 += collectionViewGroupInternal.ItemCount - 1;
						}
						if (index == num2)
						{
							flag3 = true;
						}
					}
				}
				else if ((e.Key == Key.Up && index == 0) || (e.Key == Key.Down && index == base.Items.Count - 1))
				{
					flag3 = true;
				}
				if (flag3 && TryDefaultNavigation(e, currentInfo))
				{
					return;
				}
			}
			Key key = e.Key;
			if (base.FlowDirection == FlowDirection.RightToLeft)
			{
				switch (key)
				{
				case Key.Left:
					key = Key.Right;
					break;
				case Key.Right:
					key = Key.Left;
					break;
				}
			}
			switch (key)
			{
			case Key.Left:
				if (flag2)
				{
					i = InternalColumns.FirstVisibleDisplayIndex;
					break;
				}
				i--;
				while (i >= 0 && !ColumnFromDisplayIndex(i).IsVisible)
				{
					i--;
				}
				if (i >= 0)
				{
					break;
				}
				switch (directionalNavigation)
				{
				case KeyboardNavigationMode.Cycle:
					break;
				case KeyboardNavigationMode.Contained:
				{
					DependencyObject dependencyObject3 = current.PredictFocusedElement(currentCellContainer, KeyToTraversalDirection(key), treeViewNavigation: false, considerDescendants: false);
					if (dependencyObject3 != null && current.IsAncestorOfEx(this, dependencyObject3))
					{
						Keyboard.Focus(dependencyObject3 as IInputElement);
					}
					return;
				}
				default:
					MoveFocus(new TraversalRequest((e.Key == Key.Left) ? FocusNavigationDirection.Left : FocusNavigationDirection.Right));
					return;
				}
				i = InternalColumns.LastVisibleDisplayIndex;
				break;
			case Key.Right:
			{
				if (flag2)
				{
					i = Math.Max(0, InternalColumns.LastVisibleDisplayIndex);
					break;
				}
				i++;
				for (int count = Columns.Count; i < count && !ColumnFromDisplayIndex(i).IsVisible; i++)
				{
				}
				if (i < Columns.Count)
				{
					break;
				}
				switch (directionalNavigation)
				{
				case KeyboardNavigationMode.Cycle:
					break;
				case KeyboardNavigationMode.Contained:
				{
					DependencyObject dependencyObject5 = current.PredictFocusedElement(currentCellContainer, KeyToTraversalDirection(key), treeViewNavigation: false, considerDescendants: false);
					if (dependencyObject5 != null && current.IsAncestorOfEx(this, dependencyObject5))
					{
						Keyboard.Focus(dependencyObject5 as IInputElement);
					}
					return;
				}
				default:
					MoveFocus(new TraversalRequest((e.Key == Key.Left) ? FocusNavigationDirection.Left : FocusNavigationDirection.Right));
					return;
				}
				i = InternalColumns.FirstVisibleDisplayIndex;
				break;
			}
			case Key.Up:
				if (flag2)
				{
					num = 0;
					break;
				}
				num--;
				if (num >= 0)
				{
					break;
				}
				switch (directionalNavigation)
				{
				case KeyboardNavigationMode.Cycle:
					break;
				case KeyboardNavigationMode.Contained:
				{
					DependencyObject dependencyObject4 = current.PredictFocusedElement(currentCellContainer, KeyToTraversalDirection(key), treeViewNavigation: false, considerDescendants: false);
					if (dependencyObject4 != null && current.IsAncestorOfEx(this, dependencyObject4))
					{
						Keyboard.Focus(dependencyObject4 as IInputElement);
					}
					return;
				}
				default:
					MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
					return;
				}
				num = base.Items.Count - 1;
				break;
			default:
				if (flag2)
				{
					num = Math.Max(0, base.Items.Count - 1);
					break;
				}
				num++;
				if (num < base.Items.Count)
				{
					break;
				}
				switch (directionalNavigation)
				{
				case KeyboardNavigationMode.Cycle:
					break;
				case KeyboardNavigationMode.Contained:
				{
					DependencyObject dependencyObject2 = current.PredictFocusedElement(currentCellContainer, KeyToTraversalDirection(key), treeViewNavigation: false, considerDescendants: false);
					if (dependencyObject2 != null && current.IsAncestorOfEx(this, dependencyObject2))
					{
						Keyboard.Focus(dependencyObject2 as IInputElement);
					}
					return;
				}
				default:
					MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
					return;
				}
				num = 0;
				break;
			}
			DataGridColumn column = ColumnFromDisplayIndex(i);
			ItemInfo info = ItemInfoFromIndex(num);
			ScrollCellIntoView(info, column);
			DataGridCell dataGridCell = TryFindCell(info, column);
			if (dataGridCell == null || dataGridCell == currentCellContainer || !dataGridCell.Focus())
			{
				return;
			}
		}
		else if (TryDefaultNavigation(e, null))
		{
			return;
		}
		TraversalRequest request = new TraversalRequest(KeyToTraversalDirection(e.Key));
		if (flag || (uIElement != null && uIElement.MoveFocus(request)) || (contentElement != null && contentElement.MoveFocus(request)))
		{
			SelectAndEditOnFocusMove(e, currentCellContainer, isEditing, allowsExtendSelect: true, ignoreControlKey: true);
		}
	}

	private bool TryDefaultNavigation(KeyEventArgs e, ItemInfo currentInfo)
	{
		if (Keyboard.FocusedElement is FrameworkElement frameworkElement && base.ItemsHost.IsAncestorOf(frameworkElement))
		{
			PrepareNavigateByLine(currentInfo, frameworkElement, (e.Key == Key.Up) ? FocusNavigationDirection.Up : FocusNavigationDirection.Down, new ItemNavigateArgs(e.KeyboardDevice, Keyboard.Modifiers), out var container);
			if (container != null)
			{
				DataGridRow dataGridRow = DataGridHelper.FindVisualParent<DataGridRow>(container);
				if (dataGridRow == null || dataGridRow.DataGridOwner != this)
				{
					container.Focus();
					return true;
				}
			}
		}
		return false;
	}

	private void OnTabKeyDown(KeyEventArgs e)
	{
		DataGridCell currentCellContainer = CurrentCellContainer;
		if (currentCellContainer == null)
		{
			return;
		}
		bool isEditing = currentCellContainer.IsEditing;
		bool flag = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
		UIElement uIElement = Keyboard.FocusedElement as UIElement;
		ContentElement contentElement = ((uIElement == null) ? (Keyboard.FocusedElement as ContentElement) : null);
		if (uIElement == null && contentElement == null)
		{
			return;
		}
		e.Handled = true;
		TraversalRequest request = new TraversalRequest(flag ? FocusNavigationDirection.Previous : FocusNavigationDirection.Next);
		if ((uIElement == null || !uIElement.MoveFocus(request)) && (contentElement == null || !contentElement.MoveFocus(request)))
		{
			return;
		}
		if (isEditing && flag && Keyboard.FocusedElement == currentCellContainer)
		{
			currentCellContainer.MoveFocus(request);
		}
		if (base.IsGrouping && isEditing)
		{
			DataGridCell cellForSelectAndEditOnFocusMove = GetCellForSelectAndEditOnFocusMove();
			if (cellForSelectAndEditOnFocusMove != null && cellForSelectAndEditOnFocusMove.RowDataItem == currentCellContainer.RowDataItem)
			{
				DataGridCell dataGridCell = TryFindCell(cellForSelectAndEditOnFocusMove.RowDataItem, cellForSelectAndEditOnFocusMove.Column);
				if (dataGridCell == null)
				{
					UpdateLayout();
					dataGridCell = TryFindCell(cellForSelectAndEditOnFocusMove.RowDataItem, cellForSelectAndEditOnFocusMove.Column);
				}
				if (dataGridCell != null && dataGridCell != cellForSelectAndEditOnFocusMove)
				{
					dataGridCell.Focus();
				}
			}
		}
		SelectAndEditOnFocusMove(e, currentCellContainer, isEditing, allowsExtendSelect: false, ignoreControlKey: true);
	}

	private void OnEnterKeyDown(KeyEventArgs e)
	{
		DataGridCell currentCellContainer = CurrentCellContainer;
		if (currentCellContainer == null || _columns.Count <= 0)
		{
			return;
		}
		e.Handled = true;
		DataGridColumn column = currentCellContainer.Column;
		if (!CommitAnyEdit() || (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0)
		{
			return;
		}
		bool flag = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
		int count = base.Items.Count;
		int num = CurrentInfo.Index;
		if (num < 0)
		{
			num = base.Items.IndexOf(CurrentItem);
		}
		num = Math.Max(0, Math.Min(count - 1, num + ((!flag) ? 1 : (-1))));
		if (num < count)
		{
			ItemInfo itemInfo = ItemInfoFromIndex(num);
			ScrollIntoView(itemInfo, column);
			if (!ItemsControl.EqualsEx(CurrentCell.Item, itemInfo.Item))
			{
				SetCurrentValueInternal(CurrentCellProperty, new DataGridCellInfo(itemInfo, column, this));
				SelectAndEditOnFocusMove(e, currentCellContainer, wasEditing: false, allowsExtendSelect: false, ignoreControlKey: true);
			}
			else
			{
				CurrentCellContainer?.Focus();
			}
		}
	}

	private DataGridCell GetCellForSelectAndEditOnFocusMove()
	{
		DataGridCell dataGridCell = Keyboard.FocusedElement as DataGridCell;
		if (dataGridCell == null && CurrentCellContainer != null && CurrentCellContainer.IsKeyboardFocusWithin)
		{
			dataGridCell = CurrentCellContainer;
		}
		return dataGridCell;
	}

	private void SelectAndEditOnFocusMove(KeyEventArgs e, DataGridCell oldCell, bool wasEditing, bool allowsExtendSelect, bool ignoreControlKey)
	{
		DataGridCell cellForSelectAndEditOnFocusMove = GetCellForSelectAndEditOnFocusMove();
		if (cellForSelectAndEditOnFocusMove == null || cellForSelectAndEditOnFocusMove.DataGridOwner != this)
		{
			return;
		}
		if (ignoreControlKey || (e.KeyboardDevice.Modifiers & ModifierKeys.Control) == 0)
		{
			if (ShouldSelectRowHeader && allowsExtendSelect)
			{
				HandleSelectionForRowHeaderAndDetailsInput(cellForSelectAndEditOnFocusMove.RowOwner, startDragging: false);
			}
			else
			{
				HandleSelectionForCellInput(cellForSelectAndEditOnFocusMove, startDragging: false, allowsExtendSelect, allowsMinimalSelect: false);
			}
		}
		if (wasEditing && !cellForSelectAndEditOnFocusMove.IsEditing && oldCell.RowDataItem == cellForSelectAndEditOnFocusMove.RowDataItem)
		{
			BeginEdit(e);
		}
	}

	private void OnHomeOrEndKeyDown(KeyEventArgs e)
	{
		if (_columns.Count <= 0 || base.Items.Count <= 0)
		{
			return;
		}
		e.Handled = true;
		bool flag = e.Key == Key.Home;
		bool num = (e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
		if (num)
		{
			ScrollViewer internalScrollHost = InternalScrollHost;
			if (internalScrollHost != null)
			{
				if (flag)
				{
					internalScrollHost.ScrollToHome();
				}
				else
				{
					internalScrollHost.ScrollToEnd();
				}
			}
		}
		ItemInfo info = (num ? ItemInfoFromIndex((!flag) ? (base.Items.Count - 1) : 0) : CurrentInfo);
		DataGridColumn column = ColumnFromDisplayIndex(flag ? InternalColumns.FirstVisibleDisplayIndex : InternalColumns.LastVisibleDisplayIndex);
		ScrollCellIntoView(info, column);
		DataGridCell dataGridCell = TryFindCell(info, column);
		if (dataGridCell != null)
		{
			dataGridCell.Focus();
			if (ShouldSelectRowHeader)
			{
				HandleSelectionForRowHeaderAndDetailsInput(dataGridCell.RowOwner, startDragging: false);
			}
			else
			{
				HandleSelectionForCellInput(dataGridCell, startDragging: false, allowsExtendSelect: true, allowsMinimalSelect: false);
			}
		}
	}

	private void OnPageUpOrDownKeyDown(KeyEventArgs e)
	{
		ScrollViewer internalScrollHost = InternalScrollHost;
		if (internalScrollHost == null)
		{
			return;
		}
		e.Handled = true;
		ItemInfo currentInfo = CurrentInfo;
		if (VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item && !base.IsGrouping)
		{
			int index = currentInfo.Index;
			if (index < 0)
			{
				return;
			}
			int num = Math.Max(1, (int)internalScrollHost.ViewportHeight - 1);
			int val = ((e.Key == Key.Prior) ? (index - num) : (index + num));
			val = Math.Max(0, Math.Min(val, base.Items.Count - 1));
			ItemInfo itemInfo = ItemInfoFromIndex(val);
			DataGridColumn currentColumn = CurrentColumn;
			if (currentColumn == null)
			{
				OnBringItemIntoView(itemInfo);
				SetCurrentItem(itemInfo.Item);
				return;
			}
			ScrollCellIntoView(itemInfo, currentColumn);
			DataGridCell dataGridCell = TryFindCell(itemInfo, currentColumn);
			if (dataGridCell != null)
			{
				dataGridCell.Focus();
				if (ShouldSelectRowHeader)
				{
					HandleSelectionForRowHeaderAndDetailsInput(dataGridCell.RowOwner, startDragging: false);
				}
				else
				{
					HandleSelectionForCellInput(dataGridCell, startDragging: false, allowsExtendSelect: true, allowsMinimalSelect: false);
				}
			}
			return;
		}
		FocusNavigationDirection direction = ((e.Key == Key.Prior) ? FocusNavigationDirection.Up : FocusNavigationDirection.Down);
		ItemInfo startingInfo = currentInfo;
		FrameworkElement frameworkElement = null;
		if (base.IsGrouping)
		{
			frameworkElement = Keyboard.FocusedElement as FrameworkElement;
			if (frameworkElement != null)
			{
				startingInfo = null;
				DataGridRow dataGridRow = frameworkElement as DataGridRow;
				if (dataGridRow == null)
				{
					dataGridRow = DataGridHelper.FindVisualParent<DataGridRow>(frameworkElement);
				}
				if (dataGridRow != null && ItemsControl.ItemsControlFromItemContainer(dataGridRow) as DataGrid == this)
				{
					startingInfo = ItemInfoFromContainer(dataGridRow);
				}
			}
		}
		PrepareToNavigateByPage(startingInfo, frameworkElement, direction, new ItemNavigateArgs(Keyboard.PrimaryDevice, Keyboard.Modifiers), out var container);
		DataGridRow dataGridRow2 = container as DataGridRow;
		if (dataGridRow2 == null)
		{
			dataGridRow2 = DataGridHelper.FindVisualParent<DataGridRow>(container);
		}
		if (dataGridRow2 != null)
		{
			ItemInfo itemInfo2 = ItemInfoFromContainer(dataGridRow2);
			DataGridColumn currentColumn2 = CurrentColumn;
			if (currentColumn2 == null)
			{
				SetCurrentItem(itemInfo2.Item);
				return;
			}
			DataGridCell dataGridCell2 = TryFindCell(itemInfo2, currentColumn2);
			if (dataGridCell2 != null)
			{
				dataGridCell2.Focus();
				if (ShouldSelectRowHeader)
				{
					HandleSelectionForRowHeaderAndDetailsInput(dataGridCell2.RowOwner, startDragging: false);
				}
				else
				{
					HandleSelectionForCellInput(dataGridCell2, startDragging: false, allowsExtendSelect: true, allowsMinimalSelect: false);
				}
			}
		}
		else
		{
			container?.Focus();
		}
	}

	/// <summary>Updates the collection of items that are selected due to the user dragging the mouse in the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <param name="e">The mouse data.</param>
	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (!_isDraggingSelection)
		{
			return;
		}
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			Point position = Mouse.GetPosition(this);
			if (DoubleUtil.AreClose(position, _dragPoint))
			{
				return;
			}
			_dragPoint = position;
			RelativeMousePositions relativeMousePosition = RelativeMousePosition;
			if (relativeMousePosition == RelativeMousePositions.Over)
			{
				if (_isRowDragging)
				{
					DataGridRow mouseOverRow = MouseOverRow;
					if (mouseOverRow != null && mouseOverRow.Item != CurrentItem)
					{
						HandleSelectionForRowHeaderAndDetailsInput(mouseOverRow, startDragging: false);
						SetCurrentItem(mouseOverRow.Item);
						e.Handled = true;
					}
					return;
				}
				DataGridCell dataGridCell = MouseOverCell;
				if (dataGridCell == null && MouseOverRow != null)
				{
					dataGridCell = GetCellNearMouse();
				}
				if (dataGridCell != null && dataGridCell != CurrentCellContainer)
				{
					HandleSelectionForCellInput(dataGridCell, startDragging: false, allowsExtendSelect: true, allowsMinimalSelect: true);
					dataGridCell.Focus();
					e.Handled = true;
				}
			}
			else if (_isRowDragging && IsMouseToLeftOrRightOnly(relativeMousePosition))
			{
				DataGridRow rowNearMouse = GetRowNearMouse();
				if (rowNearMouse != null && rowNearMouse.Item != CurrentItem)
				{
					HandleSelectionForRowHeaderAndDetailsInput(rowNearMouse, startDragging: false);
					SetCurrentItem(rowNearMouse.Item);
					e.Handled = true;
				}
			}
			else if (_hasAutoScrolled)
			{
				if (DoAutoScroll())
				{
					e.Handled = true;
				}
			}
			else
			{
				StartAutoScroll();
			}
		}
		else
		{
			EndDragging();
		}
	}

	private static void OnAnyMouseUpThunk(object sender, MouseButtonEventArgs e)
	{
		((DataGrid)sender).OnAnyMouseUp(e);
	}

	private void OnAnyMouseUp(MouseButtonEventArgs e)
	{
		EndDragging();
	}

	/// <summary>Selects a cell if its context menu is opened.</summary>
	/// <param name="e">The item whose context menu was opened.</param>
	protected override void OnContextMenuOpening(ContextMenuEventArgs e)
	{
		if (!base.IsEnabled)
		{
			return;
		}
		DataGridCell dataGridCell = null;
		DataGridRowHeader dataGridRowHeader = null;
		for (UIElement uIElement = e.OriginalSource as UIElement; uIElement != null; uIElement = VisualTreeHelper.GetParent(uIElement) as UIElement)
		{
			dataGridCell = uIElement as DataGridCell;
			if (dataGridCell != null)
			{
				break;
			}
			dataGridRowHeader = uIElement as DataGridRowHeader;
			if (dataGridRowHeader != null)
			{
				break;
			}
		}
		if (dataGridCell != null && !dataGridCell.IsSelected && !dataGridCell.IsKeyboardFocusWithin)
		{
			dataGridCell.Focus();
			HandleSelectionForCellInput(dataGridCell, startDragging: false, allowsExtendSelect: true, allowsMinimalSelect: true);
		}
		if (dataGridRowHeader != null)
		{
			DataGridRow parentRow = dataGridRowHeader.ParentRow;
			if (parentRow != null && !parentRow.IsSelected)
			{
				HandleSelectionForRowHeaderAndDetailsInput(parentRow, startDragging: false);
			}
		}
	}

	private DataGridRow GetRowNearMouse()
	{
		Panel internalItemsHost = InternalItemsHost;
		if (internalItemsHost != null)
		{
			bool isGrouping = base.IsGrouping;
			for (int num = (isGrouping ? (base.Items.Count - 1) : (internalItemsHost.Children.Count - 1)); num >= 0; num--)
			{
				DataGridRow dataGridRow = null;
				dataGridRow = ((!isGrouping) ? (internalItemsHost.Children[num] as DataGridRow) : (base.ItemContainerGenerator.ContainerFromIndex(num) as DataGridRow));
				if (dataGridRow != null)
				{
					Point position = Mouse.GetPosition(dataGridRow);
					Rect rect = new Rect(default(Point), dataGridRow.RenderSize);
					if (position.Y >= rect.Top && position.Y <= rect.Bottom)
					{
						return dataGridRow;
					}
				}
			}
		}
		return null;
	}

	private DataGridCell GetCellNearMouse()
	{
		Panel internalItemsHost = InternalItemsHost;
		if (internalItemsHost != null)
		{
			Rect itemsHostBounds = new Rect(default(Point), internalItemsHost.RenderSize);
			double num = double.PositiveInfinity;
			DataGridCell dataGridCell = null;
			bool isMouseInCorner = IsMouseInCorner(RelativeMousePosition);
			bool isGrouping = base.IsGrouping;
			for (int num2 = (isGrouping ? (base.Items.Count - 1) : (internalItemsHost.Children.Count - 1)); num2 >= 0; num2--)
			{
				DataGridRow dataGridRow = null;
				dataGridRow = ((!isGrouping) ? (internalItemsHost.Children[num2] as DataGridRow) : (base.ItemContainerGenerator.ContainerFromIndex(num2) as DataGridRow));
				if (dataGridRow != null)
				{
					DataGridCellsPresenter cellsPresenter = dataGridRow.CellsPresenter;
					if (cellsPresenter != null)
					{
						for (ContainerTracking<DataGridCell> containerTracking = cellsPresenter.CellTrackingRoot; containerTracking != null; containerTracking = containerTracking.Next)
						{
							DataGridCell container = containerTracking.Container;
							if (CalculateCellDistance(container, dataGridRow, internalItemsHost, itemsHostBounds, isMouseInCorner, out var distance) && (dataGridCell == null || distance < num))
							{
								num = distance;
								dataGridCell = container;
							}
						}
						DataGridRowHeader rowHeader = dataGridRow.RowHeader;
						if (rowHeader != null && CalculateCellDistance(rowHeader, dataGridRow, internalItemsHost, itemsHostBounds, isMouseInCorner, out var distance2) && (dataGridCell == null || distance2 < num))
						{
							DataGridCell dataGridCell2 = dataGridRow.TryGetCell(DisplayIndexMap[0]);
							if (dataGridCell2 != null)
							{
								num = distance2;
								dataGridCell = dataGridCell2;
							}
						}
					}
				}
			}
			return dataGridCell;
		}
		return null;
	}

	private static bool CalculateCellDistance(FrameworkElement cell, DataGridRow rowOwner, Panel itemsHost, Rect itemsHostBounds, bool isMouseInCorner, out double distance)
	{
		GeneralTransform generalTransform = cell.TransformToAncestor(itemsHost);
		Rect rect = new Rect(default(Point), cell.RenderSize);
		if (itemsHostBounds.Contains(generalTransform.TransformBounds(rect)))
		{
			Point position = Mouse.GetPosition(cell);
			if (isMouseInCorner)
			{
				distance = new Vector(position.X - rect.Width * 0.5, position.Y - rect.Height * 0.5).Length;
				return true;
			}
			Point position2 = Mouse.GetPosition(rowOwner);
			Rect rect2 = new Rect(default(Point), rowOwner.RenderSize);
			if (position.X >= rect.Left && position.X <= rect.Right)
			{
				if (position2.Y >= rect2.Top && position2.Y <= rect2.Bottom)
				{
					distance = 0.0;
				}
				else
				{
					distance = Math.Abs(position.Y - rect.Top);
				}
				return true;
			}
			if (position2.Y >= rect2.Top && position2.Y <= rect2.Bottom)
			{
				distance = Math.Abs(position.X - rect.Left);
				return true;
			}
		}
		distance = double.PositiveInfinity;
		return false;
	}

	private static bool IsMouseToLeft(RelativeMousePositions position)
	{
		return (position & RelativeMousePositions.Left) == RelativeMousePositions.Left;
	}

	private static bool IsMouseToRight(RelativeMousePositions position)
	{
		return (position & RelativeMousePositions.Right) == RelativeMousePositions.Right;
	}

	private static bool IsMouseAbove(RelativeMousePositions position)
	{
		return (position & RelativeMousePositions.Above) == RelativeMousePositions.Above;
	}

	private static bool IsMouseBelow(RelativeMousePositions position)
	{
		return (position & RelativeMousePositions.Below) == RelativeMousePositions.Below;
	}

	private static bool IsMouseToLeftOrRightOnly(RelativeMousePositions position)
	{
		if (position != RelativeMousePositions.Left)
		{
			return position == RelativeMousePositions.Right;
		}
		return true;
	}

	private static bool IsMouseInCorner(RelativeMousePositions position)
	{
		if (position != 0 && position != RelativeMousePositions.Above && position != RelativeMousePositions.Below && position != RelativeMousePositions.Left)
		{
			return position != RelativeMousePositions.Right;
		}
		return false;
	}

	/// <summary>Returns the automation peer for this <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>The automation peer for this <see cref="T:System.Windows.Controls.DataGrid" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DataGridAutomationPeer(this);
	}

	private CellAutomationValueHolder GetCellAutomationValueHolder(object item, DataGridColumn column)
	{
		if (_editingRowInfo == null || !ItemsControl.EqualsEx(item, _editingRowInfo.Item) || !_editingCellAutomationValueHolders.TryGetValue(column, out var value))
		{
			DataGridCell dataGridCell = TryFindCell(item, column);
			return (dataGridCell != null) ? new CellAutomationValueHolder(dataGridCell) : new CellAutomationValueHolder(item, column);
		}
		return value;
	}

	internal string GetCellAutomationValue(object item, DataGridColumn column)
	{
		return GetCellAutomationValueHolder(item, column).Value;
	}

	internal object GetCellClipboardValue(object item, DataGridColumn column)
	{
		return GetCellAutomationValueHolder(item, column).GetClipboardValue();
	}

	internal void SetCellAutomationValue(object item, DataGridColumn column, string value)
	{
		SetCellValue(item, column, value, clipboard: false);
	}

	internal void SetCellClipboardValue(object item, DataGridColumn column, object value)
	{
		SetCellValue(item, column, value, clipboard: true);
	}

	private void SetCellValue(object item, DataGridColumn column, object value, bool clipboard)
	{
		CurrentCellContainer = TryFindCell(item, column);
		if (CurrentCellContainer == null)
		{
			ScrollCellIntoView(NewItemInfo(item), column);
			CurrentCellContainer = TryFindCell(item, column);
		}
		if (CurrentCellContainer != null && BeginEdit())
		{
			if (_editingCellAutomationValueHolders.TryGetValue(column, out var value2))
			{
				value2.SetValue(this, value, clipboard);
			}
			else
			{
				CancelEdit();
			}
		}
	}

	private void EnsureCellAutomationValueHolder(DataGridCell cell)
	{
		if (!_editingCellAutomationValueHolders.ContainsKey(cell.Column))
		{
			_editingCellAutomationValueHolders.Add(cell.Column, new CellAutomationValueHolder(cell));
		}
	}

	private void UpdateCellAutomationValueHolder(DataGridCell cell)
	{
		if (_editingCellAutomationValueHolders.TryGetValue(cell.Column, out var value))
		{
			value.TrackValue();
		}
	}

	private void ReleaseCellAutomationValueHolders()
	{
		foreach (KeyValuePair<DataGridColumn, CellAutomationValueHolder> editingCellAutomationValueHolder in _editingCellAutomationValueHolders)
		{
			editingCellAutomationValueHolder.Value.TrackValue();
		}
		_editingCellAutomationValueHolders.Clear();
	}

	internal DataGridCell TryFindCell(DataGridCellInfo info)
	{
		return TryFindCell(LeaseItemInfo(info.ItemInfo), info.Column);
	}

	internal DataGridCell TryFindCell(ItemInfo info, DataGridColumn column)
	{
		DataGridRow dataGridRow = (DataGridRow)info.Container;
		int num = _columns.IndexOf(column);
		if (dataGridRow != null && num >= 0)
		{
			return dataGridRow.TryGetCell(num);
		}
		return null;
	}

	internal DataGridCell TryFindCell(object item, DataGridColumn column)
	{
		DataGridRow dataGridRow = (DataGridRow)base.ItemContainerGenerator.ContainerFromItem(item);
		int num = _columns.IndexOf(column);
		if (dataGridRow != null && num >= 0)
		{
			return dataGridRow.TryGetCell(num);
		}
		return null;
	}

	private static object OnCoerceCanUserSortColumns(DependencyObject d, object baseValue)
	{
		DataGrid dataGrid = (DataGrid)d;
		if (DataGridHelper.IsPropertyTransferEnabled(dataGrid, CanUserSortColumnsProperty) && DataGridHelper.IsDefaultValue(dataGrid, CanUserSortColumnsProperty) && !dataGrid.Items.CanSort)
		{
			return false;
		}
		return baseValue;
	}

	private static void OnCanUserSortColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGridHelper.TransferProperty((DataGrid)d, CanUserSortColumnsProperty);
		OnNotifyColumnPropertyChanged(d, e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.Sorting" /> event.</summary>
	/// <param name="eventArgs">The data for the event.</param>
	protected virtual void OnSorting(DataGridSortingEventArgs eventArgs)
	{
		eventArgs.Handled = false;
		if (this.Sorting != null)
		{
			this.Sorting(this, eventArgs);
		}
		if (!eventArgs.Handled)
		{
			DefaultSort(eventArgs.Column, (Keyboard.Modifiers & ModifierKeys.Shift) != ModifierKeys.Shift);
		}
	}

	internal void PerformSort(DataGridColumn sortColumn)
	{
		if (!CanUserSortColumns || !sortColumn.CanUserSort || !CommitAnyEdit())
		{
			return;
		}
		PrepareForSort(sortColumn);
		DataGridSortingEventArgs eventArgs = new DataGridSortingEventArgs(sortColumn);
		OnSorting(eventArgs);
		if (!base.Items.NeedsRefresh)
		{
			return;
		}
		try
		{
			base.Items.Refresh();
		}
		catch (InvalidOperationException innerException)
		{
			base.Items.SortDescriptions.Clear();
			throw new InvalidOperationException(SR.DataGrid_ProbableInvalidSortDescription, innerException);
		}
	}

	private void PrepareForSort(DataGridColumn sortColumn)
	{
		if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift || Columns == null)
		{
			return;
		}
		foreach (DataGridColumn column in Columns)
		{
			if (column != sortColumn)
			{
				column.SortDirection = null;
			}
		}
	}

	private void DefaultSort(DataGridColumn column, bool clearExistingSortDescriptions)
	{
		ListSortDirection listSortDirection = ListSortDirection.Ascending;
		ListSortDirection? sortDirection = column.SortDirection;
		if (sortDirection.HasValue && sortDirection.Value == ListSortDirection.Ascending)
		{
			listSortDirection = ListSortDirection.Descending;
		}
		string sortMemberPath = column.SortMemberPath;
		if (string.IsNullOrEmpty(sortMemberPath))
		{
			return;
		}
		try
		{
			using (base.Items.DeferRefresh())
			{
				int num = -1;
				if (clearExistingSortDescriptions)
				{
					base.Items.SortDescriptions.Clear();
				}
				else
				{
					for (int i = 0; i < base.Items.SortDescriptions.Count; i++)
					{
						if (string.Compare(base.Items.SortDescriptions[i].PropertyName, sortMemberPath, StringComparison.Ordinal) == 0 && (GroupingSortDescriptionIndices == null || !GroupingSortDescriptionIndices.Contains(i)))
						{
							num = i;
							break;
						}
					}
				}
				SortDescription sortDescription = new SortDescription(sortMemberPath, listSortDirection);
				if (num >= 0)
				{
					base.Items.SortDescriptions[num] = sortDescription;
				}
				else
				{
					base.Items.SortDescriptions.Add(sortDescription);
				}
				if (clearExistingSortDescriptions || !_sortingStarted)
				{
					RegenerateGroupingSortDescriptions();
					_sortingStarted = true;
				}
			}
			column.SortDirection = listSortDirection;
		}
		catch (InvalidOperationException exception)
		{
			TraceData.TraceAndNotify(TraceEventType.Error, TraceData.CannotSort(sortMemberPath), exception);
			base.Items.SortDescriptions.Clear();
		}
	}

	private void OnItemsSortDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		if (_ignoreSortDescriptionsChange || GroupingSortDescriptionIndices == null)
		{
			return;
		}
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
		{
			int j = 0;
			for (int count = GroupingSortDescriptionIndices.Count; j < count; j++)
			{
				if (GroupingSortDescriptionIndices[j] >= e.NewStartingIndex)
				{
					GroupingSortDescriptionIndices[j]++;
				}
			}
			break;
		}
		case NotifyCollectionChangedAction.Remove:
		{
			int i = 0;
			for (int num = GroupingSortDescriptionIndices.Count; i < num; i++)
			{
				if (GroupingSortDescriptionIndices[i] > e.OldStartingIndex)
				{
					GroupingSortDescriptionIndices[i]--;
				}
				else if (GroupingSortDescriptionIndices[i] == e.OldStartingIndex)
				{
					GroupingSortDescriptionIndices.RemoveAt(i);
					i--;
					num--;
				}
			}
			break;
		}
		case NotifyCollectionChangedAction.Replace:
			GroupingSortDescriptionIndices.Remove(e.OldStartingIndex);
			break;
		case NotifyCollectionChangedAction.Reset:
			GroupingSortDescriptionIndices.Clear();
			break;
		case NotifyCollectionChangedAction.Move:
			break;
		}
	}

	private void RemoveGroupingSortDescriptions()
	{
		if (GroupingSortDescriptionIndices == null)
		{
			return;
		}
		bool ignoreSortDescriptionsChange = _ignoreSortDescriptionsChange;
		_ignoreSortDescriptionsChange = true;
		try
		{
			int i = 0;
			for (int count = GroupingSortDescriptionIndices.Count; i < count; i++)
			{
				base.Items.SortDescriptions.RemoveAt(GroupingSortDescriptionIndices[i] - i);
			}
			GroupingSortDescriptionIndices.Clear();
		}
		finally
		{
			_ignoreSortDescriptionsChange = ignoreSortDescriptionsChange;
		}
	}

	private static bool CanConvertToSortDescription(PropertyGroupDescription propertyGroupDescription)
	{
		if (propertyGroupDescription != null && propertyGroupDescription.Converter == null && propertyGroupDescription.StringComparison == StringComparison.Ordinal)
		{
			return true;
		}
		return false;
	}

	private void AddGroupingSortDescriptions()
	{
		bool ignoreSortDescriptionsChange = _ignoreSortDescriptionsChange;
		_ignoreSortDescriptionsChange = true;
		try
		{
			int index = 0;
			foreach (GroupDescription groupDescription in base.Items.GroupDescriptions)
			{
				PropertyGroupDescription propertyGroupDescription = groupDescription as PropertyGroupDescription;
				if (CanConvertToSortDescription(propertyGroupDescription))
				{
					SortDescription item = new SortDescription(propertyGroupDescription.PropertyName, ListSortDirection.Ascending);
					base.Items.SortDescriptions.Insert(index, item);
					if (GroupingSortDescriptionIndices == null)
					{
						GroupingSortDescriptionIndices = new List<int>();
					}
					GroupingSortDescriptionIndices.Add(index++);
				}
			}
		}
		finally
		{
			_ignoreSortDescriptionsChange = ignoreSortDescriptionsChange;
		}
	}

	private void RegenerateGroupingSortDescriptions()
	{
		RemoveGroupingSortDescriptions();
		AddGroupingSortDescriptions();
	}

	private void OnItemsGroupDescriptionsChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		EnqueueNewItemMarginComputation();
		if (!_sortingStarted)
		{
			return;
		}
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
			if (CanConvertToSortDescription(e.NewItems[0] as PropertyGroupDescription))
			{
				RegenerateGroupingSortDescriptions();
			}
			break;
		case NotifyCollectionChangedAction.Remove:
			if (CanConvertToSortDescription(e.OldItems[0] as PropertyGroupDescription))
			{
				RegenerateGroupingSortDescriptions();
			}
			break;
		case NotifyCollectionChangedAction.Replace:
			if (CanConvertToSortDescription(e.OldItems[0] as PropertyGroupDescription) || CanConvertToSortDescription(e.NewItems[0] as PropertyGroupDescription))
			{
				RegenerateGroupingSortDescriptions();
			}
			break;
		case NotifyCollectionChangedAction.Reset:
			RemoveGroupingSortDescriptions();
			break;
		case NotifyCollectionChangedAction.Move:
			break;
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.AutoGeneratedColumns" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnAutoGeneratedColumns(EventArgs e)
	{
		if (this.AutoGeneratedColumns != null)
		{
			this.AutoGeneratedColumns(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.AutoGeneratingColumn" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected virtual void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
	{
		if (this.AutoGeneratingColumn != null)
		{
			this.AutoGeneratingColumn(this, e);
		}
	}

	/// <summary>Determines the desired size of the <see cref="T:System.Windows.Controls.DataGrid" />.</summary>
	/// <returns>The desired size of the <see cref="T:System.Windows.Controls.DataGrid" />.</returns>
	/// <param name="availableSize">The maximum size that the <see cref="T:System.Windows.Controls.DataGrid" /> can occupy.</param>
	protected override Size MeasureOverride(Size availableSize)
	{
		if (_measureNeverInvoked)
		{
			_measureNeverInvoked = false;
			if (AutoGenerateColumns)
			{
				AddAutoColumns();
			}
			InternalColumns.InitializeDisplayIndexMap();
			CoerceValue(FrozenColumnCountProperty);
			CoerceValue(CanUserAddRowsProperty);
			CoerceValue(CanUserDeleteRowsProperty);
			UpdateNewItemPlaceholder(isAddingNewItem: false);
			EnsureItemBindingGroup();
			base.ItemBindingGroup.SharesProposedValues = true;
		}
		else if (DeferAutoGeneration && AutoGenerateColumns)
		{
			AddAutoColumns();
		}
		return base.MeasureOverride(availableSize);
	}

	private void EnsureItemBindingGroup()
	{
		if (base.ItemBindingGroup == null)
		{
			_defaultBindingGroup = new BindingGroup();
			SetCurrentValue(ItemsControl.ItemBindingGroupProperty, _defaultBindingGroup);
		}
	}

	private void ClearSortDescriptionsOnItemsSourceChange()
	{
		base.Items.SortDescriptions.Clear();
		_sortingStarted = false;
		GroupingSortDescriptionIndices?.Clear();
		foreach (DataGridColumn column in Columns)
		{
			column.SortDirection = null;
		}
	}

	private static object OnCoerceItemsSourceProperty(DependencyObject d, object baseValue)
	{
		DataGrid dataGrid = (DataGrid)d;
		if (baseValue != dataGrid._cachedItemsSource && dataGrid._cachedItemsSource != null)
		{
			dataGrid.ClearSortDescriptionsOnItemsSourceChange();
		}
		return baseValue;
	}

	/// <summary>Invoked when the <see cref="P:System.Windows.Controls.ItemsControl.ItemsSource" /> property changes. </summary>
	/// <param name="oldValue">The old source.</param>
	/// <param name="newValue">The new source.</param>
	protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
	{
		base.OnItemsSourceChanged(oldValue, newValue);
		if (newValue == null)
		{
			ClearSortDescriptionsOnItemsSourceChange();
		}
		_cachedItemsSource = newValue;
		using (UpdateSelectedCells())
		{
			List<Tuple<int, int>> ranges = new List<Tuple<int, int>>();
			LocateSelectedItems(ranges);
			_selectedCells.RestoreOnlyFullRows(ranges);
		}
		if (AutoGenerateColumns)
		{
			RegenerateAutoColumns();
		}
		InternalColumns.RefreshAutoWidthColumns = true;
		InternalColumns.InvalidateColumnWidthsComputation();
		CoerceValue(CanUserAddRowsProperty);
		CoerceValue(CanUserDeleteRowsProperty);
		DataGridHelper.TransferProperty(this, CanUserSortColumnsProperty);
		ResetRowHeaderActualWidth();
		UpdateNewItemPlaceholder(isAddingNewItem: false);
		HasCellValidationError = false;
		HasRowValidationError = false;
	}

	/// <summary>Performs column auto generation and updates validation flags when items change.</summary>
	/// <param name="e">The data for the event.</param>
	protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
	{
		base.OnItemsChanged(e);
		if (e.Action == NotifyCollectionChangedAction.Add)
		{
			if (DeferAutoGeneration)
			{
				AddAutoColumns();
			}
		}
		else
		{
			if (e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Replace)
			{
				if (!HasRowValidationError && !HasCellValidationError)
				{
					return;
				}
				{
					foreach (object oldItem in e.OldItems)
					{
						if (IsAddingOrEditingRowItem(oldItem))
						{
							HasRowValidationError = false;
							HasCellValidationError = false;
							break;
						}
					}
					return;
				}
			}
			if (e.Action == NotifyCollectionChangedAction.Reset)
			{
				ResetRowHeaderActualWidth();
				HasRowValidationError = false;
				HasCellValidationError = false;
			}
		}
	}

	internal override void AdjustItemInfoOverride(NotifyCollectionChangedEventArgs e)
	{
		List<ItemInfo> list = new List<ItemInfo>();
		if (_selectionAnchor.HasValue)
		{
			list.Add(_selectionAnchor.Value.ItemInfo);
		}
		if (_editingRowInfo != null)
		{
			list.Add(_editingRowInfo);
		}
		if (CellInfoNeedsAdjusting(CurrentCell))
		{
			list.Add(CurrentCell.ItemInfo);
		}
		AdjustItemInfos(e, list);
		base.AdjustItemInfoOverride(e);
	}

	internal override void AdjustItemInfosAfterGeneratorChangeOverride()
	{
		List<ItemInfo> list = new List<ItemInfo>();
		if (_selectionAnchor.HasValue)
		{
			list.Add(_selectionAnchor.Value.ItemInfo);
		}
		if (_editingRowInfo != null)
		{
			list.Add(_editingRowInfo);
		}
		if (CellInfoNeedsAdjusting(CurrentCell))
		{
			list.Add(CurrentCell.ItemInfo);
		}
		AdjustItemInfosAfterGeneratorChange(list, claimUniqueContainer: false);
		base.AdjustItemInfosAfterGeneratorChangeOverride();
		AdjustPendingInfos();
	}

	private static bool CellInfoNeedsAdjusting(DataGridCellInfo cellInfo)
	{
		ItemInfo itemInfo = cellInfo.ItemInfo;
		if (itemInfo != null)
		{
			return itemInfo.Index != -1;
		}
		return false;
	}

	private void AdjustPendingInfos()
	{
		int count;
		if (_pendingInfos == null || _pendingInfos.Count <= 0 || (count = _columns.Count) <= 0)
		{
			return;
		}
		using (UpdateSelectedCells())
		{
			for (int num = _pendingInfos.Count - 1; num >= 0; num--)
			{
				ItemInfo itemInfo = _pendingInfos[num];
				if (itemInfo.Index >= 0)
				{
					_pendingInfos.RemoveAt(num);
					_selectedCells.AddRegion(itemInfo.Index, 0, 1, count);
				}
			}
		}
	}

	private void AddAutoColumns()
	{
		ReadOnlyCollection<ItemPropertyInfo> itemProperties = ((IItemProperties)base.Items).ItemProperties;
		if (itemProperties == null && DataItemsCount == 0)
		{
			DeferAutoGeneration = true;
		}
		else if (!_measureNeverInvoked)
		{
			GenerateColumns(itemProperties, this, null);
			DeferAutoGeneration = false;
			OnAutoGeneratedColumns(EventArgs.Empty);
		}
	}

	private void DeleteAutoColumns()
	{
		if (!DeferAutoGeneration && !_measureNeverInvoked)
		{
			for (int num = Columns.Count - 1; num >= 0; num--)
			{
				if (Columns[num].IsAutoGenerated)
				{
					Columns.RemoveAt(num);
				}
			}
		}
		else
		{
			DeferAutoGeneration = false;
		}
	}

	private void RegenerateAutoColumns()
	{
		DeleteAutoColumns();
		AddAutoColumns();
	}

	/// <summary>Generates columns for the specified properties of an object.</summary>
	/// <returns>The collection of columns for the properties of the object.</returns>
	/// <param name="itemProperties">The properties of the object to be in the columns.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="itemProperties" /> is null.</exception>
	public static Collection<DataGridColumn> GenerateColumns(IItemProperties itemProperties)
	{
		if (itemProperties == null)
		{
			throw new ArgumentNullException("itemProperties");
		}
		Collection<DataGridColumn> collection = new Collection<DataGridColumn>();
		GenerateColumns(itemProperties.ItemProperties, null, collection);
		return collection;
	}

	private static void GenerateColumns(ReadOnlyCollection<ItemPropertyInfo> itemProperties, DataGrid dataGrid, Collection<DataGridColumn> columnCollection)
	{
		if (itemProperties == null || itemProperties.Count <= 0)
		{
			return;
		}
		foreach (ItemPropertyInfo itemProperty in itemProperties)
		{
			DataGridColumn dataGridColumn = DataGridColumn.CreateDefaultColumn(itemProperty);
			if (dataGrid != null)
			{
				DataGridAutoGeneratingColumnEventArgs dataGridAutoGeneratingColumnEventArgs = new DataGridAutoGeneratingColumnEventArgs(dataGridColumn, itemProperty);
				dataGrid.OnAutoGeneratingColumn(dataGridAutoGeneratingColumnEventArgs);
				if (!dataGridAutoGeneratingColumnEventArgs.Cancel && dataGridAutoGeneratingColumnEventArgs.Column != null)
				{
					dataGridAutoGeneratingColumnEventArgs.Column.IsAutoGenerated = true;
					dataGrid.Columns.Add(dataGridAutoGeneratingColumnEventArgs.Column);
				}
			}
			else
			{
				columnCollection.Add(dataGridColumn);
			}
		}
	}

	private static void OnAutoGenerateColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		bool num = (bool)e.NewValue;
		DataGrid dataGrid = (DataGrid)d;
		if (num)
		{
			dataGrid.AddAutoColumns();
		}
		else
		{
			dataGrid.DeleteAutoColumns();
		}
	}

	private static object OnCoerceFrozenColumnCount(DependencyObject d, object baseValue)
	{
		DataGrid dataGrid = (DataGrid)d;
		if ((int)baseValue > dataGrid.Columns.Count)
		{
			return dataGrid.Columns.Count;
		}
		return baseValue;
	}

	private static void OnFrozenColumnCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.CellsPresenter | DataGridNotificationTarget.ColumnCollection | DataGridNotificationTarget.ColumnHeadersPresenter);
	}

	private static bool ValidateFrozenColumnCount(object value)
	{
		return (int)value >= 0;
	}

	public override void OnApplyTemplate()
	{
		if (InternalItemsHost != null && !IsAncestorOf(InternalItemsHost))
		{
			InternalItemsHost = null;
		}
		CleanUpInternalScrollControls();
		base.OnApplyTemplate();
	}

	private static void OnEnableRowVirtualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGrid obj = (DataGrid)d;
		obj.CoerceValue(VirtualizingPanel.IsVirtualizingProperty);
		Panel internalItemsHost = obj.InternalItemsHost;
		if (internalItemsHost != null)
		{
			internalItemsHost.InvalidateMeasure();
			internalItemsHost.InvalidateArrange();
		}
	}

	private static object OnCoerceIsVirtualizingProperty(DependencyObject d, object baseValue)
	{
		if (!DataGridHelper.IsDefaultValue(d, EnableRowVirtualizationProperty))
		{
			return d.GetValue(EnableRowVirtualizationProperty);
		}
		return baseValue;
	}

	private static void OnEnableColumnVirtualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.CellsPresenter | DataGridNotificationTarget.ColumnCollection | DataGridNotificationTarget.ColumnHeadersPresenter);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.ColumnHeaderDragStarted" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected internal virtual void OnColumnHeaderDragStarted(DragStartedEventArgs e)
	{
		if (this.ColumnHeaderDragStarted != null)
		{
			this.ColumnHeaderDragStarted(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.ColumnReordering" /> event.</summary>
	/// <param name="e">The data for the event.</param>
	protected internal virtual void OnColumnReordering(DataGridColumnReorderingEventArgs e)
	{
		if (this.ColumnReordering != null)
		{
			this.ColumnReordering(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.ColumnHeaderDragDelta" /> event. </summary>
	/// <param name="e">The data for the event.</param>
	protected internal virtual void OnColumnHeaderDragDelta(DragDeltaEventArgs e)
	{
		if (this.ColumnHeaderDragDelta != null)
		{
			this.ColumnHeaderDragDelta(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.ColumnHeaderDragCompleted" /> event. </summary>
	/// <param name="e">The data for the event.</param>
	protected internal virtual void OnColumnHeaderDragCompleted(DragCompletedEventArgs e)
	{
		if (this.ColumnHeaderDragCompleted != null)
		{
			this.ColumnHeaderDragCompleted(this, e);
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.ColumnReordered" /> event. </summary>
	/// <param name="e">The data for the event.</param>
	protected internal virtual void OnColumnReordered(DataGridColumnEventArgs e)
	{
		if (this.ColumnReordered != null)
		{
			this.ColumnReordered(this, e);
		}
	}

	private static void OnClipboardCopyModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		CommandManager.InvalidateRequerySuggested();
	}

	private static void OnCanExecuteCopy(object target, CanExecuteRoutedEventArgs args)
	{
		((DataGrid)target).OnCanExecuteCopy(args);
	}

	/// <summary>Provides handling for the <see cref="E:System.Windows.Input.CommandBinding.CanExecute" /> event associated with the <see cref="P:System.Windows.Input.ApplicationCommands.Copy" /> command.</summary>
	/// <param name="args">The data for the event.</param>
	protected virtual void OnCanExecuteCopy(CanExecuteRoutedEventArgs args)
	{
		args.CanExecute = ClipboardCopyMode != 0 && _selectedCells.Count > 0;
		args.Handled = true;
	}

	private static void OnExecutedCopy(object target, ExecutedRoutedEventArgs args)
	{
		((DataGrid)target).OnExecutedCopy(args);
	}

	/// <summary>Provides handling for the <see cref="E:System.Windows.Input.CommandBinding.Executed" /> event associated with the <see cref="P:System.Windows.Input.ApplicationCommands.Copy" /> command.</summary>
	/// <param name="args">The data for the event.</param>
	/// <exception cref="T:System.NotSupportedException">
	///   <see cref="P:System.Windows.Controls.DataGrid.ClipboardCopyMode" /> is set to <see cref="F:System.Windows.Controls.DataGridClipboardCopyMode.None" />.</exception>
	protected virtual void OnExecutedCopy(ExecutedRoutedEventArgs args)
	{
		if (ClipboardCopyMode == DataGridClipboardCopyMode.None)
		{
			throw new NotSupportedException(SR.ClipboardCopyMode_Disabled);
		}
		args.Handled = true;
		Collection<string> collection = new Collection<string>(new string[4]
		{
			DataFormats.Html,
			DataFormats.Text,
			DataFormats.UnicodeText,
			DataFormats.CommaSeparatedValue
		});
		Dictionary<string, StringBuilder> dictionary = new Dictionary<string, StringBuilder>(collection.Count);
		foreach (string item2 in collection)
		{
			dictionary[item2] = new StringBuilder();
		}
		if (_selectedCells.GetSelectionRange(out var minColumnDisplayIndex, out var maxColumnDisplayIndex, out var minRowIndex, out var maxRowIndex))
		{
			if (ClipboardCopyMode == DataGridClipboardCopyMode.IncludeHeader)
			{
				DataGridRowClipboardEventArgs dataGridRowClipboardEventArgs = new DataGridRowClipboardEventArgs(null, minColumnDisplayIndex, maxColumnDisplayIndex, isColumnHeadersRow: true);
				OnCopyingRowClipboardContent(dataGridRowClipboardEventArgs);
				foreach (string item3 in collection)
				{
					dictionary[item3].Append(dataGridRowClipboardEventArgs.FormatClipboardCellValues(item3));
				}
			}
			for (int i = minRowIndex; i <= maxRowIndex; i++)
			{
				object item = base.Items[i];
				if (!_selectedCells.Intersects(i))
				{
					continue;
				}
				DataGridRowClipboardEventArgs dataGridRowClipboardEventArgs2 = new DataGridRowClipboardEventArgs(item, minColumnDisplayIndex, maxColumnDisplayIndex, isColumnHeadersRow: false, i);
				OnCopyingRowClipboardContent(dataGridRowClipboardEventArgs2);
				foreach (string item4 in collection)
				{
					dictionary[item4].Append(dataGridRowClipboardEventArgs2.FormatClipboardCellValues(item4));
				}
			}
		}
		DataGridClipboardHelper.GetClipboardContentForHtml(dictionary[DataFormats.Html]);
		DataObject dataObject = new DataObject();
		foreach (string item5 in collection)
		{
			dataObject.SetData(item5, dictionary[item5].ToString(), autoConvert: false);
		}
		try
		{
			Clipboard.CriticalSetDataObject(dataObject, copy: true);
		}
		catch (ExternalException)
		{
		}
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGrid.CopyingRowClipboardContent" /> event. </summary>
	/// <param name="args">The data for the event.</param>
	protected virtual void OnCopyingRowClipboardContent(DataGridRowClipboardEventArgs args)
	{
		if (args.IsColumnHeadersRow)
		{
			for (int i = args.StartColumnDisplayIndex; i <= args.EndColumnDisplayIndex; i++)
			{
				DataGridColumn dataGridColumn = ColumnFromDisplayIndex(i);
				if (dataGridColumn.IsVisible)
				{
					args.ClipboardRowContent.Add(new DataGridClipboardCellContent(args.Item, dataGridColumn, dataGridColumn.Header));
				}
			}
		}
		else
		{
			int num = args.RowIndexHint;
			if (num < 0)
			{
				num = base.Items.IndexOf(args.Item);
			}
			if (_selectedCells.Intersects(num))
			{
				for (int j = args.StartColumnDisplayIndex; j <= args.EndColumnDisplayIndex; j++)
				{
					DataGridColumn dataGridColumn2 = ColumnFromDisplayIndex(j);
					if (dataGridColumn2.IsVisible)
					{
						object content = null;
						if (_selectedCells.Contains(num, j))
						{
							content = dataGridColumn2.OnCopyingCellClipboardContent(args.Item);
						}
						args.ClipboardRowContent.Add(new DataGridClipboardCellContent(args.Item, dataGridColumn2, content));
					}
				}
			}
		}
		if (this.CopyingRowClipboardContent != null)
		{
			this.CopyingRowClipboardContent(this, args);
		}
	}

	private static void CellsPanelActualWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		double value = (double)e.OldValue;
		double value2 = (double)e.NewValue;
		if (!DoubleUtil.AreClose(value, value2))
		{
			((DataGrid)d).NotifyPropertyChanged(d, e, DataGridNotificationTarget.ColumnHeadersPresenter);
		}
	}

	internal void QueueInvalidateCellsPanelHorizontalOffset()
	{
		if (!CellsPanelHorizontalOffsetComputationPending)
		{
			base.Dispatcher.BeginInvoke(new DispatcherOperationCallback(InvalidateCellsPanelHorizontalOffset), DispatcherPriority.Loaded, this);
			CellsPanelHorizontalOffsetComputationPending = true;
		}
	}

	private object InvalidateCellsPanelHorizontalOffset(object args)
	{
		if (!CellsPanelHorizontalOffsetComputationPending)
		{
			return null;
		}
		IProvideDataGridColumn anyCellOrColumnHeader = GetAnyCellOrColumnHeader();
		if (anyCellOrColumnHeader != null)
		{
			CellsPanelHorizontalOffset = DataGridHelper.GetParentCellsPanelHorizontalOffset(anyCellOrColumnHeader);
		}
		else if (!double.IsNaN(RowHeaderWidth))
		{
			CellsPanelHorizontalOffset = RowHeaderWidth;
		}
		else
		{
			CellsPanelHorizontalOffset = 0.0;
		}
		CellsPanelHorizontalOffsetComputationPending = false;
		return null;
	}

	internal IProvideDataGridColumn GetAnyCellOrColumnHeader()
	{
		if (_rowTrackingRoot != null)
		{
			for (ContainerTracking<DataGridRow> containerTracking = _rowTrackingRoot; containerTracking != null; containerTracking = containerTracking.Next)
			{
				if (containerTracking.Container.IsVisible)
				{
					DataGridCellsPresenter cellsPresenter = containerTracking.Container.CellsPresenter;
					if (cellsPresenter != null)
					{
						for (ContainerTracking<DataGridCell> containerTracking2 = cellsPresenter.CellTrackingRoot; containerTracking2 != null; containerTracking2 = containerTracking2.Next)
						{
							if (containerTracking2.Container.IsVisible)
							{
								return containerTracking2.Container;
							}
						}
					}
				}
			}
		}
		if (ColumnHeadersPresenter != null)
		{
			for (ContainerTracking<DataGridColumnHeader> containerTracking3 = ColumnHeadersPresenter.HeaderTrackingRoot; containerTracking3 != null; containerTracking3 = containerTracking3.Next)
			{
				if (containerTracking3.Container.IsVisible)
				{
					return containerTracking3.Container;
				}
			}
		}
		return null;
	}

	internal double GetViewportWidthForColumns()
	{
		if (InternalScrollHost == null)
		{
			return 0.0;
		}
		return InternalScrollHost.ViewportWidth - CellsPanelHorizontalOffset;
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		if (!base.IsEnabled)
		{
			VisualStates.GoToState(this, useTransitions, "Disabled", "Normal");
		}
		else
		{
			VisualStates.GoToState(this, useTransitions, "Normal");
		}
		base.ChangeVisualState(useTransitions);
	}
}
