using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.PtsHost;
using MS.Internal.Text;

namespace MS.Internal.Documents;

internal class TextDocumentView : TextViewBase
{
	private readonly FlowDocumentPage _owner;

	private readonly ITextContainer _textContainer;

	private ReadOnlyCollection<ColumnResult> _columns;

	private ReadOnlyCollection<ParagraphResult> _floatingElements;

	private static ReadOnlyCollection<ParagraphResult> _emptyParagraphCollection = new ReadOnlyCollection<ParagraphResult>(new List<ParagraphResult>(0));

	private ReadOnlyCollection<TextSegment> _segments;

	private bool _hasTextContent;

	internal override UIElement RenderScope
	{
		get
		{
			UIElement result = null;
			if (!_owner.IsDisposed)
			{
				Visual visual = _owner.Visual;
				while (visual != null && !(visual is UIElement))
				{
					visual = VisualTreeHelper.GetParent(visual) as Visual;
				}
				result = visual as UIElement;
			}
			return result;
		}
	}

	internal override ITextContainer TextContainer => _textContainer;

	internal override bool IsValid => _owner.IsLayoutDataValid;

	internal override ReadOnlyCollection<TextSegment> TextSegments
	{
		get
		{
			if (!IsValid)
			{
				return new ReadOnlyCollection<TextSegment>(new List<TextSegment>());
			}
			return TextSegmentsCore;
		}
	}

	private ReadOnlyCollection<TextSegment> TextSegmentsCore
	{
		get
		{
			if (_segments == null)
			{
				_segments = GetTextSegments();
				Invariant.Assert(_segments != null, "TextSegment collection is empty.");
			}
			return _segments;
		}
	}

	internal ReadOnlyCollection<ColumnResult> Columns
	{
		get
		{
			Invariant.Assert(IsValid, "TextView is not updated.");
			if (_columns == null)
			{
				_columns = _owner.GetColumnResults(out _hasTextContent);
				Invariant.Assert(_columns != null, "Column collection is null.");
			}
			return _columns;
		}
	}

	internal ReadOnlyCollection<ParagraphResult> FloatingElements
	{
		get
		{
			Invariant.Assert(IsValid, "TextView is not updated.");
			if (_floatingElements == null)
			{
				_floatingElements = _owner.FloatingElementResults;
				Invariant.Assert(_floatingElements != null, "Floating elements collection is null.");
			}
			return _floatingElements;
		}
	}

	internal TextDocumentView(FlowDocumentPage owner, ITextContainer textContainer)
	{
		_owner = owner;
		_textContainer = textContainer;
	}

