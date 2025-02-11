using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.TextFormatting;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal abstract class LineBase : UnmanagedHandle
{
	protected readonly BaseParaClient _paraClient;

	protected bool _hasFigures;

	protected bool _hasFloaters;

	protected static int _syntheticCharacterLength = 1;

	protected static int _elementEdgeCharacterLength = 1;

	internal static int SyntheticCharacterLength => _syntheticCharacterLength;

	internal bool HasFigures => _hasFigures;

	internal bool HasFloaters => _hasFloaters;

	internal LineBase(BaseParaClient paraClient)
		: base(paraClient.PtsContext)
	{
		_paraClient = paraClient;
	}

	internal abstract TextRun GetTextRun(int dcp);

	internal abstract TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int dcp);

	internal abstract int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int dcp);

	protected TextRun HandleText(StaticTextPointer position)
	{
		Invariant.Assert(position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text, "TextPointer does not point to characters.");
		DependencyObject target = ((position.Parent == null) ? _paraClient.Paragraph.Element : position.Parent);
		TextProperties textRunProperties = new TextProperties(target, position, inlineObjects: false, getBackground: true, _paraClient.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		StaticTextPointer position2 = position.TextContainer.Highlights.GetNextPropertyChangePosition(position, LogicalDirection.Forward);
		if (position.GetOffsetToPosition(position2) > 4096)
		{
			position2 = position.CreatePointer(4096);
		}
		char[] array = new char[position.GetOffsetToPosition(position2)];
		int textInRun = position.GetTextInRun(LogicalDirection.Forward, array, 0, array.Length);
		return new TextCharacters(array, 0, textInRun, textRunProperties);
	}

	protected TextRun HandleElementStartEdge(StaticTextPointer position)
	{
		Invariant.Assert(position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart, "TextPointer does not point to element start edge.");
		TextRun textRun = null;
		TextElement textElement = (TextElement)position.GetAdjacentElement(LogicalDirection.Forward);
		Invariant.Assert(!(textElement is Block), "We do not expect any Blocks inside Paragraphs");
		if (textElement is Figure || textElement is Floater)
		{
			textRun = new FloatingRun(TextContainerHelper.GetElementLength(_paraClient.Paragraph.StructuralCache.TextContainer, textElement), textElement is Figure);
			if (textElement is Figure)
			{
				_hasFigures = true;
			}
			else
			{
				_hasFloaters = true;
			}
		}
		else if (textElement is LineBreak)
		{
			textRun = new LineBreakRun(TextContainerHelper.GetElementLength(_paraClient.Paragraph.StructuralCache.TextContainer, textElement), PTS.FSFLRES.fsflrSoftBreak);
		}
		else if (textElement.IsEmpty)
		{
			TextProperties textRunProperties = new TextProperties(textElement, position, inlineObjects: false, getBackground: true, _paraClient.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
			char[] array = new char[_elementEdgeCharacterLength * 2];
			Invariant.Assert(_elementEdgeCharacterLength == 1, "Expected value of _elementEdgeCharacterLength is 1");
			array[0] = '\u200b';
			array[1] = '\u200b';
			textRun = new TextCharacters(array, 0, array.Length, textRunProperties);
		}
		else
		{
			Inline inline = (Inline)textElement;
			DependencyObject parent = inline.Parent;
			FlowDirection flowDirection = inline.FlowDirection;
			FlowDirection flowDirection2 = flowDirection;
			TextDecorationCollection textDecorations = DynamicPropertyReader.GetTextDecorations(inline);
			if (parent != null)
			{
				flowDirection2 = (FlowDirection)parent.GetValue(FrameworkElement.FlowDirectionProperty);
			}
			textRun = ((flowDirection != flowDirection2) ? ((textDecorations != null && textDecorations.Count != 0) ? new TextSpanModifier(_elementEdgeCharacterLength, textDecorations, inline.Foreground, flowDirection) : new TextSpanModifier(_elementEdgeCharacterLength, null, null, flowDirection)) : ((textDecorations != null && textDecorations.Count != 0) ? ((TextRun)new TextSpanModifier(_elementEdgeCharacterLength, textDecorations, inline.Foreground)) : ((TextRun)new TextHidden(_elementEdgeCharacterLength))));
		}
		return textRun;
	}

	protected TextRun HandleElementEndEdge(StaticTextPointer position)
	{
		Invariant.Assert(position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd, "TextPointer does not point to element end edge.");
		if (position.Parent == _paraClient.Paragraph.Element)
		{
			return new ParagraphBreakRun(_syntheticCharacterLength, PTS.FSFLRES.fsflrEndOfParagraph);
		}
		Inline inline = (Inline)(TextElement)position.GetAdjacentElement(LogicalDirection.Forward);
		DependencyObject parent = inline.Parent;
		FlowDirection flowDirection = inline.FlowDirection;
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

	protected TextRun HandleEmbeddedObject(int dcp, StaticTextPointer position)
	{
		Invariant.Assert(position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.EmbeddedElement, "TextPointer does not point to embedded object.");
		TextRun textRun = null;
		DependencyObject dependencyObject = position.GetAdjacentElement(LogicalDirection.Forward) as DependencyObject;
		if (dependencyObject is UIElement)
		{
			TextRunProperties textProps = new TextProperties(dependencyObject, position, inlineObjects: true, getBackground: true, _paraClient.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
			return new InlineObjectRun(TextContainerHelper.EmbeddedObjectLength, (UIElement)dependencyObject, textProps, _paraClient.Paragraph as TextParagraph);
		}
		return new TextHidden(TextContainerHelper.EmbeddedObjectLength);
	}
}
