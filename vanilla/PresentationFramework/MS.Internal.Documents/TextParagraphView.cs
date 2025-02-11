using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MS.Internal.Documents;

internal class TextParagraphView : TextViewBase
{
	private readonly TextBlock _owner;

	private readonly ITextContainer _textContainer;

	private ReadOnlyCollection<LineResult> _lines;

	internal override UIElement RenderScope => _owner;

	internal override ITextContainer TextContainer => _textContainer;

	internal override bool IsValid => _owner.IsLayoutDataValid;

	internal override ReadOnlyCollection<TextSegment> TextSegments => new ReadOnlyCollection<TextSegment>(new List<TextSegment>(1)
	{
		new TextSegment(_textContainer.Start, _textContainer.End, preserveLogicalDirection: true)
	});

	internal ReadOnlyCollection<LineResult> Lines
	{
		get
		{
			if (_lines == null)
			{
				_lines = _owner.GetLineResults();
			}
			return _lines;
		}
	}

	internal TextParagraphView(TextBlock owner, ITextContainer textContainer)
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
		ITextPointer textPositionFromPoint = GetTextPositionFromPoint(Lines, point, snapToText);
		Invariant.Assert(textPositionFromPoint?.HasValidLayout ?? true);
		return textPositionFromPoint;
	}

	internal override Rect GetRawRectangleFromTextPosition(ITextPointer position, out Transform transform)
	{
		transform = Transform.Identity;
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		return _owner.GetRectangleFromTextPosition(position);
	}

	internal override Geometry GetTightBoundingGeometryFromTextPositions(ITextPointer startPosition, ITextPointer endPosition)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (startPosition == null)
		{
			throw new ArgumentNullException("startPosition");
		}
		if (endPosition == null)
		{
			throw new ArgumentNullException("endPosition");
		}
		ValidationHelper.VerifyPosition(_textContainer, startPosition, "startPosition");
		ValidationHelper.VerifyDirection(startPosition.LogicalDirection, "startPosition.LogicalDirection");
		ValidationHelper.VerifyPosition(_textContainer, endPosition, "endPosition");
		return _owner.GetTightBoundingGeometryFromTextPositions(startPosition, endPosition);
	}

	internal override ITextPointer GetPositionAtNextLine(ITextPointer position, double suggestedX, int count, out double newSuggestedX, out int linesMoved)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		newSuggestedX = suggestedX;
		linesMoved = 0;
		if (count == 0)
		{
			return position;
		}
		ReadOnlyCollection<LineResult> lines = Lines;
		int lineFromPosition = GetLineFromPosition(lines, position);
		if (lineFromPosition < 0 || lineFromPosition >= lines.Count)
		{
			throw new ArgumentOutOfRangeException("position");
		}
		int num = lineFromPosition;
		lineFromPosition = Math.Max(0, lineFromPosition + count);
		lineFromPosition = Math.Min(lines.Count - 1, lineFromPosition);
		linesMoved = lineFromPosition - num;
		ITextPointer textPointer = ((linesMoved == 0) ? position : (double.IsNaN(suggestedX) ? lines[lineFromPosition].StartPosition.CreatePointer(LogicalDirection.Forward) : lines[lineFromPosition].GetTextPositionFromDistance(suggestedX)));
		Invariant.Assert(textPointer?.HasValidLayout ?? true);
		return textPointer;
	}

	internal override bool IsAtCaretUnitBoundary(ITextPointer position)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		int lineFromPosition = GetLineFromPosition(Lines, position);
		int startPositionCP = Lines[lineFromPosition].StartPositionCP;
		return _owner.IsAtCaretUnitBoundary(position, startPositionCP, lineFromPosition);
	}

	internal override ITextPointer GetNextCaretUnitPosition(ITextPointer position, LogicalDirection direction)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		int lineFromPosition = GetLineFromPosition(Lines, position);
		int startPositionCP = Lines[lineFromPosition].StartPositionCP;
		ITextPointer nextCaretUnitPosition = _owner.GetNextCaretUnitPosition(position, direction, startPositionCP, lineFromPosition);
		Invariant.Assert(nextCaretUnitPosition?.HasValidLayout ?? true);
		return nextCaretUnitPosition;
	}

	internal override ITextPointer GetBackspaceCaretUnitPosition(ITextPointer position)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		int lineFromPosition = GetLineFromPosition(Lines, position);
		int startPositionCP = Lines[lineFromPosition].StartPositionCP;
		ITextPointer backspaceCaretUnitPosition = _owner.GetBackspaceCaretUnitPosition(position, startPositionCP, lineFromPosition);
		Invariant.Assert(backspaceCaretUnitPosition?.HasValidLayout ?? true);
		return backspaceCaretUnitPosition;
	}

	internal override TextSegment GetLineRange(ITextPointer position)
	{
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		ReadOnlyCollection<LineResult> lines = Lines;
		int lineFromPosition = GetLineFromPosition(lines, position);
		return new TextSegment(lines[lineFromPosition].StartPosition, lines[lineFromPosition].GetContentEndPosition(), preserveLogicalDirection: true);
	}

	internal override bool Contains(ITextPointer position)
	{
		if (position == null)
		{
			throw new ArgumentNullException("position");
		}
		ValidationHelper.VerifyPosition(_textContainer, position, "position");
		if (!IsValid)
		{
			throw new InvalidOperationException(SR.TextViewInvalidLayout);
		}
		return true;
	}

	internal override bool Validate()
	{
		_owner.UpdateLayout();
		return IsValid;
	}

	internal static ITextPointer GetTextPositionFromPoint(ReadOnlyCollection<LineResult> lines, Point point, bool snapToText)
	{
		int lineFromPoint = GetLineFromPoint(lines, point, snapToText);
		if (lineFromPoint < 0)
		{
			return null;
		}
		return lines[lineFromPoint].GetTextPositionFromDistance(point.X);
	}

	internal static int GetLineFromPosition(ReadOnlyCollection<LineResult> lines, ITextPointer position)
	{
		int num = -1;
		int num2 = 0;
		int num3 = lines.Count - 1;
		int num4 = lines[0].StartPosition.GetOffsetToPosition(position) + lines[0].StartPositionCP;
		if (num4 < lines[0].StartPositionCP || num4 > lines[lines.Count - 1].EndPositionCP)
		{
			if (num4 >= lines[0].StartPositionCP)
			{
				return lines.Count - 1;
			}
			return 0;
		}
		num = 0;
		while (num2 < num3)
		{
			num = ((num3 - num2 >= 2) ? (num2 + (num3 - num2) / 2) : ((num == num2) ? num3 : num2));
			if (num4 < lines[num].StartPositionCP)
			{
				num3 = num;
				continue;
			}
			if (num4 > lines[num].EndPositionCP)
			{
				num2 = num;
				continue;
			}
			if (num4 == lines[num].EndPositionCP)
			{
				if (position.LogicalDirection == LogicalDirection.Forward && num != lines.Count - 1)
				{
					num++;
				}
			}
			else if (num4 == lines[num].StartPositionCP && position.LogicalDirection == LogicalDirection.Backward && num != 0)
			{
				num--;
			}
			break;
		}
		return num;
	}

	internal void OnUpdated()
	{
		OnUpdated(EventArgs.Empty);
	}

	internal void Invalidate()
	{
		_lines = null;
	}

	internal static int GetLineFromPoint(ReadOnlyCollection<LineResult> lines, Point point, bool snapToText)
	{
		int lineIndex;
		bool flag = GetVerticalLineFromPoint(lines, point, snapToText, out lineIndex);
		if (flag)
		{
			flag = GetHorizontalLineFromPoint(lines, point, snapToText, ref lineIndex);
		}
		if (!flag)
		{
			return -1;
		}
		return lineIndex;
	}

	private static bool GetVerticalLineFromPoint(ReadOnlyCollection<LineResult> lines, Point point, bool snapToText, out int lineIndex)
	{
		bool flag = false;
		double height = lines[0].LayoutBox.Height;
		lineIndex = Math.Max(Math.Min((int)(point.Y / height), lines.Count - 1), 0);
		while (!flag)
		{
			Rect layoutBox = lines[lineIndex].LayoutBox;
			if (point.Y < layoutBox.Y)
			{
				if (lineIndex > 0)
				{
					lineIndex--;
					continue;
				}
				flag = snapToText;
				break;
			}
			if (point.Y > layoutBox.Y + layoutBox.Height)
			{
				if (lineIndex < lines.Count - 1)
				{
					Rect layoutBox2 = lines[lineIndex + 1].LayoutBox;
					if (point.Y < layoutBox2.Y)
					{
						double num = layoutBox2.Y - (layoutBox.Y + layoutBox.Height);
						if (point.Y > layoutBox.Y + layoutBox.Height + num / 2.0)
						{
							lineIndex++;
						}
						flag = snapToText;
						break;
					}
					lineIndex++;
					continue;
				}
				flag = snapToText;
				break;
			}
			double num2 = 0.0;
			if (lineIndex > 0)
			{
				Rect layoutBox3 = lines[lineIndex - 1].LayoutBox;
				num2 = layoutBox.Y - (layoutBox3.Y + layoutBox3.Height);
			}
			if (num2 < 0.0)
			{
				if (point.Y < layoutBox.Y - num2 / 2.0)
				{
					lineIndex--;
				}
			}
			else
			{
				num2 = 0.0;
				if (lineIndex < lines.Count - 1)
				{
					num2 = lines[lineIndex + 1].LayoutBox.Y - (layoutBox.Y + layoutBox.Height);
				}
				if (num2 < 0.0 && point.Y > layoutBox.Y + layoutBox.Height + num2 / 2.0)
				{
					lineIndex++;
				}
			}
			flag = true;
			break;
		}
		return flag;
	}

	private static bool GetHorizontalLineFromPoint(ReadOnlyCollection<LineResult> lines, Point point, bool snapToText, ref int lineIndex)
	{
		bool result = false;
		bool flag = true;
		while (flag)
		{
			Rect layoutBox = lines[lineIndex].LayoutBox;
			if (point.X < layoutBox.X && lineIndex > 0)
			{
				Rect layoutBox2 = lines[lineIndex - 1].LayoutBox;
				if (DoubleUtil.AreClose(layoutBox2.Y, layoutBox.Y))
				{
					if (point.X <= layoutBox2.X + layoutBox2.Width)
					{
						lineIndex--;
						continue;
					}
					double num = Math.Max(layoutBox.X - (layoutBox2.X + layoutBox2.Width), 0.0);
					if (point.X < layoutBox.X - num / 2.0)
					{
						lineIndex--;
					}
					result = snapToText;
					flag = false;
					break;
				}
				result = snapToText;
				flag = false;
				break;
			}
			if (point.X > layoutBox.X + layoutBox.Width && lineIndex < lines.Count - 1)
			{
				Rect layoutBox2 = lines[lineIndex + 1].LayoutBox;
				if (DoubleUtil.AreClose(layoutBox2.Y, layoutBox.Y))
				{
					if (point.X >= layoutBox2.X)
					{
						lineIndex++;
						continue;
					}
					double num = Math.Max(layoutBox2.X - (layoutBox.X + layoutBox.Width), 0.0);
					if (point.X > layoutBox2.X - num / 2.0)
					{
						lineIndex++;
					}
					result = snapToText;
					flag = false;
					break;
				}
				result = snapToText;
				flag = false;
				break;
			}
			result = snapToText || (point.X >= layoutBox.X && point.X <= layoutBox.X + layoutBox.Width);
			flag = false;
			break;
		}
		return result;
	}
}
