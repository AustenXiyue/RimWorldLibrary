using System.ComponentModel;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;
using MS.Internal;
using MS.Internal.Commands;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls.Primitives;

/// <summary>Represents a control that provides a scroll bar that has a sliding <see cref="T:System.Windows.Controls.Primitives.Thumb" /> whose position corresponds to a value.</summary>
[Localizability(LocalizationCategory.NeverLocalize)]
[TemplatePart(Name = "PART_Track", Type = typeof(Track))]
public class ScrollBar : RangeBase
{
	/// <summary>Identifies the <see cref="E:System.Windows.Controls.Primitives.ScrollBar.Scroll" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="E:System.Windows.Controls.Primitives.ScrollBar.Scroll" /> routed event.</returns>
	public static readonly RoutedEvent ScrollEvent;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.ScrollBar.Orientation" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.ScrollBar.Orientation" /> dependency property.</returns>
	public static readonly DependencyProperty OrientationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.Primitives.ScrollBar.ViewportSize" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.Primitives.ScrollBar.ViewportSize" /> dependency property.</returns>
	public static readonly DependencyProperty ViewportSizeProperty;

	/// <summary>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a small amount in the vertical direction of decreasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </summary>
	/// <returns>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a small amount in the vertical direction of decreasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </returns>
	public static readonly RoutedCommand LineUpCommand;

	/// <summary>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a small amount in the vertical direction of increasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </summary>
	/// <returns>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a small amount in the vertical direction of increasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </returns>
	public static readonly RoutedCommand LineDownCommand;

	/// <summary>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a small amount in the horizontal direction of decreasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </summary>
	/// <returns>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a small amount in the horizontal direction of decreasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </returns>
	public static readonly RoutedCommand LineLeftCommand;

	/// <summary>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a small amount in the horizontal direction of increasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </summary>
	/// <returns>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a small amount in the horizontal direction of increasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </returns>
	public static readonly RoutedCommand LineRightCommand;

	/// <summary>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a large amount in the vertical direction of decreasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </summary>
	/// <returns>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a large amount in the vertical direction of decreasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </returns>
	public static readonly RoutedCommand PageUpCommand;

	/// <summary>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a large amount in the vertical direction of increasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </summary>
	/// <returns>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a large amount in the vertical direction of increasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </returns>
	public static readonly RoutedCommand PageDownCommand;

	/// <summary>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a large amount in the horizontal direction of decreasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </summary>
	/// <returns>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a large amount in the horizontal direction of decreasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </returns>
	public static readonly RoutedCommand PageLeftCommand;

	/// <summary>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a large amount in the horizontal direction of increasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </summary>
	/// <returns>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> by a large amount in the horizontal direction of increasing value of its <see cref="T:System.Windows.Controls.Primitives.Track" />. </returns>
	public static readonly RoutedCommand PageRightCommand;

	/// <summary>The command that scrolls the content to the lower-right corner of a <see cref="T:System.Windows.Controls.ScrollViewer" /> control. </summary>
	/// <returns>The command that scrolls the content to the lower-right corner of a <see cref="T:System.Windows.Controls.ScrollViewer" /> control. </returns>
	public static readonly RoutedCommand ScrollToEndCommand;

	/// <summary>The command that scrolls the content to the upper-left corner of a <see cref="T:System.Windows.Controls.ScrollViewer" /> control. </summary>
	/// <returns>The command that scrolls the content to the upper-left corner of a <see cref="T:System.Windows.Controls.ScrollViewer" /> control. </returns>
	public static readonly RoutedCommand ScrollToHomeCommand;

	/// <summary>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> to the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> value for a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />. </summary>
	/// <returns>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> to the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> value for a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />. </returns>
	public static readonly RoutedCommand ScrollToRightEndCommand;

	/// <summary>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> to the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> value for a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />. </summary>
	/// <returns>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> to the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Minimum" /> value for a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />. </returns>
	public static readonly RoutedCommand ScrollToLeftEndCommand;

	/// <summary>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> to the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> value for a vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />. </summary>
	/// <returns>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> to the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> value for a vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />. </returns>
	public static readonly RoutedCommand ScrollToTopCommand;

	/// <summary>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> to the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> value. </summary>
	/// <returns>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> to the <see cref="P:System.Windows.Controls.Primitives.RangeBase.Maximum" /> value. </returns>
	public static readonly RoutedCommand ScrollToBottomCommand;

