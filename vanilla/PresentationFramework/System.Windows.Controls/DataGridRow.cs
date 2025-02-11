using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Represents a <see cref="T:System.Windows.Controls.DataGrid" /> row.</summary>
public class DataGridRow : Control
{
	private const byte DATAGRIDROW_stateMouseOverCode = 0;

	private const byte DATAGRIDROW_stateMouseOverEditingCode = 1;

	private const byte DATAGRIDROW_stateMouseOverEditingFocusedCode = 2;

	private const byte DATAGRIDROW_stateMouseOverSelectedCode = 3;

	private const byte DATAGRIDROW_stateMouseOverSelectedFocusedCode = 4;

	private const byte DATAGRIDROW_stateNormalCode = 5;

	private const byte DATAGRIDROW_stateNormalEditingCode = 6;

	private const byte DATAGRIDROW_stateNormalEditingFocusedCode = 7;

	private const byte DATAGRIDROW_stateSelectedCode = 8;

	private const byte DATAGRIDROW_stateSelectedFocusedCode = 9;

	private const byte DATAGRIDROW_stateNullCode = byte.MaxValue;

	private static byte[] _idealStateMapping;

	private static byte[] _fallbackStateMapping;

	private static string[] _stateNames;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.Item" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.Item" /> dependency property.</returns>
	public static readonly DependencyProperty ItemProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.ItemsPanel" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.ItemsPanel" /> dependency property.</returns>
	public static readonly DependencyProperty ItemsPanelProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.Header" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.Header" /> dependency property.</returns>
	public static readonly DependencyProperty HeaderProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.HeaderStyle" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.HeaderStyle" /> dependency property.</returns>
	public static readonly DependencyProperty HeaderStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.HeaderTemplate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.HeaderTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty HeaderTemplateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.HeaderTemplateSelector" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.HeaderTemplateSelector" /> dependency property.</returns>
	public static readonly DependencyProperty HeaderTemplateSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.ValidationErrorTemplate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.ValidationErrorTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty ValidationErrorTemplateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.DetailsTemplate" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.DetailsTemplate" /> dependency property.</returns>
	public static readonly DependencyProperty DetailsTemplateProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.DetailsTemplateSelector" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.DetailsTemplateSelector" /> dependency property.</returns>
	public static readonly DependencyProperty DetailsTemplateSelectorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.DetailsVisibility" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.DetailsVisibility" /> dependency property.</returns>
	public static readonly DependencyProperty DetailsVisibilityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.AlternationIndex" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.AlternationIndex" /> dependency property.</returns>
	public static readonly DependencyProperty AlternationIndexProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.IsSelected" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.IsSelected" /> dependency property.</returns>
	public static readonly DependencyProperty IsSelectedProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.DataGridRow.Selected" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.DataGridRow.Selected" /> routed event.</returns>
	public static readonly RoutedEvent SelectedEvent;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.DataGridRow.Unselected" /> routed event.</summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.DataGridRow.Unselected" /> routed event.</returns>
	public static readonly RoutedEvent UnselectedEvent;

	private static readonly DependencyPropertyKey IsEditingPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.IsEditing" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.IsEditing" /> dependency property.</returns>
	public static readonly DependencyProperty IsEditingProperty;

	internal static readonly DependencyPropertyKey IsNewItemPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.DataGridRow.IsNewItem" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.DataGridRow.IsNewItem" /> dependency property.</returns>
	public static readonly DependencyProperty IsNewItemProperty;

	internal bool _detailsLoaded;

	private DataGrid _owner;

	private DataGridCellsPresenter _cellsPresenter;

	private DataGridDetailsPresenter _detailsPresenter;

	private DataGridRowHeader _rowHeader;

	private ContainerTracking<DataGridRow> _tracker;

	private double _cellsPresenterResizeHeight;

	/// <summary>Gets or sets the data item that the row represents. </summary>
	/// <returns>The data item that the row represents. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public object Item
	{
		get
		{
			return GetValue(ItemProperty);
		}
		set
		{
			SetValue(ItemProperty, value);
		}
	}

	/// <summary>Gets or sets the template that defines the panel that controls the layout of cells in the row. </summary>
	/// <returns>The template that defines the panel to use for the layout of cells in the row. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public ItemsPanelTemplate ItemsPanel
	{
		get
		{
			return (ItemsPanelTemplate)GetValue(ItemsPanelProperty);
		}
		set
		{
			SetValue(ItemsPanelProperty, value);
		}
	}

	private bool IsDataGridKeyboardFocusWithin => DataGridOwner?.IsKeyboardFocusWithin ?? false;

	/// <summary>Gets or sets an object that represents the row header contents. </summary>
	/// <returns>The row header contents. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
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

	/// <summary>Gets or sets the style that is used when rendering the row header. </summary>
	/// <returns>The style that is used when rendering the row header. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
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