	internal override ITextPointer GetTextPositionFromPoint(Point point, bool snapToText)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		_owner.EnsureValidVisuals();
		TransformToContent(ref point);
		return GetTextPositionFromPoint(Columns, FloatingElements, point, snapToText);
	}

	internal override Rect GetRawRectangleFromTextPosition(ITextPointer position, out Transform transform)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		if (!ContainsCore(position))
		{
			throw new ArgumentOutOfRangeException("position");
		}
		_owner.EnsureValidVisuals();
		Rect rect = GetRectangleFromTextPosition(Columns, FloatingElements, position);
		TransformFromContent(ref rect, out transform);
		return rect;
	}

	internal override Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition)
	{
		Geometry geometry = null;
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		ValidationHelper.VerifyPosition(_textContainer, startPosition, "startPosition");
		ValidationHelper.VerifyPosition(_textContainer, endPosition, "endPosition");
		_owner.EnsureValidVisuals();
		Rect rect = CalculateViewportRect();
		bool success = false;
		if (FloatingElements.Count > 0)
		{
			Geometry tightBoundingGeometryFromTextPositionsInFloatingElements = GetTightBoundingGeometryFromTextPositionsInFloatingElements(FloatingElements, startPosition, endPosition, 0.0, rect, out success);
			CaretElement.AddGeometry(ref geometry, tightBoundingGeometryFromTextPositionsInFloatingElements);
			if (geometry != null)
			{
				TransformFromContent(geometry);
			}
		}
		if (!success)
		{
			Invariant.Assert(geometry == null);
			ReadOnlyCollection<TextSegment> textSegments = TextSegments;
			for (int i = 0; i < textSegments.Count; i++)
			{
				TextSegment textSegment = textSegments[i];
				ITextPointer textPointer = ((startPosition.CompareTo(textSegment.Start) > 0) ? startPosition : textSegment.Start);
				ITextPointer textPointer2 = ((endPosition.CompareTo(textSegment.End) < 0) ? endPosition : textSegment.End);
				if (textPointer.CompareTo(textPointer2) >= 0)
				{
					continue;
				}
				ReadOnlyCollection<ColumnResult> columns = Columns;
				for (int j = 0; j < columns.Count; j++)
				{
					Rect layoutBox = columns[j].LayoutBox;
					layoutBox.X = rect.X;
					if (layoutBox.IntersectsWith(rect))
					{
						Geometry tightBoundingGeometryFromTextPositionsHelper = GetTightBoundingGeometryFromTextPositionsHelper(columns[j].Paragraphs, textPointer, textPointer2, 0.0, rect);
						CaretElement.AddGeometry(ref geometry, tightBoundingGeometryFromTextPositionsHelper);
					}
				}
				if (geometry != null)
				{
					TransformFromContent(geometry);
				}
			}
		}
		return geometry;
	}

	private Rect CalculateViewportRect()
	{
		Rect rect = Rect.Empty;
		if (RenderScope is IScrollInfo)
		{
			IScrollInfo scrollInfo = (IScrollInfo)RenderScope;
			if (scrollInfo.ViewportWidth != 0.0 && scrollInfo.ViewportHeight != 0.0)
			{
				rect = new Rect(scrollInfo.HorizontalOffset, scrollInfo.VerticalOffset, scrollInfo.ViewportWidth, scrollInfo.ViewportHeight);
			}
		}
		if (rect.IsEmpty)
		{
			rect = _owner.Viewport;
		}
		TransformToContent(ref rect);
		return rect;
	}

	internal override ITextPointer GetPositionAtNextLine(ITextPointer position, double suggestedX, int count, out double newSuggestedX, out int linesMoved)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		if (!ContainsCore(position))
		{
			throw new ArgumentOutOfRangeException("position");
		}
		_owner.EnsureValidVisuals();
		Point point = new Point(suggestedX, 0.0);
		TransformToContent(ref point);
		suggestedX = (newSuggestedX = point.X);
		linesMoved = count;
		if (count == 0)
		{
			return position;
		}
		bool positionFound;
		ITextPointer textPointer = GetPositionAtNextLine(Columns, FloatingElements, position, suggestedX, ref count, out newSuggestedX, out positionFound);
		linesMoved -= count;
		point = new Point(newSuggestedX, 0.0);
		TransformFromContent(ref point);
		newSuggestedX = point.X;
		if (textPointer == null || !ContainsCore(textPointer))
		{
			textPointer = position;
			linesMoved = 0;
		}
		return textPointer;
	}

	internal override bool IsAtCaretUnitBoundary(ITextPointer position)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		if (!ContainsCore(position))
		{
			throw new ArgumentOutOfRangeException("position");
		}
		return IsAtCaretUnitBoundary(Columns, FloatingElements, position);
	}

	internal override ITextPointer GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		ValidationHelper.VerifyDirection(direction, "direction");
		if (!ContainsCore(position))
		{
			throw new ArgumentOutOfRangeException("position");
		}
		return GetNextCaretUnitPosition(Columns, FloatingElements, position, direction);
	}

	internal override ITextPointer GetBackspaceCaretUnitPosition(ITextPointer position)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		if (!ContainsCore(position))
		{
			throw new ArgumentOutOfRangeException("position");
		}
		return GetBackspaceCaretUnitPosition(Columns, FloatingElements, position);
	}

	internal override TextSegment GetLineRange(ITextPointer position)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		if (!ContainsCore(position))
		{
			throw new ArgumentOutOfRangeException("position");
		}
		return GetLineRangeFromPosition(Columns, FloatingElements, position);
	}

	internal override ReadOnlyCollection<GlyphRun> GetGlyphRuns(ITextPointer start, ITextPointer end)
	{
		List<GlyphRun> list = new List<GlyphRun>();
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		ValidationHelper.VerifyPosition(_textContainer, start, "start");
		ValidationHelper.VerifyPosition(_textContainer, end, "end");
		ValidationHelper.VerifyPositionPair(start, end);
		if (!ContainsCore(start))
		{
			throw new ArgumentOutOfRangeException("start");
		}
		if (!ContainsCore(end))
		{
			throw new ArgumentOutOfRangeException("end");
		}
		GetGlyphRuns(list, start, end, Columns, FloatingElements);
		return new ReadOnlyCollection<GlyphRun>(list);
	}

	internal override bool Contains(ITextPointer position)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		return ContainsCore(position);
	}

	internal override bool Validate()
	{
		return IsValid;
	}

	internal override void ThrottleBackgroundTasksForUserInput()
	{
		_owner.StructuralCache.ThrottleBackgroundFormatting();
	}

	internal CellInfo GetCellInfoFromPoint(Point point, Table tableFilter)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		return GetCellInfoFromPoint(Columns, FloatingElements, point, tableFilter);
	}

	internal void OnUpdated()
	{
		OnUpdated(EventArgs.Empty);
	}

	internal void Invalidate()
	{
		_columns = null;
		_segments = null;
		_floatingElements = null;
	}

	internal static bool Contains(ITextPointer position, ReadOnlyCollection<TextSegment> segments)
	{
		bool flag = false;
		Invariant.Assert(segments != null);
		foreach (TextSegment segment in segments)
		{
			if (segment.Start.CompareTo(position) < 0 && segment.End.CompareTo(position) > 0)
			{
				flag = true;
				break;
			}
			if (segment.Start.CompareTo(position) == 0)
			{
				if (position.LogicalDirection == LogicalDirection.Forward)
				{
					flag = true;
					break;
				}
				if (segment.Start.LogicalDirection == LogicalDirection.Backward)
				{
					flag = true;
					break;
				}
			}
			if (segment.End.CompareTo(position) == 0)
			{
				if (position.LogicalDirection == LogicalDirection.Backward)
				{
					flag = true;
					break;
				}
				if (segment.End.LogicalDirection == LogicalDirection.Forward)
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag && segments.Count > 0)
		{
			if (position.TextContainer.Start.CompareTo(position) == 0 && position.LogicalDirection == LogicalDirection.Backward)
			{
				flag = position.TextContainer.Start.CompareTo(segments[0].Start) == 0;
			}
			else if (position.TextContainer.End.CompareTo(position) == 0 && position.LogicalDirection == LogicalDirection.Forward)
			{
				flag = position.TextContainer.End.CompareTo(segments[segments.Count - 1].End) == 0;
			}
		}
		return flag;
	}

	private ITextPointer GetTextPositionFromPoint(ReadOnlyCollection<ParagraphResult> paragraphs, ReadOnlyCollection<ParagraphResult> floatingElements, Point point, bool snapToText, bool snapToTextInFloatingElements)
	{
		Invariant.Assert(paragraphs != null, "Paragraph collection is empty.");
		Invariant.Assert(floatingElements != null, "Floating element collection is empty.");
		int paragraphFromPointInFloatingElements = GetParagraphFromPointInFloatingElements(floatingElements, point, snapToTextInFloatingElements);
		ParagraphResult paragraph;
		if (paragraphFromPointInFloatingElements < 0)
		{
			Invariant.Assert(!snapToTextInFloatingElements || floatingElements.Count == 0, "When snap to text is enabled a valid text position is required if paragraphs exist.");
			if (snapToTextInFloatingElements)
			{
				return null;
			}
			paragraphFromPointInFloatingElements = GetParagraphFromPoint(paragraphs, point, snapToText);
			if (paragraphFromPointInFloatingElements < 0)
			{
				Invariant.Assert(!snapToText || paragraphs.Count == 0, "When snap to text is enabled a valid text position is required if paragraphs exist.");
				return null;
			}
			Invariant.Assert(paragraphFromPointInFloatingElements < paragraphs.Count);
			paragraph = paragraphs[paragraphFromPointInFloatingElements];
		}
		else
		{
			Invariant.Assert(paragraphFromPointInFloatingElements < floatingElements.Count);
			paragraph = floatingElements[paragraphFromPointInFloatingElements];
		}
		return GetTextPositionFromPoint(paragraph, point, snapToText);
	}

	private ITextPointer GetTextPositionFromPoint(ParagraphResult paragraph, Point point, bool snapToText)
	{
		ITextPointer result = null;
		Rect layoutBox = paragraph.LayoutBox;
		if (paragraph is ContainerParagraphResult)
		{
			ReadOnlyCollection<ParagraphResult> paragraphs = ((ContainerParagraphResult)paragraph).Paragraphs;
			Invariant.Assert(paragraphs != null, "Paragraph collection is null.");
			result = ((paragraphs.Count > 0) ? GetTextPositionFromPoint(paragraphs, _emptyParagraphCollection, point, snapToText, snapToTextInFloatingElements: false) : ((!(point.X <= layoutBox.Width)) ? paragraph.EndPosition.CreatePointer(LogicalDirection.Backward) : paragraph.StartPosition.CreatePointer(LogicalDirection.Forward)));
		}
		else if (paragraph is TextParagraphResult)
		{
			ReadOnlyCollection<LineResult> lines = ((TextParagraphResult)paragraph).Lines;
			Invariant.Assert(lines != null, "Lines collection is null");
			result = (((TextParagraphResult)paragraph).HasTextContent ? TextParagraphView.GetTextPositionFromPoint(lines, point, snapToText) : null);
		}
		else if (paragraph is TableParagraphResult)
		{
			ReadOnlyCollection<ParagraphResult> paragraphs2 = ((TableParagraphResult)paragraph).Paragraphs;
			Invariant.Assert(paragraphs2 != null, "Paragraph collection is null.");
			int paragraphFromPoint = GetParagraphFromPoint(paragraphs2, point, snapToText);
			if (paragraphFromPoint != -1)
			{
				ParagraphResult paragraphResult = paragraphs2[paragraphFromPoint];
				if (point.X > paragraphResult.LayoutBox.Right)
				{
					result = ((TextElement)paragraphResult.Element).ElementEnd;
				}
				else
				{
					ReadOnlyCollection<ParagraphResult> paragraphsFromPoint = ((TableParagraphResult)paragraph).GetParagraphsFromPoint(point, snapToText);
					result = GetTextPositionFromPoint(paragraphsFromPoint, _emptyParagraphCollection, point, snapToText, snapToTextInFloatingElements: false);
				}
			}
			else
			{
				result = null;
				if (snapToText)
				{
					result = ((TextElement)paragraph.Element).ContentStart;
				}
			}
		}
		else if (paragraph is SubpageParagraphResult)
		{
			SubpageParagraphResult subpageParagraphResult = (SubpageParagraphResult)paragraph;
			point.X -= subpageParagraphResult.ContentOffset.X;
			point.Y -= subpageParagraphResult.ContentOffset.Y;
			result = GetTextPositionFromPoint(subpageParagraphResult.Columns, subpageParagraphResult.FloatingElements, point, snapToText);
		}
		else if (paragraph is FigureParagraphResult || paragraph is FloaterParagraphResult)
		{
			ReadOnlyCollection<ColumnResult> columns;
			ReadOnlyCollection<ParagraphResult> floatingElements;
			if (paragraph is FloaterParagraphResult)
			{
				FloaterParagraphResult floaterParagraphResult = (FloaterParagraphResult)paragraph;
				columns = floaterParagraphResult.Columns;
				floatingElements = floaterParagraphResult.FloatingElements;
				TransformToSubpage(ref point, floaterParagraphResult.ContentOffset);
			}
			else
			{
				FigureParagraphResult figureParagraphResult = (FigureParagraphResult)paragraph;
				columns = figureParagraphResult.Columns;
				floatingElements = figureParagraphResult.FloatingElements;
				TransformToSubpage(ref point, figureParagraphResult.ContentOffset);
			}
			Invariant.Assert(columns != null, "Columns collection is null.");
			Invariant.Assert(floatingElements != null, "Floating elements collection is null.");
			result = ((columns.Count <= 0 && floatingElements.Count <= 0) ? null : GetTextPositionFromPoint(columns, floatingElements, point, snapToText));
		}
		else if (!(paragraph is UIElementParagraphResult))
		{
			result = ((!(point.X <= layoutBox.Width)) ? paragraph.EndPosition.CreatePointer(LogicalDirection.Backward) : paragraph.StartPosition.CreatePointer(LogicalDirection.Forward));
		}
		else if (paragraph.Element is BlockUIContainer blockUIContainer)
		{
			result = null;
			if (layoutBox.Contains(point) || snapToText)
			{
				result = ((!DoubleUtil.LessThanOrClose(point.X, layoutBox.X + layoutBox.Width / 2.0)) ? blockUIContainer.ContentEnd.CreatePointer(LogicalDirection.Backward) : blockUIContainer.ContentStart.CreatePointer(LogicalDirection.Forward));
			}
		}
		return result;
	}

	private ITextPointer GetTextPositionFromPoint(ReadOnlyCollection<ColumnResult> columns, ReadOnlyCollection<ParagraphResult> floatingElements, Point point, bool snapToText)
	{
		ITextPointer textPointer = null;
		Invariant.Assert(floatingElements != null);
		int columnFromPoint = GetColumnFromPoint(columns, point, snapToText);
		if (columnFromPoint < 0 && floatingElements.Count == 0)
		{
			textPointer = null;
		}
		else
		{
			bool snapToTextInFloatingElements = false;
			ReadOnlyCollection<ParagraphResult> paragraphs;
			if (columnFromPoint < columns.Count && columnFromPoint >= 0)
			{
				ColumnResult columnResult = columns[columnFromPoint];
				if (!columnResult.HasTextContent)
				{
					snapToTextInFloatingElements = true;
				}
				paragraphs = columnResult.Paragraphs;
			}
			else
			{
				paragraphs = _emptyParagraphCollection;
			}
			textPointer = GetTextPositionFromPoint(paragraphs, floatingElements, point, snapToText, snapToTextInFloatingElements);
		}
		if (textPointer != null && !ContainsCore(textPointer))
		{
			textPointer = null;
		}
		return textPointer;
	}

	private CellInfo GetCellInfoFromPoint(ReadOnlyCollection<ParagraphResult> paragraphs, ReadOnlyCollection<ParagraphResult> floatingElements, Point point, Table tableFilter)
	{
		CellInfo result = null;
		Invariant.Assert(paragraphs != null, "Paragraph collection is empty.");
		Invariant.Assert(floatingElements != null, "Floating element collection is empty.");
		int paragraphFromPointInFloatingElements = GetParagraphFromPointInFloatingElements(floatingElements, point, snapToText: false);
		ParagraphResult paragraphResult = null;
		if (paragraphFromPointInFloatingElements >= 0)
		{
			Invariant.Assert(paragraphFromPointInFloatingElements < floatingElements.Count);
			paragraphResult = floatingElements[paragraphFromPointInFloatingElements];
		}
		else
		{
			paragraphFromPointInFloatingElements = GetParagraphFromPoint(paragraphs, point, snapToText: false);
			if (paragraphFromPointInFloatingElements >= 0)
			{
				Invariant.Assert(paragraphFromPointInFloatingElements < paragraphs.Count);
				paragraphResult = paragraphs[paragraphFromPointInFloatingElements];
			}
		}
		if (paragraphResult != null)
		{
			result = GetCellInfoFromPoint(paragraphResult, point, tableFilter);
		}
		return result;
	}

	private CellInfo GetCellInfoFromPoint(ParagraphResult paragraph, Point point, Table tableFilter)
	{
		CellInfo cellInfo = null;
		if (paragraph is ContainerParagraphResult)
		{
			ReadOnlyCollection<ParagraphResult> paragraphs = ((ContainerParagraphResult)paragraph).Paragraphs;
			Invariant.Assert(paragraphs != null, "Paragraph collection is null");
			if (paragraphs.Count > 0)
			{
				cellInfo = GetCellInfoFromPoint(paragraphs, _emptyParagraphCollection, point, tableFilter);
			}
		}
		else if (paragraph is TableParagraphResult)
		{
			ReadOnlyCollection<ParagraphResult> paragraphsFromPoint = ((TableParagraphResult)paragraph).GetParagraphsFromPoint(point, snapToText: false);
			Invariant.Assert(paragraphsFromPoint != null, "Paragraph collection is null");
			if (paragraphsFromPoint.Count > 0)
			{
				cellInfo = GetCellInfoFromPoint(paragraphsFromPoint, _emptyParagraphCollection, point, tableFilter);
			}
			if (cellInfo == null)
			{
				cellInfo = ((TableParagraphResult)paragraph).GetCellInfoFromPoint(point);
			}
		}
		else if (paragraph is SubpageParagraphResult)
		{
			SubpageParagraphResult subpageParagraphResult = (SubpageParagraphResult)paragraph;
			point.X -= subpageParagraphResult.ContentOffset.X;
			point.Y -= subpageParagraphResult.ContentOffset.Y;
			cellInfo = GetCellInfoFromPoint(subpageParagraphResult.Columns, subpageParagraphResult.FloatingElements, point, tableFilter);
			cellInfo?.Adjust(new Point(subpageParagraphResult.ContentOffset.X, subpageParagraphResult.ContentOffset.Y));
		}
		else if (paragraph is FigureParagraphResult)
		{
			FigureParagraphResult figureParagraphResult = (FigureParagraphResult)paragraph;
			TransformToSubpage(ref point, figureParagraphResult.ContentOffset);
			cellInfo = GetCellInfoFromPoint(figureParagraphResult.Columns, figureParagraphResult.FloatingElements, point, tableFilter);
			cellInfo?.Adjust(new Point(figureParagraphResult.ContentOffset.X, figureParagraphResult.ContentOffset.Y));
		}
		else if (paragraph is FloaterParagraphResult)
		{
			FloaterParagraphResult floaterParagraphResult = (FloaterParagraphResult)paragraph;
			TransformToSubpage(ref point, floaterParagraphResult.ContentOffset);
			cellInfo = GetCellInfoFromPoint(floaterParagraphResult.Columns, floaterParagraphResult.FloatingElements, point, tableFilter);
			cellInfo?.Adjust(new Point(floaterParagraphResult.ContentOffset.X, floaterParagraphResult.ContentOffset.Y));
		}
		if (tableFilter != null && cellInfo != null && cellInfo.Cell.Table != tableFilter)
		{
			cellInfo = null;
		}
		return cellInfo;
	}

	private CellInfo GetCellInfoFromPoint(ReadOnlyCollection<ColumnResult> columns, ReadOnlyCollection<ParagraphResult> floatingElements, Point point, Table tableFilter)
	{
		Invariant.Assert(floatingElements != null);
		int columnFromPoint = GetColumnFromPoint(columns, point, snapToText: false);
		if (columnFromPoint < 0 && floatingElements.Count == 0)
		{
			return null;
		}
		ReadOnlyCollection<ParagraphResult> paragraphs = ((columnFromPoint < columns.Count && columnFromPoint >= 0) ? columns[columnFromPoint].Paragraphs : _emptyParagraphCollection);
		return GetCellInfoFromPoint(paragraphs, floatingElements, point, tableFilter);
	}

	private Rect GetRectangleFromTextPosition(ReadOnlyCollection<ParagraphResult> paragraphs, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer position)
	{
		Invariant.Assert(paragraphs != null, "Paragraph collection is null");
		Invariant.Assert(floatingElements != null, "Floating element collection is null");
		Rect result = Rect.Empty;
		bool isFloatingPara = false;
		int paragraphFromPosition = GetParagraphFromPosition(paragraphs, floatingElements, position, out isFloatingPara);
		ParagraphResult paragraphResult = null;
		if (isFloatingPara)
		{
			Invariant.Assert(paragraphFromPosition < floatingElements.Count);
			paragraphResult = floatingElements[paragraphFromPosition];
		}
		else if (paragraphFromPosition < paragraphs.Count)
		{
			paragraphResult = paragraphs[paragraphFromPosition];
		}
		if (paragraphResult != null)
		{
			result = GetRectangleFromTextPosition(paragraphResult, position);
		}
		return result;
	}

	private Rect GetRectangleFromTextPosition(ParagraphResult paragraph, ITextPointer position)
	{
		Rect rect = Rect.Empty;
		if (paragraph is ContainerParagraphResult)
		{
			rect = GetRectangleFromEdge(paragraph, position);
			if (rect == Rect.Empty)
			{
				ReadOnlyCollection<ParagraphResult> paragraphs = ((ContainerParagraphResult)paragraph).Paragraphs;
				Invariant.Assert(paragraphs != null, "Paragraph collection is null.");
				if (paragraphs.Count > 0)
				{
					rect = GetRectangleFromTextPosition(paragraphs, _emptyParagraphCollection, position);
				}
			}
		}
		else if (paragraph is TextParagraphResult)
		{
			rect = ((TextParagraphResult)paragraph).GetRectangleFromTextPosition(position);
		}
		else if (paragraph is TableParagraphResult)
		{
			rect = GetRectangleFromEdge(paragraph, position);
			if (rect == Rect.Empty)
			{
				ReadOnlyCollection<ParagraphResult> paragraphsFromPosition = ((TableParagraphResult)paragraph).GetParagraphsFromPosition(position);
				Invariant.Assert(paragraphsFromPosition != null, "Paragraph collection is null.");
				if (paragraphsFromPosition.Count > 0)
				{
					rect = GetRectangleFromTextPosition(paragraphsFromPosition, _emptyParagraphCollection, position);
				}
				else if (position is TextPointer && ((TextPointer)position).IsAtRowEnd)
				{
					rect = ((TableParagraphResult)paragraph).GetRectangleFromRowEndPosition(position);
				}
			}
		}
		else if (paragraph is SubpageParagraphResult)
		{
			SubpageParagraphResult subpageParagraphResult = (SubpageParagraphResult)paragraph;
			rect = GetRectangleFromTextPosition(subpageParagraphResult.Columns, subpageParagraphResult.FloatingElements, position);
			if (rect != Rect.Empty)
			{
				rect.X += subpageParagraphResult.ContentOffset.X;
				rect.Y += subpageParagraphResult.ContentOffset.Y;
			}
		}
		else if (paragraph is FloaterParagraphResult)
		{
			FloaterParagraphResult floaterParagraphResult = (FloaterParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns = floaterParagraphResult.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements = floaterParagraphResult.FloatingElements;
			Invariant.Assert(columns != null, "Columns collection is null.");
			Invariant.Assert(floatingElements != null, "Paragraph collection is null.");
			if (floatingElements.Count > 0 || columns.Count > 0)
			{
				rect = GetRectangleFromTextPosition(columns, floatingElements, position);
				TransformFromSubpage(ref rect, floaterParagraphResult.ContentOffset);
			}
		}
		else if (paragraph is FigureParagraphResult)
		{
			FigureParagraphResult figureParagraphResult = (FigureParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns2 = figureParagraphResult.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements2 = figureParagraphResult.FloatingElements;
			Invariant.Assert(columns2 != null, "Columns collection is null.");
			Invariant.Assert(floatingElements2 != null, "Paragraph collection is null.");
			if (floatingElements2.Count > 0 || columns2.Count > 0)
			{
				rect = GetRectangleFromTextPosition(columns2, floatingElements2, position);
				TransformFromSubpage(ref rect, figureParagraphResult.ContentOffset);
			}
		}
		else if (paragraph is UIElementParagraphResult)
		{
			rect = GetRectangleFromEdge(paragraph, position);
			if (rect == Rect.Empty)
			{
				rect = GetRectangleFromContentEdge(paragraph, position);
			}
		}
		return rect;
	}

	private Rect GetRectangleFromTextPosition(ReadOnlyCollection<ColumnResult> columns, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer position)
	{
		Rect result = Rect.Empty;
		Invariant.Assert(floatingElements != null);
		int columnFromPosition = GetColumnFromPosition(columns, position);
		if (columnFromPosition < columns.Count || floatingElements.Count > 0)
		{
			ReadOnlyCollection<ParagraphResult> paragraphs = ((columnFromPosition < columns.Count && columnFromPosition >= 0) ? columns[columnFromPosition].Paragraphs : _emptyParagraphCollection);
			result = GetRectangleFromTextPosition(paragraphs, floatingElements, position);
		}
		return result;
	}

	internal static Geometry GetTightBoundingGeometryFromTextPositionsHelper(ReadOnlyCollection<ParagraphResult> paragraphs, ITextPointer startPosition, ITextPointer endPosition, double paragraphTopSpace, Rect visibleRect)
	{
		Geometry geometry = null;
		int count = paragraphs.Count;
		for (int i = 0; i < count && endPosition.CompareTo(paragraphs[i].StartPosition) > 0; i++)
		{
			if (startPosition.CompareTo(paragraphs[i].EndPosition) > 0)
			{
				continue;
			}
			Rect layoutBox = GetLayoutBox(paragraphs[i]);
			layoutBox.X = visibleRect.X;
			if (layoutBox.IntersectsWith(visibleRect))
			{
				Geometry addedGeometry = null;
				if (paragraphs[i] is ContainerParagraphResult)
				{
					addedGeometry = ((ContainerParagraphResult)paragraphs[i]).GetTightBoundingGeometryFromTextPositions(startPosition, endPosition, visibleRect);
				}
				else if (paragraphs[i] is TextParagraphResult)
				{
					addedGeometry = ((TextParagraphResult)paragraphs[i]).GetTightBoundingGeometryFromTextPositions(startPosition, endPosition, paragraphTopSpace, visibleRect);
				}
				else if (paragraphs[i] is TableParagraphResult)
				{
					addedGeometry = ((TableParagraphResult)paragraphs[i]).GetTightBoundingGeometryFromTextPositions(startPosition, endPosition, visibleRect);
				}
				else if (paragraphs[i] is UIElementParagraphResult)
				{
					addedGeometry = ((UIElementParagraphResult)paragraphs[i]).GetTightBoundingGeometryFromTextPositions(startPosition, endPosition);
				}
				CaretElement.AddGeometry(ref geometry, addedGeometry);
			}
		}
		return geometry;
	}

	internal static Geometry GetTightBoundingGeometryFromTextPositionsHelper(ReadOnlyCollection<ParagraphResult> paragraphs, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer startPosition, ITextPointer endPosition, double paragraphTopSpace, Rect visibleRect)
	{
		Geometry result = null;
		bool success = false;
		if (floatingElements != null && floatingElements.Count > 0)
		{
			result = GetTightBoundingGeometryFromTextPositionsInFloatingElements(floatingElements, startPosition, endPosition, paragraphTopSpace, visibleRect, out success);
		}
		if (!success)
		{
			result = GetTightBoundingGeometryFromTextPositionsHelper(paragraphs, startPosition, endPosition, paragraphTopSpace, visibleRect);
		}
		return result;
	}

	private static Geometry GetTightBoundingGeometryFromTextPositionsInFloatingElements(ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer startPosition, ITextPointer endPosition, double paragraphTopSpace, Rect visibleRect, out bool success)
	{
		Geometry geometry = null;
		success = false;
		int count = floatingElements.Count;
		for (int i = 0; i < count; i++)
		{
			if (startPosition.CompareTo(floatingElements[i].StartPosition) <= 0 || endPosition.CompareTo(floatingElements[i].EndPosition) >= 0)
			{
				continue;
			}
			Rect layoutBox = GetLayoutBox(floatingElements[i]);
			Rect rect = visibleRect;
			layoutBox.X = rect.X;
			if (layoutBox.IntersectsWith(rect))
			{
				Geometry geometry2 = null;
				Invariant.Assert(floatingElements[i] is FloaterParagraphResult || floatingElements[i] is FigureParagraphResult);
				if (floatingElements[i] is FloaterParagraphResult)
				{
					FloaterParagraphResult floaterParagraphResult = (FloaterParagraphResult)floatingElements[i];
					TransformToSubpage(ref rect, floaterParagraphResult.ContentOffset);
					geometry2 = floaterParagraphResult.GetTightBoundingGeometryFromTextPositions(startPosition, endPosition, rect, out success);
					TransformFromSubpage(geometry2, floaterParagraphResult.ContentOffset);
				}
				else if (floatingElements[i] is FigureParagraphResult)
				{
					FigureParagraphResult figureParagraphResult = (FigureParagraphResult)floatingElements[i];
					TransformToSubpage(ref rect, figureParagraphResult.ContentOffset);
					geometry2 = figureParagraphResult.GetTightBoundingGeometryFromTextPositions(startPosition, endPosition, rect, out success);
					TransformFromSubpage(geometry2, figureParagraphResult.ContentOffset);
				}
				CaretElement.AddGeometry(ref geometry, geometry2);
				if (success)
				{
					break;
				}
			}
		}
		return geometry;
	}

	private static Rect GetLayoutBox(ParagraphResult paragraph)
	{
		if (!(paragraph is SubpageParagraphResult) && !(paragraph is RowParagraphResult))
		{
			return paragraph.LayoutBox;
		}
		return Rect.Empty;
	}

	private bool IsAtCaretUnitBoundary(ReadOnlyCollection<ParagraphResult> paragraphs, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer position)
	{
		Invariant.Assert(paragraphs != null, "Paragraph collection is null");
		Invariant.Assert(floatingElements != null, "Floating element collection is null");
		bool result = false;
		bool isFloatingPara;
		int paragraphFromPosition = GetParagraphFromPosition(paragraphs, floatingElements, position, out isFloatingPara);
		ParagraphResult paragraphResult = null;
		if (isFloatingPara)
		{
			Invariant.Assert(paragraphFromPosition < floatingElements.Count);
			paragraphResult = floatingElements[paragraphFromPosition];
		}
		else if (paragraphFromPosition < paragraphs.Count)
		{
			paragraphResult = paragraphs[paragraphFromPosition];
		}
		if (paragraphResult != null)
		{
			result = IsAtCaretUnitBoundary(paragraphResult, position);
		}
		return result;
	}

	private bool IsAtCaretUnitBoundary(ParagraphResult paragraph, ITextPointer position)
	{
		bool result = false;
		if (paragraph is ContainerParagraphResult)
		{
			ReadOnlyCollection<ParagraphResult> paragraphs = ((ContainerParagraphResult)paragraph).Paragraphs;
			Invariant.Assert(paragraphs != null, "Paragraph collection is null.");
			if (paragraphs.Count > 0)
			{
				result = IsAtCaretUnitBoundary(paragraphs, _emptyParagraphCollection, position);
			}
		}
		else if (paragraph is TextParagraphResult)
		{
			result = ((TextParagraphResult)paragraph).IsAtCaretUnitBoundary(position);
		}
		else if (paragraph is TableParagraphResult)
		{
			ReadOnlyCollection<ParagraphResult> paragraphsFromPosition = ((TableParagraphResult)paragraph).GetParagraphsFromPosition(position);
			Invariant.Assert(paragraphsFromPosition != null, "Paragraph collection is null.");
			if (paragraphsFromPosition.Count > 0)
			{
				result = IsAtCaretUnitBoundary(paragraphsFromPosition, _emptyParagraphCollection, position);
			}
		}
		else if (paragraph is SubpageParagraphResult)
		{
			SubpageParagraphResult obj = (SubpageParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns = obj.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements = obj.FloatingElements;
			Invariant.Assert(columns != null, "Column collection is null.");
			Invariant.Assert(floatingElements != null, "Paragraph collection is null.");
			if (columns.Count > 0 || floatingElements.Count > 0)
			{
				result = IsAtCaretUnitBoundary(columns, floatingElements, position);
			}
		}
		else if (paragraph is FigureParagraphResult)
		{
			FigureParagraphResult obj2 = (FigureParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns2 = obj2.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements2 = obj2.FloatingElements;
			Invariant.Assert(columns2 != null, "Column collection is null.");
			Invariant.Assert(floatingElements2 != null, "Paragraph collection is null.");
			if (columns2.Count > 0 || floatingElements2.Count > 0)
			{
				result = IsAtCaretUnitBoundary(columns2, floatingElements2, position);
			}
		}
		else if (paragraph is FloaterParagraphResult)
		{
			FloaterParagraphResult obj3 = (FloaterParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns3 = obj3.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements3 = obj3.FloatingElements;
			Invariant.Assert(columns3 != null, "Column collection is null.");
			Invariant.Assert(floatingElements3 != null, "Paragraph collection is null.");
			if (columns3.Count > 0 || floatingElements3.Count > 0)
			{
				result = IsAtCaretUnitBoundary(columns3, floatingElements3, position);
			}
		}
		return result;
	}

	private bool IsAtCaretUnitBoundary(ReadOnlyCollection<ColumnResult> columns, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer position)
	{
		int columnFromPosition = GetColumnFromPosition(columns, position);
		if (columnFromPosition < columns.Count || floatingElements.Count > 0)
		{
			ReadOnlyCollection<ParagraphResult> paragraphs = ((columnFromPosition < columns.Count && columnFromPosition >= 0) ? columns[columnFromPosition].Paragraphs : _emptyParagraphCollection);
			return IsAtCaretUnitBoundary(paragraphs, floatingElements, position);
		}
		return false;
	}

	private ITextPointer GetNextCaretUnitPosition(ReadOnlyCollection<ParagraphResult> paragraphs, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer position, LogicalDirection direction)
	{
		Invariant.Assert(paragraphs != null, "Paragraph collection is null");
		Invariant.Assert(floatingElements != null, "Floating element collection is null");
		ITextPointer result = position;
		bool isFloatingPara;
		int paragraphFromPosition = GetParagraphFromPosition(paragraphs, floatingElements, position, out isFloatingPara);
		ParagraphResult paragraphResult = null;
		if (isFloatingPara)
		{
			Invariant.Assert(paragraphFromPosition < floatingElements.Count);
			paragraphResult = floatingElements[paragraphFromPosition];
		}
		else if (paragraphFromPosition < paragraphs.Count)
		{
			paragraphResult = paragraphs[paragraphFromPosition];
		}
		if (paragraphResult != null)
		{
			result = GetNextCaretUnitPosition(paragraphResult, position, direction);
		}
		return result;
	}

	private ITextPointer GetNextCaretUnitPosition(ParagraphResult paragraph, ITextPointer position, LogicalDirection direction)
	{
		ITextPointer result = position;
		if (paragraph is ContainerParagraphResult)
		{
			ReadOnlyCollection<ParagraphResult> paragraphs = ((ContainerParagraphResult)paragraph).Paragraphs;
			Invariant.Assert(paragraphs != null, "Paragraph collection is null.");
			if (paragraphs.Count > 0)
			{
				result = GetNextCaretUnitPosition(paragraphs, _emptyParagraphCollection, position, direction);
			}
		}
		else if (paragraph is TextParagraphResult)
		{
			result = ((TextParagraphResult)paragraph).GetNextCaretUnitPosition(position, direction);
		}
		else if (paragraph is TableParagraphResult)
		{
			ReadOnlyCollection<ParagraphResult> paragraphsFromPosition = ((TableParagraphResult)paragraph).GetParagraphsFromPosition(position);
			Invariant.Assert(paragraphsFromPosition != null, "Paragraph collection is null.");
			if (paragraphsFromPosition.Count > 0)
			{
				result = GetNextCaretUnitPosition(paragraphsFromPosition, _emptyParagraphCollection, position, direction);
			}
		}
		else if (paragraph is SubpageParagraphResult)
		{
			SubpageParagraphResult obj = (SubpageParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns = obj.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements = obj.FloatingElements;
			Invariant.Assert(columns != null, "Column collection is null.");
			Invariant.Assert(floatingElements != null, "Paragraph collection is null.");
			if (columns.Count > 0 || floatingElements.Count > 0)
			{
				result = GetNextCaretUnitPosition(columns, floatingElements, position, direction);
			}
		}
		else if (paragraph is FigureParagraphResult)
		{
			FigureParagraphResult obj2 = (FigureParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns2 = obj2.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements2 = obj2.FloatingElements;
			Invariant.Assert(columns2 != null, "Column collection is null.");
			Invariant.Assert(floatingElements2 != null, "Paragraph collection is null.");
			if (columns2.Count > 0 || floatingElements2.Count > 0)
			{
				result = GetNextCaretUnitPosition(columns2, floatingElements2, position, direction);
			}
		}
		else if (paragraph is FloaterParagraphResult)
		{
			FloaterParagraphResult obj3 = (FloaterParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns3 = obj3.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements3 = obj3.FloatingElements;
			Invariant.Assert(columns3 != null, "Column collection is null.");
			Invariant.Assert(floatingElements3 != null, "Paragraph collection is null.");
			if (columns3.Count > 0 || floatingElements3.Count > 0)
			{
				result = GetNextCaretUnitPosition(columns3, floatingElements3, position, direction);
			}
		}
		return result;
	}

	private ITextPointer GetNextCaretUnitPosition(ReadOnlyCollection<ColumnResult> columns, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer position, LogicalDirection direction)
	{
		int columnFromPosition = GetColumnFromPosition(columns, position);
		if (columnFromPosition < columns.Count || floatingElements.Count > 0)
		{
			ReadOnlyCollection<ParagraphResult> paragraphs = ((columnFromPosition < columns.Count && columnFromPosition >= 0) ? columns[columnFromPosition].Paragraphs : _emptyParagraphCollection);
			return GetNextCaretUnitPosition(paragraphs, floatingElements, position, direction);
		}
		return position;
	}

	private ITextPointer GetBackspaceCaretUnitPosition(ReadOnlyCollection<ParagraphResult> paragraphs, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer position)
	{
		Invariant.Assert(paragraphs != null, "Paragraph collection is null");
		Invariant.Assert(floatingElements != null, "Floating element collection is null");
		ITextPointer result = position;
		bool isFloatingPara;
		int paragraphFromPosition = GetParagraphFromPosition(paragraphs, floatingElements, position, out isFloatingPara);
		ParagraphResult paragraphResult = null;
		if (isFloatingPara)
		{
			Invariant.Assert(paragraphFromPosition < floatingElements.Count);
			paragraphResult = floatingElements[paragraphFromPosition];
		}
		else if (paragraphFromPosition < paragraphs.Count)
		{
			paragraphResult = paragraphs[paragraphFromPosition];
		}
		if (paragraphResult != null)
		{
			result = GetBackspaceCaretUnitPosition(paragraphResult, position);
		}
		return result;
	}

	private ITextPointer GetBackspaceCaretUnitPosition(ParagraphResult paragraph, ITextPointer position)
	{
		ITextPointer result = position;
		if (paragraph is ContainerParagraphResult)
		{
			ReadOnlyCollection<ParagraphResult> paragraphs = ((ContainerParagraphResult)paragraph).Paragraphs;
			Invariant.Assert(paragraphs != null, "Paragraph collection is null.");
			if (paragraphs.Count > 0)
			{
				result = GetBackspaceCaretUnitPosition(paragraphs, _emptyParagraphCollection, position);
			}
		}
		else if (paragraph is TextParagraphResult)
		{
			result = ((TextParagraphResult)paragraph).GetBackspaceCaretUnitPosition(position);
		}
		else if (paragraph is TableParagraphResult)
		{
			ReadOnlyCollection<ParagraphResult> paragraphsFromPosition = ((TableParagraphResult)paragraph).GetParagraphsFromPosition(position);
			Invariant.Assert(paragraphsFromPosition != null, "Paragraph collection is null.");
			if (paragraphsFromPosition.Count > 0)
			{
				result = GetBackspaceCaretUnitPosition(paragraphsFromPosition, _emptyParagraphCollection, position);
			}
		}
		else if (paragraph is SubpageParagraphResult)
		{
			SubpageParagraphResult obj = (SubpageParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns = obj.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements = obj.FloatingElements;
			Invariant.Assert(columns != null, "Column collection is null.");
			Invariant.Assert(floatingElements != null, "Paragraph collection is null.");
			if (columns.Count > 0 || floatingElements.Count > 0)
			{
				result = GetBackspaceCaretUnitPosition(columns, floatingElements, position);
			}
		}
		else if (paragraph is FigureParagraphResult)
		{
			FigureParagraphResult obj2 = (FigureParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns2 = obj2.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements2 = obj2.FloatingElements;
			Invariant.Assert(columns2 != null, "Column collection is null.");
			Invariant.Assert(floatingElements2 != null, "Paragraph collection is null.");
			if (columns2.Count > 0 || floatingElements2.Count > 0)
			{
				result = GetBackspaceCaretUnitPosition(columns2, floatingElements2, position);
			}
		}
		else if (paragraph is FloaterParagraphResult)
		{
			FloaterParagraphResult obj3 = (FloaterParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns3 = obj3.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements3 = obj3.FloatingElements;
			Invariant.Assert(columns3 != null, "Column collection is null.");
			Invariant.Assert(floatingElements3 != null, "Paragraph collection is null.");
			if (columns3.Count > 0 || floatingElements3.Count > 0)
			{
				result = GetBackspaceCaretUnitPosition(columns3, floatingElements3, position);
			}
		}
		return result;
	}

	private ITextPointer GetBackspaceCaretUnitPosition(ReadOnlyCollection<ColumnResult> columns, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer position)
	{
		int columnFromPosition = GetColumnFromPosition(columns, position);
		if (columnFromPosition < columns.Count || floatingElements.Count > 0)
		{
			ReadOnlyCollection<ParagraphResult> paragraphs = ((columnFromPosition < columns.Count && columnFromPosition >= 0) ? columns[columnFromPosition].Paragraphs : _emptyParagraphCollection);
			return GetBackspaceCaretUnitPosition(paragraphs, floatingElements, position);
		}
		return position;
	}

	private int GetColumnFromPoint(ReadOnlyCollection<ColumnResult> columns, Point point, bool snapToText)
	{
		int num = -1;
		bool flag = false;
		Invariant.Assert(columns != null, "Column collection is null");
		for (int i = 0; i < columns.Count; i++)
		{
			Rect layoutBox = columns[i].LayoutBox;
			if (!columns[i].HasTextContent)
			{
				if (i == columns.Count - 1)
				{
					num = ((num == -1) ? i : num);
					flag = snapToText;
				}
				continue;
			}
			num = i;
			Invariant.Assert(num == i);
			if (point.X < layoutBox.Left)
			{
				flag = snapToText;
				break;
			}
			if (point.X > layoutBox.Right)
			{
				if (i >= columns.Count - 1)
				{
					flag = snapToText;
					break;
				}
				Rect layoutBox2 = columns[i + 1].LayoutBox;
				if (point.X < layoutBox2.Left)
				{
					double num2 = layoutBox2.Left - layoutBox.Right;
					if (point.X > layoutBox.Right + num2 / 2.0 && columns[i + 1].HasTextContent)
					{
						i++;
						num = i;
					}
					flag = snapToText;
					break;
				}
			}
			else
			{
				if (i >= columns.Count - 1)
				{
					flag = true;
					break;
				}
				Rect layoutBox3 = columns[i + 1].LayoutBox;
				if (point.X < layoutBox3.Left)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			Rect layoutBox = columns[num].LayoutBox;
			flag = snapToText || (point.Y >= layoutBox.Top && point.Y <= layoutBox.Bottom);
		}
		Invariant.Assert(!flag || num < columns.Count, "Column not found.");
		if (!flag)
		{
			return -1;
		}
		return num;
	}

	private int GetParagraphFromPoint(ReadOnlyCollection<ParagraphResult> paragraphs, Point point, bool snapToText)
	{
		int num = -1;
		bool flag = false;
		Invariant.Assert(paragraphs != null, "Paragraph collection is null");
		for (int i = 0; i < paragraphs.Count; i++)
		{
			Rect layoutBox = paragraphs[i].LayoutBox;
			if (!paragraphs[i].HasTextContent)
			{
				if (i == paragraphs.Count - 1)
				{
					num = ((num == -1) ? i : num);
					flag = snapToText;
				}
				continue;
			}
			num = i;
			Invariant.Assert(num == i);
			if (point.Y < layoutBox.Top)
			{
				flag = snapToText;
				break;
			}
			if (point.Y > layoutBox.Bottom)
			{
				if (i >= paragraphs.Count - 1)
				{
					flag = snapToText;
					break;
				}
				Rect layoutBox2 = paragraphs[i + 1].LayoutBox;
				if (point.Y < layoutBox2.Top)
				{
					double num2 = layoutBox2.Top - layoutBox.Bottom;
					if (point.Y > layoutBox.Bottom + num2 / 2.0 && paragraphs[i + 1].HasTextContent)
					{
						i++;
						num = i;
					}
					flag = snapToText;
					break;
				}
			}
			else
			{
				if (i >= paragraphs.Count - 1)
				{
					flag = true;
					break;
				}
				Rect layoutBox3 = paragraphs[i + 1].LayoutBox;
				if (point.Y < layoutBox3.Top)
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			Rect layoutBox = paragraphs[num].LayoutBox;
			flag = snapToText || (point.X >= layoutBox.Left && point.X <= layoutBox.Right);
		}
		Invariant.Assert(!flag || num < paragraphs.Count, "Paragraph not found.");
		if (!flag)
		{
			return -1;
		}
		return num;
	}

	private int GetParagraphFromPointInFloatingElements(ReadOnlyCollection<ParagraphResult> floatingElements, Point point, bool snapToText)
	{
		Invariant.Assert(floatingElements != null, "Paragraph collection is null");
		double num = double.MaxValue;
		int result = -1;
		for (int i = 0; i < floatingElements.Count; i++)
		{
			Rect layoutBox = floatingElements[i].LayoutBox;
			if (layoutBox.Contains(point))
			{
				return i;
			}
			Point point2 = new Point(layoutBox.X + layoutBox.Width / 2.0, layoutBox.Y + layoutBox.Height / 2.0);
			double num2 = Math.Abs(point.X - point2.X) + Math.Abs(point.Y - point2.Y);
			if (num2 < num)
			{
				result = i;
				num = num2;
			}
		}
		if (!snapToText)
		{
			return -1;
		}
		return result;
	}

	private int GetColumnFromPosition(ReadOnlyCollection<ColumnResult> columns, ITextPointer position)
	{
		Invariant.Assert(columns != null, "Column collection is null");
		int i = 0;
		if (columns.Count > 0)
		{
			if (columns.Count == 1)
			{
				i = 0;
			}
			else
			{
				for (i = 0; i < columns.Count && !columns[i].Contains(position, strict: true); i++)
				{
				}
				if (i >= columns.Count)
				{
					if (position.CompareTo(columns[0].StartPosition) == 0)
					{
						i = 0;
					}
					else if (position.CompareTo(columns[columns.Count - 1].EndPosition) == 0)
					{
						i = columns.Count - 1;
					}
				}
			}
		}
		return i;
	}

	private static int GetParagraphFromPosition(ReadOnlyCollection<ParagraphResult> paragraphs, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer position, out bool isFloatingPara)
	{
		Invariant.Assert(paragraphs != null, "Paragraph collection is null.");
		Invariant.Assert(floatingElements != null, "Floating element collection is null.");
		isFloatingPara = false;
		int paragraphFromPosition = GetParagraphFromPosition(floatingElements, position);
		if (paragraphFromPosition < floatingElements.Count)
		{
			isFloatingPara = true;
			return paragraphFromPosition;
		}
		return GetParagraphFromPosition(paragraphs, position);
	}

	private static int GetParagraphFromPosition(ReadOnlyCollection<ParagraphResult> paragraphs, ITextPointer position)
	{
		Invariant.Assert(paragraphs != null, "Paragraph collection is null.");
		int num = 0;
		int num2 = paragraphs.Count - 1;
		int num3 = 0;
		bool flag = false;
		if (paragraphs.Count > 0)
		{
			do
			{
				num = (num2 + num3) / 2;
				if (paragraphs[num].Contains(position, strict: true))
				{
					flag = true;
					break;
				}
				if (num2 == num3)
				{
					break;
				}
				if (position.CompareTo(paragraphs[num].StartPosition) < 0)
				{
					num2 = num - 1;
				}
				else
				{
					num3 = num + 1;
				}
			}
			while (num2 >= num3);
			if (!flag)
			{
				num = ((position.CompareTo(paragraphs[0].StartPosition) != 0) ? ((position.CompareTo(paragraphs[paragraphs.Count - 1].EndPosition) != 0) ? paragraphs.Count : (paragraphs.Count - 1)) : 0);
			}
		}
		return num;
	}

	private TextSegment GetLineRangeFromPosition(ReadOnlyCollection<ParagraphResult> paragraphs, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer position)
	{
		Invariant.Assert(paragraphs != null, "Paragraph collection is null");
		Invariant.Assert(floatingElements != null, "Floating element collection is null");
		TextSegment result = TextSegment.Null;
		bool isFloatingPara;
		int paragraphFromPosition = GetParagraphFromPosition(paragraphs, floatingElements, position, out isFloatingPara);
		ParagraphResult paragraphResult = null;
		if (isFloatingPara)
		{
			Invariant.Assert(paragraphFromPosition < floatingElements.Count);
			paragraphResult = floatingElements[paragraphFromPosition];
		}
		else if (paragraphFromPosition < paragraphs.Count)
		{
			paragraphResult = paragraphs[paragraphFromPosition];
		}
		if (paragraphResult != null)
		{
			result = GetLineRangeFromPosition(paragraphResult, position);
		}
		return result;
	}

	private TextSegment GetLineRangeFromPosition(ParagraphResult paragraph, ITextPointer position)
	{
		TextSegment result = TextSegment.Null;
		if (paragraph is ContainerParagraphResult)
		{
			ReadOnlyCollection<ParagraphResult> paragraphs = ((ContainerParagraphResult)paragraph).Paragraphs;
			Invariant.Assert(paragraphs != null, "Paragraph collection is null.");
			if (paragraphs.Count > 0)
			{
				result = GetLineRangeFromPosition(paragraphs, _emptyParagraphCollection, position);
			}
		}
		else if (paragraph is TextParagraphResult)
		{
			ReadOnlyCollection<LineResult> lines = ((TextParagraphResult)paragraph).Lines;
			Invariant.Assert(lines != null, "Lines collection is null");
			if (!((TextParagraphResult)paragraph).HasTextContent)
			{
				result = new TextSegment(((TextParagraphResult)paragraph).EndPosition, ((TextParagraphResult)paragraph).EndPosition, preserveLogicalDirection: true);
			}
			else
			{
				int lineFromPosition = TextParagraphView.GetLineFromPosition(lines, position);
				Invariant.Assert(lineFromPosition >= 0 && lineFromPosition < lines.Count, "Line not found.");
				result = new TextSegment(lines[lineFromPosition].StartPosition, lines[lineFromPosition].GetContentEndPosition(), preserveLogicalDirection: true);
			}
		}
		else if (paragraph is TableParagraphResult)
		{
			ReadOnlyCollection<ParagraphResult> paragraphsFromPosition = ((TableParagraphResult)paragraph).GetParagraphsFromPosition(position);
			Invariant.Assert(paragraphsFromPosition != null, "Paragraph collection is null.");
			if (paragraphsFromPosition.Count > 0)
			{
				result = GetLineRangeFromPosition(paragraphsFromPosition, _emptyParagraphCollection, position);
			}
		}
		else if (paragraph is SubpageParagraphResult)
		{
			SubpageParagraphResult obj = (SubpageParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns = obj.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements = obj.FloatingElements;
			Invariant.Assert(columns != null, "Column collection is null.");
			Invariant.Assert(floatingElements != null, "Paragraph collection is null.");
			if (columns.Count > 0 || floatingElements.Count > 0)
			{
				result = GetLineRangeFromPosition(columns, floatingElements, position);
			}
		}
		else if (paragraph is FigureParagraphResult)
		{
			FigureParagraphResult obj2 = (FigureParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns2 = obj2.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements2 = obj2.FloatingElements;
			Invariant.Assert(columns2 != null, "Column collection is null.");
			Invariant.Assert(floatingElements2 != null, "Paragraph collection is null.");
			if (columns2.Count > 0 || floatingElements2.Count > 0)
			{
				result = GetLineRangeFromPosition(columns2, floatingElements2, position);
			}
		}
		else if (paragraph is FloaterParagraphResult)
		{
			FloaterParagraphResult obj3 = (FloaterParagraphResult)paragraph;
			ReadOnlyCollection<ColumnResult> columns3 = obj3.Columns;
			ReadOnlyCollection<ParagraphResult> floatingElements3 = obj3.FloatingElements;
			Invariant.Assert(columns3 != null, "Column collection is null.");
			Invariant.Assert(floatingElements3 != null, "Paragraph collection is null.");
			if (columns3.Count > 0 || floatingElements3.Count > 0)
			{
				result = GetLineRangeFromPosition(columns3, floatingElements3, position);
			}
		}
		else if (paragraph is UIElementParagraphResult && paragraph.Element is BlockUIContainer blockUIContainer)
		{
			result = new TextSegment(blockUIContainer.ContentStart.CreatePointer(LogicalDirection.Forward), blockUIContainer.ContentEnd.CreatePointer(LogicalDirection.Backward));
		}
		return result;
	}

	private TextSegment GetLineRangeFromPosition(ReadOnlyCollection<ColumnResult> columns, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer position)
	{
		int columnFromPosition = GetColumnFromPosition(columns, position);
		if (columnFromPosition < columns.Count || floatingElements.Count > 0)
		{
			ReadOnlyCollection<ParagraphResult> paragraphs = ((columnFromPosition < columns.Count && columnFromPosition >= 0) ? columns[columnFromPosition].Paragraphs : _emptyParagraphCollection);
			return GetLineRangeFromPosition(paragraphs, floatingElements, position);
		}
		return TextSegment.Null;
	}

	private ITextPointer GetPositionAtNextLine(ReadOnlyCollection<ParagraphResult> paragraphs, ITextPointer position, double suggestedX, ref int count, out bool positionFound)
	{
		Invariant.Assert(paragraphs != null, "Paragraph collection is empty.");
		ITextPointer textPointer = position;
		positionFound = false;
		int paragraphFromPosition = GetParagraphFromPosition(paragraphs, position);
		if (paragraphFromPosition < paragraphs.Count)
		{
			positionFound = true;
			if (paragraphs[paragraphFromPosition] is ContainerParagraphResult)
			{
				_ = paragraphs[paragraphFromPosition].LayoutBox;
				ReadOnlyCollection<ParagraphResult> paragraphs2 = ((ContainerParagraphResult)paragraphs[paragraphFromPosition]).Paragraphs;
				Invariant.Assert(paragraphs2 != null, "Paragraph collection is null.");
				if (paragraphs2.Count > 0)
				{
					textPointer = GetPositionAtNextLine(paragraphs2, position, suggestedX, ref count, out positionFound);
				}
			}
			else if (paragraphs[paragraphFromPosition] is TextParagraphResult)
			{
				ReadOnlyCollection<LineResult> lines = ((TextParagraphResult)paragraphs[paragraphFromPosition]).Lines;
				Invariant.Assert(lines != null, "Lines collection is null");
				if (!((TextParagraphResult)paragraphs[paragraphFromPosition]).HasTextContent)
				{
					textPointer = position;
				}
				else
				{
					int lineFromPosition = TextParagraphView.GetLineFromPosition(lines, position);
					Invariant.Assert(lineFromPosition >= 0 && lineFromPosition < lines.Count, "Line not found.");
					_ = paragraphs[paragraphFromPosition].LayoutBox;
					int num = lineFromPosition;
					if (lineFromPosition + count < 0)
					{
						lineFromPosition = 0;
						count += num;
					}
					else if (lineFromPosition + count > lines.Count - 1)
					{
						lineFromPosition = lines.Count - 1;
						count -= lines.Count - 1 - num;
					}
					else
					{
						lineFromPosition += count;
						count = 0;
					}
					textPointer = ((count == 0) ? (double.IsNaN(suggestedX) ? lines[lineFromPosition].StartPosition.CreatePointer(LogicalDirection.Forward) : lines[lineFromPosition].GetTextPositionFromDistance(suggestedX)) : ((lineFromPosition == num) ? position : ((count < 0) ? (double.IsNaN(suggestedX) ? lines[0].StartPosition.CreatePointer(LogicalDirection.Forward) : lines[0].GetTextPositionFromDistance(suggestedX)) : (double.IsNaN(suggestedX) ? lines[lines.Count - 1].StartPosition.CreatePointer(LogicalDirection.Forward) : lines[lines.Count - 1].GetTextPositionFromDistance(suggestedX)))));
				}
			}
			else if (paragraphs[paragraphFromPosition] is TableParagraphResult)
			{
				TableParagraphResult tableParagraphResult = (TableParagraphResult)paragraphs[paragraphFromPosition];
				CellParaClient cellParaClientFromPosition = tableParagraphResult.GetCellParaClientFromPosition(position);
				CellParaClient cellParaClient = cellParaClientFromPosition;
				_ = paragraphs[paragraphFromPosition].LayoutBox;
				while ((count != 0 && cellParaClient != null) & positionFound)
				{
					SubpageParagraphResult subpageParagraphResult = (SubpageParagraphResult)cellParaClient.CreateParagraphResult();
					ReadOnlyCollection<ParagraphResult> paragraphs3 = subpageParagraphResult.Columns[0].Paragraphs;
					Invariant.Assert(paragraphs3 != null, "Paragraph collection is null.");
					if (paragraphs3.Count > 0)
					{
						if (cellParaClient != cellParaClientFromPosition)
						{
							int paragraphIndex = ((count <= 0) ? (paragraphs3.Count - 1) : 0);
							textPointer = GetPositionAtNextLineFromSiblingPara(paragraphs3, paragraphIndex, suggestedX - TextDpi.FromTextDpi(cellParaClient.Rect.u), ref count);
							if (textPointer == null)
							{
								textPointer = position;
							}
						}
						else
						{
							textPointer = GetPositionAtNextLine(paragraphs3, position, suggestedX - subpageParagraphResult.ContentOffset.X, ref count, out positionFound);
						}
					}
					if ((count < 0) & positionFound)
					{
						cellParaClient = tableParagraphResult.GetCellAbove(suggestedX, cellParaClient.Cell.RowGroupIndex, cellParaClient.Cell.RowIndex);
					}
					else if ((count > 0) & positionFound)
					{
						cellParaClient = tableParagraphResult.GetCellBelow(suggestedX, cellParaClient.Cell.RowGroupIndex, cellParaClient.Cell.RowIndex + cellParaClient.Cell.RowSpan - 1);
					}
				}
			}
			else if (paragraphs[paragraphFromPosition] is SubpageParagraphResult)
			{
				SubpageParagraphResult subpageParagraphResult2 = (SubpageParagraphResult)paragraphs[paragraphFromPosition];
				textPointer = GetPositionAtNextLine(((SubpageParagraphResult)paragraphs[paragraphFromPosition]).Columns, subpageParagraphResult2.FloatingElements, position, suggestedX - subpageParagraphResult2.ContentOffset.X, ref count, out var _, out positionFound);
			}
			if ((count != 0) & positionFound)
			{
				paragraphFromPosition = ((count <= 0) ? (paragraphFromPosition - 1) : (paragraphFromPosition + 1));
				if (paragraphFromPosition >= 0 && paragraphFromPosition < paragraphs.Count)
				{
					textPointer = GetPositionAtNextLineFromSiblingPara(paragraphs, paragraphFromPosition, suggestedX, ref count);
					if (textPointer == null)
					{
						textPointer = position;
					}
				}
			}
		}
		Invariant.Assert(textPointer != null);
		return textPointer;
	}

	private ITextPointer GetPositionAtNextLineInFloatingElements(ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer position, double suggestedX, ref int count, out bool positionFound)
	{
		ITextPointer textPointer = position;
		positionFound = false;
		ParagraphResult paragraphResult = null;
		int paragraphFromPosition = GetParagraphFromPosition(_emptyParagraphCollection, floatingElements, position, out positionFound);
		if (positionFound)
		{
			Invariant.Assert(paragraphFromPosition < floatingElements.Count);
			paragraphResult = floatingElements[paragraphFromPosition];
			Invariant.Assert(paragraphResult is FigureParagraphResult || paragraphResult is FloaterParagraphResult);
			bool positionFound2;
			if (paragraphResult is FigureParagraphResult)
			{
				FigureParagraphResult figureParagraphResult = (FigureParagraphResult)paragraphResult;
				ReadOnlyCollection<ColumnResult> columns = figureParagraphResult.Columns;
				ReadOnlyCollection<ParagraphResult> floatingElements2 = figureParagraphResult.FloatingElements;
				Invariant.Assert(columns != null, "Column collection is null.");
				Invariant.Assert(floatingElements2 != null, "Paragraph collection is null.");
				if (columns.Count > 0 || floatingElements2.Count > 0)
				{
					textPointer = GetPositionAtNextLine(columns, floatingElements2, position, suggestedX - figureParagraphResult.ContentOffset.X, ref count, out var _, out positionFound2);
				}
			}
			else
			{
				FloaterParagraphResult floaterParagraphResult = (FloaterParagraphResult)paragraphResult;
				ReadOnlyCollection<ColumnResult> columns2 = floaterParagraphResult.Columns;
				ReadOnlyCollection<ParagraphResult> floatingElements3 = floaterParagraphResult.FloatingElements;
				Invariant.Assert(columns2 != null, "Column collection is null.");
				Invariant.Assert(floatingElements3 != null, "Paragraph collection is null.");
				if (columns2.Count > 0 || floatingElements3.Count > 0)
				{
					textPointer = GetPositionAtNextLine(columns2, floatingElements3, position, suggestedX - floaterParagraphResult.ContentOffset.X, ref count, out var _, out positionFound2);
				}
			}
		}
		Invariant.Assert(textPointer != null);
		return textPointer;
	}

	private ITextPointer GetPositionAtNextLine(ReadOnlyCollection<ColumnResult> columns, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer position, double suggestedX, ref int count, out double newSuggestedX, out bool positionFound)
	{
		ITextPointer textPointer = null;
		newSuggestedX = suggestedX;
		positionFound = false;
		if (floatingElements.Count > 0)
		{
			textPointer = GetPositionAtNextLineInFloatingElements(floatingElements, position, suggestedX, ref count, out positionFound);
		}
		if (!positionFound)
		{
			int columnFromPosition = GetColumnFromPosition(columns, position);
			if (columnFromPosition < columns.Count)
			{
				positionFound = true;
				textPointer = GetPositionAtNextLine(columns[columnFromPosition].Paragraphs, position, suggestedX, ref count, out positionFound);
				int index = columnFromPosition;
				if ((count != 0) & positionFound)
				{
					columnFromPosition = ((count <= 0) ? (columnFromPosition - 1) : (columnFromPosition + 1));
					if (columnFromPosition >= 0 && columnFromPosition < columns.Count)
					{
						suggestedX = suggestedX - columns[index].LayoutBox.Left + columns[columnFromPosition].LayoutBox.Left;
						ITextPointer positionAtNextLineFromSiblingColumn = GetPositionAtNextLineFromSiblingColumn(columns, columnFromPosition, suggestedX, ref newSuggestedX, ref count);
						if (positionAtNextLineFromSiblingColumn != null)
						{
							textPointer = positionAtNextLineFromSiblingColumn;
						}
					}
				}
			}
		}
		Invariant.Assert(textPointer != null);
		return textPointer;
	}

	private ITextPointer GetPositionAtNextLineFromSiblingPara(ReadOnlyCollection<ParagraphResult> paragraphs, int paragraphIndex, double suggestedX, ref int count)
	{
		Invariant.Assert(count != 0);
		Invariant.Assert(paragraphIndex >= 0 && paragraphIndex < paragraphs.Count, "Paragraph collection is empty.");
		ITextPointer result = null;
		while (paragraphIndex >= 0 && paragraphIndex < paragraphs.Count)
		{
			if (paragraphs[paragraphIndex] is ContainerParagraphResult)
			{
				_ = paragraphs[paragraphIndex].LayoutBox;
				ReadOnlyCollection<ParagraphResult> paragraphs2 = ((ContainerParagraphResult)paragraphs[paragraphIndex]).Paragraphs;
				Invariant.Assert(paragraphs2 != null, "Paragraph collection is null.");
				if (paragraphs2.Count > 0)
				{
					int paragraphIndex2 = ((count <= 0) ? (paragraphs2.Count - 1) : 0);
					result = GetPositionAtNextLineFromSiblingPara(paragraphs2, paragraphIndex2, suggestedX, ref count);
				}
			}
			else if (paragraphs[paragraphIndex] is TextParagraphResult)
			{
				result = GetPositionAtNextLineFromSiblingTextPara((TextParagraphResult)paragraphs[paragraphIndex], suggestedX, ref count);
				if (count == 0)
				{
					break;
				}
			}
			else if (paragraphs[paragraphIndex] is TableParagraphResult)
			{
				TableParagraphResult tableParagraphResult = (TableParagraphResult)paragraphs[paragraphIndex];
				_ = paragraphs[paragraphIndex].LayoutBox;
				CellParaClient cellParaClient = null;
				if (count < 0)
				{
					cellParaClient = tableParagraphResult.GetCellAbove(suggestedX, int.MaxValue, int.MaxValue);
				}
				else if (count > 0)
				{
					cellParaClient = tableParagraphResult.GetCellBelow(suggestedX, int.MinValue, int.MinValue);
				}
				while (count != 0 && cellParaClient != null)
				{
					SubpageParagraphResult subpageParagraphResult = (SubpageParagraphResult)cellParaClient.CreateParagraphResult();
					ReadOnlyCollection<ParagraphResult> paragraphs3 = subpageParagraphResult.Columns[0].Paragraphs;
					Invariant.Assert(paragraphs3 != null, "Paragraph collection is null.");
					if (paragraphs3.Count > 0)
					{
						int paragraphIndex3 = ((count <= 0) ? (paragraphs3.Count - 1) : 0);
						result = GetPositionAtNextLineFromSiblingPara(paragraphs3, paragraphIndex3, suggestedX - subpageParagraphResult.ContentOffset.X, ref count);
					}
					if (count < 0)
					{
						cellParaClient = tableParagraphResult.GetCellAbove(suggestedX, cellParaClient.Cell.RowGroupIndex, cellParaClient.Cell.RowIndex);
					}
					else if (count > 0)
					{
						cellParaClient = tableParagraphResult.GetCellBelow(suggestedX, cellParaClient.Cell.RowGroupIndex, cellParaClient.Cell.RowIndex + cellParaClient.Cell.RowSpan - 1);
					}
				}
			}
			else if (paragraphs[paragraphIndex] is SubpageParagraphResult)
			{
				_ = paragraphs[paragraphIndex].LayoutBox;
				SubpageParagraphResult subpageParagraphResult2 = (SubpageParagraphResult)paragraphs[paragraphIndex];
				ReadOnlyCollection<ParagraphResult> paragraphs4 = subpageParagraphResult2.Columns[0].Paragraphs;
				Invariant.Assert(paragraphs4 != null, "Paragraph collection is null.");
				if (paragraphs4.Count > 0)
				{
					int paragraphIndex4 = ((count <= 0) ? (paragraphs4.Count - 1) : 0);
					result = GetPositionAtNextLineFromSiblingPara(paragraphs4, paragraphIndex4, suggestedX - subpageParagraphResult2.ContentOffset.X, ref count);
				}
			}
			else if (paragraphs[paragraphIndex] is UIElementParagraphResult)
			{
				if (count < 0)
				{
					count++;
				}
				else
				{
					count--;
				}
				if (count == 0)
				{
					Rect layoutBox = paragraphs[paragraphIndex].LayoutBox;
					if (paragraphs[paragraphIndex].Element is BlockUIContainer blockUIContainer)
					{
						result = ((!DoubleUtil.LessThanOrClose(suggestedX, layoutBox.Width / 2.0)) ? blockUIContainer.ContentEnd.CreatePointer(LogicalDirection.Backward) : blockUIContainer.ContentStart.CreatePointer(LogicalDirection.Forward));
					}
				}
			}
			if (count < 0)
			{
				paragraphIndex--;
				continue;
			}
			if (count <= 0)
			{
				break;
			}
			paragraphIndex++;
		}
		return result;
	}

	private ITextPointer GetPositionAtNextLineFromSiblingTextPara(TextParagraphResult paragraph, double suggestedX, ref int count)
	{
		ITextPointer textPointer = null;
		ReadOnlyCollection<LineResult> lines = paragraph.Lines;
		Invariant.Assert(lines != null, "Lines collection is null");
		if (!paragraph.HasTextContent)
		{
			return null;
		}
		_ = paragraph.LayoutBox;
		int num = ((count <= 0) ? (lines.Count - 1) : 0);
		if (count < 0)
		{
			count++;
		}
		else
		{
			count--;
		}
		if (num + count < 0)
		{
			count += num;
		}
		else if (num + count > lines.Count - 1)
		{
			count -= lines.Count - 1 - num;
		}
		else
		{
			num += count;
			count = 0;
		}
		if (count == 0)
		{
			if (!double.IsNaN(suggestedX))
			{
				return lines[num].GetTextPositionFromDistance(suggestedX);
			}
			return lines[num].StartPosition.CreatePointer(LogicalDirection.Forward);
		}
		if (count < 0)
		{
			if (!double.IsNaN(suggestedX))
			{
				return lines[0].GetTextPositionFromDistance(suggestedX);
			}
			return lines[0].StartPosition.CreatePointer(LogicalDirection.Forward);
		}
		if (!double.IsNaN(suggestedX))
		{
			return lines[lines.Count - 1].GetTextPositionFromDistance(suggestedX);
		}
		return lines[lines.Count - 1].StartPosition.CreatePointer(LogicalDirection.Forward);
	}

	private ITextPointer GetPositionAtNextLineFromSiblingColumn(ReadOnlyCollection<ColumnResult> columns, int columnIndex, double columnSuggestedX, ref double newSuggestedX, ref int count)
	{
		ITextPointer result = null;
		while (columnIndex >= 0 && columnIndex < columns.Count)
		{
			_ = columns[columnIndex].LayoutBox.Left;
			ReadOnlyCollection<ParagraphResult> paragraphs = columns[columnIndex].Paragraphs;
			Invariant.Assert(paragraphs != null, "Paragraph collection is null.");
			if (paragraphs.Count > 0)
			{
				int paragraphIndex = ((count <= 0) ? (paragraphs.Count - 1) : 0);
				result = GetPositionAtNextLineFromSiblingPara(paragraphs, paragraphIndex, columnSuggestedX, ref count);
			}
			newSuggestedX = columnSuggestedX;
			if (count < 0)
			{
				columnIndex--;
				continue;
			}
			if (count <= 0)
			{
				break;
			}
			columnIndex++;
		}
		return result;
	}

	private bool ContainsCore(ITextPointer position)
	{
		ReadOnlyCollection<TextSegment> textSegmentsCore = TextSegmentsCore;
		Invariant.Assert(textSegmentsCore != null, "TextSegment collection is empty.");
		return Contains(position, textSegmentsCore);
	}

	private bool GetGlyphRunsFromParagraphs(List<GlyphRun> glyphRuns, ITextPointer start, ITextPointer end, ReadOnlyCollection<ParagraphResult> paragraphs)
	{
		Invariant.Assert(paragraphs != null, "Paragraph collection is null.");
		bool flag = true;
		for (int i = 0; i < paragraphs.Count; i++)
		{
			ParagraphResult paragraphResult = paragraphs[i];
			if (paragraphResult is TextParagraphResult)
			{
				TextParagraphResult textParagraphResult = (TextParagraphResult)paragraphResult;
				if (start.CompareTo(textParagraphResult.EndPosition) < 0 && end.CompareTo(textParagraphResult.StartPosition) > 0)
				{
					ITextPointer start2 = ((start.CompareTo(textParagraphResult.StartPosition) < 0) ? textParagraphResult.StartPosition : start);
					ITextPointer end2 = ((end.CompareTo(textParagraphResult.EndPosition) < 0) ? end : textParagraphResult.EndPosition);
					textParagraphResult.GetGlyphRuns(glyphRuns, start2, end2);
				}
				if (end.CompareTo(textParagraphResult.EndPosition) < 0)
				{
					flag = false;
					break;
				}
			}
			else
			{
				if (!(paragraphResult is ContainerParagraphResult))
				{
					continue;
				}
				ReadOnlyCollection<ParagraphResult> paragraphs2 = ((ContainerParagraphResult)paragraphResult).Paragraphs;
				Invariant.Assert(paragraphs2 != null, "Paragraph collection is null.");
				if (paragraphs2.Count > 0)
				{
					flag = GetGlyphRunsFromParagraphs(glyphRuns, start, end, paragraphs2);
					if (!flag)
					{
						break;
					}
				}
			}
		}
		return flag;
	}

	private void GetGlyphRunsFromFloatingElements(List<GlyphRun> glyphRuns, ITextPointer start, ITextPointer end, ReadOnlyCollection<ParagraphResult> floatingElements, out bool success)
	{
		Invariant.Assert(floatingElements != null, "Paragraph collection is null.");
		success = false;
		for (int i = 0; i < floatingElements.Count; i++)
		{
			ParagraphResult paragraphResult = floatingElements[i];
			Invariant.Assert(paragraphResult is FigureParagraphResult || paragraphResult is FloaterParagraphResult);
			if (!paragraphResult.Contains(start, strict: true))
			{
				continue;
			}
			success = true;
			ITextPointer end2 = ((end.CompareTo(paragraphResult.EndPosition) < 0) ? end : paragraphResult.EndPosition);
			if (paragraphResult is FigureParagraphResult)
			{
				FigureParagraphResult obj = (FigureParagraphResult)paragraphResult;
				ReadOnlyCollection<ColumnResult> columns = obj.Columns;
				ReadOnlyCollection<ParagraphResult> floatingElements2 = obj.FloatingElements;
				Invariant.Assert(columns != null, "Column collection is null.");
				Invariant.Assert(floatingElements2 != null, "Paragraph collection is null.");
				if (columns.Count > 0 || floatingElements2.Count > 0)
				{
					GetGlyphRuns(glyphRuns, start, end2, columns, floatingElements2);
				}
			}
			else if (paragraphResult is FloaterParagraphResult)
			{
				FloaterParagraphResult obj2 = (FloaterParagraphResult)paragraphResult;
				ReadOnlyCollection<ColumnResult> columns2 = obj2.Columns;
				ReadOnlyCollection<ParagraphResult> floatingElements3 = obj2.FloatingElements;
				Invariant.Assert(columns2 != null, "Column collection is null.");
				Invariant.Assert(floatingElements3 != null, "Paragraph collection is null.");
				if (columns2.Count > 0 || floatingElements3.Count > 0)
				{
					GetGlyphRuns(glyphRuns, start, end2, columns2, floatingElements3);
				}
			}
			break;
		}
	}

	private void GetGlyphRuns(List<GlyphRun> glyphRuns, ITextPointer start, ITextPointer end, ReadOnlyCollection<ColumnResult> columns, ReadOnlyCollection<ParagraphResult> floatingElements)
	{
		bool success = false;
		if (floatingElements.Count > 0)
		{
			GetGlyphRunsFromFloatingElements(glyphRuns, start, end, floatingElements, out success);
		}
		if (success)
		{
			return;
		}
		int i;
		for (i = 0; i < columns.Count; i++)
		{
			ColumnResult columnResult = columns[i];
			if (start.CompareTo(columnResult.StartPosition) >= 0 && start.CompareTo(columnResult.EndPosition) <= 0)
			{
				break;
			}
		}
		int j;
		for (j = i; j < columns.Count; j++)
		{
			ColumnResult columnResult2 = columns[i];
			if (end.CompareTo(columnResult2.StartPosition) >= 0 && end.CompareTo(columnResult2.EndPosition) <= 0)
			{
				break;
			}
		}
		Invariant.Assert(i < columns.Count && j < columns.Count, "Start or End position does not belong to TextView's content range");
		for (; i <= j; i++)
		{
			ReadOnlyCollection<ParagraphResult> paragraphs = columns[i].Paragraphs;
			if (paragraphs != null && paragraphs.Count > 0)
			{
				GetGlyphRunsFromParagraphs(glyphRuns, start, end, paragraphs);
			}
		}
	}

	private ReadOnlyCollection<TextSegment> GetTextSegments()
	{
		ReadOnlyCollection<TextSegment> readOnlyCollection;
		if (!_owner.FinitePage)
		{
			ITextPointer endPosition = _textContainer.End;
			BackgroundFormatInfo backgroundFormatInfo = _owner.StructuralCache.BackgroundFormatInfo;
			if (backgroundFormatInfo != null && backgroundFormatInfo.CPInterrupted != -1)
			{
				endPosition = _textContainer.Start.CreatePointer(backgroundFormatInfo.CPInterrupted, LogicalDirection.Backward);
			}
			readOnlyCollection = new ReadOnlyCollection<TextSegment>(new List<TextSegment>(1)
			{
				new TextSegment(_textContainer.Start, endPosition, preserveLogicalDirection: true)
			});
		}
		else
		{
			TextContentRange textContentRange = new TextContentRange();
			ReadOnlyCollection<ColumnResult> columns = Columns;
			Invariant.Assert(columns != null, "Column collection is empty.");
			for (int i = 0; i < columns.Count; i++)
			{
				textContentRange.Merge(columns[i].TextContentRange);
			}
			readOnlyCollection = textContentRange.GetTextSegments();
		}
		Invariant.Assert(readOnlyCollection != null);
		return readOnlyCollection;
	}

	private void TransformToContent(ref Point point)
	{
		if ((FlowDirection)_owner.StructuralCache.PropertyOwner.GetValue(FlowDocument.FlowDirectionProperty) == FlowDirection.RightToLeft)
		{
			new MatrixTransform(-1.0, 0.0, 0.0, 1.0, _owner.Size.Width, 0.0).TryTransform(point, out var result);
			point = result;
		}
	}

	private void TransformToContent(ref Rect rect)
	{
		if ((FlowDirection)_owner.StructuralCache.PropertyOwner.GetValue(FlowDocument.FlowDirectionProperty) == FlowDirection.RightToLeft)
		{
			MatrixTransform matrixTransform = new MatrixTransform(-1.0, 0.0, 0.0, 1.0, _owner.Size.Width, 0.0);
			rect = matrixTransform.TransformBounds(rect);
		}
	}

	private void TransformFromContent(ref Rect rect, out Transform transform)
	{
		transform = Transform.Identity;
		if (!(rect == Rect.Empty) && (FlowDirection)_owner.StructuralCache.PropertyOwner.GetValue(FlowDocument.FlowDirectionProperty) == FlowDirection.RightToLeft)
		{
			transform = new MatrixTransform(-1.0, 0.0, 0.0, 1.0, _owner.Size.Width, 0.0);
		}
	}

	private void TransformFromContent(ref Point point)
	{
		if ((FlowDirection)_owner.StructuralCache.PropertyOwner.GetValue(FlowDocument.FlowDirectionProperty) == FlowDirection.RightToLeft)
		{
			new MatrixTransform(-1.0, 0.0, 0.0, 1.0, _owner.Size.Width, 0.0).TryTransform(point, out var result);
			point = result;
		}
	}

	private void TransformFromContent(Geometry geometry)
	{
		if ((FlowDirection)_owner.StructuralCache.PropertyOwner.GetValue(FlowDocument.FlowDirectionProperty) == FlowDirection.RightToLeft)
		{
			MatrixTransform transformToAdd = new MatrixTransform(-1.0, 0.0, 0.0, 1.0, _owner.Size.Width, 0.0);
			CaretElement.AddTransformToGeometry(geometry, transformToAdd);
		}
	}

	private static void TransformToSubpage(ref Point point, Vector subpageOffset)
	{
		point -= subpageOffset;
	}

	private static void TransformToSubpage(ref Rect rect, Vector subpageOffset)
	{
		if (!(rect == Rect.Empty))
		{
			rect.Offset(-subpageOffset);
		}
	}

	private static void TransformFromSubpage(ref Rect rect, Vector subpageOffset)
	{
		if (!(rect == Rect.Empty))
		{
			rect.Offset(subpageOffset);
		}
	}

	private static void TransformFromSubpage(Geometry geometry, Vector subpageOffset)
	{
		if (geometry != null && (!DoubleUtil.IsZero(subpageOffset.X) || !DoubleUtil.IsZero(subpageOffset.Y)))
		{
			TranslateTransform transformToAdd = new TranslateTransform(subpageOffset.X, subpageOffset.Y);
			CaretElement.AddTransformToGeometry(geometry, transformToAdd);
		}
	}

	private Rect GetRectangleFromEdge(ParagraphResult paragraphResult, ITextPointer textPointer)
	{
		if (paragraphResult.Element is TextElement textElement)
		{
			if (textPointer.LogicalDirection == LogicalDirection.Forward && textPointer.CompareTo(textElement.ElementStart) == 0)
			{
				return new Rect(paragraphResult.LayoutBox.Left, paragraphResult.LayoutBox.Top, 0.0, paragraphResult.LayoutBox.Height);
			}
			if (textPointer.LogicalDirection == LogicalDirection.Backward && textPointer.CompareTo(textElement.ElementEnd) == 0)
			{
				return new Rect(paragraphResult.LayoutBox.Right, paragraphResult.LayoutBox.Top, 0.0, paragraphResult.LayoutBox.Height);
			}
		}
		return Rect.Empty;
	}

	private Rect GetRectangleFromContentEdge(ParagraphResult paragraphResult, ITextPointer textPointer)
	{
		if (paragraphResult.Element is TextElement textElement)
		{
			Invariant.Assert(textElement is BlockUIContainer, "Expecting BlockUIContainer");
			if (textPointer.CompareTo(textElement.ContentStart) == 0)
			{
				return new Rect(paragraphResult.LayoutBox.Left, paragraphResult.LayoutBox.Top, 0.0, paragraphResult.LayoutBox.Height);
			}
			if (textPointer.CompareTo(textElement.ContentEnd) == 0)
			{
				return new Rect(paragraphResult.LayoutBox.Right, paragraphResult.LayoutBox.Top, 0.0, paragraphResult.LayoutBox.Height);
			}
		}
		return Rect.Empty;
	}
}
