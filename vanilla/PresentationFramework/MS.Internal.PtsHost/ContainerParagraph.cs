using System;
using System.Windows;
using System.Windows.Documents;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;

namespace MS.Internal.PtsHost;

internal class ContainerParagraph : BaseParagraph, ISegment
{
	private BaseParagraph _firstChild;

	private BaseParagraph _lastFetchedChild;

	private UpdateRecord _ur;

	private bool _firstParaValidInUpdateMode;

	internal ContainerParagraph(DependencyObject element, StructuralCache structuralCache)
		: base(element, structuralCache)
	{
	}

	public override void Dispose()
	{
		BaseParagraph baseParagraph = _firstChild;
		while (baseParagraph != null)
		{
			BaseParagraph baseParagraph2 = baseParagraph;
			baseParagraph = baseParagraph.Next;
			baseParagraph2.Dispose();
			baseParagraph2.Next = null;
			baseParagraph2.Previous = null;
		}
		_firstChild = (_lastFetchedChild = null);
		base.Dispose();
		GC.SuppressFinalize(this);
	}

	void ISegment.GetFirstPara(out int fSuccessful, out nint firstParaName)
	{
		if (_ur != null)
		{
			int cPFromElement = TextContainerHelper.GetCPFromElement(base.StructuralCache.TextContainer, base.Element, ElementEdge.AfterStart);
			if (_ur.SyncPara != null && cPFromElement == _ur.SyncPara.ParagraphStartCharacterPosition)
			{
				_ur.SyncPara.Previous = null;
				if (_ur.Next != null && _ur.Next.FirstPara == _ur.SyncPara)
				{
					_ur.SyncPara.SetUpdateInfo(_ur.Next.ChangeType, stopAsking: false);
				}
				else
				{
					_ur.SyncPara.SetUpdateInfo(PTS.FSKCHANGE.fskchNone, _ur.Next == null);
				}
				Invariant.Assert(_firstChild == null);
				_firstChild = _ur.SyncPara;
				_ur = _ur.Next;
			}
		}
		if (_firstChild != null)
		{
			if (base.StructuralCache.CurrentFormatContext.IncrementalUpdate && _ur == null && NeedsUpdate() && !_firstParaValidInUpdateMode)
			{
				if (!base.StructuralCache.CurrentFormatContext.FinitePage)
				{
					for (BaseParagraph baseParagraph = _firstChild; baseParagraph != null; baseParagraph = baseParagraph.Next)
					{
						baseParagraph.Dispose();
					}
					_firstChild = null;
				}
				_firstParaValidInUpdateMode = true;
			}
			else if (_ur != null && _ur.InProcessing && _ur.FirstPara == _firstChild)
			{
				_firstChild.SetUpdateInfo(PTS.FSKCHANGE.fskchInside, stopAsking: false);
			}
		}
		if (_firstChild == null)
		{
			ITextPointer contentStart = TextContainerHelper.GetContentStart(base.StructuralCache.TextContainer, base.Element);
			_firstChild = GetParagraph(contentStart, fEmptyOk: false);
			if (_ur != null && _firstChild != null)
			{
				_firstChild.SetUpdateInfo(PTS.FSKCHANGE.fskchNew, stopAsking: false);
			}
		}
		if (base.StructuralCache.CurrentFormatContext.IncrementalUpdate)
		{
			_firstParaValidInUpdateMode = true;
		}
		_lastFetchedChild = _firstChild;
		fSuccessful = PTS.FromBoolean(_firstChild != null);
		firstParaName = ((_firstChild != null) ? _firstChild.Handle : IntPtr.Zero);
	}