	/// <summary>Gets or sets the template that is used to display the row header. </summary>
	/// <returns>The template that is used to display the row header or null to use the <see cref="P:System.Windows.Controls.DataGrid.RowHeaderTemplate" /> setting. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
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

	/// <summary>Gets or sets a template selector that provides custom logic for choosing a row header template. </summary>
	/// <returns>A template selector for choosing the row header template. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
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

	/// <summary>Gets or sets the template that is used to visually indicate an error in row validation. </summary>
	/// <returns>The template that is used to visually indicate an error in row validation, or null to use the <see cref="P:System.Windows.Controls.DataGrid.RowValidationErrorTemplate" /> setting. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public ControlTemplate ValidationErrorTemplate
	{
		get
		{
			return (ControlTemplate)GetValue(ValidationErrorTemplateProperty);
		}
		set
		{
			SetValue(ValidationErrorTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets the template that is used to display the details section of the row. </summary>
	/// <returns>The template that is used to display the row details section or null to use the <see cref="P:System.Windows.Controls.DataGrid.RowDetailsTemplate" /> setting. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataTemplate DetailsTemplate
	{
		get
		{
			return (DataTemplate)GetValue(DetailsTemplateProperty);
		}
		set
		{
			SetValue(DetailsTemplateProperty, value);
		}
	}

	/// <summary>Gets or sets a template selector that provides custom logic for choosing a row details template. </summary>
	/// <returns>A template selector for choosing the row details template. The registered default is null. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public DataTemplateSelector DetailsTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(DetailsTemplateSelectorProperty);
		}
		set
		{
			SetValue(DetailsTemplateSelectorProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates when the details section of the row is displayed. </summary>
	/// <returns>A value that specifies the visibility of the row details. The registered default is <see cref="F:System.Windows.Visibility.Collapsed" />. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public Visibility DetailsVisibility
	{
		get
		{
			return (Visibility)GetValue(DetailsVisibilityProperty);
		}
		set
		{
			SetValue(DetailsVisibilityProperty, value);
		}
	}

	internal bool DetailsLoaded
	{
		get
		{
			return _detailsLoaded;
		}
		set
		{
			_detailsLoaded = value;
		}
	}

	internal ContainerTracking<DataGridRow> Tracker => _tracker;

	internal DataGridCellsPresenter CellsPresenter
	{
		get
		{
			return _cellsPresenter;
		}
		set
		{
			_cellsPresenter = value;
		}
	}

	internal DataGridDetailsPresenter DetailsPresenter
	{
		get
		{
			return _detailsPresenter;
		}
		set
		{
			_detailsPresenter = value;
		}
	}

	internal DataGridRowHeader RowHeader
	{
		get
		{
			return _rowHeader;
		}
		set
		{
			_rowHeader = value;
		}
	}

	/// <summary>Gets the index of the row within a set of alternating rows.</summary>
	/// <returns>The index of the row within a set of alternating rows. The registered default is 0. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public int AlternationIndex => (int)GetValue(AlternationIndexProperty);

	/// <summary>Gets or sets a value that indicates whether the row is selected. </summary>
	/// <returns>true if the row is selected; otherwise, false. The registered default is false. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
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

	private bool IsSelectable
	{
		get
		{
			DataGrid dataGridOwner = DataGridOwner;
			if (dataGridOwner != null)
			{
				DataGridSelectionUnit selectionUnit = dataGridOwner.SelectionUnit;
				if (selectionUnit != DataGridSelectionUnit.FullRow)
				{
					return selectionUnit == DataGridSelectionUnit.CellOrRowHeader;
				}
				return true;
			}
			return true;
		}
	}

	/// <summary>Gets a value that indicates whether the row is in editing mode.</summary>
	/// <returns>true if the row is in editing mode; otherwise, false. The registered default is false. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	public bool IsEditing
	{
		get
		{
			return (bool)GetValue(IsEditingProperty);
		}
		internal set
		{
			SetValue(IsEditingPropertyKey, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.DataGridRow" /> is a placeholder for a new item or for an item that has not been committed.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.DataGridRow" /> is a placeholder for a new item or for an item that has not been committed; otherwise, false.The registered default is false. For more information about what can influence the value, see Dependency Property Value Precedence.</returns>
	public bool IsNewItem
	{
		get
		{
			return (bool)GetValue(IsNewItemProperty);
		}
		internal set
		{
			SetValue(IsNewItemPropertyKey, value);
		}
	}

	internal DataGrid DataGridOwner => _owner;

	internal bool DetailsPresenterDrawsGridLines
	{
		get
		{
			if (_detailsPresenter != null)
			{
				return _detailsPresenter.Visibility == Visibility.Visible;
			}
			return false;
		}
	}

	/// <summary>Occurs when the row is selected. </summary>
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

	/// <summary>Occurs when the row selection is cleared.</summary>
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

	static DataGridRow()
	{
		_idealStateMapping = new byte[16]
		{
			5, 5, 0, 0, 255, 255, 255, 255, 8, 9,
			3, 4, 6, 7, 1, 2
		};
		_fallbackStateMapping = new byte[10] { 5, 2, 7, 4, 9, 255, 7, 9, 9, 5 };
		_stateNames = new string[10] { "MouseOver", "MouseOver_Unfocused_Editing", "MouseOver_Editing", "MouseOver_Unfocused_Selected", "MouseOver_Selected", "Normal", "Unfocused_Editing", "Normal_Editing", "Unfocused_Selected", "Normal_Selected" };
		ItemProperty = DependencyProperty.Register("Item", typeof(object), typeof(DataGridRow), new FrameworkPropertyMetadata(null, OnNotifyRowPropertyChanged));
		ItemsPanelProperty = ItemsControl.ItemsPanelProperty.AddOwner(typeof(DataGridRow));
		HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(DataGridRow), new FrameworkPropertyMetadata(null, OnNotifyRowAndRowHeaderPropertyChanged));
		HeaderStyleProperty = DependencyProperty.Register("HeaderStyle", typeof(Style), typeof(DataGridRow), new FrameworkPropertyMetadata(null, OnNotifyRowAndRowHeaderPropertyChanged, OnCoerceHeaderStyle));
		HeaderTemplateProperty = DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(DataGridRow), new FrameworkPropertyMetadata(null, OnNotifyRowAndRowHeaderPropertyChanged, OnCoerceHeaderTemplate));
		HeaderTemplateSelectorProperty = DependencyProperty.Register("HeaderTemplateSelector", typeof(DataTemplateSelector), typeof(DataGridRow), new FrameworkPropertyMetadata(null, OnNotifyRowAndRowHeaderPropertyChanged, OnCoerceHeaderTemplateSelector));
		ValidationErrorTemplateProperty = DependencyProperty.Register("ValidationErrorTemplate", typeof(ControlTemplate), typeof(DataGridRow), new FrameworkPropertyMetadata(null, OnNotifyRowPropertyChanged, OnCoerceValidationErrorTemplate));
		DetailsTemplateProperty = DependencyProperty.Register("DetailsTemplate", typeof(DataTemplate), typeof(DataGridRow), new FrameworkPropertyMetadata(null, OnNotifyDetailsTemplatePropertyChanged, OnCoerceDetailsTemplate));
		DetailsTemplateSelectorProperty = DependencyProperty.Register("DetailsTemplateSelector", typeof(DataTemplateSelector), typeof(DataGridRow), new FrameworkPropertyMetadata(null, OnNotifyDetailsTemplatePropertyChanged, OnCoerceDetailsTemplateSelector));
		DetailsVisibilityProperty = DependencyProperty.Register("DetailsVisibility", typeof(Visibility), typeof(DataGridRow), new FrameworkPropertyMetadata(Visibility.Collapsed, OnNotifyDetailsVisibilityChanged, OnCoerceDetailsVisibility));
		AlternationIndexProperty = ItemsControl.AlternationIndexProperty.AddOwner(typeof(DataGridRow));
		IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(typeof(DataGridRow), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnIsSelectedChanged));
		SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(DataGridRow));
		UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(DataGridRow));
		IsEditingPropertyKey = DependencyProperty.RegisterReadOnly("IsEditing", typeof(bool), typeof(DataGridRow), new FrameworkPropertyMetadata(false, OnNotifyRowAndRowHeaderPropertyChanged));
		IsEditingProperty = IsEditingPropertyKey.DependencyProperty;
		IsNewItemPropertyKey = DependencyProperty.RegisterReadOnly("IsNewItem", typeof(bool), typeof(DataGridRow), new FrameworkPropertyMetadata(false));
		IsNewItemProperty = IsNewItemPropertyKey.DependencyProperty;
		UIElement.VisibilityProperty.OverrideMetadata(typeof(DataGridRow), new FrameworkPropertyMetadata(null, OnCoerceVisibility));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridRow), new FrameworkPropertyMetadata(typeof(DataGridRow)));
		ItemsPanelProperty.OverrideMetadata(typeof(DataGridRow), new FrameworkPropertyMetadata(new ItemsPanelTemplate(new FrameworkElementFactory(typeof(DataGridCellsPanel)))));
		UIElement.FocusableProperty.OverrideMetadata(typeof(DataGridRow), new FrameworkPropertyMetadata(false));
		Control.BackgroundProperty.OverrideMetadata(typeof(DataGridRow), new FrameworkPropertyMetadata(null, OnNotifyRowPropertyChanged, OnCoerceBackground));
		FrameworkElement.BindingGroupProperty.OverrideMetadata(typeof(DataGridRow), new FrameworkPropertyMetadata(OnNotifyRowPropertyChanged));
		UIElement.SnapsToDevicePixelsProperty.OverrideMetadata(typeof(DataGridRow), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange));
		UIElement.IsMouseOverPropertyKey.OverrideMetadata(typeof(DataGridRow), new UIPropertyMetadata(OnNotifyRowAndRowHeaderPropertyChanged));
		VirtualizingPanel.ShouldCacheContainerSizeProperty.OverrideMetadata(typeof(DataGridRow), new FrameworkPropertyMetadata(null, OnCoerceShouldCacheContainerSize));
		AutomationProperties.IsOffscreenBehaviorProperty.OverrideMetadata(typeof(DataGridRow), new FrameworkPropertyMetadata(IsOffscreenBehavior.FromClip));
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridRow" /> class. </summary>
	public DataGridRow()
	{
		_tracker = new ContainerTracking<DataGridRow>(this);
	}

