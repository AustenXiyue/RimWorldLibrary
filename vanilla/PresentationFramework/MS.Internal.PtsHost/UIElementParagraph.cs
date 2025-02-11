using System;
using System.Windows;
using System.Windows.Documents;
using MS.Internal.Documents;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;

namespace MS.Internal.PtsHost;

internal sealed class UIElementParagraph : FloaterBaseParagraph
{
	private UIElementIsland _uiElementIsland;

	internal UIElementIsland UIElementIsland => _uiElementIsland;

	private bool SizeToFigureParent
	{
		get
		{
			if (!IsOnlyChildOfFigure)
			{
				return false;
			}
			Figure figure = (Figure)((BlockUIContainer)base.Element).Parent;
			if (figure.Height.IsAuto)
			{
				return false;
			}
			if (!base.StructuralCache.CurrentFormatContext.FinitePage && !figure.Height.IsAbsolute)
			{
				return false;
			}
			return true;
		}
	}

	private bool IsOnlyChildOfFigure
	{
		get
		{
			DependencyObject parent = ((BlockUIContainer)base.Element).Parent;
			if (parent is Figure)
			{
				Figure figure = parent as Figure;
				if (figure.Blocks.FirstChild == figure.Blocks.LastChild && figure.Blocks.FirstChild == base.Element)
				{
					return true;
				}
			}
			return false;
		}
	}

	internal UIElementParagraph(TextElement element, StructuralCache structuralCache)
		: base(element, structuralCache)
	{
	}

	public override void Dispose()
	{
		ClearUIElementIsland();
		base.Dispose();
	}

	internal override bool InvalidateStructure(int startPosition)
	{
		if (_uiElementIsland != null)
		{
			_uiElementIsland.DesiredSizeChanged -= OnUIElementDesiredSizeChanged;
			_uiElementIsland.Dispose();
			_uiElementIsland = null;
		}
		return base.InvalidateStructure(startPosition);
	}

	internal override void CreateParaclient(out nint paraClientHandle)
	{
		UIElementParaClient uIElementParaClient = new UIElementParaClient(this);
		paraClientHandle = uIElementParaClient.Handle;
	}

	internal override void CollapseMargin(BaseParaClient paraClient, MarginCollapsingState mcs, uint fswdir, bool suppressTopSpace, out int dvr)
	{
		MbpInfo mbp = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		MarginCollapsingState.CollapseTopMargin(base.PtsContext, mbp, mcs, out var mcsNew, out var margin);
		if (suppressTopSpace)
		{
			dvr = 0;
		}
		else
		{
			dvr = margin;
			if (mcsNew != null)
			{
				dvr += mcsNew.Margin;
			}
		}
		mcsNew?.Dispose();
	}

	internal override void GetFloaterProperties(uint fswdirTrack, out PTS.FSFLOATERPROPS fsfloaterprops)
	{
		fsfloaterprops = default(PTS.FSFLOATERPROPS);
		fsfloaterprops.fFloat = 0;
		fsfloaterprops.fskclear = PTS.WrapDirectionToFskclear((WrapDirection)base.Element.GetValue(Block.ClearFloatersProperty));
		fsfloaterprops.fskfloatalignment = PTS.FSKFLOATALIGNMENT.fskfloatalignMin;
		fsfloaterprops.fskwr = PTS.FSKWRAP.fskwrNone;
		fsfloaterprops.fDelayNoProgress = 1;
	}

