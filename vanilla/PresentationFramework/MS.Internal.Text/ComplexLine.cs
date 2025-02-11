using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using MS.Internal.Documents;
using MS.Internal.PtsHost;

namespace MS.Internal.Text;

internal sealed class ComplexLine : Line
{
	private static int _elementEdgeCharacterLength = 1;

	public override TextRun GetTextRun(int dcp)
	{
		TextRun textRun = null;
		StaticTextPointer position = _owner.TextContainer.CreateStaticPointerAtOffset(dcp);
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
			textRun = HandleInlineObject(position, dcp);
			break;
		case TextPointerContext.None:
			textRun = new TextEndOfParagraph(Line._syntheticCharacterLength);
			break;
		}
		if (textRun.Properties != null)
		{
			textRun.Properties.PixelsPerDip = base.PixelsPerDip;
		}
		return textRun;
	}

	public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int dcp)
	{
		int num = 0;
		CharacterBufferRange characterBufferRange = CharacterBufferRange.Empty;
		CultureInfo culture = null;
		if (dcp > 0)
		{
			ITextPointer textPointer = _owner.TextContainer.CreatePointerAtOffset(dcp, LogicalDirection.Backward);
			while (textPointer.GetPointerContext(LogicalDirection.Backward) != TextPointerContext.Text && textPointer.CompareTo(_owner.TextContainer.Start) != 0)
			{
				textPointer.MoveByOffset(-1);
				num++;
			}
			string textInRun = textPointer.GetTextInRun(LogicalDirection.Backward);
			characterBufferRange = new CharacterBufferRange(textInRun, 0, textInRun.Length);
			StaticTextPointer staticTextPointer = textPointer.CreateStaticPointer();
			culture = DynamicPropertyReader.GetCultureInfo((staticTextPointer.Parent != null) ? staticTextPointer.Parent : _owner);
		}
		return new TextSpan<CultureSpecificCharacterBufferRange>(num + characterBufferRange.Length, new CultureSpecificCharacterBufferRange(culture, characterBufferRange));
	}

	public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
	{
		return textSourceCharacterIndex;
	}

	internal ComplexLine(TextBlock owner)
		: base(owner)
	{
	}

	internal override void Arrange(VisualCollection vc, Vector lineOffset)
	{
		int num = _dcp;
		IList<TextSpan<TextRun>> textRunSpans = _line.GetTextRunSpans();
		_ = lineOffset.X;
		CalculateXOffsetShift();
		foreach (TextSpan<TextRun> item in textRunSpans)
		{
			TextRun value = item.Value;
			if (value is InlineObject)
			{
				InlineObject inlineObject = value as InlineObject;
				if (VisualTreeHelper.GetParent(inlineObject.Element) is Visual visual)
				{
					ContainerVisual obj = visual as ContainerVisual;
					Invariant.Assert(obj != null, "parent should always derives from ContainerVisual");
					obj.Children.Remove(inlineObject.Element);
				}
				FlowDirection flowDirection;
				Rect boundsFromPosition = GetBoundsFromPosition(num, inlineObject.Length, out flowDirection);
				ContainerVisual containerVisual = new ContainerVisual();
				if (inlineObject.Element is FrameworkElement)
				{
					FlowDirection childFD = _owner.FlowDirection;
					DependencyObject parent = ((FrameworkElement)inlineObject.Element).Parent;
					if (parent != null)
					{
						childFD = (FlowDirection)parent.GetValue(FrameworkElement.FlowDirectionProperty);
					}
					PtsHelper.UpdateMirroringTransform(_owner.FlowDirection, childFD, containerVisual, boundsFromPosition.Width);
				}
				vc.Add(containerVisual);
				if (_owner.UseLayoutRounding)
				{
					DpiScale dpi = _owner.GetDpi();
					containerVisual.Offset = new Vector(UIElement.RoundLayoutValue(lineOffset.X + boundsFromPosition.Left, dpi.DpiScaleX), UIElement.RoundLayoutValue(lineOffset.Y + boundsFromPosition.Top, dpi.DpiScaleY));
				}
				else
				{
					containerVisual.Offset = new Vector(lineOffset.X + boundsFromPosition.Left, lineOffset.Y + boundsFromPosition.Top);
				}
				containerVisual.Children.Add(inlineObject.Element);
				inlineObject.Element.Arrange(new Rect(inlineObject.Element.DesiredSize));
			}
			num += item.Length;
		}
	}

	internal override bool HasInlineObjects()
	{
		bool result = false;
		foreach (TextSpan<TextRun> textRunSpan in _line.GetTextRunSpans())
		{
			if (textRunSpan.Value is InlineObject)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	internal override IInputElement InputHitTest(double offset)
	{
		TextPointerContext textPointerContext = TextPointerContext.None;
		DependencyObject dependencyObject = null;
		TextContainer obj = _owner.TextContainer as TextContainer;
		double num = CalculateXOffsetShift();
		if (obj != null)
		{
			CharacterHit characterHitFromDistance;
			if (_line.HasOverflowed && _owner.ParagraphProperties.TextTrimming != 0)
			{
				Invariant.Assert(DoubleUtil.AreClose(num, 0.0));
				TextLine textLine = _line.Collapse(GetCollapsingProps(_wrappingWidth, _owner.ParagraphProperties));
				Invariant.Assert(textLine.HasCollapsed, "Line has not been collapsed");
				characterHitFromDistance = textLine.GetCharacterHitFromDistance(offset);
			}
			else
			{
				characterHitFromDistance = _line.GetCharacterHitFromDistance(offset - num);
			}
			TextPointer textPointer = new TextPointer(_owner.ContentStart, CalcPositionOffset(characterHitFromDistance), LogicalDirection.Forward);
			if (textPointer != null)
			{
				switch ((characterHitFromDistance.TrailingLength != 0) ? textPointer.GetPointerContext(LogicalDirection.Backward) : textPointer.GetPointerContext(LogicalDirection.Forward))
				{
				case TextPointerContext.Text:
				case TextPointerContext.ElementEnd:
					dependencyObject = textPointer.Parent as TextElement;
					break;
				case TextPointerContext.ElementStart:
					dependencyObject = textPointer.GetAdjacentElementFromOuterPosition(LogicalDirection.Forward);
					break;
				}
			}
		}
		return dependencyObject as IInputElement;
	}

	private TextRun HandleText(StaticTextPointer position)
	{
		DependencyObject target = ((position.Parent == null) ? _owner : position.Parent);
		TextRunProperties textRunProperties = new TextProperties(target, position, inlineObjects: false, getBackground: true, base.PixelsPerDip);
		StaticTextPointer position2 = _owner.Highlights.GetNextPropertyChangePosition(position, LogicalDirection.Forward);
		if (position.GetOffsetToPosition(position2) > 4096)
		{
			position2 = position.CreatePointer(4096);
		}
		char[] array = new char[position.GetOffsetToPosition(position2)];
		int textInRun = position.GetTextInRun(LogicalDirection.Forward, array, 0, array.Length);
		return new TextCharacters(array, 0, textInRun, textRunProperties);
	}

	private TextRun HandleElementStartEdge(StaticTextPointer position)
	{
		TextRun textRun = null;
		TextElement textElement = (TextElement)position.GetAdjacentElement(LogicalDirection.Forward);
		if (textElement is LineBreak)
		{
			return new TextEndOfLine(_elementEdgeCharacterLength * 2);
		}
		if (textElement.IsEmpty)
		{
			TextRunProperties textRunProperties = new TextProperties(textElement, position, inlineObjects: false, getBackground: true, base.PixelsPerDip);
			char[] array = new char[_elementEdgeCharacterLength * 2];
			array[0] = '\u200b';
			array[1] = '\u200b';
			return new TextCharacters(array, 0, array.Length, textRunProperties);
		}
		if (!(textElement is Inline { Parent: var parent, FlowDirection: var flowDirection } inline))
		{
			return new TextHidden(_elementEdgeCharacterLength);
		}
		FlowDirection flowDirection2 = flowDirection;
		if (parent != null)
		{
			flowDirection2 = (FlowDirection)parent.GetValue(FrameworkElement.FlowDirectionProperty);
		}
		TextDecorationCollection textDecorations = DynamicPropertyReader.GetTextDecorations(inline);
		if (flowDirection != flowDirection2)
		{
			if (textDecorations == null || textDecorations.Count == 0)
			{
				return new TextSpanModifier(_elementEdgeCharacterLength, null, null, flowDirection);
			}
			return new TextSpanModifier(_elementEdgeCharacterLength, textDecorations, inline.Foreground, flowDirection);
		}
		if (textDecorations == null || textDecorations.Count == 0)
		{
			return new TextHidden(_elementEdgeCharacterLength);
		}
		return new TextSpanModifier(_elementEdgeCharacterLength, textDecorations, inline.Foreground);
	}

	private TextRun HandleElementEndEdge(StaticTextPointer position)
	{
		TextRun textRun = null;
		if (!((TextElement)position.GetAdjacentElement(LogicalDirection.Forward) is Inline { Parent: var parent, FlowDirection: var flowDirection } inline))
		{
			return new TextHidden(_elementEdgeCharacterLength);
		}
		if (parent != null)
		{
			flowDirection = (FlowDirection)parent.GetValue(FrameworkElement.FlowDirectionProperty);
		}
		if (inline.FlowDirection != flowDirection)
		{
			return new TextEndOfSegment(_elementEdgeCharacterLength);
		}
		TextDecorationCollection textDecorations = DynamicPropertyReader.GetTextDecorations(inline);
		if (textDecorations == null || textDecorations.Count == 0)
		{
			return new TextHidden(_elementEdgeCharacterLength);
		}
		return new TextEndOfSegment(_elementEdgeCharacterLength);
	}

	private TextRun HandleInlineObject(StaticTextPointer position, int dcp)
	{
		TextRun textRun = null;
		DependencyObject dependencyObject = position.GetAdjacentElement(LogicalDirection.Forward) as DependencyObject;
		if (dependencyObject is UIElement)
		{
			TextRunProperties textProps = new TextProperties(dependencyObject, position, inlineObjects: true, getBackground: true, base.PixelsPerDip);
			return new InlineObject(dcp, TextContainerHelper.EmbeddedObjectLength, (UIElement)dependencyObject, textProps, _owner);
		}
		return HandleElementEndEdge(position);
	}

	private int CalcPositionOffset(CharacterHit charHit)
	{
		int num = charHit.FirstCharacterIndex + charHit.TrailingLength;
		if (base.EndOfParagraph)
		{
			num = Math.Min(_dcp + base.Length, num);
		}
		return num;
	}
}
