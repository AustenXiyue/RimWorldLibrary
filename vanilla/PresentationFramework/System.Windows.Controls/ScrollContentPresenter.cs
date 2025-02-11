using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal;
using MS.Utility;

namespace System.Windows.Controls;

/// <summary>Displays the content of a <see cref="T:System.Windows.Controls.ScrollViewer" /> control.</summary>
public sealed class ScrollContentPresenter : ContentPresenter, IScrollInfo
{
	private class ScrollData
	{
		internal ScrollViewer _scrollOwner;

		internal bool _canHorizontallyScroll;

		internal bool _canVerticallyScroll;

		internal Vector _offset;

		internal Vector _computedOffset;

		internal Size _viewport;

		internal Size _extent;
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.ScrollContentPresenter.CanContentScroll" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.ScrollContentPresenter.CanContentScroll" /> dependency property.</returns>
	public static readonly DependencyProperty CanContentScrollProperty = ScrollViewer.CanContentScrollProperty.AddOwner(typeof(ScrollContentPresenter), new FrameworkPropertyMetadata(OnCanContentScrollChanged));

	private IScrollInfo _scrollInfo;

	private ScrollData _scrollData;

	private readonly AdornerLayer _adornerLayer;

	/// <summary>Gets the <see cref="T:System.Windows.Documents.AdornerLayer" /> on which adorners are rendered.</summary>
	/// <returns>The <see cref="T:System.Windows.Documents.AdornerLayer" /> on which adorners are rendered.</returns>
	public AdornerLayer AdornerLayer => _adornerLayer;

	/// <summary>Indicates whether the content, if it supports <see cref="T:System.Windows.Controls.Primitives.IScrollInfo" />, should be allowed to control scrolling.   </summary>
	/// <returns>true if the content is allowed to scroll; otherwise, false. A false value indicates that the <see cref="T:System.Windows.Controls.ScrollContentPresenter" /> acts as the scrolling client. This property has no default value.</returns>
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

