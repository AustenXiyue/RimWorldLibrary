using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal class PtsPage : IDisposable
{
	private static DispatcherOperationCallback BackgroundUpdateCallback = BackgroundFormatStatic;

	private readonly Section _section;

	private PageBreakRecord _breakRecord;

	private ContainerVisual _visual;

	private DispatcherOperation _backgroundFormatOperation;

	private Size _calculatedSize;

	private Size _contentSize;

	private PageContext _pageContextOfThisPage = new PageContext();

	private MS.Internal.SecurityCriticalDataForSet<nint> _ptsPage;

	private bool _finitePage;

	private bool _incrementalUpdate;

	internal bool _useSizingWorkaroundForTextBox;

	private int _disposed;

	internal PageBreakRecord BreakRecord => _breakRecord;

	internal Size CalculatedSize => _calculatedSize;

	internal Size ContentSize => _contentSize;

	internal bool FinitePage => _finitePage;

	internal PageContext PageContext => _pageContextOfThisPage;

	internal bool IncrementalUpdate => _incrementalUpdate;

	internal PtsContext PtsContext => _section.PtsContext;

	internal nint PageHandle => _ptsPage.Value;

	internal bool UseSizingWorkaroundForTextBox
	{
		get
		{
			return _useSizingWorkaroundForTextBox;
		}
		set
		{
			_useSizingWorkaroundForTextBox = value;
		}
	}

	private bool IsEmpty => _ptsPage.Value == IntPtr.Zero;

	internal PtsPage(Section section)
		: this()
	{
		_section = section;
	}

	private PtsPage()
	{
		_ptsPage = new MS.Internal.SecurityCriticalDataForSet<nint>(IntPtr.Zero);
	}

	~PtsPage()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	internal bool PrepareForBottomlessUpdate()
	{
		bool flag = !IsEmpty;
		if (!_section.CanUpdate)
		{
			flag = false;
		}
		else if (_section.StructuralCache != null)
		{
			if (_section.StructuralCache.ForceReformat)
			{
				flag = false;
				_section.StructuralCache.ClearUpdateInfo(destroyStructureCache: true);
			}
			else if (_section.StructuralCache.DtrList != null && !flag)
			{
				_section.InvalidateStructure();
				_section.StructuralCache.ClearUpdateInfo(destroyStructureCache: false);
			}
		}
		return flag;
	}

	internal bool PrepareForFiniteUpdate(PageBreakRecord breakRecord)
	{
		bool flag = !IsEmpty;
		if (_section.StructuralCache != null)
		{
			if (_section.StructuralCache.ForceReformat)
			{
				flag = false;
				_section.InvalidateStructure();
				_section.StructuralCache.ClearUpdateInfo(_section.StructuralCache.DestroyStructure);
			}
			else if (_section.StructuralCache.DtrList != null)
			{
				_section.InvalidateStructure();
				if (!flag)
				{
					_section.StructuralCache.ClearUpdateInfo(destroyStructureCache: false);
				}
			}
			else
			{
				flag = false;
				_section.StructuralCache.ClearUpdateInfo(destroyStructureCache: false);
			}
		}
		return flag;
	}

	internal IInputElement InputHitTest(Point p)
	{
		IInputElement result = null;
		if (!IsEmpty)
		{
			PTS.FSPOINT pt = TextDpi.ToTextPoint(p);
			result = InputHitTestPage(pt);
		}
		return result;
	}

	internal List<Rect> GetRectangles(ContentElement e, int start, int length)
	{
		List<Rect> result = new List<Rect>();
		if (!IsEmpty)
		{
			result = GetRectanglesInPage(e, start, length);
		}
		return result;
	}

	private static object BackgroundFormatStatic(object arg)
	{
		Invariant.Assert(arg is PtsPage);
		((PtsPage)arg).BackgroundFormat();
		return null;
	}

	private void BackgroundFormat()
	{
		FlowDocument formattingOwner = _section.StructuralCache.FormattingOwner;
		if (formattingOwner.Formatter is FlowDocumentFormatter)
		{
			_section.StructuralCache.BackgroundFormatInfo.BackgroundFormat(formattingOwner.BottomlessFormatter, ignoreThrottle: false);
		}
	}

	private void DeferFormattingToBackground()
	{
		int cPInterrupted = _section.StructuralCache.BackgroundFormatInfo.CPInterrupted;
		int cchAllText = _section.StructuralCache.BackgroundFormatInfo.CchAllText;
		DirtyTextRange dtr = new DirtyTextRange(cPInterrupted, cchAllText - cPInterrupted, cchAllText - cPInterrupted);
		_section.StructuralCache.AddDirtyTextRange(dtr);
		_backgroundFormatOperation = Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, BackgroundUpdateCallback, this);
	}

	internal void CreateBottomlessPage()
	{
		OnBeforeFormatPage(finitePage: false, incremental: false);
		if (TracePageFormatting.IsEnabled)
		{
			TracePageFormatting.Trace(TraceEventType.Start, TracePageFormatting.FormatPage, PageContext, PtsContext);
		}
		PTS.FSFMTRBL pfsfmtrbl;
		nint ppfspage;
		int num = PTS.FsCreatePageBottomless(PtsContext.Context, _section.Handle, out pfsfmtrbl, out ppfspage);
		if (num != 0)
		{
			_ptsPage.Value = IntPtr.Zero;
			PTS.ValidateAndTrace(num, PtsContext);
		}
		else
		{
			_ptsPage.Value = ppfspage;
		}
		if (TracePageFormatting.IsEnabled)
		{
			TracePageFormatting.Trace(TraceEventType.Stop, TracePageFormatting.FormatPage, PageContext, PtsContext);
		}
		OnAfterFormatPage(setSize: true, incremental: false);
		if (pfsfmtrbl == PTS.FSFMTRBL.fmtrblInterrupted)
		{
			DeferFormattingToBackground();
		}
	}

	internal void UpdateBottomlessPage()
	{
		if (!IsEmpty)
		{
			OnBeforeFormatPage(finitePage: false, incremental: true);
			if (TracePageFormatting.IsEnabled)
			{
				TracePageFormatting.Trace(TraceEventType.Start, TracePageFormatting.FormatPage, PageContext, PtsContext);
			}
			PTS.FSFMTRBL pfsfmtrbl;
			int num = PTS.FsUpdateBottomlessPage(PtsContext.Context, _ptsPage.Value, _section.Handle, out pfsfmtrbl);
			if (num != 0)
			{
				DestroyPage();
				PTS.ValidateAndTrace(num, PtsContext);
			}
			if (TracePageFormatting.IsEnabled)
			{
				TracePageFormatting.Trace(TraceEventType.Stop, TracePageFormatting.FormatPage, PageContext, PtsContext);
			}
			OnAfterFormatPage(setSize: true, incremental: true);
			if (pfsfmtrbl == PTS.FSFMTRBL.fmtrblInterrupted)
			{
				DeferFormattingToBackground();
			}
		}
	}

	internal void CreateFinitePage(PageBreakRecord breakRecord)
	{
		OnBeforeFormatPage(finitePage: true, incremental: false);
		if (TracePageFormatting.IsEnabled)
		{
			TracePageFormatting.Trace(TraceEventType.Start, TracePageFormatting.FormatPage, PageContext, PtsContext);
		}
		nint pfsBRPageStart = breakRecord?.BreakRecord ?? IntPtr.Zero;
		PTS.FSFMTR pfsfmtrOut;
		nint ppfsPageOut;
		nint ppfsBRPageOut;
		int num = PTS.FsCreatePageFinite(PtsContext.Context, pfsBRPageStart, _section.Handle, out pfsfmtrOut, out ppfsPageOut, out ppfsBRPageOut);
		if (num != 0)
		{
			_ptsPage.Value = IntPtr.Zero;
			ppfsBRPageOut = IntPtr.Zero;
			PTS.ValidateAndTrace(num, PtsContext);
		}
		else
		{
			_ptsPage.Value = ppfsPageOut;
		}
		if (ppfsBRPageOut != IntPtr.Zero && _section.StructuralCache != null)
		{
			_breakRecord = new PageBreakRecord(PtsContext, new MS.Internal.SecurityCriticalDataForSet<nint>(ppfsBRPageOut), (breakRecord == null) ? 1 : (breakRecord.PageNumber + 1));
		}
		if (TracePageFormatting.IsEnabled)
		{
			TracePageFormatting.Trace(TraceEventType.Stop, TracePageFormatting.FormatPage, PageContext, PtsContext);
		}
		OnAfterFormatPage(setSize: true, incremental: false);
	}

	internal void UpdateFinitePage(PageBreakRecord breakRecord)
	{
		if (!IsEmpty)
		{
			OnBeforeFormatPage(finitePage: true, incremental: true);
			if (TracePageFormatting.IsEnabled)
			{
				TracePageFormatting.Trace(TraceEventType.Start, TracePageFormatting.FormatPage, PageContext, PtsContext);
			}
			nint pfsBRPageStart = breakRecord?.BreakRecord ?? IntPtr.Zero;
			PTS.FSFMTR pfsfmtrOut;
			nint ppfsBRPageOut;
			int num = PTS.FsUpdateFinitePage(PtsContext.Context, _ptsPage.Value, pfsBRPageStart, _section.Handle, out pfsfmtrOut, out ppfsBRPageOut);
			if (num != 0)
			{
				DestroyPage();
				PTS.ValidateAndTrace(num, PtsContext);
			}
			if (ppfsBRPageOut != IntPtr.Zero && _section.StructuralCache != null)
			{
				_breakRecord = new PageBreakRecord(PtsContext, new MS.Internal.SecurityCriticalDataForSet<nint>(ppfsBRPageOut), (breakRecord == null) ? 1 : (breakRecord.PageNumber + 1));
			}
			if (TracePageFormatting.IsEnabled)
			{
				TracePageFormatting.Trace(TraceEventType.Stop, TracePageFormatting.FormatPage, PageContext, PtsContext);
			}
			OnAfterFormatPage(setSize: true, incremental: true);
		}
	}

	internal void ArrangePage()
	{
		if (IsEmpty)
		{
			return;
		}
		_section.UpdateSegmentLastFormatPositions();
		PTS.Validate(PTS.FsQueryPageDetails(PtsContext.Context, _ptsPage.Value, out var pPageDetails));
		if (PTS.ToBoolean(pPageDetails.fSimple))
		{
			_section.StructuralCache.CurrentArrangeContext.PushNewPageData(_pageContextOfThisPage, pPageDetails.u.simple.trackdescr.fsrc, _finitePage);
			PtsHelper.ArrangeTrack(PtsContext, ref pPageDetails.u.simple.trackdescr, PTS.FlowDirectionToFswdir(_section.StructuralCache.PageFlowDirection));
			_section.StructuralCache.CurrentArrangeContext.PopPageData();
			return;
		}
		ErrorHandler.Assert(pPageDetails.u.complex.cFootnoteColumns == 0, ErrorHandler.NotSupportedFootnotes);
		if (pPageDetails.u.complex.cSections != 0)
		{
			PtsHelper.SectionListFromPage(PtsContext, _ptsPage.Value, ref pPageDetails, out var arraySectionDesc);
			for (int i = 0; i < arraySectionDesc.Length; i++)
			{
				ArrangeSection(ref arraySectionDesc[i]);
			}
		}
	}

	internal void UpdateViewport(ref PTS.FSRECT viewport)
	{
		if (IsEmpty)
		{
			return;
		}
		PTS.Validate(PTS.FsQueryPageDetails(PtsContext.Context, _ptsPage.Value, out var pPageDetails));
		if (PTS.ToBoolean(pPageDetails.fSimple))
		{
			PtsHelper.UpdateViewportTrack(PtsContext, ref pPageDetails.u.simple.trackdescr, ref viewport);
			return;
		}
		ErrorHandler.Assert(pPageDetails.u.complex.cFootnoteColumns == 0, ErrorHandler.NotSupportedFootnotes);
		if (pPageDetails.u.complex.cSections != 0)
		{
			PtsHelper.SectionListFromPage(PtsContext, _ptsPage.Value, ref pPageDetails, out var arraySectionDesc);
			for (int i = 0; i < arraySectionDesc.Length; i++)
			{
				UpdateViewportSection(ref arraySectionDesc[i], ref viewport);
			}
		}
	}

	internal void ClearUpdateInfo()
	{
		if (!IsEmpty)
		{
			PTS.Validate(PTS.FsClearUpdateInfoInPage(PtsContext.Context, _ptsPage.Value), PtsContext);
		}
	}

	internal ContainerVisual GetPageVisual()
	{
		if (_visual == null)
		{
			_visual = new ContainerVisual();
		}
		if (!IsEmpty)
		{
			UpdatePageVisuals(_calculatedSize);
		}
		else
		{
			_visual.Children.Clear();
		}
		return _visual;
	}

	private void Dispose(bool disposing)
	{
		if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
		{
			if (!IsEmpty)
			{
				_section.PtsContext.OnPageDisposed(_ptsPage, disposing, enterContext: true);
			}
			_ptsPage.Value = IntPtr.Zero;
			_breakRecord = null;
			_visual = null;
			_backgroundFormatOperation = null;
		}
	}

	private void OnBeforeFormatPage(bool finitePage, bool incremental)
	{
		if (!incremental && !IsEmpty)
		{
			DestroyPage();
		}
		_incrementalUpdate = incremental;
		_finitePage = finitePage;
		_breakRecord = null;
		_pageContextOfThisPage.PageRect = new PTS.FSRECT(new Rect(_section.StructuralCache.CurrentFormatContext.PageSize));
		if (_backgroundFormatOperation != null)
		{
			_backgroundFormatOperation.Abort();
		}
		if (!_finitePage)
		{
			_section.StructuralCache.BackgroundFormatInfo.UpdateBackgroundFormatInfo();
		}
	}

	private void OnAfterFormatPage(bool setSize, bool incremental)
	{
		if (setSize)
		{
			PTS.FSRECT rect = GetRect();
			PTS.FSBBOX boundingBox = GetBoundingBox();
			if (!FinitePage && PTS.ToBoolean(boundingBox.fDefined))
			{
				rect.dv = Math.Max(rect.dv, boundingBox.fsrc.dv);
			}
			_calculatedSize.Width = Math.Max(TextDpi.MinWidth, TextDpi.FromTextDpi(rect.du));
			_calculatedSize.Height = Math.Max(TextDpi.MinWidth, TextDpi.FromTextDpi(rect.dv));
			if (PTS.ToBoolean(boundingBox.fDefined))
			{
				_contentSize.Width = Math.Max(Math.Max(TextDpi.FromTextDpi(boundingBox.fsrc.du), TextDpi.MinWidth), _calculatedSize.Width);
				_contentSize.Height = Math.Max(TextDpi.MinWidth, TextDpi.FromTextDpi(boundingBox.fsrc.dv));
				if (!FinitePage)
				{
					_contentSize.Height = Math.Max(_contentSize.Height, _calculatedSize.Height);
				}
			}
			else
			{
				_contentSize = _calculatedSize;
			}
		}
		if (!IsEmpty && !incremental)
		{
			PtsContext.OnPageCreated(_ptsPage);
		}
		if (_section.StructuralCache != null)
		{
			_section.StructuralCache.ClearUpdateInfo(destroyStructureCache: false);
		}
	}

	private PTS.FSRECT GetRect()
	{
		if (IsEmpty)
		{
			return default(PTS.FSRECT);
		}
		PTS.Validate(PTS.FsQueryPageDetails(PtsContext.Context, _ptsPage.Value, out var pPageDetails));
		if (PTS.ToBoolean(pPageDetails.fSimple))
		{
			return pPageDetails.u.simple.trackdescr.fsrc;
		}
		ErrorHandler.Assert(pPageDetails.u.complex.cFootnoteColumns == 0, ErrorHandler.NotSupportedFootnotes);
		return pPageDetails.u.complex.fsrcPageBody;
	}

	private PTS.FSBBOX GetBoundingBox()
	{
		PTS.FSBBOX result = default(PTS.FSBBOX);
		if (!IsEmpty)
		{
			PTS.Validate(PTS.FsQueryPageDetails(PtsContext.Context, _ptsPage.Value, out var pPageDetails));
			if (PTS.ToBoolean(pPageDetails.fSimple))
			{
				return pPageDetails.u.simple.trackdescr.fsbbox;
			}
			ErrorHandler.Assert(pPageDetails.u.complex.cFootnoteColumns == 0, ErrorHandler.NotSupportedFootnotes);
			return pPageDetails.u.complex.fsbboxPageBody;
		}
		return result;
	}

	private void ArrangeSection(ref PTS.FSSECTIONDESCRIPTION sectionDesc)
	{
		PTS.Validate(PTS.FsQuerySectionDetails(PtsContext.Context, sectionDesc.pfssection, out var pSectionDetails));
		if (PTS.ToBoolean(pSectionDetails.fFootnotesAsPagenotes))
		{
			ErrorHandler.Assert(pSectionDetails.u.withpagenotes.cEndnoteColumns == 0, ErrorHandler.NotSupportedFootnotes);
			if (pSectionDetails.u.withpagenotes.cBasicColumns != 0)
			{
				PtsHelper.TrackListFromSection(PtsContext, sectionDesc.pfssection, ref pSectionDetails, out var arrayTrackDesc);
				for (int i = 0; i < arrayTrackDesc.Length; i++)
				{
					_section.StructuralCache.CurrentArrangeContext.PushNewPageData(_pageContextOfThisPage, arrayTrackDesc[i].fsrc, _finitePage);
					PtsHelper.ArrangeTrack(PtsContext, ref arrayTrackDesc[i], pSectionDetails.u.withpagenotes.fswdir);
					_section.StructuralCache.CurrentArrangeContext.PopPageData();
				}
			}
		}
		else
		{
			ErrorHandler.Assert(condition: false, ErrorHandler.NotSupportedCompositeColumns);
		}
	}

	private void UpdateViewportSection(ref PTS.FSSECTIONDESCRIPTION sectionDesc, ref PTS.FSRECT viewport)
	{
		PTS.Validate(PTS.FsQuerySectionDetails(PtsContext.Context, sectionDesc.pfssection, out var pSectionDetails));
		if (PTS.ToBoolean(pSectionDetails.fFootnotesAsPagenotes))
		{
			ErrorHandler.Assert(pSectionDetails.u.withpagenotes.cEndnoteColumns == 0, ErrorHandler.NotSupportedFootnotes);
			if (pSectionDetails.u.withpagenotes.cBasicColumns != 0)
			{
				PtsHelper.TrackListFromSection(PtsContext, sectionDesc.pfssection, ref pSectionDetails, out var arrayTrackDesc);
				for (int i = 0; i < arrayTrackDesc.Length; i++)
				{
					PtsHelper.UpdateViewportTrack(PtsContext, ref arrayTrackDesc[i], ref viewport);
				}
			}
		}
		else
		{
			ErrorHandler.Assert(condition: false, ErrorHandler.NotSupportedCompositeColumns);
		}
	}

	private void UpdatePageVisuals(Size arrangeSize)
	{
		Invariant.Assert(!IsEmpty);
		PTS.Validate(PTS.FsQueryPageDetails(PtsContext.Context, _ptsPage.Value, out var pPageDetails));
		if (pPageDetails.fskupd == PTS.FSKUPDATE.fskupdNoChange)
		{
			return;
		}
		ErrorHandler.Assert(pPageDetails.fskupd != PTS.FSKUPDATE.fskupdShifted, ErrorHandler.UpdateShiftedNotValid);
		if (_visual.Children.Count != 2)
		{
			_visual.Children.Clear();
			_visual.Children.Add(new ContainerVisual());
			_visual.Children.Add(new ContainerVisual());
		}
		ContainerVisual containerVisual = (ContainerVisual)_visual.Children[0];
		ContainerVisual visual = (ContainerVisual)_visual.Children[1];
		if (PTS.ToBoolean(pPageDetails.fSimple))
		{
			PTS.FSKUPDATE fskupd = pPageDetails.u.simple.trackdescr.fsupdinf.fskupd;
			if (fskupd == PTS.FSKUPDATE.fskupdInherited)
			{
				fskupd = pPageDetails.fskupd;
			}
			VisualCollection children = containerVisual.Children;
			if (fskupd == PTS.FSKUPDATE.fskupdNew)
			{
				children.Clear();
				children.Add(new ContainerVisual());
			}
			else if (children.Count == 1 && children[0] is SectionVisual)
			{
				children.Clear();
				children.Add(new ContainerVisual());
			}
			PtsHelper.UpdateTrackVisuals(visualCollection: ((ContainerVisual)children[0]).Children, ptsContext: PtsContext, fskupdInherited: pPageDetails.fskupd, trackDesc: ref pPageDetails.u.simple.trackdescr);
		}
		else
		{
			ErrorHandler.Assert(pPageDetails.u.complex.cFootnoteColumns == 0, ErrorHandler.NotSupportedFootnotes);
			bool flag = pPageDetails.u.complex.cSections == 0;
			if (!flag)
			{
				PtsHelper.SectionListFromPage(PtsContext, _ptsPage.Value, ref pPageDetails, out var arraySectionDesc);
				flag = arraySectionDesc.Length == 0;
				if (!flag)
				{
					ErrorHandler.Assert(arraySectionDesc.Length == 1, ErrorHandler.NotSupportedMultiSection);
					VisualCollection children = containerVisual.Children;
					if (children.Count == 0)
					{
						children.Add(new SectionVisual());
					}
					else if (!(children[0] is SectionVisual))
					{
						children.Clear();
						children.Add(new SectionVisual());
					}
					UpdateSectionVisuals((SectionVisual)children[0], pPageDetails.fskupd, ref arraySectionDesc[0]);
				}
			}
			if (flag)
			{
				containerVisual.Children.Clear();
			}
		}
		PtsHelper.UpdateFloatingElementVisuals(visual, _pageContextOfThisPage.FloatingElementList);
	}

	private void UpdateSectionVisuals(SectionVisual visual, PTS.FSKUPDATE fskupdInherited, ref PTS.FSSECTIONDESCRIPTION sectionDesc)
	{
		PTS.FSKUPDATE fSKUPDATE = sectionDesc.fsupdinf.fskupd;
		if (fSKUPDATE == PTS.FSKUPDATE.fskupdInherited)
		{
			fSKUPDATE = fskupdInherited;
		}
		ErrorHandler.Assert(fSKUPDATE != PTS.FSKUPDATE.fskupdShifted, ErrorHandler.UpdateShiftedNotValid);
		if (fSKUPDATE == PTS.FSKUPDATE.fskupdNoChange)
		{
			return;
		}
		PTS.Validate(PTS.FsQuerySectionDetails(PtsContext.Context, sectionDesc.pfssection, out var pSectionDetails));
		bool flag;
		if (PTS.ToBoolean(pSectionDetails.fFootnotesAsPagenotes))
		{
			ErrorHandler.Assert(pSectionDetails.u.withpagenotes.cEndnoteColumns == 0, ErrorHandler.NotSupportedFootnotes);
			flag = pSectionDetails.u.withpagenotes.cBasicColumns == 0;
			if (!flag)
			{
				PtsHelper.TrackListFromSection(PtsContext, sectionDesc.pfssection, ref pSectionDetails, out var arrayTrackDesc);
				flag = arrayTrackDesc.Length == 0;
				if (!flag)
				{
					ColumnPropertiesGroup columnProperties = new ColumnPropertiesGroup(_section.Element);
					visual.DrawColumnRules(ref arrayTrackDesc, TextDpi.FromTextDpi(sectionDesc.fsrc.v), TextDpi.FromTextDpi(sectionDesc.fsrc.dv), columnProperties);
					VisualCollection children = visual.Children;
					if (fSKUPDATE == PTS.FSKUPDATE.fskupdNew)
					{
						children.Clear();
						for (int i = 0; i < arrayTrackDesc.Length; i++)
						{
							children.Add(new ContainerVisual());
						}
					}
					ErrorHandler.Assert(children.Count == arrayTrackDesc.Length, ErrorHandler.ColumnVisualCountMismatch);
					for (int j = 0; j < arrayTrackDesc.Length; j++)
					{
						ContainerVisual containerVisual = (ContainerVisual)children[j];
						PtsHelper.UpdateTrackVisuals(PtsContext, containerVisual.Children, fSKUPDATE, ref arrayTrackDesc[j]);
					}
				}
			}
		}
		else
		{
			ErrorHandler.Assert(condition: false, ErrorHandler.NotSupportedCompositeColumns);
			flag = true;
		}
		if (flag)
		{
			visual.Children.Clear();
		}
	}

	private IInputElement InputHitTestPage(PTS.FSPOINT pt)
	{
		IInputElement inputElement = null;
		if (_pageContextOfThisPage.FloatingElementList != null)
		{
			for (int i = 0; i < _pageContextOfThisPage.FloatingElementList.Count; i++)
			{
				if (inputElement != null)
				{
					break;
				}
				inputElement = _pageContextOfThisPage.FloatingElementList[i].InputHitTest(pt);
			}
		}
		if (inputElement == null)
		{
			PTS.Validate(PTS.FsQueryPageDetails(PtsContext.Context, _ptsPage.Value, out var pPageDetails));
			if (PTS.ToBoolean(pPageDetails.fSimple))
			{
				if (pPageDetails.u.simple.trackdescr.fsrc.Contains(pt))
				{
					inputElement = PtsHelper.InputHitTestTrack(PtsContext, pt, ref pPageDetails.u.simple.trackdescr);
				}
			}
			else
			{
				ErrorHandler.Assert(pPageDetails.u.complex.cFootnoteColumns == 0, ErrorHandler.NotSupportedFootnotes);
				if (pPageDetails.u.complex.cSections != 0)
				{
					PtsHelper.SectionListFromPage(PtsContext, _ptsPage.Value, ref pPageDetails, out var arraySectionDesc);
					for (int j = 0; j < arraySectionDesc.Length; j++)
					{
						if (inputElement != null)
						{
							break;
						}
						if (arraySectionDesc[j].fsrc.Contains(pt))
						{
							inputElement = InputHitTestSection(pt, ref arraySectionDesc[j]);
						}
					}
				}
			}
		}
		return inputElement;
	}

	private List<Rect> GetRectanglesInPage(ContentElement e, int start, int length)
	{
		List<Rect> list = new List<Rect>();
		Invariant.Assert(!IsEmpty);
		PTS.Validate(PTS.FsQueryPageDetails(PtsContext.Context, _ptsPage.Value, out var pPageDetails));
		if (PTS.ToBoolean(pPageDetails.fSimple))
		{
			list = PtsHelper.GetRectanglesInTrack(PtsContext, e, start, length, ref pPageDetails.u.simple.trackdescr);
		}
		else
		{
			ErrorHandler.Assert(pPageDetails.u.complex.cFootnoteColumns == 0, ErrorHandler.NotSupportedFootnotes);
			if (pPageDetails.u.complex.cSections != 0)
			{
				PtsHelper.SectionListFromPage(PtsContext, _ptsPage.Value, ref pPageDetails, out var arraySectionDesc);
				for (int i = 0; i < arraySectionDesc.Length; i++)
				{
					list = GetRectanglesInSection(e, start, length, ref arraySectionDesc[i]);
					Invariant.Assert(list != null);
					if (list.Count != 0)
					{
						break;
					}
				}
			}
			else
			{
				list = new List<Rect>();
			}
		}
		return list;
	}

	private IInputElement InputHitTestSection(PTS.FSPOINT pt, ref PTS.FSSECTIONDESCRIPTION sectionDesc)
	{
		IInputElement result = null;
		PTS.Validate(PTS.FsQuerySectionDetails(PtsContext.Context, sectionDesc.pfssection, out var pSectionDetails));
		if (PTS.ToBoolean(pSectionDetails.fFootnotesAsPagenotes))
		{
			ErrorHandler.Assert(pSectionDetails.u.withpagenotes.cEndnoteColumns == 0, ErrorHandler.NotSupportedFootnotes);
			if (pSectionDetails.u.withpagenotes.cBasicColumns != 0)
			{
				PtsHelper.TrackListFromSection(PtsContext, sectionDesc.pfssection, ref pSectionDetails, out var arrayTrackDesc);
				for (int i = 0; i < arrayTrackDesc.Length; i++)
				{
					if (arrayTrackDesc[i].fsrc.Contains(pt))
					{
						result = PtsHelper.InputHitTestTrack(PtsContext, pt, ref arrayTrackDesc[i]);
						break;
					}
				}
			}
		}
		else
		{
			ErrorHandler.Assert(condition: false, ErrorHandler.NotSupportedCompositeColumns);
		}
		return result;
	}

	private List<Rect> GetRectanglesInSection(ContentElement e, int start, int length, ref PTS.FSSECTIONDESCRIPTION sectionDesc)
	{
		PTS.Validate(PTS.FsQuerySectionDetails(PtsContext.Context, sectionDesc.pfssection, out var pSectionDetails));
		List<Rect> list = new List<Rect>();
		if (PTS.ToBoolean(pSectionDetails.fFootnotesAsPagenotes))
		{
			ErrorHandler.Assert(pSectionDetails.u.withpagenotes.cEndnoteColumns == 0, ErrorHandler.NotSupportedFootnotes);
			if (pSectionDetails.u.withpagenotes.cBasicColumns != 0)
			{
				PtsHelper.TrackListFromSection(PtsContext, sectionDesc.pfssection, ref pSectionDetails, out var arrayTrackDesc);
				for (int i = 0; i < arrayTrackDesc.Length; i++)
				{
					List<Rect> rectanglesInTrack = PtsHelper.GetRectanglesInTrack(PtsContext, e, start, length, ref arrayTrackDesc[i]);
					Invariant.Assert(rectanglesInTrack != null);
					if (rectanglesInTrack.Count != 0)
					{
						list.AddRange(rectanglesInTrack);
					}
				}
			}
		}
		else
		{
			ErrorHandler.Assert(condition: false, ErrorHandler.NotSupportedCompositeColumns);
		}
		return list;
	}

	private void DestroyPage()
	{
		if (_ptsPage.Value != IntPtr.Zero)
		{
			PtsContext.OnPageDisposed(_ptsPage, disposing: true, enterContext: false);
			_ptsPage.Value = IntPtr.Zero;
		}
	}
}
