using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.TextFormatting;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class PtsHost
{
	private PtsContext _ptsContext;

	private MS.Internal.SecurityCriticalDataForSet<nint> _context;

	private static int _objectContextOffset = 10;

	private static int _customParaId = 0;

	private PtsContext PtsContext
	{
		get
		{
			Invariant.Assert(_ptsContext != null);
			return _ptsContext;
		}
	}

	internal nint Context
	{
		get
		{
			Invariant.Assert(_context.Value != IntPtr.Zero);
			return _context.Value;
		}
		set
		{
			Invariant.Assert(_context.Value == IntPtr.Zero);
			_context.Value = value;
		}
	}

	internal static int ContainerParagraphId => _customParaId;

	internal static int SubpageParagraphId => ContainerParagraphId + 1;

	internal static int FloaterParagraphId => SubpageParagraphId + 1;

	internal static int TableParagraphId => FloaterParagraphId + 1;

	internal PtsHost()
	{
		_context = new MS.Internal.SecurityCriticalDataForSet<nint>(IntPtr.Zero);
	}

	internal void EnterContext(PtsContext ptsContext)
	{
		Invariant.Assert(_ptsContext == null);
		_ptsContext = ptsContext;
	}

	internal void LeaveContext(PtsContext ptsContext)
	{
		Invariant.Assert(_ptsContext == ptsContext);
		_ptsContext = null;
	}

	internal void AssertFailed(string arg1, string arg2, int arg3, uint arg4)
	{
		if (!PtsCache.IsDisposed())
		{
			ErrorHandler.Assert(false, ErrorHandler.PTSAssert, arg1, arg2, arg3, arg4);
		}
	}

	internal int GetFigureProperties(nint pfsclient, nint pfsparaclientFigure, nint nmpFigure, int fInTextLine, uint fswdir, int fBottomUndefined, out int dur, out int dvr, out PTS.FSFIGUREPROPS fsfigprops, out int cPolygons, out int cVertices, out int durDistTextLeft, out int durDistTextRight, out int dvrDistTextTop, out int dvrDistTextBottom)
	{
		int result = 0;
		try
		{
			FigureParagraph obj = PtsContext.HandleToObject(nmpFigure) as FigureParagraph;
			PTS.ValidateHandle(obj);
			FigureParaClient figureParaClient = PtsContext.HandleToObject(pfsparaclientFigure) as FigureParaClient;
			PTS.ValidateHandle(figureParaClient);
			obj.GetFigureProperties(figureParaClient, fInTextLine, fswdir, fBottomUndefined, out dur, out dvr, out fsfigprops, out cPolygons, out cVertices, out durDistTextLeft, out durDistTextRight, out dvrDistTextTop, out dvrDistTextBottom);
		}
		catch (Exception callbackException)
		{
			dur = (dvr = (cPolygons = (cVertices = 0)));
			fsfigprops = default(PTS.FSFIGUREPROPS);
			durDistTextLeft = (durDistTextRight = (dvrDistTextTop = (dvrDistTextBottom = 0)));
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			dur = (dvr = (cPolygons = (cVertices = 0)));
			fsfigprops = default(PTS.FSFIGUREPROPS);
			durDistTextLeft = (durDistTextRight = (dvrDistTextTop = (dvrDistTextBottom = 0)));
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal unsafe int GetFigurePolygons(nint pfsclient, nint pfsparaclientFigure, nint nmpFigure, uint fswdir, int ncVertices, int nfspt, int* rgcVertices, out int ccVertices, PTS.FSPOINT* rgfspt, out int cfspt, out int fWrapThrough)
	{
		int result = 0;
		try
		{
			FigureParagraph obj = PtsContext.HandleToObject(nmpFigure) as FigureParagraph;
			PTS.ValidateHandle(obj);
			FigureParaClient figureParaClient = PtsContext.HandleToObject(pfsparaclientFigure) as FigureParaClient;
			PTS.ValidateHandle(figureParaClient);
			obj.GetFigurePolygons(figureParaClient, fswdir, ncVertices, nfspt, rgcVertices, out ccVertices, rgfspt, out cfspt, out fWrapThrough);
		}
		catch (Exception callbackException)
		{
			ccVertices = (cfspt = (fWrapThrough = 0));
			rgfspt = null;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			ccVertices = (cfspt = (fWrapThrough = 0));
			rgfspt = null;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int CalcFigurePosition(nint pfsclient, nint pfsparaclientFigure, nint nmpFigure, uint fswdir, ref PTS.FSRECT fsrcPage, ref PTS.FSRECT fsrcMargin, ref PTS.FSRECT fsrcTrack, ref PTS.FSRECT fsrcFigurePreliminary, int fMustPosition, int fInTextLine, out int fPushToNextTrack, out PTS.FSRECT fsrcFlow, out PTS.FSRECT fsrcOverlap, out PTS.FSBBOX fsbbox, out PTS.FSRECT fsrcSearch)
	{
		int result = 0;
		try
		{
			FigureParagraph obj = PtsContext.HandleToObject(nmpFigure) as FigureParagraph;
			PTS.ValidateHandle(obj);
			FigureParaClient figureParaClient = PtsContext.HandleToObject(pfsparaclientFigure) as FigureParaClient;
			PTS.ValidateHandle(figureParaClient);
			obj.CalcFigurePosition(figureParaClient, fswdir, ref fsrcPage, ref fsrcMargin, ref fsrcTrack, ref fsrcFigurePreliminary, fMustPosition, fInTextLine, out fPushToNextTrack, out fsrcFlow, out fsrcOverlap, out fsbbox, out fsrcSearch);
		}
		catch (Exception callbackException)
		{
			fPushToNextTrack = 0;
			fsrcFlow = (fsrcOverlap = (fsrcSearch = default(PTS.FSRECT)));
			fsbbox = default(PTS.FSBBOX);
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fPushToNextTrack = 0;
			fsrcFlow = (fsrcOverlap = (fsrcSearch = default(PTS.FSRECT)));
			fsbbox = default(PTS.FSBBOX);
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int FSkipPage(nint pfsclient, nint nms, out int fSkip)
	{
		int result = 0;
		try
		{
			Section obj = PtsContext.HandleToObject(nms) as Section;
			PTS.ValidateHandle(obj);
			obj.FSkipPage(out fSkip);
		}
		catch (Exception callbackException)
		{
			fSkip = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fSkip = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetPageDimensions(nint pfsclient, nint nms, out uint fswdir, out int fHeaderFooterAtTopBottom, out int durPage, out int dvrPage, ref PTS.FSRECT fsrcMargin)
	{
		int result = 0;
		try
		{
			Section obj = PtsContext.HandleToObject(nms) as Section;
			PTS.ValidateHandle(obj);
			obj.GetPageDimensions(out fswdir, out fHeaderFooterAtTopBottom, out durPage, out dvrPage, ref fsrcMargin);
		}
		catch (Exception callbackException)
		{
			fswdir = 0u;
			fHeaderFooterAtTopBottom = (durPage = (dvrPage = 0));
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fswdir = 0u;
			fHeaderFooterAtTopBottom = (durPage = (dvrPage = 0));
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetNextSection(nint pfsclient, nint nmsCur, out int fSuccess, out nint nmsNext)
	{
		int result = 0;
		try
		{
			Section obj = PtsContext.HandleToObject(nmsCur) as Section;
			PTS.ValidateHandle(obj);
			obj.GetNextSection(out fSuccess, out nmsNext);
		}
		catch (Exception callbackException)
		{
			fSuccess = 0;
			nmsNext = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fSuccess = 0;
			nmsNext = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetSectionProperties(nint pfsclient, nint nms, out int fNewPage, out uint fswdir, out int fApplyColumnBalancing, out int ccol, out int cSegmentDefinedColumnSpanAreas, out int cHeightDefinedColumnSpanAreas)
	{
		int result = 0;
		try
		{
			Section obj = PtsContext.HandleToObject(nms) as Section;
			PTS.ValidateHandle(obj);
			obj.GetSectionProperties(out fNewPage, out fswdir, out fApplyColumnBalancing, out ccol, out cSegmentDefinedColumnSpanAreas, out cHeightDefinedColumnSpanAreas);
		}
		catch (Exception callbackException)
		{
			fNewPage = (fApplyColumnBalancing = (ccol = 0));
			fswdir = 0u;
			cSegmentDefinedColumnSpanAreas = (cHeightDefinedColumnSpanAreas = 0);
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fNewPage = (fApplyColumnBalancing = (ccol = 0));
			fswdir = 0u;
			cSegmentDefinedColumnSpanAreas = (cHeightDefinedColumnSpanAreas = 0);
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal unsafe int GetJustificationProperties(nint pfsclient, nint* rgnms, int cnms, int fLastSectionNotBroken, out int fJustify, out PTS.FSKALIGNPAGE fskal, out int fCancelAtLastColumn)
	{
		int result = 0;
		try
		{
			Section obj = PtsContext.HandleToObject(*rgnms) as Section;
			PTS.ValidateHandle(obj);
			obj.GetJustificationProperties(rgnms, cnms, fLastSectionNotBroken, out fJustify, out fskal, out fCancelAtLastColumn);
		}
		catch (Exception callbackException)
		{
			fJustify = (fCancelAtLastColumn = 0);
			fskal = PTS.FSKALIGNPAGE.fskalpgTop;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fJustify = (fCancelAtLastColumn = 0);
			fskal = PTS.FSKALIGNPAGE.fskalpgTop;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetMainTextSegment(nint pfsclient, nint nmsSection, out nint nmSegment)
	{
		int result = 0;
		try
		{
			Section obj = PtsContext.HandleToObject(nmsSection) as Section;
			PTS.ValidateHandle(obj);
			obj.GetMainTextSegment(out nmSegment);
		}
		catch (Exception callbackException)
		{
			nmSegment = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			nmSegment = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetHeaderSegment(nint pfsclient, nint nms, nint pfsbrpagePrelim, uint fswdir, out int fHeaderPresent, out int fHardMargin, out int dvrMaxHeight, out int dvrFromEdge, out uint fswdirHeader, out nint nmsHeader)
	{
		int result = 0;
		try
		{
			Section obj = PtsContext.HandleToObject(nms) as Section;
			PTS.ValidateHandle(obj);
			obj.GetHeaderSegment(pfsbrpagePrelim, fswdir, out fHeaderPresent, out fHardMargin, out dvrMaxHeight, out dvrFromEdge, out fswdirHeader, out nmsHeader);
		}
		catch (Exception callbackException)
		{
			fHeaderPresent = (fHardMargin = (dvrMaxHeight = (dvrFromEdge = 0)));
			fswdirHeader = 0u;
			nmsHeader = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fHeaderPresent = (fHardMargin = (dvrMaxHeight = (dvrFromEdge = 0)));
			fswdirHeader = 0u;
			nmsHeader = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetFooterSegment(nint pfsclient, nint nms, nint pfsbrpagePrelim, uint fswdir, out int fFooterPresent, out int fHardMargin, out int dvrMaxHeight, out int dvrFromEdge, out uint fswdirFooter, out nint nmsFooter)
	{
		int result = 0;
		try
		{
			Section obj = PtsContext.HandleToObject(nms) as Section;
			PTS.ValidateHandle(obj);
			obj.GetFooterSegment(pfsbrpagePrelim, fswdir, out fFooterPresent, out fHardMargin, out dvrMaxHeight, out dvrFromEdge, out fswdirFooter, out nmsFooter);
		}
		catch (Exception callbackException)
		{
			fFooterPresent = (fHardMargin = (dvrMaxHeight = (dvrFromEdge = 0)));
			fswdirFooter = 0u;
			nmsFooter = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fFooterPresent = (fHardMargin = (dvrMaxHeight = (dvrFromEdge = 0)));
			fswdirFooter = 0u;
			nmsFooter = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int UpdGetSegmentChange(nint pfsclient, nint nms, out PTS.FSKCHANGE fskch)
	{
		int result = 0;
		try
		{
			ContainerParagraph obj = PtsContext.HandleToObject(nms) as ContainerParagraph;
			PTS.ValidateHandle(obj);
			obj.UpdGetSegmentChange(out fskch);
		}
		catch (Exception callbackException)
		{
			fskch = PTS.FSKCHANGE.fskchNone;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fskch = PTS.FSKCHANGE.fskchNone;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal unsafe int GetSectionColumnInfo(nint pfsclient, nint nms, uint fswdir, int ncol, PTS.FSCOLUMNINFO* fscolinfo, out int ccol)
	{
		int result = 0;
		try
		{
			Section obj = PtsContext.HandleToObject(nms) as Section;
			PTS.ValidateHandle(obj);
			obj.GetSectionColumnInfo(fswdir, ncol, fscolinfo, out ccol);
		}
		catch (Exception callbackException)
		{
			ccol = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			ccol = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal unsafe int GetSegmentDefinedColumnSpanAreaInfo(nint pfsclient, nint nms, int cAreas, nint* rgnmSeg, int* rgcColumns, out int cAreasActual)
	{
		cAreasActual = 0;
		return -10000;
	}

	internal unsafe int GetHeightDefinedColumnSpanAreaInfo(nint pfsclient, nint nms, int cAreas, int* rgdvrAreaHeight, int* rgcColumns, out int cAreasActual)
	{
		cAreasActual = 0;
		return -10000;
	}

	internal int GetFirstPara(nint pfsclient, nint nms, out int fSuccessful, out nint nmp)
	{
		int result = 0;
		try
		{
			ISegment obj = PtsContext.HandleToObject(nms) as ISegment;
			PTS.ValidateHandle(obj);
			obj.GetFirstPara(out fSuccessful, out nmp);
		}
		catch (Exception callbackException)
		{
			fSuccessful = 0;
			nmp = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fSuccessful = 0;
			nmp = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetNextPara(nint pfsclient, nint nms, nint nmpCur, out int fFound, out nint nmpNext)
	{
		int result = 0;
		try
		{
			ISegment obj = PtsContext.HandleToObject(nms) as ISegment;
			PTS.ValidateHandle(obj);
			BaseParagraph baseParagraph = PtsContext.HandleToObject(nmpCur) as BaseParagraph;
			PTS.ValidateHandle(baseParagraph);
			obj.GetNextPara(baseParagraph, out fFound, out nmpNext);
		}
		catch (Exception callbackException)
		{
			fFound = 0;
			nmpNext = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fFound = 0;
			nmpNext = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int UpdGetFirstChangeInSegment(nint pfsclient, nint nms, out int fFound, out int fChangeFirst, out nint nmpBeforeChange)
	{
		int result = 0;
		try
		{
			ISegment obj = PtsContext.HandleToObject(nms) as ISegment;
			PTS.ValidateHandle(obj);
			obj.UpdGetFirstChangeInSegment(out fFound, out fChangeFirst, out nmpBeforeChange);
		}
		catch (Exception callbackException)
		{
			fFound = (fChangeFirst = 0);
			nmpBeforeChange = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fFound = (fChangeFirst = 0);
			nmpBeforeChange = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int UpdGetParaChange(nint pfsclient, nint nmp, out PTS.FSKCHANGE fskch, out int fNoFurtherChanges)
	{
		int result = 0;
		try
		{
			BaseParagraph obj = PtsContext.HandleToObject(nmp) as BaseParagraph;
			PTS.ValidateHandle(obj);
			obj.UpdGetParaChange(out fskch, out fNoFurtherChanges);
		}
		catch (Exception callbackException)
		{
			fskch = PTS.FSKCHANGE.fskchNone;
			fNoFurtherChanges = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fskch = PTS.FSKCHANGE.fskchNone;
			fNoFurtherChanges = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetParaProperties(nint pfsclient, nint nmp, ref PTS.FSPAP fspap)
	{
		int result = 0;
		try
		{
			BaseParagraph obj = PtsContext.HandleToObject(nmp) as BaseParagraph;
			PTS.ValidateHandle(obj);
			obj.GetParaProperties(ref fspap);
		}
		catch (Exception callbackException)
		{
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int CreateParaclient(nint pfsclient, nint nmp, out nint pfsparaclient)
	{
		int result = 0;
		try
		{
			BaseParagraph obj = PtsContext.HandleToObject(nmp) as BaseParagraph;
			PTS.ValidateHandle(obj);
			obj.CreateParaclient(out pfsparaclient);
		}
		catch (Exception callbackException)
		{
			pfsparaclient = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			pfsparaclient = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int TransferDisplayInfo(nint pfsclient, nint pfsparaclientOld, nint pfsparaclientNew)
	{
		int result = 0;
		try
		{
			BaseParaClient baseParaClient = PtsContext.HandleToObject(pfsparaclientOld) as BaseParaClient;
			PTS.ValidateHandle(baseParaClient);
			BaseParaClient obj = PtsContext.HandleToObject(pfsparaclientNew) as BaseParaClient;
			PTS.ValidateHandle(obj);
			obj.TransferDisplayInfo(baseParaClient);
		}
		catch (Exception callbackException)
		{
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int DestroyParaclient(nint pfsclient, nint pfsparaclient)
	{
		int result = 0;
		try
		{
			BaseParaClient obj = PtsContext.HandleToObject(pfsparaclient) as BaseParaClient;
			PTS.ValidateHandle(obj);
			obj.Dispose();
		}
		catch (Exception callbackException)
		{
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int FInterruptFormattingAfterPara(nint pfsclient, nint pfsparaclient, nint nmp, int vr, out int fInterruptFormatting)
	{
		fInterruptFormatting = 0;
		return 0;
	}

	internal int GetEndnoteSeparators(nint pfsclient, nint nmsSection, out nint nmsEndnoteSeparator, out nint nmsEndnoteContSeparator, out nint nmsEndnoteContNotice)
	{
		int result = 0;
		try
		{
			Section obj = PtsContext.HandleToObject(nmsSection) as Section;
			PTS.ValidateHandle(obj);
			obj.GetEndnoteSeparators(out nmsEndnoteSeparator, out nmsEndnoteContSeparator, out nmsEndnoteContNotice);
		}
		catch (Exception callbackException)
		{
			nmsEndnoteSeparator = (nmsEndnoteContSeparator = (nmsEndnoteContNotice = IntPtr.Zero));
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			nmsEndnoteSeparator = (nmsEndnoteContSeparator = (nmsEndnoteContNotice = IntPtr.Zero));
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetEndnoteSegment(nint pfsclient, nint nmsSection, out int fEndnotesPresent, out nint nmsEndnotes)
	{
		int result = 0;
		try
		{
			Section obj = PtsContext.HandleToObject(nmsSection) as Section;
			PTS.ValidateHandle(obj);
			obj.GetEndnoteSegment(out fEndnotesPresent, out nmsEndnotes);
		}
		catch (Exception callbackException)
		{
			fEndnotesPresent = 0;
			nmsEndnotes = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fEndnotesPresent = 0;
			nmsEndnotes = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetNumberEndnoteColumns(nint pfsclient, nint nms, out int ccolEndnote)
	{
		ccolEndnote = 0;
		return -10000;
	}

	internal unsafe int GetEndnoteColumnInfo(nint pfsclient, nint nms, uint fswdir, int ncolEndnote, PTS.FSCOLUMNINFO* fscolinfoEndnote, out int ccolEndnote)
	{
		ccolEndnote = 0;
		return -10000;
	}

	internal int GetFootnoteSeparators(nint pfsclient, nint nmsSection, out nint nmsFtnSeparator, out nint nmsFtnContSeparator, out nint nmsFtnContNotice)
	{
		nmsFtnSeparator = (nmsFtnContSeparator = (nmsFtnContNotice = IntPtr.Zero));
		return -10000;
	}

	internal int FFootnoteBeneathText(nint pfsclient, nint nms, out int fFootnoteBeneathText)
	{
		fFootnoteBeneathText = 0;
		return -10000;
	}

	internal int GetNumberFootnoteColumns(nint pfsclient, nint nms, out int ccolFootnote)
	{
		ccolFootnote = 0;
		return -10000;
	}

	internal unsafe int GetFootnoteColumnInfo(nint pfsclient, nint nms, uint fswdir, int ncolFootnote, PTS.FSCOLUMNINFO* fscolinfoFootnote, out int ccolFootnote)
	{
		ccolFootnote = 0;
		return -10000;
	}

	internal int GetFootnoteSegment(nint pfsclient, nint nmftn, out nint nmsFootnote)
	{
		nmsFootnote = IntPtr.Zero;
		return -10000;
	}

	internal unsafe int GetFootnotePresentationAndRejectionOrder(nint pfsclient, int cFootnotes, nint* rgProposedPresentationOrder, nint* rgProposedRejectionOrder, out int fProposedPresentationOrderAccepted, nint* rgFinalPresentationOrder, out int fProposedRejectionOrderAccepted, nint* rgFinalRejectionOrder)
	{
		fProposedPresentationOrderAccepted = (fProposedRejectionOrderAccepted = 0);
		return -10000;
	}

	internal int FAllowFootnoteSeparation(nint pfsclient, nint nmftn, out int fAllow)
	{
		fAllow = 0;
		return -10000;
	}

	internal int DuplicateMcsclient(nint pfsclient, nint pmcsclientIn, out nint pmcsclientNew)
	{
		int result = 0;
		try
		{
			MarginCollapsingState marginCollapsingState = PtsContext.HandleToObject(pmcsclientIn) as MarginCollapsingState;
			PTS.ValidateHandle(marginCollapsingState);
			pmcsclientNew = marginCollapsingState.Clone().Handle;
		}
		catch (Exception callbackException)
		{
			pmcsclientNew = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			pmcsclientNew = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int DestroyMcsclient(nint pfsclient, nint pmcsclient)
	{
		int result = 0;
		try
		{
			MarginCollapsingState obj = PtsContext.HandleToObject(pmcsclient) as MarginCollapsingState;
			PTS.ValidateHandle(obj);
			obj.Dispose();
		}
		catch (Exception callbackException)
		{
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int FEqualMcsclient(nint pfsclient, nint pmcsclient1, nint pmcsclient2, out int fEqual)
	{
		int result = 0;
		if (pmcsclient1 == IntPtr.Zero || pmcsclient2 == IntPtr.Zero)
		{
			fEqual = PTS.FromBoolean(pmcsclient1 == pmcsclient2);
		}
		else
		{
			try
			{
				MarginCollapsingState marginCollapsingState = PtsContext.HandleToObject(pmcsclient1) as MarginCollapsingState;
				PTS.ValidateHandle(marginCollapsingState);
				MarginCollapsingState marginCollapsingState2 = PtsContext.HandleToObject(pmcsclient2) as MarginCollapsingState;
				PTS.ValidateHandle(marginCollapsingState2);
				fEqual = PTS.FromBoolean(marginCollapsingState.IsEqual(marginCollapsingState2));
			}
			catch (Exception callbackException)
			{
				fEqual = 0;
				PtsContext.CallbackException = callbackException;
				result = -100002;
			}
			catch
			{
				fEqual = 0;
				PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
				result = -100002;
			}
		}
		return result;
	}

	internal int ConvertMcsclient(nint pfsclient, nint pfsparaclient, nint nmp, uint fswdir, nint pmcsclient, int fSuppressTopSpace, out int dvr)
	{
		int result = 0;
		try
		{
			BaseParagraph obj = PtsContext.HandleToObject(nmp) as BaseParagraph;
			PTS.ValidateHandle(obj);
			BaseParaClient baseParaClient = PtsContext.HandleToObject(pfsparaclient) as BaseParaClient;
			PTS.ValidateHandle(baseParaClient);
			MarginCollapsingState marginCollapsingState = null;
			if (pmcsclient != IntPtr.Zero)
			{
				marginCollapsingState = PtsContext.HandleToObject(pmcsclient) as MarginCollapsingState;
				PTS.ValidateHandle(marginCollapsingState);
			}
			obj.CollapseMargin(baseParaClient, marginCollapsingState, fswdir, PTS.ToBoolean(fSuppressTopSpace), out dvr);
		}
		catch (Exception callbackException)
		{
			dvr = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			dvr = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetObjectHandlerInfo(nint pfsclient, int idobj, nint pObjectInfo)
	{
		int result = 0;
		try
		{
			if (idobj == FloaterParagraphId)
			{
				PtsCache.GetFloaterHandlerInfo(this, pObjectInfo);
			}
			else if (idobj == TableParagraphId)
			{
				PtsCache.GetTableObjHandlerInfo(this, pObjectInfo);
			}
			else
			{
				pObjectInfo = IntPtr.Zero;
			}
		}
		catch (Exception callbackException)
		{
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int CreateParaBreakingSession(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, int fsdcpStart, nint pfsbreakreclineclient, uint fswdir, int urStartTrack, int durTrack, int urPageLeftMargin, out nint ppfsparabreakingsession, out int fParagraphJustified)
	{
		int result = 0;
		try
		{
			TextParagraph obj = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(obj);
			TextParaClient textParaClient = PtsContext.HandleToObject(pfsparaclient) as TextParaClient;
			PTS.ValidateHandle(textParaClient);
			LineBreakRecord lineBreakRecord = null;
			if (pfsbreakreclineclient != IntPtr.Zero)
			{
				lineBreakRecord = PtsContext.HandleToObject(pfsbreakreclineclient) as LineBreakRecord;
				PTS.ValidateHandle(lineBreakRecord);
			}
			obj.CreateOptimalBreakSession(textParaClient, fsdcpStart, durTrack, lineBreakRecord, out var optimalBreakSession, out var isParagraphJustified);
			fParagraphJustified = PTS.FromBoolean(isParagraphJustified);
			ppfsparabreakingsession = optimalBreakSession.Handle;
		}
		catch (Exception callbackException)
		{
			ppfsparabreakingsession = IntPtr.Zero;
			fParagraphJustified = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			ppfsparabreakingsession = IntPtr.Zero;
			fParagraphJustified = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int DestroyParaBreakingSession(nint pfsclient, nint pfsparabreakingsession)
	{
		OptimalBreakSession obj = PtsContext.HandleToObject(pfsparabreakingsession) as OptimalBreakSession;
		PTS.ValidateHandle(obj);
		obj.Dispose();
		return 0;
	}

	internal int GetTextProperties(nint pfsclient, nint nmp, int iArea, ref PTS.FSTXTPROPS fstxtprops)
	{
		int result = 0;
		try
		{
			TextParagraph obj = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(obj);
			obj.GetTextProperties(iArea, ref fstxtprops);
		}
		catch (Exception callbackException)
		{
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetNumberFootnotes(nint pfsclient, nint nmp, int fsdcpStart, int fsdcpLim, out int nFootnote)
	{
		int result = 0;
		try
		{
			TextParagraph obj = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(obj);
			obj.GetNumberFootnotes(fsdcpStart, fsdcpLim, out nFootnote);
		}
		catch (Exception callbackException)
		{
			nFootnote = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			nFootnote = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal unsafe int GetFootnotes(nint pfsclient, nint nmp, int fsdcpStart, int fsdcpLim, int nFootnotes, nint* rgnmftn, int* rgdcp, out int cFootnotes)
	{
		cFootnotes = 0;
		return -10000;
	}

	internal int FormatDropCap(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, uint fswdir, int fSuppressTopSpace, out nint pfsdropc, out int fInMargin, out int dur, out int dvr, out int cPolygons, out int cVertices, out int durText)
	{
		pfsdropc = IntPtr.Zero;
		fInMargin = (dur = (dvr = (cPolygons = (cVertices = (durText = 0)))));
		return -10000;
	}

	internal unsafe int GetDropCapPolygons(nint pfsclient, nint pfsdropc, nint nmp, uint fswdir, int ncVertices, int nfspt, int* rgcVertices, out int ccVertices, PTS.FSPOINT* rgfspt, out int cfspt, out int fWrapThrough)
	{
		ccVertices = (cfspt = (fWrapThrough = 0));
		return -10000;
	}

	internal int DestroyDropCap(nint pfsclient, nint pfsdropc)
	{
		return -10000;
	}

	internal int FormatBottomText(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, uint fswdir, nint pfslineLast, int dvrLine, out nint pmcsclientOut)
	{
		int result = 0;
		try
		{
			TextParagraph textParagraph = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(textParagraph);
			if (PtsContext.HandleToObject(pfslineLast) is Line line)
			{
				PTS.ValidateHandle(line);
				textParagraph.FormatBottomText(iArea, fswdir, line, dvrLine, out pmcsclientOut);
			}
			else
			{
				Invariant.Assert(PtsContext.HandleToObject(pfslineLast) is LineBreakpoint);
				pmcsclientOut = IntPtr.Zero;
			}
		}
		catch (Exception callbackException)
		{
			pmcsclientOut = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			pmcsclientOut = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int FormatLine(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, int dcp, nint pbrlineIn, uint fswdir, int urStartLine, int durLine, int urStartTrack, int durTrack, int urPageLeftMargin, int fAllowHyphenation, int fClearOnLeft, int fClearOnRight, int fTreatAsFirstInPara, int fTreatAsLastInPara, int fSuppressTopSpace, out nint pfsline, out int dcpLine, out nint ppbrlineOut, out int fForcedBroken, out PTS.FSFLRES fsflres, out int dvrAscent, out int dvrDescent, out int urBBox, out int durBBox, out int dcpDepend, out int fReformatNeighborsAsLastLine)
	{
		int result = 0;
		try
		{
			TextParagraph obj = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(obj);
			TextParaClient textParaClient = PtsContext.HandleToObject(pfsparaclient) as TextParaClient;
			PTS.ValidateHandle(textParaClient);
			obj.FormatLine(textParaClient, iArea, dcp, pbrlineIn, fswdir, urStartLine, durLine, urStartTrack, durTrack, urPageLeftMargin, PTS.ToBoolean(fAllowHyphenation), PTS.ToBoolean(fClearOnLeft), PTS.ToBoolean(fClearOnRight), PTS.ToBoolean(fTreatAsFirstInPara), PTS.ToBoolean(fTreatAsLastInPara), PTS.ToBoolean(fSuppressTopSpace), out pfsline, out dcpLine, out ppbrlineOut, out fForcedBroken, out fsflres, out dvrAscent, out dvrDescent, out urBBox, out durBBox, out dcpDepend, out fReformatNeighborsAsLastLine);
		}
		catch (Exception callbackException)
		{
			pfsline = (ppbrlineOut = IntPtr.Zero);
			dcpLine = (fForcedBroken = (dvrAscent = (dvrDescent = (urBBox = (durBBox = (dcpDepend = (fReformatNeighborsAsLastLine = 0)))))));
			fsflres = PTS.FSFLRES.fsflrOutOfSpace;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			pfsline = (ppbrlineOut = IntPtr.Zero);
			dcpLine = (fForcedBroken = (dvrAscent = (dvrDescent = (urBBox = (durBBox = (dcpDepend = (fReformatNeighborsAsLastLine = 0)))))));
			fsflres = PTS.FSFLRES.fsflrOutOfSpace;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int FormatLineForced(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, int dcp, nint pbrlineIn, uint fswdir, int urStartLine, int durLine, int urStartTrack, int durTrack, int urPageLeftMargin, int fClearOnLeft, int fClearOnRight, int fTreatAsFirstInPara, int fTreatAsLastInPara, int fSuppressTopSpace, int dvrAvailable, out nint pfsline, out int dcpLine, out nint ppbrlineOut, out PTS.FSFLRES fsflres, out int dvrAscent, out int dvrDescent, out int urBBox, out int durBBox, out int dcpDepend)
	{
		int result = 0;
		try
		{
			TextParagraph obj = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(obj);
			TextParaClient textParaClient = PtsContext.HandleToObject(pfsparaclient) as TextParaClient;
			PTS.ValidateHandle(textParaClient);
			obj.FormatLine(textParaClient, iArea, dcp, pbrlineIn, fswdir, urStartLine, durLine, urStartTrack, durTrack, urPageLeftMargin, fAllowHyphenation: true, PTS.ToBoolean(fClearOnLeft), PTS.ToBoolean(fClearOnRight), PTS.ToBoolean(fTreatAsFirstInPara), PTS.ToBoolean(fTreatAsLastInPara), PTS.ToBoolean(fSuppressTopSpace), out pfsline, out dcpLine, out ppbrlineOut, out var _, out fsflres, out dvrAscent, out dvrDescent, out urBBox, out durBBox, out dcpDepend, out var _);
		}
		catch (Exception callbackException)
		{
			pfsline = (ppbrlineOut = IntPtr.Zero);
			dcpLine = (dvrAscent = (dvrDescent = (urBBox = (durBBox = (dcpDepend = 0)))));
			fsflres = PTS.FSFLRES.fsflrOutOfSpace;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			pfsline = (ppbrlineOut = IntPtr.Zero);
			dcpLine = (dvrAscent = (dvrDescent = (urBBox = (durBBox = (dcpDepend = 0)))));
			fsflres = PTS.FSFLRES.fsflrOutOfSpace;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal unsafe int FormatLineVariants(nint pfsclient, nint pfsparabreakingsession, int dcp, nint pbrlineIn, uint fswdir, int urStartLine, int durLine, int fAllowHyphenation, int fClearOnLeft, int fClearOnRight, int fTreatAsFirstInPara, int fTreatAsLastInPara, int fSuppressTopSpace, nint lineVariantRestriction, int nLineVariantsAlloc, PTS.FSLINEVARIANT* rgfslinevariant, out int nLineVariantsActual, out int iLineVariantBest)
	{
		int result = 0;
		try
		{
			OptimalBreakSession optimalBreakSession = PtsContext.HandleToObject(pfsparabreakingsession) as OptimalBreakSession;
			PTS.ValidateHandle(optimalBreakSession);
			TextLineBreak textLineBreak = null;
			if (pbrlineIn != IntPtr.Zero)
			{
				LineBreakRecord obj = PtsContext.HandleToObject(pbrlineIn) as LineBreakRecord;
				PTS.ValidateHandle(obj);
				textLineBreak = obj.TextLineBreak;
			}
			IList<TextBreakpoint> list = optimalBreakSession.TextParagraph.FormatLineVariants(optimalBreakSession.TextParaClient, optimalBreakSession.TextParagraphCache, optimalBreakSession.OptimalTextSource, dcp, textLineBreak, fswdir, urStartLine, durLine, PTS.ToBoolean(fAllowHyphenation), PTS.ToBoolean(fClearOnLeft), PTS.ToBoolean(fClearOnRight), PTS.ToBoolean(fTreatAsFirstInPara), PTS.ToBoolean(fTreatAsLastInPara), PTS.ToBoolean(fSuppressTopSpace), lineVariantRestriction, out iLineVariantBest);
			for (int i = 0; i < Math.Min(list.Count, nLineVariantsAlloc); i++)
			{
				TextBreakpoint textBreakpoint = list[i];
				LineBreakpoint lineBreakpoint = new LineBreakpoint(optimalBreakSession, textBreakpoint);
				TextLineBreak textLineBreak2 = textBreakpoint.GetTextLineBreak();
				if (textLineBreak2 != null)
				{
					LineBreakRecord lineBreakRecord = new LineBreakRecord(optimalBreakSession.PtsContext, textLineBreak2);
					rgfslinevariant[i].pfsbreakreclineclient = lineBreakRecord.Handle;
				}
				else
				{
					rgfslinevariant[i].pfsbreakreclineclient = IntPtr.Zero;
				}
				int dvrAscent = TextDpi.ToTextDpi(textBreakpoint.Baseline);
				int dvrDescent = TextDpi.ToTextDpi(textBreakpoint.Height - textBreakpoint.Baseline);
				optimalBreakSession.TextParagraph.CalcLineAscentDescent(dcp, ref dvrAscent, ref dvrDescent);
				rgfslinevariant[i].pfslineclient = lineBreakpoint.Handle;
				rgfslinevariant[i].dcpLine = textBreakpoint.Length;
				rgfslinevariant[i].fForceBroken = PTS.FromBoolean(textBreakpoint.IsTruncated);
				rgfslinevariant[i].fslres = optimalBreakSession.OptimalTextSource.GetFormatResultForBreakpoint(dcp, textBreakpoint);
				rgfslinevariant[i].dvrAscent = dvrAscent;
				rgfslinevariant[i].dvrDescent = dvrDescent;
				rgfslinevariant[i].fReformatNeighborsAsLastLine = 0;
				rgfslinevariant[i].ptsLinePenaltyInfo = textBreakpoint.GetTextPenaltyResource().Value;
			}
			nLineVariantsActual = list.Count;
		}
		catch (Exception callbackException)
		{
			nLineVariantsActual = 0;
			iLineVariantBest = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			nLineVariantsActual = 0;
			iLineVariantBest = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int ReconstructLineVariant(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, int dcpStart, nint pbrlineIn, int dcpLine, uint fswdir, int urStartLine, int durLine, int urStartTrack, int durTrack, int urPageLeftMargin, int fAllowHyphenation, int fClearOnLeft, int fClearOnRight, int fTreatAsFirstInPara, int fTreatAsLastInPara, int fSuppressTopSpace, out nint pfsline, out nint ppbrlineOut, out int fForcedBroken, out PTS.FSFLRES fsflres, out int dvrAscent, out int dvrDescent, out int urBBox, out int durBBox, out int dcpDepend, out int fReformatNeighborsAsLastLine)
	{
		int result = 0;
		try
		{
			TextParagraph obj = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(obj);
			TextParaClient textParaClient = PtsContext.HandleToObject(pfsparaclient) as TextParaClient;
			PTS.ValidateHandle(textParaClient);
			obj.ReconstructLineVariant(textParaClient, iArea, dcpStart, pbrlineIn, dcpLine, fswdir, urStartLine, durLine, urStartTrack, durTrack, urPageLeftMargin, PTS.ToBoolean(fAllowHyphenation), PTS.ToBoolean(fClearOnLeft), PTS.ToBoolean(fClearOnRight), PTS.ToBoolean(fTreatAsFirstInPara), PTS.ToBoolean(fTreatAsLastInPara), PTS.ToBoolean(fSuppressTopSpace), out pfsline, out dcpLine, out ppbrlineOut, out fForcedBroken, out fsflres, out dvrAscent, out dvrDescent, out urBBox, out durBBox, out dcpDepend, out fReformatNeighborsAsLastLine);
		}
		catch (Exception callbackException)
		{
			pfsline = (ppbrlineOut = IntPtr.Zero);
			dcpLine = (fForcedBroken = (dvrAscent = (dvrDescent = (urBBox = (durBBox = (dcpDepend = (fReformatNeighborsAsLastLine = 0)))))));
			fsflres = PTS.FSFLRES.fsflrOutOfSpace;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			pfsline = (ppbrlineOut = IntPtr.Zero);
			dcpLine = (fForcedBroken = (dvrAscent = (dvrDescent = (urBBox = (durBBox = (dcpDepend = (fReformatNeighborsAsLastLine = 0)))))));
			fsflres = PTS.FSFLRES.fsflrOutOfSpace;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int DestroyLine(nint pfsclient, nint pfsline)
	{
		((UnmanagedHandle)PtsContext.HandleToObject(pfsline)).Dispose();
		return 0;
	}

	internal int DuplicateLineBreakRecord(nint pfsclient, nint pbrlineIn, out nint pbrlineDup)
	{
		int result = 0;
		try
		{
			LineBreakRecord lineBreakRecord = PtsContext.HandleToObject(pbrlineIn) as LineBreakRecord;
			PTS.ValidateHandle(lineBreakRecord);
			pbrlineDup = lineBreakRecord.Clone().Handle;
		}
		catch (Exception callbackException)
		{
			pbrlineDup = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			pbrlineDup = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int DestroyLineBreakRecord(nint pfsclient, nint pbrlineIn)
	{
		int result = 0;
		try
		{
			LineBreakRecord obj = PtsContext.HandleToObject(pbrlineIn) as LineBreakRecord;
			PTS.ValidateHandle(obj);
			obj.Dispose();
		}
		catch (Exception callbackException)
		{
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int SnapGridVertical(nint pfsclient, uint fswdir, int vrMargin, int vrCurrent, out int vrNew)
	{
		vrNew = 0;
		return -10000;
	}

	internal int GetDvrSuppressibleBottomSpace(nint pfsclient, nint pfsparaclient, nint pfsline, uint fswdir, out int dvrSuppressible)
	{
		int result = 0;
		try
		{
			if (PtsContext.HandleToObject(pfsline) is Line line)
			{
				PTS.ValidateHandle(line);
				line.GetDvrSuppressibleBottomSpace(out dvrSuppressible);
			}
			else
			{
				dvrSuppressible = 0;
			}
		}
		catch (Exception callbackException)
		{
			dvrSuppressible = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			dvrSuppressible = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetDvrAdvance(nint pfsclient, nint pfsparaclient, nint nmp, int dcp, uint fswdir, out int dvr)
	{
		int result = 0;
		try
		{
			TextParagraph obj = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(obj);
			obj.GetDvrAdvance(dcp, fswdir, out dvr);
		}
		catch (Exception callbackException)
		{
			dvr = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			dvr = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int UpdGetChangeInText(nint pfsclient, nint nmp, out int dcpStart, out int ddcpOld, out int ddcpNew)
	{
		int result = 0;
		try
		{
			TextParagraph obj = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(obj);
			obj.UpdGetChangeInText(out dcpStart, out ddcpOld, out ddcpNew);
		}
		catch (Exception callbackException)
		{
			dcpStart = (ddcpOld = (ddcpNew = 0));
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			dcpStart = (ddcpOld = (ddcpNew = 0));
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int UpdGetDropCapChange(nint pfsclient, nint nmp, out int fChanged)
	{
		fChanged = 0;
		return -10000;
	}

	internal int FInterruptFormattingText(nint pfsclient, nint pfsparaclient, nint nmp, int dcp, int vr, out int fInterruptFormatting)
	{
		int result = 0;
		try
		{
			TextParagraph textParagraph = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(textParagraph);
			fInterruptFormatting = PTS.FromBoolean(textParagraph.InterruptFormatting(dcp, vr));
		}
		catch (Exception callbackException)
		{
			fInterruptFormatting = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fInterruptFormatting = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetTextParaCache(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, uint fswdir, int urStartLine, int durLine, int urStartTrack, int durTrack, int urPageLeftMargin, int fClearOnLeft, int fClearOnRight, int fSuppressTopSpace, out int fFound, out int dcpPara, out int urBBox, out int durBBox, out int dvrPara, out PTS.FSKCLEAR fskclear, out nint pmcsclientAfterPara, out int cLines, out int fOptimalLines, out int fOptimalLineDcpsCached, out int dvrMinLineHeight)
	{
		fFound = 0;
		dcpPara = (urBBox = (durBBox = (dvrPara = (cLines = (dvrMinLineHeight = (fOptimalLines = (fOptimalLineDcpsCached = 0)))))));
		pmcsclientAfterPara = IntPtr.Zero;
		fskclear = PTS.FSKCLEAR.fskclearNone;
		return 0;
	}

	internal unsafe int SetTextParaCache(nint pfsclient, nint pfsparaclient, nint nmp, int iArea, uint fswdir, int urStartLine, int durLine, int urStartTrack, int durTrack, int urPageLeftMargin, int fClearOnLeft, int fClearOnRight, int fSuppressTopSpace, int dcpPara, int urBBox, int durBBox, int dvrPara, PTS.FSKCLEAR fskclear, nint pmcsclientAfterPara, int cLines, int fOptimalLines, int* rgdcpOptimalLines, int dvrMinLineHeight)
	{
		return 0;
	}

	internal unsafe int GetOptimalLineDcpCache(nint pfsclient, int cLines, int* rgdcp)
	{
		return -10000;
	}

	internal int GetNumberAttachedObjectsBeforeTextLine(nint pfsclient, nint nmp, int dcpFirst, out int cAttachedObjects)
	{
		int num = 0;
		try
		{
			TextParagraph textParagraph = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(textParagraph);
			int lastDcpAttachedObjectBeforeLine = textParagraph.GetLastDcpAttachedObjectBeforeLine(dcpFirst);
			cAttachedObjects = textParagraph.GetAttachedObjectCount(dcpFirst, lastDcpAttachedObjectBeforeLine);
			return 0;
		}
		catch (Exception callbackException)
		{
			cAttachedObjects = 0;
			PtsContext.CallbackException = callbackException;
			return -100002;
		}
		catch
		{
			cAttachedObjects = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			return -100002;
		}
	}

	internal unsafe int GetAttachedObjectsBeforeTextLine(nint pfsclient, nint nmp, int dcpFirst, int nAttachedObjects, nint* rgnmpAttachedObject, int* rgidobj, int* rgdcpAnchor, out int cObjects, out int fEndOfParagraph)
	{
		int result = 0;
		try
		{
			TextParagraph obj = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(obj);
			int lastDcpAttachedObjectBeforeLine = obj.GetLastDcpAttachedObjectBeforeLine(dcpFirst);
			List<AttachedObject> attachedObjects = obj.GetAttachedObjects(dcpFirst, lastDcpAttachedObjectBeforeLine);
			for (int i = 0; i < attachedObjects.Count; i++)
			{
				if (attachedObjects[i] is FigureObject)
				{
					FigureObject figureObject = (FigureObject)attachedObjects[i];
					rgnmpAttachedObject[i] = figureObject.Para.Handle;
					rgdcpAnchor[i] = figureObject.Dcp;
					rgidobj[i] = -2;
				}
				else
				{
					FloaterObject floaterObject = (FloaterObject)attachedObjects[i];
					rgnmpAttachedObject[i] = floaterObject.Para.Handle;
					rgdcpAnchor[i] = floaterObject.Dcp;
					rgidobj[i] = FloaterParagraphId;
				}
			}
			cObjects = attachedObjects.Count;
			fEndOfParagraph = 0;
		}
		catch (Exception callbackException)
		{
			cObjects = 0;
			fEndOfParagraph = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			cObjects = 0;
			fEndOfParagraph = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetNumberAttachedObjectsInTextLine(nint pfsclient, nint pfsline, nint nmp, int dcpFirst, int dcpLim, int fFoundAttachedObjectsBeforeLine, int dcpMaxAnchorAttachedObjectBeforeLine, out int cAttachedObjects)
	{
		int num = 0;
		try
		{
			TextParagraph textParagraph = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(textParagraph);
			LineBase lineBase = PtsContext.HandleToObject(pfsline) as LineBase;
			if (lineBase == null)
			{
				LineBreakpoint obj = PtsContext.HandleToObject(pfsline) as LineBreakpoint;
				PTS.ValidateHandle(obj);
				lineBase = obj.OptimalBreakSession.OptimalTextSource;
			}
			if (lineBase.HasFigures || lineBase.HasFloaters)
			{
				int lastDcpAttachedObjectBeforeLine = textParagraph.GetLastDcpAttachedObjectBeforeLine(dcpFirst);
				cAttachedObjects = textParagraph.GetAttachedObjectCount(lastDcpAttachedObjectBeforeLine, dcpLim);
			}
			else
			{
				cAttachedObjects = 0;
			}
			return 0;
		}
		catch (Exception callbackException)
		{
			cAttachedObjects = 0;
			PtsContext.CallbackException = callbackException;
			return -100002;
		}
		catch
		{
			cAttachedObjects = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			return -100002;
		}
	}

	internal unsafe int GetAttachedObjectsInTextLine(nint pfsclient, nint pfsline, nint nmp, int dcpFirst, int dcpLim, int fFoundAttachedObjectsBeforeLine, int dcpMaxAnchorAttachedObjectBeforeLine, int nAttachedObjects, nint* rgnmpAttachedObject, int* rgidobj, int* rgdcpAnchor, out int cObjects)
	{
		int result = 0;
		try
		{
			TextParagraph obj = PtsContext.HandleToObject(nmp) as TextParagraph;
			PTS.ValidateHandle(obj);
			int lastDcpAttachedObjectBeforeLine = obj.GetLastDcpAttachedObjectBeforeLine(dcpFirst);
			List<AttachedObject> attachedObjects = obj.GetAttachedObjects(lastDcpAttachedObjectBeforeLine, dcpLim);
			for (int i = 0; i < attachedObjects.Count; i++)
			{
				if (attachedObjects[i] is FigureObject)
				{
					FigureObject figureObject = (FigureObject)attachedObjects[i];
					rgnmpAttachedObject[i] = figureObject.Para.Handle;
					rgdcpAnchor[i] = figureObject.Dcp;
					rgidobj[i] = -2;
				}
				else
				{
					FloaterObject floaterObject = (FloaterObject)attachedObjects[i];
					rgnmpAttachedObject[i] = floaterObject.Para.Handle;
					rgdcpAnchor[i] = floaterObject.Dcp;
					rgidobj[i] = FloaterParagraphId;
				}
			}
			cObjects = attachedObjects.Count;
		}
		catch (Exception callbackException)
		{
			cObjects = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			cObjects = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int UpdGetAttachedObjectChange(nint pfsclient, nint nmp, nint nmpObject, out PTS.FSKCHANGE fskchObject)
	{
		int result = 0;
		try
		{
			BaseParagraph obj = PtsContext.HandleToObject(nmpObject) as BaseParagraph;
			PTS.ValidateHandle(obj);
			obj.UpdGetParaChange(out fskchObject, out var _);
		}
		catch (Exception callbackException)
		{
			fskchObject = PTS.FSKCHANGE.fskchNone;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fskchObject = PTS.FSKCHANGE.fskchNone;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetDurFigureAnchor(nint pfsclient, nint pfsparaclient, nint pfsparaclientFigure, nint pfsline, nint nmpFigure, uint fswdir, nint pfsfmtlinein, out int dur)
	{
		int result = 0;
		try
		{
			if (PtsContext.HandleToObject(pfsline) is Line line)
			{
				PTS.ValidateHandle(line);
				FigureParagraph figureParagraph = PtsContext.HandleToObject(nmpFigure) as FigureParagraph;
				PTS.ValidateHandle(figureParagraph);
				line.GetDurFigureAnchor(figureParagraph, fswdir, out dur);
			}
			else
			{
				Invariant.Assert(condition: false);
				dur = 0;
			}
		}
		catch (Exception callbackException)
		{
			dur = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			dur = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetFloaterProperties(nint pfsclient, nint nmFloater, uint fswdirTrack, out PTS.FSFLOATERPROPS fsfloaterprops)
	{
		int result = 0;
		try
		{
			FloaterBaseParagraph obj = PtsContext.HandleToObject(nmFloater) as FloaterBaseParagraph;
			PTS.ValidateHandle(obj);
			obj.GetFloaterProperties(fswdirTrack, out fsfloaterprops);
		}
		catch (Exception callbackException)
		{
			fsfloaterprops = default(PTS.FSFLOATERPROPS);
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fsfloaterprops = default(PTS.FSFLOATERPROPS);
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int FormatFloaterContentFinite(nint pfsclient, nint pfsparaclient, nint pfsbrkFloaterContentIn, int fBreakRecordFromPreviousPage, nint nmFloater, nint pftnrej, int fEmptyOk, int fSuppressTopSpace, uint fswdirTrack, int fAtMaxWidth, int durAvailable, int dvrAvailable, PTS.FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstparaIn, out PTS.FSFMTR fsfmtr, out nint pfsFloatContent, out nint pbrkrecpara, out int durFloaterWidth, out int dvrFloaterHeight, out PTS.FSBBOX fsbbox, out int cPolygons, out int cVertices)
	{
		int result = 0;
		try
		{
			FloaterBaseParagraph obj = PtsContext.HandleToObject(nmFloater) as FloaterBaseParagraph;
			PTS.ValidateHandle(obj);
			FloaterBaseParaClient floaterBaseParaClient = PtsContext.HandleToObject(pfsparaclient) as FloaterBaseParaClient;
			PTS.ValidateHandle(floaterBaseParaClient);
			obj.FormatFloaterContentFinite(floaterBaseParaClient, pfsbrkFloaterContentIn, fBreakRecordFromPreviousPage, pftnrej, fEmptyOk, fSuppressTopSpace, fswdirTrack, fAtMaxWidth, durAvailable, dvrAvailable, fsksuppresshardbreakbeforefirstparaIn, out fsfmtr, out pfsFloatContent, out pbrkrecpara, out durFloaterWidth, out dvrFloaterHeight, out fsbbox, out cPolygons, out cVertices);
		}
		catch (Exception callbackException)
		{
			fsfmtr = default(PTS.FSFMTR);
			pfsFloatContent = (pbrkrecpara = IntPtr.Zero);
			durFloaterWidth = (dvrFloaterHeight = (cPolygons = (cVertices = 0)));
			fsbbox = default(PTS.FSBBOX);
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fsfmtr = default(PTS.FSFMTR);
			pfsFloatContent = (pbrkrecpara = IntPtr.Zero);
			durFloaterWidth = (dvrFloaterHeight = (cPolygons = (cVertices = 0)));
			fsbbox = default(PTS.FSBBOX);
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int FormatFloaterContentBottomless(nint pfsclient, nint pfsparaclient, nint nmFloater, int fSuppressTopSpace, uint fswdirTrack, int fAtMaxWidth, int durAvailable, int dvrAvailable, out PTS.FSFMTRBL fsfmtrbl, out nint pfsFloatContent, out int durFloaterWidth, out int dvrFloaterHeight, out PTS.FSBBOX fsbbox, out int cPolygons, out int cVertices)
	{
		int result = 0;
		try
		{
			FloaterBaseParagraph obj = PtsContext.HandleToObject(nmFloater) as FloaterBaseParagraph;
			PTS.ValidateHandle(obj);
			FloaterBaseParaClient floaterBaseParaClient = PtsContext.HandleToObject(pfsparaclient) as FloaterBaseParaClient;
			PTS.ValidateHandle(floaterBaseParaClient);
			obj.FormatFloaterContentBottomless(floaterBaseParaClient, fSuppressTopSpace, fswdirTrack, fAtMaxWidth, durAvailable, dvrAvailable, out fsfmtrbl, out pfsFloatContent, out durFloaterWidth, out dvrFloaterHeight, out fsbbox, out cPolygons, out cVertices);
		}
		catch (Exception callbackException)
		{
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			pfsFloatContent = IntPtr.Zero;
			durFloaterWidth = (dvrFloaterHeight = (cPolygons = (cVertices = 0)));
			fsbbox = default(PTS.FSBBOX);
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			pfsFloatContent = IntPtr.Zero;
			durFloaterWidth = (dvrFloaterHeight = (cPolygons = (cVertices = 0)));
			fsbbox = default(PTS.FSBBOX);
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int UpdateBottomlessFloaterContent(nint pfsFloaterContent, nint pfsparaclient, nint nmFloater, int fSuppressTopSpace, uint fswdirTrack, int fAtMaxWidth, int durAvailable, int dvrAvailable, out PTS.FSFMTRBL fsfmtrbl, out int durFloaterWidth, out int dvrFloaterHeight, out PTS.FSBBOX fsbbox, out int cPolygons, out int cVertices)
	{
		int result = 0;
		try
		{
			FloaterBaseParagraph obj = PtsContext.HandleToObject(nmFloater) as FloaterBaseParagraph;
			PTS.ValidateHandle(obj);
			FloaterBaseParaClient floaterBaseParaClient = PtsContext.HandleToObject(pfsparaclient) as FloaterBaseParaClient;
			PTS.ValidateHandle(floaterBaseParaClient);
			obj.UpdateBottomlessFloaterContent(floaterBaseParaClient, fSuppressTopSpace, fswdirTrack, fAtMaxWidth, durAvailable, dvrAvailable, pfsFloaterContent, out fsfmtrbl, out durFloaterWidth, out dvrFloaterHeight, out fsbbox, out cPolygons, out cVertices);
		}
		catch (Exception callbackException)
		{
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			durFloaterWidth = (dvrFloaterHeight = (cPolygons = (cVertices = 0)));
			fsbbox = default(PTS.FSBBOX);
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			durFloaterWidth = (dvrFloaterHeight = (cPolygons = (cVertices = 0)));
			fsbbox = default(PTS.FSBBOX);
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal unsafe int GetFloaterPolygons(nint pfsparaclient, nint pfsFloaterContent, nint nmFloater, uint fswdirTrack, int ncVertices, int nfspt, int* rgcVertices, out int ccVertices, PTS.FSPOINT* rgfspt, out int cfspt, out int fWrapThrough)
	{
		int result = 0;
		try
		{
			FloaterBaseParagraph obj = PtsContext.HandleToObject(nmFloater) as FloaterBaseParagraph;
			PTS.ValidateHandle(obj);
			FloaterBaseParaClient floaterBaseParaClient = PtsContext.HandleToObject(pfsparaclient) as FloaterBaseParaClient;
			PTS.ValidateHandle(floaterBaseParaClient);
			obj.GetFloaterPolygons(floaterBaseParaClient, fswdirTrack, ncVertices, nfspt, rgcVertices, out ccVertices, rgfspt, out cfspt, out fWrapThrough);
		}
		catch (Exception callbackException)
		{
			ccVertices = (cfspt = (fWrapThrough = 0));
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			ccVertices = (cfspt = (fWrapThrough = 0));
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int ClearUpdateInfoInFloaterContent(nint pfsFloaterContent)
	{
		if (PtsContext.IsValidHandle(pfsFloaterContent) && (PtsContext.HandleToObject(pfsFloaterContent) as FloaterBaseParaClient) is UIElementParaClient)
		{
			return 0;
		}
		return PTS.FsClearUpdateInfoInSubpage(Context, pfsFloaterContent);
	}

	internal int CompareFloaterContents(nint pfsFloaterContentOld, nint pfsFloaterContentNew, out PTS.FSCOMPRESULT fscmpr)
	{
		if (PtsContext.IsValidHandle(pfsFloaterContentOld) && PtsContext.IsValidHandle(pfsFloaterContentNew))
		{
			FloaterBaseParaClient floaterBaseParaClient = PtsContext.HandleToObject(pfsFloaterContentOld) as FloaterBaseParaClient;
			FloaterBaseParaClient floaterBaseParaClient2 = PtsContext.HandleToObject(pfsFloaterContentNew) as FloaterBaseParaClient;
			if (floaterBaseParaClient is UIElementParaClient && !(floaterBaseParaClient2 is UIElementParaClient))
			{
				fscmpr = PTS.FSCOMPRESULT.fscmprChangeInside;
				return 0;
			}
			if (floaterBaseParaClient2 is UIElementParaClient && !(floaterBaseParaClient is UIElementParaClient))
			{
				fscmpr = PTS.FSCOMPRESULT.fscmprChangeInside;
				return 0;
			}
			if (floaterBaseParaClient is UIElementParaClient && floaterBaseParaClient2 is UIElementParaClient)
			{
				if (((IntPtr)pfsFloaterContentOld).Equals(pfsFloaterContentNew))
				{
					fscmpr = PTS.FSCOMPRESULT.fscmprNoChange;
					return 0;
				}
				fscmpr = PTS.FSCOMPRESULT.fscmprChangeInside;
				return 0;
			}
		}
		return PTS.FsCompareSubpages(Context, pfsFloaterContentOld, pfsFloaterContentNew, out fscmpr);
	}

	internal int DestroyFloaterContent(nint pfsFloaterContent)
	{
		if (PtsContext.IsValidHandle(pfsFloaterContent) && (PtsContext.HandleToObject(pfsFloaterContent) as FloaterBaseParaClient) is UIElementParaClient)
		{
			return 0;
		}
		return PTS.FsDestroySubpage(Context, pfsFloaterContent);
	}

	internal int DuplicateFloaterContentBreakRecord(nint pfsclient, nint pfsbrkFloaterContent, out nint pfsbrkFloaterContentDup)
	{
		if (PtsContext.IsValidHandle(pfsbrkFloaterContent) && (PtsContext.HandleToObject(pfsbrkFloaterContent) as FloaterBaseParaClient) is UIElementParaClient)
		{
			Invariant.Assert(condition: false, "Embedded UIElement should not have break record");
		}
		return PTS.FsDuplicateSubpageBreakRecord(Context, pfsbrkFloaterContent, out pfsbrkFloaterContentDup);
	}

	internal int DestroyFloaterContentBreakRecord(nint pfsclient, nint pfsbrkFloaterContent)
	{
		if (PtsContext.IsValidHandle(pfsbrkFloaterContent) && (PtsContext.HandleToObject(pfsbrkFloaterContent) as FloaterBaseParaClient) is UIElementParaClient)
		{
			Invariant.Assert(condition: false, "Embedded UIElement should not have break record");
		}
		return PTS.FsDestroySubpageBreakRecord(Context, pfsbrkFloaterContent);
	}

	internal int GetFloaterContentColumnBalancingInfo(nint pfsFloaterContent, uint fswdir, out int nlines, out int dvrSumHeight, out int dvrMinHeight)
	{
		if (PtsContext.IsValidHandle(pfsFloaterContent))
		{
			FloaterBaseParaClient floaterBaseParaClient = PtsContext.HandleToObject(pfsFloaterContent) as FloaterBaseParaClient;
			if (floaterBaseParaClient is UIElementParaClient)
			{
				if (((BlockUIContainer)floaterBaseParaClient.Paragraph.Element).Child != null)
				{
					nlines = 1;
					UIElement child = ((BlockUIContainer)floaterBaseParaClient.Paragraph.Element).Child;
					dvrSumHeight = TextDpi.ToTextDpi(child.DesiredSize.Height);
					dvrMinHeight = TextDpi.ToTextDpi(child.DesiredSize.Height);
				}
				else
				{
					nlines = 0;
					dvrSumHeight = (dvrMinHeight = 0);
				}
				return 0;
			}
		}
		uint fswdir2;
		return PTS.FsGetSubpageColumnBalancingInfo(Context, pfsFloaterContent, out fswdir2, out nlines, out dvrSumHeight, out dvrMinHeight);
	}

	internal int GetFloaterContentNumberFootnotes(nint pfsFloaterContent, out int cftn)
	{
		if (PtsContext.IsValidHandle(pfsFloaterContent) && (PtsContext.HandleToObject(pfsFloaterContent) as FloaterBaseParaClient) is UIElementParaClient)
		{
			cftn = 0;
			return 0;
		}
		return PTS.FsGetNumberSubpageFootnotes(Context, pfsFloaterContent, out cftn);
	}

	internal int GetFloaterContentFootnoteInfo(nint pfsFloaterContent, uint fswdir, int nftn, int iftnFirst, ref PTS.FSFTNINFO fsftninf, out int iftnLim)
	{
		iftnLim = 0;
		return 0;
	}

	internal int TransferDisplayInfoInFloaterContent(nint pfsFloaterContentOld, nint pfsFloaterContentNew)
	{
		if (PtsContext.IsValidHandle(pfsFloaterContentOld) && PtsContext.IsValidHandle(pfsFloaterContentNew))
		{
			FloaterBaseParaClient obj = PtsContext.HandleToObject(pfsFloaterContentOld) as FloaterBaseParaClient;
			FloaterBaseParaClient floaterBaseParaClient = PtsContext.HandleToObject(pfsFloaterContentNew) as FloaterBaseParaClient;
			if (obj is UIElementParaClient || floaterBaseParaClient is UIElementParaClient)
			{
				return 0;
			}
		}
		return PTS.FsTransferDisplayInfoSubpage(PtsContext.Context, pfsFloaterContentOld, pfsFloaterContentNew);
	}

	internal int GetMCSClientAfterFloater(nint pfsclient, nint pfsparaclient, nint nmFloater, uint fswdirTrack, nint pmcsclientIn, out nint pmcsclientOut)
	{
		int result = 0;
		try
		{
			FloaterBaseParagraph obj = PtsContext.HandleToObject(nmFloater) as FloaterBaseParagraph;
			PTS.ValidateHandle(obj);
			MarginCollapsingState marginCollapsingState = null;
			if (pmcsclientIn != IntPtr.Zero)
			{
				marginCollapsingState = PtsContext.HandleToObject(pmcsclientIn) as MarginCollapsingState;
				PTS.ValidateHandle(marginCollapsingState);
			}
			obj.GetMCSClientAfterFloater(fswdirTrack, marginCollapsingState, out pmcsclientOut);
		}
		catch (Exception callbackException)
		{
			pmcsclientOut = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			pmcsclientOut = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetDvrUsedForFloater(nint pfsclient, nint pfsparaclient, nint nmFloater, uint fswdirTrack, nint pmcsclientIn, int dvrDisplaced, out int dvrUsed)
	{
		int result = 0;
		try
		{
			FloaterBaseParagraph obj = PtsContext.HandleToObject(nmFloater) as FloaterBaseParagraph;
			PTS.ValidateHandle(obj);
			MarginCollapsingState marginCollapsingState = null;
			if (pmcsclientIn != IntPtr.Zero)
			{
				marginCollapsingState = PtsContext.HandleToObject(pmcsclientIn) as MarginCollapsingState;
				PTS.ValidateHandle(marginCollapsingState);
			}
			obj.GetDvrUsedForFloater(fswdirTrack, marginCollapsingState, dvrDisplaced, out dvrUsed);
		}
		catch (Exception callbackException)
		{
			dvrUsed = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			dvrUsed = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int SubtrackCreateContext(nint pfsclient, nint pfsc, nint pfscbkobj, uint ffi, int idobj, out nint pfssobjc)
	{
		pfssobjc = idobj + _objectContextOffset;
		return 0;
	}

	internal int SubtrackDestroyContext(nint pfssobjc)
	{
		return 0;
	}

	internal int SubtrackFormatParaFinite(nint pfssobjc, nint pfsparaclient, nint pfsobjbrk, int fBreakRecordFromPreviousPage, nint nmp, int iArea, nint pftnrej, nint pfsgeom, int fEmptyOk, int fSuppressTopSpace, uint fswdir, ref PTS.FSRECT fsrcToFill, nint pmcsclientIn, PTS.FSKCLEAR fskclearIn, PTS.FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstparaIn, int fBreakInside, out PTS.FSFMTR fsfmtr, out nint pfspara, out nint pbrkrecpara, out int dvrUsed, out PTS.FSBBOX fsbbox, out nint pmcsclientOut, out PTS.FSKCLEAR fskclearOut, out int dvrTopSpace, out int fBreakInsidePossible)
	{
		int result = 0;
		fBreakInsidePossible = 0;
		try
		{
			ContainerParagraph obj = PtsContext.HandleToObject(nmp) as ContainerParagraph;
			PTS.ValidateHandle(obj);
			ContainerParaClient containerParaClient = PtsContext.HandleToObject(pfsparaclient) as ContainerParaClient;
			PTS.ValidateHandle(containerParaClient);
			MarginCollapsingState marginCollapsingState = null;
			if (pmcsclientIn != IntPtr.Zero)
			{
				marginCollapsingState = PtsContext.HandleToObject(pmcsclientIn) as MarginCollapsingState;
				PTS.ValidateHandle(marginCollapsingState);
			}
			obj.FormatParaFinite(containerParaClient, pfsobjbrk, fBreakRecordFromPreviousPage, iArea, pftnrej, pfsgeom, fEmptyOk, fSuppressTopSpace, fswdir, ref fsrcToFill, marginCollapsingState, fskclearIn, fsksuppresshardbreakbeforefirstparaIn, out fsfmtr, out pfspara, out pbrkrecpara, out dvrUsed, out fsbbox, out pmcsclientOut, out fskclearOut, out dvrTopSpace);
		}
		catch (Exception callbackException)
		{
			fsfmtr = default(PTS.FSFMTR);
			pfspara = (pbrkrecpara = (pmcsclientOut = IntPtr.Zero));
			dvrUsed = (dvrTopSpace = 0);
			fsbbox = default(PTS.FSBBOX);
			fskclearOut = PTS.FSKCLEAR.fskclearNone;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fsfmtr = default(PTS.FSFMTR);
			pfspara = (pbrkrecpara = (pmcsclientOut = IntPtr.Zero));
			dvrUsed = (dvrTopSpace = 0);
			fsbbox = default(PTS.FSBBOX);
			fskclearOut = PTS.FSKCLEAR.fskclearNone;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int SubtrackFormatParaBottomless(nint pfssobjc, nint pfsparaclient, nint nmp, int iArea, nint pfsgeom, int fSuppressTopSpace, uint fswdir, int urTrack, int durTrack, int vrTrack, nint pmcsclientIn, PTS.FSKCLEAR fskclearIn, int fInterruptable, out PTS.FSFMTRBL fsfmtrbl, out nint pfspara, out int dvrUsed, out PTS.FSBBOX fsbbox, out nint pmcsclientOut, out PTS.FSKCLEAR fskclearOut, out int dvrTopSpace, out int fPageBecomesUninterruptable)
	{
		int result = 0;
		try
		{
			ContainerParagraph obj = PtsContext.HandleToObject(nmp) as ContainerParagraph;
			PTS.ValidateHandle(obj);
			ContainerParaClient containerParaClient = PtsContext.HandleToObject(pfsparaclient) as ContainerParaClient;
			PTS.ValidateHandle(containerParaClient);
			MarginCollapsingState marginCollapsingState = null;
			if (pmcsclientIn != IntPtr.Zero)
			{
				marginCollapsingState = PtsContext.HandleToObject(pmcsclientIn) as MarginCollapsingState;
				PTS.ValidateHandle(marginCollapsingState);
			}
			obj.FormatParaBottomless(containerParaClient, iArea, pfsgeom, fSuppressTopSpace, fswdir, urTrack, durTrack, vrTrack, marginCollapsingState, fskclearIn, fInterruptable, out fsfmtrbl, out pfspara, out dvrUsed, out fsbbox, out pmcsclientOut, out fskclearOut, out dvrTopSpace, out fPageBecomesUninterruptable);
		}
		catch (Exception callbackException)
		{
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			pfspara = (pmcsclientOut = IntPtr.Zero);
			dvrUsed = (dvrTopSpace = (fPageBecomesUninterruptable = 0));
			fsbbox = default(PTS.FSBBOX);
			fskclearOut = PTS.FSKCLEAR.fskclearNone;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			pfspara = (pmcsclientOut = IntPtr.Zero);
			dvrUsed = (dvrTopSpace = (fPageBecomesUninterruptable = 0));
			fsbbox = default(PTS.FSBBOX);
			fskclearOut = PTS.FSKCLEAR.fskclearNone;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int SubtrackUpdateBottomlessPara(nint pfspara, nint pfsparaclient, nint nmp, int iArea, nint pfsgeom, int fSuppressTopSpace, uint fswdir, int urTrack, int durTrack, int vrTrack, nint pmcsclientIn, PTS.FSKCLEAR fskclearIn, int fInterruptable, out PTS.FSFMTRBL fsfmtrbl, out int dvrUsed, out PTS.FSBBOX fsbbox, out nint pmcsclientOut, out PTS.FSKCLEAR fskclearOut, out int dvrTopSpace, out int fPageBecomesUninterruptable)
	{
		int result = 0;
		try
		{
			ContainerParagraph obj = PtsContext.HandleToObject(nmp) as ContainerParagraph;
			PTS.ValidateHandle(obj);
			ContainerParaClient containerParaClient = PtsContext.HandleToObject(pfsparaclient) as ContainerParaClient;
			PTS.ValidateHandle(containerParaClient);
			MarginCollapsingState marginCollapsingState = null;
			if (pmcsclientIn != IntPtr.Zero)
			{
				marginCollapsingState = PtsContext.HandleToObject(pmcsclientIn) as MarginCollapsingState;
				PTS.ValidateHandle(marginCollapsingState);
			}
			obj.UpdateBottomlessPara(pfspara, containerParaClient, iArea, pfsgeom, fSuppressTopSpace, fswdir, urTrack, durTrack, vrTrack, marginCollapsingState, fskclearIn, fInterruptable, out fsfmtrbl, out dvrUsed, out fsbbox, out pmcsclientOut, out fskclearOut, out dvrTopSpace, out fPageBecomesUninterruptable);
		}
		catch (Exception callbackException)
		{
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			pfspara = (pmcsclientOut = IntPtr.Zero);
			dvrUsed = (dvrTopSpace = (fPageBecomesUninterruptable = 0));
			fsbbox = default(PTS.FSBBOX);
			fskclearOut = PTS.FSKCLEAR.fskclearNone;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			pfspara = (pmcsclientOut = IntPtr.Zero);
			dvrUsed = (dvrTopSpace = (fPageBecomesUninterruptable = 0));
			fsbbox = default(PTS.FSBBOX);
			fskclearOut = PTS.FSKCLEAR.fskclearNone;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int SubtrackSynchronizeBottomlessPara(nint pfspara, nint pfsparaclient, nint pfsgeom, uint fswdir, int dvrShift)
	{
		int result = 0;
		try
		{
			PTS.ValidateHandle(PtsContext.HandleToObject(pfsparaclient) as ContainerParaClient);
			PTS.Validate(PTS.FsSynchronizeBottomlessSubtrack(Context, pfspara, pfsgeom, fswdir, dvrShift), PtsContext);
		}
		catch (Exception callbackException)
		{
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int SubtrackComparePara(nint pfsparaclientOld, nint pfsparaOld, nint pfsparaclientNew, nint pfsparaNew, uint fswdir, out PTS.FSCOMPRESULT fscmpr, out int dvrShifted)
	{
		return PTS.FsCompareSubtrack(Context, pfsparaOld, pfsparaNew, fswdir, out fscmpr, out dvrShifted);
	}

	internal int SubtrackClearUpdateInfoInPara(nint pfspara)
	{
		return PTS.FsClearUpdateInfoInSubtrack(Context, pfspara);
	}

	internal int SubtrackDestroyPara(nint pfspara)
	{
		return PTS.FsDestroySubtrack(Context, pfspara);
	}

	internal int SubtrackDuplicateBreakRecord(nint pfssobjc, nint pfsbrkrecparaOrig, out nint pfsbrkrecparaDup)
	{
		return PTS.FsDuplicateSubtrackBreakRecord(Context, pfsbrkrecparaOrig, out pfsbrkrecparaDup);
	}

	internal int SubtrackDestroyBreakRecord(nint pfssobjc, nint pfsobjbrk)
	{
		return PTS.FsDestroySubtrackBreakRecord(Context, pfsobjbrk);
	}

	internal int SubtrackGetColumnBalancingInfo(nint pfspara, uint fswdir, out int nlines, out int dvrSumHeight, out int dvrMinHeight)
	{
		return PTS.FsGetSubtrackColumnBalancingInfo(Context, pfspara, fswdir, out nlines, out dvrSumHeight, out dvrMinHeight);
	}

	internal int SubtrackGetNumberFootnotes(nint pfspara, out int nftn)
	{
		return PTS.FsGetNumberSubtrackFootnotes(Context, pfspara, out nftn);
	}

	internal unsafe int SubtrackGetFootnoteInfo(nint pfspara, uint fswdir, int nftn, int iftnFirst, PTS.FSFTNINFO* pfsftninf, out int iftnLim)
	{
		iftnLim = 0;
		return -10000;
	}

	internal int SubtrackShiftVertical(nint pfspara, nint pfsparaclient, nint pfsshift, uint fswdir, out PTS.FSBBOX pfsbbox)
	{
		pfsbbox = default(PTS.FSBBOX);
		return 0;
	}

	internal int SubtrackTransferDisplayInfoPara(nint pfsparaOld, nint pfsparaNew)
	{
		return PTS.FsTransferDisplayInfoSubtrack(Context, pfsparaOld, pfsparaNew);
	}

	internal int SubpageCreateContext(nint pfsclient, nint pfsc, nint pfscbkobj, uint ffi, int idobj, out nint pfssobjc)
	{
		pfssobjc = idobj + _objectContextOffset + 1;
		return 0;
	}

	internal int SubpageDestroyContext(nint pfssobjc)
	{
		return 0;
	}

	internal int SubpageFormatParaFinite(nint pfssobjc, nint pfsparaclient, nint pfsobjbrk, int fBreakRecordFromPreviousPage, nint nmp, int iArea, nint pftnrej, nint pfsgeom, int fEmptyOk, int fSuppressTopSpace, uint fswdir, ref PTS.FSRECT fsrcToFill, nint pmcsclientIn, PTS.FSKCLEAR fskclearIn, PTS.FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstparaIn, int fBreakInside, out PTS.FSFMTR fsfmtr, out nint pfspara, out nint pbrkrecpara, out int dvrUsed, out PTS.FSBBOX fsbbox, out nint pmcsclientOut, out PTS.FSKCLEAR fskclearOut, out int dvrTopSpace, out int fBreakInsidePossible)
	{
		int result = 0;
		fBreakInsidePossible = 0;
		try
		{
			SubpageParagraph obj = PtsContext.HandleToObject(nmp) as SubpageParagraph;
			PTS.ValidateHandle(obj);
			SubpageParaClient subpageParaClient = PtsContext.HandleToObject(pfsparaclient) as SubpageParaClient;
			PTS.ValidateHandle(subpageParaClient);
			MarginCollapsingState marginCollapsingState = null;
			if (pmcsclientIn != IntPtr.Zero)
			{
				marginCollapsingState = PtsContext.HandleToObject(pmcsclientIn) as MarginCollapsingState;
				PTS.ValidateHandle(marginCollapsingState);
			}
			obj.FormatParaFinite(subpageParaClient, pfsobjbrk, fBreakRecordFromPreviousPage, pftnrej, fEmptyOk, fSuppressTopSpace, fswdir, ref fsrcToFill, marginCollapsingState, fskclearIn, fsksuppresshardbreakbeforefirstparaIn, out fsfmtr, out pfspara, out pbrkrecpara, out dvrUsed, out fsbbox, out pmcsclientOut, out fskclearOut, out dvrTopSpace);
		}
		catch (Exception callbackException)
		{
			fsfmtr = default(PTS.FSFMTR);
			pfspara = (pbrkrecpara = (pmcsclientOut = IntPtr.Zero));
			dvrUsed = (dvrTopSpace = 0);
			fsbbox = default(PTS.FSBBOX);
			fskclearOut = PTS.FSKCLEAR.fskclearNone;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fsfmtr = default(PTS.FSFMTR);
			pfspara = (pbrkrecpara = (pmcsclientOut = IntPtr.Zero));
			dvrUsed = (dvrTopSpace = 0);
			fsbbox = default(PTS.FSBBOX);
			fskclearOut = PTS.FSKCLEAR.fskclearNone;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int SubpageFormatParaBottomless(nint pfssobjc, nint pfsparaclient, nint nmp, int iArea, nint pfsgeom, int fSuppressTopSpace, uint fswdir, int urTrack, int durTrack, int vrTrack, nint pmcsclientIn, PTS.FSKCLEAR fskclearIn, int fInterruptable, out PTS.FSFMTRBL fsfmtrbl, out nint pfspara, out int dvrUsed, out PTS.FSBBOX fsbbox, out nint pmcsclientOut, out PTS.FSKCLEAR fskclearOut, out int dvrTopSpace, out int fPageBecomesUninterruptable)
	{
		int result = 0;
		try
		{
			SubpageParagraph obj = PtsContext.HandleToObject(nmp) as SubpageParagraph;
			PTS.ValidateHandle(obj);
			SubpageParaClient subpageParaClient = PtsContext.HandleToObject(pfsparaclient) as SubpageParaClient;
			PTS.ValidateHandle(subpageParaClient);
			MarginCollapsingState marginCollapsingState = null;
			if (pmcsclientIn != IntPtr.Zero)
			{
				marginCollapsingState = PtsContext.HandleToObject(pmcsclientIn) as MarginCollapsingState;
				PTS.ValidateHandle(marginCollapsingState);
			}
			obj.FormatParaBottomless(subpageParaClient, fSuppressTopSpace, fswdir, urTrack, durTrack, vrTrack, marginCollapsingState, fskclearIn, fInterruptable, out fsfmtrbl, out pfspara, out dvrUsed, out fsbbox, out pmcsclientOut, out fskclearOut, out dvrTopSpace, out fPageBecomesUninterruptable);
		}
		catch (Exception callbackException)
		{
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			pfspara = (pmcsclientOut = IntPtr.Zero);
			dvrUsed = (dvrTopSpace = (fPageBecomesUninterruptable = 0));
			fsbbox = default(PTS.FSBBOX);
			fskclearOut = PTS.FSKCLEAR.fskclearNone;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			pfspara = (pmcsclientOut = IntPtr.Zero);
			dvrUsed = (dvrTopSpace = (fPageBecomesUninterruptable = 0));
			fsbbox = default(PTS.FSBBOX);
			fskclearOut = PTS.FSKCLEAR.fskclearNone;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int SubpageUpdateBottomlessPara(nint pfspara, nint pfsparaclient, nint nmp, int iArea, nint pfsgeom, int fSuppressTopSpace, uint fswdir, int urTrack, int durTrack, int vrTrack, nint pmcsclientIn, PTS.FSKCLEAR fskclearIn, int fInterruptable, out PTS.FSFMTRBL fsfmtrbl, out int dvrUsed, out PTS.FSBBOX fsbbox, out nint pmcsclientOut, out PTS.FSKCLEAR fskclearOut, out int dvrTopSpace, out int fPageBecomesUninterruptable)
	{
		int result = 0;
		try
		{
			SubpageParagraph obj = PtsContext.HandleToObject(nmp) as SubpageParagraph;
			PTS.ValidateHandle(obj);
			SubpageParaClient subpageParaClient = PtsContext.HandleToObject(pfsparaclient) as SubpageParaClient;
			PTS.ValidateHandle(subpageParaClient);
			MarginCollapsingState marginCollapsingState = null;
			if (pmcsclientIn != IntPtr.Zero)
			{
				marginCollapsingState = PtsContext.HandleToObject(pmcsclientIn) as MarginCollapsingState;
				PTS.ValidateHandle(marginCollapsingState);
			}
			obj.UpdateBottomlessPara(pfspara, subpageParaClient, fSuppressTopSpace, fswdir, urTrack, durTrack, vrTrack, marginCollapsingState, fskclearIn, fInterruptable, out fsfmtrbl, out dvrUsed, out fsbbox, out pmcsclientOut, out fskclearOut, out dvrTopSpace, out fPageBecomesUninterruptable);
		}
		catch (Exception callbackException)
		{
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			pfspara = (pmcsclientOut = IntPtr.Zero);
			dvrUsed = (dvrTopSpace = (fPageBecomesUninterruptable = 0));
			fsbbox = default(PTS.FSBBOX);
			fskclearOut = PTS.FSKCLEAR.fskclearNone;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			pfspara = (pmcsclientOut = IntPtr.Zero);
			dvrUsed = (dvrTopSpace = (fPageBecomesUninterruptable = 0));
			fsbbox = default(PTS.FSBBOX);
			fskclearOut = PTS.FSKCLEAR.fskclearNone;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int SubpageSynchronizeBottomlessPara(nint pfspara, nint pfsparaclient, nint pfsgeom, uint fswdir, int dvrShift)
	{
		return 0;
	}

	internal int SubpageComparePara(nint pfsparaclientOld, nint pfsparaOld, nint pfsparaclientNew, nint pfsparaNew, uint fswdir, out PTS.FSCOMPRESULT fscmpr, out int dvrShifted)
	{
		dvrShifted = 0;
		return PTS.FsCompareSubpages(Context, pfsparaOld, pfsparaNew, out fscmpr);
	}

	internal int SubpageClearUpdateInfoInPara(nint pfspara)
	{
		return PTS.FsClearUpdateInfoInSubpage(Context, pfspara);
	}

	internal int SubpageDestroyPara(nint pfspara)
	{
		return PTS.FsDestroySubpage(Context, pfspara);
	}

	internal int SubpageDuplicateBreakRecord(nint pfssobjc, nint pfsbrkrecparaOrig, out nint pfsbrkrecparaDup)
	{
		return PTS.FsDuplicateSubpageBreakRecord(Context, pfsbrkrecparaOrig, out pfsbrkrecparaDup);
	}

	internal int SubpageDestroyBreakRecord(nint pfssobjc, nint pfsobjbrk)
	{
		return PTS.FsDestroySubpageBreakRecord(Context, pfsobjbrk);
	}

	internal int SubpageGetColumnBalancingInfo(nint pfspara, uint fswdir, out int nlines, out int dvrSumHeight, out int dvrMinHeight)
	{
		return PTS.FsGetSubpageColumnBalancingInfo(Context, pfspara, out fswdir, out nlines, out dvrSumHeight, out dvrMinHeight);
	}

	internal int SubpageGetNumberFootnotes(nint pfspara, out int nftn)
	{
		return PTS.FsGetNumberSubpageFootnotes(Context, pfspara, out nftn);
	}

	internal unsafe int SubpageGetFootnoteInfo(nint pfspara, uint fswdir, int nftn, int iftnFirst, PTS.FSFTNINFO* pfsftninf, out int iftnLim)
	{
		return PTS.FsGetSubpageFootnoteInfo(Context, pfspara, nftn, iftnFirst, out fswdir, pfsftninf, out iftnLim);
	}

	internal int SubpageShiftVertical(nint pfspara, nint pfsparaclient, nint pfsshift, uint fswdir, out PTS.FSBBOX pfsbbox)
	{
		pfsbbox = default(PTS.FSBBOX);
		return 0;
	}

	internal int SubpageTransferDisplayInfoPara(nint pfsparaOld, nint pfsparaNew)
	{
		return PTS.FsTransferDisplayInfoSubpage(Context, pfsparaOld, pfsparaNew);
	}

	internal int GetTableProperties(nint pfsclient, nint nmTable, uint fswdirTrack, out PTS.FSTABLEOBJPROPS fstableobjprops)
	{
		int result = 0;
		try
		{
			TableParagraph obj = PtsContext.HandleToObject(nmTable) as TableParagraph;
			PTS.ValidateHandle(obj);
			obj.GetTableProperties(fswdirTrack, out fstableobjprops);
		}
		catch (Exception callbackException)
		{
			fstableobjprops = default(PTS.FSTABLEOBJPROPS);
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fstableobjprops = default(PTS.FSTABLEOBJPROPS);
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int AutofitTable(nint pfsclient, nint pfsparaclientTable, nint nmTable, uint fswdirTrack, int durAvailableSpace, out int durTableWidth)
	{
		int result = 0;
		try
		{
			TableParaClient obj = PtsContext.HandleToObject(pfsparaclientTable) as TableParaClient;
			PTS.ValidateHandle(obj);
			obj.AutofitTable(fswdirTrack, durAvailableSpace, out durTableWidth);
		}
		catch (Exception callbackException)
		{
			durTableWidth = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			durTableWidth = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int UpdAutofitTable(nint pfsclient, nint pfsparaclientTable, nint nmTable, uint fswdirTrack, int durAvailableSpace, out int durTableWidth, out int fNoChangeInCellWidths)
	{
		int result = 0;
		try
		{
			TableParaClient obj = PtsContext.HandleToObject(pfsparaclientTable) as TableParaClient;
			PTS.ValidateHandle(obj);
			obj.UpdAutofitTable(fswdirTrack, durAvailableSpace, out durTableWidth, out fNoChangeInCellWidths);
		}
		catch (Exception callbackException)
		{
			durTableWidth = 0;
			fNoChangeInCellWidths = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			durTableWidth = 0;
			fNoChangeInCellWidths = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetMCSClientAfterTable(nint pfsclient, nint pfsparaclientTable, nint nmTable, uint fswdirTrack, nint pmcsclientIn, out nint ppmcsclientOut)
	{
		int result = 0;
		try
		{
			TableParagraph obj = PtsContext.HandleToObject(nmTable) as TableParagraph;
			PTS.ValidateHandle(obj);
			obj.GetMCSClientAfterTable(fswdirTrack, pmcsclientIn, out ppmcsclientOut);
		}
		catch (Exception callbackException)
		{
			ppmcsclientOut = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			ppmcsclientOut = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetFirstHeaderRow(nint pfsclient, nint nmTable, int fRepeatedHeader, out int fFound, out nint pnmFirstHeaderRow)
	{
		int result = 0;
		try
		{
			TableParagraph obj = PtsContext.HandleToObject(nmTable) as TableParagraph;
			PTS.ValidateHandle(obj);
			obj.GetFirstHeaderRow(fRepeatedHeader, out fFound, out pnmFirstHeaderRow);
		}
		catch (Exception callbackException)
		{
			fFound = 0;
			pnmFirstHeaderRow = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fFound = 0;
			pnmFirstHeaderRow = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetNextHeaderRow(nint pfsclient, nint nmTable, nint nmHeaderRow, int fRepeatedHeader, out int fFound, out nint pnmNextHeaderRow)
	{
		int result = 0;
		try
		{
			TableParagraph obj = PtsContext.HandleToObject(nmTable) as TableParagraph;
			PTS.ValidateHandle(obj);
			obj.GetNextHeaderRow(fRepeatedHeader, nmHeaderRow, out fFound, out pnmNextHeaderRow);
		}
		catch (Exception callbackException)
		{
			fFound = 0;
			pnmNextHeaderRow = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fFound = 0;
			pnmNextHeaderRow = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetFirstFooterRow(nint pfsclient, nint nmTable, int fRepeatedFooter, out int fFound, out nint pnmFirstFooterRow)
	{
		int result = 0;
		try
		{
			TableParagraph obj = PtsContext.HandleToObject(nmTable) as TableParagraph;
			PTS.ValidateHandle(obj);
			obj.GetFirstFooterRow(fRepeatedFooter, out fFound, out pnmFirstFooterRow);
		}
		catch (Exception callbackException)
		{
			fFound = 0;
			pnmFirstFooterRow = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fFound = 0;
			pnmFirstFooterRow = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetNextFooterRow(nint pfsclient, nint nmTable, nint nmFooterRow, int fRepeatedFooter, out int fFound, out nint pnmNextFooterRow)
	{
		int result = 0;
		try
		{
			TableParagraph obj = PtsContext.HandleToObject(nmTable) as TableParagraph;
			PTS.ValidateHandle(obj);
			obj.GetNextFooterRow(fRepeatedFooter, nmFooterRow, out fFound, out pnmNextFooterRow);
		}
		catch (Exception callbackException)
		{
			fFound = 0;
			pnmNextFooterRow = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fFound = 0;
			pnmNextFooterRow = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetFirstRow(nint pfsclient, nint nmTable, out int fFound, out nint pnmFirstRow)
	{
		int result = 0;
		try
		{
			TableParagraph obj = PtsContext.HandleToObject(nmTable) as TableParagraph;
			PTS.ValidateHandle(obj);
			obj.GetFirstRow(out fFound, out pnmFirstRow);
		}
		catch (Exception callbackException)
		{
			fFound = 0;
			pnmFirstRow = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fFound = 0;
			pnmFirstRow = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetNextRow(nint pfsclient, nint nmTable, nint nmRow, out int fFound, out nint pnmNextRow)
	{
		int result = 0;
		try
		{
			TableParagraph obj = PtsContext.HandleToObject(nmTable) as TableParagraph;
			PTS.ValidateHandle(obj);
			obj.GetNextRow(nmRow, out fFound, out pnmNextRow);
		}
		catch (Exception callbackException)
		{
			fFound = 0;
			pnmNextRow = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fFound = 0;
			pnmNextRow = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int UpdFChangeInHeaderFooter(nint pfsclient, nint nmTable, out int fHeaderChanged, out int fFooterChanged, out int fRepeatedHeaderChanged, out int fRepeatedFooterChanged)
	{
		int result = 0;
		try
		{
			TableParagraph obj = PtsContext.HandleToObject(nmTable) as TableParagraph;
			PTS.ValidateHandle(obj);
			obj.UpdFChangeInHeaderFooter(out fHeaderChanged, out fFooterChanged, out fRepeatedHeaderChanged, out fRepeatedFooterChanged);
		}
		catch (Exception callbackException)
		{
			fHeaderChanged = 0;
			fFooterChanged = 0;
			fRepeatedHeaderChanged = 0;
			fRepeatedFooterChanged = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fHeaderChanged = 0;
			fFooterChanged = 0;
			fRepeatedHeaderChanged = 0;
			fRepeatedFooterChanged = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int UpdGetFirstChangeInTable(nint pfsclient, nint nmTable, out int fFound, out int fChangeFirst, out nint pnmRowBeforeChange)
	{
		int result = 0;
		try
		{
			TableParagraph obj = PtsContext.HandleToObject(nmTable) as TableParagraph;
			PTS.ValidateHandle(obj);
			obj.UpdGetFirstChangeInTable(out fFound, out fChangeFirst, out pnmRowBeforeChange);
		}
		catch (Exception callbackException)
		{
			fFound = 0;
			fChangeFirst = 0;
			pnmRowBeforeChange = IntPtr.Zero;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fFound = 0;
			fChangeFirst = 0;
			pnmRowBeforeChange = IntPtr.Zero;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int UpdGetRowChange(nint pfsclient, nint nmTable, nint nmRow, out PTS.FSKCHANGE fskch, out int fNoFurtherChanges)
	{
		int result = 0;
		try
		{
			PTS.ValidateHandle(PtsContext.HandleToObject(nmTable) as TableParagraph);
			RowParagraph obj = PtsContext.HandleToObject(nmRow) as RowParagraph;
			PTS.ValidateHandle(obj);
			obj.UpdGetParaChange(out fskch, out fNoFurtherChanges);
		}
		catch (Exception callbackException)
		{
			fskch = PTS.FSKCHANGE.fskchNone;
			fNoFurtherChanges = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fskch = PTS.FSKCHANGE.fskchNone;
			fNoFurtherChanges = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int UpdGetCellChange(nint pfsclient, nint nmRow, nint nmCell, out int fWidthChanged, out PTS.FSKCHANGE fskchCell)
	{
		int result = 0;
		try
		{
			CellParagraph obj = PtsContext.HandleToObject(nmCell) as CellParagraph;
			PTS.ValidateHandle(obj);
			obj.UpdGetCellChange(out fWidthChanged, out fskchCell);
		}
		catch (Exception callbackException)
		{
			fWidthChanged = 0;
			fskchCell = PTS.FSKCHANGE.fskchNone;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fWidthChanged = 0;
			fskchCell = PTS.FSKCHANGE.fskchNone;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetDistributionKind(nint pfsclient, nint nmTable, uint fswdirTable, out PTS.FSKTABLEHEIGHTDISTRIBUTION tabledistr)
	{
		int result = 0;
		try
		{
			TableParagraph obj = PtsContext.HandleToObject(nmTable) as TableParagraph;
			PTS.ValidateHandle(obj);
			obj.GetDistributionKind(fswdirTable, out tabledistr);
		}
		catch (Exception callbackException)
		{
			tabledistr = PTS.FSKTABLEHEIGHTDISTRIBUTION.fskdistributeUnchanged;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			tabledistr = PTS.FSKTABLEHEIGHTDISTRIBUTION.fskdistributeUnchanged;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetRowProperties(nint pfsclient, nint nmRow, uint fswdirTable, out PTS.FSTABLEROWPROPS rowprops)
	{
		int result = 0;
		try
		{
			RowParagraph obj = PtsContext.HandleToObject(nmRow) as RowParagraph;
			PTS.ValidateHandle(obj);
			obj.GetRowProperties(fswdirTable, out rowprops);
		}
		catch (Exception callbackException)
		{
			rowprops = default(PTS.FSTABLEROWPROPS);
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			rowprops = default(PTS.FSTABLEROWPROPS);
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal unsafe int GetCells(nint pfsclient, nint nmRow, int cCells, nint* rgnmCell, PTS.FSTABLEKCELLMERGE* rgkcellmerge)
	{
		int result = 0;
		try
		{
			RowParagraph obj = PtsContext.HandleToObject(nmRow) as RowParagraph;
			PTS.ValidateHandle(obj);
			obj.GetCells(cCells, rgnmCell, rgkcellmerge);
		}
		catch (Exception callbackException)
		{
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int FInterruptFormattingTable(nint pfsclient, nint pfsparaclient, nint nmRow, int dvr, out int fInterrupt)
	{
		int result = 0;
		try
		{
			RowParagraph obj = PtsContext.HandleToObject(nmRow) as RowParagraph;
			PTS.ValidateHandle(obj);
			obj.FInterruptFormattingTable(dvr, out fInterrupt);
		}
		catch (Exception callbackException)
		{
			fInterrupt = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fInterrupt = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal unsafe int CalcHorizontalBBoxOfRow(nint pfsclient, nint nmRow, int cCells, nint* rgnmCell, nint* rgpfscell, out int urBBox, out int durBBox)
	{
		int result = 0;
		try
		{
			RowParagraph obj = PtsContext.HandleToObject(nmRow) as RowParagraph;
			PTS.ValidateHandle(obj);
			obj.CalcHorizontalBBoxOfRow(cCells, rgnmCell, rgpfscell, out urBBox, out durBBox);
		}
		catch (Exception callbackException)
		{
			urBBox = 0;
			durBBox = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			urBBox = 0;
			durBBox = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int FormatCellFinite(nint pfsclient, nint pfsparaclientTable, nint pfsbrkcell, nint nmCell, nint pfsFtnRejector, int fEmptyOK, uint fswdirTable, int dvrExtraHeight, int dvrAvailable, out PTS.FSFMTR pfmtr, out nint ppfscell, out nint pfsbrkcellOut, out int dvrUsed)
	{
		int result = 0;
		try
		{
			CellParagraph obj = PtsContext.HandleToObject(nmCell) as CellParagraph;
			PTS.ValidateHandle(obj);
			TableParaClient tableParaClient = PtsContext.HandleToObject(pfsparaclientTable) as TableParaClient;
			PTS.ValidateHandle(tableParaClient);
			obj.FormatCellFinite(tableParaClient, pfsbrkcell, pfsFtnRejector, fEmptyOK, fswdirTable, dvrAvailable, out pfmtr, out ppfscell, out pfsbrkcellOut, out dvrUsed);
		}
		catch (Exception callbackException)
		{
			pfmtr = default(PTS.FSFMTR);
			ppfscell = IntPtr.Zero;
			pfsbrkcellOut = IntPtr.Zero;
			dvrUsed = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			pfmtr = default(PTS.FSFMTR);
			ppfscell = IntPtr.Zero;
			pfsbrkcellOut = IntPtr.Zero;
			dvrUsed = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int FormatCellBottomless(nint pfsclient, nint pfsparaclientTable, nint nmCell, uint fswdirTable, out PTS.FSFMTRBL fmtrbl, out nint ppfscell, out int dvrUsed)
	{
		int result = 0;
		try
		{
			CellParagraph obj = PtsContext.HandleToObject(nmCell) as CellParagraph;
			PTS.ValidateHandle(obj);
			TableParaClient tableParaClient = PtsContext.HandleToObject(pfsparaclientTable) as TableParaClient;
			PTS.ValidateHandle(tableParaClient);
			obj.FormatCellBottomless(tableParaClient, fswdirTable, out fmtrbl, out ppfscell, out dvrUsed);
		}
		catch (Exception callbackException)
		{
			fmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			ppfscell = IntPtr.Zero;
			dvrUsed = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			ppfscell = IntPtr.Zero;
			dvrUsed = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int UpdateBottomlessCell(nint pfscell, nint pfsparaclientTable, nint nmCell, uint fswdirTable, out PTS.FSFMTRBL fmtrbl, out int dvrUsed)
	{
		int result = 0;
		try
		{
			CellParagraph obj = PtsContext.HandleToObject(nmCell) as CellParagraph;
			PTS.ValidateHandle(obj);
			CellParaClient cellParaClient = PtsContext.HandleToObject(pfscell) as CellParaClient;
			PTS.ValidateHandle(cellParaClient);
			TableParaClient tableParaClient = PtsContext.HandleToObject(pfsparaclientTable) as TableParaClient;
			PTS.ValidateHandle(tableParaClient);
			obj.UpdateBottomlessCell(cellParaClient, tableParaClient, fswdirTable, out fmtrbl, out dvrUsed);
		}
		catch (Exception callbackException)
		{
			fmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			dvrUsed = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			fmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			dvrUsed = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int CompareCells(nint pfscellOld, nint pfscellNew, out PTS.FSCOMPRESULT fscmpr)
	{
		fscmpr = PTS.FSCOMPRESULT.fscmprChangeInside;
		return 0;
	}

	internal int ClearUpdateInfoInCell(nint pfscell)
	{
		return 0;
	}

	internal int SetCellHeight(nint pfscell, nint pfsparaclientTable, nint pfsbrkcell, nint nmCell, int fBrokenHere, uint fswdirTable, int dvrActual)
	{
		int result = 0;
		try
		{
			CellParagraph obj = PtsContext.HandleToObject(nmCell) as CellParagraph;
			PTS.ValidateHandle(obj);
			CellParaClient cellParaClient = PtsContext.HandleToObject(pfscell) as CellParaClient;
			PTS.ValidateHandle(cellParaClient);
			TableParaClient tableParaClient = PtsContext.HandleToObject(pfsparaclientTable) as TableParaClient;
			PTS.ValidateHandle(tableParaClient);
			obj.SetCellHeight(cellParaClient, tableParaClient, pfsbrkcell, fBrokenHere, fswdirTable, dvrActual);
		}
		catch (Exception callbackException)
		{
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int DuplicateCellBreakRecord(nint pfsclient, nint pfsbrkcell, out nint ppfsbrkcellDup)
	{
		return PTS.FsDuplicateSubpageBreakRecord(Context, pfsbrkcell, out ppfsbrkcellDup);
	}

	internal int DestroyCellBreakRecord(nint pfsclient, nint pfsbrkcell)
	{
		return PTS.FsDestroySubpageBreakRecord(Context, pfsbrkcell);
	}

	internal int DestroyCell(nint pfsCell)
	{
		int result = 0;
		try
		{
			if (PtsContext.HandleToObject(pfsCell) is CellParaClient cellParaClient)
			{
				cellParaClient.Dispose();
			}
		}
		catch (Exception callbackException)
		{
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetCellNumberFootnotes(nint pfsCell, out int cFtn)
	{
		int result = 0;
		try
		{
			PTS.ValidateHandle(PtsContext.HandleToObject(pfsCell) as CellParaClient);
			cFtn = 0;
		}
		catch (Exception callbackException)
		{
			cFtn = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			cFtn = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int GetCellMinColumnBalancingStep(nint pfscell, uint fswdir, out int dvrMinStep)
	{
		int result = 0;
		try
		{
			PTS.ValidateHandle(PtsContext.HandleToObject(pfscell) as CellParaClient);
			dvrMinStep = TextDpi.ToTextDpi(1.0);
		}
		catch (Exception callbackException)
		{
			dvrMinStep = 0;
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			dvrMinStep = 0;
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}

	internal int TransferDisplayInfoCell(nint pfscellOld, nint pfscellNew)
	{
		int result = 0;
		try
		{
			CellParaClient cellParaClient = PtsContext.HandleToObject(pfscellOld) as CellParaClient;
			PTS.ValidateHandle(cellParaClient);
			CellParaClient obj = PtsContext.HandleToObject(pfscellNew) as CellParaClient;
			PTS.ValidateHandle(obj);
			obj.TransferDisplayInfo(cellParaClient);
		}
		catch (Exception callbackException)
		{
			PtsContext.CallbackException = callbackException;
			result = -100002;
		}
		catch
		{
			PtsContext.CallbackException = new Exception("Caught a non CLS Exception");
			result = -100002;
		}
		return result;
	}
}