	/// <summary>Gets or sets a value that indicates whether scrolling on the horizontal axis is possible.</summary>
	/// <returns>true if scrolling is possible; otherwise, false. This property has no default value.</returns>
	public bool CanHorizontallyScroll
	{
		get
		{
			if (!IsScrollClient)
			{
				return false;
			}
			return EnsureScrollData()._canHorizontallyScroll;
		}
		set
		{
			if (IsScrollClient && EnsureScrollData()._canHorizontallyScroll != value)
			{
				_scrollData._canHorizontallyScroll = value;
				InvalidateMeasure();
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether scrolling on the vertical axis is possible.</summary>
	/// <returns>true if scrolling is possible; otherwise, false. This property has no default value.</returns>
	public bool CanVerticallyScroll
	{
		get
		{
			if (!IsScrollClient)
			{
				return false;
			}
			return EnsureScrollData()._canVerticallyScroll;
		}
		set
		{
			if (IsScrollClient && EnsureScrollData()._canVerticallyScroll != value)
			{
				_scrollData._canVerticallyScroll = value;
				InvalidateMeasure();
			}
		}
	}

	/// <summary>Gets the horizontal size of the extent.</summary>
	/// <returns>The horizontal size of the extent. This property has no default value.</returns>
	public double ExtentWidth
	{
		get
		{
			if (!IsScrollClient)
			{
				return 0.0;
			}
			return EnsureScrollData()._extent.Width;
		}
	}

	/// <summary>Gets the vertical size of the extent.</summary>
	/// <returns>The vertical size of the extent. This property has no default value.</returns>
	public double ExtentHeight
	{
		get
		{
			if (!IsScrollClient)
			{
				return 0.0;
			}
			return EnsureScrollData()._extent.Height;
		}
	}

	/// <summary>Gets the horizontal size of the viewport for this content.</summary>
	/// <returns>The horizontal size of the viewport for this content. This property has no default value.</returns>
	public double ViewportWidth
	{
		get
		{
			if (!IsScrollClient)
			{
				return 0.0;
			}
			return EnsureScrollData()._viewport.Width;
		}
	}

	/// <summary>Gets the vertical size of the viewport for this content.</summary>
	/// <returns>The vertical size of the viewport for this content. This property has no default value.</returns>
	public double ViewportHeight
	{
		get
		{
			if (!IsScrollClient)
			{
				return 0.0;
			}
			return EnsureScrollData()._viewport.Height;
		}
	}

	/// <summary>Gets the horizontal offset of the scrolled content.</summary>
	/// <returns>The horizontal offset. This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public double HorizontalOffset
	{
		get
		{
			if (!IsScrollClient)
			{
				return 0.0;
			}
			return EnsureScrollData()._computedOffset.X;
		}
	}

	/// <summary>Gets the vertical offset of the scrolled content.</summary>
	/// <returns>The vertical offset of the scrolled content. Valid values are between zero and the <see cref="P:System.Windows.Controls.ScrollContentPresenter.ExtentHeight" /> minus the <see cref="P:System.Windows.Controls.ScrollContentPresenter.ViewportHeight" />. This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public double VerticalOffset
	{
		get
		{
			if (!IsScrollClient)
			{
				return 0.0;
			}
			return EnsureScrollData()._computedOffset.Y;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Controls.ScrollViewer" /> element that controls scrolling behavior.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ScrollViewer" /> element that controls scrolling behavior. This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ScrollViewer ScrollOwner
	{
		get
		{
			if (!IsScrollClient)
			{
				return null;
			}
			return _scrollData._scrollOwner;
		}
		set
		{
			if (IsScrollClient)
			{
				_scrollData._scrollOwner = value;
			}
		}
	}

	protected override int VisualChildrenCount
	{
		get
		{
			if (base.TemplateChild != null)
			{
				return 2;
			}
			return 0;
		}
	}

	internal override UIElement TemplateChild
	{
		get
		{
			return base.TemplateChild;
		}
		set
		{
			UIElement templateChild = base.TemplateChild;
			if (value != templateChild)
			{
				if (templateChild != null && value == null)
				{
					RemoveVisualChild(_adornerLayer);
				}
				base.TemplateChild = value;
				if (templateChild == null && value != null)
				{
					AddVisualChild(_adornerLayer);
				}
			}
		}
	}

	private bool IsScrollClient => _scrollInfo == this;

	internal override int EffectiveValuesInitialSize => 42;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.ScrollContentPresenter" /> class. </summary>
	public ScrollContentPresenter()
	{
		_adornerLayer = new AdornerLayer();
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.ScrollContentPresenter" /> content upward by one line.</summary>
	public void LineUp()
	{
		if (IsScrollClient)
		{
			SetVerticalOffset(VerticalOffset - 16.0);
		}
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.ScrollContentPresenter" /> content downward by one line.</summary>
	public void LineDown()
	{
		if (IsScrollClient)
		{
			SetVerticalOffset(VerticalOffset + 16.0);
		}
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.ScrollContentPresenter" /> content to the left by a predetermined amount.</summary>
	public void LineLeft()
	{
		if (IsScrollClient)
		{
			SetHorizontalOffset(HorizontalOffset - 16.0);
		}
	}

	/// <summary>Scrolls the <see cref="T:System.Windows.Controls.ScrollContentPresenter" /> content to the right by a predetermined amount.</summary>
	public void LineRight()
	{
		if (IsScrollClient)
		{
			SetHorizontalOffset(HorizontalOffset + 16.0);
		}
	}

	/// <summary>Scrolls up within content by one page.</summary>
	public void PageUp()
	{
		if (IsScrollClient)
		{
			SetVerticalOffset(VerticalOffset - ViewportHeight);
		}
	}

	/// <summary>Scrolls down within content by one page.</summary>
	public void PageDown()
	{
		if (IsScrollClient)
		{
			SetVerticalOffset(VerticalOffset + ViewportHeight);
		}
	}

	/// <summary>Scrolls left within content by one page.</summary>
	public void PageLeft()
	{
		if (IsScrollClient)
		{
			SetHorizontalOffset(HorizontalOffset - ViewportWidth);
		}
	}

	/// <summary>Scrolls right within content by one page.</summary>
	public void PageRight()
	{
		if (IsScrollClient)
		{
			SetHorizontalOffset(HorizontalOffset + ViewportWidth);
		}
	}

	/// <summary>Scrolls up within content after a user clicks the wheel button on a mouse.</summary>
	public void MouseWheelUp()
	{
		if (IsScrollClient)
		{
			SetVerticalOffset(VerticalOffset - 48.0);
		}
	}

	/// <summary>Scrolls down within content after a user clicks the wheel button on a mouse.</summary>
	public void MouseWheelDown()
	{
		if (IsScrollClient)
		{
			SetVerticalOffset(VerticalOffset + 48.0);
		}
	}

	/// <summary>Scrolls left within content after a user clicks the wheel button on a mouse.</summary>
	public void MouseWheelLeft()
	{
		if (IsScrollClient)
		{
			SetHorizontalOffset(HorizontalOffset - 48.0);
		}
	}

	/// <summary>Scrolls right within content after a user clicks the wheel button on a mouse.</summary>
	public void MouseWheelRight()
	{
		if (IsScrollClient)
		{
			SetHorizontalOffset(HorizontalOffset + 48.0);
		}
	}

	/// <summary>Sets the amount of horizontal offset.</summary>
	/// <param name="offset">The degree to which content is horizontally offset from the containing viewport.</param>
	public void SetHorizontalOffset(double offset)
	{
		if (IsScrollClient)
		{
			double num = ValidateInputOffset(offset, "HorizontalOffset");
			if (!DoubleUtil.AreClose(EnsureScrollData()._offset.X, num))
			{
				_scrollData._offset.X = num;
				InvalidateArrange();
			}
		}
	}

	/// <summary>Sets the amount of vertical offset.</summary>
	/// <param name="offset">The degree to which content is vertically offset from the containing viewport.</param>
	public void SetVerticalOffset(double offset)
	{
		if (IsScrollClient)
		{
			double num = ValidateInputOffset(offset, "VerticalOffset");
			if (!DoubleUtil.AreClose(EnsureScrollData()._offset.Y, num))
			{
				_scrollData._offset.Y = num;
				InvalidateArrange();
			}
		}
	}

	/// <summary>Forces content to scroll until the coordinate space of a <see cref="T:System.Windows.Media.Visual" /> object is visible. </summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> that represents the visible region.</returns>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> that becomes visible.</param>
	/// <param name="rectangle">The bounding rectangle that identifies the coordinate space to make visible.</param>
	public Rect MakeVisible(Visual visual, Rect rectangle)
	{
		return MakeVisible(visual, rectangle, throwOnError: true);
	}

	protected override Visual GetVisualChild(int index)
	{
		if (base.TemplateChild == null)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return index switch
		{
			0 => base.TemplateChild, 
			1 => _adornerLayer, 
			_ => throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange), 
		};
	}

	protected override Size MeasureOverride(Size constraint)
	{
		Size size = default(Size);
		bool flag = IsScrollClient && EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info);
		if (flag)
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringBegin, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "SCROLLCONTENTPRESENTER:MeasureOverride");
		}
		try
		{
			if (VisualChildrenCount > 0)
			{
				_adornerLayer.Measure(constraint);
				if (!IsScrollClient)
				{
					size = base.MeasureOverride(constraint);
				}
				else
				{
					Size constraint2 = constraint;
					if (_scrollData._canHorizontallyScroll)
					{
						constraint2.Width = double.PositiveInfinity;
					}
					if (_scrollData._canVerticallyScroll)
					{
						constraint2.Height = double.PositiveInfinity;
					}
					size = base.MeasureOverride(constraint2);
				}
			}
			if (IsScrollClient)
			{
				VerifyScrollData(constraint, size);
			}
			size.Width = Math.Min(constraint.Width, size.Width);
			size.Height = Math.Min(constraint.Height, size.Height);
			return size;
		}
		finally
		{
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringEnd, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "SCROLLCONTENTPRESENTER:MeasureOverride");
			}
		}
	}

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		bool flag = IsScrollClient && EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info);
		if (flag)
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringBegin, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "SCROLLCONTENTPRESENTER:ArrangeOverride");
		}
		try
		{
			int visualChildrenCount = VisualChildrenCount;
			if (IsScrollClient)
			{
				VerifyScrollData(arrangeSize, _scrollData._extent);
			}
			if (visualChildrenCount > 0)
			{
				_adornerLayer.Arrange(new Rect(arrangeSize));
				if (GetVisualChild(0) is UIElement uIElement)
				{
					Rect finalRect = new Rect(uIElement.DesiredSize);
					if (IsScrollClient)
					{
						finalRect.X = 0.0 - HorizontalOffset;
						finalRect.Y = 0.0 - VerticalOffset;
					}
					finalRect.Width = Math.Max(finalRect.Width, arrangeSize.Width);
					finalRect.Height = Math.Max(finalRect.Height, arrangeSize.Height);
					uIElement.Arrange(finalRect);
				}
			}
		}
		finally
		{
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringEnd, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "SCROLLCONTENTPRESENTER:ArrangeOverride");
			}
		}
		return arrangeSize;
	}

	protected override Geometry GetLayoutClip(Size layoutSlotSize)
	{
		return new RectangleGeometry(new Rect(base.RenderSize));
	}

	/// <summary>Invoked when an internal process or application calls <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />, which is used to build the visual tree of the current template.</summary>
	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		HookupScrollingComponents();
	}

	internal Rect MakeVisible(Visual visual, Rect rectangle, bool throwOnError)
	{
		if (rectangle.IsEmpty || visual == null || visual == this || !IsAncestorOf(visual))
		{
			return Rect.Empty;
		}
		rectangle = visual.TransformToAncestor(this).TransformBounds(rectangle);
		if (!IsScrollClient || (!throwOnError && rectangle.IsEmpty))
		{
			return rectangle;
		}
		Rect rect = new Rect(HorizontalOffset, VerticalOffset, ViewportWidth, ViewportHeight);
		rectangle.X += rect.X;
		rectangle.Y += rect.Y;
		double num = ComputeScrollOffsetWithMinimalScroll(rect.Left, rect.Right, rectangle.Left, rectangle.Right);
		double num2 = ComputeScrollOffsetWithMinimalScroll(rect.Top, rect.Bottom, rectangle.Top, rectangle.Bottom);
		SetHorizontalOffset(num);
		SetVerticalOffset(num2);
		rect.X = num;
		rect.Y = num2;
		rectangle.Intersect(rect);
		if (throwOnError)
		{
			rectangle.X -= rect.X;
			rectangle.Y -= rect.Y;
		}
		else if (!rectangle.IsEmpty)
		{
			rectangle.X -= rect.X;
			rectangle.Y -= rect.Y;
		}
		return rectangle;
	}

	internal static double ComputeScrollOffsetWithMinimalScroll(double topView, double bottomView, double topChild, double bottomChild)
	{
		bool alignTop = false;
		bool alignBottom = false;
		return ComputeScrollOffsetWithMinimalScroll(topView, bottomView, topChild, bottomChild, ref alignTop, ref alignBottom);
	}

	internal static double ComputeScrollOffsetWithMinimalScroll(double topView, double bottomView, double topChild, double bottomChild, ref bool alignTop, ref bool alignBottom)
	{
		bool flag = DoubleUtil.LessThan(topChild, topView) && DoubleUtil.LessThan(bottomChild, bottomView);
		bool flag2 = DoubleUtil.GreaterThan(bottomChild, bottomView) && DoubleUtil.GreaterThan(topChild, topView);
		bool flag3 = bottomChild - topChild > bottomView - topView;
		if (((flag && !flag3) || (flag2 && flag3)) | alignTop)
		{
			alignTop = true;
			return topChild;
		}
		if ((flag || flag2) | alignBottom)
		{
			alignBottom = true;
			return bottomChild - (bottomView - topView);
		}
		return topView;
	}

	internal static double ValidateInputOffset(double offset, string parameterName)
	{
		if (double.IsNaN(offset))
		{
			throw new ArgumentOutOfRangeException(parameterName, SR.Format(SR.ScrollViewer_CannotBeNaN, parameterName));
		}
		return Math.Max(0.0, offset);
	}

	private ScrollData EnsureScrollData()
	{
		if (_scrollData == null)
		{
			_scrollData = new ScrollData();
		}
		return _scrollData;
	}

	internal void HookupScrollingComponents()
	{
		if (base.TemplatedParent is ScrollViewer scrollViewer)
		{
			IScrollInfo scrollInfo = null;
			if (CanContentScroll)
			{
				scrollInfo = base.Content as IScrollInfo;
				if (scrollInfo == null && base.Content is Visual visual)
				{
					ItemsPresenter itemsPresenter = visual as ItemsPresenter;
					if (itemsPresenter == null && scrollViewer.TemplatedParent is FrameworkElement frameworkElement)
					{
						itemsPresenter = frameworkElement.GetTemplateChild("ItemsPresenter") as ItemsPresenter;
					}
					if (itemsPresenter != null)
					{
						itemsPresenter.ApplyTemplate();
						if (VisualTreeHelper.GetChildrenCount(itemsPresenter) > 0)
						{
							scrollInfo = VisualTreeHelper.GetChild(itemsPresenter, 0) as IScrollInfo;
						}
					}
				}
			}
			if (scrollInfo == null)
			{
				scrollInfo = this;
				EnsureScrollData();
			}
			if (scrollInfo != _scrollInfo && _scrollInfo != null)
			{
				if (IsScrollClient)
				{
					_scrollData = null;
				}
				else
				{
					_scrollInfo.ScrollOwner = null;
				}
			}
			if (scrollInfo != null)
			{
				_scrollInfo = scrollInfo;
				scrollInfo.ScrollOwner = scrollViewer;
				scrollViewer.ScrollInfo = scrollInfo;
			}
		}
		else if (_scrollInfo != null)
		{
			if (_scrollInfo.ScrollOwner != null)
			{
				_scrollInfo.ScrollOwner.ScrollInfo = null;
			}
			_scrollInfo.ScrollOwner = null;
			_scrollInfo = null;
			_scrollData = null;
		}
	}

	private void VerifyScrollData(Size viewport, Size extent)
	{
		if (double.IsInfinity(viewport.Width))
		{
			viewport.Width = extent.Width;
		}
		if (double.IsInfinity(viewport.Height))
		{
			viewport.Height = extent.Height;
		}
		int num = (int)(1u & (DoubleUtil.AreClose(viewport, _scrollData._viewport) ? 1u : 0u)) & (DoubleUtil.AreClose(extent, _scrollData._extent) ? 1 : 0);
		_scrollData._viewport = viewport;
		_scrollData._extent = extent;
		if (((uint)num & (CoerceOffsets() ? 1u : 0u)) == 0)
		{
			ScrollOwner.InvalidateScrollInfo();
		}
	}

	internal static double CoerceOffset(double offset, double extent, double viewport)
	{
		if (offset > extent - viewport)
		{
			offset = extent - viewport;
		}
		if (offset < 0.0)
		{
			offset = 0.0;
		}
		return offset;
	}

	private bool CoerceOffsets()
	{
		Vector vector = new Vector(CoerceOffset(_scrollData._offset.X, _scrollData._extent.Width, _scrollData._viewport.Width), CoerceOffset(_scrollData._offset.Y, _scrollData._extent.Height, _scrollData._viewport.Height));
		bool result = DoubleUtil.AreClose(_scrollData._computedOffset, vector);
		_scrollData._computedOffset = vector;
		return result;
	}

	private static void OnCanContentScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ScrollContentPresenter scrollContentPresenter = (ScrollContentPresenter)d;
		if (scrollContentPresenter._scrollInfo != null)
		{
			scrollContentPresenter.HookupScrollingComponents();
			scrollContentPresenter.InvalidateMeasure();
		}
	}
}
