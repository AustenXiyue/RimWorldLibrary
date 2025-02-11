using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class Line : LineBase
{
	internal class FormattingContext
	{
		internal TextRunCache TextRunCache;

		internal bool MeasureMode;

		internal bool ClearOnLeft;

		internal bool ClearOnRight;

		internal int LineFormatLengthTarget;

		internal FormattingContext(bool measureMode, bool clearOnLeft, bool clearOnRight, TextRunCache textRunCache)
		{
			MeasureMode = measureMode;
			ClearOnLeft = clearOnLeft;
			ClearOnRight = clearOnRight;
			TextRunCache = textRunCache;
			LineFormatLengthTarget = -1;
		}
	}

	private readonly TextFormatterHost _host;

	private readonly int _cpPara;

	private FormattingContext _formattingContext;

	private TextLine _line;

	private IList<TextSpan<TextRun>> _runs;

	private int _dcp;

	private double _wrappingWidth;

	private double _trackWidth = double.NaN;

	private bool _mirror;

	private double _indent;

	private TextAlignment _textAlignment;

	internal int Start => TextDpi.ToTextDpi(_line.Start) + TextDpi.ToTextDpi(_indent) + CalculateUOffsetShift();

	internal int Width
	{
		get
		{
			int num = ((!IsWidthAdjusted) ? (TextDpi.ToTextDpi(_line.Width) - TextDpi.ToTextDpi(_indent)) : (TextDpi.ToTextDpi(_line.WidthIncludingTrailingWhitespace) - TextDpi.ToTextDpi(_indent)));
			Invariant.Assert(num >= 0, "Line width cannot be negative");
			return num;
		}
	}

	internal int Height => TextDpi.ToTextDpi(_line.Height);

	internal int Baseline => TextDpi.ToTextDpi(_line.Baseline);

	internal bool EndOfParagraph
	{
		get
		{
			if (_line.NewlineLength == 0)
			{
				return false;
			}
			return _runs[_runs.Count - 1].Value is ParagraphBreakRun;
		}
	}

	internal int SafeLength => _line.Length;

	internal int ActualLength => _line.Length - (EndOfParagraph ? LineBase._syntheticCharacterLength : 0);

	internal int ContentLength => _line.Length - _line.NewlineLength;

	internal int DependantLength => _line.DependentLength;

	internal bool IsTruncated => _line.IsTruncated;

	internal PTS.FSFLRES FormattingResult
	{
		get
		{
			PTS.FSFLRES result = PTS.FSFLRES.fsflrOutOfSpace;
			if (_line.NewlineLength == 0)
			{
				return result;
			}
			TextRun value = _runs[_runs.Count - 1].Value;
			if (value is ParagraphBreakRun)
			{
				result = ((ParagraphBreakRun)value).BreakReason;
			}
			else if (value is LineBreakRun)
			{
				result = ((LineBreakRun)value).BreakReason;
			}
			return result;
		}
	}

	private bool HasLineBreak => _line.NewlineLength > 0;

	private bool IsUOffsetAdjusted
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

	private bool IsWidthAdjusted
	{
		get
		{
			bool result = false;
			if ((HasLineBreak || EndOfParagraph) && !ShowEllipses)
			{
				result = true;
			}
			return result;
		}
	}

	private bool ShowEllipses
	{
		get
		{
			if (TextParagraph.Properties.TextTrimming == TextTrimming.None)
			{
				return false;
			}
			if (_line.HasOverflowed)
			{
				return true;
			}
			return false;
		}
	}

	private TextParagraph TextParagraph => _paraClient.Paragraph as TextParagraph;

	internal Line(TextFormatterHost host, TextParaClient paraClient, int cpPara)
		: base(paraClient)
	{
		_host = host;
		_cpPara = cpPara;
		_textAlignment = (TextAlignment)TextParagraph.Element.GetValue(Block.TextAlignmentProperty);
		_indent = 0.0;
	}

	public override void Dispose()
	{
		try
		{
			if (_line != null)
			{
				_line.Dispose();
			}
		}
		finally
		{
			_line = null;
			_runs = null;
			_hasFigures = false;
			_hasFloaters = false;
			base.Dispose();
		}
	}

	internal void GetDvrSuppressibleBottomSpace(out int dvrSuppressible)
	{
		dvrSuppressible = Math.Max(0, TextDpi.ToTextDpi(_line.OverhangAfter));
	}

	internal void GetDurFigureAnchor(FigureParagraph paraFigure, uint fswdir, out int dur)
	{
		int firstCharacterIndex = TextContainerHelper.GetCPFromElement(_paraClient.Paragraph.StructuralCache.TextContainer, paraFigure.Element, ElementEdge.BeforeStart) - _cpPara;
		double distanceFromCharacterHit = _line.GetDistanceFromCharacterHit(new CharacterHit(firstCharacterIndex, 0));
		dur = TextDpi.ToTextDpi(distanceFromCharacterHit);
	}

	internal override TextRun GetTextRun(int dcp)
	{
		TextRun textRun = null;
		StaticTextPointer position = ((ITextContainer)_paraClient.Paragraph.StructuralCache.TextContainer).CreateStaticPointerAtOffset(_cpPara + dcp);
		switch (position.GetPointerContext(LogicalDirection.Forward))
		{
		case TextPointerContext.Text:
			textRun = HandleText(position);
			break;
		case TextPointerContext.ElementStart:
			textRun = HandleElementStartEdge(position);
			break;
		case TextPointerContext.ElementEnd:
			textRun = HandleElementEndEdge(position);
			break;
		case TextPointerContext.EmbeddedElement:
			textRun = HandleEmbeddedObject(dcp, position);
			break;
		case TextPointerContext.None:
			textRun = new ParagraphBreakRun(LineBase._syntheticCharacterLength, PTS.FSFLRES.fsflrEndOfParagraph);
			break;
		}
		Invariant.Assert(textRun != null, "TextRun has not been created.");
		Invariant.Assert(textRun.Length > 0, "TextRun has to have positive length.");
		return textRun;
	}

	internal override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int dcp)
	{
		Invariant.Assert(dcp >= 0);
		int num = 0;
		CharacterBufferRange characterBufferRange = CharacterBufferRange.Empty;
		CultureInfo culture = null;
		if (dcp > 0)
		{
			ITextPointer textPointerFromCP = TextContainerHelper.GetTextPointerFromCP(_paraClient.Paragraph.StructuralCache.TextContainer, _cpPara, LogicalDirection.Forward);
			ITextPointer textPointerFromCP2 = TextContainerHelper.GetTextPointerFromCP(_paraClient.Paragraph.StructuralCache.TextContainer, _cpPara + dcp, LogicalDirection.Forward);
			while (textPointerFromCP2.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.Text && textPointerFromCP2.CompareTo(textPointerFromCP) != 0)
			{
				textPointerFromCP2.MoveByOffset(-1);
				num++;
			}
			string textInRun = textPointerFromCP2.GetTextInRun(LogicalDirection.Backward);
			characterBufferRange = new CharacterBufferRange(textInRun, 0, textInRun.Length);
			StaticTextPointer staticTextPointer = textPointerFromCP2.CreateStaticPointer();
			culture = DynamicPropertyReader.GetCultureInfo((staticTextPointer.Parent != null) ? staticTextPointer.Parent : _paraClient.Paragraph.Element);
		}
		return new TextSpan<CultureSpecificCharacterBufferRange>(num + characterBufferRange.Length, new CultureSpecificCharacterBufferRange(culture, characterBufferRange));
	}

	internal override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int dcp)
	{
		return _cpPara + dcp;
	}

	internal void Format(FormattingContext ctx, int dcp, int width, int trackWidth, TextParagraphProperties lineProps, TextLineBreak textLineBreak)
	{
		_formattingContext = ctx;
		_dcp = dcp;
		_host.Context = this;
		_wrappingWidth = TextDpi.FromTextDpi(width);
		_trackWidth = TextDpi.FromTextDpi(trackWidth);
		_mirror = lineProps.FlowDirection == FlowDirection.RightToLeft;
		_indent = lineProps.Indent;
		try
		{
			if (ctx.LineFormatLengthTarget == -1)
			{
				_line = _host.TextFormatter.FormatLine(_host, dcp, _wrappingWidth, lineProps, textLineBreak, ctx.TextRunCache);
			}
			else
			{
				_line = _host.TextFormatter.RecreateLine(_host, dcp, ctx.LineFormatLengthTarget, _wrappingWidth, lineProps, textLineBreak, ctx.TextRunCache);
			}
			_runs = _line.GetTextRunSpans();
			Invariant.Assert(_runs != null, "Cannot retrieve runs collection.");
			if (!_formattingContext.MeasureMode)
			{
				return;
			}
			List<InlineObject> list = new List<InlineObject>(1);
			int num = _dcp;
			foreach (TextSpan<TextRun> run in _runs)
			{
				TextRun value = run.Value;
				if (value is InlineObjectRun)
				{
					list.Add(new InlineObject(num, ((InlineObjectRun)value).UIElementIsland, (TextParagraph)_paraClient.Paragraph));
				}
				else if (value is FloatingRun)
				{
					if (((FloatingRun)value).Figure)
					{
						_hasFigures = true;
					}
					else
					{
						_hasFloaters = true;
					}
				}
				num += run.Length;
			}
			if (list.Count == 0)
			{
				list = null;
			}
			TextParagraph.SubmitInlineObjects(dcp, dcp + ActualLength, list);
		}
		finally
		{
			_host.Context = null;
		}
	}

	internal Size MeasureChild(InlineObjectRun inlineObject)
	{
		if (_formattingContext.MeasureMode)
		{
			double height = _paraClient.Paragraph.StructuralCache.CurrentFormatContext.DocumentPageSize.Height;
			if (!_paraClient.Paragraph.StructuralCache.CurrentFormatContext.FinitePage)
			{
				height = double.PositiveInfinity;
			}
			return inlineObject.UIElementIsland.DoLayout(new Size(_trackWidth, height), horizontalAutoSize: true, verticalAutoSize: true);
		}
		return inlineObject.UIElementIsland.Root.DesiredSize;
	}

	internal ContainerVisual CreateVisual()
	{
		LineVisual lineVisual = new LineVisual();
		_host.Context = this;
		try
		{
			IList<TextSpan<TextRun>> list = _runs;
			TextLine textLine = _line;
			if (_line.HasOverflowed && TextParagraph.Properties.TextTrimming != 0)
			{
				textLine = _line.Collapse(GetCollapsingProps(_wrappingWidth, TextParagraph.Properties));
				Invariant.Assert(textLine.HasCollapsed, "Line has not been collapsed");
				list = textLine.GetTextRunSpans();
			}
			if (HasInlineObjects())
			{
				VisualCollection children = lineVisual.Children;
				FlowDirection parentFD = (FlowDirection)_paraClient.Paragraph.Element.GetValue(FrameworkElement.FlowDirectionProperty);
				int num = _dcp;
				foreach (TextSpan<TextRun> item in list)
				{
					TextRun value = item.Value;
					if (value is InlineObjectRun)
					{
						InlineObjectRun inlineObjectRun = (InlineObjectRun)value;
						FlowDirection flowDirection;
						Rect boundsFromPosition = GetBoundsFromPosition(num, value.Length, out flowDirection);
						if (VisualTreeHelper.GetParent(inlineObjectRun.UIElementIsland) is Visual visual)
						{
							ContainerVisual obj = visual as ContainerVisual;
							Invariant.Assert(obj != null, "Parent should always derives from ContainerVisual.");
							obj.Children.Remove(inlineObjectRun.UIElementIsland);
						}
						if (!textLine.HasCollapsed || boundsFromPosition.Left + inlineObjectRun.UIElementIsland.Root.DesiredSize.Width < textLine.Width)
						{
							if (inlineObjectRun.UIElementIsland.Root is FrameworkElement)
							{
								FlowDirection childFD = (FlowDirection)((FrameworkElement)inlineObjectRun.UIElementIsland.Root).Parent.GetValue(FrameworkElement.FlowDirectionProperty);
								PtsHelper.UpdateMirroringTransform(parentFD, childFD, inlineObjectRun.UIElementIsland, boundsFromPosition.Width);
							}
							children.Add(inlineObjectRun.UIElementIsland);
							inlineObjectRun.UIElementIsland.Offset = new Vector(boundsFromPosition.Left, boundsFromPosition.Top);
						}
					}
					num += item.Length;
				}
			}
			double x = TextDpi.FromTextDpi(CalculateUOffsetShift());
			DrawingContext drawingContext = lineVisual.Open();
			textLine.Draw(drawingContext, new Point(x, 0.0), _mirror ? InvertAxes.Horizontal : InvertAxes.None);
			drawingContext.Close();
			lineVisual.WidthIncludingTrailingWhitespace = textLine.WidthIncludingTrailingWhitespace - _indent;
			return lineVisual;
		}
		finally
		{
			_host.Context = null;
		}
	}

	internal Rect GetBoundsFromTextPosition(int textPosition, out FlowDirection flowDirection)
	{
		return GetBoundsFromPosition(textPosition, 1, out flowDirection);
	}

	internal List<Rect> GetRangeBounds(int cp, int cch, double xOffset, double yOffset)
	{
		List<Rect> list = new List<Rect>();
		double num = TextDpi.FromTextDpi(CalculateUOffsetShift());
		double num2 = xOffset + num;
		IList<TextBounds> textBounds;
		if (_line.HasOverflowed && TextParagraph.Properties.TextTrimming != 0)
		{
			Invariant.Assert(DoubleUtil.AreClose(num, 0.0));
			TextLine textLine = _line.Collapse(GetCollapsingProps(_wrappingWidth, TextParagraph.Properties));
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

	internal TextLineBreak GetTextLineBreak()
	{
		if (_line == null)
		{
			return null;
		}
		return _line.GetTextLineBreak();
	}

	internal CharacterHit GetTextPositionFromDistance(int urDistance)
	{
		int num = CalculateUOffsetShift();
		if (_line.HasOverflowed && TextParagraph.Properties.TextTrimming != 0)
		{
			TextLine textLine = _line.Collapse(GetCollapsingProps(_wrappingWidth, TextParagraph.Properties));
			Invariant.Assert(num == 0);
			Invariant.Assert(textLine.HasCollapsed, "Line has not been collapsed");
			return textLine.GetCharacterHitFromDistance(TextDpi.FromTextDpi(urDistance));
		}
		return _line.GetCharacterHitFromDistance(TextDpi.FromTextDpi(urDistance - num));
	}

	internal IInputElement InputHitTest(int urOffset)
	{
		DependencyObject dependencyObject = null;
		TextPointerContext textPointerContext = TextPointerContext.None;
		int num = CalculateUOffsetShift();
		CharacterHit characterHitFromDistance;
		if (_line.HasOverflowed && TextParagraph.Properties.TextTrimming != 0)
		{
			Invariant.Assert(num == 0);
			TextLine textLine = _line.Collapse(GetCollapsingProps(_wrappingWidth, TextParagraph.Properties));
			Invariant.Assert(textLine.HasCollapsed, "Line has not been collapsed");
			characterHitFromDistance = textLine.GetCharacterHitFromDistance(TextDpi.FromTextDpi(urOffset));
		}
		else
		{
			characterHitFromDistance = _line.GetCharacterHitFromDistance(TextDpi.FromTextDpi(urOffset - num));
		}
		int cp = _paraClient.Paragraph.ParagraphStartCharacterPosition + characterHitFromDistance.FirstCharacterIndex + characterHitFromDistance.TrailingLength;
		if (TextContainerHelper.GetTextPointerFromCP(_paraClient.Paragraph.StructuralCache.TextContainer, cp, LogicalDirection.Forward) is TextPointer textPointer)
		{
			switch (textPointer.GetPointerContext((characterHitFromDistance.TrailingLength == 0) ? LogicalDirection.Forward : LogicalDirection.Backward))
			{
			case TextPointerContext.Text:
			case TextPointerContext.ElementEnd:
				dependencyObject = textPointer.Parent;
				break;
			case TextPointerContext.ElementStart:
				dependencyObject = textPointer.GetAdjacentElementFromOuterPosition(LogicalDirection.Forward);
				break;
			}
		}
		return dependencyObject as IInputElement;
	}

	internal int GetEllipsesLength()
	{
		if (!_line.HasOverflowed)
		{
			return 0;
		}
		if (TextParagraph.Properties.TextTrimming == TextTrimming.None)
		{
			return 0;
		}
		TextLine textLine = _line.Collapse(GetCollapsingProps(_wrappingWidth, TextParagraph.Properties));
		Invariant.Assert(textLine.HasCollapsed, "Line has not been collapsed");
		IList<TextCollapsedRange> textCollapsedRanges = textLine.GetTextCollapsedRanges();
		if (textCollapsedRanges != null)
		{
			Invariant.Assert(textCollapsedRanges.Count == 1, "Multiple collapsed ranges are not supported.");
			return textCollapsedRanges[0].Length;
		}
		return 0;
	}

	internal void GetGlyphRuns(List<GlyphRun> glyphRuns, int dcpStart, int dcpEnd)
	{
		int num = dcpStart - _dcp;
		int num2 = dcpEnd - dcpStart;
		IList<TextSpan<TextRun>> textRunSpans = _line.GetTextRunSpans();
		DrawingGroup drawingGroup = new DrawingGroup();
		DrawingContext drawingContext = drawingGroup.Open();
		double x = TextDpi.FromTextDpi(CalculateUOffsetShift());
		_line.Draw(drawingContext, new Point(x, 0.0), InvertAxes.None);
		drawingContext.Close();
		int cchGlyphRuns = 0;
		ArrayList arrayList = new ArrayList(4);
		AddGlyphRunRecursive(drawingGroup, arrayList, ref cchGlyphRuns);
		int num3 = 0;
		foreach (TextSpan<TextRun> item in textRunSpans)
		{
			if (item.Value is TextCharacters)
			{
				num3 += item.Length;
			}
		}
		while (cchGlyphRuns > num3)
		{
			GlyphRun glyphRun = (GlyphRun)arrayList[0];
			cchGlyphRuns -= ((glyphRun.Characters != null) ? glyphRun.Characters.Count : 0);
			arrayList.RemoveAt(0);
		}
		int num4 = 0;
		int num5 = 0;
		foreach (TextSpan<TextRun> item2 in textRunSpans)
		{
			if (item2.Value is TextCharacters)
			{
				int num6 = 0;
				while (num6 < item2.Length)
				{
					Invariant.Assert(num5 < arrayList.Count);
					GlyphRun glyphRun2 = (GlyphRun)arrayList[num5];
					int num7 = ((glyphRun2.Characters != null) ? glyphRun2.Characters.Count : 0);
					if (num < num4 + num7 && num + num2 > num4)
					{
						glyphRuns.Add(glyphRun2);
					}
					num6 += num7;
					num5++;
				}
				Invariant.Assert(num6 == item2.Length);
				if (num + num2 <= num4 + item2.Length)
				{
					break;
				}
			}
			num4 += item2.Length;
		}
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

	private bool HasInlineObjects()
	{
		bool result = false;
		foreach (TextSpan<TextRun> run in _runs)
		{
			if (run.Value is InlineObjectRun)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private Rect GetBoundsFromPosition(int cp, int cch, out FlowDirection flowDirection)
	{
		double num = TextDpi.FromTextDpi(CalculateUOffsetShift());
		IList<TextBounds> textBounds;
		if (_line.HasOverflowed && TextParagraph.Properties.TextTrimming != 0)
		{
			Invariant.Assert(DoubleUtil.AreClose(num, 0.0));
			TextLine textLine = _line.Collapse(GetCollapsingProps(_wrappingWidth, TextParagraph.Properties));
			Invariant.Assert(textLine.HasCollapsed, "Line has not been collapsed");
			textBounds = textLine.GetTextBounds(cp, cch);
		}
		else
		{
			textBounds = _line.GetTextBounds(cp, cch);
		}
		Invariant.Assert(textBounds != null && textBounds.Count == 1, "Expecting exactly one TextBounds for a single text position.");
		Rect result = textBounds[0].TextRunBounds?[0].Rectangle ?? textBounds[0].Rectangle;
		flowDirection = textBounds[0].FlowDirection;
		result.X += num;
		return result;
	}

	private TextCollapsingProperties GetCollapsingProps(double wrappingWidth, LineProperties paraProperties)
	{
		Invariant.Assert(paraProperties.TextTrimming != TextTrimming.None, "Text trimming must be enabled.");
		if (paraProperties.TextTrimming == TextTrimming.CharacterEllipsis)
		{
			return new TextTrailingCharacterEllipsis(wrappingWidth, paraProperties.DefaultTextRunProperties);
		}
		return new TextTrailingWordEllipsis(wrappingWidth, paraProperties.DefaultTextRunProperties);
	}

	private void AddGlyphRunRecursive(Drawing drawing, IList glyphRunsCollection, ref int cchGlyphRuns)
	{
		if (drawing is DrawingGroup drawingGroup)
		{
			{
				foreach (Drawing child in drawingGroup.Children)
				{
					AddGlyphRunRecursive(child, glyphRunsCollection, ref cchGlyphRuns);
				}
				return;
			}
		}
		if (drawing is GlyphRunDrawing { GlyphRun: { } glyphRun })
		{
			cchGlyphRuns += ((glyphRun.Characters != null) ? glyphRun.Characters.Count : 0);
			glyphRunsCollection.Add(glyphRun);
		}
	}

	internal int CalculateUOffsetShift()
	{
		int num = 0;
		int num2;
		if (IsUOffsetAdjusted)
		{
			num2 = TextDpi.ToTextDpi(_line.WidthIncludingTrailingWhitespace);
			num = TextDpi.ToTextDpi(_line.Width) - num2;
			Invariant.Assert(num <= 0);
		}
		else
		{
			num2 = TextDpi.ToTextDpi(_line.Width);
			num = 0;
		}
		int num3 = 0;
		if ((_textAlignment == TextAlignment.Center || _textAlignment == TextAlignment.Right) && !ShowEllipses)
		{
			num3 = ((num2 > TextDpi.ToTextDpi(_wrappingWidth)) ? (num2 - TextDpi.ToTextDpi(_wrappingWidth)) : 0);
		}
		if (_textAlignment == TextAlignment.Center)
		{
			return (num3 + num) / 2;
		}
		return num3 + num;
	}
}
