using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.Text;

internal abstract class Line : TextSource, IDisposable
{
	protected TextBlock _owner;

	protected TextLine _line;

	protected int _dcp;

	protected static int _syntheticCharacterLength = 1;

	protected bool _mirror;

	protected TextAlignment _textAlignment;

	protected bool _showParagraphEllipsis;

	protected double _wrappingWidth;

	internal double Width
	{
		get
		{
			if (IsWidthAdjusted)
			{
				return _line.WidthIncludingTrailingWhitespace;
			}
			return _line.Width;
		}
	}

	internal double Start
	{
		get
		{
			if (IsXOffsetAdjusted)
			{
				return _line.Start + CalculateXOffsetShift();
			}
			return _line.Start;
		}
	}

	internal double Height => _line.Height;

	internal double BaselineOffset => _line.Baseline;

	internal bool EndOfParagraph
	{
		get
		{
			if (_line.NewlineLength == 0)
			{
				return false;
			}
			IList<TextSpan<TextRun>> textRunSpans = _line.GetTextRunSpans();
			return textRunSpans[textRunSpans.Count - 1].Value is TextEndOfParagraph;
		}
	}

	internal int Length => _line.Length - (EndOfParagraph ? _syntheticCharacterLength : 0);

	internal int ContentLength => _line.Length - _line.NewlineLength;

	protected bool ShowEllipsis
	{
		get
		{
			if (_owner.ParagraphProperties.TextTrimming == TextTrimming.None)
			{
				return false;
			}
			if (_line.HasOverflowed || _showParagraphEllipsis)
			{
				return true;
			}
			return false;
		}
	}

	protected bool HasLineBreak => _line.NewlineLength > 0;

	protected bool IsXOffsetAdjusted
	{
		get
		{
			if (_textAlignment == TextAlignment.Right || _textAlignment == TextAlignment.Center)
			{
				return IsWidthAdjusted;
			}
			return false;
		}
	}

	protected bool IsWidthAdjusted
	{
		get
		{
			bool result = false;
			if ((HasLineBreak || EndOfParagraph) && !ShowEllipsis)
			{
				result = true;
			}
			return result;
		}
	}

	public void Dispose()
	{
		if (_line != null)
		{
			_line.Dispose();
			_line = null;
		}
		GC.SuppressFinalize(this);
	}

	internal Line(TextBlock owner)
	{
		_owner = owner;
		_textAlignment = owner.TextAlignment;
		_showParagraphEllipsis = false;
		_wrappingWidth = _owner.RenderSize.Width;
		base.PixelsPerDip = _owner.GetDpi().PixelsPerDip;
	}

	internal void Format(int dcp, double width, TextParagraphProperties lineProperties, TextLineBreak textLineBreak, TextRunCache textRunCache, bool showParagraphEllipsis)
	{
		_mirror = lineProperties.FlowDirection == FlowDirection.RightToLeft;
		_dcp = dcp;
		_showParagraphEllipsis = showParagraphEllipsis;
		_wrappingWidth = width;
		_line = _owner.TextFormatter.FormatLine(this, dcp, width, lineProperties, textLineBreak, textRunCache);
	}

	internal virtual void Arrange(VisualCollection vc, Vector lineOffset)
	{
	}

	internal void Render(DrawingContext ctx, Point lineOffset)
	{
		TextLine textLine = _line;
		if (_line.HasOverflowed && _owner.ParagraphProperties.TextTrimming != 0)
		{
			textLine = _line.Collapse(GetCollapsingProps(_wrappingWidth, _owner.ParagraphProperties));
		}
		double num = CalculateXOffsetShift();
		textLine.Draw(ctx, new Point(lineOffset.X + num, lineOffset.Y), _mirror ? InvertAxes.Horizontal : InvertAxes.None);
	}

	internal Rect GetBoundsFromTextPosition(int characterIndex, out FlowDirection flowDirection)
	{
		return GetBoundsFromPosition(characterIndex, 1, out flowDirection);
	}

	internal List<Rect> GetRangeBounds(int cp, int cch, double xOffset, double yOffset)
	{
		List<Rect> list = new List<Rect>();
		double num = CalculateXOffsetShift();
		double num2 = xOffset + num;
		IList<TextBounds> textBounds;
		if (_line.HasOverflowed && _owner.ParagraphProperties.TextTrimming != 0)
		{
			Invariant.Assert(DoubleUtil.AreClose(num, 0.0));
			TextLine textLine = _line.Collapse(GetCollapsingProps(_wrappingWidth, _owner.ParagraphProperties));
			Invariant.Assert(textLine.HasCollapsed, "Line has not been collapsed");
			textBounds = textLine.GetTextBounds(cp, cch);
		}
		else
		{
			textBounds = _line.GetTextBounds(cp, cch);
		}
		Invariant.Assert(textBounds.Count > 0);
		for (int i = 0; i < textBounds.Count; i++)
		{
			Rect rectangle = textBounds[i].Rectangle;
			rectangle.X += num2;
			rectangle.Y += yOffset;
			list.Add(rectangle);
		}
		return list;
	}

