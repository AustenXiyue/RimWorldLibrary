using System;
using System.Windows;
using System.Windows.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class FigureParagraph : BaseParagraph
{
	private BaseParagraph _mainTextSegment;

	internal FigureParagraph(DependencyObject element, StructuralCache structuralCache)
		: base(element, structuralCache)
	{
	}

	public override void Dispose()
	{
		base.Dispose();
		if (_mainTextSegment != null)
		{
			_mainTextSegment.Dispose();
			_mainTextSegment = null;
		}
	}

	internal override void GetParaProperties(ref PTS.FSPAP fspap)
	{
		GetParaProperties(ref fspap, ignoreElementProps: false);
		fspap.idobj = -2;
	}

	internal override void CreateParaclient(out nint paraClientHandle)
	{
		FigureParaClient figureParaClient = new FigureParaClient(this);
		paraClientHandle = figureParaClient.Handle;
		if (_mainTextSegment == null)
		{
			_mainTextSegment = new ContainerParagraph(base.Element, base.StructuralCache);
		}
	}

	internal void GetFigureProperties(FigureParaClient paraClient, int fInTextLine, uint fswdir, int fBottomUndefined, out int dur, out int dvr, out PTS.FSFIGUREPROPS fsfigprops, out int cPolygons, out int cVertices, out int durDistTextLeft, out int durDistTextRight, out int dvrDistTextTop, out int dvrDistTextBottom)
	{
		Invariant.Assert(base.StructuralCache.CurrentFormatContext.FinitePage);
		PTS.FlowDirectionToFswdir((FlowDirection)base.Element.GetValue(FrameworkElement.FlowDirectionProperty));
		Figure figure = (Figure)base.Element;
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		durDistTextLeft = (durDistTextRight = (dvrDistTextTop = (dvrDistTextBottom = 0)));
		bool isWidthAuto;
		double width = FigureHelper.CalculateFigureWidth(base.StructuralCache, figure, figure.Width, out isWidthAuto);
		double d = LimitTotalWidthFromAnchor(width, TextDpi.FromTextDpi(mbpInfo.MarginLeft + mbpInfo.MarginRight));
		int num = Math.Max(1, TextDpi.ToTextDpi(d) - (mbpInfo.BPLeft + mbpInfo.BPRight));
		bool isHeightAuto;
		double height = FigureHelper.CalculateFigureHeight(base.StructuralCache, figure, figure.Height, out isHeightAuto);
		double d2 = LimitTotalHeightFromAnchor(height, TextDpi.FromTextDpi(mbpInfo.MarginTop + mbpInfo.MarginBottom));
		int num2 = Math.Max(1, TextDpi.ToTextDpi(d2) - (mbpInfo.BPTop + mbpInfo.BPBottom));
		int num3 = 1;
		PTS.FSCOLUMNINFO[] array = new PTS.FSCOLUMNINFO[num3];
		array[0].durBefore = 0;
		array[0].durWidth = num;
		PTS.FSRECT rcMargin = new PTS.FSRECT(0, 0, num, num2);
		CreateSubpageFiniteHelper(base.PtsContext, IntPtr.Zero, 0, _mainTextSegment.Handle, IntPtr.Zero, 0, 1, fswdir, num, num2, ref rcMargin, num3, array, 0, out var _, out var pSubPage, out var brParaOut, out dvr, out var fsBBox, out var pfsMcsClient, out var topSpace);
		if (brParaOut != IntPtr.Zero)
		{
			PTS.Validate(PTS.FsDestroySubpageBreakRecord(base.PtsContext.Context, brParaOut));
		}
		if (PTS.ToBoolean(fsBBox.fDefined))
		{
			if (fsBBox.fsrc.du < num && isWidthAuto)
			{
				if (pSubPage != IntPtr.Zero)
				{
					PTS.Validate(PTS.FsDestroySubpage(base.PtsContext.Context, pSubPage), base.PtsContext);
				}
				if (pfsMcsClient != IntPtr.Zero)
				{
					MarginCollapsingState obj = base.PtsContext.HandleToObject(pfsMcsClient) as MarginCollapsingState;
					PTS.ValidateHandle(obj);
					obj.Dispose();
					pfsMcsClient = IntPtr.Zero;
				}
				num = fsBBox.fsrc.du + 1;
				array[0].durWidth = num;
				PTS.FSRECT rcMargin2 = new PTS.FSRECT(0, 0, num, num2);
				CreateSubpageFiniteHelper(base.PtsContext, IntPtr.Zero, 0, _mainTextSegment.Handle, IntPtr.Zero, 0, 1, fswdir, num, num2, ref rcMargin2, num3, array, 0, out var _, out pSubPage, out var brParaOut2, out dvr, out fsBBox, out pfsMcsClient, out topSpace);
				if (brParaOut2 != IntPtr.Zero)
				{
					PTS.Validate(PTS.FsDestroySubpageBreakRecord(base.PtsContext.Context, brParaOut2));
				}
			}
		}
		else
		{
			num = TextDpi.ToTextDpi(TextDpi.MinWidth);
		}
		dur = num + mbpInfo.MBPLeft + mbpInfo.MBPRight;
		if (pfsMcsClient != IntPtr.Zero)
		{
			MarginCollapsingState obj2 = base.PtsContext.HandleToObject(pfsMcsClient) as MarginCollapsingState;
			PTS.ValidateHandle(obj2);
			obj2.Dispose();
			pfsMcsClient = IntPtr.Zero;
		}
		dvr += mbpInfo.MBPTop + mbpInfo.MBPBottom;
		if (!isHeightAuto)
		{
			dvr = TextDpi.ToTextDpi(d2) + mbpInfo.MarginTop + mbpInfo.MarginBottom;
		}
		FigureHorizontalAnchor horizontalAnchor = figure.HorizontalAnchor;
		FigureVerticalAnchor verticalAnchor = figure.VerticalAnchor;
		fsfigprops.fskrefU = (PTS.FSKREF)((int)horizontalAnchor / 3);
		fsfigprops.fskrefV = (PTS.FSKREF)((int)verticalAnchor / 3);
		fsfigprops.fskalfU = (PTS.FSKALIGNFIG)((int)horizontalAnchor % 3);
		fsfigprops.fskalfV = (PTS.FSKALIGNFIG)((int)verticalAnchor % 3);
		if (!PTS.ToBoolean(fInTextLine))
		{
			if (fsfigprops.fskrefU == PTS.FSKREF.fskrefChar)
			{
				fsfigprops.fskrefU = PTS.FSKREF.fskrefMargin;
				fsfigprops.fskalfU = PTS.FSKALIGNFIG.fskalfMin;
			}
			if (fsfigprops.fskrefV == PTS.FSKREF.fskrefChar)
			{
				fsfigprops.fskrefV = PTS.FSKREF.fskrefMargin;
				fsfigprops.fskalfV = PTS.FSKALIGNFIG.fskalfMin;
			}
		}
		fsfigprops.fskwrap = PTS.WrapDirectionToFskwrap(figure.WrapDirection);
		fsfigprops.fNonTextPlane = 0;
		fsfigprops.fAllowOverlap = 0;
		fsfigprops.fDelayable = PTS.FromBoolean(figure.CanDelayPlacement);
		cPolygons = (cVertices = 0);
		paraClient.SubpageHandle = pSubPage;
	}

	internal unsafe void GetFigurePolygons(FigureParaClient paraClient, uint fswdir, int ncVertices, int nfspt, int* rgcVertices, out int ccVertices, PTS.FSPOINT* rgfspt, out int cfspt, out int fWrapThrough)
	{
		ccVertices = (cfspt = (fWrapThrough = 0));
	}

	internal void CalcFigurePosition(FigureParaClient paraClient, uint fswdir, ref PTS.FSRECT fsrcPage, ref PTS.FSRECT fsrcMargin, ref PTS.FSRECT fsrcTrack, ref PTS.FSRECT fsrcFigurePreliminary, int fMustPosition, int fInTextLine, out int fPushToNextTrack, out PTS.FSRECT fsrcFlow, out PTS.FSRECT fsrcOverlap, out PTS.FSBBOX fsbbox, out PTS.FSRECT fsrcSearch)
	{
		Figure figure = (Figure)base.Element;
		FigureHorizontalAnchor horizontalAnchor = figure.HorizontalAnchor;
		FigureVerticalAnchor verticalAnchor = figure.VerticalAnchor;
		fsrcSearch = CalculateSearchArea(horizontalAnchor, verticalAnchor, ref fsrcPage, ref fsrcMargin, ref fsrcTrack, ref fsrcFigurePreliminary);
		if (verticalAnchor == FigureVerticalAnchor.ParagraphTop && fsrcFigurePreliminary.v != fsrcMargin.v && fsrcFigurePreliminary.v + fsrcFigurePreliminary.dv > fsrcTrack.v + fsrcTrack.dv && !PTS.ToBoolean(fMustPosition))
		{
			fPushToNextTrack = 1;
		}
		else
		{
			fPushToNextTrack = 0;
		}
		fsrcFlow = fsrcFigurePreliminary;
		if (FigureHelper.IsHorizontalColumnAnchor(horizontalAnchor))
		{
			fsrcFlow.u += CalculateParagraphToColumnOffset(horizontalAnchor, fsrcFigurePreliminary);
		}
		fsrcFlow.u += TextDpi.ToTextDpi(figure.HorizontalOffset);
		fsrcFlow.v += TextDpi.ToTextDpi(figure.VerticalOffset);
		fsrcOverlap = fsrcFlow;
		if (!FigureHelper.IsHorizontalPageAnchor(horizontalAnchor) && horizontalAnchor != FigureHorizontalAnchor.ColumnCenter && horizontalAnchor != FigureHorizontalAnchor.ContentCenter)
		{
			FigureHelper.GetColumnMetrics(base.StructuralCache, out var _, out var width, out var gap, out var _);
			int num = TextDpi.ToTextDpi(width);
			int num2 = TextDpi.ToTextDpi(gap);
			int num3 = num + num2;
			int du = (fsrcOverlap.du / num3 + 1) * num3 - num2;
			fsrcOverlap.du = du;
			if (horizontalAnchor == FigureHorizontalAnchor.ContentRight || horizontalAnchor == FigureHorizontalAnchor.ColumnRight)
			{
				fsrcOverlap.u = fsrcFlow.u + fsrcFlow.du + num2 - fsrcOverlap.du;
			}
			fsrcSearch.u = fsrcOverlap.u;
			fsrcSearch.du = fsrcOverlap.du;
		}
		fsbbox = default(PTS.FSBBOX);
		fsbbox.fDefined = 1;
		fsbbox.fsrc = fsrcFlow;
	}

	internal override void ClearUpdateInfo()
	{
		if (_mainTextSegment != null)
		{
			_mainTextSegment.ClearUpdateInfo();
		}
		base.ClearUpdateInfo();
	}

	internal override bool InvalidateStructure(int startPosition)
	{
		if (_mainTextSegment != null && _mainTextSegment.InvalidateStructure(startPosition))
		{
			_mainTextSegment.Dispose();
			_mainTextSegment = null;
		}
		return _mainTextSegment == null;
	}

	internal override void InvalidateFormatCache()
	{
		if (_mainTextSegment != null)
		{
			_mainTextSegment.InvalidateFormatCache();
		}
	}

	internal void UpdateSegmentLastFormatPositions()
	{
		_mainTextSegment.UpdateLastFormatPositions();
	}

	private unsafe void CreateSubpageFiniteHelper(PtsContext ptsContext, nint brParaIn, int fFromPreviousPage, nint nSeg, nint pFtnRej, int fEmptyOk, int fSuppressTopSpace, uint fswdir, int lWidth, int lHeight, ref PTS.FSRECT rcMargin, int cColumns, PTS.FSCOLUMNINFO[] columnInfoCollection, int fApplyColumnBalancing, out PTS.FSFMTR fsfmtr, out nint pSubPage, out nint brParaOut, out int dvrUsed, out PTS.FSBBOX fsBBox, out nint pfsMcsClient, out int topSpace)
	{
		base.StructuralCache.CurrentFormatContext.PushNewPageData(new Size(TextDpi.FromTextDpi(lWidth), TextDpi.FromTextDpi(lHeight)), default(Thickness), incrementalUpdate: false, finitePage: true);
		fixed (PTS.FSCOLUMNINFO* rgColumnInfo = columnInfoCollection)
		{
			PTS.Validate(PTS.FsCreateSubpageFinite(ptsContext.Context, brParaIn, fFromPreviousPage, nSeg, pFtnRej, fEmptyOk, fSuppressTopSpace, fswdir, lWidth, lHeight, ref rcMargin, cColumns, rgColumnInfo, 0, 0, null, null, 0, null, null, 0, PTS.FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA.fsksuppresshardbreakbeforefirstparaNone, out fsfmtr, out pSubPage, out brParaOut, out dvrUsed, out fsBBox, out pfsMcsClient, out topSpace), ptsContext);
		}
		base.StructuralCache.CurrentFormatContext.PopPageData();
	}

	private int CalculateParagraphToColumnOffset(FigureHorizontalAnchor horizontalAnchor, PTS.FSRECT fsrcInColumn)
	{
		Invariant.Assert(FigureHelper.IsHorizontalColumnAnchor(horizontalAnchor));
		int num = horizontalAnchor switch
		{
			FigureHorizontalAnchor.ColumnLeft => fsrcInColumn.u, 
			FigureHorizontalAnchor.ColumnRight => fsrcInColumn.u + fsrcInColumn.du - 1, 
			_ => fsrcInColumn.u + fsrcInColumn.du / 2 - 1, 
		};
		FigureHelper.GetColumnMetrics(base.StructuralCache, out var cColumns, out var width, out var gap, out var _);
		Invariant.Assert(cColumns > 0);
		int num2 = TextDpi.ToTextDpi(width + gap);
		int num3 = (num - base.StructuralCache.CurrentFormatContext.PageMarginRect.u) / num2;
		int num4 = base.StructuralCache.CurrentFormatContext.PageMarginRect.u + num3 * num2;
		int num5 = TextDpi.ToTextDpi(width);
		int num6 = num4 - fsrcInColumn.u;
		int num7 = num4 + num5 - (fsrcInColumn.u + fsrcInColumn.du);
		return horizontalAnchor switch
		{
			FigureHorizontalAnchor.ColumnLeft => num6, 
			FigureHorizontalAnchor.ColumnRight => num7, 
			_ => (num7 + num6) / 2, 
		};
	}

	private double LimitTotalWidthFromAnchor(double width, double elementMarginWidth)
	{
		FigureHorizontalAnchor horizontalAnchor = ((Figure)base.Element).HorizontalAnchor;
		double num = 0.0;
		if (FigureHelper.IsHorizontalPageAnchor(horizontalAnchor))
		{
			num = base.StructuralCache.CurrentFormatContext.PageWidth;
		}
		else if (FigureHelper.IsHorizontalContentAnchor(horizontalAnchor))
		{
			Thickness pageMargin = base.StructuralCache.CurrentFormatContext.PageMargin;
			num = base.StructuralCache.CurrentFormatContext.PageWidth - pageMargin.Left - pageMargin.Right;
		}
		else
		{
			FigureHelper.GetColumnMetrics(base.StructuralCache, out var _, out var width2, out var _, out var _);
			num = width2;
		}
		if (width + elementMarginWidth > num)
		{
			width = Math.Max(TextDpi.MinWidth, num - elementMarginWidth);
		}
		return width;
	}

	private double LimitTotalHeightFromAnchor(double height, double elementMarginHeight)
	{
		FigureVerticalAnchor verticalAnchor = ((Figure)base.Element).VerticalAnchor;
		double num = 0.0;
		if (FigureHelper.IsVerticalPageAnchor(verticalAnchor))
		{
			num = base.StructuralCache.CurrentFormatContext.PageHeight;
		}
		else
		{
			Thickness pageMargin = base.StructuralCache.CurrentFormatContext.PageMargin;
			num = base.StructuralCache.CurrentFormatContext.PageHeight - pageMargin.Top - pageMargin.Bottom;
		}
		if (height + elementMarginHeight > num)
		{
			height = Math.Max(TextDpi.MinWidth, num - elementMarginHeight);
		}
		return height;
	}

	private PTS.FSRECT CalculateSearchArea(FigureHorizontalAnchor horizAnchor, FigureVerticalAnchor vertAnchor, ref PTS.FSRECT fsrcPage, ref PTS.FSRECT fsrcMargin, ref PTS.FSRECT fsrcTrack, ref PTS.FSRECT fsrcFigurePreliminary)
	{
		PTS.FSRECT result = default(PTS.FSRECT);
		if (FigureHelper.IsHorizontalPageAnchor(horizAnchor))
		{
			result.u = fsrcPage.u;
			result.du = fsrcPage.du;
		}
		else if (FigureHelper.IsHorizontalContentAnchor(horizAnchor))
		{
			result.u = fsrcMargin.u;
			result.du = fsrcMargin.du;
		}
		else
		{
			result.u = fsrcTrack.u;
			result.du = fsrcTrack.du;
		}
		if (FigureHelper.IsVerticalPageAnchor(vertAnchor))
		{
			result.v = fsrcPage.v;
			result.dv = fsrcPage.dv;
		}
		else if (FigureHelper.IsVerticalContentAnchor(vertAnchor))
		{
			result.v = fsrcMargin.v;
			result.dv = fsrcMargin.dv;
		}
		else
		{
			result.v = fsrcFigurePreliminary.v;
			result.dv = fsrcTrack.v + fsrcTrack.dv - fsrcFigurePreliminary.v;
		}
		return result;
	}
}