	void ISegment.GetNextPara(BaseParagraph prevParagraph, out int fFound, out nint nextParaName)
	{
		if (_ur != null)
		{
			int paragraphEndCharacterPosition = prevParagraph.ParagraphEndCharacterPosition;
			if (_ur.SyncPara != null && paragraphEndCharacterPosition == _ur.SyncPara.ParagraphStartCharacterPosition)
			{
				_ur.SyncPara.Previous = prevParagraph;
				prevParagraph.Next = _ur.SyncPara;
				if (_ur.Next != null && _ur.Next.FirstPara == _ur.SyncPara)
				{
					_ur.SyncPara.SetUpdateInfo(_ur.Next.ChangeType, stopAsking: false);
				}
				else
				{
					_ur.SyncPara.SetUpdateInfo(PTS.FSKCHANGE.fskchNone, _ur.Next == null);
				}
				_ur = _ur.Next;
			}
			else
			{
				Invariant.Assert(_ur.SyncPara == null || paragraphEndCharacterPosition < _ur.SyncPara.ParagraphStartCharacterPosition);
				if (!_ur.InProcessing && _ur.FirstPara != prevParagraph.Next && prevParagraph.Next != null)
				{
					prevParagraph.Next.SetUpdateInfo(PTS.FSKCHANGE.fskchNone, stopAsking: false);
				}
				else if (_ur.FirstPara != null && _ur.FirstPara == prevParagraph.Next)
				{
					_ur.InProcessing = true;
					prevParagraph.Next.SetUpdateInfo(PTS.FSKCHANGE.fskchInside, stopAsking: false);
				}
			}
		}
		BaseParagraph baseParagraph = prevParagraph.Next;
		if (baseParagraph == null)
		{
			ITextPointer textPointerFromCP = TextContainerHelper.GetTextPointerFromCP(base.StructuralCache.TextContainer, prevParagraph.ParagraphEndCharacterPosition, LogicalDirection.Forward);
			baseParagraph = GetParagraph(textPointerFromCP, fEmptyOk: true);
			if (baseParagraph != null)
			{
				baseParagraph.Previous = prevParagraph;
				prevParagraph.Next = baseParagraph;
				if (_changeType == PTS.FSKCHANGE.fskchInside)
				{
					baseParagraph.SetUpdateInfo(PTS.FSKCHANGE.fskchNew, stopAsking: false);
				}
			}
		}
		if (baseParagraph != null)
		{
			fFound = 1;
			nextParaName = baseParagraph.Handle;
			_lastFetchedChild = baseParagraph;
		}
		else
		{
			fFound = 0;
			nextParaName = IntPtr.Zero;
			_lastFetchedChild = prevParagraph;
			_ur = null;
		}
	}

	void ISegment.UpdGetFirstChangeInSegment(out int fFound, out int fChangeFirst, out nint nmpBeforeChange)
	{
		BuildUpdateRecord();
		fFound = PTS.FromBoolean(_ur != null);
		fChangeFirst = PTS.FromBoolean(_ur != null && (_firstChild == null || _firstChild == _ur.FirstPara));
		if (PTS.ToBoolean(fFound) && !PTS.ToBoolean(fChangeFirst))
		{
			if (_ur.FirstPara == null)
			{
				BaseParagraph baseParagraph = _lastFetchedChild;
				while (baseParagraph.Next != null)
				{
					baseParagraph = baseParagraph.Next;
				}
				nmpBeforeChange = baseParagraph.Handle;
			}
			else
			{
				if (_ur.ChangeType == PTS.FSKCHANGE.fskchNew)
				{
					_ur.FirstPara.Previous.Next = null;
				}
				nmpBeforeChange = _ur.FirstPara.Previous.Handle;
			}
		}
		else
		{
			nmpBeforeChange = IntPtr.Zero;
		}
		if (PTS.ToBoolean(fFound))
		{
			_ur.InProcessing = PTS.ToBoolean(fChangeFirst);
			_changeType = PTS.FSKCHANGE.fskchInside;
			_stopAsking = false;
		}
	}