	internal CharacterHit GetTextPositionFromDistance(double distance)
	{
		double num = CalculateXOffsetShift();
		if (_line.HasOverflowed && _owner.ParagraphProperties.TextTrimming != 0)
		{
			TextLine textLine = _line.Collapse(GetCollapsingProps(_wrappingWidth, _owner.ParagraphProperties));
			Invariant.Assert(DoubleUtil.AreClose(num, 0.0));
			Invariant.Assert(textLine.HasCollapsed, "Line has not been collapsed");
			return textLine.GetCharacterHitFromDistance(distance);
		}
		return _line.GetCharacterHitFromDistance(distance - num);
	}

	internal CharacterHit GetNextCaretCharacterHit(CharacterHit index)
	{
		return _line.GetNextCaretCharacterHit(index);
	}

	internal CharacterHit GetPreviousCaretCharacterHit(CharacterHit index)
	{
		return _line.GetPreviousCaretCharacterHit(index);
	}

	internal CharacterHit GetBackspaceCaretCharacterHit(CharacterHit index)
	{
		return _line.GetBackspaceCaretCharacterHit(index);
	}

	internal bool IsAtCaretCharacterHit(CharacterHit charHit)
	{
		return _line.IsAtCaretCharacterHit(charHit, _dcp);
	}

	internal virtual bool HasInlineObjects()
	{
		return false;
	}

	internal virtual IInputElement InputHitTest(double offset)
	{
		return null;
	}

	internal TextLineBreak GetTextLineBreak()
	{
		if (_line == null)
		{
			return null;
		}
		return _line.GetTextLineBreak();
	}

	internal int GetEllipsesLength()
	{
		if (!_line.HasOverflowed)
		{
			return 0;
		}
		if (_owner.ParagraphProperties.TextTrimming == TextTrimming.None)
		{
			return 0;
		}
		return _line.Collapse(GetCollapsingProps(_wrappingWidth, _owner.ParagraphProperties)).GetTextCollapsedRanges()?[0].Length ?? 0;
	}

	internal double GetCollapsedWidth()
	{
		if (!_line.HasOverflowed)
		{
			return Width;
		}
		if (_owner.ParagraphProperties.TextTrimming == TextTrimming.None)
		{
			return Width;
		}
		return _line.Collapse(GetCollapsingProps(_wrappingWidth, _owner.ParagraphProperties)).Width;
	}

	protected Rect GetBoundsFromPosition(int cp, int cch, out FlowDirection flowDirection)
	{
		double num = CalculateXOffsetShift();
		IList<TextBounds> textBounds;
		if (_line.HasOverflowed && _owner.ParagraphProperties.TextTrimming != 0)
		{
			Invariant.Assert(DoubleUtil.AreClose(num, 0.0));
			TextLine textLine = _line.Collapse(GetCollapsingProps(_wrappingWidth, _owner.ParagraphProperties));
			Invariant.Assert(textLine.HasCollapsed, "Line has not been collapsed");
			textBounds = textLine.GetTextBounds(cp, cch);
		}
		else
		{
			textBounds = _line.GetTextBounds(cp, cch);
		}
		Invariant.Assert(textBounds != null && textBounds.Count == 1, "Expecting exactly one TextBounds for a single text position.");
		Rect result = textBounds[0].TextRunBounds?[0].Rectangle ?? textBounds[0].Rectangle;
		result.X += num;
		flowDirection = textBounds[0].FlowDirection;
		return result;
	}

	protected TextCollapsingProperties GetCollapsingProps(double wrappingWidth, LineProperties paraProperties)
	{
		if (paraProperties.TextTrimming == TextTrimming.CharacterEllipsis)
		{
			return new TextTrailingCharacterEllipsis(wrappingWidth, paraProperties.DefaultTextRunProperties);
		}
		return new TextTrailingWordEllipsis(wrappingWidth, paraProperties.DefaultTextRunProperties);
	}

	protected double CalculateXOffsetShift()
	{
		if (IsXOffsetAdjusted)
		{
			if (_textAlignment == TextAlignment.Center)
			{
				return (_line.Width - _line.WidthIncludingTrailingWhitespace) / 2.0;
			}
			return _line.Width - _line.WidthIncludingTrailingWhitespace;
		}
		return 0.0;
	}
}