	/// <summary>The command that scrolls a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> in a <see cref="T:System.Windows.Controls.ScrollViewer" /> to the value that is provided in <see cref="P:System.Windows.Input.ExecutedRoutedEventArgs.Parameter" />. </summary>
	/// <returns>The command that scrolls a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> in a <see cref="T:System.Windows.Controls.ScrollViewer" /> to the value that is provided in <see cref="P:System.Windows.Input.ExecutedRoutedEventArgs.Parameter" />. </returns>
	public static readonly RoutedCommand ScrollToHorizontalOffsetCommand;

	/// <summary>The command that scrolls a vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> in a <see cref="T:System.Windows.Controls.ScrollViewer" /> to the value that is provided in <see cref="P:System.Windows.Input.ExecutedRoutedEventArgs.Parameter" />. </summary>
	/// <returns>The command that scrolls a vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> in a <see cref="T:System.Windows.Controls.ScrollViewer" /> to the value that is provided in <see cref="P:System.Windows.Input.ExecutedRoutedEventArgs.Parameter" />. </returns>
	public static readonly RoutedCommand ScrollToVerticalOffsetCommand;

	/// <summary>The command that notifies the <see cref="T:System.Windows.Controls.ScrollViewer" /> that the user is dragging the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> of the horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> to the value that is provided in <see cref="P:System.Windows.Input.ExecutedRoutedEventArgs.Parameter" />.  </summary>
	/// <returns>The command that occurs when the user drags the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> of a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> on a <see cref="T:System.Windows.Controls.ScrollViewer" /> that has deferred scrolling enabled. </returns>
	public static readonly RoutedCommand DeferScrollToHorizontalOffsetCommand;

	/// <summary>The command that notifies the <see cref="T:System.Windows.Controls.ScrollViewer" /> that the user is dragging the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> of the vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> to the value that is provided in <see cref="P:System.Windows.Input.ExecutedRoutedEventArgs.Parameter" />.  </summary>
	/// <returns>The command that notifies the <see cref="T:System.Windows.Controls.ScrollViewer" /> that the user is dragging the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> of the vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> to the value that is provided in <see cref="P:System.Windows.Input.ExecutedRoutedEventArgs.Parameter" />.  </returns>
	public static readonly RoutedCommand DeferScrollToVerticalOffsetCommand;

	/// <summary>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> to the point of the mouse click that opened the <see cref="T:System.Windows.Controls.ContextMenu" /> in the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />. </summary>
	/// <returns>The command that scrolls a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> to the point of the mouse click that opened the <see cref="T:System.Windows.Controls.ContextMenu" /> in the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />. </returns>
	public static readonly RoutedCommand ScrollHereCommand;

	private const double MaxPerpendicularDelta = 150.0;

	private const string TrackName = "PART_Track";

	private Track _track;

	private Point _latestRightButtonClickPoint = new Point(-1.0, -1.0);

	private bool _canScroll = true;

	private bool _hasScrolled;

	private bool _isStandalone = true;

	private bool _openingContextMenu;

	private double _previousValue;

	private Vector _thumbOffset;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets whether the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> is displayed horizontally or vertically.  </summary>
	/// <returns>An <see cref="T:System.Windows.Controls.Orientation" /> enumeration value that defines whether the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> is displayed horizontally or vertically. The default is <see cref="F:System.Windows.Controls.Orientation.Vertical" />.</returns>
	public Orientation Orientation
	{
		get
		{
			return (Orientation)GetValue(OrientationProperty);
		}
		set
		{
			SetValue(OrientationProperty, value);
		}
	}

	/// <summary>Gets or sets the amount of the scrollable content that is currently visible.  </summary>
	/// <returns>The amount of the scrollable content that is currently visible. The default is 0.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public double ViewportSize
	{
		get
		{
			return (double)GetValue(ViewportSizeProperty);
		}
		set
		{
			SetValue(ViewportSizeProperty, value);
		}
	}

	/// <summary>Gets the <see cref="T:System.Windows.Controls.Primitives.Track" /> for a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> control.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.Primitives.Track" /> that is used with a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> control.</returns>
	public Track Track => _track;

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> is enabled.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> is enabled in a <see cref="T:System.Windows.Controls.ScrollViewer" /> and the size of the content is larger than the display area; otherwise, false. The default is true.</returns>
	protected override bool IsEnabledCore
	{
		get
		{
			if (base.IsEnabledCore)
			{
				return _canScroll;
			}
			return false;
		}
	}

