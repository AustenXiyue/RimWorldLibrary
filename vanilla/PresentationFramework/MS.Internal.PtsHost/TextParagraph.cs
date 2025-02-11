using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.TextFormatting;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class TextParagraph : BaseParagraph
{
	private List<AttachedObject> _attachedObjects;

	private List<InlineObject> _inlineObjects;

	private LineProperties _lineProperties;

	private TextRunCache _textRunCache = new TextRunCache();

	private Line _currentLine;

	internal TextRunCache TextRunCache => _textRunCache;

	internal LineProperties Properties
	{
		get
		{
			EnsureLineProperties();
			return _lineProperties;
		}
	}

	internal bool IsOptimalParagraph
	{
		get
		{
			if (base.StructuralCache.IsOptimalParagraphEnabled)
			{
				return GetLineProperties(firstLine: false, 0).TextWrapping != TextWrapping.NoWrap;
			}
			return false;
		}
	}

	internal TextParagraph(DependencyObject element, StructuralCache structuralCache)
		: base(element, structuralCache)
	{
	}

	public override void Dispose()
	{
		if (_attachedObjects != null)
		{
			foreach (AttachedObject attachedObject in _attachedObjects)
			{
				attachedObject.Dispose();
			}
			_attachedObjects = null;
		}
		if (_inlineObjects != null)
		{
			foreach (InlineObject inlineObject in _inlineObjects)
			{
				inlineObject.Dispose();
			}
			_inlineObjects = null;
		}
		base.Dispose();
	}

	internal override void GetParaProperties(ref PTS.FSPAP fspap)
	{
		fspap.fKeepWithNext = 0;
		fspap.fBreakPageBefore = 0;
		fspap.fBreakColumnBefore = 0;
		GetParaProperties(ref fspap, ignoreElementProps: true);
		fspap.idobj = -1;
	}

	internal override void CreateParaclient(out nint paraClientHandle)
	{
		TextParaClient textParaClient = new TextParaClient(this);
		paraClientHandle = textParaClient.Handle;
	}

	internal void GetTextProperties(int iArea, ref PTS.FSTXTPROPS fstxtprops)
	{
		fstxtprops.fswdir = PTS.FlowDirectionToFswdir((FlowDirection)base.Element.GetValue(FrameworkElement.FlowDirectionProperty));
		fstxtprops.dcpStartContent = 0;
		fstxtprops.fKeepTogether = PTS.FromBoolean(DynamicPropertyReader.GetKeepTogether(base.Element));
		fstxtprops.cMinLinesAfterBreak = DynamicPropertyReader.GetMinOrphanLines(base.Element);
		fstxtprops.cMinLinesBeforeBreak = DynamicPropertyReader.GetMinWidowLines(base.Element);
		fstxtprops.fDropCap = 0;
		fstxtprops.fVerticalGrid = 0;
		fstxtprops.fOptimizeParagraph = PTS.FromBoolean(IsOptimalParagraph);
		fstxtprops.fAvoidHyphenationAtTrackBottom = 0;
		fstxtprops.fAvoidHyphenationOnLastChainElement = 0;
		fstxtprops.cMaxConsecutiveHyphens = int.MaxValue;
	}

	internal void CreateOptimalBreakSession(TextParaClient textParaClient, int dcpStart, int durTrack, LineBreakRecord lineBreakRecord, out OptimalBreakSession optimalBreakSession, out bool isParagraphJustified)
	{
		_textRunCache = new TextRunCache();
		TextFormatter textFormatter = base.StructuralCache.TextFormatterHost.TextFormatter;
		TextLineBreak previousLineBreak = lineBreakRecord?.TextLineBreak;
		OptimalTextSource optimalTextSource = new OptimalTextSource(base.StructuralCache.TextFormatterHost, base.ParagraphStartCharacterPosition, durTrack, textParaClient, _textRunCache);
		base.StructuralCache.TextFormatterHost.Context = optimalTextSource;
		TextParagraphCache textParagraphCache = textFormatter.CreateParagraphCache(base.StructuralCache.TextFormatterHost, dcpStart, TextDpi.FromTextDpi(durTrack), GetLineProperties(firstLine: true, dcpStart), previousLineBreak, _textRunCache);
		base.StructuralCache.TextFormatterHost.Context = null;
		optimalBreakSession = new OptimalBreakSession(this, textParaClient, textParagraphCache, optimalTextSource);
		isParagraphJustified = (TextAlignment)base.Element.GetValue(Block.TextAlignmentProperty) == TextAlignment.Justify;
	}

	internal void GetNumberFootnotes(int fsdcpStart, int fsdcpLim, out int nFootnote)
	{
		nFootnote = 0;
	}

	internal void FormatBottomText(int iArea, uint fswdir, Line lastLine, int dvrLine, out nint mcsClient)
	{
		Invariant.Assert(iArea == 0);
		mcsClient = IntPtr.Zero;
	}

	internal bool InterruptFormatting(int dcpCur, int vrCur)
	{
		BackgroundFormatInfo backgroundFormatInfo = base.StructuralCache.BackgroundFormatInfo;
		if (!BackgroundFormatInfo.IsBackgroundFormatEnabled)
		{
			return false;
		}
		if (base.StructuralCache.CurrentFormatContext.FinitePage)
		{
			return false;
		}
		if (vrCur < TextDpi.ToTextDpi(double.IsPositiveInfinity(backgroundFormatInfo.ViewportHeight) ? 500.0 : backgroundFormatInfo.ViewportHeight))
		{
			return false;
		}
		if (backgroundFormatInfo.BackgroundFormatStopTime > DateTime.UtcNow)
		{
			return false;
		}
		if (!backgroundFormatInfo.DoesFinalDTRCoverRestOfText)
		{
			return false;
		}
		if (dcpCur + base.ParagraphStartCharacterPosition <= backgroundFormatInfo.LastCPUninterruptible)
		{
			return false;
		}
		base.StructuralCache.BackgroundFormatInfo.CPInterrupted = dcpCur + base.ParagraphStartCharacterPosition;
		return true;
	}

	internal IList<TextBreakpoint> FormatLineVariants(TextParaClient textParaClient, TextParagraphCache textParagraphCache, OptimalTextSource optimalTextSource, int dcp, TextLineBreak textLineBreak, uint fswdir, int urStartLine, int durLine, bool allowHyphenation, bool clearOnLeft, bool clearOnRight, bool treatAsFirstInPara, bool treatAsLastInPara, bool suppressTopSpace, nint lineVariantRestriction, out int iLineBestVariant)
	{
		base.StructuralCache.TextFormatterHost.Context = optimalTextSource;
		IList<TextBreakpoint> result = textParagraphCache.FormatBreakpoints(dcp, textLineBreak, lineVariantRestriction, TextDpi.FromTextDpi(durLine), out iLineBestVariant);
		base.StructuralCache.TextFormatterHost.Context = null;
		return result;
	}

	internal void ReconstructLineVariant(TextParaClient paraClient, int iArea, int dcp, nint pbrlineIn, int dcpLineIn, uint fswdir, int urStartLine, int durLine, int urStartTrack, int durTrack, int urPageLeftMargin, bool fAllowHyphenation, bool fClearOnLeft, bool fClearOnRight, bool fTreatAsFirstInPara, bool fTreatAsLastInPara, bool fSuppressTopSpace, out nint lineHandle, out int dcpLine, out nint ppbrlineOut, out int fForcedBroken, out PTS.FSFLRES fsflres, out int dvrAscent, out int dvrDescent, out int urBBox, out int durBBox, out int dcpDepend, out int fReformatNeighborsAsLastLine)
	{
		Invariant.Assert(iArea == 0);
		base.StructuralCache.CurrentFormatContext.OnFormatLine();
		Line line = new Line(base.StructuralCache.TextFormatterHost, paraClient, base.ParagraphStartCharacterPosition);
		Line.FormattingContext formattingContext = new Line.FormattingContext(measureMode: true, fClearOnLeft, fClearOnRight, _textRunCache);
		formattingContext.LineFormatLengthTarget = dcpLineIn;
		FormatLineCore(line, pbrlineIn, formattingContext, dcp, durLine, durTrack, fTreatAsFirstInPara, dcp);
		lineHandle = line.Handle;
		dcpLine = line.SafeLength;
		TextLineBreak textLineBreak = line.GetTextLineBreak();
		if (textLineBreak != null)
		{
			LineBreakRecord lineBreakRecord = new LineBreakRecord(base.PtsContext, textLineBreak);
			ppbrlineOut = lineBreakRecord.Handle;
		}
		else
		{
			ppbrlineOut = IntPtr.Zero;
		}
		fForcedBroken = PTS.FromBoolean(line.IsTruncated);
		fsflres = line.FormattingResult;
		dvrAscent = line.Baseline;
		dvrDescent = line.Height - line.Baseline;
		urBBox = urStartLine + line.Start;
		durBBox = line.Width;
		dcpDepend = line.DependantLength;
		fReformatNeighborsAsLastLine = 0;
		CalcLineAscentDescent(dcp, ref dvrAscent, ref dvrDescent);
		int num = base.ParagraphStartCharacterPosition + dcp + line.ActualLength + dcpDepend;
		int symbolCount = base.StructuralCache.TextContainer.SymbolCount;
		if (num > symbolCount)
		{
			num = symbolCount;
		}
		base.StructuralCache.CurrentFormatContext.DependentMax = base.StructuralCache.TextContainer.CreatePointerAtOffset(num, LogicalDirection.Backward);
	}

	internal void FormatLine(TextParaClient paraClient, int iArea, int dcp, nint pbrlineIn, uint fswdir, int urStartLine, int durLine, int urStartTrack, int durTrack, int urPageLeftMargin, bool fAllowHyphenation, bool fClearOnLeft, bool fClearOnRight, bool fTreatAsFirstInPara, bool fTreatAsLastInPara, bool fSuppressTopSpace, out nint lineHandle, out int dcpLine, out nint ppbrlineOut, out int fForcedBroken, out PTS.FSFLRES fsflres, out int dvrAscent, out int dvrDescent, out int urBBox, out int durBBox, out int dcpDepend, out int fReformatNeighborsAsLastLine)
	{
		Invariant.Assert(iArea == 0);
		base.StructuralCache.CurrentFormatContext.OnFormatLine();
		Line line = new Line(base.StructuralCache.TextFormatterHost, paraClient, base.ParagraphStartCharacterPosition);
		Line.FormattingContext ctx = new Line.FormattingContext(measureMode: true, fClearOnLeft, fClearOnRight, _textRunCache);
		FormatLineCore(line, pbrlineIn, ctx, dcp, durLine, durTrack, fTreatAsFirstInPara, dcp);
		lineHandle = line.Handle;
		dcpLine = line.SafeLength;
		TextLineBreak textLineBreak = line.GetTextLineBreak();
		if (textLineBreak != null)
		{
			LineBreakRecord lineBreakRecord = new LineBreakRecord(base.PtsContext, textLineBreak);
			ppbrlineOut = lineBreakRecord.Handle;
		}
		else
		{
			ppbrlineOut = IntPtr.Zero;
		}
		fForcedBroken = PTS.FromBoolean(line.IsTruncated);
		fsflres = line.FormattingResult;
		dvrAscent = line.Baseline;
		dvrDescent = line.Height - line.Baseline;
		urBBox = urStartLine + line.Start;
		durBBox = line.Width;
		dcpDepend = line.DependantLength;
		fReformatNeighborsAsLastLine = 0;
		CalcLineAscentDescent(dcp, ref dvrAscent, ref dvrDescent);
		int num = base.ParagraphStartCharacterPosition + dcp + line.ActualLength + dcpDepend;
		int symbolCount = base.StructuralCache.TextContainer.SymbolCount;
		if (num > symbolCount)
		{
			num = symbolCount;
		}
		base.StructuralCache.CurrentFormatContext.DependentMax = base.StructuralCache.TextContainer.CreatePointerAtOffset(num, LogicalDirection.Backward);
	}

	internal void UpdGetChangeInText(out int dcpStart, out int ddcpOld, out int ddcpNew)
	{
		DtrList dtrList = base.StructuralCache.DtrsFromRange(base.ParagraphStartCharacterPosition, base.LastFormatCch);
		if (dtrList != null)
		{
			dcpStart = dtrList[0].StartIndex - base.ParagraphStartCharacterPosition;
			ddcpNew = dtrList[0].PositionsAdded;
			ddcpOld = dtrList[0].PositionsRemoved;
			if (dtrList.Length > 1)
			{
				for (int i = 1; i < dtrList.Length; i++)
				{
					int num = dtrList[i].StartIndex - dtrList[i - 1].StartIndex;
					ddcpNew += num + dtrList[i].PositionsAdded;
					ddcpOld += num + dtrList[i].PositionsRemoved;
				}
			}
			if (!base.StructuralCache.CurrentFormatContext.FinitePage)
			{
				UpdateEmbeddedObjectsCache(ref _attachedObjects, dcpStart, ddcpOld, ddcpNew - ddcpOld);
				UpdateEmbeddedObjectsCache(ref _inlineObjects, dcpStart, ddcpOld, ddcpNew - ddcpOld);
			}
			Invariant.Assert(dcpStart >= 0 && base.Cch >= dcpStart && base.LastFormatCch >= dcpStart);
			ddcpOld = Math.Min(ddcpOld, base.LastFormatCch - dcpStart + 1);
			ddcpNew = Math.Min(ddcpNew, base.Cch - dcpStart + 1);
		}
		else
		{
			dcpStart = (ddcpOld = (ddcpNew = 0));
		}
	}

	internal void GetDvrAdvance(int dcp, uint fswdir, out int dvr)
	{
		EnsureLineProperties();
		dvr = TextDpi.ToTextDpi(_lineProperties.CalcLineAdvanceForTextParagraph(this, dcp, _lineProperties.DefaultTextRunProperties.FontRenderingEmSize));
	}

	internal int GetLastDcpAttachedObjectBeforeLine(int dcpFirst)
	{
		ITextPointer textPointerFromCP = TextContainerHelper.GetTextPointerFromCP(base.StructuralCache.TextContainer, base.ParagraphStartCharacterPosition + dcpFirst, LogicalDirection.Forward);
		ITextPointer contentStart = TextContainerHelper.GetContentStart(base.StructuralCache.TextContainer, base.Element);
		while (textPointerFromCP.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart)
		{
			TextElement adjacentElementFromOuterPosition = ((TextPointer)textPointerFromCP).GetAdjacentElementFromOuterPosition(LogicalDirection.Forward);
			if (!(adjacentElementFromOuterPosition is Figure) && !(adjacentElementFromOuterPosition is Floater))
			{
				break;
			}
			textPointerFromCP.MoveByOffset(adjacentElementFromOuterPosition.SymbolCount);
		}
		return contentStart.GetOffsetToPosition(textPointerFromCP);
	}

	private List<TextElement> GetAttachedObjectElements(int dcpFirst, int dcpLast)
	{
		List<TextElement> list = new List<TextElement>();
		ITextPointer contentStart = TextContainerHelper.GetContentStart(base.StructuralCache.TextContainer, base.Element);
		ITextPointer textPointerFromCP = TextContainerHelper.GetTextPointerFromCP(base.StructuralCache.TextContainer, base.ParagraphStartCharacterPosition + dcpFirst, LogicalDirection.Forward);
		if (dcpLast > base.Cch)
		{
			dcpLast = base.Cch;
		}
		while (contentStart.GetOffsetToPosition(textPointerFromCP) < dcpLast)
		{
			if (textPointerFromCP.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart)
			{
				TextElement adjacentElementFromOuterPosition = ((TextPointer)textPointerFromCP).GetAdjacentElementFromOuterPosition(LogicalDirection.Forward);
				if (adjacentElementFromOuterPosition is Figure || adjacentElementFromOuterPosition is Floater)
				{
					list.Add(adjacentElementFromOuterPosition);
					textPointerFromCP.MoveByOffset(adjacentElementFromOuterPosition.SymbolCount);
				}
				else
				{
					textPointerFromCP.MoveToNextContextPosition(LogicalDirection.Forward);
				}
			}
			else
			{
				textPointerFromCP.MoveToNextContextPosition(LogicalDirection.Forward);
			}
		}
		return list;
	}

	internal int GetAttachedObjectCount(int dcpFirst, int dcpLast)
	{
		List<TextElement> attachedObjectElements = GetAttachedObjectElements(dcpFirst, dcpLast);
		if (attachedObjectElements.Count == 0)
		{
			SubmitAttachedObjects(dcpFirst, dcpLast, null);
		}
		return attachedObjectElements.Count;
	}

	internal List<AttachedObject> GetAttachedObjects(int dcpFirst, int dcpLast)
	{
		ITextPointer contentStart = TextContainerHelper.GetContentStart(base.StructuralCache.TextContainer, base.Element);
		List<AttachedObject> list = new List<AttachedObject>();
		List<TextElement> attachedObjectElements = GetAttachedObjectElements(dcpFirst, dcpLast);
		for (int i = 0; i < attachedObjectElements.Count; i++)
		{
			TextElement textElement = attachedObjectElements[i];
			if (textElement is Figure && base.StructuralCache.CurrentFormatContext.FinitePage)
			{
				FigureParagraph figureParagraph = new FigureParagraph(textElement, base.StructuralCache);
				if (base.StructuralCache.CurrentFormatContext.IncrementalUpdate)
				{
					figureParagraph.SetUpdateInfo(PTS.FSKCHANGE.fskchNew, stopAsking: false);
				}
				FigureObject item = new FigureObject(contentStart.GetOffsetToPosition(textElement.ElementStart), figureParagraph);
				list.Add(item);
			}
			else
			{
				FloaterParagraph floaterParagraph = new FloaterParagraph(textElement, base.StructuralCache);
				if (base.StructuralCache.CurrentFormatContext.IncrementalUpdate)
				{
					floaterParagraph.SetUpdateInfo(PTS.FSKCHANGE.fskchNew, stopAsking: false);
				}
				FloaterObject item2 = new FloaterObject(contentStart.GetOffsetToPosition(textElement.ElementStart), floaterParagraph);
				list.Add(item2);
			}
		}
		if (list.Count != 0)
		{
			SubmitAttachedObjects(dcpFirst, dcpLast, list);
		}
		return list;
	}

	internal void SubmitInlineObjects(int dcpStart, int dcpLim, List<InlineObject> inlineObjects)
	{
		SubmitEmbeddedObjects(ref _inlineObjects, dcpStart, dcpLim, inlineObjects);
	}

	internal void SubmitAttachedObjects(int dcpStart, int dcpLim, List<AttachedObject> attachedObjects)
	{
		SubmitEmbeddedObjects(ref _attachedObjects, dcpStart, dcpLim, attachedObjects);
	}

	internal List<InlineObject> InlineObjectsFromRange(int dcpStart, int dcpLast)
	{
		List<InlineObject> list = null;
		if (_inlineObjects != null)
		{
			list = new List<InlineObject>(_inlineObjects.Count);
			for (int i = 0; i < _inlineObjects.Count; i++)
			{
				InlineObject inlineObject = _inlineObjects[i];
				if (inlineObject.Dcp >= dcpStart && inlineObject.Dcp < dcpLast)
				{
					list.Add(inlineObject);
				}
				else if (inlineObject.Dcp >= dcpLast)
				{
					break;
				}
			}
		}
		if (list == null || list.Count == 0)
		{
			return null;
		}
		return list;
	}

	internal void CalcLineAscentDescent(int dcp, ref int dvrAscent, ref int dvrDescent)
	{
		EnsureLineProperties();
		int num = dvrAscent + dvrDescent;
		int num2 = TextDpi.ToTextDpi(_lineProperties.CalcLineAdvanceForTextParagraph(this, dcp, TextDpi.FromTextDpi(num)));
		if (num != num2)
		{
			double num3 = 1.0 * (double)num2 / (1.0 * (double)num);
			dvrAscent = (int)((double)dvrAscent * num3);
			dvrDescent = (int)((double)dvrDescent * num3);
		}
	}

	internal override void SetUpdateInfo(PTS.FSKCHANGE fskch, bool stopAsking)
	{
		base.SetUpdateInfo(fskch, stopAsking);
		if (fskch == PTS.FSKCHANGE.fskchInside)
		{
			_textRunCache = new TextRunCache();
			_lineProperties = null;
		}
	}

	internal override void ClearUpdateInfo()
	{
		base.ClearUpdateInfo();
		if (_attachedObjects != null)
		{
			for (int i = 0; i < _attachedObjects.Count; i++)
			{
				_attachedObjects[i].Para.ClearUpdateInfo();
			}
		}
	}

	internal override bool InvalidateStructure(int startPosition)
	{
		Invariant.Assert(base.ParagraphEndCharacterPosition >= startPosition);
		bool result = false;
		if (base.ParagraphStartCharacterPosition == startPosition)
		{
			result = true;
			AnchoredBlock anchoredBlock = null;
			if (_attachedObjects != null && _attachedObjects.Count > 0)
			{
				anchoredBlock = (AnchoredBlock)_attachedObjects[0].Element;
			}
			if (anchoredBlock != null && startPosition == anchoredBlock.ElementStartOffset)
			{
				StaticTextPointer staticTextPointerFromCP = TextContainerHelper.GetStaticTextPointerFromCP(base.StructuralCache.TextContainer, startPosition);
				if (staticTextPointerFromCP.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementStart)
				{
					result = anchoredBlock != staticTextPointerFromCP.GetAdjacentElement(LogicalDirection.Forward);
				}
			}
		}
		InvalidateTextFormatCache();
		if (_attachedObjects != null)
		{
			for (int i = 0; i < _attachedObjects.Count; i++)
			{
				BaseParagraph para = _attachedObjects[i].Para;
				if (para.ParagraphEndCharacterPosition >= startPosition)
				{
					para.InvalidateStructure(startPosition);
				}
			}
		}
		return result;
	}

	internal override void InvalidateFormatCache()
	{
		InvalidateTextFormatCache();
		if (_attachedObjects != null)
		{
			for (int i = 0; i < _attachedObjects.Count; i++)
			{
				_attachedObjects[i].Para.InvalidateFormatCache();
			}
		}
	}

	internal void InvalidateTextFormatCache()
	{
		_textRunCache = new TextRunCache();
		_lineProperties = null;
	}

	internal void FormatLineCore(Line line, nint pbrLineIn, Line.FormattingContext ctx, int dcp, int width, bool firstLine, int dcpLine)
	{
		FormatLineCore(line, pbrLineIn, ctx, dcp, width, -1, firstLine, dcpLine);
	}

	internal void FormatLineCore(Line line, nint pbrLineIn, Line.FormattingContext ctx, int dcp, int width, int trackWidth, bool firstLine, int dcpLine)
	{
		TextDpi.EnsureValidLineWidth(ref width);
		_currentLine = line;
		TextLineBreak textLineBreak = null;
		if (pbrLineIn != IntPtr.Zero)
		{
			LineBreakRecord obj = base.PtsContext.HandleToObject(pbrLineIn) as LineBreakRecord;
			PTS.ValidateHandle(obj);
			textLineBreak = obj.TextLineBreak;
		}
		try
		{
			line.Format(ctx, dcp, width, trackWidth, GetLineProperties(firstLine, dcpLine), textLineBreak);
		}
		finally
		{
			_currentLine = null;
		}
	}

	internal Size MeasureChild(InlineObjectRun inlineObject)
	{
		if (_currentLine == null)
		{
			return ((OptimalTextSource)base.StructuralCache.TextFormatterHost.Context).MeasureChild(inlineObject);
		}
		return _currentLine.MeasureChild(inlineObject);
	}

	internal bool HasFiguresFloatersOrInlineObjects()
	{
		if (HasFiguresOrFloaters() || (_inlineObjects != null && _inlineObjects.Count > 0))
		{
			return true;
		}
		return false;
	}

	internal bool HasFiguresOrFloaters()
	{
		if (_attachedObjects != null)
		{
			return _attachedObjects.Count > 0;
		}
		return false;
	}

	internal void UpdateTextContentRangeFromAttachedObjects(TextContentRange textContentRange, int dcpFirst, int dcpLast, PTS.FSATTACHEDOBJECTDESCRIPTION[] arrayAttachedObjectDesc)
	{
		int num = dcpFirst;
		int num2 = 0;
		while (_attachedObjects != null && num2 < _attachedObjects.Count)
		{
			AttachedObject attachedObject = _attachedObjects[num2];
			int paragraphStartCharacterPosition = attachedObject.Para.ParagraphStartCharacterPosition;
			int cch = attachedObject.Para.Cch;
			if (paragraphStartCharacterPosition >= num && paragraphStartCharacterPosition < dcpLast)
			{
				textContentRange.Merge(new TextContentRange(num, paragraphStartCharacterPosition, base.StructuralCache.TextContainer));
				num = paragraphStartCharacterPosition + cch;
			}
			if (dcpLast < num)
			{
				break;
			}
			num2++;
		}
		if (num < dcpLast)
		{
			textContentRange.Merge(new TextContentRange(num, dcpLast, base.StructuralCache.TextContainer));
		}
		int num3 = 0;
		while (arrayAttachedObjectDesc != null && num3 < arrayAttachedObjectDesc.Length)
		{
			_ = ref arrayAttachedObjectDesc[num3];
			BaseParaClient baseParaClient = base.PtsContext.HandleToObject(arrayAttachedObjectDesc[num3].pfsparaclient) as BaseParaClient;
			PTS.ValidateHandle(baseParaClient);
			textContentRange.Merge(baseParaClient.GetTextContentRange());
			num3++;
		}
	}

	internal void OnUIElementDesiredSizeChanged(object sender, DesiredSizeChangedEventArgs e)
	{
		base.StructuralCache.FormattingOwner.OnChildDesiredSizeChanged(e.Child);
	}

	private void EnsureLineProperties()
	{
		if (_lineProperties == null || (_lineProperties != null && _lineProperties.DefaultTextRunProperties.PixelsPerDip != base.StructuralCache.TextFormatterHost.PixelsPerDip))
		{
			TextProperties defaultTextProperties = new TextProperties(base.Element, StaticTextPointer.Null, inlineObjects: false, getBackground: false, base.StructuralCache.TextFormatterHost.PixelsPerDip);
			_lineProperties = new LineProperties(base.Element, base.StructuralCache.FormattingOwner, defaultTextProperties, null);
			if ((bool)base.Element.GetValue(Block.IsHyphenationEnabledProperty))
			{
				_lineProperties.Hyphenator = base.StructuralCache.Hyphenator;
			}
		}
	}

	private void SubmitEmbeddedObjects<T>(ref List<T> objectsCached, int dcpStart, int dcpLim, List<T> objectsNew) where T : EmbeddedObject
	{
		ErrorHandler.Assert(objectsNew == null || (objectsNew[0].Dcp >= dcpStart && objectsNew[objectsNew.Count - 1].Dcp <= dcpLim), ErrorHandler.SubmitInvalidList);
		if (objectsCached == null)
		{
			if (objectsNew == null)
			{
				return;
			}
			objectsCached = new List<T>(objectsNew.Count);
		}
		int num = objectsCached.Count;
		while (num > 0 && objectsCached[num - 1].Dcp >= dcpLim)
		{
			num--;
		}
		int num2 = num;
		while (num2 > 0 && objectsCached[num2 - 1].Dcp >= dcpStart)
		{
			num2--;
		}
		if (objectsNew == null)
		{
			for (int i = num2; i < num; i++)
			{
				objectsCached[i].Dispose();
			}
			objectsCached.RemoveRange(num2, num - num2);
			return;
		}
		if (num == num2)
		{
			objectsCached.InsertRange(num2, objectsNew);
			return;
		}
		int num3 = 0;
		while (num2 < num)
		{
			T val = objectsCached[num2];
			int j;
			for (j = num3; j < objectsNew.Count; j++)
			{
				T val2 = objectsNew[j];
				if (val.Element == val2.Element)
				{
					if (j > num3)
					{
						objectsCached.InsertRange(num2, objectsNew.GetRange(num3, j - num3));
						num += j - num3;
						num2 += j - num3;
					}
					val.Update(val2);
					objectsNew[j] = val;
					num3 = j + 1;
					num2++;
					val2.Dispose();
					break;
				}
			}
			if (j >= objectsNew.Count)
			{
				objectsCached[num2].Dispose();
				objectsCached.RemoveAt(num2);
				num--;
			}
		}
		if (num3 < objectsNew.Count)
		{
			objectsCached.InsertRange(num, objectsNew.GetRange(num3, objectsNew.Count - num3));
		}
	}

	private void UpdateEmbeddedObjectsCache<T>(ref List<T> objectsCached, int dcpStart, int cchDeleted, int cchDiff) where T : EmbeddedObject
	{
		if (objectsCached == null)
		{
			return;
		}
		int i;
		for (i = 0; i < objectsCached.Count && objectsCached[i].Dcp < dcpStart; i++)
		{
		}
		int j;
		for (j = i; j < objectsCached.Count && objectsCached[j].Dcp < dcpStart + cchDeleted; j++)
		{
		}
		if (i != j)
		{
			for (int k = i; k < j; k++)
			{
				objectsCached[k].Dispose();
			}
			objectsCached.RemoveRange(i, j - i);
		}
		for (; j < objectsCached.Count; j++)
		{
			objectsCached[j].Dcp += cchDiff;
		}
		if (objectsCached.Count == 0)
		{
			objectsCached = null;
		}
	}

	private TextParagraphProperties GetLineProperties(bool firstLine, int dcpLine)
	{
		EnsureLineProperties();
		if (firstLine && _lineProperties.HasFirstLineProperties)
		{
			if (dcpLine != 0)
			{
				firstLine = false;
			}
			else if (TextContainerHelper.GetCPFromElement(base.StructuralCache.TextContainer, base.Element, ElementEdge.AfterStart) < base.ParagraphStartCharacterPosition)
			{
				firstLine = false;
			}
			if (firstLine)
			{
				return _lineProperties.FirstLineProps;
			}
		}
		return _lineProperties;
	}
}
