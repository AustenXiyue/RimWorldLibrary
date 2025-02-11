using System.ComponentModel;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Input;
using System.Windows.Media;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents an individual <see cref="T:System.Windows.Controls.DataGrid" /> row header. </summary>
[TemplatePart(Name = "PART_TopHeaderGripper", Type = typeof(Thumb))]
[TemplatePart(Name = "PART_BottomHeaderGripper", Type = typeof(Thumb))]
public class DataGridRowHeader : ButtonBase
{
	private const byte DATAGRIDROWHEADER_stateMouseOverCode = 0;

	private const byte DATAGRIDROWHEADER_stateMouseOverCurrentRowCode = 1;

	private const byte DATAGRIDROWHEADER_stateMouseOverEditingRowCode = 2;

	private const byte DATAGRIDROWHEADER_stateMouseOverEditingRowFocusedCode = 3;

	private const byte DATAGRIDROWHEADER_stateMouseOverSelectedCode = 4;

	private const byte DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowCode = 5;

	private const byte DATAGRIDROWHEADER_stateMouseOverSelectedCurrentRowFocusedCode = 6;

	private const byte DATAGRIDROWHEADER_stateMouseOverSelectedFocusedCode = 7;

	private const byte DATAGRIDROWHEADER_stateNormalCode = 8;

	private const byte DATAGRIDROWHEADER_stateNormalCurrentRowCode = 9;

	private const byte DATAGRIDROWHEADER_stateNormalEditingRowCode = 10;

	private const byte DATAGRIDROWHEADER_stateNormalEditingRowFocusedCode = 11;

	private const byte DATAGRIDROWHEADER_stateSelectedCode = 12;

	private const byte DATAGRIDROWHEADER_stateSelectedCurrentRowCode = 13;

	private const byte DATAGRIDROWHEADER_stateSelectedCurrentRowFocusedCode = 14;

	private const byte DATAGRIDROWHEADER_stateSelectedFocusedCode = 15;

	private const byte DATAGRIDROWHEADER_stateNullCode = byte.MaxValue;

	private static byte[] _fallbackStateMapping;

	private static byte[] _idealStateMapping;

	private static string[] _stateNames;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DataGridRowHeader.SeparatorBrush" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DataGridRowHeader.SeparatorBrush" /> dependency property.</returns>
	public static readonly DependencyProperty SeparatorBrushProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DataGridRowHeader.SeparatorVisibility" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DataGridRowHeader.SeparatorVisibility" /> dependency property.</returns>
	public static readonly DependencyProperty SeparatorVisibilityProperty;

	private static readonly DependencyPropertyKey IsRowSelectedPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.DataGridRowHeader.IsRowSelected" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.DataGridRowHeader.IsRowSelected" /> dependency property.</returns>
	public static readonly DependencyProperty IsRowSelectedProperty;

	private Thumb _topGripper;

	private Thumb _bottomGripper;

	private const string TopHeaderGripperTemplateName = "PART_TopHeaderGripper";

	private const string BottomHeaderGripperTemplateName = "PART_BottomHeaderGripper";

	/// <summary>Gets or sets the <see cref="T:System.Windows.Media.Brush" /> used to paint the row header separator lines. </summary>
	/// <returns>The brush used to paint row header separator lines. </returns>
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

	/// <summary>Gets or sets the user interface (UI) visibility of the row header separator lines. </summary>
	/// <returns>The UI visibility of the row header separator lines. The default is <see cref="F:System.Windows.Visibility.Visible" />.</returns>
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

	private bool IsRowCurrent
	{
		get
		{
			DataGridRow parentRow = ParentRow;
			if (parentRow != null)
			{
				DataGrid dataGridOwner = parentRow.DataGridOwner;
				if (dataGridOwner != null)
				{
					return dataGridOwner.IsCurrent(parentRow);
				}
			}
			return false;
		}
	}

	private bool IsRowEditing => ParentRow?.IsEditing ?? false;

	private bool IsRowMouseOver => ParentRow?.IsMouseOver ?? false;

	private bool IsDataGridKeyboardFocusWithin
	{
		get
		{
			DataGridRow parentRow = ParentRow;
			if (parentRow != null)
			{
				DataGrid dataGridOwner = parentRow.DataGridOwner;
				if (dataGridOwner != null)
				{
					return dataGridOwner.IsKeyboardFocusWithin;
				}
			}
			return false;
		}
	}

