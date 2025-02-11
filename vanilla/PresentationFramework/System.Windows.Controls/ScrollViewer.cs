using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MS.Internal;
using MS.Internal.Commands;
using MS.Internal.Documents;
using MS.Internal.KnownBoxes;
using MS.Internal.PresentationFramework;
using MS.Internal.Telemetry.PresentationFramework;
using MS.Utility;

namespace System.Windows.Controls;

/// <summary>Represents a scrollable area that can contain other visible elements. </summary>
[DefaultEvent("ScrollChangedEvent")]
[Localizability(LocalizationCategory.Ignore)]
[TemplatePart(Name = "PART_HorizontalScrollBar", Type = typeof(ScrollBar))]
[TemplatePart(Name = "PART_VerticalScrollBar", Type = typeof(ScrollBar))]
[TemplatePart(Name = "PART_ScrollContentPresenter", Type = typeof(ScrollContentPresenter))]
public class ScrollViewer : ContentControl
{
	private class PanningInfo
	{
		public const double PrePanTranslation = 3.0;

		public const double MaxInertiaBoundaryTranslation = 50.0;

		public const double PreFeedbackTranslationX = 8.0;

		public const double PreFeedbackTranslationY = 5.0;

		public const int InertiaBoundryMinimumTicks = 100;

		public PanningMode PanningMode { get; set; }

		public double OriginalHorizontalOffset { get; set; }

		public double OriginalVerticalOffset { get; set; }

		public double DeltaPerHorizontalOffet { get; set; }

		public double DeltaPerVerticalOffset { get; set; }

		public bool IsPanning { get; set; }

		public Vector UnusedTranslation { get; set; }

		public bool InHorizontalFeedback { get; set; }

		public bool InVerticalFeedback { get; set; }

		public int InertiaBoundaryBeginTimestamp { get; set; }
	}

	private enum Commands
	{
		Invalid,
		LineUp,
		LineDown,
		LineLeft,
		LineRight,
		PageUp,
		PageDown,
		PageLeft,
		PageRight,
		SetHorizontalOffset,
		SetVerticalOffset,
		MakeVisible
	}

	private struct Command
	{
		internal Commands Code;

		internal double Param;

		internal MakeVisibleParams MakeVisibleParam;

		internal Command(Commands code, double param, MakeVisibleParams mvp)
		{
			Code = code;
			Param = param;
			MakeVisibleParam = mvp;
		}
	}

	private class MakeVisibleParams
	{
		internal Visual Child;

		internal Rect TargetRect;

		internal MakeVisibleParams(Visual child, Rect targetRect)
		{
			Child = child;
			TargetRect = targetRect;
		}
	}

	private struct CommandQueue
	{
		private const int _capacity = 32;

		private int _lastWritePosition;

		private int _lastReadPosition;

		private Command[] _array;

		internal void Enqueue(Command command)
		{
			if (_lastWritePosition == _lastReadPosition)
			{
				_array = new Command[32];
				_lastWritePosition = (_lastReadPosition = 0);
			}
			if (!OptimizeCommand(command))
			{
				_lastWritePosition = (_lastWritePosition + 1) % 32;
				if (_lastWritePosition == _lastReadPosition)
				{
					_lastReadPosition = (_lastReadPosition + 1) % 32;
				}
				_array[_lastWritePosition] = command;
			}
		}

		private bool OptimizeCommand(Command command)
		{
			if (_lastWritePosition != _lastReadPosition && ((command.Code == Commands.SetHorizontalOffset && _array[_lastWritePosition].Code == Commands.SetHorizontalOffset) || (command.Code == Commands.SetVerticalOffset && _array[_lastWritePosition].Code == Commands.SetVerticalOffset) || (command.Code == Commands.MakeVisible && _array[_lastWritePosition].Code == Commands.MakeVisible)))
			{
				_array[_lastWritePosition].Param = command.Param;
				_array[_lastWritePosition].MakeVisibleParam = command.MakeVisibleParam;
				return true;
			}
			return false;
		}

		internal Command Fetch()
		{
			if (_lastWritePosition == _lastReadPosition)
			{
				return new Command(Commands.Invalid, 0.0, null);
			}
			_lastReadPosition = (_lastReadPosition + 1) % 32;
			Command result = _array[_lastReadPosition];
			_array[_lastReadPosition].MakeVisibleParam = null;
			if (_lastWritePosition == _lastReadPosition)
			{
				_array = null;
			}
			return result;
		}

		internal bool IsEmpty()
		{
			return _lastWritePosition == _lastReadPosition;
		}
	}

	[Flags]
	private enum Flags
	{
		None = 0,
		InvalidatedMeasureFromArrange = 1,
		InChildInvalidateMeasure = 2,
		HandlesMouseWheelScrolling = 4,
		ForceNextManipulationComplete = 8,
		ManipulationBindingsInitialized = 0x10,
		CompleteScrollManipulation = 0x20,
		InChildMeasurePass1 = 0x40,
		InChildMeasurePass2 = 0x80,
		InChildMeasurePass3 = 0xC0
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.CanContentScroll" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.CanContentScroll" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty CanContentScrollProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.HorizontalScrollBarVisibility" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.HorizontalScrollBarVisibility" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.VerticalScrollBarVisibility" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.VerticalScrollBarVisibility" /> dependency property.</returns>
	[CommonDependencyProperty]
	public static readonly DependencyProperty VerticalScrollBarVisibilityProperty;

	private static readonly DependencyPropertyKey ComputedHorizontalScrollBarVisibilityPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.ComputedHorizontalScrollBarVisibility" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.ComputedHorizontalScrollBarVisibility" /> dependency property.</returns>
	public static readonly DependencyProperty ComputedHorizontalScrollBarVisibilityProperty;

	private static readonly DependencyPropertyKey ComputedVerticalScrollBarVisibilityPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.ComputedVerticalScrollBarVisibility" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.ComputedVerticalScrollBarVisibility" /> dependency property.</returns>
	public static readonly DependencyProperty ComputedVerticalScrollBarVisibilityProperty;

	private static readonly DependencyPropertyKey VerticalOffsetPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.VerticalOffset" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.VerticalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty VerticalOffsetProperty;

	private static readonly DependencyPropertyKey HorizontalOffsetPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.HorizontalOffset" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.HorizontalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty HorizontalOffsetProperty;

	private static readonly DependencyPropertyKey ContentVerticalOffsetPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.ContentVerticalOffset" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.ContentVerticalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty ContentVerticalOffsetProperty;

	private static readonly DependencyPropertyKey ContentHorizontalOffsetPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.ContentHorizontalOffset" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.ContentHorizontalOffset" /> dependency property.</returns>
	public static readonly DependencyProperty ContentHorizontalOffsetProperty;

	private static readonly DependencyPropertyKey ExtentWidthPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.ExtentWidth" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.ExtentWidth" /> dependency property.</returns>
	public static readonly DependencyProperty ExtentWidthProperty;

	private static readonly DependencyPropertyKey ExtentHeightPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.ExtentHeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.ExtentHeight" /> dependency property.</returns>
	public static readonly DependencyProperty ExtentHeightProperty;

	private static readonly DependencyPropertyKey ScrollableWidthPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.ScrollableWidth" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.ScrollableWidth" /> dependency property.</returns>
	public static readonly DependencyProperty ScrollableWidthProperty;

	private static readonly DependencyPropertyKey ScrollableHeightPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.ScrollableHeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.ScrollableHeight" /> dependency property.</returns>
	public static readonly DependencyProperty ScrollableHeightProperty;

	private static readonly DependencyPropertyKey ViewportWidthPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.ViewportWidth" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.ViewportWidth" /> dependency property.</returns>
	public static readonly DependencyProperty ViewportWidthProperty;

	internal static readonly DependencyPropertyKey ViewportHeightPropertyKey;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.ViewportHeight" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.ViewportHeight" /> dependency property.</returns>
	public static readonly DependencyProperty ViewportHeightProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.IsDeferredScrollingEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.IsDeferredScrollingEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsDeferredScrollingEnabledProperty;

	/// <summary>Identifies the <see cref="E:System.Windows.Controls.ScrollViewer.ScrollChanged" /> routed event. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.ScrollableWidth" /> routed event.</returns>
	public static readonly RoutedEvent ScrollChangedEvent;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.PanningMode" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.PanningMode" /> dependency property.</returns>
	public static readonly DependencyProperty PanningModeProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.PanningDeceleration" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.PanningDeceleration" /> dependency property.</returns>
	public static readonly DependencyProperty PanningDecelerationProperty;

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollViewer.PanningRatio" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollViewer.PanningRatio" /> dependency property.</returns>
	public static readonly DependencyProperty PanningRatioProperty;

	private bool _seenTapGesture;

	internal const double _scrollLineDelta = 16.0;

	internal const double _mouseWheelDelta = 48.0;

	private const string HorizontalScrollBarTemplateName = "PART_HorizontalScrollBar";

	private const string VerticalScrollBarTemplateName = "PART_VerticalScrollBar";

	internal const string ScrollContentPresenterTemplateName = "PART_ScrollContentPresenter";

	private Visibility _scrollVisibilityX;

	private Visibility _scrollVisibilityY;

	private double _xPositionISI;

	private double _yPositionISI;

	private double _xExtent;

	private double _yExtent;

	private double _xSize;

	private double _ySize;

	private EventHandler _layoutUpdatedHandler;

	private IScrollInfo _scrollInfo;

	private CommandQueue _queue;

	private PanningInfo _panningInfo;

	private Flags _flags = Flags.HandlesMouseWheelScrolling;

	private static DependencyObjectType _dType;

