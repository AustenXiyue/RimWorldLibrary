using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class FigureParaClient : BaseParaClient
{
	private PTS.FSRECT _contentRect;

	private PTS.FSRECT _paddingRect;

	private PageContext _pageContextOfThisPage = new PageContext();

	internal nint SubpageHandle
	{
		get
		{
			return _paraHandle.Value;
		}
		set
		{
			_paraHandle.Value = value;
		}
	}

	internal PTS.FSRECT ContentRect => _contentRect;

	internal ReadOnlyCollection<ParagraphResult> FloatingElementResults
	{
		get
		{
			List<ParagraphResult> list = new List<ParagraphResult>(0);
			List<BaseParaClient> floatingElementList = _pageContextOfThisPage.FloatingElementList;
			if (floatingElementList != null)
			{
				for (int i = 0; i < floatingElementList.Count; i++)
				{
					ParagraphResult item = floatingElementList[i].CreateParagraphResult();
					list.Add(item);
				}
			}
			return new ReadOnlyCollection<ParagraphResult>(list);
		}
	}

	internal FigureParaClient(FigureParagraph paragraph)
		: base(paragraph)
	{
	}

	public override void Dispose()
	{
		if (SubpageHandle != IntPtr.Zero)
		{
			PTS.Validate(PTS.FsDestroySubpage(base.PtsContext.Context, SubpageHandle), base.PtsContext);
			SubpageHandle = IntPtr.Zero;
		}
		if (_pageContext != null)
		{
			_pageContext.RemoveFloatingParaClient(this);
		}
		base.Dispose();
	}

	protected override void OnArrange()
	{
		base.OnArrange();
		((FigureParagraph)base.Paragraph).UpdateSegmentLastFormatPositions();
		PTS.Validate(PTS.FsQuerySubpageDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubPageDetails));
		_pageContext.AddFloatingParaClient(this);
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Paragraph.Element, base.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (base.ThisFlowDirection != base.PageFlowDirection)
		{
			mbpInfo.MirrorBP();
		}
		_contentRect.u = _rect.u + mbpInfo.BPLeft;
		_contentRect.du = Math.Max(TextDpi.ToTextDpi(TextDpi.MinWidth), _rect.du - mbpInfo.BPRight - mbpInfo.BPLeft);
		_contentRect.v = _rect.v + mbpInfo.BPTop;
		_contentRect.dv = Math.Max(TextDpi.ToTextDpi(TextDpi.MinWidth), _rect.dv - mbpInfo.BPBottom - mbpInfo.BPTop);
		_paddingRect.u = _rect.u + mbpInfo.BorderLeft;
		_paddingRect.du = Math.Max(TextDpi.ToTextDpi(TextDpi.MinWidth), _rect.du - mbpInfo.BorderRight - mbpInfo.BorderLeft);
		_paddingRect.v = _rect.v + mbpInfo.BorderTop;
		_paddingRect.dv = Math.Max(TextDpi.ToTextDpi(TextDpi.MinWidth), _rect.dv - mbpInfo.BorderBottom - mbpInfo.BorderTop);
		if (PTS.ToBoolean(pSubPageDetails.fSimple))
		{
			_pageContextOfThisPage.PageRect = new PTS.FSRECT(pSubPageDetails.u.simple.trackdescr.fsrc);
			base.Paragraph.StructuralCache.CurrentArrangeContext.PushNewPageData(_pageContextOfThisPage, pSubPageDetails.u.simple.trackdescr.fsrc, base.Paragraph.StructuralCache.CurrentArrangeContext.FinitePage);
			PtsHelper.ArrangeTrack(base.PtsContext, ref pSubPageDetails.u.simple.trackdescr, pSubPageDetails.u.simple.fswdir);
			base.Paragraph.StructuralCache.CurrentArrangeContext.PopPageData();
			return;
		}
		_pageContextOfThisPage.PageRect = new PTS.FSRECT(pSubPageDetails.u.complex.fsrc);
		if (pSubPageDetails.u.complex.cBasicColumns != 0)
		{
			PtsHelper.TrackListFromSubpage(base.PtsContext, _paraHandle.Value, ref pSubPageDetails, out var arrayTrackDesc);
			for (int i = 0; i < arrayTrackDesc.Length; i++)
			{
				base.Paragraph.StructuralCache.CurrentArrangeContext.PushNewPageData(_pageContextOfThisPage, arrayTrackDesc[i].fsrc, base.Paragraph.StructuralCache.CurrentArrangeContext.FinitePage);
				PtsHelper.ArrangeTrack(base.PtsContext, ref arrayTrackDesc[i], pSubPageDetails.u.complex.fswdir);
				base.Paragraph.StructuralCache.CurrentArrangeContext.PopPageData();
			}
		}
	}

	internal override void UpdateViewport(ref PTS.FSRECT viewport)
	{
		PTS.Validate(PTS.FsQuerySubpageDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubPageDetails));
		PTS.FSRECT viewport2 = default(PTS.FSRECT);
		viewport2.u = viewport.u - ContentRect.u;
		viewport2.v = viewport.v - ContentRect.v;
		viewport2.du = viewport.du;
		viewport2.dv = viewport.dv;
		if (PTS.ToBoolean(pSubPageDetails.fSimple))
		{
			PtsHelper.UpdateViewportTrack(base.PtsContext, ref pSubPageDetails.u.simple.trackdescr, ref viewport2);
		}
		else
		{
			if (pSubPageDetails.u.complex.cBasicColumns == 0)
			{
				return;
			}
			PtsHelper.TrackListFromSubpage(base.PtsContext, _paraHandle.Value, ref pSubPageDetails, out var arrayTrackDesc);
			if (arrayTrackDesc.Length != 0)
			{
				for (int i = 0; i < arrayTrackDesc.Length; i++)
				{
					PtsHelper.UpdateViewportTrack(base.PtsContext, ref arrayTrackDesc[i], ref viewport2);
				}
			}
		}
	}

	internal void ArrangeFigure(PTS.FSRECT rcFigure, PTS.FSRECT rcHostPara, uint fswdirParent, PageContext pageContext)
	{
		_rect = rcFigure;
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Paragraph.Element, base.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		_rect.v += mbpInfo.MarginTop;
		_rect.dv -= mbpInfo.MarginTop + mbpInfo.MarginBottom;
		_rect.u += mbpInfo.MarginLeft;
		_rect.du -= mbpInfo.MarginLeft + mbpInfo.MarginRight;
		_pageContext = pageContext;
		_flowDirectionParent = PTS.FswdirToFlowDirection(fswdirParent);
		_flowDirection = (FlowDirection)base.Paragraph.Element.GetValue(FrameworkElement.FlowDirectionProperty);
		OnArrange();
	}

	internal override IInputElement InputHitTest(PTS.FSPOINT pt)
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
			PTS.Validate(PTS.FsQuerySubpageDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubPageDetails));
			if (base.Rect.Contains(pt))
			{
				if (ContentRect.Contains(pt))
				{
					pt = new PTS.FSPOINT(pt.u - ContentRect.u, pt.v - ContentRect.v);
					if (PTS.ToBoolean(pSubPageDetails.fSimple))
					{
						inputElement = PtsHelper.InputHitTestTrack(base.PtsContext, pt, ref pSubPageDetails.u.simple.trackdescr);
					}
					else if (pSubPageDetails.u.complex.cBasicColumns != 0)
					{
						PtsHelper.TrackListFromSubpage(base.PtsContext, _paraHandle.Value, ref pSubPageDetails, out var arrayTrackDesc);
						for (int j = 0; j < arrayTrackDesc.Length; j++)
						{
							if (inputElement != null)
							{
								break;
							}
							inputElement = PtsHelper.InputHitTestTrack(base.PtsContext, pt, ref arrayTrackDesc[j]);
						}
					}
				}
				if (inputElement == null)
				{
					inputElement = base.Paragraph.Element as IInputElement;
				}
			}
		}
		return inputElement;
	}

	internal override List<Rect> GetRectangles(ContentElement e, int start, int length)
	{
		List<Rect> rectangles = new List<Rect>();
		if (base.Paragraph.Element as ContentElement == e)
		{
			GetRectanglesForParagraphElement(out rectangles);
		}
		else
		{
			PTS.Validate(PTS.FsQuerySubpageDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubPageDetails));
			if (PTS.ToBoolean(pSubPageDetails.fSimple))
			{
				rectangles = PtsHelper.GetRectanglesInTrack(base.PtsContext, e, start, length, ref pSubPageDetails.u.simple.trackdescr);
			}
			else if (pSubPageDetails.u.complex.cBasicColumns != 0)
			{
				PtsHelper.TrackListFromSubpage(base.PtsContext, _paraHandle.Value, ref pSubPageDetails, out var arrayTrackDesc);
				for (int i = 0; i < arrayTrackDesc.Length; i++)
				{
					List<Rect> rectanglesInTrack = PtsHelper.GetRectanglesInTrack(base.PtsContext, e, start, length, ref arrayTrackDesc[i]);
					Invariant.Assert(rectanglesInTrack != null);
					if (rectanglesInTrack.Count != 0)
					{
						rectangles.AddRange(rectanglesInTrack);
					}
				}
			}
			rectangles = PtsHelper.OffsetRectangleList(rectangles, TextDpi.FromTextDpi(ContentRect.u), TextDpi.FromTextDpi(ContentRect.v));
		}
		Invariant.Assert(rectangles != null);
		return rectangles;
	}

	internal override void ValidateVisual(PTS.FSKUPDATE fskupdInherited)
	{
		fskupdInherited = PTS.FSKUPDATE.fskupdNew;
		PTS.Validate(PTS.FsQuerySubpageDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubPageDetails));
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Paragraph.Element, base.Paragraph.StructuralCache.TextFormatterHost.PixelsPerDip);
		if (base.ThisFlowDirection != base.PageFlowDirection)
		{
			mbpInfo.MirrorBP();
		}
		Brush backgroundBrush = (Brush)base.Paragraph.Element.GetValue(TextElement.BackgroundProperty);
		Visual.DrawBackgroundAndBorder(backgroundBrush, mbpInfo.BorderBrush, mbpInfo.Border, _rect.FromTextDpi(), IsFirstChunk, IsLastChunk);
		if (_visual.Children.Count != 2)
		{
			_visual.Children.Clear();
			_visual.Children.Add(new ContainerVisual());
			_visual.Children.Add(new ContainerVisual());
		}
		ContainerVisual containerVisual = (ContainerVisual)_visual.Children[0];
		ContainerVisual containerVisual2 = (ContainerVisual)_visual.Children[1];
		if (PTS.ToBoolean(pSubPageDetails.fSimple))
		{
			PTS.FSKUPDATE fSKUPDATE = pSubPageDetails.u.simple.trackdescr.fsupdinf.fskupd;
			if (fSKUPDATE == PTS.FSKUPDATE.fskupdInherited)
			{
				fSKUPDATE = fskupdInherited;
			}
			VisualCollection children = containerVisual.Children;
			if (fSKUPDATE == PTS.FSKUPDATE.fskupdNew)
			{
				children.Clear();
				children.Add(new ContainerVisual());
			}
			else if (children.Count == 1 && !(children[0] is ContainerVisual))
			{
				children.Clear();
				children.Add(new ContainerVisual());
			}
			ContainerVisual containerVisual3 = (ContainerVisual)children[0];
			PtsHelper.UpdateTrackVisuals(base.PtsContext, containerVisual3.Children, fskupdInherited, ref pSubPageDetails.u.simple.trackdescr);
		}
		else
		{
			bool flag = pSubPageDetails.u.complex.cBasicColumns == 0;
			if (!flag)
			{
				PtsHelper.TrackListFromSubpage(base.PtsContext, _paraHandle.Value, ref pSubPageDetails, out var arrayTrackDesc);
				flag = arrayTrackDesc.Length == 0;
				if (!flag)
				{
					PTS.FSKUPDATE num = fskupdInherited;
					ErrorHandler.Assert(num != PTS.FSKUPDATE.fskupdShifted, ErrorHandler.UpdateShiftedNotValid);
					VisualCollection children2 = containerVisual.Children;
					if (children2.Count == 0)
					{
						children2.Add(new SectionVisual());
					}
					else if (!(children2[0] is SectionVisual))
					{
						children2.Clear();
						children2.Add(new SectionVisual());
					}
					SectionVisual obj = (SectionVisual)children2[0];
					obj.DrawColumnRules(columnProperties: new ColumnPropertiesGroup(base.Paragraph.Element), arrayColumnDesc: ref arrayTrackDesc, columnVStart: TextDpi.FromTextDpi(pSubPageDetails.u.complex.fsrc.v), columnHeight: TextDpi.FromTextDpi(pSubPageDetails.u.complex.fsrc.dv));
					children2 = obj.Children;
					if (num == PTS.FSKUPDATE.fskupdNew)
					{
						children2.Clear();
						for (int i = 0; i < arrayTrackDesc.Length; i++)
						{
							children2.Add(new ContainerVisual());
						}
					}
					ErrorHandler.Assert(children2.Count == arrayTrackDesc.Length, ErrorHandler.ColumnVisualCountMismatch);
					for (int j = 0; j < arrayTrackDesc.Length; j++)
					{
						ContainerVisual containerVisual4 = (ContainerVisual)children2[j];
						PtsHelper.UpdateTrackVisuals(base.PtsContext, containerVisual4.Children, fskupdInherited, ref arrayTrackDesc[j]);
					}
				}
			}
			if (flag)
			{
				_visual.Children.Clear();
			}
		}
		containerVisual.Offset = new PTS.FSVECTOR(ContentRect.u, ContentRect.v).FromTextDpi();
		containerVisual2.Offset = new PTS.FSVECTOR(ContentRect.u, ContentRect.v).FromTextDpi();
		PTS.FSRECT fSRECT = new PTS.FSRECT(_paddingRect.u - _contentRect.u, _paddingRect.v - _contentRect.v, _paddingRect.du, _paddingRect.dv);
		PtsHelper.ClipChildrenToRect(_visual, fSRECT.FromTextDpi());
		PtsHelper.UpdateFloatingElementVisuals(containerVisual2, _pageContextOfThisPage.FloatingElementList);
	}

	internal override ParagraphResult CreateParagraphResult()
	{
		return new FigureParagraphResult(this);
	}

	internal override TextContentRange GetTextContentRange()
	{
		PTS.Validate(PTS.FsQuerySubpageDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubPageDetails));
		TextContentRange textContentRange;
		if (PTS.ToBoolean(pSubPageDetails.fSimple))
		{
			textContentRange = PtsHelper.TextContentRangeFromTrack(base.PtsContext, pSubPageDetails.u.simple.trackdescr.pfstrack);
		}
		else
		{
			textContentRange = new TextContentRange();
			if (pSubPageDetails.u.complex.cBasicColumns != 0)
			{
				PtsHelper.TrackListFromSubpage(base.PtsContext, _paraHandle.Value, ref pSubPageDetails, out var arrayTrackDesc);
				Invariant.Assert(arrayTrackDesc.Length == 1);
				for (int i = 0; i < arrayTrackDesc.Length; i++)
				{
					textContentRange.Merge(PtsHelper.TextContentRangeFromTrack(base.PtsContext, arrayTrackDesc[i].pfstrack));
				}
			}
		}
		if (IsFirstChunk)
		{
			textContentRange.Merge(TextContainerHelper.GetTextContentRangeForTextElementEdge(base.Paragraph.Element as TextElement, ElementEdge.BeforeStart));
		}
		if (IsLastChunk)
		{
			textContentRange.Merge(TextContainerHelper.GetTextContentRangeForTextElementEdge(base.Paragraph.Element as TextElement, ElementEdge.AfterEnd));
		}
		return textContentRange;
	}

	private ReadOnlyCollection<ParagraphResult> GetChildrenParagraphResults(out bool hasTextContent)
	{
		PTS.Validate(PTS.FsQuerySubpageDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubPageDetails));
		hasTextContent = false;
		List<ParagraphResult> list;
		if (PTS.ToBoolean(pSubPageDetails.fSimple))
		{
			PTS.Validate(PTS.FsQueryTrackDetails(base.PtsContext.Context, pSubPageDetails.u.simple.trackdescr.pfstrack, out var pTrackDetails));
			if (pTrackDetails.cParas == 0)
			{
				return new ReadOnlyCollection<ParagraphResult>(new List<ParagraphResult>(0));
			}
			PtsHelper.ParaListFromTrack(base.PtsContext, pSubPageDetails.u.simple.trackdescr.pfstrack, ref pTrackDetails, out var arrayParaDesc);
			list = new List<ParagraphResult>(arrayParaDesc.Length);
			for (int i = 0; i < arrayParaDesc.Length; i++)
			{
				BaseParaClient obj = base.PtsContext.HandleToObject(arrayParaDesc[i].pfsparaclient) as BaseParaClient;
				PTS.ValidateHandle(obj);
				ParagraphResult paragraphResult = obj.CreateParagraphResult();
				if (paragraphResult.HasTextContent)
				{
					hasTextContent = true;
				}
				list.Add(paragraphResult);
			}
		}
		else
		{
			if (pSubPageDetails.u.complex.cBasicColumns == 0)
			{
				return new ReadOnlyCollection<ParagraphResult>(new List<ParagraphResult>(0));
			}
			PtsHelper.TrackListFromSubpage(base.PtsContext, _paraHandle.Value, ref pSubPageDetails, out var arrayTrackDesc);
			PTS.Validate(PTS.FsQueryTrackDetails(base.PtsContext.Context, arrayTrackDesc[0].pfstrack, out var pTrackDetails2));
			if (pTrackDetails2.cParas == 0)
			{
				return new ReadOnlyCollection<ParagraphResult>(new List<ParagraphResult>(0));
			}
			PtsHelper.ParaListFromTrack(base.PtsContext, arrayTrackDesc[0].pfstrack, ref pTrackDetails2, out var arrayParaDesc2);
			list = new List<ParagraphResult>(arrayParaDesc2.Length);
			for (int j = 0; j < arrayParaDesc2.Length; j++)
			{
				BaseParaClient obj2 = base.PtsContext.HandleToObject(arrayParaDesc2[j].pfsparaclient) as BaseParaClient;
				PTS.ValidateHandle(obj2);
				ParagraphResult paragraphResult2 = obj2.CreateParagraphResult();
				if (paragraphResult2.HasTextContent)
				{
					hasTextContent = true;
				}
				list.Add(paragraphResult2);
			}
		}
		return new ReadOnlyCollection<ParagraphResult>(list);
	}

	internal ReadOnlyCollection<ColumnResult> GetColumnResults(out bool hasTextContent)
	{
		List<ColumnResult> list = new List<ColumnResult>(0);
		Vector contentOffset = default(Vector);
		hasTextContent = false;
		PTS.Validate(PTS.FsQuerySubpageDetails(base.PtsContext.Context, _paraHandle.Value, out var pSubPageDetails));
		if (PTS.ToBoolean(pSubPageDetails.fSimple))
		{
			PTS.Validate(PTS.FsQueryTrackDetails(base.PtsContext.Context, pSubPageDetails.u.simple.trackdescr.pfstrack, out var pTrackDetails));
			if (pTrackDetails.cParas > 0)
			{
				list = new List<ColumnResult>(1);
				ColumnResult columnResult = new ColumnResult(this, ref pSubPageDetails.u.simple.trackdescr, contentOffset);
				list.Add(columnResult);
				if (columnResult.HasTextContent)
				{
					hasTextContent = true;
				}
			}
		}
		else if (pSubPageDetails.u.complex.cBasicColumns != 0)
		{
			PtsHelper.TrackListFromSubpage(base.PtsContext, _paraHandle.Value, ref pSubPageDetails, out var arrayTrackDesc);
			list = new List<ColumnResult>(1);
			PTS.Validate(PTS.FsQueryTrackDetails(base.PtsContext.Context, arrayTrackDesc[0].pfstrack, out var pTrackDetails2));
			if (pTrackDetails2.cParas > 0)
			{
				ColumnResult columnResult2 = new ColumnResult(this, ref arrayTrackDesc[0], contentOffset);
				list.Add(columnResult2);
				if (columnResult2.HasTextContent)
				{
					hasTextContent = true;
				}
			}
		}
		return new ReadOnlyCollection<ColumnResult>(list);
	}

	internal ReadOnlyCollection<ParagraphResult> GetParagraphResultsFromColumn(nint pfstrack, Vector parentOffset, out bool hasTextContent)
	{
		return GetChildrenParagraphResults(out hasTextContent);
	}

	internal TextContentRange GetTextContentRangeFromColumn(nint pfstrack)
	{
		return GetTextContentRange();
	}

	internal Geometry GetTightBoundingGeometryFromTextPositions(ReadOnlyCollection<ColumnResult> columns, ReadOnlyCollection<ParagraphResult> floatingElements, ITextPointer startPosition, ITextPointer endPosition, Rect visibleRect)
	{
		Geometry geometry = null;
		Invariant.Assert(columns != null && columns.Count <= 1, "Columns collection is null.");
		Invariant.Assert(floatingElements != null, "Floating element collection is null.");
		ReadOnlyCollection<ParagraphResult> readOnlyCollection = ((columns.Count > 0) ? columns[0].Paragraphs : new ReadOnlyCollection<ParagraphResult>(new List<ParagraphResult>(0)));
		if (readOnlyCollection.Count > 0 || floatingElements.Count > 0)
		{
			geometry = TextDocumentView.GetTightBoundingGeometryFromTextPositionsHelper(readOnlyCollection, floatingElements, startPosition, endPosition, TextDpi.FromTextDpi(_dvrTopSpace), visibleRect);
			Rect viewport = new Rect(0.0, 0.0, TextDpi.FromTextDpi(_contentRect.du), TextDpi.FromTextDpi(_contentRect.dv));
			CaretElement.ClipGeometryByViewport(ref geometry, viewport);
		}
		return geometry;
	}
}
