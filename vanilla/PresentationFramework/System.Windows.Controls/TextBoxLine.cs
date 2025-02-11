using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal;
using MS.Internal.Text;

namespace System.Windows.Controls;

internal class TextBoxLine : TextSource, IDisposable
{
	private readonly TextBoxView _owner;

	private TextLine _line;

	private int _dcp;

	private LineProperties _lineProperties;

	private TextProperties _spellerErrorProperties;

	private double _paragraphWidth;

	private const int _syntheticCharacterLength = 1;

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

	internal double Height => _line.Height;

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

	internal int Length => _line.Length - (EndOfParagraph ? 1 : 0);

	internal int ContentLength => _line.Length - _line.NewlineLength;

	internal bool HasLineBreak => _line.NewlineLength > 0;

	private bool IsXOffsetAdjusted
	{
		get
		{
			if (_lineProperties.TextAlignmentInternal == TextAlignment.Right || _lineProperties.TextAlignmentInternal == TextAlignment.Center)
			{
				return IsWidthAdjusted;
			}
			return false;
		}
	}

	private bool IsWidthAdjusted
	{
		get
		{
			if (!HasLineBreak)
			{
				return EndOfParagraph;
			}
			return true;
		}
	}

	internal TextBoxLine(TextBoxView owner)
	{
		_owner = owner;
		base.PixelsPerDip = _owner.GetDpi().PixelsPerDip;
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

	public override TextRun GetTextRun(int dcp)
	{
		TextRun textRun = null;
		StaticTextPointer position = _owner.Host.TextContainer.CreateStaticPointerAtOffset(dcp);
		switch (position.GetPointerContext(LogicalDirection.Forward))
		{
		case TextPointerContext.Text:
			textRun = HandleText(position);
			break;
		case TextPointerContext.None:
			textRun = new TextEndOfParagraph(1);
			break;
		default:
			Invariant.Assert(condition: false, "Unsupported position type.");
			break;
		}
		Invariant.Assert(textRun != null, "TextRun has not been created.");
		Invariant.Assert(textRun.Length > 0, "TextRun has to have positive length.");
		if (textRun.Properties != null)
		{
			textRun.Properties.PixelsPerDip = base.PixelsPerDip;
		}
		return textRun;
	}

	public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int dcp)
	{
		CharacterBufferRange characterBufferRange = CharacterBufferRange.Empty;
		CultureInfo culture = null;
		if (dcp > 0)
		{
			ITextPointer textPointer = _owner.Host.TextContainer.CreatePointerAtOffset(dcp, LogicalDirection.Backward);
			int num = Math.Min(128, textPointer.GetTextRunLength(LogicalDirection.Backward));
			char[] array = new char[num];
			textPointer.GetTextInRun(LogicalDirection.Backward, array, 0, num);
			characterBufferRange = new CharacterBufferRange(array, 0, num);
			culture = DynamicPropertyReader.GetCultureInfo((Control)_owner.Host);
		}
		return new TextSpan<CultureSpecificCharacterBufferRange>(characterBufferRange.Length, new CultureSpecificCharacterBufferRange(culture, characterBufferRange));
	}

	public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
	{
		return textSourceCharacterIndex;
	}

	internal void Format(int dcp, double formatWidth, double paragraphWidth, LineProperties lineProperties, TextRunCache textRunCache, TextFormatter formatter)
	{
		_lineProperties = lineProperties;
		_dcp = dcp;
		_paragraphWidth = paragraphWidth;
		lineProperties.IgnoreTextAlignment = lineProperties.TextAlignment != TextAlignment.Justify;
		try
		{
			_line = formatter.FormatLine(this, dcp, formatWidth, lineProperties, null, textRunCache);
		}
		finally
		{
			lineProperties.IgnoreTextAlignment = false;
		}
	}