	internal void UpdGetSegmentChange(out PTS.FSKCHANGE fskch)
	{
		if (base.StructuralCache.CurrentFormatContext.FinitePage)
		{
			DtrList dtrList = base.StructuralCache.DtrsFromRange(TextContainerHelper.GetCPFromElement(base.StructuralCache.TextContainer, base.Element, ElementEdge.BeforeStart), base.LastFormatCch);
			if (dtrList != null)
			{
				int cPFromElement = TextContainerHelper.GetCPFromElement(base.StructuralCache.TextContainer, base.Element, ElementEdge.AfterStart);
				DirtyTextRange dirtyTextRange = dtrList[0];
				int num = cPFromElement;
				BaseParagraph baseParagraph = _firstChild;
				if (num < dirtyTextRange.StartIndex)
				{
					while (baseParagraph != null && num + baseParagraph.LastFormatCch <= dirtyTextRange.StartIndex && (num + baseParagraph.LastFormatCch != dirtyTextRange.StartIndex || !(baseParagraph is TextParagraph)))
					{
						num += baseParagraph.Cch;
						baseParagraph = baseParagraph.Next;
					}
					baseParagraph?.SetUpdateInfo(PTS.FSKCHANGE.fskchInside, stopAsking: false);
				}
				else
				{
					baseParagraph.SetUpdateInfo(PTS.FSKCHANGE.fskchNew, stopAsking: false);
				}
				if (baseParagraph != null)
				{
					for (baseParagraph = baseParagraph.Next; baseParagraph != null; baseParagraph = baseParagraph.Next)
					{
						baseParagraph.SetUpdateInfo(PTS.FSKCHANGE.fskchNew, stopAsking: false);
					}
				}
				_changeType = PTS.FSKCHANGE.fskchInside;
			}
		}
		fskch = _changeType;
	}

	internal override void GetParaProperties(ref PTS.FSPAP fspap)
	{
		GetParaProperties(ref fspap, ignoreElementProps: false);
		fspap.idobj = PtsHost.ContainerParagraphId;
	}

	internal override void CreateParaclient(out nint paraClientHandle)
	{
		ContainerParaClient containerParaClient = new ContainerParaClient(this);
		paraClientHandle = containerParaClient.Handle;
	}

	internal void FormatParaFinite(ContainerParaClient paraClient, nint pbrkrecIn, int fBRFromPreviousPage, int iArea, nint footnoteRejector, nint geometry, int fEmptyOk, int fSuppressTopSpace, uint fswdir, ref PTS.FSRECT fsrcToFill, MarginCollapsingState mcs, PTS.FSKCLEAR fskclearIn, PTS.FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstparaIn, out PTS.FSFMTR fsfmtr, out nint pfspara, out nint pbrkrecOut, out int dvrUsed, out PTS.FSBBOX fsbbox, out nint pmcsclientOut, out PTS.FSKCLEAR fskclearOut, out int dvrTopSpace)
	{
		uint num = PTS.FlowDirectionToFswdir((FlowDirection)base.Element.GetValue(FrameworkElement.FlowDirectionProperty));
		if (mcs != null && pbrkrecIn != IntPtr.Zero)
		{
			mcs = null;
		}
		int margin = 0;
		int margin2 = 0;
		MarginCollapsingState mcsNew = null;
		Invariant.Assert(base.Element is Block || base.Element is ListItem);
		fskclearIn = PTS.WrapDirectionToFskclear((WrapDirection)base.Element.GetValue(Block.ClearFloatersProperty));
		PTS.FSRECT rectTransform = fsrcToFill;
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (num != fswdir)
		{
			PTS.FSRECT rectPage = base.StructuralCache.CurrentFormatContext.PageRect;
			PTS.Validate(PTS.FsTransformRectangle(fswdir, ref rectPage, ref rectTransform, num, out rectTransform));
			PTS.Validate(PTS.FsTransformRectangle(fswdir, ref rectPage, ref fsrcToFill, num, out fsrcToFill));
			mbpInfo.MirrorMargin();
		}
		rectTransform.u += mbpInfo.MBPLeft;
		rectTransform.du -= mbpInfo.MBPLeft + mbpInfo.MBPRight;
		rectTransform.u = Math.Max(Math.Min(rectTransform.u, fsrcToFill.u + fsrcToFill.du - 1), fsrcToFill.u);
		rectTransform.du = Math.Max(rectTransform.du, 0);
		if (pbrkrecIn == IntPtr.Zero)
		{
			MarginCollapsingState.CollapseTopMargin(base.PtsContext, mbpInfo, mcs, out mcsNew, out margin);
			if (PTS.ToBoolean(fSuppressTopSpace))
			{
				margin = 0;
			}
			rectTransform.v += margin + mbpInfo.BPTop;
			rectTransform.dv -= margin + mbpInfo.BPTop;
			rectTransform.v = Math.Max(Math.Min(rectTransform.v, fsrcToFill.v + fsrcToFill.dv - 1), fsrcToFill.v);
			rectTransform.dv = Math.Max(rectTransform.dv, 0);
		}
		int pTopSpace = 0;
		try
		{
			PTS.Validate(PTS.FsFormatSubtrackFinite(base.PtsContext.Context, pbrkrecIn, fBRFromPreviousPage, base.Handle, iArea, footnoteRejector, geometry, fEmptyOk, fSuppressTopSpace, num, ref rectTransform, mcsNew?.Handle ?? IntPtr.Zero, fskclearIn, fsksuppresshardbreakbeforefirstparaIn, out fsfmtr, out pfspara, out pbrkrecOut, out dvrUsed, out fsbbox, out pmcsclientOut, out fskclearOut, out pTopSpace), base.PtsContext);
		}
		finally
		{
			if (mcsNew != null)
			{
				mcsNew.Dispose();
				mcsNew = null;
			}
			if (pTopSpace > 1073741823)
			{
				pTopSpace = 0;
			}
		}
		dvrTopSpace = ((mbpInfo.BPTop != 0) ? margin : pTopSpace);
		dvrUsed += rectTransform.v - fsrcToFill.v;
		if (fsfmtr.kstop >= PTS.FSFMTRKSTOP.fmtrNoProgressOutOfSpace)
		{
			dvrUsed = 0;
		}
		if (pmcsclientOut != IntPtr.Zero)
		{
			mcsNew = base.PtsContext.HandleToObject(pmcsclientOut) as MarginCollapsingState;
			PTS.ValidateHandle(mcsNew);
			pmcsclientOut = IntPtr.Zero;
		}
		if (fsfmtr.kstop == PTS.FSFMTRKSTOP.fmtrGoalReached)
		{
			MarginCollapsingState mcsNew2 = null;
			MarginCollapsingState.CollapseBottomMargin(base.PtsContext, mbpInfo, mcsNew, out mcsNew2, out margin2);
			pmcsclientOut = mcsNew2?.Handle ?? IntPtr.Zero;
			dvrUsed += margin2 + mbpInfo.BPBottom;
			dvrUsed = Math.Min(fsrcToFill.dv, dvrUsed);
		}
		if (mcsNew != null)
		{
			mcsNew.Dispose();
			mcsNew = null;
		}
		fsbbox.fsrc.u -= mbpInfo.MBPLeft;
		fsbbox.fsrc.du += mbpInfo.MBPLeft + mbpInfo.MBPRight;
		if (num != fswdir)
		{
			PTS.FSRECT rectPage2 = base.StructuralCache.CurrentFormatContext.PageRect;
			PTS.Validate(PTS.FsTransformBbox(num, ref rectPage2, ref fsbbox, fswdir, out fsbbox));
		}
		paraClient.SetChunkInfo(pbrkrecIn == IntPtr.Zero, pbrkrecOut == IntPtr.Zero);
	}