	/// <summary>Updates the displayed cells when the <see cref="P:System.Windows.Controls.DataGridRow.Item" /> property value has changed. </summary>
	/// <param name="oldItem">The previous value of the <see cref="P:System.Windows.Controls.DataGridRow.Item" /> property.</param>
	/// <param name="newItem">The new value of the <see cref="P:System.Windows.Controls.DataGridRow.Item" /> property.</param>
	protected virtual void OnItemChanged(object oldItem, object newItem)
	{
		DataGridCellsPresenter cellsPresenter = CellsPresenter;
		if (cellsPresenter != null)
		{
			cellsPresenter.Item = newItem;
		}
	}

	/// <param name="oldTemplate">The old template.</param>
	/// <param name="newTemplate">The new template.</param>
	protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
	{
		base.OnTemplateChanged(oldTemplate, newTemplate);
		CellsPresenter = null;
		DetailsPresenter = null;
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		byte b = 0;
		if (IsSelected || IsEditing)
		{
			b += 8;
		}
		if (IsEditing)
		{
			b += 4;
		}
		if (base.IsMouseOver)
		{
			b += 2;
		}
		if (IsDataGridKeyboardFocusWithin)
		{
			b++;
		}
		for (byte b2 = _idealStateMapping[b]; b2 != byte.MaxValue; b2 = _fallbackStateMapping[b2])
		{
			string stateName = ((b2 != 5) ? _stateNames[b2] : ((AlternationIndex % 2 != 1) ? "Normal" : "Normal_AlternatingRow"));
			if (VisualStateManager.GoToState(this, stateName, useTransitions))
			{
				break;
			}
		}
		base.ChangeVisualState(useTransitions);
	}