	internal TextBoxLineDrawingVisual CreateVisual(Geometry selectionGeometry)
	{
		TextBoxLineDrawingVisual textBoxLineDrawingVisual = new TextBoxLineDrawingVisual();
		double x = CalculateXOffsetShift();
		DrawingContext drawingContext = textBoxLineDrawingVisual.RenderOpen();
		if (selectionGeometry != null)
		{
			FrameworkElement frameworkElement = _owner?.Host?.TextContainer?.TextSelection?.TextEditor?.UiScope;
			if (frameworkElement != null && frameworkElement.GetValue(TextBoxBase.SelectionBrushProperty) is Brush brush)
			{
				double opacity = (double)frameworkElement.GetValue(TextBoxBase.SelectionOpacityProperty);
				drawingContext.PushOpacity(opacity);
				drawingContext.DrawGeometry(brush, new Pen
				{
					Brush = brush
				}, selectionGeometry);
				drawingContext.Pop();
			}
		}
		_line.Draw(drawingContext, new Point(x, 0.0), (_lineProperties.FlowDirection == FlowDirection.RightToLeft) ? InvertAxes.Horizontal : InvertAxes.None);
		drawingContext.Close();
		return textBoxLineDrawingVisual;
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
		IList<TextBounds> textBounds = _line.GetTextBounds(cp, cch);
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

	private TextRun HandleText(StaticTextPointer position)
	{
		StaticTextPointer position2 = _owner.Host.TextContainer.Highlights.GetNextPropertyChangePosition(position, LogicalDirection.Forward);
		if (position.GetOffsetToPosition(position2) > 4096)
		{
			position2 = position.CreatePointer(4096);
		}
		Highlights highlights = position.TextContainer.Highlights;
		TextDecorationCollection textDecorationCollection = highlights.GetHighlightValue(position, LogicalDirection.Forward, typeof(SpellerHighlightLayer)) as TextDecorationCollection;
		TextRunProperties textRunProperties = _lineProperties.DefaultTextRunProperties;
		if (textDecorationCollection != null)
		{
			if (_spellerErrorProperties == null)
			{
				_spellerErrorProperties = new TextProperties((TextProperties)textRunProperties, textDecorationCollection);
			}
			textRunProperties = _spellerErrorProperties;
		}
		TextEditor textEditor = position.TextContainer.TextSelection?.TextEditor;
		if (textEditor != null && textEditor.TextView?.RendersOwnSelection == true && highlights.GetHighlightValue(position, LogicalDirection.Forward, typeof(TextSelection)) != DependencyProperty.UnsetValue)
		{
			TextProperties textProperties = new TextProperties((TextProperties)textRunProperties, textDecorationCollection);
			FrameworkElement frameworkElement = textEditor?.UiScope;
			if (frameworkElement != null)
			{
				textProperties.SetBackgroundBrush(null);
				if (frameworkElement.GetValue(TextBoxBase.SelectionTextBrushProperty) is Brush foregroundBrush)
				{
					textProperties.SetForegroundBrush(foregroundBrush);
				}
			}
			textRunProperties = textProperties;
		}
		char[] array = new char[position.GetOffsetToPosition(position2)];
		int textInRun = position.GetTextInRun(LogicalDirection.Forward, array, 0, array.Length);
		Invariant.Assert(textInRun == array.Length);
		return new TextCharacters(array, 0, textInRun, textRunProperties);
	}

	private Rect GetBoundsFromPosition(int cp, int cch, out FlowDirection flowDirection)
	{
		double num = CalculateXOffsetShift();
		IList<TextBounds> textBounds = _line.GetTextBounds(cp, cch);
		Invariant.Assert(textBounds != null && textBounds.Count == 1, "Expecting exactly one TextBounds for a single text position.");
		IList<TextRunBounds> textRunBounds = textBounds[0].TextRunBounds;
		Rect rectangle;
		if (textRunBounds != null)
		{
			Invariant.Assert(textRunBounds.Count == 1, "Expecting exactly one TextRunBounds for a single text position.");
			rectangle = textRunBounds[0].Rectangle;
		}
		else
		{
			rectangle = textBounds[0].Rectangle;
		}
		rectangle.X += num;
		flowDirection = textBounds[0].FlowDirection;
		return rectangle;
	}

	private double CalculateXOffsetShift()
	{
		double num = 0.0;
		if (_lineProperties.TextAlignmentInternal == TextAlignment.Right)
		{
			num = _paragraphWidth - _line.Width;
		}
		else if (_lineProperties.TextAlignmentInternal == TextAlignment.Center)
		{
			num = (_paragraphWidth - _line.Width) / 2.0;
		}
		if (IsXOffsetAdjusted)
		{
			num = ((_lineProperties.TextAlignmentInternal != TextAlignment.Center) ? (num + (_line.Width - _line.WidthIncludingTrailingWhitespace)) : (num + (_line.Width - _line.WidthIncludingTrailingWhitespace) / 2.0));
		}
		return num;
	}
}