	internal void FormatParaBottomless(ContainerParaClient paraClient, int iArea, nint geometry, int fSuppressTopSpace, uint fswdir, int urTrack, int durTrack, int vrTrack, MarginCollapsingState mcs, PTS.FSKCLEAR fskclearIn, int fInterruptable, out PTS.FSFMTRBL fsfmtrbl, out nint pfspara, out int dvrUsed, out PTS.FSBBOX fsbbox, out nint pmcsclientOut, out PTS.FSKCLEAR fskclearOut, out int dvrTopSpace, out int fPageBecomesUninterruptable)
	{
		uint num = PTS.FlowDirectionToFswdir((FlowDirection)base.Element.GetValue(FrameworkElement.FlowDirectionProperty));
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		MarginCollapsingState.CollapseTopMargin(base.PtsContext, mbpInfo, mcs, out var mcsNew, out var margin);
		if (PTS.ToBoolean(fSuppressTopSpace))
		{
			margin = 0;
		}
		Invariant.Assert(base.Element is Block || base.Element is ListItem);
		fskclearIn = PTS.WrapDirectionToFskclear((WrapDirection)base.Element.GetValue(Block.ClearFloatersProperty));
		if (num != fswdir)
		{
			PTS.FSRECT rectTransform = new PTS.FSRECT(urTrack, 0, durTrack, 0);
			PTS.FSRECT rectPage = base.StructuralCache.CurrentFormatContext.PageRect;
			PTS.Validate(PTS.FsTransformRectangle(fswdir, ref rectPage, ref rectTransform, num, out rectTransform));
			urTrack = rectTransform.u;
			durTrack = rectTransform.du;
			mbpInfo.MirrorMargin();
		}
		int pTopSpace = 0;
		int ur = Math.Max(Math.Min(urTrack + mbpInfo.MBPLeft, urTrack + durTrack - 1), urTrack);
		int dur = Math.Max(durTrack - (mbpInfo.MBPLeft + mbpInfo.MBPRight), 0);
		int num2 = vrTrack + (margin + mbpInfo.BPTop);
		try
		{
			PTS.Validate(PTS.FsFormatSubtrackBottomless(base.PtsContext.Context, base.Handle, iArea, geometry, fSuppressTopSpace, num, ur, dur, num2, mcsNew?.Handle ?? IntPtr.Zero, fskclearIn, fInterruptable, out fsfmtrbl, out pfspara, out dvrUsed, out fsbbox, out pmcsclientOut, out fskclearOut, out pTopSpace, out fPageBecomesUninterruptable), base.PtsContext);
		}
		finally
		{
			if (mcsNew != null)
			{
				mcsNew.Dispose();
				mcsNew = null;
			}
		}
		if (fsfmtrbl != PTS.FSFMTRBL.fmtrblCollision)
		{
			if (pmcsclientOut != IntPtr.Zero)
			{
				mcsNew = base.PtsContext.HandleToObject(pmcsclientOut) as MarginCollapsingState;
				PTS.ValidateHandle(mcsNew);
				pmcsclientOut = IntPtr.Zero;
			}
			MarginCollapsingState.CollapseBottomMargin(base.PtsContext, mbpInfo, mcsNew, out var mcsNew2, out var margin2);
			pmcsclientOut = mcsNew2?.Handle ?? IntPtr.Zero;
			if (mcsNew != null)
			{
				mcsNew.Dispose();
				mcsNew = null;
			}
			dvrTopSpace = ((mbpInfo.BPTop != 0) ? margin : pTopSpace);
			dvrUsed += num2 - vrTrack + margin2 + mbpInfo.BPBottom;
		}
		else
		{
			pfspara = IntPtr.Zero;
			dvrTopSpace = 0;
		}
		fsbbox.fsrc.u -= mbpInfo.MBPLeft;
		fsbbox.fsrc.du += mbpInfo.MBPLeft + mbpInfo.MBPRight;
		if (num != fswdir)
		{
			PTS.FSRECT rectPage2 = base.StructuralCache.CurrentFormatContext.PageRect;
			PTS.Validate(PTS.FsTransformBbox(num, ref rectPage2, ref fsbbox, fswdir, out fsbbox));
		}
		paraClient.SetChunkInfo(isFirstChunk: true, isLastChunk: true);
	}

