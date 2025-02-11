using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.Telemetry.PresentationFramework;
using MS.Utility;

namespace System.Windows.Controls;

/// <summary>Arranges child elements into a single line that can be oriented horizontally or vertically. </summary>
public class StackPanel : Panel, IScrollInfo, IStackMeasure
{
	private class ScrollData : IStackMeasureScrollData
	{
		internal bool _allowHorizontal;

		internal bool _allowVertical;

		internal Vector _offset;

		internal Vector _computedOffset = new Vector(0.0, 0.0);

		internal Size _viewport;

		internal Size _extent;

		internal double _physicalViewport;

		internal ScrollViewer _scrollOwner;

		public Vector Offset
		{
			get
			{
				return _offset;
			}
			set
			{
				_offset = value;
			}
		}

		public Size Viewport
		{
			get
			{
				return _viewport;
			}
			set
			{
				_viewport = value;
			}
		}

		public Size Extent
		{
			get
			{
				return _extent;
			}
			set
			{
				_extent = value;
			}
		}

		public Vector ComputedOffset
		{
			get
			{
				return _computedOffset;
			}
			set
			{
				_computedOffset = value;
			}
		}

		internal void ClearLayout()
		{
			_offset = default(Vector);
			_viewport = (_extent = default(Size));
			_physicalViewport = 0.0;
		}

		public void SetPhysicalViewport(double value)
		{
			_physicalViewport = value;
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.StackPanel.Orientation" /> dependency property. </summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.StackPanel.Orientation" /> dependency property.</returns>
	public static readonly DependencyProperty OrientationProperty;

	private ScrollData _scrollData;

	/// <summary>Gets or sets a value that indicates the dimension by which child elements are stacked.  </summary>
	/// <returns>The <see cref="T:System.Windows.Controls.Orientation" /> of child content.</returns>
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

	/// <summary>Gets a value that indicates if this <see cref="T:System.Windows.Controls.StackPanel" /> has vertical or horizontal orientation.</summary>
	/// <returns>This property always returns true.</returns>
	protected internal override bool HasLogicalOrientation => true;

	/// <summary>Gets a value that represents the <see cref="T:System.Windows.Controls.Orientation" /> of the <see cref="T:System.Windows.Controls.StackPanel" />.</summary>
	/// <returns>An <see cref="T:System.Windows.Controls.Orientation" /> value.</returns>
	protected internal override Orientation LogicalOrientation => Orientation;

	/// <summary>Gets or sets a value that indicates whether a <see cref="T:System.Windows.Controls.StackPanel" /> can scroll in the horizontal dimension. </summary>
	/// <returns>true if content can scroll in the horizontal dimension; otherwise, false.</returns>
	[DefaultValue(false)]
	public bool CanHorizontallyScroll
	{
		get
		{
			if (_scrollData == null)
			{
				return false;
			}
			return _scrollData._allowHorizontal;
		}
		set
		{
			EnsureScrollData();
			if (_scrollData._allowHorizontal != value)
			{
				_scrollData._allowHorizontal = value;
				InvalidateMeasure();
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether content can scroll in the vertical dimension. </summary>
	/// <returns>true if content can scroll in the vertical dimension; otherwise, false. The default value is false.</returns>
	[DefaultValue(false)]
	public bool CanVerticallyScroll
	{
		get
		{
			if (_scrollData == null)
			{
				return false;
			}
			return _scrollData._allowVertical;
		}
		set
		{
			EnsureScrollData();
			if (_scrollData._allowVertical != value)
			{
				_scrollData._allowVertical = value;
				InvalidateMeasure();
			}
		}
	}

	/// <summary>Gets a value that contains the horizontal size of the extent.</summary>
	/// <returns>
	///   <see cref="T:System.Double" /> that represents the horizontal size of the extent. The default value is 0.0.</returns>
	public double ExtentWidth
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData._extent.Width;
		}
	}

	/// <summary>Gets a value that contains the vertical size of the extent.</summary>
	/// <returns>The <see cref="T:System.Double" /> that represents the vertical size of the extent. The default value is 0.0.</returns>
	public double ExtentHeight
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData._extent.Height;
		}
	}

