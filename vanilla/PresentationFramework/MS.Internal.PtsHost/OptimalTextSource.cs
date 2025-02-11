using System.Globalization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.TextFormatting;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class OptimalTextSource : LineBase
{
	private readonly TextFormatterHost _host;

	private TextRunCache _runCache;

	private int _durTrack;

	private int _cpPara;

	internal OptimalTextSource(TextFormatterHost host, int cpPara, int durTrack, TextParaClient paraClient, TextRunCache runCache)
		: base(paraClient)
	{
		_host = host;
		_durTrack = durTrack;
		_runCache = runCache;
		_cpPara = cpPara;
	}

	public override void Dispose()
	{
		base.Dispose();
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
		ITextPointer textPointerFromCP = TextContainerHelper.GetTextPointerFromCP(_paraClient.Paragraph.StructuralCache.TextContainer, _cpPara + dcp, LogicalDirection.Forward);
		return textPointerFromCP.TextContainer.Start.GetOffsetToPosition(textPointerFromCP);
	}

	internal PTS.FSFLRES GetFormatResultForBreakpoint(int dcp, TextBreakpoint textBreakpoint)
	{
		int num = 0;
		PTS.FSFLRES result = PTS.FSFLRES.fsflrOutOfSpace;
		foreach (TextSpan<TextRun> textRunSpan in _runCache.GetTextRunSpans())
		{
			TextRun value = textRunSpan.Value;
			if (value != null && num + value.Length >= dcp + textBreakpoint.Length)
			{
				if (value is ParagraphBreakRun)
				{
					result = ((ParagraphBreakRun)value).BreakReason;
				}
				else if (value is LineBreakRun)
				{
					result = ((LineBreakRun)value).BreakReason;
				}
				break;
			}
			num += textRunSpan.Length;
		}
		return result;
	}

	internal Size MeasureChild(InlineObjectRun inlineObject)
	{
		double height = _paraClient.Paragraph.StructuralCache.CurrentFormatContext.DocumentPageSize.Height;
		if (!_paraClient.Paragraph.StructuralCache.CurrentFormatContext.FinitePage)
		{
			height = double.PositiveInfinity;
		}
		return inlineObject.UIElementIsland.DoLayout(new Size(TextDpi.FromTextDpi(_durTrack), height), horizontalAutoSize: true, verticalAutoSize: true);
	}
}
