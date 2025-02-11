using System.Collections;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls;

/// <summary>Represents the control that redistributes space between columns or rows of a <see cref="T:System.Windows.Controls.Grid" /> control. </summary>
[StyleTypedProperty(Property = "PreviewStyle", StyleTargetType = typeof(Control))]
public class GridSplitter : Thumb
{
	private sealed class PreviewAdorner : Adorner
	{
		private TranslateTransform Translation;

		private Decorator _decorator;

		protected override int VisualChildrenCount => 1;

		public double OffsetX
		{
			get
			{
				return Translation.X;
			}
			set
			{
				Translation.X = value;
			}
		}

		public double OffsetY
		{
			get
			{
				return Translation.Y;
			}
			set
			{
				Translation.Y = value;
			}
		}

		public PreviewAdorner(GridSplitter gridSplitter, Style previewStyle)
			: base(gridSplitter)
		{
			Control child = new Control
			{
				Style = previewStyle,
				IsEnabled = false
			};
			Translation = new TranslateTransform();
			_decorator = new Decorator();
			_decorator.Child = child;
			_decorator.RenderTransform = Translation;
			AddVisualChild(_decorator);
		}

		protected override Visual GetVisualChild(int index)
		{
			if (index != 0)
			{
				throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
			}
			return _decorator;
		}

		protected override Size ArrangeOverride(Size finalSize)
		{
			_decorator.Arrange(new Rect(default(Point), finalSize));
			return finalSize;
		}
	}

	private enum SplitBehavior
	{
		Split,
		Resize1,
		Resize2
	}

	private class ResizeData
	{
		public bool ShowsPreview;

		public PreviewAdorner Adorner;

		public double MinChange;

		public double MaxChange;

		public Grid Grid;

		public GridResizeDirection ResizeDirection;

		public GridResizeBehavior ResizeBehavior;

		public DefinitionBase Definition1;

		public DefinitionBase Definition2;

		public SplitBehavior SplitBehavior;

		public int SplitterIndex;

		public int Definition1Index;

		public int Definition2Index;

		public GridLength OriginalDefinition1Length;

		public GridLength OriginalDefinition2Length;

		public double OriginalDefinition1ActualLength;

		public double OriginalDefinition2ActualLength;

		public double SplitterLength;
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridSplitter.ResizeDirection" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.GridSplitter.ResizeDirection" /> dependency property.</returns>
	public static readonly DependencyProperty ResizeDirectionProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridSplitter.ResizeBehavior" /> dependency property. </summary>
	public static readonly DependencyProperty ResizeBehaviorProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridSplitter.ShowsPreview" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.GridSplitter.ShowsPreview" /> dependency property.</returns>
	public static readonly DependencyProperty ShowsPreviewProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridSplitter.PreviewStyle" /> dependency property. </summary>
	public static readonly DependencyProperty PreviewStyleProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridSplitter.KeyboardIncrement" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.GridSplitter.KeyboardIncrement" /> dependency property.</returns>
	public static readonly DependencyProperty KeyboardIncrementProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridSplitter.DragIncrement" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.GridSplitter.DragIncrement" /> dependency property.</returns>
	public static readonly DependencyProperty DragIncrementProperty;

	private ResizeData _resizeData;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.GridSplitter" /> control resizes rows or columns.  </summary>
	/// <returns>One of the enumeration values that specifies whether to resize rows or columns. The default is <see cref="F:System.Windows.Controls.GridResizeDirection.Auto" />.</returns>
	public GridResizeDirection ResizeDirection
	{
		get
		{
			return (GridResizeDirection)GetValue(ResizeDirectionProperty);
		}
		set
		{
			SetValue(ResizeDirectionProperty, value);
		}
	}