	/// <summary>Gets a value that contains the horizontal size of the content's viewport.</summary>
	/// <returns>The <see cref="T:System.Double" /> that represents the vertical size of the content's viewport. The default value is 0.0.</returns>
	public double ViewportWidth
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData._viewport.Width;
		}
	}

	/// <summary>Gets a value that contains the vertical size of the content's viewport.</summary>
	/// <returns>The <see cref="T:System.Double" /> that represents the vertical size of the content's viewport. The default value is 0.0.</returns>
	public double ViewportHeight
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData._viewport.Height;
		}
	}

	/// <summary>Gets a value that contains the horizontal offset of the scrolled content.</summary>
	/// <returns>The <see cref="T:System.Double" /> that represents the horizontal offset of the scrolled content. The default value is 0.0.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public double HorizontalOffset
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData._computedOffset.X;
		}
	}

	/// <summary>Gets a value that contains the vertical offset of the scrolled content.</summary>
	/// <returns>The <see cref="T:System.Double" /> that represents the vertical offset of the scrolled content. The default value is 0.0.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public double VerticalOffset
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData._computedOffset.Y;
		}
	}

	/// <summary>Gets or sets a value that identifies the container that controls scrolling behavior in this <see cref="T:System.Windows.Controls.StackPanel" />.</summary>
	/// <returns>The <see cref="T:System.Windows.Controls.ScrollViewer" /> that owns scrolling for this <see cref="T:System.Windows.Controls.StackPanel" />. This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public ScrollViewer ScrollOwner
	{
		get
		{
			EnsureScrollData();
			return _scrollData._scrollOwner;
		}
		set
		{
			EnsureScrollData();
			if (value != _scrollData._scrollOwner)
			{
				ResetScrolling(this);
				_scrollData._scrollOwner = value;
			}
		}
	}

	private bool IsScrolling
	{
		get
		{
			if (_scrollData != null)
			{
				return _scrollData._scrollOwner != null;
			}
			return false;
		}
	}

	internal override int EffectiveValuesInitialSize => 9;

	bool IStackMeasure.IsScrolling => IsScrolling;

	UIElementCollection IStackMeasure.InternalChildren => base.InternalChildren;

	private bool CanMouseWheelVerticallyScroll => SystemParameters.WheelScrollLines > 0;

	static StackPanel()
	{
		OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(StackPanel), new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, OnOrientationChanged), ScrollBar.IsValidOrientation);
		ControlsTraceLogger.AddControl(TelemetryControls.StackPanel);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.StackPanel" /> class.</summary>
	public StackPanel()
	{
	}

	/// <summary>Scrolls content by one logical unit upward.</summary>
	public void LineUp()
	{
		SetVerticalOffset(VerticalOffset - ((Orientation == Orientation.Vertical) ? 1.0 : 16.0));
	}

	/// <summary>Scrolls content downward by one logical unit.</summary>
	public void LineDown()
	{
		SetVerticalOffset(VerticalOffset + ((Orientation == Orientation.Vertical) ? 1.0 : 16.0));
	}

	/// <summary>Scrolls content by one logical unit to the left.</summary>
	public void LineLeft()
	{
		SetHorizontalOffset(HorizontalOffset - ((Orientation == Orientation.Horizontal) ? 1.0 : 16.0));
	}

	/// <summary>Scrolls content by one logical unit to the right.</summary>
	public void LineRight()
	{
		SetHorizontalOffset(HorizontalOffset + ((Orientation == Orientation.Horizontal) ? 1.0 : 16.0));
	}

	/// <summary>Scrolls content logically upward by one page.</summary>
	public void PageUp()
	{
		SetVerticalOffset(VerticalOffset - ViewportHeight);
	}

	/// <summary>Scrolls content logically downward by one page.</summary>
	public void PageDown()
	{
		SetVerticalOffset(VerticalOffset + ViewportHeight);
	}

	/// <summary>Scrolls content logically to the left by one page.</summary>
	public void PageLeft()
	{
		SetHorizontalOffset(HorizontalOffset - ViewportWidth);
	}

	/// <summary>Scrolls content logically to the right by one page.</summary>
	public void PageRight()
	{
		SetHorizontalOffset(HorizontalOffset + ViewportWidth);
	}

	/// <summary>Scrolls content logically upward in response to a click of the mouse wheel button.</summary>
	public void MouseWheelUp()
	{
		if (CanMouseWheelVerticallyScroll)
		{
			SetVerticalOffset(VerticalOffset - (double)SystemParameters.WheelScrollLines * ((Orientation == Orientation.Vertical) ? 1.0 : 16.0));
		}
		else
		{
			PageUp();
		}
	}

	/// <summary>Scrolls content logically downward in response to a click of the mouse wheel button.</summary>
	public void MouseWheelDown()
	{
		if (CanMouseWheelVerticallyScroll)
		{
			SetVerticalOffset(VerticalOffset + (double)SystemParameters.WheelScrollLines * ((Orientation == Orientation.Vertical) ? 1.0 : 16.0));
		}
		else
		{
			PageDown();
		}
	}

	/// <summary>Scrolls content logically to the left in response to a click of the mouse wheel button.</summary>
	public void MouseWheelLeft()
	{
		SetHorizontalOffset(HorizontalOffset - 3.0 * ((Orientation == Orientation.Horizontal) ? 1.0 : 16.0));
	}

	/// <summary>Scrolls content logically to the right in response to a click of the mouse wheel button.</summary>
	public void MouseWheelRight()
	{
		SetHorizontalOffset(HorizontalOffset + 3.0 * ((Orientation == Orientation.Horizontal) ? 1.0 : 16.0));
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.StackPanel.HorizontalOffset" /> property.</summary>
	/// <param name="offset">The value of the <see cref="P:System.Windows.Controls.StackPanel.HorizontalOffset" /> property.</param>
	public void SetHorizontalOffset(double offset)
	{
		EnsureScrollData();
		double num = ScrollContentPresenter.ValidateInputOffset(offset, "HorizontalOffset");
		if (!DoubleUtil.AreClose(num, _scrollData._offset.X))
		{
			_scrollData._offset.X = num;
			InvalidateMeasure();
		}
	}

	/// <summary>Sets the value of the <see cref="P:System.Windows.Controls.StackPanel.VerticalOffset" /> property.</summary>
	/// <param name="offset">The value of the <see cref="P:System.Windows.Controls.StackPanel.VerticalOffset" /> property.</param>
	public void SetVerticalOffset(double offset)
	{
		EnsureScrollData();
		double num = ScrollContentPresenter.ValidateInputOffset(offset, "VerticalOffset");
		if (!DoubleUtil.AreClose(num, _scrollData._offset.Y))
		{
			_scrollData._offset.Y = num;
			InvalidateMeasure();
		}
	}

	/// <summary>Scrolls to the specified coordinates and makes that part of a <see cref="T:System.Windows.Media.Visual" /> visible. </summary>
	/// <returns>A <see cref="T:System.Windows.Rect" /> in the coordinate space that is made visible.</returns>
	/// <param name="visual">The <see cref="T:System.Windows.Media.Visual" /> that becomes visible.</param>
	/// <param name="rectangle">The <see cref="T:System.Windows.Rect" /> that represents coordinate space within a visual.</param>
	public Rect MakeVisible(Visual visual, Rect rectangle)
	{
		Vector newOffset = default(Vector);
		Rect newRect = default(Rect);
		if (rectangle.IsEmpty || visual == null || visual == this || !IsAncestorOf(visual))
		{
			return Rect.Empty;
		}
		rectangle = visual.TransformToAncestor(this).TransformBounds(rectangle);
		if (!IsScrolling)
		{
			return rectangle;
		}
		MakeVisiblePhysicalHelper(rectangle, ref newOffset, ref newRect);
		int childIndex = FindChildIndexThatParentsVisual(visual);
		MakeVisibleLogicalHelper(childIndex, ref newOffset, ref newRect);
		newOffset.X = ScrollContentPresenter.CoerceOffset(newOffset.X, _scrollData._extent.Width, _scrollData._viewport.Width);
		newOffset.Y = ScrollContentPresenter.CoerceOffset(newOffset.Y, _scrollData._extent.Height, _scrollData._viewport.Height);
		if (!DoubleUtil.AreClose(newOffset, _scrollData._offset))
		{
			_scrollData._offset = newOffset;
			InvalidateMeasure();
			OnScrollChange();
		}
		return newRect;
	}

	/// <summary>Measures the child elements of a <see cref="T:System.Windows.Controls.StackPanel" /> in anticipation of arranging them during the <see cref="M:System.Windows.Controls.StackPanel.ArrangeOverride(System.Windows.Size)" /> pass.</summary>
	/// <returns>The <see cref="T:System.Windows.Size" /> that represents the desired size of the element.</returns>
	/// <param name="constraint">An upper limit <see cref="T:System.Windows.Size" /> that should not be exceeded.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		Size size = default(Size);
		bool flag = IsScrolling && EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info);
		if (flag)
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringBegin, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "STACK:MeasureOverride");
		}
		try
		{
			return StackMeasureHelper(this, _scrollData, constraint);
		}
		finally
		{
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringEnd, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "STACK:MeasureOverride");
			}
		}
	}

	internal static Size StackMeasureHelper(IStackMeasure measureElement, IStackMeasureScrollData scrollData, Size constraint)
	{
		Size size = default(Size);
		UIElementCollection internalChildren = measureElement.InternalChildren;
		Size availableSize = constraint;
		bool flag = measureElement.Orientation == Orientation.Horizontal;
		int num = -1;
		int num2;
		double num3;
		if (flag)
		{
			availableSize.Width = double.PositiveInfinity;
			if (measureElement.IsScrolling && measureElement.CanVerticallyScroll)
			{
				availableSize.Height = double.PositiveInfinity;
			}
			num2 = (measureElement.IsScrolling ? CoerceOffsetToInteger(scrollData.Offset.X, internalChildren.Count) : 0);
			num3 = constraint.Width;
		}
		else
		{
			availableSize.Height = double.PositiveInfinity;
			if (measureElement.IsScrolling && measureElement.CanHorizontallyScroll)
			{
				availableSize.Width = double.PositiveInfinity;
			}
			num2 = (measureElement.IsScrolling ? CoerceOffsetToInteger(scrollData.Offset.Y, internalChildren.Count) : 0);
			num3 = constraint.Height;
		}
		int i = 0;
		for (int count = internalChildren.Count; i < count; i++)
		{
			UIElement uIElement = internalChildren[i];
			if (uIElement == null)
			{
				continue;
			}
			uIElement.Measure(availableSize);
			Size desiredSize = uIElement.DesiredSize;
			double num4;
			if (flag)
			{
				size.Width += desiredSize.Width;
				size.Height = Math.Max(size.Height, desiredSize.Height);
				num4 = desiredSize.Width;
			}
			else
			{
				size.Width = Math.Max(size.Width, desiredSize.Width);
				size.Height += desiredSize.Height;
				num4 = desiredSize.Height;
			}
			if (measureElement.IsScrolling && num == -1 && i >= num2)
			{
				num3 -= num4;
				if (DoubleUtil.LessThanOrClose(num3, 0.0))
				{
					num = i;
				}
			}
		}
		if (measureElement.IsScrolling)
		{
			Size viewport = constraint;
			Size extent = size;
			Vector offset = scrollData.Offset;
			if (num == -1)
			{
				num = internalChildren.Count - 1;
			}
			while (num2 > 0)
			{
				double num5 = num3;
				num5 = ((!flag) ? (num5 - internalChildren[num2 - 1].DesiredSize.Height) : (num5 - internalChildren[num2 - 1].DesiredSize.Width));
				if (DoubleUtil.LessThan(num5, 0.0))
				{
					break;
				}
				num2--;
				num3 = num5;
			}
			int count2 = internalChildren.Count;
			int num6 = num - num2;
			if (num6 == 0 || DoubleUtil.GreaterThanOrClose(num3, 0.0))
			{
				num6++;
			}
			if (flag)
			{
				scrollData.SetPhysicalViewport(viewport.Width);
				viewport.Width = num6;
				extent.Width = count2;
				offset.X = num2;
				offset.Y = Math.Max(0.0, Math.Min(offset.Y, extent.Height - viewport.Height));
			}
			else
			{
				scrollData.SetPhysicalViewport(viewport.Height);
				viewport.Height = num6;
				extent.Height = count2;
				offset.Y = num2;
				offset.X = Math.Max(0.0, Math.Min(offset.X, extent.Width - viewport.Width));
			}
			size.Width = Math.Min(size.Width, constraint.Width);
			size.Height = Math.Min(size.Height, constraint.Height);
			VerifyScrollingData(measureElement, scrollData, viewport, extent, offset);
		}
		return size;
	}

	/// <summary>Arranges the content of a <see cref="T:System.Windows.Controls.StackPanel" /> element.</summary>
	/// <returns>The <see cref="T:System.Windows.Size" /> that represents the arranged size of this <see cref="T:System.Windows.Controls.StackPanel" /> element and its child elements.</returns>
	/// <param name="arrangeSize">The <see cref="T:System.Windows.Size" /> that this element should use to arrange its child elements.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		bool flag = IsScrolling && EventTrace.IsEnabled(EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info);
		if (flag)
		{
			EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringBegin, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "STACK:ArrangeOverride");
		}
		try
		{
			StackArrangeHelper(this, _scrollData, arrangeSize);
			return arrangeSize;
		}
		finally
		{
			if (flag)
			{
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientStringEnd, EventTrace.Keyword.KeywordGeneral, EventTrace.Level.Info, "STACK:ArrangeOverride");
			}
		}
	}

	internal static Size StackArrangeHelper(IStackMeasure arrangeElement, IStackMeasureScrollData scrollData, Size arrangeSize)
	{
		UIElementCollection internalChildren = arrangeElement.InternalChildren;
		bool flag = arrangeElement.Orientation == Orientation.Horizontal;
		Rect finalRect = new Rect(arrangeSize);
		double num = 0.0;
		if (arrangeElement.IsScrolling)
		{
			if (flag)
			{
				finalRect.X = ComputePhysicalFromLogicalOffset(arrangeElement, scrollData.ComputedOffset.X, fHorizontal: true);
				finalRect.Y = -1.0 * scrollData.ComputedOffset.Y;
			}
			else
			{
				finalRect.X = -1.0 * scrollData.ComputedOffset.X;
				finalRect.Y = ComputePhysicalFromLogicalOffset(arrangeElement, scrollData.ComputedOffset.Y, fHorizontal: false);
			}
		}
		int i = 0;
		for (int count = internalChildren.Count; i < count; i++)
		{
			UIElement uIElement = internalChildren[i];
			if (uIElement != null)
			{
				if (flag)
				{
					finalRect.X += num;
					num = (finalRect.Width = uIElement.DesiredSize.Width);
					finalRect.Height = Math.Max(arrangeSize.Height, uIElement.DesiredSize.Height);
				}
				else
				{
					finalRect.Y += num;
					num = (finalRect.Height = uIElement.DesiredSize.Height);
					finalRect.Width = Math.Max(arrangeSize.Width, uIElement.DesiredSize.Width);
				}
				uIElement.Arrange(finalRect);
			}
		}
		return arrangeSize;
	}

	private void EnsureScrollData()
	{
		if (_scrollData == null)
		{
			_scrollData = new ScrollData();
		}
	}

	private static void ResetScrolling(StackPanel element)
	{
		element.InvalidateMeasure();
		if (element.IsScrolling)
		{
			element._scrollData.ClearLayout();
		}
	}

	private void OnScrollChange()
	{
		if (ScrollOwner != null)
		{
			ScrollOwner.InvalidateScrollInfo();
		}
	}

	private static void VerifyScrollingData(IStackMeasure measureElement, IStackMeasureScrollData scrollData, Size viewport, Size extent, Vector offset)
	{
		int num = (int)(1u & (DoubleUtil.AreClose(viewport, scrollData.Viewport) ? 1u : 0u) & (DoubleUtil.AreClose(extent, scrollData.Extent) ? 1u : 0u)) & (DoubleUtil.AreClose(offset, scrollData.ComputedOffset) ? 1 : 0);
		scrollData.Offset = offset;
		if (num == 0)
		{
			scrollData.Viewport = viewport;
			scrollData.Extent = extent;
			scrollData.ComputedOffset = offset;
			measureElement.OnScrollChange();
		}
	}

	private static double ComputePhysicalFromLogicalOffset(IStackMeasure arrangeElement, double logicalOffset, bool fHorizontal)
	{
		double num = 0.0;
		UIElementCollection internalChildren = arrangeElement.InternalChildren;
		for (int i = 0; (double)i < logicalOffset; i++)
		{
			num -= (fHorizontal ? internalChildren[i].DesiredSize.Width : internalChildren[i].DesiredSize.Height);
		}
		return num;
	}

	private int FindChildIndexThatParentsVisual(Visual child)
	{
		DependencyObject dependencyObject = child;
		DependencyObject parent = VisualTreeHelper.GetParent(child);
		while (parent != this)
		{
			dependencyObject = parent;
			parent = VisualTreeHelper.GetParent(dependencyObject);
			if (parent == null)
			{
				throw new ArgumentException(SR.Stack_VisualInDifferentSubTree, "child");
			}
		}
		return base.Children.IndexOf((UIElement)dependencyObject);
	}

	private void MakeVisiblePhysicalHelper(Rect r, ref Vector newOffset, ref Rect newRect)
	{
		bool num = Orientation == Orientation.Horizontal;
		double num2;
		double num3;
		double num5;
		double num4;
		if (num)
		{
			num2 = _scrollData._computedOffset.Y;
			num3 = ViewportHeight;
			num4 = r.Y;
			num5 = r.Height;
		}
		else
		{
			num2 = _scrollData._computedOffset.X;
			num3 = ViewportWidth;
			num4 = r.X;
			num5 = r.Width;
		}
		num4 += num2;
		double num6 = ScrollContentPresenter.ComputeScrollOffsetWithMinimalScroll(num2, num2 + num3, num4, num4 + num5);
		double num7 = Math.Max(num4, num6);
		num5 = Math.Max(Math.Min(num5 + num4, num6 + num3) - num7, 0.0);
		num4 = num7;
		num4 -= num2;
		if (num)
		{
			newOffset.Y = num6;
			newRect.Y = num4;
			newRect.Height = num5;
		}
		else
		{
			newOffset.X = num6;
			newRect.X = num4;
			newRect.Width = num5;
		}
	}

	private void MakeVisibleLogicalHelper(int childIndex, ref Vector newOffset, ref Rect newRect)
	{
		bool flag = Orientation == Orientation.Horizontal;
		double num = 0.0;
		int num2;
		int num3;
		if (flag)
		{
			num2 = (int)_scrollData._computedOffset.X;
			num3 = (int)_scrollData._viewport.Width;
		}
		else
		{
			num2 = (int)_scrollData._computedOffset.Y;
			num3 = (int)_scrollData._viewport.Height;
		}
		int num4 = num2;
		if (childIndex < num2)
		{
			num4 = childIndex;
		}
		else if (childIndex > num2 + num3 - 1)
		{
			Size desiredSize = base.InternalChildren[childIndex].DesiredSize;
			double num5 = (flag ? desiredSize.Width : desiredSize.Height);
			double num6 = _scrollData._physicalViewport - num5;
			int num7 = childIndex;
			while (num7 > 0 && DoubleUtil.GreaterThanOrClose(num6, 0.0))
			{
				num7--;
				desiredSize = base.InternalChildren[num7].DesiredSize;
				num5 = (flag ? desiredSize.Width : desiredSize.Height);
				num += num5;
				num6 -= num5;
			}
			if (num7 != childIndex && DoubleUtil.LessThan(num6, 0.0))
			{
				num -= num5;
				num7++;
			}
			num4 = num7;
		}
		if (flag)
		{
			newOffset.X = num4;
			newRect.X = num;
			newRect.Width = base.InternalChildren[childIndex].DesiredSize.Width;
		}
		else
		{
			newOffset.Y = num4;
			newRect.Y = num;
			newRect.Height = base.InternalChildren[childIndex].DesiredSize.Height;
		}
	}

	private static int CoerceOffsetToInteger(double offset, int numberOfItems)
	{
		if (double.IsNegativeInfinity(offset))
		{
			return 0;
		}
		if (double.IsPositiveInfinity(offset))
		{
			return numberOfItems - 1;
		}
		int val = (int)offset;
		return Math.Max(Math.Min(numberOfItems - 1, val), 0);
	}

	private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ResetScrolling(d as StackPanel);
	}

	void IStackMeasure.OnScrollChange()
	{
		OnScrollChange();
	}
}
