using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.PtsHost;
using MS.Internal.Text;

namespace MS.Internal.Documents;

internal class FlowDocumentView : FrameworkElement, IScrollInfo, IServiceProvider
{
	private FlowDocument _document;

	private PageVisual _pageVisual;

	private FlowDocumentFormatter _formatter;

	private ScrollData _scrollData;

	private DocumentPageTextView _textView;

	private bool _suspendLayout;

	protected override int VisualChildrenCount => (_pageVisual != null) ? 1 : 0;

	internal FlowDocument Document
	{
		get
		{
			return _document;
		}
		set
		{
			if (_formatter != null)
			{
				HandleFormatterSuspended(_formatter, EventArgs.Empty);
			}
			_suspendLayout = false;
			_textView = null;
			_document = value;
			InvalidateMeasure();
			InvalidateVisual();
		}
	}

	internal FlowDocumentPage DocumentPage
	{
		get
		{
			if (_document != null)
			{
				EnsureFormatter();
				return _formatter.DocumentPage;
			}
			return null;
		}
	}

	bool IScrollInfo.CanVerticallyScroll
	{
		get
		{
			if (_scrollData == null)
			{
				return false;
			}
			return _scrollData.CanVerticallyScroll;
		}
		set
		{
			if (_scrollData != null)
			{
				_scrollData.CanVerticallyScroll = value;
			}
		}
	}

	bool IScrollInfo.CanHorizontallyScroll
	{
		get
		{
			if (_scrollData == null)
			{
				return false;
			}
			return _scrollData.CanHorizontallyScroll;
		}
		set
		{
			if (_scrollData != null)
			{
				_scrollData.CanHorizontallyScroll = value;
			}
		}
	}

