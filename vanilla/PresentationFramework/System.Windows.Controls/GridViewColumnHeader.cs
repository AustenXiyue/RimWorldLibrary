using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.KnownBoxes;

namespace System.Windows.Controls;

/// <summary>Represents a column header for a <see cref="T:System.Windows.Controls.GridViewColumn" />.</summary>
[TemplatePart(Name = "PART_HeaderGripper", Type = typeof(Thumb))]
[TemplatePart(Name = "PART_FloatingHeaderCanvas", Type = typeof(Canvas))]
public class GridViewColumnHeader : ButtonBase
{
	[Flags]
	private enum Flags
	{
		None = 0,
		StyleSetByUser = 1,
		IgnoreStyle = 2,
		ContentTemplateSetByUser = 4,
		IgnoreContentTemplate = 8,
		ContentTemplateSelectorSetByUser = 0x10,
		IgnoreContentTemplateSelector = 0x20,
		ContextMenuSetByUser = 0x40,
		IgnoreContextMenu = 0x80,
		ToolTipSetByUser = 0x100,
		IgnoreToolTip = 0x200,
		SuppressClickEvent = 0x400,
		IsInternalGenerated = 0x800,
		IsAccessKeyOrAutomation = 0x1000,
		ContentStringFormatSetByUser = 0x2000,
		IgnoreContentStringFormat = 0x4000
	}

	internal static readonly DependencyPropertyKey ColumnPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewColumnHeader.Column" /> dependency property. </summary>
	public static readonly DependencyProperty ColumnProperty;

	internal static readonly DependencyPropertyKey RolePropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.GridViewColumnHeader.Role" /> dependency property. </summary>
	public static readonly DependencyProperty RoleProperty;

	private static DependencyObjectType _dType;

	private GridViewColumnHeader _previousHeader;

	private static Cursor _splitCursorCache;

	private static Cursor _splitOpenCursorCache;

	private Flags _flags;

	private Thumb _headerGripper;

	private double _originalWidth;

	private Canvas _floatingHeaderCanvas;

	private GridViewColumnHeader _srcHeader;

	private const int c_SPLIT = 100;

	private const int c_SPLITOPEN = 101;

	private const string HeaderGripperTemplateName = "PART_HeaderGripper";

	private const string FloatingHeaderCanvasTemplateName = "PART_FloatingHeaderCanvas";

	/// <summary>Gets the <see cref="T:System.Windows.Controls.GridViewColumn" /> that is associated with the <see cref="T:System.Windows.Controls.GridViewColumnHeader" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.GridViewColumn" /> that is associated with this <see cref="T:System.Windows.Controls.GridViewColumnHeader" />. The default is null.</returns>
	public GridViewColumn Column => (GridViewColumn)GetValue(ColumnProperty);

	/// <summary>Gets the role of a <see cref="T:System.Windows.Controls.GridViewColumnHeader" />. </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.GridViewColumnHeaderRole" /> enumeration value that specifies the current role of the column.</returns>
	[Category("Behavior")]
	public GridViewColumnHeaderRole Role => (GridViewColumnHeaderRole)GetValue(RoleProperty);

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	internal GridViewColumnHeader PreviousVisualHeader
	{
		get
		{
			return _previousHeader;
		}
		set
		{
			_previousHeader = value;
		}
	}

	internal bool SuppressClickEvent
	{
		get
		{
			return GetFlag(Flags.SuppressClickEvent);
		}
		set
		{
			SetFlag(Flags.SuppressClickEvent, value);
		}
	}

	internal GridViewColumnHeader FloatSourceHeader
	{
		get
		{
			return _srcHeader;
		}
		set
		{
			_srcHeader = value;
		}
	}

	internal bool IsInternalGenerated
	{
		get
		{
			return GetFlag(Flags.IsInternalGenerated);
		}
		set
		{
			SetFlag(Flags.IsInternalGenerated, value);
		}
	}

	private Cursor SplitCursor
	{
		get
		{
			if (_splitCursorCache == null)
			{
				_splitCursorCache = GetCursor(100);
			}
			return _splitCursorCache;
		}
	}

