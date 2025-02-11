using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using MS.Internal;
using MS.Internal.Documents;
using MS.Internal.PtsHost;
using MS.Internal.Text;
using Standard;

namespace System.Windows.Controls;

internal class TextBoxView : FrameworkElement, ITextView, IScrollInfo, IServiceProvider
{
	[Flags]
	private enum Flags
	{
		TextContainerListenersInitialized = 1,
		BackgroundLayoutPending = 2,
		ArrangePendingFromHighlightLayer = 4
	}

	private class TextCache
	{
		private readonly LineProperties _lineProperties;

		private readonly TextRunCache _textRunCache;

		private TextFormatter _textFormatter;

		internal LineProperties LineProperties => _lineProperties;

		internal TextRunCache TextRunCache => _textRunCache;

		internal TextFormatter TextFormatter => _textFormatter;

		internal TextCache(TextBoxView owner)
		{
			_lineProperties = owner.GetLineProperties();
			_textRunCache = new TextRunCache();
			TextFormattingMode textFormattingMode = TextOptions.GetTextFormattingMode((Control)owner.Host);
			_textFormatter = TextFormatter.FromCurrentDispatcher(textFormattingMode);
		}
	}

	private class LineRecord
	{
		private int _offset;

		private readonly int _length;

		private readonly int _contentLength;

		private readonly double _width;

		internal int Offset
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

		internal int Length => _length;

		internal int ContentLength => _contentLength;

		internal double Width => _width;

		internal int EndOffset => _offset + _length;

		internal LineRecord(int offset, TextBoxLine line)
		{
			_offset = offset;
			_length = line.Length;
			_contentLength = line.ContentLength;
			_width = line.Width;
		}
	}

	private readonly ITextBoxViewHost _host;

	private Size _contentSize;

	private Size _previousConstraint;

	private TextCache _cache;

	private double _lineHeight;

	private List<TextBoxLineDrawingVisual> _visualChildren;

	private List<LineRecord> _lineMetrics;

	private List<TextBoxLineDrawingVisual> _viewportLineVisuals;

	private int _viewportLineVisualsIndex;

	private ScrollData _scrollData;

	private DtrList _dirtyList;

	private DispatcherTimer _throttleBackgroundTimer;

	private Flags _flags;

	private EventHandler UpdatedEvent;

	private const uint _maxMeasureTimeMs = 200u;