	/// <summary>Called when the value of the <see cref="P:System.Windows.Controls.DataGridRow.Header" /> property has changed.</summary>
	/// <param name="oldHeader">The previous value of the <see cref="P:System.Windows.Controls.DataGridRow.Header" /> property.</param>
	/// <param name="newHeader">The new value of the <see cref="P:System.Windows.Controls.DataGridRow.Header" /> property. </param>
	protected virtual void OnHeaderChanged(object oldHeader, object newHeader)
	{
	}

	/// <summary>Invoked whenever the effective value of any dependency property on this <see cref="T:System.Windows.Controls.DataGridRow" /> has been updated. </summary>
	/// <param name="e">The event data that describes the property that changed, as well as old and new values.</param>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == AlternationIndexProperty)
		{
			NotifyPropertyChanged(this, e, DataGridNotificationTarget.Rows);
		}
	}

	internal void PrepareRow(object item, DataGrid owningDataGrid)
	{
		bool num = _owner != owningDataGrid;
		bool forcePrepareCells = false;
		_owner = owningDataGrid;
		if (this != item)
		{
			if (Item != item)
			{
				Item = item;
			}
			else
			{
				forcePrepareCells = true;
			}
		}
		if (IsEditing)
		{
			IsEditing = false;
		}
		if (num)
		{
			SyncProperties(forcePrepareCells);
		}
		CoerceValue(VirtualizingPanel.ShouldCacheContainerSizeProperty);
		base.Dispatcher.BeginInvoke(new DispatcherOperationCallback(DelayedValidateWithoutUpdate), DispatcherPriority.DataBind, base.BindingGroup);
	}

	internal void ClearRow(DataGrid owningDataGrid)
	{
		DataGridCellsPresenter cellsPresenter = CellsPresenter;
		if (cellsPresenter != null)
		{
			PersistAttachedItemValue(cellsPresenter, FrameworkElement.HeightProperty);
		}
		PersistAttachedItemValue(this, DetailsVisibilityProperty);
		Item = BindingExpressionBase.DisconnectedItem;
		DataGridDetailsPresenter detailsPresenter = DetailsPresenter;
		if (detailsPresenter != null)
		{
			detailsPresenter.Content = BindingExpressionBase.DisconnectedItem;
		}
		_owner = null;
	}

	private void PersistAttachedItemValue(DependencyObject objectWithProperty, DependencyProperty property)
	{
		if (DependencyPropertyHelper.GetValueSource(objectWithProperty, property).BaseValueSource == BaseValueSource.Local)
		{
			_owner.ItemAttachedStorage.SetValue(Item, property, objectWithProperty.GetValue(property));
			objectWithProperty.ClearValue(property);
		}
	}

	private void RestoreAttachedItemValue(DependencyObject objectWithProperty, DependencyProperty property)
	{
		if (_owner.ItemAttachedStorage.TryGetValue(Item, property, out var value))
		{
			objectWithProperty.SetValue(property, value);
		}
	}

	internal void OnRowResizeStarted()
	{
		DataGridCellsPresenter cellsPresenter = CellsPresenter;
		if (cellsPresenter != null)
		{
			_cellsPresenterResizeHeight = cellsPresenter.Height;
		}
	}

	internal void OnRowResize(double changeAmount)
	{
		DataGridCellsPresenter cellsPresenter = CellsPresenter;
		if (cellsPresenter != null)
		{
			double num = cellsPresenter.ActualHeight + changeAmount;
			double num2 = Math.Max(RowHeader.DesiredSize.Height, base.MinHeight);
			if (DoubleUtil.LessThan(num, num2))
			{
				num = num2;
			}
			double maxHeight = base.MaxHeight;
			if (DoubleUtil.GreaterThan(num, maxHeight))
			{
				num = maxHeight;
			}
			cellsPresenter.Height = num;
		}
	}

	internal void OnRowResizeCompleted(bool canceled)
	{
		DataGridCellsPresenter cellsPresenter = CellsPresenter;
		if (cellsPresenter != null && canceled)
		{
			cellsPresenter.Height = _cellsPresenterResizeHeight;
		}
	}

	internal void OnRowResizeReset()
	{
		DataGridCellsPresenter cellsPresenter = CellsPresenter;
		if (cellsPresenter != null)
		{
			cellsPresenter.ClearValue(FrameworkElement.HeightProperty);
			if (_owner != null)
			{
				_owner.ItemAttachedStorage.ClearValue(Item, FrameworkElement.HeightProperty);
			}
		}
	}

	/// <summary>Called to update the displayed cells when the <see cref="P:System.Windows.Controls.DataGrid.Columns" /> collection has changed. </summary>
	/// <param name="columns">The <see cref="P:System.Windows.Controls.DataGrid.Columns" /> collection.</param>
	/// <param name="e">The event data from the <see cref="E:System.Collections.ObjectModel.ObservableCollection`1.CollectionChanged" /> event of the <see cref="P:System.Windows.Controls.DataGrid.Columns" /> collection.</param>
	protected internal virtual void OnColumnsChanged(ObservableCollection<DataGridColumn> columns, NotifyCollectionChangedEventArgs e)
	{
		CellsPresenter?.OnColumnsChanged(columns, e);
	}

	private static object OnCoerceHeaderStyle(DependencyObject d, object baseValue)
	{
		DataGridRow dataGridRow = (DataGridRow)d;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridRow, baseValue, HeaderStyleProperty, dataGridRow.DataGridOwner, DataGrid.RowHeaderStyleProperty);
	}

	private static object OnCoerceHeaderTemplate(DependencyObject d, object baseValue)
	{
		DataGridRow dataGridRow = (DataGridRow)d;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridRow, baseValue, HeaderTemplateProperty, dataGridRow.DataGridOwner, DataGrid.RowHeaderTemplateProperty);
	}

	private static object OnCoerceHeaderTemplateSelector(DependencyObject d, object baseValue)
	{
		DataGridRow dataGridRow = (DataGridRow)d;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridRow, baseValue, HeaderTemplateSelectorProperty, dataGridRow.DataGridOwner, DataGrid.RowHeaderTemplateSelectorProperty);
	}

	private static object OnCoerceBackground(DependencyObject d, object baseValue)
	{
		DataGridRow dataGridRow = (DataGridRow)d;
		object result = baseValue;
		switch (dataGridRow.AlternationIndex)
		{
		case 0:
			result = DataGridHelper.GetCoercedTransferPropertyValue(dataGridRow, baseValue, Control.BackgroundProperty, dataGridRow.DataGridOwner, DataGrid.RowBackgroundProperty);
			break;
		case 1:
			result = DataGridHelper.GetCoercedTransferPropertyValue(dataGridRow, baseValue, Control.BackgroundProperty, dataGridRow.DataGridOwner, DataGrid.AlternatingRowBackgroundProperty);
			break;
		}
		return result;
	}

	private static object OnCoerceValidationErrorTemplate(DependencyObject d, object baseValue)
	{
		DataGridRow dataGridRow = (DataGridRow)d;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridRow, baseValue, ValidationErrorTemplateProperty, dataGridRow.DataGridOwner, DataGrid.RowValidationErrorTemplateProperty);
	}

	private static object OnCoerceDetailsTemplate(DependencyObject d, object baseValue)
	{
		DataGridRow dataGridRow = (DataGridRow)d;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridRow, baseValue, DetailsTemplateProperty, dataGridRow.DataGridOwner, DataGrid.RowDetailsTemplateProperty);
	}

	private static object OnCoerceDetailsTemplateSelector(DependencyObject d, object baseValue)
	{
		DataGridRow dataGridRow = (DataGridRow)d;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridRow, baseValue, DetailsTemplateSelectorProperty, dataGridRow.DataGridOwner, DataGrid.RowDetailsTemplateSelectorProperty);
	}

	private static object OnCoerceDetailsVisibility(DependencyObject d, object baseValue)
	{
		DataGridRow dataGridRow = (DataGridRow)d;
		object obj = DataGridHelper.GetCoercedTransferPropertyValue(dataGridRow, baseValue, DetailsVisibilityProperty, dataGridRow.DataGridOwner, DataGrid.RowDetailsVisibilityModeProperty);
		if (obj is DataGridRowDetailsVisibilityMode dataGridRowDetailsVisibilityMode)
		{
			bool flag = dataGridRow.DetailsTemplate != null || dataGridRow.DetailsTemplateSelector != null;
			bool flag2 = dataGridRow.Item != CollectionView.NewItemPlaceholder;
			obj = dataGridRowDetailsVisibilityMode switch
			{
				DataGridRowDetailsVisibilityMode.Collapsed => Visibility.Collapsed, 
				DataGridRowDetailsVisibilityMode.Visible => (!(flag && flag2)) ? Visibility.Collapsed : Visibility.Visible, 
				DataGridRowDetailsVisibilityMode.VisibleWhenSelected => (!(dataGridRow.IsSelected && flag && flag2)) ? Visibility.Collapsed : Visibility.Visible, 
				_ => Visibility.Collapsed, 
			};
		}
		return obj;
	}

	private static object OnCoerceVisibility(DependencyObject d, object baseValue)
	{
		DataGridRow obj = (DataGridRow)d;
		DataGrid dataGridOwner = obj.DataGridOwner;
		if (obj.Item == CollectionView.NewItemPlaceholder && dataGridOwner != null)
		{
			return dataGridOwner.PlaceholderVisibility;
		}
		return baseValue;
	}

	private static object OnCoerceShouldCacheContainerSize(DependencyObject d, object baseValue)
	{
		if (((DataGridRow)d).Item == CollectionView.NewItemPlaceholder)
		{
			return false;
		}
		return baseValue;
	}

	private static void OnNotifyRowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as DataGridRow).NotifyPropertyChanged(d, e, DataGridNotificationTarget.Rows);
	}

	private static void OnNotifyRowAndRowHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as DataGridRow).NotifyPropertyChanged(d, e, DataGridNotificationTarget.RowHeaders | DataGridNotificationTarget.Rows);
	}

	private static void OnNotifyDetailsTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGridRow dataGridRow = (DataGridRow)d;
		dataGridRow.NotifyPropertyChanged(dataGridRow, e, DataGridNotificationTarget.DetailsPresenter | DataGridNotificationTarget.Rows);
		if (dataGridRow.DetailsLoaded && d.GetValue(e.Property) == e.NewValue)
		{
			if (dataGridRow.DataGridOwner != null)
			{
				dataGridRow.DataGridOwner.OnUnloadingRowDetailsWrapper(dataGridRow);
			}
			if (e.NewValue != null)
			{
				Dispatcher.CurrentDispatcher.BeginInvoke(new DispatcherOperationCallback(DataGrid.DelayedOnLoadingRowDetails), DispatcherPriority.Loaded, dataGridRow);
			}
		}
	}

	private static void OnNotifyDetailsVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		DataGridRow dataGridRow = (DataGridRow)d;
		Dispatcher.CurrentDispatcher.BeginInvoke(new DispatcherOperationCallback(DelayedRowDetailsVisibilityChanged), DispatcherPriority.Loaded, dataGridRow);
		dataGridRow.NotifyPropertyChanged(d, e, DataGridNotificationTarget.DetailsPresenter | DataGridNotificationTarget.Rows);
	}

	private static object DelayedRowDetailsVisibilityChanged(object arg)
	{
		DataGridRow dataGridRow = (DataGridRow)arg;
		DataGrid dataGridOwner = dataGridRow.DataGridOwner;
		FrameworkElement detailsElement = ((dataGridRow.DetailsPresenter != null) ? dataGridRow.DetailsPresenter.DetailsElement : null);
		if (dataGridOwner != null)
		{
			DataGridRowDetailsEventArgs e = new DataGridRowDetailsEventArgs(dataGridRow, detailsElement);
			dataGridOwner.OnRowDetailsVisibilityChanged(e);
		}
		return null;
	}

	internal void NotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e, DataGridNotificationTarget target)
	{
		NotifyPropertyChanged(d, string.Empty, e, target);
	}

	internal void NotifyPropertyChanged(DependencyObject d, string propertyName, DependencyPropertyChangedEventArgs e, DataGridNotificationTarget target)
	{
		if (DataGridHelper.ShouldNotifyRows(target))
		{
			if (e.Property == DataGrid.RowBackgroundProperty || e.Property == DataGrid.AlternatingRowBackgroundProperty || e.Property == Control.BackgroundProperty || e.Property == AlternationIndexProperty)
			{
				DataGridHelper.TransferProperty(this, Control.BackgroundProperty);
			}
			else if (e.Property == DataGrid.RowHeaderStyleProperty || e.Property == HeaderStyleProperty)
			{
				DataGridHelper.TransferProperty(this, HeaderStyleProperty);
			}
			else if (e.Property == DataGrid.RowHeaderTemplateProperty || e.Property == HeaderTemplateProperty)
			{
				DataGridHelper.TransferProperty(this, HeaderTemplateProperty);
			}
			else if (e.Property == DataGrid.RowHeaderTemplateSelectorProperty || e.Property == HeaderTemplateSelectorProperty)
			{
				DataGridHelper.TransferProperty(this, HeaderTemplateSelectorProperty);
			}
			else if (e.Property == DataGrid.RowValidationErrorTemplateProperty || e.Property == ValidationErrorTemplateProperty)
			{
				DataGridHelper.TransferProperty(this, ValidationErrorTemplateProperty);
			}
			else if (e.Property == DataGrid.RowDetailsTemplateProperty || e.Property == DetailsTemplateProperty)
			{
				DataGridHelper.TransferProperty(this, DetailsTemplateProperty);
				DataGridHelper.TransferProperty(this, DetailsVisibilityProperty);
			}
			else if (e.Property == DataGrid.RowDetailsTemplateSelectorProperty || e.Property == DetailsTemplateSelectorProperty)
			{
				DataGridHelper.TransferProperty(this, DetailsTemplateSelectorProperty);
				DataGridHelper.TransferProperty(this, DetailsVisibilityProperty);
			}
			else if (e.Property == DataGrid.RowDetailsVisibilityModeProperty || e.Property == DetailsVisibilityProperty || e.Property == IsSelectedProperty)
			{
				DataGridHelper.TransferProperty(this, DetailsVisibilityProperty);
			}
			else if (e.Property == ItemProperty)
			{
				OnItemChanged(e.OldValue, e.NewValue);
			}
			else if (e.Property == HeaderProperty)
			{
				OnHeaderChanged(e.OldValue, e.NewValue);
			}
			else if (e.Property == FrameworkElement.BindingGroupProperty)
			{
				base.Dispatcher.BeginInvoke(new DispatcherOperationCallback(DelayedValidateWithoutUpdate), DispatcherPriority.DataBind, e.NewValue);
			}
			else if (e.Property == IsEditingProperty || e.Property == UIElement.IsMouseOverProperty || e.Property == UIElement.IsKeyboardFocusWithinProperty)
			{
				UpdateVisualState();
			}
		}
		if (DataGridHelper.ShouldNotifyDetailsPresenter(target) && DetailsPresenter != null)
		{
			DetailsPresenter.NotifyPropertyChanged(d, e);
		}
		if (DataGridHelper.ShouldNotifyCellsPresenter(target) || DataGridHelper.ShouldNotifyCells(target) || DataGridHelper.ShouldRefreshCellContent(target))
		{
			CellsPresenter?.NotifyPropertyChanged(d, propertyName, e, target);
		}
		if (DataGridHelper.ShouldNotifyRowHeaders(target) && RowHeader != null)
		{
			RowHeader.NotifyPropertyChanged(d, e);
		}
	}

	private object DelayedValidateWithoutUpdate(object arg)
	{
		BindingGroup bindingGroup = (BindingGroup)arg;
		if (bindingGroup != null && bindingGroup.Items.Count > 0)
		{
			bindingGroup.ValidateWithoutUpdate();
		}
		return null;
	}

	private void SyncProperties(bool forcePrepareCells)
	{
		DataGridHelper.TransferProperty(this, Control.BackgroundProperty);
		DataGridHelper.TransferProperty(this, HeaderStyleProperty);
		DataGridHelper.TransferProperty(this, HeaderTemplateProperty);
		DataGridHelper.TransferProperty(this, HeaderTemplateSelectorProperty);
		DataGridHelper.TransferProperty(this, ValidationErrorTemplateProperty);
		DataGridHelper.TransferProperty(this, DetailsTemplateProperty);
		DataGridHelper.TransferProperty(this, DetailsTemplateSelectorProperty);
		DataGridHelper.TransferProperty(this, DetailsVisibilityProperty);
		CoerceValue(UIElement.VisibilityProperty);
		RestoreAttachedItemValue(this, DetailsVisibilityProperty);
		DataGridCellsPresenter cellsPresenter = CellsPresenter;
		if (cellsPresenter != null)
		{
			cellsPresenter.SyncProperties(forcePrepareCells);
			RestoreAttachedItemValue(cellsPresenter, FrameworkElement.HeightProperty);
		}
		if (DetailsPresenter != null)
		{
			DetailsPresenter.SyncProperties();
		}
		if (RowHeader != null)
		{
			RowHeader.SyncProperties();
		}
	}

	private static void OnIsSelectedChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		DataGridRow dataGridRow = (DataGridRow)sender;
		bool flag = (bool)e.NewValue;
		if (flag && !dataGridRow.IsSelectable)
		{
			throw new InvalidOperationException(SR.DataGridRow_CannotSelectRowWhenCells);
		}
		DataGrid dataGridOwner = dataGridRow.DataGridOwner;
		if (dataGridOwner != null && dataGridRow.DataContext != null && UIElementAutomationPeer.FromElement(dataGridOwner) is DataGridAutomationPeer dataGridAutomationPeer && dataGridAutomationPeer.FindOrCreateItemAutomationPeer(dataGridRow.DataContext) is DataGridItemAutomationPeer dataGridItemAutomationPeer)
		{
			dataGridItemAutomationPeer.RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, (bool)e.OldValue, flag);
		}
		dataGridRow.NotifyPropertyChanged(dataGridRow, e, DataGridNotificationTarget.RowHeaders | DataGridNotificationTarget.Rows);
		dataGridRow.RaiseSelectionChangedEvent(flag);
		dataGridRow.UpdateVisualState();
		dataGridRow.NotifyPropertyChanged(dataGridRow, e, DataGridNotificationTarget.RowHeaders | DataGridNotificationTarget.Rows);
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

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGridRow.Selected" /> event when the <see cref="P:System.Windows.Controls.DataGridRow.IsSelected" /> property value changes to true. </summary>
	/// <param name="e">The event data, which is empty when this method is called by the <see cref="T:System.Windows.Controls.DataGridRow" />.</param>
	protected virtual void OnSelected(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.DataGridRow.Unselected" /> event when the <see cref="P:System.Windows.Controls.DataGridRow.IsSelected" /> property value changes to false. </summary>
	/// <param name="e">The event data, which is empty when this method is called by the <see cref="T:System.Windows.Controls.DataGridRow" />.</param>
	protected virtual void OnUnselected(RoutedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Returns a new <see cref="T:System.Windows.Automation.Peers.DataGridRowAutomationPeer" /> for this row.</summary>
	/// <returns>A new automation peer for this row.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DataGridRowAutomationPeer(this);
	}

	internal void ScrollCellIntoView(int index)
	{
		CellsPresenter?.ScrollCellIntoView(index);
	}

	/// <summary>Arranges the content of the row.</summary>
	/// <returns>The actual area used by the row.</returns>
	/// <param name="arrangeBounds">The area that is available for the row. </param>
	protected override Size ArrangeOverride(Size arrangeBounds)
	{
		DataGridOwner?.QueueInvalidateCellsPanelHorizontalOffset();
		return base.ArrangeOverride(arrangeBounds);
	}

	/// <summary>Returns the index of the row's data item within the <see cref="P:System.Windows.Controls.ItemsControl.Items" /> collection of the <see cref="T:System.Windows.Controls.DataGrid" />. </summary>
	/// <returns>The index of the row's data item, or -1 if the item was not found. </returns>
	public int GetIndex()
	{
		return DataGridOwner?.ItemContainerGenerator.IndexFromContainer(this) ?? (-1);
	}

	/// <summary>Returns the <see cref="T:System.Windows.Controls.DataGridRow" /> that contains the specified element. </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.DataGridRow" /> that contains the specified element. </returns>
	/// <param name="element">An element contained in a row to be found. </param>
	public static DataGridRow GetRowContainingElement(FrameworkElement element)
	{
		return DataGridHelper.FindVisualParent<DataGridRow>(element);
	}

	internal DataGridCell TryGetCell(int index)
	{
		DataGridCellsPresenter cellsPresenter = CellsPresenter;
		if (cellsPresenter != null)
		{
			return cellsPresenter.ItemContainerGenerator.ContainerFromIndex(index) as DataGridCell;
		}
		return null;
	}
}