	private IInputElement CommandTarget
	{
		get
		{
			IInputElement inputElement = base.TemplatedParent as IInputElement;
			if (inputElement == null)
			{
				inputElement = this;
			}
			return inputElement;
		}
	}

	internal bool IsStandalone
	{
		get
		{
			return _isStandalone;
		}
		set
		{
			_isStandalone = value;
		}
	}

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	private static ContextMenu VerticalContextMenu => new ContextMenu
	{
		Items = 
		{
			(object)CreateMenuItem("ScrollBar_ContextMenu_ScrollHere", "ScrollHere", ScrollHereCommand),
			(object)new Separator(),
			(object)CreateMenuItem("ScrollBar_ContextMenu_Top", "Top", ScrollToTopCommand),
			(object)CreateMenuItem("ScrollBar_ContextMenu_Bottom", "Bottom", ScrollToBottomCommand),
			(object)new Separator(),
			(object)CreateMenuItem("ScrollBar_ContextMenu_PageUp", "PageUp", PageUpCommand),
			(object)CreateMenuItem("ScrollBar_ContextMenu_PageDown", "PageDown", PageDownCommand),
			(object)new Separator(),
			(object)CreateMenuItem("ScrollBar_ContextMenu_ScrollUp", "ScrollUp", LineUpCommand),
			(object)CreateMenuItem("ScrollBar_ContextMenu_ScrollDown", "ScrollDown", LineDownCommand)
		}
	};

	private static ContextMenu HorizontalContextMenuLTR => new ContextMenu
	{
		Items = 
		{
			(object)CreateMenuItem("ScrollBar_ContextMenu_ScrollHere", "ScrollHere", ScrollHereCommand),
			(object)new Separator(),
			(object)CreateMenuItem("ScrollBar_ContextMenu_LeftEdge", "LeftEdge", ScrollToLeftEndCommand),
			(object)CreateMenuItem("ScrollBar_ContextMenu_RightEdge", "RightEdge", ScrollToRightEndCommand),
			(object)new Separator(),
			(object)CreateMenuItem("ScrollBar_ContextMenu_PageLeft", "PageLeft", PageLeftCommand),
			(object)CreateMenuItem("ScrollBar_ContextMenu_PageRight", "PageRight", PageRightCommand),
			(object)new Separator(),
			(object)CreateMenuItem("ScrollBar_ContextMenu_ScrollLeft", "ScrollLeft", LineLeftCommand),
			(object)CreateMenuItem("ScrollBar_ContextMenu_ScrollRight", "ScrollRight", LineRightCommand)
		}
	};

	private static ContextMenu HorizontalContextMenuRTL => new ContextMenu
	{
		Items = 
		{
			(object)CreateMenuItem("ScrollBar_ContextMenu_ScrollHere", "ScrollHere", ScrollHereCommand),
			(object)new Separator(),
			(object)CreateMenuItem("ScrollBar_ContextMenu_LeftEdge", "LeftEdge", ScrollToRightEndCommand),
			(object)CreateMenuItem("ScrollBar_ContextMenu_RightEdge", "RightEdge", ScrollToLeftEndCommand),
			(object)new Separator(),
			(object)CreateMenuItem("ScrollBar_ContextMenu_PageLeft", "PageLeft", PageRightCommand),
			(object)CreateMenuItem("ScrollBar_ContextMenu_PageRight", "PageRight", PageLeftCommand),
			(object)new Separator(),
			(object)CreateMenuItem("ScrollBar_ContextMenu_ScrollLeft", "ScrollLeft", LineRightCommand),
			(object)CreateMenuItem("ScrollBar_ContextMenu_ScrollRight", "ScrollRight", LineLeftCommand)
		}
	};

	internal override int EffectiveValuesInitialSize => 42;

