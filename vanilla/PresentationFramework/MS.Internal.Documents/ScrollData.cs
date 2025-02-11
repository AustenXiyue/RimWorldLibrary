using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MS.Internal.Documents;

internal class ScrollData
{
	private bool _disableHorizonalScroll;

	private bool _disableVerticalScroll;

	private Vector _offset;

	private Size _viewport;

	private Size _extent;

	private ScrollViewer _scrollOwner;

	internal bool CanVerticallyScroll
	{
		get
		{
			return !_disableVerticalScroll;
		}
		set
		{
			_disableVerticalScroll = !value;
		}
	}

	internal bool CanHorizontallyScroll
	{
		get
		{
			return !_disableHorizonalScroll;
		}
		set
		{
			_disableHorizonalScroll = !value;
		}
	}

	internal double ExtentWidth
	{
		get
		{
			return _extent.Width;
		}
		set
		{
			_extent.Width = value;
		}
	}

	internal double ExtentHeight
	{
		get
		{
			return _extent.Height;
		}
		set
		{
			_extent.Height = value;
		}
	}

	internal double ViewportWidth => _viewport.Width;

	internal double ViewportHeight => _viewport.Height;

	internal double HorizontalOffset => _offset.X;

	internal double VerticalOffset => _offset.Y;

	internal ScrollViewer ScrollOwner => _scrollOwner;

	internal Vector Offset
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

	internal Size Extent
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

	internal Size Viewport
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

	internal void LineUp(UIElement owner)
	{
		SetVerticalOffset(owner, _offset.Y - 16.0);
	}

	internal void LineDown(UIElement owner)
	{
		SetVerticalOffset(owner, _offset.Y + 16.0);
	}

	internal void LineLeft(UIElement owner)
	{
		SetHorizontalOffset(owner, _offset.X - 16.0);
	}

	internal void LineRight(UIElement owner)
	{
		SetHorizontalOffset(owner, _offset.X + 16.0);
	}

	internal void PageUp(UIElement owner)
	{
		SetVerticalOffset(owner, _offset.Y - _viewport.Height);
	}

	internal void PageDown(UIElement owner)
	{
		SetVerticalOffset(owner, _offset.Y + _viewport.Height);
	}

	internal void PageLeft(UIElement owner)
	{
		SetHorizontalOffset(owner, _offset.X - _viewport.Width);
	}

	internal void PageRight(UIElement owner)
	{
		SetHorizontalOffset(owner, _offset.X + _viewport.Width);
	}

	internal void MouseWheelUp(UIElement owner)
	{
		SetVerticalOffset(owner, _offset.Y - 48.0);
	}

	internal void MouseWheelDown(UIElement owner)
	{
		SetVerticalOffset(owner, _offset.Y + 48.0);
	}

	internal void MouseWheelLeft(UIElement owner)
	{
		SetHorizontalOffset(owner, _offset.X - 48.0);
	}

	internal void MouseWheelRight(UIElement owner)
	{
		SetHorizontalOffset(owner, _offset.X + 48.0);
	}

	internal void SetHorizontalOffset(UIElement owner, double offset)
	{
		if (!CanHorizontallyScroll)
		{
			return;
		}
		offset = Math.Max(0.0, Math.Min(_extent.Width - _viewport.Width, offset));
		if (!DoubleUtil.AreClose(offset, _offset.X))
		{
			_offset.X = offset;
			owner.InvalidateArrange();
			if (_scrollOwner != null)
			{
				_scrollOwner.InvalidateScrollInfo();
			}
		}
	}

	internal void SetVerticalOffset(UIElement owner, double offset)
	{
		if (!CanVerticallyScroll)
		{
			return;
		}
		offset = Math.Max(0.0, Math.Min(_extent.Height - _viewport.Height, offset));
		if (!DoubleUtil.AreClose(offset, _offset.Y))
		{
			_offset.Y = offset;
			owner.InvalidateArrange();
			if (_scrollOwner != null)
			{
				_scrollOwner.InvalidateScrollInfo();
			}
		}
	}

	internal Rect MakeVisible(UIElement owner, Visual visual, Rect rectangle)
	{
		if (rectangle.IsEmpty || visual == null || (visual != owner && !owner.IsAncestorOf(visual)))
		{
			return Rect.Empty;
		}
		rectangle = visual.TransformToAncestor(owner).TransformBounds(rectangle);
		Rect rect = new Rect(_offset.X, _offset.Y, _viewport.Width, _viewport.Height);
		rectangle.X += rect.X;
		rectangle.Y += rect.Y;
		double num = ComputeScrollOffset(rect.Left, rect.Right, rectangle.Left, rectangle.Right);
		double num2 = ComputeScrollOffset(rect.Top, rect.Bottom, rectangle.Top, rectangle.Bottom);
		SetHorizontalOffset(owner, num);
		SetVerticalOffset(owner, num2);
		if (CanHorizontallyScroll)
		{
			rect.X = num;
		}
		else
		{
			rectangle.X = rect.X;
		}
		if (CanVerticallyScroll)
		{
			rect.Y = num2;
		}
		else
		{
			rectangle.Y = rect.Y;
		}
		rectangle.Intersect(rect);
		if (!rectangle.IsEmpty)
		{
			rectangle.X -= rect.X;
			rectangle.Y -= rect.Y;
		}
		return rectangle;
	}

	internal void SetScrollOwner(UIElement owner, ScrollViewer value)
	{
		if (value != _scrollOwner)
		{
			_disableHorizonalScroll = false;
			_disableVerticalScroll = false;
			_offset = default(Vector);
			_viewport = default(Size);
			_extent = default(Size);
			_scrollOwner = value;
			owner.InvalidateArrange();
		}
	}

	private double ComputeScrollOffset(double topView, double bottomView, double topChild, double bottomChild)
	{
		bool num = DoubleUtil.GreaterThanOrClose(topChild, topView) && DoubleUtil.LessThan(topChild, bottomView);
		bool flag = DoubleUtil.LessThanOrClose(bottomChild, bottomView) && DoubleUtil.GreaterThan(bottomChild, topView);
		if (num && flag)
		{
			return topView;
		}
		return topChild;
	}
}