	internal override void FormatFloaterContentFinite(FloaterBaseParaClient paraClient, nint pbrkrecIn, int fBRFromPreviousPage, nint footnoteRejector, int fEmptyOk, int fSuppressTopSpace, uint fswdir, int fAtMaxWidth, int durAvailable, int dvrAvailable, PTS.FSKSUPPRESSHARDBREAKBEFOREFIRSTPARA fsksuppresshardbreakbeforefirstparaIn, out PTS.FSFMTR fsfmtr, out nint pfsFloatContent, out nint pbrkrecOut, out int durFloaterWidth, out int dvrFloaterHeight, out PTS.FSBBOX fsbbox, out int cPolygons, out int cVertices)
	{
		Invariant.Assert(paraClient is UIElementParaClient);
		Invariant.Assert(base.Element is BlockUIContainer);
		if (fAtMaxWidth == 0 && fEmptyOk == 1)
		{
			durFloaterWidth = (dvrFloaterHeight = 0);
			cPolygons = (cVertices = 0);
			fsfmtr = default(PTS.FSFMTR);
			fsfmtr.kstop = PTS.FSFMTRKSTOP.fmtrNoProgressOutOfSpace;
			fsfmtr.fContainsItemThatStoppedBeforeFootnote = 0;
			fsfmtr.fForcedProgress = 0;
			fsbbox = default(PTS.FSBBOX);
			fsbbox.fDefined = 0;
			pbrkrecOut = IntPtr.Zero;
			pfsFloatContent = IntPtr.Zero;
			return;
		}
		cPolygons = (cVertices = 0);
		fsfmtr.fForcedProgress = PTS.FromBoolean(fAtMaxWidth == 0);
		if (((BlockUIContainer)base.Element).Child != null)
		{
			EnsureUIElementIsland();
			FormatUIElement(durAvailable, out fsbbox);
		}
		else
		{
			ClearUIElementIsland();
			MbpInfo mbpInfo = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
			fsbbox.fsrc = default(PTS.FSRECT);
			fsbbox.fsrc.du = durAvailable;
			fsbbox.fsrc.dv = mbpInfo.BPTop + mbpInfo.BPBottom;
		}
		durFloaterWidth = fsbbox.fsrc.du;
		dvrFloaterHeight = fsbbox.fsrc.dv;
		if (dvrAvailable < dvrFloaterHeight && fEmptyOk == 1)
		{
			durFloaterWidth = (dvrFloaterHeight = 0);
			fsfmtr = default(PTS.FSFMTR);
			fsfmtr.kstop = PTS.FSFMTRKSTOP.fmtrNoProgressOutOfSpace;
			fsbbox = default(PTS.FSBBOX);
			fsbbox.fDefined = 0;
			pfsFloatContent = IntPtr.Zero;
		}
		else
		{
			fsbbox.fDefined = 1;
			pfsFloatContent = paraClient.Handle;
			if (dvrAvailable < dvrFloaterHeight)
			{
				Invariant.Assert(fEmptyOk == 0);
				fsfmtr.fForcedProgress = 1;
			}
			fsfmtr.kstop = PTS.FSFMTRKSTOP.fmtrGoalReached;
		}
		pbrkrecOut = IntPtr.Zero;
		fsfmtr.fContainsItemThatStoppedBeforeFootnote = 0;
	}

	internal override void FormatFloaterContentBottomless(FloaterBaseParaClient paraClient, int fSuppressTopSpace, uint fswdir, int fAtMaxWidth, int durAvailable, int dvrAvailable, out PTS.FSFMTRBL fsfmtrbl, out nint pfsFloatContent, out int durFloaterWidth, out int dvrFloaterHeight, out PTS.FSBBOX fsbbox, out int cPolygons, out int cVertices)
	{
		Invariant.Assert(paraClient is UIElementParaClient);
		Invariant.Assert(base.Element is BlockUIContainer);
		if (fAtMaxWidth == 0)
		{
			durFloaterWidth = durAvailable + 1;
			dvrFloaterHeight = dvrAvailable + 1;
			cPolygons = (cVertices = 0);
			fsfmtrbl = PTS.FSFMTRBL.fmtrblInterrupted;
			fsbbox = default(PTS.FSBBOX);
			fsbbox.fDefined = 0;
			pfsFloatContent = IntPtr.Zero;
			return;
		}
		cPolygons = (cVertices = 0);
		if (((BlockUIContainer)base.Element).Child != null)
		{
			EnsureUIElementIsland();
			FormatUIElement(durAvailable, out fsbbox);
			pfsFloatContent = paraClient.Handle;
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			fsbbox.fDefined = 1;
			durFloaterWidth = fsbbox.fsrc.du;
			dvrFloaterHeight = fsbbox.fsrc.dv;
		}
		else
		{
			ClearUIElementIsland();
			MbpInfo mbpInfo = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
			fsbbox.fsrc = default(PTS.FSRECT);
			fsbbox.fsrc.du = durAvailable;
			fsbbox.fsrc.dv = mbpInfo.BPTop + mbpInfo.BPBottom;
			fsbbox.fDefined = 1;
			pfsFloatContent = paraClient.Handle;
			fsfmtrbl = PTS.FSFMTRBL.fmtrblGoalReached;
			durFloaterWidth = fsbbox.fsrc.du;
			dvrFloaterHeight = fsbbox.fsrc.dv;
		}
	}