	/// <summary>Gets or sets a value that indicates whether elements that support the <see cref="T:System.Windows.Controls.Primitives.IScrollInfo" /> interface are allowed to scroll.  </summary>
	/// <returns>true if the <see cref="T:System.Windows.Controls.ScrollViewer" /> scrolls in terms of logical units; false if the <see cref="T:System.Windows.Controls.ScrollViewer" /> scrolls in terms of physical units. The default is false.</returns>
	public bool CanContentScroll
	{
		get
		{
			return (bool)GetValue(CanContentScrollProperty);
		}
		set
		{
			SetValue(CanContentScrollProperty, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> should be displayed.  </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.ScrollBarVisibility" /> value that indicates whether a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> should be displayed. The default is <see cref="F:System.Windows.Controls.ScrollBarVisibility.Hidden" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
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

	/// <summary>Gets or sets a value that indicates whether a vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> should be displayed.  </summary>
	/// <returns>A <see cref="T:System.Windows.Controls.ScrollBarVisibility" /> value that indicates whether a vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> should be displayed. The default is <see cref="F:System.Windows.Controls.ScrollBarVisibility.Visible" />.</returns>
	[Bindable(true)]
	[Category("Appearance")]
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

	/// <summary>Gets a value that indicates whether the horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> is visible.  </summary>
	/// <returns>A <see cref="T:System.Windows.Visibility" /> that indicates whether the horizontal scroll bar is visible. The default is <see cref="F:System.Windows.Controls.ScrollBarVisibility.Hidden" />.</returns>
	public Visibility ComputedHorizontalScrollBarVisibility => _scrollVisibilityX;

	/// <summary>Gets a value that indicates whether the vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar" /> is visible.   </summary>
	/// <returns>A <see cref="T:System.Windows.Visibility" /> that indicates whether the vertical scroll bar is visible. The default is <see cref="F:System.Windows.Controls.ScrollBarVisibility.Visible" />.</returns>
	public Visibility ComputedVerticalScrollBarVisibility => _scrollVisibilityY;

	/// <summary>Gets a value that contains the horizontal offset of the scrolled content.  </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the horizontal offset of the scrolled content. The default is 0.0.</returns>
	public double HorizontalOffset
	{
		get
		{
			return _xPositionISI;
		}
		private set
		{
			SetValue(HorizontalOffsetPropertyKey, value);
		}
	}

	/// <summary>Gets a value that contains the vertical offset of the scrolled content.  </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the vertical offset of the scrolled content. The default is 0.0.</returns>
	public double VerticalOffset
	{
		get
		{
			return _yPositionISI;
		}
		private set
		{
			SetValue(VerticalOffsetPropertyKey, value);
		}
	}

	/// <summary>Gets a value that contains the horizontal size of the extent.  </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the horizontal size of the extent. The default is 0.0.</returns>
	[Category("Layout")]
	public double ExtentWidth => _xExtent;

	/// <summary>Gets a value that contains the vertical size of the extent.  </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the vertical size of the extent. The default is 0.0.</returns>
	[Category("Layout")]
	public double ExtentHeight => _yExtent;

	/// <summary>Gets a value that represents the horizontal size of the content element that can be scrolled.  </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the horizontal size of the content element that can be scrolled. This property has no default value.</returns>
	public double ScrollableWidth => Math.Max(0.0, ExtentWidth - ViewportWidth);

	/// <summary>Gets a value that represents the vertical size of the content element that can be scrolled.  </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the vertical size of the content element that can be scrolled. This property has no default value.</returns>
	public double ScrollableHeight => Math.Max(0.0, ExtentHeight - ViewportHeight);

	/// <summary>Gets a value that contains the horizontal size of the content's viewport.  </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the horizontal size of the content's viewport. The default is 0.0.</returns>
	[Category("Layout")]
	public double ViewportWidth => _xSize;

	/// <summary>Gets a value that contains the vertical size of the content's viewport.  </summary>
	/// <returns>A <see cref="T:System.Double" /> that represents the vertical size of the content's viewport. This property has no default value.</returns>
	[Category("Layout")]
	public double ViewportHeight => _ySize;

	/// <summary>Gets the vertical offset of the visible content.</summary>
	/// <returns>The vertical offset of the visible content.</returns>
	public double ContentVerticalOffset
	{
		get
		{
			return (double)GetValue(ContentVerticalOffsetProperty);
		}
		private set
		{
			SetValue(ContentVerticalOffsetPropertyKey, value);
		}
	}

	/// <summary>Gets the horizontal offset of the visible content.</summary>
	/// <returns>The horizontal offset of the visible content.</returns>
	public double ContentHorizontalOffset
	{
		get
		{
			return (double)GetValue(ContentHorizontalOffsetProperty);
		}
		private set
		{
			SetValue(ContentHorizontalOffsetPropertyKey, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the content is stationary when the user drags the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> of a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />.</summary>
	/// <returns>true if the content is stationary when the user drags the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> of a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />; otherwise, false.</returns>
	public bool IsDeferredScrollingEnabled
	{
		get
		{
			return (bool)GetValue(IsDeferredScrollingEnabledProperty);
		}
		set
		{
			SetValue(IsDeferredScrollingEnabledProperty, BooleanBoxes.Box(value));
		}
	}

	/// <summary>Gets a value that indicates that a control has a <see cref="T:System.Windows.Controls.ScrollViewer" /> defined in its style that defines custom keyboard scrolling behavior.</summary>
	/// <returns>true if this control defines custom keyboard scrolling behavior; otherwise, false.</returns>
	protected internal override bool HandlesScrolling => true;

	/// <summary>Gets or sets the element that implements the <see cref="T:System.Windows.Controls.Primitives.IScrollInfo" /> interface and provides values for scrolling properties of this <see cref="T:System.Windows.Controls.ScrollViewer" />. </summary>
	/// <returns>The element that controls scrolling properties, such as extent, offset, or viewport size. This property has no default value.</returns>
	protected internal IScrollInfo ScrollInfo
	{
		get
		{
			return _scrollInfo;
		}
		set
		{
			_scrollInfo = value;
			if (_scrollInfo != null)
			{
				_scrollInfo.CanHorizontallyScroll = HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled;
				_scrollInfo.CanVerticallyScroll = VerticalScrollBarVisibility != ScrollBarVisibility.Disabled;
				EnsureQueueProcessing();
			}
		}
	}

	/// <summary>Gets or sets the way <see cref="T:System.Windows.Controls.ScrollViewer" /> reacts to touch manipulation.</summary>
	/// <returns>A value that specifies how <see cref="T:System.Windows.Controls.ScrollViewer" /> reacts to touch manipulation.  The default is <see cref="F:System.Windows.Controls.PanningMode.None" />.</returns>
	public PanningMode PanningMode
	{
		get
		{
			return (PanningMode)GetValue(PanningModeProperty);
		}
		set
		{
			SetValue(PanningModeProperty, value);
		}
	}

	/// <summary>Gets or sets the rate <see cref="T:System.Windows.Controls.ScrollViewer" /> slows in device-independent units (1/96th inch per unit) per squared millisecond when in inertia.</summary>
	/// <returns>The rate <see cref="T:System.Windows.Controls.ScrollViewer" /> slows in device-independent units (1/96th inch per unit) per squared millisecond.</returns>
	public double PanningDeceleration
	{
		get
		{
			return (double)GetValue(PanningDecelerationProperty);
		}
		set
		{
			SetValue(PanningDecelerationProperty, value);
		}
	}

	/// <summary>Gets or sets the ratio of scrolling offset to translate manipulation offset.</summary>
	/// <returns>The ratio of scrolling offset to translate manipulation offset. The default is 1.</returns>
	public double PanningRatio
	{
		get
		{
			return (double)GetValue(PanningRatioProperty);
		}
		set
		{
			SetValue(PanningRatioProperty, value);
		}
	}

	internal bool HandlesMouseWheelScrolling
	{
		get
		{
			return (_flags & Flags.HandlesMouseWheelScrolling) == Flags.HandlesMouseWheelScrolling;
		}
		set
		{
			SetFlagValue(Flags.HandlesMouseWheelScrolling, value);
		}
	}

	internal bool InChildInvalidateMeasure
	{
		get
		{
			return (_flags & Flags.InChildInvalidateMeasure) == Flags.InChildInvalidateMeasure;
		}
		set
		{
			SetFlagValue(Flags.InChildInvalidateMeasure, value);
		}
	}

	private bool InvalidatedMeasureFromArrange
	{
		get
		{
			return (_flags & Flags.InvalidatedMeasureFromArrange) == Flags.InvalidatedMeasureFromArrange;
		}
		set
		{
			SetFlagValue(Flags.InvalidatedMeasureFromArrange, value);
		}
	}

	private bool ForceNextManipulationComplete
	{
		get
		{
			return (_flags & Flags.ForceNextManipulationComplete) == Flags.ForceNextManipulationComplete;
		}
		set
		{
			SetFlagValue(Flags.ForceNextManipulationComplete, value);
		}
	}

	private bool ManipulationBindingsInitialized
	{
		get
		{
			return (_flags & Flags.ManipulationBindingsInitialized) == Flags.ManipulationBindingsInitialized;
		}
		set
		{
			SetFlagValue(Flags.ManipulationBindingsInitialized, value);
		}
	}

	private bool CompleteScrollManipulation
	{
		get
		{
			return (_flags & Flags.CompleteScrollManipulation) == Flags.CompleteScrollManipulation;
		}
		set
		{
			SetFlagValue(Flags.CompleteScrollManipulation, value);
		}
	}

	internal bool InChildMeasurePass1
	{
		get
		{
			return (_flags & Flags.InChildMeasurePass1) == Flags.InChildMeasurePass1;
		}
		set
		{
			SetFlagValue(Flags.InChildMeasurePass1, value);
		}
	}

	internal bool InChildMeasurePass2
	{
		get
		{
			return (_flags & Flags.InChildMeasurePass2) == Flags.InChildMeasurePass2;
		}
		set
		{
			SetFlagValue(Flags.InChildMeasurePass2, value);
		}
	}

	internal bool InChildMeasurePass3
	{
		get
		{
			return (_flags & Flags.InChildMeasurePass3) == Flags.InChildMeasurePass3;
		}
		set
		{
			SetFlagValue(Flags.InChildMeasurePass3, value);
		}
	}

	internal override int EffectiveValuesInitialSize => 28;

	internal override DependencyObjectType DTypeThemeStyleKey => _dType;

	/// <summary>Occurs when changes are detected to the scroll position, extent, or viewport size.</summary>
	[Category("Action")]
	public event ScrollChangedEventHandler ScrollChanged
	{
		add
		{
			AddHandler(ScrollChangedEvent, value);
		}
		remove
		{
			RemoveHandler(ScrollChangedEvent, value);
		}
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.ScrollViewer" /> content upward by one line. </summary>
	public void LineUp()
	{
		EnqueueCommand(Commands.LineUp, 0.0, null);
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.ScrollViewer" /> content downward by one line. </summary>
	public void LineDown()
	{
		EnqueueCommand(Commands.LineDown, 0.0, null);
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.ScrollViewer" /> content to the left by a predetermined amount. </summary>
	public void LineLeft()
	{
		EnqueueCommand(Commands.LineLeft, 0.0, null);
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.ScrollViewer" /> content to the right by a predetermined amount. </summary>
	public void LineRight()
	{
		EnqueueCommand(Commands.LineRight, 0.0, null);
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.ScrollViewer" /> content upward by one page. </summary>
	public void PageUp()
	{
		EnqueueCommand(Commands.PageUp, 0.0, null);
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.ScrollViewer" /> content downward by one page. </summary>
	public void PageDown()
	{
		EnqueueCommand(Commands.PageDown, 0.0, null);
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.ScrollViewer" /> content to the left by one page. </summary>
	public void PageLeft()
	{
		EnqueueCommand(Commands.PageLeft, 0.0, null);
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.ScrollViewer" /> content to the right by one page. </summary>
	public void PageRight()
	{
		EnqueueCommand(Commands.PageRight, 0.0, null);
	}

	/// <summary>Scrolls horizontally to the beginning of the <see cref="T:System.Windows.Controls.ScrollViewer" /> content. </summary>
	public void ScrollToLeftEnd()
	{
		EnqueueCommand(Commands.SetHorizontalOffset, double.NegativeInfinity, null);
	}

	/// <summary>Scrolls horizontally to the end of the <see cref="T:System.Windows.Controls.ScrollViewer" /> content. </summary>
	public void ScrollToRightEnd()
	{
		EnqueueCommand(Commands.SetHorizontalOffset, double.PositiveInfinity, null);
	}

	/// <summary>Scrolls vertically to the beginning of the <see cref="T:System.Windows.Controls.ScrollViewer" /> content. </summary>
	public void ScrollToHome()
	{
		EnqueueCommand(Commands.SetHorizontalOffset, double.NegativeInfinity, null);
		EnqueueCommand(Commands.SetVerticalOffset, double.NegativeInfinity, null);
	}

	/// <summary>Scrolls vertically to the end of the <see cref="T:System.Windows.Controls.ScrollViewer" /> content. </summary>
	public void ScrollToEnd()
	{
		EnqueueCommand(Commands.SetHorizontalOffset, double.NegativeInfinity, null);
		EnqueueCommand(Commands.SetVerticalOffset, double.PositiveInfinity, null);
	}

	/// <summary>Scrolls vertically to the beginning of the <see cref="T:System.Windows.Controls.ScrollViewer" /> content. </summary>
	public void ScrollToTop()
	{
		EnqueueCommand(Commands.SetVerticalOffset, double.NegativeInfinity, null);
	}

	/// <summary>Scrolls vertically to the end of the <see cref="T:System.Windows.Controls.ScrollViewer" /> content.</summary>
	public void ScrollToBottom()
	{
		EnqueueCommand(Commands.SetVerticalOffset, double.PositiveInfinity, null);
	}

	/// <summary>Scrolls the content within the <see cref="T:System.Windows.Controls.ScrollViewer" /> to the specified horizontal offset position.</summary>
	/// <param name="offset">The position that the content scrolls to.</param>
	public void ScrollToHorizontalOffset(double offset)
	{
		double param = ScrollContentPresenter.ValidateInputOffset(offset, "offset");
		EnqueueCommand(Commands.SetHorizontalOffset, param, null);
	}

	/// <summary>Scrolls the content within the <see cref="T:System.Windows.Controls.ScrollViewer" /> to the specified vertical offset position.</summary>
	/// <param name="offset">The position that the content scrolls to.</param>
	public void ScrollToVerticalOffset(double offset)
	{
		double param = ScrollContentPresenter.ValidateInputOffset(offset, "offset");
		EnqueueCommand(Commands.SetVerticalOffset, param, null);
	}

	private void DeferScrollToHorizontalOffset(double offset)
	{
		double horizontalOffset = ScrollContentPresenter.ValidateInputOffset(offset, "offset");
		HorizontalOffset = horizontalOffset;
	}

	private void DeferScrollToVerticalOffset(double offset)
	{
		double verticalOffset = ScrollContentPresenter.ValidateInputOffset(offset, "offset");
		VerticalOffset = verticalOffset;
	}

	internal void MakeVisible(Visual child, Rect rect)
	{
		MakeVisibleParams mvp = new MakeVisibleParams(child, rect);
		EnqueueCommand(Commands.MakeVisible, 0.0, mvp);
	}

	private void EnsureLayoutUpdatedHandler()
	{
		if (_layoutUpdatedHandler == null)
		{
			_layoutUpdatedHandler = OnLayoutUpdated;
			base.LayoutUpdated += _layoutUpdatedHandler;
		}
		InvalidateArrange();
	}

	private void ClearLayoutUpdatedHandler()
	{
		if (_layoutUpdatedHandler != null && _queue.IsEmpty())
		{
			base.LayoutUpdated -= _layoutUpdatedHandler;
			_layoutUpdatedHandler = null;
		}
	}

	/// <summary>Called by an <see cref="T:System.Windows.Controls.Primitives.IScrollInfo" /> interface that is attached to a <see cref="T:System.Windows.Controls.ScrollViewer" /> when the value of any scrolling property size changes. Scrolling properties include offset, extent, or viewport. </summary>
	public void InvalidateScrollInfo()
	{
		if (ScrollInfo == null)
		{
			return;
		}
		if (!base.MeasureInProgress && (!base.ArrangeInProgress || !InvalidatedMeasureFromArrange))
		{
			double extentWidth = ScrollInfo.ExtentWidth;
			double viewportWidth = ScrollInfo.ViewportWidth;
			if (HorizontalScrollBarVisibility == ScrollBarVisibility.Auto && ((_scrollVisibilityX == Visibility.Collapsed && DoubleUtil.GreaterThan(extentWidth, viewportWidth)) || (_scrollVisibilityX == Visibility.Visible && DoubleUtil.LessThanOrClose(extentWidth, viewportWidth))))
			{
				InvalidateMeasure();
			}
			else
			{
				extentWidth = ScrollInfo.ExtentHeight;
				viewportWidth = ScrollInfo.ViewportHeight;
				if (VerticalScrollBarVisibility == ScrollBarVisibility.Auto && ((_scrollVisibilityY == Visibility.Collapsed && DoubleUtil.GreaterThan(extentWidth, viewportWidth)) || (_scrollVisibilityY == Visibility.Visible && DoubleUtil.LessThanOrClose(extentWidth, viewportWidth))))
				{
					InvalidateMeasure();
				}
			}
		}
		if (!DoubleUtil.AreClose(HorizontalOffset, ScrollInfo.HorizontalOffset) || !DoubleUtil.AreClose(VerticalOffset, ScrollInfo.VerticalOffset) || !DoubleUtil.AreClose(ViewportWidth, ScrollInfo.ViewportWidth) || !DoubleUtil.AreClose(ViewportHeight, ScrollInfo.ViewportHeight) || !DoubleUtil.AreClose(ExtentWidth, ScrollInfo.ExtentWidth) || !DoubleUtil.AreClose(ExtentHeight, ScrollInfo.ExtentHeight))
		{
			EnsureLayoutUpdatedHandler();
		}
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ScrollViewer.CanContentScroll" /> dependency property to a given element.</summary>
	/// <param name="element">The element on which to set the property value.</param>
	/// <param name="canContentScroll">The property value to set.</param>
	public static void SetCanContentScroll(DependencyObject element, bool canContentScroll)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(CanContentScrollProperty, canContentScroll);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ScrollViewer.CanContentScroll" /> dependency property from a given element.</summary>
	/// <returns>true if this element can scroll; otherwise, false.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	public static bool GetCanContentScroll(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(CanContentScrollProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ScrollViewer.HorizontalScrollBarVisibility" /> dependency property to a given element.</summary>
	/// <param name="element">The element on which to set the property value.</param>
	/// <param name="horizontalScrollBarVisibility">The property value to set.</param>
	public static void SetHorizontalScrollBarVisibility(DependencyObject element, ScrollBarVisibility horizontalScrollBarVisibility)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(HorizontalScrollBarVisibilityProperty, horizontalScrollBarVisibility);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ScrollViewer.HorizontalScrollBarVisibility" /> dependency property from a given element.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Controls.ScrollViewer.HorizontalScrollBarVisibility" /> dependency property.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	public static ScrollBarVisibility GetHorizontalScrollBarVisibility(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (ScrollBarVisibility)element.GetValue(HorizontalScrollBarVisibilityProperty);
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.ScrollViewer.VerticalScrollBarVisibility" /> dependency property to a given element.</summary>
	/// <param name="element">The element on which to set the property value.</param>
	/// <param name="verticalScrollBarVisibility">The property value to set.</param>
	public static void SetVerticalScrollBarVisibility(DependencyObject element, ScrollBarVisibility verticalScrollBarVisibility)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(VerticalScrollBarVisibilityProperty, verticalScrollBarVisibility);
	}

	/// <summary>Gets the value of the <see cref="P:System.Windows.Controls.ScrollViewer.VerticalScrollBarVisibility" /> dependency property from a given element.</summary>
	/// <returns>The value of the <see cref="P:System.Windows.Controls.ScrollViewer.VerticalScrollBarVisibility" />  dependency property.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	public static ScrollBarVisibility GetVerticalScrollBarVisibility(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (ScrollBarVisibility)element.GetValue(VerticalScrollBarVisibilityProperty);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.ScrollViewer.IsDeferredScrollingEnabled" /> property for the specified object.</summary>
	/// <returns>true if the content is stationary when the user drags the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> of a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />; otherwise, false.</returns>
	/// <param name="element">The object from which to get <see cref="P:System.Windows.Controls.ScrollViewer.IsDeferredScrollingEnabled" />.</param>
	public static bool GetIsDeferredScrollingEnabled(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (bool)element.GetValue(IsDeferredScrollingEnabledProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Controls.ScrollViewer.IsDeferredScrollingEnabled" /> property for the specified object.</summary>
	/// <param name="element">The object on which to set the <see cref="P:System.Windows.Controls.ScrollViewer.IsDeferredScrollingEnabled" /> property.</param>
	/// <param name="value">true to have the content remain stationary when the user drags the <see cref="T:System.Windows.Controls.Primitives.Thumb" /> of a <see cref="T:System.Windows.Controls.Primitives.ScrollBar" />; otherwise, false.</param>
	public static void SetIsDeferredScrollingEnabled(DependencyObject element, bool value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(IsDeferredScrollingEnabledProperty, BooleanBoxes.Box(value));
	}

	protected override void OnStylusSystemGesture(StylusSystemGestureEventArgs e)
	{
		_seenTapGesture = e.SystemGesture == SystemGesture.Tap;
	}

	/// <summary>Called when a change in scrolling state is detected, such as a change in scroll position, extent, or viewport size.</summary>
	/// <param name="e">The <see cref="T:System.Windows.Controls.ScrollChangedEventArgs" /> that contain information about the change in the scrolling state.</param>
	protected virtual void OnScrollChanged(ScrollChangedEventArgs e)
	{
		RaiseEvent(e);
	}

	/// <summary>Performs a hit test to determine whether the specified points are within the bounds of this <see cref="T:System.Windows.Controls.ScrollViewer" />.</summary>
	/// <returns>The result of the hit test.</returns>
	/// <param name="hitTestParameters">The parameters for hit testing within a visual object.</param>
	protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
	{
		if (new Rect(0.0, 0.0, base.ActualWidth, base.ActualHeight).Contains(hitTestParameters.HitPoint))
		{
			return new PointHitTestResult(this, hitTestParameters.HitPoint);
		}
		return null;
	}

	/// <summary>Responds to specific keyboard input and invokes associated scrolling behavior.</summary>
	/// <param name="e">Required arguments for this event.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.Handled || base.TemplatedParent is Control { HandlesScrolling: not false })
		{
			return;
		}
		if (e.OriginalSource == this)
		{
			ScrollInDirection(e);
		}
		else if (e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Up || e.Key == Key.Down)
		{
			if (!(GetTemplateChild("PART_ScrollContentPresenter") is ScrollContentPresenter scrollContentPresenter))
			{
				ScrollInDirection(e);
				return;
			}
			FocusNavigationDirection direction = KeyboardNavigation.KeyToTraversalDirection(e.Key);
			DependencyObject dependencyObject = null;
			DependencyObject dependencyObject2 = Keyboard.FocusedElement as DependencyObject;
			if (IsInViewport(scrollContentPresenter, dependencyObject2))
			{
				if (dependencyObject2 is UIElement uIElement)
				{
					dependencyObject = uIElement.PredictFocus(direction);
				}
				else if (dependencyObject2 is ContentElement contentElement)
				{
					dependencyObject = contentElement.PredictFocus(direction);
				}
				else if (dependencyObject2 is UIElement3D uIElement3D)
				{
					dependencyObject = uIElement3D.PredictFocus(direction);
				}
			}
			else
			{
				dependencyObject = scrollContentPresenter.PredictFocus(direction);
			}
			if (dependencyObject == null)
			{
				ScrollInDirection(e);
				return;
			}
			if (IsInViewport(scrollContentPresenter, dependencyObject))
			{
				((IInputElement)dependencyObject).Focus();
				e.Handled = true;
				return;
			}
			ScrollInDirection(e);
			UpdateLayout();
			if (IsInViewport(scrollContentPresenter, dependencyObject))
			{
				((IInputElement)dependencyObject).Focus();
			}
		}
		else
		{
			ScrollInDirection(e);
		}
	}

	private bool IsInViewport(ScrollContentPresenter scp, DependencyObject element)
	{
		Visual visualRoot = KeyboardNavigation.GetVisualRoot(scp);
		Visual visualRoot2 = KeyboardNavigation.GetVisualRoot(element);
		while (visualRoot != visualRoot2)
		{
			if (visualRoot2 == null)
			{
				return false;
			}
			if (!(visualRoot2 is FrameworkElement frameworkElement))
			{
				return false;
			}
			element = frameworkElement.Parent;
			if (element == null)
			{
				return false;
			}
			visualRoot2 = KeyboardNavigation.GetVisualRoot(element);
		}
		Rect rectangle = KeyboardNavigation.GetRectangle(scp);
		Rect rectangle2 = KeyboardNavigation.GetRectangle(element);
		return rectangle.IntersectsWith(rectangle2);
	}

	internal void ScrollInDirection(KeyEventArgs e)
	{
		bool flag = (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0;
		if ((e.KeyboardDevice.Modifiers & ModifierKeys.Alt) != 0)
		{
			return;
		}
		bool flag2 = base.FlowDirection == FlowDirection.RightToLeft;
		switch (e.Key)
		{
		case Key.Left:
			if (flag2)
			{
				LineRight();
			}
			else
			{
				LineLeft();
			}
			e.Handled = true;
			break;
		case Key.Right:
			if (flag2)
			{
				LineLeft();
			}
			else
			{
				LineRight();
			}
			e.Handled = true;
			break;
		case Key.Up:
			LineUp();
			e.Handled = true;
			break;
		case Key.Down:
			LineDown();
			e.Handled = true;
			break;
		case Key.Prior:
			PageUp();
			e.Handled = true;
			break;
		case Key.Next:
			PageDown();
			e.Handled = true;
			break;
		case Key.Home:
			if (flag)
			{
				ScrollToTop();
			}
			else
			{
				ScrollToLeftEnd();
			}
			e.Handled = true;
			break;
		case Key.End:
			if (flag)
			{
				ScrollToBottom();
			}
			else
			{
				ScrollToRightEnd();
			}
			e.Handled = true;
			break;
		}
	}

	/// <summary>Responds to a click of the mouse wheel.</summary>
	/// <param name="e">Required arguments that describe this event.</param>
	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		if (e.Handled || !HandlesMouseWheelScrolling)
		{
			return;
		}
		if (ScrollInfo != null)
		{
			if (e.Delta < 0)
			{
				ScrollInfo.MouseWheelDown();
			}
			else
			{
				ScrollInfo.MouseWheelUp();
			}
		}
		e.Handled = true;
	}

	/// <summary>Responds to a click of the left mouse button. </summary>
	/// <param name="e">Required arguments that describe this event.</param>
	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		if (Focus())
		{
			e.Handled = true;
		}
		base.OnMouseLeftButtonDown(e);
	}

	/// <summary>Measures the content of a <see cref="T:System.Windows.Controls.ScrollViewer" /> element.</summary>
	/// <returns>The computed desired limit <see cref="T:System.Windows.Size" /> of the <see cref="T:System.Windows.Controls.ScrollViewer" /> element.</returns>
	/// <param name="constraint">The upper limit <see cref="T:System.Windows.Size" /> that should not be exceeded.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		InChildInvalidateMeasure = false;
		IScrollInfo scrollInfo = ScrollInfo;
		UIElement uIElement = ((VisualChildrenCount > 0) ? (GetVisualChild(0) as UIElement) : null);
		ScrollBarVisibility verticalScrollBarVisibility = VerticalScrollBarVisibility;
		ScrollBarVisibility horizontalScrollBarVisibility = HorizontalScrollBarVisibility;
		Size result = default(Size);
		if (uIElement != null)
		{
			bool flag = EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info);
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringBegin, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "SCROLLVIEWER:MeasureOverride");
			}
			try
			{
				bool flag2 = verticalScrollBarVisibility == ScrollBarVisibility.Auto;
				bool flag3 = horizontalScrollBarVisibility == ScrollBarVisibility.Auto;
				bool flag4 = verticalScrollBarVisibility == ScrollBarVisibility.Disabled;
				bool flag5 = horizontalScrollBarVisibility == ScrollBarVisibility.Disabled;
				Visibility visibility = ((verticalScrollBarVisibility != ScrollBarVisibility.Visible) ? Visibility.Collapsed : Visibility.Visible);
				Visibility visibility2 = ((horizontalScrollBarVisibility != ScrollBarVisibility.Visible) ? Visibility.Collapsed : Visibility.Visible);
				if (_scrollVisibilityY != visibility)
				{
					_scrollVisibilityY = visibility;
					SetValue(ComputedVerticalScrollBarVisibilityPropertyKey, _scrollVisibilityY);
				}
				if (_scrollVisibilityX != visibility2)
				{
					_scrollVisibilityX = visibility2;
					SetValue(ComputedHorizontalScrollBarVisibilityPropertyKey, _scrollVisibilityX);
				}
				if (scrollInfo != null)
				{
					scrollInfo.CanHorizontallyScroll = !flag5;
					scrollInfo.CanVerticallyScroll = !flag4;
				}
				try
				{
					InChildMeasurePass1 = true;
					uIElement.Measure(constraint);
				}
				finally
				{
					InChildMeasurePass1 = false;
				}
				scrollInfo = ScrollInfo;
				if (scrollInfo != null && (flag3 || flag2))
				{
					bool flag6 = flag3 && DoubleUtil.GreaterThan(scrollInfo.ExtentWidth, scrollInfo.ViewportWidth);
					bool flag7 = flag2 && DoubleUtil.GreaterThan(scrollInfo.ExtentHeight, scrollInfo.ViewportHeight);
					if (flag6 && _scrollVisibilityX != 0)
					{
						_scrollVisibilityX = Visibility.Visible;
						SetValue(ComputedHorizontalScrollBarVisibilityPropertyKey, _scrollVisibilityX);
					}
					if (flag7 && _scrollVisibilityY != 0)
					{
						_scrollVisibilityY = Visibility.Visible;
						SetValue(ComputedVerticalScrollBarVisibilityPropertyKey, _scrollVisibilityY);
					}
					if (flag6 || flag7)
					{
						InChildInvalidateMeasure = true;
						uIElement.InvalidateMeasure();
						try
						{
							InChildMeasurePass2 = true;
							uIElement.Measure(constraint);
						}
						finally
						{
							InChildMeasurePass2 = false;
						}
					}
					if (flag3 && flag2 && flag6 != flag7)
					{
						bool num = !flag6 && DoubleUtil.GreaterThan(scrollInfo.ExtentWidth, scrollInfo.ViewportWidth);
						bool flag8 = !flag7 && DoubleUtil.GreaterThan(scrollInfo.ExtentHeight, scrollInfo.ViewportHeight);
						if (num)
						{
							if (_scrollVisibilityX != 0)
							{
								_scrollVisibilityX = Visibility.Visible;
								SetValue(ComputedHorizontalScrollBarVisibilityPropertyKey, _scrollVisibilityX);
							}
						}
						else if (flag8 && _scrollVisibilityY != 0)
						{
							_scrollVisibilityY = Visibility.Visible;
							SetValue(ComputedVerticalScrollBarVisibilityPropertyKey, _scrollVisibilityY);
						}
						if (num || flag8)
						{
							InChildInvalidateMeasure = true;
							uIElement.InvalidateMeasure();
							try
							{
								InChildMeasurePass3 = true;
								uIElement.Measure(constraint);
							}
							finally
							{
								InChildMeasurePass3 = false;
							}
						}
					}
				}
			}
			finally
			{
				if (flag)
				{
					EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringEnd, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "SCROLLVIEWER:MeasureOverride");
				}
			}
			result = uIElement.DesiredSize;
		}
		if (!base.ArrangeDirty && InvalidatedMeasureFromArrange)
		{
			InvalidatedMeasureFromArrange = false;
		}
		return result;
	}

	/// <summary>Arranges the content of the <see cref="T:System.Windows.Controls.ScrollViewer" />.</summary>
	/// <param name="arrangeSize">The final area within the parent that this element should use to arrange itself and its children.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		bool invalidatedMeasureFromArrange = InvalidatedMeasureFromArrange;
		Size result = base.ArrangeOverride(arrangeSize);
		if (invalidatedMeasureFromArrange)
		{
			InvalidatedMeasureFromArrange = false;
		}
		else
		{
			InvalidatedMeasureFromArrange = base.MeasureDirty;
		}
		return result;
	}

	private void BindToTemplatedParent(DependencyProperty property)
	{
		if (!HasNonDefaultValue(property))
		{
			Binding binding = new Binding();
			binding.RelativeSource = RelativeSource.TemplatedParent;
			binding.Path = new PropertyPath(property);
			SetBinding(property, binding);
		}
	}

	internal override void OnPreApplyTemplate()
	{
		base.OnPreApplyTemplate();
		if (base.TemplatedParent != null)
		{
			BindToTemplatedParent(HorizontalScrollBarVisibilityProperty);
			BindToTemplatedParent(VerticalScrollBarVisibilityProperty);
			BindToTemplatedParent(CanContentScrollProperty);
			BindToTemplatedParent(IsDeferredScrollingEnabledProperty);
			BindToTemplatedParent(PanningModeProperty);
		}
	}

	/// <summary>Called when an internal process or application calls <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />, which is used to build the current template's visual tree.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		if (GetTemplateChild("PART_HorizontalScrollBar") is ScrollBar scrollBar)
		{
			scrollBar.IsStandalone = false;
		}
		if (GetTemplateChild("PART_VerticalScrollBar") is ScrollBar scrollBar2)
		{
			scrollBar2.IsStandalone = false;
		}
		OnPanningModeChanged();
	}

	/// <summary>Sets the <see cref="P:System.Windows.Controls.ScrollViewer.PanningMode" /> property for the specified object.</summary>
	/// <param name="element">The object on which to set the <see cref="P:System.Windows.Controls.ScrollViewer.PanningMode" /> property.</param>
	/// <param name="panningMode">A value that specifies how <see cref="T:System.Windows.Controls.ScrollViewer" /> reacts to touch manipulation.</param>
	public static void SetPanningMode(DependencyObject element, PanningMode panningMode)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(PanningModeProperty, panningMode);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.ScrollViewer.PanningMode" /> property for the specified object.</summary>
	/// <returns>A value that specifies how <see cref="T:System.Windows.Controls.ScrollViewer" /> reacts to touch manipulation.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	public static PanningMode GetPanningMode(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (PanningMode)element.GetValue(PanningModeProperty);
	}

	private static void OnPanningModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ScrollViewer scrollViewer)
		{
			scrollViewer.OnPanningModeChanged();
		}
	}

	private void OnPanningModeChanged()
	{
		if (base.HasTemplateGeneratedSubTree)
		{
			PanningMode panningMode = PanningMode;
			InvalidateProperty(UIElement.IsManipulationEnabledProperty);
			if (panningMode != 0)
			{
				SetCurrentValueInternal(UIElement.IsManipulationEnabledProperty, BooleanBoxes.TrueBox);
			}
		}
	}

	/// <summary>Sets the <see cref="P:System.Windows.Controls.ScrollViewer.PanningDeceleration" /> property for the specified object.</summary>
	/// <param name="element">The object on which to set the <see cref="P:System.Windows.Controls.ScrollViewer.PanningDeceleration" /> property.</param>
	/// <param name="value">The rate <see cref="T:System.Windows.Controls.ScrollViewer" /> slows in device-independent units (1/96th inch per unit) per squared millisecond.</param>
	public static void SetPanningDeceleration(DependencyObject element, double value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(PanningDecelerationProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.ScrollViewer.PanningDeceleration" /> property for the specified object.</summary>
	/// <returns>The rate <see cref="T:System.Windows.Controls.ScrollViewer" /> slows in device-independent units (1/96th inch per unit) per squared millisecond.</returns>
	/// <param name="element">The element from which the property value is read.</param>
	public static double GetPanningDeceleration(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(PanningDecelerationProperty);
	}

	/// <summary>Sets the <see cref="P:System.Windows.Controls.ScrollViewer.PanningRatio" /> property for the specified object.</summary>
	/// <param name="element">The object on which to set the <see cref="P:System.Windows.Controls.ScrollViewer.PanningRatio" /> property.</param>
	/// <param name="value">The ratio of scrolling offset to translate manipulation offset.</param>
	public static void SetPanningRatio(DependencyObject element, double value)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		element.SetValue(PanningRatioProperty, value);
	}

	/// <summary>Returns the value of the <see cref="P:System.Windows.Controls.ScrollViewer.PanningRatio" /> property for the specified object.</summary>
	/// <returns>The ratio of scrolling offset to translate manipulation offset. </returns>
	/// <param name="element">The element from which the property value is read.</param>
	public static double GetPanningRatio(DependencyObject element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		return (double)element.GetValue(PanningRatioProperty);
	}

	private static bool CheckFiniteNonNegative(object value)
	{
		double num = (double)value;
		if (DoubleUtil.GreaterThanOrClose(num, 0.0))
		{
			return !double.IsInfinity(num);
		}
		return false;
	}

	/// <summary>Called when the <see cref="E:System.Windows.UIElement.ManipulationStarting" /> event occurs.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
	{
		_panningInfo = null;
		_seenTapGesture = false;
		PanningMode panningMode = PanningMode;
		if (panningMode == PanningMode.None)
		{
			return;
		}
		CompleteScrollManipulation = false;
		ScrollContentPresenter scrollContentPresenter = GetTemplateChild("PART_ScrollContentPresenter") as ScrollContentPresenter;
		if (ShouldManipulateScroll(e, scrollContentPresenter))
		{
			switch (panningMode)
			{
			case PanningMode.HorizontalOnly:
				e.Mode = ManipulationModes.TranslateX;
				break;
			case PanningMode.VerticalOnly:
				e.Mode = ManipulationModes.TranslateY;
				break;
			default:
				e.Mode = ManipulationModes.Translate;
				break;
			}
			e.ManipulationContainer = this;
			_panningInfo = new PanningInfo
			{
				OriginalHorizontalOffset = HorizontalOffset,
				OriginalVerticalOffset = VerticalOffset,
				PanningMode = panningMode
			};
			double num = ViewportWidth + 1.0;
			double num2 = ViewportHeight + 1.0;
			if (scrollContentPresenter != null)
			{
				_panningInfo.DeltaPerHorizontalOffet = (DoubleUtil.AreClose(num, 0.0) ? 0.0 : (scrollContentPresenter.ActualWidth / num));
				_panningInfo.DeltaPerVerticalOffset = (DoubleUtil.AreClose(num2, 0.0) ? 0.0 : (scrollContentPresenter.ActualHeight / num2));
			}
			else
			{
				_panningInfo.DeltaPerHorizontalOffet = (DoubleUtil.AreClose(num, 0.0) ? 0.0 : (base.ActualWidth / num));
				_panningInfo.DeltaPerVerticalOffset = (DoubleUtil.AreClose(num2, 0.0) ? 0.0 : (base.ActualHeight / num2));
			}
			if (!ManipulationBindingsInitialized)
			{
				BindToTemplatedParent(PanningDecelerationProperty);
				BindToTemplatedParent(PanningRatioProperty);
				ManipulationBindingsInitialized = true;
			}
		}
		else
		{
			e.Cancel();
			ForceNextManipulationComplete = false;
		}
		e.Handled = true;
	}

	private bool ShouldManipulateScroll(ManipulationStartingEventArgs e, ScrollContentPresenter viewport)
	{
		if (!PresentationSource.UnderSamePresentationSource(e.OriginalSource as DependencyObject, this))
		{
			return false;
		}
		if (viewport == null)
		{
			return true;
		}
		GeneralTransform generalTransform = TransformToDescendant(viewport);
		double actualWidth = viewport.ActualWidth;
		double actualHeight = viewport.ActualHeight;
		foreach (IManipulator manipulator in e.Manipulators)
		{
			Point point = generalTransform.Transform(manipulator.GetPosition(this));
			if (DoubleUtil.LessThan(point.X, 0.0) || DoubleUtil.LessThan(point.Y, 0.0) || DoubleUtil.GreaterThan(point.X, actualWidth) || DoubleUtil.GreaterThan(point.Y, actualHeight))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Called when the <see cref="E:System.Windows.UIElement.ManipulationDelta" /> event occurs.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnManipulationDelta(ManipulationDeltaEventArgs e)
	{
		if (_panningInfo == null)
		{
			return;
		}
		if (e.IsInertial && CompleteScrollManipulation)
		{
			e.Complete();
		}
		else
		{
			bool cancelManipulation = false;
			if (_seenTapGesture)
			{
				e.Cancel();
				_panningInfo = null;
			}
			else if (_panningInfo.IsPanning)
			{
				ManipulateScroll(e);
			}
			else if (CanStartScrollManipulation(e.CumulativeManipulation.Translation, out cancelManipulation))
			{
				_panningInfo.IsPanning = true;
				ManipulateScroll(e);
			}
			else if (cancelManipulation)
			{
				e.Cancel();
				_panningInfo = null;
			}
		}
		e.Handled = true;
	}

	private void ManipulateScroll(ManipulationDeltaEventArgs e)
	{
		PanningMode panningMode = _panningInfo.PanningMode;
		if (panningMode != PanningMode.VerticalOnly)
		{
			ManipulateScroll(e.DeltaManipulation.Translation.X, e.CumulativeManipulation.Translation.X, isHorizontal: true);
		}
		if (panningMode != PanningMode.HorizontalOnly)
		{
			ManipulateScroll(e.DeltaManipulation.Translation.Y, e.CumulativeManipulation.Translation.Y, isHorizontal: false);
		}
		if (e.IsInertial && IsPastInertialLimit())
		{
			e.Complete();
			return;
		}
		double num = _panningInfo.UnusedTranslation.X;
		if (!_panningInfo.InHorizontalFeedback && DoubleUtil.LessThan(Math.Abs(num), 8.0))
		{
			num = 0.0;
		}
		_panningInfo.InHorizontalFeedback = !DoubleUtil.AreClose(num, 0.0);
		double num2 = _panningInfo.UnusedTranslation.Y;
		if (!_panningInfo.InVerticalFeedback && DoubleUtil.LessThan(Math.Abs(num2), 5.0))
		{
			num2 = 0.0;
		}
		_panningInfo.InVerticalFeedback = !DoubleUtil.AreClose(num2, 0.0);
		if (_panningInfo.InHorizontalFeedback || _panningInfo.InVerticalFeedback)
		{
			e.ReportBoundaryFeedback(new ManipulationDelta(new Vector(num, num2), 0.0, new Vector(1.0, 1.0), default(Vector)));
			if (e.IsInertial && _panningInfo.InertiaBoundaryBeginTimestamp == 0)
			{
				_panningInfo.InertiaBoundaryBeginTimestamp = Environment.TickCount;
			}
		}
	}

	private void ManipulateScroll(double delta, double cumulativeTranslation, bool isHorizontal)
	{
		double num = (isHorizontal ? _panningInfo.UnusedTranslation.X : _panningInfo.UnusedTranslation.Y);
		double num2 = (isHorizontal ? HorizontalOffset : VerticalOffset);
		double num3 = (isHorizontal ? ScrollableWidth : ScrollableHeight);
		if (DoubleUtil.AreClose(num3, 0.0))
		{
			num = 0.0;
			delta = 0.0;
		}
		else if ((DoubleUtil.GreaterThanZero(delta) && DoubleUtil.IsZero(num2)) || (DoubleUtil.LessThan(delta, 0.0) && DoubleUtil.AreClose(num2, num3)))
		{
			num += delta;
			delta = 0.0;
		}
		else if (DoubleUtil.LessThan(delta, 0.0) && DoubleUtil.GreaterThanZero(num))
		{
			double num4 = Math.Max(num + delta, 0.0);
			delta += num - num4;
			num = num4;
		}
		else if (DoubleUtil.GreaterThan(delta, 0.0) && DoubleUtil.LessThan(num, 0.0))
		{
			double num5 = Math.Min(num + delta, 0.0);
			delta += num - num5;
			num = num5;
		}
		if (isHorizontal)
		{
			if (!DoubleUtil.AreClose(delta, 0.0))
			{
				ScrollToHorizontalOffset(_panningInfo.OriginalHorizontalOffset - Math.Round(PanningRatio * cumulativeTranslation / _panningInfo.DeltaPerHorizontalOffet));
			}
			_panningInfo.UnusedTranslation = new Vector(num, _panningInfo.UnusedTranslation.Y);
		}
		else
		{
			if (!DoubleUtil.AreClose(delta, 0.0))
			{
				ScrollToVerticalOffset(_panningInfo.OriginalVerticalOffset - Math.Round(PanningRatio * cumulativeTranslation / _panningInfo.DeltaPerVerticalOffset));
			}
			_panningInfo.UnusedTranslation = new Vector(_panningInfo.UnusedTranslation.X, num);
		}
	}

	private bool IsPastInertialLimit()
	{
		if (Math.Abs(Environment.TickCount - _panningInfo.InertiaBoundaryBeginTimestamp) < 100)
		{
			return false;
		}
		if (!DoubleUtil.GreaterThanOrClose(Math.Abs(_panningInfo.UnusedTranslation.X), 50.0))
		{
			return DoubleUtil.GreaterThanOrClose(Math.Abs(_panningInfo.UnusedTranslation.Y), 50.0);
		}
		return true;
	}

	private bool CanStartScrollManipulation(Vector translation, out bool cancelManipulation)
	{
		cancelManipulation = false;
		PanningMode panningMode = _panningInfo.PanningMode;
		if (panningMode == PanningMode.None)
		{
			cancelManipulation = true;
			return false;
		}
		bool flag = DoubleUtil.GreaterThan(Math.Abs(translation.X), 3.0);
		bool flag2 = DoubleUtil.GreaterThan(Math.Abs(translation.Y), 3.0);
		if ((panningMode == PanningMode.Both && (flag || flag2)) || (panningMode == PanningMode.HorizontalOnly && flag) || (panningMode == PanningMode.VerticalOnly && flag2))
		{
			return true;
		}
		switch (panningMode)
		{
		case PanningMode.HorizontalFirst:
		{
			bool flag4 = DoubleUtil.GreaterThanOrClose(Math.Abs(translation.X), Math.Abs(translation.Y));
			if (flag && flag4)
			{
				return true;
			}
			if (flag2)
			{
				cancelManipulation = true;
				return false;
			}
			break;
		}
		case PanningMode.VerticalFirst:
		{
			bool flag3 = DoubleUtil.GreaterThanOrClose(Math.Abs(translation.Y), Math.Abs(translation.X));
			if (flag2 && flag3)
			{
				return true;
			}
			if (flag)
			{
				cancelManipulation = true;
				return false;
			}
			break;
		}
		}
		return false;
	}

	/// <summary>Called when the <see cref="E:System.Windows.UIElement.ManipulationInertiaStarting" /> event occurs.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnManipulationInertiaStarting(ManipulationInertiaStartingEventArgs e)
	{
		if (_panningInfo != null)
		{
			if (!_panningInfo.IsPanning && !ForceNextManipulationComplete)
			{
				e.Cancel();
				_panningInfo = null;
			}
			else
			{
				e.TranslationBehavior.DesiredDeceleration = PanningDeceleration;
			}
			e.Handled = true;
		}
	}

	/// <summary>Called when the <see cref="E:System.Windows.UIElement.ManipulationCompleted" /> event occurs.</summary>
	/// <param name="e">The event data.</param>
	protected override void OnManipulationCompleted(ManipulationCompletedEventArgs e)
	{
		if (_panningInfo == null)
		{
			return;
		}
		if (!e.IsInertial || !CompleteScrollManipulation)
		{
			if (e.IsInertial && !DoubleUtil.AreClose(e.FinalVelocities.LinearVelocity, default(Vector)) && !IsPastInertialLimit())
			{
				ForceNextManipulationComplete = true;
			}
			else
			{
				if (!e.IsInertial && !_panningInfo.IsPanning && !ForceNextManipulationComplete)
				{
					e.Cancel();
				}
				ForceNextManipulationComplete = false;
			}
		}
		_panningInfo = null;
		CompleteScrollManipulation = false;
		e.Handled = true;
	}

	private bool ExecuteNextCommand()
	{
		IScrollInfo scrollInfo = ScrollInfo;
		if (scrollInfo == null)
		{
			return false;
		}
		Command command = _queue.Fetch();
		switch (command.Code)
		{
		case Commands.LineUp:
			scrollInfo.LineUp();
			break;
		case Commands.LineDown:
			scrollInfo.LineDown();
			break;
		case Commands.LineLeft:
			scrollInfo.LineLeft();
			break;
		case Commands.LineRight:
			scrollInfo.LineRight();
			break;
		case Commands.PageUp:
			scrollInfo.PageUp();
			break;
		case Commands.PageDown:
			scrollInfo.PageDown();
			break;
		case Commands.PageLeft:
			scrollInfo.PageLeft();
			break;
		case Commands.PageRight:
			scrollInfo.PageRight();
			break;
		case Commands.SetHorizontalOffset:
			scrollInfo.SetHorizontalOffset(command.Param);
			break;
		case Commands.SetVerticalOffset:
			scrollInfo.SetVerticalOffset(command.Param);
			break;
		case Commands.MakeVisible:
		{
			Visual child = command.MakeVisibleParam.Child;
			Visual visual = scrollInfo as Visual;
			if (child == null || visual == null || (visual != child && !visual.IsAncestorOf(child)) || !IsAncestorOf(visual))
			{
				break;
			}
			Rect rectangle = command.MakeVisibleParam.TargetRect;
			if (rectangle.IsEmpty)
			{
				rectangle = ((!(child is UIElement uIElement)) ? default(Rect) : new Rect(uIElement.RenderSize));
			}
			Rect rect = ((!(scrollInfo.GetType() == typeof(ScrollContentPresenter))) ? scrollInfo.MakeVisible(child, rectangle) : ((ScrollContentPresenter)scrollInfo).MakeVisible(child, rectangle, throwOnError: false));
			if (!rect.IsEmpty)
			{
				if (visual is UIElement uIElement2)
				{
					rect.Intersect(new Rect(uIElement2.RenderSize));
				}
				rect = visual.TransformToAncestor(this).TransformBounds(rect);
			}
			BringIntoView(rect);
			break;
		}
		case Commands.Invalid:
			return false;
		}
		return true;
	}

	private void EnqueueCommand(Commands code, double param, MakeVisibleParams mvp)
	{
		_queue.Enqueue(new Command(code, param, mvp));
		EnsureQueueProcessing();
	}

	private void EnsureQueueProcessing()
	{
		if (!_queue.IsEmpty())
		{
			EnsureLayoutUpdatedHandler();
		}
	}

	private void OnLayoutUpdated(object sender, EventArgs e)
	{
		if (ExecuteNextCommand())
		{
			InvalidateArrange();
			return;
		}
		double horizontalOffset = HorizontalOffset;
		double verticalOffset = VerticalOffset;
		double viewportWidth = ViewportWidth;
		double viewportHeight = ViewportHeight;
		double extentWidth = ExtentWidth;
		double extentHeight = ExtentHeight;
		double scrollableWidth = ScrollableWidth;
		double scrollableHeight = ScrollableHeight;
		bool flag = false;
		if (ScrollInfo != null && !DoubleUtil.AreClose(horizontalOffset, ScrollInfo.HorizontalOffset))
		{
			_xPositionISI = ScrollInfo.HorizontalOffset;
			HorizontalOffset = _xPositionISI;
			ContentHorizontalOffset = _xPositionISI;
			flag = true;
		}
		if (ScrollInfo != null && !DoubleUtil.AreClose(verticalOffset, ScrollInfo.VerticalOffset))
		{
			_yPositionISI = ScrollInfo.VerticalOffset;
			VerticalOffset = _yPositionISI;
			ContentVerticalOffset = _yPositionISI;
			flag = true;
		}
		if (ScrollInfo != null && !DoubleUtil.AreClose(viewportWidth, ScrollInfo.ViewportWidth))
		{
			_xSize = ScrollInfo.ViewportWidth;
			SetValue(ViewportWidthPropertyKey, _xSize);
			flag = true;
		}
		if (ScrollInfo != null && !DoubleUtil.AreClose(viewportHeight, ScrollInfo.ViewportHeight))
		{
			_ySize = ScrollInfo.ViewportHeight;
			SetValue(ViewportHeightPropertyKey, _ySize);
			flag = true;
		}
		if (ScrollInfo != null && !DoubleUtil.AreClose(extentWidth, ScrollInfo.ExtentWidth))
		{
			_xExtent = ScrollInfo.ExtentWidth;
			SetValue(ExtentWidthPropertyKey, _xExtent);
			flag = true;
		}
		if (ScrollInfo != null && !DoubleUtil.AreClose(extentHeight, ScrollInfo.ExtentHeight))
		{
			_yExtent = ScrollInfo.ExtentHeight;
			SetValue(ExtentHeightPropertyKey, _yExtent);
			flag = true;
		}
		double scrollableWidth2 = ScrollableWidth;
		if (!DoubleUtil.AreClose(scrollableWidth, ScrollableWidth))
		{
			SetValue(ScrollableWidthPropertyKey, scrollableWidth2);
			flag = true;
		}
		double scrollableHeight2 = ScrollableHeight;
		if (!DoubleUtil.AreClose(scrollableHeight, ScrollableHeight))
		{
			SetValue(ScrollableHeightPropertyKey, scrollableHeight2);
			flag = true;
		}
		if (flag)
		{
			ScrollChangedEventArgs scrollChangedEventArgs = new ScrollChangedEventArgs(new Vector(HorizontalOffset, VerticalOffset), new Vector(HorizontalOffset - horizontalOffset, VerticalOffset - verticalOffset), new Size(ExtentWidth, ExtentHeight), new Vector(ExtentWidth - extentWidth, ExtentHeight - extentHeight), new Size(ViewportWidth, ViewportHeight), new Vector(ViewportWidth - viewportWidth, ViewportHeight - viewportHeight));
			scrollChangedEventArgs.RoutedEvent = ScrollChangedEvent;
			scrollChangedEventArgs.Source = this;
			try
			{
				OnScrollChanged(scrollChangedEventArgs);
				if (UIElementAutomationPeer.FromElement(this) is ScrollViewerAutomationPeer scrollViewerAutomationPeer)
				{
					scrollViewerAutomationPeer.RaiseAutomationEvents(extentWidth, extentHeight, viewportWidth, viewportHeight, horizontalOffset, verticalOffset);
				}
			}
			finally
			{
				ClearLayoutUpdatedHandler();
			}
		}
		ClearLayoutUpdatedHandler();
	}

	/// <summary>Provides an appropriate <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation for this control, as part of the Windows Presentation Foundation (WPF) automation infrastructure.</summary>
	/// <returns>The appropriate <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation for this control.</returns>
	protected override AutomationPeer OnCreateAutomationPeer()
	{
		return new ScrollViewerAutomationPeer(this);
	}

	private static void OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
	{
		ScrollViewer scrollViewer = sender as ScrollViewer;
		if (e.TargetObject is Visual visual)
		{
			if (visual != scrollViewer && visual.IsDescendantOf(scrollViewer))
			{
				e.Handled = true;
				scrollViewer.MakeVisible(visual, e.TargetRect);
			}
		}
		else
		{
			if (!(e.TargetObject is ContentElement contentElement))
			{
				return;
			}
			IContentHost contentHost = ContentHostHelper.FindContentHost(contentElement);
			if (contentHost is Visual visual2 && visual2.IsDescendantOf(scrollViewer))
			{
				ReadOnlyCollection<Rect> rectangles = contentHost.GetRectangles(contentElement);
				if (rectangles.Count > 0)
				{
					e.Handled = true;
					scrollViewer.MakeVisible(visual2, rectangles[0]);
				}
			}
		}
	}

	private static void OnScrollCommand(object target, ExecutedRoutedEventArgs args)
	{
		if (args.Command == ScrollBar.DeferScrollToHorizontalOffsetCommand)
		{
			if (args.Parameter is double)
			{
				((ScrollViewer)target).DeferScrollToHorizontalOffset((double)args.Parameter);
			}
		}
		else if (args.Command == ScrollBar.DeferScrollToVerticalOffsetCommand)
		{
			if (args.Parameter is double)
			{
				((ScrollViewer)target).DeferScrollToVerticalOffset((double)args.Parameter);
			}
		}
		else if (args.Command == ScrollBar.LineLeftCommand)
		{
			((ScrollViewer)target).LineLeft();
		}
		else if (args.Command == ScrollBar.LineRightCommand)
		{
			((ScrollViewer)target).LineRight();
		}
		else if (args.Command == ScrollBar.PageLeftCommand)
		{
			((ScrollViewer)target).PageLeft();
		}
		else if (args.Command == ScrollBar.PageRightCommand)
		{
			((ScrollViewer)target).PageRight();
		}
		else if (args.Command == ScrollBar.LineUpCommand)
		{
			((ScrollViewer)target).LineUp();
		}
		else if (args.Command == ScrollBar.LineDownCommand)
		{
			((ScrollViewer)target).LineDown();
		}
		else if (args.Command == ScrollBar.PageUpCommand || args.Command == ComponentCommands.ScrollPageUp)
		{
			((ScrollViewer)target).PageUp();
		}
		else if (args.Command == ScrollBar.PageDownCommand || args.Command == ComponentCommands.ScrollPageDown)
		{
			((ScrollViewer)target).PageDown();
		}
		else if (args.Command == ScrollBar.ScrollToEndCommand)
		{
			((ScrollViewer)target).ScrollToEnd();
		}
		else if (args.Command == ScrollBar.ScrollToHomeCommand)
		{
			((ScrollViewer)target).ScrollToHome();
		}
		else if (args.Command == ScrollBar.ScrollToLeftEndCommand)
		{
			((ScrollViewer)target).ScrollToLeftEnd();
		}
		else if (args.Command == ScrollBar.ScrollToRightEndCommand)
		{
			((ScrollViewer)target).ScrollToRightEnd();
		}
		else if (args.Command == ScrollBar.ScrollToTopCommand)
		{
			((ScrollViewer)target).ScrollToTop();
		}
		else if (args.Command == ScrollBar.ScrollToBottomCommand)
		{
			((ScrollViewer)target).ScrollToBottom();
		}
		else if (args.Command == ScrollBar.ScrollToHorizontalOffsetCommand)
		{
			if (args.Parameter is double)
			{
				((ScrollViewer)target).ScrollToHorizontalOffset((double)args.Parameter);
			}
		}
		else if (args.Command == ScrollBar.ScrollToVerticalOffsetCommand && args.Parameter is double)
		{
			((ScrollViewer)target).ScrollToVerticalOffset((double)args.Parameter);
		}
		if (target is ScrollViewer scrollViewer)
		{
			scrollViewer.CompleteScrollManipulation = true;
		}
	}

	private static void OnQueryScrollCommand(object target, CanExecuteRoutedEventArgs args)
	{
		args.CanExecute = true;
		if (args.Command == ComponentCommands.ScrollPageUp || args.Command == ComponentCommands.ScrollPageDown)
		{
			Control control = ((target is ScrollViewer scrollViewer) ? (scrollViewer.TemplatedParent as Control) : null);
			if (control != null && control.HandlesScrolling)
			{
				args.CanExecute = false;
				args.ContinueRouting = true;
				args.Handled = true;
			}
		}
		else if ((args.Command == ScrollBar.DeferScrollToHorizontalOffsetCommand || args.Command == ScrollBar.DeferScrollToVerticalOffsetCommand) && target is ScrollViewer { IsDeferredScrollingEnabled: false })
		{
			args.CanExecute = false;
			args.Handled = true;
		}
	}

	private static void InitializeCommands()
	{
		ExecutedRoutedEventHandler executedRoutedEventHandler = OnScrollCommand;
		CanExecuteRoutedEventHandler canExecuteRoutedEventHandler = OnQueryScrollCommand;
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.LineLeftCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.LineRightCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.PageLeftCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.PageRightCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.LineUpCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.LineDownCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.PageUpCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.PageDownCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.ScrollToLeftEndCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.ScrollToRightEndCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.ScrollToEndCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.ScrollToHomeCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.ScrollToTopCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.ScrollToBottomCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.ScrollToHorizontalOffsetCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.ScrollToVerticalOffsetCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.DeferScrollToHorizontalOffsetCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ScrollBar.DeferScrollToVerticalOffsetCommand, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ComponentCommands.ScrollPageUp, executedRoutedEventHandler, canExecuteRoutedEventHandler);
		CommandHelpers.RegisterCommandHandler(typeof(ScrollViewer), ComponentCommands.ScrollPageDown, executedRoutedEventHandler, canExecuteRoutedEventHandler);
	}

	private static ControlTemplate CreateDefaultControlTemplate()
	{
		FrameworkElementFactory frameworkElementFactory = new FrameworkElementFactory(typeof(Grid), "Grid");
		FrameworkElementFactory frameworkElementFactory2 = new FrameworkElementFactory(typeof(ColumnDefinition), "ColumnDefinitionOne");
		FrameworkElementFactory frameworkElementFactory3 = new FrameworkElementFactory(typeof(ColumnDefinition), "ColumnDefinitionTwo");
		FrameworkElementFactory frameworkElementFactory4 = new FrameworkElementFactory(typeof(RowDefinition), "RowDefinitionOne");
		FrameworkElementFactory frameworkElementFactory5 = new FrameworkElementFactory(typeof(RowDefinition), "RowDefinitionTwo");
		FrameworkElementFactory frameworkElementFactory6 = new FrameworkElementFactory(typeof(ScrollBar), "PART_VerticalScrollBar");
		FrameworkElementFactory frameworkElementFactory7 = new FrameworkElementFactory(typeof(ScrollBar), "PART_HorizontalScrollBar");
		FrameworkElementFactory frameworkElementFactory8 = new FrameworkElementFactory(typeof(ScrollContentPresenter), "PART_ScrollContentPresenter");
		FrameworkElementFactory frameworkElementFactory9 = new FrameworkElementFactory(typeof(Rectangle), "Corner");
		Binding binding = new Binding("HorizontalOffset");
		binding.Mode = BindingMode.OneWay;
		binding.RelativeSource = RelativeSource.TemplatedParent;
		Binding binding2 = new Binding("VerticalOffset");
		binding2.Mode = BindingMode.OneWay;
		binding2.RelativeSource = RelativeSource.TemplatedParent;
		frameworkElementFactory.SetValue(Panel.BackgroundProperty, new TemplateBindingExtension(Control.BackgroundProperty));
		frameworkElementFactory.AppendChild(frameworkElementFactory2);
		frameworkElementFactory.AppendChild(frameworkElementFactory3);
		frameworkElementFactory.AppendChild(frameworkElementFactory4);
		frameworkElementFactory.AppendChild(frameworkElementFactory5);
		frameworkElementFactory.AppendChild(frameworkElementFactory9);
		frameworkElementFactory.AppendChild(frameworkElementFactory8);
		frameworkElementFactory.AppendChild(frameworkElementFactory6);
		frameworkElementFactory.AppendChild(frameworkElementFactory7);
		frameworkElementFactory2.SetValue(ColumnDefinition.WidthProperty, new GridLength(1.0, GridUnitType.Star));
		frameworkElementFactory3.SetValue(ColumnDefinition.WidthProperty, new GridLength(1.0, GridUnitType.Auto));
		frameworkElementFactory4.SetValue(RowDefinition.HeightProperty, new GridLength(1.0, GridUnitType.Star));
		frameworkElementFactory5.SetValue(RowDefinition.HeightProperty, new GridLength(1.0, GridUnitType.Auto));
		frameworkElementFactory8.SetValue(Grid.ColumnProperty, 0);
		frameworkElementFactory8.SetValue(Grid.RowProperty, 0);
		frameworkElementFactory8.SetValue(FrameworkElement.MarginProperty, new TemplateBindingExtension(Control.PaddingProperty));
		frameworkElementFactory8.SetValue(ContentControl.ContentProperty, new TemplateBindingExtension(ContentControl.ContentProperty));
		frameworkElementFactory8.SetValue(ContentControl.ContentTemplateProperty, new TemplateBindingExtension(ContentControl.ContentTemplateProperty));
		frameworkElementFactory8.SetValue(CanContentScrollProperty, new TemplateBindingExtension(CanContentScrollProperty));
		frameworkElementFactory7.SetValue(ScrollBar.OrientationProperty, Orientation.Horizontal);
		frameworkElementFactory7.SetValue(Grid.ColumnProperty, 0);
		frameworkElementFactory7.SetValue(Grid.RowProperty, 1);
		frameworkElementFactory7.SetValue(RangeBase.MinimumProperty, 0.0);
		frameworkElementFactory7.SetValue(RangeBase.MaximumProperty, new TemplateBindingExtension(ScrollableWidthProperty));
		frameworkElementFactory7.SetValue(ScrollBar.ViewportSizeProperty, new TemplateBindingExtension(ViewportWidthProperty));
		frameworkElementFactory7.SetBinding(RangeBase.ValueProperty, binding);
		frameworkElementFactory7.SetValue(UIElement.VisibilityProperty, new TemplateBindingExtension(ComputedHorizontalScrollBarVisibilityProperty));
		frameworkElementFactory7.SetValue(FrameworkElement.CursorProperty, Cursors.Arrow);
		frameworkElementFactory7.SetValue(AutomationProperties.AutomationIdProperty, "HorizontalScrollBar");
		frameworkElementFactory6.SetValue(Grid.ColumnProperty, 1);
		frameworkElementFactory6.SetValue(Grid.RowProperty, 0);
		frameworkElementFactory6.SetValue(RangeBase.MinimumProperty, 0.0);
		frameworkElementFactory6.SetValue(RangeBase.MaximumProperty, new TemplateBindingExtension(ScrollableHeightProperty));
		frameworkElementFactory6.SetValue(ScrollBar.ViewportSizeProperty, new TemplateBindingExtension(ViewportHeightProperty));
		frameworkElementFactory6.SetBinding(RangeBase.ValueProperty, binding2);
		frameworkElementFactory6.SetValue(UIElement.VisibilityProperty, new TemplateBindingExtension(ComputedVerticalScrollBarVisibilityProperty));
		frameworkElementFactory6.SetValue(FrameworkElement.CursorProperty, Cursors.Arrow);
		frameworkElementFactory6.SetValue(AutomationProperties.AutomationIdProperty, "VerticalScrollBar");
		frameworkElementFactory9.SetValue(Grid.ColumnProperty, 1);
		frameworkElementFactory9.SetValue(Grid.RowProperty, 1);
		frameworkElementFactory9.SetResourceReference(Shape.FillProperty, SystemColors.ControlBrushKey);
		ControlTemplate controlTemplate = new ControlTemplate(typeof(ScrollViewer));
		controlTemplate.VisualTree = frameworkElementFactory;
		controlTemplate.Seal();
		return controlTemplate;
	}

	private void SetFlagValue(Flags flag, bool value)
	{
		if (value)
		{
			_flags |= flag;
		}
		else
		{
			_flags &= ~flag;
		}
	}

	static ScrollViewer()
	{
		CanContentScrollProperty = DependencyProperty.RegisterAttached("CanContentScroll", typeof(bool), typeof(ScrollViewer), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		HorizontalScrollBarVisibilityProperty = DependencyProperty.RegisterAttached("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(ScrollViewer), new FrameworkPropertyMetadata(ScrollBarVisibility.Disabled, FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidScrollBarVisibility);
		VerticalScrollBarVisibilityProperty = DependencyProperty.RegisterAttached("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(ScrollViewer), new FrameworkPropertyMetadata(ScrollBarVisibility.Visible, FrameworkPropertyMetadataOptions.AffectsMeasure), IsValidScrollBarVisibility);
		ComputedHorizontalScrollBarVisibilityPropertyKey = DependencyProperty.RegisterReadOnly("ComputedHorizontalScrollBarVisibility", typeof(Visibility), typeof(ScrollViewer), new FrameworkPropertyMetadata(Visibility.Visible));
		ComputedHorizontalScrollBarVisibilityProperty = ComputedHorizontalScrollBarVisibilityPropertyKey.DependencyProperty;
		ComputedVerticalScrollBarVisibilityPropertyKey = DependencyProperty.RegisterReadOnly("ComputedVerticalScrollBarVisibility", typeof(Visibility), typeof(ScrollViewer), new FrameworkPropertyMetadata(Visibility.Visible));
		ComputedVerticalScrollBarVisibilityProperty = ComputedVerticalScrollBarVisibilityPropertyKey.DependencyProperty;
		VerticalOffsetPropertyKey = DependencyProperty.RegisterReadOnly("VerticalOffset", typeof(double), typeof(ScrollViewer), new FrameworkPropertyMetadata(0.0));
		VerticalOffsetProperty = VerticalOffsetPropertyKey.DependencyProperty;
		HorizontalOffsetPropertyKey = DependencyProperty.RegisterReadOnly("HorizontalOffset", typeof(double), typeof(ScrollViewer), new FrameworkPropertyMetadata(0.0));
		HorizontalOffsetProperty = HorizontalOffsetPropertyKey.DependencyProperty;
		ContentVerticalOffsetPropertyKey = DependencyProperty.RegisterReadOnly("ContentVerticalOffset", typeof(double), typeof(ScrollViewer), new FrameworkPropertyMetadata(0.0));
		ContentVerticalOffsetProperty = ContentVerticalOffsetPropertyKey.DependencyProperty;
		ContentHorizontalOffsetPropertyKey = DependencyProperty.RegisterReadOnly("ContentHorizontalOffset", typeof(double), typeof(ScrollViewer), new FrameworkPropertyMetadata(0.0));
		ContentHorizontalOffsetProperty = ContentHorizontalOffsetPropertyKey.DependencyProperty;
		ExtentWidthPropertyKey = DependencyProperty.RegisterReadOnly("ExtentWidth", typeof(double), typeof(ScrollViewer), new FrameworkPropertyMetadata(0.0));
		ExtentWidthProperty = ExtentWidthPropertyKey.DependencyProperty;
		ExtentHeightPropertyKey = DependencyProperty.RegisterReadOnly("ExtentHeight", typeof(double), typeof(ScrollViewer), new FrameworkPropertyMetadata(0.0));
		ExtentHeightProperty = ExtentHeightPropertyKey.DependencyProperty;
		ScrollableWidthPropertyKey = DependencyProperty.RegisterReadOnly("ScrollableWidth", typeof(double), typeof(ScrollViewer), new FrameworkPropertyMetadata(0.0));
		ScrollableWidthProperty = ScrollableWidthPropertyKey.DependencyProperty;
		ScrollableHeightPropertyKey = DependencyProperty.RegisterReadOnly("ScrollableHeight", typeof(double), typeof(ScrollViewer), new FrameworkPropertyMetadata(0.0));
		ScrollableHeightProperty = ScrollableHeightPropertyKey.DependencyProperty;
		ViewportWidthPropertyKey = DependencyProperty.RegisterReadOnly("ViewportWidth", typeof(double), typeof(ScrollViewer), new FrameworkPropertyMetadata(0.0));
		ViewportWidthProperty = ViewportWidthPropertyKey.DependencyProperty;
		ViewportHeightPropertyKey = DependencyProperty.RegisterReadOnly("ViewportHeight", typeof(double), typeof(ScrollViewer), new FrameworkPropertyMetadata(0.0));
		ViewportHeightProperty = ViewportHeightPropertyKey.DependencyProperty;
		IsDeferredScrollingEnabledProperty = DependencyProperty.RegisterAttached("IsDeferredScrollingEnabled", typeof(bool), typeof(ScrollViewer), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		ScrollChangedEvent = EventManager.RegisterRoutedEvent("ScrollChanged", RoutingStrategy.Bubble, typeof(ScrollChangedEventHandler), typeof(ScrollViewer));
		PanningModeProperty = DependencyProperty.RegisterAttached("PanningMode", typeof(PanningMode), typeof(ScrollViewer), new FrameworkPropertyMetadata(PanningMode.None, OnPanningModeChanged));
		PanningDecelerationProperty = DependencyProperty.RegisterAttached("PanningDeceleration", typeof(double), typeof(ScrollViewer), new FrameworkPropertyMetadata(0.001), CheckFiniteNonNegative);
		PanningRatioProperty = DependencyProperty.RegisterAttached("PanningRatio", typeof(double), typeof(ScrollViewer), new FrameworkPropertyMetadata(1.0), CheckFiniteNonNegative);
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ScrollViewer), new FrameworkPropertyMetadata(typeof(ScrollViewer)));
		_dType = DependencyObjectType.FromSystemTypeInternal(typeof(ScrollViewer));
		InitializeCommands();
		ControlTemplate defaultValue = CreateDefaultControlTemplate();
		Control.TemplateProperty.OverrideMetadata(typeof(ScrollViewer), new FrameworkPropertyMetadata(defaultValue));
		Control.IsTabStopProperty.OverrideMetadata(typeof(ScrollViewer), new FrameworkPropertyMetadata(BooleanBoxes.FalseBox));
		KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(ScrollViewer), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));
		EventManager.RegisterClassHandler(typeof(ScrollViewer), FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler(OnRequestBringIntoView));
		ControlsTraceLogger.AddControl(TelemetryControls.ScrollViewer);
	}

	private static bool IsValidScrollBarVisibility(object o)
	{
		ScrollBarVisibility scrollBarVisibility = (ScrollBarVisibility)o;
		if (scrollBarVisibility != 0 && scrollBarVisibility != ScrollBarVisibility.Auto && scrollBarVisibility != ScrollBarVisibility.Hidden)
		{
			return scrollBarVisibility == ScrollBarVisibility.Visible;
		}
		return true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ScrollViewer" /> class. </summary>
	public ScrollViewer()
	{
	}
}