	/// <summary>Gets or sets which columns or rows are resized relative to the column or row for which the <see cref="T:System.Windows.Controls.GridSplitter" /> control is defined. </summary>
	/// <returns>One of the enumeration values that indicates which columns or rows are resized by this <see cref="T:System.Windows.Controls.GridSplitter" /> control. The default is <see cref="F:System.Windows.Controls.GridResizeBehavior.BasedOnAlignment" />.</returns>
	public GridResizeBehavior ResizeBehavior
	{
		get
		{
			return (GridResizeBehavior)GetValue(ResizeBehaviorProperty);
		}
		set
		{
			SetValue(ResizeBehaviorProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the <see cref="T:System.Windows.Controls.GridSplitter" /> control updates the column or row size as the user drags the control. </summary>
	/// <returns>true if a <see cref="T:System.Windows.Controls.GridSplitter" /> preview is displayed; otherwise, false. The default is false.</returns>
	public bool ShowsPreview
	{
		get
		{
			return (bool)GetValue(ShowsPreviewProperty);
		}
		set
		{
			SetValue(ShowsPreviewProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets or sets the style that customizes the appearance, effects, or other style characteristics for the <see cref="T:System.Windows.Controls.GridSplitter" /> control preview indicator that is displayed when the <see cref="P:System.Windows.Controls.GridSplitter.ShowsPreview" /> property is set to true. </summary>
	/// <returns>Returns the <see cref="T:System.Windows.Style" /> for the preview indicator that shows the potential change in <see cref="T:System.Windows.Controls.Grid" /> dimensions as you move the <see cref="T:System.Windows.Controls.GridSplitter" /> control. The default is the style that the current theme supplies.</returns>
	public Style PreviewStyle
	{
		get
		{
			return (Style)GetValue(PreviewStyleProperty);
		}
		set
		{
			SetValue(PreviewStyleProperty, value);
		}
	}

	/// <summary>Gets or sets the distance that each press of an arrow key moves a <see cref="T:System.Windows.Controls.GridSplitter" /> control. </summary>
	/// <returns>The distance that the <see cref="T:System.Windows.Controls.GridSplitter" /> moves for each press of an arrow key. The default is 10. </returns>
	public double KeyboardIncrement
	{
		get
		{
			return (double)GetValue(KeyboardIncrementProperty);
		}
		set
		{
			SetValue(KeyboardIncrementProperty, value);
		}
	}

	/// <summary>Gets or sets the minimum distance that a user must drag a mouse to resize rows or columns with a <see cref="T:System.Windows.Controls.GridSplitter" /> control. </summary>
	/// <returns>A value that represents the minimum distance that a user must use the mouse to drag a <see cref="T:System.Windows.Controls.GridSplitter" /> to resize rows or columns. The default is 1.</returns>
	public double DragIncrement
	{
		get
		{
			return (double)GetValue(DragIncrementProperty);
		}
		set
		{
			SetValue(DragIncrementProperty, value);
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	static GridSplitter()
	{
		ResizeDirectionProperty = DependencyProperty.Register("ResizeDirection", typeof(GridResizeDirection), typeof(GridSplitter), new FrameworkPropertyMetadata(GridResizeDirection.Auto, UpdateCursor), IsValidResizeDirection);
		ResizeBehaviorProperty = DependencyProperty.Register("ResizeBehavior", typeof(GridResizeBehavior), typeof(GridSplitter), new FrameworkPropertyMetadata(GridResizeBehavior.BasedOnAlignment), IsValidResizeBehavior);
		ShowsPreviewProperty = DependencyProperty.Register("ShowsPreview", typeof(bool), typeof(GridSplitter), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		PreviewStyleProperty = DependencyProperty.Register("PreviewStyle", typeof(Style), typeof(GridSplitter), new FrameworkPropertyMetadata((object)null));
		KeyboardIncrementProperty = DependencyProperty.Register("KeyboardIncrement", typeof(double), typeof(GridSplitter), new FrameworkPropertyMetadata(10.0), IsValidDelta);
		DragIncrementProperty = DependencyProperty.Register("DragIncrement", typeof(double), typeof(GridSplitter), new FrameworkPropertyMetadata(1.0), IsValidDelta);
		EventManager.RegisterClassHandler(typeof(GridSplitter), Thumb.DragStartedEvent, new DragStartedEventHandler(OnDragStarted));
		EventManager.RegisterClassHandler(typeof(GridSplitter), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnDragDelta));
		EventManager.RegisterClassHandler(typeof(GridSplitter), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnDragCompleted));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GridSplitter), new FrameworkPropertyMetadata(typeof(GridSplitter)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(GridSplitter));
		UIElement.FocusableProperty.OverrideMetadata(typeof(GridSplitter), new FrameworkPropertyMetadata(BooleanBoxes.TrueBox));
		FrameworkElement.HorizontalAlignmentProperty.OverrideMetadata(typeof(GridSplitter), new FrameworkPropertyMetadata(HorizontalAlignment.Right));
		FrameworkElement.CursorProperty.OverrideMetadata(typeof(GridSplitter), new FrameworkPropertyMetadata(null, CoerceCursor));
		ControlsTraceLogger.AddControl(TelemetryControls.GridSplitter);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.GridSplitter" /> class. </summary>
	public GridSplitter()
	{
	}

	private static void UpdateCursor(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		o.CoerceValue(FrameworkElement.CursorProperty);
	}

	private static object CoerceCursor(DependencyObject o, object value)
	{
		GridSplitter gridSplitter = (GridSplitter)o;
		bool hasModifiers;
		BaseValueSourceInternal valueSource = gridSplitter.GetValueSource(FrameworkElement.CursorProperty, null, out hasModifiers);
		if (value == null && valueSource == BaseValueSourceInternal.Default)
		{
			switch (gridSplitter.GetEffectiveResizeDirection())
			{
			case GridResizeDirection.Columns:
				return Cursors.SizeWE;
			case GridResizeDirection.Rows:
				return Cursors.SizeNS;
			}
		}
		return value;
	}

	private static bool IsValidResizeDirection(object o)
	{
		GridResizeDirection gridResizeDirection = (GridResizeDirection)o;
		if (gridResizeDirection != 0 && gridResizeDirection != GridResizeDirection.Columns)
		{
			return gridResizeDirection == GridResizeDirection.Rows;
		}
		return true;
	}

	private static bool IsValidResizeBehavior(object o)
	{
		GridResizeBehavior gridResizeBehavior = (GridResizeBehavior)o;
		if (gridResizeBehavior != 0 && gridResizeBehavior != GridResizeBehavior.CurrentAndNext && gridResizeBehavior != GridResizeBehavior.PreviousAndCurrent)
		{
			return gridResizeBehavior == GridResizeBehavior.PreviousAndNext;
		}
		return true;
	}

	private static bool IsValidDelta(object o)
	{
		double num = (double)o;
		if (num > 0.0)
		{
			return !double.IsPositiveInfinity(num);
		}
		return false;
	}

	/// <summary>Creates the implementation of <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for the <see cref="T:System.Windows.Controls.GridSplitter" /> control.</summary>
	/// <returns>A new <see cref="T:System.Windows.Automation.Peers.GridSplitterAutomationPeer" /> for this <see cref="T:System.Windows.Controls.ToolTip" /> control.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new GridSplitterAutomationPeer(this);
	}

	private GridResizeDirection GetEffectiveResizeDirection()
	{
		GridResizeDirection gridResizeDirection = ResizeDirection;
		if (gridResizeDirection == GridResizeDirection.Auto)
		{
			gridResizeDirection = ((base.HorizontalAlignment != HorizontalAlignment.Stretch) ? GridResizeDirection.Columns : ((base.VerticalAlignment != VerticalAlignment.Stretch) ? GridResizeDirection.Rows : ((base.ActualWidth <= base.ActualHeight) ? GridResizeDirection.Columns : GridResizeDirection.Rows)));
		}
		return gridResizeDirection;
	}

	private GridResizeBehavior GetEffectiveResizeBehavior(GridResizeDirection direction)
	{
		GridResizeBehavior gridResizeBehavior = ResizeBehavior;
		if (gridResizeBehavior == GridResizeBehavior.BasedOnAlignment)
		{
			gridResizeBehavior = ((direction == GridResizeDirection.Columns) ? (base.HorizontalAlignment switch
			{
				HorizontalAlignment.Left => GridResizeBehavior.PreviousAndCurrent, 
				HorizontalAlignment.Right => GridResizeBehavior.CurrentAndNext, 
				_ => GridResizeBehavior.PreviousAndNext, 
			}) : (base.VerticalAlignment switch
			{
				VerticalAlignment.Top => GridResizeBehavior.PreviousAndCurrent, 
				VerticalAlignment.Bottom => GridResizeBehavior.CurrentAndNext, 
				_ => GridResizeBehavior.PreviousAndNext, 
			}));
		}
		return gridResizeBehavior;
	}

	/// <summary>Responds to a change in the dimensions of the <see cref="T:System.Windows.Controls.GridSplitter" /> control.</summary>
	/// <param name="sizeInfo">Information about the change in size of the <see cref="T:System.Windows.Controls.GridSplitter" />.</param>
	protected internal override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		CoerceValue(FrameworkElement.CursorProperty);
	}

	private void RemovePreviewAdorner()
	{
		if (_resizeData.Adorner != null)
		{
			(VisualTreeHelper.GetParent(_resizeData.Adorner) as AdornerLayer).Remove(_resizeData.Adorner);
		}
	}

	private void InitializeData(bool ShowsPreview)
	{
		if (base.Parent is Grid grid)
		{
			_resizeData = new ResizeData();
			_resizeData.Grid = grid;
			_resizeData.ShowsPreview = ShowsPreview;
			_resizeData.ResizeDirection = GetEffectiveResizeDirection();
			_resizeData.ResizeBehavior = GetEffectiveResizeBehavior(_resizeData.ResizeDirection);
			_resizeData.SplitterLength = Math.Min(base.ActualWidth, base.ActualHeight);
			if (!SetupDefinitionsToResize())
			{
				_resizeData = null;
			}
			else
			{
				SetupPreview();
			}
		}
	}

	private bool SetupDefinitionsToResize()
	{
		if ((int)GetValue((_resizeData.ResizeDirection == GridResizeDirection.Columns) ? Grid.ColumnSpanProperty : Grid.RowSpanProperty) == 1)
		{
			int num = (int)GetValue((_resizeData.ResizeDirection == GridResizeDirection.Columns) ? Grid.ColumnProperty : Grid.RowProperty);
			int num2;
			int num3;
			switch (_resizeData.ResizeBehavior)
			{
			case GridResizeBehavior.PreviousAndCurrent:
				num2 = num - 1;
				num3 = num;
				break;
			case GridResizeBehavior.CurrentAndNext:
				num2 = num;
				num3 = num + 1;
				break;
			default:
				num2 = num - 1;
				num3 = num + 1;
				break;
			}
			int num4 = ((_resizeData.ResizeDirection == GridResizeDirection.Columns) ? _resizeData.Grid.ColumnDefinitions.Count : _resizeData.Grid.RowDefinitions.Count);
			if (num2 >= 0 && num3 < num4)
			{
				_resizeData.SplitterIndex = num;
				_resizeData.Definition1Index = num2;
				_resizeData.Definition1 = GetGridDefinition(_resizeData.Grid, num2, _resizeData.ResizeDirection);
				_resizeData.OriginalDefinition1Length = _resizeData.Definition1.UserSizeValueCache;
				_resizeData.OriginalDefinition1ActualLength = GetActualLength(_resizeData.Definition1);
				_resizeData.Definition2Index = num3;
				_resizeData.Definition2 = GetGridDefinition(_resizeData.Grid, num3, _resizeData.ResizeDirection);
				_resizeData.OriginalDefinition2Length = _resizeData.Definition2.UserSizeValueCache;
				_resizeData.OriginalDefinition2ActualLength = GetActualLength(_resizeData.Definition2);
				bool flag = IsStar(_resizeData.Definition1);
				bool flag2 = IsStar(_resizeData.Definition2);
				if (flag && flag2)
				{
					_resizeData.SplitBehavior = SplitBehavior.Split;
				}
				else
				{
					_resizeData.SplitBehavior = ((!flag) ? SplitBehavior.Resize1 : SplitBehavior.Resize2);
				}
				return true;
			}
		}
		return false;
	}

	private void SetupPreview()
	{
		if (_resizeData.ShowsPreview)
		{
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_resizeData.Grid);
			if (adornerLayer != null)
			{
				_resizeData.Adorner = new PreviewAdorner(this, PreviewStyle);
				adornerLayer.Add(_resizeData.Adorner);
				GetDeltaConstraints(out _resizeData.MinChange, out _resizeData.MaxChange);
			}
		}
	}

	/// <summary>Called when the <see cref="T:System.Windows.Controls.GridSplitter" /> control loses keyboard focus.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.KeyboardFocusChangedEventArgs" /> that contains the event data. </param>
	protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnLostKeyboardFocus(e);
		if (_resizeData != null)
		{
			CancelResize();
		}
	}

	private static void OnDragStarted(object sender, DragStartedEventArgs e)
	{
		(sender as GridSplitter).OnDragStarted(e);
	}

	private void OnDragStarted(DragStartedEventArgs e)
	{
		InitializeData(ShowsPreview);
	}

	private static void OnDragDelta(object sender, DragDeltaEventArgs e)
	{
		(sender as GridSplitter).OnDragDelta(e);
	}

	private void OnDragDelta(DragDeltaEventArgs e)
	{
		if (_resizeData == null)
		{
			return;
		}
		double horizontalChange = e.HorizontalChange;
		double verticalChange = e.VerticalChange;
		double dragIncrement = DragIncrement;
		horizontalChange = Math.Round(horizontalChange / dragIncrement) * dragIncrement;
		verticalChange = Math.Round(verticalChange / dragIncrement) * dragIncrement;
		if (_resizeData.ShowsPreview)
		{
			if (_resizeData.ResizeDirection == GridResizeDirection.Columns)
			{
				_resizeData.Adorner.OffsetX = Math.Min(Math.Max(horizontalChange, _resizeData.MinChange), _resizeData.MaxChange);
			}
			else
			{
				_resizeData.Adorner.OffsetY = Math.Min(Math.Max(verticalChange, _resizeData.MinChange), _resizeData.MaxChange);
			}
		}
		else
		{
			MoveSplitter(horizontalChange, verticalChange);
		}
	}

	private static void OnDragCompleted(object sender, DragCompletedEventArgs e)
	{
		(sender as GridSplitter).OnDragCompleted(e);
	}

	private void OnDragCompleted(DragCompletedEventArgs e)
	{
		if (_resizeData != null)
		{
			if (_resizeData.ShowsPreview)
			{
				MoveSplitter(_resizeData.Adorner.OffsetX, _resizeData.Adorner.OffsetY);
				RemovePreviewAdorner();
			}
			_resizeData = null;
		}
	}

	/// <summary>Called when a key is pressed.</summary>
	/// <param name="e">A <see cref="T:System.Windows.Input.KeyEventArgs" /> that contains the event data. </param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		switch (e.Key)
		{
		case Key.Escape:
			if (_resizeData != null)
			{
				CancelResize();
				e.Handled = true;
			}
			break;
		case Key.Left:
			e.Handled = KeyboardMoveSplitter(0.0 - KeyboardIncrement, 0.0);
			break;
		case Key.Right:
			e.Handled = KeyboardMoveSplitter(KeyboardIncrement, 0.0);
			break;
		case Key.Up:
			e.Handled = KeyboardMoveSplitter(0.0, 0.0 - KeyboardIncrement);
			break;
		case Key.Down:
			e.Handled = KeyboardMoveSplitter(0.0, KeyboardIncrement);
			break;
		}
	}

	private void CancelResize()
	{
		_ = base.Parent;
		if (_resizeData.ShowsPreview)
		{
			RemovePreviewAdorner();
		}
		else
		{
			SetDefinitionLength(_resizeData.Definition1, _resizeData.OriginalDefinition1Length);
			SetDefinitionLength(_resizeData.Definition2, _resizeData.OriginalDefinition2Length);
		}
		_resizeData = null;
	}

	private static bool IsStar(DefinitionBase definition)
	{
		return definition.UserSizeValueCache.IsStar;
	}

	private static DefinitionBase GetGridDefinition(Grid grid, int index, GridResizeDirection direction)
	{
		if (direction != GridResizeDirection.Columns)
		{
			return grid.RowDefinitions[index];
		}
		return grid.ColumnDefinitions[index];
	}

	private double GetActualLength(DefinitionBase definition)
	{
		if (definition is ColumnDefinition columnDefinition)
		{
			return columnDefinition.ActualWidth;
		}
		return ((RowDefinition)definition).ActualHeight;
	}

	private static void SetDefinitionLength(DefinitionBase definition, GridLength length)
	{
		definition.SetValue((definition is ColumnDefinition) ? ColumnDefinition.WidthProperty : RowDefinition.HeightProperty, length);
	}

	private void GetDeltaConstraints(out double minDelta, out double maxDelta)
	{
		double actualLength = GetActualLength(_resizeData.Definition1);
		double num = _resizeData.Definition1.UserMinSizeValueCache;
		double userMaxSizeValueCache = _resizeData.Definition1.UserMaxSizeValueCache;
		double actualLength2 = GetActualLength(_resizeData.Definition2);
		double num2 = _resizeData.Definition2.UserMinSizeValueCache;
		double userMaxSizeValueCache2 = _resizeData.Definition2.UserMaxSizeValueCache;
		if (_resizeData.SplitterIndex == _resizeData.Definition1Index)
		{
			num = Math.Max(num, _resizeData.SplitterLength);
		}
		else if (_resizeData.SplitterIndex == _resizeData.Definition2Index)
		{
			num2 = Math.Max(num2, _resizeData.SplitterLength);
		}
		if (_resizeData.SplitBehavior == SplitBehavior.Split)
		{
			minDelta = 0.0 - Math.Min(actualLength - num, userMaxSizeValueCache2 - actualLength2);
			maxDelta = Math.Min(userMaxSizeValueCache - actualLength, actualLength2 - num2);
		}
		else if (_resizeData.SplitBehavior == SplitBehavior.Resize1)
		{
			minDelta = num - actualLength;
			maxDelta = userMaxSizeValueCache - actualLength;
		}
		else
		{
			minDelta = actualLength2 - userMaxSizeValueCache2;
			maxDelta = actualLength2 - num2;
		}
	}

	private void SetLengths(double definition1Pixels, double definition2Pixels)
	{
		if (_resizeData.SplitBehavior == SplitBehavior.Split)
		{
			IEnumerable enumerable;
			if (_resizeData.ResizeDirection != GridResizeDirection.Columns)
			{
				IEnumerable rowDefinitions = _resizeData.Grid.RowDefinitions;
				enumerable = rowDefinitions;
			}
			else
			{
				IEnumerable rowDefinitions = _resizeData.Grid.ColumnDefinitions;
				enumerable = rowDefinitions;
			}
			int num = 0;
			{
				foreach (DefinitionBase item in enumerable)
				{
					if (num == _resizeData.Definition1Index)
					{
						SetDefinitionLength(item, new GridLength(definition1Pixels, GridUnitType.Star));
					}
					else if (num == _resizeData.Definition2Index)
					{
						SetDefinitionLength(item, new GridLength(definition2Pixels, GridUnitType.Star));
					}
					else if (IsStar(item))
					{
						SetDefinitionLength(item, new GridLength(GetActualLength(item), GridUnitType.Star));
					}
					num++;
				}
				return;
			}
		}
		if (_resizeData.SplitBehavior == SplitBehavior.Resize1)
		{
			SetDefinitionLength(_resizeData.Definition1, new GridLength(definition1Pixels));
		}
		else
		{
			SetDefinitionLength(_resizeData.Definition2, new GridLength(definition2Pixels));
		}
	}

	private void MoveSplitter(double horizontalChange, double verticalChange)
	{
		DpiScale dpi = GetDpi();
		double num;
		if (_resizeData.ResizeDirection == GridResizeDirection.Columns)
		{
			num = horizontalChange;
			if (base.UseLayoutRounding)
			{
				num = UIElement.RoundLayoutValue(num, dpi.DpiScaleX);
			}
		}
		else
		{
			num = verticalChange;
			if (base.UseLayoutRounding)
			{
				num = UIElement.RoundLayoutValue(num, dpi.DpiScaleY);
			}
		}
		DefinitionBase definition = _resizeData.Definition1;
		DefinitionBase definition2 = _resizeData.Definition2;
		if (definition == null || definition2 == null)
		{
			return;
		}
		double actualLength = GetActualLength(definition);
		double actualLength2 = GetActualLength(definition2);
		if (_resizeData.SplitBehavior == SplitBehavior.Split && !LayoutDoubleUtil.AreClose(actualLength + actualLength2, _resizeData.OriginalDefinition1ActualLength + _resizeData.OriginalDefinition2ActualLength))
		{
			CancelResize();
			return;
		}
		GetDeltaConstraints(out var minDelta, out var maxDelta);
		if (base.FlowDirection != _resizeData.Grid.FlowDirection)
		{
			num = 0.0 - num;
		}
		num = Math.Min(Math.Max(num, minDelta), maxDelta);
		double num2 = actualLength + num;
		double definition2Pixels = actualLength + actualLength2 - num2;
		SetLengths(num2, definition2Pixels);
	}

	internal bool KeyboardMoveSplitter(double horizontalChange, double verticalChange)
	{
		if (_resizeData != null)
		{
			return false;
		}
		InitializeData(ShowsPreview: false);
		if (_resizeData == null)
		{
			return false;
		}
		if (base.FlowDirection == FlowDirection.RightToLeft)
		{
			horizontalChange = 0.0 - horizontalChange;
		}
		MoveSplitter(horizontalChange, verticalChange);
		_resizeData = null;
		return true;
	}
}