	internal override void UpdateBottomlessFloaterContent(FloaterBaseParaClient paraClient, int fSuppressTopSpace, uint fswdir, int fAtMaxWidth, int durAvailable, int dvrAvailable, nint pfsFloatContent, out PTS.FSFMTRBL fsfmtrbl, out int durFloaterWidth, out int dvrFloaterHeight, out PTS.FSBBOX fsbbox, out int cPolygons, out int cVertices)
	{
		FormatFloaterContentBottomless(paraClient, fSuppressTopSpace, fswdir, fAtMaxWidth, durAvailable, dvrAvailable, out fsfmtrbl, out pfsFloatContent, out durFloaterWidth, out dvrFloaterHeight, out fsbbox, out cPolygons, out cVertices);
	}

	internal override void GetMCSClientAfterFloater(uint fswdirTrack, MarginCollapsingState mcs, out nint pmcsclientOut)
	{
		MbpInfo mbp = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		MarginCollapsingState.CollapseBottomMargin(base.PtsContext, mbp, null, out var mcsNew, out var _);
		if (mcsNew != null)
		{
			pmcsclientOut = mcsNew.Handle;
		}
		else
		{
			pmcsclientOut = IntPtr.Zero;
		}
	}

	private void FormatUIElement(int durAvailable, out PTS.FSBBOX fsbbox)
	{
		MbpInfo mbpInfo = MbpInfo.FromElement(base.Element, base.StructuralCache.TextFormatterHost.PixelsPerDip);
		double width = TextDpi.FromTextDpi(Math.Max(1, durAvailable - (mbpInfo.MBPLeft + mbpInfo.MBPRight)));
		if (SizeToFigureParent)
		{
			double num;
			if (base.StructuralCache.CurrentFormatContext.FinitePage)
			{
				num = base.StructuralCache.CurrentFormatContext.PageHeight;
			}
			else
			{
				Figure obj = (Figure)((BlockUIContainer)base.Element).Parent;
				Invariant.Assert(obj.Height.IsAbsolute);
				num = obj.Height.Value;
			}
			num = Math.Max(TextDpi.FromTextDpi(1), num - TextDpi.FromTextDpi(mbpInfo.MBPTop + mbpInfo.MBPBottom));
			UIElementIsland.DoLayout(new Size(width, num), horizontalAutoSize: false, verticalAutoSize: false);
			fsbbox.fsrc = default(PTS.FSRECT);
			fsbbox.fsrc.du = durAvailable;
			fsbbox.fsrc.dv = TextDpi.ToTextDpi(num) + mbpInfo.BPTop + mbpInfo.BPBottom;
			fsbbox.fDefined = 1;
		}
		else
		{
			double num;
			if (base.StructuralCache.CurrentFormatContext.FinitePage)
			{
				Thickness documentPageMargin = base.StructuralCache.CurrentFormatContext.DocumentPageMargin;
				num = base.StructuralCache.CurrentFormatContext.DocumentPageSize.Height - documentPageMargin.Top - documentPageMargin.Bottom - TextDpi.FromTextDpi(mbpInfo.MBPTop + mbpInfo.MBPBottom);
				num = Math.Max(TextDpi.FromTextDpi(1), num);
			}
			else
			{
				num = double.PositiveInfinity;
			}
			Size size = UIElementIsland.DoLayout(new Size(width, num), horizontalAutoSize: false, verticalAutoSize: true);
			fsbbox.fsrc = default(PTS.FSRECT);
			fsbbox.fsrc.du = durAvailable;
			fsbbox.fsrc.dv = TextDpi.ToTextDpi(size.Height) + mbpInfo.BPTop + mbpInfo.BPBottom;
			fsbbox.fDefined = 1;
		}
	}

	private void EnsureUIElementIsland()
	{
		if (_uiElementIsland == null)
		{
			_uiElementIsland = new UIElementIsland(((BlockUIContainer)base.Element).Child);
			_uiElementIsland.DesiredSizeChanged += OnUIElementDesiredSizeChanged;
		}
	}

	private void ClearUIElementIsland()
	{
		try
		{
			if (_uiElementIsland != null)
			{
				_uiElementIsland.DesiredSizeChanged -= OnUIElementDesiredSizeChanged;
				_uiElementIsland.Dispose();
			}
		}
		finally
		{
			_uiElementIsland = null;
		}
	}

	private void OnUIElementDesiredSizeChanged(object sender, DesiredSizeChangedEventArgs e)
	{
		base.StructuralCache.FormattingOwner.OnChildDesiredSizeChanged(e.Child);
	}
}