	internal void UpdateBottomlessPara(nint pfspara, ContainerParaClient paraClient, int iArea, nint pfsgeom, int fSuppressTopSpace, uint fswdir, int urTrack, int durTrack, int vrTrack, MarginCollapsingState mcs, PTS.FSKCLEAR fskclearIn, int fInterruptable, out PTS.FSFMTRBL fsfmtrbl, out int dvrUsed, out PTS.FSBBOX fsbbox, out nint pmcsclientOut, out PTS.FSKCLEAR fskclearOut, out int dvrTopSpace, out int fPageBecomesUninterruptable)
	{
		uint num = PTS.FlowDirectionToFswdir((FlowDirection)base.Element.GetValue(FrameworkElement.FlowDirectionProperty));
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		MarginCollapsingState.CollapseTopMargin(base.PtsContext, mbpInfo, mcs, out var mcsNew, out var margin);
		if (PTS.ToBoolean(fSuppressTopSpace))
		{
			margin = 0;
		}
		Invariant.Assert(base.Element is Block || base.Element is ListItem);
		fskclearIn = PTS.WrapDirectionToFskclear((WrapDirection)base.Element.GetValue(Block.ClearFloatersProperty));
		if (num != fswdir)
		{
			PTS.FSRECT rectTransform = new PTS.FSRECT(urTrack, 0, durTrack, 0);
			PTS.FSRECT rectPage = base.StructuralCache.CurrentFormatContext.PageRect;
			PTS.Validate(PTS.FsTransformRectangle(fswdir, ref rectPage, ref rectTransform, num, out rectTransform));
			urTrack = rectTransform.u;
			durTrack = rectTransform.du;
			mbpInfo.MirrorMargin();
		}
		int pTopSpace = 0;
		int ur = Math.Max(Math.Min(urTrack + mbpInfo.MBPLeft, urTrack + durTrack - 1), urTrack);
		int dur = Math.Max(durTrack - (mbpInfo.MBPLeft + mbpInfo.MBPRight), 0);
		int num2 = vrTrack + (margin + mbpInfo.BPTop);
		try
		{
			PTS.Validate(PTS.FsUpdateBottomlessSubtrack(base.PtsContext.Context, pfspara, base.Handle, iArea, pfsgeom, fSuppressTopSpace, num, ur, dur, num2, mcsNew?.Handle ?? IntPtr.Zero, fskclearIn, fInterruptable, out fsfmtrbl, out dvrUsed, out fsbbox, out pmcsclientOut, out fskclearOut, out pTopSpace, out fPageBecomesUninterruptable), base.PtsContext);
		}
		finally
		{
			if (mcsNew != null)
			{
				mcsNew.Dispose();
				mcsNew = null;
			}
		}
		if (fsfmtrbl != PTS.FSFMTRBL.fmtrblCollision)
		{
			if (pmcsclientOut != IntPtr.Zero)
			{
				mcsNew = base.PtsContext.HandleToObject(pmcsclientOut) as MarginCollapsingState;
				PTS.ValidateHandle(mcsNew);
				pmcsclientOut = IntPtr.Zero;
			}
			MarginCollapsingState.CollapseBottomMargin(base.PtsContext, mbpInfo, mcsNew, out var mcsNew2, out var margin2);
			pmcsclientOut = mcsNew2?.Handle ?? IntPtr.Zero;
			if (mcsNew != null)
			{
				mcsNew.Dispose();
				mcsNew = null;
			}
			dvrTopSpace = ((mbpInfo.BPTop != 0) ? margin : pTopSpace);
			dvrUsed += num2 - vrTrack + margin2 + mbpInfo.BPBottom;
		}
		else
		{
			pfspara = IntPtr.Zero;
			dvrTopSpace = 0;
		}
		fsbbox.fsrc.u -= mbpInfo.MBPLeft;
		fsbbox.fsrc.du += mbpInfo.MBPLeft + mbpInfo.MBPRight;
		if (num != fswdir)
		{
			PTS.FSRECT rectPage2 = base.StructuralCache.CurrentFormatContext.PageRect;
			PTS.Validate(PTS.FsTransformBbox(num, ref rectPage2, ref fsbbox, fswdir, out fsbbox));
		}
		paraClient.SetChunkInfo(isFirstChunk: true, isLastChunk: true);
	}