	private const int _throttleBackgroundSeconds = 2;

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
			double num = 0.0;
			if (_scrollData != null)
			{
				num = _scrollData.ExtentWidth;
				if (base.UseLayoutRounding)
				{
					num = UIElement.RoundLayoutValue(num, GetDpi().DpiScaleX);
				}
			}
			return num;
		}
	}

	double IScrollInfo.ExtentHeight
	{
		get
		{
			double num = 0.0;
			if (_scrollData != null)
			{
				num = _scrollData.ExtentHeight;
				if (base.UseLayoutRounding)
				{
					num = UIElement.RoundLayoutValue(num, GetDpi().DpiScaleY);
				}
			}
			return num;
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

	protected override int VisualChildrenCount
	{
		get
		{
			if (_visualChildren != null)
			{
				return _visualChildren.Count;
			}
			return 0;
		}
	}

	internal ITextBoxViewHost Host => _host;

	UIElement ITextView.RenderScope => this;

	ITextContainer ITextView.TextContainer => _host.TextContainer;

	bool ITextView.IsValid => IsLayoutValid;

	bool ITextView.RendersOwnSelection => !FrameworkAppContextSwitches.UseAdornerForTextboxSelectionRendering;

	ReadOnlyCollection<TextSegment> ITextView.TextSegments
	{
		get
		{
			List<TextSegment> list = new List<TextSegment>(1);
			if (_lineMetrics != null)
			{
				ITextPointer startPosition = _host.TextContainer.CreatePointerAtOffset(_lineMetrics[0].Offset, LogicalDirection.Backward);
				ITextPointer endPosition = _host.TextContainer.CreatePointerAtOffset(_lineMetrics[_lineMetrics.Count - 1].EndOffset, LogicalDirection.Forward);
				list.Add(new TextSegment(startPosition, endPosition, preserveLogicalDirection: true));
			}
			return new ReadOnlyCollection<TextSegment>(list);
		}
	}

	private bool IsLayoutValid
	{
		get
		{
			if (base.IsMeasureValid)
			{
				return base.IsArrangeValid;
			}
			return false;
		}
	}

	private Rect Viewport
	{
		get
		{
			if (_scrollData != null)
			{
				return new Rect(_scrollData.HorizontalOffset, _scrollData.VerticalOffset, _scrollData.ViewportWidth, _scrollData.ViewportHeight);
			}
			return Rect.Empty;
		}
	}

	private bool IsBackgroundLayoutPending => CheckFlags(Flags.BackgroundLayoutPending);

	private double VerticalAlignmentOffset => ((Control)_host).VerticalContentAlignment switch
	{
		VerticalAlignment.Center => VerticalPadding / 2.0, 
		VerticalAlignment.Bottom => VerticalPadding, 
		_ => 0.0, 
	};

	private TextAlignment CalculatedTextAlignment
	{
		get
		{
			Control control = (Control)_host;
			object obj = null;
			BaseValueSource baseValueSource = DependencyPropertyHelper.GetValueSource(control, TextBox.TextAlignmentProperty).BaseValueSource;
			BaseValueSource baseValueSource2 = DependencyPropertyHelper.GetValueSource(control, Control.HorizontalContentAlignmentProperty).BaseValueSource;
			if (baseValueSource == BaseValueSource.Local)
			{
				return (TextAlignment)control.GetValue(TextBox.TextAlignmentProperty);
			}
			if (baseValueSource2 == BaseValueSource.Local)
			{
				obj = control.GetValue(Control.HorizontalContentAlignmentProperty);
				return HorizontalAlignmentToTextAlignment((HorizontalAlignment)obj);
			}
			if (baseValueSource == BaseValueSource.Default && baseValueSource2 != BaseValueSource.Default)
			{
				obj = control.GetValue(Control.HorizontalContentAlignmentProperty);
				return HorizontalAlignmentToTextAlignment((HorizontalAlignment)obj);
			}
			return (TextAlignment)control.GetValue(TextBox.TextAlignmentProperty);
		}
	}

	private double VerticalPadding
	{
		get
		{
			Rect viewport = Viewport;
			if (viewport.IsEmpty)
			{
				return 0.0;
			}
			return Math.Max(0.0, viewport.Height - _contentSize.Height);
		}
	}

	event BringPositionIntoViewCompletedEventHandler ITextView.BringPositionIntoViewCompleted
	{
		add
		{
			Invariant.Assert(condition: false);
		}
		remove
		{
			Invariant.Assert(condition: false);
		}
	}

	event BringPointIntoViewCompletedEventHandler ITextView.BringPointIntoViewCompleted
	{
		add
		{
			Invariant.Assert(condition: false);
		}
		remove
		{
			Invariant.Assert(condition: false);
		}
	}

	event BringLineIntoViewCompletedEventHandler ITextView.BringLineIntoViewCompleted
	{
		add
		{
			Invariant.Assert(condition: false);
		}
		remove
		{
			Invariant.Assert(condition: false);
		}
	}

	event BringPageIntoViewCompletedEventHandler ITextView.BringPageIntoViewCompleted
	{
		add
		{
			Invariant.Assert(condition: false);
		}
		remove
		{
			Invariant.Assert(condition: false);
		}
	}

	event EventHandler ITextView.Updated
	{
		add
		{
			UpdatedEvent = (EventHandler)Delegate.Combine(UpdatedEvent, value);
		}
		remove
		{
			UpdatedEvent = (EventHandler)Delegate.Remove(UpdatedEvent, value);
		}
	}

	static TextBoxView()
	{
		FrameworkElement.MarginProperty.OverrideMetadata(typeof(TextBoxView), new FrameworkPropertyMetadata(new Thickness(2.0, 0.0, 2.0, 0.0)));
	}

	internal TextBoxView(ITextBoxViewHost host)
	{
		Invariant.Assert(host is Control);
		_host = host;
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		object result = null;
		if (serviceType == typeof(ITextView))
		{
			result = this;
		}
		return result;
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

	protected override Size MeasureOverride(Size constraint)
	{
		EnsureTextContainerListeners();
		if (_lineMetrics == null)
		{
			_lineMetrics = new List<LineRecord>(1);
		}
		_cache = null;
		EnsureCache();
		LineProperties lineProperties = _cache.LineProperties;
		bool num = !DoubleUtil.AreClose(constraint.Width, _previousConstraint.Width);
		if (num && lineProperties.TextAlignment != 0)
		{
			_viewportLineVisuals = null;
		}
		bool flag = num && lineProperties.TextWrapping != TextWrapping.NoWrap;
		Size size;
		if (_lineMetrics.Count == 0 || flag)
		{
			_dirtyList = null;
		}
		else if (_dirtyList == null && !IsBackgroundLayoutPending)
		{
			size = _contentSize;
			goto IL_0176;
		}
		if (_dirtyList != null && _lineMetrics.Count == 1 && _lineMetrics[0].EndOffset == 0)
		{
			_lineMetrics.Clear();
			_viewportLineVisuals = null;
			_dirtyList = null;
		}
		Size size2 = constraint;
		TextDpi.EnsureValidLineWidth(ref size2);
		if (_dirtyList == null)
		{
			if (flag)
			{
				_lineMetrics.Clear();
				_viewportLineVisuals = null;
			}
			size = FullMeasureTick(size2.Width, lineProperties);
		}
		else
		{
			size = IncrementalMeasure(size2.Width, lineProperties);
		}
		Invariant.Assert(_lineMetrics.Count >= 1);
		_dirtyList = null;
		double width = _contentSize.Width;
		_contentSize = size;
		if (width != size.Width && lineProperties.TextAlignment != 0)
		{
			Rerender();
		}
		goto IL_0176;
		IL_0176:
		if (_scrollData != null)
		{
			size.Width = Math.Min(constraint.Width, size.Width);
			size.Height = Math.Min(constraint.Height, size.Height);
		}
		_previousConstraint = constraint;
		return size;
	}

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		if (_lineMetrics != null && _lineMetrics.Count != 0)
		{
			EnsureCache();
			ArrangeScrollData(arrangeSize);
			ArrangeVisuals(arrangeSize);
			_cache = null;
			FireTextViewUpdatedEvent();
		}
		return arrangeSize;
	}

	protected override void OnRender(DrawingContext context)
	{
		context.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, new Rect(0.0, 0.0, base.RenderSize.Width, base.RenderSize.Height));
	}

	protected override Visual GetVisualChild(int index)
	{
		if (index >= VisualChildrenCount)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return _visualChildren[index];
	}

	ITextPointer ITextView.GetTextPositionFromPoint(Point point, bool snapToText)
	{
		Invariant.Assert(IsLayoutValid);
		point = TransformToDocumentSpace(point);
		int lineIndexFromPoint = GetLineIndexFromPoint(point, snapToText);
		ITextPointer textPointer;
		if (lineIndexFromPoint == -1)
		{
			textPointer = null;
		}
		else
		{
			textPointer = GetTextPositionFromDistance(lineIndexFromPoint, point.X);
			textPointer.Freeze();
		}
		return textPointer;
	}

	Rect ITextView.GetRectangleFromTextPosition(ITextPointer position)
	{
		Invariant.Assert(IsLayoutValid);
		Invariant.Assert(Contains(position));
		int num = position.Offset;
		if (num > 0 && position.LogicalDirection == LogicalDirection.Backward)
		{
			num--;
		}
		int lineIndexFromOffset = GetLineIndexFromOffset(num);
		LineProperties lineProperties;
		Rect boundsFromTextPosition;
		FlowDirection flowDirection;
		using (TextBoxLine textBoxLine = GetFormattedLine(lineIndexFromOffset, out lineProperties))
		{
			boundsFromTextPosition = textBoxLine.GetBoundsFromTextPosition(num, out flowDirection);
		}
		if (!boundsFromTextPosition.IsEmpty)
		{
			boundsFromTextPosition.Y += (double)lineIndexFromOffset * _lineHeight;
			if (lineProperties.FlowDirection != flowDirection)
			{
				if (position.LogicalDirection == LogicalDirection.Forward || position.Offset == 0)
				{
					boundsFromTextPosition.X = boundsFromTextPosition.Right;
				}
			}
			else if (position.LogicalDirection == LogicalDirection.Backward && position.Offset > 0)
			{
				boundsFromTextPosition.X = boundsFromTextPosition.Right;
			}
			boundsFromTextPosition.Width = 0.0;
		}
		return TransformToVisualSpace(boundsFromTextPosition);
	}

	Rect ITextView.GetRawRectangleFromTextPosition(ITextPointer position, out Transform transform)
	{
		transform = Transform.Identity;
		return ((ITextView)this).GetRectangleFromTextPosition(position);
	}

	Geometry ITextView.GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition)
	{
		Invariant.Assert(IsLayoutValid);
		Geometry geometry = null;
		double num = ((Control)_host).FontSize * 0.5;
		int num2 = Math.Min(_lineMetrics[_lineMetrics.Count - 1].EndOffset, startPosition.Offset);
		int num3 = Math.Min(_lineMetrics[_lineMetrics.Count - 1].EndOffset, endPosition.Offset);
		GetVisibleLines(out var firstLineIndex, out var lastLineIndex);
		firstLineIndex = Math.Max(firstLineIndex, GetLineIndexFromOffset(num2, LogicalDirection.Forward));
		lastLineIndex = Math.Min(lastLineIndex, GetLineIndexFromOffset(num3, LogicalDirection.Backward));
		if (firstLineIndex > lastLineIndex)
		{
			return null;
		}
		bool num4 = _lineMetrics[firstLineIndex].Offset < num2 || _lineMetrics[firstLineIndex].EndOffset > num3;
		bool flag = _lineMetrics[lastLineIndex].Offset < num2 || _lineMetrics[lastLineIndex].EndOffset > num3;
		TextAlignment calculatedTextAlignment = CalculatedTextAlignment;
		int i = firstLineIndex;
		if (num4)
		{
			GetTightBoundingGeometryFromLineIndex(i, num2, num3, calculatedTextAlignment, num, ref geometry);
			i++;
		}
		if (firstLineIndex <= lastLineIndex && !flag)
		{
			lastLineIndex++;
		}
		for (; i < lastLineIndex; i++)
		{
			double contentOffset = GetContentOffset(_lineMetrics[i].Width, calculatedTextAlignment);
			Rect rect = new Rect(contentOffset, (double)i * _lineHeight, _lineMetrics[i].Width, _lineHeight);
			if (TextPointerBase.IsNextToPlainLineBreak(_host.TextContainer.CreatePointerAtOffset(_lineMetrics[i].EndOffset, LogicalDirection.Backward), LogicalDirection.Backward))
			{
				rect.Width += num;
			}
			rect = TransformToVisualSpace(rect);
			CaretElement.AddGeometry(ref geometry, new RectangleGeometry(rect));
		}
		if (i == lastLineIndex && flag)
		{
			GetTightBoundingGeometryFromLineIndex(i, num2, num3, calculatedTextAlignment, num, ref geometry);
		}
		return geometry;
	}

	ITextPointer ITextView.GetPositionAtNextLine(ITextPointer position, double suggestedX, int count, out double newSuggestedX, out int linesMoved)
	{
		Invariant.Assert(IsLayoutValid);
		Invariant.Assert(Contains(position));
		newSuggestedX = suggestedX;
		int lineIndexFromPosition = GetLineIndexFromPosition(position);
		int num = Math.Max(0, Math.Min(_lineMetrics.Count - 1, lineIndexFromPosition + count));
		linesMoved = num - lineIndexFromPosition;
		ITextPointer textPointer;
		if (linesMoved == 0)
		{
			textPointer = position.GetFrozenPointer(position.LogicalDirection);
		}
		else if (double.IsNaN(suggestedX))
		{
			textPointer = _host.TextContainer.CreatePointerAtOffset(_lineMetrics[lineIndexFromPosition + linesMoved].Offset, LogicalDirection.Forward);
		}
		else
		{
			suggestedX -= GetTextAlignmentCorrection(CalculatedTextAlignment, GetWrappingWidth(base.RenderSize.Width));
			textPointer = GetTextPositionFromDistance(num, suggestedX);
		}
		textPointer.Freeze();
		return textPointer;
	}

	ITextPointer ITextView.GetPositionAtNextPage(ITextPointer position, Point suggestedOffset, int count, out Point newSuggestedOffset, out int pagesMoved)
	{
		Invariant.Assert(condition: false);
		newSuggestedOffset = default(Point);
		pagesMoved = 0;
		return null;
	}

	bool ITextView.IsAtCaretUnitBoundary(ITextPointer position)
	{
		Invariant.Assert(IsLayoutValid);
		Invariant.Assert(Contains(position));
		bool flag = false;
		int lineIndexFromPosition = GetLineIndexFromPosition(position);
		CharacterHit charHit = default(CharacterHit);
		if (position.LogicalDirection == LogicalDirection.Forward)
		{
			charHit = new CharacterHit(position.Offset, 0);
		}
		else if (position.LogicalDirection == LogicalDirection.Backward)
		{
			if (position.Offset <= _lineMetrics[lineIndexFromPosition].Offset)
			{
				return false;
			}
			charHit = new CharacterHit(position.Offset - 1, 1);
		}
		using TextBoxLine textBoxLine = GetFormattedLine(lineIndexFromPosition);
		return textBoxLine.IsAtCaretCharacterHit(charHit);
	}

	ITextPointer ITextView.GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction)
	{
		Invariant.Assert(IsLayoutValid);
		Invariant.Assert(Contains(position));
		if (position.Offset == 0 && direction == LogicalDirection.Backward)
		{
			return position.GetFrozenPointer(LogicalDirection.Forward);
		}
		if (position.Offset == _host.TextContainer.SymbolCount && direction == LogicalDirection.Forward)
		{
			return position.GetFrozenPointer(LogicalDirection.Backward);
		}
		int lineIndexFromPosition = GetLineIndexFromPosition(position);
		CharacterHit index = new CharacterHit(position.Offset, 0);
		CharacterHit characterHit;
		using (TextBoxLine textBoxLine = GetFormattedLine(lineIndexFromPosition))
		{
			characterHit = ((direction != LogicalDirection.Forward) ? textBoxLine.GetPreviousCaretCharacterHit(index) : textBoxLine.GetNextCaretCharacterHit(index));
		}
		LogicalDirection direction2 = ((characterHit.FirstCharacterIndex + characterHit.TrailingLength == _lineMetrics[lineIndexFromPosition].EndOffset && direction == LogicalDirection.Forward) ? ((lineIndexFromPosition != _lineMetrics.Count - 1) ? LogicalDirection.Forward : LogicalDirection.Backward) : (((characterHit.FirstCharacterIndex + characterHit.TrailingLength != _lineMetrics[lineIndexFromPosition].Offset || direction != 0) ? (characterHit.TrailingLength <= 0) : (lineIndexFromPosition == 0)) ? LogicalDirection.Forward : LogicalDirection.Backward));
		ITextPointer textPointer = _host.TextContainer.CreatePointerAtOffset(characterHit.FirstCharacterIndex + characterHit.TrailingLength, direction2);
		textPointer.Freeze();
		return textPointer;
	}

	ITextPointer ITextView.GetBackspaceCaretUnitPosition(ITextPointer position)
	{
		Invariant.Assert(IsLayoutValid);
		Invariant.Assert(Contains(position));
		if (position.Offset == 0)
		{
			return position.GetFrozenPointer(LogicalDirection.Forward);
		}
		int lineIndexFromPosition = GetLineIndexFromPosition(position, LogicalDirection.Backward);
		CharacterHit index = new CharacterHit(position.Offset, 0);
		CharacterHit backspaceCaretCharacterHit;
		using (TextBoxLine textBoxLine = GetFormattedLine(lineIndexFromPosition))
		{
			backspaceCaretCharacterHit = textBoxLine.GetBackspaceCaretCharacterHit(index);
		}
		LogicalDirection direction = (((backspaceCaretCharacterHit.FirstCharacterIndex + backspaceCaretCharacterHit.TrailingLength != _lineMetrics[lineIndexFromPosition].Offset) ? (backspaceCaretCharacterHit.TrailingLength <= 0) : (lineIndexFromPosition == 0)) ? LogicalDirection.Forward : LogicalDirection.Backward);
		ITextPointer textPointer = _host.TextContainer.CreatePointerAtOffset(backspaceCaretCharacterHit.FirstCharacterIndex + backspaceCaretCharacterHit.TrailingLength, direction);
		textPointer.Freeze();
		return textPointer;
	}

	TextSegment ITextView.GetLineRange(ITextPointer position)
	{
		Invariant.Assert(IsLayoutValid);
		Invariant.Assert(Contains(position));
		int lineIndexFromPosition = GetLineIndexFromPosition(position);
		ITextPointer startPosition = _host.TextContainer.CreatePointerAtOffset(_lineMetrics[lineIndexFromPosition].Offset, LogicalDirection.Forward);
		ITextPointer endPosition = _host.TextContainer.CreatePointerAtOffset(_lineMetrics[lineIndexFromPosition].Offset + _lineMetrics[lineIndexFromPosition].ContentLength, LogicalDirection.Forward);
		return new TextSegment(startPosition, endPosition, preserveLogicalDirection: true);
	}

	ReadOnlyCollection<GlyphRun> ITextView.GetGlyphRuns(ITextPointer start, ITextPointer end)
	{
		Invariant.Assert(condition: false);
		return null;
	}

	bool ITextView.Contains(ITextPointer position)
	{
		return Contains(position);
	}

	void ITextView.BringPositionIntoViewAsync(ITextPointer position, object userState)
	{
		Invariant.Assert(condition: false);
	}

	void ITextView.BringPointIntoViewAsync(Point point, object userState)
	{
		Invariant.Assert(condition: false);
	}

	void ITextView.BringLineIntoViewAsync(ITextPointer position, double suggestedX, int count, object userState)
	{
		Invariant.Assert(condition: false);
	}

	void ITextView.BringPageIntoViewAsync(ITextPointer position, Point suggestedOffset, int count, object userState)
	{
		Invariant.Assert(condition: false);
	}

	void ITextView.CancelAsync(object userState)
	{
		Invariant.Assert(condition: false);
	}

	bool ITextView.Validate()
	{
		UpdateLayout();
		return IsLayoutValid;
	}

	bool ITextView.Validate(Point point)
	{
		return ((ITextView)this).Validate();
	}

	bool ITextView.Validate(ITextPointer position)
	{
		if (position.TextContainer != _host.TextContainer)
		{
			return false;
		}
		if (!IsLayoutValid)
		{
			UpdateLayout();
			if (!IsLayoutValid)
			{
				return false;
			}
		}
		int num = _lineMetrics[_lineMetrics.Count - 1].EndOffset;
		while (!Contains(position))
		{
			InvalidateMeasure();
			UpdateLayout();
			if (!IsLayoutValid)
			{
				break;
			}
			int endOffset = _lineMetrics[_lineMetrics.Count - 1].EndOffset;
			if (num >= endOffset)
			{
				break;
			}
			num = endOffset;
		}
		if (IsLayoutValid)
		{
			return Contains(position);
		}
		return false;
	}

	void ITextView.ThrottleBackgroundTasksForUserInput()
	{
		if (_throttleBackgroundTimer == null)
		{
			_throttleBackgroundTimer = new DispatcherTimer(DispatcherPriority.Background);
			_throttleBackgroundTimer.Interval = new TimeSpan(0, 0, 2);
			_throttleBackgroundTimer.Tick += OnThrottleBackgroundTimeout;
		}
		else
		{
			_throttleBackgroundTimer.Stop();
		}
		_throttleBackgroundTimer.Start();
	}

	internal void Remeasure()
	{
		if (_lineMetrics != null)
		{
			_lineMetrics.Clear();
			_viewportLineVisuals = null;
		}
		InvalidateMeasure();
	}

	internal void Rerender()
	{
		_viewportLineVisuals = null;
		InvalidateArrange();
	}

	internal int GetLineIndexFromOffset(int offset)
	{
		int num = -1;
		int num2 = 0;
		int num3 = _lineMetrics.Count;
		Invariant.Assert(_lineMetrics.Count >= 1);
		LineRecord lineRecord;
		while (true)
		{
			Invariant.Assert(num2 < num3, "Couldn't find offset!");
			num = num2 + (num3 - num2) / 2;
			lineRecord = _lineMetrics[num];
			if (offset < lineRecord.Offset)
			{
				num3 = num;
				continue;
			}
			if (offset <= lineRecord.EndOffset)
			{
				break;
			}
			num2 = num + 1;
		}
		if (offset == lineRecord.EndOffset && num < _lineMetrics.Count - 1)
		{
			num++;
		}
		return num;
	}

	internal void RemoveTextContainerListeners()
	{
		if (CheckFlags(Flags.TextContainerListenersInitialized))
		{
			((Control)_host).Unloaded -= OnHostUnloaded;
			_host.TextContainer.Changing -= OnTextContainerChanging;
			_host.TextContainer.Change -= OnTextContainerChange;
			_host.TextContainer.Highlights.Changed -= OnHighlightChanged;
			SetFlags(value: false, Flags.TextContainerListenersInitialized);
		}
	}

	private void EnsureTextContainerListeners()
	{
		if (!CheckFlags(Flags.TextContainerListenersInitialized))
		{
			((Control)_host).Unloaded += OnHostUnloaded;
			_host.TextContainer.Changing += OnTextContainerChanging;
			_host.TextContainer.Change += OnTextContainerChange;
			_host.TextContainer.Highlights.Changed += OnHighlightChanged;
			SetFlags(value: true, Flags.TextContainerListenersInitialized);
		}
	}

	private void EnsureCache()
	{
		if (_cache == null)
		{
			_cache = new TextCache(this);
		}
	}

	private LineProperties GetLineProperties()
	{
		TextProperties defaultTextProperties = new TextProperties((Control)_host, _host.IsTypographyDefaultValue);
		return new LineProperties((Control)_host, (Control)_host, defaultTextProperties, null, CalculatedTextAlignment);
	}

	private void OnTextContainerChanging(object sender, EventArgs args)
	{
	}

	private void OnTextContainerChange(object sender, TextContainerChangeEventArgs args)
	{
		if (args.Count != 0)
		{
			if (_dirtyList == null)
			{
				_dirtyList = new DtrList();
			}
			DirtyTextRange dtr = new DirtyTextRange(args);
			_dirtyList.Merge(dtr);
			InvalidateMeasure();
		}
	}

	private void OnHighlightChanged(object sender, HighlightChangedEventArgs args)
	{
		if (args.OwnerType != typeof(SpellerHighlightLayer) && (!((ITextView)this).RendersOwnSelection || args.OwnerType != typeof(TextSelection)))
		{
			return;
		}
		bool measureNeeded = false;
		bool arrangeNeeded = false;
		if (_dirtyList == null)
		{
			_dirtyList = new DtrList();
		}
		DtrList dtrList = new DtrList();
		foreach (TextSegment range in args.Ranges)
		{
			int num = range.End.Offset - range.Start.Offset;
			DirtyTextRange dtr = new DirtyTextRange(range.Start.Offset, num, num, fromHighlightLayer: true);
			dtrList.Merge(dtr);
		}
		DirtyTextRange mergedRange = dtrList.GetMergedRange();
		if (args.OwnerType == typeof(TextSelection))
		{
			HandleTextSelectionHighlightChange(mergedRange, ref arrangeNeeded, ref measureNeeded);
		}
		else if (args.OwnerType == typeof(SpellerHighlightLayer))
		{
			_dirtyList.Merge(mergedRange);
			measureNeeded = true;
		}
		if (measureNeeded)
		{
			InvalidateMeasure();
		}
		else if (arrangeNeeded)
		{
			InvalidateArrange();
		}
	}

	private void HandleTextSelectionHighlightChange(DirtyTextRange currentSelectionRange, ref bool arrangeNeeded, ref bool measureNeeded)
	{
		if (_lineMetrics.Count == 0)
		{
			measureNeeded = true;
			return;
		}
		if (_dirtyList.Length > 0 && _dirtyList.DtrsFromRange(currentSelectionRange.StartIndex, currentSelectionRange.PositionsAdded) != null)
		{
			_dirtyList.Merge(currentSelectionRange);
			measureNeeded = true;
			return;
		}
		int[] array = new int[2]
		{
			currentSelectionRange.StartIndex,
			currentSelectionRange.StartIndex + currentSelectionRange.PositionsAdded
		};
		using (TextBoxLine textBoxLine = new TextBoxLine(this))
		{
			Control element = (Control)_host;
			LineProperties lineProperties = GetLineProperties();
			TextFormatter formatter = TextFormatter.FromCurrentDispatcher(TextOptions.GetTextFormattingMode(element));
			double wrappingWidth = GetWrappingWidth(base.RenderSize.Width);
			double wrappingWidth2 = GetWrappingWidth(_previousConstraint.Width);
			int[] array2 = array;
			foreach (int offset in array2)
			{
				int lineIndexFromOffset = GetLineIndexFromOffset(offset);
				LineRecord lineRecord = _lineMetrics[lineIndexFromOffset];
				textBoxLine.Format(lineRecord.Offset, wrappingWidth2, wrappingWidth, lineProperties, new TextRunCache(), formatter);
				if (lineRecord.Length != textBoxLine.Length)
				{
					measureNeeded = true;
					_dirtyList.Merge(new DirtyTextRange(lineRecord.Offset, lineRecord.Length, lineRecord.Length, fromHighlightLayer: true));
				}
			}
		}
		if (!measureNeeded)
		{
			DirtyTextRange? selectionRenderRange = GetSelectionRenderRange(currentSelectionRange);
			if (selectionRenderRange.HasValue)
			{
				_dirtyList.Merge(selectionRenderRange.Value);
				arrangeNeeded = true;
				SetFlags(value: true, Flags.ArrangePendingFromHighlightLayer);
			}
			else if (_dirtyList.Length == 0)
			{
				_dirtyList = null;
			}
		}
	}

	private void SetFlags(bool value, Flags flags)
	{
		_flags = (value ? (_flags | flags) : (_flags & ~flags));
	}

	private bool CheckFlags(Flags flags)
	{
		return (_flags & flags) == flags;
	}

	private void FireTextViewUpdatedEvent()
	{
		if (UpdatedEvent != null)
		{
			UpdatedEvent(this, EventArgs.Empty);
		}
	}

	private int GetLineIndexFromPoint(Point point, bool snapToText)
	{
		Invariant.Assert(_lineMetrics.Count >= 1);
		if (point.Y < 0.0)
		{
			if (!snapToText)
			{
				return -1;
			}
			return 0;
		}
		if (point.Y >= _lineHeight * (double)_lineMetrics.Count)
		{
			if (!snapToText)
			{
				return -1;
			}
			return _lineMetrics.Count - 1;
		}
		int num = -1;
		int num2 = 0;
		int num3 = _lineMetrics.Count;
		while (num2 < num3)
		{
			num = num2 + (num3 - num2) / 2;
			LineRecord lineRecord = _lineMetrics[num];
			double num4 = _lineHeight * (double)num;
			if (point.Y < num4)
			{
				num3 = num;
				continue;
			}
			if (point.Y >= num4 + _lineHeight)
			{
				num2 = num + 1;
				continue;
			}
			if (!snapToText)
			{
				double contentOffset = GetContentOffset(lineRecord.Width, CalculatedTextAlignment);
				if (point.X < contentOffset || point.X >= lineRecord.Width + contentOffset)
				{
					num = -1;
				}
			}
			break;
		}
		if (num2 >= num3)
		{
			return -1;
		}
		return num;
	}

	private int GetLineIndexFromPosition(ITextPointer position)
	{
		return GetLineIndexFromOffset(position.Offset, position.LogicalDirection);
	}

	private int GetLineIndexFromPosition(ITextPointer position, LogicalDirection direction)
	{
		return GetLineIndexFromOffset(position.Offset, direction);
	}

	private int GetLineIndexFromOffset(int offset, LogicalDirection direction)
	{
		if (offset > 0 && direction == LogicalDirection.Backward)
		{
			offset--;
		}
		return GetLineIndexFromOffset(offset);
	}

	private TextBoxLine GetFormattedLine(int lineIndex)
	{
		LineProperties lineProperties;
		return GetFormattedLine(lineIndex, out lineProperties);
	}

	private TextBoxLine GetFormattedLine(int lineIndex, out LineProperties lineProperties)
	{
		TextBoxLine textBoxLine = new TextBoxLine(this);
		LineRecord lineRecord = _lineMetrics[lineIndex];
		lineProperties = GetLineProperties();
		TextFormatter formatter = TextFormatter.FromCurrentDispatcher(TextOptions.GetTextFormattingMode((Control)_host));
		double wrappingWidth = GetWrappingWidth(base.RenderSize.Width);
		double wrappingWidth2 = GetWrappingWidth(_previousConstraint.Width);
		textBoxLine.Format(lineRecord.Offset, wrappingWidth2, wrappingWidth, lineProperties, new TextRunCache(), formatter);
		Invariant.Assert(lineRecord.Length == textBoxLine.Length, "Line is out of sync with metrics!");
		return textBoxLine;
	}

	private ITextPointer GetTextPositionFromDistance(int lineIndex, double x)
	{
		LineProperties lineProperties;
		CharacterHit textPositionFromDistance;
		LogicalDirection direction;
		using (TextBoxLine textBoxLine = GetFormattedLine(lineIndex, out lineProperties))
		{
			textPositionFromDistance = textBoxLine.GetTextPositionFromDistance(x);
			direction = ((textPositionFromDistance.TrailingLength <= 0) ? LogicalDirection.Forward : LogicalDirection.Backward);
		}
		return _host.TextContainer.CreatePointerAtOffset(textPositionFromDistance.FirstCharacterIndex + textPositionFromDistance.TrailingLength, direction);
	}

	private void ArrangeScrollData(Size arrangeSize)
	{
		if (_scrollData != null)
		{
			bool flag = false;
			if (!DoubleUtil.AreClose(_scrollData.Viewport, arrangeSize))
			{
				_scrollData.Viewport = arrangeSize;
				flag = true;
			}
			if (!DoubleUtil.AreClose(_scrollData.Extent, _contentSize))
			{
				_scrollData.Extent = _contentSize;
				flag = true;
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
		}
	}

	private void ArrangeVisuals(Size arrangeSize)
	{
		Invariant.Assert(CheckFlags(Flags.ArrangePendingFromHighlightLayer) || _dirtyList == null);
		SetFlags(value: false, Flags.ArrangePendingFromHighlightLayer);
		if (_dirtyList != null)
		{
			InvalidateDirtyVisuals();
			_dirtyList = null;
		}
		if (_visualChildren == null)
		{
			_visualChildren = new List<TextBoxLineDrawingVisual>(1);
		}
		EnsureCache();
		LineProperties lineProperties = _cache.LineProperties;
		TextBoxLine textBoxLine = new TextBoxLine(this);
		GetVisibleLines(out var firstLineIndex, out var lastLineIndex);
		SetViewportLines(firstLineIndex, lastLineIndex);
		double wrappingWidth = GetWrappingWidth(arrangeSize.Width);
		double num = GetTextAlignmentCorrection(lineProperties.TextAlignment, wrappingWidth);
		double num2 = VerticalAlignmentOffset;
		if (_scrollData != null)
		{
			num -= _scrollData.HorizontalOffset;
			num2 -= _scrollData.VerticalOffset;
		}
		DetachDiscardedVisualChildren();
		double wrappingWidth2 = GetWrappingWidth(_previousConstraint.Width);
		double endOfParaGlyphWidth = ((Control)_host).FontSize * 0.5;
		bool flag = ((ITextView)this).RendersOwnSelection && ((bool)((Control)_host).GetValue(TextBoxBase.IsInactiveSelectionHighlightEnabledProperty) || (bool)((Control)_host).GetValue(TextBoxBase.IsSelectionActiveProperty));
		for (int i = firstLineIndex; i <= lastLineIndex; i++)
		{
			TextBoxLineDrawingVisual textBoxLineDrawingVisual = GetLineVisual(i);
			if (textBoxLineDrawingVisual == null)
			{
				LineRecord lineRecord = _lineMetrics[i];
				using (textBoxLine)
				{
					textBoxLine.Format(lineRecord.Offset, wrappingWidth2, wrappingWidth, lineProperties, _cache.TextRunCache, _cache.TextFormatter);
					if (!IsBackgroundLayoutPending)
					{
						Invariant.Assert(lineRecord.Length == textBoxLine.Length, "Line is out of sync with metrics!");
					}
					Geometry geometry = null;
					if (flag)
					{
						ITextSelection textSelection = _host.TextContainer.TextSelection;
						if (!textSelection.IsEmpty)
						{
							GetTightBoundingGeometryFromLineIndexForSelection(textBoxLine, i, textSelection.Start.CharOffset, textSelection.End.CharOffset, CalculatedTextAlignment, endOfParaGlyphWidth, ref geometry);
						}
					}
					textBoxLineDrawingVisual = textBoxLine.CreateVisual(geometry);
				}
				SetLineVisual(i, textBoxLineDrawingVisual);
				AttachVisualChild(textBoxLineDrawingVisual);
			}
			textBoxLineDrawingVisual.Offset = new Vector(num, num2 + (double)i * _lineHeight);
		}
	}

	private void InvalidateDirtyVisuals()
	{
		for (int i = 0; i < _dirtyList.Length; i++)
		{
			DirtyTextRange dirtyTextRange = _dirtyList[i];
			Invariant.Assert(dirtyTextRange.FromHighlightLayer);
			Invariant.Assert(dirtyTextRange.PositionsAdded == dirtyTextRange.PositionsRemoved);
			int lineIndexFromOffset = GetLineIndexFromOffset(dirtyTextRange.StartIndex, LogicalDirection.Forward);
			int offset = Math.Min(dirtyTextRange.StartIndex + dirtyTextRange.PositionsAdded, _host.TextContainer.SymbolCount);
			int lineIndexFromOffset2 = GetLineIndexFromOffset(offset, LogicalDirection.Backward);
			for (int j = lineIndexFromOffset; j <= lineIndexFromOffset2; j++)
			{
				ClearLineVisual(j);
			}
		}
	}

	private void DetachDiscardedVisualChildren()
	{
		int num = _visualChildren.Count - 1;
		for (int num2 = _visualChildren.Count - 1; num2 >= 0; num2--)
		{
			if (_visualChildren[num2] == null || _visualChildren[num2].DiscardOnArrange)
			{
				RemoveVisualChild(_visualChildren[num2]);
				if (num2 < num)
				{
					_visualChildren[num2] = _visualChildren[num];
				}
				num--;
			}
		}
		if (num < _visualChildren.Count - 1)
		{
			_visualChildren.RemoveRange(num + 1, _visualChildren.Count - num - 1);
		}
	}

	private void AttachVisualChild(TextBoxLineDrawingVisual lineVisual)
	{
		lineVisual._parentIndex = _visualChildren.Count;
		AddVisualChild(lineVisual);
		_visualChildren.Add(lineVisual);
	}

	private void ClearVisualChildren()
	{
		for (int i = 0; i < _visualChildren.Count; i++)
		{
			RemoveVisualChild(_visualChildren[i]);
		}
		_visualChildren.Clear();
	}

	private Point TransformToDocumentSpace(Point point)
	{
		if (_scrollData != null)
		{
			point = new Point(point.X + _scrollData.HorizontalOffset, point.Y + _scrollData.VerticalOffset);
		}
		point.X -= GetTextAlignmentCorrection(CalculatedTextAlignment, GetWrappingWidth(base.RenderSize.Width));
		point.Y -= VerticalAlignmentOffset;
		return point;
	}

	private Rect TransformToVisualSpace(Rect rect)
	{
		if (_scrollData != null)
		{
			rect.X -= _scrollData.HorizontalOffset;
			rect.Y -= _scrollData.VerticalOffset;
		}
		rect.X += GetTextAlignmentCorrection(CalculatedTextAlignment, GetWrappingWidth(base.RenderSize.Width));
		rect.Y += VerticalAlignmentOffset;
		return rect;
	}

	private void GetTightBoundingGeometryFromLineIndex(int lineIndex, int unclippedStartOffset, int unclippedEndOffset, TextAlignment alignment, double endOfParaGlyphWidth, ref Geometry geometry)
	{
		int num = Math.Max(_lineMetrics[lineIndex].Offset, unclippedStartOffset);
		int num2 = Math.Min(_lineMetrics[lineIndex].EndOffset, unclippedEndOffset);
		if (num == num2)
		{
			if (unclippedStartOffset == _lineMetrics[lineIndex].EndOffset)
			{
				if (TextPointerBase.IsNextToPlainLineBreak(_host.TextContainer.CreatePointerAtOffset(unclippedStartOffset, LogicalDirection.Backward), LogicalDirection.Backward))
				{
					Rect rect = new Rect(0.0, (double)lineIndex * _lineHeight, endOfParaGlyphWidth, _lineHeight);
					CaretElement.AddGeometry(ref geometry, new RectangleGeometry(rect));
				}
			}
			else
			{
				Invariant.Assert(num2 == _lineMetrics[lineIndex].Offset || num2 == _lineMetrics[lineIndex].Offset + _lineMetrics[lineIndex].ContentLength);
			}
			return;
		}
		IList<Rect> rangeBounds;
		using (TextBoxLine textBoxLine = GetFormattedLine(lineIndex))
		{
			rangeBounds = textBoxLine.GetRangeBounds(num, num2 - num, 0.0, (double)lineIndex * _lineHeight);
		}
		for (int i = 0; i < rangeBounds.Count; i++)
		{
			Rect rect2 = TransformToVisualSpace(rangeBounds[i]);
			CaretElement.AddGeometry(ref geometry, new RectangleGeometry(rect2));
		}
		if (unclippedEndOffset >= _lineMetrics[lineIndex].EndOffset && TextPointerBase.IsNextToPlainLineBreak(_host.TextContainer.CreatePointerAtOffset(num2, LogicalDirection.Backward), LogicalDirection.Backward))
		{
			double contentOffset = GetContentOffset(_lineMetrics[lineIndex].Width, alignment);
			Rect rect3 = new Rect(contentOffset + _lineMetrics[lineIndex].Width, (double)lineIndex * _lineHeight, endOfParaGlyphWidth, _lineHeight);
			rect3 = TransformToVisualSpace(rect3);
			CaretElement.AddGeometry(ref geometry, new RectangleGeometry(rect3));
		}
	}

	private void GetTightBoundingGeometryFromLineIndexForSelection(TextBoxLine line, int lineIndex, int unclippedStartOffset, int unclippedEndOffset, TextAlignment alignment, double endOfParaGlyphWidth, ref Geometry geometry)
	{
		int offset = _lineMetrics[lineIndex].Offset;
		int endOffset = _lineMetrics[lineIndex].EndOffset;
		if (offset > unclippedEndOffset || endOffset <= unclippedStartOffset)
		{
			return;
		}
		int num = Math.Max(offset, unclippedStartOffset);
		int num2 = Math.Min(endOffset, unclippedEndOffset);
		if (num == num2)
		{
			if (unclippedStartOffset == _lineMetrics[lineIndex].EndOffset)
			{
				if (TextPointerBase.IsNextToPlainLineBreak(_host.TextContainer.CreatePointerAtOffset(unclippedStartOffset, LogicalDirection.Backward), LogicalDirection.Backward))
				{
					Rect rect = new Rect(0.0, 0.0, endOfParaGlyphWidth, _lineHeight);
					CaretElement.AddGeometry(ref geometry, new RectangleGeometry(rect));
				}
			}
			else
			{
				Invariant.Assert(num2 == _lineMetrics[lineIndex].Offset || num2 == _lineMetrics[lineIndex].Offset + _lineMetrics[lineIndex].ContentLength);
			}
			return;
		}
		IList<Rect> rangeBounds = line.GetRangeBounds(num, num2 - num, 0.0, 0.0);
		for (int i = 0; i < rangeBounds.Count; i++)
		{
			Rect rect2 = rangeBounds[i];
			CaretElement.AddGeometry(ref geometry, new RectangleGeometry(rect2));
		}
		if (unclippedEndOffset >= _lineMetrics[lineIndex].EndOffset && TextPointerBase.IsNextToPlainLineBreak(_host.TextContainer.CreatePointerAtOffset(num2, LogicalDirection.Backward), LogicalDirection.Backward))
		{
			double contentOffset = GetContentOffset(_lineMetrics[lineIndex].Width, alignment);
			Rect rect3 = new Rect(contentOffset + _lineMetrics[lineIndex].Width, 0.0, endOfParaGlyphWidth, _lineHeight);
			CaretElement.AddGeometry(ref geometry, new RectangleGeometry(rect3));
		}
	}

	private void GetVisibleLines(out int firstLineIndex, out int lastLineIndex)
	{
		Rect viewport = Viewport;
		if (!viewport.IsEmpty)
		{
			firstLineIndex = (int)(viewport.Y / _lineHeight);
			lastLineIndex = (int)Math.Ceiling((viewport.Y + viewport.Height) / _lineHeight) - 1;
			firstLineIndex = Math.Max(0, Math.Min(firstLineIndex, _lineMetrics.Count - 1));
			lastLineIndex = Math.Max(0, Math.Min(lastLineIndex, _lineMetrics.Count - 1));
		}
		else
		{
			firstLineIndex = 0;
			lastLineIndex = _lineMetrics.Count - 1;
		}
	}

	private Size FullMeasureTick(double constraintWidth, LineProperties lineProperties)
	{
		TextBoxLine textBoxLine = new TextBoxLine(this);
		Size result;
		int num;
		if (_lineMetrics.Count == 0)
		{
			result = default(Size);
			num = 0;
		}
		else
		{
			result = _contentSize;
			num = _lineMetrics[_lineMetrics.Count - 1].EndOffset;
		}
		DateTime dateTime = (((ScrollBarVisibility)((Control)_host).GetValue(ScrollViewer.VerticalScrollBarVisibilityProperty) != ScrollBarVisibility.Auto) ? DateTime.Now.AddMilliseconds(200.0) : DateTime.MaxValue);
		bool endOfParagraph;
		do
		{
			using (textBoxLine)
			{
				textBoxLine.Format(num, constraintWidth, constraintWidth, lineProperties, _cache.TextRunCache, _cache.TextFormatter);
				_lineHeight = lineProperties.CalcLineAdvance(textBoxLine.Height);
				_lineMetrics.Add(new LineRecord(num, textBoxLine));
				result.Width = Math.Max(result.Width, textBoxLine.Width);
				result.Height += _lineHeight;
				num += textBoxLine.Length;
				endOfParagraph = textBoxLine.EndOfParagraph;
			}
		}
		while (!endOfParagraph && DateTime.Now < dateTime);
		if (!endOfParagraph)
		{
			SetFlags(value: true, Flags.BackgroundLayoutPending);
			base.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(OnBackgroundMeasure), null);
		}
		else
		{
			SetFlags(value: false, Flags.BackgroundLayoutPending);
		}
		return result;
	}

	private object OnBackgroundMeasure(object o)
	{
		if (_throttleBackgroundTimer == null)
		{
			InvalidateMeasure();
		}
		return null;
	}

	private Size IncrementalMeasure(double constraintWidth, LineProperties lineProperties)
	{
		Invariant.Assert(_dirtyList != null);
		Invariant.Assert(_dirtyList.Length > 0);
		Size desiredSize = _contentSize;
		DirtyTextRange range = _dirtyList[0];
		if (range.StartIndex > _lineMetrics[_lineMetrics.Count - 1].EndOffset)
		{
			Invariant.Assert(IsBackgroundLayoutPending);
			return desiredSize;
		}
		int startIndex = range.StartIndex;
		int num = range.PositionsAdded;
		int num2 = range.PositionsRemoved;
		for (int i = 1; i < _dirtyList.Length; i++)
		{
			range = _dirtyList[i];
			if (range.StartIndex > _lineMetrics[_lineMetrics.Count - 1].EndOffset)
			{
				Invariant.Assert(IsBackgroundLayoutPending);
				break;
			}
			int num3 = range.StartIndex - startIndex;
			num += num3 + range.PositionsAdded;
			num2 += num3 + range.PositionsRemoved;
			startIndex = range.StartIndex;
		}
		range = new DirtyTextRange(_dirtyList[0].StartIndex, num, num2);
		if (range.PositionsAdded >= range.PositionsRemoved)
		{
			IncrementalMeasureLinesAfterInsert(constraintWidth, lineProperties, range, ref desiredSize);
		}
		else if (range.PositionsAdded < range.PositionsRemoved)
		{
			IncrementalMeasureLinesAfterDelete(constraintWidth, lineProperties, range, ref desiredSize);
		}
		return desiredSize;
	}

	private void IncrementalMeasureLinesAfterInsert(double constraintWidth, LineProperties lineProperties, DirtyTextRange range, ref Size desiredSize)
	{
		int num = range.PositionsAdded - range.PositionsRemoved;
		Invariant.Assert(num >= 0);
		int num2 = GetLineIndexFromOffset(range.StartIndex, LogicalDirection.Forward);
		if (num > 0)
		{
			for (int i = num2 + 1; i < _lineMetrics.Count; i++)
			{
				_lineMetrics[i].Offset += num;
			}
		}
		TextBoxLine textBoxLine = new TextBoxLine(this);
		bool endOfParagraph = false;
		int lineOffset;
		if (num2 > 0)
		{
			FormatFirstIncrementalLine(num2 - 1, constraintWidth, lineProperties, textBoxLine, out lineOffset, out endOfParagraph);
		}
		else
		{
			lineOffset = _lineMetrics[num2].Offset;
		}
		if (!endOfParagraph)
		{
			using (textBoxLine)
			{
				textBoxLine.Format(lineOffset, constraintWidth, constraintWidth, lineProperties, _cache.TextRunCache, _cache.TextFormatter);
				_lineMetrics[num2] = new LineRecord(lineOffset, textBoxLine);
				lineOffset += textBoxLine.Length;
				endOfParagraph = textBoxLine.EndOfParagraph;
			}
			ClearLineVisual(num2);
			num2++;
		}
		SyncLineMetrics(range, constraintWidth, lineProperties, textBoxLine, endOfParagraph, num2, lineOffset);
		desiredSize = BruteForceCalculateDesiredSize();
	}

	private void IncrementalMeasureLinesAfterDelete(double constraintWidth, LineProperties lineProperties, DirtyTextRange range, ref Size desiredSize)
	{
		int num = range.PositionsAdded - range.PositionsRemoved;
		Invariant.Assert(num < 0);
		int lineIndexFromOffset = GetLineIndexFromOffset(range.StartIndex);
		int num2 = range.StartIndex + -num - 1;
		if (num2 > _lineMetrics[_lineMetrics.Count - 1].EndOffset)
		{
			Invariant.Assert(IsBackgroundLayoutPending);
			num2 = _lineMetrics[_lineMetrics.Count - 1].EndOffset;
			if (range.StartIndex == num2)
			{
				return;
			}
		}
		int lineIndexFromOffset2 = GetLineIndexFromOffset(num2);
		for (int i = lineIndexFromOffset2 + 1; i < _lineMetrics.Count; i++)
		{
			_lineMetrics[i].Offset += num;
		}
		TextBoxLine textBoxLine = new TextBoxLine(this);
		int num3 = lineIndexFromOffset;
		int lineOffset;
		bool endOfParagraph;
		if (num3 > 0)
		{
			FormatFirstIncrementalLine(num3 - 1, constraintWidth, lineProperties, textBoxLine, out lineOffset, out endOfParagraph);
		}
		else
		{
			lineOffset = _lineMetrics[num3].Offset;
			endOfParagraph = false;
		}
		if (!endOfParagraph && (range.StartIndex > lineOffset || range.StartIndex + -num < _lineMetrics[num3].EndOffset))
		{
			using (textBoxLine)
			{
				textBoxLine.Format(lineOffset, constraintWidth, constraintWidth, lineProperties, _cache.TextRunCache, _cache.TextFormatter);
				_lineMetrics[num3] = new LineRecord(lineOffset, textBoxLine);
				lineOffset += textBoxLine.Length;
				endOfParagraph = textBoxLine.EndOfParagraph;
			}
			ClearLineVisual(num3);
			num3++;
		}
		_lineMetrics.RemoveRange(num3, lineIndexFromOffset2 - num3 + 1);
		RemoveLineVisualRange(num3, lineIndexFromOffset2 - num3 + 1);
		SyncLineMetrics(range, constraintWidth, lineProperties, textBoxLine, endOfParagraph, num3, lineOffset);
		desiredSize = BruteForceCalculateDesiredSize();
	}

	private void FormatFirstIncrementalLine(int lineIndex, double constraintWidth, LineProperties lineProperties, TextBoxLine line, out int lineOffset, out bool endOfParagraph)
	{
		int endOffset = _lineMetrics[lineIndex].EndOffset;
		lineOffset = _lineMetrics[lineIndex].Offset;
		using (line)
		{
			line.Format(lineOffset, constraintWidth, constraintWidth, lineProperties, _cache.TextRunCache, _cache.TextFormatter);
			_lineMetrics[lineIndex] = new LineRecord(lineOffset, line);
			lineOffset += line.Length;
			endOfParagraph = line.EndOfParagraph;
		}
		if (endOffset != _lineMetrics[lineIndex].EndOffset)
		{
			ClearLineVisual(lineIndex);
		}
	}

	private void SyncLineMetrics(DirtyTextRange range, double constraintWidth, LineProperties lineProperties, TextBoxLine line, bool endOfParagraph, int lineIndex, int lineOffset)
	{
		bool flag = range.PositionsAdded == 0 || range.PositionsRemoved == 0;
		int num = range.StartIndex + Math.Max(range.PositionsAdded, range.PositionsRemoved);
		while (!endOfParagraph && (lineIndex == _lineMetrics.Count || !flag || lineOffset != _lineMetrics[lineIndex].Offset))
		{
			if (lineIndex < _lineMetrics.Count && lineOffset >= _lineMetrics[lineIndex].EndOffset)
			{
				_lineMetrics.RemoveAt(lineIndex);
				RemoveLineVisualRange(lineIndex, 1);
				continue;
			}
			using (line)
			{
				line.Format(lineOffset, constraintWidth, constraintWidth, lineProperties, _cache.TextRunCache, _cache.TextFormatter);
				LineRecord lineRecord = new LineRecord(lineOffset, line);
				if (lineIndex == _lineMetrics.Count || lineOffset + line.Length <= _lineMetrics[lineIndex].Offset)
				{
					_lineMetrics.Insert(lineIndex, lineRecord);
					AddLineVisualPlaceholder(lineIndex);
					goto IL_01ad;
				}
				Invariant.Assert(lineOffset < _lineMetrics[lineIndex].EndOffset);
				LineRecord lineRecord2 = _lineMetrics[lineIndex];
				if (range.FromHighlightLayer && lineRecord2.Offset > num && lineRecord2.ContentLength == lineRecord.ContentLength && lineRecord2.EndOffset == lineRecord.EndOffset && lineRecord2.Length == lineRecord.Length && lineRecord2.Offset == lineRecord.Offset && DoubleUtilities.AreClose(lineRecord2.Width, lineRecord.Width))
				{
					break;
				}
				_lineMetrics[lineIndex] = lineRecord;
				ClearLineVisual(lineIndex);
				flag |= num <= lineRecord.EndOffset && line.HasLineBreak;
				goto IL_01ad;
				IL_01ad:
				lineIndex++;
				lineOffset += line.Length;
				endOfParagraph = line.EndOfParagraph;
				continue;
			}
		}
		if (endOfParagraph && lineIndex < _lineMetrics.Count)
		{
			int count = _lineMetrics.Count - lineIndex;
			_lineMetrics.RemoveRange(lineIndex, count);
			RemoveLineVisualRange(lineIndex, count);
		}
	}

	private Size BruteForceCalculateDesiredSize()
	{
		Size result = default(Size);
		for (int i = 0; i < _lineMetrics.Count; i++)
		{
			result.Width = Math.Max(result.Width, _lineMetrics[i].Width);
		}
		result.Height = (double)_lineMetrics.Count * _lineHeight;
		return result;
	}

	private void SetViewportLines(int firstLineIndex, int lastLineIndex)
	{
		List<TextBoxLineDrawingVisual> viewportLineVisuals = _viewportLineVisuals;
		int viewportLineVisualsIndex = _viewportLineVisualsIndex;
		_viewportLineVisuals = null;
		_viewportLineVisualsIndex = -1;
		int num = lastLineIndex - firstLineIndex + 1;
		if (num <= 1)
		{
			ClearVisualChildren();
			return;
		}
		_viewportLineVisuals = new List<TextBoxLineDrawingVisual>(num);
		_viewportLineVisuals.AddRange(new TextBoxLineDrawingVisual[num]);
		_viewportLineVisualsIndex = firstLineIndex;
		if (viewportLineVisuals == null)
		{
			ClearVisualChildren();
			return;
		}
		int num2 = viewportLineVisualsIndex + viewportLineVisuals.Count - 1;
		if (viewportLineVisualsIndex <= lastLineIndex && num2 >= firstLineIndex)
		{
			int num3 = Math.Max(viewportLineVisualsIndex, firstLineIndex);
			int num4 = Math.Min(num2, firstLineIndex + num - 1) - num3 + 1;
			for (int i = 0; i < num4; i++)
			{
				_viewportLineVisuals[num3 - _viewportLineVisualsIndex + i] = viewportLineVisuals[num3 - viewportLineVisualsIndex + i];
			}
			for (int j = 0; j < num3 - viewportLineVisualsIndex; j++)
			{
				if (viewportLineVisuals[j] != null)
				{
					viewportLineVisuals[j].DiscardOnArrange = true;
				}
			}
			for (int k = num3 - viewportLineVisualsIndex + num4; k < viewportLineVisuals.Count; k++)
			{
				if (viewportLineVisuals[k] != null)
				{
					viewportLineVisuals[k].DiscardOnArrange = true;
				}
			}
		}
		else
		{
			ClearVisualChildren();
		}
	}

	private TextBoxLineDrawingVisual GetLineVisual(int lineIndex)
	{
		TextBoxLineDrawingVisual result = null;
		if (_viewportLineVisuals != null)
		{
			result = _viewportLineVisuals[lineIndex - _viewportLineVisualsIndex];
		}
		return result;
	}

	private void SetLineVisual(int lineIndex, TextBoxLineDrawingVisual lineVisual)
	{
		if (_viewportLineVisuals != null)
		{
			_viewportLineVisuals[lineIndex - _viewportLineVisualsIndex] = lineVisual;
		}
	}

	private void AddLineVisualPlaceholder(int lineIndex)
	{
		if (_viewportLineVisuals != null && lineIndex >= _viewportLineVisualsIndex && lineIndex < _viewportLineVisualsIndex + _viewportLineVisuals.Count)
		{
			_viewportLineVisuals.Insert(lineIndex - _viewportLineVisualsIndex, null);
		}
	}

	private void ClearLineVisual(int lineIndex)
	{
		if (_viewportLineVisuals != null && lineIndex >= _viewportLineVisualsIndex && lineIndex < _viewportLineVisualsIndex + _viewportLineVisuals.Count && _viewportLineVisuals[lineIndex - _viewportLineVisualsIndex] != null)
		{
			_viewportLineVisuals[lineIndex - _viewportLineVisualsIndex].DiscardOnArrange = true;
			_viewportLineVisuals[lineIndex - _viewportLineVisualsIndex] = null;
		}
	}

	private void RemoveLineVisualRange(int lineIndex, int count)
	{
		if (_viewportLineVisuals == null)
		{
			return;
		}
		if (lineIndex < _viewportLineVisualsIndex)
		{
			count -= _viewportLineVisualsIndex - lineIndex;
			count = Math.Max(0, count);
			lineIndex = _viewportLineVisualsIndex;
		}
		if (lineIndex >= _viewportLineVisualsIndex + _viewportLineVisuals.Count)
		{
			return;
		}
		int num = lineIndex - _viewportLineVisualsIndex;
		count = Math.Min(count, _viewportLineVisuals.Count - num);
		for (int i = 0; i < count; i++)
		{
			if (_viewportLineVisuals[num + i] != null)
			{
				_viewportLineVisuals[num + i].DiscardOnArrange = true;
			}
		}
		_viewportLineVisuals.RemoveRange(num, count);
	}

	private void OnThrottleBackgroundTimeout(object sender, EventArgs e)
	{
		StopAndClearThrottleBackgroundTimer();
		if (IsBackgroundLayoutPending)
		{
			OnBackgroundMeasure(null);
		}
	}

	private void OnHostUnloaded(object sender, RoutedEventArgs e)
	{
		StopAndClearThrottleBackgroundTimer();
	}

	public void StopAndClearThrottleBackgroundTimer()
	{
		if (_throttleBackgroundTimer != null)
		{
			_throttleBackgroundTimer.Stop();
			_throttleBackgroundTimer.Tick -= OnThrottleBackgroundTimeout;
			_throttleBackgroundTimer = null;
		}
	}

	private double GetContentOffset(double lineWidth, TextAlignment aligment)
	{
		double wrappingWidth = GetWrappingWidth(base.RenderSize.Width);
		return aligment switch
		{
			TextAlignment.Right => wrappingWidth - lineWidth, 
			TextAlignment.Center => (wrappingWidth - lineWidth) / 2.0, 
			_ => 0.0, 
		};
	}

	private TextAlignment HorizontalAlignmentToTextAlignment(HorizontalAlignment horizontalAlignment)
	{
		return horizontalAlignment switch
		{
			HorizontalAlignment.Right => TextAlignment.Right, 
			HorizontalAlignment.Center => TextAlignment.Center, 
			HorizontalAlignment.Stretch => TextAlignment.Justify, 
			_ => TextAlignment.Left, 
		};
	}

	private bool Contains(ITextPointer position)
	{
		Invariant.Assert(IsLayoutValid);
		if (position.TextContainer == _host.TextContainer && _lineMetrics != null)
		{
			return _lineMetrics[_lineMetrics.Count - 1].EndOffset >= position.Offset;
		}
		return false;
	}

	private double GetWrappingWidth(double width)
	{
		if (width < _contentSize.Width)
		{
			width = _contentSize.Width;
		}
		if (width > _previousConstraint.Width)
		{
			width = _previousConstraint.Width;
		}
		TextDpi.EnsureValidLineWidth(ref width);
		return width;
	}

	private double GetTextAlignmentCorrection(TextAlignment textAlignment, double width)
	{
		double result = 0.0;
		if (textAlignment != 0 && _contentSize.Width > width)
		{
			result = 0.0 - GetContentOffset(_contentSize.Width, textAlignment);
		}
		return result;
	}

	private DirtyTextRange? GetSelectionRenderRange(DirtyTextRange selectionRange)
	{
		DirtyTextRange? result = null;
		GetVisibleLines(out var firstLineIndex, out var lastLineIndex);
		int startIndex = selectionRange.StartIndex;
		int num = selectionRange.StartIndex + selectionRange.PositionsAdded;
		int offset = _lineMetrics[firstLineIndex].Offset;
		int endOffset = _lineMetrics[lastLineIndex].EndOffset;
		if (endOffset >= startIndex && offset <= num)
		{
			int num2 = Math.Max(offset, startIndex);
			int num3 = Math.Min(endOffset, num) - num2;
			result = new DirtyTextRange(num2, num3, num3, fromHighlightLayer: true);
		}
		return result;
	}
}