	private Cursor SplitOpenCursor
	{
		get
		{
			if (_splitOpenCursorCache == null)
			{
				_splitOpenCursorCache = GetCursor(101);
			}
			return _splitOpenCursorCache;
		}
	}

	private bool IsAccessKeyOrAutomation
	{
		get
		{
			return GetFlag(Flags.IsAccessKeyOrAutomation);
		}
		set
		{
			SetFlag(Flags.IsAccessKeyOrAutomation, value);
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

	static GridViewColumnHeader()
	{
		ColumnPropertyKey = DependencyProperty.RegisterReadOnly("Column", typeof(GridViewColumn), typeof(GridViewColumnHeader), null);
		ColumnProperty = ColumnPropertyKey.DependencyProperty;
		RolePropertyKey = DependencyProperty.RegisterReadOnly("Role", typeof(GridViewColumnHeaderRole), typeof(GridViewColumnHeader), new FrameworkPropertyMetadata(GridViewColumnHeaderRole.Normal));
		RoleProperty = RolePropertyKey.DependencyProperty;
		_splitCursorCache = null;
		_splitOpenCursorCache = null;
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(GridViewColumnHeader), new FrameworkPropertyMetadata(typeof(GridViewColumnHeader)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(GridViewColumnHeader));
		UIElement.FocusableProperty.OverrideMetadata(typeof(GridViewColumnHeader), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		FrameworkElement.StyleProperty.OverrideMetadata(typeof(GridViewColumnHeader), new FrameworkPropertyMetadata(PropertyChanged));
		ContentControl.ContentTemplateProperty.OverrideMetadata(typeof(GridViewColumnHeader), new FrameworkPropertyMetadata(PropertyChanged));
		ContentControl.ContentTemplateSelectorProperty.OverrideMetadata(typeof(GridViewColumnHeader), new FrameworkPropertyMetadata(PropertyChanged));
		FrameworkElement.ContextMenuProperty.OverrideMetadata(typeof(GridViewColumnHeader), new FrameworkPropertyMetadata(PropertyChanged));
		FrameworkElement.ToolTipProperty.OverrideMetadata(typeof(GridViewColumnHeader), new FrameworkPropertyMetadata(PropertyChanged));
	}

	/// <summary>Responds to the creation of the visual tree for the <see cref="T:System.Windows.Controls.GridViewColumnHeader" />.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		switch (Role)
		{
		case GridViewColumnHeaderRole.Normal:
			HookupGripperEvents();
			break;
		case GridViewColumnHeaderRole.Floating:
			_floatingHeaderCanvas = GetTemplateChild("PART_FloatingHeaderCanvas") as Canvas;
			UpdateFloatingHeaderCanvas();
			break;
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseLeftButtonUp" /> event when the user releases the left mouse button while pausing the mouse pointer on the <see cref="T:System.Windows.Controls.GridViewColumnHeader" />.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonUp(e);
		e.Handled = false;
		if (base.ClickMode == ClickMode.Hover && base.IsMouseCaptured)
		{
			ReleaseMouseCapture();
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseLeftButtonDown" /> event when the user presses the left mouse button while pausing the mouse pointer on the <see cref="T:System.Windows.Controls.GridViewColumnHeader" />.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonDown(e);
		e.Handled = false;
		if (base.ClickMode == ClickMode.Hover && e.ButtonState == MouseButtonState.Pressed)
		{
			CaptureMouse();
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseMove" /> event that occurs when the user moves the mouse within a <see cref="T:System.Windows.Controls.GridViewColumnHeader" />. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (base.ClickMode != ClickMode.Hover && base.IsMouseCaptured && Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed)
		{
			SetValue(ButtonBase.IsPressedPropertyKey, BooleanBoxes.TrueBox);
		}
		e.Handled = false;
	}

	/// <summary>Responds to a change in <see cref="T:System.Windows.Controls.GridViewColumnHeader" /> dimensions.</summary>
	/// <param name="sizeInfo">Information about the change in the size of the <see cref="T:System.Windows.Controls.GridViewColumnHeader" />. </param>
	protected internal override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		CheckWidthForPreviousHeaderGripper();
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.Controls.Primitives.ButtonBase.Click" /> event for a <see cref="T:System.Windows.Controls.GridViewColumnHeader" />.</summary>
	protected override void OnClick()
	{
		if (!SuppressClickEvent && (IsAccessKeyOrAutomation || !IsMouseOutside()))
		{
			IsAccessKeyOrAutomation = false;
			ClickImplement();
			MakeParentGotFocus();
		}
	}

	/// <summary>Responds when the <see cref="P:System.Windows.Controls.AccessText.AccessKey" /> for the <see cref="T:System.Windows.Controls.GridViewColumnHeader" /> is pressed.</summary>
	/// <param name="e">The event arguments.</param>
	protected override void OnAccessKey(AccessKeyEventArgs e)
	{
		IsAccessKeyOrAutomation = true;
		base.OnAccessKey(e);
	}

	/// <summary>Determines whether to serialize a <see cref="T:System.Windows.DependencyProperty" />.</summary>
	/// <returns>true if the <see cref="T:System.Windows.DependencyProperty" /> must be serialized; otherwise, false. The default is false.</returns>
	/// <param name="dp">The dependency property.</param>
	protected internal override bool ShouldSerializeProperty(DependencyProperty dp)
	{
		if (IsInternalGenerated)
		{
			return false;
		}
		PropertyToFlags(dp, out var flag, out var _);
		if (flag == Flags.None || GetFlag(flag))
		{
			return base.ShouldSerializeProperty(dp);
		}
		return false;
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseEnter" /> event when the user pauses the mouse pointer on the <see cref="T:System.Windows.Controls.GridViewColumnHeader" />.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseEnter(MouseEventArgs e)
	{
		if (HandleIsMouseOverChanged())
		{
			e.Handled = true;
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.MouseLeave" /> event when the mouse moves off the <see cref="T:System.Windows.Controls.GridViewColumnHeader" />.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnMouseLeave(MouseEventArgs e)
	{
		if (HandleIsMouseOverChanged())
		{
			e.Handled = true;
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.LostKeyboardFocus" /> event for a <see cref="T:System.Windows.Controls.GridViewColumnHeader" />.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
	{
		base.OnLostKeyboardFocus(e);
		if (base.ClickMode == ClickMode.Hover && base.IsMouseCaptured)
		{
			ReleaseMouseCapture();
		}
	}

	internal void AutomationClick()
	{
		IsAccessKeyOrAutomation = true;
		OnClick();
	}

	internal void OnColumnHeaderKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape && _headerGripper != null && _headerGripper.IsDragging)
		{
			_headerGripper.CancelDrag();
			e.Handled = true;
		}
	}

	internal void CheckWidthForPreviousHeaderGripper()
	{
		bool hide = false;
		if (_headerGripper != null)
		{
			hide = DoubleUtil.LessThan(base.ActualWidth, _headerGripper.Width);
		}
		if (_previousHeader != null)
		{
			_previousHeader.HideGripperRightHalf(hide);
		}
		UpdateGripperCursor();
	}

	internal void ResetFloatingHeaderCanvasBackground()
	{
		if (_floatingHeaderCanvas != null)
		{
			_floatingHeaderCanvas.Background = null;
		}
	}

	internal void UpdateProperty(DependencyProperty dp, object value)
	{
		Flags ignoreFlag = Flags.None;
		if (!IsInternalGenerated)
		{
			PropertyToFlags(dp, out var flag, out ignoreFlag);
			if (GetFlag(flag))
			{
				return;
			}
			SetFlag(ignoreFlag, set: true);
		}
		if (value != null)
		{
			SetValue(dp, value);
		}
		else
		{
			ClearValue(dp);
		}
		SetFlag(ignoreFlag, set: false);
	}

	internal void UpdateColumnHeaderWidth(double width)
	{
		if (Column != null)
		{
			Column.Width = width;
		}
		else
		{
			base.Width = width;
		}
	}

	/// <summary>Provides an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation for a <see cref="T:System.Windows.Controls.GridViewColumnHeader" />.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.GridViewColumnHeaderAutomationPeer" /> for this <see cref="T:System.Windows.Controls.GridViewColumnHeader" />.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new GridViewColumnHeaderAutomationPeer(this);
	}

	private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		GridViewColumnHeader gridViewColumnHeader = (GridViewColumnHeader)d;
		if (gridViewColumnHeader.IsInternalGenerated)
		{
			return;
		}
		PropertyToFlags(e.Property, out var flag, out var ignoreFlag);
		if (gridViewColumnHeader.GetFlag(ignoreFlag))
		{
			return;
		}
		if (e.NewValueSource == BaseValueSourceInternal.Local)
		{
			gridViewColumnHeader.SetFlag(flag, set: true);
			return;
		}
		gridViewColumnHeader.SetFlag(flag, set: false);
		if (gridViewColumnHeader.Parent is GridViewHeaderRowPresenter gridViewHeaderRowPresenter)
		{
			gridViewHeaderRowPresenter.UpdateHeaderProperty(gridViewColumnHeader, e.Property);
		}
	}

	private static void PropertyToFlags(DependencyProperty dp, out Flags flag, out Flags ignoreFlag)
	{
		if (dp == FrameworkElement.StyleProperty)
		{
			flag = Flags.StyleSetByUser;
			ignoreFlag = Flags.IgnoreStyle;
		}
		else if (dp == ContentControl.ContentTemplateProperty)
		{
			flag = Flags.ContentTemplateSetByUser;
			ignoreFlag = Flags.IgnoreContentTemplate;
		}
		else if (dp == ContentControl.ContentTemplateSelectorProperty)
		{
			flag = Flags.ContentTemplateSelectorSetByUser;
			ignoreFlag = Flags.IgnoreContentTemplateSelector;
		}
		else if (dp == ContentControl.ContentStringFormatProperty)
		{
			flag = Flags.ContentStringFormatSetByUser;
			ignoreFlag = Flags.IgnoreContentStringFormat;
		}
		else if (dp == FrameworkElement.ContextMenuProperty)
		{
			flag = Flags.ContextMenuSetByUser;
			ignoreFlag = Flags.IgnoreContextMenu;
		}
		else if (dp == FrameworkElement.ToolTipProperty)
		{
			flag = Flags.ToolTipSetByUser;
			ignoreFlag = Flags.IgnoreToolTip;
		}
		else
		{
			flag = (ignoreFlag = Flags.None);
		}
	}

	private void HideGripperRightHalf(bool hide)
	{
		if (_headerGripper != null && _headerGripper.Parent is FrameworkElement frameworkElement)
		{
			frameworkElement.ClipToBounds = hide;
		}
	}

	private void OnColumnHeaderGripperDragStarted(object sender, DragStartedEventArgs e)
	{
		MakeParentGotFocus();
		_originalWidth = ColumnActualWidth;
		e.Handled = true;
	}

	private void MakeParentGotFocus()
	{
		if (base.Parent is GridViewHeaderRowPresenter gridViewHeaderRowPresenter)
		{
			gridViewHeaderRowPresenter.MakeParentItemsControlGotFocus();
		}
	}

	private void OnColumnHeaderResize(object sender, DragDeltaEventArgs e)
	{
		double num = ColumnActualWidth + e.HorizontalChange;
		if (DoubleUtil.LessThanOrClose(num, 0.0))
		{
			num = 0.0;
		}
		UpdateColumnHeaderWidth(num);
		e.Handled = true;
	}

	private void OnColumnHeaderGripperDragCompleted(object sender, DragCompletedEventArgs e)
	{
		if (e.Canceled)
		{
			UpdateColumnHeaderWidth(_originalWidth);
		}
		UpdateGripperCursor();
		e.Handled = true;
	}

	private void HookupGripperEvents()
	{
		UnhookGripperEvents();
		_headerGripper = GetTemplateChild("PART_HeaderGripper") as Thumb;
		if (_headerGripper != null)
		{
			_headerGripper.DragStarted += OnColumnHeaderGripperDragStarted;
			_headerGripper.DragDelta += OnColumnHeaderResize;
			_headerGripper.DragCompleted += OnColumnHeaderGripperDragCompleted;
			_headerGripper.MouseDoubleClick += OnGripperDoubleClicked;
			_headerGripper.MouseEnter += OnGripperMouseEnterLeave;
			_headerGripper.MouseLeave += OnGripperMouseEnterLeave;
			_headerGripper.Cursor = SplitCursor;
		}
	}

	private void OnGripperDoubleClicked(object sender, MouseButtonEventArgs e)
	{
		if (Column != null)
		{
			if (double.IsNaN(Column.Width))
			{
				Column.Width = Column.ActualWidth;
			}
			Column.Width = double.NaN;
			e.Handled = true;
		}
	}

	private void UnhookGripperEvents()
	{
		if (_headerGripper != null)
		{
			_headerGripper.DragStarted -= OnColumnHeaderGripperDragStarted;
			_headerGripper.DragDelta -= OnColumnHeaderResize;
			_headerGripper.DragCompleted -= OnColumnHeaderGripperDragCompleted;
			_headerGripper.MouseDoubleClick -= OnGripperDoubleClicked;
			_headerGripper.MouseEnter -= OnGripperMouseEnterLeave;
			_headerGripper.MouseLeave -= OnGripperMouseEnterLeave;
			_headerGripper = null;
		}
	}

	private Cursor GetCursor(int cursorID)
	{
		Invariant.Assert(cursorID == 100 || cursorID == 101, "incorrect cursor type");
		Cursor result = null;
		Stream stream = null;
		Assembly assembly = GetType().Assembly;
		switch (cursorID)
		{
		case 100:
			stream = assembly.GetManifestResourceStream("split.cur");
			break;
		case 101:
			stream = assembly.GetManifestResourceStream("splitopen.cur");
			break;
		}
		if (stream != null)
		{
			result = new Cursor(stream);
		}
		return result;
	}

	private void UpdateGripperCursor()
	{
		if (_headerGripper != null && !_headerGripper.IsDragging)
		{
			Cursor cursor = ((!DoubleUtil.IsZero(base.ActualWidth)) ? SplitCursor : SplitOpenCursor);
			if (cursor != null)
			{
				_headerGripper.Cursor = cursor;
			}
		}
	}

	private bool IsMouseOutside()
	{
		Point position = Mouse.PrimaryDevice.GetPosition(this);
		if (position.X >= 0.0 && position.X <= base.ActualWidth && position.Y >= 0.0)
		{
			return !(position.Y <= base.ActualHeight);
		}
		return true;
	}

	private void ClickImplement()
	{
		if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
		{
			UIElementAutomationPeer.CreatePeerForElement(this)?.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
		}
		base.OnClick();
	}

	private bool GetFlag(Flags flag)
	{
		return (_flags & flag) == flag;
	}

	private void SetFlag(Flags flag, bool set)
	{
		if (set)
		{
			_flags |= flag;
		}
		else
		{
			_flags &= ~flag;
		}
	}

	private void UpdateFloatingHeaderCanvas()
	{
		if (_floatingHeaderCanvas != null && FloatSourceHeader != null)
		{
			Vector offset = VisualTreeHelper.GetOffset(FloatSourceHeader);
			VisualBrush visualBrush = new VisualBrush(FloatSourceHeader);
			visualBrush.ViewboxUnits = BrushMappingMode.Absolute;
			visualBrush.Viewbox = new Rect(offset.X, offset.Y, FloatSourceHeader.ActualWidth, FloatSourceHeader.ActualHeight);
			_floatingHeaderCanvas.Background = visualBrush;
			FloatSourceHeader = null;
		}
	}

	private bool HandleIsMouseOverChanged()
	{
		if (base.ClickMode == ClickMode.Hover)
		{
			if (base.IsMouseOver && (_headerGripper == null || !_headerGripper.IsMouseOver))
			{
				SetValue(ButtonBase.IsPressedPropertyKey, BooleanBoxes.Box(value: true));
				OnClick();
			}
			else
			{
				ClearValue(ButtonBase.IsPressedPropertyKey);
			}
			return true;
		}
		return false;
	}

	private void OnGripperMouseEnterLeave(object sender, MouseEventArgs e)
	{
		HandleIsMouseOverChanged();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.GridViewColumnHeader" /> class.</summary>
	public GridViewColumnHeader()
	{
	}
}