	internal override void ClearUpdateInfo()
	{
		for (BaseParagraph baseParagraph = _firstChild; baseParagraph != null; baseParagraph = baseParagraph.Next)
		{
			baseParagraph.ClearUpdateInfo();
		}
		base.ClearUpdateInfo();
		_ur = null;
		_firstParaValidInUpdateMode = false;
	}

	internal override bool InvalidateStructure(int startPosition)
	{
		int paragraphStartCharacterPosition = base.ParagraphStartCharacterPosition;
		if (startPosition <= paragraphStartCharacterPosition + TextContainerHelper.ElementEdgeCharacterLength)
		{
			BaseParagraph baseParagraph = _firstChild;
			while (baseParagraph != null)
			{
				BaseParagraph baseParagraph2 = baseParagraph;
				baseParagraph = baseParagraph.Next;
				baseParagraph2.Dispose();
				baseParagraph2.Next = null;
				baseParagraph2.Previous = null;
			}
			_firstChild = (_lastFetchedChild = null);
		}
		else
		{
			for (BaseParagraph baseParagraph = _firstChild; baseParagraph != null; baseParagraph = baseParagraph.Next)
			{
				if (baseParagraph.ParagraphStartCharacterPosition + baseParagraph.LastFormatCch >= startPosition)
				{
					if (!baseParagraph.InvalidateStructure(startPosition))
					{
						baseParagraph = baseParagraph.Next;
					}
					if (baseParagraph != null)
					{
						if (baseParagraph.Previous != null)
						{
							baseParagraph.Previous.Next = null;
							_lastFetchedChild = baseParagraph.Previous;
						}
						else
						{
							_firstChild = (_lastFetchedChild = null);
						}
						while (baseParagraph != null)
						{
							BaseParagraph baseParagraph3 = baseParagraph;
							baseParagraph = baseParagraph.Next;
							baseParagraph3.Dispose();
							baseParagraph3.Next = null;
							baseParagraph3.Previous = null;
						}
					}
					break;
				}
			}
		}
		return startPosition < paragraphStartCharacterPosition + TextContainerHelper.ElementEdgeCharacterLength;
	}