	/// <summary>Gets a value that indicates whether the row is selected. </summary>
	/// <returns>true if the row is selected; otherwise, false. The registered default is false. For information about what can influence the value, see <see cref="T:System.Windows.DependencyProperty" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
	public bool IsRowSelected => (bool)GetValue(IsRowSelectedProperty);

	private DataGridRow PreviousRow
	{
		get
		{
			DataGridRow parentRow = ParentRow;
			if (parentRow != null)
			{
				DataGrid dataGridOwner = parentRow.DataGridOwner;
				if (dataGridOwner != null)
				{
					int num = dataGridOwner.ItemContainerGenerator.IndexFromContainer(parentRow);
					if (num > 0)
					{
						return (DataGridRow)dataGridOwner.ItemContainerGenerator.ContainerFromIndex(num - 1);
					}
				}
			}
			return null;
		}
	}

	internal DataGridRow ParentRow => DataGridHelper.FindParent<DataGridRow>(this);

	private DataGrid DataGridOwner => ParentRow?.DataGridOwner;

	static DataGridRowHeader()
	{
		_fallbackStateMapping = new byte[16]
		{
			8, 9, 3, 11, 7, 6, 15, 15, 255, 8,
			11, 14, 15, 14, 9, 8
		};
		_idealStateMapping = new byte[32]
		{
			8, 8, 0, 0, 255, 255, 255, 255, 12, 15,
			4, 7, 10, 11, 2, 3, 9, 9, 1, 1,
			255, 255, 255, 255, 13, 14, 5, 6, 10, 11,
			2, 3
		};
		_stateNames = new string[16]
		{
			"MouseOver", "MouseOver_CurrentRow", "MouseOver_Unfocused_EditingRow", "MouseOver_EditingRow", "MouseOver_Unfocused_Selected", "MouseOver_Unfocused_CurrentRow_Selected", "MouseOver_CurrentRow_Selected", "MouseOver_Selected", "Normal", "Normal_CurrentRow",
			"Unfocused_EditingRow", "Normal_EditingRow", "Unfocused_Selected", "Unfocused_CurrentRow_Selected", "Normal_CurrentRow_Selected", "Normal_Selected"
		};
		SeparatorBrushProperty = DependencyProperty.Register("SeparatorBrush", typeof(Brush), typeof(DataGridRowHeader), new FrameworkPropertyMetadata(null));
		SeparatorVisibilityProperty = DependencyProperty.Register("SeparatorVisibility", typeof(Visibility), typeof(DataGridRowHeader), new FrameworkPropertyMetadata(Visibility.Visible));
		IsRowSelectedPropertyKey = DependencyProperty.RegisterReadOnly("IsRowSelected", typeof(bool), typeof(DataGridRowHeader), new FrameworkPropertyMetadata(false, Control.OnVisualStatePropertyChanged, OnCoerceIsRowSelected));
		IsRowSelectedProperty = IsRowSelectedPropertyKey.DependencyProperty;
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridRowHeader), new FrameworkPropertyMetadata(typeof(DataGridRowHeader)));
		ContentControl.ContentProperty.OverrideMetadata(typeof(DataGridRowHeader), new FrameworkPropertyMetadata(OnNotifyPropertyChanged, OnCoerceContent));
		ContentControl.ContentTemplateProperty.OverrideMetadata(typeof(DataGridRowHeader), new FrameworkPropertyMetadata(OnNotifyPropertyChanged, OnCoerceContentTemplate));
		ContentControl.ContentTemplateSelectorProperty.OverrideMetadata(typeof(DataGridRowHeader), new FrameworkPropertyMetadata(OnNotifyPropertyChanged, OnCoerceContentTemplateSelector));
		FrameworkElement.StyleProperty.OverrideMetadata(typeof(DataGridRowHeader), new FrameworkPropertyMetadata(OnNotifyPropertyChanged, OnCoerceStyle));
		FrameworkElement.WidthProperty.OverrideMetadata(typeof(DataGridRowHeader), new FrameworkPropertyMetadata(OnNotifyPropertyChanged, OnCoerceWidth));
		ButtonBase.ClickModeProperty.OverrideMetadata(typeof(DataGridRowHeader), new FrameworkPropertyMetadata(ClickMode.Press));
		UIElement.FocusableProperty.OverrideMetadata(typeof(DataGridRowHeader), new FrameworkPropertyMetadata(false));
		AutomationProperties.IsOffscreenBehaviorProperty.OverrideMetadata(typeof(DataGridRowHeader), new FrameworkPropertyMetadata(IsOffscreenBehavior.FromClip));
	}

	/// <summary>Returns a new <see cref="T:System.Windows.Automation.Peers.DataGridRowHeaderAutomationPeer" /> for this row header.</summary>
	/// <returns>A new automation peer for this row header.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new DataGridRowHeaderAutomationPeer(this);
	}

	/// <summary>Measures the children of a <see cref="T:System.Windows.Controls.Primitives.DataGridRowHeader" /> to prepare for arranging them during the <see cref="M:System.Windows.Controls.Control.ArrangeOverride(System.Windows.Size)" /> pass. </summary>
	/// <returns>The size that the <see cref="T:System.Windows.Controls.Primitives.DataGridRowHeader" /> determines it needs during layout, based on its calculations of child object allocated sizes. </returns>
	/// <param name="availableSize">The available size that this element can give to child elements. Indicates an upper limit that child elements should not exceed. </param>
	protected override Size MeasureOverride(Size availableSize)
	{
		Size result = base.MeasureOverride(availableSize);
		DataGrid dataGridOwner = DataGridOwner;
		if (dataGridOwner == null)
		{
			return result;
		}
		if (double.IsNaN(dataGridOwner.RowHeaderWidth) && result.Width > dataGridOwner.RowHeaderActualWidth)
		{
			dataGridOwner.RowHeaderActualWidth = result.Width;
		}
		return new Size(dataGridOwner.RowHeaderActualWidth, result.Height);
	}

	/// <summary>Builds the visual tree for the row header when a new template is applied. </summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		DataGridRow parentRow = ParentRow;
		if (parentRow != null)
		{
			parentRow.RowHeader = this;
			SyncProperties();
		}
		HookupGripperEvents();
	}

	internal void SyncProperties()
	{
		DataGridHelper.TransferProperty(this, ContentControl.ContentProperty);
		DataGridHelper.TransferProperty(this, FrameworkElement.StyleProperty);
		DataGridHelper.TransferProperty(this, ContentControl.ContentTemplateProperty);
		DataGridHelper.TransferProperty(this, ContentControl.ContentTemplateSelectorProperty);
		DataGridHelper.TransferProperty(this, FrameworkElement.WidthProperty);
		CoerceValue(IsRowSelectedProperty);
		OnCanUserResizeRowsChanged();
	}

	private static void OnNotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((DataGridRowHeader)d).NotifyPropertyChanged(d, e);
	}

	internal void NotifyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.Property == DataGridRow.HeaderProperty || e.Property == ContentControl.ContentProperty)
		{
			DataGridHelper.TransferProperty(this, ContentControl.ContentProperty);
		}
		else if (e.Property == DataGrid.RowHeaderStyleProperty || e.Property == DataGridRow.HeaderStyleProperty || e.Property == FrameworkElement.StyleProperty)
		{
			DataGridHelper.TransferProperty(this, FrameworkElement.StyleProperty);
		}
		else if (e.Property == DataGrid.RowHeaderTemplateProperty || e.Property == DataGridRow.HeaderTemplateProperty || e.Property == ContentControl.ContentTemplateProperty)
		{
			DataGridHelper.TransferProperty(this, ContentControl.ContentTemplateProperty);
		}
		else if (e.Property == DataGrid.RowHeaderTemplateSelectorProperty || e.Property == DataGridRow.HeaderTemplateSelectorProperty || e.Property == ContentControl.ContentTemplateSelectorProperty)
		{
			DataGridHelper.TransferProperty(this, ContentControl.ContentTemplateSelectorProperty);
		}
		else if (e.Property == DataGrid.RowHeaderWidthProperty || e.Property == FrameworkElement.WidthProperty)
		{
			DataGridHelper.TransferProperty(this, FrameworkElement.WidthProperty);
		}
		else if (e.Property == DataGridRow.IsSelectedProperty)
		{
			CoerceValue(IsRowSelectedProperty);
		}
		else if (e.Property == DataGrid.CanUserResizeRowsProperty)
		{
			OnCanUserResizeRowsChanged();
		}
		else if (e.Property == DataGrid.RowHeaderActualWidthProperty)
		{
			InvalidateMeasure();
			InvalidateArrange();
			if (base.Parent is UIElement uIElement)
			{
				uIElement.InvalidateMeasure();
				uIElement.InvalidateArrange();
			}
		}
		else if (e.Property == DataGrid.CurrentItemProperty || e.Property == DataGridRow.IsEditingProperty || e.Property == UIElement.IsMouseOverProperty || e.Property == UIElement.IsKeyboardFocusWithinProperty)
		{
			UpdateVisualState();
		}
	}

	private static object OnCoerceContent(DependencyObject d, object baseValue)
	{
		DataGridRowHeader dataGridRowHeader = d as DataGridRowHeader;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridRowHeader, baseValue, ContentControl.ContentProperty, dataGridRowHeader.ParentRow, DataGridRow.HeaderProperty);
	}

	private static object OnCoerceContentTemplate(DependencyObject d, object baseValue)
	{
		DataGridRowHeader obj = d as DataGridRowHeader;
		DataGridRow parentRow = obj.ParentRow;
		return DataGridHelper.GetCoercedTransferPropertyValue(grandParentObject: parentRow?.DataGridOwner, baseObject: obj, baseValue: baseValue, baseProperty: ContentControl.ContentTemplateProperty, parentObject: parentRow, parentProperty: DataGridRow.HeaderTemplateProperty, grandParentProperty: DataGrid.RowHeaderTemplateProperty);
	}

	private static object OnCoerceContentTemplateSelector(DependencyObject d, object baseValue)
	{
		DataGridRowHeader obj = d as DataGridRowHeader;
		DataGridRow parentRow = obj.ParentRow;
		return DataGridHelper.GetCoercedTransferPropertyValue(grandParentObject: parentRow?.DataGridOwner, baseObject: obj, baseValue: baseValue, baseProperty: ContentControl.ContentTemplateSelectorProperty, parentObject: parentRow, parentProperty: DataGridRow.HeaderTemplateSelectorProperty, grandParentProperty: DataGrid.RowHeaderTemplateSelectorProperty);
	}

	private static object OnCoerceStyle(DependencyObject d, object baseValue)
	{
		DataGridRowHeader dataGridRowHeader = d as DataGridRowHeader;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridRowHeader, baseValue, FrameworkElement.StyleProperty, dataGridRowHeader.ParentRow, DataGridRow.HeaderStyleProperty, dataGridRowHeader.DataGridOwner, DataGrid.RowHeaderStyleProperty);
	}

	private static object OnCoerceWidth(DependencyObject d, object baseValue)
	{
		DataGridRowHeader dataGridRowHeader = d as DataGridRowHeader;
		return DataGridHelper.GetCoercedTransferPropertyValue(dataGridRowHeader, baseValue, FrameworkElement.WidthProperty, dataGridRowHeader.DataGridOwner, DataGrid.RowHeaderWidthProperty);
	}

	internal override void ChangeVisualState(bool useTransitions)
	{
		byte b = 0;
		if (IsRowCurrent)
		{
			b += 16;
		}
		if (IsRowSelected || IsRowEditing)
		{
			b += 8;
		}
		if (IsRowEditing)
		{
			b += 4;
		}
		if (IsRowMouseOver)
		{
			b += 2;
		}
		if (IsDataGridKeyboardFocusWithin)
		{
			b++;
		}
		for (byte b2 = _idealStateMapping[b]; b2 != byte.MaxValue; b2 = _fallbackStateMapping[b2])
		{
			string stateName = _stateNames[b2];
			if (VisualStateManager.GoToState(this, stateName, useTransitions))
			{
				break;
			}
		}
		ChangeValidationVisualState(useTransitions);
	}

	private static object OnCoerceIsRowSelected(DependencyObject d, object baseValue)
	{
		DataGridRow parentRow = ((DataGridRowHeader)d).ParentRow;
		if (parentRow != null)
		{
			return parentRow.IsSelected;
		}
		return baseValue;
	}

	/// <summary>Raises the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> event and initiates row selection or drag operations. </summary>
	protected override void OnClick()
	{
		base.OnClick();
		if (Mouse.Captured == this)
		{
			ReleaseMouseCapture();
		}
		DataGrid dataGridOwner = DataGridOwner;
		DataGridRow parentRow = ParentRow;
		if (dataGridOwner != null && parentRow != null)
		{
			dataGridOwner.HandleSelectionForRowHeaderAndDetailsInput(parentRow, startDragging: true);
		}
	}

	private void HookupGripperEvents()
	{
		UnhookGripperEvents();
		_topGripper = GetTemplateChild("PART_TopHeaderGripper") as Thumb;
		_bottomGripper = GetTemplateChild("PART_BottomHeaderGripper") as Thumb;
		if (_topGripper != null)
		{
			_topGripper.DragStarted += OnRowHeaderGripperDragStarted;
			_topGripper.DragDelta += OnRowHeaderResize;
			_topGripper.DragCompleted += OnRowHeaderGripperDragCompleted;
			_topGripper.MouseDoubleClick += OnGripperDoubleClicked;
			SetTopGripperVisibility();
		}
		if (_bottomGripper != null)
		{
			_bottomGripper.DragStarted += OnRowHeaderGripperDragStarted;
			_bottomGripper.DragDelta += OnRowHeaderResize;
			_bottomGripper.DragCompleted += OnRowHeaderGripperDragCompleted;
			_bottomGripper.MouseDoubleClick += OnGripperDoubleClicked;
			SetBottomGripperVisibility();
		}
	}

	private void UnhookGripperEvents()
	{
		if (_topGripper != null)
		{
			_topGripper.DragStarted -= OnRowHeaderGripperDragStarted;
			_topGripper.DragDelta -= OnRowHeaderResize;
			_topGripper.DragCompleted -= OnRowHeaderGripperDragCompleted;
			_topGripper.MouseDoubleClick -= OnGripperDoubleClicked;
			_topGripper = null;
		}
		if (_bottomGripper != null)
		{
			_bottomGripper.DragStarted -= OnRowHeaderGripperDragStarted;
			_bottomGripper.DragDelta -= OnRowHeaderResize;
			_bottomGripper.DragCompleted -= OnRowHeaderGripperDragCompleted;
			_bottomGripper.MouseDoubleClick -= OnGripperDoubleClicked;
			_bottomGripper = null;
		}
	}

	private void SetTopGripperVisibility()
	{
		if (_topGripper != null)
		{
			DataGrid dataGridOwner = DataGridOwner;
			DataGridRow parentRow = ParentRow;
			if (dataGridOwner != null && parentRow != null && dataGridOwner.CanUserResizeRows && dataGridOwner.Items.Count > 1 && parentRow.Item != dataGridOwner.Items[0])
			{
				_topGripper.Visibility = Visibility.Visible;
			}
			else
			{
				_topGripper.Visibility = Visibility.Collapsed;
			}
		}
	}

	private void SetBottomGripperVisibility()
	{
		if (_bottomGripper != null)
		{
			DataGrid dataGridOwner = DataGridOwner;
			if (dataGridOwner != null && dataGridOwner.CanUserResizeRows)
			{
				_bottomGripper.Visibility = Visibility.Visible;
			}
			else
			{
				_bottomGripper.Visibility = Visibility.Collapsed;
			}
		}
	}

	private DataGridRow RowToResize(object gripper)
	{
		if (gripper != _bottomGripper)
		{
			return PreviousRow;
		}
		return ParentRow;
	}

	private void OnRowHeaderGripperDragStarted(object sender, DragStartedEventArgs e)
	{
		DataGridRow dataGridRow = RowToResize(sender);
		if (dataGridRow != null)
		{
			dataGridRow.OnRowResizeStarted();
			e.Handled = true;
		}
	}

	private void OnRowHeaderResize(object sender, DragDeltaEventArgs e)
	{
		DataGridRow dataGridRow = RowToResize(sender);
		if (dataGridRow != null)
		{
			dataGridRow.OnRowResize(e.VerticalChange);
			e.Handled = true;
		}
	}

	private void OnRowHeaderGripperDragCompleted(object sender, DragCompletedEventArgs e)
	{
		DataGridRow dataGridRow = RowToResize(sender);
		if (dataGridRow != null)
		{
			dataGridRow.OnRowResizeCompleted(e.Canceled);
			e.Handled = true;
		}
	}

	private void OnGripperDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		DataGridRow dataGridRow = RowToResize(sender);
		if (dataGridRow != null)
		{
			dataGridRow.OnRowResizeReset();
			e.Handled = true;
		}
	}

	private void OnCanUserResizeRowsChanged()
	{
		SetTopGripperVisibility();
		SetBottomGripperVisibility();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.DataGridRowHeader" /> class. </summary>
	public DataGridRowHeader()
	{
	}
}