	/// <summary>Occurs one or more times as content scrolls in a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> when the user moves the <see cref="P:System.Windows.Controls.Primitives.Track.Thumb" /> by using the mouse.</summary>
	[Category("Behavior")]
	public event ScrollEventHandler Scroll
	{
		add
		{
			AddHandler(ScrollEvent, value);
		}
		remove
		{
			RemoveHandler(ScrollEvent, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> class.</summary>
	public ScrollBar()
	{
	}

	/// <summary>Creates an <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> for this <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> control.</summary>
	/// <returns>A <see cref="T:System.Windows.Automation.Peers.ScrollBarAutomationPeer" /> for the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> control.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ScrollBarAutomationPeer(this);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.PreviewMouseLeftButtonDown" /> event.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		_thumbOffset = default(Vector);
		if (Track != null && Track.IsMouseOver && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
		{
			Point position = e.MouseDevice.GetPosition(Track);
			double num = Track.ValueFromPoint(position);
			if (Shape.IsDoubleFinite(num))
			{
				ChangeValue(num, defer: false);
			}
			if (Track.Thumb != null && Track.Thumb.IsMouseOver)
			{
				Point position2 = e.MouseDevice.GetPosition(Track.Thumb);
				_thumbOffset = position2 - new Point(Track.Thumb.ActualWidth * 0.5, Track.Thumb.ActualHeight * 0.5);
			}
			else
			{
				e.Handled = true;
			}
		}
		base.OnPreviewMouseLeftButtonDown(e);
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.UIElement.PreviewMouseRightButtonUp" /> event. </summary>
	/// <param name="e">The event data.</param>
	protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
	{
		if (Track != null)
		{
			_latestRightButtonClickPoint = e.MouseDevice.GetPosition(Track);
		}
		else
		{
			_latestRightButtonClickPoint = new Point(-1.0, -1.0);
		}
		base.OnPreviewMouseRightButtonUp(e);
	}

	/// <summary>Creates the visual tree for the <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		_track = GetTemplateChild("PART_Track") as Track;
	}

	private static void OnThumbDragStarted(object sender, DragStartedEventArgs e)
	{
		if (sender is ScrollBar scrollBar)
		{
			scrollBar._hasScrolled = false;
			scrollBar._previousValue = scrollBar.Value;
		}
	}

	private static void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (sender is ScrollBar scrollBar)
		{
			scrollBar.UpdateValue(e.HorizontalChange + scrollBar._thumbOffset.X, e.VerticalChange + scrollBar._thumbOffset.Y);
		}
	}

	private void UpdateValue(double horizontalDragDelta, double verticalDragDelta)
	{
		if (Track == null)
		{
			return;
		}
		double num = Track.ValueFromDistance(horizontalDragDelta, verticalDragDelta);
		if (Shape.IsDoubleFinite(num) && !DoubleUtil.IsZero(num))
		{
			double value = base.Value;
			double num2 = value + num;
			double value2 = ((Orientation != 0) ? Math.Abs(horizontalDragDelta) : Math.Abs(verticalDragDelta));
			if (DoubleUtil.GreaterThan(value2, 150.0))
			{
				num2 = _previousValue;
			}
			if (!DoubleUtil.AreClose(value, num2))
			{
				_hasScrolled = true;
				ChangeValue(num2, defer: true);
				RaiseScrollEvent(ScrollEventType.ThumbTrack);
			}
		}
	}

	private static void OnThumbDragCompleted(object sender, DragCompletedEventArgs e)
	{
		((ScrollBar)sender).OnThumbDragCompleted(e);
	}

	private void OnThumbDragCompleted(DragCompletedEventArgs e)
	{
		if (_hasScrolled)
		{
			FinishDrag();
			RaiseScrollEvent(ScrollEventType.EndScroll);
		}
	}

	private void FinishDrag()
	{
		double value = base.Value;
		IInputElement commandTarget = CommandTarget;
		if (((Orientation == Orientation.Horizontal) ? DeferScrollToHorizontalOffsetCommand : DeferScrollToVerticalOffsetCommand).CanExecute(value, commandTarget))
		{
			ChangeValue(value, defer: false);
		}
	}

	private void ChangeValue(double newValue, bool defer)
	{
		newValue = Math.Min(Math.Max(newValue, base.Minimum), base.Maximum);
		if (IsStandalone)
		{
			base.Value = newValue;
			return;
		}
		IInputElement commandTarget = CommandTarget;
		RoutedCommand routedCommand = null;
		bool flag = Orientation == Orientation.Horizontal;
		if (defer)
		{
			routedCommand = (flag ? DeferScrollToHorizontalOffsetCommand : DeferScrollToVerticalOffsetCommand);
			if (routedCommand.CanExecute(newValue, commandTarget))
			{
				routedCommand.Execute(newValue, commandTarget);
			}
			else
			{
				routedCommand = null;
			}
		}
		if (routedCommand == null)
		{
			routedCommand = (flag ? ScrollToHorizontalOffsetCommand : ScrollToVerticalOffsetCommand);
			if (routedCommand.CanExecute(newValue, commandTarget))
			{
				routedCommand.Execute(newValue, commandTarget);
			}
		}
	}

	internal void ScrollToLastMousePoint()
	{
		Point point = new Point(-1.0, -1.0);
		if (Track != null && _latestRightButtonClickPoint != point)
		{
			double num = Track.ValueFromPoint(_latestRightButtonClickPoint);
			if (Shape.IsDoubleFinite(num))
			{
				ChangeValue(num, defer: false);
				_latestRightButtonClickPoint = point;
				RaiseScrollEvent(ScrollEventType.ThumbPosition);
			}
		}
	}

	internal void RaiseScrollEvent(ScrollEventType scrollEventType)
	{
		ScrollEventArgs scrollEventArgs = new ScrollEventArgs(scrollEventType, base.Value);
		scrollEventArgs.Source = this;
		RaiseEvent(scrollEventArgs);
	}

	private static void OnScrollCommand(object target, ExecutedRoutedEventArgs args)
	{
		ScrollBar scrollBar = (ScrollBar)target;
		if (args.Command == ScrollHereCommand)
		{
			scrollBar.ScrollToLastMousePoint();
		}
		if (!scrollBar.IsStandalone)
		{
			return;
		}
		if (scrollBar.Orientation == Orientation.Vertical)
		{
			if (args.Command == LineUpCommand)
			{
				scrollBar.LineUp();
			}
			else if (args.Command == LineDownCommand)
			{
				scrollBar.LineDown();
			}
			else if (args.Command == PageUpCommand)
			{
				scrollBar.PageUp();
			}
			else if (args.Command == PageDownCommand)
			{
				scrollBar.PageDown();
			}
			else if (args.Command == ScrollToTopCommand)
			{
				scrollBar.ScrollToTop();
			}
			else if (args.Command == ScrollToBottomCommand)
			{
				scrollBar.ScrollToBottom();
			}
		}
		else if (args.Command == LineLeftCommand)
		{
			scrollBar.LineLeft();
		}
		else if (args.Command == LineRightCommand)
		{
			scrollBar.LineRight();
		}
		else if (args.Command == PageLeftCommand)
		{
			scrollBar.PageLeft();
		}
		else if (args.Command == PageRightCommand)
		{
			scrollBar.PageRight();
		}
		else if (args.Command == ScrollToLeftEndCommand)
		{
			scrollBar.ScrollToLeftEnd();
		}
		else if (args.Command == ScrollToRightEndCommand)
		{
			scrollBar.ScrollToRightEnd();
		}
	}

	private void SmallDecrement()
	{
		double num = Math.Max(base.Value - base.SmallChange, base.Minimum);
		if (base.Value != num)
		{
			base.Value = num;
			RaiseScrollEvent(ScrollEventType.SmallDecrement);
		}
	}

	private void SmallIncrement()
	{
		double num = Math.Min(base.Value + base.SmallChange, base.Maximum);
		if (base.Value != num)
		{
			base.Value = num;
			RaiseScrollEvent(ScrollEventType.SmallIncrement);
		}
	}

	private void LargeDecrement()
	{
		double num = Math.Max(base.Value - base.LargeChange, base.Minimum);
		if (base.Value != num)
		{
			base.Value = num;
			RaiseScrollEvent(ScrollEventType.LargeDecrement);
		}
	}

	private void LargeIncrement()
	{
		double num = Math.Min(base.Value + base.LargeChange, base.Maximum);
		if (base.Value != num)
		{
			base.Value = num;
			RaiseScrollEvent(ScrollEventType.LargeIncrement);
		}
	}

	private void ToMinimum()
	{
		if (base.Value != base.Minimum)
		{
			base.Value = base.Minimum;
			RaiseScrollEvent(ScrollEventType.First);
		}
	}

	private void ToMaximum()
	{
		if (base.Value != base.Maximum)
		{
			base.Value = base.Maximum;
			RaiseScrollEvent(ScrollEventType.Last);
		}
	}

	private void LineUp()
	{
		SmallDecrement();
	}

	private void LineDown()
	{
		SmallIncrement();
	}

	private void PageUp()
	{
		LargeDecrement();
	}

	private void PageDown()
	{
		LargeIncrement();
	}

	private void ScrollToTop()
	{
		ToMinimum();
	}

	private void ScrollToBottom()
	{
		ToMaximum();
	}

	private void LineLeft()
	{
		SmallDecrement();
	}

	private void LineRight()
	{
		SmallIncrement();
	}

	private void PageLeft()
	{
		LargeDecrement();
	}

	private void PageRight()
	{
		LargeIncrement();
	}

	private void ScrollToLeftEnd()
	{
		ToMinimum();
	}

	private void ScrollToRightEnd()
	{
		ToMaximum();
	}

	private static void OnQueryScrollHereCommand(object target, CanExecuteRoutedEventArgs args)
	{
		args.CanExecute = args.Command == ScrollHereCommand;
	}

	private static void OnQueryScrollCommand(object target, CanExecuteRoutedEventArgs args)
	{
		args.CanExecute = ((ScrollBar)target).IsStandalone;
	}

	static ScrollBar()
	{
		ScrollEvent = EventManager.RegisterRoutedEvent("Scroll", RoutingStrategy.Bubble, typeof(ScrollEventHandler), typeof(ScrollBar));
		OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(ScrollBar), new FrameworkPropertyMetadata(Orientation.Vertical), IsValidOrientation);
		ViewportSizeProperty = DependencyProperty.Register("ViewportSize", typeof(double), typeof(ScrollBar), new FrameworkPropertyMetadata(0.0), Shape.IsDoubleFiniteNonNegative);
		LineUpCommand = new RoutedCommand("LineUp", typeof(ScrollBar));
		LineDownCommand = new RoutedCommand("LineDown", typeof(ScrollBar));
		LineLeftCommand = new RoutedCommand("LineLeft", typeof(ScrollBar));
		LineRightCommand = new RoutedCommand("LineRight", typeof(ScrollBar));
		PageUpCommand = new RoutedCommand("PageUp", typeof(ScrollBar));
		PageDownCommand = new RoutedCommand("PageDown", typeof(ScrollBar));
		PageLeftCommand = new RoutedCommand("PageLeft", typeof(ScrollBar));
		PageRightCommand = new RoutedCommand("PageRight", typeof(ScrollBar));
		ScrollToEndCommand = new RoutedCommand("ScrollToEnd", typeof(ScrollBar));
		ScrollToHomeCommand = new RoutedCommand("ScrollToHome", typeof(ScrollBar));
		ScrollToRightEndCommand = new RoutedCommand("ScrollToRightEnd", typeof(ScrollBar));
		ScrollToLeftEndCommand = new RoutedCommand("ScrollToLeftEnd", typeof(ScrollBar));
		ScrollToTopCommand = new RoutedCommand("ScrollToTop", typeof(ScrollBar));
		ScrollToBottomCommand = new RoutedCommand("ScrollToBottom", typeof(ScrollBar));
		ScrollToHorizontalOffsetCommand = new RoutedCommand("ScrollToHorizontalOffset", typeof(ScrollBar));
		ScrollToVerticalOffsetCommand = new RoutedCommand("ScrollToVerticalOffset", typeof(ScrollBar));
		DeferScrollToHorizontalOffsetCommand = new RoutedCommand("DeferScrollToToHorizontalOffset", typeof(ScrollBar));
		DeferScrollToVerticalOffsetCommand = new RoutedCommand("DeferScrollToVerticalOffset", typeof(ScrollBar));
		ScrollHereCommand = new RoutedCommand("ScrollHere", typeof(ScrollBar));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ScrollBar), new FrameworkPropertyMetadata(typeof(ScrollBar)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ScrollBar));
		ExecutedRoutedEventHandler executedRoutedEventHandler = OnScrollCommand;
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = OnQueryScrollCommand;
		UIElement.FocusableProperty.OverrideMetadata(typeof(ScrollBar), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		EventManager.RegisterClassHandler(typeof(ScrollBar), Thumb.DragStartedEvent, new DragStartedEventHandler(OnThumbDragStarted));
		EventManager.RegisterClassHandler(typeof(ScrollBar), Thumb.DragDeltaEvent, new DragDeltaEventHandler(OnThumbDragDelta));
		EventManager.RegisterClassHandler(typeof(ScrollBar), Thumb.DragCompletedEvent, new DragCompletedEventHandler(OnThumbDragCompleted));
		CommandHelpers.RegisterCommandHandler(typeof(ScrollBar), ScrollHereCommand, executedRoutedEventHandler, OnQueryScrollHereCommand);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollBar), LineUpCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Up);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollBar), LineDownCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Down);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollBar), PageUpCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Prior);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollBar), PageDownCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Next);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollBar), ScrollToTopCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.Home, ModifierKeys.Control));
		CommandHelpers.RegisterCommandHandler(typeof(ScrollBar), ScrollToBottomCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler, new KeyGesture(Key.End, ModifierKeys.Control));
		CommandHelpers.RegisterCommandHandler(typeof(ScrollBar), LineLeftCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Left);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollBar), LineRightCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Right);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollBar), PageLeftCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollBar), PageRightCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollBar), ScrollToLeftEndCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.Home);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollBar), ScrollToRightEndCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler, Key.End);
		RangeBase.MaximumProperty.OverrideMetadata(typeof(ScrollBar), new FrameworkPropertyMetadata(ViewChanged));
		RangeBase.MinimumProperty.OverrideMetadata(typeof(ScrollBar), new FrameworkPropertyMetadata(ViewChanged));
		FrameworkElement.ContextMenuProperty.OverrideMetadata(typeof(ScrollBar), new FrameworkPropertyMetadata(null, CoerceContextMenu));
		ControlsTraceLogger.AddControl(TelemetryControls.ScrollBar);
	}

	private static void ViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ScrollBar scrollBar = (ScrollBar)d;
		bool flag = scrollBar.Maximum > scrollBar.Minimum;
		if (flag != scrollBar._canScroll)
		{
			scrollBar._canScroll = flag;
			scrollBar.CoerceValue(UIElement.IsEnabledProperty);
		}
	}

	internal static bool IsValidOrientation(object o)
	{
		Orientation orientation = (Orientation)o;
		if (orientation != 0)
		{
			return orientation == Orientation.Vertical;
		}
		return true;
	}

	private static object CoerceContextMenu(DependencyObject o, object value)
	{
		ScrollBar scrollBar = (ScrollBar)o;
		if (scrollBar._openingContextMenu && scrollBar.GetValueSource(FrameworkElement.ContextMenuProperty, null, out var hasModifiers) == BaseValueSourceInternal.Default && !hasModifiers)
		{
			if (scrollBar.Orientation == Orientation.Vertical)
			{
				return VerticalContextMenu;
			}
			if (scrollBar.FlowDirection == FlowDirection.LeftToRight)
			{
				return HorizontalContextMenuLTR;
			}
			return HorizontalContextMenuRTL;
		}
		return value;
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.FrameworkElement.ContextMenuOpening" /> event that occurs when the <see cref="T:System.Windows.Controls.ContextMenu" /> for a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> opens.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnContextMenuOpening(ContextMenuEventArgs e)
	{
		base.OnContextMenuOpening(e);
		if (!e.Handled)
		{
			_openingContextMenu = true;
			CoerceValue(FrameworkElement.ContextMenuProperty);
		}
	}

	/// <summary>Provides class handling for the <see cref="E:System.Windows.FrameworkElement.ContextMenuClosing" /> event that occurs when the <see cref="T:System.Windows.Controls.ContextMenu" /> for a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> closes.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnContextMenuClosing(ContextMenuEventArgs e)
	{
		base.OnContextMenuClosing(e);
		_openingContextMenu = false;
		CoerceValue(FrameworkElement.ContextMenuProperty);
	}

	private static MenuItem CreateMenuItem(string name, string automationId, RoutedCommand command)
	{
		MenuItem obj = new MenuItem
		{
			Header = SR.GetResourceString(name),
			Command = command
		};
		AutomationProperties.SetAutomationId(obj, automationId);
		obj.SetBinding(binding: new Binding
		{
			Path = new PropertyPath(ContextMenu.PlacementTargetProperty),
			Mode = BindingMode.OneWay,
			RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(ContextMenu), 1)
		}, dp: MenuItem.CommandTargetProperty);
		return obj;
	}
}