	internal override void InvalidateFormatCache()
	{
		for (BaseParagraph baseParagraph = _firstChild; baseParagraph != null; baseParagraph = baseParagraph.Next)
		{
			baseParagraph.InvalidateFormatCache();
		}
	}

	protected virtual BaseParagraph GetParagraph(ITextPointer textPointer, bool fEmptyOk)
	{
		BaseParagraph baseParagraph = null;
		switch (textPointer.GetPointerContext(LogicalDirection.Forward))
		{
		case TextPointerContext.Text:
			if (textPointer.TextContainer.Start.CompareTo(textPointer) > 0 && (!(base.Element is TextElement) || ((TextElement)base.Element).ContentStart != textPointer))
			{
				throw new InvalidOperationException(SR.Format(SR.TextSchema_TextIsNotAllowedInThisContext, base.Element.GetType().Name));
			}
			baseParagraph = new TextParagraph(base.Element, base.StructuralCache);
			break;
		case TextPointerContext.ElementEnd:
			Invariant.Assert(textPointer is TextPointer);
			Invariant.Assert(base.Element == ((TextPointer)textPointer).Parent);
			if (!fEmptyOk)
			{
				baseParagraph = new TextParagraph(base.Element, base.StructuralCache);
			}
			break;
		case TextPointerContext.ElementStart:
		{
			TextElement adjacentElementFromOuterPosition = ((TextPointer)textPointer).GetAdjacentElementFromOuterPosition(LogicalDirection.Forward);
			if (adjacentElementFromOuterPosition is List)
			{
				baseParagraph = new ListParagraph(adjacentElementFromOuterPosition, base.StructuralCache);
			}
			else if (adjacentElementFromOuterPosition is Table)
			{
				baseParagraph = new TableParagraph(adjacentElementFromOuterPosition, base.StructuralCache);
			}
			else if (adjacentElementFromOuterPosition is BlockUIContainer)
			{
				baseParagraph = new UIElementParagraph(adjacentElementFromOuterPosition, base.StructuralCache);
			}
			else if (adjacentElementFromOuterPosition is Block || adjacentElementFromOuterPosition is ListItem)
			{
				baseParagraph = new ContainerParagraph(adjacentElementFromOuterPosition, base.StructuralCache);
			}
			else if (adjacentElementFromOuterPosition is Inline)
			{
				baseParagraph = new TextParagraph(base.Element, base.StructuralCache);
			}
			else
			{
				Invariant.Assert(condition: false);
			}
			break;
		}
		case TextPointerContext.EmbeddedElement:
			baseParagraph = new TextParagraph(base.Element, base.StructuralCache);
			break;
		case TextPointerContext.None:
			Invariant.Assert(textPointer.CompareTo(textPointer.TextContainer.End) == 0);
			if (!fEmptyOk)
			{
				baseParagraph = new TextParagraph(base.Element, base.StructuralCache);
			}
			break;
		}
		if (baseParagraph != null)
		{
			base.StructuralCache.CurrentFormatContext.DependentMax = (TextPointer)textPointer;
		}
		return baseParagraph;
	}

	private bool NeedsUpdate()
	{
		return base.StructuralCache.DtrsFromRange(base.ParagraphStartCharacterPosition, base.LastFormatCch) != null;
	}