	double IScrollInfo.ExtentWidth
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData.ExtentWidth;
		}
	}

	double IScrollInfo.ExtentHeight
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData.ExtentHeight;
		}
	}

	double IScrollInfo.ViewportWidth
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData.ViewportWidth;
		}
	}

	double IScrollInfo.ViewportHeight
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData.ViewportHeight;
		}
	}

	double IScrollInfo.HorizontalOffset
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData.HorizontalOffset;
		}
	}

	double IScrollInfo.VerticalOffset
	{
		get
		{
			if (_scrollData == null)
			{
				return 0.0;
			}
			return _scrollData.VerticalOffset;
		}
	}

	ScrollViewer IScrollInfo.ScrollOwner
	{
		get
		{
			if (_scrollData == null)
			{
				return null;
			}
			return _scrollData.ScrollOwner;
		}
		set
		{
			if (_scrollData == null)
			{
				_scrollData = new ScrollData();
			}
			_scrollData.SetScrollOwner(this, value);
		}
	}

	static FlowDocumentView()
	{
	}

	internal FlowDocumentView()
	{
	}

	protected sealed override Size MeasureOverride(Size constraint)
	{
		Size result = default(Size);
		if (_suspendLayout)
		{
			return base.DesiredSize;
		}
		if (Document != null)
		{
			EnsureFormatter();
			_formatter.Format(constraint);
			if (_scrollData == null)
			{
				return _formatter.DocumentPage.Size;
			}
			result.Width = Math.Min(constraint.Width, _formatter.DocumentPage.Size.Width);
			result.Height = Math.Min(constraint.Height, _formatter.DocumentPage.Size.Height);
		}
		return result;
	}

	protected sealed override Size ArrangeOverride(Size arrangeSize)
	{
		Rect viewport = Rect.Empty;
		bool flag = false;
		Size size = arrangeSize;
		if (!_suspendLayout)
		{
			TextDpi.SnapToTextDpi(ref size);
			if (Document != null)
			{
				EnsureFormatter();
				if (_scrollData != null)
				{
					if (!DoubleUtil.AreClose(_scrollData.Viewport, size))
					{
						_scrollData.Viewport = size;
						flag = true;
					}
					if (!DoubleUtil.AreClose(_scrollData.Extent, _formatter.DocumentPage.Size))
					{
						_scrollData.Extent = _formatter.DocumentPage.Size;
						flag = true;
						if (Math.Abs(_scrollData.ExtentWidth - _scrollData.ViewportWidth) < 1.0)
						{
							_scrollData.ExtentWidth = _scrollData.ViewportWidth;
						}
						if (Math.Abs(_scrollData.ExtentHeight - _scrollData.ViewportHeight) < 1.0)
						{
							_scrollData.ExtentHeight = _scrollData.ViewportHeight;
						}
					}
					Vector vector = new Vector(Math.Max(0.0, Math.Min(_scrollData.ExtentWidth - _scrollData.ViewportWidth, _scrollData.HorizontalOffset)), Math.Max(0.0, Math.Min(_scrollData.ExtentHeight - _scrollData.ViewportHeight, _scrollData.VerticalOffset)));
					if (!DoubleUtil.AreClose(vector, _scrollData.Offset))
					{
						_scrollData.Offset = vector;
						flag = true;
					}
					if (flag && _scrollData.ScrollOwner != null)
					{
						_scrollData.ScrollOwner.InvalidateScrollInfo();
					}
					viewport = new Rect(_scrollData.HorizontalOffset, _scrollData.VerticalOffset, size.Width, size.Height);
				}
				_formatter.Arrange(size, viewport);
				if (_pageVisual != _formatter.DocumentPage.Visual)
				{
					if (_textView != null)
					{
						_textView.OnPageConnected();
					}
					if (_pageVisual != null)
					{
						RemoveVisualChild(_pageVisual);
					}
					_pageVisual = (PageVisual)_formatter.DocumentPage.Visual;
					AddVisualChild(_pageVisual);
				}
				if (_scrollData != null)
				{
					_pageVisual.Offset = new Vector(0.0 - _scrollData.HorizontalOffset, 0.0 - _scrollData.VerticalOffset);
				}
				PtsHelper.UpdateMirroringTransform(base.FlowDirection, FlowDirection.LeftToRight, _pageVisual, size.Width);
			}
			else
			{
				if (_pageVisual != null)
				{
					if (_textView != null)
					{
						_textView.OnPageDisconnected();
					}
					RemoveVisualChild(_pageVisual);
					_pageVisual = null;
				}
				if (_scrollData != null)
				{
					if (!DoubleUtil.AreClose(_scrollData.Viewport, size))
					{
						_scrollData.Viewport = size;
						flag = true;
					}
					if (!DoubleUtil.AreClose(_scrollData.Extent, default(Size)))
					{
						_scrollData.Extent = default(Size);
						flag = true;
					}
					if (!DoubleUtil.AreClose(_scrollData.Offset, default(Vector)))
					{
						_scrollData.Offset = default(Vector);
						flag = true;
					}
					if (flag && _scrollData.ScrollOwner != null)
					{
						_scrollData.ScrollOwner.InvalidateScrollInfo();
					}
				}
			}
		}
		return arrangeSize;
	}

	protected override Visual GetVisualChild(int index)
	{
		if (index != 0)
		{
			throw new ArgumentOutOfRangeException("index", index, SR.Visual_ArgumentOutOfRange);
		}
		return _pageVisual;
	}

	internal void SuspendLayout()
	{
		_suspendLayout = true;
		if (_pageVisual != null)
		{
			_pageVisual.Opacity = 0.5;
		}
	}

	internal void ResumeLayout()
	{
		_suspendLayout = false;
		if (_pageVisual != null)
		{
			_pageVisual.Opacity = 1.0;
		}
		InvalidateMeasure();
	}

	private void EnsureFormatter()
	{
		Invariant.Assert(_document != null);
		if (_formatter == null)
		{
			_formatter = _document.BottomlessFormatter;
			_formatter.ContentInvalidated += HandleContentInvalidated;
			_formatter.Suspended += HandleFormatterSuspended;
		}
		Invariant.Assert(_formatter == _document.BottomlessFormatter);
	}

	private void HandleContentInvalidated(object sender, EventArgs e)
	{
		Invariant.Assert(sender == _formatter);
		InvalidateMeasure();
		InvalidateVisual();
	}

	private void HandleFormatterSuspended(object sender, EventArgs e)
	{
		Invariant.Assert(sender == _formatter);
		_formatter.ContentInvalidated -= HandleContentInvalidated;
		_formatter.Suspended -= HandleFormatterSuspended;
		_formatter = null;
		if (_pageVisual != null && !_suspendLayout)
		{
			if (_textView != null)
			{
				_textView.OnPageDisconnected();
			}
			RemoveVisualChild(_pageVisual);
			_pageVisual = null;
		}
	}

	void IScrollInfo.LineUp()
	{
		if (_scrollData != null)
		{
			_scrollData.LineUp(this);
		}
	}

	void IScrollInfo.LineDown()
	{
		if (_scrollData != null)
		{
			_scrollData.LineDown(this);
		}
	}

	void IScrollInfo.LineLeft()
	{
		if (_scrollData != null)
		{
			_scrollData.LineLeft(this);
		}
	}

	void IScrollInfo.LineRight()
	{
		if (_scrollData != null)
		{
			_scrollData.LineRight(this);
		}
	}

	void IScrollInfo.PageUp()
	{
		if (_scrollData != null)
		{
			_scrollData.PageUp(this);
		}
	}

	void IScrollInfo.PageDown()
	{
		if (_scrollData != null)
		{
			_scrollData.PageDown(this);
		}
	}

	void IScrollInfo.PageLeft()
	{
		if (_scrollData != null)
		{
			_scrollData.PageLeft(this);
		}
	}

	void IScrollInfo.PageRight()
	{
		if (_scrollData != null)
		{
			_scrollData.PageRight(this);
		}
	}

	void IScrollInfo.MouseWheelUp()
	{
		if (_scrollData != null)
		{
			_scrollData.MouseWheelUp(this);
		}
	}

	void IScrollInfo.MouseWheelDown()
	{
		if (_scrollData != null)
		{
			_scrollData.MouseWheelDown(this);
		}
	}

	void IScrollInfo.MouseWheelLeft()
	{
		if (_scrollData != null)
		{
			_scrollData.MouseWheelLeft(this);
		}
	}

	void IScrollInfo.MouseWheelRight()
	{
		if (_scrollData != null)
		{
			_scrollData.MouseWheelRight(this);
		}
	}

	void IScrollInfo.SetHorizontalOffset(double offset)
	{
		if (_scrollData != null)
		{
			_scrollData.SetHorizontalOffset(this, offset);
		}
	}

	void IScrollInfo.SetVerticalOffset(double offset)
	{
		if (_scrollData != null)
		{
			_scrollData.SetVerticalOffset(this, offset);
		}
	}

	Rect IScrollInfo.MakeVisible(Visual visual, Rect rectangle)
	{
		rectangle = ((_scrollData != null) ? _scrollData.MakeVisible(this, visual, rectangle) : Rect.Empty);
		return rectangle;
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		object result = null;
		if (serviceType == typeof(ITextView))
		{
			if (_textView == null && _document != null)
			{
				_textView = new DocumentPageTextView(this, _document.StructuralCache.TextContainer);
			}
			result = _textView;
		}
		else if (serviceType == typeof(ITextContainer) && Document != null)
		{
			result = Document.StructuralCache.TextContainer;
		}
		return result;
	}
}