	private void BuildUpdateRecord()
	{
		_ur = null;
		DtrList dtrList = base.StructuralCache.DtrsFromRange(base.ParagraphStartCharacterPosition, base.LastFormatCch);
		UpdateRecord updateRecord2;
		if (dtrList != null)
		{
			UpdateRecord updateRecord = null;
			for (int i = 0; i < dtrList.Length; i++)
			{
				int cPFromElement = TextContainerHelper.GetCPFromElement(base.StructuralCache.TextContainer, base.Element, ElementEdge.AfterStart);
				updateRecord2 = UpdateRecordFromDtr(dtrList, dtrList[i], cPFromElement);
				if (updateRecord == null)
				{
					_ur = updateRecord2;
				}
				else
				{
					updateRecord.Next = updateRecord2;
				}
				updateRecord = updateRecord2;
			}
			updateRecord2 = _ur;
			while (updateRecord2.Next != null)
			{
				if (updateRecord2.SyncPara != null)
				{
					if (updateRecord2.SyncPara.Previous == updateRecord2.Next.FirstPara)
					{
						updateRecord2.MergeWithNext();
					}
					else if (updateRecord2.SyncPara == updateRecord2.Next.FirstPara && updateRecord2.Next.ChangeType == PTS.FSKCHANGE.fskchNew)
					{
						updateRecord2.MergeWithNext();
					}
					else
					{
						updateRecord2 = updateRecord2.Next;
					}
				}
				else
				{
					updateRecord2.MergeWithNext();
				}
			}
		}
		updateRecord2 = _ur;
		while (updateRecord2 != null && updateRecord2.FirstPara != null)
		{
			BaseParagraph baseParagraph = null;
			if (updateRecord2.ChangeType == PTS.FSKCHANGE.fskchInside)
			{
				baseParagraph = updateRecord2.FirstPara.Next;
				updateRecord2.FirstPara.Next = null;
			}
			else
			{
				baseParagraph = updateRecord2.FirstPara;
			}
			while (baseParagraph != updateRecord2.SyncPara)
			{
				if (baseParagraph.Next != null)
				{
					baseParagraph.Next.Previous = null;
				}
				if (baseParagraph.Previous != null)
				{
					baseParagraph.Previous.Next = null;
				}
				baseParagraph.Dispose();
				baseParagraph = baseParagraph.Next;
			}
			updateRecord2 = updateRecord2.Next;
		}
		if (_ur != null && _ur.FirstPara == _firstChild && _ur.ChangeType == PTS.FSKCHANGE.fskchNew)
		{
			_firstChild = null;
		}
		_firstParaValidInUpdateMode = true;
	}

	private UpdateRecord UpdateRecordFromDtr(DtrList dtrs, DirtyTextRange dtr, int dcpContent)
	{
		UpdateRecord updateRecord = new UpdateRecord();
		updateRecord.Dtr = dtr;
		BaseParagraph baseParagraph = _firstChild;
		int num = dcpContent;
		if (num < updateRecord.Dtr.StartIndex)
		{
			while (baseParagraph != null && num + baseParagraph.LastFormatCch <= updateRecord.Dtr.StartIndex && (num + baseParagraph.LastFormatCch != updateRecord.Dtr.StartIndex || !(baseParagraph is TextParagraph)))
			{
				num += baseParagraph.LastFormatCch;
				baseParagraph = baseParagraph.Next;
			}
		}
		updateRecord.FirstPara = baseParagraph;
		if (baseParagraph == null)
		{
			updateRecord.ChangeType = PTS.FSKCHANGE.fskchNew;
		}
		else if (num < updateRecord.Dtr.StartIndex)
		{
			updateRecord.ChangeType = PTS.FSKCHANGE.fskchInside;
		}
		else
		{
			updateRecord.ChangeType = PTS.FSKCHANGE.fskchNew;
		}
		updateRecord.SyncPara = null;
		while (baseParagraph != null)
		{
			if (num + baseParagraph.LastFormatCch > updateRecord.Dtr.StartIndex + updateRecord.Dtr.PositionsRemoved || (num + baseParagraph.LastFormatCch == updateRecord.Dtr.StartIndex + updateRecord.Dtr.PositionsRemoved && updateRecord.ChangeType != PTS.FSKCHANGE.fskchNew))
			{
				updateRecord.SyncPara = baseParagraph.Next;
				break;
			}
			num += baseParagraph.LastFormatCch;
			baseParagraph = baseParagraph.Next;
		}
		return updateRecord;
	}
}
